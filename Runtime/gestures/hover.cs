using System;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    
    public class HoverRecognizer : DiagnosticableTree {
        public HoverRecognizer(object debugOwner = null) {
            this.debugOwner = debugOwner;
        }

        readonly object debugOwner;
        
        public Action OnPointerEnter = () => {};

        public Action OnPointerLeave = () => {};
    }
}