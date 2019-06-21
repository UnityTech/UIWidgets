using System;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial struct uiMatrix3 {
        //Constants
        enum TypeMask {
            kIdentity_Mask = 0, //!< identity SkMatrix; all bits clear
            kTranslate_Mask = 0x01, //!< translation SkMatrix
            kScale_Mask = 0x02, //!< scale SkMatrix
            kAffine_Mask = 0x04, //!< skew or rotate SkMatrix
            kPerspective_Mask = 0x08, //!< perspective SkMatrix
        }

        const int kRectStaysRect_Mask = 0x10;

        const int kOnlyPerspectiveValid_Mask = 0x40;

        const int kUnknown_Mask = 0x80;

        const int kORableMasks =
            (int)
            (TypeMask.kTranslate_Mask |
             TypeMask.kScale_Mask |
             TypeMask.kAffine_Mask |
             TypeMask.kPerspective_Mask);

        const int kAllMasks =
            (int)
            (TypeMask.kTranslate_Mask |
             TypeMask.kScale_Mask |
             TypeMask.kAffine_Mask |
             TypeMask.kPerspective_Mask) |
            kRectStaysRect_Mask;
        
        enum TypeShift {
            kTranslate_Shift,
            kScale_Shift,
            kAffine_Shift,
            kPerspective_Shift,
            kRectStaysRect_Shift
        }
        
        const int kScalar1Int = 0x3f800000;
    }

    
    public partial struct uiMatrix3 {
        //Variables
        //public readonly float fMat[9];
        public float kMScaleX;        //0
        public float kMSkewX;        //1
        public float kMTransX;        //2
        public float kMSkewY;         //3
        public float kMScaleY;        //4
        public float kMTransY;        //5
        public float kMPersp0;        //6
        public float kMPersp1;        //7
        public float kMPersp2;        //8
        
        public int fTypeMask;
    }

    public partial struct uiMatrix3 {
        //private methods
        void _setScale(float sx, float sy) {
            if (1 == sx && 1 == sy) {
                this.reset();
            }
            else {
                this.kMScaleX = sx;
                this.kMScaleY = sy;
                this.kMPersp2 = 1;

                this.kMTransX = this.kMTransY = this.kMSkewX =
                    this.kMSkewY = this.kMPersp0 = this.kMPersp1 = 0;

                this._setTypeMask((int) TypeMask.kScale_Mask | kRectStaysRect_Mask);
            }
        }
        
        void _setScale(float sx, float sy, float px, float py) {
            if (1 == sx && 1 == sy) {
                this.reset();
            }
            else {
                this._setScaleTranslate(sx, sy, px - sx * px, py - sy * py);
            }
        }
        
        int _computeTypeMask() {
            int mask = 0;

            if (this.kMPersp0 != 0 || this.kMPersp1 != 0 ||
                this.kMPersp2 != 1) {
                // Once it is determined that that this is a perspective transform,
                // all other flags are moot as far as optimizations are concerned.
                return kORableMasks;
            }

            if (this.kMTransX != 0 || this.kMTransY != 0) {
                mask |= (int) TypeMask.kTranslate_Mask;
            }

            int m00 = uiScalarUtils.ScalarAs2sCompliment(this.kMScaleX);
            int m01 = uiScalarUtils.ScalarAs2sCompliment(this.kMSkewX);
            int m10 = uiScalarUtils.ScalarAs2sCompliment(this.kMSkewY);
            int m11 = uiScalarUtils.ScalarAs2sCompliment(this.kMScaleY);

            if ((m01 != 0) | (m10 != 0)) {
                mask |= (int) TypeMask.kAffine_Mask | (int) TypeMask.kScale_Mask;

                m01 = m01 != 0 ? 1 : 0;
                m10 = m10 != 0 ? 1 : 0;

                int dp0 = 0 == (m00 | m11) ? 1 : 0;
                int ds1 = m01 & m10;

                mask |= (dp0 & ds1) << (int) TypeShift.kRectStaysRect_Shift;
            }
            else {
                if (((m00 ^ kScalar1Int) | (m11 ^ kScalar1Int)) != 0) {
                    mask |= (int) TypeMask.kScale_Mask;
                }

                m00 = m00 != 0 ? 1 : 0;
                m11 = m11 != 0 ? 1 : 0;

                mask |= (m00 & m11) << (int) TypeShift.kRectStaysRect_Shift;
            }

            return mask;
        }
        
        
        TypeMask _getType() {
            if ((this.fTypeMask & kUnknown_Mask) != 0) {
                this.fTypeMask = this._computeTypeMask();
            }
            // only return the public masks
            return (TypeMask) (this.fTypeMask & 0xF);
        }
        
        public void reset() {
            this.kMScaleX = this.kMScaleY = this.kMPersp2 = 1;
            
            this.kMSkewX = this.kMSkewY = this.kMTransX =
                this.kMTransY = this.kMPersp0 = this.kMPersp1 = 0;
            
            this._setTypeMask((int) TypeMask.kIdentity_Mask | kRectStaysRect_Mask);
        }
        
        bool _isTriviallyIdentity() {
            if ((this.fTypeMask & kUnknown_Mask) != 0) {
                return false;
            }

            return (this.fTypeMask & 0xF) == 0;
        }
        
        void _setConcat(uiMatrix3 a, uiMatrix3 b) {
            TypeMask aType = a._getType();
            TypeMask bType = b._getType();

            if (a._isTriviallyIdentity()) {
                this.copyFrom(b);
            }
            else if (b._isTriviallyIdentity()) {
                this.copyFrom(a);
            }
            else if (_only_scale_and_translate((int) aType | (int) bType)) {
                this._setScaleTranslate(a.kMScaleX * b.kMScaleX,
                    a.kMScaleY * b.kMScaleY,
                    a.kMScaleX * b.kMTransX + a.kMTransX,
                    a.kMScaleY * b.kMTransY + a.kMTransY);
            }
            else {
                uiMatrix3 tmp = new uiMatrix3();

                if (((aType | bType) & TypeMask.kPerspective_Mask) != 0) {
                    tmp.kMScaleX = _rowcol3(a, 0, b, 0);
                    tmp.kMSkewX = _rowcol3(a, 0, b, 1);
                    tmp.kMTransX = _rowcol3(a, 0, b, 2);
                    tmp.kMSkewY = _rowcol3(a, 3, b, 0);
                    tmp.kMScaleY = _rowcol3(a, 3, b, 1);
                    tmp.kMTransY = _rowcol3(a, 3, b, 2);
                    tmp.kMPersp0 = _rowcol3(a, 6, b, 0);
                    tmp.kMPersp1 = _rowcol3(a, 6, b, 1);
                    tmp.kMPersp2 = _rowcol3(a, 6, b, 2);

                    tmp._setTypeMask(kUnknown_Mask);
                }
                else {
                    tmp.kMScaleX = _muladdmul(a.kMScaleX,
                        b.kMScaleX,
                        a.kMSkewX,
                        b.kMSkewY);

                    tmp.kMSkewX = _muladdmul(a.kMScaleX,
                        b.kMSkewX,
                        a.kMSkewX,
                        b.kMScaleY);

                    tmp.kMTransX = _muladdmul(a.kMScaleX,
                                             b.kMTransX,
                                             a.kMSkewX,
                                             b.kMTransY) + a.kMTransX;

                    tmp.kMSkewY = _muladdmul(a.kMSkewY,
                        b.kMScaleX,
                        a.kMScaleY,
                        b.kMSkewY);

                    tmp.kMScaleY = _muladdmul(a.kMSkewY,
                        b.kMSkewX,
                        a.kMScaleY,
                        b.kMScaleY);

                    tmp.kMTransY = _muladdmul(a.kMSkewY,
                                             b.kMTransX,
                                             a.kMScaleY,
                                             b.kMTransY) + a.kMTransY;

                    tmp.kMPersp0 = 0;
                    tmp.kMPersp1 = 0;
                    tmp.kMPersp2 = 1;
                    tmp._setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
                }

                this.copyFrom(tmp);
            }
        }
        
        
        void _setScaleTranslate(float sx, float sy, float tx, float ty) {
            this.kMScaleX = sx;
            this.kMSkewX = 0;
            this.kMTransX = tx;

            this.kMSkewY = 0;
            this.kMScaleY = sy;
            this.kMTransY = ty;

            this.kMPersp0 = 0;
            this.kMPersp1 = 0;
            this.kMPersp2 = 1;

            int mask = 0;
            if (sx != 1 || sy != 1) {
                mask |= (int) TypeMask.kScale_Mask;
            }

            if (tx != 0 || ty != 0) {
                mask |= (int) TypeMask.kTranslate_Mask;
            }

            this._setTypeMask(mask | kRectStaysRect_Mask);
        }
        
        void _setTypeMask(int mask) {
            D.assert(kUnknown_Mask == mask || (mask & kAllMasks) == mask || 
                     ((kUnknown_Mask | kOnlyPerspectiveValid_Mask) & mask) == 
                     (kUnknown_Mask | kOnlyPerspectiveValid_Mask));
            this.fTypeMask = mask;
        }
        
        
        int _computePerspectiveTypeMask() {
            if (this.kMPersp0 != 0 || this.kMPersp1 != 0 ||
                this.kMPersp2 != 1) {
                return kORableMasks;
            }
            return kOnlyPerspectiveValid_Mask | kUnknown_Mask;
        }
        
        TypeMask _getPerspectiveTypeMaskOnly() {
            if ((this.fTypeMask & kUnknown_Mask) != 0 &&
                (this.fTypeMask & kOnlyPerspectiveValid_Mask) == 0) {
                this.fTypeMask = this._computePerspectiveTypeMask();
            }

            return (TypeMask) (this.fTypeMask & 0xF);
        }
        
        bool _hasPerspective() {
            return (this._getPerspectiveTypeMaskOnly() & TypeMask.kPerspective_Mask) != 0;
        }
        
        void _updateTranslateMask() {
            if ((this.kMTransX != 0) | (this.kMTransY != 0)) {
                this.fTypeMask |= (int) TypeMask.kTranslate_Mask;
            }
            else {
                this.fTypeMask &= ~(int) TypeMask.kTranslate_Mask;
            }
        }
        
        public bool _isFinite() {
            return uiScalarUtils.ScalarsAreFinite(this);
        }
        
        uiMatrix3? _invertNonIdentity(bool invertableCheck) {
            D.assert(!this.isIdentity());
            TypeMask mask = this._getType();

            if (0 == (mask & ~(TypeMask.kScale_Mask | TypeMask.kTranslate_Mask))) {
                bool invertible = true;
                
                if (!invertableCheck) {
                    if ((mask & TypeMask.kScale_Mask) != 0) {
                        var invX = this.kMScaleX;
                        var invY = this.kMScaleY;
                        if (0 == invX || 0 == invY) {
                            return null;
                        }

                        invX = 1f / invX;
                        invY = 1f / invY;

                        var _inv = I();
                        _inv.kMSkewX = _inv.kMSkewY =
                        _inv.kMPersp0 = _inv.kMPersp1 = 0;

                        _inv.kMScaleX = invX;
                        _inv.kMScaleY = invY;
                        _inv.kMPersp2 = 1;
                        _inv.kMTransX = -this.kMTransX * invX;
                        _inv.kMTransY = -this.kMTransY * invY;

                        _inv._setTypeMask((int) mask | kRectStaysRect_Mask);
                        return _inv;
                    }
                    else {
                        var _inv = I();
                        _inv.setTranslate(-this.kMTransX, -this.kMTransY);
                        return _inv;
                    }
                }
                else {
                    if (this.kMScaleX == 0 || this.kMScaleY == 0) {
                        return null;
                    }
                    return I();
                }
            }

            int isPersp = (int) (mask & TypeMask.kPerspective_Mask);
            float invDet = uiScalarUtils.inv_determinant(this, isPersp);

            if (invDet == 0) {
                // underflow
                return null;
            }
            var inv = ComputeInv(this, invDet, isPersp != 0);
            if (!inv._isFinite()) {
                return null;
            }

            inv._setTypeMask(this.fTypeMask);
            return inv;
        }
        
        public bool _isScaleTranslate() {
            return (this._getType() & ~(TypeMask.kScale_Mask | TypeMask.kTranslate_Mask)) == 0;
        }
        
        public uiRect _mapRectScaleTranslate(uiRect src) {
            D.assert(this._isScaleTranslate());

            var sx = this.kMScaleX;
            var sy = this.kMScaleY;
            var tx = this.kMTransX;
            var ty = this.kMTransY;

            var dst = uiRectHelper.fromLTRB(
                src.left * sx + tx,
                src.top * sy + ty,
                src.right * sx + tx,
                src.bottom * sy + ty
            );
            
            dst = uiRectHelper.normalize(dst);
            return dst;
        }
    }
    
    
    public partial struct uiMatrix3 {
        //public methods  
        public uiMatrix3(uiMatrix3 other) {
            
            this.kMScaleX = other.kMScaleX;
        this.kMSkewX = other.kMSkewX;
        this.kMTransX = other.kMTransX;      
        this.kMSkewY = other.kMSkewY;      
        this.kMScaleY = other.kMScaleY;
        
        this.kMTransY = other.kMTransY;
        
        this.kMPersp0 = other.kMPersp0;
        
        this.kMPersp1 = other.kMPersp1;
        
        this.kMPersp2 = other.kMPersp2;
            this.fTypeMask = other.fTypeMask;
        }
        
        
        public void copyFrom(uiMatrix3 other) {
    this.kMScaleX = other.kMScaleX;
    this.kMSkewX = other.kMSkewX;
    this.kMTransX = other.kMTransX;      
    this.kMSkewY = other.kMSkewY;      
    this.kMScaleY = other.kMScaleY;
        
    this.kMTransY = other.kMTransY;
        
    this.kMPersp0 = other.kMPersp0;
        
    this.kMPersp1 = other.kMPersp1;
        
    this.kMPersp2 = other.kMPersp2;
            this.fTypeMask = other.fTypeMask;
        }
        
        
        public bool isIdentity() {
            return this._getType() == 0;
        }
        
        public float getScaleX() {
            return this.kMScaleX;
        }

        public float getScaleY() {
            return this.kMScaleY;
        }

        public float getSkewY() {
            return this.kMSkewY;
        }

        public float getSkewX() {
            return this.kMSkewX;
        }

        public float getTranslateX() {
            return this.kMTransX;
        }

        public float getTranslateY() {
            return this.kMTransY;
        }

        public float getPerspX() {
            return this.kMPersp0;
        }

        public float getPerspY() {
            return this.kMPersp1;
        }
        
        public void postConcat(uiMatrix3 mat) {
            if (!mat.isIdentity()) {
                this._setConcat(mat, this);
            }
        }

        public void setSinCos(float sinV, float cosV, float px, float py) {
            var oneMinusCosV = 1 - cosV;

            this.kMScaleX = cosV;
            this.kMSkewX = -sinV;
            this.kMTransX = uiScalarUtils.sdot(sinV, py, oneMinusCosV, px);

            this.kMSkewY = sinV;
            this.kMScaleY = cosV;
            this.kMTransY = uiScalarUtils.sdot(-sinV, px, oneMinusCosV, py);

            this.kMPersp0 = this.kMPersp1 = 0;
            this.kMPersp2 = 1;

            this._setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
        }

        public void setSinCos(float sinV, float cosV) {
            this.kMScaleX = cosV;
            this.kMSkewX = -sinV;
            this.kMTransX = 0;

            this.kMSkewY = sinV;
            this.kMScaleY = cosV;
            this.kMTransY = 0;

            this.kMPersp0 = this.kMPersp1 = 0;
            this.kMPersp2 = 1;

            this._setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
        }     
        
        public void setTranslate(float dx, float dy) {
            if ((dx != 0) | (dy != 0)) {
                this.kMTransX = dx;
                this.kMTransY = dy;

                this.kMScaleX = this.kMScaleY = this.kMPersp2 = 1;
                this.kMSkewX = this.kMSkewY =
                    this.kMPersp0 = this.kMPersp1 = 0;

                this._setTypeMask((int) TypeMask.kTranslate_Mask | kRectStaysRect_Mask);
            }
            else {
                this.reset();
            }
        }
        
        
        public void postTranslate(float dx, float dy) {
            if (this._hasPerspective()) {
                var m = new uiMatrix3();
                m.setTranslate(dx, dy);
                this.postConcat(m);
            }
            else {
                this.kMTransX += dx;
                this.kMTransY += dy;
                this._updateTranslateMask();
            }
        }

        public void postScale(float sx, float sy) {
            if (1 == sx && 1 == sy) {
                return;
            }

            var m = new uiMatrix3();
            m._setScale(sx, sy);
            this.postConcat(m);
        }
        
        public bool rectStaysRect() {
            if ((this.fTypeMask & kUnknown_Mask) != 0) {
                this.fTypeMask = this._computeTypeMask();
            }

            return (this.fTypeMask & kRectStaysRect_Mask) != 0;
        }
        
        public uiMatrix3? invert(bool invertableCheck = false) {
            if (this.isIdentity()) {
                return I();
            }

            return this._invertNonIdentity(invertableCheck);
        }
        
        public uiRect mapRect(uiRect src) {
            if (this._getType() <= TypeMask.kTranslate_Mask) {
                var tx = this.kMTransX;
                var ty = this.kMTransY;

                var dst = uiRectHelper.fromLTRB(
                    src.left + tx,
                    src.top + ty,
                    src.right + tx,
                    src.bottom + ty
                );
                dst = uiRectHelper.normalize(dst);
                return dst;
            }

            if (this._isScaleTranslate()) {
                return this._mapRectScaleTranslate(src);
            }
            else {
                float x1, y1, x2, y2, x3, y3, x4, y4;
                this.mapXY(src.left, src.top, out x1, out y1);
                this.mapXY(src.right, src.top, out x2, out y2);
                this.mapXY(src.right, src.bottom, out x3, out y3);
                this.mapXY(src.left, src.bottom, out x4, out y4);

                var minX = x1;
                var minY = y1;
                var maxX = x1;
                var maxY = y1;

                if (x2 < minX) {
                    minX = x2;
                }
                if (x2 > maxX) {
                    maxX = x2;
                }
                if (y2 < minY) {
                    minY = y2;
                }
                if (y2 > maxY) {
                    maxY = y2;
                }

                if (x3 < minX) {
                    minX = x3;
                }
                if (x3 > maxX) {
                    maxX = x3;
                }
                if (y3 < minY) {
                    minY = y3;
                }
                if (y3 > maxY) {
                    maxY = y3;
                }

                if (x4 < minX) {
                    minX = x4;
                }
                if (x4 > maxX) {
                    maxX = x4;
                }
                if (y4 < minY) {
                    minY = y4;
                }
                if (y4 > maxY) {
                    maxY = y4;
                }

                var dst = uiRectHelper.fromLTRB(minX, minY, maxX, maxY);
                return dst;
            }
        }
        
        public void mapXY(float x, float y, out float x1, out float y1) {
            this._getMapXYProc()(this, x, y, out x1, out y1);
        }
        
        public Matrix4x4 toMatrix4x4() {
            var matrix = Matrix4x4.identity;

            matrix[0, 0] = this.kMScaleX; // row 0
            matrix[0, 1] = this.kMSkewX;
            matrix[0, 3] = this.kMTransX;

            matrix[1, 0] = this.kMSkewY; // row 1
            matrix[1, 1] = this.kMScaleY;
            matrix[1, 3] = this.kMTransY;

            matrix[3, 0] = this.kMPersp0; // row 2
            matrix[3, 1] = this.kMPersp1;
            matrix[3, 3] = this.kMPersp2;

            return matrix;
        }
        
        public void preTranslate(float dx, float dy) {
            var mask = this._getType();

            if (mask <= TypeMask.kTranslate_Mask) {
                this.kMTransX += dx;
                this.kMTransY += dy;
            }
            else if ((mask & TypeMask.kPerspective_Mask) != 0) {
                var m = new uiMatrix3();
                m.setTranslate(dx, dy);
                this.preConcat(m);
                return;
            }
            else {
                this.kMTransX += this.kMScaleX * dx + this.kMSkewX * dy;
                this.kMTransY += this.kMSkewY * dx + this.kMScaleY * dy;
            }

            this._updateTranslateMask();
        }
        
        public void preConcat(uiMatrix3 other) {
            if (!other.isIdentity()) {
                this._setConcat(this, other);
            }
        }
    }

    public partial struct uiMatrix3 {
        public void mapPoints(Offset[] dst, Offset[] src) {
            D.assert(dst != null && src != null && dst.Length == src.Length);
            this._getMapPtsProc()(this, dst, src, src.Length);
        }

        public void mapPoints(Offset[] pts) {
            this.mapPoints(pts, pts);
        }
        
        delegate void MapPtsProc(uiMatrix3 mat, Offset[] dst, Offset[] src, int count);

        static readonly MapPtsProc[] gMapPtsProcs = {
            Identity_pts, Trans_pts,
            Scale_pts, Scale_pts,
            Affine_pts, Affine_pts,
            Affine_pts, Affine_pts,
            // repeat the persp proc 8 times
            Persp_pts, Persp_pts,
            Persp_pts, Persp_pts,
            Persp_pts, Persp_pts,
            Persp_pts, Persp_pts
        };

        static MapPtsProc GetMapPtsProc(TypeMask mask) {
            D.assert(((int) mask & ~kAllMasks) == 0);
            return gMapPtsProcs[(int) mask & kAllMasks];
        }

        MapPtsProc _getMapPtsProc() {
            return GetMapPtsProc(this._getType());
        }

        static void Identity_pts(uiMatrix3 m, Offset[] dst, Offset[] src, int count) {
            D.assert(m._getType() == 0);

            if (dst != src && count > 0) {
                Array.Copy(src, dst, count);
            }
        }

        static void Trans_pts(uiMatrix3 m, Offset[] dst, Offset[] src, int count) {
            D.assert(m._getType() <= TypeMask.kTranslate_Mask);
            if (count > 0) {
                var tx = m.getTranslateX();
                var ty = m.getTranslateY();
                for (int i = 0; i < count; ++i) {
                    dst[i] = new Offset(src[i].dx + tx, src[i].dy + ty);
                }
            }
        }

        static void Scale_pts(uiMatrix3 m, Offset[] dst, Offset[] src, int count) {
            D.assert(m._getType() <= (TypeMask.kScale_Mask | TypeMask.kTranslate_Mask));
            if (count > 0) {
                var tx = m.getTranslateX();
                var ty = m.getTranslateY();
                var sx = m.getScaleX();
                var sy = m.getScaleY();

                for (int i = 0; i < count; ++i) {
                    dst[i] = new Offset(src[i].dx * sx + tx, src[i].dy * sy + ty);
                }
            }
        }

        static void Persp_pts(uiMatrix3 m, Offset[] dst, Offset[] src, int count) {
            D.assert(m._hasPerspective());

            if (count > 0) {
                for (int i = 0; i < count; ++i) {
                    var sy = src[i].dy;
                    var sx = src[i].dx;
                    var x = uiScalarUtils.sdot(sx, m.kMScaleX, sy, m.kMSkewX) +
                            m.kMTransX;
                    var y = uiScalarUtils.sdot(sx, m.kMSkewY, sy, m.kMScaleY) +
                            m.kMTransY;
                    var z = uiScalarUtils.sdot(sx, m.kMPersp0, sy, m.kMPersp1) +
                            m.kMPersp2;
                    if (z != 0) {
                        z = 1 / z;
                    }

                    dst[i] = new Offset(x * z, y * z);
                }
            }
        }

        static void Affine_pts(uiMatrix3 m, Offset[] dst, Offset[] src, int count) {
            D.assert(m._getType() != TypeMask.kPerspective_Mask);
            if (count > 0) {
                var tx = m.getTranslateX();
                var ty = m.getTranslateY();
                var sx = m.getScaleX();
                var sy = m.getScaleY();
                var kx = m.getSkewX();
                var ky = m.getSkewY();

                for (int i = 0; i < count; ++i) {
                    dst[i] = new Offset(
                        src[i].dx * sx + src[i].dy * kx + tx,
                        src[i].dx * ky + src[i].dy * sy + ty);
                }
            }
        }
    }


    public partial struct uiMatrix3 {
        delegate void MapXYProc(uiMatrix3 mat, float x, float y, out float x1, out float y1);

        static readonly MapXYProc[] gMapXYProcs = {
            Identity_xy, Trans_xy,
            Scale_xy, ScaleTrans_xy,
            Rot_xy, RotTrans_xy,
            Rot_xy, RotTrans_xy,
            // repeat the persp proc 8 times
            Persp_xy, Persp_xy,
            Persp_xy, Persp_xy,
            Persp_xy, Persp_xy,
            Persp_xy, Persp_xy
        };

        static MapXYProc GetMapXYProc(TypeMask mask) {
            D.assert(((int) mask & ~kAllMasks) == 0);
            return gMapXYProcs[(int) mask & kAllMasks];
        }

        MapXYProc _getMapXYProc() {
            return GetMapXYProc(this._getType());
        }

        static void Identity_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert(0 == m._getType());

            resX = sx;
            resY = sy;
        }

        static void Trans_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert(m._getType() == TypeMask.kTranslate_Mask);

            resX = sx + m.kMTransX;
            resY = sy + m.kMTransY;
        }

        static void Scale_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m._getType() & (TypeMask.kScale_Mask | TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask))
                     == TypeMask.kScale_Mask);
            D.assert(0 == m.kMTransX);
            D.assert(0 == m.kMTransY);

            resX = sx * m.kMScaleX;
            resY = sy * m.kMScaleY;
        }

        static void ScaleTrans_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m._getType() & (TypeMask.kScale_Mask | TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask))
                     == TypeMask.kScale_Mask);

            resX = sx * m.kMScaleX + m.kMTransX;
            resY = sy * m.kMScaleY + m.kMTransY;
        }

        static void Rot_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m._getType() & (TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask)) == TypeMask.kAffine_Mask);
            D.assert(0 == m.kMTransX);
            D.assert(0 == m.kMTransY);

            resX = uiScalarUtils.sdot(sx, m.kMScaleX, sy, m.kMSkewX);
            resY = uiScalarUtils.sdot(sx, m.kMSkewY, sy, m.kMScaleY);
        }

        static void RotTrans_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m._getType() & (TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask)) == TypeMask.kAffine_Mask);

            resX = uiScalarUtils.sdot(sx, m.kMScaleX, sy, m.kMSkewX) + m.kMTransX;
            resY = uiScalarUtils.sdot(sx, m.kMSkewY, sy, m.kMScaleY) + m.kMTransY;
        }

        static void Persp_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert(m._hasPerspective());

            float x = uiScalarUtils.sdot(sx, m.kMScaleX, sy, m.kMSkewX) +
                      m.kMTransX;
            float y = uiScalarUtils.sdot(sx, m.kMSkewY, sy, m.kMScaleY) +
                      m.kMTransY;
            float z = uiScalarUtils.sdot(sx, m.kMPersp0, sy, m.kMPersp1) +
                      m.kMPersp2;
            if (z != 0) {
                z = 1 / z;
            }

            resX = x * z;
            resY = y * z;
        }
    }

    
    public partial struct uiMatrix3 {
        //static methods
        public static uiMatrix3 I() {
            var m = new uiMatrix3();
            m.reset();
            return m;
        }
        
        public static uiMatrix3 makeTrans(float dx, float dy) {
            var m = new uiMatrix3();
            m.setTranslate(dx, dy);
            return m;
        }
        
        public static uiMatrix3 makeScale(float sx, float sy) {
            var m = new uiMatrix3();
            m._setScale(sx, sy);
            return m;
        }
        
        public static uiMatrix3 makeRotate(float radians) {
            var m = new uiMatrix3();
            m.setRotate(radians);
            return m;
        }

        public static uiMatrix3 makeRotate(float radians, float px, float py) {
            var m = new uiMatrix3();
            m.setRotate(radians, px, py);
            return m;
        }

        public static uiMatrix3 makeTrans(Offset offset) {
            var m = new uiMatrix3();
            m.setTranslate(offset.dx, offset.dy);
            return m;
        }

        public static uiMatrix3 makeSkew(float dx, float dy) {
            var m = new uiMatrix3();
            m.setSkew(dx, dy);
            return m;
        }
        
        public static uiMatrix3 concat(uiMatrix3 a, uiMatrix3 b) {
            uiMatrix3 result = I();
            result._setConcat(a, b);
            return result;
        }
        
        public void setRotate(float radians, float px, float py) {
            float sinV, cosV;
            sinV = uiScalarUtils.ScalarSinCos(radians, out cosV);
            this.setSinCos(sinV, cosV, px, py);
        }

        public void setRotate(float radians) {
            float sinV, cosV;
            sinV = uiScalarUtils.ScalarSinCos(radians, out cosV);
            this.setSinCos(sinV, cosV);
        }
        
        public void setSkew(float kx, float ky, float px, float py) {
            this.kMScaleX = 1;
            this.kMSkewX = kx;
            this.kMTransX = -kx * py;

            this.kMSkewY = ky;
            this.kMScaleY = 1;
            this.kMTransY = -ky * px;

            this.kMPersp0 = this.kMPersp1 = 0;
            this.kMPersp2 = 1;

            this._setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
        }

        public void setSkew(float kx, float ky) {
            this.kMScaleX = 1;
            this.kMSkewX = kx;
            this.kMTransX = 0;

            this.kMSkewY = ky;
            this.kMScaleY = 1;
            this.kMTransY = 0;

            this.kMPersp0 = this.kMPersp1 = 0;
            this.kMPersp2 = 1;

            this._setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
        }
        
        public static bool equals(uiMatrix3? a, uiMatrix3? b) {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) {
                return true;
            }

            if (ReferenceEquals(a, b)) {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;
            }
            var ma = a.Value;
            var mb = b.Value;

            return ma.kMScaleX == mb.kMScaleX && ma.kMSkewX == mb.kMSkewX && ma.kMTransX == mb.kMTransX &&
                   ma.kMSkewY == mb.kMSkewY && ma.kMScaleY == mb.kMScaleY && ma.kMTransY == mb.kMTransY &&
                   ma.kMPersp0 == mb.kMPersp0 && ma.kMPersp1 == mb.kMPersp1 && ma.kMPersp2 == mb.kMPersp2;
        }
    }
    
    
    public partial struct uiMatrix3 {
        public static uiMatrix3 fromMatrix3(Matrix3 mat3) {
            var uiMat3 = I();

            
            uiMat3.kMScaleX = mat3[0];
            uiMat3.kMSkewX = mat3[1];
            uiMat3.kMTransX = mat3[2];
            uiMat3.kMSkewY = mat3[3];
            uiMat3.kMScaleY = mat3[4];
            uiMat3.kMTransY = mat3[5];
            uiMat3.kMPersp0 = mat3[6];
            uiMat3.kMPersp1 = mat3[7];
            uiMat3.kMPersp2 = mat3[8];

            uiMat3._setTypeMask(kUnknown_Mask);
            return uiMat3;
        }
        
        static bool _only_scale_and_translate(int mask) {
            return 0 == (mask & (int) (TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask));
        }

        static float _getitem(uiMatrix3 mat, int i) {
            switch (i)
            {
                case 0:
                    return mat.kMScaleX;
                case 1:
                    return mat.kMSkewX;
                case 2:
                    return mat.kMTransX;
                case 3:
                    return mat.kMSkewY;
                case 4:
                    return mat.kMScaleY;
                case 5:
                    return mat.kMTransY;
                case 6:
                    return mat.kMPersp0;
                case 7:
                    return mat.kMPersp1;
                case 8:
                    return mat.kMPersp2;
                default: {
                    return -1;
                }
            }
        }
        
        static float _rowcol3(uiMatrix3 row, int i, uiMatrix3 col, int j) {
            return _getitem(row,i) * _getitem(col,j) + _getitem(row,i + 1) * _getitem(col,j + 3) + 
                   _getitem(row,i + 2) * _getitem(col,j + 6);
        }

        static float _muladdmul(float a, float b, float c, float d) {
            return (float) ((double) a * b + (double) c * d);
        }
        
        static uiMatrix3 ComputeInv(uiMatrix3 src, float invDet, bool isPersp) {
            uiMatrix3 dst = I();
            if (isPersp) {
                dst.kMScaleX =
                    uiScalarUtils.scross_dscale(src.kMScaleY, src.kMPersp2, src.kMTransY,
                        src.kMPersp1, invDet);
                dst.kMSkewX =
                    uiScalarUtils.scross_dscale(src.kMTransX, src.kMPersp1, src.kMSkewX,
                        src.kMPersp2, invDet);
                dst.kMTransX =
                    uiScalarUtils.scross_dscale(src.kMSkewX, src.kMTransY, src.kMTransX,
                        src.kMScaleY, invDet);

                dst.kMSkewY =
                    uiScalarUtils.scross_dscale(src.kMTransY, src.kMPersp0, src.kMSkewY,
                        src.kMPersp2, invDet);
                dst.kMScaleY =
                    uiScalarUtils.scross_dscale(src.kMScaleX, src.kMPersp2, src.kMTransX,
                        src.kMPersp0, invDet);
                dst.kMTransY =
                    uiScalarUtils.scross_dscale(src.kMTransX, src.kMSkewY, src.kMScaleX,
                        src.kMTransY, invDet);

                dst.kMPersp0 =
                    uiScalarUtils.scross_dscale(src.kMSkewY, src.kMPersp1, src.kMScaleY,
                        src.kMPersp0, invDet);
                dst.kMPersp1 =
                    uiScalarUtils.scross_dscale(src.kMSkewX, src.kMPersp0, src.kMScaleX,
                        src.kMPersp1, invDet);
                dst.kMPersp2 =
                    uiScalarUtils.scross_dscale(src.kMScaleX, src.kMScaleY, src.kMSkewX,
                        src.kMSkewY, invDet);
            }
            else {
                // not perspective
                dst.kMScaleX = src.kMScaleY * invDet;
                dst.kMSkewX = -src.kMSkewX * invDet;
                dst.kMTransX =
                    uiScalarUtils.dcross_dscale(src.kMSkewX, src.kMTransY, src.kMScaleY,
                        src.kMTransX, invDet);

                dst.kMSkewY = -src.kMSkewY * invDet;
                dst.kMScaleY = src.kMScaleX * invDet;
                dst.kMTransY =
                    uiScalarUtils.dcross_dscale(src.kMSkewY, src.kMTransX, src.kMScaleX,
                        src.kMTransY, invDet);

                dst.kMPersp0 = 0;
                dst.kMPersp1 = 0;
                dst.kMPersp2 = 1;
            }

            return dst;
        }
        
        //utils
        class uiScalarUtils {
            public const float kScalarNearlyZero = 1f / (1 << 12);

            public static bool ScalarNearlyZero(float x, float tolerance = kScalarNearlyZero) {
                D.assert(tolerance >= 0);
                return Mathf.Abs(x) <= tolerance;
            }

            public static bool ScalarNearlyEqual(float x, float y, float tolerance = kScalarNearlyZero) {
                D.assert(tolerance >= 0);
                return Mathf.Abs(x - y) <= tolerance;
            }

            public static bool ScalarIsInteger(float scalar) {
                return scalar == Mathf.FloorToInt(scalar);
            }

            public static float DegreesToRadians(float degrees) {
                return degrees * (Mathf.PI / 180);
            }

            public static float RadiansToDegrees(float radians) {
                return radians * (180 / Mathf.PI);
            }

            public static float ScalarSinCos(float radians, out float cosValue) {
                float sinValue = Mathf.Sin(radians);

                cosValue = Mathf.Cos(radians);
                if (ScalarNearlyZero(cosValue)) {
                    cosValue = 0;
                }

                if (ScalarNearlyZero(sinValue)) {
                    sinValue = 0;
                }

                return sinValue;
            }

            public static bool ScalarsAreFinite(uiMatrix3 m) {
                float prod = m.kMScaleX * m.kMSkewX * m.kMTransX * m.kMSkewY * m.kMScaleY * m.kMTransY * m.kMPersp0 * m.kMPersp1 * m.kMPersp2;
                // At this point, prod will either be NaN or 0
                return prod == 0; // if prod is NaN, this check will return false
            }

            public static int ScalarAs2sCompliment(float x) {
                var result = (int) x;
                if (result < 0) {
                    result &= 0x7FFFFFFF;
                    result = -result;
                }

                return result;
            }

            public static float sdot(float a, float b, float c, float d) {
                return a * b + c * d;
            }

            public static float sdot(float a, float b, float c, float d, float e, float f) {
                return a * b + c * d + e * f;
            }

            public static float scross(float a, float b, float c, float d) {
                return a * b - c * d;
            }

            public static double dcross(double a, double b, double c, double d) {
                return a * b - c * d;
            }

            public static float scross_dscale(float a, float b,
                float c, float d, double scale) {
                return (float) (scross(a, b, c, d) * scale);
            }

            public static float dcross_dscale(double a, double b,
                double c, double d, double scale) {
                return (float) (dcross(a, b, c, d) * scale);
            }

            public static bool is_degenerate_2x2(
                float scaleX, float skewX,
                float skewY, float scaleY) {
                float perp_dot = scaleX * scaleY - skewX * skewY;
                return ScalarNearlyZero(perp_dot,
                    kScalarNearlyZero * kScalarNearlyZero);
            }

            public static float inv_determinant(uiMatrix3 mat, int isPerspective) {
                double det;

                if (isPerspective != 0) {
                    det = mat.kMScaleX *
                          dcross(mat.kMScaleY, mat.kMPersp2,
                              mat.kMTransY, mat.kMPersp1)
                          +
                          mat.kMSkewX *
                          dcross(mat.kMTransY, mat.kMPersp0,
                              mat.kMSkewY, mat.kMPersp2)
                          +
                          mat.kMTransX *
                          dcross(mat.kMSkewY, mat.kMPersp1,
                              mat.kMScaleY, mat.kMPersp0);
                }
                else {
                    det = dcross(mat.kMScaleX, mat.kMScaleY,
                        mat.kMSkewX, mat.kMSkewY);
                }

                // Since the determinant is on the order of the cube of the matrix members,
                // compare to the cube of the default nearly-zero constant (although an
                // estimate of the condition number would be better if it wasn't so expensive).
                if (ScalarNearlyZero((float) det,
                    kScalarNearlyZero * kScalarNearlyZero * kScalarNearlyZero)) {
                    return 0;
                }

                return 1.0f / (float) det;
            }
        }
    }
}