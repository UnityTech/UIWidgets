using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;

namespace UIWidgetsGallery.gallery {
    class _ContactCategory : StatelessWidget {
        public _ContactCategory(Key key = null, IconData icon = null, List<Widget> children = null) : base(key: key) {
            this.icon = icon;
            this.children = children;
        }

        public readonly IconData icon;
        public readonly List<Widget> children;

        public override Widget build(BuildContext context) {
            ThemeData themeData = Theme.of(context);
            return new Container(
                padding: EdgeInsets.symmetric(vertical: 16.0f),
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(color: themeData.dividerColor))
                ),
                child: new DefaultTextStyle(
                    style: Theme.of(context).textTheme.subhead,
                    child: new SafeArea(
                        top: false,
                        bottom: false,
                        child: new Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: new List<Widget> {
                                new Container(
                                    padding: EdgeInsets.symmetric(vertical: 24.0f),
                                    width: 72.0f,
                                    child: new Icon(this.icon, color: themeData.primaryColor)
                                ),
                                new Expanded(child: new Column(children: this.children))
                            }
                        )
                    )
                )
            );
        }
    }

    class _ContactItem : StatelessWidget {
        public _ContactItem(Key key = null, IconData icon = null, List<string> lines = null, string tooltip = null,
            VoidCallback onPressed = null) : base(key: key) {
            D.assert(lines.Count > 1);
            this.icon = icon;
            this.lines = lines;
            this.tooltip = tooltip;
            this.onPressed = onPressed;
        }

        public readonly IconData icon;
        public readonly List<string> lines;
        public readonly string tooltip;
        public readonly VoidCallback onPressed;

        public override Widget build(BuildContext context) {
            ThemeData themeData = Theme.of(context);
            List<Widget> columnChildren = this.lines.GetRange(0, this.lines.Count - 1)
                .Select<string, Widget>((string line) => new Text(line)).ToList();
            columnChildren.Add(new Text(this.lines.Last(), style: themeData.textTheme.caption));

            List<Widget> rowChildren = new List<Widget> {
                new Expanded(
                    child: new Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: columnChildren
                    )
                )
            };
            if (this.icon != null) {
                rowChildren.Add(new SizedBox(
                    width: 72.0f,
                    child: new IconButton(
                        icon: new Icon(this.icon),
                        color: themeData.primaryColor,
                        onPressed: this.onPressed
                    )
                ));
            }

            return new Padding(
                padding: EdgeInsets.symmetric(vertical: 16.0f),
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: rowChildren
                )
            );
        }
    }

    public class ContactsDemo : StatefulWidget {
        public const string routeName = "/contacts";

        public override State createState() {
            return new ContactsDemoState();
        }
    }

    public enum AppBarBehavior {
        normal,
        pinned,
        floating,
        snapping
    }

    public class ContactsDemoState : State<ContactsDemo> {
        readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();
        const float _appBarHeight = 256.0f;

        AppBarBehavior _appBarBehavior = AppBarBehavior.pinned;

        public override Widget build(BuildContext context) {
            return new Theme(
                data: new ThemeData(
                    brightness: Brightness.light,
                    primarySwatch: Colors.indigo,
                    platform: Theme.of(context).platform
                ),
                child: new Scaffold(
                    key: this._scaffoldKey,
                    body: new CustomScrollView(
                        slivers: new List<Widget> {
                            new SliverAppBar(
                                expandedHeight: _appBarHeight,
                                pinned: this._appBarBehavior == AppBarBehavior.pinned,
                                floating: this._appBarBehavior == AppBarBehavior.floating ||
                                          this._appBarBehavior == AppBarBehavior.snapping,
                                snap: this._appBarBehavior == AppBarBehavior.snapping,
                                actions: new List<Widget> {
                                    new IconButton(
                                        icon: new Icon(Icons.create),
                                        tooltip: "Edit",
                                        onPressed: () => {
                                            this._scaffoldKey.currentState.showSnackBar(new SnackBar(
                                                content: new Text("Editing isn't supported in this screen.")
                                            ));
                                        }
                                    ),
                                    new PopupMenuButton<AppBarBehavior>(
                                        onSelected: (AppBarBehavior value) => {
                                            this.setState(() => { this._appBarBehavior = value; });
                                        },
                                        itemBuilder: (BuildContext _context) => new List<PopupMenuEntry<AppBarBehavior>> {
                                            new PopupMenuItem<AppBarBehavior>(
                                                value: AppBarBehavior.normal,
                                                child: new Text("App bar scrolls away")
                                            ),
                                            new PopupMenuItem<AppBarBehavior>(
                                                value: AppBarBehavior.pinned,
                                                child: new Text("App bar stays put")
                                            ),
                                            new PopupMenuItem<AppBarBehavior>(
                                                value: AppBarBehavior.floating,
                                                child: new Text("App bar floats")
                                            ),
                                            new PopupMenuItem<AppBarBehavior>(
                                                value: AppBarBehavior.snapping,
                                                child: new Text("App bar snaps")
                                            )
                                        }
                                    )
                                },
                                flexibleSpace: new FlexibleSpaceBar(
                                    title: new Text("Ali Connors"),
                                    background: new Stack(
                                        fit: StackFit.expand,
                                        children: new List<Widget> {
                                            Image.asset(
                                                "people/ali_landscape",
                                                fit: BoxFit.cover,
                                                height: _appBarHeight
                                            ),
                                            new DecoratedBox(
                                                decoration: new BoxDecoration(
                                                    gradient: new LinearGradient(
                                                        begin: new Alignment(0.0f, -1.0f),
                                                        end: new Alignment(0.0f, -0.4f),
                                                        colors: new List<Color>
                                                            {new Color(0x60000000), new Color(0x00000000)}
                                                    )
                                                )
                                            )
                                        }
                                    )
                                )
                            ),
                            new SliverList(
                                del: new SliverChildListDelegate(new List<Widget> {
                                    new AnnotatedRegion<SystemUiOverlayStyle>(
                                        value: SystemUiOverlayStyle.dark,
                                        child: new _ContactCategory(
                                            icon: Icons.call,
                                            children: new List<Widget> {
                                                new _ContactItem(
                                                    icon: Icons.message,
                                                    tooltip: "Send message",
                                                    onPressed: () => {
                                                        this._scaffoldKey.currentState.showSnackBar(new SnackBar(
                                                            content: new Text(
                                                                "Pretend that this opened your SMS application.")
                                                        ));
                                                    },
                                                    lines: new List<string> {
                                                        "(650) 555-1234",
                                                        "Mobile"
                                                    }
                                                ),
                                                new _ContactItem(
                                                    icon: Icons.message,
                                                    tooltip: "Send message",
                                                    onPressed: () => {
                                                        this._scaffoldKey.currentState.showSnackBar(new SnackBar(
                                                            content: new Text("A messaging app appears.")
                                                        ));
                                                    },
                                                    lines: new List<string> {
                                                        "(323) 555-6789",
                                                        "Work"
                                                    }
                                                ),
                                                new _ContactItem(
                                                    icon: Icons.message,
                                                    tooltip: "Send message",
                                                    onPressed: () => {
                                                        this._scaffoldKey.currentState.showSnackBar(new SnackBar(
                                                            content: new Text(
                                                                "Imagine if you will, a messaging application.")
                                                        ));
                                                    },
                                                    lines: new List<string> {
                                                        "(650) 555-6789",
                                                        "Home"
                                                    }
                                                )
                                            }
                                        )
                                    ),
                                    new _ContactCategory(
                                        icon: Icons.contact_mail,
                                        children: new List<Widget> {
                                            new _ContactItem(
                                                icon: Icons.email,
                                                tooltip: "Send personal e-mail",
                                                onPressed: () => {
                                                    this._scaffoldKey.currentState.showSnackBar(new SnackBar(
                                                        content: new Text("Here, your e-mail application would open.")
                                                    ));
                                                },
                                                lines: new List<string> {
                                                    "ali_connors@example.com",
                                                    "Personal"
                                                }
                                            ),
                                            new _ContactItem(
                                                icon: Icons.email,
                                                tooltip: "Send work e-mail",
                                                onPressed: () => {
                                                    this._scaffoldKey.currentState.showSnackBar(new SnackBar(
                                                        content: new Text(
                                                            "Summon your favorite e-mail application here.")
                                                    ));
                                                },
                                                lines: new List<string> {
                                                    "aliconnors@example.com",
                                                    "Work"
                                                }
                                            )
                                        }
                                    ),
                                    new _ContactCategory(
                                        icon: Icons.location_on,
                                        children: new List<Widget> {
                                            new _ContactItem(
                                                icon: Icons.map,
                                                tooltip: "Open map",
                                                onPressed: () => {
                                                    this._scaffoldKey.currentState.showSnackBar(new SnackBar(
                                                        content: new Text("This would show a map of San Francisco.")
                                                    ));
                                                },
                                                lines: new List<string> {
                                                    "2000 Main Street",
                                                    "San Francisco, CA",
                                                    "Home"
                                                }
                                            ),
                                            new _ContactItem(
                                                icon: Icons.map,
                                                tooltip: "Open map",
                                                onPressed: () => {
                                                    this._scaffoldKey.currentState.showSnackBar(new SnackBar(
                                                        content: new Text("This would show a map of Mountain View.")
                                                    ));
                                                },
                                                lines: new List<string> {
                                                    "1600 Amphitheater Parkway",
                                                    "Mountain View, CA",
                                                    "Work"
                                                }
                                            ),
                                            new _ContactItem(
                                                icon: Icons.map,
                                                tooltip: "Open map",
                                                onPressed: () => {
                                                    this._scaffoldKey.currentState.showSnackBar(new SnackBar(
                                                        content: new Text(
                                                            "This would also show a map, if this was not a demo.")
                                                    ));
                                                },
                                                lines: new List<string> {
                                                    "126 Severyns Ave",
                                                    "Mountain View, CA",
                                                    "Jet Travel",
                                                }
                                            )
                                        }
                                    ),
                                    new _ContactCategory(
                                        icon: Icons.today,
                                        children: new List<Widget> {
                                            new _ContactItem(
                                                lines: new List<string> {
                                                    "Birthday",
                                                    "January 9th, 1989"
                                                }
                                            ),
                                            new _ContactItem(
                                                lines: new List<string> {
                                                    "Wedding anniversary",
                                                    "June 21st, 2014"
                                                }
                                            ),
                                            new _ContactItem(
                                                lines: new List<string> {
                                                    "First day in office",
                                                    "January 20th, 2015",
                                                }
                                            ),
                                            new _ContactItem(
                                                lines: new List<string> {
                                                    "Last day in office",
                                                    "August 9th, 2018"
                                                }
                                            )
                                        }
                                    )
                                })
                            )
                        }
                    )
                )
            );
        }
    }
}