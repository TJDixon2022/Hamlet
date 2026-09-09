using System;
using System.Linq;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 281, task 3: the ring carries its states with no captions at
/// all, and hovering it says which in words.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**: *"I want clean visual screens with text only
/// where I, the user, intentionally hover."* Unit 280 cut the two-sentence turn line
/// down to two- and three-word captions; this cuts the captions. *click a reply*,
/// *yours is next* and *nothing heard yet* are on the ring's own hover.</para>
/// <para>**TWO CAPTIONS STAY AND BOTH ARE FAULTS.** A clock that has not been
/// measured and a transmission stopped partway are things gone wrong, and a fault
/// speaks unasked — the one exception the ruling carries. The on-air line is not a
/// caption either: it is the message going out, which is a fact.</para>
/// <para>**IT COUNTS DOWN AND DOES NOTHING ELSE** (§0.2). Nothing reads it, nothing
/// arms on it, and reaching zero sends nothing.</para>
/// <para>**AND THE FOUR SEPARATE WITHOUT COLOUR OR WORDS** (§0.6). The caption used
/// to be one of the carriers, so taking it away would have left hue alone to tell
/// his slot from theirs; theirs is now drawn at a thinner stroke. Roughly one man in
/// twelve has a colour vision deficiency and this hobby's demographics make that a
/// real slice of the people who will use this.</para>
/// </remarks>
public sealed class TheTurnIsARingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the four states are printed.</param>
    public TheTurnIsARingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**The three captions the ruling names are gone from the screen.**</summary>
    /// <remarks>
    /// Watched failing first: with the old switch in place all four states carried
    /// words, and this asserted an empty caption for three of them.
    /// </remarks>
    [AvaloniaFact]
    public void TheRingSaysNothingUntilItIsHovered()
    {
        foreach (var (name, turn) in States())
        {
            var panel = Panel(turn);

            _output.WriteLine(
                name.PadRight(12) + " ring " + panel.TurnRingSweep.ToString("0")
                + "°  count \"" + panel.TurnRingCount + "\"  caption \""
                + panel.TurnRingCaption + "\"");

            // **ON AIR IS THE ONE THAT SPEAKS**, and what it says is the message
            // going out rather than a caption about the state.
            if (name == "on air")
            {
                continue;
            }

            Assert.True(
                panel.TurnRingCaption.Length == 0,
                name + " still carries a caption: \"" + panel.TurnRingCaption + "\"");
        }
    }

    /// <summary>**A fault still speaks, and is not behind the hover.**</summary>
    /// <remarks>
    /// The single exception to the ruling. An unmeasured clock and a transmission
    /// stopped partway are both things gone wrong, and §0.0 is broken by omission if
    /// either waits to be asked about.
    /// </remarks>
    [AvaloniaFact]
    public void AFaultKeepsItsWords()
    {
        var noClock = Panel(Ft8Turn.Read(At(7), ClockOffset.Unknown, Slot(45)));

        _output.WriteLine("no clock -> \"" + noClock.TurnRingCaption + "\"");

        Assert.Equal("clock not measured", noClock.TurnRingCaption);
    }

    /// <summary>**Every state answers when it is hovered, and nothing was lost.**</summary>
    /// <remarks>
    /// §0.0 and HM-DEC-092: moving a sentence behind a hover and deleting it look
    /// identical on the screen, and only one of them is allowed.
    /// </remarks>
    [AvaloniaFact]
    public void HoveringTheRingAnswersInEveryState()
    {
        foreach (var (name, turn) in States())
        {
            var panel = Panel(turn);

            _output.WriteLine(name.PadRight(12) + panel.TurnRingTip);

            Assert.False(
                string.IsNullOrWhiteSpace(panel.TurnRingTip),
                name + " has nothing behind its hover");
        }

        // The three that left the screen are reachable, in words a person reads.
        Assert.Contains(
            "Right-click a decoded message",
            Panel(Ft8Turn.Read(At(3), Measured(), Slot(45))).TurnRingTip,
            StringComparison.Ordinal);

        Assert.Contains(
            "the next one is",
            Panel(Ft8Turn.Read(At(18), Measured(), Slot(45))).TurnRingTip,
            StringComparison.Ordinal);

        Assert.Contains(
            "Nothing has been heard on this frequency yet",
            Panel(Ft8Turn.Read(At(3), Measured(), null)).TurnRingTip,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **The four are distinguishable in grayscale**, by dash and thickness.
    /// </summary>
    /// <remarks>
    /// **AND THE CAPTION IS NO LONGER ONE OF THE CARRIERS**, which is what makes
    /// this the load-bearing test of task 3. Hue is stripped out on purpose and so
    /// are the words: what is compared is the dash pattern and the stroke thickness
    /// alone, which is all that survives a black and white printer once the captions
    /// have gone. Watched failing first — before the thin ring for their slot, his
    /// and theirs were both solid 3px and this found two identical shapes.
    /// </remarks>
    [AvaloniaFact]
    public void TheFourStatesSeparateWithoutHue()
    {
        var seen = new System.Collections.Generic.List<string>();

        foreach (var (name, turn) in States())
        {
            var window = Window(turn);

            var arc = window.GetVisualDescendants()
                .OfType<Arc>()
                .FirstOrDefault(a => a.Name == "DigitalTurnRing");

            Assert.True(
                arc is not null,
                "no ring named DigitalTurnRing on the realized window for " + name
                + ". Arcs found: " + window.GetVisualDescendants().OfType<Arc>().Count());

            var dashed = arc!.StrokeDashArray is { Count: > 0 };
            var shape = (dashed ? "dashed" : "solid") + " "
                + arc.StrokeThickness.ToString("0.#") + "px";

            _output.WriteLine(name.PadRight(12) + shape);

            seen.Add(shape);
        }

        // **NO TWO STATES LOOK ALIKE ONCE HUE IS GONE.**
        Assert.Equal(seen.Count, seen.Distinct().Count());
    }

    /// <summary>**On air drains the transmission, the others drain the slot.**</summary>
    /// <remarks>
    /// A slot is 15 s and a transmission is 12.64 s. The arc is a fraction of
    /// whichever it is, so a full ring means *all of this* either way, and what says
    /// which it is, is the thickness and the caption rather than the arc.
    /// </remarks>
    [AvaloniaFact]
    public void OnAirDrainsTheTransmissionAndTheOthersDrainTheSlot()
    {
        // Half a slot left, and half a transmission left, both draw half a ring.
        var halfSlot = Panel(Ft8Turn.Read(At(7), Measured(), Slot(45)));
        var halfSend = Panel(Ft8Turn.Read(At(6), Measured(), Slot(45), Slot(0)));

        _output.WriteLine("8 s of a 15 s slot     -> " + halfSlot.TurnRingSweep.ToString("0") + "°");
        _output.WriteLine("7 s of a 12.64 s send  -> " + halfSend.TurnRingSweep.ToString("0") + "°");

        // 8 of 15 and 7 of 12.64 are both a little over half.
        Assert.InRange(halfSlot.TurnRingSweep, 180, 210);
        Assert.InRange(halfSend.TurnRingSweep, 180, 210);

        // **AND THE ON-AIR RING IS THE THICK ONE**, which is what carries the
        // difference rather than the arc.
        Assert.True(halfSend.TurnRingIsOnAir);
        Assert.False(halfSlot.TurnRingIsOnAir);
    }

    /// <summary>
    /// **The clock counts down even where the turn is unknown, and the turn stays
    /// unknown.**
    /// </summary>
    /// <remarks>
    /// <para>**TIM'S RULING, 2026-09-08** (work instruction 286 task 1): *"Nobody's
    /// responded, but I want to know how long I have till the next transmit cycle.
    /// That might make me decide, oh, I'll do a CQ or I'll respond to this
    /// guy."*</para>
    /// <para>**THIS REPLACES UNIT 277'S `UnknownShowsAQuestionMarkAndNotAZero`**,
    /// which asserted a question mark for both unknown states. That was right about
    /// one of them and wrong about the other, and the red it gave when the rule
    /// changed said so: *Expected "?" / Actual "8"*.</para>
    /// <para>**THE TWO UNKNOWNS ARE NOT THE SAME UNKNOWN.** With no measured offset
    /// there is no boundary to count to, so `NoClock` still shows a question mark —
    /// a zero there would be a reading nobody took (§0.0). With a clock but no
    /// station, the boundary is known to the second and only the parity is not.</para>
    /// <para>**AND THE TURN IS NOT GUESSED EITHER WAY.** Unit 277's rule is
    /// untouched: `TurnRingIsUnknown` stays true, the ring stays dashed, and neither
    /// side is claimed (§0.6 — the dash is the carrier, not the number).</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheClockCountsDownEvenWhereTheTurnIsUnknown()
    {
        // A clock, and nothing heard from anybody. 7 s into the slot leaves 8.
        var noStation = Panel(Ft8Turn.Read(At(7), Measured(), null));

        _output.WriteLine(
            "no station : count \"" + noStation.TurnRingCount + "\"  unknown "
            + noStation.TurnRingIsUnknown + "  sweep "
            + noStation.TurnRingSweep.ToString("0") + "°");

        Assert.Equal("8", noStation.TurnRingCount);
        Assert.True(noStation.TurnRingIsUnknown, "the turn is being claimed");
        Assert.False(noStation.TurnRingIsHis);
        Assert.False(noStation.TurnRingIsTheirs);
        Assert.False(noStation.TurnRingIsOnAir);
        Assert.True(noStation.TurnRingSweep > 0, "the ring is not draining");

        // No offset: no boundary, so no count. This half is unit 277's and stands.
        var noClock = Panel(Ft8Turn.Read(At(7), ClockOffset.Unknown, Slot(45)));

        _output.WriteLine(
            "no clock   : count \"" + noClock.TurnRingCount + "\"  unknown "
            + noClock.TurnRingIsUnknown);

        Assert.Equal("?", noClock.TurnRingCount);
        Assert.True(noClock.TurnRingIsUnknown);
    }

    /// <summary>**The count is the corrected clock and not the machine's.**</summary>
    /// <remarks>
    /// The order names the source: `Ft8Slots.TrueUtc` and the measured offset, not a
    /// second timing source. So the same PC moment under two different offsets must
    /// give two different counts.
    /// </remarks>
    [AvaloniaFact]
    public void TheCountComesFromCorrectedUtc()
    {
        var pc = At(7);

        // Same machine moment; the second offset moves the true time on by 4 s.
        var onTime = Panel(Ft8Turn.Read(pc, Measured(), null));
        var late = Panel(Ft8Turn.Read(
            pc, new ClockOffset(4.0, At(0)), null));

        _output.WriteLine("offset 0 s : " + onTime.TurnRingCount);
        _output.WriteLine("offset 4 s : " + late.TurnRingCount);

        Assert.Equal("8", onTime.TurnRingCount);
        Assert.Equal("4", late.TurnRingCount);
    }

    /// <summary>**The hover says what the count is, not whose slot it is.**</summary>
    [AvaloniaFact]
    public void TheHoverSaysWhatTheCountIs()
    {
        var panel = Panel(Ft8Turn.Read(At(7), Measured(), null));

        _output.WriteLine(panel.TurnRingTip);

        Assert.Contains(
            "next slot starts in 8 seconds", panel.TurnRingTip, StringComparison.Ordinal);

        // And it still says the turn is not known, because it is not.
        Assert.Contains(
            "no turn to work out", panel.TurnRingTip, StringComparison.Ordinal);
    }

    /// <summary>**The on-air caption names the message going out.**</summary>
    [AvaloniaFact]
    public void TheOnAirCaptionIsTheMessageGoingOut()
    {
        var panel = Panel(
            Ft8Turn.Read(At(3), Measured(), Slot(45), Slot(0)),
            sending: "K9XP KC3QIS R-09");

        _output.WriteLine("on air caption: \"" + panel.TurnRingCaption + "\"");

        Assert.Equal("K9XP KC3QIS R-09", panel.TurnRingCaption);
    }

    /// <summary>The four states the ring has to draw.</summary>
    private static (string Name, Ft8Turn Turn)[] States()
        =>
        [
            ("his slot", Ft8Turn.Read(At(3), Measured(), Slot(45))),
            ("their slot", Ft8Turn.Read(At(18), Measured(), Slot(45))),
            ("on air", Ft8Turn.Read(At(3), Measured(), Slot(45), Slot(0))),
            ("unknown", Ft8Turn.Read(At(3), Measured(), null)),
        ];

    /// <summary>A panel holding one turn state.</summary>
    private static MainWindowViewModel Panel(Ft8Turn turn, string sending = "")
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        panel.UseTurnForTests(turn, sending);

        return panel;
    }

    /// <summary>A realized window showing one turn state.</summary>
    private static Avalonia.Controls.Window Window(Ft8Turn turn)
    {
        var window = new MainWindow { DataContext = Panel(turn) };

        window.Show();
        window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        return window;
    }

    /// <summary>An offset that has been measured, so slots can be placed.</summary>
    private static ClockOffset Measured()
        => new(0.0, new DateTime(2026, 9, 8, 2, 11, 0, DateTimeKind.Utc));

    /// <summary>A moment inside the minute.</summary>
    private static DateTime At(int second)
        => new DateTime(2026, 9, 8, 2, 11, 0, DateTimeKind.Utc).AddSeconds(second);

    /// <summary>A slot boundary in that minute.</summary>
    private static DateTime Slot(int second)
        => new(2026, 9, 8, 2, 11, second, DateTimeKind.Utc);
}
