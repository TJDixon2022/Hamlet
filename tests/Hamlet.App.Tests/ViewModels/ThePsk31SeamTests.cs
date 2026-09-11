using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 312 task 2: **PSK31 is a mode everywhere FT4 is a mode.**
/// </summary>
/// <remarks>
/// <para>**STEP 0 OF THE PSK31 PHASE IS THE SEAM AND NOTHING ELSE.** Nothing here
/// decodes, transmits, parses or logs a contact. What is asserted is that the
/// application knows the mode exists, goes to the right place when it is pressed, and
/// says plainly that it cannot yet read anything there.</para>
/// <para>**THE GAP THIS UNIT FOUND** (task 1): PSK31 was already a chip on the strip,
/// already had the cited 14.070 row, already wrote USB-D through `ModeFollowPlan`, and
/// was already in `ContactModes.Six`. What it did not have was a truthful panel -
/// `DigitalModeFor` mapped it onto `DigitalMode.Ft8`, so pressing PSK31 tuned to
/// 14.070 and then ran FT8's slot grid and FT8's decoder on it, under a line reading
/// *slots here run 15 seconds* on a mode that has no slots at all (§0.0,
/// HM-DEC-092).</para>
/// <para>**COMPUTED, NOT SEEN.** These read view-model values, a lookup and a
/// telemetry file. Nothing here looks at a pixel.</para>
/// </remarks>
public sealed class ThePsk31SeamTests : IDisposable
{
    private const string Mode = "PSK31";
    private const string Band = "20 m";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public ThePsk31SeamTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-seam-" + Guid.NewGuid().ToString("N"));

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
            // **A LEFT-OVER TEMP FOLDER IS NOT A TEST FAILURE**, and §8's
            // never-throw discipline applies to a test's own housekeeping too.
        }
    }

    /// <summary>**The mode is enumerated beside FT8 and FT4, in the Digital family.**</summary>
    [Fact]
    public void TheModeIsEnumeratedBesideFt8AndFt4InTheDigitalFamily()
    {
        _output.WriteLine(
            "the strip carries: " + string.Join(", ", DigitalModeChip.Labels));

        Assert.Contains("FT8", DigitalModeChip.Labels);
        Assert.Contains("FT4", DigitalModeChip.Labels);
        Assert.Contains(Mode, DigitalModeChip.Labels);

        // **AND THE CANONICAL FORM ROUND-TRIPS**, which is what a settings file
        // holding `psk31` in lower case depends on.
        Assert.Equal(Mode, DigitalModeChip.Canonical("psk31"));

        // **THE FAMILY IS THE MAP'S ANSWER AND NOT A LITERAL HERE** (HM-DEC-032,
        // HM-DEC-054). The cited row says which family the block belongs to.
        var row = DigitalCallingFrequencies.Find(Band, Mode);

        Assert.NotNull(row);
        Assert.Equal(ModeFamily.Digital, row!.Family);

        _output.WriteLine(
            "the cited row : " + row.Name + " · family " + row.Family
            + " · cite " + row.Cite);
    }

    /// <summary>**Pressing it goes to the cited 14.070 block, in USB-D.**</summary>
    /// <remarks>
    /// **THE SOURCE IS ASSERTED AND NOT ONLY THE NUMBER** (the instruction). A test
    /// that checked for 14070000 would pass just as well against a constant typed into
    /// the view model, which is the thing §0 forbids and the thing the band map exists
    /// to prevent.
    /// </remarks>
    [Fact]
    public void PressingItGoesToTheCitedBlockInUsbD()
    {
        var row = DigitalCallingFrequencies.Find(Band, Mode);

        Assert.NotNull(row);

        _output.WriteLine(
            "cited row     : " + row!.LowHz + " to " + row.HighHz
            + ", jump " + row.JumpHz + ", cite " + row.Cite);

        // **THE ROW CARRIES ITS SOURCE.** A row with no citation is a number
        // somebody typed, and this map exists so that no such number is acted on.
        Assert.False(string.IsNullOrWhiteSpace(row.Cite));

        // **14.070 IS THE PHASE'S OWN FREQUENCY AND IT IS THE ROW THAT SAYS SO.**
        Assert.Equal(14_070_000, row.JumpHz);

        var model = Panel(null);

        // **THE BAND IS PICKED FIRST, AND FINDING THAT OUT WAS THE POINT.** The panel
        // opens on 40 m, so an earlier draft of this test asserted 14.070 and read
        // back 7.070 - which is the press behaving correctly, going to the PSK31 row
        // of the band the operator is actually on. **Every band with a row is the
        // real assertion**, and 20 m is the one this phase is about.
        var twenty = model.Bands.FirstOrDefault(b => b.Band.Name == Band);

        Assert.NotNull(twenty);

        model.SelectedBand = twenty!;

        model.ChooseDigitalModeCommand.Execute(Mode);

        _output.WriteLine("band          : " + model.SelectedBand.Band.Name);
        _output.WriteLine("chosen        : " + model.ChosenDigitalMode);
        _output.WriteLine("tune line     : " + model.DigitalTuneLine);

        Assert.Equal(Mode, model.ChosenDigitalMode);

        // **THE NUMBER ON THE LINE IS THE ROW'S OWN**, formatted the way the rig
        // display formats a frequency. With nothing connected the dial cannot move,
        // and what the line says is where it would go.
        var megahertz = (row.JumpHz / 1_000_000.0)
            .ToString("0.000000", CultureInfo.InvariantCulture);

        Assert.Contains(megahertz, model.DigitalTuneLine, StringComparison.Ordinal);

        // **AND IT IS THE ROW ON EVERY BAND THAT HAS ONE**, so nothing here can be
        // satisfied by a constant that happens to match one band.
        foreach (var band in DigitalCallingFrequencies.BandsWith(Mode))
        {
            var here = DigitalCallingFrequencies.Find(band, Mode);

            var picked = model.Bands.FirstOrDefault(b => b.Band.Name == band);

            if (here is null || picked is null)
            {
                continue;
            }

            model.SelectedBand = picked;
            model.ChooseDigitalModeCommand.Execute(Mode);

            var said = (here.JumpHz / 1_000_000.0)
                .ToString("0.000000", CultureInfo.InvariantCulture);

            _output.WriteLine(
                "  " + band.PadRight(6) + said + " · " + model.DigitalTuneLine);

            Assert.Contains(said, model.DigitalTuneLine, StringComparison.Ordinal);
        }

        // **AND THE MODE THAT BLOCK ASKS FOR IS USB WITH THE DATA FLAG** - which is
        // USB-D, and is `ModeFollowPlan`'s answer rather than a second opinion here.
        var target = ModeFollowPlan.TargetFor(row);

        Assert.NotNull(target);
        Assert.Equal(CivMode.Usb, target!.Mode);
        Assert.True(target.DataMode, "the block does not ask for the data variant");

        _output.WriteLine(
            "mode target   : " + target.Mode + ", data " + target.DataMode);
    }

    /// <summary>**The panel names the mode and says nothing decodes.**</summary>
    /// <remarks>
    /// **IT USED TO TALK ABOUT SLOTS.** `DigitalIdleText.ModeStripFor` names the slot
    /// length, which is the right sentence for FT8 and FT4 and a false one here: PSK31
    /// is a continuous carrier carrying free text and has no slots to wait for. A line
    /// telling the operator to give it a slot or two is telling him to wait for
    /// something that never arrives.
    /// </remarks>
    [Fact]
    public void ThePanelNamesTheModeAndSaysNothingDecodes()
    {
        var model = Panel(null);

        model.ChooseDigitalModeCommand.Execute(Mode);

        var line = model.DigitalModeStripLine;

        _output.WriteLine("strip line    : " + line);

        Assert.Contains(Mode, line, StringComparison.Ordinal);

        // **IT SAYS IT CANNOT READ THIS YET.**
        Assert.Contains("cannot", line, StringComparison.OrdinalIgnoreCase);

        // **AND IT SAYS NOTHING ABOUT SLOTS**, because this mode has none.
        foreach (var slotted in new[] { "slot", "seconds" })
        {
            Assert.DoesNotContain(slotted, line, StringComparison.OrdinalIgnoreCase);
        }

        // **FT8 IS UNTOUCHED**, which is the half a change like this most easily
        // breaks.
        var ft8 = Panel(null);

        ft8.ChooseDigitalModeCommand.Execute("FT8");

        _output.WriteLine("FT8 line      : " + ft8.DigitalModeStripLine);

        Assert.Contains(
            "slot", ft8.DigitalModeStripLine, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**The log offers the mode.**</summary>
    /// <remarks>
    /// **ALREADY TRUE SINCE UNIT 287, AND ASSERTED SO IT STAYS TRUE.** The spelling is
    /// read from the ADIF specification rather than recalled, and PSK31 is a submode of
    /// `PSK` - a record saying `MODE=PSK31` is not valid ADIF. **This unit adds no
    /// field and changes no spelling**; step 5 owns the RST field and the submode
    /// written into an export.
    /// </remarks>
    [Fact]
    public void TheLogOffersTheMode()
    {
        var named = ContactModes.Named(Mode);

        Assert.NotNull(named);

        _output.WriteLine(
            "log entry     : " + named!.Name + " -> " + named.AdifSpelling);

        Assert.Contains(Mode, ContactModes.Six.Select(m => m.Name));

        // **AND IT IS A MODE SOMEBODY CAN BE WORKED IN**, unlike WSPR, which is in
        // the same table and is not.
        Assert.True(named.IsContactMode);

        // **THE SPELLING IS ADIF'S AND NOT THE LABEL.** `MODE=PSK31` is not valid
        // ADIF, and a record that said it would light nothing in any logger.
        Assert.Equal("MODE=PSK, SUBMODE=PSK31", named.AdifSpelling);
    }

    /// <summary>**The record says PSK31, and nothing personal is in it.**</summary>
    [Fact]
    public void TheRecordSaysPsk31AndNothingPersonalIsInIt()
    {
        const string call = "KC3QIS";
        const string grid = "FN00DJ";
        // **THE NAME IS DELIBERATELY NOT HIS.** The first draft used `Tim` and the
        // scan failed on `spot_timer_changed`, which contains it and is not his name
        // at all - a substring sweep cannot tell a value from a coincidence inside a
        // token. The real profile walk is `CallsignPrivacyTests`; this one only has
        // to be able to say whether the press wrote a personal value, so it uses a
        // name no JSON key can contain.
        const string who = "Quillfeather";
        const string where = "Trafford PA";

        using (var telemetry = new JsonlTelemetry(_folder, "seam", _ => true))
        {
            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = call;
            settings.Operator.GridSquare = grid;
            settings.Operator.OperatorName = who;
            settings.Operator.Location = where;

            var model = Panel(telemetry, settings);

            model.ChooseDigitalModeCommand.Execute(Mode);
        }

        var lines = Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .ToList();

        foreach (var line in lines)
        {
            _output.WriteLine(line);
        }

        Assert.NotEmpty(lines);

        // **THE MODE FIELD READS PSK31 SOMEWHERE IN WHAT THE PRESS WROTE.**
        Assert.Contains(lines, l => l.Contains("\"" + Mode + "\"", StringComparison.Ordinal));

        // **AND NOTHING PERSONAL IS ANYWHERE IN IT** (§2.1).
        foreach (var personal in new[] { call, grid, who, where })
        {
            Assert.All(lines, l => Assert.DoesNotContain(
                personal, l, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>**No path from this tab reaches anything that keys the radio.**</summary>
    /// <remarks>
    /// **§0.2, AND IT IS THE ONE ASSERTION HERE THAT IS ABOUT SAFETY.** Pressing a mode
    /// chip changes a receive frequency. It must not arm a send, compose a
    /// transmission, or leave a send control offering one, and a mode with no modulator
    /// at all must not be able to offer a press that would look like it had one.
    /// </remarks>
    [Fact]
    public void NoPathFromThisTabReachesAnythingThatKeysTheRadio()
    {
        using (var telemetry = new JsonlTelemetry(_folder, "keying", _ => true))
        {
            var model = Panel(telemetry);

            model.ChooseDigitalModeCommand.Execute(Mode);

            _output.WriteLine("chosen        : " + model.ChosenDigitalMode);

            // **NO CARD APPEARED**, because a card is what carries a send button.
            Assert.Empty(model.DigitalCards);
        }

        var lines = Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .ToList();

        foreach (var line in lines)
        {
            _output.WriteLine(line);
        }

        // **NOTHING THE PRESS WROTE IS ABOUT KEYING.** A tune is a receive-frequency
        // change and is expected here; a transmission, an arming or a press of the
        // call button is not, and the record is where a path that reached one would
        // show up whether or not anybody thought to assert against it.
        foreach (var keying in new[]
        {
            "transmission", "cq_pressed", "send_", "ptt", "keyed", "transmit",
        })
        {
            Assert.All(lines, l => Assert.DoesNotContain(
                keying, l, StringComparison.OrdinalIgnoreCase));
        }
    }

    private MainWindowViewModel Panel(
        JsonlTelemetry? telemetry, AppSettings? given = null)
    {
        var settings = given ?? new AppSettings { ReconnectOnStartup = false };

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }
}
