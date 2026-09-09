using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// The slot arithmetic on both grids, and the evidence FT8's answers did not move.
/// </summary>
/// <remarks>
/// <para>**WORK INSTRUCTION 290, TASKS 1 AND 2.** Task 1 wrote this file against
/// the code as unit 289 left it and asserted the *wrong* answers - a 7.5-second
/// grid cut on seven-second boundaries - as the starting position. Task 2 rewrote
/// it to assert the right ones. The wrong answers are kept as prose in
/// `WhatTheOldWholeSecondArithmeticDid` rather than deleted, because the census is
/// only worth having if what it measured is still legible afterwards.</para>
/// <para>**THE BREAKAGE THIS CATCHES.** A 7.5-second grid quietly cut on
/// seven-second boundaries. It produces slots of very nearly the right count, with
/// every edge in the wrong place, and nothing anywhere raises an error - the cutter
/// hands over audio, the sidecar writes a boundary list, and the decoder simply
/// finds less than it should. That is HM-DEC-009 broken where nobody would check
/// it: a grid drawn at a boundary nobody measured.</para>
/// <para>**AND THE SECOND BREAKAGE: AN FT8 BOUNDARY MOVED BY A TICK WHILE MAKING
/// ROOM FOR FT4.** `Ft8sBoundariesAreTickIdenticalToTheWholeSecondArithmetic`
/// compares the shipping code against a transcription of the arithmetic it
/// replaced, over every sixteenth of a second across four minutes plus a spread of
/// awkward tick offsets. It would be invisible in the application until a decode
/// rate fell weeks later.</para>
/// </remarks>
public sealed class TheGridIsNotAConstantTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheGridIsNotAConstantTests(ITestOutputHelper output)
        => _output = output;

    /// <summary><c>Ft8Slots.cs:182-190</c> as it stood before unit 290 task 2.</summary>
    /// <remarks>
    /// **KEPT AS THE CONTROL, NOT AS A SECOND IMPLEMENTATION.** Nothing in the
    /// application calls it. It exists so the claim *FT8's numbers are unchanged*
    /// is a measurement against the code that produced them rather than an
    /// assertion about a diff.
    /// </remarks>
    private static DateTime WhatTheOldWholeSecondArithmeticDid(
        DateTime trueUtc, double slotSeconds)
    {
        var second = (trueUtc.Second / (int)slotSeconds) * (int)slotSeconds;

        return new DateTime(
            trueUtc.Year, trueUtc.Month, trueUtc.Day,
            trueUtc.Hour, trueUtc.Minute, second,
            DateTimeKind.Utc);
    }

    private static readonly DateTime Minute =
        new(2026, 9, 9, 14, 22, 0, DateTimeKind.Utc);

    private static DateTime At(double second) => Minute.AddSeconds(second);

    /// <remarks>
    /// **FT4'S GRID IS BUILT FROM THE PORT AND TYPED NOWHERE IN THE ENGINE.** The
    /// slot is <c>Ft8Sharp.Ft4Timing.SlotSeconds</c> and the occupancy is
    /// <c>Ft4Timing.OccupancySeconds</c>, so the open 4.48-against-5.04 question
    /// still costs one edit in one file to settle, and this test asserts the slot
    /// rather than the occupancy because the slot is 7.5 s on either answer.
    /// </remarks>
    [Fact]
    public void Ft4sGridComesFromThePortAndNotFromANumberTypedHere()
    {
        Assert.Equal(7.5, SlotGrid.Ft4.SlotSeconds, 6);
        Assert.Equal(
            Ft8Sharp.Ft4Timing.SlotSeconds, SlotGrid.Ft4.SlotSeconds, 6);
        Assert.Equal(
            Ft8Sharp.Ft4Timing.OccupancySeconds,
            SlotGrid.Ft4.TransmissionSeconds,
            6);

        // Eight slots to the minute, and eight is even - which is the whole of
        // why unit 277's parity rule survives the change of grid.
        Assert.Equal(8, SlotGrid.Ft4.SlotsPerMinute);
        Assert.Equal(4, SlotGrid.Ft8.SlotsPerMinute);

        _output.WriteLine($"  FT8: {SlotGrid.Ft8.Describe()}");
        _output.WriteLine($"  FT4: {SlotGrid.Ft4.Describe()}");
    }

    /// <remarks>
    /// **THE FOUR MOMENTS THE CENSUS ASKED ABOUT.** The old arithmetic answered
    /// :00, :07, :07 and :14; three of those four are wrong and the fourth is right
    /// by coincidence, because <c>(int)7.5</c> is <c>7</c>.
    /// </remarks>
    [Fact]
    public void ASevenAndAHalfSecondGridIsCutOnSevenAndAHalfSecondBoundaries()
    {
        (double Moment, double Boundary, double OldWrongAnswer)[] cases =
        [
            (0.0, 0.0, 0.0),
            (7.0, 0.0, 7.0),
            (7.5, 7.5, 7.0),
            (8.0, 7.5, 7.0),
            (14.9, 7.5, 14.0),
            (15.0, 15.0, 14.0),
            (22.5, 22.5, 21.0),
            (29.999, 22.5, 28.0),
            (37.5, 37.5, 35.0),
            (52.5, 52.5, 49.0),
            (59.999, 52.5, 56.0),
        ];

        _output.WriteLine("  moment    7.5 s grid   what whole seconds gave");

        foreach (var (moment, boundary, old) in cases)
        {
            var got = SlotGrid.Ft4.SlotStart(At(moment));

            _output.WriteLine(
                $"  :{moment,-8:0.000} :{boundary,-11:0.0} :{old:0.0}");

            Assert.Equal(At(boundary), got);
            Assert.Equal(DateTimeKind.Utc, got.Kind);
            Assert.Equal(
                moment - boundary, SlotGrid.Ft4.IntoSlot(At(moment)), 6);

            // The starting position, still legible: of these eleven moments the
            // old arithmetic agreed at exactly one, `:00`, and by coincidence -
            // it is where a seven-second grid and a 7.5-second grid touch. By
            // `:52.5` it was three and a half seconds out.
            var wasWrong = WhatTheOldWholeSecondArithmeticDid(At(moment), 7.5);
            Assert.Equal(At(old), wasWrong);
        }
    }

    /// <remarks>
    /// **THE HALF-SECOND BOUNDARY IS NOW EXPRESSIBLE, WHICH IT WAS NOT.** Four of
    /// FT4's eight boundaries in a minute fall on a half second, and the whole-second
    /// <see cref="DateTime"/> constructor had no field for it. This asserts the
    /// boundary carries the exact half tick for tick rather than something near it.
    /// </remarks>
    [Fact]
    public void AHalfSecondBoundaryIsCarriedExactlyRatherThanRoundedAway()
    {
        var halves = 0;

        for (var slot = 0; slot < SlotGrid.Ft4.SlotsPerMinute; slot++)
        {
            var boundary = SlotGrid.Ft4.SlotStart(At((slot * 7.5) + 1.0));

            Assert.Equal(At(slot * 7.5), boundary);

            if (boundary.Ticks % TimeSpan.TicksPerSecond != 0)
            {
                halves++;
                Assert.Equal(
                    TimeSpan.TicksPerSecond / 2,
                    boundary.Ticks % TimeSpan.TicksPerSecond);
            }
        }

        _output.WriteLine(
            $"  {halves} of {SlotGrid.Ft4.SlotsPerMinute} FT4 boundaries in a "
            + "minute fall on a half second, and none of them could be built "
            + "by the whole-second constructor");

        Assert.Equal(4, halves);
    }

    /// <remarks>
    /// **THIS IS THE ONE THIS UNIT MAY NOT FAIL.** Every FT8 number the application
    /// produces today must be produced identically afterwards, and a tick is the
    /// unit that matters: a boundary a tick early is a sample count a tick short,
    /// which is a decode rate falling weeks later with nothing on its face to say
    /// why.
    /// </remarks>
    [Fact]
    public void Ft8sBoundariesAreTickIdenticalToTheWholeSecondArithmetic()
    {
        var checkedMoments = 0;

        // Every sixteenth of a second across four minutes - which crosses three
        // minute boundaries and every one of FT8's four slots in each.
        for (var sixteenth = 0; sixteenth < 4 * 60 * 16; sixteenth++)
        {
            var moment = Minute.AddTicks(
                sixteenth * (TimeSpan.TicksPerSecond / 16));

            Assert.Equal(
                WhatTheOldWholeSecondArithmeticDid(moment, 15).Ticks,
                Ft8Slots.SlotStart(moment).Ticks);

            checkedMoments++;
        }

        // And a spread of awkward tick offsets, including one tick either side of
        // a boundary, which is where a floor written two ways parts company.
        long[] offsets =
        [
            0, 1, -1, 9_999_999, 10_000_000, 149_999_999, 150_000_000,
            150_000_001, 599_999_999, 600_000_000, 12_345_678, 1,
        ];

        foreach (var offset in offsets)
        {
            foreach (var slot in new[] { 0, 15, 30, 45 })
            {
                var moment = At(slot).AddTicks(offset);

                Assert.Equal(
                    WhatTheOldWholeSecondArithmeticDid(moment, 15).Ticks,
                    Ft8Slots.SlotStart(moment).Ticks);
                Assert.Equal(
                    Ft8Slots.SlotStart(moment), SlotGrid.Ft8.SlotStart(moment));

                checkedMoments++;
            }
        }

        _output.WriteLine(
            $"  {checkedMoments} moments, every one tick-identical to the "
            + "arithmetic this unit replaced");

        Assert.Equal((4 * 60 * 16) + (offsets.Length * 4), checkedMoments);
    }

    /// <remarks>
    /// **THE BOUNDARY LIST STEPS IN TICKS, SO IT CANNOT DRIFT OFF ITS OWN GRID.**
    /// A half-second step added in floating point a thousand times is a rule the
    /// waterfall draws in the wrong place, and it would be invisible on FT8 because
    /// fifteen is exact in binary and silent on FT4 because 7.5 is too - until the
    /// arithmetic stopped being exact somewhere upstream of it.
    /// </remarks>
    [Fact]
    public void BoundariesAcrossTenMinutesLandOnTheGridToTheTick()
    {
        var from = Minute.AddSeconds(-0.5);
        var to = Minute.AddMinutes(10);

        foreach (var grid in new[] { SlotGrid.Ft8, SlotGrid.Ft4 })
        {
            var boundaries = grid.BoundariesBetween(from, to);
            var slotTicks = (long)Math.Round(
                grid.SlotSeconds * TimeSpan.TicksPerSecond);

            // Ten minutes of boundaries, plus the one closing the stretch: the
            // window opens half a second before a minute, so the first boundary
            // inside it is that minute and the last is the tenth one after it.
            Assert.Equal((10 * grid.SlotsPerMinute) + 1, boundaries.Count);

            for (var i = 0; i < boundaries.Count; i++)
            {
                Assert.Equal(Minute.AddTicks(i * slotTicks), boundaries[i]);
                Assert.Equal(DateTimeKind.Utc, boundaries[i].Kind);
                Assert.Equal(boundaries[i], grid.SlotStart(boundaries[i]));
            }

            _output.WriteLine(
                $"  {grid.Describe()}: {boundaries.Count} boundaries in ten "
                + $"minutes, first {boundaries[0]:HH:mm:ss.fff}, last "
                + $"{boundaries[^1]:HH:mm:ss.fff}");
        }
    }
}
