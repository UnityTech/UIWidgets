using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class ButtonsDemo : StatefulWidget {
        public const string routeName = "/material/buttons";

        public ButtonsDemo(Key key = null) : base(key: key) {
        }

        public override State createState() {
            return new _ButtonsDemoState();
        }
    }

    class _ButtonsDemoState : State<ButtonsDemo> {
        const string _raisedText =
            "Raised buttons add dimension to mostly flat layouts. They emphasize " +
            "functions on busy or wide spaces.";

        const string _raisedCode = "buttons_raised";

        const string _flatText = "A flat button displays an ink splash on press " +
                                 "but does not lift. Use flat buttons on toolbars, in dialogs and " +
                                 "inline with padding";

        const string _flatCode = "buttons_flat";

        const string _outlineText =
            "Outline buttons become opaque and elevate when pressed. They are often " +
            "paired with raised buttons to indicate an alternative, secondary action.";

        const string _outlineCode = "buttons_outline";

        const string _dropdownText =
            "A dropdown button displays a menu that's used to select a value from a " +
            "small set of values. The button displays the current value and a down " +
            "arrow.";

        const string _dropdownCode = "buttons_dropdown";

        const string _iconText =
            "IconButtons are appropriate for toggle buttons that allow a single choice " +
            "to be selected or deselected, such as adding or removing an item's star.";

        const string _iconCode = "buttons_icon";

        const string _actionText =
            "Floating action buttons are used for a promoted action. They are " +
            "distinguished by a circled icon floating above the UI and can have motion " +
            "behaviors that include morphing, launching, and a transferring anchor " +
            "point.";

        const string _actionCode = "buttons_action";

        ShapeBorder _buttonShape;

        public _ButtonsDemoState() {
        }

        public override Widget build(BuildContext context) {
            ButtonThemeData buttonTheme = ButtonTheme.of(context).copyWith(
                shape: this._buttonShape
            );

            List<ComponentDemoTabData> demos = new List<ComponentDemoTabData> {
                new ComponentDemoTabData(
                    tabName: "RAISED",
                    description: _raisedText,
                    demoWidget: ButtonTheme.fromButtonThemeData(
                        data: buttonTheme,
                        child: this.buildRaisedButton()
                    ),
                    exampleCodeTag: _raisedCode,
                    documentationUrl: "https://docs.flutter.io/flutter/material/RaisedButton-class.html"
                ),
                new ComponentDemoTabData(
                    tabName: "FLAT",
                    description: _flatText,
                    demoWidget: ButtonTheme.fromButtonThemeData(
                        data: buttonTheme,
                        child: this.buildFlatButton()
                    ),
                    exampleCodeTag: _flatCode,
                    documentationUrl: "https://docs.flutter.io/flutter/material/FlatButton-class.html"
                ),
                new ComponentDemoTabData(
                    tabName: "OUTLINE",
                    description: _outlineText,
                    demoWidget: ButtonTheme.fromButtonThemeData(
                        data: buttonTheme,
                        child: this.buildOutlineButton()
                    ),
                    exampleCodeTag: _outlineCode,
                    documentationUrl: "https://docs.flutter.io/flutter/material/OutlineButton-class.html"
                ),
                new ComponentDemoTabData(
                    tabName: "DROPDOWN",
                    description: _dropdownText,
                    demoWidget: this.buildDropdownButton(),
                    exampleCodeTag: _dropdownCode,
                    documentationUrl: "https://docs.flutter.io/flutter/material/DropdownButton-class.html"
                ),
                new ComponentDemoTabData(
                    tabName: "ICON",
                    description: _iconText,
                    demoWidget: this.buildIconButton(),
                    exampleCodeTag: _iconCode,
                    documentationUrl: "https://docs.flutter.io/flutter/material/IconButton-class.html"
                ),
                new ComponentDemoTabData(
                    tabName: "ACTION",
                    description: _actionText,
                    demoWidget: this.buildActionButton(),
                    exampleCodeTag: _actionCode,
                    documentationUrl: "https://docs.flutter.io/flutter/material/FloatingActionButton-class.html"
                )
            };

            return new TabbedComponentDemoScaffold(
                title: "Buttons",
                demos: demos,
                actions: new List<Widget> {
                    new IconButton(
                        icon: new Icon(Icons.sentiment_very_satisfied),
                        onPressed: () => {
                            this.setState(() => {
                                this._buttonShape = this._buttonShape == null ? new StadiumBorder() : null;
                            });
                        }
                    )
                }
            );
        }

        public Widget buildRaisedButton() {
            return new Align(
                alignment: new Alignment(0.0f, -0.2f),
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget> {
                        new ButtonBar(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                new RaisedButton(
                                    child: new Text("RAISED BUTTON"),
                                    onPressed: () => {
                                        // Perform some action
                                    }
                                ),
                                new RaisedButton(
                                    child: new Text("DISABLED"),
                                    onPressed: null
                                )
                            }
                        ),
                        new ButtonBar(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                RaisedButton.icon(
                                    icon: new Icon(Icons.add, size: 18.0f),
                                    label: new Text("RAISED BUTTON"),
                                    onPressed: () => {
                                        // Perform some action
                                    }
                                ),
                                RaisedButton.icon(
                                    icon: new Icon(Icons.add, size: 18.0f),
                                    label: new Text("DISABLED"),
                                    onPressed: null
                                )
                            }
                        )
                    }
                )
            );
        }

        public Widget buildFlatButton() {
            return new Align(
                alignment: new Alignment(0.0f, -0.2f),
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget> {
                        new ButtonBar(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                new FlatButton(
                                    child: new Text("FLAT BUTTON"),
                                    onPressed: () => {
                                        // Perform some action
                                    }
                                ),
                                new FlatButton(
                                    child: new Text("DISABLED"),
                                    onPressed: null
                                )
                            }
                        ),
                        new ButtonBar(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                FlatButton.icon(
                                    icon: new Icon(Icons.add_circle_outline, size: 18.0f),
                                    label: new Text("FLAT BUTTON"),
                                    onPressed: () => {
                                        // Perform some action
                                    }
                                ),
                                FlatButton.icon(
                                    icon: new Icon(Icons.add_circle_outline, size: 18.0f),
                                    label: new Text("DISABLED"),
                                    onPressed: null
                                ),
                            }
                        )
                    }
                )
            );
        }

        Widget buildOutlineButton() {
            return new Align(
                alignment: new Alignment(0.0f, -0.2f),
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget> {
                        new ButtonBar(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                new OutlineButton(
                                    child: new Text("OUTLINE BUTTON"),
                                    onPressed: () => {
                                        // Perform some action
                                    }
                                ),

                                new OutlineButton(
                                    child: new Text("DISABLED"),
                                    onPressed: null
                                )
                            }
                        ),
                        new ButtonBar(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                OutlineButton.icon(
                                    icon: new Icon(Icons.add, size: 18.0f),
                                    label: new Text("OUTLINE BUTTON"),
                                    onPressed: () => {
                                        // Perform some action
                                    }
                                ),
                                OutlineButton.icon(
                                    icon: new Icon(Icons.add, size: 18.0f),
                                    label: new Text("DISABLED"),
                                    onPressed: null
                                )
                            }
                        )
                    }
                )
            );
        }

        // https://en.wikipedia.org/wiki/Free_Four
        string dropdown1Value = "Free";
        string dropdown2Value;
        string dropdown3Value = "Four";

        public Widget buildDropdownButton() {
            return new Padding(
                padding: EdgeInsets.all(24.0f),
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.start,
                    children: new List<Widget> {
                        new ListTile(
                            title: new Text("Simple dropdown:"),
                            trailing: new DropdownButton<string>(
                                value: this.dropdown1Value,
                                onChanged: (string newValue) => {
                                    this.setState(() => { this.dropdown1Value = newValue; });
                                },
                                items: new List<string> {"One", "Two", "Free", "Four"}.Select((string value) => {
                                    return new DropdownMenuItem<string>(
                                        value: value,
                                        child: new Text(value)
                                    );
                                }).ToList()
                            )
                        ),
                        new SizedBox(
                            height: 24.0f
                        ),
                        new ListTile(
                            title: new Text("Dropdown with a hint:"),
                            trailing: new DropdownButton<string>(
                                value: this.dropdown2Value,
                                hint: new Text("Choose"),
                                onChanged: (string newValue) => {
                                    this.setState(() => { this.dropdown2Value = newValue; });
                                },
                                items: new List<string> {"One", "Two", "Free", "Four"}.Select((string value) => {
                                    return new DropdownMenuItem<string>(
                                        value: value,
                                        child: new Text(value)
                                    );
                                }).ToList()
                            )
                        ),
                        new SizedBox(
                            height: 24.0f
                        ),
                        new ListTile(
                            title: new Text("Scrollable dropdown:"),
                            trailing: new DropdownButton<string>(
                                value: this.dropdown3Value,
                                onChanged: (string newValue) => {
                                    this.setState(() => { this.dropdown3Value = newValue; });
                                },
                                items: new List<string> {
                                    "One", "Two", "Free", "Four", "Can", "I", "Have", "A", "Little",
                                    "Bit", "More", "Five", "Six", "Seven", "Eight", "Nine", "Ten"
                                }.Select<string, DropdownMenuItem<string>>(value => {
                                    return new DropdownMenuItem<string>(
                                        value: value,
                                        child: new Text(value)
                                    );
                                }).ToList()
                            )
                        )
                    }
                )
            );
        }

        bool iconButtonToggle = false;

        public Widget buildIconButton() {
            return new Align(
                alignment: new Alignment(0.0f, -0.2f),
                child: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget> {
                        new IconButton(
                            icon: new Icon(
                                Icons.thumb_up
                            ),
                            onPressed: () => { this.setState(() => this.iconButtonToggle = !this.iconButtonToggle); },
                            color: this.iconButtonToggle ? Theme.of(this.context).primaryColor : null
                        ),
                        new IconButton(
                            icon: new Icon(
                                Icons.thumb_up
                            ),
                            onPressed: null
                        )
                    }.Select<Widget, Widget>(
                        (Widget button) => new SizedBox(width: 64.0f, height: 64.0f, child: button)).ToList()
                )
            );
        }

        public Widget buildActionButton() {
            return new Align(
                alignment: new Alignment(0.0f, -0.2f),
                child: new FloatingActionButton(
                    child: new Icon(Icons.add),
                    onPressed: () => {
                        // Perform some action
                    },
                    tooltip: "floating action button"
                )
            );
        }
    }
}