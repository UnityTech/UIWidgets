Shader "UIWidgets/canvas_fill0"
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
            #include "UIWidgets_canvas.cginc"
            #pragma vertex vert
            #pragma fragment frag_stencil
            ENDCG
        }
    }
}
