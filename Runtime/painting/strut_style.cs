using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class StrutStyle : Diagnosticable {
        public StrutStyle(
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            float? fontSize = null,
            float? height = null,
            float? leading = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            bool forceStrutHeight = false,
            string debugLabel = null
        ) {
            D.assert(fontSize == null || fontSize > 0);
            D.assert(leading == null || leading >= 0);
            this.fontFamily = fontFamily;
            this._fontFamilyFallback = fontFamilyFallback;
            this.fontSize = fontSize;
            this.height = height;
            this.fontWeight = fontWeight;
            this.fontStyle = fontStyle;
            this.leading = leading;
            this.forceStrutHeight = forceStrutHeight;
            this.debugLabel = debugLabel;
        }

        public static StrutStyle fromTextStyle(
            TextStyle textStyle,
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            float? fontSize = null,
            float? height = null,
            float? leading = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            bool forceStrutHeight = false,
            string debugLabel = null
        ) {
            D.assert(textStyle != null);
            D.assert(fontSize == null || fontSize > 0);
            D.assert(leading == null || leading >= 0);
            return new StrutStyle(
                fontFamily: fontFamily ?? textStyle.fontFamily,
                fontFamilyFallback: fontFamilyFallback ?? textStyle.fontFamilyFallback,
                height: height ?? textStyle.height,
                fontSize: fontSize ?? textStyle.fontSize,
                fontWeight: fontWeight ?? textStyle.fontWeight,
                fontStyle: fontStyle ?? textStyle.fontStyle,
                debugLabel: debugLabel ?? textStyle.debugLabel
            );
        }

        public static readonly StrutStyle disabled = new StrutStyle(
            height: 0.0f,
            leading: 0.0f
        );

        public readonly string fontFamily;

        public List<string> fontFamilyFallback {
            get { return this._fontFamilyFallback; }
        }

        readonly List<string> _fontFamilyFallback;

        public readonly float? fontSize;
        public readonly float? height;
        public readonly FontWeight fontWeight;
        public readonly FontStyle? fontStyle;
        public readonly float? leading;
        public readonly bool forceStrutHeight;
        public readonly string debugLabel;

        public RenderComparison compareTo(StrutStyle other) {
            if (ReferenceEquals(this, other)) {
                return RenderComparison.identical;
            }

            if (other == null) {
                return RenderComparison.layout;
            }

            if (this.fontFamily != other.fontFamily ||
                this.fontSize != other.fontSize ||
                this.fontWeight != other.fontWeight ||
                this.fontStyle != other.fontStyle ||
                this.height != other.height ||
                this.leading != other.leading ||
                this.forceStrutHeight != other.forceStrutHeight ||
                !CollectionUtils.equalsList(this.fontFamilyFallback, other.fontFamilyFallback)) {
                return RenderComparison.layout;
            }

            return RenderComparison.identical;
        }

        public StrutStyle inheritFromTextStyle(TextStyle other) {
            if (other == null) {
                return this;
            }

            return new StrutStyle(
                fontFamily: this.fontFamily ?? other.fontFamily,
                fontFamilyFallback: this.fontFamilyFallback ?? other.fontFamilyFallback,
                height: this.height ?? other.height,
                leading: this.leading,
                fontSize: this.fontSize ?? other.fontSize,
                fontWeight: this.fontWeight ?? other.fontWeight,
                fontStyle: this.fontStyle ?? other.fontStyle,
                forceStrutHeight: this.forceStrutHeight,
                debugLabel: this.debugLabel ?? other.debugLabel
            );
        }

        public bool Equals(StrutStyle other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.fontFamily == other.fontFamily &&
                   this.fontSize == other.fontSize &&
                   this.fontWeight == other.fontWeight &&
                   this.fontStyle == other.fontStyle &&
                   this.height == other.height &&
                   this.leading == other.leading &&
                   this.forceStrutHeight == other.forceStrutHeight;
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

            return this.Equals((StrutStyle) obj);
        }

        public static bool operator ==(StrutStyle left, StrutStyle right) {
            return Equals(left, right);
        }

        public static bool operator !=(StrutStyle left, StrutStyle right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.fontFamily?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (this.fontSize?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.fontWeight?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.fontStyle?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.height?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.leading?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ this.forceStrutHeight.GetHashCode();
                return hashCode;
            }
        }

        public override string toStringShort() {
            return $"{this.GetType()}";
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            if (this.debugLabel != null) {
                properties.add(new MessageProperty("debugLabel", this.debugLabel));
            }

            List<DiagnosticsNode> styles = new List<DiagnosticsNode>();
            styles.Add(new StringProperty("family", this.fontFamily, defaultValue: Diagnostics.kNullDefaultValue,
                quoted: false));
            styles.Add(new EnumerableProperty<string>("familyFallback", this.fontFamilyFallback));
            styles.Add(new DiagnosticsProperty<float?>("size", this.fontSize,
                defaultValue: Diagnostics.kNullDefaultValue));
            string weightDescription = "";
            if (this.fontWeight != null) {
                weightDescription = this.fontWeight.weightValue.ToString();
            }

            styles.Add(new DiagnosticsProperty<FontWeight>(
                "weight", this.fontWeight,
                description: weightDescription,
                defaultValue: Diagnostics.kNullDefaultValue
            ));
            styles.Add(new EnumProperty<FontStyle?>("style", this.fontStyle,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<float?>("height", this.height,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new FlagProperty("forceStrutHeight", value: this.forceStrutHeight,
                defaultValue: Diagnostics.kNullDefaultValue));

            bool styleSpecified = styles.Any((DiagnosticsNode n) => !n.isFiltered(DiagnosticLevel.info));
            foreach (var style in styles) {
                properties.add(style);
            }

            if (!styleSpecified) {
                properties.add(new FlagProperty("forceStrutHeight", value: this.forceStrutHeight,
                    ifTrue: "<strut height forced>",
                    ifFalse: "<strut height normal>"));
            }
        }
    }
}