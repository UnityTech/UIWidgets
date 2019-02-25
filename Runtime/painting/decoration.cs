using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public abstract class Decoration : Diagnosticable {
        protected Decoration() {
        }

        public override string toStringShort() {
            return this.GetType().ToString();
        }

        public virtual bool debugAssertIsValid() {
            return true;
        }

        public virtual EdgeInsets padding {
            get { return EdgeInsets.zero; }
        }

        public virtual bool isComplex {
            get { return false; }
        }

        public virtual Decoration lerpFrom(Decoration a, float t) {
            return null;
        }

        public virtual Decoration lerpTo(Decoration b, float t) {
            return null;
        }

        public static Decoration lerp(Decoration a, Decoration b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b.lerpFrom(null, t) ?? b;
            }

            if (b == null) {
                return a.lerpTo(null, t) ?? a;
            }

            if (t == 0.0) {
                return a;
            }

            if (t == 1.0) {
                return b;
            }

            return b.lerpFrom(a, t)
                   ?? a.lerpTo(b, t)
                   ?? (t < 0.5 ? (a.lerpTo(null, t * 2.0f) ?? a) : (b.lerpFrom(null, (t - 0.5f) * 2.0f) ?? b));
        }

        public virtual bool hitTest(Size size, Offset position) {
            return true;
        }

        public abstract BoxPainter createBoxPainter(VoidCallback onChanged = null);
    }

    public abstract class BoxPainter : IDisposable {
        protected BoxPainter(VoidCallback onChanged = null) {
            this.onChanged = onChanged;
        }

        public readonly VoidCallback onChanged;

        public abstract void paint(Canvas canvas, Offset offset, ImageConfiguration configuration);

        public virtual void Dispose() {
        }
    }
}