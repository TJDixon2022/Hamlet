using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A decoder must not be able to score well by going quiet: every keyed recording
/// has a floor on how many named characters it reads at all (work instruction 412,
/// task 2; PHASE_PLAN.md 2.2, R59).
/// </summary>
/// <remarks>
/// <para>**THE EDIT COUNT REWARDS SILENCE.** On 17:37 fifteen of the twenty-nine
/// edits are spaces the decoder added and ten more are letters it added, so a
/// change that stops printing most of what it hears takes those edits away and
/// reads as a repair. This floor is what makes it read as the regression it is:
/// **a change that drops a keyed recording below its named floor is a regression
/// whatever its edit count** (2.2).</para>
/// <para>**THE FLOOR COUNTS NAMED CHARACTERS OVER THE WHOLE RECORDING**, not over
/// the scored region: the region is chosen by the alignment and shrinks with the
/// decode, and *read at all* is about everything the decoder settled. A named
/// character is neither a word gap nor a placeholder (R57, HM-DEC-168), so
/// placeholders are free here as they are in the capture floors. The unsure count
/// over the scored region is printed beside it, 2.1's guard, and asserted on
/// nothing.</para>
/// <para>**WHY IT IS NOT THE CAPTURE FLOOR AGAIN.** 17:37 is in no capture row, and
/// the count floors of the three adjudicated recordings in the capture table
/// retired in favor of their anchors (Tim, 2026-08-25); an anchor is a substring
/// and a decoder that goes quiet everywhere but there keeps it. Here every keyed
/// recording has a count again, on the same footing.</para>
/// <para>Set from the baseline at `02ce4602` by work instruction 412, fed as the
/// baseline is fed (<see cref="TheBaselineIsScoredTests.Read"/>). **FLOORS ONLY
/// EVER RISE** (PHASE_PLAN.md §6).</para>
/// </remarks>
public sealed class TheNumberCannotBeGamedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public TheNumberCannotBeGamedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Every keyed recording, with the named characters it read when the floor was set.</summary>
    public static TheoryData<string, int> NamedFloors { get; } = new()
    {
        // The baseline's four (PHASE_PLAN.md 0.2).
        { TheSeventeenThirtySevenCaptureTests.Name, 46 },
        { "cw-2026-08-17-013347", 57 },
        { "cw-2026-08-17-134712", 21 },
        { "unadjudicated/cw-2026-08-18-003758", 44 },

        // The other adjudicated recordings, scored outside the baseline total.
        { "unadjudicated/cw-2026-08-24-012403", 21 },
        { "cw-2026-08-18-004507", 49 },
        { "unadjudicated/cw-2026-08-22-031838", 43 },
        { "unadjudicated/cw-2026-08-22-031905", 36 },
        { "unadjudicated/cw-2026-08-22-031948", 31 },
        { "unadjudicated/cw-2026-08-22-032012", 43 },
        { "unadjudicated/cw-2026-08-22-032050", 44 },
        { "unadjudicated/cw-2026-08-22-032113", 47 },
        { "unadjudicated/cw-2026-08-22-032129", 65 },
    };

    /// <summary>A keyed recording scored as the baseline scores it.</summary>
    /// <param name="name">The recording.</param>
    /// <returns>The score.</returns>
    internal static CwScore Score(string name)
        => name == TheSeventeenThirtySevenCaptureTests.Name
            ? TheBaselineIsScoredTests.SeventeenThirtySeven()
            : TheBaselineIsScoredTests.Adjudicated(
                TheAdjudicatedReadingsKeepReadingTests.All.Single(r => r.Name == name));

    /// <remarks>
    /// Proves 2.2: each keyed recording still reads at least as many named
    /// characters as it did when its floor was set, whatever its edits did.
    /// </remarks>
    /// <param name="name">The recording.</param>
    /// <param name="floor">The named characters it read when the floor was set.</param>
    [Theory]
    [MemberData(nameof(NamedFloors))]
    public void EachKeyedRecordingIsReadAtAll(string name, int floor)
    {
        // Counted as the capture floors count: one per settled character, so a
        // prosign is one character here where the region's text spells it out.
        var named = TheSeventeenThirtySevenCaptureTests.Settle(name)
            .Count(c => !c.IsWordGap && !c.IsUnreadable);
        var score = Score(name);

        _output.WriteLine(
            $"named | {name} | {named} against a floor of {floor} | {score} | region `{score.Region}`");

        Assert.True(
            named >= floor,
            $"{name} fell from {floor} named characters to {named}, at {score}");
    }
}
