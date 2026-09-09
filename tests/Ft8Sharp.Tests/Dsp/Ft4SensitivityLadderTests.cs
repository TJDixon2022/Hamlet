using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// Unit 289 task 8 — step 1's one nice-to-pass criterion: <b>a sensitivity ladder at stated SNRs,
/// with its trial count and its wrong count counted separately from its missed count.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The decibel axis is <see cref="SignalToNoise"/>'s and its definition is written out there in
/// full</b>: power in a 2500 Hz reference bandwidth, which is the convention the published FT8
/// figures are quoted against. Unit 222 checked that axis against a second instrument sharing no
/// line of code and the two agreed to a mean of 0.0098 dB. <b>The SNR delivered at each rung is
/// measured from the samples rather than assumed from the request</b>, and it is the measured figure
/// that is reported.
/// </para>
/// <para>
/// <b>The signal power is taken over the transmission and not over the slot.</b> An FT4 slot is
/// 5.04 s of signal in 7.5 s, so a mean square over the whole buffer would read a third low and every
/// rung would be quoted about 1.7 dB better than it is.
/// </para>
/// <para>
/// <b>WHAT THIS IS NOT.</b> It is not a step 6 result and it is not comparable with any published
/// FT4 threshold. It is a curve over audio this library synthesized itself — no fading, no drift, no
/// neighbours, one signal in an empty slot — drawn once, in one process, at one seed. The
/// reproduction, the verdict against a published figure and the graceful-degradation criterion that
/// a real sensitivity step wants are none of them taken here.
/// </para>
/// <para>
/// <b>A wrong decode is counted separately from a missed one at every rung</b>, and the wrong count
/// is reported even where it is zero. One transmission goes into each slot, so any other message
/// coming out of it is a message nobody sent.
/// </para>
/// </remarks>
public class Ft4SensitivityLadderTests
{
    private readonly Unit289Report _output;

    public Ft4SensitivityLadderTests(ITestOutputHelper output) =>
        _output = new Unit289Report("task8-ladder", output);

    /// <summary>The rungs, in decibels in the 2500 Hz reference bandwidth.</summary>
    private static readonly double[] Rungs = [0.0, -5.0, -10.0, -13.0, -15.0, -17.0, -19.0, -21.0];

    /// <summary>
    /// The seed. <b>Fixed, so the ladder is a number rather than a range</b>, and stated so that a
    /// later unit re-drawing it knows what it is re-drawing.
    /// </summary>
    private const int Seed = 289;

    [Fact]
    public void TheFt4LadderIsDrawnWithItsWrongCountSeparateFromItsMissedCount()
    {
        var corpus = Ft4RoundTripCorpus.Build();
        var decoder = new Ft4SlotDecoder();
        const int rate = Ft4Waveform.DefaultSampleRate;
        const float frequency = 1000.0f;

        _output.WriteLine("THE FT4 SENSITIVITY LADDER");
        _output.WriteLine($"  corpus            {corpus.Count} messages, each sent once per rung");
        _output.WriteLine($"  base frequency    {frequency:F1} Hz");
        _output.WriteLine($"  noise             white Gaussian, seed {Seed}, added across the whole slot");
        _output.WriteLine("  axis              power in a 2500 Hz reference bandwidth, measured per trial");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"requested",-11}{"delivered",-11}{"trials",-8}{"decoded",-9}{"WRONG",-7}missed");

        var totalTrials = 0;
        var totalWrong = 0;
        var wrongDetail = new List<string>();

        foreach (var rung in Rungs)
        {
            var noise = new GaussianNoise(Seed);
            var trials = 0;
            var decoded = 0;
            var wrong = 0;
            var missed = 0;
            var deliveredSum = 0.0;

            foreach (var entry in corpus)
            {
                var symbols = Ft4SymbolEncoder.Encode(entry.Message);
                var signal = Ft4Waveform.Synthesize(symbols, rate, frequency);
                var slot = Ft4Waveform.SynthesizeSlot(symbols, rate, frequency);

                // Over the transmission, not over the slot: two thirds of the slot is silence.
                var signalPower = SignalToNoise.MeanSquare(signal);
                var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, rate);
                var mixed = noise.AddedTo(slot, sigma);

                deliveredSum += SignalToNoise.DecibelsFor(signalPower, sigma * sigma, rate);

                var result = decoder.Decode(mixed);
                trials++;

                var itsOwn = false;
                foreach (var message in result.Messages)
                {
                    if (message.Text == entry.Text)
                    {
                        itsOwn = true;
                        continue;
                    }

                    wrong++;
                    wrongDetail.Add(
                        $"at {rung:F1} dB, sent \"{entry.Text}\" and read \"{message.Text}\"");
                }

                if (itsOwn)
                {
                    decoded++;
                }
                else
                {
                    missed++;
                }
            }

            totalTrials += trials;
            totalWrong += wrong;

            _output.WriteLine(
                $"{rung,-11:F1}{deliveredSum / trials,-11:F3}{trials,-8}{decoded,-9}{wrong,-7}{missed}");
        }

        // Slots with no transmission in them at all. A decoder that returns anything here is
        // inventing, which is the fault §0.0 exists to prevent.
        var emptyNoise = new GaussianNoise(Seed + 1);
        var reference = Ft4Waveform.Synthesize(
            Ft4SymbolEncoder.Encode(corpus[0].Message), rate, frequency);
        var emptySigma = SignalToNoise.NoiseAmplitudeFor(
            SignalToNoise.MeanSquare(reference), -10.0, rate);
        var fromNothing = 0;
        const int emptySlots = 20;
        for (var i = 0; i < emptySlots; i++)
        {
            var noiseOnly = emptyNoise.Block(Ft4Waveform.SlotSampleCount(rate), emptySigma);
            fromNothing += decoder.Decode(noiseOnly).Messages.Count;
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  trials in all              {totalTrials}");
        _output.WriteLine($"  WRONG DECODES in all       {totalWrong}");
        _output.WriteLine($"  messages out of {emptySlots} slots of noise alone: {fromNothing}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NOT A STEP 6 RESULT. One process, one seed, one frequency, audio this");
        _output.WriteLine("  library synthesized itself - no fading, no drift, no neighbours. Not");
        _output.WriteLine("  reproduced in a second process and not compared with any published FT4");
        _output.WriteLine("  figure as a verdict.");

        if (wrongDetail.Count > 0)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("WRONG DECODES");
            foreach (var line in wrongDetail)
            {
                _output.WriteLine("  " + line);
            }
        }

        // The ladder itself is a measurement and this test does not require it to reach any
        // particular rung. What it DOES require is the property §0.0 is about.
        Assert.True(
            totalWrong == 0,
            $"{totalWrong} wrong decode(s) across {totalTrials} trials — a message on the screen "
            + "nobody sent:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, wrongDetail));

        Assert.Equal(0, fromNothing);
        Assert.Equal(Rungs.Length * corpus.Count, totalTrials);
    }
}
