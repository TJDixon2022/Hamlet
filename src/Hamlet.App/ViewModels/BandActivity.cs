using System.Globalization;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>What is known about a band's activity right now.</summary>
public enum BandActivityState
{
    /// <summary>
    /// Nothing can be said: no enabled, healthy source reports on this band.
    /// </summary>
    /// <remarks>
    /// Distinct from <see cref="NothingHeard"/> on purpose. "I cannot see this
    /// band" and "I am watching and hearing nothing" are different claims and
    /// must not render identically (HM-DEC-031).
    /// </remarks>
    NoData,

    /// <summary>Sources are watching this band and have heard nothing.</summary>
    NothingHeard,

    /// <summary>Something was heard.</summary>
    Heard,
}

/// <summary>One band's activity indicator and the evidence behind it.</summary>
/// <param name="BandName">Band name, e.g. "40 m".</param>
/// <param name="State">Whether anything can be said at all.</param>
/// <param name="SpotCount">Spots inside the window.</param>
/// <param name="CwCount">How many of them were CW.</param>
/// <param name="Pips">
/// Filled pips, 0 to <see cref="BandActivity.MaxPips"/>, scaled against the
/// busiest band on display right now.
/// </param>
/// <param name="Claim">The plain-language verdict, e.g. "busy".</param>
/// <param name="Evidence">The counts and the sources they came from.</param>
/// <param name="Confidence">How far the claim can be pushed.</param>
public sealed record BandActivityReading(
    string BandName,
    BandActivityState State,
    int SpotCount,
    int CwCount,
    int Pips,
    string Claim,
    string Evidence,
    ConditionsConfidence Confidence)
{
    /// <summary>The hover text: claim first, then its receipts.</summary>
    /// <remarks>
    /// The band and the claim are separated by the middle dot the rest of the
    /// app uses for label runs, rather than by a dash. It is a separator and
    /// not punctuation, so it reads the same way "CW · POTA · 2 min ago" does
    /// and stays out of the prose rule's way (HM-DEC-040).
    /// </remarks>
    public string Tooltip
        => Evidence.Length == 0
            ? $"{BandName} · {Claim}"
            : $"{BandName} · {Claim} {Evidence}";

    /// <summary>True when the indicator should render as unknown rather than empty.</summary>
    public bool IsUnknown => State == BandActivityState.NoData;
}

/// <summary>
/// Summarizes how busy each band is, from the spots already in hand.
/// </summary>
/// <remarks>
/// <para>The band buttons are the first control anybody touches, and six of
/// the seven used to say nothing. A newcomer picking a band was guessing. The
/// data to fix that was already flowing (HM-DEC-031).</para>
/// <para>THE HONESTY CONSTRAINT, which shapes every string below. A spot count
/// is a proxy for ACTIVITY, not for propagation and not for whether this
/// operator could work anything. RBN counts say where skimmers are; POTA says
/// where activators went. So nothing here asserts that a band is open or
/// closed. It reports what was heard, names who was listening, and lets the
/// operator draw the conclusion. The single hedged exception — "likely closed
/// rather than unwatched" — is allowed only when every source that can see the
/// band is healthy and reporting zero, and even then it is hedged in the
/// sentence itself.</para>
/// <para>WHICH IS WHY SOURCE SCOPE MATTERS. RBN is filtered to the band on
/// screen, so its silence about 17 m is not evidence about 17 m. Crediting it
/// with that silence would manufacture confidence out of a source that was
/// never pointed there, which is the exact failure HM-DEC-025 exists to
/// prevent. Every band is summarized only from the sources that can actually
/// see it.</para>
/// <para>Pure functions of a spot set, a window and the source statuses. No
/// clock read (§5), same shape as <see cref="BandConditions"/>.</para>
/// </remarks>
public static class BandActivity
{
    /// <summary>The window each reading covers.</summary>
    /// <remarks>Shared with the conditions line, so the button and the panel
    /// beneath it are never counting different minutes.</remarks>
    public static readonly TimeSpan Window = BandConditions.Window;

    /// <summary>Pips in a full indicator.</summary>
    public const int MaxPips = 4;

    /// <summary>Under this many spots, the wording softens.</summary>
    public const int ThinSample = BandConditions.ThinSample;

    /// <summary>At or over this many spots, a band is called busy.</summary>
    public const int BusyThreshold = BandConditions.BusyThreshold;

    /// <summary>
    /// Summarize every band on display.
    /// </summary>
    /// <param name="bands">The bands shown as buttons.</param>
    /// <param name="allSpots">Every spot held, across all bands.</param>
    /// <param name="statuses">What each source contributed.</param>
    /// <param name="nowUtc">Reference time for spot ages.</param>
    /// <returns>One reading per band, in the order given.</returns>
    /// <remarks>
    /// The scale is relative and computed across this call: the busiest band
    /// right now sets the top of the range. An absolute scale would be a
    /// number nobody can calibrate — "34 spots" means nothing without knowing
    /// whether 34 is a lot tonight.
    /// </remarks>
    public static IReadOnlyList<BandActivityReading> Summarize(
        IReadOnlyList<CwBand> bands,
        IReadOnlyList<ActivitySpot> allSpots,
        IReadOnlyList<SourceStatus> statuses,
        DateTime nowUtc)
    {
        var counts = new Dictionary<string, (int Total, int Cw)>(StringComparer.Ordinal);

        foreach (var band in bands)
        {
            counts[band.Name] = (0, 0);
        }

        foreach (var spot in allSpots)
        {
            if (nowUtc - spot.HeardAtUtc > Window)
            {
                continue;
            }

            var band = bands.FirstOrDefault(
                b => spot.FrequencyHz >= b.LowHz && spot.FrequencyHz <= b.HighHz);

            if (band is null)
            {
                continue;
            }

            var (total, cw) = counts[band.Name];
            counts[band.Name] = (
                total + 1,
                cw + (string.Equals(spot.Mode, "CW", StringComparison.OrdinalIgnoreCase) ? 1 : 0));
        }

        var busiest = counts.Values.Count == 0 ? 0 : counts.Values.Max(c => c.Total);

        return bands
            .Select(b => Read(b.Name, counts[b.Name], busiest, statuses))
            .ToList();
    }

    private static BandActivityReading Read(
        string bandName,
        (int Total, int Cw) count,
        int busiest,
        IReadOnlyList<SourceStatus> statuses)
    {
        // Only sources that can actually see this band get a say about it.
        var covering = statuses.Where(s => s.CoversBand(bandName)).ToList();
        var answering = covering.Where(s => s.State == SourceState.Ok).ToList();
        var letDown = covering.Where(s => s.IsLetDown).ToList();

        // Nothing is watching this band: say so, and say nothing else.
        if (answering.Count == 0)
        {
            var enabled = covering.Where(s => s.State != SourceState.Disabled).ToList();

            return new BandActivityReading(
                bandName,
                BandActivityState.NoData,
                0,
                0,
                0,
                "no data.",
                enabled.Count == 0
                    ? "No enabled source is reporting on this band right now."
                    : $"No answer from {NameList(enabled.Select(s => s.Name))}, so nothing "
                      + "is being reported on this band right now.",
                ConditionsConfidence.Blind);
        }

        var from = $"From {NameList(answering.Select(s => s.Name))}.";
        var missing = letDown.Count > 0
            ? $" {NameList(letDown.Select(s => s.Name))} "
              + $"{(letDown.Count == 1 ? "isn't" : "aren't")} answering right now."
            : "";

        if (count.Total == 0)
        {
            // The one hedged sentence in this file. It is reachable only when
            // every source that can see this band is healthy, and even then it
            // says "likely" and names the possibility it cannot rule out. With
            // any source down, the evidence is stated flatly instead — a gap
            // in the watch is not evidence about the band.
            var silence = letDown.Count == 0
                ? $"{Sources(answering)} answering, so the band is likely closed rather "
                  + "than unwatched."
                : from + missing;

            return new BandActivityReading(
                bandName,
                BandActivityState.NothingHeard,
                0,
                0,
                0,
                $"nothing heard in the last {Minutes()} minutes.",
                silence,
                letDown.Count > 0 ? ConditionsConfidence.Thin : ConditionsConfidence.Sound);
        }

        var thin = count.Total < ThinSample || letDown.Count > 0;

        var claim = count.Total >= BusyThreshold
            ? "busy."
            : thin
                ? "quiet."
                : "ticking over.";

        var evidence = $"{Count(count.Total, "signal")} in the last {Minutes()} minutes"
            + (count.Cw > 0 && count.Cw != count.Total ? $", {count.Cw} of them CW" : "")
            + (count.Total < ThinSample ? ", but that is too little to be sure" : "")
            + $". {from}{missing}";

        return new BandActivityReading(
            bandName,
            BandActivityState.Heard,
            count.Total,
            count.Cw,
            PipsFor(count.Total, busiest),
            claim,
            evidence,
            thin ? ConditionsConfidence.Thin : ConditionsConfidence.Sound);
    }

    /// <summary>
    /// Scale a count against the busiest band on display.
    /// </summary>
    /// <param name="count">Spots on this band.</param>
    /// <param name="busiest">Spots on the busiest band.</param>
    /// <returns>Pips from 1 to <see cref="MaxPips"/>; 0 for no spots.</returns>
    /// <remarks>
    /// <para>A band with anything on it gets at least one pip. The indicator
    /// answers "which of these is worth a look right now", and a band that
    /// rounded away to nothing would be indistinguishable from one that heard
    /// nothing at all.</para>
    /// <para>The scale is the square root of the ratio, not the ratio itself.
    /// Band activity is heavily tailed — one band routinely carries several
    /// times the traffic of every other — so a linear scale across four pips
    /// puts almost everything in the bottom bucket and the indicator stops
    /// distinguishing anything. Compressing the top of the range is also the
    /// idiom of the domain: S-meters and signal reports are logarithmic for
    /// the same reason.</para>
    /// </remarks>
    public static int PipsFor(int count, int busiest)
    {
        if (count <= 0)
        {
            return 0;
        }

        if (busiest <= 0)
        {
            return 1;
        }

        var scaled = (int)Math.Ceiling(Math.Sqrt(count / (double)busiest) * MaxPips);
        return Math.Clamp(scaled, 1, MaxPips);
    }

    private static string Sources(IReadOnlyList<SourceStatus> answering)
    {
        var names = NameList(answering.Select(s => s.Name));

        return answering.Count switch
        {
            1 => $"{names} is",
            2 => $"{names} are both",
            _ => $"{names} are all",
        };
    }

    private static string Count(int n, string noun)
        => n == 1
            ? $"1 {noun}"
            : $"{n.ToString(CultureInfo.InvariantCulture)} {noun}s";

    private static string Minutes()
        => ((int)Window.TotalMinutes).ToString(CultureInfo.InvariantCulture);

    private static string NameList(IEnumerable<string> names)
    {
        var list = names.ToList();
        return list.Count switch
        {
            0 => "no sources",
            1 => list[0],
            2 => $"{list[0]} and {list[1]}",
            _ => string.Join(", ", list.Take(list.Count - 1)) + " and " + list[^1],
        };
    }
}
