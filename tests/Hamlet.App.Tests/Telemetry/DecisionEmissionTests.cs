using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// A refusal reaches the record whether or not anybody pressed anything
/// (HM-DEC-077).
/// </summary>
/// <remarks>
/// THE EXACT HOLE THIS FILLS. A disabled button fires no handler, so nothing was
/// written, so the record could not distinguish "Hamlet refused" from "Hamlet is
/// broken" from "nobody pressed it". The evaluation now fires when readiness
/// recomputes rather than when something is pressed, which is what makes the
/// absence of a press visible instead of invisible.
/// </remarks>
public sealed class DecisionEmissionTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 21, 0, 0, DateTimeKind.Utc);

    private static RigCapabilities Radio { get; } = new(
        "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
        HasUsbAudio: true, CanTransmit: true, new[] { "40 m" });

    private static RigState Ready(int breakIn = 2) => RigState.Empty.With(new[]
    {
        RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
        RigValue.Known(RigField.BreakIn, breakIn, "full", Now, "CI-V 16 47"),
        RigValue.Known(RigField.TransmitStatus, 0, "receiving", Now, "CI-V 1C 00"),
    });

    /// <summary>A sender that cannot actually key anything.</summary>
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

    /// <remarks>
    /// Proves HM-DEC-077: a refusal with nobody pressing anything still emits.
    /// This is the evening's failure in one test.
    /// </remarks>
    [Fact]
    public void ARefusalWithNoButtonPressStillEmits()
    {
        var state = Ready(breakIn: 0);
        var emitted = new List<CwReadiness>();

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio, state),
            null,
            (readiness, _, _) => emitted.Add(readiness));

        panel.Attach(new CwTransmitter(new Silent()));

        // Nothing was pressed. Nothing could be: the buttons are off.
        Assert.False(panel.CanSend);

        Assert.Single(emitted);
        Assert.Equal(CwReadyState.BreakInOff, emitted[0].State);
        Assert.Equal("break_in_off", emitted[0].Reason);
        Assert.Equal(Outcome.Refused, emitted[0].Outcome);
    }

    /// <remarks>
    /// Proves HM-DEC-077: a state arriving after connect causes readiness to
    /// re-evaluate and emit. **This is the detector for the first candidate**:
    /// if the gate computed once while break-in was unknown and nothing
    /// invalidated it, this test is what would have failed.
    /// </remarks>
    [Fact]
    public void AStateArrivingAfterConnectReEvaluatesAndEmits()
    {
        // Connect first, with nothing read yet.
        var state = RigState.Empty;
        var emitted = new List<CwReadiness>();

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio, state),
            null,
            (readiness, _, _) => emitted.Add(readiness));

        panel.Attach(new CwTransmitter(new Silent()));

        Assert.False(panel.CanSend);
        Assert.Single(emitted);
        Assert.Equal(CwReadyState.ModeUnknown, emitted[0].State);

        // Now the reads land, exactly as they do a moment after a real connect.
        state = Ready();
        panel.Refresh();

        Assert.True(panel.CanSend);
        Assert.Equal(2, emitted.Count);
        Assert.Equal(CwReadyState.Ready, emitted[1].State);
        Assert.Equal(Outcome.Proceeded, emitted[1].Outcome);
    }

    /// <remarks>
    /// Proves HM-DEC-077: an unchanged verdict is not written again. The
    /// evaluation fires on recompute, and recompute happens on every rig state
    /// change, so writing every time would bury the transitions that are the
    /// whole diagnosis under thousands of identical rows.
    /// </remarks>
    [Fact]
    public void AnUnchangedVerdictIsNotWrittenAgain()
    {
        var state = Ready();
        var emitted = new List<CwReadiness>();

        var panel = new CwTransmitViewModel(
            () => new TransmitContext(
                LicenseClass.General, 7_030_000, true, true, Radio, state),
            null,
            (readiness, _, _) => emitted.Add(readiness));

        panel.Attach(new CwTransmitter(new Silent()));

        for (var i = 0; i < 20; i++)
        {
            panel.Refresh();
        }

        Assert.Single(emitted);

        // And a change writes again immediately.
        state = Ready(breakIn: 0);
        panel.Refresh();

        Assert.Equal(2, emitted.Count);
    }

    /// <remarks>
    /// Proves HM-DEC-077: the decision log keeps transitions and drops repeats,
    /// so the window a person reads matches the file they would upload.
    /// </remarks>
    [Fact]
    public void TheDecisionLogKeepsTransitionsAndDropsRepeats()
    {
        var log = new DecisionLogViewModel();

        Assert.True(log.IsEmpty);

        log.Note("Can I send", "break_in_unknown", Outcome.Refused, "not read yet", Now);
        log.Note("Can I send", "break_in_unknown", Outcome.Refused, "not read yet", Now);
        log.Note("Can I send", "ok", Outcome.Proceeded, "", Now);

        Assert.Equal(2, log.Rows.Count);
        Assert.Equal("ok", log.Rows[0].Reason);
        Assert.False(log.IsEmpty);

        var text = log.ForBugReport();

        Assert.Contains("break_in_unknown", text, StringComparison.Ordinal);
        Assert.Contains("refused", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-018 where the record grew most: nothing the decision log
    /// holds or copies can carry a callsign, a location or a message. The window
    /// is the thing somebody pastes into a bug report, so what it writes is
    /// exactly what is on screen.
    /// </remarks>
    [Fact]
    public void NothingTheDecisionLogCopiesCanIdentifyAnybody()
    {
        var log = new DecisionLogViewModel();

        log.Note("Can I send", "break_in_off", Outcome.Refused,
            "Break-in is off on the radio.", Now);

        var text = log.ForBugReport();

        foreach (var forbidden in new[]
                 { "KC3QIS", "W1AW", "Pittsburgh", "Timothy", "EN90", "CQ CQ" })
        {
            Assert.DoesNotContain(forbidden, text, StringComparison.OrdinalIgnoreCase);
        }
    }
}
