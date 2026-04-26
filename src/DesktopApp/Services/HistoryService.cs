//===========================================================================
// File:        HistoryService.cs
// Project:     DesktopApp
// Author:      Martin Hrnecek <xhrnecm00>
// Description: In-memory history service maintaining a bounded queue of
//              recent text analysis results for display in the WPF UI.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using DesktopApp.Models;

namespace DesktopApp.Services;

/// <summary>
/// Maintains a bounded in-memory history of recent
/// <see cref="TextAnalysisResult"/> instances, with the most recent
/// entry first.
/// </summary>
public class HistoryService
{
    private const int MaxHistory = 20;
    private const string HistoryPrefix = "history://";

    private readonly List<TextAnalysisResult> _history = new();

    /// <summary>
    /// Adds a new analysis result to the front of the history. If the
    /// history exceeds <see cref="MaxHistory"/> entries, the oldest one
    /// is discarded.
    /// </summary>
    /// <param name="result">The analysis result to record.</param>
    public void Add(TextAnalysisResult result)
    {
        _history.Insert(0, result);
        if (_history.Count > MaxHistory)
            _history.RemoveAt(_history.Count - 1);
    }

    /// <summary>
    /// Returns the most recent <paramref name="count"/> history entries.
    /// </summary>
    /// <param name="count">The maximum number of entries to return.</param>
    /// <returns>An enumeration of the most recent entries, newest first.</returns>
    public IEnumerable<TextAnalysisResult> GetRecent(int count)
        => _history.Take(count);

    /// <summary>Removes all entries from the history.</summary>
    public void Clear() => _history.Clear();

    /// <summary>Gets the current number of entries stored in the history.</summary>
    public int Count => _history.Count;

    /// <summary>
    /// Returns a stable string key identifying the history entry at the
    /// specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the history entry.</param>
    /// <returns>A prefixed key string of the form <c>history://{index}</c>.</returns>
    public string GetHistoryKey(int index)
        => $"{HistoryPrefix}{index}";
}