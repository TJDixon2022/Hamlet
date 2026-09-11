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
}
