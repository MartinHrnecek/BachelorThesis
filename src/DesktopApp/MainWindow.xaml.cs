using System.Windows;
using DesktopApp.Services;
using DesktopApp.Models;

namespace DesktopApp;

public partial class MainWindow : Window
{
    private readonly TextAnalyzerService _analyzerService;
    private readonly HistoryService _historyService;

    public MainWindow()
    {
        InitializeComponent();
        _analyzerService = new TextAnalyzerService();
        _historyService = new HistoryService();
        StatusText.Text = "Ready - Enter text to analyze";
    }

    private void OnAnalyzeClick(object sender, RoutedEventArgs e)
    {
        var input = InputTextBox.Text;

        if (string.IsNullOrWhiteSpace(input))
        {
            StatusText.Text = "Error: Input cannot be empty.";
            return;
        }

        var result = _analyzerService.Analyze(input);
        _historyService.Add(result);

        ResultsListBox.Items.Clear();
        ResultsListBox.Items.Add($"Input: {result.OriginalText}");
        ResultsListBox.Items.Add($"Words: {result.WordCount}");
        ResultsListBox.Items.Add($"Characters: {result.CharacterCount}");
        ResultsListBox.Items.Add($"Sentences: {result.SentenceCount}");
        ResultsListBox.Items.Add($"Is Palindrome: {result.IsPalindrome}");
        ResultsListBox.Items.Add($"Uppercase: {result.UppercaseText}");
        ResultsListBox.Items.Add($"Reversed: {result.ReversedText}");
        ResultsListBox.Items.Add("--- History ---");

        foreach (var item in _historyService.GetRecent(5))
            ResultsListBox.Items.Add($"  [{item.AnalyzedAt:HH:mm:ss}] {item.OriginalText.Truncate(30)}");

        StatusText.Text = $"Analyzed successfully at {DateTime.Now:HH:mm:ss}";
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        InputTextBox.Text = string.Empty;
        ResultsListBox.Items.Clear();
        StatusText.Text = "Cleared.";
    }
}