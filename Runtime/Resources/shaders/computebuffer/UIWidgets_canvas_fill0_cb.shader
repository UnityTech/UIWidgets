Shader "UIWidgets/canvas_fill0_cb"
{
    Properties {
        _StencilComp("_StencilComp", Float) = 3 // - Equal, 8 - Always 
    }
   
    SubShader {
        ZTest Always
        ZWrite Off
        
        Cull Off        
        ColorMask 0
        Stencil {
            Ref 128
            CompFront [_StencilComp]
            CompBack [_StencilComp]
            ReadMask 128
            WriteMask 127
            PassFront IncrWrap
            PassBack DecrWrap
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
