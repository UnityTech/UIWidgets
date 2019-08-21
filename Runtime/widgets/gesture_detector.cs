using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public abstract class GestureRecognizerFactory {
        public abstract GestureRecognizer constructorRaw();

        public abstract void initializerRaw(GestureRecognizer instance);

        internal abstract bool _debugAssertTypeMatches(Type type);
    }

    public abstract class GestureRecognizerFactory<T> : GestureRecognizerFactory where T : GestureRecognizer {
        public override GestureRecognizer constructorRaw() {
            return this.constructor();
        }

        public override void initializerRaw(GestureRecognizer instance) {
            this.initializer((T) instance);
        }

        public abstract T constructor();

        public abstract void initializer(T instance);

        internal override bool _debugAssertTypeMatches(Type type) {
            D.assert(type == typeof(T),
                () => "GestureRecognizerFactory of type " + typeof(T) + " was used where type $type was specified.");
            return true;
        }
    }

    public delegate T GestureRecognizerFactoryConstructor<T>() where T : GestureRecognizer;

    public delegate void GestureRecognizerFactoryInitializer<T>(T instance) where T : GestureRecognizer;

    public class GestureRecognizerFactoryWithHandlers<T> : GestureRecognizerFactory<T> where T : GestureRecognizer {
        public GestureRecognizerFactoryWithHandlers(GestureRecognizerFactoryConstructor<T> constructor,
            GestureRecognizerFactoryInitializer<T> initializer) {
            D.assert(constructor != null);
            D.assert(initializer != null);

            this._constructor = constructor;
            this._initializer = initializer;
        }

        readonly GestureRecognizerFactoryConstructor<T> _constructor;

        readonly GestureRecognizerFactoryInitializer<T> _initializer;

        public override T constructor() {
            return this._constructor();
        }

        public override void initializer(T instance) {
            this._initializer(instance);
        }
    }

    public class GestureDetector : StatelessWidget {
        public GestureDetector(
            Key key = null,
            Widget child = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onTapUp = null,
            GestureTapCallback onTap = null,
            GestureTapCancelCallback onTapCancel = null,
            GestureDoubleTapCallback onDoubleTap = null,
            GestureLongPressCallback onLongPress = null,
            GestureLongPressStartCallback onLongPressStart = null,
            GestureLongPressMoveUpdateCallback onLongPressMoveUpdate = null,
            GestureLongPressUpCallback onLongPressUp = null,
            GestureLongPressEndCallback onLongPressEnd = null,
            GestureDragDownCallback onVerticalDragDown = null,
            GestureDragStartCallback onVerticalDragStart = null,
            GestureDragUpdateCallback onVerticalDragUpdate = null,
            GestureDragEndCallback onVerticalDragEnd = null,
            GestureDragCancelCallback onVerticalDragCancel = null,
            GestureDragDownCallback onHorizontalDragDown = null,
            GestureDragStartCallback onHorizontalDragStart = null,
            GestureDragUpdateCallback onHorizontalDragUpdate = null,
            GestureDragEndCallback onHorizontalDragEnd = null,
            GestureDragCancelCallback onHorizontalDragCancel = null,
            GestureDragDownCallback onPanDown = null,
            GestureDragStartCallback onPanStart = null,
            GestureDragUpdateCallback onPanUpdate = null,
            GestureDragEndCallback onPanEnd = null,
            GestureDragCancelCallback onPanCancel = null,
            GestureScaleStartCallback onScaleStart = null,
            GestureScaleUpdateCallback onScaleUpdate = null,
            GestureScaleEndCallback onScaleEnd = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            DragStartBehavior dragStartBehavior = DragStartBehavior.down
        ) : base(key) {
            D.assert(() => {
                bool haveVerticalDrag =
                    onVerticalDragStart != null || onVerticalDragUpdate != null ||
                    onVerticalDragEnd != null;
                bool haveHorizontalDrag =
                    onHorizontalDragStart != null || onHorizontalDragUpdate != null ||
                    onHorizontalDragEnd != null;
                bool havePan = onPanStart != null || onPanUpdate != null || onPanEnd != null;
                bool haveScale = onScaleStart != null || onScaleUpdate != null || onScaleEnd != null;
                if (havePan || haveScale) {
                    if (havePan && haveScale) {
                        throw new UIWidgetsError(
                            "Incorrect GestureDetector arguments.\n" +
                            "Having both a pan gesture recognizer and a scale gesture recognizer is redundant; scale is a superset of pan. Just use the scale gesture recognizer."
                        );
                    }

                    string recognizer = havePan ? "pan" : "scale";
                    if (haveVerticalDrag && haveHorizontalDrag) {
                        throw new UIWidgetsError(
                            "Incorrect GestureDetector arguments.\n" +
                            $"Simultaneously having a vertical drag gesture recognizer, a horizontal drag gesture recognizer, and a {recognizer} gesture recognizer " +
                            $"will result in the {recognizer} gesture recognizer being ignored, since the other two will catch all drags."
                        );
                    }
                }

                return true;
            });

            this.child = child;
            this.onTapDown = onTapDown;
            this.onTapUp = onTapUp;
            this.onTap = onTap;
            this.onTapCancel = onTapCancel;
            this.onDoubleTap = onDoubleTap;
            this.onLongPress = onLongPress;
            this.onLongPressUp = onLongPressUp;
            this.onLongPressStart = onLongPressStart;
            this.onLongPressMoveUpdate = onLongPressMoveUpdate;
            this.onLongPressEnd = onLongPressEnd;
            this.onVerticalDragDown = onVerticalDragDown;
            this.onVerticalDragStart = onVerticalDragStart;
            this.onVerticalDragUpdate = onVerticalDragUpdate;
            this.onVerticalDragEnd = onVerticalDragEnd;
            this.onVerticalDragCancel = onVerticalDragCancel;
            this.onHorizontalDragDown = onHorizontalDragDown;
            this.onHorizontalDragStart = onHorizontalDragStart;
            this.onHorizontalDragUpdate = onHorizontalDragUpdate;
            this.onHorizontalDragEnd = onHorizontalDragEnd;
            this.onHorizontalDragCancel = onHorizontalDragCancel;
            this.onPanDown = onPanDown;
            this.onPanStart = onPanStart;
            this.onPanUpdate = onPanUpdate;
            this.onPanEnd = onPanEnd;
            this.onPanCancel = onPanCancel;
            this.onScaleStart = onScaleStart;
            this.onScaleUpdate = onScaleUpdate;
            this.onScaleEnd = onScaleEnd;
            this.behavior = behavior;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly Widget child;
        public readonly GestureTapDownCallback onTapDown;
        public readonly GestureTapUpCallback onTapUp;
        public readonly GestureTapCallback onTap;
        public readonly GestureTapCancelCallback onTapCancel;
        public readonly GestureDoubleTapCallback onDoubleTap;
        public readonly GestureLongPressCallback onLongPress;
        public readonly GestureLongPressUpCallback onLongPressUp;
        public readonly GestureLongPressStartCallback onLongPressStart;
        public readonly GestureLongPressMoveUpdateCallback onLongPressMoveUpdate;
        public readonly GestureLongPressEndCallback onLongPressEnd;
        public readonly GestureDragDownCallback onVerticalDragDown;
        public readonly GestureDragStartCallback onVerticalDragStart;
        public readonly GestureDragUpdateCallback onVerticalDragUpdate;
        public readonly GestureDragEndCallback onVerticalDragEnd;
        public readonly GestureDragCancelCallback onVerticalDragCancel;
        public readonly GestureDragDownCallback onHorizontalDragDown;
        public readonly GestureDragStartCallback onHorizontalDragStart;
        public readonly GestureDragUpdateCallback onHorizontalDragUpdate;
        public readonly GestureDragEndCallback onHorizontalDragEnd;
        public readonly GestureDragCancelCallback onHorizontalDragCancel;
        public readonly GestureDragDownCallback onPanDown;
        public readonly GestureDragStartCallback onPanStart;
        public readonly GestureDragUpdateCallback onPanUpdate;
        public readonly GestureDragEndCallback onPanEnd;
        public readonly GestureDragCancelCallback onPanCancel;
        public readonly GestureScaleStartCallback onScaleStart;
        public readonly GestureScaleUpdateCallback onScaleUpdate;
        public readonly GestureScaleEndCallback onScaleEnd;
        public readonly HitTestBehavior behavior;
        public readonly DragStartBehavior dragStartBehavior;

        public override Widget build(BuildContext context) {
            var gestures = new Dictionary<Type, GestureRecognizerFactory>();

            if (this.onTapDown != null ||
                this.onTapUp != null ||
                this.onTap != null ||
                this.onTapCancel != null) {
                gestures[typeof(TapGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<TapGestureRecognizer>(
                        () => new TapGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onTapDown = this.onTapDown;
                            instance.onTapUp = this.onTapUp;
                            instance.onTap = this.onTap;
                            instance.onTapCancel = this.onTapCancel;
                        }
                    );
            }

            if (this.onDoubleTap != null) {
                gestures[typeof(DoubleTapGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<DoubleTapGestureRecognizer>(
                        () => new DoubleTapGestureRecognizer(debugOwner: this),
                        instance => { instance.onDoubleTap = this.onDoubleTap; }
                    );
            }

            if (this.onLongPress != null ||
                this.onLongPressUp != null ||
                this.onLongPressStart != null ||
                this.onLongPressMoveUpdate != null ||
                this.onLongPressEnd != null) {
                gestures[typeof(LongPressGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<LongPressGestureRecognizer>(
                        () => new LongPressGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onLongPress = this.onLongPress;
                            instance.onLongPressStart = this.onLongPressStart;
                            instance.onLongPressMoveUpdate = this.onLongPressMoveUpdate;
                            instance.onLongPressEnd = this.onLongPressEnd;
                            instance.onLongPressUp = this.onLongPressUp;
                        }
                    );
            }

            if (this.onVerticalDragDown != null ||
                this.onVerticalDragStart != null ||
                this.onVerticalDragUpdate != null ||
                this.onVerticalDragEnd != null ||
                this.onVerticalDragCancel != null) {
                gestures[typeof(VerticalDragGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<VerticalDragGestureRecognizer>(
                        () => new VerticalDragGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onDown = this.onVerticalDragDown;
                            instance.onStart = this.onVerticalDragStart;
                            instance.onUpdate = this.onVerticalDragUpdate;
                            instance.onEnd = this.onVerticalDragEnd;
                            instance.onCancel = this.onVerticalDragCancel;
                            instance.dragStartBehavior = this.dragStartBehavior;
                        }
                    );
            }

            if (this.onHorizontalDragDown != null ||
                this.onHorizontalDragStart != null ||
                this.onHorizontalDragUpdate != null ||
                this.onHorizontalDragEnd != null ||
                this.onHorizontalDragCancel != null) {
                gestures[typeof(HorizontalDragGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<HorizontalDragGestureRecognizer>(
                        () => new HorizontalDragGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onDown = this.onHorizontalDragDown;
                            instance.onStart = this.onHorizontalDragStart;
                            instance.onUpdate = this.onHorizontalDragUpdate;
                            instance.onEnd = this.onHorizontalDragEnd;
                            instance.onCancel = this.onHorizontalDragCancel;
                            instance.dragStartBehavior = this.dragStartBehavior;
                        }
                    );
            }

            if (this.onPanDown != null ||
                this.onPanStart != null ||
                this.onPanUpdate != null ||
                this.onPanEnd != null ||
                this.onPanCancel != null) {
                gestures[typeof(PanGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<PanGestureRecognizer>(
                        () => new PanGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onDown = this.onPanDown;
                            instance.onStart = this.onPanStart;
                            instance.onUpdate = this.onPanUpdate;
                            instance.onEnd = this.onPanEnd;
                            instance.onCancel = this.onPanCancel;
                            instance.dragStartBehavior = this.dragStartBehavior;
                        }
                    );
            }

            if (this.onScaleStart != null ||
                this.onScaleUpdate != null ||
                this.onScaleEnd != null) {
                gestures[typeof(ScaleGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<ScaleGestureRecognizer>(
                        () => new ScaleGestureRecognizer(debugOwner: this),
                        instance => {
                            instance.onStart = this.onScaleStart;
                            instance.onUpdate = this.onScaleUpdate;
                            instance.onEnd = this.onScaleEnd;
                        }
                    );
            }

            return new RawGestureDetector(
                gestures: gestures,
                behavior: this.behavior,
                child: this.child
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<DragStartBehavior>("startBehavior", this.dragStartBehavior));
        }
    }

    public class RawGestureDetector : StatefulWidget {
        public RawGestureDetector(
            Key key = null,
            Widget child = null,
            Dictionary<Type, GestureRecognizerFactory> gestures = null,
            HitTestBehavior? behavior = null
        ) : base(key: key) {
            D.assert(gestures != null);
            this.child = child;
            this.gestures = gestures ?? new Dictionary<Type, GestureRecognizerFactory>();
            this.behavior = behavior;
        }

        public readonly Widget child;

        public readonly Dictionary<Type, GestureRecognizerFactory> gestures;

        public readonly HitTestBehavior? behavior;

        public override State createState() {
            return new RawGestureDetectorState();
        }
    }

    public class RawGestureDetectorState : State<RawGestureDetector> {
        Dictionary<Type, GestureRecognizer> _recognizers = new Dictionary<Type, GestureRecognizer>();

        public override void initState() {
            base.initState();
            this._syncAll(this.widget.gestures);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            this._syncAll(this.widget.gestures);
        }

        public void replaceGestureRecognizers(Dictionary<Type, GestureRecognizerFactory> gestures) {
            D.assert(() => {
                if (!this.context.findRenderObject().owner.debugDoingLayout) {
                    throw new UIWidgetsError(
                        "Unexpected call to replaceGestureRecognizers() method of RawGestureDetectorState.\n" +
                        "The replaceGestureRecognizers() method can only be called during the layout phase. " +
                        "To set the gesture recognizers at other times, trigger a new build using setState() " +
                        "and provide the new gesture recognizers as constructor arguments to the corresponding " +
                        "RawGestureDetector or GestureDetector object.");
                }

                return true;
            });
            this._syncAll(gestures);
        }

        public override void dispose() {
            foreach (GestureRecognizer recognizer in this._recognizers.Values) {
                recognizer.dispose();
            }

            this._recognizers = null;
            base.dispose();
        }

        void _syncAll(Dictionary<Type, GestureRecognizerFactory> gestures) {
            D.assert(this._recognizers != null);
            var oldRecognizers = this._recognizers;
            this._recognizers = new Dictionary<Type, GestureRecognizer>();

            foreach (Type type in gestures.Keys) {
                D.assert(gestures[type] != null);
                D.assert(gestures[type]._debugAssertTypeMatches(type));
                D.assert(!this._recognizers.ContainsKey(type));
                this._recognizers[type] = oldRecognizers.ContainsKey(type)
                    ? oldRecognizers[type]
                    : gestures[type].constructorRaw();
                D.assert(this._recognizers[type].GetType() == type,
                    () => "GestureRecognizerFactory of type " + type + " created a GestureRecognizer of type " +
                          this._recognizers[type].GetType() +
                          ". The GestureRecognizerFactory must be specialized with the type of the class that it returns from its constructor method.");
                gestures[type].initializerRaw(this._recognizers[type]);
            }

            foreach (Type type in oldRecognizers.Keys) {
                if (!this._recognizers.ContainsKey(type)) {
                    oldRecognizers[type].dispose();
                }
            }
        }

        void _handlePointerDown(PointerDownEvent evt) {
            D.assert(this._recognizers != null);
            foreach (GestureRecognizer recognizer in this._recognizers.Values) {
                recognizer.addPointer(evt);
            }
        }

        void _handlePointerScroll(PointerScrollEvent evt) {
            D.assert(this._recognizers != null);
            foreach (GestureRecognizer recognizer in this._recognizers.Values) {
                recognizer.addScrollPointer(evt);
            }
        }

        HitTestBehavior _defaultBehavior {
            get { return this.widget.child == null ? HitTestBehavior.translucent : HitTestBehavior.deferToChild; }
        }


        public override Widget build(BuildContext context) {
            Widget result = new Listener(
                onPointerDown: this._handlePointerDown,
                onPointerScroll: this._handlePointerScroll,
                behavior: this.widget.behavior ?? this._defaultBehavior,
                child: this.widget.child
            );
            return result;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            if (this._recognizers == null) {
                properties.add(DiagnosticsNode.message("DISPOSED"));
            }
            else {
                List<string> gestures = this._recognizers.Values.Select(recognizer => recognizer.debugDescription)
                    .ToList();
                properties.add(new EnumerableProperty<string>("gestures", gestures, ifEmpty: "<none>"));
                properties.add(new EnumerableProperty<GestureRecognizer>("recognizers", this._recognizers.Values,
                    level: DiagnosticLevel.fine));
            }

            properties.add(new EnumProperty<HitTestBehavior?>("behavior", this.widget.behavior,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }
}