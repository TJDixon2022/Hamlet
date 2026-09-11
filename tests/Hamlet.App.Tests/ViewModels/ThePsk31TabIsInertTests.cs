using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 314 task 1: **the PSK31 tab is inert.**
/// </summary>
/// <remarks>
/// <para>**THIS IS WHAT HAPPENED TO TIM** (2026-09-11): *"I clicked PSK31, saw the radio
/// move. Saw one CQ (in 10 minutes), did a respond, got nothing back."* There is no PSK31
/// decoder in this tree, so the CQ he saw was **FT8's decoder still running under the
/// PSK31 tab**, and his answer **composed an FT8 message and put it on 14.070** - the
/// PSK31 watering hole, where nobody is listening for FT8.</para>
/// <para>**UNIT 312 FIXED THE SENTENCE AND LEFT THE MACHINERY CONNECTED.** The panel said
/// Hamlet cannot read PSK31 while the slot watch cut FT8 slots and the FT8 decoder ran on
/// them. A tab that says one thing and does another is the fault §0.0 exists to stop, and
/// a tab that can transmit the wrong mode into the wrong segment is worse than a tab that
/// does nothing at all (§0.2).</para>
/// <para>**COMPUTED, NOT SEEN.** These drive the real slot tick and the real send door and
/// read back what they did. Nothing here looks at a pixel and nothing here is evidence
/// about the radio.</para>
/// </remarks>
public sealed class ThePsk31TabIsInertTests : IDisposable
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public ThePsk31TabIsInertTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-inert-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>**No decoder runs and no slot grid is cut under PSK31.**</summary>
    /// <remarks>
    /// **NOT HIDDEN - NOT RUNNING** (the instruction). The slot watch is what cuts audio
    /// into FT8 slots and hands them to the decoder; under PSK31 it must not be asked for
    /// a slot at all.
    /// </remarks>
    [Fact]
    public void NoDecoderRunsAndNoSlotGridIsCutUnderPsk31()
    {
        var model = Panel();

        model.ChooseDigitalModeCommand.Execute("PSK31");

        Assert.Equal("PSK31", model.ChosenDigitalMode);

        // **THIRTY LOOKS, WHICH IS SEVEN AND A HALF SECONDS OF TICKS.**
        for (var i = 0; i < 30; i++)
        {
            model.LookForASlotForTests();
        }

        _output.WriteLine("rows on the list : " + model.DigitalDecodes.Count);
        _output.WriteLine("slot looks       : " + model.SlotLooksForTests);
        _output.WriteLine("slots read       : " + model.SlotsReadForTests);
        _output.WriteLine("strip line       : " + model.DigitalModeStripLine);

        Assert.Empty(model.DigitalDecodes);
        Assert.Empty(model.DigitalVisibleDecodes);

        // **THE WATCH WAS NEVER ASKED**, which is *not running* rather than
        // running and finding nothing.
        Assert.Equal(0, model.SlotLooksForTests);
        Assert.Equal(0, model.SlotsReadForTests);
    }

    /// <summary>**FT8 is untouched: it still reaches the slot watch on the same audio.**</summary>
    /// <remarks>
    /// **THE HALF A CHANGE LIKE THIS MOST EASILY BREAKS.** Silencing a mode by silencing
    /// the tick would silence all four, and the two that work would go with it.
    /// </remarks>
    [Fact]
    public void Ft8AndFt4AreUntouched()
    {
        foreach (var mode in new[] { "FT8", "FT4" })
        {
            var model = Panel();

            model.ChooseDigitalModeCommand.Execute(mode);

            for (var i = 0; i < 30; i++)
            {
                model.LookForASlotForTests();
            }

            _output.WriteLine(mode + ": slot looks " + model.SlotLooksForTests
                + ", slots read " + model.SlotsReadForTests
                + ", strip line " + model.DigitalModeStripLine);

            // **THE WATCH IS ASKED ON EVERY TICK.** Thirty ticks inside a
            // millisecond close no slot on any mode - the boundary is fifteen
            // seconds away - so what is asserted is that the tick reaches the
            // watch at all, which is the thing PSK31 must not do and these two
            // must.
            Assert.Equal(30, model.SlotLooksForTests);
        }
    }

    /// <summary>**No row on the list is clickable, because there are no rows.**</summary>
    [Fact]
    public void NoRowOnTheListIsClickable()
    {
        var model = Panel();

        model.ChooseDigitalModeCommand.Execute("PSK31");

        // **A ROW PUT THERE BY HAND IS STILL NOT ANSWERABLE.** The list is empty under
        // PSK31 and this asserts the gate rather than the emptiness, so a later unit
        // that fills the list with PSK31 text does not quietly make every row a send.
        model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240", HisCall + " W1AW FN31",
            Slot("02:11:15"), heardOnHz: 14_070_000);

        var row = Assert.Single(model.DigitalDecodes);

        _output.WriteLine("row          : " + row.Message);
        _output.WriteLine("can be answered: " + model.CanAnswerRowsForTests);

        Assert.False(
            model.CanAnswerRowsForTests,
            "a row under PSK31 offers an answer, which would compose FT8");
    }

    /// <summary>**A CQ press under PSK31 reaches no send path, and says why.**</summary>
    /// <remarks>
    /// **THE ONE DOOR IS WHERE THE GUARD GOES** (§0.2). `SendMessage` is the only call
    /// site that composes a signal in the whole of `src/`, so a refusal there covers the
    /// CQ button, the right-click answer and anything a later unit adds.
    /// </remarks>
    [Fact]
    public void ACqPressUnderPsk31ReachesNoSendPathAndSaysWhy()
    {
        using (var telemetry = new JsonlTelemetry(_folder, "inert", _ => true))
        {
            var model = Panel(telemetry);

            model.ChooseDigitalModeCommand.Execute("PSK31");

            model.SendCallToAnyoneCommand.Execute(null);

            _output.WriteLine("send line : " + model.DigitalSendLine);

            // **IT SAYS WHY, IN WORDS** (§0.5.1, §0.7). A button that does nothing and
            // says nothing is indistinguishable from a broken one.
            Assert.Contains("PSK31", model.DigitalSendLine, StringComparison.Ordinal);

            Assert.True(
                model.DigitalSendLine.Length > 20,
                "the refusal does not explain itself: " + model.DigitalSendLine);
        }

        var lines = Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .ToList();

        foreach (var line in lines.Where(l => l.Contains("send", StringComparison.Ordinal)
            || l.Contains("transmission", StringComparison.Ordinal)))
        {
            _output.WriteLine(line);
        }

        // **THE PRESS IS RECORDED AND THE TRANSMISSION IS NOT.** A refusal is an
        // outcome and is as loggable as a success (§8.1); what must not be in the file
        // is any sign that a signal was composed or played.
        foreach (var keyed in new[] { "composed", "ft8_transmission", "Played", "keyed" })
        {
            Assert.All(lines, l => Assert.DoesNotContain(
                keyed, l, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>**FT8 can still send after all of it.**</summary>
    [Fact]
    public void Ft8CanStillReachTheSendDoor()
    {
        var model = Panel();

        model.ChooseDigitalModeCommand.Execute("FT8");

        model.SendCallToAnyoneCommand.Execute(null);

        _output.WriteLine("FT8 send line : " + model.DigitalSendLine);

        // **IT GETS PAST THE MODE GATE.** With no radio connected it stops later, and
        // where it stops is not this test's business - what matters is that it is not
        // stopped for being PSK31.
        Assert.DoesNotContain("PSK31", model.DigitalSendLine, StringComparison.Ordinal);
    }

    private MainWindowViewModel Panel(JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        // **A REAL TAP WITH REAL AUDIO IN IT**, because the slot look needs one and this
        // machine has no sound card.
        var tap = new AudioTap();

        tap.Take(new float[12_000], 12_000);

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = tap,
            ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-11 " + at, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
