using UnityEditor;
using UnityEngine;

namespace UIWidgets.Tests {
    public class IMGUIText: EditorWindow {
        Font font;
        string str = "wei";
        Mesh mesh;
        
        [MenuItem("UIWidgetsTests/IMGUIText")]
        public static void getIMGUIText() {
            EditorWindow.GetWindow(typeof(IMGUIText));
        }

        void OnEnable() {
            font = Font.CreateDynamicFontFromOSFont("Helvetica", 14);
            // Set the rebuild callback so that the mesh is regenerated on font changes.
            Font.textureRebuilt += OnFontTextureRebuilt;

            // Request characters.
            font.RequestCharactersInTexture(str);
            // Set up mesh.
            mesh = new Mesh();
            // Generate font mesh.
            RebuildMesh();
        }
        
        void RebuildMesh()
        {
            // Generate a mesh for the characters we want to print.
            var vertices = new Vector3[str.Length * 4];
            var triangles = new int[str.Length * 6];
            var uv = new Vector2[str.Length * 4];
            Vector3 pos = new Vector3(20, 60f, 0.0f);
            for (int i = 0; i < str.Length; i++)
            {
                // Get character rendering information from the font
                CharacterInfo ch;
                font.GetCharacterInfo(str[i], out ch);

                vertices[4 * i + 0] = pos + new Vector3(ch.minX, -ch.maxY, 0);
                vertices[4 * i + 1] = pos + new Vector3(ch.maxX, -ch.maxY, 0);
                vertices[4 * i + 2] = pos + new Vector3(ch.maxX, -ch.minY, 0);
                vertices[4 * i + 3] = pos + new Vector3(ch.minX, -ch.minY, 0);

                uv[4 * i + 0] = ch.uvTopLeft;
                uv[4 * i + 1] = ch.uvTopRight;
                uv[4 * i + 2] = ch.uvBottomRight;
                uv[4 * i + 3] = ch.uvBottomLeft;

                triangles[6 * i + 0] = 4 * i + 0;
                triangles[6 * i + 1] = 4 * i + 1;
                triangles[6 * i + 2] = 4 * i + 2;

                triangles[6 * i + 3] = 4 * i + 0;
                triangles[6 * i + 4] = 4 * i + 2;
                triangles[6 * i + 5] = 4 * i + 3;

                // Advance character position
                pos += new Vector3(ch.advance, 0, 0);
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
        }

        public void setup() {
            {
               
            }
        }
        
        void OnDestroy()
        {
            Font.textureRebuilt -= OnFontTextureRebuilt;
        }
        
        void OnFontTextureRebuilt(Font changedFont)
        {
            if (changedFont != font)
                return;

            RebuildMesh();
        }

        void OnGUI() {
            if (Event.current.type == EventType.Repaint) {
                font.material.SetPass(0);
                var tt = font.material.mainTexture;
                font.material.color = Color.black;
                Graphics.DrawMeshNow(this.mesh, Matrix4x4.identity);
            }
        }

        void Update() {
            font.RequestCharactersInTexture(str);
        }
    }
}