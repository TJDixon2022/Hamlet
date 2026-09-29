using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.Tests.Views;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// One W1AW button on the CW tab, for the band the radio is on (work instructions 494 and 495,
/// HM-DEC-199). The owner: *"We only need one button - it turns to the current band."*
/// </summary>
public sealed class W1awButtonsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the button's words are printed.</param>
    public W1awButtonsTests(ITestOutputHelper output) => _output = output;

    private static MainWindowViewModel On(long dialHz, out RecordingRig rig)
    {
        var model = new MainWindowViewModel(new AppSettings { ReconnectOnStartup = false }, null);

        rig = new RecordingRig();
        model.UseRigForTests(rig);
        // Tuned as a press tunes, which takes the band with it: set directly, the dial stops at the
        // edge of the band on screen.
        model.TuneToCommand.Execute(dialHz);

        return model;
    }

    private async Task PressesTo(long dialHz, string label, long wantHz)
    {
        var model = On(dialHz, out var rig);
        var button = model.W1awHere;

        _output.WriteLine($"dial {dialHz}: `{button.Label}`, can press {model.TuneToW1awCommand.CanExecute(button)}");

        Assert.Equal(label, button.Label);
        Assert.True(model.TuneToW1awCommand.CanExecute(button));

        await model.TuneToW1awCommand.ExecuteAsync(button);

        _output.WriteLine($"  asked for {model.FrequencyHz}, mode writes {rig.ModeSets} {rig.LastMode} data {rig.LastData}, settings {rig.SettingWrites}");

        Assert.Equal(wantHz, model.FrequencyHz);
        Assert.Equal(1, rig.ModeSets);
        Assert.Equal(CivMode.Cw, rig.LastMode);
        Assert.False(rig.LastData);
        Assert.Equal(0, rig.SettingWrites);
        Assert.True(model.ModeFollowSuspended);
    }

    /// <remarks>
    /// Case 1: on 40 m the button reads "W1AW on 40 m", and a press asks for 7.0475 MHz and CW - one
    /// mode write, no setting written, nothing keyed (the rig throws if anything tries).
    /// </remarks>
    [Fact]
    public Task On40m() => PressesTo(7_030_000, "W1AW on 40 m", 7_047_500);

    /// <remarks>
    /// Case 2: on 20 m the same button reads "W1AW on 20 m" and asks for 14.0475 MHz; the label
    /// follows the dial from 40 m to 20 m on one model.
    /// </remarks>
    [Fact]
    public async Task On20mAndItFollowsTheDial()
    {
        var model = On(7_030_000, out _);

        Assert.Equal("W1AW on 40 m", model.W1awHere.Label);

        model.TuneToCommand.Execute(14_030_000L);

        Assert.Equal("W1AW on 20 m", model.W1awHere.Label);

        await PressesTo(14_030_000, "W1AW on 20 m", 14_047_500);
    }

    /// <remarks>
    /// Case 3: on a band W1AW does not send Morse on, the button says so and cannot be pressed; off
    /// the spectrum Hamlet knows, as on 6 m, it says Hamlet does not know the band rather than that
    /// W1AW is not there. The license never disables it: a Technician on 20 m can press it, and the
    /// tip says sending Morse there is not covered (HM-DEC-029).
    /// </remarks>
    [Fact]
    public void WhereW1awIsNotItSaysSoAndCannotBePressed()
    {
        var thirty = On(10_120_000, out _);
        var six = W1awButton.ForDial(W1awMorseFrequencies.Default, new PrivilegePlan(), LicenseClass.General, 50_200_000);

        _output.WriteLine($"30 m: `{thirty.W1awHere.Label}`; 6 m: `{six.Label}`");

        Assert.Equal("W1AW not on 30 m", thirty.W1awHere.Label);
        Assert.False(thirty.TuneToW1awCommand.CanExecute(thirty.W1awHere));
        Assert.Equal("W1AW: not a band Hamlet knows", six.Label);
        Assert.False(thirty.TuneToW1awCommand.CanExecute(six));

        var tech = W1awButton.ForDial(W1awMorseFrequencies.Default, new PrivilegePlan(), LicenseClass.Technician, 14_030_000);

        Assert.True(tech.CanTune);
        Assert.Equal(PrivilegeTone.ListenOnly, tech.Tone);
        Assert.Contains("does not cover sending Morse here", tech.Tip, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Case 4: the button is on the CW tab and nowhere else, there is exactly one W1AW button, and
    /// with and without it the send and receive panels stand where they stood.
    /// </remarks>
    [AvaloniaFact]
    public void OneButtonOnTheCwTabMovingNothing()
    {
        var (window, _) = TheTopRowTuned.Open(1400, 900, LicenseClass.General, "CW");

        try
        {
            var button = Named(window, "W1awButton");
            var workspace = Named(window, "CwWorkspace");
            var send = Named(window, "SendPanel");
            var receive = Named(window, "ReceivePanel");
            var w1awButtons = window.GetVisualDescendants().OfType<Button>()
                .Count(b => b.Content is string s && s.StartsWith("W1AW", StringComparison.Ordinal));

            _output.WriteLine($"W1AW buttons in the window: {w1awButtons}, this one reads `{((Button)button).Content}`");

            Assert.True(button.IsEffectivelyVisible, "the W1AW button is not showing on the CW tab");
            Assert.Contains(workspace, button.GetVisualAncestors());
            Assert.Equal(1, w1awButtons);

            var with = (send.Bounds, receive.Bounds, workspace.Bounds);

            button.IsVisible = false;
            TheTopRowTuned.Settle(window);

            var without = (send.Bounds, receive.Bounds, workspace.Bounds);

            _output.WriteLine($"with it:    send {with.Item1}, receive {with.Item2}, workspace {with.Item3}");
            _output.WriteLine($"without it: send {without.Item1}, receive {without.Item2}, workspace {without.Item3}");

            Assert.Equal(without.Item1.Position, with.Item1.Position);
            Assert.Equal(without.Item1.Width, with.Item1.Width);
            Assert.Equal(without.Item2, with.Item2);
            Assert.Equal(without.Item3, with.Item3);
        }
        finally
        {
            window.Close();
        }

        var (digital, _) = TheTopRowTuned.Open(1400, 900, LicenseClass.General, "Digital");

        try
        {
            Assert.False(Named(digital, "W1awButton").IsEffectivelyVisible, "the W1AW button shows off the CW tab");
        }
        finally
        {
            digital.Close();
        }
    }

    private static Control Named(Window window, string name)
        => window.GetVisualDescendants().OfType<Control>().First(c => c.Name == name);

    /// <summary>A radio that records what it is asked for and keys nothing.</summary>
    private sealed class RecordingRig : IRig
    {
        public int FrequencySets { get; private set; }

        public int ModeSets { get; private set; }

        public int SettingWrites { get; private set; }

        public CivMode? LastMode { get; private set; }

        public bool LastData { get; private set; }

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
        {
            FrequencySets++;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RigValue>>(new[] { RigValue.Unknown(field, "the recording radio answers nothing") });

        public Task<RigWriteResult> SetModeAsync(
            CivMode mode, bool dataMode, byte? filterSlot = null, CancellationToken cancellationToken = default)
        {
            ModeSets++;
            LastMode = mode;
            LastData = dataMode;

            return Task.FromResult(RigWriteResult.NotSupported("the recording radio records modes and sets none"));
        }

        public Task<RigWriteResult> SetSettingAsync(CivWrite write, int value, CancellationToken cancellationToken = default)
        {
            SettingWrites++;

            return Task.FromResult(RigWriteResult.NotSupported("the recording radio does not do settings"));
        }

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
