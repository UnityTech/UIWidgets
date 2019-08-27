using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    public class GalleryDemoCategory : IEquatable<GalleryDemoCategory> {
        public GalleryDemoCategory(string name = null, IconData icon = null) {
            D.assert(name != null);
            D.assert(icon != null);

            this.name = name;
            this.icon = icon;
        }

        public readonly string name;

        public readonly IconData icon;

        public bool Equals(GalleryDemoCategory other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(this.name, other.name) && Equals(this.icon, other.icon);
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

            return this.Equals((GalleryDemoCategory) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.name != null ? this.name.GetHashCode() : 0) * 397) ^
                       (this.icon != null ? this.icon.GetHashCode() : 0);
            }
        }

        public static bool operator ==(GalleryDemoCategory left, GalleryDemoCategory right) {
            return Equals(left, right);
        }

        public static bool operator !=(GalleryDemoCategory left, GalleryDemoCategory right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.name})";
        }
    }

    public static partial class DemoUtils {
        internal static readonly GalleryDemoCategory _kDemos = new GalleryDemoCategory(
            name: "Studies",
            icon: GalleryIcons.animation
        );

        internal static readonly GalleryDemoCategory _kStyle = new GalleryDemoCategory(
            name: "Style",
            icon: GalleryIcons.custom_typography
        );

        internal static readonly GalleryDemoCategory _kMaterialComponents = new GalleryDemoCategory(
            name: "Material",
            icon: GalleryIcons.category_mdc
        );

        internal static readonly GalleryDemoCategory _kCupertinoComponents = new GalleryDemoCategory(
            name: "Cupertino",
            icon: GalleryIcons.phone_iphone
        );

        internal static readonly GalleryDemoCategory _kMedia = new GalleryDemoCategory(
            name: "Media",
            icon: GalleryIcons.drive_video
        );
    }

    public class GalleryDemo {
        public GalleryDemo(
            string title = null,
            IconData icon = null,
            string subtitle = null,
            GalleryDemoCategory category = null,
            string routeName = null,
            string documentationUrl = null,
            WidgetBuilder buildRoute = null
        ) {
            D.assert(title != null);
            D.assert(icon != null);
            D.assert(category != null);
            D.assert(routeName != null);
            D.assert(buildRoute != null);
            this.title = title;
            this.icon = icon;
            this.subtitle = subtitle;
            this.category = category;
            this.routeName = routeName;
            this.documentationUrl = documentationUrl;
            this.buildRoute = buildRoute;
        }

        public readonly string title;
        public readonly IconData icon;
        public readonly string subtitle;
        public readonly GalleryDemoCategory category;
        public readonly string routeName;
        public readonly string documentationUrl;
        public readonly WidgetBuilder buildRoute;

        public override string ToString() {
            return $"{this.GetType()}({this.title} {this.routeName})";
        }
    }

    public static partial class DemoUtils {
        static List<GalleryDemo> _buildGalleryDemos() {
            List<GalleryDemo> galleryDemos = new List<GalleryDemo> {
                // Demos
                new GalleryDemo(
                    title: "Shrine",
                    subtitle: "Basic shopping app",
                    icon: GalleryIcons.shrine,
                    category: _kDemos,
                    routeName: ShrineDemo.routeName,
                    buildRoute: (BuildContext context) => new ShrineDemo()
                ),
                new GalleryDemo(
                    title: "Contact profile",
                    subtitle: "Address book entry with a flexible appbar",
                    icon: GalleryIcons.account_box,
                    category: _kDemos,
                    routeName: ContactsDemo.routeName,
                    buildRoute: (BuildContext context) => new ContactsDemo()
                ),
                new GalleryDemo(
                    title: "Animation",
                    subtitle: "Section organizer",
                    icon: GalleryIcons.animation,
                    category: _kDemos,
                    routeName: AnimationDemo.routeName,
                    buildRoute: (BuildContext context) => new AnimationDemo()
                ),

                // Style
                new GalleryDemo(
                    title: "Colors",
                    subtitle: "All of the predefined colors",
                    icon: GalleryIcons.colors,
                    category: _kStyle,
                    routeName: ColorsDemo.routeName,
                    buildRoute: (BuildContext context) => new ColorsDemo()
                ),
                new GalleryDemo(
                    title: "Typography",
                    subtitle: "All of the predefined text styles",
                    icon: GalleryIcons.custom_typography,
                    category: _kStyle,
                    routeName: TypographyDemo.routeName,
                    buildRoute: (BuildContext context) => new TypographyDemo()
                ),

                // Material Components
                new GalleryDemo(
                    title: "Backdrop",
                    subtitle: "Select a front layer from back layer",
                    icon: GalleryIcons.backdrop,
                    category: _kMaterialComponents,
                    routeName: BackdropDemo.routeName,
                    buildRoute: (BuildContext context) => new BackdropDemo()
                ),
                new GalleryDemo(
                    title: "Bottom app bar",
                    subtitle: "Optional floating action button notch",
                    icon: GalleryIcons.bottom_app_bar,
                    category: _kMaterialComponents,
                    routeName: BottomAppBarDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/BottomAppBar-class.html",
                    buildRoute: (BuildContext context) => new BottomAppBarDemo()
                ),
                new GalleryDemo(
                    title: "Bottom navigation",
                    subtitle: "Bottom navigation with cross-fading views",
                    icon: GalleryIcons.bottom_navigation,
                    category: _kMaterialComponents,
                    routeName: BottomNavigationDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/BottomNavigationBar-class.html",
                    buildRoute: (BuildContext context) => new BottomNavigationDemo()
                ),
                new GalleryDemo(
                    title: "Bottom sheet: Modal",
                    subtitle: "A dismissable bottom sheet",
                    icon: GalleryIcons.bottom_sheets,
                    category: _kMaterialComponents,
                    routeName: ModalBottomSheetDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/showModalBottomSheet.html",
                    buildRoute: (BuildContext context) => new ModalBottomSheetDemo()
                ),
                new GalleryDemo(
                    title: "Bottom sheet: Persistent",
                    subtitle: "A bottom sheet that sticks around",
                    icon: GalleryIcons.bottom_sheet_persistent,
                    category: _kMaterialComponents,
                    routeName: PersistentBottomSheetDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/ScaffoldState/showBottomSheet.html",
                    buildRoute: (BuildContext context) => new PersistentBottomSheetDemo()
                ),
                new GalleryDemo(
                    title: "Buttons",
                    subtitle: "Flat, raised, dropdown, and more",
                    icon: GalleryIcons.generic_buttons,
                    category: _kMaterialComponents,
                    routeName: ButtonsDemo.routeName,
                    buildRoute: (BuildContext context) => new ButtonsDemo()
                ),
                new GalleryDemo(
                    title: "Buttons: Floating Action Button",
                    subtitle: "FAB with transitions",
                    icon: GalleryIcons.buttons,
                    category: _kMaterialComponents,
                    routeName: TabsFabDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/FloatingActionButton-class.html",
                    buildRoute: (BuildContext context) => new TabsFabDemo()
                ),
                new GalleryDemo(
                    title: "Cards",
                    subtitle: "Baseline cards with rounded corners",
                    icon: GalleryIcons.cards,
                    category: _kMaterialComponents,
                    routeName: CardsDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/Card-class.html",
                    buildRoute: (BuildContext context) => new CardsDemo()
                ),
                new GalleryDemo(
                    title: "Chips",
                    subtitle: "Labeled with delete buttons and avatars",
                    icon: GalleryIcons.chips,
                    category: _kMaterialComponents,
                    routeName: ChipDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/material/Chip-class.html",
                    buildRoute: (BuildContext context) => new ChipDemo()
                ),
                // new GalleryDemo(
                //     title: "Data tables",
                //     subtitle: "Rows and columns",
                //     icon: GalleryIcons.data_table,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: DataTableDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/PaginatedDataTable-class.html",
                //     buildRoute: (BuildContext context) => DataTableDemo()
                // ),
                // new GalleryDemo(
                //     title: "Dialogs",
                //     subtitle: "Simple, alert, and fullscreen",
                //     icon: GalleryIcons.dialogs,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: DialogDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/showDialog.html",
                //     buildRoute: (BuildContext context) => DialogDemo()
                // ),
                // new GalleryDemo(
                //     title: "Elevations",
                //     subtitle: "Shadow values on cards",
                //     // TODO(larche): Change to custom icon for elevations when one exists.
                //     icon: GalleryIcons.cupertino_progress,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: ElevationDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/Material/elevation.html",
                //     buildRoute: (BuildContext context) => ElevationDemo()
                // ),
                // new GalleryDemo(
                //     title: "Expand/collapse list control",
                //     subtitle: "A list with one sub-list level",
                //     icon: GalleryIcons.expand_all,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: TwoLevelListDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/ExpansionTile-class.html",
                //     buildRoute: (BuildContext context) => TwoLevelListDemo()
                // ),
                // new GalleryDemo(
                //     title: "Expansion panels",
                //     subtitle: "List of expanding panels",
                //     icon: GalleryIcons.expand_all,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: ExpansionPanelsDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/ExpansionPanel-class.html",
                //     buildRoute: (BuildContext context) => ExpansionPanelsDemo()
                // ),
                // new GalleryDemo(
                //     title: "Grid",
                //     subtitle: "Row and column layout",
                //     icon: GalleryIcons.grid_on,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: GridListDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/widgets/GridView-class.html",
                //     buildRoute: (BuildContext context) => new GridListDemo()
                // ),
                // new GalleryDemo(
                //     title: "Icons",
                //     subtitle: "Enabled and disabled icons with opacity",
                //     icon: GalleryIcons.sentiment_very_satisfied,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: IconsDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/IconButton-class.html",
                //     buildRoute: (BuildContext context) => IconsDemo()
                // ),
                // new GalleryDemo(
                //     title: "Lists",
                //     subtitle: "Scrolling list layouts",
                //     icon: GalleryIcons.list_alt,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: ListDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/ListTile-class.html",
                //     buildRoute: (BuildContext context) => new ListDemo()
                // ),
                // new GalleryDemo(
                //     title: "Lists: leave-behind list items",
                //     subtitle: "List items with hidden actions",
                //     icon: GalleryIcons.lists_leave_behind,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: LeaveBehindDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/widgets/Dismissible-class.html",
                //     buildRoute: (BuildContext context) => new LeaveBehindDemo()
                // ),
                // new GalleryDemo(
                //     title: "Lists: reorderable",
                //     subtitle: "Reorderable lists",
                //     icon: GalleryIcons.list_alt,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: ReorderableListDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/ReorderableListView-class.html",
                //     buildRoute: (BuildContext context) => new ReorderableListDemo()
                // ),
                // new GalleryDemo(
                //     title: "Menus",
                //     subtitle: "Menu buttons and simple menus",
                //     icon: GalleryIcons.more_vert,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: MenuDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/PopupMenuButton-class.html",
                //     buildRoute: (BuildContext context) => new MenuDemo()
                // ),
                // new GalleryDemo(
                //     title: "Navigation drawer",
                //     subtitle: "Navigation drawer with standard header",
                //     icon: GalleryIcons.menu,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: DrawerDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/Drawer-class.html",
                //     buildRoute: (BuildContext context) => DrawerDemo()
                // ),
                // new GalleryDemo(
                //     title: "Pagination",
                //     subtitle: "PageView with indicator",
                //     icon: GalleryIcons.page_control,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: PageSelectorDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/TabBarView-class.html",
                //     buildRoute: (BuildContext context) => PageSelectorDemo()
                // ),
                // new GalleryDemo(
                //     title: "Pickers",
                //     subtitle: "Date and time selection widgets",
                //     icon: GalleryIcons.@event,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: DateAndTimePickerDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/showDatePicker.html",
                //     buildRoute: (BuildContext context) => DateAndTimePickerDemo()
                // ),
                // new GalleryDemo(
                //     title: "Progress indicators",
                //     subtitle: "Linear, circular, indeterminate",
                //     icon: GalleryIcons.progress_activity,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: ProgressIndicatorDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/LinearProgressIndicator-class.html",
                //     buildRoute: (BuildContext context) => ProgressIndicatorDemo()
                // ),
                // new GalleryDemo(
                //     title: "Pull to refresh",
                //     subtitle: "Refresh indicators",
                //     icon: GalleryIcons.refresh,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: OverscrollDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/RefreshIndicator-class.html",
                //     buildRoute: (BuildContext context) => OverscrollDemo()
                // ),
                // new GalleryDemo(
                //     title: "Search",
                //     subtitle: "Expandable search",
                //     icon: Icons.search,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: SearchDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/showSearch.html",
                //     buildRoute: (BuildContext context) => SearchDemo()
                // ),
                // new GalleryDemo(
                //     title: "Selection controls",
                //     subtitle: "Checkboxes, radio buttons, and switches",
                //     icon: GalleryIcons.check_box,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: SelectionControlsDemo.routeName,
                //     buildRoute: (BuildContext context) => SelectionControlsDemo()
                // ),
                // new GalleryDemo(
                //     title: "Sliders",
                //     subtitle: "Widgets for selecting a value by swiping",
                //     icon: GalleryIcons.sliders,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: SliderDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/Slider-class.html",
                //     buildRoute: (BuildContext context) => SliderDemo()
                // ),
                // new GalleryDemo(
                //     title: "Snackbar",
                //     subtitle: "Temporary messaging",
                //     icon: GalleryIcons.snackbar,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: SnackBarDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/ScaffoldState/showSnackBar.html",
                //     buildRoute: (BuildContext context) => SnackBarDemo()
                // ),
                // new GalleryDemo(
                //     title: "Tabs",
                //     subtitle: "Tabs with independently scrollable views",
                //     icon: GalleryIcons.tabs,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: TabsDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/TabBarView-class.html",
                //     buildRoute: (BuildContext context) => TabsDemo()
                // ),
                // new GalleryDemo(
                //     title: "Tabs: Scrolling",
                //     subtitle: "Tab bar that scrolls",
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     icon: GalleryIcons.tabs,
                //     routeName: ScrollableTabsDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/TabBar-class.html",
                //     buildRoute: (BuildContext context) => ScrollableTabsDemo()
                // ),
                // new GalleryDemo(
                //     title: "Text fields",
                //     subtitle: "Single line of editable text and numbers",
                //     icon: GalleryIcons.text_fields_alt,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: TextFormFieldDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/TextFormField-class.html",
                //     buildRoute: (BuildContext context) =>  const TextFormFieldDemo()
                // ),
                // new GalleryDemo(
                //     title: "Tooltips",
                //     subtitle: "Short message displayed on long-press",
                //     icon: GalleryIcons.tooltip,
                //     category: GalleryDemoCategory._kMaterialComponents,
                //     routeName: TooltipDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/material/Tooltip-class.html",
                //     buildRoute: (BuildContext context) => TooltipDemo()
                // ),
                //
                // Cupertino Components
                // new GalleryDemo(
                //     title: "Activity Indicator",
                //     icon: GalleryIcons.cupertino_progress,
                //     category: _kCupertinoComponents,
                //     routeName: CupertinoProgressIndicatorDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/cupertino/CupertinoActivityIndicator-class.html",
                //     buildRoute: (BuildContext context) => CupertinoProgressIndicatorDemo()
                // ),
                new GalleryDemo(
                    title: "Alerts",
                    icon: GalleryIcons.dialogs,
                    category: _kCupertinoComponents,
                    routeName: CupertinoAlertDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/cupertino/showCupertinoDialog.html",
                    buildRoute: (BuildContext context) => new CupertinoAlertDemo()
                ),
                new GalleryDemo(
                    title: "Buttons",
                    icon: GalleryIcons.generic_buttons,
                    category: _kCupertinoComponents,
                    routeName: CupertinoButtonsDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/cupertino/CupertinoButton-class.html",
                    buildRoute: (BuildContext context) => new CupertinoButtonsDemo()
                ),
                // new GalleryDemo(
                //     title: "Navigation",
                //     icon: GalleryIcons.bottom_navigation,
                //     category: _kCupertinoComponents,
                //     routeName: CupertinoNavigationDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/cupertino/CupertinoTabScaffold-class.html",
                //     buildRoute: (BuildContext context) => new CupertinoNavigationDemo()
                // ),
                // new GalleryDemo(
                //     title: "Pickers",
                //     icon: GalleryIcons.@event,
                //     category: GalleryDemoCategory._kCupertinoComponents,
                //     routeName: CupertinoPickerDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/cupertino/CupertinoPicker-class.html",
                //     buildRoute: (BuildContext context) => CupertinoPickerDemo()
                // ),
                // new GalleryDemo(
                //     title: "Pull to refresh",
                //     icon: GalleryIcons.cupertino_pull_to_refresh,
                //     category: _kCupertinoComponents,
                //     routeName: CupertinoRefreshControlDemo.routeName,
                //     documentationUrl:
                //     "https://docs.flutter.io/flutter/cupertino/CupertinoSliverRefreshControl-class.html",
                //     buildRoute: (BuildContext context) => CupertinoRefreshControlDemo()
                // ),
                // new GalleryDemo(
                //     title: "Segmented Control",
                //     icon: GalleryIcons.tabs,
                //     category: GalleryDemoCategory._kCupertinoComponents,
                //     routeName: CupertinoSegmentedControlDemo.routeName,
                //     documentationUrl: "https://docs.flutter.io/flutter/cupertino/CupertinoSegmentedControl-class.html",
                //     buildRoute: (BuildContext context) => CupertinoSegmentedControlDemo()
                // ),
                new GalleryDemo(
                    title: "Sliders",
                    icon: GalleryIcons.sliders,
                    category: _kCupertinoComponents,
                    routeName: CupertinoSliderDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/cupertino/CupertinoSlider-class.html",
                    buildRoute: (BuildContext context) => new CupertinoSliderDemo()
                ),
                new GalleryDemo(
                    title: "Switches",
                    icon: GalleryIcons.cupertino_switch,
                    category: _kCupertinoComponents,
                    routeName: CupertinoSwitchDemo.routeName,
                    documentationUrl: "https://docs.flutter.io/flutter/cupertino/CupertinoSwitch-class.html",
                    buildRoute: (BuildContext context) => new CupertinoSwitchDemo()
                ),
                new GalleryDemo(
                    title: "Text Fields",
                    icon: GalleryIcons.text_fields_alt,
                    category: _kCupertinoComponents,
                    routeName: CupertinoTextFieldDemo.routeName,
                    buildRoute: (BuildContext context) => new CupertinoTextFieldDemo()
                ),
                //
                // // Media
                // new GalleryDemo(
                //     title: "Animated images",
                //     subtitle: "GIF and WebP animations",
                //     icon: GalleryIcons.animation,
                //     category: GalleryDemoCategory._kMedia,
                //     routeName: ImagesDemo.routeName,
                //     buildRoute: (BuildContext context) => ImagesDemo()
                // ),
                // new GalleryDemo(
                //     title: "Video",
                //     subtitle: "Video playback",
                //     icon: GalleryIcons.drive_video,
                //     category: GalleryDemoCategory._kMedia,
                //     routeName: VideoDemo.routeName,
                //     buildRoute: (BuildContext context) => new VideoDemo()
                // ),
            };

            // Keep Pesto around for its regression test value. It is not included
            // in (release builds) the performance tests.
            D.assert(() => {
                // galleryDemos.Insert(0,
                //     new GalleryDemo(
                //         title: "Pesto",
                //         subtitle: "Simple recipe browser",
                //         icon: Icons.adjust,
                //         category: GalleryDemoCategory._kDemos,
                //         routeName: PestoDemo.routeName,
                //         buildRoute: (BuildContext context) => new PestoDemo()
                //     )
                // );
                return true;
            });

            return galleryDemos;
        }

        public static readonly List<GalleryDemo> kAllGalleryDemos = _buildGalleryDemos();

        public static readonly HashSet<GalleryDemoCategory> kAllGalleryDemoCategories =
            new HashSet<GalleryDemoCategory>(kAllGalleryDemos.Select(demo => demo.category));

        public static readonly Dictionary<GalleryDemoCategory, List<GalleryDemo>> kGalleryCategoryToDemos =
            kAllGalleryDemoCategories.ToDictionary(
                category => category,
                category => kAllGalleryDemos.Where(demo => demo.category == category).ToList()
            );

        public static readonly Dictionary<string, string> kDemoDocumentationUrl =
            kAllGalleryDemos.Where(demo => demo.documentationUrl != null).ToDictionary(
                demo => demo.routeName,
                demo => demo.documentationUrl
            );
    }
}