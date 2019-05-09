using System;

namespace Unity.UIWidgets.ui {
    public class ArrayRef<T> {
        public T[] array;

        public int length;

        public ArrayRef() {
            this.array = Array.Empty<T>();
            this.length = 0;
        }

        public void add(T item) {
            if (this.length == this.array.Length) {
                int newCapacity = this.array.Length == 0 ? 4 : this.array.Length * 2;
                Array.Resize(ref this.array, newCapacity);
            }

            this.array[this.length++] = item;
        }
    }
}
