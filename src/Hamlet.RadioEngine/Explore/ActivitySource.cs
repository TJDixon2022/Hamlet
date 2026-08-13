namespace Hamlet.RadioEngine.Explore;

/// <summary>One thing happening on the air, told in plain language.</summary>
/// <param name="Story">The invitation, e.g. "Park activation in Ohio —
/// calling CQ in Morse at 15 WPM".</param>
/// <param name="FrequencyHz">Where to tune.</param>
/// <param name="Mode">Mode name, matching a field-guide entry when possible.</param>
/// <param name="Source">Which network reported it (RBN, POTA, fixture…).
/// A spot is a third party's claim; the source is shown, per the prime
/// directive.</param>
/// <param name="HeardAtUtc">When it was reported. Age is shown, never hidden —
/// a stale spot dressed as live is a lie.</param>
/// <param name="Wpm">CW speed when known; the hook for copy-speed filtering
/// (FG-002).</param>
public sealed record ActivitySpot(
    string Story, long FrequencyHz, string Mode, string Source,
    DateTime HeardAtUtc, int? Wpm);

/// <summary>
/// The engine's seam for "what's happening on the air" (HM-DEC-016). Live
/// implementations (RBN, POTA, PSK Reporter, contest calendars) slide in
/// behind it exactly as Ic7300Rig slid in behind FakeRig.
/// </summary>
public interface IActivitySource
{
    /// <summary>Current spots, freshest first. Never throws for an
    /// unreachable network — an empty list with a stale timestamp is the
    /// honest failure mode.</summary>
    Task<IReadOnlyList<ActivitySpot>> GetSpotsAsync(
        CancellationToken cancellationToken = default);
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
    /// <summary>Calls between swaps of the rotating slot.</summary>
    private const int RotateEveryCalls = 3;

    /// <summary>The steady spots: present on every call, ages advancing.</summary>
    private static readonly (string Story, long Hz, string Mode, int AgeMinutes, int? Wpm)[] Steady =
    {
        ("Park activation in Ohio — calling CQ in Morse at 15 WPM",
            7_032_000, "CW", 2, 15),
        ("Japan reachable on FT8 — 14 US stations decoded it this minute",
            7_074_000, "FT8", 1, null),
        ("Evening ragchew net starting — newcomers welcomed on check-in",
            7_188_000, "SSB", 4, null),
        ("Summit activation, slow CW — the operator is cold, be quick",
            7_062_500, "CW", 7, 12),
        ("Slow-speed CW club calling near the beginners' spot",
            7_055_000, "CW", 3, 10),
        ("RTTY roundup practice — twin rails all over the digital corner",
            7_063_000, "RTTY", 9, null),
    };

    /// <summary>The rotating slot: one of these, changing every few calls.</summary>
    private static readonly (string Story, long Hz, string Mode, int AgeMinutes, int? Wpm)[] Rotating =
    {
        ("20 m open to Europe on FT8 while the sun is up",
            14_074_000, "FT8", 5, null),
        ("A beacon-like CQ machine near the QRP watering hole — easy first copy",
            7_030_000, "CW", 1, 13),
        ("Straight-key night warm-up — hand-sent CW, plenty of character",
            7_058_000, "CW", 2, 14),
        ("PSK31 ribbon on the digital shelf — someone is typing to Spain",
            14_070_000, "PSK31", 6, null),
    };

    private int _calls;

    /// <inheritdoc/>
    public Task<IReadOnlyList<ActivitySpot>> GetSpotsAsync(
        CancellationToken cancellationToken = default)
    {
        var call = Interlocked.Increment(ref _calls) - 1;
        var now = DateTime.UtcNow;

        var spots = new List<ActivitySpot>(Steady.Length + 1);
        foreach (var s in Steady)
        {
            spots.Add(Make(s, now));
        }

        spots.Add(Make(Rotating[call / RotateEveryCalls % Rotating.Length], now));

        return Task.FromResult<IReadOnlyList<ActivitySpot>>(spots);
    }

    private static ActivitySpot Make(
        (string Story, long Hz, string Mode, int AgeMinutes, int? Wpm) s, DateTime now)
        => new(s.Story, s.Hz, s.Mode, "sample", now.AddMinutes(-s.AgeMinutes), s.Wpm);
}
