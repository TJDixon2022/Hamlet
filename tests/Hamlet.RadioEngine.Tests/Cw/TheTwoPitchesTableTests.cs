using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The pitch the decoder mixes at beside the pitch the independent sweep hears
/// the keying at, one row per recording (work instruction 409, 3.8).
/// </summary>
/// <remarks>
/// <para>A printer. It asserts nothing and is on neither carry-forward line.</para>
/// <para>**THE DECODER'S PITCH IS WHAT IT MIXES AT**, `CwDecoder.Stream.ToneHz`,
/// read after every hop exactly as the floors feed a capture. It is taken as the
/// most-held value, hop-weighted, from the hop the first named character settled
/// to the hop the last one did, or over the whole recording where none settled.
/// **THE SWEEP IS <see cref="KeyingEnvelope"/>**, which shares nothing with the
/// decoder, judged over the tree's own range, 300 to 900 Hz; the 400 to 1200 Hz
/// column is printed beside it and judges nothing.</para>
/// <para>**A PITCH AGREEING IS NOT A READING BEING RIGHT** (CLAUDE.md 0.0).</para>
/// </remarks>
public sealed class TheTwoPitchesTableTests
{
    /// <summary>One bin of the sweep; agree is this or less.</summary>
    public const double AgreeWithinHz = KeyingEnvelope.ToneStepHz;

    /// <summary>Single-sender needs the runner-up under this share of the winner.</summary>
    public const double SecondBestBelow = 0.5;

    /// <summary>The three adjudicated anchors' recordings.</summary>
    public static readonly string[] AnchorRecordings =
    {
        "cw-2026-08-17-013347",
        "cw-2026-08-17-134712",
        "unadjudicated/cw-2026-08-18-003758",
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public TheTwoPitchesTableTests(ITestOutputHelper output) => _output = output;

    /// <summary>What one recording measured.</summary>
    public sealed record Row(
        string Name,
        bool Anchor,
        int Named,
        double DecoderHz,
        double DecoderLowHz,
        double DecoderHighHz,
        double ShareApart,
        double Sweep900Hz,
        double Sweep1200Hz,
        double SecondBest,
        bool Keying,
        bool SingleSender,
        string Segments,
        int SpanHops,
        int FirstApartHop,
        KeyingProfile SweepProfile)
    {
        /// <summary>The difference the judgment is on, in hertz.</summary>
        public double DifferenceHz => Math.Abs(DecoderHz - Sweep900Hz);

        /// <summary>Within one bin.</summary>
        public bool Agree => DifferenceHz <= AgreeWithinHz;
    }

    /// <summary>Every case the table reads: the 37 captures and 17:37.</summary>
    public static IReadOnlyList<string> Cases()
        => TheCapturesThatDecodeKeepDecodingTests.Floors
            .Select(row => (string)row[0])
            .Append(TheSeventeenThirtySevenCaptureTests.Name)
            .ToList();

    /// <summary>Read one recording and measure both pitches.</summary>
    /// <param name="name">The recording, under the captures folder.</param>
    /// <returns>Its row.</returns>
    public static Row Measure(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        // **THE SWEEP FIRST, AND FROM THE AUDIO ALONE.** Nothing the decoder does
        // below can reach these numbers.
        var candidates = new List<(double Hz, KeyingProfile Profile)>();

        for (var tone = KeyingEnvelope.LowestToneHz; tone <= KeyingEnvelope.HighestToneHz; tone += KeyingEnvelope.ToneStepHz)
        {
            candidates.Add((tone, KeyingEnvelope.Measure(audio, tone)));
        }

        var best = KeyingEnvelope.Best(audio)!.Value;
        var winner = best.Profile.Score;
        var runnerUp = candidates
            .Where(c => Math.Abs(c.Hz - best.ToneHz) > 2 * KeyingEnvelope.ToneStepHz)
            .Select(c => c.Profile.Score)
            .DefaultIfEmpty(0)
            .Max();
        var second = winner > 0 ? runnerUp / winner : double.NaN;

        // The same selection rule, over 400 to 1200.
        double wide = double.NaN;
        var wideScore = double.NegativeInfinity;

        for (var tone = 400.0; tone <= 1200; tone += KeyingEnvelope.ToneStepHz)
        {
            var score = KeyingEnvelope.Measure(audio, tone).Score;

            if (double.IsNaN(wide) || score > wideScore)
            {
                wide = tone;
                wideScore = score;
            }
        }

        // CwKeyingMeter's own verdict, asked of the whole recording.
        var keying = best.Profile.Score >= CwKeyingThresholds.KeyingScore
                     && best.Profile.ElementMedianMs >= CwKeyingThresholds.SlowestChatterMs
                     && best.Profile.ElementMedianMs <= CwKeyingThresholds.LongestElementMs
                     && best.Profile.SwingDb >= CwKeyingThresholds.ConfidentSwingDb;

        // **THEN THE DECODER, FED A HOP AT A TIME AS THE FLOORS FEED IT.**
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var hop = decoder.Tracker.HopSamples;
        var mix = new List<double>();
        var namedHops = new List<int>();

        decoder.CharacterSettled += c =>
        {
            if (!c.IsWordGap && !c.IsUnreadable)
            {
                namedHops.Add(mix.Count);
            }
        };

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            mix.Add(decoder.Stream.ToneHz);
        }

        decoder.Flush();

        var from = namedHops.Count == 0 ? 0 : Math.Min(namedHops.Min(), mix.Count - 1);
        var to = namedHops.Count == 0 ? mix.Count - 1 : Math.Min(namedHops.Max(), mix.Count - 1);
        var span = mix.Skip(from).Take(to - from + 1).ToList();

        var held = span
            .GroupBy(Math.Round)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .First()
            .Key;

        var apart = span.Count(v => Math.Abs(v - best.ToneHz) > AgreeWithinHz);
        var firstApart = span.FindIndex(v => Math.Abs(v - best.ToneHz) > AgreeWithinHz);

        return new Row(
            name,
            AnchorRecordings.Contains(name),
            namedHops.Count,
            held,
            span.Min(),
            span.Max(),
            span.Count == 0 ? 0 : (double)apart / span.Count,
            best.ToneHz,
            wide,
            second,
            keying,
            SingleSender: keying && second < SecondBestBelow,
            Segments(mix, from, to),
            span.Count,
            firstApart < 0 ? -1 : from + firstApart,
            best.Profile);
    }

    /// <summary>Where the mix sat, as value and hop range, rounded to a hertz.</summary>
    private static string Segments(IReadOnlyList<double> mix, int from, int to)
    {
        var parts = new List<string>();
        var start = 0;

        for (var i = 1; i <= mix.Count; i++)
        {
            if (i < mix.Count && Math.Round(mix[i]) == Math.Round(mix[start]))
            {
                continue;
            }

            parts.Add($"{Math.Round(mix[start]):0}@{start}-{i - 1}");
            start = i;
        }

        return $"span {from}-{to}: " + string.Join(" ", parts);
    }

    /// <remarks>Prints the table; asserts nothing.</remarks>
    [Fact]
    public void EveryCaseIsTabled()
    {
        var rows = Cases().Select(Measure).ToList();

        _output.WriteLine(
            "row | case | anchor | named | decoder Hz | low | high | share apart | sweep 300-900 | sweep 400-1200 "
            + "| second-best | keying | single-sender | difference Hz | agree");

        foreach (var r in rows)
        {
            _output.WriteLine(
                $"row | {r.Name} | {(r.Anchor ? "anchor" : "")} | {r.Named} | {r.DecoderHz:0} | {r.DecoderLowHz:0.0} "
                + $"| {r.DecoderHighHz:0.0} | {r.ShareApart:0.00} | {r.Sweep900Hz:0} | {r.Sweep1200Hz:0} "
                + $"| {r.SecondBest:0.00} | {(r.Keying ? "keying" : "no")} | {(r.SingleSender ? "yes" : "no")} "
                + $"| {r.DifferenceHz:0} | {(r.Agree ? "agree" : "apart")}");
        }

        foreach (var r in rows)
        {
            _output.WriteLine($"seg | {r.Name} | first apart hop {r.FirstApartHop} | {r.Segments}");
        }

        // Why the verdict is what it is: the four figures CwKeyingMeter asks of.
        foreach (var r in rows)
        {
            _output.WriteLine(
                $"keying-why | {r.Name} | score {r.SweepProfile.Score:0.000} against {CwKeyingThresholds.KeyingScore} "
                + $"| element median {r.SweepProfile.ElementMedianMs:0} ms against {CwKeyingThresholds.SlowestChatterMs} to "
                + $"{CwKeyingThresholds.LongestElementMs} | swing {r.SweepProfile.SwingDb:0.0} dB against {CwKeyingThresholds.ConfidentSwingDb}");
        }

        foreach (var r in rows.Where(r => r.Sweep900Hz != r.Sweep1200Hz))
        {
            var wideDifference = Math.Abs(r.DecoderHz - r.Sweep1200Hz);

            _output.WriteLine(
                $"sweeps-differ | {r.Name} | 300-900 {r.Sweep900Hz:0} | 400-1200 {r.Sweep1200Hz:0} "
                + $"| decoder {r.DecoderHz:0} | on 400-1200 it would be {wideDifference:0} Hz, "
                + $"{(wideDifference <= AgreeWithinHz ? "agree" : "apart")}");
        }

        var single = rows.Where(r => r.SingleSender).ToList();

        _output.WriteLine(
            $"count | single-sender {single.Count} of {rows.Count} | apart {single.Count(r => !r.Agree)} "
            + $"| apart names {string.Join(", ", single.Where(r => !r.Agree).Select(r => r.Name))}");
    }
}
