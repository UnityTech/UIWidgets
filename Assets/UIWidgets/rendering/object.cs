using UIWidgets.foundation;
using UnityEngine;

namespace UIWidgets.rendering {
    public abstract class RenderObject : AbstractNode {
        protected RenderObject() {
        }

        public void onGUI(GUIContext context) {
        }
    }

    public class GUIContext {
        public void paintChild(RenderObject child) {
            child.onGUI(this);
        }
    }

    public class RenderLabel {
        public RenderLabel() {
        }
    }

    public class TextSpan {
        
    }
}