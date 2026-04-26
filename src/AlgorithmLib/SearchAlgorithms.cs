//===========================================================================
// File:        SearchAlgorithms.cs
// Project:     AlgorithmLib
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Implementations of search algorithms used by the test
//              application: iterative binary search and linear search.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using AlgorithmLib.Interfaces;

namespace AlgorithmLib;

/// <summary>
/// Provides implementations of array search algorithms.
/// </summary>
public class SearchAlgorithms : ISearchAlgorithms
{
    /// <summary>
    /// Searches a sorted array for the specified value using iterative
    /// binary search.
    /// </summary>
    /// <param name="arr">A pre-sorted array of integers.</param>
    /// <param name="target">The value to locate.</param>
    /// <returns>
    /// The zero-based index of <paramref name="target"/> in
    /// <paramref name="arr"/>, or <c>-1</c> if the value is not present.
    /// </returns>
    public int BinarySearch(int[] arr, int target)
    {
        int left = 0, right = arr.Length - 1;
        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            if (arr[mid] == target) return mid;
            if (arr[mid] < target) left = mid + 1;
            else right = mid - 1;
        }
        return -1;
    }

    /// <summary>
    /// Searches an array for the specified value using sequential scan.
    /// </summary>
    /// <param name="arr">An array of integers (need not be sorted).</param>
    /// <param name="target">The value to locate.</param>
    /// <returns>
    /// The zero-based index of the first occurrence of
    /// <paramref name="target"/> in <paramref name="arr"/>, or <c>-1</c>
    /// if the value is not present.
    /// </returns>
    public int LinearSearch(int[] arr, int target)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == target) return i;
        return -1;
    }
}