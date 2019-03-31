using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using FontStyle = Unity.UIWidgets.ui.FontStyle;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsSample {
    public class FontWeightStyle : UIWidgetsSamplePanel {
        protected override void OnEnable() {
            // To run this sample, you need to download Roboto fonts and place them under Resources/Fonts folder
            // Roboto fonts could be downloaded from google website
            // https://fonts.google.com/specimen/Roboto?selection.family=Roboto
            FontManager.instance.addFont(Resources.Load<Font>(path: "MaterialIcons-Regular"), "Material Icons");
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-Black"), "Roboto",
                FontWeight.w900);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-BlackItalic"), "Roboto",
                FontWeight.w900, FontStyle.italic);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-Bold"), "Roboto",
                FontWeight.bold);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-BoldItalic"), "Roboto",
                FontWeight.bold, FontStyle.italic);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-Regular"), "Roboto",
                FontWeight.normal);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-Italic"), "Roboto",
                FontWeight.normal, FontStyle.italic);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-Medium"), "Roboto",
                FontWeight.w500);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-MediumItalic"), "Roboto",
                FontWeight.w500, FontStyle.italic);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-Light"), "Roboto",
                FontWeight.w300);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-LightItalic"), "Roboto",
                FontWeight.w300, FontStyle.italic);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-Thin"), "Roboto",
                FontWeight.w100);
            FontManager.instance.addFont(Resources.Load<Font>(path: "Fonts/Roboto-ThinItalic"), "Roboto",
                FontWeight.w100, FontStyle.italic);
            
            base.OnEnable();
        }

        protected override Widget createWidget() {
            return new MaterialApp(
                title: "Navigation Basics",
                home: new FontWeightStyleWidget()
            );
        }
    }

    class FontWeightStyleWidget : StatelessWidget {
        public override Widget build(BuildContext context) {
            var fontStyleTexts = new List<Widget> {
                new Text("Thin", style: new TextStyle(fontWeight: FontWeight.w100)),
                new Text("Thin Italic", style: new TextStyle(fontWeight: FontWeight.w100,
                    fontStyle: FontStyle.italic)),
                new Text("Light", style: new TextStyle(fontWeight: FontWeight.w300)),
                new Text("Light Italic", style: new TextStyle(fontWeight: FontWeight.w300,
                    fontStyle: FontStyle.italic)),
                new Text("Regular", style: new TextStyle(fontWeight: FontWeight.normal)),
                new Text("Regular Italic", style: new TextStyle(fontWeight: FontWeight.normal,
                    fontStyle: FontStyle.italic)),
                new Text("Medium", style: new TextStyle(fontWeight: FontWeight.w500)),
                new Text("Medium Italic", style: new TextStyle(fontWeight: FontWeight.w500,
                    fontStyle: FontStyle.italic)),
                new Text("Bold", style: new TextStyle(fontWeight: FontWeight.bold)),
                new Text("Bold Italic", style: new TextStyle(fontWeight: FontWeight.bold,
                    fontStyle: FontStyle.italic)),
                new Text("Black", style: new TextStyle(fontWeight: FontWeight.w900)),
                new Text("Black Italic", style: new TextStyle(fontWeight: FontWeight.w900,
                    fontStyle: FontStyle.italic)),
            };
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Font weight & style")
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