using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UIWidgets.ui;

namespace UIWidgets.debugger
{
    public class DiagnosticsNode : IEquatable<DiagnosticsNode>
    {
        private readonly Dictionary<string, object> _json; // todo use json class such as simple json
        public readonly bool isProperty;
        private DiagnosticsNode _parent;

        public DiagnosticsNode(Dictionary<string, object> json, bool isProperty)
        {
            this._json = json;
            this.isProperty = isProperty;
        }

        public bool Equals(DiagnosticsNode other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(diagnosticRef, other.diagnosticRef);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DiagnosticsNode) obj);
        }

        public static bool operator ==(DiagnosticsNode left, DiagnosticsNode right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DiagnosticsNode left, DiagnosticsNode right)
        {
            return !Equals(left, right);
        }

        public DiagnosticsNode parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public string getStringMember(string memberName)
        {
            object ret;
            _json.TryGetValue(memberName, out ret);
            return ret == null ? null : ret.ToString();
        }

        public string separator
        {
            get { return showSeparator ? ":" : ""; }
        }

        public string name
        {
            get { return getStringMember("name"); }
        }

        public bool showSeparator
        {
            get { return getBoolMember("showSeparator", true); }
        }

        public string description
        {
            get { return getStringMember("description"); }
        }

        public DiagnosticLevel level
        {
            get { return getLevelMember("level", DiagnosticLevel.info); }
        }

        public bool showName
        {
            get { return getBoolMember("showName", true); }
        }

        public string emptyBodyDescription
        {
            get { return getStringMember("emptyBodyDescription"); }
        }

        public DiagnosticsTreeStyle style
        {
            get { return getStyleMember("style", DiagnosticsTreeStyle.sparse); }
        }

        public string type
        {
            get { return getStringMember("type"); }
        }

        public bool isQuoted
        {
            get { return getBoolMember("quoted", false); }
        }

        public bool hasIsQuoted
        {
            get { return _json.ContainsKey("quoted"); }
        }

        public string unit
        {
            get { return getStringMember("unit"); }
        }

        public bool hasUnit
        {
            get { return _json.ContainsKey("unit"); }
        }

        public string numberToString
        {
            get { return getStringMember("numberToString"); }
        }

        public bool hasNumberToString
        {
            get { return _json.ContainsKey("numberToString"); }
        }

        public string ifTrue
        {
            get { return getStringMember("ifTrue"); }
        }

        public bool hasIfTrue
        {
            get { return _json.ContainsKey("ifTrue"); }
        }

        public string ifFalse
        {
            get { return getStringMember("ifFalse"); }
        }

        public bool hasIfFalse
        {
            get { return _json.ContainsKey("ifFalse"); }
        }

        public List<string> values
        {
            get
            {
                object value;
                _json.TryGetValue("values", out value);
                if (value == null)
                {
                    return null;
                }

                return ((IEnumerable) value).Cast<object>().Select(o => o.ToString()).ToList();
            }
        }

        public bool hasValues
        {
            get { return _json.ContainsKey("values"); }
        }

        public string ifPresent
        {
            get { return getStringMember("ifPresent"); }
        }

        public bool hasIfPresent
        {
            get { return _json.ContainsKey("ifPresent"); }
        }

        public string defaultValue
        {
            get { return getStringMember("defaultValue"); }
        }

        public bool hasDefaultValue
        {
            get { return _json.ContainsKey("defaultValue"); }
        }

        public string ifEmpty
        {
            get { return getStringMember("ifEmpty"); }
        }

        public string ifNull
        {
            get { return getStringMember("ifNull"); }
        }

        public string tooltip
        {
            get { return getStringMember("tooltip"); }
        }

        public bool hasTooltip
        {
            get { return _json.ContainsKey("tooltip"); }
        }

        public bool missingIfNull
        {
            get { return getBoolMember("missingIfNull", false); }
        }

        public string exception
        {
            get { return getStringMember("exception"); }
        }

        public bool hasException
        {
            get { return _json.ContainsKey("exception"); }
        }

        public string propertyType
        {
            get { return getStringMember("propertyType"); }
        }

        public DiagnosticLevel defaultLevel
        {
            get { return getLevelMember("defaultLevel", DiagnosticLevel.info); }
        }

        public bool isDiagnosticableValue
        {
            get { return getBoolMember("isDiagnosticableValue", false); }
        }

        public InspectorInstanceRef valueRef
        {
            get
            {
                var id = getStringMember("valueId");
                return new InspectorInstanceRef(id);
            }
        }

        public List<DiagnosticsNode> children
        {
            get
            {
                object value;
                _json.TryGetValue("children", out value);
                if (value == null)
                {
                    return new List<DiagnosticsNode>();
                }

                return ((IEnumerable) value).Cast<Dictionary<string, object>>()
                    .Select(n => new DiagnosticsNode(n, false))
                    .ToList();
            }
        }

        public Dictionary<string, object> valuePropertiesJson
        {
            get
            {
                object value;
                _json.TryGetValue("valueProperties", out value);
                return (Dictionary<string, object>) value;

            }
        }

        public bool isColorProperty
        {
            get { return isProperty && (propertyType == typeof(Color).ToString()); }
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


        private bool getBoolMember(string memberName, bool defaultValue = false)
        {
            object value;
            if (!_json.TryGetValue(memberName, out value))
            {
                return defaultValue;
            }

            if (value == null)
            {
                return defaultValue;
            }

            return Convert.ToBoolean(value);
        }

        private DiagnosticLevel getLevelMember(string memberName, DiagnosticLevel defaultValue)
        {
            return getEnumMember(memberName, defaultValue);
        }

        private DiagnosticsTreeStyle getStyleMember(string memberName, DiagnosticsTreeStyle defaultValue)
        {
            return getEnumMember(memberName, defaultValue);
        }

        private T getEnumMember<T>(string memberName, T defaultValue)
        {
            object value;
            if (!_json.TryGetValue(memberName, out value))
            {
                return defaultValue;
            }


            if (value == null)
            {
                return defaultValue;
            }

            return (T) Enum.Parse(typeof(T), value.ToString());
        }

        public InspectorInstanceRef diagnosticRef
        {
            get { return new InspectorInstanceRef(_json["objectId"].ToString()); }
        }

        public List<DiagnosticsNode> inlineProperties
        {
            get
            {
                var properties = new List<DiagnosticsNode>();
                object value;
                _json.TryGetValue("properties", out value);
                if (value != null)
                {
                    foreach (var v in (IEnumerable<Dictionary<string, object>>) value)
                    {
                        properties.Add(new DiagnosticsNode(v, true));
                    }
                }
                return properties;
            }
        }
    }

    public class InspectorInstanceRef : IEquatable<InspectorInstanceRef>
    {
        public readonly string id;

        public InspectorInstanceRef(string id)
        {
            this.id = id;
        }

        public bool Equals(InspectorInstanceRef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(id, other.id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InspectorInstanceRef) obj);
        }

        public override int GetHashCode()
        {
            return (id != null ? id.GetHashCode() : 0);
        }

        public static bool operator ==(InspectorInstanceRef left, InspectorInstanceRef right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InspectorInstanceRef left, InspectorInstanceRef right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Format("Id: {0}", id);
        }
    }
}