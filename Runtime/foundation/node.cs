using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.UIWidgets.foundation {
    class _DependencyList : IEquatable<_DependencyList> {
        internal _DependencyList(Type type, object target) {
            D.assert(type != null);
            this._type = type;

            var fields = _getTypeFields(type);

            this._list = new List<object>(fields.Length);
            foreach (var field in fields) {
                this._list.Add(field.GetValue(target));
            }
        }

        readonly List<object> _list;

        readonly Type _type;

        static readonly Dictionary<Type, FieldInfo[]> _typeFields = new Dictionary<Type, FieldInfo[]>();

        static FieldInfo[] _getTypeFields(Type type) {
            FieldInfo[] fields;
            if (_typeFields.TryGetValue(type, out fields)) {
                return fields;
            }

            _typeFields[type] = fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);


            D.assert(() => {
                foreach (var field in fields) {
                    if (!field.IsInitOnly) {
                        throw new UIWidgetsError(
                            type + " is pure and should be immutable. All public fields need to be readonly. " +
                            field + " is not readonly.");
                    }
                }

                return true;
            });

            return fields;
        }

        static bool _sequenceEquals(IList list1, IList list2) {
            if (list1 == null && list2 == null) {
                return true;
            }

            if (list1 == null || list2 == null) {
                return true;
            }

            if (list1.Count != list2.Count) {
                return false;
            }

            for (var i = 0; i < list1.Count; i++) {
                var item1 = list1[i];
                var item2 = list2[i];

                if (item1 is IList && item2 is IList) {
                    if (!_sequenceEquals((IList) item1, (IList) item2)) {
                        return false;
                    }
                }
                else {
                    if (!Equals(item1, item2)) {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool Equals(_DependencyList other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this._type == other._type && _sequenceEquals(this._list, other._list);
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

            return this.Equals((_DependencyList) obj);
        }

        static int _sequenceHashCode(IList list) {
            unchecked {
                if (list == null) {
                    return 0;
                }

                var hashCode = 0;
                for (var i = 0; i < list.Count; i++) {
                    var item = list[i];
                    if (item is IList) {
                        hashCode = (hashCode * 397) ^ _sequenceHashCode((IList) item);
                    }
                    else {
                        hashCode = (hashCode * 397) ^ (item == null ? 0 : item.GetHashCode());
                    }
                }

                return hashCode;
            }
        }

        public override int GetHashCode() {
            unchecked {
                return (this._type.GetHashCode() * 397) ^ _sequenceHashCode(this._list);
            }
        }

        public static bool operator ==(_DependencyList left, _DependencyList right) {
            return Equals(left, right);
        }

        public static bool operator !=(_DependencyList left, _DependencyList right) {
            return !Equals(left, right);
        }
    }
}