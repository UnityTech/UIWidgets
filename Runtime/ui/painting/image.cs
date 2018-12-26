using UnityEngine;

namespace UIWidgets.ui {
    public class Image {
        public Image(byte[] raw = null, Texture2D texture = null) {
            this.rawData = raw ?? new byte[0];
            this._texture = texture;
        }

        public byte[] rawData;

        public int height {
            get { return texture != null ? texture.height : 0; }
        }

        public int width {
            get { return texture != null ? texture.width : 0; }
        }

        public Texture2D texture {
            get {
                if (_texture == null && rawData.Length != 0) {
                    _texture = new Texture2D(2, 2);
                    _texture.LoadImage(rawData);
                }

                return _texture;
            }
        }

        private Texture2D _texture;
    }
}