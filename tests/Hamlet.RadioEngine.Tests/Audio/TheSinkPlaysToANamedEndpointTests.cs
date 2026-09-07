using System.Diagnostics;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Tests.Transmit;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// **The render sink: it opens the endpoint it was named, it says what rate it
/// got, and it does not say it is finished until the card has emptied.**
/// </summary>
/// <remarks>
/// <para>**These tests make sound on a real computer** (work instruction 256,
/// task 2). Each one says which endpoint and for how long. The endpoint is chosen
/// by <see cref="RenderChoice"/> and named in the output, never defaulted to.
/// </para>
/// <para>**Every device figure here is about the development machine** and says
/// nothing about the IC-7300's USB codec, which `SHACK_FACTS.md` FACT-004 rules
/// is not present on this machine and may not be inferred from it.</para>
/// <para>**No serial port is opened and nothing is keyed.** The one test that
/// runs the transmit sequence drives it with unit 253's fake transport, because a
/// real port and a real sink together would be a real transmission.</para>
/// </remarks>
public sealed class TheSinkPlaysToANamedEndpointTests
{
    private readonly ITestOutputHelper _output;

    public TheSinkPlaysToANamedEndpointTests(ITestOutputHelper output) => _output = output;

    /// <summary>**It opens what it was told to open, and reports the format it found.**</summary>
    [Fact]
    public void TheSinkOpensTheEndpointItWasNamedAndSaysWhatItGot()
    {
        var endpoint = RenderChoice.Preferred(out var why);

        if (endpoint is null)
        {
            _output.WriteLine($"NO RENDER ENDPOINT: {why} - nothing to open (FACT-004)");

            return;
        }

        using var sink = new WasapiTransmitSink(endpoint.Id);

        _output.WriteLine($"chosen because  : {why}");
        _output.WriteLine($"requested       : {sink.Requested}");
        _output.WriteLine($"opened          : {sink.DeviceName}  [development machine]");
        _output.WriteLine($"endpoint id     : {sink.DeviceId}");
        _output.WriteLine($"share mode      : {sink.ShareMode}");
        _output.WriteLine(
            $"endpoint format : {sink.EndpointSampleRate} Hz, {sink.EndpointChannels} ch, "
            + $"{sink.EndpointEncoding}  [development machine]");
        _output.WriteLine($"buffer          : {sink.BufferFrames} frames "
            + $"({sink.BufferFrames * 1000.0 / sink.EndpointSampleRate:0.#} ms)  [development machine]");

        Assert.Equal(endpoint.Id, sink.DeviceId);
        Assert.Equal(endpoint.Name, sink.DeviceName);
        Assert.Equal(endpoint.SampleRate, sink.EndpointSampleRate);
        Assert.True(sink.BufferFrames > 0);
    }

    /// <summary>
    /// **An endpoint that is not there is a refusal, not a fallback to the
    /// default.**
    /// </summary>
    /// <remarks>
    /// A transmission arriving at the wrong endpoint is the software shape of a
    /// transmission going out on the wrong band, and every number a sink reports
    /// would look identical either way.
    /// </remarks>
    [Fact]
    public void AnEndpointThatIsNotThereIsRefusedRatherThanFallenBackFrom()
    {
        const string NotThere = "this endpoint does not exist on any machine 256";

        var thrown = Assert.Throws<InvalidOperationException>(
            () => new WasapiTransmitSink(NotThere));

        _output.WriteLine($"asked for : {NotThere}");
        _output.WriteLine($"refusal   : {thrown.Message}");

        Assert.Contains(NotThere, thrown.Message, StringComparison.Ordinal);

        // And it did not open the default one instead. If there is a default, its
        // name is not in the refusal and no sink came back at all.
        var endpoint = RenderChoice.Preferred(out _);

        if (endpoint is not null)
        {
            Assert.DoesNotContain(endpoint.Name, thrown.Message, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// **A rate the endpoint does not speak is refused, not quietly resampled.**
    /// </summary>
    /// <remarks>
    /// Shared-mode WASAPI will happily resample anything handed to it, which is
    /// how a transmit path acquires a rate conversion nobody chose and nobody can
    /// see. The refusal names both numbers.
    /// </remarks>
    [Fact]
    public async Task ARateTheEndpointDoesNotSpeakIsRefusedRatherThanQuietlyChanged()
    {
        var endpoint = RenderChoice.Preferred(out var why);

        if (endpoint is null)
        {
            _output.WriteLine($"NO RENDER ENDPOINT: {why} (FACT-004)");

            return;
        }

        using var sink = new WasapiTransmitSink(endpoint.Id);

        var wrong = sink.EndpointSampleRate == 12_000 ? 48_000 : 12_000;
        var samples = new float[wrong];

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sink.PlayAsync(samples, wrong, CancellationToken.None));

        _output.WriteLine($"endpoint     : {sink.DeviceName}  [development machine]");
        _output.WriteLine($"rate asked   : {sink.RateAsked} Hz");
        _output.WriteLine($"rate got     : {sink.RateGot} Hz  [development machine]");
        _output.WriteLine($"refusal      : {thrown.Message}");

        Assert.Equal(wrong, sink.RateAsked);
        Assert.Contains(wrong.ToString(System.Globalization.CultureInfo.InvariantCulture),
            thrown.Message, StringComparison.Ordinal);
        Assert.Contains(
            sink.EndpointSampleRate.ToString(System.Globalization.CultureInfo.InvariantCulture),
            thrown.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE ONE THIS UNIT WATCHED FAIL FIRST. It does not return until the
    /// endpoint has emptied.**
    /// </summary>
    /// <remarks>
    /// <para>**The breakage:** a sink that writes its last buffer and returns
    /// <c>SamplesPlayed = samples.Length</c> without waiting for the card to drain.
    /// It passes any test that only counts samples, and it is a lie - the caller
    /// is told the audio has gone out while the last buffer is still inside the
    /// device, and whatever it does next it does during a transmission that has
    /// not finished.</para>
    /// <para>**A stopwatch is the only thing that can catch it**, which is why
    /// this test measures elapsed time against the audio's own duration rather
    /// than counting anything.</para>
    /// <para>Two seconds of a quiet tone, not 12.64 - long enough that the
    /// endpoint's own buffer is a small fraction of it and short enough to run in
    /// a test.</para>
    /// </remarks>
    [Fact]
    public async Task ThePlayDoesNotReturnUntilTheEndpointHasEmptied()
    {
        var endpoint = RenderChoice.Preferred(out var why);

        if (endpoint is null)
        {
            _output.WriteLine($"NO RENDER ENDPOINT: {why} (FACT-004)");

            return;
        }

        using var sink = new WasapiTransmitSink(endpoint.Id);

        const double Seconds = 2.0;
        var samples = RenderChoice.Tone(Seconds, sink.EndpointSampleRate, 1000.0, 0.25);
        var duration = TimeSpan.FromSeconds(samples.Length / (double)sink.EndpointSampleRate);

        var clock = Stopwatch.StartNew();
        var played = await sink.PlayAsync(samples, sink.EndpointSampleRate, CancellationToken.None);
        clock.Stop();

        _output.WriteLine($"endpoint        : {sink.DeviceName}  [development machine]");
        _output.WriteLine($"chosen because  : {why}");
        _output.WriteLine($"audio           : {samples.Length} samples, {duration.TotalSeconds:0.###} s");
        _output.WriteLine($"buffer          : {sink.BufferFrames} frames "
            + $"({sink.BufferFrames * 1000.0 / sink.EndpointSampleRate:0.#} ms)  [development machine]");
        _output.WriteLine($"samples played  : {played.SamplesPlayed}");
        _output.WriteLine($"sink says took  : {played.Took.TotalSeconds:0.###} s");
        _output.WriteLine($"outside measure : {clock.Elapsed.TotalSeconds:0.###} s");
        _output.WriteLine($"peak written    : {sink.PeakWritten:0.######}");
        _output.WriteLine($"rms written     : {sink.RmsWritten:0.######}");
        _output.WriteLine($"clipped         : {sink.ClippedSamples}");

        // The whole transmission went out.
        Assert.Equal(samples.Length, played.SamplesPlayed);

        // **AND IT TOOK AS LONG AS THE AUDIO IS LONG.** A sink that returns before
        // the card has emptied comes back roughly one buffer early, and the fifty
        // milliseconds of slack here is far smaller than the two hundred that
        // costs.
        var floor = duration - TimeSpan.FromMilliseconds(50);
        Assert.True(
            played.Took >= floor,
            $"the sink said it played {duration.TotalSeconds:0.###} s of audio in "
            + $"{played.Took.TotalSeconds:0.###} s, which is less time than the audio lasts - "
            + "the last buffer was still in the endpoint when it returned");

        // A quiet tone is a quiet tone: nothing clipped on the way out.
        Assert.Equal(0, sink.ClippedSamples);
    }

    /// <summary>
    /// **Cancelled part way through, the sink says how much went out - and the
    /// sequence calls that a failure.**
    /// </summary>
    /// <remarks>
    /// <c>Ft8TransmitSequence</c> is not touched. It already treats
    /// <c>SamplesPlayed != samples.Length</c> as <c>AudioFailed</c>, and this
    /// proves the real sink produces exactly that shape rather than throwing or
    /// rounding up. Two seconds of audio, cancelled at four hundred milliseconds.
    /// </remarks>
    [Fact]
    public async Task ACancelledPlayGoesOutShortAndTheSequenceCallsItAudioFailed()
    {
        var endpoint = RenderChoice.Preferred(out var why);

        if (endpoint is null)
        {
            _output.WriteLine($"NO RENDER ENDPOINT: {why} (FACT-004)");

            return;
        }

        using var sink = new WasapiTransmitSink(endpoint.Id);

        var samples = RenderChoice.Tone(2.0, sink.EndpointSampleRate, 1000.0, 0.25);

        // A transmission record carrying two seconds of tone rather than 12.64 s of
        // FT8. The sequence cares about the sample count and the rate, and a real
        // transmission here would cost the run twelve seconds to prove nothing more.
        var transmission = new Ft8Transmission(
            "SHORT PLAY", "SHORT PLAY", Ft8MessageType.FreeText,
            samples, sink.EndpointSampleRate, 1000.0f, false);

        var send = new OperatorSend(
            transmission, 14_074_000, LicenseClass.General, true,
            new DateTime(2026, 9, 6, 23, 45, 0, DateTimeKind.Utc), 0.5);

        var port = new FakeSerialPort();
        var sequence = new Ft8TransmitSequence(port, sink);

        using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(400));

        var clock = Stopwatch.StartNew();
        var run = await sequence.RunAsync(send, cancel.Token);
        clock.Stop();

        _output.WriteLine($"endpoint        : {sink.DeviceName}  [development machine]");
        _output.WriteLine($"offered         : {samples.Length} samples at {sink.EndpointSampleRate} Hz");
        _output.WriteLine($"played          : {run.Played?.SamplesPlayed}");
        _output.WriteLine($"wall clock      : {clock.Elapsed.TotalSeconds:0.###} s");
        _output.WriteLine($"outcome         : {run.Outcome}");
        _output.WriteLine($"reason          : {run.Reason}");
        _output.WriteLine($"came out of tx  : {run.CameOutOfTransmit}");

        Assert.NotNull(run.Played);
        Assert.True(
            run.Played!.Value.SamplesPlayed < samples.Length,
            $"cancelled at 400 ms of 2000 ms and the sink still reported "
            + $"{run.Played.Value.SamplesPlayed} of {samples.Length} samples");
        Assert.True(run.Played.Value.SamplesPlayed > 0);

        // And the sequence, unchanged, calls a short play a failure.
        Assert.Equal(Ft8TransmitOutcome.AudioFailed, run.Outcome);
        Assert.True(run.RadioIsInReceive);
    }

    /// <summary>**The sink knows nothing about anything but a sound card.**</summary>
    /// <remarks>
    /// Checked against the file the way
    /// <c>TheSeamNamesNoRadioNoDeviceAndNoEncoderOfItsOwn</c> checks the composer.
    /// The sink is allowed to name WASAPI and NAudio - that is its whole job - and
    /// is not allowed to name a rig, a port, or a control protocol.
    /// </remarks>
    [Fact]
    public void TheSinkNamesNoRadioOfItsOwn()
    {
        var path = SinkPath();
        var source = File.ReadAllText(path);
        var body = string.Join(
            '\n',
            source.Split('\n')
                .Where(line => !line.TrimStart().StartsWith("///", StringComparison.Ordinal)));

        string[] forbidden =
        [
            "TransmitAbort", "PTT", "Ptt", "Civ", "CIV", "SerialPort", "ISerialPort",
            "Ic7300", "IRig", "RigState", "TransmitGuard", "LicenseClass", "Frequency",
        ];

        var found = forbidden.Where(name => body.Contains(name, StringComparison.Ordinal)).ToList();

        _output.WriteLine($"sink           : {path}");
        _output.WriteLine($"lines          : {source.Split('\n').Length}");
        _output.WriteLine($"patterns tried : {forbidden.Length}");
        _output.WriteLine($"found in code  : {(found.Count == 0 ? "none" : string.Join(", ", found))}");

        Assert.Empty(found);
    }

    /// <summary>The sink's source file, found from this test assembly's location.</summary>
    private static string SinkPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Hamlet.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return Path.Combine(
            directory!.FullName, "src", "Hamlet.RadioEngine", "Audio", "WasapiTransmitSink.cs");
    }
}
