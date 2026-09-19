using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 363 task 3: **the engine's Olivia listener hears everyone** - one channel per
/// station, heard by RSID or found blind, each with its own reader and its own text as blocks
/// arrive (step 3 criterion 3.1, the engine half).
/// </summary>
/// <remarks>
/// <para>**FED AS IT WOULD BE FED, AND NEVER TOLD WHERE ANYONE IS** (decisions Y, Z, I and O). Every
/// file is hash-checked, then handed to the listener a quarter of a second at a time, then flushed
/// as a recording ends. The manifest's variant, center and text only check what came out.</para>
/// <para>**DECISION Z'S GATE**: each shipped file through the listener reads what
/// <see cref="OliviaDemodulator.Decode(MonoAudio, double)"/> read over the whole file, printed beside
/// unit 362's numbers; a clean file off 0.0000 is a regression in the listener.</para>
/// <para>**3.1 IS THE ENGINE HALF ONLY**: the rows are the next unit's, drawn from these channels.</para>
/// <para>**RUN ALONE, BECAUSE THE CPU IS THE PROCESS'S** (<see cref="CpuMeasuredAlone"/>).
/// **COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class TheOliviaListenerTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each feed is printed.</param>
    public TheOliviaListenerTests(ITestOutputHelper output) => _output = output;

    private static OliviaFormat Format => OliviaData.Current.Format!;

    /// <summary>
    /// **3.1, engine half, and decision AB: the two-signal file yields exactly two channels, 8/250
    /// within 5 Hz of 1000 and 16/500 within 5 Hz of 2000, each found by RSID, each reading its own
    /// half of the manifest text at CER 0.05 or under, with the other station's callsign nowhere in
    /// it and no character no accepted block could carry.**
    /// </summary>
    [Fact]
    public void TwoStationsAreTwoChannelsEachWithItsOwnText()
    {
        var fixture = OliviaFixtures.Load("olivia-two-signals-rsid.wav");
        var feed = Feed(fixture, null);
        var halves = fixture.Text.Split(" | ");

        // The manifest's note says the first half is the 8/250 station's: the check, not the input.
        var expected = new[] { ("8/250", 1000.0, halves[0], halves[1]), ("16/500", 2000.0, halves[1], halves[0]) };

        Assert.Equal(2, feed.Listener.Channels.Count);
        Assert.Equal(2, feed.MostChannels);

        foreach (var (variant, centerHz, own, other) in expected)
        {
            var channel = Assert.Single(feed.Listener.Channels, c => c.Variant == variant);
            var cer = OliviaFixtures.CharacterErrorRate(channel.Text, own);
            var otherCall = CallsignOf(other);

            Assert.Equal(OliviaListener.FoundByRsid, channel.Found);
            Assert.InRange(channel.CenterHz, centerHz - 5, centerHz + 5);
            Assert.True(cer <= 0.05, $"{variant}: CER {cer:0.0000} over 0.05");
            Assert.DoesNotContain(otherCall, channel.Text, StringComparison.Ordinal);
            Assert.InRange(channel.Text.Length, 0, channel.BlocksDecoded * Format.Variant(variant)!.BitsPerSymbol);
        }
    }

    /// <summary>
    /// **Decision Z's regression rows: each RSID file through the listener is one channel, found by
    /// RSID at the manifest's variant within 5 Hz of its center, reading at its ceiling; one channel
    /// across the whole feed (decision AA).**
    /// </summary>
    /// <param name="file">The fixture.</param>
    /// <param name="ceiling">Its ceiling: 0 for a clean file, 0.05 for the -10 dB file.</param>
    /// <param name="wholeFile">Unit 362's whole-file reading, printed beside.</param>
    [Theory]
    [InlineData("olivia-8-250-cq-rsid.wav", 0.0, "CER 0.0000, 13 blocks")]
    [InlineData("olivia-16-500-qso-rsid.wav", 0.0, "CER 0.0000, 63 blocks")]
    [InlineData("olivia-32-1000-qso-rsid.wav", 0.0, "CER 0.0000, 51 blocks")]
    [InlineData("olivia-16-500-qso-snr-10db.wav", 0.05, "CER 0.0000, 63 blocks")]
    public void EachAnnouncedFileIsOneChannelReadAsDecodeReadsIt(string file, double ceiling, string wholeFile)
    {
        var fixture = OliviaFixtures.Load(file);
        var feed = Feed(fixture, wholeFile);
        var channel = Assert.Single(feed.Listener.Channels);
        var cer = OliviaFixtures.CharacterErrorRate(channel.Text, fixture.Text);

        Assert.Equal(1, feed.MostChannels);
        Assert.Equal(OliviaListener.FoundByRsid, channel.Found);
        Assert.Equal(fixture.Variant, channel.Variant);
        Assert.InRange(channel.CenterHz, fixture.CenterHz - 5, fixture.CenterHz + 5);
        Assert.True(cer <= ceiling, $"{file}: CER {cer:0.0000} over {ceiling:0.0000}");
    }

    /// <summary>
    /// **Decision Z's blind row and decision AA: the no-RSID file is one channel found blind, 8/250
    /// within 5 Hz of 1000, reading at CER 0.05 or under; one channel across the whole feed.**
    /// </summary>
    [Fact]
    public void TheUnannouncedFileIsOneChannelFoundBlind()
    {
        var fixture = OliviaFixtures.Load("olivia-8-250-qso-norsid.wav");
        var feed = Feed(fixture, "blind search then Decode, CER 0.0000, 84 blocks");
        var channel = Assert.Single(feed.Listener.Channels);
        var cer = OliviaFixtures.CharacterErrorRate(channel.Text, fixture.Text);

        Assert.Equal(1, feed.MostChannels);
        Assert.Equal(OliviaListener.FoundBlind, channel.Found);
        Assert.Equal(fixture.Variant, channel.Variant);
        Assert.InRange(channel.CenterHz, fixture.CenterHz - 5, fixture.CenterHz + 5);
        Assert.True(cer <= 0.05, $"CER {cer:0.0000} over 0.05");
    }

    /// <summary>**Decision Z's noise row: thirty seconds of noise open no channel and show no character.**</summary>
    [Fact]
    public void NoiseOpensNothing()
    {
        var feed = Feed(OliviaFixtures.Load("olivia-noise-only-30s.wav"), "no candidate, no character");

        Assert.Equal(0, feed.MostChannels);
        Assert.Empty(feed.Listener.Channels);
    }

    /// <summary>
    /// **§R13, decision AD: on the two-signal file the listener writes a channel-open event for each
    /// station and a state for every block, mode olivia and the variant, with no decoded text and no
    /// callsign in any of it.**
    /// </summary>
    [Fact]
    public void TheListenersEventsCarryNoTextOrCallsign()
    {
        var fixture = OliviaFixtures.Load("olivia-two-signals-rsid.wav");
        var telemetry = new RecordingTelemetry();
        var feed = Feed(fixture, null, telemetry);
        var opened = telemetry.Events.Where(e => e.Name == "olivia_channel" && (string?)e.Data["state"] == "opened").ToList();
        var blocks = telemetry.Events.Where(e => e.Name == "olivia_block").ToList();

        Assert.Equal(2, opened.Count);
        Assert.Equal(new[] { "16/500", "8/250" }, opened.Select(e => (string)e.Data["variant"]!).OrderBy(v => v, StringComparer.Ordinal));
        Assert.All(opened, e => Assert.Equal(OliviaListener.FoundByRsid, e.Data["found"]));
        Assert.Equal(feed.Listener.Channels.Sum(c => c.BlocksDecoded), blocks.Count(e => (bool)e.Data["accepted"]!));

        foreach (var e in opened.Concat(blocks.Take(3)))
        {
            _output.WriteLine($"{e.Category} {e.Name} {JsonSerializer.Serialize(e.Data)}");
        }

        foreach (var e in telemetry.Events)
        {
            var json = JsonSerializer.Serialize(e.Data);

            Assert.Equal(TelemetryCategory.Psk31, e.Category);
            Assert.Equal("olivia", e.Data["mode"]);
            Assert.NotNull(e.Data["variant"]);

            // **NO TEXT AND NO CALLSIGN** (HM-DEC-018): not either station, not any word either sent.
            foreach (var word in fixture.Text.Split(' ', '\n', '|').Where(w => w.Length >= 3 && w.Any(char.IsLetter)))
            {
                Assert.DoesNotContain(word, json, StringComparison.Ordinal);
            }

            Assert.All(e.Data.Values, v => Assert.True(v is not string s || s.Length < 12));
        }
    }

    private static string CallsignOf(string half)
    {
        var words = half.Split(' ');
        var at = Array.IndexOf(words, "de");

        return words[at + 1];
    }

    private Fed Feed(OliviaFixture fixture, string? wholeFile, ITelemetry? telemetry = null)
    {
        var audio = WavAudio.Read(fixture.Path);
        var listener = new OliviaListener(
            Format, OliviaData.Current.Rsid!, audio.SampleRate, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz, telemetry);
        var piece = audio.SampleRate / 4;
        var most = 0;
        var opened = new List<string>();
        var worst = 0.0;
        var process = Process.GetCurrentProcess();
        var before = process.TotalProcessorTime;

        for (var at = 0; at < audio.Samples.Length; at += piece)
        {
            var started = Stopwatch.GetTimestamp();

            listener.Add(audio.Samples.AsSpan(at, Math.Min(piece, audio.Samples.Length - at)));
            worst = Math.Max(worst, Stopwatch.GetElapsedTime(started).TotalSeconds);
            most = Math.Max(most, listener.Channels.Count);

            foreach (var c in listener.Channels.Where(c => opened.All(o => !o.StartsWith($"{c.Id}:", StringComparison.Ordinal))))
            {
                opened.Add($"{c.Id}: {c.Variant} at {c.CenterHz:0.00} Hz by {c.Found}, opened at {c.OpenedSeconds:0.000} s");
            }
        }

        listener.Flush();
        most = Math.Max(most, listener.Channels.Count);

        process.Refresh();

        var cpu = (process.TotalProcessorTime - before).TotalSeconds;
        var seconds = audio.Samples.Length / (double)audio.SampleRate;

        _output.WriteLine(
            $"{System.IO.Path.GetFileName(fixture.Path)} (manifest {fixture.Variant} at {(double.IsNaN(fixture.CenterHz) ? "-" : fixture.CenterHz.ToString("0"))}): "
            + $"{seconds:0.00} s in {piece}-sample pieces; channels at the end {listener.Channels.Count}, most at once {most}; "
            + $"cpu {cpu:0.000} s, {cpu / seconds:0.000} of real time; longest single Add {worst:0.000} s wall"
            + (wholeFile is null ? "" : $"; unit 362 whole-file: {wholeFile}"));

        foreach (var line in opened)
        {
            _output.WriteLine("   opened " + line);
        }

        foreach (var c in listener.Channels)
        {
            var texts = fixture.Text.Split(" | ");

            _output.WriteLine(
                $"   channel {c.Id}: {c.Variant} at {c.CenterHz:0.00} Hz, found {c.Found}, opened {c.OpenedSeconds:0.000} s; "
                + $"blocks decoded {c.BlocksDecoded}, rejected {c.BlocksRejected}; characters {c.Text.Length}; "
                + "CER against " + string.Join(", ", texts.Select((t, i) => $"half {i} {OliviaFixtures.CharacterErrorRate(c.Text, t):0.0000}")));

            if (!texts.Contains(c.Text))
            {
                _output.WriteLine("      text: " + JsonSerializer.Serialize(c.Text));
            }
        }

        return new Fed(listener, most, cpu, worst);
    }

    private sealed record Fed(OliviaListener Listener, int MostChannels, double Cpu, double WorstAddSeconds);

    private sealed record Event(TelemetryCategory Category, string Name, IReadOnlyDictionary<string, object?> Data);

    private sealed class RecordingTelemetry : ITelemetry
    {
        public List<Event> Events { get; } = new();

        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => Events.Add(new Event(category, eventName, data ?? new Dictionary<string, object?>()));
    }
}
