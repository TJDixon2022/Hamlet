using Hamlet.RadioEngine.Civ;

namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// One change Hamlet made to somebody's radio, and how to put it back
/// (HM-DEC-084).
/// </summary>
/// <param name="Write">Which setting, and its citation.</param>
/// <param name="Was">
/// What it was before, or null when it had never been read. **Null is a real
/// answer** and it is not the same as zero (HM-DEC-050).
/// </param>
/// <param name="Now">What it was set to.</param>
/// <param name="Reason">Why, in the operator's words.</param>
/// <param name="Outcome">Whether the radio confirmed it.</param>
/// <param name="AtUtc">When.</param>
public sealed record SettingChange(
    CivWrite Write,
    int? Was,
    int Now,
    string Reason,
    RigWriteOutcome Outcome,
    DateTime AtUtc)
{
    /// <summary>True only when a read-back agreed with what was asked for.</summary>
    public bool Confirmed => Outcome == RigWriteOutcome.Confirmed;

    /// <summary>True when the prior value can be restored.</summary>
    /// <remarks>
    /// **AN UNDO THAT INVENTS A PRIOR VALUE IS WORSE THAN NO UNDO.** If the
    /// setting was never read before it was changed, Hamlet does not know what
    /// to put back, and writing a plausible number into somebody's radio while
    /// calling it "restoring" would be the guess §0.0 forbids wearing the most
    /// reassuring word in the application.
    /// </remarks>
    public bool CanUndo => Was is not null;

    /// <summary>What Hamlet says it did.</summary>
    /// <remarks>
    /// EVERY WRITE IS ANNOUNCED (HM-DEC-084). A silent change to somebody's
    /// radio would break the whole posture of an application that says what it
    /// knows and where it learned it.
    /// </remarks>
    public string Says
    {
        get
        {
            var what = Reason.Length > 0 ? Reason : Write.Field.ToString();

            if (Outcome != RigWriteOutcome.Confirmed)
            {
                return $"{what} Hamlet asked the radio and it did not confirm, so "
                    + "it cannot say whether that took.";
            }

            return Was is { } before
                ? $"{what} It was {before} and it is {Now} now."
                : $"{what} It is {Now} now. Hamlet had not read it before, so it "
                  + "cannot put it back.";
        }
    }
}
