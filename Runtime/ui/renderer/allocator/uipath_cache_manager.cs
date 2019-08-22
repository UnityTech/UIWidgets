using System.Collections.Generic;

namespace Unity.UIWidgets.ui {
    public static class uiPathCacheManager {
        static readonly Dictionary<uint, uiPath> cache = new Dictionary<uint, uiPath>(256);

        //remove unused cache items every 1 frame
        static readonly Dictionary<uint, bool> touched = new Dictionary<uint, bool>(256);

        static float curFrame;

        static readonly List<uint> untouched = new List<uint>();

        public static void tickNextFrame() {
            untouched.Clear();
            foreach (var key in cache.Keys) {
                if (!touched.ContainsKey(key)) {
                    untouched.Add(key);
                }
            }

            foreach (var key in untouched) {
                ObjectPool<uiPath>.release(cache[key]);
                cache.Remove(key);
            }

            touched.Clear();
        }

        public static void putToCache(uiPath uipath) {
            if (!uipath.needCache) {
                ObjectPool<uiPath>.release(uipath);
            }
        }

        public static bool tryGetUiPath(uint pathKey, out uiPath outpath) {
            if (cache.ContainsKey(pathKey)) {
                touched[pathKey] = true;
                outpath = cache[pathKey];
                return true;
            }

            var uipath = uiPath.create();
            cache[pathKey] = uipath;
            touched[pathKey] = true;

            uipath.needCache = true;
            uipath.pathKey = pathKey;

            outpath = uipath;
            return false;
        }
    }
}