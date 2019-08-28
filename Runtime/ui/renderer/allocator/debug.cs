using System.Collections.Generic;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class DebugMeta {
        public string objName;
        public int watermark;
        public int prev_watermark;
        public int borrowed;
        public int allocated;

        public void onAlloc(int allocatedCount) {
            this.borrowed++;
            this.watermark = this.borrowed > this.watermark ? this.borrowed : this.watermark;
            this.allocated = allocatedCount;
        }

        public void onRelease(int allocatedCount) {
            this.borrowed--;
            this.allocated = allocatedCount;
        }
    }

    public static class AllocDebugger {
        public const bool enableDebugging = false;

        static int allocCount = 0;

        static readonly Dictionary<int, DebugMeta> debugInfos = new Dictionary<int, DebugMeta>();

        public static void onFrameEnd() {
            if (!enableDebugging) {
                return;
            }

            #pragma warning disable CS0162
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
            #pragma warning restore CS0162
        }

        public static void onAlloc(int objKey, string objName, int allocatedCount) {
            if (!debugInfos.ContainsKey(objKey)) {
                debugInfos[objKey] = new DebugMeta {
                    objName = objName,
                    watermark = 0,
                    borrowed = 0,
                    allocated = 0
                };
            }

            debugInfos[objKey].onAlloc(allocatedCount);
        }

        public static void onRelease(int objKey, string objName, int allocatedCount) {
            Debug.Assert(debugInfos.ContainsKey(objKey), "An unregistered pool object cannot be released");
            debugInfos[objKey].onRelease(allocatedCount);
        }
    }
}