namespace Hamlet.RadioEngine.Psk31;

/// <summary>
/// **Where a PSK31 call may go out without landing on somebody.**
/// </summary>
/// <remarks>
/// <para>**§R6: A CQ GOES OUT ON A CLEAR SPOT HAMLET FINDS, NOT A FIXED OFFSET.** A fixed
/// offset is what every beginner's software does and it is why 14.070 has a pile-up at
/// 1000 Hz: the operator cannot see the passband, so the application has to look.</para>
/// <para>**THE RULE IS <see cref="Rule"/> AND IT IS STATED IN ONE PLACE** (work instruction
/// 323 task 2). The record carries that sentence beside the offset it chose, so a reader of
/// the file is reading the same rule the code ran.</para>
/// <para>**IT CHOOSES AND IT DOES NOT SEND** (§0.1, §0.2). Numbers in, one number or
/// nothing out. It knows no radio, no tab, no telemetry and no operator, it keys nothing,
/// and removing every caller would change no behaviour.</para>
/// <para>**NOTHING IS NOT A FAILURE TO ANSWER; IT IS THE ANSWER** (§0.0). On a band where
/// every gap is under <see cref="ClearHz"/>, there is no spot, and saying so is what stops
/// Hamlet calling on top of a conversation. The caller refuses in plain words; it does not
/// fall back to a default, and it does not shade the rule to find one.</para>
/// </remarks>
public static class Psk31ClearSpot
{
    /// <summary>**How far a call must sit from anything being heard: 150 Hz.**</summary>
    /// <remarks>
    /// <para>**WHY A NUMBER AT ALL.** A PSK31 signal is about 31 Hz wide
    /// (<see cref="Psk31Modulator"/>'s own measurement), so two stations 150 Hz apart are
    /// not touching by a wide margin. **The margin is for the receiver on the other end**,
    /// not for the arithmetic: everybody's filter, AGC and AFC are different, a strong
    /// caller 60 Hz away can capture a weak one's decoder, and the operator running this
    /// has no way to hear that he has done it.</para>
    /// <para>**IT IS THE INSTRUCTION'S NUMBER, CARRIED RATHER THAN DERIVED.** Work
    /// instruction 323 task 2 rules 150 Hz. Nothing here measured it and nothing here
    /// claims to have.</para>
    /// </remarks>
    public const double ClearHz = 150;

    /// <summary>**How good a candidate has to look before it is somebody: 0.4.**</summary>
    /// <remarks>
    /// **THE SAME NUMBER THE SEARCH KEEPS A CARRIER ON** - see
    /// <see cref="Psk31CarrierSearch.CoherenceToStay"/>. A candidate at or under this is
    /// noise the search itself would let go of, and giving way to noise would leave a busy
    /// band with no clear spot anywhere. **Read from the search rather than typed**, so the
    /// two cannot drift apart.
    /// </remarks>
    public const double Occupied = Psk31CarrierSearch.CoherenceToStay;

    /// <summary>**The lowest a call may be placed: 400 Hz.**</summary>
    /// <remarks>
    /// <para>**A CALL IS BOUNDED BY WHAT THE RADIO WILL TRANSMIT, NOT BY WHAT HAMLET CAN
    /// HEAR.** <see cref="Psk31CarrierSearch"/> searches to half the sample rate, which at
    /// 48 000 Hz is nearly 24 kHz; **the widest gap in that span is always miles above the
    /// SSB filter**, and a carrier placed there would leave the sound card and never leave
    /// the radio. The receive search is right to look everywhere - a signal Hamlet can hear
    /// is worth reading - and the transmit choice is a different question with a different
    /// answer.</para>
    /// <para>**400 TO 2200 IS THIS UNIT'S CHOICE AND NOT A MEASUREMENT.** An SSB transmit
    /// filter is nominally about 300 Hz to 2700 Hz and rolls off at both ends rather than
    /// stopping; these two sit inside that with room, which is what a 31 Hz signal needs.
    /// **`SHACK_FACTS.md` records no filter fact for this radio**, so nothing here claims
    /// to have read one (§0.0). If the operator ever rules a width, it replaces these.</para>
    /// </remarks>
    public const double LowestCallHz = 400;

    /// <summary>**The highest a call may be placed: 2200 Hz.** See <see cref="LowestCallHz"/>.</summary>
    public const double HighestCallHz = 2200;

    /// <summary>The rule, in one sentence, for the record and the panel.</summary>
    public const string Rule =
        "at least 150 Hz from every carrier being read and from every candidate over "
        + "quality 0.4, in the middle of the widest such gap between 400 and 2200 Hz";

    /// <summary>Where this call should go out, or null where there is nowhere.</summary>
    /// <param name="heldHz">The offsets of the carriers being read right now.</param>
    /// <param name="candidates">What the last search pass measured.</param>
    /// <param name="lowHz">
    /// The bottom of the passband being listened to. **Raised to
    /// <see cref="LowestCallHz"/> where it is lower**, because a call is bounded by what
    /// the radio will transmit and not by what Hamlet can hear.
    /// </param>
    /// <param name="highHz">The top of it, lowered to <see cref="HighestCallHz"/>.</param>
    /// <returns>The offset, or null where no gap is wide enough.</returns>
    /// <remarks>
    /// <para>**THE MIDDLE OF THE WIDEST GAP.** Any spot inside a wide enough gap satisfies
    /// the rule; the middle of the widest one is the spot that stays clear longest as other
    /// stations arrive, and it is deterministic, which is what makes the choice
    /// reproducible from the record.</para>
    /// <para>**THE PASSBAND'S OWN EDGES ARE NOT GUARDED BY <see cref="ClearHz"/>.** The
    /// margin exists so as not to land on a station; there is no station at the edge of the
    /// passband, and a 150 Hz exclusion at each end would throw away 300 Hz of a band that
    /// is under 2 kHz wide.</para>
    /// </remarks>
    public static double? Choose(
        IEnumerable<double> heldHz,
        IEnumerable<Psk31Candidate> candidates,
        double lowHz,
        double highHz)
    {
        ArgumentNullException.ThrowIfNull(heldHz);
        ArgumentNullException.ThrowIfNull(candidates);

        var low = Math.Max(lowHz, LowestCallHz);
        var high = Math.Min(highHz, HighestCallHz);

        if (!(high > low))
        {
            return null;
        }

        lowHz = low;
        highHz = high;

        var busy = heldHz
            .Concat(candidates.Where(c => c.Coherence > Occupied).Select(c => c.OffsetHz))
            .Where(hz => hz > lowHz - ClearHz && hz < highHz + ClearHz)
            .OrderBy(hz => hz)
            .ToList();

        var best = default(double?);
        var widest = 0.0;
        var from = lowHz;

        // **EVERY GAP, INCLUDING THE TWO AT THE ENDS.** The walk takes each busy offset in
        // turn as the end of the gap before it, and the top of the passband closes the last
        // one; a band with nothing on it is one gap, the whole passband.
        foreach (var hz in busy.Append(highHz + ClearHz))
        {
            var to = hz - ClearHz;

            if (to - from > widest)
            {
                widest = to - from;
                best = from + ((to - from) / 2);
            }

            from = Math.Max(from, hz + ClearHz);
        }

        // **A GAP OF NO WIDTH IS NOT A GAP.** `widest` is strictly positive only where some
        // stretch of the passband is more than `ClearHz` from everything busy.
        return widest > 0 ? Math.Round(best!.Value, 1) : null;
    }
}
