using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>One chip on the Digital tab's mode strip.</summary>
/// <param name="Label">What it says, e.g. "FT8".</param>
/// <param name="IsLit">Whether the dial is in this mode's block.</param>
/// <remarks>
/// <para>**THE STRIP WAS STATIC FROM WORK INSTRUCTION 037 UNTIL UNIT 228.** FT8
/// was lit in the markup and the other three were greyed there, so the strip
/// asserted that the dial was in FT8 territory wherever the dial actually was,
/// which is a picture making a claim nobody had measured (§0.0, HM-DEC-092).
/// The unit that put a readiness line on the tab could not leave a caption
/// beside it saying something different.</para>
/// <para>**IT IS THE MAP THAT ANSWERS, NOT A FREQUENCY WRITTEN HERE.** The
/// neighborhood's own short label is what `ModeFollowPlan` already reads to
/// decide whether to put the radio into a data mode, so the strip, the map and
/// the automation cannot disagree about what lives at a frequency
/// (HM-DEC-054).</para>
/// <para>**NOTHING LIT IS A REAL ANSWER AND THE COMMON ONE.** In a Morse block,
/// in open ground, or in a digital block whose mode is not one of the four the
/// strip carries, every chip is unlit. Lighting the nearest one instead would be
/// a guess dressed as a reading.</para>
/// </remarks>
public sealed record DigitalModeChip(string Label, bool IsLit)
{
    /// <summary>
    /// The four modes the strip carries, in the order they are drawn.
    /// </summary>
    /// <remarks>
    /// **THE OWNER'S FOUR, KEPT.** They were chosen in August and this unit
    /// lights them rather than choosing a different set. The map knows blocks
    /// the strip has no chip for, JS8 and RTTY among them, and in one of those
    /// the honest picture is four unlit chips rather than a fifth invented here
    /// (§12.1).
    /// </remarks>
    public static readonly IReadOnlyList<string> Labels =
        new[] { "FT8", "FT4", "PSK31", "WSPR" };

    /// <summary>The strip for one neighborhood.</summary>
    /// <param name="here">Where the dial is, or null when the map has no block.</param>
    /// <returns>Four chips, at most one of them lit.</returns>
    public static IReadOnlyList<DigitalModeChip> For(Neighborhood? here)
    {
        // A block that is not digital territory lights nothing, and so does a
        // frequency the map has no block for. Both are the absence of a reading
        // rather than a reading of absence, and neither is a licence to guess.
        var label = here is { Family: ModeFamily.Digital }
            ? here.ShortName.Trim().ToUpperInvariant()
            : "";

        return Labels
            .Select(one => new DigitalModeChip(one, one == label))
            .ToList();
    }
}
