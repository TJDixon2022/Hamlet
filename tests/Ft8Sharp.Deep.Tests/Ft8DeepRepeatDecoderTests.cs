using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The combiner in the loop: slots in order, and a repeat heard as the sum of both hearings.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Three properties, and the first is what keeps the instrument an instrument.</b> With combining
/// off the whole <see cref="Ft8SlotResult"/> is the port's, count for count and message for message;
/// remembering hearings changes nothing either, so the scoreboard's OSD-off column is still the port's
/// own numbers; and with combining on, every message the single-slot path returned is still there, in
/// order, unchanged — <b>combining only ever adds.</b>
/// </para>
/// <para>
/// The gain itself is measured on the ladder in <c>tests/Ft8Sharp.Tests</c>, where the harness lives.
/// This file is the sibling's own suite and stands on audio it synthesises itself.
/// </para>
/// </remarks>
public class Ft8DeepRepeatDecoderTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>
    /// <b>Noise amplitudes to walk, and they are amplitudes rather than decibels on purpose.</b> The
    /// calibrated ratio lives in the harness with the ladder, in <c>tests/Ft8Sharp.Tests</c>; this
    /// suite has no calibration and does not pretend to one. The range is wide enough to cross the
    /// point where a single slot stops decoding, which is what these tests are about, and the whole
    /// sweep is printed so the crossing is visible rather than assumed.
    /// </summary>
    private static readonly double[] NoiseSweep = [2.0, 4.0, 6.0, 8.0, 10.0, 12.0, 14.0, 16.0];

    /// <summary>
    /// <b>Combining off is the port, whole.</b> Not optional and not to be weakened: without it a
    /// difference between the scoreboard's columns stops being attributable to one named change.
    /// </summary>
    [Fact]
    public void WithCombiningOffTheWholeResultIsThePortsResult()
    {
        var slot = CleanSlot("HAMLET 247");
        var noisy = WithNoise(slot, 0.6, 247_201);

        var port = new Ft8SlotDecoder();
        var sibling = new Ft8DeepSlotDecoder();
        var repeat = new Ft8DeepRepeatDecoder();

        Assert.Null(repeat.Combining);

        foreach (var (name, audio) in new[] { ("clean", slot), ("noisy", noisy) })
        {
            var expected = port.Decode(audio);
            var fromSibling = sibling.Decode(audio);
            var fromRepeat = repeat.Decode(audio);

            AssertSameResult(name + ", sibling", expected, fromSibling);
            AssertSameResult(name + ", repeat decoder", expected, fromRepeat);

            Assert.Equal(default, repeat.LastCombine);

            output.WriteLine(
                $"{name,-6}: candidates {expected.CandidateCount,3}, parity "
                + $"{expected.ParitySatisfiedCount,3}, checksum {expected.ChecksumPassedCount,3}, "
                + $"text {expected.BecameTextCount,3}, duplicates {expected.DuplicateCount,3}, "
                + $"messages {expected.Messages.Count} - IDENTICAL through all three");
        }
    }

    /// <summary>
    /// <b>Remembering hearings changes no decision and no count.</b> It only keeps a copy of what was
    /// already computed, which is why it can be turned on without moving a scoreboard row.
    /// </summary>
    [Fact]
    public void RememberingHearingsChangesNothingAboutTheResult()
    {
        var noisy = WithNoise(CleanSlot("HAMLET 247"), 0.6, 247_202);

        var forgetful = new Ft8DeepSlotDecoder();
        var remembering = new Ft8DeepSlotDecoder(rememberHearings: true);

        Assert.False(forgetful.RemembersHearings);
        Assert.True(remembering.RemembersHearings);

        var expected = forgetful.Decode(noisy);
        var actual = remembering.Decode(noisy);

        AssertSameResult("remembering", expected, actual);

        Assert.Empty(forgetful.LastHearings);
        Assert.Equal(expected.CandidateCount, remembering.LastHearings.Count);

        foreach (var hearing in remembering.LastHearings)
        {
            Assert.Equal(Ft8SoftSymbols.RatioCount, hearing.Ratios.Length);

            // At the port's own scale, because that is what the gate saw.
            Assert.Equal(
                Ft8SoftSymbols.NormalisedVariance,
                Ft8SoftSymbols.Variance(hearing.Ratios),
                1);
        }

        output.WriteLine(
            $"{remembering.LastHearings.Count} hearings kept, {Ft8SoftSymbols.RatioCount} ratios each, "
            + $"about {remembering.LastHearings.Count * Ft8SoftSymbols.RatioCount * 4 / 1024} kB.");
    }

    /// <summary>
    /// <b>THE HEADLINE: a message neither slot could decode alone, read out of the two together.</b>
    /// </summary>
    /// <remarks>
    /// The same transmission at the same place in two slots, each with its own noise draw. The noise
    /// is swept because where the threshold falls is a property of the audio rather than something to
    /// assume, and the whole sweep is printed. <b>What is asserted is that at least one level produced
    /// a decode neither slot gave up alone, and that no level produced a message that was not
    /// sent.</b>
    /// </remarks>
    [Fact]
    public void AMessageNeitherSlotCouldDecodeAloneIsReadOutOfTheTwoTogether()
    {
        const string text = "HAMLET 247";
        var clean = CleanSlot(text);

        var onlyTogether = 0;
        var wrong = new List<string>();
        var submissions = 0;
        var offered = 0;

        output.WriteLine("noise  slot A alone  slot B alone  COMBINED  offered  submitted  accepted");

        foreach (var amplitude in NoiseSweep)
        {
            var first = WithNoise(clean, amplitude, 247_301);
            var second = WithNoise(clean, amplitude, 247_302);

            var alone = new Ft8DeepSlotDecoder();
            var aloneA = alone.Decode(first).Texts.Contains(text, StringComparer.Ordinal);
            var aloneB = alone.Decode(second).Texts.Contains(text, StringComparer.Ordinal);

            var repeat = new Ft8DeepRepeatDecoder(combining: Ft8DeepCombineSettings.Default);
            repeat.Decode(first);
            var together = repeat.Decode(second);

            var counts = repeat.LastCombine;
            offered += counts.Offered;
            submissions += counts.Submitted;

            var decoded = together.Texts.Contains(text, StringComparer.Ordinal);
            foreach (var returned in together.Texts)
            {
                if (!string.Equals(returned, text, StringComparison.Ordinal))
                {
                    wrong.Add($"noise {amplitude:F1}: SENT \"{text}\" RETURNED \"{returned}\"");
                }
            }

            output.WriteLine(
                $"{amplitude,5:F1}  {(aloneA ? "decoded" : "missed"),12}  "
                + $"{(aloneB ? "decoded" : "missed"),12}  {(decoded ? "DECODED" : "missed"),8}  "
                + $"{counts.Offered,7}  {counts.Submitted,9}  {counts.Accepted,8}");

            if (!aloneA && !aloneB && decoded)
            {
                onlyTogether++;
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            $"{onlyTogether} noise levels where NEITHER slot decoded alone and the two together did.");
        output.WriteLine(
            $"{offered} pairs offered, {submissions} combinations submitted to the port's gates, "
            + $"naive expectation "
            + $"{Ft8DeepCombineSettings.ExpectedFalseAccepts(submissions):F4} messages nobody sent.");

        foreach (var line in wrong)
        {
            output.WriteLine($"  {line}");
        }

        Assert.Empty(wrong);
        Assert.True(
            onlyTogether > 0,
            "no noise level in the sweep produced a message that neither slot could decode alone and "
                + "the combination could, so this test is not evidence that the loop reaches anything.");
    }

    /// <summary>
    /// <b>Combining only ever adds, asserted rather than intended.</b>
    /// </summary>
    [Fact]
    public void EveryMessageTheSingleSlotPathReturnedIsStillThereInOrder()
    {
        var clean = CleanSlot("HAMLET 247");
        var checkedLevels = 0;

        foreach (var amplitude in NoiseSweep)
        {
            var first = WithNoise(clean, amplitude, 247_401);
            var second = WithNoise(clean, amplitude, 247_402);

            var off = new Ft8DeepRepeatDecoder();
            off.Decode(first);
            var withoutCombining = off.Decode(second);

            var on = new Ft8DeepRepeatDecoder(combining: Ft8DeepCombineSettings.Default);
            on.Decode(first);
            var withCombining = on.Decode(second);

            // THE SUPERSET PROPERTY: same messages, same order, at the front.
            Assert.True(
                withCombining.Messages.Count >= withoutCombining.Messages.Count,
                $"noise {amplitude:F1}: combining off returned {withoutCombining.Messages.Count} "
                    + $"messages and combining on returned {withCombining.Messages.Count}. Combining "
                    + "only ever adds.");

            for (var i = 0; i < withoutCombining.Messages.Count; i++)
            {
                Assert.Equal(withoutCombining.Messages[i], withCombining.Messages[i]);
            }

            // The five counts are the port's report on the port's belief propagation and combining
            // does not touch them.
            Assert.Equal(withoutCombining.CandidateCount, withCombining.CandidateCount);
            Assert.Equal(withoutCombining.ParitySatisfiedCount, withCombining.ParitySatisfiedCount);
            Assert.Equal(withoutCombining.ChecksumPassedCount, withCombining.ChecksumPassedCount);
            Assert.Equal(withoutCombining.BecameTextCount, withCombining.BecameTextCount);
            Assert.Equal(withoutCombining.DuplicateCount, withCombining.DuplicateCount);

            output.WriteLine(
                $"noise {amplitude:F1}: off {withoutCombining.Messages.Count} messages, on "
                + $"{withCombining.Messages.Count}, first {withoutCombining.Messages.Count} identical "
                + $"and in order; combined added {on.LastCombine.Added}");

            checkedLevels++;
        }

        Assert.Equal(NoiseSweep.Length, checkedLevels);
    }

    /// <summary>
    /// <b>The budget is not exceeded, whatever the audio.</b> Submissions per slot are bounded by
    /// candidates times partners times history depth, and that bound is checked against what was
    /// actually spent.
    /// </summary>
    [Fact]
    public void TheSubmissionsSpentNeverExceedTheBudgetTheSettingsBound()
    {
        var clean = CleanSlot("HAMLET 247");

        foreach (var partners in new[] { 1, 2, 4 })
        {
            foreach (var depth in new[] { 1, 2, 3 })
            {
                var settings = new Ft8DeepCombineSettings(
                    historyDepth: depth, maximumPartners: partners);
                var repeat = new Ft8DeepRepeatDecoder(combining: settings);

                var spent = 0;
                var worstBound = 0;

                for (var slot = 0; slot < 5; slot++)
                {
                    var result = repeat.Decode(WithNoise(clean, 1.2, 247_500 + slot));
                    var bound = settings.SubmissionsPerSlot(result.CandidateCount);

                    Assert.True(
                        repeat.LastCombine.Submitted <= bound,
                        $"partners {partners}, depth {depth}, slot {slot}: "
                            + $"{repeat.LastCombine.Submitted} combinations were submitted against a "
                            + $"bound of {bound} for {result.CandidateCount} candidates. The budget is "
                            + "the whole of what stops this step putting a message nobody sent in "
                            + "front of the operator.");

                    spent += repeat.LastCombine.Submitted;
                    worstBound = Math.Max(worstBound, bound);
                    Assert.True(repeat.RememberedSlots <= depth);
                }

                output.WriteLine(
                    $"partners {partners}, depth {depth}: {spent} submitted over 5 slots, worst "
                    + $"single-slot bound {worstBound}, {repeat.RememberedSlots} slots remembered");
            }
        }
    }

    /// <summary>
    /// <b>Reset forgets, so one measurement's last slot is never combined with the next one's
    /// first.</b>
    /// </summary>
    [Fact]
    public void ResetForgetsEveryRememberedSlot()
    {
        var repeat = new Ft8DeepRepeatDecoder(combining: Ft8DeepCombineSettings.Default);
        var clean = CleanSlot("HAMLET 247");

        Assert.Equal(0, repeat.RememberedSlots);
        repeat.Decode(WithNoise(clean, 1.0, 247_601));
        Assert.Equal(1, repeat.RememberedSlots);
        Assert.Equal(0, repeat.LastCombine.Offered);

        repeat.Decode(WithNoise(clean, 1.0, 247_602));
        Assert.Equal(1, repeat.RememberedSlots);
        Assert.True(repeat.LastCombine.Offered > 0);

        repeat.Reset();
        Assert.Equal(0, repeat.RememberedSlots);

        repeat.Decode(WithNoise(clean, 1.0, 247_603));
        Assert.Equal(0, repeat.LastCombine.Offered);
        output.WriteLine("After Reset the next slot offered no pairs, so nothing carried over.");
    }

    /// <summary>
    /// <b>A combiner with nothing remembered is refused rather than silently doing nothing.</b>
    /// </summary>
    [Fact]
    public void CombiningOverADecoderThatForgetsIsRefused()
    {
        var refusal = Assert.Throws<ArgumentException>(
            () => new Ft8DeepRepeatDecoder(
                new Ft8DeepSlotDecoder(), Ft8DeepCombineSettings.Default));

        output.WriteLine(refusal.Message);

        // And the null-inner path builds one that remembers, so the ordinary caller cannot get it
        // wrong.
        var made = new Ft8DeepRepeatDecoder(combining: Ft8DeepCombineSettings.Default);
        Assert.True(made.Inner.RemembersHearings);

        // Off means off: no remembering is asked for and none happens.
        var off = new Ft8DeepRepeatDecoder();
        Assert.False(off.Inner.RemembersHearings);
    }

    private static void AssertSameResult(string what, Ft8SlotResult expected, Ft8SlotResult actual)
    {
        Assert.True(expected.CandidateCount == actual.CandidateCount, $"{what}: candidate count");
        Assert.True(
            expected.ParitySatisfiedCount == actual.ParitySatisfiedCount, $"{what}: parity count");
        Assert.True(
            expected.ChecksumPassedCount == actual.ChecksumPassedCount, $"{what}: checksum count");
        Assert.True(expected.BecameTextCount == actual.BecameTextCount, $"{what}: became text count");
        Assert.True(expected.DuplicateCount == actual.DuplicateCount, $"{what}: duplicate count");
        Assert.True(
            expected.Messages.Count == actual.Messages.Count, $"{what}: message count");

        for (var i = 0; i < expected.Messages.Count; i++)
        {
            Assert.True(expected.Messages[i] == actual.Messages[i], $"{what}: message {i}");
        }
    }

    /// <summary>A clean transmission of one free-text message, laid into a whole slot.</summary>
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
