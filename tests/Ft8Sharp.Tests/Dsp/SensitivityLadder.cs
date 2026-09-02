using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Tests.Encode;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The rungs unit 218 walks down, and the row shape every ladder in this unit reports in — so the
/// aligned ladder and the impaired ones are read off the same axis rather than off three of them.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is a diagnostic ladder, not step 6's sensitivity curve, and the difference is not
/// cosmetic.</b> Step 6 wants a curve generated across a range of SNR and reproducible, a decode rate
/// at -21 dB compared against the published figure as a verdict, and behaviour below the threshold
/// shown to degrade rather than produce wrong decodes. <b>None of those three is claimed here.</b>
/// This is one session's measurement of where this path stops answering, taken so that criterion 3's
/// residue can be read as either this receiver's deafness or the benchmark's reach.
/// </para>
/// <para>
/// <b>The instrument is the one that already exists.</b> <see cref="SignalToNoise"/>,
/// <see cref="GaussianNoise"/> and <see cref="SearchFixture"/> were built by unit 214 and used by
/// unit 216; nothing here writes a second noise generator or a second convention, because two
/// conventions in one tree is how a measurement quietly stops being comparable.
/// </para>
/// <para>
/// <b>Nothing on this class ever reaches the decode path.</b> The truth lives here and is compared
/// against what came back, after it came back.
/// </para>
/// </remarks>
internal static class SensitivityLadder
{
    /// <summary>
    /// The requested ratios, in decibels in the 2500 Hz reference bandwidth.
    /// </summary>
    /// <remarks>
    /// <b>-10 is the one rung anybody has stood on</b> —
    /// <c>Ft8SlotDecoderTests.TheCorpusComesBackInSeededNoiseAtAMeasuredRatio</c> — and the ladder
    /// starts there so the top of the table is a result already believed. Steps are no larger than
    /// 2 dB, <b>-21 is on the ladder deliberately</b> because it is the ratio step 6 will eventually
    /// be judged at, and the bottom runs past -24 so the collapse is <em>bracketed</em> rather than
    /// merely approached from above.
    /// </remarks>
    internal static readonly double[] Rungs =
    {
        -10.0, -12.0, -14.0, -16.0, -18.0, -20.0, -21.0, -22.0, -24.0, -26.0,
    };

    /// <summary>The seeds every ladder draws its noise from. Two, so a rung is not one draw.</summary>
    internal static readonly int[] Seeds = { 218_001, 218_002 };

    /// <summary>The waterfall's tone spacing, for placing signals off a bin centre.</summary>
    internal const double BinHz = 6.25;

    /// <summary>One rung's result: what was asked for, what was delivered, and what came back.</summary>
    internal sealed class Rung(double requested)
    {
        private readonly List<double> _delivered = new();
        private readonly List<int> _agreements = new();

        internal double Requested { get; } = requested;

        internal int Offered { get; private set; }

        internal int Returned { get; private set; }

        internal int Wrong { get; private set; }

        internal long Candidates { get; private set; }

        internal long Parity { get; private set; }

        internal long Checksum { get; private set; }

        internal long Text { get; private set; }

        /// <summary>The mean ratio actually put on the samples, which is what the row is binned by.</summary>
        internal double DeliveredMean => _delivered.Count == 0 ? double.NaN : _delivered.Average();

        internal double DeliveredWorst => _delivered.Count == 0 ? double.NaN : _delivered.Min();

        internal double DeliveredBest => _delivered.Count == 0 ? double.NaN : _delivered.Max();

        /// <summary>
        /// Hard-decision agreement with the true codeword at the best candidate, out of 174, over
        /// every trial at this rung — <b>the figure that gives unit 217's on-air histogram a
        /// decibel value.</b>
        /// </summary>
        internal IReadOnlyList<int> Agreements => _agreements;

        internal double MeanAgreement => _agreements.Count == 0 ? double.NaN : _agreements.Average();

        /// <summary>Agreement over the trials at this rung that did <em>not</em> come back.</summary>
        internal List<int> MissAgreements { get; } = new();

        internal double Rate => Offered == 0 ? 0 : 100.0 * Returned / Offered;

        internal void Add(Ft8SlotResult result, double deliveredDecibels, bool returned, int wrong)
        {
            Offered++;
            _delivered.Add(deliveredDecibels);
            Candidates += result.CandidateCount;
            Parity += result.ParitySatisfiedCount;
            Checksum += result.ChecksumPassedCount;
            Text += result.BecameTextCount;

            if (returned)
            {
                Returned++;
            }

            Wrong += wrong;
        }

        internal void AddAgreement(int agreement, bool returned)
        {
            _agreements.Add(agreement);
            if (!returned)
            {
                MissAgreements.Add(agreement);
            }
        }

        internal string Row() =>
            $"{Requested,9:F1} {DeliveredMean,10:F3} {Offered,8} {Returned,9} {Rate,7:F1} "
            + $"{Wrong,7} {Candidates / (double)Math.Max(Offered, 1),7:F1} "
            + $"{Parity / (double)Math.Max(Offered, 1),7:F1} "
            + $"{Checksum / (double)Math.Max(Offered, 1),7:F1} "
            + $"{Text / (double)Math.Max(Offered, 1),7:F1}";
    }

    /// <summary>The header the rows below it line up under.</summary>
    internal const string Header =
        "requested  delivered  offered  returned    rate   WRONG   cand     par     crc     txt";

    /// <summary>
    /// The messages every ladder in this unit offers: the corpus filtered exactly the way
    /// <c>TheCorpusComesBackInSeededNoiseAtAMeasuredRatio</c> filters it, then thinned to keep the
    /// run under its budget while staying spread across the corpus's message kinds.
    /// </summary>
    /// <remarks>
    /// <b>Thinned by taking every second entry, not by taking the first N.</b> The corpus is written
    /// in blocks by kind — CQ, standard, free text, telemetry, non-standard — so the first N would be
    /// one kind and the ladder would measure that kind's sensitivity rather than the path's.
    /// </remarks>
    internal static IReadOnlyList<EncodeCorpus.Entry> Messages()
    {
        var corpus = EncodeCorpus.Build().Where(e => !e.CarriesHashedCallsign).ToArray();
        return corpus.Where((_, i) => i % 2 == 0).ToArray();
    }

    /// <summary>
    /// The 174-bit codeword the entry's own 77 bits encode to. <b>The truth, exactly, because this
    /// fixture generated the signal</b> — no packer, no expected list, no third party's text.
    /// </summary>
    internal static byte[] TrueCodeword(EncodeCorpus.Entry entry)
    {
        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(entry.Message, codeword);
        return codeword;
    }

    /// <summary>
    /// Hard-decision agreement out of 174 between the ratios extracted at <paramref name="candidate"/>
    /// and the true codeword. -1 where there was no candidate to extract at.
    /// </summary>
    internal static int AgreementAt(Ft8Waterfall waterfall, Ft8Candidate? candidate, byte[] codeword)
    {
        if (candidate is null)
        {
            return -1;
        }

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        Ft8SoftSymbols.Extract(waterfall, candidate.Value, ratios);
        Ft8SoftSymbols.Normalise(ratios);

        var decisions = new byte[Ft8SoftSymbols.RatioCount];
        Ft8SoftSymbols.HardDecision(ratios, decisions);

        var agree = 0;
        for (var bit = 0; bit < decisions.Length; bit++)
        {
            var truth = (codeword[bit / 8] >> (7 - (bit % 8))) & 1;
            if (decisions[bit] == truth)
            {
                agree++;
            }
        }

        return agree;
    }
}
