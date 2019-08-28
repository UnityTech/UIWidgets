using Unity.UIWidgets.material;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    static class ShadowUtils {
        public const bool kUseFastShadow = true;

        const float kAmbientHeightFactor = 1.0f / 128.0f;
        const float kAmbientGeomFactor = 64.0f;

        const float kBlurSigmaScale = 0.57735f;

        const float kMaxAmbientRadius = 300 * kAmbientHeightFactor * kAmbientGeomFactor;

        const bool debugShadow = false;

        static float divideAndPin(float numer, float denom, float min, float max) {
            return (numer / denom).clamp(min, max);
        }

        static float ambientBlurRadius(float height) {
            return Mathf.Min(height * kAmbientHeightFactor * kAmbientGeomFactor, kMaxAmbientRadius);
        }

        static float ambientRecipAlpha(float height) {
            return 1.0f + Mathf.Max(height * kAmbientHeightFactor, 0.0f);
        }

        static float spotBlurRadius(float occluderZ, float lightZ, float lightRadius) {
            return lightRadius * divideAndPin(occluderZ, lightZ - occluderZ, 0.0f, 0.95f);
        }

        static void getSpotParams(float occluderZ, float lightX, float lightY, float lightZ,
            float lightRadius,
            ref float blurRadius, ref float scale, ref Vector2 translate) {
            float zRatio = divideAndPin(occluderZ, lightZ - occluderZ, 0.0f, 0.95f);
            blurRadius = lightRadius * zRatio;
            scale = divideAndPin(lightZ, lightZ - occluderZ, 1.0f, 1.95f);
            translate = new Vector2(-zRatio * lightX, -zRatio * lightY);
        }

        static float convertRadiusToSigma(float radius) {
            return radius > 0 ? kBlurSigmaScale * radius + 0.5f : 0.0f;
        }

        public static void computeTonalColors(uiColor inAmbientColor, uiColor inSpotColor,
            ref uiColor? outAmbientColor, ref uiColor? outSpotColor) {
            outAmbientColor = uiColor.fromARGB(inAmbientColor.alpha, 0, 0, 0);
            outSpotColor = inSpotColor;
        }

        static readonly Matrix3 _toHomogeneous = Matrix3.I();
        
        static bool getSpotShadowTransform(Vector3 lightPos, float lightRadius, Matrix3 ctm,
            Vector3 zPlaneParams, Rect pathBounds, Matrix3 shadowTransform, ref float radius) {
            float heightFunc(float x, float y) {
                return zPlaneParams.x * x + zPlaneParams.y * y + zPlaneParams.z;
            }

            float occluderHeight = heightFunc(pathBounds.center.dx, pathBounds.center.dy);

            if (!ctm.hasPerspective()) {
                float scale = 0.0f;
                Vector2 translate = new Vector2();
                getSpotParams(occluderHeight, lightPos.x, lightPos.y, lightPos.z, lightRadius, ref radius, ref scale,
                    ref translate);
                shadowTransform.setScaleTranslate(scale, scale, translate.x, translate.y);
                shadowTransform.preConcat(ctm);
            }
            else {
                if (pathBounds.width.valueNearlyZero() || pathBounds.height.valueNearlyZero()) {
                    return false;
                }

                Offset[] pts = ctm.mapRectToQuad(pathBounds);
                if (!MathUtils.isConvexPolygon(pts, 4)) {
                    return false;
                }

                Vector3[] pts3D = new Vector3[4];
                float z = heightFunc(pathBounds.left, pathBounds.top);
                pts3D[0] = new Vector3(pts[0].dx, pts[0].dy, z);
                z = heightFunc(pathBounds.right, pathBounds.top);
                pts3D[1] = new Vector3(pts[1].dx, pts[1].dy, z);
                z = heightFunc(pathBounds.right, pathBounds.bottom);
                pts3D[2] = new Vector3(pts[2].dx, pts[2].dy, z);
                z = heightFunc(pathBounds.left, pathBounds.bottom);
                pts3D[3] = new Vector3(pts[3].dx, pts[3].dy, z);

                for (int i = 0; i < 4; i++) {
                    float dz = lightPos.z - pts3D[i].z;
                    if (dz.valueNearlyZero()) {
                        return false;
                    }

                    float zRatio = pts3D[i].z / dz;
                    pts3D[i].x -= (lightPos.x - pts3D[i].x) * zRatio;
                    pts3D[i].y -= (lightPos.y - pts3D[i].y) * zRatio;
                    pts3D[i].z = 1f;
                }

                Vector3 h0 = Vector3.Cross(Vector3.Cross(pts3D[1], pts3D[0]), Vector3.Cross(pts3D[2], pts3D[3]));
                Vector3 h1 = Vector3.Cross(Vector3.Cross(pts3D[0], pts3D[3]), Vector3.Cross(pts3D[1], pts3D[2]));
                Vector3 h2 = Vector3.Cross(Vector3.Cross(pts3D[0], pts3D[2]), Vector3.Cross(pts3D[1], pts3D[3]));

                if (h2.z.valueNearlyZero()) {
                    return false;
                }

                Vector3 v = pts3D[3] - pts3D[0];
                Vector3 w = h0 - pts3D[0];
                float perpDot = v.x * w.y - v.y * w.x;
                if (perpDot > 0) {
                    h0 = -h0;
                }

                v = pts3D[1] - pts3D[0];
                perpDot = v.x * w.y - v.y * w.x;
                if (perpDot < 0) {
                    h1 = -h1;
                }

                shadowTransform.setAll(h0.x / h2.z, h1.x / h2.z, h2.x / h2.z,
                    h0.y / h2.z, h1.y / h2.z, h2.y / h2.z,
                    h0.z / h2.z, h1.z / h2.z, 1);

                float xScale = 2.0f / (pathBounds.right - pathBounds.left);
                float yScale = 2.0f / (pathBounds.bottom - pathBounds.top);

                _toHomogeneous.setAll(xScale, 0, -xScale * pathBounds.left - 1,
                    0, yScale, -yScale * pathBounds.top - 1,
                    0, 0, 1);

                shadowTransform.preConcat(_toHomogeneous);

                radius = spotBlurRadius(occluderHeight, lightPos.z, lightRadius);
            }

            return true;
        }

        static readonly Path _devSpacePath = new Path();
        public static void drawShadow(Canvas canvas, Path path, Vector3 zPlaneParams, Vector3 devLightPos,
            float lightRadius, uiColor ambientColor, uiColor spotColor, int flags) {
            #pragma warning disable CS0162
            if (kUseFastShadow) {
                drawShadowFast(canvas, path, zPlaneParams, devLightPos, lightRadius, ambientColor, spotColor, flags);
            }
            else {
                drawShadowFull(canvas, path, zPlaneParams, devLightPos, lightRadius, ambientColor, spotColor, flags);
            }
            #pragma warning restore CS0162
        }
        
        //cached variables
        static readonly Paint _shadowPaint = new Paint();
        static readonly Matrix3 _shadowMatrix = Matrix3.I();

        static void drawShadowFull(Canvas canvas, Path path, Vector3 zPlaneParams, Vector3 devLightPos,
            float lightRadius, uiColor ambientColor, uiColor spotColor, int flags) {
            Matrix3 viewMatrix = canvas.getTotalMatrix();

            //ambient light
            _devSpacePath.resetAll();
            _devSpacePath.addPath(path, viewMatrix);
            float devSpaceOutset = ambientBlurRadius(zPlaneParams.z);
            float oneOverA = ambientRecipAlpha(zPlaneParams.z);
            float blurRadius = 0.5f * devSpaceOutset * oneOverA;
            float strokeWidth = 0.5f * (devSpaceOutset - blurRadius);

            //Paint paint = new Paint {color = ambientColor, strokeWidth = strokeWidth, style = PaintingStyle.fill};
            _shadowPaint.color = new Color(ambientColor.value);
            _shadowPaint.strokeWidth = strokeWidth;
            _shadowPaint.style = PaintingStyle.fill;
            
            canvas.save();
            _shadowMatrix.reset();
            canvas.setMatrix(_shadowMatrix);
            float sigma = convertRadiusToSigma(blurRadius);
            _shadowPaint.maskFilter = MaskFilter.blur(BlurStyle.normal, sigma);
            canvas.drawPath(_devSpacePath, _shadowPaint);
            canvas.restore();

            //spot light
            //Matrix3 shadowMatrix = Matrix3.I();
            float radius = 0.0f;

            if (!getSpotShadowTransform(devLightPos, lightRadius, viewMatrix, zPlaneParams, path.getBounds(),
                _shadowMatrix, ref radius)) {
                return;
            }

            canvas.save();
            canvas.setMatrix(_shadowMatrix);

            _shadowPaint.color = new Color(spotColor.value);
            _shadowPaint.strokeWidth = 0;
            _shadowPaint.style = PaintingStyle.fill;
            float sigma2 = convertRadiusToSigma(radius);
            _shadowPaint.maskFilter = MaskFilter.blur(BlurStyle.normal, sigma2);
            canvas.drawPath(path, _shadowPaint);

            canvas.restore();

            _shadowPaint.maskFilter = null;
        }
        
        
        static void drawShadowFast(Canvas canvas, Path path, Vector3 zPlaneParams, Vector3 devLightPos,
            float lightRadius, uiColor ambientColor, uiColor spotColor, int flags) {
            Matrix3 viewMatrix = canvas.getTotalMatrix();

            //debug shadow
            #pragma warning disable CS0162
            if (debugShadow) {
                var isRRect = path.isNaiveRRect;
                if (isRRect) {
                    ambientColor = uiColor.fromColor(Colors.red);
                    spotColor = uiColor.fromColor(Colors.red);
                }
                else {
                    ambientColor = uiColor.fromColor(Colors.green);
                    spotColor = uiColor.fromColor(Colors.green);
                }
            }
            #pragma warning restore CS0162

            //ambient light
            float devSpaceOutset = ambientBlurRadius(zPlaneParams.z);
            float oneOverA = ambientRecipAlpha(zPlaneParams.z);
            float blurRadius = 0.5f * devSpaceOutset * oneOverA;
            float strokeWidth = 0.5f * (devSpaceOutset - blurRadius);

            _shadowPaint.color = new Color(ambientColor.value);
            _shadowPaint.strokeWidth = strokeWidth;
            _shadowPaint.style = PaintingStyle.fill;
            canvas.drawPath(path, _shadowPaint);

            //spot light
            float radius = 0.0f;
            if (!getSpotShadowTransform(devLightPos, lightRadius, viewMatrix, zPlaneParams, path.getBounds(),
                _shadowMatrix, ref radius)) {
                return;
            }

            canvas.save();
            canvas.setMatrix(_shadowMatrix);

            _shadowPaint.color = new Color(spotColor.value);
            _shadowPaint.strokeWidth = 0;
            _shadowPaint.style = PaintingStyle.fill;
            float sigma2 = convertRadiusToSigma(radius);
            _shadowPaint.maskFilter = path.isNaiveRRect ? MaskFilter.fastShadow(sigma2) : MaskFilter.blur(BlurStyle.normal, sigma2);
            canvas.drawPath(path, _shadowPaint);

            canvas.restore();

            _shadowPaint.maskFilter = null;
        }
        
        /*
         * Check whether the RRect is a naive Round-Rect, of which
         * (1) all the corner radius are the same
         * (2) the corner radius is not bigger than either half the width or the height of the Round Rect's bounding box
         *
         * Usage: The shadow of a naive Round-Rect can be easily drawn using a ShadowRBox shader, so we can use it to
         * find all the situations that a fast shadow can be drawn to tackle the performance issue
         */
        public static bool isNaiveRRect(this RRect rrect) {
                var radius = rrect.tlRadiusX;
                return rrect.tlRadiusY == radius &&
                       rrect.trRadiusX == radius &&
                       rrect.trRadiusY == radius &&
                       rrect.blRadiusX == radius &&
                       rrect.blRadiusY == radius &&
                       rrect.brRadiusX == radius &&
                       rrect.brRadiusY == radius &&
                       radius <= rrect.width / 2 &&
                       radius <= rrect.height / 2;
        }
    }
}