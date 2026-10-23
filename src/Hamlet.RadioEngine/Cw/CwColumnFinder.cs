using System.Globalization;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **A MARK IS A COLUMN: A RISE, A FLAT AND A DROP, AND NOTHING ELSE IS A MARK** (work instruction 587, HM-DEC-291). The owner,
/// 2026-10-22: *"We're looking for two shapes, a short and a long, that end up being columns, like a column graph. They're not
/// sharp edges or hacksaws. They have a rise, a flat, and a drop."* This finder sees the audio through its own lanes, matches
/// the envelope of each against the radio's keying edge, pairs a rise with the next drop, and calls a column only where the
/// top between them is flat and the two edges stand at heights that agree. **No level crossing, no percentile floor, no
/// contrast line decides whether a column exists**: every figure here is about the shape, and is named as such.
/// </summary>
/// <remarks>
/// <para>**THE ENVELOPE.** A lane every <see cref="LaneSpacingHz"/> across the passband, each the audio mixed to its pitch and
/// low-passed on both quadratures by a fourth-order Butterworth at <see cref="LaneBandwidthHz"/>, so an edge of a few
/// milliseconds is still an edge; its amplitude is taken once every <see cref="StepMs"/>. The bank's lanes, sized to a dit,
/// blur a 4 ms edge into tens of milliseconds and cannot see the shape; this is the only filter the finder uses.</para>
/// <para>**THE EDGE.** The IC-7300 shapes every keyed element with a raised cosine of its CW rise time, <see cref="KeyingEdgeMs"/>.
/// The template is that edge as this lane sees it: a raised-cosine step put through the lane's own low-pass. The envelope is
/// matched against it by normalized correlation over a window of <c>4</c> edge-lengths; a rise is where the match peaks at
/// <see cref="EdgeMatch"/> or better, the step it measures stands <see cref="ColumnHeightDb"/> over the envelope just before it
/// (the chaos beside the column, read over a few edge-lengths and never a floor over seconds), and the step takes no more than
/// <see cref="EdgeSlowest"/> times the template's own transition. A drop is the same template upside down, judged against the
/// envelope just after it.</para>
/// <para>**THE COLUMN.** A rise paired with the next drop with no rise between them; the top is the stretch between the end of
/// the rise and the start of the drop. It stands where the top is at least one edge-length long, its deviation about a straight
/// line (a fade's tilt is allowed) is no more than <see cref="TopFlatness"/> of the deviation of the chaos before the rise, both
/// edges stood, and the drop's height is within <see cref="EdgePairDb"/> of the rise's. What is not a column is chaos: there is
/// no piece and no splinter.</para>
/// <para>**TWO LANES HOLDING ONE COLUMN** are resolved by height alone: columns that begin and end within an edge-length of each
/// other are one keying, and the tallest lane's is the one delivered. Its pitch is measured from that lane's own mixed signal,
/// the slope of its phase over the top, so a tone between two lanes is placed to the hertz rather than at either.</para>
/// </remarks>
internal sealed class CwColumnFinder
{
    /// <summary>
    /// The radio's CW rise time, in milliseconds: the IC-7300's CW-KEY SET menu offers 2, 4, 6 and 8 ms (Full Manual
    /// `A7292-4EX-6`, p. 4-21). **Four is the manual's default as the author recalls it and the page was not re-read in this
    /// session; the owner's own setting is on no sheet in the tree.** A received station's edges are its own transmitter's,
    /// through this radio's filter, so the match tolerates edges up to <see cref="EdgeSlowest"/> times this.
    /// </summary>
    public const double KeyingEdgeMs = 4;

    /// <summary>The lanes' low-pass cutoff on each quadrature, in Hz: the lane passes about this far either side of its pitch.</summary>
    public const double LaneBandwidthHz = 150;

    /// <summary>The lanes' spacing, in Hz: a third of the bandwidth, so a tone anywhere sits well inside at least two lanes.</summary>
    public const double LaneSpacingHz = 50;

    /// <summary>How often the envelope is read, in milliseconds: a quarter of the keying edge.</summary>
    public const double StepMs = 1;

    /// <summary>
    /// How tall a column must stand over the chaos beside it, in dB, read on its rise over the envelope just before it and on
    /// its drop over the envelope just after: a shape figure about the column's height against its own surroundings, never a
    /// floor.
    /// </summary>
    public const double ColumnHeightDb = 6;

    /// <summary>The normalized correlation an edge must reach against the keying-edge template, nought to one.</summary>
    public const double EdgeMatch = 0.9;

    /// <summary>The most a step's 20 to 80 per cent transition may take, as a multiple of the template's own.</summary>
    public const double EdgeSlowest = 2.5;

    /// <summary>The most a top's deviation about its line may be, as a share of the chaos's deviation before the rise.</summary>
    public const double TopFlatness = 0.45;

    /// <summary>How far the drop's height may differ from the rise's, in dB, the top's tilt allowed for.</summary>
    public const double EdgePairDb = 6;

    /// <summary>How many edge-lengths of chaos are read beside a column, at most: before its rise and after its drop.</summary>
    public const int ChaosEdges = 6;

    /// <summary>
    /// The least top a column may have, in edge-lengths: three. A top is judged flat against the chaos, and a top of one edge-length
    /// through a lane whose own transition is half an edge is all shoulder, with one or two independent samples in it; three
    /// edge-lengths is where a flat exists to be judged, and a 20 ms column, the narrowest the order names, still has one.
    /// </summary>
    public const int TopLeastEdges = 3;

    private readonly ColumnFigures _figures;
    private readonly int _sampleRate;
    private readonly int _inputBlock;
    private readonly int _stepSamples;
    private readonly double _stepSeconds;
    private readonly int _edge;
    private readonly int _template;
    private readonly double[] _rise;
    private readonly double _riseMean;
    private readonly double _riseNorm;
    private readonly double _templateTransition;
    private readonly int _centre;
    private readonly Lane[] _lanes;
    private readonly List<Column> _pending = [];
    private double _inputSum;
    private int _inputSummed;
    private long _steps;
    private long _samples;

    /// <summary>Creates a finder across a passband.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="lowHz">The passband's low edge.</param>
    /// <param name="highHz">The passband's high edge.</param>
    public CwColumnFinder(int sampleRate, double lowHz, double highHz)
        : this(sampleRate, lowHz, highHz, ColumnFigures.Default)
    {
    }

    /// <summary>Creates a finder across a passband with figures of the caller's, for the tests' sweeps.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="lowHz">The passband's low edge.</param>
    /// <param name="highHz">The passband's high edge.</param>
    /// <param name="figures">The shape figures.</param>
    internal CwColumnFinder(int sampleRate, double lowHz, double highHz, ColumnFigures figures)
    {
        _sampleRate = sampleRate;
        _figures = figures;

        // The audio is averaged to about 8 kHz before it is mixed, as the bank averages to 4 kHz: the radio's filter keeps it
        // under a kilohertz, and the mixers are the cost.
        _inputBlock = Math.Max(1, sampleRate / 8000);

        var mixRate = sampleRate / (double)_inputBlock;

        _stepSamples = Math.Max(1, (int)Math.Round(mixRate * StepMs / 1000));
        _stepSeconds = _stepSamples * _inputBlock / (double)sampleRate;
        _edge = Math.Max(2, (int)Math.Round(KeyingEdgeMs / 1000 / _stepSeconds));
        _template = 4 * _edge;

        // The template: the radio's raised-cosine edge, starting one edge-length into the window, through the lane's own low-pass,
        // read at the step; so the match peaks where the key went down, and the filter's delay is inside the template.
        var filter = new Lane(0, mixRate, _stepSamples, figures.LaneBandwidthHz);
        var shape = new double[_template];

        for (var i = 0; i < _template; i++)
        {
            var sum = 0.0;

            for (var s = 0; s < _stepSamples; s++)
            {
                var t = ((i * _stepSamples) + s) / mixRate;
                var into = t - (_edge * _stepSeconds);
                var keyed = into <= 0 ? 0 : into >= KeyingEdgeMs / 1000 ? 1 : 0.5 * (1 - Math.Cos(Math.PI * into / (KeyingEdgeMs / 1000)));

                sum += filter.Filter(keyed);
            }

            shape[i] = sum / _stepSamples;
        }

        _rise = shape;
        _riseMean = _rise.Average();
        _riseNorm = Math.Sqrt(_rise.Sum(v => (v - _riseMean) * (v - _riseMean)));
        // Where the template crosses half its height, in steps after the key went down: the edge's centre as the lane sees it.
        _centre = Array.FindIndex(_rise, v => v >= 0.5 * _rise[^1]) - _edge;
        _templateTransition = Transition(_rise, 0, _template, _edge + _centre, 0, _rise[^1]);

        var first = (int)Math.Ceiling(lowHz / LaneSpacingHz);
        var last = (int)Math.Floor(highHz / LaneSpacingHz);

        if (last < first)
        {
            first = last = Math.Max(1, (int)Math.Round((lowHz + highHz) / 2 / LaneSpacingHz));
        }

        _lanes = Enumerable.Range(first, last - first + 1).Select(k => new Lane(k * LaneSpacingHz, mixRate, _stepSamples, figures.LaneBandwidthHz)).ToArray();
    }

    /// <summary>How many lanes the finder holds.</summary>
    public int Lanes => _lanes.Length;

    /// <summary>The step the envelope is read at, in seconds.</summary>
    public double StepSeconds => _stepSeconds;

    /// <summary>The keying edge in steps.</summary>
    public int EdgeSteps => _edge;

    /// <summary>The template's own 20 to 80 per cent transition, in steps.</summary>
    public double TemplateTransition => _templateTransition;

    /// <summary>The template, for the tests.</summary>
    internal IReadOnlyList<double> Template => _rise;

    /// <summary>The columns completed and resolved since this was last cleared, in the order they ended.</summary>
    public List<CwMark> Found { get; } = [];

    /// <summary>Where set, one line per edge judged and per column judged, with its verdict and why by shape, for the tests' traces.</summary>
    internal List<string>? Trace { get; set; }

    /// <summary>Lanes to trace, by pitch; null traces every lane.</summary>
    internal Func<double, bool>? TraceLane { get; set; }

    private long _sequence;

    /// <summary>Take one sample of audio.</summary>
    /// <param name="x">The sample.</param>
    public void Push(float x)
    {
        _samples++;
        _inputSum += x;

        if (++_inputSummed < _inputBlock)
        {
            return;
        }

        var v = _inputSum / _inputBlock;

        _inputSum = 0;
        _inputSummed = 0;

        var stepped = false;

        foreach (var lane in _lanes)
        {
            stepped = lane.Push(v);
        }

        if (stepped)
        {
            Step();
        }
    }

    // One envelope step on every lane: the correlation at the window now complete, the edge peaks it confirms, the columns closed.
    private void Step()
    {
        var step = _steps++;
        var now = (step + 1) * _stepSeconds;

        if (step + 1 < _template)
        {
            return;
        }

        var k = step + 1 - _template;

        for (var l = 0; l < _lanes.Length; l++)
        {
            var lane = _lanes[l];

            lane.Correlate(k, _rise, _riseMean, _riseNorm, _template);

            // A peak is confirmed with half an edge either side of it, and judged once the envelope four edges past it is in: an edge back.
            var k0 = k - _edge;

            if (k0 < _edge / 2)
            {
                continue;
            }

            var rho = lane.Rho(k0);

            if (rho >= _figures.EdgeMatch && lane.IsPeak(k0, _edge / 2, true))
            {
                Rise(l, lane, k0 + _edge);
            }
            else if (rho <= -_figures.EdgeMatch && lane.IsPeak(k0, _edge / 2, false))
            {
                Drop(l, lane, k0 + _edge, now);
            }
        }

        Resolve(step);
    }

    // A rise at t: the key went down there. Judged on its height over the chaos before it and its steepness.
    private void Rise(int l, Lane lane, long t)
    {
        // The chaos beside the column reaches back no further than the last edge, rise or drop: a rise read against a top is no rise.
        var chaosFrom = Math.Max(lane.LastEdgeEnd, t - (ChaosEdges * _edge));

        if (t - chaosFrom < _edge)
        {
            // Too soon after the last column for the chaos beside it to be read: the last chaos stands for it.
            chaosFrom = t - _edge;
        }

        var before = lane.MeanDb(chaosFrom, t);
        var after = lane.MeanDb(t + (2 * _edge), t + (4 * _edge));
        var height = after - before;
        var transition = Transition(lane, t, lane.MeanAmplitude(chaosFrom, t), lane.MeanAmplitude(t + (2 * _edge), t + (4 * _edge)));
        var steep = transition <= _figures.EdgeSlowest * _templateTransition;
        var tall = height >= _figures.HeightDb;

        if (Trace is { } trace && (TraceLane is null || TraceLane(lane.Hz)))
        {
            trace.Add(string.Create(CultureInfo.InvariantCulture, $"{t * _stepSeconds:0.000} s {lane.Hz:0} Hz rise match {lane.Rho(t - _edge):0.00} height {height:0.0} dB transition {transition:0.0} steps: {(tall && steep ? "stands" : !tall ? "too short over the chaos" : "too slow")}"));
        }

        // A match with no height is the top's own ripple, or the chaos's: not an edge, and it leaves what was open as it was.
        if (!tall || !steep)
        {
            return;
        }

        lane.LastEdgeEnd = Math.Max(lane.LastEdgeEnd, t + (2 * _edge));

        // A rise with a rise after it and no drop between: the first was not a column's.
        lane.OpenRise = (t, before, lane.Rho(t - _edge), chaosFrom);
    }

    // A drop at t: the key came up there. With an open rise it closes a column, judged whole.
    private void Drop(int l, Lane lane, long t, double now)
    {
        var open = lane.OpenRise;

        // The drop on its own first: the envelope just before it over the envelope just after, and its steepness. A match with no
        // height is the top's own ripple, or the chaos's: not an edge, and it leaves what was open as it was.
        var afterTo = t + (4 * _edge);
        var after = lane.MeanDb(t + (2 * _edge), afterTo);
        var dropAlone = lane.MeanDb(t - (2 * _edge), t) - after;
        var transition = Transition(lane, t, lane.MeanAmplitude(t - (2 * _edge), t), lane.MeanAmplitude(t + (2 * _edge), afterTo));
        var steep = transition <= _figures.EdgeSlowest * _templateTransition;

        if (dropAlone < _figures.HeightDb || !steep)
        {
            if (Trace is { } ripple && (TraceLane is null || TraceLane(lane.Hz)))
            {
                ripple.Add(string.Create(CultureInfo.InvariantCulture, $"{t * _stepSeconds:0.000} s {lane.Hz:0} Hz drop match {lane.Rho(t - _edge):0.00} height {dropAlone:0.0} dB transition {transition:0.0} steps: {(dropAlone < _figures.HeightDb ? "too short over the chaos" : "too slow")}"));
            }

            return;
        }

        lane.OpenRise = null;
        lane.LastEdgeEnd = Math.Max(lane.LastEdgeEnd, t + (2 * _edge));

        if (open is not { } rise)
        {
            if (Trace is { } alone && (TraceLane is null || TraceLane(lane.Hz)))
            {
                alone.Add(string.Create(CultureInfo.InvariantCulture, $"{t * _stepSeconds:0.000} s {lane.Hz:0} Hz drop match {lane.Rho(t - _edge):0.00} height {dropAlone:0.0} dB with no rise open"));
            }

            return;
        }

        var topFrom = rise.At + (2 * _edge);
        var topTo = t;
        var topLine = lane.Line(topFrom, topTo);
        var chaosLine = lane.Line(rise.ChaosFrom, rise.At);
        var chaosDeviation = rise.At - rise.ChaosFrom >= 2 * _edge ? chaosLine.Deviation : lane.LastChaosDeviation;
        var topLong = topTo - topFrom >= _figures.TopLeastEdges * _edge;
        var flat = topLong && double.IsFinite(chaosDeviation) && topLine.Deviation <= _figures.TopFlatness * chaosDeviation;
        var riseDb = topLine.At(topFrom) - rise.BeforeDb;
        var dropDb = topLine.At(topTo) - after;
        var tall = riseDb >= _figures.HeightDb && dropDb >= _figures.HeightDb;
        var paired = Math.Abs(riseDb - dropDb) <= _figures.EdgePairDb;

        if (rise.At - rise.ChaosFrom >= 2 * _edge)
        {
            lane.LastChaosDeviation = chaosLine.Deviation;
        }

        var verdict = !topLong ? "no top" : !steep ? "drop too slow" : !tall ? "an edge too short over the chaos" : !flat ? "top not flat" : !paired ? "edges of two heights" : "a column";

        if (Trace is { } trace && (TraceLane is null || TraceLane(lane.Hz)))
        {
            trace.Add(string.Create(CultureInfo.InvariantCulture, $"{t * _stepSeconds:0.000} s {lane.Hz:0} Hz drop match {lane.Rho(t - _edge):0.00} closes rise at {rise.At * _stepSeconds:0.000} s: top {(topTo - topFrom) * _stepSeconds * 1000:0} ms deviation {topLine.Deviation:0.00} over chaos {chaosDeviation:0.00} dB, rise {riseDb:0.0} drop {dropDb:0.0} dB, transition {transition:0.0}: {verdict}"));
        }

        if (verdict != "a column")
        {
            return;
        }

        var level = topLine.Mean;
        var contrast = level - ((rise.BeforeDb + after) / 2);
        var pitch = lane.Hz + lane.OffsetHz(topFrom, topTo, _stepSeconds);
        var shape = new CwMarkShape(
            Flatness: Math.Clamp(1 - (topLine.Deviation / (_figures.TopFlatness * chaosDeviation)), 0, 1),
            Edges: Math.Clamp(Math.Abs(rise.Match) * Math.Abs(lane.Rho(t - _edge)), 0, 1),
            Narrowness: 1,
            Contrast: Math.Clamp(contrast / 15, 0, 1),
            Length: Math.Min(1, CwEnvelopeDetector.LongestDahMs / ((t - rise.At) * _stepSeconds * 1000)));

        _pending.Add(new Column(l, rise.At, t, level, contrast, pitch, shape));
    }

    // Columns that began and ended within an edge of one another are one keying: the tallest lane's is delivered, once every
    // lane has had an edge-length to close its own.
    private void Resolve(long step)
    {
        for (var i = 0; i < _pending.Count; i++)
        {
            var c = _pending[i];

            if (step < c.To + (6 * _edge))
            {
                continue;
            }

            // The same keying: edges within an edge-length of each other; or, in a lane further off where the filter's skirt delays
            // them more, a column over the same stretch whose measured pitch is the same tone's.
            var same = _pending.Where(o => (Math.Abs(o.From - c.From) <= _edge && Math.Abs(o.To - c.To) <= _edge)
                || (Math.Abs(o.PitchHz - c.PitchHz) <= LaneSpacingHz / 2
                    && Math.Min(o.To, c.To) - Math.Max(o.From, c.From) > 0.5 * Math.Min(o.To - o.From, c.To - c.From))).ToList();
            var tallest = same.OrderByDescending(o => o.LevelDb).First();

            foreach (var o in same)
            {
                _pending.Remove(o);
            }

            i = -1;

            var from = tallest.From * _stepSeconds;
            var to = tallest.To * _stepSeconds;

            Found.Add(new CwMark(++_sequence, from, to, tallest.PitchHz, tallest.LevelDb, tallest.ContrastDb)
            {
                Column = true,
                Keyed = true,
                Stood = false,
                Shape = tallest.Shape,
                OwnContrastDb = tallest.ContrastDb,
            });

            if (Trace is { } trace && (TraceLane is null || TraceLane(_lanes[tallest.Lane].Hz)))
            {
                trace.Add(string.Create(CultureInfo.InvariantCulture, $"{to:0.000} s column {from:0.000}-{to:0.000} s ({(to - from) * 1000:0} ms) at {tallest.PitchHz:0.0} Hz level {tallest.LevelDb:0.0} dB contrast {tallest.ContrastDb:0.0} dB, tallest of {same.Count} lanes"));
            }
        }
    }

    /// <summary>
    /// The 20 to 80 per cent transition of a step, in steps, between its start level and its end level, read outward from the
    /// step's centre: the last point before the centre still on the start side of the 20 per cent line, to the first point after
    /// it on the end side of the 80 per cent line. Read from the centre so a ripple well before the edge cannot begin it.
    /// Infinite where either crossing is not found.
    /// </summary>
    private static double Transition(IReadOnlyList<double> values, long from, long to, long centre, double start, double end)
    {
        var a = start + (0.2 * (end - start));
        var b = start + (0.8 * (end - start));
        var rising = end >= start;
        double tA = double.NaN, tB = double.NaN;

        for (var i = Math.Min(centre, to - 1); i >= from; i--)
        {
            var v = values[(int)(i - from)];

            if (rising ? v <= a : v >= a)
            {
                tA = i;
                break;
            }
        }

        for (var i = Math.Max(centre, from); i < to; i++)
        {
            var v = values[(int)(i - from)];

            if (rising ? v >= b : v <= b)
            {
                tB = i;
                break;
            }
        }

        return double.IsNaN(tA) || double.IsNaN(tB) ? double.PositiveInfinity : tB - tA;
    }

    // The transition of the edge at t on a lane, over the window the template is matched over, from the edge's centre.
    private double Transition(Lane lane, long t, double start, double end)
        => Transition(lane.Amplitudes(t - _edge, t + (3 * _edge)), t - _edge, t + (3 * _edge), t + _centre, start, end);

    private readonly record struct Column(int Lane, long From, long To, double LevelDb, double ContrastDb, double PitchHz, CwMarkShape Shape);

    /// <summary>A straight line fitted to a stretch of the envelope in dB, and the deviation about it.</summary>
    private readonly record struct Fit(double Mean, double Slope, double Middle, double Deviation)
    {
        public double At(long step) => Mean + (Slope * (step - Middle));
    }

    // One lane: a mixer, two quadrature low-passes, and the envelope's history in amplitude, dB and phase.
    private sealed class Lane
    {
        private const int Ring = 4096;
        private static readonly double[] SectionQ = [1 / (2 * Math.Sin(Math.PI / 8)), 1 / (2 * Math.Sin(3 * Math.PI / 8))];
        private readonly double _cosStep;
        private readonly double _sinStep;
        private readonly int _stepSamples;
        private readonly Biquad[] _i = [new(), new()];
        private readonly Biquad[] _q = [new(), new()];
        private readonly double[] _amp = new double[Ring];
        private readonly double[] _db = new double[Ring];
        private readonly double[] _re = new double[Ring];
        private readonly double[] _im = new double[Ring];
        private readonly double[] _rho = new double[Ring];
        private double _c = 1;
        private double _s;
        private int _renorm;
        private int _summed;
        private long _steps;

        public Lane(double hz, double mixRate, int stepSamples, double bandwidthHz)
        {
            Hz = hz;
            _cosStep = Math.Cos(2 * Math.PI * hz / mixRate);
            _sinStep = Math.Sin(2 * Math.PI * hz / mixRate);
            _stepSamples = stepSamples;

            for (var k = 0; k < 2; k++)
            {
                _i[k].Design(bandwidthHz, SectionQ[k], mixRate);
                _q[k].Design(bandwidthHz, SectionQ[k], mixRate);
            }
        }

        public double Hz { get; }

        public (long At, double BeforeDb, double Match, long ChaosFrom)? OpenRise { get; set; }

        /// <summary>Where the last edge judged, rise or drop, had settled: the chaos beside a column reaches back no further.</summary>
        public long LastEdgeEnd { get; set; } = long.MinValue / 2;

        public double LastChaosDeviation { get; set; } = double.NaN;

        /// <summary>The in-phase low-pass alone, for building the template.</summary>
        public double Filter(double x) => _i[1].Next(_i[0].Next(x));

        public bool Push(double x)
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

            var i = _i[1].Next(_i[0].Next(x * _c));
            var q = _q[1].Next(_q[0].Next(-x * _s));

            if (++_summed < _stepSamples)
            {
                return false;
            }

            _summed = 0;

            var at = (int)(_steps % Ring);

            _re[at] = i;
            _im[at] = q;
            _amp[at] = Math.Sqrt((i * i) + (q * q));
            _db[at] = 10 * Math.Log10((2 * ((i * i) + (q * q))) + 1e-20);
            _rho[at] = 0;
            _steps++;

            return true;
        }

        /// <summary>The normalized correlation of the window beginning at k with the template, kept at k.</summary>
        public void Correlate(long k, double[] template, double mean, double norm, int length)
        {
            double sum = 0;

            for (var i = 0; i < length; i++)
            {
                sum += _amp[(int)((k + i) % Ring)];
            }

            var m = sum / length;
            double cross = 0, energy = 0;

            for (var i = 0; i < length; i++)
            {
                var d = _amp[(int)((k + i) % Ring)] - m;

                cross += d * (template[i] - mean);
                energy += d * d;
            }

            _rho[(int)(k % Ring)] = energy <= 0 ? 0 : cross / (Math.Sqrt(energy) * norm);
        }

        public double Rho(long k) => _rho[(int)(k % Ring)];

        public bool IsPeak(long k, int half, bool max)
        {
            var v = _rho[(int)(k % Ring)];

            for (var j = 1; j <= half; j++)
            {
                var a = _rho[(int)((k - j) % Ring)];
                var b = _rho[(int)((k + j) % Ring)];

                if (max ? a > v || b >= v : a < v || b <= v)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The mean power over a stretch, in dB: a mean of powers, so one null in the chaos does not drag it down as a mean of decibels would.</summary>
        public double MeanDb(long from, long to)
        {
            if (to <= from)
            {
                return double.NaN;
            }

            double sum = 0;

            for (var i = from; i < to; i++)
            {
                var a = _amp[(int)(((i % Ring) + Ring) % Ring)];

                sum += a * a;
            }

            return 10 * Math.Log10((2 * sum / (to - from)) + 1e-20);
        }

        public double MeanAmplitude(long from, long to)
        {
            if (to <= from)
            {
                return double.NaN;
            }

            double sum = 0;

            for (var i = from; i < to; i++)
            {
                sum += _amp[(int)(((i % Ring) + Ring) % Ring)];
            }

            return sum / (to - from);
        }

        public double[] Amplitudes(long from, long to)
        {
            var a = new double[Math.Max(0, to - from)];

            for (var i = from; i < to; i++)
            {
                a[i - from] = _amp[(int)(((i % Ring) + Ring) % Ring)];
            }

            return a;
        }

        /// <summary>A straight line through the stretch in dB, and the deviation about it.</summary>
        public Fit Line(long from, long to)
        {
            var n = to - from;

            if (n <= 0)
            {
                return new Fit(double.NaN, 0, from, double.NaN);
            }

            var middle = from + ((n - 1) / 2.0);
            double sy = 0, sxy = 0, sxx = 0;

            for (var i = from; i < to; i++)
            {
                var y = _db[(int)(((i % Ring) + Ring) % Ring)];
                var x = i - middle;

                sy += y;
                sxy += x * y;
                sxx += x * x;
            }

            var mean = sy / n;
            var slope = sxx > 0 && n >= 3 ? sxy / sxx : 0;
            double ss = 0;

            for (var i = from; i < to; i++)
            {
                var r = _db[(int)(((i % Ring) + Ring) % Ring)] - mean - (slope * (i - middle));

                ss += r * r;
            }

            return new Fit(mean, slope, middle, Math.Sqrt(ss / n));
        }

        /// <summary>The tone's offset from the lane's pitch over a stretch, in Hz: the slope of the unwrapped phase, by regression.</summary>
        public double OffsetHz(long from, long to, double stepSeconds)
        {
            var n = to - from;

            if (n < 3)
            {
                return 0;
            }

            var phase = 0.0;
            var previous = Math.Atan2(_im[(int)(((from % Ring) + Ring) % Ring)], _re[(int)(((from % Ring) + Ring) % Ring)]);
            var middle = (n - 1) / 2.0;
            double sy = 0, sxy = 0, sxx = 0;

            for (var i = 0; i < n; i++)
            {
                var at = (int)((((from + i) % Ring) + Ring) % Ring);
                var p = Math.Atan2(_im[at], _re[at]);
                var d = p - previous;

                d = d > Math.PI ? d - (2 * Math.PI) : d < -Math.PI ? d + (2 * Math.PI) : d;
                phase += d;
                previous = p;

                var x = i - middle;

                sy += phase;
                sxy += x * phase;
                sxx += x * x;
            }

            var slope = sxy / sxx;

            // The mixer runs at e^{-j2πft}, so a tone above the lane's pitch turns the phase forward.
            return slope / (2 * Math.PI * stepSeconds);
        }

        private sealed class Biquad
        {
            private double _b0, _b1, _b2, _a1, _a2;
            private double _x1, _x2, _y1, _y2;

            public void Design(double cutoffHz, double q, double sampleRate)
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
}

/// <summary>The column finder's shape figures, each defaulting to the finder's own constants; the tests sweep them (work instruction 587).</summary>
/// <param name="HeightDb">How tall a column stands over the chaos beside it, in dB.</param>
/// <param name="EdgeMatch">The normalized correlation an edge must reach against the keying-edge template.</param>
/// <param name="EdgeSlowest">The most a step may take, as a multiple of the template transition.</param>
/// <param name="TopFlatness">The most a top may deviate, as a share of the chaos deviation.</param>
/// <param name="EdgePairDb">How far the drop height may differ from the rise height, in dB.</param>
/// <param name="TopLeastEdges">The least top, in edge-lengths.</param>
/// <param name="LaneBandwidthHz">The lanes' low-pass cutoff, in Hz.</param>
internal sealed record ColumnFigures(
    double HeightDb = CwColumnFinder.ColumnHeightDb,
    double EdgeMatch = CwColumnFinder.EdgeMatch,
    double EdgeSlowest = CwColumnFinder.EdgeSlowest,
    double TopFlatness = CwColumnFinder.TopFlatness,
    double EdgePairDb = CwColumnFinder.EdgePairDb,
    int TopLeastEdges = CwColumnFinder.TopLeastEdges,
    double LaneBandwidthHz = CwColumnFinder.LaneBandwidthHz)
{
    /// <summary>The finder's own figures, its constants.</summary>
    public static ColumnFigures Default { get; } = new();

    /// <inheritdoc />
    public override string ToString()
        => string.Create(CultureInfo.InvariantCulture, $"height {HeightDb:0} dB, match {EdgeMatch:0.00}, slowest {EdgeSlowest:0.0}x, flatness {TopFlatness:0.00}, pair {EdgePairDb:0} dB, top {TopLeastEdges} edges, lane {LaneBandwidthHz:0} Hz");
}
