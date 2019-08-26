using Unity.UIWidgets.animation;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace UIWidgetsSample {
    public class CupertinoSample : UIWidgetsEditorWindow {
        [MenuItem("UIWidgetsTests/CupertinoSample")]
        public static void gallery() {
            GetWindow<CupertinoSample>();
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>("CupertinoIcons"), "CupertinoIcons");
            base.OnEnable();
        }

        protected override Widget createWidget() {
            Debug.Log("[Cupertino Sample] Created");
            return new CupertinoSampleApp();
        }
    }


    public class CupertinoSampleApp : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new CupertinoApp(
                theme: new CupertinoThemeData(
                    textTheme: new CupertinoTextThemeData(
                        navLargeTitleTextStyle: new TextStyle(
                            fontWeight: FontWeight.bold,
                            fontSize: 70f,
                            color: CupertinoColors.activeBlue
                        )
                    )),
                home: new CupertinoSampleWidget()
            );
        }
    }

    public class CupertinoSampleWidget : StatefulWidget {
        public CupertinoSampleWidget(Key key = null) : base(key) { }

        public override State createState() {
            return new CupertinoSampleWidgetState();
        }
    }

    public class CupertinoSampleWidgetState : State<CupertinoSampleWidget> {
        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                child: new Center(
                    child: new Text("Hello Cupertino",
                        style: CupertinoTheme.of(context).textTheme.navLargeTitleTextStyle
                    )
                )
            );
        }
    }
}