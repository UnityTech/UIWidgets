using System.Collections.Generic;
using System.Diagnostics;

namespace Unity.UIWidgets.ui {
    public abstract class PoolObject
    {
        public bool activated_flag;
        
        public virtual void setup() {}
        public virtual void clear() {}
    }

    public static class ObjectPool<TObject> where TObject :PoolObject, new() {
        static readonly Stack<TObject> pool = new Stack<TObject>();
        
        public static TObject alloc() {
            if (pool.Count == 0) {
                for (int i = 0; i < 128; i++) {
                    var obj = new TObject();
                    pool.Push(obj);
                }
            }
            
            var ret = pool.Pop();
            ret.setup();
            
            if (AllocDebugger.enableDebugging) {
                AllocDebugger.onAlloc(debugKey, debugName);
                ret.activated_flag = true;
            }
            
            return ret;
        }

        public static void release(TObject obj) {
            if (obj == null) {
                return;
            }

            if (AllocDebugger.enableDebugging) {
                if (!obj.activated_flag) {
                    Debug.Assert(false, "an item has been recycled more than once !");
                }
                obj.activated_flag = false;

                AllocDebugger.onRelease(debugKey, debugName);
            }

            obj.clear();
            pool.Push(obj);
        }
        
        //For debugger
        static bool _debugInfoReady = false;
        static string _debugName = null;

        static void _generateDebugInfo() {
            var ctype = typeof(TObject);
            _debugName = ctype.ToString();
            _debugKey = ctype.GetHashCode();

            _debugInfoReady = true;
        }
        public static string debugName {
            get {
                if(_debugInfoReady)
                {
                    return _debugName;
                }
                _generateDebugInfo();
                return _debugName;
            }
        }

        static int _debugKey = -1;

        public static int debugKey {
            get {
                if (_debugInfoReady) {
                    return _debugKey;
                }
                _generateDebugInfo();
                return _debugKey;
            }
        }
    }
}