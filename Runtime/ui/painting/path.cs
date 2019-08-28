using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {

    public enum PathOperation {
        difference,
        intersect,
        union,
        xor,
        reverseDifference,
    }

    struct VertexUV {
        public List<Vector3> fillVertices;
        public List<Vector2> fillUV;
        public List<Vector3> strokeVertices;
        public List<Vector2> strokeUV;
    }

    public class Path {
        const float _KAPPA90 = 0.5522847493f;

        readonly List<float> _commands;
        float _commandx;
        float _commandy;
        float _minX, _minY;
        float _maxX, _maxY;

        PathCache _cache;

        static uint pathGlobalKey = 0;

        uint _pathKey = 0;

        //shadow speeder relevant
        bool _isNaiveRRect = false;
        public bool isNaiveRRect => this._isNaiveRRect;
        
        PathShapeHint _shapeHint = PathShapeHint.Other;
        public PathShapeHint shapeHint => this._shapeHint;

        float _rRectCorner;
        public float rRectCorner => this._rRectCorner;

        public uint pathKey {
            get { return this._pathKey; }
        }

        public Path(int capacity = 128) {
            this._commands = new List<float>(capacity);
            this._reset();
        }

        public List<float> commands {
            get { return this._commands; }
        }

        void _updateRRectFlag(bool isNaiveRRect, PathShapeHint shapeHint = PathShapeHint.Other, float corner = 0) {
            if (this._commands.Count > 0 && !this._isNaiveRRect) {
                return;
            }
            this._isNaiveRRect = isNaiveRRect && this._hasOnlyMoveTos();
            if (this._isNaiveRRect) {
                this._shapeHint = shapeHint;
                this._rRectCorner = corner;
            }
        }
        
        bool _hasOnlyMoveTos() {
            var i = 0;
            while (i < this._commands.Count) {
                var cmd = (PathCommand) this._commands[i];
                switch (cmd) {
                    case PathCommand.moveTo: 
                        i += 3;
                        break;
                    case PathCommand.lineTo:
                        return false;
                    case PathCommand.bezierTo:
                        return false;
                    case PathCommand.close:
                        i++;
                        break;
                    case PathCommand.winding:
                        i += 2;
                        break;
                    default:
                        return false;
                }
            }

            return true;
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
                        D.assert(false, () => "unknown cmd: " + cmd);
                        break;
                }
            }

            return sb.ToString();
        }

        public void resetAll() {
            this._reset();
        }

        void _reset() {
            this._commands.Clear();
            this._commandx = 0;
            this._commandy = 0;
            this._minX = float.MaxValue;
            this._minY = float.MaxValue;
            this._maxX = float.MinValue;
            this._maxY = float.MinValue;

            this._pathKey = pathGlobalKey++;
            this._cache = null;
            this._isNaiveRRect = false;
        }

        internal PathCache flatten(float scale) {
            scale = Mathf.Round(scale * 2.0f) / 2.0f; // round to 0.5f

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
                        D.assert(false, () => "unknown cmd: " + cmd);
                        break;
                }
            }

            this._cache.normalize();
            return this._cache;
        }

        void _expandBounds(float x, float y) {
            if (x < this._minX) {
                this._minX = x;
            }

            if (y < this._minY) {
                this._minY = y;
            }

            if (x > this._maxX) {
                this._maxX = x;
            }

            if (y > this._maxY) {
                this._maxY = y;
            }
        }

        public Rect getBounds() {
            if (this._minX >= this._maxX || this._minY >= this._maxY) {
                return Rect.zero;
            }

            return Rect.fromLTRB(this._minX, this._minY, this._maxX, this._maxY);
        }

        public static Path combine(PathOperation operation, Path path1, Path path2) {
            D.assert(path1 != null);
            D.assert(path2 != null);
            Path path = null;
            D.assert(() => {
                Debug.LogWarning("Path._op() not implemented yet!");
                return true;
            });
            return path;
//            if (path._op(path1, path2, (int) operation)) {
//                return path;
//            }
//            throw new UIWidgetsError("Path.combine() failed.  This may be due an invalid path; " +
//                                     "in particular, check for NaN values.");
        }

        public PathMetrics computeMetrics(bool forceClosed = false) {
            return PathMetrics._(this, forceClosed);
        }

        void _appendMoveTo(float x, float y) {
            this._commands.Add((float) PathCommand.moveTo);
            this._commands.Add(x);
            this._commands.Add(y);

            this._commandx = x;
            this._commandy = y;

            this._pathKey = pathGlobalKey++;
            this._cache = null;
        }

        void _appendLineTo(float x, float y) {
            this._expandBounds(this._commandx, this._commandy);
            this._expandBounds(x, y);

            this._commands.Add((float) PathCommand.lineTo);
            this._commands.Add(x);
            this._commands.Add(y);

            this._commandx = x;
            this._commandy = y;

            this._pathKey = pathGlobalKey++;
            this._cache = null;
        }

        void _appendBezierTo(float x1, float y1, float x2, float y2, float x3, float y3) {
            this._expandBounds(this._commandx, this._commandy);
            this._expandBounds(x1, y1);
            this._expandBounds(x2, y2);
            this._expandBounds(x3, y3);

            this._commands.Add((float) PathCommand.bezierTo);
            this._commands.Add(x1);
            this._commands.Add(y1);
            this._commands.Add(x2);
            this._commands.Add(y2);
            this._commands.Add(x3);
            this._commands.Add(y3);

            this._commandx = x3;
            this._commandy = y3;

            this._pathKey = pathGlobalKey++;
            this._cache = null;
        }

        void _appendClose() {
            this._commands.Add((float) PathCommand.close);

            this._pathKey = pathGlobalKey++;
            this._cache = null;
        }

        void _appendWinding(float winding) {
            this._commands.Add((float) PathCommand.winding);
            this._commands.Add(winding);

            this._pathKey = pathGlobalKey++;
            this._cache = null;
        }

        public void relativeMoveTo(float x, float y) {
            var x0 = this._commandx;
            var y0 = this._commandy;

            this._appendMoveTo(x + x0, y + y0);
        }

        public void moveTo(float x, float y) {
            this._appendMoveTo(x, y);
        }

        public void relativeLineTo(float x, float y) {
            var x0 = this._commandx;
            var y0 = this._commandy;

            this._updateRRectFlag(false);
            this._appendLineTo(x + x0, y + y0);
        }

        public void lineTo(float x, float y) {
            this._updateRRectFlag(false);
            this._appendLineTo(x, y);
        }

        public void cubicTo(float c1x, float c1y, float c2x, float c2y, float x, float y) {
            this._updateRRectFlag(false);
            this._appendBezierTo(c1x, c1y, c2x, c2y, x, y);
        }

        public void relativeCubicTo(float c1x, float c1y, float c2x, float c2y, float x, float y) {
            var x0 = this._commandx;
            var y0 = this._commandy;
            this._updateRRectFlag(false);
            this.cubicTo(x0 + c1x, y0 + c1y, x0 + c2x, y0 + c2y, x0 + x, y0 + y);
        }

        public void quadraticBezierTo(float cx, float cy, float x, float y) {
            var x0 = this._commandx;
            var y0 = this._commandy;

            const float twoThird = 2.0f / 3.0f;
            this._updateRRectFlag(false);
            this._appendBezierTo(
                x0 + twoThird * (cx - x0), y0 + twoThird * (cy - y0),
                x + twoThird * (cx - x), y + twoThird * (cy - y),
                x, y);
        }

        public void relativeQuadraticBezierTo(float cx, float cy, float x, float y) {
            var x0 = this._commandx;
            var y0 = this._commandy;

            this._updateRRectFlag(false);
            this.quadraticBezierTo(x0 + cx, y0 + cy, x0 + x, y0 + y);
        }

        public void conicTo(float x1, float y1, float x2, float y2, float w) {
            this._updateRRectFlag(false);
            if (!(w > 0)) {
                this.lineTo(x2, y2);
                return;
            }

            if (w.isInfinite()) {
                this.lineTo(x1, y1);
                this.lineTo(x2, y2);
                return;
            }

            if (w == 1) {
                this.quadraticBezierTo(x1, y1, x2, y2);
                return;
            }

            var x0 = this._commandx;
            var y0 = this._commandy;

            var conic = new _Conic {
                x0 = x0, y0 = y0,
                x1 = x1, y1 = y1,
                x2 = x2, y2 = y2,
                w = w,
            };

            var quadX = new float[5];
            var quadY = new float[5];
            conic.chopIntoQuadsPOW2(quadX, quadY, 1);

            this.quadraticBezierTo(quadX[1], quadY[1], quadX[2], quadY[2]);
            this.quadraticBezierTo(quadX[3], quadY[3], quadX[4], quadY[4]);
        }

        public void relativeConicTo(float x1, float y1, float x2, float y2, float w) {
            var x0 = this._commandx;
            var y0 = this._commandy;
            this._updateRRectFlag(false);
            this.conicTo(x0 + x1, y0 + y1, x0 + x2, y0 + y2, w);
        }

        // http://www.w3.org/TR/SVG/implnote.html#ArcConversionEndpointToCenter
        public void arcToPoint(Offset arcEnd,
            Radius radius = null,
            float rotation = 0.0f,
            bool largeArc = false,
            bool clockwise = false) {
            radius = radius ?? Radius.zero;
            this._updateRRectFlag(false);
            D.assert(PaintingUtils._offsetIsValid(arcEnd));
            D.assert(PaintingUtils._radiusIsValid(radius));

            var x0 = this._commandx;
            var y0 = this._commandy;
            var x1 = arcEnd.dx;
            var y1 = arcEnd.dy;

            var rx = Mathf.Abs(radius.x);
            var ry = Mathf.Abs(radius.y);

            if (rx == 0 || ry == 0) {
                this.lineTo(x1, y1);
                return;
            }

            if (x0 == x1 && y0 == y1) {
                this.lineTo(x1, y1);
                return;
            }

            var midPointDistanceX = (x0 - x1) * 0.5f;
            var midPointDistanceY = (y0 - y1) * 0.5f;

            var pointTransform = Matrix3.makeRotate(rotation);
            var transformedMidPoint = pointTransform.mapXY(midPointDistanceX, midPointDistanceY);

            var squareRx = rx * rx;
            var squareRy = ry * ry;
            var squareX = transformedMidPoint.dx * transformedMidPoint.dx;
            var squareY = transformedMidPoint.dy * transformedMidPoint.dy;

            // Check if the radii are big enough to draw the arc, scale radii if not.
            // http://www.w3.org/TR/SVG/implnote.html#ArcCorrectionOutOfRangeRadii
            var radiiScale = squareX / squareRx + squareY / squareRy;
            if (radiiScale > 1) {
                radiiScale = Mathf.Sqrt(radiiScale);
                rx *= radiiScale;
                ry *= radiiScale;
            }

            pointTransform.setScale(1 / rx, 1 / ry);
            pointTransform.preRotate(-rotation);

            var unitPts = new[] {
                pointTransform.mapXY(x0, y0),
                pointTransform.mapXY(x1, y1),
            };

            var delta = unitPts[1] - unitPts[0];

            var d = delta.dx * delta.dx + delta.dy * delta.dy;
            var scaleFactorSquared = Mathf.Max(1 / d - 0.25f, 0.0f);

            var scaleFactor = Mathf.Sqrt(scaleFactorSquared);
            if (!clockwise != largeArc) {
                // flipped from the original implementation
                scaleFactor = -scaleFactor;
            }

            delta = delta.scale(scaleFactor);

            var centerPoint = unitPts[0] + unitPts[1];
            centerPoint *= 0.5f;
            centerPoint = centerPoint.translate(-delta.dy, delta.dx);
            unitPts[0] -= centerPoint;
            unitPts[1] -= centerPoint;

            var theta1 = Mathf.Atan2(unitPts[0].dy, unitPts[0].dx);
            var theta2 = Mathf.Atan2(unitPts[1].dy, unitPts[1].dx);
            var thetaArc = theta2 - theta1;
            if (thetaArc < 0 && clockwise) {
                // arcSweep flipped from the original implementation
                thetaArc += Mathf.PI * 2;
            }
            else if (thetaArc > 0 && !clockwise) {
                // arcSweep flipped from the original implementation
                thetaArc -= Mathf.PI * 2;
            }

            pointTransform.setRotate(rotation);
            pointTransform.preScale(rx, ry);

            // the arc may be slightly bigger than 1/4 circle, so allow up to 1/3rd
            int segments = Mathf.CeilToInt(Mathf.Abs(thetaArc / (2 * Mathf.PI / 3)));
            var thetaWidth = thetaArc / segments;
            var t = Mathf.Tan(0.5f * thetaWidth);
            if (!t.isFinite()) {
                return;
            }

            var startTheta = theta1;
            var w = Mathf.Sqrt(0.5f + Mathf.Cos(thetaWidth) * 0.5f);

            bool expectIntegers = ScalarUtils.ScalarNearlyZero(Mathf.PI / 2 - Mathf.Abs(thetaWidth)) &&
                                  ScalarUtils.ScalarIsInteger(rx) && ScalarUtils.ScalarIsInteger(ry) &&
                                  ScalarUtils.ScalarIsInteger(x1) && ScalarUtils.ScalarIsInteger(y1);

            for (int i = 0; i < segments; ++i) {
                var endTheta = startTheta + thetaWidth;
                var sinEndTheta = ScalarUtils.ScalarSinCos(endTheta, out var cosEndTheta);

                unitPts[1] = new Offset(cosEndTheta, sinEndTheta);
                unitPts[1] += centerPoint;
                unitPts[0] = unitPts[1];
                unitPts[0] = unitPts[0].translate(t * sinEndTheta, -t * cosEndTheta);
                var mapped = new[] {
                    pointTransform.mapPoint(unitPts[0]),
                    pointTransform.mapPoint(unitPts[1]),
                };

                /*
                Computing the arc width introduces rounding errors that cause arcs to start
                outside their marks. A round rect may lose convexity as a result. If the input
                values are on integers, place the conic on integers as well.
                 */
                if (expectIntegers) {
                    for (int index = 0; i < mapped.Length; index++) {
                        mapped[index] = new Offset(
                            Mathf.Round(mapped[index].dx),
                            Mathf.Round(mapped[index].dy)
                        );
                    }
                }

                this.conicTo(mapped[0].dx, mapped[0].dy, mapped[1].dx, mapped[1].dy, w);
                startTheta = endTheta;
            }
        }

        public void close() {
            this._appendClose();
        }

        public void winding(PathWinding dir) {
            this._appendWinding((float) dir);
        }

        public void addRect(Rect rect) {
            this._updateRRectFlag(true, PathShapeHint.Rect);
            this._appendMoveTo(rect.left, rect.top);
            this._appendLineTo(rect.left, rect.bottom);
            this._appendLineTo(rect.right, rect.bottom);
            this._appendLineTo(rect.right, rect.top);
            this._appendClose();
        }

        public void addRRect(RRect rrect) {
            this._updateRRectFlag(rrect.isNaiveRRect(), PathShapeHint.NaiveRRect, rrect.blRadiusX);
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

            this._appendMoveTo(x, y + ryTL);
            this._appendLineTo(x, y + h - ryBL);
            this._appendBezierTo(x, y + h - ryBL * (1 - _KAPPA90),
                x + rxBL * (1 - _KAPPA90), y + h, x + rxBL, y + h);
            this._appendLineTo(x + w - rxBR, y + h);
            this._appendBezierTo(x + w - rxBR * (1 - _KAPPA90), y + h,
                x + w, y + h - ryBR * (1 - _KAPPA90), x + w, y + h - ryBR);
            this._appendLineTo(x + w, y + ryTR);
            this._appendBezierTo(x + w, y + ryTR * (1 - _KAPPA90),
                x + w - rxTR * (1 - _KAPPA90), y, x + w - rxTR, y);
            this._appendLineTo(x + rxTL, y);
            this._appendBezierTo(x + rxTL * (1 - _KAPPA90), y,
                x, y + ryTL * (1 - _KAPPA90), x, y + ryTL);
            this._appendClose();
        }

        public void addEllipse(float cx, float cy, float rx, float ry) {
            this._updateRRectFlag(rx == ry, PathShapeHint.Circle, rx);
            this._appendMoveTo(cx - rx, cy);
            this._appendBezierTo(cx - rx, cy + ry * _KAPPA90,
                cx - rx * _KAPPA90, cy + ry, cx, cy + ry);
            this._appendBezierTo(cx + rx * _KAPPA90, cy + ry,
                cx + rx, cy + ry * _KAPPA90, cx + rx, cy);
            this._appendBezierTo(cx + rx, cy - ry * _KAPPA90,
                cx + rx * _KAPPA90, cy - ry, cx, cy - ry);
            this._appendBezierTo(cx - rx * _KAPPA90, cy - ry,
                cx - rx, cy - ry * _KAPPA90, cx - rx, cy);
            this._appendClose();
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
            this._updateRRectFlag(false);
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
            }
            else {
                cx = x1 + dx0 * d + -dy0 * radius;
                cy = y1 + dy0 * d + dx0 * radius;
                a0 = Mathf.Atan2(-dx0, dy0);
                a1 = Mathf.Atan2(dx1, -dy1);
                dir = PathWinding.counterClockwise;
            }

            this.addArc(cx, cy, radius, a0, a1, dir);
        }

        public void arcTo(Rect rect, float startAngle, float sweepAngle, bool forceMoveTo = true) {
            this._updateRRectFlag(false);
            var mat = Matrix3.makeScale(rect.width / 2, rect.height / 2);
            var center = rect.center;
            mat.postTranslate(center.dx, center.dy);

            this._addArcCommands(0, 0, 1, startAngle, startAngle + sweepAngle,
                sweepAngle >= 0 ? PathWinding.clockwise : PathWinding.counterClockwise, forceMoveTo, mat);
        }

        public void addArc(Rect rect, float startAngle, float sweepAngle) {
            this._updateRRectFlag(false);
            this.arcTo(rect, startAngle, sweepAngle, true);
        }

        void _addArcCommands(
            float cx, float cy, float r, float a0, float a1,
            PathWinding dir, bool forceMoveTo, Matrix3 transform = null) {
            // Clamp angles
            float da = a1 - a0;
            if (dir == PathWinding.clockwise) {
                if (Mathf.Abs(da) >= Mathf.PI * 2) {
                    da = Mathf.PI * 2;
                }
                else {
                    while (da < 0.0f) {
                        da += Mathf.PI * 2;
                    }

                    if (da <= 1e-5) {
                        return;
                    }
                }
            }
            else {
                if (Mathf.Abs(da) >= Mathf.PI * 2) {
                    da = -Mathf.PI * 2;
                }
                else {
                    while (da > 0.0f) {
                        da -= Mathf.PI * 2;
                    }

                    if (da >= -1e-5) {
                        return;
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

            for (int i = 0; i <= ndivs; i++) {
                float a = a0 + da * (i / (float) ndivs);
                float dx = Mathf.Cos(a);
                float dy = Mathf.Sin(a);
                float x = cx + dx * r;
                float y = cy + dy * r;
                float tanx = -dy * r * kappa;
                float tany = dx * r * kappa;

                if (i == 0) {
                    float x1 = x, y1 = y;
                    if (transform != null) {
                        transform.mapXY(x1, y1, out x1, out y1);
                    }

                    if (move == PathCommand.moveTo) {
                        this._appendMoveTo(x1, y1);
                    }
                    else {
                        this._appendLineTo(x1, y1);
                    }
                }
                else {
                    float c1x = px + ptanx;
                    float c1y = py + ptany;
                    float c2x = x - tanx;
                    float c2y = y - tany;
                    float x1 = x;
                    float y1 = y;
                    if (transform != null) {
                        transform.mapXY(c1x, c1y, out c1x, out c1y);
                        transform.mapXY(c2x, c2y, out c2x, out c2y);
                        transform.mapXY(x1, y1, out x1, out y1);
                    }

                    this._appendBezierTo(c1x, c1y, c2x, c2y, x1, y1);
                }

                px = x;
                py = y;
                ptanx = tanx;
                ptany = tany;
            }
        }

        public void addArc(float cx, float cy, float r, float a0, float a1, PathWinding dir, bool forceMoveTo = true) {
            this._updateRRectFlag(false);
            this._addArcCommands(cx, cy, r, a0, a1, dir, forceMoveTo);
        }

        public void addPolygon(IList<Offset> points, bool close) {
            this._updateRRectFlag(false);
            D.assert(points != null);
            if (points.Count == 0) {
                return;
            }

            this._appendMoveTo(points[0].dx, points[0].dy);

            for (int i = 1; i < points.Count; i++) {
                var point = points[i];
                this._appendLineTo(point.dx, point.dy);
            }

            if (close) {
                this._appendClose();
            }
        }

        public Path shift(Offset offset) {
            offset = offset ?? Offset.zero;
            var path = new Path();
            path.addPath(this, offset);
            return path;
        }

        public Path transform(Matrix3 mat) {
            var path = new Path();
            path.addPath(this, mat);
            return path;
        }

        public void addPath(Path path, Offset offset) {
            if (offset == null) {
                this.addPath(path);
                return;
            }

            var transform = Matrix3.makeTrans(offset.dx, offset.dy);
            this.addPath(path, transform);
        }

        public void addPath(Path path, Matrix3 transform = null) {
            D.assert(path != null);
            
            this._updateRRectFlag(path.isNaiveRRect, path.shapeHint, path.rRectCorner);
            var i = 0;
            while (i < path._commands.Count) {
                var cmd = (PathCommand) path._commands[i];
                switch (cmd) {
                    case PathCommand.moveTo: {
                        float x = path._commands[i + 1];
                        float y = path._commands[i + 2];
                        if (transform != null) {
                            transform.mapXY(x, y, out x, out y);
                        }

                        this._appendMoveTo(x, y);
                    }
                        i += 3;
                        break;
                    case PathCommand.lineTo: {
                        float x = path._commands[i + 1];
                        float y = path._commands[i + 2];
                        if (transform != null) {
                            transform.mapXY(x, y, out x, out y);
                        }

                        this._appendLineTo(x, y);
                    }
                        i += 3;
                        break;
                    case PathCommand.bezierTo: {
                        float c1x = path._commands[i + 1];
                        float c1y = path._commands[i + 2];
                        float c2x = path._commands[i + 3];
                        float c2y = path._commands[i + 4];
                        float x1 = path._commands[i + 5];
                        float y1 = path._commands[i + 6];
                        if (transform != null) {
                            transform.mapXY(c1x, c1y, out c1x, out c1y);
                            transform.mapXY(c2x, c2y, out c2x, out c2y);
                            transform.mapXY(x1, y1, out x1, out y1);
                        }

                        this._appendBezierTo(c1x, c1y, c2x, c2y, x1, y1);
                    }
                        i += 7;
                        break;
                    case PathCommand.close:
                        this._appendClose();
                        i++;
                        break;
                    case PathCommand.winding:
                        this._appendWinding(path._commands[i + 1]);
                        i += 2;
                        break;
                    default:
                        D.assert(false, () => "unknown cmd: " + cmd);
                        break;
                }
            }
        }

        public bool contains(Offset point) {
            var bounds = this.getBounds();
            if (bounds == null || bounds.isEmpty) {
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
                        D.assert(false, () => "unknown cmd: " + cmd);
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

            D.assert(r >= 0 && r < 1, () => $"numer {numer}, denom {denom}, r {r}");
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

    public class PathMetrics : IEnumerable<PathMetric> {
        public PathMetrics(IEnumerator<PathMetric> enumerator) {
            this._enumerator = enumerator;
        }

        public static PathMetrics _(Path path, bool forceClosed) {
            return new PathMetrics(PathMetricIterator._(new _PathMeasure())); // TODO: complete the implementation
        }

        public readonly IEnumerator<PathMetric> _enumerator;

        public IEnumerator<PathMetric> GetEnumerator() {
            return this._enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }

    public class PathMetric {
        // TODO
        public readonly float length;
    }

    public class PathMetricIterator : IEnumerator<PathMetric> {
        PathMetricIterator(_PathMeasure measure) {
            this._pathMeasure = measure;
        }

        internal static PathMetricIterator _(_PathMeasure _pathMeasure) {
            D.assert(_pathMeasure != null);
            return new PathMetricIterator(_pathMeasure);
        }

        // PathMetric _pathMetric; // TODO
        _PathMeasure _pathMeasure;

        public void Reset() {
            throw new NotImplementedException();
        }

        public PathMetric Current {
            get {
                return null; // TODO : return this._pathMetric;
            }
        }

        object IEnumerator.Current {
            get {
                return null; // TODO : return this._pathMetric;
            }
        }

        public bool MoveNext() {
//        if (_pathMeasure._nextContour()) {
//          _pathMetric = PathMetric._(_pathMeasure);
//          return true;
//        }
//        _pathMetric = null;
            return false;
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }

    class _PathMeasure {
    }

    public enum PathWinding {
        counterClockwise = 1, // which just means the order as the input is.
        clockwise = 2, // which just means the reversed order.
    }

    class _Conic {
        public float x0;
        public float y0;
        public float x1;
        public float y1;
        public float x2;
        public float y2;
        public float w;

        public int chopIntoQuadsPOW2(float[] quadX, float[] quadY, int pow2) {
            quadX[0] = this.x0;
            quadY[0] = this.y0;

            var endIndex = this._subdivide(quadX, quadY, 1, pow2);
            var quadCount = 1 << pow2;
            var ptCount = 2 * quadCount + 1;
            D.assert(endIndex == ptCount);

            if (!(_areFinite(quadX, 0, ptCount) && _areFinite(quadY, 0, ptCount))) {
                for (int i = 1; i < ptCount - 1; i++) {
                    quadX[i] = this.x1;
                    quadY[i] = this.y1;
                }
            }

            return quadCount;
        }

        static bool _areFinite(float[] array, int index, int count) {
            float prod = 0;

            count += index;
            for (int i = index; i < count; ++i) {
                prod *= array[i];
            }

            // At this point, prod will either be NaN or 0
            return prod == 0; // if prod is NaN, this check will return false
        }

        int _subdivide(float[] quadX, float[] quadY, int index, int level) {
            D.assert(level >= 0);

            if (0 == level) {
                quadX[0 + index] = this.x1;
                quadY[0 + index] = this.y1;
                quadX[1 + index] = this.x2;
                quadY[1 + index] = this.y2;
                return 2 + index;
            }

            _Conic c1, c2;
            this._chop(out c1, out c2);

            var startY = this.y0;
            var endY = this.y2;

            if (_between(startY, this.y1, endY)) {
                // If the input is monotonic and the output is not, the scan converter hangs.
                // Ensure that the chopped conics maintain their y-order.
                var midY = c1.y2;
                if (!_between(startY, midY, endY)) {
                    // If the computed midpoint is outside the ends, move it to the closer one.
                    var closerY = Mathf.Abs(midY - startY) < Mathf.Abs(midY - endY) ? startY : endY;
                    c1.y2 = c2.y0 = closerY;
                }

                if (!_between(startY, c1.y1, c1.y2)) {
                    // If the 1st control is not between the start and end, put it at the start.
                    // This also reduces the quad to a line.
                    c1.y1 = startY;
                }

                if (!_between(c2.y0, c2.y1, endY)) {
                    // If the 2nd control is not between the start and end, put it at the end.
                    // This also reduces the quad to a line.
                    c2.y1 = endY;
                }

                // Verify that all five points are in order.
                D.assert(_between(startY, c1.y1, c1.y2));
                D.assert(_between(c1.y1, c1.y2, c2.y1));
                D.assert(_between(c1.y2, c2.y1, endY));
            }

            --level;
            index = c1._subdivide(quadX, quadY, index, level);
            return c2._subdivide(quadX, quadY, index, level);
        }

        static bool _between(float a, float b, float c) {
            return (a - b) * (c - b) <= 0;
        }

        void _chop(out _Conic c1, out _Conic c2) {
            var scale = 1.0f / (1.0f + this.w);
            var newW = Mathf.Sqrt(0.5f + this.w * 0.5f);

            var wp1X = this.w * this.x1;
            var wp1Y = this.w * this.y1;
            var mX = (this.x0 + (wp1X + wp1X) + this.x2) * scale * 0.5f;
            var mY = (this.y0 + (wp1Y + wp1Y) + this.y2) * scale * 0.5f;

            if (!(mX.isFinite() && mY.isFinite())) {
                double w_d = this.w;
                double w_2 = w_d * 2.0;
                double scale_half = 1.0 / (1.0 + w_d) * 0.5;
                mX = (float) ((this.x0 + w_2 * this.x1 + this.x2) * scale_half);
                mY = (float) ((this.y0 + w_2 * this.y1 + this.y2) * scale_half);
            }

            c1 = new _Conic {
                x0 = this.x0,
                y0 = this.y0,
                x1 = (this.x0 + wp1X) * scale,
                y1 = (this.y0 + wp1Y) * scale,
                x2 = mX,
                y2 = mY,
                w = newW,
            };

            c2 = new _Conic {
                x0 = mX,
                y0 = mY,
                x1 = (wp1X + this.x2) * scale,
                y1 = (wp1Y + this.y2) * scale,
                x2 = this.x2,
                y2 = this.y2,
                w = newW,
            };
        }
    }
    
    public enum PathShapeHint {
        Rect,
        Circle,
        NaiveRRect,
        Other
    }

    enum PathCommand {
        moveTo,
        lineTo,
        bezierTo,
        close,
        winding,
    }

    [Flags]
    enum PointFlags {
        corner = 0x01,
        left = 0x02,
        bevel = 0x04,
        innerBevel = 0x08,
    }

    struct PathPoint {
        public float x, y;
        public float dx, dy;
        public float len;
        public float dmx, dmy;
        public PointFlags flags;
    }

    struct PathPath {
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

        readonly ArrayRef<PathPath> _paths = new ArrayRef<PathPath>();
        readonly ArrayRef<PathPoint> _points = new ArrayRef<PathPoint>();
        List<Vector3> _vertices = null;
        List<Vector2> _uv = null;
        List<Vector3> _strokeVertices = null;
        List<Vector2> _strokeUV = null;

        bool _fillConvex;

        MeshMesh _fillMesh;

        public MeshMesh fillMesh {
            get { return this._fillMesh; }
        }

        MeshMesh _strokeMesh;

        public MeshMesh strokeMesh {
            get { return this._strokeMesh; }
        }

        float _strokeWidth;
        StrokeCap _lineCap;
        StrokeJoin _lineJoin;
        float _miterLimit;
        float _fringe;

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
            this._paths.add(new PathPath {
                first = this._points.length,
                winding = PathWinding.counterClockwise
            });
        }

        public void addPoint(float x, float y, PointFlags flags) {
            this._addPoint(new PathPoint {x = x, y = y, flags = flags});
        }

        void _addPoint(PathPoint point) {
            if (this._paths.length == 0) {
                this.addPath();
                this.addPoint(0, 0, PointFlags.corner);
            }

            ref var path = ref this._paths.array[this._paths.length - 1];
            if (path.count > 0) {
                ref var pt = ref this._points.array[this._points.length - 1];
                if (PathUtils.ptEquals(pt.x, pt.y, point.x, point.y, this._distTol)) {
                    pt.flags |= point.flags;
                    return;
                }
            }

            this._points.add(point);
            path.count++;
        }

        public void tessellateBezier(
            float x2, float y2,
            float x3, float y3, float x4, float y4,
            PointFlags flags) {
            float x1, y1;
            if (this._points.length == 0) {
                x1 = 0;
                y1 = 0;
            }
            else {
                ref var pt = ref this._points.array[this._points.length - 1];
                x1 = pt.x;
                y1 = pt.y;
            }

            if (x1 == x2 && x1 == x3 && x1 == x4 &&
                y1 == y2 && y1 == y3 && y1 == y4) {
                return;
            }

            var points = TessellationGenerator.tessellateBezier(x1, y1, x2, y2, x3, y3, x4, y4, this._tessTol);
            D.assert(points.Count > 0);
            for (int i = 0; i < points.Count; i++) {
                var point = points[i];
                if (i == points.Count - 1) {
                    this._addPoint(new PathPoint {
                        x = point.x + x1,
                        y = point.y + y1,
                        flags = flags,
                    });
                }
                else {
                    this._addPoint(new PathPoint {
                        x = point.x + x1,
                        y = point.y + y1,
                    });
                }
            }
        }

        public void closePath() {
            if (this._paths.length == 0) {
                return;
            }

            ref var path = ref this._paths.array[this._paths.length - 1];
            path.closed = true;
        }

        public void pathWinding(PathWinding winding) {
            if (this._paths.length == 0) {
                return;
            }

            ref var path = ref this._paths.array[this._paths.length - 1];
            path.winding = winding;
        }

        public void normalize() {
            var points = this._points;
            var paths = this._paths;
            for (var j = 0; j < paths.length; j++) {
                ref var path = ref paths.array[j];
                if (path.count <= 1) {
                    continue;
                }

                var ip0 = path.first + path.count - 1;
                var ip1 = path.first;

                ref var p0 = ref points.array[ip0];
                ref var p1 = ref points.array[ip1];
                if (PathUtils.ptEquals(p0.x, p0.y, p1.x, p1.y, this._distTol)) {
                    path.count--;
                    path.closed = true;
                }

                if (path.count > 2) {
                    if (path.winding == PathWinding.clockwise) {
                        PathUtils.polyReverse(points.array, path.first, path.count);
                    }
                }
            }
        }

        void _calculateJoins(float w, StrokeJoin lineJoin, float miterLimit) {
            float iw = w > 0.0f ? 1.0f / w : 0.0f;

            var points = this._points;
            var paths = this._paths;
            for (var i = 0; i < paths.length; i++) {
                ref var path = ref paths.array[i];
                if (path.count <= 1) {
                    continue;
                }

                var ip0 = path.first + path.count - 1;
                var ip1 = path.first;

                for (var j = 0; j < path.count; j++) {
                    ref var p0 = ref points.array[ip0];
                    ref var p1 = ref points.array[ip1];
                    p0.dx = p1.x - p0.x;
                    p0.dy = p1.y - p0.y;
                    p0.len = PathUtils.normalize(ref p0.dx, ref p0.dy);
                    ip0 = ip1++;
                }

                ip0 = path.first + path.count - 1;
                ip1 = path.first;
                for (var j = 0; j < path.count; j++) {
                    ref var p0 = ref points.array[ip0];
                    ref var p1 = ref points.array[ip1];
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

        VertexUV _expandStroke(float w, float fringe, StrokeCap lineCap, StrokeJoin lineJoin, float miterLimit) {
            float aa = fringe;
            float u0 = 0.0f, u1 = 1.0f;
            int ncap = 0;
            if (lineCap == StrokeCap.round || lineJoin == StrokeJoin.round) {
                ncap = uiPathUtils.curveDivs(w, Mathf.PI, this._tessTol);
            }

            w += aa * 0.5f;

            if (aa == 0.0f) {
                u0 = 0.5f;
                u1 = 0.5f;
            }

            this._calculateJoins(w, lineJoin, miterLimit);

            var points = this._points;
            var paths = this._paths;

            var cvertices = 0;
            for (var i = 0; i < paths.length; i++) {
                var path = paths.array[i];
                if (path.count <= 1) {
                    continue;
                }

                cvertices += path.count * 2;
                cvertices += 8;
            }

            this._vertices = new List<Vector3>(cvertices);
            this._uv = new List<Vector2>(cvertices);
            for (var i = 0; i < paths.length; i++) {
                var path = paths.array[i];
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

                var p0 = points.array[ip0];
                var p1 = points.array[ip1];

                if (!path.closed) {
                    if (lineCap == StrokeCap.butt) {
                        this._vertices.buttCapStart(this._uv, p0, p0.dx, p0.dy, w, 0.0f, aa, u0, u1);
                    }
                    else if (lineCap == StrokeCap.square) {
                        this._vertices.buttCapStart(this._uv, p0, p0.dx, p0.dy, w, w, aa, u0, u1);
                    }
                    else {
                        // round
                        this._vertices.roundCapStart(this._uv, p0, p0.dx, p0.dy, w, ncap, u0, u1);
                    }
                }

                for (var j = s; j < e; j++) {
                    p0 = points.array[ip0];
                    p1 = points.array[ip1];

                    if ((p1.flags & (PointFlags.bevel | PointFlags.innerBevel)) != 0) {
                        if (lineJoin == StrokeJoin.round) {
                            this._vertices.roundJoin(this._uv, p0, p1, w, w, ncap, u0, u1, aa);
                        }
                        else {
                            this._vertices.bevelJoin(this._uv, p0, p1, w, w, u0, u1, aa);
                        }
                    }
                    else {
                        this._vertices.Add(new Vector2(p1.x + p1.dmx * w, p1.y + p1.dmy * w));
                        this._vertices.Add(new Vector2(p1.x - p1.dmx * w, p1.y - p1.dmy * w));
                        this._uv.Add(new Vector2(u0, 1));
                        this._uv.Add(new Vector2(u1, 1));
                    }

                    ip0 = ip1++;
                }

                if (!path.closed) {
                    p0 = points.array[ip0];
                    p1 = points.array[ip1];
                    if (lineCap == StrokeCap.butt) {
                        this._vertices.buttCapEnd(this._uv, p1, p0.dx, p0.dy, w, 0.0f, aa, u0, u1);
                    }
                    else if (lineCap == StrokeCap.square) {
                        this._vertices.buttCapEnd(this._uv, p1, p0.dx, p0.dy, w, w, aa, u0, u1);
                    }
                    else {
                        // round
                        this._vertices.roundCapEnd(this._uv, p1, p0.dx, p0.dy, w, ncap, u0, u1);
                    }
                }
                else {
                    this._vertices.Add(this._vertices[path.istroke]);
                    this._vertices.Add(this._vertices[path.istroke + 1]);
                    this._uv.Add(new Vector2(u0, 1));
                    this._uv.Add(new Vector2(u1, 1));
                }

                path.nstroke = this._vertices.Count - path.istroke;
                paths.array[i] = path;
            }

            D.assert(this._uv.Count == this._vertices.Count);

            return new VertexUV {
                strokeVertices = this._vertices,
                strokeUV = this._uv,
            };
        }

        VertexUV _expandFill(float fringe) {
            float aa = fringe;
            float woff = aa * 0.5f;
            var points = this._points;
            var paths = this._paths;
            this._calculateJoins(fringe, StrokeJoin.miter, 4.0f);

            var cvertices = 0;
            for (var i = 0; i < paths.length; i++) {
                var path = paths.array[i];
                if (path.count <= 2) {
                    continue;
                }

                cvertices += path.count;
            }

            this._fillConvex = false;
            for (var i = 0; i < paths.length; i++) {
                var path = paths.array[i];
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

            this._vertices = new List<Vector3>(cvertices);
            this._uv = new List<Vector2>(cvertices);
            for (var i = 0; i < paths.length; i++) {
                var path = paths.array[i];
                if (path.count <= 2) {
                    continue;
                }

                path.ifill = this._vertices.Count;
                for (var j = 0; j < path.count; j++) {
                    var p = points.array[path.first + j];
                    if (aa > 0.0f) {
                        this._vertices.Add(new Vector2(p.x + p.dmx * woff, p.y + p.dmy * woff));
                    }
                    else {
                        this._vertices.Add(new Vector2(p.x, p.y));
                    }

                    this._uv.Add(new Vector2(0.5f, 1.0f));
                }

                path.nfill = this._vertices.Count - path.ifill;
                paths.array[i] = path;
            }

            if (aa > 0.0f) {
                this._strokeVertices = new List<Vector3>();
                this._strokeUV = new List<Vector2>();
                cvertices = 0;
                for (var i = 0; i < paths.length; i++) {
                    var path = paths.array[i];
                    if (path.count <= 2) {
                        continue;
                    }

                    cvertices += path.count * 2;
                }

                this._strokeVertices.Capacity = cvertices;
                this._strokeUV.Capacity = cvertices;

                float lw = this._fillConvex ? woff : aa + woff;
                float rw = aa - woff;
                float lu = this._fillConvex ? 0.5f : 0.0f;
                float ru = 1.0f;

                for (var i = 0; i < paths.length; i++) {
                    var path = paths.array[i];
                    if (path.count <= 2) {
                        continue;
                    }

                    path.istroke = this._strokeVertices.Count;
                    for (var j = 0; j < path.count; j++) {
                        var p = points.array[path.first + j];
                        this._strokeVertices.Add(new Vector2(p.x + p.dmx * lw, p.y + p.dmy * lw));
                        this._strokeUV.Add(new Vector2(lu, 1.0f));
                        this._strokeVertices.Add(new Vector2(p.x - p.dmx * rw, p.y - p.dmy * rw));
                        this._strokeUV.Add(new Vector2(ru, 1.0f));
                    }

                    path.nstroke = this._strokeVertices.Count - path.istroke;
                    paths.array[i] = path;
                }
            }

            return new VertexUV {
                fillVertices = this._vertices,
                fillUV = this._uv,
                strokeVertices = this._strokeVertices,
                strokeUV = this._strokeUV,
            };
        }

        public void computeStrokeMesh(float strokeWidth, float fringe, StrokeCap lineCap, StrokeJoin lineJoin,
            float miterLimit) {
            if (this._strokeMesh != null &&
                this._fillMesh == null && // Ensure that the cached stroke mesh was not calculated in computeFillMesh
                this._strokeWidth == strokeWidth &&
                this._fringe == fringe &&
                this._lineCap == lineCap &&
                this._lineJoin == lineJoin &&
                this._miterLimit == miterLimit) {
                return;
            }

            var verticesUV = this._expandStroke(strokeWidth, fringe, lineCap, lineJoin, miterLimit);

            var paths = this._paths;

            var cindices = 0;
            for (var i = 0; i < paths.length; i++) {
                var path = paths.array[i];
                if (path.count <= 1) {
                    continue;
                }

                if (path.nstroke > 0) {
                    D.assert(path.nstroke >= 2);
                    cindices += (path.nstroke - 2) * 3;
                }
            }

            var indices = new List<int>(cindices);
            for (var i = 0; i < paths.length; i++) {
                var path = paths.array[i];
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

            this._strokeMesh = new MeshMesh(null, verticesUV.strokeVertices, indices, verticesUV.strokeUV);
            this._fillMesh = null;
            this._strokeWidth = strokeWidth;
            this._fringe = fringe;
            this._lineCap = lineCap;
            this._lineJoin = lineJoin;
            this._miterLimit = miterLimit;
        }

        public void computeFillMesh(float fringe, out bool convex) {
            if (this._fillMesh != null && (fringe != 0.0f || this._strokeMesh != null) && this._fringe == fringe) {
                convex = this._fillConvex;
                return;
            }

            var verticesUV = this._expandFill(fringe);
            convex = this._fillConvex;

            var paths = this._paths;

            var cindices = 0;
            for (var i = 0; i < paths.length; i++) {
                var path = paths.array[i];
                if (path.count <= 2) {
                    continue;
                }

                if (path.nfill > 0) {
                    D.assert(path.nfill >= 2);
                    cindices += (path.nfill - 2) * 3;
                }
            }

            var indices = new List<int>(cindices);
            for (var i = 0; i < paths.length; i++) {
                var path = paths.array[i];
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

            if (verticesUV.strokeVertices != null) {
                cindices = 0;
                for (var i = 0; i < paths.length; i++) {
                    var path = paths.array[i];
                    if (path.count <= 2) {
                        continue;
                    }

                    if (path.nstroke > 0) {
                        D.assert(path.nstroke >= 6);
                        cindices += path.nstroke * 3;
                    }
                }

                var strokeIndices = new List<int>(cindices);
                for (var i = 0; i < paths.length; i++) {
                    var path = paths.array[i];
                    if (path.count <= 2) {
                        continue;
                    }

                    if (path.nstroke > 0) {
                        strokeIndices.Add(path.istroke + path.nstroke - 1);
                        strokeIndices.Add(path.istroke + path.nstroke - 2);
                        strokeIndices.Add(path.istroke);
                        strokeIndices.Add(path.istroke + path.nstroke - 1);
                        strokeIndices.Add(path.istroke);
                        strokeIndices.Add(path.istroke + 1);
                        for (var j = 2; j < path.nstroke; j++) {
                            if ((j & 1) == 0) {
                                strokeIndices.Add(path.istroke + j - 1);
                                strokeIndices.Add(path.istroke + j - 2);
                                strokeIndices.Add(path.istroke + j);
                            }
                            else {
                                strokeIndices.Add(path.istroke + j - 2);
                                strokeIndices.Add(path.istroke + j - 1);
                                strokeIndices.Add(path.istroke + j);
                            }
                        }
                    }
                }

                D.assert(strokeIndices.Count == cindices);

                this._strokeMesh = new MeshMesh(null, verticesUV.strokeVertices, strokeIndices, verticesUV.strokeUV);
            }

            var mesh = new MeshMesh(null, verticesUV.fillVertices, indices, verticesUV.fillUV);
            this._fillMesh = mesh;
            this._fringe = fringe;
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

        public static void polyReverse(PathPoint[] pts, int s, int npts) {
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

        public static void buttCapStart(this List<Vector3> dst, List<Vector2> uv, PathPoint p,
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

        public static void buttCapEnd(this List<Vector3> dst, List<Vector2> uv, PathPoint p,
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

        public static void roundCapStart(this List<Vector3> dst, List<Vector2> uv, PathPoint p,
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

        public static void roundCapEnd(this List<Vector3> dst, List<Vector2> uv, PathPoint p,
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

        public static void roundJoin(this List<Vector3> dst, List<Vector2> uv, PathPoint p0, PathPoint p1,
            float lw, float rw, int ncap, float lu, float ru, float fringe) {
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
                chooseBevel((p1.flags & PointFlags.innerBevel) != 0, p0, p1, -rw,
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

        public static void bevelJoin(this List<Vector3> dst, List<Vector2> uv, PathPoint p0, PathPoint p1,
            float lw, float rw, float lu, float ru, float fringe) {
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
                uv.Add(new Vector2(lu, 1));
                uv.Add(new Vector2(ru, 1));

                if ((p1.flags & PointFlags.bevel) != 0) {
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
                chooseBevel((p1.flags & PointFlags.innerBevel) != 0, p0, p1, -rw,
                    out rx0, out ry0, out rx1, out ry1);

                dst.Add(new Vector2(p1.x + dlx0 * lw, p1.y + dly0 * lw));
                dst.Add(new Vector2(rx0, ry0));
                uv.Add(new Vector2(lu, 1));
                uv.Add(new Vector2(ru, 1));

                if ((p1.flags & PointFlags.bevel) != 0) {
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
                mesh.MarkDynamic();
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