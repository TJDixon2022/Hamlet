using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **The tab is the mode; the map writes nothing** (work instruction 503, R111, HM-DEC-207). Tim:
/// *"If I'm on the CW tab, we have CW settings. If I'm on the data tab, we have data settings."*
/// </summary>
/// <remarks>
/// On 2026-09-30 at 12:30 UTC the app restarted, read the radio at 7.0472 MHz in CW, and two
/// seconds later mode-follow wrote USB-D, because the map calls 7.0472 the RTTY block.
/// </remarks>
public sealed class TheTabIsTheModeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each case prints what the radio was asked for.</param>
    public TheTabIsTheModeTests(ITestOutputHelper output) => _output = output;

    private static RigState Radio(long hz, CivMode mode, bool data, DateTime takenUtc)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Frequency, hz, (hz / 1_000_000.0).ToString("0.0000"), takenUtc, "CI-V 03"),
            RigValue.Known(RigField.Mode, (int)mode, mode.ToString(), takenUtc, "CI-V 04"),
            RigValue.Known(RigField.DataMode, data ? 1 : 0, data ? "on" : "off", takenUtc, "CI-V 1A 06"),
        });

    private static AppSettings On(string tab)
    {
        var settings = new AppSettings { ReconnectOnStartup = false, LastOperatingMode = tab };

        settings.Operator.LicenseClass = LicenseClass.General;

        return settings;
    }

    private static (MainWindowViewModel Model, RecordingRig Rig) Connected(string tab, long hz, CivMode mode, bool data)
    {
        var model = new MainWindowViewModel(On(tab), null);
        var rig = new RecordingRig();

        model.UseRigForTests(rig);
        model.TuneToCommand.Execute(hz);
        model.ApplyRigState(Radio(hz, mode, data, DateTime.UtcNow.AddSeconds(-5)));

        return (model, rig);
    }

    /// <remarks>
    /// Case 1: on the CW tab at 7.0472 MHz - inside the RTTY block by unit 499's data, the frequency
    /// of the 12:30 restart - the app writes CW once and never a data variant, however the dial
    /// moves: across the FT8 block, and onto 20 m's.
    /// </remarks>
    [AvaloniaFact]
    public async Task OnTheCwTabTheRadioIsCwWhereverTheDialGoes()
    {
        var (model, rig) = Connected("CW", 7_047_200, CivMode.Cw, false);
        var block = NeighborhoodPlan.ForBand(HfBands.BandFor(7_047_200)!).First(n => n.Contains(7_047_200));

        _output.WriteLine($"7.0472 is the map's `{block.ShortName}` block, whose mode-follow target was {ModeFollowPlan.TargetFor(block)?.Name ?? "none"}");

        await model.FollowTheTabForTests();

        foreach (var hz in new long[] { 7_074_000, 7_047_200, 14_074_000, 14_030_000 })
        {
            model.TuneToCommand.Execute(hz);
            model.ApplyRigState(Radio(hz, CivMode.Cw, false, DateTime.UtcNow.AddSeconds(1)));
            await model.FollowTheTabForTests();
        }

        _output.WriteLine($"writes: {string.Join(", ", rig.Writes)}");

        Assert.Equal(new[] { "Cw data False filter -" }, rig.Writes);
    }

    /// <remarks>
    /// Case 2: a restart with the CW tab selected, the radio left in USB-D: the first mode write is CW.
    /// </remarks>
    [AvaloniaFact]
    public async Task ARestartOnTheCwTabWritesCwFirst()
    {
        var (model, rig) = Connected("CW", 7_047_200, CivMode.Usb, true);

        await model.FollowTheTabForTests();

        _output.WriteLine($"tab {model.OperatingMode}; writes: {string.Join(", ", rig.Writes)}; says `{model.StatusText}`");

        Assert.Equal("CW", model.OperatingMode);
        Assert.Equal("Cw data False filter -", rig.Writes.First());
    }

    /// <remarks>
    /// Case 3: from the CW tab, select Digital: one USB-D write with the data filter and the data
    /// receive settings; back to CW: one CW write, the radio's own CW filter, and the CW settings.
    /// </remarks>
    [AvaloniaFact]
    public async Task EachTabWritesItsModeAndItsSettingsOnce()
    {
        var (model, rig) = Connected("CW", 14_074_000, CivMode.Cw, false);

        await model.FollowTheTabForTests();
        var cwSettings = model.LastReceiverSetup.Select(r => r.Condition.Control).ToList();

        model.OperatingMode = "Digital";
        await model.FollowTheTabForTests();
        await model.FollowTheTabForTests();
        var digitalSettings = model.LastReceiverSetup.Select(r => r.Condition.Control).ToList();

        model.OperatingMode = "CW";
        await model.FollowTheTabForTests();

        _output.WriteLine($"writes: {string.Join(", ", rig.Writes)}");
        _output.WriteLine($"CW settings: {string.Join(", ", cwSettings)}");
        _output.WriteLine($"Digital settings: {string.Join(", ", digitalSettings)}");

        Assert.Equal(
            new[] { "Cw data False filter -", "Usb data True filter 1", "Cw data False filter -" },
            rig.Writes);
        Assert.NotEmpty(cwSettings);
        Assert.NotEmpty(digitalSettings);
        Assert.NotEqual(cwSettings, digitalSettings);
    }

    /// <remarks>
    /// Case 4: the CW tab has set CW, and then the radio reports USB - the operator's own hand on the
    /// mode knob. The app writes nothing, follows the radio, and says so.
    /// </remarks>
    [AvaloniaFact]
    public async Task HisHandOnTheKnobWinsAndNothingIsWrittenBack()
    {
        var (model, rig) = Connected("CW", 7_030_000, CivMode.Cw, false);

        await model.FollowTheTabForTests();

        model.ApplyRigState(Radio(7_030_000, CivMode.Usb, false, DateTime.UtcNow.AddSeconds(1)));
        var said = model.StatusText;

        await model.FollowTheTabForTests();
        model.TuneToCommand.Execute(7_035_000L);
        await model.FollowTheTabForTests();

        _output.WriteLine($"writes: {string.Join(", ", rig.Writes)}; says `{said}`; suspended {model.ModeFollowSuspended}");

        Assert.Equal(new[] { "Cw data False filter -" }, rig.Writes);
        Assert.True(model.ModeFollowSuspended);
        Assert.Contains("You set the radio to", said, StringComparison.Ordinal);
        Assert.Contains("until you next change tab", said, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Case 5: every mode write the application can make, by the source: two calls, the tab's and
    /// the Olivia press's, and nothing else in the app writes a mode.
    /// </remarks>
    [Fact]
    public void EveryModeWriteIsNamed()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var app = Path.Combine(root, "src", "Hamlet.App");
        var calls = Directory.EnumerateFiles(app, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                        && !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .SelectMany(f => File.ReadAllLines(f).Select((line, i) => (File: Path.GetFileName(f), Line: i + 1, Text: line)))
            .Where(l => l.Text.Contains(".SetModeAsync(", StringComparison.Ordinal))
            .ToList();

        foreach (var call in calls)
        {
            _output.WriteLine($"{call.File}:{call.Line}: {call.Text.Trim()}");
        }

        var source = File.ReadAllText(Path.Combine(app, "ViewModels", "MainWindowViewModel.cs"));
        var tab = Between(source, "private async Task FollowTheTabAsync()", "private async Task EstablishReceiveConditionsAsync(");
        var olivia = Between(source, "private async Task TuneToOliviaAsync(", "\n    }\n");

        Assert.Equal(2, calls.Count);
        Assert.Contains(".SetModeAsync(", tab, StringComparison.Ordinal);
        Assert.Contains(".SetModeAsync(", olivia, StringComparison.Ordinal);
    }

    private static string Between(string source, string from, string to)
    {
        var start = source.IndexOf(from, StringComparison.Ordinal);

        Assert.True(start >= 0, $"`{from}` is not in the source");

        var end = source.IndexOf(to, start + from.Length, StringComparison.Ordinal);

        return end < 0 ? source[start..] : source[start..end];
    }

    /// <summary>A radio that confirms every mode it is asked for, records it with its filter, and keys nothing.</summary>
    private sealed class RecordingRig : IRig
    {
        public List<string> Writes { get; } = new();

        public int SettingWrites { get; private set; }

        public bool IsConnected => true;

        public bool IsSimulated => false;

        public RigCapabilities Capabilities { get; } = new(
            "recording CI-V", false, false, false, false, HfBands.Names);

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default) => Task.FromResult(0L);

        public Task SetFrequencyHzAsync(long frequencyHz, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RigValue>>(new[] { RigValue.Unknown(field, "the recording radio answers nothing") });

        public Task<RigWriteResult> SetModeAsync(
            CivMode mode, bool dataMode, byte? filterSlot = null, CancellationToken cancellationToken = default)
        {
            Writes.Add($"{mode} data {dataMode} filter {(filterSlot is { } slot ? slot.ToString() : "-")}");

            return Task.FromResult(RigWriteResult.Confirmed("the recording radio"));
        }

        public Task<RigWriteResult> SetSettingAsync(CivWrite write, int value, CancellationToken cancellationToken = default)
        {
            SettingWrites++;

            return Task.FromResult(RigWriteResult.NotSupported("the recording radio does not do settings"));
        }

        public Task<bool> SendCwAsync(string message, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("the tab keys nothing (CLAUDE.md 0.2)");

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
