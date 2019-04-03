using System.Collections.Generic;
using System.Linq;
using com.unity.uiwidgets.Runtime.rendering;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Constants = Unity.UIWidgets.material.Constants;
using Image = Unity.UIWidgets.widgets.Image;

namespace UIWidgetsGallery.gallery {
    class ShrineHomeUtils {
        public const float unitSize = Constants.kToolbarHeight;

        public static readonly List<Product> _products = new List<Product>(ShrineData.allProducts());
        public static readonly Dictionary<Product, Order> _shoppingCart = new Dictionary<Product, Order>();

        public const int _childrenPerBlock = 8;
        public const int _rowsPerBlock = 5;

        public static int _minIndexInRow(int rowIndex) {
            int blockIndex = rowIndex / _rowsPerBlock;
            return new List<int> {0, 2, 4, 6, 7}[rowIndex % _rowsPerBlock] + blockIndex * _childrenPerBlock;
        }

        public static int _maxIndexInRow(int rowIndex) {
            int blockIndex = rowIndex / _rowsPerBlock;
            return new List<int> {1, 3, 5, 6, 7}[rowIndex % _rowsPerBlock] + blockIndex * _childrenPerBlock;
        }

        public static int _rowAtIndex(int index) {
            int blockCount = index / _childrenPerBlock;
            return new List<int> {0, 0, 1, 1, 2, 2, 3, 4}[index - blockCount * _childrenPerBlock] +
                   blockCount * _rowsPerBlock;
        }

        public static int _columnAtIndex(int index) {
            return new List<int> {0, 1, 0, 1, 0, 1, 0, 0}[index % _childrenPerBlock];
        }

        public static int _columnSpanAtIndex(int index) {
            return new List<int> {1, 1, 1, 1, 1, 1, 2, 2}[index % _childrenPerBlock];
        }
    }

    class _ShrineGridLayout : SliverGridLayout {
        public _ShrineGridLayout(
            float? rowStride = null,
            float? columnStride = null,
            float? tileHeight = null,
            float? tileWidth = null
        ) {
            this.rowStride = rowStride;
            this.columnStride = columnStride;
            this.tileHeight = tileHeight;
            this.tileWidth = tileWidth;
        }

        public readonly float? rowStride;
        public readonly float? columnStride;
        public readonly float? tileHeight;
        public readonly float? tileWidth;

        public override int getMinChildIndexForScrollOffset(float scrollOffset) {
            return ShrineHomeUtils._minIndexInRow((int) (scrollOffset / this.rowStride));
        }

        public override int getMaxChildIndexForScrollOffset(float scrollOffset) {
            return ShrineHomeUtils._maxIndexInRow((int) (scrollOffset / this.rowStride));
        }

        public override SliverGridGeometry getGeometryForChildIndex(int index) {
            int row = ShrineHomeUtils._rowAtIndex(index);
            int column = ShrineHomeUtils._columnAtIndex(index);
            int columnSpan = ShrineHomeUtils._columnSpanAtIndex(index);
            return new SliverGridGeometry(
                scrollOffset: row * this.rowStride,
                crossAxisOffset: column * this.columnStride,
                mainAxisExtent: this.tileHeight,
                crossAxisExtent: this.tileWidth + (columnSpan - 1) * this.columnStride
            );
        }

        public override float computeMaxScrollOffset(int childCount) {
            if (childCount == 0) {
                return 0.0f;
            }

            int rowCount = ShrineHomeUtils._rowAtIndex(childCount - 1) + 1;
            float? rowSpacing = this.rowStride - this.tileHeight;
            return (this.rowStride * rowCount - rowSpacing) ?? 0.0f;
        }
    }

    class _ShrineGridDelegate : SliverGridDelegate {
        const float _spacing = 8.0f;

        public override SliverGridLayout getLayout(SliverConstraints constraints) {
            float tileWidth = (constraints.crossAxisExtent - _spacing) / 2.0f;
            const float tileHeight = 40.0f + 144.0f + 40.0f;
            return new _ShrineGridLayout(
                tileWidth: tileWidth,
                tileHeight: tileHeight,
                rowStride: tileHeight + _spacing,
                columnStride: tileWidth + _spacing
            );
        }

        public override bool shouldRelayout(SliverGridDelegate oldDelegate) {
            return false;
        }
    }

    class _VendorItem : StatelessWidget {
        public _VendorItem(Key key = null, Vendor vendor = null) : base(key: key) {
            D.assert(vendor != null);
            this.vendor = vendor;
        }

        public readonly Vendor vendor;

        public override Widget build(BuildContext context) {
            return new SizedBox(
                height: 24.0f,
                child: new Row(
                    children: new List<Widget> {
                        new SizedBox(
                            width: 24.0f,
                            child: new ClipRRect(
                                borderRadius: BorderRadius.circular(12.0f),
                                child: Image.asset(
                                    this.vendor.avatarAsset,
                                    fit: BoxFit.cover
                                )
                            )
                        ),
                        new SizedBox(width: 8.0f),
                        new Expanded(
                            child: new Text(this.vendor.name, style: ShrineTheme.of(context).vendorItemStyle)
                        )
                    }
                )
            );
        }
    }

    abstract class _PriceItem : StatelessWidget {
        public _PriceItem(Key key = null, Product product = null)
            : base(key: key) {
            D.assert(product != null);
            this.product = product;
        }

        public readonly Product product;

        public Widget buildItem(BuildContext context, TextStyle style, EdgeInsets padding) {
            BoxDecoration decoration = null;
            if (ShrineHomeUtils._shoppingCart.getOrDefault(this.product) != null) {
                decoration = new BoxDecoration(color: ShrineTheme.of(context).priceHighlightColor);
            }

            return new Container(
                padding: padding,
                decoration: decoration,
                child: new Text(this.product.priceString, style: style)
            );
        }
    }

    class _ProductPriceItem : _PriceItem {
        public _ProductPriceItem(Key key = null, Product product = null) : base(key: key, product: product) {
        }

        public override Widget build(BuildContext context) {
            return this.buildItem(
                context,
                ShrineTheme.of(context).priceStyle,
                EdgeInsets.symmetric(horizontal: 16.0f, vertical: 8.0f)
            );
        }
    }

    class _FeaturePriceItem : _PriceItem {
        public _FeaturePriceItem(Key key = null, Product product = null) : base(key: key, product: product) {
        }

        public override Widget build(BuildContext context) {
            return this.buildItem(
                context,
                ShrineTheme.of(context).featurePriceStyle,
                EdgeInsets.symmetric(horizontal: 24.0f, vertical: 16.0f)
            );
        }
    }

    class _HeadingLayout : MultiChildLayoutDelegate {
        public _HeadingLayout() {
        }

        public const string price = "price";
        public const string image = "image";
        public const string title = "title";
        public const string description = "description";
        public const string vendor = "vendor";

        public override void performLayout(Size size) {
            Size priceSize = this.layoutChild(price, BoxConstraints.loose(size));
            this.positionChild(price, new Offset(size.width - priceSize.width, 0.0f));

            float halfWidth = size.width / 2.0f;
            float halfHeight = size.height / 2.0f;
            const float halfUnit = ShrineHomeUtils.unitSize / 2.0f;
            const float margin = 16.0f;

            Size imageSize = this.layoutChild(image, BoxConstraints.loose(size));
            float imageX = imageSize.width < halfWidth - halfUnit
                ? halfWidth / 2.0f - imageSize.width / 2.0f - halfUnit
                : halfWidth - imageSize.width;
            this.positionChild(image, new Offset(imageX, halfHeight - imageSize.height / 2.0f));

            float maxTitleWidth = halfWidth + ShrineHomeUtils.unitSize - margin;
            BoxConstraints titleBoxConstraints = new BoxConstraints(maxWidth: maxTitleWidth);
            Size titleSize = this.layoutChild(title, titleBoxConstraints);
            float titleX = halfWidth - ShrineHomeUtils.unitSize;
            float titleY = halfHeight - titleSize.height;
            this.positionChild(title, new Offset(titleX, titleY));

            Size descriptionSize = this.layoutChild(description, titleBoxConstraints);
            float descriptionY = titleY + titleSize.height + margin;
            this.positionChild(description, new Offset(titleX, descriptionY));

            this.layoutChild(vendor, titleBoxConstraints);
            float vendorY = descriptionY + descriptionSize.height + margin;
            this.positionChild(vendor, new Offset(titleX, vendorY));
        }

        public override bool shouldRelayout(MultiChildLayoutDelegate oldDelegate) {
            return false;
        }
    }

    class _HeadingShrineHome : StatelessWidget {
        public _HeadingShrineHome(Key key = null, Product product = null) : base(key: key) {
            D.assert(product != null);
            D.assert(product.featureTitle != null);
            D.assert(product.featureDescription != null);
            this.product = product;
        }

        public readonly Product product;

        public override Widget build(BuildContext context) {
            Size screenSize = MediaQuery.of(context).size;
            ShrineTheme theme = ShrineTheme.of(context);
            return new SizedBox(
                height: screenSize.width > screenSize.height
                    ? (screenSize.height - Constants.kToolbarHeight) * 0.85f
                    : (screenSize.height - Constants.kToolbarHeight) * 0.70f,
                child: new Container(
                    decoration: new BoxDecoration(
                        color: theme.cardBackgroundColor,
                        border: new Border(bottom: new BorderSide(color: theme.dividerColor))
                    ),
                    child: new CustomMultiChildLayout(
                        layoutDelegate: new _HeadingLayout(),
                        children: new List<Widget> {
                            new LayoutId(
                                id: _HeadingLayout.price,
                                child: new _FeaturePriceItem(product: this.product)
                            ),
                            new LayoutId(
                                id: _HeadingLayout.image,
                                child: Image.asset(
                                    this.product.imageAsset,
                                    fit: BoxFit.cover
                                )
                            ),
                            new LayoutId(
                                id: _HeadingLayout.title,
                                child: new Text(this.product.featureTitle, style: theme.featureTitleStyle)
                            ),
                            new LayoutId(
                                id: _HeadingLayout.description,
                                child: new Text(this.product.featureDescription, style: theme.featureStyle)
                            ),
                            new LayoutId(
                                id: _HeadingLayout.vendor,
                                child: new _VendorItem(vendor: this.product.vendor)
                            )
                        }
                    )
                )
            );
        }
    }

    class _ProductItem : StatelessWidget {
        public _ProductItem(Key key = null, Product product = null, VoidCallback onPressed = null) : base(key: key) {
            D.assert(product != null);
            this.product = product;
            this.onPressed = onPressed;
        }

        public readonly Product product;
        public readonly VoidCallback onPressed;

        public override Widget build(BuildContext context) {
            return new Card(
                child: new Stack(
                    children: new List<Widget> {
                        new Column(
                            children: new List<Widget> {
                                new Align(
                                    alignment: Alignment.centerRight,
                                    child: new _ProductPriceItem(product: this.product)
                                ),
                                new Container(
                                    width: 144.0f,
                                    height: 144.0f,
                                    padding: EdgeInsets.symmetric(horizontal: 8.0f),
                                    child:new Hero(
                                        tag: this.product.tag,
                                        child: Image.asset(this.product.imageAsset,
                                            fit: BoxFit.contain
                                        )
                                    )
                                ),
                                new Padding(
                                    padding: EdgeInsets.symmetric(horizontal: 8.0f),
                                    child: new _VendorItem(vendor: this.product.vendor)
                                )
                            }
                        ),
                        new Material(
                            type: MaterialType.transparency,
                            child: new InkWell(onTap: this.onPressed == null
                                ? (GestureTapCallback) null
                                : () => { this.onPressed(); })
                        )
                    }
                )
            );
        }
    }

    public class ShrineHome : StatefulWidget {
        public override State createState() {
            return new _ShrineHomeState();
        }
    }

    class _ShrineHomeState : State<ShrineHome> {
        public _ShrineHomeState() {
        }

        readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key(debugLabel: "Shrine Home");
        static readonly _ShrineGridDelegate gridDelegate = new _ShrineGridDelegate();

        void _showOrderPage(Product product) {
            Order order = ShrineHomeUtils._shoppingCart.getOrDefault(product) ?? new Order(product: product);
            Navigator.push(this.context, new ShrineOrderRoute(
                order: order,
                builder: (BuildContext context) => {
                    return new OrderPage(
                        order: order,
                        products: ShrineHomeUtils._products,
                        shoppingCart: ShrineHomeUtils._shoppingCart
                    );
                }
            )).Then(completedOrder => {
                D.assert((completedOrder as Order).product != null);
                if ((completedOrder as Order).quantity == 0) {
                    ShrineHomeUtils._shoppingCart.Remove((completedOrder as Order).product);
                }
            });
        }

        public override Widget build(BuildContext context) {
            Product featured = ShrineHomeUtils._products.First((Product product) => product.featureDescription != null);
            return new ShrinePage(
                scaffoldKey: this._scaffoldKey,
                products: ShrineHomeUtils._products,
                shoppingCart: ShrineHomeUtils._shoppingCart,
                body: new CustomScrollView(
                    slivers: new List<Widget> {
                        new SliverToBoxAdapter(child: new _HeadingShrineHome(product: featured)),
                        new SliverSafeArea(
                            top: false,
                            minimum: EdgeInsets.all(16.0f),
                            sliver: new SliverGrid(
                                gridDelegate: gridDelegate,
                                layoutDelegate: new SliverChildListDelegate(
                                    ShrineHomeUtils._products.Select<Product, Widget>((Product product) => {
                                        return new _ProductItem(
                                            product: product,
                                            onPressed: () => { this._showOrderPage(product); }
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
}