using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **Work instruction 294 tasks 1 and 2 - what the operator is told about an FT4
/// station.** What the contact ledger answers when the traffic is FT4, and how many
/// messages the right-click menu offers on a row that carries no measured ratio.
/// </summary>
/// <remarks>
/// <para>**WHY THIS EXISTS: NOBODY HAD EVER RUN AN FT4 SLOT THROUGH THE
/// LEDGER.** Unit 293 read <see cref="Ft8StationRecord.SlotsAgo"/> and named the
/// arithmetic in its report; the number the operator would actually have seen was
/// never printed. **The breakage this catches is a later unit believing the ledger
/// was already right because a report said the fix was one line**, when nobody had
/// run a slot through it and nothing afterwards can recover the starting figure.
/// </para>
/// <para>**WHAT IT ASSERTED FIRST AND WHAT IT ASSERTS NOW.** As written for task 1
/// it asserted the behaviour at HEAD <c>36dc001</c>, wrong figures included: a
/// station heard four FT4 slots ago read *your move, 2 slots*, and gone quiet was
/// first reported at eight FT4 slots rather than four. **Task 2 rewrote the ledger
/// case to assert the right answers**, with the wrong ones quoted in the comments
/// beside them so that what moved is on the record. The menu case still asserts
/// today, and task 5 rewrites that one.</para>
/// <para>**THE TWO GRIDS WERE BOTH REAL AND ONLY ONE WAS REACHED.**
/// <see cref="SlotGrid.Ft4"/> existed, carried the instance
/// <c>BoundariesBetween</c> and was what the FT4 tab ran on;
/// <see cref="Ft8StationRecord.SlotsAgo"/> called the static
/// <c>Ft8Slots.BoundariesBetween</c>, which is <see cref="SlotGrid.Ft8"/>'s. So the
/// arithmetic was right and the grid reaching it was not, and task 2's whole change
/// is the grid arriving as a parameter with no default.</para>
/// <para>**ONE ROOT CAUSE AND NOT TWO.** Unit 293's section 4 gave *gone quiet* and
/// *slots ago* as separate defects. They are one: the decision in
/// <c>Ft8ContactStates.Read</c> is <c>sinceHeard &gt;= GoneQuietAfterSlots</c>, in
/// slots, and <c>sinceHeard</c> is <c>SlotsAgo</c>'s answer - so the grid reaching
/// <c>SlotsAgo</c> reached the threshold with it. The walk below asserts both in one
/// table, which is where a partial fix would show.</para>
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
    /// **The ledger, driven with FT4 traffic, counting on FT4's grid.**
    /// </summary>
    /// <remarks>
    /// A station heard in FT4 slot 0 and read at FT4 slots 1 to 12. The true count
    /// is the slot index, because the grid steps every 7.5 s. Before task 2
    /// <see cref="Ft8StationRecord.SlotsSinceHeard"/> answered the count of
    /// **fifteen-second** boundaries, which is half of it, and *gone quiet* -
    /// tested in slots against the same figure - therefore waited twice as long.
    /// </remarks>
    [Fact]
    public void TheLedgerCountsFt4TrafficOnFt4sGridAndGoneQuietFollowsItInTheSameRow()
    {
        var ledger = new Ft8ContactLedger(Operator);
        ledger.RecordHeard(Operator + " " + Station + " EM12", Ft4SlotUtc(0));

        output.WriteLine("THE LEDGER ON FT4 TRAFFIC");
        output.WriteLine($"  grid the tab is running   {SlotGrid.Ft4.Name}, "
            + $"{SlotGrid.Ft4.SlotSeconds:F1} s a slot");
        output.WriteLine($"  gone quiet after          {Ft8ContactStates.GoneQuietAfterSlots} slots "
            + $"= {Ft8ContactStates.GoneQuietAfterSeconds(SlotGrid.Ft4):F0} s on FT4, "
            + $"{Ft8ContactStates.GoneQuietAfterSeconds(SlotGrid.Ft8):F0} s on FT8");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"{"true FT4",-10}{"seconds",-10}{"SlotsSinceHeard",-18}{"the row reads",-24}"
            + "what it read at 36dc001");

        var record = ledger.For(Station)!;
        var firstQuiet = -1;

        for (var slot = 1; slot <= 12; slot++)
        {
            var at = Ft4SlotUtc(slot);
            var answered = record.SlotsSinceHeard(at, SlotGrid.Ft4)!.Value;
            var read = Ft8ContactStates.Read(record, at, SlotGrid.Ft4);

            // What the same moment said before task 2: the FT8 grid, whatever mode
            // the tab was running. Printed rather than remembered.
            var before = Ft8ContactStates.Read(record, at, SlotGrid.Ft8);

            if (firstQuiet < 0 && read.State == Ft8ContactState.GoneQuiet)
            {
                firstQuiet = slot;
            }

            output.WriteLine(
                $"{slot,-10}{(at - Ft4SlotUtc(0)).TotalSeconds,-10:F1}{answered,-18}"
                + $"{read.Text,-24}{before.Text}");

            // **THE COUNT IS THE TRUE COUNT AT EVERY SLOT OF THE WALK**, which is
            // what a partial fix could not do: threading the grid into SlotsAgo
            // without the threshold following, or the other way round, breaks this
            // table in the middle rather than at one row.
            Assert.Equal(slot, answered);
            Assert.Equal(slot, read.Slots);
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  A STATION HEARD FOUR FT4 SLOTS AGO");
        var atFour = Ft4SlotUtc(4);
        output.WriteLine("    true                    4 FT4 slots, 30.0 s");
        output.WriteLine($"    SlotsSinceHeard         {record.SlotsSinceHeard(atFour, SlotGrid.Ft4)}");
        output.WriteLine($"    the row                 "
            + $"\"{Ft8ContactStates.Read(record, atFour, SlotGrid.Ft4).Text}\"");
        output.WriteLine("    it read at 36dc001      \"your move, 2 slots\"");
        output.WriteLine(string.Empty);
        output.WriteLine($"  gone quiet is first reported at FT4 slot {firstQuiet} "
            + $"({firstQuiet * SlotGrid.Ft4.SlotSeconds:F1} s); at 36dc001 it was slot 8 (60.0 s)");

        // FOUR FT4 SLOTS IS FOUR. It read 2 at HEAD 36dc001.
        Assert.Equal(4, record.SlotsSinceHeard(atFour, SlotGrid.Ft4));

        // AND THE ROW SAYS SO. It read "your move, 2 slots" at HEAD 36dc001, about
        // a station that had been silent for half a minute.
        Assert.Equal("gone quiet, 4 slots", Ft8ContactStates.Read(record, atFour, SlotGrid.Ft4).Text);

        // THE THRESHOLD FOLLOWED THE COUNT WITHOUT BEING TOUCHED. It tripped at
        // FT4 slot 8 at HEAD 36dc001 and trips at 4 now.
        Assert.Equal(Ft8ContactStates.GoneQuietAfterSlots, firstQuiet);
        Assert.Equal(
            Ft8ContactState.YourMove,
            Ft8ContactStates.Read(record, Ft4SlotUtc(3), SlotGrid.Ft4).State);

        // The seconds figure is derived and follows the grid too.
        Assert.Equal(30.0, Ft8ContactStates.GoneQuietAfterSeconds(SlotGrid.Ft4));
        Assert.Equal(60.0, Ft8ContactStates.GoneQuietAfterSeconds(SlotGrid.Ft8));
    }

    /// <summary>
    /// **FT8's own answers, in the same walk, on fifteen-second traffic.**
    /// </summary>
    /// <remarks>
    /// The control for *do not change what an FT8 ledger row does*. The same
    /// station, heard on FT8's grid and read on FT8's grid: four slots is four,
    /// gone quiet trips at four, and that is what it did at HEAD <c>36dc001</c>.
    /// </remarks>
    [Fact]
    public void AnFt8RowCountsAndTripsExactlyWhereItDidBefore()
    {
        var ledger = new Ft8ContactLedger(Operator);
        var heard = SlotZeroUtc;
        ledger.RecordHeard(Operator + " " + Station + " EM12", heard);
        var record = ledger.For(Station)!;

        output.WriteLine("THE SAME WALK ON FT8, WHICH MUST NOT HAVE MOVED");
        output.WriteLine($"{"slot",-8}{"seconds",-10}{"SlotsSinceHeard",-18}the row reads");

        var firstQuiet = -1;

        for (var slot = 1; slot <= 8; slot++)
        {
            var at = heard.AddSeconds(slot * SlotGrid.Ft8.SlotSeconds);
            var answered = record.SlotsSinceHeard(at, SlotGrid.Ft8)!.Value;
            var read = Ft8ContactStates.Read(record, at, SlotGrid.Ft8);

            if (firstQuiet < 0 && read.State == Ft8ContactState.GoneQuiet)
            {
                firstQuiet = slot;
            }

            output.WriteLine(
                $"{slot,-8}{(at - heard).TotalSeconds,-10:F1}{answered,-18}{read.Text}");

            Assert.Equal(slot, answered);
        }

        Assert.Equal(4, firstQuiet);
        Assert.Equal(
            "gone quiet, 4 slots",
            Ft8ContactStates.Read(record, heard.AddSeconds(60), SlotGrid.Ft8).Text);
        Assert.Equal(
            "your move, 3 slots",
            Ft8ContactStates.Read(record, heard.AddSeconds(45), SlotGrid.Ft8).Text);
    }

    /// <summary>
    /// **The menu gap unit 294 closed, and the one thing that was ever behind it.**
    /// </summary>
    /// <remarks>
    /// <para>**THE MENU CODE WAS NEVER THE DEFECT AND THIS IS WHERE THAT SHOWS.**
    /// <c>Ft8SendOptions</c> asks one question - is there a measured report - and
    /// offers five shapes when there is and three when there is not, on either mode.
    /// It is the same code on both paths and always was. Before this unit
    /// <c>Ft8Reception.ReadFt4</c> measured nothing, so the answer on FT4 was always
    /// *there is not*, and the menu offered three.</para>
    /// <para>**SO THE TWO CALLS BELOW DIFFER IN ONE ARGUMENT** - the report - and the
    /// left-hand column is what every FT4 row got at HEAD <c>36dc001</c> while the
    /// right-hand column is what every FT8 row got. **Task 5 made an FT4 row take the
    /// right-hand column**, and
    /// <c>Unit294AnFt4RowCarriesAMeasuredReportTests</c> proves it end to end through
    /// the reader rather than by passing a number in here.</para>
    /// <para>**AND THE SHAPE THAT WAS MISSING IS THE ONE STEP 6 NEEDS.** The station
    /// sent his grid, so what conventionally answers him is a report - and it was the
    /// one shape the FT4 menu could not offer at all, which is why an FT4 exchange
    /// could not be completed.</para>
    /// </remarks>
    [Fact]
    public void TheMenuOffersThreeShapesWithNoReportAndFiveWithOneOnEitherMode()
    {
        var ledger = new Ft8ContactLedger(Operator);
        ledger.RecordHeard(Operator + " " + Station + " EM12", Ft4SlotUtc(0));
        var record = ledger.For(Station)!;

        // WHAT EVERY FT4 ROW GOT AT HEAD 36dc001: no ratio was measured, so there was
        // no report to send.
        var withoutReport = Ft8SendOptions.For(record, Operator, "FN00", reportDecibels: null);

        // AND WHAT EVERY FT8 ROW GOT, AND WHAT AN FT4 ROW GETS NOW.
        var withReport = Ft8SendOptions.For(record, Operator, "FN00", reportDecibels: -12);

        output.WriteLine("THE RIGHT-CLICK MENU, BY WHETHER A RATIO WAS MEASURED");
        output.WriteLine($"  no report   {withoutReport.Options.Count} shapes: "
            + string.Join(", ", withoutReport.Options.Select(o => o.Label)));
        output.WriteLine($"  a report    {withReport.Options.Count} shapes: "
            + string.Join(", ", withReport.Options.Select(o => o.Label)));
        output.WriteLine(string.Empty);
        output.WriteLine("  at HEAD 36dc001 every FT4 row took the first line and every");
        output.WriteLine("  FT8 row took the second. Task 5 made FT4 take the second.");
        output.WriteLine(string.Empty);
        output.WriteLine("  what the first line says is absent");
        foreach (var line in withoutReport.Absent)
        {
            output.WriteLine("    " + line);
        }

        Assert.Equal(3, withoutReport.Options.Count);
        Assert.Equal(5, withReport.Options.Count);

        Assert.DoesNotContain(withoutReport.Options, o => o.Shape == Ft8SendShape.Report);
        Assert.DoesNotContain(withoutReport.Options, o => o.Shape == Ft8SendShape.RogerAndReport);
        Assert.Contains(withReport.Options, o => o.Shape == Ft8SendShape.Report);
        Assert.Contains(withReport.Options, o => o.Shape == Ft8SendShape.RogerAndReport);

        // THE SHAPE THAT REACHES STEP 6. It is the expected one and it was the missing
        // one, which is the whole of why an FT4 exchange could not be completed.
        Assert.Contains(withReport.Options, o => o.Shape == Ft8SendShape.Report && o.IsExpected);
        Assert.DoesNotContain(withoutReport.Options, o => o.IsExpected);

        Assert.Contains(
            withoutReport.Absent,
            line => line.Contains("no signal report has been measured", StringComparison.Ordinal));
    }
}
