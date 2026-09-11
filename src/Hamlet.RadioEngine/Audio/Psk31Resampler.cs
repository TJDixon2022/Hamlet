namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// **Puts live audio on the eight kilohertz grid PSK31 was proved at.**
/// </summary>
/// <remarks>
/// <para>**THE SOUND CARD DECIDES THE RATE AND THE DEMODULATOR DOES NOT GET A VOTE.**
/// This is the same fault <see cref="Ft8Resample"/> exists for, found again on the
/// operator's own evening of 2026-09-11: `psk31_listening_started` recorded
/// `sampleRate: 48000`, because <c>WasapiAudioSource</c> passes whatever the device hands
/// over straight through and the PSK31 path took it. **Every number in
/// <see cref="Psk31.Psk31Demodulator"/> and <see cref="Psk31.Psk31CarrierSearch"/> was
/// chosen and measured at 8 000 Hz** - 256 samples a bit, a 2048-point window at 3.9 Hz a
/// bin, a 32-symbol measure. At 48 kHz a bit is 1 536 samples and the window is 8 192, and
/// on the air a real carrier 62 dB over the floor was found three times in a minute and
/// read **nothing**: 17 carriers, 0 characters.</para>
/// <para>**SO THE RATE IS FIXED HERE AND NOWHERE ELSE.** The demodulator and the search
/// are not taught a second rate; they are fed the one they were proved at. This is the
/// boundary where the device's audio enters the PSK31 path, and it is the only place in
/// the path that knows what the device is doing.</para>
/// <para>**A STREAM, NOT A SLOT, AND THAT IS THE WHOLE DIFFERENCE FROM
/// <see cref="Ft8Resample"/>.** FT8 resamples a fifteen-second slot in one block, so its
/// filter sees every sample it needs. PSK31 has no slots: audio arrives in quarter-second
/// lumps and a demodulator handed a splice reads noise across it. So this holds the tail
/// of the input the filter still needs and carries the output phase from lump to lump -
/// **the answer does not depend on where the caller cuts the stream.**</para>
/// <para>**BAND-LIMITED, NOT NEAREST-SAMPLE.** Dropping five samples in six to get from
/// 48 kHz to 8 kHz folds everything above 4 kHz back down on top of the 200-3000 Hz the
/// mode lives in, and folded noise has only two phases, so it squares as cleanly as BPSK
/// and the search would list it as a station. The filter is <see cref="SincKernel"/>, the
/// same one FT8 uses, at the same length and the same cutoff.</para>
/// <para>**PURE, AND NO CLOCK IS READ.** Samples in, samples out, so the same recording
/// resamples identically on any machine at any hour, and it knows nothing about devices,
/// tabs or radios (§0.1).</para>
/// </remarks>
public sealed class Psk31Resampler
{
    /// <summary>**The rate the PSK31 path is decoded at.**</summary>
    /// <remarks>
    /// **EIGHT THOUSAND, WHICH IS 256 SAMPLES A BIT AT 31.25 BAUD**, and the rate every
    /// fixture in `assets/fixtures` was made at and every threshold in the path was
    /// measured against.
    /// </remarks>
    public const int TargetSampleRate = 8_000;

    /// <summary>How many zero crossings of the sinc each side are kept.</summary>
    /// <remarks>**FT8'S SIXTEEN**, for FT8's reason, from one constant.</remarks>
    public const int ZeroCrossings = Ft8Resample.ZeroCrossings;

    /// <summary>Where the anti-alias filter is put, as a fraction of the lower Nyquist.</summary>
    /// <remarks>
    /// **FT8'S 0.45.** From 48 kHz to 8 kHz that puts the cutoff at 3 600 Hz - above the
    /// 3 000 Hz top of the mode's passband and below the 4 000 Hz fold.
    /// </remarks>
    public const double CutoffFraction = Ft8Resample.CutoffFraction;

    private readonly SincKernel? _kernel;
    private readonly double _step;

    private float[] _held = Array.Empty<float>();
    private float[] _made = Array.Empty<float>();
    private long _heldFrom;
    private int _heldCount;
    private long _seen;
    private long _out;

    /// <summary>Opens a resampler from whatever the device gives to the PSK31 rate.</summary>
    /// <param name="deviceSampleRate">The rate the audio arrives at.</param>
    /// <exception cref="ArgumentOutOfRangeException">The rate is not positive.</exception>
    public Psk31Resampler(int deviceSampleRate)
    {
        if (deviceSampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(deviceSampleRate), deviceSampleRate, "A sample rate must be positive.");
        }

        DeviceSampleRate = deviceSampleRate;

        _step = deviceSampleRate / (double)TargetSampleRate;

        if (deviceSampleRate != TargetSampleRate)
        {
            var cutoff = CutoffFraction
                * Math.Min(deviceSampleRate, TargetSampleRate)
                / deviceSampleRate;

            _kernel = new SincKernel(cutoff, ZeroCrossings);
        }
    }

    /// <summary>The rate the audio arrives at.</summary>
    public int DeviceSampleRate { get; }

    /// <summary>The rate the audio leaves at, which is always the PSK31 rate.</summary>
    public int SampleRate => TargetSampleRate;

    /// <summary>How many input samples go to one output sample.</summary>
    /// <remarks>
    /// **SIX AT 48 KHZ, ONE AT 8.** It goes in the record beside the two rates so a reader
    /// can see at a glance whether the path was resampling at all (§R13).
    /// </remarks>
    public double Ratio => _step;

    /// <summary>True where the device is already at the PSK31 rate and nothing is filtered.</summary>
    public bool PassesThrough => _kernel is null;

    /// <summary>Put a lump of device audio on the PSK31 grid.</summary>
    /// <param name="samples">What arrived, at <see cref="DeviceSampleRate"/>.</param>
    /// <returns>
    /// What is ready at <see cref="TargetSampleRate"/>, which may be empty. **The span is
    /// this resampler's own buffer and is good only until the next call.**
    /// </returns>
    /// <remarks>
    /// **AN OUTPUT SAMPLE IS NOT MADE UNTIL EVERY INPUT SAMPLE ITS FILTER WANTS HAS
    /// ARRIVED.** The alternative is to run the filter against a short tail and correct it
    /// later, which no downstream demodulator could be told about. The cost is a fixed
    /// delay of half a kernel - about 107 samples, 2.2 ms, at 48 kHz - and nothing else in
    /// this path measures anything that fine.
    /// </remarks>
    public ReadOnlySpan<float> Take(ReadOnlySpan<float> samples)
    {
        if (_kernel is null)
        {
            return samples;
        }

        Hold(samples);

        // The last output whose filter fits entirely inside what has arrived.
        var reach = _seen - 1 - _kernel.HalfWidth;
        var ready = reach < 0 ? 0 : (long)Math.Floor(reach / _step) + 1;

        if (ready <= _out)
        {
            return ReadOnlySpan<float>.Empty;
        }

        var count = (int)(ready - _out);

        if (_made.Length < count)
        {
            _made = new float[count];
        }

        for (var i = 0; i < count; i++)
        {
            _made[i] = At((_out + i) * _step);
        }

        _out = ready;

        Forget();

        return _made.AsSpan(0, count);
    }

    /// <summary>One output sample, filtered from the input around a place.</summary>
    /// <param name="centre">Where it sits, in input samples from the first one ever seen.</param>
    /// <remarks>
    /// **EVERY OUTPUT SAMPLE IS NORMALIZED BY ITS OWN KERNEL SUM**, the same as
    /// <see cref="Ft8Resample"/>: without it the gain wanders by a fraction of a decibel
    /// with the fractional phase, and the first samples of the stream fade in.
    /// </remarks>
    private float At(double centre)
    {
        var first = (long)Math.Ceiling(centre - _kernel!.HalfWidth);
        var last = (long)Math.Floor(centre + _kernel.HalfWidth);

        if (first < _heldFrom)
        {
            first = _heldFrom;
        }

        if (last > _seen - 1)
        {
            last = _seen - 1;
        }

        double sum = 0;
        double weight = 0;

        for (var i = first; i <= last; i++)
        {
            var tap = _kernel.At(centre - i);

            sum += _held[(int)(i - _heldFrom)] * tap;
            weight += tap;
        }

        return weight > 0 ? (float)(sum / weight) : 0f;
    }

    /// <summary>Keep what arrived, beside what was kept before.</summary>
    private void Hold(ReadOnlySpan<float> samples)
    {
        if (_heldCount + samples.Length > _held.Length)
        {
            var bigger = new float[Math.Max(1024, (_heldCount + samples.Length) * 2)];

            Array.Copy(_held, bigger, _heldCount);

            _held = bigger;
        }

        samples.CopyTo(_held.AsSpan(_heldCount));

        _heldCount += samples.Length;
        _seen += samples.Length;
    }

    /// <summary>Drop the input no future output sample can reach back to.</summary>
    private void Forget()
    {
        var wanted = (long)Math.Ceiling((_out * _step) - _kernel!.HalfWidth);

        if (wanted <= _heldFrom)
        {
            return;
        }

        var drop = (int)(wanted - _heldFrom);

        if (drop >= _heldCount)
        {
            drop = _heldCount;
        }

        Array.Copy(_held, drop, _held, 0, _heldCount - drop);

        _heldCount -= drop;
        _heldFrom += drop;
    }
}
