using System.Diagnostics;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Cw;

/// <summary>
/// Work instruction 417 task 1: the sidecar's `tonePeak` line as the sheet writes
/// it today, beside a tone-over-noise figure measured over the capture's own audio.
/// </summary>
/// <remarks>
/// <para>**A FACT THAT ASSERTS NOTHING.** It prints, per capture, the line the
/// writer's own <c>TonePeakRecordLine</c> gives from a fresh decoder that heard only
/// this file, the figure the saved sidecar carried from the live evening, and
/// <see cref="ToneOverNoiseByHand"/> at the pitch the decoder tracked, with the wall
/// time that measurement took on the file's length.</para>
/// <para>**WHETHER THE CAPTURE HOLDS KEYING** is the keying meter's verdict,
/// regenerated as work instruction 411 did, one six-second window a second: how
/// many windows it called keying out of how many it read. It shares nothing with
/// the decoder.</para>
/// </remarks>
public sealed class WhatTheTonePeakIsAboutTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public WhatTheTonePeakIsAboutTests(ITestOutputHelper output) => _output = output;

    /// <summary>The held figure and the recording's own, capture by capture.</summary>
    [Fact]
    public void TheHeldFigureIsPrintedBesideTheRecordingsOwn()
    {
        var root = Root();
        var captured = Path.Combine(root, "tests", "fixtures", "cw", "captured");

        string[] stamps =
        [
            "cw-2026-08-20-014854",
            "cw-2026-08-20-014935",
            "cw-2026-08-17-013347",
            "cw-2026-09-23-173723",
            "cw-2026-08-22-014113",
        ];

        var rows = new List<(string Stamp, double Held, double Recording)>();

        _output.WriteLine(
            "stamp | seconds | saved sidecar tonePeak | replayed held | toneHz | hasTone "
            + "| pitchMeasured | recording figure | at the pitch regardless | keying windows "
            + "| measure ms");

        foreach (var stamp in stamps)
        {
            var wav = Directory
                .GetFiles(captured, stamp + ".wav", SearchOption.AllDirectories)
                .Single();
            var audio = WavAudio.Read(wav);
            var report = Replay(audio);

            var sidecar = Path.ChangeExtension(wav, ".txt");
            var saved = File.Exists(sidecar)
                ? File.ReadLines(sidecar)
                    .FirstOrDefault(l => l.StartsWith("tonePeak", StringComparison.Ordinal))
                    ?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1] ?? "no line"
                : "no sidecar";

            var clock = Stopwatch.StartNew();
            var regardless = double.IsNaN(report.ToneHz) || report.ToneHz <= 0
                ? double.NaN
                : ToneOverNoiseByHand.Peak(audio, report.ToneHz);
            clock.Stop();

            var measurable = report.HasTone && report.PitchWasMeasured;
            var recording = measurable ? regardless : double.NaN;

            var (keyed, windows) = KeyingWindows(audio);

            _output.WriteLine(
                $"{stamp} | {audio.Samples.Length / (double)audio.SampleRate:0.0} | {saved} "
                + $"| {report.SnrDb:0.0} | {report.ToneHz:0.0} | {report.HasTone} "
                + $"| {report.PitchWasMeasured} "
                + $"| {(measurable ? recording.ToString("0.0") : "not measured")} "
                + $"| {regardless:0.0} | {keyed} of {windows} | {clock.ElapsedMilliseconds}");
            _output.WriteLine("  today: " + MainWindowViewModel.TonePeakRecordLine(report));

            if (!double.IsNaN(report.ToneHz) && report.ToneHz > 0)
            {
                _output.WriteLine(
                    $"  over its own quietest fifth at {report.ToneHz:0.0} Hz: "
                    + $"{ToneOverNoiseByHand.PeakOverOwnFloor(audio, report.ToneHz):0.0} dB");

                var (p5, p20, p50, p95) = ToneOverNoiseByHand.Percentiles(audio, report.ToneHz);

                _output.WriteLine(
                    $"  tone bin percentiles dB: 5th {p5:0.0}, 20th {p20:0.0}, 50th {p50:0.0}, 95th {p95:0.0}");
            }

            _output.WriteLine(
                "  broadband dBFS per second: "
                + string.Join(" ", ToneOverNoiseByHand.SecondsLevel(audio).Select(l => $"{l:0}")));

            _output.WriteLine(
                "  mean spectrum, dB under the loudest: "
                + string.Join(" ", ToneOverNoiseByHand.Spectrum(audio)
                    .Select(s => $"{s.Hz:0}:{s.Db:0}")));

            rows.Add((stamp, report.SnrDb, recording));
        }

        _output.WriteLine(
            "ordered by the held figure: "
            + string.Join(" > ", rows.OrderByDescending(r => r.Held)
                .Select(r => $"{r.Stamp[3..]} {r.Held:0.0}")));
        _output.WriteLine(
            "ordered by the recording figure: "
            + string.Join(" > ", rows.OrderByDescending(r => double.IsNaN(r.Recording) ? double.NegativeInfinity : r.Recording)
                .Select(r => $"{r.Stamp[3..]} {(double.IsNaN(r.Recording) ? "not measured" : r.Recording.ToString("0.0"))}")));

        var longest = Directory
            .GetFiles(captured, "*.wav", SearchOption.AllDirectories)
            .Select(WavAudio.Read)
            .OrderByDescending(a => a.Samples.Length / (double)a.SampleRate)
            .First();

        var timing = Stopwatch.StartNew();
        ToneOverNoiseByHand.Peak(longest, 600);
        timing.Stop();

        _output.WriteLine(
            $"longest capture in the tree {longest.Samples.Length / (double)longest.SampleRate:0.0} s "
            + $"at {longest.SampleRate} Hz: the by-hand measurement took {timing.ElapsedMilliseconds} ms");
    }

    private static CwDecodeReport Replay(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate);

        using (var source = new BufferedAudioSource(audio))
        {
            decoder.Listen(source);
            source.PumpAll();
            decoder.Flush();
        }

        return decoder.Report;
    }

    private static (int Keyed, int Windows) KeyingWindows(MonoAudio audio)
    {
        var meter = new CwKeyingMeter();
        var window = (int)(CwKeyingThresholds.Window.TotalSeconds * audio.SampleRate);
        var keyed = 0;
        var windows = 0;

        for (var end = window; end <= audio.Samples.Length; end += audio.SampleRate)
        {
            meter.Update(new MonoAudio(audio.SampleRate, audio.Samples[(end - window)..end]));
            windows++;

            if (meter.Reading.Verdict == KeyingVerdict.Keying)
            {
                keyed++;
            }
        }

        return (keyed, windows);
    }

    private static string Root()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
               && !Directory.Exists(Path.Combine(here.FullName, "tests", "fixtures")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return here!.FullName;
    }
}
