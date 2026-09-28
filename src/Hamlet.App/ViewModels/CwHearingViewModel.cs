using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.ViewModels;

/// <summary>What the detector already says, read once a second on the decode tick.</summary>
/// <param name="Meter">The keying meter's last reading.</param>
/// <param name="TrackerHz">The pitch the decoder is mixing at.</param>
/// <param name="TrackerHasPitch">Whether that pitch was measured rather than assumed.</param>
/// <param name="TrackerHasKeying">Whether the tracker's survey verdict holds keying.</param>
/// <param name="Survey">Every bin the coarse survey admits as keying.</param>
public sealed record CwHearingState(
    KeyingReading Meter,
    double TrackerHz,
    bool TrackerHasPitch,
    bool TrackerHasKeying,
    IReadOnlyList<KeyingCandidate> Survey)
{
    /// <summary>Nothing is listening.</summary>
    public static CwHearingState None { get; } = new(
        KeyingReading.None, double.NaN, false, false, Array.Empty<KeyingCandidate>());
}

/// <summary>What the rig and the input say at a press, for the owner's verdict row.</summary>
/// <param name="FrequencyHz">The dial, or null where it has not been read.</param>
/// <param name="Mode">The mode with its variant, or null.</param>
/// <param name="Agc">The AGC setting in the radio's words, or null.</param>
/// <param name="Preamp">The preamp setting in the radio's words, or null.</param>
/// <param name="InputPeakDb">The loudest the decoder's input has been in the last moment.</param>
/// <param name="InputFloorDb">The quietest it has been recently.</param>
public sealed record CwHearingRig(
    long? FrequencyHz,
    string? Mode,
    string? Agc,
    string? Preamp,
    double InputPeakDb,
    double InputFloorDb)
{
    /// <summary>Nothing read.</summary>
    public static CwHearingRig Unknown { get; } = new(null, null, null, null, double.NaN, double.NaN);
}

/// <summary>What the pitch strip draws.</summary>
public sealed record CwPitchStrip(
    double LowHz,
    double HighHz,
    double SearchLowHz,
    double SearchHighHz,
    string SearchLabel,
    IReadOnlyList<double> AdmittedHz,
    string AdmittedLabel,
    double TrackerHz,
    string TrackerLabel,
    double MeterHz,
    string MeterLabel)
{
    /// <summary>Nothing.</summary>
    public static CwPitchStrip Empty { get; } = new(
        0, 0, 0, 0, "", Array.Empty<double>(), "", double.NaN, "", double.NaN, "");
}

/// <summary>
/// **THE LIGHT ON THE CW TAB: WHETHER HAMLET THINKS IT HEARS CW** (work instruction 474,
/// step 11, HM-DEC-184).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-27**: *"We're trying to teach you how to find the entry, how to
/// know when to start evaluating. Right now, you have no clue."* Twenty strong stations
/// gave no characters, and nothing on the screen said whether Hamlet had even noticed
/// them.</para>
/// <para>**IT ADDS NO DETECTOR.** It is lit when the keying meter calls it keying or the
/// coarse survey admits a pitch, which are two opinions the code already forms; this only
/// puts them where the owner can judge them. **It says *I think*, never *there is***
/// (§0.0), and the words carry it with the color only agreeing (§0.6).</para>
/// </remarks>
public sealed partial class CwHearingViewModel : ObservableObject
{
    /// <summary>What the light says when it is lit.</summary>
    public const string LitWords = "I think I hear CW";

    /// <summary>What the light says when it is dark.</summary>
    public const string DarkWords = "I don't think I hear CW";

    /// <summary>What the light rests on, on hover.</summary>
    public const string LightTip =
        "lit when the keying meter calls it keying or the survey admits a pitch; "
        + "this is Hamlet's guess, not a fact.";

    /// <summary>What "I agree with you" records, on hover.</summary>
    public const string AgreeTip =
        "Tells Hamlet the light is right about what you hear now. It writes one row to "
        + "Hamlet's telemetry: your verdict, what the light said, the pitches and figures "
        + "the detector is using, and the radio's frequency, mode, AGC and preamp. No audio "
        + "is kept and nothing on the radio changes.";

    /// <summary>What "You're an idiot" records, on hover.</summary>
    public const string IdiotTip =
        "Tells Hamlet the light is wrong about what you hear now. It writes one row to "
        + "Hamlet's telemetry: your verdict, what the light said, the pitches and figures "
        + "the detector is using, and the radio's frequency, mode, AGC and preamp. No audio "
        + "is kept and nothing on the radio changes.";

    private readonly ITelemetry? _telemetry;
    private readonly Func<CwHearingRig> _rig;
    private readonly Func<DateTime> _clock;

    private DateTime _lightChangedUtc;

    /// <summary>Creates the light, dark.</summary>
    /// <param name="telemetry">Where a verdict row goes, or null.</param>
    /// <param name="rig">What the rig and the input say at a press, or null for nothing read.</param>
    /// <param name="clock">The clock, or null for the system's.</param>
    public CwHearingViewModel(
        ITelemetry? telemetry = null,
        Func<CwHearingRig>? rig = null,
        Func<DateTime>? clock = null)
    {
        _telemetry = telemetry;
        _rig = rig ?? (() => CwHearingRig.Unknown);
        _clock = clock ?? (() => DateTime.UtcNow);
        _lightChangedUtc = _clock();

        // The range and the searched band are drawn before anything listens, so the
        // edges are on the screen from the start.
        _strip = StripFor(CwHearingState.None);
        _stripTip = TipFor(_strip, KeyingReading.None);
    }

    /// <summary>What the detector said at the last look.</summary>
    public CwHearingState State { get; private set; } = CwHearingState.None;

    /// <summary>Whether the light is lit.</summary>
    [ObservableProperty]
    private bool _isLit;

    /// <summary>What the light says.</summary>
    [ObservableProperty]
    private string _lightWords = DarkWords;

    /// <summary>What the strip draws.</summary>
    [ObservableProperty]
    private CwPitchStrip _strip = CwPitchStrip.Empty;

    /// <summary>What the strip says on hover.</summary>
    [ObservableProperty]
    private string _stripTip = "";

    /// <summary>When the light last changed, or when it was made where it never has.</summary>
    public DateTime LightChangedUtc => _lightChangedUtc;

    /// <summary>Read what the detector says now.</summary>
    /// <param name="state">The detector's state.</param>
    public void Observe(CwHearingState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        State = state;

        var lit = state.Meter.Verdict == KeyingVerdict.Keying || state.Survey.Count > 0;

        if (lit != IsLit)
        {
            _lightChangedUtc = _clock();
        }

        IsLit = lit;
        LightWords = lit ? LitWords : DarkWords;

        Strip = StripFor(state);
        StripTip = TipFor(Strip, state.Meter);
    }

    /// <summary>
    /// The low end the strip is drawn from, below anything the tracker searches.
    /// </summary>
    /// <remarks>
    /// **DRAWN WIDER THAN THE DETECTOR LOOKS, ON PURPOSE** (work instruction 474). The
    /// tracker searches 300 to 900 Hz, the IC-7300's sidetone setting range from its
    /// manual (page 4-14) and not where a received station lands, so a station beating
    /// at 1000 Hz is invisible by design. The strip shows the edges so the owner can see
    /// a station sitting outside them. Author's, overrulable.
    /// </remarks>
    public const double StripLowHz = 200;

    /// <summary>The high end the strip is drawn to.</summary>
    public const double StripHighHz = 1200;

    private static CwPitchStrip StripFor(CwHearingState state)
    {
        var admitted = state.Survey.Select(c => c.ToneHz).ToList();
        var meter = state.Meter;

        return new CwPitchStrip(
            StripLowHz,
            StripHighHz,
            CwToneTracker.MinimumToneHz,
            CwToneTracker.MaximumToneHz,
            string.Create(
                CultureInfo.InvariantCulture,
                $"searched {CwToneTracker.MinimumToneHz:0} to {CwToneTracker.MaximumToneHz:0} Hz"),
            admitted,
            admitted.Count == 0
                ? "survey admitted nothing"
                : "survey admitted "
                  + string.Join(", ", admitted.Select(hz => hz.ToString("0", CultureInfo.InvariantCulture)))
                  + " Hz",
            state.TrackerHz,
            double.IsNaN(state.TrackerHz)
                ? "not listening"
                : string.Create(CultureInfo.InvariantCulture, $"mixing {state.TrackerHz:0} Hz")
                  + (state.TrackerHasPitch ? "" : ", not measured"),
            meter.ToneHz > 0 ? meter.ToneHz : double.NaN,
            meter.ToneHz > 0
                ? string.Create(CultureInfo.InvariantCulture, $"meter {meter.ToneHz:0} Hz")
                : "meter has no pitch");
    }

    private static string TipFor(CwPitchStrip strip, KeyingReading meter)
    {
        var tracker = double.IsNaN(strip.TrackerHz)
            ? "Solid line: the pitch the decoder is mixing at. Nothing is listening, so there is none."
            : "Solid line: the pitch the decoder is mixing at now, "
              + strip.TrackerLabel.Replace("mixing ", "", StringComparison.Ordinal)
              + ". Where nothing is measured it falls back to the last pitch, the bank's centre or your CW pitch.";

        var meterLine = double.IsNaN(strip.MeterHz)
            ? "Dashed line: the keying meter's best pitch. It has not measured one yet."
            : string.Create(
                CultureInfo.InvariantCulture,
                $"Dashed line: the keying meter's best pitch, {strip.MeterHz:0} Hz - score {meter.Score:0.00}, "
                + $"median {meter.MedianMs:0} ms, swing {meter.SwingDb:0} dB, verdict {VerdictWord(meter.Verdict)}.");

        return string.Join(
            Environment.NewLine,
            string.Create(
                CultureInfo.InvariantCulture,
                $"The whole range the detector could sweep, {strip.LowHz:0} to {strip.HighHz:0} Hz."),
            "Shaded: the band the tracker searches today, "
                + strip.SearchLabel.Replace("searched ", "", StringComparison.Ordinal) + ".",
            "Short ticks: pitches the survey has admitted as keying - "
                + strip.AdmittedLabel.Replace("survey admitted ", "", StringComparison.Ordinal) + ".",
            tracker,
            meterLine,
            "This shows where Hamlet looks and changes nothing about it.");
    }

    /// <summary>The owner says Hamlet was right.</summary>
    [RelayCommand]
    private void Agree() => _telemetry?.Write(TelemetryCategory.Cw, VerdictEvent, VerdictRow("agree"));

    /// <summary>The owner says Hamlet was wrong.</summary>
    [RelayCommand]
    private void Idiot() => _telemetry?.Write(TelemetryCategory.Cw, VerdictEvent, VerdictRow("idiot"));

    /// <summary>The event a press writes.</summary>
    public const string VerdictEvent = "owner_verdict";

    /// <summary>
    /// **THE ROW: THE OWNER'S EAR BESIDE THE DETECTOR'S STATE, AND NOTHING ELSE** (work
    /// instruction 474 task 3).
    /// </summary>
    /// <param name="verdict">`agree` or `idiot`.</param>
    /// <returns>The row's fields, by the names the instruction gives them.</returns>
    /// <remarks>
    /// <para>**NO AUDIO IS CAPTURED AND NO SIDECAR IS WRITTEN.** The corpus is banned
    /// (R88), and the next unit needs the owner's verdict and the state it was about,
    /// which is all this carries.</para>
    /// <para>**A FIGURE NOT MEASURED IS NULL, NEVER NaN**: the writer's serializer
    /// refuses NaN, and a row that fails to serialize is a press that left nothing.</para>
    /// </remarks>
    public IReadOnlyDictionary<string, object?> VerdictRow(string verdict)
    {
        var state = State;
        var meter = state.Meter;
        var rig = _rig();

        return new Dictionary<string, object?>
        {
            ["verdict"] = verdict,
            ["light"] = LightWords,
            ["trackerHz"] = Measured(state.TrackerHz),
            ["trackerHasPitch"] = state.TrackerHasPitch,
            ["trackerHasKeying"] = state.TrackerHasKeying,
            ["meterVerdict"] = VerdictWord(meter.Verdict),
            ["meterHz"] = meter.ToneHz > 0 ? meter.ToneHz : null,
            ["meterScore"] = Measured(meter.Score),
            ["meterMedianMs"] = Measured(meter.MedianMs),
            ["meterSwingDb"] = Measured(meter.SwingDb),
            ["survey"] = state.Survey
                .Select(c => (IReadOnlyDictionary<string, object?>)new Dictionary<string, object?>
                {
                    ["hz"] = Measured(c.ToneHz),
                    ["levelDb"] = Measured(c.KeyedDb),
                })
                .ToList(),
            ["frequency"] = rig.FrequencyHz,
            ["mode"] = rig.Mode,
            ["agc"] = rig.Agc,
            ["preamp"] = rig.Preamp,
            ["inputPeakDb"] = Measured(rig.InputPeakDb),
            ["inputFloorDb"] = Measured(rig.InputFloorDb),
            ["sinceVerdictMs"] = (long)Math.Round((_clock() - _lightChangedUtc).TotalMilliseconds),
        };
    }

    private static double? Measured(double value)
        => double.IsNaN(value) || double.IsInfinity(value) ? null : value;

    private static string VerdictWord(KeyingVerdict verdict) => verdict switch
    {
        KeyingVerdict.Keying => "keying",
        KeyingVerdict.NoKeying => "no keying",
        _ => "listening",
    };
}
