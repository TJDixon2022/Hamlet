using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Cw;

/// <summary>Why an automatic calling cycle is not running (HM-DEC-098).</summary>
/// <remarks>
/// **EVERY ONE OF THESE IS A STOP AND NONE OF THEM IS A PAUSE.** A cycle that
/// could resume itself after any of these would be deciding on the operator's
/// behalf that whatever went wrong has stopped being wrong, which is exactly the
/// judgement §0.2 refuses to let software make about a transmitter.
/// </remarks>
public enum AutoCallStop
{
    /// <summary>It has not been asked to run.</summary>
    NotStarted,

    /// <summary>It is running.</summary>
    Running,

    /// <summary>The operator stopped it.</summary>
    OperatorStopped,

    /// <summary>Somebody answered, which is the whole point.</summary>
    Answered,

    /// <summary>
    /// Something was heard that Hamlet would not read as an answer.
    /// </summary>
    /// <remarks>
    /// **A SUSPEND RATHER THAN A DISMISSAL** (phase 3). Confident text that is
    /// not QSO-shaped is somebody transmitting near enough to be read, and
    /// calling over the top of them is the failure this whole feature has to
    /// avoid. The cycle stops and says what it heard; resuming is the operator's.
    /// </remarks>
    HeardSomething,

    /// <summary>The configured number of rounds went out unanswered.</summary>
    RoundLimit,

    /// <summary>Break-in is off, so a keyer message would not reach the air.</summary>
    BreakInOff,

    /// <summary>
    /// The radio is still transmitting when it should have stopped.
    /// </summary>
    TransmitStuck,

    /// <summary>The transmitter came on while Hamlet was listening.</summary>
    /// <remarks>
    /// The operator's hand on the key or the PTT. Different from a stuck
    /// transmitter and it is his radio, so the cycle gets out of the way.
    /// </remarks>
    PttPressed,

    /// <summary>What Hamlet knows about the radio is unknown or out of date.</summary>
    RigStateStale,

    /// <summary>The dial moved, and this cycle never moves it.</summary>
    DialTouched,

    /// <summary>A between-rounds read did not answer.</summary>
    /// <remarks>
    /// **SILENCE IS A STOP.** The link going quiet while a transmitter is armed
    /// is the case with the least information and the most at stake, and
    /// continuing on state that was true a minute ago is how a stuck carrier
    /// happens.
    /// </remarks>
    DeadManSilent,

    /// <summary>The link failed outright.</summary>
    LinkFailed,

    /// <summary>The readiness check refused before anything was sent.</summary>
    NotReady,

    /// <summary>The radio has not said enough about itself yet.</summary>
    RigStateNotPopulated,

    /// <summary>The scanner is running, and the two may not overlap.</summary>
    ScannerRunning,

    /// <summary>There was nothing sendable to send.</summary>
    NothingToSend,
}

/// <summary>One transmission that went out, for the record (phase 2).</summary>
/// <param name="AtUtc">When it went.</param>
/// <param name="FrequencyHz">Where the radio was, or 0 where Hamlet did not know.</param>
/// <param name="Message">Exactly what was handed to the keyer.</param>
/// <param name="Round">Which round it was, counting from one.</param>
public sealed record AutoCallTransmission(
    DateTime AtUtc, long FrequencyHz, string Message, int Round)
{
    /// <summary>The frequency as the app writes it, or "" where it was unknown.</summary>
    public string FrequencyLabel => FrequencyHz <= 0
        ? ""
        : (FrequencyHz / 1_000_000.0).ToString(
            "0.000", System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>What a calling cycle came to.</summary>
/// <param name="Cause">Why it is not running.</param>
/// <param name="Rounds">How many transmissions went out.</param>
/// <param name="Sent">Each of them, in order.</param>
/// <param name="Heard">What ended it, where something was heard.</param>
public sealed record AutoCallOutcome(
    AutoCallStop Cause,
    int Rounds,
    IReadOnlyList<AutoCallTransmission> Sent,
    AutoCallAnswer? Heard)
{
    /// <summary>What happened, in the app's voice (§0.7).</summary>
    /// <remarks>
    /// **IT SAYS WHY IT STOPPED AND NEVER WHAT THAT MEANS ABOUT THE BAND.** A
    /// cycle that reached its round limit has not established that nobody is
    /// there, and one that stopped on a dead-man read has not established that
    /// the radio is broken (§0.0).
    /// </remarks>
    public string Sentence => Cause switch
    {
        AutoCallStop.Answered when Heard is { } answer
            => $"Hamlet stopped calling because {answer.Why}.",
        AutoCallStop.HeardSomething when Heard is { } something
            => $"Hamlet stopped calling because {something.Why}, which it would "
               + "not read as an answer. It is yours to look at.",
        AutoCallStop.RoundLimit
            => $"Hamlet called {Rounds} times and nobody answered, so it has "
               + "stopped rather than keep calling into a band it cannot hear.",
        AutoCallStop.OperatorStopped
            => "You stopped it.",
        AutoCallStop.BreakInOff
            => "Break-in went off, so a keyer message would no longer reach the "
               + "air, and Hamlet has stopped rather than send into a radio that "
               + "is still receiving.",
        AutoCallStop.TransmitStuck
            => "The radio was still transmitting when it should have finished, "
               + "so Hamlet stopped and told it to stop as well.",
        AutoCallStop.PttPressed
            => "The transmitter came on while Hamlet was listening, so it has "
               + "got out of your way.",
        AutoCallStop.DialTouched
            => "The dial moved, and this cycle never moves it, so that was you.",
        AutoCallStop.RigStateStale
            => "What Hamlet knows about the radio went out of date, and it will "
               + "not key a transmitter on a stale reading.",
        AutoCallStop.DeadManSilent
            => "The radio stopped answering between rounds. Silence is a stop.",
        AutoCallStop.LinkFailed
            => "The link to the radio failed, so Hamlet stopped and sent the "
               + "keyer's own stop code on the way out.",
        AutoCallStop.NotReady
            => "Hamlet would not have reached the air, so nothing was sent.",
        AutoCallStop.RigStateNotPopulated
            => "The radio has not finished saying what it is set to, and Hamlet "
               + "does not arm a transmitter against forty unknowns.",
        AutoCallStop.ScannerRunning
            => "The scanner is running. It moves the dial and this transmits on "
               + "it, so they never run together.",
        AutoCallStop.NothingToSend
            => "There was nothing in that message the radio's keyer could send.",
        AutoCallStop.Running => "Hamlet is calling.",
        _ => "Hamlet is not calling.",
    };
}

/// <summary>How the operator wants the cycle to run (phase 2).</summary>
/// <param name="Message">
/// **The operator's own text.** Nothing in Hamlet composes this.
/// </param>
/// <param name="IntervalSeconds">How long one round lasts, transmit included.</param>
/// <param name="MaxRounds">How many unanswered rounds before it gives up.</param>
/// <remarks>
/// <para>**NO SESSION MAY EVER INVENT THE CONTENT OF A TRANSMISSION THAT GOES
/// OUT UNDER HIS CALLSIGN.** There is no default message here and there is not
/// going to be one: an empty message is refused rather than filled in, because a
/// plausible CQ Hamlet wrote is somebody else's callsign on the air.</para>
/// <para>Thirty seconds and ten rounds are the ruled defaults. Ten rounds at
/// thirty seconds is five minutes of calling, which is a long CQ and a short
/// evening.</para>
/// </remarks>
public sealed record AutoCallSettings(
    string Message, double IntervalSeconds = 30, int MaxRounds = 10)
{
    /// <summary>The shortest round Hamlet will run.</summary>
    /// <remarks>
    /// Ten seconds. Shorter than that and a thirty-character message at a
    /// relaxed speed does not finish before the next round is due, so the cycle
    /// would be calling over its own tail.
    /// </remarks>
    public const double ShortestIntervalSeconds = 10;

    /// <summary>The most rounds Hamlet will run unattended.</summary>
    public const int MostRounds = 60;

    /// <summary>True when this is a cycle Hamlet will run.</summary>
    public bool IsUsable
        => CwMessage.Clean(Message).Length > 0
           && CwMessage.Clean(Message).Length <= CwMessage.MaximumLength
           && IntervalSeconds >= ShortestIntervalSeconds
           && MaxRounds is > 0 and <= MostRounds;

    /// <summary>
    /// Why this cycle would be refused, or "" when it would not.
    /// </summary>
    /// <remarks>
    /// **AT EDIT TIME AND NOT ON AIR** (phase 1). A message the keyer cannot
    /// take fails where the operator can see it and change it, rather than
    /// arriving as a truncated transmission under his callsign.
    /// </remarks>
    public string Refusal
    {
        get
        {
            var clean = CwMessage.Clean(Message);

            if (clean.Length == 0)
            {
                return "There is nothing in that message the radio's keyer could "
                    + "send. Hamlet does not write one for you.";
            }

            if (clean.Length > CwMessage.MaximumLength)
            {
                return $"That is {clean.Length} characters and the radio's keyer "
                    + $"takes {CwMessage.MaximumLength} in one message, so it "
                    + "would go out cut short.";
            }

            if (IntervalSeconds < ShortestIntervalSeconds)
            {
                return $"A round shorter than {ShortestIntervalSeconds:0} seconds "
                    + "does not leave time for the message to finish before the "
                    + "next one is due.";
            }

            return MaxRounds is <= 0 or > MostRounds
                ? $"Hamlet calls between one and {MostRounds} times before it stops."
                : "";
        }
    }
}

/// <summary>
/// Calling CQ over and over, into a dummy load (HM-DEC-098, §0.2).
/// </summary>
/// <remarks>
/// <para>**THIS IS THE FIRST THING IN THIS PROJECT THAT KEYS A TRANSMITTER
/// WITHOUT SOMEBODY PRESSING SOMETHING EACH TIME, AND IT GOES INTO A DUMMY LOAD
/// ONLY.** §0.2's first sentence — no unattended transmission — is not amended
/// by building this. Whether an attended automatic cycle ever reaches an antenna
/// is a separate ruling Tim takes after watching every interlock below fire into
/// the load, the USB cable pulled mid-cycle included. Reasoning about an
/// interlock is not seeing it work, and this is the one feature where the
/// difference is somebody else's band.</para>
/// <para>**THE RADIO OWNS EVERY ELEMENT'S TIMING.** Keying goes out as command
/// 17, which hands the message to the radio's own keyer; the host never holds a
/// keying line. Host-timed keying on DTR or RTS is rejected and stays rejected,
/// because it makes a PC responsible for continuous control of a transmitter it
/// cannot guarantee it will be alive to release, and RF has knocked USB devices
/// off the bus in this very shack. With 17 the worst case is one truncated
/// message already in flight.</para>
/// <para>**EVERY EXIT SENDS THE STOP CODE.** `0xFF` on command 17 stops a message
/// in progress (Full Manual p. 19-11), it is a same-thread call that awaits
/// nothing, and it goes out on the way out of every path here including the ones
/// where the link has already failed. An abort that could fail is not an abort.
/// </para>
/// <para>**SPURIOUS STOPS ARE THE CORRECT FAILURE DIRECTION.** Every guard below
/// is written to stop on doubt. A cycle that stopped when it did not need to
/// costs the operator a keypress; a cycle that carried on when it should have
/// stopped costs somebody else their frequency.</para>
/// </remarks>
public sealed class AutoCaller
{
    /// <summary>
    /// How old a reading may be before this refuses to key on it.
    /// </summary>
    /// <remarks>
    /// Four seconds, which is the scanner's own figure for the same question.
    /// A transmitter is at least as deserving of a fresh reading as a dial is.
    /// </remarks>
    public static readonly TimeSpan FreshEnough = TimeSpan.FromSeconds(4);

    /// <summary>
    /// How far the dial may move before that counts as a hand on the knob.
    /// </summary>
    /// <remarks>
    /// A hundred hertz, the scanner's own figure. **This cycle never moves the
    /// dial at all**, so anything at all is the operator; the tolerance is here
    /// only because the radio reports what it rounded to.
    /// </remarks>
    public const long DialTouchedHz = 100;

    /// <summary>
    /// How long after a message the radio is given to drop out of transmit.
    /// </summary>
    /// <remarks>
    /// A quarter of a second, comfortably past the twenty-four milliseconds of
    /// transmit-receive hang measured on this radio and past the guard's own
    /// hundred and fifty millisecond hold (HM-DEC-095). Listening before that
    /// hears the operator's own transmission as a muted receiver and reads the
    /// slivers between his elements as somebody answering, which is exactly the
    /// truncated-evidence garbage that would false-trigger a response.
    /// </remarks>
    public static readonly TimeSpan RecoverySeconds = TimeSpan.FromSeconds(0.25);

    private readonly IRig _rig;
    private readonly RigStateMonitor _monitor;
    private readonly ICwSender _sender;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;
    private readonly Func<DateTime> _utcNow;
    private readonly object _gate = new();

    private long _placedAtHz;
    private bool _knowWhereDialIs;
    private volatile bool _dialTouched;
    private bool _stopAsked;
    private CancellationTokenSource? _cancel;

    /// <summary>Creates a caller.</summary>
    /// <param name="rig">The radio.</param>
    /// <param name="monitor">What Hamlet knows about it.</param>
    /// <param name="sender">Whatever keys the transmitter.</param>
    /// <param name="delay">How to wait. Injected so tests are instant (§5).</param>
    /// <param name="utcNow">The clock, for staleness and the log.</param>
    public AutoCaller(
        IRig rig,
        RigStateMonitor monitor,
        ICwSender sender,
        Func<TimeSpan, CancellationToken, Task>? delay = null,
        Func<DateTime>? utcNow = null)
    {
        _rig = rig ?? throw new ArgumentNullException(nameof(rig));
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _delay = delay ?? Task.Delay;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>True while the cycle is running.</summary>
    public bool IsCalling { get; private set; }

    /// <summary>Why it is not running.</summary>
    public AutoCallStop Cause { get; private set; } = AutoCallStop.NotStarted;

    /// <summary>Which round it is on, counting from one.</summary>
    public int Round { get; private set; }

    /// <summary>Raised as each transmission goes out, so a surface can show it.</summary>
    public event EventHandler<AutoCallTransmission>? Transmitted;

    /// <summary>
    /// Stop, now (§0.2).
    /// </summary>
    /// <remarks>
    /// **SAME THREAD, AWAITS NOTHING, KEYS THE STOP CODE ITSELF.** The moment
    /// somebody wants a transmitter to stop is the moment they cannot wait for a
    /// task to be scheduled, so this does not merely ask the loop to notice: it
    /// sends `0xFF` on the way past. The two halves are independent and neither
    /// depends on the other working.
    /// </remarks>
    public void Stop()
    {
        lock (_gate)
        {
            _stopAsked = true;
        }

        // The stop code first, because it is the half that reaches the radio.
        _sender.Abort();

        _cancel?.Cancel();
    }

    /// <summary>
    /// Run the cycle.
    /// </summary>
    /// <param name="settings">What to send, how often, how many times.</param>
    /// <param name="listen">
    /// Listen for this long and report what was decoded. The caller supplies it
    /// so that nothing here owns a decoder or a clock (§5), and so a test can
    /// hand back an answer without synthesizing audio.
    /// </param>
    /// <param name="scannerRunning">
    /// True when the scanner has the dial. The two are mutually exclusive
    /// (HM-DEC-098).
    /// </param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>What the cycle came to. Never throws.</returns>
    public async Task<AutoCallOutcome> RunAsync(
        AutoCallSettings settings,
        Func<TimeSpan, CancellationToken, Task<IReadOnlyList<CwCharacter>>> listen,
        bool scannerRunning = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(listen);

        var sent = new List<AutoCallTransmission>();

        lock (_gate)
        {
            _stopAsked = false;
        }

        _dialTouched = false;
        _knowWhereDialIs = false;
        Round = 0;


        if (scannerRunning)
        {
            return Refuse(AutoCallStop.ScannerRunning, sent);
        }

        if (!settings.IsUsable)
        {
            return Refuse(AutoCallStop.NothingToSend, sent);
        }

        // **THE RADIO HAS TO HAVE SAID WHAT IT IS SET TO** (phase 4). A write
        // fired against forty fields of unknown provenance is this project's own
        // history, and it is the same race with a transmitter attached.
        if (!_monitor.IsPopulated)
        {
            return Refuse(AutoCallStop.RigStateNotPopulated, sent);
        }

        if (Blocked() is { } before)
        {
            return Refuse(before, sent);
        }

        var message = CwMessage.Clean(settings.Message);

        // Checked above, so this is a reading and not a guess. Anything the radio
        // reports away from it from here on is the operator.
        _placedAtHz = FrequencyNow();
        _knowWhereDialIs = _placedAtHz > 0;

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _cancel = linked;
        IsCalling = true;
        Cause = AutoCallStop.Running;

        _rig.FrequencyChanged += OnFrequencyChanged;

        try
        {
            for (var round = 1; round <= settings.MaxRounds; round++)
            {
                Round = round;

                // **THE DEAD MAN RUNS BEFORE EVERY ROUND, NOT AFTER IT** (phase
                // 4). Break-in and transmit status are re-read from the radio
                // rather than taken from the poll's own copy, because the whole
                // question is whether the radio is still answering at all.
                if (await DeadManAsync(linked.Token).ConfigureAwait(false) is { } silent)
                {
                    return Leave(silent, sent);
                }

                // **THE MORE SPECIFIC CAUSE WINS, AND GETTING THAT WRONG IS A
                // §0.0 FAULT RATHER THAN A COSMETIC ONE.** A guard that trips
                // internally calls `Stop`, which sets the same flag the operator's
                // own button sets, so testing the flag first reported that *he*
                // had stopped a cycle the dial actually stopped. The cycle halted
                // correctly either way; what was wrong was the record, and a
                // record that names the wrong reason is worth nothing on the
                // evening it is needed.
                if (Blocked() is { } during)
                {
                    return Leave(during, sent);
                }

                if (StopRequested() || linked.IsCancellationRequested)
                {
                    return Leave(AutoCallStop.OperatorStopped, sent);
                }

                CwSendResult result;

                try
                {
                    result = await _sender
                        .SendAsync(message, linked.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return Leave(AutoCallStop.OperatorStopped, sent);
                }
                catch (Exception)
                {
                    return Leave(AutoCallStop.LinkFailed, sent);
                }

                if (result.Outcome == CwSendOutcome.Aborted)
                {
                    return Leave(AutoCallStop.OperatorStopped, sent);
                }

                if (!result.Worked)
                {
                    return Leave(AutoCallStop.LinkFailed, sent);
                }

                var went = new AutoCallTransmission(
                    _utcNow(), FrequencyNow(), message, round);

                sent.Add(went);
                Transmitted?.Invoke(this, went);

                // **THE MESSAGE IS STILL GOING OUT.** Command 17 returns as soon
                // as the radio acknowledges, which is milliseconds, while the
                // radio keys for as long as the message takes (HM-DEC-085). The
                // duration is arithmetic and known before the first dit.
                var keying = CwDuration.Of(message, KeyerSpeed());

                try
                {
                    await _delay(keying + RecoverySeconds, linked.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return Leave(AutoCallStop.OperatorStopped, sent);
                }

                // Off the air by now, or it is stuck and this is the check that
                // catches a transmitter nobody asked to stay on.
                if (await StillTransmittingAsync(linked.Token).ConfigureAwait(false)
                    is { } stuck)
                {
                    return Leave(stuck, sent);
                }

                var window = TimeSpan.FromSeconds(settings.IntervalSeconds)
                    - keying - RecoverySeconds;

                if (window < TimeSpan.Zero)
                {
                    window = TimeSpan.Zero;
                }

                IReadOnlyList<CwCharacter> heard;

                try
                {
                    heard = await listen(window, linked.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return Leave(AutoCallStop.OperatorStopped, sent);
                }
                catch (Exception)
                {
                    return Leave(AutoCallStop.LinkFailed, sent);
                }

                var answer = AutoCallAnswers.Judge(heard, settings.Message);

                if (answer.Stop)
                {
                    return Leave(
                        answer.IsAnswer ? AutoCallStop.Answered : AutoCallStop.HeardSomething,
                        sent,
                        answer);
                }
            }

            return Leave(AutoCallStop.RoundLimit, sent);
        }
        finally
        {
            _rig.FrequencyChanged -= OnFrequencyChanged;
            _cancel = null;
            IsCalling = false;
        }
    }

    /// <summary>
    /// Re-read the two things that decide whether keying reaches the air.
    /// </summary>
    /// <returns>The stop, or null when both answered and both are good.</returns>
    /// <remarks>
    /// <para>**AN UNANSWERED READ IS A STOP AND NOT A RETRY.** Continuing on the
    /// previous round's reading is how a cycle keeps transmitting into a radio
    /// nobody is talking to any more.</para>
    /// <para>**WHAT SAYS THE RADIO ANSWERED IS THAT THE FIELD IS KNOWN**, and
    /// that is the monitor's own contract rather than an inference here: a read
    /// that came back empty, and a read that threw because the port is closed,
    /// both land as `RigValue.Unknown` on the field (§8). So one test covers a
    /// quiet radio and a pulled cable alike.</para>
    /// <para>**THE READING'S AGE IS DELIBERATELY NOT THE TEST.** It was, and it
    /// was wrong twice over: a refresh answering inside the same clock tick as
    /// the last one reads as silence, which stops a perfectly live cycle, and it
    /// duplicates a check the monitor already makes properly. A guard that fires
    /// on a working radio is not a safer guard, it is a broken one.</para>
    /// </remarks>
    private async Task<AutoCallStop?> DeadManAsync(CancellationToken cancellationToken)
    {
        foreach (var field in new[] { RigField.BreakIn, RigField.TransmitStatus })
        {
            try
            {
                await _monitor.RefreshAsync(field, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return AutoCallStop.OperatorStopped;
            }
            catch (Exception)
            {
                // The monitor does not throw for a failed read, so this is the
                // belt to that brace and not the expected path (§8).
                return AutoCallStop.DeadManSilent;
            }

            if (!_monitor.State[field].IsKnown)
            {
                return AutoCallStop.DeadManSilent;
            }
        }

        // **BREAK-IN IS THE ARMING INTERLOCK AND NOT A CAVEAT** (phase 1, Full
        // Manual p. 19-7 footnote 2): in CW mode a command 17 message is
        // transmitted only when TRANSMIT is on, an external switch is on, or
        // break-in is on. Off means a correct frame, a correct acknowledgement
        // and total silence, which is the worst outcome available here because
        // it looks exactly like success (§0.0).
        var breakIn = _monitor.State[RigField.BreakIn];

        return breakIn is { IsKnown: true, Number: > 0 } ? null : AutoCallStop.BreakInOff;
    }

    /// <summary>Is the radio still keyed when it should be listening?</summary>
    private async Task<AutoCallStop?> StillTransmittingAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _monitor.RefreshAsync(RigField.TransmitStatus, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return AutoCallStop.OperatorStopped;
        }
        catch (Exception)
        {
            return AutoCallStop.DeadManSilent;
        }

        return _monitor.State.IsTransmitting ? AutoCallStop.TransmitStuck : null;
    }

    /// <summary>What would stop a round before it starts.</summary>
    private AutoCallStop? Blocked()
    {
        if (_dialTouched)
        {
            return AutoCallStop.DialTouched;
        }

        var state = _monitor.State;
        var frequency = state[RigField.Frequency];

        if (!frequency.IsKnown || frequency.IsStale(_utcNow(), FreshEnough))
        {
            return AutoCallStop.RigStateStale;
        }

        // Transmitting before Hamlet asked for anything is the operator's hand
        // on the key, and this cycle gets out of his way rather than adding to
        // whatever he is doing.
        return state.IsTransmitting ? AutoCallStop.PttPressed : null;
    }

    private long FrequencyNow()
        => _monitor.State[RigField.Frequency] is { IsKnown: true, Number: { } hz }
            ? (long)hz
            : 0;

    private int KeyerSpeed()
        => _monitor.State[RigField.KeyerSpeed] is { IsKnown: true, Number: { } wpm }
            ? Math.Clamp((int)wpm, 5, 60)
            : 20;

    private bool StopRequested()
    {
        lock (_gate)
        {
            return _stopAsked;
        }
    }

    /// <summary>
    /// The dial moved, and this cycle never moves it (phase 4).
    /// </summary>
    /// <remarks>
    /// <para>**UNLIKE THE SCANNER, THERE IS NO ECHO TO ALLOW FOR.** The scanner
    /// has to tell its own tune's echo from a hand on the knob because it writes
    /// the frequency; this never writes one, so every event here is somebody
    /// else's doing.</para>
    /// <para>**AND THE BASELINE IS TAKEN WHEN THE CYCLE ARMS, NOT FROM THE FIRST
    /// EVENT.** Seeding it from the first event was measured to swallow exactly
    /// the move that matters: the operator reaches for the dial during the first
    /// transmission, that arrives as the first event, and it was consumed as
    /// "where the dial is" instead of counted as a hand on the knob. The cycle
    /// ran to its round limit with the radio somewhere nobody had checked. The
    /// frequency is already required to be known and fresh before arming, so
    /// there is a baseline to be had without waiting for the radio to volunteer
    /// one.</para>
    /// </remarks>
    private void OnFrequencyChanged(object? sender, FrequencyChangedEventArgs e)
    {
        if (!_knowWhereDialIs)
        {
            return;
        }

        if (Math.Abs(e.FrequencyHz - _placedAtHz) > DialTouchedHz)
        {
            _dialTouched = true;
            Stop();
        }
    }

    private AutoCallOutcome Refuse(AutoCallStop cause, List<AutoCallTransmission> sent)
    {
        IsCalling = false;
        Cause = cause;

        return new AutoCallOutcome(cause, sent.Count, sent, null);
    }

    /// <summary>
    /// End the cycle, with the stop code on the way out (§0.2).
    /// </summary>
    /// <remarks>
    /// **THE ABORT GOES OUT ON EVERY EXIT, INCLUDING THE ORDINARY ONES.** It
    /// costs one frame and it is the difference between a cycle that ended and a
    /// cycle that ended while a message was still going out. Where the link has
    /// already failed it does nothing, quietly, which is what an abort that
    /// cannot fail means.
    /// </remarks>
    private AutoCallOutcome Leave(
        AutoCallStop cause, List<AutoCallTransmission> sent, AutoCallAnswer? heard = null)
    {
        _sender.Abort();

        IsCalling = false;
        Cause = cause;

        return new AutoCallOutcome(cause, sent.Count, sent, heard);
    }
}
