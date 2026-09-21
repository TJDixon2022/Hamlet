using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 378 task 3: **the hover is facts and never the text, Olivia gets it by
/// construction, and the send line names the mode he chose** (step 7, criteria 7.3, 7.4, 7.5).
/// </summary>
/// <remarks>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004). The one send driven here goes
/// over `FakePort` and `FakeSink`.</para>
/// </remarks>
public sealed class TheRowHoverSaysWhatItKnowsTests : IDisposable
{
    private const string His = "W1AW";

    private const string Mine = "KC3QIS";

    private const long DialOn20m = 14_071_500;

    /// <summary>A message from him carrying a grid, so the grid and distance lines have a source.</summary>
    private const string HisReport =
        Mine + " de " + His + "  RST 599 599  Name Hiram  QTH Newington  Grid FN31 FN31  BTU "
        + Mine + " de " + His + " K\n";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each hover is printed whole.</param>
    public TheRowHoverSaysWhatItKnowsTests(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-hover-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the temporary folder.</summary>
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

    /// <summary>**7.3: the hover says what the row knows, in R39's order, and never the text.**</summary>
    [Fact]
    public void TheHoverSaysTheFactsInTimsOrderAndNeverTheText()
    {
        var model = Panel("PSK31");

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10, HisReport) });

        var row = model.DigitalDecodes.Single(r => r.IsTextOnly);
        var hover = row.RowFacts;

        _output.WriteLine("---- the hover ----");
        _output.WriteLine(hover);
        _output.WriteLine("-------------------");

        var lines = hover.Split('\n');

        // **R39's ORDER IS THE ORDER**: station, country, grid and distance, offset and
        // strength, when he started, whether he spoke to Tim, the parser's kind and certainty,
        // and last what a click and a right-click do.
        Assert.Equal(His, lines[0]);
        Assert.Equal("United States of America", lines[1]);
        Assert.StartsWith("Grid FN31 · ", lines[2], StringComparison.Ordinal);
        Assert.Equal("1000 Hz · +10 dB", lines[3]);
        Assert.StartsWith("Heard since ", lines[4], StringComparison.Ordinal);
        Assert.EndsWith(" UTC", lines[4], StringComparison.Ordinal);
        Assert.Equal("This one is addressed to you.", lines[5]);
        Assert.Equal("He sent a signal report, read for certain", lines[6]);
        Assert.Equal(
            "Click the message to read the whole of it. Right-click for the lines you can send.",
            lines[^1]);

        // **THE DISTANCE IS COMPUTED FROM TWO GRIDS AND IS A REAL READING**, not a placeholder.
        Assert.Contains(" miles ", lines[2], StringComparison.Ordinal);

        // **THE TEXT IS NOWHERE IN IT, IN ANY FORM** (7.3, R39) - not the message, not the whole
        // message, not a truncation of one. This is the whole of what R39 asked for.
        foreach (var word in new[] { "RST", "599", "Hiram", "Newington", "BTU", " de " })
        {
            Assert.DoesNotContain(word, hover, StringComparison.Ordinal);
        }

        Assert.DoesNotContain(row.Message.Trim(), hover, StringComparison.Ordinal);
        Assert.DoesNotContain(row.WholeMessage, hover, StringComparison.Ordinal);

        // **AND THE WORDS ARE STILL ONE CLICK AWAY** (§0.5): hiding detail, not information.
        Assert.True(row.HasWholeMessage);

        row.OpenTheWholeMessageCommand.Execute(null);

        Assert.True(row.WholeMessageIsOpen);
        Assert.Contains("RST 599 599", row.WholeMessage, StringComparison.Ordinal);

        // **THE ROW-WIDE HOVER IS THE FACTS**, which is what the pointer finds.
        Assert.Equal(hover, row.RowHover);
    }

    /// <summary>**7.3: a fact Hamlet does not have is absent - not unknown, not a dash, not empty.**</summary>
    [Fact]
    public void AFactHamletDoesNotHaveIsAbsentRatherThanUnknown()
    {
        var model = Panel("PSK31");

        // A CQ with no grid in it, on a station nobody has worked, whose carrier is still up.
        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, 1400, double.NaN, "CQ CQ CQ de " + His + " " + His + " pse K\n"),
        });

        var row = model.DigitalDecodes.Single(r => r.IsTextOnly);
        var hover = row.RowFacts;

        _output.WriteLine("---- the hover ----");
        _output.WriteLine(hover);
        _output.WriteLine("-------------------");

        // **NOT DRAWN AT ALL** - and the words a placeholder would have used are nowhere in it.
        foreach (var absent in new[] { "Grid", "miles", "His carrier went", "unknown", "Unknown", "—", "n/a" })
        {
            Assert.DoesNotContain(absent, hover, StringComparison.Ordinal);
        }

        // **NOTHING IS DRAWN EMPTY EITHER**: no line is blank and none ends on a bare label.
        foreach (var line in hover.Split('\n'))
        {
            Assert.NotEqual("", line.Trim());
            Assert.DoesNotContain(" · \n", line, StringComparison.Ordinal);
            Assert.False(line.TrimEnd().EndsWith(":", StringComparison.Ordinal));
            Assert.False(line.TrimEnd().EndsWith("·", StringComparison.Ordinal));
        }

        // **A STRENGTH NOBODY MEASURED DRAWS NO DECIBELS**, which is the SNR cell's own rule.
        Assert.Equal("1400 Hz", hover.Split('\n')[2]);
        Assert.DoesNotContain("dB", hover, StringComparison.Ordinal);

        // The facts it DOES have are there.
        Assert.Equal(His, hover.Split('\n')[0]);
        Assert.Contains("He is calling CQ, read for certain", hover, StringComparison.Ordinal);
    }

    /// <summary>**7.3: the moment his carrier stopped is said once it has stopped.**</summary>
    [Fact]
    public void WhenHisCarrierWentIsSaidOnceItHasGone()
    {
        var model = Panel("PSK31");

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10, HisReport) });

        var row = model.DigitalDecodes.Single(r => r.IsTextOnly);

        Assert.DoesNotContain("carrier went", row.RowFacts, StringComparison.Ordinal);

        // The carrier goes: the search stops listing it, and the panel retires the row.
        model.ShowPsk31ChannelsForTests(Array.Empty<Psk31Channel>());

        var ended = model.DigitalDecodes.Single(r => r.IsTextOnly);

        _output.WriteLine("---- the hover after it ended ----");
        _output.WriteLine(ended.RowFacts);
        _output.WriteLine("----------------------------------");

        Assert.True(ended.Ended);
        Assert.Contains("His carrier went at ", ended.RowFacts, StringComparison.Ordinal);
        Assert.Matches(@"His carrier went at \d\d:\d\d:\d\d UTC", ended.RowFacts);

        // **AND HE IS STILL HEARD SINCE WHEN HE STARTED**: both moments, not one replacing
        // the other.
        Assert.Contains("Heard since ", ended.RowFacts, StringComparison.Ordinal);
    }

    /// <summary>**7.4: an Olivia row reads the same, through the same code and not a second one.**</summary>
    /// <remarks>
    /// **THE SAME MESSAGE ON BOTH PANELS**, so what is compared is the hover and not the
    /// conversation. Olivia's row adds one line the PSK31 row cannot have - its variant, which
    /// is a fact about the signal - and everything else is identical string for string.
    /// </remarks>
    [Fact]
    public void AnOliviaRowReadsTheSameHoverAsAPsk31Row()
    {
        var psk31 = Panel("PSK31");

        psk31.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10, HisReport) });

        var olivia = Panel("Olivia");

        olivia.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "16/500", 1000, OliviaListener.FoundByRsid, 0, HisReport, 9, 0, false, 1000),
        });

        var one = psk31.DigitalDecodes.Single(r => r.IsTextOnly);
        var two = olivia.DigitalDecodes.Single(r => r.IsTextOnly);

        _output.WriteLine("---- PSK31 ----");
        _output.WriteLine(one.RowFacts);
        _output.WriteLine("---- Olivia ----");
        _output.WriteLine(two.RowFacts);

        var mine = one.RowFacts.Split('\n').ToList();
        var his = two.RowFacts.Split('\n').ToList();

        // **THE SIGNAL LINE IS THE ONE THAT DIFFERS, AND BOTH DIFFERENCES ARE HONEST ONES.**
        // The Olivia row adds its variant, which is a fact about the signal and belongs beside
        // the offset; and it carries NO STRENGTH, because nothing measures a signal-to-noise
        // ratio per Olivia channel today - so that line is ABSENT rather than drawn as a dash
        // or a zero (7.3, §0.0). That is a shortfall on the Olivia side named in unit 378's
        // report, not a second implementation of the hover.
        Assert.Equal("1000 Hz · +10 dB", mine[3]);
        Assert.Equal("1000 Hz · Olivia 16/500", his[3]);
        Assert.DoesNotContain("dB", two.RowFacts, StringComparison.Ordinal);

        mine[3] = his[3];

        Assert.Equal(mine.Count, his.Count);

        for (var at = 0; at < mine.Count; at++)
        {
            // The heard-since clock is read from the wall clock at the moment each panel first
            // saw the carrier, so it is compared by shape rather than by value.
            if (mine[at].StartsWith("Heard since", StringComparison.Ordinal))
            {
                Assert.StartsWith("Heard since", his[at], StringComparison.Ordinal);

                continue;
            }

            Assert.Equal(mine[at], his[at]);
        }

        // **AND THE MENU IS THE SAME LIST TOO** (7.4): the same labels in the same order, from
        // the same reader, with no second implementation written for Olivia.
        Assert.Equal(
            psk31.Psk31CannedMenuFor(one)!.Select(e => e.Label),
            olivia.Psk31CannedMenuFor(two)!.Select(e => e.Label));
    }

    /// <summary>**7.5: the chip he pressed is the chosen one, on all three modes.**</summary>
    /// <remarks>
    /// **MEASURED BEFORE IT WAS TOUCHED AND IT WAS ALREADY RIGHT** (work instruction 378
    /// section 6 ruling 2 item 5, R14: do not build what is there). Unit 378's task 1 trace
    /// read the strip under FT8, PSK31 and Olivia and found the chosen chip was the mode
    /// chosen and the only one chosen every time, because `DigitalModeChip.IsChosen` has
    /// compared case-insensitively against all five labels since unit 358. **So this half of
    /// the criterion is an assertion and not a repair**, and it is here so a later unit that
    /// folds the label back onto a family turns it red.
    /// </remarks>
    [Theory]
    [InlineData("FT8")]
    [InlineData("FT4")]
    [InlineData("PSK31")]
    [InlineData("Olivia")]
    public void TheChosenChipIsTheModeHePressedAndTheOnlyOne(string mode)
    {
        var model = Panel(mode);

        var chips = model.DigitalModeChips;
        var chosen = chips.Where(c => c.IsChosen).ToList();

        foreach (var chip in chips)
        {
            _output.WriteLine(
                mode + ": " + chip.Label.PadRight(7) + " chosen " + chip.IsChosen
                + ", lit " + chip.IsLit + ", plain " + chip.IsPlain);
        }

        Assert.Equal(mode, model.ChosenDigitalMode);
        Assert.Equal(mode, Assert.Single(chosen).Label);

        // **THE THREE APPEARANCES STAY EXCLUSIVE AND EXHAUSTIVE**, which is what keeps a
        // remembered press from being drawn as a reading of the radio (§0.0, HM-DEC-092).
        foreach (var chip in chips)
        {
            Assert.Equal(chip.IsChosen && !chip.IsLit, chip.IsChosenElsewhere);
            Assert.Equal(!chip.IsLit && !chip.IsChosen, chip.IsPlain);
        }
    }

    /// <summary>**7.5: a 29-second Olivia CQ reads *29 s of Olivia*, from the sentence itself.**</summary>
    /// <remarks>
    /// **READ OFF THE PANEL AND NOT OUT OF A FORMAT STRING** (the instruction, task 3). A real
    /// Olivia CQ is driven over `FakePort` and `FakeSink` and the sentence the Send area is left
    /// holding is what is asserted. On 2026-09-21 that sentence read *29 s of PSK31*.
    /// </remarks>
    [Fact]
    public void AnOliviaCqSaysItWasOliviaAndNotPsk31()
    {
        var line = WhatTheSendLineSaysAfterACq("Olivia");

        _output.WriteLine("Olivia: [" + line + "]");

        Assert.Contains(" s of Olivia.", line, StringComparison.Ordinal);
        Assert.DoesNotContain("PSK31", line, StringComparison.Ordinal);
        Assert.Matches(@" - \d+(\.\d)? s of Olivia\.$", line);

        // **AND PSK31 STILL SAYS PSK31**, so the repair is the mode being read rather than the
        // word being swapped.
        var psk31 = WhatTheSendLineSaysAfterACq("PSK31");

        _output.WriteLine("PSK31 : [" + psk31 + "]");

        Assert.Contains(" s of PSK31.", psk31, StringComparison.Ordinal);
        Assert.DoesNotContain("Olivia", psk31, StringComparison.Ordinal);
    }

    // -------------------------------------------------------------------------
    // The fixtures.
    // -------------------------------------------------------------------------

    private string WhatTheSendLineSaysAfterACq(string mode)
    {
        using var telemetry = new JsonlTelemetry(_folder, "378", _ => true);

        var model = Panel(mode, telemetry);

        model.TapForTests = new Hamlet.RadioEngine.Audio.AudioTap();

        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

        model.SendCallToAnyoneCommand.Execute(null);

        for (var tries = 0; tries < 2000 && model.HasSomethingToStop; tries++)
        {
            Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Equal(1, sink.TimesCalled);

        return model.DigitalSendLine;
    }

    private static MainWindowViewModel Panel(string mode, JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Pennsylvania";
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= DialOn20m && b.Band.HighHz >= DialOn20m);
        model.FrequencyHz = DialOn20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }
}
