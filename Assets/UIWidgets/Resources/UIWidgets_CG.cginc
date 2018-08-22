uniform float4x4 UIWidgets_GUIClipMatrix;
uniform float4 UIWidgets_GUIClipRect;
uniform float4 UIWidgets_GUIClipRectRadius;

half __getCornerAlpha (float2 p, float2 center, float radius, float pixelScale) {
    float2 v = p - center;
    float pixelCenterDist = length(v);

    float outerDist = (pixelCenterDist - radius) * pixelScale;
    half outerDistAlpha = saturate(0.5f - outerDist);

    return outerDistAlpha;
}

float getClipAlpha (float2 p, float pixelScale) {
    bool xIsLeft = (p.x - UIWidgets_GUIClipRect[0] - UIWidgets_GUIClipRect[2] / 2.0f) <= 0.0f;
    bool yIsTop = (p.y - UIWidgets_GUIClipRect[1] - UIWidgets_GUIClipRect[3] / 2.0f) <= 0.0f;

    int radiusIndex = 0;
    if (xIsLeft) {
        radiusIndex = yIsTop ? 0 : 3;
    } else {
        radiusIndex = yIsTop ? 1 : 2;
    }
    float activeRadius = UIWidgets_GUIClipRectRadius[radiusIndex];
    
    float2 center = float2(UIWidgets_GUIClipRect[0] + activeRadius, UIWidgets_GUIClipRect[1] + activeRadius);
    if (!xIsLeft) {
        center.x = (UIWidgets_GUIClipRect[0] + UIWidgets_GUIClipRect[2] - activeRadius);
    }
    if (!yIsTop) {
        center.y = (UIWidgets_GUIClipRect[1] + UIWidgets_GUIClipRect[3] - activeRadius);
    }

    float clipAlpha = 1.0f;        
    
    bool isInCorner = (xIsLeft ? p.x <= center.x : p.x >= center.x) && (yIsTop ? p.y <= center.y : p.y >= center.y);
    float cornerAlpha = isInCorner ? __getCornerAlpha(p, center, activeRadius, pixelScale) : 1.0f;
    clipAlpha *= cornerAlpha;

    bool isInRect =
        p.x >= UIWidgets_GUIClipRect[0]
        && p.x <= UIWidgets_GUIClipRect[0] + UIWidgets_GUIClipRect[2]
        && p.y >= UIWidgets_GUIClipRect[1]
        && p.y <= UIWidgets_GUIClipRect[1] + UIWidgets_GUIClipRect[3];
    float rectAlpha = isInRect ? 1.0f : 0.0f;
    clipAlpha *= rectAlpha;
    
    return clipAlpha;
}