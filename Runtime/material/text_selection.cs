using System;
using System.Collections.Generic;
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
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.material {
    public static class MaterialUtils {
        public static readonly TextSelectionControls materialTextSelectionControls =
            new _MaterialTextSelectionControls();
        
        internal const float _kHandleSize = 22.0f;
        
        internal const float _kToolbarScreenPadding = 8.0f;
        
    }
    
    class _TextSelectionToolbar : StatelessWidget {
        public _TextSelectionToolbar(Key key = null, VoidCallback handleCut = null,
            VoidCallback handleCopy = null, VoidCallback handlePaste = null, VoidCallback handleSelectAll = null) : base(key: key) {
            this.handleCut = handleCut;
            this.handleCopy = handleCopy;
            this.handlePaste = handlePaste;
            this.handleSelectAll = handleSelectAll;
        }

        public readonly VoidCallback handleCut;
        public readonly VoidCallback handleCopy;
        public readonly VoidCallback handlePaste;
        public readonly VoidCallback handleSelectAll;

        public override Widget build(BuildContext context) {
            List<Widget> items = new List<Widget>();
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            if (this.handleCut != null) {
                items.Add(new FlatButton(child: new Text(localizations.cutButtonLabel), onPressed: this.handleCut));
            }

            if (this.handleCopy != null) {
                items.Add(new FlatButton(child: new Text(localizations.copyButtonLabel), onPressed: this.handleCopy));
            }

            if (this.handlePaste != null) {
                
                items.Add(new FlatButton(child: new Text(localizations.pasteButtonLabel), onPressed: this.handlePaste));
            }

            if (this.handleSelectAll != null) {
                items.Add(new FlatButton(child: new Text(localizations.selectAllButtonLabel), 
                    onPressed: this.handleSelectAll));
            }

            return new Material(
                elevation: 1.0f,
                child: new Container(
                    color: new Color(0xFFEFEFEF),
                    height: 44.0f, child: new Row(mainAxisSize: MainAxisSize.min, children: items))
                );
        }
    }

    class _TextSelectionToolbarLayout : SingleChildLayoutDelegate {
        internal _TextSelectionToolbarLayout(Size screenSize = null, Rect globalEditableRegion = null,
            Offset position = null) {
            this.screenSize = screenSize;
            this.globalEditableRegion = globalEditableRegion;
            this.position = position;
        }

        public readonly Size screenSize;
        public readonly Rect globalEditableRegion;
        public readonly Offset position;

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return constraints.loosen();
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            Offset globalPosition = this.globalEditableRegion.topLeft + this.position;

            float x = globalPosition.dx - childSize.width / 2.0f;
            float y = globalPosition.dy - childSize.height;

            if (x < MaterialUtils._kToolbarScreenPadding) {
                x = MaterialUtils._kToolbarScreenPadding;
            }
            else if (x + childSize.width > this.screenSize.width - MaterialUtils._kToolbarScreenPadding) {
                x = this.screenSize.width - childSize.width - MaterialUtils._kToolbarScreenPadding;
            }

            if (y < MaterialUtils._kToolbarScreenPadding) {
                y = MaterialUtils._kToolbarScreenPadding;
            }
            else if (y + childSize.height > this.screenSize.height - MaterialUtils._kToolbarScreenPadding) {
                y = this.screenSize.height - childSize.height - MaterialUtils._kToolbarScreenPadding;
            }

            return new Offset(x, y);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate oldDelegate) {
            return this.position != ((_TextSelectionToolbarLayout) oldDelegate).position;
        }
    }

    class _TextSelectionHandlePainter : AbstractCustomPainter {
        internal _TextSelectionHandlePainter(Color color) {
            this.color = color;
        }

        public readonly Color color;

        public override void paint(Canvas canvas, Size size) {
            Paint paint = new Paint();
            paint.color = this.color;
            float radius = size.width / 2.0f;
            canvas.drawCircle(new Offset(radius, radius), radius, paint);
            canvas.drawRect(Rect.fromLTWH(0.0f, 0.0f, radius, radius), paint);
        }


        public override bool shouldRepaint(CustomPainter oldPainter) {
            return this.color != ((_TextSelectionHandlePainter) oldPainter).color;
        }
    }

    class _MaterialTextSelectionControls : TextSelectionControls {
        public override Size handleSize {
            get {
                return new Size(MaterialUtils._kHandleSize,
                    MaterialUtils._kHandleSize);
            }
        }

        public override Widget buildToolbar(BuildContext context, Rect globalEditableRegion, Offset position,
            TextSelectionDelegate selectionDelegate) {
            return new ConstrainedBox(
                constraints: BoxConstraints.tight(globalEditableRegion.size),
                child: new CustomSingleChildLayout(
                    layoutDelegate: new _TextSelectionToolbarLayout(
                        MediaQuery.of(context).size,
                        globalEditableRegion,
                        position
                    ),
                    child: new _TextSelectionToolbar(
                        handleCut: this.canCut(selectionDelegate)
                            ? () => this.handleCut(selectionDelegate)
                            : (VoidCallback) null,
                        handleCopy: this.canCopy(selectionDelegate)
                            ? () => this.handleCopy(selectionDelegate)
                            : (VoidCallback) null,
                        handlePaste: this.canPaste(selectionDelegate)
                            ? () => this.handlePaste(selectionDelegate)
                            : (VoidCallback) null,
                        handleSelectAll: this.canSelectAll(selectionDelegate)
                            ? () => this.handleSelectAll(selectionDelegate)
                            : (VoidCallback) null
                    )
                )
            );
        }

        public override Widget buildHandle(BuildContext context, TextSelectionHandleType type, float textLineHeight) {
            Widget handle = new Padding(
                padding: EdgeInsets.only(right: 26.0f, bottom: 26.0f),
                child: new SizedBox(
                    width: MaterialUtils._kHandleSize,
                    height: MaterialUtils._kHandleSize,
                    child: new CustomPaint(
                        painter: new _TextSelectionHandlePainter(
                            color: Theme.of(context).textSelectionHandleColor
                        )
                    )
                )
            );

            switch (type) {
                case TextSelectionHandleType.left: // points up-right
                    return new Transform(
                        transform: Matrix3.makeRotate(Mathf.PI / 2),
                        child: handle
                    );
                case TextSelectionHandleType.right: // points up-left
                    return handle;
                case TextSelectionHandleType.collapsed: // points up
                    return new Transform(
                        transform: Matrix3.makeRotate(Mathf.PI / 4),
                        child: handle
                    );
            }

            return null;
        }
    }
}