using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class CommandBufferCanvas : uiRecorderCanvas {
        readonly PictureFlusher _flusher;

        public CommandBufferCanvas(RenderTexture renderTexture, float devicePixelRatio, MeshPool meshPool)
            : base(new uiPictureRecorder()) {
            this._flusher = new PictureFlusher(renderTexture, devicePixelRatio, meshPool);
        }

        public override float getDevicePixelRatio() {
            return this._flusher.getDevicePixelRatio();
        }

        public override void flush() {
            var picture = this._recorder.endRecording();
            this._flusher.flush(picture);
            this._recorder.reset();
            ObjectPool<uiPicture>.release(picture);
        }

        public void dispose() {
            this._flusher.dispose();
        }
    }
}