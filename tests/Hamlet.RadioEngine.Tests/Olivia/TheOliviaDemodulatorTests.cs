using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 361 tasks 3 and 4: **Hamlet's own Olivia demodulator reads the mode author's
/// audio**, clean and below the noise, and reads nothing in noise.
/// </summary>
/// <remarks>
/// <para>**THE VARIANT AND THE CENTER COME FROM THE RSID DETECTOR, NEVER FROM THE TEST** (decision
/// I). The manifest's variant and center only check what the detector said; decoding begins at the
/// end of the burst's tones.</para>
/// <para>**EVERY FILE IS HASHED FIRST, THE CEILINGS ARE NEVER LOOSENED, AND THE CPU IS THE
/// DEMODULATOR'S OWN** (2.5, §6), measured as process CPU around the decode alone; the detector's
/// CPU is printed beside it.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
public sealed class TheOliviaDemodulatorTests
{
    private const double CpuCeilingSeconds = 20.0;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each fixture's reading is printed.</param>
    public TheOliviaDemodulatorTests(ITestOutputHelper output) => _output = output;

    /// <summary>**2.1 and 2.5: each clean fixture decodes to its text at CER 0.01 or under, in under 20 s of CPU.**</summary>
    /// <param name="file">The fixture.</param>
    [Theory]
    [InlineData("olivia-8-250-cq-rsid.wav")]
    [InlineData("olivia-16-500-qso-rsid.wav")]
    [InlineData("olivia-32-1000-qso-rsid.wav")]
    public void EachCleanFixtureDecodesToItsText(string file) => DecodesWithin(file, 0.01);

    /// <summary>**2.2: the -10 dB fixture decodes at CER 0.05 or under.**</summary>
    [Fact]
    public void TheMinusTenDecibelFixtureDecodes() => DecodesWithin("olivia-16-500-qso-snr-10db.wav", 0.05);

    /// <summary>
    /// **2.2 as corrected on 2026-09-19, and 2.5: the -16 dB fixture is read inside the CPU
    /// ceiling, its CER is measured and printed with no ceiling, and every character shown came
    /// from a block that cleared the threshold.**
    /// </summary>
    /// <remarks>
    /// <para>**THE 0.10 CEILING WAS WITHDRAWN BY THE OWNER'S PLAN, NOT LOOSENED HERE**
    /// (`PHASE_PLAN.md` §8, the revision of 2026-09-19; work instruction 362 decision U, PSK31 plan
    /// §R12): it sat below the mode's published sensitivity for 16/500.</para>
    /// <para>**IT STILL FAILS** if the burst is not heard or names the wrong variant or center, if
    /// no block is read at all, if the read runs past twenty seconds of CPU, or if a character is
    /// shown that no accepted block could have carried (§3.3, §R9): a block gives at most its
    /// bits-per-symbol characters, so more characters than accepted blocks times that is a
    /// character from nowhere.</para>
    /// </remarks>
    [Fact]
    public void TheMinusSixteenDecibelFixtureDecodes()
    {
        var (decoding, variant) = DecodesWithin("olivia-16-500-qso-snr-16db.wav", null);

        Assert.True(decoding.BlocksDecoded > 0, "the -16 dB file: no block was read at all");
        Assert.Equal(decoding.BlocksDecoded, decoding.BlockSnrs.Count(s => s >= OliviaDemodulator.SyncThreshold));
        Assert.Equal(decoding.Text.Length, decoding.CharactersOut);
        Assert.InRange(decoding.CharactersOut, 1, decoding.BlocksDecoded * variant.BitsPerSymbol);
    }

    /// <summary>**2.4: pure noise, read at each of the three variants at 1000 Hz, gives no characters.**</summary>
    /// <param name="variant">The variant the noise is read as.</param>
    [Theory]
    [InlineData("8/250")]
    [InlineData("16/500")]
    [InlineData("32/1000")]
    public void NoiseGivesNoCharacters(string variant)
    {
        var fixture = OliviaFixtures.Load("olivia-noise-only-30s.wav");
        var audio = WavAudio.Read(fixture.Path);
        var demodulator = new OliviaDemodulator(Format, Format.Variant(variant)!, 1000, audio.SampleRate);

        var (decoding, cpu) = Timed(() => demodulator.Decode(audio, 0));

        _output.WriteLine(
            $"noise as {variant}: characters {decoding.CharactersOut}, blocks decoded {decoding.BlocksDecoded}, "
            + $"rejected {decoding.BlocksRejected}, highest block snr {decoding.BlockSnrs.DefaultIfEmpty(0).Max():0.00} "
            + $"(threshold {OliviaDemodulator.SyncThreshold}), cpu {cpu:0.000} s");

        Assert.Equal(0, decoding.CharactersOut);
        Assert.Equal("", decoding.Text);
    }

    /// <summary>
    /// **§R13 and decision K: the sync and summary events fire on the clean 16/500 file, and none
    /// carries text or a callsign.**
    /// </summary>
    [Fact]
    public void TheEventsFireAndCarryNoTextOrCallsign()
    {
        var fixture = OliviaFixtures.Load("olivia-16-500-qso-rsid.wav");
        var audio = WavAudio.Read(fixture.Path);
        var heard = Assert.Single(RsidDetector.Detect(Codes, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz));
        var telemetry = new RecordingTelemetry();
        var demodulator = new OliviaDemodulator(Format, Format.Variant(heard.Variant)!, heard.CenterHz, audio.SampleRate, telemetry);

        var decoding = demodulator.Decode(audio, BurstEnd(heard));

        Assert.NotEmpty(decoding.Text);

        var sync = Assert.Single(telemetry.Events, e => e.Name == "olivia_sync" && (string?)e.Data["state"] == "found");
        var run = Assert.Single(telemetry.Events, e => e.Name == "olivia_run");

        foreach (var e in telemetry.Events)
        {
            var json = JsonSerializer.Serialize(e.Data);

            _output.WriteLine($"{e.Category} {e.Name} {json}");

            Assert.Equal(TelemetryCategory.Psk31, e.Category);
            Assert.Equal("olivia", e.Data["mode"]);
            Assert.Equal("16/500", e.Data["variant"]);

            // **NO TEXT AND NO CALLSIGN** (HM-DEC-018): not the stations, not any word of the
            // message, and no field that could hold either.
            foreach (var word in fixture.Text.Split(' ', '\n').Where(w => w.Length >= 3 && w.Any(char.IsLetter)))
            {
                Assert.DoesNotContain(word, json, StringComparison.Ordinal);
            }

            Assert.All(e.Data.Values, v => Assert.True(v is not string s || s.Length < 12));
        }

        Assert.Equal(decoding.BlocksDecoded, run.Data["blocksDecoded"]);
        Assert.Equal(decoding.CharactersOut, run.Data["charactersOut"]);
        Assert.NotNull(sync.Data["frequencyOffsetHz"]);
    }

    /// <summary>
    /// **2.6 and decision N: the timing table is measured by the demodulator**, and it is what
    /// OliviaData hands out.
    /// </summary>
    /// <remarks>
    /// <para>**THE FIGURE IS THE SLOPE, NOT THE LENGTH.** Seconds per character is the time from the
    /// first decoded block to the last over the characters the blocks before the last one carried,
    /// so the idle padding of a short final block is not charged to every character. The manifest
    /// arithmetic beside it - the file's length less the burst, over its text - is the old
    /// estimate's method and is printed, not asserted.</para>
    /// <para>**THE NO-RSID 8/250 FILE IS PRINTED AND NOT USED** (decision N): it has no burst, so its
    /// variant and center are the manifest's, which is the blind search's case and not this one.</para>
    /// </remarks>
    [Fact]
    public void TheTimingTableIsMeasuredByTheDemodulator()
    {
        var timing = OliviaData.Current.Timing;

        Assert.NotNull(timing);
        _output.WriteLine($"timing.json: source {timing!.Source}; method {timing.Method}");

        foreach (var file in new[] { "olivia-8-250-cq-rsid.wav", "olivia-16-500-qso-rsid.wav", "olivia-32-1000-qso-rsid.wav" })
        {
            var fixture = OliviaFixtures.Load(file);
            var audio = WavAudio.Read(fixture.Path);
            var d = Assert.Single(RsidDetector.Detect(Codes, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz));
            var decoding = new OliviaDemodulator(Format, Format.Variant(d.Variant)!, d.CenterHz, audio.SampleRate).Decode(audio, BurstEnd(d));
            var measured = SecondsPerCharacter(decoding);
            var arithmetic = ((audio.Samples.Length / (double)audio.SampleRate) - 2.32) / fixture.Text.Length;
            var table = timing.SecondsPerCharacter.TryGetValue(d.Variant, out var t) ? t : double.NaN;

            _output.WriteLine(
                $"{d.Variant,-8}: measured {measured:0.00000} s/character ({decoding.FirstBlockSeconds:0.000} to "
                + $"{decoding.LastBlockSeconds:0.000} s over {decoding.CharactersOut - decoding.LastBlockCharacters} characters); "
                + $"manifest arithmetic {arithmetic:0.00000}; table {table:0.00000}");

            Assert.Equal("measured", timing.Source);
            Assert.InRange(table, measured * 0.98, measured * 1.02);
        }

        var blind = OliviaFixtures.Load("olivia-8-250-qso-norsid.wav");
        var blindAudio = WavAudio.Read(blind.Path);
        var blindDecoding = new OliviaDemodulator(Format, Format.Variant(blind.Variant!)!, blind.CenterHz, blindAudio.SampleRate).Decode(blindAudio, 0);

        _output.WriteLine(
            $"8/250 no-RSID file, manifest variant and center, reported not used: {SecondsPerCharacter(blindDecoding):0.00000} s/character "
            + $"over {blindDecoding.CharactersOut} characters, CER {OliviaFixtures.CharacterErrorRate(blindDecoding.Text, blind.Text):0.0000}");
    }

    private static double SecondsPerCharacter(OliviaDecoding decoding)
        => (decoding.LastBlockSeconds - decoding.FirstBlockSeconds) / (decoding.CharactersOut - decoding.LastBlockCharacters);

    private static RsidCodes Codes
        => OliviaData.Current.Rsid ?? throw new InvalidOperationException(OliviaData.Current.Problem);

    private static OliviaFormat Format
        => OliviaData.Current.Format ?? throw new InvalidOperationException(OliviaData.Current.Problem);

    private static double BurstEnd(RsidDetection heard) => heard.StartSeconds + (Codes.Symbols / Codes.SymbolRateHz);

    private static (T Value, double CpuSeconds) Timed<T>(Func<T> work)
    {
        var before = Process.GetCurrentProcess().TotalProcessorTime;
        var value = work();

        return (value, (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds);
    }

    private (OliviaDecoding Decoding, OliviaVariant Variant) DecodesWithin(string file, double? ceiling)
    {
        var fixture = OliviaFixtures.Load(file);
        var audio = WavAudio.Read(fixture.Path);

        var (heard, detectorCpu) = Timed(
            () => RsidDetector.Detect(Codes, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz));
        var d = Assert.Single(heard);

        // **THE MANIFEST CHECKS THE DETECTION; IT DOES NOT FEED THE DEMODULATOR** (decision I).
        Assert.Equal(fixture.Variant, d.Variant);
        Assert.InRange(d.CenterHz, fixture.CenterHz - 5, fixture.CenterHz + 5);

        var variant = Format.Variant(d.Variant);

        Assert.NotNull(variant);

        var demodulator = new OliviaDemodulator(Format, variant!, d.CenterHz, audio.SampleRate);
        var (decoding, cpu) = Timed(() => demodulator.Decode(audio, BurstEnd(d)));
        var cer = OliviaFixtures.CharacterErrorRate(decoding.Text, fixture.Text);

        _output.WriteLine(
            $"{file}: detector {d.Name} variant {d.Variant} at {d.CenterHz:0.00} Hz (cpu {detectorCpu:0.000} s); "
            + $"{audio.SampleRate} Hz, {demodulator.SamplesPerSymbol} samples/symbol; "
            + $"CER {cer:0.0000} (ceiling {(ceiling is { } c ? $"{c:0.00}" : "none, measured and reported")}); characters {decoding.CharactersOut} of {fixture.Text.Length}; "
            + $"blocks decoded {decoding.BlocksDecoded}, rejected {decoding.BlocksRejected}; "
            + $"offset {decoding.FrequencyOffsetHz:0.00} Hz, symbol phase {decoding.SymbolPhase}, block phase {decoding.BlockPhase}, "
            + $"mean block snr {decoding.SyncSnr:0.00}; first block {decoding.FirstBlockSeconds:0.000} s, last {decoding.LastBlockSeconds:0.000} s; "
            + $"demodulator cpu {cpu:0.000} s");

        var sorted = decoding.BlockSnrs.OrderBy(x => x).ToArray();

        if (sorted.Length > 0)
        {
            _output.WriteLine(
                $"block snr: lowest {sorted[0]:0.00}, tenth {sorted[sorted.Length / 10]:0.00}, median {sorted[sorted.Length / 2]:0.00}, "
                + $"highest {sorted[^1]:0.00} (threshold {OliviaDemodulator.SyncThreshold})");
        }

        if (cer > 0)
        {
            _output.WriteLine("decoded : " + JsonSerializer.Serialize(decoding.Text));
            _output.WriteLine("manifest: " + JsonSerializer.Serialize(fixture.Text));
        }

        if (ceiling is { } limit)
        {
            Assert.True(cer <= limit, $"{file}: CER {cer:0.0000} over {limit:0.00}");
        }

        Assert.True(cpu < CpuCeilingSeconds, $"{file}: demodulator cpu {cpu:0.000} s");

        return (decoding, variant!);
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
