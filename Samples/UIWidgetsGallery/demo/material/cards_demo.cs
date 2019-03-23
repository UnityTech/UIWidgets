using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.gallery {
    class CardsDemoConstants {
        public static readonly List<TravelDestination> destinations = new List<TravelDestination> {
            new TravelDestination(
                assetName: "india_thanjavur_market",
                title: "Top 10 Cities to Visit in Tamil Nadu",
                description: new List<string> {
                    "Number 10",
                    "Thanjavur",
                    "Thanjavur, Tamil Nadu"
                }
            ),
            new TravelDestination(
                assetName: "india_chettinad_silk_maker",
                title: "Artisans of Southern India",
                description: new List<string> {
                    "Silk Spinners",
                    "Chettinad",
                    "Sivaganga, Tamil Nadu"
                }
            )
        };
    }

    public class TravelDestination {
        public TravelDestination(
            string assetName = null,
            string title = null,
            List<string> description = null
        ) {
            this.assetName = assetName;
            this.title = title;
            this.description = description;
        }

        public readonly string assetName;
        public readonly string title;
        public readonly List<string> description;

        public bool isValid {
            get { return this.assetName != null && this.title != null && this.description?.Count == 3; }
        }
    }


    public class TravelDestinationItem : StatelessWidget {
        public TravelDestinationItem(Key key = null, TravelDestination destination = null, ShapeBorder shape = null)
            : base(key: key) {
            D.assert(destination != null && destination.isValid);
            this.destination = destination;
            this.shape = shape;
        }

        public const float height = 366.0f;
        public readonly TravelDestination destination;
        public readonly ShapeBorder shape;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            TextStyle titleStyle = theme.textTheme.headline.copyWith(color: Colors.white);
            TextStyle descriptionStyle = theme.textTheme.subhead;

            return new SafeArea(
                top: false,
                bottom: false,
                child: new Container(
                    padding: EdgeInsets.all(8.0f),
                    height: height,
                    child: new Card(
                        shape: this.shape,
                        child: new Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: new List<Widget> {
                                new SizedBox(
                                    height: 184.0f,
                                    child: new Stack(
                                        children: new List<Widget> {
                                            Positioned.fill(
                                                child: Image.asset(this.destination.assetName,
                                                    fit: BoxFit.cover
                                                )
                                            ),
                                            new Positioned(
                                                bottom: 16.0f,
                                                left: 16.0f,
                                                right: 16.0f,
                                                child: new FittedBox(
                                                    fit: BoxFit.scaleDown,
                                                    alignment: Alignment.centerLeft,
                                                    child: new Text(this.destination.title,
                                                        style: titleStyle
                                                    )
                                                )
                                            )
                                        }
                                    )
                                ),
                                new Expanded(
                                    child: new Padding(
                                        padding: EdgeInsets.fromLTRB(16.0f, 16.0f, 16.0f, 0.0f),
                                        child: new DefaultTextStyle(
                                            softWrap: false,
                                            overflow: TextOverflow.ellipsis,
                                            style: descriptionStyle,
                                            child: new Column(
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                children: new List<Widget> {
                                                    new Padding(
                                                        padding: EdgeInsets.only(bottom: 8.0f),
                                                        child: new Text(this.destination.description[0],
                                                            style: descriptionStyle.copyWith(color: Colors.black54)
                                                        )
                                                    ),
                                                    new Text(this.destination.description[1]),
                                                    new Text(this.destination.description[2])
                                                }
                                            )
                                        )
                                    )
                                ),
                                ButtonTheme.bar(
                                    child: new ButtonBar(
                                        alignment: MainAxisAlignment.start,
                                        children: new List<Widget> {
                                            new FlatButton(
                                                child: new Text("SHARE"),
                                                textColor: Colors.amber.shade500,
                                                onPressed: () => {
                                                    /* do nothing */
                                                }
                                            ),
                                            new FlatButton(
                                                child: new Text("EXPLORE"),
                                                textColor: Colors.amber.shade500,
                                                onPressed: () => {
                                                    /* do nothing */
                                                }
                                            )
                                        }
                                    )
                                ),
                            }
                        )
                    )
                )
            );
        }
    }


    public class CardsDemo : StatefulWidget {
        public const string routeName = "/material/cards";

        public override State createState() {
            return new _CardsDemoState();
        }
    }

    class _CardsDemoState : State<CardsDemo> {
        ShapeBorder _shape;

        public override Widget build(BuildContext context) {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Travel stream"),
                    actions: new List<Widget> {
                        new MaterialDemoDocumentationButton(CardsDemo.routeName),
                        new IconButton(
                            icon: new Icon(
                                Icons.sentiment_very_satisfied
                            ),
                            onPressed: () => {
                                this.setState(() => {
                                    this._shape = this._shape != null
                                        ? null
                                        : new RoundedRectangleBorder(
                                            borderRadius: BorderRadius.only(
                                                topLeft: Radius.circular(16.0f),
                                                topRight: Radius.circular(16.0f),
                                                bottomLeft: Radius.circular(2.0f),
                                                bottomRight: Radius.circular(2.0f)
                                            )
                                        );
                                });
                            }
                        )
                    }
                ),
                body: new ListView(
                    itemExtent: TravelDestinationItem.height,
                    padding: EdgeInsets.only(top: 8.0f, left: 8.0f, right: 8.0f),
                    children: CardsDemoConstants.destinations.Select<TravelDestination, Widget>(
                        (TravelDestination destination) => {
                            return new Container(
                                margin: EdgeInsets.only(bottom: 8.0f),
                                child: new TravelDestinationItem(
                                    destination: destination,
                                    shape: this._shape
                                )
                            );
                        }).ToList()
                )
            );
        }
    }
}