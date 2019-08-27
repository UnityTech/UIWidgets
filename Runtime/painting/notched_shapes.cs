using System.Collections.Generic;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public abstract class NotchedShape {
        public NotchedShape() {
        }

        public abstract Path getOuterPath(Rect host, Rect guest);
    }

    public class CircularNotchedRectangle : NotchedShape {
        public CircularNotchedRectangle() {
        }

        public override Path getOuterPath(Rect host, Rect guest) {
            if (guest == null || !host.overlaps(guest)) {
                Path path = new Path();
                path.addRect(host);
                return path;
            }

            float notchRadius = guest.width / 2.0f;

            const float s1 = 15.0f;
            const float s2 = 1.0f;

            float r = notchRadius;
            float a = -1.0f * r - s2;
            float b = host.top - guest.center.dy;

            float n2 = Mathf.Sqrt(b * b * r * r * (a * a + b * b - r * r));
            float p2xA = ((a * r * r) - n2) / (a * a + b * b);
            float p2xB = ((a * r * r) + n2) / (a * a + b * b);
            float p2yA = Mathf.Sqrt(r * r - p2xA * p2xA);
            float p2yB = Mathf.Sqrt(r * r - p2xB * p2xB);

            Offset[] p = new Offset[6];

            p[0] = new Offset(a - s1, b);
            p[1] = new Offset(a, b);
            float cmp = b < 0 ? -1.0f : 1.0f;
            p[2] = cmp * p2yA > cmp * p2yB ? new Offset(p2xA, p2yA) : new Offset(p2xB, p2yB);

            p[3] = new Offset(-1.0f * p[2].dx, p[2].dy);
            p[4] = new Offset(-1.0f * p[1].dx, p[1].dy);
            p[5] = new Offset(-1.0f * p[0].dx, p[0].dy);

            for (int i = 0; i < p.Length; i += 1) {
                p[i] += guest.center;
            }

            Path ret = new Path();
            ret.moveTo(host.left, host.top);
            ret.lineTo(p[0].dx, p[0].dy);
            ret.quadraticBezierTo(p[1].dx, p[1].dy, p[2].dx, p[2].dy);
            ret.arcToPoint(
                p[3], 
                radius: Radius.circular(notchRadius), 
                clockwise: false);
            ret.quadraticBezierTo(p[4].dx, p[4].dy, p[5].dx, p[5].dy);
            ret.lineTo(host.right, host.top);
            ret.lineTo(host.right, host.bottom);
            ret.lineTo(host.left, host.bottom);
            ret.close();
            return ret;
        }
    }

    class AutomaticNotchedShape : NotchedShape {
        public AutomaticNotchedShape(ShapeBorder host, ShapeBorder guest = null) {
            this.host = host;
            this.guest = guest;
        }

        public readonly ShapeBorder host;
        public readonly ShapeBorder guest;

        public override Path getOuterPath(Rect hostRect, Rect guestRect) {
            Path hostPath = this.host.getOuterPath(hostRect);
            if (this.guest != null && guestRect != null) {
                Path guestPath = this.guest.getOuterPath(guestRect);
                return Path.combine(PathOperation.difference, hostPath, guestPath);
            }

            return hostPath;
        }
    }
}