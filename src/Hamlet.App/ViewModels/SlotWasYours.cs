using System.Globalization;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **What the readiness line says about a slot Hamlet did not listen to.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"The message we put up about slots after a transmit is
/// wrong. We transmitted that slot so nothing could be heard."* The line said *one slot
/// decoded, and nothing on the band looked like FT8 at all* about the slot Hamlet was
/// transmitting in.</para>
/// <para>**THAT IS §0.0 BROKEN, NOT A WORDING PROBLEM.** Hamlet suspends decoding while
/// the radio is transmitting (HM-DEC-147), so the slot produced nothing **by design**. A
/// line that reads that silence as a measurement of the band is a claim drawn from a
/// measurement nobody took, and the operator acting on it would go looking for a fault in
/// his antenna.</para>
/// <para>**ONE SENTENCE, IN ONE PLACE, IN TWO FORMS** (work instruction 331 task 5). The
/// slotted form already existed for the census line and is moved here so the readiness
/// line and the census line cannot come to say it differently; the unslotted form is
/// PSK31's, where a send is a stretch of seconds rather than a slot and the honest sentence
/// has to say how long.</para>
/// <para>Pure: a stamp and a duration in, a string out. No clock and no state (§5).</para>
/// </remarks>
public static class SlotWasYours
{
    /// <summary>The clause both forms end with.</summary>
    /// <remarks>
    /// **IT SAYS WHY THERE IS NOTHING RATHER THAN APOLOGIZING FOR IT.** *Hamlet was
    /// transmitting and did not listen* is a fact about Hamlet, which is the only kind of
    /// claim it can make about a slot it did not hear.
    /// </remarks>
    public const string DidNotListen = "Hamlet was transmitting and did not listen";

    /// <summary>**The slotted form** - FT8 and FT4, where a send fills one slot.</summary>
    /// <param name="slotStartUtc">The boundary the send went out on.</param>
    /// <returns>`14:37:30 UTC was yours - Hamlet was transmitting and did not listen`.</returns>
    public static string Slot(DateTime slotStartUtc)
        => Stamp(slotStartUtc) + " UTC was yours - " + DidNotListen;

    /// <summary>
    /// **The unslotted form** - PSK31, where a send is a stretch of seconds.
    /// </summary>
    /// <param name="fromUtc">When the keying started, measured.</param>
    /// <param name="seconds">How long the audio ran, as the run measured it.</param>
    /// <returns>
    /// `Hamlet was transmitting from 14:37:30 for 11 s and did not listen`.
    /// </returns>
    /// <remarks>
    /// **PSK31 HAS NO SLOT TO NAME, SO THE SENTENCE NAMES THE STRETCH** (§R1 - PSK31 is
    /// unslotted, and a slot boundary invented for it would be a claim about a clock this
    /// mode does not keep). The seconds are the run's own figure and are rounded to whole
    /// seconds for reading; a fractional tail on a duration says a precision the sound card
    /// does not deliver.
    /// </remarks>
    public static string Unslotted(DateTime fromUtc, double seconds)
        => "Hamlet was transmitting from " + Stamp(fromUtc) + " for "
            + Math.Max(0, (int)Math.Round(seconds)).ToString(CultureInfo.InvariantCulture)
            + " s and did not listen";

    /// <summary>
    /// **The stamp rule FT4 needs: tenths only where the boundary has them.**
    /// </summary>
    /// <remarks>
    /// **FT4's BOUNDARIES ARE 7.5 SECONDS APART**, so half of them fall on a half second
    /// and a whole-second stamp would name the wrong one - two different slots would print
    /// the same time and neither could be matched against a capture. **Every FT8 stamp is
    /// unchanged**, because every FT8 boundary is on a whole second and takes the first
    /// branch. The rule is `MainWindowViewModel.SlotStamp`'s and is here so both readers of
    /// it read one copy.
    /// </remarks>
    private static string Stamp(DateTime at)
    {
        var utc = at.ToUniversalTime();

        return utc.Ticks % TimeSpan.TicksPerSecond == 0
            ? utc.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
            : utc.ToString("HH:mm:ss.f", CultureInfo.InvariantCulture);
    }
}
