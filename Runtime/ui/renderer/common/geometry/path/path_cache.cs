using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    
    class uiPathCache : PoolObject {
        float _distTol;
        float _tessTol;

        List<uiPathPath> _paths = new List<uiPathPath>();
        List<uiPathPoint> _points = new List<uiPathPoint>();

        float _scale;
        
        //mesh cache
        uiMeshMesh _fillMesh;
        bool _fillConvex;

        uiMeshMesh _strokeMesh;
        float _strokeWidth;
        StrokeCap _lineCap;
        StrokeJoin _lineJoin;
        float _miterLimit;

        public static uiPathCache create(float scale) {
            uiPathCache newPathCache = ObjectPool<uiPathCache>.alloc();
            newPathCache._distTol = 0.01f / scale;
            newPathCache._tessTol = 0.25f / scale;
            newPathCache._scale = scale;
            return newPathCache;
        }
        
        public bool canReuse(float scale) {
            if (this._scale != scale) {
                return false;
            }

            return true;
        }

        public override void clear() {
            this._paths.Clear();
            this._points.Clear();
            ObjectPool<uiMeshMesh>.release(this._fillMesh);
            this._fillMesh = null;
            
            ObjectPool<uiMeshMesh>.release(this._strokeMesh);
            this._strokeMesh = null;
        }

        public uiPathCache() {
        }

        public void addPath() {
            this._paths.Add(uiPathPath.create(
                first : this._points.Count,
                winding : uiPathWinding.counterClockwise
            ));
        }

        public void addPoint(float x, float y, uiPointFlags flags) {
            this._addPoint(uiPathPoint.create(x : x, y : y, flags : flags));
        }

        void _addPoint(uiPathPoint point) {
            if (this._paths.Count == 0) {
                this.addPath();
                this.addPoint(0, 0, uiPointFlags.corner);
            }
            
            var path = this._paths[this._paths.Count - 1];
            if (path.count > 0) {
                var pt = this._points[this._points.Count - 1];
                if (uiPathUtils.ptEquals(pt.x, pt.y, point.x, point.y, this._distTol)) {
                    pt.flags |= point.flags;
                    this._points[this._points.Count - 1] = pt;
                    return;
                }
            }

            this._points.Add(point);
            path.count++;
            this._paths[this._paths.Count - 1] = path;
        }

        public void tessellateBezier(
            float x2, float y2,
            float x3, float y3, float x4, float y4,
            uiPointFlags flags) {
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

            if (x1 == x2 && x1 == x3 && x1 == x4 &&
                y1 == y2 && y1 == y3 && y1 == y4) {
                return;
            }

            var points = uiTessellationGenerator.tessellateBezier(x1, y1, x2, y2, x3, y3, x4, y4, this._tessTol);
            D.assert(points.Count > 0);
            for (int i = 0; i < points.Count; i++) {
                var point = points[i];
                if (i == points.Count - 1) {
                    this._addPoint(uiPathPoint.create(
                        x : point.x + x1,
                        y : point.y + y1,
                        flags : flags
                    ));
                } else {
                    this._addPoint(uiPathPoint.create(
                        x : point.x + x1,
                        y : point.y + y1
                    ));
                }
            }
        }

        public void closePath() {
            if (this._paths.Count == 0) {
                return;
            }

            var path = this._paths[this._paths.Count - 1];
            path.closed = true;
            this._paths[this._paths.Count - 1] = path;
        }

        public void pathWinding(uiPathWinding winding) {
            if (this._paths.Count == 0) {
                return;
            }

            var path =  this._paths[this._paths.Count - 1];
            path.winding = winding;
            this._paths[this._paths.Count - 1] = path;
        }

        public void normalize() {
            var points = this._points;
            var paths = this._paths;            
            for (var j = 0; j < paths.Count; j++) {
                 var path =  paths[j];
                if (path.count <= 1) {
                    continue;
                }

                var ip0 = path.first + path.count - 1;
                var ip1 = path.first;

                 var p0 =  points[ip0];
                 var p1 =  points[ip1];
                if (uiPathUtils.ptEquals(p0.x, p0.y, p1.x, p1.y, this._distTol)) {
                    path.count--;
                    path.closed = true;
                    paths[j] = path;
                }

                if (path.count > 2) {
                    if (path.winding == uiPathWinding.clockwise) {
                        uiPathUtils.polyReverse(points, path.first, path.count);
                    }
                }
            }
        }

        uiList<Vector3> _expandFill() {
            var points = this._points;
            var paths = this._paths;
            for (var j = 0; j < paths.Count; j++) {
                 var path =  paths[j];
                if (path.count <= 2) {
                    continue;
                }

                var ip0 = path.first + path.count - 1;
                var ip1 = path.first;
                for (var i = 0; i < path.count; i++) {
                     var p0 =  points[ip0];
                     var p1 =  points[ip1];
                    p0.dx = p1.x - p0.x; // no need to normalize
                    p0.dy = p1.y - p0.y;
                    points[ip0] = p0;
                    ip0 = ip1++;
                }

                path.convex = true;

                ip0 = path.first + path.count - 1;
                ip1 = path.first;
                for (var i = 0; i < path.count; i++) {
                     var p0 =  points[ip0];
                     var p1 =  points[ip1];

                    float cross = p1.dx * p0.dy - p0.dx * p1.dy;
                    if (cross < 0.0f) {
                        path.convex = false;
                    }

                    ip0 = ip1++;
                }

                paths[j] = path;
            }


            var cvertices = 0;
            for (var i = 0; i < paths.Count; i++) {
                 var path =  paths[i];
                if (path.count <= 2) {
                    continue;
                }

                cvertices += path.count;
            }

            var _vertices = ObjectPool<uiList<Vector3>>.alloc();
            _vertices.SetCapacity(cvertices);
            for (var i = 0; i < paths.Count; i++) {
                 var path =  paths[i];
                if (path.count <= 2) {
                    continue;
                }

                path.ifill = _vertices.Count;
                for (var j = 0; j < path.count; j++) {
                     var p =  points[path.first + j];
                    _vertices.Add(new Vector2(p.x, p.y));
                }

                path.nfill = _vertices.Count - path.ifill;
                paths[i] = path;
            }

            return _vertices;
        }

        public uiMeshMesh getFillMesh(out bool convex) {
            if (this._fillMesh != null) {
                convex = this._fillConvex;
                return this._fillMesh;
            }
            
            var vertices = this._expandFill();

            var paths = this._paths;
            
            var cindices = 0;
            for (var i = 0; i < paths.Count; i++) {
                 var path =  paths[i];
                if (path.count <= 2) {
                    continue;
                }

                if (path.nfill > 0) {
                    D.assert(path.nfill >= 2);
                    cindices += (path.nfill - 2) * 3;
                }
            }

            var indices = ObjectPool<uiList<int>>.alloc();
            indices.SetCapacity(cindices);
            for (var i = 0; i < paths.Count; i++) {
                 var path =  paths[i];
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

            var mesh = uiMeshMesh.create(null, vertices, indices);
            this._fillMesh = mesh;
            
             this._fillConvex = false;
            for (var i = 0; i < paths.Count; i++) {
                 var path =  paths[i];
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

            var points = this._points;
            var paths = this._paths;
            for (var i = 0; i < paths.Count; i++) {
                 var path =  paths[i];
                if (path.count <= 1) {
                    continue;
                }

                var ip0 = path.first + path.count - 1;
                var ip1 = path.first;

                for (var j = 0; j < path.count; j++) {
                     var p0 =  points[ip0];
                     var p1 =  points[ip1];
                    p0.dx = p1.x - p0.x;
                    p0.dy = p1.y - p0.y;
                    p0.len = uiPathUtils.normalize(ref p0.dx, ref p0.dy);
                    points[ip0] = p0;
                    ip0 = ip1++;
                }

                ip0 = path.first + path.count - 1;
                ip1 = path.first;
                for (var j = 0; j < path.count; j++) {
                     var p0 =  points[ip0];
                     var p1 =  points[ip1];
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
                    p1.flags &= uiPointFlags.corner;

                    // Keep track of left turns.
                    float cross = p1.dx * p0.dy - p0.dx * p1.dy;
                    if (cross > 0.0f) {
                        p1.flags |= uiPointFlags.left;
                    }

                    // Calculate if we should use bevel or miter for inner join.
                    float limit = Mathf.Max(1.01f, Mathf.Min(p0.len, p1.len) * iw);
                    if (dmr2 * limit * limit < 1.0f) {
                        p1.flags |= uiPointFlags.innerBevel;
                    }

                    // Check to see if the corner needs to be beveled.
                    if ((p1.flags & uiPointFlags.corner) != 0) {
                        if (lineJoin == StrokeJoin.bevel ||
                            lineJoin == StrokeJoin.round || dmr2 * miterLimit * miterLimit < 1.0f) {
                            p1.flags |= uiPointFlags.bevel;
                        }
                    }
                    
                    points[ip0] = p0;
                    points[ip1] = p1; 

                    ip0 = ip1++;
                }
            }
        }

        uiList<Vector3> _expandStroke(float w, StrokeCap lineCap, StrokeJoin lineJoin, float miterLimit) {
            this._calculateJoins(w, lineJoin, miterLimit);

            int ncap = 0;
            if (lineCap == StrokeCap.round || lineJoin == StrokeJoin.round) {
                ncap = uiPathUtils.curveDivs(w, Mathf.PI, this._tessTol);
            }

            var points = this._points;
            var paths = this._paths;

            var cvertices = 0;
            for (var i = 0; i < paths.Count; i++) {
                 var path =  paths[i];
                if (path.count <= 1) {
                    continue;
                }

                cvertices += path.count * 2;
                cvertices += 4;
            }

            var _vertices = ObjectPool<uiList<Vector3>>.alloc();
            _vertices.SetCapacity(cvertices);
            for (var i = 0; i < paths.Count; i++) {
                 var path =  paths[i];
                if (path.count <= 1) {
                    continue;
                }

                path.istroke = _vertices.Count;

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

                 var p0 =  points[ip0];
                 var p1 =  points[ip1];

                if (!path.closed) {
                    if (lineCap == StrokeCap.butt) {
                        _vertices.buttCapStart(p0, p0.dx, p0.dy, w, 0.0f);
                    }
                    else if (lineCap == StrokeCap.square) {
                        _vertices.buttCapStart(p0, p0.dx, p0.dy, w, w);
                    }
                    else {
                        // round
                        _vertices.roundCapStart(p0, p0.dx, p0.dy, w, ncap);
                    }
                }

                for (var j = s; j < e; j++) {
                    p0 =  points[ip0];
                    p1 =  points[ip1];

                    if ((p1.flags & (uiPointFlags.bevel | uiPointFlags.innerBevel)) != 0) {
                        if (lineJoin == StrokeJoin.round) {
                            _vertices.roundJoin(p0, p1, w, w, ncap);
                        }
                        else {
                            _vertices.bevelJoin(p0, p1, w, w);
                        }
                    }
                    else {
                        _vertices.Add(new Vector2(p1.x + p1.dmx * w, p1.y + p1.dmy * w));
                        _vertices.Add(new Vector2(p1.x - p1.dmx * w, p1.y - p1.dmy * w));
                    }

                    ip0 = ip1++;
                }

                if (!path.closed) {
                    p0 =  points[ip0];
                    p1 =  points[ip1];
                    if (lineCap == StrokeCap.butt) {
                        _vertices.buttCapEnd(p1, p0.dx, p0.dy, w, 0.0f);
                    }
                    else if (lineCap == StrokeCap.square) {
                        _vertices.buttCapEnd(p1, p0.dx, p0.dy, w, w);
                    }
                    else {
                        // round
                        _vertices.roundCapEnd(p1, p0.dx, p0.dy, w, ncap);
                    }
                }
                else {
                    _vertices.Add(_vertices[path.istroke]);
                    _vertices.Add(_vertices[path.istroke + 1]);
                }

                path.nstroke = _vertices.Count - path.istroke;
                paths[i] = path;
            }

            return _vertices;
        }

        public uiMeshMesh getStrokeMesh(float strokeWidth, StrokeCap lineCap, StrokeJoin lineJoin, float miterLimit) {
            if (this._strokeMesh != null &&
                this._strokeWidth == strokeWidth &&
                this._lineCap == lineCap &&
                this._lineJoin == lineJoin &&
                this._miterLimit == miterLimit) {
                return this._strokeMesh;
            }
            
            var vertices = this._expandStroke(strokeWidth, lineCap, lineJoin, miterLimit);

            var paths = this._paths;
            
            var cindices = 0;
            for (var i = 0; i < paths.Count; i++) {
                 var path =  paths[i];
                if (path.count <= 1) {
                    continue;
                }

                if (path.nstroke > 0) {
                    D.assert(path.nstroke >= 2);
                    cindices += (path.nstroke - 2) * 3;
                }
            }

            var indices = ObjectPool<uiList<int>>.alloc();
            indices.SetCapacity(cindices);
            for (var i = 0; i < paths.Count; i++) {
                var path =  paths[i];
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

            ObjectPool<uiMeshMesh>.release(this._strokeMesh);
            this._strokeMesh = uiMeshMesh.create(null, vertices, indices);
            this._strokeWidth = strokeWidth;
            this._lineCap = lineCap;
            this._lineJoin = lineJoin;
            this._miterLimit = miterLimit;        
            return this._strokeMesh;
        }
    }
}