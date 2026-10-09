namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **THE LANE BANK: THE WINDOW FINDS THE STATIONS THE GRID CANNOT** (work instruction 569, HM-DEC-273). A lane is what the sender's
/// own window is: the audio mixed to a pitch and low-passed at a cutoff from a dit, its level read against its own contrast. The
/// bank stands lanes across the radio's passband all the time, before any station is known, so a weak station is read through
/// the filter that fits it rather than the grid's bins, which break it into pieces of one length.
/// </summary>
/// <remarks>
/// <para>**THE TRACE THAT ORDERED IT** (work instruction 568, HM-DEC-272): opened at the true pitch, the sender's window read all
/// four 5 dB CQs 21 of 21, and none of them was ever found, because the grid's 150 Hz bins hold a 5 dB mark only about 11 dB over
/// a noise that swings 5.5 dB hop to hop.</para>
/// <para>**THE LANES.** One every <see cref="SpacingHz"/> across the passband, and three dits per pitch, <see cref="DitSeconds"/>,
/// since the speed is not known before the station is. The three at a pitch share one mixer; each has its own fourth-order
/// Butterworth low-pass, the sender lane's own filter at the cutoff the sender lane takes for that dit.</para>
/// <para>**THE MARKS.** Each lane keeps its last <see cref="HistoryHops"/> levels and takes its key-up and key-down from them,
/// its own noise rather than the grid's. It calls nothing until that contrast is <see cref="MinContrastDb"/>; then a mark rises
/// at 0.4 of the contrast under key-down and falls at 0.6, at least half the lane's dit, as a window's level marks do.</para>
/// <para>**WHERE THEY GO.** Into a pattern gate of their own, one per dit, the same class and rules as the grid's. Their
/// standing marks are handed on only as <see cref="CwEnvelopeDetector"/> decides.</para>
/// </remarks>
internal sealed class CwLaneBank
{
    /// <summary>The spacing of the bank's pitches: the grid's own, 25 Hz.</summary>
    public const double SpacingHz = CwEnvelopeDetector.BinSpacingHz;

    /// <summary>The three dits a pitch is read at: about 12, 20 and 30 WPM.</summary>
    public static readonly double[] DitSeconds = [1.2 / 12, 1.2 / 20, 1.2 / 30];

    /// <summary>
    /// The contrast a lane must show over its own key-up before it calls a mark: 12 dB. Noise alone in a lane spreads about 8 dB
    /// between the 30th and 90th percentiles of its level, which is where key-up and key-down are taken from, and at 10 dB a loud
    /// noise run stood five marks at a shape of 0.68 (work instruction 569); a 5 dB station shows 15 to 17 dB.
    /// </summary>
    public const double MinContrastDb = 12;

    /// <summary>How many hops a lane keeps for its key-up and key-down: three seconds at 5 ms.</summary>
    public const int HistoryHops = 600;

    // How often, in hops, a lane's key-up and key-down are taken again: every 250 ms.
    private const int RefreshHops = 50;

    private readonly Pitch[] _pitches;
    private readonly CwPatternGate[] _gates;
    private readonly double _hopSeconds;
    private readonly int _inputBlock;
    private double _inputSum;
    private int _inputSummed;
    private long _sequence;

    /// <summary>Creates a bank across a passband.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="hopSamples">Samples per hop.</param>
    /// <param name="lowHz">The passband's low edge.</param>
    /// <param name="highHz">The passband's high edge.</param>
    public CwLaneBank(int sampleRate, int hopSamples, double lowHz, double highHz)
    {
        _hopSeconds = hopSamples / (double)sampleRate;

        // **THE AUDIO IS AVERAGED TO ABOUT 4 kHz BEFORE IT IS MIXED** (work instruction 569, task 1): the radio's CW filter keeps it
        // under a kilohertz, and twenty mixers at 48 kHz were most of the bank's cost.
        _inputBlock = Math.Max(1, sampleRate / 4000);

        var mixRate = (int)Math.Round(sampleRate / (double)_inputBlock);
        var inputDelay = (_inputBlock - 1) / (2.0 * sampleRate);
        var first = (int)Math.Ceiling(lowHz / SpacingHz);
        var last = (int)Math.Floor(highHz / SpacingHz);

        _pitches = Enumerable.Range(first, Math.Max(0, last - first + 1))
            .Select(k => new Pitch(k * SpacingHz, mixRate, Decimation(mixRate), inputDelay))
            .ToArray();
        _gates = DitSeconds.Select(_ => new CwPatternGate()).ToArray();
    }

    /// <summary>The lanes run at about 2 kHz: how many mixed samples a block averages.</summary>
    /// <param name="mixRate">The rate the pitches mix at.</param>
    /// <returns>The block, at least one sample.</returns>
    public static int Decimation(int mixRate) => Math.Max(1, mixRate / 2000);

    /// <summary>How many lanes the bank holds.</summary>
    public int Lanes => _pitches.Length * DitSeconds.Length;

    /// <summary>The bank's pattern gates, one per dit, in the order of <see cref="DitSeconds"/>.</summary>
    public IReadOnlyList<CwPatternGate> Gates => _gates;

    /// <summary>Every mark that stood in the bank, in order, for the detector to hand on or keep.</summary>
    public List<(int Dit, CwMark Mark)> Stood { get; } = [];

    /// <summary>Take one sample.</summary>
    /// <param name="x">The sample.</param>
    public void Push(float x)
    {
        _inputSum += x;

        if (++_inputSummed < _inputBlock)
        {
            return;
        }

        var averaged = (float)(_inputSum / _inputBlock);

        _inputSum = 0;
        _inputSummed = 0;

        foreach (var p in _pitches)
        {
            p.Push(averaged);
        }
    }

    /// <summary>One hop: each lane's level, its key-up and key-down, and any mark it calls.</summary>
    /// <param name="nowSeconds">The audio clock at this hop.</param>
    public void Hop(double nowSeconds)
    {
        foreach (var p in _pitches)
        {
            for (var d = 0; d < p.Lanes.Length; d++)
            {
                if (p.Lanes[d].Hop(nowSeconds, _hopSeconds) is { } span)
                {
                    var mark = new CwMark(++_sequence, span.From, span.To, p.Hz, span.LevelDb, span.ContrastDb)
                    {
                        Keyed = true,
                        OwnContrastDb = span.ContrastDb,
                    };

                    foreach (var stood in _gates[d].Offer(mark))
                    {
                        Stood.Add((d, stood));
                    }
                }
            }
        }

        // Kept for the detector to hand on: a minute's worth at most.
        if (Stood.Count > 4000)
        {
            Stood.RemoveRange(0, Stood.Count - 2000);
        }
    }

    /// <summary>The bank's standing sequences, across its gates: pitch, shape and the dit index.</summary>
    /// <param name="nowSeconds">The audio clock.</param>
    /// <returns>Each standing sequence.</returns>
    public IReadOnlyList<(int Dit, CwPatternGate.StandingSequence Sequence)> Standing(double nowSeconds)
        => _gates.SelectMany((g, d) => g.Standing(nowSeconds, CwEnvelopeDetector.HoldSeconds).Select(s => (d, s))).ToList();

    // One pitch: its mixer and its three lanes.
    private sealed class Pitch
    {
        private readonly double _cosStep;
        private readonly double _sinStep;
        private readonly int _decimation;
        private double _c = 1;
        private double _s;
        private int _renorm;
        private double _sumI;
        private double _sumQ;
        private int _summed;

        public Pitch(double hz, int sampleRate, int decimation, double inputDelay)
        {
            Hz = hz;
            _cosStep = Math.Cos(2 * Math.PI * hz / sampleRate);
            _sinStep = Math.Sin(2 * Math.PI * hz / sampleRate);
            _decimation = decimation;
            Lanes = DitSeconds.Select(d => new Lane(hz, d, sampleRate, decimation, inputDelay)).ToArray();
        }

        public double Hz { get; }

        public Lane[] Lanes { get; }

        public void Push(float x)
        {
            var c = (_c * _cosStep) - (_s * _sinStep);
            var s = (_s * _cosStep) + (_c * _sinStep);

            _c = c;
            _s = s;

            if (++_renorm == 4096)
            {
                var r = Math.Sqrt((_c * _c) + (_s * _s));

                _c /= r;
                _s /= r;
                _renorm = 0;
            }

            _sumI += x * _c;
            _sumQ -= x * _s;

            // **THE LANES RUN AT ABOUT 2 kHz** (work instruction 569, task 1): mixed down, the station is within a few tens of hertz
            // of nought, so the halves are averaged over a block and the lanes' filters take one value a block. At the audio's
            // own rate the bank cost 86 per cent of the chain without it.
            if (++_summed < _decimation)
            {
                return;
            }

            var i = _sumI / _decimation;
            var q = _sumQ / _decimation;

            _sumI = _sumQ = 0;
            _summed = 0;

            foreach (var lane in Lanes)
            {
                lane.Push(i, q);
            }
        }
    }

    // One lane: the sender lane's fourth-order Butterworth on each half, its levels, and its mark in progress.
    private sealed class Lane
    {
        private static readonly double[] SectionQ = [1 / (2 * Math.Sin(Math.PI / 8)), 1 / (2 * Math.Sin(3 * Math.PI / 8))];

        private readonly Biquad[] _i = [new(), new()];
        private readonly Biquad[] _q = [new(), new()];
        private readonly double[] _levels = new double[HistoryHops];
        private readonly double[] _sorted = new double[HistoryHops];
        private readonly double _delay;
        private readonly double _dit;
        private double _lastI;
        private double _lastQ;
        private int _count;
        private int _next;
        private int _sinceRefresh;
        private double _up = double.NaN;
        private double _down = double.NaN;
        private double _from = double.NaN;
        private double _sum;
        private int _span;
        private double _fellAt = double.NaN;
        private int _below;

        public Lane(double hz, double dit, int sampleRate, int decimation, double inputDelay)
        {
            _dit = dit;

            // The sender lane's own cutoff and delay for this dit at the lanes' rate, so a bank lane is that window, and the
            // block's own half a block of delay beside it.
            var rate = (int)Math.Round(sampleRate / (double)decimation);
            var reference = new CwSenderLane(rate);

            reference.Tune(hz, dit);
            _delay = reference.DelaySeconds + ((decimation - 1) / (2.0 * sampleRate)) + inputDelay;

            for (var k = 0; k < 2; k++)
            {
                _i[k].Design(reference.CutoffHz, SectionQ[k], rate);
                _q[k].Design(reference.CutoffHz, SectionQ[k], rate);
            }
        }

        public void Push(double i, double q)
        {
            _lastI = _i[1].Next(_i[0].Next(i));
            _lastQ = _q[1].Next(_q[0].Next(q));
        }

        public (double From, double To, double LevelDb, double ContrastDb)? Hop(double now, double hopSeconds)
        {
            var level = 10 * Math.Log10((2 * ((_lastI * _lastI) + (_lastQ * _lastQ))) + 1e-20);

            _levels[_next] = level;
            _next = (_next + 1) % HistoryHops;
            _count = Math.Min(_count + 1, HistoryHops);

            if (++_sinceRefresh >= RefreshHops && _count >= HistoryHops / 3)
            {
                _sinceRefresh = 0;

                Array.Copy(_levels, _sorted, _count);
                Array.Sort(_sorted, 0, _count);

                _up = _sorted[(int)(0.3 * _count)];
                _down = _sorted[(int)(0.9 * _count)];
            }

            var contrast = _down - _up;

            if (!double.IsFinite(contrast) || contrast < MinContrastDb)
            {
                _from = double.NaN;
                return null;
            }

            if (double.IsNaN(_from))
            {
                if (level >= _down - (0.4 * contrast))
                {
                    _from = now;
                    _sum = level;
                    _span = 1;
                }

                return null;
            }

            if (level >= _down - (0.6 * contrast))
            {
                _sum += level;
                _span++;
                _fellAt = double.NaN;
                _below = 0;
                return null;
            }

            // **A MARK ENDS ONLY ONCE THE KEY HAS STAYED UP** (work instruction 569, task 2): a hop of noise under the down line
            // broke a 200 ms dah into 45 and 140 ms with a 5 ms gap, where the sender's shortest gap is a dit. The fall must hold a
            // quarter of the lane's dit, two hops at least, and the mark ends where the fall began.
            if (double.IsNaN(_fellAt))
            {
                _fellAt = now;
            }

            if (++_below * hopSeconds < Math.Max(2 * hopSeconds, 0.25 * _dit))
            {
                return null;
            }

            var from = _from - _delay;
            var to = _fellAt - _delay;
            var mean = _sum / _span;

            _from = double.NaN;
            _fellAt = double.NaN;
            _below = 0;

            return to - from >= 0.5 * _dit ? (from, to, mean, contrast) : null;
        }
    }

    // One second-order low-pass section, as the sender lane's.
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
