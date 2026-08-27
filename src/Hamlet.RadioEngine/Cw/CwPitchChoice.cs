namespace Hamlet.RadioEngine.Cw;

/// <summary>How the pitch being decoded at came to be chosen.</summary>
/// <remarks>
/// <para>**BECAUSE "MEASURED" STOPPED BEING ONE QUESTION** when Tim's ruling of
/// 2026-08-27 let the strongest bin choose the note at acquisition. Before it,
/// a pitch either came from keying the survey admitted or it was a starting
/// point nobody had confirmed, and one flag said which. After it there is a
/// third case — chosen because that bin was the loudest thing in the band — and
/// folding that into "measured" would claim the survey found keying it never
/// found (§0.0).</para>
/// <para>**IT IS NOT A RANKING AND THE ORDER MEANS NOTHING.** Each value names
/// a different provenance, and the sheet prints the name rather than comparing
/// them.</para>
/// </remarks>
public enum CwPitchChoice
{
    /// <summary>Nothing has chosen. The bank centre is a starting point.</summary>
    /// <remarks>
    /// **THE HONEST DEFAULT.** A tracker that has never confirmed anything is
    /// pointed somewhere, and where it is pointed is not a finding about the
    /// band.
    /// </remarks>
    NotChosen,

    /// <summary>Keying the survey admitted, confirmed across two surveys.</summary>
    /// <remarks>
    /// The strongest provenance there is, and the only one that sets
    /// <see cref="CwDecodeReport.PitchWasMeasured"/>.
    /// </remarks>
    Keying,

    /// <summary>The loudest bin in the band, with no keying confirmed.</summary>
    /// <remarks>
    /// <para>**TIM'S RULING OF 2026-08-27.** Eight statistics were measured
    /// against choosing a pitch by how it is keyed and all eight were wrong on
    /// the four captures he can hear, while the strongest bin was right on all
    /// four. So the strongest bin may choose, and keying structure is demoted to
    /// a check on the winner.</para>
    /// <para>**IT IS A CHOICE AND NOT A MEASUREMENT**, and the difference is the
    /// whole reason this enum exists. A loud bin is where to point the filter; it
    /// is not evidence that anybody is sending.</para>
    /// </remarks>
    StrongestBin,

    /// <summary>The operator said he could hear a station there.</summary>
    /// <remarks>
    /// **NO SHEET MAY EVER IMPLY HAMLET FOUND WHAT A HUMAN FOUND** (§0.0). Unit
    /// 1.11.21 built this and it beats every automatic scheme on the four
    /// captures that matter.
    /// </remarks>
    OperatorAssertion,
}
