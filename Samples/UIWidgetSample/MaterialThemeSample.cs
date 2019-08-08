using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Image = Unity.UIWidgets.widgets.Image;

namespace UIWidgetsSample {
    public class MaterialThemeSample: UIWidgetsSamplePanel {
        
        protected override Widget createWidget()  {
            return new MaterialApp(
                home: new MaterialThemeSampleWidget(),
                darkTheme: new ThemeData(primaryColor: Colors.black26)
            );
        }
        
        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>(path: "MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }
    }

    public class MaterialThemeSampleWidget: StatefulWidget {
        public override State createState() {
            return new _MaterialThemeSampleWidgetState();
        }
    }

    class _MaterialThemeSampleWidgetState : State<MaterialThemeSampleWidget> {
        public override Widget build(BuildContext context) {
            return new Theme(
                data: new ThemeData(
                    appBarTheme: new AppBarTheme(
                        color: Colors.purple
                    ),
                    bottomAppBarTheme: new BottomAppBarTheme(
                        color: Colors.blue
                    ),
                    cardTheme: new CardTheme(
                        color: Colors.red,
                        elevation: 2.0f
                    )
                ),
                child: new Scaffold(
                    appBar: new AppBar(title: new Text("Test App Bar Theme")),
                    body: new Center(
                        child: new Card(
                            shape: new RoundedRectangleBorder(
                                borderRadius: BorderRadius.all(5.0f)
                            ),
                            child: new Container(
                                height: 250,
                                child: new Column(
                                    children: new List<Widget> {
                                        Image.asset(
                                            "products/backpack",
                                            fit: BoxFit.cover,
                                            width: 200,
                                            height: 200
                                        ),
                                        new Text("Card Theme")
                                    }
                                )
                            )
                        )
                    ),
                    bottomNavigationBar: new BottomAppBar(
                        child: new Row(
                        mainAxisSize: MainAxisSize.max,
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: new List<Widget> {
                            new IconButton(icon: new Icon(Unity.UIWidgets.material.Icons.menu), onPressed: () => { }),
                            new IconButton(icon: new Icon(Unity.UIWidgets.material.Icons.account_balance), onPressed: () => { })
                        })
                    )
                )
            );
        }
    }
}