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
/// <param name="EndsInsideCharacter">
/// True when the winning path's last segment is a mark or the gap between two
/// marks of one character, rather than the gap between characters or between
/// words. **This is the question HM-DEC-096 phase 3's interlock asks**, answered
/// by the path itself rather than inferred from anything: the decoder already
/// chose where every element and every character begins and ends, and the last
/// segment of that choice is what the newest audio is inside of. It is false
/// whenever the gate is closed, because nothing is being read and there is no
/// character to be part of the way through.
/// </param>
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
    IReadOnlyList<CwProbabilisticCharacter> Characters,
    bool EndsInsideCharacter = false)
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
    /// <remarks>
    /// <para>**ONE, BECAUSE HALF THE SPEEDS PEOPLE SEND AT WERE NOT ON THE
    /// GRID.** Ordinary operators work at thirteen, fifteen, seventeen, nineteen
    /// and twenty-one words a minute, and a step of two reached none of the odd
    /// ones. A hypothesis a quarter short of the truth stretches every gap
    /// measured against it.</para>
    /// <para>**IT IS NOT WHY CHARACTERS BREAK, AND THAT WAS MEASURED**: on
    /// `cw-2026-08-18-004507` the likelihood is 32.3 to 32.4 at every speed from
    /// eleven to thirty-two, so the objective is flat in speed and which
    /// hypothesis wins is nearly arbitrary; elements per character stays between
    /// 2.33 and 2.50 across that whole range.</para>
    /// <para>**AND A STEP OF ONE WAS BUILT AND MEASURED AND DOES NOT SHIP.** With
    /// a flat objective, more hypotheses is more ways to be wrong: the sensitivity
    /// fixture, which sends at eighteen, was won by nine words a minute and the
    /// sweep began inventing 0.22 of the message at eighteen decibels where it had
    /// invented nothing. It also costs 22.7 per cent of real time against 13.5.
    /// **HM-DEC-120 is not traded for reaching the odd speeds**, so the step stays
    /// at two until the objective can tell speeds apart.</para>
    /// </remarks>
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

    /// <summary>
    /// How wide the penalty on a segment's length is, as a share of the log
    /// ratio between what arrived and what was expected.
    /// </summary>
    /// <remarks>
    /// <para>**THE SCATTER IS SCORED AS A RATIO AND NOT AS A DIFFERENCE**, ruled
    /// by Tim on 2026-08-22. Timing error in a hand-sent fist is multiplicative:
    /// a sender who runs a fifth long runs a fifth long on dits, dahs and gaps
    /// alike, which is a property of hands rather than of textbooks. So the cost
    /// is `ln(span / want) / 0.35`, and **both crossovers land at the geometric
    /// mean, 1.73 units** — between a dit and a dah, and between the gap inside a
    /// character and the gap between two.</para>
    /// <para>**WHAT IT REPLACED, AND WHY THAT WAS WRONG.** The cost used to be
    /// `(span − want) / (want × 0.35)`, a share of each kind's own expected
    /// length, so the gap between characters was allowed three times the scatter
    /// of the gap inside one and the word gap seven times. **The two costs
    /// crossed at one and a half units rather than at two**: at a gap of exactly
    /// two units the element reading cost 4.08 and the character reading 0.45,
    /// with an identical evidence term, so nothing argued back. Every gap longer
    /// than one and a half dits was called a character gap, and a decoder that
    /// breaks between the elements of one letter emits E, T and I.</para>
    /// <para>**MEASURED, AND BOTH HALVES ARE TRUE.** It reads `2 MOVIES A DAY`
    /// where it read `2 IOVI ES`, `EACH` as one word, keeps `N4LQ K` on the
    /// capture HM-DEC-144 adjudicated as `N4L` and brings `VRR VA` out of the one
    /// HM-DEC-145 adjudicated as `VA3VRR`. **And elements per character is
    /// unmoved in aggregate.**</para>
    /// <para>**ONE OTHER MODEL WAS BUILT AND MEASURED AND REJECTED**: scaling the
    /// scatter by the dit rather than by the segment, which moves both crossovers
    /// to two units and costs five of seven recordings their text, because the
    /// dahs of a real fist arrive at two to two and a half units and then read as
    /// dits.</para>
    /// <para>`tools/reference-decoder/reference_decoder.py` carries the same
    /// change, so `ItReadsWhatTheReferenceReads` still means what it meant.</para>
    /// </remarks>
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
        => Decode(envelope, toneHz, atWordsPerMinute: null);

    /// <summary>Read an envelope, optionally at one imposed speed.</summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="toneHz">The pitch it was taken at.</param>
    /// <param name="atWordsPerMinute">
    /// One speed to read at, or null to search the grid.
    /// </param>
    /// <returns>What it read.</returns>
    /// <remarks>
    /// **THE IMPOSED SPEED IS FOR ASKING QUESTIONS, NOT FOR DECODING.** Nothing
    /// in the application passes it. It exists so a measurement can separate two
    /// faults that look alike: a speed the grid cannot reach, and a gap model that
    /// breaks characters wherever the speed lands.
    /// </remarks>
    public static CwProbabilisticResult Decode(
        IReadOnlyList<double> envelope, double toneHz, double? atWordsPerMinute)
        => Decode(envelope, toneHz, atWordsPerMinute, gapMilliseconds: null);

    /// <summary>Read an envelope, at one speed and with this sender's own gaps.</summary>
    /// <param name="envelope">Envelope magnitudes, one every hop.</param>
    /// <param name="toneHz">The pitch it was taken at.</param>
    /// <param name="atWordsPerMinute">One speed to read at, or null to search.</param>
    /// <param name="gapMilliseconds">
    /// How long this sender's gap inside a character, between characters and
    /// between words actually are, or null to take them as one, three and seven
    /// units.
    /// </param>
    /// <returns>What it read.</returns>
    /// <remarks>
    /// **THE GAP LENGTHS COME FROM THE GAPS OR THEY COME FROM THE UNIT, AND THE
    /// SECOND COUPLES TWO FAILURES INTO ONE.** With the expected lengths taken as
    /// multiples of the unit, the cost of reading a gap as a letter break crosses
    /// the cost of reading it as an element gap at the geometric mean of one and
    /// three units, so **a unit that is wrong moves every letter boundary with
    /// it**. Handing the measured lengths in puts that crossing at the geometric
    /// mean of two things the sender actually did, which on every capture here
    /// lands in an empty stretch of that sender's own gap distribution.
    /// </remarks>
    public static CwProbabilisticResult Decode(
        IReadOnlyList<double> envelope,
        double toneHz,
        double? atWordsPerMinute,
        IReadOnlyList<double>? gapMilliseconds)
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
        var bestLastKind = -1;
        IReadOnlyList<CwProbabilisticCharacter> bestCharacters =
            Array.Empty<CwProbabilisticCharacter>();

        var from = atWordsPerMinute ?? SlowestWpm;
        var to = atWordsPerMinute ?? FastestWpm;

        for (var wpm = from; wpm <= to + 1e-9; wpm += WpmStep)
        {
            var (score, characters, lastKind) =
                DecodeAt(envelope.Count, wpm, keyDown, keyUp, gapMilliseconds);

            if (score > bestScore)
            {
                bestScore = score;
                bestWpm = wpm;
                bestCharacters = characters;
                bestLastKind = lastKind;
            }
        }

        // **WHERE THE PATH ENDS IS WHERE THE AUDIO IS.** Kinds 0 and 1 are the
        // mark, kind 2 is the gap inside a character; 3 and 4 are the gaps
        // between characters and between words, which is where the tracker is
        // free to move.
        var insideCharacter = bestLastKind is >= 0 and <= 2;

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
            bestCharacters,
            insideCharacter);
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
    /// <param name="gapMilliseconds">
    /// This sender's own three gap lengths, or null to expect one, three and
    /// seven units.
    /// </param>
    /// <returns>The best total score and what it spells.</returns>
    /// <remarks>
    /// **EVERY PATH IS A CHAIN OF WHOLE ELEMENTS THAT MUST ALTERNATE.** A
    /// segment's score is the summed per-hop likelihood over its span plus a
    /// Gaussian penalty on how far its length sits from the one, three or seven
    /// units the hypothesis expects. Cumulative sums make a span's score two
    /// subtractions, so the whole thing is one pass over hops times durations
    /// times kinds.
    /// </remarks>
    private static (
        double Score,
        IReadOnlyList<CwProbabilisticCharacter> Characters,
        int LastKind)
        DecodeAt(
        int count,
        double wpm,
        double[] keyDown,
        double[] keyUp,
        IReadOnlyList<double>? gapMilliseconds = null)
    {
        var unit = 1200.0 / wpm / HopMilliseconds;

        // **THIS SENDER'S OWN GAPS, IN HOPS**, when they were measured. The kinds
        // keep their order — the gap inside a character, then between characters,
        // then between words — and only what each one expects to last changes.
        var gapHops = gapMilliseconds is { Count: 3 }
            ? new[]
            {
                gapMilliseconds[0] / HopMilliseconds,
                gapMilliseconds[1] / HopMilliseconds,
                gapMilliseconds[2] / HopMilliseconds,
            }
            : null;

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
                var want = gapHops is not null && !kind.IsKeyDown
                    ? gapHops[k - 2]
                    : kind.Units * unit;
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

                    // Guarded against a zero span, which the shortest-span floor
                    // already makes impossible and which would be an infinity if
                    // it ever were not.
                    var off = Math.Log(Math.Max(span, 1e-9) / want)
                        / LengthToleranceShare;
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

        return (best[count], Spell(count, fromHop, kindAt), kindAt[count]);
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
