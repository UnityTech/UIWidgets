using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public delegate bool NotificationListenerCallback<T>(T notification) where T : Notification;

    public abstract class Notification {
        protected virtual bool visitAncestor(Element element) {
            if (element is StatelessElement) {
                StatelessWidget widget = (StatelessWidget) element.widget;
                var listener = widget as _NotificationListener;
                if (listener != null) {
                    if (listener._dispatch(this, element)) {
                        return false;
                    }
                }
            }

            return true;
        }

        public void dispatch(BuildContext target) {
            target?.visitAncestorElements(this.visitAncestor);
        }

        public override string ToString() {
            var description = new List<string>();
            this.debugFillDescription(description);
            return $"{this.GetType()}({string.Join(", ", description.ToArray())})";
        }

        protected virtual void debugFillDescription(List<string> description) {
        }
    }

    interface _NotificationListener {
        bool _dispatch(Notification notification, Element element);
    }

    public class NotificationListener<T> : StatelessWidget, _NotificationListener where T : Notification {
        public NotificationListener(
            Key key = null,
            Widget child = null,
            NotificationListenerCallback<T> onNotification = null) : base(key) {
            this.child = child;
            this.onNotification = onNotification;
        }

        public readonly Widget child;

        public readonly NotificationListenerCallback<T> onNotification;

        bool _NotificationListener._dispatch(Notification notification, Element element) {
            if (this.onNotification != null && notification is T) {
                bool result = this.onNotification((T) notification);
                return result;
            }

            return false;
        }

        public override Widget build(BuildContext context) {
            return this.child;
        }
    }

    public class LayoutChangedNotification : Notification {
    }
}