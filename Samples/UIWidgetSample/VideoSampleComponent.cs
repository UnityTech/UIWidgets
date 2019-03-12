using UnityEngine;
using UnityEngine.Video;
using Texture = Unity.UIWidgets.widgets.Texture;

namespace UIWidgetsSample {
    public class VideoSampleComponent : MonoBehaviour {
        public VideoClip videoClip;
        public RenderTexture renderTexture;

        void Start() {
            var videoPlayer = this.gameObject.AddComponent<VideoPlayer>();
            videoPlayer.clip = this.videoClip;
            videoPlayer.targetTexture = this.renderTexture;
            videoPlayer.isLooping = true;
            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.frameReady += (_, __) => Texture.textureFrameAvailable();
            videoPlayer.Play();
        }
    }
}
