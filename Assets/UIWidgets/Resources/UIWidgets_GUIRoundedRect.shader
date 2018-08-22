Shader "UIWidgets/GUIRoundedRect"
{
    Properties {
        _MainTex("Texture", any) = "white" {}
        _SrcBlend("SrcBlend", Int) = 5 // SrcAlpha
        _DstBlend("DstBlend", Int) = 10 // OneMinusSrcAlpha
    }

    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 2.5

    #include "UnityCG.cginc"

    struct appdata_t {
        float4 vertex : POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        float2 clipUV : TEXCOORD1;
        float4 pos : TEXCOORD2;
    };

    sampler2D _MainTex;
    uniform float4 _MainTex_ST;
    uniform bool _ManualTex2SRGB;
    uniform int _SrcBlend;

    uniform float _Rect[4];
    uniform float UIWidgets_BorderWidth[4];
    uniform float UIWidgets_CornerRadius[4];
    
    #include "UIWidgets_CG.cginc"

    half getCornerAlpha (float2 p, float2 center, float borderWidth1, float borderWidth2, float radius, float pixelScale) {
        float2 v = p - center;
        float outerDist = (length(v) - radius) * pixelScale;
        half outerDistAlpha = saturate(0.5f - outerDist);

        bool hasBorder = borderWidth1 > 0.0f || borderWidth2 > 0.0f;
        float a = radius - borderWidth1;
        float b = radius - borderWidth2;

        v.y *= a / b;
        half rawDist = (length(v) - a) * pixelScale;
        half alpha = saturate(0.5f + rawDist);
        half innerDistAlpha = (hasBorder && a > 0 && b > 0) ? alpha : 1.0f;

        return (outerDistAlpha == 1.0f) ? innerDistAlpha : outerDistAlpha;
    }

    bool isPointInside (float2 p, float4 rect) {
        return p.x >= rect.x && p.x <= (rect.x+rect.z) && p.y >= rect.y && p.y <= (rect.y+rect.w);
    }

    v2f vert (appdata_t v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.color = v.color;
        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
        float3 eyePos = UnityObjectToViewPos(v.vertex);
        o.clipUV = mul(UIWidgets_GUIClipMatrix, float4(eyePos.xy, 0, 1.0));
        o.pos = v.vertex;
        return o;
    }

    fixed4 frag (v2f i) : SV_Target {
        half4 col = tex2D(_MainTex, i.texcoord);
        if (_ManualTex2SRGB) {
            col.rgb = LinearToGammaSpace(col.rgb);
        }
        col *= i.color;
        
        float2 p = i.pos.xy;

        bool xIsLeft = (p.x - _Rect[0] - _Rect[2] / 2.0f) <= 0.0f;
        bool yIsTop = (p.y - _Rect[1] - _Rect[3] / 2.0f) <= 0.0f;

        float bw1 = UIWidgets_BorderWidth[0];
        float bw2 = UIWidgets_BorderWidth[1];

        int radiusIndex = 0;
        if (xIsLeft) {
            radiusIndex = yIsTop ? 0 : 3;
        } else {
            radiusIndex = yIsTop ? 1 : 2;
        }

        float activeRadius = UIWidgets_CornerRadius[radiusIndex];
        float2 center = float2(_Rect[0] + activeRadius, _Rect[1] + activeRadius);

        if (!xIsLeft) {
            center.x = (_Rect[0] + _Rect[2] - activeRadius);
            bw1 = UIWidgets_BorderWidth[2];
        }
        if (!yIsTop) {
            center.y = (_Rect[1] + _Rect[3] - activeRadius);
            bw2 = UIWidgets_BorderWidth[3];
        }

        bool isInCorner = (xIsLeft ? p.x <= center.x : p.x >= center.x) && (yIsTop ? p.y <= center.y : p.y >= center.y);
        float pixelScale = 1.0f / abs(ddx(i.pos.x));
        float cornerAlpha = isInCorner ? getCornerAlpha(p, center, bw1, bw2, activeRadius, pixelScale) : 1.0f;
        col.a *= cornerAlpha;

        float4 centerRect = float4(
            _Rect[0] + UIWidgets_BorderWidth[0],
            _Rect[1] + UIWidgets_BorderWidth[1],
            _Rect[2] - (UIWidgets_BorderWidth[0] + UIWidgets_BorderWidth[2]),
            _Rect[3] - (UIWidgets_BorderWidth[1] + UIWidgets_BorderWidth[3]));
        bool isPointInCenter = isPointInside(p, centerRect);

        half middleMask = isPointInCenter ? 0.0f : 1.0f;
        bool hasBorder = UIWidgets_BorderWidth[0] > 0 || UIWidgets_BorderWidth[1] > 0 || UIWidgets_BorderWidth[2] > 0 || UIWidgets_BorderWidth[3] > 0;
        float borderAlpha = hasBorder ? (isInCorner ? 1.0f : middleMask) : 1.0f;
        col.a *= borderAlpha;

        pixelScale = 1.0f / abs(ddx(i.clipUV.x));
        float clipAlpha = getClipAlpha(i.clipUV, pixelScale);
        col.a *= clipAlpha;

        // If the source blend is not SrcAlpha (default) we need to multiply the color by the rounded corner
        // alpha factors for clipping, since it will not be done at the blending stage.
        if (_SrcBlend != 5) { // 5 SrcAlpha
            col.rgb *= cornerAlpha * borderAlpha * clipAlpha;
        }
        return col;
    }
    ENDCG

    SubShader {
        Blend [_SrcBlend] [_DstBlend], One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

    SubShader {
        Blend [_SrcBlend] [_DstBlend]
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

    FallBack "UIWidgets/GUITextureClip"
}
