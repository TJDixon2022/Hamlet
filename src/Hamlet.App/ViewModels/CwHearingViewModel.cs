using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.ViewModels;

/// <summary>What the detector already says, read once a second on the decode tick.</summary>
/// <param name="Meter">The keying meter's last reading.</param>
/// <param name="TrackerHz">The tracker's own pitch - not where the decoder mixes while the detector says keying (work instruction 488).</param>
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
/// <param name="ScopePeakHz">
/// The pitch the radio's scope points at, or NaN where it is quiet (work instruction 480).
/// </param>
/// <param name="ScopePeakLevel">That peak's height on the radio's own 0 to 160 scale, or null.</param>
/// <param name="ScopeFramesLast4s">Frames the radio's scope sent in the last four seconds, or null with no radio.</param>
public sealed record CwHearingRig(
    long? FrequencyHz,
    string? Mode,
    string? Agc,
    string? Preamp,
    double InputPeakDb,
    double InputFloorDb,
    double ScopePeakHz = double.NaN,
    int? ScopePeakLevel = null,
    int? ScopeFramesLast4s = null)
{
    /// <summary>Nothing read.</summary>
    public static CwHearingRig Unknown { get; } = new(null, null, null, null, double.NaN, double.NaN);
}

/// <summary>
/// What the oscilloscope draws: the level trace, the marks, the detector's pitch and the
/// tracker's (work instruction 478 task 1, R92).
/// </summary>
/// <param name="Hops">Every hop of the last four seconds, oldest first.</param>
/// <param name="HopMs">One hop, in milliseconds.</param>
/// <param name="Reading">The detector at its last hop.</param>
/// <param name="ToneHz">The detector's pitch while it says keying; NaN when it does not.</param>
/// <param name="MixingHz">The pitch the decoder is mixing at; NaN when nothing is decoding.</param>
/// <param name="ScopeQuiet">
/// In CW, the radio's scope has sent nothing for three seconds and the detector sweeps on its own
/// (work instruction 480 task 2).
/// </param>
public sealed record CwScopeFrame(
    IReadOnlyList<CwScopeHop> Hops,
    double HopMs,
    CwEnvelopeReading Reading,
    double ToneHz,
    double MixingHz,
    bool ScopeQuiet = false)
{
    /// <summary>The pitch line when the detector says nobody is keying.</summary>
    public const string NoKeyingWords = "no keying";

    /// <summary>What leads the tone line while the radio's scope is quiet in CW.</summary>
    public const string ScopeQuietWords = "scope quiet, sweeping";

    /// <summary>
    /// The last eight seconds of bars and the letters settled over them: what the graph draws
    /// (work instruction 480 task 3, R95).
    /// </summary>
    public CwTrainingFrame Training { get; init; } = CwTrainingFrame.Empty;

    /// <summary>The tracker's line when nothing is decoding.</summary>
    public const string NotMixingWords = "not mixing";

    /// <summary>Nothing is listening.</summary>
    public static CwScopeFrame Empty { get; } = new(
        Array.Empty<CwScopeHop>(), 5, CwEnvelopeReading.None, double.NaN, double.NaN);

    /// <summary>
    /// "tone 742 Hz heard" while the detector says keying, "no keying" otherwise (work instruction 489); led by
    /// "scope quiet, sweeping" while the radio's scope is quiet in CW.
    /// </summary>
    public string ToneLine
    {
        get
        {
            var tone = double.IsNaN(ToneHz)
                ? NoKeyingWords
                : string.Create(CultureInfo.InvariantCulture, $"tone {ToneHz:0} Hz heard");

            return ScopeQuiet ? ScopeQuietWords + ", " + tone : tone;
        }
    }

    /// <summary>"decoding at 742 Hz", where the decoder is mixing now: its own pitch since R102, honestly a different number from the tone (work instructions 488, 489).</summary>
    public string MixingLine => double.IsNaN(MixingHz)
        ? NotMixingWords
        : string.Create(CultureInfo.InvariantCulture, $"decoding at {MixingHz:0} Hz");

    /// <summary>A frame from what the detector holds.</summary>
    /// <param name="hops">Its history, oldest first.</param>
    /// <param name="hopMs">One hop, in milliseconds.</param>
    /// <param name="reading">Its last reading.</param>
    /// <param name="mixingHz">The pitch the decoder is mixing at, or NaN.</param>
    /// <param name="previous">The frame before this one, or null.</param>
    /// <param name="scopeQuiet">In CW, the radio's scope is quiet and the detector sweeps.</param>
    /// <returns>The frame.</returns>
    /// <remarks>
    /// **THE PITCH HOLDS ACROSS A GAP.** The detector names its pitch only while a mark is up,
    /// and a keyed station is half gaps; so while it still says keying, the pitch the last
    /// frame showed stands, and when it stops saying keying the line says so. Nothing is
    /// guessed: every pitch shown is one the detector measured during this keying.
    /// </remarks>
    public static CwScopeFrame From(
        IReadOnlyList<CwScopeHop> hops,
        double hopMs,
        CwEnvelopeReading reading,
        double mixingHz = double.NaN,
        CwScopeFrame? previous = null,
        bool scopeQuiet = false)
    {
        ArgumentNullException.ThrowIfNull(hops);
        ArgumentNullException.ThrowIfNull(reading);

        var tone = reading.Mark && double.IsFinite(reading.PitchHz)
            ? reading.PitchHz
            : reading.Keying && previous is not null ? previous.ToneHz : double.NaN;

        return new CwScopeFrame(
            hops, hopMs, reading, tone, double.IsFinite(mixingHz) && mixingHz > 0 ? mixingHz : double.NaN, scopeQuiet);
    }
}

/// <summary>
/// **THE OWNER'S EAR AND THE SCOPE ON THE CW TAB** (work instructions 474, 476 and 478,
/// steps 11 and 12, HM-DEC-184).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-27**: *"We're trying to teach you how to find the entry, how to
/// know when to start evaluating. Right now, you have no clue."* Unit 474 put a light and a
/// pitch strip on the tab to show what the detector thought; **unit 478 took them off**
/// (R92), because the scope shows what they showed in the one picture the owner said he
/// understood - a trace, and bars where the keying is.</para>
/// <para>**THE TWO BUTTONS AND THEIR ROW STAY.** The row's key set is unchanged; its
/// <c>light</c> field now records the scope's keying verdict in words, so a row written
/// after 478 says which picture the owner was judging.</para>
/// </remarks>
public sealed partial class CwHearingViewModel : ObservableObject
{
    /// <summary>
    /// What the row's <c>light</c> field says when the bars say keying (work instruction 478).
    /// </summary>
    /// <remarks>
    /// **NOT 474'S WORDS.** Rows written before 478 said "I think I hear CW" from the keying
    /// meter and the survey; these say what the envelope detector's bars say, and read
    /// differently so the two are never confused.
    /// </remarks>
    public const string BarsKeyingWords = "the bars say keying";

    /// <summary>What the row's <c>light</c> field says when the bars do not say keying.</summary>
    public const string BarsNoKeyingWords = "the bars say no keying";

    /// <summary>What "I agree with you" records, on hover.</summary>
    public const string AgreeTip =
        "Tells Hamlet the scope is right about what you hear now. It writes one row to "
        + "Hamlet's telemetry: your verdict, what the bars said, the pitches and figures "
        + "the detector is using, and the radio's frequency, mode, AGC and preamp. No audio "
        + "is kept and nothing on the radio changes.";

    /// <summary>What "You're an idiot" records, on hover.</summary>
    public const string IdiotTip =
        "Tells Hamlet the scope is wrong about what you hear now. It writes one row to "
        + "Hamlet's telemetry: your verdict, what the bars said, the pitches and figures "
        + "the detector is using, and the radio's frequency, mode, AGC and preamp. No audio "
        + "is kept and nothing on the radio changes.";

    private readonly ITelemetry? _telemetry;
    private readonly Func<CwHearingRig> _rig;
    private readonly Func<DateTime> _clock;

    private DateTime _lightChangedUtc;
    private bool _barsKeying;

    /// <summary>Creates the view model with nothing heard.</summary>
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
    }

    /// <summary>What the detector said at the last look.</summary>
    public CwHearingState State { get; private set; } = CwHearingState.None;

    /// <summary>The row's <c>light</c> field: the scope's keying verdict, in words.</summary>
    public string LightWords => _barsKeying ? BarsKeyingWords : BarsNoKeyingWords;

    /// <summary>When the bars' verdict last changed, or when this was made where it never has.</summary>
    public DateTime LightChangedUtc => _lightChangedUtc;

    /// <summary>Read what the detector says now, for the row.</summary>
    /// <param name="state">The detector's state.</param>
    public void Observe(CwHearingState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        State = state;
    }

    /// <summary>What the oscilloscope shows, on hover.</summary>
    public const string ScopeTip =
        "The last eight seconds of what Hamlet hears, newest at the right (work instruction 485)." + "\n"
        + "Blocks: every mark the detector found, drawn as long as it lasted. A dit is a short block "
        + "and a dah a long one, with flat tops, and a gap is empty space as long as the gap was." + "\n"
        + "Letters: each character the decoder settled, written over the blocks it made it from, "
        + "with a thin line as long as that stretch, and scrolling left with them. A letter is drawn "
        + "only where there are blocks beneath it. Bold is sure, faint and slanted is unsure, and the "
        + "square is heard but unreadable; a prosign is its bracketed name." + "\n"
        + "While nobody is keying nothing new is drawn here. The terminal reads the same marks this "
        + "picture draws, and a letter is only read from marks that agree on their pitch and their "
        + "loudness, so the two tell the same story; what is already drawn stays until it scrolls "
        + "off the left." + "\n"
        + "Top left: the pitch the detector found while it says keying, or no keying, and when the "
        + "radio's scope has gone quiet it says so; beside it the pitch the decoder is mixing at, "
        + "which it finds for itself and which can differ from the tone." + "\n"
        + "Hover a block for its length, or a letter for how sure the decoder was. This shows what "
        + "Hamlet hears and changes nothing about how it decodes.";

    /// <summary>What a bar is, on hover over one.</summary>
    public const string ScopeBarTip = "a mark - the level held flat for at least a dit";

    /// <summary>What the oscilloscope draws.</summary>
    [ObservableProperty]
    private CwScopeFrame _scope = CwScopeFrame.Empty;

    /// <summary>Read what the envelope detector holds now.</summary>
    /// <param name="frame">Its last four seconds and its reading.</param>
    public void ObserveScope(CwScopeFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.Reading.Keying != _barsKeying)
        {
            _barsKeying = frame.Reading.Keying;
            _lightChangedUtc = _clock();
        }

        Scope = frame;
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
        var scope = Scope.Reading;

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

            // **WHAT THE SCOPE SAW AT THE PRESS** (work instruction 476 task 3), at most one
            // redraw old, so the next unit can read whether it saw keying where he heard it.
            ["scopeEnvelopeDb"] = Measured(scope.EnvelopeDb),
            ["scopeFloorDb"] = Measured(scope.FloorDb),
            ["scopeThresholdDb"] = Measured(scope.ThresholdDb),
            ["scopeMark"] = scope.Mark,
            ["scopeRunMs"] = scope.RunMs,
            ["scopePitchHz"] = Measured(scope.PitchHz),

            // **WHERE THE DECODER WAS MIXING** (work instruction 488): trackerHz is the tracker's own
            // pitch, and while the detector says keying the decoder mixes elsewhere.
            ["mixingHz"] = Measured(Scope.MixingHz),
            ["scopeContrastDb"] = Measured(scope.ContrastDb),
            ["scopeMarksLast4s"] = scope.MarksLast4s,

            // **WHERE THE RADIO'S SCOPE SAID THE SIGNAL WAS** (work instruction 480 task 2). The
            // decibels are null on every row: the radio sends its waveform on a 0 to 160 scale
            // and nothing in this tree ties that scale to decibels, so the level goes beside it
            // as the radio sent it rather than as a number wearing a unit it has not earned.
            ["scopePeakHz"] = Measured(rig.ScopePeakHz),
            ["scopePeakDb"] = null,
            ["scopePeakLevel"] = rig.ScopePeakLevel,
            ["scopeFramesLast4s"] = rig.ScopeFramesLast4s,
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
