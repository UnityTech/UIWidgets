using System.Collections.Generic;
using System.Diagnostics;

namespace Unity.UIWidgets.ui {
    public abstract class PoolObject {
        public bool activated_flag;

        public virtual void setup() {
        }

        public virtual void clear() {
        }
    }

    public static class ObjectPool<TObject> where TObject : PoolObject, new() {
        static readonly Stack<TObject> pool = new Stack<TObject>();

        const int POOL_MAX_SIZE = 256;
        const int POOL_BATCH_SIZE = 128;

        static int allocatedCount = 0;

        public static TObject alloc() {
            if (pool.Count == 0) {
                for (int i = 0; i < POOL_BATCH_SIZE; i++) {
                    var obj = new TObject();
                    pool.Push(obj);
                }

                allocatedCount += POOL_BATCH_SIZE;
            }

            var ret = pool.Pop();
            ret.setup();

            #pragma warning disable CS0162
            if (AllocDebugger.enableDebugging) {
                AllocDebugger.onAlloc(debugKey, debugName, allocatedCount);
                ret.activated_flag = true;
            }
            #pragma warning restore CS0162

            return ret;
        }

        public static void release(TObject obj) {
            if (obj == null) {
                return;
            }

            #pragma warning disable CS0162
            if (AllocDebugger.enableDebugging) {
                if (!obj.activated_flag) {
                    Debug.Assert(false, "an item has been recycled more than once !");
                }

                obj.activated_flag = false;

                AllocDebugger.onRelease(debugKey, debugName, allocatedCount);
            }
            #pragma warning restore CS0162

            obj.clear();
            if (pool.Count > POOL_MAX_SIZE) {
                allocatedCount--;
                //there are enough items in the pool
                //just release the obj to GC
                return;
            }

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
                if (_debugInfoReady) {
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