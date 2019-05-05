
namespace Unity.UIWidgets.ui {
    class WordBreaker {
        public const uint U16_SURROGATE_OFFSET = ((0xd800 << 10) + 0xdc00 - 0x10000);
        TextBuff _text;
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
                this._current = this._findNextBoundaryNormal();
            }

            return this._current;
        }

        public void setText(TextBuff text) {
            this._text = text;
            this._last = 0;
            this._current = 0;
            this._scanOffset = 0;
            this._inEmailOrUrl = false;
            this.nextUntilCodePoint();
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
            this._text = default;
        }

        int _findNextBreakInEmailOrUrl() {
            return 0;
        }

        int _findNextBoundaryNormal() {
            if (this._current == this._text.size) {
                return -1;
            }
            
            WordSeparate.characterType preType = WordSeparate.classifyChar(this._text.charAt(this._current));
            bool preBoundaryChar = isBoundaryChar(this._text.charAt(this._current));
            this._current++;
            if (preBoundaryChar) {
                return this._current;
            }
            
            for (; this._current < this._text.size; ++this._current) {
                this.nextUntilCodePoint();
                if (this._current >= this._text.size) {
                    break;
                }

                if (isBoundaryChar(this._text.charAt(this._current))) {
                    break;
                }
                var currentType = WordSeparate.classifyChar(this._text.charAt(this._current));
                if ((currentType == WordSeparate.characterType.WhiteSpace) 
                    != (preType == WordSeparate.characterType.WhiteSpace)) {
                    break;
                }
                preType = currentType;
            }
            return this._current;
        }
        
        void _detectEmailOrUrl() {
        }

        static uint nextCode(TextBuff text, ref int index, int end) {
            uint ch = text.charAt(index);
            index++;
            if (isLeadSurrogate(ch)) {
                if (index < end && isTrailSurrogate(text.charAt(index))) {
                    char ch2 = text.charAt(index);
                    index++;
                    ch = getSupplementary(ch, ch2);
                }
            }

            return ch;
        }

        static uint preCode(TextBuff text, ref int index, int start) {
            --index;
            uint ch = text.charAt(index);
            if (isTrailSurrogate(ch)) {
                if (index > start && isLeadSurrogate(text.charAt(index - 1))) {
                    ch = getSupplementary(text.charAt(index - 1), ch);
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

        public static bool isBoundaryChar(char code) {
            if (char.IsPunctuation(code)) {
                return true;
            }
            if (code >= 0x4E00 && code <= 0x9FFF) { // cjk https://en.wikipedia.org/wiki/CJK_Unified_Ideographs
                return true;
            }

            // https://social.msdn.microsoft.com/Forums/en-US/0d1888de-9745-4dd1-80fd-d3c29d3e381d/checking-for-japanese-characters-in-a-string?forum=vcmfcatl
            if (code >= 0x3040 && code <= 0x30FF) { // Hiragana or Katakana
                return true;
            }

            return false;
        }
        
        void nextUntilCodePoint() {
            while (this._current < this._text.size
                   && (char.IsLowSurrogate(this._text.charAt(this._current)) 
                       ||  char.IsHighSurrogate(this._text.charAt(this._current)))) {
                this._current++;
            }
        }
    }
}