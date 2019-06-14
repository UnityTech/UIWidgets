using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;
using UnityEngine.Assertions;

namespace Unity.UIWidgets.painting {
    public class TextSpan : DiagnosticableTree, IEquatable<TextSpan> {
        public delegate bool Visitor(TextSpan span);

        public readonly TextStyle style;
        public readonly string text;
        public List<string> splitedText;
        public readonly List<TextSpan> children;
        public readonly GestureRecognizer recognizer;
        public readonly HoverRecognizer hoverRecognizer;

        public TextSpan(string text = "", TextStyle style = null, List<TextSpan> children = null,
            GestureRecognizer recognizer = null, HoverRecognizer hoverRecognizer = null) {
            this.text = text;
            this.splitedText = !string.IsNullOrEmpty(text) ? EmojiUtils.splitByEmoji(text) : null;
            this.style = style;
            this.children = children;
            this.recognizer = recognizer;
            this.hoverRecognizer = hoverRecognizer;
        }

        public void build(ParagraphBuilder builder, float textScaleFactor = 1.0f) {
            var hasStyle = this.style != null;

            if (hasStyle) {
                builder.pushStyle(this.style, textScaleFactor);
            }

            if (this.splitedText != null) {
                if (this.splitedText.Count == 1 && !char.IsHighSurrogate(this.splitedText[0][0]) &&
                    !EmojiUtils.isSingleCharEmoji(this.splitedText[0][0])) {
                    builder.addText(this.splitedText[0]);
                }
                else {
                    TextStyle style = this.style ?? new TextStyle();
                    for (int i = 0; i < this.splitedText.Count; i++) {
                        builder.pushStyle(style, textScaleFactor);
                        builder.addText(this.splitedText[i]);
                        builder.pop();
                    }
                }
            }


            if (this.children != null) {
                foreach (var child in this.children) {
                    Assert.IsNotNull(child);
                    child.build(builder, textScaleFactor);
                }
            }

            if (hasStyle) {
                builder.pop();
            }
        }

        public bool hasHoverRecognizer {
            get {
                bool need = false;
                this.visitTextSpan((text) => {
                    if (text.hoverRecognizer != null) {
                        need = true;
                        return false;
                    }

                    return true;
                });
                return need;
            }
        }

        bool visitTextSpan(Visitor visitor) {
            if (!string.IsNullOrEmpty(this.text)) {
                if (!visitor.Invoke(this)) {
                    return false;
                }
            }

            if (this.children != null) {
                foreach (var child in this.children) {
                    if (!child.visitTextSpan(visitor)) {
                        return false;
                    }
                }
            }

            return true;
        }

        public TextSpan getSpanForPosition(TextPosition position) {
            D.assert(this.debugAssertIsValid());
            var offset = 0;
            var targetOffset = position.offset;
            var affinity = position.affinity;
            TextSpan result = null;
            this.visitTextSpan((span) => {
                var endOffset = offset + span.text.Length;
                if ((targetOffset == offset && affinity == TextAffinity.downstream) ||
                    (targetOffset > offset && targetOffset < endOffset) ||
                    (targetOffset == endOffset && affinity == TextAffinity.upstream)) {
                    result = span;
                    return false;
                }

                offset = endOffset;
                return true;
            });
            return result;
        }

        public string toPlainText() {
            var sb = new StringBuilder();
            this.visitTextSpan((span) => {
                sb.Append(span.text);
                return true;
            });
            return sb.ToString();
        }

        public int? codeUnitAt(int index) {
            if (index < 0) {
                return null;
            }

            var offset = 0;
            int? result = null;
            this.visitTextSpan(span => {
                if (index - offset < span.text.Length) {
                    result = span.text[index - offset];
                    return false;
                }

                offset += span.text.Length;
                return true;
            });
            return result;
        }

        bool debugAssertIsValid() {
            D.assert(() => {
                if (!this.visitTextSpan(span => {
                    if (span.children != null) {
                        foreach (TextSpan child in span.children) {
                            if (child == null) {
                                return false;
                            }
                        }
                    }

                    return true;
                })) {
                    throw new UIWidgetsError(
                        "A TextSpan object with a non-null child list should not have any nulls in its child list.\n" +
                        "The full text in question was:\n" +
                        this.toStringDeep(prefixLineOne: "  "));
                }

                return true;
            });
            return true;
        }

        public RenderComparison compareTo(TextSpan other) {
            if (this.Equals(other)) {
                return RenderComparison.identical;
            }

            if (other.text != this.text
                || ((this.children == null) != (other.children == null))
                || (this.children != null && other.children != null && this.children.Count != other.children.Count)
                || ((this.style == null) != (other.style != null))
            ) {
                return RenderComparison.layout;
            }

            RenderComparison result = Equals(this.recognizer, other.recognizer)
                ? RenderComparison.identical
                : RenderComparison.metadata;

            if (!Equals(this.hoverRecognizer, other.hoverRecognizer)) {
                result = RenderComparison.function > result ? RenderComparison.function : result;
            }

            if (this.style != null) {
                var candidate = this.style.compareTo(other.style);
                if (candidate > result) {
                    result = candidate;
                }

                if (result == RenderComparison.layout) {
                    return result;
                }
            }

            if (this.children != null) {
                for (var index = 0; index < this.children.Count; index++) {
                    var candidate = this.children[index].compareTo(other.children[index]);
                    if (candidate > result) {
                        result = candidate;
                    }

                    if (result == RenderComparison.layout) {
                        return result;
                    }
                }
            }

            return result;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((TextSpan) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.style != null ? this.style.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.text != null ? this.text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.recognizer != null ? this.recognizer.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.childHash());
                return hashCode;
            }
        }

        public bool Equals(TextSpan other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.style, other.style) && string.Equals(this.text, other.text) &&
                   childEquals(this.children, other.children) && this.recognizer == other.recognizer;
        }

        public static bool operator ==(TextSpan left, TextSpan right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextSpan left, TextSpan right) {
            return !Equals(left, right);
        }

        int childHash() {
            unchecked {
                var hashCode = 0;
                if (this.children != null) {
                    foreach (var child in this.children) {
                        hashCode = (hashCode * 397) ^ (child != null ? child.GetHashCode() : 0);
                    }
                }

                return hashCode;
            }
        }

        static bool childEquals(List<TextSpan> left, List<TextSpan> right) {
            if (ReferenceEquals(left, right)) {
                return true;
            }

            if (left == null || right == null) {
                return false;
            }

            return left.SequenceEqual(right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.whitespace;
            // Properties on style are added as if they were properties directly on
            // this TextSpan.
            if (this.style != null) {
                this.style.debugFillProperties(properties);
            }

            properties.add(new DiagnosticsProperty<GestureRecognizer>(
                "recognizer", this.recognizer,
                description: this.recognizer == null ? "" : this.recognizer.GetType().FullName,
                defaultValue: Diagnostics.kNullDefaultValue
            ));

            properties.add(new StringProperty("text", this.text, showName: false,
                defaultValue: Diagnostics.kNullDefaultValue));
            if (this.style == null && this.text == null && this.children == null) {
                properties.add(DiagnosticsNode.message("(empty)"));
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            if (this.children == null) {
                return new List<DiagnosticsNode>();
            }

            return this.children.Select((child) => {
                if (child != null) {
                    return child.toDiagnosticsNode();
                }
                else {
                    return DiagnosticsNode.message("<null child>");
                }
            }).ToList();
        }
    }
}