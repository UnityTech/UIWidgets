using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.Runtime.external
{
    public interface ISpatialData
    {
        ref readonly Rect Rect { get; }
    }
    
    
    	/// <summary>
	/// Non-generic class to produce instances of the generic class,
	/// optionally using type inference.
	/// </summary>
	public static class ProjectionComparer
	{
		/// <summary>
		/// Creates an instance of ProjectionComparer using the specified projection.
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
		/// Creates an instance of ProjectionComparer using the specified projection.
		/// The ignored parameter is solely present to aid type inference.
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
	/// Class generic in the source only to produce instances of the 
	/// doubly generic class, optionally using type inference.
	/// </summary>
	public static class ProjectionComparer<TSource>
	{
		/// <summary>
		/// Creates an instance of ProjectionComparer using the specified projection.
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
	/// Comparer which projects each element of the comparison to a key, and then compares
	/// those keys using the specified (or default) comparer for the key type.
	/// </summary>
	/// <typeparam name="TSource">Type of elements which this comparer will be asked to compare</typeparam>
	/// <typeparam name="TKey">Type of the key projected from the element</typeparam>
	public class ProjectionComparer<TSource, TKey> : IComparer<TSource>
	{
		readonly Func<TSource, TKey> projection;
		readonly IComparer<TKey> comparer;

		/// <summary>
		/// Creates a new instance using the specified projection, which must not be null.
		/// The default comparer for the projected type is used.
		/// </summary>
		/// <param name="projection">Projection to use during comparisons</param>
		public ProjectionComparer(Func<TSource, TKey> projection)
			: this(projection, null)
		{
		}

		/// <summary>
		/// Creates a new instance using the specified projection, which must not be null.
		/// </summary>
		/// <param name="projection">Projection to use during comparisons</param>
		/// <param name="comparer">The comparer to use on the keys. May be null, in
		/// which case the default comparer will be used.</param>
		public ProjectionComparer(Func<TSource, TKey> projection, IComparer<TKey> comparer)
		{
			this.comparer = comparer ?? Comparer<TKey>.Default;
			this.projection = projection;
		}

		/// <summary>
		/// Compares x and y by projecting them to keys and then comparing the keys. 
		/// Null values are not projected; they obey the
		/// standard comparer contract such that two null values are equal; any null value is
		/// less than any non-null value.
		/// </summary>
		public int Compare(TSource x, TSource y)
		{
			// Don't want to project from nullity
			if (x == null && y == null)
			{
				return 0;
			}
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return 1;
			}
			return comparer.Compare(projection(x), projection(y));
		}
	}
	
	public partial class RBush<T>
	{
		public class Node : ISpatialData
		{
			private Rect _Rect;

			internal Node(List<ISpatialData> items, int height)
			{
				this.Height = height;
				this.children = items;
				ResetRect();
			}

			internal void Add(ISpatialData node)
			{
				children.Add(node);
				_Rect = Rect.expandToInclude(node.Rect);
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

			internal readonly List<ISpatialData> children;

			public IReadOnlyList<ISpatialData> Children => children;
			public int Height { get; }
			public bool IsLeaf => Height == 1;
			public ref readonly Rect Rect => ref _Rect;
		}
	}
		public partial class RBush<T>
	{
		#region Sort Functions
		private static readonly IComparer<ISpatialData> CompareMinX =
			ProjectionComparer<ISpatialData>.Create(d => d.Rect.left);
		private static readonly IComparer<ISpatialData> CompareMinY =
			ProjectionComparer<ISpatialData>.Create(d => d.Rect.top);
		#endregion

		#region Search
		private List<T> DoSearch(in Rect boundingBox)
		{
			if (!Root.Rect.overlaps(boundingBox))
				return new List<T>();

			var intersections = new List<T>();
			var queue = new Queue<Node>();
			queue.Enqueue(Root);

			while (queue.Count != 0)
			{
				var item = queue.Dequeue();
				if (item.IsLeaf)
				{
					foreach (T leafChildItem in item.children.Cast<T>())
						if (leafChildItem.Rect.overlaps(boundingBox))
							intersections.Add(leafChildItem);
				}
				else
				{
					foreach (var child in item.children.Cast<Node>())
						if (child.Rect.overlaps(boundingBox))
							queue.Enqueue(child);
				}
			}

			return intersections;
		}
		#endregion

		#region Insert
		private List<Node> FindCoveringArea(in Rect area, int depth)
		{
			var path = new List<Node>();
			var node = this.Root;
			var _area = area; //FIX CS1628

			while (true)
			{
				path.Add(node);
				if (node.IsLeaf || path.Count == depth) return path;

				node = node.children
					.Select(c => new { EnlargedArea = c.Rect.expandToInclude(_area).area, c.Rect.area, Node = c as Node, })
					.OrderBy(x => x.EnlargedArea)
					.ThenBy(x => x.area)
					.Select(x => x.Node)
					.First();
			}
		}

		private void Insert(ISpatialData data, int depth)
		{
			var path = FindCoveringArea(data.Rect, depth);

			var insertNode = path.Last();
			insertNode.Add(data);

			while (--depth >= 0)
			{
				if (path[depth].children.Count > maxEntries)
				{
					var newNode = SplitNode(path[depth]);
					if (depth == 0)
						SplitRoot(newNode);
					else
						path[depth - 1].Add(newNode);
				}
				else
					path[depth].ResetRect();
			}
		}

		#region SplitNode
		private void SplitRoot(Node newNode) =>
			this.Root = new Node(new List<ISpatialData> { this.Root, newNode }, this.Root.Height + 1);

		private Node SplitNode(Node node)
		{
			SortChildren(node);

			var splitPoint = GetBestSplitIndex(node.children);
			var newChildren = node.children.Skip(splitPoint).ToList();
			node.RemoveRange(splitPoint, node.children.Count - splitPoint);
			return new Node(newChildren, node.Height);
		}

		#region SortChildren
		private void SortChildren(Node node)
		{
			node.children.Sort(CompareMinX);
			var splitsByX = GetPotentialSplitMargins(node.children);
			node.children.Sort(CompareMinY);
			var splitsByY = GetPotentialSplitMargins(node.children);

			if (splitsByX < splitsByY)
				node.children.Sort(CompareMinX);
		}

		private double GetPotentialSplitMargins(List<ISpatialData> children) =>
			GetPotentialEnclosingMargins(children) +
			GetPotentialEnclosingMargins(children.AsEnumerable().Reverse().ToList());

		private double GetPotentialEnclosingMargins(List<ISpatialData> children)
		{
			var rect = Rect.zero;
			int i = 0;
			for (; i < minEntries; i++)
			{
				rect = rect.expandToInclude(children[i].Rect);
			}

			var totalMargin = rect.margin;
			for (; i < children.Count - minEntries; i++)
			{
				rect = rect.expandToInclude(children[i].Rect);
				totalMargin += rect.margin;
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
					return new { i, overlap, totalArea };
				})
				.OrderBy(x => x.overlap)
				.ThenBy(x => x.totalArea)
				.Select(x => x.i)
				.First();
		}
		#endregion
		#endregion

		#region BuildTree
		private Node BuildTree(List<ISpatialData> data)
		{
			var treeHeight = GetDepth(data.Count);
			var rootMaxEntries = (int)Math.Ceiling(data.Count / Math.Pow(this.maxEntries, treeHeight - 1));
			return BuildNodes(data, 0, data.Count - 1, treeHeight, rootMaxEntries);
		}

		private int GetDepth(int numNodes) =>
			(int)Math.Ceiling(Math.Log(numNodes) / Math.Log(this.maxEntries));

		private Node BuildNodes(List<ISpatialData> data, int left, int right, int height, int maxEntries)
		{
			var num = right - left + 1;
			if (num <= maxEntries)
			{
				return height == 1
					? new Node(data.GetRange(left, num), height)
					: new Node(
						new List<ISpatialData>
						{
							BuildNodes(data, left, right, height - 1, this.maxEntries),
						},
						height);
			}

			data.Sort(left, num, CompareMinX);

			var nodeSize = (num + (maxEntries - 1)) / maxEntries;
			var subSortLength = nodeSize * (int)Math.Ceiling(Math.Sqrt(maxEntries));

			var children = new List<ISpatialData>(maxEntries);
			for (int subCounter = left; subCounter <= right; subCounter += subSortLength)
			{
				var subRight = Math.Min(subCounter + subSortLength - 1, right);
				data.Sort(subCounter, subRight - subCounter + 1, CompareMinY);

				for (int nodeCounter = subCounter; nodeCounter <= subRight; nodeCounter += nodeSize)
				{
					children.Add(
						BuildNodes(
							data,
							nodeCounter,
							Math.Min(nodeCounter + nodeSize - 1, subRight),
							height - 1,
							this.maxEntries));
				}
			}

			return new Node(children, height);
		}
		#endregion

		private static Rect GetEnclosingRect(IEnumerable<ISpatialData> items)
		{
			var rect = Rect.zero;
			foreach (var data in items)
			{
				rect = rect.expandToInclude(data.Rect);
			}
			return rect;
		}

		private List<T> GetAllChildren(List<T> list, Node n)
		{
			if (n.IsLeaf)
			{
				list.AddRange(
					n.children.Cast<T>());
			}
			else
			{
				foreach (var node in n.children.Cast<Node>())
					GetAllChildren(list, node);
			}

			return list;
		}

	}
	public partial class RBush<T> where T : ISpatialData
	{
		private const int DefaultMaxEntries = 9;
		private const int MinimumMaxEntries = 4;
		private const int MinimumMinEntries = 2;
		private const double DefaultFillFactor = 0.4;

		private readonly EqualityComparer<T> comparer;
		private readonly int maxEntries;
		private readonly int minEntries;

		public Node Root { get; private set; }
		public ref readonly Rect Rect => ref Root.Rect;

		public RBush() : this(DefaultMaxEntries) { }
		public RBush(int maxEntries)
			: this(maxEntries, EqualityComparer<T>.Default) { }
		public RBush(int maxEntries, EqualityComparer<T> comparer)
		{
			this.comparer = comparer;
			this.maxEntries = Math.Max(MinimumMaxEntries, maxEntries);
			this.minEntries = Math.Max(MinimumMinEntries, (int)Math.Ceiling(this.maxEntries * DefaultFillFactor));

			this.Clear();
		}

		public int Count { get; private set; }

		public void Clear()
		{
			this.Root = new Node(new List<ISpatialData>(), 1);
			this.Count = 0;
		}

		public IReadOnlyList<T> Search() => GetAllChildren(new List<T>(), this.Root);

		public IReadOnlyList<T> Search(in Rect boundingBox) =>
			DoSearch(boundingBox);

		public void Insert(T item)
		{
			Insert(item, this.Root.Height);
			this.Count++;
		}

		public void BulkLoad(IEnumerable<T> items)
		{
			var data = items.Cast<ISpatialData>().ToList();
			if (data.Count == 0) return;

			if (this.Root.IsLeaf &&
				this.Root.children.Count + data.Count < maxEntries)
			{
				foreach (var i in data)
					Insert((T)i);
				return;
			}

			if (data.Count < this.minEntries)
			{
				foreach (var i in data)
					Insert((T)i);
				return;
			}

			var dataRoot = BuildTree(data);
			this.Count += data.Count;

			if (this.Root.children.Count == 0)
				this.Root = dataRoot;
			else if (this.Root.Height == dataRoot.Height)
			{
				if (this.Root.children.Count + dataRoot.children.Count <= this.maxEntries)
				{
					foreach (var isd in dataRoot.children)
						this.Root.Add(isd);
				}
				else
					SplitRoot(dataRoot);
			}
			else
			{
				if (this.Root.Height < dataRoot.Height)
				{
					var tmp = this.Root;
					this.Root = dataRoot;
					dataRoot = tmp;
				}

				this.Insert(dataRoot, this.Root.Height - dataRoot.Height);
			}
		}
	}
}