using System.Collections.Generic;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.utils {
    public static partial class GcCacheHelper {
        public static bool optimizing = false;
        
        public static void StartCaching() {
            optimizing = true;
        }

        public static void EndCaching() {
            optimizing = false;

            ListCache<PathPath>.instance.ClearAll();
            ListCache<PathPoint>.instance.ClearAll();
            ListCache<Vector3>.instance.ClearAll();
            ListCache<int>.instance.ClearAll();
            ListCache<float>.instance.ClearAll();
            ListCache<object>.instance.ClearAll();
            ListCache<PictureFlusher.RenderLayer>.instance.ClearAll();
            ListCache<PictureFlusher.State>.instance.ClearAll();
            
            ObjectCache<PathPoint>.instance.ClearAll();
            ObjectCache<Matrix3>.instance.ClearAll();
            ObjectCache<CanvasState>.instance.ClearAll();
            ObjectCache<PathPath>.instance.ClearAll();
            ObjectCache<Rect>.instance.ClearAll();
            ObjectCache<PictureFlusher.State>.instance.ClearAll();
            
            MaterialPropCache.instance.ClearAll();
            RecyclableObjectCache<PictureFlusher.CmdDraw>.instance.ClearAll();
            RecyclableObjectCache<DrawSave>.instance.ClearAll();
            RecyclableObjectCache<DrawSaveLayer>.instance.ClearAll();
            RecyclableObjectCache<DrawRestore>.instance.ClearAll();
            RecyclableObjectCache<DrawTranslate>.instance.ClearAll();
            RecyclableObjectCache<DrawScale>.instance.ClearAll();
            RecyclableObjectCache<DrawRotate>.instance.ClearAll();
            RecyclableObjectCache<DrawSkew>.instance.ClearAll();
            RecyclableObjectCache<DrawConcat>.instance.ClearAll();
            RecyclableObjectCache<DrawResetMatrix>.instance.ClearAll();
            RecyclableObjectCache<DrawSetMatrix>.instance.ClearAll();
            RecyclableObjectCache<DrawClipRect>.instance.ClearAll();
            RecyclableObjectCache<DrawClipRRect>.instance.ClearAll();
            RecyclableObjectCache<DrawClipPath>.instance.ClearAll();
            RecyclableObjectCache<DrawPath>.instance.ClearAll();
            RecyclableObjectCache<DrawImage>.instance.ClearAll();
            RecyclableObjectCache<DrawImageRect>.instance.ClearAll();
            RecyclableObjectCache<DrawImageNine>.instance.ClearAll();
            RecyclableObjectCache<DrawPicture>.instance.ClearAll();
            RecyclableObjectCache<DrawTextBlob>.instance.ClearAll();
        }
    }
    
    public interface GcRecyclable {
        void Recycle();
    }


    public class MaterialPropCache {
        static MaterialPropCache _instance;

        const int initial_size = 1024;
        const int delta_size = 128;
        int current_size = 0;

        public static MaterialPropCache instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new MaterialPropCache();
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

        public MaterialPropertyBlock Fetch() {
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

        public void ClearAll() {
            for (var i = 0; i < this.curIndex - 1; i++) {
                this.flash[i].Clear();
            }

            this.curIndex = 0;
        }
    }

    public class RecyclableObjectCache<T> where T : GcRecyclable, new() {
        static RecyclableObjectCache<T> _instance;

        const int initial_size = 1024;
        const int delta_size = 128;
        int current_size = 0;

        public static RecyclableObjectCache<T> instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new RecyclableObjectCache<T>();
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

        public T Fetch() {
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

        public void ClearAll() {
            for (var i = 0; i < this.curIndex - 1; i++) {
                this.flash[i].Recycle();
            }

            this.curIndex = 0;
        }
    }

    public class ObjectCache<T> where T : new() {
        static ObjectCache<T> _instance;

        const int initial_size = 1024;
        const int delta_size = 128;
        int current_size = 0;

        public static ObjectCache<T> instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new ObjectCache<T>();
                _instance.Setup();
                return _instance;
            }
        }

        List<T> flash;
        int curIndex;

        void Setup() {
            this.flash = new List<T>(initial_size);
            for (var i = 0; i < initial_size; i++) {
                this.flash.Add(new T());
            }

            this.current_size = this.flash.Count;
        }

        public T Fetch() {
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

        public void ClearAll() {
            this.curIndex = 0;
        }
    }

    public class ListCache<T> {
        static ListCache<T> _instance;

        const int initial_size = 1024;
        const int delta_size = 128;
        int current_size = 0;

        public static ListCache<T> instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new ListCache<T>();
                _instance.Setup();
                return _instance;
            }
        }

        List<List<T>> flash;
        int curIndex;

        void Setup() {
            this.flash = new List<List<T>>(initial_size);
            for (var i = 0; i < initial_size; i++) {
                this.flash.Add(new List<T>(256));
            }

            this.current_size = this.flash.Count;
        }

        public List<T> Fetch(int len) {
            if (!GcCacheHelper.optimizing) {
                return len != 0 ? new List<T>(len) : new List<T>();
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

        public void ClearAll() {
            this.curIndex = 0;
        }
    }

    public class ObjectPool<T> where T : new() {
        static ObjectPool<T> _instance;

        const int initial_size = 256;
        const int delta_size = initial_size / 2;
        int current_size = 0;

        public static ObjectPool<T> instance {
            get {
                if (_instance != null) {
                    return _instance;
                }

                _instance = new ObjectPool<T>();
                _instance.Setup();
                return _instance;
            }
        }

        Stack<T> pool;

        void Setup() {
            this.pool = new Stack<T>(initial_size * 2);
            for (var i = 0; i < initial_size; i++) {
                this.pool.Push(new T());
            }

            this.current_size = this.pool.Count;
        }

        public T Fetch() {
            if (!GcCacheHelper.optimizing) {
                return new T();
            }
            
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

        public void Recycle(T obj) {
            if (obj != null) {
                this.pool.Push(obj);
                this.current_size++;
            }
        }
    }
}