using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace UIWidgetsGallery.gallery {
    class AnimationHomeUtils {
        public static readonly Color _kAppBackgroundColor = new Color(0xFF353662);
        public static readonly TimeSpan _kScrollDuration = new TimeSpan(0, 0, 0, 0, 400);
        public static readonly Curve _kScrollCurve = Curves.fastOutSlowIn;
        public const float _kAppBarMinHeight = 90.0f;
        public const float _kAppBarMidHeight = 256.0f;
    }

    class _RenderStatusBarPaddingSliver : RenderSliver {
        public _RenderStatusBarPaddingSliver(
            float? maxHeight = null,
            float? scrollFactor = null
        ) {
            D.assert(maxHeight >= 0.0f);
            D.assert(scrollFactor >= 1.0f);
            this._maxHeight = maxHeight;
            this._scrollFactor = scrollFactor;
        }

        public float? maxHeight {
            get { return this._maxHeight; }
            set {
                D.assert(this.maxHeight >= 0.0f);
                if (this._maxHeight == value) {
                    return;
                }

                this._maxHeight = value;
                this.markNeedsLayout();
            }
        }

        float? _maxHeight;

        public float? scrollFactor {
            get { return this._scrollFactor; }
            set {
                D.assert(this.scrollFactor >= 1.0f);
                if (this._scrollFactor == value) {
                    return;
                }

                this._scrollFactor = value;
                this.markNeedsLayout();
            }
        }

        float? _scrollFactor;

        protected override void performLayout() {
            float? height =
                (this.maxHeight - this.constraints.scrollOffset / this.scrollFactor)?.clamp(0.0f,
                    this.maxHeight ?? 0.0f);
            this.geometry = new SliverGeometry(
                paintExtent: Mathf.Min(height ?? 0.0f, this.constraints.remainingPaintExtent),
                scrollExtent: this.maxHeight ?? 0.0f,
                maxPaintExtent: this.maxHeight ?? 0.0f
            );
        }
    }

    class _StatusBarPaddingSliver : SingleChildRenderObjectWidget {
        public _StatusBarPaddingSliver(
            Key key = null,
            float? maxHeight = null,
            float scrollFactor = 5.0f
        ) : base(key: key) {
            D.assert(maxHeight != null && maxHeight >= 0.0f);
            D.assert(scrollFactor >= 1.0f);
            this.maxHeight = maxHeight;
            this.scrollFactor = scrollFactor;
        }

        public readonly float? maxHeight;
        public readonly float scrollFactor;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderStatusBarPaddingSliver(
                maxHeight: this.maxHeight,
                scrollFactor: this.scrollFactor
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderStatusBarPaddingSliver renderObject = _renderObject as _RenderStatusBarPaddingSliver;
            renderObject.maxHeight = this.maxHeight;
            renderObject.scrollFactor = this.scrollFactor;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new FloatProperty("maxHeight", this.maxHeight));
            description.add(new FloatProperty("scrollFactor", this.scrollFactor));
        }
    }

    class _SliverAppBarDelegate : SliverPersistentHeaderDelegate {
        public _SliverAppBarDelegate(
            float minHeight,
            float maxHeight,
            Widget child
        ) {
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.child = child;
        }

        public readonly float minHeight;
        public readonly float maxHeight;
        public readonly Widget child;

        public override float? minExtent {
            get { return this.minHeight; }
        }

        public override float? maxExtent {
            get { return Mathf.Max(this.maxHeight, this.minHeight); }
        }

        public override Widget build(BuildContext context, float shrinkOffset, bool overlapsContent) {
            return SizedBox.expand(child: this.child);
        }

        public override bool shouldRebuild(SliverPersistentHeaderDelegate _oldDelegate) {
            _SliverAppBarDelegate oldDelegate = _oldDelegate as _SliverAppBarDelegate;
            return this.maxHeight != oldDelegate.maxHeight
                   || this.minHeight != oldDelegate.minHeight
                   || this.child != oldDelegate.child;
        }

        public override string ToString() {
            return "_SliverAppBarDelegate";
        }
    }

    class _AllSectionsLayout : MultiChildLayoutDelegate {
        public _AllSectionsLayout(
            Alignment translation,
            float tColumnToRow,
            float tCollapsed,
            int cardCount,
            float selectedIndex
        ) {
            this.translation = translation;
            this.tColumnToRow = tColumnToRow;
            this.tCollapsed = tCollapsed;
            this.cardCount = cardCount;
            this.selectedIndex = selectedIndex;
        }

        public readonly Alignment translation;
        public readonly float tColumnToRow;
        public readonly float tCollapsed;
        public readonly int cardCount;
        public readonly float selectedIndex;

        Rect _interpolateRect(Rect begin, Rect end) {
            return Rect.lerp(begin, end, this.tColumnToRow);
        }

        Offset _interpolatePoint(Offset begin, Offset end) {
            return Offset.lerp(begin, end, this.tColumnToRow);
        }

        public override void performLayout(Size size) {
            float columnCardX = size.width / 5.0f;
            float columnCardWidth = size.width - columnCardX;
            float columnCardHeight = size.height / this.cardCount;
            float rowCardWidth = size.width;
            Offset offset = this.translation.alongSize(size);
            float columnCardY = 0.0f;
            float rowCardX = -(this.selectedIndex * rowCardWidth);

            float columnTitleX = size.width / 10.0f;
            float rowTitleWidth = size.width * ((1 + this.tCollapsed) / 2.25f);
            float rowTitleX = (size.width - rowTitleWidth) / 2.0f - this.selectedIndex * rowTitleWidth;

            const float paddedSectionIndicatorWidth = AnimationWidgetsUtils.kSectionIndicatorWidth + 8.0f;
            float rowIndicatorWidth = paddedSectionIndicatorWidth +
                                      (1.0f - this.tCollapsed) * (rowTitleWidth - paddedSectionIndicatorWidth);
            float rowIndicatorX = (size.width - rowIndicatorWidth) / 2.0f - this.selectedIndex * rowIndicatorWidth;

            for (int index = 0; index < this.cardCount; index++) {
                Rect columnCardRect = Rect.fromLTWH(columnCardX, columnCardY, columnCardWidth, columnCardHeight);
                Rect rowCardRect = Rect.fromLTWH(rowCardX, 0.0f, rowCardWidth, size.height);
                Rect cardRect = this._interpolateRect(columnCardRect, rowCardRect).shift(offset);
                string cardId = $"card{index}";
                if (this.hasChild(cardId)) {
                    this.layoutChild(cardId, BoxConstraints.tight(cardRect.size));
                    this.positionChild(cardId, cardRect.topLeft);
                }

                Size titleSize = this.layoutChild($"title{index}", BoxConstraints.loose(cardRect.size));
                float columnTitleY = columnCardRect.centerLeft.dy - titleSize.height / 2.0f;
                float rowTitleY = rowCardRect.centerLeft.dy - titleSize.height / 2.0f;
                float centeredRowTitleX = rowTitleX + (rowTitleWidth - titleSize.width) / 2.0f;
                Offset columnTitleOrigin = new Offset(columnTitleX, columnTitleY);
                Offset rowTitleOrigin = new Offset(centeredRowTitleX, rowTitleY);
                Offset titleOrigin = this._interpolatePoint(columnTitleOrigin, rowTitleOrigin);
                this.positionChild($"title{index}", titleOrigin + offset);

                Size indicatorSize = this.layoutChild($"indicator{index}", BoxConstraints.loose(cardRect.size));
                float columnIndicatorX = cardRect.centerRight.dx - indicatorSize.width - 16.0f;
                float columnIndicatorY = cardRect.bottomRight.dy - indicatorSize.height - 16.0f;
                Offset columnIndicatorOrigin = new Offset(columnIndicatorX, columnIndicatorY);
                Rect titleRect = Rect.fromPoints(titleOrigin, titleSize.bottomRight(titleOrigin));
                float centeredRowIndicatorX = rowIndicatorX + (rowIndicatorWidth - indicatorSize.width) / 2.0f;
                float rowIndicatorY = titleRect.bottomCenter.dy + 16.0f;
                Offset rowIndicatorOrigin = new Offset(centeredRowIndicatorX, rowIndicatorY);
                Offset indicatorOrigin = this._interpolatePoint(columnIndicatorOrigin, rowIndicatorOrigin);
                this.positionChild($"indicator{index}", indicatorOrigin + offset);

                columnCardY += columnCardHeight;
                rowCardX += rowCardWidth;
                rowTitleX += rowTitleWidth;
                rowIndicatorX += rowIndicatorWidth;
            }
        }

        public override bool shouldRelayout(MultiChildLayoutDelegate _oldDelegate) {
            _AllSectionsLayout oldDelegate = _oldDelegate as _AllSectionsLayout;
            return this.tColumnToRow != oldDelegate.tColumnToRow
                   || this.cardCount != oldDelegate.cardCount
                   || this.selectedIndex != oldDelegate.selectedIndex;
        }
    }

    class _AllSectionsView : AnimatedWidget {
        public _AllSectionsView(
            Key key = null,
            int? sectionIndex = null,
            List<Section> sections = null,
            ValueNotifier<float> selectedIndex = null,
            float? minHeight = null,
            float? midHeight = null,
            float? maxHeight = null,
            List<Widget> sectionCards = null
        ) : base(key: key, listenable: selectedIndex) {
            sectionCards = sectionCards ?? new List<Widget>();
            D.assert(sections != null);
            D.assert(sectionCards.Count == sections.Count);
            D.assert(sectionIndex >= 0 && sectionIndex < sections.Count);
            D.assert(selectedIndex != null);
            D.assert(selectedIndex.value >= 0.0f && (float) selectedIndex.value < sections.Count);
            this.sectionIndex = sectionIndex;
            this.sections = sections;
            this.selectedIndex = selectedIndex;
            this.minHeight = minHeight;
            this.midHeight = midHeight;
            this.maxHeight = maxHeight;
            this.sectionCards = sectionCards;
        }

        public readonly int? sectionIndex;
        public readonly List<Section> sections;
        public readonly ValueNotifier<float> selectedIndex;
        public readonly float? minHeight;
        public readonly float? midHeight;
        public readonly float? maxHeight;
        public readonly List<Widget> sectionCards;

        float _selectedIndexDelta(int index) {
            return (index - this.selectedIndex.value).abs().clamp(0.0f, 1.0f);
        }

        Widget _build(BuildContext context, BoxConstraints constraints) {
            Size size = constraints.biggest;

            float? tColumnToRow =
                1.0f - ((size.height - this.midHeight) /
                        (this.maxHeight - this.midHeight))?.clamp(0.0f, 1.0f);


            float? tCollapsed =
                1.0f - ((size.height - this.minHeight) /
                        (this.midHeight - this.minHeight))?.clamp(0.0f, 1.0f);

            float _indicatorOpacity(int index) {
                return 1.0f - this._selectedIndexDelta(index) * 0.5f;
            }

            float? _titleOpacity(int index) {
                return 1.0f - this._selectedIndexDelta(index) * tColumnToRow * 0.5f;
            }

            float? _titleScale(int index) {
                return 1.0f - this._selectedIndexDelta(index) * tColumnToRow * 0.15f;
            }

            List<Widget> children = new List<Widget>(this.sectionCards);

            for (int index = 0; index < this.sections.Count; index++) {
                Section section = this.sections[index];
                children.Add(new LayoutId(
                    id: $"title{index}",
                    child: new SectionTitle(
                        section: section,
                        scale: _titleScale(index),
                        opacity: _titleOpacity(index)
                    )
                ));
            }

            for (int index = 0; index < this.sections.Count; index++) {
                children.Add(new LayoutId(
                    id: $"indicator{index}",
                    child: new SectionIndicator(
                        opacity: _indicatorOpacity(index)
                    )
                ));
            }

            return new CustomMultiChildLayout(
                layoutDelegate: new _AllSectionsLayout(
                    translation: new Alignment((this.selectedIndex.value - this.sectionIndex) * 2.0f - 1.0f ?? 0.0f,
                        -1.0f),
                    tColumnToRow: tColumnToRow ?? 0.0f,
                    tCollapsed: tCollapsed ?? 0.0f,
                    cardCount: this.sections.Count,
                    selectedIndex: this.selectedIndex.value
                ),
                children: children
            );
        }

        protected override Widget build(BuildContext context) {
            return new LayoutBuilder(builder: this._build);
        }
    }

    class _SnappingScrollPhysics : ClampingScrollPhysics {
        public _SnappingScrollPhysics(
            ScrollPhysics parent = null,
            float? midScrollOffset = null
        ) : base(parent: parent) {
            D.assert(midScrollOffset != null);
            this.midScrollOffset = midScrollOffset ?? 0.0f;
        }

        public readonly float midScrollOffset;

        public override ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new _SnappingScrollPhysics(parent: this.buildParent(ancestor),
                midScrollOffset: this.midScrollOffset);
        }

        Simulation _toMidScrollOffsetSimulation(float offset, float dragVelocity) {
            float velocity = Mathf.Max(dragVelocity, this.minFlingVelocity);
            return new ScrollSpringSimulation(this.spring, offset, this.midScrollOffset, velocity,
                tolerance: this.tolerance);
        }

        Simulation _toZeroScrollOffsetSimulation(float offset, float dragVelocity) {
            float velocity = Mathf.Max(dragVelocity, this.minFlingVelocity);
            return new ScrollSpringSimulation(this.spring, offset, 0.0f, velocity, tolerance: this.tolerance);
        }

        public override Simulation createBallisticSimulation(ScrollMetrics position, float dragVelocity) {
            Simulation simulation = base.createBallisticSimulation(position, dragVelocity);
            float offset = position.pixels;

            if (simulation != null) {
                float simulationEnd = simulation.x(float.PositiveInfinity);
                if (simulationEnd >= this.midScrollOffset) {
                    return simulation;
                }

                if (dragVelocity > 0.0f) {
                    return this._toMidScrollOffsetSimulation(offset, dragVelocity);
                }

                if (dragVelocity < 0.0f) {
                    return this._toZeroScrollOffsetSimulation(offset, dragVelocity);
                }
            }
            else {
                float snapThreshold = this.midScrollOffset / 2.0f;
                if (offset >= snapThreshold && offset < this.midScrollOffset) {
                    return this._toMidScrollOffsetSimulation(offset, dragVelocity);
                }

                if (offset > 0.0f && offset < snapThreshold) {
                    return this._toZeroScrollOffsetSimulation(offset, dragVelocity);
                }
            }

            return simulation;
        }
    }

    public class AnimationDemoHome : StatefulWidget {
        public AnimationDemoHome(Key key = null) : base(key: key) {
        }

        public const string routeName = "/animation";

        public override State createState() {
            return new _AnimationDemoHomeState();
        }
    }

    class _AnimationDemoHomeState : State<AnimationDemoHome> {
        ScrollController _scrollController = new ScrollController();
        PageController _headingPageController = new PageController();
        PageController _detailsPageController = new PageController();
        ScrollPhysics _headingScrollPhysics = new NeverScrollableScrollPhysics();
        ValueNotifier<float> selectedIndex = new ValueNotifier<float>(0.0f);

        public override Widget build(BuildContext context) {
            return new Scaffold(
                backgroundColor: AnimationHomeUtils._kAppBackgroundColor,
                body: new Builder(
                    builder: this._buildBody
                )
            );
        }

        void _handleBackButton(float midScrollOffset) {
            if (this._scrollController.offset >= midScrollOffset) {
                this._scrollController.animateTo(0.0f, curve: AnimationHomeUtils._kScrollCurve,
                    duration: AnimationHomeUtils._kScrollDuration);
            }
            else {
                Navigator.maybePop(this.context);
            }
        }

        bool _handleScrollNotification(ScrollNotification notification, float midScrollOffset) {
            if (notification.depth == 0 && notification is ScrollUpdateNotification) {
                ScrollPhysics physics = this._scrollController.position.pixels >= midScrollOffset
                    ? (ScrollPhysics) new PageScrollPhysics()
                    : new NeverScrollableScrollPhysics();
                if (physics != this._headingScrollPhysics) {
                    this.setState(() => { this._headingScrollPhysics = physics; });
                }
            }

            return false;
        }

        void _maybeScroll(float midScrollOffset, int pageIndex, float xOffset) {
            if (this._scrollController.offset < midScrollOffset) {
                this._headingPageController.animateToPage(pageIndex, curve: AnimationHomeUtils._kScrollCurve,
                    duration: AnimationHomeUtils._kScrollDuration);
                this._scrollController.animateTo(midScrollOffset, curve: AnimationHomeUtils._kScrollCurve,
                    duration: AnimationHomeUtils._kScrollDuration);
            }
            else {
                float centerX = this._headingPageController.position.viewportDimension / 2.0f;
                int newPageIndex = xOffset > centerX ? pageIndex + 1 : pageIndex - 1;
                this._headingPageController.animateToPage(newPageIndex, curve: AnimationHomeUtils._kScrollCurve,
                    duration: AnimationHomeUtils._kScrollDuration);
            }
        }

        bool _handlePageNotification(ScrollNotification notification, PageController leader, PageController follower) {
            if (notification.depth == 0 && notification is ScrollUpdateNotification) {
                this.selectedIndex.value = leader.page;
                if (follower.page != leader.page) {
                    follower.position.jumpTo(leader.position.pixels); // ignore: deprecated_member_use
                }
            }

            return false;
        }

        IEnumerable<Widget> _detailItemsFor(Section section) {
            IEnumerable<Widget> detailItems = section.details.Select<SectionDetail, Widget>((SectionDetail detail) => {
                return new SectionDetailView(detail: detail);
            });
            return ListTile.divideTiles(context: this.context, tiles: detailItems);
        }

        List<Widget> _allHeadingItems(float maxHeight, float midScrollOffset) {
            List<Widget> sectionCards = new List<Widget> { };
            for (int index = 0; index < AnimationSectionsUtils.allSections.Count; index++) {
                sectionCards.Add(new LayoutId(
                    id: $"card{index}",
                    child: new GestureDetector(
                        behavior: HitTestBehavior.opaque,
                        child: new SectionCard(section: AnimationSectionsUtils.allSections[index]),
                        onTapUp: (TapUpDetails details) => {
                            float xOffset = details.globalPosition.dx;
                            this.setState(() => { this._maybeScroll(midScrollOffset, index, xOffset); });
                        }
                    )
                ));
            }

            List<Widget> headings = new List<Widget> { };
            for (int index = 0; index < AnimationSectionsUtils.allSections.Count; index++) {
                headings.Add(new Container(
                        color: AnimationHomeUtils._kAppBackgroundColor,
                        child: new ClipRect(
                            child: new _AllSectionsView(
                                sectionIndex: index,
                                sections: AnimationSectionsUtils.allSections,
                                selectedIndex: this.selectedIndex,
                                minHeight: AnimationHomeUtils._kAppBarMinHeight,
                                midHeight: AnimationHomeUtils._kAppBarMidHeight,
                                maxHeight: maxHeight,
                                sectionCards: sectionCards
                            )
                        )
                    )
                );
            }

            return headings;
        }

        Widget _buildBody(BuildContext context) {
            MediaQueryData mediaQueryData = MediaQuery.of(context);
            float statusBarHeight = mediaQueryData.padding.top;
            float screenHeight = mediaQueryData.size.height;
            float appBarMaxHeight = screenHeight - statusBarHeight;

            float appBarMidScrollOffset = statusBarHeight + appBarMaxHeight - AnimationHomeUtils._kAppBarMidHeight;

            return SizedBox.expand(
                child: new Stack(
                    children: new List<Widget> {
                        new NotificationListener<ScrollNotification>(
                            onNotification: (ScrollNotification notification) => {
                                return this._handleScrollNotification(notification, appBarMidScrollOffset);
                            },
                            child: new CustomScrollView(
                                controller: this._scrollController,
                                physics: new _SnappingScrollPhysics(midScrollOffset: appBarMidScrollOffset),
                                slivers: new List<Widget> {
                                    new _StatusBarPaddingSliver(
                                        maxHeight: statusBarHeight,
                                        scrollFactor: 7.0f
                                    ),
                                    new SliverPersistentHeader(
                                        pinned: true,
                                        del: new _SliverAppBarDelegate(
                                            minHeight: AnimationHomeUtils._kAppBarMinHeight,
                                            maxHeight: appBarMaxHeight,
                                            child: new NotificationListener<ScrollNotification>(
                                                onNotification: (ScrollNotification notification) => {
                                                    return this._handlePageNotification(notification,
                                                        this._headingPageController, this._detailsPageController);
                                                },
                                                child: new PageView(
                                                    physics: this._headingScrollPhysics,
                                                    controller: this._headingPageController,
                                                    children: this._allHeadingItems(appBarMaxHeight,
                                                        appBarMidScrollOffset)
                                                )
                                            )
                                        )
                                    ),
                                    new SliverToBoxAdapter(
                                        child: new SizedBox(
                                            height: 610.0f,
                                            child: new NotificationListener<ScrollNotification>(
                                                onNotification: (ScrollNotification notification) => {
                                                    return this._handlePageNotification(notification,
                                                        this._detailsPageController, this._headingPageController);
                                                },
                                                child: new PageView(
                                                    controller: this._detailsPageController,
                                                    children: AnimationSectionsUtils.allSections
                                                        .Select<Section, Widget>((Section section) => {
                                                            return new Column(
                                                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                                                children: this._detailItemsFor(section).ToList()
                                                            );
                                                        }).ToList()
                                                )
                                            )
                                        )
                                    )
                                }
                            )
                        ),
                        new Positioned(
                            top: statusBarHeight,
                            left: 0.0f,
                            child: new IconTheme(
                                data: new IconThemeData(color: Colors.white),
                                child: new SafeArea(
                                    top: false,
                                    bottom: false,
                                    child: new IconButton(
                                        icon: new BackButtonIcon(),
                                        tooltip: "Back",
                                        onPressed: () => { this._handleBackButton(appBarMidScrollOffset); }
                                    )
                                )
                            )
                        )
                    }
                )
            );
        }
    }
}