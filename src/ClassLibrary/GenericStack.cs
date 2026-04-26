//===========================================================================
// File:        GenericStack.cs
// Project:     ClassLibrary
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Generic LIFO stack implementation backed by a doubly
//              linked list. Provides standard Push/Pop/Peek operations
//              with O(1) complexity.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace ClassLibrary;

/// <summary>
/// Represents a generic last-in, first-out (LIFO) stack of elements
/// backed by a <see cref="LinkedList{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements stored in the stack.</typeparam>
public class GenericStack<T>
{
    private readonly LinkedList<T> _list = new();
    private const string EmptyMessage = "Stack is empty";

    /// <summary>
    /// Pushes an item onto the top of the stack.
    /// </summary>
    /// <param name="item">The item to push.</param>
    public void Push(T item) => _list.AddFirst(item);

    /// <summary>
    /// Removes and returns the item at the top of the stack.
    /// </summary>
    /// <returns>The item removed from the top of the stack.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the stack is empty.</exception>
    public T Pop()
    {
        if (_list.Count == 0)
            throw new InvalidOperationException(EmptyMessage);
        var value = _list.First!.Value;
        _list.RemoveFirst();
        return value;
    }

    /// <summary>
    /// Returns the item at the top of the stack without removing it.
    /// </summary>
    /// <returns>The item at the top of the stack.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the stack is empty.</exception>
    public T Peek()
    {
        if (_list.Count == 0)
            throw new InvalidOperationException(EmptyMessage);
        return _list.First!.Value;
    }

    /// <summary>Gets a value indicating whether the stack contains no elements.</summary>
    public bool IsEmpty => _list.Count == 0;

    /// <summary>Gets the number of elements currently in the stack.</summary>
    public int Count => _list.Count;

    /// <summary>
    /// Returns an enumerable view of the stack contents from top to bottom.
    /// </summary>
    /// <returns>An enumerable iterating from the top of the stack to the bottom.</returns>
    public IEnumerable<T> ToEnumerable() => _list;
}