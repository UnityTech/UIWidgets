using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.gallery {
    public class SyntaxHighlighterStyle {
        public SyntaxHighlighterStyle(
            TextStyle baseStyle = null,
            TextStyle numberStyle = null,
            TextStyle commentStyle = null,
            TextStyle keywordStyle = null,
            TextStyle stringStyle = null,
            TextStyle punctuationStyle = null,
            TextStyle classStyle = null,
            TextStyle constantStyle = null
        ) {
            this.baseStyle = baseStyle;
            this.numberStyle = numberStyle;
            this.commentStyle = commentStyle;
            this.keywordStyle = keywordStyle;
            this.stringStyle = stringStyle;
            this.punctuationStyle = punctuationStyle;
            this.classStyle = classStyle;
            this.constantStyle = constantStyle;
        }

        public static SyntaxHighlighterStyle lightThemeStyle() {
            return new SyntaxHighlighterStyle(
                baseStyle: new TextStyle(color: new Color(0xFF000000)),
                numberStyle: new TextStyle(color: new Color(0xFF1565C0)),
                commentStyle: new TextStyle(color: new Color(0xFF9E9E9E)),
                keywordStyle: new TextStyle(color: new Color(0xFF9C27B0)),
                stringStyle: new TextStyle(color: new Color(0xFF43A047)),
                punctuationStyle: new TextStyle(color: new Color(0xFF000000)),
                classStyle: new TextStyle(color: new Color(0xFF512DA8)),
                constantStyle: new TextStyle(color: new Color(0xFF795548))
            );
        }

        public static SyntaxHighlighterStyle darkThemeStyle() {
            return new SyntaxHighlighterStyle(
                baseStyle: new TextStyle(color: new Color(0xFFFFFFFF)),
                numberStyle: new TextStyle(color: new Color(0xFF1565C0)),
                commentStyle: new TextStyle(color: new Color(0xFF9E9E9E)),
                keywordStyle: new TextStyle(color: new Color(0xFF80CBC4)),
                stringStyle: new TextStyle(color: new Color(0xFF009688)),
                punctuationStyle: new TextStyle(color: new Color(0xFFFFFFFF)),
                classStyle: new TextStyle(color: new Color(0xFF009688)),
                constantStyle: new TextStyle(color: new Color(0xFF795548))
            );
        }

        public readonly TextStyle baseStyle;
        public readonly TextStyle numberStyle;
        public readonly TextStyle commentStyle;
        public readonly TextStyle keywordStyle;
        public readonly TextStyle stringStyle;
        public readonly TextStyle punctuationStyle;
        public readonly TextStyle classStyle;
        public readonly TextStyle constantStyle;
    }

    public abstract class SyntaxHighlighter {
        // ignore: one_member_abstracts
        public abstract TextSpan format(string src);
    }

    public class DartSyntaxHighlighter : SyntaxHighlighter {
        public DartSyntaxHighlighter(SyntaxHighlighterStyle _style = null) {
            this._spans = new List<_HighlightSpan> { };
            this._style = _style ?? SyntaxHighlighterStyle.darkThemeStyle();
        }

        SyntaxHighlighterStyle _style;

        readonly List<string> _keywords = new List<string> {
            "abstract", "as", "assert", "async", "await", "break", "case", "catch",
            "class", "const", "continue", "default", "deferred", "do", "dynamic", "else",
            "enum", "export", "external", "extends", "factory", "false", "final",
            "finally", "for", "get", "if", "implements", "import", "in", "is", "library",
            "new", "null", "operator", "part", "rethrow", "return", "set", "static",
            "super", "switch", "sync", "this", "throw", "true", "try", "typedef", "var",
            "void", "while", "with", "yield"
        };

        readonly List<string> _builtInTypes = new List<string> {
            "int", "double", "num", "bool"
        };

        string _src;
        StringScanner _scanner;

        List<_HighlightSpan> _spans;

        public override TextSpan format(string src) {
            this._src = src;
            this._scanner = new StringScanner(this._src);

            if (this._generateSpans()) {
                List<TextSpan> formattedText = new List<TextSpan> { };
                int currentPosition = 0;

                foreach (_HighlightSpan span in this._spans) {
                    if (currentPosition != span.start) {
                        formattedText.Add(new TextSpan(text: this._src.Substring(currentPosition, span.start)));
                    }

                    formattedText.Add(new TextSpan(style: span.textStyle(this._style),
                        text: span.textForSpan(this._src)));

                    currentPosition = span.end;
                }

                if (currentPosition != this._src.Length) {
                    formattedText.Add(new TextSpan(text: this._src.Substring(currentPosition, this._src.Length)));
                }

                return new TextSpan(style: this._style.baseStyle, children: formattedText);
            }
            else {
                return new TextSpan(style: this._style.baseStyle, text: src);
            }
        }

        bool _generateSpans() {
            int lastLoopPosition = this._scanner.position;

            while (!this._scanner.isDone) {
                this._scanner.scan(new Regex(@"\s+"));

                // Block comments
                if (this._scanner.scan(new Regex(@"/\*(.|\n)*\*/"))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType.comment,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                    ));
                    continue;
                }

                // Line comments
                if (this._scanner.scan(new Regex(@"//"))) {
                    int startComment = this._scanner.lastMatch.Index;

                    bool eof = false;
                    int endComment;
                    if (this._scanner.scan(new Regex(@".*\n"))) {
                        endComment = this._scanner.lastMatch.Index + this._scanner.lastMatch.Length - 1;
                    }
                    else {
                        eof = true;
                        endComment = this._src.Length;
                    }

                    this._spans.Add(new _HighlightSpan(
                        _HighlightType.comment,
                        startComment,
                        endComment
                    ));

                    if (eof) {
                        break;
                    }

                    continue;
                }

                // Raw r"String"
                if (this._scanner.scan(new Regex(@"r"".*"""))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType._string,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                    ));
                    continue;
                }

                // Raw r"String"
                if (this._scanner.scan(new Regex(@"r"".*"""))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType._string,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                    ));
                    continue;
                }

                // Multiline """String"""
                if (this._scanner.scan(new Regex(@"""""""(?:[^""\\]|\\(.|\n))*"""""""))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType._string,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                    ));
                    continue;
                }

                // Multiline '''String'''
                if (this._scanner.scan(new Regex(@"'''(?:[^""\\]|\\(.|\n))*'''"))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType._string,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                    ));
                    continue;
                }

                // "String"
                if (this._scanner.scan(new Regex(@"""(?:[^""\\]|\\.)*"""))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType._string,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                    ));
                    continue;
                }

                // "String"
                if (this._scanner.scan(new Regex(@"""(?:[^""\\]|\\.)*"""))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType._string,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                    ));
                    continue;
                }

                // Double
                if (this._scanner.scan(new Regex(@"\d+\.\d+"))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType.number,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                    ));
                    continue;
                }

                // Integer
                if (this._scanner.scan(new Regex(@"\d+"))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType.number,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length)
                    );
                    continue;
                }

                // Punctuation
                if (this._scanner.scan(new Regex(@"[\[\]{}().!=<>&\|\?\+\-\*/%\^~;:,]"))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType.punctuation,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                    ));
                    continue;
                }

                // Meta data
                if (this._scanner.scan(new Regex(@"@\w+"))) {
                    this._spans.Add(new _HighlightSpan(
                        _HighlightType.keyword,
                        this._scanner.lastMatch.Index,
                        this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                    ));
                    continue;
                }

                // Words
                if (this._scanner.scan(new Regex(@"\w+"))) {
                    _HighlightType? type = null;

                    string word = this._scanner.lastMatch.Groups[0].Value;
                    if (word.StartsWith("_")) {
                        word = word.Substring(1);
                    }

                    if (this._keywords.Contains(word)) {
                        type = _HighlightType.keyword;
                    }
                    else if (this._builtInTypes.Contains(word)) {
                        type = _HighlightType.keyword;
                    }
                    else if (this._firstLetterIsUpperCase(word)) {
                        type = _HighlightType.klass;
                    }
                    else if (word.Length >= 2 && word.StartsWith("k") &&
                             this._firstLetterIsUpperCase(word.Substring(1))) {
                        type = _HighlightType.constant;
                    }

                    if (type != null) {
                        this._spans.Add(new _HighlightSpan(
                            type,
                            this._scanner.lastMatch.Index,
                            this._scanner.lastMatch.Index + this._scanner.lastMatch.Length
                        ));
                    }
                }

                // Check if this loop did anything
                if (lastLoopPosition == this._scanner.position) {
                    // Failed to parse this file, abort gracefully
                    return false;
                }

                lastLoopPosition = this._scanner.position;
            }

            this._simplify();
            return true;
        }

        void _simplify() {
            for (int i = this._spans.Count - 2; i >= 0; i -= 1) {
                if (this._spans[i].type == this._spans[i + 1].type && this._spans[i].end == this._spans[i + 1].start) {
                    this._spans[i] = new _HighlightSpan(
                        this._spans[i].type,
                        this._spans[i].start,
                        this._spans[i + 1].end
                    );
                    this._spans.RemoveAt(i + 1);
                }
            }
        }

        bool _firstLetterIsUpperCase(string str) {
            if (str.isNotEmpty()) {
                string first = str.Substring(0, 1);
                return first == first.ToUpper();
            }

            return false;
        }
    }

    enum _HighlightType {
        number,
        comment,
        keyword,
        _string,
        punctuation,
        klass,
        constant
    }

    class _HighlightSpan {
        public _HighlightSpan(_HighlightType? type, int start, int end) {
            this.type = type;
            this.start = start;
            this.end = end;
        }

        public readonly _HighlightType? type;
        public readonly int start;
        public readonly int end;

        public string textForSpan(string src) {
            return src.Substring(this.start, this.end);
        }

        public TextStyle textStyle(SyntaxHighlighterStyle style) {
            if (this.type == _HighlightType.number) {
                return style.numberStyle;
            }
            else if (this.type == _HighlightType.comment) {
                return style.commentStyle;
            }
            else if (this.type == _HighlightType.keyword) {
                return style.keywordStyle;
            }
            else if (this.type == _HighlightType._string) {
                return style.stringStyle;
            }
            else if (this.type == _HighlightType.punctuation) {
                return style.punctuationStyle;
            }
            else if (this.type == _HighlightType.klass) {
                return style.classStyle;
            }
            else if (this.type == _HighlightType.constant) {
                return style.constantStyle;
            }
            else {
                return style.baseStyle;
            }
        }
    }

    public class StringScanner {
        string _source { get; set; }
        public int position { get; set; }

        public Match lastMatch {
            get { return this._lastMatch; }
        }
        Match _lastMatch;

        public StringScanner(string source) {
            this._source = source;
            this.position = 0;
        }

        public override string ToString() {
            return this.isDone ? "" : this._source.Substring(this.position);
        }

        public bool isDone {
            get { return this.position >= this._source.Length; }
        }

        public bool scan(Regex regex) {
            var match = regex.Match(this.ToString());

            if (match.Success) {
                this.position += match.Length;
                this._lastMatch = match;
                return true;
            }
            else {
                return false;
            }
        }
    }
}