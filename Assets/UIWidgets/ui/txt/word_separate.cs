using System;

namespace UIWidgets.ui
{
    public class WordSeparate
    {
        
        enum Direction
        {
            Forward,
            Backward,
        }
        
        enum characterType
        {
            LetterLike,
            Symbol,
            WhiteSpace
        }

        private string _text;

        public WordSeparate(string text)
        {
            this._text = text;
        }
        
        public IndexRange findWordRange(int index)
        {
            var t = classifyChar(index);
            int start = index;
            for (int i = index; i >= 0; --i)
            {
                if (!char.IsLowSurrogate(_text[start]))
                {
                    if (classifyChar(i) != t)
                    {
                        break;
                    }
                    start = i;
                }
            }

            int end = index;
            for (int i = index; i < _text.Length; ++i)
            {
                if (!char.IsLowSurrogate(_text[i]))
                {
                    if (classifyChar(i) != t)
                    {
                        break;
                    }
                    end = i;
                }
            }
            return new IndexRange(start, end + 1);
        }

        
        private characterType classifyChar(int index)
        {
            if (char.IsWhiteSpace(_text, index))
                return characterType.WhiteSpace;
            if (char.IsLetterOrDigit(_text, index) || _text[index] == '\'')
                return characterType.LetterLike;
            return characterType.Symbol;
        }

    }
}