using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **HAMLET SETS AGC FOR CW, AS IT SETS THE PREAMP** (work instruction 564, HM-DEC-268), on a fake rig that holds its AGC,
/// reports it and confirms what is written. The owner, 2026-10-08: *"I have no idea how to do that."*
/// </summary>
public sealed class HamletSetsAgcForCwTests(ITestOutputHelper output)
{
    private static AppSettings On(string tab, string agcInCw = "slow")
    {
        var settings = new AppSettings { ReconnectOnStartup = false, LastOperatingMode = tab, AgcInCw = agcInCw };

        settings.Operator.LicenseClass = LicenseClass.General;

        return settings;
    }

    private static RigState Radio(long hz, CivMode mode, bool data, int? agc)
    {
        var at = DateTime.UtcNow;
        var values = new List<RigValue>
        {
            RigValue.Known(RigField.Frequency, hz, (hz / 1_000_000.0).ToString("0.0000"), at, "CI-V 03"),
            RigValue.Known(RigField.Mode, (int)mode, mode.ToString(), at, "CI-V 04"),
            RigValue.Known(RigField.DataMode, data ? 1 : 0, data ? "on" : "off", at, "CI-V 1A 06"),
        };

        if (agc is { } a)
        {
            values.Add(RigValue.Known(RigField.Agc, a, CwAgc.Words(a), at, "CI-V 16 12"));
        }

        return RigState.Empty.With(values);
    }

    private static (MainWindowViewModel Model, AgcRig Rig) Connected(string tab, CivMode mode, bool data, int agc, string agcInCw = "slow")
    {
        var model = new MainWindowViewModel(On(tab, agcInCw), null);
        var rig = new AgcRig(agc);

        model.UseRigForTests(rig);
        model.TuneToCommand.Execute(7_030_000L);
        model.ApplyRigState(Radio(7_030_000, mode, data, agc));

        return (model, rig);
    }

    /// <remarks>Entering CW with the radio on FAST sets SLOW once, says so on the story line, and does not set it again as the dial moves.</remarks>
    [AvaloniaFact]
    public async Task EnteringCwSetsSlowOnce()
    {
        var (model, rig) = Connected("CW", CivMode.Cw, false, CwAgc.Fast);

        await model.FollowTheTabForTests();
        var said = model.StatusText;

        foreach (var hz in new long[] { 7_035_000, 14_030_000 })
        {
            model.TuneToCommand.Execute(hz);
            model.ApplyRigState(Radio(hz, CivMode.Cw, false, rig.Agc));
            await model.FollowTheTabForTests();
        }

        output.WriteLine($"AGC writes: {string.Join(", ", rig.AgcWrites)}; said `{said}`");

        Assert.Equal(new[] { CwAgc.Slow }, rig.AgcWrites);
        Assert.Equal(CwAgc.Slow, rig.Agc);
        Assert.StartsWith("AGC set to SLOW for CW.", said, StringComparison.Ordinal);
    }

    /// <remarks>A change by hand after Hamlet set SLOW is not overridden on the same tab, and the record says it is his.</remarks>
    [AvaloniaFact]
    public async Task AHandChangeIsNotOverridden()
    {
        var (model, rig) = Connected("CW", CivMode.Cw, false, CwAgc.Fast);

        await model.FollowTheTabForTests();

        rig.Agc = CwAgc.Mid;
        model.TuneToCommand.Execute(14_030_000L);
        model.ApplyRigState(Radio(14_030_000, CivMode.Cw, false, rig.Agc));
        await model.FollowTheTabForTests();

        var (agc, setBy) = model.AgcForTheRecord();

        output.WriteLine($"AGC writes: {string.Join(", ", rig.AgcWrites)}; record `{agc}` {setBy}");

        Assert.Equal(new[] { CwAgc.Slow }, rig.AgcWrites);
        Assert.Equal(CwAgc.Mid, rig.Agc);
        Assert.Equal("MID", agc);
        Assert.Equal("set by hand", setBy);
    }

    /// <remarks>Leaving CW puts back the AGC the radio had before Hamlet changed it, and says so.</remarks>
    [AvaloniaFact]
    public async Task LeavingCwPutsBackWhatTheRadioHad()
    {
        var (model, rig) = Connected("CW", CivMode.Cw, false, CwAgc.Fast);

        await model.FollowTheTabForTests();
        model.OperatingMode = "Digital";

        output.WriteLine($"AGC writes: {string.Join(", ", rig.AgcWrites)}; says `{model.StatusText}`");

        Assert.Equal(new[] { CwAgc.Slow, CwAgc.Fast }, rig.AgcWrites);
        Assert.Equal(CwAgc.Fast, rig.Agc);
        Assert.Equal("AGC put back to FAST, as it was before CW.", model.StatusText);
    }

    /// <remarks>Leaving CW after his hand moved the AGC leaves it where he put it.</remarks>
    [AvaloniaFact]
    public async Task LeavingCwAfterAHandChangeLeavesHisSetting()
    {
        var (model, rig) = Connected("CW", CivMode.Cw, false, CwAgc.Fast);

        await model.FollowTheTabForTests();
        rig.Agc = CwAgc.Mid;
        model.OperatingMode = "Voice";

        output.WriteLine($"AGC writes: {string.Join(", ", rig.AgcWrites)}");

        Assert.Equal(new[] { CwAgc.Slow }, rig.AgcWrites);
        Assert.Equal(CwAgc.Mid, rig.Agc);
    }

    /// <remarks>The Digital and Voice tabs write no AGC: the radio keeps what it had.</remarks>
    [AvaloniaFact]
    public async Task TheDigitalAndVoiceTabsWriteNoAgc()
    {
        foreach (var tab in new[] { "Digital", "Voice" })
        {
            var (model, rig) = Connected(tab, tab == "Digital" ? CivMode.Usb : CivMode.Lsb, tab == "Digital", CwAgc.Fast);

            await model.FollowTheTabForTests();
            await model.FollowTheTabForTests();

            output.WriteLine($"{tab}: AGC writes: {string.Join(", ", rig.AgcWrites)}");

            Assert.Empty(rig.AgcWrites);
            Assert.Equal(CwAgc.Fast, rig.Agc);
        }
    }

    /// <remarks>The `leave the radio alone` setting writes nothing to the AGC, and the record says so.</remarks>
    [AvaloniaFact]
    public async Task LeaveAloneWritesNothing()
    {
        var (model, rig) = Connected("CW", CivMode.Cw, false, CwAgc.Fast, agcInCw: "leave");

        await model.FollowTheTabForTests();
        model.ApplyRigState(Radio(7_030_000, CivMode.Cw, false, rig.Agc));

        var (agc, setBy) = model.AgcForTheRecord();

        output.WriteLine($"choice `{model.AgcInCw}`; AGC writes: {string.Join(", ", rig.AgcWrites)}; record `{agc}` {setBy}");

        Assert.Equal("leave the radio alone", model.AgcInCw);
        Assert.Empty(rig.AgcWrites);
        Assert.Equal("FAST", agc);
        Assert.Equal("the radio's own, Hamlet is set to leave it alone", setBy);
    }

    /// <remarks>
    /// The capture sheet's line and the `cw_listen` row carry the AGC as the radio reports it and whose it is: set by Hamlet
    /// once it set it.
    /// </remarks>
    [AvaloniaFact]
    public async Task TheSheetAndTheRowCarryTheAgcAndWhoSetIt()
    {
        var (model, rig) = Connected("CW", CivMode.Cw, false, CwAgc.Fast);

        await model.FollowTheTabForTests();
        model.ApplyRigState(Radio(7_030_000, CivMode.Cw, false, rig.Agc));

        var (agc, setBy) = model.AgcForTheRecord();
        var sheet = model.AgcSheetLineForTests();
        var rows = new Rows();
        var sample = new CwListenSample(0, 0, 0, 0, [], 0, "listening", null);

        AppEvents.CwListen(rows, sample, "1.13.249", 0, agc, setBy);

        output.WriteLine(sheet);
        output.WriteLine($"row agc `{rows.All[0]["agc"]}` agcSetBy `{rows.All[0]["agcSetBy"]}`");

        Assert.Equal("agc        SLOW  (set by Hamlet; Hamlet's choice for CW is SLOW)", sheet);
        Assert.Equal("SLOW", rows.All[0]["agc"]);
        Assert.Equal("set by Hamlet", rows.All[0]["agcSetBy"]);
    }

    /// <remarks>The choice is kept across restarts.</remarks>
    [AvaloniaFact]
    public void TheChoiceIsKept()
    {
        var settings = On("CW");
        var model = new MainWindowViewModel(settings, null);

        model.AgcInCw = "FAST";
        var reopened = new MainWindowViewModel(settings, null);

        Assert.Equal("fast", settings.AgcInCw);
        Assert.Equal("FAST", reopened.AgcInCw);
        Assert.Equal(CwAgcChoice.Fast, reopened.AgcChoice);
    }

    private sealed class Rows : ITelemetry
    {
        public List<IReadOnlyDictionary<string, object?>> All { get; } = [];

        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category, string eventName, IReadOnlyDictionary<string, object?>? data = null, TelemetryLevel level = TelemetryLevel.Info)
            => All.Add(data ?? new Dictionary<string, object?>());
    }

    /// <summary>A radio that holds its AGC, reports it, and confirms what is written; every other field answers nothing.</summary>
    private sealed class AgcRig(int agc) : IRig
    {
        public int Agc { get; set; } = agc;

        public List<int> AgcWrites { get; } = [];

        public bool IsConnected => true;

        public bool IsSimulated => false;

        public RigCapabilities Capabilities { get; } = new("fake AGC radio", false, false, false, false, HfBands.Names);

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default) => Task.FromResult(0L);

        public Task SetFrequencyHzAsync(long frequencyHz, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<RigValue>> ReadAsync(RigField field, RigState context, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RigValue>>(field == RigField.Agc
                ? [RigValue.Known(RigField.Agc, Agc, CwAgc.Words(Agc), DateTime.UtcNow, "fake 16 12")]
                : [RigValue.Unknown(field, "the fake radio answers only its AGC")]);

        public Task<RigWriteResult> SetModeAsync(CivMode mode, bool dataMode, byte? filterSlot = null, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.Confirmed("the fake radio"));

        public Task<RigWriteResult> SetSettingAsync(CivWrite write, int value, CancellationToken cancellationToken = default)
        {
            if (write.Field != RigField.Agc)
            {
                return Task.FromResult(RigWriteResult.NotSupported("the fake radio holds only its AGC"));
            }

            AgcWrites.Add(value);
            Agc = value;

            return Task.FromResult(RigWriteResult.Confirmed("the fake radio"));
        }

        public Task<bool> SendCwAsync(string message, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("AGC keys nothing (CLAUDE.md 0.2)");

        public void AbortCw()
        {
        }

        internal void NobodyRaisesThese()
        {
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(0));
            ValuesReported?.Invoke(this, new RigValuesReportedEventArgs([]));
        }
    }
}
