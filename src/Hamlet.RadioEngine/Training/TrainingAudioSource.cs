using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Training;

/// <summary>
/// The training radio's receive audio: real Morse at a known speed, over the
/// same seam the IC-7300's USB codec will use.
/// </summary>
/// <remarks>
/// <para>The most useful test rig the decoder could have, and a product
/// feature besides (HM-DEC-026). The text and the speed are known exactly, so
/// what comes out of the decoder can be compared against what went in, with no
/// hardware, no antenna, and no waiting for a real station to oblige.</para>
/// <para>The message is rendered once and then looped, which keeps memory flat
/// and makes the stream endless without a generator running per sample. The
/// loop point falls inside the silence after the message, so a decoder meets a
/// clean word gap there rather than a splice.</para>
/// <para><see cref="PumpOnce"/> is how tests drive it: hand over a chosen
/// number of samples and assert on what the decoder made of them, with no
/// timer and no sleeping (§5).</para>
/// </remarks>
public sealed class TrainingAudioSource : IAudioSource
{
    /// <summary>Silence between repetitions, in seconds.</summary>
    private const double GapSeconds = 2.0;

    /// <summary>How often the timer hands over samples.</summary>
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(50);

    private readonly float[] _loop;
    private readonly object _gate = new();

    private Timer? _timer;
    private long _delivered;
    private bool _disposed;

    /// <summary>Creates a source sending a message on repeat.</summary>
    /// <param name="text">What to send; empty falls back to a CQ call.</param>
    /// <param name="wordsPerMinute">Sending speed.</param>
    /// <param name="toneHz">Pitch of the received note.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="noiseAmplitude">Noise to mix in; zero is a clean signal.</param>
    public TrainingAudioSource(
        string? text = null,
        int wordsPerMinute = 12,
        double toneHz = CwSignal.DefaultToneHz,
        int sampleRate = CwSignal.DefaultSampleRate,
        double noiseAmplitude = 0)
    {
        Text = string.IsNullOrWhiteSpace(text) ? MorseCode.CqCall("W1AW") : text.Trim();
        WordsPerMinute = Math.Max(1, wordsPerMinute);
        ToneHz = toneHz;
        SampleRate = Math.Max(1_000, sampleRate);

        _loop = CwSignal.Generate(new CwSignalRequest(
            Text,
            WordsPerMinute,
            ToneHz,
            SampleRate,
            NoiseAmplitude: noiseAmplitude,
            LeadInSeconds: GapSeconds / 2,
            TailSeconds: GapSeconds / 2)).Samples;
    }

    /// <summary>What is being sent.</summary>
    public string Text { get; }

    /// <summary>The speed it is being sent at.</summary>
    public int WordsPerMinute { get; }

    /// <summary>The pitch of the note.</summary>
    public double ToneHz { get; }

    /// <inheritdoc/>
    public string DeviceName => "Training radio";

    /// <inheritdoc/>
    public int SampleRate { get; }

    /// <inheritdoc/>
    /// <remarks>Always true, with no setter. See HM-DEC-026.</remarks>
    public bool IsSimulated => true;

    /// <inheritdoc/>
    public bool IsRunning => _timer is not null;

    /// <inheritdoc/>
    public event AudioChunkHandler? SamplesReady;

    /// <inheritdoc/>
    public void Start()
    {
        lock (_gate)
        {
            if (_disposed || _timer is not null)
            {
                return;
            }

            _timer = new Timer(_ => Tick(), null, TimeSpan.Zero, TickInterval);
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        lock (_gate)
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    /// <summary>
    /// Hand over a chosen number of samples immediately.
    /// </summary>
    /// <param name="sampleCount">How many samples to deliver.</param>
    /// <remarks>
    /// The seam that makes the whole path testable at speed: a test asks for
    /// thirty seconds of audio and gets it in a millisecond, and because
    /// nothing downstream reads a clock the decode is identical to the one
    /// thirty real seconds would have produced.
    /// </remarks>
    public void PumpOnce(int sampleCount)
    {
        var handler = SamplesReady;
        if (handler is null || sampleCount <= 0 || _loop.Length == 0)
        {
            return;
        }

        lock (_gate)
        {
            var remaining = sampleCount;

            while (remaining > 0)
            {
                var offset = (int)(_delivered % _loop.Length);
                var run = Math.Min(remaining, _loop.Length - offset);
                var chunk = new AudioChunk(
                    _delivered, SampleRate, _loop.AsSpan(offset, run));

                handler(in chunk);

                _delivered += run;
                remaining -= run;
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
        }

        Stop();
    }

    private void Tick()
    {
        try
        {
            PumpOnce((int)(SampleRate * TickInterval.TotalSeconds));
        }
        catch (Exception)
        {
            // A render fault must not take down a timer thread with nobody to
            // catch it (§8). A dropped chunk is a dropped chunk.
        }
    }
}
