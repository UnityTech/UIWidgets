using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    public enum ShrineAction {
        sortByPrice,
        sortByProduct,
        emptyCart
    }

    public class ShrinePage : StatefulWidget {
        public ShrinePage(
            Key key = null,
            GlobalKey<ScaffoldState> scaffoldKey = null,
            Widget body = null,
            Widget floatingActionButton = null,
            List<Product> products = null,
            Dictionary<Product, Order> shoppingCart = null
        ) : base(key: key) {
            D.assert(body != null);
            D.assert(scaffoldKey != null);
            this.scaffoldKey = scaffoldKey;
            this.body = body;
            this.floatingActionButton = floatingActionButton;
            this.products = products;
            this.shoppingCart = shoppingCart;
        }

        public readonly GlobalKey<ScaffoldState> scaffoldKey;
        public readonly Widget body;
        public readonly Widget floatingActionButton;
        public readonly List<Product> products;
        public readonly Dictionary<Product, Order> shoppingCart;

        public override State createState() {
            return new ShrinePageState();
        }
    }

    public class ShrinePageState : State<ShrinePage> {
        float _appBarElevation = 0.0f;

        bool _handleScrollNotification(ScrollNotification notification) {
            float elevation = notification.metrics.extentBefore() <= 0.0f ? 0.0f : 1.0f;
            if (elevation != this._appBarElevation) {
                this.setState(() => { this._appBarElevation = elevation; });
            }

            return false;
        }

        void _showShoppingCart() {
            BottomSheetUtils.showModalBottomSheet<object>(context: this.context, builder: (BuildContext context) => {
                if (this.widget.shoppingCart.isEmpty()) {
                    return new Padding(
                        padding: EdgeInsets.all(24.0f),
                        child: new Text("The shopping cart is empty")
                    );
                }

                return new ListView(
                    padding: Constants.kMaterialListPadding,
                    children: this.widget.shoppingCart.Values.Select<Order, Widget>((Order order) => {
                        return new ListTile(
                            title: new Text(order.product.name),
                            leading: new Text($"{order.quantity}"),
                            subtitle: new Text(order.product.vendor.name)
                        );
                    }).ToList()
                );
            });
        }

        void _sortByPrice() {
            this.widget.products.Sort((Product a, Product b) => a.price?.CompareTo(b.price) ?? (b == null ? 0 : -1));
        }

        void _sortByProduct() {
            this.widget.products.Sort((Product a, Product b) => a.name.CompareTo(b.name));
        }

        void _emptyCart() {
            this.widget.shoppingCart.Clear();
            this.widget.scaffoldKey.currentState.showSnackBar(
                new SnackBar(content: new Text("Shopping cart is empty")));
        }

        public override Widget build(BuildContext context) {
            ShrineTheme theme = ShrineTheme.of(context);
            return new Scaffold(
                key: this.widget.scaffoldKey,
                appBar: new AppBar(
                    elevation: this._appBarElevation,
                    backgroundColor: theme.appBarBackgroundColor,
                    iconTheme: Theme.of(context).iconTheme,
                    brightness: Brightness.light,
                    flexibleSpace: new Container(
                        decoration: new BoxDecoration(
                            border: new Border(
                                bottom: new BorderSide(color: theme.dividerColor)
                            )
                        )
                    ),
                    title: new Text("SHRINE", style: ShrineTheme.of(context).appBarTitleStyle),
                    centerTitle: true,
                    actions: new List<Widget> {
                        new IconButton(
                            icon: new Icon(Icons.shopping_cart),
                            tooltip: "Shopping cart",
                            onPressed: this._showShoppingCart
                        ),
                        new PopupMenuButton<ShrineAction>(
                            itemBuilder: (BuildContext _) => new List<PopupMenuEntry<ShrineAction>> {
                                new PopupMenuItem<ShrineAction>(
                                    value: ShrineAction.sortByPrice,
                                    child: new Text("Sort by price")
                                ),
                                new PopupMenuItem<ShrineAction>(
                                    value: ShrineAction.sortByProduct,
                                    child: new Text("Sort by product")
                                ),
                                new PopupMenuItem<ShrineAction>(
                                    value: ShrineAction.emptyCart,
                                    child: new Text("Empty shopping cart")
                                )
                            },
                            onSelected: (ShrineAction action) => {
                                switch (action) {
                                    case ShrineAction.sortByPrice:
                                        this.setState(this._sortByPrice);
                                        break;
                                    case ShrineAction.sortByProduct:
                                        this.setState(this._sortByProduct);
                                        break;
                                    case ShrineAction.emptyCart:
                                        this.setState(this._emptyCart);
                                        break;
                                }
                            }
                        )
                    }
                ),
                floatingActionButton: this.widget.floatingActionButton,
                body: new NotificationListener<ScrollNotification>(
                    onNotification: this._handleScrollNotification,
                    child: this.widget.body
                )
            );
        }
    }
}