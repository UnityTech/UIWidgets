Shader "UIWidgets/canvas_filter_cb"
{
    Properties {
    }
   
    SubShader {
        ZTest Always
        ZWrite Off
        
        Pass { // 0, mask filter 
            CGPROGRAM
            #pragma require compute
            #include "../UIWidgets_canvas.cginc"
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert_compute
            #pragma fragment frag_mf
            ENDCG
        }
    }
}
