using UnityEngine;

namespace UIWidgets.ui
{
    public class TextMesh: IMesh
    {
        private Mesh _mesh;
        private FontEntry _fontEntry;
        private string _text;
        private StyledRuns.Run _run;
        private int _textureVersion;
        
        public TextMesh(Vector2d pos, string text, Vector2d[] _characterPositions, FontEntry fontEntry, StyledRuns.Run run)
        {
            _fontEntry = fontEntry;
            _text = text;
            this._run = run;
            
            var vertices = new Vector3[_text.Length * 4];
            var triangles = new int[_text.Length * 6];
            var font = fontEntry.font;
            var offset = new Vector3((float)Utils.PixelCorrectRound(pos.x), (float)Utils.PixelCorrectRound(pos.y), 0);
            font.RequestCharactersInTexture(_text.Substring(_run.start, _run.end - _run.start), 
                _run.style.UnityFontSize, _run.style.UnityFontStyle);
            for (int charIndex = _run.start; charIndex < _run.end; ++charIndex)
            {
                CharacterInfo charInfo = new CharacterInfo();
                if (_text[charIndex] != '\n' && _text[charIndex] != '\t')
                {
                    Debug.Assert(font.GetCharacterInfo(_text[charIndex], out charInfo, _run.style.UnityFontSize, _run.style.UnityFontStyle));
                    var position = _characterPositions[charIndex];
                    vertices[4 * charIndex + 0] = offset + new Vector3((float)(position.x + charInfo.minX), 
                                                      (float)(position.y - charInfo.maxY), 0);
                    vertices[4 * charIndex + 1] = offset + new Vector3((float)(position.x + charInfo.maxX), 
                                                      (float)(position.y - charInfo.maxY), 0);
                    vertices[4 * charIndex + 2] = offset + new Vector3(
                                                      (float)(position.x + charInfo.maxX), (float)(position.y - charInfo.minY), 0);
                    vertices[4 * charIndex + 3] = offset + new Vector3(
                                                      (float)(position.x + charInfo.minX), (float)(position.y - charInfo.minY), 0);
                }
                else
                {
                    vertices[4 * charIndex + 0] = vertices[4 * charIndex + 1] =
                        vertices[4 * charIndex + 2] = vertices[4 * charIndex + 3] = offset;
                } 
               
                triangles[6 * charIndex + 0] = 4 * charIndex + 0;
                triangles[6 * charIndex + 1] = 4 * charIndex + 1;
                triangles[6 * charIndex + 2] = 4 * charIndex + 2;

                triangles[6 * charIndex + 3] = 4 * charIndex + 0;
                triangles[6 * charIndex + 4] = 4 * charIndex + 2;
                triangles[6 * charIndex + 5] = 4 * charIndex + 3;
            }

            var uv = getTextureUV();
            var mesh = new Mesh()
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv
            };
            var colors = new UnityEngine.Color[vertices.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = _run.style.UnityColor;
            }

            mesh.colors = colors;
            _textureVersion = _fontEntry.textureBuildVersion;
            _mesh = mesh;
        }
        
        public void syncTextureUV()
        {
            if (_fontEntry.textureBuildVersion != _textureVersion) // texture has been rebuilt, update the texture uv
            {
                _mesh.uv = getTextureUV();
            }
        }

        public Mesh mesh
        {
            get { return _mesh; }
        }

        private Vector2[] getTextureUV()
        {
            var font = _fontEntry.font;
            var uv = _mesh == null ? new Vector2[_text.Length * 4] : _mesh.uv; 
           
            for (int charIndex = _run.start; charIndex < _run.end; ++charIndex)
            {
                CharacterInfo charInfo = new CharacterInfo();
                if (_text[charIndex] != '\n' && _text[charIndex] != '\t')
                {
                    font.GetCharacterInfo(_text[charIndex], out charInfo, _run.style.UnityFontSize,
                        _run.style.UnityFontStyle);
                }

                if (Paragraph.isWordSpace(_text[charIndex]) || Paragraph.isLineEndSpace(_text[charIndex]) || _text[charIndex] == '\t')
                {                    
                    uv[4 * charIndex + 0] = Vector2.zero;
                    uv[4 * charIndex + 1] = Vector2.zero;
                    uv[4 * charIndex + 2] = Vector2.zero;
                    uv[4 * charIndex + 3] = Vector2.zero;
                } else
                {
                    uv[4 * charIndex + 0] = charInfo.uvTopLeft;
                    uv[4 * charIndex + 1] = charInfo.uvTopRight;
                    uv[4 * charIndex + 2] = charInfo.uvBottomRight;
                    uv[4 * charIndex + 3] = charInfo.uvBottomLeft;
                }
            }

            return uv;
        }
    }
}