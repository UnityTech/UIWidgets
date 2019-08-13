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
        Blend [_SrcBlend] [_DstBlend]
        Pass { // 0, color
            CGPROGRAM
            #define UIWIDGETS_COLOR
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
        
        Pass { // 1, linear
            CGPROGRAM
            #define UIWIDGETS_LINEAR
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        Pass { // 2, radial
            CGPROGRAM
            #define UIWIDGETS_RADIAL
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        Pass { // 3, sweep
            CGPROGRAM
            #define UIWIDGETS_SWEEP
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
        
        Pass { // 4, image
            CGPROGRAM
            #define UIWIDGETS_IMAGE
            #include "UIWidgets_canvas_cb.cginc"
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}