using System.Diagnostics;
using UnityEngine;

namespace UIWidgets.debugger
{
    public class InspectorObjectGroupManager : Singleton<InspectorObjectGroupManager>
    {
        [SerializeField]
        private int m_NextId = 0;

        public string nextGroupName(string name)
        {
            return string.Format("pid{0}_{1}_{2}", Process.GetCurrentProcess().Id, name, m_NextId++);
        }
        
    }
    public class Singleton<T>: ScriptableObject where T : ScriptableObject
    {
        private static T m_Instance;
        private static bool m_CreateNonSingletonInstance;
        private bool m_IsNonSingletonInstance;
        
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = ScriptableObject.CreateInstance<T>();
                return m_Instance;
            }
        }
        
        
        private void OnEnable()
        {
            if (Singleton<T>.m_CreateNonSingletonInstance)
            {
                this.m_IsNonSingletonInstance = true;
                this.Initialize();
            }
            else if (this.m_IsNonSingletonInstance)
                Object.DestroyImmediate((Object) this);
            else if (m_Instance == null)
            {
                m_Instance = this as T;
                this.Initialize();
            }
            else
            {
                Object.DestroyImmediate((Object) this);
            }
        }

        protected virtual void Initialize()
        {
        }

        public static T Create()
        {
            Singleton<T>.m_CreateNonSingletonInstance = true;
            var instance = ScriptableObject.CreateInstance<T>();
            Singleton<T>.m_CreateNonSingletonInstance = false;
            return (T) instance;
        }
    }
}