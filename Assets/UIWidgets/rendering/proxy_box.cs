using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.rendering {
    public class RenderProxyBox : RenderProxyBoxMixinRenderObjectWithChildMixinRenderBox<RenderBox> {
        public RenderProxyBox(RenderBox child = null) {
            this.child = child;
        }
    }

    public class RenderConstrainedBox : RenderProxyBox {
        public RenderConstrainedBox(
            RenderBox child = null,
            BoxConstraints additionalConstraints = null) : base(child) {
            this._additionalConstraints = additionalConstraints;
        }

        public BoxConstraints additionalConstraints {
            get { return this._additionalConstraints; }

            set {
                if (this._additionalConstraints == value) {
                    return;
                }

                this._additionalConstraints = value;
                this.markNeedsLayout();
            }
        }

        public BoxConstraints _additionalConstraints;

        public override double computeMinIntrinsicWidth(double height) {
            if (this._additionalConstraints.hasBoundedWidth && this._additionalConstraints.hasTightWidth) {
                return this._additionalConstraints.minWidth;
            }

            double width = base.computeMinIntrinsicWidth(height);
            if (!this._additionalConstraints.hasInfiniteWidth) {
                return this._additionalConstraints.constrainWidth(width);
            }

            return width;
        }

        public override double computeMaxIntrinsicWidth(double height) {
            if (this._additionalConstraints.hasBoundedWidth && this._additionalConstraints.hasTightWidth) {
                return this._additionalConstraints.maxWidth;
            }

            double width = base.computeMaxIntrinsicWidth(height);
            if (!this._additionalConstraints.hasInfiniteWidth) {
                return this._additionalConstraints.constrainWidth(width);
            }

            return width;
        }

        public override double computeMinIntrinsicHeight(double width) {
            if (this._additionalConstraints.hasBoundedHeight && this._additionalConstraints.hasTightHeight) {
                return this._additionalConstraints.minHeight;
            }

            double height = base.computeMinIntrinsicHeight(width);
            if (!this._additionalConstraints.hasInfiniteHeight) {
                return this._additionalConstraints.constrainHeight(height);
            }

            return height;
        }

        public override double computeMaxIntrinsicHeight(double width) {
            if (this._additionalConstraints.hasBoundedHeight && this._additionalConstraints.hasTightHeight) {
                return this._additionalConstraints.minHeight;
            }

            double height = base.computeMaxIntrinsicHeight(width);
            if (!this._additionalConstraints.hasInfiniteHeight) {
                return this._additionalConstraints.constrainHeight(height);
            }

            return height;
        }

        public override void performLayout() {
            if (this.child != null) {
                this.child.layout(this._additionalConstraints.enforce(this.constraints), parentUsesSize: true);
                this.size = this.child.size;
            } else {
                this.size = this._additionalConstraints.enforce(this.constraints).constrain(Size.zero);
            }
        }
    }

    public class RenderLimitedBox : RenderProxyBox {
        public RenderLimitedBox(
            RenderBox child = null,
            double maxWidth = double.PositiveInfinity,
            double maxHeight = double.PositiveInfinity
        ) : base(child) {
            this._maxWidth = maxWidth;
            this._maxHeight = maxHeight;
        }

        public double maxWidth {
            get { return this._maxWidth; }
            set {
                if (this._maxWidth == value) {
                    return;
                }

                this._maxWidth = value;
                this.markNeedsLayout();
            }
        }

        public double _maxWidth;

        public double maxHeight {
            get { return this._maxHeight; }
            set {
                if (this._maxHeight == value) {
                    return;
                }

                this._maxHeight = value;
                this.markNeedsLayout();
            }
        }

        public double _maxHeight;

        public BoxConstraints _limitConstraints(BoxConstraints constraints) {
            return new BoxConstraints(
                minWidth: constraints.minWidth,
                maxWidth: constraints.hasBoundedWidth
                    ? constraints.maxWidth
                    : constraints.constrainWidth(this.maxWidth),
                minHeight: constraints.minHeight,
                maxHeight: constraints.hasBoundedHeight
                    ? constraints.maxHeight
                    : constraints.constrainHeight(this.maxHeight)
            );
        }

        public override void performLayout() {
            if (this.child != null) {
                this.child.layout(this._limitConstraints(this.constraints), parentUsesSize: true);
                this.size = this.constraints.constrain(this.child.size);
            } else {
                this.size = this._limitConstraints(this.constraints).constrain(Size.zero);
            }
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
        ) : base(child) {
            this._decoration = decoration;
            this._position = position;
            this._configuration = configuration ?? ImageConfiguration.empty;
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