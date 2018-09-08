using System;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.scheduler;
using UIWidgets.ui;

namespace UIWidgets.rendering {
    public class RendererBinding : HitTestable {
        public RendererBinding(Window window, SchedulerBinding schedulerBinding) {
            this._window = window;

            this._schedulerBinding = schedulerBinding;

            this._pipelineOwner = new PipelineOwner(
                binding: this,
                onNeedVisualUpdate: this._schedulerBinding.ensureVisualUpdate
            );

            window.onMetricsChanged += this.handleMetricsChanged;
            this.initRenderView();
            this._schedulerBinding.addPersistentFrameCallback(this._handlePersistentFrameCallback);
        }

        public readonly Window _window;

        public readonly SchedulerBinding _schedulerBinding;

        public void initRenderView() {
            this.renderView = new RenderView(configuration: this.createViewConfiguration());
            this.renderView.scheduleInitialFrame();
        }

        public PipelineOwner pipelineOwner {
            get { return this._pipelineOwner; }
        }

        public PipelineOwner _pipelineOwner;

        public RenderView renderView {
            get { return (RenderView) this._pipelineOwner.rootNode; }
            set { this._pipelineOwner.rootNode = value; }
        }

        public void handleMetricsChanged() {
            this.renderView.configuration = this.createViewConfiguration();
            this._schedulerBinding.scheduleForcedFrame();
        }

        public ViewConfiguration createViewConfiguration() {
            var devicePixelRatio = this._window.devicePixelRatio;
            return new ViewConfiguration(
                size: this._window.physicalSize / devicePixelRatio,
                devicePixelRatio: devicePixelRatio
            );
        }

        public void _handlePersistentFrameCallback(TimeSpan timeStamp) {
            this.drawFrame();
        }
        
        public void hitTest(HitTestResult result, Offset position) {
            D.assert(this.renderView != null);
            this.renderView.hitTest(result, position: position);
        }

        public void drawFrame() {
            this.pipelineOwner.flushLayout();
            this.pipelineOwner.flushCompositingBits();
            this.pipelineOwner.flushPaint();
            this.renderView.compositeFrame();
        }

        public void render(Scene scene) {
            this._window.render(scene);
        }
    }

    public class RendererBindings {
        public RendererBindings(Window window) {
            this.window = window;
            this.schedulerBinding = new SchedulerBinding(window);
            this.rendererBinding = new RendererBinding(window, this.schedulerBinding);
            this.gestureBinding = new GestureBinding(window, this.rendererBinding);
        }

        public readonly Window window;
        public readonly RendererBinding rendererBinding;
        public readonly SchedulerBinding schedulerBinding;
        public readonly GestureBinding gestureBinding;

        public void setRoot(RenderBox root) {
            this.rendererBinding.renderView.child = root;
        }
    }
}