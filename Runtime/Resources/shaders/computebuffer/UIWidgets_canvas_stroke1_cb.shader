Shader "UIWidgets/canvas_stroke1_cb"
{
    Properties {
    }
   
    SubShader {
        ZTest Always
        ZWrite Off

        ColorMask 0
        Stencil {
            Ref 0
            Comp NotEqual
            ReadMask 127
            WriteMask 127
            Pass Zero
        }
        
        Pass {        
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
