using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.service {
    public class TextRange : IEquatable<TextRange> {
        public readonly int start;
        public readonly int end;

        public static TextRange collapsed(int offset) {
            D.assert(offset >= -1);
            return new TextRange(offset, offset);
        }

        public static readonly TextRange empty = new TextRange(-1, -1);

        public TextRange(int start, int end) {
            D.assert(start >= -1);
            D.assert(end >= -1);
            this.start = start;
            this.end = end;
        }

        public bool isValid {
            get { return this.start >= 0 && this.end >= 0; }
        }

        public bool isCollapsed {
            get { return this.start == this.end; }
        }

        public bool isNormalized {
            get { return this.start <= this.end; }
        }

        public string textBefore(string text) {
            D.assert(this.isNormalized);
            return text.Substring(0, this.start);
        }

        public string textAfter(string text) {
            D.assert(this.isNormalized);
            return text.Substring(this.end);
        }

        public string textInside(string text) {
            D.assert(this.isNormalized);
            return text.Substring(this.start, this.end - this.start);
        }

        public bool Equals(TextRange other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.start == other.start && this.end == other.end;
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

            return this.Equals((TextRange) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.start * 397) ^ this.end;
            }
        }

        public static bool operator ==(TextRange left, TextRange right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextRange left, TextRange right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"TextRange Start: {this.start}, End: {this.end}";
        }
    }

    public class TextSelection : TextRange, IEquatable<TextSelection> {
        public readonly int baseOffset;
        public readonly int extentOffset;
        public readonly TextAffinity affinity;
        public readonly bool isDirectional;

        public TextSelection(int baseOffset, int extentOffset, TextAffinity affinity = TextAffinity.downstream,
            bool isDirectional = false) : base(baseOffset < extentOffset ? baseOffset : extentOffset,
            baseOffset < extentOffset ? extentOffset : baseOffset) {
            this.baseOffset = baseOffset;
            this.extentOffset = extentOffset;
            this.affinity = affinity;
            this.isDirectional = isDirectional;
        }

        public static TextSelection collapsed(int offset, TextAffinity affinity = TextAffinity.downstream) {
            return new TextSelection(offset, offset, affinity, false);
        }

        public static TextSelection fromPosition(TextPosition position) {
            return collapsed(position.offset, position.affinity);
        }

        public TextPosition basePos {
            get { return new TextPosition(offset: this.baseOffset, affinity: this.affinity); }
        }

        public TextPosition extendPos {
            get { return new TextPosition(offset: this.extentOffset, affinity: this.affinity); }
        }

        public TextPosition startPos {
            get { return new TextPosition(offset: this.start, affinity: this.affinity); }
        }

        public TextPosition endPos {
            get { return new TextPosition(offset: this.end, affinity: this.affinity); }
        }

        public bool Equals(TextSelection other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.baseOffset == other.baseOffset && this.extentOffset == other.extentOffset &&
                   this.affinity == other.affinity && this.isDirectional == other.isDirectional;
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

            return this.Equals((TextSelection) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ this.baseOffset;
                hashCode = (hashCode * 397) ^ this.extentOffset;
                hashCode = (hashCode * 397) ^ (int) this.affinity;
                hashCode = (hashCode * 397) ^ this.isDirectional.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(TextSelection left, TextSelection right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextSelection left, TextSelection right) {
            return !Equals(left, right);
        }

        public TextSelection copyWith(int? baseOffset = null, int? extentOffset = null, TextAffinity? affinity = null,
            bool? isDirectional = null) {
            return new TextSelection(
                baseOffset ?? this.baseOffset, extentOffset ?? this.extentOffset, affinity ?? this.affinity,
                isDirectional ?? this.isDirectional
            );
        }

        public override string ToString() {
            return
                $"{base.ToString()}, BaseOffset: {this.baseOffset}, ExtentOffset: {this.extentOffset}, Affinity: {this.affinity}, IsDirectional: {this.isDirectional}";
        }
    }
}