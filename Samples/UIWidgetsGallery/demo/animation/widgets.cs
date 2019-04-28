using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;

namespace UIWidgetsGallery.gallery {
    class AnimationWidgetsUtils {
        public const float kSectionIndicatorWidth = 32.0f;
    }

    public class SectionCard : StatelessWidget {
        public SectionCard(
            Key key = null,
            Section section = null
        ) : base(key: key) {
            D.assert(section != null);
            this.section = section;
        }

        public readonly Section section;

        public override Widget build(BuildContext context) {
            return new DecoratedBox(
                decoration: new BoxDecoration(
                    gradient: new LinearGradient(
                        begin: Alignment.centerLeft,
                        end: Alignment.centerRight,
                        colors: new List<Color> {
                            this.section.leftColor, this.section.rightColor
                        }
                    )
                ),
                child: Image.asset(this.section.backgroundAsset,
                    color: Color.fromRGBO(255, 255, 255, 0.075f),
                    colorBlendMode: BlendMode.modulate,
                    fit: BoxFit.cover
                )
            );
        }
    }

    public class SectionTitle : StatelessWidget {
        public SectionTitle(
            Key key = null,
            Section section = null,
            float? scale = null,
            float? opacity = null
        ) : base(key: key) {
            D.assert(section != null);
            D.assert(scale != null);
            D.assert(opacity != null && opacity >= 0.0f && opacity <= 1.0f);
            this.section = section;
            this.scale = scale;
            this.opacity = opacity;
        }

        public readonly Section section;
        public readonly float? scale;
        public readonly float? opacity;

        public static readonly TextStyle sectionTitleStyle = new TextStyle(
            fontFamily: "Raleway",
            inherit: false,
            fontSize: 24.0f,
            fontWeight: FontWeight.w500,
            color: Colors.white,
            textBaseline: TextBaseline.alphabetic
        );

        public static readonly TextStyle sectionTitleShadowStyle = sectionTitleStyle.copyWith(
            color: new Color(0x19000000)
        );

        public override Widget build(BuildContext context) {
            return new IgnorePointer(
                child: new Opacity(
                    opacity: this.opacity ?? 1.0f,
                    child: new Transform(
                        transform: Matrix3.makeScale(this.scale ?? 1.0f),
                        alignment: Alignment.center,
                        child: new Stack(
                            children: new List<Widget> {
                                new Positioned(
                                    top: 4.0f,
                                    child: new Text(this.section.title, style: sectionTitleShadowStyle)
                                ),
                                new Text(this.section.title, style: sectionTitleStyle)
                            }
                        )
                    )
                )
            );
        }
    }

    public class SectionIndicator : StatelessWidget {
        public SectionIndicator(Key key = null, float opacity = 1.0f) : base(key: key) {
            this.opacity = opacity;
        }

        public readonly float opacity;

        public override Widget build(BuildContext context) {
            return new IgnorePointer(
                child: new Container(
                    width: AnimationWidgetsUtils.kSectionIndicatorWidth,
                    height: 3.0f,
                    color: Colors.white.withOpacity(this.opacity)
                )
            );
        }
    }

    public class SectionDetailView : StatelessWidget {
        public SectionDetailView(
            Key key = null,
            SectionDetail detail = null
        ) : base(key: key) {
            D.assert(detail != null && detail.imageAsset != null);
            D.assert((detail.imageAsset ?? detail.title) != null);
            this.detail = detail;
        }

        public readonly SectionDetail detail;

        public override Widget build(BuildContext context) {
            Widget image = new DecoratedBox(
                decoration: new BoxDecoration(
                    borderRadius: BorderRadius.circular(6.0f),
                    image: new DecorationImage(
                        image: new AssetImage(
                            this.detail.imageAsset
                        ),
                        fit: BoxFit.cover,
                        alignment: Alignment.center
                    )
                )
            );

            Widget item;
            if (this.detail.title == null && this.detail.subtitle == null) {
                item = new Container(
                    height: 240.0f,
                    padding: EdgeInsets.all(16.0f),
                    child: new SafeArea(
                        top: false,
                        bottom: false,
                        child: image
                    )
                );
            }
            else {
                item = new ListTile(
                    title: new Text(this.detail.title),
                    subtitle: new Text(this.detail.subtitle),
                    leading: new SizedBox(width: 32.0f, height: 32.0f, child: image)
                );
            }

            return new DecoratedBox(
                decoration: new BoxDecoration(color: Colors.grey.shade200),
                child: item
            );
        }
    }
}