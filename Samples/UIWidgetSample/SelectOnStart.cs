using UnityEngine;
using UnityEngine.EventSystems;

namespace UIWidgetsSample
{
    public class SelectOnStart: MonoBehaviour
    {
        private void Start()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}