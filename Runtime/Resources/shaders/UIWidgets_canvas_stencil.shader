Shader "UIWidgets/canvas_stencil"
{
    Properties {
    }
   
    SubShader {
        ZTest Always
        ZWrite Off
        
        Pass { // 0, stencil clear
            ColorMask 0
            Stencil {
                Ref 128
                Pass Replace
            }

            CGPROGRAM
            #include "UIWidgets_canvas.cginc"
            #pragma vertex vert
            #pragma fragment frag_stencil
            ENDCG
        }

        Pass { // 1, stencil intersect 0
            Cull Off
            ColorMask 0
            Stencil {
                WriteMask 127
                PassFront IncrWrap
                PassBack DecrWrap
            }

            CGPROGRAM
            #include "UIWidgets_canvas.cginc"
            #pragma vertex vert
            #pragma fragment frag_stencil
            ENDCG
        }
        
        Pass { // 2, stencil intersect 1
            ColorMask 0
            Stencil {
                Ref 128
                Comp Less
                Pass Replace
                Fail Zero
            }

            CGPROGRAM
            #include "UIWidgets_canvas.cginc"
            #pragma vertex vert
            #pragma fragment frag_stencil
            ENDCG
        }
    }
}
