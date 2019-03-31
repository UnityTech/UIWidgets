using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Material = Unity.UIWidgets.material.Material;

namespace UIWidgetsSample {
    public class ExpansionPanelSample : UIWidgetsSamplePanel {
        int testCaseId = 1;

        readonly List<Widget> testCases = new List<Widget> {
            new SingleChildScrollWidget(),
            new ExpansionPanelWidget()
        };

        protected override Widget createWidget()  {
            return new WidgetsApp(
                home: this.testCases[this.testCaseId],
                pageRouteBuilder: this.pageRouteBuilder);
        }
    }


    class SingleChildScrollWidget : StatefulWidget {
        public SingleChildScrollWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new SingleChildScrollWidgetState();
        }
    }

    class SingleChildScrollWidgetState : State<SingleChildScrollWidget> {
        public override Widget build(BuildContext context) {
            return new Material(
                child: new SingleChildScrollView(
                    child: new Container(
                        width: 40.0f,
                        height: 40.0f,
                        constraints: BoxConstraints.tight(new Size(40, 600)),
                        color: CLColors.red,
                        child: new Center(child: new Text("Beijing"))
                    )
                )
            );
        }
    }


    class ExpansionPanelWidget : StatefulWidget {
        public ExpansionPanelWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new ExpansionPanelWidgetState();
        }
    }

    class ExpansionPanelWidgetState : State<ExpansionPanelWidget> {
        readonly List<bool> isExpand = new List<bool> {false, false};

        public override Widget build(BuildContext context) {
            return new Material(
                child: new SingleChildScrollView(
                    child: new ExpansionPanelList(
                        expansionCallback: (int _index, bool _isExpanded) => {
                            Debug.Log("<tile " + _index + "> from [" + (_isExpanded ? "Open" : "Close") + "]" +
                                      " to [" + (_isExpanded ? "Close" : "Open") + "]");

                            this.isExpand[_index] = !_isExpanded;
                            this.setState(() => { });
                        },
                        children: new List<ExpansionPanel> {
                            new ExpansionPanel(
                                headerBuilder: (BuildContext subContext, bool isExpanded) => {
                                    return new Container(
                                        color: Colors.black45,
                                        child: new Center(
                                            child: new Text("Beijing")
                                        )
                                    );
                                },
                                body: new Container(
                                    child: new Column(
                                        children: new List<Widget> {
                                            new Card(
                                                child: new Container(
                                                    color: Colors.black38,
                                                    height: 36,
                                                    width: 300,
                                                    child: new Center(
                                                        child: new Text("Beijing")
                                                    )
                                                )
                                            )
                                        }
                                    )
                                ),
                                isExpanded: this.isExpand[0]
                            ),
                            new ExpansionPanel(
                                headerBuilder: (BuildContext subContext, bool isExpanded) => {
                                    return new Container(
                                        color: Colors.black45,
                                        child: new Center(
                                            child: new Text("Hebei")
                                        )
                                    );
                                },
                                body: new Container(
                                    child: new Column(
                                        children: new List<Widget> {
                                            new Card(
                                                child: new Container(
                                                    color: Colors.black38,
                                                    height: 36,
                                                    width: 300,
                                                    child: new Center(
                                                        child: new Text("Tianjin")
                                                    )
                                                )
                                            ),
                                            new Card(
                                                child: new Container(
                                                    color: Colors.black38,
                                                    height: 36,
                                                    width: 300,
                                                    child: new Center(
                                                        child: new Text("Shijiazhuang")
                                                    )
                                                )
                                            ),
                                            new Card(
                                                child: new Container(
                                                    color: Colors.black38,
                                                    height: 36,
                                                    width: 300,
                                                    child: new Center(
                                                        child: new Text("Zhumadian")
                                                    )
                                                )
                                            )
                                        }
                                    )
                                ),
                                isExpanded: this.isExpand[1]
                            ),
                        }
                    )
                )
            );
        }
    }
}