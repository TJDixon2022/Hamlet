using System.Globalization;
using System.Numerics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The control for the second decoder's synthetic case (V-04; work instruction
/// 457, task 3): on every keyed recording, what the audio holds in the 5 s
/// before the first keyed element of its key. For HM-REQ-122's case; proves no
/// requirement.
/// </summary>
/// <remarks>
/// <para>**A PRINTER THAT ASSERTS NOTHING.** Task 2 found fldigi loses a first
/// element that follows seconds of noise alone, and reads one that follows
/// keying at the same level. V-04 makes the real recording the generator's
/// control, so this measures which of those the air gives.</para>
/// <para>**WHERE THE KEY STARTS.** Our decoder's reading locates the key: the
/// first `CQ` on 17:37 (its scorer's rule), an adjudicated reading's anchor, a
/// benchmark stretch's key. The character there gives an arrival time and a
/// measured pattern and speed; the first element is the mark, at the pitch
/// instrument's pitch, whose onset lies nearest the arrival less the pattern's
/// length. An anchor that starts inside its adjudicated text is flagged.</para>
/// <para>**THE ENVELOPE** is this file's own: 10 ms Hann frames 5 ms apart,
/// each a single-bin Fourier sum at the pitch. A frame is keyed above the
/// midpoint, in decibels, of the recording's 20th and 97th percentile frames;
/// a mark is three or more keyed frames, a one-frame drop bridged.</para>
/// </remarks>
public sealed class WhatPrecedesAKeysFirstElementOnTheAirTests
{
    private const double FrameSeconds = 0.010;
    private const double HopSeconds = 0.005;
    private const double BeforeSeconds = 5.0;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the printer.</summary>
    /// <param name="output">Where each recording's line is printed.</param>
    public WhatPrecedesAKeysFirstElementOnTheAirTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One line per keyed recording, then one per mark in the 5 s before its key.</summary>
    [Fact]
    public void OnEveryKeyedRecording()
    {
        _output.WriteLine("control | recording | set | pitch Hz | key from | anchor offset in key | key char at s | "
            + "first element s | element dB over floor | runs in 5 s before | keyed s in 5 s | last run ends s before | "
            + "run level vs element dB (median) | floor vs element dB | other pitches in 5 s | longest run s | holds");

        foreach (var keyed in WhatTheStrayLettersRestOnTests.KeyedRecordings)
        {
            Line(keyed.Name, keyed.Set);
        }
    }

    private void Line(string name, string set)
    {
        var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
        var windows = CwPitchInstrument.Measure(audio.Samples, audio.SampleRate);

        if (windows.Count == 0)
        {
            _output.WriteLine($"control | {name} | {set} | no keyed window found by the pitch instrument");
            return;
        }

        var pitch = windows.Select(w => w.Hz).OrderBy(h => h).ElementAt(windows.Count / 2);
        var settled = TheSeventeenThirtySevenCaptureTests.Settle(name);
        var (from, anchorOffset, keyText) = Anchor(name, set, CwReading.Of(settled).Text);

        if (from < 0)
        {
            _output.WriteLine($"control | {name} | {set} | {pitch:0.0} | key not found in our reading `{keyText}`");
            return;
        }

        var character = CharacterAt(settled, from);
        var units = character.Pattern.Sum(e => e == '-' ? 3 : 1) + Math.Max(0, character.Pattern.Length - 1);
        var arrival = character.At.TotalSeconds;
        var estimate = arrival - (units * 1.2 / Math.Max(1, character.WordsPerMinute));

        var (levels, hop) = Envelope(audio, pitch);
        var sorted = levels.OrderBy(v => v).ToArray();
        var floor = sorted[(int)(0.20 * (sorted.Length - 1))];
        var mark = sorted[(int)(0.97 * (sorted.Length - 1))];
        var threshold = (floor + mark) / 2;
        var runs = Marks(levels, threshold);

        var candidates = runs.Where(r => r.Start * hop >= estimate - 0.5 && r.Start * hop <= arrival).ToList();

        if (candidates.Count == 0)
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"control | {name} | {set} | {pitch:0.0} | `{keyText}` | {anchorOffset} | {arrival:0.000} | no mark between {estimate - 0.5:0.000} and {arrival:0.000}"));
            return;
        }

        var first = candidates.OrderBy(r => Math.Abs((r.Start * hop) - estimate)).First();
        var onset = first.Start * hop;
        var elementLevel = Enumerable.Range(first.Start, first.End - first.Start).Max(i => levels[i]);
        var before = runs.Where(r => r.End * hop <= onset && r.End * hop > onset - BeforeSeconds).ToList();
        var inWindow = Enumerable.Range(0, levels.Length).Where(i => i * hop >= onset - BeforeSeconds && i * hop < onset).Select(i => levels[i]).OrderBy(v => v).ToArray();
        var windowFloor = inWindow.Length > 0 ? inWindow[(int)(0.20 * (inWindow.Length - 1))] : double.NaN;
        var others = windows
            .Where(w => w.EndSeconds > onset - BeforeSeconds && w.StartSeconds < onset && Math.Abs(w.Hz - pitch) > 20)
            .Select(w => w.Hz.ToString("0", CultureInfo.InvariantCulture))
            .Distinct()
            .ToList();
        var keyedSeconds = before.Sum(r => (r.End - r.Start) * hop);
        var median = before.Count == 0
            ? double.NaN
            : before.Select(r => Enumerable.Range(r.Start, r.End - r.Start).Max(i => levels[i]) - elementLevel).OrderBy(v => v).ElementAt(before.Count / 2);
        var longest = before.Count == 0 ? 0 : before.Max(r => (r.End - r.Start) * hop);
        var lastEnd = before.Count == 0 ? double.NaN : onset - (before.Max(r => r.End) * hop);

        var holds = others.Count > 0 && before.Count == 0 ? "another station"
            : longest > 1.0 ? "a carrier or a long key-down"
            : before.Count >= 3 && Math.Abs(median) <= 6 ? "the same operator's earlier keying"
            : before.Count > 0 ? "marks at another level"
            : others.Count > 0 ? "another station"
            : "noise only";

        _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"control | {name} | {set} | {pitch:0.0} | `{keyText}` | {anchorOffset} | {arrival:0.000} | {onset:0.000} | "
            + $"{elementLevel - floor:0.0} | {before.Count} | {keyedSeconds:0.00} | {lastEnd:0.000} | {median:0.0} | "
            + $"{windowFloor - elementLevel:0.0} | {(others.Count == 0 ? "none" : string.Join(" ", others))} | {longest:0.00} | {holds}"));

        foreach (var r in before)
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"control run | {name} | {r.Start * hop - onset:0.000} s | {(r.End - r.Start) * hop * 1000:0} ms | {Enumerable.Range(r.Start, r.End - r.Start).Max(i => levels[i]) - elementLevel:0.0} dB vs the element"));
        }
    }

    private static (int From, int Offset, string Key) Anchor(string name, string set, string text)
    {
        if (set == "17:37")
        {
            return (text.IndexOf("CQ", StringComparison.Ordinal), 0, TheSeventeenThirtySevenCaptureTests.InferredKey);
        }

        if (set == "the ten")
        {
            var stretch = TheBenchmarkIsKeyedTests.Stretches(name)[0];
            var at = text.IndexOf(stretch.Key, StringComparison.Ordinal);
            if (at >= 0) return (at, 0, stretch.Key);
            var region = stretch.Region.Trim();
            var inRegion = text.IndexOf(region, StringComparison.Ordinal);
            return (inRegion, 0, stretch.Key + " (located by its region)");
        }

        var reading = TheAdjudicatedReadingsKeepReadingTests.All.Single(r => r.Name == name);
        var anchor = reading.Anchor.Trim();
        return (text.IndexOf(anchor, StringComparison.Ordinal), Math.Max(0, reading.Adjudicated.IndexOf(anchor, StringComparison.Ordinal)), reading.Adjudicated);
    }

    private static CwCharacter CharacterAt(IReadOnlyList<CwCharacter> settled, int index)
    {
        var at = 0;

        foreach (var c in settled)
        {
            if (index < at + c.Text.Length) return c;
            at += c.Text.Length;
        }

        return settled[^1];
    }

    private static (double[] Levels, double Hop) Envelope(MonoAudio audio, double pitch)
    {
        var frame = (int)Math.Round(FrameSeconds * audio.SampleRate);
        var hop = (int)Math.Round(HopSeconds * audio.SampleRate);
        var count = Math.Max(0, ((audio.Samples.Length - frame) / hop) + 1);
        var levels = new double[count];
        var window = new double[frame];
        var step = 2 * Math.PI * pitch / audio.SampleRate;

        for (var i = 0; i < frame; i++) window[i] = 0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (frame - 1)));

        for (var f = 0; f < count; f++)
        {
            var sum = Complex.Zero;
            var start = f * hop;

            for (var i = 0; i < frame; i++)
            {
                var n = start + i;
                sum += audio.Samples[n] * window[i] * new Complex(Math.Cos(step * n), -Math.Sin(step * n));
            }

            levels[f] = 20 * Math.Log10(Complex.Abs(sum) + 1e-12);
        }

        return (levels, HopSeconds);
    }

    private static List<(int Start, int End)> Marks(double[] levels, double threshold)
    {
        var runs = new List<(int Start, int End)>();
        var start = -1;
        var below = 0;

        for (var i = 0; i < levels.Length; i++)
        {
            if (levels[i] > threshold)
            {
                if (start < 0) start = i;
                below = 0;
                continue;
            }

            if (start < 0) continue;

            below++;

            if (below > 1)
            {
                var end = i - below + 1;
                if (end - start >= 3) runs.Add((start, end));
                start = -1;
                below = 0;
            }
        }

        if (start >= 0 && levels.Length - start >= 3) runs.Add((start, levels.Length));

        return runs;
    }
}
