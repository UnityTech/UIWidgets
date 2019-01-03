using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UIWidgets.Sample.Redux.ObjectFinder {
    public class FinderGameObject : MonoBehaviour {
        // Start is called before the first frame update
        void Start() {
            GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0, 0, 1.0f);
        }

        // Update is called once per frame
        void Update() {
            var selectedId = StoreProvider.store.state.selected;
            if (selectedId == GetInstanceID()) {
                GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0, 0, 1.0f);
            } else {
                GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }

        void OnMouseDown() {
            StoreProvider.store.Dispatch(new SelectObjectAction(){id = GetInstanceID()});
        }
    }
}