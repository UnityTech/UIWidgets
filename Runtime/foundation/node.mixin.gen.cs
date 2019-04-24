using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.async;

namespace Unity.UIWidgets.foundation {



    public class AbstractNode  {
        public int depth {
            get { return this._depth; }
        }

        int _depth = 0;

        protected void redepthChild(AbstractNode child) {
            D.assert(child.owner == this.owner);
            if (child._depth <= this._depth) {
                child._depth = this._depth + 1;
                child.redepthChildren();
            }
        }

        public virtual void redepthChildren() {
        }

        public object owner {
            get { return this._owner; }
        }

        object _owner;

        public bool attached {
            get { return this._owner != null; }
        }

        public virtual void attach(object owner) {
            D.assert(owner != null);
            D.assert(this._owner == null);
            this._owner = owner;
        }

        public virtual void detach() {
            D.assert(this._owner != null);
            this._owner = null;
        }

        public AbstractNode parent {
            get { return this._parent; }
        }

        AbstractNode _parent;

        protected virtual void adoptChild(AbstractNode child) {
            D.assert(child != null);
            D.assert(child._parent == null);
            D.assert(() => {
                var node = this;
                while (node.parent != null) {
                    node = node.parent;
                }
                D.assert(node != child); // indicates we are about to create a cycle
                return true;
            });

            child._parent = this;
            if (this.attached) {
                child.attach(this._owner);
            }

            this.redepthChild(child);
        }

        protected virtual void dropChild(AbstractNode child) {
            D.assert(child != null);
            D.assert(child._parent == this);
            D.assert(child.attached == this.attached);

            child._parent = null;
            if (this.attached) {
                child.detach();
            }
        }
    }




    public class AbstractNodeMixinDiagnosticableTree  : DiagnosticableTree {
        public int depth {
            get { return this._depth; }
        }

        int _depth = 0;

        protected void redepthChild(AbstractNodeMixinDiagnosticableTree child) {
            D.assert(child.owner == this.owner);
            if (child._depth <= this._depth) {
                child._depth = this._depth + 1;
                child.redepthChildren();
            }
        }

        public virtual void redepthChildren() {
        }

        public object owner {
            get { return this._owner; }
        }

        object _owner;

        public bool attached {
            get { return this._owner != null; }
        }

        public virtual void attach(object owner) {
            D.assert(owner != null);
            D.assert(this._owner == null);
            this._owner = owner;
        }

        public virtual void detach() {
            D.assert(this._owner != null);
            this._owner = null;
        }

        public AbstractNodeMixinDiagnosticableTree parent {
            get { return this._parent; }
        }

        AbstractNodeMixinDiagnosticableTree _parent;

        protected virtual void adoptChild(AbstractNodeMixinDiagnosticableTree child) {
            D.assert(child != null);
            D.assert(child._parent == null);
            D.assert(() => {
                var node = this;
                while (node.parent != null) {
                    node = node.parent;
                }
                D.assert(node != child); // indicates we are about to create a cycle
                return true;
            });

            child._parent = this;
            if (this.attached) {
                child.attach(this._owner);
            }

            this.redepthChild(child);
        }

        protected virtual void dropChild(AbstractNodeMixinDiagnosticableTree child) {
            D.assert(child != null);
            D.assert(child._parent == this);
            D.assert(child.attached == this.attached);

            child._parent = null;
            if (this.attached) {
                child.detach();
            }
        }
    }




   public abstract class CanonicalMixinDiagnosticableTree : DiagnosticableTree {
        _DependencyList _dependencyList;

        _DependencyList _getDependencyList() {
            if (this._dependencyList == null) {
                this._dependencyList = new _DependencyList(this.GetType(), this);
            }

            return this._dependencyList;
        }
        
        CanonicalMixinDiagnosticableTree _canonical;

        CanonicalMixinDiagnosticableTree _getCanonical() {
            if (this._canonical != null) {
                return this._canonical;
            }

            var weakReference = _canonicalObjects.putIfAbsent(this._getDependencyList(), () => new WeakReference(this));
            if (weakReference.Target == null) {
                weakReference.Target = this;
            }

            return this._canonical = (CanonicalMixinDiagnosticableTree) weakReference.Target;
        }

        ~CanonicalMixinDiagnosticableTree() {
            if (ReferenceEquals(this, this._canonical)) {
                var dependencyList = this._dependencyList;
                if (dependencyList != null) {
                    Timer.runInMainFromFinalizer(() => { _canonicalObjects.Remove(dependencyList); });
                }
            }
        }
        
        static readonly Dictionary<_DependencyList, WeakReference> _canonicalObjects =
            new Dictionary<_DependencyList, WeakReference>();
        
        public bool pureWidget { get; set; } // if canonicalEquals should not be used.

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

            if (!this.pureWidget) {
                return ReferenceEquals(this, obj);
            } else {
                return ReferenceEquals(this._getCanonical(), ((CanonicalMixinDiagnosticableTree) obj)._getCanonical());
            }
        }
        
        public override int GetHashCode() {
            return RuntimeHelpers.GetHashCode(this._getCanonical());
        }
    }


}