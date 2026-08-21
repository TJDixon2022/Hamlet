using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>What the probabilistic decoder made of a stretch of audio.</summary>
/// <param name="LikelihoodRatio">
/// How much better the best reading explains the audio than "this is all noise",
/// per hop. **The null hypothesis is explicitly modelled and competes**, so an
/// empty band scores near nought here rather than being caught by a guard
/// (HM-DEC-120).
/// </param>
/// <param name="WordsPerMinute">
/// The speed hypothesis that won, which is not a measurement of anything. Nothing
/// here fits a speed from run lengths; a dozen speeds are tried and the audio
/// picks.
/// </param>
/// <param name="Text">What it read, or "" when the ratio is below the gate.</param>
/// <param name="ToneHz">The pitch it was given.</param>
/// <param name="Characters">
/// The same reading, one entry per character, each carrying the hop it ended at.
/// Empty when the gate is closed. **The streaming path needs the times**: a
/// character old enough to be behind the decision delay is settled and one inside
/// it may still be revised, which is the whole point of deciding late.
/// </param>
public readonly record struct CwProbabilisticResult(
    double LikelihoodRatio,
    double WordsPerMinute,
    string Text,
    double ToneHz,
    IReadOnlyList<CwProbabilisticCharacter> Characters)
{
    /// <summary>Nothing measured.</summary>
    public static CwProbabilisticResult None { get; }
        = new(0, 0, "", 0, Array.Empty<CwProbabilisticCharacter>());
}

/// <summary>One character the decoder read, and where it ended.</summary>
/// <param name="Text">The letter, or a space for a word gap.</param>
/// <param name="Pattern">The dits and dahs behind it, or "" for a word gap.</param>
/// <param name="EndHop">Which hop of the window it ended at.</param>
public readonly record struct CwProbabilisticCharacter(
    string Text, string Pattern, int EndHop);

/// <summary>
/// A segmental Viterbi CW decoder that never forms a threshold.
/// </summary>
/// <remarks>
/// <para>**THE OLD DECODER'S ARCHITECTURE WAS THE FAULT.** It thresholded the
/// envelope into hard key-down and key-up runs, fitted a speed by clustering
/// those run lengths, and picked its analysis width from the fitted speed. Every
/// stage depended on the one before and the evidence was discarded at the first
/// step, so nothing downstream could recover from a wrong commit. And it was a
/// loop with positive feedback: chatter shortens the fitted dit, a short dit
/// reads as a fast fist, a fast fist widens the bandwidth, more noise crosses the
/// threshold. Measured on this repository's own recordings, senders working near
/// fourteen words a minute fitted at twenty-two to fifty-six.</para>
/// <para>**THE THREE IDEAS, WHICH ARE THE WHOLE OF IT.** Every hop produces two
/// numbers, the log-likelihood the key is down and the log-likelihood it is up,
/// and nothing commits. Speed is an outer hypothesis rather than a measurement,
/// so the loop cannot exist. And element boundaries and character boundaries are
/// chosen together and late, by dynamic programming over whole elements, rather
/// than one gap at a time against a threshold.</para>
/// <para>These are E. L. Bell's ideas from 1977, reduced to something small.
/// Ported line for line from `tools/reference-decoder/reference_decoder.py`,
/// which is in this repository so that the port has an implementation to be
/// checked against rather than a description.</para>
/// <para>**AND SILENCE FALLS OUT RATHER THAN BEING BOLTED ON.** "The whole
/// stretch is noise" is a competing hypothesis with a score of its own, so on a
/// recording holding no station it wins and there is nothing to emit. That is
/// HM-DEC-120 by construction; it is still tested rather than assumed.</para>
/// </remarks>
public static class CwProbabilisticDecoder
{
    /// <summary>
    /// The log-likelihood ratio per hop below which nothing is emitted.
    /// </summary>
    /// <remarks>
    /// **PROVISIONAL, AND THE MEASURED GAP IT SITS IN IS WIDE.** On this
    /// repository's six real recordings the ratio is 24 to 39 where a station is
    /// sending and 3 to 6 where none is, with no overlap, so any value between
    /// ten and twenty reads every station and silences both empty bands. Fifteen
    /// is the middle of that. It wants an evening's captures scored against it
    /// before it stops being a number somebody chose.
    /// </remarks>
    public const double Gate = 15.0;

    /// <summary>How wide the envelope's own filter is, in hertz.</summary>
    /// <remarks>
    /// Sixty. **It is not chosen from any speed and nothing measured decides it**,
    /// which is the point: the old decoder's bandwidth came from its own fitted
    /// speed and that was the loop. A dit at forty words a minute is thirty
    /// milliseconds, so sixty hertz passes every element anybody sends.
    /// </remarks>
    public const double BandwidthHz = 60.0;

    /// <summary>How often the envelope is sampled, in milliseconds.</summary>
    public const double HopMilliseconds = 5.0;

    /// <summary>The slowest speed hypothesis tried.</summary>
    /// <remarks>
    /// **EIGHT, BECAUSE A GRID THAT STOPS AT TEN CANNOT FIT A TEN.** A hypothesis
    /// at the very edge of the range wins by default rather than on evidence:
    /// there is nothing below it to lose to, so a sender slower than the floor is
    /// fitted at the floor whatever he is actually doing. The operator this
    /// application is for works people sending eight to twelve on a straight key,
    /// which is the slowest thing on the band and the easiest to copy by ear.
    /// </remarks>
    public const double SlowestWpm = 8;

    /// <summary>The fastest speed hypothesis tried.</summary>
    /// <remarks>
    /// **FORTY, BECAUSE A MACHINE SENDER IS THE EASIEST THING ON THE BAND AND
    /// HAMLET COULD NOT FIT ONE.** A station running thirty-five or forty is
    /// almost always a program sending perfect timing, which is the least
    /// demanding audio a decoder ever sees, and the old ceiling of thirty-two put
    /// it outside the grid.
    /// </remarks>
    public const double FastestWpm = 32;

    /// <summary>How far apart the speed hypotheses sit.</summary>
    public const double WpmStep = 2;

    /// <summary>One element kind the model knows about.</summary>
    /// <param name="Units">How many dit-lengths it is expected to last.</param>
    /// <param name="IsKeyDown">Whether the key is down for it.</param>
    /// <param name="Token">What it contributes to a character, or "".</param>
    private readonly record struct Kind(int Units, bool IsKeyDown, string Token);

    /// <summary>
    /// Dit, dah, the gap inside a character, the gap between characters, and the
    /// gap between words.
    /// </summary>
    private static readonly Kind[] Kinds =
    {
        new(1, true, "."),
        new(3, true, "-"),
        new(1, false, ""),
        new(3, false, "|"),
        new(7, false, " "),
    };

    /// <summary>How far from its expected length a segment may stray, as a share.</summary>
    /// <remarks>
    /// Less than half and more than twice, which is deliberately loose: a real
    /// fist sends a dah anywhere from two and a half to four and a quarter dits
    /// (HM-DEC-144, HM-DEC-145), and the Gaussian penalty below does the work of
    /// preferring the middle rather than a bound doing it.
    /// </remarks>
    private const double ShortestShare = 0.45;

    private const double LongestShare = 2.2;

    /// <summary>How wide the penalty on a segment's length is, as a share.</summary>
    private const double LengthToleranceShare = 0.35;

    /// <summary>Read a stretch of audio at a known pitch.</summary>
    /// <param name="audio">The recording.</param>
    /// <param name="toneHz">Where the station is, from the tone tracker.</param>
    /// <returns>What it read, and how much better than noise that reading is.</returns>
    public static CwProbabilisticResult Decode(MonoAudio audio, double toneHz)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var envelope = Envelope(audio.Samples, audio.SampleRate, toneHz);

        return Decode(envelope, toneHz);
    }

    /// <summary>Read an envelope that has already been taken.</summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="toneHz">The pitch it was taken at.</param>
    /// <returns>What it read.</returns>
    /// <remarks>
    /// Separate so the streaming path can keep one rolling envelope rather than
    /// re-mixing the same audio for every window.
    /// </remarks>
    public static CwProbabilisticResult Decode(
        IReadOnlyList<double> envelope, double toneHz)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (envelope.Count < 8)
        {
            return CwProbabilisticResult.None;
        }

        var (keyDown, keyUp) = LogLikelihoods(envelope);
        var nothingAtAll = 0.0;

        foreach (var value in keyUp)
        {
            nothingAtAll += value;
        }

        var bestScore = double.NegativeInfinity;
        var bestWpm = 0.0;
        IReadOnlyList<CwProbabilisticCharacter> bestCharacters =
            Array.Empty<CwProbabilisticCharacter>();

        for (var wpm = SlowestWpm; wpm <= FastestWpm + 1e-9; wpm += WpmStep)
        {
            var (score, characters) = DecodeAt(envelope.Count, wpm, keyDown, keyUp);

            if (score > bestScore)
            {
                bestScore = score;
                bestWpm = wpm;
                bestCharacters = characters;
            }
        }

        var ratio = (bestScore - nothingAtAll) / envelope.Count;

        if (ratio < Gate)
        {
            // **THE NULL HYPOTHESIS WON.** Nothing here is worth saying and there
            // is no partial answer to give (§0.0, HM-DEC-120).
            return new CwProbabilisticResult(
                ratio, bestWpm, "", toneHz, Array.Empty<CwProbabilisticCharacter>());
        }

        return new CwProbabilisticResult(
            ratio,
            bestWpm,
            string.Concat(bestCharacters.Select(c => c.Text)),
            toneHz,
            bestCharacters);
    }

    /// <summary>
    /// Quadrature mixdown to the tone, smoothed, and sampled every hop.
    /// </summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="toneHz">The pitch.</param>
    /// <returns>One magnitude per hop.</returns>
    /// <remarks>
    /// A boxcar over the quadrature arms, which is what a filter of this
    /// bandwidth amounts to. Running sums, so the whole thing is one pass
    /// whatever the window length.
    /// </remarks>
    public static double[] Envelope(
        IReadOnlyList<float> samples, int sampleRate, double toneHz)
    {
        ArgumentNullException.ThrowIfNull(samples);

        var count = samples.Count;
        var window = Math.Max(1, (int)(sampleRate / BandwidthHz));
        var step = Math.Max(1, (int)(sampleRate * HopMilliseconds / 1000.0));

        // Prefix sums of the mixed signal, so any window is two subtractions.
        var sumI = new double[count + 1];
        var sumQ = new double[count + 1];
        var omega = -2 * Math.PI * toneHz / sampleRate;

        for (var i = 0; i < count; i++)
        {
            var angle = omega * i;

            sumI[i + 1] = sumI[i] + (samples[i] * Math.Cos(angle));
            sumQ[i + 1] = sumQ[i] + (samples[i] * Math.Sin(angle));
        }

        // **THE SAME CENTRING THE REFERENCE USES.** A boxcar of `window` samples
        // laid over the sample at the centre, zero outside the recording, which
        // is what numpy's `same` convolution does and what the port has to match
        // for its output to be comparable at all.
        var lead = (window - 1) / 2;
        var envelope = new double[(count + step - 1) / step];

        for (var out_ = 0; out_ < envelope.Length; out_++)
        {
            var centre = out_ * step;
            var from = Math.Clamp(centre - (window - 1) + lead, 0, count);
            var to = Math.Clamp(centre + lead + 1, 0, count);

            var i = (sumI[to] - sumI[from]) / window;
            var q = (sumQ[to] - sumQ[from]) / window;

            envelope[out_] = Math.Sqrt((i * i) + (q * q));
        }

        return envelope;
    }

    /// <summary>
    /// Per-hop log-likelihood that the key is down, and that it is up.
    /// </summary>
    /// <param name="envelope">The envelope.</param>
    /// <returns>The two streams.</returns>
    /// <remarks>
    /// **NO THRESHOLD IS FORMED ANYWHERE.** The noise scale comes from the lower
    /// quartile of the envelope and the signal amplitude from its upper tail, and
    /// every hop is scored against both hypotheses. Bell does this properly with a
    /// tracked noise power feeding Kalman recursions; this is the cheap version
    /// and it is where a later session should improve it.
    /// </remarks>
    public static (double[] KeyDown, double[] KeyUp) LogLikelihoods(
        IReadOnlyList<double> envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var sorted = envelope.ToArray();

        Array.Sort(sorted);

        var noise = Math.Max(Percentile(sorted, 25) * 0.6, 1e-6);
        var amplitude = Math.Max(Percentile(sorted, 97), noise * 1.05);

        var keyDown = new double[envelope.Count];
        var keyUp = new double[envelope.Count];
        var logNoise = Math.Log(noise);

        for (var i = 0; i < envelope.Count; i++)
        {
            var up = envelope[i] / noise;
            var down = (envelope[i] - amplitude) / noise;

            keyUp[i] = (-0.5 * up * up) - logNoise;
            keyDown[i] = (-0.5 * down * down) - logNoise;
        }

        return (keyDown, keyUp);
    }

    /// <summary>One value out of a sorted set, interpolating between neighbours.</summary>
    /// <param name="sorted">The values, in order.</param>
    /// <param name="percent">Which percentile.</param>
    /// <returns>The value.</returns>
    private static double Percentile(double[] sorted, double percent)
    {
        if (sorted.Length == 0)
        {
            return 0;
        }

        var at = percent / 100.0 * (sorted.Length - 1);
        var low = (int)Math.Floor(at);
        var high = Math.Min(low + 1, sorted.Length - 1);

        return sorted[low] + ((sorted[high] - sorted[low]) * (at - low));
    }

    /// <summary>
    /// The segmental Viterbi at one speed hypothesis.
    /// </summary>
    /// <param name="count">How many hops there are.</param>
    /// <param name="wpm">The speed being tried.</param>
    /// <param name="keyDown">Per-hop log-likelihood the key is down.</param>
    /// <param name="keyUp">Per-hop log-likelihood the key is up.</param>
    /// <returns>The best total score and what it spells.</returns>
    /// <remarks>
    /// **EVERY PATH IS A CHAIN OF WHOLE ELEMENTS THAT MUST ALTERNATE.** A
    /// segment's score is the summed per-hop likelihood over its span plus a
    /// Gaussian penalty on how far its length sits from the one, three or seven
    /// units the hypothesis expects. Cumulative sums make a span's score two
    /// subtractions, so the whole thing is one pass over hops times durations
    /// times kinds.
    /// </remarks>
    private static (double Score, IReadOnlyList<CwProbabilisticCharacter> Characters)
        DecodeAt(
        int count, double wpm, double[] keyDown, double[] keyUp)
    {
        var unit = 1200.0 / wpm / HopMilliseconds;

        var downTo = new double[count + 1];
        var upTo = new double[count + 1];

        for (var i = 0; i < count; i++)
        {
            downTo[i + 1] = downTo[i] + keyDown[i];
            upTo[i + 1] = upTo[i] + keyUp[i];
        }

        var best = new double[count + 1];
        var fromHop = new int[count + 1];
        var kindAt = new int[count + 1];
        var wasDown = new bool[count + 1];

        Array.Fill(best, double.NegativeInfinity);
        Array.Fill(fromHop, -1);
        best[0] = 0;

        for (var i = 1; i <= count; i++)
        {
            for (var k = 0; k < Kinds.Length; k++)
            {
                var kind = Kinds[k];
                var want = kind.Units * unit;
                var shortest = Math.Max(1, (int)(want * ShortestShare));
                var longest = Math.Max(shortest + 1, (int)(want * LongestShare));
                var ceiling = Math.Min(longest, i);

                for (var span = shortest; span <= ceiling; span++)
                {
                    var j = i - span;

                    if (double.IsNegativeInfinity(best[j]))
                    {
                        continue;
                    }

                    // Elements must alternate: a mark cannot follow a mark.
                    if (j > 0 && wasDown[j] == kind.IsKeyDown)
                    {
                        continue;
                    }

                    var evidence = kind.IsKeyDown
                        ? downTo[i] - downTo[j]
                        : upTo[i] - upTo[j];

                    var off = (span - want) / Math.Max(want * LengthToleranceShare, 1.0);
                    var score = best[j] + evidence - (0.5 * off * off);

                    if (score > best[i])
                    {
                        best[i] = score;
                        fromHop[i] = j;
                        kindAt[i] = k;
                        wasDown[i] = kind.IsKeyDown;
                    }
                }
            }
        }

        return (best[count], Spell(count, fromHop, kindAt));
    }

    /// <summary>Walk the winning path back and turn it into letters.</summary>
    /// <param name="count">How many hops there were.</param>
    /// <param name="fromHop">Where each hop's best segment started.</param>
    /// <param name="kindAt">Which kind that segment was.</param>
    /// <returns>The text.</returns>
    private static IReadOnlyList<CwProbabilisticCharacter> Spell(
        int count, int[] fromHop, int[] kindAt)
    {
        var path = new List<(int Kind, int StartHop, int EndHop)>();
        var at = count;

        while (at > 0 && fromHop[at] >= 0)
        {
            path.Add((kindAt[at], fromHop[at], at));
            at = fromHop[at];
        }

        path.Reverse();

        var pattern = new System.Text.StringBuilder();
        var characters = new List<CwProbabilisticCharacter>();

        foreach (var (k, startHop, endHop) in path)
        {
            var kind = Kinds[k];

            if (kind.IsKeyDown)
            {
                pattern.Append(kind.Token);
                continue;
            }

            if (kind.Token.Length == 0)
            {
                continue;
            }

            // **THE CHARACTER ENDED WHEN THE KEY WENT UP**, not when the gap
            // after it finished, and the difference is not cosmetic: a letter and
            // the word gap behind it would otherwise carry the same moment, and
            // a streaming reader that settles by time cannot tell them apart.
            if (pattern.Length > 0)
            {
                var spelled = pattern.ToString();

                characters.Add(new CwProbabilisticCharacter(
                    MorseAlphabet.Lookup(spelled) ?? "#", spelled, startHop));

                pattern.Clear();
            }

            if (kind.Token == " ")
            {
                characters.Add(new CwProbabilisticCharacter(" ", "", endHop));
            }
        }

        if (pattern.Length > 0)
        {
            var spelled = pattern.ToString();

            characters.Add(new CwProbabilisticCharacter(
                MorseAlphabet.Lookup(spelled) ?? "#", spelled, count));
        }

        return characters;
    }
}
