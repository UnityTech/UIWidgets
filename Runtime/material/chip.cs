using System;
using System.Collections.Generic;
using RSG.Promises;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
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
        bool? selected { get; }

        ValueChanged<bool> onSelected { get; }

        float? pressElevation { get; }

        Color selectedColor { get; }

        string tooltip { get; }

        ShapeBorder avatarBorder { get; }
        
        Color selectedShadowColor { get; }
    }

    public interface DisabledChipAttributes {
        bool? isEnabled { get; }

        Color disabledColor { get; }
    }

    public interface TappableChipAttributes {
        VoidCallback onPressed { get; }

        float? pressElevation { get; }

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
                avatar: this.avatar,
                label: this.label,
                labelStyle: this.labelStyle,
                labelPadding: this.labelPadding,
                deleteIcon: this.deleteIcon,
                onDeleted: this.onDeleted,
                deleteIconColor: this.deleteIconColor,
                deleteButtonTooltipMessage: this.deleteButtonTooltipMessage,
                tapEnabled: false,
                shape: this.shape,
                clipBehavior: this.clipBehavior,
                backgroundColor: this.backgroundColor,
                padding: this.padding,
                materialTapTargetSize: this.materialTapTargetSize,
                elevation: this.elevation,
                shadowColor: this.shadowColor,
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
            Key key = null,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsets labelPadding = null,
            bool selected = false,
            bool isEnabled = true,
            ValueChanged<bool> onSelected = null,
            Widget deleteIcon = null,
            VoidCallback onDeleted = null,
            Color deleteIconColor = null,
            string deleteButtonTooltipMessage = null,
            VoidCallback onPressed = null,
            float? pressElevation = null,
            Color disabledColor = null,
            Color selectedColor = null,
            string tooltip = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            Color backgroundColor = null,
            EdgeInsets padding = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null,
            Color selectedShadowColor = null,
            ShapeBorder avatarBorder = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(pressElevation == null || pressElevation >= 0.0f);
            D.assert(elevation == null || elevation >= 0.0f);
            this._avatarBorder = avatarBorder ?? new CircleBorder();
            this._avatar = avatar;
            this._label = label;
            this._labelStyle = labelStyle;
            this._labelPadding = labelPadding;
            this._selected = selected;
            this._isEnabled = isEnabled;
            this._onSelected = onSelected;
            this._deleteIcon = deleteIcon;
            this._onDeleted = onDeleted;
            this._deleteIconColor = deleteIconColor;
            this._deleteButtonTooltipMessage = deleteButtonTooltipMessage;
            this._onPressed = onPressed;
            this._pressElevation = pressElevation;
            this._disabledColor = disabledColor;
            this._selectedColor = selectedColor;
            this._tooltip = tooltip;
            this._shape = shape;
            this._clipBehavior = clipBehavior;
            this._backgroundColor = backgroundColor;
            this._padding = padding;
            this._materialTapTargetSize = materialTapTargetSize;
            this._elevation = elevation;
            this._shadowColor = shadowColor;
            this._selectedShadowColor = selectedShadowColor;
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

        public bool? selected {
            get { return this._selected; }
        }

        bool _selected;

        public bool? isEnabled {
            get { return this._isEnabled; }
        }

        bool _isEnabled;

        public ValueChanged<bool> onSelected {
            get { return this._onSelected; }
        }

        ValueChanged<bool> _onSelected;

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

        public VoidCallback onPressed {
            get { return this._onPressed; }
        }

        VoidCallback _onPressed;

        public float? pressElevation {
            get { return this._pressElevation; }
        }

        float? _pressElevation;

        public Color disabledColor {
            get { return this._disabledColor; }
        }

        Color _disabledColor;

        public Color selectedColor {
            get { return this._selectedColor; }
        }

        Color _selectedColor;

        public string tooltip {
            get { return this._tooltip; }
        }

        string _tooltip;

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

        public Color selectedShadowColor {
            get { return this._selectedShadowColor; }
        }

        Color _selectedShadowColor;
        
        public ShapeBorder avatarBorder {
            get { return this._avatarBorder; }
        }

        ShapeBorder _avatarBorder;

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            return new RawChip(
                avatar: this.avatar,
                label: this.label,
                labelStyle: this.labelStyle,
                labelPadding: this.labelPadding,
                deleteIcon: this.deleteIcon,
                onDeleted: this.onDeleted,
                deleteIconColor: this.deleteIconColor,
                deleteButtonTooltipMessage: this.deleteButtonTooltipMessage,
                onSelected: this.onSelected,
                onPressed: this.onPressed,
                pressElevation: this.pressElevation,
                selected: this.selected,
                tapEnabled: true,
                disabledColor: this.disabledColor,
                selectedColor: this.selectedColor,
                tooltip: this.tooltip,
                shape: this.shape,
                clipBehavior: this.clipBehavior,
                backgroundColor: this.backgroundColor,
                padding: this.padding,
                materialTapTargetSize: this.materialTapTargetSize,
                elevation: this.elevation,
                shadowColor: this.shadowColor,
                selectedShadowColor: this.selectedShadowColor,
                isEnabled: this.isEnabled == true &&
                           (this.onSelected != null || this.onDeleted != null || this.onPressed != null),
                avatarBorder: this.avatarBorder
            );
        }
    }

    public class ChoiceChip : StatelessWidget,
        ChipAttributes,
        SelectableChipAttributes,
        DisabledChipAttributes {
        public ChoiceChip(
            Key key,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsets labelPadding = null,
            ValueChanged<bool> onSelected = null,
            float? pressElevation = null,
            bool? selected = null,
            Color selectedColor = null,
            Color disabledColor = null,
            string tooltip = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            Color backgroundColor = null,
            EdgeInsets padding = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null,
            Color selectedShadowColor = null,
            ShapeBorder avatarBorder = null
        ) : base(key: key) {
            D.assert(selected != null);
            D.assert(label != null);
            D.assert(pressElevation == null || pressElevation >= 0.0f);
            D.assert(elevation == null || elevation >= 0.0f);
            this._avatarBorder = avatarBorder ?? new CircleBorder();
            this._avatar = avatar;
            this._label = label;
            this._labelStyle = labelStyle;
            this._labelPadding = labelPadding;
            this._onSelected = onSelected;
            this._pressElevation = pressElevation;
            this._selected = selected;
            this._selectedColor = selectedColor;
            this._disabledColor = disabledColor;
            this._tooltip = tooltip;
            this._shape = shape;
            this._clipBehavior = clipBehavior;
            this._backgroundColor = backgroundColor;
            this._padding = padding;
            this._materialTapTargetSize = materialTapTargetSize;
            this._elevation = elevation;
            this._shadowColor = shadowColor;
            this._selectedShadowColor = selectedShadowColor;
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

        public ValueChanged<bool> onSelected {
            get { return this._onSelected; }
        }

        ValueChanged<bool> _onSelected;

        public float? pressElevation {
            get { return this._pressElevation; }
        }

        float? _pressElevation;

        public bool? selected {
            get { return this._selected; }
        }

        bool? _selected;

        public Color disabledColor {
            get { return this._disabledColor; }
        }

        Color _disabledColor;

        public Color selectedColor {
            get { return this._selectedColor; }
        }

        Color _selectedColor;

        public string tooltip {
            get { return this._tooltip; }
        }

        string _tooltip;

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

        public Color selectedShadowColor {
            get { return this._selectedShadowColor; }
        }

        Color _selectedShadowColor;

        public ShapeBorder avatarBorder {
            get { return this._avatarBorder; }
        }

        ShapeBorder _avatarBorder;

        public bool? isEnabled {
            get { return this.onSelected != null; }
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            ChipThemeData chipTheme = ChipTheme.of(context);
            return new RawChip(
                avatar: this.avatar,
                label: this.label,
                labelStyle: this.labelStyle ?? (this.selected == true ? chipTheme.secondaryLabelStyle : null),
                labelPadding: this.labelPadding,
                onSelected: this.onSelected,
                pressElevation: this.pressElevation,
                selected: this.selected,
                showCheckmark: false,
                onDeleted: null,
                tooltip: this.tooltip,
                shape: this.shape,
                clipBehavior: this.clipBehavior,
                disabledColor: this.disabledColor,
                selectedColor: this.selectedColor ?? chipTheme.secondarySelectedColor,
                backgroundColor: this.backgroundColor,
                padding: this.padding,
                isEnabled: this.isEnabled,
                materialTapTargetSize: this.materialTapTargetSize,
                elevation: this.elevation,
                shadowColor: this.shadowColor,
                selectedShadowColor: this.selectedShadowColor,
                avatarBorder: this.avatarBorder
            );
        }
    }

    public class FilterChip : StatelessWidget,
        ChipAttributes,
        SelectableChipAttributes,
        DisabledChipAttributes {
        public FilterChip(
            Key key = null,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsets labelPadding = null,
            bool selected = false,
            ValueChanged<bool> onSelected = null,
            float? pressElevation = null,
            Color disabledColor = null,
            Color selectedColor = null,
            string tooltip = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            Color backgroundColor = null,
            EdgeInsets padding = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null,
            Color selectedShadowColor = null,
            ShapeBorder avatarBorder = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(pressElevation == null || pressElevation >= 0.0f);
            D.assert(elevation == null || elevation >= 0.0f);
            this._avatarBorder = avatarBorder ?? new CircleBorder();
            this._avatar = avatar;
            this._label = label;
            this._labelStyle = labelStyle;
            this._labelPadding = labelPadding;
            this._selected = selected;
            this._onSelected = onSelected;
            this._pressElevation = pressElevation;
            this._disabledColor = disabledColor;
            this._selectedColor = selectedColor;
            this._tooltip = tooltip;
            this._shape = shape;
            this._clipBehavior = clipBehavior;
            this._backgroundColor = backgroundColor;
            this._padding = padding;
            this._materialTapTargetSize = materialTapTargetSize;
            this._elevation = elevation;
            this._shadowColor = shadowColor;
            this._selectedShadowColor = selectedShadowColor;
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

        public bool? selected {
            get { return this._selected; }
        }

        bool _selected;

        public ValueChanged<bool> onSelected {
            get { return this._onSelected; }
        }

        ValueChanged<bool> _onSelected;

        public float? pressElevation {
            get { return this._pressElevation; }
        }

        float? _pressElevation;

        public Color disabledColor {
            get { return this._disabledColor; }
        }

        Color _disabledColor;

        public Color selectedColor {
            get { return this._selectedColor; }
        }

        Color _selectedColor;

        public string tooltip {
            get { return this._tooltip; }
        }

        string _tooltip;

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

        public Color selectedShadowColor {
            get { return this._selectedShadowColor; }
        }

        Color _selectedShadowColor;

        public ShapeBorder avatarBorder {
            get { return this._avatarBorder; }
        }

        ShapeBorder _avatarBorder;

        public bool? isEnabled {
            get { return this.onSelected != null; }
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            return new RawChip(
                avatar: this.avatar,
                label: this.label,
                labelStyle: this.labelStyle,
                labelPadding: this.labelPadding,
                onSelected: this.onSelected,
                pressElevation: this.pressElevation,
                selected: this.selected,
                tooltip: this.tooltip,
                shape: this.shape,
                clipBehavior: this.clipBehavior,
                backgroundColor: this.backgroundColor,
                disabledColor: this.disabledColor,
                selectedColor: this.selectedColor,
                padding: this.padding,
                isEnabled: this.isEnabled,
                materialTapTargetSize: this.materialTapTargetSize,
                elevation: this.elevation,
                shadowColor: this.shadowColor,
                selectedShadowColor: this.selectedShadowColor,
                avatarBorder: this.avatarBorder
            );
        }
    }

    public class ActionChip : StatelessWidget, ChipAttributes, TappableChipAttributes {
        public ActionChip(
            Key key = null,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsets labelPadding = null,
            VoidCallback onPressed = null,
            float? pressElevation = null,
            string tooltip = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            Color backgroundColor = null,
            EdgeInsets padding = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(
                onPressed != null,
                () => "Rather than disabling an ActionChip by setting onPressed to null, " +
                "remove it from the interface entirely."
            );
            D.assert(pressElevation == null || pressElevation >= 0.0f);
            D.assert(elevation == null || elevation >= 0.0f);
            this._avatar = avatar;
            this._label = label;
            this._labelStyle = labelStyle;
            this._labelPadding = labelPadding;
            this._onPressed = onPressed;
            this._pressElevation = pressElevation;
            this._tooltip = tooltip;
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

        public VoidCallback onPressed {
            get { return this._onPressed; }
        }

        VoidCallback _onPressed;

        public float? pressElevation {
            get { return this._pressElevation; }
        }

        float? _pressElevation;

        public string tooltip {
            get { return this._tooltip; }
        }

        string _tooltip;

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
                avatar: this.avatar,
                label: this.label,
                onPressed: this.onPressed,
                pressElevation: this.pressElevation,
                tooltip: this.tooltip,
                labelStyle: this.labelStyle,
                backgroundColor: this.backgroundColor,
                shape: this.shape,
                clipBehavior: this.clipBehavior,
                padding: this.padding,
                labelPadding: this.labelPadding,
                isEnabled: true,
                materialTapTargetSize: this.materialTapTargetSize,
                elevation: this.elevation,
                shadowColor: this._shadowColor
            );
        }
    }

    public class RawChip : StatefulWidget,
        ChipAttributes,
        DeletableChipAttributes,
        SelectableChipAttributes,
        DisabledChipAttributes,
        TappableChipAttributes {
        public RawChip(
            Key key = null,
            Widget avatar = null,
            Widget label = null,
            TextStyle labelStyle = null,
            EdgeInsets padding = null,
            EdgeInsets labelPadding = null,
            Widget deleteIcon = null,
            VoidCallback onDeleted = null,
            Color deleteIconColor = null,
            string deleteButtonTooltipMessage = null,
            VoidCallback onPressed = null,
            ValueChanged<bool> onSelected = null,
            float? pressElevation = null,
            bool? tapEnabled = true,
            bool? selected = null,
            bool showCheckmark = true,
            bool? isEnabled = true,
            Color disabledColor = null,
            Color selectedColor = null,
            string tooltip = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            Color backgroundColor = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            float? elevation = null,
            Color shadowColor = null,
            Color selectedShadowColor = null,
            ShapeBorder avatarBorder = null
        ) : base(key: key) {
            D.assert(label != null);
            D.assert(isEnabled != null);
            D.assert(pressElevation == null || pressElevation >= 0.0f);
            D.assert(elevation == null || elevation >= 0.0f);
            deleteIcon = deleteIcon ?? ChipUtils._kDefaultDeleteIcon;
            this._avatarBorder = avatarBorder ?? new CircleBorder();
            this._avatar = avatar;
            this._label = label;
            this._labelStyle = labelStyle;
            this._padding = padding;
            this._labelPadding = labelPadding;
            this._deleteIcon = deleteIcon;
            this._onDeleted = onDeleted;
            this._deleteIconColor = deleteIconColor;
            this._deleteButtonTooltipMessage = deleteButtonTooltipMessage;
            this._onPressed = onPressed;
            this._onSelected = onSelected;
            this._pressElevation = pressElevation;
            this._tapEnabled = tapEnabled;
            this._selected = selected;
            this._showCheckmark = showCheckmark;
            this._isEnabled = isEnabled;
            this._disabledColor = disabledColor;
            this._selectedColor = selectedColor;
            this._tooltip = tooltip;
            this._shape = shape;
            this._clipBehavior = clipBehavior;
            this._backgroundColor = backgroundColor;
            this._materialTapTargetSize = materialTapTargetSize;
            this._elevation = elevation;
            this._shadowColor = shadowColor;
            this._selectedShadowColor = selectedShadowColor;
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

        public ValueChanged<bool> onSelected {
            get { return this._onSelected; }
        }

        ValueChanged<bool> _onSelected;

        public VoidCallback onPressed {
            get { return this._onPressed; }
        }

        VoidCallback _onPressed;

        public float? pressElevation {
            get { return this._pressElevation; }
        }

        float? _pressElevation;

        public bool? selected {
            get { return this._selected; }
        }

        bool? _selected;

        public bool? isEnabled {
            get { return this._isEnabled; }
        }

        bool? _isEnabled;

        public Color disabledColor {
            get { return this._disabledColor; }
        }

        Color _disabledColor;

        public Color selectedColor {
            get { return this._selectedColor; }
        }

        Color _selectedColor;

        public string tooltip {
            get { return this._tooltip; }
        }

        string _tooltip;

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

        public Color selectedShadowColor {
            get { return this._selectedShadowColor; }
        }

        Color _selectedShadowColor;
        
        public ShapeBorder avatarBorder {
            get { return this._avatarBorder; }
        }

        ShapeBorder _avatarBorder;

        public bool showCheckmark {
            get { return this._showCheckmark; }
        }

        bool _showCheckmark;

        public bool? tapEnabled {
            get { return this._tapEnabled; }
        }

        bool? _tapEnabled;

        public override State createState() {
            return new _RawChipState();
        }
    }

    class _RawChipState : TickerProviderStateMixin<RawChip> {
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

        public bool hasDeleteButton {
            get { return this.widget.onDeleted != null; }
        }

        public bool hasAvatar {
            get { return this.widget.avatar != null; }
        }

        public bool canTap {
            get {
                return this.widget.isEnabled == true
                       && this.widget.tapEnabled == true
                       && (this.widget.onPressed != null || this.widget.onSelected != null);
            }
        }

        bool _isTapping = false;

        public bool isTapping {
            get { return !this.canTap ? false : this._isTapping; }
        }

        public override void initState() {
            D.assert(this.widget.onSelected == null || this.widget.onPressed == null);
            base.initState();
            this.selectController = new AnimationController(
                duration: ChipUtils._kSelectDuration,
                value: this.widget.selected == true ? 1.0f : 0.0f,
                vsync: this
            );
            this.selectionFade = new CurvedAnimation(
                parent: this.selectController,
                curve: Curves.fastOutSlowIn
            );
            this.avatarDrawerController = new AnimationController(
                duration: ChipUtils._kDrawerDuration,
                value: this.hasAvatar || this.widget.selected == true ? 1.0f : 0.0f,
                vsync: this
            );
            this.deleteDrawerController = new AnimationController(
                duration: ChipUtils._kDrawerDuration,
                value: this.hasDeleteButton ? 1.0f : 0.0f,
                vsync: this
            );
            this.enableController = new AnimationController(
                duration: ChipUtils._kDisableDuration,
                value: this.widget.isEnabled == true ? 1.0f : 0.0f,
                vsync: this
            );

            float checkmarkPercentage = (float) (ChipUtils._kCheckmarkDuration.TotalMilliseconds /
                                        ChipUtils._kSelectDuration.TotalMilliseconds);
            float checkmarkReversePercentage = (float) (ChipUtils._kCheckmarkReverseDuration.TotalMilliseconds /
                                               ChipUtils._kSelectDuration.TotalMilliseconds);
            float avatarDrawerReversePercentage = (float) (ChipUtils._kReverseDrawerDuration.TotalMilliseconds /
                                                  ChipUtils._kSelectDuration.TotalMilliseconds);
            this.checkmarkAnimation = new CurvedAnimation(
                parent: this.selectController,
                curve: new Interval(1.0f - checkmarkPercentage, 1.0f, curve: Curves.fastOutSlowIn),
                reverseCurve: new Interval(
                    1.0f - checkmarkReversePercentage,
                    1.0f,
                    curve: Curves.fastOutSlowIn
                )
            );
            this.deleteDrawerAnimation = new CurvedAnimation(
                parent: this.deleteDrawerController,
                curve: Curves.fastOutSlowIn
            );
            this.avatarDrawerAnimation = new CurvedAnimation(
                parent: this.avatarDrawerController,
                curve: Curves.fastOutSlowIn,
                reverseCurve: new Interval(
                    1.0f - avatarDrawerReversePercentage,
                    1.0f,
                    curve: Curves.fastOutSlowIn
                )
            );
            this.enableAnimation = new CurvedAnimation(
                parent: this.enableController,
                curve: Curves.fastOutSlowIn
            );
        }

        public override void dispose() {
            this.selectController.dispose();
            this.avatarDrawerController.dispose();
            this.deleteDrawerController.dispose();
            this.enableController.dispose();
            base.dispose();
        }

        void _handleTapDown(TapDownDetails details) {
            if (!this.canTap) {
                return;
            }

            this.setState(() => { this._isTapping = true; });
        }

        void _handleTapCancel() {
            if (!this.canTap) {
                return;
            }

            this.setState(() => { this._isTapping = false; });
        }

        void _handleTap() {
            if (!this.canTap) {
                return;
            }

            this.setState(() => { this._isTapping = false; });
            this.widget.onSelected?.Invoke(!this.widget.selected == true);
            this.widget.onPressed?.Invoke();
        }

        Color getBackgroundColor(ChipThemeData theme) {
            ColorTween backgroundTween = new ColorTween(
                begin: this.widget.disabledColor ?? theme.disabledColor,
                end: this.widget.backgroundColor ?? theme.backgroundColor
            );
            ColorTween selectTween = new ColorTween(
                begin: backgroundTween.evaluate(this.enableController),
                end: this.widget.selectedColor ?? theme.selectedColor
            );
            return selectTween.evaluate(this.selectionFade);
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            RawChip oldWidget = _oldWidget as RawChip;
            base.didUpdateWidget(oldWidget);
            if (oldWidget.isEnabled != this.widget.isEnabled) {
                this.setState(() => {
                    if (this.widget.isEnabled == true) {
                        this.enableController.forward();
                    }
                    else {
                        this.enableController.reverse();
                    }
                });
            }

            if (oldWidget.avatar != this.widget.avatar || oldWidget.selected != this.widget.selected) {
                this.setState(() => {
                    if (this.hasAvatar || this.widget.selected == true) {
                        this.avatarDrawerController.forward();
                    }
                    else {
                        this.avatarDrawerController.reverse();
                    }
                });
            }

            if (oldWidget.selected != this.widget.selected) {
                this.setState(() => {
                    if (this.widget.selected == true) {
                        this.selectController.forward();
                    }
                    else {
                        this.selectController.reverse();
                    }
                });
            }

            if (oldWidget.onDeleted != this.widget.onDeleted) {
                this.setState(() => {
                    if (this.hasDeleteButton) {
                        this.deleteDrawerController.forward();
                    }
                    else {
                        this.deleteDrawerController.reverse();
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
            if (!this.hasDeleteButton) {
                return null;
            }

            return this._wrapWithTooltip(
                this.widget.deleteButtonTooltipMessage ?? MaterialLocalizations.of(context)?.deleteButtonTooltip,
                this.widget.onDeleted,
                new InkResponse(
                    onTap: this.widget.isEnabled == true
                        ? () => { this.widget.onDeleted(); }
                        : (GestureTapCallback) null,
                    child: new IconTheme(
                        data: theme.iconTheme.copyWith(
                            color: this.widget.deleteIconColor ?? chipTheme.deleteIconColor
                        ),
                        child: this.widget.deleteIcon
                    )
                )
            );
        }

        const float _defaultElevation = 0.0f;
        const float _defaultPressElevation = 8.0f;
        static readonly Color _defaultShadowColor = Colors.black;

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));

            ThemeData theme = Theme.of(context);
            ChipThemeData chipTheme = ChipTheme.of(context);
            TextDirection textDirection = Directionality.of(context);
            ShapeBorder shape = this.widget.shape ?? chipTheme.shape;
            float elevation = this.widget.elevation ?? (chipTheme.elevation ?? _defaultElevation);
            float pressElevation = this.widget.pressElevation ?? (chipTheme.pressElevation ?? _defaultPressElevation);
            Color shadowColor = this.widget.shadowColor ?? chipTheme.shadowColor ?? _defaultShadowColor;
            Color selectedShadowColor = this.widget.selectedShadowColor ?? chipTheme.selectedShadowColor ?? _defaultShadowColor;
            bool selected = this.widget.selected ?? false;

            Widget result = new Material(
                elevation: this.isTapping ? pressElevation : elevation,
                shadowColor: selected ? selectedShadowColor : shadowColor,
                animationDuration: pressedAnimationDuration,
                shape: shape,
                clipBehavior: this.widget.clipBehavior,
                child: new InkWell(
                    onTap: this.canTap ? this._handleTap : (GestureTapCallback) null,
                    onTapDown: this.canTap ? this._handleTapDown : (GestureTapDownCallback) null,
                    onTapCancel: this.canTap ? this._handleTapCancel : (GestureTapCancelCallback) null,
                    customBorder: shape,
                    child: new AnimatedBuilder(
                        animation: ListenableUtils.merge(new List<Listenable>
                            {this.selectController, this.enableController}),
                        builder: (BuildContext _context, Widget child) => {
                            return new Container(
                                decoration: new ShapeDecoration(
                                    shape: shape,
                                    color: this.getBackgroundColor(chipTheme)
                                ),
                                child: child
                            );
                        },
                        child: this._wrapWithTooltip(this.widget.tooltip, this.widget.onPressed,
                            new _ChipRenderWidget(
                                theme: new _ChipRenderTheme(
                                    label: new DefaultTextStyle(
                                        overflow: TextOverflow.fade,
                                        textAlign: TextAlign.left,
                                        maxLines: 1,
                                        softWrap: false,
                                        style: this.widget.labelStyle ?? chipTheme.labelStyle,
                                        child: this.widget.label
                                    ),
                                    avatar: new AnimatedSwitcher(
                                        child: this.widget.avatar,
                                        duration: ChipUtils._kDrawerDuration,
                                        switchInCurve: Curves.fastOutSlowIn
                                    ),
                                    deleteIcon: new AnimatedSwitcher(
                                        child: this._buildDeleteIcon(context, theme, chipTheme),
                                        duration: ChipUtils._kDrawerDuration,
                                        switchInCurve: Curves.fastOutSlowIn
                                    ),
                                    brightness: chipTheme.brightness,
                                    padding: this.widget.padding ?? chipTheme.padding,
                                    labelPadding: this.widget.labelPadding ?? chipTheme.labelPadding,
                                    showAvatar: this.hasAvatar,
                                    showCheckmark: this.widget.showCheckmark,
                                    canTapBody: this.canTap
                                ),
                                value: this.widget.selected,
                                checkmarkAnimation: this.checkmarkAnimation,
                                enableAnimation: this.enableAnimation,
                                avatarDrawerAnimation: this.avatarDrawerAnimation,
                                deleteDrawerAnimation: this.deleteDrawerAnimation,
                                isEnabled: this.widget.isEnabled,
                                avatarBorder: this.widget.avatarBorder
                            )
                        )
                    )
                )
            );
            BoxConstraints constraints;
            switch (this.widget.materialTapTargetSize ?? theme.materialTapTargetSize) {
                case MaterialTapTargetSize.padded:
                    constraints = new BoxConstraints(minHeight: 48.0f);
                    break;
                case MaterialTapTargetSize.shrinkWrap:
                    constraints = new BoxConstraints();
                    break;
                default:
                    throw new Exception("Unknown Material Tap Target Size: " + this.widget.materialTapTargetSize);
            }

            result = new _ChipRedirectingHitDetectionWidget(
                constraints: constraints,
                child: new Center(
                    child: result,
                    widthFactor: 1.0f,
                    heightFactor: 1.0f
                )
            );
            return result;
        }
    }

    class _ChipRedirectingHitDetectionWidget : SingleChildRenderObjectWidget {
        public _ChipRedirectingHitDetectionWidget(
            Key key = null,
            Widget child = null,
            BoxConstraints constraints = null
        ) : base(key: key, child: child) {
            this.constraints = constraints;
        }

        public readonly BoxConstraints constraints;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderChipRedirectingHitDetection(this.constraints);
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderChipRedirectingHitDetection renderObject = _renderObject as _RenderChipRedirectingHitDetection;
            renderObject.additionalConstraints = this.constraints;
        }
    }

    class _RenderChipRedirectingHitDetection : RenderConstrainedBox {
        public _RenderChipRedirectingHitDetection(BoxConstraints additionalConstraints) : base(
            additionalConstraints: additionalConstraints) {
        }

        public override bool hitTest(HitTestResult result, Offset position = null) {
            if (!this.size.contains(position)) {
                return false;
            }

            return this.child.hitTest(result, position: new Offset(position.dx, this.size.height / 2));
        }
    }

    class _ChipRenderWidget : RenderObjectWidget {
        public _ChipRenderWidget(
            Key key = null,
            _ChipRenderTheme theme = null,
            bool? value = null,
            bool? isEnabled = null,
            Animation<float> checkmarkAnimation = null,
            Animation<float> avatarDrawerAnimation = null,
            Animation<float> deleteDrawerAnimation = null,
            Animation<float> enableAnimation = null,
            ShapeBorder avatarBorder = null
        ) : base(key: key) {
            D.assert(theme != null);
            this.theme = theme;
            this.value = value;
            this.isEnabled = isEnabled;
            this.checkmarkAnimation = checkmarkAnimation;
            this.avatarDrawerAnimation = avatarDrawerAnimation;
            this.deleteDrawerAnimation = deleteDrawerAnimation;
            this.enableAnimation = enableAnimation;
            this.avatarBorder = avatarBorder;
        }

        public readonly _ChipRenderTheme theme;
        public readonly bool? value;
        public readonly bool? isEnabled;
        public readonly Animation<float> checkmarkAnimation;
        public readonly Animation<float> avatarDrawerAnimation;
        public readonly Animation<float> deleteDrawerAnimation;
        public readonly Animation<float> enableAnimation;
        public readonly ShapeBorder avatarBorder;

        public override Element createElement() {
            return new _RenderChipElement(this);
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderChip renderObject = _renderObject as _RenderChip;
            renderObject.theme = this.theme;
            renderObject.value = this.value ?? false;
            renderObject.isEnabled = this.isEnabled ?? false;
            renderObject.checkmarkAnimation = this.checkmarkAnimation;
            renderObject.avatarDrawerAnimation = this.avatarDrawerAnimation;
            renderObject.deleteDrawerAnimation = this.deleteDrawerAnimation;
            renderObject.enableAnimation = this.enableAnimation;
            renderObject.avatarBorder = this.avatarBorder;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderChip(
                theme: this.theme,
                value: this.value,
                isEnabled: this.isEnabled,
                checkmarkAnimation: this.checkmarkAnimation,
                avatarDrawerAnimation: this.avatarDrawerAnimation,
                deleteDrawerAnimation: this.deleteDrawerAnimation,
                enableAnimation: this.enableAnimation,
                avatarBorder: this.avatarBorder
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

        Dictionary<_ChipSlot, Element> slotToChild = new Dictionary<_ChipSlot, Element> { };
        Dictionary<Element, _ChipSlot> childToSlot = new Dictionary<Element, _ChipSlot> { };

        public new _ChipRenderWidget widget {
            get { return (_ChipRenderWidget) base.widget; }
        }

        public new _RenderChip renderObject {
            get { return (_RenderChip) base.renderObject; }
        }

        public override void visitChildren(ElementVisitor visitor) {
            this.slotToChild.Values.Each((value) => { visitor(value); });
        }

        protected override void forgetChild(Element child) {
            D.assert(this.slotToChild.ContainsValue(child));
            D.assert(this.childToSlot.ContainsKey(child));
            _ChipSlot slot = this.childToSlot[child];
            this.childToSlot.Remove(child);
            this.slotToChild.Remove(slot);
        }

        void _mountChild(Widget widget, _ChipSlot slot) {
            Element oldChild = this.slotToChild.getOrDefault(slot);
            Element newChild = this.updateChild(oldChild, widget, slot);
            if (oldChild != null) {
                this.slotToChild.Remove(slot);
                this.childToSlot.Remove(oldChild);
            }

            if (newChild != null) {
                this.slotToChild[slot] = newChild;
                this.childToSlot[newChild] = slot;
            }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            this._mountChild(this.widget.theme.avatar, _ChipSlot.avatar);
            this._mountChild(this.widget.theme.deleteIcon, _ChipSlot.deleteIcon);
            this._mountChild(this.widget.theme.label, _ChipSlot.label);
        }

        void _updateChild(Widget widget, _ChipSlot slot) {
            Element oldChild = this.slotToChild[slot];
            Element newChild = this.updateChild(oldChild, widget, slot);
            if (oldChild != null) {
                this.childToSlot.Remove(oldChild);
                this.slotToChild.Remove(slot);
            }

            if (newChild != null) {
                this.slotToChild[slot] = newChild;
                this.childToSlot[newChild] = slot;
            }
        }

        public override void update(Widget _newWidget) {
            _ChipRenderWidget newWidget = _newWidget as _ChipRenderWidget;
            base.update(newWidget);
            D.assert(this.widget == newWidget);
            this._updateChild(this.widget.theme.label, _ChipSlot.label);
            this._updateChild(this.widget.theme.avatar, _ChipSlot.avatar);
            this._updateChild(this.widget.theme.deleteIcon, _ChipSlot.deleteIcon);
        }

        void _updateRenderObject(RenderObject child, _ChipSlot slot) {
            switch (slot) {
                case _ChipSlot.avatar:
                    this.renderObject.avatar = (RenderBox) child;
                    break;
                case _ChipSlot.label:
                    this.renderObject.label = (RenderBox) child;
                    break;
                case _ChipSlot.deleteIcon:
                    this.renderObject.deleteIcon = (RenderBox) child;
                    break;
            }
        }

        protected override void insertChildRenderObject(RenderObject child, object slotValue) {
            D.assert(child is RenderBox);
            D.assert(slotValue is _ChipSlot);
            _ChipSlot slot = (_ChipSlot) slotValue;
            this._updateRenderObject(child, slot);
            D.assert(this.renderObject.childToSlot.ContainsKey((RenderBox) child));
            D.assert(this.renderObject.slotToChild.ContainsKey(slot));
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child is RenderBox);
            D.assert(this.renderObject.childToSlot.ContainsKey((RenderBox) child));
            this._updateRenderObject(null, this.renderObject.childToSlot[(RenderBox) child]);
            D.assert(!this.renderObject.childToSlot.ContainsKey((RenderBox) child));
            D.assert(!this.renderObject.slotToChild.ContainsKey((_ChipSlot) this.slot));
        }

        protected override void moveChildRenderObject(RenderObject child, object slotValue) {
            D.assert(false, () => "not reachable");
        }
    }

    class _ChipRenderTheme {
        public _ChipRenderTheme(
            Widget avatar = null,
            Widget label = null,
            Widget deleteIcon = null,
            Brightness? brightness = null,
            EdgeInsets padding = null,
            EdgeInsets labelPadding = null,
            bool? showAvatar = null,
            bool? showCheckmark = null,
            bool? canTapBody = null
        ) {
            this.avatar = avatar;
            this.label = label;
            this.deleteIcon = deleteIcon;
            this.brightness = brightness;
            this.padding = padding;
            this.labelPadding = labelPadding;
            this.showAvatar = showAvatar;
            this.showCheckmark = showCheckmark;
            this.canTapBody = canTapBody;
        }

        public readonly Widget avatar;
        public readonly Widget label;
        public readonly Widget deleteIcon;
        public readonly Brightness? brightness;
        public readonly EdgeInsets padding;
        public readonly EdgeInsets labelPadding;
        public readonly bool? showAvatar;
        public readonly bool? showCheckmark;
        public readonly bool? canTapBody;

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

            return this.Equals((_ChipRenderTheme) obj);
        }

        public bool Equals(_ChipRenderTheme other) {
            return this.avatar == other.avatar
                   && this.label == other.label
                   && this.deleteIcon == other.deleteIcon
                   && this.brightness == other.brightness
                   && this.padding == other.padding
                   && this.labelPadding == other.labelPadding
                   && this.showAvatar == other.showAvatar
                   && this.showCheckmark == other.showCheckmark
                   && this.canTapBody == other.canTapBody;
        }

        public static bool operator ==(_ChipRenderTheme left, _ChipRenderTheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(_ChipRenderTheme left, _ChipRenderTheme right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            var hashCode = this.avatar.GetHashCode();
            hashCode = (hashCode * 397) ^ this.label.GetHashCode();
            hashCode = (hashCode * 397) ^ this.deleteIcon.GetHashCode();
            hashCode = (hashCode * 397) ^ this.brightness.GetHashCode();
            hashCode = (hashCode * 397) ^ this.padding.GetHashCode();
            hashCode = (hashCode * 397) ^ this.labelPadding.GetHashCode();
            hashCode = (hashCode * 397) ^ this.showAvatar.GetHashCode();
            hashCode = (hashCode * 397) ^ this.showCheckmark.GetHashCode();
            hashCode = (hashCode * 397) ^ this.canTapBody.GetHashCode();
            return hashCode;
        }
    }

    class _RenderChip : RenderBox {
        public _RenderChip(
            _ChipRenderTheme theme = null,
            bool? value = null,
            bool? isEnabled = null,
            Animation<float> checkmarkAnimation = null,
            Animation<float> avatarDrawerAnimation = null,
            Animation<float> deleteDrawerAnimation = null,
            Animation<float> enableAnimation = null,
            ShapeBorder avatarBorder = null
        ) {
            D.assert(theme != null);
            this._theme = theme;
            checkmarkAnimation.addListener(this.markNeedsPaint);
            avatarDrawerAnimation.addListener(this.markNeedsLayout);
            deleteDrawerAnimation.addListener(this.markNeedsLayout);
            enableAnimation.addListener(this.markNeedsPaint);
            this.value = value;
            this.isEnabled = isEnabled;
            this.checkmarkAnimation = checkmarkAnimation;
            this.avatarDrawerAnimation = avatarDrawerAnimation;
            this.deleteDrawerAnimation = deleteDrawerAnimation;
            this.enableAnimation = enableAnimation;
            this.avatarBorder = avatarBorder;
        }

        public Dictionary<_ChipSlot, RenderBox> slotToChild = new Dictionary<_ChipSlot, RenderBox> { };
        public Dictionary<RenderBox, _ChipSlot> childToSlot = new Dictionary<RenderBox, _ChipSlot> { };

        public bool? value;
        public bool? isEnabled;
        public Rect deleteButtonRect;
        public Rect pressRect;
        public Animation<float> checkmarkAnimation;
        public Animation<float> avatarDrawerAnimation;
        public Animation<float> deleteDrawerAnimation;
        public Animation<float> enableAnimation;
        public ShapeBorder avatarBorder;

        RenderBox _updateChild(RenderBox oldChild, RenderBox newChild, _ChipSlot slot) {
            if (oldChild != null) {
                this.dropChild(oldChild);
                this.childToSlot.Remove(oldChild);
                this.slotToChild.Remove(slot);
            }

            if (newChild != null) {
                this.childToSlot[newChild] = slot;
                this.slotToChild[slot] = newChild;
                this.adoptChild(newChild);
            }

            return newChild;
        }

        RenderBox _avatar;

        public RenderBox avatar {
            get { return this._avatar; }
            set { this._avatar = this._updateChild(this._avatar, value, _ChipSlot.avatar); }
        }

        RenderBox _deleteIcon;

        public RenderBox deleteIcon {
            get { return this._deleteIcon; }
            set { this._deleteIcon = this._updateChild(this._deleteIcon, value, _ChipSlot.deleteIcon); }
        }

        RenderBox _label;

        public RenderBox label {
            get { return this._label; }
            set { this._label = this._updateChild(this._label, value, _ChipSlot.label); }
        }

        public _ChipRenderTheme theme {
            get { return this._theme; }
            set {
                if (this._theme == value) {
                    return;
                }

                this._theme = value;
                this.markNeedsLayout();
            }
        }

        _ChipRenderTheme _theme;

        IEnumerable<RenderBox> _children {
            get {
                if (this.avatar != null) {
                    yield return this.avatar;
                }

                if (this.label != null) {
                    yield return this.label;
                }

                if (this.deleteIcon != null) {
                    yield return this.deleteIcon;
                }
            }
        }

        public bool isDrawingCheckmark {
            get { return this.theme.showCheckmark == true && (!(this.checkmarkAnimation?.isDismissed ?? !this.value)) == true; }
        }

        public bool deleteIconShowing {
            get { return !this.deleteDrawerAnimation.isDismissed; }
        }

        public override void attach(object _owner) {
            PipelineOwner owner = _owner as PipelineOwner;
            base.attach(owner);
            foreach (RenderBox child in this._children) {
                child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            foreach (RenderBox child in this._children) {
                child.detach();
            }
        }

        public override void redepthChildren() {
            this._children.Each(this.redepthChild);
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            this._children.Each((value) => { visitor(value); });
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            List<DiagnosticsNode> value = new List<DiagnosticsNode> { };

            void add(RenderBox child, string name) {
                if (child != null) {
                    value.Add(child.toDiagnosticsNode(name: name));
                }
            }

            add(this.avatar, "avatar");
            add(this.label, "label");
            add(this.deleteIcon, "deleteIcon");
            return value;
        }

        protected override bool sizedByParent {
            get { return false; }
        }

        static float _minWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMinIntrinsicWidth(height);
        }

        static float _maxWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMaxIntrinsicWidth(height);
        }

        static float _minHeight(RenderBox box, float width) {
            return box == null ? 0.0f : box.getMinIntrinsicHeight(width);
        }

        static Size _boxSize(RenderBox box) {
            return box == null ? Size.zero : box.size;
        }

        static Rect _boxRect(RenderBox box) {
            return box == null ? Rect.zero : _boxParentData(box).offset & box.size;
        }

        static BoxParentData _boxParentData(RenderBox box) {
            return (BoxParentData) box.parentData;
        }

        protected override float computeMinIntrinsicWidth(float height) {
            float overallPadding = this.theme.padding.horizontal + this.theme.labelPadding.horizontal;
            return overallPadding +
                   _minWidth(this.avatar, height) +
                   _minWidth(this.label, height) +
                   _minWidth(this.deleteIcon, height);
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            float overallPadding = this.theme.padding.vertical + this.theme.labelPadding.horizontal;
            return overallPadding +
                   _maxWidth(this.avatar, height) +
                   _maxWidth(this.label, height) +
                   _maxWidth(this.deleteIcon, height);
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return Mathf.Max(
                ChipUtils._kChipHeight,
                this.theme.padding.vertical + this.theme.labelPadding.vertical + _minHeight(this.label, width)
            );
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return this.computeMinIntrinsicHeight(width);
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return this.label.getDistanceToActualBaseline(baseline);
        }

        Size _layoutLabel(float iconSizes, Size size) {
            Size rawSize = _boxSize(this.label);
            if (this.constraints.maxWidth.isFinite()) {
                this.label.layout(this.constraints.copyWith(
                        minWidth: 0.0f,
                        maxWidth: Mathf.Max(
                            0.0f, this.constraints.maxWidth - iconSizes - this.theme.labelPadding.horizontal
                        ),
                        minHeight: rawSize.height,
                        maxHeight: size.height
                    ),
                    parentUsesSize: true
                );
            }
            else {
                this.label.layout(
                    new BoxConstraints(
                        minHeight: rawSize.height,
                        maxHeight: size.height,
                        minWidth: 0.0f,
                        maxWidth: size.width
                    ),
                    parentUsesSize: true
                );
            }

            return new Size(
                rawSize.width + this.theme.labelPadding.horizontal,
                rawSize.height + this.theme.labelPadding.vertical
            );
        }

        Size _layoutAvatar(BoxConstraints contentConstraints, float contentSize) {
            float requestedSize = Mathf.Max(0.0f, contentSize);
            BoxConstraints avatarConstraints = BoxConstraints.tightFor(
                width: requestedSize,
                height: requestedSize
            );
            this.avatar.layout(avatarConstraints, parentUsesSize: true);
            if (this.theme.showCheckmark != true && this.theme.showAvatar != true) {
                return new Size(0.0f, contentSize);
            }

            float avatarWidth = 0.0f;
            float avatarHeight = 0.0f;
            Size avatarBoxSize = _boxSize(this.avatar);
            if (this.theme.showAvatar == true) {
                avatarWidth += this.avatarDrawerAnimation.value * avatarBoxSize.width;
            }
            else {
                avatarWidth += this.avatarDrawerAnimation.value * contentSize;
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
            this.deleteIcon.layout(deleteIconConstraints, parentUsesSize: true);
            if (!this.deleteIconShowing) {
                return new Size(0.0f, contentSize);
            }

            float deleteIconWidth = 0.0f;
            float deleteIconHeight = 0.0f;
            Size boxSize = _boxSize(this.deleteIcon);
            deleteIconWidth += this.deleteDrawerAnimation.value * boxSize.width;
            deleteIconHeight += boxSize.height;
            return new Size(deleteIconWidth, deleteIconHeight);
        }

        public override bool hitTest(HitTestResult result, Offset position = null) {
            if (!this.size.contains(position)) {
                return false;
            }

            RenderBox hitTestChild;
            if (position.dx / this.size.width > 0.66f) {
                hitTestChild = this.deleteIcon ?? this.label ?? this.avatar;
            }
            else {
                hitTestChild = this.label ?? this.avatar;
            }

            return hitTestChild?.hitTest(result, position: hitTestChild.size.center(Offset.zero)) ?? false;
        }

        protected override void performLayout() {
            BoxConstraints contentConstraints = this.constraints.loosen();
            this.label.layout(contentConstraints, parentUsesSize: true);
            float contentSize = Mathf.Max(
                ChipUtils._kChipHeight - this.theme.padding.vertical + this.theme.labelPadding.vertical,
                _boxSize(this.label).height + this.theme.labelPadding.vertical
            );
            Size avatarSize = this._layoutAvatar(contentConstraints, contentSize);
            Size deleteIconSize = this._layoutDeleteIcon(contentConstraints, contentSize);
            Size labelSize = new Size(_boxSize(this.label).width, contentSize);
            labelSize = this._layoutLabel(avatarSize.width + deleteIconSize.width, labelSize);

            Size overallSize = new Size(
                avatarSize.width + labelSize.width + deleteIconSize.width,
                contentSize
            );


            const float left = 0.0f;

            Offset centerLayout(Size boxSize, float x) {
                D.assert(contentSize >= boxSize.height);
                return new Offset(x, (contentSize - boxSize.height) / 2.0f);
            }

            Offset avatarOffset = Offset.zero;
            Offset labelOffset = Offset.zero;
            Offset deleteIconOffset = Offset.zero;
            float start = left;
            if (this.theme.showCheckmark == true || this.theme.showAvatar == true) {
                avatarOffset = centerLayout(avatarSize, start - _boxSize(this.avatar).width + avatarSize.width);
                start += avatarSize.width;
            }

            labelOffset = centerLayout(labelSize, start);
            start += labelSize.width;
            if (this.theme.canTapBody == true) {
                this.pressRect = Rect.fromLTWH(
                    0.0f,
                    0.0f, this.deleteIconShowing
                        ? start + this.theme.padding.left
                        : overallSize.width + this.theme.padding.horizontal,
                    overallSize.height + this.theme.padding.vertical
                );
            }
            else {
                this.pressRect = Rect.zero;
            }

            start -= _boxSize(this.deleteIcon).width - deleteIconSize.width;
            if (this.deleteIconShowing) {
                deleteIconOffset = centerLayout(deleteIconSize, start);
                this.deleteButtonRect = Rect.fromLTWH(
                    start + this.theme.padding.left,
                    0.0f,
                    deleteIconSize.width + this.theme.padding.right,
                    overallSize.height + this.theme.padding.vertical
                );
            }
            else {
                this.deleteButtonRect = Rect.zero;
            }

            labelOffset = labelOffset +
                          new Offset(
                              0.0f,
                              ((labelSize.height - this.theme.labelPadding.vertical) - _boxSize(this.label).height) /
                              2.0f
                          );
            _boxParentData(this.avatar).offset = this.theme.padding.topLeft + avatarOffset;
            _boxParentData(this.label).offset =
                this.theme.padding.topLeft + labelOffset + this.theme.labelPadding.topLeft;
            _boxParentData(this.deleteIcon).offset = this.theme.padding.topLeft + deleteIconOffset;
            Size paddedSize = new Size(
                overallSize.width + this.theme.padding.horizontal,
                overallSize.height + this.theme.padding.vertical
            );
            this.size = this.constraints.constrain(paddedSize);
            D.assert(this.size.height == this.constraints.constrainHeight(paddedSize.height),
                () => $"Constrained height {this.size.height} doesn't match expected height " +
                $"{this.constraints.constrainWidth(paddedSize.height)}");
            D.assert(this.size.width == this.constraints.constrainWidth(paddedSize.width),
                () => $"Constrained width {this.size.width} doesn't match expected width " +
                $"{this.constraints.constrainWidth(paddedSize.width)}");
        }

        static ColorTween selectionScrimTween = new ColorTween(
            begin: Colors.transparent,
            end: ChipUtils._kSelectScrimColor
        );

        Color _disabledColor {
            get {
                if (this.enableAnimation == null || this.enableAnimation.isCompleted) {
                    return Colors.white;
                }

                ColorTween enableTween;
                switch (this.theme.brightness) {
                    case Brightness.light:
                        enableTween = new ColorTween(
                            begin: Colors.white.withAlpha(ChipUtils._kDisabledAlpha),
                            end: Colors.white
                        );
                        break;
                    case Brightness.dark:
                        enableTween = new ColorTween(
                            begin: Colors.black.withAlpha(ChipUtils._kDisabledAlpha),
                            end: Colors.black
                        );
                        break;
                    default:
                        throw new Exception("Unknown brightness: " + this.theme.brightness);
                }

                return enableTween.evaluate(this.enableAnimation);
            }
        }

        void _paintCheck(Canvas canvas, Offset origin, float size) {
            Color paintColor;
            switch (this.theme.brightness) {
                case Brightness.light:
                    paintColor = this.theme.showAvatar == true
                        ? Colors.white
                        : Colors.black.withAlpha(ChipUtils._kCheckmarkAlpha);
                    break;
                case Brightness.dark:
                    paintColor = this.theme.showAvatar == true
                        ? Colors.black
                        : Colors.white.withAlpha(ChipUtils._kCheckmarkAlpha);
                    break;
                default:
                    throw new Exception("Unknown brightness: " + this.theme.brightness);
            }

            ColorTween fadeTween = new ColorTween(begin: Colors.transparent, end: paintColor);

            paintColor = this.checkmarkAnimation.status == AnimationStatus.reverse
                ? fadeTween.evaluate(this.checkmarkAnimation)
                : paintColor;

            Paint paint = new Paint();
            paint.color = paintColor;
            paint.style = PaintingStyle.stroke;
            paint.strokeWidth = ChipUtils._kCheckmarkStrokeWidth *
                                (this.avatar != null ? this.avatar.size.height / 24.0f : 1.0f);
            float t = this.checkmarkAnimation.status == AnimationStatus.reverse
                ? 1.0f
                : this.checkmarkAnimation.value;
            if (t == 0.0f) {
                return;
            }

            D.assert(t > 0.0f && t <= 1.0f);
            Path path = new Path();
            Offset start = new Offset(size * 0.15f, size * 0.45f);
            Offset mid = new Offset(size * 0.4f, size * 0.7f);
            Offset end = new Offset(size * 0.85f, size * 0.25f);
            if (t < 0.5f) {
                float strokeT = t * 2.0f;
                Offset drawMid = Offset.lerp(start, mid, strokeT);
                path.moveTo(origin.dx + start.dx, origin.dy + start.dy);
                path.lineTo(origin.dx + drawMid.dx, origin.dy + drawMid.dy);
            }
            else {
                float strokeT = (t - 0.5f) * 2.0f;
                Offset drawEnd = Offset.lerp(mid, end, strokeT);
                path.moveTo(origin.dx + start.dx, origin.dy + start.dy);
                path.lineTo(origin.dx + mid.dx, origin.dy + mid.dy);
                path.lineTo(origin.dx + drawEnd.dx, origin.dy + drawEnd.dy);
            }

            canvas.drawPath(path, paint);
        }

        void _paintSelectionOverlay(PaintingContext context, Offset offset) {
            if (this.isDrawingCheckmark) {
                if (this.theme.showAvatar == true) {
                    Rect avatarRect = _boxRect(this.avatar).shift(offset);
                    Paint darkenPaint = new Paint();
                    darkenPaint.color = selectionScrimTween.evaluate(this.checkmarkAnimation);
                    darkenPaint.blendMode = BlendMode.srcATop;
                    Path path = this.avatarBorder.getOuterPath(avatarRect);
                    context.canvas.drawPath(path, darkenPaint);
                }

                float checkSize = this.avatar.size.height * 0.75f;
                Offset checkOffset = _boxParentData(this.avatar).offset +
                                     new Offset(this.avatar.size.height * 0.125f, this.avatar.size.height * 0.125f);
                this._paintCheck(context.canvas, offset + checkOffset, checkSize);
            }
        }

        void _paintAvatar(PaintingContext context, Offset offset) {
            void paintWithOverlay(PaintingContext _context, Offset _offset) {
                _context.paintChild(this.avatar, _boxParentData(this.avatar).offset + _offset);
                this._paintSelectionOverlay(_context, _offset);
            }

            if (this.theme.showAvatar == false && this.avatarDrawerAnimation.isDismissed) {
                return;
            }

            Color disabledColor = this._disabledColor;
            int disabledColorAlpha = disabledColor.alpha;
            if (this.needsCompositing) {
                context.pushLayer(new OpacityLayer(alpha: disabledColorAlpha), paintWithOverlay, offset);
            }
            else {
                Paint _paint = new Paint();
                _paint.color = this._disabledColor;
                if (disabledColorAlpha != 0xff) {
                    context.canvas.saveLayer(_boxRect(this.avatar).shift(offset).inflate(20.0f), _paint);
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

            int disabledColorAlpha = this._disabledColor.alpha;
            if (!this.enableAnimation.isCompleted) {
                if (this.needsCompositing) {
                    context.pushLayer(
                        new OpacityLayer(alpha: disabledColorAlpha),
                        (PaintingContext _context, Offset _offset) => {
                            _context.paintChild(child, _boxParentData(child).offset + _offset);
                        },
                        offset
                    );
                }
                else {
                    Rect childRect = _boxRect(child).shift(offset);
                    Paint _paint = new Paint();
                    _paint.color = this._disabledColor;
                    context.canvas.saveLayer(childRect.inflate(20.0f), _paint);
                    context.paintChild(child, _boxParentData(child).offset + offset);
                    context.canvas.restore();
                }
            }
            else {
                context.paintChild(child, _boxParentData(child).offset + offset);
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            this._paintAvatar(context, offset);
            if (this.deleteIconShowing) {
                this._paintChild(context, offset, this.deleteIcon, this.isEnabled == true);
            }

            this._paintChild(context, offset, this.label, this.isEnabled == true);
        }

        const bool _debugShowTapTargetOutlines = false;

        public override void debugPaint(PaintingContext context, Offset offset) {
            bool visualizeTapTargets() {
                Paint outlinePaint = new Paint();
                outlinePaint.color = new Color(0xff800000);
                outlinePaint.strokeWidth = 1.0f;
                outlinePaint.style = PaintingStyle.stroke;
                if (this.deleteIconShowing) {
                    context.canvas.drawRect(this.deleteButtonRect.shift(offset), outlinePaint);
                }

                outlinePaint.color = new Color(0xff008000);
                context.canvas.drawRect(this.pressRect.shift(offset), outlinePaint);
                return true;
            }

            D.assert(!_debugShowTapTargetOutlines || visualizeTapTargets());
        }

        protected override bool hitTestSelf(Offset position) {
            return this.deleteButtonRect.contains(position) || this.pressRect.contains(position);
        }
    }
}
