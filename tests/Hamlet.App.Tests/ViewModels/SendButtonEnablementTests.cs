using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The send buttons become usable when readiness says they may (HM-DEC-078).
/// </summary>
/// <remarks>
/// <para>THE REGRESSION THESE EXIST FOR KILLED TWO LIVE ATTEMPTS. Readiness
/// reached Ready, the record said so, and the buttons on screen stayed dead and
/// swallowed every click. The cause was not the gate and not the notification:
/// the buttons were being destroyed and rebuilt four times a second by the rig
/// poll, and a press and its release cannot land on the same control when the
/// control does not survive 250 milliseconds.</para>
/// <para>So the tests that matter here are the ones that cross the thread the
/// real serial loop crosses, and the one that counts how often the buttons are
/// replaced. A test that only sets state on its own thread passes while the bug
/// survives, which is how this got through twice.</para>
/// </remarks>
public sealed class SendButtonEnablementTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 18, 3, 33, DateTimeKind.Utc);

    private static RigCapabilities Radio { get; } = new(
        "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
        HasUsbAudio: true, CanTransmit: true, new[] { "40 m" });

    /// <summary>Tonight's reading: mode CW, break-in full, not transmitting.</summary>
    private static RigState LiveState() => RigState.Empty.With(new[]
    {
        RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
        RigValue.Known(RigField.BreakIn, 2, "full", Now, "CI-V 16 47"),
        RigValue.Known(RigField.TransmitStatus, 0, "receiving", Now, "CI-V 1C 00"),
    });

    private sealed class Silent : ICwSender
    {
        public bool SupportsCharacterSpacing => false;

        public string PathName => "test";

        public int MaximumMessageLength => CwMessage.MaximumLength;

        public Task<CwSendResult> SendAsync(
            string message, CancellationToken cancellationToken = default)
            => Task.FromResult(new CwSendResult(CwSendOutcome.Sent, "", 1, 1));

        public void Abort()
        {
        }
    }

    private static CwTransmitViewModel Panel(
        Func<RigState> state, Action<bool, CwReadiness?>? enabledChanged = null)
    {
        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio, state()),
            null, null, enabledChanged);

        panel.Attach(new CwTransmitter(new Silent()));

        return panel;
    }

    // ---- The regression --------------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-078: **the buttons are not replaced when nothing about them
    /// changed.** This is the bug. The rig monitor raises its state event every
    /// poll whether anything changed or not, four times a second, and every one
    /// of those reached the rebuild. A control the operator is pressing has to
    /// still be there when they let go.
    /// </remarks>
    [Fact]
    public void TheButtonsSurviveAPollStormWithTheirIdentityIntact()
    {
        var state = LiveState();
        var panel = Panel(() => state);

        Assert.NotEmpty(panel.Options);

        var before = panel.Options.ToList();

        // Four minutes of live polling at 250 ms, which is what the real
        // monitor delivers while somebody is reaching for the mouse.
        for (var i = 0; i < 960; i++)
        {
            panel.Refresh();
        }

        Assert.Equal(before.Count, panel.Options.Count);

        // The same objects, not equal ones: a replacement is a destroyed
        // control however identical it looks.
        for (var i = 0; i < before.Count; i++)
        {
            Assert.Same(before[i], panel.Options[i]);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-078: a staged message survives the same storm. Composing
    /// first and sending on a second press is on by default (HM-DEC-059), and it
    /// could never have worked while the buttons were rebuilt four times a
    /// second.
    /// </remarks>
    [Fact]
    public void AStagedMessageSurvivesThePollStorm()
    {
        var state = LiveState();
        var panel = Panel(() => state);

        panel.Options[0].IsStaged = true;

        for (var i = 0; i < 100; i++)
        {
            panel.Refresh();
        }

        Assert.True(panel.Options[0].IsStaged);
    }

    /// <remarks>
    /// Proves HM-DEC-078: rig state arriving on a background thread reaches the
    /// command. A test that sets state on its own thread passes while the bug
    /// survives, so this crosses the boundary the serial read loop crosses.
    /// </remarks>
    [Fact]
    public async Task ReadinessReachedFromABackgroundThreadMakesTheCommandExecutable()
    {
        var state = RigState.Empty;
        var panel = Panel(() => state);

        Assert.False(panel.CanSend);
        Assert.False(panel.PressCommand.CanExecute(panel.Options.FirstOrDefault()));

        // The reads land, on the thread the serial loop actually uses.
        await Task.Run(() =>
        {
            state = LiveState();
            panel.Refresh();
        });

        Assert.True(panel.CanSend);
        Assert.True(panel.PressCommand.CanExecute(panel.Options[0]));
    }

    /// <remarks>
    /// Proves HM-DEC-078: the command's own change notification is raised in
    /// both directions, so a button bound to it asks again rather than keeping
    /// the answer it had.
    /// </remarks>
    [Fact]
    public void CanExecuteChangedIsRaisedInBothDirections()
    {
        var state = RigState.Empty;
        var panel = Panel(() => state);

        var raised = 0;
        panel.PressCommand.CanExecuteChanged += (_, _) => raised++;

        state = LiveState();
        panel.Refresh();

        Assert.True(panel.CanSend);
        Assert.True(raised > 0, "nothing told the button to ask again");

        var afterReady = raised;

        // And back the other way, which is the case that matters when somebody
        // switches break-in off mid-session.
        state = LiveState().With(
            RigValue.Known(RigField.BreakIn, 0, "off", Now, "CI-V 16 47"));
        panel.Refresh();

        Assert.False(panel.CanSend);
        Assert.True(raised > afterReady, "the button was never told it went dead");
    }

    /// <remarks>
    /// Proves HM-DEC-078 against tonight's exact sequence: connect with nothing
    /// read, readiness refuses mode_unknown, the rig state arrives on a
    /// background thread, readiness reaches Ready, the commands become
    /// executable and the record says the buttons went live.
    /// </remarks>
    [Fact]
    public async Task TonightsSequenceEndsWithAUsableButton()
    {
        var state = RigState.Empty;
        var verdicts = new List<CwReadiness>();
        var enabled = new List<bool>();

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 14_065_500, true, true, Radio, state),
            null,
            (readiness, _, _) => verdicts.Add(readiness),
            (on, _) => enabled.Add(on));

        panel.Attach(new CwTransmitter(new Silent()));

        // 18:03:33.366 — refused, mode_unknown, nothing read yet.
        Assert.Single(verdicts);
        Assert.Equal("mode_unknown", verdicts[0].Reason);
        Assert.False(panel.CanSend);

        // 18:03:33.815 — all 31 fields read, off the UI thread.
        await Task.Run(() =>
        {
            state = LiveState();
            panel.Refresh();
        });

        Assert.Equal(2, verdicts.Count);
        Assert.Equal(CwReadyState.Ready, verdicts[1].State);

        // And this time the screen agrees with the engine.
        Assert.True(panel.CanSend);
        Assert.True(panel.PressCommand.CanExecute(panel.Options[0]));

        Assert.Equal(new[] { true }, enabled);
    }

    // ---- What the record could not see -----------------------------------

    /// <remarks>
    /// Proves HM-DEC-078: the button's own state is reported, so the record
    /// carries what the operator saw. The log said Ready while the screen said
    /// no, and nothing anywhere could show that disagreement.
    /// </remarks>
    [Fact]
    public void TheButtonStateIsReportedWhenItChangesAndNotOtherwise()
    {
        var state = RigState.Empty;
        var enabled = new List<bool>();
        var panel = Panel(() => state, (on, _) => enabled.Add(on));

        Assert.Empty(enabled);

        state = LiveState();
        panel.Refresh();

        Assert.Equal(new[] { true }, enabled);

        // A hundred more polls with nothing changing say nothing.
        for (var i = 0; i < 100; i++)
        {
            panel.Refresh();
        }

        Assert.Single(enabled);

        state = LiveState().With(
            RigValue.Known(RigField.BreakIn, 0, "off", Now, "CI-V 16 47"));
        panel.Refresh();

        Assert.Equal(new[] { true, false }, enabled);
    }

    /// <remarks>
    /// Proves HM-DEC-078: a disabled button always carries its reason. The whole
    /// failure was a dead control that explained nothing, so refusing without
    /// saying why is the state that may not exist.
    /// </remarks>
    [Fact]
    public void ADisabledButtonAlwaysCarriesItsReason()
    {
        foreach (var state in new[]
                 {
                     RigState.Empty,
                     LiveState().With(
                         RigValue.Known(RigField.BreakIn, 0, "off", Now, "CI-V 16 47")),
                     LiveState().With(
                         RigValue.Known(RigField.Mode, (int)CivMode.Usb, "USB", Now, "CI-V 04")),
                 })
        {
            var here = state;
            var panel = Panel(() => here);

            Assert.False(panel.CanSend);
            Assert.True(panel.IsRefusal, "a dead button with no refusal showing");
            Assert.NotEqual("", panel.Status);
        }
    }
}
