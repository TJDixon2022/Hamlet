using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Explore;

/// <summary>How a neighborhood row answers for one owned setting.</summary>
public enum OwnedAnswer
{
    /// <summary>The row states a value and Hamlet sets it.</summary>
    Stated,

    /// <summary>
    /// The row says this one is the operator's own, and nothing writes it.
    /// </summary>
    /// <remarks>
    /// **DEFERRING IS AN ANSWER AND BEING SILENT IS NOT.** A setting nobody's
    /// business is said to be nobody's business, which is a different fact from
    /// a setting somebody forgot.
    /// </remarks>
    OperatorsChoice,

    /// <summary>
    /// The row says nothing, so the setting is left exactly as it was.
    /// </summary>
    /// <remarks>
    /// **ABSENT IS NEITHER OF THE OTHER TWO AND IT IS REPORTED RATHER THAN
    /// FAILED.** The digital rows belong to another conversation, and a unit that
    /// filled them in by guesswork would be writing bytes to somebody's radio on
    /// no authority at all (§12.4).
    /// </remarks>
    Absent,
}

/// <summary>One setting Hamlet sets as a consequence of the operator's intent.</summary>
/// <param name="Control">What the operator would call it.</param>
/// <param name="Field">The rig field it lives in.</param>
/// <param name="Citation">Where §4 cites the command that writes it.</param>
public readonly record struct OwnedSetting(
    string Control, RigField Field, string Citation);

/// <summary>
/// The settings Hamlet owns, and the contract that every mode's row answers for
/// every one of them.
/// </summary>
/// <remarks>
/// <para>**ONE OWNED LIST, AND EVERY ROW ANSWERS FOR EVERY ENTRY** (Tim's ruling
/// of 2026-08-29). Two conversations are now building against one radio — this one
/// works CW at night and another works FT8 in daylight — and each was writing its
/// own set of deltas. **Whichever ran last won on whatever it happened to touch**,
/// and nothing stated what became of a setting a mode never mentioned.</para>
/// <para>**WHAT THE CONTRACT BUYS.** Switching CW to FT8 and back lands in the
/// same place every time, because the second mode restores what the first changed
/// rather than leaving whatever it did not think about. Two conversations cannot
/// write conflicting partial deltas, because a row is complete or it is reported
/// as incomplete. **Rejected: per-mode deltas**, which is what produced the
/// problem.</para>
/// <para>**WHAT IS DELIBERATELY NOT OWNED.** The CW pitch and the AF level change
/// what the operator hears in his headphones, which is his ear rather than a
/// receive condition. Break-in is a transmit setting and §0.2 keeps this off it
/// entirely — the manual's footnote 2 on p. 19-7 makes PC text become transmitted
/// CW while break-in is on, so it is the last thing an automatic write should
/// reach. The noise blanker level, the noise reduction level and the notch
/// position matter only when their function is on, and Hamlet turns those off, so
/// setting a level for a disabled function is noise in the write log.</para>
/// </remarks>
public static class OwnedSettings
{
    /// <summary>The twelve, each with the §4 citation for the byte that writes it.</summary>
    /// <remarks>
    /// **NO BYTE IS WRITTEN THAT IS NOT CITED** (HM-DEC-084). The citation
    /// travels with the setting rather than sitting in a comment beside the
    /// command, so a row that states a value has a page number behind it.
    /// </remarks>
    public static IReadOnlyList<OwnedSetting> All { get; } =
    [
        new("mode and data flag", RigField.Mode, "19-11"),
        new("filter slot", RigField.FilterSelection, "19-11"),
        new("filter width", RigField.FilterBandwidth, "19-4, scale 4-6"),
        new("auto notch", RigField.AutoNotch, "19-3"),
        new("manual notch", RigField.ManualNotch, "19-3"),
        new("noise blanker", RigField.NoiseBlanker, "19-3"),
        new("noise reduction", RigField.NoiseReduction, "19-3"),
        new("AGC", RigField.Agc, "19-3"),
        new("preamp", RigField.Preamp, "19-3"),
        new("attenuator", RigField.Attenuator, "19-3"),
        new("RF gain", RigField.RfGain, "19-3"),
        new("squelch", RigField.Squelch, "19-3"),
    ];

    /// <summary>
    /// The scope span, which is owned and has no cited byte to write it.
    /// </summary>
    /// <remarks>
    /// **IT IS ON THE LIST AND IT IS SPOKEN RATHER THAN WRITTEN.** §4 carries no
    /// CI-V command for the span — the sub-command list on p. 19-7 runs 00, 10,
    /// 11, 12 and up, and this project has read the pages for 10 and 11 only. So
    /// a row may state what the span should be and Hamlet will say it, and it
    /// will not send a byte it cannot cite. Leaving it off the list entirely
    /// would lose the sentence that saves an operator an hour in front of a block
    /// drawn seven pixels wide.
    /// </remarks>
    public const string SpokenOnly = "scope span";

    /// <summary>What a row says about every owned setting.</summary>
    /// <param name="hood">The block.</param>
    /// <returns>One answer per owned setting, in <see cref="All"/>'s order.</returns>
    /// <remarks>
    /// **THIS IS THE COVERAGE REPORT AND IT FAILS NOTHING.** A row that says
    /// nothing about a setting leaves it alone, which is the honest behaviour
    /// while another conversation owns that row.
    /// </remarks>
    public static IReadOnlyList<(OwnedSetting Setting, OwnedAnswer Answer)> Coverage(
        Neighborhood? hood)
    {
        var stated = ReceiverConditions.ForBlock(hood);
        var answers = new List<(OwnedSetting, OwnedAnswer)>(All.Count);

        foreach (var owned in All)
        {
            var row = stated.FirstOrDefault(c => c.Field == owned.Field);

            answers.Add((
                owned,
                row is null ? OwnedAnswer.Absent
                    : row.CanBeWritten ? OwnedAnswer.Stated
                    : OwnedAnswer.OperatorsChoice));
        }

        return answers;
    }
}
