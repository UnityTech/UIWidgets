using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class Path {
        const float _KAPPA90 = 0.5522847493f;

        readonly List<float> _commands = new List<float>();
        float _commandx;
        float _commandy;

        PathCache _cache;

        internal PathCache flatten(float[] xform, float devicePixelRatio) {
            if (this._cache != null && this._cache.canReuse(xform, devicePixelRatio)) {
                return this._cache;
            }

            this._cache = new PathCache(xform, devicePixelRatio);

            var i = 0;
            while (i < this._commands.Count) {
                var cmd = (PathCommand) this._commands[i];
                switch (cmd) {
                    case PathCommand.moveTo:
                        this._cache.addPath();
                        this._cache.addPoint(this._commands[i + 1], this._commands[i + 2], PointFlags.corner);
                        i += 3;
                        break;
                    case PathCommand.lineTo:
                        this._cache.addPoint(this._commands[i + 1], this._commands[i + 2], PointFlags.corner);
                        i += 3;
                        break;
                    case PathCommand.bezierTo:
                        this._cache.tessellateBezier(
                            this._commands[i + 1], this._commands[i + 2],
                            this._commands[i + 3], this._commands[i + 4],
                            this._commands[i + 5], this._commands[i + 6], PointFlags.corner);
                        i += 7;
                        break;
                    case PathCommand.close:
                        this._cache.closePath();
                        i++;
                        break;
                    case PathCommand.winding:
                        this._cache.pathWinding((PathWinding) this._commands[i + 1]);
                        i += 2;
                        break;
                    default:
                        D.assert(false, "unknown cmd: " + cmd);
                        break;
                }
            }

            this._cache.normalize();
            return this._cache;
        }

        public void reset() {
            this._commands.Clear();
            this._commandx = 0;
            this._commandy = 0;
            this._cache = null;
        }

        void _appendCommands(float[] vals) {
            if (vals.Length == 1 && (PathCommand) vals[vals.Length - 1] == PathCommand.close) {
                // last command is close
            } else if (vals.Length == 2 && (PathCommand) vals[vals.Length - 2] == PathCommand.winding) {
                // last command is winding
            } else {
                D.assert(vals.Length >= 2);
                this._commandx = vals[vals.Length - 2];
                this._commandy = vals[vals.Length - 1];
            }

            this._commands.AddRange(vals);
            this._cache = null;
        }

        public void moveTo(double x, double y) {
            this._appendCommands(new[] {
                (float) PathCommand.moveTo,
                (float) x, (float) y,
            });
        }

        public void lineTo(double x, double y) {
            this._appendCommands(new[] {
                (float) PathCommand.lineTo,
                (float) x, (float) y,
            });
        }

        public void bezierTo(double c1x, double c1y, double c2x, double c2y, double x, double y) {
            this._appendCommands(new[] {
                (float) PathCommand.bezierTo,
                (float) c1x, (float) c1y, (float) c2x, (float) c2y, (float) x, (float) y,
            });
        }

        public void quadTo(double cx, double cy, double x, double y) {
            var x0 = this._commandx;
            var y0 = this._commandy;

            this._appendCommands(new[] {
                (float) PathCommand.bezierTo,
                (float) (x0 + 2.0f / 3.0f * (cx - x0)), (float) (y0 + 2.0f / 3.0f * (cy - y0)),
                (float) (x + 2.0f / 3.0f * (cx - x)), (float) (y + 2.0f / 3.0f * (cy - y)),
                (float) x, (float) y,
            });
        }

        public void close() {
            this._appendCommands(new[] {
                (float) PathCommand.close,
            });
        }

        public void winding(PathWinding dir) {
            this._appendCommands(new[] {
                (float) PathCommand.winding,
                (float) dir
            });
        }

        public void addRect(Rect rect) {
            this._appendCommands(new[] {
                (float) PathCommand.moveTo, (float) rect.left, (float) rect.top,
                (float) PathCommand.lineTo, (float) rect.left, (float) rect.bottom,
                (float) PathCommand.lineTo, (float) rect.right, (float) rect.bottom,
                (float) PathCommand.lineTo, (float) rect.right, (float) rect.top,
                (float) PathCommand.close
            });
        }

        public void addRRect(RRect rrect) {
            float w = (float) rrect.width;
            float h = (float) rrect.height;
            float halfw = Mathf.Abs(w) * 0.5f;
            float halfh = Mathf.Abs(h) * 0.5f;
            float rxBL = Mathf.Min((float) rrect.blRadius, halfw) * Mathf.Sign(w);
            float ryBL = Mathf.Min((float) rrect.blRadius, halfh) * Mathf.Sign(h);
            float rxBR = Mathf.Min((float) rrect.brRadius, halfw) * Mathf.Sign(w);
            float ryBR = Mathf.Min((float) rrect.brRadius, halfh) * Mathf.Sign(h);
            float rxTR = Mathf.Min((float) rrect.trRadius, halfw) * Mathf.Sign(w);
            float ryTR = Mathf.Min((float) rrect.trRadius, halfh) * Mathf.Sign(h);
            float rxTL = Mathf.Min((float) rrect.tlRadius, halfw) * Mathf.Sign(w);
            float ryTL = Mathf.Min((float) rrect.tlRadius, halfh) * Mathf.Sign(h);
            float x = (float) rrect.left;
            float y = (float) rrect.top;

            this._appendCommands(new[] {
                (float) PathCommand.moveTo, x, y + ryTL,
                (float) PathCommand.lineTo, x, y + h - ryBL,
                (float) PathCommand.bezierTo, x, y + h - ryBL * (1 - _KAPPA90),
                x + rxBL * (1 - _KAPPA90), y + h, x + rxBL, y + h,
                (float) PathCommand.lineTo, x + w - rxBR, y + h,
                (float) PathCommand.bezierTo, x + w - rxBR * (1 - _KAPPA90), y + h,
                x + w, y + h - ryBR * (1 - _KAPPA90), x + w, y + h - ryBR,
                (float) PathCommand.lineTo, x + w, y + ryTR,
                (float) PathCommand.bezierTo, x + w, y + ryTR * (1 - _KAPPA90),
                x + w - rxTR * (1 - _KAPPA90), y, x + w - rxTR, y,
                (float) PathCommand.lineTo, x + rxTL, y,
                (float) PathCommand.bezierTo, x + rxTL * (1 - _KAPPA90), y,
                x, y + ryTL * (1 - _KAPPA90), x, y + ryTL,
                (float) PathCommand.close,
            });
        }

        public void addEllipse(double cx, double cy, double rx, double ry) {
            this._appendCommands(new[] {
                (float) PathCommand.moveTo, (float) (cx - rx), (float) cy,
                (float) PathCommand.bezierTo, (float) (cx - rx), (float) (cy + ry * _KAPPA90),
                (float) (cx - rx * _KAPPA90), (float) (cy + ry), (float) cx, (float) (cy + ry),
                (float) PathCommand.bezierTo, (float) (cx + rx * _KAPPA90), (float) (cy + ry),
                (float) (cx + rx), (float) (cy + ry * _KAPPA90), (float) (cx + rx), (float) cy,
                (float) PathCommand.bezierTo, (float) (cx + rx), (float) (cy - ry * _KAPPA90),
                (float) (cx + rx * _KAPPA90), (float) (cy - ry), (float) cx, (float) (cy - ry),
                (float) PathCommand.bezierTo, (float) (cx - rx * _KAPPA90), (float) (cy - ry),
                (float) (cx - rx), (float) (cy - ry * _KAPPA90), (float) (cx - rx), (float) cy,
                (float) PathCommand.close,
            });
        }

        public void addCircle(double cx, double cy, double r) {
            this.addEllipse(cx, cy, r, r);
        }
    }

    public enum PathWinding {
        counterClockwise = 1,
        clockwise = 2,
    }

    [Flags]
    enum PointFlags {
        corner = 0x01,
        left = 0x02,
        bevel = 0x04,
        innerBevel = 0x08,
    }

    class PathPoint {
        public float x, y;
        public float dx, dy;
        public float len;
        public float dmx, dmy;
        public PointFlags flags;
    }

    enum PathCommand {
        moveTo,
        lineTo,
        bezierTo,
        close,
        winding,
    }

    class PathPath {
        public int first;
        public int count;
        public bool closed;
        public int ifill;
        public int nfill;
        public int istroke;
        public int nstroke;
        public PathWinding winding;
        public bool convex;
    }

    class PathCache {
        readonly float[] _xform;
        readonly float _devicePixelRatio;
        readonly float _distTol;
        readonly float _tessTol;

        readonly List<PathPath> _paths = new List<PathPath>();
        readonly List<PathPoint> _points = new List<PathPoint>();
        readonly List<Vector3> _vertices = new List<Vector3>();

        MeshMesh _fillMesh;
        bool _fillConvex;

        MeshMesh _strokeMesh;
        float _strokeWidth;
        StrokeCap _lineCap;
        StrokeJoin _lineJoin;
        float _miterLimit;

        public PathCache(float[] xform, float devicePixelRatio) {
            D.assert(xform != null && xform.Length == 6);

            this._xform = xform;
            this._devicePixelRatio = devicePixelRatio;
            this._distTol = 0.01f / devicePixelRatio;
            this._tessTol = 0.25f / devicePixelRatio;
        }

        public bool canReuse(float[] xform, float devicePixelRatio) {
            D.assert(xform != null && xform.Length == 6);

            for (var i = 0; i < 6; ++i) {
                if (this._xform[i] != xform[i]) {
                    return false;
                }
            }

            if (this._devicePixelRatio != devicePixelRatio) {
                return false;
            }

            return true;
        }

        public void addPath() {
            this._paths.Add(new PathPath {
                first = this._points.Count,
                winding = PathWinding.counterClockwise
            });
        }

        public void addPoint(float x, float y, PointFlags flags) {
            PathUtils.transformPoint(out x, out y, this._xform, x, y);

            this._addPoint(new PathPoint{x = x, y = y, flags = flags});
        }

        void _addPoint(PathPoint point) {
            if (this._paths.Count == 0) {
                this.addPath();
            }

            var path = this._paths.Last();
            if (path.count > 0) {
                var pt = this._points.Last();
                if (PathUtils.ptEquals(pt.x, pt.y, point.x, point.y, this._distTol)) {
                    pt.flags |= point.flags;
                    return;
                }
            }

            this._points.Add(point);
            path.count++;
        }

        public void tessellateBezier(
            float x2, float y2,
            float x3, float y3, float x4, float y4,
            PointFlags flags) {
            float x1, y1;
            if (this._points.Count == 0) {
                x1 = 0;
                y1 = 0;
            } else {
                var pt = this._points.Last();
                x1 = pt.x;
                y1 = pt.y;
            }

            PathUtils.transformPoint(out x2, out y2, this._xform, x2, y2);
            PathUtils.transformPoint(out x3, out y3, this._xform, x3, y3);
            PathUtils.transformPoint(out x4, out y4, this._xform, x4, y4);

            var points = TessellationGenerator.tessellateBezier(x1, y1, x2, y2, x3, y3, x4, y4, this._tessTol);
            points[points.Count - 1].flags = flags;
            foreach (var point in points) {
                this._addPoint(point);
            }
        }

        public void closePath() {
            if (this._paths.Count == 0) {
                return;
            }

            var path = this._paths.Last();
            path.closed = true;
        }

        public void pathWinding(PathWinding winding) {
            if (this._paths.Count == 0) {
                return;
            }

            var path = this._paths.Last();
            path.winding = winding;
        }

        public void normalize() {
            for (var j = 0; j < this._paths.Count; j++) {
                var path = this._paths[j];
                if (path.count <= 1) {
                    continue;
                }

                var ip0 = path.first + path.count - 1;
                var ip1 = path.first;

                var p0 = this._points[ip0];
                var p1 = this._points[ip1];
                if (PathUtils.ptEquals(p0.x, p0.y, p1.x, p1.y, this._distTol)) {
                    path.count--;
                    path.closed = true;
                }

                if (path.count > 2) {
                    var area = PathUtils.polyArea(this._points, path.first, path.count);
                    if (path.winding == PathWinding.counterClockwise && area < 0.0f ||
                        path.winding == PathWinding.clockwise && area > 0.0f) {
                        PathUtils.polyReverse(this._points, path.first, path.count);
                    }
                }
            }
        }

        void _expandFill() {
            for (var j = 0; j < this._paths.Count; j++) {
                var path = this._paths[j];
                if (path.count <= 2) {
                    continue;
                }

                var ip0 = path.first + path.count - 1;
                var ip1 = path.first;
                for (var i = 0; i < path.count; i++) {
                    var p0 = this._points[ip0];
                    var p1 = this._points[ip1];
                    p0.dx = p1.x - p0.x; // no need to normalize
                    p0.dy = p1.y - p0.y;
                    ip0 = ip1++;
                }

                path.convex = true;

                ip0 = path.first + path.count - 1;
                ip1 = path.first;
                for (var i = 0; i < path.count; i++) {
                    var p0 = this._points[ip0];
                    var p1 = this._points[ip1];

                    float cross = p1.dx * p0.dy - p0.dx * p1.dy;
                    if (cross < 0.0f) {
                        path.convex = false;
                    }

                    ip0 = ip1++;
                }
            }

            this._vertices.Clear();
            for (var i = 0; i < this._paths.Count; i++) {
                var path = this._paths[i];
                if (path.count <= 2) {
                    continue;
                }

                path.ifill = this._vertices.Count;
                for (var j = 0; j < path.count; j++) {
                    var p = this._points[path.first + j];
                    this._vertices.Add(new Vector2(p.x, p.y));
                }

                path.nfill = this._vertices.Count - path.ifill;
            }
        }

        public MeshMesh getFillMesh(out bool convex) {
            if (this._fillMesh != null) {
                convex = this._fillConvex;
                return this._fillMesh;
            }

            this._expandFill();

            var cindices = 0;
            for (var i = 0; i < this._paths.Count; i++) {
                var path = this._paths[i];
                if (path.count <= 2) {
                    continue;
                }

                if (path.nfill > 0) {
                    D.assert(path.nfill >= 2);
                    cindices += (path.nfill - 2) * 3;
                }
            }

            var indices = new List<int>(cindices);
            for (var i = 0; i < this._paths.Count; i++) {
                var path = this._paths[i];
                if (path.count <= 2) {
                    continue;
                }

                if (path.nfill > 0) {
                    for (var j = 2; j < path.nfill; j++) {
                        indices.Add(path.ifill);
                        indices.Add(path.ifill + j);
                        indices.Add(path.ifill + j - 1);
                    }
                }
            }

            D.assert(indices.Count == cindices);
            
            var mesh = new MeshMesh(this._vertices, indices);
            this._fillMesh = mesh;
            
            this._fillConvex = false;
            for (var i = 0; i < this._paths.Count; i++) {
                var path = this._paths[i];
                if (path.count <= 2) {
                    continue;
                }

                if (this._fillConvex) {
                    // if more than two paths, convex is false.
                    this._fillConvex = false;
                    break;
                }

                if (!path.convex) {
                    // if not convex, convex is false.
                    break;
                }

                this._fillConvex = true;
            }

            convex = this._fillConvex;
            return this._fillMesh;
        }

        void _calculateJoins(float w, StrokeJoin lineJoin, float miterLimit) {
            float iw = w > 0.0f ? 1.0f / w : 0.0f;

            for (var i = 0; i < this._paths.Count; i++) {
                var path = this._paths[i];
                if (path.count <= 1) {
                    continue;
                }

                var ip0 = path.first + path.count - 1;
                var ip1 = path.first;

                for (var j = 0; j < path.count; j++) {
                    var p0 = this._points[ip0];
                    var p1 = this._points[ip1];
                    p0.dx = p1.x - p0.x;
                    p0.dy = p1.y - p0.y;
                    p0.len = PathUtils.normalize(ref p0.dx, ref p0.dy);
                    ip0 = ip1++;
                }

                ip0 = path.first + path.count - 1;
                ip1 = path.first;
                for (var j = 0; j < path.count; j++) {
                    var p0 = this._points[ip0];
                    var p1 = this._points[ip1];
                    float dlx0 = p0.dy;
                    float dly0 = -p0.dx;
                    float dlx1 = p1.dy;
                    float dly1 = -p1.dx;

                    // Calculate extrusions
                    p1.dmx = (dlx0 + dlx1) * 0.5f;
                    p1.dmy = (dly0 + dly1) * 0.5f;
                    float dmr2 = p1.dmx * p1.dmx + p1.dmy * p1.dmy;
                    if (dmr2 > 0.000001f) {
                        float scale = 1.0f / dmr2;
                        if (scale > 600.0f) {
                            scale = 600.0f;
                        }

                        p1.dmx *= scale;
                        p1.dmy *= scale;
                    }

                    // Clear flags, but keep the corner.
                    p1.flags &= PointFlags.corner;

                    // Keep track of left turns.
                    float cross = p1.dx * p0.dy - p0.dx * p1.dy;
                    if (cross > 0.0f) {
                        p1.flags |= PointFlags.left;
                    }

                    // Calculate if we should use bevel or miter for inner join.
                    float limit = Mathf.Max(1.01f, Mathf.Min(p0.len, p1.len) * iw);
                    if (dmr2 * limit * limit < 1.0f) {
                        p1.flags |= PointFlags.innerBevel;
                    }

                    // Check to see if the corner needs to be beveled.
                    if ((p1.flags & PointFlags.corner) != 0) {
                        if (lineJoin == StrokeJoin.bevel ||
                            lineJoin == StrokeJoin.round || dmr2 * miterLimit * miterLimit < 1.0f) {
                            p1.flags |= PointFlags.bevel;
                        }
                    }

                    ip0 = ip1++;
                }
            }
        }

        void _expandStroke(float w, StrokeCap lineCap, StrokeJoin lineJoin, float miterLimit) {
            this._calculateJoins(w, lineJoin, miterLimit);

            int ncap = 0;
            if (lineCap == StrokeCap.round || lineJoin == StrokeJoin.round) {
                ncap = PathUtils.curveDivs(w, Mathf.PI, this._tessTol);
            }

            this._vertices.Clear();
            for (var i = 0; i < this._paths.Count; i++) {
                var path = this._paths[i];
                if (path.count <= 1) {
                    continue;
                }

                path.istroke = this._vertices.Count;

                int s, e, ip0, ip1;
                if (path.closed) {
                    ip0 = path.first + path.count - 1;
                    ip1 = path.first;
                    s = 0;
                    e = path.count;
                } else {
                    ip0 = path.first;
                    ip1 = path.first + 1;
                    s = 1;
                    e = path.count - 1;
                }

                var p0 = this._points[ip0];
                var p1 = this._points[ip1];

                if (!path.closed) {
                    if (lineCap == StrokeCap.butt) {
                        this._vertices.buttCapStart(p0, p0.dx, p0.dy, w, 0.0f);
                    } else if (lineCap == StrokeCap.square) {
                        this._vertices.buttCapStart(p0, p0.dx, p0.dy, w, w);
                    } else {
                        // round
                        this._vertices.roundCapStart(p0, p0.dx, p0.dy, w, ncap);
                    }
                }

                for (var j = s; j < e; j++) {
                    p0 = this._points[ip0];
                    p1 = this._points[ip1];

                    if ((p1.flags & (PointFlags.bevel | PointFlags.innerBevel)) != 0) {
                        if (lineJoin == StrokeJoin.round) {
                            this._vertices.roundJoin(p0, p1, w, w, ncap);
                        } else {
                            this._vertices.bevelJoin(p0, p1, w, w);
                        }
                    } else {
                        this._vertices.Add(new Vector2(p1.x + p1.dmx * w, p1.y + p1.dmy * w));
                        this._vertices.Add(new Vector2(p1.x - p1.dmx * w, p1.y - p1.dmy * w));
                    }

                    ip0 = ip1++;
                }

                if (!path.closed) {
                    p0 = this._points[ip0];
                    p1 = this._points[ip1];
                    if (lineCap == StrokeCap.butt) {
                        this._vertices.buttCapEnd(p1, p0.dx, p0.dy, w, 0.0f);
                    } else if (lineCap == StrokeCap.square) {
                        this._vertices.buttCapEnd(p1, p0.dx, p0.dy, w, w);
                    } else {
                        // round
                        this._vertices.roundCapEnd(p1, p0.dx, p0.dy, w, ncap);
                    }
                } else {
                    this._vertices.Add(this._vertices[path.istroke]);
                    this._vertices.Add(this._vertices[path.istroke + 1]);
                }

                path.nstroke = this._vertices.Count - path.istroke;
            }
        }

        public MeshMesh getStrokeMesh(float strokeWidth, StrokeCap lineCap, StrokeJoin lineJoin, float miterLimit) {
            if (this._strokeMesh != null &&
                this._strokeWidth == strokeWidth &&
                this._lineCap == lineCap &&
                this._lineJoin == lineJoin &&
                this._miterLimit == miterLimit) {
                return this._strokeMesh;
            }

            this._expandStroke(strokeWidth, lineCap, lineJoin, miterLimit);

            var cindices = 0;
            for (var i = 0; i < this._paths.Count; i++) {
                var path = this._paths[i];
                if (path.count <= 1) {
                    continue;
                }

                if (path.nstroke > 0) {
                    D.assert(path.nstroke >= 2);
                    cindices += (path.nstroke - 2) * 3;
                }
            }

            var indices = new List<int>(cindices);
            for (var i = 0; i < this._paths.Count; i++) {
                var path = this._paths[i];
                if (path.count <= 1) {
                    continue;
                }

                if (path.nstroke > 0) {
                    for (var j = 2; j < path.nstroke; j++) {
                        if ((j & 1) == 0) {
                            indices.Add(path.istroke + j - 1);
                            indices.Add(path.istroke + j - 2);
                            indices.Add(path.istroke + j);
                        } else {
                            indices.Add(path.istroke + j - 2);
                            indices.Add(path.istroke + j - 1);
                            indices.Add(path.istroke + j);
                        }
                    }
                }
            }

            D.assert(indices.Count == cindices);

            this._strokeMesh = new MeshMesh(this._vertices, indices);
            this._strokeWidth = strokeWidth;
            this._lineCap = lineCap;
            this._lineJoin = lineJoin;
            this._miterLimit = miterLimit;
            return this._strokeMesh;
        }
    }


    static class PathUtils {
        public static bool ptEquals(float x1, float y1, float x2, float y2, float tol) {
            float dx = x2 - x1;
            float dy = y2 - y1;

            if (dx <= -tol || dx >= tol || dy <= -tol || dy >= tol) {
                return false;
            }

            return dx * dx + dy * dy < tol * tol;
        }

        public static void transformPoint(out float dx, out float dy, float[] t, float sx, float sy) {
            dx = sx * t[0] + sy * t[2] + t[4];
            dy = sx * t[1] + sy * t[3] + t[5];
        }

        public static float triarea2(float ax, float ay, float bx, float by, float cx, float cy) {
            float abx = bx - ax;
            float aby = by - ay;
            float acx = cx - ax;
            float acy = cy - ay;
            return acx * aby - abx * acy;
        }

        public static float polyArea(List<PathPoint> points, int s, int npts) {
            float area = 0;
            for (var i = s + 2; i < s + npts; i++) {
                var a = points[s];
                var b = points[i - 1];
                var c = points[i];
                area += triarea2(a.x, a.y, b.x, b.y, c.x, c.y);
            }

            return area * 0.5f;
        }

        public static void polyReverse(List<PathPoint> pts, int s, int npts) {
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

        public static void buttCapStart(this List<Vector3> dst, PathPoint p,
            float dx, float dy, float w, float d) {
            float px = p.x - dx * d;
            float py = p.y - dy * d;
            float dlx = dy;
            float dly = -dx;

            dst.Add(new Vector2(px + dlx * w, py + dly * w));
            dst.Add(new Vector2(px - dlx * w, py - dly * w));
        }

        public static void buttCapEnd(this List<Vector3> dst, PathPoint p,
            float dx, float dy, float w, float d) {
            float px = p.x + dx * d;
            float py = p.y + dy * d;
            float dlx = dy;
            float dly = -dx;

            dst.Add(new Vector2(px + dlx * w, py + dly * w));
            dst.Add(new Vector2(px - dlx * w, py - dly * w));
        }

        public static void roundCapStart(this List<Vector3> dst, PathPoint p,
            float dx, float dy, float w, int ncap) {
            float px = p.x;
            float py = p.y;
            float dlx = dy;
            float dly = -dx;


            dst.Add(new Vector2(px + dlx * w, py + dly * w));
            dst.Add(new Vector2(px - dlx * w, py - dly * w));
            for (var i = 0; i < ncap; i++) {
                float a = i / (float) (ncap - 1) * Mathf.PI;
                float ax = Mathf.Cos(a) * w, ay = Mathf.Sin(a) * w;
                dst.Add(new Vector2(px, py));
                dst.Add(new Vector2(px - dlx * ax + dx * ay, py - dly * ax + dy * ay));
            }
        }

        public static void roundCapEnd(this List<Vector3> dst, PathPoint p,
            float dx, float dy, float w, int ncap) {
            float px = p.x;
            float py = p.y;
            float dlx = dy;
            float dly = -dx;

            for (var i = 0; i < ncap; i++) {
                float a = i / (float) (ncap - 1) * Mathf.PI;
                float ax = Mathf.Cos(a) * w, ay = Mathf.Sin(a) * w;
                dst.Add(new Vector2(px - dlx * ax - dx * ay, py - dly * ax - dy * ay));
                dst.Add(new Vector2(px, py));
            }

            dst.Add(new Vector2(px + dlx * w, py + dly * w));
            dst.Add(new Vector2(px - dlx * w, py - dly * w));
        }

        public static void chooseBevel(bool bevel, PathPoint p0, PathPoint p1, float w,
            out float x0, out float y0, out float x1, out float y1) {
            if (bevel) {
                x0 = p1.x + p0.dy * w;
                y0 = p1.y - p0.dx * w;
                x1 = p1.x + p1.dy * w;
                y1 = p1.y - p1.dx * w;
            } else {
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

        public static void roundJoin(this List<Vector3> dst, PathPoint p0, PathPoint p1,
            float lw, float rw, int ncap) {
            float dlx0 = p0.dy;
            float dly0 = -p0.dx;
            float dlx1 = p1.dy;
            float dly1 = -p1.dx;

            if ((p1.flags & PointFlags.left) != 0) {
                float lx0, ly0, lx1, ly1;
                chooseBevel((p1.flags & PointFlags.innerBevel) != 0, p0, p1, lw,
                    out lx0, out ly0, out lx1, out ly1);

                float a0 = Mathf.Atan2(-dly0, -dlx0);
                float a1 = Mathf.Atan2(-dly1, -dlx1);
                if (a1 > a0) {
                    a1 -= Mathf.PI * 2;
                }

                dst.Add(new Vector2(lx0, ly0));
                dst.Add(new Vector2(p1.x - dlx0 * rw, p1.y - dly0 * rw));

                var n = Mathf.CeilToInt((a0 - a1) / Mathf.PI * ncap).clamp(2, ncap);
                for (var i = 0; i < n; i++) {
                    float u = i / (float) (n - 1);
                    float a = a0 + u * (a1 - a0);
                    float rx = p1.x + Mathf.Cos(a) * rw;
                    float ry = p1.y + Mathf.Sin(a) * rw;

                    dst.Add(new Vector2(p1.x, p1.y));
                    dst.Add(new Vector2(rx, ry));
                }

                dst.Add(new Vector2(lx1, ly1));
                dst.Add(new Vector2(p1.x - dlx1 * rw, p1.y - dly1 * rw));
            } else {
                float rx0, ry0, rx1, ry1;
                chooseBevel((p1.flags & PointFlags.innerBevel) != 0, p0, p1, -rw,
                    out rx0, out ry0, out rx1, out ry1);

                float a0 = Mathf.Atan2(dly0, dlx0);
                float a1 = Mathf.Atan2(dly1, dlx1);
                if (a1 < a0) {
                    a1 += Mathf.PI * 2;
                }

                dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                dst.Add(new Vector2(rx0, ry0));

                var n = Mathf.CeilToInt((a1 - a0) / Mathf.PI * ncap).clamp(2, ncap);
                for (var i = 0; i < n; i++) {
                    float u = i / (float) (n - 1);
                    float a = a0 + u * (a1 - a0);
                    float lx = p1.x + Mathf.Cos(a) * lw;
                    float ly = p1.y + Mathf.Sin(a) * lw;

                    dst.Add(new Vector2(lx, ly));
                    dst.Add(new Vector2(p1.x, p1.y));
                }

                dst.Add(new Vector2(p1.x + dlx1 * lw, p1.y + dly1 * lw));
                dst.Add(new Vector2(rx1, ry1));
            }
        }

        public static void bevelJoin(this List<Vector3> dst, PathPoint p0, PathPoint p1,
            float lw, float rw) {
            float rx0, ry0, rx1, ry1;
            float lx0, ly0, lx1, ly1;

            float dlx0 = p0.dy;
            float dly0 = -p0.dx;
            float dlx1 = p1.dy;
            float dly1 = -p1.dx;

            if ((p1.flags & PointFlags.left) != 0) {
                chooseBevel((p1.flags & PointFlags.innerBevel) != 0, p0, p1, lw,
                    out lx0, out ly0, out lx1, out ly1);

                dst.Add(new Vector2 {x = lx0, y = ly0});
                dst.Add(new Vector2 {x = p1.x - dlx0 * rw, y = p1.y - dly0 * rw});

                if ((p1.flags & PointFlags.bevel) != 0) {
                    dst.Add(new Vector2(lx0, ly0));
                    dst.Add(new Vector2(p1.x - dlx0 * rw, p1.y - dly0 * rw));
                    dst.Add(new Vector2(lx1, ly1));
                    dst.Add(new Vector2(p1.x - dlx1 * rw, p1.y - dly1 * rw));
                } else {
                    rx0 = p1.x - p1.dmx * rw;
                    ry0 = p1.y - p1.dmy * rw;
                    dst.Add(new Vector2(p1.x, p1.y));
                    dst.Add(new Vector2(p1.x - dlx0 * rw, p1.y - dly0 * rw));
                    dst.Add(new Vector2(rx0, ry0));
                    dst.Add(new Vector2(rx0, ry0));
                    dst.Add(new Vector2(p1.x, p1.y));
                    dst.Add(new Vector2(p1.x - dlx1 * rw, p1.y - dly1 * rw));
                }

                dst.Add(new Vector2(lx1, ly1));
                dst.Add(new Vector2(p1.x - dlx1 * rw, p1.y - dly1 * rw));
            } else {
                chooseBevel((p1.flags & PointFlags.innerBevel) != 0, p0, p1, -rw,
                    out rx0, out ry0, out rx1, out ry1);

                dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                dst.Add(new Vector2(rx0, ry0));

                if ((p1.flags & PointFlags.bevel) != 0) {
                    dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                    dst.Add(new Vector2(rx0, ry0));
                    dst.Add(new Vector2(p1.x + dlx1 * lw, p1.y + dly1 * lw));
                    dst.Add(new Vector2(rx1, ry1));
                } else {
                    lx0 = p1.x + p1.dmx * lw;
                    ly0 = p1.y + p1.dmy * lw;
                    dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                    dst.Add(new Vector2(p1.x, p1.y));
                    dst.Add(new Vector2(lx0, ly0));
                    dst.Add(new Vector2(lx0, ly0));
                    dst.Add(new Vector2(p1.x + dlx1 * lw, p1.y + dly1 * lw));
                    dst.Add(new Vector2(p1.x, p1.y));
                }

                dst.Add(new Vector2(p1.x + dlx1 * lw, p1.y + dly1 * lw));
                dst.Add(new Vector2(rx1, ry1));
            }
        }
    }

    class MeshMesh {
        public readonly List<Vector3> vertices;
        public readonly List<int> triangles;
        public readonly List<Vector2> uv;
        public readonly Rect bounds;

        MeshMesh _boundsMesh;
        static readonly List<int> _boundsTriangles = new List<int>(6) {0, 2, 1, 1, 2, 3};

        public MeshMesh boundsMesh {
            get {
                if (this._boundsMesh != null) {
                    return this._boundsMesh;
                }

                this._boundsMesh = new MeshMesh(this.bounds);
                return this._boundsMesh;
            }
        }

        public MeshMesh(Rect rect) {
            this.vertices = new List<Vector3>(4) {
                new Vector3((float) rect.right, (float) rect.bottom),
                new Vector3((float) rect.right, (float) rect.top),
                new Vector3((float) rect.left, (float) rect.bottom),
                new Vector3((float) rect.left, (float) rect.top)
            };

            this.triangles = _boundsTriangles;
            this.bounds = rect;
            this._boundsMesh = this;
        }

        public MeshMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uv = null) {
            D.assert(vertices != null);
            D.assert(vertices.Count > 0);
            D.assert(triangles != null);
            D.assert(triangles.Count > 0);
            D.assert(uv == null || uv.Count == vertices.Count);

            this.vertices = vertices;
            this.triangles = triangles;
            this.uv = uv;
            
            double minX = vertices[0].x;
            double maxX = vertices[0].x;
            double minY = vertices[0].y;
            double maxY = vertices[0].y;

            for (int i = 1; i < vertices.Count; i++) {
                var vertex = vertices[i];
                if (vertex.x < minX) {
                    minX = vertex.x;
                }
                if (vertex.x > maxX) {
                    maxX = vertex.x;
                }
                if (vertex.y < minY) {
                    minY = vertex.y;
                }
                if (vertex.y > maxY) {
                    maxY = vertex.y;
                }
            }

            this.bounds = Rect.fromLTRB(minX, minY, maxX, maxY);
        }

        public MeshMesh transform(float[] xform) {
            var transVertices = new List<Vector3>(this.vertices.Count);

            foreach (var vertex in this.vertices) {
                float x, y;
                PathUtils.transformPoint(out x, out y, xform, vertex.x, vertex.y);
                transVertices.Add(new Vector3(x, y));
            }
            
            return new MeshMesh(transVertices, this.triangles, this.uv);
        }
    }

    public class MeshPool : IDisposable {
        readonly Queue<Mesh> _pool = new Queue<Mesh>();

        public Mesh getMesh() {
            if (this._pool.Count > 0) {
                var mesh = this._pool.Dequeue();
                D.assert(mesh);
                return mesh;
            } else {
                var mesh = new Mesh();
                mesh.hideFlags = HideFlags.HideAndDontSave;
                return mesh;
            }
        }

        public void returnMesh(Mesh mesh) {
            D.assert(mesh.hideFlags == HideFlags.HideAndDontSave);
            this._pool.Enqueue(mesh);
        }

        public void Dispose() {
            foreach (var mesh in this._pool) {
                ObjectUtils.SafeDestroy(mesh);
            }
            this._pool.Clear();
        }
    }
}
