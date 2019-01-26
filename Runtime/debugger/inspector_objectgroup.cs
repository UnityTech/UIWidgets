using System.Diagnostics;
using UnityEngine;

namespace Unity.UIWidgets.debugger {
    public class InspectorObjectGroupManager : Singleton<InspectorObjectGroupManager> {
        [SerializeField] int m_NextId = 0;

        public string nextGroupName(string name) {
            return $"pid{Process.GetCurrentProcess().Id}_{name}_{this.m_NextId++}";
        }
    }

    public class Singleton<T> : ScriptableObject where T : ScriptableObject {
        static T m_Instance;
        static bool m_CreateNonSingletonInstance;
        bool m_IsNonSingletonInstance;

        public static T Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = CreateInstance<T>();
                }

                return m_Instance;
            }
        }


        void OnEnable() {
            if (m_CreateNonSingletonInstance) {
                this.m_IsNonSingletonInstance = true;
                this.Initialize();
            }
            else if (this.m_IsNonSingletonInstance) {
                DestroyImmediate((Object) this);
            }
            else if (m_Instance == null) {
                m_Instance = this as T;
                this.Initialize();
            }
            else {
                DestroyImmediate((Object) this);
            }
        }

        protected virtual void Initialize() {
        }

        public static T Create() {
            m_CreateNonSingletonInstance = true;
            var instance = CreateInstance<T>();
            m_CreateNonSingletonInstance = false;
            return (T) instance;
        }
    }
}