using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.ViewModels;

/// <summary>One thing Hamlet decided, for the operator to read (HM-DEC-077).</summary>
/// <param name="Question">What was being decided, e.g. "Can I send".</param>
/// <param name="Reason">The stable token behind it.</param>
/// <param name="Outcome">Which way it went.</param>
/// <param name="Says">The sentence the operator would have seen.</param>
/// <param name="AtUtc">When.</param>
public sealed record DecisionRow(
    string Question, string Reason, Outcome Outcome, string Says, DateTime AtUtc)
{
    /// <summary>The local time, as the window shows it.</summary>
    public string At => AtUtc.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);

    /// <summary>What happened, in one word.</summary>
    public string Verdict => Outcome.ToString().ToLowerInvariant();

    /// <summary>True when Hamlet declined or failed, which is what to look for.</summary>
    public bool IsNotable => Outcome != Outcome.Proceeded;
}

/// <summary>
/// What Hamlet has recently decided (HM-DEC-077).
/// </summary>
/// <remarks>
/// <para>THE COMPANION TO "WHAT THE RADIO IS DOING", AND IT ANSWERS THE OTHER
/// HALF. That window says what the radio is doing; this one says what Hamlet did
/// about it. The evening this was written, both questions had to be answered
/// from a photograph of a window, because a disabled button fires no handler and
/// the file recorded nothing.</para>
/// <para>NOTHING IDENTIFYING (HM-DEC-018). A row carries a question, a token, a
/// verdict and the sentence the operator would have read. There is no member
/// that can hold a callsign or a message, and the copy button writes exactly
/// what is on screen.</para>
/// </remarks>
public sealed partial class DecisionLogViewModel : ObservableObject
{
    /// <summary>How many decisions are kept.</summary>
    /// <remarks>
    /// Two hundred is a long evening of transitions and small enough to read.
    /// Only changes are noted, so an hour of a steady radio adds nothing.
    /// </remarks>
    public const int Maximum = 200;

    /// <summary>The decisions, newest first.</summary>
    public ObservableCollection<DecisionRow> Rows { get; } = new();

    /// <summary>True when nothing has been decided yet.</summary>
    public bool IsEmpty => Rows.Count == 0;

    /// <summary>What an empty window says.</summary>
    public const string EmptyNote =
        "Nothing decided yet. When Hamlet works out whether it can send, or "
        + "whether the scope is running, or why a decode was thrown away, it "
        + "lands here with the reason beside it.";

    /// <summary>The collapsed summary of the last thing decided.</summary>
    public string Summary => Rows.Count == 0
        ? "nothing decided yet"
        : $"{Rows[0].Question}: {Rows[0].Verdict}";

    /// <summary>
    /// Note a decision.
    /// </summary>
    /// <param name="question">What was being decided.</param>
    /// <param name="reason">The stable token.</param>
    /// <param name="outcome">Which way it went.</param>
    /// <param name="says">The sentence the operator would have seen.</param>
    /// <param name="atUtc">When.</param>
    /// <remarks>
    /// Changes only. Noting an unchanged verdict every second would fill this
    /// with two thousand identical rows and bury the transitions, which are the
    /// entire diagnosis.
    /// </remarks>
    public void Note(
        string question, string reason, Outcome outcome, string says, DateTime atUtc)
    {
        if (Rows.Count > 0
            && string.Equals(Rows[0].Question, question, StringComparison.Ordinal)
            && string.Equals(Rows[0].Reason, reason, StringComparison.Ordinal))
        {
            return;
        }

        Rows.Insert(0, new DecisionRow(question, reason, outcome, says, atUtc));

        while (Rows.Count > Maximum)
        {
            Rows.RemoveAt(Rows.Count - 1);
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(Summary));
    }

    /// <summary>
    /// The whole log as text, for a bug report.
    /// </summary>
    /// <returns>What is on screen, and nothing else.</returns>
    public string ForBugReport()
    {
        var text = new StringBuilder();
        text.AppendLine("What Hamlet decided");
        text.AppendLine();

        if (Rows.Count == 0)
        {
            text.AppendLine("(nothing yet)");
            return text.ToString();
        }

        foreach (var row in Rows)
        {
            text.AppendLine(
                CultureInfo.InvariantCulture,
                $"{row.At}  {row.Question}: {row.Verdict} ({row.Reason})");

            if (row.Says.Length > 0)
            {
                text.AppendLine(CultureInfo.InvariantCulture, $"          {row.Says}");
            }
        }

        return text.ToString();
    }

    /// <summary>Copy the log for a bug report.</summary>
    [RelayCommand]
    private async Task CopyAsync()
    {
        var top = Avalonia.Application.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime
            { MainWindow: { } window }
            ? window
            : null;

        if (top?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(ForBugReport());
        }
    }
}
