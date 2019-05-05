using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public static class NoAllocHelpers<T> {

        static Func<List<T>, T[]> _extractArrayFromListDelegate;

        static Action<List<T>, int> _resizeListDelegate;

        public static T[] ExtractArrayFromListT(List<T> list) {
            if (_extractArrayFromListDelegate == null) {
                var ass = Assembly.GetAssembly(typeof(Mesh));
                var type = ass.GetType("UnityEngine.NoAllocHelpers");
                var methodInfo = type.GetMethod(
                        "ExtractArrayFromListT",
                        BindingFlags.Static | BindingFlags.Public)
                    .MakeGenericMethod(typeof(T));

                _extractArrayFromListDelegate = (Func<List<T>, T[]>)
                    Delegate.CreateDelegate(typeof(Func<List<T>, T[]>), methodInfo);
            }

            return _extractArrayFromListDelegate(list);
        }

        public static void ResizeList(List<T> list, int size) {
            if (size < list.Count) {
                list.RemoveRange(size, list.Count - size);
                return;
            }

            if (size == list.Count) {
                return;
            }
            
            if (list.Capacity < size) {
                list.Capacity = size;
            }

            if (_resizeListDelegate == null) {
                var ass = Assembly.GetAssembly(typeof(Mesh)); // any class in UnityEngine
                var type = ass.GetType("UnityEngine.NoAllocHelpers");
                var methodInfo = type.GetMethod(
                        "ResizeList",
                        BindingFlags.Static | BindingFlags.Public)
                    .MakeGenericMethod(typeof(T));
                _resizeListDelegate = (Action<List<T>, int>)
                    Delegate.CreateDelegate(typeof(Action<List<T>, int>), methodInfo);
            }

            _resizeListDelegate(list, size);
        }

        public static void EnsureListElemCount(List<T> list, int size) {
            list.Clear();
            if (list.Capacity < size) {
                list.Capacity = size;
            }

            ResizeList(list, size);
        }
    }
}
