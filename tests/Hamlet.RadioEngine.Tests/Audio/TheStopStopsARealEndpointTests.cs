using System.Diagnostics;
using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// **The same stop, against a real sound card rather than a fake.**
/// </summary>
/// <remarks>
/// <para>**WHY A FAKE IS NOT ENOUGH HERE** (work instruction 263, task 5). Tasks
/// 2 to 4 prove the shape: a token reaches the sink, the sink stops, the abort
/// still fires and the operator is told. But **the fake's play loop was written
/// by the same session that wrote the assertion**, and the number that matters -
/// how much audio actually leaves the machine after the operator's thumb comes
/// off the button - depends on a real endpoint's buffer and on what
/// <c>IAudioClient::Reset</c> really does to samples already inside it. The trace
/// document takes that second one from Microsoft's documentation and marks it
/// unmeasured (<c>docs/unit263-stop-audio-trace.md</c> Q3). **This is where it is
/// measured.**</para>
/// <para>**THIS MAKES SOUND ON A REAL COMPUTER**, on the endpoint
/// <see cref="RenderChoice"/> names in the output and never on whatever the
/// machine defaults to.</para>
/// <para>**NO SERIAL PORT IS OPENED AND NOTHING IS KEYED.** The transport is
/// unit 253's <see cref="FakeSerialPort"/>; a real port and a real sink together
/// would be a real transmission. **Every figure here is about the development
/// machine** and `SHACK_FACTS.md` FACT-004 forbids inferring anything at all
/// about the IC-7300's USB codec from it - no radio has ever been attached to
/// this machine.</para>
/// </remarks>
public sealed class TheStopStopsARealEndpointTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the endpoint and the milliseconds are quoted.</param>
    public TheStopStopsARealEndpointTests(ITestOutputHelper output) => _output = output;

    /// <summary>A slot boundary in the middle of a minute.</summary>
    private static readonly DateTime Boundary =
        new(2026, 9, 7, 2, 15, 0, DateTimeKind.Utc);

    /// <summary>The abort's first frame.</summary>
    private const string CwStop = "FE FE 94 E0 17 FF FD";

    /// <summary>The abort's second frame, and the ordinary unkey.</summary>
    private const string PttOff = "FE FE 94 E0 1C 00 00 FD";

    /// <summary>What the radio would see when it is keyed.</summary>
    private const string KeyOn = "FE FE 94 E0 1C 00 01 FD";

    /// <summary>The bound <c>StopNow</c> returns inside, in milliseconds.</summary>
    /// <remarks>
    /// **The same 250 ms task 3 stated**, and it is the same claim being made
    /// against a harder case: here the thing it must not wait for is a real
    /// endpoint with a real buffer, being written to by another thread.
    /// </remarks>
    private const int Bound = 250;

    /// <summary>
    /// **A full transmission into a real card, stopped a third of the way in.**
    /// </summary>
    /// <remarks>
    /// <para>**THE THREE NUMBERS THIS UNIT WANTED FROM REAL HARDWARE**: how far
    /// short of the total the sink stopped, how long after the cancel the card
    /// actually went quiet, and whether <c>StopNow</c> returned inside its stated
    /// bound with 12.64 seconds of audio genuinely in flight.</para>
    /// <para>**"WENT QUIET" IS MEASURED AS THE PLAY RETURNING**, and that is
    /// honest rather than convenient: <c>PlayAsync</c>'s <c>finally</c> calls
    /// <c>_client.Stop()</c> and <c>_client.Reset()</c> before it returns
    /// (<c>WasapiTransmitSink.cs:398-402</c>), so the moment it hands control back
    /// is the moment the endpoint has been stopped and flushed. **Nothing here
    /// listens to the room** - there is no microphone in this measurement and it
    /// does not claim one.</para>
    /// <para>**AND WHETHER <c>Reset()</c> DISCARDED THE BUFFER OR PLAYED IT OUT IS
    /// READ OFF THE ARITHMETIC**: the sink subtracts what was still stranded
    /// inside the endpoint from what it reports
    /// (<c>WasapiTransmitSink.cs:396</c>), so a played-out buffer would show as
    /// wall time exceeding the audio time by about one buffer, and a discarded one
    /// as the two agreeing.</para>
    /// </remarks>
    [Fact]
    public async Task AStopAThirdOfTheWayInStopsARealCardAndSaysHowMuchWentOut()
    {
        var endpoint = RenderChoice.Preferred(out var why);

        if (endpoint is null)
        {
            _output.WriteLine($"NO RENDER ENDPOINT: {why} - nothing to play into (FACT-004)");

            return;
        }

        using var sink = new WasapiTransmitSink(endpoint.Id);

        if (!Ft8Composer.RateIsUsable(sink.EndpointSampleRate, out var unusable))
        {
            _output.WriteLine($"endpoint        : {sink.DeviceName}  [development machine]");
            _output.WriteLine($"NOT USABLE      : {unusable}");

            return;
        }

        var composed = Ft8Composer.ComposeSignal("W1ABC KC3QIS -10", sink.EndpointSampleRate);
        Assert.True(composed.Composed, composed.Explanation);

        var transmission = composed.Transmission!;
        var total = transmission.Samples.Length;
        var rate = transmission.SampleRate;
        var slot = TimeSpan.FromSeconds(total / (double)rate);

        var port = new FakeSerialPort();
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink));

        armed.Arm(new OperatorSend(
            transmission, 14_074_000, LicenseClass.General, true, Boundary, 0.5));

        var clock = Stopwatch.StartNew();

        // NO TOKEN, exactly as the application runs it.
        var running = armed.AtBoundaryAsync(Boundary);

        // A THIRD OF THE WAY IN, BY THE WALL CLOCK. This is real time - there is
        // no compressing a sound card.
        var third = slot / 3;

        while (clock.Elapsed < third)
        {
            await Task.Delay(5, CancellationToken.None);
        }

        var atTheStop = clock.Elapsed;

        var stopClock = Stopwatch.StartNew();
        var stop = armed.StopNow(port);
        stopClock.Stop();

        var boundary = await running;
        var wentQuietAt = clock.Elapsed;
        clock.Stop();

        var played = boundary.Run!.Played!.Value;
        var audioOut = played.SamplesPlayed * 1000.0 / rate;
        var quietAfter = (wentQuietAt - atTheStop).TotalMilliseconds;

        _output.WriteLine($"endpoint        : {sink.DeviceName}  [development machine]");
        _output.WriteLine($"chosen because  : {why}");
        _output.WriteLine($"endpoint format : {sink.EndpointSampleRate} Hz, "
            + $"{sink.EndpointChannels} ch, {sink.EndpointEncoding}  [development machine]");
        _output.WriteLine($"buffer          : {sink.BufferFrames} frames "
            + $"({sink.BufferFrames * 1000.0 / sink.EndpointSampleRate:0.#} ms)"
            + "  [development machine]");
        _output.WriteLine($"the slot        : {total} samples, {slot.TotalSeconds:0.###} s");
        _output.WriteLine($"stop pressed at : {atTheStop.TotalSeconds:0.###} s in");
        _output.WriteLine($"StopNow took    : {stopClock.Elapsed.TotalMilliseconds:0.#} ms "
            + $"(bound {Bound} ms)");
        _output.WriteLine($"samples played  : {played.SamplesPlayed} of {total}, "
            + $"short by {total - played.SamplesPlayed}");
        _output.WriteLine($"audio that went : {audioOut:0} ms of {slot.TotalMilliseconds:0} ms");
        _output.WriteLine($"CARD WENT QUIET : {quietAfter:0} ms after the stop"
            + "  [development machine]");
        _output.WriteLine($"sink says took  : {played.Took.TotalMilliseconds:0} ms");
        _output.WriteLine($"audio vs wall   : {audioOut - played.Took.TotalMilliseconds:0} ms "
            + "(negative means the card was still holding audio the sink did not count)");
        _output.WriteLine($"the run said    : {boundary.Run.Outcome}");
        _output.WriteLine($"came out of tx  : {boundary.Run.CameOutOfTransmit}");
        _output.WriteLine($"stop outcome    : {stop.Outcome}");
        _output.WriteLine($"wire            : {string.Join(" | ", Frames(port))}");

        // ---- the carrier ------------------------------------------------------
        Assert.True(stop.AnythingReachedTheRadio);
        Assert.Contains(CwStop, Frames(port), StringComparer.Ordinal);
        Assert.Contains(PttOff, Frames(port), StringComparer.Ordinal);

        // ---- the sound, on a real card ----------------------------------------
        Assert.True(
            played.SamplesPlayed < total,
            $"the whole slot went out of a real endpoint anyway: "
            + $"{played.SamplesPlayed} of {total}");

        Assert.True(
            quietAfter < 1000,
            $"the card was still playing {quietAfter:0} ms after the stop");

        // ---- and the stop did not wait for any of it ---------------------------
        Assert.True(
            stopClock.Elapsed.TotalMilliseconds < Bound,
            $"StopNow took {stopClock.Elapsed.TotalMilliseconds:0.#} ms against a real "
            + $"endpoint, which is past the {Bound} ms bound - it waited for something");

        Assert.Equal(Ft8TransmitOutcome.Cancelled, boundary.Run.Outcome);
    }

    /// <summary>Every frame the fake transport took, as hex, split on FD.</summary>
    /// <param name="port">The fake wire.</param>
    /// <returns>One string per frame, in order.</returns>
    private static string[] Frames(FakeSerialPort port)
    {
        var frames = new List<string>();
        var current = new List<byte>();

        foreach (var b in port.Written)
        {
            current.Add(b);

            if (b == 0xFD)
            {
                frames.Add(string.Join(
                    ' ', current.Select(x => x.ToString("X2", CultureInfo.InvariantCulture))));
                current.Clear();
            }
        }

        return [.. frames];
    }
}
