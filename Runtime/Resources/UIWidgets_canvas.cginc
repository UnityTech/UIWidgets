float4 _viewport;
float _mat[9];

half4 _color;
fixed _alpha;
fixed _strokeMult;
half4x4 _shaderMat;
sampler2D _shaderTex;
half4 _leftColor;
half4 _rightColor;
half _bias;
half _scale;
int _tileMode; // 0 = clamp, 1 = mirror, 2 = repeated

sampler2D _tex;
int _texMode; // 0 = post alpha, 1 = pre alpha, 2 = alpha only

half2 _mf_imgInc; // _mf stands for mask filter
int _mf_radius;
half _mf_kernel[25];

struct appdata_t {
    float4 vertex : POSITION;
    float2 tcoord : TEXCOORD0;
};

struct v2f {
    float4 vertex : SV_POSITION;
    float2 ftcoord : TEXCOORD0;
    float2 fpos : TEXCOORD1;
};


half shader_gradient_layout(half2 pos) {
    half4 p4 = half4(pos, 0.0, 1.0);
#if defined(UIWIDGETS_LINEAR)
    return mul(_shaderMat, p4).x;
#elif defined(UIWIDGETS_RADIAL)
    return length(mul(_shaderMat, p4).xy);
#elif defined(UIWIDGETS_SWEEP)
    half2 p2 = mul(_shaderMat, p4).xy;
    half angle = atan2(-p2.y, -p2.x);
    // 0.1591549430918 is 1/(2*pi), used since atan returns values [-pi, pi]
    return (angle * 0.1591549430918 + 0.5 + _bias) * _scale;
#else
    return 0;
#endif
}

half4 shader_gradient_colorize(half pt) {
    if (_tileMode == 0) { // clamp
        if (pt <= 0.0) {
            return _leftColor;
        } else if (pt >= 1.0) {
            return _rightColor;
        }
        
        half2 coord = half2(pt, 0.5);
        return tex2D(_shaderTex, coord);
    } else if (_tileMode == 1) { // mirror
        pt = pt - 1;
        pt = pt - 2 * floor(pt * 0.5) - 1;
        pt = abs(pt);
        
        half2 coord = half2(pt, 0.5);
        return tex2D(_shaderTex, coord);
    } else if (_tileMode == 2) { // repeated
        pt = frac(pt);
        
        half2 coord = half2(pt, 0.5);
        return tex2D(_shaderTex, coord);
    }
    
    return half4(0, 0, 0, 0);
}


half2 shader_image_layout(half2 pos) {
    half4 p4 = half4(pos, 0.0, 1.0);
    return mul(_shaderMat, p4).xy;
}

half4 shader_image_colorize(half2 pt) {
    if (_tileMode == 0) { // clamp
        pt.x = clamp(pt.x, 0.0, 1.0);
        pt.y = clamp(pt.y, 0.0, 1.0);
    } else if (_tileMode == 1) { // mirror
        pt.x = pt.x - 1;
        pt.x = pt.x - 2 * floor(pt.x * 0.5) - 1;
        pt.x = abs(pt.x);
        pt.y = pt.y - 1;
        pt.y = pt.y - 2 * floor(pt.y * 0.5) - 1;
        pt.y = abs(pt.y);
    } else if (_tileMode == 2) { // repeated
        pt.x = frac(pt.x);
        pt.y = frac(pt.y);
    }
    
    return tex2D(_shaderTex, pt);
}

half4 prealpha(half4 color) {
    return half4(color.x * color.w, color.y * color.w, color.z * color.w, color.w);
}

half4 shader_color (v2f i) {
    half4 c;
    
#if defined(UIWIDGETS_COLOR)
    c = _color;
#elif defined(UIWIDGETS_IMAGE)
    half2 pt = shader_image_layout(i.fpos);
    c = shader_image_colorize(pt);
#else
    half pt = shader_gradient_layout(i.fpos);
    c = shader_gradient_colorize(pt);
#endif

    c.w *= _alpha;
    return c;
}


v2f vert (appdata_t v) {
    v2f o;
    o.ftcoord = v.tcoord;
    o.fpos = v.vertex;
    
    float3x3 mat = float3x3(_mat[0], _mat[1], _mat[2], _mat[3], _mat[4], _mat[5], 0, 0, 1);
    
    float2 p = mul(mat, float3(v.vertex.xy, 1.0)).xy - _viewport.xy;
    
#if UNITY_UV_STARTS_AT_TOP
    o.vertex = float4(2.0 * p.x / _viewport.z - 1.0, 2.0 * p.y / _viewport.w - 1.0, 0, 1);
#else
    o.vertex = float4(2.0 * p.x / _viewport.z - 1.0, 1.0 - 2.0 * p.y / _viewport.w, 0, 1);
#endif

    return o;
}

fixed4 frag (v2f i) : SV_Target {
    return prealpha(shader_color(i));
}

fixed4 frag_stencil (v2f i) : SV_Target {
    return half4(0, 0, 0, 0);
}

fixed4 frag_tex (v2f i) : SV_Target {
    half4 tintColor = shader_color(i);
    
    half4 color = tex2D(_tex, i.ftcoord);
    if (_texMode == 0) { // post alpha
        color = color * tintColor;
        color = prealpha(color);
    } else if (_texMode == 1) { // pre alpha
        color = color * tintColor;
        color = half4(color.x * tintColor.w, color.y * tintColor.w, color.z * tintColor.w, color.w);
    } else if (_texMode == 2) { // alpha only
        color = half4(1, 1, 1, color.w) * tintColor;
        color = prealpha(color);
    }

    return color;
}    

fixed4 frag_mf (v2f i) : SV_Target {
     half4 color = half4(0, 0, 0, 0);
     
     float2 coord = i.ftcoord - _mf_radius * _mf_imgInc;
     int width = _mf_radius * 2.0 + 1; // use 2.0 to avoid "bitfieldInsert"
     
     [unroll(25)]
     for (int i = 0; i < width; i++) {
        color += tex2D(_tex, coord) * _mf_kernel[i];
        coord += _mf_imgInc;
     }

     return color;
}

float strokeMask(float u, float v) {
    return min(1.0, (1.0 - abs(u * 2.0 - 1.0)) * 1.0) * min(1.0, v);
}

fixed4 frag_stroke_alpha(v2f i) : SV_Target {
    return prealpha(shader_color(i)) * strokeMask(i.ftcoord.x, i.ftcoord.y);
}
