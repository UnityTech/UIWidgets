using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public class TapDownDetails {
        public TapDownDetails(Offset globalPosition = null) {
            this.globalPosition = globalPosition ?? Offset.zero;
        }

        public readonly Offset globalPosition;
    }

    public delegate void GestureTapDownCallback(TapDownDetails details);

    public class TapUpDetails {
        public TapUpDetails(Offset globalPosition = null) {
            this.globalPosition = globalPosition ?? Offset.zero;
        }

        public readonly Offset globalPosition;
    }

    public delegate void GestureTapUpCallback(TapUpDetails details);

    public delegate void GestureTapCallback();

    public delegate void GestureTapCancelCallback();

    public class TapGestureRecognizer : PrimaryPointerGestureRecognizer {
        public TapGestureRecognizer(object debugOwner = null)
            : base(deadline: Constants.kPressTimeout, debugOwner: debugOwner) { }

        public GestureTapDownCallback onTapDown;

        public GestureTapUpCallback onTapUp;

        public GestureTapCallback onTap;

        public GestureTapCancelCallback onTapCancel;

        bool _sentTapDown = false;

        bool _wonArenaForPrimaryPointer = false;

        Offset _finalPosition;

        protected override void handlePrimaryPointer(PointerEvent evt) {
            if (evt is PointerUpEvent) {
                this._finalPosition = evt.position;

                if (this._wonArenaForPrimaryPointer) {
                    this.resolve(GestureDisposition.accepted);
                    this._checkUp();
                }
            }
            else if (evt is PointerCancelEvent) {
                if (this._sentTapDown && this.onTapCancel != null) {
                    this.invokeCallback<object>("onTapCancel", () => this.onTapCancel);
                }

                this._reset();
            }
        }

        protected override void resolve(GestureDisposition disposition) {
            if (this._wonArenaForPrimaryPointer && disposition == GestureDisposition.rejected) {
                D.assert(this._sentTapDown);
                if (this.onTapCancel != null) {
                    this.invokeCallback<object>("spontaneous onTapCancel", () => {
                        this.onTapCancel();
                        return null;
                    });
                }

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
                if (this._sentTapDown && this.onTapCancel != null) {
                    this.invokeCallback<object>("forced onTapCancel", () => {
                        this.onTapCancel();
                        return null;
                    });
                }

                this._reset();
            }
        }

        void _checkDown() {
            if (!this._sentTapDown) {
                if (this.onTapDown != null) {
                    this.invokeCallback<object>("onTapDown", () => {
                        this.onTapDown(new TapDownDetails(globalPosition: this.initialPosition));
                        return null;
                    });
                }

                this._sentTapDown = true;
            }
        }

        void _checkUp() {
            if (this._finalPosition != null) {
                if (this.onTapUp != null) {
                    this.invokeCallback<object>("onTapUp", () => {
                        this.onTapUp(new TapUpDetails(globalPosition: this._finalPosition));
                        return null;
                    });
                }

                if (this.onTap != null) {
                    this.invokeCallback<object>("onTap", () => {
                        this.onTap();
                        return null;
                    });
                }

                this._reset();
            }
        }

        void _reset() {
            this._sentTapDown = false;
            this._wonArenaForPrimaryPointer = false;
            this._finalPosition = null;
        }

        public override string debugDescription {
            get { return "tap"; }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("wonArenaForPrimaryPointer",
                value: this._wonArenaForPrimaryPointer,
                ifTrue: "won arena"));
            properties.add(new DiagnosticsProperty<Offset>("finalPosition",
                this._finalPosition, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new FlagProperty("sentTapDown",
                value: this._sentTapDown, ifTrue: "sent tap down"));
        }
    }
}