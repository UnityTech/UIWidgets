using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class Card : StatelessWidget {
        public Card(
            Key key = null,
            Color color = null,
            float? elevation = null,
            ShapeBorder shape = null,
            bool borderOnForeground = true,
            EdgeInsets margin = null,
            Clip? clipBehavior = null,
            Widget child = null) : base(key: key) {
            D.assert(elevation == null || elevation >= 0.0f);
            this.color = color;
            this.elevation = elevation;
            this.shape = shape;
            this.borderOnForeground = borderOnForeground;
            this.margin = margin;
            this.clipBehavior = clipBehavior;
            this.child = child;
        }

        public readonly Color color;

        public readonly float? elevation;

        public readonly ShapeBorder shape;

        public readonly bool borderOnForeground;

        public readonly Clip? clipBehavior;

        public readonly EdgeInsets margin;

        public readonly Widget child;
        
        const float _defaultElevation = 1.0f;
        const Clip _defaultClipBehavior = Clip.none;

        public override Widget build(BuildContext context) {
            CardTheme cardTheme = CardTheme.of(context);

            return new Container(
                margin: this.margin ?? cardTheme.margin ?? EdgeInsets.all(4.0f),
                child: new Material(
                    type: MaterialType.card,
                    color: this.color ?? cardTheme.color ?? Theme.of(context).cardColor,
                    elevation: this.elevation ?? cardTheme.elevation ?? _defaultElevation,
                    shape: this.shape ?? cardTheme.shape ?? new RoundedRectangleBorder(
                               borderRadius: BorderRadius.all(Radius.circular(4.0f))
                           ),
                    borderOnForeground: this.borderOnForeground,
                    clipBehavior: this.clipBehavior ?? cardTheme.clipBehavior ?? _defaultClipBehavior,
                    child: this.child)
            );
        }
    }
}