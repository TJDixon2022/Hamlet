using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// The beat on FT4's grid: what it counts, where the parity comes from, and the
/// fact that reaching zero does nothing at all.
/// </summary>
/// <remarks>
/// <para>**WORK INSTRUCTION 290, TASK 4 - STEP 2'S SECOND EXIT CRITERION.**</para>
/// <para>**THE BREAKAGE THIS CATCHES.** A turn that says *yours, 8 s* on a
/// 7.5-second slot. It is a countdown asserting a slot length the application is not
/// cutting on, on the one screen the operator times his transmission by, and it
/// would look entirely ordinary - eight, seven, six - right up to the moment he
/// started half a second late every round and could not see why.</para>
/// <para>**AND THE SECOND ONE: A COUNTDOWN THAT DOES SOMETHING WHEN IT EXPIRES.**
/// FT4's slot is half FT8's, so the argument for letting the ring arm the next
/// transmission is twice as strong and the ruling against it is unchanged. One
/// operator action, one transmission (§0.2). The countdown is run to its last tenth
/// and across the boundary here, and the assertion is that nothing on the type can
/// reach a transmitter to begin with.</para>
/// </remarks>
public sealed class TheRingCountsDownSevenAndAHalfTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the countdown is printed.</param>
    public TheRingCountsDownSevenAndAHalfTests(ITestOutputHelper output)
        => _output = output;

    private static readonly DateTime Minute =
        new(2026, 9, 9, 14, 22, 0, DateTimeKind.Utc);

    private static ClockOffset Measured() => new(0, Minute);

    /// <summary>A moment, in tenths of a second past the minute.</summary>
    private static DateTime Tenth(int tenths)
        => Minute.AddTicks(tenths * (TimeSpan.TicksPerSecond / 10));

    /// <summary>The other station's slot boundary, in seconds past the minute.</summary>
    private static DateTime Slot(double second) => Minute.AddSeconds(second);

    /// <remarks>
    /// **THE COUNT NEVER READS 8 AND NEVER READS 0.** Eight would claim a slot the
    /// cutter is not cutting; zero would say the slot is over while a station is
    /// still transmitting in it. It opens at 7.5 and closes at 0.1.
    /// </remarks>
    [Fact]
    public void ASevenAndAHalfSecondSlotCountsDownFromSevenPointFiveAndNeverFromEight()
    {
        var seen = new List<string>();

        for (var tenths = 0; tenths < 75; tenths++)
        {
            var turn = Ft8Turn.Read(
                Tenth(tenths), Measured(), Slot(0), grid: SlotGrid.Ft4);

            seen.Add(turn.CountText);
        }

        _output.WriteLine("  " + string.Join(" ", seen));

        Assert.Equal("7.5", seen[0]);
        Assert.Equal("0.1", seen[^1]);

        Assert.DoesNotContain("8", seen);
        Assert.DoesNotContain("8.0", seen);
        Assert.DoesNotContain("0.0", seen);
        Assert.DoesNotContain("0", seen);

        // It never goes up inside a slot, and it never exceeds the slot itself.
        for (var i = 0; i < seen.Count; i++)
        {
            var value = double.Parse(seen[i], System.Globalization.CultureInfo.InvariantCulture);

            Assert.InRange(value, 0.1, SlotGrid.Ft4.SlotSeconds);

            if (i > 0)
            {
                var before = double.Parse(
                    seen[i - 1], System.Globalization.CultureInfo.InvariantCulture);

                Assert.True(value <= before, $"the count went up at tenth {i}");
            }
        }

        // And it rolls over rather than expiring: the tenth after the last one is
        // the top of the next slot, not a zero and not an end.
        var over = Ft8Turn.Read(Tenth(75), Measured(), Slot(0), grid: SlotGrid.Ft4);

        Assert.Equal("7.5", over.CountText);
        _output.WriteLine($"  at :07.5 the next slot opens at {over.CountText}");
    }

    /// <remarks>
    /// **FT8 IS UNCHANGED, AND THIS IS THE CONTROL COLUMN BESIDE IT.** Whole seconds,
    /// 15 down to 1, exactly the digits the ring has always shown.
    /// </remarks>
    [Fact]
    public void FifteenSecondSlotsStillCountInWholeSecondsFromFifteen()
    {
        var seen = new List<string>();

        for (var tenths = 0; tenths < 150; tenths += 5)
        {
            seen.Add(Ft8Turn.Read(Tenth(tenths), Measured(), Slot(0)).CountText);
        }

        _output.WriteLine("  " + string.Join(" ", seen));

        Assert.Equal("15", seen[0]);
        Assert.Equal("1", seen[^1]);
        Assert.DoesNotContain(".", string.Join("", seen));

        // The integer and the text agree on a whole-second grid, which is what
        // makes the change invisible here.
        for (var tenths = 0; tenths < 150; tenths++)
        {
            var turn = Ft8Turn.Read(Tenth(tenths), Measured(), Slot(0));

            Assert.Equal(
                turn.SecondsLeft!.Value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                turn.CountText);
        }
    }

    /// <remarks>
    /// **UNIT 277'S RULE, UNCHANGED IN SHAPE.** Whose turn it is comes from what the
    /// other station sent, on the grid it was heard on, and nothing guesses it before
    /// a station has spoken. Eight slots to the minute is even, so the alternation
    /// does not swap over at a minute or an hour - which is the same reason it held
    /// on FT8 with four.
    /// </remarks>
    [Fact]
    public void WhoseTurnItIsStillComesFromWhatTheOtherStationSent()
    {
        Assert.Equal(8, SlotGrid.Ft4.SlotsPerMinute);
        Assert.Equal(0, SlotGrid.Ft4.SlotsPerMinute % 2);

        double[] boundaries = [0, 7.5, 15, 22.5, 30, 37.5, 45, 52.5];
        var parities = boundaries
            .Select(b => Ft8Turn.ParityOf(Slot(b), SlotGrid.Ft4))
            .ToArray();

        _output.WriteLine("  parity across the minute: "
            + string.Join(" ", parities));

        Assert.Equal(new[] { 0, 1, 0, 1, 0, 1, 0, 1 }, parities);

        // The same eight an hour later, and a day later, so nothing shifts at a
        // minute, an hour or a date.
        foreach (var later in new[]
        {
            Minute.AddHours(1), Minute.AddDays(1), Minute.AddMinutes(1),
        })
        {
            Assert.Equal(
                parities,
                boundaries.Select(
                    b => Ft8Turn.ParityOf(later.AddSeconds(b), SlotGrid.Ft4)));
        }

        // A station heard on the :07.5 half makes the :07.5 slots theirs and the
        // :00 slots his, and it derives that from their transmission rather than
        // from a default.
        var theirs = Ft8Turn.Read(
            Tenth(76), Measured(), Slot(7.5), grid: SlotGrid.Ft4);
        var mine = Ft8Turn.Read(
            Tenth(4), Measured(), Slot(7.5), grid: SlotGrid.Ft4);

        _output.WriteLine($"  heard on :07.5 -> at :07.6 {theirs.State}, "
            + $"at :00.4 {mine.State}");

        Assert.Equal(Ft8TurnState.Theirs, theirs.State);
        Assert.Equal(Ft8TurnState.Mine, mine.State);

        // **AND THE HALF IT NAMES IS 7.5, NOT 7.** The rounded field cannot hold
        // it, which is why the exact one exists.
        Assert.Equal(7.5, theirs.TheirSlotSecondExact);
        Assert.Equal(7, theirs.TheirSlotSecond);

        // Nothing heard is still a real state and still refuses to pick a side.
        var nobody = Ft8Turn.Read(Tenth(4), Measured(), null, grid: SlotGrid.Ft4);

        Assert.Equal(Ft8TurnState.NoStationYet, nobody.State);
        Assert.Null(nobody.TheirSlotSecond);
        Assert.Null(nobody.TheirSlotSecondExact);

        // ...and so is an unmeasured clock, on FT4 exactly as on FT8.
        var noClock = Ft8Turn.Read(
            Tenth(4), ClockOffset.Unknown, Slot(0), grid: SlotGrid.Ft4);

        Assert.Equal(Ft8TurnState.NoClock, noClock.State);
        Assert.Equal("", noClock.CountText);
    }

    /// <remarks>
    /// **THE TRANSMITTING COUNTDOWN'S DENOMINATOR COMES FROM THE PORT.** 5.04 is
    /// typed nowhere in <c>Hamlet.RadioEngine</c>; it is
    /// <c>Ft8Sharp.Ft4Timing.OccupancySeconds</c> arriving through
    /// <c>SlotGrid.Ft4</c>, so the owner's open 4.48-against-5.04 question costs one
    /// edit in one file and this test moves with it rather than pinning it.
    /// </remarks>
    [Fact]
    public void TheTransmittingCountdownMeasuresTheOccupancyThePortDefines()
    {
        var sending = Slot(0);

        var atStart = Ft8Turn.Read(
            Minute, Measured(), Slot(0), sending, grid: SlotGrid.Ft4);

        Assert.Equal(Ft8TurnState.Transmitting, atStart.State);
        Assert.Equal(
            SlotGrid.Ft4.TransmissionSeconds, atStart.SecondsLeftExact!.Value, 5);
        Assert.Equal(
            Ft8Sharp.Ft4Timing.OccupancySeconds, atStart.SecondsLeftExact!.Value, 5);

        _output.WriteLine(
            $"  at the top of the transmission: {atStart.CountText} s left of "
            + $"{SlotGrid.Ft4.TransmissionSeconds:0.00} s");

        // It runs out with the occupancy rather than with the slot, and what
        // follows is an ordinary reading of whose slot it is - not an end.
        var afterwards = Ft8Turn.Read(
            Minute.AddSeconds(SlotGrid.Ft4.TransmissionSeconds + 0.1),
            Measured(),
            Slot(0),
            sending,
            grid: SlotGrid.Ft4);

        Assert.NotEqual(Ft8TurnState.Transmitting, afterwards.State);
        Assert.Equal(Ft8TurnState.Theirs, afterwards.State);
    }

    /// <remarks>
    /// <para>**THE RING DRAINS AND DOES NOTHING ELSE** (§0.2, and the ruling
    /// <c>PHASE_PLAN.md</c> says the arbiter may not reason past). Running the
    /// countdown to its last tenth and across the boundary is not enough on its own -
    /// a side effect would not show up in a return value - so this also asserts that
    /// there is nothing on the type that could have one.</para>
    /// <para>**IT IS A PURE FUNCTION OF ITS ARGUMENTS**, so the same moment read
    /// twice gives the same answer, and nothing accumulates between reads.</para>
    /// </remarks>
    [Fact]
    public void RunningTheCountdownToZeroOnFt4sGridTransmitsNothing()
    {
        // Every tenth of a second across a whole FT4 slot and out the far side.
        for (var tenths = 0; tenths <= 80; tenths++)
        {
            var once = Ft8Turn.Read(
                Tenth(tenths), Measured(), Slot(0), grid: SlotGrid.Ft4);
            var twice = Ft8Turn.Read(
                Tenth(tenths), Measured(), Slot(0), grid: SlotGrid.Ft4);

            Assert.Equal(once, twice);
        }

        // **AND NOTHING ON THE TYPE COULD KEY ANYTHING.** Every public member is a
        // reading or a sentence. A method that armed, queued or sent would have to
        // appear here first.
        var members = typeof(Ft8Turn)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Select(m => m.Name)
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        _output.WriteLine("  public surface: " + string.Join(", ", members));

        string[] forbidden =
        [
            "Send", "Arm", "Key", "Queue", "Transmit", "Ptt", "Start", "Fire",
        ];

        foreach (var name in members)
        {
            foreach (var word in forbidden)
            {
                Assert.False(
                    name.Contains(word, StringComparison.OrdinalIgnoreCase),
                    $"Ft8Turn exposes '{name}', which reads like an action");
            }
        }

        // And the file itself reaches no transmit path. Ft8Turn.cs is one file and
        // its whole job is a sentence and a number.
        var source = File.ReadAllText(SourceOf("Ft8Turn.cs"));

        foreach (var word in new[] { "ICivLink", "Ptt", "Ft8ArmedSend", "SendAsync" })
        {
            Assert.DoesNotContain(word, source, StringComparison.Ordinal);
        }
    }

    /// <summary>Where a RadioEngine audio source file lives, from the test binary.</summary>
    private static string SourceOf(string fileName)
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null && !File.Exists(Path.Combine(here.FullName, "Hamlet.sln")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return Path.Combine(
            here!.FullName, "src", "Hamlet.RadioEngine", "Audio", fileName);
    }
}
