using System.Collections.Generic;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Unity.UIWidgets.utils {
    public static class GcCacheHelper {
        public static bool optimizing = false;

        public static void StartCaching() {
            optimizing = true;
        }

        public static void EndCaching() {
            optimizing = false;

            Flash<PathPath>.instance.clearAll();
            Flash<PathPoint>.instance.clearAll();
            Flash<Vector3>.instance.clearAll();
            Flash<int>.instance.clearAll();
            Flash<float>.instance.clearAll();
            SimpleFlash<PathPoint>.instance.clearAll();
            SimpleFlash<Matrix3>.instance.clearAll();
            SimpleFlash<CanvasState>.instance.clearAll();
            SimpleFlash<PathPath>.instance.clearAll();
            SimpleFlash<Rect>.instance.clearAll();
            ClearableSimpleFlash<PictureFlusher.CmdDraw>.instance.clearAll();
            ClearableMaterialPropFlash.instance.clearAll();
            Flash<object>.instance.clearAll();
            Flash<PictureFlusher.RenderLayer>.instance.clearAll();
            Flash<PictureFlusher.State>.instance.clearAll();
            SimpleFlash<PictureFlusher.State>.instance.clearAll();
            ClearableSimpleFlash<DrawSave>.instance.clearAll();
            ClearableSimpleFlash<DrawSaveLayer>.instance.clearAll();
            ClearableSimpleFlash<DrawRestore>.instance.clearAll();
            ClearableSimpleFlash<DrawTranslate>.instance.clearAll();
            ClearableSimpleFlash<DrawScale>.instance.clearAll();
            ClearableSimpleFlash<DrawRotate>.instance.clearAll();
            ClearableSimpleFlash<DrawSkew>.instance.clearAll();
            ClearableSimpleFlash<DrawConcat>.instance.clearAll();
            ClearableSimpleFlash<DrawResetMatrix>.instance.clearAll();
            ClearableSimpleFlash<DrawSetMatrix>.instance.clearAll();
            ClearableSimpleFlash<DrawClipRect>.instance.clearAll();
            ClearableSimpleFlash<DrawClipRRect>.instance.clearAll();
            ClearableSimpleFlash<DrawClipPath>.instance.clearAll();
            ClearableSimpleFlash<DrawPath>.instance.clearAll();
            ClearableSimpleFlash<DrawImage>.instance.clearAll();
            ClearableSimpleFlash<DrawImageRect>.instance.clearAll();
            ClearableSimpleFlash<DrawImageNine>.instance.clearAll();
            ClearableSimpleFlash<DrawPicture>.instance.clearAll();
            ClearableSimpleFlash<DrawTextBlob>.instance.clearAll();
        }
    }

    public interface Clearable {
        void clear();
    }


    public class ClearableMaterialPropFlash {
        static ClearableMaterialPropFlash _instance;

        const int initial_size = 1024;
        const int delta_size = 128;
        int current_size = 0;

        public static ClearableMaterialPropFlash instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new ClearableMaterialPropFlash();
                _instance.setup();
                return _instance;
            }
        }

        List<MaterialPropertyBlock> flash;
        int curIndex;

        void setup() {
            this.flash = new List<MaterialPropertyBlock>(initial_size);
            for (var i = 0; i < initial_size; i++) {
                this.flash.Add(new MaterialPropertyBlock());
            }

            this.current_size = this.flash.Count;
        }

        public MaterialPropertyBlock fetch() {
            if (!GcCacheHelper.optimizing) {
                return new MaterialPropertyBlock();
            }

            if (this.curIndex >= this.current_size) {
                for (var i = 0; i < delta_size; i++) {
                    this.flash.Add(new MaterialPropertyBlock());
                }

                this.current_size = this.flash.Count;
            }

            var ret = this.flash[this.curIndex++];
            return ret;
        }

        public void clearAll() {
            for (var i = 0; i < this.curIndex - 1; i++) {
                this.flash[i].Clear();
            }

            this.curIndex = 0;
        }
    }

    public class ClearableSimpleFlash<T> where T : Clearable, new() {
        static ClearableSimpleFlash<T> _instance;

        const int initial_size = 1024;
        const int delta_size = 128;
        int current_size = 0;

        public static ClearableSimpleFlash<T> instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new ClearableSimpleFlash<T>();
                _instance.setup();
                return _instance;
            }
        }

        List<T> flash;
        int curIndex;

        void setup() {
            this.flash = new List<T>(initial_size);
            for (var i = 0; i < initial_size; i++) {
                this.flash.Add(new T());
            }

            this.current_size = this.flash.Count;
        }

        public T fetch() {
            if (!GcCacheHelper.optimizing) {
                return new T();
            }

            if (this.curIndex >= this.current_size) {
                for (var i = 0; i < delta_size; i++) {
                    this.flash.Add(new T());
                }

                this.current_size = this.flash.Count;
            }

            var ret = this.flash[this.curIndex++];
            return ret;
        }

        public void clearAll() {
            for (var i = 0; i < this.curIndex - 1; i++) {
                this.flash[i].clear();
            }

            this.curIndex = 0;
        }
    }

    public class SimpleFlash<T> where T : new() {
        static SimpleFlash<T> _instance;

        const int initial_size = 1024;
        const int delta_size = 128;
        int current_size = 0;

        public static SimpleFlash<T> instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new SimpleFlash<T>();
                _instance.setup();
                return _instance;
            }
        }

        List<T> flash;
        int curIndex;

        void setup() {
            this.flash = new List<T>(initial_size);
            for (var i = 0; i < initial_size; i++) {
                this.flash.Add(new T());
            }

            this.current_size = this.flash.Count;
        }

        public T fetch() {
            if (!GcCacheHelper.optimizing) {
                return new T();
            }

            if (this.curIndex >= this.current_size) {
                for (var i = 0; i < delta_size; i++) {
                    this.flash.Add(new T());
                }

                this.current_size = this.flash.Count;
            }

            var ret = this.flash[this.curIndex++];
            return ret;
        }

        public void clearAll() {
            this.curIndex = 0;
        }
    }

    public class Flash<T> {
        static Flash<T> _instance;

        const int initial_size = 1024;
        const int delta_size = 128;
        int current_size = 0;

        public static Flash<T> instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new Flash<T>();
                _instance.setup();
                return _instance;
            }
        }

        List<List<T>> flash;
        int curIndex;

        void setup() {
            this.flash = new List<List<T>>(initial_size);
            for (var i = 0; i < initial_size; i++) {
                this.flash.Add(new List<T>(256));
            }

            this.current_size = this.flash.Count;
        }

        public List<T> fetch() {
            if (!GcCacheHelper.optimizing) {
                return new List<T>();
            }

            if (this.curIndex >= this.current_size) {
                for (var i = 0; i < delta_size; i++) {
                    this.flash.Add(new List<T>(256));
                }

                this.current_size = this.flash.Count;
            }


            var ret = this.flash[this.curIndex++];
            ret.Clear();
            return ret;
        }

        public void clearAll() {
            this.curIndex = 0;
        }
    }

    public class SimplePool<T> where T : new() {
        static SimplePool<T> _instance;

        const int initial_size = 256;
        const int delta_size = initial_size / 2;
        int current_size = 0;

        public static SimplePool<T> instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new SimplePool<T>();
                _instance.setup();
                return _instance;
            }
        }

        Stack<T> pool;

        void setup() {
            this.pool = new Stack<T>(initial_size * 2);
            for (var i = 0; i < initial_size; i++) {
                this.pool.Push(new T());
            }

            this.current_size = this.pool.Count;
        }

        public T fetch() {
            if (this.current_size == 0) {
                for (var i = 0; i < delta_size; i++) {
                    this.pool.Push(new T());
                }

                this.current_size = this.pool.Count;
            }

            var ret = this.pool.Pop();
            this.current_size--;
            return ret;
        }

        public void recycle(T obj) {
            if (obj != null) {
                this.pool.Push(obj);
                this.current_size++;
            }
        }
    }
}