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
/// W1AW's Morse frequencies are one button per band on the CW tab (work instruction 494,
/// HM-DEC-199). Nothing here reads from disk but the table the build embeds.
/// </summary>
public sealed class W1awButtonsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the buttons are printed.</param>
    public W1awButtonsTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Case 1: one button per band Hamlet can honestly take the dial to, 160 m through 10 m, and no
    /// 2 m button - the IC-7300 cannot tune it. **No 6 m button either**: the radio tunes 6 m, but
    /// the spectrum Hamlet knows does not carry it, so the card would say "not an amateur band"
    /// there, which is false (§0.0); the report says so.
    /// </remarks>
    [Fact]
    public void OneButtonPerBandAndNo2m()
    {
        var buttons = W1awButton.For(W1awMorseFrequencies.Default, new PrivilegePlan(), LicenseClass.General);

        foreach (var b in buttons)
        {
            _output.WriteLine($"{b.Label} {b.FrequencyHz} {b.Tone}");
        }

        Assert.Equal(
            new[] { "W1AW 160 m", "W1AW 80 m", "W1AW 40 m", "W1AW 20 m", "W1AW 17 m", "W1AW 15 m", "W1AW 10 m" },
            buttons.Select(b => b.Label));
        Assert.DoesNotContain(buttons, b => b.Label.EndsWith(" 2 m", StringComparison.Ordinal));
        Assert.All(buttons, b => Assert.Equal(PrivilegeTone.Yours, b.Tone));
    }

    /// <remarks>
    /// Case 2: pressing W1AW 20 m asks for 14.0475 MHz and CW, and for nothing else - one mode
    /// write, CW without the data variant, no setting written, and nothing keyed (the rig throws if
    /// anything tries).
    /// </remarks>
    [Fact]
    public async Task Pressing20mAsksFor140475AndCwAndNothingElse()
    {
        var model = new MainWindowViewModel(new AppSettings { ReconnectOnStartup = false }, null);
        var rig = new RecordingRig();

        model.UseRigForTests(rig);

        var button = model.W1awButtons.Single(b => b.Label == "W1AW 20 m");

        await model.TuneToW1awCommand.ExecuteAsync(button);

        _output.WriteLine($"frequency {model.FrequencyHz}, mode writes {rig.ModeSets} {rig.LastMode} data {rig.LastData}, settings {rig.SettingWrites}, direct frequency writes {rig.FrequencySets}");

        Assert.Equal(14_047_500, model.FrequencyHz);
        Assert.Equal(1, rig.ModeSets);
        Assert.Equal(CivMode.Cw, rig.LastMode);
        Assert.False(rig.LastData);
        Assert.Equal(0, rig.SettingWrites);
        Assert.True(model.ModeFollowSuspended);
    }

    /// <remarks>
    /// Case 3: a band outside the operator's CW privileges is present. **It is pressable**, and its
    /// hover says the license does not cover sending there: listening is never restricted
    /// (HM-DEC-029), and grey is kept for what cannot be used (HM-DEC-087). A Technician has no
    /// Morse on 20 m.
    /// </remarks>
    [Fact]
    public void ABandOutsideThePrivilegesIsPresentAndSaysSo()
    {
        var model = new MainWindowViewModel(new AppSettings { ReconnectOnStartup = false }, null);
        var buttons = W1awButton.For(W1awMorseFrequencies.Default, new PrivilegePlan(), LicenseClass.Technician);
        var twenty = buttons.Single(b => b.Label == "W1AW 20 m");

        _output.WriteLine($"{twenty.Label} {twenty.Tone}: {twenty.Tip}");

        Assert.Equal(PrivilegeTone.ListenOnly, twenty.Tone);
        Assert.Contains("does not cover sending Morse here", twenty.Tip, StringComparison.Ordinal);
        Assert.Contains("listening is never restricted", twenty.Tip, StringComparison.Ordinal);
        Assert.True(model.TuneToW1awCommand.CanExecute(twenty));
    }

    /// <remarks>
    /// Case 4: the row is on the CW tab and nowhere else, and it moves no panel: with the row taken
    /// out, the send and receive panels stand exactly where they stood.
    /// </remarks>
    [AvaloniaFact]
    public void TheRowIsOnTheCwTabAndMovesNothing()
    {
        var (window, _) = TheTopRowTuned.Open(1400, 900, LicenseClass.General, "CW");

        try
        {
            var row = Named(window, "W1awRow");
            var workspace = Named(window, "CwWorkspace");
            var send = Named(window, "SendPanel");
            var receive = Named(window, "ReceivePanel");

            Assert.True(row.IsEffectivelyVisible, "the W1AW row is not showing on the CW tab");
            Assert.Contains(workspace, row.GetVisualAncestors());
            Assert.Equal(7, row.GetVisualDescendants().OfType<Button>().Count());

            var withRow = (send.Bounds, receive.Bounds, workspace.Bounds);

            row.IsVisible = false;
            TheTopRowTuned.Settle(window);

            var withoutRow = (send.Bounds, receive.Bounds, workspace.Bounds);

            _output.WriteLine($"with the row: send {withRow.Item1}, receive {withRow.Item2}, workspace {withRow.Item3}");
            _output.WriteLine($"without it:   send {withoutRow.Item1}, receive {withoutRow.Item2}, workspace {withoutRow.Item3}");

            Assert.Equal(withoutRow.Item1.Position, withRow.Item1.Position);
            Assert.Equal(withoutRow.Item1.Width, withRow.Item1.Width);
            Assert.Equal(withoutRow.Item2, withRow.Item2);
            Assert.Equal(withoutRow.Item3, withRow.Item3);
        }
        finally
        {
            window.Close();
        }

        var (digital, _) = TheTopRowTuned.Open(1400, 900, LicenseClass.General, "Digital");

        try
        {
            Assert.False(Named(digital, "W1awRow").IsEffectivelyVisible, "the W1AW row shows off the CW tab");
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
