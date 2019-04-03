using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;

namespace UIWidgetsGallery.gallery {
    class ShrineData {
        const string _kGalleryAssetsPackage = "flutter_gallery_assets";

        static Vendor _ali = new Vendor(
            name: "Ali’s shop",
            avatarAsset: "people/square/ali",
            avatarAssetPackage: _kGalleryAssetsPackage,
            description:
            "Ali Connor’s makes custom goods for folks of all shapes and sizes " +
            "made by hand and sometimes by machine, but always with love and care. " +
            "Custom orders are available upon request if you need something extra special."
        );

        static Vendor _peter = new Vendor(
            name: "Peter’s shop",
            avatarAsset: "people/square/peter",
            avatarAssetPackage: _kGalleryAssetsPackage,
            description:
            "Peter makes great stuff for awesome people like you. Super cool and extra " +
            "awesome all of his shop’s goods are handmade with love. Custom orders are " +
            "available upon request if you need something extra special."
        );

        static Vendor _sandra = new Vendor(
            name: "Sandra’s shop",
            avatarAsset: "people/square/sandra",
            avatarAssetPackage: _kGalleryAssetsPackage,
            description:
            "Sandra specializes in furniture, beauty and travel products with a classic vibe. " +
            "Custom orders are available if you’re looking for a certain color or material."
        );

        static Vendor _stella = new Vendor(
            name: "Stella’s shop",
            avatarAsset: "people/square/stella",
            avatarAssetPackage: _kGalleryAssetsPackage,
            description:
            "Stella sells awesome stuff at lovely prices. made by hand and sometimes by " +
            "machine, but always with love and care. Custom orders are available upon request " +
            "if you need something extra special."
        );

        static Vendor _trevor = new Vendor(
            name: "Trevor’s shop",
            avatarAsset: "people/square/trevor",
            avatarAssetPackage: _kGalleryAssetsPackage,
            description:
            "Trevor makes great stuff for awesome people like you. Super cool and extra " +
            "awesome all of his shop’s goods are handmade with love. Custom orders are " +
            "available upon request if you need something extra special."
        );

        static readonly List<Product> _allProducts = new List<Product> {
            new Product(
                name: "Vintage Brown Belt",
                imageAsset: "products/belt",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"fashion", "latest"},
                price: 300.00f,
                vendor: _sandra,
                description:
                "Isn’t it cool when things look old, but they're not. Looks Old But Not makes " +
                "awesome vintage goods that are base smart. This ol’ belt just got an upgrade. "
            ),
            new Product(
                name: "Sunglasses",
                imageAsset: "products/sunnies",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"travel", "fashion", "beauty"},
                price: 20.00f,
                vendor: _trevor,
                description:
                "Be an optimist. Carry Sunglasses with you at all times. All Tints and " +
                "Shades products come with polarized lenses and base duper UV protection " +
                "so you can look at the sun for however long you want. Sunglasses make you " +
                "look cool, wear them."
            ),
            new Product(
                name: "Flatwear",
                imageAsset: "products/flatwear",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"furniture"},
                price: 30.00f,
                vendor: _trevor,
                description:
                "Leave the tunnel and the rain is fallin amazing things happen when you wait"
            ),
            new Product(
                name: "Salmon Sweater",
                imageAsset: "products/sweater",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"fashion"},
                price: 300.00f,
                vendor: _stella,
                description:
                "Looks can be deceiving. This sweater comes in a wide variety of " +
                "flavors, including salmon, that pop as soon as they hit your eyes. " +
                "Sweaters heat quickly, so savor the warmth."
            ),
            new Product(
                name: "Pine Table",
                imageAsset: "products/table",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"furniture"},
                price: 63.00f,
                vendor: _stella,
                description:
                "Leave the tunnel and the rain is fallin amazing things happen when you wait"
            ),
            new Product(
                name: "Green Comfort Jacket",
                imageAsset: "products/jacket",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"fashion"},
                price: 36.00f,
                vendor: _ali,
                description:
                "Leave the tunnel and the rain is fallin amazing things happen when you wait"
            ),
            new Product(
                name: "Chambray Top",
                imageAsset: "products/top",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"fashion"},
                price: 125.00f,
                vendor: _peter,
                description:
                "Leave the tunnel and the rain is fallin amazing things happen when you wait"
            ),
            new Product(
                name: "Blue Cup",
                imageAsset: "products/cup",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"travel", "furniture"},
                price: 75.00f,
                vendor: _sandra,
                description:
                "Drinksy has been making extraordinary mugs for decades. With each " +
                "cup purchased Drinksy donates a cup to those in need. Buy yourself a mug, " +
                "buy someone else a mug."
            ),
            new Product(
                name: "Tea Set",
                imageAsset: "products/teaset",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"furniture", "fashion"},
                price: 70.00f,
                vendor: _trevor,
                featureTitle: "Beautiful glass teapot",
                featureDescription:
                "Teapot holds extremely hot liquids and pours them from the spout.",
                description:
                "Impress your guests with Tea Set by Kitchen Stuff. Teapot holds extremely " +
                "hot liquids and pours them from the spout. Use the handle, shown on the right, " +
                "so your fingers don’t get burnt while pouring."
            ),
            new Product(
                name: "Blue linen napkins",
                imageAsset: "products/napkins",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"furniture", "fashion"},
                price: 89.00f,
                vendor: _trevor,
                description:
                "Blue linen napkins were meant to go with friends, so you may want to pick " +
                "up a bunch of these. These things are absorbant."
            ),
            new Product(
                name: "Dipped Earrings",
                imageAsset: "products/earrings",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"fashion", "beauty"},
                price: 25.00f,
                vendor: _stella,
                description:
                "WeDipIt does it again. These hand-dipped 4 inch earrings are perfect for " +
                "the office or the beach. Just be sure you don’t drop it in a bucket of " +
                "red paint, then they won’t look dipped anymore."
            ),
            new Product(
                name: "Perfect Planters",
                imageAsset: "products/planters",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"latest", "furniture"},
                price: 30.00f,
                vendor: _ali,
                description:
                "The Perfect Planter Co makes the best vessels for just about anything you " +
                "can pot. This set of Perfect Planters holds succulents and cuttings perfectly. " +
                "Looks great in any room. Keep out of reach from cats."
            ),
            new Product(
                name: "Cloud-White Dress",
                imageAsset: "products/dress",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"fashion"},
                price: 54.00f,
                vendor: _sandra,
                description:
                "Trying to find the perfect outift to match your mood? Try no longer. " +
                "This Cloud-White Dress has you covered for those nights when you need " +
                "to get out, or even if you’re just headed to work."
            ),
            new Product(
                name: "Backpack",
                imageAsset: "products/backpack",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"travel", "fashion"},
                price: 25.00f,
                vendor: _peter,
                description:
                "This backpack by Bags ‘n’ stuff can hold just about anything: a laptop, " +
                "a pen, a protractor, notebooks, small animals, plugs for your devices, " +
                "sunglasses, gym clothes, shoes, gloves, two kittens, and even lunch!"
            ),
            new Product(
                name: "Charcoal Straw Hat",
                imageAsset: "products/hat",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"travel", "fashion", "latest"},
                price: 25.00f,
                vendor: _ali,
                description:
                "This is the  helmet for those warm summer days on the road. " +
                "Jetset approved, these hats have been rigorously tested. Keep that face " +
                "protected from the sun."
            ),
            new Product(
                name: "Ginger Scarf",
                imageAsset: "products/scarf",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"latest", "fashion"},
                price: 17.00f,
                vendor: _peter,
                description:
                "Leave the tunnel and the rain is fallin amazing things happen when you wait"
            ),
            new Product(
                name: "Blush Sweats",
                imageAsset: "products/sweats",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"travel", "fashion", "latest"},
                price: 25.00f,
                vendor: _stella,
                description:
                "Leave the tunnel and the rain is fallin amazing things happen when you wait"
            ),
            new Product(
                name: "Mint Jumper",
                imageAsset: "products/jumper",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"travel", "fashion", "beauty"},
                price: 25.00f,
                vendor: _peter,
                description:
                "Leave the tunnel and the rain is fallin amazing things happen when you wait"
            ),
            new Product(
                name: "Ochre Shirt",
                imageAsset: "products/shirt",
                imageAssetPackage: _kGalleryAssetsPackage,
                categories: new List<string> {"fashion", "latest"},
                price: 120.00f,
                vendor: _stella,
                description:
                "Leave the tunnel and the rain is fallin amazing things happen when you wait"
            )
        };

        public static List<Product> allProducts() {
            D.assert(_allProducts.All((Product product) => product.isValid()));
            return new List<Product>(_allProducts);
        }
    }
}