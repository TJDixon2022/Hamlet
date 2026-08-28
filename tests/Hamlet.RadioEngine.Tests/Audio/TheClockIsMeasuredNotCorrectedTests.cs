using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// The clock offset and the FT8 slot grid, both testable without a clock.
/// </summary>
/// <remarks>
/// <para>**TASK 4 OF WORK INSTRUCTION 038.** Tim's ruling of 2026-08-28: UTC is
/// measured and displayed, never corrected. One query, the offset shown, and
/// trimming refuses while the offset is unknown.</para>
/// <para>**EVERY THRESHOLD IS A PURE FUNCTION OVER AN OFFSET AND A MOMENT**, so
/// none of this needs a network or a wall clock to check.</para>
/// </remarks>
public sealed class TheClockIsMeasuredNotCorrectedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheClockIsMeasuredNotCorrectedTests(ITestOutputHelper output)
        => _output = output;

    private static readonly DateTime Now =
        new(2026, 8, 28, 14, 22, 47, DateTimeKind.Utc);

    /// <remarks>
    /// <para>**UNKNOWN IS NEVER ZERO** (HM-DEC-009). A clock nobody has checked
    /// and a clock checked at no drift are different facts, and only one of them
    /// permits slots to be cut.</para>
    /// </remarks>
    [Fact]
    public void AnUnmeasuredClockSaysSoAndIsNotZero()
    {
        var unknown = ClockOffset.Unknown;

        _output.WriteLine($"  unknown: {unknown.Describe(Now)}");

        Assert.False(unknown.IsKnown);
        Assert.Null(unknown.OffsetSeconds);
        Assert.Null(unknown.Age(Now));
        Assert.Null(Ft8Slots.TrueUtc(Now, unknown));

        Assert.Contains("not checked", unknown.Describe(Now), StringComparison.Ordinal);

        // A measured zero is a measurement and reads differently.
        var measuredZero = new ClockOffset(0, Now);

        _output.WriteLine($"  measured zero: {measuredZero.Describe(Now)}");

        Assert.True(measuredZero.IsKnown);
        Assert.NotNull(Ft8Slots.TrueUtc(Now, measuredZero));
        Assert.DoesNotContain(
            "not checked", measuredZero.Describe(Now), StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the amber threshold, and that it is symmetric — a fast clock is as
    /// much a problem as a slow one.
    /// </remarks>
    [Theory]
    [InlineData(0.0, false)]
    [InlineData(0.30, false)]
    [InlineData(0.50, true)]
    [InlineData(-0.50, true)]
    [InlineData(-2.0, true)]
    public void TheAmberThresholdIsSymmetric(double seconds, bool concerning)
    {
        var offset = new ClockOffset(seconds, Now);

        _output.WriteLine(
            $"  {seconds,6:0.00} s -> {(ClockOffset.IsConcerning(offset) ? "amber" : "fine")}"
            + $"   \"{offset.Describe(Now)}\"");

        Assert.Equal(concerning, ClockOffset.IsConcerning(offset));
    }

    /// <remarks>
    /// Proves the age is spoken rather than counted (§0.7), and that staleness is
    /// a separate question from size.
    /// </remarks>
    [Fact]
    public void TheAgeIsSpokenAndStalenessIsItsOwnQuestion()
    {
        var fresh = new ClockOffset(0.12, Now.AddSeconds(-20));
        var older = new ClockOffset(0.12, Now.AddMinutes(-40));
        var stale = new ClockOffset(0.12, Now.AddHours(-3));

        foreach (var (what, offset) in new[]
        {
            ("fresh", fresh), ("older", older), ("stale", stale),
        })
        {
            _output.WriteLine($"  {what,-6} {offset.Describe(Now)}");
        }

        Assert.Contains("just now", fresh.Describe(Now), StringComparison.Ordinal);
        Assert.Contains("40 minutes ago", older.Describe(Now), StringComparison.Ordinal);

        Assert.False(fresh.IsStale(Now));
        Assert.True(stale.IsStale(Now));

        // A small offset measured long ago is stale without being concerning.
        Assert.False(ClockOffset.IsConcerning(stale));
    }

    /// <remarks>
    /// <para>Proves the slot grid falls on UTC quarter-minutes and that the
    /// offset moves it.</para>
    /// <para>**A GRID DRAWN AT A GUESSED BOUNDARY IS THE PRIME DIRECTIVE BROKEN
    /// WHERE NOBODY WOULD CHECK IT**, because wrong lines look exactly like right
    /// ones — so the arithmetic is asserted rather than eyeballed.</para>
    /// </remarks>
    [Fact]
    public void SlotsFallOnQuarterMinutesOfTrueUtc()
    {
        // 14:22:47 with a clock two seconds slow is really 14:22:49.
        var offset = new ClockOffset(2.0, Now);
        var trueUtc = Ft8Slots.TrueUtc(Now, offset)!.Value;

        _output.WriteLine($"  pc {Now:HH:mm:ss}, true {trueUtc:HH:mm:ss}");
        _output.WriteLine(
            $"  slot starts {Ft8Slots.SlotStart(trueUtc):HH:mm:ss}, "
            + $"{Ft8Slots.IntoSlot(trueUtc):0.0} s in");

        Assert.Equal(49, trueUtc.Second);
        Assert.Equal(45, Ft8Slots.SlotStart(trueUtc).Second);
        Assert.Equal(4.0, Ft8Slots.IntoSlot(trueUtc), 3);

        foreach (var second in new[] { 0, 14, 15, 29, 30, 44, 45, 59 })
        {
            var at = new DateTime(2026, 8, 28, 14, 22, second, DateTimeKind.Utc);
            var start = Ft8Slots.SlotStart(at);

            Assert.Equal(0, start.Second % 15);
            Assert.True(start <= at);
            Assert.True(at - start < TimeSpan.FromSeconds(15));
        }
    }

    /// <remarks>
    /// Proves the boundaries a waterfall would rule, over a stretch of time.
    /// </remarks>
    [Fact]
    public void TheBoundariesAcrossAStretchAreTheQuarterMinutes()
    {
        var from = new DateTime(2026, 8, 28, 14, 22, 7, DateTimeKind.Utc);
        var to = from.AddSeconds(40);

        var edges = Ft8Slots.BoundariesBetween(from, to);

        _output.WriteLine(
            $"  {from:HH:mm:ss} to {to:HH:mm:ss}: "
            + string.Join(", ", edges.Select(e => e.ToString("HH:mm:ss"))));

        Assert.Equal(3, edges.Count);
        Assert.Equal(15, edges[0].Second);
        Assert.Equal(30, edges[1].Second);
        Assert.Equal(45, edges[2].Second);

        Assert.Empty(Ft8Slots.BoundariesBetween(to, from));
    }

    /// <remarks>
    /// <para>Proves the SNTP reply parser without a network, including the reply
    /// that means refusal.</para>
    /// <para>**A ZERO TIMESTAMP IS A REFUSAL AND NOT A DATE IN 1900.** Read as a
    /// time it would report the clock as more than a century out, which would
    /// then be displayed as a measurement.</para>
    /// </remarks>
    [Fact]
    public void TheReplyParserReadsATimeAndRefusesAnEmptyOne()
    {
        var reply = new byte[48];

        // 2026-08-28 14:22:47 UTC as seconds since 1900.
        var seconds = (uint)(new DateTime(2026, 8, 28, 14, 22, 47, DateTimeKind.Utc)
            - new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        reply[40] = (byte)(seconds >> 24);
        reply[41] = (byte)(seconds >> 16);
        reply[42] = (byte)(seconds >> 8);
        reply[43] = (byte)seconds;

        var read = SntpClock.TransmitTimestamp(reply);

        _output.WriteLine($"  parsed {read:yyyy-MM-dd HH:mm:ss} UTC");

        Assert.NotNull(read);
        Assert.Equal(new DateTime(2026, 8, 28, 14, 22, 47, DateTimeKind.Utc), read!.Value);

        Assert.Null(SntpClock.TransmitTimestamp(new byte[48]));
        Assert.Null(SntpClock.TransmitTimestamp(new byte[12]));
    }
}
