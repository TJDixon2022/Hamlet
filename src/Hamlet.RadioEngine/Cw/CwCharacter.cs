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
