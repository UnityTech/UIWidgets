Shader "UIWidgets/canvas_convexFill_cb"
{
    Properties
    {
       _SrcBlend("_SrcBlend", Int) = 1 // One
       _DstBlend("_DstBlend", Int) = 10 // OneMinusSrcAlpha
       _StencilComp("_StencilComp", Float) = 3 // - Equal, 8 - Always 
    }

    SubShader
    {   
        ZTest Always
        ZWrite Off
        Blend [_SrcBlend] [_DstBlend]
        
        Stencil {
            Ref 128
            Comp [_StencilComp]
        }
        
        Pass { // 0, color
            CGPROGRAM
            #pragma require compute
            #define UIWIDGETS_COLOR
            #include "../UIWidgets_canvas.cginc"
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert_compute
            #pragma fragment frag
            ENDCG
        }
        
        Pass { // 1, linear
            CGPROGRAM
            #pragma require compute
            #define UIWIDGETS_LINEAR
            #include "../UIWidgets_canvas.cginc"
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert_compute
            #pragma fragment frag
            ENDCG
        }

        Pass { // 2, radial
            CGPROGRAM
            #pragma require compute
            #define UIWIDGETS_RADIAL
            #include "../UIWidgets_canvas.cginc"
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert_compute
            #pragma fragment frag
            ENDCG
        }

        Pass { // 3, sweep
            CGPROGRAM
            #pragma require compute
            #define UIWIDGETS_SWEEP
            #include "../UIWidgets_canvas.cginc"
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert_compute
            #pragma fragment frag
            ENDCG
        }
        
        Pass { // 4, image
            CGPROGRAM
            #pragma require compute
            #define UIWIDGETS_IMAGE
            #include "../UIWidgets_canvas.cginc"
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert_compute
            #pragma fragment frag
            ENDCG
        }
    }
}