using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;

namespace UIWidgetsGallery.gallery {
    public class AnimationSectionsUtils {
        public static readonly Color _mariner = new Color(0xFF3B5F8F);
        public static readonly Color _mediumPurple = new Color(0xFF8266D4);
        public static readonly Color _tomato = new Color(0xFFF95B57);
        public static readonly Color _mySin = new Color(0xFFF3A646);
        const string _kGalleryAssetsPackage = "flutter_gallery_assets";

        public static readonly SectionDetail _eyeglassesDetail = new SectionDetail(
            imageAsset: "products/sunnies",
            imageAssetPackage: _kGalleryAssetsPackage,
            title: "Flutter enables interactive animation",
            subtitle: "3K views - 5 days"
        );

        public static readonly SectionDetail _eyeglassesImageDetail = new SectionDetail(
            imageAsset: "products/sunnies",
            imageAssetPackage: _kGalleryAssetsPackage
        );

        public static readonly SectionDetail _seatingDetail = new SectionDetail(
            imageAsset: "products/table",
            imageAssetPackage: _kGalleryAssetsPackage,
            title: "Flutter enables interactive animation",
            subtitle: "3K views - 5 days"
        );

        public static readonly SectionDetail _seatingImageDetail = new SectionDetail(
            imageAsset: "products/table",
            imageAssetPackage: _kGalleryAssetsPackage
        );

        public static readonly SectionDetail _decorationDetail = new SectionDetail(
            imageAsset: "products/earrings",
            imageAssetPackage: _kGalleryAssetsPackage,
            title: "Flutter enables interactive animation",
            subtitle: "3K views - 5 days"
        );

        public static readonly SectionDetail _decorationImageDetail = new SectionDetail(
            imageAsset: "products/earrings",
            imageAssetPackage: _kGalleryAssetsPackage
        );

        public static readonly SectionDetail _protectionDetail = new SectionDetail(
            imageAsset: "products/hat",
            imageAssetPackage: _kGalleryAssetsPackage,
            title: "Flutter enables interactive animation",
            subtitle: "3K views - 5 days"
        );

        public static readonly SectionDetail _protectionImageDetail = new SectionDetail(
            imageAsset: "products/hat",
            imageAssetPackage: _kGalleryAssetsPackage
        );

        public static List<Section> allSections = new List<Section> {
            new Section(
                title: "SUNGLASSES",
                leftColor: _mediumPurple,
                rightColor: _mariner,
                backgroundAsset: "products/sunnies",
                backgroundAssetPackage: _kGalleryAssetsPackage,
                details: new List<SectionDetail> {
                    _eyeglassesDetail,
                    _eyeglassesImageDetail,
                    _eyeglassesDetail,
                    _eyeglassesDetail,
                    _eyeglassesDetail,
                    _eyeglassesDetail
                }
            ),
            new Section(
                title: "FURNITURE",
                leftColor: _tomato,
                rightColor: _mediumPurple,
                backgroundAsset: "products/table",
                backgroundAssetPackage: _kGalleryAssetsPackage,
                details: new List<SectionDetail> {
                    _seatingDetail,
                    _seatingImageDetail,
                    _seatingDetail,
                    _seatingDetail,
                    _seatingDetail,
                    _seatingDetail
                }
            ),
            new Section(
                title: "JEWELRY",
                leftColor: _mySin,
                rightColor: _tomato,
                backgroundAsset: "products/earrings",
                backgroundAssetPackage: _kGalleryAssetsPackage,
                details: new List<SectionDetail> {
                    _decorationDetail,
                    _decorationImageDetail,
                    _decorationDetail,
                    _decorationDetail,
                    _decorationDetail,
                    _decorationDetail
                }
            ),
            new Section(
                title: "HEADWEAR",
                leftColor: Colors.white,
                rightColor: _tomato,
                backgroundAsset: "products/hat",
                backgroundAssetPackage: _kGalleryAssetsPackage,
                details: new List<SectionDetail> {
                    _protectionDetail,
                    _protectionImageDetail,
                    _protectionDetail,
                    _protectionDetail,
                    _protectionDetail,
                    _protectionDetail
                }
            )
        };
    }


    public class SectionDetail {
        public SectionDetail(
            string title = null,
            string subtitle = null,
            string imageAsset = null,
            string imageAssetPackage = null
        ) {
            this.title = title;
            this.subtitle = subtitle;
            this.imageAsset = imageAsset;
            this.imageAssetPackage = imageAssetPackage;
        }

        public readonly string title;
        public readonly string subtitle;
        public readonly string imageAsset;
        public readonly string imageAssetPackage;
    }

    public class Section {
        public Section(
            string title,
            string backgroundAsset,
            string backgroundAssetPackage,
            Color leftColor,
            Color rightColor,
            List<SectionDetail> details
        ) {
            this.title = title;
            this.backgroundAsset = backgroundAsset;
            this.backgroundAssetPackage = backgroundAssetPackage;
            this.leftColor = leftColor;
            this.rightColor = rightColor;
            this.details = details;
        }

        public readonly string title;
        public readonly string backgroundAsset;
        public readonly string backgroundAssetPackage;
        public readonly Color leftColor;
        public readonly Color rightColor;
        public readonly List<SectionDetail> details;

        public static bool operator ==(Section left, Section right) {
            return Equals(left, right);
        }

        public static bool operator !=(Section left, Section right) {
            return !Equals(left, right);
        }

        public bool Equals(Section other) {
            return this.title == other.title;
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

            return this.Equals((Section) obj);
        }

        public override int GetHashCode() {
            return this.title.GetHashCode();
        }
    }
}
