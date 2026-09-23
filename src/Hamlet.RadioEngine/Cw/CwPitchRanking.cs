namespace Hamlet.RadioEngine.Cw;

/// <summary>Where the ranking says a station is, and how sure the ranking is.</summary>
/// <param name="ToneHz">The winning pitch, or NaN where nothing was ranked.</param>
/// <param name="Score">
/// The winner's likelihood ratio against the common floor, or NaN.
/// </param>
/// <param name="RunnerUpHz">The second-placed pitch, or NaN.</param>
/// <param name="RunnerUpScore">The second-placed score, or NaN.</param>
/// <remarks>
/// **THE RUNNER-UP TRAVELS WITH THE WINNER BECAUSE A WRONG PICK IS OTHERWISE A
/// MYSTERY AFTERWARDS.** A winner three times its runner-up and a winner a
/// hundredth above it are different situations, and the capture sheet cannot tell
/// them apart from the winner alone (§0.0.1).
/// </remarks>
public readonly record struct CwPitchRank(
    double ToneHz,
    double Score,
    double RunnerUpHz,
    double RunnerUpScore)
{
    /// <summary>Nothing was ranked.</summary>
    public static CwPitchRank None { get; }
        = new(double.NaN, double.NaN, double.NaN, double.NaN);

    /// <summary>True where a pitch was actually chosen by ranking.</summary>
    public bool Ranked => !double.IsNaN(ToneHz);

    /// <summary>How far clear of the runner-up the winner is, or NaN.</summary>
    /// <remarks>
    /// Reported rather than acted on. Nothing in the application decides anything
    /// from it; it exists so a sheet can say whether the choice was close.
    /// </remarks>
    public double Margin
        => double.IsNaN(Score) || double.IsNaN(RunnerUpScore)
            ? double.NaN
            : Score - RunnerUpScore;
}

/// <summary>
/// Which pitch across the band reads best, judged by decoding at each one.
/// </summary>
/// <remarks>
/// <para>**THE PITCH IS CHOSEN BY WHICH CANDIDATE DECODES BEST, NOT BY WHETHER A
/// BIN PASSES A TEST** (Tim's ruling of 2026-08-28). Six families of admission
/// statistic were built and measured dead across five units, all of them asking
/// *is this bin a station*. This asks *which of these candidates decodes best*,
/// which is a question the decoder can already answer about any stretch of
/// audio.</para>
/// <para>**AND THE SCORE HAS TO BE STOOD ON A COMMON FLOOR FIRST, OR IT RANKS
/// BACKWARDS.** <see cref="CwProbabilisticResult.LikelihoodRatio"/> estimates
/// both the noise scale and the keyed level from the very envelope it is scoring
/// (<see cref="CwProbabilisticDecoder.LogLikelihoods(System.Collections.Generic.IReadOnlyList{double})"/>), so it is scale
/// invariant and has no common unit between two pitches. A bin the receiver's
/// filter has already emptied has almost no noise left in it, its residual wobble
/// is scored against a tiny sigma, and it looks like the cleanest keying in the
/// band. **The quietest bin wins.** Measured over this repository's forty-four
/// captures, ranking on the bare score picks the station on **one** of them; on
/// `cw-2026-08-28-004844` the winner sat at 875 Hz scoring 312.62 and read
/// `E E EE E EEEE E EEE`, against 29.84 at the pitch that reads the net.</para>
/// <para>**THE PEDESTAL IS WHAT MAKES THE NUMBERS MEAN THE SAME THING AT 400 HZ
/// AS AT 900.** Every candidate's envelope is combined in power with one floor
/// measured across the whole band, which is what each bin would look like if the
/// receiver's floor were flat. A bin holding nothing goes flat against it and
/// scores near nothing; a bin holding a keyed station keeps its marks well above
/// it and keeps its structure. Same window, same decoder, one change: **thirty-four
/// of forty-four**, with the winners reading `VA3VRR`, `N4L`, `KD0UN`, the ARRL
/// bulletin, `W7GB` and `BRUCE`.</para>
/// <para>**IT IS A KEYING MEASUREMENT AND NOT A LOUDNESS ONE** (HM-DEC-095). A
/// carrier is as flat against a common pedestal as silence is: what the score
/// rewards is a two-state structure, and a louder bin with no structure in it
/// scores nothing at all.</para>
/// </remarks>
public static class CwPitchRanking
{
    /// <summary>How much audio the ranking looks at, in seconds.</summary>
    /// <remarks>
    /// <para>**FOUR, BY TIM'S RULING OF 2026-08-28, AND IT IS A TRADE MADE
    /// DELIBERATELY.** Twenty-five candidates at the decoder's own twelve-second
    /// window costs 1240 ms a sweep, which is 248 % of one core at the shipped
    /// cadence and does not fit. Cost is linear in window length: four seconds
    /// costs 390 ms, and holds several characters at twenty words a minute.</para>
    /// <para>**THREE WAS REJECTED** as cheaper but thin on evidence the first time
    /// this runs on a live band.</para>
    /// </remarks>
    public const double WindowSeconds = 4.0;

    /// <summary>Rank every candidate pitch across the band this audio was taken in.</summary>
    /// <param name="samples">The audio, oldest first.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>The winner and the runner-up, or <see cref="CwPitchRank.None"/>.</returns>
    /// <remarks>
    /// **IT REFUSES RATHER THAN GUESSING.** Too little audio to hold a character,
    /// or a band in which every candidate scores the same, and nothing is ranked:
    /// the caller keeps whatever it had. A pitch nobody measured must not produce
    /// letters that imply it was measured (§0.0).
    /// </remarks>
    public static CwPitchRank Rank(IReadOnlyList<float> samples, int sampleRate)
        => Rank(samples, sampleRate, Candidates());

    /// <summary>Rank a stated set of candidates.</summary>
    /// <param name="samples">The audio, oldest first.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="candidates">The pitches to try.</param>
    /// <returns>The winner and the runner-up, or <see cref="CwPitchRank.None"/>.</returns>
    /// <remarks>
    /// Separated so a sweep can vary the candidate set without the application
    /// having a way to vary it. Nothing in the application passes anything but
    /// <see cref="Candidates"/>.
    /// </remarks>
    public static CwPitchRank Rank(
        IReadOnlyList<float> samples,
        int sampleRate,
        IReadOnlyList<double> candidates)
    {
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentNullException.ThrowIfNull(candidates);

        if (candidates.Count < 2 || sampleRate <= 0)
        {
            return CwPitchRank.None;
        }

        var scores = Score(samples, sampleRate, candidates);

        if (scores is null)
        {
            return CwPitchRank.None;
        }

        var winner = 0;
        var runnerUp = -1;

        for (var i = 1; i < scores.Length; i++)
        {
            if (scores[i] > scores[winner])
            {
                runnerUp = winner;
                winner = i;
            }
            else if (runnerUp < 0 || scores[i] > scores[runnerUp])
            {
                runnerUp = i;
            }
        }

        return new CwPitchRank(
            candidates[winner],
            scores[winner],
            candidates[runnerUp],
            scores[runnerUp]);
    }

    /// <summary>
    /// Every candidate's score, on the common floor, in the candidates' own order.
    /// </summary>
    /// <param name="samples">The audio, oldest first.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="candidates">The pitches to try.</param>
    /// <returns>One score per candidate, or null where nothing could be scored.</returns>
    /// <remarks>
    /// Public because a floor cannot be swept from the winner alone, and because a
    /// test that cannot see the losers cannot prove the winner beat anything.
    /// </remarks>
    public static double[]? Score(
        IReadOnlyList<float> samples,
        int sampleRate,
        IReadOnlyList<double> candidates)
    {
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentNullException.ThrowIfNull(candidates);

        if (candidates.Count == 0 || sampleRate <= 0)
        {
            return null;
        }

        var envelopes = new double[candidates.Count][];

        for (var i = 0; i < candidates.Count; i++)
        {
            envelopes[i] = CwProbabilisticDecoder.Envelope(
                samples, sampleRate, candidates[i]);
        }

        if (envelopes[0].Length < 8)
        {
            // Less audio than the decoder will read at all. Saying nothing is
            // right: a ranking over a handful of hops is a ranking of noise.
            return null;
        }

        var floor = Floor(envelopes);

        if (floor <= 0)
        {
            // Digital silence rather than a quiet band, which is an absence of
            // measurement and not a measurement of absence (HM-DEC-120).
            return null;
        }

        var scores = new double[candidates.Count];

        for (var i = 0; i < candidates.Count; i++)
        {
            scores[i] = CwProbabilisticDecoder
                .DecodeUngated(StandOn(envelopes[i], floor), candidates[i])
                .LikelihoodRatio;
        }

        return scores;
    }

    /// <summary>The tracker's own coarse grid, which is the candidate set.</summary>
    /// <returns>Every candidate pitch, low to high.</returns>
    /// <remarks>
    /// **THE SAME TWENTY-FIVE BINS THE SURVEY ALREADY SEARCHES**, so the ranking
    /// and the survey are answering one question about one set of places rather
    /// than two questions about two.
    /// </remarks>
    public static IReadOnlyList<double> Candidates()
    {
        var count = (int)Math.Round(
            (CwToneTracker.MaximumToneHz - CwToneTracker.MinimumToneHz)
            / CwToneTracker.CoarseSpacingHz) + 1;

        var list = new double[count];

        for (var i = 0; i < count; i++)
        {
            list[i] = CwToneTracker.MinimumToneHz
                + (i * CwToneTracker.CoarseSpacingHz);
        }

        return list;
    }

    /// <summary>The common noise floor, taken across the whole band.</summary>
    /// <param name="envelopes">One envelope per candidate.</param>
    /// <returns>The floor.</returns>
    /// <remarks>
    /// **THE LOUDEST PER-BIN FLOOR, SO NO CANDIDATE IS GIVEN A QUIETER NOISE
    /// SCALE THAN THE NOISIEST BIN GENUINELY HAS.** Each bin's own floor is its
    /// lower quartile, which is what the decoder's own estimator uses. Taking the
    /// median or the mean instead would let the emptiest bins pull the pedestal
    /// down toward themselves, which is the fault this exists to remove.
    /// </remarks>
    public static double Floor(IReadOnlyList<double[]> envelopes)
    {
        ArgumentNullException.ThrowIfNull(envelopes);

        var floor = 0.0;

        foreach (var envelope in envelopes)
        {
            floor = Math.Max(floor, Quartile(envelope));
        }

        return floor;
    }

    /// <summary>Stand an envelope on a common noise floor.</summary>
    /// <param name="envelope">The envelope.</param>
    /// <param name="floor">The floor.</param>
    /// <returns>The envelope as it would be if the band's floor were flat.</returns>
    /// <remarks>
    /// **COMBINED IN POWER AND NOT ADDED**, because an envelope magnitude is the
    /// length of a quadrature pair and two uncorrelated contributions add as
    /// squares. Adding the floor to the magnitude would shift every value by the
    /// same amount and leave the spread — and therefore the score — untouched,
    /// which is exactly the scale invariance being removed.
    /// </remarks>
    public static double[] StandOn(IReadOnlyList<double> envelope, double floor)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var stood = new double[envelope.Count];
        var squared = floor * floor;

        for (var i = 0; i < stood.Length; i++)
        {
            stood[i] = Math.Sqrt((envelope[i] * envelope[i]) + squared);
        }

        return stood;
    }

    /// <summary>The lower quartile of an envelope, which is its own noise floor.</summary>
    private static double Quartile(double[] envelope)
    {
        if (envelope.Length == 0)
        {
            return 0;
        }

        var sorted = (double[])envelope.Clone();

        Array.Sort(sorted);

        return sorted[sorted.Length / 4];
    }
}
