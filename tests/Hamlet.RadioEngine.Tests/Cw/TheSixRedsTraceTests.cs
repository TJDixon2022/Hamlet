using System.Reflection;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A printer, not a test: the six reds still open under 3.6, traced past where
/// unit 402's printer stopped (work instruction 405, task 1).
/// </summary>
/// <remarks>
/// <para>**IT ASSERTS NOTHING** and is on no carry-forward line. Group B is the
/// easy tier behind #43 to #45, character by character with the speed and the
/// unit in force when each settled; group D is the speed fit behind #15 and the
/// first character behind #6; group C is every span the transmit guard marks on
/// the fixture behind #42 and on the two real captures a skip cost. The stream's
/// window, its held gaps, the decoder's reading callback and two of the unit
/// estimator's private steps are read by reflection here, in the printer
/// only.</para>
/// </remarks>
public sealed class TheSixRedsTraceTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

    private static readonly FieldInfo EnvelopeField =
        typeof(CwProbabilisticStream).GetField("_envelope", Private)!;

    private static readonly FieldInfo HopsSeenField =
        typeof(CwProbabilisticStream).GetField("_hopsSeen", Private)!;

    private static readonly FieldInfo StructureHeldField =
        typeof(CwProbabilisticStream).GetField("_structureHeld", Private)!;

    private static readonly FieldInfo HeldGapsField =
        typeof(CwProbabilisticStream).GetField("_heldGaps", Private)!;

    private static readonly FieldInfo OnReadingField =
        typeof(CwDecoder).GetField("_onReading", Private)!;

    private static readonly MethodInfo TwoMeansMethod =
        typeof(CwUnitEstimator).GetMethod("TwoMeansOnLogs", PrivateStatic)!;

    private static readonly MethodInfo OtsuMethod =
        typeof(CwUnitEstimator).GetMethod("Otsu", PrivateStatic)!;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the printer.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheSixRedsTraceTests(ITestOutputHelper output) => _output = output;

    private static double[] Window(CwProbabilisticStream stream)
    {
        var envelope = (double[])EnvelopeField.GetValue(stream)!;
        var window = new double[stream.EnvelopeHops];

        Array.Copy(envelope, window, window.Length);

        return window;
    }

    private static double Now(CwProbabilisticStream stream)
        => (long)HopsSeenField.GetValue(stream)! * CwProbabilisticDecoder.HopMilliseconds / 1000.0;

    private static bool Held(CwProbabilisticStream stream)
        => (bool)StructureHeldField.GetValue(stream)!;

    private static CwUnitEstimator.CwGapLengths HeldGaps(CwProbabilisticStream stream)
        => (CwUnitEstimator.CwGapLengths)HeldGapsField.GetValue(stream)!;

    private static double Median(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return double.NaN;
        }

        var sorted = values.OrderBy(v => v).ToArray();

        return sorted[sorted.Length / 2];
    }

    /// <summary>The estimator's two clusters, short and long, as medians of their members.</summary>
    private static string Clusters(IReadOnlyList<double> values)
    {
        if (values.Count < 2)
        {
            return "n/a";
        }

        var (low, high) = ((double, double))TwoMeansMethod.Invoke(null, new object[] { values })!;
        var boundary = Math.Sqrt(low * high);
        var shortMembers = values.Where(v => v <= boundary).ToList();
        var longMembers = values.Where(v => v > boundary).ToList();

        return $"short {Median(shortMembers):0} ms x{shortMembers.Count}, "
            + $"long {Median(longMembers):0} ms x{longMembers.Count}";
    }

    /// <summary>What the estimator makes of a window, and what the stream holds.</summary>
    private static string Estimator(CwProbabilisticStream stream, double[] window)
    {
        var measured = CwUnitEstimator.Measure(window, CwProbabilisticDecoder.HopMilliseconds);
        var (marks, gaps) = CwUnitEstimator.Elements(window, CwProbabilisticDecoder.HopMilliseconds);
        var held = HeldGaps(stream);

        return $"est unit {measured.UnitMilliseconds:0.0} ms = {measured.WordsPerMinute:0.0} wpm "
            + $"(dit-mark {measured.DitMarkMilliseconds:0}, elem-gap {measured.ElementGapMilliseconds:0}, "
            + $"marks {marks.Count}) | marks {Clusters(marks)} | gaps {Clusters(gaps)} "
            + $"| structure {Held(stream)} held {held.ElementMilliseconds:0}/{held.CharacterMilliseconds:0}/"
            + $"{held.WordMilliseconds:0}";
    }

    /// <summary>Group B: every settled character of the easy tier, with the speed it settled at.</summary>
    /// <param name="name">The fixture.</param>
    [Theory]
    [InlineData("coverage-easy")]
    [InlineData("exchange-easy")]
    [InlineData("tightfist-easy")]
    public void GroupBEasyTierBySettle(string name)
    {
        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == name);
        var sent = recipe.Text.Replace("^", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        var audio = WavAudio.Read(Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var stream = decoder.Stream;

        var settled = new List<CwCharacter>();
        var rows = new List<string>();
        var lastLattice = double.NaN;
        var mixHz = double.NaN;
        var original = (Action<ToneReading>)OnReadingField.GetValue(decoder)!;

        OnReadingField.SetValue(decoder, (Action<ToneReading>)(r =>
        {
            original(r);

            if (stream.ToneHz != mixHz)
            {
                mixHz = stream.ToneHz;
                _output.WriteLine($"B-mix | {name} | {r.SampleIndex / (double)audio.SampleRate:0.000} s "
                    + $"| stream mixes at {stream.ToneHz:0.0} Hz | tracker {decoder.Tracker.ToneHz:0.0} "
                    + $"measured {decoder.Tracker.HasMeasuredPitch} | retunes {decoder.Tracker.Retunes} "
                    + $"follows {decoder.Tracker.Follows} | sender {recipe.ToneHz:0}");
            }
        }));

        decoder.CharacterSettled += c =>
        {
            settled.Add(c);

            var window = Window(stream);
            var lattice = stream.Last.WordsPerMinute;

            rows.Add($"settled at {Now(stream):0.00} s | char end {c.At.TotalSeconds:0.00} s "
                + $"| '{c.Text}' {c.Pattern} | lattice {lattice:0.0} wpm, fit/sender "
                + $"{lattice / recipe.WordsPerMinute:0.00} | {Estimator(stream, window)}");
        };

        decoder.LeadingEdge += _ =>
        {
            var lattice = stream.Last.WordsPerMinute;

            if (lattice != lastLattice)
            {
                _output.WriteLine($"B-read | {name} | {Now(stream):0.00} s | lattice {lattice:0.0} "
                    + $"| {Estimator(stream, Window(stream))}");
                lastLattice = lattice;
            }
        };

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        var matches = CwAlignment.Align(settled, recipe.Text);

        _output.WriteLine($"B | {name} | sender {recipe.WordsPerMinute:0.0} wpm, dit {recipe.DitMilliseconds:0} ms, "
            + $"element gap {recipe.ElementGapMilliseconds:0}, character gap {recipe.CharacterGapMilliseconds:0}, "
            + $"word gap {recipe.WordGapMilliseconds:0} | {settled.Count} settled");

        for (var i = 0; i < settled.Count; i++)
        {
            var c = settled[i];
            var m = matches[i];
            var stranger = !c.IsWordGap && !c.IsUnreadable && c.Text.Length == 1
                && !sent.Contains(c.Text[0], StringComparison.Ordinal);

            _output.WriteLine($"B | {name} | {i:00} | {m.Kind} expected '{m.Expected}'"
                + (stranger ? " STRANGER" : "") + (c.IsUnreadable ? " UNREADABLE" : "")
                + $" | {rows[i]}");
        }

        _output.WriteLine($"B | {name} | settled '{string.Concat(settled.Select(c => c.Text))}'");
    }

    /// <summary>The runs a two-level trigger finds, with where each starts.</summary>
    private static List<(bool Down, double From, double Length, double PeakDb)> Runs(double[] window)
    {
        var db = window.Select(v => 20 * Math.Log10(Math.Max(v, 1e-12))).ToArray();
        var cut = (double)OtsuMethod.Invoke(null, new object[] { db })!;
        var on = cut + CwUnitEstimator.HysteresisDb;
        var off = cut - CwUnitEstimator.HysteresisDb;
        var runs = new List<(bool, double, double, double)>();
        var keyDown = db[0] > on;
        var start = 0;
        var hop = CwProbabilisticDecoder.HopMilliseconds;

        for (var i = 1; i < db.Length; i++)
        {
            if (!(keyDown ? db[i] < off : db[i] > on))
            {
                continue;
            }

            runs.Add((keyDown, start * hop / 1000.0, (i - start) * hop, db[start..i].Max()));
            keyDown = !keyDown;
            start = i;
        }

        return runs;
    }

    /// <summary>Group D: #6's and #15's cases, read by read, per seed.</summary>
    /// <param name="wordsPerMinute">The sender.</param>
    /// <param name="prefix">The run-up, or none.</param>
    [Theory]
    [InlineData(12, "VVV ")]
    [InlineData(25, "")]
    public void GroupDSpeedAndStart(int wordsPerMinute, string prefix)
    {
        const string call = "CQ CQ DE N0CALL N0CALL K";
        var label = $"{wordsPerMinute} wpm {(prefix.Length == 0 ? "bare" : "run-up")}";

        foreach (var seed in new[] { 7919, 104729, 15485863 })
        {
            var audio = CwSignal.Generate(new CwSignalRequest(
                prefix + call, WordsPerMinute: wordsPerMinute, ToneHz: 640, Amplitude: 0.5,
                NoiseAmplitude: CwSensitivity.NoiseFor(18.0), Seed: seed));

            var decoder = new CwDecoder(audio.SampleRate, CwSignal.DefaultToneHz);
            var stream = decoder.Stream;
            var settled = new List<CwCharacter>();
            var reads = 0;
            var mixHz = double.NaN;
            var original = (Action<ToneReading>)OnReadingField.GetValue(decoder)!;

            OnReadingField.SetValue(decoder, (Action<ToneReading>)(r =>
            {
                original(r);

                if (stream.ToneHz != mixHz)
                {
                    mixHz = stream.ToneHz;
                    _output.WriteLine($"D-mix | {label} | seed {seed} | {r.SampleIndex / (double)audio.SampleRate:0.000} s "
                        + $"| stream mixes at {stream.ToneHz:0.0} Hz | tracker {decoder.Tracker.ToneHz:0.0} "
                        + $"measured {decoder.Tracker.HasMeasuredPitch} | retunes {decoder.Tracker.Retunes} "
                        + $"follows {decoder.Tracker.Follows} | sender 640");
                }
            }));

            decoder.CharacterSettled += settled.Add;
            decoder.LeadingEdge += _ =>
            {
                reads++;

                var window = Window(stream);
                var last = stream.Last;
                var gaps = Held(stream)
                    ? new[]
                    {
                        HeldGaps(stream).ElementMilliseconds,
                        HeldGaps(stream).CharacterMilliseconds,
                        HeldGaps(stream).WordMilliseconds,
                    }
                    : null;

                var grid = new StringBuilder();
                var bestWpm = 0.0;
                var bestRatio = double.NegativeInfinity;

                for (var wpm = CwProbabilisticDecoder.SlowestWpm;
                     wpm <= CwProbabilisticDecoder.FastestWpm + 1e-9;
                     wpm += CwProbabilisticDecoder.WpmStep)
                {
                    var at = CwProbabilisticDecoder.Decode(window, stream.ToneHz, wpm, gaps);

                    grid.Append($" {wpm:0}:{at.LikelihoodRatio:0.000}");

                    if (at.LikelihoodRatio > bestRatio)
                    {
                        bestRatio = at.LikelihoodRatio;
                        bestWpm = wpm;
                    }
                }

                _output.WriteLine($"D-read | {label} | seed {seed} | {Now(stream):0.00} s | window {window.Length} hops "
                    + $"| lattice {last.WordsPerMinute:0.0} wpm ratio {last.LikelihoodRatio:0.000} "
                    + $"| grid best {bestWpm:0} at {bestRatio:0.000} | {Estimator(stream, window)} | '{last.Text}'");
                _output.WriteLine($"D-grid | {label} | seed {seed} | {Now(stream):0.00} s |{grid}");

                if (reads <= 3)
                {
                    foreach (var c in last.Characters)
                    {
                        var end = (Now(stream) - (window.Length * CwProbabilisticDecoder.HopMilliseconds / 1000.0))
                            + (c.EndHop * CwProbabilisticDecoder.HopMilliseconds / 1000.0);

                        _output.WriteLine($"D-lattice | {label} | seed {seed} | read {reads} | '{c.Text}' {c.Pattern} "
                            + $"ends {end:0.000} s span {c.SpanHops * CwProbabilisticDecoder.HopMilliseconds:0} ms "
                            + $"margin {c.SpanMargin:0.00}");
                    }
                }

                if (reads == 1)
                {
                    foreach (var run in Runs(window).Where(r => r.From < 2.0))
                    {
                        _output.WriteLine($"D-first | {label} | seed {seed} | {(run.Down ? "mark" : "gap ")} "
                            + $"from {run.From:0.000} s length {run.Length:0} ms peak {run.PeakDb:0.0} dB");
                    }
                }
            };

            using (var source = new BufferedAudioSource(audio))
            {
                decoder.Listen(source);
                source.PumpAll();
            }

            decoder.Flush();

            var matches = CwAlignment.Align(settled, prefix + call);
            var kinds = new StringBuilder();

            foreach (var m in matches.Where(m => !m.Decoded.IsWordGap))
            {
                kinds.Append(m.Kind switch
                {
                    CwMatchKind.Correct => m.Decoded.Text,
                    CwMatchKind.Wrong => $"[{m.Decoded.Text}/{m.Expected}]",
                    _ => $"(+{m.Decoded.Text})",
                });
            }

            var share = (double)matches.Count(m => m.Kind == CwMatchKind.Correct
                && !m.Decoded.IsWordGap && m.Expected != "V") / call.Count(c => c != ' ');

            _output.WriteLine($"D | {label} | seed {seed} | share {share:0.00} | final wpm "
                + $"{decoder.Reading.WordsPerMinute:0} | settled '{string.Concat(settled.Select(c => c.Text))}' "
                + $"| kinds {kinds}");

            foreach (var c in settled.Take(6))
            {
                _output.WriteLine($"D-settled | {label} | seed {seed} | '{c.Text}' {c.Pattern} at {c.At.TotalSeconds:0.000} s "
                    + $"wpm {c.WordsPerMinute}");
            }
        }
    }

    /// <summary>Group C: every span the transmit guard marks, on the fixture and on two captures.</summary>
    /// <param name="file">The recording, under its folder.</param>
    [Theory]
    [InlineData("fixture:qsk-preamble")]
    [InlineData("capture:cw-2026-08-17-013347")]
    [InlineData("capture:cw-2026-08-17-013622")]
    public void GroupCGuardSpans(string file)
    {
        var (kind, name) = (file.Split(':')[0], file.Split(':')[1]);
        var folder = kind == "fixture" ? CwFixtureCatalogue.Folder : CapturedSignalTests.Folder;
        var audio = WavAudio.Read(Path.Combine(folder, name + ".wav"));
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var readings = new List<(ToneReading Reading, bool Muted)>();
        var settled = new List<CwCharacter>();
        var original = (Action<ToneReading>)OnReadingField.GetValue(decoder)!;

        OnReadingField.SetValue(decoder, (Action<ToneReading>)(r =>
        {
            readings.Add((r, decoder.Tracker.Guard.IsMuted));
            original(r);
        }));

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        double Seconds(ToneReading r) => r.SampleIndex / (double)audio.SampleRate;

        var spans = new List<(int From, int To)>();
        int? open = null;

        for (var i = 0; i < readings.Count; i++)
        {
            if (readings[i].Reading.Blocked && open is null)
            {
                open = i;
            }
            else if (!readings[i].Reading.Blocked && open is { } o)
            {
                spans.Add((o, i));
                open = null;
            }
        }

        if (open is { } last)
        {
            spans.Add((last, readings.Count));
        }

        var hopSeconds = hop / (double)audio.SampleRate;

        _output.WriteLine($"C | {name} | {audio.Samples.Length / (double)audio.SampleRate:0.0} s at {audio.SampleRate} Hz "
            + $"| tracker hop {hopSeconds * 1000:0.0} ms | readings {readings.Count} | spans {spans.Count} "
            + $"| own transmit {decoder.Report.OwnTransmitSeconds:0.00} s | transmissions {decoder.Tracker.Guard.Transmissions} "
            + $"| suspended chunks {decoder.SuspendedChunks} | decoding suspended {decoder.DecodingSuspended}");

        var beforeWindow = (int)Math.Round(0.5 / hopSeconds);

        foreach (var (from, to) in spans)
        {
            var inside = readings.Skip(from).Take(to - from).ToList();
            var muted = inside.Where(r => r.Muted).Select(r => r.Reading.BroadbandDbfs).ToList();
            var before = readings.Skip(Math.Max(0, from - beforeWindow)).Take(from - Math.Max(0, from - beforeWindow))
                .Where(r => !r.Reading.Blocked).ToList();
            var mutedRuns = 0;

            for (var i = 0; i < inside.Count; i++)
            {
                if (inside[i].Muted && (i == 0 || !inside[i - 1].Muted))
                {
                    mutedRuns++;
                }
            }

            _output.WriteLine($"C-span | {name} | from {Seconds(readings[from].Reading):0.000} s "
                + $"| length {(to - from) * hopSeconds:0.000} s | muted {muted.Count * hopSeconds:0.000} s in {mutedRuns} runs "
                + $"| muted broadband min {(muted.Count > 0 ? muted.Min() : double.NaN):0.0} "
                + $"median {Median(muted):0.0} max {(muted.Count > 0 ? muted.Max() : double.NaN):0.0} dBFS "
                + $"| 0.5 s before: broadband median {Median(before.Select(r => r.Reading.BroadbandDbfs).ToList()):0.0} dBFS, "
                + $"tone power median {Median(before.Select(r => r.Reading.PowerDb).ToList()):0.0} dB, "
                + $"snr median {Median(before.Where(r => r.Reading.HasNoise).Select(r => r.Reading.SnrDb).ToList()):0.0} dB "
                + $"max {(before.Any(r => r.Reading.HasNoise) ? before.Where(r => r.Reading.HasNoise).Max(r => r.Reading.SnrDb) : double.NaN):0.0} "
                + $"| tone {readings[from].Reading.ToneHz:0}");
        }

        var unblocked = readings.Where(r => !r.Reading.Blocked).ToList();
        var level = unblocked.Select(r => r.Reading.BroadbandDbfs).ToList();

        _output.WriteLine($"C | {name} | unblocked broadband median {Median(level):0.0} dBFS, min "
            + $"{(level.Count > 0 ? level.Min() : double.NaN):0.0} | muted readings {readings.Count(r => r.Muted)} "
            + $"| blocked readings {readings.Count(r => r.Reading.Blocked)}");

        foreach (var c in settled.Where(c => !c.IsWordGap))
        {
            var end = c.At.TotalSeconds;
            var start = end - (c.SpanHops * CwProbabilisticDecoder.HopMilliseconds / 1000.0);
            var nearest = spans.Count == 0
                ? double.NaN
                : spans.Min(s =>
                {
                    var a = Seconds(readings[s.From].Reading);
                    var b = Seconds(readings[Math.Min(s.To, readings.Count - 1)].Reading);

                    return end < a ? a - end : start > b ? start - b : 0;
                });

            _output.WriteLine($"C-char | {name} | '{c.Text}' {c.Pattern} {start:0.000} to {end:0.000} s "
                + $"| distance to nearest span {nearest:0.000} s");
        }

        _output.WriteLine($"C | {name} | settled '{string.Concat(settled.Select(c => c.Text))}'");
    }
}
