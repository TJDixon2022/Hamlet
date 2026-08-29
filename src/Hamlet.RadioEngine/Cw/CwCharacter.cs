namespace Hamlet.RadioEngine.Cw;

/// <summary>How much the decoder is willing to stand behind a character.</summary>
/// <remarks>
/// THREE STATES, AND THE THIRD ONE IS THE POINT (§0.0). A decoder with two
/// states has to pick a letter for everything it hears, and the letters it
/// picks under pressure are indistinguishable from the ones it is sure of.
/// </remarks>
public enum CwConfidence
{
    /// <summary>
    /// The timings clustered cleanly and the tone stood well above the noise.
    /// Shown normally, because it can be read as what was sent.
    /// </summary>
    High,

    /// <summary>
    /// The timings were ambiguous, the signal was marginal, or both. Shown
    /// dimmed, because the reader has to be able to see Hamlet struggling.
    /// </summary>
    Low,

    /// <summary>
    /// Something was clearly heard and could not be resolved into anything.
    /// Shown as a placeholder and never as a letter.
    /// </summary>
    Unreadable,
}

/// <summary>
/// Which of Hamlet's two passes produced a character (HM-DEC-096, phase 1).
/// </summary>
/// <remarks>
/// <para>**HAMLET READS THE SAME AUDIO TWICE AND THE READER HAS TO BE ABLE TO
/// TELL WHICH ONE IS SPEAKING.** The streaming pass answers at the leading edge,
/// while somebody is still sending, and it decides where the threshold is before
/// it has heard the stretch that threshold describes. The settled pass runs a few
/// seconds behind with the whole stretch in hand.</para>
/// <para>A provisional reading shown as though it were final is §0.0 broken by
/// omission: it is a guess presented as a decode, however good a guess it usually
/// is.</para>
/// </remarks>
public enum CwReadingStage
{
    /// <summary>
    /// The leading edge, read as the elements completed. Correct far more often
    /// than not and never final.
    /// </summary>
    Provisional,

    /// <summary>
    /// The leading edge while the settled pass has refused or is re-acquiring, so
    /// nothing is coming along behind to confirm it. Shown marked (phase 4).
    /// </summary>
    Unstable,

    /// <summary>
    /// Read a second time from a threshold fitted to the stretch it sits in. This
    /// is what the transcript keeps.
    /// </summary>
    Settled,
}

/// <summary>
/// One decoded character, with the evidence behind it.
/// </summary>
/// <param name="Text">
/// What was heard: a letter, a digit, a prosign such as <c>&lt;AR&gt;</c>, a
/// space, or the unreadable placeholder.
/// </param>
/// <param name="Confidence">How much the decoder stands behind it.</param>
/// <param name="Score">The confidence as a number from 0 to 1.</param>
/// <param name="Pattern">The dots and dashes as actually measured.</param>
/// <param name="SignalToNoiseDb">
/// How far the weakest element of this character stood above the noise.
/// </param>
/// <param name="WordsPerMinute">The sending speed as estimated at this moment.</param>
/// <param name="At">When it arrived, measured from the start of the audio.</param>
/// <remarks>
/// <para>THE EVIDENCE TRAVELS WITH THE CHARACTER (§0.0.1). A wrong decode that
/// arrives with the pattern that produced it, the signal margin at the time and
/// the speed the decoder believed it was running at is something somebody can
/// fix. A wrong letter on its own is an argument.</para>
/// <para><see cref="At"/> comes from counting samples and never from a clock,
/// which is what lets a fixture decode identically on any machine at any speed
/// (§5).</para>
/// </remarks>
public sealed record CwCharacter(
    string Text,
    CwConfidence Confidence,
    double Score,
    string Pattern,
    double SignalToNoiseDb,
    int WordsPerMinute,
    TimeSpan At)
{
    /// <summary>Which pass read it (HM-DEC-096, phase 1).</summary>
    public CwReadingStage Stage { get; init; } = CwReadingStage.Provisional;

    /// <summary>True when this is the gap between two words rather than a character.</summary>
    public bool IsWordGap => Text == MorseAlphabet.WordGap;

    /// <summary>True when something was heard and could not be resolved.</summary>
    public bool IsUnreadable => Confidence == CwConfidence.Unreadable;

    /// <summary>True when nothing is coming along behind to confirm this.</summary>
    public bool IsUnstable => Stage == CwReadingStage.Unstable;

    /// <summary>
    /// How much better this character's own span is explained by keying than by
    /// the key having been up throughout it.
    /// </summary>
    /// <remarks>
    /// <para>**THE EVIDENCE FOR THIS CHARACTER, RATHER THAN FOR THE WINDOW IT
    /// SAT IN** (§0.0.1, HM-DEC-007). <see cref="Score"/> is the whole window's
    /// likelihood ratio, so every character read out of one window carries the
    /// same number and nothing beside a wrong letter said whether that letter
    /// had a signal behind it. This one is measured over the character's own
    /// marks and nothing else.</para>
    /// <para>It is written to the capture sidecar and to nothing else. What the
    /// screen asserts does not change on the strength of a field added to make
    /// later work measurable (§0.0), and a number a reader cannot calibrate is
    /// worse on a display than absent.</para>
    /// <para><see cref="double.NaN"/> where the pass that produced this
    /// character does not measure it, which is not the same as zero: zero is a
    /// character all-key-up explains exactly as well.</para>
    /// </remarks>
    public double SpanLogLikelihoodRatio { get; init; } = double.NaN;

    /// <summary>How many hops this character spans, or nought where unmeasured.</summary>
    public int SpanHops { get; init; }

    /// <summary>
    /// How much better the winning reading was than the nearest alternative
    /// arriving at the same place.
    /// </summary>
    /// <remarks>
    /// **RECORDED AND READ BY NOTHING** (§0.0.1). See
    /// <see cref="CwProbabilisticCharacter.MarginLlr"/> for what it is and why
    /// the quantity beside it is not enough. It goes to the capture sidecar and
    /// to the record, and to no display: a number a reader cannot calibrate is
    /// worse on a screen than absent.
    /// </remarks>
    public double MarginLlr { get; init; } = double.NaN;

    /// <summary>
    /// The probability that this character is what the path says, marginalised
    /// over every path through the lattice.
    /// </summary>
    /// <remarks>
    /// **THE FIRST CONFIDENCE HERE THAT CANNOT GROW WITH LOUDNESS.** Five
    /// quantities have been measured against correctness and all five were
    /// negative — the fit ratio at −0.179 and −0.203, `MarginLlr` at −0.351,
    /// `MarginShareForRecord` at −0.345, `SpanMarginForRecord` at −0.190 — each
    /// a difference of path scores carrying an unbounded level term. A posterior
    /// is a ratio over the sum of all paths, so the level cancels.
    /// **NaN where none could be computed, which is not a probability of nought**
    /// (§0.0).
    /// </remarks>
    public double Posterior { get; init; } = double.NaN;

    /// <summary>The widest either likelihood figure may be written as.</summary>
    /// <remarks>
    /// **A MILLION, BECAUSE THE RECORD HAS PRINTED QUADRILLIONS.** The
    /// `6:27306879.3` family is a per-hop log-likelihood on a recording whose
    /// noise estimate went to nothing, and a sheet carrying it is a sheet nobody
    /// reads the rest of. Clamping is a statement about the *record's* range and
    /// not about the measurement, so a clamped figure is written with a mark
    /// saying it was clamped rather than silently.
    /// </remarks>
    public const double WidestRecordedLlr = 1_000_000;

    /// <summary>
    /// The character's own evidence per hop, in the units the window ratio uses.
    /// </summary>
    /// <remarks>
    /// **THE ONLY FORM OF THIS QUANTITY THAT MEANS THE SAME THING ON TWO
    /// RECORDINGS.** The raw sum scales with the recording's own noise estimate,
    /// so a correct character on one capture scores three thousand and one on
    /// another scores eleven billion. Divided by its own span it is the same
    /// arithmetic the window ratio is, over one character instead of a window.
    /// </remarks>
    public double SpanMarginForRecord
        => SpanHops <= 0 ? 0 : SpanLogLikelihoodRatio / SpanHops;

    /// <summary>
    /// How far ahead the winning reading finished, as a share of the evidence
    /// the character carried at all.
    /// </summary>
    /// <remarks>
    /// <para>**DIMENSIONLESS BY CONSTRUCTION, WHICH IS THE POINT.** Both figures
    /// are sums of log-likelihoods computed through the same noise estimate, so
    /// the estimate cancels in the quotient. Unit 1.11.14 measured the raw
    /// <see cref="MarginLlr"/> across this repository's captures and found it
    /// reaching 2.98 × 10⁸ on one recording and 1.8 on another, which is the
    /// same incomparability <see cref="SpanMarginForRecord"/> exists to escape.
    /// Measured across the same 1,580 characters, this quotient's entire
    /// observed range is −20.1 to +2.45.</para>
    /// <para>**IT IS NOT A SECOND COPY OF WHAT THE SHEET ALREADY PRINTS.** Both
    /// inputs are clamped at <see cref="WidestRecordedLlr"/> before they reach a
    /// record, so on precisely the recordings where the raw margin runs to
    /// hundreds of millions the printed figure is `>1000000` and the quotient
    /// cannot be recovered from it.</para>
    /// <para>**AND IT DOES NOT SEPARATE A GOOD CHARACTER FROM A BAD ONE.**
    /// Split by whether the recording carries an adjudicated anchor, the medians
    /// are 0.004 and 0.005. What it says instead is worth reading on its own: the
    /// runner-up path is almost always within a few thousandths of the winner,
    /// so a character's second-best reading fitting nearly as well is the normal
    /// case rather than the suspicious one.</para>
    /// <para>Nought where the character carried no span to measure against,
    /// which is not the same as a margin of nought (§0.0).</para>
    /// </remarks>
    public double MarginShareForRecord
        => double.IsNaN(SpanLogLikelihoodRatio)
            || double.IsNaN(MarginLlr)
            || Math.Abs(SpanLogLikelihoodRatio) < 1e-6
            ? double.NaN
            : MarginLlr / SpanLogLikelihoodRatio;
}
