using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.rendering {
    public interface RenderSliverBoxChildManager {
        void createChild(int index, RenderBox after = null);

        void removeChild(RenderBox child);

        double? estimateMaxScrollOffset(
            SliverConstraints constraints,
            int firstIndex = 0,
            int lastIndex = 0,
            double leadingScrollOffset = 0,
            double trailingScrollOffset = 0);

        int? childCount { get; }

        void didAdoptChild(RenderBox child);

        void setDidUnderflow(bool value);

        void didStartLayout();

        void didFinishLayout();
        
        bool debugAssertChildListLocked();
    }

    public class SliverMultiBoxAdaptorParentData : ContainerParentDataMixinSliverLogicalParentData<RenderBox> {
        public int index;

        public bool keepAlive = false;

        public bool _keptAlive = false;
    }

    public abstract class RenderSliverMultiBoxAdaptor
        : ContainerRenderObjectMixinRenderSliver<RenderBox, SliverMultiBoxAdaptorParentData> {
        public RenderSliverMultiBoxAdaptor(
            RenderSliverBoxChildManager childManager = null
        ) {
            this._childManager = childManager;
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverMultiBoxAdaptorParentData)) {
                child.parentData = new SliverMultiBoxAdaptorParentData();
            }
        }

        public RenderSliverBoxChildManager childManager {
            get { return this._childManager; }
        }

        public RenderSliverBoxChildManager _childManager;

        public readonly Dictionary<int, RenderBox> _keepAliveBucket = new Dictionary<int, RenderBox>();

        protected override void adoptChild(AbstractNodeMixinDiagnosticableTree childNode) {
            base.adoptChild(childNode);
            var child = (RenderBox) childNode;
            var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            if (!childParentData._keptAlive) {
                this.childManager.didAdoptChild(child);
            }
        }

        public override void insert(RenderBox child, RenderBox after = null) {
            base.insert(child, after: after);
        }

        public override void remove(RenderBox child) {
            var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            if (!childParentData._keptAlive) {
                base.remove(child);
                return;
            }

            this._keepAliveBucket.Remove(childParentData.index);
            this.dropChild(child);
        }

        public override void removeAll() {
            base.removeAll();

            foreach (var child in this._keepAliveBucket.Values) {
                this.dropChild(child);
            }

            this._keepAliveBucket.Clear();
        }

        void _createOrObtainChild(int index, RenderBox after = null) {
            this.invokeLayoutCallback<SliverConstraints>((SliverConstraints constraints) => {
                if (this._keepAliveBucket.ContainsKey(index)) {
                    RenderBox child = this._keepAliveBucket[index];
                    this._keepAliveBucket.Remove(index);
                    var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                    this.dropChild(child);
                    child.parentData = childParentData;
                    this.insert(child, after: after);
                    childParentData._keptAlive = false;
                } else {
                    this._childManager.createChild(index, after: after);
                }
            });
        }

        public void _destroyOrCacheChild(RenderBox child) {
            var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            if (childParentData.keepAlive) {
                this.remove(child);
                this._keepAliveBucket[childParentData.index] = child;
                child.parentData = childParentData;
                base.adoptChild(child);
                childParentData._keptAlive = true;
            } else {
                this._childManager.removeChild(child);
            }
        }

        public override void attach(object owner) {
            base.attach(owner);
            foreach (RenderBox child in this._keepAliveBucket.Values) {
                child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            foreach (RenderBox child in this._keepAliveBucket.Values) {
                child.detach();
            }
        }

        public override void redepthChildren() {
            base.redepthChildren();
            foreach (var child in this._keepAliveBucket.Values) {
                this.redepthChild(child);
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            base.visitChildren(visitor);

            foreach (var child in this._keepAliveBucket.Values) {
                visitor(child);
            }
        }

        public bool addInitialChild(int index = 0, double layoutOffset = 0.0) {
            this._createOrObtainChild(index, after: null);
            if (this.firstChild != null) {
                var firstChildParentData = (SliverMultiBoxAdaptorParentData) this.firstChild.parentData;
                firstChildParentData.layoutOffset = layoutOffset;
                return true;
            }

            this.childManager.setDidUnderflow(true);
            return false;
        }

        public RenderBox insertAndLayoutLeadingChild(BoxConstraints childConstraints, bool parentUsesSize = false) {
            int index = this.indexOf(this.firstChild) - 1;
            this._createOrObtainChild(index, after: null);
            if (this.indexOf(this.firstChild) == index) {
                this.firstChild.layout(childConstraints, parentUsesSize: parentUsesSize);
                return this.firstChild;
            }

            this.childManager.setDidUnderflow(true);
            return null;
        }

        public RenderBox insertAndLayoutChild(
            BoxConstraints childConstraints,
            RenderBox after = null,
            bool parentUsesSize = false
        ) {
            int index = this.indexOf(after) + 1;
            this._createOrObtainChild(index, after: after);
            RenderBox child = this.childAfter(after);
            if (child != null && this.indexOf(child) == index) {
                child.layout(childConstraints, parentUsesSize: parentUsesSize);
                return child;
            }

            this.childManager.setDidUnderflow(true);
            return null;
        }

        public void collectGarbage(int leadingGarbage, int trailingGarbage) {
            this.invokeLayoutCallback<SliverConstraints>((SliverConstraints constraints) => {
                while (leadingGarbage > 0) {
                    this._destroyOrCacheChild(this.firstChild);
                    leadingGarbage -= 1;
                }

                while (trailingGarbage > 0) {
                    this._destroyOrCacheChild(this.lastChild);
                    trailingGarbage -= 1;
                }

                this._keepAliveBucket.Values.Where((RenderBox child) => {
                    var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                    return !childParentData.keepAlive;
                }).ToList().ForEach(this._childManager.removeChild);
            });
        }

        public int indexOf(RenderBox child) {
            var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            return childParentData.index;
        }

        public double paintExtentOf(RenderBox child) {
            switch (this.constraints.axis) {
                case Axis.horizontal:
                    return child.size.width;
                case Axis.vertical:
                    return child.size.height;
            }

            return 0.0;
        }

        public override double childMainAxisPosition(RenderObject child) {
            return this.childScrollOffset(child) - this.constraints.scrollOffset;
        }

        public override double childScrollOffset(RenderObject child) {
            var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            return childParentData.layoutOffset;
        }

        public override void applyPaintTransform(RenderObject child, ref Matrix4x4 transform) {
            this.applyPaintTransformForBoxChild((RenderBox) child, ref transform);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.firstChild == null) {
                return;
            }

            Offset mainAxisUnit = null, crossAxisUnit = null, originOffset = null;
            bool addExtent = false;
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(this.constraints.axisDirection,
                this.constraints.growthDirection)) {
                case AxisDirection.up:
                    mainAxisUnit = new Offset(0.0, -1.0);
                    crossAxisUnit = new Offset(1.0, 0.0);
                    originOffset = offset + new Offset(0.0, this.geometry.paintExtent);
                    addExtent = true;
                    break;
                case AxisDirection.right:
                    mainAxisUnit = new Offset(1.0, 0.0);
                    crossAxisUnit = new Offset(0.0, 1.0);
                    originOffset = offset;
                    addExtent = false;
                    break;
                case AxisDirection.down:
                    mainAxisUnit = new Offset(0.0, 1.0);
                    crossAxisUnit = new Offset(1.0, 0.0);
                    originOffset = offset;
                    addExtent = false;
                    break;
                case AxisDirection.left:
                    mainAxisUnit = new Offset(-1.0, 0.0);
                    crossAxisUnit = new Offset(0.0, 1.0);
                    originOffset = offset + new Offset(this.geometry.paintExtent, 0.0);
                    addExtent = true;
                    break;
            }

            RenderBox child = this.firstChild;
            while (child != null) {
                double mainAxisDelta = this.childMainAxisPosition(child);
                double crossAxisDelta = this.childCrossAxisPosition(child);
                Offset childOffset = new Offset(
                    originOffset.dx + mainAxisUnit.dx * mainAxisDelta + crossAxisUnit.dx * crossAxisDelta,
                    originOffset.dy + mainAxisUnit.dy * mainAxisDelta + crossAxisUnit.dy * crossAxisDelta
                );
                if (addExtent) {
                    childOffset += mainAxisUnit * this.paintExtentOf(child);
                }

                if (mainAxisDelta < this.constraints.remainingPaintExtent &&
                    mainAxisDelta + this.paintExtentOf(child) > 0) {
                    context.paintChild(child, childOffset);
                }

                child = this.childAfter(child);
            }
        }
    }
}