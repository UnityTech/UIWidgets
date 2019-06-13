using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {

    public class ItemDebugInfo {
        public int _watermark;
        public int _old_watermark;
        public int _size;

        public void consume() {
            this._size++;
            if (this._size > this._watermark) {
                this._watermark = this._size;
            }
        }

        public void recycle() {
            this._size--;
        }
    }
    
    public static class ItemPoolManager {
        static readonly Dictionary<Type, List<PoolItem>> poolDict = new Dictionary<Type, List<PoolItem>>();
        static readonly Dictionary<Type, ItemDebugInfo> debugInfo = new Dictionary<Type, ItemDebugInfo>();

        const bool _debugFlag = true;

        static int _allocTick = 0;

        public static void showDebugInfo() {
            string info = "";
            foreach (var key in debugInfo.Keys) {
                if (debugInfo[key]._old_watermark == debugInfo[key]._watermark) {
                    continue;
                }
                info += "| " + key + " = " + debugInfo[key]._watermark + " |\n";
                debugInfo[key]._old_watermark = debugInfo[key]._watermark;
            }
            
            Debug.Log(info);
        }

        public static T alloc<T>() where T : PoolItem, new() {

            if (_debugFlag) {
                if (_allocTick >= 5000) {
                    showDebugInfo();
                    _allocTick = 0;
                }

                _allocTick++;
            }

            if (!poolDict.ContainsKey(typeof(T))) {
                var pool = new List<PoolItem>(128);

                for (int i = 0; i < 128; i++) {
                    var item = new T();
                    item.setup();
                    
                    pool.Add(item);
                }
                
                poolDict[typeof(T)] = pool;

                if (_debugFlag) {
                    debugInfo[typeof(T)] = new ItemDebugInfo {_watermark = 0, _size = 0};
                }
            }

            var curPool = poolDict[typeof(T)];
            if (curPool.Count == 0) {
                for (int i = 0; i < 128; i++) {
                    var item = new T();
                    item.setup();
                    
                    curPool.Add(item);
                }
            }

            var curItem = curPool[0];
            curPool.RemoveAt(0);

            if (_debugFlag) {
                debugInfo[typeof(T)].consume();
            }
            
            curItem.activate();
            return (T)curItem;
        }
        
        public static void recycle<T>(T item) where T : PoolItem {
            var typeofT = item.GetType();
            D.assert(poolDict.ContainsKey(typeofT));

            poolDict[typeofT].Add(item);

            if (_debugFlag) {
                debugInfo[typeofT].recycle();
            }
        }
    }

    public abstract class PoolItem {
        //ensure that base class has a empty constructor
        bool __activated_flag;
        
        public PoolItem() {
            
        }

        public void activate() {
            this.__activated_flag = true;
        }

        public virtual void setup() {
            
        }

        public virtual void clear() {
            
        }

        public void dispose() {
            if (!this.__activated_flag) {
                Debug.Assert(false, "an item has been recycled more than once !");
                return;
            }
            
            this.clear();
            this.recycle();
            this.__activated_flag = false;
        }

        public void recycle() {
            ItemPoolManager.recycle(this);
        }
    }
    
}