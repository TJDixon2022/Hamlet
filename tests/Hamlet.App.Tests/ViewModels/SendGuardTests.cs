using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Training;
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
    /// Proves HM-DEC-083, which supersedes HM-DEC-079 on this point at Tim's
    /// direction: **sending has no look of its own.** You cannot send while
    /// sending, so the buttons go grey for the duration and the status block
    /// says what is happening. The dedicated green treatment was solving a
    /// problem the latch had already removed, and a state that needs its own
    /// color to be understood has not been explained.
    ///
    /// Armed is the state that still needs one, because it is pressable and the
    /// press is the point.
    /// </remarks>
    [Fact]
    public void SendingWearsTheDisabledLookAndArmedDoesNot()
    {
        var sending = new SendButtonViewModel(
            new SendOption(ContactStage.Calling, "Call CQ", "CQ DE KC3QIS K",
                "Calling anyone.", ""))
        {
            State = SendState.Sending,
        };

        Assert.True(sending.LooksRefused);
        Assert.True(sending.LooksSending);
        Assert.Equal("Sending…", sending.ButtonLabel);

        var armed = new SendButtonViewModel(
            new SendOption(ContactStage.Calling, "Call CQ", "CQ DE KC3QIS K",
                "Calling anyone.", ""))
        {
            State = SendState.Armed,
        };

        Assert.False(armed.LooksRefused);
        Assert.Equal(1.0, armed.Dimmed);
        Assert.Equal("Press again to send", armed.ButtonLabel);
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

            // Whatever the transmit line is doing, this control holds one
            // state for the whole send (HM-DEC-079's latch), and that state is
            // sending rather than a per-element sample.
            Assert.True(button.LooksSending);
            Assert.True(panel.IsSending);
        }

        Assert.Equal(new[] { SendState.Sending }, seen);

        sender.Finish();
        await sending;

        // AND IT IS STILL SENDING (HM-DEC-085). The send call returning means the
        // radio took the message, not that the radio sent it, and this line used
        // to assert the opposite. The state ends when the transmission does.
        Assert.True(panel.IsSending);
        Assert.True(panel.Options[0].LooksSending);
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
    /// <remarks>
    /// <para>Proves HM-DEC-085, and it is the assertion this test used to make
    /// backwards. It used to require the finish to arrive inside the press,
    /// which encoded the bug: **handing the message over is not the
    /// transmission.** Command `17` gives the keyer up to thirty characters and
    /// returns about thirteen milliseconds later, and the radio then keys for
    /// another eighteen seconds.</para>
    /// <para>So the start arrives at the press and the finish does not.</para>
    /// </remarks>
    [Fact]
    public async Task TheSendStartsAtThePressAndDoesNotFinishThere()
    {
        var clock = Now;
        var state = Live();
        var started = new List<string>();
        var finished = new List<double>();

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio, state),
            null, null, null,
            (message, _) => started.Add(message),
            (_, _, _, elapsed, _) => finished.Add(elapsed.TotalSeconds),
            now: () => clock);

        panel.Attach(new CwTransmitter(new Recorder()));

        await panel.PressCommand.ExecuteAsync(panel.Options[0]);

        Assert.Single(started);
        Assert.Empty(finished);
        Assert.True(panel.IsSending);

        // The start carries the message so the caller can measure it. What
        // reaches telemetry is its length and never its words (HM-DEC-018).
        Assert.Equal(panel.Options[0].Original, started[0]);
    }

    /// <remarks>
    /// <para>**THE TEST THE PREVIOUS TWO ATTEMPTS DID NOT HAVE** (HM-DEC-085).
    /// It runs a whole send at the panel and drives the transmit line the way
    /// full break-in really drives it, from the message's own key pattern at the
    /// rate the rig is really polled, and counts how many times the send controls
    /// change appearance.</para>
    /// <para>**One change down and one back up. Nothing in between.** That is the
    /// operator's most-repeated complaint, raised three times and shipped wrong
    /// twice, and this is the assertion that would have caught both.</para>
    /// </remarks>
    [Fact]
    public async Task TheSendButtonsChangeOnceDownAndOnceBackUp()
    {
        var clock = Now;
        var keying = 0;

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio,
                Live(transmitting: keying)),
            now: () => clock);

        panel.Attach(new CwTransmitter(new Recorder()));

        var message = panel.Options[0].Original;
        await panel.PressCommand.ExecuteAsync(panel.Options[0]);

        // What the operator looks at: whether the buttons are wearing the
        // unpressable look. Sampled every time the rig state comes round.
        bool Grey() => panel.Options.All(o => o.LooksRefused);

        var was = Grey();
        var changes = 0;
        var lineMoved = 0;
        var wasKeyed = false;

        var pattern = MorseCode.KeyPattern(message);
        var dits = MorseCode.LengthInDits(message);
        var dit = MorseCode.Dit(CwDuration.DefaultWpm).TotalMilliseconds;
        var total = dits * dit;

        Assert.True(was, "the buttons did not go grey when the send started");

        for (var t = 250.0; t < total + 4000; t += 250)
        {
            var down = t <= total
                && MorseCode.IsKeyDown(pattern, dits, dits * 10, t / dit);

            if (down != wasKeyed)
            {
                lineMoved++;
                wasKeyed = down;
            }

            keying = down ? 1 : 0;
            clock = Now.AddMilliseconds(t);
            panel.Refresh();

            if (Grey() != was)
            {
                changes++;
                was = Grey();
            }
        }

        // The line really flapped, so this is the condition that broke the panel
        // and not an easier one.
        Assert.True(lineMoved > 20, $"the line only moved {lineMoved} times");

        Assert.Equal(1, changes);
        Assert.False(panel.IsSending);
        Assert.False(Grey());
    }

    /// <remarks>
    /// Proves HM-DEC-085: what the record and the operator are told is the length
    /// of the transmission and not of the handover. It used to be a hundredth of
    /// a second, and that figure reached the screen as "the radio keyed for 0
    /// seconds" under an eighteen-second call.
    /// </remarks>
    [Fact]
    public async Task TheFinishReportsTheRealSeconds()
    {
        var clock = Now;
        var keying = 0;
        var finished = new List<double>();

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio,
                Live(transmitting: keying)),
            null, null, null, null,
            (_, _, _, elapsed, _) => finished.Add(elapsed.TotalSeconds),
            now: () => clock);

        panel.Attach(new CwTransmitter(new Recorder()));

        var message = panel.Options[0].Original;
        await panel.PressCommand.ExecuteAsync(panel.Options[0]);

        var pattern = MorseCode.KeyPattern(message);
        var dits = MorseCode.LengthInDits(message);
        var dit = MorseCode.Dit(CwDuration.DefaultWpm).TotalMilliseconds;
        var total = dits * dit;

        for (var t = 250.0; t < total + 4000; t += 250)
        {
            keying = t <= total
                && MorseCode.IsKeyDown(pattern, dits, dits * 10, t / dit) ? 1 : 0;
            clock = Now.AddMilliseconds(t);
            panel.Refresh();
        }

        var expected = CwDuration.Of(message, CwDuration.DefaultWpm).TotalSeconds;

        Assert.Single(finished);
        Assert.True(finished[0] > 1.0, $"reported {finished[0]:0.00} seconds");
        Assert.InRange(finished[0], expected * 0.9, expected * 1.3);
    }

    /// <remarks>
    /// Proves HM-DEC-085 and §0.2: stopping ends the state on the spot, with no
    /// hold-off and nothing awaited. The abort may never wait for a poll.
    /// </remarks>
    [Fact]
    public async Task StoppingEndsTheSendImmediately()
    {
        var clock = Now;

        // Not keying yet at the moment of the press, which is the only state the
        // guard will let a send start from.
        var keying = 0;
        var finished = new List<double>();

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio,
                Live(transmitting: keying)),
            null, null, null, null,
            (_, _, _, elapsed, _) => finished.Add(elapsed.TotalSeconds),
            now: () => clock);

        panel.Attach(new CwTransmitter(new Recorder()));

        await panel.PressCommand.ExecuteAsync(panel.Options[0]);
        Assert.True(panel.IsSending);

        // The radio starts keying, and the panel sees it.
        keying = 1;
        clock = Now.AddSeconds(1);
        panel.Refresh();
        Assert.True(panel.IsSending);

        // Three seconds into an eighteen-second call, and the radio drops its
        // transmit line the moment the abort reaches it.
        clock = Now.AddSeconds(3);
        keying = 0;
        panel.AbortCommand.Execute(null);

        // No hold-off waited out and no poll waited for.
        Assert.False(panel.IsSending);
        Assert.Single(finished);
        Assert.Equal(3.0, finished[0], precision: 2);

        // And the controls come straight back rather than staying grey until
        // something else happens to refresh them.
        Assert.DoesNotContain(panel.Options, o => o.LooksRefused);
    }
}
