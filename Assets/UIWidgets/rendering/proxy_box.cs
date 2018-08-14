using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.rendering {
    public class RenderProxyBox : RenderProxyBoxMixinRenderObjectWithChildMixinRenderBox<RenderBox> {
        public RenderProxyBox(RenderBox child = null) {
            this.child = child;
        }
    }

    public enum DecorationPosition {
        background,
        foreground,
    }

    public class RenderDecoratedBox : RenderProxyBox {
        public RenderDecoratedBox(
            Decoration decoration,
            DecorationPosition position = DecorationPosition.background,
            ImageConfiguration configuration = null,
            RenderBox child = null
        ) {
        }

        public BoxPainter _painter;

        public Decoration decoration {
            get { return this._decoration; }
            set {
                if (value == this._decoration) {
                    return;
                }

                if (this._painter != null) {
                    this._painter.dispose();
                    this._painter = null;
                }

                this._decoration = value;
                this.markNeedsPaint();
            }
        }

        public Decoration _decoration;

        public DecorationPosition position {
            get { return this._position; }
            set {
                if (value == this._position) {
                    return;
                }

                this._position = value;
                this.markNeedsPaint();
            }
        }

        public DecorationPosition _position;


        public ImageConfiguration configuration {
            get { return this._configuration; }
            set {
                if (value == this._configuration) {
                    return;
                }

                this._configuration = value;
                this.markNeedsPaint();
            }
        }

        public ImageConfiguration _configuration;

        public override void detach() {
            if (this._painter != null) {
                this._painter.dispose();
                this._painter = null;
            }

            base.detach();
            this.markNeedsPaint();
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._painter == null) {
                this._painter = this._decoration.createBoxPainter(this.markNeedsPaint);
            }

            var filledConfiguration = this.configuration.copyWith(size: this.size);
            if (this.position == DecorationPosition.background) {
                this._painter.paint(context.canvas, offset, filledConfiguration);
            }

            base.paint(context, offset);

            if (this.position == DecorationPosition.foreground) {
                this._painter.paint(context.canvas, offset, filledConfiguration);
            }
        }
    }
}