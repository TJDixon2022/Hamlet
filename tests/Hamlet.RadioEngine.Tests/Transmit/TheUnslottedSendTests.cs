using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// Work instruction 318 task 3b: **the one transmit path carries a send with no slot, as
/// `PHASE_PLAN.md` §R10 rules, and refuses one that would run on.**
/// </summary>
/// <remarks>
/// <para>**THE SAME PATH, NOT A SECOND ONE.** Every send here goes through
/// <see cref="Ft8ArmedSend.Arm"/> and <see cref="Ft8ArmedSend.NowAsync"/> into
/// <see cref="Ft8TransmitSequence.RunAsync"/> - the gate, the single keying write, the
/// <c>finally</c> that unkeys or aborts, and <see cref="Ft8ArmedSend.StopNow"/> - which is
/// the path FT8 and FT4 take, pinned unchanged by
/// <see cref="TheFt8AndFt4SendsAreByteIdenticalTests"/>.</para>
/// <para>**A FAKE PORT AND A FAKE SINK, AND NOTHING ELSE** (FACT-006). Nothing keys a radio
/// and no sound card is opened. The PSK31 audio is Hamlet's own, from
/// <see cref="Psk31Modulator"/>, at the rate a real endpoint declares.</para>
/// </remarks>
public sealed class TheUnslottedSendTests
{
    private const long Psk31Dial = 14_070_000;
    private const int Rate = 48_000;
    private const string Mine = "KC3QIS";
    private const string PttOnThenPttOff = "FE FE 94 E0 1C 00 01 FD FE FE 94 E0 1C 00 00 FD";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the wire, the runs and the records are printed.</param>
    public TheUnslottedSendTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Must-pass 1: under the cap, fired now, through the one path.**</summary>
    [Fact]
    public async Task ASendWithNoSlotUnderTheCapGoesOutAtOnceThroughTheOnePath()
    {
        var (armed, port, sink, telemetry) = Armed();
        var send = Cq();

        Assert.Null(armed.Arm(send));
        Assert.Same(send, armed.Armed);

        var result = await armed.NowAsync();
        var run = result.Run;

        _output.WriteLine($"audio     : {send.Unslotted!.Seconds:0.00} s, {send.Samples.Length} samples at {send.SampleRate} Hz");
        _output.WriteLine($"now       : {result.Outcome}, run {run?.Outcome}");
        _output.WriteLine($"wire      : {TheUnkeyHappensWhateverGoesWrongTests.Hex(port.Written)}");
        _output.WriteLine($"sink      : {sink.TimesCalled} call, {sink.SamplesHandedOver} samples at {sink.RateAskedFor} Hz");
        Print(telemetry);

        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.NotNull(run);
        Assert.Equal(Ft8TransmitOutcome.Played, run!.Outcome);
        Assert.Null(armed.Armed);

        // THE GATE IS ASKED FIRST.
        Assert.Equal(SendStage.EventName, telemetry.Events[0].Event);
        Assert.Equal(SendStage.GateAsked, telemetry.Events[0].Data["stage"]);

        // ONE KEYING FRAME, THEN ONE UNKEY, AND NOTHING ELSE ON THE WIRE.
        Assert.Equal(PttOnThenPttOff, TheUnkeyHappensWhateverGoesWrongTests.Hex(port.Written));
        Assert.Equal(UnkeyRoute.OrdinaryUnkey, run.CameOutOfTransmit);

        // EVERY SAMPLE, AT THE RATE IT WAS MADE AT.
        Assert.Equal(1, sink.TimesCalled);
        Assert.Equal(send.Samples.Length, sink.SamplesHandedOver);
        Assert.Equal(Rate, sink.RateAskedFor);

        // THE RECORD NAMES THE MODE AND CARRIES NO SLOT.
        var record = Assert.Single(telemetry.Events, e => e.Event == TransmitRecord.EventName);

        Assert.Equal(TelemetryLevel.Info, record.Level);
        Assert.Equal("Psk31", record.Data["mode"]);
        Assert.False(record.Data.ContainsKey("slotStartUtc"), "a send with no slot recorded a slot start");
        Assert.False(record.Data.ContainsKey("startSecondsIntoSlot"), "a send with no slot recorded an offset");
        Assert.False(record.Data.ContainsKey("messageType"), "a PSK31 send recorded an FT8 message type");
        Assert.Equal(send.Unslotted.MessageLength, record.Data["messageLength"]);
        NothingPersonal(record);
    }

    /// <summary>**Must-pass 2: over the cap, refused before it arms, and the refusal is a record.**</summary>
    [Fact]
    public async Task ASendWithNoSlotLongerThanTheCapIsRefusedBeforeItArmsWithARecord()
    {
        var (armed, port, sink, telemetry) = Armed();
        var send = TooLong();

        _output.WriteLine($"audio     : {send.Unslotted!.Seconds:0.00} s against a cap of {OperatorSend.LongestUnslottedSeconds:0} s");

        Assert.True(send.Unslotted.Seconds > OperatorSend.LongestUnslottedSeconds);

        var refusal = armed.Arm(send);

        _output.WriteLine($"arm       : {refusal?.Outcome} - {refusal?.Reason}");
        Print(telemetry);

        Assert.NotNull(refusal);
        Assert.Equal(Ft8TransmitOutcome.RefusedAsUnsendable, refusal!.Outcome);
        Assert.Contains("30 s", refusal.Reason, StringComparison.Ordinal);
        Assert.False(refusal.Keyed);
        Assert.Null(armed.Armed);
        Assert.Empty(port.Written);
        Assert.True(sink.WasNeverTouched);

        var record = Assert.Single(telemetry.Events);

        Assert.Equal(TransmitRecord.EventName, record.Event);
        Assert.Equal(TelemetryLevel.Warn, record.Level);
        Assert.Equal("RefusedAsUnsendable", record.Data["outcome"]);
        Assert.Equal("LongerThanTheCap", record.Data["fit"]);
        Assert.Equal(OperatorSend.LongestUnslottedSeconds, Assert.IsType<double>(record.Data["longestSeconds"]));
        Assert.Equal(send.Unslotted.Seconds, Assert.IsType<double>(record.Data["audioSeconds"]));
        NothingPersonal(record);

        // AND NOW HAS NOTHING TO FIRE.
        var now = await armed.NowAsync();

        Assert.Equal(Ft8ArmOutcome.NothingArmed, now.Outcome);
        Assert.Empty(port.Written);

        // THE BACKSTOP: HANDED STRAIGHT TO THE SEQUENCE, THE SAME SEND IS REFUSED THERE TOO.
        var backstopPort = new FakeSerialPort();
        var backstopSink = new FakeTransmitAudioSink();
        var backstopTelemetry = new RecordingTelemetry();

        var run = await new Ft8TransmitSequence(backstopPort, backstopSink, guard: null, telemetry: backstopTelemetry)
            .RunAsync(send);

        _output.WriteLine($"backstop  : {run.Outcome} - {run.Reason}");
        Print(backstopTelemetry);

        Assert.Equal(Ft8TransmitOutcome.RefusedAsUnsendable, run.Outcome);
        Assert.Contains("30 s", run.Reason, StringComparison.Ordinal);
        Assert.Empty(backstopPort.Written);
        Assert.True(backstopSink.WasNeverTouched);

        var backstop = Assert.Single(backstopTelemetry.Events, e => e.Event == TransmitRecord.EventName);

        Assert.Equal(TelemetryLevel.Warn, backstop.Level);
        Assert.Equal("LongerThanTheCap", backstop.Data["fit"]);
    }

    /// <summary>**Must-pass 3: the licence gate holds for a send with no slot.**</summary>
    /// <param name="what">Which refusal.</param>
    /// <param name="licenseClass">The operator's class.</param>
    /// <param name="guardEnabled">The Settings toggle.</param>
    [Theory]
    [InlineData("the guard is off, so the gate permits on the operator's word", LicenseClass.Technician, false)]
    [InlineData("the class is unknown", LicenseClass.Unknown, true)]
    [InlineData("outside a Technician's privileges", LicenseClass.Technician, true)]
    public async Task TheLicenceGateHoldsForASendWithNoSlot(string what, LicenseClass licenseClass, bool guardEnabled)
    {
        var (armed, port, sink, _) = Armed();
        var send = OperatorSend.Now(CqAudio(), Psk31Dial, licenseClass, guardEnabled);

        Assert.Null(armed.Arm(send));

        var result = await armed.NowAsync();

        _output.WriteLine($"{what}: {result.Outcome}, run {result.Run?.Outcome} - {result.Run?.Reason}");

        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.Equal(Ft8TransmitOutcome.RefusedByLicence, result.Run!.Outcome);
        Assert.False(result.Run.Keyed);
        Assert.Empty(port.Written);
        Assert.True(sink.WasNeverTouched);
    }

    /// <summary>**Must-pass 4: the operator's stop reaches a send with no slot while it plays.**</summary>
    [Fact]
    public async Task TheOperatorsStopReachesASendWithNoSlotWhileItPlays()
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink { PlaysOver = TimeSpan.FromSeconds(2) };
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink));

        Assert.Null(armed.Arm(Cq()));

        var running = armed.NowAsync();

        Assert.True(sink.Entered.Wait(TimeSpan.FromSeconds(5)), "the play never started");

        var stop = armed.StopNow(port);
        var result = await running;

        _output.WriteLine($"stop      : {stop.Outcome}, audio told {stop.AudioToldToStop}");
        _output.WriteLine($"run       : {result.Run?.Outcome}, came out by {result.Run?.CameOutOfTransmit}");
        _output.WriteLine($"played    : {sink.PlayedSoFar} of {sink.SamplesHandedOver}");
        _output.WriteLine($"wire      : {TheUnkeyHappensWhateverGoesWrongTests.Hex(port.Written)}");

        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.NotNull(stop.Abort);
        Assert.True(stop.AudioToldToStop);
        Assert.True(sink.StoppedByTheToken);
        Assert.Equal(Ft8TransmitOutcome.Cancelled, result.Run!.Outcome);
        Assert.True(result.Run.RadioIsInReceive);
    }

    /// <summary>**Must-pass 5: a boundary never runs a send with no slot, and now never runs a slotted one.**</summary>
    [Fact]
    public async Task ABoundaryNeverRunsASendWithNoSlotAndNowNeverRunsASlottedOne()
    {
        var (armed, port, sink, _) = Armed();
        var unslotted = Cq();

        Assert.Null(armed.Arm(unslotted));

        var boundary = await armed.AtBoundaryAsync(new DateTime(2026, 9, 11, 18, 0, 15, DateTimeKind.Utc));

        _output.WriteLine($"a boundary with no-slot armed : {boundary.Outcome}");

        Assert.Equal(Ft8ArmOutcome.HasNoSlot, boundary.Outcome);
        Assert.Null(boundary.Run);
        Assert.Same(unslotted, armed.Armed);
        Assert.Empty(port.Written);
        Assert.True(sink.WasNeverTouched);

        var (other, otherPort, otherSink, _) = Armed();
        var slotted = TheUnkeyHappensWhateverGoesWrongTests.Send();

        Assert.Null(other.Arm(slotted));

        var now = await other.NowAsync();

        _output.WriteLine($"now with a slotted send armed : {now.Outcome}");

        Assert.Equal(Ft8ArmOutcome.WaitsForItsSlot, now.Outcome);
        Assert.Null(now.Run);
        Assert.Same(slotted, other.Armed);
        Assert.Empty(otherPort.Written);
        Assert.True(otherSink.WasNeverTouched);
    }

    /// <summary>**A send with no slot has no slot to read, and says so rather than inventing one.**</summary>
    [Fact]
    public void ASendWithNoSlotHasNoSlotStartOffsetGridOrFt8Transmission()
    {
        var send = Cq();

        Assert.False(send.HasSlot);
        Assert.NotNull(send.Unslotted);
        Assert.Same(send.Unslotted!.Samples, send.Samples);
        Assert.Equal(Rate, send.SampleRate);

        var slotStart = Assert.Throws<InvalidOperationException>(() => send.SlotStartUtc);

        Assert.Throws<InvalidOperationException>(() => send.StartSecondsIntoSlot);
        Assert.Throws<InvalidOperationException>(() => send.Grid);
        Assert.Throws<InvalidOperationException>(() => send.Transmission);

        _output.WriteLine("asked for a slot start: " + slotStart.Message);
        _output.WriteLine("printed               : " + send.ToString()[..Math.Min(160, send.ToString().Length)]);

        Assert.Contains("HasSlot = False", send.ToString(), StringComparison.Ordinal);
        Assert.True(TheUnkeyHappensWhateverGoesWrongTests.Send().HasSlot);
    }

    /// <summary>**Must-pass 7: `PttOn` has exactly one use site under `src\`, counted and printed.**</summary>
    [Fact]
    public void PttOnHasExactlyOneUseSiteUnderSrc()
    {
        var source = Path.Combine(TheUnkeyHappensWhateverGoesWrongTests.RepositoryRoot(), "src");
        var sites = new List<string>();

        foreach (var file in Directory.EnumerateFiles(source, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            var lines = File.ReadAllLines(file);

            for (var i = 0; i < lines.Length; i++)
            {
                if (!lines[i].TrimStart().StartsWith("///", StringComparison.Ordinal)
                    && lines[i].Contains("CivConstants.PttOn", StringComparison.Ordinal))
                {
                    sites.Add(Path.GetRelativePath(source, file).Replace('\\', '/') + ":" + (i + 1) + "  " + lines[i].Trim());
                }
            }
        }

        foreach (var site in sites)
        {
            _output.WriteLine("PttOn used at : " + site);
        }

        _output.WriteLine("use sites     : " + sites.Count);

        var only = Assert.Single(sites);

        Assert.Contains("Ft8TransmitSequence.cs", only, StringComparison.Ordinal);
    }

    // ---- helpers ---------------------------------------------------------

    /// <summary>§R2's CQ, as Hamlet's own modulator makes it at a real endpoint's rate.</summary>
    private static UnslottedTransmission CqAudio()
        => Psk31Modulator.Compose(Psk31Macros.Cq(Mine), Rate, 1000, Ft8Composer.DefaultDrivePeak);

    /// <summary>A CQ with no slot, from a General on the PSK31 watering hole.</summary>
    private static OperatorSend Cq()
        => OperatorSend.Now(CqAudio(), Psk31Dial, LicenseClass.General, true);

    /// <summary>§R2's report with a long place in Settings - real text, over the cap.</summary>
    private static OperatorSend TooLong()
        => OperatorSend.Now(
            Psk31Modulator.Compose(
                Psk31Macros.Report(
                    "VP2V/W1AW", Mine, "Tim", "Llanfairpwllgwyngyll Isle of Anglesey Wales", "FN00DJ"),
                Rate,
                1000,
                Ft8Composer.DefaultDrivePeak),
            Psk31Dial,
            LicenseClass.General,
            true);

    /// <summary>An armed send over a fake wire, a fake card and a recording log.</summary>
    private static (Ft8ArmedSend Armed, FakeSerialPort Port, FakeTransmitAudioSink Sink, RecordingTelemetry Telemetry)
        Armed()
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink { Took = TimeSpan.Zero };
        var telemetry = new RecordingTelemetry();

        return (new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry: telemetry)), port, sink, telemetry);
    }

    /// <summary>Every event, key by key.</summary>
    private void Print(RecordingTelemetry telemetry)
    {
        foreach (var written in telemetry.Events)
        {
            _output.WriteLine($"event     : {written.Event} {written.Level} "
                + string.Join(", ", written.Data.Select(p => $"{p.Key}={p.Value}")));
        }
    }

    /// <summary>
    /// **The record carries no callsign and no words** (HM-DEC-018): every value is a
    /// number, a flag, a stage token, or the name of an enumeration member.
    /// </summary>
    private static void NothingPersonal(TelemetryEvent record)
    {
        var everything = string.Join(" | ", record.Data.Select(p => $"{p.Key}={p.Value}"));

        Assert.DoesNotContain(Mine, everything, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("W1AW", everything, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CQ CQ", everything, StringComparison.OrdinalIgnoreCase);

        string[] stages =
        [
            SendStage.GateAsked, SendStage.Keyed, SendStage.HandedToTheSoundCard, SendStage.Unkeyed, "unknown",
        ];

        foreach (var pair in record.Data)
        {
            if (pair.Value is int or long or double or bool)
            {
                continue;
            }

            var text = Assert.IsType<string>(pair.Value);

            if (pair.Key == "stagesEntered")
            {
                Assert.All(text.Split(" | "), stage => Assert.Contains(stage, stages));
                continue;
            }

            var isEnumName =
                Enum.GetNames<Ft8TransmitOutcome>().Contains(text, StringComparer.Ordinal)
                || Enum.GetNames<UnkeyRoute>().Contains(text, StringComparer.Ordinal)
                || Enum.GetNames<UnslottedMode>().Contains(text, StringComparer.Ordinal)
                || Enum.GetNames<UnslottedFit>().Contains(text, StringComparer.Ordinal);

            Assert.True(isEnumName, $"'{pair.Key}' is \"{text}\", which is not an enumeration name");
        }
    }

    /// <summary>A telemetry sink that keeps every event, in order.</summary>
    private sealed class RecordingTelemetry : ITelemetry
    {
        public List<TelemetryEvent> Events { get; } = [];

        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => Events.Add(new TelemetryEvent(
                DateTime.UnixEpoch, "test", level, "test", category, eventName,
                data ?? new Dictionary<string, object?>(StringComparer.Ordinal)));
    }
}
