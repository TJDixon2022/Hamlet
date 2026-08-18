using System.Globalization;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>How much a conditions claim can be trusted.</summary>
public enum ConditionsConfidence
{
    /// <summary>No source is answering; nothing can be said about the band.</summary>
    Blind,

    /// <summary>Too few reports to draw a conclusion from.</summary>
    Thin,

    /// <summary>Enough reports to describe the band.</summary>
    Sound,
}

/// <summary>The band-conditions line and the evidence behind it.</summary>
/// <param name="Claim">The plain-language sentence, e.g. "40 m is busy".</param>
/// <param name="Evidence">The counts and sources behind the claim.</param>
/// <param name="Confidence">How far the claim can be pushed.</param>
/// <param name="SuggestedBand">A band worth trying instead, or null.</param>
public sealed record ConditionsLine(
    string Claim, string Evidence, ConditionsConfidence Confidence, string? SuggestedBand)
{
    /// <summary>Claim and evidence as one line for a single-line control.</summary>
    public string FullText => Evidence.Length == 0 ? Claim : $"{Claim} {Evidence}";
}

/// <summary>
/// Answers the question a newcomer cannot answer alone: is tonight worth it,
/// and where should I be?
/// </summary>
/// <remarks>
/// <para>THE EVIDENCE RULE, which is the whole of HM-DEC-025 in one place: a
/// plain-language claim, the evidence shown beside it, phrasing that softens
/// when the sample is thin, and an outright confession when the sources are
/// not answering. "40 m looks quiet" and "40 m looks quiet — but POTA and RBN
/// are both down" are different statements, and a line that cannot tell them
/// apart is worse than no line at all.</para>
/// <para>Hamlet never invents calm. A silent band and a broken feed produce
/// identical spot counts, so the counts alone can never be trusted to
/// distinguish them; only the source statuses can, and they are therefore an
/// input here rather than a detail of the plumbing.</para>
/// <para>The empty answer is a first-class result. An operator who quits after
/// an hour on a dead band is exactly who this line is for, so "nothing here,
/// try there" is a success — and when another band has traffic, the line says
/// which one and how it knows.</para>
/// <para>Pure functions of a spot set, an elapsed window and the source
/// statuses. No clock read (§5).</para>
/// </remarks>
public static class BandConditions
{
    /// <summary>
    /// The window the line reports on.
    /// </summary>
    /// <remarks>
    /// Widened from ten minutes to an hour (HM-DEC-045). Ten was never a
    /// considered figure, and once the lead card started counting history the
    /// two lines contradicted each other on screen: "nine on 40 m" sitting
    /// directly above "no spots in the last 10 minutes". The count and the
    /// claim have to be measured over the same ground.
    /// </remarks>
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(60);

    /// <summary>Under this many reports, the wording softens.</summary>
    public const int ThinSample = 5;

    /// <summary>At or over this many reports, the band is called busy.</summary>
    public const int BusyThreshold = 25;

    /// <summary>At or over this, the band is called unusually busy.</summary>
    public const int VeryBusyThreshold = 50;

    /// <summary>At or under this, the band is called quiet.</summary>
    public const int QuietThreshold = 4;

    /// <summary>
    /// Describe the band on screen.
    /// </summary>
    /// <param name="bandName">Band on screen, e.g. "40 m".</param>
    /// <param name="spotsOnBand">Spots currently held for that band.</param>
    /// <param name="allSpots">Every spot held, all bands — the evidence for
    /// suggesting somewhere else.</param>
    /// <param name="statuses">What each source contributed.</param>
    /// <param name="nowUtc">Reference time for spot ages.</param>
    /// <returns>The claim, its evidence, and where to go instead.</returns>
    public static ConditionsLine Describe(
        string bandName,
        IReadOnlyList<ActivitySpot> spotsOnBand,
        IReadOnlyList<ActivitySpot> allSpots,
        IReadOnlyList<SourceStatus> statuses,
        DateTime nowUtc)
    {
        var answering = statuses.Where(s => s.State == SourceState.Ok).ToList();
        var letDown = statuses.Where(s => s.IsLetDown).ToList();
        var enabled = statuses.Where(s => s.State != SourceState.Disabled).ToList();

        // Nothing is answering: say that, and say nothing about the band.
        if (answering.Count == 0)
        {
            return new ConditionsLine(
                "Hamlet cannot see the bands right now.",
                enabled.Count == 0
                    ? "Every spot source is switched off in Settings."
                    : $"No answer from {NameList(enabled.Select(s => s.Name))}. "
                      + "That says nothing about whether the band is busy.",
                ConditionsConfidence.Blind,
                null);
        }

        var recent = spotsOnBand.Where(s => nowUtc - s.HeardAtUtc <= Window).ToList();
        var cw = recent.Count(s => string.Equals(s.Mode, "CW", StringComparison.OrdinalIgnoreCase));
        var alternative = SuggestBand(bandName, allSpots, nowUtc);

        var sources = NameList(answering.Select(s => s.Name));
        var missing = letDown.Count > 0
            ? $" {NameList(letDown.Select(s => s.Name))} {(letDown.Count == 1 ? "isn't" : "aren't")} answering right now."
            : "";

        // Nothing on this band, but the sources are healthy.
        if (recent.Count == 0)
        {
            var claim = alternative is null
                ? $"Nobody's on {bandName} from here right now."
                : $"Nobody's on {bandName} from here right now. Try {alternative}.";

            return new ConditionsLine(
                claim,
                $"No spots in the last {Minutes(Window)} minutes, from {sources}.{missing}",
                letDown.Count > 0 ? ConditionsConfidence.Thin : ConditionsConfidence.Sound,
                alternative);
        }

        var confidence = recent.Count < ThinSample || letDown.Count > 0
            ? ConditionsConfidence.Thin
            : ConditionsConfidence.Sound;

        var evidence =
            $"{Count(recent.Count, "signal")} in the last {Minutes(Window)} minutes"
            + (cw > 0 && cw != recent.Count ? $", {cw} of them CW" : "")
            + $", from {sources}.{missing}";

        return new ConditionsLine(
            BuildClaim(bandName, recent.Count, cw, confidence, alternative),
            evidence,
            confidence,
            alternative);
    }

    private static string BuildClaim(
        string bandName, int total, int cw, ConditionsConfidence confidence, string? alternative)
    {
        // A thin sample never gets a confident verb. Four signals is not
        // evidence that a band is quiet; it is evidence that four people were
        // heard.
        if (confidence == ConditionsConfidence.Thin)
        {
            var tail = alternative is null ? "" : $" {alternative} looks better.";
            return total <= QuietThreshold
                ? $"{bandName} looks thin, but that is too little to be sure.{tail}"
                : $"{bandName} has something going on, though not enough to call it.{tail}";
        }

        if (total >= VeryBusyThreshold)
        {
            return $"{bandName} is unusually busy.";
        }

        if (total >= BusyThreshold)
        {
            return $"{bandName} is busy, and a good night to be on it.";
        }

        if (total <= QuietThreshold)
        {
            var tail = alternative is null
                ? " Worth a listen, but do not expect a queue."
                : $" Try {alternative}.";
            return $"{bandName} looks quiet.{tail}";
        }

        return cw >= total / 2 && cw > 0
            ? $"{bandName} is ticking over, mostly CW."
            : $"{bandName} is ticking over.";
    }

    /// <summary>
    /// A band with more going on than the one on screen, or null.
    /// </summary>
    /// <remarks>
    /// Only suggested when the evidence is real: the other band must have a
    /// clear margin, so the operator is not sent chasing a difference of one
    /// spot. The counts come from the whole-spectrum sources (POTA and SOTA),
    /// which is why they are not filtered to the band on screen.
    /// </remarks>
    private static string? SuggestBand(
        string bandName, IReadOnlyList<ActivitySpot> allSpots, DateTime nowUtc)
    {
        var here = 0;
        var elsewhere = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var spot in allSpots)
        {
            if (nowUtc - spot.HeardAtUtc > Window)
            {
                continue;
            }

            var band = BandPlanName(spot.FrequencyHz);
            if (band is null)
            {
                continue;
            }

            if (string.Equals(band, bandName, StringComparison.Ordinal))
            {
                here++;
                continue;
            }

            elsewhere[band] = elsewhere.GetValueOrDefault(band) + 1;
        }

        if (elsewhere.Count == 0)
        {
            return null;
        }

        var best = elsewhere.OrderByDescending(p => p.Value).First();
        return best.Value >= Math.Max(3, here * 2) ? best.Key : null;
    }

    private static string? BandPlanName(long hz)
        => Hamlet.RadioEngine.Bands.HfBands.BandFor(hz)?.Name;

    private static string Count(int n, string noun)
        => n == 1
            ? $"1 {noun}"
            : $"{n.ToString(CultureInfo.InvariantCulture)} {noun}s";

    private static string Minutes(TimeSpan span)
        => ((int)span.TotalMinutes).ToString(CultureInfo.InvariantCulture);

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
