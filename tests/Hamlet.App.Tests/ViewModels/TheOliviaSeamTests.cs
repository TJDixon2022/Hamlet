using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 358 task 2: **Olivia is a mode everywhere PSK31 is a mode.**
/// </summary>
/// <remarks>
/// <para>**STEP 0 OF THE OLIVIA PHASE IS THE SEAM AND NOTHING ELSE.** Nothing here decodes,
/// detects an RSID, transmits or logs a contact. What is asserted is that the application
/// knows the mode exists, goes to the cited calling spot when it is pressed, and says plainly
/// that it cannot read anything there yet - the shape of `ThePsk31SeamTests`, copied.</para>
/// <para>**EVERY FREQUENCY COMES OUT OF THE CITED FILE** (HM-DEC-054, `PHASE_PLAN.md` R29).
/// The tests ask <see cref="OliviaData"/> where the spot is and assert the radio was asked for
/// that, so a constant typed into the view model could not pass.</para>
/// <para>**COMPUTED, NOT SEEN.** These read view-model values, a fake radio and a telemetry
/// file. Nothing here looks at a pixel.</para>
/// </remarks>
public sealed class TheOliviaSeamTests : IDisposable
{
    private const string Mode = "Olivia";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheOliviaSeamTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-olivia-seam-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>**The mode is enumerated in the Digital family, and colored as text in its ink.**</summary>
    [Fact]
    public void TheModeIsEnumeratedInTheDigitalFamily()
    {
        _output.WriteLine("the strip carries: " + string.Join(", ", DigitalModeChip.Labels));

        Assert.Contains("FT8", DigitalModeChip.Labels);
        Assert.Contains("PSK31", DigitalModeChip.Labels);
        Assert.Contains(Mode, DigitalModeChip.Labels);

        // **A SETTINGS FILE HOLDING `OLIVIA` IN ANY CASE ROUND-TRIPS.**
        Assert.Equal(Mode, DigitalModeChip.Canonical("OLIVIA"));
        Assert.Equal(Mode, DigitalModeChip.Canonical("olivia"));

        // **THE FAMILY IS THE GUIDE'S ANSWER, AND THE COLOR IS THE FAMILY'S INK** (§0.6).
        var family = ModeGuide.FamilyFor(Mode);

        _output.WriteLine("family        : " + family);

        Assert.Equal(ModeFamily.Digital, family);
        Assert.Same(ModePalette.Digital, ModePalette.For(family));

        // **AND PRESSING IT PICKS OUT ITS CHIP**, which a label compared in the wrong case
        // would never do.
        var model = Panel(null);

        model.ChooseDigitalModeCommand.Execute(Mode);

        var chip = Assert.Single(model.DigitalModeChips, c => c.Label == Mode);

        Assert.True(chip.IsChosen);
        Assert.Equal(Mode, model.ChosenDigitalMode);
    }

    /// <summary>**Selecting it asks the radio for the calling spot from the cited row, in USB-D.**</summary>
    [Fact]
    public async Task SelectingItAsksTheRadioForTheCallingSpotFromTheCitedRow()
    {
        var calling = OliviaData.Current.Calling;

        Assert.NotNull(calling);

        var bands = HfBands.Names.Where(b => calling!.CallingRowFor(b) is not null).ToList();

        _output.WriteLine("bands with a calling row: " + string.Join(", ", bands));

        Assert.NotEmpty(bands);

        foreach (var band in bands)
        {
            var row = calling!.CallingRowFor(band)!;
            var dial = calling.DialHzFor(row);

            Assert.NotNull(dial);

            var model = OnBand(band, out var rig);

            rig.ConfirmWhateverItIsGiven = true;

            await model.ChooseDigitalModeCommand.ExecuteAsync(Mode);

            _output.WriteLine(
                "  " + band.PadRight(6) + "center " + row.CenterHz + ", asked " + rig.LastSet
                + ", mode " + rig.LastMode + " data " + rig.LastData + " · " + model.DigitalTuneLine);

            // **THE ROW'S OWN DIAL**, not a number in this test.
            Assert.Equal(dial, rig.LastSet);
            Assert.Equal(1, rig.Sets);

            // **AND THE LINE NAMES THE CENTER THE ROW CITES**, which is the number the
            // operator will find on the community's own page.
            Assert.Contains(Megahertz(row.CenterHz), model.DigitalTuneLine, StringComparison.Ordinal);
            Assert.Contains(Mode, model.DigitalTuneLine, StringComparison.Ordinal);

            // **USB WITH THE DATA FLAG**, asked for by the press on every band, including the
            // three where the dial lands in no block mode-follow would set it for.
            Assert.Equal(1, rig.ModeSets);
            Assert.Equal(CivMode.Usb, rig.LastMode);
            Assert.True(rig.LastData);
        }

        // **ON 20 M THE DIAL IS THE ONE THE FILE PRINTS**, so the arithmetic is checked
        // against the file and not against itself.
        var twenty = calling!.CallingRowFor("20 m")!;

        Assert.NotNull(twenty.DialHz);
        Assert.Equal(twenty.DialHz, calling.DialHzFor(twenty));
    }

    /// <summary>**A calling table that cannot be read moves nothing, and the panel says why.**</summary>
    [Fact]
    public async Task AnUnreadableTableMovesNothingAndThePanelSaysWhy()
    {
        var root = RepoRoot();
        var rsid = File.ReadAllText(Path.Combine(root, "data", "rsid", "rsid-codes.json"));
        var broken = OliviaData.Read("{ \"rows\": [", rsid);

        Assert.NotNull(broken.Problem);

        var model = OnBand("20 m", out var rig);

        rig.ConfirmWhateverItIsGiven = true;
        model.UseOliviaDataForTests(broken);

        await model.ChooseDigitalModeCommand.ExecuteAsync(Mode);

        _output.WriteLine("tune line : " + model.DigitalTuneLine);
        _output.WriteLine("strip line: " + model.DigitalModeStripLine);

        Assert.Equal(0, rig.Sets);
        Assert.Equal(0, rig.ModeSets);
        Assert.True(model.DigitalTuneFailed);
        Assert.Contains("has not moved", model.DigitalTuneLine, StringComparison.Ordinal);
        Assert.Contains(OliviaCallingTable.FilePath, model.DigitalTuneLine, StringComparison.Ordinal);
        Assert.Contains(broken.Problem!, model.DigitalModeStripLine, StringComparison.Ordinal);
    }

    /// <summary>**The panel names the mode and promises no slot.**</summary>
    /// <remarks>
    /// <para>**REWRITTEN BY WORK INSTRUCTION 364 UNDER PSK31 PLAN §R12 (decision AM).** It asserted
    /// the sentence *Hamlet cannot read Olivia yet*, which guarded a shut door: from unit 364
    /// Olivia is read and drawn as rows, and the sentence is false.</para>
    /// <para>**AND REWRITTEN AGAIN BY WORK INSTRUCTION 366 UNDER §R12 (decision BI).** It then
    /// asserted *nothing on those lines can be answered yet* and
    /// <c>CanAnswerRowsForTests</c> false, which guarded the mode gate while it was shut. **This
    /// unit opened it**, so both were guarding a door that is now open and both are gone; what is
    /// left is what still holds and what criterion 0.5 was always about - the tab names its mode,
    /// and nothing on a panel for a mode with no slots may promise one. **The door itself is
    /// guarded by `TheOliviaSendTests`**, which drives a press all the way to the sound card.</para>
    /// </remarks>
    [Fact]
    public void ThePanelNamesTheModeAndPromisesNoSlot()
    {
        var model = Panel(null);

        model.ChooseDigitalModeCommand.Execute(Mode);

        // **A SPECTRUM, SO THE HEADER HAS SOMETHING TO DESCRIBE.** Without one it reads
        // *not listening yet* and the slot assertion below would pass for the wrong reason.
        model.DigitalSpectrum = new AudioSpectrumSource(8000);

        var bound = new (string Name, string Text)[]
        {
            ("mode strip", model.DigitalModeStripLine),
            ("decoded idle", model.DigitalDecodedIdle),
            ("waterfall header", model.DigitalWaterfallSummary),
        };

        foreach (var (name, text) in bound)
        {
            _output.WriteLine(name.PadRight(18) + ": " + text);
        }

        Assert.Contains(Mode, model.DigitalModeStripLine, StringComparison.Ordinal);
        Assert.Contains(Mode, model.DigitalDecodedIdle, StringComparison.Ordinal);

        // **OLIVIA HAS NO SLOTS**, so nothing bound on its panel may promise one.
        foreach (var (_, text) in bound)
        {
            Assert.DoesNotContain("slot", text, StringComparison.OrdinalIgnoreCase);
        }

        // **AND THE STRIP SAYS WHAT A LINE ACTUALLY OFFERS** (§0.0). Until this unit it said
        // nothing on a line could be answered, which the mode gate made true; the gate is open, so
        // the sentence that stands has to be one that is.
        Assert.Contains("Answer", model.DigitalModeStripLine, StringComparison.Ordinal);
    }

    /// <summary>**The log offers `OLIVIA`.**</summary>
    /// <remarks>
    /// **THE SUBMODE IS STEP 5'S, AND SO ARE THE ACHIEVEMENTS.** The entry names the mode
    /// and carries no submode, and it is not one of the six the achievements count, which
    /// is the table step 5 extends.
    /// </remarks>
    [Fact]
    public void TheLogOffersOlivia()
    {
        var named = ContactModes.Named(Mode);

        Assert.NotNull(named);

        _output.WriteLine("log entry     : " + named!.Name + " -> " + named.AdifSpelling);

        Assert.Contains(named, ContactModes.Logged);
        Assert.True(named.IsContactMode);
        Assert.Equal("MODE=OLIVIA", named.AdifSpelling);
        Assert.Null(named.AdifSubmode);

        Assert.DoesNotContain(ContactModes.Six, m => m.Name == Mode);
    }

    /// <summary>**The record says Olivia, and nothing personal is in it.**</summary>
    [Fact]
    public void TheRecordSaysOliviaAndNothingPersonalIsInIt()
    {
        const string call = "KC3QIS";
        const string grid = "FN00DJ";
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

        var lines = Lines(_folder);

        foreach (var line in lines)
        {
            _output.WriteLine(line);
        }

        Assert.NotEmpty(lines);
        Assert.Contains(lines, l => l.Contains("\"mode\":\"" + Mode + "\"", StringComparison.Ordinal));

        foreach (var personal in new[] { call, grid, who, where })
        {
            Assert.All(lines, l => Assert.DoesNotContain(
                personal, l, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>**No other mode's decoder is attached, and nothing is composed until a press.**</summary>
    /// <remarks>
    /// <para>**§0.2, AND THE PHASE PLAN'S CRITERION 0.5.** Under Olivia the FT8 slot watch is not
    /// asked and the PSK31 listener does not start.</para>
    /// <para>**REWRITTEN BY WORK INSTRUCTION 366 UNDER §R12 (decision BI).** It pressed CQ and
    /// asserted the door refused it for being this mode, with `send_refused` the last word in the
    /// record. That guarded the shut gate, which this unit opened. **What still holds, and is what
    /// §0.2 is actually about, is that nothing is composed, armed or keyed until the operator
    /// presses something** - so the press is gone and the sweep now covers the whole run of ticks
    /// before one. A press that goes out is `TheOliviaSendTests`' business.</para>
    /// </remarks>
    [Fact]
    public void NoDecoderIsAttachedAndNothingIsComposedUntilAPress()
    {
        var keying = Path.Combine(_folder, "keying");

        Directory.CreateDirectory(keying);

        using (var telemetry = new JsonlTelemetry(keying, "keying", _ => true))
        {
            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";
            settings.Operator.GridSquare = "FN00DJ";

            var model = Panel(telemetry, settings);

            model.TapForTests = Heard();
            model.ClockOffset = new ClockOffset(0.033, DateTime.UtcNow);

            model.ChooseDigitalModeCommand.Execute(Mode);

            for (var i = 0; i < 30; i++)
            {
                model.LookForASlotForTests();
            }

            _output.WriteLine("slot looks    : " + model.SlotLooksForTests);
            _output.WriteLine("slots read    : " + model.SlotsReadForTests);
            _output.WriteLine("rows          : " + model.DigitalDecodes.Count);

            Assert.Equal(0, model.SlotLooksForTests);
            Assert.Equal(0, model.SlotsReadForTests);
            Assert.Empty(model.DigitalDecodes);
            Assert.Empty(model.DigitalCards);

            // **AND NOTHING WAS PRESSED**, which is the whole point of the sweep below: thirty
            // ticks of audio went through the tab and not one of them composed, armed or keyed
            // anything. One operator action is what makes a transmission, and there was none.
            _output.WriteLine("send line     : \"" + model.DigitalSendLine + "\"");

            Assert.Equal("nothing sent yet", model.DigitalSendLine);
        }

        var lines = Lines(keying);

        foreach (var line in lines)
        {
            _output.WriteLine(line);
        }

        Assert.NotEmpty(lines);

        // **NO OTHER MODE'S LISTENER STARTED**, which is where a PSK31 path left running
        // under this tab would show. **Rewritten by work instruction 364 under §R12 (decision AM)**:
        // it asserted no `psk31_` line at all, and from unit 364 the Olivia rows write the PSK31
        // row events with `mode: olivia` (decision AK). A line with no such mode is PSK31's own.
        Assert.All(
            lines.Where(l => l.Contains("\"event\":\"psk31_", StringComparison.Ordinal)),
            l => Assert.Contains("\"mode\":\"olivia\"", l, StringComparison.Ordinal));

        // **NOTHING WAS PRESSED, SO THE TRANSMIT CATEGORY IS EMPTY.** Every line that reaches it
        // comes from a click - the press, the request, the refusal, the stages, the record - and
        // thirty ticks of audio produce none of them. **This is the half that outlives the gate**:
        // the door being open makes a press go out, and it does not make a decode into one.
        var transmitEvents = lines
            .Select(l => System.Text.Json.JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("category").GetString() == "transmit")
            .ToList();

        foreach (var e in transmitEvents)
        {
            _output.WriteLine("transmit category: " + e.GetRawText());
        }

        Assert.Empty(transmitEvents);

        foreach (var word in new[] { "transmission", "ptt", "keyed", "composed", "send_stage", "armed" })
        {
            Assert.All(lines, l => Assert.DoesNotContain(word, l, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>**The map picks out the Olivia spot when the tab is selected.**</summary>
    /// <remarks>
    /// <para>**TASK 4, AND IT IS THE STEP'S NICE-TO-PASS.** PSK31 is picked out by outlining
    /// its block, because the cited band data has a PSK31 block. **There is no Olivia block**:
    /// its calling spots sit inside other modes' blocks, or between them, so the same outline
    /// would pick out nothing. What is picked out is the spot itself, at the cited center.</para>
    /// <para>**COMPUTED, NOT SEEN.** What is asserted is the value the map is handed and the
    /// rule it draws by. Nothing here looks at a pixel.</para>
    /// </remarks>
    [Fact]
    public void TheMapPicksOutTheOliviaSpotWhenTheTabIsSelected()
    {
        var calling = OliviaData.Current.Calling;

        Assert.NotNull(calling);

        var model = Panel(null);
        var twenty = model.Bands.First(b => b.Band.Name == "20 m");

        model.SelectedBand = twenty;

        // **NOTHING CHOSEN PICKS OUT NOTHING.**
        Assert.Null(model.MapChosenSpotHz);

        model.ChooseDigitalModeCommand.Execute(Mode);

        var row = calling!.CallingRowFor("20 m")!;

        _output.WriteLine(
            "20 m spot     : " + model.MapChosenSpotHz + " on a map of " + model.MapLowHz
            + " to " + model.MapHighHz);

        Assert.Equal(row.CenterHz, model.MapChosenSpotHz);
        Assert.True(NeighborhoodMapControl.IsSpotOnMap(model.MapChosenSpotHz, model.MapLowHz, model.MapHighHz));

        // **NO BLOCK IS OUTLINED**, because none is Olivia's, so the spot is the only thing
        // picked out and nothing else is claimed.
        Assert.DoesNotContain(
            model.Neighborhoods, h => NeighborhoodMapControl.IsChosen(h, model.ChosenDigitalMode));

        // **EVERY BAND WITH A ROW, FROM ITS OWN ROW.**
        foreach (var band in HfBands.Names.Where(b => calling.CallingRowFor(b) is not null))
        {
            model.SelectedBand = model.Bands.First(b => b.Band.Name == band);

            _output.WriteLine("  " + band.PadRight(6) + model.MapChosenSpotHz);

            Assert.Equal(calling.CallingRowFor(band)!.CenterHz, model.MapChosenSpotHz);
            Assert.True(NeighborhoodMapControl.IsSpotOnMap(model.MapChosenSpotHz, model.MapLowHz, model.MapHighHz));
        }

        // **PSK31 PICKS OUT ITS BLOCK AND NO SPOT**, so the two mechanisms never draw at once.
        model.SelectedBand = twenty;
        model.ChooseDigitalModeCommand.Execute("PSK31");

        Assert.Null(model.MapChosenSpotHz);
        Assert.Single(model.Neighborhoods, h => NeighborhoodMapControl.IsChosen(h, model.ChosenDigitalMode));

        // **A TABLE THAT COULD NOT BE READ PICKS OUT NOTHING**, rather than a remembered spot.
        model.ChooseDigitalModeCommand.Execute(Mode);
        model.UseOliviaDataForTests(OliviaData.Read(null, null));

        Assert.Null(model.MapChosenSpotHz);
        Assert.False(NeighborhoodMapControl.IsSpotOnMap(model.MapChosenSpotHz, model.MapLowHz, model.MapHighHz));
    }

    /// <summary>**Under Olivia, an RSID burst through the tap writes `rsid_heard`, and nothing else moves.**</summary>
    /// <param name="deviceRate">The rate the tap hands over: the fixture's own, and a sound card's.</param>
    /// <remarks>
    /// <para>**WORK INSTRUCTION 359 TASK 5, CRITERION 1.6.** The mode author's 8/250 CQ, with
    /// fldigi's own burst in front, is handed to the real tick through the tap the way the sound
    /// card hands it over. The line says the code, the mode, the variant, the center and the
    /// quality, and nothing personal.</para>
    /// <para>**A DETECTION CHANGES NOTHING ELSE** (the arbiter's decision B, §0.2). The radio is
    /// asked for nothing more, the dial, the mode and the tab stay where they were, no row but an
    /// Olivia row and no card appears (since unit 364, which draws the station it announced), and
    /// nothing is composed or sent.</para>
    /// </remarks>
    [Theory]
    [InlineData(8_000)]
    [InlineData(48_000)]
    public async Task UnderOliviaABurstThroughTheTapWritesRsidHeardAndMovesNothing(int deviceRate)
    {
        var folder = Path.Combine(_folder, "heard-" + deviceRate);

        Directory.CreateDirectory(folder);

        const string call = "KC3QIS";
        const string grid = "FN00DJ";
        const string who = "Quillfeather";
        const string where = "Trafford PA";

        using (var telemetry = new JsonlTelemetry(folder, "rsid", _ => true))
        {
            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = call;
            settings.Operator.GridSquare = grid;
            settings.Operator.OperatorName = who;
            settings.Operator.Location = where;

            var model = Panel(telemetry, settings);

            model.SelectedBand = model.Bands.First(b => b.Band.Name == "20 m");
            model.FrequencyHz = model.SelectedBand.Band.JumpHz;

            var rig = new ModeFakeRig { ConfirmWhateverItIsGiven = true };

            model.UseRigForTests(rig);

            await model.ChooseDigitalModeCommand.ExecuteAsync(Mode);

            var dial = rig.LastSet;
            var sets = rig.Sets;
            var modeSets = rig.ModeSets;
            var frequency = model.FrequencyHz;

            var tap = new AudioTap();

            model.TapForTests = tap;

            Feed(model, tap, Fixture("olivia-8-250-cq-rsid.wav"), deviceRate);

            _output.WriteLine(
                $"at {deviceRate} Hz: radio asked {rig.Sets} time(s), mode {rig.ModeSets}, dial {rig.LastSet}, "
                + $"frequency {model.FrequencyHz}, tab {model.ChosenDigitalMode}, rows {model.DigitalDecodes.Count}, cards {model.DigitalCards.Count}");

            Assert.Equal(sets, rig.Sets);
            Assert.Equal(modeSets, rig.ModeSets);
            Assert.Equal(dial, rig.LastSet);
            Assert.Equal(frequency, model.FrequencyHz);
            Assert.Equal(Mode, model.ChosenDigitalMode);
            Assert.Equal("Digital", model.OperatingMode);

            // **REWRITTEN BY WORK INSTRUCTION 364 UNDER §R12 (decision AM).** It asserted no row at
            // all, which guarded the shut door: from unit 364 the station is read and drawn. What
            // 0.5 guards is that nothing but Olivia's own listener draws one and nothing opens a
            // card to answer, so every row is an Olivia row and no card is up.
            Assert.All(model.DigitalDecodes, r => Assert.True(r.HasVariant));
            Assert.Empty(model.DigitalCards);
        }

        var lines = Lines(folder);

        foreach (var line in lines.Where(l => l.Contains("rsid", StringComparison.OrdinalIgnoreCase)))
        {
            _output.WriteLine(line);
        }

        var heard = Assert.Single(lines, l => l.Contains("\"event\":\"rsid_heard\"", StringComparison.Ordinal));
        var data = System.Text.Json.JsonDocument.Parse(heard).RootElement.GetProperty("data");
        var codes = OliviaData.Current.Rsid!;

        Assert.Equal(codes.CodeOf("OLIVIA_8_250"), data.GetProperty("code").GetInt32());
        Assert.Equal("OLIVIA", data.GetProperty("mode").GetString());
        Assert.Equal("8/250", data.GetProperty("variant").GetString());
        Assert.InRange(data.GetProperty("centerHz").GetDouble(), 995, 1005);
        Assert.True(data.GetProperty("quality").GetDouble() > 0);

        Assert.Subset(
            new HashSet<string> { "code", "mode", "variant", "centerHz", "quality", "tonesRight" },
            data.EnumerateObject().Select(p => p.Name).ToHashSet());

        foreach (var personal in new[] { call, grid, who, where })
        {
            Assert.DoesNotContain(personal, heard, StringComparison.OrdinalIgnoreCase);
        }

        // **AND NOTHING WAS COMPOSED OR SENT.**
        foreach (var name in new[] { "rsid_sent", "psk31_send_composed", "send_stage", "ft8_transmission" })
        {
            Assert.DoesNotContain(lines, l => l.Contains("\"event\":\"" + name + "\"", StringComparison.Ordinal));
        }
    }

    /// <summary>**Under FT8 the same audio writes no RSID event**: listening for RSID is Olivia's in this step.</summary>
    [Fact]
    public void UnderFt8TheSameAudioWritesNoRsidEvent()
    {
        var folder = Path.Combine(_folder, "ft8");

        Directory.CreateDirectory(folder);

        using (var telemetry = new JsonlTelemetry(folder, "rsid", _ => true))
        {
            var model = Panel(telemetry);

            model.ChooseDigitalModeCommand.Execute("FT8");

            var tap = new AudioTap();

            model.TapForTests = tap;

            Feed(model, tap, Fixture("olivia-8-250-cq-rsid.wav"), 8_000);

            _output.WriteLine("tab: " + model.ChosenDigitalMode);
        }

        var lines = Lines(folder);

        _output.WriteLine("lines written: " + lines.Count);

        Assert.DoesNotContain(lines, l => l.Contains("\"event\":\"rsid_", StringComparison.Ordinal));
    }

    /// <summary>Hand a recording to the tick in quarter-second pieces, as a sound card would.</summary>
    /// <remarks>
    /// At a device rate above the recording's, each sample is held for the whole ratio, which puts
    /// the audio at the device's rate for the resampler to bring back down.
    /// </remarks>
    private static void Feed(MainWindowViewModel model, AudioTap tap, MonoAudio audio, int deviceRate)
    {
        var hold = deviceRate / audio.SampleRate;
        var piece = new float[deviceRate / 4];
        var filled = 0;

        foreach (var sample in audio.Samples)
        {
            for (var i = 0; i < hold; i++)
            {
                piece[filled++] = sample;

                if (filled == piece.Length)
                {
                    tap.Take(piece, deviceRate);
                    model.LookForASlotForTests();
                    filled = 0;
                }
            }
        }

        if (filled > 0)
        {
            tap.Take(piece.AsSpan(0, filled), deviceRate);
            model.LookForASlotForTests();
        }
    }

    /// <summary>A fixture from the mode author's set, after its hash has been checked against the manifest.</summary>
    private static MonoAudio Fixture(string file)
    {
        var folder = Path.Combine(RepoRoot(), "assets", "fixtures", "olivia");

        using var manifest = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));

        var entry = manifest.RootElement.EnumerateArray().Single(e => e.GetProperty("file").GetString() == file);
        var path = Path.Combine(folder, file);

        Assert.Equal(
            entry.GetProperty("sha256").GetString(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant());

        return WavAudio.Read(path);
    }

    private static AudioTap Heard()
    {
        var tap = new AudioTap();

        tap.Take(new float[12_000], 12_000);

        return tap;
    }

    private static List<string> Lines(string folder)
        => Directory.GetFiles(folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

    private static string Megahertz(long hz)
        => (hz / 1_000_000.0).ToString("0.000000", CultureInfo.InvariantCulture);

    private static MainWindowViewModel Panel(JsonlTelemetry? telemetry, AppSettings? given = null)
    {
        var settings = given ?? new AppSettings { ReconnectOnStartup = false };

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }

    private static MainWindowViewModel OnBand(string band, out ModeFakeRig rig)
    {
        var model = new MainWindowViewModel(new AppSettings { ReconnectOnStartup = false }, null)
        {
            OperatingMode = "Digital",
        };

        model.SelectedBand = model.Bands.First(b => b.Band.Name == band);
        model.FrequencyHz = model.SelectedBand.Band.JumpHz;

        rig = new ModeFakeRig();
        model.UseRigForTests(rig);

        return model;
    }

    private static string RepoRoot()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    /// <summary>A radio that confirms frequencies and records the mode it was asked for.</summary>
    private sealed class ModeFakeRig : IRig
    {
        public bool ConfirmWhateverItIsGiven { get; set; }

        public long LastSet { get; private set; }

        public int Sets { get; private set; }

        public int ModeSets { get; private set; }

        public CivMode? LastMode { get; private set; }

        public bool LastData { get; private set; }

        public bool IsConnected => true;

        public bool IsSimulated => false;

        public RigCapabilities Capabilities { get; } = new(
            "fake CI-V", false, false, false, false, HfBands.Names);

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ConfirmWhateverItIsGiven ? LastSet : 0L);

        public Task SetFrequencyHzAsync(long frequencyHz, CancellationToken cancellationToken = default)
        {
            Sets++;
            LastSet = frequencyHz;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RigValue>>(
                new[] { RigValue.Unknown(field, "the fake radio answers nothing else") });

        public Task<RigWriteResult> SetModeAsync(
            CivMode mode, bool dataMode, byte? filterSlot = null,
            CancellationToken cancellationToken = default)
        {
            ModeSets++;
            LastMode = mode;
            LastData = dataMode;

            return Task.FromResult(RigWriteResult.NotSupported("the fake radio records modes and sets none"));
        }

        public Task<RigWriteResult> SetSettingAsync(
            CivWrite write, int value, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("the fake radio does not do settings"));

        public Task<bool> SendCwAsync(string message, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("nothing in unit 358 keys a transmitter (CLAUDE.md 0.2)");

        public void AbortCw()
        {
        }

        internal void NobodyRaisesThese()
        {
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(0));
            ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(Array.Empty<RigValue>()));
        }
    }
}
