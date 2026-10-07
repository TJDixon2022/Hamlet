using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Capture;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Hamlet.App.Telemetry;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **HAMLET CAPTURES FOR ITSELF** (work instruction 549, HM-DEC-253): the automatic capture, started and stopped with
/// listening, ticked once a second, and the line under the terminal header that says what it is doing.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-07:** *"I think you can figure something out to allow this unattended. Perhaps auto capture
/// on your side."* The engine decides (<see cref="CwAutoCapture"/>); this hands it the audio and what the app knows, and
/// shows what it says.</para>
/// <para>**IT ONLY READS.** Nothing here tunes, changes a mode or a setting, keys or transmits.</para>
/// </remarks>
public sealed partial class MainWindowViewModel
{
    private CwAutoCapture? _autoCapture;
    private DateTime _lastAutoTickUtc = DateTime.MinValue;
    private string _autoSheetExtra = string.Empty;
    private CwListenSampler _listenSampler = new();
    private long _lastQualityLetters = -1;

    /// <summary>Where automatic captures go: <c>captures\auto</c> under Hamlet's data folder, and nothing else is ever deleted.</summary>
    internal static string AutoCaptureFolder => Path.Combine(CaptureFolder, "auto");

    /// <summary>The line under the terminal header while an automatic capture runs or is paused; empty otherwise.</summary>
    [ObservableProperty]
    private string _autoCaptureLine = string.Empty;

    /// <summary>The line's hover: where the files go.</summary>
    [ObservableProperty]
    private string _autoCaptureTip = string.Empty;

    /// <summary>
    /// Whether the line shows: it shares the hint's cell under the terminal header with the scan's line, so the one layout
    /// holds and nothing moves when it appears; a scan's own line wins, since a scan moving the dial ends a W1AW capture.
    /// </summary>
    public bool ShowsAutoCaptureLine => AutoCaptureLine.Length > 0 && CatchScanLine.Length == 0;

    /// <summary>Whether the hint shows: neither the scan nor the capture has anything to say.</summary>
    public bool ShowsTerminalHint => AutoCaptureLine.Length == 0 && CatchScanLine.Length == 0;

    partial void OnAutoCaptureLineChanged(string value) => HeaderLinesChanged();

    partial void OnCatchScanLineChanged(string value) => HeaderLinesChanged();

    private void HeaderLinesChanged()
    {
        OnPropertyChanged(nameof(ShowsAutoCaptureLine));
        OnPropertyChanged(nameof(ShowsTerminalHint));
    }

    /// <summary>The automatic capture, for the tests.</summary>
    internal CwAutoCapture? AutoCapture => _autoCapture;

    /// <summary>Start the automatic capture beside the decoder, on the same audio.</summary>
    /// <param name="source">The audio.</param>
    private void StartAutoCapture(IAudioSource source)
    {
        _autoCapture?.Dispose();
        _autoCapture = new CwAutoCapture(AutoCaptureFolder, W1awMorseFrequencies.Default, () => DateTime.UtcNow)
        {
            SheetExtra = () => Volatile.Read(ref _autoSheetExtra),
        };
        _autoCapture.Listen(source);
        _listenSampler = new CwListenSampler();
        _lastAutoTickUtc = DateTime.MinValue;

        // **WHAT THE TERMINAL PRINTS**, for the stray-letter trigger (task 2), on the chain's thread.
        if (_decoder is not null)
        {
            _decoder.CharacterSettled += OnSettledForAutoCapture;
        }
    }

    private void OnSettledForAutoCapture(CwCharacter character) => _autoCapture?.Character(character);

    /// <summary>Stop it: a capture under way ends, saying listening stopped.</summary>
    private void StopAutoCapture()
    {
        if (_decoder is not null)
        {
            _decoder.CharacterSettled -= OnSettledForAutoCapture;
        }

        _autoCapture?.Dispose();
        _autoCapture = null;
        AutoCaptureLine = string.Empty;
        AutoCaptureTip = string.Empty;
    }

    /// <summary>Once a second, from the decode tick: what the app knows, handed to the automatic capture.</summary>
    private void TickAutoCapture()
    {
        if (_autoCapture is not { } auto)
        {
            return;
        }

        var now = DateTime.UtcNow;

        if (now - _lastAutoTickUtc < TimeSpan.FromSeconds(1))
        {
            return;
        }

        _lastAutoTickUtc = now;

        var state = RigState;
        var inCw = state[RigField.Mode] is { IsKnown: true, Number: { } mode } && CivValues.IsCw((CivMode)(int)mode);
        long? hz = state[RigField.Frequency] is { IsKnown: true, Number: { } read } ? (long)read : null;

        // **THE SHEET'S OWN LINES ARE COMPOSED HERE, ON THE APP'S THREAD**, and handed to the capture's as one string.
        Volatile.Write(ref _autoSheetExtra, AutoSheetLines());

        auto.Tick(new AutoCaptureConditions(
            IsDecoding,
            inCw,
            IsCatchScanning,
            hz,
            state.FilterBandwidthHz,
            _decoder?.ShapeSide.Senders.Count ?? 0,
            _liveFeed?.Continuity.LostMilliseconds ?? 0));

        AutoCaptureLine = auto.Line;
        AutoCaptureTip = auto.Tip;

        // **AND THE TELEMETRY SAYS WHAT IS HAPPENING, EVERY TEN SECONDS** (task 3).
        if (_liveFeed?.Continuity is { } audio
            && _listenSampler.Sample(now, audio, _decoder?.ShapeSide ?? CwShapeSideReading.Nothing, CwHearing.ShapeLightWords, auto.CaptureUnderWay) is { } sample)
        {
            AppEvents.CwListen(_telemetry, sample);
        }
    }

    /// <summary>
    /// What a Record sheet says about the radio and the shape side, for an automatic capture's sheet: the frequency and band
    /// from the radio, the printed sender's pitch and speed, every sender held, the audio line and every rig field with its
    /// provenance. The figures that need the recording in hand - its peak, the tone over it, the duty - are a Record press's.
    /// </summary>
    private string AutoSheetLines()
    {
        var shape = _decoder?.ShapeSide ?? CwShapeSideReading.Nothing;
        var lines = new List<string>
        {
            $"frequency  {CapturedFrequency()}",
            $"band       {CapturedBand()}",
            $"pitch      {PitchForTheRecord(shape)}",
            $"speed      {SpeedForTheRecord(shape)}",
            $"senders    {SendersForTheRecord(shape)}",
            $"audio      {(_liveFeed?.Continuity is { } audio ? audio.SheetLine + "  (since the decoder started listening)" : "not counted  (nothing is listening)")}",
            string.Format(CultureInfo.InvariantCulture, "composed   {0:yyyy-MM-dd HH:mm:ss} UTC  (these lines, as the app last had them)", DateTime.UtcNow),
            string.Empty,
        };

        foreach (var value in RigState.All())
        {
            lines.Add($"{value.Field,-20} " + (value.IsKnown ? value.Text : value.State.ToString().ToLowerInvariant()));
        }

        return string.Join(Environment.NewLine, lines);
    }
}
