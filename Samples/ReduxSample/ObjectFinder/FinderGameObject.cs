using UnityEngine;

namespace Unity.UIWidgets.Sample.Redux.ObjectFinder {
    public class FinderGameObject : MonoBehaviour {
        // Start is called before the first frame update
        void Start() {
            this.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0, 0, 1.0f);
        }

        // Update is called once per frame
        void Update() {
            var selectedId = StoreProvider.store.getState().selected;
            if (selectedId == this.GetInstanceID()) {
                this.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0, 0, 1.0f);
            }
            else {
                this.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }

        void OnMouseDown() {
            StoreProvider.store.dispatcher.dispatch(new SelectObjectAction() {id = this.GetInstanceID()});
        }
    }
}