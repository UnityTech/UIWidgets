using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

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
            D.assert(axis != null);
            D.assert(sizeFactor != null);
            D.assert(axisAlignment != null);
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


    public class AnimatedBuilder : AnimatedWidget {
        public readonly TransitionBuilder builder;

        public readonly Widget child;

        public AnimatedBuilder(Key key = null, Listenable animation = null, TransitionBuilder builder = null,
            Widget child = null) :
            base(key, animation) {
            D.assert(builder != null);
            this.builder = builder;
            this.child = child;
        }

        protected internal override Widget build(BuildContext context) {
            return this.builder(context, this.child);
        }
    }
}