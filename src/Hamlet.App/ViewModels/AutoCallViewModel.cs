using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.App.ViewModels;

/// <summary>One transmission, as the screen shows it (phase 2).</summary>
/// <param name="Round">Which round it was.</param>
/// <param name="Line">Where it went and what went, written for a person.</param>
public sealed record AutoCallRow(int Round, string Line);

/// <summary>
/// Arming, stopping, and the facts consent is given against (phase 4, §0.2).
/// </summary>
/// <remarks>
/// <para>**ARM IS A DISTINCT STEP FROM START AND THAT IS THE WHOLE DESIGN.**
/// What the operator is consenting to is a transmission repeating under his
/// callsign while he may not be watching, and consent given by pressing a button
/// whose state he is inferring is not consent. So arming displays the facts —
/// the message, the frequency, the power, the rounds, break-in, and whether the
/// radio has said enough about itself — and starting is a second, separate act
/// against them.</para>
/// <para>**NOTHING HERE DECIDES WHETHER IT MAY TRANSMIT.** Every refusal, every
/// interlock and every stop is `AutoCaller`'s and `TransmitReadiness`'s, and this
/// reads them out. A face that made its own judgement about a transmitter would
/// be a second copy of the rules, and a second copy drifts.</para>
/// </remarks>
public sealed partial class AutoCallViewModel : ObservableObject
{
    private readonly Action<string> _say;
    private readonly Func<bool> _scannerRunning;

    private IRig? _rig;
    private RigStateMonitor? _monitor;
    private CwDecoder? _decoder;
    private AutoCaller? _caller;
    private Task? _running;

    /// <summary>Creates the face.</summary>
    /// <param name="say">How to put a line in the status bar.</param>
    /// <param name="scannerRunning">
    /// Whether the scanner has the dial. Asked rather than tracked, so the two
    /// cannot disagree about which of them is running (HM-DEC-098).
    /// </param>
    public AutoCallViewModel(Action<string> say, Func<bool>? scannerRunning = null)
    {
        _say = say;
        _scannerRunning = scannerRunning ?? (() => false);
    }

    /// <summary>The operator's own text. Hamlet never writes one.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanArm))]
    [NotifyPropertyChangedFor(nameof(MessageRefusal))]
    [NotifyPropertyChangedFor(nameof(HasMessageRefusal))]
    [NotifyPropertyChangedFor(nameof(WillSendLine))]
    private string _message = "";

    /// <summary>How long one round lasts, transmit included.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanArm))]
    [NotifyPropertyChangedFor(nameof(MessageRefusal))]
    [NotifyPropertyChangedFor(nameof(HasMessageRefusal))]
    [NotifyPropertyChangedFor(nameof(RoundsLine))]
    private double _intervalSeconds = 30;

    /// <summary>How many unanswered rounds before it gives up.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanArm))]
    [NotifyPropertyChangedFor(nameof(MessageRefusal))]
    [NotifyPropertyChangedFor(nameof(HasMessageRefusal))]
    [NotifyPropertyChangedFor(nameof(RoundsLine))]
    private int _maxRounds = 10;

    /// <summary>True once the operator has looked at the facts and armed it.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    [NotifyPropertyChangedFor(nameof(CanArm))]
    private bool _isArmed;

    /// <summary>True while it is transmitting on a cycle.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    [NotifyPropertyChangedFor(nameof(CanArm))]
    [NotifyPropertyChangedFor(nameof(Summary))]
    private bool _isCalling;

    /// <summary>The sentence that says Hamlet is transmitting, or empty.</summary>
    [ObservableProperty]
    private string _callingLine = "";

    /// <summary>Which round, written for a person, or empty.</summary>
    [ObservableProperty]
    private string _whereNow = "";

    /// <summary>How it ended, or empty.</summary>
    [ObservableProperty]
    private string _outcomeLine = "";

    /// <summary>Why it would not run, or empty.</summary>
    [ObservableProperty]
    private string _refusal = "";

    /// <summary>Every transmission that went out, newest first.</summary>
    public ObservableCollection<AutoCallRow> Sent { get; } = new();

    /// <summary>True when there is a log worth drawing.</summary>
    public bool HasSent => Sent.Count > 0;

    /// <summary>The settings as they stand.</summary>
    public AutoCallSettings Settings => new(Message, IntervalSeconds, MaxRounds);

    /// <summary>Why the message would be refused, or "".</summary>
    public string MessageRefusal => Settings.Refusal;

    /// <summary>True when there is a refusal worth drawing.</summary>
    public bool HasMessageRefusal => MessageRefusal.Length > 0;

    /// <summary>
    /// What Hamlet would do, said before it is armed (phase 4).
    /// </summary>
    /// <remarks>
    /// **THE FACTS CONSENT IS GIVEN AGAINST.** It says the message and the
    /// frequency, because those are what go on the air, and it says them from the
    /// radio's own readings rather than from what the app last assumed.
    /// </remarks>
    public string WillSendLine
    {
        get
        {
            var clean = CwMessage.Clean(Message);

            if (clean.Length == 0)
            {
                return "";
            }

            var where = _monitor?.State[RigField.Frequency] is { IsKnown: true, Number: { } hz }
                ? $" on {hz / 1_000_000.0:0.000} MHz"
                : ", on a frequency Hamlet has not read yet";

            return $"Hamlet will send \"{clean}\"{where}.";
        }
    }

    /// <summary>How many times, and how far apart.</summary>
    public string RoundsLine
        => $"Up to {MaxRounds} times, about {IntervalSeconds:0} seconds apart, "
           + $"which is around {MaxRounds * IntervalSeconds / 60.0:0.#} minutes of "
           + "calling if nobody answers.";

    /// <summary>
    /// What the radio says about whether keying would reach the air.
    /// </summary>
    /// <remarks>
    /// **BREAK-IN IS THE ARMING INTERLOCK AND IT IS SHOWN, NOT ASSUMED.** With it
    /// off, command 17 produces a correct frame, a correct acknowledgement and
    /// total silence (Full Manual p. 19-7, footnote 2), which looks exactly like
    /// success. The operator sees its state before he arms anything.
    /// </remarks>
    public string BreakInLine
    {
        get
        {
            if (_monitor is null)
            {
                return "Nothing is connected, so Hamlet has not read break-in.";
            }

            var breakIn = _monitor.State[RigField.BreakIn];

            if (!breakIn.IsKnown)
            {
                return "Hamlet has not read break-in yet, and unknown is not "
                    + "permission.";
            }

            return breakIn.Number > 0
                ? $"Break-in is {breakIn.Text}, so a keyer message will reach the air."
                : "Break-in is off. A keyer message would be sent, acknowledged, "
                  + "and never transmitted, so Hamlet will not start.";
        }
    }

    /// <summary>The power the radio is set to, as a share of its range.</summary>
    /// <remarks>
    /// **A PERCENTAGE AND NEVER A WATTAGE** (HM-DEC-082). Hamlet reads a
    /// percentage of the radio's own range and does not know what that is in
    /// watts at this frequency into this load.
    /// </remarks>
    public string PowerLine
        => _monitor?.State[RigField.RfPower] is { IsKnown: true, Text: { } text }
            ? $"The power control is at {text}."
            : "Hamlet has not read the power setting.";

    /// <summary>Whether the radio has said enough about itself to arm against.</summary>
    public string ReadyLine
        => _monitor is null
            ? "Nothing is connected."
            : _monitor.IsPopulated
                ? "The radio has said what it is set to."
                : "The radio has not finished saying what it is set to, so nothing "
                  + "can be armed against it yet.";

    /// <summary>True when arming is offered.</summary>
    public bool CanArm
        => !IsCalling && !IsArmed && _caller is not null && Settings.IsUsable;

    /// <summary>True when starting is offered.</summary>
    public bool CanStart => IsArmed && !IsCalling && _caller is not null;

    /// <summary>What a collapsed panel still says (§0.5).</summary>
    public string Summary => IsCalling
        ? $"transmitting, round {Round}"
        : IsArmed
            ? "armed, not transmitting"
            : Sent.Count > 0
                ? $"{Sent.Count} sent"
                : "not armed";

    /// <summary>Which round it is on.</summary>
    public int Round => _caller?.Round ?? 0;

    /// <summary>Attach a radio, or detach with nulls.</summary>
    /// <param name="rig">The radio.</param>
    /// <param name="monitor">What Hamlet knows about it.</param>
    /// <param name="decoder">Where the listening comes from.</param>
    public void Attach(IRig? rig, RigStateMonitor? monitor, CwDecoder? decoder)
    {
        StopNow();

        _rig = rig;
        _monitor = monitor;
        _decoder = decoder;

        _caller = rig is not null && monitor is not null
            ? new AutoCaller(rig, monitor, new KeyerCwSender(rig))
            : null;

        IsArmed = false;

        OnPropertyChanged(nameof(CanArm));
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(WillSendLine));
        OnPropertyChanged(nameof(BreakInLine));
        OnPropertyChanged(nameof(PowerLine));
        OnPropertyChanged(nameof(ReadyLine));
    }

    /// <summary>
    /// Look at the facts and arm (phase 4).
    /// </summary>
    /// <remarks>
    /// Arming transmits nothing. What it does is refuse early where the answer is
    /// already no, so the operator finds out before he has consented rather than
    /// after.
    /// </remarks>
    [RelayCommand]
    private void Arm()
    {
        Refusal = "";
        OutcomeLine = "";

        if (_scannerRunning())
        {
            Refusal = "The scanner is running. It moves the dial and this "
                + "transmits on it, so they never run together. Stop the scan "
                + "first.";

            _say(Refusal);
            return;
        }

        if (!Settings.IsUsable)
        {
            Refusal = Settings.Refusal;
            _say(Refusal);
            return;
        }

        IsArmed = true;

        _say("Armed. Nothing has been transmitted yet — press the send control "
            + "to start calling.");
    }

    /// <summary>Take the arming back.</summary>
    [RelayCommand]
    private void Disarm()
    {
        IsArmed = false;
        Refusal = "";
        _say("Disarmed. Nothing will be transmitted.");
    }

    /// <summary>
    /// Stop, now (§0.2).
    /// </summary>
    /// <remarks>
    /// **AWAITS NOTHING**, so it cannot queue behind the transmission it is
    /// stopping, and it keys the stop code itself rather than asking the loop to
    /// notice.
    /// </remarks>
    [RelayCommand]
    private void Stop() => StopNow();

    /// <summary>
    /// Stop, from anywhere, awaiting nothing (§0.2).
    /// </summary>
    /// <remarks>
    /// Separate from the command so a disconnect, a window closing or an Escape
    /// key can call it without going through a control that may not be on screen.
    /// **A stop that only exists as a button is a stop that does not exist while
    /// the button is gone.**
    /// </remarks>
    public void StopNow()
    {
        _caller?.Stop();
        IsArmed = false;
    }

    /// <summary>Start calling.</summary>
    [RelayCommand]
    private async Task StartAsync()
    {
        if (_caller is null || IsCalling || !IsArmed)
        {
            return;
        }

        if (_scannerRunning())
        {
            Refusal = "The scanner started. Hamlet will not transmit on a dial "
                + "something else is moving.";

            _say(Refusal);
            IsArmed = false;
            return;
        }

        Refusal = "";
        OutcomeLine = "";
        Sent.Clear();
        OnPropertyChanged(nameof(HasSent));

        IsCalling = true;

        CallingLine = "Hamlet is transmitting on a cycle, under your callsign. It "
            + "stops the moment somebody answers, and the stop control is beside "
            + "the connect button.";

        _caller.Transmitted += OnTransmitted;

        AutoCallOutcome outcome;

        try
        {
            var run = _caller.RunAsync(
                Settings, ListenAsync, _scannerRunning());

            _running = run;
            outcome = await run.ConfigureAwait(true);
        }
        finally
        {
            _running = null;
            _caller.Transmitted -= OnTransmitted;
            IsCalling = false;
            IsArmed = false;
            CallingLine = "";
            WhereNow = "";
        }

        OutcomeLine = outcome.Sentence;
        _say(outcome.Sentence);

        if (outcome.Cause is not AutoCallStop.Answered
            and not AutoCallStop.RoundLimit
            and not AutoCallStop.OperatorStopped)
        {
            Refusal = outcome.Sentence;
        }

        OnPropertyChanged(nameof(Summary));
    }

    /// <summary>
    /// Listen for as long as the round has left.
    /// </summary>
    /// <remarks>
    /// <para>**THE SETTLED PASS FEEDS THIS AND THE LEADING EDGE DOES NOT**
    /// (HM-DEC-096), for the same reason the scanner uses it: a provisional
    /// reading is right far more often than not, and acting on one would stop a
    /// cycle on a `CQ` that a second reading dissolves.</para>
    /// <para>**AND IT STARTS AFTER THE TRANSMISSION, NOT DURING IT.** The caller
    /// has already waited out the message and the transmit-receive hang before
    /// this is called, because the operator's own full-break-in transmission
    /// arrives as deep audio mutes and the guard reads that as a muted receiver
    /// (HM-DEC-095) — which is exactly the truncated-evidence garbage that would
    /// false-trigger a response.</para>
    /// </remarks>
    private async Task<AutoCallWindow> ListenAsync(
        TimeSpan window, CancellationToken cancellationToken)
    {
        var decoder = _decoder;

        WhereNow = $"listening for {window.TotalSeconds:0} seconds";
        OnPropertyChanged(nameof(Summary));

        if (decoder is null || window <= TimeSpan.Zero)
        {
            return AutoCallWindow.Empty;
        }

        // **A FOLLOW AND NEVER A REFINEMENT** (HM-DEC-123). The tracker settling
        // one bin over on the station it is already reading says nothing about
        // anybody arriving; going to a different station says somebody started
        // transmitting, and it says it sooner than any classifier can.
        var followsBefore = decoder.Tracker.Follows;
        var heard = new List<CwCharacter>();

        void Take(CwCharacter c) => heard.Add(c);

        decoder.CharacterSettled += Take;

        try
        {
            await Task.Delay(window, cancellationToken).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            // Stopping mid-window is ordinary. What was heard up to here is
            // still what was heard.
        }
        finally
        {
            decoder.CharacterSettled -= Take;
        }

        return new AutoCallWindow(heard, decoder.Tracker.Follows != followsBefore);
    }

    private void OnTransmitted(object? sender, AutoCallTransmission went)
    {
        var where = went.FrequencyLabel.Length > 0
            ? $" on {went.FrequencyLabel} MHz"
            : "";

        Sent.Insert(0, new AutoCallRow(
            went.Round,
            $"round {went.Round}{where} at {went.AtUtc.ToLocalTime():HH:mm:ss} — "
            + went.Message));

        WhereNow = $"round {went.Round} went out";

        OnPropertyChanged(nameof(HasSent));
        OnPropertyChanged(nameof(Summary));
    }
}
