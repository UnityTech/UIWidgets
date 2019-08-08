using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.flow {
    public class PhysicalShapeLayer : ContainerLayer {
        public PhysicalShapeLayer(
            Clip clipBehavior) {
            this._isRect = false;
            this._clip_behavior = clipBehavior;
        }

        float _elevation;
        Color _color;
        Color _shadow_color;
        float _device_pixel_ratio;
        Path _path;
#pragma warning disable 0414
        bool _isRect;
#pragma warning restore 0414
        Rect _frameRRect;
        Clip _clip_behavior;

        public Path path {
            set {
                //todo: xingwei.zhu : try to do path => rect transfer
                this._path = value;
                this._isRect = false;
                this._frameRRect = value.getBounds();
            }
        }

        public float elevation {
            set { this._elevation = value; }
        }

        public Color color {
            set { this._color = value; }
        }

        public Color shadowColor {
            set { this._shadow_color = value; }
        }

        public float devicePixelRatio {
            set { this._device_pixel_ratio = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            Rect child_paint_bounds = Rect.zero;
            this.prerollChildren(context, matrix, ref child_paint_bounds);

            if (this._elevation == 0) {
                this.paintBounds = this._path.getBounds();
            }
            else {
                Rect bounds = this._path.getBounds();
                Rect outset = bounds.outset(20.0f, 20.0f);
                this.paintBounds = outset;
            }
        }

        Paint _shadowPaint = new Paint();

        public override void paint(PaintContext context) {
            if (this._elevation != 0) {
                drawShadow(context.canvas, this._path, this._shadow_color, this._elevation,
                    this._color.alpha != 255, this._device_pixel_ratio);
            }

            this._shadowPaint.color = this._color;
            context.canvas.drawPath(this._path, this._shadowPaint);

            context.canvas.save();
            context.canvas.clipPath(this._path);
            this.paintChildren(context);
            context.canvas.restore();
        }

        const float kAmbientAlpha = 0.039f;
        const float kLightHeight = 600f;
        const float kLightRadius = 800f;
        const float kSpotAlpha = 0.25f;

        public static void drawShadow(Canvas canvas, Path path, Color color, float elevation, bool transparentOccluder,
            float dpr) {
            Rect bounds = path.getBounds();
            float shadow_x = (bounds.left + bounds.right) / 2f;
            float shadow_y = bounds.top - 600.0f;

            uiColor uicolor = uiColor.fromColor(color);

            uiColor inAmbient = uicolor.withAlpha((int) (kAmbientAlpha * uicolor.alpha));
            uiColor inSpot = uicolor.withAlpha((int) (kSpotAlpha * uicolor.alpha));
            uiColor? ambientColor = null;
            uiColor? spotColor = null;
            ShadowUtils.computeTonalColors(inAmbient, inSpot, ref ambientColor, ref spotColor);
            ShadowUtils.drawShadow(
                canvas,
                path,
                new Vector3(0, 0, dpr * elevation),
                new Vector3(shadow_x, shadow_y, dpr * kLightHeight),
                dpr * kLightRadius,
                ambientColor.Value,
                spotColor.Value,
                0
            );
        }
    }
}