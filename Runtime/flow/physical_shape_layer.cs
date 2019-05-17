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

        public override void paint(PaintContext context) {
            if (this._elevation != 0) {
                drawShadow(context.canvas, this._path, this._shadow_color, this._elevation,
                    this._color.alpha != 255, this._device_pixel_ratio);
            }

            Paint paint = new Paint {color = this._color};
            //todo: xingwei.zhu: process according to different clipBehavior, currently use antiAlias as default

            context.canvas.drawPath(this._path, paint);

            context.canvas.save();
            context.canvas.clipPath(this._path);
            this.paintChildren(context);
            context.canvas.restore();
        }


        public static void drawShadow(Canvas canvas, Path path, Color color, float elevation, bool transparentOccluder,
            float dpr) {
            float kAmbientAlpha = 0.039f;
            float kSpotAlpha = ShadowUtils.kUseFastShadow ? 0.1f : 0.25f;
            float kLightHeight = 600f;
            float kLightRadius = 800f;

            Rect bounds = path.getBounds();
            float shadow_x = (bounds.left + bounds.right) / 2f;
            float shadow_y = bounds.top - 600.0f;
            Color inAmbient = color.withAlpha((int) (kAmbientAlpha * color.alpha));
            Color inSpot = color.withAlpha((int) (kSpotAlpha * color.alpha));
            Color ambientColor = null;
            Color spotColor = null;
            ShadowUtils.computeTonalColors(inAmbient, inSpot, ref ambientColor, ref spotColor);
            ShadowUtils.drawShadow(
                canvas,
                path,
                new Vector3(0, 0, dpr * elevation),
                new Vector3(shadow_x, shadow_y, dpr * kLightHeight),
                dpr * kLightRadius,
                ambientColor,
                spotColor,
                0
            );
        }
    }
}