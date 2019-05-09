using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class TessellationKey : IEquatable<TessellationKey> {
        public readonly float x2;
        public readonly float y2;
        public readonly float x3;
        public readonly float y3;
        public readonly float x4;
        public readonly float y4;
        public readonly float tessTol;

        public TessellationKey(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4,
            float tessTol) {
            this.x2 = x2 - x1;
            this.y2 = y2 - y1;
            this.x3 = x3 - x1;
            this.y3 = y3 - y1;
            this.x4 = x4 - x1;
            this.y4 = y4 - y1;
            this.tessTol = tessTol;
        }

        public bool Equals(TessellationKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.x2 == other.x2 && this.y2 == other.y2 && this.x3 == other.x3 &&
                   this.y3 == other.y3 && this.x4 == other.x4 && this.y4 == other.y4 &&
                   this.tessTol == other.tessTol;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((TessellationKey) obj);
        }

        public override unsafe int GetHashCode() {
            unchecked {
                var hashCode = 0;
                float x = this.x2;
                hashCode ^= *(int*) &x;
                x = this.y2;
                hashCode = (hashCode * 13) ^ *(int*) &x;
                x = this.x3;
                hashCode = (hashCode * 13) ^ *(int*) &x;
                x = this.y3;
                hashCode = (hashCode * 13) ^ *(int*) &x;
                x = this.x4;
                hashCode = (hashCode * 13) ^ *(int*) &x;
                x = this.y4;
                hashCode = (hashCode * 13) ^ *(int*) &x;
                x = this.tessTol;
                hashCode = (hashCode * 13) ^ *(int*) &x;
                return hashCode;
            }
        }

        public static bool operator ==(TessellationKey left, TessellationKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(TessellationKey left, TessellationKey right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"TessellationKey(" +
                   $"x2: {this.x2}, " +
                   $"y2: {this.y2}, " +
                   $"x3: {this.x3}, " +
                   $"y3: {this.y3}, " +
                   $"x4: {this.x4}, " +
                   $"y4: {this.y4}, " +
                   $"tessTol: {this.tessTol})";
        }
    }

    class TessellationInfo {
        public readonly TessellationKey key;
        public readonly List<Vector2> points;
        long _timeToLive;

        public TessellationInfo(TessellationKey key, List<Vector2> points, int timeToLive = 5) {
            this.points = points;
            this.key = key;
            this.touch(timeToLive);
        }

        public long timeToLive {
            get { return this._timeToLive; }
        }

        public void touch(long timeTolive = 5) {
            this._timeToLive = timeTolive + TextBlobMesh.frameCount;
        }
    }


    static class TessellationGenerator {
        static readonly Dictionary<TessellationKey, TessellationInfo> _tessellations =
            new Dictionary<TessellationKey, TessellationInfo>();

        static long _frameCount = 0;

        public static long frameCount {
            get { return _frameCount; }
        }

        public static int tessellationCount {
            get { return _tessellations.Count; }
        }

        public static void tickNextFrame() {
            _frameCount++;
            var keysToRemove = _tessellations.Values.Where(info => info.timeToLive < _frameCount)
                .Select(info => info.key).ToList();
            foreach (var key in keysToRemove) {
                _tessellations.Remove(key);
            }
        }

        public static List<Vector2> tessellateBezier(float x1, float y1, float x2, float y2,
            float x3, float y3, float x4, float y4, float tessTol) {
            var key = new TessellationKey(x1, y1, x2, y2, x3, y3, x4, y4, tessTol);

            _tessellations.TryGetValue(key, out var tessellationInfo);
            if (tessellationInfo != null) {
                tessellationInfo.touch();

                return tessellationInfo.points;
            }
            
            var points = _tessellateBezier(x1, y1, x2, y2, x3, y3, x4, y4, tessTol);
            _tessellations[key] = new TessellationInfo(key, points);

            return points;
        }

        struct _StackData {
            public float x1;
            public float y1;
            public float x2;
            public float y2;
            public float x3;
            public float y3;
            public float x4;
            public float y4;
            public int level;
        }

        static readonly Stack<_StackData> _stack = new Stack<_StackData>();

        static List<Vector2> _tessellateBezier(
            float x1, float y1, float x2, float y2,
            float x3, float y3, float x4, float y4,
            float tessTol) {
            x2 = x2 - x1;
            y2 = y2 - y1;
            x3 = x3 - x1;
            y3 = y3 - y1;
            x4 = x4 - x1;
            y4 = y4 - y1;

            var points = new List<Vector2>();

            _stack.Clear();
            _stack.Push(new _StackData {
                x1 = 0, y1 = 0, x2 = x2, y2 = y2, x3 = x3, y3 = y3, x4 = x4, y4 = y4, level = 0,
            });

            while (_stack.Count > 0) {
                var stackData = _stack.Pop();
                x1 = stackData.x1;
                y1 = stackData.y1;
                x2 = stackData.x2;
                y2 = stackData.y2;
                x3 = stackData.x3;
                y3 = stackData.y3;
                x4 = stackData.x4;
                y4 = stackData.y4;
                int level = stackData.level;

                float dx = x4 - x1;
                float dy = y4 - y1;
                float d2 = Mathf.Abs((x2 - x4) * dy - (y2 - y4) * dx);
                float d3 = Mathf.Abs((x3 - x4) * dy - (y3 - y4) * dx);

                if ((d2 + d3) * (d2 + d3) <= tessTol * (dx * dx + dy * dy)) {
                    points.Add(new Vector2(x4, y4));
                    continue;
                }

                float x12 = (x1 + x2) * 0.5f;
                float y12 = (y1 + y2) * 0.5f;
                float x23 = (x2 + x3) * 0.5f;
                float y23 = (y2 + y3) * 0.5f;
                float x34 = (x3 + x4) * 0.5f;
                float y34 = (y3 + y4) * 0.5f;
                float x123 = (x12 + x23) * 0.5f;
                float y123 = (y12 + y23) * 0.5f;
                float x234 = (x23 + x34) * 0.5f;
                float y234 = (y23 + y34) * 0.5f;
                float x1234 = (x123 + x234) * 0.5f;
                float y1234 = (y123 + y234) * 0.5f;

                if (level < 10) {
                    _stack.Push(new _StackData {
                        x1 = x1234, y1 = y1234, x2 = x234, y2 = y234, x3 = x34, y3 = y34, x4 = x4, y4 = y4,
                        level = level + 1,
                    });
                    _stack.Push(new _StackData {
                        x1 = x1, y1 = y1, x2 = x12, y2 = y12, x3 = x123, y3 = y123, x4 = x1234, y4 = y1234,
                        level = level + 1,
                    });
                }
            }

            return points;
        }
    }
}