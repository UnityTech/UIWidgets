using UnityEngine;
using UnityEngine.EventSystems;

namespace UIWidgetsSample {
    public class SelectOnStart : MonoBehaviour {
        void Start() {
            EventSystem.current.SetSelectedGameObject(this.gameObject);
        }
    }
}