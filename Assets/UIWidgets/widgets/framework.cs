using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIWidgets.foundation;
using UIWidgets.rendering;
using UIWidgets.ui;
using UnityEngine.Assertions;

namespace UIWidgets.widgets {
    public class UniqueKey : LocalKey {
        public UniqueKey() {
        }

        public override string ToString() {
            return string.Format("[#{0}]", Diagnostics.shortHash(this));
        }
    }

    public class ObjectKey : LocalKey, IEquatable<ObjectKey> {
        public ObjectKey(object value) {
            this.value = value;
        }

        public readonly object value;

        public bool Equals(ObjectKey other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return object.Equals(this.value, other.value);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((ObjectKey) obj);
        }

        public override int GetHashCode() {
            return (this.value != null ? this.value.GetHashCode() : 0);
        }

        public static bool operator ==(ObjectKey left, ObjectKey right) {
            return object.Equals(left, right);
        }

        public static bool operator !=(ObjectKey left, ObjectKey right) {
            return !object.Equals(left, right);
        }

        public override string ToString() {
            return string.Format("[{0} {1}]", this.GetType(), Diagnostics.describeIdentity(this.value));
        }
    }

    public abstract class GlobalKey<T> : Key where T : State<StatefulWidget> {
        protected GlobalKey() {
        }

        public static GlobalKey<T> key(string debugLabel = null) {
            return new LabeledGlobalKey<T>(debugLabel);
        }

        static readonly Dictionary<GlobalKey<T>, Element> _registry = new Dictionary<GlobalKey<T>, Element>();
        static readonly HashSet<GlobalKey<T>> _removedKeys = new HashSet<GlobalKey<T>>();
        static readonly HashSet<Element> _debugIllFatedElements = new HashSet<Element>();
        static readonly Dictionary<GlobalKey<T>, Element> _debugReservations = new Dictionary<GlobalKey<T>, Element>();

        void _register(Element element) {
            D.assert(() => {
                if (_registry.ContainsKey(this)) {
                    D.assert(element.widget != null);
                    D.assert(_registry[this].widget != null);
                    D.assert(element.widget.GetType() != _registry[this].widget.GetType());
                    _debugIllFatedElements.Add(_registry[this]);
                }

                return true;
            });
            _registry[this] = element;
        }

        void _unregister(Element element) {
            D.assert(() => {
                if (_registry.ContainsKey(this) && _registry[this] != element) {
                    D.assert(element.widget != null);
                    D.assert(_registry[this].widget != null);
                    D.assert(element.widget.GetType() != _registry[this].widget.GetType());
                }

                return true;
            });
            if (_registry[this] == element) {
                _registry.Remove(this);
                _removedKeys.Add(this);
            }
        }

        void _debugReserveFor(Element parent) {
            D.assert(() => {
                D.assert(parent != null);
                if (_debugReservations.ContainsKey(this) && _debugReservations[this] != parent) {
                    string older = _debugReservations[this].ToString();
                    string newer = parent.ToString();
                    if (older != newer) {
                        throw new UIWidgetsError(
                            string.Format(
                                "Multiple widgets used the same GlobalKey.\n" +
                                "The key {0} was used by multiple widgets. The parents of those widgets were:\n" +
                                "- {1}\n" +
                                "- {2}\n" +
                                "A GlobalKey can only be specified on one widget at a time in the widget tree.",
                                this, older, newer));
                    }

                    throw new UIWidgetsError(
                        string.Format(
                            "Multiple widgets used the same GlobalKey.\n" +
                            "The key {0} was used by multiple widgets. The parents of those widgets were " +
                            "different widgets that both had the following description:\n" +
                            "  {1}\n" +
                            "A GlobalKey can only be specified on one widget at a time in the widget tree.",
                            this, newer));
                }

                _debugReservations[this] = parent;
                return true;
            });
        }

        static void _debugVerifyIllFatedPopulation() {
            D.assert(() => {
                Dictionary<GlobalKey<T>, HashSet<Element>> duplicates = null;
                foreach (Element element in _debugIllFatedElements) {
                    if (element._debugLifecycleState != _ElementLifecycle.defunct) {
                        D.assert(element != null);
                        D.assert(element.widget != null);
                        D.assert(element.widget.key != null);
                        GlobalKey key = element.widget.key;
                        D.assert(_registry.ContainsKey(key));
                        duplicates = duplicates ?? new Dictionary<GlobalKey<T>, HashSet<Element>>();
                        var elements = duplicates.putIfAbsent(key, () => new HashSet<Element>());
                        elements.Add(element);
                        elements.Add(_registry[key]);
                    }
                }

                _debugIllFatedElements.Clear();
                _debugReservations.Clear();
                if (duplicates != null) {
                    var buffer = new StringBuilder();
                    buffer.AppendLine("Multiple widgets used the same GlobalKey.\n");
                    foreach (GlobalKey<T> key in duplicates.Keys) {
                        HashSet<Element> elements = duplicates[key];
                        buffer.AppendLine(string.Format("The key {0} was used by {1} widgets:", key, elements.Count));
                        foreach (Element element in elements) {
                            buffer.AppendLine("- " + element);
                        }
                    }

                    buffer.Append("A GlobalKey can only be specified on one widget at a time in the widget tree.");
                    throw new UIWidgetsError(buffer.ToString());
                }

                return true;
            });
        }

        Element _currentElement {
            get { return _registry[this]; }
        }

        public BuildContext currentContext {
            get { return this._currentElement; }
        }

        public Widget currentWidget {
            get { return this._currentElement == null ? null : this._currentElement.widget; }
        }

        public T currentState {
            get {
                Element element = this._currentElement;
                if (element is StatefulElement) {
                    var statefulElement = (StatefulElement) element;
                    State state = statefulElement.state;
                    if (state is T) {
                        return (T) state;
                    }
                }

                return null;
            }
        }
    }

    public class LabeledGlobalKey<T> : GlobalKey<T> where T : State<StatefulWidget> {
        public LabeledGlobalKey(string _debugLabel = null) {
        }

        readonly string _debugLabel;

        public override string ToString() {
            string label = this._debugLabel != null ? " " + this._debugLabel : "";
            if (this.GetType() == typeof(LabeledGlobalKey<T>)) {
                return string.Format("[GlobalKey#{0}{1}]", Diagnostics.shortHash(this), label);
            }

            return string.Format("[{0}{1}]", Diagnostics.describeIdentity(this), label);
        }
    }

    class GlobalObjectKey<T> : GlobalKey<T>, IEquatable<GlobalObjectKey<T>> where T : State<StatefulWidget> {
        public GlobalObjectKey(object value) {
        }

        public readonly Object value;

        public bool Equals(GlobalObjectKey<T> other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(this.value, other.value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GlobalObjectKey<T>) obj);
        }

        public override int GetHashCode() {
            return (this.value != null ? this.value.GetHashCode() : 0);
        }

        public static bool operator ==(GlobalObjectKey<T> left, GlobalObjectKey<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(GlobalObjectKey<T> left, GlobalObjectKey<T> right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            String selfType = this.GetType().ToString();
            string suffix = "<State<StatefulWidget>>";
            if (selfType.EndsWith(suffix)) {
                selfType = selfType.Substring(0, selfType.Length - suffix.Length);
            }

            return string.Format("[{0} {1}]", selfType, Diagnostics.describeIdentity(this.value));
        }
    }

    public delegate void ElementVisitor(Element element);

    public delegate bool ElementVisitorBool(Element element);

    public abstract class Widget : DiagnosticableTree {
        protected Widget(Key key = null) {
            this.key = key;
        }

        public readonly Key key;

        protected abstract Element createElement();

        public override string toStringShort() {
            return this.key == null ? this.GetType().ToString() : this.GetType() + "-" + this.key;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.dense;
        }

        public static bool canUpdate(Widget oldWidget, Widget newWidget) {
            return oldWidget.GetType() == newWidget.GetType() && object.Equals(oldWidget.key, newWidget.key);
        }
    }

    public abstract class StatelessWidget : Widget {
        protected StatelessWidget(Key key = null) : base(key: key) {
        }

        protected override Element createElement() {
            return new StatelessElement(this);
        }

        protected abstract Widget build(BuildContext context);
    }

    public abstract class StatefulWidget : Widget {
        protected StatefulWidget(Key key) : base(key: key) {
        }

        protected override Element createElement() {
            return new StatefulElement(this);
        }

        protected abstract State createState();
    }

    enum _StateLifecycle {
        created,
        initialized,
        ready,
        defunct,
    }

    public delegate void StateSetter(VoidCallback fn);

    public abstract class State : Diagnosticable {
        public StatefulWidget widget {
            get { return this._widget; }
        }

        StatefulWidget _widget;
        
        _StateLifecycle _debugLifecycleState = _StateLifecycle.created;

        protected virtual bool _debugTypesAreRight(Widget widget) {
            return widget is StatefulWidget;
        }

        public BuildContext context {
            get { return this._element; }
        }

        StatefulElement _element;

        public bool mounted {
            get { return this._element != null; }
        }

        protected virtual void initState() {
            D.assert(this._debugLifecycleState == _StateLifecycle.created);
        }

        public abstract void didUpdateWidget(StatefulWidget oldWidget);

        protected void setState(VoidCallback fn) {
            D.assert(fn != null);

            D.assert(() => {
                if (this._debugLifecycleState == _StateLifecycle.defunct) {
                    throw new UIWidgetsError(
                        "setState() called after dispose(): " + this + "\n" +
                        "This error happens if you call setState() on a State object for a widget that " +
                        "no longer appears in the widget tree (e.g., whose parent widget no longer " +
                        "includes the widget in its build). This error can occur when code calls " +
                        "setState() from a timer or an animation callback. The preferred solution is " +
                        "to cancel the timer or stop listening to the animation in the dispose() " +
                        "callback. Another solution is to check the \"mounted\" property of this " +
                        "object before calling setState() to ensure the object is still in the " +
                        "tree.\n" +
                        "This error might indicate a memory leak if setState() is being called " +
                        "because another object is retaining a reference to this State object " +
                        "after it has been removed from the tree. To avoid memory leaks, " +
                        "consider breaking the reference to this object during dispose()."
                    );
                }

                if (this._debugLifecycleState == _StateLifecycle.created && !this.mounted) {
                    throw new UIWidgetsError(
                        "setState() called in constructor: " + this + "\n" +
                        "This happens when you call setState() on a State object for a widget that " +
                        "hasn\"t been inserted into the widget tree yet. It is not necessary to call " +
                        "setState() in the constructor, since the state is already assumed to be dirty " +
                        "when it is initially created."
                    );
                }

                return true;
            });

            fn();
            this._element.markNeedsBuild();
        }

        protected virtual void deactivate() {
        }

        protected void dispose() {
            D.assert(this._debugLifecycleState == _StateLifecycle.ready);
            D.assert(() => {
                this._debugLifecycleState = _StateLifecycle.defunct;
                return true;
            });
        }

        public abstract Widget build(BuildContext context);

        protected virtual void didChangeDependencies() {
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);

            D.assert(() => {
                properties.add(new EnumProperty<_StateLifecycle>("lifecycle state", _debugLifecycleState,
                    defaultValue: _StateLifecycle.ready));
                return true;
            });

            properties.add(new ObjectFlagProperty<StatefulWidget>("_widget", this._widget, ifNull: "no widget"));
            properties.add(new ObjectFlagProperty<StatefulElement>("_element", this._element, ifNull: "not mounted"));
        }
    }

    public abstract class State<T> : State where T : StatefulWidget {
        public new T widget {
            get { return (T) base.widget; }
        }

        protected bool _debugTypesAreRight(Widget widget) {
            return widget is T;
        }
    }

    public abstract class ProxyWidget : Widget {
        protected ProxyWidget(Key key = null, Widget child = null) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;
    }

    public abstract class ParentDataWidget<T> : ProxyWidget where T : RenderObjectWidget {
        public ParentDataWidget(Key key = null, Widget child = null)
            : base(key: key, child: child) {
        }

        protected override Element createElement() {
            return new ParentDataElement<T>(this);
        }

        protected virtual bool debugIsValidAncestor(RenderObjectWidget ancestor) {
            D.assert(typeof(T) != typeof(RenderObjectWidget));
            return ancestor is T;
        }

        protected virtual string debugDescribeInvalidAncestorChain(
            String description = null, String ownershipChain = null, bool foundValidAncestor = false,
            IEnumerable<Widget> badAncestors = null
        ) {
            D.assert(typeof(T) != typeof(RenderObjectWidget));

            String result;
            if (!foundValidAncestor) {
                result = string.Format("{0} widgets must be placed inside {1} widgets.\n" +
                                       "{2} has no {1} ancestor at all.\n", this.GetType(), typeof(T), description);
            } else {
                D.assert(badAncestors != null);
                D.assert(badAncestors.Any());
                result = string.Format("{0} widgets must be placed directly inside {1} widgets.\n" +
                                       "{2} has a {1} ancestor, but there are other widgets between them:\n",
                    this.GetType(), typeof(T), description);

                foreach (Widget ancestor in badAncestors) {
                    if (ancestor.GetType() == this.GetType()) {
                        result += string.Format("- {0} (this is a different {1} than the one with the problem)\n",
                            ancestor, this.GetType());
                    } else {
                        result += string.Format("- {0}\n", ancestor);
                    }
                }

                result += "These widgets cannot come between a " + this.GetType() + " and its " + typeof(T) + ".\n";
            }

            result += "The ownership chain for the parent of the offending "
                      + this.GetType() + " was:\n  " + ownershipChain;
            return result;
        }

        protected abstract void applyParentData(RenderObject renderObject);

        protected virtual bool debugCanApplyOutOfTurn() {
            return false;
        }
    }

    public abstract class ProxyWidget : Widget {
        protected ProxyWidget(string key, Widget child) : base(key) {
            this.child = child;
        }

        public Widget child;
    }

    public abstract class InheritedWidget : ProxyWidget {
        protected InheritedWidget(string key, Widget child) : base(key, child) {
        }

        public abstract bool updateShouldNotify(InheritedWidget oldWidget);
    }

    public abstract class RenderObjectWidget : Widget {
        protected RenderObjectWidget(string key) : base(key) {
        }

        public abstract RenderObject createRenderObject(BuildContext context);

        public virtual void updateRenderObject(BuildContext context, RenderObject renderObject) {
        }

        public virtual void didUnmountRenderObject(RenderObject renderObject) {
        }
    }

    public abstract class RootRenderObjectElement : RenderObjectElement {
        protected RootRenderObjectElement(RenderObjectWidget widget) : base(widget) {
        }

        public void assignOwner(BuildOwner owner) {
            _owner = owner;
        }
        
        public override void mount(Element parent, object newSlot) {
            D.assert(parent == null);
            D.assert(newSlot == null);
            base.mount(parent, newSlot);
        }
    }

    public abstract class LeafRenderObjectWidget : RenderObjectWidget {
        protected LeafRenderObjectWidget(string key) : base(key) {
        }

        public override Element createElement() {
            return new LeafRenderObjectElement(this);
        }
    }

    public abstract class SingleChildRenderObjectWidget : RenderObjectWidget {
        protected SingleChildRenderObjectWidget(string key) : base(key) {
        }

        public Widget child;

        public override Element createElement() {
            return new SingleChildRenderObjectElement(this);
        }
    }

    public abstract class Element : BuildContext {
        protected Element(Widget widget) {
            Assert.IsNotNull(widget);
            this._widget = widget;
        }

        public Element _parent;

        public override bool Equals(object obj) {
            return object.ReferenceEquals(this, obj);
        }

        private static int _nextHashCode = 1;
        private readonly int _cachedHash = Element._nextHashCode = (Element._nextHashCode + 1) % 0xffffff;

        public override int GetHashCode() {
            return this._cachedHash;
        }

        public object _slot;

        public object slot {
            get { return this._slot; }
        }

        public int _depth;

        public int depth {
            get { return this._depth; }
        }

        public static int _sort(Element a, Element b) {
            if (a.depth < b.depth)
                return -1;
            if (b.depth < a.depth)
                return 1;
            if (b.dirty && !a.dirty)
                return -1;
            if (a.dirty && !b.dirty)
                return 1;
            return 0;
        }

        public Widget _widget;

        public Widget widget {
            get { return this._widget; }
        }

        public BuildOwner _owner;

        public BuildOwner owner {
            get { return this._owner; }
        }

        public bool _active = false;

        public virtual void _reassemble() {
            markNeedsBuild();
            ElementVisitor visit = null;
            visit = child => { child._reassemble(); };
            visitChildren(visit);
        }

        public RenderObject renderObject {
            get {
                RenderObject result = null;
                ElementVisitor visit = null;
                visit = (element) => {
                    Assert.IsNull(result);
                    if (element is RenderObjectElement) {
                        result = element.renderObject;
                    } else {
                        element.visitChildren(visit);
                    }
                };
                visit(this);
                return result;
            }
        }

        public virtual InheritedWidget inheritFromWidgetOfExactType(Type targetType) {
            InheritedElement ancestor =
                _inheritedWidgets == null ? null : _inheritedWidgets[targetType];
            if (ancestor != null) {
                if (_dependencies == null) {
                    _dependencies = new HashSet<InheritedElement>();
                }
                _dependencies.Add(ancestor);
                ancestor._dependents.Add(this);
                return ancestor.widget;
            }
            _hadUnsatisfiedDependencies = true;
            return null;
        }

        public void visitAncestorElements(Func<Element, bool> visitor) {
        }

        public virtual void visitChildren(ElementVisitor visitor) {
        }

        public void visitChildElements(ElementVisitor visitor) {
            this.visitChildren(visitor);
        }

        public Element updateChild(Element child, Widget newWidget, object newSlot) {
            if (newWidget == null) {
                if (child != null) {
                    this.deactivateChild(child);
                }

                return null;
            }

            if (child != null) {
                if (child.widget == newWidget) {
                    if (child.slot != newSlot) {
                        this.updateSlotForChild(child, newSlot);
                    }

                    return child;
                }

                if (Widget.canUpdate(child.widget, newWidget)) {
                    if (child.slot != newSlot) {
                        this.updateSlotForChild(child, newSlot);
                    }

                    child.update(newWidget);
                    return child;
                }

                this.deactivateChild(child);
            }

            return inflateWidget(newWidget, newSlot);
        }

        public virtual void update(Widget newWidget) {
            this._widget = newWidget;
        }

        public virtual void mount(Element parent, object newSlot) {
            this._parent = parent;
            this._slot = newSlot;
            this._depth = parent != null ? parent.depth + 1 : 1;
            this._active = true;
            if (parent != null) {
                this._owner = parent.owner;
            }

// _updateInheritance();
        }

        void updateSlotForChild(Element child, object newSlot) {
            ElementVisitor visit = null;
            visit = (element) => {
                element._updateSlot(newSlot);
                if (element is RenderObjectElement) {
                    element.visitChildren(visit);
                }
            };
            visit(child);
        }

        void _updateSlot(object newSlot) {
            this._slot = newSlot;
        }

        void _updateDepth(int parentDepth) {
            int expectedDepth = parentDepth + 1;
            if (this._depth < expectedDepth) {
                this._depth = expectedDepth;
                this.visitChildren(child => { child._updateDepth(expectedDepth); });
            }
        }

        public void detachRenderObject() {
            this.visitChildren(child => { child.detachRenderObject(); });
            this._slot = null;
        }

        public void attachRenderObject(object newSlot) {
            Assert.IsNull(_slot);
            this.visitChildren(child => { child.attachRenderObject(newSlot); });
            this._slot = newSlot;
        }

        public Element inflateWidget(Widget newWidget, object newSlot) {
            Element newChild = newWidget.createElement();
            newChild.mount(this, newSlot);
            return newChild;
        }

        public void deactivateChild(Element child) {
            child._parent = null;
            child.detachRenderObject();
            this.owner._inactiveElements.add(child);
        }

        public abstract void forgetChild(Element child);

        public void activate() {
            Assert.IsTrue(!this._active);
            this._active = true;
            if (this._dirty) {
                this.owner.scheduleBuildFor(this);
            }
        }

        public void deactivate() {
            Assert.IsTrue(this._active);
            this._active = false;
        }

        public void unmount() {
        }

        public RenderObject findRenderObject() {
            return this.renderObject;
        }
        
        public Dictionary<Type, InheritedElement> _inheritedWidgets;
        public HashSet<InheritedElement> _dependencies;
        bool _hadUnsatisfiedDependencies = false;

        public void didChangeDependencies() {
            this.markNeedsBuild();
        }

        public void visitAncestorElements(ElementVisitorBool visitor) {
            Element ancestor = this._parent;
            while (ancestor != null && visitor(ancestor)) {
                ancestor = ancestor._parent;
            }
        }

        public bool _dirty;

        public bool dirty {
            get { return this._dirty; }
        }

        public bool _inDirtyList = false;

        public void markNeedsBuild() {
            if (!this._active) {
                return;
            }

            if (this.dirty) {
                return;
            }

            this._dirty = true;
            this.owner.scheduleBuildFor(this);
        }

        public void rebuild() {
            if (!this._active || !this._dirty) {
                return;
            }

            this.performRebuild();
            Assert.IsTrue(!this._dirty);
        }

        public abstract void performRebuild();
    }

    public abstract class ComponentElement : Element {
        protected ComponentElement(Widget widget) : base(widget) {
        }

        public Element _child;

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            this._firstBuild();
        }

        public virtual void _firstBuild() {
            this.rebuild();
        }

        public override void performRebuild() {
            Widget built;
            try {
                built = this.build();
            }
            catch (Exception e) {
                built = null; // ErrorWidget.builder(_debugReportException('building $this', e, stack));
            }
            finally {
                this._dirty = false;
            }

            try {
                this._child = this.updateChild(this._child, built, this.slot);
                Assert.IsNotNull(_child);
            }
            catch (Exception e) {
                built = null; //ErrorWidget.builder(_debugReportException('building $this', e, stack));
                this._child = this.updateChild(null, built, this.slot);
            }
        }

        public abstract Widget build();

        public override void visitChildren(ElementVisitor visitor) {
            if (this._child != null) {
                visitor(this._child);
            }
        }

        public override void forgetChild(Element child) {
            Assert.IsTrue(child == _child);
            this._child = null;
        }
    }

    public class StatelessElement : ComponentElement {
        public StatelessElement(StatelessWidget widget) : base(widget) {
        }

        public new StatelessWidget widget {
            get { return (StatelessWidget) base.widget; }
        }

        public override Widget build() {
            return this.widget.build(this);
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            this._dirty = true;
            this.rebuild();
        }
    }

    public class StatefulElement : ComponentElement {
        public StatefulElement(StatefulWidget widget) : base(widget) {
            _state = widget.createState();
            _state.element = this;
            _state.widget = widget;
        }

        public State state {
            get { return _state; }
        }

        private State _state;

        public override Widget build() {
            return state.build(this);
        }

        public override void _reassemble() {
            state.reassemble();
            base._reassemble();
        }
    }

    public abstract class ProxyElement : ComponentElement {
        protected ProxyElement(Widget widget) : base(widget) {
        }
        
        public ProxyWidget widget {
            get { return (ProxyWidget) base.widget; }
        }

        public override Widget build() {
            return ((ProxyWidget) widget).child;
        }
    }

    public class InheritedElement : ProxyElement {
        public InheritedElement(Widget widget) : base(widget) {
        }
        
        public InheritedWidget widget {
            get { return (InheritedWidget) base.widget; }
        }
        
        public HashSet<Element> _dependents = new HashSet<Element>();

        public void _updateInheritance() {
            Dictionary<Type, InheritedElement> incomingWidgets = _parent == null ? null : _inheritedWidgets;
            if (incomingWidgets != null) {
                _inheritedWidgets = new Dictionary<Type, InheritedElement>(incomingWidgets);
            }
            else {
                _inheritedWidgets = new Dictionary<Type, InheritedElement>();
            }

            _inheritedWidgets[widget.GetType()] = this;
        }

        public void notifyClients(InheritedWidget oldWidget) {
            if (!widget.updateShouldNotify(oldWidget)) return;
            foreach (Element dependent in _dependents) {
                D.assert(() => {
                    Element ancestor = dependent._parent;
                    while (ancestor != this && ancestor != null) {
                        ancestor = ancestor._parent;
                    }

                    return ancestor == this;
                });
                D.assert(dependent._dependencies.Contains(this));
                dependent.didChangeDependencies();
            }
        }
    }

    public abstract class RenderObjectElement : Element {
        protected RenderObjectElement(RenderObjectWidget widget) : base(widget) {
        }

        public new RenderObjectWidget widget {
            get { return (RenderObjectWidget) base.widget; }
        }

        public RenderObject _renderObject;

        public RenderObject renderObject {
            get { return this._renderObject; }
        }

        public RenderObjectElement _ancestorRenderObjectElement;

        public override void mount(Element parent, object newSlot) {
            this._renderObject = this.widget.createRenderObject(this);
            this.attachRenderObject(newSlot);
            this._dirty = false;
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            this.widget.updateRenderObject(this, this.renderObject);
            this._dirty = false;
        }

        public override void performRebuild() {
            this.widget.updateRenderObject(this, this.renderObject);
            this._dirty = false;
        }

        public List<Element> updateChildren(List<Element> oldChildren, List<Widget> newWidgets,
            HashSet<Element> forgottenChildren) {
            return null;
        }
    }

    public interface BuildContext {
        Widget widget { get; }
        InheritedWidget inheritFromWidgetOfExactType(Type targetType);
        void visitAncestorElements(Func<Element, bool> visitor);
    }

    public class BuildOwner {
        public VoidCallback onBuildScheduled;

        public BuildOwner(VoidCallback onBuildScheduled) {
            this.onBuildScheduled = onBuildScheduled;
        }

        public readonly _InactiveElements _inactiveElements = new _InactiveElements();
        public readonly List<Element> _dirtyElements = new List<Element>();
        public bool _scheduledFlushDirtyElements = false;
        public bool _dirtyElementsNeedsResorting = false;

        public void scheduleBuildFor(Element element) {
            if (element._inDirtyList) {
                this._dirtyElementsNeedsResorting = true;
                return;
            }

            if (!this._scheduledFlushDirtyElements && this.onBuildScheduled != null) {
                this._scheduledFlushDirtyElements = true;
                this.onBuildScheduled();
            }

            this._dirtyElements.Add(element);
            element._inDirtyList = true;
        }

        public void buildScope(Element context, VoidCallback callback = null) {
            if (callback == null && this._dirtyElements.Count == 0) {
                return;
            }

            try {
                this._scheduledFlushDirtyElements = true;
                if (callback != null) {
                    this._dirtyElementsNeedsResorting = false;
                    callback();
                }

                this._dirtyElements.Sort(Element._sort);
                this._dirtyElementsNeedsResorting = false;
                int dirtyCount = this._dirtyElements.Count;
                int index = 0;
                while (index < dirtyCount) {
                    this._dirtyElements[index].rebuild();
                    index++;
                    if (dirtyCount < this._dirtyElements.Count || this._dirtyElementsNeedsResorting) {
                        this._dirtyElements.Sort(Element._sort);
                        this._dirtyElementsNeedsResorting = false;
                        dirtyCount = this._dirtyElements.Count;
                        while (index > 0 && this._dirtyElements[index - 1].dirty) {
                            index -= 1;
                        }
                    }
                }
            }
            finally {
                foreach (Element element in this._dirtyElements) {
                    Assert.IsTrue(element._inDirtyList);
                    element._inDirtyList = false;
                }

                this._dirtyElements.Clear();
                this._scheduledFlushDirtyElements = false;
                this._dirtyElementsNeedsResorting = false;
            }
        }

        public void finalizeTree() {
            this._inactiveElements._unmountAll();
        }
    }

    public class LeafRenderObjectElement : RenderObjectElement {
        public LeafRenderObjectElement(LeafRenderObjectWidget widget) : base(widget) {
        }

        public override void forgetChild(Element child) {
        }
    }

    public class SingleChildRenderObjectElement : RenderObjectElement {
        public SingleChildRenderObjectElement(SingleChildRenderObjectWidget widget) : base(widget) {
        }

        public override void forgetChild(Element child) {
        }
    }

    public class _InactiveElements {
        public readonly HashSet<Element> _elements = new HashSet<Element>();

        public void _unmount(Element element) {
            element.visitChildren(child => {
                Assert.IsTrue(child._parent.Equals(element));
                this._unmount(child);
            });
            element.unmount();
        }

        public void _unmountAll() {
            List<Element> elements = this._elements.ToList();
            this._elements.Clear();
            elements.Sort(Element._sort);
            elements.Reverse();
            elements.ForEach(this._unmount);
        }

        public void _deactivateRecursively(Element element) {
            element.deactivate();
            element.visitChildren(this._deactivateRecursively);
        }

        public void add(Element element) {
            if (element._active) {
                this._deactivateRecursively(element);
            }

            this._elements.Add(element);
        }

        public void remove(Element element) {
            this._elements.Remove(element);
        }
    }
}