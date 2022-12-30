using System;
using System.Collections;
using System.Collections.Generic;

namespace AoC22;

/// <summary>
/// A basic implementation for automatically keeping a list ordered according to a Comparer.
/// This was made because SortedList is based off key-value pairs and SortedSet doesn't allow duplicates or ties.
/// </summary>
public class OrderedList<T> : IList<T>
{
    private readonly IComparer<T> _comparer;
    private readonly List<T> _innerList = new();

    public OrderedList() : this(Comparer<T>.Default) { }

    public OrderedList(IComparer<T> comparer) => _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

    public T this[int index]
    {
        get => _innerList[index];
        set => throw new NotSupportedException("Cannot set an indexed item in an Ordered List.");
    }

    public T Min => this[0];
    public T Max => this[^1];

    public int Count => _innerList.Count;
    public bool IsReadOnly => false;

    public void Add(T item)
    {
        int index = _innerList.BinarySearch(item, _comparer);
        if (index < 0) index = ~index;
        _innerList.Insert(index, item);
    }

    public void Clear() => _innerList.Clear();
    public bool Contains(T item) => _innerList.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _innerList.CopyTo(array);
    public IEnumerator<T> GetEnumerator() => _innerList.GetEnumerator();
    public int IndexOf(T item) => _innerList.IndexOf(item);
    public void Insert(int index, T item) => throw new NotSupportedException("Cannot insert an indexed item in an Ordered List.");
    public bool Remove(T item) => _innerList.Remove(item);
    public void RemoveAt(int index) => _innerList.RemoveAt(index);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}