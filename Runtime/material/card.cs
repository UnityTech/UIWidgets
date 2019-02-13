using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class Card : StatelessWidget {
        public Card(
            Key key = null,
            Color color = null,
            double? elevation = null,
            ShapeBorder shape = null,
            EdgeInsets margin = null,
            Clip clipBehavior = Clip.none,
            Widget child = null) : base(key: key) {
            this.color = color;
            this.elevation = elevation;
            this.shape = shape;
            this.margin = margin ?? EdgeInsets.all(4.0);
            this.clipBehavior = clipBehavior;
            this.child = child;
        }

        public readonly Color color;

        public readonly double? elevation;

        public readonly ShapeBorder shape;

        public readonly Clip clipBehavior;

        public readonly EdgeInsets margin;

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new Container(
                margin: this.margin ?? EdgeInsets.all(4.0),
                child: new Material(
                    type: MaterialType.card,
                    color: this.color ?? Theme.of(context).cardColor,
                    elevation: this.elevation ?? 1.0,
                    shape: this.shape ?? new RoundedRectangleBorder(
                               borderRadius: BorderRadius.all(Radius.circular(4.0))
                           ),
                    clipBehavior: this.clipBehavior,
                    child: this.child)
            );
        }
    }
}