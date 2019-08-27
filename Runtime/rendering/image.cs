using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    class RenderImage : RenderBox {
        public RenderImage(
            Image image = null,
            float? width = null,
            float? height = null,
            float scale = 1.0f,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool invertColors = false,
            FilterMode filterMode = FilterMode.Bilinear
        ) {
            this._image = image;
            this._width = width;
            this._height = height;
            this._scale = scale;
            this._color = color;
            this._colorBlendMode = colorBlendMode;
            this._fit = fit;
            this._repeat = repeat;
            this._centerSlice = centerSlice;
            this._alignment = alignment ?? Alignment.center;
            this._invertColors = invertColors;
            this._filterMode = filterMode;
            this._updateColorFilter();
        }

        Image _image;

        public Image image {
            get { return this._image; }
            set {
                if (value == this._image) {
                    return;
                }

                this._image = value;
                this.markNeedsPaint();
                if (this._width == null || this._height == null) {
                    this.markNeedsLayout();
                }
            }
        }

        float? _width;

        public float? width {
            get { return this._width; }
            set {
                if (value == this._width) {
                    return;
                }

                this._width = value;
                this.markNeedsLayout();
            }
        }

        float? _height;

        public float? height {
            get { return this._height; }
            set {
                if (value == this._height) {
                    return;
                }

                this._height = value;
                this.markNeedsLayout();
            }
        }

        float _scale;

        public float scale {
            get { return this._scale; }
            set {
                if (value == this._scale) {
                    return;
                }

                this._scale = value;
                this.markNeedsLayout();
            }
        }
        
        
        ColorFilter _colorFilter;

        void _updateColorFilter() {
            if (this._color == null) {
                this._colorFilter = null;
            } else {
                this._colorFilter = ColorFilter.mode(this._color, this._colorBlendMode);
            }
        }

        Color _color;

        public Color color {
            get { return this._color; }
            set {
                if (value == this._color) {
                    return;
                }

                this._color = value;
                this._updateColorFilter();
                this.markNeedsPaint();
            }
        }

        BlendMode _colorBlendMode;

        public BlendMode colorBlendMode {
            get { return this._colorBlendMode; }
            set {
                if (value == this._colorBlendMode) {
                    return;
                }

                this._colorBlendMode = value;
                this._updateColorFilter();
                this.markNeedsPaint();
            }
        }

        FilterMode _filterMode;

        public FilterMode filterMode {
            get { return this._filterMode; }
            set {
                if (value == this._filterMode) {
                    return;
                }

                this._filterMode = value;
                this.markNeedsPaint();
            }
        }

        BoxFit? _fit;

        public BoxFit? fit {
            get { return this._fit; }
            set {
                if (value == this._fit) {
                    return;
                }

                this._fit = value;
                this.markNeedsPaint();
            }
        }

        Alignment _alignment;

        public Alignment alignment {
            get { return this._alignment; }
            set {
                if (value == this._alignment) {
                    return;
                }

                this._alignment = value;
                this.markNeedsPaint();
            }
        }

        ImageRepeat _repeat;

        public ImageRepeat repeat {
            get { return this._repeat; }
            set {
                if (value == this._repeat) {
                    return;
                }

                this._repeat = value;
                this.markNeedsPaint();
            }
        }

        Rect _centerSlice;

        public Rect centerSlice {
            get { return this._centerSlice; }
            set {
                if (value == this._centerSlice) {
                    return;
                }

                this._centerSlice = value;
                this.markNeedsPaint();
            }
        }

        bool _invertColors;

        public bool invertColors {
            get { return this._invertColors; }
            set {
                if (value == this._invertColors) {
                    return;
                }

                this._invertColors = value;
                this.markNeedsPaint();
            }
        }

        Size _sizeForConstraints(BoxConstraints constraints) {
            constraints = BoxConstraints.tightFor(
                this._width,
                this._height
            ).enforce(constraints);

            if (this._image == null) {
                return constraints.smallest;
            }

            return constraints.constrainSizeAndAttemptToPreserveAspectRatio(new Size(
                (this._image.width / this._scale),
                (this._image.height / this._scale)
            ));
        }

        protected override float computeMinIntrinsicWidth(float height) {
            D.assert(height >= 0.0);
            if (this._width == null && this._height == null) {
                return 0.0f;
            }

            return this._sizeForConstraints(BoxConstraints.tightForFinite(height: height)).width;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            D.assert(height >= 0.0);
            return this._sizeForConstraints(BoxConstraints.tightForFinite(height: height)).width;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            D.assert(width >= 0.0);
            if (this._width == null && this._height == null) {
                return 0.0f;
            }

            return this._sizeForConstraints(BoxConstraints.tightForFinite(width: width)).height;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            D.assert(width >= 0.0);
            return this._sizeForConstraints(BoxConstraints.tightForFinite(width: width)).height;
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        protected override void performLayout() {
            this.size = this._sizeForConstraints(this.constraints);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._image == null) {
                return;
            }

            ImageUtils.paintImage(
                canvas: context.canvas,
                rect: offset & this.size,
                image: this._image,
                scale: this._scale,
                colorFilter: this._colorFilter,
                fit: this._fit,
                alignment: this._alignment,
                centerSlice: this._centerSlice,
                repeat: this._repeat,
                invertColors: this._invertColors,
                filterMode: this._filterMode
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Image>("image", this.image));
            properties.add(new FloatProperty("width", this.width, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new FloatProperty("height", this.height, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new FloatProperty("scale", this.scale, defaultValue: 1.0f));
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
            properties.add(new DiagnosticsProperty<bool>("invertColors", this.invertColors));
            properties.add(new EnumProperty<FilterMode>("filterMode", this.filterMode));
        }
    }
}