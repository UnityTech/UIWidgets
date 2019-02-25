using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class GridTile : StatelessWidget {
        public GridTile(
            Key key = null,
            Widget header = null,
            Widget footer = null,
            Widget child = null) : base(key: key) {
            D.assert(child != null);
            this.header = header;
            this.footer = footer;
            this.child = child;
        }

        public readonly Widget header;

        public readonly Widget footer;

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            if (this.header == null && this.footer == null) {
                return this.child;
            }

            List<Widget> children = new List<Widget> {
                Positioned.fill(
                    child: this.child)
            };
            if (this.header != null) {
                children.Add(new Positioned(
                    top: 0.0f,
                    left: 0.0f,
                    right: 0.0f,
                    child: this.header));
            }

            if (this.footer != null) {
                children.Add(new Positioned(
                    left: 0.0f,
                    bottom: 0.0f,
                    right: 0.0f,
                    child: this.footer));
            }

            return new Stack(
                children: children);
        }
    }
}