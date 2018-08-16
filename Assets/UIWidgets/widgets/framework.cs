using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UIWidgets.rendering;
using UIWidgets.ui;
using UnityEngine.Assertions;

namespace UIWidgets.widgets {
    public delegate void ElementVisitor(Element element);
    public delegate bool ElementVisitorBool(Element element);



    public abstract class Widget {
        protected Widget(string key) {
            this._key = key;
        }

        public readonly string _key;

        public string key {
            get { return this._key; }
        }

        public abstract Element createElement();

        public static bool canUpdate(Widget oldWidget, Widget newWidget) {
            return oldWidget.GetType() == newWidget.GetType() && oldWidget.key == newWidget.key;
        }
    }

    public abstract class StatelessWidget : Widget {
        protected StatelessWidget(string key) : base(key) {
        }

        public override Element createElement() {
            return new StatelessElement(this);
        }

        public abstract Widget build(BuildContext context);
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

        public void _firstBuild() {
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