using UIWidgets.ui;
using UIWidgets.painting;
using UnityEngine.Rendering;
using BlendMode = UIWidgets.ui.BlendMode;

namespace UIWidgets.rendering
{
    class RenderImage : RenderBox
    {
        public RenderImage(ui.Image image,
            double width,
            double height,
            Color color,
            ui.BlendMode colorBlendMode,
            BoxFit fit,
            ImageRepeat repeat,
            Rect centerSlice,
//            TextDirection textDirection,
            bool matchTextDirection = false,
            AlignmentGeometry alignment = null,
            double scale = 1.0
        )
        {
            this._image = image;
            this._width = width;
            this._height = height;
            this._scale = scale;
            this._color = color;
            this._colorBlendMode = colorBlendMode;
            this._fit = fit;
            this._repeat = repeat;
            this._centerSlice = centerSlice;
//            this._matchTextDirection = matchTextDirection;
//            this._textDir
            this._alignment = alignment ?? Alignment.center;
            this._textDirection = textDirection;
            _updateColorFilter();
        }

        Alignment _resolvedAlignment;
        bool _flipHorizontally;

        void _resolve()
        {
            if (_resolvedAlignment != null)
                return;
            _resolvedAlignment = alignment.resolve(textDirection);
            _flipHorizontally = matchTextDirection && textDirection == TextDirection.rtl;
        }

        void _markNeedsResolution()
        {
            _resolvedAlignment = null;
            _flipHorizontally = false;
            markNeedsPaint();
        }

        private ui.Image _image;

        public ui.Image image
        {
            get { return this._image; }
            set
            {
                if (value == _image)
                    return;
                _image = value;
                markNeedsPaint();
                if (_width == null || _height == null)
                    markNeedsLayout();
            }
        }

        private double _width;

        public double width
        {
            get { return _width; }
            set
            {
                if (value == _width)
                    return;
                _width = value;
                markNeedsLayout();
            }
        }

        private double _height;

        public double height
        {
            get { return _height; }
            set
            {
                if (value == _height)
                    return;
                _height = value;
                markNeedsLayout();
            }
        }

        private double _scale;

        public double scale
        {
            get { return _scale; }
            set
            {
                if (value == _scale)
                    return;
                _scale = value;
                markNeedsLayout();
            }
        }

        ColorFilter _colorFilter;

        void _updateColorFilter()
        {
            if (_color == null)
                _colorFilter = null;
            else
            {
                _colorFilter = new ColorFilter(_color,
                    _colorBlendMode == BlendMode.None ? BlendMode.srcIn : _colorBlendMode);
            }
        }

        private Color _color;

        public Color color
        {
            get { return _color; }
            set
            {
                if (value == _color)
                    return;
                _color = value;
                _updateColorFilter();
                markNeedsPaint();
            }
        }
        // todo more parameters

        private ui.BlendMode _colorBlendMode;

        public ui.BlendMode colorBlendMode
        {
            get { return _colorBlendMode; }
            set
            {
                if (value == _colorBlendMode)
                    return;
                _colorBlendMode = value;
                _updateColorFilter();
                markNeedsPaint();
            }
        }

        private BoxFit _fit;

        public BoxFit fit
        {
            get { return _fit; }
            set
            {
                if (value == _fit)
                    return;
                _fit = value;
                markNeedsPaint();
            }
        }

        private AlignmentGeometry _alignment;

        public AlignmentGeometry alignment
        {
            get { return _alignment; }
            set
            {
                if (value == _alignment)
                    return;
                _alignment = value;
                _markNeedsResolution();
            }
        }

        private ImageRepeat _repeat;

        public ImageRepeat repeat
        {
            get { return _repeat; }
            set
            {
                if (value == _repeat)
                    return;
                _repeat = value;
                markNeedsPaint();
            }
        }

        private Rect _centerSlice;

        public Rect centerSlice
        {
            get { return _centerSlice; }
            set
            {
                if (value == _centerSlice)
                    return;
                _centerSlice = value;
                markNeedsPaint();
            }
        }

        private bool _matchTextDirection;

        public bool matchTextDirection
        {
            get { return _matchTextDirection; }
            set
            {
                if (value == _matchTextDirection)
                    return;
                _matchTextDirection = value;
                _markNeedsResolution();
            }
        }

        private TextDirection _textDirection;

        public TextDirection textDirection
        {
            get { return _textDirection; }
            set
            {
                if (_textDirection == value)
                    return;
                _textDirection = value;
                _markNeedsResolution();
            }
        }

        Size _sizeForConstraints(BoxConstraints constraints)
        {
            // Folds the given |width| and |height| into |constraints| so they can all
            // be treated uniformly.
            constraints = BoxConstraints.tightFor(
                _width,
                _height
            );
            constraints = constraints.enforce(constraints);

            if (_image == null)
                return constraints.smallest;

            return constraints.constrainSizeAndAttemptToPreserveAspectRatio(new Size(
                _image.width / _scale,
                _image.height / _scale
            ));
        }

        public override void paint(PaintingContext context, Offset offset)
        {
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
                _repeat,
                _flipHorizontally
                // todo
            );
        }
    }
}