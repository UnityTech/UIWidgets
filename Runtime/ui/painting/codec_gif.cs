using System;
using System.Collections;
using System.IO;
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
        volatile GifDecoder _decoder;
        volatile MemoryStream _stream;
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
            this._decoder = new GifDecoder();
            this._stream = new MemoryStream(this._bytes);
            this._frameData = new FrameData() {
                frameInfo = new FrameInfo()
            };
        }

        IEnumerator _startDecoding() {
            this._stream.Seek(0, SeekOrigin.Begin);

            if (this._decoder.read(this._stream) != GifDecoder.STATUS_OK) {
                throw new Exception("Failed to decode gif.");
            }

            this._width = this._decoder.frameWidth;
            this._height = this._decoder.frameHeight;

            if (this._texture == null) {
                this._texture = new Texture2D(this._width, this._height, TextureFormat.BGRA32, false);
                this._texture.hideFlags = HideFlags.HideAndDontSave;
                this._image = new Image(this._texture, isDynamic: true);
                this._frameData.frameInfo.image = this._image;
            }

            this._frameData.gifFrame = this._decoder.currentFrame;
            D.assert(this._frameData.gifFrame != null);

            int i = 0;
            while (true) {
                if (this._decoder.nextFrame() != GifDecoder.STATUS_OK) {
                    throw new Exception("Failed to decode gif.");
                }

                if (this._decoder.done) {
                    break;
                }


                i++;

                yield return null;
            }

            D.assert(this._decoder.frameCount == i);
            this._frameCount = this._decoder.frameCount;
            this._repetitionCount = this._decoder.loopCount;
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

        public FrameInfo getNextFrame() {
            this._nextFrame();
            this._texture.LoadRawTextureData(this._frameData.gifFrame.bytes);
            this._texture.Apply();
            this._frameData.frameInfo.duration = TimeSpan.FromMilliseconds(this._frameData.gifFrame.delay);
            return this._frameData.frameInfo;
        }

        public void Dispose() {
            this._decoder.Dispose();
        }
    }
}