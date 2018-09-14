using System;
using System.Collections.Generic;

namespace UIWidgets.foundation {
    public abstract class Key {
        protected Key() {
        }

        public static Key key(string value) {
            return new ValueKey<string>(value);
        }
    }

    public abstract class LocalKey : Key {
        protected LocalKey() {
        }
    }

    public class ValueKey<T> : LocalKey, IEquatable<ValueKey<T>> {
        public ValueKey(T value) {
            this.value = value;
        }

        public readonly T value;

        public bool Equals(ValueKey<T> other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(this.value, other.value);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((ValueKey<T>) obj);
        }

        public override int GetHashCode() {
            return EqualityComparer<T>.Default.GetHashCode(this.value);
        }

        public static bool operator ==(ValueKey<T> left, ValueKey<T> right) {
            return object.Equals(left, right);
        }

        public static bool operator !=(ValueKey<T> left, ValueKey<T> right) {
            return !object.Equals(left, right);
        }

        public override string ToString() {
            string valueString = typeof(T) == typeof(string) ? "<\'" + this.value + "\'>" : "<" + this.value + ">";

            if (this.GetType() == typeof(ValueKey<T>)) {
                return string.Format("[{0}]", valueString);
            }

            return string.Format("[{0} {1}]", this.GetType(), valueString);
        }
    }
}