Shader "UIWidgets/ShadowRBox"
{
    //originally from http://madebyevan.com/shaders/fast-rounded-rectangle-shadows/
    Properties
    {
        _SrcBlend("_SrcBlend", Int) = 1 // One
        _DstBlend("_DstBlend", Int) = 10 // OneMinusSrcAlpha
    }
    SubShader
    {
        Blend [_SrcBlend] [_DstBlend]
        Pass {
            CGPROGRAM
            
            float4 box;
            float2 window;
            float sigma;
            float4 color;
            float corner;
            
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 coord : TEXCOORD0;
            };
            
            float gaussian(float x, float sigma)
            {
                float pi = 3.141592653589793;
                return exp(-(x*x) / (2.0 * sigma * sigma)) / (sqrt(2.0 * pi) * sigma);
            }
            
            float2 erf(float2 x) 
            {
                float2 s = sign(x);
                float2 a = abs(x);
                x = 1.0 + (0.278393 + (0.230389 + 0.078108 * (a * a)) * a) * a;
                x = x * x;
                return s - s / (x * x);
                return s;
            }
            
            float roundedBoxShadowX(float x, float y, float sigma, float corner, float2 halfSize)
            {
                float delta = min(halfSize.y - corner - abs(y), 0.0);
                float curved = halfSize.x - corner + sqrt(max(0.0, corner * corner - delta * delta));
                float2 integral = 0.5 + 0.5 * erf((x + float2(-curved, curved)) * (sqrt(0.5)/sigma));
                return integral.y - integral.x;
            }
            
            float roundedBoxShadow(float2 lower, float2 upper, float2 pnt, float sigma, float corner)
            {
                float2 center = (lower + upper) * 0.5;
                float2 halfSize = (upper - lower) * 0.5;
                pnt -= center;
                
                float low = pnt.y - halfSize.y;
                float high = pnt.y + halfSize.y;
                float start = clamp(-3.0 * sigma, low, high);
                float end = clamp(3.0 * sigma, low, high);
                
                float step = (end - start) / 4.0;
                float y = start + step * 0.5;
                float value = 0.0;
                
                for(int i=0; i<4;i++)
                {
                    value += roundedBoxShadowX(pnt.x, pnt.y - y, sigma, corner, halfSize) * gaussian(y, sigma) * step;
                    y += step;
                }
                
                return value;
            }
            
            v2f vert(appdata v){
                v2f o;
                float padding = 3.0 * sigma;
                o.coord = lerp(box.xy - padding, box.zw + padding, v.vertex.xy);
                o.vertex = float4(o.coord.x * 2.0 /window.x - 1.0, o.coord.y * 2.0/window.y - 1.0, 0, 1);
                return o;
            }
            
            float4 frag(v2f i) : SV_TARGET {
                float4 fragColor = color;
                fragColor.a = fragColor.a * roundedBoxShadow(box.xy, box.zw, i.coord, sigma, corner);
                return fragColor;
            }
            
            #pragma vertex vert
            #pragma fragment frag
            
            ENDCG
        }
    }
}