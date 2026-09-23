namespace Hamlet.RadioEngine.Cw;

/// <summary>One candidate pitch and what the decoder read at it.</summary>
/// <param name="ToneHz">The pitch scored.</param>
/// <param name="Ratio">
/// The decoder's own log-likelihood ratio per hop, the quantity
/// <see cref="CwProbabilisticDecoder.Gate"/> is expressed in.
/// </param>
/// <param name="Characters">How many characters the window spelled there.</param>
/// <param name="WordsPerMinute">The speed the search settled on.</param>
/// <param name="Text">What it spelled, for the record.</param>
/// <param name="MedianSpanMargin">
/// The median of the characters' own <see cref="CwProbabilisticCharacter.SpanMargin"/>
/// — how far each character's marks stood above the noise, per hop.
/// </param>
public readonly record struct RankedPitch(
    double ToneHz,
    double Ratio,
    int Characters,
    double WordsPerMinute,
    string Text,
    double MedianSpanMargin = 0);

/// <summary>
/// Choosing a pitch by what the decoder reads at it, rather than by how the
/// energy in that bin clusters.
/// </summary>
/// <remarks>
/// <para>**WHY THIS EXISTS.** Six statistics have been measured against *is a
/// station keying in this bin* and all six are dead — cluster separation, the
/// dah/dit ratio, level spread, lift over the band floor, the quantisation
/// residual, and agreement between fitted units. On the last of them an empty
/// band scored 0.028 and the adjudicated callsign `VA3VRR` scored 0.400, which
/// is the wrong way round. **Every one of the six measures clustering**, and at
/// bin level, from nineteen marks and three seconds, clustering has no
/// answer.</para>
/// <para>HM-DEC-125 named the way out and made it conditional: *"Scoring
/// candidates by their own speed estimator at the tracked pitch is the direction
/// if a measurement later shows a gap; it is a measurement of reading rather
/// than of clustering."* **And HM-DEC-095 asks for exactly this** — a note is
/// chosen by how it is keyed and never by how loud it is, and ranking bins by
/// what each reads is that rule taken literally.</para>
/// <para>**IT RANKS AND IT DOES NOT DECIDE.** <see cref="Rank"/> returns the
/// candidates in order and asserts nothing about whether the best one holds a
/// station. That question is <see cref="Winner"/>'s, and the answer it gives is
/// deliberately not a bare comparison against
/// <see cref="CwProbabilisticDecoder.Gate"/> — see the warning there, which is
/// the measured reason this class does not do what it was commissioned to
/// do.</para>
/// <para>**PURE, AND NOTHING HERE TOUCHES A RADIO.** Envelopes in, an ordering
/// out. Same input, same order (§5).</para>
/// </remarks>
public static class CwPitchRanking
{
    /// <summary>
    /// How many candidates a ranking may carry inside one survey cadence.
    /// </summary>
    /// <remarks>
    /// **MEASURED, NOT CHOSEN.** One decode of a three-second window costs about
    /// 22 ms once the envelope is taken, against a cadence of 500 ms
    /// (<see cref="CwProbabilisticStream.ReadEverySeconds"/>), so twenty-two fit
    /// if the whole cadence is spent on it. Eight is a quarter of the budget,
    /// which leaves the cadence doing what it already does.
    /// </remarks>
    public const int Shortlist = 8;

    /// <summary>Score each candidate pitch by what the decoder reads at it.</summary>
    /// <param name="samples">The audio to score over.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="candidateHz">The pitches to score, in any order.</param>
    /// <returns>
    /// The candidates, best-reading first. Empty when nothing was offered.
    /// </returns>
    /// <remarks>
    /// **THE SHORTLIST MAY BE DRAWN BY ENERGY AND THE CHOICE MAY NOT BE.** A
    /// caller that hands in the loudest eight bins has narrowed the search and
    /// not made the decision, which is the distinction HM-DEC-095 turns on. What
    /// orders them here is only what each one read.
    /// </remarks>
    public static IReadOnlyList<RankedPitch> Rank(
        IReadOnlyList<float> samples, int sampleRate, IReadOnlyList<double> candidateHz)
    {
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentNullException.ThrowIfNull(candidateHz);

        var scored = new List<RankedPitch>(candidateHz.Count);

        foreach (var hz in candidateHz)
        {
            var envelope = CwProbabilisticDecoder.Envelope(samples, sampleRate, hz);

            // **UNGATED, BECAUSE THE GATE IS A DECISION AND THIS IS A
            // MEASUREMENT.** A gated decode returns an empty character list
            // below the floor, so every candidate under it would score alike and
            // the ranking could not tell a near miss from silence.
            var read = CwProbabilisticDecoder.DecodeUngated(envelope, hz);

            scored.Add(new RankedPitch(
                hz, read.LikelihoodRatio, read.Characters.Count,
                read.WordsPerMinute, read.Text, Median(read.Characters)));
        }

        scored.Sort((a, b) => b.Ratio.CompareTo(a.Ratio));

        return scored;
    }

    /// <summary>The middle character's own evidence per hop.</summary>
    /// <param name="characters">What the window spelled.</param>
    /// <returns>The median span margin, or zero when nothing was spelled.</returns>
    /// <remarks>
    /// **THE MEDIAN AND NOT THE MEAN**, because one character read out of a
    /// strong opening can carry a mean over a window of nothing, and the question
    /// is whether this pitch reads *typically* rather than whether it ever did.
    /// </remarks>
    private static double Median(IReadOnlyList<CwProbabilisticCharacter> characters)
    {
        if (characters.Count == 0)
        {
            return 0;
        }

        var margins = characters
            .Where(c => c.SpanHops > 0)
            .Select(c => c.SpanMargin)
            .OrderBy(m => m)
            .ToArray();

        return margins.Length == 0 ? 0 : margins[margins.Length / 2];
    }

    /// <summary>The best-reading candidate, or null when none was offered.</summary>
    /// <param name="ranked">The output of <see cref="Rank"/>.</param>
    /// <returns>The winner, or null.</returns>
    /// <remarks>
    /// <para>**A WINNER IS NOT A STATION, AND THIS IS THE MEASURED REASON THE
    /// RANKING IS NOT WIRED TO THE TRACKER.**</para>
    /// <para>Work instruction 032 proposed that ranking needs no threshold
    /// because <see cref="CwProbabilisticDecoder.Gate"/> would refuse the winner
    /// afterwards. **That was measured on 2026-08-27 and it does not hold.** The
    /// gate of 1.40 was calibrated against one pitch — the one the tracker had
    /// already settled on — and its own evidence records
    /// `cw-2026-08-20-014854`, holding nothing, at a highest window ratio of
    /// 0.840.</para>
    /// <para>**TAKE THE BEST OF THE COARSE BANK INSTEAD AND THAT SAME EMPTY
    /// RECORDING SCORES 4.47 AND SPELLS 93 CHARACTERS.** Both recordings that
    /// hold nothing clear the gate at every window length tried, three seconds,
    /// six and twelve. The maximum over twenty-five bins is a different
    /// statistic from a single draw, and a floor calibrated for one does not
    /// transfer to the other. Somewhere in six hundred hertz of noise there is
    /// always a pitch that reads.</para>
    /// <para>So this returns the winner and says nothing about what it is. **A
    /// caller that treats it as a station will put ninety-three characters of
    /// nothing on an empty band**, which is the prime directive broken as
    /// directly as it can be (§0.0).</para>
    /// </remarks>
    public static RankedPitch? Winner(IReadOnlyList<RankedPitch> ranked)
    {
        ArgumentNullException.ThrowIfNull(ranked);

        return ranked.Count > 0 ? ranked[0] : null;
    }

    /// <summary>The pitches the coarse bank would offer across the whole range.</summary>
    /// <returns>300 to 900 hertz at the coarse spacing.</returns>
    /// <remarks>
    /// Read from <see cref="CwToneTracker"/>'s own constants rather than
    /// restated, so a change to the bank cannot leave two plans disagreeing (§0).
    /// </remarks>
    public static double[] CoarseBank()
    {
        var count = (int)Math.Round(
            (CwToneTracker.MaximumToneHz - CwToneTracker.MinimumToneHz)
            / CwToneTracker.CoarseSpacingHz) + 1;

        var bank = new double[count];

        for (var i = 0; i < count; i++)
        {
            bank[i] = CwToneTracker.MinimumToneHz + (i * CwToneTracker.CoarseSpacingHz);
        }

        return bank;
    }
}
