using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class ButtonBar : StatelessWidget {
        public ButtonBar(
            Key key = null,
            MainAxisAlignment alignment = MainAxisAlignment.end,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            List<Widget> children = null
        ) : base(key: key) {
            this.alignment = alignment;
            this.mainAxisSize = mainAxisSize;
            this.children = children ?? new List<Widget>();
        }

        public readonly MainAxisAlignment alignment;

        public readonly MainAxisSize mainAxisSize;

        public readonly List<Widget> children;


        public override Widget build(BuildContext context) {
            ButtonThemeData buttonTheme = ButtonTheme.of(context);
            float paddingUnit = buttonTheme.padding.horizontal / 4.0f;
            List<Widget> _children = new List<Widget>();
            foreach (Widget _child in this.children) {
                _children.Add(
                    new Padding(
                        padding: EdgeInsets.symmetric(horizontal: paddingUnit),
                        child: _child
                    )
                );
            }

            Widget child = new Row(
                mainAxisAlignment: this.alignment,
                mainAxisSize: this.mainAxisSize,
                children: _children
            );

            switch (buttonTheme.layoutBehavior) {
                case ButtonBarLayoutBehavior.padded:
                    return new Padding(
                        padding: EdgeInsets.symmetric(
                            vertical: 2.0f * paddingUnit,
                            horizontal: paddingUnit
                        ),
                        child: child
                    );
                case ButtonBarLayoutBehavior.constrained:
                    return new Container(
                        padding: EdgeInsets.symmetric(horizontal: paddingUnit),
                        constraints: new BoxConstraints(minHeight: 52.0f),
                        alignment: Alignment.center,
                        child: child
                    );
            }

            D.assert(false);
            return null;
        }
    }
}