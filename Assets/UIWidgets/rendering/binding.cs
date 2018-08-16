using System;
using UIWidgets.scheduler;
using UIWidgets.ui;

namespace UIWidgets.rendering {
    public abstract class RendererBinding : SchedulerBinding {
        public RendererBinding(Window window) : base(window) {
            this._pipelineOwner = new PipelineOwner(
                binding: this,
                onNeedVisualUpdate: this.ensureVisualUpdate
            );

            window._onMetricsChanged = this.handleMetricsChanged;
            this.initRenderView();
            this.addPersistentFrameCallback(this._handlePersistentFrameCallback);
        }

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
            this.scheduleForcedFrame();
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

    public class RendererBindingImpl : RendererBinding {
        public RendererBindingImpl(Window window, RenderBox root) : base(window) {
            this.renderView.child = root;
        }
    }
}