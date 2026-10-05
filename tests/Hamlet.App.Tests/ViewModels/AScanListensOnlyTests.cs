using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **WHILE A SCAN RUNS, NOTHING IN HAMLET CAN TRANSMIT** (work instruction 540, task 3, HM-DEC-244): the application's
/// half of the proof. The engine's half drives a whole scan on a fake radio (<c>TheCatchScanTests</c>).
/// </summary>
/// <remarks>
/// The view model is given a fake radio, a fake port and a fake sound card, its keying paths built on its own scan lock
/// as connecting builds them, and the lock is held as a running scan holds it. Then every transmit control the window
/// carries is pressed: the CW send, the CW auto-caller's arm and start, the digital CQ, the PSK31 answer, typed line and
/// canned lines, and the message send. Nothing keys: no keyer command reaches the radio, no frame reaches the port, and
/// nothing is handed to the sound card. The CW send buttons say why.
/// </remarks>
public sealed class AScanListensOnlyTests
{
    /// <remarks>Task 3: every transmit control pressed during a scan does nothing.</remarks>
    [AvaloniaFact]
    public async Task EveryTransmitControlDoesNothingWhileAScanRuns()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);
        var rig = new KeyCountingRig();
        var port = new FakePort();
        var sink = new FakeSink();
        var listenOnly = model.ListenOnlyLock;

        model.UseRigForTests(rig);
        model.UseRigPortForTests(port);
        model.Transmit.Attach(new CwTransmitter(new KeyerCwSender(rig, listenOnly), listenOnly: listenOnly));
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, listenOnly: listenOnly)));

        using (listenOnly.Hold())
        {
            model.Transmit.Refresh();

            // The CW buttons are grey for the scan and say so, not for some other missing thing.
            Assert.False(model.Transmit.CanSend);
            Assert.Equal(Hamlet.RadioEngine.Scan.ListenOnlyLock.Refusal, model.Transmit.Status);

            Press(model.ComposeCqCommand);
            Press(model.Transmit.PressCommand);
            Press(model.Transmit.PressCommand);
            Press(model.AutoCall.ArmCommand);
            Press(model.AutoCall.StartCommand);
            Press(model.SendCallToAnyoneCommand);
            Press(model.SendMessageCommand);
            Press(model.AnswerPsk31Command);
            Press(model.SendTypedPsk31Command);

            await Task.Delay(500);
        }

        Assert.Equal(0, rig.Keyed);
        Assert.True(port.WasNeverWrittenTo, $"{port.Written.Count} frames reached the port");
        Assert.Equal(0, sink.TimesCalled);

        // Let go, the same refresh no longer gives the scan's reason.
        model.Transmit.Refresh();

        Assert.NotEqual(Hamlet.RadioEngine.Scan.ListenOnlyLock.Refusal, model.Transmit.Status);
    }

    private static void Press(System.Windows.Input.ICommand command)
    {
        try
        {
            command.Execute(null);
        }
        catch (Exception)
        {
            // A command that throws for want of a parameter or a radio keys nothing either; what is asserted is the radio.
        }
    }

    /// <summary>A radio in CW with break-in on that counts every keying command and every write but the frequency.</summary>
    private sealed class KeyCountingRig : IRig
    {
        private static readonly DateTime Now = DateTime.UtcNow;

        public int Keyed { get; private set; }

        public bool IsConnected => true;

        public bool IsSimulated => true;

        public RigCapabilities Capabilities { get; } = new("Listen-only test radio", true, true, true, true, Array.Empty<string>());

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default) => Task.FromResult(7_030_000L);

        public Task SetFrequencyHzAsync(long frequencyHz, CancellationToken cancellationToken = default)
        {
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(frequencyHz));
            return Task.CompletedTask;
        }

        public Task<bool> SendCwAsync(string message, CancellationToken cancellationToken = default)
        {
            Keyed++;
            return Task.FromResult(true);
        }

        public void AbortCw()
        {
        }

        public Task<RigWriteResult> SetSettingAsync(CivWrite write, int value, CancellationToken cancellationToken = default)
        {
            Keyed++;
            return Task.FromResult(RigWriteResult.NotSupported("listen-only test radio"));
        }

        public Task<RigWriteResult> SetModeAsync(CivMode mode, bool dataMode, byte? filterSlot, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("listen-only test radio"));

        public Task<IReadOnlyList<RigValue>> ReadAsync(RigField field, RigState context, CancellationToken cancellationToken = default)
        {
            RigValue value = field switch
            {
                RigField.Frequency => RigValue.Known(field, 7_030_000, "7.030 MHz", Now, "test"),
                RigField.Mode => RigValue.Known(field, 3, "CW", Now, "test"),
                RigField.BreakIn => RigValue.Known(field, 1, "semi break-in", Now, "test"),
                _ => RigValue.Known(field, 0, "0", Now, "test"),
            };

            return Task.FromResult<IReadOnlyList<RigValue>>(new[] { value });
        }

        public void Unused() => ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(Array.Empty<RigValue>()));
    }
}
