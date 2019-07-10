using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.foundation {
    
#pragma warning disable 0660
#pragma warning disable 0661
    public abstract class Key {
        protected Key() {
        }

        public static Key key(string value) {
            return new ValueKey<string>(value);
        }

        public static bool operator ==(Key left, Key right) {
            return Equals(left, right);
        }

        public static bool operator !=(Key left, Key right) {
            return !Equals(left, right);
        }
    }
#pragma warning restore 0660
#pragma warning restore 0661

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
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return EqualityComparer<T>.Default.Equals(this.value, other.value);
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

            return this.Equals((ValueKey<T>) obj);
        }

        public override int GetHashCode() {
            return EqualityComparer<T>.Default.GetHashCode(this.value);
        }

        public static bool operator ==(ValueKey<T> left, ValueKey<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(ValueKey<T> left, ValueKey<T> right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            string valueString = typeof(T) == typeof(string) ? "<\'" + this.value + "\'>" : "<" + this.value + ">";

            if (this.GetType() == typeof(ValueKey<T>)) {
                return $"[{valueString}]";
            }

            return $"[{this.GetType()} {valueString}]";
        }
    }
}