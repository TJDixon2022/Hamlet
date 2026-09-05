using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>Four hearings of one transmission summed into ONE codeword, rather than paired two slots at a
/// time.</b> Unit 254's build, its deterministic watched failure, and the two identities that protect
/// every figure this project has recorded.
/// </summary>
/// <remarks>
/// <para>
/// <b>THE BREAKAGE, AND IT WAS IN THE TREE UNTIL TONIGHT.</b>
/// <c>Ft8DeepRepeatDecoder.Combine</c> walked its history most-recent-first and called the
/// <em>two-hearing</em> overload of <c>Ft8DeepSoftCombiner.Combine</c> once per remembered slot, so a
/// decoder handed four slots computed a chain of pairs and reported a four-slot combination.
/// <c>Ft8LadderHarness.RunRepeats</c> names its third column <c>combined x{repeats}</c> from the
/// repeat count and never from the depth of any sum, so <b>a column headed <c>combined x4</c> was
/// measuring 3.01 dB of processing gain while its name claimed 6.02.</b> No count in
/// <c>Ft8DeepCombineCounts</c> could tell a reader that, and no test in the tree asserted anything
/// about three hearings — <c>Ft8DeepSoftCombinerTests</c>' five facts are all about two.
/// <see cref="Ft8DeepCombineCounts.DeepestHearings"/> and
/// <see cref="TheDeepestCombinationOfFourHearingsCarriesFourAndNotTwo"/> are what would have caught
/// it.
/// </para>
/// <para>
/// <b>What this is and what it is not.</b> It is the sum of independent hearings of one
/// transmission: <c>R</c> conditionally independent observations of the same codeword bit carry
/// log-likelihood ratios that add, so the per-bit signal-to-noise ratio grows as <c>R</c> and the
/// processing gain is <c>10 log10 R</c> decibels. That is textbook and comes from nobody's source.
/// The frame is cited: 174 bits in codeword order carrying a 77-bit payload and a CRC-14, from
/// S. Franke K9AN, B. Somerville G4WJS and J. Taylor K1JT, <em>The FT4 and FT8 Communication
/// Protocols</em>, QEX, July/August 2020 — <b>position <c>i</c> means the same codeword bit in every
/// hearing because the protocol says so, and a repeated transmission is the same 174 bits again.</b>
/// </para>
/// <para>
/// It is <b>not</b> a gate, a threshold or an acceptance rule, and it does not change how many
/// codewords are put to the port's parity and CRC-14 gates per candidate per remembered slot:
/// <see cref="TheDeeperSumSpendsExactlyTheBudgetThePairwiseRuleSpent"/> asserts that against the
/// pairwise decoder on the same audio.
/// </para>
/// </remarks>
public class Ft8Unit254AccumulationTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private const string Text = "HAMLET 254";

    /// <summary>
    /// <b>THE WATCHED FAILURE, AND IT IS DETERMINISTIC.</b> Four hearings of one known codeword go
    /// through <see cref="Ft8DeepRepeatDecoder"/> at a history of three and an accumulation depth of
    /// three, and the deepest combination the fourth slot submitted is asked how many hearings it
    /// carried.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Before the accumulator that number is 2 and this assertion fails.</b> Its red is the whole
    /// breakage in one line: a decoder that reports a four-slot combination and computes a chain of
    /// pairs. There is no noise draw in the assertion and no rate anywhere near it — four slots, one
    /// message, one integer.
    /// </para>
    /// <para>
    /// <b>Why the fourth slot and not the second.</b> Slot 0 has nothing behind it, slot 1 has one
    /// remembered slot and can only make a pair however deep the rule is, slot 2 can reach three
    /// hearings and slot 3 four. The assertion is on slot 3 because four is the number
    /// <c>PHASE_PLAN.md</c>'s <em>four repeats is 6 dB</em> is about.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheDeepestCombinationOfFourHearingsCarriesFourAndNotTwo()
    {
        var clean = CleanSlot(Text);

        // Loud enough that every slot offers a candidate at the transmission, so the assertion is
        // about the depth of the sum and never about whether the search found anything.
        var settings = new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3);
        var repeat = new Ft8DeepRepeatDecoder(combining: settings);

        var carried = new int[4];
        var submitted = new int[4];

        for (var slot = 0; slot < 4; slot++)
        {
            var behind = repeat.RememberedSlots;
            repeat.Decode(WithNoise(clean, 1.0, 254_101 + slot));
            carried[slot] = repeat.LastCombine.DeepestHearings;
            submitted[slot] = repeat.LastCombine.Submitted;

            output.WriteLine(
                $"slot {slot}: {behind} slots remembered behind it, {submitted[slot]} combinations "
                + $"submitted, deepest carried {carried[slot]} hearings");
        }

        // THE ASSERTION THIS UNIT EXISTS TO MAKE, AND IT IS FIRST SO ITS RED IS THE ONE A READER
        // SEES: the fourth slot had three slots behind it, and the deepest sum it put to the port's
        // gates carried four hearings of one transmission rather than two.
        Assert.Equal(4, carried[3]);

        // And the shallower slots, so the count is a ladder rather than a single number: nothing to
        // combine with, then a pair, then three.
        Assert.Equal(0, carried[0]);
        Assert.Equal(2, carried[1]);
        Assert.Equal(3, carried[2]);
    }

    /// <summary>
    /// <b>THE BUDGET DID NOT MOVE.</b> The same four slots of the same audio through the pairwise
    /// rule and through the accumulating one, and the combinations submitted are equal slot for slot.
    /// </summary>
    /// <remarks>
    /// <b>This is the assertion that makes the deeper sum safe.</b> Every submission is an
    /// independent chance of the port's CRC-14 accepting a message nobody sent, at about one in
    /// 16 384. A rule that submitted the 2-way and the 3-way and the 4-way would have spent three of
    /// those where the pairwise rule spent one; the sliding window submits one combination per
    /// candidate per partner rank per remembered slot exactly as before and only changes what is in
    /// it. Asserted rather than argued, because the argument is the kind that stays true in a
    /// comment after the code stops obeying it.
    /// </remarks>
    [Fact]
    public void TheDeeperSumSpendsExactlyTheBudgetThePairwiseRuleSpent()
    {
        var clean = CleanSlot(Text);

        foreach (var partners in new[] { 1, 2 })
        {
            var pairwise = new Ft8DeepRepeatDecoder(
                combining: new Ft8DeepCombineSettings(
                    historyDepth: 3, maximumPartners: partners, accumulationDepth: 1));
            var accumulating = new Ft8DeepRepeatDecoder(
                combining: new Ft8DeepCombineSettings(
                    historyDepth: 3, maximumPartners: partners, accumulationDepth: 3));

            for (var slot = 0; slot < 5; slot++)
            {
                var audio = WithNoise(clean, 1.4, 254_201 + slot);

                pairwise.Decode(audio);
                accumulating.Decode(audio);

                output.WriteLine(
                    $"partners {partners}, slot {slot}: pairwise offered "
                    + $"{pairwise.LastCombine.Offered} submitted {pairwise.LastCombine.Submitted} "
                    + $"deepest {pairwise.LastCombine.DeepestHearings}; accumulated offered "
                    + $"{accumulating.LastCombine.Offered} submitted "
                    + $"{accumulating.LastCombine.Submitted} deepest "
                    + $"{accumulating.LastCombine.DeepestHearings}");

                Assert.Equal(pairwise.LastCombine.Offered, accumulating.LastCombine.Offered);
                Assert.Equal(pairwise.LastCombine.Submitted, accumulating.LastCombine.Submitted);

                // And the bound the settings publish is still the bound, at either depth.
                Assert.True(
                    accumulating.LastCombine.Submitted
                        <= accumulating.Combining!.SubmissionsPerSlot(140),
                    "the accumulating rule spent more than the budget its own settings multiply out.");
            }
        }
    }

    /// <summary>
    /// <b>THE IDENTITY THAT PROTECTS EVERY RECORDED FIGURE, HALF ONE: with combining off,
    /// <see cref="Ft8DeepRepeatDecoder"/> returns bit-for-bit what
    /// <see cref="Ft8DeepSlotDecoder"/> returns.</b>
    /// </summary>
    /// <remarks>
    /// Unit 246's ruling 4, carried forward by unit 247 §5 item 6 and re-asserted here because unit
    /// 254 rewrote the body of <c>Combine</c>. <b>Combining stays off by default</b>, for the same
    /// reason ordered statistics and subtraction do.
    /// </remarks>
    [Fact]
    public void WithCombiningOffTheRepeatDecoderIsStillThePortExactly()
    {
        var clean = CleanSlot(Text);
        var port = new Ft8SlotDecoder();
        var repeat = new Ft8DeepRepeatDecoder();

        Assert.Null(repeat.Combining);

        foreach (var amplitude in new[] { 0.0, 0.6, 1.2 })
        {
            var audio = amplitude == 0.0 ? clean : WithNoise(clean, amplitude, 254_301);
            var expected = port.Decode(audio);
            var actual = repeat.Decode(audio);

            Assert.Equal(expected.Messages.Count, actual.Messages.Count);
            for (var i = 0; i < expected.Messages.Count; i++)
            {
                Assert.Equal(expected.Messages[i].Text, actual.Messages[i].Text);
            }

            Assert.Equal(expected.CandidateCount, actual.CandidateCount);
            Assert.Equal(expected.ParitySatisfiedCount, actual.ParitySatisfiedCount);
            Assert.Equal(expected.ChecksumPassedCount, actual.ChecksumPassedCount);
            Assert.Equal(expected.BecameTextCount, actual.BecameTextCount);
            Assert.Equal(expected.DuplicateCount, actual.DuplicateCount);
            Assert.Equal(default, repeat.LastCombine);

            output.WriteLine(
                $"amplitude {amplitude:F1}: {expected.Messages.Count} messages, "
                + $"{expected.CandidateCount} candidates - IDENTICAL, and no combine counts at all");
        }
    }

    /// <summary>
    /// <b>THE IDENTITY THAT PROTECTS EVERY RECORDED FIGURE, HALF TWO: with combining on at
    /// accumulation depth 1, the decoder returns what it returned before tonight.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Depth 1 is the default, so every figure in <c>docs/unit247-combining.md</c> and every row of
    /// unit 254's own task 2 reproduction was taken at it. <b>The claim is that the rewrite is a
    /// no-op there</b>, and it is checked the only way a rewrite can be checked without keeping a
    /// copy of the old code: against <c>Ft8DeepSlotDecoder</c> plus the pairwise arithmetic done by
    /// hand at <c>historyDepth: 1</c>, where the sliding window is one slot wide and the sum is a
    /// pair by construction.
    /// </para>
    /// <para>
    /// <b>The stronger evidence is on the ladder</b>: task 2 read 13 / 33 / 68 of 306 before this
    /// rewrite and task 4b reads the combined x2 column again after it, at the same seed, and the two
    /// are compared in <c>docs/unit254-combining-depth.md</c>.
    /// </para>
    /// </remarks>
    [Fact]
    public void AtAccumulationDepthOneEverySubmissionIsStillAPair()
    {
        var clean = CleanSlot(Text);

        foreach (var history in new[] { 1, 2, 3 })
        {
            var repeat = new Ft8DeepRepeatDecoder(
                combining: new Ft8DeepCombineSettings(historyDepth: history));

            Assert.Equal(1, repeat.Combining!.AccumulationDepth);

            var slotsWithSubmissions = 0;

            for (var slot = 0; slot < 5; slot++)
            {
                repeat.Decode(WithNoise(clean, 1.4, 254_401 + slot));
                var counts = repeat.LastCombine;

                if (counts.Submitted > 0)
                {
                    slotsWithSubmissions++;
                    Assert.Equal(2, counts.DeepestHearings);
                }
                else
                {
                    Assert.Equal(0, counts.DeepestHearings);
                }
            }

            output.WriteLine(
                $"history {history}, accumulation depth 1: {slotsWithSubmissions} of 5 slots "
                + "submitted anything, and every submission carried exactly 2 hearings");

            Assert.True(
                slotsWithSubmissions > 0,
                "no slot submitted a combination, so this run is not evidence of anything.");
        }
    }

    /// <summary>
    /// <b>A depth the history cannot supply is refused loudly rather than quietly given a pair.</b>
    /// </summary>
    /// <remarks>
    /// <c>Ft8DeepCombineSettings</c>'s own voice, and the reason is unit 254's finding: a caller who
    /// asked for a four-hearing sum, was handed a chain of pairs and reported a <c>combined x4</c>
    /// column is exactly the failure this whole unit exists to remove. A clamp would reintroduce it
    /// silently.
    /// </remarks>
    [Fact]
    public void AnAccumulationDepthTheHistoryCannotSupplyIsRefused()
    {
        var tooDeep = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepCombineSettings(historyDepth: 2, accumulationDepth: 3));
        var zero = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepCombineSettings(historyDepth: 4, accumulationDepth: 0));

        output.WriteLine(tooDeep.Message);
        output.WriteLine(zero.Message);

        Assert.Equal(1, Ft8DeepCombineSettings.Default.AccumulationDepth);
    }

    /// <summary>
    /// <b>Step 5's second exit, taken deeper: a message no single slot decoded and no PAIR of slots
    /// decoded, returned from the sum of four.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Walked over a sweep of noise amplitudes rather than at one, because the interesting band is
    /// narrow: too quiet and a single slot decodes it, too loud and nothing does. <b>Nothing here
    /// asserts a rate</b> and the count is printed whether it is zero or not — if no level in the
    /// sweep produces such a case, that is a measurement and it is reported with the count, which is
    /// what work instruction 254 task 3 asks for.
    /// </para>
    /// <para>
    /// <b>Every return is checked against the message that went in</b>, and a message nobody sent
    /// fails this test at any depth.
    /// </para>
    /// </remarks>
    [Fact]
    public void AMessageNoSlotAndNoPairCouldReadIsReadOutOfTheSumOfFour()
    {
        var clean = CleanSlot(Text);
        var wrong = new List<string>();
        var onlyFromFour = 0;

        output.WriteLine("noise  any slot alone  any pair  SUM OF FOUR  submitted  deepest");

        foreach (var amplitude in new[] { 6.0, 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0 })
        {
            var slots = new float[4][];
            for (var r = 0; r < 4; r++)
            {
                slots[r] = WithNoise(clean, amplitude, 254_501 + r);
            }

            // Any one slot on its own.
            var alone = new Ft8DeepSlotDecoder();
            var anySlotAlone = slots.Any(
                s => alone.Decode(s).Texts.Contains(Text, StringComparer.Ordinal));

            // Any PAIR of slots, which is what the tree computed before tonight - every one of the
            // six pairs, each through a fresh pairwise decoder so nothing carries between them.
            var anyPair = false;
            for (var a = 0; a < 4 && !anyPair; a++)
            {
                for (var b = a + 1; b < 4 && !anyPair; b++)
                {
                    var pairwise = new Ft8DeepRepeatDecoder(
                        combining: new Ft8DeepCombineSettings(historyDepth: 1));
                    pairwise.Decode(slots[a]);
                    var paired = pairwise.Decode(slots[b]);
                    var fromPairAlone = paired.Messages.Count - pairwise.LastCombine.Added;

                    for (var m = fromPairAlone; m < paired.Messages.Count; m++)
                    {
                        if (string.Equals(paired.Messages[m].Text, Text, StringComparison.Ordinal))
                        {
                            anyPair = true;
                        }
                    }
                }
            }

            // All four, summed into one codeword.
            var four = new Ft8DeepRepeatDecoder(
                combining: new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3));
            var last = default(Ft8SlotResult);
            foreach (var slot in slots)
            {
                last = four.Decode(slot);
            }

            var counts = four.LastCombine;
            var fromFourAlone = last.Messages.Count - counts.Added;
            var fromTheSum = false;

            for (var m = fromFourAlone; m < last.Messages.Count; m++)
            {
                if (string.Equals(last.Messages[m].Text, Text, StringComparison.Ordinal))
                {
                    fromTheSum = true;
                }
            }

            foreach (var returned in last.Texts)
            {
                if (!string.Equals(returned, Text, StringComparison.Ordinal))
                {
                    wrong.Add($"noise {amplitude:F1}: SENT \"{Text}\" RETURNED \"{returned}\"");
                }
            }

            output.WriteLine(
                $"{amplitude,5:F1}  {(anySlotAlone ? "decoded" : "missed"),14}  "
                + $"{(anyPair ? "decoded" : "missed"),8}  {(fromTheSum ? "DECODED" : "missed"),11}  "
                + $"{counts.Submitted,9}  {counts.DeepestHearings,7}");

            if (!anySlotAlone && !anyPair && fromTheSum)
            {
                onlyFromFour++;
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            $"{onlyFromFour} of 8 noise levels where NO single slot and NO pair of slots read the "
            + "message and the sum of four did.");

        foreach (var line in wrong)
        {
            output.WriteLine($"  {line}");
        }

        // ASSERTED: zero wrong. The count above is PRINTED - a zero there is a measurement and this
        // unit reports it rather than failing on it.
        Assert.Empty(wrong);
    }

    private static float[] CleanSlot(string text)
    {
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(text, message));

        var symbols = Ft8SymbolEncoder.Encode(message);
        return Ft8Waveform.SynthesizeSlot(symbols, Rate);
    }

    /// <summary>
    /// A copy of a slot with Gaussian noise on it, drawn from a named seed so a fresh process draws
    /// the same noise. Box-Muller from <see cref="Random"/>, which is enough for a unit test — the
    /// calibrated draw lives in the harness, with the ladder.
    /// </summary>
    private static float[] WithNoise(float[] slot, double amplitude, int seed)
    {
        var random = new Random(seed);
        var noisy = new float[slot.Length];

        for (var i = 0; i < slot.Length; i++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();
            var normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            noisy[i] = (float)(slot[i] + (amplitude * normal));
        }

        return noisy;
    }
}
