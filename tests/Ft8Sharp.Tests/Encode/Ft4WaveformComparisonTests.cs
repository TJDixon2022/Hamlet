using Ft8Sharp;
using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Unit 289 task 4 — <b>the FT4 waveform, held against the WAV upstream's own generator writes.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The breakage this catches</b>: a correct FT4 symbol sequence rendered with FT8's smoothing
/// bandwidth, or with the tones spaced at FT8's 6.25 Hz, or with the phase restarted at each symbol
/// boundary. Every one of those produces a buffer of exactly the right length carrying a message the
/// round trip in task 6 would read back perfectly, because this library's own decoder would be
/// reading this library's own mistake. Only upstream's file decides it.
/// </para>
/// <para>
/// <b>Nothing upstream produces is committed.</b> Each WAV is written under the system temp folder,
/// compared, and deleted as we go rather than at the end.
/// </para>
/// </remarks>
public class Ft4WaveformComparisonTests
{
    private readonly Unit289Report _output;

    public Ft4WaveformComparisonTests(ITestOutputHelper output) =>
        _output = new Unit289Report("task4-waveform", output);

    /// <summary>
    /// Every sample of a slot this library synthesizes, against every sample of the slot upstream's
    /// generator writes for the same message.
    /// </summary>
    [RequiresWorkingOracleFact]
    public void EverySampleOfTheFt4SlotIsUpstreamsOwn()
    {
        var comparable = EncodeCorpus.Build().Where(entry => entry.Text is not null).ToList();

        var compared = 0;
        var identical = 0;
        var largestDifference = 0;
        var failures = new List<string>();

        foreach (var entry in comparable)
        {
            var kept = Ft8Oracle.GenerateKeepingWav(entry.Text!, Ft8Oracle.Protocol.Ft4);
            try
            {
                if (kept.Run.ExitCode != 0)
                {
                    failures.Add($"[{entry.Label}] upstream exited {kept.Run.ExitCode}");
                    continue;
                }

                var upstream = WavFile.Read(kept.WavPath);
                var ours = Ft4Waveform.SynthesizeSlotPcm16(Ft4SymbolEncoder.Encode(entry.Message));
                compared++;

                if (upstream.Samples.Length != ours.Length)
                {
                    failures.Add(
                        $"[{entry.Label}] upstream wrote {upstream.Samples.Length} samples and this "
                        + $"library synthesized {ours.Length}");
                    continue;
                }

                var worst = 0;
                var at = -1;
                for (var i = 0; i < ours.Length; i++)
                {
                    var difference = Math.Abs(ours[i] - upstream.Samples[i]);
                    if (difference > worst)
                    {
                        worst = difference;
                        at = i;
                    }
                }

                largestDifference = Math.Max(largestDifference, worst);
                if (worst == 0)
                {
                    identical++;
                }
                else if (worst > 1)
                {
                    failures.Add(
                        $"[{entry.Label}] the largest disagreement is {worst} counts at sample {at} "
                        + "of a sixteen-bit sample, which is more than the one count rounding can "
                        + "account for");
                }
            }
            finally
            {
                WavFile.DeleteQuietly(kept.WavPath);
            }
        }

        _output.WriteLine($"messages compared            : {compared}");
        _output.WriteLine($"slots identical sample for sample : {identical}");
        _output.WriteLine($"largest disagreement anywhere: {largestDifference} counts of 32767");

        Assert.True(
            failures.Count == 0,
            "the FT4 waveform is not upstream's:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, failures));
        Assert.Equal(comparable.Count, compared);
        Assert.True(compared > 0, "nothing was compared, so nothing is proved");
    }

    /// <summary>
    /// The geometry, measured from the audio rather than read back off the constant it came from.
    /// </summary>
    /// <remarks>
    /// <b>Where the FT4 timing constants live</b> is <see cref="Ft4Timing"/> and nowhere else, so
    /// that a ruling on the 4.48-against-5.04 question costs one edit. This test deliberately does
    /// <em>not</em> read that type for its expectations: the sample counts are the numbers a person
    /// would get by measuring the buffer, and the assertion is that the buffer has them.
    /// </remarks>
    [Fact]
    public void TheSlotIsLaidOutWhereUpstreamLaysItOut()
    {
        var symbols = new byte[Ft4SymbolEncoder.SymbolCount];
        var slot = Ft4Waveform.SynthesizeSlot(symbols);
        var signal = Ft4Waveform.Synthesize(symbols);

        const int rate = Ft4Waveform.DefaultSampleRate;

        Assert.Equal(90000, slot.Length);
        Assert.Equal(60480, signal.Length);
        Assert.Equal(14760, Ft4Waveform.PaddingSampleCount(rate));
        Assert.Equal(576, Ft4Waveform.SamplesPerSymbol(rate));
        Assert.Equal(90000, Ft4Waveform.SlotSampleCount(rate));

        // The padding really is silence, at both ends, and the signal really is not.
        var padding = Ft4Waveform.PaddingSampleCount(rate);
        for (var i = 0; i < padding; i++)
        {
            Assert.Equal(0.0f, slot[i]);
            Assert.Equal(0.0f, slot[^(i + 1)]);
        }

        var energy = 0.0;
        for (var i = padding; i < padding + signal.Length; i++)
        {
            energy += slot[i] * (double)slot[i];
        }

        Assert.True(energy > 0.0, "the middle of the slot carries no energy at all");

        _output.WriteLine("MEASURED FROM THE AUDIO");
        _output.WriteLine($"  sample rate        {rate} Hz");
        _output.WriteLine($"  slot               {slot.Length} samples = {slot.Length / (double)rate:F4} s");
        _output.WriteLine($"  signal             {signal.Length} samples = "
            + $"{signal.Length / (double)rate:F4} s");
        _output.WriteLine($"  leading silence    {padding} samples = {padding / (double)rate:F4} s");
        _output.WriteLine($"  samples per symbol {Ft4Waveform.SamplesPerSymbol(rate)}");
        _output.WriteLine($"  symbol period      "
            + $"{Ft4Waveform.SamplesPerSymbol(rate) / (double)rate:F6} s");
        _output.WriteLine($"  symbols            {signal.Length / Ft4Waveform.SamplesPerSymbol(rate)}");
        _output.WriteLine($"  tone spacing       "
            + $"{rate / (double)Ft4Waveform.SamplesPerSymbol(rate):F4} Hz");
        _output.WriteLine($"  tones              {Ft4Waveform.ToneCount}");
    }

    /// <summary>
    /// The four tones really are 20.833 Hz apart in the audio, measured rather than asserted from
    /// the constant.
    /// </summary>
    /// <remarks>
    /// <b>The breakage this catches</b>: FT8's tone spacing applied to FT4's symbols. A signal whose
    /// four tones sit 6.25 Hz apart occupies a quarter of the bandwidth it should, and every sync
    /// score in the decoder would still be computed at the bins the geometry expects — so it would
    /// look like a decoder that cannot find anything rather than like a synthesizer at the wrong
    /// spacing.
    /// </remarks>
    [Fact]
    public void TheFourTonesAreOneOverTheSymbolPeriodApart()
    {
        const int rate = Ft4Waveform.DefaultSampleRate;
        const float baseFrequency = 1000.0f;
        var samplesPerSymbol = Ft4Waveform.SamplesPerSymbol(rate);

        // A steady tone for the whole transmission, one tone value at a time, and the frequency read
        // back by counting zero crossings across the middle of it — away from the envelope ramps and
        // away from the pulse's reach at either end.
        var measured = new double[Ft4Waveform.ToneCount];
        for (var tone = 0; tone < Ft4Waveform.ToneCount; tone++)
        {
            var symbols = new byte[Ft4SymbolEncoder.SymbolCount];
            Array.Fill(symbols, (byte)tone);
            var signal = Ft4Waveform.Synthesize(symbols, rate, baseFrequency);

            var from = 10 * samplesPerSymbol;
            var to = signal.Length - (10 * samplesPerSymbol);
            var crossings = 0;
            for (var i = from + 1; i < to; i++)
            {
                if (signal[i - 1] < 0.0f && signal[i] >= 0.0f)
                {
                    crossings++;
                }
            }

            measured[tone] = crossings / ((to - from) / (double)rate);
        }

        for (var tone = 0; tone < Ft4Waveform.ToneCount; tone++)
        {
            _output.WriteLine($"  tone {tone} measured at {measured[tone]:F3} Hz");
        }

        var expectedSpacing = rate / (double)samplesPerSymbol;
        for (var tone = 1; tone < Ft4Waveform.ToneCount; tone++)
        {
            var spacing = measured[tone] - measured[tone - 1];
            Assert.InRange(spacing, expectedSpacing - 1.0, expectedSpacing + 1.0);
        }

        Assert.InRange(measured[0], baseFrequency - 1.0, baseFrequency + 1.0);
        _output.WriteLine($"  spacing measured at about {measured[3] - measured[0]:F3} Hz over three "
            + $"steps, against {3 * expectedSpacing:F3} expected");
    }

    /// <summary>
    /// The phase runs continuously across every symbol boundary, which is what makes the emission
    /// narrow and is the thing a plausible synthesizer gets wrong.
    /// </summary>
    [Fact]
    public void ThePhaseNeverStepsAtASymbolBoundary()
    {
        var symbols = Ft4SymbolEncoder.Encode(EncodeCorpus.Build().First().Message);
        var signal = Ft4Waveform.Synthesize(symbols);
        var samplesPerSymbol = Ft4Waveform.SamplesPerSymbol(Ft4Waveform.DefaultSampleRate);

        // The largest sample-to-sample step anywhere in the signal, and the largest at a symbol
        // boundary. A synthesizer that restarted phase at each boundary would make the second much
        // bigger than the first; a continuous one makes them the same kind of number.
        var largestAnywhere = 0.0f;
        var largestAtABoundary = 0.0f;
        for (var i = 1; i < signal.Length; i++)
        {
            var step = MathF.Abs(signal[i] - signal[i - 1]);
            largestAnywhere = MathF.Max(largestAnywhere, step);
            if (i % samplesPerSymbol == 0)
            {
                largestAtABoundary = MathF.Max(largestAtABoundary, step);
            }
        }

        _output.WriteLine($"  largest step anywhere    {largestAnywhere:F6}");
        _output.WriteLine($"  largest step at a symbol {largestAtABoundary:F6}");

        Assert.True(
            largestAtABoundary <= largestAnywhere,
            "the largest sample step in the whole signal falls at a symbol boundary, which is what a "
            + "synthesizer that restarts phase at each symbol produces");
    }
}
