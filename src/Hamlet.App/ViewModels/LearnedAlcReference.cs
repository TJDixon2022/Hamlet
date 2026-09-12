using System;
using System.Globalization;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **The highest ALC reading Hamlet has seen during a send it has no reason to
/// doubt, with when it was taken and what mode it came from.**
/// </summary>
/// <param name="Reading">The meter reading, on the manual's 0 to 120 scale.</param>
/// <param name="TakenUtc">When the radio's poll took it.</param>
/// <param name="Mode">The mode that was sending: `FT8` or `FT4`.</param>
/// <remarks>
/// <para>**§R15, TIM 2026-09-11, ON UNIT 324's ITEM 1.** The manual gives the ALC
/// meter's scale and not where its zone ends on that scale, so unit 324 reported
/// the reading and judged nothing, and unit 323 before it invented 128 - a figure
/// it got by halving the transmit power *setting*'s range, which is a different
/// meter. **Neither an invented number nor a question put to the operator is
/// wanted.** FT8 already transmits cleanly through this radio, so what a good
/// send reads on this radio is a fact Hamlet can measure instead of a number it
/// has to be told.</para>
/// <para>**IT IS A MEASUREMENT AND IT CARRIES ITS AGE** (HM-DEC-111). A reference
/// learned an hour ago, at a drive setting since changed, is a weaker thing than
/// one learned a minute ago, and the operator is shown which he has.</para>
/// <para>**UNTIL THERE IS ONE, THERE IS NO VERDICT** (§0.0). This type being null
/// is the whole of *Hamlet has not learned this yet*, and nothing anywhere
/// substitutes a default for it.</para>
/// </remarks>
public sealed record LearnedAlcReference(double Reading, DateTime TakenUtc, string Mode)
{
    /// <summary>Where the number came from, in the operator's words.</summary>
    /// <remarks>
    /// **IT NAMES THE MODE AND THE CLOCK TIME** (§0.0). A number on a panel with
    /// no provenance is a number the reader has to trust; this one says what
    /// produced it and when, so he can tell a reference taken before he moved the
    /// drive from one taken after.
    /// </remarks>
    public string Provenance
        => "learned from " + Mode + " send at "
           + TakenUtc.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + " UTC";

    /// <summary>The reading and where it came from, as one line.</summary>
    /// <param name="nowUtc">The moment the line is being read at.</param>
    /// <returns>The sentence.</returns>
    /// <remarks>
    /// **THE AGE IS PART OF THE READING** (HM-DEC-111). It is in whole seconds or
    /// whole minutes, because the poll's own cadence does not support finer, and a
    /// precision the instrument does not carry is a claim it cannot support.
    /// </remarks>
    public string Line(DateTime nowUtc)
        => "Hamlet has a reference for your radio's level control: "
           + Reading.ToString("0", CultureInfo.InvariantCulture)
           + " out of 120, " + Provenance + ", " + Ago(nowUtc) + ".";

    /// <summary>How long ago the reference was taken, in words.</summary>
    private string Ago(DateTime nowUtc)
    {
        var seconds = (int)Math.Round((nowUtc - TakenUtc).TotalSeconds);

        if (seconds < 0)
        {
            // **A REFERENCE FROM THE FUTURE IS A CLOCK FAULT AND SAYS SO** (§0.0).
            // It is not silently clamped to *just now*, because *just now* would be
            // a false statement about a real measurement.
            return "taken at a moment later than this one, which is a clock fault";
        }

        if (seconds < 60)
        {
            return seconds.ToString(CultureInfo.InvariantCulture) + " s ago";
        }

        var minutes = seconds / 60;

        return minutes.ToString(CultureInfo.InvariantCulture)
               + (minutes == 1 ? " minute ago" : " minutes ago");
    }
}
