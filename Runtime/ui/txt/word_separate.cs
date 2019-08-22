namespace Unity.UIWidgets.ui {
    struct WordSeparate {
        internal enum CharacterType {
            LetterLike,
            Symbol,
            WhiteSpace
        }

        string _text;

        public WordSeparate(string text) {
            this._text = text;
        }

        public Range<int> findWordRange(int index) {
            if (index >= this._text.Length) {
                return new Range<int>(0, 0);
            }

            var t = classifyChar(this._text, index);
            int start = index;
            for (int i = index; i >= 0; --i) {
                if (!char.IsLowSurrogate(this._text[start])) {
                    if (classifyChar(this._text, i) != t) {
                        break;
                    }

                    start = i;
                }
            }

            int end = index;
            for (int i = index; i < this._text.Length; ++i) {
                if (!char.IsLowSurrogate(this._text[i])) {
                    if (classifyChar(this._text, i) != t) {
                        break;
                    }

                    end = i;
                }
            }

            return new Range<int>(start, end + 1);
        }


        internal static CharacterType classifyChar(string text, int index) {
            return classifyChar(text[index]);
        }
        
        internal static CharacterType classifyChar(char ch) {
            if (char.IsWhiteSpace(ch)) {
                return CharacterType.WhiteSpace;
            }

            if (char.IsLetterOrDigit(ch) || ch == '\'') {
                return CharacterType.LetterLike;
            }

            return CharacterType.Symbol;
        }
    }
}