using System;

namespace Ft8Sharp.Dsp;

/// <summary>
/// One place in the waterfall where a transmission may begin, and how strongly the Costas sync
/// pattern was found there. <b>A candidate is a place, not a message</b> — nothing here has been
/// demodulated, no bit has been extracted and no CRC has been checked.
/// </summary>
/// <remarks>
/// <para>
/// <b>The five fields are upstream's</b>, read out of <c>ftx_candidate_t</c> in <c>ft8/decode.h</c>
/// by <c>UpstreamSyncSearchInventoryTests</c>: a score, a block offset, a time sub-offset, a bin
/// offset and a frequency sub-offset. Upstream holds the first three as <c>int16_t</c> and the last
/// two as <c>uint8_t</c>; they are widened to <c>int</c> here because narrowing them would buy
/// nothing in a managed struct and would put an overflow between the search and its caller.
/// </para>
/// <para>
/// <b>The score is an integer and that is load-bearing.</b> It is a sum of differences of stored
/// waterfall bytes — whole counts of half a decibel — divided by however many differences were
/// actually taken. Two candidates therefore compare exactly, with no floating-point near-equality
/// anywhere, which is what makes the ranking below reproducible rather than merely repeatable.
/// </para>
/// <para>
/// <b>The ordering is a TOTAL order, and that is a deliberate divergence from upstream.</b>
/// Upstream compares candidates on <c>score</c> and on nothing else, and sorts them with a heapsort,
/// which is not stable — so where two candidates tie, its returned order is whatever the heap's
/// swaps happened to leave. Scores here are small integers over tens of thousands of hypotheses, so
/// ties are the ordinary case rather than the exception. <see cref="CompareTo"/> therefore continues
/// past the score through every remaining field in a stated sequence, and because no two distinct
/// hypotheses share all four position fields, <b>no two distinct candidates ever compare equal.</b>
/// The order is a function of the input and of nothing else. Recorded as divergence 19 in
/// <c>porting-notes.md</c>; the reason is that a ranking the caller cannot reproduce is not a
/// ranking, and step 5 will consume this list in order.
/// </para>
/// </remarks>
/// <param name="Score">
/// The averaged Costas sync score. Higher is stronger. It may be negative, which means the expected
/// sync tones were quieter than their neighbours.
/// </param>
/// <param name="BlockOffset">
/// The block the transmission's first symbol sits in, counted from the start of the analysis.
/// <b>It may be negative</b>, which says the transmission began before the slot was opened.
/// </param>
/// <param name="TimeSubOffset">Which time subdivision of that block.</param>
/// <param name="BinOffset">The waterfall bin the transmission's lowest tone sits in.</param>
/// <param name="FrequencySubOffset">Which frequency subdivision of that bin.</param>
public readonly record struct Ft8Candidate(
    int Score,
    int BlockOffset,
    int TimeSubOffset,
    int BinOffset,
    int FrequencySubOffset) : IComparable<Ft8Candidate>
{
    /// <summary>
    /// Best first: by score descending, and then — where the scores tie, which they constantly do —
    /// by block offset, time sub-offset, bin offset and frequency sub-offset, each ascending.
    /// </summary>
    /// <remarks>
    /// <b>The sequence is stated here and in <c>porting-notes.md</c> so it can be relied on.</b>
    /// After the score it prefers the earlier transmission, and among equally early ones the lower
    /// frequency. Nothing about that sequence is claimed to be better than another; what is claimed
    /// is that it is fixed, that it exhausts every field, and that it therefore leaves no pair of
    /// distinct candidates undecided.
    /// </remarks>
    public int CompareTo(Ft8Candidate other)
    {
        var byScore = other.Score.CompareTo(Score);
        if (byScore != 0)
        {
            return byScore;
        }

        var byBlock = BlockOffset.CompareTo(other.BlockOffset);
        if (byBlock != 0)
        {
            return byBlock;
        }

        var byTimeSub = TimeSubOffset.CompareTo(other.TimeSubOffset);
        if (byTimeSub != 0)
        {
            return byTimeSub;
        }

        var byBin = BinOffset.CompareTo(other.BinOffset);
        return byBin != 0 ? byBin : FrequencySubOffset.CompareTo(other.FrequencySubOffset);
    }

    /// <summary>
    /// The frequency this candidate's lowest tone sits at, by the geometry's own arithmetic.
    /// </summary>
    public double FrequencyHz(Ft8WaterfallGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        return geometry.FrequencyHz(BinOffset, FrequencySubOffset);
    }

    /// <summary>
    /// The time this candidate's first symbol begins, in seconds from the start of the analysis, by
    /// the geometry's own arithmetic. <b>Negative for a transmission that began before the slot.</b>
    /// </summary>
    public double TimeSeconds(Ft8WaterfallGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        return geometry.TimeSeconds(BlockOffset, TimeSubOffset);
    }
}
