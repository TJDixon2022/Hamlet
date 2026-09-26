namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// What the decoder can say about the pitch it reports (HM-REQ-093).
/// </summary>
/// <remarks>
/// <para>**THREE ANSWERS BECAUSE "MEASURED" WAS ONE ANSWER TO TWO QUESTIONS**
/// (work instruction 450). The flag it refines said a number is held, which is
/// true of a pitch the survey confirmed half a second ago and of one it last
/// confirmed a minute ago on a station that has since stopped. The requirement's
/// own rationale: not a survey candidate, not a stale hold.</para>
/// <para>**IT DESCRIBES THE PITCH AND NOTHING READS IT.** No character's class, no
/// emission gate and no speed path consults it, and it is not "acquired": that
/// question is the owner's.</para>
/// </remarks>
public enum CwPitchProof
{
    /// <summary>No pitch is held.</summary>
    /// <remarks>
    /// The tracker points at the operator's configured pitch or, from cold, at the
    /// loudest bin, and neither is a finding about a station (HM-REQ-005).
    /// </remarks>
    None,

    /// <summary>A pitch is held that the latest survey does not name.</summary>
    /// <remarks>
    /// Keying set it once and the evidence is remembered rather than current: a
    /// hold after the station stopped, while the survey admits nothing it may act
    /// on, or while a move elsewhere waits for a character to end.
    /// </remarks>
    Hypothesis,

    /// <summary>The latest survey's confirmed keyed verdict names this pitch.</summary>
    /// <remarks>
    /// HM-DEC-095's rule: keying seen on two surveys running, and for a move
    /// standing over the band and not far below the station being read.
    /// </remarks>
    Proved,
}
