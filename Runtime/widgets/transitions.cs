using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public abstract class AnimatedWidget : StatefulWidget {
        public readonly Listenable listenable;

        protected AnimatedWidget(Key key = null, Listenable listenable = null) : base(key) {
            D.assert(listenable != null);
            this.listenable = listenable;
        }

        protected internal abstract Widget build(BuildContext context);

        public override State createState() {
            return new _AnimatedState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Listenable>("animation", this.listenable));
        }
    }

    public class _AnimatedState : State<AnimatedWidget> {
        public override void initState() {
            base.initState();
            this.widget.listenable.addListener(this._handleChange);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (this.widget.listenable != ((AnimatedWidget) oldWidget).listenable) {
                ((AnimatedWidget) oldWidget).listenable.removeListener(this._handleChange);
                this.widget.listenable.addListener(this._handleChange);
            }
        }

        public override void dispose() {
            this.widget.listenable.removeListener(this._handleChange);
            base.dispose();
        }

        void _handleChange() {
            this.setState(() => {
                // The listenable's state is our build state, and it changed already.
            });
        }

        public override Widget build(BuildContext context) {
            return this.widget.build(context);
        }
    }

    public class SlideTransition : AnimatedWidget {
        public SlideTransition(Key key = null,
            Animation<Offset> position = null,
            bool transformHitTests = true,
            TextDirection? textDirection = null,
            Widget child = null) : base(key: key, listenable: position) {
            D.assert(position != null);
            this.transformHitTests = transformHitTests;
            this.textDirection = textDirection;
            this.child = child;
        }

        public Animation<Offset> position {
            get { return (Animation<Offset>) this.listenable; }
        }

        public readonly TextDirection? textDirection;

        public readonly bool transformHitTests;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            var offset = this.position.value;
            if (this.textDirection == TextDirection.rtl) {
                offset = new Offset(-offset.dx, offset.dy);
            }

            return new FractionalTranslation(
                translation: offset,
                transformHitTests: this.transformHitTests,
                child: this.child
            );
        }
    }


    public class ScaleTransition : AnimatedWidget {
        public ScaleTransition(
            Key key = null,
            Animation<float> scale = null,
            Alignment alignment = null,
            Widget child = null
        ) : base(key: key, listenable: scale) {
            alignment = alignment ?? Alignment.center;
            D.assert(scale != null);
            this.alignment = alignment;
            this.child = child;
        }

        public Animation<float> scale {
            get { return (Animation<float>) this.listenable; }
        }

        public readonly Alignment alignment;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            float scaleValue = this.scale.value;
            Matrix3 transform = Matrix3.makeScale(scaleValue, scaleValue);
            return new Transform(
                transform: transform,
                alignment: this.alignment,
                child: this.child
            );
        }
    }


    public class RotationTransition : AnimatedWidget {
        public RotationTransition(
            Key key = null,
            Animation<float> turns = null,
            Alignment alignment = null,
            Widget child = null) : base(key: key, listenable: turns) {
            D.assert(turns != null);
            this.alignment = alignment ?? Alignment.center;
            this.child = child;
        }

        public Animation<float> turns {
            get { return (Animation<float>) this.listenable; }
        }

        public readonly Alignment alignment;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            float turnsValue = this.turns.value;
            Matrix3 transform = Matrix3.makeRotate((turnsValue * Mathf.PI * 2.0f));
            return new Transform(
                transform: transform,
                alignment: this.alignment,
                child: this.child);
        }
    }

    public class SizeTransition : AnimatedWidget {
        public SizeTransition(
            Key key = null,
            Axis axis = Axis.vertical,
            Animation<float> sizeFactor = null,
            float axisAlignment = 0.0f,
            Widget child = null) : base(key: key, listenable: sizeFactor) {
            D.assert(sizeFactor != null);
            this.axis = axis;
            this.axisAlignment = axisAlignment;
            this.child = child;
        }

        public readonly Axis axis;

        public readonly float axisAlignment;

        Animation<float> sizeFactor {
            get { return (Animation<float>) this.listenable; }
        }

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            Alignment alignment;
            if (this.axis == Axis.vertical) {
                alignment = new Alignment(-1.0f, this.axisAlignment);
            }
            else {
                alignment = new Alignment(this.axisAlignment, -1.0f);
            }

            return new ClipRect(
                child: new Align(
                    alignment: alignment,
                    widthFactor: this.axis == Axis.horizontal ? (float?) Mathf.Max(this.sizeFactor.value, 0.0f) : null,
                    heightFactor: this.axis == Axis.vertical ? (float?) Mathf.Max(this.sizeFactor.value, 0.0f) : null,
                    child: this.child
                )
            );
        }
    }

    public class FadeTransition : SingleChildRenderObjectWidget {
        public FadeTransition(Key key = null, Animation<float> opacity = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(opacity != null);
            this.opacity = opacity;
        }

        public readonly Animation<float> opacity;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderAnimatedOpacity(
                opacity: this.opacity
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderAnimatedOpacity) renderObject).opacity = this.opacity;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Animation<float>>("opacity", this.opacity));
        }
    }

    public class RelativeRectTween : Tween<RelativeRect> {
        public RelativeRectTween(RelativeRect begin = null, RelativeRect end = null) : base(begin: begin, end: end) {
        }

        public override RelativeRect lerp(float t) {
            return RelativeRect.lerp(this.begin, this.end, t);
        }
    }


    public class PositionedTransition : AnimatedWidget {
        public PositionedTransition(
            Key key = null,
            Animation<RelativeRect> rect = null,
            Widget child = null
        ) : base(key: key, listenable: rect) {
            D.assert(rect != null);
            D.assert(child != null);
            this.child = child;
        }

        Animation<RelativeRect> rect {
            get { return (Animation<RelativeRect>) this.listenable; }
        }


        public readonly Widget child;


        protected internal override Widget build(BuildContext context) {
            return Positioned.fromRelativeRect(
                rect: this.rect.value,
                child: this.child
            );
        }
    }

    public class RelativePositionedTransition : AnimatedWidget {
        public RelativePositionedTransition(
            Key key = null,
            Animation<Rect> rect = null,
            Size size = null,
            Widget child = null
        ) : base(key: key, listenable: rect) {
            D.assert(rect != null);
            D.assert(size != null);
            D.assert(child != null);
            this.size = size;
            this.child = child;
        }

        Animation<Rect> rect {
            get { return (Animation<Rect>) this.listenable; }
        }

        public readonly Size size;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            RelativeRect offsets = RelativeRect.fromSize(this.rect.value, this.size);
            return new Positioned(
                top: offsets.top,
                right: offsets.right,
                bottom: offsets.bottom,
                left: offsets.left,
                child: this.child
            );
        }
    }


    public class DecoratedBoxTransition : AnimatedWidget {
        public DecoratedBoxTransition(
            Key key = null,
            Animation<Decoration> decoration = null,
            DecorationPosition position = DecorationPosition.background,
            Widget child = null
        ) : base(key: key, listenable: decoration) {
            D.assert(decoration != null);
            D.assert(child != null);
            this.decoration = decoration;
            this.position = position;
            this.child = child;
        }

        public readonly Animation<Decoration> decoration;

        public readonly DecorationPosition position;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            return new DecoratedBox(
                decoration: this.decoration.value,
                position: this.position,
                child: this.child
            );
        }
    }

    public class AlignTransition : AnimatedWidget {
        public AlignTransition(
            Key key = null,
            Animation<Alignment> alignment = null,
            Widget child = null,
            float? widthFactor = null,
            float? heightFactor = null
        ) : base(key: key, listenable: alignment) {
            D.assert(alignment != null);
            D.assert(child != null);
            this.child = child;
            this.widthFactor = widthFactor;
            this.heightFactor = heightFactor;
        }

        Animation<Alignment> alignment {
            get { return (Animation<Alignment>) this.listenable; }
        }

        public readonly float? widthFactor;

        public readonly float? heightFactor;

        public readonly Widget child;


        protected internal override Widget build(BuildContext context) {
            return new Align(
                alignment: this.alignment.value,
                widthFactor: this.widthFactor,
                heightFactor: this.heightFactor,
                child: this.child
            );
        }
    }

    public class DefaultTextStyleTransition : AnimatedWidget {
        public DefaultTextStyleTransition(
            Key key = null,
            Animation<TextStyle> style = null,
            Widget child = null,
            TextAlign? textAlign = null,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip,
            int? maxLines = null
        ) : base(key: key, listenable: style) {
            D.assert(style != null);
            D.assert(child != null);
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.maxLines = maxLines;
            this.child = child;
        }

        Animation<TextStyle> style {
            get { return (Animation<TextStyle>) this.listenable; }
        }

        public readonly TextAlign? textAlign;

        public readonly bool softWrap;
        public readonly TextOverflow overflow;
        public readonly int? maxLines;
        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            return new DefaultTextStyle(
                style: this.style.value,
                textAlign: this.textAlign,
                softWrap: this.softWrap,
                overflow: this.overflow,
                maxLines: this.maxLines,
                child: this.child
            );
        }
    }


    public class AnimatedBuilder : AnimatedWidget {
        public readonly TransitionBuilder builder;

        public readonly Widget child;

        public AnimatedBuilder(Key key = null, Listenable animation = null, TransitionBuilder builder = null,
            Widget child = null) :
            base(key, animation) {
            D.assert(builder != null);
            D.assert(animation != null);
            this.builder = builder;
            this.child = child;
        }

        protected internal override Widget build(BuildContext context) {
            return this.builder(context, this.child);
        }
    }
}