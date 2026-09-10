using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 303 task 1: **the clock query records what it did.**
/// </summary>
/// <remarks>
/// <para>**THE HOLE THIS CLOSES COST TWENTY MINUTES ON THE WRONG MACHINE.** Hamlet
/// said all day that it had not been able to check the clock against a time server,
/// and today's telemetry held **2,600 events across eleven app starts and not one
/// naming a time server, an offset, an attempt or a failure.** So *asked and failed*
/// and *never asked* were indistinguishable, and they are different problems with
/// different fixes.</para>
/// <para>**THE ATTEMPT IS WRITTEN BEFORE THE RESULT**, which is the half that makes
/// an absence readable: after this, no record at all means nothing was tried.</para>
/// <para>**AND SIX WAYS TO FAIL USED TO RETURN ONE VALUE.** `SntpClock` collapsed a
/// name that would not resolve, a packet that never came back, a reply too short and
/// a timestamp that would not parse into `ClockOffset.Unknown`, with nothing to tell
/// them apart. Each now carries a stable token (§8.1).</para>
/// </remarks>
public sealed class Unit303ClockRecordTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the records are quoted.</param>
    public Unit303ClockRecordTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A measurement, a timeout and a throw write three records.**</summary>
    [Fact]
    public void ThreeOutcomesWriteThreeDifferentRecords()
    {
        var sink = new Recording();

        AppEvents.ClockQueryStarted(sink, SntpClock.DefaultServer);

        AppEvents.ClockQueryFinished(
            sink,
            ClockAnswer.Measured(
                SntpClock.DefaultServer,
                new ClockOffset(1.66, new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc)),
                TimeSpan.FromMilliseconds(38)));

        AppEvents.ClockQueryFinished(
            sink,
            ClockAnswer.Failed(
                SntpClock.DefaultServer, "timeout",
                "nothing came back within 3000 ms"));

        AppEvents.ClockQueryFinished(
            sink,
            ClockAnswer.Failed(
                SntpClock.DefaultServer, "threw_InvalidOperationException",
                "the socket was already in use"));

        foreach (var line in sink.Written)
        {
            _output.WriteLine(line);
        }

        Assert.Equal(4, sink.Written.Count);

        // **THE ATTEMPT COMES FIRST**, so an absence can be read.
        Assert.Contains("clock_query_started", sink.Written[0], StringComparison.Ordinal);

        // **THREE DIFFERENT RECORDS, NOT ONE.**
        var reasons = sink.Events
            .Where(e => e.Name == "clock_query_finished")
            .Select(e => e.Data["reason"]?.ToString())
            .ToList();

        Assert.Equal(
            new[] { "measured", "timeout", "threw_InvalidOperationException" },
            reasons);

        // The measurement carries its offset; the failures carry none.
        var measured = sink.Events.First(
            e => e.Name == "clock_query_finished"
                 && (string?)e.Data["reason"] == "measured");

        Assert.Equal(1.66, Convert.ToDouble(
            measured.Data["offsetSeconds"], CultureInfo.InvariantCulture), 3);

        Assert.All(
            sink.Events.Where(
                e => e.Name == "clock_query_finished"
                     && (string?)e.Data["reason"] != "measured"),
            e => Assert.False(e.Data.ContainsKey("offsetSeconds")));
    }

    /// <summary>**A failure is a warning, so it can be found by scanning.**</summary>
    /// <remarks>
    /// **LEVELS MEAN SOMETHING** (§8.1). Slot timing depends on this reading, so a
    /// query nobody could complete is not `info`.
    /// </remarks>
    [Fact]
    public void AFailureIsAWarningAndAMeasurementIsNot()
    {
        var sink = new Recording();

        AppEvents.ClockQueryFinished(
            sink,
            ClockAnswer.Measured(
                "pool.ntp.org", new ClockOffset(0.2, DateTime.UtcNow),
                TimeSpan.FromMilliseconds(20)));

        AppEvents.ClockQueryFinished(
            sink, ClockAnswer.Failed("pool.ntp.org", "socket_HostNotFound", "no such host"));

        foreach (var e in sink.Events)
        {
            _output.WriteLine(e.Level + "  " + e.Data["reason"]);
        }

        Assert.Equal(TelemetryLevel.Info, sink.Events[0].Level);
        Assert.Equal(TelemetryLevel.Warn, sink.Events[1].Level);
    }

    /// <summary>**It is in a category, so About's count still holds.**</summary>
    /// <remarks>
    /// **`Diagnostics` RATHER THAN AN EIGHTH CATEGORY.** That category is documented
    /// as the application's own record - app start and stop, version, unhandled
    /// errors - and a clock query Hamlet makes about itself belongs there. Unit 285's
    /// About window reports *7 of 7 categories on* and still does.
    /// </remarks>
    [Fact]
    public void TheClockEventsAreInACategory()
    {
        var sink = new Recording();

        AppEvents.ClockQueryStarted(sink, "pool.ntp.org");
        AppEvents.ClockQueryFinished(
            sink, ClockAnswer.Failed("pool.ntp.org", "timeout", "nothing came back"));

        _output.WriteLine("categories used: " + string.Join(
            ", ", sink.Events.Select(e => e.Category).Distinct()));

        Assert.All(
            sink.Events,
            e => Assert.Equal(TelemetryCategory.Diagnostics, e.Category));
    }

    /// <summary>**Nothing personal is written.**</summary>
    /// <remarks>
    /// **A HOSTNAME IS NOT PERSONAL AND AN OFFSET IS NOT PERSONAL** (§2.1), and the
    /// instruction says so outright. What must never appear is a callsign, a name, a
    /// location or a grid - so this sweeps every written value for the operator's own
    /// details and fails on any of them.
    /// </remarks>
    [Fact]
    public void NothingPersonalIsWritten()
    {
        var sink = new Recording();

        AppEvents.ClockQueryStarted(sink, "pool.ntp.org");
        AppEvents.ClockQueryFinished(
            sink,
            ClockAnswer.Measured(
                "pool.ntp.org", new ClockOffset(1.66, DateTime.UtcNow),
                TimeSpan.FromMilliseconds(38)));

        var everything = string.Join(" ", sink.Written);

        _output.WriteLine(everything);

        foreach (var personal in new[]
        {
            "KC3QIS", "Dixon", "Timothy", "Trafford", "FN00", "6118",
        })
        {
            Assert.DoesNotContain(
                personal, everything, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>A sink that remembers, so a test can read what was written.</summary>
    private sealed class Recording : ITelemetry
    {
        /// <summary>Every event, as it was handed over.</summary>
        public List<(TelemetryCategory Category, string Name,
            IReadOnlyDictionary<string, object?> Data, TelemetryLevel Level)> Events
        { get; } = new();

        /// <summary>The same, rendered the way the file renders them.</summary>
        public List<string> Written { get; } = new();

        /// <inheritdoc/>
        public long DroppedEventCount => 0;

        /// <inheritdoc/>
        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
        {
            var fields = data ?? new Dictionary<string, object?>();

            Events.Add((category, eventName, fields, level));

            Written.Add(
                category + "/" + eventName + "  "
                + string.Join(
                    "  ",
                    fields.Select(kv => kv.Key + "=" + kv.Value)));
        }
    }
}
