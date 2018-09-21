using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.ui;

namespace UIWidgets.widgets
{
    public class FocusNode : ChangeNotifier {
        internal FocusScopeNode _parent;
        internal FocusManager _manager;
        internal bool _hasKeyboardToken = false;

        public bool hasFocus
        {
            get
            {
                FocusNode node = null;
                if (_manager != null)
                {
                    node = _manager._currentFocus;
                }
                return node == this;
            }
        }

        public bool consumeKeyboardToken()
        {
            if (!_hasKeyboardToken)
            {
                return false;
            }

            _hasKeyboardToken = false;
            return true;
        }

        public void unfocus()
        {
            if (_parent != null)
            {
                _parent._resignFocus(this);
            }
            D.assert(_parent == null);
            D.assert(_manager == null);
        }
        
        public override void dispose() {
            if (_manager != null)
            {
                _manager._willDisposeFocusNode(this);
            }

            if (_parent != null)
            {
                _parent._resignFocus(this);
            }
            D.assert(_parent == null);
            D.assert(_manager == null);
            base.dispose();
        }

        internal void _notify()
        {
            notifyListeners();
        }

        public override string ToString()
        {
            return string.Format("{0} hasFocus: {1}", Diagnostics.describeIdentity(this), hasFocus);
        }
    }

    public class FocusScopeNode:DiagnosticableTree
    {
                
        internal FocusManager _manager;
        internal FocusScopeNode _parent;

        internal FocusScopeNode _nextSibling;
        internal FocusScopeNode _previousSibling;

        internal FocusScopeNode _firstChild;
        internal FocusScopeNode _lastChild;

        internal FocusNode _focus;

        public bool isFirstFocus
        {
            get { return _parent == null || _parent._firstChild == this; }
        }

        internal void _prepend(FocusScopeNode child)
        {
            D.assert(child != this);
            D.assert(child != _firstChild);
            D.assert(child != _lastChild);
            D.assert(child == null);
            D.assert(child._manager == null);
            D.assert(child._nextSibling == null);
            D.assert(child._previousSibling == null);
            D.assert(() =>
            {
                var node = this;
                while (node._parent != null)
                {
                    node = node._parent;
                }
                D.assert(node != child);
                return true;
            });
            child._parent = this;
            child._nextSibling = _firstChild;
            if (_firstChild != null)
            {
                _firstChild._previousSibling = child;
            }
            _firstChild = child;
            _lastChild = _lastChild ?? child;
            child._updateManager(_manager);
        }

        void _updateManager(FocusManager manager)
        {
            Action<FocusScopeNode> update = null;
            update = (child) =>
            {
                if (child._manager == manager)
                    return;
                child._manager = manager;
                // We don't proactively null out the manager for FocusNodes because the
                // manager holds the currently active focus node until the end of the
                // microtask, even if that node is detached from the focus tree.
                if (manager != null && child._focus != null)
                {
                    child._focus._manager = manager;
                } 
                child._visitChildren(update);
            };
            update(this);
        }
        
        void _visitChildren(Action<FocusScopeNode> vistor) {
            FocusScopeNode child = _firstChild;
            while (child != null) {
                vistor.Invoke(child);
                child = child._nextSibling;
            }
        }

        private bool _debugUltimatePreviousSiblingOf(FocusScopeNode child, FocusScopeNode equals)
        {
            while (child._previousSibling != null)
            {
                D.assert(child._previousSibling != child);
                child = child._previousSibling;
            }

            return child == equals;
        }
        
        private bool _debugUltimateNextSiblingOf(FocusScopeNode child, FocusScopeNode equals)
        {
            while (child._nextSibling != null)
            {
                D.assert(child._nextSibling != child);
                child = child._nextSibling;
            }

            return child == equals;
        }
        
        internal void _remove(FocusScopeNode child)
        {
            D.assert(child._parent == this);
            D.assert(child._manager == _manager);
            D.assert(_debugUltimatePreviousSiblingOf(child, equals: _firstChild));
            D.assert(_debugUltimateNextSiblingOf(child, equals: _lastChild));
            if (child._previousSibling == null)
            {
                D.assert(_firstChild == child);
                _firstChild = child._nextSibling;
            }
            else
            {
                child._previousSibling._nextSibling = child._nextSibling;
            }
            
            if (child._nextSibling == null) {
                D.assert(_lastChild == child);
                _lastChild = child._previousSibling;
            } else {
                child._nextSibling._previousSibling = child._previousSibling;
            }
            
            child._previousSibling = null;
            child._nextSibling = null;
            child._parent = null;
            child._updateManager(null);
        }
        
        internal void _didChangeFocusChain() {
            if (isFirstFocus && _manager != null)
                _manager._markNeedsUpdate();
        }

        public void requestFocus(FocusNode node)
        {
            D.assert(node != null);
            if (_focus == node)
            {
                return;
            }

            if (_focus != null)
            {
                _focus.unfocus();
            }

            node._hasKeyboardToken = true;
            _setFocus(node);
        }
        
        public void autofocus(FocusNode node) {
            D.assert(node != null);
            if (_focus == null)
            {
                node._hasKeyboardToken = true;
                _setFocus(node);
            }
        }
        
        public void reparentIfNeeded(FocusNode node) {
            D.assert(node != null);
            if (node._parent == null || node._parent == this)
                return;
            node.unfocus();
            D.assert(node._parent == null);
            if (_focus == null)
                _setFocus(node);
        }
        
        internal void _setFocus(FocusNode node) {
            D.assert(node != null);
            D.assert(node._parent == null);
            D.assert(_focus == null);
            _focus = node;
            _focus._parent = this;
            _focus._manager = _manager;
            _focus._hasKeyboardToken = true;
            _didChangeFocusChain();
        }
        
        internal void _resignFocus(FocusNode node) {
            D.assert(node != null);
            if (_focus != node)
            {
                return;
            }

            _focus._parent = null;
            _focus._manager = null;
            _focus = null;
            _didChangeFocusChain();
        }

        public void setFirstFocus(FocusScopeNode child)
        {
            D.assert(child != null);
            D.assert(child._parent == null || child._parent == this);
            if (_firstChild == child)
            {
                return;
            }
            child.detach();
            _prepend(child);
            D.assert(child._parent == this);
            _didChangeFocusChain();
        }
        
        public void reparentScopeIfNeeded(FocusScopeNode child) {
            D.assert(child != null);
            if (child._parent == null || child._parent == this)
                return;
            if (child.isFirstFocus)
                setFirstFocus(child);
            else
                child.detach();
        }

        public void detach() {
            _didChangeFocusChain();
            if (_parent != null)
            {
                _parent._remove(this);
            }
            D.assert(_parent == null);
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            if (_focus != null)
                properties.add(new DiagnosticsProperty<FocusNode>("focus", _focus));
        }
        
        public override List<DiagnosticsNode> debugDescribeChildren()  {
            var children = new List<DiagnosticsNode>();
            if (_firstChild != null) {
                FocusScopeNode child = _firstChild;
                int count = 1;
                while (true) {
                    children.Add(child.toDiagnosticsNode(name: string.Format("child {0}", count)));
                    if (child == _lastChild)
                        break;
                    child = child._nextSibling;
                    count += 1;
                }
            }
            return children;
        }
    }

    public class FocusManager
    {
        public FocusManager()
        {
            rootScope._manager = this;
            D.assert(rootScope._firstChild == null);  
            D.assert(rootScope._lastChild == null);
        }

        public readonly FocusScopeNode rootScope = new FocusScopeNode();
        internal FocusNode _currentFocus;
        
        
        internal void _willDisposeFocusNode(FocusNode node) {
            D.assert(node != null);
            if (_currentFocus == node)
                _currentFocus = null;
        }
        
        
        bool _haveScheduledUpdate = false;
        internal void _markNeedsUpdate() {
            if (_haveScheduledUpdate)
            {
                return;
            }

            _haveScheduledUpdate = true;
            Window.instance.scheduleMicrotask(_update);
        }

        
        internal FocusNode _findNextFocus() {
            FocusScopeNode scope = rootScope;
            while (scope._firstChild != null)
                scope = scope._firstChild;
            return scope._focus;
        }

        internal void _update()
        {
            _haveScheduledUpdate = false;
            var nextFocus = _findNextFocus();
            if (_currentFocus == nextFocus)
                return;
            var previousFocus = _currentFocus;
            _currentFocus = nextFocus;
            if (previousFocus != null)
            {
                previousFocus._notify();
            }

            if (_currentFocus != null)
            {
                _currentFocus._notify();
            }
        }

        public override string ToString()
        {
            var status = _haveScheduledUpdate ? " UPDATE SCHEDULED" : "";
            var indent = "    ";
            return string.Format("{1}{2}\n{0}currentFocus: {3}\n{4}", indent, Diagnostics.describeIdentity(this), 
                status, _currentFocus, rootScope.toStringDeep(prefixLineOne: indent, prefixOtherLines: indent));
        }
    }
}