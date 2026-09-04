using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.Dmo;
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

    /// <summary>The device buffer length asked for, in milliseconds.</summary>
    /// <remarks>
    /// **IT IS WHAT NAUDIO 2.2.1 ALREADY DEFAULTED TO**, read off a real capture
    /// rather than recalled, so writing it down pins the budget without changing
    /// how the card is driven.
    /// </remarks>
    public const int BufferMilliseconds = 100;

    private CallbackBudget? _budget;

    private WasapiCapture? _capture;
    private float[] _mono = Array.Empty<float>();
    private long _delivered;
    private long _callbackFailures;
    private long _emptyBuffers;
    private double _longestCallbackMicroseconds;
    private string _lastCallbackFailure = string.Empty;
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

            // **THE BUFFER LENGTH IS SET HERE RATHER THAN TAKEN**, because it
            // is the budget every callback is measured against and a budget
            // nobody wrote down is a budget that changes under a package
            // upgrade. NAudio 2.2.1 has no property for it - unit 239 task 1
            // went looking and found there is only this constructor parameter,
            // so it cannot be adjusted after the capture is built.
            //
            // **ONE HUNDRED MILLISECONDS IS WHAT THIS ALREADY HAD**, read off a
            // real WasapiCapture in `WhatBufferPeriodIsInForceTests`. Writing it
            // down changes no behavior at all; what it changes is that the
            // number in `BufferPeriodMicroseconds` is now the number in force
            // rather than a reasonable guess about a default.
            //
            // **AND IT IS NOT LOWERED, ON PURPOSE.** A shorter period would mean
            // more callbacks with less work in each, which sounds like an
            // improvement and is a change to how the sound card is driven on a
            // machine whose audio path this unit is still measuring. That is a
            // separate question with its own measurement (§12.6).
            _capture = new WasapiCapture(endpoint, false, BufferMilliseconds);
            _budget = new CallbackBudget(BufferMilliseconds * 1000.0);
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

        var started = Stopwatch.GetTimestamp();

        try
        {
            var frames = Downmix(e.Buffer, e.BytesRecorded, format, ref _mono);
            if (frames <= 0)
            {
                // **A BUFFER THAT DOWNMIXED TO NOTHING IS NOT DELIVERED**, and
                // the census must be able to say so. It is counted here rather
                // than left to look like a device that went quiet.
                _emptyBuffers++;

                return;
            }

            var chunk = new AudioChunk(_delivered, SampleRate, _mono.AsSpan(0, frames));
            _delivered += frames;
            handler(in chunk);
        }
        catch (Exception ex)
        {
            // **A BAD BUFFER MUST NOT TAKE DOWN THE CAPTURE THREAD** (CLAUDE.md
            // section 8), and it must not vanish either. **A dropped chunk that
            // leaves no number is the fault this whole unit is about**: the tap
            // was starved for weeks and nothing counted it (HM-DEC-093). The
            // count and the last type are read by the census, the sidecar and
            // the slot refusal, so a chunk that failed downmix is visibly a
            // chunk that was not delivered rather than a gap the operator has to
            // guess at.
            _callbackFailures++;
            _lastCallbackFailure = ex.GetType().Name;
        }
        finally
        {
            // **THE LONGEST CALLBACK SEEN IS THE WHOLE POINT OF TASK 2.** Before
            // this unit the callback ran a complete CW decode, so its duration
            // was the decode's; the assertion that it now returns inside one
            // buffer duration is only checkable because this is measured.
            var micros = (Stopwatch.GetTimestamp() - started) * 1_000_000.0
                / Stopwatch.Frequency;

            if (micros > _longestCallbackMicroseconds)
            {
                _longestCallbackMicroseconds = micros;
            }

            // **AND WHETHER IT FIT IN THE TIME IT HAD.** The longest callback on
            // its own cannot say that: 91,372 us is a catastrophe against a
            // 20,000 us budget and 91% of a 100,000 us one, and unit 238
            // asserted against the first of those figures without the device
            // ever having had that period (HM-DEC-093).
            _budget?.Record(micros);
        }
    }

    /// <summary>How many callbacks threw, and were counted rather than lost.</summary>
    public long CallbackFailures => _callbackFailures;

    /// <summary>The type name of the most recent callback failure, or "".</summary>
    /// <remarks>
    /// The type rather than the message: a message carries device paths and user
    /// text, and HM-DEC-018 keeps both out of anything that might be recorded.
    /// </remarks>
    public string LastCallbackFailure => _lastCallbackFailure;

    /// <summary>How many device buffers downmixed to no samples at all.</summary>
    public long EmptyBuffers => _emptyBuffers;

    /// <summary>The longest a single callback has taken, in microseconds.</summary>
    public double LongestCallbackMicroseconds => _longestCallbackMicroseconds;

    /// <summary>The device buffer period every callback is measured against.</summary>
    public double BufferPeriodMicroseconds => _budget?.PeriodMicroseconds ?? 0;

    /// <summary>Callbacks that ran longer than the whole buffer period.</summary>
    public long CallbacksOverPeriod => _budget?.OverPeriod ?? 0;

    /// <summary>Callbacks that ran longer than half the buffer period.</summary>
    public long CallbacksOverHalfPeriod => _budget?.OverHalfPeriod ?? 0;

    /// <summary>How many callbacks have been timed.</summary>
    public long CallbacksTimed => _budget?.Measured ?? 0;

    /// <summary>How many samples the device has delivered since it started.</summary>
    public long DeliveredSamples => _delivered;

    /// <summary>
    /// Convert the device's buffer into mono floats.
    /// </summary>
    /// <param name="buffer">The device's buffer.</param>
    /// <param name="byteCount">How much of it the device filled.</param>
    /// <param name="format">What the device says those bytes are.</param>
    /// <param name="mono">
    /// The scratch array to write into, grown in place when it is too small.
    /// </param>
    /// <returns>How many mono samples were written.</returns>
    /// <remarks>
    /// **INTERNAL AND STATIC SO A TEST CAN REACH IT, AND FOR NO OTHER REASON**
    /// (unit 237). The only constructor this class has opens a real capture
    /// device, so an instance method here is unreachable without a sound card and
    /// the conversion between the antenna and the tap went eleven units without a
    /// single test executing it. The scratch array moves from a field to a
    /// <c>ref</c> parameter and nothing else changes: the caller still passes its
    /// own <c>_mono</c>, it is still grown once and reused, and this still
    /// allocates nothing after the first buffer.
    /// </remarks>
    internal static int Downmix(
        byte[] buffer, int byteCount, WaveFormat format, ref float[] mono)
    {
        var channels = Math.Max(1, format.Channels);
        var bytesPerSample = format.BitsPerSample / 8;

        if (bytesPerSample <= 0)
        {
            return 0;
        }

        var frames = byteCount / (bytesPerSample * channels);

        if (mono.Length < frames)
        {
            mono = new float[frames];
        }

        for (var frame = 0; frame < frames; frame++)
        {
            var sum = 0.0;

            for (var channel = 0; channel < channels; channel++)
            {
                var offset = ((frame * channels) + channel) * bytesPerSample;
                sum += ReadSample(buffer, offset, format);
            }

            mono[frame] = (float)(sum / channels);
        }

        return frames;
    }

    /// <summary>One sample, whatever the device chose to speak.</summary>
    /// <param name="buffer">The device's buffer.</param>
    /// <param name="offset">Where in it this sample starts.</param>
    /// <param name="format">What the device says those bytes are.</param>
    /// <returns>The sample, as a fraction of full scale.</returns>
    /// <exception cref="NotSupportedException">
    /// The device speaks something this cannot read.
    /// </exception>
    internal static double ReadSample(byte[] buffer, int offset, WaveFormat format)
    {
        if (Kind(format) == SampleKind.Float)
        {
            return format.BitsPerSample == 32
                ? BitConverter.ToSingle(buffer, offset)
                : throw Unreadable(format);
        }

        return format.BitsPerSample switch
        {
            16 => BitConverter.ToInt16(buffer, offset) / 32768.0,

            // 24-bit is packed three bytes little-endian, sign carried in the
            // top one. Sign-extended by hand because there is no 24-bit type.
            24 => ((buffer[offset] | (buffer[offset + 1] << 8) | ((sbyte)buffer[offset + 2] << 16))
                   / 8388608.0),

            // 32-bit integer, which is also how a 24-in-32 container arrives:
            // those samples are left-aligned in the word, so the same division
            // is right and the low bits are simply zero.
            32 => BitConverter.ToInt32(buffer, offset) / 2147483648.0,
            8 => (buffer[offset] - 128) / 128.0,
            _ => throw Unreadable(format),
        };
    }

    /// <summary>What a device's bytes really are, whatever it calls them.</summary>
    private enum SampleKind
    {
        /// <summary>Nothing here can read them.</summary>
        Unreadable,

        /// <summary>Signed integers, or unsigned at 8 bits.</summary>
        Integer,

        /// <summary>IEEE-754 floating point.</summary>
        Float,
    }

    /// <summary>Which of those a format is.</summary>
    /// <param name="format">What the device declared.</param>
    /// <returns>The kind, or <see cref="SampleKind.Unreadable"/>.</returns>
    /// <remarks>
    /// <para>**THE TOP-LEVEL TAG IS NOT THE ANSWER, AND THIS IS WHY THE RADIO WAS
    /// SILENT** (unit 237). Windows shared-mode capture presents its mix format as
    /// WAVE_FORMAT_EXTENSIBLE, which says what the bytes are in a subformat GUID
    /// and leaves the top-level tag reading <c>Extensible</c>. Until this unit the
    /// float branch here asked only for <c>IeeeFloat</c>, so an extensible float
    /// device fell through to the 32-bit integer arm and every sample the radio
    /// delivered was read as a whole number: loud, structureless, and with every
    /// FT8 tone in it destroyed. Measured, not reasoned - 0.999023438 in and
    /// 0.496086100 out.</para>
    /// <para>**AND AN UNRECOGNISED SUBFORMAT IS UNREADABLE RATHER THAN INTEGER**
    /// (HM-DEC-009). A device speaking something else through an extensible
    /// wrapper would read as integers with the same shape of failure, and
    /// answering a question nobody can answer is what this project's prime
    /// directive forbids.</para>
    /// </remarks>
    private static SampleKind Kind(WaveFormat format)
    {
        if (format.Encoding == WaveFormatEncoding.Extensible)
        {
            // A format that says it is extensible but did not arrive as one
            // cannot be asked what its subformat is, so nothing is known.
            if (format is not WaveFormatExtensible extensible)
            {
                return SampleKind.Unreadable;
            }

            if (extensible.SubFormat == AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT)
            {
                return SampleKind.Float;
            }

            return extensible.SubFormat == AudioMediaSubtypes.MEDIASUBTYPE_PCM
                ? SampleKind.Integer
                : SampleKind.Unreadable;
        }

        return format.Encoding switch
        {
            WaveFormatEncoding.IeeeFloat => SampleKind.Float,
            WaveFormatEncoding.Pcm => SampleKind.Integer,
            _ => SampleKind.Unreadable,
        };
    }

    /// <summary>The refusal a format nothing here can read produces.</summary>
    /// <param name="format">What the device declared.</param>
    /// <returns>The exception to throw.</returns>
    /// <remarks>
    /// **A REFUSAL, BECAUSE THE ALTERNATIVE LOOKS LIKE QUIET AUDIO** (§0.0,
    /// HM-DEC-009). This arm used to return <c>0.0</c>, which put a stream of
    /// silence on the tap that no reading anywhere could tell from a dead band -
    /// and the operator would spend the morning on a gain knob. Throwing means the
    /// buffer is dropped by <see cref="OnDataAvailable"/> and nothing at all
    /// reaches the tap, which unit 236's slot level writes down as *no level at
    /// all* rather than as a quiet one. **The format is named and the device never
    /// is** (HM-DEC-018).
    /// </remarks>
    private static NotSupportedException Unreadable(WaveFormat format) =>
        new($"the capture device is delivering {format.Encoding} "
            + $"{format.BitsPerSample}-bit samples, which this conversion cannot "
            + "read. Nothing is passed on rather than silence being invented.");
}
