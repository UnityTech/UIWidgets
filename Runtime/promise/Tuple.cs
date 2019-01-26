namespace RSG {
    /// <summary>
    /// Provides static methods for creating tuple objects.
    /// 
    /// Tuple implementation for .NET 3.5
    /// </summary>
    public class Tuple {
        /// <summary>
        /// Create a new 2-tuple, or pair.
        /// </summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <returns>A 2-tuple whose value is (item1, item2)</returns>
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) {
            return new Tuple<T1, T2>(item1, item2);
        }

        /// <summary>
        /// Create a new 3-tuple, or triple.
        /// </summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <returns>A 3-tuple whose value is (item1, item2, item3)</returns>
        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }

        /// <summary>
        /// Create a new 4-tuple, or quadruple.
        /// </summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <returns>A 3-tuple whose value is (item1, item2, item3, item4)</returns>
        public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) {
            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }

    /// <summary>
    /// Represents a 2-tuple, or pair.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    public class Tuple<T1, T2> {
        internal Tuple(T1 item1, T2 item2) {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        /// <summary>
        /// Gets the value of the current tuple's first component.
        /// </summary>
        public T1 Item1 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's second component.
        /// </summary>
        public T2 Item2 { get; private set; }
    }

    /// <summary>
    /// Represents a 3-tuple, or triple.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    public class Tuple<T1, T2, T3> {
        internal Tuple(T1 item1, T2 item2, T3 item3) {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }

        /// <summary>
        /// Gets the value of the current tuple's first component.
        /// </summary>
        public T1 Item1 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's second component.
        /// </summary>
        public T2 Item2 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's third component.
        /// </summary>
        public T3 Item3 { get; private set; }
    }

    /// <summary>
    /// Represents a 4-tuple, or quadruple.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    public class Tuple<T1, T2, T3, T4> {
        internal Tuple(T1 item1, T2 item2, T3 item3, T4 item4) {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
        }

        /// <summary>
        /// Gets the value of the current tuple's first component.
        /// </summary>
        public T1 Item1 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's second component.
        /// </summary>
        public T2 Item2 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's third component.
        /// </summary>
        public T3 Item3 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's fourth component.
        /// </summary>
        public T4 Item4 { get; private set; }
    }
}