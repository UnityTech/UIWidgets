half4 _color;

struct vdata
            {
                float2 vertex;
                float2 uv;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 ftcoord : TEXCOORD0;
                float2 fpos : TEXCOORD1;
            };

            StructuredBuffer<vdata> databuffer;
            StructuredBuffer<int> indexbuffer;
            float4 _viewport;
            int _startVertex;
            
            v2f vert (uint vertex_id: SV_VertexID, uint instance_id: SV_InstanceID)
            {
                v2f o = (v2f)0;
                vdata v = databuffer[indexbuffer[_startVertex + vertex_id]];
                o.vertex = float4(v.vertex.x * 2.0 / _viewport.z - 1.0, v.vertex.y * 2.0 / _viewport.w - 1.0, 0, 1);
                o.ftcoord = v.uv;
                o.fpos = v.vertex;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return _color;
            }