using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **The W1AW button tunes, and the CW tab's CW stands** (work instructions 499 and 503, HM-DEC-203,
/// HM-DEC-207). Unit 499 made the press set CW and hold mode-follow off; the hold did not survive a
/// restart, and since R111 the CW tab sets CW and keeps it, so the press only tunes. Re-pinned to
/// R111: the tab writes the mode, the map and the button do not.
/// </summary>
public sealed class TheW1awPressStaysInCwTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each case prints what the radio was asked for.</param>
    public TheW1awPressStaysInCwTests(ITestOutputHelper output) => _output = output;

    private static RigState Radio(long hz, CivMode mode, bool data, DateTime takenUtc)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Frequency, hz, (hz / 1_000_000.0).ToString("0.0000"), takenUtc, "CI-V 03"),
            RigValue.Known(RigField.Mode, (int)mode, mode.ToString(), takenUtc, "CI-V 04"),
            RigValue.Known(RigField.DataMode, data ? 1 : 0, data ? "on" : "off", takenUtc, "CI-V 1A 06"),
        });

    /// <remarks>
    /// On the CW tab, the tab writes CW once; the press, the radio reporting 7.0475 back, and the tab
    /// looking again write nothing more, and no data variant.
    /// </remarks>
    [AvaloniaFact]
    public async Task ThePressTunesAndTheTabsCwIsTheOnlyWrite()
    {
        var (model, rig) = Pressed(7_030_000, 7_030_000);

        model.ApplyRigState(Radio(7_047_500, CivMode.Cw, false, DateTime.UtcNow.AddSeconds(1)));
        await model.FollowTheTabForTests();

        _output.WriteLine($"writes: {string.Join(", ", rig.Writes)}; suspended {model.ModeFollowSuspended}");

        Assert.Equal(new[] { "Cw data False" }, rig.Writes);
        Assert.False(model.ModeFollowSuspended);
    }

    /// <remarks>
    /// A band change re-arms nothing (R111): after the operator sets USB on the radio's own knob,
    /// his own band press leaves the app standing down, and nothing is written back.
    /// </remarks>
    [AvaloniaFact]
    public async Task HisBandChangeDoesNotReArmTheModeOverHisKnob()
    {
        var (model, rig) = Pressed(7_030_000, 7_030_000);

        model.ApplyRigState(Radio(7_047_500, CivMode.Usb, false, DateTime.UtcNow.AddSeconds(1)));
        model.SelectBandCommand.Execute(model.Bands[3]);
        await model.FollowTheTabForTests();

        _output.WriteLine($"after his band press: {model.SelectedBand.Band.Name}, suspended {model.ModeFollowSuspended}, writes {string.Join(", ", rig.Writes)}");

        Assert.True(model.ModeFollowSuspended);
        Assert.Equal(new[] { "Cw data False" }, rig.Writes);
    }

    /// <remarks>
    /// At 7.0475 the card reads Morse, with no digital sub-mode under it, and the license line
    /// speaks about Morse (unit 499's case 2, unchanged).
    /// </remarks>
    [AvaloniaFact]
    public void TheCardAtW1awReadsMorse()
    {
        var (model, _) = Pressed(7_030_000, 7_030_000);
        var card = model.GreenZone;

        _output.WriteLine($"`{card.Band}  {card.Frequency}  {card.ModeLine}` / `{card.License}`");

        Assert.Equal("Morse", card.Family);
        Assert.False(card.HasSubMode);
        Assert.Contains("Morse", card.License, StringComparison.Ordinal);
    }

    private static AppSettings General()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.LicenseClass = LicenseClass.General;

        return settings;
    }

    private static (MainWindowViewModel Model, RecordingRig Rig) Pressed(long dialHz, long radioHz)
    {
        var model = new MainWindowViewModel(General(), null);
        var rig = new RecordingRig();

        model.UseRigForTests(rig);
        model.TuneToCommand.Execute(dialHz);
        model.ApplyRigState(Radio(radioHz, CivMode.Cw, false, DateTime.UtcNow.AddSeconds(-5)));

        // The CW tab writes CW once, as it does when a radio connects (work instruction 503).
        model.FollowTheTabForTests().GetAwaiter().GetResult();

        var button = model.W1awHere;

        Assert.Equal("W1AW on 40 m", button.Label);
        model.TuneToW1awCommand.ExecuteAsync(button).GetAwaiter().GetResult();

        Assert.Equal(7_047_500, model.FrequencyHz);

        return (model, rig);
    }

    /// <summary>A radio that confirms every mode it is asked for, records it, and keys nothing.</summary>
    private sealed class RecordingRig : IRig
    {
        public List<string> Writes { get; } = new();

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
            Writes.Add($"{mode} data {dataMode}");

            return Task.FromResult(RigWriteResult.Confirmed("the recording radio"));
        }

        public Task<RigWriteResult> SetSettingAsync(CivWrite write, int value, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("the recording radio does not do settings"));

        public Task<bool> SendCwAsync(string message, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("a W1AW button keys nothing (CLAUDE.md 0.2)");

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
