using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 344 task 2: **do either of the two hypotheses reproduce the garble?**
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, as the instruction asks. It prints a
/// table and fails on nothing, so it can never turn a hypothesis into a wall.</para>
/// <para>**THE ROW IT IS TRYING TO REPRODUCE**, from 7.070 on 2026-09-13:
/// `dt  oe epe Ae peey@teI` - the short varicodes surviving and the long ones breaking,
/// which is what a PSK31 decoder makes when most bits are right and a few are wrong.</para>
/// <para>**THE SKIRT ARM IS MODULATOR-MADE AND THE FADE ARM IS THE SHIPPED FIXTURE**, and
/// the difference matters (§12.5). The fade needs no new carrier, so it runs on
/// `psk31-clean-1000hz.wav`, made by `reference-modem.py` - a second implementation, which
/// is what makes a fixture able to judge this one. **The skirt needs the carrier at 330 Hz
/// and no such fixture exists**, so both of its arms are made here by `Psk31Modulator`.
/// That makes the skirt a comparison between two arms of one round trip rather than a
/// measurement against an independent reference: what it can show is 330 Hz against
/// 1000 Hz through the same code, which is the question, and what it cannot show is an
/// absolute error rate. The unfiltered reference run is printed beside them as the
/// anchor.</para>
/// <para>**COMPUTED, NOT SEEN**, and none of this is evidence about the radio: synthetic
/// audio on a machine with none (FACT-004, FACT-006). **A hypothesis stays a hypothesis
/// until a real capture settles it** (§0.0), which is what task 1 was built for.</para>
/// </remarks>
public sealed class Unit344Measure
{
    /// <summary>Where the filter is three decibels down, as the instruction sets it.</summary>
    private const double CornerHz = 300;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the measurement.</summary>
    /// <param name="output">Where the table is printed.</param>
    public Unit344Measure(ITestOutputHelper output) => _output = output;

    /// <summary>Print what the skirt and the fade do to the text.</summary>
    [Fact]
    public void NeitherHypothesisIsAssertedAndBothAreMeasured()
    {
        var reference = WavAudio.Read(Fixture("psk31-clean-1000hz.wav"));
        var want = ManifestText("psk31-clean-1000hz.wav");
        var rate = reference.SampleRate;

        _output.WriteLine(
            "the reference text is " + want.Length + " characters at "
            + rate.ToString(CultureInfo.InvariantCulture) + " Hz; the filter is a "
            + "4th-order Butterworth high-pass, -3 dB at "
            + CornerHz.ToString("0", CultureInfo.InvariantCulture) + " Hz");

        Head("anchor - the shipped reference fixture, reference-modem.py, no filter");
        Row("reference at 1000 Hz", Decode(reference.Samples, rate, 1000), want);

        Head("skirt - Psk31Modulator's own audio, both arms through the same code");

        var low = Psk31Modulator.Modulate(want, rate, 330, 0.5f);
        var high = Psk31Modulator.Modulate(want, rate, 1000, 0.5f);

        Row("330 Hz, no filter", Decode(low, rate, 330), want);
        Row("330 Hz, high-passed", Decode(HighPass(low, rate), rate, 330), want);
        Row("1000 Hz, no filter (control)", Decode(high, rate, 1000), want);
        Row("1000 Hz, high-passed (control)", Decode(HighPass(high, rate), rate, 1000), want);

        // **IS THE INSTRUMENT ON?** A filter that changes nothing and a hypothesis that is
        // wrong look identical in the table above (12.5), so the filter is measured on the
        // audio it was applied to before anything is concluded from it.
        _output.WriteLine("");
        _output.WriteLine(
            "   the high-pass takes the 330 Hz arm to "
            + Gain(low, HighPass(low, rate)).ToString("0.00", CultureInfo.InvariantCulture)
            + " dB and the 1000 Hz arm to "
            + Gain(high, HighPass(high, rate)).ToString("0.00", CultureInfo.InvariantCulture)
            + " dB");

        Head("skirt - how far up the carrier has to sit before it breaks, 330 Hz arm");

        foreach (var corner in new double[] { 300, 340, 400, 500, 700, 900 })
        {
            Row(
                "corner " + corner.ToString("0", CultureInfo.InvariantCulture)
                + " Hz  (" + Gain(low, HighPass(low, rate, corner)).ToString(
                    "0.0", CultureInfo.InvariantCulture) + " dB)",
                Decode(HighPass(low, rate, corner), rate, 330),
                want);
        }

        Head("fade - the shipped reference fixture, 0.2 Hz, full scale to a floor and back");

        var cycles = reference.Samples.Length / (double)rate * 0.2;

        foreach (var floorDb in new double[] { -20, -30, -40, -50, -60 })
        {
            Row(
                "floor " + floorDb.ToString("0", CultureInfo.InvariantCulture) + " dB",
                Decode(Fade(reference.Samples, rate, 0.2, floorDb), rate, 1000),
                want);
        }

        _output.WriteLine(
            "   the fade runs " + cycles.ToString("0.0", CultureInfo.InvariantCulture)
            + " full cycles over the fixture, so the squelch has that many chances to shut "
            + "and reopen on each row above");

        // **A THIRD THING TO COMPARE THE ROW AGAINST, WHICH NEITHER HYPOTHESIS IS.** The
        // skirt only attenuates - it is 36 dB down at the far end above and still perfect -
        // and on the air attenuation is not felt alone: it arrives with the band's noise
        // behind it. These four fixtures are the shipped ones and are reference-made.
        Head("noise - the shipped signal-to-noise fixtures, for the shape of the garble");

        foreach (var file in new[]
        {
            "psk31-snr+10db-1000hz.wav",
            "psk31-snr+3db-1000hz.wav",
            "psk31-snr-3db-1000hz.wav",
            "psk31-snr-10db-1000hz.wav",
        })
        {
            var audio = WavAudio.Read(Fixture(file));

            Row(
                file.Replace("psk31-", "", StringComparison.Ordinal)
                    .Replace("-1000hz.wav", "", StringComparison.Ordinal),
                Decode(audio.Samples, audio.SampleRate, 1000),
                ManifestText(file));
        }
    }

    /// <summary>How far a filter moved the audio, in decibels of root mean square.</summary>
    /// <param name="before">The audio.</param>
    /// <param name="after">The same audio, filtered.</param>
    private static double Gain(float[] before, float[] after)
    {
        static double Rms(float[] x)
        {
            double sum = 0;

            foreach (var v in x)
            {
                sum += (double)v * v;
            }

            return x.Length == 0 ? 0 : Math.Sqrt(sum / x.Length);
        }

        var a = Rms(before);
        var b = Rms(after);

        return a <= 0 || b <= 0 ? double.NegativeInfinity : 20 * Math.Log10(b / a);
    }

    private void Head(string what)
    {
        _output.WriteLine("");
        _output.WriteLine("== " + what);
        _output.WriteLine(
            "   " + "case".PadRight(32) + "chars".PadLeft(6) + "CER".PadLeft(8)
            + "  first 36 emitted");
    }

    private void Row(string what, string got, string want)
        => _output.WriteLine(
            "   " + what.PadRight(32)
            + got.Length.ToString(CultureInfo.InvariantCulture).PadLeft(6)
            + ErrorRate(got, want).ToString("0.0000", CultureInfo.InvariantCulture).PadLeft(8)
            + "  " + Show(got, 36));

    private static string Decode(float[] samples, int rate, double offsetHz)
        => new Psk31Demodulator(rate, offsetHz).Add(samples);

    /// <summary>A 4th-order Butterworth high-pass, as two biquads in series.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="rate">Its sample rate.</param>
    /// <returns>The filtered audio.</returns>
    /// <remarks>
    /// **FOURTH ORDER IS THE INSTRUCTION'S OWN DEFAULT**, and it is stated rather than
    /// assumed: two biquad sections with the Butterworth Q values for a 4-pole cascade,
    /// 0.5412 and 1.3066, which put the pair of pole angles where a 4th-order Butterworth
    /// has them. That is 24 dB an octave below the corner.
    /// </remarks>
    private static float[] HighPass(float[] samples, int rate)
        => HighPass(samples, rate, CornerHz);

    private static float[] HighPass(float[] samples, int rate, double cornerHz)
        => Biquad(Biquad(samples, rate, 0.54119610, cornerHz), rate, 1.30656296, cornerHz);

    private static float[] Biquad(float[] samples, int rate, double q, double cornerHz)
    {
        var w = 2 * Math.PI * cornerHz / rate;
        var cos = Math.Cos(w);
        var alpha = Math.Sin(w) / (2 * q);

        var b0 = (1 + cos) / 2;
        var b1 = -(1 + cos);
        var b2 = (1 + cos) / 2;
        var a0 = 1 + alpha;
        var a1 = -2 * cos;
        var a2 = 1 - alpha;

        var made = new float[samples.Length];
        double x1 = 0, x2 = 0, y1 = 0, y2 = 0;

        for (var i = 0; i < samples.Length; i++)
        {
            double x = samples[i];
            var y = (b0 / a0 * x) + (b1 / a0 * x1) + (b2 / a0 * x2)
                - (a1 / a0 * y1) - (a2 / a0 * y2);

            x2 = x1;
            x1 = x;
            y2 = y1;
            y1 = y;

            made[i] = (float)y;
        }

        return made;
    }

    /// <summary>A slow sinusoidal fade between full scale and a floor.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="rate">Its sample rate.</param>
    /// <param name="hz">How many full fades a second.</param>
    /// <param name="floorDb">How far down the bottom of the fade is.</param>
    private static float[] Fade(float[] samples, int rate, double hz, double floorDb)
    {
        var made = new float[samples.Length];

        for (var i = 0; i < samples.Length; i++)
        {
            // Nought at the top of the fade and one at the bottom, so the level walks
            // from full scale down to the floor and back.
            var down = (1 - Math.Cos(2 * Math.PI * hz * i / rate)) / 2;

            made[i] = (float)(samples[i] * Math.Pow(10, floorDb * down / 20));
        }

        return made;
    }

    /// <summary>Edit distance over the reference length, idle trimmed.</summary>
    private static double ErrorRate(string got, string want)
    {
        var a = got.Trim();
        var b = want.Trim();

        if (b.Length == 0)
        {
            return a.Length == 0 ? 0 : 1;
        }

        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;

            for (var j = 1; j <= b.Length; j++)
            {
                var swap = previous[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1);

                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), swap);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length] / (double)b.Length;
    }

    /// <summary>The first few characters, with controls made visible.</summary>
    private static string Show(string text, int many)
    {
        var shown = new StringBuilder();

        foreach (var c in text)
        {
            if (shown.Length >= many)
            {
                break;
            }

            shown.Append(char.IsControl(c) ? '.' : c);
        }

        return shown.Length == 0 ? "(nothing)" : shown.ToString();
    }

    private static string ManifestText(string file)
    {
        using var document = JsonDocument.Parse(
            File.ReadAllText(Fixture("manifest.json")));

        foreach (var entry in document.RootElement.EnumerateArray())
        {
            if (entry.GetProperty("file").GetString() == file)
            {
                return entry.GetProperty("text").GetString() ?? "";
            }
        }

        throw new InvalidOperationException(file + " is not in the manifest");
    }

    private static string Fixture(string file)
        => Path.Combine(Root(), "assets", "fixtures", file);

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
