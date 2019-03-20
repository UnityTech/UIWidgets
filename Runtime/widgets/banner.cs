using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    class BannerConstants {
        public const float _kOffset = 40.0f;
        public const float _kHeight = 12.0f;
        public const float _kBottomOffset = _kOffset + 0.707f * _kHeight;
        public static readonly Rect _kRect = Rect.fromLTWH(-_kOffset, _kOffset - _kHeight, _kOffset * 2.0f, _kHeight);

        public static readonly Color _kColor = new Color(0xA0B71C1C);

        public static readonly TextStyle _kTextStyle = new TextStyle(
            color: new Color(0xFFFFFFFF),
            fontSize: _kHeight * 0.85f,
            fontWeight: FontWeight.w700,
            height: 1.0f
        );
    }

    public enum BannerLocation {
        topStart,

        topEnd,

        bottomStart,

        bottomEnd,
    }

    public class BannerPainter : AbstractCustomPainter {
        public BannerPainter(
            string message,
            BannerLocation? location,
            Color color = null,
            TextStyle textStyle = null
        ) {
            D.assert(message != null);
            D.assert(location != null);
            this.color = color ?? BannerConstants._kColor;
            this.message = message;
            this.location = location;
            this.textStyle = textStyle ?? BannerConstants._kTextStyle;
        }

        public readonly string message;

        public readonly BannerLocation? location;

        public readonly Color color;

        public readonly TextStyle textStyle;

        readonly BoxShadow _shadow = new BoxShadow(
            color: new Color(0x7F000000),
            blurRadius: 6.0f
        );

        bool _prepared = false;
        TextPainter _textPainter;
        Paint _paintShadow;
        Paint _paintBanner;

        void _prepare() {
            this._paintShadow = this._shadow.toPaint();
            this._paintBanner = new Paint();
            this._paintBanner.color = this.color;
            this._textPainter = new TextPainter(
                text: new TextSpan(style: this.textStyle, text: this.message),
                textAlign: TextAlign.center
            );
            this._prepared = true;
        }

        public override void paint(Canvas canvas, Size size) {
            if (!this._prepared) {
                this._prepare();
            }

            canvas.translate(this._translationX(size.width), this._translationY(size.height));
            canvas.rotate(this._rotation);
            canvas.drawRect(BannerConstants._kRect, this._paintShadow);
            canvas.drawRect(BannerConstants._kRect, this._paintBanner);
            const float width = BannerConstants._kOffset * 2.0f;
            this._textPainter.layout(minWidth: width, maxWidth: width);
            this._textPainter.paint(canvas,
                BannerConstants._kRect.topLeft + new Offset(0.0f,
                    (BannerConstants._kRect.height - this._textPainter.height) / 2.0f));
        }

        public override bool shouldRepaint(CustomPainter _oldDelegate) {
            BannerPainter oldDelegate = _oldDelegate as BannerPainter;
            return this.message != oldDelegate.message
                   || this.location != oldDelegate.location
                   || this.color != oldDelegate.color
                   || this.textStyle != oldDelegate.textStyle;
        }

        public override bool? hitTest(Offset position) {
            return false;
        }

        float _translationX(float width) {
            switch (this.location) {
                case BannerLocation.bottomEnd:
                    return width - BannerConstants._kBottomOffset;
                case BannerLocation.topEnd:
                    return width;
                case BannerLocation.bottomStart:
                    return BannerConstants._kBottomOffset;
                case BannerLocation.topStart:
                    return 0.0f;
                default:
                    throw new Exception("Unknown location: " + this.location);
            }
        }

        float _translationY(float height) {
            D.assert(this.location != null);
            switch (this.location) {
                case BannerLocation.bottomStart:
                case BannerLocation.bottomEnd:
                    return height - BannerConstants._kBottomOffset;
                case BannerLocation.topStart:
                case BannerLocation.topEnd:
                    return 0.0f;
                default:
                    throw new Exception("Unknown location: " + this.location);
            }
        }

        float _rotation {
            get {
                switch (this.location) {
                    case BannerLocation.bottomStart:
                    case BannerLocation.topEnd:
                        return Mathf.PI / 4.0f;
                    case BannerLocation.bottomEnd:
                    case BannerLocation.topStart:
                        return -Mathf.PI / 4.0f;
                    default:
                        throw new Exception("Unknown location: " + this.location);
                }
            }
        }
    }

    public class Banner : StatelessWidget {
        public Banner(
            Key key = null,
            Widget child = null,
            string message = null,
            BannerLocation? location = null,
            Color color = null,
            TextStyle textStyle = null
        ) : base(key: key) {
            D.assert(message != null);
            this.child = child;
            this.message = message;
            this.location = location;
            this.color = color ?? BannerConstants._kColor;
            this.textStyle = textStyle ?? BannerConstants._kTextStyle;
        }

        public readonly Widget child;

        public readonly string message;

        public readonly BannerLocation? location;

        public readonly Color color;

        public readonly TextStyle textStyle;

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            return new CustomPaint(
                foregroundPainter: new BannerPainter(
                    message: this.message,
                    location: this.location,
                    color: this.color,
                    textStyle: this.textStyle
                ),
                child: this.child
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("message", this.message, showName: false));
            properties.add(new EnumProperty<BannerLocation?>("location", this.location));
            properties.add(new DiagnosticsProperty<Color>("color", this.color, showName: false));
            this.textStyle?.debugFillProperties(properties);
        }
    }

    public class CheckedModeBanner : StatelessWidget {
        public CheckedModeBanner(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(child != null);
            this.child = child;
        }


        public readonly Widget child;

        public override Widget build(BuildContext context) {
            Widget result = this.child;
            D.assert(() => {
                result = new Banner(
                    child: result,
                    message: "DEBUG",
                    location: BannerLocation.topEnd
                );
                return true;
            });
            return result;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            string message = "disabled";
            D.assert(() => {
                message = "'DEBUG'";
                return true;
            });
            properties.add(DiagnosticsNode.message(message));
        }
    }
}