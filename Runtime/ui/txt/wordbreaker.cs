namespace Unity.UIWidgets.ui {
    public class WordBreaker {
        public const uint U16_SURROGATE_OFFSET = ((0xd800 << 10) + 0xdc00 - 0x10000);
        string _text;
        int _offset;
        int _size;
        int _current;
        int _last;
        int _scanOffset;
        bool _inEmailOrUrl;


        public int next() {
            this._last = this._current;
            this._detectEmailOrUrl();
            if (this._inEmailOrUrl) {
                this._current = this._findNextBreakInEmailOrUrl();
            }
            else {
                this._current = this._findNextBreakNormal();
            }

            return this._current;
        }

        public void setText(string data, int offset, int size) {
            this._text = data;
            this._offset = offset;
            this._size = size;
            this._last = 0;
            this._current = 0;
            this._scanOffset = 0;
            this._inEmailOrUrl = false;
        }

        public int current() {
            return this._current;
        }

        public int wordStart() {
            if (this._inEmailOrUrl) {
                return this._last;
            }

            var result = this._last;
            while (result < this._current) {
                int ix = result;
                uint c = nextCode(this._text, ref ix, this._current);
                if (!LayoutUtils.isLineEndSpace((char) c)) {
                    break;
                }

                result = ix;
            }

            return result;
        }

        public int wordEnd() {
            if (this._inEmailOrUrl) {
                return this._last;
            }

            int result = this._current;
            while (result > this._last) {
                int ix = result;
                uint ch = preCode(this._text, ref ix, this._last);
                if (!LayoutUtils.isLineEndSpace((char) ch)) {
                    break;
                }

                result = ix;
            }

            return result;
        }

        public int breakBadness() {
            return (this._inEmailOrUrl && this._current < this._scanOffset) ? 1 : 0;
        }

        public void finish() {
            this._text = null;
        }

        int _findNextBreakInEmailOrUrl() {
            return 0;
        }

        int _findNextBreakNormal() {
            if (this._current == this._size) {
                return -1;
            }

            this._current++;
            for (; this._current < this._size; ++this._current) {
                char c = this._text[this._current + this._offset];
                if (LayoutUtils.isWordSpace(c) || c == '\t') {
                    return this._current;
                }
            }

            return this._current;
        }

        void _detectEmailOrUrl() {
        }

        static uint nextCode(string text, ref int index, int end) {
            uint ch = text[index++];
            if (isLeadSurrogate(ch)) {
                if (index < end && isTrailSurrogate(text[index])) {
                    char ch2 = text[index];
                    index++;
                    ch = getSupplementary(ch, ch2);
                }
            }

            return ch;
        }

        static uint preCode(string text, ref int index, int start) {
            uint ch = text[--index];
            if (isTrailSurrogate(ch)) {
                if (index > start && isLeadSurrogate(text[index - 1])) {
                    ch = getSupplementary(text[index - 1], ch);
                    --index;
                }
            }

            return ch;
        }

        public static bool isLeadSurrogate(uint c) {
            return ((c) & 0xfffffc00) == 0xd800;
        }


        public static bool isTrailSurrogate(uint c) {
            return ((c) & 0xfffffc00) == 0xdc00;
        }

        public static uint getSupplementary(uint lead, uint trail) {
            return (char) (((uint) (lead) << 10) + (uint) (trail - U16_SURROGATE_OFFSET));
        }
    }
}