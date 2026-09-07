using NAudio.CoreAudioApi;
using NAudio.Wave;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// **Work instruction 256, task 1. A measurement, not an assertion.**
/// </summary>
/// <remarks>
/// <para>This exists because the shell refused to enumerate sound devices, and
/// task 1 permits exactly this: a test constructed by the work instruction that
/// prints what the machine says about itself. It asserts nothing about the
/// numbers it prints, because there is no right answer to assert - a machine
/// with no render endpoint is a normal machine (`SHACK_FACTS.md` FACT-004).</para>
/// <para>**Every figure it prints is about this development machine and says
/// nothing whatever about the IC-7300's USB codec**, which FACT-004 rules is not
/// present here and may not be inferred from here.</para>
/// </remarks>
public sealed class WhatThisMachineCanPlayAndCaptureTests
{
    private readonly ITestOutputHelper _output;

    public WhatThisMachineCanPlayAndCaptureTests(ITestOutputHelper output) =>
        _output = output;

    /// <summary>Names every active render endpoint and the default one's format.</summary>
    [Fact]
    public void WhatRenderEndpointsThisDevelopmentMachineHas()
    {
        using var enumerator = new MMDeviceEnumerator();

        string? defaultId = null;
        try
        {
            using var console = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            defaultId = console.ID;
            _output.WriteLine($"DEFAULT Role.Console : {console.FriendlyName}");
            _output.WriteLine($"DEFAULT ID           : {console.ID}");
            _output.WriteLine($"DEFAULT MIX FORMAT   : {Describe(console.AudioClient.MixFormat)}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"NO DEFAULT RENDER ENDPOINT: {ex.GetType().Name}: {ex.Message}");
        }

        var count = 0;
        foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            using (device)
            {
                count++;
                var isDefault = string.Equals(device.ID, defaultId, StringComparison.Ordinal);
                _output.WriteLine(
                    $"RENDER [{count}]{(isDefault ? " *DEFAULT*" : "")} : {device.FriendlyName}");
                _output.WriteLine($"         ID     : {device.ID}");

                try
                {
                    _output.WriteLine($"         FORMAT : {Describe(device.AudioClient.MixFormat)}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"         FORMAT : unreadable - {ex.GetType().Name}");
                }
            }
        }

        _output.WriteLine($"ACTIVE RENDER ENDPOINT COUNT: {count}");

        var captureCount = 0;
        foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
        {
            using (device)
            {
                captureCount++;
                _output.WriteLine($"CAPTURE [{captureCount}] : {device.FriendlyName}");
            }
        }

        _output.WriteLine($"ACTIVE CAPTURE ENDPOINT COUNT: {captureCount}");
    }

    /// <summary>Starts loopback capture, takes what arrives, stops. The go/no-go.</summary>
    [Fact]
    public void WhetherLoopbackCaptureStartsOnThisDevelopmentMachine()
    {
        WasapiLoopbackCapture? capture = null;

        try
        {
            capture = new WasapiLoopbackCapture();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"LOOPBACK WILL NOT CONSTRUCT: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        _output.WriteLine($"LOOPBACK FORMAT: {Describe(capture.WaveFormat)}");

        var bytes = 0L;
        var buffers = 0;
        var peak = 0.0;
        using var done = new ManualResetEventSlim(false);

        capture.DataAvailable += (_, e) =>
        {
            Interlocked.Add(ref bytes, e.BytesRecorded);
            Interlocked.Increment(ref buffers);

            var format = capture.WaveFormat;
            if (format.BitsPerSample == 32 && e.BytesRecorded >= 4)
            {
                for (var i = 0; i + 4 <= e.BytesRecorded; i += 4)
                {
                    var value = Math.Abs((double)BitConverter.ToSingle(e.Buffer, i));
                    if (value > peak)
                    {
                        peak = value;
                    }
                }
            }
        };

        capture.RecordingStopped += (_, e) =>
        {
            if (e.Exception is not null)
            {
                _output.WriteLine($"LOOPBACK STOPPED WITH: {e.Exception.GetType().Name}: {e.Exception.Message}");
            }

            done.Set();
        };

        try
        {
            capture.StartRecording();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"LOOPBACK WILL NOT START: {ex.GetType().Name}: {ex.Message}");
            capture.Dispose();
            return;
        }

        // A second is long enough for several device buffers to arrive on any
        // endpoint that delivers them at all. A silent machine still delivers
        // buffers of zeroes under WASAPI loopback, which is itself the answer.
        Thread.Sleep(1000);
        capture.StopRecording();
        done.Wait(TimeSpan.FromSeconds(5));

        _output.WriteLine($"LOOPBACK BUFFERS : {buffers}");
        _output.WriteLine($"LOOPBACK BYTES   : {bytes}");
        _output.WriteLine($"LOOPBACK PEAK    : {peak:F9} (nothing was playing, so zero is expected)");
        _output.WriteLine(buffers > 0
            ? "LOOPBACK STARTED AND DELIVERED BUFFERS"
            : "LOOPBACK STARTED AND DELIVERED NOTHING");

        capture.Dispose();
    }

    /// <summary>Which render endpoints will actually open, and which are silent.</summary>
    /// <remarks>
    /// Opens each one in shared mode at its own mix format and closes it again
    /// without writing a sample, so nothing is heard. Task 3 plays 12.64 s of FT8
    /// tones and needs an endpoint chosen deliberately rather than defaulted to.
    /// </remarks>
    [Fact]
    public void WhichRenderEndpointsWillOpenOnThisDevelopmentMachine()
    {
        using var enumerator = new MMDeviceEnumerator();

        foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            using (device)
            {
                var name = device.FriendlyName;

                try
                {
                    var client = device.AudioClient;
                    var mix = client.MixFormat;
                    client.Initialize(
                        AudioClientShareMode.Shared,
                        AudioClientStreamFlags.None,
                        1_000_000,
                        0,
                        mix,
                        Guid.Empty);

                    _output.WriteLine(
                        $"OPENS: {name} - shared mode at {Describe(mix)}, "
                        + $"buffer {client.BufferSize} frames");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"WILL NOT OPEN: {name} - {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>What a device declared, in one line.</summary>
    private static string Describe(WaveFormat format)
    {
        var subformat = format is WaveFormatExtensible extensible
            ? $", subformat {extensible.SubFormat}"
            : string.Empty;

        return $"{format.SampleRate} Hz, {format.Channels} ch, {format.BitsPerSample}-bit, "
            + $"tag {format.Encoding}{subformat}, {format.AverageBytesPerSecond} B/s";
    }
}
