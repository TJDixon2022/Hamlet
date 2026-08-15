using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The send controls tell the truth about their own state (HM-DEC-079).
/// </summary>
/// <remarks>
/// <para>CW TRANSMIT WORKS ON REAL HARDWARE. What did not work was the operator
/// being able to tell that. Two successful transmissions went out and he did not
/// know it at the time, because every send took two presses with nothing on
/// screen saying so, and because grey was being spent on three different things
/// at once.</para>
/// <para>So these hold the two durable rules: **grey means refused and nothing
/// else**, and **the confirming press guards what the operator wrote rather than
/// what Hamlet wrote.**</para>
/// </remarks>
public sealed class SendGuardTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 18, 48, 30, DateTimeKind.Utc);

    private static RigCapabilities Radio { get; } = new(
        "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
        HasUsbAudio: true, CanTransmit: true, new[] { "40 m" });

    private static RigState Live(int breakIn = 2, int transmitting = 0)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
            RigValue.Known(RigField.BreakIn, breakIn, "full", Now, "CI-V 16 47"),
            RigValue.Known(
                RigField.TransmitStatus, transmitting,
                transmitting == 1 ? "transmitting" : "receiving", Now, "CI-V 1C 00"),
        });

    /// <summary>A sender that records what it was asked to send.</summary>
    private sealed class Recorder : ICwSender
    {
        public List<string> Sent { get; } = new();

        public bool SupportsCharacterSpacing => false;

        public string PathName => "test";

        public int MaximumMessageLength => CwMessage.MaximumLength;

        public Task<CwSendResult> SendAsync(
            string message, CancellationToken cancellationToken = default)
        {
            Sent.Add(message);
            return Task.FromResult(new CwSendResult(CwSendOutcome.Sent, "", 1, 1));
        }

        public void Abort()
        {
        }
    }

    private static (CwTransmitViewModel Panel, Recorder Sender) Panel(
        Func<RigState> state)
    {
        var sender = new Recorder();

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio, state()));

        panel.Attach(new CwTransmitter(sender));

        return (panel, sender);
    }

    // ---- The guard is for what the operator wrote ------------------------

    /// <remarks>
    /// Proves HM-DEC-079: **Hamlet's own words send on one press.** The message
    /// is on screen in full and has already been read, so a confirming press
    /// adds nothing. This is the change: it used to take two, nothing said so,
    /// and the operator concluded the button was broken.
    /// </remarks>
    [Fact]
    public async Task UneditedTextSendsOnOnePress()
    {
        var state = Live();
        var (panel, sender) = Panel(() => state);

        var button = panel.Options[0];

        Assert.False(button.IsEdited);

        await panel.PressCommand.ExecuteAsync(button);

        Assert.Single(sender.Sent);
        Assert.Equal(button.Original, sender.Sent[0]);
    }

    /// <remarks>
    /// Proves HM-DEC-079: **edited text takes two presses.** The first arms and
    /// transmits nothing, the second sends it exactly as it stands. That is the
    /// message nobody has checked and the one worth guarding.
    /// </remarks>
    [Fact]
    public async Task EditedTextArmsOnTheFirstPressAndSendsOnTheSecond()
    {
        var state = Live();
        var (panel, sender) = Panel(() => state);

        var button = panel.Options[0];
        button.Message = "CQ DE KC3QIS PSE K";

        Assert.True(button.IsEdited);

        await panel.PressCommand.ExecuteAsync(button);

        Assert.Empty(sender.Sent);
        Assert.True(button.IsArmed);
        Assert.Equal(SendState.Armed, button.State);
        Assert.Equal("Press again to send", button.ButtonLabel);

        await panel.PressCommand.ExecuteAsync(button);

        Assert.Single(sender.Sent);
        Assert.Equal("CQ DE KC3QIS PSE K", sender.Sent[0]);
    }

    /// <remarks>
    /// Proves HM-DEC-079: cancel returns to unarmed without transmitting. There
    /// used to be no way out of an armed message except pressing the very thing
    /// somebody was unsure about, which is the opposite of what a confirming
    /// press is for.
    /// </remarks>
    [Fact]
    public async Task CancelReturnsToUnarmedWithoutTransmitting()
    {
        var state = Live();
        var (panel, sender) = Panel(() => state);

        var button = panel.Options[0];
        button.Message = "SOMETHING ELSE";

        await panel.PressCommand.ExecuteAsync(button);

        Assert.True(button.IsArmed);

        panel.DisarmCommand.Execute(null);

        Assert.False(button.IsArmed);
        Assert.Equal(SendState.Ready, button.State);
        Assert.Empty(sender.Sent);
    }

    /// <remarks>
    /// Proves HM-DEC-079: editing and then reverting sends on one press again.
    /// Somebody who changes his mind and deletes back to the original has not
    /// written anything, and asking him to confirm Hamlet's own words would be
    /// the guard firing on the case it exists to skip.
    /// </remarks>
    [Fact]
    public async Task TextRevertedToTheOriginalSendsOnOnePressAgain()
    {
        var state = Live();
        var (panel, sender) = Panel(() => state);

        var button = panel.Options[0];
        var original = button.Original;

        button.Message = original + " EXTRA";
        Assert.True(button.IsEdited);

        button.Message = original;

        Assert.False(button.IsEdited);
        Assert.False(button.IsArmed);

        await panel.PressCommand.ExecuteAsync(button);

        Assert.Single(sender.Sent);
    }

    /// <remarks>
    /// Proves HM-DEC-079: an armed message disarms itself the moment the text
    /// goes back to Hamlet's, so somebody who arms and then reverts is not left
    /// facing a second press for no reason.
    /// </remarks>
    [Fact]
    public async Task RevertingAnArmedMessageDisarmsIt()
    {
        var state = Live();
        var (panel, sender) = Panel(() => state);

        var button = panel.Options[0];
        button.Message = "EDITED";

        await panel.PressCommand.ExecuteAsync(button);

        Assert.True(button.IsArmed);

        panel.RevertCommand.Execute(button);

        Assert.False(button.IsArmed);
        Assert.Equal(button.Original, button.Message);
        Assert.Empty(sender.Sent);
    }

    /// <remarks>
    /// Proves HM-DEC-079: arming one message and turning to another clears the
    /// guard, so a press on the second is never consumed by the first one's
    /// confirmation.
    /// </remarks>
    [Fact]
    public async Task ArmingOneMessageAndPressingAnotherClearsTheGuard()
    {
        var state = Live();
        var (panel, sender) = Panel(() => state);

        Assert.True(panel.Options.Count > 1, "this needs two options to mean anything");

        var first = panel.Options[0];
        var second = panel.Options[1];

        first.Message = "EDITED";
        await panel.PressCommand.ExecuteAsync(first);

        Assert.True(first.IsArmed);

        // The second is Hamlet's own words, so it goes on one press and the
        // first stops being armed.
        await panel.PressCommand.ExecuteAsync(second);

        Assert.False(first.IsArmed);
        Assert.Single(sender.Sent);
        Assert.Equal(second.Original, sender.Sent[0]);
    }

    /// <remarks>
    /// Proves HM-DEC-079: the option for somebody who wants the old behavior
    /// still works, and it is off by default so it cannot surprise anybody.
    /// </remarks>
    [Fact]
    public async Task ConfirmEverythingIsOffByDefaultAndStillWorksWhenOn()
    {
        var state = Live();
        var (panel, sender) = Panel(() => state);

        Assert.False(panel.AlwaysConfirm);

        panel.AlwaysConfirm = true;

        await panel.PressCommand.ExecuteAsync(panel.Options[0]);

        Assert.Empty(sender.Sent);
        Assert.True(panel.Options[0].IsArmed);
    }

    // ---- Grey means refused and nothing else -----------------------------

    /// <remarks>
    /// Proves HM-DEC-079, and this is the durable one. **Only a readiness
    /// refusal may look disabled.** Grey means "you cannot press this" in every
    /// interface anybody has ever used, and spending it on armed and on sending
    /// left it meaning nothing. Asserted on the property the style binds to, so
    /// it cannot regress silently.
    /// </remarks>
    [Fact]
    public async Task OnlyARefusalEverLooksDisabled()
    {
        var state = Live();
        var (panel, _) = Panel(() => state);

        var button = panel.Options[0];

        // Ready: active.
        Assert.Equal(SendState.Ready, button.State);
        Assert.False(button.LooksRefused);
        Assert.Equal(1.0, button.Dimmed);

        // Armed: active. It does something, so it may not be dimmed.
        button.Message = "EDITED";
        await panel.PressCommand.ExecuteAsync(button);

        Assert.Equal(SendState.Armed, button.State);
        Assert.False(button.LooksRefused);
        Assert.Equal(1.0, button.Dimmed);

        panel.DisarmCommand.Execute(null);

        // Refused: and only now.
        state = Live(breakIn: 0);
        panel.Refresh();

        Assert.Equal(SendState.Refused, button.State);
        Assert.True(button.LooksRefused);
        Assert.True(button.Dimmed < 1.0);

        // And a refusal always says why.
        Assert.True(panel.IsRefusal);
        Assert.NotEqual("", panel.Status);
    }

    /// <remarks>
    /// Proves HM-DEC-079: typing in a message does not cause the button to be
    /// rebuilt, which would throw the operator's own words away four times a
    /// second. What decides a rebuild is the script changing its mind, not the
    /// operator changing his (HM-DEC-078).
    /// </remarks>
    [Fact]
    public void EditedTextSurvivesThePollStorm()
    {
        var state = Live();
        var (panel, _) = Panel(() => state);

        var button = panel.Options[0];
        button.Message = "CQ DE KC3QIS QRS PSE K";

        for (var i = 0; i < 200; i++)
        {
            panel.Refresh();
        }

        Assert.Same(button, panel.Options[0]);
        Assert.Equal("CQ DE KC3QIS QRS PSE K", panel.Options[0].Message);
        Assert.True(panel.Options[0].IsEdited);
    }

    /// <remarks>
    /// Proves HM-DEC-079: sending is an active state and never wears the
    /// disabled look. It is working, which is the opposite of what grey says.
    /// </remarks>
    [Fact]
    public void SendingIsActiveAndNeverLooksDisabled()
    {
        var button = new SendButtonViewModel(
            new SendOption(ContactStage.Calling, "Call CQ", "CQ DE KC3QIS K",
                "Calling anyone.", ""))
        {
            State = SendState.Sending,
        };

        Assert.False(button.LooksRefused);
        Assert.Equal(1.0, button.Dimmed);
        Assert.Equal("Sending…", button.ButtonLabel);
    }

    // ---- Sending is a state, not a per-element sample ---------------------

    /// <remarks>
    /// Proves HM-DEC-079: the controls hold one state across a transmission in
    /// which the transmit line toggles repeatedly. Under full break-in the radio
    /// keys element by element, so readiness refuses "already transmitting"
    /// dozens of times across one eighteen second call, and the buttons used to
    /// flip enabled and disabled on every dah with clicks lost into the disabled
    /// frames.
    /// </remarks>
    [Fact]
    public async Task TheControlsHoldOneStateWhileTheTransmitLineToggles()
    {
        var state = Live();
        var sender = new SlowRecorder();

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio, state));

        panel.Attach(new CwTransmitter(sender));

        var button = panel.Options[0];
        var sending = panel.PressCommand.ExecuteAsync(button);

        await sender.Started;

        // Eighteen seconds of break-in, sampled the way the poll loop samples
        // it: on, off, on, off, faster than anybody can click.
        var seen = new HashSet<SendState>();

        for (var i = 0; i < 72; i++)
        {
            state = Live(transmitting: i % 2);
            panel.Refresh();
            seen.Add(button.State);

            // Whatever the transmit line is doing, this control is not grey.
            Assert.False(button.LooksRefused);
            Assert.True(panel.IsSending);
        }

        Assert.Equal(new[] { SendState.Sending }, seen);

        sender.Finish();
        await sending;

        Assert.True(panel.Options[0].State is SendState.Ready or SendState.Refused);
    }

    /// <summary>A sender the test can hold open, as a real send holds open.</summary>
    private sealed class SlowRecorder : ICwSender
    {
        private readonly TaskCompletionSource _released = new();
        private readonly TaskCompletionSource _started = new();

        public bool SupportsCharacterSpacing => false;

        public string PathName => "test";

        public int MaximumMessageLength => CwMessage.MaximumLength;

        public Task Started => _started.Task;

        public void Finish() => _released.TrySetResult();

        public async Task<CwSendResult> SendAsync(
            string message, CancellationToken cancellationToken = default)
        {
            _started.TrySetResult();
            await _released.Task.ConfigureAwait(false);

            return new CwSendResult(CwSendOutcome.Sent, "", 1, 1);
        }

        public void Abort() => _released.TrySetResult();
    }

    // ---- The send is in the record ---------------------------------------

    /// <remarks>
    /// Proves HM-DEC-079: a send writes its start and its finish. The record had
    /// every decision the gate made and nothing about what the radio did, so two
    /// successful transmissions had to be reconstructed from the shape of a
    /// status line flapping (§0.0.1).
    /// </remarks>
    [Fact]
    public async Task ASendWritesItsStartAndItsFinish()
    {
        var state = Live();
        var started = new List<string>();
        var finished = new List<TransmitOutcome?>();

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio, state),
            null, null, null,
            (message, _) => started.Add(message),
            (_, _, outcome) => finished.Add(outcome));

        panel.Attach(new CwTransmitter(new Recorder()));

        await panel.PressCommand.ExecuteAsync(panel.Options[0]);

        Assert.Single(started);
        Assert.Single(finished);
        Assert.True(finished[0]?.Sent);

        // The start carries the message so the caller can measure it. What
        // reaches telemetry is its length and never its words (HM-DEC-018).
        Assert.Equal(panel.Options[0].Original, started[0]);
    }
}
