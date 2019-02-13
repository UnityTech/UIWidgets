using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public abstract class SliverChildDelegate {
        public virtual int? estimatedChildCount {
            get { return null; }
        }

        public abstract Widget build(BuildContext context, int index);

        public virtual double? estimateMaxScrollOffset(
            int firstIndex,
            int lastIndex,
            double leadingScrollOffset,
            double trailingScrollOffset
        ) {
            return null;
        }

        public virtual void didFinishLayout(int firstIndex, int lastIndex) {
        }

        public abstract bool shouldRebuild(SliverChildDelegate oldDelegate);

        public override string ToString() {
            var description = new List<string>();
            this.debugFillDescription(description: description);
            return $"{Diagnostics.describeIdentity(this)}({string.Join(", ", description.ToArray())})";
        }

        protected virtual void debugFillDescription(List<string> description) {
            try {
                var children = this.estimatedChildCount;
                if (children != null) {
                    description.Add("estimated child count: " + children);
                }
            }
            catch (Exception ex) {
                description.Add("estimated child count: EXCEPTION (" + ex.GetType() + ")");
            }
        }
    }

    public class SliverChildBuilderDelegate : SliverChildDelegate {
        public readonly bool addAutomaticKeepAlives;

        public readonly bool addRepaintBoundaries;

        public readonly IndexedWidgetBuilder builder;

        public readonly int? childCount;

        public SliverChildBuilderDelegate(
            IndexedWidgetBuilder builder,
            int? childCount = null,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true
        ) {
            D.assert(builder != null);
            this.builder = builder;
            this.childCount = childCount;
            this.addAutomaticKeepAlives = addAutomaticKeepAlives;
            this.addRepaintBoundaries = addRepaintBoundaries;
        }

        public override int? estimatedChildCount {
            get { return this.childCount; }
        }

        public override Widget build(BuildContext context, int index) {
            D.assert(this.builder != null);
            if (index < 0 || this.childCount != null && index >= this.childCount) {
                return null;
            }

            Widget child = this.builder(context: context, index: index);
            if (child == null) {
                return null;
            }

            if (this.addRepaintBoundaries) {
                child = RepaintBoundary.wrap(child: child, childIndex: index);
            }

            if (this.addAutomaticKeepAlives) {
                child = new AutomaticKeepAlive(child: child);
            }

            return child;
        }

        public override bool shouldRebuild(SliverChildDelegate oldDelegate) {
            return true;
        }
    }

    public class SliverChildListDelegate : SliverChildDelegate {
        public readonly bool addAutomaticKeepAlives;

        public readonly bool addRepaintBoundaries;

        public readonly List<Widget> children;

        public SliverChildListDelegate(
            List<Widget> children,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true
        ) {
            D.assert(children != null);
            this.children = children;
            this.addAutomaticKeepAlives = addAutomaticKeepAlives;
            this.addRepaintBoundaries = addRepaintBoundaries;
        }

        public override int? estimatedChildCount {
            get { return this.children.Count; }
        }

        public override Widget build(BuildContext context, int index) {
            D.assert(this.children != null);
            if (index < 0 || index >= this.children.Count) {
                return null;
            }

            Widget child = this.children[index: index];
            D.assert(child != null);
            if (this.addRepaintBoundaries) {
                child = RepaintBoundary.wrap(child: child, childIndex: index);
            }

            if (this.addAutomaticKeepAlives) {
                child = new AutomaticKeepAlive(child: child);
            }

            return child;
        }

        public override bool shouldRebuild(SliverChildDelegate oldDelegate) {
            return this.children != ((SliverChildListDelegate) oldDelegate).children;
        }
    }

    public abstract class SliverMultiBoxAdaptorWidget : RenderObjectWidget {
        public readonly SliverChildDelegate del;

        protected SliverMultiBoxAdaptorWidget(
            Key key = null,
            SliverChildDelegate del = null
        ) : base(key: key) {
            D.assert(del != null);
            this.del = del;
        }

        public override Element createElement() {
            return new SliverMultiBoxAdaptorElement(this);
        }

        public double? estimateMaxScrollOffset(
            SliverConstraints constraints,
            int firstIndex,
            int lastIndex,
            double leadingScrollOffset,
            double trailingScrollOffset
        ) {
            D.assert(lastIndex >= firstIndex);
            return this.del.estimateMaxScrollOffset(
                firstIndex: firstIndex,
                lastIndex: lastIndex,
                leadingScrollOffset: leadingScrollOffset,
                trailingScrollOffset: trailingScrollOffset
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties: properties);
            properties.add(new DiagnosticsProperty<SliverChildDelegate>("del", value: this.del));
        }
    }

    public class SliverList : SliverMultiBoxAdaptorWidget {
        public SliverList(
            Key key = null,
            SliverChildDelegate del = null
        ) : base(key: key, del: del) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            SliverMultiBoxAdaptorElement element = (SliverMultiBoxAdaptorElement) context;
            return new RenderSliverList(childManager: element);
        }
    }

    public class SliverFixedExtentList : SliverMultiBoxAdaptorWidget {
        public readonly double itemExtent;

        public SliverFixedExtentList(
            Key key = null,
            SliverChildDelegate del = null,
            double itemExtent = 0
        ) : base(key: key, del: del) {
            this.itemExtent = itemExtent;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            SliverMultiBoxAdaptorElement element = (SliverMultiBoxAdaptorElement) context;
            return new RenderSliverFixedExtentList(childManager: element, itemExtent: this.itemExtent);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderSliverFixedExtentList) renderObjectRaw;
            renderObject.itemExtent = this.itemExtent;
        }
    }

    public class SliverFillViewport : SliverMultiBoxAdaptorWidget {
        public readonly double viewportFraction;

        public SliverFillViewport(
            Key key = null, SliverChildDelegate del = null,
            double viewportFraction = 1.0) : base(key: key, del: del) {
            D.assert(viewportFraction > 0.0);
            this.viewportFraction = viewportFraction;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            SliverMultiBoxAdaptorElement element = (SliverMultiBoxAdaptorElement) context;
            return new RenderSliverFillViewport(childManager: element, viewportFraction: this.viewportFraction);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderSliverFillViewport) renderObject).viewportFraction = this.viewportFraction;
        }
    }

    public class SliverMultiBoxAdaptorElement : RenderObjectElement, RenderSliverBoxChildManager {
        readonly SplayTree<int, Element> _childElements = new SplayTree<int, Element>();

        readonly Dictionary<int, Widget> _childWidgets = new Dictionary<int, Widget>();

        RenderBox _currentBeforeChild;

        int? _currentlyUpdatingChildIndex;

        bool _didUnderflow;

        public SliverMultiBoxAdaptorElement(SliverMultiBoxAdaptorWidget widget) : base(widget: widget) {
        }

        public new SliverMultiBoxAdaptorWidget widget {
            get { return (SliverMultiBoxAdaptorWidget) base.widget; }
        }

        public new RenderSliverMultiBoxAdaptor renderObject {
            get { return (RenderSliverMultiBoxAdaptor) base.renderObject; }
        }

        public void createChild(int index, RenderBox after = null) {
            D.assert(this._currentlyUpdatingChildIndex == null);
            this.owner.buildScope(this, () => {
                bool insertFirst = after == null;
                D.assert(insertFirst || this._childElements[index - 1] != null);
                this._currentBeforeChild = insertFirst ? null : (RenderBox) this._childElements[index - 1].renderObject;
                Element newChild;
                try {
                    this._currentlyUpdatingChildIndex = index;
                    this._childElements.TryGetValue(key: index, value: out newChild);
                    newChild = this.updateChild(child: newChild, this._build(index: index), newSlot: index);
                }
                finally {
                    this._currentlyUpdatingChildIndex = null;
                }

                if (newChild != null) {
                    this._childElements[key: index] = newChild;
                }
                else {
                    this._childElements.Remove(key: index);
                }
            });
        }

        public void removeChild(RenderBox child) {
            int index = this.renderObject.indexOf(child: child);
            D.assert(this._currentlyUpdatingChildIndex == null);
            D.assert(index >= 0);
            this.owner.buildScope(this, () => {
                D.assert(this._childElements.ContainsKey(key: index));
                try {
                    this._currentlyUpdatingChildIndex = index;
                    Element result = this.updateChild(this._childElements[key: index], null, newSlot: index);
                    D.assert(result == null);
                }
                finally {
                    this._currentlyUpdatingChildIndex = null;
                }

                this._childElements.Remove(key: index);
                D.assert(!this._childElements.ContainsKey(key: index));
            });
        }

        public double estimateMaxScrollOffset(SliverConstraints constraints,
            int firstIndex = 0,
            int lastIndex = 0,
            double leadingScrollOffset = 0,
            double trailingScrollOffset = 0
        ) {
            int? childCount = this.childCount;
            if (childCount == null) {
                return double.PositiveInfinity;
            }

            return this.widget.estimateMaxScrollOffset(
                       constraints: constraints,
                       firstIndex: firstIndex,
                       lastIndex: lastIndex,
                       leadingScrollOffset: leadingScrollOffset,
                       trailingScrollOffset: trailingScrollOffset
                   ) ?? _extrapolateMaxScrollOffset(
                       firstIndex: firstIndex,
                       lastIndex: lastIndex,
                       leadingScrollOffset: leadingScrollOffset,
                       trailingScrollOffset: trailingScrollOffset,
                       childCount: childCount.Value
                   );
        }

        public int? childCount {
            get { return this.widget.del.estimatedChildCount; }
        }

        public void didStartLayout() {
            D.assert(this.debugAssertChildListLocked());
        }

        public void didFinishLayout() {
            D.assert(this.debugAssertChildListLocked());
            int firstIndex = this._childElements.FirstOrDefault().Key;
            int lastIndex = this._childElements.LastOrDefault().Key;
            this.widget.del.didFinishLayout(firstIndex: firstIndex, lastIndex: lastIndex);
        }

        public bool debugAssertChildListLocked() {
            D.assert(this._currentlyUpdatingChildIndex == null);
            return true;
        }

        public void didAdoptChild(RenderBox child) {
            D.assert(this._currentlyUpdatingChildIndex != null);
            SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            childParentData.index = this._currentlyUpdatingChildIndex.Value;
        }

        public void setDidUnderflow(bool value) {
            this._didUnderflow = value;
        }

        public override void update(Widget newWidgetRaw) {
            var newWidget = (SliverMultiBoxAdaptorWidget) newWidgetRaw;
            SliverMultiBoxAdaptorWidget oldWidget = this.widget;
            base.update(newWidget: newWidget);
            SliverChildDelegate newDelegate = newWidget.del;
            SliverChildDelegate oldDelegate = oldWidget.del;
            if (newDelegate != oldDelegate &&
                (newDelegate.GetType() != oldDelegate.GetType() ||
                 newDelegate.shouldRebuild(oldDelegate: oldDelegate))) {
                this.performRebuild();
            }
        }

        protected override void performRebuild() {
            this._childWidgets.Clear();
            base.performRebuild();

            this._currentBeforeChild = null;
            D.assert(this._currentlyUpdatingChildIndex == null);
            try {
                int firstIndex = 0;
                int lastIndex = 0;

                if (!this._childElements.isEmpty()) {
                    firstIndex = this._childElements.First().Key;
                    lastIndex = this._childElements.Last().Key;
                    if (this._didUnderflow) {
                        lastIndex += 1;
                    }
                }

                for (int index = firstIndex; index <= lastIndex; ++index) {
                    this._currentlyUpdatingChildIndex = index;
                    Element newChild = this.updateChild(this._childElements.getOrDefault(key: index),
                        this._build(index: index),
                        newSlot: index);
                    if (newChild != null) {
                        this._childElements[key: index] = newChild;
                        this._currentBeforeChild = (RenderBox) newChild.renderObject;
                    }
                    else {
                        this._childElements.Remove(key: index);
                    }
                }
            }
            finally {
                this._currentlyUpdatingChildIndex = null;
            }
        }

        Widget _build(int index) {
            return this._childWidgets.putIfAbsent(key: index, () => this.widget.del.build(this, index: index));
        }

        protected override Element updateChild(Element child, Widget newWidget, object newSlot) {
            SliverMultiBoxAdaptorParentData oldParentData = null;
            if (child != null && child.renderObject != null) {
                oldParentData = (SliverMultiBoxAdaptorParentData) child.renderObject.parentData;
            }

            Element newChild = base.updateChild(child: child, newWidget: newWidget, newSlot: newSlot);

            SliverMultiBoxAdaptorParentData newParentData = null;
            if (newChild != null && newChild.renderObject != null) {
                newParentData = (SliverMultiBoxAdaptorParentData) newChild.renderObject.parentData;
            }

            if (oldParentData != newParentData && oldParentData != null && newParentData != null) {
                newParentData.layoutOffset = oldParentData.layoutOffset;
            }

            return newChild;
        }

        protected override void forgetChild(Element child) {
            D.assert(child != null);
            D.assert(child.slot != null);
            D.assert(this._childElements.ContainsKey((int) child.slot));
            this._childElements.Remove((int) child.slot);
        }

        static double _extrapolateMaxScrollOffset(
            int firstIndex,
            int lastIndex,
            double leadingScrollOffset,
            double trailingScrollOffset,
            int childCount
        ) {
            if (lastIndex == childCount - 1) {
                return trailingScrollOffset;
            }

            int reifiedCount = lastIndex - firstIndex + 1;
            double averageExtent = (trailingScrollOffset - leadingScrollOffset) / reifiedCount;
            int remainingCount = childCount - lastIndex - 1;
            return trailingScrollOffset + averageExtent * remainingCount;
        }

        protected override void insertChildRenderObject(RenderObject child, object slotRaw) {
            D.assert(slotRaw != null);
            int slot = (int) slotRaw;

            D.assert(this._currentlyUpdatingChildIndex == slot);
            D.assert(this.renderObject.debugValidateChild(child: child));
            this.renderObject.insert((RenderBox) child, after: this._currentBeforeChild);
            D.assert(() => {
                SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                D.assert(slot == childParentData.index);
                return true;
            });
        }

        protected override void moveChildRenderObject(RenderObject child, object slotRaw) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(this._currentlyUpdatingChildIndex != null);
            this.renderObject.remove((RenderBox) child);
        }

        public override void visitChildren(ElementVisitor visitor) {
            D.assert(!this._childElements.Values.Any(child => child == null));
            this._childElements.Values.ToList().ForEach(e => visitor(element: e));
        }

        public override void debugVisitOnstageChildren(ElementVisitor visitor) {
            this._childElements.Values.Where(child => {
                SliverMultiBoxAdaptorParentData parentData =
                    (SliverMultiBoxAdaptorParentData) child.renderObject.parentData;
                double itemExtent = 0;
                switch (this.renderObject.constraints.axis) {
                    case Axis.horizontal:
                        itemExtent = child.renderObject.paintBounds.width;
                        break;
                    case Axis.vertical:
                        itemExtent = child.renderObject.paintBounds.height;
                        break;
                }

                return parentData.layoutOffset < this.renderObject.constraints.scrollOffset +
                       this.renderObject.constraints.remainingPaintExtent &&
                       parentData.layoutOffset + itemExtent > this.renderObject.constraints.scrollOffset;
            }).ToList().ForEach(e => visitor(element: e));
        }
    }

    public class KeepAlive : ParentDataWidget<SliverMultiBoxAdaptorWidget> {
        public readonly bool keepAlive;

        public KeepAlive(
            Key key = null,
            bool keepAlive = true,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(child != null);
            this.keepAlive = keepAlive;
        }

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is SliverMultiBoxAdaptorParentData);
            SliverMultiBoxAdaptorParentData parentData = (SliverMultiBoxAdaptorParentData) renderObject.parentData;
            if (parentData.keepAlive != this.keepAlive) {
                parentData.keepAlive = this.keepAlive;
                var targetParent = renderObject.parent;
                if (targetParent is RenderObject && !this.keepAlive) {
                    ((RenderObject) targetParent).markNeedsLayout();
                }
            }
        }

        public override bool debugCanApplyOutOfTurn() {
            return this.keepAlive;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties: properties);
            properties.add(new DiagnosticsProperty<bool>("keepAlive", value: this.keepAlive));
        }
    }
}