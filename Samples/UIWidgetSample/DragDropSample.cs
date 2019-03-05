using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsSample {
    public class DragDropSample : UIWidgetsSamplePanel {
        protected override Widget createWidget() {
            return new WidgetsApp(
                home: new DragDropApp(),
                pageRouteBuilder: this.pageRouteBuilder
                );
        }

        class DragDropApp : StatefulWidget {
            public DragDropApp(Key key = null) : base(key) {
            }

            public override State createState() {
                return new DragDropState();
            }
        }

        class DragTargetWidget : StatefulWidget {
            public DragTargetWidget(Key key = null) : base(key) {
            }

            public override State createState() {
                return new DragTargetWidgetState();
            }
        }

        class DragTargetWidgetState : State<DragTargetWidget> {
            int value;

            public override Widget build(BuildContext context) {
                return new Positioned(
                    left: 40.0f,
                    bottom: 40.0f,
                    child: new DragTarget<int>(
                        onAccept: obj => {
                            Debug.Log("ON ACCEPTED ..." + obj);
                            this.setState(() => { this.value += obj; });
                        },
                        builder: (inner_context2, accepted, rejected) => {
                            return new Container(
                                width: 40.0f,
                                height: 40.0f,
                                constraints: BoxConstraints.tight(new Size(40, 40)),
                                color: CLColors.red,
                                child: new Center(child: new Text("" + this.value))
                            );
                        }
                    )
                );
            }
        }

        class DragDropState : State<DragDropApp> {
            public override Widget build(BuildContext context) {
                var entries = new List<OverlayEntry>();

                var entry_bg = new OverlayEntry(
                    inner_context => new Container(
                        color: CLColors.white
                    ));

                var entry = new OverlayEntry(
                    inner_context => new Positioned(
                        left: 0.0f,
                        bottom: 0.0f,
                        child: new GestureDetector(
                            onTap: () => { },
                            child: new Draggable<int>(
                                5,
                                child: new Container(
                                    color: CLColors.blue,
                                    width: 30.0f,
                                    height: 30.0f,
                                    constraints: BoxConstraints.tight(new Size(30, 30)),
                                    child: new Center(child: new Text("5"))
                                ),
                                feedback: new Container(
                                    color: CLColors.green,
                                    width: 30.0f,
                                    height: 30.0f),
                                //maxSimultaneousDrags: 1,
                                childWhenDragging: new Container(
                                    color: CLColors.black,
                                    width: 30.0f,
                                    height: 30.0f,
                                    constraints: BoxConstraints.tight(new Size(30, 30))
                                )
                            )
                        )
                    )
                );

                var entry3 = new OverlayEntry(
                    inner_context => new Positioned(
                        left: 0.0f,
                        bottom: 40.0f,
                        child: new GestureDetector(
                            onTap: () => { },
                            child:
                            new Draggable<int>(
                                8,
                                child: new Container(
                                    color: CLColors.background4,
                                    width: 30.0f,
                                    height: 30.0f,
                                    constraints: BoxConstraints.tight(new Size(30, 30)),
                                    child: new Center(child: new Text("8")))
                                ,
                                feedback: new Container(
                                    color: CLColors.green,
                                    width: 30.0f,
                                    height: 30.0f),
                                maxSimultaneousDrags: 1,
                                childWhenDragging: new Container(
                                    color: CLColors.black,
                                    width: 30.0f,
                                    height: 30.0f,
                                    constraints: BoxConstraints.tight(new Size(30, 30))
                                )
                            )
                        )
                    )
                );

                var entry2 = new OverlayEntry(
                    inner_context => new DragTargetWidget()
                );

                entries.Add(entry_bg);
                entries.Add(entry);
                entries.Add(entry2);
                entries.Add(entry3);

                return new Container(
                    color: CLColors.white,
                    child: new Overlay(
                        initialEntries: entries
                    )
                );
            }
        }
    }
}