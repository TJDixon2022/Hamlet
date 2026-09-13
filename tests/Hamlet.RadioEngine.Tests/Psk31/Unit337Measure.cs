using System;
using System.Globalization;
using System.IO;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 337 task 2: **what 262 characters and no turnover were made of.**
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION.** The instruction asks which is more
/// likely from the tree - readable text the splitter never cut, or bit-slip garbage - and
/// says measure rather than guess. It runs the demodulator over the clean fixture and the
/// two idle fixtures at the true carrier, at one hertz off, and with a two per cent clock
/// error, and prints what comes out. **It asserts nothing**, so it can never turn a
/// measurement into a wall.</para>
/// <para>**THE CLOCK ERROR IS APPLIED TO THE AUDIO, NOT TO THE DEMODULATOR**, by resampling
/// the samples. Telling the demodulator a wrong sample rate would move its idea of the
/// carrier by the same two per cent and measure two faults at once. Resampling moves the
/// carrier too, so the offset is moved with it: **what is left is the symbol clock alone**,
/// which is the question, because the AFC tracks a carrier and nothing tracks a clock.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio: the fixtures
/// are this project's own modem (FACT-004) on a machine with none (FACT-006).</para>
/// </remarks>
public sealed class Unit337Measure
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the measurement.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit337Measure(ITestOutputHelper output) => _output = output;

    /// <summary>Print what the demodulator emits under a small carrier and clock error.</summary>
    [Fact]
    public void WhatASmallErrorDoesToWhatIsEmitted()
    {
        Head("psk31-clean-1000hz.wav, 1000 Hz carrier, 255 reference characters");

        Measure("psk31-clean-1000hz.wav", 1000.0, clock: 1.0, "true carrier, true clock");
        Measure("psk31-clean-1000hz.wav", 1001.0, clock: 1.0, "carrier +1 Hz, true clock");
        Measure("psk31-clean-1000hz.wav", 1000.0, clock: 1.02, "true carrier, clock +2%");
        Measure("psk31-clean-1000hz.wav", 1001.0, clock: 1.02, "carrier +1 Hz, clock +2%");

        Head("psk31-idle-8s-1000hz.wav, eight seconds of unmodulated idle");

        Measure("psk31-idle-8s-1000hz.wav", 1000.0, clock: 1.0, "true carrier, true clock");
        Measure("psk31-idle-8s-1000hz.wav", 1001.0, clock: 1.0, "carrier +1 Hz, true clock");
        Measure("psk31-idle-8s-1000hz.wav", 1000.0, clock: 1.02, "true carrier, clock +2%");

        Head("psk31-idle-gap-1000hz.wav, text with an idle gap in it");

        Measure("psk31-idle-gap-1000hz.wav", 1000.0, clock: 1.0, "true carrier, true clock");
        Measure("psk31-idle-gap-1000hz.wav", 1001.0, clock: 1.0, "carrier +1 Hz, true clock");
        Measure("psk31-idle-gap-1000hz.wav", 1000.0, clock: 1.02, "true carrier, clock +2%");
    }

    private void Head(string what)
    {
        _output.WriteLine("");
        _output.WriteLine("== " + what);
        _output.WriteLine(
            "   "
            + "case".PadRight(28)
            + "chars".PadLeft(6)
            + "print%".PadLeft(8)
            + "letter%".PadLeft(9)
            + "turnovers".PadLeft(11)
            + "  first 48 emitted");
    }

    private void Measure(string file, double offset, double clock, string what)
    {
        var audio = WavAudio.Read(Fixture(file));

        // **THE CARRIER MOVES WITH THE CLOCK**, because resampling moves everything in the
        // file. Correcting the offset by the same factor leaves the symbol clock as the
        // only thing wrong, which is what this is measuring.
        var samples = clock == 1.0 ? audio.Samples : Resample(audio.Samples, clock);
        var carrier = offset * clock;

        var demodulator = new Psk31Demodulator(audio.SampleRate, carrier);
        var text = demodulator.Add(samples);

        var printable = 0;
        var letters = 0;

        foreach (var c in text)
        {
            if (!char.IsControl(c))
            {
                printable++;
            }

            if (char.IsLetterOrDigit(c) || c == ' ')
            {
                letters++;
            }
        }

        // **THE TURNOVER IS WHAT THE SPLITTER CUTS ON** (unit 316), so counting them says
        // directly whether a splitter fed this text would ever have produced a line.
        var turnovers = Turnovers(text);

        _output.WriteLine(
            "   "
            + what.PadRight(28)
            + text.Length.ToString(CultureInfo.InvariantCulture).PadLeft(6)
            + Share(printable, text.Length).PadLeft(8)
            + Share(letters, text.Length).PadLeft(9)
            + turnovers.ToString(CultureInfo.InvariantCulture).PadLeft(11)
            + "  " + Show(text, 48));
    }

    private static string Share(int part, int whole)
        => whole == 0
            ? "-"
            : (100.0 * part / whole).ToString("0", CultureInfo.InvariantCulture);

    /// <summary>Count the turnover words a splitter would cut on.</summary>
    private static int Turnovers(string text)
    {
        var count = 0;

        foreach (var word in new[] { " K ", " KN ", " SK ", " BTU ", " DE " })
        {
            var at = 0;

            while (true)
            {
                var found = (" " + text.ToUpperInvariant() + " ").IndexOf(
                    word, at, StringComparison.Ordinal);

                if (found < 0)
                {
                    break;
                }

                count++;
                at = found + 1;
            }
        }

        return count;
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

    /// <summary>Linear resample, speeding the audio up by <paramref name="by"/>.</summary>
    private static float[] Resample(float[] samples, double by)
    {
        var wanted = (int)(samples.Length / by);
        var made = new float[wanted];

        for (var i = 0; i < wanted; i++)
        {
            var at = i * by;
            var low = (int)at;
            var part = at - low;

            made[i] = low + 1 < samples.Length
                ? (float)(samples[low] * (1 - part) + samples[low + 1] * part)
                : samples[Math.Min(low, samples.Length - 1)];
        }

        return made;
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
