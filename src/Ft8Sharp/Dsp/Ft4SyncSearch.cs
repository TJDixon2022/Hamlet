using System;
using System.Collections.Generic;
using Ft8Sharp.Encode;

namespace Ft8Sharp.Dsp;

/// <summary>
/// Finds where FT4 transmissions are. Given a slot of audio — or the waterfall built from one — and
/// <b>nothing else</b>, it correlates FT4's four <em>different</em> four-tone Costas groups against
/// every position the geometry admits and returns the strongest, ranked.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft4_sync_score</c> and <c>ftx_find_candidates</c> in <c>ft8/decode.c</c></b>,
/// which is a different function from <c>ft8_sync_score</c> rather than the same one with different
/// constants. The two structural differences are worth naming because a port that treated them as
/// constants would compile, run, and find nothing:
/// </para>
/// <list type="number">
/// <item><description>
/// <b>Four different patterns, one per group.</b> FT8 scores one seven-symbol Costas array three
/// times; FT4 scores row <c>m</c> of a four-by-four table at group <c>m</c>. A port that repeated
/// row 0 would score a quarter of the sync energy correctly and three quarters against the wrong
/// tones.
/// </description></item>
/// <item><description>
/// <b>The groups begin at symbol 1, not symbol 0.</b> Upstream writes
/// <c>block = 1 + (FT4_SYNC_OFFSET * m) + k</c>, because symbol 0 is a ramp. A port that dropped the
/// <c>1 +</c> would look one symbol early at every one of the sixteen sync positions.
/// </description></item>
/// </list>
/// <para>
/// <b>Nothing tells it where to look, and there is no parameter through which anything could.</b>
/// The same prohibition <see cref="Ft8SyncSearch"/> works under: a search with a hint cannot be shown
/// not to have used it.
/// </para>
/// <para>
/// <b>THE SWEEP IS WIDER THAN THE DEMO APPLICATION'S, AND THAT IS THE ONE DEPARTURE IN THIS FILE.</b>
/// See <see cref="DefaultLastBlockOffset"/>. It is not a protocol constant and it is the difference
/// between an FT4 decoder that works and one that reads zero out of a correct signal.
/// </para>
/// <para>
/// <b>The same two ranking divergences as FT8's search</b>, and neither is about the score: every
/// hypothesis is scored, the survivors are sorted on <see cref="Ft8Candidate.CompareTo"/>'s total
/// order rather than heapsorted on the score alone, so no two distinct candidates ever compare
/// equal and the list is a function of the input and of nothing else.
/// </para>
/// <para><b>Thread-safe.</b> It holds no mutable state.</para>
/// </remarks>
public sealed class Ft4SyncSearch
{
    /// <summary>Symbols in one Costas sync group. <c>FT4_LENGTH_SYNC</c>.</summary>
    public const int SyncGroupLength = Ft4SymbolEncoder.SyncGroupLength;

    /// <summary>Sync groups in one transmission. <c>FT4_NUM_SYNC</c>.</summary>
    public const int SyncGroupCount = Ft4SymbolEncoder.SyncGroupCount;

    /// <summary>Symbols from the start of one sync group to the next. <c>FT4_SYNC_OFFSET</c>.</summary>
    public const int SyncGroupOffset = Ft4SymbolEncoder.SyncGroupOffset;

    /// <summary>Where the first sync group begins. Symbol 0 is a ramp, so the groups start at 1.</summary>
    public const int FirstSyncGroupStart = Ft4SymbolEncoder.FirstSyncGroupStart;

    /// <summary>Tones an FT4 symbol may take, which is how many bins a candidate spans.</summary>
    public const int ToneCount = Ft4SymbolEncoder.ToneCount;

    /// <summary>
    /// The lowest block offset swept, negative on purpose: a transmission that began before the slot
    /// was opened is still findable as long as enough of it is inside. Upstream's own lower bound.
    /// </summary>
    public const int DefaultFirstBlockOffset = -10;

    /// <summary>
    /// The highest block offset swept, inclusive. <b>51, where the demo application uses 19, and it
    /// is the one number in the FT4 path that is not upstream's.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Why it had to move.</b> <c>ftx_find_candidates</c> sweeps <c>time_offset</c> from -10 to
    /// +19 for both protocols. At FT8's 0.160 s block that reaches 3.04 s into a 15 s slot, and an
    /// FT8 signal centred in its slot starts at 1.18 s — comfortably inside. At FT4's 0.048 s block
    /// the same range reaches <b>0.912 s</b>, and an FT4 signal centred in its slot by upstream's own
    /// generator starts at <b>1.23 s</b>. Unit 288 found that upstream's own decoder reads zero
    /// messages out of upstream's own generator, and unit 289 task 1 measured why: the signal is a
    /// third of a second past the last place the search looks.
    /// </para>
    /// <para>
    /// <b>Why 51, and why this is a number rather than a guess.</b> A 7.5 s slot holds 156 blocks and
    /// a transmission occupies 105 of them, so a transmission that fits wholly inside its own slot
    /// begins somewhere in blocks 0 to 51. That covers every placement a well-formed FT4 signal can
    /// have, including upstream's centred 25, and it costs about twice the demo application's
    /// hypothesis count on a protocol whose waterfall is a third of FT8's width to begin with.
    /// </para>
    /// <para>
    /// <b>Why this is not a divergence on a constant, in the sense the standing ruling forbids.</b>
    /// The bound is not in <c>ft8/</c> at all. It is the same class of number as
    /// <see cref="DefaultMinimumScore"/> and <see cref="DefaultCandidateLimit"/> — a file-scope
    /// judgement in <c>demo/decode_ft8.c</c> about how much work to do — and it is a constructor
    /// parameter here for exactly that reason. <b>Nothing about the modulation, the tables, the
    /// codeword or the waveform is changed.</b> A caller who wants upstream's own sweep can ask for
    /// it and will get upstream's own result, which is nothing.
    /// </para>
    /// </remarks>
    public const int DefaultLastBlockOffset = 51;

    /// <summary>
    /// The score below which a candidate is discarded. Upstream's <c>kMin_score</c>, and the same
    /// weak anchor <see cref="Ft8SyncSearch.DefaultMinimumScore"/> records: a file-scope constant in
    /// <c>demo/decode_ft8.c</c> rather than anything in the library.
    /// </summary>
    public const int DefaultMinimumScore = 10;

    /// <summary>The most candidates returned. Upstream's <c>kMax_candidates</c>, same anchor.</summary>
    public const int DefaultCandidateLimit = 140;

    /// <summary>Builds a search, or refuses one.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The candidate limit is negative, or the block offset range is inverted.
    /// </exception>
    public Ft4SyncSearch(
        int candidateLimit = DefaultCandidateLimit,
        int minimumScore = DefaultMinimumScore,
        int firstBlockOffset = DefaultFirstBlockOffset,
        int lastBlockOffset = DefaultLastBlockOffset)
    {
        if (candidateLimit < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(candidateLimit),
                candidateLimit,
                "A candidate limit is how many places to return and cannot be negative. Zero is "
                + "allowed and means an empty list.");
        }

        if (lastBlockOffset < firstBlockOffset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lastBlockOffset),
                lastBlockOffset,
                $"The block offset sweep runs from {firstBlockOffset} to {lastBlockOffset}, which is "
                + "empty. A sweep with no positions in it would report an empty band rather than a "
                + "mistake, so it is refused.");
        }

        CandidateLimit = candidateLimit;
        MinimumScore = minimumScore;
        FirstBlockOffset = firstBlockOffset;
        LastBlockOffset = lastBlockOffset;
    }

    /// <summary>The most candidates this search returns.</summary>
    public int CandidateLimit { get; }

    /// <summary>The score at or above which a candidate is kept.</summary>
    public int MinimumScore { get; }

    /// <summary>The lowest block offset swept.</summary>
    public int FirstBlockOffset { get; }

    /// <summary>The highest block offset swept, inclusive.</summary>
    public int LastBlockOffset { get; }

    /// <summary>Analyses a slot of audio and returns the transmissions found in it, strongest first.</summary>
    /// <param name="samples">The audio. At least one block long.</param>
    /// <param name="geometry">
    /// The extents to analyse to, or null for FT4's own at 12 kHz. <b>An extent, not a hint.</b>
    /// </param>
    public IReadOnlyList<Ft8Candidate> Find(
        ReadOnlySpan<float> samples, Ft4WaterfallGeometry? geometry = null) =>
        Find(new Ft8Monitor(geometry ?? new Ft4WaterfallGeometry()).Analyse(samples));

    /// <summary>Returns the transmissions found in an already-built waterfall, strongest first.</summary>
    /// <exception cref="ArgumentNullException">The waterfall is null.</exception>
    public IReadOnlyList<Ft8Candidate> Find(Ft8Waterfall waterfall)
    {
        ArgumentNullException.ThrowIfNull(waterfall);

        var geometry = waterfall.Geometry;
        var lastBin = geometry.BinCount - ToneCount;

        var kept = new List<Ft8Candidate>();

        for (var timeSub = 0; timeSub < geometry.TimeOversampling; timeSub++)
        {
            for (var freqSub = 0; freqSub < geometry.FrequencyOversampling; freqSub++)
            {
                for (var block = FirstBlockOffset; block <= LastBlockOffset; block++)
                {
                    for (var bin = 0; bin <= lastBin; bin++)
                    {
                        var score = ScoreAt(waterfall, block, timeSub, bin, freqSub);
                        if (score < MinimumScore)
                        {
                            continue;
                        }

                        kept.Add(new Ft8Candidate(score, block, timeSub, bin, freqSub));
                    }
                }
            }
        }

        kept.Sort();

        if (kept.Count > CandidateLimit)
        {
            kept.RemoveRange(CandidateLimit, kept.Count - CandidateLimit);
        }

        return kept;
    }

    /// <summary>
    /// The Costas sync score of one hypothesis: <b>upstream's <c>ft4_sync_score</c>, term for term,
    /// guard for guard, in the same order.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>What is being summed.</b> For each of the sixteen sync symbols, the stored magnitude of the
    /// tone row <c>m</c> of the FT4 Costas table says should be there, minus the stored magnitude of
    /// each of up to four neighbours: one bin lower, one bin higher, one symbol earlier and one
    /// symbol later. Each term is taken only where its neighbour exists, and the total is divided by
    /// the number actually taken.
    /// </para>
    /// <para>
    /// <b>The frequency guards are FT4's own.</b> Upstream tests <c>sm &gt; 0</c> and <c>sm &lt; 3</c>
    /// rather than FT8's <c>sm &lt; 7</c>, because the alphabet is four tones. A port that carried
    /// FT8's 7 across would read a fifth bin that belongs to the next block.
    /// </para>
    /// <para>
    /// <b>Stored bytes, not decibels</b>, and the division at the end truncates toward zero — which C
    /// and C# do identically, including for a negative total. The same reading
    /// <see cref="Ft8SyncSearch.ScoreAt"/> records.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The waterfall is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A sub-offset is outside the geometry, or the bin offset does not leave room for four tones.
    /// </exception>
    public static int ScoreAt(
        Ft8Waterfall waterfall,
        int blockOffset,
        int timeSubOffset,
        int binOffset,
        int frequencySubOffset)
    {
        ArgumentNullException.ThrowIfNull(waterfall);

        var geometry = waterfall.Geometry;

        if (timeSubOffset < 0 || timeSubOffset >= geometry.TimeOversampling)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeSubOffset),
                timeSubOffset,
                $"There are {geometry.TimeOversampling} time sub-offsets per block.");
        }

        if (frequencySubOffset < 0 || frequencySubOffset >= geometry.FrequencyOversampling)
        {
            throw new ArgumentOutOfRangeException(
                nameof(frequencySubOffset),
                frequencySubOffset,
                $"There are {geometry.FrequencyOversampling} frequency sub-offsets per bin.");
        }

        if (binOffset < 0 || binOffset + ToneCount > geometry.BinCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(binOffset),
                binOffset,
                $"An FT4 candidate spans {ToneCount} bins and the waterfall keeps "
                + $"{geometry.BinCount}, so the bin offset runs 0 to "
                + $"{geometry.BinCount - ToneCount}. A hypothesis whose top tone falls outside the "
                + "passband would be scored against bins belonging to the next block, which is "
                + "silent corruption rather than a weak signal.");
        }

        var magnitudes = waterfall.Magnitudes;
        var blocks = waterfall.BlockCount;
        var stride = geometry.BlockStride;
        var costas = Ft8Tables.Ft4CostasPattern;

        var score = 0;
        var averagedOver = 0;

        for (var group = 0; group < SyncGroupCount; group++)
        {
            for (var k = 0; k < SyncGroupLength; k++)
            {
                var block = FirstSyncGroupStart + (SyncGroupOffset * group) + k;
                var blockAbsolute = blockOffset + block;

                // Before the analysis: skip this symbol and keep going. Past the end of it: abandon
                // the rest of this group. Upstream's own asymmetry, kept.
                if (blockAbsolute < 0)
                {
                    continue;
                }

                if (blockAbsolute >= blocks)
                {
                    break;
                }

                var start = (((blockAbsolute * geometry.TimeOversampling) + timeSubOffset)
                        * geometry.FrequencyOversampling
                        + frequencySubOffset)
                    * geometry.BinCount
                    + binOffset;

                // ROW m OF THE TABLE AT GROUP m. This is the structural difference from FT8 and it
                // is the line a port that treated FT4 as FT8-with-other-constants gets wrong.
                int expected = costas[(group * SyncGroupLength) + k];
                var here = magnitudes[start + expected];

                if (expected > 0)
                {
                    score += here - magnitudes[start + expected - 1];
                    averagedOver++;
                }

                if (expected < ToneCount - 1)
                {
                    score += here - magnitudes[start + expected + 1];
                    averagedOver++;
                }

                if (k > 0 && blockAbsolute > 0)
                {
                    score += here - magnitudes[start + expected - stride];
                    averagedOver++;
                }

                if (k + 1 < SyncGroupLength && blockAbsolute + 1 < blocks)
                {
                    score += here - magnitudes[start + expected + stride];
                    averagedOver++;
                }
            }
        }

        if (averagedOver > 0)
        {
            score /= averagedOver;
        }

        return score;
    }
}
