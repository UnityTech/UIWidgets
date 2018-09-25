using UIWidgets.ui.txt;
using UnityEditor;
using UnityEngine;

namespace UIWidgets.ui.painting.txt
{
    public static class MeshGenrator
    {
        public static Mesh generateMesh(TextBlob textBlob, double x, double y)
        {
            var style = textBlob.style;
            var font = FontManager.instance.getOrCreate(style.fontFamily, style.UnityFontSize);
            var length = textBlob.end - textBlob.start;
            var vertices = new Vector3[length * 4];
            var triangles = new int[length * 6];
            var uv = new Vector2[length * 4]; 
            var text = textBlob.text;
            var scale = EditorGUIUtility.pixelsPerPoint;
            var fontSizeToLoad = (int)scale * style.UnityFontSize;
            var offset = new Vector3((float)Utils.PixelCorrectRound(x), (float)Utils.PixelCorrectRound(y), 0);
            font.RequestCharactersInTexture(textBlob.text.Substring(textBlob.start, textBlob.end - textBlob.start), 
                fontSizeToLoad, style.UnityFontStyle);
            for (int charIndex = 0; charIndex < length; ++charIndex)
            {
                var ch = text[charIndex + textBlob.start];
                var position = textBlob.positions[charIndex + textBlob.start];
                
                CharacterInfo charInfo = new CharacterInfo();
                if (Paragraph.isWordSpace(ch) || Paragraph.isLineEndSpace(ch) || ch== '\t')
                {
                    
                    vertices[4 * charIndex + 0] = vertices[4 * charIndex + 1] =
                        vertices[4 * charIndex + 2] = vertices[4 * charIndex + 3] = offset;
                    
                    uv[4 * charIndex + 0] = Vector2.zero;
                    uv[4 * charIndex + 1] = Vector2.zero;
                    uv[4 * charIndex + 2] = Vector2.zero;
                    uv[4 * charIndex + 3] = Vector2.zero;
                }
                else
                {
                    font.GetCharacterInfo(ch, out charInfo, fontSizeToLoad, style.UnityFontStyle);
                    var minX = charInfo.minX / scale;
                    var maxX = charInfo.maxX / scale;
                    var minY = charInfo.minY / scale;
                    var maxY = charInfo.maxY / scale;
                    vertices[4 * charIndex + 0] = offset + new Vector3((float)(position.x + minX), 
                                                      (float)(position.y - maxY), 0);
                    vertices[4 * charIndex + 1] = offset + new Vector3((float)(position.x + maxX), 
                                                      (float)(position.y - maxY), 0);
                    vertices[4 * charIndex + 2] = offset + new Vector3(
                                                      (float)(position.x + maxX), (float)(position.y - minY), 0);
                    vertices[4 * charIndex + 3] = offset + new Vector3(
                                                      (float)(position.x + minX), (float)(position.y - minY), 0);
                    
                    uv[4 * charIndex + 0] = charInfo.uvTopLeft;
                    uv[4 * charIndex + 1] = charInfo.uvTopRight;
                    uv[4 * charIndex + 2] = charInfo.uvBottomRight;
                    uv[4 * charIndex + 3] = charInfo.uvBottomLeft;
                } 
               
                triangles[6 * charIndex + 0] = 4 * charIndex + 0;
                triangles[6 * charIndex + 1] = 4 * charIndex + 1;
                triangles[6 * charIndex + 2] = 4 * charIndex + 2;

                triangles[6 * charIndex + 3] = 4 * charIndex + 0;
                triangles[6 * charIndex + 4] = 4 * charIndex + 2;
                triangles[6 * charIndex + 5] = 4 * charIndex + 3;
            }
            
            var mesh = new Mesh()
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv
            };
            var colors = new UnityEngine.Color[vertices.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = style.UnityColor;
            }
            mesh.colors = colors;
            return mesh;
        }
    }
}