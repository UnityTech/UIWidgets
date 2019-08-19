struct vdata
{
    float2 vertex;
    float2 uv;
};

StructuredBuffer<vdata> databuffer;
StructuredBuffer<int> indexbuffer;
int _startVertex;

v2f vert_compute (uint vertex_id: SV_VertexID)
{
    v2f o = (v2f)0;
    vdata v = databuffer[indexbuffer[_startVertex + vertex_id]];
    o.ftcoord = v.uv;
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