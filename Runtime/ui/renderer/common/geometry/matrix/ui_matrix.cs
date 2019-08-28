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
        public float kMScaleX; //0
        public float kMSkewX; //1
        public float kMTransX; //2
        public float kMSkewY; //3
        public float kMScaleY; //4
        public float kMTransY; //5
        public float kMPersp0; //6
        public float kMPersp1; //7
        public float kMPersp2; //8

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

        void _orTypeMask(int mask) {
            D.assert((mask & kORableMasks) == mask);
            this.fTypeMask |= mask;
        }

        void _clearTypeMask(int mask) {
            // only allow a valid mask
            D.assert((mask & kAllMasks) == mask);
            this.fTypeMask &= ~mask;
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
                // bool invertible = true; // Fix warning: value is never used

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

        public void preScale(float sx, float sy, float px, float py) {
            if (1 == sx && 1 == sy) {
                return;
            }

            var m = new uiMatrix3();
            m.setScale(sx, sy, px, py);
            this.preConcat(m);
        }

        public void preScale(float sx, float sy) {
            if (1 == sx && 1 == sy) {
                return;
            }

            this.kMScaleX *= sx;
            this.kMSkewY *= sx;
            this.kMPersp0 *= sx;

            this.kMSkewX *= sy;
            this.kMScaleY *= sy;
            this.kMPersp1 *= sy;

            if (this.kMScaleX == 1 && this.kMScaleY == 1 && (this.fTypeMask &
                                                             (int) (TypeMask.kPerspective_Mask | TypeMask.kAffine_Mask)
                ) == 0) {
                this._clearTypeMask((int) TypeMask.kScale_Mask);
            }
            else {
                this._orTypeMask((int) TypeMask.kScale_Mask);
            }
        }

        public void preRotate(float radians, float px, float py) {
            var m = new uiMatrix3();
            m.setRotate(radians, px, py);
            this.preConcat(m);
        }

        public void preRotate(float radians) {
            var m = new uiMatrix3();
            m.setRotate(radians);
            this.preConcat(m);
        }

        public void preSkew(float kx, float ky) {
            var m = new uiMatrix3();
            m.setSkew(kx, ky);
            this.preConcat(m);
        }

        public void setScale(float sx, float sy, float px, float py) {
            if (1 == sx && 1 == sy) {
                this.reset();
            }
            else {
                this._setScaleTranslate(sx, sy, px - sx * px, py - sy * py);
            }
        }
    }
}