using System.Collections.Generic;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsSample {
    public class PageViewSample : UIWidgetsSamplePanel {
            
        protected override Widget createWidget() {
            return new WidgetsApp(
                home: new Container(
                width: 200,
                height: 400,
                child: new PageView(
                    children: new List<Widget>() {
                        new Container(
                            color: new Color(0xFFE91E63)
                        ),
                        new Container(
                            color: new Color(0xFF00BCD4)
                        ),
                        new Container(
                            color: new Color(0xFF673AB7)
                        )
                    }
                )),
                pageRouteBuilder: this.pageRouteBuilder);
        }
    }
}