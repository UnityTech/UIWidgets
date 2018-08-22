Shader "UIWidgets/ShadowRect"
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
    uniform float UIWidgets_sigma;
    
    #include "UIWidgets_CG.cginc"

    // http://madebyevan.com/shaders/fast-rounded-rectangle-shadows/
    float4 erf (float4 x) {
        float4 s = sign(x);
        float4 a = abs(x);
        
        x = 1.0 + (0.278393 + (0.230389 + 0.078108 * (a * a)) * a) * a;
        x *= x;
        return s - s / (x * x);
    }
    
    float UIWidgets_boxShadow (float2 lower, float2 upper, float2 pos, float sigma) {
        float4 query = float4((lower - pos).xy, (upper - pos).xy);
        float4 integral = erf(query * (sqrt(0.5) / sigma)) * 0.5 + 0.5;
        return (integral.z - integral.x) * (integral.w - integral.y);
    }

    v2f vert (appdata_t v) {
        float3 eyePos = UnityObjectToViewPos(v.vertex);
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.color = v.color;
        o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
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
        float shadowAlpha = UIWidgets_boxShadow(
            float2(_Rect[0] + 3 * UIWidgets_sigma, _Rect[1] + 3 * UIWidgets_sigma),
            float2(_Rect[0] + _Rect[2] - 3 * UIWidgets_sigma, _Rect[1] + _Rect[3] - 3 * UIWidgets_sigma),
            p, UIWidgets_sigma);
        col.a *= shadowAlpha;

        float pixelScale = 1.0f / abs(ddx(i.clipUV.x));
        float clipAlpha = getClipAlpha(i.clipUV, pixelScale);
        col.a *= clipAlpha;
                
        // If the source blend is not SrcAlpha (default) we need to multiply the color by the rounded corner
        // alpha factors for clipping, since it will not be done at the blending stage.
        if (_SrcBlend != 5) { // 5 SrcAlpha
            col.rgb *= shadowAlpha * clipAlpha;
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
