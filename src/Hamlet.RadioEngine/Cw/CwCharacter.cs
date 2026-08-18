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
}

/// <summary>
/// A provisional reading and what the settled pass made of the same audio
/// (HM-DEC-096, phase 1).
/// </summary>
/// <param name="Provisional">What the leading edge said.</param>
/// <param name="Settled">What the second pass said.</param>
/// <param name="Agreed">Whether the two read the same character.</param>
/// <remarks>
/// **IN MEMORY AND EXPORTABLE, NEVER WRITTEN TO DISK.** This is diagnostic
/// (§0.0.1) rather than a record of the air, and a log that grows on disk needs a
/// retention policy nobody has designed.
/// </remarks>
public readonly record struct CwRevision(
    CwCharacter Provisional, CwCharacter Settled, bool Agreed);

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
/// says how far the weakest part of the character stood above the noise and
/// above any station near enough to be confused with it. A character can fail
/// either way and passing one test does not excuse the other, so they are
/// combined with a minimum rather than an average. An average would let a
/// beautifully timed character buried in noise come out looking certain.</para>
/// <para>Then one veto on top, for the case neither score can see: another
/// station within a few decibels does not add noise, it moves the gate, and the
/// letter that comes out is a different letter with perfect timing and a
/// healthy margin. See <see cref="ContestedWithinDb"/>.</para>
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

    /// <summary>
    /// Below this margin the decoder emits nothing at all (HM-DEC-097).
    /// </summary>
    /// <remarks>
    /// <para>**SEVENTEEN, AND IT IS HM-DEC-097 TRANSLATED RATHER THAN
    /// RENEGOTIATED.** That ruling says the decoder refuses below nought
    /// decibels signal to noise and copies nothing into the band where it is
    /// half wrong. Its decibels are the broadband ratio a fixture is generated
    /// at; the decoder measures inside a narrow tone filter and reads about
    /// seventeen higher for the same audio — 17.2 to 19.0 where the fixture was
    /// generated at nought, 15.3 to 17.1 at minus two, 7.6 to 14.4 at minus
    /// five. So seventeen here cuts in at the ruling's own line.</para>
    /// <para>**FIFTEEN WAS REJECTED** because it lets minus two through, which
    /// is the case the ruling names at 0.44 of the message invented. **Ten was
    /// rejected** as leaving open the whole band the ruling exists to silence.
    /// </para>
    /// <para>A trained ear copies to roughly nought decibels, so refusing there
    /// meets the stated goal — decode almost anything the operator can hear —
    /// rather than falling short of it. **A degraded label was rejected as a
    /// substitute**: marking a message does not make a plausible wrong callsign
    /// on screen any less actionable (§0.0).</para>
    /// </remarks>
    public const double RefusalFloorDb = 17.0;

    /// <summary>Signal margin at or above which the signal stops being the limit.</summary>
    public const double GoodSignalDb = 18.0;

    /// <summary>
    /// How close another station may come, in decibels, before this one stops
    /// being worth vouching for.
    /// </summary>
    /// <remarks>
    /// THE VETO, and it is a rule rather than a score because the failure it
    /// catches is not a matter of degree. A station within a few decibels and a
    /// couple of hundred hertz does not merely add noise: its keying lifts the
    /// gate's idea of where a signal sits, the threshold rides up with it, and
    /// the wanted station's dahs get truncated into dits. The result is a
    /// different letter with clean timing and a healthy margin above the noise,
    /// which is exactly the shape of thing both scores are built to wave
    /// through.
    /// <para>So a character that arrived while somebody else was that close
    /// cannot be marked certain, whatever else is true about it. Hamlet can
    /// still show it, and dimmed is the honest way to show it: somebody was
    /// sitting on top of this and the decoder is not going to pretend
    /// otherwise (§0.0).</para>
    /// </remarks>
    public const double ContestedWithinDb = 10.0;

    /// <summary>How much the signal margin alone is worth, from 0 to 1.</summary>
    /// <param name="signalToNoiseDb">Margin above the noise, in decibels.</param>
    /// <returns>0 to 1.</returns>
    public static double SignalClarity(double signalToNoiseDb)
        => Math.Clamp(
            (signalToNoiseDb - PoorSignalDb) / (GoodSignalDb - PoorSignalDb), 0, 1);

    /// <summary>Combine the measurements into one score.</summary>
    /// <param name="timingClarity">How cleanly the timings clustered, 0 to 1.</param>
    /// <param name="signalToNoiseDb">Margin above the noise and any rival, in decibels.</param>
    /// <returns>0 to 1.</returns>
    /// <remarks>
    /// The worse of the two, never the average. A character can fail either way
    /// on its own and passing one test does not excuse failing the other. An
    /// average would let a beautifully timed character buried in noise come out
    /// looking certain, which is the whole failure this model exists to prevent.
    /// </remarks>
    public static double Score(double timingClarity, double signalToNoiseDb)
        => Math.Min(
            Math.Clamp(timingClarity, 0, 1), SignalClarity(signalToNoiseDb));

    /// <summary>
    /// Which of the three states a score and a lookup result add up to.
    /// </summary>
    /// <param name="score">The combined score.</param>
    /// <param name="resolved">Whether the pattern named anything at all.</param>
    /// <param name="contestedMarginDb">
    /// How close the nearest other station came while this character was
    /// arriving, in decibels. Large when nobody else was there.
    /// </param>
    /// <returns>The confidence.</returns>
    public static CwConfidence Rate(
        double score, bool resolved, double contestedMarginDb = double.MaxValue)
    {
        if (!resolved || score < UnreadableBelow)
        {
            return CwConfidence.Unreadable;
        }

        // The veto. Nothing raises a confidence, and this is the one thing that
        // lowers one on evidence outside the score.
        return score >= HighAbove && contestedMarginDb >= ContestedWithinDb
            ? CwConfidence.High
            : CwConfidence.Low;
    }
}
