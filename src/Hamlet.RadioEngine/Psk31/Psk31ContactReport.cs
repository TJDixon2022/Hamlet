using Hamlet.RadioEngine.Contacts;

namespace Hamlet.RadioEngine.Psk31;

/// <summary>
/// **The RST one station put on the air in a PSK31 conversation, for the log.**
/// </summary>
/// <remarks>
/// <para>**IT COUNTS; IT DOES NOT INFER** (§12.1, §0.0). Every answer here is a report
/// the parser actually read out of a message that actually passed. `599` is the
/// conventional report for a clean copy and Hamlet's own Report macro sends it, and
/// that is exactly why an unread one is null rather than `599`: a log entry is the one
/// artifact in this project that outlives everything else, and nothing later can tell a
/// filled-in report from a heard one.</para>
/// <para>**THE LAST ONE, NOT THE FIRST** - the same rule <see cref="Ft8ContactLogEntry"/>
/// uses for a decibel report. A repeated exchange is one contact, and the report that
/// stood is the one sent last.</para>
/// <para>**ONLY OFF A CERTAIN MESSAGE** (§R1, §0.0). A parse that was a guess about who
/// was speaking is a guess about whose report it was, and a report under the wrong
/// callsign is worse in a log than no report at all.</para>
/// </remarks>
public static class Psk31ContactReport
{
    /// <summary>The last RST that station certainly sent in this conversation.</summary>
    /// <param name="conversation">Both halves of one contact, oldest first.</param>
    /// <param name="callsign">Whose report is wanted.</param>
    /// <returns>The report, `599`-style, or null where none was read.</returns>
    /// <remarks>
    /// **THE SPEAKER AND NOT THE ADDRESSEE.** A report is a statement about how the
    /// speaker is hearing the other station, so `UR RST 599` in a message *from* him is
    /// the report *he* sent - which is what the operator received.
    /// </remarks>
    public static string? From(
        IReadOnlyList<Psk31Message>? conversation, string? callsign)
    {
        var who = (callsign ?? "").Trim();

        if (conversation is null || who.Length == 0)
        {
            return null;
        }

        for (var at = conversation.Count - 1; at >= 0; at--)
        {
            var exchange = conversation[at].Exchange;

            if (exchange is { IsCertain: true, Rst: { Length: > 0 } report }
                && Ft8MessageSplit.IsSameStation(exchange.Speaker, who))
            {
                return report;
            }
        }

        return null;
    }
}
