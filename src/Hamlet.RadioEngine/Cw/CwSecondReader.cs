using Hamlet.RadioEngine.Cw.Second;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// The fldigi port, reading live beside our decoder: the same hops, brought to
/// 8000 Hz, at the pitch our decoder has in force (HM-REQ-120; work instruction
/// 465, task 3; PHASE_PLAN.md 9.6).
/// </summary>
/// <remarks>
/// <para>**OUTSIDE <c>Cw/Second/</c>, AND NOTHING THERE CHANGES** (HM-REQ-122, 129;
/// 465 DECIDED (8)). The port is constructed, fed through its own
/// <see cref="FldigiCwDecoder.rx_process"/> and read through
/// <see cref="CwSecondHarvester"/>, with <see cref="FldigiCwDecoder.TraceDecisions"/>
/// on because only its decision rows place its key events.</para>
/// <para>**THE RESAMPLING IS <see cref="FldigiRateAdapter"/>'S, STREAMED.** The same
/// 481-tap Blackman-windowed sinc at 3600 Hz, the same kernel arithmetic and the
/// same order of summation, so a whole file streamed here gives the port the same
/// doubles <see cref="FldigiRateAdapter.ToFldigiRate"/> gives it; an output is made
/// once the 240 samples after its centre have arrived, 5 ms at 48000 Hz. A rate
/// that is not a whole multiple of 8000 is refused, as the adapter refuses it, and
/// the reader then reads nothing and says why in <see cref="Unavailable"/>.</para>
/// <para>**THE PITCH IS FOLLOWED BY RE-CONSTRUCTION** (465 task 1, item 4). The
/// port's carrier frequency is set once, by its constructor, and it has no setter;
/// fldigi's <c>modem::set_freq</c> is not in the port. So when the pitch our
/// decoder has in force moves <see cref="RetuneHz"/> or more from the port's, the
/// port is built afresh at the new pitch, losing its filter, level and speed
/// state. The line is half our own mixdown filter,
/// <see cref="CwProbabilisticDecoder.BandwidthHz"/>, the line our decoder already
/// uses for "the same sender".</para>
/// <para>**ITS MEMORY IS BOUNDED.** The decision rows grow at 500 a second; after
/// <see cref="RowCap"/> of them, at the first two seconds with no key event, the
/// port is rebuilt at the same pitch. None of the keyed recordings is long enough
/// to reach it.</para>
/// </remarks>
public sealed class CwSecondReader
{
    /// <summary>How far our pitch may move from the port's before the port is rebuilt there: half our mixdown filter, 30 Hz.</summary>
    public const double RetuneHz = CwProbabilisticDecoder.BandwidthHz / 2;

    /// <summary>Decision rows one port may hold before it is rebuilt at a quiet moment: five minutes at 500 a second.</summary>
    public const int RowCap = 150_000;

    // Two seconds of decision rows with no key event.
    private const int QuietRows = 1_000;

    // FldigiRateAdapter's filter, taps and cut-off.
    private const int Taps = 481;
    private const double CutoffHz = 3600;

    private readonly int _factor;
    private readonly double[] _taps = Array.Empty<double>();
    private readonly List<float> _input = new();
    private long _inputBase;
    private long _inputCount;
    private long _nextOutput;

    private FldigiCwDecoder? _port;
    private CwSecondHarvester? _harvester;

    /// <summary>Creates a reader for audio at a rate.</summary>
    /// <param name="sampleRate">The rate our decoder reads at.</param>
    public CwSecondReader(int sampleRate)
    {
        SampleRate = sampleRate;

        if (sampleRate <= 0 || sampleRate % FldigiCwDecoder.CW_SAMPLERATE != 0)
        {
            Unavailable = $"{sampleRate} Hz is not a whole multiple of fldigi's {FldigiCwDecoder.CW_SAMPLERATE} Hz, which FldigiRateAdapter refuses";
            return;
        }

        _factor = sampleRate / FldigiCwDecoder.CW_SAMPLERATE;
        _taps = _factor == 1 ? Array.Empty<double>() : Kernel(CutoffHz / sampleRate);
    }

    /// <summary>The rate it is fed at.</summary>
    public int SampleRate { get; }

    /// <summary>Why the port cannot read at this rate, or null where it reads.</summary>
    public string? Unavailable { get; }

    /// <summary>Samples it has been handed or told to skip, at <see cref="SampleRate"/>.</summary>
    public long SamplesRead => _inputCount;

    /// <summary>The port now reading, or null before the first hop.</summary>
    public FldigiCwDecoder? Port => _port;

    /// <summary>How many times a port was built: the first, and every rebuild after.</summary>
    public int Constructions { get; private set; }

    /// <summary>How many of those were for a pitch move of <see cref="RetuneHz"/> or more.</summary>
    public int Retunes { get; private set; }

    /// <summary>How many were after audio was skipped (the radio transmitting, or Digital mode).</summary>
    public int AfterSkips { get; private set; }

    /// <summary>How many were for the memory bound.</summary>
    public int MemoryRebuilds { get; private set; }

    /// <summary>Reads one hop at the pitch in force.</summary>
    /// <param name="samples">The same hop our decoder is about to read.</param>
    /// <param name="pitchHz">The pitch our decoder mixes it at.</param>
    /// <returns>The port's readings completed by this hop.</returns>
    public IReadOnlyList<CwSecondReading> Read(ReadOnlySpan<float> samples, double pitchHz)
    {
        if (Unavailable is not null)
        {
            _inputCount += samples.Length;
            return Array.Empty<CwSecondReading>();
        }

        var found = new List<CwSecondReading>();

        if (_port is null)
        {
            Build(pitchHz);
        }
        else if (Math.Abs(pitchHz - _port.Frequency) >= RetuneHz)
        {
            found.AddRange(_harvester!.Take(_port));
            Build(pitchHz);
            Retunes++;
        }
        else if (_port.Decisions.Count >= RowCap && Quiet(_port))
        {
            found.AddRange(_harvester!.Take(_port));
            Build(_port.Frequency);
            MemoryRebuilds++;
        }

        foreach (var s in samples)
        {
            _input.Add(s);
        }

        _inputCount += samples.Length;
        Feed(false);
        found.AddRange(_harvester!.Take(_port!));

        return found;
    }

    /// <summary>Audio passed by without being read: the clock runs on, and the port starts afresh after it.</summary>
    /// <param name="count">Samples skipped.</param>
    /// <returns>What the port had completed before the gap.</returns>
    public IReadOnlyList<CwSecondReading> Skip(int count)
    {
        var found = _port is null || _harvester is null ? Array.Empty<CwSecondReading>() : _harvester.Take(_port);

        if (_port is not null)
        {
            AfterSkips++;
        }

        _port = null;
        _harvester = null;
        _inputCount += count;
        _input.Clear();
        _inputBase = _inputCount;
        _nextOutput = _factor <= 0 ? 0 : (_inputCount + _factor - 1) / _factor;

        return found;
    }

    /// <summary>The end of the audio: the last outputs made with the file's end as silence, as the adapter makes them, and read.</summary>
    /// <returns>The port's readings completed by the end.</returns>
    public IReadOnlyList<CwSecondReading> Flush()
    {
        if (Unavailable is not null || _port is null)
        {
            return Array.Empty<CwSecondReading>();
        }

        Feed(true);

        return _harvester!.Take(_port);
    }

    private void Build(double pitchHz)
    {
        _port = new FldigiCwDecoder(pitchHz) { TraceDecisions = true };
        _harvester = new CwSecondHarvester(_nextOutput / (double)FldigiCwDecoder.CW_SAMPLERATE);
        Constructions++;
    }

    private static bool Quiet(FldigiCwDecoder port)
    {
        var rows = port.Decisions;

        for (var i = Math.Max(0, rows.Count - QuietRows); i < rows.Count; i++)
        {
            if (rows[i].Events.Length > 0)
            {
                return false;
            }
        }

        return true;
    }

    // Makes every 8000 Hz output whose inputs have arrived (or, at the end, every one the adapter would make) and gives them to the port.
    private void Feed(bool atEnd)
    {
        var half = Taps / 2;
        var outputs = new List<double>();

        if (_factor == 1)
        {
            for (; _nextOutput < _inputCount; _nextOutput++)
            {
                outputs.Add(_input[(int)(_nextOutput - _inputBase)]);
            }
        }
        else
        {
            var ready = _inputCount - 1 - half;
            var last = atEnd ? _inputCount / _factor : ready < 0 ? 0 : (ready / _factor) + 1;

            for (; _nextOutput < last; _nextOutput++)
            {
                var centre = _nextOutput * _factor;
                var sum = 0.0;

                for (var t = 0; t < Taps; t++)
                {
                    var at = centre + t - half;

                    if (at >= _inputBase && at < _inputCount)
                    {
                        sum += _taps[t] * _input[(int)(at - _inputBase)];
                    }
                }

                outputs.Add(sum);
            }
        }

        if (outputs.Count > 0)
        {
            _port!.rx_process(outputs.ToArray());
        }

        // Keep only what a later output still reaches back to.
        var keepFrom = Math.Max(_inputBase, (_nextOutput * Math.Max(1, _factor)) - half);
        var drop = (int)Math.Min(_input.Count, keepFrom - _inputBase);

        if (drop > 0)
        {
            _input.RemoveRange(0, drop);
            _inputBase += drop;
        }
    }

    // FldigiRateAdapter.Kernel, the same arithmetic in the same order.
    private static double[] Kernel(double cutoff)
    {
        var taps = new double[Taps];
        var half = Taps / 2;
        var total = 0.0;

        for (var i = 0; i < Taps; i++)
        {
            var x = i - half;
            var sinc = x == 0 ? 2 * cutoff : Math.Sin(2 * Math.PI * cutoff * x) / (Math.PI * x);
            var window = 0.42 - (0.5 * Math.Cos(2 * Math.PI * i / (Taps - 1))) + (0.08 * Math.Cos(4 * Math.PI * i / (Taps - 1)));
            taps[i] = sinc * window;
            total += taps[i];
        }

        for (var i = 0; i < Taps; i++) taps[i] /= total;

        return taps;
    }
}
