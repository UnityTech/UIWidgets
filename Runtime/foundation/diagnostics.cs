using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.foundation {
    public enum DiagnosticLevel {
        hidden,
        fine,
        debug,
        info,
        warning,
        error,
        off,
    }

    public enum DiagnosticsTreeStyle {
        sparse,
        offstage,
        dense,
        transition,
        whitespace,
        singleLine,
    }

    public class TextTreeConfiguration {
        public TextTreeConfiguration(
            string prefixLineOne = null,
            string prefixOtherLines = null,
            string prefixLastChildLineOne = null,
            string prefixOtherLinesRootNode = null,
            string linkCharacter = null,
            string propertyPrefixIfChildren = null,
            string propertyPrefixNoChildren = null,
            string lineBreak = "\n",
            bool lineBreakProperties = true,
            string afterName = ":",
            string afterDescriptionIfBody = "",
            string beforeProperties = "",
            string afterProperties = "",
            string propertySeparator = "",
            string bodyIndent = "",
            string footer = "",
            bool showChildren = true,
            bool addBlankLineIfNoChildren = true,
            bool isNameOnOwnLine = false,
            bool isBlankLineBetweenPropertiesAndChildren = true
        ) {
            D.assert(prefixLineOne != null);
            D.assert(prefixOtherLines != null);
            D.assert(prefixLastChildLineOne != null);
            D.assert(prefixOtherLinesRootNode != null);
            D.assert(linkCharacter != null);
            D.assert(propertyPrefixIfChildren != null);
            D.assert(propertyPrefixNoChildren != null);
            D.assert(lineBreak != null);
            D.assert(afterName != null);
            D.assert(afterDescriptionIfBody != null);
            D.assert(beforeProperties != null);
            D.assert(afterProperties != null);
            D.assert(propertySeparator != null);
            D.assert(bodyIndent != null);
            D.assert(footer != null);

            this.prefixLineOne = prefixLineOne;
            this.prefixOtherLines = prefixOtherLines;
            this.prefixLastChildLineOne = prefixLastChildLineOne;
            this.prefixOtherLinesRootNode = prefixOtherLinesRootNode;
            this.propertyPrefixIfChildren = propertyPrefixIfChildren;
            this.propertyPrefixNoChildren = propertyPrefixNoChildren;
            this.linkCharacter = linkCharacter;
            this.childLinkSpace = new string(' ', linkCharacter.Length);
            this.lineBreak = lineBreak;
            this.lineBreakProperties = lineBreakProperties;
            this.afterName = afterName;
            this.afterDescriptionIfBody = afterDescriptionIfBody;
            this.beforeProperties = beforeProperties;
            this.afterProperties = afterProperties;
            this.propertySeparator = propertySeparator;
            this.bodyIndent = bodyIndent;
            this.showChildren = showChildren;
            this.addBlankLineIfNoChildren = addBlankLineIfNoChildren;
            this.isNameOnOwnLine = isNameOnOwnLine;
            this.footer = footer;
            this.isBlankLineBetweenPropertiesAndChildren = isBlankLineBetweenPropertiesAndChildren;
        }

        public readonly string prefixLineOne;

        public readonly string prefixOtherLines;

        public readonly string prefixLastChildLineOne;

        public readonly string prefixOtherLinesRootNode;

        public readonly string propertyPrefixIfChildren;

        public readonly string propertyPrefixNoChildren;

        public readonly string linkCharacter;

        public readonly string childLinkSpace;

        public readonly string lineBreak;

        public readonly bool lineBreakProperties;

        public readonly string afterName;

        public readonly string afterDescriptionIfBody;

        public readonly string beforeProperties;

        public readonly string afterProperties;

        public readonly string propertySeparator;

        public readonly string bodyIndent;

        public readonly bool showChildren;

        public readonly bool addBlankLineIfNoChildren;

        public readonly bool isNameOnOwnLine;

        public readonly string footer;

        public readonly bool isBlankLineBetweenPropertiesAndChildren;
    }

    class _PrefixedstringBuilder {
        internal _PrefixedstringBuilder(string prefixLineOne, string prefixOtherLines) {
            this.prefixLineOne = prefixLineOne;
            this.prefixOtherLines = prefixOtherLines;
        }

        public readonly string prefixLineOne;

        public string prefixOtherLines;

        readonly StringBuilder _buffer = new StringBuilder();
        bool _atLineStart = true;
        bool _hasMultipleLines = false;

        public bool hasMultipleLines {
            get { return this._hasMultipleLines; }
        }

        public void write(string s) {
            if (s.isEmpty()) {
                return;
            }

            if (s == "\n") {
                if (this._buffer.Length == 0) {
                    this._buffer.Append(this.prefixLineOne.TrimEnd());
                }
                else if (this._atLineStart) {
                    this._buffer.Append(this.prefixOtherLines.TrimEnd());
                    this._hasMultipleLines = true;
                }

                this._buffer.Append("\n");
                this._atLineStart = true;
                return;
            }

            if (this._buffer.Length == 0) {
                this._buffer.Append(this.prefixLineOne);
            }
            else if (this._atLineStart) {
                this._buffer.Append(this.prefixOtherLines);
                this._hasMultipleLines = true;
            }

            bool lineTerminated = false;

            if (s.EndsWith("\n")) {
                s = s.Substring(0, s.Length - 1);
                lineTerminated = true;
            }

            var parts = s.Split('\n');
            this._buffer.Append(parts[0]);
            for (int i = 1; i < parts.Length; ++i) {
                this._buffer.Append("\n")
                    .Append(this.prefixOtherLines)
                    .Append(parts[i]);
            }

            if (lineTerminated) {
                this._buffer.Append("\n");
            }

            this._atLineStart = lineTerminated;
        }

        public void writeRaw(string text) {
            if (text.isEmpty()) {
                return;
            }

            this._buffer.Append(text);
            this._atLineStart = text.EndsWith("\n");
        }


        public void writeRawLine(string line) {
            if (line.isEmpty()) {
                return;
            }

            this._buffer.Append(line);
            if (!line.EndsWith("\n")) {
                this._buffer.Append('\n');
            }

            this._atLineStart = true;
        }

        public override string ToString() {
            return this._buffer.ToString();
        }
    }

    class _NoDefaultValue {
        internal _NoDefaultValue() {
        }
    }

    class _NullDefaultValue {
        internal _NullDefaultValue() {
        }
    }

    public abstract class DiagnosticsNode {
        protected DiagnosticsNode(
            string name = null,
            DiagnosticsTreeStyle? style = null,
            bool showName = true,
            bool showSeparator = true
        ) {
            D.assert(name == null || !name.EndsWith(":"),
                () => "Names of diagnostic nodes must not end with colons.");
            this.name = name;
            this._style = style;
            this._showName = showName;
            this.showSeparator = showSeparator;
        }

        public static DiagnosticsNode message(
            string message,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) {
            return new DiagnosticsProperty<object>(
                "",
                null,
                description: message,
                style: style,
                showName: false,
                level: level
            );
        }

        public readonly string name;

        public abstract string toDescription(
            TextTreeConfiguration parentConfiguration = null
        );

        public readonly bool showSeparator;

        public bool isFiltered(DiagnosticLevel minLevel) {
            return this.level < minLevel;
        }

        public virtual DiagnosticLevel level {
            get { return DiagnosticLevel.info; }
        }

        public virtual bool showName {
            get { return this._showName; }
        }

        readonly bool _showName;

        public virtual string emptyBodyDescription {
            get { return null; }
        }

        public abstract object valueObject { get; }

        public virtual DiagnosticsTreeStyle? style {
            get { return this._style; }
        }

        readonly DiagnosticsTreeStyle? _style;

        public abstract List<DiagnosticsNode> getProperties();

        public abstract List<DiagnosticsNode> getChildren();

        string _separator {
            get { return this.showSeparator ? ":" : ""; }
        }

        public virtual Dictionary<string, object> toJsonMap() {
            var data = new Dictionary<string, object> {
                {"name", this.name},
                {"showSeparator", this.showSeparator},
                {"description", this.toDescription()},
                {"level", Convert.ToString(this.level)},
                {"showName", this.showName},
                {"emptyBodyDescription", this.emptyBodyDescription},
                {"style", Convert.ToString(this.style)},
                {"valueToString", Convert.ToString(this.valueObject)},
                {"type", this.GetType().ToString()},
                {"hasChildren", this.getChildren().isNotEmpty()}
            };
            return data;
        }

        public override string ToString() {
            return this.toString();
        }

        public virtual string toString(
            TextTreeConfiguration parentConfiguration = null,
            DiagnosticLevel minLevel = DiagnosticLevel.info
        ) {
            D.assert(this.style != null);
            if (this.style == DiagnosticsTreeStyle.singleLine) {
                return this.toStringDeep(parentConfiguration: parentConfiguration, minLevel: minLevel);
            }

            var description = this.toDescription(parentConfiguration: parentConfiguration);
            if (this.name.isEmpty() || !this.showName) {
                return description;
            }

            return description.Contains("\n")
                ? this.name + this._separator + "\n" + description
                : this.name + this._separator + description;
        }

        protected TextTreeConfiguration textTreeConfiguration {
            get {
                D.assert(this.style != null);
                switch (this.style) {
                    case DiagnosticsTreeStyle.dense:
                        return Diagnostics.denseTextConfiguration;
                    case DiagnosticsTreeStyle.sparse:
                        return Diagnostics.sparseTextConfiguration;
                    case DiagnosticsTreeStyle.offstage:
                        return Diagnostics.dashedTextConfiguration;
                    case DiagnosticsTreeStyle.whitespace:
                        return Diagnostics.whitespaceTextConfiguration;
                    case DiagnosticsTreeStyle.transition:
                        return Diagnostics.transitionTextConfiguration;
                    case DiagnosticsTreeStyle.singleLine:
                        return Diagnostics.singleLineTextConfiguration;
                }

                return null;
            }
        }

        TextTreeConfiguration _childTextConfiguration(
            DiagnosticsNode child,
            TextTreeConfiguration textStyle
        ) {
            return child != null && child.style != DiagnosticsTreeStyle.singleLine
                ? child.textTreeConfiguration
                : textStyle;
        }

        public string toStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = null,
            TextTreeConfiguration parentConfiguration = null,
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            prefixOtherLines = prefixOtherLines ?? prefixLineOne;

            var children = this.getChildren();
            var config = this.textTreeConfiguration;
            if (prefixOtherLines.isEmpty()) {
                prefixOtherLines += config.prefixOtherLinesRootNode;
            }

            var builder = new _PrefixedstringBuilder(
                prefixLineOne,
                prefixOtherLines
            );

            var description = this.toDescription(parentConfiguration: parentConfiguration);
            if (description.isEmpty()) {
                if (this.name.isNotEmpty() && this.showName) {
                    builder.write(this.name);
                }
            }
            else {
                if (this.name.isNotEmpty() && this.showName) {
                    builder.write(this.name);
                    if (this.showSeparator) {
                        builder.write(config.afterName);
                    }

                    builder.write(
                        config.isNameOnOwnLine || description.Contains("\n") ? "\n" : "");
                    if (description.Contains("\n") && this.style == DiagnosticsTreeStyle.singleLine) {
                        builder.prefixOtherLines += "  ";
                    }
                }

                builder.prefixOtherLines +=
                    children.isEmpty() ? config.propertyPrefixNoChildren : config.propertyPrefixIfChildren;
                builder.write(description);
            }

            var properties = this.getProperties().Where(n => !n.isFiltered(minLevel)).ToList();

            if (properties.isNotEmpty() || children.isNotEmpty() || this.emptyBodyDescription != null) {
                builder.write(config.afterDescriptionIfBody);
            }

            if (config.lineBreakProperties) {
                builder.write(config.lineBreak);
            }

            if (properties.isNotEmpty()) {
                builder.write(config.beforeProperties);
            }

            builder.prefixOtherLines += config.bodyIndent;
            if (this.emptyBodyDescription != null &&
                properties.isEmpty() &&
                children.isEmpty() &&
                prefixLineOne.isNotEmpty()) {
                builder.write(this.emptyBodyDescription);
                if (config.lineBreakProperties) {
                    builder.write(config.lineBreak);
                }
            }

            for (int i = 0; i < properties.Count; ++i) {
                DiagnosticsNode property = properties[i];
                if (i > 0) {
                    builder.write(config.propertySeparator);
                }

                const int kWrapWidth = 65;
                if (property.style != DiagnosticsTreeStyle.singleLine) {
                    TextTreeConfiguration propertyStyle = property.textTreeConfiguration;
                    builder.writeRaw(property.toStringDeep(
                        prefixLineOne: builder.prefixOtherLines + propertyStyle.prefixLineOne,
                        prefixOtherLines: builder.prefixOtherLines + propertyStyle.linkCharacter +
                                          propertyStyle.prefixOtherLines,
                        parentConfiguration: config,
                        minLevel: minLevel
                    ));
                    continue;
                }

                D.assert(property.style == DiagnosticsTreeStyle.singleLine);
                string message = property.toString(parentConfiguration: config, minLevel: minLevel);
                if (!config.lineBreakProperties || message.Length < kWrapWidth) {
                    builder.write(message);
                }
                else {
                    var lines = message.Split('\n');
                    for (int j = 0; j < lines.Length; ++j) {
                        string line = lines[j];
                        if (j > 0) {
                            builder.write(config.lineBreak);
                        }

                        builder.write(string.Join("\n",
                            DebugPrint.debugWordWrap(line, kWrapWidth, wrapIndent: "  ").ToArray()));
                    }
                }

                if (config.lineBreakProperties) {
                    builder.write(config.lineBreak);
                }
            }

            if (properties.isNotEmpty()) {
                builder.write(config.afterProperties);
            }

            if (!config.lineBreakProperties) {
                builder.write(config.lineBreak);
            }

            var prefixChildren = prefixOtherLines + config.bodyIndent;
            if (children.isEmpty() &&
                config.addBlankLineIfNoChildren &&
                builder.hasMultipleLines) {
                string prefix = prefixChildren.TrimEnd();
                if (prefix.isNotEmpty()) {
                    builder.writeRaw(prefix + config.lineBreak);
                }
            }

            if (children.isNotEmpty() && config.showChildren) {
                if (config.isBlankLineBetweenPropertiesAndChildren &&
                    properties.isNotEmpty() &&
                    children.First().textTreeConfiguration.isBlankLineBetweenPropertiesAndChildren) {
                    builder.write(config.lineBreak);
                }

                for (int i = 0; i < children.Count; i++) {
                    DiagnosticsNode child = children[i];
                    D.assert(child != null);
                    TextTreeConfiguration childConfig = this._childTextConfiguration(child, config);
                    if (i == children.Count - 1) {
                        string lastChildPrefixLineOne = prefixChildren + childConfig.prefixLastChildLineOne;
                        builder.writeRawLine(child.toStringDeep(
                            prefixLineOne: lastChildPrefixLineOne,
                            prefixOtherLines: prefixChildren + childConfig.childLinkSpace +
                                              childConfig.prefixOtherLines,
                            parentConfiguration: config,
                            minLevel: minLevel
                        ));
                        if (childConfig.footer.isNotEmpty()) {
                            builder.writeRaw(prefixChildren + childConfig.childLinkSpace + childConfig.footer);
                        }
                    }
                    else {
                        TextTreeConfiguration nextChildStyle = this._childTextConfiguration(children[i + 1], config);
                        string childPrefixLineOne = prefixChildren + childConfig.prefixLineOne;
                        string childPrefixOtherLines =
                            prefixChildren + nextChildStyle.linkCharacter + childConfig.prefixOtherLines;
                        builder.writeRawLine(child.toStringDeep(
                            prefixLineOne: childPrefixLineOne,
                            prefixOtherLines: childPrefixOtherLines,
                            parentConfiguration: config,
                            minLevel: minLevel
                        ));
                        if (childConfig.footer.isNotEmpty()) {
                            builder.writeRaw(prefixChildren + nextChildStyle.linkCharacter + childConfig.footer);
                        }
                    }
                }
            }

            return builder.ToString();
        }
    }

    public class MessageProperty : DiagnosticsProperty<object> {
        public MessageProperty(string name, string message,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name, null, description: message, level: level) {
            D.assert(name != null);
            D.assert(message != null);
        }
    }

    public class StringProperty : DiagnosticsProperty<string> {
        public StringProperty(string name, string value,
            string description = null,
            string tooltip = null,
            bool showName = true,
            object defaultValue = null,
            bool quoted = true,
            string ifEmpty = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name,
            value,
            description: description,
            defaultValue: defaultValue,
            tooltip: tooltip,
            showName: showName,
            ifEmpty: ifEmpty,
            level: level) {
            this.quoted = quoted;
        }

        public readonly bool quoted;

        public override Dictionary<string, object> toJsonMap() {
            var json = base.toJsonMap();
            json["quoted"] = this.quoted;
            return json;
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            string text = this._description ?? this.value;
            if (parentConfiguration != null &&
                !parentConfiguration.lineBreakProperties &&
                text != null) {
                text = text.Replace("\n", "\\n");
            }

            if (this.quoted && text != null) {
                if (this.ifEmpty != null && text.isEmpty()) {
                    return this.ifEmpty;
                }

                return "\"" + text + "\"";
            }

            return text ?? "null";
        }
    }

    public abstract class _NumProperty<T> : DiagnosticsProperty<T> {
        internal _NumProperty(string name,
            T value,
            string ifNull = null,
            string unit = null,
            bool showName = true,
            object defaultValue = null,
            string tooltip = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifNull: ifNull,
            showName: showName,
            defaultValue: defaultValue,
            tooltip: tooltip,
            level: level
        ) {
            this.unit = unit;
        }

        internal _NumProperty(string name,
            ComputePropertyValueCallback<T> computeValue,
            string ifNull = null,
            string unit = null,
            bool showName = true,
            object defaultValue = null,
            string tooltip = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            computeValue,
            ifNull: ifNull,
            showName: showName,
            defaultValue: defaultValue,
            tooltip: tooltip,
            level: level
        ) {
            this.unit = unit;
        }

        public override Dictionary<string, object> toJsonMap() {
            var json = base.toJsonMap();
            if (this.unit != null) {
                json["unit"] = this.unit;
            }

            json["numberToString"] = this.numberToString();
            return json;
        }

        public readonly string unit;

        protected abstract string numberToString();

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (this.value == null) {
                return "null";
            }

            return this.unit != null ? this.numberToString() + this.unit : this.numberToString();
        }
    }

    public class IntProperty : _NumProperty<int?> {
        public IntProperty(string name, int? value,
            string ifNull = null,
            bool showName = true,
            string unit = null,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifNull: ifNull,
            showName: showName,
            unit: unit,
            defaultValue: defaultValue,
            level: level
        ) {
        }

        protected override string numberToString() {
            if (this.value == null) {
                return "null";
            }

            return this.value.Value.ToString();
        }
    }

    public class FloatProperty : _NumProperty<float?> {
        public FloatProperty(string name, float? value,
            string ifNull = null,
            string unit = null,
            string tooltip = null,
            object defaultValue = null,
            bool showName = true,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifNull: ifNull,
            unit: unit,
            tooltip: tooltip,
            defaultValue: defaultValue,
            showName: showName,
            level: level
        ) {
        }

        FloatProperty(
            string name,
            ComputePropertyValueCallback<float?> computeValue,
            string ifNull = null,
            bool showName = true,
            string unit = null,
            string tooltip = null,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            computeValue,
            showName: showName,
            ifNull: ifNull,
            unit: unit,
            tooltip: tooltip,
            defaultValue: defaultValue,
            level: level
        ) {
        }

        public static FloatProperty lazy(
            string name,
            ComputePropertyValueCallback<float?> computeValue,
            string ifNull = null,
            bool showName = true,
            string unit = null,
            string tooltip = null,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) {
            return new FloatProperty(
                name,
                computeValue,
                showName: showName,
                ifNull: ifNull,
                unit: unit,
                tooltip: tooltip,
                defaultValue: defaultValue,
                level: level
            );
        }

        protected override string numberToString() {
            if (this.value != null) {
                return this.value.Value.ToString("F1");
            }

            return "null";
        }
    }

    public class PercentProperty : FloatProperty {
        public PercentProperty(string name, float fraction,
            string ifNull = null,
            bool showName = true,
            string tooltip = null,
            string unit = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            fraction,
            ifNull: ifNull,
            showName: showName,
            tooltip: tooltip,
            unit: unit,
            level: level
        ) {
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (this.value == null) {
                return "null";
            }

            return this.unit != null ? this.numberToString() + " " + this.unit : this.numberToString();
        }

        protected override string numberToString() {
            if (this.value == null) {
                return "null";
            }

            return (this.value.Value.clamp(0.0f, 1.0f) * 100).ToString("F1") + "%";
        }
    }

    public class FlagProperty : DiagnosticsProperty<bool?> {
        public FlagProperty(string name,
            bool? value,
            string ifTrue = null,
            string ifFalse = null,
            bool showName = false,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name,
            value,
            showName: showName,
            defaultValue: defaultValue,
            level: level
        ) {
            D.assert(ifTrue != null || ifFalse != null);
        }

        public override Dictionary<string, object> toJsonMap() {
            var json = base.toJsonMap();
            if (this.ifTrue != null) {
                json["ifTrue"] = this.ifTrue;
            }

            if (this.ifFalse != null) {
                json["ifFalse"] = this.ifFalse;
            }

            return json;
        }

        public readonly string ifTrue;

        public readonly string ifFalse;

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (this.value == true) {
                if (this.ifTrue != null) {
                    return this.ifTrue;
                }
            }
            else if (this.value == false) {
                if (this.ifFalse != null) {
                    return this.ifFalse;
                }
            }

            return base.valueToString(parentConfiguration: parentConfiguration);
        }

        public override bool showName {
            get {
                if (this.value == null || this.value == true && this.ifTrue == null ||
                    this.value == false && this.ifFalse == null) {
                    return true;
                }

                return base.showName;
            }
        }

        public override DiagnosticLevel level {
            get {
                if (this.value == true) {
                    if (this.ifTrue == null) {
                        return DiagnosticLevel.hidden;
                    }
                }

                if (this.value == false) {
                    if (this.ifFalse == null) {
                        return DiagnosticLevel.hidden;
                    }
                }

                return base.level;
            }
        }
    }

    public class EnumerableProperty<T> : DiagnosticsProperty<IEnumerable<T>> {
        public EnumerableProperty(
            string name,
            IEnumerable<T> value,
            object defaultValue = null,
            string ifNull = null,
            string ifEmpty = "[]",
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            bool showName = true,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            defaultValue: defaultValue,
            ifNull: ifNull,
            ifEmpty: ifEmpty,
            style: style,
            showName: showName,
            level: level
        ) {
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (this.value == null) {
                return "null";
            }

            if (!this.value.Any()) {
                return this.ifEmpty ?? "[]";
            }

            if (parentConfiguration != null && !parentConfiguration.lineBreakProperties) {
                return string.Join(", ", this.value.Select(v => v.ToString()).ToArray());
            }

            return string.Join(this.style == DiagnosticsTreeStyle.singleLine ? ", " : "\n",
                this.value.Select(v => v.ToString()).ToArray());
        }

        public override DiagnosticLevel level {
            get {
                if (this.ifEmpty == null &&
                    this.value != null && !this.value.Any()
                    && base.level != DiagnosticLevel.hidden) {
                    return DiagnosticLevel.fine;
                }

                return base.level;
            }
        }

        public override Dictionary<string, object> toJsonMap() {
            var json = base.toJsonMap();
            if (this.value != null) {
                json["values"] = this.value.Select(v => v.ToString()).ToList();
            }

            return json;
        }
    }


    public class EnumProperty<T> : DiagnosticsProperty<T> {
        public EnumProperty(string name, T value,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            defaultValue: defaultValue,
            level: level
        ) {
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (this.value == null) {
                return "null";
            }

            return this.value.ToString();
        }
    }

    public class ObjectFlagProperty<T> : DiagnosticsProperty<T> {
        public ObjectFlagProperty(string name, T value,
            string ifPresent = null,
            string ifNull = null,
            bool showName = false,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            showName: showName,
            ifNull: ifNull,
            level: level
        ) {
            D.assert(ifPresent != null || ifNull != null);
        }

        ObjectFlagProperty(
            string name,
            T value,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            showName: false,
            level: level
        ) {
            D.assert(name != null);
            this.ifPresent = "has " + name;
        }

        public static ObjectFlagProperty<T> has(
            string name,
            T value,
            DiagnosticLevel level = DiagnosticLevel.info
        ) {
            return new ObjectFlagProperty<T>(name, value, level);
        }

        public readonly string ifPresent;

        protected override string valueToString(
            TextTreeConfiguration parentConfiguration = null) {
            if (this.value != null) {
                if (this.ifPresent != null) {
                    return this.ifPresent;
                }
            }
            else {
                if (this.ifNull != null) {
                    return this.ifNull;
                }
            }

            return base.valueToString(parentConfiguration: parentConfiguration);
        }

        public override bool showName {
            get {
                if ((this.value != null && this.ifPresent == null) || (this.value == null && this.ifNull == null)) {
                    return true;
                }

                return base.showName;
            }
        }

        public override DiagnosticLevel level {
            get {
                if (this.value != null) {
                    if (this.ifPresent == null) {
                        return DiagnosticLevel.hidden;
                    }
                }
                else {
                    if (this.ifNull == null) {
                        return DiagnosticLevel.hidden;
                    }
                }

                return base.level;
            }
        }

        public override Dictionary<string, object> toJsonMap() {
            var json = base.toJsonMap();
            if (this.ifPresent != null) {
                json["ifPresent"] = this.ifPresent;
            }

            return json;
        }
    }

    public delegate T ComputePropertyValueCallback<T>();

    public class DiagnosticsProperty<T> : DiagnosticsNode {
        public DiagnosticsProperty(
            string name,
            T value,
            string description = null,
            string ifNull = null,
            string ifEmpty = null,
            bool showName = true,
            bool showSeparator = true,
            object defaultValue = null,
            string tooltip = null,
            bool missingIfNull = false,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name: name,
            showName: showName,
            showSeparator: showSeparator,
            style: style
        ) {
            defaultValue = defaultValue ?? Diagnostics.kNoDefaultValue;
            if (defaultValue == Diagnostics.kNullDefaultValue) {
                defaultValue = null;
            }

            D.assert(defaultValue == null || defaultValue == Diagnostics.kNoDefaultValue || defaultValue is T);
            this._description = description;
            this._valueComputed = true;
            this._value = value;
            this._computeValue = null;
            this.ifNull = ifNull ?? (missingIfNull ? "MISSING" : null);
            this._defaultLevel = level;
            this.ifEmpty = ifEmpty;
            this.defaultValue = defaultValue;
            this.tooltip = tooltip;
            this.missingIfNull = missingIfNull;
        }

        internal DiagnosticsProperty(
            string name,
            ComputePropertyValueCallback<T> computeValue,
            string description = null,
            string ifNull = null,
            string ifEmpty = null,
            bool showName = true,
            bool showSeparator = true,
            object defaultValue = null,
            string tooltip = null,
            bool missingIfNull = false,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name: name,
            showName: showName,
            showSeparator: showSeparator,
            style: style
        ) {
            defaultValue = defaultValue ?? Diagnostics.kNoDefaultValue;
            if (defaultValue == Diagnostics.kNullDefaultValue) {
                defaultValue = null;
            }

            D.assert(defaultValue == null || defaultValue == Diagnostics.kNoDefaultValue || defaultValue is T);
            this._description = description;
            this._valueComputed = false;
            this._value = default(T);
            this._computeValue = computeValue;
            this._defaultLevel = level;
            this.ifNull = ifNull ?? (missingIfNull ? "MISSING" : null);
            this.ifEmpty = ifEmpty;
            this.defaultValue = defaultValue;
            this.tooltip = tooltip;
            this.missingIfNull = missingIfNull;
        }

        public static DiagnosticsProperty<T> lazy(
            string name,
            ComputePropertyValueCallback<T> computeValue,
            string description = null,
            string ifNull = null,
            string ifEmpty = null,
            bool showName = true,
            bool showSeparator = true,
            object defaultValue = null,
            string tooltip = null,
            bool missingIfNull = false,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) {
            return new DiagnosticsProperty<T>(
                name,
                computeValue,
                description,
                ifNull,
                ifEmpty,
                showName,
                showSeparator,
                defaultValue,
                tooltip,
                missingIfNull,
                style,
                level);
        }

        internal readonly string _description;

        public override Dictionary<string, object> toJsonMap() {
            var json = base.toJsonMap();
            if (this.defaultValue != Diagnostics.kNoDefaultValue) {
                json["defaultValue"] = Convert.ToString(this.defaultValue);
            }

            if (this.ifEmpty != null) {
                json["ifEmpty"] = this.ifEmpty;
            }

            if (this.ifNull != null) {
                json["ifNull"] = this.ifNull;
            }

            if (this.tooltip != null) {
                json["tooltip"] = this.tooltip;
            }

            json["missingIfNull"] = this.missingIfNull;
            if (this.exception != null) {
                json["exception"] = this.exception.ToString();
            }

            json["propertyType"] = this.propertyType.ToString();
            json["valueToString"] = this.valueToString();
            json["defaultLevel"] = Convert.ToString(this._defaultLevel);
            if (typeof(Diagnosticable).IsAssignableFrom(typeof(T))) {
                json["isDiagnosticableValue"] = true;
            }

            return json;
        }

        protected virtual string valueToString(
            TextTreeConfiguration parentConfiguration = null
        ) {
            var v = this.value;
            var tree = v as DiagnosticableTree;
            return tree != null ? tree.toStringShort() : v != null ? v.ToString() : "null";
        }

        public override string toDescription(
            TextTreeConfiguration parentConfiguration = null
        ) {
            if (this._description != null) {
                return this._addTooltip(this._description);
            }

            if (this.exception != null) {
                return "EXCEPTION (" + this.exception.GetType() + ")";
            }

            if (this.ifNull != null && this.value == null) {
                return this._addTooltip(this.ifNull);
            }

            string result = this.valueToString(parentConfiguration: parentConfiguration);
            if (result.isEmpty() && this.ifEmpty != null) {
                result = this.ifEmpty;
            }

            return this._addTooltip(result);
        }

        string _addTooltip(string text) {
            D.assert(text != null);
            return this.tooltip == null ? text : text + "(" + this.tooltip + ")";
        }

        public readonly string ifNull;
        public readonly string ifEmpty;
        public readonly string tooltip;
        public readonly bool missingIfNull;

        public Type propertyType {
            get { return typeof(T); }
        }

        public override object valueObject {
            get { return this.value; }
        }

        public T value {
            get {
                this._maybeCacheValue();
                return this._value;
            }
        }

        T _value;
        bool _valueComputed;
        Exception _exception;

        public Exception exception {
            get {
                this._maybeCacheValue();
                return this._exception;
            }
        }

        void _maybeCacheValue() {
            if (this._valueComputed) {
                return;
            }

            this._valueComputed = true;
            try {
                this._value = this._computeValue();
            }
            catch (Exception ex) {
                this._exception = ex;
                this._value = default(T);
            }
        }

        public readonly object defaultValue;
        DiagnosticLevel _defaultLevel;

        public override DiagnosticLevel level {
            get {
                if (this._defaultLevel == DiagnosticLevel.hidden) {
                    return this._defaultLevel;
                }

                if (this.exception != null) {
                    return DiagnosticLevel.error;
                }

                if (this.value == null && this.missingIfNull) {
                    return DiagnosticLevel.warning;
                }

                if (this.defaultValue != Diagnostics.kNoDefaultValue && Equals(this.value, this.defaultValue)) {
                    return DiagnosticLevel.fine;
                }

                return this._defaultLevel;
            }
        }

        readonly ComputePropertyValueCallback<T> _computeValue;

        public override List<DiagnosticsNode> getProperties() {
            return new List<DiagnosticsNode>();
        }

        public override List<DiagnosticsNode> getChildren() {
            return new List<DiagnosticsNode>();
        }
    }

    public class DiagnosticableNode<T> : DiagnosticsNode where T : Diagnosticable {
        public DiagnosticableNode(
            string name = null,
            T value = null,
            DiagnosticsTreeStyle? style = null
        ) : base(name: name, style: style) {
            D.assert(value != null);
            this._value = value;
        }

        public override object valueObject {
            get { return this.value; }
        }

        public T value {
            get { return this._value; }
        }

        readonly T _value;
        DiagnosticPropertiesBuilder _cachedBuilder;

        DiagnosticPropertiesBuilder _builder {
            get {
                if (this._cachedBuilder == null) {
                    this._cachedBuilder = new DiagnosticPropertiesBuilder();
                    if (this._value != null) {
                        this._value.debugFillProperties(this._cachedBuilder);
                    }
                }

                return this._cachedBuilder;
            }
        }

        public override DiagnosticsTreeStyle? style {
            get { return base.style ?? this._builder.defaultDiagnosticsTreeStyle; }
        }

        public override string emptyBodyDescription {
            get { return this._builder.emptyBodyDescription; }
        }

        public override List<DiagnosticsNode> getProperties() {
            return this._builder.properties;
        }

        public override List<DiagnosticsNode> getChildren() {
            return new List<DiagnosticsNode>();
        }

        public override string toDescription(
            TextTreeConfiguration parentConfiguration = null
        ) {
            return this._value.toStringShort();
        }
    }

    class _DiagnosticableTreeNode : DiagnosticableNode<DiagnosticableTree> {
        internal _DiagnosticableTreeNode(
            string name,
            DiagnosticableTree value,
            DiagnosticsTreeStyle style
        ) : base(
            name: name,
            value: value,
            style: style
        ) {
        }

        public override List<DiagnosticsNode> getChildren() {
            if (this.value != null) {
                return this.value.debugDescribeChildren();
            }

            return new List<DiagnosticsNode>();
        }
    }

    public class DiagnosticPropertiesBuilder {
        public void add(DiagnosticsNode property) {
            this.properties.Add(property);
        }

        public readonly List<DiagnosticsNode> properties = new List<DiagnosticsNode>();
        public DiagnosticsTreeStyle defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.sparse;
        public string emptyBodyDescription;
    }

    public abstract class Diagnosticable {
        protected Diagnosticable() {
        }

        public virtual string toStringShort() {
            return Diagnostics.describeIdentity(this);
        }

        public override string ToString() {
            return this.toString();
        }

        public virtual string toString(DiagnosticLevel minLevel = DiagnosticLevel.debug) {
            string fullString = null;
            D.assert(() => {
                fullString = this.toDiagnosticsNode(style: DiagnosticsTreeStyle.singleLine)
                    .toString(minLevel: minLevel);
                return true;
            });
            return fullString ?? this.toStringShort();
        }

        public virtual DiagnosticsNode toDiagnosticsNode(
            string name = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse) {
            return new DiagnosticableNode<Diagnosticable>(
                name: name, value: this, style: style
            );
        }

        public virtual void debugFillProperties(DiagnosticPropertiesBuilder properties) {
        }
    }

    public abstract class DiagnosticableTree : Diagnosticable {
        protected DiagnosticableTree() {
        }

        public virtual string toStringShallow(
            string joiner = ", ",
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            var result = new StringBuilder();
            result.Append(this.ToString());
            result.Append(joiner);
            DiagnosticPropertiesBuilder builder = new DiagnosticPropertiesBuilder();
            this.debugFillProperties(builder);
            result.Append(string.Join(joiner,
                builder.properties.Where(n => !n.isFiltered(minLevel)).Select(n => n.ToString()).ToArray())
            );
            return result.ToString();
        }

        public virtual string toStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = null,
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            return this.toDiagnosticsNode().toStringDeep(
                prefixLineOne: prefixLineOne,
                prefixOtherLines: prefixOtherLines,
                minLevel: minLevel);
        }

        public override string toStringShort() {
            return Diagnostics.describeIdentity(this);
        }

        public override DiagnosticsNode toDiagnosticsNode(
            string name = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse) {
            return new _DiagnosticableTreeNode(
                name: name,
                value: this,
                style: style
            );
        }

        public virtual List<DiagnosticsNode> debugDescribeChildren() {
            return new List<DiagnosticsNode>();
        }
    }

    public static class Diagnostics {
        public static readonly TextTreeConfiguration sparseTextConfiguration = new TextTreeConfiguration(
            prefixLineOne: "├─",
            prefixOtherLines: " ",
            prefixLastChildLineOne: "└─",
            linkCharacter: "│",
            propertyPrefixIfChildren: "│ ",
            propertyPrefixNoChildren: "  ",
            prefixOtherLinesRootNode: " "
        );

        public static readonly TextTreeConfiguration dashedTextConfiguration = new TextTreeConfiguration(
            prefixLineOne: "╎╌",
            prefixLastChildLineOne: "└╌",
            prefixOtherLines: " ",
            linkCharacter: "╎",
            propertyPrefixIfChildren: "│ ",
            propertyPrefixNoChildren: "  ",
            prefixOtherLinesRootNode: " "
        );

        public static readonly TextTreeConfiguration denseTextConfiguration = new TextTreeConfiguration(
            propertySeparator: ", ",
            beforeProperties: "(",
            afterProperties: ")",
            lineBreakProperties: false,
            prefixLineOne: "├",
            prefixOtherLines: "",
            prefixLastChildLineOne: "└",
            linkCharacter: "│",
            propertyPrefixIfChildren: "│",
            propertyPrefixNoChildren: " ",
            prefixOtherLinesRootNode: "",
            addBlankLineIfNoChildren: false,
            isBlankLineBetweenPropertiesAndChildren: false
        );

        public static readonly TextTreeConfiguration transitionTextConfiguration = new TextTreeConfiguration(
            prefixLineOne: "╞═╦══ ",
            prefixLastChildLineOne: "╘═╦══ ",
            prefixOtherLines: " ║ ",
            footer: " ╚═══════════\n",
            linkCharacter: "│",
            propertyPrefixIfChildren: "",
            propertyPrefixNoChildren: "",
            prefixOtherLinesRootNode: "",
            afterName: " ═══",
            afterDescriptionIfBody: ":",
            bodyIndent: "  ",
            isNameOnOwnLine: true,
            addBlankLineIfNoChildren: false,
            isBlankLineBetweenPropertiesAndChildren: false
        );

        public static readonly TextTreeConfiguration whitespaceTextConfiguration = new TextTreeConfiguration(
            prefixLineOne: "",
            prefixLastChildLineOne: "",
            prefixOtherLines: " ",
            prefixOtherLinesRootNode: "  ",
            bodyIndent: "",
            propertyPrefixIfChildren: "",
            propertyPrefixNoChildren: "",
            linkCharacter: " ",
            addBlankLineIfNoChildren: false,
            afterDescriptionIfBody: ":",
            isBlankLineBetweenPropertiesAndChildren: false
        );

        public static readonly TextTreeConfiguration singleLineTextConfiguration = new TextTreeConfiguration(
            propertySeparator: ", ",
            beforeProperties: "(",
            afterProperties: ")",
            prefixLineOne: "",
            prefixOtherLines: "",
            prefixLastChildLineOne: "",
            lineBreak: "",
            lineBreakProperties: false,
            addBlankLineIfNoChildren: false,
            showChildren: false,
            propertyPrefixIfChildren: "",
            propertyPrefixNoChildren: "",
            linkCharacter: "",
            prefixOtherLinesRootNode: ""
        );

        internal static readonly _NoDefaultValue kNoDefaultValue = new _NoDefaultValue();

        public static readonly object kNullDefaultValue = new _NullDefaultValue();

        public static string shortHash(object o) {
            return (o.GetHashCode() & 0xFFFFF).ToString("X").PadLeft(5, '0');
        }

        public static string describeIdentity(object o) {
            return $"{o.GetType()}#{shortHash(o)}";
        }
    }
}