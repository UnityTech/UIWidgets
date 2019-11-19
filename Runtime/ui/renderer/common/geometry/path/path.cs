using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial class uiPath : PoolObject {
        const float _KAPPA90 = 0.5522847493f;

        uiList<float> _commands;
        float _commandx;
        float _commandy;
        float _minX, _minY;
        float _maxX, _maxY;

        uiPathCache _cache;

        public uint pathKey = 0;
        public bool needCache = false;
        
        bool _isNaiveRRect = false;
        public bool isNaiveRRect => this._isNaiveRRect;
        
        uiPathShapeHint _shapeHint = uiPathShapeHint.Other;
        public uiPathShapeHint shapeHint => this._shapeHint;
        
        float _rRectCorner;
        public float rRectCorner => this._rRectCorner;
        
        void _updateRRectFlag(bool isNaiveRRect, uiPathShapeHint shapeHint = uiPathShapeHint.Other, float corner = 0) {
            if (this._commands.Count > 0 && !this._isNaiveRRect) {
                return;
            }
            this._isNaiveRRect = isNaiveRRect && this._hasOnlyMoveTos();
            this._shapeHint = shapeHint;
            this._rRectCorner = corner;
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

        public static uiPath create(int capacity = 128) {
            uiPath newPath = ObjectPool<uiPath>.alloc();
            newPath._reset();
            return newPath;
        }

        public uiPath() {
        }

        public override void clear() {
            ObjectPool<uiList<float>>.release(this._commands);
            ObjectPool<uiPathCache>.release(this._cache);
            this._cache = null;
            this._commands = null;

            this.needCache = false;
            this.pathKey = 0;
            this._isNaiveRRect = false;
            this._shapeHint = uiPathShapeHint.Other;
            this._rRectCorner = 0;
        }

        void _reset() {
            this._commands = ObjectPool<uiList<float>>.alloc();
            this._commandx = 0;
            this._commandy = 0;
            this._minX = float.MaxValue;
            this._minY = float.MaxValue;
            this._maxX = float.MinValue;
            this._maxY = float.MinValue;
            ObjectPool<uiPathCache>.release(this._cache);
            this._cache = null;
            this._isNaiveRRect = false;
        }

        internal uiPathCache flatten(float scale) {
            scale = Mathf.Round(scale * 2.0f) / 2.0f; // round to 0.5f

            if (this._cache != null && this._cache.canReuse(scale)) {
                return this._cache;
            }

            var _cache = uiPathCache.create(scale, this._shapeHint);

            var i = 0;
            while (i < this._commands.Count) {
                var cmd = (uiPathCommand) this._commands[i];
                switch (cmd) {
                    case uiPathCommand.moveTo:
                        _cache.addPath();
                        _cache.addPoint(this._commands[i + 1], this._commands[i + 2], uiPointFlags.corner);
                        i += 3;
                        break;
                    case uiPathCommand.lineTo:
                        _cache.addPoint(this._commands[i + 1], this._commands[i + 2], uiPointFlags.corner);
                        i += 3;
                        break;
                    case uiPathCommand.bezierTo:
                        _cache.tessellateBezier(
                            this._commands[i + 1], this._commands[i + 2],
                            this._commands[i + 3], this._commands[i + 4],
                            this._commands[i + 5], this._commands[i + 6], uiPointFlags.corner);
                        i += 7;
                        break;
                    case uiPathCommand.close:
                        _cache.closePath();
                        i++;
                        break;
                    case uiPathCommand.winding:
                        _cache.pathWinding((uiPathWinding) this._commands[i + 1]);
                        i += 2;
                        break;
                    default:
                        D.assert(false, () => "unknown cmd: " + cmd);
                        break;
                }
            }

            _cache.normalize();
            ObjectPool<uiPathCache>.release(this._cache);
            this._cache = _cache;
            return _cache;
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
        
        public uiRect getBounds() {
            if (this._minX >= this._maxX || this._minY >= this._maxY) {
                return uiRectHelper.zero;
            }

            return uiRectHelper.fromLTRB(this._minX, this._minY, this._maxX, this._maxY);
        }

        void _appendMoveTo(float x, float y) {
            this._commands.Add((float) uiPathCommand.moveTo);
            this._commands.Add(x);
            this._commands.Add(y);

            this._commandx = x;
            this._commandy = y;

            ObjectPool<uiPathCache>.release(this._cache);
            this._cache = null;
        }

        void _appendLineTo(float x, float y) {
            this._expandBounds(this._commandx, this._commandy);
            this._expandBounds(x, y);

            this._commands.Add((float) uiPathCommand.lineTo);
            this._commands.Add(x);
            this._commands.Add(y);

            this._commandx = x;
            this._commandy = y;

            ObjectPool<uiPathCache>.release(this._cache);
            this._cache = null;
        }

        void _appendBezierTo(float x1, float y1, float x2, float y2, float x3, float y3) {
            this._expandBounds(this._commandx, this._commandy);
            this._expandBounds(x1, y1);
            this._expandBounds(x2, y2);
            this._expandBounds(x3, y3);

            this._commands.Add((float) uiPathCommand.bezierTo);
            this._commands.Add(x1);
            this._commands.Add(y1);
            this._commands.Add(x2);
            this._commands.Add(y2);
            this._commands.Add(x3);
            this._commands.Add(y3);

            this._commandx = x3;
            this._commandy = y3;

            ObjectPool<uiPathCache>.release(this._cache);
            this._cache = null;
        }

        void _appendClose() {
            this._commands.Add((float) uiPathCommand.close);

            ObjectPool<uiPathCache>.release(this._cache);
            this._cache = null;
        }

        void _appendWinding(float winding) {
            this._commands.Add((float) uiPathCommand.winding);
            this._commands.Add(winding);

            ObjectPool<uiPathCache>.release(this._cache);
            this._cache = null;
        }

        public void addRect(uiRect rect) {
            this._updateRRectFlag(true, uiPathShapeHint.Rect);
            this._appendMoveTo(rect.left, rect.top);
            this._appendLineTo(rect.left, rect.bottom);
            this._appendLineTo(rect.right, rect.bottom);
            this._appendLineTo(rect.right, rect.top);
            this._appendClose();
        }

        public void addRect(Rect rect) {
            this._updateRRectFlag(true, uiPathShapeHint.Rect);
            this._appendMoveTo(rect.left, rect.top);
            this._appendLineTo(rect.left, rect.bottom);
            this._appendLineTo(rect.right, rect.bottom);
            this._appendLineTo(rect.right, rect.top);
            this._appendClose();
        }

        public void addRRect(RRect rrect) {
            this._updateRRectFlag(rrect.isNaiveRRect(), uiPathShapeHint.NaiveRRect, rrect.blRadiusX);
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

        public void moveTo(float x, float y) {
            this._appendMoveTo(x, y);
        }

        public void lineTo(float x, float y) {
            this._updateRRectFlag(false);
            this._appendLineTo(x, y);
        }

        public void winding(PathWinding dir) {
            this._appendWinding((float) dir);
        }

        public void addEllipse(float cx, float cy, float rx, float ry) {
            this._updateRRectFlag(rx == ry, uiPathShapeHint.Circle, rx);
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

        public void arcTo(Rect rect, float startAngle, float sweepAngle, bool forceMoveTo = true) {
            this._updateRRectFlag(false);
            var mat = Matrix3.makeScale(rect.width / 2, rect.height / 2);
            var center = rect.center;
            mat.postTranslate(center.dx, center.dy);

            this._addArcCommands(0, 0, 1, startAngle, startAngle + sweepAngle,
                sweepAngle >= 0 ? PathWinding.clockwise : PathWinding.counterClockwise, forceMoveTo, mat);
        }

        public void close() {
            this._appendClose();
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

        public static uiPath fromPath(Path path) {
            D.assert(path != null);

            uiPath uipath;
            bool exists = uiPathCacheManager.tryGetUiPath(path.pathKey, out uipath);
            if (exists) {
                return uipath;
            }
            
            uipath._updateRRectFlag(path.isNaiveRRect, (uiPathShapeHint)path.shapeHint, path.rRectCorner);
            
            var i = 0;
            var _commands = path.commands;
            while (i < _commands.Count) {
                var cmd = (uiPathCommand) _commands[i];
                switch (cmd) {
                    case uiPathCommand.moveTo: {
                        float x = _commands[i + 1];
                        float y = _commands[i + 2];
                        uipath._appendMoveTo(x, y);
                    }
                        i += 3;
                        break;
                    case uiPathCommand.lineTo: {
                        float x = _commands[i + 1];
                        float y = _commands[i + 2];

                        uipath._appendLineTo(x, y);
                    }
                        i += 3;
                        break;
                    case uiPathCommand.bezierTo: {
                        float c1x = _commands[i + 1];
                        float c1y = _commands[i + 2];
                        float c2x = _commands[i + 3];
                        float c2y = _commands[i + 4];
                        float x1 = _commands[i + 5];
                        float y1 = _commands[i + 6];

                        uipath._appendBezierTo(c1x, c1y, c2x, c2y, x1, y1);
                    }
                        i += 7;
                        break;
                    case uiPathCommand.close:
                        uipath._appendClose();
                        i++;
                        break;
                    case uiPathCommand.winding:
                        uipath._appendWinding(_commands[i + 1]);
                        i += 2;
                        break;
                    default:
                        D.assert(false, () => "unknown cmd: " + cmd);
                        break;
                }
            }

            return uipath;
        }
    }
}