using System;
using System.Collections.Generic;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.widgets {
    internal class ImageUtil {
        public static ImageConfiguration createLocalImageConfiguration(BuildContext context, Size size = null) {
            return new ImageConfiguration(
                size: size
            );
        }
    }

    public class Image : StatefulWidget {
        public ImageProvider image;
        public double? width;
        public double? height;
        public Color color;
        public BoxFit fit;
        public Alignment alignment;
        public BlendMode colorBlendMode;
        public ImageRepeat repeat;
        public ui.Rect centerSlice;

        public bool gaplessPlayback;

        public Image(
            Key key,
            ImageProvider image,
            Color color,
            BlendMode colorBlendMode,
            BoxFit fit,
            ui.Rect centerSlice,
            Alignment alignment,
            double? width = null,
            double? height = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            bool gaplessPlayback = false
        ) : base(key) {
            this.image = image;
            this.width = width;
            this.height = height;
            this.color = color;
            this.colorBlendMode = colorBlendMode;
            this.fit = fit;
            this.alignment = alignment == null ? Alignment.center : alignment;
            this.repeat = repeat;
            this.centerSlice = centerSlice;
            this.gaplessPlayback = gaplessPlayback;
        }

        public static Image network(
            string src,
            Key key = null,
            double? width = null,
            double? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcOver,
            BoxFit fit = BoxFit.none,
            Alignment alignment = null,
            ui.Rect centerSlice = null,
            Dictionary<String, String> headers = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            bool gaplessPlayback = false,
            double scale = 1.0
        ) {
            var networkImage = new NetworkImage(src, headers, scale);
            return new Image(
                key,
                networkImage,
                color,
                colorBlendMode,
                fit,
                centerSlice,
                alignment,
                width,
                height,
                repeat,
                gaplessPlayback
            );
        }

        public static Image file(
            string path,
            Key key = null,
            double? width = null,
            double? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcOver,
            BoxFit fit = BoxFit.none,
            Alignment alignment = null,
            ui.Rect centerSlice = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            bool gaplessPlayback = false,
            double scale = 1.0
        ) {
            var fileImage = new FileImage(path, scale);
            return new Image(
                key,
                fileImage,
                color,
                colorBlendMode,
                fit,
                centerSlice,
                alignment,
                width,
                height,
                repeat,
                gaplessPlayback
            );
        }

        public override State createState() {
            return new _ImageState();
        }
    }

    public class _ImageState : State {
        ImageStream _imageStream;
        ImageInfo _imageInfo;
        bool _isListeningToStream = false;

        public override void didChangeDependencies() {
            _resolveImage();
            if (TickerMode.of(context))
                _listenToStream();
            else
                _stopListeningToStream();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            if (((Image) widget).image != ((Image) oldWidget).image)
                _resolveImage();
        }

//        public override void reassemble() {
//            _resolveImage(); // in case the image cache was flushed
//        }

        void _resolveImage() {
            var imageWidget = (Image) widget;
            ImageStream newStream =
                imageWidget.image.resolve(ImageUtil.createLocalImageConfiguration(
                    context,
                    size: imageWidget.width != null && imageWidget.height != null
                        ? new Size(imageWidget.width.Value, imageWidget.height.Value)
                        : null
                ));
            _updateSourceStream(newStream);
        }

        void _handleImageChanged(ImageInfo imageInfo, bool synchronousCall) {
            setState(() => { _imageInfo = imageInfo; });
        }

        void _updateSourceStream(ImageStream newStream) {
            if ((_imageStream == null ? null : _imageStream.key) == (newStream == null ? null : newStream.key))
                return;

            if (_isListeningToStream && _imageStream != null)
                _imageStream.removeListener(_handleImageChanged);

            if (!((Image) widget).gaplessPlayback) {
                setState(() => { _imageInfo = null; });

                _imageStream = newStream;
                if (_isListeningToStream && _imageStream != null)
                    _imageStream.addListener(_handleImageChanged);
            }
        }

        void _listenToStream() {
            if (_isListeningToStream)
                return;
            _imageStream.addListener(_handleImageChanged);
            _isListeningToStream = true;
        }

        void _stopListeningToStream() {
            if (!_isListeningToStream)
                return;
            _imageStream.removeListener(_handleImageChanged);
            _isListeningToStream = false;
        }

        public override Widget build(BuildContext context) {
            var imageWidget = (Image) widget;
            RawImage image = new RawImage(
                null,
                _imageInfo == null ? null : _imageInfo.image,
                _imageInfo == null ? 1.0 : _imageInfo.scale,
                imageWidget.color,
                imageWidget.colorBlendMode,
                imageWidget.fit,
                imageWidget.centerSlice,
                imageWidget.width,
                imageWidget.height,
                imageWidget.alignment,
                imageWidget.repeat
            );

            return image;
        }
    }
}