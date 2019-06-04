using System;
using RSG;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class FrameInfo {
        public Image image;
        public TimeSpan duration;
    }

    public interface Codec : IDisposable {
        int frameCount { get; }
        int repetitionCount { get; }
        FrameInfo getNextFrame();
    }

    public class ImageCodec : Codec {
        Image _image;

        public ImageCodec(Image image) {
            D.assert(image != null);
            this._image = image;
        }

        public int frameCount {
            get { return 1; }
        }

        public int repetitionCount {
            get { return 0; }
        }

        public FrameInfo getNextFrame() {
            D.assert(this._image != null);

            return new FrameInfo {
                duration = TimeSpan.Zero,
                image = this._image
            };
        }

        public void Dispose() {
            if (this._image != null) {
                this._image.Dispose();
                this._image = null;
            }
        }
    }


    public static class CodecUtils {
        public static IPromise<Codec> getCodec(byte[] bytes) {
            if (GifCodec.isGif(bytes)) {
                return Promise<Codec>.Resolved(new GifCodec(bytes));
            }

            var texture = new Texture2D(2, 2);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.LoadImage(bytes);
            return Promise<Codec>.Resolved(new ImageCodec(new Image(texture)));
        }

        public static IPromise<Codec> getCodec(Image image) {
            return Promise<Codec>.Resolved(new ImageCodec(image));
        }
    }
}