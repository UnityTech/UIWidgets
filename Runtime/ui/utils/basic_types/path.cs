using System;
using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.foundation;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Unity.UIWidgets.ui {
    public class uiPath : PoolItem {
        const float _KAPPA90 = 0.5522847493f;

        uiList<float> _commands;
        float _commandx;
        float _commandy;
        float _minX, _minY;
        float _maxX, _maxY;

        public static uiPath create(int capacity = 128) {
            uiPath newPath = ItemPoolManager.alloc<uiPath>();
            newPath._reset();
            return newPath;
        }

        public uiPath() {
        }

        public override void clear() {
            this._commands.dispose();
        }

        void _reset() {
            this._commands = ItemPoolManager.alloc<uiList<float>>();
            this._commandx = 0;
            this._commandy = 0;
            this._minX = float.MaxValue;
            this._minY = float.MaxValue;
            this._maxX = float.MinValue;
            this._maxY = float.MinValue;
        }

        internal uiPathCache flatten(float scale) {
            scale = Mathf.Round(scale * 2.0f) / 2.0f; // round to 0.5f
            
            var _cache = uiPathCache.create(scale);

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

        void _appendMoveTo(float x, float y) {
            this._commands.Add((float) uiPathCommand.moveTo);
            this._commands.Add(x);
            this._commands.Add(y);

            this._commandx = x;
            this._commandy = y;
        }

        void _appendLineTo(float x, float y) {
            this._expandBounds(this._commandx, this._commandy);
            this._expandBounds(x, y);

            this._commands.Add((float) uiPathCommand.lineTo);
            this._commands.Add(x);
            this._commands.Add(y);

            this._commandx = x;
            this._commandy = y;
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
        }
        
        void _appendClose() {
            this._commands.Add((float) uiPathCommand.close);
        }

        void _appendWinding(float winding) {
            this._commands.Add((float) uiPathCommand.winding);
            this._commands.Add(winding);
        }

        public void addRect(Rect rect) {
            this._appendMoveTo(rect.left, rect.top);
            this._appendLineTo(rect.left, rect.bottom);
            this._appendLineTo(rect.right, rect.bottom);
            this._appendLineTo(rect.right, rect.top);
            this._appendClose();
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

        public static uiPath fromPath(Path path) {
            D.assert(path != null);

            uiPath uipath = uiPath.create();

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