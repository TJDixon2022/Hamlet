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
    /// <summary>True when this is the gap between two words rather than a character.</summary>
    public bool IsWordGap => Text == MorseAlphabet.WordGap;

    /// <summary>True when something was heard and could not be resolved.</summary>
    public bool IsUnreadable => Confidence == CwConfidence.Unreadable;
}

/// <summary>
/// Turns two measurements into a confidence, and refuses to round it up.
/// </summary>
/// <remarks>
/// <para>WHY THIS EXISTS AT ALL. A decoder that prints a confident wrong letter
/// does specific damage in this feature that it does nowhere else in the app.
/// The person reading it has been told for years that CW takes an ear they do
/// not have. They look at a line of garbage, and the app has given them no way
/// to tell whether the signal was marginal or whether they are the problem.
/// Dimmed text says Hamlet is struggling. Clean text that is wrong says the
/// operator is, and that is a lie the whole project exists to stop
/// telling.</para>
/// <para>Two independent measurements, and the worse one wins. Timing clarity
/// says how far each element sat from the decision made about it. Signal margin
/// says how far the weakest part of the character stood above the noise. A
/// character can fail either way and passing one test does not excuse the
/// other, so they are combined with a minimum rather than an average. An
/// average would let a beautifully timed character buried in noise come out
/// looking certain.</para>
/// <para>Nothing anywhere may raise a score. Not a spell check, not a callsign
/// that nearly matches, not a word that would make sense. Those are all ways of
/// preferring a tidy transcript to a true one.</para>
/// </remarks>
public static class CwConfidenceModel
{
    /// <summary>At or above this, a character is shown as read.</summary>
    public const double HighAbove = 0.60;

    /// <summary>
    /// Below this a character is unreadable even though the pattern matched
    /// something.
    /// </summary>
    /// <remarks>
    /// The case this covers is real and would otherwise slip through: every
    /// element landed on a decision boundary, the pattern happened to spell a
    /// letter, and that letter is a coin toss wearing a name. "Something was
    /// here and Hamlet could not tell you what" is the true statement.
    /// </remarks>
    public const double UnreadableBelow = 0.12;

    /// <summary>Signal margin at or below which a character scores nothing.</summary>
    public const double PoorSignalDb = 5.0;

    /// <summary>Signal margin at or above which the signal stops being the limit.</summary>
    public const double GoodSignalDb = 18.0;

    /// <summary>
    /// How far the signal level may move between one character and the next
    /// before the decode stops being trustworthy, in decibels.
    /// </summary>
    /// <remarks>
    /// THE THIRD MEASUREMENT, and it catches a failure the other two cannot see
    /// at all. A signal sinking through a fade does not get noisy, it gets
    /// shorter: the dahs go under the threshold before the dits do, so "W"
    /// arrives as "E" with beautiful timing and a healthy margin above the
    /// noise, and both other tests wave it through. What gives it away is that
    /// the level moved. A character decoded while the signal was dropping ten
    /// decibels may be missing pieces, whatever the pieces that survived look
    /// like.
    /// </remarks>
    public const double LevelMoveSpanDb = 10.0;

    /// <summary>How much a steady level is worth, from 0 to 1.</summary>
    /// <param name="levelChangeDb">
    /// How far the signal margin moved since the character before.
    /// </param>
    /// <returns>0 to 1.</returns>
    public static double LevelStability(double levelChangeDb)
        => Math.Clamp(1 - (Math.Abs(levelChangeDb) / LevelMoveSpanDb), 0, 1);

    /// <summary>How much the signal margin alone is worth, from 0 to 1.</summary>
    /// <param name="signalToNoiseDb">Margin above the noise, in decibels.</param>
    /// <returns>0 to 1.</returns>
    public static double SignalClarity(double signalToNoiseDb)
        => Math.Clamp(
            (signalToNoiseDb - PoorSignalDb) / (GoodSignalDb - PoorSignalDb), 0, 1);

    /// <summary>Combine the measurements into one score.</summary>
    /// <param name="timingClarity">How cleanly the timings clustered, 0 to 1.</param>
    /// <param name="signalToNoiseDb">Margin above the noise and any rival, in decibels.</param>
    /// <param name="levelChangeDb">How far the level moved since the last character.</param>
    /// <returns>0 to 1.</returns>
    /// <remarks>
    /// The worst of the three, never the average. A character can fail in any
    /// one of these ways on its own, and passing two tests does not excuse
    /// failing the third. An average would let a beautifully timed character
    /// buried in noise come out looking certain, which is the whole failure
    /// this model exists to prevent.
    /// </remarks>
    public static double Score(
        double timingClarity, double signalToNoiseDb, double levelChangeDb = 0)
        => Math.Min(
            Math.Min(Math.Clamp(timingClarity, 0, 1), SignalClarity(signalToNoiseDb)),
            LevelStability(levelChangeDb));

    /// <summary>
    /// Which of the three states a score and a lookup result add up to.
    /// </summary>
    /// <param name="score">The combined score.</param>
    /// <param name="resolved">Whether the pattern named anything at all.</param>
    /// <returns>The confidence.</returns>
    public static CwConfidence Rate(double score, bool resolved)
        => !resolved || score < UnreadableBelow
            ? CwConfidence.Unreadable
            : score >= HighAbove
                ? CwConfidence.High
                : CwConfidence.Low;
}
