using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    class CheckboxUtils {
        public const float _kEdgeSize = Checkbox.width;
        public static readonly Radius _kEdgeRadius = Radius.circular(1.0f);
        public const float _kStrokeWidth = 2.0f;
    }

    public class Checkbox : StatefulWidget {
        public Checkbox(
            Key key = null,
            bool? value = false,
            bool tristate = false,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color checkColor = null,
            MaterialTapTargetSize? materialTapTargetSize = null
        ) : base(key: key) {
            D.assert(tristate || value != null);
            this.value = value;
            this.onChanged = onChanged;
            this.activeColor = activeColor;
            this.checkColor = checkColor;
            this.tristate = tristate;
            this.materialTapTargetSize = materialTapTargetSize;
        }

        public readonly bool? value;

        public readonly ValueChanged<bool?> onChanged;

        public readonly Color activeColor;

        public readonly Color checkColor;

        public readonly bool tristate;

        public readonly MaterialTapTargetSize? materialTapTargetSize;

        public const float width = 18.0f;

        public override State createState() {
            return new _CheckboxState();
        }
    }

    class _CheckboxState : TickerProviderStateMixin<Checkbox> {
        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            ThemeData themeData = Theme.of(context);
            Size size;
            switch (this.widget.materialTapTargetSize ?? themeData.materialTapTargetSize) {
                case MaterialTapTargetSize.padded:
                    size = new Size(2 * Constants.kRadialReactionRadius + 8.0f,
                        2 * Constants.kRadialReactionRadius + 8.0f);
                    break;
                case MaterialTapTargetSize.shrinkWrap:
                    size = new Size(2 * Constants.kRadialReactionRadius, 2 * Constants.kRadialReactionRadius);
                    break;
                default:
                    throw new Exception("Unknown target size: " + this.widget.materialTapTargetSize);
            }

            BoxConstraints additionalConstraints = BoxConstraints.tight(size);
            return new _CheckboxRenderObjectWidget(
                value: this.widget.value,
                tristate: this.widget.tristate,
                activeColor: this.widget.activeColor ?? themeData.toggleableActiveColor,
                checkColor: this.widget.checkColor ?? new Color(0xFFFFFFFF),
                inactiveColor: this.widget.onChanged != null
                    ? themeData.unselectedWidgetColor
                    : themeData.disabledColor,
                onChanged: this.widget.onChanged,
                additionalConstraints: additionalConstraints,
                vsync: this
            );
        }
    }

    class _CheckboxRenderObjectWidget : LeafRenderObjectWidget {
        public _CheckboxRenderObjectWidget(
            Key key = null,
            bool? value = null,
            bool tristate = false,
            Color activeColor = null,
            Color checkColor = null,
            Color inactiveColor = null,
            ValueChanged<bool?> onChanged = null,
            TickerProvider vsync = null,
            BoxConstraints additionalConstraints = null
        ) : base(key: key) {
            D.assert(tristate || value != null);
            D.assert(activeColor != null);
            D.assert(inactiveColor != null);
            D.assert(vsync != null);
            this.value = value;
            this.tristate = tristate;
            this.activeColor = activeColor;
            this.checkColor = checkColor;
            this.inactiveColor = inactiveColor;
            this.onChanged = onChanged;
            this.vsync = vsync;
            this.additionalConstraints = additionalConstraints;
        }

        public readonly bool? value;
        public readonly bool tristate;
        public readonly Color activeColor;
        public readonly Color checkColor;
        public readonly Color inactiveColor;
        public readonly ValueChanged<bool?> onChanged;
        public readonly TickerProvider vsync;
        public readonly BoxConstraints additionalConstraints;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCheckbox(
                value: this.value,
                tristate: this.tristate,
                activeColor: this.activeColor,
                checkColor: this.checkColor,
                inactiveColor: this.inactiveColor,
                onChanged: this.onChanged,
                vsync: this.vsync,
                additionalConstraints: this.additionalConstraints
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderCheckbox renderObject = _renderObject as _RenderCheckbox;
            renderObject.value = this.value;
            renderObject.tristate = this.tristate;
            renderObject.activeColor = this.activeColor;
            renderObject.checkColor = this.checkColor;
            renderObject.inactiveColor = this.inactiveColor;
            renderObject.onChanged = this.onChanged;
            renderObject.additionalConstraints = this.additionalConstraints;
            renderObject.vsync = this.vsync;
        }
    }


    class _RenderCheckbox : RenderToggleable {
        public _RenderCheckbox(
            bool? value = null,
            bool tristate = false,
            Color activeColor = null,
            Color checkColor = null,
            Color inactiveColor = null,
            BoxConstraints additionalConstraints = null,
            ValueChanged<bool?> onChanged = null,
            TickerProvider vsync = null
        ) : base(
                value: value,
                tristate: tristate,
                activeColor: activeColor,
                inactiveColor: inactiveColor,
                onChanged: onChanged,
                additionalConstraints: additionalConstraints,
                vsync: vsync
            ) {
            this._oldValue = value;
            this.checkColor = checkColor;
        }

        bool? _oldValue;
        public Color checkColor;

        public override bool? value {
            set {
                if (value == this.value) {
                    return;
                }

                this._oldValue = this.value;
                base.value = value;
            }
        }

        RRect _outerRectAt(Offset origin, float t) {
            float inset = 1.0f - (t - 0.5f).abs() * 2.0f;
            float size = CheckboxUtils._kEdgeSize - inset * CheckboxUtils._kStrokeWidth;
            Rect rect = Rect.fromLTWH(origin.dx + inset, origin.dy + inset, size, size);
            return RRect.fromRectAndRadius(rect, CheckboxUtils._kEdgeRadius);
        }

        Color _colorAt(float t) {
            return this.onChanged == null
                ? this.inactiveColor
                : (t >= 0.25f ? this.activeColor : Color.lerp(this.inactiveColor, this.activeColor, t * 4.0f));
        }

        void _initStrokePaint(Paint paint) {
            paint.color = this.checkColor;
            paint.style = PaintingStyle.stroke;
            paint.strokeWidth = CheckboxUtils._kStrokeWidth;
        }

        void _drawBorder(Canvas canvas, RRect outer, float t, Paint paint) {
            D.assert(t >= 0.0f && t <= 0.5f);
            float size = outer.width;
            RRect inner = outer.deflate(Mathf.Min(size / 2.0f, CheckboxUtils._kStrokeWidth + size * t));
            canvas.drawDRRect(outer, inner, paint);
        }

        void _drawCheck(Canvas canvas, Offset origin, float t, Paint paint) {
            D.assert(t >= 0.0f && t <= 1.0f);
            Path path = new Path();
            Offset start = new Offset(CheckboxUtils._kEdgeSize * 0.15f, CheckboxUtils._kEdgeSize * 0.45f);
            Offset mid = new Offset(CheckboxUtils._kEdgeSize * 0.4f, CheckboxUtils._kEdgeSize * 0.7f);
            Offset end = new Offset(CheckboxUtils._kEdgeSize * 0.85f, CheckboxUtils._kEdgeSize * 0.25f);
            if (t < 0.5f) {
                float strokeT = t * 2.0f;
                Offset drawMid = Offset.lerp(start, mid, strokeT);
                path.moveTo(origin.dx + start.dx, origin.dy + start.dy);
                path.lineTo(origin.dx + drawMid.dx, origin.dy + drawMid.dy);
            }
            else {
                float strokeT = (t - 0.5f) * 2.0f;
                Offset drawEnd = Offset.lerp(mid, end, strokeT);
                path.moveTo(origin.dx + start.dx, origin.dy + start.dy);
                path.lineTo(origin.dx + mid.dx, origin.dy + mid.dy);
                path.lineTo(origin.dx + drawEnd.dx, origin.dy + drawEnd.dy);
            }

            canvas.drawPath(path, paint);
        }

        void _drawDash(Canvas canvas, Offset origin, float t, Paint paint) {
            D.assert(t >= 0.0f && t <= 1.0f);
            Offset start = new Offset(CheckboxUtils._kEdgeSize * 0.2f, CheckboxUtils._kEdgeSize * 0.5f);
            Offset mid = new Offset(CheckboxUtils._kEdgeSize * 0.5f, CheckboxUtils._kEdgeSize * 0.5f);
            Offset end = new Offset(CheckboxUtils._kEdgeSize * 0.8f, CheckboxUtils._kEdgeSize * 0.5f);
            Offset drawStart = Offset.lerp(start, mid, 1.0f - t);
            Offset drawEnd = Offset.lerp(mid, end, t);
            canvas.drawLine(origin + drawStart, origin + drawEnd, paint);
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;
            this.paintRadialReaction(canvas, offset, this.size.center(Offset.zero));

            Offset origin = offset + (this.size / 2.0f - Size.square(CheckboxUtils._kEdgeSize) / 2.0f);
            AnimationStatus status = this.position.status;
            float tNormalized = status == AnimationStatus.forward || status == AnimationStatus.completed
                ? this.position.value
                : 1.0f - this.position.value;

            if (this._oldValue == false || this.value == false) {
                float t = this.value == false ? 1.0f - tNormalized : tNormalized;
                RRect outer = this._outerRectAt(origin, t);
                Paint paint = new Paint();
                paint.color = this._colorAt(t);

                if (t <= 0.5f) {
                    this._drawBorder(canvas, outer, t, paint);
                }
                else {
                    canvas.drawRRect(outer, paint);

                    this._initStrokePaint(paint);
                    float tShrink = (t - 0.5f) * 2.0f;
                    if (this._oldValue == null || this.value == null) {
                        this._drawDash(canvas, origin, tShrink, paint);
                    }
                    else {
                        this._drawCheck(canvas, origin, tShrink, paint);
                    }
                }
            }
            else {
                // Two cases: null to true, true to null
                RRect outer = this._outerRectAt(origin, 1.0f);
                Paint paint = new Paint();
                paint.color = this._colorAt(1.0f);
                canvas.drawRRect(outer, paint);

                this._initStrokePaint(paint);
                if (tNormalized <= 0.5f) {
                    float tShrink = 1.0f - tNormalized * 2.0f;
                    if (this._oldValue == true) {
                        this._drawCheck(canvas, origin, tShrink, paint);
                    }
                    else {
                        this._drawDash(canvas, origin, tShrink, paint);
                    }
                }
                else {
                    float tExpand = (tNormalized - 0.5f) * 2.0f;
                    if (this.value == true) {
                        this._drawCheck(canvas, origin, tExpand, paint);
                    }
                    else {
                        this._drawDash(canvas, origin, tExpand, paint);
                    }
                }
            }
        }
    }
}