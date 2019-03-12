using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsSample {
    public class MaterialSample : UIWidgetsSamplePanel {
        int testCaseId = 2;

        List<Widget> testCases = new List<Widget> {
            new MaterialButtonWidget(),
            new MaterialInkWellWidget(),
            new MaterialAppBarWidget()
        };

        protected override Widget createWidget() {
            return new WidgetsApp(
                home: this.testCases[this.testCaseId],
                pageRouteBuilder: this.pageRouteBuilder);
        }
        
        protected override void OnEnable() {
            base.OnEnable();
            FontManager.instance.addFont(Resources.Load<Font>(path: "MaterialIcons-Regular"));
        }
    }

    class MaterialAppBarWidget : StatefulWidget {
        public MaterialAppBarWidget(Key key = null) : base(key) {
            
        }
        public override State createState() {
            return new MaterialAppBarWidgetState();
        }
    }

    class MaterialAppBarWidgetState : State<MaterialAppBarWidget> {
        Choice _selectedChoice = Choice.choices[0];

        void _select(Choice choice) {
            this.setState(() => { this._selectedChoice = choice; });
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                    appBar: new AppBar(
                        backgroundColor : Colors.blue,
                        title: new Text("Basic AppBar"),
                        actions: new List<Widget> {
                            new IconButton(
                                icon: new Icon(Choice.choices[0].icon),
                                //color: Colors.blue,
                                onPressed: () => {
                                    this._select((Choice.choices[0]));
                                }
                                ),
                            new IconButton(
                                icon: new Icon(Choice.choices[1].icon),
                                //color: Colors.blue,
                                onPressed: () => {
                                    this._select((Choice.choices[1]));
                                }
                            ),
                            
                            /*new PopupMenuButton<Choice>(
                                onSelected: this._select,
                                itemBuilder: (BuildContext subContext) => {
                                    List<PopupMenuEntry<Choice>> popupItems = new List<PopupMenuEntry<Choice>>();
                                    for (int i = 2; i < Choice.choices.Count; i++) {
                                        popupItems.Add(new PopupMenuItem<Choice>(
                                            value: Choice.choices[i],
                                            child: new Text(Choice.choices[i].title)));
                                    }

                                    return popupItems;
                                }
                            )*/
                        }
                        ),
                    body: new Padding(
                        padding: EdgeInsets.all(16.0f),
                        child: new ChoiceCard(choice: this._selectedChoice)
                        )
                );
        }
    }


    class Choice {
        public Choice(string title, IconData icon) {
            this.title = title;
            this.icon = icon;
        }

        public readonly string title;
        public readonly IconData icon;

        public static List<Choice> choices = new List<Choice> {
            new Choice("Car", Unity.UIWidgets.material.Icons.directions_car),
            new Choice("Bicycle", Unity.UIWidgets.material.Icons.directions_bike),
            new Choice("Boat", Unity.UIWidgets.material.Icons.directions_boat),
            new Choice("Bus", Unity.UIWidgets.material.Icons.directions_bus),
            new Choice("Train", Unity.UIWidgets.material.Icons.directions_railway),
            new Choice("Walk", Unity.UIWidgets.material.Icons.directions_walk)
        };
    }

    class ChoiceCard : StatelessWidget {
        public ChoiceCard(Key key = null, Choice choice = null) : base(key: key) {
            this.choice = choice;
        }

        public readonly Choice choice;

        public override Widget build(BuildContext context) {
            TextStyle textStyle = Theme.of(context).textTheme.display1;
            return new Card(
                    color: Colors.white,
                    child: new Center(
                        child: new Column(
                                   mainAxisSize: MainAxisSize.min,
                                   crossAxisAlignment: CrossAxisAlignment.center,
                                   children: new List<Widget>{
                        new Icon(this.choice.icon, size: 128.0f, color: textStyle.color),
                        new Text(this.choice.title, style: textStyle)
                        }
                    )
                )
            );
        }
    }

    class MaterialInkWellWidget : StatefulWidget {
        public MaterialInkWellWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialInkWidgetState();
        }
    }

    class MaterialInkWidgetState : State<MaterialInkWellWidget> {
        public override Widget build(BuildContext context) {
            return new Material(
                //color: Colors.blue,
                child: new Center(
                    child: new Container(
                        width: 200,
                        height: 200,
                        child: new InkWell(
                            borderRadius: BorderRadius.circular(2.0f),
                            highlightColor: new Color(0xAAFF0000),
                            splashColor: new Color(0xAA0000FF),
                            onTap: () => { Debug.Log("on tap"); }
                        )
                    )
                )
            );
        }
    }

    class MaterialButtonWidget : StatefulWidget {
        public MaterialButtonWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialButtonWidgetState();
        }
    }

    class MaterialButtonWidgetState : State<MaterialButtonWidget> {
        public override Widget build(BuildContext context) {
            return new Stack(
                children: new List<Widget> {
                    new Material(
                        child: new Center(
                            child: new FlatButton(
                                shape: new RoundedRectangleBorder(borderRadius: BorderRadius.all(20.0f)),
                                color: new Color(0xFF00FF00),
                                splashColor: new Color(0xFFFF0011),
                                highlightColor: new Color(0x88FF0011),
                                child: new Text("Click Me"),
                                onPressed: () => { Debug.Log("pressed here"); }
                            )
                        )
                    ),
                    new PerformanceOverlay()
                }
            );
        }
    }
}