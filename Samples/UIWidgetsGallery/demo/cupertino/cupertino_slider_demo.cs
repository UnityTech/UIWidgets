using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class CupertinoSliderDemo : StatefulWidget {
        public static string routeName = "/cupertino/slider";

        public override State createState() {
            return new _CupertinoSliderDemoState();
        }
    }

    class _CupertinoSliderDemoState : State<CupertinoSliderDemo> {
        float _value = 25.0f;
        float _discreteValue = 20.0f;

        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                navigationBar: new CupertinoNavigationBar(
                    middle: new Text("Sliders"),
                    previousPageTitle: "Cupertino",
                    trailing: new CupertinoDemoDocumentationButton(CupertinoSliderDemo.routeName)
                ),
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new SafeArea(
                        child: new Center(
                            child: new Column(
                                mainAxisAlignment: MainAxisAlignment.spaceAround,
                                children: new List<Widget> {
                                    new Column(
                                        mainAxisSize: MainAxisSize.min,
                                        children: new List<Widget> {
                                            new CupertinoSlider(
                                                value: this._value,
                                                min: 0.0f,
                                                max: 100.0f,
                                                onChanged: (float value) => {
                                                    this.setState(() => { this._value = value; });
                                                }
                                            ),
                                            new Text($"Cupertino Continuous: {this._value.ToString("F1")}"),
                                        }
                                    ),
                                    new Column(
                                        mainAxisSize: MainAxisSize.min,
                                        children: new List<Widget> {
                                            new CupertinoSlider(
                                                value: this._discreteValue,
                                                min: 0.0f,
                                                max: 100.0f,
                                                divisions: 5,
                                                onChanged: (float value) => {
                                                    this.setState(() => { this._discreteValue = value; });
                                                }
                                            ),
                                            new Text($"Cupertino Discrete: {this._discreteValue}"),
                                        }
                                    ),
                                }
                            )
                        )
                    )
                )
            );
        }
    }
}