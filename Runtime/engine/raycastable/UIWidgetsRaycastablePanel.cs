using Unity.UIWidgets.engine;
using UnityEngine;

namespace Unity.UIWidgets.engine.raycast {
    [RequireComponent(typeof(RectTransform))]
    public class UIWidgetsRaycastablePanel : UIWidgetsPanel, ICanvasRaycastFilter {
        int windowHashCode;

        protected override void InitWindowAdapter() {
            base.InitWindowAdapter();
            this.windowHashCode = this.window.GetHashCode();
            RaycastManager.NewWindow(this.windowHashCode);
        }

        protected override void OnDisable() {
            base.OnDisable();
            RaycastManager.DisposeWindow(this.windowHashCode);
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
            if (!this.enabled) {
                return true;
            }

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, screenPoint, eventCamera,
                out local);

            Rect rect = this.rectTransform.rect;

            // Convert top left corner as reference origin point.
            local.x += this.rectTransform.pivot.x * rect.width;
            local.y -= this.rectTransform.pivot.y * rect.height;
            local.x = local.x / this.devicePixelRatio;
            local.y = -local.y / this.devicePixelRatio;

            return !RaycastManager.CheckCastThrough(this.windowHashCode, local);
        }
    }
}