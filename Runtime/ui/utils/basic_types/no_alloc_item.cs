using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.ui {
    public static class ItemPoolManager {
        static readonly Dictionary<Type, List<PoolItem>> poolDict = new Dictionary<Type, List<PoolItem>>();

        public static T alloc<T>() where T : PoolItem, new() {
            if (!poolDict.ContainsKey(typeof(T))) {
                var pool = new List<PoolItem>(128);

                for (int i = 0; i < 128; i++) {
                    var item = new T();
                    item.Setup();
                    
                    pool.Add(item);
                }
                
                poolDict[typeof(T)] = pool;
            }

            var curPool = poolDict[typeof(T)];
            if (curPool.Count == 0) {
                for (int i = 0; i < 128; i++) {
                    var item = new T();
                    item.Setup();
                    
                    curPool.Add(item);
                }
            }

            var curItem = curPool[0];
            curPool.RemoveAt(0);
            
            return (T)curItem;
        }
        
        public static void recycle<T>(T item) where T : PoolItem {
            var typeofT = item.GetType();
            D.assert(poolDict.ContainsKey(typeofT));
            poolDict[typeofT].Add(item);
        }
    }

    public static class TestJob {
        public static void testJob() {
            var TestPoolItem = ItemPoolManager.alloc<TestPoolItem>();
            
            TestPoolItem.SetLength(2f);
            
            TestPoolItem.Dispose();
        }
    }

    public class TestPoolItem : PoolItem {
        float length;

        public void SetLength(float len) {
            this.length = len;
        }

        public override void Dispose() {
            this.length = 0;
            base.Dispose();
        }
    }

    public abstract class PoolItem {
        //ensure that base class has a empty constructor
        public PoolItem() {
            
        }

        public void Setup() {
            
        }

        public virtual void Dispose() {
            this.Recycle();
        }

        public void Recycle() {
            ItemPoolManager.recycle(this);
        }
    }
    
}