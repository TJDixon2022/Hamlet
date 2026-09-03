using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// Lists the machine's capture devices through WASAPI.
/// </summary>
/// <remarks>
/// Never throws. A machine with no sound card, a driver mid-install, or an
/// operating system that has no WASAPI at all must all produce an empty list
/// and let the app carry on, because the Explorer and the training radio work
/// perfectly well with nothing plugged in (§8).
/// </remarks>
public sealed class WasapiAudioDevices : IAudioDevices
{
    /// <inheritdoc/>
    public IReadOnlyList<AudioDevice> List()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();

            var defaultId = DefaultCaptureId(enumerator);
            var devices = new List<AudioDevice>();

            foreach (var device in enumerator.EnumerateAudioEndPoints(
                DataFlow.Capture, DeviceState.Active))
            {
                using (device)
                {
                    devices.Add(new AudioDevice(
                        device.ID,
                        device.FriendlyName,
                        string.Equals(device.ID, defaultId, StringComparison.Ordinal)));
                }
            }

            return devices;
        }
        catch (Exception)
        {
            return Array.Empty<AudioDevice>();
        }
    }

    /// <summary>
    /// What Windows is doing to one capture device (HM-DEC-088).
    /// </summary>
    /// <param name="deviceId">Which device, or null for the default one.</param>
    /// <returns>What was read, with unread values left null.</returns>
    /// <remarks>
    /// **NEVER THROWS AND NEVER GUESSES.** A machine with no sound card, a device
    /// that has gone away, or an endpoint that will not answer all produce
    /// nulls rather than an exception or a plausible number, because a level
    /// nobody read reported as a level would send the operator to adjust
    /// something that was never the problem (§0.0).
    /// </remarks>
    public static CaptureHealth Health(string? deviceId)
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();

            using var device = string.IsNullOrEmpty(deviceId)
                ? enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console)
                : enumerator.GetDevice(deviceId);

            if (device is null)
            {
                return CaptureHealth.Unknown;
            }

            return new CaptureHealth(
                device.FriendlyName,
                device.AudioEndpointVolume.MasterVolumeLevelScalar,
                device.AudioEndpointVolume.Mute);
        }
        catch (Exception)
        {
            return CaptureHealth.Unknown;
        }
    }

    private static string? DefaultCaptureId(MMDeviceEnumerator enumerator)
    {
        try
        {
            using var device = enumerator.GetDefaultAudioEndpoint(
                DataFlow.Capture, Role.Console);
            return device.ID;
        }
        catch (Exception)
        {
            // A machine with no default capture device is a normal machine.
            return null;
        }
    }
}

/// <summary>
/// Receive audio from a real sound card, which on a connected IC-7300 is its
/// USB codec.
/// </summary>
/// <remarks>
/// <para>The only class in the engine that knows what a sound device is. The
/// decoder above it sees <see cref="IAudioSource"/> and nothing more, which is
/// what lets every decoder test run without hardware (HM-DEC-007) and what
/// would let the engine be wrapped in a console app without touching it
/// (§0.1).</para>
/// <para>Whatever format the device hands over is converted here to mono
/// floats, because the rest of the engine has no business knowing that some
/// codecs speak 24-bit and some speak float, or that this one is stereo with
/// silence in the right channel. The sample rate is passed through rather than
/// resampled: the decoder derives all its timing from sample counts, so it is
/// indifferent to the rate, and resampling would be work done to no end on the
/// one path that has to keep up with real time.</para>
/// </remarks>
public sealed class WasapiAudioSource : IAudioSource
{
    private readonly object _gate = new();
    private readonly string _deviceId;

    private WasapiCapture? _capture;
    private float[] _mono = Array.Empty<float>();
    private long _delivered;
    private bool _disposed;

    /// <summary>Opens a source over a capture device.</summary>
    /// <param name="device">The device to listen to.</param>
    /// <exception cref="InvalidOperationException">The device cannot be opened.</exception>
    public WasapiAudioSource(AudioDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        _deviceId = device.Id;
        DeviceName = device.Name;

        try
        {
            using var enumerator = new MMDeviceEnumerator();
            using var endpoint = enumerator.GetDevice(_deviceId);

            _capture = new WasapiCapture(endpoint);
            _capture.DataAvailable += OnDataAvailable;
            SampleRate = _capture.WaveFormat.SampleRate;

            // **THE REST OF THE FORMAT, READ ONCE AND KEPT** (§0.0.1). A capture
            // sheet that does not say how many channels the device delivers, or at
            // what depth, cannot separate a deaf decoder from a sound card
            // delivering something nobody expected.
            ChannelCount = _capture.WaveFormat.Channels;
            Encoding = $"{_capture.WaveFormat.Encoding} "
                + $"{_capture.WaveFormat.BitsPerSample}-bit";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"could not open the audio device '{device.Name}'", ex);
        }
    }

    /// <inheritdoc/>
    public string DeviceName { get; }

    /// <inheritdoc/>
    public int SampleRate { get; }

    /// <summary>How many channels the device delivers, as the driver reports it.</summary>
    /// <remarks>
    /// **NOT ON <see cref="IAudioSource"/>, DELIBERATELY.** The training radio has
    /// no channel count and no encoding, and a seam widened to carry nulls for it
    /// would put a row on every sheet that never says anything. A caller that wants
    /// this asks the WASAPI source for it and reports *unknown (not read)* when it
    /// is looking at something else.
    /// </remarks>
    public int ChannelCount { get; }

    /// <summary>The encoding and bit depth, as the driver reports them.</summary>
    public string Encoding { get; } = "";

    /// <inheritdoc/>
    /// <remarks>
    /// Always false, with no setter, which is the other half of HM-DEC-026's
    /// guarantee. Real audio says it is real and synthesized audio says it is
    /// synthesized, and neither can be talked out of it.
    /// </remarks>
    public bool IsSimulated => false;

    /// <inheritdoc/>
    public bool IsRunning { get; private set; }

    /// <inheritdoc/>
    public event AudioChunkHandler? SamplesReady;

    /// <inheritdoc/>
    public void Start()
    {
        lock (_gate)
        {
            if (_disposed || IsRunning || _capture is null)
            {
                return;
            }

            _capture.StartRecording();
            IsRunning = true;
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        lock (_gate)
        {
            if (!IsRunning || _capture is null)
            {
                return;
            }

            try
            {
                _capture.StopRecording();
            }
            catch (Exception)
            {
                // Tearing down an audio device is best-effort (§8).
            }

            IsRunning = false;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Stop();

        lock (_gate)
        {
            _disposed = true;

            if (_capture is not null)
            {
                _capture.DataAvailable -= OnDataAvailable;
                _capture.Dispose();
                _capture = null;
            }
        }
    }

    /// <summary>
    /// One buffer from the device, on WASAPI's own thread.
    /// </summary>
    /// <remarks>
    /// Allocates nothing after the first buffer: the mono scratch array is
    /// grown once and reused, and the chunk handed on is a span over it. This
    /// runs every few milliseconds for as long as the app is open, and it is
    /// the same discipline HM-DEC-006 imposed on the waterfall for the same
    /// reason.
    /// </remarks>
    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        var handler = SamplesReady;
        var format = _capture?.WaveFormat;

        if (handler is null || format is null || e.BytesRecorded <= 0)
        {
            return;
        }

        try
        {
            var frames = Downmix(e.Buffer, e.BytesRecorded, format);
            if (frames <= 0)
            {
                return;
            }

            var chunk = new AudioChunk(_delivered, SampleRate, _mono.AsSpan(0, frames));
            _delivered += frames;
            handler(in chunk);
        }
        catch (Exception)
        {
            // A bad buffer must not take down the capture thread (§8). A
            // dropped chunk shows up as a gap in the decode, which the
            // terminal's own note already knows how to say out loud.
        }
    }

    /// <summary>
    /// Convert the device's buffer into mono floats.
    /// </summary>
    /// <returns>How many mono samples were written.</returns>
    private int Downmix(byte[] buffer, int byteCount, WaveFormat format)
    {
        var channels = Math.Max(1, format.Channels);
        var bytesPerSample = format.BitsPerSample / 8;

        if (bytesPerSample <= 0)
        {
            return 0;
        }

        var frames = byteCount / (bytesPerSample * channels);

        if (_mono.Length < frames)
        {
            _mono = new float[frames];
        }

        for (var frame = 0; frame < frames; frame++)
        {
            var sum = 0.0;

            for (var channel = 0; channel < channels; channel++)
            {
                var offset = ((frame * channels) + channel) * bytesPerSample;
                sum += ReadSample(buffer, offset, format);
            }

            _mono[frame] = (float)(sum / channels);
        }

        return frames;
    }

    /// <summary>One sample, whatever the device chose to speak.</summary>
    private static double ReadSample(byte[] buffer, int offset, WaveFormat format)
    {
        if (format.Encoding == WaveFormatEncoding.IeeeFloat && format.BitsPerSample == 32)
        {
            return BitConverter.ToSingle(buffer, offset);
        }

        return format.BitsPerSample switch
        {
            16 => BitConverter.ToInt16(buffer, offset) / 32768.0,

            // 24-bit is packed three bytes little-endian, sign carried in the
            // top one. Sign-extended by hand because there is no 24-bit type.
            24 => ((buffer[offset] | (buffer[offset + 1] << 8) | ((sbyte)buffer[offset + 2] << 16))
                   / 8388608.0),
            32 => BitConverter.ToInt32(buffer, offset) / 2147483648.0,
            8 => (buffer[offset] - 128) / 128.0,
            _ => 0.0,
        };
    }
}
