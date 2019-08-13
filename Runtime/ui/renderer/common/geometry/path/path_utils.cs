using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public enum uiPathWinding {
        counterClockwise = 1, // which just means the order as the input is.
        clockwise = 2, // which just means the reversed order.
    }

    enum uiPathCommand {
        moveTo,
        lineTo,
        bezierTo,
        close,
        winding,
    }

    [Flags]
    enum uiPointFlags {
        corner = 0x01,
        left = 0x02,
        bevel = 0x04,
        innerBevel = 0x08,
    }

    struct uiPathPoint {
        public float x, y;
        public float dx, dy;
        public float len;
        public float dmx, dmy;
        public uiPointFlags flags;

        public static uiPathPoint create(float x = 0, float y = 0, float dx = 0, float dy = 0, float len = 0,
            float dmx = 0, float dmy = 0,
            uiPointFlags flags = uiPointFlags.corner) {
            uiPathPoint newPoint = new uiPathPoint();
            newPoint.x = x;
            newPoint.y = y;
            newPoint.dx = dx;
            newPoint.dy = dy;
            newPoint.len = len;
            newPoint.dmx = dmx;
            newPoint.dmy = dmy;
            newPoint.flags = flags;
            return newPoint;
        }
    }

    struct uiPathPath {
        public int first;
        public int count;
        public bool closed;
        public int ifill;
        public int nfill;
        public int istroke;
        public int nstroke;
        public uiPathWinding winding;
        public bool convex;

        public static uiPathPath create(int first = 0, int count = 0, bool closed = false, int ifill = 0, int nfill = 0,
            int istroke = 0,
            int nstroke = 0, uiPathWinding winding = uiPathWinding.counterClockwise, bool convex = false) {
            uiPathPath newPath = new uiPathPath();
            newPath.first = first;
            newPath.count = count;
            newPath.closed = closed;
            newPath.ifill = ifill;
            newPath.nfill = nfill;
            newPath.istroke = istroke;
            newPath.nstroke = nstroke;
            newPath.winding = winding;
            newPath.convex = convex;

            return newPath;
        }
    }

    struct uiVertexUV {
        public uiList<Vector3> fillVertices;
        public uiList<Vector2> fillUV;
        public uiList<Vector3> strokeVertices;
        public uiList<Vector2> strokeUV;
    }

    static class uiPathUtils {
        public static bool ptEquals(float x1, float y1, float x2, float y2, float tol) {
            float dx = x2 - x1;
            float dy = y2 - y1;

            if (dx <= -tol || dx >= tol || dy <= -tol || dy >= tol) {
                return false;
            }

            return dx * dx + dy * dy < tol * tol;
        }

        public static void polyReverse(List<uiPathPoint> pts, int s, int npts) {
            int i = s, j = s + npts - 1;
            while (i < j) {
                var tmp = pts[i];
                pts[i] = pts[j];
                pts[j] = tmp;
                i++;
                j--;
            }
        }

        public static float normalize(ref float x, ref float y) {
            float d = Mathf.Sqrt(x * x + y * y);
            if (d > 1e-6f) {
                float id = 1.0f / d;
                x *= id;
                y *= id;
            }

            return d;
        }

        public static void buttCapStart(this uiList<Vector3> dst, uiList<Vector2> uv, uiPathPoint p,
            float dx, float dy, float w, float d, float aa, float u0, float u1) {
            float px = p.x - dx * d;
            float py = p.y - dy * d;
            float dlx = dy;
            float dly = -dx;

            dst.Add(new Vector2(px + dlx * w - dx * aa, py + dly * w - dy * aa));
            dst.Add(new Vector2(px - dlx * w - dx * aa, py - dly * w - dy * aa));
            dst.Add(new Vector2(px + dlx * w, py + dly * w));
            dst.Add(new Vector2(px - dlx * w, py - dly * w));
            uv.Add(new Vector2(u0, 0));
            uv.Add(new Vector2(u1, 0));
            uv.Add(new Vector2(u0, 1));
            uv.Add(new Vector2(u1, 1));
        }

        public static void buttCapEnd(this uiList<Vector3> dst, uiList<Vector2> uv, uiPathPoint p,
            float dx, float dy, float w, float d, float aa, float u0, float u1) {
            float px = p.x + dx * d;
            float py = p.y + dy * d;
            float dlx = dy;
            float dly = -dx;

            dst.Add(new Vector2(px + dlx * w, py + dly * w));
            dst.Add(new Vector2(px - dlx * w, py - dly * w));
            dst.Add(new Vector2(px + dlx * w + dx * aa, py + dly * w + dy * aa));
            dst.Add(new Vector2(px - dlx * w + dx * aa, py - dly * w + dy * aa));
            uv.Add(new Vector2(u0, 1));
            uv.Add(new Vector2(u1, 1));
            uv.Add(new Vector2(u0, 0));
            uv.Add(new Vector2(u1, 0));
        }

        public static void roundCapStart(this uiList<Vector3> dst, uiList<Vector2> uv, uiPathPoint p,
            float dx, float dy, float w, int ncap, float u0, float u1) {
            float px = p.x;
            float py = p.y;
            float dlx = dy;
            float dly = -dx;

            for (var i = 0; i < ncap; i++) {
                float a = (float) i / (ncap - 1) * Mathf.PI;
                float ax = Mathf.Cos(a) * w, ay = Mathf.Sin(a) * w;
                dst.Add(new Vector2(px - dlx * ax - dx * ay, py - dly * ax - dy * ay));
                dst.Add(new Vector2(px, py));
                uv.Add(new Vector2(u0, 1));
                uv.Add(new Vector2(0.5f, 1));
            }

            dst.Add(new Vector2(px + dlx * w, py + dly * w));
            dst.Add(new Vector2(px - dlx * w, py - dly * w));
            uv.Add(new Vector2(u0, 1));
            uv.Add(new Vector2(u1, 1));
        }

        public static void roundCapEnd(this uiList<Vector3> dst, uiList<Vector2> uv, uiPathPoint p,
            float dx, float dy, float w, int ncap, float u0, float u1) {
            float px = p.x;
            float py = p.y;
            float dlx = dy;
            float dly = -dx;

            dst.Add(new Vector2(px + dlx * w, py + dly * w));
            dst.Add(new Vector2(px - dlx * w, py - dly * w));
            uv.Add(new Vector2(u0, 1));
            uv.Add(new Vector2(u1, 1));

            for (var i = 0; i < ncap; i++) {
                float a = (float) i / (ncap - 1) * Mathf.PI;
                float ax = Mathf.Cos(a) * w, ay = Mathf.Sin(a) * w;
                dst.Add(new Vector2(px, py));
                dst.Add(new Vector2(px - dlx * ax + dx * ay, py - dly * ax + dy * ay));
                uv.Add(new Vector2(0.5f, 1));
                uv.Add(new Vector2(u0, 1));
            }
        }

        public static void chooseBevel(bool bevel, uiPathPoint p0, uiPathPoint p1, float w,
            out float x0, out float y0, out float x1, out float y1) {
            if (bevel) {
                x0 = p1.x + p0.dy * w;
                y0 = p1.y - p0.dx * w;
                x1 = p1.x + p1.dy * w;
                y1 = p1.y - p1.dx * w;
            }
            else {
                x0 = p1.x + p1.dmx * w;
                y0 = p1.y + p1.dmy * w;
                x1 = p1.x + p1.dmx * w;
                y1 = p1.y + p1.dmy * w;
            }
        }

        public static int curveDivs(float r, float arc, float tol) {
            float da = Mathf.Acos(r / (r + tol)) * 2.0f;
            return Mathf.Max(2, Mathf.CeilToInt(arc / da));
        }

        public static void roundJoin(this uiList<Vector3> dst, uiList<Vector2> uv, uiPathPoint p0, uiPathPoint p1,
            float lw, float rw, int ncap, float lu, float ru, float fringe) {
            float dlx0 = p0.dy;
            float dly0 = -p0.dx;
            float dlx1 = p1.dy;
            float dly1 = -p1.dx;

            if ((p1.flags & uiPointFlags.left) != 0) {
                float lx0, ly0, lx1, ly1;
                chooseBevel((p1.flags & uiPointFlags.innerBevel) != 0, p0, p1, lw,
                    out lx0, out ly0, out lx1, out ly1);

                float a0 = Mathf.Atan2(-dly0, -dlx0);
                float a1 = Mathf.Atan2(-dly1, -dlx1);
                if (a1 > a0) {
                    a1 -= Mathf.PI * 2;
                }

                dst.Add(new Vector2(lx0, ly0));
                dst.Add(new Vector2(p1.x - dlx0 * rw, p1.y - dly0 * rw));
                uv.Add(new Vector2(lu, 1));
                uv.Add(new Vector2(ru, 1));

                var n = Mathf.CeilToInt((a0 - a1) / Mathf.PI * ncap).clamp(2, ncap);
                for (var i = 0; i < n; i++) {
                    float u = (float) i / (n - 1);
                    float a = a0 + u * (a1 - a0);
                    float rx = p1.x + Mathf.Cos(a) * rw;
                    float ry = p1.y + Mathf.Sin(a) * rw;

                    dst.Add(new Vector2(p1.x, p1.y));
                    dst.Add(new Vector2(rx, ry));
                    uv.Add(new Vector2(0.5f, 1));
                    uv.Add(new Vector2(ru, 1));
                }

                dst.Add(new Vector2(lx1, ly1));
                dst.Add(new Vector2(p1.x - dlx1 * rw, p1.y - dly1 * rw));
                uv.Add(new Vector2(lu, 1));
                uv.Add(new Vector2(ru, 1));
            }
            else {
                float rx0, ry0, rx1, ry1;
                chooseBevel((p1.flags & uiPointFlags.innerBevel) != 0, p0, p1, -rw,
                    out rx0, out ry0, out rx1, out ry1);

                float a0 = Mathf.Atan2(dly0, dlx0);
                float a1 = Mathf.Atan2(dly1, dlx1);
                if (a1 < a0) {
                    a1 += Mathf.PI * 2;
                }

                dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                dst.Add(new Vector2(rx0, ry0));
                uv.Add(new Vector2(lu, 1));
                uv.Add(new Vector2(ru, 1));

                var n = Mathf.CeilToInt((a1 - a0) / Mathf.PI * ncap).clamp(2, ncap);
                for (var i = 0; i < n; i++) {
                    float u = (float) i / (n - 1);
                    float a = a0 + u * (a1 - a0);
                    float lx = p1.x + Mathf.Cos(a) * lw;
                    float ly = p1.y + Mathf.Sin(a) * lw;

                    dst.Add(new Vector2(lx, ly));
                    dst.Add(new Vector2(p1.x, p1.y));
                    uv.Add(new Vector2(lu, 1));
                    uv.Add(new Vector2(0.5f, 1));
                }

                dst.Add(new Vector2(p1.x + dlx1 * lw, p1.y + dly1 * lw));
                dst.Add(new Vector2(rx1, ry1));
                uv.Add(new Vector2(lu, 1));
                uv.Add(new Vector2(ru, 1));
            }
        }

        public static void bevelJoin(this uiList<Vector3> dst, uiList<Vector2> uv, uiPathPoint p0, uiPathPoint p1,
            float lw, float rw, float lu, float ru, float fringe) {
            float rx0, ry0, rx1, ry1;
            float lx0, ly0, lx1, ly1;

            float dlx0 = p0.dy;
            float dly0 = -p0.dx;
            float dlx1 = p1.dy;
            float dly1 = -p1.dx;

            if ((p1.flags & uiPointFlags.left) != 0) {
                chooseBevel((p1.flags & uiPointFlags.innerBevel) != 0, p0, p1, lw,
                    out lx0, out ly0, out lx1, out ly1);

                dst.Add(new Vector2 {x = lx0, y = ly0});
                dst.Add(new Vector2 {x = p1.x - dlx0 * rw, y = p1.y - dly0 * rw});
                uv.Add(new Vector2(lu, 1));
                uv.Add(new Vector2(ru, 1));

                if ((p1.flags & uiPointFlags.bevel) != 0) {
                    dst.Add(new Vector2(lx0, ly0));
                    dst.Add(new Vector2(p1.x - dlx0 * rw, p1.y - dly0 * rw));
                    dst.Add(new Vector2(lx1, ly1));
                    dst.Add(new Vector2(p1.x - dlx1 * rw, p1.y - dly1 * rw));
                    uv.Add(new Vector2(lu, 1));
                    uv.Add(new Vector2(ru, 1));
                    uv.Add(new Vector2(lu, 1));
                    uv.Add(new Vector2(ru, 1));
                }
                else {
                    rx0 = p1.x - p1.dmx * rw;
                    ry0 = p1.y - p1.dmy * rw;
                    dst.Add(new Vector2(p1.x, p1.y));
                    dst.Add(new Vector2(p1.x - dlx0 * rw, p1.y - dly0 * rw));
                    dst.Add(new Vector2(rx0, ry0));
                    dst.Add(new Vector2(rx0, ry0));
                    dst.Add(new Vector2(p1.x, p1.y));
                    dst.Add(new Vector2(p1.x - dlx1 * rw, p1.y - dly1 * rw));
                    uv.Add(new Vector2(0.5f, 1));
                    uv.Add(new Vector2(ru, 1));
                    uv.Add(new Vector2(ru, 1));
                    uv.Add(new Vector2(ru, 1));
                    uv.Add(new Vector2(0.5f, 1));
                    uv.Add(new Vector2(ru, 1));
                }

                dst.Add(new Vector2(lx1, ly1));
                dst.Add(new Vector2(p1.x - dlx1 * rw, p1.y - dly1 * rw));
                uv.Add(new Vector2(lu, 1));
                uv.Add(new Vector2(ru, 1));
            }
            else {
                chooseBevel((p1.flags & uiPointFlags.innerBevel) != 0, p0, p1, -rw,
                    out rx0, out ry0, out rx1, out ry1);

                dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                dst.Add(new Vector2(rx0, ry0));
                uv.Add(new Vector2(lu, 1));
                uv.Add(new Vector2(ru, 1));

                if ((p1.flags & uiPointFlags.bevel) != 0) {
                    dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                    dst.Add(new Vector2(rx0, ry0));
                    dst.Add(new Vector2(p1.x + dlx1 * lw, p1.y + dly1 * lw));
                    dst.Add(new Vector2(rx1, ry1));
                    uv.Add(new Vector2(lu, 1));
                    uv.Add(new Vector2(ru, 1));
                    uv.Add(new Vector2(lu, 1));
                    uv.Add(new Vector2(ru, 1));
                }
                else {
                    lx0 = p1.x + p1.dmx * lw;
                    ly0 = p1.y + p1.dmy * lw;
                    dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                    dst.Add(new Vector2(p1.x, p1.y));
                    dst.Add(new Vector2(lx0, ly0));
                    dst.Add(new Vector2(lx0, ly0));
                    dst.Add(new Vector2(p1.x + dlx1 * lw, p1.y + dly1 * lw));
                    dst.Add(new Vector2(p1.x, p1.y));
                    uv.Add(new Vector2(lu, 1));
                    uv.Add(new Vector2(0.5f, 1));
                    uv.Add(new Vector2(lu, 1));
                    uv.Add(new Vector2(lu, 1));
                    uv.Add(new Vector2(lu, 1));
                    uv.Add(new Vector2(0.5f, 1));
                }

                dst.Add(new Vector2(p1.x + dlx1 * lw, p1.y + dly1 * lw));
                dst.Add(new Vector2(rx1, ry1));
                uv.Add(new Vector2(lu, 1));
                uv.Add(new Vector2(ru, 1));
            }
        }
    }
}