using System.Reflection;
using System.Text.Json;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Every FT8 slot the tab decodes leaves a line saying how far its candidates got,
/// and a slot that refused leaves one too (unit 233).
/// </summary>
/// <remarks>
/// <para>**THIS IS THE ARTEFACT THAT SURVIVES AN UNATTENDED SESSION.** On
/// 2026-09-03 the owner sat at 14.074, pressed the thing this phase was built for,
/// and got an empty table. The machine kept no record of it at all — the capture
/// folder had never been created, and the only thing telemetry had ever said about
/// a decode was a count of CW characters. Since unit 225 the tab decodes every slot
/// with no press, so a morning at the radio produces four of these a minute.</para>
/// <para>**IT COUNTS AND IT DOES NOT INTERPRET** (`CLAUDE.md` §12.1). The line says
/// how many places were looked at and how many became words. It does not say what
/// anybody said and it does not conclude anything about the band.</para>
/// </remarks>
public sealed class EverySlotLeavesALineTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the written line is printed.</param>
    /// <remarks>
    /// **THE LINE IS PRINTED AS IT WILL ACTUALLY APPEAR** (unit 236). A payload
    /// asserted key by key and never seen whole is a payload nobody has read, and
    /// the operator is going to be told to open this file at the radio.
    /// </remarks>
    public EverySlotLeavesALineTests(ITestOutputHelper output)
        => _output = output;

    private static readonly DateTime SlotStart =
        new(2026, 9, 3, 14, 22, 30, DateTimeKind.Utc);

    private static readonly DateTime Now =
        new(2026, 9, 3, 14, 22, 45, DateTimeKind.Utc);

    private static ClockOffset Measured =>
        new(0.25, new DateTime(2026, 9, 3, 14, 20, 45, DateTimeKind.Utc));

    /// <summary>
    /// **THE ONE THE UNIT EXISTS FOR.** A slot that found signals and read none of
    /// them writes the four numbers that name the stage that refused.
    /// </summary>
    [Fact]
    public void ASlotThatDecodedNothingStillWritesItsCensus()
    {
        var sink = new CapturingTelemetry();

        AppEvents.Ft8SlotsRead(
            sink,
            new[] { new Ft8SlotCensus(SlotStart, 14, 0, 0, 0, 0, new[] { 31, 24, 19 }, 48000) },
            string.Empty,
            Measured,
            Now);

        var written = Assert.Single(sink.Events);

        Assert.Equal(TelemetryCategory.Decode, written.Category);
        Assert.Equal("ft8_slot", written.Name);

        // Found and not read is the case worth finding by scanning a night's file.
        Assert.Equal(TelemetryLevel.Warn, written.Level);

        Assert.Equal("decoded", written.Data["outcome"]);
        Assert.Equal("2026-09-03T14:22:30Z", written.Data["slotStartUtc"]);
        Assert.Equal(14, written.Data["candidates"]);
        Assert.Equal(0, written.Data["paritySatisfied"]);
        Assert.Equal(0, written.Data["checksumPassed"]);
        Assert.Equal(0, written.Data["becameText"]);
        Assert.Equal(0, written.Data["duplicates"]);
        Assert.Equal(48000, written.Data["sampleRate"]);
        Assert.Equal(0.25, written.Data["clockOffsetSeconds"]);
        Assert.Equal(120.0, written.Data["clockOffsetAgeSeconds"]);

        var scores = Assert.IsAssignableFrom<IReadOnlyList<int>>(
            written.Data["topCostasMatchCounts"]);

        Assert.Equal(new[] { 31, 24, 19 }, scores);
    }

    /// <summary>
    /// **A SLOT THAT REFUSED IS AN EVENT, NOT A SILENCE**, and the sentence the
    /// operator read is the sentence the file keeps.
    /// </summary>
    [Fact]
    public void ARefusalIsWrittenVerbatimRatherThanLeavingAGap()
    {
        var sink = new CapturingTelemetry();

        AppEvents.Ft8SlotsRead(
            sink,
            Array.Empty<Ft8SlotCensus>(),
            Ft8SlotCutter.NoOffset,
            ClockOffset.Unknown,
            Now);

        var written = Assert.Single(sink.Events);

        Assert.Equal("ft8_slot", written.Name);
        Assert.Equal(TelemetryLevel.Warn, written.Level);
        Assert.Equal("refused", written.Data["outcome"]);
        Assert.Equal(Ft8SlotCutter.NoOffset, written.Data["refusal"]);

        // Unknown is a real state and is never written as zero (HM-DEC-009).
        Assert.Null(written.Data["clockOffsetSeconds"]);
        Assert.Null(written.Data["clockOffsetAgeSeconds"]);
    }

    /// <summary>One line per slot, not one per look and not one per message.</summary>
    [Fact]
    public void EverySlotInAReadingGetsItsOwnLine()
    {
        var sink = new CapturingTelemetry();

        AppEvents.Ft8SlotsRead(
            sink,
            new[]
            {
                new Ft8SlotCensus(SlotStart, 9, 3, 2, 2, 1, new[] { 40 }, 12000),
                new Ft8SlotCensus(
                    SlotStart.AddSeconds(15), 0, 0, 0, 0, 0, Array.Empty<int>(), 12000),
            },
            string.Empty,
            Measured,
            Now);

        Assert.Equal(2, sink.Events.Count);

        // A slot that read what it found is ordinary, and so is a slot with nobody
        // on it — hearing nothing is the ordinary state of a receiver.
        Assert.All(sink.Events, e => Assert.Equal(TelemetryLevel.Info, e.Level));
        Assert.Equal(2, sink.Events[0].Data["becameText"]);
        Assert.Equal(0, sink.Events[1].Data["candidates"]);
    }

    /// <summary>
    /// **THE SHAPE REFUSES RATHER THAN THE CALL SITE REMEMBERING** (HM-DEC-018).
    /// </summary>
    /// <remarks>
    /// An FT8 message is very often a pair of callsigns, and
    /// <see cref="Ft8Reception"/> carries the decoded rows. If a later unit widens a
    /// telemetry method to take one, this fails — which is the point.
    /// <see cref="Ft8SlotCensus"/> has no member that can hold a character.
    /// </remarks>
    [Fact]
    public void NoTelemetryEventCanBeHandedAnythingThatHoldsADecodedMessage()
    {
        var forbidden = new[] { typeof(Ft8Reception), typeof(Ft8Decode) };

        var methods = typeof(AppEvents)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.NotEmpty(methods);

        foreach (var method in methods)
        {
            foreach (var parameter in method.GetParameters())
            {
                Assert.DoesNotContain(parameter.ParameterType, forbidden);
            }
        }
    }

    /// <summary>
    /// **AND NO COLUMN CALLED SNR** (`CLAUDE.md` §0.0). There is no
    /// signal-to-noise ratio anywhere in this path, and a plausible number under
    /// that heading would be read as a measurement by every reader downstream.
    /// </summary>
    [Fact]
    public void TheCostasMatchCountIsNeverWrittenAsASignalToNoiseRatio()
    {
        var sink = new CapturingTelemetry();

        AppEvents.Ft8SlotsRead(
            sink,
            new[] { new Ft8SlotCensus(SlotStart, 3, 1, 1, 1, 0, new[] { 33 }, 44100) },
            string.Empty,
            Measured,
            Now);

        var keys = Assert.Single(sink.Events).Data.Keys;

        Assert.DoesNotContain("snr", keys, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("snrDb", keys, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("signalToNoise", keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("topCostasMatchCounts", keys);
    }

    /// <summary>The payload survives being written as JSON, keys and all.</summary>
    [Fact]
    public void ThePayloadSerialisesTheWayTheFileWillHoldIt()
    {
        var sink = new CapturingTelemetry();

        AppEvents.Ft8SlotsRead(
            sink,
            new[] { new Ft8SlotCensus(SlotStart, 5, 2, 1, 1, 0, new[] { 28, 21 }, 8000) },
            string.Empty,
            Measured,
            Now);

        var json = JsonSerializer.Serialize(Assert.Single(sink.Events).Data);

        Assert.Contains("\"candidates\":5", json, StringComparison.Ordinal);
        Assert.Contains("\"topCostasMatchCounts\":[28,21]", json, StringComparison.Ordinal);
        Assert.Contains("\"sampleRate\":8000", json, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE FORK THE BENCH CHECK OF 2026-09-03 DIED ON** (unit 236). Everything
    /// else on this line describes the decode, so a muted sound card and a quiet
    /// band wrote the same line. The level says which.
    /// </summary>
    [Fact]
    public void HowLoudTheAudioWasReachesTheLine()
    {
        var sink = new CapturingTelemetry();

        AppEvents.Ft8SlotsRead(
            sink,
            new[]
            {
                new Ft8SlotCensus(SlotStart, 140, 44, 41, 40, 1, new[] { 51 }, 12000)
                {
                    Level = new Ft8SlotLevel(-2.0541, -14.1684, 180_000, 13),
                },
            },
            string.Empty,
            Measured,
            Now);

        var data = Assert.Single(sink.Events).Data;

        _output.WriteLine("  decoded: " + JsonSerializer.Serialize(data));

        Assert.Equal(-2.05, data["audioPeakDbFullScale"]);
        Assert.Equal(-14.17, data["audioRmsDbFullScale"]);
        Assert.Equal(180_000, data["audioSamples"]);
        Assert.Equal(13, data["audioZeroSamples"]);
        Assert.Equal(0.000072, data["audioZeroSampleFraction"]);
    }

    /// <summary>
    /// **DIGITAL SILENCE WRITES NOTHING WHERE A LEVEL WOULD GO, AND SAYS WHY**
    /// (HM-DEC-009). The logarithm of nought is not a number. A floor written in
    /// its place is a measurement somebody will average months later, and the
    /// zero-sample count standing at the whole slot is the honest answer instead.
    /// </summary>
    [Fact]
    public void AnAllZeroSlotWritesNoLevelRatherThanAPlausibleNumber()
    {
        var sink = new CapturingTelemetry();

        AppEvents.Ft8SlotsRead(
            sink,
            new[]
            {
                new Ft8SlotCensus(SlotStart, 0, 0, 0, 0, 0, Array.Empty<int>(), 48000)
                {
                    Level = new Ft8SlotLevel(null, null, 720_000, 720_000),
                },
            },
            string.Empty,
            Measured,
            Now);

        var data = Assert.Single(sink.Events).Data;

        _output.WriteLine("  digital silence: " + JsonSerializer.Serialize(data));

        Assert.Null(data["audioPeakDbFullScale"]);
        Assert.Null(data["audioRmsDbFullScale"]);
        Assert.Equal(720_000, data["audioSamples"]);
        Assert.Equal(720_000, data["audioZeroSamples"]);
        Assert.Equal(1.0, data["audioZeroSampleFraction"]);

        // The fields are PRESENT and null, rather than absent. A missing key and a
        // measurement that refused are different facts to whoever reads the file.
        Assert.Contains("audioPeakDbFullScale", data.Keys);
        Assert.Contains("audioRmsDbFullScale", data.Keys);
    }

    /// <summary>
    /// **AND NONE OF THE LEVEL KEYS READS AS A SIGNAL-TO-NOISE RATIO** (`CLAUDE.md`
    /// §0.0). A level says how loud the audio was. It says nothing about how strong
    /// a signal in it was, and it is not comparable with this mode's published
    /// sensitivity figure.
    /// </summary>
    [Fact]
    public void NoLevelKeyReadsAsASignalToNoiseRatio()
    {
        var sink = new CapturingTelemetry();

        AppEvents.Ft8SlotsRead(
            sink,
            new[]
            {
                new Ft8SlotCensus(SlotStart, 3, 1, 1, 1, 0, new[] { 33 }, 44100)
                {
                    Level = new Ft8SlotLevel(-6.02, -9.03, 661_500, 0),
                },
            },
            string.Empty,
            Measured,
            Now);

        var keys = Assert.Single(sink.Events).Data.Keys;

        foreach (var forbidden in new[] { "snr", "signal", "strength", "db" })
        {
            Assert.DoesNotContain(forbidden, keys, StringComparer.OrdinalIgnoreCase);
        }

        // What is there instead says what it is: decibels relative to full scale.
        Assert.Contains("audioPeakDbFullScale", keys);
        Assert.Contains("audioRmsDbFullScale", keys);
    }

    /// <summary>
    /// **THE REFUSAL BRANCH IS UNTOUCHED AND WRITES NO SLOT LINE** (unit 236). A
    /// reading that could not cut anything has no audio to describe, and a level
    /// beside a refusal would be a level of nothing.
    /// </summary>
    [Fact]
    public void ARefusalStillCarriesNoLevelAtAll()
    {
        var sink = new CapturingTelemetry();

        AppEvents.Ft8SlotsRead(
            sink,
            Array.Empty<Ft8SlotCensus>(),
            Ft8SlotCutter.NoOffset,
            ClockOffset.Unknown,
            Now);

        var keys = Assert.Single(sink.Events).Data.Keys;

        Assert.DoesNotContain("audioPeakDbFullScale", keys);
        Assert.DoesNotContain("audioRmsDbFullScale", keys);
        Assert.DoesNotContain("audioSamples", keys);
        Assert.DoesNotContain("audioZeroSamples", keys);
    }

    private sealed record Written(
        TelemetryCategory Category,
        string Name,
        IReadOnlyDictionary<string, object?> Data,
        TelemetryLevel Level);

    private sealed class CapturingTelemetry : ITelemetry
    {
        private readonly List<Written> _events = new();

        public IReadOnlyList<Written> Events => _events;

        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => _events.Add(new Written(
                category,
                eventName,
                data ?? new Dictionary<string, object?>(StringComparer.Ordinal),
                level));
    }
}
