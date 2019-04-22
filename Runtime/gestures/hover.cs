using System;
using Unity.UIWidgets.foundation;
using UnityEngine;

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
    }
}