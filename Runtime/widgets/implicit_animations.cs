using System;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Random = UnityEngine.Random;


namespace Unity.UIWidgets.widgets {
    
    class BoxConstraintsTween : Tween<BoxConstraints> {
        public BoxConstraintsTween(
            BoxConstraints begin = null,
            BoxConstraints end = null
        ) : base(begin: begin, end: end) {
        }
        
        public override BoxConstraints lerp(double t) => BoxConstraints.lerp(this.begin, this.end, t);
    }


    public abstract class ImplicitlyAnimatedWidget : StatefulWidget {

        public ImplicitlyAnimatedWidget(
            Key key = null,
            Curve curve = null,
            TimeSpan? duration = null
        ) : base(key: key) {
            if (curve == null) {
                curve = Curves.linear;
            }
            D.assert(curve != null);
            D.assert(duration != null);
            this.curve = curve;
            this.duration = duration ?? TimeSpan.Zero;
        }

        readonly Curve curve;

        readonly TimeSpan duration;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("duration", (int)this.duration.TotalMilliseconds, unit: "ms"));
        }
    }
}