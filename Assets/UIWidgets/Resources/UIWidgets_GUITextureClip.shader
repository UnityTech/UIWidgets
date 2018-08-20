Shader "UIWidgets/GUITextureClip"
{
    Properties {
        _MainTex ("Texture", Any) = "white" {}
    }

    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 2.0

    #include "UnityCG.cginc"

    struct appdata_t {
        float4 vertex : POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        float2 clipUV : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    sampler2D _MainTex;
    uniform float4 _MainTex_ST;
    uniform bool _ManualTex2SRGB;
    
    #include "UIWidgets_CG.cginc"
    
    v2f vert (appdata_t v) {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.vertex = UnityObjectToClipPos(v.vertex);
        float3 eyePos = UnityObjectToViewPos(v.vertex);
        o.clipUV = mul(UIWidgets_GUIClipMatrix, float4(eyePos.xy, 0, 1.0));
        o.color = v.color;
        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
        return o;
    }

    fixed4 frag (v2f i) : SV_Target {
        fixed4 colTex = tex2D(_MainTex, i.texcoord);
        if (_ManualTex2SRGB) {
            colTex.rgb = LinearToGammaSpace(colTex.rgb);
        }
        fixed4 col = colTex * i.color;
        
        float pixelScale = 1.0f / abs(ddx(i.clipUV.x));
        col.a *= getClipAlpha(i.clipUV, pixelScale);

        return col;
    }
    ENDCG

    SubShader {

        Tags { "ForceSupported" = "True" }

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha, One One
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

    SubShader {

        Tags { "ForceSupported" = "True" }

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }
}
