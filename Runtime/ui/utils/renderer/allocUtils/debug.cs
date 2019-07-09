using System.Collections.Generic;
using UnityEngine;

namespace Unity.UIWidgets.ui {

    class DebugMeta {
        public string objName;
        public int watermark;
        public int prev_watermark;
        public int borrowed;

        public void onAlloc() {
            this.borrowed++;
            this.watermark = this.borrowed > this.watermark ? this.borrowed : this.watermark;
        }

        public void onRelease() {
            this.borrowed--;
        }
    }
    
    public static class AllocDebugger {

        public const bool enableDebugging = true;

        static int allocCount = 0;
        
        static readonly Dictionary<int, DebugMeta> debugInfos = new Dictionary<int, DebugMeta>();

        public static void onFrameEnd() {
            if (!enableDebugging) {
                return;
            }
            
            allocCount++;
            if (allocCount >= 120) {
                allocCount = 0;

                string debugInfo = "Alloc Stats: ";
                foreach (var key in debugInfos.Keys) {
                    var item = debugInfos[key];
                    if (item.watermark <= item.prev_watermark) {
                        debugInfo += "|" + item.objName + " = " + item.watermark + "," + item.borrowed + "|";
                        continue;
                    }

                    item.prev_watermark = item.watermark;
                    debugInfo += "|" + item.objName + " = " + item.watermark + "," + item.borrowed + "|";
                }

                if (debugInfo == "Alloc Stats: ") {
                    return;
                }
                Debug.Log(debugInfo);
            }
        }

        public static void onAlloc(int objKey, string objName) {
            if (!debugInfos.ContainsKey(objKey)) {
                debugInfos[objKey] = new DebugMeta {
                    objName = objName,
                    watermark = 0,
                    borrowed = 0
                };
            }

            debugInfos[objKey].onAlloc();
        }

        public static void onRelease(int objKey, string objName) {
            Debug.Assert(debugInfos.ContainsKey(objKey), "An unregistered pool object cannot be released");
            debugInfos[objKey].onRelease();
        }
    }
}