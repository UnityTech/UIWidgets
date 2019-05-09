using UnityEngine;

namespace Unity.UIWidgets.ui {
    static class ShadowUtils {
        public static bool kUseFastShadow = false;

        public const float kAmbientHeightFactor = 1.0f / 128.0f;
        public const float kAmbientGeomFactor = 64.0f;

        public const float kBlurSigmaScale = 0.57735f;

        public const float kMaxAmbientRadius = 300 * kAmbientHeightFactor * kAmbientGeomFactor;

        public static float divideAndPin(float numer, float denom, float min, float max) {
            return (numer / denom).clamp(min, max);
        }

        public static float ambientBlurRadius(float height) {
            return Mathf.Min(height * kAmbientHeightFactor * kAmbientGeomFactor, kMaxAmbientRadius);
        }

        public static float ambientRecipAlpha(float height) {
            return 1.0f + Mathf.Max(height * kAmbientHeightFactor, 0.0f);
        }

        public static float spotBlurRadius(float occluderZ, float lightZ, float lightRadius) {
            return lightRadius * divideAndPin(occluderZ, lightZ - occluderZ, 0.0f, 0.95f);
        }

        public static void getSpotParams(float occluderZ, float lightX, float lightY, float lightZ,
            float lightRadius,
            ref float blurRadius, ref float scale, ref Vector2 translate) {
            float zRatio = divideAndPin(occluderZ, lightZ - occluderZ, 0.0f, 0.95f);
            blurRadius = lightRadius * zRatio;
            scale = divideAndPin(lightZ, lightZ - occluderZ, 1.0f, 1.95f);
            translate = new Vector2(-zRatio * lightX, -zRatio * lightY);
        }

        public static float convertRadiusToSigma(float radius) {
            return radius > 0 ? kBlurSigmaScale * radius + 0.5f : 0.0f;
        }

        public static void computeTonalColors(Color inAmbientColor, Color inSpotColor,
            ref Color outAmbientColor, ref Color outSpotColor) {
            outAmbientColor = Color.fromARGB(inAmbientColor.alpha, 0, 0, 0);
            outSpotColor = inSpotColor;
        }

        public static bool getSpotShadowTransform(Vector3 lightPos, float lightRadius, Matrix3 ctm,
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

                Matrix3 toHomogeneous = Matrix3.I();
                float xScale = 2.0f / (pathBounds.right - pathBounds.left);
                float yScale = 2.0f / (pathBounds.bottom - pathBounds.top);

                toHomogeneous.setAll(xScale, 0, -xScale * pathBounds.left - 1,
                    0, yScale, -yScale * pathBounds.top - 1,
                    0, 0, 1);

                shadowTransform.preConcat(toHomogeneous);

                radius = spotBlurRadius(occluderHeight, lightPos.z, lightRadius);
            }

            return true;
        }

        public static void drawShadow(Canvas canvas, Path path, Vector3 zPlaneParams, Vector3 devLightPos,
            float lightRadius, Color ambientColor, Color spotColor, int flags) {
            if (kUseFastShadow) {
                drawShadowFast(canvas, path, zPlaneParams, devLightPos, lightRadius, ambientColor, spotColor, flags);
            }
            else {
                drawShadowFull(canvas, path, zPlaneParams, devLightPos, lightRadius, ambientColor, spotColor, flags);
            }
        }

        static void drawShadowFull(Canvas canvas, Path path, Vector3 zPlaneParams, Vector3 devLightPos,
            float lightRadius, Color ambientColor, Color spotColor, int flags) {
            Matrix3 viewMatrix = canvas.getTotalMatrix();

            //ambient light
            Path devSpacePath = path.transform(viewMatrix);
            float devSpaceOutset = ambientBlurRadius(zPlaneParams.z);
            float oneOverA = ambientRecipAlpha(zPlaneParams.z);
            float blurRadius = 0.5f * devSpaceOutset * oneOverA;
            float strokeWidth = 0.5f * (devSpaceOutset - blurRadius);

            Paint paint = new Paint {color = ambientColor, strokeWidth = strokeWidth, style = PaintingStyle.fill};

            canvas.save();
            canvas.setMatrix(Matrix3.I());
            float sigma = convertRadiusToSigma(blurRadius);
            paint.maskFilter = MaskFilter.blur(BlurStyle.normal, sigma);
            canvas.drawPath(devSpacePath, paint);
            canvas.restore();

            //spot light
            Matrix3 shadowMatrix = Matrix3.I();
            float radius = 0.0f;

            if (!getSpotShadowTransform(devLightPos, lightRadius, viewMatrix, zPlaneParams, path.getBounds(),
                shadowMatrix, ref radius)) {
                return;
            }

            canvas.save();
            canvas.setMatrix(shadowMatrix);
            Paint paint2 = new Paint {color = spotColor};
            float sigma2 = convertRadiusToSigma(radius);
            paint2.maskFilter = MaskFilter.blur(BlurStyle.normal, sigma2);
            canvas.drawPath(path, paint2);

            canvas.restore();
        }

        static void drawShadowFast(Canvas canvas, Path path, Vector3 zPlaneParams, Vector3 devLightPos,
            float lightRadius, Color ambientColor, Color spotColor, int flags) {
            Matrix3 viewMatrix = canvas.getTotalMatrix();

            //ambient light
            float devSpaceOutset = ambientBlurRadius(zPlaneParams.z);
            float oneOverA = ambientRecipAlpha(zPlaneParams.z);
            float blurRadius = 0.5f * devSpaceOutset * oneOverA;
            float strokeWidth = 0.5f * (devSpaceOutset - blurRadius);

            Paint paint = new Paint {color = ambientColor, strokeWidth = strokeWidth, style = PaintingStyle.fill};
            canvas.drawPath(path, paint);

            //spot light
            Matrix3 shadowMatrix = Matrix3.I();
            float radius = 0.0f;

            if (!getSpotShadowTransform(devLightPos, lightRadius, viewMatrix, zPlaneParams, path.getBounds(),
                shadowMatrix, ref radius)) {
                return;
            }

            canvas.save();
            canvas.setMatrix(shadowMatrix);
            Paint paint2 = new Paint {color = spotColor};
            canvas.drawPath(path, paint2);

            canvas.restore();
        }
    }
}