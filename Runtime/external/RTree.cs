using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.Runtime.external
{
    public interface ISpatialData
    {
        uiRect bounds { get; }
    }

    public class IndexedRect : ISpatialData
    {
        private uiRect _bounds;

        public uiRect bounds
        {
            get { return _bounds; }
        }

        public readonly int index;

        public IndexedRect(uiRect bounds, int index)
        {
            this._bounds = bounds;
            this.index = index;
        }
    }

    /// <summary>
    ///     Non-generic class to produce instances of the generic class,
    ///     optionally using type inference.
    /// </summary>
    public static class ProjectionComparer
    {
	    /// <summary>
	    ///     Creates an instance of ProjectionComparer using the specified projection.
	    /// </summary>
	    /// <typeparam name="TSource">Type parameter for the elements to be compared</typeparam>
	    /// <typeparam name="TKey">Type parameter for the keys to be compared, after being projected from the elements</typeparam>
	    /// <param name="projection">Projection to use when determining the key of an element</param>
	    /// <returns>A comparer which will compare elements by projecting each element to its key, and comparing keys</returns>
	    public static ProjectionComparer<TSource, TKey> Create<TSource, TKey>(Func<TSource, TKey> projection)
        {
            return new ProjectionComparer<TSource, TKey>(projection);
        }

	    /// <summary>
	    ///     Creates an instance of ProjectionComparer using the specified projection.
	    ///     The ignored parameter is solely present to aid type inference.
	    /// </summary>
	    /// <typeparam name="TSource">Type parameter for the elements to be compared</typeparam>
	    /// <typeparam name="TKey">Type parameter for the keys to be compared, after being projected from the elements</typeparam>
	    /// <param name="ignored">Value is ignored - type may be used by type inference</param>
	    /// <param name="projection">Projection to use when determining the key of an element</param>
	    /// <returns>A comparer which will compare elements by projecting each element to its key, and comparing keys</returns>
	    public static ProjectionComparer<TSource, TKey> Create<TSource, TKey>
        (TSource ignored,
            Func<TSource, TKey> projection)
        {
            return new ProjectionComparer<TSource, TKey>(projection);
        }
    }

    /// <summary>
    ///     Class generic in the source only to produce instances of the
    ///     doubly generic class, optionally using type inference.
    /// </summary>
    public static class ProjectionComparer<TSource>
    {
	    /// <summary>
	    ///     Creates an instance of ProjectionComparer using the specified projection.
	    /// </summary>
	    /// <typeparam name="TKey">Type parameter for the keys to be compared, after being projected from the elements</typeparam>
	    /// <param name="projection">Projection to use when determining the key of an element</param>
	    /// <returns>A comparer which will compare elements by projecting each element to its key, and comparing keys</returns>
	    public static ProjectionComparer<TSource, TKey> Create<TKey>(Func<TSource, TKey> projection)
        {
            return new ProjectionComparer<TSource, TKey>(projection);
        }
    }

    /// <summary>
    ///     Comparer which projects each element of the comparison to a key, and then compares
    ///     those keys using the specified (or default) comparer for the key type.
    /// </summary>
    /// <typeparam name="TSource">Type of elements which this comparer will be asked to compare</typeparam>
    /// <typeparam name="TKey">Type of the key projected from the element</typeparam>
    public class ProjectionComparer<TSource, TKey> : IComparer<TSource>
    {
        private readonly IComparer<TKey> comparer;
        private readonly Func<TSource, TKey> projection;

        /// <summary>
        ///     Creates a new instance using the specified projection, which must not be null.
        ///     The default comparer for the projected type is used.
        /// </summary>
        /// <param name="projection">Projection to use during comparisons</param>
        public ProjectionComparer(Func<TSource, TKey> projection)
            : this(projection, null)
        {
        }

        /// <summary>
        ///     Creates a new instance using the specified projection, which must not be null.
        /// </summary>
        /// <param name="projection">Projection to use during comparisons</param>
        /// <param name="comparer">
        ///     The comparer to use on the keys. May be null, in
        ///     which case the default comparer will be used.
        /// </param>
        public ProjectionComparer(Func<TSource, TKey> projection, IComparer<TKey> comparer)
        {
            this.comparer = comparer ?? Comparer<TKey>.Default;
            this.projection = projection;
        }

        /// <summary>
        ///     Compares x and y by projecting them to keys and then comparing the keys.
        ///     Null values are not projected; they obey the
        ///     standard comparer contract such that two null values are equal; any null value is
        ///     less than any non-null value.
        /// </summary>
        public int Compare(TSource x, TSource y)
        {
            // Don't want to project from nullity
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return comparer.Compare(projection(x), projection(y));
        }
    }

    public interface BBoxHierarchy<T> where T : ISpatialData
    {
        IReadOnlyList<T> Search(in uiRect boundingBox);
        void BulkLoad(IEnumerable<T> items);

        void Insert(T data);

        void Clear();
    }

    public class RTree<T> : BBoxHierarchy<T> where T : ISpatialData
    {
        public class RTreeNode : ISpatialData
        {
            internal readonly List<ISpatialData> children;
            private uiRect _Rect;

            internal RTreeNode(List<ISpatialData> items, int height)
            {
                Height = height;
                children = items;
                ResetRect();
            }

            public IReadOnlyList<ISpatialData> Children => children;
            public int Height { get; }
            public bool IsLeaf => Height == 1;
            public uiRect bounds => _Rect;

            internal void Add(ISpatialData node)
            {
                children.Add(node);
                _Rect = bounds.expandToInclude(node.bounds);
            }

            internal void Remove(ISpatialData node)
            {
                children.Remove(node);
                ResetRect();
            }

            internal void RemoveRange(int index, int count)
            {
                children.RemoveRange(index, count);
                ResetRect();
            }

            internal void ResetRect()
            {
                _Rect = GetEnclosingRect(children);
            }
        }
        #region Search

        private List<T> DoSearch(in uiRect boundingBox)
        {
            if (!uiRectHelper.overlaps(Root.bounds, boundingBox))
                return new List<T>();

            var intersections = new List<T>();
            var queue = new Queue<RTreeNode>();
            queue.Enqueue(Root);

            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                if (item.IsLeaf)
                {
                    foreach (var leafChildItem in item.children.Cast<T>())
                        if (uiRectHelper.overlaps(leafChildItem.bounds, boundingBox))
                            intersections.Add(leafChildItem);
                }
                else
                {
                    foreach (var child in item.children.Cast<RTreeNode>())
                        if (uiRectHelper.overlaps(child.bounds, boundingBox))
                            queue.Enqueue(child);
                }
            }

            return intersections;
        }

        #endregion

        private static uiRect GetEnclosingRect(IEnumerable<ISpatialData> items)
        {
            var uiRect = uiRectHelper.zero;
            foreach (var data in items) uiRect = uiRect.expandToInclude(data.bounds);
            return uiRect;
        }

        private List<T> GetAllChildren(List<T> list, RTreeNode n)
        {
            if (n.IsLeaf)
                list.AddRange(
                    n.children.Cast<T>());
            else
                foreach (var node in n.children.Cast<RTreeNode>())
                    GetAllChildren(list, node);

            return list;
        }

        #region Sort Functions

        private static readonly IComparer<ISpatialData> CompareMinX =
            ProjectionComparer<ISpatialData>.Create(d => d.bounds.left);

        private static readonly IComparer<ISpatialData> CompareMinY =
            ProjectionComparer<ISpatialData>.Create(d => d.bounds.top);

        #endregion

        #region Insert

        private List<RTreeNode> FindCoveringArea(in uiRect area, int depth)
        {
            var path = new List<RTreeNode>();
            var node = Root;
            var _area = area; //FIX CS1628

            while (true)
            {
                path.Add(node);
                if (node.IsLeaf || path.Count == depth) return path;

                node = node.children
                    .Select(c => new
                        {EnlargedArea = c.bounds.expandToInclude(_area).area, c.bounds.area, Node = c as RTreeNode})
                    .OrderBy(x => x.EnlargedArea)
                    .ThenBy(x => x.area)
                    .Select(x => x.Node)
                    .First();
            }
        }

        private void Insert(ISpatialData data, int depth)
        {
            var path = FindCoveringArea(data.bounds, depth);

            var insertNode = path.Last();
            insertNode.Add(data);

            while (--depth >= 0)
                if (path[depth].children.Count > maxEntries)
                {
                    var newNode = SplitNode(path[depth]);
                    if (depth == 0)
                        SplitRoot(newNode);
                    else
                        path[depth - 1].Add(newNode);
                }
                else
                {
                    path[depth].ResetRect();
                }
        }

        #region SplitNode

        private void SplitRoot(RTreeNode newRTreeNode)
        {
            Root = new RTreeNode(new List<ISpatialData> {Root, newRTreeNode}, Root.Height + 1);
        }

        private RTreeNode SplitNode(RTreeNode rTreeNode)
        {
            SortChildren(rTreeNode);

            var splitPoint = GetBestSplitIndex(rTreeNode.children);
            var newChildren = rTreeNode.children.Skip(splitPoint).ToList();
            rTreeNode.RemoveRange(splitPoint, rTreeNode.children.Count - splitPoint);
            return new RTreeNode(newChildren, rTreeNode.Height);
        }

        #region SortChildren

        private void SortChildren(RTreeNode rTreeNode)
        {
            rTreeNode.children.Sort(CompareMinX);
            var splitsByX = GetPotentialSplitMargins(rTreeNode.children);
            rTreeNode.children.Sort(CompareMinY);
            var splitsByY = GetPotentialSplitMargins(rTreeNode.children);

            if (splitsByX < splitsByY)
                rTreeNode.children.Sort(CompareMinX);
        }

        private float GetPotentialSplitMargins(List<ISpatialData> children)
        {
            return GetPotentialEnclosingMargins(children) +
                   GetPotentialEnclosingMargins(children.AsEnumerable().Reverse().ToList());
        }

        private float GetPotentialEnclosingMargins(List<ISpatialData> children)
        {
            var uiRect = uiRectHelper.zero;
            var i = 0;
            for (; i < minEntries; i++) uiRect = uiRect.expandToInclude(children[i].bounds);

            var totalMargin = uiRect.margin;
            for (; i < children.Count - minEntries; i++)
            {
                uiRect = uiRect.expandToInclude(children[i].bounds);
                totalMargin += uiRect.margin;
            }

            return totalMargin;
        }

        #endregion

        private int GetBestSplitIndex(List<ISpatialData> children)
        {
            return Enumerable.Range(minEntries, children.Count - minEntries)
                .Select(i =>
                {
                    var leftRect = GetEnclosingRect(children.Take(i));
                    var rightRect = GetEnclosingRect(children.Skip(i));

                    var overlap = leftRect.intersect(rightRect).area;
                    var totalArea = leftRect.area + rightRect.area;
                    return new {i, overlap, totalArea};
                })
                .OrderBy(x => x.overlap)
                .ThenBy(x => x.totalArea)
                .Select(x => x.i)
                .First();
        }

        #endregion

        #endregion

        #region BuildTree

        private RTreeNode BuildTree(List<ISpatialData> data)
        {
            var treeHeight = GetDepth(data.Count);
            var rootMaxEntries = (int) Math.Ceiling(data.Count / Math.Pow(maxEntries, treeHeight - 1));
            return BuildNodes(data, 0, data.Count - 1, treeHeight, rootMaxEntries);
        }

        private int GetDepth(int numNodes)
        {
            return (int) Math.Ceiling(Math.Log(numNodes) / Math.Log(maxEntries));
        }

        private RTreeNode BuildNodes(List<ISpatialData> data, int left, int right, int height, int maxEntries)
        {
            var num = right - left + 1;
            if (num <= maxEntries)
                return height == 1
                    ? new RTreeNode(data.GetRange(left, num), height)
                    : new RTreeNode(
                        new List<ISpatialData>
                        {
                            BuildNodes(data, left, right, height - 1, this.maxEntries)
                        },
                        height);

            data.Sort(left, num, CompareMinX);

            var nodeSize = (num + (maxEntries - 1)) / maxEntries;
            var subSortLength = nodeSize * (int) Math.Ceiling(Math.Sqrt(maxEntries));

            var children = new List<ISpatialData>(maxEntries);
            for (var subCounter = left; subCounter <= right; subCounter += subSortLength)
            {
                var subRight = Math.Min(subCounter + subSortLength - 1, right);
                data.Sort(subCounter, subRight - subCounter + 1, CompareMinY);

                for (var nodeCounter = subCounter; nodeCounter <= subRight; nodeCounter += nodeSize)
                    children.Add(
                        BuildNodes(
                            data,
                            nodeCounter,
                            Math.Min(nodeCounter + nodeSize - 1, subRight),
                            height - 1,
                            this.maxEntries));
            }

            return new RTreeNode(children, height);
        }

        #endregion
        private const int DefaultMaxEntries = 9;
        private const int MinimumMaxEntries = 4;
        private const int MinimumMinEntries = 2;
        private const float DefaultFillFactor = 0.4f;

        private readonly EqualityComparer<T> comparer;
        private readonly int maxEntries;
        private readonly int minEntries;

        public RTree() : this(DefaultMaxEntries)
        {
        }

        public RTree(int maxEntries)
            : this(maxEntries, EqualityComparer<T>.Default)
        {
        }

        public RTree(int maxEntries, EqualityComparer<T> comparer)
        {
            this.comparer = comparer;
            this.maxEntries = Math.Max(MinimumMaxEntries, maxEntries);
            minEntries = Math.Max(MinimumMinEntries, (int) Math.Ceiling(this.maxEntries * DefaultFillFactor));

            Clear();
        }

        public RTreeNode Root { get; private set; }
        public uiRect uiRect => Root.bounds;

        public int Count { get; private set; }

        public void Clear()
        {
            Root = new RTreeNode(new List<ISpatialData>(), 1);
            Count = 0;
        }

        public IReadOnlyList<T> Search()
        {
            return GetAllChildren(new List<T>(), Root);
        }

        public IReadOnlyList<T> Search(in uiRect boundingBox)
        {
            return DoSearch(boundingBox);
        }

        public void Insert(T item)
        {
            Insert(item, Root.Height);
            Count++;
        }

        public void BulkLoad(IEnumerable<T> items)
        {
            var data = items.Cast<ISpatialData>().ToList();
            if (data.Count == 0) return;

            if (Root.IsLeaf &&
                Root.children.Count + data.Count < maxEntries)
            {
                foreach (var i in data)
                    Insert((T) i);
                return;
            }

            if (data.Count < minEntries)
            {
                foreach (var i in data)
                    Insert((T) i);
                return;
            }

            var dataRoot = BuildTree(data);
            Count += data.Count;

            if (Root.children.Count == 0)
            {
                Root = dataRoot;
            }
            else if (Root.Height == dataRoot.Height)
            {
                if (Root.children.Count + dataRoot.children.Count <= maxEntries)
                    foreach (var isd in dataRoot.children)
                        Root.Add(isd);
                else
                    SplitRoot(dataRoot);
            }
            else
            {
                if (Root.Height < dataRoot.Height)
                {
                    var tmp = Root;
                    Root = dataRoot;
                    dataRoot = tmp;
                }

                Insert(dataRoot, Root.Height - dataRoot.Height);
            }
        }
    }
}