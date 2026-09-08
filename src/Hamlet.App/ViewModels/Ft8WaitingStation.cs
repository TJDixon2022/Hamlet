namespace Hamlet.App.ViewModels;

/// <summary>One station calling him that is not the conversation on show.</summary>
/// <remarks>
/// <para>**NOTHING IS HIDDEN** (Tim's ruling, 2026-09-08). The For you panel shows
/// one conversation, and every other station that has called him is a row here with
/// a count and an age, one click from being the conversation instead. A station he
/// cannot see is a station he will not answer.</para>
/// <para>**IT CARRIES NO MESSAGE AND NO REPORT.** A waiting row says who, how many
/// and how long ago, which is what deciding *shall I switch to this one* needs. The
/// messages themselves are a click away and are never summarized here, because a
/// summary of a message is a message the operator did not read.</para>
/// </remarks>
/// <param name="Callsign">Who is calling.</param>
/// <param name="Messages">
/// How many messages that station has sent him, counted as they arrived rather than
/// as they would fold. Two identical reports are two attempts to reach him.
/// </param>
/// <param name="Quiet">
/// How long since it last transmitted, in slots, or "" where no clock offset has
/// been measured and no boundary can be placed (§0.0).
/// </param>
/// <param name="LastSlotUtc">
/// The boundary of its most recent message, which is what the list is ordered by.
/// **Not shown**; it is here so the ordering is a fact about the row rather than a
/// comparison done twice.
/// </param>
public sealed record Ft8WaitingStation(
    string Callsign, int Messages, string Quiet, DateTime LastSlotUtc)
{
    /// <summary>What the row reads, beside the callsign.</summary>
    /// <remarks>
    /// **THE AGE IS THE POINT AND IT LEADS** (§0.7). A station four slots quiet has
    /// had four chances to hear him and taken none, and that is what decides whether
    /// switching to it is worth a transmission. Where there is no clock the row says
    /// how many messages and stops, rather than filling the gap with a plausible age.
    /// </remarks>
    public string Detail
    {
        get
        {
            var many = Messages == 1
                ? "1 message"
                : Messages.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + " messages";

            return Quiet.Length == 0 ? many : many + ", last " + Quiet;
        }
    }
}
