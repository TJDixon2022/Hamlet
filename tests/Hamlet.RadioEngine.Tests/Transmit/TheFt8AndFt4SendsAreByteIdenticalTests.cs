using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// Work instruction 318 task 3a: **what an FT8 and an FT4 send put on the wire today, pinned
/// before the chain learns to carry a send with no slot.**
/// </summary>
/// <remarks>
/// <para>**§R10 SAYS FT8 AND FT4 STAY BYTE-IDENTICAL, AND THIS IS HOW THAT IS MEASURED
/// RATHER THAN ASSERTED.** Written and run green on the unchanged chain, committed on its
/// own, and never edited afterwards: whatever the chain becomes, these two sends must still
/// put the same bytes on the port, the same samples in the sink, the same run in the result
/// and the same lines in the record.</para>
/// <para>**BUILT AS THE APPLICATION BUILDS THEM.** <c>SendMessage</c> composes with
/// <c>ComposeSignal</c> at the endpoint's declared rate and the operator's drive level,
/// arms an <see cref="OperatorSend"/> at the next boundary half a second in with the grid on
/// the send, and the slot tick hands that boundary to
/// <see cref="Ft8ArmedSend.AtBoundaryAsync"/>. Here the endpoint declares 48 000 Hz and the
/// drive is the shipped default.</para>
/// <para>**NOTHING REAL IS OPENED.** A recording port and a recording sink; no radio and
/// no sound card (FACT-006).</para>
/// </remarks>
public sealed class TheFt8AndFt4SendsAreByteIdenticalTests
{
    private const string Message = "CQ KC3QIS FN00";
    private const int EndpointRate = 48_000;

    private static readonly DateTime Ft8Slot = new(2026, 9, 11, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Ft4Slot = new(2026, 9, 11, 18, 0, 7, 500, DateTimeKind.Utc);

    /// <summary>The FT8 send, as the unchanged chain at <c>30d5c78</c> produced it.</summary>
    private static readonly string[] Ft8Pinned =
    [
        "boundary: Ran",
        "port: FE FE 94 E0 1C 00 01 FD FE FE 94 E0 1C 00 00 FD",
        "sink: 1 call, 606720 samples at 48000 Hz, sha256 a66c5929374bb079f385a728cd614a14f9a394cfad8ea7570d4c6f6fdee22af1",
        "run.Outcome: Played",
        "run.Reason: ",
        "run.Citation: 0 chars, sha256 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
        "run.Keyed: True",
        "run.UnkeyedNormally: True",
        "run.Abort: null",
        "run.Played: 606720 in 00:00:00",
        "run.SamplesOffered: 606720",
        "run.SecondsOffered: 12.64",
        "run.CameOutOfTransmit: OrdinaryUnkey",
        "run.RadioIsInReceive: True",
        "event: send_stage Info Transmit",
        "  stage = String gate_asked",
        "  entered = Boolean True",
        "  detail = String 14074000 Hz",
        "event: send_stage Info Transmit",
        "  stage = String keyed",
        "  entered = Boolean True",
        "  detail = String unknown",
        "event: send_stage Info Transmit",
        "  stage = String handed_to_the_sound_card",
        "  entered = Boolean True",
        "  detail = String 606720 samples",
        "event: send_stage Info Transmit",
        "  stage = String unkeyed",
        "  entered = Boolean True",
        "  detail = String unknown",
        "event: ft8_transmission Info Transmit",
        "  slotStartUtc = String 2026-09-11T18:00:00.0000000Z",
        "  startSecondsIntoSlot = Double 0.5",
        "  frequencyHz = Int64 14074000",
        "  durationSeconds = Double 12.64",
        "  sampleRate = Int32 48000",
        "  sampleCount = Int32 606720",
        "  messageType = String Standard",
        "  messageLength = Int32 14",
        "  carriedHashedCallsign = Boolean False",
        "  outcome = String Played",
        "  cameOutOfTransmit = String OrdinaryUnkey",
        "  keyed = Boolean True",
        "  stagesEntered = String gate_asked | keyed | handed_to_the_sound_card | unkeyed",
    ];

    /// <summary>The FT4 send, as the unchanged chain at <c>30d5c78</c> produced it.</summary>
    private static readonly string[] Ft4Pinned =
    [
        "boundary: Ran",
        "port: FE FE 94 E0 1C 00 01 FD FE FE 94 E0 1C 00 00 FD",
        "sink: 1 call, 241920 samples at 48000 Hz, sha256 a224275149be459b4a22621ecd4240fbbb8c474e5435729af2446eeedeb9066b",
        "run.Outcome: Played",
        "run.Reason: ",
        "run.Citation: 0 chars, sha256 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
        "run.Keyed: True",
        "run.UnkeyedNormally: True",
        "run.Abort: null",
        "run.Played: 241920 in 00:00:00",
        "run.SamplesOffered: 241920",
        "run.SecondsOffered: 5.04",
        "run.CameOutOfTransmit: OrdinaryUnkey",
        "run.RadioIsInReceive: True",
        "event: send_stage Info Transmit",
        "  stage = String gate_asked",
        "  entered = Boolean True",
        "  detail = String 14080000 Hz",
        "event: send_stage Info Transmit",
        "  stage = String keyed",
        "  entered = Boolean True",
        "  detail = String unknown",
        "event: send_stage Info Transmit",
        "  stage = String handed_to_the_sound_card",
        "  entered = Boolean True",
        "  detail = String 241920 samples",
        "event: send_stage Info Transmit",
        "  stage = String unkeyed",
        "  entered = Boolean True",
        "  detail = String unknown",
        "event: ft8_transmission Info Transmit",
        "  slotStartUtc = String 2026-09-11T18:00:07.5000000Z",
        "  startSecondsIntoSlot = Double 0.5",
        "  frequencyHz = Int64 14080000",
        "  durationSeconds = Double 5.04",
        "  sampleRate = Int32 48000",
        "  sampleCount = Int32 241920",
        "  messageType = String Standard",
        "  messageLength = Int32 14",
        "  carriedHashedCallsign = Boolean False",
        "  outcome = String Played",
        "  cameOutOfTransmit = String OrdinaryUnkey",
        "  keyed = Boolean True",
        "  stagesEntered = String gate_asked | keyed | handed_to_the_sound_card | unkeyed",
    ];

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every pinned line is printed.</param>
    public TheFt8AndFt4SendsAreByteIdenticalTests(ITestOutputHelper output) => _output = output;

    /// <summary>**One FT8 send, byte for byte as it went out before §R10.**</summary>
    [Fact]
    public async Task AnFt8SendIsTheSameBytesSamplesRunAndRecord()
    {
        var composed = Ft8Composer.ComposeSignal(
            Message, EndpointRate, Ft8Composer.DefaultBaseFrequencyHz, Ft8Composer.DefaultDrivePeak);

        Assert.True(composed.Composed, composed.Explanation);

        var send = new OperatorSend(
            composed.Transmission!, 14_074_000, LicenseClass.General, true, Ft8Slot, 0.5)
        {
            Grid = SlotGrid.Ft8,
        };

        Assert.Equal(Ft8Pinned, await Measure("FT8", send));
    }

    /// <summary>**One FT4 send, byte for byte as it went out before §R10.**</summary>
    [Fact]
    public async Task AnFt4SendIsTheSameBytesSamplesRunAndRecord()
    {
        var composed = Ft4Composer.ComposeSignal(
            Message, EndpointRate, Ft4Composer.DefaultBaseFrequencyHz, Ft8Composer.DefaultDrivePeak);

        Assert.True(composed.Composed, composed.Explanation);

        var send = new OperatorSend(
            composed.Transmission!, 14_080_000, LicenseClass.General, true, Ft4Slot, 0.5)
        {
            Grid = SlotGrid.Ft4,
        };

        Assert.Equal(Ft4Pinned, await Measure("FT4", send));
    }

    /// <summary>Arm it, hand it its boundary, and write down everything that came out.</summary>
    private async Task<string[]> Measure(string mode, OperatorSend send)
    {
        var port = new FakeSerialPort();
        var sink = new RecordingSink();
        var telemetry = new RecordingTelemetry();
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry: telemetry));

        armed.Arm(send);

        var result = await armed.AtBoundaryAsync(send.SlotStartUtc);
        var run = result.Run!;

        var lines = new List<string>
        {
            "boundary: " + result.Outcome,
            "port: " + TheUnkeyHappensWhateverGoesWrongTests.Hex(port.Written),
            "sink: " + sink.Calls + " call, " + sink.Samples.Length + " samples at " + sink.Rate
                + " Hz, sha256 " + Sha256(MemoryMarshal.AsBytes(sink.Samples.AsSpan())),
            "run.Outcome: " + run.Outcome,
            "run.Reason: " + run.Reason,
            "run.Citation: " + run.Citation.Length + " chars, sha256 "
                + Sha256(Encoding.UTF8.GetBytes(run.Citation)),
            "run.Keyed: " + run.Keyed,
            "run.UnkeyedNormally: " + run.UnkeyedNormally,
            "run.Abort: " + (run.Abort is null ? "null" : "fired"),
            "run.Played: " + (run.Played is { } played ? played.SamplesPlayed + " in " + played.Took : "null"),
            "run.SamplesOffered: " + run.SamplesOffered,
            "run.SecondsOffered: " + run.SecondsOffered.ToString("R", CultureInfo.InvariantCulture),
            "run.CameOutOfTransmit: " + run.CameOutOfTransmit,
            "run.RadioIsInReceive: " + run.RadioIsInReceive,
        };

        foreach (var written in telemetry.Events)
        {
            lines.Add("event: " + written.Event + " " + written.Level + " " + written.Category);

            foreach (var pair in written.Data)
            {
                lines.Add("  " + pair.Key + " = " + Shown(pair.Value));
            }
        }

        _output.WriteLine($"---- {mode}: paste-ready ----");

        foreach (var line in lines)
        {
            _output.WriteLine("        \"" + line.Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal) + "\",");
        }

        return lines.ToArray();
    }

    /// <summary>A value with its type, so a number that became a string would show.</summary>
    private static string Shown(object? value) => value switch
    {
        null => "null",
        double d => "Double " + d.ToString("R", CultureInfo.InvariantCulture),
        IFormattable f => value.GetType().Name + " " + f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.GetType().Name + " " + value,
    };

    private static string Sha256(ReadOnlySpan<byte> bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    /// <summary>A sink that keeps a copy of what it was handed and plays it at once.</summary>
    private sealed class RecordingSink : ITransmitAudioSink
    {
        public float[] Samples { get; private set; } = [];

        public int Rate { get; private set; }

        public int Calls { get; private set; }

        public int EndpointSampleRate => EndpointRate;

        public Task<PlayedAudio> PlayAsync(
            ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
        {
            Calls++;
            Samples = samples.ToArray();
            Rate = sampleRate;

            return Task.FromResult(new PlayedAudio(samples.Length, TimeSpan.Zero));
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
                DateTime.UnixEpoch, "pin", level, "pin", category, eventName,
                data ?? new Dictionary<string, object?>(StringComparer.Ordinal)));
    }
}
