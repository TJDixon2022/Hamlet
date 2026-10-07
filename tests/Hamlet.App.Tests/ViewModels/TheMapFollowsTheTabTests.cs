using System;
using System.Collections.Generic;
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
/// **THE MAP FOLLOWS THE TAB** (work instruction 551, HM-DEC-255): at W1AW's 7.0475 the map's block and the header's word
/// are Morse on the CW tab and data on the Digital tab, and repainting them writes nothing to the radio.
/// </summary>
public sealed class TheMapFollowsTheTabTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each step prints what the map and the header said.</param>
    public TheMapFollowsTheTabTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// On the CW tab, then the Digital, then the Voice, then CW again, at 7.0475. The radio is asked for exactly the three
    /// modes the tab already writes (HM-DEC-207), one for each tab, CW, USB-D, LSB and CW, and nothing more: the repaint adds no write.
    /// </remarks>
    [AvaloniaFact]
    public async Task AtW1awTheMapAndTheHeaderFollowTheTabAndWriteNothing()
    {
        var settings = new AppSettings { ReconnectOnStartup = false, LastOperatingMode = "CW" };

        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, null);
        var rig = new RecordingRig();

        model.UseRigForTests(rig);
        model.TuneToCommand.Execute(7_047_500L);
        model.ApplyRigState(RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Frequency, 7_047_500, "7.0475", DateTime.UtcNow.AddSeconds(-5), "CI-V 03"),
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "Cw", DateTime.UtcNow.AddSeconds(-5), "CI-V 04"),
            RigValue.Known(RigField.DataMode, 0, "off", DateTime.UtcNow.AddSeconds(-5), "CI-V 1A 06"),
        }));

        var seen = new List<(string Tab, ModeFamily Map, string Header)>();

        foreach (var tab in new[] { "CW", "Digital", "Voice", "CW" })
        {
            model.OperatingMode = tab;
            await model.FollowTheTabForTests();

            var block = model.MapNeighborhoods.First(n => n.Contains(7_047_500));

            seen.Add((tab, block.Family, model.GreenZone.Family));
            _output.WriteLine($"{tab} tab: the map paints 7.0475 {block.Family} `{block.ShortName}`, the header says `{model.GreenZone.Family}`");
        }

        _output.WriteLine($"radio writes: {string.Join(", ", rig.Writes)}; setting writes: {rig.SettingWrites}");

        Assert.Equal(ModeFamily.Cw, seen[0].Map);
        Assert.Equal(ModeFamily.Digital, seen[1].Map);
        Assert.Equal(ModeFamily.Cw, seen[2].Map);
        Assert.Equal(ModeFamily.Cw, seen[3].Map);
        Assert.All(seen, s => Assert.Equal(Hamlet.App.Controls.ModePalette.For(s.Map).Label, s.Header));

        // The modes the tab itself writes, once each, and no other.
        Assert.Equal(new[] { "Cw", "Usb", "Lsb", "Cw" }, rig.Writes.Select(w => w.Split(' ')[0]));
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
