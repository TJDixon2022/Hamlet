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
    }

    /// <summary>What the detector said at the last look.</summary>
    public CwHearingState State { get; private set; } = CwHearingState.None;

    /// <summary>Whether the light is lit.</summary>
    [ObservableProperty]
    private bool _isLit;

    /// <summary>What the light says.</summary>
    [ObservableProperty]
    private string _lightWords = DarkWords;

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
    }
}
