using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial class PictureFlusher {
        internal abstract class RenderCmd : PoolItem {
            
        }
        
        internal class CmdLayer : RenderCmd {
            public RenderLayer layer;

            public CmdLayer() {
            }

            public override void clear() {
                this.layer = null;
            }

            public static CmdLayer create(RenderLayer layer) {
                CmdLayer newCmd = ItemPoolManager.alloc<CmdLayer>();
                newCmd.layer = layer;
                return newCmd;
            }
        }
        
        internal class CmdDraw : RenderCmd {
            public uiMeshMesh mesh;
            public TextBlobMesh textMesh;
            public int pass;
            public MaterialPropertyBlock properties;
            public int? layerId;
            public Material material;
            public Image image; // just to keep a reference to avoid GC.
            public Mesh meshObj;
            public bool meshObjCreated;

            public static readonly Matrix4x4 idMat = Matrix4x4.identity;
            public static readonly Matrix3 idMat3 = Matrix3.I();
            public static readonly int texId = Shader.PropertyToID("_tex");
            public static readonly int matId = Shader.PropertyToID("_mat");

            
            public override void clear() {
                this.mesh?.dispose();
                this.textMesh?.dispose();
            }

            public CmdDraw() {
            }

            public static CmdDraw create(uiMeshMesh mesh = null, TextBlobMesh textMesh = null, int pass = 0,
                MaterialPropertyBlock properties = null, int? layerId = null, Material material = null,
                Image image = null, Mesh meshObj = null,
                bool meshObjCreated = false) {
                CmdDraw newCmd = ItemPoolManager.alloc<CmdDraw>();
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
        }

        internal class CmdScissor : RenderCmd {
            public uiRect? deviceScissor;

            public CmdScissor() {
            }
            
            public override void clear() {
                this.deviceScissor = null;
            }

            public static CmdScissor create(uiRect? deviceScissor) {
                CmdScissor newCmd = ItemPoolManager.alloc<CmdScissor>();
                newCmd.deviceScissor = deviceScissor;
                return newCmd;
            }
        }
    }
}