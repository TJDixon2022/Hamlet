using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.RadioEngine.Olivia;

/// <summary>What one run of the Olivia demodulator read, and how.</summary>
/// <param name="Text">The characters of every block it was sure of, idle characters left out.</param>
/// <param name="BlocksDecoded">Blocks whose signal-to-noise cleared the threshold, <see cref="OliviaDemodulator.SyncThreshold"/> by default.</param>
/// <param name="BlocksRejected">Blocks at the chosen phase that did not, whose characters were not shown.</param>
/// <param name="CharactersOut">How many characters <paramref name="Text"/> holds.</param>
/// <param name="FrequencyOffsetHz">How far from the tones the center names the tones were found.</param>
/// <param name="SymbolPhase">Which of the analysis frames within a symbol the symbols were read on.</param>
/// <param name="BlockPhase">Which symbol, counted from the start, the first block began on.</param>
/// <param name="SyncSnr">The mean signal-to-noise of the blocks decoded, or 0 where none was.</param>
/// <param name="FirstBlockSeconds">Where the first decoded block began, or NaN where none was.</param>
/// <param name="LastBlockSeconds">Where the last decoded block began, or NaN where none was.</param>
/// <param name="BlockSnrs">Every block's signal-to-noise at the chosen sync, in order, decoded or not.</param>
/// <remarks>
/// **THE TEXT IS FOR THE SCREEN AND NEVER FOR THE RECORD** (HM-DEC-018). The events this run
/// writes carry the counts beside it and not a character of it.
/// </remarks>
public sealed record OliviaDecoding(
    string Text,
    int BlocksDecoded,
    int BlocksRejected,
    int CharactersOut,
    double FrequencyOffsetHz,
    int SymbolPhase,
    int BlockPhase,
    double SyncSnr,
    double FirstBlockSeconds,
    double LastBlockSeconds,
    IReadOnlyList<double> BlockSnrs);

/// <summary>
/// **Reads Olivia text for a named variant at a named center, in Hamlet's own code.**
/// </summary>
/// <remarks>
/// <para>**HAMLET'S OWN, AND EVERY FORMAT FACT IS THE FILE'S** (work instruction 361 decisions H and
/// M, PSK31 plan §R5). The tone count, the spacing, the symbol length, the scrambling code, the
/// shift, the Walsh butterfly and the Gray table all come from <see cref="OliviaFormat"/>;
/// `pj_mfsk.h` was read for what the format is and nothing of its receiver was taken. The rate is
/// the caller's, which in Hamlet is <see cref="Psk31Resampler.TargetSampleRate"/>, the rate the
/// RSID detector is run at.</para>
/// <para>**THE SHAPE, IN FIVE STEPS.** Every eighth of a symbol, one analysis window of audio,
/// raised-cosine shaped as the transmitter shapes a symbol, is transformed with a quarter-bin
/// grid, and the power on every tone is kept for every offset within half a tone of where the
/// center puts them. The offset that puts the most power on the tones is taken. Each tone's power,
/// in units of the noise, becomes a likelihood, and the likelihoods a soft value for each bit the
/// tone carries. For each of the frames in a symbol and each of the block's symbols as a starting
/// point, every block is decoded - de-interleaved, descrambled and correlated against every Walsh
/// function - and the pairing whose blocks stand furthest out of the noise is the sync. At that
/// sync each block is read again, iteratively, and a block whose signal-to-noise clears the
/// threshold gives its characters and a block that does not gives none.</para>
/// <para>**A BLOCK OR NOTHING** (`PHASE_PLAN.md` §3.3, PSK31 plan §R9). There is no per-character
/// maybe: a block the decoder is not sure of shows nothing, and idle characters are not text.</para>
/// <para>**IT HEARS AND DOES NOTHING ELSE** (§0.2). It keys nothing, tunes nothing and changes no
/// mode; what it read is a value handed back.</para>
/// </remarks>
public sealed class OliviaDemodulator
{
    /// <summary>Analysis frames per symbol: an eighth of a symbol between them.</summary>
    /// <remarks>
    /// **THE UNIT'S NUMBER, CHOSEN ON THE -16 dB FIXTURE** (work instruction 361 task 4). The
    /// symbol timing is found to within a sixteenth of a symbol. At four frames the -16 dB file,
    /// every block accepted, read at CER 0.3745; at eight, 0.3267. The clean files, the -10 dB
    /// file and the noise did not move.
    /// </remarks>
    public const int FramesPerSymbol = 8;

    /// <summary>Transform bins per analysis-window bin: the zero padding.</summary>
    /// <remarks>Tones are two window bins apart, so four puts eight grid points between tones and
    /// the offset is found to within about two hertz at every variant this rate carries.</remarks>
    public const int PaddingFactor = 4;

    /// <summary>How many times the characters' codes hand what they learned back to the tones.</summary>
    /// <remarks>
    /// **THE UNIT'S NUMBER, CHOSEN ON THE -16 dB FIXTURE** (work instruction 361 task 4). The sync
    /// search reads every block once; only the blocks at the chosen sync are read iteratively. On
    /// that file, every block accepted, one reading gave CER 0.3865 and three passes 0.3745.
    /// </remarks>
    public const int Passes = 3;

    /// <summary>
    /// The block signal-to-noise a block must clear to be shown, in the decoder's own units.
    /// </summary>
    /// <remarks>
    /// <para>**THE UNIT'S NUMBER, CHOSEN ON THE NOISE-ONLY FIXTURE** (work instruction 361 tasks 3
    /// and 4, a mechanism the arbiter's rules leave to the unit). A block's figure is the mean of
    /// its characters' Walsh peaks over the root of the mean power in the other correlations. Thirty
    /// seconds of pure noise, read as each of the three variants, tops out at 3.48; the weakest
    /// block of the -10 dB file stands at 13.81.</para>
    /// <para>**IT IS NOT LOWERED TO READ THE -16 dB FILE**, because lowering it shows wrong
    /// characters (PSK31 plan §R9): at 3.0 that file reads at CER 0.43 and noise would clear it,
    /// and with every block accepted it still reads at 0.33, so no threshold meets its ceiling.</para>
    /// </remarks>
    public const double SyncThreshold = 4.0;

    private readonly OliviaFormat _format;
    private readonly OliviaVariant _variant;
    private readonly double _centerHz;
    private readonly int _sampleRate;
    private readonly ITelemetry? _telemetry;
    private readonly double _threshold;
    private readonly int[] _toneToSymbol;

    /// <summary>A demodulator for one variant at one center.</summary>
    /// <param name="format">The format's facts.</param>
    /// <param name="variant">The variant, as the RSID detection named it.</param>
    /// <param name="centerHz">The center, as the RSID detection measured it.</param>
    /// <param name="sampleRate">The audio's samples a second.</param>
    /// <param name="telemetry">Where the sync and summary events are written, or null.</param>
    /// <param name="threshold">The block signal-to-noise a block must clear; <see cref="SyncThreshold"/> unless a measurement asks otherwise.</param>
    /// <exception cref="ArgumentNullException">The format or the variant is missing.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The rate cannot carry the variant at that center.</exception>
    public OliviaDemodulator(
        OliviaFormat format,
        OliviaVariant variant,
        double centerHz,
        int sampleRate,
        ITelemetry? telemetry = null,
        double threshold = SyncThreshold)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(variant);

        var top = centerHz - variant.FirstToneOffsetHz + variant.ToneSpacingHz;

        if (sampleRate <= 0 || top >= sampleRate / 2.0 || centerHz + variant.FirstToneOffsetHz - variant.ToneSpacingHz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        _format = format;
        _variant = variant;
        _centerHz = centerHz;
        _sampleRate = sampleRate;
        _telemetry = telemetry;
        _threshold = threshold;
        _toneToSymbol = new int[variant.Tones];

        // **THE GRAY TABLE, TURNED ROUND**: the file says which tone a symbol goes out on, and a
        // receiver needs the symbol a tone came in on.
        var filled = new bool[variant.Tones];

        for (var symbol = 0; symbol < variant.Tones; symbol++)
        {
            var tone = format.SymbolToTone[symbol];

            if (tone >= variant.Tones || filled[tone])
            {
                throw new ArgumentOutOfRangeException(nameof(format), "the Gray table does not map this variant's symbols onto its tones one to one");
            }

            _toneToSymbol[tone] = symbol;
            filled[tone] = true;
        }
    }

    /// <summary>Samples in one symbol at this rate.</summary>
    public int SamplesPerSymbol => (int)Math.Round(_variant.SymbolSeconds * _sampleRate);

    /// <summary>The variant being read.</summary>
    public OliviaVariant Variant => _variant;

    /// <summary>Read a whole recording from a point in it.</summary>
    /// <param name="audio">The recording, at the rate this demodulator was made for.</param>
    /// <param name="startSeconds">Where to begin: the end of the RSID burst's tones.</param>
    /// <returns>What was read.</returns>
    /// <exception cref="ArgumentException">The recording's rate is not this demodulator's.</exception>
    public OliviaDecoding Decode(MonoAudio audio, double startSeconds)
    {
        ArgumentNullException.ThrowIfNull(audio);

        if (audio.SampleRate != _sampleRate)
        {
            throw new ArgumentException("the recording is at " + audio.SampleRate + " Hz and this demodulator at " + _sampleRate, nameof(audio));
        }

        var start = Math.Clamp((int)Math.Ceiling(startSeconds * _sampleRate), 0, audio.Samples.Length);

        return Decode(audio.Samples.AsSpan(start), start / (double)_sampleRate);
    }

    private OliviaDecoding Decode(ReadOnlySpan<float> samples, double startSeconds)
    {
        var symbolSamples = SamplesPerSymbol;
        var window = _format.AnalysisWindowSymbols * symbolSamples;
        var hop = symbolSamples / FramesPerSymbol;
        var size = PaddingFactor * (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)window);
        var binHz = (double)_sampleRate / size;
        var binsPerTone = _variant.ToneSpacingHz / binHz;
        var reach = (int)Math.Floor(binsPerTone / 2);
        var firstBin = (int)Math.Round((_centerHz + _variant.FirstToneOffsetHz) / binHz);
        var lowBin = firstBin - reach;
        var span = (int)Math.Round((_variant.Tones - 1) * binsPerTone) + (2 * reach) + 1;
        // **THE LAST SYMBOL'S WINDOW RUNS PAST THE END OF THE AUDIO**, because the transmitter
        // stops at the end of its last symbol period and the window is two of them. What lies past
        // the end is read as silence, which is what it is, rather than dropping the last block.
        var frames = samples.Length == 0 ? 0 : ((samples.Length - 1) / hop) + 1;

        // 1. Power on the grid round the tones, every eighth of a symbol.
        var power = new double[frames][];
        var fft = new RealFft(size);
        var magnitudes = new double[fft.BinCount];
        var real = new double[size];
        var imaginary = new double[size];
        var shaped = new float[size];
        var shape = new double[window];

        for (var i = 0; i < window; i++)
        {
            shape[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / window));
        }

        for (var f = 0; f < frames; f++)
        {
            var at = f * hop;

            for (var i = 0; i < window; i++)
            {
                shaped[i] = at + i < samples.Length ? (float)(samples[at + i] * shape[i]) : 0f;
            }

            fft.Magnitudes(shaped, magnitudes, real, imaginary);

            var row = new double[span];

            for (var b = 0; b < span; b++)
            {
                var m = magnitudes[lowBin + b];
                row[b] = m * m;
            }

            power[f] = row;
        }

        // 2. The offset that puts the most power on the tones.
        var bestOffset = 0;
        var bestScore = double.MinValue;

        for (var offset = -reach; offset <= reach; offset++)
        {
            double score = 0;

            for (var f = 0; f < frames; f++)
            {
                double loudest = 0;

                for (var k = 0; k < _variant.Tones; k++)
                {
                    loudest = Math.Max(loudest, power[f][ToneColumn(k, offset, reach, binsPerTone)]);
                }

                score += loudest;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestOffset = offset;
            }
        }

        // 3. A soft value for every bit of every frame: the log-likelihood that the bit is clear.
        var bits = _variant.BitsPerSymbol;
        var tones = _variant.Tones;
        var energies = new double[frames * tones];

        for (var f = 0; f < frames; f++)
        {
            for (var k = 0; k < tones; k++)
            {
                energies[(f * tones) + k] = power[f][ToneColumn(k, bestOffset, reach, binsPerTone)];
            }
        }

        var (noise, signal) = NoiseAndSignal(energies, frames, tones);
        var soft = new double[frames][];
        var metrics = new double[frames][];

        for (var f = 0; f < frames; f++)
        {
            var metric = new double[tones];

            metrics[f] = metric;

            for (var k = 0; k < tones; k++)
            {
                // **NONCOHERENT DETECTION OF ONE TONE AMONG MANY**: with the power in units of the
                // noise in one bin, the likelihood of the tone carrying the signal goes as the
                // modified Bessel function of twice the root of signal times power.
                metric[k] = LogBesselI0(2 * Math.Sqrt(signal * energies[(f * tones) + k] / noise));
            }

            var values = new double[bits];

            for (var b = 0; b < bits; b++)
            {
                var zero = double.NegativeInfinity;
                var one = double.NegativeInfinity;

                for (var k = 0; k < tones; k++)
                {
                    if (((_toneToSymbol[k] >> b) & 1) == 0)
                    {
                        zero = LogAdd(zero, metric[k]);
                    }
                    else
                    {
                        one = LogAdd(one, metric[k]);
                    }
                }

                // **A SET BIT IS A NEGATIVE WALSH VALUE** where the file says so, so a bit that
                // reads as clear is handed to the correlation as positive.
                values[b] = _format.NegativeSetsTheBit ? zero - one : one - zero;
            }

            soft[f] = values;
        }

        // 4. The sync: the frame within a symbol and the symbol a block starts on.
        var perBlock = _format.SymbolsPerBlock;
        var decoder = new BlockDecoder(_format, bits, _toneToSymbol);
        var bestPhase = 0;
        var bestBlock = 0;
        var bestSum = double.MinValue;

        for (var phase = 0; phase < FramesPerSymbol; phase++)
        {
            var symbols = phase >= frames ? 0 : ((frames - 1 - phase) / FramesPerSymbol) + 1;

            for (var block = 0; block < perBlock; block++)
            {
                double sum = 0;

                for (var first = block; first + perBlock <= symbols; first += perBlock)
                {
                    sum += decoder.Decode(soft, phase + (first * FramesPerSymbol), FramesPerSymbol).Snr;
                }

                if (sum > bestSum)
                {
                    bestSum = sum;
                    bestPhase = phase;
                    bestBlock = block;
                }
            }
        }

        // 5. Read at the sync; a block that does not clear the threshold shows nothing.
        var text = new StringBuilder();
        var decoded = 0;
        var rejected = 0;
        var snrSum = 0.0;
        var firstSeconds = double.NaN;
        var lastSeconds = double.NaN;
        var inSync = false;
        var blockSnrs = new List<double>();
        var offsetHz = bestOffset * binHz + (firstBin * binHz) - (_centerHz + _variant.FirstToneOffsetHz);
        var totalSymbols = bestPhase >= frames ? 0 : ((frames - 1 - bestPhase) / FramesPerSymbol) + 1;

        for (var first = bestBlock; first + perBlock <= totalSymbols; first += perBlock)
        {
            var frame = bestPhase + (first * FramesPerSymbol);
            var result = decoder.DecodeIterating(metrics, frame, FramesPerSymbol, Passes);
            var seconds = startSeconds + (((frame * hop) + (window / 2.0) - (symbolSamples / 2.0)) / _sampleRate);

            blockSnrs.Add(result.Snr);

            if (result.Snr >= _threshold)
            {
                if (!inSync)
                {
                    Sync("found", seconds, offsetHz, bestPhase, first % perBlock, result.Snr);
                    inSync = true;
                }

                decoded++;
                snrSum += result.Snr;

                if (double.IsNaN(firstSeconds))
                {
                    firstSeconds = seconds;
                }

                lastSeconds = seconds;

                foreach (var character in result.Characters)
                {
                    if (IsText(character))
                    {
                        text.Append((char)character);
                    }
                }
            }
            else
            {
                if (inSync)
                {
                    Sync("lost", seconds, offsetHz, bestPhase, first % perBlock, result.Snr);
                    inSync = false;
                }

                rejected++;
            }
        }

        var decoding = new OliviaDecoding(
            text.ToString(),
            decoded,
            rejected,
            text.Length,
            offsetHz,
            bestPhase,
            bestBlock,
            decoded == 0 ? 0 : snrSum / decoded,
            firstSeconds,
            lastSeconds,
            blockSnrs);

        Summary(decoding, symbolSamples, window, hop, size);

        return decoding;
    }

    /// <summary>The noise in one tone bin, and the signal in one symbol in units of it.</summary>
    /// <remarks>
    /// <para>**THE NOISE IS THE MEDIAN TONE POWER OVER LN 2.** Only one tone in each symbol carries
    /// the signal, so the median of every tone's power is a noise bin's, and noise power in a bin is
    /// exponentially distributed, whose median is its mean times ln 2.</para>
    /// <para>**THE SIGNAL IS WHAT A SYMBOL CARRIES ABOVE THE NOISE**, averaged over the frames that
    /// carry anything: the tones of one frame hold one noise bin each plus the one symbol. It is
    /// floored at a tenth of a noise bin, so a run that holds no signal still has a likelihood.</para>
    /// </remarks>
    private static (double Noise, double Signal) NoiseAndSignal(double[] energies, int frames, int tones)
    {
        if (frames == 0)
        {
            return (1, 0.1);
        }

        var sorted = (double[])energies.Clone();

        Array.Sort(sorted);

        var mean = sorted.Average();
        var noise = Math.Max(sorted[sorted.Length / 2] / Math.Log(2), mean * 1e-9);

        if (noise <= 0)
        {
            return (1, 0.1);
        }

        double above = 0;
        var counted = 0;

        for (var f = 0; f < frames; f++)
        {
            double total = 0;

            for (var k = 0; k < tones; k++)
            {
                total += energies[(f * tones) + k];
            }

            if (total > 0)
            {
                above += (total / noise) - tones;
                counted++;
            }
        }

        return (noise, Math.Max(counted == 0 ? 0 : above / counted, 0.1));
    }

    /// <summary>The natural log of the modified Bessel function of the first kind, order zero.</summary>
    /// <remarks>The polynomial approximations of Abramowitz and Stegun 9.8.1 and 9.8.2, the second
    /// written in logs so a large argument does not overflow.</remarks>
    private static double LogBesselI0(double x)
    {
        if (x < 3.75)
        {
            var t = (x / 3.75) * (x / 3.75);

            return Math.Log(Horner(t, SmallArgument));
        }

        return x - (0.5 * Math.Log(x)) + Math.Log(Horner(3.75 / x, LargeArgument));
    }

    /// <summary>Abramowitz and Stegun 9.8.1's coefficients, lowest power first.</summary>
    private static readonly double[] SmallArgument =
        [1.0, 3.5156229, 3.0899424, 1.2067492, 0.2659732, 0.0360768, 0.0045813];

    /// <summary>Abramowitz and Stegun 9.8.2's coefficients, lowest power first.</summary>
    private static readonly double[] LargeArgument =
        [0.39894228, 0.01328592, 0.00225319, -0.00157565, 0.00916281, -0.02057706, 0.02635537, -0.01647633, 0.00392377];

    /// <summary>A polynomial with its coefficients lowest power first.</summary>
    private static double Horner(double x, double[] coefficients)
    {
        var sum = 0.0;

        for (var i = coefficients.Length - 1; i >= 0; i--)
        {
            sum = (sum * x) + coefficients[i];
        }

        return sum;
    }

    /// <summary>The log of the sum of two numbers given as logs.</summary>
    private static double LogAdd(double a, double b)
    {
        if (double.IsNegativeInfinity(a))
        {
            return b;
        }

        if (double.IsNegativeInfinity(b))
        {
            return a;
        }

        var high = Math.Max(a, b);

        return high + Math.Log(1 + Math.Exp(Math.Min(a, b) - high));
    }

    /// <summary>Whether a character is text rather than idle or a control nobody reads.</summary>
    private bool IsText(int character)
        => character != _format.NullCharacter
           && (character == '\n' || character == '\r' || character == '\t' || (character >= ' ' && character < 127));

    private int ToneColumn(int tone, int offset, int reach, double binsPerTone)
        => reach + offset + (int)Math.Round(tone * binsPerTone);

    private void Sync(string state, double seconds, double offsetHz, int symbolPhase, int blockPhase, double snr)
        => _telemetry?.Write(
            TelemetryCategory.Psk31,
            "olivia_sync",
            new Dictionary<string, object?>
            {
                ["mode"] = "olivia",
                ["variant"] = _variant.Name,
                ["state"] = state,
                ["centerHz"] = Math.Round(_centerHz, 2),
                ["frequencyOffsetHz"] = Math.Round(offsetHz, 2),
                ["symbolPhase"] = symbolPhase,
                ["blockPhase"] = blockPhase,
                ["snr"] = Math.Round(snr, 2),
                ["threshold"] = _threshold,
                ["atSeconds"] = Math.Round(seconds, 3),
            });

    private void Summary(OliviaDecoding decoding, int symbolSamples, int window, int hop, int size)
        => _telemetry?.Write(
            TelemetryCategory.Psk31,
            "olivia_run",
            new Dictionary<string, object?>
            {
                ["mode"] = "olivia",
                ["variant"] = _variant.Name,
                ["centerHz"] = Math.Round(_centerHz, 2),
                ["sampleRate"] = _sampleRate,
                ["samplesPerSymbol"] = symbolSamples,
                ["windowSamples"] = window,
                ["hopSamples"] = hop,
                ["transformSize"] = size,
                ["frequencyOffsetHz"] = Math.Round(decoding.FrequencyOffsetHz, 2),
                ["symbolPhase"] = decoding.SymbolPhase,
                ["blockPhase"] = decoding.BlockPhase,
                ["blocksDecoded"] = decoding.BlocksDecoded,
                ["blocksRejected"] = decoding.BlocksRejected,
                ["charactersOut"] = decoding.CharactersOut,
                ["meanSnr"] = Math.Round(decoding.SyncSnr, 2),
                ["threshold"] = _threshold,
            });

    /// <summary>One block's characters and how far they stood out of the noise.</summary>
    private readonly record struct Block(int[] Characters, double Snr);

    /// <summary>De-interleaves, descrambles and correlates one block against every Walsh function.</summary>
    private sealed class BlockDecoder
    {
        private readonly OliviaFormat _format;
        private readonly int[] _toneToSymbol;
        private readonly int _bits;
        private readonly int _length;
        private readonly double[] _buffer;
        private readonly int[,] _walsh;
        private readonly int _a;
        private readonly int _b;
        private readonly int _c;
        private readonly int _d;

        public BlockDecoder(OliviaFormat format, int bits, int[] toneToSymbol)
        {
            _format = format;
            _bits = bits;
            _toneToSymbol = toneToSymbol;
            _length = format.SymbolsPerBlock;
            _buffer = new double[_length];

            // **THE CORRELATION IS THE TRANSPOSE OF THE FILE'S INVERSE BUTTERFLY**: the inverse
            // made each character's Walsh function, so its transpose, applied stage by stage,
            // correlates a block against all of them at once.
            _a = format.WalshInverseKernel[0, 0];
            _b = format.WalshInverseKernel[1, 0];
            _c = format.WalshInverseKernel[0, 1];
            _d = format.WalshInverseKernel[1, 1];

            // **AND EACH WALSH FUNCTION ITSELF**, made the way the transmitter makes it: the inverse
            // butterfly applied to one index set, which the iterative pass needs symbol by symbol.
            _walsh = new int[_length, _length];

            for (var index = 0; index < _length; index++)
            {
                var column = new int[_length];

                column[index] = 1;

                for (var step = _length / 2; step >= 1; step /= 2)
                {
                    for (var at = 0; at < _length; at += 2 * step)
                    {
                        for (var i = at; i < at + step; i++)
                        {
                            var lower = column[i];
                            var upper = column[i + step];

                            column[i] = (format.WalshInverseKernel[0, 0] * lower) + (format.WalshInverseKernel[0, 1] * upper);
                            column[i + step] = (format.WalshInverseKernel[1, 0] * lower) + (format.WalshInverseKernel[1, 1] * upper);
                        }
                    }
                }

                for (var t = 0; t < _length; t++)
                {
                    _walsh[index, t] = column[t];
                }
            }
        }

        /// <summary>One pass: every character correlated against the bit values as they stand.</summary>
        public Block Decode(double[][] soft, int firstFrame, int stride)
        {
            var characters = new int[_bits];
            double peaks = 0;
            double noise = 0;

            for (var c = 0; c < _bits; c++)
            {
                for (var t = 0; t < _length; t++)
                {
                    _buffer[t] = Descrambled(c, t, soft[firstFrame + (t * stride)][BitOf(c, t)]);
                }

                Correlate();

                var (character, peak, rest) = Strongest();

                characters[c] = character;
                peaks += Math.Abs(peak);
                noise += rest;
            }

            return new Block(characters, Snr(peaks, noise));
        }

        /// <summary>
        /// **The iterative pass**: what each character's code says about its bits is handed back
        /// to the tones those bits share, and the tones re-read, before the characters are read.
        /// </summary>
        /// <remarks>
        /// <para>**HAMLET'S OWN, AND NOT IN `pj_mfsk.h`**, whose decoder reads each character once.
        /// Every tone carries one bit of every character in the block, so a character read with
        /// confidence says a great deal about which tones were sent, and that sharpens the bits of
        /// every other character on the same tones. The passes exchange only what each side did
        /// not already know - the code's answer less the bit value it was handed - so nothing is
        /// counted twice.</para>
        /// <para>**THE BLOCK'S SIGNAL-TO-NOISE IS READ ON THE LAST BIT VALUES**, which carry what the
        /// other characters said and never what the character's own code said about itself.</para>
        /// </remarks>
        public Block DecodeIterating(double[][] metrics, int firstFrame, int stride, int passes)
        {
            var tones = _toneToSymbol.Length;
            var prior = new double[_bits, _length];
            var bits = new double[_length, _bits];
            var corr = new double[2 * _length];
            var characters = new int[_bits];
            double peaks = 0;
            double noise = 0;

            for (var pass = 0; pass <= passes; pass++)
            {
                // Tones to bits, each bit leaving out what its own character said.
                for (var t = 0; t < _length; t++)
                {
                    var m = metrics[firstFrame + (t * stride)];

                    for (var bit = 0; bit < _bits; bit++)
                    {
                        var zero = double.NegativeInfinity;
                        var one = double.NegativeInfinity;

                        for (var k = 0; k < tones; k++)
                        {
                            var symbol = _toneToSymbol[k];
                            var value = m[k];

                            for (var other = 0; other < _bits; other++)
                            {
                                if (other != bit)
                                {
                                    var p = prior[CharacterOf(other, t), t];

                                    value += ((symbol >> other) & 1) == 0 ? p / 2 : -p / 2;
                                }
                            }

                            if (((symbol >> bit) & 1) == 0)
                            {
                                zero = LogAdd(zero, value);
                            }
                            else
                            {
                                one = LogAdd(one, value);
                            }
                        }

                        bits[t, bit] = _format.NegativeSetsTheBit ? zero - one : one - zero;
                    }
                }

                peaks = 0;
                noise = 0;

                // Bits to characters, and what each character's code says back about its bits.
                for (var c = 0; c < _bits; c++)
                {
                    for (var t = 0; t < _length; t++)
                    {
                        _buffer[t] = Descrambled(c, t, bits[t, BitOf(c, t)]);
                    }

                    var input = (double[])_buffer.Clone();

                    Correlate();

                    var (character, peak, rest) = Strongest();

                    characters[c] = character;
                    peaks += Math.Abs(peak);
                    noise += rest;

                    if (pass == passes)
                    {
                        continue;
                    }

                    for (var j = 0; j < _length; j++)
                    {
                        corr[j] = _buffer[j] / 2;
                        corr[j + _length] = -_buffer[j] / 2;
                    }

                    for (var t = 0; t < _length; t++)
                    {
                        // **IN LOGS THROUGHOUT**, each side summed from its own largest term, so a
                        // clean signal whose correlations run to thousands cannot saturate either
                        // side and leave the difference meaningless.
                        var plus = double.NegativeInfinity;
                        var minus = double.NegativeInfinity;

                        for (var j = 0; j < _length; j++)
                        {
                            if (_walsh[j, t] > 0)
                            {
                                plus = LogAdd(plus, corr[j]);
                                minus = LogAdd(minus, corr[j + _length]);
                            }
                            else
                            {
                                plus = LogAdd(plus, corr[j + _length]);
                                minus = LogAdd(minus, corr[j]);
                            }
                        }

                        var posterior = plus - minus;
                        var extrinsic = posterior - input[t];

                        // Back to the bit's own sign, and to "clear" as positive.
                        var toBit = Descrambled(c, t, extrinsic);

                        prior[c, t] = _format.NegativeSetsTheBit ? toBit : -toBit;
                    }
                }
            }

            return new Block(characters, Snr(peaks, noise));
        }

        private int BitOf(int character, int t) => (character + (t * _format.ToneBitRotationPerSymbol)) % _bits;

        /// <summary>Which character's bit sits at a given bit of a given symbol.</summary>
        private int CharacterOf(int bit, int t)
            => (((bit - (t * _format.ToneBitRotationPerSymbol)) % _bits) + _bits) % _bits;

        private double Descrambled(int character, int t, double value)
        {
            var codeBit = ((character * _format.ScramblingShiftPerCharacter) + t) & (_length - 1);

            return ((_format.ScramblingCode >> codeBit) & 1) == 1 ? -value : value;
        }

        private void Correlate()
        {
            for (var step = 1; step < _length; step *= 2)
            {
                for (var at = 0; at < _length; at += 2 * step)
                {
                    for (var i = at; i < at + step; i++)
                    {
                        var lower = _buffer[i];
                        var upper = _buffer[i + step];

                        _buffer[i] = (_a * lower) + (_b * upper);
                        _buffer[i + step] = (_c * lower) + (_d * upper);
                    }
                }
            }
        }

        private (int Character, double Peak, double Others) Strongest()
        {
            var peak = 0.0;
            var index = 0;
            double squares = 0;

            for (var i = 0; i < _length; i++)
            {
                squares += _buffer[i] * _buffer[i];

                if (Math.Abs(_buffer[i]) > Math.Abs(peak))
                {
                    peak = _buffer[i];
                    index = i;
                }
            }

            var character = index;

            if (peak < 0)
            {
                character = _format.UpperHalfNegated ? index + _length : index;
            }

            return (character & _format.CharacterMask, peak, (squares - (peak * peak)) / (_length - 1));
        }

        private double Snr(double peaks, double noise)
            => noise <= 0 ? (peaks > 0 ? double.MaxValue : 0) : (peaks / _bits) / Math.Sqrt(noise / _bits);
    }
}
