using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>
/// The one suggestion at the top of the Explorer, for somebody with no idea
/// where to start.
/// </summary>
/// <param name="HasSuggestion">False when nothing on the band is worth
/// sending a beginner to.</param>
/// <param name="Headline">One line naming the opportunity.</param>
/// <param name="Body">Why it suits this operator and what to expect.</param>
/// <param name="Reason">The same evidence line the ranked card carries.</param>
/// <param name="TuneHz">Where the Tune button goes; 0 when there is nothing
/// to tune to.</param>
/// <param name="FrequencyLabel">Frequency in megahertz for the button.</param>
public sealed record LeadSuggestion(
    bool HasSuggestion, string Headline, string Body, string Reason,
    long TuneHz, string FrequencyLabel);

/// <summary>
/// Picks the lead suggestion and writes it in plain language.
/// </summary>
/// <remarks>
/// <para>This is the answer to the question the app exists for: "show me
/// something interesting, right now, and tell me why it's interesting". A
/// licensed operator who has spent six years tuning around and finding
/// nothing does not need another list — they need one sentence telling them
/// where to point the radio and what they will hear when they get there.</para>
/// <para>So the card is written, not templated from a spot's fields alone: it
/// says what it is, why it suits this operator, and what to expect on
/// arrival. The "what to expect" half matters most. Somebody who has never
/// answered a CQ does not know that you listen through a couple of repeats
/// first, or that an activator will come back slowly if you send slowly.</para>
/// <para>THE REFUSAL CASE IS THE IMPORTANT ONE (HM-DEC-025). When nothing
/// clears the bar the card says so and says what to do instead. Inventing an
/// encouraging suggestion out of a beacon and a contest run would be exactly
/// the failure HM-DEC-009 forbids, and it would cost this operator another
/// wasted evening — which is the outcome the whole feature exists to
/// prevent.</para>
/// </remarks>
public static class LeadCard
{
    /// <summary>
    /// A spot must score at least this well to be put in front of a beginner
    /// as "the thing to do next".
    /// </summary>
    /// <remarks>
    /// Set so that a bare in-band spot with no call type and no speed cannot
    /// reach it, while a fresh CQ that is either slow or nearby can. The
    /// threshold is the whole difference between a recommendation and a
    /// shrug, so it is a named constant with a test on both sides of it.
    /// </remarks>
    public const int MinimumScore = 40;

    /// <summary>What a beginner is told when nothing qualifies.</summary>
    public const string NothingHeadline = "Nothing here worth your next ten minutes";

    /// <summary>
    /// Choose the lead suggestion from a ranked list.
    /// </summary>
    /// <param name="ranked">Spots already ranked, best first.</param>
    /// <param name="bandName">The band on screen, e.g. "40 m".</param>
    /// <param name="anySourceAnswering">False when no spot source is
    /// answering, which changes the wording of the refusal.</param>
    /// <returns>The suggestion, or a refusal that says what to do instead.</returns>
    public static LeadSuggestion Choose(
        IReadOnlyList<RankedSpot> ranked, string bandName, bool anySourceAnswering = true)
    {
        var best = ranked.FirstOrDefault(r => IsSuitable(r));

        if (best is null)
        {
            return NoSuggestion(bandName, anySourceAnswering, ranked.Count);
        }

        return new LeadSuggestion(
            true,
            Headline(best),
            Body(best),
            best.Reason,
            best.Spot.FrequencyHz,
            FormatMhz(best.Spot.FrequencyHz));
    }

    /// <summary>
    /// Whether a spot may be recommended at all.
    /// </summary>
    /// <param name="ranked">The scored spot.</param>
    /// <returns>True when it is worth a beginner's time.</returns>
    public static bool IsSuitable(RankedSpot ranked)
    {
        // A beacon transmits to nobody. However strong and however close, it
        // is never the answer to "who can I talk to".
        if (ranked.Spot.CallType == SpotCallType.Beacon)
        {
            return false;
        }

        return ranked.Score >= MinimumScore;
    }

    private static string Headline(RankedSpot ranked)
    {
        var spot = ranked.Spot;
        var where = string.IsNullOrWhiteSpace(spot.PlaceLabel) ? "" : $" in {spot.PlaceLabel}";

        if (spot.IsActivation)
        {
            var kind = string.Equals(spot.Source, "SOTA", StringComparison.OrdinalIgnoreCase)
                ? "a summit"
                : "a park";
            var who = string.IsNullOrWhiteSpace(spot.DxCall) ? "Someone" : spot.DxCall;
            var speed = spot.Wpm is not null ? $" at {spot.Wpm} WPM" : "";
            return $"{who} is calling CQ{speed} from {kind}{where}";
        }

        if (spot.CallType == SpotCallType.Cq)
        {
            var who = string.IsNullOrWhiteSpace(spot.DxCall) ? "Someone" : spot.DxCall;
            var speed = spot.Wpm is not null ? $" at {spot.Wpm} WPM" : "";
            return $"{who} is calling CQ{speed} — and wants an answer";
        }

        return spot.Story;
    }

    /// <summary>
    /// Why it suits this operator, then what to expect on arrival.
    /// </summary>
    private static string Body(RankedSpot ranked)
    {
        var spot = ranked.Spot;
        var why = new List<string>();

        if (spot.Wpm is { } wpm && wpm <= SpotRanking.CopyableWpm)
        {
            why.Add(wpm <= SpotRanking.ComfortableWpm
                ? "Slow enough to copy while you are still counting"
                : "Close to a copyable speed");
        }

        if (spot.Proximity == SpotProximity.Local)
        {
            why.Add("close enough that your antenna should hear it");
        }
        else if (spot.ReportCount is { } reports && reports >= 5)
        {
            why.Add($"{reports} receivers are hearing it, so yours probably can too");
        }
        else if (spot.SignalDb is >= SpotRanking.StrongSignalDb)
        {
            why.Add("coming in strong");
        }

        if (spot.IsActivation)
        {
            // Reads as a closing clause when it follows something, and as a
            // whole reason when it stands alone.
            why.Add(why.Count == 0
                ? "An activator needs contacts, so they will be patient with you"
                : "and an activator needs contacts, so they will be patient with you");
        }

        var whyText = why.Count == 0
            ? "It is the best thing on the band right now."
            : Sentence(why);

        return whyText + " " + WhatToExpect(spot);
    }

    /// <summary>
    /// The half a newcomer cannot get anywhere else: what actually happens
    /// when they tune there.
    /// </summary>
    private static string WhatToExpect(ActivitySpot spot)
    {
        var mode = (spot.Mode ?? "").ToUpperInvariant();

        return mode switch
        {
            "CW" => "Listen for a few repeats before you answer — you will hear the "
                    + "rhythm of the call before you can read it. Then send your "
                    + "callsign once, at your speed, and wait.",
            "SSB" => "Listen for a gap, then give your callsign once, clearly. "
                     + "If nobody comes back, wait for the next round and try again.",
            "FT8" or "FT4" => "This one is the computer's job, not the key's — Hamlet "
                              + "cannot decode it yet (phase 3), so treat it as a sign "
                              + "the band is open rather than a contact to make.",
            _ => "Tune there and listen for a minute before you do anything else. "
                 + "Knowing what a busy frequency sounds like is worth the minute.",
        };
    }

    private static LeadSuggestion NoSuggestion(
        string bandName, bool anySourceAnswering, int rankedCount)
    {
        if (!anySourceAnswering)
        {
            return new LeadSuggestion(
                false,
                "Hamlet cannot see the bands right now",
                "No spot source is answering, so there is nothing to recommend and "
                + "Hamlet will not guess. The band may be busy or empty — this is "
                + "Hamlet's problem, not the ionosphere's. Check the Sources section "
                + "in Settings, or tune around and trust your own ears.",
                "no sources answering",
                0,
                "");
        }

        var evidence = rankedCount == 0
            ? $"Nothing is being reported on {bandName} at all."
            : $"There are {rankedCount} spots on {bandName}, but they are beacons, "
              + "contest runs or too old to chase.";

        return new LeadSuggestion(
            false,
            NothingHeadline,
            evidence + " That is a real answer, not a failure — it is the answer that "
            + "saves you an hour of tuning across a dead band. Try another band from "
            + "the row above, or come back at a different time of day.",
            "nothing on this band clears the bar",
            0,
            "");
    }

    private static string Sentence(List<string> reasons)
    {
        var joined = reasons.Count switch
        {
            1 => reasons[0],
            2 => reasons[0] + ", " + reasons[1],
            _ => string.Join(", ", reasons.Take(reasons.Count - 1)) + ", " + reasons[^1],
        };

        var text = char.ToUpperInvariant(joined[0]) + joined[1..];
        return text.EndsWith('.') ? text : text + ".";
    }

    private static string FormatMhz(long hz)
        => (hz / 1_000_000.0).ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
}
