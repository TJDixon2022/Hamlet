using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The transmit path on the day the antenna is real (HM-DEC-074).
/// </summary>
/// <remarks>
/// Everything here has been proved against canned bytes, fakes and a dummy
/// load. What these add is the behavior that only matters once a signal can
/// actually leave: that a send which cannot reach the air says so before the
/// press, that the abort works while a send is running, and that nothing
/// reports success it did not see.
/// </remarks>
public sealed class LiveFireTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 14, 0, 0, DateTimeKind.Utc);

    private static RigCapabilities Radio { get; } = new(
        "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
        HasUsbAudio: true, CanTransmit: true, new[] { "40 m" });

    private static RigState State(int breakIn = 1, int? powerPercent = null)
    {
        var values = new List<RigValue>
        {
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
            RigValue.Known(RigField.BreakIn, breakIn, breakIn == 0 ? "off" : "semi",
                Now, "CI-V 16 47"),
        };

        if (powerPercent is { } percent)
        {
            values.Add(RigValue.Known(
                RigField.RfPower, percent, $"{percent}%", Now, "CI-V 14 0A"));
        }

        return RigState.Empty.With(values);
    }

    private static TransmitContext Context(RigState? state = null)
        => new(LicenseClass.General, 7_030_000, true, true, Radio, state ?? State());

    /// <summary>A sender that blocks until the test lets it finish.</summary>
    private sealed class SlowSender : ICwSender
    {
        private readonly TaskCompletionSource _released = new();
        private readonly TaskCompletionSource _started = new();

        public bool SupportsCharacterSpacing => false;

        public string PathName => "test keyer";

        public int MaximumMessageLength => CwMessage.MaximumLength;

        public bool AbortWasCalled { get; private set; }

        /// <summary>Completes once the send is genuinely in flight.</summary>
        public Task Started => _started.Task;

        public async Task<CwSendResult> SendAsync(
            string message, CancellationToken cancellationToken = default)
        {
            _started.TrySetResult();
            await _released.Task.ConfigureAwait(false);

            return AbortWasCalled
                ? new CwSendResult(CwSendOutcome.Aborted, "Stopped part way through.", 1, 3)
                : new CwSendResult(CwSendOutcome.Sent, "", 3, 3);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Records and releases on the calling thread, awaiting nothing, which
        /// is the shape §0.2 requires of every abort.
        /// </remarks>
        public void Abort()
        {
            AbortWasCalled = true;
            _released.TrySetResult();
        }
    }

    // ---- The precondition, before the press ----------------------------

    /// <remarks>
    /// Proves HM-DEC-074: with break-in off the send is refused before anything
    /// is attempted, and the reason names the setting. A correct frame, a
    /// correct acknowledgement and no signal is the worst outcome available on a
    /// live day, because he would conclude the app is lying or that nobody wants
    /// to talk to him.
    /// </remarks>
    [Fact]
    public void BreakInOffIsSaidBeforeThePressAndNamesTheSetting()
    {
        var sender = new SlowSender();
        var transmitter = new CwTransmitter(sender);

        var check = transmitter.Check(Context(State(breakIn: 0)));

        Assert.False(check.Sent);
        Assert.Contains("Break-in is off", check.Detail, StringComparison.Ordinal);
        Assert.Contains("19-7", check.Citation, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-074: an unread break-in setting refuses too. Not having
    /// looked is not permission, and "I do not know whether this will go out" is
    /// a different answer from "it will" (§0.0).
    /// </remarks>
    [Fact]
    public void AnUnreadBreakInSettingIsNotPermission()
    {
        var state = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
        });

        var check = new CwTransmitter(new SlowSender()).Check(Context(state));

        Assert.False(check.Sent);
        Assert.Contains("has not read", check.Detail, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-074: nothing is sent when the precondition fails, so the
    /// refusal is a real gate and not only a message beside a live control.
    /// </remarks>
    [Fact]
    public async Task NothingIsSentWhenThePreconditionFails()
    {
        var sender = new SlowSender();
        var transmitter = new CwTransmitter(sender);

        var outcome = await transmitter.SendAsync(
            "CQ CQ DE KC3QIS K", Context(State(breakIn: 0)));

        Assert.False(outcome.Sent);
        Assert.Null(outcome.Result);
        Assert.False(sender.Started.IsCompleted);
    }

    // ---- The abort, while a send is running -----------------------------

    /// <remarks>
    /// Proves §0.2 and HM-DEC-074: the abort works while a send is genuinely in
    /// flight, and it awaits nothing. A stop that waits its turn behind the
    /// thing it is stopping is not a stop.
    /// </remarks>
    [Fact]
    public async Task TheAbortWorksWhileASendIsRunning()
    {
        var sender = new SlowSender();
        var transmitter = new CwTransmitter(sender);

        var sending = transmitter.SendAsync("CQ CQ DE KC3QIS K", Context());

        await sender.Started;

        Assert.False(sending.IsCompleted);

        // On this thread, returning nothing, with the send still in the air.
        transmitter.Abort();

        Assert.True(sender.AbortWasCalled);

        var outcome = await sending;

        Assert.False(outcome.Sent);
        Assert.Equal(CwSendOutcome.Aborted, outcome.Result?.Outcome);
    }

    /// <remarks>
    /// Proves §0.2: aborting when nothing is sending is safe, and aborting twice
    /// is safe. An abort that could throw is an abort somebody cannot rely on at
    /// the moment they need it most.
    /// </remarks>
    [Fact]
    public void AbortingIsAlwaysSafe()
    {
        var transmitter = new CwTransmitter(new SlowSender());

        transmitter.Abort();
        transmitter.Abort();
    }

    // ---- Honest failure --------------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-074: a radio that did not confirm produces an unknown
    /// rather than a success. Success is never inferred from the absence of an
    /// error (§0.0).
    /// </remarks>
    [Fact]
    public void ARadioThatDidNotConfirmIsNeverReportedAsSent()
    {
        var noAnswer = new CwSendResult(
            CwSendOutcome.NoAnswer,
            "The radio did not take that, so Hamlet cannot say what went out.",
            1, 3);

        Assert.False(noAnswer.Worked);

        foreach (var outcome in Enum.GetValues<CwSendOutcome>())
        {
            var result = new CwSendResult(outcome, "", 0, 1);

            Assert.Equal(outcome == CwSendOutcome.Sent, result.Worked);
        }
    }

    // ---- What Hamlet says beside the buttons ----------------------------

    /// <remarks>
    /// Proves HM-DEC-074: the dummy load warning is retired and what replaces it
    /// does not pretend to know what is connected. Nothing in the CI-V read
    /// table reports the antenna socket, so a claim either way would be invented.
    /// </remarks>
    [Fact]
    public void HamletDoesNotPretendToKnowWhatIsConnected()
    {
        // THE NOTICE IS GONE (HM-DEC-083). What replaced it is the chain report,
        // which says what the meters read rather than admitting ignorance, and
        // its own test sweeps it for claims about the socket. What survives here
        // is the rule the notice served: nothing Hamlet says beside the send
        // controls is an instruction or a scolding.
        var said = string.Join(" ", TransmitNotes.For(
            State().With(RigValue.Known(
                RigField.RfPower, 5, "5%", Now, "CI-V 14 0A")))).ToLowerInvariant();

        Assert.NotEqual("", said);

        foreach (var scold in new[]
                 { "you must", "you should", "be careful", "make sure", "do not " })
        {
            Assert.False(said.Contains(scold, StringComparison.Ordinal),
                $"the note says '{scold}'");
        }
    }

    /// <remarks>
    /// Proves HM-DEC-074: power turned down is said as a consequence, which is
    /// the specific evening this exists for. Somebody turns the power down for a
    /// dummy load test, connects an antenna, and cannot work out why the band
    /// has gone quiet.
    /// </remarks>
    [Fact]
    public void PowerLeftTurnedDownIsSaidAsAConsequence()
    {
        var quiet = TransmitNotes.PowerNote(State(powerPercent: 5));

        Assert.Contains("5 percent", quiet, StringComparison.Ordinal);
        Assert.Contains("about the power rather than about your sending", quiet,
            StringComparison.Ordinal);

        // A PERCENTAGE AND NEVER A WATTAGE. Turning the radio's own scale into
        // watts needs a power curve section 4 has no citation for.
        Assert.DoesNotContain("watt", quiet, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves HM-DEC-074: nothing is said in the middle of the range, and
    /// nothing at all is said from a power that was never read. A line that
    /// always appears is a line nobody reads, and one resting on an assumed
    /// setting is the confident guess §0.0 forbids.
    /// </remarks>
    [Theory]
    [InlineData(50)]
    [InlineData(80)]
    public void NothingIsSaidAboutAnOrdinaryPowerSetting(int percent)
    {
        Assert.Equal("", TransmitNotes.PowerNote(State(powerPercent: percent)));

        // And nothing else stands in for it: an ordinary radio says nothing at
        // all rather than something bland (HM-DEC-083).
        Assert.Empty(TransmitNotes.For(State(powerPercent: percent)));
    }

    /// <remarks>Proves HM-DEC-074: a power nobody has read says nothing.</remarks>
    [Fact]
    public void AnUnreadPowerSaysNothing()
    {
        Assert.Equal("", TransmitNotes.PowerNote(State()));
        Assert.Equal("", TransmitNotes.PowerNote(RigState.Empty));
    }
}
