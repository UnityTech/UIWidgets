Shader "UIWidgets/2D Handles Lines" {
    Properties
    {
        _MainTex ("Texture", Any) = "white" {}
        _HandleZTest ("_HandleZTest", Int) = 8 // Always
    }
    SubShader {
        Tags { "ForceSupported" = "True" }
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest [_HandleZTest]
        BindChannels {
            Bind "vertex", vertex
            Bind "color", color
            Bind "TexCoord", texcoord
        }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 clipUV : TEXCOORD1;
            };

            sampler2D _MainTex;
            uniform float4 _MainTex_ST;

            #include "UIWidgets_CG.cginc"

            v2f vert (float4 vertex : POSITION, float2 uv : TEXCOORD0, float4 color : COLOR0)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                float3 screenUV = UnityObjectToViewPos(vertex);
                o.clipUV = mul(UIWidgets_GUIClipMatrix, float4(screenUV.xy, 0, 1.0));
                o.color = color;
                o.uv = TRANSFORM_TEX(uv, _MainTex);
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                float pixelScale = 1.0f / abs(ddx(i.clipUV.x));
                col.a *= getClipAlpha(i.clipUV, pixelScale);
                
                return col;
            }
            ENDCG
        }
    }
}
