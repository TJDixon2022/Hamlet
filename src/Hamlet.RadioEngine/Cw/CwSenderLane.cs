namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **A STANDING SENDER SEEN THROUGH A WINDOW THAT FITS IT** (work instruction 529, HM-DEC-233): the audio mixed down at
/// the sender's own pitch and low-passed at a cutoff from its own dit, one level per sample, causal.
/// </summary>
/// <remarks>
/// <para>**WHY.** The per-bin path reads every pitch through a ten millisecond window on a 25 Hz grid, wide enough for
/// a 45 WPM dit, about 150 Hz of noise, and centred on the grid rather than on the tone. On the owner's recording of
/// 2026-10-02 the top of the T of BEST wobbled 3 to 4 dB through it; mixed at the tone and low-passed at 60 Hz it
/// wobbled 0.4. The flatness test is right and the measurement is noisy. Once a sender stands its pitch and its dit are
/// known, so it can be looked at through a filter matched to it.</para>
/// <para>**THE FILTER.** The signal is mixed with a complex tone at the sender's pitch, and the in-phase and
/// quadrature halves are each low-passed by a fourth-order Butterworth, two biquads by the bilinear transform. Its
/// level is twice their summed squares, so a full-scale sine reads -3 dB, as a bin's Goertzel level does: the two
/// paths' levels are on one scale.</para>
/// <para>**THE CUTOFF, AUTHOR'S: THE KEY'S EDGES TAKE A QUARTER OF A DIT.** A keyed rectangle comes out of a low-pass
/// with its edges slowed to the filter's rise time. Rising and falling together take half a dit when each takes a
/// quarter, which leaves a dit's top flat over its middle half, enough for the flatness test, and a dah flat over
/// nearly all of it. Narrower would round a dit's top away; wider lets in noise the dit does not need. So the cutoff is
/// set where the filter's 10 to 90 per cent step rise is a quarter of the dit: <see cref="RiseShare"/>. It is not taken
/// from the recording; the web session's 60 Hz on a 75 ms dit is one point, and this rule puts that dit at about 21 Hz.
/// It is held between <see cref="LowestCutoffHz"/> and <see cref="HighestCutoffHz"/>.</para>
/// <para>**THE DELAY.** A causal filter is late. An edge's half-amplitude crossing comes out
/// <see cref="DelaySeconds"/> after the edge went in, measured on the filter's own step response, and a mark's times
/// are taken back by it.</para>
/// </remarks>
internal sealed class CwSenderLane
{
    /// <summary>The share of a dit the filter's 10 to 90 per cent rise takes: a quarter.</summary>
    public const double RiseShare = 0.25;

    /// <summary>The narrowest cutoff, in Hz: a 5 WPM dit's quarter is 60 ms, which needs about 9 Hz; this sits under it.</summary>
    public const double LowestCutoffHz = 5;

    /// <summary>The widest cutoff, in Hz: the per-bin window's own noise bandwidth, past which the lane sees no better.</summary>
    public const double HighestCutoffHz = 150;

    // The two Butterworth sections' Q for a fourth order.
    private static readonly double[] SectionQ = { 1 / (2 * Math.Sin(Math.PI / 8)), 1 / (2 * Math.Sin(3 * Math.PI / 8)) };

    private readonly int _sampleRate;
    private readonly Biquad[] _i = { new(), new() };
    private readonly Biquad[] _q = { new(), new() };
    private double _phase;
    private double _step;
    private double _lastI;
    private double _lastQ;

    /// <summary>Creates a lane for one sample rate, not yet tuned.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    public CwSenderLane(int sampleRate)
    {
        _sampleRate = sampleRate;
    }

    /// <summary>The pitch mixed at, in Hz; NaN before it is tuned.</summary>
    public double PitchHz { get; private set; } = double.NaN;

    /// <summary>The dit the cutoff was set from, in seconds.</summary>
    public double DitSeconds { get; private set; } = double.NaN;

    /// <summary>The low-pass cutoff, in Hz.</summary>
    public double CutoffHz { get; private set; } = double.NaN;

    /// <summary>How late an edge's half-amplitude crossing comes out, in seconds.</summary>
    public double DelaySeconds { get; private set; }

    /// <summary>The filter's 10 to 90 per cent step rise, in seconds.</summary>
    public double RiseSeconds { get; private set; }

    /// <summary>The level now: twice the mixed halves' summed squares, in dB, so a full-scale sine reads -3 dB.</summary>
    public double LevelDb => 10 * Math.Log10((2 * ((_lastI * _lastI) + (_lastQ * _lastQ))) + 1e-20);

    /// <summary>The cutoff that makes the rise a quarter of a dit.</summary>
    /// <param name="ditSeconds">The sender's dit.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>The cutoff in Hz, held to its limits.</returns>
    public static double CutoffFor(double ditSeconds, int sampleRate)
    {
        // The rise of a fixed filter shape scales as one over its cutoff, so it is measured once at a reference cutoff.
        var reference = sampleRate / 200.0;
        var (rise, _) = Measure(reference, sampleRate);

        return Math.Clamp(reference * rise / (RiseShare * ditSeconds), LowestCutoffHz, Math.Min(HighestCutoffHz, sampleRate / 10.0));
    }

    /// <summary>Tune to a sender: its pitch, and a cutoff from its dit. The filter's state is kept across a retune.</summary>
    /// <param name="pitchHz">The sender's pitch.</param>
    /// <param name="ditSeconds">The sender's dit.</param>
    public void Tune(double pitchHz, double ditSeconds)
    {
        PitchHz = pitchHz;
        DitSeconds = ditSeconds;
        _step = 2 * Math.PI * pitchHz / _sampleRate;

        var cutoff = CutoffFor(ditSeconds, _sampleRate);

        if (double.IsFinite(CutoffHz) && Math.Abs(cutoff - CutoffHz) <= 0.1 * CutoffHz)
        {
            return;
        }

        CutoffHz = cutoff;

        for (var k = 0; k < 2; k++)
        {
            _i[k].Design(cutoff, SectionQ[k], _sampleRate);
            _q[k].Design(cutoff, SectionQ[k], _sampleRate);
        }

        (RiseSeconds, DelaySeconds) = Measure(cutoff, _sampleRate);
    }

    /// <summary>Clear the filter's state, for a sender that is not the last one.</summary>
    public void Reset()
    {
        foreach (var b in _i.Concat(_q))
        {
            b.Reset();
        }

        _lastI = _lastQ = 0;
    }

    /// <summary>Take one sample.</summary>
    /// <param name="x">The sample.</param>
    public void Push(double x)
    {
        _phase += _step;

        if (_phase > 2 * Math.PI)
        {
            _phase -= 2 * Math.PI;
        }

        _lastI = _i[1].Next(_i[0].Next(x * Math.Cos(_phase)));
        _lastQ = _q[1].Next(_q[0].Next(-x * Math.Sin(_phase)));
    }

    /// <summary>The filter's 10 to 90 per cent rise and its half-amplitude delay, from its own step response.</summary>
    private static (double RiseSeconds, double DelaySeconds) Measure(double cutoffHz, int sampleRate)
    {
        var a = new Biquad();
        var b = new Biquad();

        a.Design(cutoffHz, SectionQ[0], sampleRate);
        b.Design(cutoffHz, SectionQ[1], sampleRate);

        double t10 = double.NaN, t50 = double.NaN, t90 = double.NaN;
        var prev = 0.0;
        var limit = (int)(20 * sampleRate / cutoffHz);

        for (var n = 0; n < limit && double.IsNaN(t90); n++)
        {
            var y = b.Next(a.Next(1));

            double Cross(double level) => n - 1 + ((level - prev) / (y - prev));

            if (double.IsNaN(t10) && y >= 0.1)
            {
                t10 = Cross(0.1);
            }

            if (double.IsNaN(t50) && y >= 0.5)
            {
                t50 = Cross(0.5);
            }

            if (double.IsNaN(t90) && y >= 0.9)
            {
                t90 = Cross(0.9);
            }

            prev = y;
        }

        // The step goes in at sample nought and the first output is sample nought, so the crossings are in samples from the edge.
        return ((t90 - t10) / sampleRate, (t50 + 1) / sampleRate);
    }

    /// <summary>One second-order low-pass section.</summary>
    private sealed class Biquad
    {
        private double _b0, _b1, _b2, _a1, _a2;
        private double _x1, _x2, _y1, _y2;

        public void Design(double cutoffHz, double q, int sampleRate)
        {
            var w0 = 2 * Math.PI * cutoffHz / sampleRate;
            var alpha = Math.Sin(w0) / (2 * q);
            var cos = Math.Cos(w0);
            var a0 = 1 + alpha;

            _b0 = (1 - cos) / 2 / a0;
            _b1 = (1 - cos) / a0;
            _b2 = (1 - cos) / 2 / a0;
            _a1 = -2 * cos / a0;
            _a2 = (1 - alpha) / a0;
        }

        public void Reset() => _x1 = _x2 = _y1 = _y2 = 0;

        public double Next(double x)
        {
            var y = (_b0 * x) + (_b1 * _x1) + (_b2 * _x2) - (_a1 * _y1) - (_a2 * _y2);

            _x2 = _x1;
            _x1 = x;
            _y2 = _y1;
            _y1 = y;

            return y;
        }
    }
}
