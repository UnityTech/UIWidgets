using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public class MultiChildLayoutParentData : ContainerParentDataMixinBoxParentData<RenderBox> {
        public object id;

        public override string ToString() {
            return $"{base.ToString()}; id={this.id}";
        }
    }

    public abstract class MultiChildLayoutDelegate {
        Dictionary<object, RenderBox> _idToChild;
        HashSet<RenderBox> _debugChildrenNeedingLayout;

        public bool hasChild(object childId) {
            return this._idToChild.getOrDefault(childId) != null;
        }

        public Size layoutChild(object childId, BoxConstraints constraints) {
            RenderBox child = this._idToChild[childId];
            D.assert(() => {
                if (child == null) {
                    throw new UIWidgetsError(
                        $"The {this} custom multichild layout delegate tried to lay out a non-existent child.\n" +
                        $"There is no child with the id \"{childId}\"."
                    );
                }

                if (!this._debugChildrenNeedingLayout.Remove(child)) {
                    throw new UIWidgetsError(
                        $"The $this custom multichild layout delegate tried to lay out the child with id \"{childId}\" more than once.\n" +
                        "Each child must be laid out exactly once."
                    );
                }

                try {
                    D.assert(constraints.debugAssertIsValid(isAppliedConstraint: true));
                }
                catch (AssertionError exception) {
                    throw new UIWidgetsError(
                        $"The {this} custom multichild layout delegate provided invalid box constraints for the child with id \"{childId}\".\n" +
                        $"{exception}n" +
                        "The minimum width and height must be greater than or equal to zero.\n" +
                        "The maximum width must be greater than or equal to the minimum width.\n" +
                        "The maximum height must be greater than or equal to the minimum height.");
                }

                return true;
            });
            child.layout(constraints, parentUsesSize: true);
            return child.size;
        }

        public void positionChild(object childId, Offset offset) {
            RenderBox child = this._idToChild[childId];
            D.assert(() => {
                if (child == null) {
                    throw new UIWidgetsError(
                        $"The {this} custom multichild layout delegate tried to position out a non-existent child:\n" +
                        $"There is no child with the id \"{childId}\"."
                    );
                }

                if (offset == null) {
                    throw new UIWidgetsError(
                        $"The {this} custom multichild layout delegate provided a null position for the child with id \"{childId}\"."
                    );
                }

                return true;
            });
            MultiChildLayoutParentData childParentData = (MultiChildLayoutParentData) child.parentData;
            childParentData.offset = offset;
        }

        string _debugDescribeChild(RenderBox child) {
            MultiChildLayoutParentData childParentData = (MultiChildLayoutParentData) child.parentData;
            return $"{childParentData.id}: {child}";
        }


        internal void _callPerformLayout(Size size, RenderBox firstChild) {
            Dictionary<object, RenderBox> previousIdToChild = this._idToChild;

            HashSet<RenderBox> debugPreviousChildrenNeedingLayout = null;
            D.assert(() => {
                debugPreviousChildrenNeedingLayout = this._debugChildrenNeedingLayout;
                this._debugChildrenNeedingLayout = new HashSet<RenderBox>();
                return true;
            });

            try {
                this._idToChild = new Dictionary<object, RenderBox>();
                RenderBox child = firstChild;
                while (child != null) {
                    MultiChildLayoutParentData childParentData = (MultiChildLayoutParentData) child.parentData;
                    D.assert(() => {
                        if (childParentData.id == null) {
                            throw new UIWidgetsError(
                                "The following child has no ID:\n" +
                                $"  {child}\n" +
                                "Every child of a RenderCustomMultiChildLayoutBox must have an ID in its parent data."
                            );
                        }

                        return true;
                    });
                    this._idToChild[childParentData.id] = child;
                    D.assert(() => {
                        this._debugChildrenNeedingLayout.Add(child);
                        return true;
                    });
                    child = childParentData.nextSibling;
                }

                this.performLayout(size);
                D.assert(() => {
                    if (this._debugChildrenNeedingLayout.isNotEmpty()) {
                        if (this._debugChildrenNeedingLayout.Count > 1) {
                            throw new UIWidgetsError(
                                $"The $this custom multichild layout delegate forgot to lay out the following children:\n" +
                                $"  {string.Join("\n  ", this._debugChildrenNeedingLayout.Select(this._debugDescribeChild))}\n" +
                                "Each child must be laid out exactly once."
                            );
                        }
                        else {
                            throw new UIWidgetsError(
                                $"The $this custom multichild layout delegate forgot to lay out the following child:\n" +
                                $"  {this._debugDescribeChild(this._debugChildrenNeedingLayout.First())}\n" +
                                "Each child must be laid out exactly once."
                            );
                        }
                    }

                    return true;
                });
            }
            finally {
                this._idToChild = previousIdToChild;
                D.assert(() => {
                    this._debugChildrenNeedingLayout = debugPreviousChildrenNeedingLayout;
                    return true;
                });
            }
        }

        public virtual Size getSize(BoxConstraints constraints) {
            return constraints.biggest;
        }


        public abstract void performLayout(Size size);


        public abstract bool shouldRelayout(MultiChildLayoutDelegate oldDelegate);

        public override string ToString() {
            return $"{this.GetType()}";
        }
    }

    public class RenderCustomMultiChildLayoutBox : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<
        RenderBox
        , MultiChildLayoutParentData> {
        public RenderCustomMultiChildLayoutBox(
            List<RenderBox> children = null,
            MultiChildLayoutDelegate layoutDelegate = null
        ) {
            D.assert(layoutDelegate != null);
            this._delegate = layoutDelegate;
            this.addAll(children);
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is MultiChildLayoutParentData)) {
                child.parentData = new MultiChildLayoutParentData();
            }
        }

        public MultiChildLayoutDelegate layoutDelegate {
            get { return this._delegate; }
            set {
                D.assert(value != null);
                if (this._delegate == value) {
                    return;
                }

                if (value.GetType() != this._delegate.GetType() || value.shouldRelayout(this._delegate)) {
                    this.markNeedsLayout();
                }

                this._delegate = value;
            }
        }

        MultiChildLayoutDelegate _delegate;


        Size _getSize(BoxConstraints constraints) {
            D.assert(constraints.debugAssertIsValid());
            return constraints.constrain(this._delegate.getSize(constraints));
        }

        protected override float computeMinIntrinsicWidth(float height) {
            float width = this._getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite()) {
                return width;
            }

            return 0.0f;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            float width = this._getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite()) {
                return width;
            }

            return 0.0f;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            float height = this._getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float height = this._getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected override void performLayout() {
            this.size = this._getSize(this.constraints);
            this.layoutDelegate._callPerformLayout(this.size, this.firstChild);
        }

        public override void paint(PaintingContext context, Offset offset) {
            this.defaultPaint(context, offset);
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position) {
            return this.defaultHitTestChildren(result, position: position);
        }
    }
}