using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    public class TextStyleItem : StatelessWidget {
        public TextStyleItem(
            Key key = null,
            string name = null,
            TextStyle style = null,
            string text = null
        ) : base(key: key) {
            D.assert(name != null);
            D.assert(style != null);
            D.assert(text != null);
            this.name = name;
            this.style = style;
            this.text = text;
        }

        public readonly string name;
        public readonly TextStyle style;
        public readonly string text;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            TextStyle nameStyle = theme.textTheme.caption.copyWith(color: theme.textTheme.caption.color);
            return new Padding(
                padding: EdgeInsets.symmetric(horizontal: 8.0f, vertical: 16.0f),
                child: new Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new SizedBox(
                            width: 72.0f,
                            child: new Text(this.name, style: nameStyle)
                        ),
                        new Expanded(
                            child: new Text(this.text, style: this.style.copyWith(height: 1.0f))
                        )
                    }
                )
            );
        }
    }

    public class TypographyDemo : StatelessWidget {
        public const string routeName = "/typography";

        public override Widget build(BuildContext context) {
            TextTheme textTheme = Theme.of(context).textTheme;
            List<Widget> styleItems = new List<Widget> {
                new TextStyleItem(name: "Display 3", style: textTheme.display3, text: "Regular 56sp"),
                new TextStyleItem(name: "Display 2", style: textTheme.display2, text: "Regular 45sp"),
                new TextStyleItem(name: "Display 1", style: textTheme.display1, text: "Regular 34sp"),
                new TextStyleItem(name: "Headline", style: textTheme.headline, text: "Regular 24sp"),
                new TextStyleItem(name: "Title", style: textTheme.title, text: "Medium 20sp"),
                new TextStyleItem(name: "Subheading", style: textTheme.subhead, text: "Regular 16sp"),
                new TextStyleItem(name: "Body 2", style: textTheme.body2, text: "Medium 14sp"),
                new TextStyleItem(name: "Body 1", style: textTheme.body1, text: "Regular 14sp"),
                new TextStyleItem(name: "Caption", style: textTheme.caption, text: "Regular 12sp"),
                new TextStyleItem(name: "Button", style: textTheme.button, text: "MEDIUM (ALL CAPS) 14sp")
            };

            if (MediaQuery.of(context).size.width > 500.0f) {
                styleItems.Insert(0, new TextStyleItem(
                    name: "Display 4",
                    style: textTheme.display4,
                    text: "Light 112sp"
                ));
            }

            return new Scaffold(
                appBar: new AppBar(title: new Text("Typography")),
                body: new SafeArea(
                    top: false,
                    bottom: false,
                    child: new ListView(children: styleItems)
                )
            );
        }
    }
}