using System.Diagnostics;
using System.Runtime.InteropServices;
using Hamlet.RadioEngine.Transmit;
using NAudio.CoreAudioApi;
using NAudio.Dmo;
using NAudio.Wave;

namespace Hamlet.RadioEngine.Audio;

/// <summary>One render endpoint, as the machine describes it.</summary>
/// <param name="Id">The endpoint id, which is what a caller should hold on to.</param>
/// <param name="Name">The friendly name, which is what a person recognises.</param>
/// <param name="IsDefault">True for the <c>Role.Console</c> default.</param>
/// <param name="SampleRate">The mix format's rate, in samples per second.</param>
/// <param name="Channels">How many channels the mix format carries.</param>
/// <param name="BitsPerSample">The mix format's depth.</param>
/// <param name="Encoding">The tag and, where there is one, the subformat.</param>
/// <remarks>
/// **DELIBERATELY NOT <see cref="AudioDevice"/>** (work instruction 256, task 1).
/// Every existing caller of <see cref="IAudioDevices"/> is asking *what can this
/// listen to*, and the answer is fed to <see cref="WasapiAudioSource"/>, which
/// opens it for capture. **A render endpoint in that list is a device the caller
/// cannot open**, and <c>AudioDevice</c> carries no field that would let it tell
/// the two apart.
/// </remarks>
public sealed record RenderEndpoint(
    string Id,
    string Name,
    bool IsDefault,
    int SampleRate,
    int Channels,
    int BitsPerSample,
    string Encoding);

/// <summary>
/// Plays a transmission's samples out of a named render endpoint.
/// </summary>
/// <remarks>
/// <para>**IT PLAYS SOUND AND IT KEYS NOTHING.** No serial port, no rig, no
/// transmit-and-receive switching, no control protocol of any kind. It is handed
/// an array of floats and a rate and it puts them into a sound card, which is the
/// whole of what it does. What holds this and the keying together is
/// <see cref="Ft8TransmitSequence"/>, where coming back out of transmit is
/// guaranteed.</para>
/// <para>**THE CALLER NAMES THE DEVICE AND THERE IS NO FALLBACK.** The
/// constructor is given an endpoint id or an exact friendly name and it either
/// opens that endpoint or throws. **It never plays to whatever the machine
/// happens to default to**: a transmission arriving at the wrong endpoint is the
/// software shape of a transmission going out on the wrong band, and it would
/// look, from every number this class reports, exactly like a success.</para>
/// <para>**SHARED MODE, AT THE ENDPOINT'S OWN MIX FORMAT, AND THE RATE IS NOT
/// QUIETLY CHANGED.** Exclusive mode takes an endpoint away from everything else
/// on the machine and refuses outright if anything already holds it, which is not
/// how a station application should behave on somebody's desktop. But shared mode
/// has a trap in it: hand WASAPI a format that is not the mix format and Windows
/// will silently resample on the way through, so a sink written the obvious way
/// reports *I asked for 12000 and it worked* while something else entirely
/// decided what the samples became. **So this initialises at the endpoint's own
/// mix format, publishes that rate as <see cref="EndpointSampleRate"/>, and
/// refuses a mismatched rate loudly rather than accepting one and letting a
/// resampler nobody chose sit in the middle of a transmit path.**</para>
/// <para>**THE CONVERSION IS HERE, AND SO IS THE CLIPPING.** Samples arrive in
/// -1 to +1 and the endpoint speaks 32-bit float or 16-bit integer, at some
/// number of channels. The peak actually written is
/// <see cref="PeakWritten"/> and anything that had to be clamped is counted in
/// <see cref="ClippedSamples"/>, because a conversion that clips silently is
/// indistinguishable from one that did not (section 0.0).</para>
/// <para>**AND IT WAITS FOR THE CARD TO EMPTY BEFORE IT SAYS IT IS DONE.** The
/// tempting shape - write the last buffer, return
/// <c>SamplesPlayed = samples.Length</c> - passes every naive test and is a lie:
/// the samples are still inside the device when the caller is told they have gone
/// out, and whatever the caller does next it does while audio is still being
/// played. So the write loop is followed by a drain, and what comes back is what
/// the endpoint had actually consumed.</para>
/// </remarks>
public sealed class WasapiTransmitSink : ITransmitAudioSink, ITransmitLevelReport, IDisposable
{
    /// <summary>How much buffer to ask the endpoint for, in milliseconds.</summary>
    /// <remarks>
    /// **Deep enough that a managed wait cannot starve it.** The write loop wakes
    /// on a timer whose real granularity is around fifteen milliseconds, so a
    /// hundred-millisecond buffer would be running at the edge; two hundred leaves
    /// an order of magnitude. It is stated here rather than defaulted for the
    /// reason <see cref="WasapiAudioSource.BufferMilliseconds"/> is: a budget
    /// nobody wrote down changes under a package upgrade.
    /// </remarks>
    public const int DefaultBufferMilliseconds = 200;

    /// <summary>How long to sleep when there is no room and nothing to do.</summary>
    private const int WaitMilliseconds = 5;

    private readonly MMDeviceEnumerator _enumerator;
    private readonly MMDevice _device;
    private readonly AudioClient _client;
    private readonly AudioRenderClient _render;
    private readonly SampleKind _kind;
    private readonly object _gate = new();

    private float[] _floatScratch = [];
    private byte[] _byteScratch = [];
    private bool _disposed;

    /// <summary>Opens one named render endpoint, or refuses.</summary>
    /// <param name="device">The endpoint id, or its exact friendly name.</param>
    /// <param name="bufferMilliseconds">How much buffer to ask for.</param>
    /// <exception cref="InvalidOperationException">
    /// There is no active render endpoint by that id or name, or the one there is
    /// speaks something this cannot write.
    /// </exception>
    public WasapiTransmitSink(string device, int bufferMilliseconds = DefaultBufferMilliseconds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(device);

        Requested = device;
        _enumerator = new MMDeviceEnumerator();

        try
        {
            _device = Find(_enumerator, device);
        }
        catch
        {
            _enumerator.Dispose();
            throw;
        }

        try
        {
            DeviceId = _device.ID;
            DeviceName = _device.FriendlyName;

            // **THE CLIENT IS TAKEN ONCE AND KEPT.** Reading the property again
            // activates a second, uninitialised client, and the one that was
            // initialised is thrown away - which fails later, at the first render
            // call, as AUDCLNT_E_NOT_INITIALIZED and nowhere near its cause.
            var client = _device.AudioClient;

            var mix = client.MixFormat;
            EndpointSampleRate = mix.SampleRate;
            EndpointChannels = mix.Channels;
            EndpointBitsPerSample = mix.BitsPerSample;
            EndpointEncoding = Describe(mix);

            _kind = KindOf(mix);

            if (_kind == SampleKind.Unwritable)
            {
                // **REFUSED RATHER THAN GUESSED** (HM-DEC-009). Writing a format
                // this does not understand puts something on the air, and what it
                // would be is not knowable from here.
                throw new InvalidOperationException(
                    $"the render endpoint speaks {EndpointEncoding}, which this conversion "
                    + "cannot write. Nothing is played rather than a format being invented.");
            }

            client.Initialize(
                AudioClientShareMode.Shared,
                AudioClientStreamFlags.None,
                bufferMilliseconds * 10_000L,
                0,
                mix,
                Guid.Empty);

            _client = client;
            _render = _client.AudioRenderClient;
            BufferFrames = _client.BufferSize;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _device.Dispose();
            _enumerator.Dispose();

            throw new InvalidOperationException(
                $"the render endpoint '{device}' would not open: {ex.Message}", ex);
        }
        catch
        {
            _device.Dispose();
            _enumerator.Dispose();
            throw;
        }
    }

    /// <summary>What the caller asked for, exactly as it asked.</summary>
    public string Requested { get; }

    /// <summary>The endpoint id actually opened.</summary>
    public string DeviceId { get; } = string.Empty;

    /// <summary>The friendly name of the endpoint actually opened.</summary>
    public string DeviceName { get; } = string.Empty;

    /// <inheritdoc/>
    /// <remarks>
    /// The rate the endpoint declared, which is the rate it will get. **It was
    /// already here before it was on the interface** (work instruction 262); what
    /// changed is that a caller can now read it.
    /// </remarks>
    public int EndpointSampleRate { get; }

    /// <summary>How many channels the endpoint declared.</summary>
    public int EndpointChannels { get; }

    /// <summary>The endpoint's declared depth.</summary>
    public int EndpointBitsPerSample { get; }

    /// <summary>The endpoint's declared tag, and its subformat where it has one.</summary>
    public string EndpointEncoding { get; } = string.Empty;

    /// <summary>The share mode in force, in a word.</summary>
    public string ShareMode => "shared";

    /// <summary>How many frames of buffer the endpoint gave, having been asked.</summary>
    public int BufferFrames { get; }

    /// <summary>The rate the last call to <see cref="PlayAsync"/> asked for.</summary>
    /// <remarks>Zero until something has been played.</remarks>
    public int RateAsked { get; private set; }

    /// <summary>The rate that was played at, which is the endpoint's own.</summary>
    public int RateGot => EndpointSampleRate;

    /// <inheritdoc/>
    /// <remarks>
    /// Measured on the way out, after clamping, so it is what the device was
    /// handed rather than what the caller supplied.
    ///
    /// **IT IS ON <see cref="ITransmitLevelReport"/> SINCE UNIT 269** so the
    /// application can read it off the sink it built, after the boundary has
    /// returned, without the keying path or <see cref="ITransmitAudioSink"/>
    /// changing. The property itself is untouched.
    /// </remarks>
    public double PeakWritten { get; private set; }

    /// <summary>The root-mean-square of what was actually written.</summary>
    public double RmsWritten { get; private set; }

    /// <inheritdoc/>
    /// <remarks>
    /// **Counted, never rounded away.** Zero is the expected answer for anything
    /// this repository composes, and a number other than zero is a finding.
    /// </remarks>
    public long ClippedSamples { get; private set; }

    /// <summary>Every active render endpoint the machine has.</summary>
    /// <returns>What is there, or an empty list.</returns>
    /// <remarks>
    /// **NEVER THROWS**, for the same reason
    /// <see cref="WasapiAudioDevices.List"/> does not: a machine with no sound
    /// card is a normal machine and the rest of the application works perfectly
    /// well on one (section 8).
    /// </remarks>
    public static IReadOnlyList<RenderEndpoint> Endpoints()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();

            string? defaultId = null;

            try
            {
                using var console = enumerator.GetDefaultAudioEndpoint(
                    DataFlow.Render, Role.Console);
                defaultId = console.ID;
            }
            catch (Exception)
            {
                // A machine with no default render endpoint is a normal machine.
            }

            var found = new List<RenderEndpoint>();

            foreach (var device in enumerator.EnumerateAudioEndPoints(
                DataFlow.Render, DeviceState.Active))
            {
                using (device)
                {
                    var format = device.AudioClient.MixFormat;

                    found.Add(new RenderEndpoint(
                        device.ID,
                        device.FriendlyName,
                        string.Equals(device.ID, defaultId, StringComparison.Ordinal),
                        format.SampleRate,
                        format.Channels,
                        format.BitsPerSample,
                        Describe(format)));
                }
            }

            return found;
        }
        catch (Exception)
        {
            return [];
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// The rate asked for is not the rate the endpoint speaks.
    /// </exception>
    public async Task<PlayedAudio> PlayAsync(
        ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        RateAsked = sampleRate;

        if (sampleRate != EndpointSampleRate)
        {
            // **NOT RESAMPLED HERE AND NOT HANDED OVER AS THOUGH IT MATCHED.**
            // Either would put a rate conversion nobody chose inside a transmit
            // path, and the caller would have no way to know it happened.
            throw new InvalidOperationException(
                $"the samples are at {sampleRate} Hz and the endpoint speaks "
                + $"{EndpointSampleRate} Hz. Nothing is played rather than a rate being "
                + "silently changed on the way out.");
        }

        var total = samples.Length;

        if (total == 0)
        {
            return new PlayedAudio(0, TimeSpan.Zero);
        }

        var clock = Stopwatch.StartNew();
        var written = 0;
        var peak = 0.0;
        var squares = 0.0;
        var clipped = 0L;

        lock (_gate)
        {
            _client.Reset();
        }

        _client.Start();

        try
        {
            while (written < total && !cancellationToken.IsCancellationRequested)
            {
                var free = BufferFrames - _client.CurrentPadding;

                if (free <= 0)
                {
                    // CancellationToken.None on the wait itself: cancelling is a
                    // reason to stop the loop, not a reason to throw out of a
                    // method whose whole contract is to say how much went out.
                    await Task.Delay(WaitMilliseconds, CancellationToken.None)
                        .ConfigureAwait(false);
                    continue;
                }

                var take = Math.Min(free, total - written);
                var buffer = _render.GetBuffer(take);

                Convert(samples.Span.Slice(written, take), buffer, take,
                    ref peak, ref squares, ref clipped);

                _render.ReleaseBuffer(take, AudioClientBufferFlags.None);
                written += take;
            }

            // **THE DRAIN, AND IT WAS WATCHED TO FAIL WITHOUT IT** (unit 256,
            // task 2). Everything above put samples into the endpoint; none of it
            // waited for the endpoint to take them out again. Built without this,
            // the sink reported 96,000 of 96,000 samples played in 1.810 s of a
            // 2.000 s tone - 190 ms of audio still inside the card at the moment
            // the caller was told the transmission had gone out.
            var deadline = Stopwatch.StartNew();
            var bound = TimeSpan.FromMilliseconds((BufferFrames * 4000.0) / EndpointSampleRate);

            while (!cancellationToken.IsCancellationRequested
                && _client.CurrentPadding > 0
                && deadline.Elapsed < bound)
            {
                await Task.Delay(WaitMilliseconds, CancellationToken.None).ConfigureAwait(false);
            }
        }
        finally
        {
            // What is still inside the endpoint never went out, and is subtracted
            // rather than counted. On a clean run this is zero.
            var stranded = 0;

            try
            {
                stranded = _client.CurrentPadding;
            }
            catch (Exception)
            {
                // An endpoint that will not say how full it is has not said that
                // it is empty, so nothing is assumed about the tail.
                stranded = 0;
            }

            written = Math.Max(0, written - stranded);

            lock (_gate)
            {
                _client.Stop();
                _client.Reset();
            }

            clock.Stop();
        }

        PeakWritten = peak;
        ClippedSamples = clipped;
        RmsWritten = written > 0 ? Math.Sqrt(squares / (written * (double)EndpointChannels)) : 0.0;

        return new PlayedAudio(written, clock.Elapsed);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            _client.Stop();
        }
        catch (Exception)
        {
            // Tearing down a sound device is best-effort (section 8).
        }

        _device.Dispose();
        _enumerator.Dispose();
    }

    /// <summary>The endpoint the caller named, or a refusal saying so.</summary>
    /// <param name="enumerator">The machine's endpoints.</param>
    /// <param name="wanted">An endpoint id, or an exact friendly name.</param>
    /// <returns>The endpoint.</returns>
    /// <exception cref="InvalidOperationException">There is no such endpoint.</exception>
    /// <remarks>
    /// **THE FAILURE IS LOUD AND THERE IS NO SECOND CHOICE.** Matching is ordinal
    /// and exact on either the id or the friendly name; a near miss is a miss.
    /// The refusal names what was asked for - which the caller supplied and
    /// therefore already knows - and how many endpoints there were, and it names
    /// no other endpoint (HM-DEC-018).
    /// </remarks>
    private static MMDevice Find(MMDeviceEnumerator enumerator, string wanted)
    {
        MMDevice? found = null;
        var seen = 0;

        foreach (var device in enumerator.EnumerateAudioEndPoints(
            DataFlow.Render, DeviceState.Active))
        {
            seen++;

            if (found is null
                && (string.Equals(device.ID, wanted, StringComparison.Ordinal)
                    || string.Equals(device.FriendlyName, wanted, StringComparison.Ordinal)))
            {
                found = device;
                continue;
            }

            device.Dispose();
        }

        return found ?? throw new InvalidOperationException(
            $"there is no active render endpoint called '{wanted}'. {seen} were offered and "
            + "none of them matched. Nothing is played rather than something else being "
            + "played instead.");
    }

    /// <summary>What a format is, in one line.</summary>
    private static string Describe(WaveFormat format)
    {
        var subformat = format is WaveFormatExtensible extensible
            ? $" subformat {extensible.SubFormat}"
            : string.Empty;

        return $"{format.Encoding} {format.BitsPerSample}-bit{subformat}";
    }

    /// <summary>What the bytes an endpoint wants really are.</summary>
    private enum SampleKind
    {
        /// <summary>Nothing here can write them.</summary>
        Unwritable,

        /// <summary>IEEE-754 single precision.</summary>
        Float32,

        /// <summary>Signed 16-bit integers.</summary>
        Pcm16,
    }

    /// <summary>Which of those a format is.</summary>
    /// <param name="format">What the endpoint declared.</param>
    /// <returns>The kind, or <see cref="SampleKind.Unwritable"/>.</returns>
    /// <remarks>
    /// **THE TOP-LEVEL TAG IS NOT THE ANSWER**, which is the same lesson
    /// <see cref="WasapiAudioSource"/> learned on the way in and paid for with a
    /// silent radio in unit 237. Every render endpoint on the development machine
    /// this was written on declares <c>Extensible</c> with the IEEE float
    /// subformat, so a check that asked only for <c>IeeeFloat</c> would refuse
    /// every one of them.
    /// </remarks>
    private static SampleKind KindOf(WaveFormat format)
    {
        if (format.Encoding == WaveFormatEncoding.Extensible)
        {
            if (format is not WaveFormatExtensible extensible)
            {
                return SampleKind.Unwritable;
            }

            if (extensible.SubFormat == AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT)
            {
                return format.BitsPerSample == 32 ? SampleKind.Float32 : SampleKind.Unwritable;
            }

            if (extensible.SubFormat == AudioMediaSubtypes.MEDIASUBTYPE_PCM)
            {
                return format.BitsPerSample == 16 ? SampleKind.Pcm16 : SampleKind.Unwritable;
            }

            return SampleKind.Unwritable;
        }

        return format.Encoding switch
        {
            WaveFormatEncoding.IeeeFloat when format.BitsPerSample == 32 => SampleKind.Float32,
            WaveFormatEncoding.Pcm when format.BitsPerSample == 16 => SampleKind.Pcm16,
            _ => SampleKind.Unwritable,
        };
    }

    /// <summary>
    /// Mono floats in -1 to +1, into whatever the endpoint speaks.
    /// </summary>
    /// <param name="mono">The samples for these frames.</param>
    /// <param name="destination">The endpoint's own buffer.</param>
    /// <param name="frames">How many frames that is.</param>
    /// <param name="peak">The largest magnitude written so far, updated.</param>
    /// <param name="squares">The running sum of squares, updated.</param>
    /// <param name="clipped">How many samples were clamped, updated.</param>
    /// <remarks>
    /// <para>**THE SAME SAMPLE GOES TO EVERY CHANNEL.** One transmission is one
    /// signal, and a stereo endpoint is two copies of it rather than a signal and
    /// a silence - a caller downstream that takes only one channel, or that mixes
    /// them, gets the transmission either way.</para>
    /// <para>**CLAMPED, AND THE CLAMP IS COUNTED.** Anything outside -1 to +1 is
    /// brought back to the rail, and every sample that had to be is counted, so a
    /// conversion that clipped can never be reported as one that did not
    /// (section 0.0).</para>
    /// </remarks>
    private void Convert(
        ReadOnlySpan<float> mono,
        IntPtr destination,
        int frames,
        ref double peak,
        ref double squares,
        ref long clipped)
    {
        var channels = EndpointChannels;
        var values = frames * channels;

        if (_kind == SampleKind.Float32)
        {
            if (_floatScratch.Length < values)
            {
                _floatScratch = new float[values];
            }

            for (var frame = 0; frame < frames; frame++)
            {
                var sample = Clamp(mono[frame], ref clipped);
                var magnitude = Math.Abs((double)sample);

                if (magnitude > peak)
                {
                    peak = magnitude;
                }

                for (var channel = 0; channel < channels; channel++)
                {
                    _floatScratch[(frame * channels) + channel] = sample;
                    squares += magnitude * magnitude;
                }
            }

            Marshal.Copy(_floatScratch, 0, destination, values);

            return;
        }

        var bytes = values * 2;

        if (_byteScratch.Length < bytes)
        {
            _byteScratch = new byte[bytes];
        }

        for (var frame = 0; frame < frames; frame++)
        {
            var sample = Clamp(mono[frame], ref clipped);
            var magnitude = Math.Abs((double)sample);

            if (magnitude > peak)
            {
                peak = magnitude;
            }

            // 32767 rather than 32768: the positive rail of a signed 16-bit word
            // is one short of the negative one, and scaling by the larger number
            // wraps a full-scale positive sample to the bottom of the range.
            var word = (short)Math.Round(sample * 32767.0);

            for (var channel = 0; channel < channels; channel++)
            {
                var offset = (((frame * channels) + channel)) * 2;
                _byteScratch[offset] = (byte)(word & 0xFF);
                _byteScratch[offset + 1] = (byte)((word >> 8) & 0xFF);
                squares += magnitude * magnitude;
            }
        }

        Marshal.Copy(_byteScratch, 0, destination, bytes);
    }

    /// <summary>One sample, brought inside the rails, with the clamp counted.</summary>
    private static float Clamp(float sample, ref long clipped)
    {
        if (float.IsNaN(sample))
        {
            // A sample that is not a number is not a quiet sample. It is counted
            // as a clip and written as silence rather than handed to a converter
            // whose behaviour on it is undefined.
            clipped++;

            return 0.0f;
        }

        if (sample > 1.0f)
        {
            clipped++;

            return 1.0f;
        }

        if (sample < -1.0f)
        {
            clipped++;

            return -1.0f;
        }

        return sample;
    }
}
