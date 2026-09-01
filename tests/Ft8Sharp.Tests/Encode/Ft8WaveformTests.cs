using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// What the synthesizer can be held to <em>without</em> upstream — the half of tonight's evidence
/// that survives on a machine that has never heard of <c>ft8_lib</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Every test in this file runs everywhere.</b> Nothing here reads the pinned clone, invokes a
/// binary, or opens a file. The comparison against upstream's own WAV is stronger evidence and it
/// is in <see cref="Ft8WaveformComparisonTests"/>, where it skips on every machine but the one it
/// was measured on. These are what is left when it skips, and they are not nothing: a waveform of
/// the right length, with continuous phase, out of which all seventy-nine symbols can be read back.
/// </para>
/// <para>
/// <b>The one that matters is the tone recovery.</b> Length, range and determinism are the sort of
/// thing a wrong synthesizer passes easily. Measuring the frequency back out of the samples and
/// recovering the symbol sequence is the test that says the audio carries what was put into it.
/// </para>
/// </remarks>
public class Ft8WaveformTests
{
    private readonly ITestOutputHelper _output;

    public Ft8WaveformTests(ITestOutputHelper output) => _output = output;

    /// <summary>A second rate, so nothing can be right by accident at the default one alone.</summary>
    private const int SecondSampleRate = 48000;

    /// <summary>The symbols of one real message, for the tests that need only one.</summary>
    private static byte[] OneMessage()
    {
        var message = new byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message));
        return Ft8SymbolEncoder.Encode(message);
    }

    // ------------------------------------------------------------------ 1. Length

    [Fact]
    public void TheSampleCountIsWhatTheTimingImpliesAtTwoDifferentRates()
    {
        foreach (var rate in new[] { Ft8Waveform.DefaultSampleRate, SecondSampleRate })
        {
            var perSymbol = Ft8Waveform.SamplesPerSymbol(rate);
            var signal = Ft8Waveform.Synthesize(OneMessage(), rate);
            var slot = Ft8Waveform.SynthesizeSlot(OneMessage(), rate);

            _output.WriteLine($"{rate} Hz: {perSymbol} samples per symbol, "
                + $"{signal.Length} of signal, {slot.Length} in the slot, "
                + $"{Ft8Waveform.PaddingSampleCount(rate)} of silence at each end");

            // A symbol lasts the published 0.16 s, so a symbol is that many samples.
            Assert.Equal((int)(rate * 0.16f + 0.5f), perSymbol);

            // Seventy-nine of them, and nothing over.
            Assert.Equal(Ft8Waveform.SymbolCount * perSymbol, signal.Length);
            Assert.Equal(Ft8Waveform.SampleCount(rate), signal.Length);

            // And the slot is the published fifteen seconds exactly.
            Assert.Equal(rate * 15, slot.Length);
            Assert.Equal(Ft8Waveform.SlotSampleCount(rate), slot.Length);

            // The silence is silence, at both ends, and the signal sits between.
            var padding = Ft8Waveform.PaddingSampleCount(rate);
            for (var i = 0; i < padding; i++)
            {
                Assert.Equal(0.0f, slot[i]);
                Assert.Equal(0.0f, slot[slot.Length - 1 - i]);
            }

            Assert.Equal(signal, slot.AsSpan(padding, signal.Length).ToArray());
        }
    }

    // ------------------------------------------------------------------ 2. Range

    [Fact]
    public void EverySampleIsInRangeAndTheSixteenBitConversionNeverWraps()
    {
        // The loudest sequence that can be built: every symbol the top tone, so the frequency sits
        // as high as the modulation puts it and the phase advances fastest.
        var loudest = new byte[Ft8Waveform.SymbolCount];
        Array.Fill(loudest, (byte)(Ft8Waveform.ToneCount - 1));

        var signal = Ft8Waveform.SynthesizeSlot(loudest);
        var pcm = Ft8Waveform.SynthesizeSlotPcm16(loudest);

        var peak = 0.0f;
        foreach (var sample in signal)
        {
            Assert.False(float.IsNaN(sample), "a sample came out NaN.");
            Assert.InRange(sample, -1.0f, 1.0f);
            peak = MathF.Max(peak, MathF.Abs(sample));
        }

        var peakCount = 0;
        foreach (var sample in pcm)
        {
            peakCount = Math.Max(peakCount, Math.Abs((int)sample));
        }

        _output.WriteLine($"peak of the signal      : {peak}");
        _output.WriteLine($"peak in counts          : {peakCount}");
        Assert.Equal(signal.Length, pcm.Length);
        Assert.True(peakCount <= short.MaxValue, "the conversion produced a count out of range.");

        // A wrap shows as a large positive next to a large negative, so the sign of every loud
        // sample must agree with the sign of the sample it came from.
        for (var i = 0; i < signal.Length; i++)
        {
            if (MathF.Abs(signal[i]) > 0.5f)
            {
                Assert.Equal(signal[i] > 0, pcm[i] > 0);
            }
        }

        // And the conversion is fed past both ends of its range on purpose, because a clip that has
        // never been asked to clip is not a clip.
        var forced = Ft8Waveform.ToPcm16(new[] { 2.0f, -2.0f, 1.0f, -1.0f, 0.0f, float.MaxValue, float.MinValue });
        _output.WriteLine($"clipped                 : [{string.Join(", ", forced)}]");
        Assert.Equal(short.MaxValue, forced[0]);
        Assert.Equal(forced[0], forced[2]);
        Assert.Equal(forced[1], forced[3]);
        Assert.Equal(forced[0], forced[5]);
        Assert.Equal(forced[1], forced[6]);
        Assert.Equal(0, forced[4]);
        foreach (var value in forced)
        {
            Assert.InRange((int)value, short.MinValue, short.MaxValue);
        }
    }

    // ------------------------------------------------------------------ 3. Phase continuity

    /// <summary>
    /// The test that catches a port which restarts phase at each symbol.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Such a port produces a waveform of exactly the right length, carrying exactly the right
    /// frequencies, out of which every one of the seventy-nine symbols can still be recovered — so
    /// the length test, the range test and even the tone recovery all pass. What it is not is FT8:
    /// the discontinuities splatter energy across the band. This is the only test here that would
    /// go red.
    /// </para>
    /// <para>
    /// <b>The bound is named and it is derived, not chosen to fit.</b> Between two adjacent samples
    /// a sinusoid at the highest frequency in the signal advances by at most one sample's worth of
    /// phase, so the sample can change by at most that much times the slope of a sine, which is one.
    /// The bound below is that quantity with a factor of two of headroom, and the test reports the
    /// largest step it actually found both at symbol boundaries and everywhere else, so the two can
    /// be compared rather than taken on trust.
    /// </para>
    /// </remarks>
    [Fact]
    public void PhaseIsContinuousAcrossEverySymbolBoundary()
    {
        var symbols = OneMessage();
        var rate = Ft8Waveform.DefaultSampleRate;
        var signal = Ft8Waveform.Synthesize(symbols, rate);
        var perSymbol = Ft8Waveform.SamplesPerSymbol(rate);

        var topFrequency = Ft8Waveform.DefaultBaseFrequency
            + ((Ft8Waveform.ToneCount - 1) * Ft8Waveform.ToneSpacingHz);
        var bound = 2.0f * (2.0f * MathF.PI * topFrequency / rate);

        var worstAtABoundary = 0.0f;
        var worstBoundary = -1;
        var worstElsewhere = 0.0f;

        for (var i = 1; i < signal.Length; i++)
        {
            var step = MathF.Abs(signal[i] - signal[i - 1]);
            if (i % perSymbol == 0)
            {
                if (step > worstAtABoundary)
                {
                    worstAtABoundary = step;
                    worstBoundary = i / perSymbol;
                }
            }
            else if (step > worstElsewhere)
            {
                worstElsewhere = step;
            }
        }

        _output.WriteLine($"bound                   : {bound}");
        _output.WriteLine($"largest step at a symbol boundary : {worstAtABoundary} (boundary {worstBoundary})");
        _output.WriteLine($"largest step anywhere else        : {worstElsewhere}");

        Assert.True(
            worstAtABoundary <= bound,
            $"the sample-to-sample step at symbol boundary {worstBoundary} is {worstAtABoundary}, "
            + $"past the bound of {bound}. A step that size at a symbol boundary and not between "
            + "them is phase being restarted at each symbol rather than accumulated across the "
            + "whole transmission — a signal the right length, at the right frequencies, with the "
            + "wrong shape.");

        // The stronger statement: boundaries are not special. If phase were restarted, the steps at
        // boundaries would stand out against the steps everywhere else, and this is what says they
        // do not.
        Assert.True(
            worstAtABoundary <= worstElsewhere * 1.5f,
            $"the largest step at a symbol boundary is {worstAtABoundary} and the largest step "
            + $"anywhere else is {worstElsewhere}. Boundaries should be indistinguishable from the "
            + "middle of a symbol, and these are not.");
    }

    // ------------------------------------------------------------------ 4. Tone recovery

    /// <summary>
    /// Measures the frequency back out of the waveform and recovers all seventy-nine symbols, for
    /// every message in the corpus. <b>This is the self-evidence that the audio carries what was
    /// put into it.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The estimator is not a Fourier transform, on purpose.</b> The tones are 6.25 Hz apart and
    /// a symbol lasts 0.16 s, so a transform over one symbol resolves them to exactly one bin —
    /// there is no margin, and the pulse shaping smears energy across neighbours. Instead this uses
    /// the recurrence a sampled sinusoid obeys, x[n+1] + x[n-1] = 2·cos(ω)·x[n], solved in the least
    /// squares sense over the settled middle of each symbol. That gives the frequency directly and
    /// far more finely than the tones are spaced.
    /// </para>
    /// <para>
    /// <b>Why the middle half of the symbol.</b> The smoothing pulse reaches exactly one at the
    /// centre of its own symbol and has fallen to nothing a whole symbol away, so at the centre the
    /// frequency is the symbol's own tone and neighbours contribute nothing measurable. A quarter of
    /// a symbol either side of the centre is still inside that settled region, and it stays clear of
    /// the envelope ramp on the first and last symbols, which occupies an eighth of one.
    /// </para>
    /// </remarks>
    [Fact]
    public void EverySymbolOfEveryMessageIsRecoveredBackOutOfTheWaveform()
    {
        var rate = Ft8Waveform.DefaultSampleRate;
        var perSymbol = Ft8Waveform.SamplesPerSymbol(rate);
        var corpus = EncodeCorpus.Build();

        var messages = 0;
        var recovered = 0;
        var messagesWhole = 0;
        var worstError = 0.0;
        var worstAt = string.Empty;

        foreach (var entry in corpus)
        {
            var symbols = Ft8SymbolEncoder.Encode(entry.Message);
            var signal = Ft8Waveform.Synthesize(symbols, rate);

            var whole = true;
            for (var s = 0; s < Ft8Waveform.SymbolCount; s++)
            {
                var from = (s * perSymbol) + (perSymbol / 4);
                var count = perSymbol / 2;
                var frequency = EstimateFrequency(signal, from, count, rate);
                var exact = Ft8Waveform.DefaultBaseFrequency
                    + (symbols[s] * Ft8Waveform.ToneSpacingHz);
                var error = Math.Abs(frequency - exact);
                if (error > worstError)
                {
                    worstError = error;
                    worstAt = $"{entry.Label}, symbol {s}";
                }

                var read = (int)Math.Round(
                    (frequency - Ft8Waveform.DefaultBaseFrequency) / Ft8Waveform.ToneSpacingHz);
                if (read == symbols[s])
                {
                    recovered++;
                }
                else
                {
                    whole = false;
                    _output.WriteLine(
                        $"    {entry.Label}: symbol {s} was put in as {symbols[s]} and read back as "
                        + $"{read} ({frequency:F3} Hz against {exact:F3} Hz)");
                }
            }

            messages++;
            if (whole)
            {
                messagesWhole++;
            }
        }

        _output.WriteLine($"messages synthesized    : {messages}");
        _output.WriteLine($"messages whose every symbol came back : {messagesWhole}");
        _output.WriteLine($"symbols recovered       : {recovered} of {messages * Ft8Waveform.SymbolCount}");
        _output.WriteLine($"worst frequency error   : {worstError:F6} Hz at {worstAt}");
        _output.WriteLine($"tone spacing            : {Ft8Waveform.ToneSpacingHz} Hz");

        Assert.Equal(corpus.Count, messages);
        Assert.Equal(messages * Ft8Waveform.SymbolCount, recovered);
        Assert.Equal(messages, messagesWhole);

        // The margin, stated rather than implied: the worst frequency error must be a small
        // fraction of the tone spacing, or the recovery is passing on rounding luck.
        Assert.True(
            worstError < Ft8Waveform.ToneSpacingHz / 4,
            $"the worst frequency error is {worstError:F6} Hz against a tone spacing of "
            + $"{Ft8Waveform.ToneSpacingHz} Hz, at {worstAt}. Every symbol still rounded to the "
            + "right tone, but with that little margin the recovery is luck rather than evidence.");
    }

    /// <summary>
    /// The frequency of a windowed stretch of a sampled sinusoid, from the recurrence it obeys.
    /// </summary>
    private static double EstimateFrequency(float[] signal, int from, int count, int sampleRate)
    {
        double numerator = 0;
        double denominator = 0;
        for (var i = from; i < from + count; i++)
        {
            double x = signal[i];
            numerator += x * (signal[i + 1] + signal[i - 1]);
            denominator += 2 * x * x;
        }

        var cosine = Math.Clamp(numerator / denominator, -1.0, 1.0);
        return Math.Acos(cosine) * sampleRate / (2 * Math.PI);
    }

    // ------------------------------------------------------------------ 5. Determinism

    [Fact]
    public void TwoCallsWithTheSameArgumentsProduceIdenticalBuffers()
    {
        var symbols = OneMessage();

        var first = Ft8Waveform.Synthesize(symbols);
        var second = Ft8Waveform.Synthesize(symbols);
        Assert.NotSame(first, second);
        Assert.Equal(first, second);

        // Byte-identical, not merely equal to a tolerance — nothing here reads a clock, a random
        // source or any ambient state, so anything less than identity would be a defect.
        Assert.Equal(
            System.Runtime.InteropServices.MemoryMarshal.AsBytes(first.AsSpan()).ToArray(),
            System.Runtime.InteropServices.MemoryMarshal.AsBytes(second.AsSpan()).ToArray());

        Assert.Equal(
            Ft8Waveform.SynthesizeSlotPcm16(symbols),
            Ft8Waveform.SynthesizeSlotPcm16(symbols));

        // And a different argument really does produce a different buffer, so the equality above is
        // not the equality of two things that ignore their inputs.
        Assert.NotEqual(first, Ft8Waveform.Synthesize(symbols, baseFrequency: 1500f));
        _output.WriteLine($"identical over {first.Length} samples, and different at another frequency");
    }

    // ------------------------------------------------------------------ 6. Watched refusing

    [Fact]
    public void ASymbolCountThatIsNotSeventyNineIsRefused()
    {
        foreach (var length in new[] { 0, 78, 80, 158 })
        {
            var thrown = Assert.Throws<ArgumentException>(
                () => Ft8Waveform.Synthesize(new byte[length]));
            _output.WriteLine($"{length,4} symbols: {thrown.Message.Split('.')[0]}");
            Assert.Contains(Ft8Waveform.SymbolCount.ToString(), thrown.Message);
            Assert.Contains(length.ToString(), thrown.Message);
        }
    }

    [Fact]
    public void ASymbolOutsideTheEightToneAlphabetIsRefused()
    {
        foreach (var (at, tone) in new[] { (0, (byte)8), (40, (byte)9), (78, byte.MaxValue) })
        {
            var symbols = OneMessage();
            symbols[at] = tone;
            var thrown = Assert.Throws<ArgumentException>(() => Ft8Waveform.Synthesize(symbols));
            _output.WriteLine($"symbol {at} = {tone}: {thrown.Message.Split('.')[0]}");
            Assert.Contains($"symbol {at}", thrown.Message);
            Assert.Contains(tone.ToString(), thrown.Message);
        }
    }

    [Fact]
    public void ASampleRateThatIsNotPositiveIsRefused()
    {
        foreach (var rate in new[] { 0, -1, -12000 })
        {
            var thrown = Assert.Throws<ArgumentOutOfRangeException>(
                () => Ft8Waveform.Synthesize(OneMessage(), rate));
            _output.WriteLine($"{rate,7} Hz: {thrown.Message.Split('.')[0]}");
            Assert.Equal("sampleRate", thrown.ParamName);
            Assert.Contains("samples per second", thrown.Message);
        }
    }

    [Fact]
    public void ABaseFrequencyThatWouldPutAToneOutsideTheChannelIsRefused()
    {
        // Above Nyquist at the top.
        var high = Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8Waveform.Synthesize(OneMessage(), 12000, 5999f));
        _output.WriteLine($"5999 Hz at 12000 Hz : {high.Message.Split('.')[0]}");
        Assert.Equal("baseFrequency", high.ParamName);
        Assert.Contains("Nyquist", high.Message);

        // And a rate at which a perfectly ordinary frequency no longer fits, so the refusal is
        // about the pair rather than about one magic number.
        var narrow = Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8Waveform.Synthesize(OneMessage(), 1000, Ft8Waveform.DefaultBaseFrequency));
        Assert.Contains("Nyquist", narrow.Message);

        // Below zero at the bottom.
        foreach (var frequency in new[] { 0f, -1f, -1000f })
        {
            var low = Assert.Throws<ArgumentOutOfRangeException>(
                () => Ft8Waveform.Synthesize(OneMessage(), 12000, frequency));
            _output.WriteLine($"{frequency,8} Hz : {low.Message.Split('.')[0]}");
            Assert.Equal("baseFrequency", low.ParamName);
            Assert.Contains("DC", low.Message);
        }
    }

    /// <summary>
    /// The guard on the geometry upstream leaves implicit — a rate at which the signal's two
    /// lengths disagree.
    /// </summary>
    [Fact]
    public void ASampleRateAtWhichTheSignalsTwoLengthsDisagreeIsRefused()
    {
        var thrown = Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8Waveform.Synthesize(OneMessage(), 12001));
        _output.WriteLine($"12001 Hz: {thrown.Message.Split('.')[0]}");
        Assert.Equal("sampleRate", thrown.ParamName);
        Assert.Contains("wrong offset", thrown.Message);

        // And the rate the modulation is actually used at is not refused, which is what says the
        // guard is a guard rather than an obstacle.
        Assert.Equal(
            Ft8Waveform.SymbolCount * Ft8Waveform.SamplesPerSymbol(Ft8Waveform.DefaultSampleRate),
            Ft8Waveform.SampleCount(Ft8Waveform.DefaultSampleRate));
    }
}
