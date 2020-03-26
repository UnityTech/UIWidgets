using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class Spacer : StatelessWidget {
        public Spacer(
            Key key = null,
            int flex = 1)
            : base(key: key) {
            D.assert(flex > 0);
            this.flex = flex;
        }

        public readonly int flex;

        public override Widget build(BuildContext context) {
            return new Expanded(
                flex: this.flex,
                child: SizedBox.shrink()
            );
        }
    }
}