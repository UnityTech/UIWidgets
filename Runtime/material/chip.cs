using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    class ChipUtils {
public const float _kChipHeight = 32.0f;
public const float _kDeleteIconSize = 18.0f;
public const int _kCheckmarkAlpha = 0xde; // 87%
public const int _kDisabledAlpha = 0x61; // 38%
public const float _kCheckmarkStrokeWidth = 2.0f;
public static readonly TimeSpan _kSelectDuration = new TimeSpan(0, 0, 0, 0, 195);
public static readonly TimeSpan _kCheckmarkDuration = new TimeSpan(0, 0, 0, 0, 150);
public static readonly TimeSpan _kCheckmarkReverseDuration = new TimeSpan(0, 0, 0, 0, 50);
public static readonly TimeSpan _kDrawerDuration = new TimeSpan(0, 0, 0, 0, 150);
public static readonly TimeSpan _kReverseDrawerDuration = new TimeSpan(0, 0, 0, 0, 100);
public static readonly TimeSpan _kDisableDuration = new TimeSpan(0, 0, 0, 0, 75);
public static readonly Color _kSelectScrimColor = new Color(0x60191919);
public static readonly Icon _kDefaultDeleteIcon = new Icon(Icons.cancel, size: _kDeleteIconSize);
    }

public interface ChipAttributes {
  Widget label { get; }

  Widget avatar { get; }

  TextStyle labelStyle { get; }

  ShapeBorder shape { get; }

  Clip clipBehavior { get; }

  Color backgroundColor { get; }

  EdgeInsets padding { get; }

  EdgeInsets labelPadding { get; }

  MaterialTapTargetSize? materialTapTargetSize { get; }

  float? elevation { get; }

  Color shadowColor { get; }
}

public interface DeletableChipAttributes {
  Widget deleteIcon { get; }

  VoidCallback onDeleted { get; }

  Color deleteIconColor { get; }

  string deleteButtonTooltipMessage { get; }
}

public interface SelectableChipAttributes {
  bool selected { get; }

  ValueChanged<bool> onSelected { get; }

  float pressElevation { get; }

  Color selectedColor { get; }

  Color selectedShadowColor { get; }

  string tooltip { get; }

  ShapeBorder avatarBorder { get; }
}

public interface DisabledChipAttributes {
  bool isEnabled { get; }

  Color disabledColor { get; }
}

public interface TappableChipAttributes {
  VoidCallback onPressed { get; }

  float pressElevation { get; }

  string tooltip { get; }
}

public class Chip : StatelessWidget, ChipAttributes, DeletableChipAttributes {
  public Chip(
    Key key = null,
    Widget avatar = null,
    Widget label = null,
    TextStyle labelStyle = null,
    EdgeInsets labelPadding = null,
    Widget deleteIcon = null,
    VoidCallback onDeleted = null,
    Color deleteIconColor = null,
    string deleteButtonTooltipMessage = null,
    ShapeBorder shape = null,
    Clip clipBehavior = Clip.none,
    Color backgroundColor = null,
    EdgeInsets padding = null,
    MaterialTapTargetSize? materialTapTargetSize = null,
    float? elevation = null,
    Color shadowColor = null
  ) : base(key: key) {
      D.assert(label != null);
       D.assert(elevation == null || elevation >= 0.0f);
    this._avatar = avatar;
    this._label = label;
    this._labelStyle = labelStyle;
    this._labelPadding = labelPadding;
    this._deleteIcon = deleteIcon;
    this._onDeleted = onDeleted;
    this._deleteIconColor = deleteIconColor;
    this._deleteButtonTooltipMessage = deleteButtonTooltipMessage;
    this._shape = shape;
    this._clipBehavior = clipBehavior;
    this._backgroundColor = backgroundColor;
    this._padding = padding;
    this._materialTapTargetSize = materialTapTargetSize;
    this._elevation = elevation;
    this._shadowColor = shadowColor;
       }

  public Widget avatar {
      get { return this._avatar; }
  }
    Widget _avatar;
  public Widget label {
      get { return this._label; }
  }
    Widget _label;
  public TextStyle labelStyle {
      get { return this._labelStyle; }
  }
    TextStyle _labelStyle;
  public EdgeInsets labelPadding {
      get { return this._labelPadding; }
  }
    EdgeInsets _labelPadding;
  public ShapeBorder shape {
      get { return this._shape; }
  }
    ShapeBorder _shape;
  public Clip clipBehavior {
      get { return this._clipBehavior; }
  }
    Clip _clipBehavior;
  public Color backgroundColor {
      get { return this._backgroundColor; }
  }
    Color _backgroundColor;
  public EdgeInsets padding {
      get { return this._padding; }
  }
    EdgeInsets _padding;
  public Widget deleteIcon {
      get { return this._deleteIcon; }
  }
    Widget _deleteIcon;
  public VoidCallback onDeleted {
      get { return this._onDeleted; }
  }
    VoidCallback _onDeleted;
  public Color deleteIconColor {
      get { return this._deleteIconColor; }
  }
    Color _deleteIconColor;
  public string deleteButtonTooltipMessage {
      get { return this._deleteButtonTooltipMessage; }
  }
    string _deleteButtonTooltipMessage;
  public MaterialTapTargetSize? materialTapTargetSize {
      get { return this._materialTapTargetSize; }
  }
    MaterialTapTargetSize? _materialTapTargetSize;
  public float? elevation {
      get { return this._elevation; }
  }
    float? _elevation;
  public Color shadowColor {
      get { return this._shadowColor; }
  }
    Color _shadowColor;

  public override Widget build(BuildContext context) {
    D.assert(MaterialD.debugCheckHasMaterial(context));
    return new RawChip(
      avatar: avatar,
      label: label,
      labelStyle: labelStyle,
      labelPadding: labelPadding,
      deleteIcon: deleteIcon,
      onDeleted: onDeleted,
      deleteIconColor: deleteIconColor,
      deleteButtonTooltipMessage: deleteButtonTooltipMessage,
      tapEnabled: false,
      shape: shape,
      clipBehavior: clipBehavior,
      backgroundColor: backgroundColor,
      padding: padding,
      materialTapTargetSize: materialTapTargetSize,
      elevation: elevation,
      shadowColor: shadowColor,
      isEnabled: true
    );
  }
}

public class InputChip : StatelessWidget,
        ChipAttributes,
        DeletableChipAttributes,
        SelectableChipAttributes,
        DisabledChipAttributes,
        TappableChipAttributes {
  public InputChip(
    Key key,
    Widget avatar,
    Widget label,
    TextStyle labelStyle,
    EdgeInsets labelPadding,
    bool selected = false,
    bool isEnabled = true,
    ValueChanged<bool> onSelected,
    Widget deleteIcon,
    VoidCallback onDeleted,
    Color deleteIconColor,
    string deleteButtonTooltipMessage,
    VoidCallback onPressed,
    float pressElevation,
    Color disabledColor,
    Color selectedColor,
    string tooltip,
    ShapeBorder shape,
    Clip clipBehavior = Clip.none,
    Color backgroundColor,
    EdgeInsets padding,
    MaterialTapTargetSize materialTapTargetSize,
    float elevation,
    Color shadowColor,
    Color selectedShadowColor,
    ShapeBorder avatarBorder = const CircleBorder()
  ) : base(key: key) {
      D.assert(selected != null);
       D.assert(isEnabled != null);
       D.assert(label != null);
       D.assert(clipBehavior != null);
       D.assert(pressElevation == null || pressElevation >= 0.0f);
       D.assert(elevation == null || elevation >= 0.0f);
       }

  public Widget avatar;
  public Widget label;
  public TextStyle labelStyle;
  public EdgeInsets labelPadding;
  public bool selected;
  public bool isEnabled;
  public ValueChanged<bool> onSelected;
  public Widget deleteIcon;
  public VoidCallback onDeleted;
  public Color deleteIconColor;
  public string deleteButtonTooltipMessage;
  public VoidCallback onPressed;
  public float pressElevation;
  public Color disabledColor;
  public Color selectedColor;
  public string tooltip;
  public ShapeBorder shape;
  public Clip clipBehavior;
  public Color backgroundColor;
  public EdgeInsets padding;
  public MaterialTapTargetSize materialTapTargetSize;
  public float elevation;
  public Color shadowColor;
  public Color selectedShadowColor;
  public ShapeBorder avatarBorder;

  public override Widget build(BuildContext context) {
    D.assert(MaterialD.debugCheckHasMaterial(context));
    return new RawChip(
      avatar: avatar,
      label: label,
      labelStyle: labelStyle,
      labelPadding: labelPadding,
      deleteIcon: deleteIcon,
      onDeleted: onDeleted,
      deleteIconColor: deleteIconColor,
      deleteButtonTooltipMessage: deleteButtonTooltipMessage,
      onSelected: onSelected,
      onPressed: onPressed,
      pressElevation: pressElevation,
      selected: selected,
      tapEnabled: true,
      disabledColor: disabledColor,
      selectedColor: selectedColor,
      tooltip: tooltip,
      shape: shape,
      clipBehavior: clipBehavior,
      backgroundColor: backgroundColor,
      padding: padding,
      materialTapTargetSize: materialTapTargetSize,
      elevation: elevation,
      shadowColor: shadowColor,
      selectedShadowColor: selectedShadowColor,
      isEnabled: isEnabled && (onSelected != null || onDeleted != null || onPressed != null),
      avatarBorder: avatarBorder
    );
  }
}

public class ChoiceChip : StatelessWidget
    implements
        ChipAttributes,
        SelectableChipAttributes,
        DisabledChipAttributes {
  public ChoiceChip(
    Key key,
    Widget avatar,
    required this.label,
    TextStyle labelStyle,
    EdgeInsets labelPadding,
    ValueChanged<bool> onSelected,
    float pressElevation,
    required this.selected,
    Color selectedColor,
    Color disabledColor,
    string tooltip,
    ShapeBorder shape,
    Clip clipBehavior = Clip.none,
    Color backgroundColor,
    EdgeInsets padding,
    MaterialTapTargetSize materialTapTargetSize,
    float elevation,
    Color shadowColor,
    Color selectedShadowColor,
    ShapeBorder avatarBorder = const CircleBorder()
  ) : base(key: key) {
      D.assert(selected != null);
       D.assert(label != null);
       D.assert(clipBehavior != null);
       D.assert(pressElevation == null || pressElevation >= 0.0f);
       D.assert(elevation == null || elevation >= 0.0f);
       }

  public Widget avatar;
  public Widget label;
  public TextStyle labelStyle;
  public EdgeInsets labelPadding;
  public ValueChanged<bool> onSelected;
  public float pressElevation;
  public bool selected;
  public Color disabledColor;
  public Color selectedColor;
  public string tooltip;
  public ShapeBorder shape;
  public Clip clipBehavior;
  public Color backgroundColor;
  public EdgeInsets padding;
  public MaterialTapTargetSize materialTapTargetSize;
  public float elevation;
  public Color shadowColor;
  public Color selectedShadowColor;
  public ShapeBorder avatarBorder;

  public bool get isEnabled => onSelected != null;

  public override Widget build(BuildContext context) {
    D.assert(MaterialD.debugCheckHasMaterial(context));
    ChipThemeData chipTheme = ChipTheme.of(context);
    return new RawChip(
      avatar: avatar,
      label: label,
      labelStyle: labelStyle ?? (selected ? chipTheme.secondaryLabelStyle : null),
      labelPadding: labelPadding,
      onSelected: onSelected,
      pressElevation: pressElevation,
      selected: selected,
      showCheckmark: false,
      onDeleted: null,
      tooltip: tooltip,
      shape: shape,
      clipBehavior: clipBehavior,
      disabledColor: disabledColor,
      selectedColor: selectedColor ?? chipTheme.secondarySelectedColor,
      backgroundColor: backgroundColor,
      padding: padding,
      isEnabled: isEnabled,
      materialTapTargetSize: materialTapTargetSize,
      elevation: elevation,
      shadowColor: shadowColor,
      selectedShadowColor: selectedShadowColor,
      avatarBorder: avatarBorder
    );
  }
}

public class FilterChip : StatelessWidget
    implements
        ChipAttributes,
        SelectableChipAttributes,
        DisabledChipAttributes {
  public FilterChip(
    Key key,
    Widget avatar,
    required this.label,
    TextStyle labelStyle,
    EdgeInsets labelPadding,
    bool selected = false,
    required this.onSelected,
    float pressElevation,
    Color disabledColor,
    Color selectedColor,
    string tooltip,
    ShapeBorder shape,
    Clip clipBehavior = Clip.none,
    Color backgroundColor,
    EdgeInsets padding,
    MaterialTapTargetSize materialTapTargetSize,
    float elevation,
    Color shadowColor,
    Color selectedShadowColor,
    ShapeBorder avatarBorder = const CircleBorder()
  ) : base(key: key) {
      D.assert(selected != null);
       D.assert(label != null);
       D.assert(clipBehavior != null);
       D.assert(pressElevation == null || pressElevation >= 0.0f);
       D.assert(elevation == null || elevation >= 0.0f);
       }

  public Widget avatar;
  public Widget label;
  public TextStyle labelStyle;
  public EdgeInsets labelPadding;
  public bool selected;
  public ValueChanged<bool> onSelected;
  public float pressElevation;
  public Color disabledColor;
  public Color selectedColor;
  public string tooltip;
  public ShapeBorder shape;
  public Clip clipBehavior;
  public Color backgroundColor;
  public EdgeInsets padding;
  public MaterialTapTargetSize materialTapTargetSize;
  public float elevation;
  public Color shadowColor;
  public Color selectedShadowColor;
  public ShapeBorder avatarBorder;

  public bool get isEnabled => onSelected != null;

  public override Widget build(BuildContext context) {
    D.assert(MaterialD.debugCheckHasMaterial(context));
    return new RawChip(
      avatar: avatar,
      label: label,
      labelStyle: labelStyle,
      labelPadding: labelPadding,
      onSelected: onSelected,
      pressElevation: pressElevation,
      selected: selected,
      tooltip: tooltip,
      shape: shape,
      clipBehavior: clipBehavior,
      backgroundColor: backgroundColor,
      disabledColor: disabledColor,
      selectedColor: selectedColor,
      padding: padding,
      isEnabled: isEnabled,
      materialTapTargetSize: materialTapTargetSize,
      elevation: elevation,
      shadowColor: shadowColor,
      selectedShadowColor: selectedShadowColor,
      avatarBorder: avatarBorder
    );
  }
}

public class ActionChip : StatelessWidget implements ChipAttributes, TappableChipAttributes {
  public ActionChip(
    Key key,
    Widget avatar,
    required this.label,
    using labelStyle,
    EdgeInsets labelPadding,
    required this.onPressed,
    using pressElevation,
    using tooltip,
    using shape,
    using clipBehavior = Clip.none,
    Color backgroundColor,
    EdgeInsets padding,
    using materialTapTargetSize,
    using elevation,
    using shadowColor
  ) : D.assert(label != null),
       D.assert(
         onPressed != null,
         "Rather than disabling an ActionChip by setting onPressed to null, " +
         "remove it from the interface entirely."
       ),
       D.assert(pressElevation == null || pressElevation >= 0.0f);
       D.assert(elevation == null || elevation >= 0.0f);
       base(key: key);

  public Widget avatar;
  public Widget label;
  public TextStyle labelStyle;
  public EdgeInsets labelPadding;
  public VoidCallback onPressed;
  public float pressElevation;
  public string tooltip;
  public ShapeBorder shape;
  public Clip clipBehavior;
  public Color backgroundColor;
  public EdgeInsets padding;
  public MaterialTapTargetSize materialTapTargetSize;
  public float elevation;
  public Color shadowColor;

  public override Widget build(BuildContext context) {
    D.assert(MaterialD.debugCheckHasMaterial(context));
    return new RawChip(
      avatar: avatar,
      label: label,
      onPressed: onPressed,
      pressElevation: pressElevation,
      tooltip: tooltip,
      labelStyle: labelStyle,
      backgroundColor: backgroundColor,
      shape: shape,
      clipBehavior: clipBehavior,
      padding: padding,
      labelPadding: labelPadding,
      isEnabled: true,
      materialTapTargetSize: materialTapTargetSize,
      elevation: elevation,
      shadowColor: shadowColor
    );
  }
}

public class RawChip : StatefulWidget
    implements
        ChipAttributes,
        DeletableChipAttributes,
        SelectableChipAttributes,
        DisabledChipAttributes,
        TappableChipAttributes {
  public RawChip(
    Key key,
    Widget avatar,
    required this.label,
    TextStyle labelStyle,
    EdgeInsets padding,
    EdgeInsets labelPadding,
    Widget deleteIcon,
    VoidCallback onDeleted,
    Color deleteIconColor,
    string deleteButtonTooltipMessage,
    VoidCallback onPressed,
    ValueChanged<bool> onSelected,
    float pressElevation,
    bool tapEnabled = true,
    bool selected,
    bool showCheckmark = true,
    bool isEnabled = true,
    Color disabledColor,
    Color selectedColor,
    string tooltip,
    ShapeBorder shape,
    Clip clipBehavior = Clip.none,
    Color backgroundColor,
    MaterialTapTargetSize materialTapTargetSize,
    float elevation,
    Color shadowColor,
    Color selectedShadowColor,
    ShapeBorder avatarBorder = const CircleBorder()
  ) : D.assert(label != null),
       D.assert(isEnabled != null);
       D.assert(clipBehavior != null);
       D.assert(pressElevation == null || pressElevation >= 0.0f);
       D.assert(elevation == null || elevation >= 0.0f);
       deleteIcon = deleteIcon ?? _kDefaultDeleteIcon,
       base(key: key);

  public Widget avatar;
  public Widget label;
  public TextStyle labelStyle;
  public EdgeInsets labelPadding;
  public Widget deleteIcon;
  public VoidCallback onDeleted;
  public Color deleteIconColor;
  public string deleteButtonTooltipMessage;
  public ValueChanged<bool> onSelected;
  public VoidCallback onPressed;
  public float pressElevation;
  public bool selected;
  public bool isEnabled;
  public Color disabledColor;
  public Color selectedColor;
  public string tooltip;
  public ShapeBorder shape;
  public Clip clipBehavior;
  public Color backgroundColor;
  public EdgeInsets padding;
  public MaterialTapTargetSize materialTapTargetSize;
  public float elevation;
  public Color shadowColor;
  public Color selectedShadowColor;
  public CircleBorder avatarBorder;

  public readonly bool showCheckmark;

  public readonly bool tapEnabled;

  public override State createState() => new _RawChipState();
}

class _RawChipState : State<RawChip> with TickerProviderStateMixin<RawChip> {
  static readonly TimeSpan pressedAnimationDuration = new TimeSpan(0, 0, 0, 0, 75);

  AnimationController selectController;
  AnimationController avatarDrawerController;
  AnimationController deleteDrawerController;
  AnimationController enableController;
  Animation<float> checkmarkAnimation;
  Animation<float> avatarDrawerAnimation;
  Animation<float> deleteDrawerAnimation;
  Animation<float> enableAnimation;
  Animation<float> selectionFade;

public   bool hasDeleteButton {
    get {
        return widget.onDeleted != null;
    }
}
public   bool hasAvatar {
    get {
        return widget.avatar != null;
    }
}

  bool get canTap {
    return widget.isEnabled
        && widget.tapEnabled
        && (widget.onPressed != null || widget.onSelected != null);
  }

  bool _isTapping = false;
public   bool isTapping {
    get {
        return !canTap ? false : _isTapping;
    }
}

  public override void initState() {
    D.assert(widget.onSelected == null || widget.onPressed == null);
    base.initState();
    selectController = new AnimationController(
      duration: _kSelectDuration,
      value: widget.selected == true ? 1.0f : 0.0f,
      vsync: this
    );
    selectionFade = new CurvedAnimation(
      parent: selectController,
      curve: Curves.fastOutSlowIn
    );
    avatarDrawerController = new AnimationController(
      duration: _kDrawerDuration,
      value: hasAvatar || widget.selected == true ? 1.0f : 0.0f,
      vsync: this
    );
    deleteDrawerController = new AnimationController(
      duration: _kDrawerDuration,
      value: hasDeleteButton ? 1.0f : 0.0f,
      vsync: this
    );
    enableController = new AnimationController(
      duration: _kDisableDuration,
      value: widget.isEnabled ? 1.0f : 0.0f,
      vsync: this
    );

    float checkmarkPercentage = _kCheckmarkDuration.inMilliseconds /
        _kSelectDuration.inMilliseconds;
    float checkmarkReversePercentage = _kCheckmarkReverseDuration.inMilliseconds /
        _kSelectDuration.inMilliseconds;
    float avatarDrawerReversePercentage = _kReverseDrawerDuration.inMilliseconds /
        _kSelectDuration.inMilliseconds;
    checkmarkAnimation = new CurvedAnimation(
      parent: selectController,
      curve: Interval(1.0f - checkmarkPercentage, 1.0f, curve: Curves.fastOutSlowIn),
      reverseCurve: Interval(
        1.0f - checkmarkReversePercentage,
        1.0f,
        curve: Curves.fastOutSlowIn
      )
    );
    deleteDrawerAnimation = new CurvedAnimation(
      parent: deleteDrawerController,
      curve: Curves.fastOutSlowIn
    );
    avatarDrawerAnimation = new CurvedAnimation(
      parent: avatarDrawerController,
      curve: Curves.fastOutSlowIn,
      reverseCurve: Interval(
        1.0f - avatarDrawerReversePercentage,
        1.0f,
        curve: Curves.fastOutSlowIn
      )
    );
    enableAnimation = new CurvedAnimation(
      parent: enableController,
      curve: Curves.fastOutSlowIn
    );
  }

  public override void dispose() {
    selectController.dispose();
    avatarDrawerController.dispose();
    deleteDrawerController.dispose();
    enableController.dispose();
    base.dispose();
  }

  void _handleTapDown(TapDownDetails details) {
    if (!canTap) {
      return;
    }
    setState(() => {
      _isTapping = true;
    });
  }

  void _handleTapCancel() {
    if (!canTap) {
      return;
    }
    setState(() => {
      _isTapping = false;
    });
  }

  void _handleTap() {
    if (!canTap) {
      return;
    }
    setState(() => {
      _isTapping = false;
    });
    widget.onSelected?.call(!widget.selected);
    widget.onPressed?.call();
  }

  Color getBackgroundColor(ChipThemeData theme) {
    ColorTween backgroundTween = new ColorTween(
      begin: widget.disabledColor ?? theme.disabledColor,
      end: widget.backgroundColor ?? theme.backgroundColor
    );
    ColorTween selectTween = new ColorTween(
      begin: backgroundTween.evaluate(enableController),
      end: widget.selectedColor ?? theme.selectedColor
    );
    return selectTween.evaluate(selectionFade);
  }

  public override void didUpdateWidget(RawChip oldWidget) {
    base.didUpdateWidget(oldWidget);
    if (oldWidget.isEnabled != widget.isEnabled) {
      setState(() => {
        if (widget.isEnabled) {
          enableController.forward();
        } else {
          enableController.reverse();
        }
      });
    }
    if (oldWidget.avatar != widget.avatar || oldWidget.selected != widget.selected) {
      setState(() => {
        if (hasAvatar || widget.selected == true) {
          avatarDrawerController.forward();
        } else {
          avatarDrawerController.reverse();
        }
      });
    }
    if (oldWidget.selected != widget.selected) {
      setState(() => {
        if (widget.selected == true) {
          selectController.forward();
        } else {
          selectController.reverse();
        }
      });
    }
    if (oldWidget.onDeleted != widget.onDeleted) {
      setState(() => {
        if (hasDeleteButton) {
          deleteDrawerController.forward();
        } else {
          deleteDrawerController.reverse();
        }
      });
    }
  }

  Widget _wrapWithTooltip(string tooltip, VoidCallback callback, Widget child) {
    if (child == null || callback == null || tooltip == null) {
      return child;
    }
    return new Tooltip(
      message: tooltip,
      child: child
    );
  }

  Widget _buildDeleteIcon(BuildContext context, ThemeData theme, ChipThemeData chipTheme) {
    if (!hasDeleteButton) {
      return null;
    }
    return new _wrapWithTooltip(
      widget.deleteButtonTooltipMessage ?? MaterialLocalizations.of(context)?.deleteButtonTooltip,
      widget.onDeleted,
      InkResponse(
        onTap: widget.isEnabled ? widget.onDeleted : null,
        child: new IconTheme(
          data: theme.iconTheme.copyWith(
            color: widget.deleteIconColor ?? chipTheme.deleteIconColor
          ),
          child: widget.deleteIcon
        )
      )
    );
  }

  const float _defaultElevation = 0.0f;
  const float _defaultPressElevation = 8.0f;
  static readonly Color _defaultShadowColor = Colors.black;

  public override Widget build(BuildContext context) {
    D.assert(MaterialD.debugCheckHasMaterial(context));
    D.assert(debugCheckHasMediaQuery(context));
    D.assert(WidgetsD.debugCheckHasDirectionality(context));
    D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));

    ThemeData theme = Theme.of(context);
    ChipThemeData chipTheme = ChipTheme.of(context);
    TextDirection textDirection = Directionality.of(context);
    ShapeBorder shape = widget.shape ?? chipTheme.shape;
    float elevation = widget.elevation ?? chipTheme.elevation ?? _defaultElevation;
    float pressElevation = widget.pressElevation ?? chipTheme.pressElevation ?? _defaultPressElevation;
    Color shadowColor = widget.shadowColor ?? chipTheme.shadowColor ?? _defaultShadowColor;
    Color selectedShadowColor = widget.selectedShadowColor ?? chipTheme.selectedShadowColor ?? _defaultShadowColor;
    bool selected = widget.selected ?? false;

    Widget result = Material(
      elevation: isTapping ? pressElevation : elevation,
      shadowColor: selected ? selectedShadowColor : shadowColor,
      animationDuration: pressedAnimationDuration,
      shape: shape,
      clipBehavior: widget.clipBehavior,
      child: new InkWell(
        onTap: canTap ? _handleTap : null,
        onTapDown: canTap ? _handleTapDown : null,
        onTapCancel: canTap ? _handleTapCancel : null,
        customBorder: shape,
        child: new AnimatedBuilder(
          animation: Listenable.merge(new List<Listenable>{selectController, enableController}),
          builder: (BuildContext context, Widget child) => {
            return new Container(
              decoration: ShapeDecoration(
                shape: shape,
                color: getBackgroundColor(chipTheme)
              ),
              child: child
            );
          },
          child: new _wrapWithTooltip(
            widget.tooltip,
            widget.onPressed,
            _ChipRenderWidget(
              theme: _ChipRenderTheme(
                label: DefaultTextStyle(
                  overflow: TextOverflow.fade,
                  textAlign: TextAlign.start,
                  maxLines: 1,
                  softWrap: false,
                  style: widget.labelStyle ?? chipTheme.labelStyle,
                  child: widget.label
                ),
                avatar: AnimatedSwitcher(
                  child: widget.avatar,
                  duration: _kDrawerDuration,
                  switchInCurve: Curves.fastOutSlowIn
                ),
                deleteIcon: AnimatedSwitcher(
                  child: new _buildDeleteIcon(context, theme, chipTheme),
                  duration: _kDrawerDuration,
                  switchInCurve: Curves.fastOutSlowIn
                ),
                brightness: chipTheme.brightness,
                padding: (widget.padding ?? chipTheme.padding).resolve(textDirection),
                labelPadding: (widget.labelPadding ?? chipTheme.labelPadding).resolve(textDirection),
                showAvatar: hasAvatar,
                showCheckmark: widget.showCheckmark,
                canTapBody: canTap
              ),
              value: widget.selected,
              checkmarkAnimation: checkmarkAnimation,
              enableAnimation: enableAnimation,
              avatarDrawerAnimation: avatarDrawerAnimation,
              deleteDrawerAnimation: deleteDrawerAnimation,
              isEnabled: widget.isEnabled,
              avatarBorder: widget.avatarBorder
            )
          )
        )
      )
    );
    BoxConstraints constraints;
    switch (widget.materialTapTargetSize ?? theme.materialTapTargetSize) {
      case MaterialTapTargetSize.padded:
        constraints = const BoxConstraints(minHeight: 48.0f);
        break;
      case MaterialTapTargetSize.shrinkWrap:
        constraints = const BoxConstraints();
        break;
    }
    result = _ChipRedirectingHitDetectionWidget(
      constraints: constraints,
      child: new Center(
        child: result,
        widthFactor: 1.0f,
        heightFactor: 1.0f
      )
    );
    return new Semantics(
      container: true,
      selected: widget.selected,
      enabled: canTap ? widget.isEnabled : null,
      child: result
    );
  }
}

class _ChipRedirectingHitDetectionWidget : SingleChildRenderObjectWidget {
  public _ChipRedirectingHitDetectionWidget(
    Key key,
    Widget child,
    BoxConstraints constraints
  ) : base(key: key, child: child) {
  }

  public readonly BoxConstraints constraints;

  public override RenderObject createRenderObject(BuildContext context) {
    return new _RenderChipRedirectingHitDetection(constraints);
  }

  public override void updateRenderObject(BuildContext context, covariant _RenderChipRedirectingHitDetection renderObject) {
    renderObject.additionalConstraints = constraints;
  }
}

class _RenderChipRedirectingHitDetection : RenderConstrainedBox {
  public _RenderChipRedirectingHitDetection(BoxConstraints additionalConstraints) : base(additionalConstraints: additionalConstraints) {
  }

  public override bool hitTest(HitTestResult result, { Offset position }) {
    if (!size.Contains(position))
      return false;
    return child.hitTest(result, position: new Offset(position.dx, size.height / 2));
  }
}

class _ChipRenderWidget : RenderObjectWidget {
  public _ChipRenderWidget(
    Key key,
    _ChipRenderTheme theme,
    bool value,
    bool isEnabled,
    Animation<float> checkmarkAnimation,
    Animation<float> avatarDrawerAnimation,
    Animation<float> deleteDrawerAnimation,
    Animation<float> enableAnimation,
    ShapeBorder avatarBorder
  ) : base(key: key) {
      D.assert(theme != null);
       }

  public readonly _ChipRenderTheme theme;
  public readonly bool value;
  public readonly bool isEnabled;
  public readonly Animation<float> checkmarkAnimation;
  public readonly Animation<float> avatarDrawerAnimation;
  public readonly Animation<float> deleteDrawerAnimation;
  public readonly Animation<float> enableAnimation;
  public readonly ShapeBorder avatarBorder;

  public override _RenderChipElement createElement() => _RenderChipElement(this);

  public override void updateRenderObject(BuildContext context, _RenderChip renderObject) {
    renderObject
      ..theme = theme
      ..textDirection = Directionality.of(context)
      ..value = value
      ..isEnabled = isEnabled
      ..checkmarkAnimation = checkmarkAnimation
      ..avatarDrawerAnimation = avatarDrawerAnimation
      ..deleteDrawerAnimation = deleteDrawerAnimation
      ..enableAnimation = enableAnimation
      ..avatarBorder = avatarBorder;
  }

  public override RenderObject createRenderObject(BuildContext context) {
    return new _RenderChip(
      theme: theme,
      textDirection: Directionality.of(context),
      value: value,
      isEnabled: isEnabled,
      checkmarkAnimation: checkmarkAnimation,
      avatarDrawerAnimation: avatarDrawerAnimation,
      deleteDrawerAnimation: deleteDrawerAnimation,
      enableAnimation: enableAnimation,
      avatarBorder: avatarBorder
    );
  }
}

enum _ChipSlot {
  label,
  avatar,
  deleteIcon
}

class _RenderChipElement : RenderObjectElement {
  public _RenderChipElement(_ChipRenderWidget chip) : base(chip) {
  }

  Dictionary<_ChipSlot, Element> slotToChild = new Dictionary<_ChipSlot, Element>{};
  Dictionary<Element, _ChipSlot> childToSlot = new Dictionary<Element, _ChipSlot>{};

  public override _ChipRenderWidget get widget => base.widget;

  public override _RenderChip get renderObject => base.renderObject;

  public override void visitChildren(ElementVisitor visitor) {
    slotToChild.values.forEach(visitor);
  }

  public override void forgetChild(Element child) {
    D.assert(slotToChild.values.Contains(child));
    D.assert(childToSlot.keys.Contains(child));
    _ChipSlot slot = childToSlot[child];
    childToSlot.Remove(child);
    slotToChild.Remove(slot);
  }

  void _mountChild(Widget widget, _ChipSlot slot) {
    Element oldChild = slotToChild[slot];
    Element newChild = updateChild(oldChild, widget, slot);
    if (oldChild != null) {
      slotToChild.Remove(slot);
      childToSlot.Remove(oldChild);
    }
    if (newChild != null) {
      slotToChild[slot] = newChild;
      childToSlot[newChild] = slot;
    }
  }

  public override void mount(Element parent, dynamic newSlot) {
    base.mount(parent, newSlot);
    _mountChild(widget.theme.avatar, _ChipSlot.avatar);
    _mountChild(widget.theme.deleteIcon, _ChipSlot.deleteIcon);
    _mountChild(widget.theme.label, _ChipSlot.label);
  }

  void _updateChild(Widget widget, _ChipSlot slot) {
    Element oldChild = slotToChild[slot];
    Element newChild = updateChild(oldChild, widget, slot);
    if (oldChild != null) {
      childToSlot.Remove(oldChild);
      slotToChild.Remove(slot);
    }
    if (newChild != null) {
      slotToChild[slot] = newChild;
      childToSlot[newChild] = slot;
    }
  }

  public override void update(_ChipRenderWidget newWidget) {
    base.update(newWidget);
    D.assert(widget == newWidget);
    _updateChild(widget.theme.label, _ChipSlot.label);
    _updateChild(widget.theme.avatar, _ChipSlot.avatar);
    _updateChild(widget.theme.deleteIcon, _ChipSlot.deleteIcon);
  }

  void _updateRenderObject(RenderObject child, _ChipSlot slot) {
    switch (slot) {
      case _ChipSlot.avatar:
        renderObject.avatar = child;
        break;
      case _ChipSlot.label:
        renderObject.label = child;
        break;
      case _ChipSlot.deleteIcon:
        renderObject.deleteIcon = child;
        break;
    }
  }

  public override void insertChildRenderObject(RenderObject child, dynamic slotValue) {
    D.assert(child is RenderBox);
    D.assert(slotValue is _ChipSlot);
    _ChipSlot slot = slotValue;
    _updateRenderObject(child, slot);
    D.assert(renderObject.childToSlot.keys.Contains(child));
    D.assert(renderObject.slotToChild.keys.Contains(slot));
  }

  public override void removeChildRenderObject(RenderObject child) {
    D.assert(child is RenderBox);
    D.assert(renderObject.childToSlot.keys.Contains(child));
    _updateRenderObject(null, renderObject.childToSlot[child]);
    D.assert(!renderObject.childToSlot.keys.Contains(child));
    D.assert(!renderObject.slotToChild.keys.Contains(slot));
  }

  public override void moveChildRenderObject(RenderObject child, dynamic slotValue) {
    D.assert(false, "not reachable");
  }
}

class _ChipRenderTheme {
  public _ChipRenderTheme(
    required this.avatar,
    required this.label,
    required this.deleteIcon,
    required this.brightness,
    required this.padding,
    required this.labelPadding,
    required this.showAvatar,
    required this.showCheckmark,
    required this.canTapBody
  ) {}

  public readonly Widget avatar;
  public readonly Widget label;
  public readonly Widget deleteIcon;
  public readonly Brightness brightness;
  public readonly EdgeInsets padding;
  public readonly EdgeInsets labelPadding;
  public readonly bool showAvatar;
  public readonly bool showCheckmark;
  public readonly bool canTapBody;

  public override bool operator ==(dynamic other) {
    if (identical(this, other)) {
      return true;
    }
    if (other.runtimeType != runtimeType) {
      return false;
    }
    _ChipRenderTheme typedOther = other;
    return typedOther.avatar == avatar
        && typedOther.label == label
        && typedOther.deleteIcon == deleteIcon
        && typedOther.brightness == brightness
        && typedOther.padding == padding
        && typedOther.labelPadding == labelPadding
        && typedOther.showAvatar == showAvatar
        && typedOther.showCheckmark == showCheckmark
        && typedOther.canTapBody == canTapBody;
  }

  public override int get hashCode {
    return new hashValues(
      avatar,
      label,
      deleteIcon,
      brightness,
      padding,
      labelPadding,
      showAvatar,
      showCheckmark,
      canTapBody
    );
  }
}

class _RenderChip : RenderBox {
  public _RenderChip(
    required _ChipRenderTheme theme,
    required TextDirection textDirection,
    bool value,
    bool isEnabled,
    Animation<float> checkmarkAnimation,
    Animation<float> avatarDrawerAnimation,
    Animation<float> deleteDrawerAnimation,
    Animation<float> enableAnimation,
    ShapeBorder avatarBorder
  ) : D.assert(theme != null),
       D.assert(textDirection != null);
       _theme = theme;
       _textDirection = textDirection {
    checkmarkAnimation.addListener(markNeedsPaint);
    avatarDrawerAnimation.addListener(markNeedsLayout);
    deleteDrawerAnimation.addListener(markNeedsLayout);
    enableAnimation.addListener(markNeedsPaint);
  }

  Dictionary<_ChipSlot, RenderBox> slotToChild = new Dictionary<_ChipSlot, RenderBox>{};
  Dictionary<RenderBox, _ChipSlot> childToSlot = new Dictionary<RenderBox, _ChipSlot>{};

  bool value;
  bool isEnabled;
  Rect deleteButtonRect;
  Rect pressRect;
  Animation<float> checkmarkAnimation;
  Animation<float> avatarDrawerAnimation;
  Animation<float> deleteDrawerAnimation;
  Animation<float> enableAnimation;
  ShapeBorder avatarBorder;

  RenderBox _updateChild(RenderBox oldChild, RenderBox newChild, _ChipSlot slot) {
    if (oldChild != null) {
      dropChild(oldChild);
      childToSlot.Remove(oldChild);
      slotToChild.Remove(slot);
    }
    if (newChild != null) {
      childToSlot[newChild] = slot;
      slotToChild[slot] = newChild;
      adoptChild(newChild);
    }
    return newChild;
  }

  RenderBox _avatar;
public   RenderBox avatar {
    get {
        return _avatar;
    }
  set {
    _avatar = _updateChild(_avatar, value, _ChipSlot.avatar);
  }
}

  RenderBox _deleteIcon;
public   RenderBox deleteIcon {
    get {
        return _deleteIcon;
    }
  set {
    _deleteIcon = _updateChild(_deleteIcon, value, _ChipSlot.deleteIcon);
  }
}

  RenderBox _label;
public   RenderBox label {
    get {
        return _label;
    }
  set {
    _label = _updateChild(_label, value, _ChipSlot.label);
  }
}

public   _ChipRenderTheme theme {
    get {
        return _theme;
    }
  set {
    if (_theme == value) {
      return;
    }
    _theme = value;
    markNeedsLayout();
  }
}
  _ChipRenderTheme _theme;

public   TextDirection textDirection {
    get {
        return _textDirection;
    }
  set {
    if (_textDirection == value) {
      return;
    }
    _textDirection = value;
    markNeedsLayout();
  }
}
  TextDirection _textDirection;

  IEnumerable<RenderBox> get _children sync* {
    if (avatar != null) {
      yield avatar;
    }
    if (label != null) {
      yield label;
    }
    if (deleteIcon != null) {
      yield deleteIcon;
    }
  }

public   bool isDrawingCheckmark {
    get {
        return theme.showCheckmark && !(checkmarkAnimation?.isDismissed ?? !value);
    }
}
public   bool deleteIconShowing {
    get {
        return !deleteDrawerAnimation.isDismissed;
    }
}

  public override void attach(PipelineOwner owner) {
    base.attach(owner);
    for (RenderBox child in _children) {
      child.attach(owner);
    }
  }

  public override void detach() {
    base.detach();
    for (RenderBox child in _children) {
      child.detach();
    }
  }

  public override void redepthChildren() {
    _children.forEach(redepthChild);
  }

  public override void visitChildren(RenderObjectVisitor visitor) {
    _children.forEach(visitor);
  }

  public override List<DiagnosticsNode> debugDescribeChildren() {
    List<DiagnosticsNode> value = new List<DiagnosticsNode>{};
    void add(RenderBox child, string name) {
      if (child != null) {
        value.Add(child.toDiagnosticsNode(name: name));
      }
    }

    add(avatar, "avatar");
    add(label, "label");
    add(deleteIcon, "deleteIcon");
    return value;
  }

  public override bool get sizedByParent => false;

  static float _minWidth(RenderBox box, float height) {
    return box == null ? 0.0f : box.getMinIntrinsicWidth(height);
  }

  static float _maxWidth(RenderBox box, float height) {
    return box == null ? 0.0f : box.getMaxIntrinsicWidth(height);
  }

  static float _minHeight(RenderBox box, float width) {
    return box == null ? 0.0f : box.getMinIntrinsicHeight(width);
  }

  static Size _boxSize(RenderBox box) => box == null ? Size.zero : box.size;

  static Rect _boxRect(RenderBox box) => box == null ? Rect.zero : _boxParentData(box).offset & box.size;

  static BoxParentData _boxParentData(RenderBox box) => box.parentData;

  public override float computeMinIntrinsicWidth(float height) {
    float overallPadding = theme.padding.horizontal +
        theme.labelPadding.horizontal;
    return overallPadding +
        _minWidth(avatar, height) +
        _minWidth(label, height) +
        _minWidth(deleteIcon, height);
  }

  public override float computeMaxIntrinsicWidth(float height) {
    float overallPadding = theme.padding.vertical +
        theme.labelPadding.horizontal;
    return overallPadding +
        _maxWidth(avatar, height) +
        _maxWidth(label, height) +
        _maxWidth(deleteIcon, height);
  }

  public override float computeMinIntrinsicHeight(float width) {
    return Mathf.Max(
      _kChipHeight,
      theme.padding.vertical + theme.labelPadding.vertical + _minHeight(label, width)
    );
  }

  public override float computeMaxIntrinsicHeight(float width) => computeMinIntrinsicHeight(width);

  public override float computeDistanceToActualBaseline(TextBaseline baseline) {
    return label.getDistanceToActualBaseline(baseline);
  }

  Size _layoutLabel(float iconSizes, Size size) {
    Size rawSize = _boxSize(label);
    if (constraints.maxWidth.isFinite) {
      label.layout(
        constraints.copyWith(
          minWidth: 0.0f,
          maxWidth: Mathf.Max(
            0.0f,
            constraints.maxWidth - iconSizes - theme.labelPadding.horizontal
          ),
          minHeight: rawSize.height,
          maxHeight: size.height
        ),
        parentUsesSize: true
      );
    } else {
      label.layout(
        BoxConstraints(
          minHeight: rawSize.height,
          maxHeight: size.height,
          minWidth: 0.0f,
          maxWidth: size.width
        ),
        parentUsesSize: true
      );
    }
    return new Size(
      rawSize.width + theme.labelPadding.horizontal,
      rawSize.height + theme.labelPadding.vertical
    );
  }

  Size _layoutAvatar(BoxConstraints contentConstraints, float contentSize) {
    float requestedSize = Mathf.Max(0.0f, contentSize);
    BoxConstraints avatarConstraints = BoxConstraints.tightFor(
      width: requestedSize,
      height: requestedSize
    );
    avatar.layout(avatarConstraints, parentUsesSize: true);
    if (!theme.showCheckmark && !theme.showAvatar) {
      return new Size(0.0f, contentSize);
    }
    float avatarWidth = 0.0f;
    float avatarHeight = 0.0f;
    Size avatarBoxSize = _boxSize(avatar);
    if (theme.showAvatar) {
      avatarWidth += avatarDrawerAnimation.value * avatarBoxSize.width;
    } else {
      avatarWidth += avatarDrawerAnimation.value * contentSize;
    }
    avatarHeight += avatarBoxSize.height;
    return new Size(avatarWidth, avatarHeight);
  }

  Size _layoutDeleteIcon(BoxConstraints contentConstraints, float contentSize) {
    float requestedSize = Mathf.Max(0.0f, contentSize);
    BoxConstraints deleteIconConstraints = BoxConstraints.tightFor(
      width: requestedSize,
      height: requestedSize
    );
    deleteIcon.layout(deleteIconConstraints, parentUsesSize: true);
    if (!deleteIconShowing) {
      return new Size(0.0f, contentSize);
    }
    float deleteIconWidth = 0.0f;
    float deleteIconHeight = 0.0f;
    Size boxSize = _boxSize(deleteIcon);
    deleteIconWidth += deleteDrawerAnimation.value * boxSize.width;
    deleteIconHeight += boxSize.height;
    return new Size(deleteIconWidth, deleteIconHeight);
  }

  public override bool hitTest(HitTestResult result, { Offset position }) {
    if (!size.Contains(position))
      return false;
    RenderBox hitTestChild;
    switch (textDirection) {
      case TextDirection.ltr:
        if (position.dx / size.width > 0.66f)
          hitTestChild = deleteIcon ?? label ?? avatar;
        else
          hitTestChild = label ?? avatar;
        break;
      case TextDirection.rtl:
        if (position.dx / size.width < 0.33f)
          hitTestChild = deleteIcon ?? label ?? avatar;
        else
          hitTestChild = label ?? avatar;
        break;
    }
    return hitTestChild?.hitTest(result, position: hitTestChild.size.center(Offset.zero)) ?? false;
  }

  protected override void performLayout() {
    BoxConstraints contentConstraints = constraints.loosen();
    label.layout(contentConstraints, parentUsesSize: true);
    float contentSize = Mathf.Max(
      _kChipHeight - theme.padding.vertical + theme.labelPadding.vertical,
      _boxSize(label).height + theme.labelPadding.vertical
    );
    Size avatarSize = _layoutAvatar(contentConstraints, contentSize);
    Size deleteIconSize = _layoutDeleteIcon(contentConstraints, contentSize);
    Size labelSize = Size(_boxSize(label).width, contentSize);
    labelSize = _layoutLabel(avatarSize.width + deleteIconSize.width, labelSize);

    Size overallSize = Size(
      avatarSize.width + labelSize.width + deleteIconSize.width,
      contentSize
    );


    const float left = 0.0f;
    float right = overallSize.width;

    Offset centerLayout(Size boxSize, float x) {
      D.assert(contentSize >= boxSize.height);
      Offset boxOffset;
      switch (textDirection) {
        case TextDirection.rtl:
          boxOffset = new Offset(x - boxSize.width, (contentSize - boxSize.height) / 2.0f);
          break;
        case TextDirection.ltr:
          boxOffset = new Offset(x, (contentSize - boxSize.height) / 2.0f);
          break;
      }
      return boxOffset;
    }

    Offset avatarOffset = Offset.zero;
    Offset labelOffset = Offset.zero;
    Offset deleteIconOffset = Offset.zero;
    switch (textDirection) {
      case TextDirection.rtl:
        float start = right;
        if (theme.showCheckmark || theme.showAvatar) {
          avatarOffset = centerLayout(avatarSize, start);
          start -= avatarSize.width;
        }
        labelOffset = centerLayout(labelSize, start);
        start -= labelSize.width;
        if (deleteIconShowing) {
          deleteButtonRect = Rect.fromLTWH(
            0.0f,
            0.0f,
            deleteIconSize.width + theme.padding.right,
            overallSize.height + theme.padding.vertical
          );
          deleteIconOffset = centerLayout(deleteIconSize, start);
        } else {
          deleteButtonRect = Rect.zero;
        }
        start -= deleteIconSize.width;
        if (theme.canTapBody) {
          pressRect = Rect.fromLTWH(
            deleteButtonRect.width,
            0.0f,
            overallSize.width - deleteButtonRect.width + theme.padding.horizontal,
            overallSize.height + theme.padding.vertical
          );
        } else {
          pressRect = Rect.zero;
        }
        break;
      case TextDirection.ltr:
        float start = left;
        if (theme.showCheckmark || theme.showAvatar) {
          avatarOffset = centerLayout(avatarSize, start - _boxSize(avatar).width + avatarSize.width);
          start += avatarSize.width;
        }
        labelOffset = centerLayout(labelSize, start);
        start += labelSize.width;
        if (theme.canTapBody) {
          pressRect = Rect.fromLTWH(
            0.0f,
            0.0f,
            deleteIconShowing
                ? start + theme.padding.left
                : overallSize.width + theme.padding.horizontal,
            overallSize.height + theme.padding.vertical
          );
        } else {
          pressRect = Rect.zero;
        }
        start -= _boxSize(deleteIcon).width - deleteIconSize.width;
        if (deleteIconShowing) {
          deleteIconOffset = centerLayout(deleteIconSize, start);
          deleteButtonRect = Rect.fromLTWH(
            start + theme.padding.left,
            0.0f,
            deleteIconSize.width + theme.padding.right,
            overallSize.height + theme.padding.vertical
          );
        } else {
          deleteButtonRect = Rect.zero;
        }
        break;
    }
    labelOffset = labelOffset +
        new Offset(
          0.0f,
          ((labelSize.height - theme.labelPadding.vertical) - _boxSize(label).height) / 2.0f
        );
    _boxParentData(avatar).offset = theme.padding.topLeft + avatarOffset;
    _boxParentData(label).offset = theme.padding.topLeft + labelOffset + theme.labelPadding.topLeft;
    _boxParentData(deleteIcon).offset = theme.padding.topLeft + deleteIconOffset;
    Size paddedSize = Size(
      overallSize.width + theme.padding.horizontal,
      overallSize.height + theme.padding.vertical
    );
    size = constraints.constrain(paddedSize);
    D.assert(
        size.height == constraints.constrainHeight(paddedSize.height),
        "Constrained height ${size.height} doesn't match expected height " +
        "${constraints.constrainWidth(paddedSize.height)}");
    D.assert(
        size.width == constraints.constrainWidth(paddedSize.width),
        "Constrained width ${size.width} doesn't match expected width " +
        "${constraints.constrainWidth(paddedSize.width)}");
  }

  static final ColorTween selectionScrimTween = new ColorTween(
    begin: Colors.transparent,
    end: _kSelectScrimColor
  );

  Color get _disabledColor {
    if (enableAnimation == null || enableAnimation.isCompleted) {
      return Colors.white;
    }
    ColorTween enableTween;
    switch (theme.brightness) {
      case Brightness.light:
        enableTween = new ColorTween(
          begin: Colors.white.withAlpha(_kDisabledAlpha),
          end: Colors.white
        );
        break;
      case Brightness.dark:
        enableTween = new ColorTween(
          begin: Colors.black.withAlpha(_kDisabledAlpha),
          end: Colors.black
        );
        break;
    }
    return enableTween.evaluate(enableAnimation);
  }

  void _paintCheck(Canvas canvas, Offset origin, float size) {
    Color paintColor;
    switch (theme.brightness) {
      case Brightness.light:
        paintColor = theme.showAvatar ? Colors.white : Colors.black.withAlpha(_kCheckmarkAlpha);
        break;
      case Brightness.dark:
        paintColor = theme.showAvatar ? Colors.black : Colors.white.withAlpha(_kCheckmarkAlpha);
        break;
    }

    ColorTween fadeTween = new ColorTween(begin: Colors.transparent, end: paintColor);

    paintColor = checkmarkAnimation.status == AnimationStatus.reverse
        ? fadeTween.evaluate(checkmarkAnimation)
        : paintColor;

    Paint paint = Paint()
      ..color = paintColor
      ..style = PaintingStyle.stroke
      ..strokeWidth = _kCheckmarkStrokeWidth * (avatar != null ? avatar.size.height / 24.0f : 1.0f);
    float t = checkmarkAnimation.status == AnimationStatus.reverse
        ? 1.0f
        : checkmarkAnimation.value;
    if (t == 0.0f) {
      return;
    }
    D.assert(t > 0.0f && t <= 1.0f);
    Path path = Path();
    Offset start = new Offset(size * 0.15f, size * 0.45f);
    Offset mid = new Offset(size * 0.4f, size * 0.7f);
    Offset end = new Offset(size * 0.85f, size * 0.25f);
    if (t < 0.5f) {
      float strokeT = t * 2.0f;
      Offset drawMid = Offset.lerp(start, mid, strokeT);
      path.moveTo(origin.dx + start.dx, origin.dy + start.dy);
      path.lineTo(origin.dx + drawMid.dx, origin.dy + drawMid.dy);
    } else {
      float strokeT = (t - 0.5f) * 2.0f;
      Offset drawEnd = Offset.lerp(mid, end, strokeT);
      path.moveTo(origin.dx + start.dx, origin.dy + start.dy);
      path.lineTo(origin.dx + mid.dx, origin.dy + mid.dy);
      path.lineTo(origin.dx + drawEnd.dx, origin.dy + drawEnd.dy);
    }
    canvas.drawPath(path, paint);
  }

  void _paintSelectionOverlay(PaintingContext context, Offset offset) {
    if (isDrawingCheckmark) {
      if (theme.showAvatar) {
        Rect avatarRect = _boxRect(avatar).shift(offset);
        Paint darkenPaint = Paint()
          ..color = selectionScrimTween.evaluate(checkmarkAnimation)
          ..blendMode = BlendMode.srcATop;
        Path path =  avatarBorder.getOuterPath(avatarRect);
        context.canvas.drawPath(path, darkenPaint);
      }
      float checkSize = avatar.size.height * 0.75f;
      Offset checkOffset = _boxParentData(avatar).offset +
          new Offset(avatar.size.height * 0.125f, avatar.size.height * 0.125f);
      _paintCheck(context.canvas, offset + checkOffset, checkSize);
    }
  }

  void _paintAvatar(PaintingContext context, Offset offset) {
    void paintWithOverlay(PaintingContext context, Offset offset) {
      context.paintChild(avatar, _boxParentData(avatar).offset + offset);
      _paintSelectionOverlay(context, offset);
    }

    if (theme.showAvatar == false && avatarDrawerAnimation.isDismissed) {
      return;
    }
    Color disabledColor = _disabledColor;
    int disabledColorAlpha = disabledColor.alpha;
    if (needsCompositing) {
      context.pushLayer(OpacityLayer(alpha: disabledColorAlpha), paintWithOverlay, offset);
    } else {
      if (disabledColorAlpha != 0xff) {
        context.canvas.saveLayer(
          _boxRect(avatar).shift(offset).inflate(20.0f),
          Paint()..color = disabledColor
        );
      }
      paintWithOverlay(context, offset);
      if (disabledColorAlpha != 0xff) {
        context.canvas.restore();
      }
    }
  }

  void _paintChild(PaintingContext context, Offset offset, RenderBox child, bool isEnabled) {
    if (child == null) {
      return;
    }
    int disabledColorAlpha = _disabledColor.alpha;
    if (!enableAnimation.isCompleted) {
      if (needsCompositing) {
        context.pushLayer(
          OpacityLayer(alpha: disabledColorAlpha),
          (PaintingContext context, Offset offset) {
            context.paintChild(child, _boxParentData(child).offset + offset);
          },
          offset
        );
      } else {
        Rect childRect = _boxRect(child).shift(offset);
        context.canvas.saveLayer(childRect.inflate(20.0f), Paint()..color = _disabledColor);
        context.paintChild(child, _boxParentData(child).offset + offset);
        context.canvas.restore();
      }
    } else {
      context.paintChild(child, _boxParentData(child).offset + offset);
    }
  }

  public override void paint(PaintingContext context, Offset offset) {
    _paintAvatar(context, offset);
    if (deleteIconShowing) {
      _paintChild(context, offset, deleteIcon, isEnabled);
    }
    _paintChild(context, offset, label, isEnabled);
  }

  const bool _debugShowTapTargetOutlines = false;

  public override void debugPaint(PaintingContext context, Offset offset) {
    D.assert(!_debugShowTapTargetOutlines ||
        () {
          Paint outlinePaint = Paint()
            ..color = const Color(0xff800000)
            ..strokeWidth = 1.0f
            ..style = PaintingStyle.stroke;
          if (deleteIconShowing) {
            context.canvas.drawRect(deleteButtonRect.shift(offset), outlinePaint);
          }
          context.canvas.drawRect(
            pressRect.shift(offset),
            outlinePaint..color = const Color(0xff008000)
          );
          return true;
        }());
  }

  public override bool hitTestSelf(Offset position) => deleteButtonRect.Contains(position) || pressRect.Contains(position);
}

}
