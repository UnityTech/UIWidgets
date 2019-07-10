using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.ui {
    struct TextBuff {
        public readonly string text;
        public readonly int offset;
        public readonly int size;

        public TextBuff(string text, int? offset = null, int? size = null) {
            this.text = text;
            this.offset = offset ?? 0;
            this.size = size ?? text.Length - this.offset;
        }

        public char charAt(int index) {
            return this.text[this.offset + index];
        }

        public TextBuff subBuff(int shift, int size) {
            D.assert(shift >= 0 && shift <= this.size);
            D.assert(shift + size <= this.size);
            return new TextBuff(this.text, this.offset + shift, size);
        }

        public override string ToString() {
            return this.text.Substring(this.offset, this.size);
        }

        public string subString(int shift, int size) {
            return this.text.Substring(this.offset + shift, size);
        }
    }
}