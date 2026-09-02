using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// The three things step 6 will lean on: that the decoder gives the same answer twice, that the
/// sign convention is not something it is blind to, and that every refusal has been watched
/// refusing.
/// </summary>
/// <remarks>
/// <b>Every comparison here is on the values and never on a count.</b> Two runs agreeing on
/// <em>how many</em> decoded while disagreeing on <em>which</em> is exactly the failure a count
/// hides, and it is the failure a sensitivity sweep would then average away without anybody
/// seeing it.
/// </remarks>
public class Ft8LdpcDecoderDeterminismTests
{
    private readonly ITestOutputHelper _output;

    public Ft8LdpcDecoderDeterminismTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Enough damage that the decoder is doing real work and the iteration counts differ from
    /// message to message. A clean codeword would agree with itself trivially in one iteration
    /// and would prove nothing about the state carried between calls.
    /// </summary>
    private const int FlipsPerMessage = 10;

    private const int DamageSeed = 21_561;

    private sealed record Answer(byte[] Bits, int UnsatisfiedChecks, int Iterations);

    /// <summary>
    /// The same ratios give the same bits, the same iteration count and the same
    /// unsatisfied-check count -- twice over, in reverse, and in a seeded shuffle.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The reversed and shuffled runs are the ones that could actually fail.</b> Calling the
    /// same input twice in a row catches almost nothing; calling every input in a different order
    /// catches state carried from one call into the next, which is the only way a static decoder
    /// could be non-deterministic at all. <see cref="LdpcDecoder"/> has no instance to reuse --
    /// it is static and allocates both message arrays per call -- so "a reused instance" has no
    /// subject here and the order comparisons stand in its place.
    /// </para>
    /// <para>
    /// The comparison is <b>174 bit positions plus two counts, per message, per ordering</b>, and
    /// every one of them is asserted individually.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheSameRatiosGiveTheSameAnswerTwiceInReverseAndInAShuffledOrder()
    {
        var corpus = EncodeCorpus.Build();

        // The damage is drawn once and held, so every ordering below decodes byte-identical
        // inputs. A generator consumed inside the loop would make the reversed run a different
        // experiment rather than the same one.
        var random = new Random(DamageSeed);
        var inputs = corpus
            .Select(entry =>
            {
                var ratios = SoftCodeword.RatiosFor(SoftCodeword.CodewordBitsFor(entry.Message));
                SoftCodeword.FlipDistinctPositions(ratios, FlipsPerMessage, random);
                return ratios;
            })
            .ToArray();

        var forward = new Answer[inputs.Length];
        for (var i = 0; i < inputs.Length; i++)
        {
            forward[i] = DecodeOne(inputs[i]);
        }

        var again = new Answer[inputs.Length];
        for (var i = 0; i < inputs.Length; i++)
        {
            again[i] = DecodeOne(inputs[i]);
        }

        var reversed = new Answer[inputs.Length];
        for (var i = inputs.Length - 1; i >= 0; i--)
        {
            reversed[i] = DecodeOne(inputs[i]);
        }

        var order = Enumerable.Range(0, inputs.Length).ToArray();
        var shuffler = new Random(DamageSeed + 1);
        for (var i = order.Length - 1; i > 0; i--)
        {
            var j = shuffler.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        var shuffled = new Answer[inputs.Length];
        foreach (var i in order)
        {
            shuffled[i] = DecodeOne(inputs[i]);
        }

        var comparisons = 0;
        var recovered = forward.Count(a => a.UnsatisfiedChecks == 0);

        foreach (var (name, other) in new[] { ("run twice", again), ("reversed", reversed), ("shuffled", shuffled) })
        {
            for (var i = 0; i < inputs.Length; i++)
            {
                for (var bit = 0; bit < Ft8Tables.LdpcN; bit++)
                {
                    Assert.True(
                        forward[i].Bits[bit] == other[i].Bits[bit],
                        $"the {name} run disagreed with the forward run about message {i}, bit {bit}.");
                    comparisons++;
                }

                Assert.True(
                    forward[i].UnsatisfiedChecks == other[i].UnsatisfiedChecks,
                    $"the {name} run disagreed about message {i}'s unsatisfied-check count: "
                    + $"{forward[i].UnsatisfiedChecks} against {other[i].UnsatisfiedChecks}.");
                Assert.True(
                    forward[i].Iterations == other[i].Iterations,
                    $"the {name} run disagreed about message {i}'s iteration count: "
                    + $"{forward[i].Iterations} against {other[i].Iterations}.");
                comparisons += 2;
            }
        }

        _output.WriteLine($"messages                        : {inputs.Length}, each damaged by "
            + $"{FlipsPerMessage} bit flips at seed {DamageSeed}");
        _output.WriteLine($"orderings compared              : forward, again, reversed, shuffled");
        _output.WriteLine($"VALUE comparisons, all equal    : {comparisons}");
        _output.WriteLine($"  and they are not all the same answer, which is what makes the comparison bite:");
        _output.WriteLine($"  messages recovering           : {recovered} of {inputs.Length}");
        _output.WriteLine($"  distinct iteration counts seen: "
            + $"{string.Join(", ", forward.Select(a => a.Iterations).Distinct().OrderBy(v => v))}");
        _output.WriteLine($"  distinct unsatisfied counts   : "
            + $"{string.Join(", ", forward.Select(a => a.UnsatisfiedChecks).Distinct().OrderBy(v => v))}");
    }

    /// <summary>
    /// <b>THE UNIT'S MOST IMPORTANT REFUSAL.</b> A clean codeword decoded with every ratio
    /// negated -- the sign convention backwards -- and what happens is reported rather than
    /// assumed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Why this exists.</b> A round trip through this project's own test helpers proves
    /// nothing about the convention: the helper and the decoder would agree with each other just
    /// as perfectly if both were backwards, every test written tonight would pass, and the
    /// library would be stone deaf on the first real signal, because the next unit's extraction
    /// will follow upstream's convention and not this project's. <b>So the convention's evidence
    /// is the reading in <c>UpstreamLdpcDecoderInventoryTests</c>, and this is the refusal that
    /// shows the decoder can tell the difference.</b>
    /// </para>
    /// <para>
    /// <b>And there is a reason to expect it to tell the difference, which is worth writing down
    /// because it makes the result predictable rather than lucky.</b> Negating every ratio
    /// complements the hard decision, and the complement of a codeword <c>c</c> is
    /// <c>c ⊕ 1</c>, where <c>1</c> is the all-ones word. The all-ones word satisfies a parity
    /// check exactly when that check has even degree -- and this code's checks do not all have
    /// the same degree. 59 of the 83 cover six variables and 24 cover seven, so an inverted
    /// codeword fails <em>precisely the 24 odd-degree checks</em>, which is four times the six
    /// bit errors every trial recovered from in task 4. <b>If this code were regular, an inverted
    /// codeword would be a perfect codeword and the decoder would be convention-blind.</b> It is
    /// not, and this test prints the count so the reasoning can be checked against the
    /// measurement.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheInvertedSignConventionIsWatchedRefusing()
    {
        var corpus = EncodeCorpus.Build();

        var oddDegreeChecks = 0;
        for (var m = 0; m < Ft8Tables.LdpcM; m++)
        {
            if (Ft8Tables.LdpcNumRows[m] % 2 == 1)
            {
                oddDegreeChecks++;
            }
        }

        var upright = 0;
        var invertedDecoded = 0;
        var invertedWrongMessage = 0;
        var invertedRefused = 0;
        var unsatisfiedAtFirstDecision = new List<int>();

        foreach (var entry in corpus)
        {
            var bits = SoftCodeword.CodewordBitsFor(entry.Message);
            var ratios = SoftCodeword.RatiosFor(bits);

            if (Ft8CodewordDecoder.Decode(ratios).Correction.ParitySatisfied)
            {
                upright++;
            }

            var inverted = ratios.Select(r => -r).ToArray();

            // What the very first hard decision costs, before belief propagation gets a chance
            // to move anything: one iteration is the raw decision on the ratios as given.
            var firstPass = new byte[Ft8Tables.LdpcN];
            var first = LdpcDecoder.Decode(inverted, firstPass, maxIterations: 1);
            unsatisfiedAtFirstDecision.Add(first.UnsatisfiedChecks);

            var result = Ft8CodewordDecoder.Decode(inverted);
            if (result.Decoded || result.Message.Text.Length > 0)
            {
                var recovered = result.Message.Text == Ft8MessageTextOf(entry.Message);
                if (recovered)
                {
                    invertedDecoded++;
                }
                else
                {
                    invertedWrongMessage++;
                }
            }
            else
            {
                invertedRefused++;
            }
        }

        _output.WriteLine($"check degrees: {Ft8Tables.LdpcM - oddDegreeChecks} of "
            + $"{Ft8Tables.LdpcM} even, {oddDegreeChecks} odd");
        _output.WriteLine($"  -- so an inverted codeword is predicted to fail exactly the "
            + $"{oddDegreeChecks} odd-degree checks");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"corpus messages                                   : {corpus.Count}");
        _output.WriteLine($"upright, parity satisfied                         : {upright}");
        _output.WriteLine($"inverted, unsatisfied checks at the FIRST decision: "
            + $"{string.Join(", ", unsatisfiedAtFirstDecision.Distinct().OrderBy(v => v))}  "
            + "<-- measured, against the prediction above");
        _output.WriteLine(string.Empty);
        _output.WriteLine("WITH EVERY RATIO NEGATED:");
        _output.WriteLine($"  returned the CORRECT message : {invertedDecoded}");
        _output.WriteLine($"  returned a WRONG message     : {invertedWrongMessage}");
        _output.WriteLine($"  returned NOTHING             : {invertedRefused} of {corpus.Count}");

        Assert.Equal(corpus.Count, upright);
        Assert.All(unsatisfiedAtFirstDecision, count => Assert.Equal(oddDegreeChecks, count));
        Assert.Equal(0, invertedDecoded);
        Assert.Equal(0, invertedWrongMessage);
        Assert.Equal(corpus.Count, invertedRefused);
    }

    /// <summary>
    /// Every refusal the decoder and the gate define, in one table, with what was handed in and
    /// how far it fell short.
    /// </summary>
    /// <remarks>
    /// The individual refusals are exercised in <see cref="Ft8LdpcDecoderRefusalTests"/>; this
    /// gathers them so the report can quote one block instead of eight. <b>"By how much it
    /// missed" is meaningful only for the bounds</b> -- a wrong array length misses by a length
    /// and the table says so, where a set of ratios misses by a number of unsatisfied checks.
    /// </remarks>
    [Fact]
    public void EveryRefusalInOneTableWithHowFarEachMissed()
    {
        var entry = EncodeCorpus.Build()[0];
        var clean = SoftCodeword.RatiosFor(SoftCodeword.CodewordBitsFor(entry.Message));
        var rows = new List<string>();

        void Shape(string what, Action call)
        {
            try
            {
                call();
                rows.Add($"{what,-46} | DID NOT REFUSE");
                Assert.Fail($"{what} did not refuse.");
            }
            catch (ArgumentException error)
            {
                rows.Add($"{what,-46} | refused: {error.Message.Split('.')[0]}");
            }
        }

        void Bound(string what, float[] ratios)
        {
            var bits = new byte[Ft8Tables.LdpcN];
            var result = LdpcDecoder.Decode(ratios, bits);
            var gated = Ft8CodewordDecoder.Decode(ratios);
            rows.Add($"{what,-46} | {gated.Status}, missed by {result.UnsatisfiedChecks} of "
                + $"{Ft8Tables.LdpcM} checks");
            Assert.False(gated.Decoded);
        }

        Shape("ratios: 173 long", () => LdpcDecoder.Decode(new float[173], new byte[Ft8Tables.LdpcN]));
        Shape("ratios: 175 long", () => LdpcDecoder.Decode(new float[175], new byte[Ft8Tables.LdpcN]));
        Shape("ratios: empty", () => LdpcDecoder.Decode(Array.Empty<float>(), new byte[Ft8Tables.LdpcN]));
        Shape("output buffer: 173 long", () => LdpcDecoder.Decode(new float[Ft8Tables.LdpcN], new byte[173]));
        Shape(
            "maxIterations: -1",
            () => LdpcDecoder.Decode(new float[Ft8Tables.LdpcN], new byte[Ft8Tables.LdpcN], -1));
        Shape("gate: ratios 173 long", () => Ft8CodewordDecoder.Decode(new float[173]));

        Bound("all ratios exactly zero", new float[Ft8Tables.LdpcN]);
        Bound("every bit confidently 0", clean.Select(_ => -SoftCodeword.ConfidentMagnitude).ToArray());
        Bound("every ratio negated (convention inverted)", clean.Select(r => -r).ToArray());
        Bound("a clean codeword with 44 bits flipped", Damaged(clean, 44));

        // And the one that must NOT refuse, so the table is not a list of a decoder that always
        // says no.
        var accepted = Ft8CodewordDecoder.Decode(clean);
        rows.Add($"{"a clean codeword, unaltered",-46} | {accepted.Status}, missed by "
            + $"{accepted.Correction.UnsatisfiedChecks} of {Ft8Tables.LdpcM} checks");

        _output.WriteLine("what was handed in                             | what happened");
        _output.WriteLine("-----------------------------------------------+------------------------------------");
        foreach (var row in rows)
        {
            _output.WriteLine(row);
        }

        Assert.True(accepted.Decoded);
    }

    private static float[] Damaged(float[] clean, int k)
    {
        var ratios = (float[])clean.Clone();
        SoftCodeword.FlipDistinctPositions(ratios, k, new Random(DamageSeed + 2));
        return ratios;
    }

    private static string Ft8MessageTextOf(byte[] message) =>
        Ft8Sharp.Message.Ft8MessageDecoder.Decode(message).Text;

    private static Answer DecodeOne(float[] ratios)
    {
        var bits = new byte[Ft8Tables.LdpcN];
        var result = LdpcDecoder.Decode(ratios, bits);
        return new Answer(bits, result.UnsatisfiedChecks, result.Iterations);
    }
}
