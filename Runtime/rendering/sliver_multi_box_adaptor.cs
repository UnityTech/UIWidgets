using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public interface RenderSliverBoxChildManager {
        void createChild(int index, RenderBox after = null);

        void removeChild(RenderBox child);

        float estimateMaxScrollOffset(
            SliverConstraints constraints,
            int firstIndex = 0,
            int lastIndex = 0,
            float leadingScrollOffset = 0,
            float trailingScrollOffset = 0);

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

        public bool keptAlive {
            get { return this._keptAlive; }
        }

        internal bool _keptAlive = false;
        
        public override string ToString() {
            return $"index={this.index}; {(this.keepAlive ? "keeyAlive; " : "")}{base.ToString()}";
        }
    }

    public abstract class RenderSliverMultiBoxAdaptor
        : ContainerRenderObjectMixinRenderSliver<RenderBox, SliverMultiBoxAdaptorParentData> {
        public RenderSliverMultiBoxAdaptor(
            RenderSliverBoxChildManager childManager = null
        ) {
            D.assert(childManager != null);
            this._childManager = childManager;
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverMultiBoxAdaptorParentData)) {
                child.parentData = new SliverMultiBoxAdaptorParentData();
            }
        }

        protected RenderSliverBoxChildManager childManager {
            get { return this._childManager; }
        }

        readonly RenderSliverBoxChildManager _childManager;

        readonly Dictionary<int, RenderBox> _keepAliveBucket = new Dictionary<int, RenderBox>();

        protected override void adoptChild(AbstractNodeMixinDiagnosticableTree childNode) {
            base.adoptChild(childNode);
            var child = (RenderBox) childNode;
            var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            if (!childParentData._keptAlive) {
                this.childManager.didAdoptChild(child);
            }
        }

        bool _debugAssertChildListLocked() {
            return this.childManager.debugAssertChildListLocked();
        }

        public override void insert(RenderBox child, RenderBox after = null) {
            D.assert(!this._keepAliveBucket.ContainsValue(value: child));
            base.insert(child, after: after);
            D.assert(this.firstChild != null);
            D.assert(() => {
                int index = this.indexOf(this.firstChild);
                RenderBox childAfter = this.childAfter(this.firstChild);
                while (childAfter != null) {
                    D.assert(this.indexOf(childAfter) > index);
                    index = this.indexOf(childAfter);
                    childAfter = this.childAfter(childAfter);
                }

                return true;
            });
        }

        public override void remove(RenderBox child) {
            var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            if (!childParentData._keptAlive) {
                base.remove(child);
                return;
            }

            D.assert(this._keepAliveBucket[childParentData.index] == child);
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
            this.invokeLayoutCallback<SliverConstraints>(constraints => {
                D.assert(constraints == this.constraints);
                if (this._keepAliveBucket.ContainsKey(index)) {
                    RenderBox child = this._keepAliveBucket[index];
                    this._keepAliveBucket.Remove(index);
                    var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                    D.assert(childParentData._keptAlive);
                    this.dropChild(child);
                    child.parentData = childParentData;
                    this.insert(child, after: after);
                    childParentData._keptAlive = false;
                }
                else {
                    this._childManager.createChild(index, after: after);
                }
            });
        }

        public void _destroyOrCacheChild(RenderBox child) {
            var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            if (childParentData.keepAlive) {
                D.assert(!childParentData._keptAlive);
                this.remove(child);
                this._keepAliveBucket[childParentData.index] = child;
                child.parentData = childParentData;
                base.adoptChild(child);
                childParentData._keptAlive = true;
            }
            else {
                D.assert(child.parent == this);
                this._childManager.removeChild(child);
                D.assert(child.parent == null);
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

        protected bool addInitialChild(int index = 0, float layoutOffset = 0.0f) {
            D.assert(this._debugAssertChildListLocked());
            D.assert(this.firstChild == null);

            this._createOrObtainChild(index, after: null);
            if (this.firstChild != null) {
                D.assert(this.firstChild == this.lastChild);
                D.assert(this.indexOf(this.firstChild) == index);

                var firstChildParentData = (SliverMultiBoxAdaptorParentData) this.firstChild.parentData;
                firstChildParentData.layoutOffset = layoutOffset;
                return true;
            }

            this.childManager.setDidUnderflow(true);
            return false;
        }

        protected RenderBox insertAndLayoutLeadingChild(BoxConstraints childConstraints, bool parentUsesSize = false) {
            D.assert(this._debugAssertChildListLocked());

            int index = this.indexOf(this.firstChild) - 1;
            this._createOrObtainChild(index, after: null);
            if (this.indexOf(this.firstChild) == index) {
                this.firstChild.layout(childConstraints, parentUsesSize: parentUsesSize);
                return this.firstChild;
            }

            this.childManager.setDidUnderflow(true);
            return null;
        }

        protected RenderBox insertAndLayoutChild(
            BoxConstraints childConstraints,
            RenderBox after = null,
            bool parentUsesSize = false
        ) {
            D.assert(this._debugAssertChildListLocked());
            D.assert(after != null);

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

        protected void collectGarbage(int leadingGarbage, int trailingGarbage) {
            D.assert(this._debugAssertChildListLocked());
            D.assert(this.childCount >= leadingGarbage + trailingGarbage);

            this.invokeLayoutCallback<SliverConstraints>(constraints => {
                while (leadingGarbage > 0) {
                    this._destroyOrCacheChild(this.firstChild);
                    leadingGarbage -= 1;
                }

                while (trailingGarbage > 0) {
                    this._destroyOrCacheChild(this.lastChild);
                    trailingGarbage -= 1;
                }

                this._keepAliveBucket.Values.Where(child => {
                    var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                    return !childParentData.keepAlive;
                }).ToList().ForEach(this._childManager.removeChild);

                D.assert(this._keepAliveBucket.Values.Where(child => {
                    var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                    return !childParentData.keepAlive;
                }).ToList().isEmpty());
            });
        }

        public int indexOf(RenderBox child) {
            D.assert(child != null);
            var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            return childParentData.index;
        }

        protected float paintExtentOf(RenderBox child) {
            D.assert(child != null);
            D.assert(child.hasSize);

            switch (this.constraints.axis) {
                case Axis.horizontal:
                    return child.size.width;
                case Axis.vertical:
                    return child.size.height;
            }

            return 0.0f;
        }

        protected override bool hitTestChildren(HitTestResult result, float mainAxisPosition = 0.0f,
            float crossAxisPosition = 0.0f) {
            RenderBox child = this.lastChild;
            while (child != null) {
                if (this.hitTestBoxChild(result, child, mainAxisPosition: mainAxisPosition,
                    crossAxisPosition: crossAxisPosition)) {
                    return true;
                }

                child = this.childBefore(child);
            }

            return false;
        }

        public override float childMainAxisPosition(RenderObject child) {
            return this.childScrollOffset(child) - this.constraints.scrollOffset;
        }

        public override float childScrollOffset(RenderObject child) {
            D.assert(child != null);
            D.assert(child.parent == this);

            var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            return childParentData.layoutOffset;
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            this.applyPaintTransformForBoxChild((RenderBox) child, transform);
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
                    mainAxisUnit = new Offset(0.0f, -1.0f);
                    crossAxisUnit = new Offset(1.0f, 0.0f);
                    originOffset = offset + new Offset(0.0f, this.geometry.paintExtent);
                    addExtent = true;
                    break;
                case AxisDirection.right:
                    mainAxisUnit = new Offset(1.0f, 0.0f);
                    crossAxisUnit = new Offset(0.0f, 1.0f);
                    originOffset = offset;
                    addExtent = false;
                    break;
                case AxisDirection.down:
                    mainAxisUnit = new Offset(0.0f, 1.0f);
                    crossAxisUnit = new Offset(1.0f, 0.0f);
                    originOffset = offset;
                    addExtent = false;
                    break;
                case AxisDirection.left:
                    mainAxisUnit = new Offset(-1.0f, 0.0f);
                    crossAxisUnit = new Offset(0.0f, 1.0f);
                    originOffset = offset + new Offset(this.geometry.paintExtent, 0.0f);
                    addExtent = true;
                    break;
            }

            RenderBox child = this.firstChild;
            while (child != null) {
                float mainAxisDelta = this.childMainAxisPosition(child);
                float crossAxisDelta = this.childCrossAxisPosition(child);
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

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(DiagnosticsNode.message(this.firstChild != null
                ? "currently live children: " + this.indexOf(this.firstChild) + " to " + this.indexOf(this.lastChild)
                : "no children current live"));
        }

        public bool debugAssertChildListIsNonEmptyAndContiguous() {
            D.assert(() => {
                D.assert(this.firstChild != null);
                int index = this.indexOf(this.firstChild);
                RenderBox child = this.childAfter(this.firstChild);
                while (child != null) {
                    index += 1;
                    D.assert(this.indexOf(child) == index);
                    child = this.childAfter(child);
                }

                return true;
            });
            return true;
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();
            if (this.firstChild != null) {
                RenderBox child = this.firstChild;
                while (true) {
                    var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                    children.Add(child.toDiagnosticsNode(name: "child with index " + childParentData.index));
                    if (child == this.lastChild) {
                        break;
                    }

                    child = childParentData.nextSibling;
                }
            }

            if (this._keepAliveBucket.isNotEmpty()) {
                List<int> indices = this._keepAliveBucket.Keys.ToList();
                indices.Sort();

                foreach (int index in indices) {
                    children.Add(this._keepAliveBucket[index].toDiagnosticsNode(
                        name: "child with index " + index + " (kept alive but not laid out)",
                        style: DiagnosticsTreeStyle.offstage
                    ));
                }
            }

            return children;
        }
    }
}