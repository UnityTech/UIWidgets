using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsSample {
    
    public class TextStyleSample : UIWidgetsSamplePanel {
      
        protected override Widget createWidget() {
            return new MaterialApp(
                title: "Text Style",
                home: new TextStyleSampleWidget()
            );
        }
    }

    class TextStyleSampleWidget : StatelessWidget {
        public override Widget build(BuildContext context) {
            var fontStyleTexts = new List<Widget> {
                new Text("text", style: new TextStyle(fontSize: 18)),
                new Text("text with font size 0 below", style: new TextStyle(fontSize: 14)),
                new Text("font size 0", style: new TextStyle(fontSize: 0)),
                new Text("text with font size 0 above", style: new TextStyle(fontSize: 14)),
                new Text("text with font size 0.3f", style: new TextStyle(fontSize: 0.3f)),
                new Text("Text with background", style: new TextStyle(fontSize: 14, background:
                    new Paint(){color = new Color(0xFF00FF00)})),
                new Text("positive letter spacing", style: new TextStyle(fontSize: 14, letterSpacing:5)),
                new Text("negative letter spacing", style: new TextStyle(fontSize: 14, letterSpacing:-1)),
                new Text("positive word spacing test", style: new TextStyle(fontSize: 14, wordSpacing: 20f)),
                new Text("negative word spacing test", style: new TextStyle(fontSize: 14, wordSpacing: -4f)),
                
            };
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Text Style")
                ),
                body: new Card(
                    child: new DefaultTextStyle(
                        style: new TextStyle(fontSize: 40, fontFamily: "Roboto"),
                        child: new ListView(children: fontStyleTexts))
                )
            );
        }
    }
}