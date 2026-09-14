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

    /// <summary>**The panel names the mode and says it cannot be read yet.**</summary>
    [Fact]
    public void ThePanelNamesTheModeAndSaysItCannotBeReadYet()
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
        Assert.Contains("cannot read " + Mode + " yet", model.DigitalModeStripLine, StringComparison.Ordinal);
        Assert.Contains(Mode, model.DigitalDecodedIdle, StringComparison.Ordinal);

        // **OLIVIA HAS NO SLOTS**, so nothing bound on its panel may promise one.
        foreach (var (_, text) in bound)
        {
            Assert.DoesNotContain("slot", text, StringComparison.OrdinalIgnoreCase);
        }

        // **AND NOTHING SAYS IT IS LISTENING**, because nothing is.
        Assert.DoesNotContain("listening for", model.DigitalModeStripLine, StringComparison.OrdinalIgnoreCase);
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

    /// <summary>**No decoder is attached, and no path from the tab reaches anything that keys.**</summary>
    /// <remarks>
    /// **§0.2, AND THE PHASE PLAN'S CRITERION 0.5.** Under Olivia the FT8 slot watch is not
    /// asked, the PSK31 listener does not start, no card appears, no row can be answered,
    /// and the one send door refuses the press for being this mode.
    /// </remarks>
    [Fact]
    public void NoDecoderIsAttachedAndNoPathReachesAnythingThatKeys()
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
            Assert.False(model.CanAnswerRowsForTests);

            // **THE ONE DOOR REFUSES IT FOR BEING THIS MODE.**
            model.SendCallToAnyoneCommand.Execute(null);

            _output.WriteLine("send line     : " + model.DigitalSendLine);

            Assert.Contains("cannot send " + Mode, model.DigitalSendLine, StringComparison.Ordinal);
        }

        var lines = Lines(keying);

        foreach (var line in lines)
        {
            _output.WriteLine(line);
        }

        Assert.NotEmpty(lines);

        // **NO OTHER MODE'S LISTENER STARTED**, which is where a PSK31 path left running
        // under this tab would show.
        Assert.DoesNotContain(lines, l => l.Contains("psk31_", StringComparison.Ordinal));

        // **THE PRESS IS RECORDED, AND THE RECORD STOPS AT THE REFUSAL.** The operator's own
        // action is written under the transmit category whatever the mode - the press, the
        // request and the refusal - and that is the record of a click, not a keying. What must
        // not be there is anything past the gate: a stage, a composed signal, a keying or a
        // transmission record.
        var transmitEvents = lines
            .Select(l => System.Text.Json.JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("category").GetString() == "transmit")
            .ToList();

        foreach (var e in transmitEvents)
        {
            _output.WriteLine("transmit category: " + e.GetRawText());
        }

        Assert.All(transmitEvents, e => Assert.Equal("operator_action", e.GetProperty("event").GetString()));

        var actions = transmitEvents
            .Select(e => e.GetProperty("data").GetProperty("action").GetString())
            .ToList();

        Assert.Subset(new HashSet<string?> { "cq_pressed", "send_requested", "send_refused" }, actions.ToHashSet());
        Assert.Equal("send_refused", actions.Last());

        foreach (var word in new[] { "transmission", "ptt", "keyed", "composed", "send_stage", "armed" })
        {
            Assert.All(lines, l => Assert.DoesNotContain(word, l, StringComparison.OrdinalIgnoreCase));
        }
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
