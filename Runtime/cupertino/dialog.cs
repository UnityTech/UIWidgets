using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    class CupertinoDialogUtils {
        public static readonly TextStyle _kCupertinoDialogTitleStyle = new TextStyle(
            fontFamily: ".SF UI Display",
            fontSize: 18.0f,
            fontWeight: FontWeight.w600,
            color: CupertinoColors.black,
            letterSpacing: 0.48f,
            textBaseline: TextBaseline.alphabetic
        );

        public static readonly TextStyle _kCupertinoDialogContentStyle = new TextStyle(
            fontFamily: ".SF UI Text",
            fontSize: 13.4f,
            fontWeight: FontWeight.w400,
            color: CupertinoColors.black,
            height: 1.036f,
            letterSpacing: -0.25f,
            textBaseline: TextBaseline.alphabetic
        );

        public static readonly TextStyle _kCupertinoDialogActionStyle = new TextStyle(
            fontFamily: ".SF UI Text",
            fontSize: 16.8f,
            fontWeight: FontWeight.w400,
            color: CupertinoColors.activeBlue,
            textBaseline: TextBaseline.alphabetic
        );

        public const float _kCupertinoDialogWidth = 270.0f;
        public const float _kAccessibilityCupertinoDialogWidth = 310.0f;

        public static readonly BoxDecoration _kCupertinoDialogBlurOverlayDecoration = new BoxDecoration(
            color: CupertinoColors.white,
            backgroundBlendMode: BlendMode.overlay
        );

        public const float _kBlurAmount = 20.0f;
        public const float _kEdgePadding = 20.0f;
        public const float _kMinButtonHeight = 45.0f;
        public const float _kMinButtonFontSize = 10.0f;
        public const float _kDialogCornerRadius = 12.0f;
        public const float _kDividerThickness = 1.0f;
        public const float _kMaxRegularTextScaleFactor = 1.4f;

        public static readonly Color _kDialogColor = new Color(0xC0FFFFFF);
        public static readonly Color _kDialogPressedColor = new Color(0x90FFFFFF);
        public static readonly Color _kButtonDividerColor = new Color(0x40FFFFFF);

        public static bool _isInAccessibilityMode(BuildContext context) {
            MediaQueryData data = MediaQuery.of(context, nullOk: true);
            return data != null && data.textScaleFactor > _kMaxRegularTextScaleFactor;
        }
    }


    public class CupertinoAlertDialog : StatelessWidget {
        public CupertinoAlertDialog(
            Key key = null,
            Widget title = null,
            Widget content = null,
            List<Widget> actions = null,
            ScrollController scrollController = null,
            ScrollController actionScrollController = null
        ) : base(key: key) {
            D.assert(actions != null);

            this.title = title;
            this.content = content;
            this.actions = actions ?? new List<Widget>();
            this.scrollController = scrollController;
            this.actionScrollController = actionScrollController;
        }

        public readonly Widget title;
        public readonly Widget content;
        public readonly List<Widget> actions;
        public readonly ScrollController scrollController;
        public readonly ScrollController actionScrollController;

        Widget _buildContent() {
            List<Widget> children = new List<Widget>();
            if (this.title != null || this.content != null) {
                Widget titleSection = new _CupertinoDialogAlertContentSection(
                    title: this.title,
                    content: this.content,
                    scrollController: this.scrollController
                );
                children.Add(new Flexible(flex: 3, child: titleSection));
            }

            return new Container(
                color: CupertinoDialogUtils._kDialogColor,
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: children
                )
            );
        }

        Widget _buildActions() {
            Widget actionSection = new Container(
                height: 0.0f
            );
            if (this.actions.isNotEmpty()) {
                actionSection = new _CupertinoDialogAlertActionSection(
                    children: this.actions,
                    scrollController: this.actionScrollController
                );
            }

            return actionSection;
        }

        public override Widget build(BuildContext context) {
            CupertinoLocalizations localizations = CupertinoLocalizations.of(context);
            bool isInAccessibilityMode = CupertinoDialogUtils._isInAccessibilityMode(context);

            float textScaleFactor = MediaQuery.of(context).textScaleFactor;
            return new MediaQuery(
                data: MediaQuery.of(context).copyWith(
                    textScaleFactor: Mathf.Max(textScaleFactor, 1.0f)
                ),
                child: new LayoutBuilder(
                    builder: (BuildContext _context, BoxConstraints constraints) => {
                        return new Center(
                            child: new Container(
                                margin: EdgeInsets.symmetric(vertical: CupertinoDialogUtils._kEdgePadding),
                                width: isInAccessibilityMode
                                    ? CupertinoDialogUtils._kAccessibilityCupertinoDialogWidth
                                    : CupertinoDialogUtils._kCupertinoDialogWidth,
                                child: new CupertinoPopupSurface(
                                    isSurfacePainted: false,
                                    child: new _CupertinoDialogRenderWidget(
                                        contentSection: this._buildContent(),
                                        actionsSection: this._buildActions()
                                    )
                                )
                            )
                        );
                    }
                )
            );
        }
    }

    public class CupertinoDialog : StatelessWidget {
        public CupertinoDialog(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new Center(
                child: new SizedBox(
                    width: CupertinoDialogUtils._kCupertinoDialogWidth,
                    child: new CupertinoPopupSurface(
                        child: this.child
                    )
                )
            );
        }
    }

    public class CupertinoPopupSurface : StatelessWidget {
        public CupertinoPopupSurface(
            Key key = null,
            bool isSurfacePainted = true,
            Widget child = null
        ) : base(key: key) {
            this.isSurfacePainted = isSurfacePainted;
            this.child = child;
        }

        public readonly bool isSurfacePainted;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new ClipRRect(
                borderRadius: BorderRadius.circular(CupertinoDialogUtils._kDialogCornerRadius),
                child: new BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: CupertinoDialogUtils._kBlurAmount,
                        sigmaY: CupertinoDialogUtils._kBlurAmount),
                    child: new Container(
                        decoration: CupertinoDialogUtils._kCupertinoDialogBlurOverlayDecoration,
                        child: new Container(
                            color: this.isSurfacePainted ? CupertinoDialogUtils._kDialogColor : null,
                            child: this.child
                        )
                    )
                )
            );
        }
    }

    class _CupertinoDialogRenderWidget : RenderObjectWidget {
        public _CupertinoDialogRenderWidget(
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
            return new _RenderCupertinoDialog(
                dividerThickness: CupertinoDialogUtils._kDividerThickness / MediaQuery.of(context).devicePixelRatio,
                isInAccessibilityMode: CupertinoDialogUtils._isInAccessibilityMode(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((_RenderCupertinoDialog) renderObject).isInAccessibilityMode =
                CupertinoDialogUtils._isInAccessibilityMode(context);
        }

        public override Element createElement() {
            return new _CupertinoDialogRenderElement(this);
        }
    }

    class _CupertinoDialogRenderElement : RenderObjectElement {
        public _CupertinoDialogRenderElement(_CupertinoDialogRenderWidget widget) : base(widget) {
        }

        Element _contentElement;
        Element _actionsElement;

        public new _CupertinoDialogRenderWidget widget {
            get { return base.widget as _CupertinoDialogRenderWidget; }
        }

        public new _RenderCupertinoDialog renderObject {
            get { return base.renderObject as _RenderCupertinoDialog; }
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
                _AlertDialogSections.contentSection);
            this._actionsElement = this.updateChild(this._actionsElement, this.widget.actionsSection,
                _AlertDialogSections.actionsSection);
        }


        protected override void insertChildRenderObject(RenderObject child, object slot) {
            D.assert(slot != null);
            switch (slot) {
                case _AlertDialogSections.contentSection:
                    this.renderObject.contentSection = child as RenderBox;
                    break;
                case _AlertDialogSections.actionsSection:
                    this.renderObject.actionsSection = child as RenderBox;
                    ;
                    break;
            }
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            this._contentElement = this.updateChild(this._contentElement, this.widget.contentSection,
                _AlertDialogSections.contentSection);
            this._actionsElement = this.updateChild(this._actionsElement, this.widget.actionsSection,
                _AlertDialogSections.actionsSection);
        }

        protected override void forgetChild(Element child) {
            D.assert(child == this._contentElement || child == this._actionsElement);
            if (this._contentElement == child) {
                this._contentElement = null;
            }
            else {
                D.assert(this._actionsElement == child);
                this._actionsElement = null;
            }
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child == this.renderObject.contentSection || child == this.renderObject.actionsSection);
            if (this.renderObject.contentSection == child) {
                this.renderObject.contentSection = null;
            }
            else {
                D.assert(this.renderObject.actionsSection == child);
                this.renderObject.actionsSection = null;
            }
        }
    }

    class _RenderCupertinoDialog : RenderBox {
        public _RenderCupertinoDialog(
            RenderBox contentSection = null,
            RenderBox actionsSection = null,
            float dividerThickness = 0.0f,
            bool isInAccessibilityMode = false
        ) {
            this._contentSection = contentSection;
            this._actionsSection = actionsSection;
            this._dividerThickness = dividerThickness;
            this._isInAccessibilityMode = isInAccessibilityMode;
        }

        public RenderBox contentSection {
            get { return this._contentSection; }
            set {
                if (value != this._contentSection) {
                    if (this._contentSection != null) {
                        this.dropChild(this._contentSection);
                    }

                    this._contentSection = value;
                    if (this._contentSection != null) {
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

        public bool isInAccessibilityMode {
            get { return this._isInAccessibilityMode; }
            set {
                if (value != this._isInAccessibilityMode) {
                    this._isInAccessibilityMode = value;
                    this.markNeedsLayout();
                }
            }
        }

        bool _isInAccessibilityMode;

        float _dialogWidth {
            get {
                return this.isInAccessibilityMode
                    ? CupertinoDialogUtils._kAccessibilityCupertinoDialogWidth
                    : CupertinoDialogUtils._kCupertinoDialogWidth;
            }
        }

        readonly float _dividerThickness;

        readonly Paint _dividerPaint = new Paint() {
            color = CupertinoDialogUtils._kButtonDividerColor,
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
            if (!(child.parentData is BoxParentData)) {
                child.parentData = new BoxParentData();
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
            return this._dialogWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return this._dialogWidth;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            float contentHeight = this.contentSection.getMinIntrinsicHeight(width);
            float actionsHeight = this.actionsSection.getMinIntrinsicHeight(width);
            bool hasDivider = contentHeight > 0.0f && actionsHeight > 0.0f;
            float height = contentHeight + (hasDivider ? this._dividerThickness : 0.0f) + actionsHeight;
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
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected override void performLayout() {
            if (this.isInAccessibilityMode) {
                this.performAccessibilityLayout();
            }
            else {
                this.performRegularLayout();
            }
        }

        void performRegularLayout() {
            bool hasDivider = this.contentSection.getMaxIntrinsicHeight(this._dialogWidth) > 0.0f
                              && this.actionsSection.getMaxIntrinsicHeight(this._dialogWidth) > 0.0f;
            float dividerThickness = hasDivider ? this._dividerThickness : 0.0f;
            float minActionsHeight = this.actionsSection.getMinIntrinsicHeight(this._dialogWidth);
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
            float dialogHeight = contentSize.height + dividerThickness + actionsSize.height;
            this.size = this.constraints.constrain(
                new Size(this._dialogWidth, dialogHeight)
            );
            D.assert(this.actionsSection.parentData is BoxParentData);
            BoxParentData actionParentData = this.actionsSection.parentData as BoxParentData;
            actionParentData.offset = new Offset(0.0f, contentSize.height + dividerThickness);
        }

        void performAccessibilityLayout() {
            bool hasDivider = this.contentSection.getMaxIntrinsicHeight(this._dialogWidth) > 0.0f
                              && this.actionsSection.getMaxIntrinsicHeight(this._dialogWidth) > 0.0f;
            float dividerThickness = hasDivider ? this._dividerThickness : 0.0f;
            float maxContentHeight = this.contentSection.getMaxIntrinsicHeight(this._dialogWidth);
            float maxActionsHeight = this.actionsSection.getMaxIntrinsicHeight(this._dialogWidth);
            Size contentSize;
            Size actionsSize;
            if (maxContentHeight + dividerThickness + maxActionsHeight > this.constraints.maxHeight) {
                this.actionsSection.layout(
                    this.constraints.deflate(EdgeInsets.only(top: this.constraints.maxHeight / 2.0f)),
                    parentUsesSize: true
                );
                actionsSize = this.actionsSection.size;
                this.contentSection.layout(
                    this.constraints.deflate(EdgeInsets.only(bottom: actionsSize.height + dividerThickness)),
                    parentUsesSize: true
                );
                contentSize = this.contentSection.size;
            }
            else {
                this.contentSection.layout(this.constraints,
                    parentUsesSize: true
                );
                contentSize = this.contentSection.size;
                this.actionsSection.layout(this.constraints.deflate(EdgeInsets.only(top: contentSize.height)),
                    parentUsesSize: true
                );
                actionsSize = this.actionsSection.size;
            }

            float dialogHeight = contentSize.height + dividerThickness + actionsSize.height;
            this.size = this.constraints.constrain(
                new Size(this._dialogWidth, dialogHeight)
            );
            D.assert(this.actionsSection.parentData is BoxParentData);
            BoxParentData actionParentData = this.actionsSection.parentData as BoxParentData;
            actionParentData.offset = new Offset(0.0f, contentSize.height + dividerThickness);
        }

        public override void paint(PaintingContext context, Offset offset) {
            BoxParentData contentParentData = this.contentSection.parentData as BoxParentData;
            this.contentSection.paint(context, offset + contentParentData.offset);
            bool hasDivider = this.contentSection.size.height > 0.0f && this.actionsSection.size.height > 0.0f;
            if (hasDivider) {
                this._paintDividerBetweenContentAndActions(context.canvas, offset);
            }

            BoxParentData actionsParentData = this.actionsSection.parentData as BoxParentData;
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

        protected override bool hitTestChildren(HitTestResult result, Offset position = null
        ) {
            bool isHit = false;
            BoxParentData contentSectionParentData = this.contentSection.parentData as BoxParentData;
            BoxParentData actionsSectionParentData = this.actionsSection.parentData as BoxParentData;
            ;
            if (this.contentSection.hitTest(result, position: position - contentSectionParentData.offset)) {
                isHit = true;
            }
            else if (this.actionsSection.hitTest(result, position: position - actionsSectionParentData.offset)) {
                isHit = true;
            }

            return isHit;
        }
    }

    enum _AlertDialogSections {
        contentSection,
        actionsSection,
    }

    class _CupertinoDialogAlertContentSection : StatelessWidget {
        public _CupertinoDialogAlertContentSection(
            Key key = null,
            Widget title = null,
            Widget content = null,
            ScrollController scrollController = null
        ) : base(key: key) {
            this.title = title;
            this.content = content;
            this.scrollController = scrollController;
        }

        public readonly Widget title;
        public readonly Widget content;
        public readonly ScrollController scrollController;

        public override Widget build(BuildContext context) {
            float textScaleFactor = MediaQuery.of(context).textScaleFactor;
            List<Widget> titleContentGroup = new List<Widget>();
            if (this.title != null) {
                titleContentGroup.Add(new Padding(
                    padding: EdgeInsets.only(
                        left: CupertinoDialogUtils._kEdgePadding,
                        right: CupertinoDialogUtils._kEdgePadding,
                        bottom: this.content == null ? CupertinoDialogUtils._kEdgePadding : 1.0f,
                        top: CupertinoDialogUtils._kEdgePadding * textScaleFactor
                    ),
                    child: new DefaultTextStyle(
                        style: CupertinoDialogUtils._kCupertinoDialogTitleStyle,
                        textAlign: TextAlign.center,
                        child: this.title
                    )
                ));
            }

            if (this.content != null) {
                titleContentGroup.Add(
                    new Padding(
                        padding: EdgeInsets.only(
                            left: CupertinoDialogUtils._kEdgePadding,
                            right: CupertinoDialogUtils._kEdgePadding,
                            bottom: CupertinoDialogUtils._kEdgePadding * textScaleFactor,
                            top: this.title == null ? CupertinoDialogUtils._kEdgePadding : 1.0f
                        ),
                        child: new DefaultTextStyle(
                            style: CupertinoDialogUtils._kCupertinoDialogContentStyle,
                            textAlign: TextAlign.center,
                            child: this.content
                        )
                    )
                );
            }

            if (titleContentGroup.isEmpty()) {
                return new SingleChildScrollView(
                    controller: this.scrollController,
                    child: new Container(width: 0.0f, height: 0.0f)
                );
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

    class _CupertinoDialogAlertActionSection : StatefulWidget {
        public _CupertinoDialogAlertActionSection(
            List<Widget> children,
            Key key = null,
            ScrollController scrollController = null
        ) : base(key: key) {
            D.assert(children != null);
            this.children = children;
            this.scrollController = scrollController;
        }

        public readonly List<Widget> children;
        public readonly ScrollController scrollController;

        public override State createState() {
            return new _CupertinoDialogAlertActionSectionState();
        }
    }

    class _CupertinoDialogAlertActionSectionState : State<_CupertinoDialogAlertActionSection> {
        public override Widget build(BuildContext context) {
            float devicePixelRatio = MediaQuery.of(context).devicePixelRatio;
            List<Widget> interactiveButtons = new List<Widget>();
            for (int i = 0; i < this.widget.children.Count; i += 1) {
                interactiveButtons.Add(
                    new _PressableDialogActionButton(
                        child: this.widget.children[i]
                    )
                );
            }

            return new CupertinoScrollbar(
                child: new SingleChildScrollView(
                    controller: this.widget.scrollController,
                    child: new _CupertinoDialogActionsRenderWidget(
                        actionButtons: interactiveButtons,
                        dividerThickness: CupertinoDialogUtils._kDividerThickness / devicePixelRatio
                    )
                )
            );
        }
    }

    class _PressableDialogActionButton : StatefulWidget {
        public _PressableDialogActionButton(
            Widget child
        ) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _PressableDialogActionButtonState();
        }
    }

    class _PressableDialogActionButtonState : State<_PressableDialogActionButton> {
        bool _isPressed = false;

        public override Widget build(BuildContext context) {
            return new _DialogActionButtonParentDataWidget(
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

    class _DialogActionButtonParentDataWidget : ParentDataWidget<_CupertinoDialogActionsRenderWidget> {
        public _DialogActionButtonParentDataWidget(
            Widget child,
            bool isPressed = false,
            Key key = null
        ) : base(key: key, child: child) {
            this.isPressed = isPressed;
        }

        public readonly bool isPressed;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is _DialogActionButtonParentData);
            _DialogActionButtonParentData parentData = renderObject.parentData as _DialogActionButtonParentData;
            if (parentData.isPressed != this.isPressed) {
                parentData.isPressed = this.isPressed;
                AbstractNodeMixinDiagnosticableTree targetParent = renderObject.parent;
                if (targetParent is RenderObject) {
                    ((RenderObject) targetParent).markNeedsPaint();
                }
            }
        }
    }

    class _DialogActionButtonParentData : MultiChildLayoutParentData {
        public _DialogActionButtonParentData(
            bool isPressed = false
        ) {
            this.isPressed = isPressed;
        }

        public bool isPressed;
    }

    public class CupertinoDialogAction : StatelessWidget {
        public CupertinoDialogAction(
            Widget child,
            VoidCallback onPressed = null,
            bool isDefaultAction = false,
            bool isDestructiveAction = false,
            TextStyle textStyle = null
        ) {
            D.assert(child != null);
            this.onPressed = onPressed;
            this.isDefaultAction = isDefaultAction;
            this.isDestructiveAction = isDestructiveAction;
            this.textStyle = textStyle;
            this.child = child;
        }

        public readonly VoidCallback onPressed;
        public readonly bool isDefaultAction;
        public readonly bool isDestructiveAction;
        public readonly TextStyle textStyle;
        public readonly Widget child;

        public bool enabled {
            get { return this.onPressed != null; }
        }

        float _calculatePadding(BuildContext context) {
            return 8.0f * MediaQuery.textScaleFactorOf(context);
        }

        Widget _buildContentWithRegularSizingPolicy(
            BuildContext context,
            TextStyle textStyle,
            Widget content
        ) {
            bool isInAccessibilityMode = CupertinoDialogUtils._isInAccessibilityMode(context);
            float dialogWidth = isInAccessibilityMode
                ? CupertinoDialogUtils._kAccessibilityCupertinoDialogWidth
                : CupertinoDialogUtils._kCupertinoDialogWidth;
            float textScaleFactor = MediaQuery.textScaleFactorOf(context);
            float fontSizeRatio =
                (textScaleFactor * textStyle.fontSize) / CupertinoDialogUtils._kMinButtonFontSize ?? 0f;
            float padding = this._calculatePadding(context);
            return new IntrinsicHeight(
                child: new SizedBox(
                    width: float.PositiveInfinity,
                    child: new FittedBox(
                        fit: BoxFit.scaleDown,
                        child: new ConstrainedBox(
                            constraints: new BoxConstraints(
                                maxWidth: fontSizeRatio * (dialogWidth - (2 * padding))
                            ),
                            child: new DefaultTextStyle(
                                style: textStyle,
                                textAlign: TextAlign.center,
                                overflow: TextOverflow.ellipsis,
                                maxLines: 1,
                                child: content
                            )
                        )
                    )
                )
            );
        }

        Widget _buildContentWithAccessibilitySizingPolicy(
            TextStyle textStyle,
            Widget content
        ) {
            return new DefaultTextStyle(
                style: textStyle,
                textAlign: TextAlign.center,
                child: content
            );
        }

        public override Widget build(BuildContext context) {
            TextStyle style = CupertinoDialogUtils._kCupertinoDialogActionStyle;
            style = style.merge(this.textStyle);
            if (this.isDestructiveAction) {
                style = style.copyWith(color: CupertinoColors.destructiveRed);
            }

            if (!this.enabled) {
                style = style.copyWith(color: style.color.withOpacity(0.5f));
            }

            Widget sizedContent = CupertinoDialogUtils._isInAccessibilityMode(context)
                ? this._buildContentWithAccessibilitySizingPolicy(
                    textStyle: style,
                    content: this.child
                )
                : this._buildContentWithRegularSizingPolicy(
                    context: context,
                    textStyle: style,
                    content: this.child
                );
            return new GestureDetector(
                onTap: () => this.onPressed(),
                behavior: HitTestBehavior.opaque,
                child: new ConstrainedBox(
                    constraints: new BoxConstraints(
                        minHeight: CupertinoDialogUtils._kMinButtonHeight
                    ),
                    child: new Container(
                        alignment: Alignment.center,
                        padding: EdgeInsets.all(this._calculatePadding(context)),
                        child: sizedContent
                    )
                )
            );
        }
    }

    class _CupertinoDialogActionsRenderWidget : MultiChildRenderObjectWidget {
        public _CupertinoDialogActionsRenderWidget(
            List<Widget> actionButtons,
            Key key = null,
            float dividerThickness = 0.0f
        ) : base(key: key, children: actionButtons) {
            this._dividerThickness = dividerThickness;
        }

        public readonly float _dividerThickness;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoDialogActions(
                dialogWidth: CupertinoDialogUtils._isInAccessibilityMode(context)
                    ? CupertinoDialogUtils._kAccessibilityCupertinoDialogWidth
                    : CupertinoDialogUtils._kCupertinoDialogWidth,
                dividerThickness: this._dividerThickness
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            (renderObject as _RenderCupertinoDialogActions).dialogWidth =
                CupertinoDialogUtils._isInAccessibilityMode(context)
                    ? CupertinoDialogUtils._kAccessibilityCupertinoDialogWidth
                    : CupertinoDialogUtils._kCupertinoDialogWidth;
            (renderObject as _RenderCupertinoDialogActions).dividerThickness = this._dividerThickness;
        }
    }

    class _RenderCupertinoDialogActions : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<
        RenderBox, MultiChildLayoutParentData> {
        public _RenderCupertinoDialogActions(
            float dialogWidth,
            List<RenderBox> children = null,
            float dividerThickness = 0.0f
        ) {
            this._dialogWidth = dialogWidth;
            this._dividerThickness = dividerThickness;
            this.addAll(children);
        }

        public float dialogWidth {
            get { return this._dialogWidth; }
            set {
                if (value != this._dialogWidth) {
                    this._dialogWidth = value;
                    this.markNeedsLayout();
                }
            }
        }

        float _dialogWidth;


        public float dividerThickness {
            get { return this._dividerThickness; }
            set {
                if (value != this._dividerThickness) {
                    this._dividerThickness = value;
                    this.markNeedsLayout();
                }
            }
        }

        float _dividerThickness;

        readonly Paint _buttonBackgroundPaint = new Paint() {
            color = CupertinoDialogUtils._kDialogColor,
            style = PaintingStyle.fill
        };

        readonly Paint _pressedButtonBackgroundPaint = new Paint() {
            color = CupertinoDialogUtils._kDialogPressedColor,
            style = PaintingStyle.fill
        };


        readonly Paint _dividerPaint = new Paint() {
            color = CupertinoDialogUtils._kButtonDividerColor,
            style = PaintingStyle.fill
        };

        List<RenderBox> _pressedButtons {
            get {
                List<RenderBox> childList = new List<RenderBox>();

                RenderBox currentChild = this.firstChild;
                while (currentChild != null) {
                    D.assert(currentChild.parentData is _DialogActionButtonParentData);
                    _DialogActionButtonParentData parentData = currentChild.parentData as _DialogActionButtonParentData;
                    if (parentData.isPressed) {
                        childList.Add(currentChild);
                    }

                    currentChild = this.childAfter(currentChild);
                }

                return childList;
            }
        }

        bool _isButtonPressed {
            get {
                RenderBox currentChild = this.firstChild;
                while (currentChild != null) {
                    D.assert(currentChild.parentData is _DialogActionButtonParentData);
                    _DialogActionButtonParentData parentData = currentChild.parentData as _DialogActionButtonParentData;
                    if (parentData.isPressed) {
                        return true;
                    }

                    currentChild = this.childAfter(currentChild);
                }

                return false;
            }
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is _DialogActionButtonParentData)) {
                child.parentData = new _DialogActionButtonParentData();
            }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return this.dialogWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return this.dialogWidth;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            float minHeight;
            if (this.childCount == 0) {
                minHeight = 0.0f;
            }
            else if (this.childCount == 1) {
                minHeight = this._computeMinIntrinsicHeightSideBySide(width);
            }
            else {
                if (this.childCount == 2 && this._isSingleButtonRow(width)) {
                    minHeight = this._computeMinIntrinsicHeightSideBySide(width);
                }
                else {
                    minHeight = this._computeMinIntrinsicHeightStacked(width);
                }
            }

            return minHeight;
        }

        float _computeMinIntrinsicHeightSideBySide(float width) {
            D.assert(this.childCount >= 1 && this.childCount <= 2);
            float minHeight;
            if (this.childCount == 1) {
                minHeight = this.firstChild.getMinIntrinsicHeight(width);
            }
            else {
                float perButtonWidth = (width - this.dividerThickness) / 2.0f;
                minHeight = Mathf.Max(this.firstChild.getMinIntrinsicHeight(perButtonWidth),
                    this.lastChild.getMinIntrinsicHeight(perButtonWidth)
                );
            }

            return minHeight;
        }

        float _computeMinIntrinsicHeightStacked(float width) {
            D.assert(this.childCount >= 2);
            return this.firstChild.getMinIntrinsicHeight(width)
                   + this.dividerThickness
                   + (0.5f * this.childAfter(this.firstChild).getMinIntrinsicHeight(width));
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float maxHeight;
            if (this.childCount == 0) {
                maxHeight = 0.0f;
            }
            else if (this.childCount == 1) {
                maxHeight = this.firstChild.getMaxIntrinsicHeight(width);
            }
            else if (this.childCount == 2) {
                if (this._isSingleButtonRow(width)) {
                    float perButtonWidth = (width - this.dividerThickness) / 2.0f;
                    maxHeight = Mathf.Max(this.firstChild.getMaxIntrinsicHeight(perButtonWidth),
                        this.lastChild.getMaxIntrinsicHeight(perButtonWidth)
                    );
                }
                else {
                    maxHeight = this._computeMaxIntrinsicHeightStacked(width);
                }
            }
            else {
                maxHeight = this._computeMaxIntrinsicHeightStacked(width);
            }

            return maxHeight;
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

        bool _isSingleButtonRow(float width) {
            bool isSingleButtonRow;
            if (this.childCount == 1) {
                isSingleButtonRow = true;
            }
            else if (this.childCount == 2) {
                float sideBySideWidth = this.firstChild.getMaxIntrinsicWidth(float.PositiveInfinity)
                                        + this.dividerThickness
                                        + this.lastChild.getMaxIntrinsicWidth(float.PositiveInfinity);
                isSingleButtonRow = sideBySideWidth <= width;
            }
            else {
                isSingleButtonRow = false;
            }

            return isSingleButtonRow;
        }

        protected override void performLayout() {
            if (this._isSingleButtonRow(this.dialogWidth)) {
                if (this.childCount == 1) {
                    this.firstChild.layout(
                        this.constraints,
                        parentUsesSize: true
                    );
                    this.size = this.constraints.constrain(
                        new Size(this.dialogWidth, this.firstChild.size.height)
                    );
                }
                else {
                    BoxConstraints perButtonnewraints = new BoxConstraints(
                        minWidth: (this.constraints.minWidth - this.dividerThickness) / 2.0f,
                        maxWidth: (this.constraints.maxWidth - this.dividerThickness) / 2.0f,
                        minHeight: 0.0f,
                        maxHeight: float.PositiveInfinity
                    );
                    this.firstChild.layout(
                        perButtonnewraints,
                        parentUsesSize: true
                    );
                    this.lastChild.layout(
                        perButtonnewraints,
                        parentUsesSize: true
                    );
                    D.assert(this.lastChild.parentData is MultiChildLayoutParentData);
                    MultiChildLayoutParentData secondButtonParentData =
                        this.lastChild.parentData as MultiChildLayoutParentData;
                    secondButtonParentData.offset =
                        new Offset(this.firstChild.size.width + this.dividerThickness, 0.0f);
                    this.size = this.constraints.constrain(
                        new Size(this.dialogWidth,
                            Mathf.Max(this.firstChild.size.height, this.lastChild.size.height
                            )
                        )
                    );
                }
            }
            else {
                BoxConstraints perButtonnewraints = this.constraints.copyWith(
                    minHeight: 0.0f,
                    maxHeight: float.PositiveInfinity
                );
                RenderBox child = this.firstChild;
                int index = 0;
                float verticalOffset = 0.0f;
                while (child != null) {
                    child.layout(
                        perButtonnewraints,
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
                    new Size(this.dialogWidth, verticalOffset)
                );
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;
            if (this._isSingleButtonRow(this.size.width)) {
                this._drawButtonBackgroundsAndDividersSingleRow(canvas, offset);
            }
            else {
                this._drawButtonBackgroundsAndDividersStacked(canvas, offset);
            }

            this._drawButtons(context, offset);
        }

        void _drawButtonBackgroundsAndDividersSingleRow(Canvas canvas, Offset offset) {
            Rect verticalDivider = this.childCount == 2 && !this._isButtonPressed
                ? Rect.fromLTWH(
                    offset.dx + this.firstChild.size.width,
                    offset.dy, this.dividerThickness,
                    Mathf.Max(this.firstChild.size.height, this.lastChild.size.height
                    )
                )
                : Rect.zero;
            List<Rect> pressedButtonRects = new List<Rect>();

            foreach (var item in this._pressedButtons) {
                MultiChildLayoutParentData buttonParentData = item.parentData as MultiChildLayoutParentData;
                pressedButtonRects.Add(
                    Rect.fromLTWH(
                        offset.dx + buttonParentData.offset.dx,
                        offset.dy + buttonParentData.offset.dy,
                        item.size.width,
                        item.size.height
                    ));
            }

            Path backgroundFillPath = new Path();

            // backgroundFillPath.fillType = PathFillType.evenOdd;
            backgroundFillPath.addRect(Rect.fromLTWH(0.0f, 0.0f, this.size.width, this.size.height));
            backgroundFillPath.addRect(verticalDivider);

            for (int i = 0; i < pressedButtonRects.Count; i += 1) {
                backgroundFillPath.addRect(pressedButtonRects[i]);
            }

            canvas.drawPath(
                backgroundFillPath, this._buttonBackgroundPaint
            );
            Path pressedBackgroundFillPath = new Path();
            for (int i = 0; i < pressedButtonRects.Count; i += 1) {
                pressedBackgroundFillPath.addRect(pressedButtonRects[i]);
            }

            canvas.drawPath(
                pressedBackgroundFillPath, this._pressedButtonBackgroundPaint
            );
            Path dividersPath = new Path();
            dividersPath.addRect(verticalDivider);
            canvas.drawPath(
                dividersPath, this._dividerPaint
            );
        }

        void _drawButtonBackgroundsAndDividersStacked(Canvas canvas, Offset offset) {
            Offset dividerOffset = new Offset(0.0f, this.dividerThickness);
            Path backgroundFillPath = new Path();
            // ..fillType = PathFillType.evenOdd
            backgroundFillPath.addRect(Rect.fromLTWH(0.0f, 0.0f, this.size.width, this.size.height));
            Path pressedBackgroundFillPath = new Path();
            Path dividersPath = new Path();
            Offset accumulatingOffset = offset;
            RenderBox child = this.firstChild;
            RenderBox prevChild = null;

            while (child != null) {
                D.assert(child.parentData is _DialogActionButtonParentData);
                _DialogActionButtonParentData currentButtonParentData =
                    child.parentData as _DialogActionButtonParentData;
                bool isButtonPressed = currentButtonParentData.isPressed;
                bool isPrevButtonPressed = false;
                if (prevChild != null) {
                    D.assert(prevChild.parentData is _DialogActionButtonParentData);
                    _DialogActionButtonParentData previousButtonParentData =
                        prevChild.parentData as _DialogActionButtonParentData;
                    isPrevButtonPressed = previousButtonParentData.isPressed;
                }

                bool isDividerPresent = child != this.firstChild;
                bool isDividerPainted = isDividerPresent && !(isButtonPressed || isPrevButtonPressed);
                Rect dividerRect = Rect.fromLTWH(
                    accumulatingOffset.dx,
                    accumulatingOffset.dy, this.size.width, this.dividerThickness
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

        protected override bool hitTestChildren(HitTestResult result,
            Offset position = null
        ) {
            return this.defaultHitTestChildren(result, position: position);
        }
    }
}