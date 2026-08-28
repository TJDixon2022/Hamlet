using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// Overlapping spectra of the received audio, for the digital waterfall.
/// </summary>
/// <remarks>
/// <para>**THIS IS AN FFT OF THE CODEC AUDIO, NOT THE RADIO'S SCOPE** (Tim's
/// ruling of 2026-08-28). The scope stream is band-wide RF in kilohertz and FT8
/// occupies a fifty hertz sliver of a three kilohertz audio passband, so a
/// picture built from it would show nothing useful and the slot grid would have
/// nothing to align to.</para>
/// <para>**AND IT IS NOT THE DECODER'S OWN INTERNAL ARRAY**, for the reason that
/// ruling gives: tying the picture to the decoder means the waterfall goes dark
/// exactly when the decoder is silent, and that is the case most worth
/// seeing.</para>
/// <para>**IT SUBSCRIBES; IT DOES NOT OPEN A SECOND CAPTURE.**
/// <see cref="IAudioSource.SamplesReady"/> is an ordinary multicast event, so
/// this rides alongside the CW decoder without either knowing about the other
/// and without the audio path being restructured.</para>
/// <para>**PURE BELOW THE PUMP** (§5.4). Samples and a timestamp go in, frames
/// come out; nothing here reads a clock, so a fixture produces the same frames
/// on every run.</para>
/// </remarks>
public sealed class AudioSpectrumSource : ISpectrumSource, IDisposable
{
    /// <summary>The lowest frequency the waterfall shows.</summary>
    /// <remarks>
    /// Two hundred hertz, from the order. Below it is where a receiver's own
    /// rumble lives and no digital mode sits there.
    /// </remarks>
    public const int LowHz = 200;

    /// <summary>The highest frequency the waterfall shows.</summary>
    /// <remarks>Three thousand, which is the top of an SSB passband.</remarks>
    public const int HighHz = 3000;

    /// <summary>Samples per transform window.</summary>
    /// <remarks>
    /// <para>**CHOSEN FROM FT8'S TONE SPACING AND STATED RATHER THAN ASSUMED.**
    /// FT8 places its eight tones 6.25 hertz apart, which is the reciprocal of
    /// its own 0.16 second symbol. To see them as separate stripes the bin width
    /// has to be meaningfully finer than that spacing.</para>
    /// <para>At 48 kHz a 16384-sample window gives a bin every **2.93 Hz** and
    /// covers **0.341 s** of audio. That is a little over two bins per FT8 tone,
    /// which resolves them, and about two symbols of smear in time, which is
    /// what buys the frequency resolution. **A wider window would resolve better
    /// and blur the fifteen-second slot edges the grid exists to show**, and
    /// those edges are what make FT8 recognisable at a glance.</para>
    /// <para>At 8 kHz the same window is 2.05 s, far too long. The window is
    /// therefore chosen from the sample rate in <see cref="WindowFor"/> rather
    /// than fixed, so a fixture at 8 kHz and a radio at 48 kHz both produce a
    /// sensible picture.</para>
    /// </remarks>
    public const int WindowAt48K = 16384;

    /// <summary>How much of a window is fresh audio.</summary>
    /// <remarks>
    /// A quarter, so consecutive frames overlap by three quarters. Overlap is
    /// what makes a waterfall read as continuous rather than as a flip-book, and
    /// a quarter-hop at 48 kHz is a new row about every 85 milliseconds.
    /// </remarks>
    public const int HopDivisor = 4;

    private readonly object _gate = new();
    private readonly int _sampleRate;
    private readonly RealFft _fft;
    private readonly float[] _ring;
    private readonly float[] _window;
    private readonly double[] _taper;
    private readonly double[] _magnitudes;
    private readonly double[] _real;
    private readonly double[] _imaginary;
    private readonly byte[] _bins;
    private readonly double[] _scratch;
    private readonly int _hop;
    private readonly int _firstBin;
    private readonly int _lastBin;

    private IAudioSource? _attached;
    private int _fill;
    private int _sinceHop;
    private long _samplesSeen;
    private double _floorDb = -90;

    /// <summary>Creates a spectrum source over one sample rate.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="simulated">
    /// Whether the audio behind it is synthesised (HM-DEC-026).
    /// </param>
    public AudioSpectrumSource(int sampleRate, bool simulated = false)
    {
        _sampleRate = Math.Max(1000, sampleRate);
        IsSimulated = simulated;

        var size = WindowFor(_sampleRate);
        _fft = new RealFft(size);
        _hop = size / HopDivisor;

        _ring = new float[size];
        _window = new float[size];
        _taper = new double[size];
        _magnitudes = new double[_fft.BinCount];
        _real = new double[size];
        _imaginary = new double[size];

        // A Hann taper, so a tone that does not land exactly on a bin does not
        // smear across the whole picture.
        for (var i = 0; i < size; i++)
        {
            _taper[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (size - 1)));
        }

        _firstBin = (int)Math.Floor(LowHz * (double)size / _sampleRate);
        _lastBin = Math.Min(
            _fft.BinCount - 1,
            (int)Math.Ceiling(HighHz * (double)size / _sampleRate));

        _bins = new byte[Math.Max(1, _lastBin - _firstBin + 1)];
        _scratch = new double[_bins.Length];
    }

    /// <summary>The window size that suits a sample rate.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>A power of two.</returns>
    /// <remarks>
    /// **SCALED SO EVERY RATE SEES THE SAME SPAN OF TIME**, about a third of a
    /// second, which is what fixes the bin width near three hertz whatever the
    /// radio or the fixture runs at.
    /// </remarks>
    public static int WindowFor(int sampleRate)
    {
        var wanted = sampleRate * WindowAt48K / 48000.0;
        var size = 256;

        while (size * 2 <= wanted)
        {
            size *= 2;
        }

        return size;
    }

    /// <summary>How wide one bin is, in hertz.</summary>
    public double BinWidthHz => (double)_sampleRate / _fft.Size;

    /// <summary>How long one window covers, in seconds.</summary>
    public double WindowSeconds => (double)_fft.Size / _sampleRate;

    /// <inheritdoc />
    public bool IsSimulated { get; }

    /// <inheritdoc />
    public bool IsRunning { get; private set; }

    /// <inheritdoc />
    public event SpectrumFrameHandler? FrameReady;

    /// <inheritdoc />
    public void Start() => IsRunning = true;

    /// <inheritdoc />
    public void Stop() => IsRunning = false;

    /// <summary>Ride along with an audio source, or stop riding.</summary>
    /// <param name="source">The source, or null to detach.</param>
    /// <remarks>
    /// **IT SUBSCRIBES AND NEVER STARTS OR STOPS THE SOURCE.** Whoever opened
    /// the audio owns its lifetime; a second consumer that could stop it would
    /// be able to silence the CW decoder from the Digital tab.
    /// </remarks>
    public void Listen(IAudioSource? source)
    {
        if (ReferenceEquals(_attached, source))
        {
            return;
        }

        if (_attached is not null)
        {
            _attached.SamplesReady -= OnSamples;
        }

        _attached = source;

        if (_attached is not null)
        {
            _attached.SamplesReady += OnSamples;
        }
    }

    private void OnSamples(in AudioChunk chunk) => Push(chunk.Samples);

    /// <summary>
    /// Feed samples in and raise a frame whenever a hop has filled.
    /// </summary>
    /// <param name="samples">The newest audio.</param>
    /// <remarks>
    /// **DETERMINISTIC: NO CLOCK IS READ HERE.** A frame's timestamp is derived
    /// from how many samples have been seen, so the same fixture produces the
    /// same frames with the same times on every run (§5.4).
    /// </remarks>
    public void Push(ReadOnlySpan<float> samples)
    {
        if (!IsRunning)
        {
            return;
        }

        foreach (var sample in samples)
        {
            lock (_gate)
            {
                if (_fill < _ring.Length)
                {
                    _ring[_fill++] = sample;
                }
                else
                {
                    Array.Copy(_ring, 1, _ring, 0, _ring.Length - 1);
                    _ring[^1] = sample;
                }

                _samplesSeen++;
                _sinceHop++;

                if (_fill < _ring.Length || _sinceHop < _hop)
                {
                    continue;
                }

                _sinceHop = 0;
            }

            Emit();
        }
    }

    private void Emit()
    {
        var handler = FrameReady;

        if (handler is null)
        {
            return;
        }

        lock (_gate)
        {
            for (var i = 0; i < _ring.Length; i++)
            {
                _window[i] = (float)(_ring[i] * _taper[i]);
            }
        }

        _fft.Magnitudes(_window, _magnitudes, _real, _imaginary);

        // **THE PICTURE IS IN DECIBELS AGAINST A FLOOR THAT FOLLOWS THE BAND.**
        // A fixed scale makes a quiet band black and a loud one solid white; the
        // floor tracks the quietest quarter of the visible span so the picture
        // stays readable as conditions move, which is what every waterfall does
        // and what makes one legible at all.
        var span = _lastBin - _firstBin + 1;

        for (var i = 0; i < span; i++)
        {
            _scratch[i] = ToDb(_magnitudes[_firstBin + i]);
        }

        // **THE FLOOR IS A LOW PERCENTILE, NOT THE MINIMUM, AND THAT IS NOT A
        // REFINEMENT.** Taken as the minimum it tracks whichever bin happened to
        // cancel to nearly nothing, which on a clean tone is numerical zero
        // around minus two hundred and forty decibels: the whole picture then
        // saturates white and the loudest bin is indistinguishable from the
        // quietest. Measured on a generated tone, that is exactly what happened.
        //
        // The twenty-fifth percentile of the visible span is the band between
        // the signals, which is what a floor is meant to be.
        Array.Sort(_scratch, 0, span);
        var quietest = _scratch[span / 4];

        _floorDb = (_floorDb * 0.9) + (quietest * 0.1);

        const double Range = 45;

        for (var i = 0; i < span; i++)
        {
            var db = ToDb(_magnitudes[_firstBin + i]);
            var above = (db - _floorDb) / Range;

            _bins[i] = (byte)Math.Clamp(above * 255, 0, 255);
        }

        // Derived from the sample count so a replay is identical to a live run.
        var at = DateTime.UnixEpoch.AddSeconds((double)_samplesSeen / _sampleRate);

        handler(new SpectrumFrame(
            (long)Math.Round(_firstBin * BinWidthHz),
            (long)Math.Round((_lastBin + 1) * BinWidthHz),
            at,
            _bins));
    }

    private static double ToDb(double magnitude)
        => 20 * Math.Log10(Math.Max(magnitude, 1e-12));

    /// <summary>Stop riding along.</summary>
    public void Dispose() => Listen(null);
}
