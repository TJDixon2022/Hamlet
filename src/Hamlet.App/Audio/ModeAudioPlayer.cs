using Hamlet.RadioEngine.Training;
using NAudio.Wave;

namespace Hamlet.App.Audio;

/// <summary>
/// Plays the field guide's generated samples to the computer's speakers.
/// </summary>
/// <remarks>
/// <para>The only part of "hear it" that has to live in the shell. The audio
/// itself is generated in the engine, which knows nothing about devices or
/// UI (§0.1); this wraps it in NAudio and sends it to the default output —
/// the speakers, deliberately not the radio. Nothing here transmits, and
/// nothing here can (§0.2).</para>
/// <para>Generation runs off the UI thread. A six-second SSB sample is a few
/// million trigonometric evaluations, which is fast but not free, and a field
/// guide that freezes when you press "hear it" would be teaching patience
/// rather than radio.</para>
/// </remarks>
public sealed class ModeAudioPlayer : IDisposable
{
    private readonly object _gate = new();
    private IWavePlayer? _device;
    private CancellationTokenSource? _pending;
    private bool _disposed;

    /// <summary>Raised on the thread that changed it when playback starts or stops.</summary>
    public event EventHandler? StateChanged;

    /// <summary>True while a sample is playing.</summary>
    public bool IsPlaying
    {
        get
        {
            lock (_gate)
            {
                return _device?.PlaybackState == PlaybackState.Playing;
            }
        }
    }

    /// <summary>What is playing now, or null.</summary>
    public AudioSampleRequest? Current { get; private set; }

    /// <summary>
    /// Generate and play a sample, replacing whatever was playing.
    /// </summary>
    /// <param name="request">Which sample to play.</param>
    /// <returns>A task that completes once playback has started.</returns>
    /// <remarks>
    /// Playing a second sample stops the first: two modes at once teaches
    /// nothing, and the operator pressing another button plainly means "that
    /// one instead".
    /// </remarks>
    public async Task PlayAsync(AudioSampleRequest request)
    {
        if (_disposed)
        {
            return;
        }

        Stop();

        CancellationTokenSource cts;
        lock (_gate)
        {
            _pending?.Cancel();
            _pending = cts = new CancellationTokenSource();
        }

        float[] samples;
        try
        {
            samples = await Task.Run(() => ModeAudio.Generate(request), cts.Token)
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (cts.IsCancellationRequested || _disposed)
        {
            return;
        }

        try
        {
            lock (_gate)
            {
                var provider = new FloatArraySampleProvider(samples, ModeAudio.SampleRate);
                var device = new WaveOutEvent { DesiredLatency = 120 };
                device.Init(provider);
                device.PlaybackStopped += OnPlaybackStopped;
                device.Play();

                _device = device;
                Current = request;
            }

            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception)
        {
            // No sound device, or one that refuses: the field guide still
            // works, it just cannot demonstrate. Never a crash (§8).
            Current = null;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Stop playback. Safe when nothing is playing.</summary>
    public void Stop()
    {
        IWavePlayer? device;

        lock (_gate)
        {
            _pending?.Cancel();
            device = _device;
            _device = null;
            Current = null;
        }

        if (device is null)
        {
            return;
        }

        try
        {
            device.PlaybackStopped -= OnPlaybackStopped;
            device.Stop();
            device.Dispose();
        }
        catch (Exception)
        {
            // Tearing down an audio device is best-effort.
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposed = true;
        Stop();
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        Current = null;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Feeds a generated buffer to NAudio, once.</summary>
    private sealed class FloatArraySampleProvider : ISampleProvider
    {
        private readonly float[] _samples;
        private int _position;

        public FloatArraySampleProvider(float[] samples, int sampleRate)
        {
            _samples = samples;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
        }

        public WaveFormat WaveFormat { get; }

        public int Read(float[] buffer, int offset, int count)
        {
            var remaining = _samples.Length - _position;
            var take = Math.Min(remaining, count);

            if (take <= 0)
            {
                return 0;
            }

            Array.Copy(_samples, _position, buffer, offset, take);
            _position += take;
            return take;
        }
    }
}
