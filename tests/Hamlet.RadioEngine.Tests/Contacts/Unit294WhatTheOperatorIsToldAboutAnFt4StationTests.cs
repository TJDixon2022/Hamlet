using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **Work instruction 294 task 1 - the starting position, driven rather than read.**
/// What the contact ledger answers when the traffic is FT4, and how many messages
/// the right-click menu offers on a row that carries no measured ratio.
/// </summary>
/// <remarks>
/// <para>**WHY THIS EXISTS: NOBODY HAD EVER RUN AN FT4 SLOT THROUGH THE
/// LEDGER.** Unit 293 read <see cref="Ft8StationRecord.SlotsAgo"/> and named the
/// arithmetic in its report; the number the operator would actually have seen was
/// never printed. **The breakage this catches is a later unit believing the ledger
/// was already right because a report said the fix was one line**, when nobody had
/// run a slot through it and nothing afterwards can recover the starting figure.
/// </para>
/// <para>**IT ASSERTS TODAY AND NOT THE RIGHT ANSWER.** Every assertion below is
/// on the behaviour at HEAD <c>36dc001</c>, wrong figures included, and it is
/// rewritten by tasks 2 and 5 to assert the right ones - the shape units 292 and
/// 293 used for their own traces. A number that is wrong and recorded is worth
/// more than a number that is wrong and never printed.</para>
/// <para>**THE TWO GRIDS ARE BOTH REAL AND ONLY ONE IS REACHED.**
/// <see cref="SlotGrid.Ft4"/> exists, carries the instance
/// <c>BoundariesBetween</c> and is what the FT4 tab runs on;
/// <see cref="Ft8StationRecord.SlotsAgo"/> calls the static
/// <c>Ft8Slots.BoundariesBetween</c>, which is <see cref="SlotGrid.Ft8"/>'s. So
/// the arithmetic is right and the grid reaching it is not.</para>
/// <para>**NOTHING HERE DECODES OR TRANSMITS.** It is the ledger, the four states
/// and <see cref="Ft8SendOptions"/>, all of which are pure over values.</para>
/// </remarks>
public sealed class Unit294WhatTheOperatorIsToldAboutAnFt4StationTests(ITestOutputHelper output)
{
    /// <summary>Whose station the walk is written from.</summary>
    private const string Operator = "KC3QIS";

    /// <summary>The station heard on FT4.</summary>
    private const string Station = "W9GAP";

    /// <summary>The boundary the walk's slot 0 opens on. A minute boundary, so both grids start together.</summary>
    private static readonly DateTime SlotZeroUtc = new(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>The moment FT4 slot <paramref name="slot"/> opens.</summary>
    private static DateTime Ft4SlotUtc(int slot) =>
        SlotZeroUtc.AddTicks((long)(slot * SlotGrid.Ft4.SlotSeconds * TimeSpan.TicksPerSecond));

    /// <summary>
    /// **The ledger, driven with FT4 traffic, printed beside the true count.**
    /// </summary>
    /// <remarks>
    /// A station heard in FT4 slot 0 and read at FT4 slots 1 to 12. The true count
    /// is the slot index, because the grid steps every 7.5 s; what
    /// <see cref="Ft8StationRecord.SlotsSinceHeard"/> answers is the count of
    /// **fifteen-second** boundaries, which is half of it.
    /// </remarks>
    [Fact]
    public void TheLedgerCountsFt4TrafficOnFt8sGridAndTheRowSaysHalfTheTrueAge()
    {
        var ledger = new Ft8ContactLedger(Operator);
        ledger.RecordHeard(Operator + " " + Station + " EM12", Ft4SlotUtc(0));

        output.WriteLine("THE LEDGER ON FT4 TRAFFIC, BEFORE UNIT 294 CHANGED ANYTHING");
        output.WriteLine($"  grid the tab is running   {SlotGrid.Ft4.Name}, "
            + $"{SlotGrid.Ft4.SlotSeconds:F1} s a slot");
        output.WriteLine($"  grid SlotsAgo counts on   {SlotGrid.Ft8.Name}, "
            + $"{SlotGrid.Ft8.SlotSeconds:F1} s a slot");
        output.WriteLine($"  gone quiet after          {Ft8ContactStates.GoneQuietAfterSlots} slots "
            + $"= {Ft8ContactStates.GoneQuietAfterSeconds:F0} s (the constant, on FT8's grid)");
        output.WriteLine(string.Empty);
        output.WriteLine($"{"true FT4",-10}{"seconds",-10}{"SlotsSinceHeard",-18}{"row says",-24}what it should say");

        var record = ledger.For(Station)!;
        var firstQuietReported = -1;
        var firstQuietTrue = -1;

        for (var slot = 1; slot <= 12; slot++)
        {
            var at = Ft4SlotUtc(slot);
            var answered = record.SlotsSinceHeard(at)!.Value;
            var read = Ft8ContactStates.Read(record, at);
            var trueRead = slot >= Ft8ContactStates.GoneQuietAfterSlots
                ? $"gone quiet, {slot} slots"
                : $"your move, {slot} slots";

            if (firstQuietReported < 0 && read.State == Ft8ContactState.GoneQuiet)
            {
                firstQuietReported = slot;
            }

            if (firstQuietTrue < 0 && slot >= Ft8ContactStates.GoneQuietAfterSlots)
            {
                firstQuietTrue = slot;
            }

            output.WriteLine(
                $"{slot,-10}{(at - Ft4SlotUtc(0)).TotalSeconds,-10:F1}{answered,-18}"
                + $"{read.Text,-24}{trueRead}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  A STATION HEARD FOUR FT4 SLOTS AGO");
        var atFour = Ft4SlotUtc(4);
        output.WriteLine($"    true                    4 FT4 slots, 30.0 s");
        output.WriteLine($"    SlotsSinceHeard         {record.SlotsSinceHeard(atFour)}");
        output.WriteLine($"    the row                 \"{Ft8ContactStates.Read(record, atFour).Text}\"");
        output.WriteLine($"    it should say           \"gone quiet, 4 slots\"");
        output.WriteLine(string.Empty);
        output.WriteLine($"  gone quiet is first reported at FT4 slot {firstQuietReported} "
            + $"({firstQuietReported * SlotGrid.Ft4.SlotSeconds:F1} s); "
            + $"it is true from FT4 slot {firstQuietTrue} "
            + $"({firstQuietTrue * SlotGrid.Ft4.SlotSeconds:F1} s)");

        // TODAY'S ANSWERS. Four FT4 slots after he was heard the ledger says two,
        // because it counted the two fifteen-second boundaries that fell in those
        // thirty seconds.
        Assert.Equal(2, record.SlotsSinceHeard(atFour));
        Assert.Equal("your move, 2 slots", Ft8ContactStates.Read(record, atFour).Text);

        // AND THE THRESHOLD FOLLOWS IT, WHICH IS THE ONE-ROOT-CAUSE READING.
        // The decision at Ft8ContactState.cs:141 is `sinceHeard >= 4` in slots and
        // sinceHeard is SlotsAgo's answer, so gone quiet trips at eight FT4 slots
        // rather than four. Nothing reads GoneQuietAfterSeconds to decide anything.
        Assert.Equal(8, firstQuietReported);
        Assert.Equal(4, firstQuietTrue);
        Assert.Equal("gone quiet, 4 slots", Ft8ContactStates.Read(record, Ft4SlotUtc(8)).Text);

        // The grid that would have been right is already a value and already has
        // the arithmetic. It is simply never asked.
        Assert.Equal(
            4,
            SlotGrid.Ft4.BoundariesBetween(Ft4SlotUtc(0), atFour).Count(b => b > Ft4SlotUtc(0)));
    }

    /// <summary>
    /// **The right-click menu on an FT4 row against the same row on FT8.**
    /// </summary>
    /// <remarks>
    /// The only thing that differs between the two calls is the report: an FT8 row
    /// carries a measured ratio and an FT4 row carries null, because
    /// <c>Ft8Reception.ReadFt4</c> measures nothing. Everything downstream -
    /// <c>DigitalDecodeRow.FormatSnr</c>, <c>MainWindowViewModel.MeasuredReport</c>,
    /// <c>Ft8SendOptions.TextFor</c> - is the same code on both paths.
    /// </remarks>
    [Fact]
    public void TheFt4MenuOffersThreeShapesWhereTheFt8MenuOffersFive()
    {
        var ledger = new Ft8ContactLedger(Operator);
        ledger.RecordHeard(Operator + " " + Station + " EM12", Ft4SlotUtc(0));
        var record = ledger.For(Station)!;

        // The FT4 row: no ratio was measured, so there is no report to send.
        var ft4 = Ft8SendOptions.For(record, Operator, "FN00", reportDecibels: null);

        // The same row on FT8, where unit 251's estimator measured one.
        var ft8 = Ft8SendOptions.For(record, Operator, "FN00", reportDecibels: -12);

        output.WriteLine("THE RIGHT-CLICK MENU, BEFORE UNIT 294 CHANGED ANYTHING");
        output.WriteLine($"  on an FT4 row   {ft4.Options.Count} shapes: "
            + string.Join(", ", ft4.Options.Select(o => o.Label)));
        output.WriteLine($"  on an FT8 row   {ft8.Options.Count} shapes: "
            + string.Join(", ", ft8.Options.Select(o => o.Label)));
        output.WriteLine(string.Empty);
        output.WriteLine("  what the FT4 row says is absent");
        foreach (var line in ft4.Absent)
        {
            output.WriteLine("    " + line);
        }

        Assert.Equal(3, ft4.Options.Count);
        Assert.Equal(5, ft8.Options.Count);

        Assert.DoesNotContain(ft4.Options, o => o.Shape == Ft8SendShape.Report);
        Assert.DoesNotContain(ft4.Options, o => o.Shape == Ft8SendShape.RogerAndReport);
        Assert.Contains(ft8.Options, o => o.Shape == Ft8SendShape.Report);
        Assert.Contains(ft8.Options, o => o.Shape == Ft8SendShape.RogerAndReport);

        // AND THE ONE THAT REACHES STEP 6. The station sent his grid, so what
        // conventionally answers him is a report - and it is the one shape the FT4
        // menu cannot offer at all.
        Assert.Contains(ft8.Options, o => o.Shape == Ft8SendShape.Report && o.IsExpected);
        Assert.DoesNotContain(ft4.Options, o => o.IsExpected);

        Assert.Contains(
            ft4.Absent,
            line => line.Contains("no signal report has been measured", StringComparison.Ordinal));
    }
}
