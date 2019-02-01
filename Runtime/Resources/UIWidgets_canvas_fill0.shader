Shader "UIWidgets/canvas_fill0"
{
    Properties {
    }
   
    SubShader {
        ZTest Always
        ZWrite Off
        
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
            
        Pass {
            CGPROGRAM
            #include "UIWidgets_canvas.cginc"
            #pragma vertex vert
            #pragma fragment frag_stencil
            ENDCG
        }
    }
}
