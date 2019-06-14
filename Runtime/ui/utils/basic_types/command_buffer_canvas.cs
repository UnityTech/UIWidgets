using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class CommandBufferCanvas : RecorderCanvas {
        readonly PictureFlusher _flusher;

        public CommandBufferCanvas(RenderTexture renderTexture, float devicePixelRatio, MeshPool meshPool)
            : base(new PictureRecorder()) {
            this._flusher = new PictureFlusher(renderTexture, devicePixelRatio, meshPool);
        }

        public override float getDevicePixelRatio() {
            return this._flusher.getDevicePixelRatio();
        }

        public override void flush() {
            var picture = this._recorder.endRecording();
            this._recorder.reset();
            this._flusher.flush(picture);
        }
    }
}