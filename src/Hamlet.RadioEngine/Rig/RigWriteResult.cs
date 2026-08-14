namespace Hamlet.RadioEngine.Rig;

/// <summary>What became of a write.</summary>
/// <remarks>
/// FOUR ANSWERS, AND ONLY ONE OF THEM IS "IT WORKED" (§0.0, HM-DEC-056). A write
/// that was sent is not a write that took effect, and the difference matters
/// more here than for a read: a wrong reading shows a wrong number, and a mode
/// Hamlet believes it set and did not is the app and the radio disagreeing about
/// what the operator is listening to.
/// </remarks>
public enum RigWriteOutcome
{
    /// <summary>The radio acknowledged it.</summary>
    Confirmed,

    /// <summary>The radio answered and refused.</summary>
    Refused,

    /// <summary>Nothing came back inside the timeout.</summary>
    NoAnswer,

    /// <summary>This radio does not do this (HM-DEC-030).</summary>
    NotSupported,
}

/// <summary>The outcome of one write, with enough to say why.</summary>
/// <param name="Outcome">What became of it.</param>
/// <param name="Detail">
/// What to tell the operator, in one sentence, or "" when it simply worked.
/// </param>
/// <param name="Source">
/// The command that was sent, verbatim enough to check: "CI-V 26" (§0.0.1).
/// </param>
public sealed record RigWriteResult(RigWriteOutcome Outcome, string Detail, string Source)
{
    /// <summary>True only when the radio said yes.</summary>
    public bool Worked => Outcome == RigWriteOutcome.Confirmed;

    /// <summary>The radio acknowledged it.</summary>
    /// <param name="source">The command sent.</param>
    /// <returns>The result.</returns>
    public static RigWriteResult Confirmed(string source)
        => new(RigWriteOutcome.Confirmed, "", source);

    /// <summary>The radio answered and refused.</summary>
    /// <param name="source">The command sent.</param>
    /// <returns>The result.</returns>
    public static RigWriteResult Refused(string source)
        => new(
            RigWriteOutcome.Refused,
            "The radio turned that down, so it is still on whatever it was on.",
            source);

    /// <summary>Nothing came back.</summary>
    /// <param name="source">The command sent.</param>
    /// <returns>The result.</returns>
    public static RigWriteResult NoAnswer(string source)
        => new(
            RigWriteOutcome.NoAnswer,
            "The radio did not answer, so Hamlet cannot say what mode it is in "
            + "now.",
            source);

    /// <summary>This radio does not do this.</summary>
    /// <param name="why">What decided that.</param>
    /// <returns>The result.</returns>
    public static RigWriteResult NotSupported(string why)
        => new(RigWriteOutcome.NotSupported, "", why);
}
