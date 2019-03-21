using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class BottomAppBar : StatefulWidget {
        public BottomAppBar(
            Key key = null,
            Color color = null,
            float elevation = 8.0f,
            NotchedShape shape = null,
            Clip clipBehavior = Clip.none,
            float notchMargin = 4.0f,
            Widget child = null
        ) : base(key: key) {
            D.assert(elevation != null);
            D.assert(elevation >= 0.0f);
            D.assert(clipBehavior != null);
            this.child = child;
            this.color = color;
            this.elevation = elevation;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
            this.notchMargin = notchMargin;
        }

        public readonly Widget child;

        public readonly Color color;

        public readonly float elevation;

        public readonly NotchedShape shape;

        public readonly Clip clipBehavior;

        public readonly float notchMargin;

        public override State createState() {
            return new _BottomAppBarState();
        }
    }

    class _BottomAppBarState : State<BottomAppBar> {
        ValueListenable<ScaffoldGeometry> geometryListenable;

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            this.geometryListenable = Scaffold.geometryOf(this.context);
        }

        public override Widget build(BuildContext context) {
            CustomClipper<Path> clipper = this.widget.shape != null
                ? (CustomClipper<Path>) new _BottomAppBarClipper(
                    geometry: this.geometryListenable,
                    shape: this.widget.shape,
                    notchMargin: this.widget.notchMargin
                )
                : new ShapeBorderClipper(shape: new RoundedRectangleBorder());
            return new PhysicalShape(
                clipper: clipper,
                elevation: this.widget.elevation,
                color: this.widget.color ?? Theme.of(context).bottomAppBarColor,
                clipBehavior: this.widget.clipBehavior,
                child: new Material(
                    type: MaterialType.transparency,
                    child: this.widget.child == null
                        ? null
                        : new SafeArea(child: this.widget.child)
                )
            );
        }
    }

    class _BottomAppBarClipper : CustomClipper<Path> {
        public _BottomAppBarClipper(
            ValueListenable<ScaffoldGeometry> geometry,
            NotchedShape shape,
            float notchMargin
        ) : base(reclip: geometry) {
            D.assert(geometry != null);
            D.assert(shape != null);
            D.assert(notchMargin != null);
            this.geometry = geometry;
            this.shape = shape;
            this.notchMargin = notchMargin;
        }

        public readonly ValueListenable<ScaffoldGeometry> geometry;
        public readonly NotchedShape shape;
        public readonly float notchMargin;

        public override Path getClip(Size size) {
            Rect appBar = Offset.zero & size;
            if (this.geometry.value.floatingActionButtonArea == null) {
                Path path = new Path();
                path.addRect(appBar);
                return path;
            }

            Rect button = this.geometry.value.floatingActionButtonArea
                .translate(0.0f, (this.geometry.value.bottomNavigationBarTop * -1.0f) ?? 0.0f);

            return this.shape.getOuterPath(appBar, button.inflate(this.notchMargin));
        }

        public override bool shouldReclip(CustomClipper<Path> oldClipper) {
            return (oldClipper as _BottomAppBarClipper).geometry != this.geometry;
        }
    }
}