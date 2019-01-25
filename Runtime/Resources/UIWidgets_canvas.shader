Shader "UIWidgets/canvas"
{
    Properties {
       _SrcBlend("_SrcBlend", Int) = 1 // One
       _DstBlend("_DstBlend", Int) = 10 // OneMinusSrcAlpha
    }

    CGINCLUDE
    float4 _viewport;
    float4x4 _paintMat;
    half4 _innerCol;
    half4 _outerCol;
    float2 _extent;
    float _radius;
    float _feather;
    sampler2D _tex;
    
    half2 _mf_imgInc; // _mf stands for mask filter
    int _mf_radius;
    half _mf_kernel[25];
        
    struct appdata_t {
        float4 vertex : POSITION;
        float2 tcoord : TEXCOORD0;
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        float2 ftcoord : TEXCOORD0;
        float2 fpos : TEXCOORD1;
    };

    float sdroundrect (float2 pt, float2 ext, float rad) {
        float2 ext2 = ext - float2(rad, rad);
		float2 d = abs(pt) - ext2;
		return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rad;
    }
		
    #pragma vertex vert
    v2f vert (appdata_t v) {
        v2f o;
        o.ftcoord = v.tcoord;
        o.fpos = v.vertex;
        
        float x = v.vertex.x - _viewport.x;
        float y = v.vertex.y - _viewport.y;
        
    #if UNITY_UV_STARTS_AT_TOP
        o.vertex = float4(2.0 * x / _viewport.z - 1.0, 2.0 * y / _viewport.w - 1.0, 0, 1);
    #else
        o.vertex = float4(2.0 * x / _viewport.z - 1.0, 1.0 - 2.0 * y / _viewport.w, 0, 1);
    #endif

        return o;
    }
    
    fixed4 frag (v2f i) : SV_Target {
        float2 pt = (mul(_paintMat, float3(i.fpos, 1.0))).xy;
        float d = clamp((sdroundrect(pt, _extent, _radius) + _feather * 0.5) / _feather, 0.0, 1.0);
        half4 color = lerp(_innerCol, _outerCol, d);
        return color;
    }
    
    fixed4 frag_stencil (v2f i) : SV_Target {
        return half4(0, 0, 0, 0);
    }
    
    fixed4 frag_tex (v2f i) : SV_Target {
        half4 color = tex2D(_tex, i.ftcoord);
        color = color * _innerCol; // tint color
        color = half4(color.xyz * color.w, color.w);
        return color;
    }    
    
    fixed4 frag_texrt (v2f i) : SV_Target {
         half4 color = tex2D(_tex, i.ftcoord);
         color = color * _innerCol; // tint color
         color = half4(color.xyz * _innerCol.w, color.w);
         return color;
    }
    
    fixed4 frag_texfont (v2f i) : SV_Target {
         half4 color = tex2D(_tex, i.ftcoord);
         color = half4(1, 1, 1, color.w) * _innerCol; // tint color
         color = half4(color.xyz * color.w, color.w);
         return color;
    }
    
    fixed4 frag_mf (v2f i) : SV_Target {
         half4 color = half4(0, 0, 0, 0);
         
         float2 coord = i.ftcoord - _mf_radius * _mf_imgInc;
         int width = _mf_radius * 2 + 1;
         for (int i = 0; i < width; i++) {
            color += tex2D(_tex, coord) * _mf_kernel[i];
            coord += _mf_imgInc;
         }
    
         color = color * _innerCol; // tint color
         color = half4(color.xyz * _innerCol.w, color.w);
         return color;
    }
    
    ENDCG

    SubShader {
        ZTest Always
        ZWrite Off
        Blend [_SrcBlend] [_DstBlend]
        
        Pass { // 0, fill pass 0
            Cull Off        
            ColorMask 0
            Stencil {
                Ref 128
                CompFront Equal
                CompBack Equal
                ReadMask 128
                WriteMask 127
                PassFront IncrWrap
                PassBack DecrWrap
            }
            
            CGPROGRAM
            #pragma fragment frag_stencil
            ENDCG
        }

        Pass { // 1, fill pass 1        
            Stencil {
                Ref 0
                Comp NotEqual
                ReadMask 127
                WriteMask 127
                Pass Zero
            }
            
            CGPROGRAM
            #pragma fragment frag
            ENDCG
        }
        
        Pass { // 2, convex fill
            Stencil {
                Ref 128
                Comp Equal
            }
            CGPROGRAM
            #pragma fragment frag
            ENDCG
        }
        
        Pass { // 3, stroke pass 0  
            Stencil {
                Ref 128
                Comp Equal
                Pass IncrSat
            }
            
            CGPROGRAM
            #pragma fragment frag
            ENDCG
        }

        Pass { // 4, stroke pass 1        
            ColorMask 0
            Stencil {
                Ref 0
                Comp NotEqual
                ReadMask 127
                WriteMask 127
                Pass Zero
            }
            
            CGPROGRAM
            #pragma fragment frag_stencil
            ENDCG
        }
        
        Pass { // 5, texture pass 0
            Stencil {
                Ref 128
                Comp Equal
            }

            CGPROGRAM
            #pragma fragment frag_tex
            ENDCG
        }
        
        Pass { // 6, render texture pass 0
            Stencil {
                Ref 128
                Comp Equal
            }

            CGPROGRAM
            #pragma fragment frag_texrt
            ENDCG
        }
        
        Pass { // 7, stencil clear
            ColorMask 0
            Stencil {
                Ref 128
                Pass Replace
            }

            CGPROGRAM
            #pragma fragment frag_stencil
            ENDCG
        }

        Pass { // 8, stencil intersect 0
            Cull Off
            ColorMask 0
            Stencil {
                WriteMask 127
                PassFront IncrWrap
                PassBack DecrWrap
            }

            CGPROGRAM
            #pragma fragment frag_stencil
            ENDCG
        }
        
        Pass { // 9, stencil intersect 1
            ColorMask 0
            Stencil {
                Ref 128
                Comp Less
                Pass Replace
                Fail Zero
            }

            CGPROGRAM
            #pragma fragment frag_stencil
            ENDCG
        }
        
        Pass { // 10, font texture pass 0
            Stencil {
                Ref 128
                Comp Equal
            }

            CGPROGRAM
            #pragma fragment frag_texfont
            ENDCG
        }
        
        Pass { // 11, mask filter 
            CGPROGRAM
            #pragma fragment frag_mf
            ENDCG
        }
    }
}
