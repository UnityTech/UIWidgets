using UnityEditor;
using UnityEngine;

namespace UIWidgets.editor
{
    public class TextMeshWindow : EditorWindow
    {
        Font font;
        string str = "Hello World";
        Mesh mesh;
        
        // Add menu named "My Window" to the Window menu
        [MenuItem("TextMeshWindow/TextMeshWindow")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            TextMeshWindow window = (TextMeshWindow)EditorWindow.GetWindow(typeof(TextMeshWindow));
            window.Show();
        }
        
        void OnFontTextureRebuilt(Font changedFont)
        {
            if (changedFont != font)
                return;
            TextGenerator t = new TextGenerator();
            RebuildMesh();
        }


        void RebuildMesh()
        {
            // Generate a mesh for the characters we want to print.
            var vertices = new Vector3[str.Length * 4];
            var triangles = new int[str.Length * 6];
            var uv = new Vector2[str.Length * 4];
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < str.Length; i++)
            {
                // Get character rendering information from the font
                CharacterInfo ch;
                font.GetCharacterInfo(str[i], out ch);

                vertices[4 * i + 0] = pos + new Vector3(ch.minX, -ch.maxY + 100, 0);
                vertices[4 * i + 1] = pos + new Vector3(ch.maxX, -ch.maxY + 100, 0);
                vertices[4 * i + 2] = pos + new Vector3(ch.maxX, -ch.minY + 100, 0);
                vertices[4 * i + 3] = pos + new Vector3(ch.minX, -ch.minY + 100, 0);

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
        
        private void OnEnable()
        {
            font = Font.CreateDynamicFontFromOSFont("Helvetica", 16);
            // Set the rebuild callback so that the mesh is regenerated on font changes.
            Font.textureRebuilt += OnFontTextureRebuilt;

            // Request characters.
            font.RequestCharactersInTexture(str);

            // Set up mesh.
            mesh = new Mesh();
            // Generate font mesh.
            RebuildMesh();
        }

        void OnGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                font.RequestCharactersInTexture(str);
                Material mat = font.material;
                mat.SetPass(0);
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
               // Graphics.DrawMesh(mesh, new Vector3(0, 0, 100), Quaternion.identity, font.material, 0);
            }

            if (GUI.Button(new Rect(200, 200, 100, 100), "Set Font"))
            {
                font = Font.CreateDynamicFontFromOSFont("Helvetica", 16);
              
                // Request characters.
                font.RequestCharactersInTexture(str);

                // Set up mesh.
                mesh = new Mesh();
                // Generate font mesh.
                RebuildMesh();
            }
        }

        void OnUpdate()
        {
            font.RequestCharactersInTexture(str);
        }
    }
}