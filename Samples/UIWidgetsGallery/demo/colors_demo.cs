using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.gallery {
    class ColorDemoConstants {
        public const float kColorItemHeight = 48.0f;

        public static readonly List<Palette> allPalettes = new List<Palette> {
            new Palette(name: "RED", primary: Colors.red, accent: Colors.redAccent, threshold: 300),
            new Palette(name: "PINK", primary: Colors.pink, accent: Colors.pinkAccent, threshold: 200),
            new Palette(name: "PURPLE", primary: Colors.purple, accent: Colors.purpleAccent, threshold: 200),
            new Palette(name: "DEEP PURPLE", primary: Colors.deepPurple, accent: Colors.deepPurpleAccent,
                threshold: 200),
            new Palette(name: "INDIGO", primary: Colors.indigo, accent: Colors.indigoAccent, threshold: 200),
            new Palette(name: "BLUE", primary: Colors.blue, accent: Colors.blueAccent, threshold: 400),
            new Palette(name: "LIGHT BLUE", primary: Colors.lightBlue, accent: Colors.lightBlueAccent, threshold: 500),
            new Palette(name: "CYAN", primary: Colors.cyan, accent: Colors.cyanAccent, threshold: 600),
            new Palette(name: "TEAL", primary: Colors.teal, accent: Colors.tealAccent, threshold: 400),
            new Palette(name: "GREEN", primary: Colors.green, accent: Colors.greenAccent, threshold: 500),
            new Palette(name: "LIGHT GREEN", primary: Colors.lightGreen, accent: Colors.lightGreenAccent,
                threshold: 600),
            new Palette(name: "LIME", primary: Colors.lime, accent: Colors.limeAccent, threshold: 800),
            new Palette(name: "YELLOW", primary: Colors.yellow, accent: Colors.yellowAccent),
            new Palette(name: "AMBER", primary: Colors.amber, accent: Colors.amberAccent),
            new Palette(name: "ORANGE", primary: Colors.orange, accent: Colors.orangeAccent, threshold: 700),
            new Palette(name: "DEEP ORANGE", primary: Colors.deepOrange, accent: Colors.deepOrangeAccent,
                threshold: 400),
            new Palette(name: "BROWN", primary: Colors.brown, threshold: 200),
            new Palette(name: "GREY", primary: Colors.grey, threshold: 500),
            new Palette(name: "BLUE GREY", primary: Colors.blueGrey, threshold: 500),
        };
    }

    public class Palette {
        public Palette(string name = null, MaterialColor primary = null, MaterialAccentColor accent = null,
            int threshold = 900) {
            this.name = name;
            this.primary = primary;
            this.accent = accent;
            this.threshold = threshold;
        }

        public readonly string name;
        public readonly MaterialColor primary;
        public readonly MaterialAccentColor accent;
        public readonly int threshold;

        public bool isValid {
            get { return this.name != null && this.primary != null; }
        }
    }


    public class ColorItem : StatelessWidget {
        public ColorItem(
            Key key = null,
            int? index = null,
            Color color = null,
            string prefix = ""
        ) : base(key: key) {
            D.assert(index != null);
            D.assert(color != null);
            D.assert(prefix != null);
            this.index = index;
            this.color = color;
            this.prefix = prefix;
        }


        public readonly int? index;
        public readonly Color color;
        public readonly string prefix;

        string colorString() {
            return $"#{this.color.value.ToString("X8").ToUpper()}";
        }

        public override Widget build(BuildContext context) {
            return new Container(
                height: ColorDemoConstants.kColorItemHeight,
                padding: EdgeInsets.symmetric(horizontal: 16.0f),
                color: this.color,
                child: new SafeArea(
                    top: false,
                    bottom: false,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget> {
                            new Text($"{this.prefix}{this.index}"),
                            new Text(this.colorString())
                        }
                    )
                )
            );
        }
    }

    public class PaletteTabView : StatelessWidget {
        public PaletteTabView(
            Key key = null,
            Palette colors = null
        ) : base(key: key) {
            D.assert(colors != null && colors.isValid);
            this.colors = colors;
        }

        public readonly Palette colors;

        public readonly static List<int> primaryKeys = new List<int> {50, 100, 200, 300, 400, 500, 600, 700, 800, 900};
        public readonly static List<int> accentKeys = new List<int> {100, 200, 400, 700};

        public override Widget build(BuildContext context) {
            TextTheme textTheme = Theme.of(context).textTheme;
            TextStyle whiteTextStyle = textTheme.body1.copyWith(color: Colors.white);
            TextStyle blackTextStyle = textTheme.body1.copyWith(color: Colors.black);
            List<Widget> colorItems = primaryKeys.Select<int, Widget>((int index) => {
                return new DefaultTextStyle(
                    style: index > this.colors.threshold ? whiteTextStyle : blackTextStyle,
                    child: new ColorItem(index: index, color: this.colors.primary[index])
                );
            }).ToList();

            if (this.colors.accent != null) {
                colorItems.AddRange(accentKeys.Select<int, Widget>((int index) => {
                    return new DefaultTextStyle(
                        style: index > this.colors.threshold ? whiteTextStyle : blackTextStyle,
                        child: new ColorItem(index: index, color: this.colors.accent[index], prefix: "A")
                    );
                }).ToList());
            }

            return new ListView(
                itemExtent: ColorDemoConstants.kColorItemHeight,
                children: colorItems
            );
        }
    }

    public class ColorsDemo : StatelessWidget {
        public const string routeName = "/colors";

        public override Widget build(BuildContext context) {
            return new DefaultTabController(
                length: ColorDemoConstants.allPalettes.Count,
                child: new Scaffold(
                    appBar: new AppBar(
                        elevation: 0.0f,
                        title: new Text("Colors"),
                        bottom: new TabBar(
                            isScrollable: true,
                            tabs: ColorDemoConstants.allPalettes
                                .Select<Palette, Widget>((Palette swatch) => new Tab(text: swatch.name)).ToList()
                        )
                    ),
                    body: new TabBarView(
                        children: ColorDemoConstants.allPalettes.Select<Palette, Widget>((Palette colors) => {
                            return new PaletteTabView(colors: colors);
                        }).ToList()
                    )
                )
            );
        }
    }
}