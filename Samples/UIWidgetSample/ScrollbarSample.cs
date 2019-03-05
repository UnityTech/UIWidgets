using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsSample {
    public class ScrollbarSample : UIWidgetsSamplePanel {
        protected override Widget createWidget() {
            var scroll = new Container(
                decoration: new BoxDecoration(
                    border: Border.all(color: new Color(0xFFFFFF00))
                ),
                child: new Scrollbar(
                    child: new ListView(
                        children: new List<Widget> {
                            new Container(height: 40.0f, child: new Text("0")),
                            new Container(height: 40.0f, child: new Text("1")),
                            new Container(height: 40.0f, child: new Text("2")),
                            new Container(height: 40.0f, child: new Text("3")),
                            new Container(height: 40.0f, child: new Text("4")),
                            new Container(height: 40.0f, child: new Text("5")),
                            new Container(height: 40.0f, child: new Text("6")),
                            new Container(height: 40.0f, child: new Text("7")),
                            new Container(height: 40.0f, child: new Text("8")),
                            new Container(height: 40.0f, child: new Text("9")),
                            new Container(height: 40.0f, child: new Text("10")),
                            new Container(height: 40.0f, child: new Text("11")),
                            new Container(height: 40.0f, child: new Text("12")),
                            new Container(height: 40.0f, child: new Text("13")),
                            new Container(height: 40.0f, child: new Text("14")),
                            new Container(height: 40.0f, child: new Text("15")),
                            new Container(height: 40.0f, child: new Text("16")),
                            new Container(height: 40.0f, child: new Text("17")),
                        }
                    )
                )
            );
            return new WidgetsApp(
                home: scroll,
                pageRouteBuilder: this.pageRouteBuilder);
        }
    }
}