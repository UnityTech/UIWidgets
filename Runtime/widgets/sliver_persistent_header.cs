using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public abstract class SliverPersistentHeaderDelegate {
        public SliverPersistentHeaderDelegate() {
        }

        public abstract Widget build(BuildContext context, float shrinkOffset, bool overlapsContent);

        public abstract float? minExtent { get; }

        public abstract float? maxExtent { get; }

        public virtual FloatingHeaderSnapConfiguration snapConfiguration {
            get { return null; }
        }

        public abstract bool shouldRebuild(SliverPersistentHeaderDelegate oldDelegate);
    }

    public class SliverPersistentHeader : StatelessWidget {
        public SliverPersistentHeader(
            Key key = null,
            SliverPersistentHeaderDelegate del = null,
            bool pinned = false,
            bool floating = false
        ) : base(key: key) {
            D.assert(del != null);
            this.layoutDelegate = del;
            this.pinned = pinned;
            this.floating = floating;
        }

        public readonly SliverPersistentHeaderDelegate layoutDelegate;

        public readonly bool pinned;

        public readonly bool floating;

        public override Widget build(BuildContext context) {
            if (this.floating && this.pinned) {
                return new _SliverFloatingPinnedPersistentHeader(layoutDelegate: this.layoutDelegate);
            }

            if (this.pinned) {
                return new _SliverPinnedPersistentHeader(layoutDelegate: this.layoutDelegate);
            }

            if (this.floating) {
                return new _SliverFloatingPersistentHeader(layoutDelegate: this.layoutDelegate);
            }

            return new _SliverScrollingPersistentHeader(layoutDelegate: this.layoutDelegate);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverPersistentHeaderDelegate>("layoutDelegate", this.layoutDelegate));
            List<string> flags = new List<string> { };
            if (this.pinned) {
                flags.Add("pinned");
            }

            if (this.floating) {
                flags.Add("floating");
            }

            if (flags.isEmpty()) {
                flags.Add("normal");
            }

            properties.add(new EnumerableProperty<string>("mode", flags));
        }
    }

    class _SliverPersistentHeaderElement : RenderObjectElement {
        public _SliverPersistentHeaderElement(_SliverPersistentHeaderRenderObjectWidget widget) : base(widget) {
        }

        public new _SliverPersistentHeaderRenderObjectWidget widget {
            get { return (_SliverPersistentHeaderRenderObjectWidget) base.widget; }
        }

        public override RenderObject renderObject {
            get { return base.renderObject; }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            (this.renderObject as _RenderSliverPersistentHeaderForWidgetsMixin)._element = this;
        }

        public override void unmount() {
            base.unmount();
            (this.renderObject as _RenderSliverPersistentHeaderForWidgetsMixin)._element = null;
        }

        public override void update(Widget _newWidget) {
            base.update(_newWidget);
            _SliverPersistentHeaderRenderObjectWidget newWidget =
                _newWidget as _SliverPersistentHeaderRenderObjectWidget;
            _SliverPersistentHeaderRenderObjectWidget oldWidget = this.widget;
            SliverPersistentHeaderDelegate newDelegate = newWidget.layoutDelegate;
            SliverPersistentHeaderDelegate oldDelegate = oldWidget.layoutDelegate;
            if (newDelegate != oldDelegate &&
                (newDelegate.GetType() != oldDelegate.GetType() || newDelegate.shouldRebuild(oldDelegate))) {
                (this.renderObject as _RenderSliverPersistentHeaderForWidgetsMixin).triggerRebuild();
            }
        }

        protected override void performRebuild() {
            base.performRebuild();
            (this.renderObject as _RenderSliverPersistentHeaderForWidgetsMixin).triggerRebuild();
        }

        Element child;

        public void _build(float shrinkOffset, bool overlapsContent) {
            this.owner.buildScope(this,
                () => {
                    this.child = this.updateChild(this.child,
                        this.widget.layoutDelegate.build(this, shrinkOffset, overlapsContent), null);
                });
        }

        protected override void forgetChild(Element child) {
            D.assert(child == this.child);
            this.child = null;
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            D.assert((bool) (this.renderObject as RenderSliverPersistentHeader).debugValidateChild(child));
            (this.renderObject as RenderSliverPersistentHeader).child = (RenderBox) child;
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            (this.renderObject as RenderSliverPersistentHeader).child = null;
        }

        public override void visitChildren(ElementVisitor visitor) {
            if (this.child != null) {
                visitor(this.child);
            }
        }
    }

    abstract class _SliverPersistentHeaderRenderObjectWidget : RenderObjectWidget {
        public _SliverPersistentHeaderRenderObjectWidget(
            Key key = null,
            SliverPersistentHeaderDelegate layoutDelegate = null
        ) : base(key: key) {
            D.assert(layoutDelegate != null);
            this.layoutDelegate = layoutDelegate;
        }

        public readonly SliverPersistentHeaderDelegate layoutDelegate;

        public override Element createElement() {
            return new _SliverPersistentHeaderElement(this);
        }

        public abstract override RenderObject createRenderObject(BuildContext context);

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<SliverPersistentHeaderDelegate>("layoutDelegate", this.layoutDelegate));
        }
    }

    interface _RenderSliverPersistentHeaderForWidgetsMixin {
        _SliverPersistentHeaderElement _element { get; set; }

        float? minExtent { get; }

        float? maxExtent { get; }

        void triggerRebuild();
    }

    class _SliverScrollingPersistentHeader : _SliverPersistentHeaderRenderObjectWidget {
        public _SliverScrollingPersistentHeader(
            Key key = null,
            SliverPersistentHeaderDelegate layoutDelegate = null
        ) : base(key: key, layoutDelegate: layoutDelegate) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSliverScrollingPersistentHeaderForWidgets();
        }
    }

    abstract class _RenderSliverScrollingPersistentHeader : RenderSliverScrollingPersistentHeader {
    }

    class _RenderSliverScrollingPersistentHeaderForWidgets : _RenderSliverScrollingPersistentHeader,
        _RenderSliverPersistentHeaderForWidgetsMixin {
        public _SliverPersistentHeaderElement _element {
            get { return this._ele; }
            set { this._ele = value; }
        }

        _SliverPersistentHeaderElement _ele;

        public override float? minExtent {
            get { return this._element.widget.layoutDelegate.minExtent; }
        }

        public override float? maxExtent {
            get { return this._element.widget.layoutDelegate.maxExtent; }
        }

        protected override void updateChild(float shrinkOffset, bool overlapsContent) {
            D.assert(this._element != null);
            this._element._build(shrinkOffset, overlapsContent);
        }

        public void triggerRebuild() {
            this.markNeedsLayout();
        }
    }

    class _SliverPinnedPersistentHeader : _SliverPersistentHeaderRenderObjectWidget {
        public _SliverPinnedPersistentHeader(
            Key key = null,
            SliverPersistentHeaderDelegate layoutDelegate = null
        ) : base(key: key, layoutDelegate: layoutDelegate) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSliverPinnedPersistentHeaderForWidgets();
        }
    }

    abstract class _RenderSliverPinnedPersistentHeader : RenderSliverPinnedPersistentHeader {
    }

    class _RenderSliverPinnedPersistentHeaderForWidgets : _RenderSliverPinnedPersistentHeader,
        _RenderSliverPersistentHeaderForWidgetsMixin {
        public _SliverPersistentHeaderElement _element {
            get { return this._ele; }
            set { this._ele = value; }
        }

        _SliverPersistentHeaderElement _ele;

        public override float? minExtent {
            get { return this._element.widget.layoutDelegate.minExtent; }
        }

        public override float? maxExtent {
            get { return this._element.widget.layoutDelegate.maxExtent; }
        }

        protected override void updateChild(float shrinkOffset, bool overlapsContent) {
            D.assert(this._element != null);
            this._element._build(shrinkOffset, overlapsContent);
        }

        public void triggerRebuild() {
            this.markNeedsLayout();
        }
    }

    class _SliverFloatingPersistentHeader : _SliverPersistentHeaderRenderObjectWidget {
        public _SliverFloatingPersistentHeader(
            Key key = null,
            SliverPersistentHeaderDelegate layoutDelegate = null
        ) : base(key: key, layoutDelegate: layoutDelegate) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            _RenderSliverFloatingPersistentHeaderForWidgets ret = new _RenderSliverFloatingPersistentHeaderForWidgets();
            ret.snapConfiguration = this.layoutDelegate.snapConfiguration;
            return ret;
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderSliverFloatingPersistentHeaderForWidgets renderObject =
                _renderObject as _RenderSliverFloatingPersistentHeaderForWidgets;
            renderObject.snapConfiguration = this.layoutDelegate.snapConfiguration;
        }
    }

    abstract class _RenderSliverFloatingPinnedPersistentHeader : RenderSliverFloatingPinnedPersistentHeader {
    }

    class _RenderSliverFloatingPinnedPersistentHeaderForWidgets : _RenderSliverFloatingPinnedPersistentHeader,
        _RenderSliverPersistentHeaderForWidgetsMixin {
        public _SliverPersistentHeaderElement _element {
            get { return this._ele; }
            set { this._ele = value; }
        }

        _SliverPersistentHeaderElement _ele;

        public override float? minExtent {
            get { return this._element.widget.layoutDelegate.minExtent; }
        }

        public override float? maxExtent {
            get { return this._element.widget.layoutDelegate.maxExtent; }
        }

        protected override void updateChild(float shrinkOffset, bool overlapsContent) {
            D.assert(this._element != null);
            this._element._build(shrinkOffset, overlapsContent);
        }

        public void triggerRebuild() {
            this.markNeedsLayout();
        }
    }

    class _SliverFloatingPinnedPersistentHeader : _SliverPersistentHeaderRenderObjectWidget {
        public _SliverFloatingPinnedPersistentHeader(
            Key key = null,
            SliverPersistentHeaderDelegate layoutDelegate = null
        ) : base(key: key, layoutDelegate: layoutDelegate) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            _RenderSliverFloatingPinnedPersistentHeaderForWidgets ret =
                new _RenderSliverFloatingPinnedPersistentHeaderForWidgets();
            ret.snapConfiguration = this.layoutDelegate.snapConfiguration;
            return ret;
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderSliverFloatingPinnedPersistentHeaderForWidgets renderObject =
                _renderObject as _RenderSliverFloatingPinnedPersistentHeaderForWidgets;
            renderObject.snapConfiguration = this.layoutDelegate.snapConfiguration;
        }
    }

    abstract class _RenderSliverFloatingPersistentHeader : RenderSliverFloatingPersistentHeader {
    }

    class _RenderSliverFloatingPersistentHeaderForWidgets : _RenderSliverFloatingPersistentHeader,
        _RenderSliverPersistentHeaderForWidgetsMixin {
        public _SliverPersistentHeaderElement _element {
            get { return this._ele; }
            set { this._ele = value; }
        }

        _SliverPersistentHeaderElement _ele;

        public override float? minExtent {
            get { return this._element.widget.layoutDelegate.minExtent; }
        }

        public override float? maxExtent {
            get { return this._element.widget.layoutDelegate.maxExtent; }
        }

        protected override void updateChild(float shrinkOffset, bool overlapsContent) {
            D.assert(this._element != null);
            this._element._build(shrinkOffset, overlapsContent);
        }

        public void triggerRebuild() {
            this.markNeedsLayout();
        }
    }
}