using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 328 task 2, rewritten from unit 327's under §R12: **the mark is the
/// quill again - thin, in the gutter - and clicking it still tells you why.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12, ON UNIT 327'S DISC**: *"so so so so so ugly."* Unit 327 had
/// answered his *"ugly and useless"* about unit 325's 9.9 px of hairline quill by going the
/// other way entirely - **an 18 px disc filled solid** - and a filled blob the height of the
/// row is a bullet rather than a mark. He was drawn three treatments and chose **A: a thin
/// quill in the gutter**, the tray's own vane at row scale, hairline and not filled, with a
/// ring for a door.</para>
/// <para>**SO THE SHAPE IS WHAT THIS CLASS NOW ASSERTS**, and it asserts it off the
/// drawing the control's own `Render` emits rather than off a name: a **stroked path with no
/// fill**, about <see cref="AchievementMarkControl.RowVane"/> px tall, at
/// <see cref="AchievementMarkControl.RowHairline"/> px of pen. **The box is unchanged** at
/// <see cref="AchievementMarkControl.RowSide"/> px - it is the hit target and the room the
/// door's ring needs - so what changed is the ink in it and not the space it takes.</para>
/// <para>**THE POPUP ASSERTIONS ARE UNIT 327'S, UNTOUCHED** (the work instruction says so):
/// the press, the words, the unmarked row's refusal and the telemetry payload are the same
/// four tests that were green before this unit and are green after it.</para>
/// <para>**EVERY APPEARANCE CLAIM HERE IS COMPUTED, NOT SEEN.** These are geometries
/// recorded out of `Render` and properties read off a control, not pixels sampled from a
/// screenshot, and nobody looked at the application while this was written.</para>
/// </remarks>
public sealed class TheMarkIsSeenAndClickedTests : IDisposable
{
    private const string HisCall = "KC3QIS";

    /// <summary>Costa Rica: unworked, on a continent he has worked. A counter.</summary>
    private const string Counter = "TI2AIM";

    /// <summary>Brazil: a continent he has never worked. A door.</summary>
    private const string Door = "PY2ABC";

    /// <summary>The United States, which he has worked. No mark.</summary>
    private const string Worked = "W4JNC";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sizes, colors and words are printed.</param>
    public TheMarkIsSeenAndClickedTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-nudge-mark-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>
    /// **Assertion 1: the mark is a stroked vane, about 12 px tall, and nothing on it is a
    /// filled disc.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS READ OUT OF `Render` AND NOT OFF A PROPERTY NAME.** A control can be
    /// renamed from disc to vane without a pixel changing, so the geometries the mark emits
    /// are recorded and each one is asked what it is, how big it is and whether it is
    /// filled - which is the only form of this assertion that would have failed against unit
    /// 327's mark.</para>
    /// <para>**THE BOX DOES NOT CHANGE AND IS ASSERTED ANYWAY.** 18 px is the hit target and
    /// the room the door's ring needs; the change Tim asked for is the ink inside it.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheMarkIsAHairlineVaneAboutTwelvePixelsTallAndNotAFilledDisc()
    {
        var mark = new AchievementMarkControl { Form = AchievementMarkForm.Counter };

        mark.Measure(new Size(100, 100));

        Assert.Equal(AchievementMarkControl.RowSide, mark.DesiredSize.Width);
        Assert.Equal(AchievementMarkControl.RowSide, mark.DesiredSize.Height);

        Assert.True(mark.IsRowVane, "a counter is not drawn as the vane");

        var drawn = Drawn(AchievementMarkForm.Counter, lit: true);

        foreach (var line in Describe(drawn))
        {
            _output.WriteLine(line);
        }

        // **ONE OBJECT, AND IT IS THE VANE.** A counter draws no ring and no bead, so
        // whatever is here is the mark itself.
        var vane = Assert.Single(drawn);

        // **A ROUND THING IS ASKED ABOUT BY ITS BOUNDS AND NOT BY ITS TYPE**, which this
        // unit measured rather than assumed: the recorder hands every shape back as
        // `PlatformGeometry`, so `IsNotType<EllipseGeometry>` passes on a disc and proves
        // nothing at all. A circle's bounds are square; the vane is taller than it is wide.
        Assert.False(IsRound(vane), "the mark's bounds are square, which a disc's are");

        // **NOT FILLED** - the whole of Tim's complaint about the disc.
        Assert.Null(vane.Brush);

        Assert.NotNull(vane.Pen);
        Assert.Equal(AchievementMarkControl.RowHairline, vane.Pen!.Thickness);

        // **ABOUT TWELVE PIXELS TALL**, measured off the geometry the control drew rather
        // than off the constant it was scaled by.
        Assert.Equal(AchievementMarkControl.RowVane, vane.Geometry!.Bounds.Height, 1);

        _output.WriteLine(
            "327: a disc "
            + AchievementMarkControl.RowSide.ToString("0", CultureInfo.InvariantCulture)
            + " px filled solid -> 328: a vane "
            + vane.Geometry.Bounds.Height.ToString("0.0", CultureInfo.InvariantCulture)
            + " px tall and "
            + vane.Geometry.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
            + " px wide at "
            + AchievementMarkControl.RowHairline.ToString(
                "0.0", CultureInfo.InvariantCulture)
            + " px of pen, not filled");

        // **AND NOTHING ON EITHER FORM IS A DISC.** The door's bead is a filled circle and
        // is meant to be - it is the turn, kept as unit 327 built it - so the assertion is
        // that no filled round thing is anywhere near the size of a mark.
        foreach (var form in new[] { AchievementMarkForm.Counter, AchievementMarkForm.Door })
        {
            foreach (var drawing in Drawn(form, lit: true))
            {
                if (!IsRound(drawing) || drawing.Brush is null)
                {
                    continue;
                }

                var across = drawing.Geometry!.Bounds.Width;

                _output.WriteLine(
                    form + " filled round thing: "
                    + across.ToString("0.0", CultureInfo.InvariantCulture) + " px across");

                Assert.True(
                    across < AchievementMarkControl.RowVane / 2,
                    form + " draws a filled disc " + across + " px across");
            }
        }

        // **AND THE TRAY IS UNTOUCHED**, because that mark's behaviour is ruled and the
        // complaint this unit answers is about a different surface.
        mark.Form = AchievementMarkForm.Tray;

        Assert.False(mark.IsRowVane, "the tray mark stopped being the tray's own quill");
    }

    /// <summary>
    /// **Assertion 2: a counter is green and still; a door is orange, ringed and turning.**
    /// </summary>
    /// <remarks>
    /// **TWO CARRIERS, AND THE SECOND ONE IS A SHAPE** (§0.6, §R16). The ring survives a
    /// greyscale print and a color vision deficiency both, so the two kinds are two
    /// different objects before anybody has to tell green from orange.
    /// </remarks>
    [AvaloniaFact]
    public void ACounterIsGreenAndStillAndADoorIsOrangeAndRingedAndTurning()
    {
        var counter = new AchievementMarkControl
        {
            Form = AchievementMarkForm.Counter,
            IsNew = true,
        };

        var door = new AchievementMarkControl
        {
            Form = AchievementMarkForm.Door,
            IsNew = true,
        };

        _output.WriteLine("counter: " + Hex(counter.LitBrush) + ", ring " + counter.HasRing
            + ", turning " + counter.IsOrbiting);
        _output.WriteLine("door   : " + Hex(door.LitBrush) + ", ring " + door.HasRing
            + ", turning " + door.IsOrbiting);

        // **THE COUNTER'S GREEN IS THE ONE THE INSTRUCTION NAMES.**
        Assert.Equal("#FF3B6D11", Hex(counter.LitBrush));

        Assert.False(counter.HasRing, "a counter drew a ring");
        Assert.False(counter.IsOrbiting, "a counter is moving");

        // **AND THE DOOR'S IS ORANGE, MEASURED AND NOT ASSERTED BY ITS NAME.** The
        // palette calls `#C25E00` amber and Tim ruled orange, so the question is settled
        // by the hue and not by what the resource is called: 194, 94, 0 is **hue 29
        // degrees**, and orange is 30. It reads orange.
        Assert.Equal("#FFC25E00", Hex(door.LitBrush));

        var (hue, saturation, value) = Hsv(door.LitBrush);

        _output.WriteLine(
            "door hue " + hue.ToString("0.0", CultureInfo.InvariantCulture)
            + " deg, saturation " + saturation.ToString("0.00", CultureInfo.InvariantCulture)
            + ", value " + value.ToString("0.00", CultureInfo.InvariantCulture));

        Assert.InRange(hue, 20, 40);

        Assert.True(door.HasRing, "a door drew no ring");
        Assert.True(door.IsOrbiting, "a door is not turning");

        // **AND THE RING IS A DRAWN RING AND NOT A FLAG**, recorded out of `Render` the same
        // way the vane is: a round thing, no fill, hairline, wide enough to hold the vane.
        var rings = Drawn(AchievementMarkForm.Door, lit: true)
            .Where(d => IsRound(d) && d.Brush is null)
            .ToList();

        foreach (var line in Describe(Drawn(AchievementMarkForm.Door, lit: true)))
        {
            _output.WriteLine(line);
        }

        var ring = Assert.Single(rings);

        Assert.NotNull(ring.Pen);
        Assert.Equal(AchievementMarkControl.RowHairline, ring.Pen!.Thickness);

        Assert.True(
            ring.Geometry!.Bounds.Width > AchievementMarkControl.RowVane,
            "the door's ring is narrower than the vane inside it: "
            + ring.Geometry.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture));

        // **AND A COUNTER DRAWS NO ROUND THING AT ALL**, which is the §0.6 difference.
        Assert.DoesNotContain(Drawn(AchievementMarkForm.Counter, lit: true), IsRound);
    }

    /// <summary>
    /// **Assertion 3: a worked, faded station has no mark, as now.**
    /// </summary>
    /// <remarks>
    /// **§3.7: AN UNMARKED ROW IS NOT A LESSER ROW.** Nothing about this changes and the
    /// assertion is here so that a later change to the mark cannot quietly start marking
    /// everybody.
    /// </remarks>
    [Fact]
    public void AWorkedStationHasNoMark()
    {
        var model = Panel(null);

        Heard(model, Counter);
        Heard(model, Door);
        Heard(model, Worked);

        foreach (var row in model.DigitalDecodes)
        {
            _output.WriteLine(
                row.Sender.PadRight(8) + row.Nudge.ToString().PadRight(9)
                + row.NudgeForm);
        }

        Assert.False(Row(model, Worked).IsNudged);
        Assert.Equal(AchievementMarkForm.Counter, Row(model, Counter).NudgeForm);
        Assert.Equal(AchievementMarkForm.Door, Row(model, Door).NudgeForm);
    }

    /// <summary>
    /// **Assertion 4: clicking opens the popup, and the X closes it.**
    /// </summary>
    /// <remarks>
    /// **THE WHOLE OF TIM'S SECOND COMPLAINT** - *"no achievement possibility click
    /// response"* - is this one property going true when a command runs.
    /// </remarks>
    [Fact]
    public void ClickingOpensThePopupAndTheXClosesIt()
    {
        var model = Panel(null);

        Heard(model, Counter);

        var row = Row(model, Counter);

        Assert.False(row.NudgeIsOpen, "the popup was open before anybody pressed anything");

        row.OpenTheNudgeCommand.Execute(null);

        _output.WriteLine("after the press: open " + row.NudgeIsOpen);

        Assert.True(row.NudgeIsOpen, "pressing the mark did nothing");

        row.CloseTheNudgeCommand.Execute(null);

        Assert.False(row.NudgeIsOpen, "the dismiss X did nothing");

        // **AND AN UNMARKED ROW'S COMMAND REFUSES** rather than opening an empty box
        // for somebody who reached it by keyboard (§0.5.1).
        Heard(model, Worked);

        var unmarked = Row(model, Worked);

        unmarked.OpenTheNudgeCommand.Execute(null);

        Assert.False(unmarked.NudgeIsOpen, "an unmarked row opened a reason");
    }

    /// <summary>
    /// **Assertion 5: the popup names the entity for a counter and names nothing for a
    /// door, and nothing anywhere says *confirmed*.**
    /// </summary>
    /// <remarks>
    /// <para>**§3.1 IS THE HARD ONE.** The CQ list must not be the thing that tells him an
    /// area exists, so the door's words are checked against every continent name the cited
    /// table carries rather than against the one this fixture happens to use.</para>
    /// <para>**§4: WORKED, NEVER CONFIRMED.** Hamlet has contacts and no confirmations, and
    /// the count in the counter's line is the place that word would have crept in.</para>
    /// </remarks>
    [Fact]
    public void ThePopupNamesTheEntityForACounterAndNamesNothingForADoor()
    {
        var model = Panel(null);

        Heard(model, Counter);
        Heard(model, Door);

        var counter = Row(model, Counter);
        var door = Row(model, Door);

        foreach (var row in new[] { counter, door })
        {
            _output.WriteLine(row.Sender + " hover : " + row.NudgeTip);
            _output.WriteLine(row.Sender + " reason: " + row.NudgeReasonLine);
            _output.WriteLine(row.Sender + " earns : " + row.NudgeEarnsLine);
        }

        // **THE COUNTER NAMES THE ENTITY AND THE COUNT IT MOVES.**
        Assert.Contains("Costa Rica", counter.NudgeReasonLine, StringComparison.Ordinal);
        Assert.Contains("a new country", counter.NudgeReasonLine, StringComparison.Ordinal);
        Assert.Contains(
            "you have worked", counter.NudgeReasonLine, StringComparison.Ordinal);

        // **THE DOOR NAMES NOTHING.** Not the entity, not the continent, not any
        // continent.
        Assert.DoesNotContain("Brazil", door.NudgeReasonLine, StringComparison.OrdinalIgnoreCase);

        foreach (var continent in new[]
        {
            "Africa", "Antarctica", "Asia", "Europe", "North America", "Oceania",
            "South America",
        })
        {
            Assert.DoesNotContain(
                continent, door.NudgeReasonLine, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Equal(NudgeWords.DoorReason, door.NudgeReasonLine);

        // **AND BOTH SAY WHAT WORKING HIM EARNS, IN ONE LINE, THE SAME ONE.**
        Assert.Equal(NudgeWords.Earns, counter.NudgeEarnsLine);
        Assert.Equal(NudgeWords.Earns, door.NudgeEarnsLine);

        // **THE WORD *confirmed* APPEARS NOWHERE** (§4).
        foreach (var words in new[]
        {
            counter.NudgeTip, counter.NudgeReasonLine, counter.NudgeEarnsLine,
            door.NudgeTip, door.NudgeReasonLine, door.NudgeEarnsLine,
            NudgeWords.Door, NudgeWords.DoorReason, NudgeWords.Earns,
        })
        {
            Assert.DoesNotContain("confirm", words, StringComparison.OrdinalIgnoreCase);
        }

        // **AND THE HOVER IS STILL ONE LINE**, unchanged by any of this.
        Assert.DoesNotContain("\n", counter.NudgeTip, StringComparison.Ordinal);
        Assert.DoesNotContain("\n", door.NudgeTip, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Assertion 6: `nudge_opened` carries the kind and nothing else.**
    /// </summary>
    /// <remarks>
    /// **HM-DEC-018, §2.1.** Not the callsign, not the entity, not the continent, not the
    /// frequency. `counter` or `door`, and that is the whole payload.
    /// </remarks>
    [Fact]
    public void TheRecordSaysWhichKindWasOpenedAndNothingElse()
    {
        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "327", _ => true))
        {
            var model = Panel(telemetry);

            Heard(model, Counter);
            Heard(model, Door);

            Row(model, Counter).OpenTheNudgeCommand.Execute(null);
            Row(model, Door).OpenTheNudgeCommand.Execute(null);
        }

        lines = Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .Where(l => l.Contains("nudge_opened", StringComparison.Ordinal))
            .ToList();

        foreach (var line in lines)
        {
            _output.WriteLine(line);
        }

        Assert.Equal(2, lines.Count);

        var kinds = lines
            .Select(l => JsonDocument.Parse(l).RootElement.GetProperty("data"))
            .ToList();

        Assert.Contains(kinds, d => d.GetProperty("kind").GetString() == "counter");
        Assert.Contains(kinds, d => d.GetProperty("kind").GetString() == "door");

        foreach (var data in kinds)
        {
            Assert.Single(data.EnumerateObject());
        }

        // **AND NOTHING PERSONAL IS ANYWHERE IN THOSE LINES.**
        foreach (var word in new[] { Counter, Door, "Costa Rica", "Brazil", HisCall })
        {
            Assert.DoesNotContain(
                lines,
                l => l.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>Whether one recorded drawing is a circle - a ring, a bead or a disc.</summary>
    /// <remarks>
    /// **MEASURED, BECAUSE THE TYPE NAME IS NOT AVAILABLE.** The recorder returns every
    /// shape as `PlatformGeometry`, whatever it was drawn as, so a test asking whether the
    /// mark is an ellipse by its type passes on unit 327's filled disc. Square bounds are
    /// what a circle actually has and what this quill's vane never has.
    /// </remarks>
    private static bool IsRound(GeometryDrawing drawing)
    {
        var bounds = drawing.Geometry?.Bounds ?? default;

        return bounds.Width > 0
            && Math.Abs(bounds.Width - bounds.Height) < 0.01;
    }

    /// <summary>**What the control's own `Render` emitted**, recorded rather than recomputed.</summary>
    /// <param name="form">Which of the three jobs the mark is doing.</param>
    /// <param name="lit">Whether something unseen has been earned.</param>
    /// <returns>Every geometry the recorder holds, however deeply it nested it.</returns>
    /// <remarks>
    /// **THE VANE IS DRAWN INSIDE A TRANSFORM AND THE RECORDER NESTS IT**, which unit 300's
    /// own sizes test found the hard way - reading only the top level reports the mark drawing
    /// nothing at all. The transform the row mark pushes is a translation, so the sizes read
    /// off these geometries are the sizes on the screen.
    /// </remarks>
    private static IReadOnlyList<GeometryDrawing> Drawn(AchievementMarkForm form, bool lit)
    {
        var side = AchievementMarkControl.RowSide;

        var mark = new AchievementMarkControl
        {
            Width = side,
            Height = side,
            Form = form,
            IsNew = lit,
        };

        mark.Measure(new Size(side, side));
        mark.Arrange(new Rect(0, 0, side, side));

        var group = new DrawingGroup();

        using (var context = group.Open())
        {
            mark.Render(context);
        }

        var flat = new List<GeometryDrawing>();

        Flatten(group, flat);

        return flat;
    }

    private static void Flatten(DrawingGroup group, List<GeometryDrawing> into)
    {
        foreach (var child in group.Children)
        {
            switch (child)
            {
                case GeometryDrawing drawing:
                    into.Add(drawing);
                    break;

                case DrawingGroup nested:
                    Flatten(nested, into);
                    break;

                default:
                    break;
            }
        }
    }

    /// <summary>One line per drawing: what it is, how big, and how it is inked.</summary>
    private static IEnumerable<string> Describe(IReadOnlyList<GeometryDrawing> drawings)
        => drawings.Select(d =>
        {
            var bounds = d.Geometry?.Bounds ?? default;

            return (d.Geometry?.GetType().Name ?? "(none)").PadRight(18)
                + bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
                + " x " + bounds.Height.ToString("0.0", CultureInfo.InvariantCulture)
                + "   fill " + (d.Brush is null ? "(none)" : Hex(d.Brush))
                + "   pen " + (d.Pen is null
                    ? "(none)"
                    : d.Pen.Thickness.ToString("0.0", CultureInfo.InvariantCulture));
        });

    private static string Hex(IBrush brush)
        => (brush is ISolidColorBrush solid ? solid.Color.ToString() : brush.ToString() ?? "")
            .ToUpperInvariant();

    /// <summary>Hue in degrees, saturation and value, from a brush.</summary>
    /// <remarks>
    /// **SO *IS IT ORANGE* IS ANSWERED BY ARITHMETIC AND NOT BY THE RESOURCE'S NAME.** The
    /// palette calls this color amber; the work instruction asks whether it reads orange,
    /// which is a question about the hue.
    /// </remarks>
    private static (double Hue, double Saturation, double Value) Hsv(IBrush brush)
    {
        var color = ((ISolidColorBrush)brush).Color;

        double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var span = max - min;

        var hue = span == 0 ? 0
            : max == r ? 60 * (((g - b) / span) % 6)
            : max == g ? 60 * (((b - r) / span) + 2)
            : 60 * (((r - g) / span) + 4);

        return ((hue + 360) % 360, max == 0 ? 0 : span / max, max);
    }

    private static DigitalDecodeRow Row(MainWindowViewModel model, string who)
        => model.DigitalDecodes.Single(r => r.Sender == who);

    /// <summary>A panel whose log holds the United States and nothing else.</summary>
    /// <remarks>
    /// **ONE WORKED ENTITY IS WHAT PUTS BOTH KINDS ON THE LIST AT ONCE.** North America is
    /// then a continent he has worked, so Costa Rica is a counter; South America is one he
    /// has not, so Brazil is a door.
    /// </remarks>
    private static MainWindowViewModel Panel(JsonlTelemetry? telemetry)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, telemetry)
        {
            DigitalNewestFirst = false,
        };

        var entities = new[] { "W9ZZZ" }
            .Select(DxccPrefixes.EntityOf)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());
        model.UseNudgeSetForTests(
            new NudgeSet(entities, entities.Select(DxccContinents.Of)));

        return model;
    }

    private static void Heard(MainWindowViewModel model, string who)
        => model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240", "CQ " + who + " EK99",
            new DateTime(2026, 9, 12, 2, 11, 15, DateTimeKind.Utc),
            heardOnHz: 14_074_000);
}
