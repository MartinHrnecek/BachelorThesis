//===========================================================================
// File:        Repository.cs
// Project:     CliApp
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Generic in-memory repository providing add, remove, filter
//              and sort operations over a typed item collection. Used to
//              exercise generic types and lambda predicates in IL code,
//              which the obfuscators must preserve in metadata.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace CliApp;

/// <summary>
/// In-memory generic repository storing items of type
/// <typeparamref name="T"/> and supporting basic CRUD-like operations,
/// filtering and sorting.
/// </summary>
/// <typeparam name="T">The type of items held by the repository.</typeparam>
public class Repository<T>
{
    private readonly List<T> _items = new();
    private readonly string _name;

    /// <summary>
    /// Initializes a new <see cref="Repository{T}"/> with an optional name.
    /// </summary>
    /// <param name="name">The display name of the repository.</param>
    public Repository(string name = "DefaultRepo")
    {
        _name = name;
    }

    /// <summary>Adds an item to the repository.</summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item) => _items.Add(item);

    /// <summary>Removes the first occurrence of an item from the repository.</summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><c>true</c> if the item was found and removed; otherwise <c>false</c>.</returns>
    public bool Remove(T item) => _items.Remove(item);

    /// <summary>Returns the items matching the given predicate.</summary>
    /// <param name="predicate">The filter applied to each stored item.</param>
    /// <returns>An enumeration of items satisfying <paramref name="predicate"/>.</returns>
    public IEnumerable<T> GetFiltered(Func<T, bool> predicate)
        => _items.Where(predicate);

    /// <summary>Returns the items ordered by the given key selector.</summary>
    /// <typeparam name="TKey">The type of the sort key.</typeparam>
    /// <param name="keySelector">A function that extracts the sort key from each item.</param>
    /// <returns>An enumeration of items sorted in ascending key order.</returns>
    public IEnumerable<T> GetSorted<TKey>(Func<T, TKey> keySelector)
        => _items.OrderBy(keySelector);

    /// <summary>Gets the number of items currently held by the repository.</summary>
    public int Count => _items.Count;

    /// <summary>Returns the first item matching the given predicate.</summary>
    /// <param name="predicate">The filter applied to each stored item.</param>
    /// <returns>The first matching item, or <c>default</c> if none is found.</returns>
    public T? FindFirst(Func<T, bool> predicate)
        => _items.FirstOrDefault(predicate);

    /// <summary>Returns a string representation showing the repository name and item count.</summary>
    /// <returns>A human-readable summary of the repository state.</returns>
    public override string ToString()
        => $"Repository '{_name}' contains {_items.Count} items.";
}