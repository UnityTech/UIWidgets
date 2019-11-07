using Unity.UIWidgets.engine;
using UnityEngine;

namespace Unity.UIWidgets.plugins.raycast {
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIWidgetsPanel))]
    [DefaultExecutionOrder(1)]
    public class UIWidgetsPanelRaycastFilter : MonoBehaviour, ICanvasRaycastFilter {
        public bool reversed;
        
        UIWidgetsPanel panel;
        int windowHashCode;

        void OnEnable() {
            this.panel = this.GetComponent<UIWidgetsPanel>();
            this.windowHashCode = this.panel.window.GetHashCode();
            RaycastManager.VerifyWindow(this.windowHashCode);
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
            if (!this.enabled) {
                return true;
            }

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.panel.rectTransform, screenPoint, eventCamera,
                out local);

            Rect rect = this.panel.rectTransform.rect;

            // Convert top left corner as reference origin point.
            local.x += this.panel.rectTransform.pivot.x * rect.width;
            local.y -= this.panel.rectTransform.pivot.y * rect.height;
            local.x = local.x / this.panel.devicePixelRatio;
            local.y = -local.y / this.panel.devicePixelRatio;

            if (this.reversed) {
                return RaycastManager.CheckCastThrough(this.windowHashCode, local);
            }
            else {
                return !RaycastManager.CheckCastThrough(this.windowHashCode, local);
            }
        }
    }
}