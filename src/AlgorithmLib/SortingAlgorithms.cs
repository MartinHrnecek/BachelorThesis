//===========================================================================
// File:        SortingAlgorithms.cs
// Project:     AlgorithmLib
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Implementations of sorting algorithms used by the test
//              application: bubble sort, quicksort and insertion sort.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using AlgorithmLib.Interfaces;

namespace AlgorithmLib;

/// <summary>
/// Provides implementations of in-place array sorting algorithms.
/// </summary>
public class SortingAlgorithms : ISortingAlgorithms
{
    /// <summary>
    /// Sorts the array in ascending order using the bubble sort algorithm.
    /// </summary>
    /// <param name="arr">The array to be sorted in place.</param>
    public void BubbleSort(int[] arr)
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
            for (int j = 0; j < n - i - 1; j++)
                if (arr[j] > arr[j + 1])
                    (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
    }

    /// <summary>
    /// Sorts the specified subarray in ascending order using the recursive
    /// quicksort algorithm with last-element pivot selection.
    /// </summary>
    /// <param name="arr">The array to be sorted in place.</param>
    /// <param name="low">The inclusive lower bound of the subarray.</param>
    /// <param name="high">The inclusive upper bound of the subarray.</param>
    public void QuickSort(int[] arr, int low, int high)
    {
        if (low < high)
        {
            int pivot = Partition(arr, low, high);
            QuickSort(arr, low, pivot - 1);
            QuickSort(arr, pivot + 1, high);
        }
    }

    /// <summary>
    /// Performs the Lomuto partition step used by <see cref="QuickSort"/>.
    /// </summary>
    /// <param name="arr">The array being partitioned.</param>
    /// <param name="low">The inclusive lower bound of the partition range.</param>
    /// <param name="high">The inclusive upper bound; its element is used as the pivot.</param>
    /// <returns>The final index of the pivot element after partitioning.</returns>
    private int Partition(int[] arr, int low, int high)
    {
        int pivot = arr[high];
        int i = low - 1;
        for (int j = low; j < high; j++)
        {
            if (arr[j] <= pivot)
            {
                i++;
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }
        (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
        return i + 1;
    }

    /// <summary>
    /// Sorts the array in ascending order using the insertion sort algorithm.
    /// </summary>
    /// <param name="arr">The array to be sorted in place.</param>
    public void InsertionSort(int[] arr)
    {
        for (int i = 1; i < arr.Length; i++)
        {
            int key = arr[i];
            int j = i - 1;
            while (j >= 0 && arr[j] > key)
            {
                arr[j + 1] = arr[j];
                j--;
            }
            arr[j + 1] = key;
        }
    }
}