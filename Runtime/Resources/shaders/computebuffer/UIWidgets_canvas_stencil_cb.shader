Shader "UIWidgets/canvas_stencil_cb"
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
            #pragma require compute
            #include "../UIWidgets_canvas.cginc"
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert_compute
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
            #pragma require compute
            #include "../UIWidgets_canvas.cginc"
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert_compute
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
            #pragma require compute
            #include "../UIWidgets_canvas.cginc"
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert_compute
            #pragma fragment frag_stencil
            ENDCG
        }
    }
}
