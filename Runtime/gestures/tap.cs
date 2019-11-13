using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public class TapDownDetails {
        public TapDownDetails(Offset globalPosition = null,
            Offset localPosition = null,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0) {
            this.globalPosition = globalPosition ?? Offset.zero;
            this.localPosition = localPosition ?? this.globalPosition;
            this.kind = kind;
            this.device = device;
        }

        public readonly Offset globalPosition;

        public readonly Offset localPosition;

        public readonly PointerDeviceKind kind;

        public readonly int device;
    }

    public delegate void GestureTapDownCallback(TapDownDetails details);

    public class TapUpDetails {
        public TapUpDetails(Offset globalPosition = null,
            Offset localPosition = null,
            PointerDeviceKind kind = PointerDeviceKind.touch,
            int device = 0) {
            this.globalPosition = globalPosition ?? Offset.zero;
            this.localPosition = localPosition ?? this.globalPosition;
            this.kind = kind;
            this.device = device;
        }

        public readonly Offset globalPosition;
        
        public readonly Offset localPosition;

        public readonly PointerDeviceKind kind;

        public readonly int device;
    }

    public delegate void GestureTapUpCallback(TapUpDetails details);

    public delegate void GestureTapCallback();

    public delegate void GestureTapCancelCallback();


    public abstract class BaseTapGestureRecognizer : PrimaryPointerGestureRecognizer {
        public BaseTapGestureRecognizer(object debugOwner = null) 
        : base(deadline: Constants.kPressTimeout, debugOwner: debugOwner) {
            
        }

        bool _sentTapDown = false;
        bool _wonArenaForPrimaryPointer = false;

        PointerDownEvent _down;
        PointerUpEvent _up;

        protected abstract void handleTapDown(PointerDownEvent down);

        protected abstract void handleTapUp(PointerDownEvent down, PointerUpEvent up);

        protected abstract void handleTapCancel(PointerDownEvent down, PointerCancelEvent cancel, string reason);

        public override void addAllowedPointer(PointerDownEvent evt) {
            if (this.state == GestureRecognizerState.ready) {
                this._down = evt;
            }
            base.addAllowedPointer(evt);
        }

        protected override void handlePrimaryPointer(PointerEvent evt) {
            if (evt is PointerUpEvent) {
                this._up = (PointerUpEvent) evt;
                this._checkUp();
            } else if (evt is PointerCancelEvent) {
                this.resolve(GestureDisposition.rejected);
                if (this._sentTapDown) {
                    this._checkCancel((PointerCancelEvent) evt, "");
                }

                this._reset();
            } else if (evt.buttons != this._down?.buttons) {
                this.resolve(GestureDisposition.rejected);
                this.stopTrackingPointer(this.primaryPointer);
            }
        }

        protected override void resolve(GestureDisposition disposition) {
            if (this._wonArenaForPrimaryPointer && disposition == GestureDisposition.rejected) {
                D.assert(this._sentTapDown);
                this._checkCancel(null, "spontaneous");
                this._reset();
            }
            
            base.resolve(disposition);
        }

        protected override void didExceedDeadline() {
            this._checkDown();
        }

        public override void acceptGesture(int pointer) {
            base.acceptGesture(pointer);

            if (pointer == this.primaryPointer) {
                this._checkDown();
                this._wonArenaForPrimaryPointer = true;
                this._checkUp();
            }
        }

        public override void rejectGesture(int pointer) {
            base.rejectGesture(pointer);
            if (pointer == this.primaryPointer) {
                D.assert(this.state != GestureRecognizerState.possible);
                if (this._sentTapDown) {
                    this._checkCancel(null, "forced");
                }

                this._reset();
            }
        }

        void _checkDown() {
            if (this._sentTapDown) {
                return;
            }
            
            this.handleTapDown(down: this._down);
            this._sentTapDown = true;
        }

        void _checkUp() {
            if (!this._wonArenaForPrimaryPointer || this._up == null) {
                return;
            }
            this.handleTapUp(down: this._down, up: this._up);
            this._reset();
        }

        void _checkCancel(PointerCancelEvent evt, string note) {
            this.handleTapCancel(down: this._down, cancel: evt, reason: note);
        }

        void _reset() {
            this._sentTapDown = false;
            this._wonArenaForPrimaryPointer = false;
            this._up = null;
            this._down = null;
        }
        
        public override string debugDescription {
            get { return "base tap"; }
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("wonArenaForPrimaryPointer", value: this._wonArenaForPrimaryPointer, ifTrue: "won arena"));
            properties.add(new DiagnosticsProperty<Offset>("finalPosition", this._up?.position, defaultValue: null));
            properties.add(new DiagnosticsProperty<Offset>("finalLocalPosition", this._up?.position, defaultValue: this._up?.position));
            properties.add(new DiagnosticsProperty<int>("button", this._down?.buttons?? 0, defaultValue: 0));
            properties.add(new FlagProperty("sentTapDown", value: this._sentTapDown, ifTrue: "sent tap down"));
        }
    }

    public class TapGestureRecognizer : BaseTapGestureRecognizer {
        public TapGestureRecognizer(object debugOwner = null) : base(debugOwner: debugOwner) {
        }
        
        public GestureTapDownCallback onTapDown;
        
        public GestureTapUpCallback onTapUp;
        
        public GestureTapCallback onTap;
        
        public GestureTapCancelCallback onTapCancel;
        
        public GestureTapDownCallback onSecondaryTapDown;
        
        public GestureTapUpCallback onSecondaryTapUp;
        
        public GestureTapCancelCallback onSecondaryTapCancel;

        protected override bool isPointerAllowed(PointerDownEvent evt) {
            if (this.onTapDown == null && 
                this.onTap == null && 
                this.onTapUp == null && 
                this.onTapCancel == null) {
                return false;
            }
            
            return base.isPointerAllowed(evt);
        }

        protected override void handleTapDown(PointerDownEvent down) {
            
            if (this.onTapDown != null) {
                TapDownDetails details = new TapDownDetails(
                    globalPosition: down.position,                
                    kind: down.kind,
                    device: down.device
                );

                    this.invokeCallback<object>("onTapDown", () => {
                        this.onTapDown(details);
                        return null;
                    });
            }
        }

        protected override void handleTapUp(PointerDownEvent down, PointerUpEvent up) {
            TapUpDetails details = new TapUpDetails(
                globalPosition: up.position,
                kind: up.kind,
                device: up.device
                );

            if (this.onTapUp != null) {
                this.invokeCallback<object>("onTapUp", () => {
                    this.onTapUp(details);
                    return null;
                });
            }

            if (this.onTap != null) {
                this.invokeCallback<object>("onTap", () => {
                    this.onTap();
                    return null;
                });
            }
        }

        protected override void handleTapCancel(PointerDownEvent down, PointerCancelEvent cancel, string note) {
            if (this.onTapCancel != null) {
                this.invokeCallback<object>("onTapCancel", () => {
                    this.onTapCancel();
                    return null;
                });
            }
        }
        
        public override string debugDescription {
            get { return "tap"; }
        }
    }
}