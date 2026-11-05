using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **THE LEARNED READER** (work instruction 597, HM-DEC-302): a small trained net (a convolutional front end, two bidirectional GRU layers,
/// CTC) reads letters straight from the audio. It was trained on synthetic keying only and has heard no recording. This class is
/// <c>Cw\Model\CWNET_SPEC.md</c> and nothing else: nothing in the features, the alphabet or the decode is tuned here.
/// </summary>
/// <remarks>
/// <para>**HOW IT REACHES THE SCREEN.** A letter leaves as its dots, dashes and letter end, the stream the sequence reader hands the lookup
/// table, so the terminal, the scope and the record read it unchanged.</para>
/// <para>**THE NET HAS NO MARKS AND NO PITCH.** So that a letter can carry its evidence (§0.0.1), and be scored at all, each carries one
/// <see cref="CwMark"/> measured after the net has spoken: its span runs back from the frame that named it to the last letter, 0.6 s at most,
/// and its pitch is the strongest bin of the features over that span. Neither decides any letter. The letter's score is the net's own
/// probability at that frame, dimmed under one half as the sequence reader dims its own.</para>
/// </remarks>
public sealed class CwLearnedReader
{
    /// <summary>The net's sample rate (spec section 1).</summary>
    public const int NetRate = 8000;

    private const int FrameLength = 256;
    private const int FrameHop = 80;
    private const int FirstBin = 8;
    private const int BinCount = 26;
    private const double HzPerBin = NetRate / (double)FrameLength;

    private const int WindowSamples = 12 * NetRate;
    private const int RunEverySamples = NetRate;
    private const int LeastWindowSamples = NetRate;
    private const int CommitLagSamples = 2 * NetRate;
    private const int SamplesPerOutputFrame = 160;
    private const int OutputSize = 47;

    // How far back from the frame that named it a letter's span reaches, and how far past it.
    private const double SpanBackSeconds = 0.6;
    private const double SpanOnSeconds = 0.04;

    // The output alphabet, index 1 on (spec section 3); index 0 is the CTC blank.
    private const string Alphabet = " !&(+,./0123456789=?ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private static readonly IReadOnlyDictionary<string, string> PatternOfText =
        MorseAlphabet.All.GroupBy(p => p.Value, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().Key, StringComparer.Ordinal);

    // The periodic Hann window times each kept bin's cosine and sine, over 128 (spec section 2, step 1).
    private static readonly double[][] Cosines = Kernel(Math.Cos);
    private static readonly double[][] Sines = Kernel(Math.Sin);

    // **LOADED ONCE**, one thread (spec section 4). A session may be run from several threads at once.
    private static readonly Lazy<InferenceSession> Net = new(() =>
    {
        using var options = new SessionOptions
        {
            IntraOpNumThreads = 1,
            InterOpNumThreads = 1,
            ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
        };

        return new InferenceSession(Path.Combine(AppContext.BaseDirectory, "Cw", "Model", "cwnet.onnx"), options);
    });

    // The anti-alias filter, or null at the net's own rate.
    private readonly float[]? _taps;
    private readonly float[] _history = [];
    private readonly bool _everyNth;
    private readonly int _nth;
    private readonly double _step;
    private readonly double _delaySeconds;
    private int _historyAt;
    private long _heard;
    private double _nextOut;
    private float _lastFiltered;

    // The last twelve seconds at the net's rate, and how many samples there have been in all.
    private readonly float[] _ring = new float[WindowSamples];
    private long _now;

    private readonly List<CwSymbol> _out = [];
    private long _committedUpTo;
    private int _prev;
    private bool _letterSinceSpace;
    private double _lastLetterSeconds = double.NegativeInfinity;

    /// <summary>Creates the reader.</summary>
    /// <param name="sampleRate">The rate of the audio it will be pushed.</param>
    public CwLearnedReader(int sampleRate)
    {
        if (sampleRate == NetRate)
        {
            return;
        }

        // A Blackman windowed sinc, cut off at 3.4 kHz (spec section 1), its transition 800 Hz wide.
        var halfLength = (int)Math.Ceiling(2.75 * sampleRate / 800.0);
        var cutoff = Math.Min(3400.0, 0.425 * sampleRate) / sampleRate;
        var taps = new double[(2 * halfLength) + 1];

        for (var i = 0; i < taps.Length; i++)
        {
            var n = i - halfLength;
            var sinc = n == 0 ? 2 * cutoff : Math.Sin(2 * Math.PI * cutoff * n) / (Math.PI * n);
            var phase = 2 * Math.PI * i / (taps.Length - 1);

            taps[i] = sinc * (0.42 - (0.5 * Math.Cos(phase)) + (0.08 * Math.Cos(2 * phase)));
        }

        var sum = taps.Sum();

        _taps = taps.Select(t => (float)(t / sum)).ToArray();
        _history = new float[2 * _taps.Length];
        _delaySeconds = halfLength / (double)sampleRate;
        _step = sampleRate / (double)NetRate;
        _everyNth = sampleRate % NetRate == 0;
        _nth = sampleRate / NetRate;
    }

    /// <summary>Hear one sample, as it arrives.</summary>
    /// <param name="sample">The sample.</param>
    public void Push(float sample)
    {
        if (_taps is null)
        {
            Add(sample);
            return;
        }

        // Written twice, so the newest stretch is always contiguous.
        _history[_historyAt] = sample;
        _history[_historyAt + _taps.Length] = sample;
        _historyAt = (_historyAt + 1) % _taps.Length;
        _heard++;

        if (_everyNth)
        {
            if ((_heard - 1) % _nth == 0)
            {
                Add(Filtered());
            }

            return;
        }

        // A rate that is no multiple of the net's: between two filtered samples, by line.
        var filtered = Filtered();

        while (_nextOut <= _heard - 1)
        {
            Add(_lastFiltered + ((filtered - _lastFiltered) * (float)(_nextOut - (_heard - 2))));
            _nextOut += _step;
        }

        _lastFiltered = filtered;
    }

    /// <summary>What was committed since the last call.</summary>
    /// <param name="final">True where the audio is over and everything is committed.</param>
    /// <returns>Dots, dashes, letter ends and word ends.</returns>
    public IReadOnlyList<CwSymbol> Take(bool final)
    {
        if (final)
        {
            Run(final: true);
        }

        if (_out.Count == 0)
        {
            return [];
        }

        var symbols = _out.ToArray();

        _out.Clear();

        return symbols;
    }

    /// <summary>The feature matrix of a window (spec section 2), row after row.</summary>
    /// <param name="x">The window at the net's rate.</param>
    /// <returns>The matrix, its frame count, and the scale it was divided by.</returns>
    internal static (float[] Features, int Frames, double Scale) Features(ReadOnlySpan<float> x)
    {
        var frames = ((x.Length - FrameLength) / FrameHop) + 1;
        var log = new double[frames * BinCount];
        var row = new double[BinCount];

        for (var t = 0; t < frames; t++)
        {
            var frame = x.Slice(t * FrameHop, FrameLength);

            for (var b = 0; b < BinCount; b++)
            {
                var cosine = Cosines[b];
                var sine = Sines[b];
                double re = 0, im = 0;

                for (var n = 0; n < FrameLength; n++)
                {
                    re += cosine[n] * frame[n];
                    im += sine[n] * frame[n];
                }

                row[b] = 10 * Math.Log10((re * re) + (im * im) + 1e-10);
            }

            // The receiver's broadband gain: each row less its median over the bins.
            var sorted = row.ToArray();

            Array.Sort(sorted);

            var median = (sorted[(BinCount / 2) - 1] + sorted[BinCount / 2]) / 2;

            for (var b = 0; b < BinCount; b++)
            {
                log[(t * BinCount) + b] = row[b] - median;
            }
        }

        // The noise floor's shape: each column less its 10th percentile over the frames.
        var column = new double[frames];

        for (var b = 0; b < BinCount; b++)
        {
            for (var t = 0; t < frames; t++)
            {
                column[t] = log[(t * BinCount) + b];
            }

            Array.Sort(column);

            var floor = Percentile(column, 0.10);

            for (var t = 0; t < frames; t++)
            {
                log[(t * BinCount) + b] -= floor;
            }
        }

        // One scale over the whole matrix.
        var all = log.ToArray();

        Array.Sort(all);

        var spread = Percentile(all, 0.97) - Percentile(all, 0.10);
        var features = new float[log.Length];

        for (var i = 0; i < log.Length; i++)
        {
            features[i] = (float)(log[i] / (spread + 3));
        }

        return (features, frames, spread + 3);
    }

    /// <summary>The net's scores per output frame for a feature matrix (spec section 3), row after row of <c>OutputSize</c> values.</summary>
    /// <param name="features">The matrix.</param>
    /// <param name="frames">Its frame count.</param>
    /// <returns>The scores and the output frame count.</returns>
    internal static (float[] Logits, int Frames) Logits(float[] features, int frames)
    {
        var input = new DenseTensor<float>(features, [1, frames, BinCount]);

        using var results = Net.Value.Run([NamedOnnxValue.CreateFromTensor("feat", input)]);

        var logits = results.First(r => r.Name == "logits").AsTensor<float>();

        return (logits.ToArray(), logits.Dimensions[1]);
    }

    /// <summary>The index the net scores highest at an output frame.</summary>
    /// <param name="logits">The scores.</param>
    /// <param name="frame">The output frame.</param>
    /// <returns>The index, 0 the blank.</returns>
    internal static int Best(float[] logits, int frame)
    {
        var at = frame * OutputSize;
        var best = 0;

        for (var i = 1; i < OutputSize; i++)
        {
            if (logits[at + i] > logits[at + best])
            {
                best = i;
            }
        }

        return best;
    }

    // NumPy's default percentile over sorted values (spec section 2, step 5).
    private static double Percentile(double[] sorted, double share)
    {
        var position = share * (sorted.Length - 1);
        var low = (int)Math.Floor(position);
        var high = Math.Min(low + 1, sorted.Length - 1);

        return sorted[low] + ((position - low) * (sorted[high] - sorted[low]));
    }

    private static double[][] Kernel(Func<double, double> wave)
    {
        var kernel = new double[BinCount][];

        for (var b = 0; b < BinCount; b++)
        {
            kernel[b] = new double[FrameLength];

            for (var n = 0; n < FrameLength; n++)
            {
                var hann = 0.5 - (0.5 * Math.Cos(2 * Math.PI * n / FrameLength));

                kernel[b][n] = hann * wave(2 * Math.PI * (FirstBin + b) * n / FrameLength) / 128;
            }
        }

        return kernel;
    }

    private float Filtered()
    {
        var taps = _taps!;
        var from = _historyAt;
        var sum = 0f;

        for (var i = 0; i < taps.Length; i++)
        {
            sum += taps[i] * _history[from + i];
        }

        return sum;
    }

    private void Add(float sample)
    {
        _ring[(int)(_now % WindowSamples)] = sample;
        _now++;

        if (_now % RunEverySamples == 0)
        {
            Run(final: false);
        }
    }

    // One run of the sliding window (spec section 4): the last twelve seconds, or all there is, at least a second.
    private void Run(bool final)
    {
        var start = Math.Max(0, _now - WindowSamples);

        // The window after the last whole second, at the end of the audio: its frames must still fall on the stream's.
        if (start % SamplesPerOutputFrame != 0)
        {
            start += SamplesPerOutputFrame - (start % SamplesPerOutputFrame);
        }

        var length = (int)(_now - start);

        if (length < LeastWindowSamples)
        {
            return;
        }

        var window = new float[length];

        for (var i = 0; i < length; i++)
        {
            window[i] = _ring[(int)((start + i) % WindowSamples)];
        }

        var (features, frames, scale) = Features(window);
        var (logits, outputs) = Logits(features, frames);
        var first = start / SamplesPerOutputFrame;

        for (; _committedUpTo - first < outputs; _committedUpTo++)
        {
            if (!final && (_committedUpTo * SamplesPerOutputFrame) + SamplesPerOutputFrame > _now - CommitLagSamples)
            {
                break;
            }

            Commit(_committedUpTo, (int)(_committedUpTo - first), start, features, frames, scale, logits);
        }
    }

    // The greedy collapse over the committed stream (spec section 3): `_prev` carries across runs, so a letter spanning two is emitted once.
    private void Commit(long absolute, int frame, long start, float[] features, int frames, double scale, float[] logits)
    {
        var best = Best(logits, frame);
        var changed = best != _prev;

        _prev = best;

        if (!changed || best == 0)
        {
            return;
        }

        var seconds = Math.Max(0, (absolute * SamplesPerOutputFrame / (double)NetRate) - _delaySeconds);

        if (best == 1)
        {
            // A word end opens the next word, as the sequence reader's does, and one is enough for the terminal's spacing.
            if (_letterSinceSpace)
            {
                _out.Add(new CwSymbol(CwSymbolKind.WordEnd)
                {
                    Reading = new CwCharacter(MorseAlphabet.WordGap, CwConfidence.High, 1, string.Empty, double.NaN, 0, TimeSpan.FromSeconds(seconds))
                    {
                        Stage = CwReadingStage.Settled,
                    },
                });
                _letterSinceSpace = false;
            }

            return;
        }

        Letter(best, seconds, frame, start, features, frames, scale, logits);
        _letterSinceSpace = true;
    }

    private void Letter(int index, double seconds, int frame, long start, float[] features, int frames, double scale, float[] logits)
    {
        var text = Alphabet[index - 1] switch
        {
            '!' => "<SK>",
            '&' => "<AS>",
            '(' => "<KN>",
            '+' => "<AR>",
            '=' => "<BT>",
            var c => c.ToString(),
        };
        var pattern = PatternOfText.TryGetValue(text, out var p) ? p : string.Empty;
        var from = Math.Max(Math.Max(0, seconds - SpanBackSeconds), _lastLetterSeconds);
        var to = seconds + SpanOnSeconds;
        var (pitch, levelDb) = Pitch(from, to, start, features, frames, scale);
        var score = Probability(logits, frame, index);

        _lastLetterSeconds = seconds;

        foreach (var element in pattern)
        {
            _out.Add(new CwSymbol(element == '-' ? CwSymbolKind.Dash : CwSymbolKind.Dot));
        }

        _out.Add(new CwSymbol(CwSymbolKind.LetterEnd)
        {
            Reading = new CwCharacter(MorseAlphabet.Unreadable, score >= 0.5 ? CwConfidence.High : CwConfidence.Low, score, pattern, levelDb, 0, TimeSpan.FromSeconds(to))
            {
                Stage = CwReadingStage.Settled,
            },
            Marks = [new CwMark(0, from, to, pitch, levelDb, levelDb) { Keyed = true, Stood = true }],
        });
    }

    // The strongest bin over a span of the window, between bins by the parabola through its neighbours, and how far it stands over its floor in dB.
    private (double PitchHz, double LevelDb) Pitch(double from, double to, long start, float[] features, int frames, double scale)
    {
        var first = Math.Clamp((int)(((from + _delaySeconds) * NetRate) - start) / FrameHop, 0, frames - 1);
        var last = Math.Clamp((int)(((to + _delaySeconds) * NetRate) - start) / FrameHop, first, frames - 1);
        var sums = new double[BinCount];

        for (var t = first; t <= last; t++)
        {
            for (var b = 0; b < BinCount; b++)
            {
                sums[b] += features[(t * BinCount) + b];
            }
        }

        var peak = Array.IndexOf(sums, sums.Max());
        var offset = 0.0;

        if (peak > 0 && peak < BinCount - 1)
        {
            var curve = sums[peak - 1] - (2 * sums[peak]) + sums[peak + 1];

            offset = curve < 0 ? Math.Clamp(0.5 * (sums[peak - 1] - sums[peak + 1]) / curve, -0.5, 0.5) : 0;
        }

        return ((FirstBin + peak + offset) * HzPerBin, sums[peak] / (last - first + 1) * scale);
    }

    private static double Probability(float[] logits, int frame, int index)
    {
        var at = frame * OutputSize;
        var top = double.NegativeInfinity;

        for (var i = 0; i < OutputSize; i++)
        {
            top = Math.Max(top, logits[at + i]);
        }

        var sum = 0.0;

        for (var i = 0; i < OutputSize; i++)
        {
            sum += Math.Exp(logits[at + i] - top);
        }

        return Math.Exp(logits[at + index] - top) / sum;
    }
}
