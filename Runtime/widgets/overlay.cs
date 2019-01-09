using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;

namespace Unity.UIWidgets.widgets
{
    public class OverlayEntry
    {
        public OverlayEntry(WidgetBuilder builder = null, bool opaque = false, bool maintainState = false)
        {
            D.assert(builder != null);
            _opaque = opaque;
            _maintainState = maintainState;
        }

        public readonly WidgetBuilder builder;

        bool _opaque;

        public bool opaque
        {
            get { return _opaque; }
            set
            {
                if (_opaque == value)
                {
                    return;
                }

                _opaque = value;
                D.assert(_overlay != null);
                _overlay._didChangeEntryOpacity();
            }
        }

        bool _maintainState;

        public bool maintainState
        {
            get { return _maintainState; }
            set
            {
                if (_maintainState == value)
                {
                    return;
                }

                _maintainState = value;
                D.assert(_overlay != null);
                _overlay._didChangeEntryOpacity();
            }
        }

        internal OverlayState _overlay;

        internal readonly GlobalKey<_OverlayEntryState> _key = new LabeledGlobalKey<_OverlayEntryState>();

        public void remove()
        {
            D.assert(_overlay != null);
            OverlayState overlay = _overlay;
            _overlay = null;
            if (SchedulerBinding.instance.schedulerPhase == SchedulerPhase.persistentCallbacks)
            {
                SchedulerBinding.instance.addPostFrameCallback((duration) => { overlay._remove(this); });
            }
            else
            {
                overlay._remove(this);
            }
        }

        public void markNeedsBuild()
        {
            _key.currentState?._markNeedsBuild();
        }

        public override string ToString()
        {
            return $"{Diagnostics.describeIdentity(this)}(opaque: {opaque}; maintainState: {maintainState})";
        }
    }


    class _OverlayEntry : StatefulWidget
    {
        internal _OverlayEntry(OverlayEntry entry) : base(key: entry._key)
        {
            D.assert(entry != null);
        }

        public readonly OverlayEntry entry;

        public override State createState()
        {
            return new _OverlayEntryState();
        }
    }

    class _OverlayEntryState : State<_OverlayEntry>
    {
        public override Widget build(BuildContext context)
        {
            return widget.entry.builder(context);
        }

        internal void _markNeedsBuild()
        {
            setState(() =>
            {
                /* the state that changed is in the builder */
            });
        }
    }

    public class Overlay : StatefulWidget
    {
        public Overlay(Key key = null, List<OverlayEntry> initialEntries = null) : base(key)
        {
            D.assert(initialEntries != null);
            this.initialEntries = initialEntries;
        }

        public readonly List<OverlayEntry> initialEntries;

        public static OverlayState of(BuildContext context, Widget debugRequiredFor = null)
        {
            OverlayState result = (OverlayState) context.ancestorStateOfType(new TypeMatcher<OverlayState>());
            D.assert(() =>
            {
                if (debugRequiredFor != null && result == null)
                {
                    var additional = context.widget != debugRequiredFor
                        ? $"\nThe context from which that widget was searching for an overlay was:\n  {context}"
                        : "";
                    throw new UIWidgetsError(
                        "No Overlay widget found.\n" +
                        $"{debugRequiredFor.GetType()} widgets require an Overlay widget ancestor for correct operation.\n" +
                        "The most common way to add an Overlay to an application is to include a MaterialApp or Navigator widget in the runApp() call.\n" +
                        "The specific widget that failed to find an overlay was:\n" +
                        $"  {debugRequiredFor}" +
                        $"{additional}"
                    );
                }

                return true;
            });
            return result;
        }

        public override State createState()
        {
            return new OverlayState();
        }
    }

    public class OverlayState : TickerProviderStateMixin<Overlay>
    {
        readonly List<OverlayEntry> _entries = new List<OverlayEntry>();

        public override void initState()
        {
            base.initState();
            insertAll(widget.initialEntries);
        }

        public void insert(OverlayEntry entry, OverlayEntry above = null)
        {
            D.assert(entry._overlay == null);
            D.assert(above == null || (above._overlay == this && _entries.Contains(above)));
            entry._overlay = this;
            setState(() =>
            {
                int index = above == null ? _entries.Count : _entries.IndexOf(above) + 1;
                _entries.Insert(index, entry);
            });
        }

        public void insertAll(ICollection<OverlayEntry> entries, OverlayEntry above = null)
        {
            D.assert(above == null || (above._overlay == this && _entries.Contains(above)));
            if (entries.isEmpty())
                return;
            foreach (OverlayEntry entry in entries)
            {
                D.assert(entry._overlay == null);
                entry._overlay = this;
            }

            setState(() =>
            {
                int index = above == null ? _entries.Count : _entries.IndexOf(above) + 1;
                _entries.InsertRange(index, entries);
            });
        }

        internal void _remove(OverlayEntry entry)
        {
            if (mounted)
            {
                _entries.Remove(entry);
                setState(() =>
                {
                    /* entry was removed */
                });
            }
        }

        public bool debugIsVisible(OverlayEntry entry)
        {
            bool result = false;
            D.assert(_entries.Contains(entry));
            D.assert(() =>
            {
                for (int i = _entries.Count - 1; i > 0; i -= 1)
                {
                    // todo why not including 0?
                    OverlayEntry candidate = _entries[i];
                    if (candidate == entry)
                    {
                        result = true;
                        break;
                    }

                    if (candidate.opaque)
                        break;
                }

                return true;
            });
            return result;
        }

        internal void _didChangeEntryOpacity()
        {
            setState(() => { });
        }

        public override Widget build(BuildContext context)
        {
            var onstageChildren = new List<Widget>();
            var offstageChildren = new List<Widget>();
            var onstage = true;
            for (var i = _entries.Count - 1; i >= 0; i -= 1)
            {
                var entry = _entries[i];
                if (onstage)
                {
                    onstageChildren.Add(new _OverlayEntry(entry));
                    if (entry.opaque)
                    {
                        onstage = false;
                    }
                }
                else if (entry.maintainState)
                {
                    offstageChildren.Add(new TickerMode(enabled: false, child: new _OverlayEntry(entry)));
                }
            }

            onstageChildren.Reverse();
            return new _Theatre(
                onstage: new Stack(
                    fit: StackFit.expand,
                    children: onstageChildren
                ),
                offstage: offstageChildren
            );
        }
    }

    class _Theatre : RenderObjectWidget
    {
        internal _Theatre(Stack onstage = null, List<Widget> offstage = null)
        {
            D.assert(offstage != null);
            D.assert(!offstage.Any((child) => child == null));
            this.onstage = onstage;
            this.offstage = offstage;
        }

        public readonly Stack onstage;

        public readonly List<Widget> offstage;

        public override Element createElement()
        {
            return new _TheatreElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context)
        {
            return new _RenderTheatre();
        }
    }

    class _TheatreElement : RenderObjectElement
    {
        public _TheatreElement(RenderObjectWidget widget) : base(widget)
        {
            D.assert(!WidgetsD.debugChildrenHaveDuplicateKeys(widget, ((_Theatre) widget).offstage));
        }

        public new _Theatre widget
        {
            get { return (_Theatre) base.widget; }
        }

        public new _RenderTheatre renderObject
        {
            get { return (_RenderTheatre) base.renderObject; }
        }

        Element _onstage;
        static readonly object _onstageSlot = new object();

        List<Element> _offstage;
        readonly HashSet<Element> _forgottenOffstageChildren = new HashSet<Element>();


        protected override void insertChildRenderObject(RenderObject child, object slot)
        {
            D.assert(renderObject.debugValidateChild(child));
            if (slot == _onstageSlot)
            {
                D.assert(child is RenderStack);
                renderObject.child = (RenderStack) child;
            }
            else
            {
                D.assert(slot == null || slot is Element);
                renderObject.insert((RenderBox) child, after: (RenderBox) ((Element) slot)?.renderObject);
            }
        }

        protected override void moveChildRenderObject(RenderObject child, object slot)
        {
            if (slot == _onstageSlot)
            {
                renderObject.remove((RenderBox) child);
                D.assert(child is RenderStack);
                renderObject.child = (RenderStack) child;
            }
            else
            {
                D.assert(slot == null || slot is Element);
                if (renderObject.child == child)
                {
                    renderObject.child = null;
                    renderObject.insert((RenderBox) child, after: (RenderBox) ((Element) slot)?.renderObject);
                }
                else
                {
                    renderObject.move((RenderBox) child, after: (RenderBox) ((Element) slot)?.renderObject);
                }
            }
        }

        protected override void removeChildRenderObject(RenderObject child)
        {
            if (renderObject.child == child)
            {
                renderObject.child = null;
            }
            else
            {
                renderObject.remove((RenderBox) child);
            }
        }

        public override void visitChildren(ElementVisitor visitor)
        {
            if (_onstage != null)
                visitor(_onstage);
            foreach (var child in _offstage)
            {
                if (!_forgottenOffstageChildren.Contains(child))
                    visitor(child);
            }
        }

        public override void debugVisitOnstageChildren(ElementVisitor visitor)
        {
            if (_onstage != null)
                visitor(_onstage);
        }


        protected override void forgetChild(Element child)
        {
            if (child == _onstage)
            {
                _onstage = null;
            }
            else
            {
                D.assert(_offstage.Contains(child));
                D.assert(!_forgottenOffstageChildren.Contains(child));
                _forgottenOffstageChildren.Add(child);
            }
        }

        public override void mount(Element parent, object newSlot)
        {
            base.mount(parent, newSlot);
            _onstage = updateChild(_onstage, widget.onstage, _onstageSlot);
            _offstage = new List<Element>(widget.offstage.Count);
            Element previousChild = null;
            for (int i = 0; i < _offstage.Count; i += 1)
            {
                var newChild = inflateWidget(widget.offstage[i], previousChild);
                _offstage[i] = newChild;
                previousChild = newChild;
            }
        }

        public override void update(Widget newWidget)
        {
            base.update(newWidget);
            D.assert(Equals(widget, newWidget));
            _onstage = updateChild(_onstage, widget.onstage, _onstageSlot);
            _offstage = updateChildren(_offstage, widget.offstage, forgottenChildren: _forgottenOffstageChildren);
            _forgottenOffstageChildren.Clear();
        }
    }

    class _RenderTheatre : ContainerRenderObjectMixinRenderProxyBoxMixinRenderObjectWithChildMixinRenderBoxRenderStack<
        RenderBox, StackParentData>
    {
        public override void setupParentData(RenderObject child)
        {
            if (!(child.parentData is StackParentData))
                child.parentData = new StackParentData();
        }

        public override void redepthChildren()
        {
            if (child != null)
                redepthChild(child);
            base.redepthChildren();
        }

        public override void visitChildren(RenderObjectVisitor visitor)
        {
            if (child != null)
                visitor(child);
            base.visitChildren(visitor);
        }

        public override List<DiagnosticsNode> debugDescribeChildren()
        {
            var children = new List<DiagnosticsNode>();

            if (child != null)
                children.Add(child.toDiagnosticsNode(name: "onstage"));

            if (firstChild != null)
            {
                var child = firstChild;

                int count = 1;
                while (true)
                {
                    children.Add(
                        child.toDiagnosticsNode(
                            name: $"offstage {count}",
                            style: DiagnosticsTreeStyle.offstage
                        )
                    );
                    if (child == lastChild)
                        break;
                    var childParentData = (StackParentData) child.parentData;
                    child = childParentData.nextSibling;
                    count += 1;
                }
            }
            else
            {
                children.Add(
                    DiagnosticsNode.message(
                        "no offstage children",
                        style: DiagnosticsTreeStyle.offstage
                    )
                );
            }

            return children;
        }
    }
}