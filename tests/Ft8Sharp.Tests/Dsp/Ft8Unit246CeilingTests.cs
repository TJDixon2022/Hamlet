using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The ceiling: how close the sync search's best candidate actually gets to the codeword that was
/// transmitted, at the rung the phase is about.</b> Unit 246 task 1, and it is a measurement rather
/// than an algorithm.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is measured before a line of ordered statistics decoding is written.</b> An OSD of
/// order λ re-encodes from the most reliable 91 positions of a candidate's hard decision, so it can
/// only reach the transmitted codeword when <em>at most λ of those 91 positions are wrong</em>.
/// Errors outside the basis cost nothing — re-encoding overwrites them. So the number that decides
/// whether OSD can help at all is not the total hard-decision error count that unit 222 measured at
/// about 31; it is <b>how many of those errors fall inside the most reliable 91</b>. This test
/// measures that, per trial, over one whole 51-message block at -21 dB.
/// </para>
/// <para>
/// <b>And it measures whether the search even returns a place near the signal.</b> If the closest
/// candidate the search returns is no better than a coin toss against the true codeword, no amount of
/// code searching helps and the missing decibel is in synchronisation instead. That is a finding
/// about which step to take next, and it is worth more to the phase than a working order-1 search.
/// </para>
/// <para>
/// <b>Nothing under <c>src/Ft8Sharp/</c> is touched.</b> The port is the instrument: every number
/// here comes out of its own public members — <see cref="Ft8SyncSearch.Find(Ft8Waterfall)"/>,
/// <see cref="Ft8SoftSymbols.Extract"/>, <see cref="Ft8SoftSymbols.Normalise"/>,
/// <see cref="Ft8SoftSymbols.HardDecision"/> and <see cref="LdpcEncoder.Encode(ReadOnlySpan{byte}, Span{byte})"/>.
/// </para>
/// </remarks>
public class Ft8Unit246CeilingTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>The rung the phase is about: <c>HM-OPEN-067</c>'s 13 of 306 sits here.</summary>
    private const double Rung = -21.0;

    /// <summary>Codeword bits, 174.</summary>
    private const int N = LdpcDecoder.CodewordBits;

    /// <summary>Systematic bits, 91: the message and its checksum, which is also the basis size.</summary>
    private const int K = LdpcEncoder.PayloadBits;

    /// <summary>
    /// <b>The distance at which a candidate is called "near the signal", and the arithmetic behind
    /// it.</b> A candidate unrelated to the transmission has a hard-decision distance drawn from
    /// Binomial(174, 0.5): mean 87, standard deviation 6.6. 60 is 4.1 standard deviations below that
    /// mean, so the chance of an unrelated candidate reaching it is about 2 in 100000. One block is
    /// 51 trials of at most 140 candidates, about 7000 draws, so fewer than one false "near" is
    /// expected across the whole run. <b>It is a threshold on noise, not a threshold on success</b> —
    /// a candidate at distance 55 is nowhere near decodable and is still plainly the signal.
    /// </summary>
    private const int NearThreshold = 60;

    /// <summary>
    /// <b>Task 1.1 — the generator read off the port's own encoder, and checked.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The code is systematic in its first 91 bits: <c>Ft8CodewordDecoder</c> packs
    /// <c>codewordBits[..91]</c> straight as the payload. So row <c>i</c> of the 91 by 174 generator
    /// is what <see cref="LdpcEncoder.Encode(ReadOnlySpan{byte}, Span{byte})"/> returns for the
    /// payload with bit <c>i</c> set and every other bit clear. <b>That is read off the encoder rather
    /// than unpacked out of <c>Ft8Tables.LdpcGenerator</c>, whose rows are the 83 parity checks in
    /// upstream's own packing</b>, and a mistake in that packing would poison every number this unit
    /// produces while looking exactly like an algorithm that does not work.
    /// </para>
    /// <para>
    /// <b>The check is the whole point of the task.</b> Encoding is linear over GF(2), so the codeword
    /// of any payload must be the exclusive-or of the rows its set bits select. This asserts that for
    /// several hundred random payloads, bit for bit, against the port's own encoder.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheGeneratorReadOffTheEncoderReproducesEveryEncodeItIsCheckedAgainst()
    {
        var generator = GeneratorRows();

        Assert.Equal(K, generator.Length);
        foreach (var row in generator)
        {
            Assert.Equal(N, row.Length);
        }

        // Row i must be systematic at column i and clear at every other systematic column: that is
        // what "systematic in its first 91 bits" means, and it is checked rather than assumed.
        for (var i = 0; i < K; i++)
        {
            for (var j = 0; j < K; j++)
            {
                Assert.True(
                    generator[i][j] == (i == j ? 1 : 0),
                    $"generator row {i} column {j} is {generator[i][j]}, so the code is not systematic "
                        + "in its first 91 bits and every number in this unit would be measured against "
                        + "the wrong codeword.");
            }
        }

        var random = new Random(246246);
        var payload = new byte[LdpcEncoder.PayloadBytes];
        var fromPort = new byte[LdpcEncoder.CodewordBytes];
        var bits = new byte[K];
        var mine = new byte[N];
        const int trials = 500;

        for (var t = 0; t < trials; t++)
        {
            for (var i = 0; i < K; i++)
            {
                bits[i] = (byte)random.Next(2);
            }

            Pack(bits, payload);
            LdpcEncoder.Encode(payload, fromPort);
            var expected = Unpack(fromPort, N);

            Array.Clear(mine);
            for (var i = 0; i < K; i++)
            {
                if (bits[i] == 0)
                {
                    continue;
                }

                var row = generator[i];
                for (var j = 0; j < N; j++)
                {
                    mine[j] ^= row[j];
                }
            }

            for (var j = 0; j < N; j++)
            {
                Assert.True(
                    mine[j] == expected[j],
                    $"trial {t}, codeword bit {j}: the generator read off the encoder gives "
                        + $"{mine[j]} and LdpcEncoder.Encode gives {expected[j]}.");
            }
        }

        output.WriteLine(
            $"G is {K} rows by {N} columns, each row LdpcEncoder.Encode of a unit payload.");
        output.WriteLine(
            $"Systematic check: rows 0..{K - 1} form the identity on columns 0..{K - 1}. PASSED.");
        output.WriteLine(
            $"Linearity check: {trials} random payloads, {trials * N} codeword bits compared against "
            + "LdpcEncoder.Encode. EVERY BIT AGREED.");
        output.WriteLine(string.Empty);
        output.WriteLine(
            "So the 91 by 174 generator this unit's OSD works from is the port's own encoder, and it "
            + "was verified rather than assumed.");
    }

    /// <summary>
    /// <b>Tasks 1.2 and 1.3 — the population OSD gets to work on, and the ceiling above it.</b> One
    /// whole block of 51 trials at -21 dB, the harness's own seed, frequency and offset.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>1.2 uses the port's five counts and instruments nothing.</b> Candidates returned, how many
    /// reached parity, how many passed the checksum, how many became text: those are
    /// <see cref="Ft8SlotResult"/>'s own fields, summed over the block.
    /// </para>
    /// <para>
    /// <b>1.3 is the finding.</b> For each trial the true 174-bit codeword is known —
    /// <c>EncodeCorpus.Entry.Message</c> is the 77 bits that went on the wire,
    /// <see cref="Ft8Payload.Create"/> adds the checksum and
    /// <see cref="LdpcEncoder.Encode(ReadOnlySpan{byte}, Span{byte})"/> gives the codeword. Every
    /// candidate the search returned is extracted and normalised exactly as
    /// <c>Ft8SlotDecoder.Decode</c> does it, hard-decided, and compared. The smallest distance is the
    /// ceiling; the errors of that closest candidate that fall inside its 91 most reliable positions
    /// are what an OSD of order λ would have to cover.
    /// </para>
    /// <para>
    /// <b>The 91 most reliable positions are not exactly the most reliable basis</b>, which is the
    /// first 91 <em>independent</em> columns in reliability order. They are the same set whenever the
    /// leading 91 columns happen to be independent, and the true basis otherwise reaches slightly
    /// further down the ordering. So this count is a lower bound on the basis error count and the
    /// distribution below reads as the best case for OSD. It is reported as what it is.
    /// </para>
    /// <para>
    /// <b>Nothing here asserts a rate or a bound.</b> Two things are asserted: that the block ran, and
    /// that the search returned candidates at all. Everything else is printed.
    /// </para>
    /// </remarks>
    [Fact]
    public void AtMinus21DbTheClosestCandidateToTheTransmittedCodewordIsMeasuredOverOneWholeBlock()
    {
        var population = Ft8Step6Ladder.Population();
        var offset = Ft8LadderHarness.DefaultOffsetSamples;
        var blockSeed = Ft8LadderHarness.DefaultSeed + (int)Math.Round(Rung * 10.0);
        var noise = new GaussianNoise(blockSeed);

        var port = new Ft8SlotDecoder();
        var monitor = new Ft8Monitor(port.Geometry);
        var search = new Ft8SyncSearch();

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var hard = new byte[N];

        var candidates = 0L;
        var parity = 0L;
        var checksum = 0L;
        var text = 0L;
        var duplicates = 0L;
        var decoded = 0;

        var closest = new List<int>(population.Count);
        var inBasis = new List<int>(population.Count);
        var withNoCandidateNear = new List<string>();
        var emptySearches = 0;

        var clock = Stopwatch.StartNew();
        var trial = 0;

        foreach (var entry in population)
        {
            var (clean, _) = SearchFixture.OneSignal(
                Rate, entry, Ft8LadderHarness.DefaultFrequencyHz, offset);
            var signalPower = SearchFixture.TransmissionPower(
                Rate, entry, Ft8LadderHarness.DefaultFrequencyHz);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, Rung, Rate);
            var mixed = SearchFixture.AddNoise(clean, noise, sigma, out _);

            // 1.2 -- the port's own five counts, taken from the port's own decode of the same audio.
            var result = port.Decode(mixed);
            candidates += result.CandidateCount;
            parity += result.ParitySatisfiedCount;
            checksum += result.ChecksumPassedCount;
            text += result.BecameTextCount;
            duplicates += result.DuplicateCount;

            var sent = Ft8MessageDecoder.Decode(entry.Message).Text;
            if (result.Texts.Contains(sent, StringComparer.Ordinal))
            {
                decoded++;
            }

            // 1.3 -- the ceiling. The truth, then every candidate against it.
            var truth = TrueCodeword(entry);

            var waterfall = monitor.Analyse(mixed);
            var found = search.Find(waterfall);
            if (found.Count == 0)
            {
                emptySearches++;
            }

            var best = int.MaxValue;
            var bestBasisErrors = -1;

            foreach (var candidate in found)
            {
                Ft8SoftSymbols.Extract(waterfall, candidate, ratios);
                Ft8SoftSymbols.Normalise(ratios);
                Ft8SoftSymbols.HardDecision(ratios, hard);

                var distance = 0;
                for (var i = 0; i < N; i++)
                {
                    if (hard[i] != truth[i])
                    {
                        distance++;
                    }
                }

                if (distance >= best)
                {
                    continue;
                }

                best = distance;
                bestBasisErrors = ErrorsInTheMostReliable(ratios, hard, truth);
            }

            if (found.Count == 0)
            {
                best = N;
                bestBasisErrors = K;
            }

            closest.Add(best);
            inBasis.Add(bestBasisErrors);

            if (best > NearThreshold)
            {
                withNoCandidateNear.Add($"trial {trial,2} {entry.Label}: closest {best} of {N}");
            }

            trial++;
        }

        clock.Stop();

        output.WriteLine($"UNIT 246 TASK 1 - THE CEILING AT {Rung:F1} dB");
        output.WriteLine(
            $"one whole block of {population.Count} trials, seed {blockSeed}, "
            + $"{Ft8LadderHarness.DefaultFrequencyHz:F0} Hz, offset {offset} samples");
        output.WriteLine($"wall clock {clock.Elapsed.TotalSeconds:F1} s");
        output.WriteLine(string.Empty);

        output.WriteLine("1.2  THE POPULATION, from Ft8SlotResult's own five counts:");
        output.WriteLine($"  candidates returned by the search   {candidates}");
        output.WriteLine($"  of those, reached parity            {parity}");
        output.WriteLine($"  of those, passed the checksum       {checksum}");
        output.WriteLine($"  of those, became text               {text}");
        output.WriteLine($"  of those, were duplicates           {duplicates}");
        output.WriteLine($"  trials whose own message came back  {decoded} of {population.Count}");
        output.WriteLine($"  searches that returned nothing      {emptySearches}");
        output.WriteLine(string.Empty);

        output.WriteLine("1.3  THE CEILING - closest candidate's hard-decision distance to the");
        output.WriteLine("     TRANSMITTED codeword, and how many of those errors are inside the 91");
        output.WriteLine("     most reliable positions.");
        output.WriteLine(string.Empty);
        output.WriteLine("  closest distance of 174, all 51 trials, sorted:");
        output.WriteLine("    " + string.Join(" ", closest.OrderBy(d => d)));
        output.WriteLine(Histogram("  closest distance", closest, [10, 17, 25, 35, 45, 60, 80, N]));
        output.WriteLine(string.Empty);
        output.WriteLine("  of those errors, how many fall in the 91 most reliable positions, sorted:");
        output.WriteLine("    " + string.Join(" ", inBasis.OrderBy(d => d)));
        output.WriteLine(Histogram("  errors in the most reliable 91", inBasis, [0, 1, 2, 3, 5, 10, 20, K]));
        output.WriteLine(string.Empty);

        for (var order = 0; order <= 4; order++)
        {
            var reachable = inBasis.Count(e => e >= 0 && e <= order);
            output.WriteLine(
                $"  order {order}: {reachable} of {population.Count} trials have a candidate whose "
                + $"basis carries at most {order} error(s)");
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            $"  trials with NO candidate within {NearThreshold} of the truth: "
            + $"{withNoCandidateNear.Count} of {population.Count}");
        foreach (var line in withNoCandidateNear)
        {
            output.WriteLine("    " + line);
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            "READ IT THIS WAY. An OSD of order lambda reaches the transmitted codeword only when the");
        output.WriteLine(
            "most reliable basis carries at most lambda errors. The second distribution is therefore");
        output.WriteLine(
            "the ceiling on this approach, and it is a LOWER bound on the basis error count because");
        output.WriteLine(
            "the true basis is the first 91 INDEPENDENT columns in reliability order, which reaches");
        output.WriteLine("at least as far down the ordering as the leading 91 do.");

        Assert.Equal(population.Count, trial);
        Assert.True(
            candidates > 0,
            "the search returned no candidates anywhere in the block, so there was nothing to measure "
                + "a ceiling over and this run is not evidence about OSD.");
    }

    /// <summary>The 174-bit codeword an entry's 77 bits actually put on the wire.</summary>
    private static byte[] TrueCodeword(EncodeCorpus.Entry entry)
    {
        var payload = new byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(entry.Message, payload);

        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);

        return Unpack(codeword, N);
    }

    /// <summary>
    /// How many of a hard decision's errors fall in the 91 positions with the largest
    /// <c>|ratio|</c>. Ties break on index, so the answer does not depend on a sort's stability.
    /// </summary>
    private static int ErrorsInTheMostReliable(
        ReadOnlySpan<float> ratios, ReadOnlySpan<byte> hard, ReadOnlySpan<byte> truth)
    {
        var order = new int[N];
        var magnitude = new float[N];
        for (var i = 0; i < N; i++)
        {
            order[i] = i;
            magnitude[i] = Math.Abs(ratios[i]);
        }

        var byReliability = order
            .OrderByDescending(i => magnitude[i])
            .ThenBy(i => i)
            .ToArray();

        var errors = 0;
        for (var r = 0; r < K; r++)
        {
            if (hard[byReliability[r]] != truth[byReliability[r]])
            {
                errors++;
            }
        }

        return errors;
    }

    /// <summary>The 91 rows of the generator, read off the port's encoder.</summary>
    private static byte[][] GeneratorRows()
    {
        var rows = new byte[K][];
        var payload = new byte[LdpcEncoder.PayloadBytes];
        var codeword = new byte[LdpcEncoder.CodewordBytes];

        for (var i = 0; i < K; i++)
        {
            Array.Clear(payload);
            payload[i / 8] = (byte)(0x80u >> (i % 8));
            LdpcEncoder.Encode(payload, codeword);
            rows[i] = Unpack(codeword, N);
        }

        return rows;
    }

    /// <summary>One byte per bit out of a packed buffer, most significant bit of each byte first.</summary>
    private static byte[] Unpack(ReadOnlySpan<byte> packed, int count)
    {
        var bits = new byte[count];
        for (var i = 0; i < count; i++)
        {
            bits[i] = (byte)((packed[i / 8] >> (7 - (i % 8))) & 1);
        }

        return bits;
    }

    /// <summary>One bit per byte into a packed buffer, most significant bit of each byte first.</summary>
    private static void Pack(ReadOnlySpan<byte> bits, Span<byte> packed)
    {
        packed.Clear();
        for (var i = 0; i < bits.Length; i++)
        {
            if (bits[i] != 0)
            {
                packed[i / 8] |= (byte)(0x80u >> (i % 8));
            }
        }
    }

    /// <summary>A cumulative histogram, printed as "at or below n: c".</summary>
    private static string Histogram(string what, IReadOnlyList<int> values, int[] bounds)
    {
        var lines = new List<string> { $"{what}, cumulative:" };
        foreach (var bound in bounds)
        {
            lines.Add($"    at or below {bound,3}: {values.Count(v => v <= bound),3} of {values.Count}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
