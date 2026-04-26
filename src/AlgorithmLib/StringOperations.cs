//===========================================================================
// File:        StringOperations.cs
// Project:     AlgorithmLib
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Implementations of common string manipulation routines used
//              by the test application: palindrome check, word reversal,
//              character counting and camelCase conversion.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using AlgorithmLib.Interfaces;

namespace AlgorithmLib;

/// <summary>
/// Provides implementations of common string manipulation algorithms.
/// </summary>
public class StringOperations : IStringOperations
{
    /// <summary>
    /// Determines whether the specified string reads the same forwards
    /// and backwards (case- and whitespace-sensitive).
    /// </summary>
    /// <param name="s">The string to test.</param>
    /// <returns><c>true</c> if <paramref name="s"/> is a palindrome; otherwise <c>false</c>.</returns>
    public bool IsPalindrome(string s)
    {
        int left = 0, right = s.Length - 1;
        while (left < right)
        {
            if (s[left] != s[right]) return false;
            left++;
            right--;
        }
        return true;
    }

    /// <summary>
    /// Reverses the order of space-separated words in the given sentence.
    /// </summary>
    /// <param name="sentence">The sentence whose words are to be reversed.</param>
    /// <returns>A new string with the order of words reversed.</returns>
    public string ReverseWords(string sentence)
    {
        var words = sentence.Split(' ');
        Array.Reverse(words);
        return string.Join(' ', words);
    }

    /// <summary>
    /// Counts the number of occurrences of <paramref name="target"/>
    /// in <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The string to scan.</param>
    /// <param name="target">The character to count.</param>
    /// <returns>The number of occurrences of <paramref name="target"/> in <paramref name="text"/>.</returns>
    public int CountOccurrences(string text, char target)
    {
        int count = 0;
        foreach (char c in text)
            if (c == target) count++;
        return count;
    }

    /// <summary>
    /// Converts a space-separated string to camelCase.
    /// </summary>
    /// <param name="input">The space-separated input string.</param>
    /// <returns>
    /// The input converted to camelCase: the first word is lowercased and
    /// each subsequent word has its initial letter uppercased.
    /// </returns>
    public string ToCamelCase(string input)
    {
        var words = input.Split(' ');
        var result = words[0].ToLower();
        for (int i = 1; i < words.Length; i++)
            result += char.ToUpper(words[i][0]) + words[i][1..].ToLower();
        return result;
    }
}