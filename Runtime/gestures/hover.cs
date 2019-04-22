using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.gestures {
    
    public delegate void PointerHoverEnterCallback(PointerHoverEvent evt);

    public delegate void PointerHoverLeaveCallback();
    
    public class HoverRecognizer : DiagnosticableTree {
        public HoverRecognizer(object debugOwner = null) {
            this.debugOwner = debugOwner;
        }

        readonly object debugOwner;

        public PointerHoverEnterCallback OnPointerEnter;

        public PointerHoverLeaveCallback OnPointerLeave;
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("debugOwner", this.debugOwner,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }
}