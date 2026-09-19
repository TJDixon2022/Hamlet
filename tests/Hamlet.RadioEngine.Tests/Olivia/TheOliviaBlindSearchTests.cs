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
/// Work instruction 362 task 3: **the blind variant search finds the carrier that never announced
/// itself, names its variant and center from the audio alone, and finds nothing in noise** (step 2
/// criterion 2.3).
/// </summary>
/// <remarks>
/// <para>**THE SEARCH IS NEVER TOLD THE ANSWER** (decision O). It is handed the audio and the
/// passband and nothing else: no variant, no center, no start time. The manifest's variant and
/// center only check what it named.</para>
/// <para>**THE STATED TIME IS TWO NUMBERS** (decision Q): the seconds of audio the search consumed
/// before it named the carrier, and its CPU seconds. The decode that follows is measured against
/// 2.5's twenty seconds on its own.</para>
/// <para>**EVERY FILE IS HASHED FIRST AND NOTHING IS LOOSENED**: 5 Hz on the center, CER 0.05.
/// **COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// <para>**RUN ALONE, BECAUSE THE CPU IS THE PROCESS'S** (<see cref="CpuMeasuredAlone"/>).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class TheOliviaBlindSearchTests
{
    private const double CpuCeilingSeconds = 20.0;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each search is printed.</param>
    public TheOliviaBlindSearchTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **2.3 and decision Q: the no-RSID 8/250 file yields one candidate, 8/250 within 5 Hz of 1000,
    /// and the decode that follows reads the manifest text at CER 0.05 or under.**
    /// </summary>
    [Fact]
    public void TheUnannouncedCarrierIsFoundAndRead()
    {
        var fixture = OliviaFixtures.Load("olivia-8-250-qso-norsid.wav");
        var audio = WavAudio.Read(fixture.Path);

        var (search, searchCpu) = Timed(() => new OliviaBlindSearch(Format).Search(audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz));

        Print("olivia-8-250-qso-norsid.wav", search, searchCpu);

        var found = Assert.Single(search.Candidates);

        // **THE MANIFEST CHECKS WHAT THE SEARCH NAMED; IT NEVER FED IT** (decision O).
        Assert.Equal(fixture.Variant, found.Variant.Name);
        Assert.InRange(found.CenterHz, fixture.CenterHz - 5, fixture.CenterHz + 5);

        var (decoding, decodeCpu) = Timed(() => new OliviaDemodulator(Format, found.Variant, found.CenterHz, audio.SampleRate).Decode(audio, 0));
        var cer = OliviaFixtures.CharacterErrorRate(decoding.Text, fixture.Text);

        _output.WriteLine(
            $"decode after the search: {found.Variant.Name} at {found.CenterHz:0.00} Hz, CER {cer:0.0000} (ceiling 0.05), "
            + $"characters {decoding.CharactersOut} of {fixture.Text.Length}, blocks decoded {decoding.BlocksDecoded}, rejected {decoding.BlocksRejected}, "
            + $"sync snr {decoding.SyncSnr:0.00}, first block {decoding.FirstBlockSeconds:0.000} s; decode cpu {decodeCpu:0.000} s (2.5's ceiling {CpuCeilingSeconds} s)");

        if (cer > 0)
        {
            _output.WriteLine("decoded : " + JsonSerializer.Serialize(decoding.Text));
            _output.WriteLine("manifest: " + JsonSerializer.Serialize(fixture.Text));
        }

        Assert.True(cer <= 0.05, $"CER {cer:0.0000} over 0.05");
        Assert.True(decodeCpu < CpuCeilingSeconds, $"decode cpu {decodeCpu:0.000} s");
    }

    /// <summary>**Decision R: thirty seconds of pure noise, searched over the same passband, yields no candidate.**</summary>
    [Fact]
    public void NoiseYieldsNoCandidate()
    {
        var fixture = OliviaFixtures.Load("olivia-noise-only-30s.wav");
        var audio = WavAudio.Read(fixture.Path);

        var (search, cpu) = Timed(() => new OliviaBlindSearch(Format).Search(audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz));

        Print("olivia-noise-only-30s.wav", search, cpu);

        Assert.Empty(search.Candidates);
        Assert.Equal(audio.Samples.Length / (double)audio.SampleRate, search.AudioSeconds, 3);
    }

    /// <summary>
    /// **§R13: the search writes its own event - what it measured, the rows it weighed and the one
    /// it chose, mode olivia and the variant - in the PSK31 receive category, with no text and no
    /// callsign in it.**
    /// </summary>
    [Fact]
    public void TheSearchWritesItsEventWithNoTextOrCallsign()
    {
        var fixture = OliviaFixtures.Load("olivia-8-250-qso-norsid.wav");
        var audio = WavAudio.Read(fixture.Path);
        var telemetry = new RecordingTelemetry();

        var search = new OliviaBlindSearch(Format, telemetry).Search(audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);
        var found = Assert.Single(search.Candidates);
        var e = Assert.Single(telemetry.Events);
        var json = JsonSerializer.Serialize(e.Data);

        _output.WriteLine($"{e.Category} {e.Name} {json}");

        Assert.Equal(TelemetryCategory.Psk31, e.Category);
        Assert.Equal("olivia_search", e.Name);
        Assert.Equal("olivia", e.Data["mode"]);
        Assert.Equal(found.Variant.Name, e.Data["variant"]);

        foreach (var key in new[] { "centerHz", "measuredCenterHz", "toneSpacingHz", "tonesCounted", "occupiedLowHz", "occupiedHighHz", "weighed", "weighedSnr", "audioSeconds" })
        {
            Assert.True(e.Data.ContainsKey(key), $"the event has no {key}");
        }

        Assert.Contains(found.Variant.Name, (IEnumerable<string>)e.Data["weighed"]!);

        // **NO TEXT AND NO CALLSIGN** (HM-DEC-018): not the stations, not any word of the message.
        foreach (var word in fixture.Text.Split(' ', '\n').Where(w => w.Length >= 3 && w.Any(char.IsLetter)))
        {
            Assert.DoesNotContain(word, json, StringComparison.Ordinal);
        }

        Assert.All(e.Data.Values, v => Assert.True(v is not string s || s.Length < 12));
    }

    private static OliviaFormat Format
        => OliviaData.Current.Format ?? throw new InvalidOperationException(OliviaData.Current.Problem);

    private static (T Value, double CpuSeconds) Timed<T>(Func<T> work)
    {
        var before = Process.GetCurrentProcess().TotalProcessorTime;
        var value = work();

        return (value, (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds);
    }

    private void Print(string file, OliviaSearch search, double cpu)
    {
        _output.WriteLine(
            $"{file}: candidates {search.Candidates.Count}; audio consumed {search.AudioSeconds:0.000} s; search cpu {cpu:0.000} s; "
            + $"passband floor {search.FloorDb:0.0} dB, loudest bin {search.PeakOverFloorDb:0.00} dB over it");

        foreach (var c in search.Candidates)
        {
            _output.WriteLine(
                $"   named {c.Variant.Name} at {c.CenterHz:0.00} Hz (measured middle of the tones {c.MeasuredCenterHz:0.00} Hz), confidence {c.Confidence:0.00}, "
                + $"after {c.AudioSeconds:0.000} s of audio; spacing {c.ToneSpacingHz:0.000} Hz over {c.TonesCounted} tones; "
                + $"occupied {c.OccupiedLowHz:0.00}-{c.OccupiedHighHz:0.00} Hz = {c.OccupiedHighHz - c.OccupiedLowHz:0.00} Hz");

            foreach (var t in c.Trials)
            {
                _output.WriteLine(
                    $"      weighed {t.Variant,-8} sync snr {t.SyncSnr:0.00}, blocks decoded {t.BlocksDecoded}, rejected {t.BlocksRejected}, "
                    + $"highest block {t.HighestBlockSnr:0.00}");
            }
        }
    }

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

/// <summary>
/// **Tests whose CPU ceiling is measured as process CPU run with nothing else in the process.**
/// </summary>
/// <remarks>
/// Process CPU counts every thread, and xunit runs other test classes beside this one: in the
/// engine carry-forward run the decode after the blind search read 23.5 s of process CPU, where
/// the same decode alone read 5.3. A ceiling read off a shared clock measures the neighbors.
/// </remarks>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class CpuMeasuredAlone
{
    /// <summary>The collection's name.</summary>
    public const string Name = "CPU measured alone";
}
