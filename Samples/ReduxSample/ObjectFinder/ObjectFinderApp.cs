using System;
using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.Sample.Redux.ObjectFinder {
    public class ObjectFinderApp : WidgetCanvas {
        public ObjectFinderApp() {
        }

        protected override Widget getWidget() {
            return new StoreProvider<FinderAppState>(StoreProvider.store, this.createWidget());
        }

        Widget createWidget() {
            return new StoreConnector<FinderAppState, ObjectFinderAppWidgetModel>(
                (context, viewModel) => new ObjectFinderAppWidget(
                    viewModel, this.gameObject.name
                ),
                (state, dispacher) => new ObjectFinderAppWidgetModel() {
                    objects = state.objects,
                    selected = state.selected,
                    doSearch = (text) => dispacher(new SearchAction() {keyword = text}),
                    onSelect = (id) => dispacher(new SelectObjectAction() {id = id})
                }
            );
        }
    }


    public delegate void onFindCallback(string keyword);

    public class ObjectFinderAppWidgetModel {
        public int selected;
        public List<GameObjectInfo> objects;
        public Action<string> doSearch;
        public Action<int> onSelect;
    }

    public class ObjectFinderAppWidget : StatefulWidget {
        public readonly List<GameObjectInfo> objectInfos;
        public readonly int selected;

        public readonly Action<string> doSearch;

        public readonly Action<int> onSelect;

        public readonly string title;

        public ObjectFinderAppWidget(ObjectFinderAppWidgetModel model, string title, Key key = null) : base(key) {
            this.objectInfos = model.objects;
            this.selected = model.selected;
            this.doSearch = model.doSearch;
            this.onSelect = model.onSelect;
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
                //scheduler.SchedulerBinding.instance.scheduleFrameCallback
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
                    color: this.widget.selected == obj.id ? new Color(0xFFFF0000) : null
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