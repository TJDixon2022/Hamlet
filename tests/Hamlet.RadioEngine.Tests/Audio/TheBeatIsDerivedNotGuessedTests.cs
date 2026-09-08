using System;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 277, task 3: whose slot this is, how much of it is left, and
/// the two cases where the honest answer is that Hamlet does not know.
/// </summary>
/// <remarks>
/// <para>**THE EXPOSURE IS THE TURN** (§0.0). Telling the operator it is his turn
/// when it is not sends him into a slot the other station is using: he transmits on
/// top of the station he is trying to work, and the collision reads to him like a
/// station that stopped answering. So a turn is derived from what that station has
/// actually transmitted, or it is not claimed.</para>
/// <para>**AND IT COUNTS DOWN AND NOTHING ELSE.** There is no arm, no queue and no
/// send anywhere in <see cref="Ft8Turn"/>; reaching the last second does nothing at
/// all. A countdown that transmits when it expires is automatic sequencing wearing
/// a clock's face.</para>
/// </remarks>
public sealed class TheBeatIsDerivedNotGuessedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheBeatIsDerivedNotGuessedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **A station sending on `:15` and `:45` makes `:00` and `:30` his.**
    /// </summary>
    /// <remarks>
    /// The instruction's own case. All four slots of one minute are read, so the
    /// answer is the alternation rather than one boundary that happens to agree.
    /// </remarks>
    [Fact]
    public void AStationOnTheOddHalfMakesTheEvenHalfHis()
    {
        var offset = Measured();

        foreach (var (second, mine) in new[]
        {
            (0, true), (15, false), (30, true), (45, false),
        })
        {
            var turn = Ft8Turn.Read(At(second, 3), offset, Slot(45));

            _output.WriteLine(
                "at :" + second.ToString("00") + " -> "
                + turn.State + ", " + turn.SecondsLeft + "s left");

            Assert.True(turn.IsKnown);
            Assert.Equal(mine, turn.MineNow);
            Assert.Equal(
                mine ? Ft8TurnState.Mine : Ft8TurnState.Theirs, turn.State);

            // **THE PARITY IS REPORTED AS THE HALF, NOT AS ONE BOUNDARY.** A
            // station on `:15` transmits on `:45` too, and 15 is what says so.
            Assert.Equal(15, turn.TheirSlotSecond);
        }
    }

    /// <summary>
    /// **A station on the even half makes the odd half his**, which is the mirror.
    /// </summary>
    /// <remarks>
    /// Written because a parity bug that reads the wrong way round passes every
    /// test that only ever tries one of the two halves.
    /// </remarks>
    [Fact]
    public void AndTheMirrorHolds()
    {
        var offset = Measured();

        Assert.Equal(Ft8TurnState.Theirs, Ft8Turn.Read(At(0, 3), offset, Slot(30)).State);
        Assert.Equal(Ft8TurnState.Mine, Ft8Turn.Read(At(15, 3), offset, Slot(30)).State);
        Assert.Equal(0, Ft8Turn.Read(At(15, 3), offset, Slot(30)).TheirSlotSecond);
    }

    /// <summary>
    /// **A station that has transmitted nothing yields no turn and no guess.**
    /// </summary>
    /// <remarks>
    /// §0.0. There is no pattern to read, and picking one would be right half the
    /// time, which is the worst possible property for a claim the operator acts on.
    /// </remarks>
    [Fact]
    public void NothingHeardMeansNoTurnIsClaimed()
    {
        var turn = Ft8Turn.Read(At(7, 3), Measured(), null);

        _output.WriteLine(turn.Line());

        Assert.Equal(Ft8TurnState.NoStationYet, turn.State);
        Assert.False(turn.IsKnown);
        Assert.False(turn.MineNow);
        Assert.Null(turn.TheirSlotSecond);

        // **THE COUNTDOWN STILL RUNS**, because the slot boundaries are a fact
        // about the clock and not about any station.
        Assert.Equal(8, turn.SecondsLeft);
    }

    /// <summary>
    /// **An unmeasured clock places no slot at all**, and the PC clock is not used.
    /// </summary>
    /// <remarks>
    /// The send path does fall back to the machine's clock, defensibly, because a
    /// boundary has to be picked for something the operator has already clicked. A
    /// turn line has no such obligation, and a countdown run off an uncorrected
    /// clock and drawn as the beat is a measurement nobody made.
    /// </remarks>
    [Fact]
    public void AnUnmeasuredClockCountsDownToNothing()
    {
        var turn = Ft8Turn.Read(At(7, 3), ClockOffset.Unknown, Slot(45));

        _output.WriteLine(turn.Line());

        Assert.Equal(Ft8TurnState.NoClock, turn.State);
        Assert.False(turn.IsKnown);
        Assert.Null(turn.SecondsLeft);
        Assert.Null(turn.TheirSlotSecond);
    }

    /// <summary>
    /// **The countdown is read off the corrected clock and not the machine's.**
    /// </summary>
    /// <remarks>
    /// The offset here is large enough to move the reading into a different slot,
    /// so a countdown taken from the PC clock cannot coincidentally agree.
    /// </remarks>
    [Fact]
    public void TheCountdownComesFromTheCorrectedClock()
    {
        // The machine says :02. It is four seconds slow, so the truth is :06, and
        // the two disagree about how much of the slot is left by exactly that.
        var pc = At(2, 0);
        var offset = new ClockOffset(4.0, pc);

        var turn = Ft8Turn.Read(pc, offset, Slot(45));

        _output.WriteLine(
            "pc :02, offset +4 -> true :06, " + turn.SecondsLeft + "s left");

        Assert.Equal(9, turn.SecondsLeft);

        // And the uncorrected reading would have said thirteen, so this is not
        // agreeing with the machine by accident.
        Assert.NotEqual(13, turn.SecondsLeft);
    }

    /// <summary>
    /// **The last part of a second reads as one second left, never as none.**
    /// </summary>
    /// <remarks>
    /// A zero on the display says the slot has ended while the station is still
    /// transmitting in it, and it is also the number somebody would later be
    /// tempted to make something happen on.
    /// </remarks>
    [Fact]
    public void TheCountdownNeverShowsZero()
    {
        var turn = Ft8Turn.Read(At(14, 800), Measured(), Slot(45));

        Assert.Equal(1, turn.SecondsLeft);
        Assert.Equal(15, Ft8Turn.Read(At(0, 0), Measured(), Slot(45)).SecondsLeft);
    }

    /// <summary>
    /// **The parity does not shift at a minute or an hour boundary.**
    /// </summary>
    /// <remarks>
    /// A minute holds four slots, an even number, which is what makes this safe.
    /// It is asserted rather than reasoned about because a fifteen-second grid laid
    /// over a sixty-second minute is exactly the kind of arithmetic that is right
    /// until it is not.
    /// </remarks>
    [Fact]
    public void TheParityHoldsAcrossMinutesAndHours()
    {
        var theirs = new DateTime(2026, 9, 8, 2, 11, 45, DateTimeKind.Utc);

        Assert.Equal(1, Ft8Turn.ParityOf(theirs));
        Assert.Equal(1, Ft8Turn.ParityOf(theirs.AddMinutes(1)));
        Assert.Equal(1, Ft8Turn.ParityOf(theirs.AddHours(1)));
        Assert.Equal(1, Ft8Turn.ParityOf(theirs.AddDays(1)));

        // And the other half stays the other half.
        Assert.Equal(0, Ft8Turn.ParityOf(theirs.AddSeconds(15)));
    }

    /// <summary>
    /// **Every state says what it is in words, and the two unknowns say why.**
    /// </summary>
    /// <remarks>
    /// §0.7. *Unknown* teaches nothing; *nobody has transmitted to you yet, so
    /// there is no pattern to read* tells him what would change it. And §0.7's em
    /// dash rule is checked here rather than only by the sweep, because these four
    /// sentences are the ones this unit adds.
    /// </remarks>
    [Fact]
    public void EveryStateSaysWhatItIsAndTheUnknownsSayWhy()
    {
        var offset = Measured();

        var lines = new[]
        {
            Ft8Turn.Read(At(7, 0), ClockOffset.Unknown, null).Line(),
            Ft8Turn.Read(At(7, 0), offset, null).Line(),
            Ft8Turn.Read(At(7, 0), offset, Slot(0)).Line(),
            Ft8Turn.Read(At(7, 0), offset, Slot(15)).Line(),
        };

        foreach (var line in lines)
        {
            _output.WriteLine(line);

            Assert.NotEqual("", line);
            Assert.DoesNotContain("—", line);
            Assert.DoesNotContain("unknown", line, StringComparison.OrdinalIgnoreCase);
        }

        // The four are four different sentences, so no state is silently wearing
        // another one's words.
        Assert.Equal(4, new HashSet<string>(lines, StringComparer.Ordinal).Count);

        // **THE LATE ANSWER IS NAMED WHERE IT IS ABOUT TO HAPPEN.** A click in his
        // own slot arms the following one, which is theirs, and that is the mistake
        // that cost him the contact on 2026-09-08.
        Assert.Contains("slot late", lines[3], StringComparison.Ordinal);
    }

    /// <summary>
    /// **While his own transmission is going out, the line says so.**
    /// </summary>
    /// <remarks>
    /// <para>Work instruction 279 task 6. **This unit's §0.0 exposure**: telling him
    /// his slot is open while his own carrier is on it is worse than saying nothing,
    /// because the one thing he might do about an open slot is fill it.</para>
    /// <para>**IT COUNTS DOWN THE TRANSMISSION AND NOT THE SLOT.** A transmission
    /// occupies 12.64 s of a 15 s slot, so the two numbers are different and the one
    /// he wants is how long his own signal has left to run.</para>
    /// </remarks>
    [Fact]
    public void ATransmissionInFlightSaysSoRatherThanSayingHisSlotIsOpen()
    {
        var slot = Slot(0);
        var turn = Ft8Turn.Read(At(3, 0), Measured(), Slot(45), slot);

        _output.WriteLine("3 s into his own transmission: " + turn.Line());

        Assert.Equal(Ft8TurnState.Transmitting, turn.State);

        // **10 s LEFT OF 12.64**, rounded up, which is the transmission and not the
        // twelve seconds left of the slot.
        Assert.Equal(10, turn.SecondsLeft);

        // **AND IT DOES NOT SAY THE SLOT IS OPEN.** The parity says this is his
        // slot, which is true and is no longer the useful sentence.
        Assert.DoesNotContain("Yours is next", turn.Line(), StringComparison.Ordinal);
        Assert.DoesNotContain("click now", turn.Line(), StringComparison.Ordinal);
        Assert.Contains("You are transmitting", turn.Line(), StringComparison.Ordinal);

        // **IT IS NOT A TURN.** Nothing that decides whether to encourage him
        // should read this as his slot standing open.
        Assert.False(turn.IsKnown);
        Assert.False(turn.MineNow);
    }

    /// <summary>**After it ends, the turn line comes back.**</summary>
    /// <remarks>
    /// A transmission runs 12.64 s of the slot, so the last two seconds belong to
    /// the beat again and the line says whose they are.
    /// </remarks>
    [Fact]
    public void AfterItEndsTheTurnLineReturns()
    {
        // 13 s in: the transmission finished at 12.64.
        var turn = Ft8Turn.Read(At(13, 0), Measured(), Slot(45), Slot(0));

        _output.WriteLine("13 s in, transmission over: " + turn.Line());

        Assert.Equal(Ft8TurnState.Mine, turn.State);
        Assert.Equal(2, turn.SecondsLeft);
        Assert.True(turn.IsKnown);
    }

    /// <summary>**Stopped early says stopped, not that the slot ran out.**</summary>
    /// <remarks>
    /// Part of a message went out and the rest did not, so whoever was listening
    /// heard something incomplete. A line saying the slot ran its course would hide
    /// that at the one moment it matters.
    /// </remarks>
    [Fact]
    public void StoppedEarlySaysSoRatherThanPretendingItRan()
    {
        var turn = Ft8Turn.Read(At(6, 0), Measured(), Slot(45), Slot(0), stopped: true);

        _output.WriteLine("stopped at 6 s: " + turn.Line());

        Assert.Equal(Ft8TurnState.Stopped, turn.State);

        // **NO COUNTDOWN.** There is nothing left running to count.
        Assert.Null(turn.SecondsLeft);

        Assert.Contains("stopped", turn.Line(), StringComparison.Ordinal);
        Assert.Contains("incomplete", turn.Line(), StringComparison.Ordinal);
        Assert.False(turn.IsKnown);
        Assert.False(turn.MineNow);
    }

    /// <summary>**A transmission in a slot that is not now does not claim the line.**</summary>
    /// <remarks>
    /// The slot it went out in is remembered until the run comes back, and a stale
    /// value must not keep saying he is transmitting a minute later.
    /// </remarks>
    [Fact]
    public void AStaleSendingSlotDoesNotHoldTheLine()
    {
        // A transmission from a slot two minutes ago, with the clock now here.
        var old = Slot(0).AddMinutes(-2);
        var turn = Ft8Turn.Read(At(3, 0), Measured(), Slot(45), old);

        _output.WriteLine("stale sending slot: " + turn.Line());

        Assert.NotEqual(Ft8TurnState.Transmitting, turn.State);
        Assert.Equal(Ft8TurnState.Mine, turn.State);
    }

    /// <summary>**Every state still says what it is, including the two new ones.**</summary>
    [Fact]
    public void TheTwoNewStatesSayWhatTheyAreToo()
    {
        var lines = new[]
        {
            Ft8Turn.Read(At(3, 0), Measured(), Slot(45), Slot(0)).Line(),
            Ft8Turn.Read(At(3, 0), Measured(), Slot(45), Slot(0), stopped: true).Line(),
        };

        foreach (var line in lines)
        {
            _output.WriteLine(line);

            Assert.NotEqual("", line);
            Assert.DoesNotContain("—", line);
            Assert.DoesNotContain("unknown", line, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Equal(2, new HashSet<string>(lines, StringComparer.Ordinal).Count);
    }

    /// <summary>An offset that has been measured, so slots can be placed.</summary>
    private static ClockOffset Measured()
        => new(0.0, new DateTime(2026, 9, 8, 2, 11, 0, DateTimeKind.Utc));

    /// <summary>A moment inside the minute the exchange happened in.</summary>
    private static DateTime At(int second, int milliseconds)
        => new DateTime(2026, 9, 8, 2, 11, 0, DateTimeKind.Utc)
            .AddSeconds(second)
            .AddMilliseconds(milliseconds);

    /// <summary>A slot boundary in that minute.</summary>
    private static DateTime Slot(int second)
        => new(2026, 9, 8, 2, 11, second, DateTimeKind.Utc);
}
