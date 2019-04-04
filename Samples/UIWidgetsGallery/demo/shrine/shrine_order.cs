using System.Collections.Generic;
using System.Linq;
using com.unity.uiwidgets.Runtime.rendering;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;

namespace UIWidgetsGallery.gallery {
    class _ProductItemShrineOrder : StatelessWidget {
        public _ProductItemShrineOrder(
            Key key = null,
            Product product = null,
            int? quantity = null,
            ValueChanged<int> onChanged = null
        ) : base(key: key) {
            D.assert(product != null);
            D.assert(quantity != null);
            D.assert(onChanged != null);
            this.product = product;
            this.quantity = quantity;
            this.onChanged = onChanged;
        }

        public readonly Product product;
        public readonly int? quantity;
        public readonly ValueChanged<int> onChanged;

        public override Widget build(BuildContext context) {
            ShrineTheme theme = ShrineTheme.of(context);
            return new Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: new List<Widget> {
                    new Text(this.product.name, style: theme.featureTitleStyle),
                    new SizedBox(height: 24.0f),
                    new Text(this.product.description, style: theme.featureStyle),
                    new SizedBox(height: 16.0f),
                    new Padding(
                        padding: EdgeInsets.only(top: 8.0f, bottom: 8.0f, right: 88.0f),
                        child: new DropdownButtonHideUnderline(
                            child: new Container(
                                decoration: new BoxDecoration(
                                    border: Border.all(
                                        color: new Color(0xFFD9D9D9)
                                    )
                                ),
                                child: new DropdownButton<string>(
                                    items: new List<int> {0, 1, 2, 3, 4, 5}.Select<int, DropdownMenuItem<string>>(
                                        (int value) => {
                                            return new DropdownMenuItem<string>(
                                                value: $"{value}",
                                                child: new Padding(
                                                    padding: EdgeInsets.only(left: 8.0f),
                                                    child: new Text($"Quantity {value}", style: theme.quantityMenuStyle)
                                                )
                                            );
                                        }).ToList(),
                                    value: $"{this.quantity}",
                                    onChanged: (value) => { this.onChanged(int.Parse(value)); })
                            )
                        )
                    )
                }
            );
        }
    }

    class _VendorItemShrineOrder : StatelessWidget {
        public _VendorItemShrineOrder(Key key = null, Vendor vendor = null)
            : base(key: key) {
            D.assert(vendor != null);
            this.vendor = vendor;
        }

        public readonly Vendor vendor;

        public override Widget build(BuildContext context) {
            ShrineTheme theme = ShrineTheme.of(context);
            return new Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: new List<Widget> {
                    new SizedBox(
                        height: 24.0f,
                        child: new Align(
                            alignment: Alignment.bottomLeft,
                            child: new Text(this.vendor.name, style: theme.vendorTitleStyle)
                        )
                    ),
                    new SizedBox(height: 16.0f),
                    new Text(this.vendor.description, style: theme.vendorStyle)
                }
            );
        }
    }

    class _HeadingLayoutShrineOrder : MultiChildLayoutDelegate {
        public _HeadingLayoutShrineOrder() {
        }

        public const string image = "image";
        public const string icon = "icon";
        public const string product = "product";
        public const string vendor = "vendor";

        public override void performLayout(Size size) {
            const float margin = 56.0f;
            bool landscape = size.width > size.height;
            float imageWidth = (landscape ? size.width / 2.0f : size.width) - margin * 2.0f;
            BoxConstraints imageConstraints = new BoxConstraints(maxHeight: 224.0f, maxWidth: imageWidth);
            Size imageSize = this.layoutChild(image, imageConstraints);
            const float imageY = 0.0f;
            this.positionChild(image, new Offset(margin, imageY));

            float productWidth = landscape ? size.width / 2.0f : size.width - margin;
            BoxConstraints productConstraints = new BoxConstraints(maxWidth: productWidth);
            Size productSize = this.layoutChild(product, productConstraints);
            float productX = landscape ? size.width / 2.0f : margin;
            float productY = landscape ? 0.0f : imageY + imageSize.height + 16.0f;
            this.positionChild(product, new Offset(productX, productY));

            Size iconSize = this.layoutChild(icon, BoxConstraints.loose(size));
            this.positionChild(icon, new Offset(productX - iconSize.width - 16.0f, productY + 8.0f));

            float vendorWidth = landscape ? size.width - margin : productWidth;
            this.layoutChild(vendor, new BoxConstraints(maxWidth: vendorWidth));
            float vendorX = landscape ? margin : productX;
            float vendorY = productY + productSize.height + 16.0f;
            this.positionChild(vendor, new Offset(vendorX, vendorY));
        }

        public override bool shouldRelayout(MultiChildLayoutDelegate oldDelegate) {
            return true;
        }
    }

    class _HeadingShrineOrder : StatelessWidget {
        public _HeadingShrineOrder(
            Key key = null,
            Product product = null,
            int? quantity = null,
            ValueChanged<int> quantityChanged = null
        ) : base(key: key) {
            D.assert(product != null);
            D.assert(quantity != null && quantity >= 0 && quantity <= 5);
            this.product = product;
            this.quantity = quantity;
            this.quantityChanged = quantityChanged;
        }

        public readonly Product product;
        public readonly int? quantity;
        public readonly ValueChanged<int> quantityChanged;

        public override Widget build(BuildContext context) {
            Size screenSize = MediaQuery.of(context).size;
            return new SizedBox(
                height: (screenSize.height - Constants.kToolbarHeight) * 1.35f,
                child: new Material(
                    type: MaterialType.card,
                    elevation: 0.0f,
                    child: new Padding(
                        padding: EdgeInsets.only(left: 16.0f, top: 18.0f, right: 16.0f, bottom: 24.0f),
                        child: new CustomMultiChildLayout(
                            layoutDelegate: new _HeadingLayoutShrineOrder(),
                            children: new List<Widget> {
                                new LayoutId(
                                    id: _HeadingLayoutShrineOrder.image,
                                    child: new Hero(
                                        tag: this.product.tag,
                                        child: Image.asset(this.product.imageAsset,
                                            fit: BoxFit.contain,
                                            alignment: Alignment.center
                                        )
                                    )
                                ),
                                new LayoutId(
                                    id: _HeadingLayoutShrineOrder.icon,
                                    child: new Icon(
                                        Icons.info_outline,
                                        size: 24.0f,
                                        color: new Color(0xFFFFE0E0)
                                    )
                                ),
                                new LayoutId(
                                    id: _HeadingLayoutShrineOrder.product,
                                    child: new _ProductItemShrineOrder(
                                        product: this.product,
                                        quantity: this.quantity,
                                        onChanged: this.quantityChanged
                                    )
                                ),
                                new LayoutId(
                                    id: _HeadingLayoutShrineOrder.vendor,
                                    child: new _VendorItemShrineOrder(vendor: this.product.vendor)
                                )
                            }
                        )
                    )
                )
            );
        }
    }

    public class OrderPage : StatefulWidget {
        public OrderPage(
            Key key = null,
            Order order = null,
            List<Product> products = null,
            Dictionary<Product, Order> shoppingCart = null
        ) : base(key: key) {
            D.assert(order != null);
            D.assert(products != null && products.isNotEmpty());
            D.assert(shoppingCart != null);
            this.order = order;
            this.products = products;
            this.shoppingCart = shoppingCart;
        }

        public readonly Order order;
        public readonly List<Product> products;
        public readonly Dictionary<Product, Order> shoppingCart;

        public override State createState() {
            return new _OrderPageState();
        }
    }

    class _OrderPageState : State<OrderPage> {
        GlobalKey<ScaffoldState> scaffoldKey;

        public override void initState() {
            base.initState();
            this.scaffoldKey = GlobalKey<ScaffoldState>.key(debugLabel: $"Shrine Order {this.widget.order}");
        }

        public Order currentOrder {
            get { return ShrineOrderRoute.of(this.context).order; }
            set { ShrineOrderRoute.of(this.context).order = value; }
        }

        void updateOrder(int? quantity = null, bool? inCart = null) {
            Order newOrder = this.currentOrder.copyWith(quantity: quantity, inCart: inCart);
            if (this.currentOrder != newOrder) {
                this.setState(() => {
                    this.widget.shoppingCart[newOrder.product] = newOrder;
                    this.currentOrder = newOrder;
                });
            }
        }

        void showSnackBarMessage(string message) {
            this.scaffoldKey.currentState.showSnackBar(new SnackBar(content: new Text(message)));
        }

        public override Widget build(BuildContext context) {
            return new ShrinePage(
                scaffoldKey: this.scaffoldKey,
                products: this.widget.products,
                shoppingCart: this.widget.shoppingCart,
                floatingActionButton: new FloatingActionButton(
                    onPressed: () => {
                        this.updateOrder(inCart: true);
                        int n = this.currentOrder.quantity;
                        string item = this.currentOrder.product.name;
                        string message = n == 1 ? $"is one {item} item" : $"are {n} {item} items";
                        this.showSnackBarMessage(
                            $"There {message} in the shopping cart."
                        );
                    },
                    backgroundColor: new Color(0xFF16F0F0),
                    tooltip: "Add to cart",
                    child: new Icon(
                        Icons.add_shopping_cart,
                        color: Colors.black
                    )
                ),
                body: new CustomScrollView(
                    slivers: new List<Widget> {
                        new SliverToBoxAdapter(
                            child: new _HeadingShrineOrder(
                                product: this.widget.order.product,
                                quantity: this.currentOrder.quantity,
                                quantityChanged: (int value) => { this.updateOrder(quantity: value); }
                            )
                        ),
                        new SliverSafeArea(
                            top: false,
                            minimum: EdgeInsets.fromLTRB(8.0f, 32.0f, 8.0f, 8.0f),
                            sliver: new SliverGrid(
                                gridDelegate: new SliverGridDelegateWithMaxCrossAxisExtent(
                                    maxCrossAxisExtent: 248.0f,
                                    mainAxisSpacing: 8.0f,
                                    crossAxisSpacing: 8.0f
                                ),
                                layoutDelegate: new SliverChildListDelegate(
                                    this.widget.products
                                        .FindAll((Product product) => product != this.widget.order.product)
                                        .Select<Product, Widget>((Product product) => {
                                            return new Card(
                                                elevation: 1.0f,
                                                child: Image.asset(
                                                    product.imageAsset,
                                                    fit: BoxFit.contain
                                                )
                                            );
                                        }).ToList()
                                )
                            )
                        )
                    }
                )
            );
        }
    }

    public class ShrineOrderRoute : ShrinePageRoute<Order> {
        public ShrineOrderRoute(
            Order order = null,
            WidgetBuilder builder = null,
            RouteSettings settings = null
        ) : base(builder: builder, settings: settings) {
            D.assert(order != null);
            this.order = order;
        }

        public Order order;

        public override object currentResult {
            get { return this.order; }
        }

        public new static ShrineOrderRoute of(BuildContext context) {
            return (ShrineOrderRoute) ModalRoute.of(context);
        }
    }
}