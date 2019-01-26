using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RSG;
using Unity.UIWidgets.async;
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

        readonly List<Promise<FrameData>> _frames;
        volatile int _width;
        volatile int _height;
        volatile int _frameCount;
        volatile int _repetitionCount;
        volatile bool _isDone;
        volatile int _frameIndex;
        readonly UIWidgetsCoroutine _coroutine;

        public GifCodec(byte[] bytes) {
            D.assert(bytes != null);
            D.assert(isGif(bytes));

            this._frames = new List<Promise<FrameData>>();
            this._width = 0;
            this._height = 0;
            this._frameCount = 0;
            this._repetitionCount = 0;
            this._isDone = false;
            this._frameIndex = 0;

            this._coroutine = Window.instance.startBackgroundCoroutine(
                this._startDecoding(bytes, Window.instance));
        }

        IEnumerator _startDecoding(byte[] bytes, Window window) {
            var bytesStream = new MemoryStream(bytes);

            var gifDecoder = new GifDecoder();
            if (gifDecoder.read(bytesStream) != GifDecoder.STATUS_OK) {
                throw new Exception("Failed to decode gif.");
            }

            this._width = gifDecoder.frameWidth;
            this._height = gifDecoder.frameHeight;

            int i = 0;
            while (true) {
                yield return null;

                if (gifDecoder.nextFrame() != GifDecoder.STATUS_OK) {
                    throw new Exception("Failed to decode gif.");
                }

                if (gifDecoder.done) {
                    break;
                }

                var frameData = new FrameData {
                    gifFrame = gifDecoder.currentFrame,
                };

                Promise<FrameData> frame;

                lock (this._frames) {
                    if (i < this._frames.Count) {
                    }
                    else {
                        D.assert(this._frames.Count == i);
                        this._frames.Add(new Promise<FrameData>());
                    }

                    frame = this._frames[i];
                }

                window.runInMain(() => { frame.Resolve(frameData); });

                i++;
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

            if (this._isDone && this._frameIndex >= this._frameCount) {
                this._frameIndex = 0;
            }
        }

        public IPromise<FrameInfo> getNextFrame() {
            Promise<FrameData> frame;

            lock (this._frames) {
                if (this._frameIndex == this._frames.Count) {
                    this._frames.Add(new Promise<FrameData>());
                }
                else {
                    D.assert(this._frameIndex < this._frames.Count);
                }

                frame = this._frames[this._frameIndex];
            }


            return frame.Then(frameData => {
                this._nextFrame();

                if (frameData.frameInfo != null) {
                    return frameData.frameInfo;
                }

                var tex = new Texture2D(this._width, this._height, TextureFormat.BGRA32, false);
                tex.hideFlags = HideFlags.HideAndDontSave;
                tex.LoadRawTextureData(frameData.gifFrame.bytes);
                tex.Apply();

                frameData.frameInfo = new FrameInfo {
                    image = new Image(tex),
                    duration = TimeSpan.FromMilliseconds(frameData.gifFrame.delay)
                };
                frameData.gifFrame = null; // dispose gifFrame

                return frameData.frameInfo;
            });
        }

        public void Dispose() {
            this._coroutine.stop();
        }
    }
}