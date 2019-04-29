using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class Path {
        const float _KAPPA90 = 0.5522847493f;

        readonly List<float> _commands = new List<float>();
        float _commandx;
        float _commandy;
        float _minX, _minY;
        float _maxX, _maxY;

        PathCache _cache;

        internal PathCache flatten(float scale) {
            if (this._cache != null && this._cache.canReuse(scale)) {
                return this._cache;
            }

            this._cache = new PathCache(scale);

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

        public Path() {
            this._reset();
        }

        public override string ToString() {
            var sb = new StringBuilder("Path: count = " + this._commands.Count);

            var i = 0;
            while (i < this._commands.Count) {
                var cmd = (PathCommand) this._commands[i];
                switch (cmd) {
                    case PathCommand.moveTo:
                        sb.Append(", moveTo(" + this._commands[i + 1] + ", " + this._commands[i + 2] + ")");
                        i += 3;
                        break;
                    case PathCommand.lineTo:
                        sb.Append(", lineTo(" + this._commands[i + 1] + ", " + this._commands[i + 2] + ")");
                        i += 3;
                        break;
                    case PathCommand.bezierTo:
                        sb.Append(", bezierTo(" + this._commands[i + 1] + ", " + this._commands[i + 2] +
                                  ", " + this._commands[i + 3] + ", " + this._commands[i + 4] +
                                  ", " + this._commands[i + 5] + ", " + this._commands[i + 6] + ")");
                        i += 7;
                        break;
                    case PathCommand.close:
                        sb.Append(", close()");
                        i++;
                        break;
                    case PathCommand.winding:
                        sb.Append(", winding(" + (PathWinding) this._commands[i + 1] + ")");
                        i += 2;
                        break;
                    default:
                        D.assert(false, "unknown cmd: " + cmd);
                        break;
                }
            }

            return sb.ToString();
        }

        void _reset() {
            this._commands.Clear();
            this._commandx = 0;
            this._commandy = 0;
            this._minX = float.MaxValue;
            this._minY = float.MaxValue;
            this._maxX = float.MinValue;
            this._maxY = float.MinValue;
            this._cache = null;
        }

        void _expandBounds(float x, float y) {
            this._minX = Mathf.Min(this._minX, x);
            this._minY = Mathf.Min(this._minY, y);
            this._maxX = Mathf.Max(this._maxX, x);
            this._maxY = Mathf.Max(this._maxY, y);
        }

        public Rect getBounds() {
            if (this._minX >= this._maxX || this._minY >= this._maxY) {
                return Rect.zero;
            }

            return Rect.fromLTRB(this._minX, this._minY, this._maxX, this._maxY);
        }

        void _appendCommands(float[] commands) {
            var i = 0;
            while (i < commands.Length) {
                var cmd = (PathCommand) commands[i];
                switch (cmd) {
                    case PathCommand.moveTo:
                        this._commandx = commands[i + 1];
                        this._commandy = commands[i + 2];
                        i += 3;
                        break;
                    case PathCommand.lineTo:
                        this._expandBounds(this._commandx, this._commandy);
                        this._expandBounds(commands[i + 1], commands[i + 2]);
                        this._commandx = commands[i + 1];
                        this._commandy = commands[i + 2];
                        i += 3;
                        break;
                    case PathCommand.bezierTo:
                        this._expandBounds(this._commandx, this._commandy);
                        this._expandBounds(commands[i + 1], commands[i + 2]);
                        this._expandBounds(commands[i + 3], commands[i + 4]);
                        this._expandBounds(commands[i + 5], commands[i + 6]);
                        this._commandx = commands[i + 5];
                        this._commandy = commands[i + 6];
                        i += 7;
                        break;
                    case PathCommand.close:
                        i++;
                        break;
                    case PathCommand.winding:
                        i += 2;
                        break;
                    default:
                        D.assert(false, "unknown cmd: " + cmd);
                        break;
                }
            }

            this._commands.AddRange(commands);
            this._cache = null;
        }

        public void relativeMoveTo(float x, float y) {
            var x0 = this._commandx;
            var y0 = this._commandy;
            
            this._appendCommands(new[] {
                (float) PathCommand.moveTo,
                x + x0, y + y0,
            });
        }

        public void moveTo(float x, float y) {
            this._appendCommands(new[] {
                (float) PathCommand.moveTo,
                x, y,
            });
        }


        public void relativeLineTo(float x, float y) {
            var x0 = this._commandx;
            var y0 = this._commandy;
            
            this._appendCommands(new[] {
                (float) PathCommand.lineTo,
                x + x0, y + y0,
            });
        }
        
        public void lineTo(float x, float y) {
            this._appendCommands(new[] {
                (float) PathCommand.lineTo,
                x, y,
            });
        }

        public void cubicTo(float c1x, float c1y, float c2x, float c2y, float x, float y) {
            this._appendCommands(new[] {
                (float) PathCommand.bezierTo,
                c1x, c1y, c2x, c2y, x, y,
            });
        }
        
        public void relativeCubicTo(float c1x, float c1y, float c2x, float c2y, float x, float y) {
            var x0 = this._commandx;
            var y0 = this._commandy;
            
            this.cubicTo(x0 + c1x, y0 + c1y, x0 + c2x, y0 + c2y, x0 + x, y0 + y);
        }

        public void quadraticBezierTo(float cx, float cy, float x, float y) {
            var x0 = this._commandx;
            var y0 = this._commandy;

            this._appendCommands(new[] {
                (float) PathCommand.bezierTo,
                (x0 + 2.0f / 3.0f * (cx - x0)), (y0 + 2.0f / 3.0f * (cy - y0)),
                (x + 2.0f / 3.0f * (cx - x)), (y + 2.0f / 3.0f * (cy - y)),
                x, y,
            });
        }
        
        public void relativeQuadraticBezierTo(float cx, float cy, float x, float y) {
            var x0 = this._commandx;
            var y0 = this._commandy;

            this.quadraticBezierTo(x0 + cx, y0 + cy, x0 + x, y0 + y);
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
                (float) PathCommand.moveTo, rect.left, rect.top,
                (float) PathCommand.lineTo, rect.left, rect.bottom,
                (float) PathCommand.lineTo, rect.right, rect.bottom,
                (float) PathCommand.lineTo, rect.right, rect.top,
                (float) PathCommand.close
            });
        }

        public void addRRect(RRect rrect) {
            float w = rrect.width;
            float h = rrect.height;
            float halfw = Mathf.Abs(w) * 0.5f;
            float halfh = Mathf.Abs(h) * 0.5f;
            float signW = Mathf.Sign(w);
            float signH = Mathf.Sign(h);

            float rxBL = Mathf.Min(rrect.blRadiusX, halfw) * signW;
            float ryBL = Mathf.Min(rrect.blRadiusY, halfh) * signH;
            float rxBR = Mathf.Min(rrect.brRadiusX, halfw) * signW;
            float ryBR = Mathf.Min(rrect.brRadiusY, halfh) * signH;
            float rxTR = Mathf.Min(rrect.trRadiusX, halfw) * signW;
            float ryTR = Mathf.Min(rrect.trRadiusY, halfh) * signH;
            float rxTL = Mathf.Min(rrect.tlRadiusX, halfw) * signW;
            float ryTL = Mathf.Min(rrect.tlRadiusY, halfh) * signH;
            float x = rrect.left;
            float y = rrect.top;

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

        public void addEllipse(float cx, float cy, float rx, float ry) {
            this._appendCommands(new[] {
                (float) PathCommand.moveTo, (cx - rx), cy,
                (float) PathCommand.bezierTo, (cx - rx), (cy + ry * _KAPPA90),
                (cx - rx * _KAPPA90), (cy + ry), cx, (cy + ry),
                (float) PathCommand.bezierTo, (cx + rx * _KAPPA90), (cy + ry),
                (cx + rx), (cy + ry * _KAPPA90), (cx + rx), cy,
                (float) PathCommand.bezierTo, (cx + rx), (cy - ry * _KAPPA90),
                (cx + rx * _KAPPA90), (cy - ry), cx, (cy - ry),
                (float) PathCommand.bezierTo, (cx - rx * _KAPPA90), (cy - ry),
                (cx - rx), (cy - ry * _KAPPA90), (cx - rx), cy,
                (float) PathCommand.close,
            });
        }

        public void addCircle(float cx, float cy, float r) {
            this.addEllipse(cx, cy, r, r);
        }

        public void addOval(Rect oval) {
            D.assert(oval != null);
            var center = oval.center;
            this.addEllipse(center.dx, center.dy, oval.width / 2, oval.height / 2);
        }

        public void arcTo(float x1, float y1, float x2, float y2, float radius) {
            var x0 = this._commandx;
            var y0 = this._commandy;

            // Calculate tangential circle to lines (x0,y0)-(x1,y1) and (x1,y1)-(x2,y2).
            float dx0 = x0 - x1;
            float dy0 = y0 - y1;
            float dx1 = x2 - x1;
            float dy1 = y2 - y1;
            PathUtils.normalize(ref dx0, ref dy0);
            PathUtils.normalize(ref dx1, ref dy1);
            float a = Mathf.Acos(dx0 * dx1 + dy0 * dy1);
            float d = radius / Mathf.Tan(a / 2.0f);

            if (d > 10000.0f) {
                this.lineTo(x1, y1);
                return;
            }

            float cx, cy, a0, a1;
            PathWinding dir;
            float cross = dx1 * dy0 - dx0 * dy1;
            if (cross > 0.0f) {
                cx = x1 + dx0 * d + dy0 * radius;
                cy = y1 + dy0 * d + -dx0 * radius;
                a0 = Mathf.Atan2(dx0, -dy0);
                a1 = Mathf.Atan2(-dx1, dy1);
                dir = PathWinding.clockwise;
            } else {
                cx = x1 + dx0 * d + -dy0 * radius;
                cy = y1 + dy0 * d + dx0 * radius;
                a0 = Mathf.Atan2(-dx0, dy0);
                a1 = Mathf.Atan2(dx1, -dy1);
                dir = PathWinding.counterClockwise;
            }

            this.addArc(cx, cy, radius, a0, a1, dir);
        }

        public void arcTo(Rect rect, float startAngle, float sweepAngle, bool forceMoveTo = true) {
            var mat = Matrix3.makeScale(rect.width / 2, rect.height / 2);
            var center = rect.center;
            mat.postTranslate(center.dx, center.dy);

            var vals = this._getArcCommands(0, 0, 1, startAngle, startAngle + sweepAngle,
                sweepAngle >= 0 ? PathWinding.clockwise : PathWinding.counterClockwise, forceMoveTo);

            this._transformCommands(vals, mat);
            this._appendCommands(vals.ToArray());
        }

        public void addArc(Rect rect, float startAngle, float sweepAngle) {
            this.arcTo(rect, startAngle, sweepAngle, true);
        }

        public Path transform(Matrix3 mat) {
            Path ret = new Path();
            
            var i = 0;
            while (i < this._commands.Count) {
                var cmd = (PathCommand) this._commands[i];
                switch (cmd) {
                    case PathCommand.moveTo:
                        var res_move = mat.mapXY(this._commands[i + 1], this._commands[i + 2]);
                        ret.moveTo(res_move.dx, res_move.dy);
                        i += 3;
                        break;
                    case PathCommand.lineTo:
                        var res_lineto = mat.mapXY(this._commands[i + 1], this._commands[i + 2]);
                        ret.lineTo(res_lineto.dx, res_lineto.dy);
                        i += 3;
                        break;
                    case PathCommand.bezierTo:
                        var res1 = mat.mapXY(this._commands[i + 1], this._commands[i + 2]);
                        var res2 = mat.mapXY(this._commands[i + 3], this._commands[i + 4]);
                        var res3 = mat.mapXY(this._commands[i + 5], this._commands[i + 6]);
                        ret.cubicTo(res1.dx, res1.dy, res2.dx, res2.dy, res3.dx, res3.dy);
                        i += 7;
                        break;
                    case PathCommand.close:
                        i++;
                        break;
                    case PathCommand.winding:
                        i += 2;
                        break;
                    default:
                        D.assert(false, "unknown cmd: " + cmd);
                        break;
                }
            }

            return ret;
        }

        void _transformCommands(List<float> commands, Matrix3 mat) {
            if (mat == null) {
                return;
            }
            
            var i = 0;
            while (i < commands.Count) {
                var cmd = (PathCommand) commands[i];
                switch (cmd) {
                    case PathCommand.moveTo:
                    case PathCommand.lineTo:
                        var res = mat.mapXY(commands[i + 1], commands[i + 2]);
                        commands[i + 1] = res.dx;
                        commands[i + 2] = res.dy;
                        i += 3;
                        break;
                    case PathCommand.bezierTo:
                        var res1 = mat.mapXY(commands[i + 1], commands[i + 2]);
                        commands[i + 1] = res1.dx;
                        commands[i + 2] = res1.dy;
                        var res2 = mat.mapXY(commands[i + 3], commands[i + 4]);
                        commands[i + 3] = res2.dx;
                        commands[i + 4] = res2.dy;
                        var res3 = mat.mapXY(commands[i + 5], commands[i + 6]);
                        commands[i + 5] = res3.dx;
                        commands[i + 6] = res3.dy;
                        i += 7;
                        break;
                    case PathCommand.close:
                        i++;
                        break;
                    case PathCommand.winding:
                        i += 2;
                        break;
                    default:
                        D.assert(false, "unknown cmd: " + cmd);
                        break;
                }
            }

        }
        
        List<float>  _getArcCommands(float cx, float cy, float r, float a0, float a1, PathWinding dir, bool forceMoveTo) {
            // Clamp angles
            float da = a1 - a0;
            if (dir == PathWinding.clockwise) {
                if (Mathf.Abs(da) >= Mathf.PI * 2) {
                    da = Mathf.PI * 2;
                } else {
                    while (da < 0.0f) {
                        da += Mathf.PI * 2;
                    }
                    if (da <= 1e-5) {
                        return new List<float>();
                    }
                }
            } else {
                if (Mathf.Abs(da) >= Mathf.PI * 2) {
                    da = -Mathf.PI * 2;
                } else {
                    while (da > 0.0f) {
                        da -= Mathf.PI * 2;
                    }
                    if (da >= -1e-5) {
                        return new List<float>();
                    }
                }
            }
            // Split arc into max 90 degree segments.
            int ndivs = Mathf.Max(1, Mathf.Min((int) (Mathf.Abs(da) / (Mathf.PI * 0.5f) + 0.5f), 5));
            float hda = (da / ndivs) / 2.0f;
            float kappa = Mathf.Abs(4.0f / 3.0f * (1.0f - Mathf.Cos(hda)) / Mathf.Sin(hda));

            if (dir == PathWinding.counterClockwise) {
                kappa = -kappa;
            }

            PathCommand move = (forceMoveTo || this._commands.Count == 0) ? PathCommand.moveTo : PathCommand.lineTo;
            float px = 0, py = 0, ptanx = 0, ptany = 0;

            List<float> vals = new List<float>();
            for (int i = 0; i <= ndivs; i++) {
                float a = a0 + da * (i / (float) ndivs);
                float dx = Mathf.Cos(a);
                float dy = Mathf.Sin(a);
                float x = cx + dx * r;
                float y = cy + dy * r;
                float tanx = -dy * r * kappa;
                float tany = dx * r * kappa;

                if (i == 0) {
                    vals.Add((float) move);
                    vals.Add(x);
                    vals.Add(y);
                } else {
                    vals.Add((float) PathCommand.bezierTo);
                    vals.Add(px + ptanx);
                    vals.Add(py + ptany);
                    vals.Add(x - tanx);
                    vals.Add(y - tany);
                    vals.Add(x);
                    vals.Add(y);
                }
                px = x;
                py = y;
                ptanx = tanx;
                ptany = tany;
            }

            return vals;
        }
        
        public void addArc(float cx, float cy, float r, float a0, float a1, PathWinding dir, bool forceMoveTo = true) {
            var vals = this._getArcCommands(cx, cy, r, a0, a1, dir, forceMoveTo);
            this._appendCommands(vals.ToArray());
        }

        public void addPolygon(IList<Offset> points, bool close) {
            D.assert(points != null);
            if (points.Count == 0) {
                return;
            }

            var commands = new List<float>();
            commands.Add((float) PathCommand.moveTo);
            commands.Add(points[0].dx);
            commands.Add(points[0].dy);

            for (int i = 1; i < points.Count; i++) {
                var point = points[i];
                commands.Add((float) PathCommand.lineTo);
                commands.Add(point.dx);
                commands.Add(point.dy);
            }

            if (close) {
                commands.Add((float) PathCommand.close);
            }

            this._appendCommands(commands.ToArray());
        }

        public Path shift(Offset offset) {
            offset = offset ?? Offset.zero;
            var path = new Path();
            path.addPath(this, offset);
            return path;
        }
        
        public void addPath(Path path, Offset offset) {
            D.assert(path != null);
            D.assert(offset != null);

            var commands = new List<float>();

            var i = 0;
            while (i < path._commands.Count) {
                var cmd = (PathCommand) path._commands[i];
                switch (cmd) {
                    case PathCommand.moveTo:
                    case PathCommand.lineTo:
                        commands.Add(path._commands[i]);
                        commands.Add(path._commands[i + 1] + offset.dx);
                        commands.Add(path._commands[i + 2] + offset.dy);
                        i += 3;
                        break;
                    case PathCommand.bezierTo:
                        commands.Add(path._commands[i]);
                        commands.Add(path._commands[i + 1] + offset.dx);
                        commands.Add(path._commands[i + 2] + offset.dy);
                        commands.Add(path._commands[i + 3] + offset.dx);
                        commands.Add(path._commands[i + 4] + offset.dy);
                        commands.Add(path._commands[i + 5] + offset.dx);
                        commands.Add(path._commands[i + 6] + offset.dy);
                        i += 7;
                        break;
                    case PathCommand.close:
                        commands.Add(path._commands[i]);
                        i++;
                        break;
                    case PathCommand.winding:
                        commands.Add(path._commands[i]);
                        commands.Add(path._commands[i + 1]);
                        i += 2;
                        break;
                    default:
                        D.assert(false, "unknown cmd: " + cmd);
                        break;
                }
            }

            this._appendCommands(commands.ToArray());
        }

        public bool contains(Offset point) {
            var bounds = this.getBounds();
            if (bounds == null) {
                return false;
            }

            if (!bounds.containsInclusive(point)) {
                return false;
            }

            float x = point.dx;
            float y = point.dy;

            float lastMoveToX = 0;
            float lastMoveToY = 0;
            float commandx = 0;
            float commandy = 0;
            PathWinding winding = PathWinding.counterClockwise;

            var totalW = 0;
            var w = 0;
            var i = 0;
            while (i < this._commands.Count) {
                var cmd = (PathCommand) this._commands[i];
                switch (cmd) {
                    case PathCommand.moveTo:
                        if (lastMoveToX != commandx || lastMoveToY != commandy) {
                            w += windingLine(
                                commandx, commandy,
                                lastMoveToX, lastMoveToY,
                                x, y);
                        }

                        if (w != 0) {
                            totalW += winding == PathWinding.counterClockwise ? w : -w;
                            w = 0;
                        }

                        lastMoveToX = commandx = this._commands[i + 1];
                        lastMoveToY = commandy = this._commands[i + 2];
                        winding = PathWinding.counterClockwise;
                        i += 3;
                        break;
                    case PathCommand.lineTo:
                        w += windingLine(
                            commandx, commandy,
                            this._commands[i + 1], this._commands[i + 2],
                            x, y);
                        commandx = this._commands[i + 1];
                        commandy = this._commands[i + 2];
                        i += 3;
                        break;
                    case PathCommand.bezierTo:
                        w += windingCubic(
                            commandx, commandy,
                            this._commands[i + 1], this._commands[i + 2],
                            this._commands[i + 3], this._commands[i + 4],
                            this._commands[i + 5], this._commands[i + 6],
                            x, y);
                        commandx = this._commands[i + 5];
                        commandy = this._commands[i + 6];
                        i += 7;
                        break;
                    case PathCommand.close:
                        i++;
                        break;
                    case PathCommand.winding:
                        winding = (PathWinding) this._commands[i + 1];
                        i += 2;
                        break;
                    default:
                        D.assert(false, "unknown cmd: " + cmd);
                        break;
                }
            }

            if (lastMoveToX != commandx || lastMoveToY != commandy) {
                w += windingLine(
                    commandx, commandy,
                    lastMoveToX, lastMoveToY,
                    x, y);
            }

            if (w != 0) {
                totalW += winding == PathWinding.counterClockwise ? w : -w;
                w = 0;
            }

            return totalW != 0;
        }

        static int windingLine(float x0, float y0, float x1, float y1, float x, float y) {
            if (y0 == y1) {
                return 0;
            }

            int dir = 1; // down. y0 < y1
            float minY = y0;
            float maxY = y1;

            if (y0 > y1) {
                dir = -1;
                minY = y1;
                maxY = y0;
            }

            if (y < minY || y >= maxY) {
                return 0;
            }

            float cross = (x1 - x0) * (y - y0) - (x - x0) * (y1 - y0);
            if (cross == 0) {
                return 0;
            }

            if (cross.sign() == dir) {
                return 0;
            }

            return dir;
        }

        static int windingCubic(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4,
            float x, float y) {
            Offset[] src = {
                new Offset(x1, y1),
                new Offset(x2, y2),
                new Offset(x3, y3),
                new Offset(x4, y4),
            };

            Offset[] dst = new Offset[10];
            int n = _chopCubicAtYExtrema(src, dst);

            int w = 0;
            for (int i = 0; i <= n; ++i) {
                w += _winding_mono_cubic(dst, i * 3, x, y);
            }

            return w;
        }

        static int _winding_mono_cubic(Offset[] pts, int ptsBase, float x, float y) {
            float y0 = pts[ptsBase + 0].dy;
            float y3 = pts[ptsBase + 3].dy;

            if (y0 == y3) {
                return 0;
            }

            int dir = 1; // down. y0 < y3
            float minY = y0;
            float maxY = y3;

            if (y0 > y3) {
                dir = -1;
                minY = y3;
                maxY = y0;
            }

            if (y < minY || y >= maxY) {
                return 0;
            }

            // quickreject or quickaccept
            float minX = float.MaxValue, maxX = float.MinValue;
            for (int i = 0; i < 4; i++) {
                var dx = pts[ptsBase + i].dx;
                if (dx < minX) {
                    minX = dx;
                }

                if (dx > maxX) {
                    maxX = dx;
                }
            }

            if (x < minX) {
                return 0;
            }

            if (x > maxX) {
                return dir;
            }

            // compute the actual x(t) value
            float t;
            if (!_chopMonoAtY(pts, ptsBase, y, out t)) {
                return 0;
            }

            float xt = _eval_cubic_pts(
                pts[ptsBase + 0].dx,
                pts[ptsBase + 1].dx,
                pts[ptsBase + 2].dx,
                pts[ptsBase + 3].dx, t);

            return xt < x ? dir : 0;
        }

        static float _eval_cubic_pts(float c0, float c1, float c2, float c3,
            float t) {
            float A = c3 + 3 * (c1 - c2) - c0;
            float B = 3 * (c2 - c1 - c1 + c0);
            float C = 3 * (c1 - c0);
            float D = c0;
            return _poly_eval(A, B, C, D, t);
        }

        static float _poly_eval(float A, float B, float C, float D, float t) {
            return ((A * t + B) * t + C) * t + D;
        }

        static bool _chopMonoAtY(Offset[] pts, int ptsBase, float y, out float t) {
            float[] ycrv = {
                pts[ptsBase + 0].dy - y,
                pts[ptsBase + 1].dy - y,
                pts[ptsBase + 2].dy - y,
                pts[ptsBase + 3].dy - y
            };

            // NEWTON_RAPHSON Quadratic convergence, typically <= 3 iterations.
            // Initial guess.
            // is not only monotonic but degenerate.
            float t1 = ycrv[0] / (ycrv[0] - ycrv[3]);

            // Newton's iterations.
            const float tol = 1f / 16384; // This leaves 2 fixed noise bits.
            float t0;
            const int maxiters = 5;
            int iters = 0;
            bool converged;
            do {
                t0 = t1;
                float y01 = MathUtils.lerpFloat(ycrv[0], ycrv[1], t0);
                float y12 = MathUtils.lerpFloat(ycrv[1], ycrv[2], t0);
                float y23 = MathUtils.lerpFloat(ycrv[2], ycrv[3], t0);
                float y012 = MathUtils.lerpFloat(y01, y12, t0);
                float y123 = MathUtils.lerpFloat(y12, y23, t0);
                float y0123 = MathUtils.lerpFloat(y012, y123, t0);
                float yder = (y123 - y012) * 3;
                t1 -= y0123 / yder;
                converged = (t1 - t0).abs() <= tol; // NaN-safe
                ++iters;
            } while (!converged && (iters < maxiters));

            t = t1;

            // The result might be valid, even if outside of the range [0, 1], but
            // we never evaluate a Bezier outside this interval, so we return false.
            if (t1 < 0 || t1 > 1) {
                return false;
            }

            return converged;
        }

        static void _flatten_double_cubic_extrema(Offset[] dst, int dstBase) {
            var dy = dst[dstBase + 3].dy;
            dst[dstBase + 2] = new Offset(dst[dstBase + 2].dx, dy);
            dst[dstBase + 4] = new Offset(dst[dstBase + 4].dx, dy);
        }

        static int _chopCubicAtYExtrema(Offset[] src, Offset[] dst) {
            D.assert(src != null && src.Length == 4);
            D.assert(dst != null && dst.Length == 10);

            float[] tValues = new float[2];
            int roots = _findCubicExtrema(
                src[0].dy, src[1].dy, src[2].dy, src[3].dy,
                tValues);

            _chopCubicAt(src, dst, tValues, roots);
            if (dst != null && roots > 0) {
                // we do some cleanup to ensure our Y extrema are flat
                _flatten_double_cubic_extrema(dst, 0);
                if (roots == 2) {
                    _flatten_double_cubic_extrema(dst, 3);
                }
            }

            return roots;
        }

        static void _chopCubicAt(Offset[] src, int srcBase, Offset[] dst, int dstBase, float t) {
            D.assert(src != null && (src.Length == 4 || src.Length == 10));
            D.assert(dst != null && dst.Length == 10);

            D.assert(t > 0 && t < 1);

            var p0 = src[srcBase + 0];
            var p1 = src[srcBase + 1];
            var p2 = src[srcBase + 2];
            var p3 = src[srcBase + 3];

            var ab = Offset.lerp(p0, p1, t);
            var bc = Offset.lerp(p1, p2, t);
            var cd = Offset.lerp(p2, p3, t);
            var abc = Offset.lerp(ab, bc, t);
            var bcd = Offset.lerp(bc, cd, t);
            var abcd = Offset.lerp(abc, bcd, t);

            dst[dstBase + 0] = p0;
            dst[dstBase + 1] = ab;
            dst[dstBase + 2] = abc;
            dst[dstBase + 3] = abcd;
            dst[dstBase + 4] = bcd;
            dst[dstBase + 5] = cd;
            dst[dstBase + 6] = p3;
        }

        static void _chopCubicAt(Offset[] src, Offset[] dst, float[] tValues, int roots) {
            D.assert(src != null && src.Length == 4);
            D.assert(dst != null && dst.Length == 10);

            D.assert(() => {
                for (int i = 0; i < roots - 1; i++) {
                    D.assert(0 < tValues[i] && tValues[i] < 1);
                    D.assert(0 < tValues[i + 1] && tValues[i + 1] < 1);
                    D.assert(tValues[i] < tValues[i + 1]);
                }

                return true;
            });

            if (dst != null) {
                if (roots == 0) {
                    dst[0] = src[0];
                    dst[1] = src[1];
                    dst[2] = src[2];
                    dst[3] = src[3];
                }
                else {
                    float t = tValues[0];

                    int srcBase = 0;
                    int dstBase = 0;
                    for (int i = 0; i < roots; i++) {
                        _chopCubicAt(src, srcBase, dst, dstBase, t);
                        if (i == roots - 1) {
                            break;
                        }

                        dstBase += 3;
                        src = dst;
                        srcBase = dstBase;

                        // watch out in case the renormalized t isn't in range
                        if (_valid_unit_divide(tValues[i + 1] - tValues[i], 1 - tValues[i], out t) == 0) {
                            // if we can't, just create a degenerate cubic
                            dst[dstBase + 4] = dst[dstBase + 5] = dst[dstBase + 6] = src[srcBase + 3];
                            break;
                        }
                    }
                }
            }
        }

        /** Cubic'(t) = At^2 + Bt + C, where
            A = 3(-a + 3(b - c) + d)
            B = 6(a - 2b + c)
            C = 3(b - a)
            Solve for t, keeping only those that fit between 0 < t < 1
        */
        static int _findCubicExtrema(float a, float b, float c, float d, float[] tValues) {
            // we divide A,B,C by 3 to simplify
            float A = d - a + 3 * (b - c);
            float B = 2 * (a - b - b + c);
            float C = b - a;

            return _findUnitQuadRoots(A, B, C, tValues);
        }

        static int _valid_unit_divide(float numer, float denom, out float ratio) {
            ratio = 0;

            if (numer < 0) {
                numer = -numer;
                denom = -denom;
            }

            if (denom == 0 || numer == 0 || numer >= denom) {
                return 0;
            }

            float r = numer / denom;
            if (float.IsNaN(r)) {
                return 0;
            }

            D.assert(r >= 0 && r < 1, $"numer {numer}, denom {denom}, r {r}");
            if (r == 0) {
                // catch underflow if numer <<<< denom
                return 0;
            }

            ratio = r;
            return 1;
        }

        // Just returns its argument, but makes it easy to set a break-point to know when
        // _findUnitQuadRoots is going to return 0 (an error).
        static int _return_check_zero(int value) {
            if (value == 0) {
                return 0;
            }

            return value;
        }

        static int _findUnitQuadRoots(float A, float B, float C, float[] roots) {
            if (A == 0) {
                return _return_check_zero(_valid_unit_divide(-C, B, out roots[0]));
            }

            int r = 0;

            // use doubles so we don't overflow temporarily trying to compute R
            double dr = (double) B * B - 4 * (double) A * C;
            if (dr < 0) {
                return _return_check_zero(0);
            }

            dr = Math.Sqrt(dr);
            float R = (float) dr;

            if (float.IsInfinity(R)) {
                return _return_check_zero(0);
            }

            float Q = (B < 0) ? -(B - R) / 2 : -(B + R) / 2;
            r += _valid_unit_divide(Q, A, out roots[r]);
            r += _valid_unit_divide(C, Q, out roots[r]);
            if (r == 2) {
                if (roots[0] > roots[1]) {
                    float tmp = roots[0];
                    roots[0] = roots[1];
                    roots[1] = tmp;
                }
                else if (roots[0] == roots[1]) {
                    // nearly-equal?
                    r -= 1; // skip the double root
                }
            }

            return _return_check_zero(r);
        }
    }

    public enum PathWinding {
        counterClockwise = 1, // which just means the order as the input is.
        clockwise = 2, // which just means the reversed order.
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
        readonly float _scale;
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

        public PathCache(float scale) {
            this._scale = scale;
            this._distTol = 0.01f / scale;
            this._tessTol = 0.25f / scale;
        }

        public bool canReuse(float scale) {
            if (this._scale != scale) {
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
            this._addPoint(new PathPoint {x = x, y = y, flags = flags});
        }

        void _addPoint(PathPoint point) {
            if (this._paths.Count == 0) {
                this.addPath();
                this.addPoint(0, 0, PointFlags.corner);
            }

            var path = this._paths[this._paths.Count - 1];
            if (path.count > 0) {
                var pt = this._points[this._points.Count - 1];
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
            }
            else {
                var pt = this._points[this._points.Count - 1];
                x1 = pt.x;
                y1 = pt.y;
            }

            var points = TessellationGenerator.tessellateBezier(x1, y1, x2, y2, x3, y3, x4, y4, this._tessTol);
            D.assert(points.Count > 0);            
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

            var path = this._paths[this._paths.Count - 1];
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
                    if (path.winding == PathWinding.clockwise) {
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

            var mesh = new MeshMesh(null, this._vertices, indices);
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
                }
                else {
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
                    }
                    else if (lineCap == StrokeCap.square) {
                        this._vertices.buttCapStart(p0, p0.dx, p0.dy, w, w);
                    }
                    else {
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
                        }
                        else {
                            this._vertices.bevelJoin(p0, p1, w, w);
                        }
                    }
                    else {
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
                    }
                    else if (lineCap == StrokeCap.square) {
                        this._vertices.buttCapEnd(p1, p0.dx, p0.dy, w, w);
                    }
                    else {
                        // round
                        this._vertices.roundCapEnd(p1, p0.dx, p0.dy, w, ncap);
                    }
                }
                else {
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
                        }
                        else {
                            indices.Add(path.istroke + j - 2);
                            indices.Add(path.istroke + j - 1);
                            indices.Add(path.istroke + j);
                        }
                    }
                }
            }

            D.assert(indices.Count == cindices);

            this._strokeMesh = new MeshMesh(null, this._vertices, indices);
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

            for (var i = 0; i < ncap; i++) {
                float a = (float) i / (ncap - 1) * Mathf.PI;
                float ax = Mathf.Cos(a) * w, ay = Mathf.Sin(a) * w;
                dst.Add(new Vector2(px - dlx * ax - dx * ay, py - dly * ax - dy * ay));
                dst.Add(new Vector2(px, py));
            }

            dst.Add(new Vector2(px + dlx * w, py + dly * w));
            dst.Add(new Vector2(px - dlx * w, py - dly * w));
        }

        public static void roundCapEnd(this List<Vector3> dst, PathPoint p,
            float dx, float dy, float w, int ncap) {
            float px = p.x;
            float py = p.y;
            float dlx = dy;
            float dly = -dx;

            dst.Add(new Vector2(px + dlx * w, py + dly * w));
            dst.Add(new Vector2(px - dlx * w, py - dly * w));

            for (var i = 0; i < ncap; i++) {
                float a = (float) i / (ncap - 1) * Mathf.PI;
                float ax = Mathf.Cos(a) * w, ay = Mathf.Sin(a) * w;
                dst.Add(new Vector2(px, py));
                dst.Add(new Vector2(px - dlx * ax + dx * ay, py - dly * ax + dy * ay));
            }
        }

        public static void chooseBevel(bool bevel, PathPoint p0, PathPoint p1, float w,
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
                    float u = (float) i / (n - 1);
                    float a = a0 + u * (a1 - a0);
                    float rx = p1.x + Mathf.Cos(a) * rw;
                    float ry = p1.y + Mathf.Sin(a) * rw;

                    dst.Add(new Vector2(p1.x, p1.y));
                    dst.Add(new Vector2(rx, ry));
                }

                dst.Add(new Vector2(lx1, ly1));
                dst.Add(new Vector2(p1.x - dlx1 * rw, p1.y - dly1 * rw));
            }
            else {
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
                    float u = (float) i / (n - 1);
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
                }

                dst.Add(new Vector2(lx1, ly1));
                dst.Add(new Vector2(p1.x - dlx1 * rw, p1.y - dly1 * rw));
            }
            else {
                chooseBevel((p1.flags & PointFlags.innerBevel) != 0, p0, p1, -rw,
                    out rx0, out ry0, out rx1, out ry1);

                dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                dst.Add(new Vector2(rx0, ry0));

                if ((p1.flags & PointFlags.bevel) != 0) {
                    dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                    dst.Add(new Vector2(rx0, ry0));
                    dst.Add(new Vector2(p1.x + dlx1 * lw, p1.y + dly1 * lw));
                    dst.Add(new Vector2(rx1, ry1));
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
        public readonly Matrix3 matrix;
        public readonly Rect rawBounds;

        Rect _bounds;

        public Rect bounds {
            get {
                if (this._bounds == null) {
                    this._bounds = this.matrix != null ? this.matrix.mapRect(this.rawBounds) : this.rawBounds;
                }

                return this._bounds;
            }
        }


        MeshMesh _boundsMesh;

        static readonly List<int> _boundsTriangles = new List<int>(6) {
            0, 2, 1, 1, 2, 3
        };

        public MeshMesh boundsMesh {
            get {
                if (this._boundsMesh == null) {
                    this._boundsMesh = new MeshMesh(this.bounds);
                }

                return this._boundsMesh;
            }
        }

        public MeshMesh(Rect rect) {
            this.vertices = new List<Vector3>(4) {
                new Vector3(rect.right, rect.bottom),
                new Vector3(rect.right, rect.top),
                new Vector3(rect.left, rect.bottom),
                new Vector3(rect.left, rect.top)
            };

            this.triangles = _boundsTriangles;
            this.rawBounds = rect;

            this._bounds = this.rawBounds;
            this._boundsMesh = this;
        }

        public MeshMesh(Matrix3 matrix, List<Vector3> vertices, List<int> triangles, List<Vector2> uv = null,
            Rect rawBounds = null) {
            D.assert(vertices != null);
            D.assert(vertices.Count >= 0);
            D.assert(triangles != null);
            D.assert(triangles.Count >= 0);
            D.assert(uv == null || uv.Count == vertices.Count);

            this.matrix = matrix;
            this.vertices = vertices;
            this.triangles = triangles;
            this.uv = uv;

            if (rawBounds == null) {
                if (vertices.Count > 0) {
                    float minX = vertices[0].x;
                    float maxX = vertices[0].x;
                    float minY = vertices[0].y;
                    float maxY = vertices[0].y;

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

                    rawBounds = Rect.fromLTRB(minX, minY, maxX, maxY);
                }
                else {
                    rawBounds = Rect.zero;
                }
            }

            this.rawBounds = rawBounds;
        }

        public MeshMesh transform(Matrix3 matrix) {
            return new MeshMesh(matrix, this.vertices, this.triangles, this.uv, this.rawBounds);
        }
    }

    public class MeshPool : IDisposable {
        readonly Queue<Mesh> _pool = new Queue<Mesh>();

        public Mesh getMesh() {
            if (this._pool.Count > 0) {
                var mesh = this._pool.Dequeue();
                return mesh;
            }
            else {
                var mesh = new Mesh();
                mesh.hideFlags = HideFlags.HideAndDontSave;
                return mesh;
            }
        }

        public void returnMesh(Mesh mesh) {
            D.assert(mesh != null);
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