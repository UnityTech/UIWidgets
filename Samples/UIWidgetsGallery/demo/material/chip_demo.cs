using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class ChipDemoUtils {
        public static readonly List<string> _defaultMaterials = new List<string> {
            "poker",
            "tortilla",
            "fish and",
            "micro",
            "wood"
        };

        public static readonly List<string> _defaultActions = new List<string> {
            "flake",
            "cut",
            "fragment",
            "splinter",
            "nick",
            "fry",
            "solder",
            "cash in",
            "eat"
        };

        public static readonly Dictionary<string, string> _results = new Dictionary<string, string> {
            {"flake", "flaking"},
            {"cut", "cutting"},
            {"fragment", "fragmenting"},
            {"splinter", "splintering"},
            {"nick", "nicking"},
            {"fry", "frying"},
            {"solder", "soldering"},
            {"cash in", "cashing in"},
            {"eat", "eating"}
        };

        public static readonly List<string> _defaultTools = new List<string> {
            "hammer",
            "chisel",
            "fryer",
            "fabricator",
            "customer"
        };

        public static readonly Dictionary<string, string> _avatars = new Dictionary<string, string> {
            {"hammer", "people/square/ali"},
            {"chisel", "people/square/sandra"},
            {"fryer", "people/square/trevor"},
            {"fabricator", "people/square/stella"},
            {"customer", "people/square/peter"}
        };

        public static readonly Dictionary<string, HashSet<string>> _toolActions =
            new Dictionary<string, HashSet<string>> {
                {"hammer", new HashSet<string>(new List<string> {"flake", "fragment", "splinter"})},
                {"chisel", new HashSet<string>(new List<string> {"flake", "nick", "splinter"})},
                {"fryer", new HashSet<string>(new List<string> {"fry"})},
                {"fabricator", new HashSet<string>(new List<string> {"solder"})},
                {"customer", new HashSet<string>(new List<string> {"cash in", "eat"})}
            };

        public static readonly Dictionary<string, HashSet<string>> _materialActions =
            new Dictionary<string, HashSet<string>> {
                {"poker", new HashSet<string>(new List<string> {"cash in"})},
                {"tortilla", new HashSet<string>(new List<string> {"fry", "eat"})},
                {"fish and", new HashSet<string>(new List<string> {"fry", "eat"})},
                {"micro", new HashSet<string>(new List<string> {"solder", "fragment"})},
                {"wood", new HashSet<string>(new List<string> {"flake", "cut", "splinter", "nick"})}
            };
    }


    class _ChipsTile : StatelessWidget {
        public _ChipsTile(
            Key key = null,
            string label = null,
            List<Widget> children = null
        ) : base(key: key) {
            this.label = label;
            this.children = children;
        }

        public readonly string label;
        public readonly List<Widget> children;

        public override Widget build(BuildContext context) {
            List<Widget> cardChildren = new List<Widget> {
                new Container(
                    padding: EdgeInsets.only(top: 16.0f, bottom: 4.0f),
                    alignment: Alignment.center,
                    child: new Text(this.label, textAlign: TextAlign.left)
                )
            };
            if (this.children.isNotEmpty()) {
                cardChildren.Add(new Wrap(
                    children: this.children.Select<Widget, Widget>((Widget chip) => {
                        return new Padding(
                            padding: EdgeInsets.all(2.0f),
                            child: chip
                        );
                    }).ToList()));
            }
            else {
                TextStyle textStyle = Theme.of(context).textTheme.caption.copyWith(fontStyle: FontStyle.italic);
                cardChildren.Add(
                    new Container(
                        alignment: Alignment.center,
                        constraints: new BoxConstraints(minWidth: 48.0f, minHeight: 48.0f),
                        padding: EdgeInsets.all(8.0f),
                        child: new Text("None", style: textStyle)
                    )
                );
            }

            return new Card(
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    children: cardChildren
                )
            );
        }
    }

    public class ChipDemo : StatefulWidget {
        public const string routeName = "/material/chip";

        public override State createState() {
            return new _ChipDemoState();
        }
    }

    class _ChipDemoState : State<ChipDemo> {
        public _ChipDemoState() {
            this._reset();
        }

        HashSet<string> _materials = new HashSet<string>(new List<string> { });
        string _selectedMaterial = "";
        string _selectedAction = "";
        HashSet<string> _tools = new HashSet<string>(new List<string> { });
        HashSet<string> _selectedTools = new HashSet<string>(new List<string> { });
        HashSet<string> _actions = new HashSet<string>(new List<string> { });
        bool _showShapeBorder = false;

        void _reset() {
            this._materials.Clear();
            this._materials.UnionWith(ChipDemoUtils._defaultMaterials);
            this._actions.Clear();
            this._actions.UnionWith(ChipDemoUtils._defaultActions);
            this._tools.Clear();
            this._tools.UnionWith(ChipDemoUtils._defaultTools);
            this._selectedMaterial = "";
            this._selectedAction = "";
            this._selectedTools.Clear();
        }

        void _removeMaterial(string name) {
            this._materials.Remove(name);
            if (this._selectedMaterial == name) {
                this._selectedMaterial = "";
            }
        }

        void _removeTool(string name) {
            this._tools.Remove(name);
            this._selectedTools.Remove(name);
        }

        string _capitalize(string name) {
            D.assert(name != null && name.isNotEmpty());
            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }

        Color _nameToColor(string name) {
            D.assert(name.Length > 1);
            int hash = name.GetHashCode() & 0xffff;
            float hue = (360.0f * hash / (1 << 15)) % 360.0f;
            return HSVColor.fromAHSV(1.0f, hue, 0.4f, 0.90f).toColor();
        }

        AssetImage _nameToAvatar(string name) {
            D.assert(ChipDemoUtils._avatars.ContainsKey(name));
            return new AssetImage(
                ChipDemoUtils._avatars[name]
            );
        }

        string _createResult() {
            if (this._selectedAction.isEmpty()) {
                return "";
            }

            return this._capitalize(ChipDemoUtils._results[this._selectedAction]) + "!";
        }

        public override Widget build(BuildContext context) {
            List<Widget> chips = this._materials.Select<string, Widget>((string name) => {
                return new Chip(
                    key: Key.key(name),
                    backgroundColor: this._nameToColor(name),
                    label: new Text(this._capitalize(name)),
                    onDeleted: () => { this.setState(() => { this._removeMaterial(name); }); }
                );
            }).ToList();

            List<Widget> inputChips = this._tools.Select<string, Widget>((string name) => {
                return new InputChip(
                    key: ValueKey<string>.key(name),
                    avatar: new CircleAvatar(
                        backgroundImage: this._nameToAvatar(name)
                    ),
                    label: new Text(this._capitalize(name)),
                    onDeleted: () => { this.setState(() => { this._removeTool(name); }); });
            }).ToList();

            List<Widget> choiceChips = this._materials.Select<string, Widget>((string name) => {
                return new ChoiceChip(
                    key: ValueKey<string>.key(name),
                    backgroundColor: this._nameToColor(name),
                    label: new Text(this._capitalize(name)),
                    selected: this._selectedMaterial == name,
                    onSelected: (bool value) => {
                        this.setState(() => { this._selectedMaterial = value ? name : ""; });
                    }
                );
            }).ToList();

            List<Widget> filterChips = ChipDemoUtils._defaultTools.Select<string, Widget>((string name) => {
                return new FilterChip(
                    key: ValueKey<string>.key(name),
                    label: new Text(this._capitalize(name)),
                    selected: this._tools.Contains(name) ? this._selectedTools.Contains(name) : false,
                    onSelected: !this._tools.Contains(name)
                        ? (ValueChanged<bool>) null
                        : (bool value) => {
                            this.setState(() => {
                                if (!value) {
                                    this._selectedTools.Remove(name);
                                }
                                else {
                                    this._selectedTools.Add(name);
                                }
                            });
                        }
                );
            }).ToList();

            HashSet<string> allowedActions = new HashSet<string>(new List<string> { });
            if (this._selectedMaterial != null && this._selectedMaterial.isNotEmpty()) {
                foreach (string tool in this._selectedTools) {
                    allowedActions.UnionWith(ChipDemoUtils._toolActions[tool]);
                }

                allowedActions =
                    new HashSet<string>(
                        allowedActions.Intersect(ChipDemoUtils._materialActions[this._selectedMaterial]));
            }

            List<Widget> actionChips = allowedActions.Select<string, Widget>((string name) => {
                return new ActionChip(
                    label: new Text(this._capitalize(name)),
                    onPressed: () => { this.setState(() => { this._selectedAction = name; }); }
                );
            }).ToList();

            ThemeData theme = Theme.of(context);
            List<Widget> tiles = new List<Widget> {
                new SizedBox(height: 8.0f, width: 0.0f),
                new _ChipsTile(label: "Available Materials (Chip)", children: chips),
                new _ChipsTile(label: "Available Tools (InputChip)", children: inputChips),
                new _ChipsTile(label: "Choose a Material (ChoiceChip)", children: choiceChips),
                new _ChipsTile(label: "Choose Tools (FilterChip)", children: filterChips),
                new _ChipsTile(label: "Perform Allowed Action (ActionChip)", children: actionChips),
                new Divider(),
                new Padding(
                    padding: EdgeInsets.all(8.0f),
                    child: new Center(
                        child: new Text(this._createResult(),
                            style: theme.textTheme.title
                        )
                    )
                )
            };

            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Chips"),
                    actions: new List<Widget> {
                        new MaterialDemoDocumentationButton(ChipDemo.routeName),
                        new IconButton(
                            onPressed: () => {
                                this.setState(() => { this._showShapeBorder = !this._showShapeBorder; });
                            },
                            icon: new Icon(Icons.vignette)
                        )
                    }
                ),
                body: new ChipTheme(
                    data: this._showShapeBorder
                        ? theme.chipTheme.copyWith(
                            shape: new BeveledRectangleBorder(
                                side: new BorderSide(width: 0.66f, style: BorderStyle.solid, color: Colors.grey),
                                borderRadius: BorderRadius.circular(10.0f)
                            ))
                        : theme.chipTheme,
                    child: new ListView(children: tiles)
                ),
                floatingActionButton: new FloatingActionButton(
                    onPressed: () => this.setState(this._reset),
                    child: new Icon(Icons.refresh)
                )
            );
        }
    }
}