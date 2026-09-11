using Hamlet.RadioEngine.Psk31;

namespace Hamlet.App.Telemetry;

/// <summary>
/// **Which of §R2's four macros a send is, as the record spells it.**
/// </summary>
/// <remarks>
/// <para>**STABLE TOKENS, NEVER DISPLAY STRINGS** (§8.1). `psk31_send_composed` and
/// `psk31_send_refused` carry one of these, so a comparison across two evenings is a
/// comparison of the same words. They are lower case because every other token in the
/// `psk31` category is.</para>
/// <para>**THE KIND IS WHAT THE RECORD GETS INSTEAD OF THE TEXT** (§2.1, HM-DEC-018). A
/// macro carries the operator's callsign twice over and, in the report, his name and where
/// he lives; the file says which of the four it was and how long it was, and nothing
/// else.</para>
/// <para>**ONE TYPE SAYS WHICH MACRO, AND IT IS THE ENGINE'S** (work instruction 323 task
/// 1c). <see cref="Psk31Macro"/> is what <see cref="Psk31Offer"/> answers and what the
/// conversation card holds, so the macro a card offers and the macro the record names are
/// the same value rather than two spellings that can drift. This turns it into the token,
/// and does nothing else.</para>
/// <para>**THE FILE IS NAMED FOR THE ENGINE'S ENUM AND DECLARES A DIFFERENT TYPE**, which
/// is a wart: the session that wrote it could not delete or rename a file in this tree, so
/// the name it first created is the name it is stuck with. Reported rather than left for
/// somebody to find.</para>
/// </remarks>
public static class Psk31MacroToken
{
    /// <summary>The token for a macro, as the `psk31` category spells it.</summary>
    /// <param name="macro">Which macro.</param>
    /// <returns>`cq`, `answer`, `report`, `confirm`, or `none`.</returns>
    public static string For(Psk31Macro macro) => macro switch
    {
        Psk31Macro.Cq => "cq",
        Psk31Macro.Answer => "answer",
        Psk31Macro.Report => "report",
        Psk31Macro.Confirm => "confirm",
        _ => "none",
    };
}
