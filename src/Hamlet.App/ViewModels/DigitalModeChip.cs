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
/// <param name="IsChosen">
/// Whether this is the sub-mode the operator last picked. **A different fact from
/// <paramref name="IsLit"/> and never conflated with it** — see the remarks on
/// <see cref="For(Neighborhood?, string?)"/>.
/// </param>
public sealed record DigitalModeChip(string Label, bool IsLit, bool IsChosen)
{
    /// <summary>True where the chip is picked but the dial is not in its block.</summary>
    /// <remarks>
    /// **THE ONE STATE THE STRIP HAD NO WAY TO DRAW.** He asked for FT8 and the
    /// dial is somewhere else — because the tune has not landed yet, because it
    /// did not take, or because the band has no FT8 block at all. Drawing it as
    /// lit would say the radio is there; drawing it as unlit would lose his
    /// choice. It is its own appearance.
    /// </remarks>
    public bool IsChosenElsewhere => IsChosen && !IsLit;

    /// <summary>True where the chip is neither lit nor chosen.</summary>
    /// <remarks>
    /// **THE THREE APPEARANCES ARE EXCLUSIVE AND EXHAUSTIVE**, computed here
    /// rather than assembled out of negations in the markup, so a fourth state
    /// cannot appear by accident and two of them can never draw at once.
    /// </remarks>
    public bool IsPlain => !IsLit && !IsChosen;

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
    /// <param name="chosen">
    /// The sub-mode the operator last picked, or null where he has picked none.
    /// </param>
    /// <returns>Four chips, at most one lit and at most one chosen.</returns>
    /// <remarks>
    /// <para>**TWO FACTS, KEPT APART** (unit 251 task 3). `IsLit` is a
    /// measurement — the dial is inside this mode's block, and the map is what
    /// answers. `IsChosen` is a preference — this is the one he pressed, and it
    /// is remembered between evenings.</para>
    /// <para>**THEY DISAGREE OFTEN AND THAT IS THE INTERESTING CASE.** He presses
    /// FT8 on a band with no FT8 block, or the tune does not take, or the app has
    /// just started and nothing has moved the dial at all. Merging them into one
    /// flag would make a remembered press look like a reading of the radio, which
    /// is a picture asserting something nobody measured (§0.0, HM-DEC-092).</para>
    /// </remarks>
    public static IReadOnlyList<DigitalModeChip> For(Neighborhood? here, string? chosen)
    {
        // A block that is not digital territory lights nothing, and so does a
        // frequency the map has no block for. Both are the absence of a reading
        // rather than a reading of absence, and neither is a licence to guess.
        var label = here is { Family: ModeFamily.Digital }
            ? here.ShortName.Trim().ToUpperInvariant()
            : "";

        var picked = chosen?.Trim().ToUpperInvariant() ?? "";

        return Labels
            .Select(one => new DigitalModeChip(one, one == label, one == picked))
            .ToList();
    }

    /// <summary>The strip for one neighborhood, with nothing chosen.</summary>
    /// <param name="here">Where the dial is, or null when the map has no block.</param>
    /// <returns>Four chips, at most one of them lit.</returns>
    public static IReadOnlyList<DigitalModeChip> For(Neighborhood? here)
        => For(here, null);

    /// <summary>Whether a label is one of the four the strip carries.</summary>
    /// <param name="label">A candidate sub-mode name, from settings or a press.</param>
    /// <returns>The canonical label, or null where it is not one of the four.</returns>
    /// <remarks>
    /// **A SETTINGS FILE NAMING A FIFTH MODE GETS NONE.** The strip carries the
    /// owner's four; anything else read out of `settings.json` is dropped rather
    /// than shown, because a chip the strip cannot draw is a preference nothing
    /// can act on.
    /// </remarks>
    public static string? Canonical(string? label)
    {
        var wanted = label?.Trim().ToUpperInvariant();

        return Labels.FirstOrDefault(
            one => string.Equals(one, wanted, StringComparison.Ordinal));
    }
}
