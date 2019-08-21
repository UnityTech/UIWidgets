using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public abstract class InteractiveInkFeature : InkFeature {
        public InteractiveInkFeature(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Color color = null,
            VoidCallback onRemoved = null
        ) : base(controller: controller, referenceBox: referenceBox, onRemoved: onRemoved) {
            D.assert(controller != null);
            D.assert(referenceBox != null);
            this._color = color;
        }

        public virtual void confirm() {
        }

        public virtual void cancel() {
        }

        public Color color {
            get { return this._color; }
            set {
                if (value == this._color) {
                    return;
                }

                this._color = value;
                this.controller.markNeedsPaint();
            }
        }

        Color _color;
    }

    public abstract class InteractiveInkFeatureFactory {
        public InteractiveInkFeatureFactory() {
        }

        public abstract InteractiveInkFeature create(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Offset position = null,
            Color color = null,
            bool containedInkWell = false,
            RectCallback rectCallback = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            float? radius = null,
            VoidCallback onRemoved = null);
    }


    public class InkResponse : StatefulWidget {
        public InkResponse(
            Key key = null,
            Widget child = null,
            GestureTapCallback onTap = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapCancelCallback onTapCancel = null,
            GestureTapCallback onDoubleTap = null,
            GestureLongPressCallback onLongPress = null,
            ValueChanged<bool> onHighlightChanged = null,
            bool containedInkWell = false,
            BoxShape highlightShape = BoxShape.circle,
            float? radius = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null) : base(key: key) {
            this.child = child;
            this.onTap = onTap;
            this.onTapDown = onTapDown;
            this.onTapCancel = onTapCancel;
            this.onDoubleTap = onDoubleTap;
            this.onLongPress = onLongPress;
            this.onHighlightChanged = onHighlightChanged;
            this.containedInkWell = containedInkWell;
            this.highlightShape = highlightShape;
            this.radius = radius;
            this.borderRadius = borderRadius;
            this.customBorder = customBorder;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.splashFactory = splashFactory;
        }

        public readonly Widget child;

        public readonly GestureTapCallback onTap;

        public readonly GestureTapDownCallback onTapDown;

        public readonly GestureTapCancelCallback onTapCancel;

        public readonly GestureTapCallback onDoubleTap;

        public readonly GestureLongPressCallback onLongPress;

        public readonly ValueChanged<bool> onHighlightChanged;

        public readonly bool containedInkWell;

        public readonly BoxShape highlightShape;

        public readonly float? radius;

        public readonly BorderRadius borderRadius;

        public readonly ShapeBorder customBorder;

        public readonly Color highlightColor;

        public readonly Color splashColor;

        public readonly InteractiveInkFeatureFactory splashFactory;

        public virtual RectCallback getRectCallback(RenderBox referenceBox) {
            return null;
        }


        public virtual bool debugCheckContext(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            return true;
        }

        public override State createState() {
            return new _InkResponseState<InkResponse>();
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            List<string> gestures = new List<string>();
            if (this.onTap != null) {
                gestures.Add("tap");
            }

            if (this.onDoubleTap != null) {
                gestures.Add("double tap");
            }

            if (this.onLongPress != null) {
                gestures.Add("long press");
            }

            if (this.onTapDown != null) {
                gestures.Add("tap down");
            }

            if (this.onTapCancel != null) {
                gestures.Add("tap cancel");
            }

            properties.add(new EnumerableProperty<string>("gestures", gestures, ifEmpty: "<none>"));
            properties.add(new DiagnosticsProperty<bool>("containedInkWell", this.containedInkWell,
                level: DiagnosticLevel.fine));
            properties.add(new DiagnosticsProperty<BoxShape>(
                "highlightShape",
                this.highlightShape,
                description: (this.containedInkWell ? "clipped to" : "") + this.highlightShape,
                showName: false
            ));
        }
    }


    public class _InkResponseState<T> : AutomaticKeepAliveClientMixin<T> where T : InkResponse {
        HashSet<InteractiveInkFeature> _splashes;
        InteractiveInkFeature _currentSplash;
        InkHighlight _lastHighlight;

        protected override bool wantKeepAlive {
            get { return this._lastHighlight != null || (this._splashes != null && this._splashes.isNotEmpty()); }
        }

        public void updateHighlight(bool value) {
            if (value == (this._lastHighlight != null && this._lastHighlight.active)) {
                return;
            }

            if (value) {
                if (this._lastHighlight == null) {
                    RenderBox referenceBox = (RenderBox) this.context.findRenderObject();
                    this._lastHighlight = new InkHighlight(
                        controller: Material.of(this.context),
                        referenceBox: referenceBox,
                        color: this.widget.highlightColor ?? Theme.of(this.context).highlightColor,
                        shape: this.widget.highlightShape,
                        borderRadius: this.widget.borderRadius,
                        customBorder: this.widget.customBorder,
                        rectCallback: this.widget.getRectCallback(referenceBox),
                        onRemoved: this._handleInkHighlightRemoval);
                    this.updateKeepAlive();
                }
                else {
                    this._lastHighlight.activate();
                }
            }
            else {
                this._lastHighlight.deactivate();
            }

            D.assert(value == (this._lastHighlight != null && this._lastHighlight.active));
            if (this.widget.onHighlightChanged != null) {
                this.widget.onHighlightChanged(value);
            }
        }

        void _handleInkHighlightRemoval() {
            D.assert(this._lastHighlight != null);
            this._lastHighlight = null;
            this.updateKeepAlive();
        }

        InteractiveInkFeature _createInkFeature(TapDownDetails details) {
            MaterialInkController inkController = Material.of(this.context);
            RenderBox referenceBox = (RenderBox) this.context.findRenderObject();
            Offset position = referenceBox.globalToLocal(details.globalPosition);
            Color color = this.widget.splashColor ?? Theme.of(this.context).splashColor;
            RectCallback rectCallback = this.widget.containedInkWell ? this.widget.getRectCallback(referenceBox) : null;
            BorderRadius borderRadius = this.widget.borderRadius;
            ShapeBorder customBorder = this.widget.customBorder;

            InteractiveInkFeature splash = null;

            void OnRemoved() {
                if (this._splashes != null) {
                    D.assert(this._splashes.Contains(splash));
                    this._splashes.Remove(splash);
                    if (this._currentSplash == splash) {
                        this._currentSplash = null;
                    }

                    this.updateKeepAlive();
                }
            }

            splash = (this.widget.splashFactory ?? Theme.of(this.context).splashFactory).create(
                controller: inkController,
                referenceBox: referenceBox,
                position: position,
                color: color,
                containedInkWell: this.widget.containedInkWell,
                rectCallback: rectCallback,
                radius: this.widget.radius,
                borderRadius: borderRadius,
                customBorder: customBorder,
                onRemoved: OnRemoved);

            return splash;
        }


        void _handleTapDown(TapDownDetails details) {
            InteractiveInkFeature splash = this._createInkFeature(details);
            this._splashes = this._splashes ?? new HashSet<InteractiveInkFeature>();
            this._splashes.Add(splash);
            this._currentSplash = splash;
            if (this.widget.onTapDown != null) {
                this.widget.onTapDown(details);
            }

            this.updateKeepAlive();
            this.updateHighlight(true);
        }

        void _handleTap(BuildContext context) {
            this._currentSplash?.confirm();
            this._currentSplash = null;
            this.updateHighlight(false);
            if (this.widget.onTap != null) {
                this.widget.onTap();
            }
        }

        void _handleTapCancel() {
            this._currentSplash?.cancel();
            this._currentSplash = null;
            if (this.widget.onTapCancel != null) {
                this.widget.onTapCancel();
            }

            this.updateHighlight(false);
        }

        void _handleDoubleTap() {
            this._currentSplash?.confirm();
            this._currentSplash = null;
            if (this.widget.onDoubleTap != null) {
                this.widget.onDoubleTap();
            }
        }

        void _handleLongPress(BuildContext context) {
            this._currentSplash?.confirm();
            this._currentSplash = null;
            if (this.widget.onLongPress != null) {
                this.widget.onLongPress();
            }
        }

        public override void deactivate() {
            if (this._splashes != null) {
                HashSet<InteractiveInkFeature> splashes = this._splashes;
                this._splashes = null;
                foreach (InteractiveInkFeature splash in splashes) {
                    splash.dispose();
                }

                this._currentSplash = null;
            }

            D.assert(this._currentSplash == null);
            this._lastHighlight?.dispose();
            this._lastHighlight = null;
            base.deactivate();
        }

        public override Widget build(BuildContext context) {
            D.assert(this.widget.debugCheckContext(context));
            base.build(context);
            ThemeData themeData = Theme.of(context);
            if (this._lastHighlight != null) {
                this._lastHighlight.color = this.widget.highlightColor ?? themeData.highlightColor;
            }

            if (this._currentSplash != null) {
                this._currentSplash.color = this.widget.splashColor ?? themeData.splashColor;
            }

            bool enabled = this.widget.onTap != null || this.widget.onDoubleTap != null ||
                           this.widget.onLongPress != null;

            return new GestureDetector(
                onTapDown: enabled ? (GestureTapDownCallback) this._handleTapDown : null,
                onTap: enabled ? (GestureTapCallback) (() => this._handleTap(context)) : null,
                onTapCancel: enabled ? (GestureTapCancelCallback) this._handleTapCancel : null,
                onDoubleTap: this.widget.onDoubleTap != null
                    ? (GestureDoubleTapCallback) (details => this._handleDoubleTap())
                    : null,
                onLongPress: this.widget.onLongPress != null
                    ? (GestureLongPressCallback) (() => this._handleLongPress(context))
                    : null,
                behavior: HitTestBehavior.opaque,
                child: this.widget.child
            );
        }
    }


    public class InkWell : InkResponse {
        public InkWell(
            Key key = null,
            Widget child = null,
            GestureTapCallback onTap = null,
            GestureTapCallback onDoubleTap = null,
            GestureLongPressCallback onLongPress = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapCancelCallback onTapCancel = null,
            ValueChanged<bool> onHighlightChanged = null,
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null,
            float? radius = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null
        ) : base(
            key: key,
            child: child,
            onTap: onTap,
            onDoubleTap: onDoubleTap,
            onLongPress: onLongPress,
            onTapDown: onTapDown,
            onTapCancel: () => {
                if (onTapCancel != null) {
                    onTapCancel();
                }
            },
            onHighlightChanged: onHighlightChanged,
            containedInkWell: true,
            highlightShape: BoxShape.rectangle,
            highlightColor: highlightColor,
            splashColor: splashColor,
            splashFactory: splashFactory,
            radius: radius,
            borderRadius: borderRadius,
            customBorder: customBorder) {
        }
    }
}