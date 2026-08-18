using Hamlet.RadioEngine.Bands;

namespace Hamlet.RadioEngine.Cw;

/// <summary>What just happened, for the card at the end of a contact.</summary>
/// <param name="TheirCall">Who it was with, or "".</param>
/// <param name="Where">Where they said they were, or "".</param>
/// <param name="BandName">Which band, e.g. "40 m".</param>
/// <param name="FrequencyHz">Where on it.</param>
/// <param name="ReportSent">The report the operator sent, or "".</param>
/// <param name="ReportReceived">The report they got back, or "".</param>
/// <param name="WordsPerMinute">The speed it ran at, or null.</param>
public sealed record ContactSummary(
    string TheirCall,
    string Where,
    string BandName,
    long FrequencyHz,
    string ReportSent,
    string ReportReceived,
    int? WordsPerMinute);

/// <summary>The card that appears when a contact ends (HM-DEC-059).</summary>
/// <param name="Headline">The one line, e.g. "You just worked W1ABC on 40 m".</param>
/// <param name="Detail">What was exchanged, in a sentence.</param>
/// <param name="Encouragement">
/// What a friend would say about it, or "" when there is nothing to add.
/// </param>
public sealed record ClosingCard(string Headline, string Detail, string Encouragement);

/// <summary>
/// What to say when a contact ends (HM-DEC-059).
/// </summary>
/// <remarks>
/// <para>FOR SOMEBODY'S FIRST CONTACT THIS IS THE THING THEY WILL LOOK AT
/// AFTERWARD. It reads like a friend telling them it went fine, not like a log
/// entry, because the person this is for has spent six years not doing this and
/// the ninety seconds that just happened are a bigger thing to them than the
/// record of it.</para>
/// <para>IT IS NOT A LOGBOOK, and it deliberately does not try to be. FG-004 is
/// where logging lives, with its own shape and its own confirmations. This is
/// one card that says what happened and then goes away.</para>
/// <para>Nothing is claimed that was not observed. A report nobody sent is not
/// mentioned, a speed nobody measured is not stated, and a callsign Hamlet did
/// not hear is left out rather than filled in (§0.0).</para>
/// </remarks>
public static class ContactClosing
{
    /// <summary>Build the card.</summary>
    /// <param name="summary">What happened.</param>
    /// <returns>The card.</returns>
    public static ClosingCard Build(ContactSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        var who = summary.TheirCall.Trim().Length > 0
            ? summary.TheirCall.Trim().ToUpperInvariant()
            : "";

        var headline = who.Length > 0
            ? $"You just worked {who} on {summary.BandName}"
            : $"That was a contact on {summary.BandName}";

        return new ClosingCard(headline, Detail(summary, who), Encouragement(summary));
    }

    private static string Detail(ContactSummary s, string who)
    {
        var parts = new List<string>
        {
            $"{Megahertz(s.FrequencyHz)} MHz, in Morse",
        };

        if (s.Where.Trim().Length > 0)
        {
            parts.Add($"they were in {s.Where.Trim()}");
        }

        if (s.ReportSent.Trim().Length > 0 && s.ReportReceived.Trim().Length > 0)
        {
            parts.Add($"you gave them {s.ReportSent.Trim()} and got {s.ReportReceived.Trim()}");
        }
        else if (s.ReportSent.Trim().Length > 0)
        {
            parts.Add($"you gave them {s.ReportSent.Trim()}");
        }

        if (s.WordsPerMinute is { } wpm)
        {
            parts.Add($"it ran at about {wpm} words a minute");
        }

        var body = string.Join(", ", parts) + ".";

        return who.Length > 0
            ? $"{body} That is a complete contact, exactly as it is supposed to go."
            : $"{body} That is a complete contact.";
    }

    /// <summary>
    /// The sentence a friend would add, chosen from what actually happened.
    /// </summary>
    /// <remarks>
    /// Warmth never buys a claim (§0.7). Every line here is about the thing that
    /// occurred rather than about how well anybody did, because "well done" from
    /// a program is worth nothing and "the other operator was in Boston" is
    /// worth something.
    /// </remarks>
    private static string Encouragement(ContactSummary s)
    {
        if (s.WordsPerMinute is { } fast and >= 20)
        {
            return "That was quick sending and you stayed with it, which is the "
                 + "part that only comes from doing it.";
        }

        if (s.Where.Trim().Length > 0)
        {
            return "Somebody at the other end of that heard you, wrote down your "
                 + "callsign and sent it back. Everything after this is the same "
                 + "thing again.";
        }

        return "Ninety seconds is a complete and normal contact, so if it felt "
             + "short, that is because that is what one is.";
    }

    private static string Megahertz(long hz)
        => (hz / 1_000_000.0).ToString(
            "0.000", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>The band a frequency is on, for the summary.</summary>
    /// <param name="hz">The frequency.</param>
    /// <returns>Its band name, or "the air" when it is on none.</returns>
    public static string BandNameFor(long hz) => HfBands.BandFor(hz)?.Name ?? "the air";
}
