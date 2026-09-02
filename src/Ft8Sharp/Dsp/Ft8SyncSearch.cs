using System;
using System.Collections.Generic;

namespace Ft8Sharp.Dsp;

/// <summary>
/// Finds where transmissions are. Given a slot of audio — or the waterfall built from one — and
/// <b>nothing else</b>, it correlates the seven-tone Costas sync pattern against every position the
/// geometry admits and returns the strongest, ranked.
/// </summary>
/// <remarks>
/// <para>
/// <b>Nothing tells it where to look, and there is no parameter through which anything could.</b>
/// <see cref="Find(Ft8Waterfall)"/> and <see cref="Find(ReadOnlySpan{float}, Ft8WaterfallGeometry?)"/>
/// take the data and the extents and take no frequency, no time, no offset and no hint of any kind.
/// That is deliberate and it is the whole difference between this type and everything the library
/// could do before it: a search with a hint parameter cannot be shown not to have used it.
/// </para>
/// <para>
/// <b>Ported from <c>ftx_find_candidates</c> and <c>ft8_sync_score</c> in <c>ft8/decode.c</c></b>,
/// read through the test process at the pinned commit and asserted by
/// <c>UpstreamSyncSearchInventoryTests</c>. The scoring arithmetic is upstream's, term for term and
/// guard for guard, in the same order. <b>It is not ported from
/// <c>src/Hamlet.RadioEngine/Audio/Ft8Sync.cs</c></b>, which is this repository's own Costas search
/// and was deliberately not read for structure, not copied and not referenced — see
/// <c>porting-notes.md</c>.
/// </para>
/// <para>
/// <b>What it does NOT do.</b> It does not demodulate. No soft symbol, no log-likelihood ratio, no
/// belief propagation, no CRC, no text. <b>A candidate is a place, not a message</b>, and a strong
/// score is not a decode.
/// </para>
/// <para>
/// <b>Deterministic, and it is checked rather than intended.</b> No clock, no random source, no
/// environment, no parallelism, no dictionary. Every hypothesis is scored, those at or above the
/// minimum are sorted into the total order <see cref="Ft8Candidate.CompareTo"/> defines, and the
/// first <see cref="CandidateLimit"/> are returned. The same waterfall gives the same list, element
/// for element, on every run.
/// </para>
/// <para>
/// <b>Two divergences from upstream, both about the ranking and neither about the score.</b>
/// Upstream keeps candidates in a min-heap on <c>score</c> alone and then heapsorts them, which is
/// not stable, so tied candidates come back in whatever order the heap's swaps left. This scores
/// every hypothesis and sorts the survivors on a total order instead. Recorded as divergences 19 and
/// 20 in <c>porting-notes.md</c>. The set of scores returned is upstream's; which of several equally
/// scored candidates survives the cut, and in what order, is this library's and is defined.
/// </para>
/// <para><b>Thread-safe.</b> It holds no mutable state. One instance may serve many callers.</para>
/// </remarks>
public sealed class Ft8SyncSearch
{
    /// <summary>Symbols in one Costas sync group. <c>FT8_LENGTH_SYNC</c>.</summary>
    public const int SyncGroupLength = 7;

    /// <summary>Sync groups in one transmission. <c>FT8_NUM_SYNC</c>.</summary>
    public const int SyncGroupCount = 3;

    /// <summary>Symbols from the start of one sync group to the next. <c>FT8_SYNC_OFFSET</c>.</summary>
    public const int SyncGroupOffset = 36;

    /// <summary>Tones an FT8 symbol may take, which is how many bins a candidate spans.</summary>
    public const int ToneCount = 8;

    /// <summary>
    /// The lowest block offset swept, and it is negative on purpose: a transmission that began
    /// before the slot was opened is still findable as long as enough of it is inside.
    /// <b>The application's choice, not the protocol's</b> — see the remarks on
    /// <see cref="MinimumScore"/>.
    /// </summary>
    public const int DefaultFirstBlockOffset = -10;

    /// <summary>The highest block offset swept, inclusive.</summary>
    public const int DefaultLastBlockOffset = 19;

    /// <summary>
    /// The score below which a candidate is discarded. <b>Upstream's <c>kMin_score</c>, and it is
    /// the weakest anchor in this port</b>: it is not in the library at all, it is a file-scope
    /// constant in <c>demo/decode_ft8.c</c>. It is one application's judgement about how much
    /// sensitivity to trade for how much work, so it is a parameter here rather than a literal
    /// buried in a loop.
    /// </summary>
    public const int DefaultMinimumScore = 10;

    /// <summary>
    /// The most candidates returned. <b>Upstream's <c>kMax_candidates</c>, and the same weakest
    /// anchor</b>: also the demo application's, also absent from the library.
    /// </summary>
    public const int DefaultCandidateLimit = 140;

    /// <summary>Builds a search, or refuses one.</summary>
    /// <param name="candidateLimit">
    /// The most candidates to return. Zero is allowed and means an empty list.
    /// </param>
    /// <param name="minimumScore">
    /// The score at or above which a candidate is kept. No candidate below it is ever returned, at
    /// any rank, for any reason.
    /// </param>
    /// <param name="firstBlockOffset">The lowest block offset swept, ordinarily negative.</param>
    /// <param name="lastBlockOffset">The highest block offset swept, inclusive.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The candidate limit is negative, or the block offset range is inverted.
    /// </exception>
    public Ft8SyncSearch(
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

    /// <summary>
    /// Analyses a slot of audio and returns the transmissions found in it, strongest first.
    /// </summary>
    /// <param name="samples">The audio. At least one block long.</param>
    /// <param name="geometry">
    /// The extents to analyse to, or null for FT8's own at 12 kHz. <b>This is an extent, not a
    /// hint</b>: it says how wide the passband is and how finely to subdivide it, and says nothing
    /// about where a signal might be.
    /// </param>
    /// <remarks>
    /// <b>The samples and the geometry, and nothing else.</b> There is no third parameter and there
    /// is no overload that takes one.
    /// </remarks>
    public IReadOnlyList<Ft8Candidate> Find(
        ReadOnlySpan<float> samples, Ft8WaterfallGeometry? geometry = null) =>
        Find(new Ft8Monitor(geometry).Analyse(samples));

    /// <summary>
    /// Returns the transmissions found in an already-built waterfall, strongest first.
    /// </summary>
    /// <param name="waterfall">The spectrogram of one slot.</param>
    /// <returns>
    /// At most <see cref="CandidateLimit"/> candidates, every one of them at or above
    /// <see cref="MinimumScore"/>, in the total order <see cref="Ft8Candidate.CompareTo"/> defines.
    /// <b>Empty is an ordinary answer</b> and is returned cleanly: a minimum no hypothesis reaches,
    /// a limit of zero and a waterfall with nothing in it all give an empty list rather than an
    /// exception or a partly filled one.
    /// </returns>
    /// <exception cref="ArgumentNullException">The waterfall is null.</exception>
    public IReadOnlyList<Ft8Candidate> Find(Ft8Waterfall waterfall)
    {
        ArgumentNullException.ThrowIfNull(waterfall);

        var geometry = waterfall.Geometry;
        var lastBin = geometry.BinCount - ToneCount;

        var kept = new List<Ft8Candidate>();

        // Upstream's sweep, in upstream's own nesting: both sub-offset axes outermost, then the
        // block offsets from before the start of the slot, then every bin offset that still leaves
        // room for the eighth tone. THE ORDER OF THIS SWEEP DOES NOT AFFECT THE ANSWER — the result
        // is sorted on a total order afterwards — and Ft8SyncSearchTests runs it reversed to show
        // that it does not.
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
    /// The Costas sync score of one hypothesis: <b>upstream's <c>ft8_sync_score</c>, term for term,
    /// guard for guard, in the same order.</b>
    /// </summary>
    /// <param name="waterfall">The spectrogram to read.</param>
    /// <param name="blockOffset">
    /// The block the hypothesised transmission's first symbol sits in. May be negative.
    /// </param>
    /// <param name="timeSubOffset">Which time subdivision of that block.</param>
    /// <param name="binOffset">The bin the hypothesised lowest tone sits in.</param>
    /// <param name="frequencySubOffset">Which frequency subdivision of that bin.</param>
    /// <returns>
    /// The sum of every neighbour difference taken, divided by how many were taken. Zero where none
    /// was — a hypothesis entirely outside the analysed blocks scores zero rather than throwing.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <b>What is being summed.</b> For each of the twenty-one sync symbols, the stored magnitude of
    /// the tone the Costas pattern says should be there, minus the stored magnitude of each of up to
    /// four neighbours: one bin lower, one bin higher, one symbol earlier and one symbol later. Each
    /// term is taken only where its neighbour exists, and the total is divided by the number
    /// actually taken, which is what makes a candidate at the edge of the slot comparable with one
    /// in the middle.
    /// </para>
    /// <para>
    /// <b>Stored bytes, not decibels.</b> Upstream reads through <c>WF_ELEM_MAG_INT</c>, which in
    /// the compiled branch is the identity on a <c>uint8_t</c>. So the arithmetic is in whole counts
    /// of half a decibel, in integers, and the division at the end truncates toward zero — which C
    /// and C# do identically, including for a negative total.
    /// </para>
    /// <para>
    /// <b>The two boundary rules are not symmetric, and that is upstream's.</b> A sync block before
    /// the start of the analysis is skipped and the group carries on; a sync block past the end of
    /// it abandons the rest of that group.
    /// </para>
    /// <para>
    /// <b>This is a scoring primitive and not a search.</b> It is public because a caller measuring
    /// the search — asking what the score was at a position it already knows — needs it, and because
    /// nothing about the search's own answer can be reached through it. <b>The search itself is
    /// never told a position</b>, and this method is not on the path by which it finds one.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The waterfall is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A sub-offset is outside the geometry, or the bin offset does not leave room for eight tones.
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
                $"A candidate spans {ToneCount} bins and the waterfall keeps {geometry.BinCount}, so "
                + $"the bin offset runs 0 to {geometry.BinCount - ToneCount}. A hypothesis whose top "
                + "tone falls outside the passband would be scored against bins belonging to the "
                + "next block, which is silent corruption rather than a weak signal.");
        }

        var magnitudes = waterfall.Magnitudes;
        var blocks = waterfall.BlockCount;
        var stride = geometry.BlockStride;
        var costas = Ft8Tables.Ft8CostasPattern;

        var score = 0;
        var averagedOver = 0;

        for (var group = 0; group < SyncGroupCount; group++)
        {
            for (var k = 0; k < SyncGroupLength; k++)
            {
                var block = (SyncGroupOffset * group) + k;
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

                // The first of the eight tone bins of this symbol. Computed from the absolute block
                // rather than carried as a running pointer, because upstream's pointer is allowed to
                // sit before the array while the block offset is negative and a managed index is
                // not. The arithmetic that lands here is identical.
                var start = (((blockAbsolute * geometry.TimeOversampling) + timeSubOffset)
                        * geometry.FrequencyOversampling
                        + frequencySubOffset)
                    * geometry.BinCount
                    + binOffset;

                int expected = costas[k];
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
