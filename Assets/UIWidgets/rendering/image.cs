using UIWidgets.ui;
using UIWidgets.painting;
using UnityEngine;
using Color = UIWidgets.ui.Color;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering {
    class RenderImage : RenderBox {
        public RenderImage(ui.Image image,
            double width,
            double height,
            Color color,
            ui.BlendMode colorBlendMode,
            BoxFit fit,
            ImageRepeat repeat,
            Rect centerSlice,
            Alignment alignment = null,
            double scale = 1.0
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
        }

        Alignment _resolvedAlignment;

        void _resolve() {
            if (_resolvedAlignment != null)
                return;
            _resolvedAlignment = alignment;
        }

        void _markNeedsResolution() {
            _resolvedAlignment = null;
            markNeedsPaint();
        }

        private ui.Image _image;

        public ui.Image image {
            get { return this._image; }
            set {
                if (value == _image)
                    return;
                _image = value;
                markNeedsPaint();
                if (_width == 0.0 || _height == 0.0)
                    markNeedsLayout();
            }
        }

        private double _width;

        public double width {
            get { return _width; }
            set {
                if (value == _width)
                    return;
                _width = value;
                markNeedsLayout();
            }
        }

        private double _height;

        public double height {
            get { return _height; }
            set {
                if (value == _height)
                    return;
                _height = value;
                markNeedsLayout();
            }
        }

        private double _scale;

        public double scale {
            get { return _scale; }
            set {
                if (value == _scale)
                    return;
                _scale = value;
                markNeedsLayout();
            }
        }

        private Color _color;

        public Color color {
            get { return _color; }
            set {
                if (value == _color)
                    return;
                _color = value;
                markNeedsPaint();
            }
        }

        private ui.BlendMode _colorBlendMode;

        public ui.BlendMode colorBlendMode {
            get { return _colorBlendMode; }
            set {
                if (value == _colorBlendMode)
                    return;
                _colorBlendMode = value;
                markNeedsPaint();
            }
        }

        private BoxFit _fit;

        public BoxFit fit {
            get { return _fit; }
            set {
                if (value == _fit)
                    return;
                _fit = value;
                markNeedsPaint();
            }
        }

        private Alignment _alignment;

        public Alignment alignment {
            get { return _alignment; }
            set {
                if (value == _alignment)
                    return;
                _alignment = value;
                _markNeedsResolution();
            }
        }

        private ImageRepeat _repeat;

        public ImageRepeat repeat {
            get { return _repeat; }
            set {
                if (value == _repeat)
                    return;
                _repeat = value;
                markNeedsPaint();
            }
        }

        private Rect _centerSlice;

        public Rect centerSlice {
            get { return _centerSlice; }
            set {
                if (value == _centerSlice)
                    return;
                _centerSlice = value;
                markNeedsPaint();
            }
        }

        Size _sizeForConstraints(BoxConstraints constraints) {
            // Folds the given |width| and |height| into |constraints| so they can all
            // be treated uniformly.
            constraints = BoxConstraints.tightFor(
                _width,
                _height
            ).enforce(constraints);

            if (_image == null)
                return constraints.smallest;

            return constraints.constrainSizeAndAttemptToPreserveAspectRatio(new Size(
                _image.width / _scale,
                _image.height / _scale
            ));
        }
        
        protected override void performLayout() {
            this.size = _sizeForConstraints(constraints);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (_image == null)
                return;
            _resolve();
            DecorationImageUtil.paintImage(
                context.canvas,
                offset & size,
                _image,
                _fit,
                _centerSlice,
                _resolvedAlignment,
                _repeat
            );
        }
    }
}