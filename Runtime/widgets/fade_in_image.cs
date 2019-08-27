using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.widgets {
    public class FadeInImage : StatefulWidget {
        public FadeInImage(
            ImageProvider placeholder,
            ImageProvider image,
            TimeSpan? fadeOutDuration = null,
            Curve fadeOutCurve = null,
            TimeSpan? fadeInDuration = null,
            Curve fadeInCurve = null,
            float? width = null,
            float? height = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Key key = null
        ) : base(key) {
            D.assert(placeholder != null);
            D.assert(image != null);
            D.assert(fadeOutDuration != null);
            D.assert(fadeOutCurve != null);
            D.assert(fadeInDuration != null);
            D.assert(fadeInCurve != null);
            D.assert(alignment != null);
            this.placeholder = placeholder;
            this.image = image;
            this.width = width;
            this.height = height;
            this.fit = fit;
            this.fadeOutDuration = fadeOutDuration ?? TimeSpan.FromMilliseconds(300);
            this.fadeOutCurve = fadeOutCurve ?? Curves.easeOut;
            this.fadeInDuration = fadeInDuration ?? TimeSpan.FromMilliseconds(700);
            this.fadeInCurve = fadeInCurve ?? Curves.easeIn;
            this.alignment = alignment ?? Alignment.center;
            this.repeat = repeat;
        }

        public static FadeInImage memoryNetwork(
            byte[] placeholder,
            string image,
            float placeholderScale = 1.0f,
            float imageScale = 1.0f,
            TimeSpan? fadeOutDuration = null,
            Curve fadeOutCurve = null,
            TimeSpan? fadeInDuration = null,
            Curve fadeInCurve = null,
            float? width = null,
            float? height = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Key key = null
        ) {
            D.assert(placeholder != null);
            D.assert(image != null);
            D.assert(fadeOutDuration != null);
            D.assert(fadeOutCurve != null);
            D.assert(fadeInDuration != null);
            D.assert(fadeInCurve != null);
            D.assert(alignment != null);
            var memoryImage = new MemoryImage(placeholder, placeholderScale);
            var networkImage = new NetworkImage(image, imageScale);
            return new FadeInImage(
                memoryImage,
                networkImage,
                fadeOutDuration,
                fadeOutCurve,
                fadeInDuration,
                fadeInCurve,
                width, height,
                fit,
                alignment,
                repeat,
                key
            );
        }

        public static FadeInImage assetNetwork(
            string placeholder,
            string image,
            AssetBundle bundle = null,
            float? placeholderScale = null,
            float imageScale = 1.0f,
            TimeSpan? fadeOutDuration = null,
            Curve fadeOutCurve = null,
            TimeSpan? fadeInDuration = null,
            Curve fadeInCurve = null,
            float? width = null,
            float? height = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Key key = null
        ) {
            D.assert(placeholder != null);
            D.assert(image != null);
            fadeOutDuration = fadeOutDuration ?? new TimeSpan(0, 0, 0, 0, 300);
            fadeOutCurve = fadeOutCurve ?? Curves.easeOut;
            fadeInDuration = fadeInDuration ?? new TimeSpan(0, 0, 0, 0, 700);
            fadeInCurve = Curves.easeIn;
            alignment = alignment ?? Alignment.center;
            var imageProvider = placeholderScale != null
                ? new ExactAssetImage(placeholder, bundle: bundle, scale: placeholderScale ?? 1.0f)
                : (ImageProvider) new AssetImage(placeholder, bundle: bundle);

            var networkImage = new NetworkImage(image, imageScale);
            return new FadeInImage(
                imageProvider,
                networkImage,
                fadeOutDuration,
                fadeOutCurve,
                fadeInDuration,
                fadeInCurve,
                width, height,
                fit,
                alignment,
                repeat,
                key
            );
        }

        public readonly ImageProvider placeholder;
        public readonly ImageProvider image;
        public readonly TimeSpan fadeOutDuration;
        public readonly Curve fadeOutCurve;
        public readonly TimeSpan fadeInDuration;
        public readonly Curve fadeInCurve;
        public readonly float? width;
        public readonly float? height;
        public readonly BoxFit? fit;
        public readonly Alignment alignment;
        public readonly ImageRepeat repeat;

        public override State createState() {
            return new _FadeInImageState();
        }
    }

    enum FadeInImagePhase {
        start,
        waiting,
        fadeOut,
        fadeIn,
        completed
    }

    delegate void _ImageProviderResolverListener();

    class _ImageProviderResolver {
        public _ImageProviderResolver(
            _FadeInImageState state,
            _ImageProviderResolverListener listener
        ) {
            this.state = state;
            this.listener = listener;
        }

        readonly _FadeInImageState state;
        readonly _ImageProviderResolverListener listener;

        FadeInImage widget {
            get { return this.state.widget; }
        }

        public ImageStream _imageStream;
        public ImageInfo _imageInfo;

        public void resolve(ImageProvider provider) {
            ImageStream oldImageStream = this._imageStream;
            Size size = null;
            if (this.widget.width != null && this.widget.height != null) {
                size = new Size((int) this.widget.width, (int) this.widget.height);
            }

            this._imageStream = provider.resolve(ImageUtils.createLocalImageConfiguration(this.state.context, size));
            D.assert(this._imageStream != null);

            if (this._imageStream.key != oldImageStream?.key) {
                oldImageStream?.removeListener(this._handleImageChanged);
                this._imageStream.addListener(this._handleImageChanged);
            }
        }

        void _handleImageChanged(ImageInfo imageInfo, bool synchronousCall) {
            this._imageInfo = imageInfo;
            this.listener();
        }

        public void stopListening() {
            this._imageStream?.removeListener(this._handleImageChanged);
        }
    }


    class _FadeInImageState : TickerProviderStateMixin<FadeInImage> {
        _ImageProviderResolver _imageResolver;
        _ImageProviderResolver _placeholderResolver;

        AnimationController _controller;
        Animation<float> _animation;

        FadeInImagePhase _phase = FadeInImagePhase.start;

        public override void initState() {
            this._imageResolver = new _ImageProviderResolver(state: this, this._updatePhase);
            this._placeholderResolver = new _ImageProviderResolver(state: this, listener: () => {
                this.setState(() => {
                    // Trigger rebuild to display the placeholder image
                });
            });
            this._controller = new AnimationController(
                value: 1.0f,
                vsync: this
            );
            this._controller.addListener(() => {
                this.setState(() => {
                    // Trigger rebuild to update opacity value.
                });
            });
            this._controller.addStatusListener(status => { this._updatePhase(); });
            base.initState();
        }

        public override void didChangeDependencies() {
            this._resolveImage();
            base.didChangeDependencies();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            FadeInImage fadeInImage = oldWidget as FadeInImage;
            if (this.widget.image != fadeInImage.image || this.widget.placeholder != fadeInImage.placeholder) {
                this._resolveImage();
            }
        }

        void _resolveImage() {
            this._imageResolver.resolve(this.widget.image);

            if (this._isShowingPlaceholder) {
                this._placeholderResolver.resolve(this.widget.placeholder);
            }

            if (this._phase == FadeInImagePhase.start) {
                this._updatePhase();
            }
        }

        void _updatePhase() {
            this.setState(() => {
                switch (this._phase) {
                    case FadeInImagePhase.start:
                        if (this._imageResolver._imageInfo != null) {
                            this._phase = FadeInImagePhase.completed;
                        }
                        else {
                            this._phase = FadeInImagePhase.waiting;
                        }

                        break;
                    case FadeInImagePhase.waiting:
                        if (this._imageResolver._imageInfo != null) {
                            this._controller.duration = this.widget.fadeOutDuration;
                            this._animation = new CurvedAnimation(
                                parent: this._controller,
                                curve: this.widget.fadeOutCurve
                            );
                            this._phase = FadeInImagePhase.fadeOut;
                            this._controller.reverse(1.0f);
                        }

                        break;
                    case FadeInImagePhase.fadeOut:
                        if (this._controller.status == AnimationStatus.dismissed) {
                            // Done fading out placeholder. Begin target image fade-in.
                            this._controller.duration = this.widget.fadeInDuration;
                            this._animation = new CurvedAnimation(
                                parent: this._controller,
                                curve: this.widget.fadeInCurve
                            );
                            this._phase = FadeInImagePhase.fadeIn;
                            this._placeholderResolver.stopListening();
                            this._controller.forward(0.0f);
                        }

                        break;
                    case FadeInImagePhase.fadeIn:
                        if (this._controller.status == AnimationStatus.completed) {
                            // Done finding in new image.
                            this._phase = FadeInImagePhase.completed;
                        }

                        break;
                    case FadeInImagePhase.completed:
                        // Nothing to do.
                        break;
                }
            });
        }

        public override void dispose() {
            this._imageResolver.stopListening();
            this._placeholderResolver.stopListening();
            this._controller.dispose();
            base.dispose();
        }

        bool _isShowingPlaceholder {
            get {
                switch (this._phase) {
                    case FadeInImagePhase.start:
                    case FadeInImagePhase.waiting:
                    case FadeInImagePhase.fadeOut:
                        return true;
                    case FadeInImagePhase.fadeIn:
                    case FadeInImagePhase.completed:
                        return false;
                }

                return true;
            }
        }

        ImageInfo _imageInfo {
            get {
                return this._isShowingPlaceholder
                    ? this._placeholderResolver._imageInfo
                    : this._imageResolver._imageInfo;
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(this._phase != FadeInImagePhase.start);
            ImageInfo imageInfo = this._imageInfo;
            return new RawImage(
                image: imageInfo?.image,
                width: this.widget.width,
                height: this.widget.height,
                scale: imageInfo?.scale ?? 1.0f,
                color: Color.fromRGBO(255, 255, 255, this._animation?.value ?? 1.0f),
                colorBlendMode: BlendMode.modulate,
                fit: this.widget.fit,
                alignment: this.widget.alignment,
                repeat: this.widget.repeat
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<FadeInImagePhase>("phase", this._phase));
            properties.add(new DiagnosticsProperty<ImageInfo>("pixels", this._imageInfo));
            properties.add(new DiagnosticsProperty<ImageStream>("image stream", this._imageResolver._imageStream));
            properties.add(new DiagnosticsProperty<ImageStream>("placeholder stream",
                this._placeholderResolver._imageStream));
        }
    }
}