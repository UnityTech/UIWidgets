using UIWidgets.painting;
using UIWidgets.editor;
using UIWidgets.widgets;
using System.Collections.Generic;
using UIWidgets.rendering;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.ui;
using Color = UIWidgets.ui.Color;
using TextStyle = UIWidgets.painting.TextStyle;

namespace UIWidgets.Tests {
    public class Widgets : EditorWindow {
        private WindowAdapter windowAdapter;

        private PaintingBinding paintingBinding;

        private readonly Func<Widget>[] _options;

        private readonly string[] _optionStrings;

        private int _selected;

        [NonSerialized] private bool hasInvoked = false;

        Widgets() {
            this._options = new Func<Widget>[] {
                this.container,
                this.flexRow,
                this.flexColumn,
                this.containerSimple,
                this.eventsPage,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("Widgets Test");
        }

        void OnGUI() {
            var selected = EditorGUILayout.Popup("test case", this._selected, this._optionStrings);
            if (selected != this._selected || !this.hasInvoked) {
                this._selected = selected;
                this.hasInvoked = true;

                var rootWidget = this._options[this._selected]();

                this.windowAdapter.attachRootWidget(rootWidget);
            }

            if (this.windowAdapter != null) {
                this.windowAdapter.OnGUI();
            }
        }

        private void Update() {
            if (this.windowAdapter != null) {
                this.windowAdapter.Update();
            }
        }

        private void OnEnable() {
            this.paintingBinding = new PaintingBinding(null);
            paintingBinding.initInstances();
            this.windowAdapter = new WindowAdapter(this);
        }

        void OnDestroy() {
            this.windowAdapter = null;
        }

        Widget flexRow() {
            var image = new widgets.Image(
                "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                width: 100,
                height: 100
            );
            List<Widget> rowImages = new List<Widget>();
            rowImages.Add(image);
            rowImages.Add(image);
            rowImages.Add(image);
            rowImages.Add(image);

            var row = new widgets.Row(
                textDirection: null,
                textBaseline: null,
                key: null,
                mainAxisAlignment: MainAxisAlignment.start,
                mainAxisSize: MainAxisSize.max,
                crossAxisAlignment: CrossAxisAlignment.center,
                verticalDirection: VerticalDirection.down,
                children: rowImages
            );

            return row;
        }

        Widget flexColumn() {
            var image = new widgets.Image(
                "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                width: 100,
                height: 100
            );
            List<Widget> columnImages = new List<Widget>();
            columnImages.Add(image);
            columnImages.Add(image);
            columnImages.Add(image);

            var column = new widgets.Column(
                textDirection: null,
                textBaseline: null,
                key: null,
                mainAxisAlignment: MainAxisAlignment.start,
                mainAxisSize: MainAxisSize.max,
                crossAxisAlignment: CrossAxisAlignment.center,
                verticalDirection: VerticalDirection.down,
                children: columnImages
            );

            return column;
        }

        Widget container() {
            var image = new widgets.Image(
                "https://tse3.mm.bing.net/th?id=OIP.XOAIpvR1kh-CzISe_Nj9GgHaHs&pid=Api",
                width: 100,
                height: 100,
                repeat: ImageRepeat.repeatX
            );
            var container = new widgets.Container(
                width: 200,
                height: 200,
                margin: EdgeInsets.all(30.0),
                padding: EdgeInsets.all(15.0),
                color: ui.Color.fromARGB(255, 244, 190, 85),
                child: image
            );

            return container;
        }

        Widget containerSimple() {
            var container = new Container(
                alignment: Alignment.centerRight,
                color: ui.Color.fromARGB(255, 244, 190, 85),
                child: new Container(
                    width: 120,
                    height: 120,
                    color: ui.Color.fromARGB(255, 255, 0, 85)
                )
            );

            return container;
        }

        Widget eventsPage() {
            return new EventsWaterfallScreen();
        }
    }

    public class EventsWaterfallScreen : StatefulWidget {
        public EventsWaterfallScreen(Key key = null) : base(key: key) {
        }

        public override State createState() {
            return new _EventsWaterfallScreenState();
        }
    }

    class _EventsWaterfallScreenState : State<EventsWaterfallScreen> {
        const double headerHeight = 80.0;

        double _offsetY = 0.0;

        Widget _buildHeader(BuildContext context) {
            return new Container(
                padding: EdgeInsets.only(left: 16.0, right: 8.0),
                height: headerHeight - _offsetY,
                child: new Flex(
                    direction: Axis.horizontal,
                    children: new List<Widget> {
                        new Row(
                            children: new List<Widget> {
                                new Text(
                                    "Today",
                                    style: new TextStyle(
                                        fontSize: (34.0 / headerHeight) * (headerHeight - _offsetY),
                                        color: CLColors.white
                                    )
                                ),

                                new CustomButton(
                                    padding: EdgeInsets.all(8.0),
                                    child: new Icon(
                                        Icons.notifications,
                                        size: 28.0,
                                        color: CLColors.icon2
                                    )
                                ),
                                new CustomButton(
                                    padding: EdgeInsets.all(8.0),
                                    child: new Icon(
                                        Icons.account_circle,
                                        size: 28.0,
                                        color: CLColors.icon2
                                    )
                                )
                            }
                        )
                    }
                )
            );
        }
        
        

        Widget _buildContentList(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
                    return true;
                },
                child: new Container()
            );
        }

        public override Widget build(BuildContext context) {
            var container = new Container(
                color: CLColors.background1,
                child: new Container(
                    color: CLColors.background1,
                    child: new Column(
                        children: new List<Widget> {
                            this._buildHeader(context),
                            this._buildContentList(context)
                        }
                    )
                )
            );
            return container;
        }
    }

    public class CustomButton : StatelessWidget {
        public CustomButton(
            Key key = null,
            GestureTapCallback onPressed = null,
            EdgeInsets padding = null,
            Color backgroundColor = null,
            Widget child = null
        ) : base(key: key) {
            this.onPressed = onPressed;
            this.padding = padding ?? EdgeInsets.all(8.0);
            this.backgroundColor = backgroundColor ?? CLColors.transparent;
            this.child = child;
        }

        public readonly GestureTapCallback onPressed;
        public readonly EdgeInsets padding;
        public readonly Widget child;
        public readonly Color backgroundColor;

        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onTap: this.onPressed,
                child: new Container(
                    padding: this.padding,
                    color: this.backgroundColor,
                    child: this.child
                )
            );
        }
    }

    public static class Icons {
        public static readonly IconData notifications = new IconData(0xe7f4, fontFamily: "MaterialIcons");
        public static readonly IconData account_circle = new IconData(0xe853, fontFamily: "MaterialIcons");
    }

    public static class CLColors {
        public static readonly Color primary = new Color(0xFFE91E63);
        public static readonly Color secondary1 = new Color(0xFF00BCD4);
        public static readonly Color secondary2 = new Color(0xFFF0513C);
        public static readonly Color background1 = new Color(0xFF292929);
        public static readonly Color background2 = new Color(0xFF383838);
        public static readonly Color icon1 = new Color(0xFFFFFFFF);
        public static readonly Color icon2 = new Color(0xFFA4A4A4);
        public static readonly Color text1 = new Color(0xFFFFFFFF);
        public static readonly Color text2 = new Color(0xFFD8D8D8);
        public static readonly Color text3 = new Color(0xFF959595);
        public static readonly Color dividingLine1 = new Color(0xFF666666);
        public static readonly Color dividingLine2 = new Color(0xFF404040);

        public static readonly Color transparent = new Color(0x00000000);
        public static readonly Color white = new Color(0xFFFFFFFF);
        public static readonly Color black = new Color(0xFF000000);
        public static readonly Color red = new Color(0xFFFF0000);
        public static readonly Color green = new Color(0xFF00FF00);
        public static readonly Color blue = new Color(0xFF0000FF);
    }
}