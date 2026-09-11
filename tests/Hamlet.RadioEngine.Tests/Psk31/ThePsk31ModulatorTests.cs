using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Tests.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 318 task 2: **Hamlet's own PSK31 voice, read back by Hamlet's own ear.**
/// </summary>
/// <remarks>
/// <para>**THE LOOPBACK IS THE WHOLE PROOF AND IT IS EXACT.** Each of §R2's four macros is
/// modulated by <see cref="Psk31Modulator"/> and handed to a fresh
/// <see cref="Psk31Demodulator"/>, and the text that comes back must be the text that went
/// in, character for character. Not an error rate and not a ceiling: a clean signal that
/// did not read back identically would be a modulator and a demodulator disagreeing about
/// the mode, and the air would not forgive that.</para>
/// <para>**THE SIGNAL IS MEASURED, NOT DESCRIBED.** The bandwidth comes off the tree's own
/// <see cref="RealFft"/>, the symbol length comes off the envelope's minima, and the peak
/// comes off the samples. No package (§0.4).</para>
/// <para>**COMPUTED, NOT SEEN, AND NO RADIO WAS INVOLVED** (FACT-004, FACT-006). Every
/// number here is synthetic and from the bench.</para>
/// </remarks>
public sealed class ThePsk31ModulatorTests
{
    private const string Mine = "KC3QIS";
    private const string His = "W1AW";
    private const string Name = "Tim";
    private const string Qth = "Trafford PA";

    /// <summary>Six characters, as Settings holds a grid in the app's tests.</summary>
    private const string SettingsGrid = "FN00DJ";

    /// <summary>A drive level that is not unit amplitude, so a peak test means something.</summary>
    private const float Peak = 0.5f;

    /// <summary>R10's ceiling on a send with no slot. Task 3 states the cap under it.</summary>
    private const double RulingCeilingSeconds = 30.0;

    /// <summary>**§R2's four texts as the plan writes them**, with W1AW as the other station.</summary>
    private static readonly (string Name, string Text)[] RulingTexts =
    [
        ("CQ", "CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K"),
        ("Answer", "W1AW de KC3QIS KC3QIS K"),
        ("Report", "W1AW de KC3QIS  RST 599 599  Name Tim Tim  QTH Trafford PA  Grid FN00 FN00  BTU W1AW de KC3QIS K"),
        ("Confirm", "W1AW de KC3QIS  R R  TNX for the QSO  73 73  W1AW de KC3QIS SK"),
    ];

    /// <summary>Instruction 317's arithmetic, where it gave one.</summary>
    private static readonly Dictionary<string, string> ArbiterSeconds = new(StringComparer.Ordinal)
    {
        ["Report"] = "23.2",
        ["Confirm"] = "14.7",
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the texts, widths and table are printed.</param>
    public ThePsk31ModulatorTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The macros are §R2's texts to the space.**</summary>
    [Fact]
    public void TheFourMacrosAreTheRulingsTextsToTheSpace()
    {
        var made = Macros();

        for (var i = 0; i < RulingTexts.Length; i++)
        {
            _output.WriteLine($"{made[i].Name,-8} \"{made[i].Text}\"");
            Assert.Equal(RulingTexts[i].Name, made[i].Name);
            Assert.Equal(RulingTexts[i].Text, made[i].Text);
        }
    }

    /// <summary>**Must-pass 1: each macro comes back identical.**</summary>
    /// <param name="sampleRate">Samples a second.</param>
    /// <param name="offsetHz">Where the carrier sits.</param>
    [Theory]
    [InlineData(48_000, 1000.0)]
    [InlineData(48_000, 1500.0)]
    [InlineData(8_000, 1000.0)]
    public void EachMacroComesBackIdenticalThroughHamletsOwnDemodulator(int sampleRate, double offsetHz)
    {
        var wrong = new List<string>();

        foreach (var (name, text) in Macros())
        {
            Assert.Equal(RulingTexts.Single(r => r.Name == name).Text, text);

            var samples = Psk31Modulator.Modulate(text, sampleRate, offsetHz, Peak);
            var read = new Psk31Demodulator(sampleRate, offsetHz).Add(samples);
            var same = string.Equals(text, read, StringComparison.Ordinal);

            _output.WriteLine($"{name,-8} at {sampleRate} Hz, {offsetHz:0} Hz: {(same ? "identical" : "DIFFERENT")}");
            _output.WriteLine($"  sent : \"{text}\"");
            _output.WriteLine($"  read : \"{read}\"");

            if (!same)
            {
                wrong.Add(name);
            }
        }

        Assert.Empty(wrong);
    }

    /// <summary>**Must-pass 2: under 100 Hz wide at -30 dB, measured.**</summary>
    /// <remarks>
    /// **HOW IT IS MEASURED.** The Report macro, the longest, at 48 000 Hz with the carrier
    /// at 1000 Hz. Welch's average: a periodic Hann window of 32 768 samples (1.46 Hz a bin),
    /// half-overlapping, power averaged over every whole window in the signal. The width at a
    /// level is the distance between the lowest and the highest bin at or above that many
    /// decibels below the spectral peak, so a stray lobe outside the main one counts against
    /// it rather than being missed.
    /// </remarks>
    [Fact]
    public void TheLongestMacroIsUnderAHundredHertzWideThirtyDecibelsDown()
    {
        const int Rate = 48_000;
        const double Offset = 1000;
        const int Size = 32_768;

        var report = Macros().Single(m => m.Name == "Report").Text;
        var samples = Psk31Modulator.Modulate(report, Rate, Offset, Peak);
        Assert.True(samples.Length >= Size, "the Report macro is shorter than one window");

        var fft = new RealFft(Size);
        var power = new double[fft.BinCount];
        var magnitudes = new double[fft.BinCount];
        var real = new double[Size];
        var imaginary = new double[Size];
        var frame = new float[Size];
        var segments = 0;

        for (var start = 0; start + Size <= samples.Length; start += Size / 2)
        {
            for (var i = 0; i < Size; i++)
            {
                var hann = 0.5 - (0.5 * Math.Cos(2 * Math.PI * i / Size));
                frame[i] = (float)(samples[start + i] * hann);
            }

            fft.Magnitudes(frame, magnitudes, real, imaginary);

            for (var bin = 0; bin < power.Length; bin++)
            {
                power[bin] += magnitudes[bin] * magnitudes[bin];
            }

            segments++;
        }

        var peakBin = Array.IndexOf(power, power.Max());

        double Width(double decibels)
        {
            var threshold = power[peakBin] * Math.Pow(10, -decibels / 10);
            var low = Array.FindIndex(power, p => p >= threshold);
            var high = Array.FindLastIndex(power, p => p >= threshold);

            return fft.BinHz(high, Rate) - fft.BinHz(low, Rate);
        }

        var at6 = Width(6);
        var at20 = Width(20);
        var at30 = Width(30);

        _output.WriteLine($"signal     : Report macro, {samples.Length} samples at {Rate} Hz, carrier {Offset:0} Hz");
        _output.WriteLine($"window     : periodic Hann, {Size} samples, {fft.BinHz(1, Rate):0.00} Hz a bin, half overlap, {segments} windows power-averaged");
        _output.WriteLine($"peak       : {fft.BinHz(peakBin, Rate):0.00} Hz");
        _output.WriteLine($"width  -6 dB: {at6:0.0} Hz");
        _output.WriteLine($"width -20 dB: {at20:0.0} Hz");
        _output.WriteLine($"width -30 dB: {at30:0.0} Hz   (must be under 100)");

        Assert.True(at30 < 100, $"the -30 dB width is {at30:0.0} Hz");
    }

    /// <summary>
    /// **Must-pass 3: 31.25 baud from the waveform, and the envelope dips at every reversal
    /// and nowhere else.**
    /// </summary>
    [Fact]
    public void TheSymbolIsThirtyOnePointTwoFiveBaudAndTheEnvelopeDipsOnlyAtReversals()
    {
        const int Rate = 48_000;
        const double Offset = 1000;

        var text = Macros().Single(m => m.Name == "Report").Text;
        var samples = Psk31Modulator.Modulate(text, Rate, Offset, Peak);
        var envelope = Envelope(samples, Rate, Offset);
        var expected = Rate / Psk31Demodulator.Baud;

        // FROM THE WAVEFORM ALONE: every local minimum under a twentieth of the peak, with
        // any two closer than half a symbol taken as one.
        var minima = new List<int>();

        for (var n = 1; n < envelope.Length - 1; n++)
        {
            if (envelope[n] < 0.05 && envelope[n] <= envelope[n - 1] && envelope[n] < envelope[n + 1])
            {
                if (minima.Count > 0 && n - minima[^1] < expected / 2)
                {
                    if (envelope[n] < envelope[minima[^1]])
                    {
                        minima[^1] = n;
                    }

                    continue;
                }

                minima.Add(n);
            }
        }

        // The idle before the text is a reversal on every symbol, so its minima are one
        // symbol apart and nothing else is.
        var idle = minima.Take(Psk31Modulator.IdleBitsBefore).ToList();
        var spacing = (idle[^1] - idle[0]) / (double)(idle.Count - 1);

        _output.WriteLine($"minima in the waveform      : {minima.Count}");
        _output.WriteLine($"spacing over the idle ({idle.Count}) : {spacing:0.000} samples");
        _output.WriteLine($"sampleRate / 31.25          : {expected:0.000} samples");
        _output.WriteLine($"measured rate               : {Rate / spacing:0.0000} baud");

        Assert.InRange(spacing, expected - 1, expected + 1);

        // AND AGAINST THE BITS THAT WENT OUT. A reversal is a dip to nothing at its
        // boundary; no change is an envelope that stays at the peak across the boundary.
        var bits = Psk31Modulator.BitsFor(text);
        var margin = (int)(expected / 16);
        var reversals = 0;
        var holds = 0;
        var failures = new List<string>();

        for (var k = 1; k <= bits.Length; k++)
        {
            var at = (int)Math.Round(k * expected);

            if (bits[k - 1] == '0')
            {
                reversals++;

                if (envelope[at] > 0.03)
                {
                    failures.Add($"boundary {k}: a reversal whose envelope is {envelope[at]:0.000} of the peak");
                }
            }
            else
            {
                holds++;

                var from = at - (int)(expected / 2) + margin;
                var to = at + (int)(expected / 2) - margin;
                var lowest = envelope[from..to].Min();

                if (lowest < 0.97)
                {
                    failures.Add($"boundary {k}: no change, and the envelope fell to {lowest:0.000} of the peak");
                }
            }
        }

        _output.WriteLine($"reversals, each a dip       : {reversals}");
        _output.WriteLine($"no-change boundaries, flat  : {holds}");

        foreach (var failure in failures.Take(10))
        {
            _output.WriteLine("  " + failure);
        }

        Assert.Empty(failures);
    }

    /// <summary>**Must-pass 4: the peak never exceeds the peak asked for.**</summary>
    /// <param name="peak">The drive level handed in.</param>
    [Theory]
    [InlineData(1.0f)]
    [InlineData(0.5f)]
    [InlineData(0.25f)]
    [InlineData(0.1f)]
    public void ThePeakNeverExceedsThePeakAskedFor(float peak)
    {
        var loudest = 0f;

        foreach (var (_, text) in Macros())
        {
            foreach (var sample in Psk31Modulator.Modulate(text, 48_000, 1000, peak))
            {
                loudest = Math.Max(loudest, Math.Abs(sample));
            }
        }

        _output.WriteLine($"asked {peak:0.000}, loudest sample {loudest:0.000000}");

        Assert.True(loudest <= peak, $"a sample of {loudest} over a peak of {peak}");
        Assert.True(loudest >= 0.99f * peak, $"the loudest sample is {loudest}, which is not the level asked for");
    }

    /// <summary>**Must-pass 5: the table task 3's cap is chosen from.**</summary>
    [Fact]
    public void TheTableOfTheFourMacros()
    {
        const int Rate = 48_000;

        _output.WriteLine("macro     chars  varicode bits  text s  with idle s  instruction 317");

        var rows = 0;

        foreach (var (name, text) in Macros())
        {
            var bits = Varicode.Encode(text).Length;
            var textSeconds = bits / Psk31Demodulator.Baud;
            var withIdle = Psk31Modulator.Modulate(text, Rate, 1000, Peak).Length / (double)Rate;
            var arbiter = ArbiterSeconds.TryGetValue(name, out var said) ? said + " s" : "not stated";

            _output.WriteLine(
                $"{name,-8}  {text.Length,5}  {bits,13}  {textSeconds,6:0.00}  {withIdle,11:0.00}  {arbiter}");

            Assert.True(withIdle > textSeconds, $"{name}: {withIdle} s with idle is not longer than {textSeconds} s of text");
            rows++;
        }

        _output.WriteLine(
            $"idle: {Psk31Modulator.IdleBitsBefore} bits before, {Psk31Modulator.IdleBitsAfter} after, "
            + "and half a symbol of ramp at each end");

        Assert.Equal(4, rows);
    }

    /// <summary>**Nice-to-pass 6: the Report to a compound callsign, against the ceiling.**</summary>
    [Fact]
    public void TheReportToACompoundCallsignFitsUnderTheRulingsCeiling()
    {
        const int Rate = 48_000;

        var report = Psk31Macros.Report("VP2V/W1AW", Mine, Name, Qth, SettingsGrid);
        var withIdle = Psk31Modulator.Modulate(report, Rate, 1000, Peak).Length / (double)Rate;

        _output.WriteLine($"text      : \"{report}\"");
        _output.WriteLine($"with idle : {withIdle:0.00} s against R10's ceiling of {RulingCeilingSeconds:0} s");

        Assert.True(withIdle <= RulingCeilingSeconds, $"{withIdle:0.00} s");
    }

    /// <summary>**The idle is longer than the shortest that still reads back, measured.**</summary>
    /// <param name="sampleRate">Samples a second.</param>
    /// <remarks>
    /// Walked down one bit at a time from the stated length while every macro in the set
    /// still reads back identically, so the shortest reported is the shortest of an unbroken
    /// run and not a lucky value below a failing one.
    /// </remarks>
    [Theory]
    [InlineData(48_000)]
    [InlineData(8_000)]
    public void TheIdleEitherSideIsLongerThanTheShortestThatStillReadsBack(int sampleRate)
    {
        var set = Macros().Where(m => m.Name is "CQ" or "Answer" or "Confirm").ToList();

        Assert.True(
            ReadsBack(set, sampleRate, Psk31Modulator.IdleBitsBefore, Psk31Modulator.IdleBitsAfter),
            "the stated idle does not read back");

        var before = Psk31Modulator.IdleBitsBefore;

        while (before > 0 && ReadsBack(set, sampleRate, before - 1, Psk31Modulator.IdleBitsAfter))
        {
            before--;
        }

        var after = Psk31Modulator.IdleBitsAfter;

        while (after > 0 && ReadsBack(set, sampleRate, Psk31Modulator.IdleBitsBefore, after - 1))
        {
            after--;
        }

        _output.WriteLine($"at {sampleRate} Hz");
        _output.WriteLine($"  before: shortest that reads back {before} bits, stated {Psk31Modulator.IdleBitsBefore}");
        _output.WriteLine($"  after : shortest that reads back {after} bits, stated {Psk31Modulator.IdleBitsAfter}");

        Assert.True(Psk31Modulator.IdleBitsBefore > before, "no margin before the text");
        Assert.True(Psk31Modulator.IdleBitsAfter > after, "no margin after the text");
    }

    /// <summary>**No callsign is written into the modulator or the macros.**</summary>
    [Fact]
    public void NoCallsignIsWrittenIntoTheModulatorOrTheMacros()
    {
        foreach (var file in new[] { "Psk31Modulator.cs", "Psk31Macros.cs" })
        {
            var path = Path.Combine(
                TheUnkeyHappensWhateverGoesWrongTests.RepositoryRoot(),
                "src", "Hamlet.RadioEngine", "Psk31", file);

            var source = File.ReadAllText(path);

            _output.WriteLine($"{file}: {source.Length} characters read");

            Assert.DoesNotContain(Mine, source, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(His, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>The four macros, filled from test values.</summary>
    private static IReadOnlyList<(string Name, string Text)> Macros() =>
    [
        ("CQ", Psk31Macros.Cq(Mine)),
        ("Answer", Psk31Macros.Answer(His, Mine)),
        ("Report", Psk31Macros.Report(His, Mine, Name, Qth, SettingsGrid)),
        ("Confirm", Psk31Macros.Confirm(His, Mine)),
    ];

    /// <summary>Whether every macro in the set reads back identically with this idle.</summary>
    private static bool ReadsBack(
        IEnumerable<(string Name, string Text)> set, int sampleRate, int before, int after)
        => set.All(macro =>
        {
            var samples = Psk31Modulator.Modulate(macro.Text, sampleRate, 1000, Peak, before, after);

            return string.Equals(
                macro.Text,
                new Psk31Demodulator(sampleRate, 1000).Add(samples),
                StringComparison.Ordinal);
        });

    /// <summary>The envelope as a fraction of the peak asked for.</summary>
    /// <remarks>
    /// Mixed down at the carrier and averaged over one carrier cycle, centred, which cancels
    /// the term at twice the carrier exactly and blurs the envelope by a millisecond against
    /// a 32 ms symbol.
    /// </remarks>
    private static double[] Envelope(float[] samples, int rate, double offset)
    {
        var cycle = (int)Math.Round(rate / offset);
        var w = 2 * Math.PI * offset / rate;
        var sumReal = new double[samples.Length + 1];
        var sumImaginary = new double[samples.Length + 1];

        for (var n = 0; n < samples.Length; n++)
        {
            sumReal[n + 1] = sumReal[n] + (samples[n] * Math.Cos(w * n));
            sumImaginary[n + 1] = sumImaginary[n] - (samples[n] * Math.Sin(w * n));
        }

        var envelope = new double[samples.Length];

        for (var n = 0; n < samples.Length; n++)
        {
            var from = Math.Max(0, n - (cycle / 2));
            var to = Math.Min(samples.Length, from + cycle);
            var count = to - from;
            var re = (sumReal[to] - sumReal[from]) / count;
            var im = (sumImaginary[to] - sumImaginary[from]) / count;

            envelope[n] = 2 * Math.Sqrt((re * re) + (im * im)) / Peak;
        }

        return envelope;
    }
}
