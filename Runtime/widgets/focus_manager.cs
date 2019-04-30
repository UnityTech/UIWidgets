using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class FocusNode : ChangeNotifier {
        internal FocusScopeNode _parent;
        internal FocusManager _manager;
        internal bool _hasKeyboardToken = false;

        public bool hasFocus {
            get {
                FocusNode node = null;
                if (this._manager != null) {
                    node = this._manager._currentFocus;
                }

                return node == this;
            }
        }

        public bool consumeKeyboardToken() {
            if (!this._hasKeyboardToken) {
                return false;
            }

            this._hasKeyboardToken = false;
            return true;
        }

        public void unfocus() {
            if (this._parent != null) {
                this._parent._resignFocus(this);
            }

            D.assert(this._parent == null);
            D.assert(this._manager == null);
        }

        public override void dispose() {
            if (this._manager != null) {
                this._manager._willDisposeFocusNode(this);
            }

            if (this._parent != null) {
                this._parent._resignFocus(this);
            }

            D.assert(this._parent == null);
            D.assert(this._manager == null);
            base.dispose();
        }

        internal void _notify() {
            this.notifyListeners();
        }

        public override string ToString() {
            return $"{Diagnostics.describeIdentity(this)} hasFocus: {this.hasFocus}";
        }
    }

    public class FocusScopeNode : DiagnosticableTree {
        internal FocusManager _manager;
        internal FocusScopeNode _parent;

        internal FocusScopeNode _nextSibling;
        internal FocusScopeNode _previousSibling;

        internal FocusScopeNode _firstChild;
        internal FocusScopeNode _lastChild;

        internal FocusNode _focus;
        internal List<FocusScopeNode> _focusPath;

        public bool isFirstFocus {
            get { return this._parent == null || this._parent._firstChild == this; }
        }

        internal List<FocusScopeNode> _getFocusPath() {
            List<FocusScopeNode> nodes = new List<FocusScopeNode> {this};
            FocusScopeNode node = this._parent;
            while (node != null && node != this._manager?.rootScope) {
                nodes.Add(node);
                node = node._parent;
            }

            return nodes;
        }

        internal void _prepend(FocusScopeNode child) {
            D.assert(child != this);
            D.assert(child != this._firstChild);
            D.assert(child != this._lastChild);
            D.assert(child._parent == null);
            D.assert(child._manager == null);
            D.assert(child._nextSibling == null);
            D.assert(child._previousSibling == null);
            D.assert(() => {
                var node = this;
                while (node._parent != null) {
                    node = node._parent;
                }

                D.assert(node != child);
                return true;
            });
            child._parent = this;
            child._nextSibling = this._firstChild;
            if (this._firstChild != null) {
                this._firstChild._previousSibling = child;
            }

            this._firstChild = child;
            this._lastChild = this._lastChild ?? child;
            child._updateManager(this._manager);
        }

        void _updateManager(FocusManager manager) {
            Action<FocusScopeNode> update = null;
            update = (child) => {
                if (child._manager == manager) {
                    return;
                }

                child._manager = manager;
                // We don't proactively null out the manager for FocusNodes because the
                // manager holds the currently active focus node until the end of the
                // microtask, even if that node is detached from the focus tree.
                if (manager != null && child._focus != null) {
                    child._focus._manager = manager;
                }

                child._visitChildren(update);
            };
            update(this);
        }

        void _visitChildren(Action<FocusScopeNode> vistor) {
            FocusScopeNode child = this._firstChild;
            while (child != null) {
                vistor.Invoke(child);
                child = child._nextSibling;
            }
        }

        bool _debugUltimatePreviousSiblingOf(FocusScopeNode child, FocusScopeNode equals) {
            while (child._previousSibling != null) {
                D.assert(child._previousSibling != child);
                child = child._previousSibling;
            }

            return child == equals;
        }

        bool _debugUltimateNextSiblingOf(FocusScopeNode child, FocusScopeNode equals) {
            while (child._nextSibling != null) {
                D.assert(child._nextSibling != child);
                child = child._nextSibling;
            }

            return child == equals;
        }

        internal void _remove(FocusScopeNode child) {
            D.assert(child._parent == this);
            D.assert(child._manager == this._manager);
            D.assert(this._debugUltimatePreviousSiblingOf(child, equals: this._firstChild));
            D.assert(this._debugUltimateNextSiblingOf(child, equals: this._lastChild));
            if (child._previousSibling == null) {
                D.assert(this._firstChild == child);
                this._firstChild = child._nextSibling;
            }
            else {
                child._previousSibling._nextSibling = child._nextSibling;
            }

            if (child._nextSibling == null) {
                D.assert(this._lastChild == child);
                this._lastChild = child._previousSibling;
            }
            else {
                child._nextSibling._previousSibling = child._previousSibling;
            }

            child._previousSibling = null;
            child._nextSibling = null;
            child._parent = null;
            child._updateManager(null);
        }

        internal void _didChangeFocusChain() {
            if (this.isFirstFocus && this._manager != null) {
                this._manager._markNeedsUpdate();
            }
        }

        public void requestFocus(FocusNode node) {
            D.assert(node != null);
            var focusPath = this._manager?._getCurrentFocusPath();
            if (this._focus == node &&
                (this._focusPath == focusPath || (focusPath != null && this._focusPath != null &&
                                                  this._focusPath.SequenceEqual(focusPath)))) {
                return;
            }

            if (this._focus != null) {
                this._focus.unfocus();
            }

            node._hasKeyboardToken = true;
            this._setFocus(node);
        }

        public void autofocus(FocusNode node) {
            D.assert(node != null);
            if (this._focus == null) {
                node._hasKeyboardToken = true;
                this._setFocus(node);
            }
        }

        public void reparentIfNeeded(FocusNode node) {
            D.assert(node != null);
            if (node._parent == null || node._parent == this) {
                return;
            }

            node.unfocus();
            D.assert(node._parent == null);
            if (this._focus == null) {
                this._setFocus(node);
            }
        }

        internal void _setFocus(FocusNode node) {
            D.assert(node != null);
            D.assert(node._parent == null);
            D.assert(this._focus == null);
            this._focus = node;
            this._focus._parent = this;
            this._focus._manager = this._manager;
            this._focus._hasKeyboardToken = true;
            this._didChangeFocusChain();
            this._focusPath = this._getFocusPath();
        }

        internal void _resignFocus(FocusNode node) {
            D.assert(node != null);
            if (this._focus != node) {
                return;
            }

            this._focus._parent = null;
            this._focus._manager = null;
            this._focus = null;
            this._didChangeFocusChain();
        }

        public void setFirstFocus(FocusScopeNode child) {
            D.assert(child != null);
            D.assert(child._parent == null || child._parent == this);
            if (this._firstChild == child) {
                return;
            }

            child.detach();
            this._prepend(child);
            D.assert(child._parent == this);
            this._didChangeFocusChain();
        }

        public void reparentScopeIfNeeded(FocusScopeNode child) {
            D.assert(child != null);
            if (child._parent == null || child._parent == this) {
                return;
            }

            if (child.isFirstFocus) {
                this.setFirstFocus(child);
            }
            else {
                child.detach();
            }
        }

        public void detach() {
            this._didChangeFocusChain();
            if (this._parent != null) {
                this._parent._remove(this);
            }

            D.assert(this._parent == null);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            if (this._focus != null) {
                properties.add(new DiagnosticsProperty<FocusNode>("focus", this._focus));
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();
            if (this._firstChild != null) {
                FocusScopeNode child = this._firstChild;
                int count = 1;
                while (true) {
                    children.Add(child.toDiagnosticsNode(name: $"child {count}"));
                    if (child == this._lastChild) {
                        break;
                    }

                    child = child._nextSibling;
                    count += 1;
                }
            }

            return children;
        }
    }

    public class FocusManager {
        public FocusManager() {
            this.rootScope._manager = this;
            D.assert(this.rootScope._firstChild == null);
            D.assert(this.rootScope._lastChild == null);
        }

        public readonly FocusScopeNode rootScope = new FocusScopeNode();
        internal readonly FocusScopeNode _noneScope = new FocusScopeNode();

        public FocusNode currentFocus {
            get { return this._currentFocus; }
        }

        internal FocusNode _currentFocus;

        internal void _willDisposeFocusNode(FocusNode node) {
            D.assert(node != null);
            if (this._currentFocus == node) {
                this._currentFocus = null;
            }
        }

        bool _haveScheduledUpdate = false;

        internal void _markNeedsUpdate() {
            if (this._haveScheduledUpdate) {
                return;
            }

            this._haveScheduledUpdate = true;
            Window.instance.scheduleMicrotask(this._update);
        }

        internal FocusNode _findNextFocus() {
            FocusScopeNode scope = this.rootScope;
            while (scope._firstChild != null) {
                scope = scope._firstChild;
            }

            return scope._focus;
        }

        internal void _update() {
            this._haveScheduledUpdate = false;
            var nextFocus = this._findNextFocus();
            if (this._currentFocus == nextFocus) {
                return;
            }

            var previousFocus = this._currentFocus;
            this._currentFocus = nextFocus;
            if (previousFocus != null) {
                previousFocus._notify();
            }

            if (this._currentFocus != null) {
                this._currentFocus._notify();
            }
        }

        internal List<FocusScopeNode> _getCurrentFocusPath() {
            return this._currentFocus?._parent?._getFocusPath();
        }

        public void focusNone(bool focus) {
            if (focus) {
                if (this._noneScope._parent != null && this._noneScope.isFirstFocus) {
                    return;
                }

                this.rootScope.setFirstFocus(this._noneScope);
            }
            else {
                if (this._noneScope._parent == null) {
                    return;
                }

                this._noneScope.detach();
            }
        }

        public override string ToString() {
            var status = this._haveScheduledUpdate ? " UPDATE SCHEDULED" : "";
            var indent = "    ";
            return string.Format("{1}{2}\n{0}currentFocus: {3}\n{4}", indent, Diagnostics.describeIdentity(this),
                status, this._currentFocus,
                this.rootScope.toStringDeep(prefixLineOne: indent, prefixOtherLines: indent));
        }
    }
}