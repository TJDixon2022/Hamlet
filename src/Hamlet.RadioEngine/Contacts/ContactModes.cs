using System;
using System.Collections.Generic;
using System.Linq;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>
/// One mode, as ADIF spells it and as a log record could carry it.
/// </summary>
/// <param name="Name">What Hamlet calls it, which is what the operator asked for.</param>
/// <param name="AdifModes">
/// Every value of ADIF's `MODE` that counts as this mode. **Usually one, and
/// Voice is three**, because *Voice* is Hamlet's own word and the specification
/// has `SSB`, `AM` and `FM` underneath it.
/// </param>
/// <param name="AdifSubmode">
/// The value of `SUBMODE` a record would also need, or null where the mode stands
/// on its own. **A mode with one of these cannot be recognized today**, because
/// <see cref="AdifContact"/> carries no submode at all.
/// </param>
/// <param name="IsContactMode">
/// **False where nobody works anybody.** WSPR is a beacon, so a QSO record for it
/// would name a station the operator never worked.
/// </param>
public sealed record ContactMode(
    string Name,
    IReadOnlyList<string> AdifModes,
    string? AdifSubmode,
    bool IsContactMode)
{
    /// <summary>True where a record with these fields is this mode.</summary>
    /// <param name="mode">The record's `MODE`, or null.</param>
    /// <param name="submode">The record's `SUBMODE`, or null.</param>
    /// <returns>True where the record says this mode and no other.</returns>
    /// <remarks>
    /// <para>**A SUBMODE THIS MODE NEEDS AND THE RECORD DOES NOT CARRY IS A
    /// MISS**, deliberately and permanently. `MODE=PSK` on its own is *some kind of
    /// phase-shift keying*, and reading it as PSK31 would be exactly the guess §0.0
    /// forbids: the operator would be shown a first he had not made.</para>
    /// <para>**AND THAT IS WHY FT4 AND PSK31 CANNOT LIGHT TODAY** rather than
    /// merely having not lit yet. It is a fact about the record's fields, not about
    /// his operating, and the screen says which.</para>
    /// </remarks>
    public bool Matches(string? mode, string? submode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return false;
        }

        if (!AdifModes.Any(m => string.Equals(
                m, mode.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return AdifSubmode is null
            || string.Equals(
                AdifSubmode,
                submode?.Trim(),
                StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The one value of `MODE` a record carries for this mode, or **null where
    /// this is a family and a record could not say which**.
    /// </summary>
    /// <remarks>
    /// <para>**VOICE IS THE ONE THAT COMES BACK NULL, AND THAT IS §0.0 RATHER
    /// THAN A GAP.** *Voice* is Hamlet's own word for `SSB`, `AM` and `FM`, and
    /// writing the first of the three into a log record would name a mode the
    /// operator may not have worked. There is nothing later that could tell such
    /// a record from a true one, so the mode is left out rather than guessed.</para>
    /// <para>**IT IS THE WRITE HALF OF <see cref="Matches"/>.** `Matches` accepts
    /// any of <see cref="AdifModes"/>, because a record made elsewhere may say any
    /// of them; this says what Hamlet may write, which is only ever the
    /// unambiguous one.</para>
    /// </remarks>
    public string? AdifMode => AdifModes.Count == 1 ? AdifModes[0] : null;

    /// <summary>How a record would have to spell this, for a reader.</summary>
    /// <returns>The tag values, in the form a record writes them.</returns>
    /// <remarks>
    /// **THE OPERATOR CHECKS HIS FILE AGAINST ANOTHER LOGGER WITH THIS**, the same
    /// reason the log dialog puts the ADIF tag name beside every value.
    /// </remarks>
    public string AdifSpelling
    {
        get
        {
            var modes = AdifModes.Count == 1
                ? "MODE=" + AdifModes[0]
                : "MODE=" + string.Join(", ", AdifModes.Take(AdifModes.Count - 1))
                  + " or " + AdifModes[^1];

            return AdifSubmode is null
                ? modes
                : modes + ", SUBMODE=" + AdifSubmode;
        }
    }
}

/// <summary>
/// **The six modes the operator keeps a first in, spelled as ADIF spells them.**
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**: the first contact in each mode is an
/// achievement, and the six are CW, FT8, FT4, PSK31, WSPR and Voice.</para>
/// <para>**THE SPELLINGS ARE READ AND CITED, NEVER RECALLED** (work instruction
/// 287 task 1, and unit 274's rule before it). Read from the **ADIF
/// Specification, version 3.1.4, released 6 December 2022**, at
/// `https://www.adif.org/314/ADIF_314.htm`, retrieved 2026-09-08. That is the same
/// edition and the same page <see cref="AdifLog"/> already cites, so this file adds
/// a reading and not a second source.</para>
/// <para>**THREE OF THE SIX ARE NOT WHAT THEY LOOK LIKE, AND THAT IS THE WHOLE
/// REASON THIS TABLE EXISTS.** `FT4` is a **submode of `MFSK`**, `PSK31` is a
/// **submode of `PSK`**, and **`Voice` is not in the specification at all** — it is
/// Hamlet's own family name, and the modes under it are `SSB`, `AM` and `FM`. A
/// first that matched on the obvious string would never fire, which is worse than
/// one that is missing, because a row that looks earnable and is not is a promise
/// the application cannot keep.</para>
/// <para>**THIS FILE HOLDS WHAT ADIF SAYS AND NOTHING ABOUT WHAT HAMLET CAN DO**
/// (§0.1). Whether there is a way to send a mode, or a way to write it down, is a
/// fact about the application and belongs where the screen is built.</para>
/// </remarks>
public static class ContactModes
{
    /// <summary>The specification these spellings were read from.</summary>
    public const string Cite =
        "ADIF Specification 3.1.4, released 2022-12-06, "
        + "https://www.adif.org/314/ADIF_314.htm, retrieved 2026-09-08";

    /// <summary>The six, in the order the operator listed them.</summary>
    public static IReadOnlyList<ContactMode> Six { get; } = new[]
    {
        new ContactMode("CW", new[] { "CW" }, null, true),
        new ContactMode("FT8", new[] { "FT8" }, null, true),

        // **A SUBMODE OF `MFSK`, WHICH IS WHY IT CANNOT BE RECOGNIZED YET.**
        new ContactMode("FT4", new[] { "MFSK" }, "FT4", true),

        // **A SUBMODE OF `PSK`.** `MODE=PSK31` is not valid ADIF.
        new ContactMode("PSK31", new[] { "PSK" }, "PSK31", true),

        // **A REAL MODE AND NOT A CONTACT.** A record could say `MODE=WSPR` and any
        // logger would read it; nobody works anybody on a beacon, so the record
        // would name a station he never worked.
        new ContactMode("WSPR", new[] { "WSPR" }, null, false),

        // **HAMLET'S OWN WORD FOR A FAMILY.** `ModeFamily.Phone` in the tree, and
        // three modes in the specification.
        new ContactMode("Voice", new[] { "SSB", "AM", "FM" }, null, true),
    };

    /// <summary>The mode of that name, or null.</summary>
    /// <param name="name">Hamlet's own name for it, e.g. "FT8".</param>
    /// <returns>The entry, or null where the name is not one of the six.</returns>
    public static ContactMode? Named(string name)
        => Six.FirstOrDefault(
            m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
}
