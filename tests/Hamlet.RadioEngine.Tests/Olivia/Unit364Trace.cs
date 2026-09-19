using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 364 task 1: **the trace before the rows are built** - items 4 and 5, the gaps
/// the retire window is checked against and the lag from the row's side.
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit363Trace`. It asserts
/// nothing, so it cannot become a wall, and it is not on the carry-forward line. Items 1 to 3 are
/// read from the source and reported, not measured here.</para>
/// <para>**FED AS THE LISTENER IS FED**: each shipped Olivia file, hash-checked, a quarter-second at
/// a time, then flushed as a recording ends. When a block's text reached a channel is the
/// listener's own sample count at the moment its `olivia_block` event was written.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class Unit364Trace
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit364Trace(ITestOutputHelper output) => _output = output;

    private static OliviaFormat Format => OliviaData.Current.Format!;

    private static OliviaTiming Timing => OliviaData.Current.Timing!;

    /// <summary>
    /// Item 4: on every shipped Olivia file, per channel, the longest gap between the ends of two
    /// accepted blocks, the longest the listener went without learning of a new accepted block
    /// while the station was still sending, and the seconds from the last accepted block to the
    /// file's end.
    /// </summary>
    [Fact]
    public void TheGapsTheRetireWindowIsCheckedAgainst()
    {
        foreach (var v in Format.Variants.Where(v => Timing.SecondsPerCharacter.ContainsKey(v.Name)))
        {
            var spc = Timing.SecondsPerCharacter[v.Name];

            _output.WriteLine(
                $"{v.Name,-8}: {spc:0.00000} s a character, block {BlockSeconds(v):0.000} s ({v.BitsPerSymbol} characters), "
                + $"24 x = {24 * spc:0.000} s");
        }

        foreach (var file in new[]
                 {
                     "olivia-8-250-cq-rsid.wav", "olivia-16-500-qso-rsid.wav", "olivia-32-1000-qso-rsid.wav",
                     "olivia-8-250-qso-norsid.wav", "olivia-16-500-qso-snr-10db.wav", "olivia-16-500-qso-snr-16db.wav",
                     "olivia-two-signals-rsid.wav", "olivia-noise-only-30s.wav",
                 })
        {
            var run = Run(file);

            _output.WriteLine($"{file}: {run.Seconds:0.00} s, channels {run.Channels.Count}");

            foreach (var c in run.Channels)
            {
                var spc = Timing.SecondsPerCharacter[c.Variant];
                var ends = c.Blocks.Where(b => b.Accepted).Select(b => b.EndSeconds).ToList();
                var gaps = ends.Zip(ends.Skip(1), (a, b) => b - a).DefaultIfEmpty(0).ToList();
                var lastEnd = ends.DefaultIfEmpty(c.ReadFromSeconds).Last();

                _output.WriteLine(
                    $"   {c.Id} {c.Variant} by {c.Found}, read from {c.ReadFromSeconds:0.000} s: accepted {ends.Count}, rejected {c.Blocks.Count(b => !b.Accepted)}; "
                    + $"longest gap between accepted block ends {gaps.Max():0.000} s = {gaps.Max() / spc:0.0} characters; "
                    + $"longest the listener went with no new accepted block while he was sending {c.LongestUnheardWhileSending:0.000} s "
                    + $"= {c.LongestUnheardWhileSending / spc:0.0} characters (smallest whole factor that holds {Math.Floor(c.LongestUnheardWhileSending / spc) + 1}); "
                    + $"last accepted block ends {lastEnd:0.000} s, {run.Seconds - lastEnd:0.000} s before the file's end");
            }
        }
    }

    /// <summary>
    /// Item 5: on the two-signal file and the 16/500 QSO, when each accepted block ended in the
    /// audio and when its text reached the channel - first, median and worst.
    /// </summary>
    [Fact]
    public void TheLagFromTheRowsSide()
    {
        foreach (var file in new[] { "olivia-two-signals-rsid.wav", "olivia-16-500-qso-rsid.wav" })
        {
            var run = Run(file);

            foreach (var c in run.Channels)
            {
                var lags = c.Blocks.Where(b => b.Accepted).Select(b => b.ReachedSeconds - b.EndSeconds).ToList();

                if (lags.Count == 0)
                {
                    _output.WriteLine($"{file} {c.Variant}: no accepted block");
                    continue;
                }

                var sorted = lags.OrderBy(l => l).ToList();
                var first = c.Blocks.First(b => b.Accepted);

                _output.WriteLine(
                    $"{file} {c.Variant} at {c.LastCenterHz:0.00} Hz: {lags.Count} accepted blocks; first block ended {first.EndSeconds:0.000} s "
                    + $"and reached the channel at {first.ReachedSeconds:0.000} s, lag {lags[0]:0.000} s; median {sorted[sorted.Count / 2]:0.000} s; "
                    + $"worst {sorted[^1]:0.000} s ({sorted[^1] / BlockSeconds(Format.Variant(c.Variant)!):0.00} blocks); "
                    + $"the flush's blocks: {c.Blocks.Count(b => b.Accepted && b.ByFlush)}");
            }
        }
    }

    private static double BlockSeconds(OliviaVariant v) => Format.SymbolsPerBlock * v.SymbolSeconds;

    private static Traced Run(string file)
    {
        var fixture = OliviaFixtures.Load(file);
        var audio = WavAudio.Read(fixture.Path);
        var channels = new Dictionary<int, TracedChannel>();
        var listener = default(OliviaListener);
        var flushing = false;
        var telemetry = new Sink((name, data) =>
        {
            var id = (int)data["id"]!;
            var now = listener!.SamplesSeen / (double)audio.SampleRate;

            if (name == "olivia_channel")
            {
                if ((string?)data["state"] == "opened")
                {
                    channels[id] = new TracedChannel(id, (string)data["variant"]!, (string)data["found"]!, (double)data["readFromSeconds"]!);
                }

                return;
            }

            if (name != "olivia_block")
            {
                return;
            }

            var c = channels[id];
            var variant = Format.Variant(c.Variant)!;
            var end = (double)data["atSeconds"]! + BlockSeconds(variant);

            c.Blocks.Add(new TracedBlock(end, now, (bool)data["accepted"]!, flushing));

            if ((bool)data["accepted"]!)
            {
                c.LastCenterHz = (double)data["centerHz"]!;
            }
        });

        listener = new OliviaListener(
            Format, OliviaData.Current.Rsid!, audio.SampleRate, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz, telemetry);

        var piece = audio.SampleRate / 4;

        for (var at = 0; at < audio.Samples.Length; at += piece)
        {
            listener.Add(audio.Samples.AsSpan(at, Math.Min(piece, audio.Samples.Length - at)));

            var now = listener.SamplesSeen / (double)audio.SampleRate;

            foreach (var c in channels.Values)
            {
                c.Silences.Add((now, now - c.Blocks.Where(b => b.Accepted).Select(b => b.EndSeconds).DefaultIfEmpty(c.ReadFromSeconds).Last()));
            }
        }

        flushing = true;
        listener.Flush();

        foreach (var c in channels.Values)
        {
            // **WHILE HE WAS STILL SENDING**: up to the moment the last accepted block reached the
            // channel. After that the silence is the station having stopped.
            var lastReached = c.Blocks.Where(b => b.Accepted && !b.ByFlush).Select(b => b.ReachedSeconds).DefaultIfEmpty(0).Max();

            c.LongestUnheardWhileSending = c.Silences.Where(s => s.At <= lastReached).Select(s => s.Silence).DefaultIfEmpty(0).Max();
        }

        return new Traced(audio.Samples.Length / (double)audio.SampleRate, channels.Values.OrderBy(c => c.Id).ToList());
    }

    private sealed record Traced(double Seconds, IReadOnlyList<TracedChannel> Channels);

    private sealed record TracedBlock(double EndSeconds, double ReachedSeconds, bool Accepted, bool ByFlush);

    private sealed class TracedChannel(int id, string variant, string found, double readFromSeconds)
    {
        public int Id { get; } = id;

        public string Variant { get; } = variant;

        public string Found { get; } = found;

        public double ReadFromSeconds { get; } = readFromSeconds;

        public List<TracedBlock> Blocks { get; } = new();

        public List<(double At, double Silence)> Silences { get; } = new();

        public double LongestUnheardWhileSending { get; set; }

        public double LastCenterHz { get; set; }
    }

    private sealed class Sink(Action<string, IReadOnlyDictionary<string, object?>> seen) : ITelemetry
    {
        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => seen(eventName, data ?? new Dictionary<string, object?>());
    }
}
