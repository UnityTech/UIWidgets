using System;
using System.Collections;
using System.IO;
using RSG;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class GifCodec : Codec {
        public static bool isGif(byte[] bytes) {
            return bytes != null && bytes.Length >= 3 && bytes[0] == 'G' && bytes[1] == 'I' && bytes[2] == 'F';
        }

        public class FrameData {
            public FrameInfo frameInfo;
            public GifDecoder.GifFrame gifFrame;
        }

        volatile byte[] _bytes;
        volatile int _width;
        volatile int _height;
        volatile int _frameCount;
        volatile int _repetitionCount;
        volatile bool _isDone;
        volatile int _frameIndex;
        volatile Texture2D _texture;
        volatile FrameData _frameData;
        volatile Image _image;
        IEnumerator _coroutine;

        public GifCodec(byte[] bytes) {
            D.assert(bytes != null);
            D.assert(isGif(bytes));

            this._frameCount = 0;
            this._repetitionCount = 0;
            this._isDone = false;
            this._frameIndex = 0;
            this._bytes = bytes;
            this._coroutine = this._startDecoding();
            this._init();
            this._texture = new Texture2D(this._width, this._height, TextureFormat.BGRA32, false);
            this._texture.hideFlags = HideFlags.HideAndDontSave;
            this._image = new Image(this._texture);
        }

        void _init() {
            var bytesStream = new MemoryStream(this._bytes);

            var gifDecoder = new GifDecoder();
            if (gifDecoder.read(bytesStream) != GifDecoder.STATUS_OK) {
                throw new Exception("Failed to decode gif.");
            }

            this._width = gifDecoder.frameWidth;
            this._height = gifDecoder.frameHeight;
        }

        IEnumerator _startDecoding() {
            var bytesStream = new MemoryStream(this._bytes);

            var gifDecoder = new GifDecoder();
            if (gifDecoder.read(bytesStream) != GifDecoder.STATUS_OK) {
                throw new Exception("Failed to decode gif.");
            }

            this._width = gifDecoder.frameWidth;
            this._height = gifDecoder.frameHeight;

            int i = 0;
            while (true) {
                if (gifDecoder.nextFrame() != GifDecoder.STATUS_OK) {
                    throw new Exception("Failed to decode gif.");
                }

                if (gifDecoder.done) {
                    break;
                }

                var frameData = new FrameData {
                    gifFrame = gifDecoder.currentFrame
                };

                this._frameData = frameData;

                i++;

                yield return null;
            }

            D.assert(gifDecoder.frameCount == i);
            this._frameCount = gifDecoder.frameCount;
            this._repetitionCount = gifDecoder.loopCount;
            this._isDone = true;
        }

        public int frameCount {
            get { return this._frameCount; }
        }

        public int repetitionCount {
            get { return this._repetitionCount - 1; }
        }


        void _nextFrame() {
            this._frameIndex++;

            this._coroutine.MoveNext();

            if (this._isDone && this._frameIndex >= this._frameCount) {
                this._frameIndex = 0;
                this._isDone = false;
                this._coroutine = this._startDecoding();
                this._coroutine.MoveNext();
            }
        }

        public IPromise<FrameInfo> getNextFrame() {
            this._nextFrame();
            this._texture.LoadRawTextureData(this._frameData.gifFrame.bytes);
            this._texture.Apply();
            this._frameData.frameInfo = new FrameInfo() {
                image = this._image,
                duration = TimeSpan.FromMilliseconds(this._frameData.gifFrame.delay)
            };
            return Promise<FrameInfo>.Resolved(this._frameData.frameInfo);
        }

        public void Dispose() {
        }
    }
}