namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// The engine's seam for "what's happening on the air" (HM-DEC-016). Live
/// implementations (RBN, POTA, SOTA) slide in behind it exactly as Ic7300Rig
/// slid in behind TrainingRig.
/// </summary>
public interface IActivitySource
{
    /// <summary>
    /// Short stable name shown to the operator and used as the settings key,
    /// e.g. "POTA". Never localised, never generated.
    /// </summary>
    string Name { get; }

    /// <summary>Current spots, freshest first. Never throws for an
    /// unreachable network — an empty list with a stale timestamp is the
    /// honest failure mode.</summary>
    /// <param name="cancellationToken">Cancels the fetch.</param>
    /// <returns>The spots this source can currently vouch for.</returns>
    Task<IReadOnlyList<ActivitySpot>> GetSpotsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Context a source may need to shape what it asks for: which band the
/// operator is looking at, and where they are.
/// </summary>
/// <remarks>
/// Pushed in rather than read from settings, because the engine owns radio
/// knowledge but knows nothing about the shell's settings file (§0.1). RBN
/// uses all of it; POTA and SOTA use the band only.
/// </remarks>
public sealed class ActivityContext
{
    /// <summary>Lower edge of the band on screen, in hertz.</summary>
    public long BandLowHz { get; set; }

    /// <summary>Name of the band on screen, e.g. "40 m".</summary>
    /// <remarks>
    /// Carried so a band-scoped source can say WHICH band it is limited to.
    /// A source that only watches one band must be able to declare that, or
    /// the UI ends up crediting it with silence on bands it never looked at
    /// (HM-DEC-031).
    /// </remarks>
    public string BandName { get; set; } = "";

    /// <summary>Upper edge of the band on screen, in hertz.</summary>
    public long BandHighHz { get; set; } = 30_000_000;

    /// <summary>The operator's US call district, or null when unknown.</summary>
    public int? HomeDistrict { get; set; }

    /// <summary>True when the operator is known to be in North America.</summary>
    public bool HomeInNorthAmerica { get; set; }

    /// <summary>True when <paramref name="hz"/> is on the band on screen.</summary>
    /// <param name="hz">Frequency in hertz.</param>
    /// <returns>True when in band.</returns>
    public bool IsInBand(long hz) => hz >= BandLowHz && hz <= BandHighHz;
}

/// <summary>
/// A source that can only report on one band at a time.
/// </summary>
/// <remarks>
/// RBN is the case this exists for. It is filtered to the band on screen at
/// the source, because six spots a second worldwide is unusable otherwise
/// (HM-DEC-024) — which means its silence about 17 m is not evidence about
/// 17 m, it is evidence that nobody asked it. Anything summarising per-band
/// activity has to know the difference, or it will report "RBN is answering
/// and heard nothing here" about a band RBN never looked at (HM-DEC-031).
/// </remarks>
public interface IBandScopedActivitySource : IActivitySource
{
    /// <summary>The only band this source currently reports on, or null when
    /// it reports across the whole spectrum.</summary>
    string? ScopedBandName { get; }
}

/// <summary>A source that wants to know which band and operator it serves.</summary>
public interface IContextualActivitySource : IActivitySource
{
    /// <summary>Tell the source what the operator is looking at.</summary>
    /// <param name="context">Current band and operator location.</param>
    void SetContext(ActivityContext context);
}

/// <summary>
/// Fixture spots for building and demonstrating the Explorer with no
/// network. Every spot is plainly labeled Source = "sample" — the UI shows
/// that label, so even the demo obeys the prime directive.
/// </summary>
/// <remarks>
/// The feed moves between calls on purpose (HM-DEC-020): ages are recomputed
/// from the clock every call, and one rotating slot swaps its spot every few
/// calls. A fixture that returns the identical list forever cannot exercise
/// the auto-refresh path's new-arrival handling, so the bug would only ever
/// appear once live feeds landed behind this seam.
/// </remarks>
public sealed class FakeActivitySource : IActivitySource
{
    /// <summary>The settings key and display name for this source.</summary>
    public const string SourceName = "Sample";

    /// <summary>Calls between swaps of the rotating slot.</summary>
    private const int RotateEveryCalls = 3;

    /// <summary>The steady spots: present on every call, ages advancing.</summary>
    private static readonly SampleSpot[] Steady =
    {
        new("Park activation in Ohio — calling CQ in Morse at 15 WPM",
            7_032_000, "CW", 2, 15, SpotCallType.Cq, true, "US-OH", 14),
        new("Japan reachable on FT8 — 14 US stations decoded it this minute",
            7_074_000, "FT8", 1, null, SpotCallType.Unknown, false, null, null),
        new("Evening ragchew net starting — newcomers welcomed on check-in",
            7_188_000, "SSB", 4, null, SpotCallType.Unknown, false, null, null),
        new("Summit activation, slow CW — the operator is cold, be quick",
            7_062_500, "CW", 7, 12, SpotCallType.Cq, true, "W3", 9),
        new("Slow-speed CW club calling near the beginners' spot",
            7_055_000, "CW", 3, 10, SpotCallType.Cq, false, null, 21),
        new("RTTY roundup practice — twin rails all over the digital corner",
            7_063_000, "RTTY", 9, null, SpotCallType.Contest, false, null, 6),
    };

    /// <summary>The rotating slot: one of these, changing every few calls.</summary>
    private static readonly SampleSpot[] Rotating =
    {
        new("20 m open to Europe on FT8 while the sun is up",
            14_074_000, "FT8", 5, null, SpotCallType.Unknown, false, null, null),
        new("A beacon-like CQ machine near the QRP watering hole — easy first copy",
            7_030_000, "CW", 1, 13, SpotCallType.Cq, false, null, 18),
        new("Straight-key night warm-up — hand-sent CW, plenty of character",
            7_058_000, "CW", 2, 14, SpotCallType.Cq, false, null, 11),
        new("PSK31 ribbon on the digital shelf — someone is typing to Spain",
            14_070_000, "PSK31", 6, null, SpotCallType.Unknown, false, null, null),
    };

    private int _calls;

    /// <inheritdoc/>
    public string Name => SourceName;

    /// <inheritdoc/>
    public Task<IReadOnlyList<ActivitySpot>> GetSpotsAsync(
        CancellationToken cancellationToken = default)
    {
        var call = Interlocked.Increment(ref _calls) - 1;
        var now = DateTime.UtcNow;

        var spots = new List<ActivitySpot>(Steady.Length + 1);
        foreach (var s in Steady)
        {
            spots.Add(s.ToSpot(now));
        }

        spots.Add(Rotating[call / RotateEveryCalls % Rotating.Length].ToSpot(now));

        return Task.FromResult<IReadOnlyList<ActivitySpot>>(spots);
    }

    private sealed record SampleSpot(
        string Story, long Hz, string Mode, int AgeMinutes, int? Wpm,
        SpotCallType CallType, bool IsActivation, string? Place, int? SignalDb)
    {
        public ActivitySpot ToSpot(DateTime now) => new(
            Story, Hz, Mode, "sample", now.AddMinutes(-AgeMinutes), Wpm)
        {
            CallType = CallType,
            IsActivation = IsActivation,
            PlaceLabel = Place,
            SignalDb = SignalDb,
            Proximity = Place is null ? SpotProximity.Unknown : SpotProximity.Local,
        };
    }
}
