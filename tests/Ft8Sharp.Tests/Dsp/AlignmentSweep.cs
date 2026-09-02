using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Message;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The instrument unit 219 is built on: one named transmission, asked at every alignment in a
/// bounded neighbourhood whether it is there.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE PROHIBITION THIS FILE EXISTS UNDER, WRITTEN WHERE IT CANNOT BE MISSED.</b> This is a
/// diagnostic and it is told everything — the file, the frequency the list gives, and the exact
/// text. <b>That is what makes it able to answer the question, and it is exactly why its answer may
/// never be counted.</b> A point at which this sweep recovers the expected text is <b>evidence that
/// the transmission is present in the recording</b> and it is nothing else: it is not a decode, it
/// is not a match, it does not move criterion 3's 760, and it is added to no total anywhere.
/// Criterion 3 is re-taken only through
/// <c>TheReferenceRecordingsDecodeAgainstUpstreamsOwnExpectedLists</c>, unchanged, where nothing
/// about the expected answer reaches <see cref="Ft8SlotDecoder"/>, <see cref="Ft8SoftSymbols"/>,
/// <see cref="Ft8SyncSearch"/> or <see cref="Ft8CodewordDecoder"/>.
/// </para>
/// <para>
/// <b>Why sweeping and not reading the nearest kept candidate.</b> Unit 217 and unit 218 both read
/// the hard-decision agreement <em>at the nearest kept candidate</em>, and unit 218's own report
/// recorded that this reading could not be settled: <b>the nearest kept candidate to a missed line
/// may be sitting on somebody else's transmission entirely.</b> A neighbourhood sweep does not have
/// that ambiguity, because it asks at every alignment the search itself could have proposed rather
/// than at the one place the search happened to keep.
/// </para>
/// <para>
/// <b>Three readings at every point</b>, and all three come out of parts that already exist:
/// <see cref="Ft8SyncSearch.ScoreAt"/>, which its own file documents as a scoring primitive for a
/// caller asking what the score was at a position it already knows; the hard-decision agreement out
/// of 174 against the true codeword, through <see cref="Ft8SoftSymbols.Extract"/> and
/// <see cref="Ft8SoftSymbols.HardDecision"/>; and <see cref="Ft8CodewordDecoder.Decode"/> at the
/// points worth decoding.
/// </para>
/// <para>
/// <b>The true codeword is this library's own encoder's</b>, through the same
/// <c>Ft8Payload.Create</c> and <see cref="LdpcEncoder.Encode"/> chain
/// <c>Ft8MissAccountingTests</c> already uses — the chain unit 212 proved bit-identical to
/// upstream's own encoder over 51 of 51 messages. <b>It is rebuilt here rather than borrowed only
/// because that file's copy is private</b>; the three steps are the same three steps.
/// </para>
/// </remarks>
internal static class AlignmentSweep
{
    /// <summary>
    /// Bins swept either side of the bin the list's frequency lands in. <b>Two bins is two whole
    /// FT8 tone spacings</b>, and with both frequency sub-offsets that reaches about 15.6 Hz either
    /// way — comfortably past the four-hertz test every previous unit used.
    /// </summary>
    internal const int BinSpan = 2;

    /// <summary>
    /// How many of the neighbourhood's points are decoded, taken in order of agreement.
    /// <b>Belief propagation is the only expensive thing here</b>; scoring and agreement are cheap,
    /// so every point is scored and agreed and only the most promising are decoded. Every point the
    /// search itself kept is decoded as well, whatever its agreement, so the sweep can never miss a
    /// decode the untold path could have had.
    /// </summary>
    internal const int DecodeBudget = 20;

    /// <summary>
    /// The agreement at or above which a line with no decoding point is called <b>present and not
    /// recoverable</b> rather than <b>not present</b>. <b>Fixed before the run</b> and read against
    /// the quiet-frequency control, which is swept over the same number of points so that the
    /// best-of-neighbourhood statistic has a null distribution of its own.
    /// </summary>
    internal const int PresentButUnrecoverable = 130;

    /// <summary>One alignment in the neighbourhood, and the two cheap readings taken there.</summary>
    internal readonly record struct Point(int Block, int TimeSub, int Bin, int FreqSub, int Score, int Agreement)
    {
        public override string ToString() => $"blk {Block,3} t{TimeSub} bin {Bin,3} f{FreqSub}";
    }

    /// <summary>Everything one swept line produced.</summary>
    internal sealed class Outcome
    {
        internal required string File { get; init; }

        internal required string Text { get; init; }

        internal required double ListHz { get; init; }

        internal required double ListSnr { get; init; }

        /// <summary>The bin and sub-offset the list's frequency lands in — the centre of the sweep.</summary>
        internal int CentreBin { get; init; }

        internal int CentreFreqSub { get; init; }

        internal int Points { get; init; }

        internal int DecodesRun { get; init; }

        internal Point BestAgreement { get; init; }

        internal Point BestScore { get; init; }

        /// <summary>The rank, one-based, the search gave the best-agreeing point. -1 if it kept none.</summary>
        internal int RankOfBestAgreementPoint { get; init; }

        /// <summary>The best rank the search gave any point in the whole neighbourhood. -1 if none.</summary>
        internal int BestRankInNeighbourhood { get; init; }

        /// <summary>How many of the neighbourhood's points the search kept as candidates.</summary>
        internal int KeptPointsInNeighbourhood { get; init; }

        /// <summary><b>The recovered text equalled the expected text exactly.</b> Nothing else counts.</summary>
        internal bool Decoded { get; init; }

        internal Point? DecodedAt { get; init; }

        /// <summary>
        /// The belief propagation's 174 corrected bits equalled the true codeword exactly, whether or
        /// not the message layer could then put it into words. Reported beside <see cref="Decoded"/>
        /// and never in place of it.
        /// </summary>
        internal bool CodewordRecovered { get; init; }

        internal Point? CodewordRecoveredAt { get; init; }

        /// <summary>Divergence 22's refusal, where a hand-built candidate met it. Null otherwise.</summary>
        internal string? PassbandRefusal { get; init; }

        /// <summary>A, B or C. Decided by the numbers above and by nothing else.</summary>
        internal char Verdict =>
            Decoded ? 'A'
            : BestAgreement.Agreement >= PresentButUnrecoverable ? 'B'
            : 'C';

        internal string Row() =>
            $"{File,-22} {ListHz,7:F0} {ListSnr,5:F0} {BestAgreement.Agreement,5} "
            + $"[{BestAgreement}] {BestScore.Score,5} [{BestScore}] "
            + $"{(BestRankInNeighbourhood > 0 ? BestRankInNeighbourhood.ToString() : "-"),5} "
            + $"{(Decoded ? "DECODED" : CodewordRecovered ? "codeword" : "no"),8} {Verdict}  {Text}";
    }

    /// <summary>
    /// The exact 174-bit codeword the expected text puts on the air, or null where this library's
    /// message layer cannot pack that text at all.
    /// </summary>
    /// <remarks>
    /// The three steps <see cref="LdpcEncoder"/> itself takes, in its order: the 77 message bits,
    /// then the payload with its CRC-14, then the parity.
    /// </remarks>
    internal static byte[]? TrueCodeword(string text)
    {
        if (ExpectedMessagePacker.TryPack(text, out var message) != ExpectedMessagePacker.PackFailure.None)
        {
            return null;
        }

        var payload = new byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(message, payload);

        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);

        // One byte per bit, so the comparison against a hard decision is a byte compare.
        var bits = new byte[Ft8SoftSymbols.RatioCount];
        for (var bit = 0; bit < bits.Length; bit++)
        {
            bits[bit] = (byte)((codeword[bit / 8] >> (7 - (bit % 8))) & 1);
        }

        return bits;
    }

    /// <summary>
    /// <b>The sweep.</b> Every alignment in the neighbourhood of <paramref name="listHz"/> is scored
    /// and agreed against <paramref name="trueCodeword"/>; the most promising are decoded.
    /// </summary>
    /// <param name="waterfall">Built once per recording by the caller and shared across its lines.</param>
    /// <param name="candidates">The search's own kept list for that waterfall, read and never filtered.</param>
    /// <param name="search">Supplies the block-offset extents, so the sweep spans what the search spans.</param>
    internal static Outcome Sweep(
        Ft8Waterfall waterfall,
        IReadOnlyList<Ft8Candidate> candidates,
        Ft8SyncSearch search,
        string file,
        string text,
        double listSnr,
        double listHz,
        byte[] trueCodeword)
    {
        var geometry = waterfall.Geometry;
        geometry.TryBinFor(listHz, out var centreBin, out var centreFreqSub);

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var decisions = new byte[Ft8SoftSymbols.RatioCount];
        var corrected = new byte[LdpcDecoder.CodewordBits];

        var points = new List<Point>(
            ((search.LastBlockOffset - search.FirstBlockOffset + 1)
                * geometry.TimeOversampling
                * ((2 * BinSpan) + 1)
                * geometry.FrequencyOversampling));

        string? refusal = null;

        for (var bin = centreBin - BinSpan; bin <= centreBin + BinSpan; bin++)
        {
            if (bin < 0 || bin + Ft8SyncSearch.ToneCount - 1 >= geometry.BinCount)
            {
                // DIVERGENCE 22. Upstream reads past the end of its array here; this library refuses,
                // and a hand-built candidate near the edge of the passband can meet that refusal.
                // It is REPORTED, not worked around and not fixed.
                refusal ??= $"bin {bin} needs bins {bin}..{bin + Ft8SyncSearch.ToneCount - 1} "
                    + $"and this waterfall keeps {geometry.BinCount}";
                continue;
            }

            for (var freqSub = 0; freqSub < geometry.FrequencyOversampling; freqSub++)
            {
                for (var block = search.FirstBlockOffset; block <= search.LastBlockOffset; block++)
                {
                    for (var timeSub = 0; timeSub < geometry.TimeOversampling; timeSub++)
                    {
                        var score = Ft8SyncSearch.ScoreAt(waterfall, block, timeSub, bin, freqSub);

                        Ft8SoftSymbols.Extract(waterfall, new Ft8Candidate(score, block, timeSub, bin, freqSub), ratios);
                        Ft8SoftSymbols.Normalise(ratios);
                        Ft8SoftSymbols.HardDecision(ratios, decisions);

                        var agree = 0;
                        for (var bit = 0; bit < decisions.Length; bit++)
                        {
                            if (decisions[bit] == trueCodeword[bit])
                            {
                                agree++;
                            }
                        }

                        points.Add(new Point(block, timeSub, bin, freqSub, score, agree));
                    }
                }
            }
        }

        if (points.Count == 0)
        {
            return new Outcome
            {
                File = file,
                Text = text,
                ListHz = listHz,
                ListSnr = listSnr,
                CentreBin = centreBin,
                CentreFreqSub = centreFreqSub,
                Points = 0,
                PassbandRefusal = refusal,
                RankOfBestAgreementPoint = -1,
                BestRankInNeighbourhood = -1,
            };
        }

        var bestAgreement = points[0];
        var bestScore = points[0];
        foreach (var point in points)
        {
            if (point.Agreement > bestAgreement.Agreement)
            {
                bestAgreement = point;
            }

            if (point.Score > bestScore.Score)
            {
                bestScore = point;
            }
        }

        // Where the search itself stood on these points. A READING and never a change: nothing here
        // alters the candidate limit, the minimum score, or which candidates the search kept.
        var ranks = new Dictionary<(int, int, int, int), int>();
        for (var i = 0; i < candidates.Count; i++)
        {
            var c = candidates[i];
            ranks[(c.BlockOffset, c.TimeSubOffset, c.BinOffset, c.FrequencySubOffset)] = i + 1;
        }

        var keptHere = 0;
        var bestRank = -1;
        foreach (var point in points)
        {
            if (ranks.TryGetValue((point.Block, point.TimeSub, point.Bin, point.FreqSub), out var rank))
            {
                keptHere++;
                if (bestRank < 0 || rank < bestRank)
                {
                    bestRank = rank;
                }
            }
        }

        ranks.TryGetValue(
            (bestAgreement.Block, bestAgreement.TimeSub, bestAgreement.Bin, bestAgreement.FreqSub),
            out var rankOfBest);

        // THE DECODE RULE, STATED IN THE REPORT BEFORE THE RUN: the DecodeBudget best-agreeing
        // points, plus every point in the neighbourhood the search itself kept.
        var toDecode = points
            .OrderByDescending(p => p.Agreement)
            .ThenByDescending(p => p.Score)
            .Take(DecodeBudget)
            .ToList();

        foreach (var point in points)
        {
            if (ranks.ContainsKey((point.Block, point.TimeSub, point.Bin, point.FreqSub))
                && !toDecode.Contains(point))
            {
                toDecode.Add(point);
            }
        }

        var decoded = false;
        Point? decodedAt = null;
        var codewordRecovered = false;
        Point? codewordAt = null;

        foreach (var point in toDecode)
        {
            var candidate = new Ft8Candidate(point.Score, point.Block, point.TimeSub, point.Bin, point.FreqSub);
            Ft8SoftSymbols.Extract(waterfall, candidate, ratios);
            Ft8SoftSymbols.Normalise(ratios);

            // No cache. The sweep never hands the decoder anything, and a callsign memory built from
            // the expected text would be handing it the answer.
            var result = Ft8CodewordDecoder.Decode(ratios);

            if (!codewordRecovered)
            {
                var correction = LdpcDecoder.Decode(ratios, corrected);
                if (correction.ParitySatisfied && corrected.AsSpan().SequenceEqual(trueCodeword))
                {
                    codewordRecovered = true;
                    codewordAt = point;
                }
            }

            // A DECODE IS TEXT EQUALITY AND NOTHING ELSE.
            if (result.Decoded
                && string.Equals(
                    ReferenceRecording.Normalise(result.Message.Text), text, StringComparison.Ordinal))
            {
                decoded = true;
                decodedAt = point;
                break;
            }
        }

        return new Outcome
        {
            File = file,
            Text = text,
            ListHz = listHz,
            ListSnr = listSnr,
            CentreBin = centreBin,
            CentreFreqSub = centreFreqSub,
            Points = points.Count,
            DecodesRun = toDecode.Count,
            BestAgreement = bestAgreement,
            BestScore = bestScore,
            RankOfBestAgreementPoint = rankOfBest == 0 ? -1 : rankOfBest,
            BestRankInNeighbourhood = bestRank,
            KeptPointsInNeighbourhood = keptHere,
            Decoded = decoded,
            DecodedAt = decodedAt,
            CodewordRecovered = codewordRecovered,
            CodewordRecoveredAt = codewordAt,
            PassbandRefusal = refusal,
        };
    }

    /// <summary>
    /// The hard-decision agreement at one already-found candidate — <b>unit 217's own reading</b>,
    /// written independently here so that the two can be held against each other in task 2.
    /// </summary>
    internal static int AgreementAt(Ft8Waterfall waterfall, Ft8Candidate candidate, byte[] trueCodeword)
    {
        var ratios = new float[Ft8SoftSymbols.RatioCount];
        Ft8SoftSymbols.Extract(waterfall, candidate, ratios);
        Ft8SoftSymbols.Normalise(ratios);

        var decisions = new byte[Ft8SoftSymbols.RatioCount];
        Ft8SoftSymbols.HardDecision(ratios, decisions);

        var agree = 0;
        for (var bit = 0; bit < decisions.Length; bit++)
        {
            if (decisions[bit] == trueCodeword[bit])
            {
                agree++;
            }
        }

        return agree;
    }

    /// <summary>One expected line as the lists give it: the text, its SNR and its frequency.</summary>
    internal readonly record struct ExpectedLine(string Text, double Snr, double Hz);

    /// <summary>
    /// Every line of one recording's expected list that carries all three fields.
    /// <b>Read after the search has run and used only to choose where to look.</b>
    /// </summary>
    internal static IReadOnlyList<ExpectedLine> ExpectedLines(ReferenceRecording recording)
    {
        var lines = new List<ExpectedLine>();

        foreach (var raw in File.ReadAllLines(recording.ExpectedPath))
        {
            var tilde = raw.IndexOf('~');
            if (tilde < 0)
            {
                continue;
            }

            var text = ReferenceRecording.Normalise(raw[(tilde + 1)..]);
            if (text.Length == 0)
            {
                continue;
            }

            var fields = raw[..tilde].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 4
                || !double.TryParse(fields[1], out var snr)
                || !double.TryParse(fields[3], out var hz))
            {
                continue;
            }

            lines.Add(new ExpectedLine(text, snr, hz));
        }

        return lines;
    }
}
