Shader "UIWidgets/canvas_filter"
{
    Properties {
    }
   
    SubShader {
        ZTest Always
        ZWrite Off
        
        Pass { // 0, mask filter 
            CGPROGRAM
            #include "UIWidgets_canvas.cginc"
            #pragma vertex vert
            #pragma fragment frag_mf
            ENDCG
        }
    }
}
