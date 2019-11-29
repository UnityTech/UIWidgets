using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.UIWidgets.Runtime.external
{    class SplayTree<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable<TKey> {
        SplayTreeNode root;
        int count;
        int version = 0;

        public void Add(TKey key, TValue value) {
            this.Set(key, value, throwOnExisting: true);
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            this.Set(item.Key, item.Value, throwOnExisting: true);
        }
        
        public void AddAll(IEnumerable<TKey> list) {
            foreach (var key in list) {
                this.Add(new KeyValuePair<TKey, TValue>(key, default));
            }
        }

        void Set(TKey key, TValue value, bool throwOnExisting) {
            if (this.count == 0) {
                this.version++;
                this.root = new SplayTreeNode(key, value);
                this.count = 1;
                return;
            }

            this.Splay(key);

            var c = key.CompareTo(this.root.Key);
            if (c == 0) {
                if (throwOnExisting) {
                    throw new ArgumentException("An item with the same key already exists in the tree.");
                }

                this.version++;
                this.root.Value = value;
                return;
            }

            var n = new SplayTreeNode(key, value);
            if (c < 0) {
                n.LeftChild = this.root.LeftChild;
                n.RightChild = this.root;
                this.root.LeftChild = null;
            }
            else {
                n.RightChild = this.root.RightChild;
                n.LeftChild = this.root;
                this.root.RightChild = null;
            }

            this.root = n;
            this.count++;
            this.Splay(key);
            this.version++;
        }

        public void Clear() {
            this.root = null;
            this.count = 0;
            this.version++;
        }

        public bool ContainsKey(TKey key) {
            if (this.count == 0) {
                return false;
            }

            this.Splay(key);

            return key.CompareTo(this.root.Key) == 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            if (this.count == 0) {
                return false;
            }

            this.Splay(item.Key);

            return item.Key.CompareTo(this.root.Key) == 0 &&
                   (ReferenceEquals(this.root.Value, item.Value) ||
                    (!ReferenceEquals(item.Value, null) && item.Value.Equals(this.root.Value)));
        }

        public KeyValuePair<TKey, TValue>? First() {
            SplayTreeNode t = this.root;
            if (t == null) {
                return null;
            }

            while (t.LeftChild != null) {
                t = t.LeftChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        public KeyValuePair<TKey, TValue> FirstOrDefault() {
            SplayTreeNode t = this.root;
            if (t == null) {
                return new KeyValuePair<TKey, TValue>(default(TKey), default(TValue));
            }

            while (t.LeftChild != null) {
                t = t.LeftChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        public KeyValuePair<TKey, TValue>? Last() {
            SplayTreeNode t = this.root;
            if (t == null) {
                return null;
            }

            while (t.RightChild != null) {
                t = t.RightChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        public KeyValuePair<TKey, TValue> LastOrDefault() {
            SplayTreeNode t = this.root;
            if (t == null) {
                return new KeyValuePair<TKey, TValue>(default(TKey), default(TValue));
            }

            while (t.RightChild != null) {
                t = t.RightChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        void Splay(TKey key) {
            SplayTreeNode l, r, t, y, header;
            l = r = header = new SplayTreeNode(default(TKey), default(TValue));
            t = this.root;
            while (true) {
                var c = key.CompareTo(t.Key);
                if (c < 0) {
                    if (t.LeftChild == null) {
                        break;
                    }

                    if (key.CompareTo(t.LeftChild.Key) < 0) {
                        y = t.LeftChild;
                        t.LeftChild = y.RightChild;
                        y.RightChild = t;
                        t = y;
                        if (t.LeftChild == null) {
                            break;
                        }
                    }

                    r.LeftChild = t;
                    r = t;
                    t = t.LeftChild;
                }
                else if (c > 0) {
                    if (t.RightChild == null) {
                        break;
                    }

                    if (key.CompareTo(t.RightChild.Key) > 0) {
                        y = t.RightChild;
                        t.RightChild = y.LeftChild;
                        y.LeftChild = t;
                        t = y;
                        if (t.RightChild == null) {
                            break;
                        }
                    }

                    l.RightChild = t;
                    l = t;
                    t = t.RightChild;
                }
                else {
                    break;
                }
            }

            l.RightChild = t.LeftChild;
            r.LeftChild = t.RightChild;
            t.LeftChild = header.RightChild;
            t.RightChild = header.LeftChild;
            this.root = t;
        }

        public bool Remove(TKey key) {
            if (this.count == 0) {
                return false;
            }

            this.Splay(key);

            if (key.CompareTo(this.root.Key) != 0) {
                return false;
            }

            if (this.root.LeftChild == null) {
                this.root = this.root.RightChild;
            }
            else {
                var swap = this.root.RightChild;
                this.root = this.root.LeftChild;
                this.Splay(key);
                this.root.RightChild = swap;
            }

            this.version++;
            this.count--;
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            if (this.count == 0) {
                value = default(TValue);
                return false;
            }

            this.Splay(key);
            if (key.CompareTo(this.root.Key) != 0) {
                value = default(TValue);
                return false;
            }

            value = this.root.Value;
            return true;
        }

        public TValue this[TKey key] {
            get {
                if (this.count == 0) {
                    throw new KeyNotFoundException("The key was not found in the tree.");
                }

                this.Splay(key);
                if (key.CompareTo(this.root.Key) != 0) {
                    throw new KeyNotFoundException("The key was not found in the tree.");
                }

                return this.root.Value;
            }

            set { this.Set(key, value, throwOnExisting: false); }
        }

        public int Count {
            get { return this.count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            if (this.count == 0) {
                return false;
            }

            this.Splay(item.Key);

            if (item.Key.CompareTo(this.root.Key) == 0 && (ReferenceEquals(this.root.Value, item.Value) ||
                                                           (!ReferenceEquals(item.Value, null) &&
                                                            item.Value.Equals(this.root.Value)))) {
                return false;
            }

            if (this.root.LeftChild == null) {
                this.root = this.root.RightChild;
            }
            else {
                var swap = this.root.RightChild;
                this.root = this.root.LeftChild;
                this.Splay(item.Key);
                this.root.RightChild = swap;
            }

            this.version++;
            this.count--;
            return true;
        }

        public void Trim(int depth) {
            if (depth < 0) {
                throw new ArgumentOutOfRangeException("depth", "The trim depth must not be negative.");
            }

            if (this.count == 0) {
                return;
            }

            if (depth == 0) {
                this.Clear();
            }
            else {
                var prevCount = this.count;
                this.count = this.Trim(this.root, depth - 1);
                if (prevCount != this.count) {
                    this.version++;
                }
            }
        }

        int Trim(SplayTreeNode node, int depth) {
            if (depth == 0) {
                node.LeftChild = null;
                node.RightChild = null;
                return 1;
            }
            else {
                int count = 1;

                if (node.LeftChild != null) {
                    count += this.Trim(node.LeftChild, depth - 1);
                }

                if (node.RightChild != null) {
                    count += this.Trim(node.RightChild, depth - 1);
                }

                return count;
            }
        }

        public ICollection<TKey> Keys {
            get { return new TiedList<TKey>(this, this.version, this.AsList(node => node.Key)); }
        }

        public ICollection<TValue> Values {
            get { return new TiedList<TValue>(this, this.version, this.AsList(node => node.Value)); }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            this.AsList(node => new KeyValuePair<TKey, TValue>(node.Key, node.Value)).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return new TiedList<KeyValuePair<TKey, TValue>>(this, this.version,
                this.AsList(node => new KeyValuePair<TKey, TValue>(node.Key, node.Value))).GetEnumerator();
        }

        IList<TEnumerator> AsList<TEnumerator>(Func<SplayTreeNode, TEnumerator> selector) {
            if (this.root == null) {
                return new TEnumerator[0];
            }

            var result = new List<TEnumerator>(this.count);
            this.PopulateList(this.root, result, selector);
            return result;
        }

        void PopulateList<TEnumerator>(SplayTreeNode node, List<TEnumerator> list,
            Func<SplayTreeNode, TEnumerator> selector) {
            if (node.LeftChild != null) {
                this.PopulateList(node.LeftChild, list, selector);
            }

            list.Add(selector(node));
            if (node.RightChild != null) {
                this.PopulateList(node.RightChild, list, selector);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        sealed class SplayTreeNode {
            public readonly TKey Key;

            public TValue Value;
            public SplayTreeNode LeftChild;
            public SplayTreeNode RightChild;

            public SplayTreeNode(TKey key, TValue value) {
                this.Key = key;
                this.Value = value;
            }
        }

        sealed class TiedList<T> : IList<T> {
            readonly SplayTree<TKey, TValue> tree;
            readonly int version;
            readonly IList<T> backingList;

            public TiedList(SplayTree<TKey, TValue> tree, int version, IList<T> backingList) {
                if (tree == null) {
                    throw new ArgumentNullException("tree");
                }

                if (backingList == null) {
                    throw new ArgumentNullException("backingList");
                }

                this.tree = tree;
                this.version = version;
                this.backingList = backingList;
            }

            public int IndexOf(T item) {
                if (this.tree.version != this.version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                return this.backingList.IndexOf(item);
            }

            public void Insert(int index, T item) {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index) {
                throw new NotSupportedException();
            }

            public T this[int index] {
                get {
                    if (this.tree.version != this.version) {
                        throw new InvalidOperationException("The collection has been modified.");
                    }

                    return this.backingList[index];
                }
                set { throw new NotSupportedException(); }
            }

            public void Add(T item) {
                throw new NotSupportedException();
            }

            public void Clear() {
                throw new NotSupportedException();
            }

            public bool Contains(T item) {
                if (this.tree.version != this.version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                return this.backingList.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex) {
                if (this.tree.version != this.version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                this.backingList.CopyTo(array, arrayIndex);
            }

            public int Count {
                get { return this.tree.count; }
            }

            public bool IsReadOnly {
                get { return true; }
            }

            public bool Remove(T item) {
                throw new NotSupportedException();
            }

            public IEnumerator<T> GetEnumerator() {
                if (this.tree.version != this.version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                foreach (var item in this.backingList) {
                    yield return item;
                    if (this.tree.version != this.version) {
                        throw new InvalidOperationException("The collection has been modified.");
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
        }
    }
}