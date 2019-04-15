using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace UIWidgetsGallery.gallery {
    public class Vendor {
        public Vendor(
            string name,
            string description,
            string avatarAsset,
            string avatarAssetPackage
        ) {
            this.name = name;
            this.description = description;
            this.avatarAsset = avatarAsset;
            this.avatarAssetPackage = avatarAssetPackage;
        }

        public readonly string name;
        public readonly string description;
        public readonly string avatarAsset;
        public readonly string avatarAssetPackage;

        public bool isValid() {
            return this.name != null && this.description != null && this.avatarAsset != null;
        }

        public override string ToString() {
            return $"Vendor({this.name})";
        }
    }

    public class Product {
        public Product(
            string name = null,
            string description = null,
            string featureTitle = null,
            string featureDescription = null,
            string imageAsset = null,
            string imageAssetPackage = null,
            List<string> categories = null,
            float? price = null,
            Vendor vendor = null
        ) {
            this.name = name;
            this.description = description;
            this.featureTitle = featureTitle;
            this.featureDescription = featureDescription;
            this.imageAsset = imageAsset;
            this.imageAssetPackage = imageAssetPackage;
            this.categories = categories;
            this.price = price;
            this.vendor = vendor;
        }

        public readonly string name;
        public readonly string description;
        public readonly string featureTitle;
        public readonly string featureDescription;
        public readonly string imageAsset;
        public readonly string imageAssetPackage;
        public readonly List<string> categories;
        public readonly float? price;
        public readonly Vendor vendor;

        public string tag {
            get { return this.name; }
        }

        public string priceString {
            get { return $"${(this.price ?? 0.0f).floor()}"; }
        }

        public bool isValid() {
            return this.name != null && this.description != null && this.imageAsset != null &&
                   this.categories != null && this.categories.isNotEmpty() && this.price != null &&
                   this.vendor.isValid();
        }

        public override string ToString() {
            return $"Product({this.name})";
        }
    }

    public class Order {
        public Order(Product product, int quantity = 1, bool inCart = false) {
            D.assert(product != null);
            D.assert(quantity >= 0);
            this.product = product;
            this.quantity = quantity;
            this.inCart = inCart;
        }

        public readonly Product product;
        public readonly int quantity;
        public readonly bool inCart;

        public Order copyWith(Product product = null, int? quantity = null, bool? inCart = null) {
            return new Order(
                product: product ?? this.product,
                quantity: quantity ?? this.quantity,
                inCart: inCart ?? this.inCart
            );
        }

        public static bool operator ==(Order left, Order right) {
            if (left is null && right is null) {
                return true;
            }

            if (left is null || right is null) {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Order left, Order right) {
            if (left is null && right is null) {
                return false;
            }

            if (left is null || right is null) {
                return true;
            }

            return !left.Equals(right);
        }

        public bool Equals(Order other) {
            return this.product == other.product &&
                   this.quantity == other.quantity &&
                   this.inCart == other.inCart;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((Order) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.product != null ? this.product.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.quantity.GetHashCode();
                hashCode = (hashCode * 397) ^ this.inCart.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            return $"Order({this.product}, quantity={this.quantity}, inCart={this.inCart})";
        }
    }
}