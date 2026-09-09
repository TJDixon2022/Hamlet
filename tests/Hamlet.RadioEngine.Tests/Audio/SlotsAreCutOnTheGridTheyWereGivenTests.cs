using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// One stretch of audio cut on both grids, and a sidecar that says which.
/// </summary>
/// <remarks>
/// <para>**WORK INSTRUCTION 290, TASK 3 - STEP 2'S FIRST EXIT CRITERION.** The same
/// sixty seconds goes through <see cref="Ft8SlotCutter"/> twice, once on FT8's grid
/// and once on FT4's, and both the boundary times and the slot counts are asserted
/// rather than the counts alone. A count on its own would pass for a cut that put
/// eight slots in the right number and the wrong places.</para>
/// <para>**THE BREAKAGE THIS CATCHES.** An FT4 capture cut on fifteen-second
/// boundaries with a sidecar that does not say so. It is a file that looks like a
/// recording of FT4 and is a recording of every other FT4 slot, with nothing on its
/// face to reveal it - the boundary count is plausible, the audio is real, and the
/// decoder simply finds half of what was on the air. A year later there is no way
/// to tell it from a quiet band.</para>
/// <para>**NO CLOCK IS CORRECTED AND NONE IS GUESSED.** Both grids take their
/// boundaries from <see cref="Ft8Slots.TrueUtc"/> over a measured offset, and with
/// an unknown offset both refuse. That is HM-DEC-009 and a mode parameter does not
/// get to change it.</para>
/// </remarks>
public sealed class SlotsAreCutOnTheGridTheyWereGivenTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the boundaries are printed.</param>
    public SlotsAreCutOnTheGridTheyWereGivenTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 12_000;

    /// <summary>The minute the recording covers, start and end on the minute.</summary>
    private static readonly DateTime Started =
        new(2026, 9, 9, 14, 22, 0, DateTimeKind.Utc);

    private static readonly DateTime Ended = Started.AddSeconds(60);

    private static MonoAudio OneMinute()
        => new(Rate, new float[60 * Rate]);

    /// <remarks>
    /// **THE EDGES, NOT JUST THE COUNT.** FT8 puts four boundaries in this minute
    /// that a whole transmission still follows, on the quarter-minutes. FT4 puts
    /// eight, four of which fall on a half second and none of which the arithmetic
    /// this unit replaced could have produced.
    /// </remarks>
    [Fact]
    public void TheSameMinuteCutsFourFt8SlotsAndEightFt4Slots()
    {
        var clock = new ClockOffset(0, Ended);

        var ft8 = Ft8SlotCutter.Cut(OneMinute(), Ended, clock);
        var ft4 = Ft8SlotCutter.Cut(OneMinute(), Ended, clock, SlotGrid.Ft4);

        _output.WriteLine($"  FT8 ({SlotGrid.Ft8.Describe()}): {ft8.Slots.Count} slots");

        foreach (var slot in ft8.Slots)
        {
            _output.WriteLine(
                $"    {slot.StartUtc:HH:mm:ss.fff}  sample {slot.FirstSample,6}  "
                + $"{slot.Audio.Samples.Length} samples, padded {slot.PadSeconds:0.00} s");
        }

        _output.WriteLine($"  FT4 ({SlotGrid.Ft4.Describe()}): {ft4.Slots.Count} slots");

        foreach (var slot in ft4.Slots)
        {
            _output.WriteLine(
                $"    {slot.StartUtc:HH:mm:ss.fff}  sample {slot.FirstSample,6}  "
                + $"{slot.Audio.Samples.Length} samples, padded {slot.PadSeconds:0.00} s");
        }

        Assert.Equal("", ft8.Reason);
        Assert.Equal("", ft4.Reason);

        Assert.Equal(
            new[] { 0.0, 15.0, 30.0, 45.0 },
            ft8.Slots.Select(s => (s.StartUtc - Started).TotalSeconds));

        Assert.Equal(
            new[] { 0.0, 7.5, 15.0, 22.5, 30.0, 37.5, 45.0, 52.5 },
            ft4.Slots.Select(s => (s.StartUtc - Started).TotalSeconds));

        // A slot is the grid's own length in samples, on both.
        Assert.All(ft8.Slots, s => Assert.Equal(
            (int)Math.Round(SlotGrid.Ft8.SlotSeconds * Rate), s.Audio.Samples.Length));
        Assert.All(ft4.Slots, s => Assert.Equal(
            (int)Math.Round(SlotGrid.Ft4.SlotSeconds * Rate), s.Audio.Samples.Length));

        // Nothing was padded: every boundary here is followed by a whole slot.
        Assert.All(ft8.Slots, s => Assert.Equal(0, s.PadSamples));
        Assert.All(ft4.Slots, s => Assert.Equal(0, s.PadSamples));

        // **AND FT8 IS EXACTLY WHAT IT WAS.** The default overload and the one
        // handed FT8's grid explicitly are the same cut, slot for slot.
        var ft8Explicit = Ft8SlotCutter.Cut(OneMinute(), Ended, clock, SlotGrid.Ft8);

        Assert.Equal(
            ft8.Slots.Select(s => s.StartUtc),
            ft8Explicit.Slots.Select(s => s.StartUtc));
    }

    /// <remarks>
    /// **THE WATCH RUNS ON THE GRID IT WAS BUILT WITH.** It is the piece that turns
    /// a running band into slots without a press, so a watch on the wrong grid is
    /// the same file-shaped fault arriving live instead of in a capture.
    /// </remarks>
    [Fact]
    public void AWatchOnFt4sGridNoticesEightBoundariesWhereAnFt8WatchNoticesFour()
    {
        var ft8 = new Ft8SlotWatch();
        var ft4 = new Ft8SlotWatch { Grid = SlotGrid.Ft4 };

        Assert.Equal(SlotGrid.Ft8, ft8.Grid);
        Assert.Equal(SlotGrid.Ft4, ft4.Grid);

        // **THE TAP IS EMPTY AND THAT IS THE POINT.** The watch records which slot
        // it is in before it asks whether there is audio, so an empty tap exercises
        // the boundary arithmetic on its own without a ring to fill.
        var tap = new AudioTap();

        var clock = new ClockOffset(0, Started);

        // Arm both at the top of the minute, then step a quarter-second at a time
        // through it, counting the looks at which each watch sees its slot change.
        ft8.Look(tap, Started, clock);
        ft4.Look(tap, Started, clock);

        var ft8Boundaries = new List<double>();
        var ft4Boundaries = new List<double>();

        for (var quarter = 1; quarter <= 60 * 4; quarter++)
        {
            var at = Started.AddSeconds(quarter * 0.25);
            var wasFt8 = ft8.LastSeenSlotStart;
            var wasFt4 = ft4.LastSeenSlotStart;

            ft8.Look(tap, at, clock);
            ft4.Look(tap, at, clock);

            if (ft8.LastSeenSlotStart != wasFt8)
            {
                ft8Boundaries.Add((ft8.LastSeenSlotStart!.Value - Started).TotalSeconds);
            }

            if (ft4.LastSeenSlotStart != wasFt4)
            {
                ft4Boundaries.Add((ft4.LastSeenSlotStart!.Value - Started).TotalSeconds);
            }
        }

        _output.WriteLine("  FT8 boundaries seen: "
            + string.Join(", ", ft8Boundaries.Select(b => b.ToString("0.0"))));
        _output.WriteLine("  FT4 boundaries seen: "
            + string.Join(", ", ft4Boundaries.Select(b => b.ToString("0.0"))));

        Assert.Equal(new[] { 15.0, 30.0, 45.0, 60.0 }, ft8Boundaries);
        Assert.Equal(
            new[] { 7.5, 15.0, 22.5, 30.0, 37.5, 45.0, 52.5, 60.0 },
            ft4Boundaries);
    }

    /// <remarks>
    /// **AN UNMEASURED CLOCK REFUSES ON BOTH GRIDS.** The grid says which boundaries
    /// exist; the offset says where they fall in wall-clock time, and without it the
    /// answer is unknown rather than the PC's own guess. The refusal is the same
    /// sentence on both, because the operator meets this state through several doors.
    /// </remarks>
    [Fact]
    public void AnUnmeasuredClockCutsNothingOnEitherGrid()
    {
        foreach (var grid in new[] { SlotGrid.Ft8, SlotGrid.Ft4 })
        {
            var cut = Ft8SlotCutter.Cut(
                OneMinute(), Ended, ClockOffset.Unknown, grid);

            _output.WriteLine($"  {grid.Describe()}: '{cut.Reason}'");

            Assert.Empty(cut.Slots);
            Assert.Equal(Ft8SlotCutter.NoOffset, cut.Reason);

            // **AND THIS IS THE §0.0 BREACH TASK 1'S CENSUS NAMED, ON THE
            // RECORD.** `Ft8SlotCutter.NoOffset` says *fifteen-second* whichever
            // grid was asked for, so an FT4 operator with an unmeasured clock is
            // told about a boundary spacing his mode does not use. It is left
            // standing deliberately: it is one of the on-screen sentences, and a
            // screen that says two different things about the same grid is worse
            // than one that says the same wrong thing twice.
            Assert.Contains(
                "fifteen-second", cut.Reason, StringComparison.Ordinal);
        }
    }

    private static RigState AtTheRadio() => RigState.Empty.With(new[]
    {
        RigValue.Known(RigField.Mode, (int)CivMode.Usb, "USB", Ended, "CI-V 04"),
        RigValue.Known(RigField.DataMode, 1, "on", Ended, "CI-V 26 00"),
        RigValue.Known(
            RigField.Frequency, 14_080_000, "14.080000 MHz", Ended, "CI-V 03"),
    });

    private static string Sheet(SlotGrid? grid, ClockOffset clock)
        => DigitalCaptureSheet.Compose(
            Ended,
            60.0,
            Rate,
            AtTheRadio(),
            clock,
            Ended,
            "FT8 city",
            3000,
            null,
            null,
            clock.IsKnown ? "" : Ft8SlotCutter.NoOffset,
            default,
            null,
            grid);

    /// <remarks>
    /// **THIS IS THE LINE A READER A YEAR FROM NOW HAS TO BE ABLE TO TRUST.** It
    /// carries both lengths rather than a mode name, because the mode name is a
    /// label and the lengths are the measurement - and because the occupancy figure
    /// is still the owner's open question, so a sheet reading `FT4` would be
    /// asserting an answer to it.
    /// </remarks>
    [Fact]
    public void TheSidecarNamesTheGridItCountedOn()
    {
        var known = new ClockOffset(0, Ended);

        var ft8 = Sheet(null, known);
        var ft4 = Sheet(SlotGrid.Ft4, known);

        foreach (var line in ft8.Split('\n'))
        {
            if (line.StartsWith("slotGrid", StringComparison.Ordinal)
                || line.StartsWith("wholeSlots", StringComparison.Ordinal))
            {
                _output.WriteLine("  FT8  " + line);
            }
        }

        foreach (var line in ft4.Split('\n'))
        {
            if (line.StartsWith("slotGrid", StringComparison.Ordinal)
                || line.StartsWith("wholeSlots", StringComparison.Ordinal))
            {
                _output.WriteLine("  FT4  " + line);
            }
        }

        Assert.Contains(
            "slotGrid   5 boundaries, corrected to UTC, on 15.00 s slots, "
            + "12.64 s transmission",
            ft8,
            StringComparison.Ordinal);

        Assert.Contains(
            "slotGrid   9 boundaries, corrected to UTC, on 7.50 s slots, ",
            ft4,
            StringComparison.Ordinal);

        // The occupancy comes from the port, so it is asserted from the port
        // rather than typed - the 4.48-against-5.04 question is the owner's.
        Assert.Contains(
            $"on 7.50 s slots, {SlotGrid.Ft4.TransmissionSeconds:0.00} s transmission",
            ft4,
            StringComparison.Ordinal);

        // Four FT8 boundaries are followed by a whole 12.64 s transmission;
        // eight FT4 boundaries are followed by a whole one of its own.
        Assert.Contains("wholeSlots 4  ", ft8, StringComparison.Ordinal);
        Assert.Contains("wholeSlots 8  ", ft4, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **THE UNREAD CASE KEEPS SAYING UNKNOWN, AND NOW SAYS WHAT IT WOULD HAVE
    /// USED.** Which grid was asked for is a fact about the capture; where its
    /// boundaries fall is a measurement nobody took, and only the second one is
    /// withheld.
    /// </remarks>
    [Fact]
    public void AnUnmeasuredClockLeavesTheGridUnreadOnBothModes()
    {
        var ft8 = Sheet(null, ClockOffset.Unknown);
        var ft4 = Sheet(SlotGrid.Ft4, ClockOffset.Unknown);

        foreach (var sheet in new[] { ft8, ft4 })
        {
            Assert.Contains(
                "slotGrid   " + DigitalCaptureSheet.Unread,
                sheet,
                StringComparison.Ordinal);
            Assert.Contains(
                "wholeSlots " + DigitalCaptureSheet.Unread,
                sheet,
                StringComparison.Ordinal);
        }

        Assert.Contains("15.00 s slots, 12.64 s transmission", ft8, StringComparison.Ordinal);
        Assert.Contains("7.50 s slots,", ft4, StringComparison.Ordinal);

        _output.WriteLine("  FT8 unread: " + Line(ft8));
        _output.WriteLine("  FT4 unread: " + Line(ft4));

        static string Line(string sheet)
            => sheet.Split('\n').First(
                l => l.StartsWith("slotGrid", StringComparison.Ordinal));
    }
}
