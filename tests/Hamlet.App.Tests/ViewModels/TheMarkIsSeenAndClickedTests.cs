using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
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
/// Work instruction 327 task 3: **the mark is a thing you can see, and clicking it tells
/// you why.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12, LOOKING AT WHAT UNIT 325 SHIPPED**: *"ugly and useless"*, and
/// *"no achievement possibility click response"*. Two nights earlier he had ruled that a
/// subtle mark is invisible to him - *"It's a tiny dot lost in the sea of the tray"* - and
/// 325's row mark is that mark: **a 16 px box with the quill drawn at `side * 0.62`, which
/// is 9.9 px of hairline strokes**, and nothing at all happens when it is pressed.</para>
/// <para>**SO THIS UNIT SHIPS A DISC.** The work instruction allows it in as many words -
/// *if the quill shape does not survive at row height, a filled disc does, and the shape
/// difference for a door is the ring* - and a disc filled green or orange is a mark rather
/// than a bar (§0.5, HM-DEC-012). Row height is
/// <see cref="AchievementMarkControl.RowSide"/>, 18 px, and 18 px of solid ink against 9.9
/// px of outline is about thirty times the area.</para>
/// <para>**EVERY APPEARANCE CLAIM HERE IS COMPUTED, NOT SEEN.** These are properties read
/// off a control and a view model, not pixels sampled from a screenshot, and nobody looked
/// at the application while this was written.</para>
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
    /// **Assertion 1: a qualifying row's mark is row-height, and it is a disc.**
    /// </summary>
    /// <remarks>
    /// **THE SIZE IS THE SMALLER HALF OF THE CHANGE AND THE SHAPE IS THE LARGER.** A 16 px
    /// box holding 9.9 px of hairline quill and an 18 px box holding an 18 px filled disc
    /// differ by two pixels of box and by everything of substance.
    /// </remarks>
    [Fact]
    public void AQualifyingRowsMarkIsRowHeightAndIsADisc()
    {
        var mark = new AchievementMarkControl { Form = AchievementMarkForm.Counter };

        _output.WriteLine(
            "325 shipped 16 px of box with the quill at side*0.62 = "
            + (16 * 0.62).ToString("0.0", CultureInfo.InvariantCulture)
            + " px of stroke; 327 ships "
            + AchievementMarkControl.RowSide.ToString("0", CultureInfo.InvariantCulture)
            + " px of box filled solid");

        mark.Measure(new Avalonia.Size(100, 100));

        Assert.Equal(AchievementMarkControl.RowSide, mark.DesiredSize.Width);
        Assert.Equal(AchievementMarkControl.RowSide, mark.DesiredSize.Height);

        Assert.True(mark.IsDisc, "a counter is not drawn as a disc");

        mark.Form = AchievementMarkForm.Door;

        Assert.True(mark.IsDisc, "a door is not drawn as a disc");

        // **AND THE TRAY IS UNTOUCHED**, because that mark's behaviour is ruled and the
        // complaint this unit answers is about a different surface.
        mark.Form = AchievementMarkForm.Tray;

        Assert.False(mark.IsDisc, "the tray mark stopped being the quill");
    }

    /// <summary>
    /// **Assertion 2: a counter is green and still; a door is orange, ringed and turning.**
    /// </summary>
    /// <remarks>
    /// **TWO CARRIERS, AND THE SECOND ONE IS A SHAPE** (§0.6, §R16). The ring survives a
    /// greyscale print and a color vision deficiency both, so the two kinds are two
    /// different objects before anybody has to tell green from orange.
    /// </remarks>
    [Fact]
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
