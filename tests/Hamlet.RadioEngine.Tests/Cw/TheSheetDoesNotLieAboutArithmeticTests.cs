using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A counter window that spans a reset says it cannot be measured, rather than
/// printing a negative.
/// </summary>
/// <remarks>
/// <para>**IT REACHED THE OPERATOR** (work instruction 055, task 1).
/// `cw-2026-08-31-003229`'s sidecar reads `inThis -250 characters emitted, -96
/// unsure, -466 elements seen, -466 resolved`. That is the sheet he diagnoses
/// everything with, and it was lying about arithmetic.</para>
/// <para>**THE CAUSE IS A RESET INSIDE THE WINDOW.** `CwDecoder.Retuned` zeroes
/// the counters when the operator moves, because a count earned on another
/// frequency does not belong on this sheet. The trail keeps its samples from
/// before that, so `Over` subtracted a large earlier reading from a small later
/// one.</para>
/// <para>**SAYING SO IS THE ANSWER RATHER THAN CLAMPING TO ZERO** (§0.0). Nought
/// characters in this recording and a window nobody can measure are different
/// facts, and the second is the true one.</para>
/// </remarks>
public sealed class TheSheetDoesNotLieAboutArithmeticTests
{
    private readonly ITestOutputHelper _output;

    public TheSheetDoesNotLieAboutArithmeticTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Counters that only rise give an ordinary delta.</summary>
    /// <remarks>
    /// **THE CONTROL.** Without it a change that refused every window would pass
    /// the test below and break the field entirely.
    /// </remarks>
    [Fact]
    public void AnOrdinaryWindowStillMeasures()
    {
        var trail = new CwCounterTrail(80_000);

        trail.Note(new CwCounterSample(0, 10, 10, 4, 1));
        trail.Note(new CwCounterSample(40_000, 30, 30, 12, 3));
        trail.Note(new CwCounterSample(80_000, 60, 60, 25, 6));

        var over = trail.Over(80_000, 80_000);

        Assert.NotNull(over);

        _output.WriteLine(
            $"{over!.Value.CharactersEmitted} characters, "
            + $"{over.Value.ElementsSeen} elements over the window");

        Assert.Equal(21, over.Value.CharactersEmitted);
        Assert.Equal(50, over.Value.ElementsSeen);
    }

    /// <summary>A window spanning a reset refuses rather than going negative.</summary>
    /// <remarks>
    /// The numbers are the operator's own: a large reading, then a retune, then a
    /// small one. Before this fix the window read −250 characters.
    /// </remarks>
    [Fact]
    public void AWindowSpanningAResetIsNotDerived()
    {
        var trail = new CwCounterTrail(80_000);

        // Before the retune.
        trail.Note(new CwCounterSample(0, 500, 500, 280, 100));

        // After it: the decoder has zeroed and started again.
        trail.Note(new CwCounterSample(40_000, 20, 20, 15, 2));
        trail.Note(new CwCounterSample(80_000, 34, 34, 30, 4));

        var over = trail.Over(80_000, 80_000);

        _output.WriteLine(over is null
            ? "not derived, which is the honest answer"
            : $"{over.Value.CharactersEmitted} characters — this should not print");

        Assert.Null(over);
    }

    /// <summary>Each counter is checked, not just the one that happened to fail.</summary>
    /// <remarks>
    /// **THE SHEET PRINTS FOUR NUMBERS AND ALL FOUR WENT NEGATIVE.** A guard on
    /// one of them would have left the other three able to lie.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void AnyCounterGoingBackwardsRefusesTheWindow(int which)
    {
        var trail = new CwCounterTrail(80_000);

        trail.Note(new CwCounterSample(0, 100, 100, 100, 100));

        // Everything rises except the one under test.
        trail.Note(new CwCounterSample(
            80_000,
            which == 0 ? 90 : 200,
            which == 1 ? 90 : 200,
            which == 2 ? 90 : 200,
            which == 3 ? 90 : 200));

        var over = trail.Over(80_000, 80_000);

        _output.WriteLine($"counter {which} went backwards: over is {(over is null ? "null" : "a value")}");

        Assert.Null(over);
    }
}
