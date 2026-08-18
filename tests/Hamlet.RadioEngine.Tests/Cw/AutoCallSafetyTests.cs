using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The envelope around the first thing in this project that keys a transmitter
/// without somebody pressing something each time (HM-DEC-098, §0.2).
/// </summary>
/// <remarks>
/// <para>**NOTHING IN THIS FILE TRANSMITS.** Every keying call lands on a fake
/// that records what would have gone out and returns, so the whole cycle can be
/// driven end to end with no radio and no RF. That is not a convenience: it is
/// the only way to break each interlock in turn without keying a transmitter
/// forty times to do it.</para>
/// <para>**EVERY ABORT IS SIMULATED RATHER THAN REASONED ABOUT**, which is the
/// standard the scanner's own safety suite set and the standard HM-DEC-098 names:
/// reasoning about an interlock is not the same as seeing it work, and this is
/// the one feature where the difference is somebody else's band. Each test below
/// breaks exactly one thing and asserts that exactly its own stop comes back.
/// </para>
/// <para>**AND SEEING THEM FIRE HERE IS NOT SEEING THEM FIRE AT THE RADIO.**
/// HM-DEC-098 requires them watched into a dummy load before the antenna
/// question is even asked, and none of this is evidence about the radio
/// (HM-DEC-093).</para>
/// </remarks>
public sealed class AutoCallSafetyTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the outcomes are printed.</param>
    public AutoCallSafetyTests(ITestOutputHelper output) => _output = output;

    private const long Home = 7_030_000;

    /// <summary>The operator's own text, and Hamlet never writes one.</summary>
    private const string Call = "CQ CQ DE W1AW W1AW K";

    private static readonly AutoCallSettings Settings =
        new(Call, IntervalSeconds: 10, MaxRounds: 3);

    /// <summary>A window in which nothing was heard.</summary>
    private static Task<IReadOnlyList<CwCharacter>> HeardNothing(
        TimeSpan window, CancellationToken token)
        => Task.FromResult<IReadOnlyList<CwCharacter>>(Array.Empty<CwCharacter>());

    /// <summary>A window holding somebody answering.</summary>
    private static Task<IReadOnlyList<CwCharacter>> HeardAnAnswer(
        TimeSpan window, CancellationToken token)
        => Task.FromResult(Characters("W1AW DE K2ABC"));

    private static IReadOnlyList<CwCharacter> Characters(string text, double score = 0.95)
        => text.Select(c => c == ' '
                ? new CwCharacter(
                    MorseAlphabet.WordGap, CwConfidence.High, 1, "", 25, 18, TimeSpan.Zero)
                : new CwCharacter(
                    c.ToString(),
                    score >= 0.7 ? CwConfidence.High : CwConfidence.Low,
                    score, ".-", 25, 18, TimeSpan.Zero))
            .ToList();

    private static async Task<(AutoTestRig Rig, RigStateMonitor Monitor, RecordingSender Sender)>
        Ready()
    {
        var rig = new AutoTestRig(Home);
        var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        monitor.Start();
        await monitor.Populated.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();

        return (rig, monitor, new RecordingSender());
    }

    private AutoCaller Caller(
        AutoTestRig rig, RigStateMonitor monitor, RecordingSender sender)
        => new(rig, monitor, sender, (_, _) => Task.CompletedTask, () => rig.Now);

    private void Report(AutoCallOutcome outcome, RecordingSender sender)
    {
        _output.WriteLine($"{outcome.Cause}: {outcome.Sentence}");
        _output.WriteLine($"  {outcome.Rounds} rounds, {sender.Sent.Count} keyed, "
            + $"{sender.Aborts} aborts");
    }

    // ---- the ordinary run ------------------------------------------------

    /// <remarks>
    /// <para>The control for everything below: nobody answers, so it calls its
    /// configured number of times and stops. **A cycle that never stopped on its
    /// own would leave an unattended app calling into an empty band for an
    /// hour**, which is the reason the round limit exists and is ruled at ten.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task NobodyAnswersSoItStopsAtTheRoundLimit()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.RoundLimit, outcome.Cause);
        Assert.Equal(3, outcome.Rounds);
        Assert.Equal(3, sender.Sent.Count);

        // **THE OPERATOR'S OWN TEXT, UNCHANGED.** Nothing in Hamlet composed it
        // and nothing in Hamlet edited it on the way out beyond folding it to
        // what the keyer accepts.
        Assert.All(sender.Sent, m => Assert.Equal(Call, m));
    }

    /// <remarks>
    /// Proves the log (phase 2): every transmission carries its timestamp, the
    /// frequency the radio was on, the message and the round. **An audit trail of
    /// what the operator's callsign put on the air**, which is the least a
    /// feature that transmits unattended owes him.
    /// </remarks>
    [Fact]
    public async Task EveryTransmissionIsLoggedWithWhereAndWhatAndWhen()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        foreach (var went in outcome.Sent)
        {
            _output.WriteLine($"round {went.Round}: {went.Message} at "
                + $"{went.FrequencyLabel} MHz, {went.AtUtc:HH:mm:ss}");
        }

        Assert.Equal(3, outcome.Sent.Count);
        Assert.Equal(new[] { 1, 2, 3 }, outcome.Sent.Select(t => t.Round));
        Assert.All(outcome.Sent, t => Assert.Equal(Home, t.FrequencyHz));
        Assert.All(outcome.Sent, t => Assert.Equal(Call, t.Message));
    }

    // ---- phase 5: each interlock, broken ---------------------------------

    /// <remarks>
    /// <para>**BREAK-IN OFF AT ARM.** Footnote 2 on the command table (Full
    /// Manual p. 19-7): in CW mode a message sent with command 17 transmits only
    /// when TRANSMIT is on, an external switch is on, or break-in is on. With it
    /// off the app sends a correct frame, gets a correct acknowledgement, and the
    /// radio stays silent — **which looks exactly like success**, and is the
    /// worst outcome available here (§0.0).</para>
    /// </remarks>
    [Fact]
    public async Task BreakInOffAtArmSendsNothingAndSaysSo()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        rig.BreakIn = 0;

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.BreakInOff, outcome.Cause);
        Assert.Empty(sender.Sent);
        Assert.Contains("break-in", outcome.Sentence, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// **BREAK-IN GOING OFF MID-CYCLE**, which is the same fault arriving later:
    /// the operator turns it off between rounds and every subsequent round would
    /// be a silent non-transmission. The dead-man re-reads it before each round
    /// for exactly this.
    /// </remarks>
    [Fact]
    public async Task BreakInGoingOffMidCycleStopsAtTheNextRound()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        var sender2 = sender;
        sender.OnSend = _ =>
        {
            if (sender2.Sent.Count == 1)
            {
                rig.BreakIn = 0;
            }
        };

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.BreakInOff, outcome.Cause);
        Assert.Single(sender.Sent);
    }

    /// <remarks>
    /// **TRANSMIT STATUS STUCK ON.** The message should have finished and the
    /// radio still says it is transmitting, which is the one state that could
    /// leave a carrier on somebody else's frequency. The stop code goes out on
    /// the way past.
    /// </remarks>
    [Fact]
    public async Task ATransmitterStillOnAfterTheMessageStopsTheCycle()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        sender.OnSend = _ => rig.Transmitting = true;

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.TransmitStuck, outcome.Cause);
        Assert.Single(sender.Sent);
        Assert.True(sender.Aborts > 0, "the stop code did not go out");
    }

    /// <remarks>
    /// **THE OPERATOR'S HAND ON THE KEY.** The transmitter comes on while Hamlet
    /// is listening, well after its own message finished. It is his radio, so the
    /// cycle gets out of his way rather than adding to whatever he is doing.
    /// </remarks>
    [Fact]
    public async Task ThePttPressedWhileListeningStopsTheCycle()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        var outcome = await Caller(rig, monitor, sender).RunAsync(
            Settings,
            (_, _) =>
            {
                rig.Transmitting = true;
                return Task.FromResult<IReadOnlyList<CwCharacter>>(
                    Array.Empty<CwCharacter>());
            });

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.PttPressed, outcome.Cause);
        Assert.Single(sender.Sent);
    }

    /// <remarks>
    /// **THE DIAL MOVED.** This cycle never moves the dial, so any move at all is
    /// the operator, and he is entitled to have his radio do what he just told it
    /// to rather than transmit on a frequency Hamlet still believes it is on.
    /// </remarks>
    [Fact]
    public async Task TheDialMovedStopsTheCycle()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        sender.OnSend = _ => rig.OperatorTunesTo(Home + 5_000);

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.DialTouched, outcome.Cause);
        Assert.Single(sender.Sent);
    }

    /// <remarks>
    /// **A READING THAT WENT OUT OF DATE.** The radio has stopped volunteering
    /// where it is, so what Hamlet holds is a fact about a minute ago. It will
    /// not key a transmitter on that.
    /// </remarks>
    [Fact]
    public async Task AStaleReadingStopsTheCycle()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        // The clock moves on and the radio's reading does not.
        var caller = new AutoCaller(
            rig, monitor, sender, (_, _) => Task.CompletedTask,
            () => rig.Now + TimeSpan.FromMinutes(5));

        var outcome = await caller.RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.RigStateStale, outcome.Cause);
        Assert.Empty(sender.Sent);
    }

    /// <remarks>
    /// <para>**AN UNANSWERED DEAD-MAN READ.** The radio takes the read and says
    /// nothing back. Continuing on the previous round's reading is how a cycle
    /// keeps transmitting into a radio nobody is talking to any more, so **silence
    /// is a stop**.</para>
    /// </remarks>
    [Fact]
    public async Task AnUnansweredDeadManReadStopsTheCycle()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        rig.ReadsGoQuiet = true;

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.DeadManSilent, outcome.Cause);
        Assert.Empty(sender.Sent);
    }

    /// <remarks>
    /// <para>**THE ONE HM-DEC-098 NAMES: THE LINK FAILING OUTRIGHT MID-CYCLE.**
    /// The operator will pull the USB cable and watch what happens, so this path
    /// exists and is tested before he does. The read throws rather than answering
    /// quietly, which is what a closed port does, and the cycle stops with the
    /// stop code sent on the way out — where it reaches nothing, quietly, because
    /// an abort that could fail is not an abort.</para>
    /// </remarks>
    [Fact]
    public async Task TheLinkFailingMidCycleStopsTheCycleAndKeysTheStop()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        sender.OnSend = _ => rig.LinkIsSilent = true;

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.DeadManSilent, outcome.Cause);
        Assert.Single(sender.Sent);
        Assert.True(sender.Aborts > 0, "the stop code did not go out");
    }

    /// <remarks>
    /// **THE SEND ITSELF FAILING**, which is the other half of a broken link: the
    /// radio does not take the message. Nothing is repeated automatically and the
    /// cycle stops, because a keyer that did not answer cannot be said to have
    /// sent anything.
    /// </remarks>
    [Fact]
    public async Task ASendTheRadioDidNotTakeStopsTheCycle()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        sender.Refuse = true;

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.LinkFailed, outcome.Cause);
    }

    /// <remarks>
    /// **THE POPULATED GATE** (phase 4, HM-DEC-094). A write fired against forty
    /// fields of unknown provenance is this project's own history, and it is the
    /// same race with a transmitter attached.
    /// </remarks>
    [Fact]
    public async Task ItRefusesToStartBeforeTheRadioHasSaidWhatItIsSetTo()
    {
        var rig = new AutoTestRig(Home);
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);
        var sender = new RecordingSender();

        Assert.False(monitor.IsPopulated);

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.RigStateNotPopulated, outcome.Cause);
        Assert.Empty(sender.Sent);
    }

    /// <remarks>
    /// **MUTUALLY EXCLUSIVE WITH THE SCANNER** (HM-DEC-098). The scanner tunes
    /// the VFO and this transmits on it, so running both means transmitting
    /// mid-tune on a frequency neither component believes it is on.
    /// </remarks>
    [Fact]
    public async Task ItRefusesToRunWhileTheScannerHasTheDial()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing, scannerRunning: true);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.ScannerRunning, outcome.Cause);
        Assert.Empty(sender.Sent);
    }

    /// <remarks>
    /// **THE STOP, WHICH AWAITS NOTHING** (§0.2). It sets the flag, keys the stop
    /// code and returns, so it cannot queue behind the send it is stopping. Here
    /// it is called from inside the listening window, which is where the operator
    /// reaching for it actually is.
    /// </remarks>
    [Fact]
    public async Task TheOperatorStoppingItEndsTheCycleAndKeysTheStop()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        var caller = Caller(rig, monitor, sender);

        var outcome = await caller.RunAsync(
            Settings,
            (_, _) =>
            {
                caller.Stop();
                return Task.FromResult<IReadOnlyList<CwCharacter>>(
                    Array.Empty<CwCharacter>());
            });

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.OperatorStopped, outcome.Cause);
        Assert.Single(sender.Sent);
        Assert.True(sender.Aborts > 0, "the stop code did not go out");
    }

    /// <remarks>
    /// **A RESPONSE DETECTED**, which is what the whole cycle is for. It stops
    /// after the round it heard the answer in, so the other operator is not
    /// called over while he is replying.
    /// </remarks>
    [Fact]
    public async Task SomebodyAnsweringStopsTheCycle()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardAnAnswer);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.Answered, outcome.Cause);
        Assert.Single(sender.Sent);
        Assert.NotNull(outcome.Heard);
        Assert.True(outcome.Heard!.IsAnswer);
    }

    /// <remarks>
    /// **AND THE ABORT GOES OUT ON EVERY EXIT, INCLUDING THE ORDINARY ONE.** It
    /// costs one frame and it is the difference between a cycle that ended and a
    /// cycle that ended while a message was still going out.
    /// </remarks>
    [Fact]
    public async Task EveryExitKeysTheStopCode()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        var outcome = await Caller(rig, monitor, sender)
            .RunAsync(Settings, HeardNothing);

        Report(outcome, sender);

        Assert.Equal(AutoCallStop.RoundLimit, outcome.Cause);
        Assert.True(sender.Aborts > 0, "an ordinary finish did not key the stop");
    }

    // ---- what may never happen -------------------------------------------

    /// <remarks>
    /// <para>**THE TRIPWIRE, AND IT IS THE POINT OF THE WHOLE FILE.** Every path
    /// above ran to completion and **not one of them reached a real transmitter**:
    /// the keying calls all land on a recording fake. What this asserts is that
    /// the rig underneath never had `SendCwAsync` called on it at all, so no test
    /// in this suite can key anything even if the sender is later rewired
    /// (§0.2).</para>
    /// </remarks>
    [Fact]
    public async Task NoTestInThisSuiteCanReachARealTransmitter()
    {
        var (rig, monitor, sender) = await Ready();
        using var _ = monitor;

        await Caller(rig, monitor, sender).RunAsync(Settings, HeardNothing);

        _output.WriteLine($"rig keying attempts: {rig.KeyingAttempts}");

        Assert.Equal(0, rig.KeyingAttempts);
    }

    /// <summary>
    /// A sender that records what would have gone out and keys nothing.
    /// </summary>
    /// <remarks>
    /// Hand-rolled rather than a mocking framework (§6). It stands exactly where
    /// `KeyerCwSender` stands, so the cycle above is the real cycle.
    /// </remarks>
    private sealed class RecordingSender : ICwSender
    {
        private readonly List<string> _sent = new();

        public IReadOnlyList<string> Sent => _sent;

        public int Aborts { get; private set; }

        /// <summary>Set to make the radio refuse the message.</summary>
        public bool Refuse { get; set; }

        /// <summary>Run when a message goes out, so a test can break something.</summary>
        public Action<string>? OnSend { get; set; }

        public bool SupportsCharacterSpacing => false;

        public int MaximumMessageLength => CwMessage.MaximumLength;

        public string PathName => "a fake that keys nothing";

        public Task<CwSendResult> SendAsync(
            string message, CancellationToken cancellationToken = default)
        {
            if (Refuse)
            {
                return Task.FromResult(new CwSendResult(
                    CwSendOutcome.NoAnswer, "the radio did not take that", 0, 1));
            }

            _sent.Add(message);
            OnSend?.Invoke(message);

            return Task.FromResult(new CwSendResult(CwSendOutcome.Sent, "", 1, 1));
        }

        public void Abort() => Aborts++;
    }

    /// <summary>
    /// A radio that answers reads and counts any attempt to key it.
    /// </summary>
    private sealed class AutoTestRig : IRig
    {
        public AutoTestRig(long frequencyHz) => FrequencyHz = frequencyHz;

        public long FrequencyHz { get; private set; }

        public bool Transmitting { get; set; }

        /// <summary>Break-in, as the radio reports it: 0 off, 1 semi, 2 full.</summary>
        public int BreakIn { get; set; } = 2;

        /// <summary>Set to make reads answer "unknown" without throwing.</summary>
        public bool ReadsGoQuiet { get; set; }

        /// <summary>Set to make the port throw, which is what a pulled cable does.</summary>
        public bool LinkIsSilent { get; set; }

        /// <summary>Any attempt to key the real transmitter (§0.2).</summary>
        public int KeyingAttempts { get; private set; }

        public DateTime Now { get; } = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

        public bool IsConnected => true;

        public bool IsSimulated => true;

        public RigCapabilities Capabilities { get; } = new(
            "Auto-call test radio", false, true, true, true, new[] { "40 m" });

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        /// <summary>A hand on the knob.</summary>
        public void OperatorTunesTo(long hz)
        {
            FrequencyHz = hz;
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(hz));
        }

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(FrequencyHz);

        public Task SetFrequencyHzAsync(
            long frequencyHz, CancellationToken cancellationToken = default)
        {
            FrequencyHz = frequencyHz;
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(frequencyHz));

            return Task.CompletedTask;
        }

        /// <summary>**THE TRIPWIRE.** Nothing in this suite may reach it (§0.2).</summary>
        public Task<bool> SendCwAsync(
            string message, CancellationToken cancellationToken = default)
        {
            KeyingAttempts++;
            return Task.FromResult(false);
        }

        /// <summary>Counted for the same reason as the send.</summary>
        public void AbortCw() => KeyingAttempts++;

        public Task<RigWriteResult> SetSettingAsync(
            CivWrite write, int value, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("auto-call test radio"));

        public Task<RigWriteResult> SetModeAsync(
            CivMode mode, bool dataMode, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("auto-call test radio"));

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
        {
            if (LinkIsSilent)
            {
                return Task.FromException<IReadOnlyList<RigValue>>(
                    new IOException("the radio stopped answering"));
            }

            if (ReadsGoQuiet)
            {
                return Task.FromResult<IReadOnlyList<RigValue>>(new[]
                {
                    RigValue.Unknown(field, "the radio said nothing"),
                });
            }

            RigValue value = field switch
            {
                RigField.Frequency => RigValue.Known(
                    field, FrequencyHz, $"{FrequencyHz / 1_000_000.0:0.000} MHz",
                    Now, "auto-call test radio"),
                RigField.TransmitStatus => RigValue.Known(
                    field, Transmitting ? 1 : 0,
                    Transmitting ? "transmitting" : "receiving", Now,
                    "auto-call test radio"),
                RigField.BreakIn => RigValue.Known(
                    field, BreakIn, BreakIn == 0 ? "off" : "on", Now,
                    "auto-call test radio"),
                RigField.KeyerSpeed => RigValue.Known(
                    field, 20, "20 wpm", Now, "auto-call test radio"),
                _ => RigValue.Known(field, 0, "0", Now, "auto-call test radio"),
            };

            return Task.FromResult<IReadOnlyList<RigValue>>(new[] { value });
        }

        /// <summary>Unused here, and present because the seam requires it.</summary>
        public void Volunteer(params RigValue[] values)
            => ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(values));
    }
}
