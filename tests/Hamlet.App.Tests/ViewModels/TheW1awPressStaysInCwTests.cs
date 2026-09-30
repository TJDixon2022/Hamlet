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
/// **The W1AW button's CW sticks** (work instruction 499, HM-DEC-203). The owner pressed `W1AW on
/// 40 m` and the radio ended in USB-D at 3 kHz; after a press the radio is in CW and stays in CW
/// until he changes band or mode himself.
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
    /// Case 3: from 40 m, press, and the radio reports the new frequency and CW back; mode-follow
    /// then looks, as its settle timer does. One mode write, CW, and no later data variant.
    /// </remarks>
    [AvaloniaFact]
    public async Task OnePressOneCwWriteAndNothingAfterTheReportBack()
    {
        var (model, rig) = Pressed(7_030_000, 7_030_000);

        model.ApplyRigState(Radio(7_047_500, CivMode.Cw, false, DateTime.UtcNow.AddSeconds(1)));
        await model.FollowTheMapForTests();

        _output.WriteLine($"writes: {string.Join(", ", rig.Writes)}; suspended {model.ModeFollowSuspended}");

        Assert.Equal(new[] { "Cw data False" }, rig.Writes);
        Assert.True(model.ModeFollowSuspended);
    }

    /// <remarks>
    /// Case 3, the press's own band change: with the band on screen elsewhere, the press moves it to
    /// 40 m, and that band change keeps the hold rather than re-arming mode-follow over the CW.
    /// </remarks>
    [AvaloniaFact]
    public async Task ThePressesOwnBandChangeKeepsTheHold()
    {
        var model = new MainWindowViewModel(General(), null);
        var rig = new RecordingRig();

        model.UseRigForTests(rig);
        model.TuneToCommand.Execute(7_030_000L);
        model.SelectedBand = model.Bands[3];

        await model.TuneToW1awCommand.ExecuteAsync(model.W1awHere);
        await model.FollowTheMapForTests();

        _output.WriteLine($"band {model.SelectedBand.Band.Name}, dial {model.FrequencyHz}; writes: {string.Join(", ", rig.Writes)}; suspended {model.ModeFollowSuspended}");

        Assert.Equal("40 m", model.SelectedBand.Band.Name);
        Assert.Equal(new[] { "Cw data False" }, rig.Writes);
        Assert.True(model.ModeFollowSuspended);
    }

    /// <remarks>
    /// Case 3, and the hold ends where the order says: the operator changing band himself re-arms
    /// mode-follow exactly as before (HM-DEC-056).
    /// </remarks>
    [AvaloniaFact]
    public async Task HisOwnBandChangeReArmsIt()
    {
        var (model, _) = Pressed(7_030_000, 7_030_000);

        model.SelectBandCommand.Execute(model.Bands[3]);
        await Task.Yield();

        _output.WriteLine($"after his band press: {model.SelectedBand.Band.Name}, suspended {model.ModeFollowSuspended}");

        Assert.False(model.ModeFollowSuspended);
    }

    /// <remarks>
    /// Case 2: at 7.0475 the card reads Morse, with no digital sub-mode under it, and the license
    /// line speaks about Morse.
    /// </remarks>
    [AvaloniaFact]
    public async Task TheCardAtW1awReadsMorse()
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
