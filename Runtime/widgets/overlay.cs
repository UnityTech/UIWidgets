using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;

namespace Unity.UIWidgets.widgets {
    public class OverlayEntry {
        public OverlayEntry(WidgetBuilder builder = null, bool opaque = false, bool maintainState = false) {
            D.assert(builder != null);
            this._opaque = opaque;
            this._maintainState = maintainState;
            this.builder = builder;
        }

        public readonly WidgetBuilder builder;

        bool _opaque;

        public bool opaque {
            get { return this._opaque; }
            set {
                if (this._opaque == value) {
                    return;
                }

                this._opaque = value;
                D.assert(this._overlay != null);
                this._overlay._didChangeEntryOpacity();
            }
        }

        bool _maintainState;

        public bool maintainState {
            get { return this._maintainState; }
            set {
                if (this._maintainState == value) {
                    return;
                }

                this._maintainState = value;
                D.assert(this._overlay != null);
                this._overlay._didChangeEntryOpacity();
            }
        }

        internal OverlayState _overlay;

        internal readonly GlobalKey<_OverlayEntryState> _key = new LabeledGlobalKey<_OverlayEntryState>();

        public void remove() {
            D.assert(this._overlay != null);
            OverlayState overlay = this._overlay;
            this._overlay = null;
            if (SchedulerBinding.instance.schedulerPhase == SchedulerPhase.persistentCallbacks) {
                SchedulerBinding.instance.addPostFrameCallback((duration) => { overlay._remove(this); });
            }
            else {
                overlay._remove(this);
            }
        }

        public void markNeedsBuild() {
            this._key.currentState?._markNeedsBuild();
        }

        public override string ToString() {
            return $"{Diagnostics.describeIdentity(this)}(opaque: {this.opaque}; maintainState: {this.maintainState})";
        }
    }


    class _OverlayEntry : StatefulWidget {
        internal _OverlayEntry(OverlayEntry entry) : base(key: entry._key) {
            D.assert(entry != null);
            this.entry = entry;
        }

        public readonly OverlayEntry entry;

        public override State createState() {
            return new _OverlayEntryState();
        }
    }

    class _OverlayEntryState : State<_OverlayEntry> {
        public override Widget build(BuildContext context) {
            return this.widget.entry.builder(context);
        }

        internal void _markNeedsBuild() {
            this.setState(() => {
                /* the state that changed is in the builder */
            });
        }
    }

    public class Overlay : StatefulWidget {
        public Overlay(Key key = null, List<OverlayEntry> initialEntries = null) : base(key) {
            D.assert(initialEntries != null);
            this.initialEntries = initialEntries;
        }

        public readonly List<OverlayEntry> initialEntries;

        public static OverlayState of(BuildContext context, Widget debugRequiredFor = null) {
            OverlayState result = (OverlayState) context.ancestorStateOfType(new TypeMatcher<OverlayState>());
            D.assert(() => {
                if (debugRequiredFor != null && result == null) {
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

        public override State createState() {
            return new OverlayState();
        }
    }

    public class OverlayState : TickerProviderStateMixin<Overlay> {
        readonly List<OverlayEntry> _entries = new List<OverlayEntry>();

        public override void initState() {
            base.initState();
            this.insertAll(this.widget.initialEntries);
        }

        internal int _insertionIndex(OverlayEntry below, OverlayEntry above) {
            D.assert(below == null || above == null);
            if (below != null) {
                return this._entries.IndexOf(below);
            }

            if (above != null) {
                return this._entries.IndexOf(above) + 1;
            }

            return this._entries.Count;
        }

        public void insert(OverlayEntry entry, OverlayEntry below = null, OverlayEntry above = null) {
            D.assert(above == null || below == null, () => "Only one of `above` and `below` may be specified.");
            D.assert(above == null || (above._overlay == this && this._entries.Contains(above)),
                () => "The provided entry for `above` is not present in the Overlay.");
            D.assert(below == null || (below._overlay == this && this._entries.Contains(below)),
                () => "The provided entry for `below` is not present in the Overlay.");
            D.assert(!this._entries.Contains(entry), () => "The specified entry is already present in the Overlay.");
            D.assert(entry._overlay == null, () => "The specified entry is already present in another Overlay.");
            entry._overlay = this;
            this.setState(() => { this._entries.Insert(this._insertionIndex(below, above), entry); });
        }

        public void insertAll(ICollection<OverlayEntry> entries, OverlayEntry below = null, OverlayEntry above = null) {
            D.assert(above == null || below == null, () => "Only one of `above` and `below` may be specified.");
            D.assert(above == null || (above._overlay == this && this._entries.Contains(above)),
                () => "The provided entry for `above` is not present in the Overlay.");
            D.assert(below == null || (below._overlay == this && this._entries.Contains(below)),
                () => "The provided entry for `below` is not present in the Overlay.");
            D.assert(entries.All(entry => !this._entries.Contains(entry)),
                () => "One or more of the specified entries are already present in the Overlay.");
            D.assert(entries.All(entry => entry._overlay == null),
                () => "One or more of the specified entries are already present in another Overlay.");
            if (entries.isEmpty()) {
                return;
            }

            foreach (OverlayEntry entry in entries) {
                D.assert(entry._overlay == null);
                entry._overlay = this;
            }

            this.setState(() => {
                this._entries.InsertRange(this._insertionIndex(below, above), entries);
            });
        }

        public void rearrange(IEnumerable<OverlayEntry> newEntries, OverlayEntry below = null, OverlayEntry above = null) {
            List<OverlayEntry> newEntriesList =
                newEntries is List<OverlayEntry> ?(newEntries as List<OverlayEntry>) : newEntries.ToList();
            D.assert(above == null || below == null, () => "Only one of `above` and `below` may be specified.");
            D.assert(above == null || (above._overlay == this && this._entries.Contains(above)),
                () => "The provided entry for `above` is not present in the Overlay.");
            D.assert(below == null || (below._overlay == this && this._entries.Contains(below)),
                () => "The provided entry for `below` is not present in the Overlay.");
            D.assert(newEntriesList.All(entry => !this._entries.Contains(entry)),
                () => "One or more of the specified entries are already present in the Overlay.");
            D.assert(newEntriesList.All(entry => entry._overlay == null),
                () => "One or more of the specified entries are already present in another Overlay.");
            if (newEntriesList.isEmpty()) {
                return;
            }

            if (this._entries.SequenceEqual(newEntriesList)) {
                return;
            }

            HashSet<OverlayEntry> old = new HashSet<OverlayEntry>(this._entries);
            foreach(OverlayEntry entry in newEntriesList) {
                entry._overlay = entry._overlay ?? this;
            }
            this.setState(() => {
                this._entries.Clear();
                this._entries.AddRange(newEntriesList);
                foreach (OverlayEntry entry in newEntriesList) {
                    old.Remove(entry);
                }

                this._entries.InsertRange(this._insertionIndex(below, above), old);
            });
        }

        internal void _remove(OverlayEntry entry) {
            if (this.mounted) {
                this.setState(() => { this._entries.Remove(entry); });
            }
        }

        public bool debugIsVisible(OverlayEntry entry) {
            bool result = false;
            D.assert(this._entries.Contains(entry));
            D.assert(() => {
                for (int i = this._entries.Count - 1; i > 0; i -= 1) {
                    // todo why not including 0?
                    OverlayEntry candidate = this._entries[i];
                    if (candidate == entry) {
                        result = true;
                        break;
                    }

                    if (candidate.opaque) {
                        break;
                    }
                }

                return true;
            });
            return result;
        }

        internal void _didChangeEntryOpacity() {
            this.setState(() => { });
        }

        public override Widget build(BuildContext context) {
            var onstageChildren = new List<Widget>();
            var offstageChildren = new List<Widget>();
            var onstage = true;
            for (var i = this._entries.Count - 1; i >= 0; i -= 1) {
                var entry = this._entries[i];
                if (onstage) {
                    onstageChildren.Add(new _OverlayEntry(entry));
                    if (entry.opaque) {
                        onstage = false;
                    }
                }
                else if (entry.maintainState) {
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

    class _Theatre : RenderObjectWidget {
        internal _Theatre(Stack onstage = null, List<Widget> offstage = null) {
            D.assert(offstage != null);
            D.assert(!offstage.Any((child) => child == null));
            this.onstage = onstage;
            this.offstage = offstage;
        }

        public readonly Stack onstage;

        public readonly List<Widget> offstage;

        public override Element createElement() {
            return new _TheatreElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderTheatre();
        }
    }

    class _TheatreElement : RenderObjectElement {
        public _TheatreElement(RenderObjectWidget widget) : base(widget) {
            D.assert(!WidgetsD.debugChildrenHaveDuplicateKeys(widget, ((_Theatre) widget).offstage));
        }

        public new _Theatre widget {
            get { return (_Theatre) base.widget; }
        }

        public new _RenderTheatre renderObject {
            get { return (_RenderTheatre) base.renderObject; }
        }

        Element _onstage;
        static readonly object _onstageSlot = new object();

        List<Element> _offstage;
        readonly HashSet<Element> _forgottenOffstageChildren = new HashSet<Element>();


        protected override void insertChildRenderObject(RenderObject child, object slot) {
            D.assert(this.renderObject.debugValidateChild(child));
            if (slot == _onstageSlot) {
                D.assert(child is RenderStack);
                this.renderObject.child = (RenderStack) child;
            }
            else {
                D.assert(slot == null || slot is Element);
                this.renderObject.insert((RenderBox) child, after: (RenderBox) ((Element) slot)?.renderObject);
            }
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            if (slot == _onstageSlot) {
                this.renderObject.remove((RenderBox) child);
                D.assert(child is RenderStack);
                this.renderObject.child = (RenderStack) child;
            }
            else {
                D.assert(slot == null || slot is Element);
                if (this.renderObject.child == child) {
                    this.renderObject.child = null;
                    this.renderObject.insert((RenderBox) child, after: (RenderBox) ((Element) slot)?.renderObject);
                }
                else {
                    this.renderObject.move((RenderBox) child, after: (RenderBox) ((Element) slot)?.renderObject);
                }
            }
        }

        protected override void removeChildRenderObject(RenderObject child) {
            if (this.renderObject.child == child) {
                this.renderObject.child = null;
            }
            else {
                this.renderObject.remove((RenderBox) child);
            }
        }

        public override void visitChildren(ElementVisitor visitor) {
            if (this._onstage != null) {
                visitor(this._onstage);
            }

            foreach (var child in this._offstage) {
                if (!this._forgottenOffstageChildren.Contains(child)) {
                    visitor(child);
                }
            }
        }

        public override void debugVisitOnstageChildren(ElementVisitor visitor) {
            if (this._onstage != null) {
                visitor(this._onstage);
            }
        }


        protected override void forgetChild(Element child) {
            if (child == this._onstage) {
                this._onstage = null;
            }
            else {
                D.assert(this._offstage.Contains(child));
                D.assert(!this._forgottenOffstageChildren.Contains(child));
                this._forgottenOffstageChildren.Add(child);
            }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            this._onstage = this.updateChild(this._onstage, this.widget.onstage, _onstageSlot);
            this._offstage = new List<Element>(this.widget.offstage.Count);
            Element previousChild = null;
            for (int i = 0; i < this._offstage.Count; i += 1) {
                var newChild = this.inflateWidget(this.widget.offstage[i], previousChild);
                this._offstage[i] = newChild;
                previousChild = newChild;
            }
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(Equals(this.widget, newWidget));
            this._onstage = this.updateChild(this._onstage, this.widget.onstage, _onstageSlot);
            this._offstage = this.updateChildren(this._offstage, this.widget.offstage,
                forgottenChildren: this._forgottenOffstageChildren);
            this._forgottenOffstageChildren.Clear();
        }
    }

    class _RenderTheatre :
        ContainerRenderObjectMixinRenderProxyBoxMixinRenderObjectWithChildMixinRenderBoxRenderStack<
            RenderBox, StackParentData> {
        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is StackParentData)) {
                child.parentData = new StackParentData();
            }
        }

        public override void redepthChildren() {
            if (this.child != null) {
                this.redepthChild(this.child);
            }

            base.redepthChildren();
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            if (this.child != null) {
                visitor(this.child);
            }

            base.visitChildren(visitor);
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();

            if (this.child != null) {
                children.Add(this.child.toDiagnosticsNode(name: "onstage"));
            }

            if (this.firstChild != null) {
                var child = this.firstChild;

                int count = 1;
                while (true) {
                    children.Add(
                        child.toDiagnosticsNode(
                            name: $"offstage {count}",
                            style: DiagnosticsTreeStyle.offstage
                        )
                    );
                    if (child == this.lastChild) {
                        break;
                    }

                    var childParentData = (StackParentData) child.parentData;
                    child = childParentData.nextSibling;
                    count += 1;
                }
            }
            else {
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