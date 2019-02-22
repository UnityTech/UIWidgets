using Unity.UIWidgets.animation;
using Unity.UIWidgets.painting;

namespace Unity.UIWidgets.rendering {
    public class AlignmentTween : Tween<Alignment> {
        public AlignmentTween(
            Alignment begin = null,
            Alignment end = null)
            : base(begin: begin, end: end) {
        }

        public override Alignment lerp(float t) {
            return Alignment.lerp(this.begin, this.end, t);
        }
    }
}