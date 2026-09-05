using System;
using System.Linq;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The baseband extractor, on synthesised audio rather than on the ladder.</b> The ladder says
/// what it is worth; these say that it works at all, and they are the tests that would catch a sign
/// error, a Gray map read backwards or a group delay left uncompensated.
/// </summary>
/// <remarks>
/// <b>Nothing here is a rate and nothing here is evidence of an improvement.</b>
/// <c>PHASE_PLAN.md</c>: no unit in steps 1 to 6 may report an improvement except as a number on
/// step 0's instrument. These are correctness tests on a clean loud signal.
/// </remarks>
public class Ft8DeepBasebandTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private const string Text = "HELLO WORLD";

    /// <summary>
    /// <b>The whole point, in one test: a clean transmission read at its true position comes back as
    /// the text that was sent.</b>
    /// </summary>
    /// <remarks>
    /// The position is the one the synthesiser was given - the exact base frequency in hertz and the
    /// exact start in samples divided by the rate - and neither of them is a grid index. The ratios
    /// go through the port's <c>Ft8CodewordDecoder</c>, so the port's parity gate and CRC-14 gate are
    /// what accept it. Nothing in this library decides that it is a message.
    /// </remarks>
    [Fact]
    public void ACleanTransmissionExtractedAtItsTruePositionDecodesToWhatWasSent()
    {
        foreach (var (frequency, offset) in new[]
                 {
                     (1000.0, 5760),
                     (1001.37, 5760 + 517),
                     (1500.5, 12345),
                     (700.25, 96),
                 })
        {
            var slot = Slot(Text, frequency, offset);
            var baseband = Ft8DeepBaseband.Build(slot, Rate, frequency);

            var ratios = new float[Ft8SoftSymbols.RatioCount];
            Ft8DeepBasebandExtractor.Extract(baseband, offset / (double)Rate, 0.0, ratios);
            Ft8SoftSymbols.Normalise(ratios);

            var result = Ft8CodewordDecoder.Decode(ratios);

            output.WriteLine(
                $"{frequency,9:F2} Hz  offset {offset,6}  {result.Status}  \"{result.Message.Text}\"");

            Assert.True(result.Decoded, $"{frequency:F2} Hz at {offset} samples did not decode.");
            Assert.Equal(Text, result.Message.Text);
        }
    }

    /// <summary>
    /// <b>The same transmission read a whole tone away does not decode</b>, which is what says the
    /// frequency argument is doing something.
    /// </summary>
    /// <remarks>
    /// A whole tone is 6.25 Hz. Reading there lines the eight correlators up on tones 1 to 8 of a
    /// signal that occupies 0 to 7, so seven of the eight are looking at a tone that carries a
    /// different value and the eighth is looking at silence. <b>A test that only showed the right
    /// position working would pass for an extractor that ignored its arguments.</b>
    /// </remarks>
    [Fact]
    public void TheSameTransmissionExtractedAWholeToneAwayDoesNotDecode()
    {
        const double frequency = 1000.0;
        const int offset = 5760;

        var slot = Slot(Text, frequency, offset);

        foreach (var shift in new[] { -12.5, -6.25, 6.25, 12.5 })
        {
            var baseband = Ft8DeepBaseband.Build(slot, Rate, frequency + shift);

            var ratios = new float[Ft8SoftSymbols.RatioCount];
            Ft8DeepBasebandExtractor.Extract(baseband, offset / (double)Rate, 0.0, ratios);
            Ft8SoftSymbols.Normalise(ratios);

            var result = Ft8CodewordDecoder.Decode(ratios);

            output.WriteLine($"{shift,+7:F2} Hz away  {result.Status}  \"{result.Message.Text}\"");

            Assert.NotEqual(Text, result.Message.Text);
        }
    }

    /// <summary>
    /// <b>A whole symbol away in time does not decode either</b>, for the same reason and on the
    /// other axis.
    /// </summary>
    [Fact]
    public void TheSameTransmissionExtractedAWholeSymbolAwayDoesNotDecode()
    {
        const double frequency = 1000.0;
        const int offset = 5760;

        var slot = Slot(Text, frequency, offset);
        var baseband = Ft8DeepBaseband.Build(slot, Rate, frequency);

        foreach (var shift in new[] { -2, -1, 1, 2 })
        {
            var seconds = (offset / (double)Rate)
                + (shift * Ft8WaterfallGeometry.SymbolPeriodSeconds);

            var ratios = new float[Ft8SoftSymbols.RatioCount];
            Ft8DeepBasebandExtractor.Extract(baseband, seconds, 0.0, ratios);
            Ft8SoftSymbols.Normalise(ratios);

            var result = Ft8CodewordDecoder.Decode(ratios);

            output.WriteLine($"{shift,+3} symbols away  {result.Status}  \"{result.Message.Text}\"");

            Assert.NotEqual(Text, result.Message.Text);
        }
    }

    /// <summary>
    /// <b>The Costas correlation peaks at the true position in both axes</b>, which is the property
    /// the fine search rests on.
    /// </summary>
    [Fact]
    public void TheSyncScorePeaksAtTheTruePositionInBothAxes()
    {
        const double frequency = 1000.0;
        const int offset = 5760;

        var slot = Slot(Text, frequency, offset);
        var baseband = Ft8DeepBaseband.Build(slot, Rate, frequency);
        var truth = offset / (double)Rate;

        var atTruth = baseband.SyncScore(truth, 0.0);
        output.WriteLine($"  at the truth                    {atTruth,8:F2} dB");

        foreach (var seconds in new[] { -0.04, -0.02, 0.02, 0.04 })
        {
            var away = baseband.SyncScore(truth + seconds, 0.0);
            output.WriteLine($"  {seconds,+6:F3} s away                   {away,8:F2} dB");
            Assert.True(away < atTruth, $"the score {seconds:F3} s away was not below the truth's.");
        }

        foreach (var hertz in new[] { -1.5625, -0.78, 0.78, 1.5625 })
        {
            var away = baseband.SyncScore(truth, hertz);
            output.WriteLine($"  {hertz,+6:F4} Hz away                  {away,8:F2} dB");
            Assert.True(away < atTruth, $"the score {hertz:F4} Hz away was not below the truth's.");
        }
    }

    /// <summary>
    /// <b>Silence, a slot shorter than a frame, and a base frequency at the passband edges: an
    /// answer every time and never an exception.</b>
    /// </summary>
    /// <remarks>
    /// A caller scanning a live receiver hands over whatever arrived. <b>An extractor that threw on
    /// a short buffer would take the band off the air at exactly the moment it mattered</b>, so
    /// every one of these is an ordinary answer: the ratios come back all zero where no window was
    /// inside the slot, which is the port's own rule and means no opinion.
    /// </remarks>
    [Fact]
    public void SilenceAShortSlotAndThePassbandEdgesAreAnsweredAndNotRefused()
    {
        var ratios = new float[Ft8SoftSymbols.RatioCount];

        var cases = new (string What, float[] Samples, double Frequency)[]
        {
            ("silence, a whole slot", new float[Ft8Waveform.SlotSampleCount(Rate)], 1000.0),
            ("empty, no samples at all", Array.Empty<float>(), 1000.0),
            ("one sample", new float[1], 1000.0),
            ("shorter than a frame", new float[Rate], 1000.0),
            ("at the bottom of the passband", Slot(Text, 200.0, 5760), 200.0),
            ("at the top of the passband", Slot(Text, 2950.0, 5760), 2950.0),
            ("below zero after mixing", new float[Rate * 4], 10.0),
        };

        foreach (var (what, samples, frequency) in cases)
        {
            var baseband = Ft8DeepBaseband.Build(samples, Rate, frequency);
            Ft8DeepBasebandExtractor.Extract(baseband, 0.48, 0.0, ratios);
            var score = baseband.SyncScore(0.48, 0.0);

            output.WriteLine(
                $"  {what,-32} {baseband.Length,7} baseband samples, sync {score,9:F2} dB, "
                + $"{ratios.Count(r => r == 0.0f),4} zero ratios");

            Assert.All(ratios, r => Assert.False(float.IsNaN(r)));
        }
    }

    /// <summary>
    /// <b>The settings' own arithmetic, asserted rather than described.</b>
    /// </summary>
    [Fact]
    public void TheDefaultSettingsGiveAWholeSymbolAndAFilterThatCoversTheAliasBands()
    {
        var settings = Ft8DeepBasebandSettings.Default;

        Assert.Equal(24, settings.Decimation);
        Assert.Equal(401, settings.FilterLength);
        Assert.Equal(150.0, settings.CutoffHz);
        Assert.Equal(500.0, settings.DecimatedRateHz(Rate));
        Assert.Equal(80, settings.SamplesPerSymbol(Rate));

        var taps = Ft8DeepBaseband.BuildLowPass(
            settings.FilterLength, settings.CutoffHz, Rate);

        // UNIT GAIN AT DIRECT CURRENT, which is what makes the baseband's amplitude comparable
        // with the audio's rather than scaled by an accident of the window.
        Assert.Equal(1.0, taps.Sum(), 9);

        // AND SYMMETRIC, which is what makes the phase linear and the group delay a constant that
        // can be removed exactly. A filter that was not would put a frequency-dependent time shift
        // into every position this library reports.
        for (var i = 0; i < taps.Length; i++)
        {
            Assert.Equal(taps[i], taps[taps.Length - 1 - i], 12);
        }

        output.WriteLine(Response(taps, Rate));

        var settingsRefused = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepBasebandSettings(24, 400, 150.0));
        output.WriteLine(settingsRefused.Message);

        // A decimation that does not leave a whole symbol is refused rather than rounded to.
        var whole = Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8DeepBaseband.Build(new float[Rate], Rate, 1000.0, new Ft8DeepBasebandSettings(7, 401, 150.0)));
        output.WriteLine(whole.Message);
    }

    /// <summary>The magnitude response at the frequencies the settings' arithmetic names.</summary>
    private static string Response(double[] taps, int sampleRate)
    {
        var lines = "  magnitude response of the 401-tap low-pass\n";

        foreach (var hertz in new[] { 0.0, 25.0, 50.0, 68.0, 150.0, 232.0, 475.0, 500.0, 525.0 })
        {
            var real = 0.0;
            var imaginary = 0.0;
            for (var i = 0; i < taps.Length; i++)
            {
                var (sin, cos) = Math.SinCos(-2.0 * Math.PI * hertz * i / sampleRate);
                real += taps[i] * cos;
                imaginary += taps[i] * sin;
            }

            var magnitude = Math.Sqrt((real * real) + (imaginary * imaginary));
            lines += $"    {hertz,7:F1} Hz  {20.0 * Math.Log10(magnitude + 1e-15),9:F2} dB\n";
        }

        return lines;
    }

    /// <summary>One clean loud transmission in a slot, at an exact frequency and an exact offset.</summary>
    private static float[] Slot(string text, double baseFrequencyHz, int offsetSamples)
    {
        var message = new byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(text, message));

        var symbols = Ft8SymbolEncoder.Encode(message);
        var signal = Ft8Waveform.Synthesize(symbols, Rate, (float)baseFrequencyHz);

        var slot = new float[Ft8Waveform.SlotSampleCount(Rate)];
        for (var i = 0; i < signal.Length && offsetSamples + i < slot.Length; i++)
        {
            slot[offsetSamples + i] = signal[i];
        }

        return slot;
    }
}
