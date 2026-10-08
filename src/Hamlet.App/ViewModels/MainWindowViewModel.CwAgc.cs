using CommunityToolkit.Mvvm.ComponentModel;
using Hamlet.App.Settings;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **HAMLET SETS AGC FOR CW, AS IT SETS THE PREAMP** (work instruction 564, HM-DEC-268): the choice beside the scan's
/// settings, what the CW tune-in changed so leaving CW can put it back, and who the AGC belongs to for the record.
/// </summary>
/// <remarks>
/// The owner, 2026-10-08, asked how to change the radio's AGC: *"I have no idea how to do that."* The CW row of the receiver
/// conditions sets it once on entering CW, his hand wins as it does for every row, and what the radio had before Hamlet
/// changed it is put back when the tab leaves CW. Listen-only: AGC is a receive setting and nothing here keys.
/// </remarks>
public sealed partial class MainWindowViewModel
{
    /// <summary>The choices, in the popover's order: the words shown and the words stored.</summary>
    public static IReadOnlyList<string> AgcInCwChoices { get; } = ["SLOW", "MID", "FAST", "leave the radio alone"];

    /// <summary>The choice shown, one of <see cref="AgcInCwChoices"/>; kept across restarts.</summary>
    [ObservableProperty]
    private string _agcInCw = "SLOW";

    // What the CW tune-in changed the AGC from and to, for leaving CW; null where it changed nothing.
    private CwAgcHold? _cwAgcHold;

    // What the last CW tune-in left the AGC at, set or found, and whether it wrote it; for the record.
    private int? _cwAgcLeft;
    private bool _cwAgcWrote;

    /// <summary>The choice as the engine's.</summary>
    internal CwAgcChoice AgcChoice => AgcInCw switch
    {
        "MID" => CwAgcChoice.Mid,
        "FAST" => CwAgcChoice.Fast,
        "leave the radio alone" => CwAgcChoice.LeaveAlone,
        _ => CwAgcChoice.Slow,
    };

    /// <summary>The operator's receiver choices, as the engine takes them with a tab's conditions.</summary>
    private ReceiverChoices ReceiverChoicesNow => new(AgcChoice);

    // While the setting is being read in at start, a change is not a change to save.
    private bool _loadingAgcInCw;

    private void LoadAgcInCw()
    {
        _loadingAgcInCw = true;

        try
        {
            AgcInCw = _settings.AgcInCw switch
            {
                "mid" => "MID",
                "fast" => "FAST",
                "leave" => "leave the radio alone",
                _ => "SLOW",
            };
        }
        finally
        {
            _loadingAgcInCw = false;
        }
    }

    partial void OnAgcInCwChanged(string value)
    {
        OnPropertyChanged(nameof(CatchScanSettingsLine));

        if (_loadingAgcInCw)
        {
            return;
        }

        _settings.AgcInCw = AgcChoice switch
        {
            CwAgcChoice.Mid => "mid",
            CwAgcChoice.Fast => "fast",
            CwAgcChoice.LeaveAlone => "leave",
            _ => "slow",
        };

        SettingsStore.Save(_settings);
    }

    /// <summary>What the CW tune-in did to the AGC: kept for leaving CW and for the record, and the story line's sentence.</summary>
    /// <param name="results">The tune-in's results.</param>
    /// <returns>`AGC set to SLOW for CW.` where it was changed, or "".</returns>
    private string KeepWhatTheTabChanged(IReadOnlyList<ConditionResult> results)
    {
        _cwAgcHold = CwAgc.HoldFrom(results);

        var agc = results.FirstOrDefault(r => r.Condition.Field == RigField.Agc);

        _cwAgcLeft = agc?.Outcome is ConditionOutcome.Changed or ConditionOutcome.AlreadyRight ? agc.Condition.Wanted : null;
        _cwAgcWrote = agc?.Outcome == ConditionOutcome.Changed;

        return _cwAgcHold is { } hold ? $"AGC set to {CwAgc.Words(hold.Set)} for CW. " : "";
    }

    /// <summary>
    /// On leaving the CW tab, put back the AGC the radio had before Hamlet changed it, unless his hand has moved it since.
    /// </summary>
    /// <remarks>Never-throw discipline (§8): a restore that could not happen is a sentence, not a crash.</remarks>
    private async Task RestoreCwAgcAsync()
    {
        var hold = _cwAgcHold;

        _cwAgcHold = null;
        _cwAgcLeft = null;
        _cwAgcWrote = false;

        if (hold is null || _rig is not { } rig || !IsConnected)
        {
            return;
        }

        try
        {
            var said = await CwAgc.RestoreAsync(rig, hold).ConfigureAwait(true);

            if (said.Length > 0)
            {
                Narrate(said);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Hamlet could not put the AGC back: {ex.Message}";
        }
    }

    /// <summary>The AGC as the radio reports it, and whose it is, for the capture sheet's line and the telemetry row.</summary>
    internal (string? Agc, string SetBy) AgcForTheRecord()
    {
        var reading = RigState[RigField.Agc] is { IsKnown: true, Number: { } n } ? (int?)n : null;
        var words = RigState[RigField.Agc] is { IsKnown: true } agc ? agc.Text : null;

        return (words, CwAgc.SetBy(reading, IsCwMode, AgcChoice, _cwAgcLeft, _cwAgcWrote));
    }

    /// <summary>The capture sheet's AGC line.</summary>
    private string AgcSheetLine()
    {
        var (agc, setBy) = AgcForTheRecord();

        return $"agc        {agc ?? "unknown"}  ({setBy}; Hamlet's choice for CW is {AgcInCw})";
    }

    /// <summary>The capture sheet's AGC line, for a test.</summary>
    internal string AgcSheetLineForTests() => AgcSheetLine();
}
