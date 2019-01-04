using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets
{
    public delegate Widget InspectorSelectButtonBuilder(BuildContext context, VoidCallback onPressed);
    internal class _SerializeConfig
    {
        public _SerializeConfig(string groupName, bool summaryTree = false, 
            int subtreeDepth = 1, List<Diagnosticable> pathToInclude = null,
            bool includeProperties = false, bool expandPropertyValues = true)
        {
            this.groupName = groupName;
            this.summaryTree = summaryTree;
            this.subtreeDepth = subtreeDepth;
            this.pathToInclude = pathToInclude;
            this.includeProperties = includeProperties;
            this.expandPropertyValues = expandPropertyValues;
        }

        public static _SerializeConfig merge(_SerializeConfig from, int? subtreeDepth = null,
            List<Diagnosticable> pathToInclude = null)
        {
            return new _SerializeConfig(from.groupName, from.summaryTree, subtreeDepth??from.subtreeDepth,
                pathToInclude??from.pathToInclude, from.includeProperties, from.expandPropertyValues);
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
    internal class _InspectorReferenceData
    {
        public readonly object obj;
        public int count = 1;

        public _InspectorReferenceData(object obj)
        {
            this.obj = obj;
        }
    }
    
    public class WidgetInspectorService
    {
        public readonly WidgetsBinding widgetsBinding;
        private bool _debugShowInspector;
        private readonly Dictionary<string, HashSet<_InspectorReferenceData>> _groups = new Dictionary<string, HashSet<_InspectorReferenceData>>();
        private readonly Dictionary<string, _InspectorReferenceData> _idToReferenceData = new Dictionary<string, _InspectorReferenceData>();
        private readonly  Dictionary<object, string> _objectToId = new Dictionary<object, string>();
        private int _nextId = 0;
        public readonly InspectorSelection selection = new InspectorSelection();
        public InspectorSelectionChangedCallback selectionChangedCallback;
        public DeveloperInspect developerInspect;
        public InspectorShowCallback inspectorShowCallback;

        public static WidgetInspectorService instance
        {
            get { return WidgetsBinding.instance.widgetInspectorService; }
        }
        
        public WidgetInspectorService(WidgetsBinding widgetsBinding)
        {
            this.widgetsBinding = widgetsBinding;
            
        }
        

//        private static WidgetInspectorService _instance;
//
//        public WidgetInspectorService instance
//        {
//            get { return _instance; }
//        }

        public Dictionary<string, object> getRootWidget(string groupName)
        {
            return _nodeToJson(widgetsBinding.renderViewElement.toDiagnosticsNode(), new _SerializeConfig(groupName));
        }
        
        public Dictionary<string, object> getRootWidgetSummaryTree(string groupName)
        {
            return _nodeToJson(widgetsBinding.renderViewElement.toDiagnosticsNode(), 
                new _SerializeConfig(groupName, subtreeDepth: 1000000, summaryTree: true));
        }
        
        public Dictionary<string, object> getRootRenderObject(string groupName)
        {
            return _nodeToJson(widgetsBinding.renderView.toDiagnosticsNode(), 
                new _SerializeConfig(groupName: groupName, subtreeDepth: 1000000));
        }

        public Dictionary<string, object> getDetailsSubtree(string id, string groupName)
        {
            var root = toObject(id) as DiagnosticsNode;
            if (root == null)
            {
                return null;
            }
            return _nodeToJson(root, 
                new _SerializeConfig(groupName, summaryTree:false, subtreeDepth:2, includeProperties:true));
        }
        
        public bool setSelectionById(string id, string groupName = "") {
            return setSelection(toObject(id), groupName);
        }

         
        public Dictionary<string, object> getSelectedRenderObject(string previousSelectionId, string groupName)
        {
            DiagnosticsNode previousSelection = toObject(previousSelectionId) as DiagnosticsNode;
            RenderObject current = selection == null ? null : selection.current;
            return _nodeToJson(
                current == (previousSelection == null ? null: previousSelection.valueObject) ? previousSelection : (current == null ? null : current.toDiagnosticsNode()), 
                new _SerializeConfig(groupName: groupName));
        }
        
        
        public Dictionary<string, object> getSelectedWidget(string previousSelectionId, string groupName)
        {
            DiagnosticsNode previousSelection = (DiagnosticsNode)toObject(previousSelectionId);
            Element current = null;
            if (selection != null)
            {
                current = selection.currentElement;
            }

            return _nodeToJson(
                current == (previousSelection == null ? null : previousSelection.valueObject) ? previousSelection :
                (current == null ? null : current.toDiagnosticsNode()), new _SerializeConfig(groupName: groupName));
        }

        public Dictionary<string, object> getSelectedSummaryWidget(string previousSelectionId, string groupName)
        {
            return getSelectedWidget(previousSelectionId, groupName);
        }
       
        public bool debugShowInspector
        {
            get { return _debugShowInspector; }
            set
            {
                var old = _debugShowInspector;
                _debugShowInspector = value;
                if (_debugShowInspector != old && inspectorShowCallback != null)
                {
                    inspectorShowCallback();
                } 
            }
        }

        public void disposeGroup(string groupName)
        {
            HashSet<_InspectorReferenceData> references;
            _groups.TryGetValue(groupName, out references);
            _groups.Remove(groupName);
            if (references != null)
            {
                foreach (var r in references)
                {
                    _decrementReferenceCount(r);
                }
            }
            
            
            
            D.assert(() =>
            {
//                var groupNames = _groups.Select((entry) => string.Format("groupName={0} count={1}", entry.Key, entry.Value.Count));
//                Debug.LogFormat("groups {0} idsCount={1} objectsCount={2}", string.Join(",", groupNames.ToArray()),
//                    _idToReferenceData.Count, _objectToId.Count);
                return true;
            });

        }
        
        protected bool setSelection(object obj, string groupName = "")
        {
            if (obj is Element || obj is RenderObject)
            {
                if (obj is Element) {
                    if (ReferenceEquals(obj, selection.currentElement)) {
                        return false;
                    }
                    selection.currentElement = (Element)obj;
                } else {
                    if (obj == selection.current) {
                        return false;
                    }
                    selection.current = (RenderObject)obj;
                }
                
                if (selectionChangedCallback != null)
                {
                    if (WidgetsBinding.instance.schedulerPhase == SchedulerPhase.idle) {
                        selectionChangedCallback();
                    } else {
                        // todo schedule task ?
                        WidgetsBinding.instance.scheduleFrameCallback(
                            duration =>
                            {
                                selectionChangedCallback();
                            }
                        );
                    }
                    
                }
                return true;
            }

            return false;
        }

        private Dictionary<string, object> _nodeToJson(foundation.DiagnosticsNode node, _SerializeConfig config)
        {
            if (node == null)
            {
                return null;
            }

            var ret = node.toJsonMap();
            var value = node.valueObject;
            ret["objectId"] = toId(node, config.groupName);
            ret["valueId"] = toId(value, config.groupName);

            if (config.summaryTree)
            {
                ret["summaryTree"] = config.summaryTree;
            }

            var createdByLocalProject = true; // todo;
            if (config.subtreeDepth > 0 || (config.pathToInclude != null && config.pathToInclude.Count > 0))
            {
                ret["children"] = _nodesToJson(_getChildrenFiltered(node, config), config);
            }

            if (config.includeProperties)
            {
                ret["properties"] = _nodesToJson(
                    node.getProperties().Where((n) =>
                        !n.isFiltered(createdByLocalProject ? DiagnosticLevel.fine : DiagnosticLevel.info)).ToList(),
                    new _SerializeConfig(groupName: config.groupName, subtreeDepth: 1, expandPropertyValues: true)
                );
            }

            var typeDef = typeof(DiagnosticsProperty<>);
            var nodeType = node.GetType();
            if (nodeType.IsGenericType && nodeType.GetGenericTypeDefinition() == typeDef)
            {
                if (value is Color)
                {
                    ret["valueProperties"] = new Dictionary<string, object>
                    {
                        {"red", ((Color)value).red},
                        {"green", ((Color)value).green},
                        {"blue", ((Color)value).blue},
                        {"alpha", ((Color)value).alpha},
                    };
                } else if (value is IconData)
                {
                    ret["valueProperties"] = new Dictionary<string, object>
                    {
                        {"codePoint", ((IconData)value).codePoint}
                    };
                }

                if (config.expandPropertyValues && value is Diagnosticable)
                {
                    ret["properties"] = _nodesToJson(
                        ((Diagnosticable) value).toDiagnosticsNode().getProperties()
                        .Where(n => !n.isFiltered(DiagnosticLevel.info)).ToList(),
                        new _SerializeConfig(groupName: config.groupName, subtreeDepth: 0, expandPropertyValues: false)
                    );
                }
            }
            
            return ret;
        }

        private List<Dictionary<string, object>> _nodesToJson(List<foundation.DiagnosticsNode> nodes, _SerializeConfig config)
        {
            if (nodes == null)
            {
                return new List<Dictionary<string, object>>();
            }

            return nodes.Select(node =>
            {
                if (config.pathToInclude != null && config.pathToInclude.Count > 0)
                {
                    if (config.pathToInclude[0] == node.valueObject)
                    {
                        return _nodeToJson(node, _SerializeConfig.merge(config,
                            pathToInclude: config.pathToInclude.GetRange(1, config.pathToInclude.Count - 1)));
                    }
                    else
                    {
                        return _nodeToJson(node, _SerializeConfig.merge(config));
                    }

                }
                
                return _nodeToJson(node, 
                    config.summaryTree || config.subtreeDepth > 1 || _shouldShowInSummaryTree(node) ? 
                    _SerializeConfig.merge(config, subtreeDepth: config.subtreeDepth - 1) : config 
                    );

            }).ToList();
        }

        public List<Dictionary<string, object>> getProperties(string diagnosticsNodeId, string groupName)
        {
            var node = toObject(diagnosticsNodeId) as DiagnosticsNode;
            return _nodesToJson(node == null ? new List<DiagnosticsNode>() : node.getProperties(),
                new _SerializeConfig(groupName: groupName));

        }

        private List<foundation.DiagnosticsNode> _getChildrenFiltered(foundation.DiagnosticsNode node,
            _SerializeConfig config)
        {
            var children = new List<foundation.DiagnosticsNode>();
            foreach (var child in node.getChildren())
            {
                if (!config.summaryTree || _shouldShowInSummaryTree(child))
                {
                    children.Add(child);
                }
                else
                {
                    children.AddRange(_getChildrenFiltered(child, config));
                }
            }
            return children;
        }

        private bool _shouldShowInSummaryTree(foundation.DiagnosticsNode node)
        {
            var value = node.valueObject;
            if (!(value is Diagnosticable))
            {
                return true;
            }

            if (!(value is Element) || !isWidgetCreationTracked())
            {
                return true;
            }

            return _isValueCreatedByLocalProject(value);
        }

        string toId(object obj, string groupName)
        {
            if (obj == null)
            {
                return null;
            }

            HashSet<_InspectorReferenceData> group;
            _groups.TryGetValue(groupName, out group);
            if (group == null)
            {
                group = new HashSet<_InspectorReferenceData>();
                _groups.Add(groupName, group);
            }

            string id;
            _objectToId.TryGetValue(obj, out id);

            _InspectorReferenceData referenceData;
            if (id == null)
            {
                id = string.Format("inspector-{0}", _nextId);
                _nextId++;
                _objectToId[obj] = id;
                referenceData = new _InspectorReferenceData(obj);
                _idToReferenceData[id] = referenceData;
                group.Add(referenceData);
            }
            else
            {
                referenceData = _idToReferenceData[id];
                if (group.Add(referenceData))
                {
                    referenceData.count += 1;
                }
            }

            return id;
        }

        object toObject(string id)
        {
            if (id == null)
            {
                return null;
            }

            _InspectorReferenceData data;
            _idToReferenceData.TryGetValue(id, out data);
            if (data == null)
            {
                throw new UIWidgetsError("Id does not exist.");
            }

            return data.obj;
        }

        private bool isWidgetCreationTracked()
        {
            return false; //todo
        }

        private bool _isValueCreatedByLocalProject(object value)
        {
            return true; // todo
        }
        
        void _decrementReferenceCount(_InspectorReferenceData reference) {
            reference.count -= 1;
            D.assert(reference.count >= 0);
            if (reference.count == 0)
            {
                string id;
                _objectToId.TryGetValue(reference.obj, out id);
                D.assert(id != null);
                _objectToId.Remove(reference.obj);
                _idToReferenceData.Remove(id);
            }
        }
    }

    public class WidgetInspector: StatefulWidget
    {
        public readonly Widget child;
        public WidgetInspector(Key key, Widget child, InspectorSelectButtonBuilder selectButtonBuilder) : base(key)
        {
            D.assert(child != null);
            this.child = child;
            this.selectButtonBuilder = selectButtonBuilder;
        }

        public readonly InspectorSelectButtonBuilder selectButtonBuilder;
        public override State createState()
        {
            return new _WidgetInspectorState(WidgetsBinding.instance.widgetInspectorService.selection);
        } 
        
    }


    class _WidgetInspectorState : State<WidgetInspector>, WidgetsBindingObserver
    {

        public _WidgetInspectorState(InspectorSelection selection)
        {
            this.selection = selection;
        }
        
        private Offset _lastPointerLocation;
        public readonly InspectorSelection selection;
        public bool isSelectMode = true;
        private readonly GlobalKey _ignorePointerKey = GlobalKey.key();
        const double _edgeHitMargin = 2.0;
        const double _kOffScreenMargin = 1.0;
        const double _kScreenEdgeMargin = 10.0;
        const double _kTooltipPadding = 5.0;
        const double _kInspectButtonMargin = 10.0;
        
        public override void initState() {
            base.initState();
            WidgetInspectorService.instance.selectionChangedCallback += _selectionChangedCallback;
        }

        public override void dispose()
        {
            WidgetInspectorService.instance.selectionChangedCallback -= _selectionChangedCallback; 
            base.dispose();
        }
        
        private bool _hitTestHelper(
            List<RenderObject> hits,
            List<RenderObject> edgeHits,
            Offset position,
            RenderObject renderObject,
        Matrix3 transform
            ) {
            var hit = false;

            Matrix3 inverse = transform.inverse();
            //var localPosition = MatrixUtils.transformPoint(inverse, position);
            var localPosition = inverse.transformPoint(position);
            
            List<DiagnosticsNode> children = renderObject.debugDescribeChildren();
            for (int i = children.Count - 1; i >= 0; --i)
            {
                DiagnosticsNode diagnostics = children[i];
                D.assert(diagnostics != null);
                if (diagnostics.style == DiagnosticsTreeStyle.offstage || (!(diagnostics.valueObject is RenderObject)))
                {
                    continue;
                }
                RenderObject child = (RenderObject)diagnostics.valueObject;
                Rect paintClip = renderObject.describeApproximatePaintClip(child);
                if (paintClip != null && !paintClip.contains(localPosition))
                {
                    continue;
                }

                var childTransform = transform;
                renderObject.applyPaintTransform(child, ref childTransform);
                if (_hitTestHelper(hits, edgeHits, position, child, childTransform))
                {
                    hit = true;
                }
            }
            
            Rect bounds = renderObject.semanticBounds;
            if (bounds.contains(localPosition)) {
                hit = true;
                if (!bounds.deflate(_edgeHitMargin).contains(localPosition))
                {
                    edgeHits.Add(renderObject);
                }     
            }

            if (hit)
            {
                hits.Add(renderObject);   
            }
            return hit;
        }
        
        List<RenderObject> hitTest(Offset position, RenderObject root) {
            List<RenderObject> regularHits = new List<RenderObject>();
            List<RenderObject> edgeHits = new List<RenderObject>();

            _hitTestHelper(regularHits, edgeHits, position, root, root.getTransformTo(null));
            regularHits.Sort(CompareByArea);
            HashSet<RenderObject> hits = new HashSet<RenderObject>(edgeHits);
            foreach (var obj in regularHits)
            {
                if (!hits.Contains(obj))
                {
                    hits.Add(obj);
                    edgeHits.Add(obj);
                }
            }

            return edgeHits;
        }
        
        void _inspectAt(Offset position) {
            if (!isSelectMode)
                return;

            RenderIgnorePointer ignorePointer = (RenderIgnorePointer)_ignorePointerKey.currentContext.findRenderObject();
            RenderObject userRender = ignorePointer.child;
            List<RenderObject> selected = hitTest(position, userRender);
            setState(() => {
                selection.candidates = selected;
            });
        }

        void _handlePanDown(DragDownDetails evt) {
            _lastPointerLocation = evt.globalPosition;
            _inspectAt(evt.globalPosition);
        }

        void _handlePanUpdate(DragUpdateDetails evt) {
            _lastPointerLocation = evt.globalPosition;
            _inspectAt(evt.globalPosition);
        }

        void _handlePanEnd(DragEndDetails details) {
            Rect bounds = (Offset.zero & (Window.instance.physicalSize /Window.instance.devicePixelRatio)).deflate(_kOffScreenMargin);
            if (!bounds.contains(_lastPointerLocation)) {
                setState(() => {
                    selection.clear();
                });
            }
        }

        void _handleTap() {
            if (!isSelectMode)
                return;
            if (_lastPointerLocation != null) {
                _inspectAt(_lastPointerLocation);

                if (selection != null) {
                    if (WidgetInspectorService.instance.developerInspect != null)
                    {
                        WidgetInspectorService.instance.developerInspect();
                    }
                }
            }
            setState(() => {
                if (widget.selectButtonBuilder != null)
                {
                    isSelectMode = false;
                }
            });
        }

        void _handleEnableSelect() {
            setState(() => {
                isSelectMode = true;
            });
        }

        public override Widget build(BuildContext context)
        {
            List<Widget> children = new List<Widget>();
            children.Add(new GestureDetector(
                onTap: _handleTap,
                onPanDown: _handlePanDown,
                onPanEnd: _handlePanEnd,
                onPanUpdate:_handlePanUpdate,
                behavior: HitTestBehavior.opaque,
                child: new IgnorePointer(
                    ignoring: isSelectMode,
                    key: _ignorePointerKey,
                    child: widget.child
                    )
                ));

            if (!isSelectMode && widget.selectButtonBuilder != null)
            {
                children.Add(new Positioned(
                    left: _kInspectButtonMargin,
                    bottom: _kInspectButtonMargin,
                    child: widget.selectButtonBuilder(context, _handleEnableSelect)
                ));
            }
            children.Add(new _InspectorOverlay(null, selection));
            return new Stack(children: children);
        }

        public void didChangeMetrics()
        {
            throw new NotImplementedException();
        }

        private void _selectionChangedCallback()
        {
            setState(() => {});
        }

        private static int CompareByArea(RenderObject o1, RenderObject o2)
        {
            return _area(o1).CompareTo(_area(o2));
        }

        private static double _area(RenderObject obj)
        {
            var bounds = obj.semanticBounds;
            Size size = null;
            if (bounds != null)
            {
                size = bounds.size;
            }
            return size == null ? double.PositiveInfinity : size.width * size.height;
        }
    }


    public class InspectorSelection
    {
        private RenderObject _current;
        private Element _currentElement;
        private List<RenderObject> _candidates = new List<RenderObject>();

        public List<RenderObject> candidates
        {
            get { return _candidates; }
            set
            {
                _candidates = value;
                _index = 0;
                _computeCurrent();
            }
        }

        private int _index = 0;

        public int index
        {
            get { return _index; }
            set { _index = value;
                _computeCurrent();}
        }

        public void clear()
        {
            candidates = new List<RenderObject>();
        }
        
        public RenderObject current
        {
            get { return _current; }
            set
            {
                if (_current != value) {
                    _current = value;
                    _DebugCreator creator = value.debugCreator as _DebugCreator;
                    _currentElement = creator.element;
                }
            }
        }

        public Element currentElement
        {
            get { return _currentElement; }
            set { if (!ReferenceEquals(currentElement, value)) {
                _currentElement = value;
                _current = value.findRenderObject();
            } }
        }
        
        void _computeCurrent() {
            if (_index < candidates.Count) {
                _current = candidates[index];
                _currentElement = ((_DebugCreator)_current.debugCreator).element;
            } else {
                _current = null;
                _currentElement = null;
            }
        }


        public bool active
        {
            get { return _current != null && _current.attached; }
        }
    }
   
    class _InspectorOverlay: LeafRenderObjectWidget 
    {
        public _InspectorOverlay(Key key, InspectorSelection selection):base(key)
        {
            this.selection = selection;
        }

        public readonly InspectorSelection selection;
        
        public override RenderObject createRenderObject(BuildContext context)
        {
            return new _RenderInspectorOverlay(selection);
        }
        
        public override void  updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((_RenderInspectorOverlay)renderObject).selection = selection;
        }
    }

    class _RenderInspectorOverlay: RenderBox 
    {
        public _RenderInspectorOverlay(InspectorSelection selection)
        {
            D.assert(selection != null);
            _selection = selection;
        }

        private InspectorSelection _selection;

        public InspectorSelection selection
        {
            get { return _selection; }
            set
            {
                if (value != _selection) {
                    _selection = value;
                    markNeedsPaint();
                }
            }
        }

        protected override bool sizedByParent
        {
            get { return true; }
        }
        
        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }
        
        protected override void performResize() {
            this.size = constraints.constrain(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        public override void paint(PaintingContext context, Offset offset)
        {
            D.assert(needsCompositing);
            context.addLayer(new _InspectorOverlayLayer(
                Rect.fromLTWH(offset.dx, offset.dy, size.width, size.height),
                selection
                ));
        }

    }

    class _TransformedRect:IEquatable<_TransformedRect>
    {
        public readonly Rect rect;
        public readonly  Matrix3 transform;

        public _TransformedRect(RenderObject obj)
        {
            rect = obj.semanticBounds;
            transform = obj.getTransformTo(null);
        }

        public bool Equals(_TransformedRect other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(rect, other.rect) && transform.Equals(other.transform);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((_TransformedRect) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((rect != null ? rect.GetHashCode() : 0) * 397) ^ transform.GetHashCode();
            }
        }

        public static bool operator ==(_TransformedRect left, _TransformedRect right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(_TransformedRect left, _TransformedRect right)
        {
            return !Equals(left, right);
        }
    }
    class _InspectorOverlayRenderState:IEquatable<_InspectorOverlayRenderState>
    {
        public readonly Rect overlayRect;
        public readonly _TransformedRect selected;
        public readonly List<_TransformedRect> candidates;
        public readonly string tooltip;
        public readonly TextDirection textDirection;

        public _InspectorOverlayRenderState(Rect overlayRect, _TransformedRect selected, 
            List<_TransformedRect> candidates, string tooltip, TextDirection textDirection)
        {
            this.overlayRect = overlayRect;
            this.selected = selected;
            this.candidates = candidates;
            this.tooltip = tooltip;
            this.textDirection = textDirection;
        }

        public bool Equals(_InspectorOverlayRenderState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(overlayRect, other.overlayRect) && Equals(selected, other.selected) 
                                                        && string.Equals(tooltip, other.tooltip) && textDirection == other.textDirection
                                                          && (candidates == other.candidates || (candidates != null && candidates.SequenceEqual(other.candidates)));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((_InspectorOverlayRenderState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (overlayRect != null ? overlayRect.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (selected != null ? selected.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (candidates != null ? candidates.Aggregate(0, (hash, rect) => (hash * 397) ^ (rect== null ? 0 : rect.GetHashCode())) : 0);
                hashCode = (hashCode * 397) ^ (tooltip != null ? tooltip.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) textDirection;
                return hashCode;
            }
        }

        public static bool operator ==(_InspectorOverlayRenderState left, _InspectorOverlayRenderState right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(_InspectorOverlayRenderState left, _InspectorOverlayRenderState right)
        {
            return !Equals(left, right);
        }
    }


    class _InspectorOverlayLayer: Layer
    {
        
        // const int _kMaxTooltipLines = 5;
        // private static Color _kTooltipBackgroundColor = Color.fromARGB(230, 60, 60, 60);
        private static  Color _kHighlightedRenderObjectFillColor = Color.fromARGB(128, 128, 128, 255);
        private static  Color _kHighlightedRenderObjectBorderColor = Color.fromARGB(128, 64, 64, 128);
        
        public _InspectorOverlayLayer(Rect overlayRect, InspectorSelection selection)
        {
            this.overlayRect = overlayRect;
            this.selection = selection;
        }
            
        public InspectorSelection selection;
        public readonly Rect overlayRect;
        _InspectorOverlayRenderState _lastState;
        public Picture _picture;
        // public  TextPainter _textPainter;
        // public double _textPainterMaxWidth;


        public override void addToScene(SceneBuilder builder, Offset layerOffset)
        {
            if (!selection.active)
            {
                return;
            }
            
            RenderObject selected = selection.current;
            List<_TransformedRect> candidates = new List<_TransformedRect>();
            foreach (RenderObject candidate in selection.candidates) {
                if (candidate == selected || !candidate.attached)
                    continue;
                candidates.Add(new _TransformedRect(candidate));
            }
            
            _InspectorOverlayRenderState state = new _InspectorOverlayRenderState(
                overlayRect,
                new _TransformedRect(selected),
                candidates,
                selection.currentElement.toStringShort(),
                TextDirection.ltr
            );

            if (state != _lastState) {
                _lastState = state;
                _picture = _buildPicture(state);
            }
            builder.addPicture(layerOffset, _picture);
        }


        ui.Picture _buildPicture(_InspectorOverlayRenderState state)
        {
            PictureRecorder recorder = new PictureRecorder();
            ui.Canvas canvas = new RecorderCanvas(recorder);
            Size size = state.overlayRect.size;
            
            var fillPaint = new Paint(){color = _kHighlightedRenderObjectFillColor};
            var borderPaint =  new Paint(){color = _kHighlightedRenderObjectBorderColor, style = PaintingStyle.stroke, strokeWidth = 1};
            Rect selectedPaintRect = state.selected.rect.deflate(0.5);
            canvas.save();
            canvas.setMatrix(state.selected.transform);
            canvas.drawRect(selectedPaintRect, fillPaint);
            canvas.drawRect(selectedPaintRect, borderPaint);
            canvas.restore();

            foreach (var transformedRect in state.candidates)
            {
                canvas.save();
                canvas.setMatrix(transformedRect.transform);
                canvas.drawRect(transformedRect.rect.deflate(0.5), borderPaint);
                canvas.restore();
            }
           
            // todo paint descipion
            return recorder.endRecording();

        }
    }
    
}