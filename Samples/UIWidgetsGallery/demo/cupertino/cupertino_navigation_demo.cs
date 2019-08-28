using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgetsGallery.gallery {
    class CupertinoNavigationDemoUtils {
        public const int _kChildCount = 50;

        public static List<Color> coolColors = new List<Color> {
            Color.fromARGB(255, 255, 59, 48),
            Color.fromARGB(255, 255, 149, 0),
            Color.fromARGB(255, 255, 204, 0),
            Color.fromARGB(255, 76, 217, 100),
            Color.fromARGB(255, 90, 200, 250),
            Color.fromARGB(255, 0, 122, 255),
            Color.fromARGB(255, 88, 86, 214),
            Color.fromARGB(255, 255, 45, 85),
        };

        public static List<string> coolColorNames = new List<string> {
            "Sarcoline", "Coquelicot", "Smaragdine", "Mikado", "Glaucous", "Wenge",
            "Fulvous", "Xanadu", "Falu", "Eburnean", "Amaranth", "Australien",
            "Banan", "Falu", "Gingerline", "Incarnadine", "Labrador", "Nattier",
            "Pervenche", "Sinoper", "Verditer", "Watchet", "Zafre",
        };

        public static Widget trailingButtons {
            get {
                return new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget> {
                        new CupertinoDemoDocumentationButton(CupertinoNavigationDemo.routeName),
                        new Padding(padding: EdgeInsets.only(left: 8.0f)),
                        new ExitButton(),
                    }
                );
            }
        }

        public static List<Widget> buildTab2Conversation() {
            return new List<Widget> {
                new Tab2ConversationRow(
                    text: "My Xanadu doesn't look right"
                ),
                new Tab2ConversationRow(
                    avatar: new Tab2ConversationAvatar(
                        text: "KL",
                        color: new Color(0xFFFD5015)
                    ),
                    text: "We'll rush you a new one.\nIt's gonna be incredible"
                ),
                new Tab2ConversationRow(
                    text: "Awesome thanks!"
                ),
                new Tab2ConversationRow(
                    avatar: new Tab2ConversationAvatar(
                        text: "SJ",
                        color: new Color(0xFF34CAD6)
                    ),
                    text: "We'll send you our\nnewest Labrador too!"
                ),
                new Tab2ConversationRow(
                    text: "Yay"
                ),
                new Tab2ConversationRow(
                    avatar: new Tab2ConversationAvatar(
                        text: "KL",
                        color: new Color(0xFFFD5015)
                    ),
                    text: "Actually there's one more thing..."
                ),
                new Tab2ConversationRow(
                    text: "What's that ? "
                ),
            };
        }
    }

    public class CupertinoNavigationDemo : StatelessWidget {
        public CupertinoNavigationDemo() {
            this.colorItems = new List<Color>();

            for (int i = 0; i < CupertinoNavigationDemoUtils._kChildCount; i++) {
                this.colorItems.Add(CupertinoNavigationDemoUtils.coolColors[
                    Random.Range(0, CupertinoNavigationDemoUtils.coolColors.Count)
                ]);
            }

            this.colorNameItems = new List<string>();

            for (int i = 0; i < CupertinoNavigationDemoUtils._kChildCount; i++) {
                this.colorNameItems.Add(CupertinoNavigationDemoUtils.coolColorNames[
                    Random.Range(0, CupertinoNavigationDemoUtils.coolColorNames.Count)
                ]);
            }
        }

        public static string routeName = "/cupertino/navigation";
        public readonly List<Color> colorItems;
        public readonly List<string> colorNameItems;

        public override Widget build(BuildContext context) {
            return new WillPopScope(
                onWillPop: () => { return Promise<bool>.Resolved(true); },
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new CupertinoTabScaffold(
                        tabBar: new CupertinoTabBar(
                            items: new List<BottomNavigationBarItem> {
                                new BottomNavigationBarItem(
                                    icon: new Icon(CupertinoIcons.home),
                                    title: new Text("Home")
                                ),
                                new BottomNavigationBarItem(
                                    icon: new Icon(CupertinoIcons.conversation_bubble),
                                    title: new Text("Support")
                                ),
                                new BottomNavigationBarItem(
                                    icon: new Icon(CupertinoIcons.profile_circled),
                                    title: new Text("Profile")
                                )
                            }
                        ),
                        tabBuilder: (BuildContext _context, int index) => {
                            D.assert(index >= 0 && index <= 2);
                            switch (index) {
                                case 0:
                                    return new CupertinoTabView(
                                        builder: (BuildContext _context1) => {
                                            return new CupertinoDemoTab1(
                                                colorItems: this.colorItems,
                                                colorNameItems: this.colorNameItems
                                            );
                                        },
                                        defaultTitle: "Colors"
                                    );
                                case 1:
                                    return new CupertinoTabView(
                                        builder: (BuildContext _context2) => new CupertinoDemoTab2(),
                                        defaultTitle: "Support Chat"
                                    );
                                case 2:
                                    return new CupertinoTabView(
                                        builder: (BuildContext _context3) => new CupertinoDemoTab3(),
                                        defaultTitle: "Account"
                                    );
                            }

                            return null;
                        }
                    )
                )
            );
        }
    }

    class ExitButton : StatelessWidget {
        public ExitButton() { }

        public override Widget build(BuildContext context) {
            return new CupertinoButton(
                padding: EdgeInsets.zero,
                child: new Tooltip(
                    message: "Back",
                    child: new Text("Exit")
                ),
                onPressed: () => { Navigator.of(context, rootNavigator: true).pop(); }
            );
        }
    }


    class CupertinoDemoTab1 : StatelessWidget {
        public CupertinoDemoTab1(
            List<Color> colorItems = null,
            List<string> colorNameItems = null
        ) {
            this.colorItems = colorItems ?? new List<Color>();
            this.colorNameItems = colorNameItems ?? new List<string>();
        }

        public readonly List<Color> colorItems;
        public readonly List<string> colorNameItems;

        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                child: new CustomScrollView(
                    slivers: new List<Widget> {
                        new CupertinoSliverNavigationBar(
                            trailing: CupertinoNavigationDemoUtils.trailingButtons
                        ),
                        new SliverPadding(
                            padding: MediaQuery.of(context).removePadding(
                                removeTop: true,
                                removeLeft: true,
                                removeRight: true
                            ).padding,
                            sliver: new SliverList(
                                del: new SliverChildBuilderDelegate(
                                    (BuildContext _context, int index) => {
                                        return new Tab1RowItem(
                                            index: index,
                                            lastItem: index == CupertinoNavigationDemoUtils._kChildCount - 1,
                                            color: this.colorItems[index],
                                            colorName: this.colorNameItems[index]
                                        );
                                    },
                                    childCount: CupertinoNavigationDemoUtils._kChildCount
                                )
                            )
                        )
                    }
                )
            );
        }
    }

    class Tab1RowItem : StatelessWidget {
        public Tab1RowItem(
            int index,
            bool lastItem,
            Color color,
            string colorName
        ) {
            this.index = index;
            this.lastItem = lastItem;
            this.color = color;
            this.colorName = colorName;
        }

        public readonly int index;
        public readonly bool lastItem;
        public readonly Color color;
        public readonly string colorName;

        public override Widget build(BuildContext context) {
            Widget row = new GestureDetector(
                behavior: HitTestBehavior.opaque,
                onTap: () => {
                    Navigator.of(context).push(new CupertinoPageRoute(
                        title: this.colorName,
                        builder: (BuildContext _context) => new Tab1ItemPage(
                            color: this.color,
                            colorName: this.colorName,
                            index: this.index
                        )
                    ));
                },
                child: new SafeArea(
                    top: false,
                    bottom: false,
                    child: new Padding(
                        padding: EdgeInsets.only(left: 16.0f, top: 8.0f, bottom: 8.0f, right: 8.0f),
                        child: new Row(
                            children: new List<Widget> {
                                new Container(
                                    height: 60.0f,
                                    width: 60.0f,
                                    decoration: new BoxDecoration(
                                        color: this.color,
                                        borderRadius: BorderRadius.circular(8.0f)
                                    )
                                ),
                                new Expanded(
                                    child: new Padding(
                                        padding: EdgeInsets.symmetric(horizontal: 12.0f),
                                        child: new Column(
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: new List<Widget> {
                                                new Text(this.colorName),
                                                new Padding(padding: EdgeInsets.only(top: 8.0f)),
                                                new Text(
                                                    "Buy this cool color",
                                                    style: new TextStyle(
                                                        color: new Color(0xFF8E8E93),
                                                        fontSize: 13.0f,
                                                        fontWeight: FontWeight.w300
                                                    )
                                                )
                                            }
                                        )
                                    )
                                ),
                                new CupertinoButton(
                                    padding: EdgeInsets.zero,
                                    child: new Icon(CupertinoIcons.plus_circled
                                    ),
                                    onPressed: () => { }
                                ),
                                new CupertinoButton(
                                    padding: EdgeInsets.zero,
                                    child: new Icon(CupertinoIcons.share
                                    ),
                                    onPressed: () => { }
                                )
                            }
                        )
                    )
                )
            );
            if (this.lastItem) {
                return row;
            }

            return new Column(
                children: new List<Widget> {
                    row,
                    new Container(
                        height: 1.0f,
                        color: new Color(0xFFD9D9D9)
                    )
                }
            );
        }
    }

    class Tab1ItemPage : StatefulWidget {
        public Tab1ItemPage(
            Color color,
            string colorName,
            int index
        ) {
            this.color = color;
            this.colorName = colorName;
            this.index = index;
        }

        public readonly Color color;
        public readonly string colorName;
        public readonly int index;

        public override State createState() {
            return new Tab1ItemPageState();
        }
    }

    class Tab1ItemPageState : State<Tab1ItemPage> {
        public override void initState() {
            base.initState();

            this.relatedColors = new List<Color>();
            for (int i = 0; i < 10; i++) {
                this.relatedColors.Add(Color.fromARGB(
                    255,
                    (this.widget.color.red + Random.Range(-50, 50)).clamp(0, 255),
                    (this.widget.color.green + Random.Range(-50, 50)).clamp(0, 255),
                    (this.widget.color.blue + Random.Range(-50, 50)).clamp(0, 255)
                ));
            }
        }

        List<Color> relatedColors;

        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                navigationBar: new CupertinoNavigationBar(
                    trailing: new ExitButton()
                ),
                child: new SafeArea(
                    bottom: false,
                    child: new ListView(
                        children: new List<Widget> {
                            new Padding(padding: EdgeInsets.only(top: 16.0f)),
                            new Padding(
                                padding: EdgeInsets.symmetric(horizontal: 16.0f),
                                child: new Row(
                                    mainAxisSize: MainAxisSize.max,
                                    children: new List<Widget> {
                                        new Container(
                                            height: 128.0f,
                                            width: 128.0f,
                                            decoration: new BoxDecoration(
                                                color: this.widget.color,
                                                borderRadius: BorderRadius.circular(24.0f)
                                            )
                                        ),
                                        new Padding(padding: EdgeInsets.only(left: 18.0f)),
                                        new Expanded(
                                            child: new Column(
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                mainAxisSize: MainAxisSize.min,
                                                children: new List<Widget> {
                                                    new Text(this.widget.colorName,
                                                        style: new TextStyle(fontSize: 24.0f,
                                                            fontWeight: FontWeight.bold)
                                                    ),
                                                    new Padding(padding: EdgeInsets.only(top: 6.0f)),
                                                    new Text(
                                                        $"Item number {this.widget.index}",
                                                        style: new TextStyle(
                                                            color: new Color(0xFF8E8E93),
                                                            fontSize: 16.0f,
                                                            fontWeight: FontWeight.w100
                                                        )
                                                    ),
                                                    new Padding(padding: EdgeInsets.only(top: 20.0f)),
                                                    new Row(
                                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                                        children: new List<Widget> {
                                                            CupertinoButton.filled(
                                                                minSize: 30.0f,
                                                                padding: EdgeInsets.symmetric(horizontal: 24.0f),
                                                                borderRadius: BorderRadius.circular(16.0f),
                                                                child: new Text(
                                                                    "GET",
                                                                    style: new TextStyle(
                                                                        fontSize: 14.0f,
                                                                        fontWeight: FontWeight.w700,
                                                                        letterSpacing: -0.28f
                                                                    )
                                                                ),
                                                                onPressed: () => { }
                                                            ),
                                                            CupertinoButton.filled(
                                                                minSize: 30.0f,
                                                                padding: EdgeInsets.zero,
                                                                borderRadius: BorderRadius.circular(16.0f),
                                                                child: new Icon(CupertinoIcons.ellipsis),
                                                                onPressed: () => { }
                                                            )
                                                        }
                                                    )
                                                }
                                            )
                                        )
                                    }
                                )
                            ),
                            new Padding(
                                padding: EdgeInsets.only(left: 16.0f, top: 28.0f, bottom: 8.0f),
                                child: new Text(
                                    "USERS ALSO LIKED",
                                    style: new TextStyle(
                                        color: new Color(0xFF646464),
                                        letterSpacing: -0.60f,
                                        fontSize: 15.0f,
                                        fontWeight: FontWeight.w500
                                    )
                                )
                            ),
                            new SizedBox(
                                height: 200.0f,
                                child: ListView.builder(
                                    scrollDirection: Axis.horizontal,
                                    itemCount: 10,
                                    itemExtent: 160.0f,
                                    itemBuilder: (BuildContext _context, int index) => {
                                        return new Padding(
                                            padding: EdgeInsets.only(left: 16.0f),
                                            child: new Container(
                                                decoration: new BoxDecoration(
                                                    borderRadius: BorderRadius.circular(8.0f),
                                                    color: this.relatedColors[index]
                                                ),
                                                child: new Center(
                                                    child: new CupertinoButton(
                                                        child: new Icon(
                                                            CupertinoIcons.plus_circled,
                                                            color: CupertinoColors.white,
                                                            size: 36.0f
                                                        ),
                                                        onPressed: () => { }
                                                    )
                                                )
                                            )
                                        );
                                    }
                                )
                            )
                        }
                    )
                )
            );
        }
    }

    class CupertinoDemoTab2 : StatelessWidget {
        public override Widget build(BuildContext context) {
            var listViewList = new List<Widget>();
            listViewList.Add(new Tab2Header());
            listViewList.AddRange(CupertinoNavigationDemoUtils.buildTab2Conversation());

            return new CupertinoPageScaffold(
                navigationBar: new CupertinoNavigationBar(
                    trailing: CupertinoNavigationDemoUtils.trailingButtons
                ),
                child:
                new SafeArea(
                    child: new ListView(
                        children: listViewList
                    )
                )
            );
        }
    }

    class Tab2Header : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new Padding(
                padding: EdgeInsets.all(16.0f),
                child: new ClipRRect(
                    borderRadius: BorderRadius.all(Radius.circular(16.0f)),
                    child: new Column(
                        mainAxisSize: MainAxisSize.min,
                        children: new List<Widget> {
                            new Container(
                                decoration: new BoxDecoration(
                                    color: new Color(0xFFE5E5E5)
                                ),
                                child: new Padding(
                                    padding: EdgeInsets.symmetric(horizontal: 18.0f, vertical: 12.0f),
                                    child: new Row(
                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                        children: new List<Widget> {
                                            new Text(
                                                "SUPPORT TICKET",
                                                style: new TextStyle(
                                                    color: new Color(0xFF646464),
                                                    letterSpacing: -0.9f,
                                                    fontSize: 14.0f,
                                                    fontWeight: FontWeight.w500
                                                )
                                            ),
                                            new Text(
                                                "Show More",
                                                style: new TextStyle(
                                                    color: new Color(0xFF646464),
                                                    letterSpacing: -0.6f,
                                                    fontSize: 12.0f,
                                                    fontWeight: FontWeight.w500
                                                )
                                            )
                                        }
                                    )
                                )
                            ),
                            new Container(
                                decoration: new BoxDecoration(
                                    color: new Color(0xFFF3F3F3)
                                ),
                                child: new Padding(
                                    padding: EdgeInsets.symmetric(horizontal: 18.0f, vertical: 12.0f),
                                    child: new Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: new List<Widget> {
                                            new Text(
                                                "Product or product packaging damaged during transit",
                                                style: new TextStyle(
                                                    fontSize: 16.0f,
                                                    fontWeight: FontWeight.w700,
                                                    letterSpacing: -0.46f
                                                )
                                            ),
                                            new Padding(padding: EdgeInsets.only(top: 16.0f)),
                                            new Text(
                                                "REVIEWERS",
                                                style: new TextStyle(
                                                    color: new Color(0xFF646464),
                                                    fontSize: 12.0f,
                                                    letterSpacing: -0.6f,
                                                    fontWeight: FontWeight.w500
                                                )
                                            ),
                                            new Padding(padding: EdgeInsets.only(top: 8.0f)),
                                            new Row(
                                                children: new List<Widget> {
                                                    new Container(
                                                        width: 44.0f,
                                                        height: 44.0f,
                                                        decoration: new BoxDecoration(
                                                            image: new DecorationImage(
                                                                image: new AssetImage(
                                                                    "people/square/trevor"
                                                                )
                                                            ),
                                                            shape: BoxShape.circle
                                                        )
                                                    ),
                                                    new Padding(padding: EdgeInsets.only(left: 8.0f)),
                                                    new Container(
                                                        width: 44.0f,
                                                        height: 44.0f,
                                                        decoration: new BoxDecoration(
                                                            image: new DecorationImage(
                                                                image: new AssetImage(
                                                                    "people/square/sandra"
                                                                )
                                                            ),
                                                            shape: BoxShape.circle
                                                        )
                                                    ),
                                                    new Padding(padding: EdgeInsets.only(left: 2.0f)),
                                                    new Icon(
                                                        CupertinoIcons.check_mark_circled,
                                                        color: new Color(0xFF646464),
                                                        size: 20.0f
                                                    )
                                                }
                                            )
                                        }
                                    )
                                )
                            )
                        }
                    )
                )
            );
        }
    }

    enum Tab2ConversationBubbleColor {
        blue,
        gray,
    }

    class Tab2ConversationBubble : StatelessWidget {
        public Tab2ConversationBubble(
            string text,
            Tab2ConversationBubbleColor color
        ) {
            this.text = text;
            this.color = color;
        }

        public readonly string text;
        public readonly Tab2ConversationBubbleColor color;

        public override Widget build(BuildContext context) {
            return new Container(
                decoration: new BoxDecoration(
                    borderRadius: BorderRadius.all(Radius.circular(18.0f)),
                    color: this.color == Tab2ConversationBubbleColor.blue
                        ? CupertinoColors.activeBlue
                        : CupertinoColors.lightBackgroundGray
                ),
                margin: EdgeInsets.symmetric(horizontal: 8.0f, vertical: 8.0f),
                padding: EdgeInsets.symmetric(horizontal: 14.0f, vertical: 10.0f),
                child: new Text(this.text,
                    style: new TextStyle(
                        color: this.color == Tab2ConversationBubbleColor.blue
                            ? CupertinoColors.white
                            : CupertinoColors.black,
                        letterSpacing: -0.4f,
                        fontSize: 15.0f,
                        fontWeight: FontWeight.w400
                    )
                )
            );
        }
    }

    class Tab2ConversationAvatar : StatelessWidget {
        public Tab2ConversationAvatar(
            string text,
            Color color
        ) {
            this.text = text;
            this.color = color;
        }

        public readonly string text;
        public readonly Color color;

        public override Widget build(BuildContext context) {
            return new Container(
                decoration: new BoxDecoration(
                    shape: BoxShape.circle,
                    gradient: new LinearGradient(
                        begin: Alignment.topCenter, // FractionalOfset.topCenter,
                        end: Alignment.bottomCenter, // FractionalOfset.bottomCenter,
                        colors: new List<Color> {
                            this.color,
                            Color.fromARGB(this.color.alpha,
                                (this.color.red - 60).clamp(0, 255),
                                (this.color.green - 60).clamp(0, 255),
                                (this.color.blue - 60).clamp(0, 255)
                            )
                        }
                    )
                ),
                margin: EdgeInsets.only(left: 8.0f, bottom: 8.0f),
                padding: EdgeInsets.all(12.0f),
                child: new Text(this.text,
                    style: new TextStyle(
                        color: CupertinoColors.white,
                        fontSize: 13.0f,
                        fontWeight: FontWeight.w500
                    )
                )
            );
        }
    }

    class Tab2ConversationRow : StatelessWidget {
        public Tab2ConversationRow(
            string text,
            Tab2ConversationAvatar avatar = null
        ) {
            this.avatar = avatar;
            this.text = text;
        }

        public readonly Tab2ConversationAvatar avatar;
        public readonly string text;

        public override Widget build(BuildContext context) {
            List<Widget> children = new List<Widget>();

            if (this.avatar != null) {
                children.Add(this.avatar);
            }

            bool isSelf = this.avatar == null;
            children.Add(
                new Tab2ConversationBubble(
                    text: this.text,
                    color: isSelf
                        ? Tab2ConversationBubbleColor.blue
                        : Tab2ConversationBubbleColor.gray
                )
            );
            return new Row(
                mainAxisAlignment: isSelf ? MainAxisAlignment.end : MainAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: isSelf ? CrossAxisAlignment.center : CrossAxisAlignment.end,
                children: children
            );
        }
    }


    class CupertinoDemoTab3 : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                navigationBar: new CupertinoNavigationBar(
                    trailing: CupertinoNavigationDemoUtils.trailingButtons
                ),
                child: new SafeArea(
                    child: new DecoratedBox(
                        decoration: new BoxDecoration(
                            color: CupertinoTheme.of(context).brightness == Brightness.light
                                ? CupertinoColors.extraLightBackgroundGray
                                : CupertinoColors.darkBackgroundGray
                        ),
                        child: new ListView(
                            children: new List<Widget> {
                                new Padding(padding: EdgeInsets.only(top: 32.0f)),
                                new GestureDetector(
                                    onTap: () => {
                                        Navigator.of(context, rootNavigator: true).push(
                                            new CupertinoPageRoute(
                                                fullscreenDialog: true,
                                                builder: (BuildContext _context) => new Tab3Dialog()
                                            )
                                        );
                                    },
                                    child: new Container(
                                        decoration: new BoxDecoration(
                                            color: CupertinoTheme.of(context).scaffoldBackgroundColor,
                                            border: new Border(
                                                top: new BorderSide(color: new Color(0xFFBCBBC1), width: 0.0f),
                                                bottom: new BorderSide(color: new Color(0xFFBCBBC1), width: 0.0f)
                                            )
                                        ),
                                        height: 44.0f,
                                        child: new Padding(
                                            padding: EdgeInsets.symmetric(horizontal: 16.0f, vertical: 8.0f),
                                            child: new SafeArea(
                                                top: false,
                                                bottom: false,
                                                child: new Row(
                                                    children: new List<Widget> {
                                                        new Text(
                                                            "Sign in",
                                                            style: new TextStyle(color: CupertinoTheme.of(context)
                                                                .primaryColor)
                                                        ),
                                                    }
                                                )
                                            )
                                        )
                                    )
                                )
                            }
                        )
                    )
                )
            );
        }
    }

    class Tab3Dialog : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                navigationBar: new CupertinoNavigationBar(
                    leading: new CupertinoButton(
                        child: new Text("Cancel"),
                        padding: EdgeInsets.zero,
                        onPressed: () => { Navigator.of(context).pop(false); }
                    )
                ),
                child: new Center(
                    child: new Column(
                        mainAxisSize: MainAxisSize.min,
                        children: new List<Widget> {
                            new Icon(
                                CupertinoIcons.profile_circled,
                                size: 160.0f,
                                color: new Color(0xFF646464)
                            ),
                            new Padding(padding: EdgeInsets.only(top: 18.0f)),
                            CupertinoButton.filled(
                                child: new Text("Sign in"),
                                onPressed: () => { Navigator.pop(context); }
                            ),
                        }
                    )
                )
            );
        }
    }
}