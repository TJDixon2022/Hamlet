using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
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
    /// <b>Walks the whole path — samples to text — down the ladder and reports what came back.</b>
    /// Where a transmission is put is the caller's, so the aligned ladder and the impaired ones are
    /// the same experiment with one thing changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>A rung that returns nothing is a MEASUREMENT and not a failure.</b> Nothing in here throws
    /// on a poor result; the caller asserts only what must always be true. A test that threw at
    /// -22 dB would destroy the measurement it was written to take.
    /// </para>
    /// <para>
    /// <b>Nothing is told to the decode path.</b> The frequency and the offset are chosen here and
    /// handed to the <em>synthesizer</em>; <see cref="Ft8SlotDecoder"/> and
    /// <see cref="Ft8SyncSearch"/> are given the samples and the geometry and nothing else. The truth
    /// is used twice and both times after the code has answered: to compare the text, and to pick
    /// which candidate the agreement figure is read at.
    /// </para>
    /// </remarks>
    /// <param name="messages">The transmissions offered at every rung.</param>
    /// <param name="frequencyFor">The base frequency for message <c>i</c>, in hertz.</param>
    /// <param name="offsetFor">Where message <c>i</c>'s first sample is written in the slot.</param>
    /// <param name="measureAgreement">
    /// Whether to read the hard-decision agreement at the candidate nearest the truth. Costs a second
    /// pass of the search per trial, so the impaired ladders leave it off.
    /// </param>
    /// <param name="log">Where the per-trial line goes, or null for silence.</param>
    internal static IReadOnlyList<Rung> Walk(
        IReadOnlyList<EncodeCorpus.Entry> messages,
        Func<int, double> frequencyFor,
        Func<int, int> offsetFor,
        bool measureAgreement,
        Action<string>? log = null)
    {
        const int rate = Ft8WaterfallGeometry.DefaultSampleRate;

        var decoder = new Ft8SlotDecoder();
        var search = new Ft8SyncSearch();
        var geometry = decoder.Geometry;
        var rungs = new List<Rung>();

        foreach (var requested in Rungs)
        {
            var rung = new Rung(requested);

            foreach (var seed in Seeds)
            {
                var noise = new GaussianNoise(seed + (int)Math.Round(requested * 10));

                for (var i = 0; i < messages.Count; i++)
                {
                    var entry = messages[i];
                    var frequency = frequencyFor(i);
                    var offset = offsetFor(i);

                    var (clean, _) = SearchFixture.OneSignal(rate, entry, frequency, offset);
                    var signalPower = SearchFixture.TransmissionPower(rate, entry, frequency);
                    var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, requested, rate);
                    var mixed = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
                    var delivered = SignalToNoise.DecibelsFor(signalPower, noisePower, rate);

                    var waterfall = new Ft8Monitor(geometry).Analyse(mixed);
                    var result = decoder.Decode(waterfall);

                    var expected = Ft8MessageDecoder.Decode(entry.Message).Text;
                    var returned = result.Texts.Contains(expected, StringComparer.Ordinal);
                    var wrong = result.Texts.Count(t => !string.Equals(t, expected, StringComparison.Ordinal));

                    rung.Add(result, delivered, returned, wrong);

                    if (measureAgreement)
                    {
                        var candidates = search.Find(waterfall);
                        var nearest = NearestTo(candidates, geometry, frequency);
                        var agreement = AgreementAt(waterfall, nearest, TrueCodeword(entry));
                        if (agreement >= 0)
                        {
                            rung.AddAgreement(agreement, returned);
                        }
                    }

                    log?.Invoke($"    rung {requested,6:F1} dB seed {seed} message {i + 1,3} of "
                        + $"{messages.Count}: {(returned ? "back" : "MISSED")}"
                        + $"{(wrong > 0 ? $" +{wrong} WRONG" : string.Empty)}");
                }
            }

            rungs.Add(rung);
        }

        return rungs;
    }

    /// <summary>
    /// The kept candidate closest in frequency to where the fixture actually put the transmission,
    /// within four hertz — <b>unit 217's rule, kept so the two agreement figures are comparable.</b>
    /// Null where the search found nothing within four hertz of it.
    /// </summary>
    internal static Ft8Candidate? NearestTo(
        IReadOnlyList<Ft8Candidate> candidates, Ft8WaterfallGeometry geometry, double hz)
    {
        Ft8Candidate? best = null;
        var bestDistance = double.MaxValue;

        foreach (var candidate in candidates)
        {
            var distance = Math.Abs(candidate.FrequencyHz(geometry) - hz);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return bestDistance <= 4.0 ? best : null;
    }

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
        // 77 bits, then the checksum makes 91, then the parity makes 174 — the same three steps
        // Ft8SymbolEncoder takes, so this is the codeword that was actually on the air.
        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(entry.Message, payload);

        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);
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
