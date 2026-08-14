using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.App.ViewModels;

/// <summary>One row of the diagnostics screen.</summary>
/// <param name="Label">What the thing is called.</param>
/// <param name="Value">What it reads, or what is known about why it does not.</param>
/// <param name="Age">How long ago, in words, or empty.</param>
/// <param name="Source">Which command produced it.</param>
/// <param name="State">How much is known.</param>
/// <param name="IsStale">Whether the reading is older than its kind should be.</param>
public sealed record RigDiagnosticRow(
    string Label,
    string Value,
    string Age,
    string Source,
    RigValueState State,
    bool IsStale)
{
    /// <summary>True when this is a real, current reading.</summary>
    public bool IsCurrent => State == RigValueState.Known && !IsStale;
}

/// <summary>
/// Everything Hamlet knows about the radio, on one screen, with each value's
/// age and where it came from.
/// </summary>
/// <remarks>
/// <para>THE SCREEN THAT WOULD HAVE ANSWERED THE QUESTIONS. The first live
/// connection produced garbage from the CW decoder and took half an hour to
/// diagnose, by asking the operator to walk to the radio and read menu settings
/// out loud: what is the filter set to, what is the ACC output level, is the
/// squelch open. Every one of those is a row here (HM-DEC-050).</para>
/// <para>It is also what a bug report should be able to attach, which is why
/// every row carries its provenance and its age rather than just a number. A
/// wrong value that arrives with the command that produced it and the moment it
/// was read is something somebody can fix; a wrong value on its own is an
/// argument (§0.0.1).</para>
/// <para>Rows for things Hamlet does not know are shown rather than hidden. A
/// screen that listed only what it had would leave somebody wondering whether a
/// missing row meant "not read" or "not a thing", which is the question the
/// screen exists to answer.</para>
/// </remarks>
public partial class RigDiagnosticsViewModel : ObservableObject
{
    private readonly RigStateMonitor? _monitor;

    /// <summary>What the copy button last put on the clipboard, for the test.</summary>
    [ObservableProperty]
    private string _copyText = "";

    /// <summary>Designer constructor.</summary>
    public RigDiagnosticsViewModel() : this(null, RigState.Empty)
    {
    }

    /// <summary>Runtime constructor.</summary>
    /// <param name="monitor">The monitor to refresh from, or null.</param>
    /// <param name="state">The state to show.</param>
    public RigDiagnosticsViewModel(RigStateMonitor? monitor, RigState state)
    {
        _monitor = monitor;
        Rows = new ObservableCollection<RigDiagnosticRow>();
        Show(state);
    }

    /// <summary>Every field, known or not.</summary>
    public ObservableCollection<RigDiagnosticRow> Rows { get; }

    /// <summary>
    /// What Hamlet can say about these settings from the values it read.
    /// </summary>
    /// <remarks>
    /// Consequences rather than instructions (HM-DEC-050). "The filter is open
    /// to 3 kHz and the radio is in Morse" is a statement about two numbers
    /// Hamlet read; telling somebody to narrow it would be operating their radio
    /// for them, which this does not do.
    /// </remarks>
    public ObservableCollection<string> Observations { get; } = new();

    /// <summary>True when there is anything worth saying.</summary>
    [ObservableProperty]
    private bool _hasObservations;

    /// <summary>Which radio this describes.</summary>
    [ObservableProperty]
    private string _rigName = "no radio connected";

    /// <summary>A one-line summary of how much is actually known.</summary>
    [ObservableProperty]
    private string _summary = "";

    /// <summary>Replace the rows from a state snapshot.</summary>
    /// <param name="state">The snapshot.</param>
    public void Show(RigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var now = DateTime.UtcNow;
        Rows.Clear();

        foreach (var value in state.All())
        {
            var readout = RigDisplay.Describe(value, now);

            Rows.Add(new RigDiagnosticRow(
                RigDisplay.Label(value.Field),
                readout.Text,
                readout.AgeText,
                value.Source,
                value.State,
                readout.Freshness == RigFreshness.Stale));
        }

        Observations.Clear();

        foreach (var line in RigObservations.For(state))
        {
            Observations.Add(line);
        }

        HasObservations = Observations.Count > 0;
        RigName = state.RigName ?? "no radio connected";

        var known = Rows.Count(r => r.State == RigValueState.Known);
        Summary = $"{known} of {Rows.Count} read from the radio";
        CopyText = BuildCopyText();
    }

    /// <summary>Ask the radio again for everything it will answer.</summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (_monitor is null)
        {
            return;
        }

        await _monitor.RefreshAllAsync().ConfigureAwait(true);
        Show(_monitor.State);
    }

    /// <summary>
    /// The whole screen as text, for pasting into a bug report.
    /// </summary>
    /// <remarks>
    /// NO CALLSIGN, NO LOCATION, NOTHING ABOUT THE OPERATOR. This is radio
    /// state and the frequency the radio is on, which is not identifying in the
    /// way a callsign is, and it is the same bargain the About box's diagnostics
    /// already make (HM-DEC-019).
    /// </remarks>
    private string BuildCopyText()
    {
        var lines = new List<string> { $"Rig: {RigName}", Summary, "" };

        foreach (var line in Observations)
        {
            lines.Add("* " + line);
        }

        if (Observations.Count > 0)
        {
            lines.Add("");
        }

        foreach (var row in Rows)
        {
            var age = row.Age.Length > 0 ? $"  ({row.Age})" : "";
            var stale = row.IsStale ? "  [stale]" : "";
            lines.Add($"{row.Label,-24} {row.Value,-28} {row.Source}{age}{stale}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
