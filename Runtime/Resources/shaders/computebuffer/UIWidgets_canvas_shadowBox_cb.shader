Shader "UIWidgets/ShadowBox_cb"
{
    //originally from http://madebyevan.com/shaders/fast-rounded-rectangle-shadows/
    Properties
    {
        _StencilComp("_StencilComp", Float) = 8 // - Equal, 8 - Always 
    }
    SubShader
    {
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Stencil {
            Ref 128
            Comp [_StencilComp]
        }
        
        Pass {
            CGPROGRAM
            #pragma require compute
            
            float4 _sb_box;
            float4 _viewport;
            float _sb_sigma;
            float4 _sb_color;
            float _mat[9];
            
            struct vdata
            {
                float2 vertex;
                float2 uv;
            };

            StructuredBuffer<vdata> databuffer;
            StructuredBuffer<int> indexbuffer;
            int _startVertex;

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 coord : TEXCOORD0;
            };
            
            float4 erf(float4 x) 
            {
                float4 s = sign(x);
                float4 a = abs(x);
                x = 1.0 + (0.278393 + (0.230389 + 0.078108 * (a * a)) * a) * a;
                x = x * x;
                return s - s / (x * x);
                return s;
            }
            
            float boxShadow(float2 lower, float2 upper, float2 pnt, float sigma)
            {
                float4 query = float4(pnt - lower, pnt - upper);
                float4 integral = 0.5 + 0.5 * erf(query * (sqrt(0.5) / sigma));
                return (integral.z - integral.x) * (integral.w - integral.y);
            }
            
            v2f vert(uint vertex_id: SV_VertexID){
                v2f o;
                vdata v = databuffer[indexbuffer[_startVertex + vertex_id]];
                float padding = 3.0 * _sb_sigma;
                o.coord = lerp(_sb_box.xy - padding, _sb_box.zw + padding, v.vertex.xy);
                float3x3 mat = float3x3(_mat[0], _mat[1], _mat[2], _mat[3], _mat[4], _mat[5], 0, 0, 1);
                float2 p = mul(mat, float3(o.coord.xy, 1.0)).xy - _viewport.xy;
                
            #if UNITY_UV_STARTS_AT_TOP
                o.vertex = float4(2.0 * p.x / _viewport.z - 1.0, 2.0 * p.y / _viewport.w - 1.0, 0, 1);
            #else
                o.vertex = float4(2.0 * p.x / _viewport.z - 1.0, 1.0 - 2.0 * p.y / _viewport.w, 0, 1);
            #endif
                return o;
            }
            
            float4 frag(v2f i) : SV_TARGET {
                float4 fragColor = _sb_color;
                fragColor.a = fragColor.a * boxShadow(_sb_box.xy, _sb_box.zw, i.coord, _sb_sigma);
                return fragColor;
            }
            
            #pragma vertex vert
            #pragma fragment frag
            
            ENDCG
        }
    }
}