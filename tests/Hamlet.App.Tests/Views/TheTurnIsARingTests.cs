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
/// Work instruction 280, task 3: the beat is a ring with a countdown in it, and
/// none of its four states is a sentence.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08: show, do not tell.** The line it replaced read
/// *You are transmitting, with 2 seconds of it left. This slot is yours and you are
/// using it, so there is nothing to send into until it finishes* — two sentences to
/// say one thing, on the screen he watches with fourteen seconds to decide in.</para>
/// <para>**IT COUNTS DOWN AND DOES NOTHING ELSE** (§0.2). Nothing reads it, nothing
/// arms on it, and reaching zero sends nothing.</para>
/// <para>**AND THE FOUR SEPARATE WITHOUT COLOUR** (§0.6). Roughly one man in twelve
/// has a colour vision deficiency and this hobby's demographics make that a real
/// slice of the people who will use this, so the ring carries its state in its dash
/// pattern and its thickness as well as its ink, and the caption differs in every
/// state.</para>
/// </remarks>
public sealed class TheTurnIsARingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the four states are printed.</param>
    public TheTurnIsARingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**Each state draws its own ring, and none of them is a sentence.**</summary>
    [AvaloniaFact]
    public void EachStateDrawsItsOwnRingAndNoneIsASentence()
    {
        foreach (var (name, turn) in States())
        {
            var panel = Panel(turn);

            _output.WriteLine(
                name.PadRight(12) + " ring " + panel.TurnRingSweep.ToString("0")
                + "°  count \"" + panel.TurnRingCount + "\"  caption \""
                + panel.TurnRingCaption + "\"");

            // **NO SENTENCES.** A caption is at most a few words, so it carries no
            // full stop and does not run past what a glance takes in.
            Assert.DoesNotContain(".", panel.TurnRingCaption, StringComparison.Ordinal);
            Assert.True(
                panel.TurnRingCaption.Split(' ').Length <= 3,
                name + "'s caption is " + panel.TurnRingCaption.Split(' ').Length
                + " words: \"" + panel.TurnRingCaption + "\"");
        }
    }

    /// <summary>
    /// **The four are distinguishable in grayscale**, by dash and thickness.
    /// </summary>
    /// <remarks>
    /// §0.6. Hue is stripped out here on purpose: what is compared is the dash
    /// pattern, the stroke thickness and the caption, which is everything that
    /// survives a black and white printer.
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
            var caption = ((MainWindowViewModel)window.DataContext!).TurnRingCaption;
            var shape = (dashed ? "dashed" : "solid") + " "
                + arc.StrokeThickness.ToString("0") + "px";

            _output.WriteLine(
                name.PadRight(12) + shape.PadRight(14) + "\"" + caption + "\"");

            seen.Add(shape + "|" + caption);
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

    /// <summary>**Unknown keeps its question mark and never picks a side.**</summary>
    /// <remarks>
    /// Unit 277's rule: before a station has transmitted there is no parity to
    /// derive, so there is no countdown to show either. A zero there would be a
    /// reading nobody took (§0.0).
    /// </remarks>
    [AvaloniaFact]
    public void UnknownShowsAQuestionMarkAndNotAZero()
    {
        foreach (var turn in new[]
        {
            Ft8Turn.Read(At(7), Measured(), null),
            Ft8Turn.Read(At(7), ClockOffset.Unknown, Slot(45)),
        })
        {
            var panel = Panel(turn);

            _output.WriteLine(turn.State + " -> \"" + panel.TurnRingCount + "\"");

            Assert.Equal("?", panel.TurnRingCount);
            Assert.True(panel.TurnRingIsUnknown);
            Assert.False(panel.TurnRingIsHis);
            Assert.False(panel.TurnRingIsOnAir);
        }
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
