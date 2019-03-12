using System.Collections.Generic;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {

 
    public abstract class ViewportNotificationMixinNotification : Notification {
        public int depth {
            get { return this._depth; }
        }

        int _depth = 0;

        protected override bool visitAncestor(Element element) {
            if (element is RenderObjectElement && element.renderObject is RenderAbstractViewport) {
                this._depth += 1;
            }

            return base.visitAncestor(element);
        }

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add(string.Format("depth: {0} ({1})",
                this._depth, this._depth == 0 ? "local" : "remote"));
        }
    }


 
    public abstract class ViewportNotificationMixinLayoutChangedNotification : LayoutChangedNotification {
        public int depth {
            get { return this._depth; }
        }

        int _depth = 0;

        protected override bool visitAncestor(Element element) {
            if (element is RenderObjectElement && element.renderObject is RenderAbstractViewport) {
                this._depth += 1;
            }

            return base.visitAncestor(element);
        }

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add(string.Format("depth: {0} ({1})",
                this._depth, this._depth == 0 ? "local" : "remote"));
        }
    }

}