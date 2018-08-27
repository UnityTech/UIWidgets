using RSG;
using UIWidgets.ui;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UIWidgets.painting {
    public delegate void ImageListener(ImageInfo image, bool synchronousCall);

    public delegate void ImageErrorListerner(System.Object exception, string stackTrack);

    public class ImageInfo {
        public ImageInfo(Image image, double scale = 1.0) {
            this.image = image;
            this.scale = scale;
        }

        public Image image;
        public double scale;
    }

    public class ImageStream {
        public ImageStream() {
        }

        private ImageStreamCompleter _completer;
        private List<_ImageListenerPair> _listeners;

        public ImageStreamCompleter completer {
            get { return _completer; }
        }

        public void setCompleter(ImageStreamCompleter value) {
            _completer = value;
            if (_listeners != null) {
                List<_ImageListenerPair> initialListeners = _listeners;
                _listeners = null;
                foreach (_ImageListenerPair listenerPair in initialListeners) {
                    _completer.addListener(
                        listenerPair.listener,
                        listenerPair.errorListener
                    );
                }
            }
        }
    }

    public abstract class ImageStreamCompleter {
        public List<_ImageListenerPair> _listeners = new List<_ImageListenerPair>();
        public ImageInfo _currentImgae;

        public void addListener(ImageListener listener, ImageErrorListerner onError) {
            this._listeners.Add(new _ImageListenerPair(listener, onError));
            if (_currentImgae != null) {
                try {
                    listener(_currentImgae, true);
                    this.removeListener(listener);
                }
                catch (Exception e) {
                    Console.WriteLine("{0} Exception caught.", e);
                }
            }

            // todo call onError
        }

        public void removeListener(ImageListener listener) {
            var pairToRemove = this._listeners.Single(lp => lp.listener == listener);
            this._listeners.Remove(pairToRemove);
        }

        public void setImage(ImageInfo image) {
            _currentImgae = image;
            if (_listeners.Count == 0) {
                return;
            }

            foreach (var lp in _listeners.ToList()) {
                // todo refine
                var listener = lp.listener;
                try {
                    listener(image, false);
                    this.removeListener(listener);
                }
                catch (Exception e) {
                    Console.WriteLine("{0} Exception caught.", e);
                }

                // todo call onError
            }
        }
    }

    public class OneFrameImageStreamCompleter : ImageStreamCompleter {
        public OneFrameImageStreamCompleter(IPromise<ImageInfo> image) {
            image.Then(result => { setImage(result); }).Catch(err => { Debug.Log(err); });
        }
    }

    public class _ImageListenerPair {
        public _ImageListenerPair(ImageListener listener, ImageErrorListerner errorListener) {
            this.listener = listener;
            this.errorListener = errorListener;
        }

        public ImageListener listener;
        public ImageErrorListerner errorListener;
    }
}