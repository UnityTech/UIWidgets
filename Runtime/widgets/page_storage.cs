using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public interface PageStorageKey {
    }

    public class PageStorageKey<T> : ValueKey<T>, PageStorageKey {
        public PageStorageKey(T value) : base(value) {
        }
    }

    class _StorageEntryIdentifier : IEquatable<_StorageEntryIdentifier> {
        internal _StorageEntryIdentifier(List<PageStorageKey> keys) {
            D.assert(keys != null);
            this.keys = keys;
        }

        public readonly List<PageStorageKey> keys;

        public bool isNotEmpty {
            get { return this.keys.isNotEmpty(); }
        }

        public override string ToString() {
            return $"StorageEntryIdentifier({string.Join(":", this.keys.Select(x => x.ToString()).ToArray())})";
        }

        public bool Equals(_StorageEntryIdentifier other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.keys.SequenceEqual(other.keys);
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

            return this.Equals((_StorageEntryIdentifier) obj);
        }

        public override int GetHashCode() {
            if (this.keys == null || this.keys.isEmpty()) {
                return 0;
            }

            var hashCode = this.keys[0].GetHashCode();
            for (var i = 1; i < this.keys.Count; i++) {
                hashCode = (hashCode * 397) ^ this.keys[i].GetHashCode();
            }

            return hashCode;
        }

        public static bool operator ==(_StorageEntryIdentifier left, _StorageEntryIdentifier right) {
            return Equals(left, right);
        }

        public static bool operator !=(_StorageEntryIdentifier left, _StorageEntryIdentifier right) {
            return !Equals(left, right);
        }
    }

    public class PageStorageBucket {
        static bool _maybeAddKey(BuildContext context, List<PageStorageKey> keys) {
            Widget widget = context.widget;
            Key key = widget.key;
            if (key is PageStorageKey) {
                keys.Add((PageStorageKey) key);
            }

            return !(widget is PageStorage);
        }

        List<PageStorageKey> _allKeys(BuildContext context) {
            List<PageStorageKey> keys = new List<PageStorageKey>();
            if (_maybeAddKey(context, keys)) {
                context.visitAncestorElements(element => _maybeAddKey(element, keys));
            }

            return keys;
        }

        _StorageEntryIdentifier _computeIdentifier(BuildContext context) {
            return new _StorageEntryIdentifier(this._allKeys(context));
        }

        Dictionary<object, object> _storage;

        public void writeState(BuildContext context, object data, object identifier = null) {
            this._storage = this._storage ?? new Dictionary<object, object>();
            if (identifier != null) {
                this._storage[identifier] = data;
            }
            else {
                _StorageEntryIdentifier contextIdentifier = this._computeIdentifier(context);
                if (contextIdentifier.isNotEmpty) {
                    this._storage[contextIdentifier] = data;
                }
            }
        }

        public object readState(BuildContext context, object identifier = null) {
            if (this._storage == null) {
                return null;
            }

            if (identifier != null) {
                return this._storage.getOrDefault(identifier);
            }

            _StorageEntryIdentifier contextIdentifier = this._computeIdentifier(context);
            return contextIdentifier.isNotEmpty ? this._storage.getOrDefault(contextIdentifier) : null;
        }
    }


    public class PageStorage : StatelessWidget {
        public PageStorage(
            Key key = null,
            PageStorageBucket bucket = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(bucket != null);
            this.bucket = bucket;
            this.child = child;
        }

        public readonly Widget child;

        public readonly PageStorageBucket bucket;

        public static PageStorageBucket of(BuildContext context) {
            PageStorage widget = (PageStorage) context.ancestorWidgetOfExactType(typeof(PageStorage));
            return widget == null ? null : widget.bucket;
        }

        public override Widget build(BuildContext context) {
            return this.child;
        }
    }
}