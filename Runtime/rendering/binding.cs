using System;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.scheduler;
using UIWidgets.ui;

namespace UIWidgets.rendering {
    public class RendererBinding : GestureBinding {
        public static new RendererBinding instance {
            get { return (RendererBinding) GestureBinding.instance; }
            set { GestureBinding.instance = value; }
        }
        
        public RendererBinding() {
            this._pipelineOwner = new PipelineOwner(
                onNeedVisualUpdate: this.ensureVisualUpdate
            );

            Window.instance.onMetricsChanged += this.handleMetricsChanged;
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
            var devicePixelRatio = Window.instance.devicePixelRatio;
            return new ViewConfiguration(
                size: Window.instance.physicalSize / devicePixelRatio,
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
}