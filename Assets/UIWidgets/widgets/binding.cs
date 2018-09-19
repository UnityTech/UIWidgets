using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.rendering;
using UIWidgets.ui;

namespace UIWidgets.widgets {
    public interface WidgetsBindingObserver {
        void didChangeMetrics();
    }

    public class WidgetsBinding : RendererBinding {
        public WidgetsBinding(Window window) : base(window) {
            _buildOwner = new BuildOwner(window);
            this.buildOwner.onBuildScheduled = this._handleBuildScheduled;
            window.onLocaleChanged += this.handleLocaleChanged;
        }

        public BuildOwner buildOwner {
            get { return this._buildOwner; }
        }

        private readonly BuildOwner _buildOwner;

        public Element renderViewElement {
            get { return this._renderViewElement; }
        }

        Element _renderViewElement;

        public List<WidgetsBindingObserver> _observers = new List<WidgetsBindingObserver>();

        void addObserver(WidgetsBindingObserver observer) {
            _observers.Add(observer);
        }

        protected override void handleMetricsChanged() {
            base.handleMetricsChanged();
            foreach (WidgetsBindingObserver observer in _observers) {
                observer.didChangeMetrics();
            }
        }

        bool removeObserver(WidgetsBindingObserver observer) {
            return _observers.Remove(observer);
        }

        void _handleBuildScheduled() {
            ensureVisualUpdate();
        }

        void handleLocaleChanged() {
            // todo
//            dispatchLocaleChanged(window.locale);
        }

        protected override void drawFrame() {
            if (renderViewElement != null) {
                buildOwner.buildScope(renderViewElement);
            }

            base.drawFrame();
            buildOwner.finalizeTree();
        }

        public void attachRootWidget(Widget rootWidget) {
            var _adapter = new RenderObjectToWidgetAdapter<RenderBox>(
                container: renderView,
                child: rootWidget
            );
            _renderViewElement = _adapter.attachToRenderTree(buildOwner,
                _renderViewElement as RenderObjectToWidgetElement<RenderBox>);
        }
    }

    public class RenderObjectToWidgetAdapter<T> : RenderObjectWidget where T : RenderObject {
        public RenderObjectToWidgetAdapter(Widget child, RenderObjectWithChildMixinRenderObject<T> container) : base(
            new GlobalObjectKey<State<StatefulWidget>>(container)) {
            this.child = child;
            this.container = container;
        }

        public Widget child;

        public RenderObjectWithChildMixinRenderObject<T> container;

        public string debugShortDescription;

        public override Element createElement() {
            return new RenderObjectToWidgetElement<T>(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return container;
        }

        public void updateRenderObject(BuildContext context, RenderObject renderObject) {
        }

        public RenderObjectToWidgetElement<T> attachToRenderTree(BuildOwner owner,
            RenderObjectToWidgetElement<T> element) {
            if (element == null) {
                element = (RenderObjectToWidgetElement<T>) createElement();
                element.assignOwner(owner);
                owner.buildScope(element, () => { element.mount(null, null); });
            }
            else {
                element._newWidget = this;
                element.markNeedsBuild();
            }

            return element;
        }
    }

    public class RenderObjectToWidgetElement<T> : RootRenderObjectElement where T : RenderObject {
        public RenderObjectToWidgetElement(RenderObjectWidget widget) : base(widget) {
        }

        public RenderObjectToWidgetAdapter<T> widget {
            get { return (RenderObjectToWidgetAdapter<T>) base.widget; }
        }

        Element _child;

        static readonly object _rootChildSlot = new object();

        public Widget _newWidget;

        public new RenderObjectWithChildMixin renderObject {
            get { return base.renderObject as RenderObjectWithChildMixin; }
        }

        public override void visitChildren(ElementVisitor visitor) {
            if (_child != null)
                visitor(_child);
        }

        protected override void forgetChild(Element child) {
            D.assert(child == _child);
            _child = null;
        }

        public override void mount(Element parent, object newSlot) {
            D.assert(parent == null);
            base.mount(parent, newSlot);
            _rebuild();
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(widget == newWidget);
            _rebuild();
        }

        protected override void performRebuild() {
            if (_newWidget != null) {
                Widget newWidget = _newWidget;
                _newWidget = null;
                update(newWidget);
            }

            base.performRebuild();
            D.assert(_newWidget == null);
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            D.assert(slot == _rootChildSlot);
            renderObject.child = child;
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(renderObject.child == child);
            renderObject.child = null;
        }

        void _rebuild() {
            try {
                _child = updateChild(_child, widget.child, _rootChildSlot);
                D.assert(_child != null);
            }
            catch (Exception e) {
                Widget error = ErrorWidget.builder(new UIWidgetsErrorDetails(e));
                _child = updateChild(null, error, _rootChildSlot);
            }
        }
    }

    public class WidgetsBindings {
        public WidgetsBindings(Window window) {
            this.window = window;
            this.widgetsBinding = new WidgetsBinding(window);
        }

        public readonly Window window;
        public readonly WidgetsBinding widgetsBinding;

        public void attachRootWidget(Widget root) {
            this.widgetsBinding.attachRootWidget(root);
        }
    }
}