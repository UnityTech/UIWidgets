using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.Sample.Redux.ObjectFinder {
    public class ObjectFinderApp : UIWidgetsSample.UIWidgetsSamplePanel {
        public ObjectFinderApp() {
        }

        protected override Widget createWidget()  {
            return new WidgetsApp(
                home: new StoreProvider<FinderAppState>(StoreProvider.store, this.createRootWidget()),
                pageRouteBuilder: this.pageRouteBuilder);
        }

        Widget createRootWidget() {
            return new StoreConnector<FinderAppState, ObjectFinderAppWidgetModel>(
                pure: true,
                builder: (context, viewModel, dispatcher) => new ObjectFinderAppWidget(
                    model: viewModel, 
                    doSearch: (text) => dispatcher.dispatch(SearchAction.create(text)),
                    onSelect: (id) => dispatcher.dispatch(new SelectObjectAction() {id = id}),
                    title: this.gameObject.name
                ),
                converter: (state) => new ObjectFinderAppWidgetModel() {
                    objects = state.objects,
                    selected = state.selected,
                }
            );
        }
    }


    public delegate void onFindCallback(string keyword);

    public class ObjectFinderAppWidgetModel : IEquatable<ObjectFinderAppWidgetModel> {
        public int selected;
        public List<GameObjectInfo> objects;

        public bool Equals(ObjectFinderAppWidgetModel other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return this.selected == other.selected && this.objects.equalsList(other.objects);
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
            return this.Equals((ObjectFinderAppWidgetModel) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.selected * 397) ^ this.objects.hashList();
            }
        }

        public static bool operator ==(ObjectFinderAppWidgetModel left, ObjectFinderAppWidgetModel right) {
            return Equals(left, right);
        }

        public static bool operator !=(ObjectFinderAppWidgetModel left, ObjectFinderAppWidgetModel right) {
            return !Equals(left, right);
        }
    }

    public class ObjectFinderAppWidget : StatefulWidget {
        public readonly List<GameObjectInfo> objectInfos;
        public readonly int selected;

        public readonly Action<string> doSearch;

        public readonly Action<int> onSelect;

        public readonly string title;

        public ObjectFinderAppWidget(
            ObjectFinderAppWidgetModel model = null,
            Action<string> doSearch = null,
            Action<int> onSelect = null,
            string title = null,
            Key key = null) : base(key) {
            this.objectInfos = model.objects;
            this.selected = model.selected;
            this.doSearch = doSearch;
            this.onSelect = onSelect;
            this.title = title;
        }

        public override State createState() {
            return new _ObjectFinderAppWidgetState();
        }
    }

    public class _ObjectFinderAppWidgetState : State<ObjectFinderAppWidget> {
        TextEditingController _controller;

        FocusNode _focusNode;

        public override void initState() {
            base.initState();
            this._controller = new TextEditingController("");
            this._focusNode = new FocusNode();
            if (this.widget.doSearch != null) {
                Window.instance.scheduleMicrotask(() => this.widget.doSearch(""));
            }

            this._controller.addListener(this.textChange);
        }

        public override void dispose() {
            this._focusNode.dispose();
            this._controller.removeListener(this.textChange);
            base.dispose();
        }

        public override Widget build(BuildContext context) {            
            Debug.Log("build ObjectFinderAppWidget");
            
            return new Container(
                padding: EdgeInsets.all(10),
                decoration: new BoxDecoration(color: new Color(0x4FFFFFFF),
                    border: Border.all(color: Color.fromARGB(255, 255, 0, 0), width: 5),
                    borderRadius: BorderRadius.all(2)),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: new List<Widget>() {
                        this._buildTitle(),
                        this._buildSearchInput(),
                        this._buildResultCount(),
                        this._buildResults(),
                    }
                )
            );
        }

        void textChange() {
            if (this.widget.doSearch != null) {
                this.widget.doSearch(this._controller.text);
            }
        }

        Widget _buildTitle() {
            return new Text(this.widget.title, textAlign: TextAlign.center,
                style: new TextStyle(fontSize: 20, height: 1.5f));
        }

        Widget _buildSearchInput() {
            return new Row(
                children: new List<Widget> {
                    new Text("Search:"),
                    new Flexible(child:
                        new Container(
                            margin: EdgeInsets.only(left: 8),
                            decoration: new BoxDecoration(border: Border.all(new Color(0xFF000000), 1)),
                            padding: EdgeInsets.only(left: 8, right: 8),
                            child: new EditableText(
                                selectionControls: MaterialUtils.materialTextSelectionControls,
                                backgroundCursorColor: Colors.transparent,
                                controller: this._controller,
                                focusNode: this._focusNode,
                                style: new TextStyle(
                                    fontSize: 18,
                                    height: 1.5f
                                ),
                                cursorColor: Color.fromARGB(255, 0, 0, 0)
                            )
                        )
                    )
                }
            );
        }

        Widget _buildResultItem(GameObjectInfo obj) {
            return new GestureDetector(child:
                new Container(
                    key: new ValueKey<int>(obj.id),
                    child: new Text(obj.name),
                    padding: EdgeInsets.all(8),
                    color: this.widget.selected == obj.id ? new Color(0xFFFF0000) : new Color(0)
                ), onTap: () => {
                    if (this.widget.onSelect != null) {
                        this.widget.onSelect(obj.id);
                    }
                });
        }

        Widget _buildResultCount() {
            return new Text($"Total Results:{this.widget.objectInfos.Count}",
                style: new TextStyle(height: 3.0f, fontSize: 12));
        }

        Widget _buildResults() {
            List<Widget> rows = new List<Widget>();
            this.widget.objectInfos.ForEach(obj => { rows.Add(this._buildResultItem(obj)); });
            return new Flexible(
                child: new ListView(
                    children: rows,
                    physics: new AlwaysScrollableScrollPhysics())
            );
        }
    }
}