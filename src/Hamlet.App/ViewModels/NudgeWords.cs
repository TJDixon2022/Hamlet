using Hamlet.RadioEngine.Contacts;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **What a marked row says on a deliberate look.**
/// </summary>
/// <remarks>
/// <para>**A DOOR MAY BE MARKED AND NOT NAMED** (Tim, 2026-09-10). An unearned card
/// that is already visible on the achievements screen may be named here; an area that
/// has never been opened may be marked, and this says that something new would open
/// **without saying which area**. §3.1 is *absent, not dimmed*: the CQ list must not
/// be the thing that tells him Eastern Europe exists.</para>
/// <para>**IT SAYS *WORKED*, NEVER *CONFIRMED*** (§4). Hamlet has contacts and no
/// confirmations, and a word it cannot stand behind is worse than a shorter
/// sentence.</para>
/// <para>**NOTHING SHAMES AND NOTHING PROMISES.** It does not say he is behind, it
/// does not count what he is missing, and it does not suggest the contact will
/// succeed - that is the band's business and not the application's.</para>
/// <para>**THE DOOR WORDING IS A PLACEHOLDER.** Wording is the product here (§3.5)
/// and no ruling has been given on this sentence; it is built so it can be read, and
/// the question is in the report for Tim.</para>
/// </remarks>
public static class NudgeWords
{
    /// <summary>What a door says, naming nothing.</summary>
    public const string Door = "new area · would open something you have not seen yet";

    /// <summary>What a door's popup says, naming nothing.</summary>
    /// <remarks>
    /// **NAMING NOTHING IS THE RULE AND THIS SENTENCE IS BUILT AROUND IT** (§3.1, Tim
    /// 2026-09-10). *a set of cards you have not seen yet* is as close as the words get; **a
    /// new area** is the category and not the area. Nothing here, and nothing that reaches
    /// here, knows which continent it is.
    /// </remarks>
    public const string DoorReason =
        "A first contact in a new area · working him opens a set of cards you have not seen yet";

    /// <summary>The line both kinds end with.</summary>
    /// <remarks>
    /// **WHAT WORKING HIM EARNS, AND IT IS TRUE OF BOTH KINDS** (work instruction 327 task
    /// 3). The QSO is the achievement; the card is what comes with it. **It does not promise
    /// the contact will succeed** - that is the band's business (§3.5, and nothing
    /// promises).
    /// </remarks>
    public const string Earns = "A QSO first. The card comes with it.";

    /// <summary>The words for one mark, or "" where there is no mark.</summary>
    /// <param name="kind">What working the station would open.</param>
    /// <param name="entity">The entity, where it may be named.</param>
    /// <returns>One line, or "".</returns>
    public static string For(NudgeKind kind, string? entity)
        => kind switch
        {
            NudgeKind.Door => Door,
            NudgeKind.Visible when !string.IsNullOrWhiteSpace(entity) =>
                entity!.Trim() + " · new country",
            _ => "",
        };

    /// <summary>
    /// **What the popup says when he presses the mark: why, in the hover's voice.**
    /// </summary>
    /// <param name="reason">What the nudge set knows about this station.</param>
    /// <returns>One line, or "" where there is no mark.</returns>
    /// <remarks>
    /// <para>**THE HOVER IS THE HEADLINE AND THIS IS THE PARAGRAPH** (work instruction 327
    /// task 3). <see cref="For"/> stays exactly as it is - one line, on hover - and this is
    /// what a deliberate press gets: the same voice, the reason spelled out, and what
    /// working him would earn.</para>
    /// <para>**A COUNTER NAMES THE ENTITY AND THE COUNT IT MOVES**, which is the whole
    /// content of *why is this one interesting*: `Costa Rica · a new country · you have
    /// worked 0 in North America`. **The count says worked** (§4) - Hamlet has contacts and
    /// no confirmations, and the word *confirmed* appears nowhere in this file.</para>
    /// <para>**NOTHING SHAMES.** *You have worked 3 in South America* is a count of what he
    /// has done, not of what he is missing, and there is no total to be short of.</para>
    /// </remarks>
    public static string Reason(NudgeReason reason)
        => reason.Kind switch
        {
            NudgeKind.Door => DoorReason,
            NudgeKind.Visible when !string.IsNullOrWhiteSpace(reason.Entity) =>
                reason.Entity.Trim() + " · a new country"
                + (string.IsNullOrWhiteSpace(reason.Continent)
                    ? ""
                    : " · you have worked " + reason.WorkedInContinent.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)
                        + " in " + reason.Continent.Trim()),
            _ => "",
        };
}
