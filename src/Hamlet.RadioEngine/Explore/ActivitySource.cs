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
public sealed class FakeActivitySource : IActivitySource
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<ActivitySpot>> GetSpotsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        IReadOnlyList<ActivitySpot> spots = new[]
        {
            new ActivitySpot(
                "Park activation in Ohio — calling CQ in Morse at 15 WPM",
                7_032_000, "CW", "sample", now.AddMinutes(-2), 15),
            new ActivitySpot(
                "Japan reachable on FT8 — 14 US stations decoded it this minute",
                7_074_000, "FT8", "sample", now.AddMinutes(-1), null),
            new ActivitySpot(
                "Evening ragchew net starting — newcomers welcomed on check-in",
                7_188_000, "SSB", "sample", now.AddMinutes(-4), null),
            new ActivitySpot(
                "Summit activation, slow CW — the operator is cold, be quick",
                7_062_500, "CW", "sample", now.AddMinutes(-7), 12),
            new ActivitySpot(
                "Slow-speed CW club calling near the beginners' spot",
                7_055_000, "CW", "sample", now.AddMinutes(-3), 10),
            new ActivitySpot(
                "RTTY roundup practice — twin rails all over the digital corner",
                7_063_000, "RTTY", "sample", now.AddMinutes(-9), null),
            new ActivitySpot(
                "20 m open to Europe on FT8 while the sun is up",
                14_074_000, "FT8", "sample", now.AddMinutes(-5), null),
        };

        return Task.FromResult(spots);
    }
}
