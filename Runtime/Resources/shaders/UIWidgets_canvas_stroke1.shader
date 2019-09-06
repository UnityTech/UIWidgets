Shader "UIWidgets/canvas_stroke1"
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
            #include "UIWidgets_canvas.cginc"
            #pragma vertex vert
            #pragma fragment frag_stencil
            ENDCG
        }
    }
}
