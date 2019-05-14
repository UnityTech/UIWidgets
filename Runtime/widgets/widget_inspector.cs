using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate Widget InspectorSelectButtonBuilder(BuildContext context, VoidCallback onPressed);

    class _SerializeConfig {
        public _SerializeConfig(string groupName, bool summaryTree = false,
            int subtreeDepth = 1, List<Diagnosticable> pathToInclude = null,
            bool includeProperties = false, bool expandPropertyValues = true) {
            this.groupName = groupName;
            this.summaryTree = summaryTree;
            this.subtreeDepth = subtreeDepth;
            this.pathToInclude = pathToInclude;
            this.includeProperties = includeProperties;
            this.expandPropertyValues = expandPropertyValues;
        }

        public static _SerializeConfig merge(_SerializeConfig from, int? subtreeDepth = null,
            List<Diagnosticable> pathToInclude = null) {
            return new _SerializeConfig(from.groupName, from.summaryTree, subtreeDepth ?? from.subtreeDepth,
                pathToInclude ?? from.pathToInclude, from.includeProperties, from.expandPropertyValues);
        }

        public readonly string groupName;
        public readonly bool summaryTree;
        public readonly int subtreeDepth;
        public readonly List<Diagnosticable> pathToInclude;
        public readonly bool includeProperties;
        public readonly bool expandPropertyValues;
    }

    public delegate void InspectorSelectionChangedCallback();

    public delegate void InspectorShowCallback();

    public delegate void DeveloperInspect();

    class _InspectorReferenceData {
        public readonly object obj;
        public int count = 1;

        public _InspectorReferenceData(object obj) {
            this.obj = obj;
        }
    }

    public class WidgetInspectorService {
        public readonly WidgetsBinding widgetsBinding;
        bool _debugShowInspector;

        readonly Dictionary<string, HashSet<_InspectorReferenceData>> _groups =
            new Dictionary<string, HashSet<_InspectorReferenceData>>();

        readonly Dictionary<string, _InspectorReferenceData> _idToReferenceData =
            new Dictionary<string, _InspectorReferenceData>();

        readonly Dictionary<object, string> _objectToId = new Dictionary<object, string>();
        int _nextId = 0;
        public readonly InspectorSelection selection = new InspectorSelection();
        public InspectorSelectionChangedCallback selectionChangedCallback;
        public DeveloperInspect developerInspect;
        public InspectorShowCallback inspectorShowCallback;

        public static WidgetInspectorService instance {
            get { return WidgetsBinding.instance.widgetInspectorService; }
        }

        public WidgetInspectorService(WidgetsBinding widgetsBinding) {
            this.widgetsBinding = widgetsBinding;
        }


//        private static WidgetInspectorService _instance;
//
//        public WidgetInspectorService instance
//        {
//            get { return _instance; }
//        }

        public Dictionary<string, object> getRootWidget(string groupName) {
            return this._nodeToJson(this.widgetsBinding.renderViewElement.toDiagnosticsNode(),
                new _SerializeConfig(groupName));
        }

        public Dictionary<string, object> getRootWidgetSummaryTree(string groupName) {
            return this._nodeToJson(this.widgetsBinding.renderViewElement.toDiagnosticsNode(),
                new _SerializeConfig(groupName, subtreeDepth: 1000000, summaryTree: true));
        }

        public Dictionary<string, object> getRootRenderObject(string groupName) {
            return this._nodeToJson(this.widgetsBinding.renderView.toDiagnosticsNode(),
                new _SerializeConfig(groupName: groupName, subtreeDepth: 1000000));
        }

        public Dictionary<string, object> getDetailsSubtree(string id, string groupName) {
            var root = this.toObject(id) as DiagnosticsNode;
            if (root == null) {
                return null;
            }

            return this._nodeToJson(root,
                new _SerializeConfig(groupName, summaryTree: false, subtreeDepth: 2, includeProperties: true));
        }

        public bool setSelectionById(string id, string groupName = "") {
            return this.setSelection(this.toObject(id), groupName);
        }


        public Dictionary<string, object> getSelectedRenderObject(string previousSelectionId, string groupName) {
            DiagnosticsNode previousSelection = this.toObject(previousSelectionId) as DiagnosticsNode;
            RenderObject current = this.selection == null ? null : this.selection.current;
            return this._nodeToJson(
                current == (previousSelection == null ? null : previousSelection.valueObject)
                    ? previousSelection
                    : (current == null ? null : current.toDiagnosticsNode()),
                new _SerializeConfig(groupName: groupName));
        }


        public Dictionary<string, object> getSelectedWidget(string previousSelectionId, string groupName) {
            DiagnosticsNode previousSelection = (DiagnosticsNode) this.toObject(previousSelectionId);
            Element current = null;
            if (this.selection != null) {
                current = this.selection.currentElement;
            }

            return this._nodeToJson(
                current == (previousSelection == null ? null : previousSelection.valueObject)
                    ? previousSelection
                    : (current == null ? null : current.toDiagnosticsNode()),
                new _SerializeConfig(groupName: groupName));
        }

        public Dictionary<string, object> getSelectedSummaryWidget(string previousSelectionId, string groupName) {
            return this.getSelectedWidget(previousSelectionId, groupName);
        }

        public bool debugShowInspector {
            get { return this._debugShowInspector; }
            set {
                var old = this._debugShowInspector;
                this._debugShowInspector = value;
                if (this._debugShowInspector != old && this.inspectorShowCallback != null) {
                    this.inspectorShowCallback();
                }
            }
        }

        public void disposeGroup(string groupName) {
            HashSet<_InspectorReferenceData> references;
            this._groups.TryGetValue(groupName, out references);
            this._groups.Remove(groupName);
            if (references != null) {
                foreach (var r in references) {
                    this._decrementReferenceCount(r);
                }
            }


            D.assert(() => {
//                var groupNames = _groups.Select((entry) => string.Format("groupName={0} count={1}", entry.Key, entry.Value.Count));
//                Debug.LogFormat("groups {0} idsCount={1} objectsCount={2}", string.Join(",", groupNames.ToArray()),
//                    _idToReferenceData.Count, _objectToId.Count);
                return true;
            });
        }

        protected bool setSelection(object obj, string groupName = "") {
            if (obj is Element || obj is RenderObject) {
                if (obj is Element) {
                    if (ReferenceEquals(obj, this.selection.currentElement)) {
                        return false;
                    }

                    this.selection.currentElement = (Element) obj;
                }
                else {
                    if (obj == this.selection.current) {
                        return false;
                    }

                    this.selection.current = (RenderObject) obj;
                }

                if (this.selectionChangedCallback != null) {
                    if (WidgetsBinding.instance.schedulerPhase == SchedulerPhase.idle) {
                        this.selectionChangedCallback();
                    }
                    else {
                        // todo schedule task ?
                        WidgetsBinding.instance.scheduleFrameCallback(
                            duration => { this.selectionChangedCallback(); }
                        );
                    }
                }

                return true;
            }

            return false;
        }

        Dictionary<string, object> _nodeToJson(DiagnosticsNode node, _SerializeConfig config) {
            if (node == null) {
                return null;
            }

            var ret = node.toJsonMap();
            var value = node.valueObject;
            ret["objectId"] = this.toId(node, config.groupName);
            ret["valueId"] = this.toId(value, config.groupName);

            if (config.summaryTree) {
                ret["summaryTree"] = config.summaryTree;
            }

            var createdByLocalProject = true; // todo;
            if (config.subtreeDepth > 0 || (config.pathToInclude != null && config.pathToInclude.Count > 0)) {
                ret["children"] = this._nodesToJson(this._getChildrenFiltered(node, config), config);
            }

            if (config.includeProperties) {
                ret["properties"] = this._nodesToJson(
                    node.getProperties().Where((n) =>
                        !n.isFiltered(createdByLocalProject ? DiagnosticLevel.fine : DiagnosticLevel.info)).ToList(),
                    new _SerializeConfig(groupName: config.groupName, subtreeDepth: 1, expandPropertyValues: true)
                );
            }

            var typeDef = typeof(DiagnosticsProperty<>);
            var nodeType = node.GetType();
            if (nodeType.IsGenericType && nodeType.GetGenericTypeDefinition() == typeDef) {
                if (value is Color) {
                    ret["valueProperties"] = new Dictionary<string, object> {
                        {"red", ((Color) value).red},
                        {"green", ((Color) value).green},
                        {"blue", ((Color) value).blue},
                        {"alpha", ((Color) value).alpha},
                    };
                }
                else if (value is IconData) {
                    ret["valueProperties"] = new Dictionary<string, object> {
                        {"codePoint", ((IconData) value).codePoint}
                    };
                }

                if (config.expandPropertyValues && value is Diagnosticable) {
                    ret["properties"] = this._nodesToJson(
                        ((Diagnosticable) value).toDiagnosticsNode().getProperties()
                        .Where(n => !n.isFiltered(DiagnosticLevel.info)).ToList(),
                        new _SerializeConfig(groupName: config.groupName, subtreeDepth: 0, expandPropertyValues: false)
                    );
                }
            }

            return ret;
        }

        List<Dictionary<string, object>> _nodesToJson(List<DiagnosticsNode> nodes, _SerializeConfig config) {
            if (nodes == null) {
                return new List<Dictionary<string, object>>();
            }

            return nodes.Select(node => {
                if (config.pathToInclude != null && config.pathToInclude.Count > 0) {
                    if (config.pathToInclude[0] == node.valueObject) {
                        return this._nodeToJson(node, _SerializeConfig.merge(config,
                            pathToInclude: config.pathToInclude.GetRange(1, config.pathToInclude.Count - 1)));
                    }
                    else {
                        return this._nodeToJson(node, _SerializeConfig.merge(config));
                    }
                }

                return this._nodeToJson(node,
                    config.summaryTree || config.subtreeDepth > 1 || this._shouldShowInSummaryTree(node)
                        ? _SerializeConfig.merge(config, subtreeDepth: config.subtreeDepth - 1)
                        : config
                );
            }).ToList();
        }

        public List<Dictionary<string, object>> getProperties(string diagnosticsNodeId, string groupName) {
            var node = this.toObject(diagnosticsNodeId) as DiagnosticsNode;
            return this._nodesToJson(node == null ? new List<DiagnosticsNode>() : node.getProperties(),
                new _SerializeConfig(groupName: groupName));
        }

        List<DiagnosticsNode> _getChildrenFiltered(DiagnosticsNode node,
            _SerializeConfig config) {
            var children = new List<DiagnosticsNode>();
            foreach (var child in node.getChildren()) {
                if (!config.summaryTree || this._shouldShowInSummaryTree(child)) {
                    children.Add(child);
                }
                else {
                    children.AddRange(this._getChildrenFiltered(child, config));
                }
            }

            return children;
        }

        bool _shouldShowInSummaryTree(DiagnosticsNode node) {
            var value = node.valueObject;
            if (!(value is Diagnosticable)) {
                return true;
            }

            if (!(value is Element) || !this.isWidgetCreationTracked()) {
                return true;
            }

            return this._isValueCreatedByLocalProject(value);
        }

        string toId(object obj, string groupName) {
            if (obj == null) {
                return null;
            }

            HashSet<_InspectorReferenceData> group;
            this._groups.TryGetValue(groupName, out group);
            if (group == null) {
                group = new HashSet<_InspectorReferenceData>();
                this._groups.Add(groupName, group);
            }

            string id;
            this._objectToId.TryGetValue(obj, out id);

            _InspectorReferenceData referenceData;
            if (id == null) {
                id = $"inspector-{this._nextId}";
                this._nextId++;
                this._objectToId[obj] = id;
                referenceData = new _InspectorReferenceData(obj);
                this._idToReferenceData[id] = referenceData;
                group.Add(referenceData);
            }
            else {
                referenceData = this._idToReferenceData[id];
                if (group.Add(referenceData)) {
                    referenceData.count += 1;
                }
            }

            return id;
        }

        object toObject(string id) {
            if (id == null) {
                return null;
            }

            _InspectorReferenceData data;
            this._idToReferenceData.TryGetValue(id, out data);
            if (data == null) {
                throw new UIWidgetsError("Id does not exist.");
            }

            return data.obj;
        }

        bool isWidgetCreationTracked() {
            return false; //todo
        }

        bool _isValueCreatedByLocalProject(object value) {
            return true; // todo
        }

        void _decrementReferenceCount(_InspectorReferenceData reference) {
            reference.count -= 1;
            D.assert(reference.count >= 0);
            if (reference.count == 0) {
                string id;
                this._objectToId.TryGetValue(reference.obj, out id);
                D.assert(id != null);
                this._objectToId.Remove(reference.obj);
                this._idToReferenceData.Remove(id);
            }
        }
    }

    public class WidgetInspector : StatefulWidget {
        public readonly Widget child;

        public WidgetInspector(Key key, Widget child, InspectorSelectButtonBuilder selectButtonBuilder) : base(key) {
            D.assert(child != null);
            this.child = child;
            this.selectButtonBuilder = selectButtonBuilder;
        }

        public readonly InspectorSelectButtonBuilder selectButtonBuilder;

        public override State createState() {
            return new _WidgetInspectorState(WidgetsBinding.instance.widgetInspectorService.selection);
        }
    }


    class _WidgetInspectorState : State<WidgetInspector>, WidgetsBindingObserver {
        public _WidgetInspectorState(InspectorSelection selection) {
            this.selection = selection;
        }

        Offset _lastPointerLocation;
        public readonly InspectorSelection selection;
        public bool isSelectMode = true;
        readonly GlobalKey _ignorePointerKey = GlobalKey.key();
        const float _edgeHitMargin = 2.0f;
        const float _kOffScreenMargin = 1.0f;
        const float _kScreenEdgeMargin = 10.0f;
        const float _kTooltipPadding = 5.0f;
        const float _kInspectButtonMargin = 10.0f;

        public override void initState() {
            base.initState();
            WidgetInspectorService.instance.selectionChangedCallback += this._selectionChangedCallback;
        }

        public override void dispose() {
            WidgetInspectorService.instance.selectionChangedCallback -= this._selectionChangedCallback;
            base.dispose();
        }

        bool _hitTestHelper(
            List<RenderObject> hits,
            List<RenderObject> edgeHits,
            Offset position,
            RenderObject renderObject,
            Matrix3 transform
        ) {
            var hit = false;

            var inverse = Matrix3.I();
            var invertible = transform.invert(inverse);
            if (!invertible) {
                return false;
            }

            var localPosition = inverse.mapPoint(position);

            List<DiagnosticsNode> children = renderObject.debugDescribeChildren();
            for (int i = children.Count - 1; i >= 0; --i) {
                DiagnosticsNode diagnostics = children[i];
                D.assert(diagnostics != null);
                if (diagnostics.style == DiagnosticsTreeStyle.offstage ||
                    (!(diagnostics.valueObject is RenderObject))) {
                    continue;
                }

                RenderObject child = (RenderObject) diagnostics.valueObject;
                Rect paintClip = renderObject.describeApproximatePaintClip(child);
                if (paintClip != null && !paintClip.contains(localPosition)) {
                    continue;
                }

                var childTransform = new Matrix3(transform);
                renderObject.applyPaintTransform(child, childTransform);
                if (this._hitTestHelper(hits, edgeHits, position, child, childTransform)) {
                    hit = true;
                }
            }

            Rect bounds = renderObject.semanticBounds;
            if (bounds.contains(localPosition)) {
                hit = true;
                if (!bounds.deflate(_edgeHitMargin).contains(localPosition)) {
                    edgeHits.Add(renderObject);
                }
            }

            if (hit) {
                hits.Add(renderObject);
            }

            return hit;
        }

        List<RenderObject> hitTest(Offset position, RenderObject root) {
            List<RenderObject> regularHits = new List<RenderObject>();
            List<RenderObject> edgeHits = new List<RenderObject>();

            this._hitTestHelper(regularHits, edgeHits, position, root, root.getTransformTo(null));
            regularHits.Sort(CompareByArea);
            HashSet<RenderObject> hits = new HashSet<RenderObject>(edgeHits);
            foreach (var obj in regularHits) {
                if (!hits.Contains(obj)) {
                    hits.Add(obj);
                    edgeHits.Add(obj);
                }
            }

            return edgeHits;
        }

        void _inspectAt(Offset position) {
            if (!this.isSelectMode) {
                return;
            }

            RenderIgnorePointer ignorePointer =
                (RenderIgnorePointer) this._ignorePointerKey.currentContext.findRenderObject();
            RenderObject userRender = ignorePointer.child;
            List<RenderObject> selected = this.hitTest(position, userRender);
            this.setState(() => { this.selection.candidates = selected; });
        }

        void _handlePanDown(DragDownDetails evt) {
            this._lastPointerLocation = evt.globalPosition;
            this._inspectAt(evt.globalPosition);
        }

        void _handlePanUpdate(DragUpdateDetails evt) {
            this._lastPointerLocation = evt.globalPosition;
            this._inspectAt(evt.globalPosition);
        }

        void _handlePanEnd(DragEndDetails details) {
            Rect bounds =
                (Offset.zero & (Window.instance.physicalSize / Window.instance.devicePixelRatio)).deflate(
                    _kOffScreenMargin);
            if (!bounds.contains(this._lastPointerLocation)) {
                this.setState(() => { this.selection.clear(); });
            }
        }

        void _handleTap() {
            if (!this.isSelectMode) {
                return;
            }

            if (this._lastPointerLocation != null) {
                this._inspectAt(this._lastPointerLocation);

                if (this.selection != null) {
                    if (WidgetInspectorService.instance.developerInspect != null) {
                        WidgetInspectorService.instance.developerInspect();
                    }
                }
            }

            this.setState(() => {
                if (this.widget.selectButtonBuilder != null) {
                    this.isSelectMode = false;
                }
            });
        }

        void _handleEnableSelect() {
            this.setState(() => { this.isSelectMode = true; });
        }

        public override Widget build(BuildContext context) {
            List<Widget> children = new List<Widget>();
            children.Add(new GestureDetector(
                onTap: this._handleTap,
                onPanDown: this._handlePanDown,
                onPanEnd: this._handlePanEnd,
                onPanUpdate: this._handlePanUpdate,
                behavior: HitTestBehavior.opaque,
                child: new IgnorePointer(
                    ignoring: this.isSelectMode,
                    key: this._ignorePointerKey,
                    child: this.widget.child
                )
            ));

            if (!this.isSelectMode && this.widget.selectButtonBuilder != null) {
                children.Add(new Positioned(
                    left: _kInspectButtonMargin,
                    bottom: _kInspectButtonMargin,
                    child: this.widget.selectButtonBuilder(context, this._handleEnableSelect)
                ));
            }

            children.Add(new _InspectorOverlay(null, this.selection));
            return new Stack(children: children);
        }

        public void didChangeMetrics() {
        }

        public void didChangeTextScaleFactor() {
        }

        public void didChangePlatformBrightness() {
        }

        public void didChangeLocales(List<Locale> locale) {
        }

        public IPromise<bool> didPopRoute() {
            return Promise<bool>.Resolved(false);
        }

        public IPromise<bool> didPushRoute(string route) {
            return Promise<bool>.Resolved(false);
        }

        void _selectionChangedCallback() {
            this.setState(() => { });
        }

        static int CompareByArea(RenderObject o1, RenderObject o2) {
            return _area(o1).CompareTo(_area(o2));
        }

        static float _area(RenderObject obj) {
            var bounds = obj.semanticBounds;
            Size size = null;
            if (bounds != null) {
                size = bounds.size;
            }

            return size == null ? float.PositiveInfinity : size.width * size.height;
        }
    }


    public class InspectorSelection {
        RenderObject _current;
        Element _currentElement;
        List<RenderObject> _candidates = new List<RenderObject>();

        public List<RenderObject> candidates {
            get { return this._candidates; }
            set {
                this._candidates = value;
                this._index = 0;
                this._computeCurrent();
            }
        }

        int _index = 0;

        public int index {
            get { return this._index; }
            set {
                this._index = value;
                this._computeCurrent();
            }
        }

        public void clear() {
            this.candidates = new List<RenderObject>();
        }

        public RenderObject current {
            get { return this._current; }
            set {
                if (this._current != value) {
                    this._current = value;
                    _DebugCreator creator = value.debugCreator as _DebugCreator;
                    this._currentElement = creator.element;
                }
            }
        }

        public Element currentElement {
            get { return this._currentElement; }
            set {
                if (!ReferenceEquals(this.currentElement, value)) {
                    this._currentElement = value;
                    this._current = value.findRenderObject();
                }
            }
        }

        void _computeCurrent() {
            if (this._index < this.candidates.Count) {
                this._current = this.candidates[this.index];
                this._currentElement = ((_DebugCreator) this._current.debugCreator).element;
            }
            else {
                this._current = null;
                this._currentElement = null;
            }
        }


        public bool active {
            get { return this._current != null && this._current.attached; }
        }
    }

    class _InspectorOverlay : LeafRenderObjectWidget {
        public _InspectorOverlay(Key key, InspectorSelection selection) : base(key) {
            this.selection = selection;
        }

        public readonly InspectorSelection selection;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderInspectorOverlay(this.selection);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((_RenderInspectorOverlay) renderObject).selection = this.selection;
        }
    }

    class _RenderInspectorOverlay : RenderBox {
        public _RenderInspectorOverlay(InspectorSelection selection) {
            D.assert(selection != null);
            this._selection = selection;
        }

        InspectorSelection _selection;

        public InspectorSelection selection {
            get { return this._selection; }
            set {
                if (value != this._selection) {
                    this._selection = value;
                    this.markNeedsPaint();
                }
            }
        }

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        protected override void performResize() {
            this.size = this.constraints.constrain(new Size(float.PositiveInfinity, float.PositiveInfinity));
        }

        public override void paint(PaintingContext context, Offset offset) {
            D.assert(this.needsCompositing);
            context.addLayer(new _InspectorOverlayLayer(
                Rect.fromLTWH(offset.dx, offset.dy, this.size.width, this.size.height), this.selection
            ));
        }
    }

    class _TransformedRect : IEquatable<_TransformedRect> {
        public readonly Rect rect;
        public readonly Matrix3 transform;

        public _TransformedRect(RenderObject obj) {
            this.rect = obj.semanticBounds;
            this.transform = obj.getTransformTo(null);
        }

        public bool Equals(_TransformedRect other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.rect, other.rect) && this.transform.Equals(other.transform);
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

            return this.Equals((_TransformedRect) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.rect != null ? this.rect.GetHashCode() : 0) * 397) ^ this.transform.GetHashCode();
            }
        }

        public static bool operator ==(_TransformedRect left, _TransformedRect right) {
            return Equals(left, right);
        }

        public static bool operator !=(_TransformedRect left, _TransformedRect right) {
            return !Equals(left, right);
        }
    }

    class _InspectorOverlayRenderState : IEquatable<_InspectorOverlayRenderState> {
        public readonly Rect overlayRect;
        public readonly _TransformedRect selected;
        public readonly List<_TransformedRect> candidates;
        public readonly string tooltip;
        public readonly TextDirection textDirection;

        public _InspectorOverlayRenderState(Rect overlayRect, _TransformedRect selected,
            List<_TransformedRect> candidates, string tooltip, TextDirection textDirection) {
            this.overlayRect = overlayRect;
            this.selected = selected;
            this.candidates = candidates;
            this.tooltip = tooltip;
            this.textDirection = textDirection;
        }

        public bool Equals(_InspectorOverlayRenderState other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.overlayRect, other.overlayRect) && Equals(this.selected, other.selected)
                                                               && string.Equals(this.tooltip, other.tooltip) &&
                                                               this.textDirection == other.textDirection
                                                               && (this.candidates == other.candidates ||
                                                                   (this.candidates != null &&
                                                                    this.candidates.SequenceEqual(
                                                                        other.candidates)));
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

            return this.Equals((_InspectorOverlayRenderState) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.overlayRect != null ? this.overlayRect.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.selected != null ? this.selected.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.candidates != null
                               ? this.candidates.Aggregate(0,
                                   (hash, rect) => (hash * 397) ^ (rect == null ? 0 : rect.GetHashCode()))
                               : 0);
                hashCode = (hashCode * 397) ^ (this.tooltip != null ? this.tooltip.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) this.textDirection;
                return hashCode;
            }
        }

        public static bool operator ==(_InspectorOverlayRenderState left, _InspectorOverlayRenderState right) {
            return Equals(left, right);
        }

        public static bool operator !=(_InspectorOverlayRenderState left, _InspectorOverlayRenderState right) {
            return !Equals(left, right);
        }
    }


    class _InspectorOverlayLayer : Layer {
        // const int _kMaxTooltipLines = 5;
        // private static Color _kTooltipBackgroundColor = Color.fromARGB(230, 60, 60, 60);
        static Color _kHighlightedRenderObjectFillColor = Color.fromARGB(128, 128, 128, 255);
        static Color _kHighlightedRenderObjectBorderColor = Color.fromARGB(128, 64, 64, 128);

        public _InspectorOverlayLayer(Rect overlayRect, InspectorSelection selection) {
            this.overlayRect = overlayRect;
            this.selection = selection;
        }

        public InspectorSelection selection;
        public readonly Rect overlayRect;
        _InspectorOverlayRenderState _lastState;

        public Picture _picture;
        // public  TextPainter _textPainter;
        // public float _textPainterMaxWidth;

        internal override S find<S>(Offset regionOffset) {
            return null;
        }

        internal override flow.Layer addToScene(SceneBuilder builder, Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            
            if (!this.selection.active) {
                return null;
            }

            RenderObject selected = this.selection.current;
            List<_TransformedRect> candidates = new List<_TransformedRect>();
            foreach (RenderObject candidate in this.selection.candidates) {
                if (candidate == selected || !candidate.attached) {
                    continue;
                }

                candidates.Add(new _TransformedRect(candidate));
            }

            _InspectorOverlayRenderState state = new _InspectorOverlayRenderState(this.overlayRect,
                new _TransformedRect(selected),
                candidates, this.selection.currentElement.toStringShort(),
                TextDirection.ltr
            );

            if (state != this._lastState) {
                this._lastState = state;
                this._picture = this._buildPicture(state);
            }

            builder.addPicture(layerOffset, this._picture);
            return null;
        }


        Picture _buildPicture(_InspectorOverlayRenderState state) {
            PictureRecorder recorder = new PictureRecorder();
            Canvas canvas = new RecorderCanvas(recorder);
            Size size = state.overlayRect.size;

            var fillPaint = new Paint() {color = _kHighlightedRenderObjectFillColor};
            var borderPaint = new Paint() {
                color = _kHighlightedRenderObjectBorderColor, style = PaintingStyle.stroke,
                strokeWidth = 1
            };
            Rect selectedPaintRect = state.selected.rect.deflate(0.5f);
            canvas.save();
            canvas.setMatrix(state.selected.transform);
            canvas.drawRect(selectedPaintRect, fillPaint);
            canvas.drawRect(selectedPaintRect, borderPaint);
            canvas.restore();

            foreach (var transformedRect in state.candidates) {
                canvas.save();
                canvas.setMatrix(transformedRect.transform);
                canvas.drawRect(transformedRect.rect.deflate(0.5f), borderPaint);
                canvas.restore();
            }

            // todo paint descipion
            return recorder.endRecording();
        }
    }
}