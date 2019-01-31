namespace Unity.UIWidgets.ui {
    public class WordSeparate {
        enum Direction {
            Forward,
            Backward,
        }

        enum characterType {
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

            var t = this.classifyChar(index);
            int start = index;
            for (int i = index; i >= 0; --i) {
                if (!char.IsLowSurrogate(this._text[start])) {
                    if (this.classifyChar(i) != t) {
                        break;
                    }

                    start = i;
                }
            }

            int end = index;
            for (int i = index; i < this._text.Length; ++i) {
                if (!char.IsLowSurrogate(this._text[i])) {
                    if (this.classifyChar(i) != t) {
                        break;
                    }

                    end = i;
                }
            }

            return new Range<int>(start, end + 1);
        }


        characterType classifyChar(int index) {
            if (char.IsWhiteSpace(this._text, index)) {
                return characterType.WhiteSpace;
            }

            if (char.IsLetterOrDigit(this._text, index) || this._text[index] == '\'') {
                return characterType.LetterLike;
            }

            return characterType.Symbol;
        }
    }
}