using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.debugger {
#pragma warning disable 0659
#pragma warning disable 0661
    public class DiagnosticsNode : IEquatable<DiagnosticsNode> {
        readonly Dictionary<string, object> _json; // todo use json class such as simple json
        public readonly bool isProperty;
        DiagnosticsNode _parent;

        public DiagnosticsNode(Dictionary<string, object> json, bool isProperty) {
            this._json = json;
            this.isProperty = isProperty;
        }

        public bool Equals(DiagnosticsNode other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.diagnosticRef, other.diagnosticRef);
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

            return this.Equals((DiagnosticsNode) obj);
        }

        public static bool operator ==(DiagnosticsNode left, DiagnosticsNode right) {
            return Equals(left, right);
        }

        public static bool operator !=(DiagnosticsNode left, DiagnosticsNode right) {
            return !Equals(left, right);
        }

        public DiagnosticsNode parent {
            get { return this._parent; }
            set { this._parent = value; }
        }

        public string getStringMember(string memberName) {
            object ret;
            this._json.TryGetValue(memberName, out ret);
            return ret == null ? null : ret.ToString();
        }

        public string separator {
            get { return this.showSeparator ? ":" : ""; }
        }

        public string name {
            get { return this.getStringMember("name"); }
        }

        public bool showSeparator {
            get { return this.getBoolMember("showSeparator", true); }
        }

        public string description {
            get { return this.getStringMember("description"); }
        }

        public DiagnosticLevel level {
            get { return this.getLevelMember("level", DiagnosticLevel.info); }
        }

        public bool showName {
            get { return this.getBoolMember("showName", true); }
        }

        public string emptyBodyDescription {
            get { return this.getStringMember("emptyBodyDescription"); }
        }

        public DiagnosticsTreeStyle style {
            get { return this.getStyleMember("style", DiagnosticsTreeStyle.sparse); }
        }

        public string type {
            get { return this.getStringMember("type"); }
        }

        public bool isQuoted {
            get { return this.getBoolMember("quoted", false); }
        }

        public bool hasIsQuoted {
            get { return this._json.ContainsKey("quoted"); }
        }

        public string unit {
            get { return this.getStringMember("unit"); }
        }

        public bool hasUnit {
            get { return this._json.ContainsKey("unit"); }
        }

        public string numberToString {
            get { return this.getStringMember("numberToString"); }
        }

        public bool hasNumberToString {
            get { return this._json.ContainsKey("numberToString"); }
        }

        public string ifTrue {
            get { return this.getStringMember("ifTrue"); }
        }

        public bool hasIfTrue {
            get { return this._json.ContainsKey("ifTrue"); }
        }

        public string ifFalse {
            get { return this.getStringMember("ifFalse"); }
        }

        public bool hasIfFalse {
            get { return this._json.ContainsKey("ifFalse"); }
        }

        public List<string> values {
            get {
                object value;
                this._json.TryGetValue("values", out value);
                if (value == null) {
                    return null;
                }

                return ((IEnumerable) value).Cast<object>().Select(o => o.ToString()).ToList();
            }
        }

        public bool hasValues {
            get { return this._json.ContainsKey("values"); }
        }

        public string ifPresent {
            get { return this.getStringMember("ifPresent"); }
        }

        public bool hasIfPresent {
            get { return this._json.ContainsKey("ifPresent"); }
        }

        public string defaultValue {
            get { return this.getStringMember("defaultValue"); }
        }

        public bool hasDefaultValue {
            get { return this._json.ContainsKey("defaultValue"); }
        }

        public string ifEmpty {
            get { return this.getStringMember("ifEmpty"); }
        }

        public string ifNull {
            get { return this.getStringMember("ifNull"); }
        }

        public string tooltip {
            get { return this.getStringMember("tooltip"); }
        }

        public bool hasTooltip {
            get { return this._json.ContainsKey("tooltip"); }
        }

        public bool missingIfNull {
            get { return this.getBoolMember("missingIfNull", false); }
        }

        public string exception {
            get { return this.getStringMember("exception"); }
        }

        public bool hasException {
            get { return this._json.ContainsKey("exception"); }
        }

        public string propertyType {
            get { return this.getStringMember("propertyType"); }
        }

        public DiagnosticLevel defaultLevel {
            get { return this.getLevelMember("defaultLevel", DiagnosticLevel.info); }
        }

        public bool isDiagnosticableValue {
            get { return this.getBoolMember("isDiagnosticableValue", false); }
        }

        public InspectorInstanceRef valueRef {
            get {
                var id = this.getStringMember("valueId");
                return new InspectorInstanceRef(id);
            }
        }

        public List<DiagnosticsNode> children {
            get {
                object value;
                this._json.TryGetValue("children", out value);
                if (value == null) {
                    return new List<DiagnosticsNode>();
                }

                return ((IEnumerable) value).Cast<Dictionary<string, object>>()
                    .Select(n => new DiagnosticsNode(n, false))
                    .ToList();
            }
        }

        public Dictionary<string, object> valuePropertiesJson {
            get {
                object value;
                this._json.TryGetValue("valueProperties", out value);
                return (Dictionary<string, object>) value;
            }
        }

        public bool isColorProperty {
            get { return this.isProperty && (this.propertyType == typeof(Color).ToString()); }
        }

//
//        public bool isEnumProperty
//        {
//            get
//            {
//                var type = this.type;
//                
//            }
//        }
//        


        bool getBoolMember(string memberName, bool defaultValue = false) {
            object value;
            if (!this._json.TryGetValue(memberName, out value)) {
                return defaultValue;
            }

            if (value == null) {
                return defaultValue;
            }

            return Convert.ToBoolean(value);
        }

        DiagnosticLevel getLevelMember(string memberName, DiagnosticLevel defaultValue) {
            return this.getEnumMember(memberName, defaultValue);
        }

        DiagnosticsTreeStyle getStyleMember(string memberName, DiagnosticsTreeStyle defaultValue) {
            return this.getEnumMember(memberName, defaultValue);
        }

        T getEnumMember<T>(string memberName, T defaultValue) {
            object value;
            if (!this._json.TryGetValue(memberName, out value)) {
                return defaultValue;
            }


            if (value == null) {
                return defaultValue;
            }

            return (T) Enum.Parse(typeof(T), value.ToString());
        }

        public InspectorInstanceRef diagnosticRef {
            get { return new InspectorInstanceRef(this._json["objectId"].ToString()); }
        }

        public List<DiagnosticsNode> inlineProperties {
            get {
                var properties = new List<DiagnosticsNode>();
                object value;
                this._json.TryGetValue("properties", out value);
                if (value != null) {
                    foreach (var v in (IEnumerable<Dictionary<string, object>>) value) {
                        properties.Add(new DiagnosticsNode(v, true));
                    }
                }

                return properties;
            }
        }
    }
#pragma warning restore 0659
#pragma warning restore 0661

    public class InspectorInstanceRef : IEquatable<InspectorInstanceRef> {
        public readonly string id;

        public InspectorInstanceRef(string id) {
            this.id = id;
        }

        public bool Equals(InspectorInstanceRef other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(this.id, other.id);
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

            return this.Equals((InspectorInstanceRef) obj);
        }

        public override int GetHashCode() {
            return (this.id != null ? this.id.GetHashCode() : 0);
        }

        public static bool operator ==(InspectorInstanceRef left, InspectorInstanceRef right) {
            return Equals(left, right);
        }

        public static bool operator !=(InspectorInstanceRef left, InspectorInstanceRef right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"Id: {this.id}";
        }
    }
}