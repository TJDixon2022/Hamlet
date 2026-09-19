using System.Text;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Olivia;

/// <summary>One block an <see cref="OliviaStream"/> read, shown or not.</summary>
/// <param name="Seconds">Where the block began, from the stream's first sample.</param>
/// <param name="Snr">Its signal-to-noise in the demodulator's units.</param>
/// <param name="Accepted">True where it cleared the threshold and its characters were shown.</param>
/// <param name="Characters">How many characters it added to the text: none where it was not accepted.</param>
/// <param name="OffsetHz">How far from the center's tones it was read.</param>
/// <remarks>**NO TEXT** (HM-DEC-018): the block's characters are in the stream's text and never here.</remarks>
public readonly record struct OliviaBlock(double Seconds, double Snr, bool Accepted, int Characters, double OffsetHz);

/// <summary>
/// **Reads Olivia as it arrives**: the demodulator's five steps, each kept and advanced rather than
/// run over a whole recording.
/// </summary>
/// <remarks>
/// <para>**WORK INSTRUCTION 363 DECISION Z.** <see cref="OliviaDemodulator.Decode(MonoAudio, double)"/> looks at every
/// frame of a recording before it reads a block: the offset track is backtracked from the last
/// segment, the noise is the median of every frame, and the sync is the best sum over every block.
/// None of that can wait for the end of a stream, so each is held as running state instead:</para>
/// <para>- **every analysis frame is transformed once**, as its window of audio arrives;</para>
/// <para>- **the offset track is decided a segment at a time**, forward only: a segment's figure is
/// its own power and <see cref="OliviaDemodulator.TrackSmoothing"/> segments either side, as in
/// <c>Decode</c>, so a segment is decided once the segments after it have arrived, and the step
/// between segments is still one grid step;</para>
/// <para>- **the noise and the signal are measured over the decided segments nearest the one being
/// read** - the smoothing's width of them, never the whole history;</para>
/// <para>- **the sync is the same sum of every block's signal-to-noise at every frame and block
/// phase**, advanced as each block completes rather than summed at the end; and a block is read, at
/// the sync that leads now, as soon as its last frame is in.</para>
/// <para>**A BLOCK OR NOTHING** (`PHASE_PLAN.md` §3.3, PSK31 plan §R9): a block that does not clear
/// the threshold shows nothing, and a block is never read twice or out of order - once one is
/// shown, no block that starts inside it is read.</para>
/// <para>**IT HEARS AND DOES NOTHING ELSE** (§0.2).</para>
/// </remarks>
public sealed class OliviaStream
{
    private readonly OliviaDemodulator _demodulator;
    private readonly OliviaFormat _format;
    private readonly OliviaVariant _variant;
    private readonly int _rate;
    private readonly int _symbolSamples;
    private readonly int _window;
    private readonly int _hop;
    private readonly int _size;
    private readonly double _binHz;
    private readonly double _binsPerTone;
    private readonly int _reach;
    private readonly int _firstBin;
    private readonly int _pad;
    private readonly int _lowBin;
    private readonly int _span;
    private readonly int _width;
    private readonly int _segment;
    private readonly int _perBlock;
    private readonly int _blockFrames;
    private readonly int _tones;
    private readonly int _bits;
    private readonly RealFft _fft;
    private readonly double[] _magnitudes;
    private readonly double[] _real;
    private readonly double[] _imaginary;
    private readonly float[] _shaped;
    private readonly double[] _shape;
    private readonly OliviaDemodulator.BlockDecoder _decoder;

    private float[] _buffer = new float[1 << 14];
    private long _bufferStart;
    private int _bufferCount;
    private long _samples;
    private long _frames;
    private bool _flushed;

    // The offset track: power rows of undecided segments, each segment's raw figure, the forward
    // figures of the last decided segment, and each decided segment's offset.
    private readonly List<double[]> _power = new();
    private long _powerBase;
    private readonly List<double[]> _raw = new();
    private long _rawBase;
    private double[]? _best;
    private readonly List<int> _path = new();
    private readonly Queue<double[]> _recentEnergies = new();

    // The soft values and the likelihoods of every decided frame still needed.
    private readonly List<double[]> _soft = new();
    private readonly List<double[]> _metrics = new();
    private long _softBase;

    // The sync, summed as blocks complete, and where each frame phase has summed to.
    private readonly double[,] _sums;
    private readonly long[] _nextSymbol;

    // The reading.
    private readonly StringBuilder _text = new();
    private readonly HashSet<long> _tried = new();
    private readonly List<OliviaBlock> _blocks = new();
    private long _readFrom;
    private double _snrSum;

    internal OliviaStream(OliviaDemodulator demodulator)
    {
        _demodulator = demodulator;
        _format = demodulator.Format;
        _variant = demodulator.Variant;
        _rate = demodulator.SampleRate;
        _symbolSamples = demodulator.SamplesPerSymbol;
        _window = _format.AnalysisWindowSymbols * _symbolSamples;
        _hop = _symbolSamples / OliviaDemodulator.FramesPerSymbol;
        _size = OliviaDemodulator.PaddingFactor * (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)_window);
        _binHz = (double)_rate / _size;
        _binsPerTone = _variant.ToneSpacingHz / _binHz;
        _reach = (int)Math.Floor(_binsPerTone / 2);
        _firstBin = (int)Math.Round((demodulator.CenterHz + _variant.FirstToneOffsetHz) / _binHz);

        var tonesSpan = (int)Math.Round((_variant.Tones - 1) * _binsPerTone);

        // The same grid as Decode, so the same frames give the same figures.
        _pad = Math.Max(_reach, Math.Min(
            _reach + (int)Math.Floor(OliviaDemodulator.TrackTones * _binsPerTone),
            Math.Min(_firstBin - 1, (_size / 2) - 1 - (_firstBin + tonesSpan))));
        _lowBin = _firstBin - _pad;
        _span = tonesSpan + (2 * _pad) + 1;
        _width = (2 * _pad) + 1;
        _perBlock = _format.SymbolsPerBlock;
        _segment = _perBlock * OliviaDemodulator.FramesPerSymbol;
        _blockFrames = _perBlock * OliviaDemodulator.FramesPerSymbol;
        _tones = _variant.Tones;
        _bits = _variant.BitsPerSymbol;
        _fft = new RealFft(_size);
        _magnitudes = new double[_fft.BinCount];
        _real = new double[_size];
        _imaginary = new double[_size];
        _shaped = new float[_size];
        _shape = new double[_window];

        for (var i = 0; i < _window; i++)
        {
            _shape[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / _window));
        }

        _decoder = new OliviaDemodulator.BlockDecoder(_format, _bits, demodulator.ToneToSymbol);
        _sums = new double[OliviaDemodulator.FramesPerSymbol, _perBlock];
        _nextSymbol = new long[OliviaDemodulator.FramesPerSymbol];
    }

    /// <summary>The variant being read.</summary>
    public OliviaVariant Variant => _variant;

    /// <summary>The center the reader was made for.</summary>
    public double CenterHz => _demodulator.CenterHz;

    /// <summary>Every character of every block shown, in order.</summary>
    public string Text => _text.ToString();

    /// <summary>How many characters <see cref="Text"/> holds.</summary>
    public int CharactersOut => _text.Length;

    /// <summary>Blocks that cleared the threshold.</summary>
    public int BlocksDecoded { get; private set; }

    /// <summary>Blocks read at the leading sync that did not.</summary>
    public int BlocksRejected { get; private set; }

    /// <summary>The mean signal-to-noise of the blocks shown, or 0 where none was.</summary>
    public double SyncSnr => BlocksDecoded == 0 ? 0 : _snrSum / BlocksDecoded;

    /// <summary>The signal-to-noise of the last block read, shown or not, or 0 before any.</summary>
    public double LastBlockSnr { get; private set; }

    /// <summary>True while the last block read was shown.</summary>
    public bool InSync { get; private set; }

    /// <summary>The best block's signal-to-noise so far, shown or not.</summary>
    public double HighestBlockSnr { get; private set; }

    /// <summary>How far from the center's tones the last decided segment put them.</summary>
    public double OffsetHz => _path.Count == 0 ? 0 : OffsetOf(_path[^1]);

    /// <summary>Samples fed.</summary>
    public long SamplesSeen => _samples;

    /// <summary>Hand the reader the next piece of audio.</summary>
    /// <param name="samples">The samples, at the demodulator's rate.</param>
    /// <exception cref="InvalidOperationException">The stream was flushed.</exception>
    public void Add(ReadOnlySpan<float> samples)
    {
        if (_flushed)
        {
            throw new InvalidOperationException("the stream has ended");
        }

        Append(samples);
        _samples += samples.Length;
        Advance();
    }

    /// <summary>
    /// The audio has ended: read what is left, the last window running past the end read as
    /// silence, as <see cref="OliviaDemodulator.Decode(MonoAudio, double)"/> reads it.
    /// </summary>
    public void Flush()
    {
        if (_flushed)
        {
            return;
        }

        _flushed = true;
        Advance();
    }

    /// <summary>The blocks read since the last call, oldest first.</summary>
    /// <returns>The blocks.</returns>
    public IReadOnlyList<OliviaBlock> DrainBlocks()
    {
        var blocks = _blocks.ToArray();

        _blocks.Clear();

        return blocks;
    }

    private long TotalFrames => _samples == 0 ? 0 : ((_samples - 1) / _hop) + 1;

    private void Append(ReadOnlySpan<float> samples)
    {
        var keepFrom = _frames * _hop;
        var drop = (int)Math.Clamp(keepFrom - _bufferStart, 0, _bufferCount);

        if (drop > 0)
        {
            Array.Copy(_buffer, drop, _buffer, 0, _bufferCount - drop);
            _bufferCount -= drop;
            _bufferStart += drop;
        }

        if (_bufferCount + samples.Length > _buffer.Length)
        {
            Array.Resize(ref _buffer, Math.Max(_buffer.Length * 2, _bufferCount + samples.Length));
        }

        samples.CopyTo(_buffer.AsSpan(_bufferCount));
        _bufferCount += samples.Length;
    }

    private void Advance()
    {
        // 1. Every frame whose window has arrived - or, once flushed, every frame that starts
        // inside the audio.
        while (_flushed ? _frames < TotalFrames : (_frames * _hop) + _window <= _samples)
        {
            Frame();
        }

        // 2. Every segment whose neighbors have arrived is decided, and its frames get their values.
        var segments = _frames == 0 ? 0 : (int)(((_frames - 1) / _segment) + 1);
        var complete = _flushed ? segments : (int)(_frames / _segment);

        while (_path.Count < segments && (_flushed || _path.Count + OliviaDemodulator.TrackSmoothing < complete))
        {
            Decide(_path.Count, _flushed ? segments - 1 : _path.Count + OliviaDemodulator.TrackSmoothing);
        }

        // 3 and 4. The sync, advanced; then every block the leading sync now has whole.
        Sync();
        Read();
        Trim();
    }

    private void Frame()
    {
        var at = (int)((_frames * _hop) - _bufferStart);

        for (var i = 0; i < _window; i++)
        {
            _shaped[i] = at + i < _bufferCount ? (float)(_buffer[at + i] * _shape[i]) : 0f;
        }

        _fft.Magnitudes(_shaped, _magnitudes, _real, _imaginary);

        var row = new double[_span];

        for (var b = 0; b < _span; b++)
        {
            var m = _magnitudes[_lowBin + b];
            row[b] = m * m;
        }

        _power.Add(row);

        // The segment's raw figure, as Decode's track sums it.
        var s = _frames / _segment;

        while (_rawBase + _raw.Count <= s)
        {
            _raw.Add(new double[_width]);
        }

        var raw = _raw[(int)(s - _rawBase)];

        for (var o = -_pad; o <= _pad; o++)
        {
            double loudest = 0;

            for (var k = 0; k < _tones; k++)
            {
                loudest = Math.Max(loudest, row[Column(k, o)]);
            }

            raw[o + _pad] += loudest;
        }

        _frames++;
    }

    private void Decide(int s, int last)
    {
        var score = new double[_width];

        for (var n = Math.Max(0, s - OliviaDemodulator.TrackSmoothing); n <= last; n++)
        {
            var raw = _raw[(int)(n - _rawBase)];

            for (var i = 0; i < _width; i++)
            {
                score[i] += raw[i];
            }
        }

        var best = new double[_width];

        for (var i = 0; i < _width; i++)
        {
            if (_best is null)
            {
                best[i] = Math.Abs(i - _pad) <= _reach ? score[i] : double.NegativeInfinity;
                continue;
            }

            best[i] = double.NegativeInfinity;

            for (var j = Math.Max(0, i - 1); j <= Math.Min(_width - 1, i + 1); j++)
            {
                var candidate = _best[j] + score[i];

                if (candidate > best[i] || (candidate == best[i] && j == i))
                {
                    best[i] = candidate;
                }
            }
        }

        var chosen = 0;

        for (var i = 1; i < _width; i++)
        {
            if (best[i] > best[chosen])
            {
                chosen = i;
            }
        }

        _best = best;
        _path.Add(chosen - _pad);

        // The segment's frames, at the decided offset.
        var from = (long)s * _segment;
        var to = Math.Min((long)(s + 1) * _segment, _frames);
        var count = (int)(to - from);
        var energies = new double[count * _tones];

        for (var f = 0; f < count; f++)
        {
            var row = _power[(int)(from + f - _powerBase)];

            for (var k = 0; k < _tones; k++)
            {
                energies[(f * _tones) + k] = row[Column(k, chosen - _pad)];
            }
        }

        // The noise and the signal over the decided segments nearest this one.
        _recentEnergies.Enqueue(energies);

        while (_recentEnergies.Count > (2 * OliviaDemodulator.TrackSmoothing) + 1)
        {
            _recentEnergies.Dequeue();
        }

        var pooled = _recentEnergies.SelectMany(e => e).ToArray();
        var (noise, signal) = OliviaDemodulator.NoiseAndSignal(pooled, pooled.Length / _tones, _tones);

        for (var f = 0; f < count; f++)
        {
            var metric = new double[_tones];

            for (var k = 0; k < _tones; k++)
            {
                metric[k] = OliviaDemodulator.LogBesselI0(2 * Math.Sqrt(signal * energies[(f * _tones) + k] / noise));
            }

            var values = new double[_bits];

            for (var b = 0; b < _bits; b++)
            {
                var zero = double.NegativeInfinity;
                var one = double.NegativeInfinity;

                for (var k = 0; k < _tones; k++)
                {
                    if (((_demodulator.ToneToSymbol[k] >> b) & 1) == 0)
                    {
                        zero = OliviaDemodulator.LogAdd(zero, metric[k]);
                    }
                    else
                    {
                        one = OliviaDemodulator.LogAdd(one, metric[k]);
                    }
                }

                values[b] = _format.NegativeSetsTheBit ? zero - one : one - zero;
            }

            _metrics.Add(metric);
            _soft.Add(values);
        }

        // What is decided is not needed as power any more, nor raw figures no later segment sums.
        var drop = (int)Math.Min(_power.Count, to - _powerBase);

        _power.RemoveRange(0, drop);
        _powerBase += drop;

        var rawDrop = (int)Math.Clamp(s + 1 - OliviaDemodulator.TrackSmoothing - _rawBase, 0, _raw.Count);

        _raw.RemoveRange(0, rawDrop);
        _rawBase += rawDrop;
    }

    private long SoftEnd => _softBase + _soft.Count;

    private void Sync()
    {
        var stride = OliviaDemodulator.FramesPerSymbol;

        for (var phase = 0; phase < stride; phase++)
        {
            while (phase + ((_nextSymbol[phase] + _perBlock - 1) * stride) < SoftEnd)
            {
                var first = phase + (_nextSymbol[phase] * stride);

                if (first >= _softBase)
                {
                    _sums[phase, (int)(_nextSymbol[phase] % _perBlock)] += _decoder.Decode(Frames(_soft, first), 0, 1).Snr;
                }

                _nextSymbol[phase]++;
            }
        }
    }

    private void Read()
    {
        var stride = OliviaDemodulator.FramesPerSymbol;
        var bestPhase = 0;
        var bestBlock = 0;
        var bestSum = double.MinValue;

        for (var phase = 0; phase < stride; phase++)
        {
            for (var block = 0; block < _perBlock; block++)
            {
                if (_sums[phase, block] > bestSum)
                {
                    bestSum = _sums[phase, block];
                    bestPhase = phase;
                    bestBlock = block;
                }
            }
        }

        if (bestSum <= 0)
        {
            return;
        }

        // The earliest block at the leading sync that starts after the last one shown - less the
        // fraction of a symbol by which a new frame phase may sit ahead of the old one.
        var floor = Math.Max(_softBase, _readFrom - stride + 1);
        var symbol = (long)Math.Ceiling((floor - bestPhase) / (double)stride);

        symbol += ((bestBlock - (symbol % _perBlock)) + _perBlock) % _perBlock;

        for (var first = bestPhase + (symbol * stride); first + ((_perBlock - 1) * stride) < SoftEnd; first += _blockFrames)
        {
            if (!_tried.Add(first))
            {
                continue;
            }

            var result = _decoder.DecodeIterating(Frames(_metrics, first), 0, 1, OliviaDemodulator.Passes);
            var accepted = result.Snr >= _demodulator.Threshold;
            var characters = 0;

            if (accepted)
            {
                foreach (var character in result.Characters)
                {
                    if (_demodulator.IsText(character))
                    {
                        _text.Append((char)character);
                        characters++;
                    }
                }

                BlocksDecoded++;
                _snrSum += result.Snr;
                _readFrom = first + _blockFrames;
            }
            else
            {
                BlocksRejected++;
            }

            LastBlockSnr = result.Snr;
            HighestBlockSnr = Math.Max(HighestBlockSnr, result.Snr);
            InSync = accepted;

            var segment = (int)Math.Min(first / _segment, _path.Count - 1);

            _blocks.Add(new OliviaBlock(
                ((first * _hop) + (_window / 2.0) - (_symbolSamples / 2.0)) / _rate,
                result.Snr,
                accepted,
                characters,
                OffsetOf(_path[segment])));
        }
    }

    private void Trim()
    {
        // Keep what the sync still has to sum and what a late change of sync could still read:
        // two blocks behind the newest frame.
        var stride = OliviaDemodulator.FramesPerSymbol;
        var needed = SoftEnd - (2 * _blockFrames);

        for (var phase = 0; phase < stride; phase++)
        {
            needed = Math.Min(needed, phase + (_nextSymbol[phase] * stride));
        }

        var drop = (int)Math.Clamp(needed - _softBase, 0, _soft.Count);

        if (drop >= _blockFrames)
        {
            _soft.RemoveRange(0, drop);
            _metrics.RemoveRange(0, drop);
            _softBase += drop;
            _tried.RemoveWhere(f => f < _softBase);
        }
    }

    private double[][] Frames(List<double[]> rows, long first)
    {
        var stride = OliviaDemodulator.FramesPerSymbol;
        var frames = new double[_perBlock][];

        for (var t = 0; t < _perBlock; t++)
        {
            frames[t] = rows[(int)(first + (t * stride) - _softBase)];
        }

        return frames;
    }

    private double OffsetOf(int path)
        => (path * _binHz) + (_firstBin * _binHz) - (_demodulator.CenterHz + _variant.FirstToneOffsetHz);

    private int Column(int tone, int offset) => _pad + offset + (int)Math.Round(tone * _binsPerTone);
}
