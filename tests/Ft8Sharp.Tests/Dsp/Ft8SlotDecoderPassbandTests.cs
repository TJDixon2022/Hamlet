using Ft8Sharp.Dsp;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Twenty at once, end to end.</b> Unit 214 proved the <em>search</em> finds 20 of 20 across the
/// passband, which is what a 3 kHz slice of 20 m actually carries. It has never been shown that
/// twenty become twenty messages.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is not dropped, though the drop condition licenses it.</b> Task 5 produced a real
/// comparison against upstream's own recordings on rung 1, which is the branch that makes this
/// droppable — a real off-air recording carries many overlapping signals and strictly dominates a
/// synthetic twenty. It is run anyway, and the reason is task 5's own result: 760 of 1298. A
/// synthetic slot is the one instrument that separates <em>overlap</em> from <em>fading,
/// interference and timing error</em>, because it has the first and none of the others. If twenty
/// clean overlapping transmissions all come back, the shortfall on the recordings is not about
/// having more than one signal in the slot, and the next unit can stop looking there.
/// </para>
/// <para>
/// <b>The same twenty the search was measured on.</b> The slot is built by
/// <c>Ft8SearchPassbandTests.BuildPassbandSlot</c>, which is unit 214's own fixture — twenty
/// different messages at twenty frequencies from 300 Hz up, every one at a different fraction of a
/// bin, at five different start offsets, summed into one buffer.
/// </para>
/// </remarks>
public class Ft8SlotDecoderPassbandTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private readonly ITestOutputHelper _output;

    public Ft8SlotDecoderPassbandTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>How many of the twenty came back as their own message, how many extra came back that were
    /// not transmitted, and the per-stage counts.</b>
    /// </summary>
    [Fact]
    public void TwentyOverlappingTransmissionsBecomeTwentyMessages()
    {
        var (slot, truths) = Ft8SearchPassbandTests.BuildPassbandSlot();
        Report("clean", slot, truths);
    }

    /// <summary>The same twenty, in seeded noise at a stated delivered ratio.</summary>
    [Fact]
    public void TwentyOverlappingTransmissionsSurviveSeededNoise()
    {
        const double RequestedSnr = -10.0;

        var (clean, truths) = Ft8SearchPassbandTests.BuildPassbandSlot();

        // The ratio is quoted PER TRANSMISSION, against the power of one of them alone, which is the
        // convention unit 214 used on the same fixture.
        var corpus = EncodeCorpus.Build();
        var onePower = SearchFixture.TransmissionPower(Rate, corpus[0], truths[0].BaseFrequencyHz);
        var sigma = SignalToNoise.NoiseAmplitudeFor(onePower, RequestedSnr, Rate);

        var mixed = SearchFixture.AddNoise(clean, new GaussianNoise(seed: 216_701), sigma, out var noisePower);
        var delivered = SignalToNoise.DecibelsFor(onePower, noisePower, Rate);

        _output.WriteLine($"  requested per-transmission ratio: {RequestedSnr:F1} dB");
        _output.WriteLine($"  delivered:                        {delivered:F3} dB");
        _output.WriteLine("  STATED, and not compared with any published sensitivity figure.");
        _output.WriteLine(string.Empty);

        Report($"in noise at {delivered:F3} dB", mixed, truths);
    }

    private void Report(string what, float[] slot, IReadOnlyList<SearchFixture.Truth> truths)
    {
        var corpus = EncodeCorpus.Build();
        var result = new Ft8SlotDecoder().Decode(slot);
        var returned = result.Texts.ToList();

        // What was actually transmitted, in the order ManySignals placed it: corpus[i % count].
        var expected = new List<string>();
        for (var i = 0; i < truths.Count; i++)
        {
            expected.Add(Ft8MessageDecoder.Decode(corpus[i % corpus.Count].Message).Text);
        }

        var outstanding = new List<string>(expected);
        var matched = 0;
        var extra = new List<string>();

        foreach (var text in returned)
        {
            var at = outstanding.FindIndex(e => string.Equals(e, text, StringComparison.Ordinal));
            if (at >= 0)
            {
                outstanding.RemoveAt(at);
                matched++;
            }
            else
            {
                extra.Add(text);
            }
        }

        _output.WriteLine($"  slot:                       {what}");
        _output.WriteLine($"  transmissions in the slot:  {truths.Count}");
        _output.WriteLine($"  candidates:                 {result.CandidateCount}");
        _output.WriteLine($"  of those, parity satisfied: {result.ParitySatisfiedCount}");
        _output.WriteLine($"  of those, checksum passed:  {result.ChecksumPassedCount}");
        _output.WriteLine($"  of those, became text:      {result.BecameTextCount}");
        _output.WriteLine($"  duplicates suppressed:      {result.DuplicateCount}");
        _output.WriteLine($"  unique messages returned:   {result.Messages.Count}");
        _output.WriteLine($"  CAME BACK AS THEMSELVES:    {matched} of {truths.Count}");
        _output.WriteLine($"  MISSED:                     {outstanding.Count}");
        _output.WriteLine($"  EXTRA, NOT TRANSMITTED:     {extra.Count}");
        _output.WriteLine(string.Empty);

        var geometry = new Ft8WaterfallGeometry();
        foreach (var message in result.Messages)
        {
            _output.WriteLine(
                $"    {message.FrequencyHz(geometry),9:F3} Hz  {message.TimeSeconds(geometry),6:F3} s  "
                + $"score {message.Candidate.Score,3}  {message.Text}");
        }

        foreach (var missing in outstanding)
        {
            _output.WriteLine($"    MISSED: {missing}");
        }

        foreach (var text in extra)
        {
            _output.WriteLine($"    EXTRA:  {text}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE NUMBERS ARE PRINTED BEFORE ANY BOUND IS ASSERTED ON THEM.");

        Assert.Empty(extra);
        Assert.Equal(truths.Count, matched);
    }
}
