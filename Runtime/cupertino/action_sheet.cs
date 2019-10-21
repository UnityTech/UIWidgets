using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    class CupertinoActionSheetUtils {
        public static readonly TextStyle _kActionSheetActionStyle = new TextStyle(
            // fontFamily: ".SF UI Text",
            fontFamily: ".SF Pro Text",
            inherit: false,
            fontSize: 20.0f,
            fontWeight: FontWeight.w400,
            color: CupertinoColors.activeBlue,
            textBaseline: TextBaseline.alphabetic
        );

        public static readonly TextStyle _kActionSheetContentStyle = new TextStyle(
            // fontFamily: ".SF UI Text",
            fontFamily: ".SF Pro Text",
            inherit: false,
            fontSize: 13.0f,
            fontWeight: FontWeight.w400,
            color: _kContentTextColor,
            textBaseline: TextBaseline.alphabetic
        );

        public static readonly BoxDecoration _kAlertBlurOverlayDecoration = new BoxDecoration(
            color: CupertinoColors.white,
            backgroundBlendMode: BlendMode.overlay
        );

        public static readonly Color _kBackgroundColor = new Color(0xD1F8F8F8);
        public static readonly Color _kPressedColor = new Color(0xA6E5E5EA);
        public static readonly Color _kButtonDividerColor = new Color(0x403F3F3F);
        public static readonly Color _kContentTextColor = new Color(0xFF8F8F8F);
        public static readonly Color _kCancelButtonPressedColor = new Color(0xFFEAEAEA);

        public const float _kBlurAmount = 20.0f;
        public const float _kEdgeHorizontalPadding = 8.0f;
        public const float _kCancelButtonPadding = 8.0f;
        public const float _kEdgeVerticalPadding = 10.0f;
        public const float _kContentHorizontalPadding = 40.0f;
        public const float _kContentVerticalPadding = 14.0f;
        public const float _kButtonHeight = 56.0f;
        public const float _kCornerRadius = 14.0f;
        public const float _kDividerThickness = 1.0f;
    }

    public class CupertinoActionSheet : StatelessWidget {
        public CupertinoActionSheet(
            Key key = null,
            Widget title = null,
            Widget message = null,
            List<Widget> actions = null,
            ScrollController messageScrollController = null,
            ScrollController actionScrollController = null,
            Widget cancelButton = null
        ) : base(key: key) {
            D.assert(actions != null || title != null || message != null || cancelButton != null,
                () =>
                    "An action sheet must have a non-null value for at least one of the following arguments: actions, title, message, or cancelButton");
            this.title = title;
            this.message = message;
            this.actions = actions ?? new List<Widget>();
            this.messageScrollController = messageScrollController;
            this.actionScrollController = actionScrollController;
            this.cancelButton = cancelButton;
        }

        public readonly Widget title;
        public readonly Widget message;
        public readonly List<Widget> actions;
        public readonly ScrollController messageScrollController;
        public readonly ScrollController actionScrollController;
        public readonly Widget cancelButton;

        Widget _buildContent() {
            List<Widget> content = new List<Widget>();
            if (this.title != null || this.message != null) {
                Widget titleSection = new _CupertinoAlertContentSection(
                    title: this.title,
                    message: this.message,
                    scrollController: this.messageScrollController
                );
                content.Add(new Flexible(child: titleSection));
            }

            return new Container(
                color: CupertinoActionSheetUtils._kBackgroundColor,
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: content
                )
            );
        }

        Widget _buildActions() {
            if (this.actions == null || this.actions.isEmpty()) {
                return new Container(height: 0.0f);
            }

            return new Container(
                child: new _CupertinoAlertActionSection(
                    children: this.actions,
                    scrollController: this.actionScrollController,
                    hasCancelButton: this.cancelButton != null
                )
            );
        }

        Widget _buildCancelButton() {
            float cancelPadding = (this.actions != null || this.message != null || this.title != null)
                ? CupertinoActionSheetUtils._kCancelButtonPadding
                : 0.0f;
            return new Padding(
                padding: EdgeInsets.only(top: cancelPadding),
                child: new _CupertinoActionSheetCancelButton(
                    child: this.cancelButton
                )
            );
        }

        public override Widget build(BuildContext context) {
            List<Widget> children = new List<Widget> {
                new Flexible(child: new ClipRRect(
                        borderRadius: BorderRadius.circular(12.0f),
                        child: new BackdropFilter(
                            filter: ImageFilter.blur(sigmaX: CupertinoActionSheetUtils._kBlurAmount,
                                sigmaY: CupertinoActionSheetUtils._kBlurAmount),
                            child: new Container(
                                decoration: CupertinoActionSheetUtils._kAlertBlurOverlayDecoration,
                                child: new _CupertinoAlertRenderWidget(
                                    contentSection: this._buildContent(),
                                    actionsSection: this._buildActions()
                                )
                            )
                        )
                    )
                ),
            };

            if (this.cancelButton != null) {
                children.Add(this._buildCancelButton()
                );
            }

            Orientation orientation = MediaQuery.of(context).orientation;

            float actionSheetWidth;
            if (orientation == Orientation.portrait) {
                actionSheetWidth = MediaQuery.of(context).size.width -
                                   (CupertinoActionSheetUtils._kEdgeHorizontalPadding * 2);
            }
            else {
                actionSheetWidth = MediaQuery.of(context).size.height -
                                   (CupertinoActionSheetUtils._kEdgeHorizontalPadding * 2);
            }

            return new SafeArea(
                child: new Container(
                    width: actionSheetWidth,
                    margin: EdgeInsets.symmetric(
                        horizontal: CupertinoActionSheetUtils._kEdgeHorizontalPadding,
                        vertical: CupertinoActionSheetUtils._kEdgeVerticalPadding
                    ),
                    child: new Column(
                        children: children,
                        mainAxisSize: MainAxisSize.min,
                        crossAxisAlignment: CrossAxisAlignment.stretch
                    )
                )
            );
        }
    }


    public class CupertinoActionSheetAction : StatelessWidget {
        public CupertinoActionSheetAction(
            Widget child,
            VoidCallback onPressed,
            bool isDefaultAction = false,
            bool isDestructiveAction = false
        ) {
            D.assert(child != null);
            D.assert(onPressed != null);
            this.child = child;
            this.onPressed = onPressed;
            this.isDefaultAction = isDefaultAction;
            this.isDestructiveAction = isDestructiveAction;
        }

        public readonly VoidCallback onPressed;
        public readonly bool isDefaultAction;
        public readonly bool isDestructiveAction;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            TextStyle style = CupertinoActionSheetUtils._kActionSheetActionStyle;

            if (this.isDefaultAction) {
                style = style.copyWith(fontWeight: FontWeight.w600);
            }

            if (this.isDestructiveAction) {
                style = style.copyWith(color: CupertinoColors.destructiveRed);
            }

            return new GestureDetector(
                onTap: () => this.onPressed(),
                behavior: HitTestBehavior.opaque,
                child: new ConstrainedBox(
                    constraints: new BoxConstraints(
                        minHeight: CupertinoActionSheetUtils._kButtonHeight
                    ),
                    child: new Container(
                        alignment: Alignment.center,
                        padding: EdgeInsets.symmetric(
                            vertical: 16.0f,
                            horizontal: 10.0f
                        ),
                        child: new DefaultTextStyle(
                            style: style,
                            child: this.child,
                            textAlign: TextAlign.center
                        )
                    )
                )
            );
        }
    }

    class _CupertinoActionSheetCancelButton : StatefulWidget {
        public _CupertinoActionSheetCancelButton(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _CupertinoActionSheetCancelButtonState();
        }
    }

    class _CupertinoActionSheetCancelButtonState : State<_CupertinoActionSheetCancelButton> {
        Color _backgroundColor;

        public override void initState() {
            this._backgroundColor = CupertinoColors.white;
            base.initState();
        }

        void _onTapDown(TapDownDetails evt) {
            this.setState(() => { this._backgroundColor = CupertinoActionSheetUtils._kCancelButtonPressedColor; });
        }

        void _onTapUp(TapUpDetails evt) {
            this.setState(() => { this._backgroundColor = CupertinoColors.white; });
        }

        void _onTapCancel() {
            this.setState(() => { this._backgroundColor = CupertinoColors.white; });
        }

        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onTapDown: this._onTapDown,
                onTapUp: this._onTapUp,
                onTapCancel: this._onTapCancel,
                child: new Container(
                    decoration: new BoxDecoration(
                        color: this._backgroundColor,
                        borderRadius: BorderRadius.circular(CupertinoActionSheetUtils._kCornerRadius)
                    ),
                    child: this.widget.child
                )
            );
        }
    }

    class _CupertinoAlertRenderWidget : RenderObjectWidget {
        public _CupertinoAlertRenderWidget(
            Widget contentSection,
            Widget actionsSection,
            Key key = null
        ) : base(key: key) {
            this.contentSection = contentSection;
            this.actionsSection = actionsSection;
        }

        public readonly Widget contentSection;
        public readonly Widget actionsSection;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoAlert(
                dividerThickness: CupertinoActionSheetUtils._kDividerThickness /
                                  MediaQuery.of(context).devicePixelRatio
            );
        }

        public override Element createElement() {
            return new _CupertinoAlertRenderElement(this);
        }
    }

    class _CupertinoAlertRenderElement : RenderObjectElement {
        public _CupertinoAlertRenderElement(_CupertinoAlertRenderWidget widget) : base(widget) { }

        Element _contentElement;
        Element _actionsElement;

        public new _CupertinoAlertRenderWidget widget {
            get { return base.widget as _CupertinoAlertRenderWidget; }
        }

        public new _RenderCupertinoAlert renderObject {
            get { return base.renderObject as _RenderCupertinoAlert; }
        }

        public override void visitChildren(ElementVisitor visitor) {
            if (this._contentElement != null) {
                visitor(this._contentElement);
            }

            if (this._actionsElement != null) {
                visitor(this._actionsElement);
            }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            this._contentElement = this.updateChild(this._contentElement, this.widget.contentSection,
                _AlertSections.contentSection);
            this._actionsElement = this.updateChild(this._actionsElement, this.widget.actionsSection,
                _AlertSections.actionsSection);
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            this._placeChildInSlot(child, slot);
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            this._placeChildInSlot(child, slot);
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            this._contentElement = this.updateChild(this._contentElement, this.widget.contentSection,
                _AlertSections.contentSection);
            this._actionsElement = this.updateChild(this._actionsElement, this.widget.actionsSection,
                _AlertSections.actionsSection);
        }

        protected override void forgetChild(Element child) {
            D.assert(child == this._contentElement || child == this._actionsElement);
            if (this._contentElement == child) {
                this._contentElement = null;
            }
            else if (this._actionsElement == child) {
                this._actionsElement = null;
            }
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child == this.renderObject.contentSection || child == this.renderObject.actionsSection);
            if (this.renderObject.contentSection == child) {
                this.renderObject.contentSection = null;
            }
            else if (this.renderObject.actionsSection == child) {
                this.renderObject.actionsSection = null;
            }
        }

        void _placeChildInSlot(RenderObject child, object slot) {
            switch ((_AlertSections) slot) {
                case _AlertSections.contentSection:
                    this.renderObject.contentSection = child as RenderBox;
                    break;
                case _AlertSections.actionsSection:
                    this.renderObject.actionsSection = child as RenderBox;
                    ;
                    break;
            }
        }
    }


    class _RenderCupertinoAlert : RenderBox {
        public _RenderCupertinoAlert(
            RenderBox contentSection = null,
            RenderBox actionsSection = null,
            float dividerThickness = 0.0f
        ) {
            this._contentSection = contentSection;
            this._actionsSection = actionsSection;
            this._dividerThickness = dividerThickness;
        }

        public RenderBox contentSection {
            get { return this._contentSection; }
            set {
                if (value != this._contentSection) {
                    if (null != this._contentSection) {
                        this.dropChild(this._contentSection);
                    }

                    this._contentSection = value;
                    if (null != this._contentSection) {
                        this.adoptChild(this._contentSection);
                    }
                }
            }
        }

        RenderBox _contentSection;


        public RenderBox actionsSection {
            get { return this._actionsSection; }
            set {
                if (value != this._actionsSection) {
                    if (null != this._actionsSection) {
                        this.dropChild(this._actionsSection);
                    }

                    this._actionsSection = value;
                    if (null != this._actionsSection) {
                        this.adoptChild(this._actionsSection);
                    }
                }
            }
        }

        RenderBox _actionsSection;

        readonly float _dividerThickness;

        readonly Paint _dividerPaint = new Paint() {
            color = CupertinoActionSheetUtils._kButtonDividerColor,
            style = PaintingStyle.fill
        };

        public override void attach(object owner) {
            base.attach(owner);
            if (null != this.contentSection) {
                this.contentSection.attach(owner);
            }

            if (null != this.actionsSection) {
                this.actionsSection.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            if (null != this.contentSection) {
                this.contentSection.detach();
            }

            if (null != this.actionsSection) {
                this.actionsSection.detach();
            }
        }

        public override void redepthChildren() {
            if (null != this.contentSection) {
                this.redepthChild(this.contentSection);
            }

            if (null != this.actionsSection) {
                this.redepthChild(this.actionsSection);
            }
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is MultiChildLayoutParentData)) {
                child.parentData = new MultiChildLayoutParentData();
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            if (this.contentSection != null) {
                visitor(this.contentSection);
            }

            if (this.actionsSection != null) {
                visitor(this.actionsSection);
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            List<DiagnosticsNode> value = new List<DiagnosticsNode>();
            if (this.contentSection != null) {
                value.Add(this.contentSection.toDiagnosticsNode(name: "content"));
            }

            if (this.actionsSection != null) {
                value.Add(this.actionsSection.toDiagnosticsNode(name: "actions"));
            }

            return value;
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return this.constraints.minWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return this.constraints.maxWidth;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            float contentHeight = this.contentSection.getMinIntrinsicHeight(width);
            float actionsHeight = this.actionsSection.getMinIntrinsicHeight(width);
            bool hasDivider = contentHeight > 0.0f && actionsHeight > 0.0f;
            float height = contentHeight + (hasDivider ? this._dividerThickness : 0.0f) + actionsHeight;

            if (actionsHeight > 0 || contentHeight > 0) {
                height -= 2 * CupertinoActionSheetUtils._kEdgeVerticalPadding;
            }

            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float contentHeight = this.contentSection.getMaxIntrinsicHeight(width);
            float actionsHeight = this.actionsSection.getMaxIntrinsicHeight(width);
            bool hasDivider = contentHeight > 0.0f && actionsHeight > 0.0f;
            float height = contentHeight + (hasDivider ? this._dividerThickness : 0.0f) + actionsHeight;

            if (actionsHeight > 0 || contentHeight > 0) {
                height -= 2 * CupertinoActionSheetUtils._kEdgeVerticalPadding;
            }

            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected override void performLayout() {
            bool hasDivider = this.contentSection.getMaxIntrinsicHeight(this.constraints.maxWidth) > 0.0f
                              && this.actionsSection.getMaxIntrinsicHeight(this.constraints.maxWidth) > 0.0f;
            float dividerThickness = hasDivider ? this._dividerThickness : 0.0f;

            float minActionsHeight = this.actionsSection.getMinIntrinsicHeight(this.constraints.maxWidth);


            this.contentSection.layout(
                this.constraints.deflate(EdgeInsets.only(bottom: minActionsHeight + dividerThickness)),
                parentUsesSize: true
            );
            Size contentSize = this.contentSection.size;


            this.actionsSection.layout(
                this.constraints.deflate(EdgeInsets.only(top: contentSize.height + dividerThickness)),
                parentUsesSize: true
            );
            Size actionsSize = this.actionsSection.size;


            float actionSheetHeight = contentSize.height + dividerThickness + actionsSize.height;


            this.size = new Size(this.constraints.maxWidth, actionSheetHeight);


            D.assert(this.actionsSection.parentData is MultiChildLayoutParentData);
            MultiChildLayoutParentData actionParentData = this.actionsSection.parentData as MultiChildLayoutParentData;
            actionParentData.offset = new Offset(0.0f, contentSize.height + dividerThickness);
        }

        public override void paint(PaintingContext context, Offset offset) {
            MultiChildLayoutParentData contentParentData = this.contentSection.parentData as MultiChildLayoutParentData;
            this.contentSection.paint(context, offset + contentParentData.offset);

            bool hasDivider = this.contentSection.size.height > 0.0f && this.actionsSection.size.height > 0.0f;
            if (hasDivider) {
                this._paintDividerBetweenContentAndActions(context.canvas, offset);
            }

            MultiChildLayoutParentData actionsParentData = this.actionsSection.parentData as MultiChildLayoutParentData;
            this.actionsSection.paint(context, offset + actionsParentData.offset);
        }

        void _paintDividerBetweenContentAndActions(Canvas canvas, Offset offset) {
            canvas.drawRect(
                Rect.fromLTWH(
                    offset.dx,
                    offset.dy + this.contentSection.size.height, this.size.width, this._dividerThickness
                ), this._dividerPaint
            );
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            bool isHit = false;
            MultiChildLayoutParentData contentSectionParentData =
                this.contentSection.parentData as MultiChildLayoutParentData;
            MultiChildLayoutParentData actionsSectionParentData =
                this.actionsSection.parentData as MultiChildLayoutParentData;
            ;
            if (this.contentSection.hitTest(result, position: position - contentSectionParentData.offset)) {
                isHit = true;
            }
            else if (this.actionsSection.hitTest(result,
                position: position - actionsSectionParentData.offset)) {
                isHit = true;
            }

            return isHit;
        }
    }


    enum _AlertSections {
        contentSection,
        actionsSection,
    }


    class _CupertinoAlertContentSection : StatelessWidget {
        public _CupertinoAlertContentSection(
            Key key = null,
            Widget title = null,
            Widget message = null,
            ScrollController scrollController = null
        ) : base(key: key) {
            this.title = title;
            this.message = message;
            this.scrollController = scrollController;
        }

        public readonly Widget title;
        public readonly Widget message;
        public readonly ScrollController scrollController;

        public override Widget build(BuildContext context) {
            List<Widget> titleContentGroup = new List<Widget>();
            if (this.title != null) {
                titleContentGroup.Add(new Padding(
                    padding: EdgeInsets.only(
                        left: CupertinoActionSheetUtils._kContentHorizontalPadding,
                        right: CupertinoActionSheetUtils._kContentHorizontalPadding,
                        bottom: CupertinoActionSheetUtils._kContentVerticalPadding,
                        top: CupertinoActionSheetUtils._kContentVerticalPadding
                    ),
                    child: new DefaultTextStyle(
                        style: this.message == null
                            ? CupertinoActionSheetUtils._kActionSheetContentStyle
                            : CupertinoActionSheetUtils._kActionSheetContentStyle.copyWith(fontWeight: FontWeight.w600),
                        textAlign: TextAlign.center,
                        child: this.title
                    )
                ));
            }

            if (this.message != null) {
                titleContentGroup.Add(
                    new Padding(
                        padding: EdgeInsets.only(
                            left: CupertinoActionSheetUtils._kContentHorizontalPadding,
                            right: CupertinoActionSheetUtils._kContentHorizontalPadding,
                            bottom: this.title == null ? CupertinoActionSheetUtils._kContentVerticalPadding : 22.0f,
                            top: this.title == null ? CupertinoActionSheetUtils._kContentVerticalPadding : 0.0f
                        ),
                        child: new DefaultTextStyle(
                            style: this.title == null
                                ? CupertinoActionSheetUtils._kActionSheetContentStyle.copyWith(
                                    fontWeight: FontWeight.w600)
                                : CupertinoActionSheetUtils._kActionSheetContentStyle,
                            textAlign: TextAlign.center,
                            child: this.message
                        )
                    )
                );
            }

            if (titleContentGroup.isEmpty()) {
                return new SingleChildScrollView(
                    controller: this.scrollController,
                    child: new Container(
                        width: 0.0f,
                        height: 0.0f
                    )
                );
            }


            if (titleContentGroup.Count > 1) {
                titleContentGroup.Insert(1, new Padding(padding: EdgeInsets.only(top: 8.0f)));
            }

            return new CupertinoScrollbar(
                child: new SingleChildScrollView(
                    controller: this.scrollController,
                    child: new Column(
                        mainAxisSize: MainAxisSize.max,
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: titleContentGroup
                    )
                )
            );
        }
    }


    class _CupertinoAlertActionSection : StatefulWidget {
        public _CupertinoAlertActionSection(
            List<Widget> children,
            Key key = null,
            ScrollController scrollController = null,
            bool hasCancelButton = false
        ) : base(key: key) {
            D.assert(children != null);
            this.children = children;
            this.scrollController = scrollController;
            this.hasCancelButton = hasCancelButton;
        }

        public readonly List<Widget> children;
        public readonly ScrollController scrollController;
        public readonly bool hasCancelButton;

        public override State createState() {
            return new _CupertinoAlertActionSectionState();
        }
    }

    class _CupertinoAlertActionSectionState : State<_CupertinoAlertActionSection> {
        public override Widget build(BuildContext context) {
            float devicePixelRatio = MediaQuery.of(context).devicePixelRatio;

            List<Widget> interactiveButtons = new List<Widget>();
            for (int i = 0; i < this.widget.children.Count; i += 1) {
                interactiveButtons.Add(new _PressableActionSheetActionButton(
                        child: this.widget.children[i]
                    )
                );
            }

            return new CupertinoScrollbar(
                child: new SingleChildScrollView(
                    controller: this.widget.scrollController,
                    child: new _CupertinoAlertActionsRenderWidget(
                        actionButtons: interactiveButtons,
                        dividerThickness: CupertinoActionSheetUtils._kDividerThickness / devicePixelRatio,
                        hasCancelButton: this.widget.hasCancelButton
                    )
                )
            );
        }
    }

    class _PressableActionSheetActionButton : StatefulWidget {
        public _PressableActionSheetActionButton(
            Widget child
        ) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _PressableActionSheetActionButtonState();
        }
    }

    class _PressableActionSheetActionButtonState : State<_PressableActionSheetActionButton> {
        bool _isPressed = false;

        public override Widget build(BuildContext context) {
            return new _ActionSheetActionButtonParentDataWidget(
                isPressed: this._isPressed,
                child: new GestureDetector(
                    behavior: HitTestBehavior.opaque,
                    onTapDown: (TapDownDetails details) => this.setState(() => { this._isPressed = true; }),
                    onTapUp: (TapUpDetails details) => this.setState(() => { this._isPressed = false; }),
                    onTapCancel: () => this.setState(() => this._isPressed = false),
                    child: this.widget.child
                )
            );
        }
    }

    class _ActionSheetActionButtonParentDataWidget : ParentDataWidget<_CupertinoAlertActionsRenderWidget> {
        public _ActionSheetActionButtonParentDataWidget(
            Widget child,
            bool isPressed = false,
            Key key = null
        ) : base(key: key, child: child) {
            this.isPressed = isPressed;
        }

        public readonly bool isPressed;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is _ActionSheetActionButtonParentData);
            _ActionSheetActionButtonParentData parentData =
                renderObject.parentData as _ActionSheetActionButtonParentData;
            if (parentData.isPressed != this.isPressed) {
                parentData.isPressed = this.isPressed;
                AbstractNodeMixinDiagnosticableTree targetParent = renderObject.parent;
                if (targetParent is RenderObject) {
                    ((RenderObject) targetParent).markNeedsPaint();
                }
            }
        }
    }

    class _ActionSheetActionButtonParentData : MultiChildLayoutParentData {
        public _ActionSheetActionButtonParentData(
            bool isPressed = false
        ) {
            this.isPressed = isPressed;
        }

        public bool isPressed;
    }

    class _CupertinoAlertActionsRenderWidget : MultiChildRenderObjectWidget {
        public _CupertinoAlertActionsRenderWidget(
            List<Widget> actionButtons,
            Key key = null,
            float dividerThickness = 0.0f,
            bool hasCancelButton = false
        ) : base(key: key, children: actionButtons) {
            this._dividerThickness = dividerThickness;
            this._hasCancelButton = hasCancelButton;
        }

        readonly float _dividerThickness;
        readonly bool _hasCancelButton;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoAlertActions(
                dividerThickness: this._dividerThickness,
                hasCancelButton: this._hasCancelButton
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((_RenderCupertinoAlertActions) renderObject).dividerThickness = this._dividerThickness;
            ((_RenderCupertinoAlertActions) renderObject).hasCancelButton = this._hasCancelButton;
        }
    }

    class _RenderCupertinoAlertActions : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox,
        MultiChildLayoutParentData> {
        public _RenderCupertinoAlertActions(
            List<RenderBox> children = null,
            float dividerThickness = 0.0f,
            bool hasCancelButton = false
        ) {
            this._dividerThickness = dividerThickness;
            this._hasCancelButton = hasCancelButton;
            this.addAll(children ?? new List<RenderBox>());
        }

        public float dividerThickness {
            get { return this._dividerThickness; }
            set {
                if (value == this._dividerThickness) {
                    return;
                }

                this._dividerThickness = value;
                this.markNeedsLayout();
            }
        }

        float _dividerThickness;

        bool _hasCancelButton;

        public bool hasCancelButton {
            get { return this._hasCancelButton; }
            set {
                if (value == this._hasCancelButton) {
                    return;
                }

                this._hasCancelButton = value;
                this.markNeedsLayout();
            }
        }


        readonly Paint _buttonBackgroundPaint = new Paint() {
            color = CupertinoActionSheetUtils._kBackgroundColor,
            style = PaintingStyle.fill
        };

        readonly Paint _pressedButtonBackgroundPaint = new Paint() {
            color = CupertinoActionSheetUtils._kPressedColor,
            style = PaintingStyle.fill
        };

        readonly Paint _dividerPaint = new Paint() {
            color = CupertinoActionSheetUtils._kButtonDividerColor,
            style = PaintingStyle.fill
        };

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is _ActionSheetActionButtonParentData)) {
                child.parentData = new _ActionSheetActionButtonParentData();
            }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return this.constraints.minWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return this.constraints.maxWidth;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (this.childCount == 0) {
                return 0.0f;
            }

            if (this.childCount == 1) {
                return this.firstChild.computeMaxIntrinsicHeight(width) + this.dividerThickness;
            }

            if (this.hasCancelButton && this.childCount < 4) {
                return this._computeMinIntrinsicHeightWithCancel(width);
            }

            return this._computeMinIntrinsicHeightWithoutCancel(width);
        }

        float _computeMinIntrinsicHeightWithCancel(float width) {
            D.assert(this.childCount == 2 || this.childCount == 3);
            if (this.childCount == 2) {
                return this.firstChild.getMinIntrinsicHeight(width)
                       + this.childAfter(this.firstChild).getMinIntrinsicHeight(width)
                       + this.dividerThickness;
            }

            return this.firstChild.getMinIntrinsicHeight(width)
                   + this.childAfter(this.firstChild).getMinIntrinsicHeight(width)
                   + this.childAfter(this.childAfter(this.firstChild)).getMinIntrinsicHeight(width)
                   + (this.dividerThickness * 2);
        }

        float _computeMinIntrinsicHeightWithoutCancel(float width) {
            D.assert(this.childCount >= 2);
            return this.firstChild.getMinIntrinsicHeight(width)
                   + this.dividerThickness
                   + (0.5f * this.childAfter(this.firstChild).getMinIntrinsicHeight(width));
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (this.childCount == 0) {
                return 0.0f;
            }

            if (this.childCount == 1) {
                return this.firstChild.computeMaxIntrinsicHeight(width) + this.dividerThickness;
            }

            return this._computeMaxIntrinsicHeightStacked(width);
        }

        float _computeMaxIntrinsicHeightStacked(float width) {
            D.assert(this.childCount >= 2);
            float allDividersHeight = (this.childCount - 1) * this.dividerThickness;
            float heightAccumulation = allDividersHeight;
            RenderBox button = this.firstChild;
            while (button != null) {
                heightAccumulation += button.getMaxIntrinsicHeight(width);
                button = this.childAfter(button);
            }

            return heightAccumulation;
        }

        protected override void performLayout() {
            BoxConstraints perButtonConstraints = this.constraints.copyWith(
                minHeight: 0.0f,
                maxHeight: float.PositiveInfinity
            );
            RenderBox child = this.firstChild;
            int index = 0;
            float verticalOffset = 0.0f;
            while (child != null) {
                child.layout(
                    perButtonConstraints,
                    parentUsesSize: true
                );
                D.assert(child.parentData is MultiChildLayoutParentData);
                MultiChildLayoutParentData parentData = child.parentData as MultiChildLayoutParentData;
                parentData.offset = new Offset(0.0f, verticalOffset);
                verticalOffset += child.size.height;
                if (index < this.childCount - 1) {
                    verticalOffset += this.dividerThickness;
                }

                index += 1;
                child = this.childAfter(child);
            }

            this.size = this.constraints.constrain(
                new Size(this.constraints.maxWidth, verticalOffset)
            );
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;
            this._drawButtonBackgroundsAndDividersStacked(canvas, offset);
            this._drawButtons(context, offset);
        }

        void _drawButtonBackgroundsAndDividersStacked(Canvas canvas, Offset offset) {
            Offset dividerOffset = new Offset(0.0f, this.dividerThickness);
            Path backgroundFillPath = new Path();
            // fillType = PathFillType.evenOdd
            backgroundFillPath.addRect(Rect.fromLTWH(0.0f, 0.0f, this.size.width, this.size.height));

            Path pressedBackgroundFillPath = new Path();
            Path dividersPath = new Path();
            Offset accumulatingOffset = offset;
            RenderBox child = this.firstChild;
            RenderBox prevChild = null;
            while (child != null) {
                D.assert(child.parentData is _ActionSheetActionButtonParentData);
                _ActionSheetActionButtonParentData currentButtonParentData =
                    child.parentData as _ActionSheetActionButtonParentData;
                bool isButtonPressed = currentButtonParentData.isPressed;
                bool isPrevButtonPressed = false;
                if (prevChild != null) {
                    D.assert(prevChild.parentData is _ActionSheetActionButtonParentData);
                    _ActionSheetActionButtonParentData previousButtonParentData =
                        prevChild.parentData as _ActionSheetActionButtonParentData;
                    isPrevButtonPressed = previousButtonParentData.isPressed;
                }

                bool isDividerPresent = child != this.firstChild;
                bool isDividerPainted = isDividerPresent && !(isButtonPressed || isPrevButtonPressed);
                Rect dividerRect = Rect.fromLTWH(
                    accumulatingOffset.dx,
                    accumulatingOffset.dy, this.size.width, this._dividerThickness
                );
                Rect buttonBackgroundRect = Rect.fromLTWH(
                    accumulatingOffset.dx,
                    accumulatingOffset.dy + (isDividerPresent ? this.dividerThickness : 0.0f), this.size.width,
                    child.size.height
                );
                if (isButtonPressed) {
                    backgroundFillPath.addRect(buttonBackgroundRect);
                    pressedBackgroundFillPath.addRect(buttonBackgroundRect);
                }

                if (isDividerPainted) {
                    backgroundFillPath.addRect(dividerRect);
                    dividersPath.addRect(dividerRect);
                }

                accumulatingOffset += (isDividerPresent ? dividerOffset : Offset.zero)
                                      + new Offset(0.0f, child.size.height);
                prevChild = child;
                child = this.childAfter(child);
            }

            canvas.drawPath(backgroundFillPath, this._buttonBackgroundPaint);
            canvas.drawPath(pressedBackgroundFillPath, this._pressedButtonBackgroundPaint);
            canvas.drawPath(dividersPath, this._dividerPaint);
        }

        void _drawButtons(PaintingContext context, Offset offset) {
            RenderBox child = this.firstChild;
            while (child != null) {
                MultiChildLayoutParentData childParentData = child.parentData as MultiChildLayoutParentData;
                context.paintChild(child, childParentData.offset + offset);
                child = this.childAfter(child);
            }
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            return this.defaultHitTestChildren(result, position: position);
        }
    }
}