namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// What the decoder can say about the speed it reports (HM-REQ-034).
/// </summary>
/// <remarks>
/// <para>**THREE ANSWERS BECAUSE A NULLABLE NUMBER WAS TWO** (work instruction
/// 451). A number named from a dit measured on keying that is still arriving and
/// a number held in a window whose keying stopped ten seconds ago printed the
/// same. The requirement's own rationale: a speed the decoder has not earned is
/// not a number.</para>
/// <para>**IT DESCRIBES THE SPEED AND NOTHING READS IT.** No character's class, no
/// emission gate and no pitch path consults it, and it is not "acquired": that
/// question is the owner's.</para>
/// </remarks>
public enum CwSpeedProof
{
    /// <summary>Nothing has been read.</summary>
    /// <remarks>
    /// The window has not refilled, or the gate refused the whole of it, so the
    /// grid's winning speed describes no reading of anybody.
    /// </remarks>
    None,

    /// <summary>The window holds a reading and its speed is not proved.</summary>
    /// <remarks>
    /// The clock is being re-acquired after a follow, the unit was won on the
    /// grid rather than measured, or no keying has been found at the pitch being
    /// read for six surveys: a hold past its keying.
    /// </remarks>
    Hypothesis,

    /// <summary>A speed is named from a measured dit on keying still at the pitch.</summary>
    /// <remarks>
    /// <see cref="CwDecoder.WordsPerMinute"/>'s guard passes, the window's unit was
    /// measured from the keying, and the tracker found keying within half the
    /// mixdown filter of the pitch being read inside its own recent span.
    /// </remarks>
    Proved,
}
