using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class PhysicalShapeLayer : ContainerLayer {
        public PhysicalShapeLayer(
            Clip clipBehavior) {
            this.isRect_ = false;
            this.clip_behavior_ = clipBehavior;
        }

        float elevation_;
        Color color_;
        Color shadow_color_;
        float device_pixel_ratio_;
        Path path_;
        bool isRect_;
        Rect frameRRect_;
        Clip clip_behavior_;

        public void set_path(Path path) {
            //todo: xingwei.zhu : try to do path => rect transfer
            this.path_ = path;
            this.isRect_ = false;
            this.frameRRect_ = path.getBounds();
        }

        public void set_elevation(float elevation) {
            this.elevation_ = elevation;
        }

        public void set_color(Color color) {
            this.color_ = color;
        }

        public void set_shadow_color(Color shadowColor) {
            this.shadow_color_ = shadowColor;
        }

        public void set_device_pixel_ratio(float dpr) {
            this.device_pixel_ratio_ = dpr;
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            Rect child_paint_bounds = Rect.zero;
            this.prerollChildren(context, matrix, ref child_paint_bounds);

            if (this.elevation_ == 0) {
                this.paintBounds = this.path_.getBounds();
            }
            else {
                Rect bounds = this.path_.getBounds();
                //todo xingwei.zhu: outter set shadow
                //bounds.outset(20.0f, 20.0f);
                this.paintBounds = bounds;
            }
        }

        public override void paint(PaintContext context) {
            if (this.elevation_ != 0) {
                this.drawShadow(context.canvas, this.path_, this.shadow_color_, this.elevation_,
                    this.color_.alpha != 255, this.device_pixel_ratio_);
            }

            Paint paint = new Paint {color = this.color_};
            if (this.clip_behavior_ != Clip.antiAliasWithSaveLayer) {
                context.canvas.drawPath(this.path_, paint);
            }

            context.canvas.save();
            int saveCount = 1;
            switch (this.clip_behavior_) {
                case Clip.hardEdge:
                    context.canvas.clipPath(this.path_);
                    break;
                case Clip.antiAlias:
                    context.canvas.clipPath(this.path_);
                    break;
                case Clip.antiAliasWithSaveLayer:
                    context.canvas.clipPath(this.path_);
                    context.canvas.saveLayer(this.paintBounds, null);
                    saveCount++;
                    break;
                case Clip.none:
                    break;
            }

            if (this.clip_behavior_ == Clip.antiAliasWithSaveLayer) {
                //todo xingwei.zhu: drawPaint
                context.canvas.drawPath(this.path_, paint);
            }

            this.paintChildren(context);
            for (int i = 0; i < saveCount; i++) {
                context.canvas.restore();
            }
        }


        void drawShadow(Canvas canvas, Path path, Color color, float elevation, bool transparentOccluder, float dpr) {
            //todo xingwei.zhu: to be implemented
        }
    }
}