using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// MET-PITCH-ERR on every capture in the tree: the tracker's pitch against the
/// note <see cref="CwPitchInstrument"/> measured, wherever the instrument found
/// keying (work instruction 447, task 2; PHASE_PLAN.md 4.2; HM-REQ-092).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.** HM-REQ-092's N is TBD, so nothing here
/// is judged; the flag `>25` marks where the decoder sat more than 25 Hz from the
/// sender, the tracker's own coarse spacing, as 4.2 asks.</para>
/// <para>**EVERY WAV UNDER `tests/fixtures/cw/captured`**, which holds the 51 rows
/// of <see cref="TheCapturesThatDecodeKeepDecodingTests"/>, the keyed recordings
/// and the whole 7.052 session. `cw-2026-09-24-135641` and `-152135` are not in
/// the tree and are not measured.</para>
/// <para>**THE DECODER IS DRIVEN AS THE CAPTURES TYPE DRIVES IT**: cold, from the
/// operator's 600 Hz, a hop at a time. The tracker's pitch is
/// <see cref="CwToneTracker.ToneHz"/> on every hop, and a window's figure is its
/// median over the hops inside that instrument window. MET-PITCH-ERR for a window
/// is that median less the instrument's pitch (CW_SPEC.md 11: estimated less
/// true); a recording's figure is the median over its keyed windows, and the
/// windows more than 25 Hz off are counted beside it. The instrument is never
/// handed to the decoder.</para>
/// </remarks>
public sealed class WhatPitchTheDecoderIsOnTests
{
    /// <summary>The distance the flag marks, in hertz.</summary>
    internal const double FlagHz = 25;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public WhatPitchTheDecoderIsOnTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The decoder's pitches on every hop of a run.</summary>
    /// <param name="HopSamples">Samples per hop.</param>
    /// <param name="Tracker">The tracker's pitch after each hop.</param>
    /// <param name="Mix">The pitch the stream mixed at after each hop.</param>
    /// <param name="Measured">Whether the tracker had measured a pitch by then.</param>
    internal sealed record DecoderPitch(int HopSamples, double[] Tracker, double[] Mix, bool[] Measured);

    /// <summary>One instrument window beside the decoder.</summary>
    /// <param name="Window">What the instrument found.</param>
    /// <param name="TrackerHz">The tracker's median pitch over the window.</param>
    /// <param name="MixHz">The stream's median mix pitch over the window.</param>
    /// <param name="MeasuredShare">The share of the window's hops on which the tracker had measured a pitch.</param>
    internal sealed record Beside(PitchWindow Window, double TrackerHz, double MixHz, double MeasuredShare)
    {
        /// <summary>MET-PITCH-ERR for this window, tracker less instrument.</summary>
        public double ErrorHz => TrackerHz - Window.Hz;
    }

    /// <summary>Every capture in the tree, relative to the captured folder, in order.</summary>
    internal static IReadOnlyList<string> Captures()
        => Directory.GetFiles(CapturedSignalTests.Folder, "*.wav", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(CapturedSignalTests.Folder, p).Replace('\\', '/'))
            .Select(p => p[..^4])
            .OrderBy(p => Path.GetFileName(p), StringComparer.Ordinal)
            .ToList();

    /// <summary>Drive the decoder over audio, cold from 600 Hz, and keep its pitches.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>The pitches, hop by hop.</returns>
    internal static DecoderPitch Drive(float[] samples, int sampleRate)
    {
        var decoder = new CwDecoder(sampleRate, 600);
        var hop = decoder.Tracker.HopSamples;
        var tracker = new List<double>();
        var mix = new List<double>();
        var measured = new List<bool>();

        for (var at = 0L; at + hop <= samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, sampleRate, samples.AsSpan((int)at, hop)));
            tracker.Add(decoder.Tracker.ToneHz);
            mix.Add(decoder.Stream.ToneHz);
            measured.Add(decoder.Tracker.HasMeasuredPitch);
        }

        decoder.Flush();

        return new DecoderPitch(hop, tracker.ToArray(), mix.ToArray(), measured.ToArray());
    }

    /// <summary>Put every instrument window beside the decoder's pitch over it.</summary>
    /// <param name="windows">What the instrument found.</param>
    /// <param name="pitch">What the decoder was on.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>One row per window.</returns>
    internal static IReadOnlyList<Beside> Compare(IReadOnlyList<PitchWindow> windows, DecoderPitch pitch, int sampleRate)
    {
        var rows = new List<Beside>();

        foreach (var w in windows)
        {
            var from = (int)Math.Floor(w.StartSeconds * sampleRate / pitch.HopSamples);
            var to = Math.Min(pitch.Tracker.Length, (int)Math.Ceiling(w.EndSeconds * sampleRate / pitch.HopSamples));

            if (to <= from)
            {
                continue;
            }

            var hops = Enumerable.Range(from, to - from).ToList();

            rows.Add(new Beside(
                w,
                Median(hops.Select(i => pitch.Tracker[i])),
                Median(hops.Select(i => pitch.Mix[i])),
                hops.Count(i => pitch.Measured[i]) / (double)hops.Count));
        }

        return rows;
    }

    /// <summary>
    /// 4.2: every capture in the tree, the decoder's pitch beside the instrument's,
    /// the `>25` cases first.
    /// </summary>
    [Fact]
    public void OnEveryCaptureInTheTree()
    {
        var rows = new List<(string Name, IReadOnlyList<Beside> Windows, double Median, int Over, string Line)>();

        foreach (var name in Captures())
        {
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
            var windows = CwPitchInstrument.Measure(audio.Samples, audio.SampleRate);
            var pitch = Drive(audio.Samples, audio.SampleRate);
            var beside = Compare(windows, pitch, audio.SampleRate);

            foreach (var b in beside)
            {
                _output.WriteLine(string.Create(Invariant,
                    $"window | {name} | {b.Window.StartSeconds:0} to {b.Window.EndSeconds:0} s | instrument {b.Window.Hz:0.0} | tracker {b.TrackerHz:0.0} | mix {b.MixHz:0.0} | measured {b.MeasuredShare:0.00} | err {b.ErrorHz:+0.0;-0.0;0.0} | swing {b.Window.ContrastDb:0.0} dB | keyed {b.Window.KeyedSeconds:0.00} s{(Math.Abs(b.ErrorHz) > FlagHz ? " | >25" : "")}"));
            }

            if (beside.Count == 0)
            {
                rows.Add((name, beside, double.NaN, 0, string.Create(Invariant,
                    $"capture | {name} | no keying found by the instrument | tracker {Median(pitch.Tracker):0.0} over the file | - | - | 0 of 0 | -")));
                continue;
            }

            var median = Median(beside.Select(b => b.ErrorHz));
            var over = beside.Count(b => Math.Abs(b.ErrorHz) > FlagHz);
            var flag = Math.Abs(median) > FlagHz ? ">25" : over > 0 ? "windows >25" : "-";

            rows.Add((name, beside, median, over, string.Create(Invariant,
                $"capture | {name} | tracker {Median(beside.Select(b => b.TrackerHz)):0.0} | instrument {Median(beside.Select(b => b.Window.Hz)):0.0} | MET-PITCH-ERR {median:+0.0;-0.0;0.0} | worst {beside.Max(b => Math.Abs(b.ErrorHz)):0.0} | {over} of {beside.Count} windows >25 | {flag}")));
        }

        _output.WriteLine("capture | file | tracker Hz (median over keyed windows) | instrument Hz (median) | MET-PITCH-ERR Hz (median of tracker less instrument) | worst window Hz | windows more than 25 Hz off | flag");

        foreach (var row in rows
            .OrderByDescending(r => !double.IsNaN(r.Median) && Math.Abs(r.Median) > FlagHz)
            .ThenByDescending(r => r.Over > 0)
            .ThenBy(r => Path.GetFileName(r.Name), StringComparer.Ordinal))
        {
            _output.WriteLine(row.Line);
        }

        var measured = rows.Count(r => r.Windows.Count > 0);

        _output.WriteLine(string.Create(Invariant,
            $"total | {rows.Count} captures in the tree | {measured} with keying found | {rows.Count - measured} with none | {rows.Count(r => !double.IsNaN(r.Median) && Math.Abs(r.Median) > FlagHz)} with MET-PITCH-ERR more than 25 Hz | {rows.Count(r => r.Over > 0)} with any window more than 25 Hz | {rows.Sum(r => r.Over)} of {rows.Sum(r => r.Windows.Count)} windows more than 25 Hz"));
        _output.WriteLine("absent | cw-2026-09-24-135641 | not in the tree, unmeasured");
        _output.WriteLine("absent | cw-2026-09-24-152135 | not in the tree, unmeasured");
    }

    /// <summary>
    /// The 7.052 opening at HEAD, spliced as <see cref="WhatTheOpeningHeardTests"/>
    /// splices it: its text, the pitch the decoder was on beside the one the
    /// instrument found, and when the first sure character came (HM-REQ-102, 103).
    /// </summary>
    [Fact]
    public void OnTheOpening()
    {
        var opening = WhatTheOpeningHeardTests.Session.Take(7).ToList();
        var (samples, rate, pieces) = WhatTheOpeningHeardTests.Splice(opening);

        foreach (var piece in pieces)
        {
            var from = piece.StreamStart / (double)rate;
            var to = (piece.StreamStart + piece.Kept) / (double)rate;
            var holds = 30.54 >= from && 30.54 < to ? " | holds 30.54 s" : "";

            _output.WriteLine(string.Create(Invariant,
                $"join | {piece.Name} | stream {from:0.00} to {to:0.00} s | skip {piece.Skip / (double)rate:0.000} s{holds} | {piece.Note}"));
        }

        var run = WhatTheOpeningHeardTests.Trace(samples, rate);
        var windows = CwPitchInstrument.Measure(samples, rate);
        var pitch = Drive(samples, rate);
        var beside = Compare(windows, pitch, rate);

        foreach (var b in beside)
        {
            _output.WriteLine(string.Create(Invariant,
                $"opening window | {b.Window.StartSeconds:0} to {b.Window.EndSeconds:0} s | instrument {b.Window.Hz:0.0} | tracker {b.TrackerHz:0.0} | mix {b.MixHz:0.0} | measured {b.MeasuredShare:0.00} | err {b.ErrorHz:+0.0;-0.0;0.0} | swing {b.Window.ContrastDb:0.0} dB{(Math.Abs(b.ErrorHz) > FlagHz ? " | >25" : "")}"));
        }

        var firstSure = run.Settled.FirstOrDefault(h => CwSymbol.Of(h.Character).Class == CwSymbolClass.Sure);
        var at3054 = beside.Where(b => b.Window.StartSeconds <= 30.54 && b.Window.EndSeconds > 30.54).ToList();

        _output.WriteLine(string.Create(Invariant,
            $"opening | text 0 to 46.2 s | {run.Text(0, 46.2)}"));
        _output.WriteLine(string.Create(Invariant,
            $"opening | text 30 to 46.2 s | {run.Text(30, 46.2)}"));
        _output.WriteLine(string.Create(Invariant,
            $"opening | text, whole stream | {run.Text()}"));
        var sure = firstSure is null
            ? "none"
            : string.Create(Invariant, $"{firstSure.Character.Text} at {firstSure.Character.At.TotalSeconds:0.00} s, tracker {firstSure.TrackerHz:0.0}, mix {firstSure.MixHz:0.0}");

        _output.WriteLine($"opening | first sure character | {sure}");
        var characters = string.Join(" ", run.Settled
            .Where(h => !h.Character.IsWordGap && h.Character.At.TotalSeconds < 46.2)
            .Select(h => string.Create(Invariant, $"{h.Character.Text}@{h.Character.At.TotalSeconds:0.0}/{h.TrackerHz:0}/{CwSymbol.Of(h.Character).Class}")));

        _output.WriteLine($"opening | characters 0 to 46.2 s, text@seconds/tracker Hz/class | {characters}");

        foreach (var b in at3054)
        {
            _output.WriteLine(string.Create(Invariant,
                $"opening | at 30.54 s | window {b.Window.StartSeconds:0} to {b.Window.EndSeconds:0} s | instrument {b.Window.Hz:0.0} | tracker {b.TrackerHz:0.0} | mix {b.MixHz:0.0} | err {b.ErrorHz:+0.0;-0.0;0.0}"));
        }

        _output.WriteLine(string.Create(Invariant,
            $"opening | total | {beside.Count} windows keyed | MET-PITCH-ERR median {(beside.Count == 0 ? double.NaN : Median(beside.Select(b => b.ErrorHz))):+0.0;-0.0;0.0} | {beside.Count(b => Math.Abs(b.ErrorHz) > FlagHz)} windows more than 25 Hz"));
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToList();

        return sorted.Count == 0
            ? double.NaN
            : sorted.Count % 2 == 1
                ? sorted[sorted.Count / 2]
                : (sorted[(sorted.Count / 2) - 1] + sorted[sorted.Count / 2]) / 2.0;
    }
}
