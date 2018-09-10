using System;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.scheduler;
using UIWidgets.ui;

namespace UIWidgets.rendering {
    public class RendererBinding : GestureBinding {
        public RendererBinding(Window window) : base(window) {
            this._pipelineOwner = new PipelineOwner(
                binding: this,
                onNeedVisualUpdate: this.ensureVisualUpdate
            );

            window.onMetricsChanged += this.handleMetricsChanged;
            this.initRenderView();
            D.assert(this.renderView != null);
            this.addPersistentFrameCallback(this._handlePersistentFrameCallback);
        }

        public void initRenderView() {
            D.assert(this.renderView == null);
            this.renderView = new RenderView(configuration: this.createViewConfiguration());
            this.renderView.scheduleInitialFrame();
        }

        public PipelineOwner pipelineOwner {
            get { return this._pipelineOwner; }
        }

        readonly PipelineOwner _pipelineOwner;

        public RenderView renderView {
            get { return (RenderView) this._pipelineOwner.rootNode; }
            set { this._pipelineOwner.rootNode = value; }
        }

        protected virtual void handleMetricsChanged() {
            this.renderView.configuration = this.createViewConfiguration();
            this.scheduleForcedFrame();
        }

        protected virtual ViewConfiguration createViewConfiguration() {
            var devicePixelRatio = this.window.devicePixelRatio;
            return new ViewConfiguration(
                size: this.window.physicalSize / devicePixelRatio,
                devicePixelRatio: devicePixelRatio
            );
        }

        void _handlePersistentFrameCallback(TimeSpan timeStamp) {
            this.drawFrame();
        }

        protected virtual void drawFrame() {
            this.pipelineOwner.flushLayout();
            this.pipelineOwner.flushCompositingBits();
            this.pipelineOwner.flushPaint();
            this.renderView.compositeFrame();
        }

        public override void hitTest(HitTestResult result, Offset position) {
            D.assert(this.renderView != null);
            this.renderView.hitTest(result, position: position);
            base.hitTest(result, position);
        }
    }

    public class RendererBindings {
        public RendererBindings(Window window) {
            this.window = window;
            this.rendererBinding = new RendererBinding(window);
        }

        public readonly Window window;
        public readonly RendererBinding rendererBinding;

        public void setRoot(RenderBox root) {
            this.rendererBinding.renderView.child = root;
        }
    }
}