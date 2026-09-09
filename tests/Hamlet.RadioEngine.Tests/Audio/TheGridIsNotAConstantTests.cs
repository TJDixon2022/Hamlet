using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// What the slot arithmetic does when it is handed a 7.5-second grid.
/// </summary>
/// <remarks>
/// <para>**WORK INSTRUCTION 290, TASK 1 - THE STARTING POSITION, RECORDED BEFORE
/// ANYTHING MOVED.** These assertions are written against the code as unit 289 left
/// it, and the answers they assert are the wrong ones. Task 2 rewrites the file to
/// assert the right ones. A survey that is written from a diff is a list of its
/// author's own edits, so the wrong answers are pinned down first.</para>
/// <para>**THE BREAKAGE THIS CATCHES.** A 7.5-second grid quietly cut on
/// seven-second boundaries. It produces slots of very nearly the right count, with
/// every edge in the wrong place, and nothing anywhere raises an error - the cutter
/// hands over audio, the sidecar writes a boundary list, and the decoder simply
/// finds less than it should. That is HM-DEC-009 broken where nobody would check
/// it: a grid drawn at a boundary nobody measured.</para>
/// <para>**WHY THE ARITHMETIC IS RESTATED HERE RATHER THAN CALLED.** Today
/// <see cref="Ft8Slots.SlotStart"/> has no way to be handed a grid at all - the
/// slot length is a <c>const</c>. `SlotStartAsTodaysCodeWouldCompute` is
/// <c>Ft8Slots.cs:182-190</c> transcribed line for line with the constant made a
/// parameter, which is exactly what a session reaching for a one-line constant
/// change would produce.</para>
/// </remarks>
public sealed class TheGridIsNotAConstantTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheGridIsNotAConstantTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>FT4's slot, named here only so the test can hand it in.</summary>
    /// <remarks>
    /// Read from <see cref="Ft8Sharp.Ft4Timing.SlotSeconds"/> rather than typed,
    /// which is the rule for every FT4 number outside <c>Ft8Sharp</c>.
    /// </remarks>
    private static double Ft4Slot => Ft8Sharp.Ft4Timing.SlotSeconds;

    /// <summary><c>Ft8Slots.cs:182-190</c>, with the constant made a parameter.</summary>
    private static DateTime SlotStartAsTodaysCodeWouldCompute(
        DateTime trueUtc, double slotSeconds)
    {
        var second = (trueUtc.Second / (int)slotSeconds) * (int)slotSeconds;

        return new DateTime(
            trueUtc.Year, trueUtc.Month, trueUtc.Day,
            trueUtc.Hour, trueUtc.Minute, second,
            DateTimeKind.Utc);
    }

    private static DateTime At(double second)
        => new DateTime(2026, 9, 9, 14, 22, 0, DateTimeKind.Utc)
            .AddSeconds(second);

    /// <remarks>
    /// **THE CAST IS THE WHOLE FAULT.** `(int)7.5` is `7`, so the grid that comes
    /// out is a seven-second one. It agrees with the real 7.5-second grid at `:00`
    /// and at nowhere else in the minute.
    /// </remarks>
    [Fact]
    public void TodaysSlotStartCutsSevenSecondSlotsWhenAskedForSevenAndAHalf()
    {
        Assert.Equal(7.5, Ft4Slot, 6);

        (double Moment, double WrongSecond, double RightSecond)[] cases =
        [
            (0.0, 0, 0),
            (7.0, 7, 0),
            (8.0, 7, 7.5),
            (14.9, 14, 7.5),
        ];

        _output.WriteLine("  moment   today returns   the 7.5 s grid wants");

        foreach (var (moment, wrong, right) in cases)
        {
            var got = SlotStartAsTodaysCodeWouldCompute(At(moment), Ft4Slot);

            _output.WriteLine(
                $"  :{moment,-6:0.0}  :{(got - At(0)).TotalSeconds,-13:0.0}  :{right:0.0}");

            Assert.Equal(At(wrong), got);
        }

        // Right at :00 by coincidence, wrong at the other three.
        Assert.Equal(At(0), SlotStartAsTodaysCodeWouldCompute(At(0), Ft4Slot));
        Assert.NotEqual(At(0), SlotStartAsTodaysCodeWouldCompute(At(7.0), Ft4Slot));
        Assert.NotEqual(At(7.5), SlotStartAsTodaysCodeWouldCompute(At(8.0), Ft4Slot));
        Assert.NotEqual(At(7.5), SlotStartAsTodaysCodeWouldCompute(At(14.9), Ft4Slot));
    }

    /// <remarks>
    /// **THE RETURN TYPE CANNOT HOLD THE ANSWER EVEN IF THE ARITHMETIC WERE
    /// FIXED.** The <see cref="DateTime"/> constructor at <c>Ft8Slots.cs:186-189</c>
    /// takes whole seconds and has no field for the half, so every boundary it can
    /// produce falls on a whole second. This is why task 2 rewrites the arithmetic
    /// rather than changing a constant.
    /// </remarks>
    [Fact]
    public void TodaysSlotStartCannotExpressAHalfSecondBoundaryAtAll()
    {
        foreach (var moment in new[] { 0.0, 3.2, 7.0, 7.5, 8.0, 14.9, 22.5, 37.5 })
        {
            var got = SlotStartAsTodaysCodeWouldCompute(At(moment), Ft4Slot);

            Assert.Equal(0, got.Millisecond);
            Assert.Equal(0, got.Ticks % TimeSpan.TicksPerSecond);
        }

        _output.WriteLine(
            "  every boundary the whole-second constructor can build "
            + "lands on a whole second");
    }

    /// <remarks>
    /// **AND SO `IntoSlot` IS WRONG BY UP TO THE WHOLE OF THE ERROR.** It is
    /// `trueUtc - SlotStart(trueUtc)`, so a boundary in the wrong place moves the
    /// position inside the slot by the same amount, and at `:14.9` the reading is
    /// 0.9 s into a slot that in truth is 7.4 s old.
    /// </remarks>
    [Fact]
    public void TodaysIntoSlotFollowsTheWrongBoundary()
    {
        (double Moment, double WrongInto, double RightInto)[] cases =
        [
            (7.0, 0.0, 7.0),
            (8.0, 1.0, 0.5),
            (14.9, 0.9, 7.4),
        ];

        foreach (var (moment, wrongInto, rightInto) in cases)
        {
            var into = (At(moment)
                - SlotStartAsTodaysCodeWouldCompute(At(moment), Ft4Slot)).TotalSeconds;

            _output.WriteLine(
                $"  :{moment:0.0}  into slot {into:0.0} s, "
                + $"the 7.5 s grid wants {rightInto:0.0} s");

            Assert.Equal(wrongInto, into, 6);
            Assert.NotEqual(rightInto, into, 6);
        }
    }

    /// <remarks>
    /// **FT8'S OWN COLUMN, SO THE STARTING POSITION IS ON THE RECORD TOO.** These
    /// are the numbers task 2 may not move by a tick. They are asserted against the
    /// shipping <see cref="Ft8Slots.SlotStart"/> rather than the transcription.
    /// </remarks>
    [Fact]
    public void TodaysFifteenSecondGridIsRecordedSoItCanBeShownNotToHaveMoved()
    {
        (double Moment, double Boundary)[] cases =
        [
            (0.0, 0),
            (7.0, 0),
            (8.0, 0),
            (14.9, 0),
            (15.0, 15),
            (22.5, 15),
            (29.999, 15),
            (30.0, 30),
            (45.0, 45),
            (59.9, 45),
        ];

        foreach (var (moment, boundary) in cases)
        {
            _output.WriteLine(
                $"  :{moment,-7:0.000} -> :{boundary:0}");

            Assert.Equal(At(boundary), Ft8Slots.SlotStart(At(moment)));
            Assert.Equal(moment - boundary, Ft8Slots.IntoSlot(At(moment)), 3);
        }
    }
}
