using Unity.UIWidgets.engine;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using System.Collections.Generic;
using UnityEngine;
using Material = Unity.UIWidgets.material.Material;

namespace UIWidgetsSample {
    public class ExpansionPanelCanvas : WidgetCanvas {
        protected override Widget getWidget() {
            return new ExpansionPanelWidget();
        }

        class ExpansionPanelWidget : StatefulWidget {
            public ExpansionPanelWidget(Key key = null) : base(key) {
            }

            public override State createState() {
                return new ExpansionPanelWidgetState();
            }
        }
        
        class ExpansionPanelWidgetState : State<ExpansionPanelWidget> {
            List<bool> isExpand = new List<bool>{false, false};
            
            public override Widget build(BuildContext context) {
                /*return new Material(
                    child: new SingleChildScrollView(
                        child: new Container(
                            width: 40.0,
                            height: 40.0,
                            constraints: BoxConstraints.tight(new Size(40, 300)),
                            color: AsScreenCanvas.CLColors.red,
                            child: new Center(child: new Text("XXXXXXXX"))
                        ) 
                            )
                    );*/
                return new Material(
                    child: new SingleChildScrollView(
                        child: new ExpansionPanelList(
                                expansionCallback: (int _index, bool _isExpanded) => {
                                    Debug.Log("???????????" + _index + " <> " + _isExpanded);
                                    
                                    this.isExpand[_index] = !_isExpanded;
                                    this.setState(() => {});
                                },
                                children: new List<ExpansionPanel> {
                                    new ExpansionPanel(
                                        headerBuilder: (BuildContext subContext, bool isExpanded) => {
                                            return new Container(
                                                width: 10.0,
                                                height: 10.0,
                                                constraints: BoxConstraints.tight(new Size(10, 10)),
                                                color: AsScreenCanvas.CLColors.blue
                                                );
                                        },
                                        body: new Container(
                                            width: 40.0,
                                            height: 10.0,
                                            constraints: BoxConstraints.tight(new Size(40, 10)),
                                            color: AsScreenCanvas.CLColors.red
                                        ),
                                       isExpanded : this.isExpand[0]
                                    ),
                                    new ExpansionPanel(
                                        headerBuilder: (BuildContext subContext, bool isExpanded) => {
                                            return new Container(
                                                width: 10.0,
                                                height: 10.0,
                                                constraints: BoxConstraints.tight(new Size(10, 10)),
                                                color: AsScreenCanvas.CLColors.blue
                                            );
                                        },
                                        body: new Container(
                                            width: 40.0,
                                            height: 10.0,
                                            constraints: BoxConstraints.tight(new Size(40, 10)),
                                            color: AsScreenCanvas.CLColors.red
                                        ),
                                        isExpanded: this.isExpand[1]
                                    )
                                }
                            )
                    )
                );
            }
        }
    }
}