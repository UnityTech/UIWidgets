using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public interface WidgetsBindingObserver {
        void didChangeMetrics();

        void didChangeTextScaleFactor();

        void didChangePlatformBrightness();

        void didChangeLocales(List<Locale> locale);

        IPromise<bool> didPopRoute();

        IPromise<bool> didPushRoute(string route);
    }

    public class WidgetsBinding : RendererBinding {
        public new static WidgetsBinding instance {
            get { return (WidgetsBinding) RendererBinding.instance; }
            set { RendererBinding.instance = value; }
        }

        public WidgetsBinding(bool inEditorWindow = false) : base(inEditorWindow) {
            this.buildOwner.onBuildScheduled = this._handleBuildScheduled;
            Window.instance.onLocaleChanged += this.handleLocaleChanged;
            this.widgetInspectorService = new WidgetInspectorService(this);
            this.addPersistentFrameCallback((duration) => {
                TextBlobMesh.tickNextFrame();
                TessellationGenerator.tickNextFrame();
                uiTessellationGenerator.tickNextFrame();
                uiPathCacheManager.tickNextFrame();
            });
        }

        public BuildOwner buildOwner {
            get { return this._buildOwner; }
        }

        readonly BuildOwner _buildOwner = new BuildOwner();

        public FocusManager focusManager {
            get { return this._buildOwner.focusManager; }
        }

        readonly List<WidgetsBindingObserver> _observers = new List<WidgetsBindingObserver>();

        public void addObserver(WidgetsBindingObserver observer) {
            this._observers.Add(observer);
        }

        public bool removeObserver(WidgetsBindingObserver observer) {
            return this._observers.Remove(observer);
        }

        public void handlePopRoute() {
            var idx = -1;
            
            void _handlePopRouteSub(bool result) {
                if (!result) {
                    idx++;
                    if (idx >= this._observers.Count) {
                        Application.Quit();
                        return;
                    }
                    this._observers[idx].didPopRoute().Then((Action<bool>) _handlePopRouteSub);
                }
            }
            
            _handlePopRouteSub(false);
        }

        public readonly WidgetInspectorService widgetInspectorService;

        protected override void handleMetricsChanged() {
            base.handleMetricsChanged();
            foreach (WidgetsBindingObserver observer in this._observers) {
                observer.didChangeMetrics();
            }
        }

        protected override void handleTextScaleFactorChanged() {
            base.handleTextScaleFactorChanged();
            foreach (WidgetsBindingObserver observer in this._observers) {
                observer.didChangeTextScaleFactor();
            }
        }
        
        protected override void handlePlatformBrightnessChanged() {
            base.handlePlatformBrightnessChanged();
            foreach (WidgetsBindingObserver observer in this._observers) {
                observer.didChangePlatformBrightness();
            }
        }

        protected virtual void handleLocaleChanged() {
            this.dispatchLocalesChanged(Window.instance.locales);
        }

        protected virtual void dispatchLocalesChanged(List<Locale> locales) {
            foreach (WidgetsBindingObserver observer in this._observers) {
                observer.didChangeLocales(locales);
            }
        }

        void _handleBuildScheduled() {
            D.assert(() => {
                if (this.debugBuildingDirtyElements) {
                    throw new UIWidgetsError(
                        "Build scheduled during frame.\n" +
                        "While the widget tree was being built, laid out, and painted, " +
                        "a new frame was scheduled to rebuild the widget tree. " +
                        "This might be because setState() was called from a layout or " +
                        "paint callback. " +
                        "If a change is needed to the widget tree, it should be applied " +
                        "as the tree is being built. Scheduling a change for the subsequent " +
                        "frame instead results in an interface that lags behind by one frame. " +
                        "If this was done to make your build dependent on a size measured at " +
                        "layout time, consider using a LayoutBuilder, CustomSingleChildLayout, " +
                        "or CustomMultiChildLayout. If, on the other hand, the one frame delay " +
                        "is the desired effect, for example because this is an " +
                        "animation, consider scheduling the frame in a post-frame callback " +
                        "using SchedulerBinding.addPostFrameCallback or " +
                        "using an AnimationController to trigger the animation."
                    );
                }

                return true;
            });

            this.ensureVisualUpdate();
        }

        protected bool debugBuildingDirtyElements = false;

        protected override void drawFrame() {
            D.assert(!this.debugBuildingDirtyElements);
            D.assert(() => {
                this.debugBuildingDirtyElements = true;
                return true;
            });
            try {
                if (this.renderViewElement != null) {
                    this.buildOwner.buildScope(this.renderViewElement);
                }

                base.drawFrame();
                this.buildOwner.finalizeTree();
            }
            finally {
                D.assert(() => {
                    this.debugBuildingDirtyElements = false;
                    return true;
                });
            }
        }

        public RenderObjectToWidgetElement<RenderBox> renderViewElement {
            get { return this._renderViewElement; }
        }

        RenderObjectToWidgetElement<RenderBox> _renderViewElement;

        public void detachRootWidget() {
            if (this._renderViewElement == null) {
                return;
            }
            
            //The former widget tree must be layout first before its destruction
            this.drawFrame();
            this.attachRootWidget(null);
            this.buildOwner.buildScope(this._renderViewElement);
            this.buildOwner.finalizeTree();
            
            this.pipelineOwner.rootNode = null;
            this._renderViewElement.deactivate();
            this._renderViewElement.unmount();
            this._renderViewElement = null;
        }

        public void attachRootWidget(Widget rootWidget) {
            this._renderViewElement = new RenderObjectToWidgetAdapter<RenderBox>(
                container: this.renderView,
                debugShortDescription: "[root]",
                child: rootWidget
            ).attachToRenderTree(this.buildOwner, this._renderViewElement);
        }
    }

    public class RenderObjectToWidgetAdapter<T> : RenderObjectWidget where T : RenderObject {
        public RenderObjectToWidgetAdapter(
            Widget child = null,
            RenderObjectWithChildMixin<T> container = null,
            string debugShortDescription = null
        ) : base(
            new GlobalObjectKey<State>(container)) {
            this.child = child;
            this.container = container;
            this.debugShortDescription = debugShortDescription;
        }

        public readonly Widget child;

        public readonly RenderObjectWithChildMixin<T> container;

        public readonly string debugShortDescription;

        public override Element createElement() {
            return new RenderObjectToWidgetElement<T>(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return (RenderObject) this.container;
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
        }

        public RenderObjectToWidgetElement<T> attachToRenderTree(BuildOwner owner,
            RenderObjectToWidgetElement<T> element) {
            if (element == null) {
                owner.lockState(() => {
                    element = (RenderObjectToWidgetElement<T>) this.createElement();
                    D.assert(element != null);
                    element.assignOwner(owner);
                });
                owner.buildScope(element, () => { element.mount(null, null); });
            }
            else {
                element._newWidget = this;
                element.markNeedsBuild();
            }

            return element;
        }

        public override string toStringShort() {
            return this.debugShortDescription ?? base.toStringShort();
        }
    }

    public class RenderObjectToWidgetElement<T> : RootRenderObjectElement where T : RenderObject {
        public RenderObjectToWidgetElement(RenderObjectToWidgetAdapter<T> widget) : base(widget) {
        }

        public new RenderObjectToWidgetAdapter<T> widget {
            get { return (RenderObjectToWidgetAdapter<T>) base.widget; }
        }

        Element _child;

        static readonly object _rootChildSlot = new object();

        public override void visitChildren(ElementVisitor visitor) {
            if (this._child != null) {
                visitor(this._child);
            }
        }

        protected override void forgetChild(Element child) {
            D.assert(child == this._child);
            this._child = null;
        }

        public override void mount(Element parent, object newSlot) {
            D.assert(parent == null);
            base.mount(parent, newSlot);
            this._rebuild();
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(this.widget == newWidget);
            this._rebuild();
        }

        internal Widget _newWidget;

        protected override void performRebuild() {
            if (this._newWidget != null) {
                Widget newWidget = this._newWidget;
                this._newWidget = null;
                this.update(newWidget);
            }

            base.performRebuild();
            D.assert(this._newWidget == null);
        }

        void _rebuild() {
            try {
                this._child = this.updateChild(this._child, this.widget.child,
                    _rootChildSlot);
                // allow 
            }
            catch (Exception ex) {
                var details = new UIWidgetsErrorDetails(
                    exception: ex,
                    library: "widgets library",
                    context: "attaching to the render tree"
                );
                UIWidgetsError.reportError(details);

                Widget error = ErrorWidget.builder(details);
                this._child = this.updateChild(null, error, _rootChildSlot);
            }
        }

        public new RenderObjectWithChildMixin<T> renderObject {
            get { return (RenderObjectWithChildMixin<T>) base.renderObject; }
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            D.assert(slot == _rootChildSlot);
            D.assert(this.renderObject.debugValidateChild(child));
            this.renderObject.child = (T) child;
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(this.renderObject.child == child);
            this.renderObject.child = null;
        }
    }
}