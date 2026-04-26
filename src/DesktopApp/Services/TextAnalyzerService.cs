//===========================================================================
// File:        TextAnalyzerService.cs
// Project:     DesktopApp
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Text analysis service producing word, character and
//              sentence counts, a palindrome check, and uppercase and
//              reversed variants of the input. Consumed by the WPF UI
//              and persisted via HistoryService.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using DesktopApp.Models;

namespace DesktopApp.Services;

/// <summary>
/// Performs simple text analysis (counts, palindrome check, case and
/// reversal transforms) and packages the result as a
/// <see cref="TextAnalysisResult"/>.
/// </summary>
public class TextAnalyzerService
{
    private const string SentenceDelimiters = ".!?";

    /// <summary>
    /// Runs the full analysis pipeline on the given text and returns
    /// the populated result object.
    /// </summary>
    /// <param name="text">The input text to analyze.</param>
    /// <returns>A <see cref="TextAnalysisResult"/> describing the analyzed text.</returns>
    public TextAnalysisResult Analyze(string text)
    {
        return new TextAnalysisResult
        {
            OriginalText = text,
            WordCount = CountWords(text),
            CharacterCount = text.Length,
            SentenceCount = CountSentences(text),
            IsPalindrome = CheckPalindrome(text),
            UppercaseText = text.ToUpper(),
            ReversedText = new string(text.Reverse().ToArray())
        };
    }

    /// <summary>Counts whitespace-separated words in the input.</summary>
    /// <param name="text">The input text.</param>
    /// <returns>The number of non-empty space-separated tokens.</returns>
    private int CountWords(string text)
        => text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

    /// <summary>
    /// Counts sentences by tallying the number of sentence-terminating
    /// punctuation marks.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The detected sentence count, or <c>1</c> if no terminator is found.</returns>
    private int CountSentences(string text)
    {
        int count = 0;
        foreach (char c in text)
            if (SentenceDelimiters.Contains(c)) count++;
        return Math.Max(count, 1);
    }

    /// <summary>
    /// Determines whether the input is a palindrome, ignoring case and
    /// non-alphanumeric characters.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns><c>true</c> if the cleaned input reads the same forwards and backwards; otherwise <c>false</c>.</returns>
    private bool CheckPalindrome(string text)
    {
        var cleaned = new string(text.Where(char.IsLetterOrDigit).ToArray()).ToLower();
        int left = 0, right = cleaned.Length - 1;
        while (left < right)
        {
            if (cleaned[left] != cleaned[right]) return false;
            left++;
            right--;
        }
        return true;
    }
}