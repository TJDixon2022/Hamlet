namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// A seeded, reproducible source of white Gaussian noise. <b>In the test project, not the library:
/// a decoder does not need to make noise.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Seeded and therefore repeatable.</b> The same seed gives the same samples on every run, which
/// is what lets a measurement over noise be quoted as a number rather than as a range, and what step
/// 4's stability criterion will eventually need underneath it.
/// </para>
/// <para>
/// <b>The method is Box–Muller</b>, in its polar rejection form: draw a point uniformly in the unit
/// square, keep it if it falls inside the unit circle, and the two coordinates scaled by
/// sqrt(-2 ln s / s) are two independent standard normals. Textbook, no package, and the second
/// normal of each pair is kept rather than thrown away so the stream costs one rejection loop per
/// two samples.
/// </para>
/// <para>
/// <b>White means flat, and flat is asserted rather than assumed</b> — see
/// <see cref="Ft8NoiseTests"/>, which measures the mean, the standard deviation and the spectral
/// flatness of what comes out of here before anything uses it as a reference.
/// </para>
/// </remarks>
internal sealed class GaussianNoise
{
    private readonly Random _random;
    private double _spare;
    private bool _hasSpare;

    public GaussianNoise(int seed) => _random = new Random(seed);

    /// <summary>One sample from a standard normal: mean zero, standard deviation one.</summary>
    public double NextStandard()
    {
        if (_hasSpare)
        {
            _hasSpare = false;
            return _spare;
        }

        double u, v, s;
        do
        {
            u = (_random.NextDouble() * 2) - 1;
            v = (_random.NextDouble() * 2) - 1;
            s = (u * u) + (v * v);
        }
        while (s >= 1.0 || s == 0.0);

        var scale = Math.Sqrt(-2.0 * Math.Log(s) / s);
        _spare = v * scale;
        _hasSpare = true;
        return u * scale;
    }

    /// <summary>A block of noise at a given root-mean-square amplitude.</summary>
    public float[] Block(int count, double rootMeanSquare)
    {
        var samples = new float[count];
        for (var i = 0; i < count; i++)
        {
            samples[i] = (float)(NextStandard() * rootMeanSquare);
        }

        return samples;
    }

    /// <summary>Adds noise at a given root-mean-square amplitude to a copy of a signal.</summary>
    public float[] AddedTo(ReadOnlySpan<float> signal, double rootMeanSquare)
    {
        var mixed = new float[signal.Length];
        for (var i = 0; i < signal.Length; i++)
        {
            mixed[i] = (float)(signal[i] + (NextStandard() * rootMeanSquare));
        }

        return mixed;
    }
}
