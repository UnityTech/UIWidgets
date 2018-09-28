using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIWidgets.foundation;
using UIWidgets.ui;
using UnityEditor;
using UnityEngine.Assertions;

namespace UIWidgets.painting
{
    public class GestureMock
    {
        
    }
    public class TextSpan: DiagnosticableTree, IEquatable<TextSpan>
    {
        public delegate bool Visitor(TextSpan span);
        public readonly TextStyle style;
        public readonly string text;
        public readonly List<TextSpan> children;
        public readonly GestureMock recognizer;

        public TextSpan(string text = "", TextStyle style = null, List<TextSpan> children = null)
        {
            this.text = text;
            this.style = style;
            this.children = children;
        }
        
        public void build(ParagraphBuilder builder, double textScaleFactor = 1.0)
        {
            var hasTyle = style != null;
            if (hasTyle)
            {
                builder.pushStyle(style);
            }
            if (!string.IsNullOrEmpty(text))
            {
                builder.addText(text);
            }
            if (children != null)
            {
                foreach (var child in children)
                {
                    Assert.IsNotNull(child);
                    child.build(builder, textScaleFactor);
                }
            }

            if (hasTyle)
            {
                builder.pop();
            }
        }

        bool visitTextSpan(Visitor visitor)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (!visitor.Invoke(this))
                {
                    return false;
                }
            }
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (!child.visitTextSpan(visitor))
                    {
                        return false;
                    }
                }
               
            }
            return true;
        }

        TextSpan getSpanForPosition(TextPosition position)
        {
            var offset = 0;
            var targetOffset = position.offset;
            var affinity = position.affinity;
            TextSpan result = null;
            visitTextSpan((span) =>
            {
                var endOffset = offset + span.text.Length;
                if ((targetOffset == offset && affinity == TextAffinity.downstream) ||
                    (targetOffset > offset && targetOffset < endOffset) ||
                    (targetOffset == endOffset && affinity == TextAffinity.upstream))
                {
                    result = span;
                    return false;
                }

                offset = endOffset;
                return true;
            });
            return result;
        }

        public string toPlainText()
        {
            var sb = new StringBuilder();
            visitTextSpan((span) =>
            {
                sb.Append(span.text);
                return true;
            });
            return sb.ToString();
        }

        public int? codeUnitAt(int index)
        {
            if (index < 0)
            {
                return null;
            }

            var offset = 0;
            int? result = null;
            visitTextSpan(span =>
            {
                if (index - offset < span.text.Length)
                {
                    result = span.text[index - offset];
                    return false;
                }

                offset += span.text.Length;
                return true;
            });
            return result;
        }

        public RenderComparison compareTo(TextSpan other)
        {
            if (Equals(other))
            {
                return RenderComparison.identical;
            }

            if (other.text != text 
                || ((children == null) != (other.children == null))
                || (children != null && other.children != null && children.Count != other.children.Count)
                || ((style == null) != (other.style != null))
            )
            {
                return RenderComparison.layout;
            }

            RenderComparison result = Equals(recognizer, other.recognizer)
                ? RenderComparison.identical
                : RenderComparison.metadata;
            if (style != null)
            {
                var candidate = style.compareTo(other.style);
                if (candidate > result)
                {
                    result = candidate;
                }

                if (result == RenderComparison.layout)
                {
                    return result;
                }
            }

            if (children != null)
            {
                for (var index = 0; index < children.Count; index++)
                {
                    var candidate = children[index].compareTo(other.children[index]);
                    if (candidate > result)
                    {
                        result = candidate;
                    }
                    if (result == RenderComparison.layout)
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextSpan) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (style != null ? style.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (text != null ? text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (childHash());
                return hashCode;
            }
        }

        public bool Equals(TextSpan other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(style, other.style) && string.Equals(text, other.text) && childEquals(children, other.children);
        }
        
        public static bool operator ==(TextSpan left, TextSpan right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TextSpan left, TextSpan right)
        {
            return !Equals(left, right);
        }
        private int childHash()
        {
            unchecked
            {
                var hashCode = 0;
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        hashCode = (hashCode * 397) ^ (child != null ? child.GetHashCode() : 0);
                    }
                }

                return hashCode;
            }
        }

        private static bool childEquals(List<TextSpan> left, List<TextSpan> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.SequenceEqual(right);
        }
        
        
        public override List<DiagnosticsNode> debugDescribeChildren() {
            if (children == null)
            {
                return new List<DiagnosticsNode>();
            }

            return children.Select((child) =>
            {
                if (child != null)
                {
                    return child.toDiagnosticsNode();
                }
                else
                {
                    return DiagnosticsNode.message("<null child>");
                }
            }).ToList();
        }
    }
}
