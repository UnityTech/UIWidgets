using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial class PictureFlusher {
        internal abstract class RenderCmd : PoolObject {
            public abstract void release();
        }

        internal class CmdLayer : RenderCmd {
            public RenderLayer layer;

            public CmdLayer() {
            }

            public override void clear() {
                this.layer = null;
            }

            public static CmdLayer create(RenderLayer layer) {
                CmdLayer newCmd = ObjectPool<CmdLayer>.alloc();
                newCmd.layer = layer;
                return newCmd;
            }

            public override void release() {
                ObjectPool<CmdLayer>.release(this);
            }
        }

        internal class CmdDraw : RenderCmd {
            public uiMeshMesh mesh;
            public TextBlobMesh textMesh;
            public int pass;
            public MaterialPropertyBlockWrapper properties;
            public int? layerId;
            public Material material;
            public Image image; // just to keep a reference to avoid GC.
            public Mesh meshObj;
            public bool meshObjCreated;

            public static readonly Matrix4x4 idMat = Matrix4x4.identity;
            public static readonly Matrix3 idMat3 = Matrix3.I();
            public static readonly int texId = Shader.PropertyToID("_tex");
            public static readonly int matId = Shader.PropertyToID("_mat");
            public static readonly int vertexBufferId = Shader.PropertyToID("databuffer");
            public static readonly int indexBufferId = Shader.PropertyToID("indexbuffer");
            public static readonly int startIndexId = Shader.PropertyToID("_startVertex");


            public override void clear() {
                ObjectPool<uiMeshMesh>.release(this.mesh);
                ObjectPool<TextBlobMesh>.release(this.textMesh);
                ObjectPool<MaterialPropertyBlockWrapper>.release(this.properties);
            }

            public CmdDraw() {
            }

            public static CmdDraw create(uiMeshMesh mesh = null, TextBlobMesh textMesh = null, int pass = 0,
                MaterialPropertyBlockWrapper properties = null, int? layerId = null, Material material = null,
                Image image = null, Mesh meshObj = null,
                bool meshObjCreated = false) {
                CmdDraw newCmd = ObjectPool<CmdDraw>.alloc();
                newCmd.mesh = mesh;
                newCmd.textMesh = textMesh;
                newCmd.pass = pass;
                newCmd.properties = properties;
                newCmd.layerId = layerId;
                newCmd.material = material;
                newCmd.image = image;
                newCmd.meshObj = meshObj;
                newCmd.meshObjCreated = meshObjCreated;

                return newCmd;
            }

            public override void release() {
                ObjectPool<CmdDraw>.release(this);
            }
        }

        internal class CmdScissor : RenderCmd {
            public uiRect? deviceScissor;

            public CmdScissor() {
            }

            public override void clear() {
                this.deviceScissor = null;
            }

            public static CmdScissor create(uiRect? deviceScissor) {
                CmdScissor newCmd = ObjectPool<CmdScissor>.alloc();
                newCmd.deviceScissor = deviceScissor;
                return newCmd;
            }

            public override void release() {
                ObjectPool<CmdScissor>.release(this);
            }
        }
    }
}