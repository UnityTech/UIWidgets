using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {
    public class ImageUtils {
        public static ImageConfiguration createLocalImageConfiguration(BuildContext context, Size size = null) {
            return new ImageConfiguration(
                bundle: DefaultAssetBundle.of(context),
                devicePixelRatio: MediaQuery.of(context, nullOk: true)?.devicePixelRatio ?? 1.0f,
                //locale: Localizations.localeOf(context, nullOk: true),
                size: size,
                platform: Application.platform
            );
        }

        public IPromise precacheImage(
            ImageProvider provider,
            BuildContext context,
            Size size = null,
            ImageErrorListener onError = null
        ) {
            ImageConfiguration config = createLocalImageConfiguration(context, size: size);
            var completer = new Promise();
            ImageStream stream = provider.resolve(config);

            void listener(ImageInfo image, bool sync) {
                completer.Resolve();
                stream.removeListener(listener);
            }

            void errorListener(Exception exception) {
                completer.Resolve();
                stream.removeListener(listener);
                if (onError != null) {
                    onError(exception);
                }
                else {
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        context: "image failed to precache",
                        library: "image resource service",
                        exception: exception,
                        silent: true
                    ));
                }
            }

            stream.addListener(listener, onError: errorListener);
            return completer;
        }
    }

    public class Image : StatefulWidget {
        public Image(
            Key key = null,
            ImageProvider image = null,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterMode filterMode = FilterMode.Bilinear
        ) : base(key) {
            D.assert(image != null);
            this.image = image;
            this.width = width;
            this.height = height;
            this.color = color;
            this.colorBlendMode = colorBlendMode;
            this.fit = fit;
            this.alignment = alignment ?? Alignment.center;
            this.repeat = repeat;
            this.centerSlice = centerSlice;
            this.gaplessPlayback = gaplessPlayback;
            this.filterMode = filterMode;
        }

        public static Image network(
            string src,
            Key key = null,
            float scale = 1.0f,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterMode filterMode = FilterMode.Bilinear,
            IDictionary<string, string> headers = null
        ) {
            var networkImage = new NetworkImage(src, scale, headers);
            return new Image(
                key,
                networkImage,
                width,
                height,
                color,
                colorBlendMode,
                fit,
                alignment,
                repeat,
                centerSlice,
                gaplessPlayback,
                filterMode
            );
        }

        public static Image file(
            string file,
            Key key = null,
            float scale = 1.0f,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterMode filterMode = FilterMode.Bilinear
        ) {
            var fileImage = new FileImage(file, scale);
            return new Image(
                key,
                fileImage,
                width,
                height,
                color,
                colorBlendMode,
                fit,
                alignment,
                repeat,
                centerSlice,
                gaplessPlayback,
                filterMode
            );
        }

        public static Image asset(
            string name,
            Key key = null,
            AssetBundle bundle = null,
            float? scale = null,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterMode filterMode = FilterMode.Bilinear
        ) {
            var image = scale != null
                ? (AssetBundleImageProvider) new ExactAssetImage(name, bundle: bundle, scale: scale.Value)
                : new AssetImage(name, bundle: bundle);

            return new Image(
                key,
                image,
                width,
                height,
                color,
                colorBlendMode,
                fit,
                alignment,
                repeat,
                centerSlice,
                gaplessPlayback,
                filterMode
            );
        }

        public static Image memory(
            byte[] bytes,
            Key key = null,
            float scale = 1.0f,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterMode filterMode = FilterMode.Bilinear
        ) {
            var memoryImage = new MemoryImage(bytes, scale);
            return new Image(
                key,
                memoryImage,
                width,
                height,
                color,
                colorBlendMode,
                fit,
                alignment,
                repeat,
                centerSlice,
                gaplessPlayback,
                filterMode
            );
        }

        public readonly ImageProvider image;
        public readonly float? width;
        public readonly float? height;
        public readonly Color color;
        public readonly FilterMode filterMode;
        public readonly BlendMode colorBlendMode;
        public readonly BoxFit? fit;
        public readonly Alignment alignment;
        public readonly ImageRepeat repeat;
        public readonly Rect centerSlice;
        public readonly bool gaplessPlayback;

        public override State createState() {
            return new _ImageState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);

            properties.add(new DiagnosticsProperty<ImageProvider>("image", this.image));
            properties.add(new FloatProperty("width", this.width, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new FloatProperty("height", this.height, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Color>("color", this.color,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumProperty<BlendMode>("colorBlendMode", this.colorBlendMode,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumProperty<BoxFit?>("fit", this.fit, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumProperty<ImageRepeat>("repeat", this.repeat, defaultValue: ImageRepeat.noRepeat));
            properties.add(new DiagnosticsProperty<Rect>("centerSlice", this.centerSlice,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumProperty<FilterMode>("filterMode", this.filterMode, Diagnostics.kNullDefaultValue));
        }
    }

    public class _ImageState : State<Image> {
        ImageStream _imageStream;
        ImageInfo _imageInfo;
        bool _isListeningToStream = false;
        bool _invertColors;

        public override void didChangeDependencies() {
            this._invertColors = false;

            this._resolveImage();

            if (TickerMode.of(this.context)) {
                this._listenToStream();
            }
            else {
                this._stopListeningToStream();
            }

            base.didChangeDependencies();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);

            if (this.widget.image != ((Image) oldWidget).image) {
                this._resolveImage();
            }
        }

        void _resolveImage() {
            ImageStream newStream =
                this.widget.image.resolve(ImageUtils.createLocalImageConfiguration(
                    this.context,
                    size: this.widget.width != null && this.widget.height != null
                        ? new Size(this.widget.width.Value, this.widget.height.Value)
                        : null
                ));
            D.assert(newStream != null);
            this._updateSourceStream(newStream);
        }

        void _handleImageChanged(ImageInfo imageInfo, bool synchronousCall) {
            this.setState(() => { this._imageInfo = imageInfo; });
        }

        void _updateSourceStream(ImageStream newStream) {
            if (this._imageStream?.key == newStream?.key) {
                return;
            }

            if (this._isListeningToStream) {
                this._imageStream.removeListener(this._handleImageChanged);
            }

            if (!this.widget.gaplessPlayback) {
                this.setState(() => { this._imageInfo = null; });
            }

            this._imageStream = newStream;
            if (this._isListeningToStream) {
                this._imageStream.addListener(this._handleImageChanged);
            }
        }

        void _listenToStream() {
            if (this._isListeningToStream) {
                return;
            }

            this._imageStream.addListener(this._handleImageChanged);
            this._isListeningToStream = true;
        }

        void _stopListeningToStream() {
            if (!this._isListeningToStream) {
                return;
            }

            this._imageStream.removeListener(this._handleImageChanged);
            this._isListeningToStream = false;
        }

        public override void dispose() {
            D.assert(this._imageStream != null);
            this._stopListeningToStream();
            base.dispose();
        }


        public override Widget build(BuildContext context) {
            RawImage image = new RawImage(
                image: this._imageInfo?.image,
                width: this.widget.width,
                height: this.widget.height,
                scale: this._imageInfo?.scale ?? 1.0f,
                color: this.widget.color,
                colorBlendMode: this.widget.colorBlendMode,
                fit: this.widget.fit,
                alignment: this.widget.alignment,
                repeat: this.widget.repeat,
                centerSlice: this.widget.centerSlice,
                invertColors: this._invertColors,
                filterMode: this.widget.filterMode
            );

            return image;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<ImageStream>("stream", this._imageStream));
            description.add(new DiagnosticsProperty<ImageInfo>("pixels", this._imageInfo));
        }
    }
}