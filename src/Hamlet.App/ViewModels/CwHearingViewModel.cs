using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Hamlet.RadioEngine.Cw;

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

    private readonly Func<DateTime> _clock;

    private DateTime _lightChangedUtc;

    /// <summary>Creates the light, dark.</summary>
    /// <param name="clock">The clock, or null for the system's.</param>
    public CwHearingViewModel(Func<DateTime>? clock = null)
    {
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

    private static string VerdictWord(KeyingVerdict verdict) => verdict switch
    {
        KeyingVerdict.Keying => "keying",
        KeyingVerdict.NoKeying => "no keying",
        _ => "listening",
    };
}
