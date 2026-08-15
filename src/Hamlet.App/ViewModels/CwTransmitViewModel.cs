using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.App.ViewModels;

/// <summary>What a send control is doing (HM-DEC-079, HM-DEC-083).</summary>
/// <remarks>
/// <para>GREY MEANS UNPRESSABLE, and that is the rule HM-DEC-079 settled. What
/// changed is that sending is now one of the things you cannot press, so it
/// wears the same look rather than one of its own: you cannot send while
/// sending, which is self-explanatory, and the status text says what is
/// happening (HM-DEC-083, Tim).</para>
/// <para>Armed is the state that still needs its own appearance, because it is
/// pressable and it is the press that matters.</para>
/// </remarks>
public enum SendState
{
    /// <summary>It can be pressed and will send. Active.</summary>
    Ready,

    /// <summary>Edited text waiting for a confirming press. Active.</summary>
    Armed,

    /// <summary>A message is going out. Active, with an abort beside it.</summary>
    Sending,

    /// <summary>Readiness refused. **The only state that may look disabled.**</summary>
    Refused,
}

/// <summary>One send button, with what it would actually send.</summary>
public sealed partial class SendButtonViewModel : ObservableObject
{
    /// <summary>Wraps one option from the script.</summary>
    /// <param name="option">What it would send.</param>
    public SendButtonViewModel(SendOption option)
    {
        Option = option;
        Label = option.Label;
        Original = option.Message;
        _message = option.Message;
        Meaning = option.Meaning;
        Note = option.Note;
        Pieces = option.Pieces;
    }

    /// <summary>The option behind it.</summary>
    public SendOption Option { get; }

    /// <summary>What the button says when nothing is in the way.</summary>
    public string Label { get; }

    /// <summary>
    /// Exactly what Hamlet wrote, before anybody touched it.
    /// </summary>
    /// <remarks>
    /// Kept so that editing back to it clears the guard (HM-DEC-079). Somebody
    /// who changes his mind and deletes back to the original has not written
    /// anything, and asking him to confirm Hamlet's own words would be the
    /// guard firing on the case it exists to skip.
    /// </remarks>
    public string Original { get; }

    /// <summary>Exactly what would go out, as it stands now.</summary>
    [ObservableProperty]
    private string _message;

    /// <summary>The same thing in plain English.</summary>
    public string Meaning { get; }

    /// <summary>Why it is like that.</summary>
    public string Note { get; }

    /// <summary>
    /// How many keyer messages it takes, so a long one is never a surprise.
    /// </summary>
    public int Pieces { get; }

    /// <summary>
    /// True when the operator has written something Hamlet did not.
    /// </summary>
    /// <remarks>
    /// THE WHOLE OF WHAT THE GUARD GUARDS (HM-DEC-079). Compared against the
    /// original rather than tracked as "was edited", so reverting genuinely
    /// un-arms it.
    /// </remarks>
    public bool IsEdited
        => !string.Equals(Message.Trim(), Original.Trim(), StringComparison.Ordinal);

    /// <summary>True when this one is waiting for its confirming press.</summary>
    [ObservableProperty]
    private bool _isArmed;

    /// <summary>What this control is doing, which decides how it looks.</summary>
    [ObservableProperty]
    private SendState _state = SendState.Ready;

    /// <summary>
    /// True when this control cannot be pressed, which is the only thing grey
    /// follows.
    /// </summary>
    /// <remarks>
    /// <para>The style binds here rather than to a negation of something else,
    /// so a future state cannot acquire the disabled look by being "not ready"
    /// (HM-DEC-079).</para>
    /// <para>SENDING IS NOW ONE OF THEM (HM-DEC-083). You cannot send while
    /// sending, so the buttons go grey for the duration and the status block
    /// says what is happening. The dedicated green treatment was solving a
    /// problem the latch had already removed, and a state that needs its own
    /// color to be understood is a state that has not been explained.</para>
    /// </remarks>
    public bool LooksRefused => State is SendState.Refused or SendState.Sending;

    /// <summary>True when this is a plain, pressable send button.</summary>
    /// <remarks>
    /// ONE FLAG PER LOOK, so the style can select on it (HM-DEC-080). The
    /// buttons had no style of their own at all and fell through to the theme's
    /// default, which is a pale grey fill in every state including the working
    /// one. Nothing was dimming them; their ordinary appearance was the
    /// problem, and three sessions went looking for a state bug that was not
    /// there.
    /// </remarks>
    public bool LooksReady => State == SendState.Ready;

    /// <summary>True when this one is waiting for its confirming press.</summary>
    public bool LooksArmed => State == SendState.Armed;

    /// <summary>
    /// True while this one is going out.
    /// </summary>
    /// <remarks>
    /// Kept so the record and the tests can tell sending from refused, and no
    /// longer bound to a style of its own (HM-DEC-083).
    /// </remarks>
    public bool LooksSending => State == SendState.Sending;

    /// <summary>What the button says right now.</summary>
    /// <remarks>
    /// THE BUTTON SAYS WHAT THE NEXT PRESS WILL DO (HM-DEC-079). "Press again to
    /// send" is the sentence that was missing: the operator pressed, saw
    /// nothing, and concluded the control was broken, because nothing on screen
    /// said a first press had armed anything.
    /// </remarks>
    public string ButtonLabel => State switch
    {
        SendState.Armed => "Press again to send",
        SendState.Sending => "Sending…",
        _ => Label,
    };

    /// <summary>One line under the meaning, or "".</summary>
    public string StateNote => "";

    /// <summary>
    /// How dim this control is. **One, unless readiness refused.**
    /// </summary>
    /// <remarks>
    /// The style binds here and to <see cref="LooksRefused"/> and to nothing
    /// else, so grey cannot be acquired by a state that merely is not ready
    /// (HM-DEC-079). Armed and sending are active and are drawn at full
    /// strength.
    /// </remarks>
    public double Dimmed => LooksRefused ? 0.45 : 1.0;

    /// <summary>
    /// The edge color for this state, from the app's own palette (HM-DEC-012).
    /// </summary>
    /// <remarks>
    /// Amber for armed, because amber is what this app already uses for the
    /// dial and for anything wanting attention. Green for sending, which is the
    /// decode family and reads as working.
    /// </remarks>
    public Avalonia.Media.IBrush EdgeBrush => State == SendState.Armed
        ? ArmedBrush
        : Controls.InstrumentPalette.IdleBrush;

    /// <summary>The app's deep amber, which is what wants attention here.</summary>
    private static readonly Avalonia.Media.IBrush ArmedBrush =
        new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#8A5200"));

    partial void OnStateChanged(SendState value)
    {
        OnPropertyChanged(nameof(LooksRefused));
        OnPropertyChanged(nameof(LooksReady));
        OnPropertyChanged(nameof(LooksArmed));
        OnPropertyChanged(nameof(LooksSending));
        OnPropertyChanged(nameof(ButtonLabel));
        OnPropertyChanged(nameof(StateNote));
        OnPropertyChanged(nameof(Dimmed));
        OnPropertyChanged(nameof(EdgeBrush));
    }

    partial void OnMessageChanged(string value)
    {
        OnPropertyChanged(nameof(IsEdited));

        // EDITING BACK TO HAMLET'S WORDS DISARMS IT. The guard is about text
        // nobody has checked, and the original has been checked by definition.
        if (IsArmed && !IsEdited)
        {
            IsArmed = false;
        }
    }
}

/// <summary>
/// Sending Morse: what to offer, what it will say, and what stands in the way
/// (HM-DEC-059).
/// </summary>
/// <remarks>
/// <para>THE TERROR IS NOT THE RADIO, IT IS NOT KNOWING WHAT TO SAY
/// (HM-DEC-043). So this offers the one or two things anybody would send next
/// rather than the whole ritual at once, shows exactly what would go out, and
/// lets somebody read it before it goes. The staged toggle defaults on for that
/// reason: somebody who can read the words first will press the button at all.
/// </para>
/// <para>EVERYTHING THAT KEYS GOES THROUGH <see cref="CwTransmitter"/>, which
/// calls the privilege guard and checks the break-in precondition before it
/// touches the radio. Nothing here reaches around it, and there is no second
/// path (§0.2).</para>
/// <para>The abort is a command with no await in front of it, bound to a button
/// that is visible the whole time a send is in flight.</para>
/// </remarks>
public sealed partial class CwTransmitViewModel : ObservableObject
{

    /// <summary>
    /// What Hamlet says when it does not know the operator's license class.
    /// </summary>
    /// <remarks>
    /// <para>CONFIRMS HM-DEC-029 RATHER THAN CHANGING IT (HM-DEC-065). An
    /// unresolved class warns and labels, and it never blocks. Hamlet has no
    /// business refusing to key somebody's radio because a lookup service did
    /// not answer, and the guard is unchanged: it says what it does not know and
    /// gets out of the way.</para>
    /// <para>Said once, beside the buttons, where somebody reads it before they
    /// press rather than after. It is a statement of what Hamlet does not know,
    /// which is a fact about Hamlet and not about them, so there is nothing here
    /// telling anybody what to do with their own license (§0.7).</para>
    /// </remarks>
    public const string UnresolvedLicenseNote =
        "Hamlet does not know which class this callsign holds, so it cannot check "
        + "this frequency against what your license allows. Nothing is stopping "
        + "you and nothing is checking either. You hold the license and you know "
        + "what it says, so satisfy yourself you are allowed here before you send. "
        + "Setting your class in Settings gives Hamlet enough to check from then on.";

    private readonly Func<TransmitContext> _context;
    private readonly Action<SendOption>? _wentOut;
    private readonly Action<CwReadiness, TransmitContext, string>? _readinessChanged;
    private readonly Action<bool, CwReadiness?>? _sendEnabledChanged;
    private readonly Action<string, TransmitContext>? _sendStarted;
    private readonly Action<string, TransmitContext, TransmitOutcome?>? _sendFinished;
    private readonly Action? _swrMeasured;
    private readonly Func<double?>? _keyedSeconds;
    private readonly Func<int?>? _skimmersListening;
    private readonly Func<string>? _bandName;
    private readonly Action<TransmitEvidence>? _chainReported;

    /// <summary>
    /// The last verdict written to the record, so an unchanged one is not
    /// written again every second (HM-DEC-077).
    /// </summary>
    /// <remarks>
    /// The evaluation fires when readiness recomputes rather than when somebody
    /// presses, which is the whole point. It does not fire when the answer is
    /// the same as last time, because a file with two thousand identical rows
    /// is a file nobody reads and the transitions are the diagnosis.
    /// </remarks>
    private CwReadyState? _lastLogged;
    private CwTransmitter? _transmitter;

    /// <summary>Creates the panel over a supplier of the current state.</summary>
    /// <param name="context">
    /// How to read everything the guard and the precondition need, at the moment
    /// somebody presses.
    /// </param>
    /// <param name="wentOut">
    /// Called when something actually reached the air, so the rest of the app
    /// can start watching for whoever heard it (HM-DEC-075).
    /// </param>
    /// <param name="readinessChanged">
    /// Called whenever the transmit precondition verdict changes, so the record
    /// carries every refusal whether or not a button was pressed (HM-DEC-077).
    /// </param>
    /// <param name="sendEnabledChanged">
    /// Called when the send buttons become usable or stop being usable, so the
    /// record carries what the operator saw and not only what the engine
    /// decided (HM-DEC-078).
    /// </param>
    /// <param name="sendStarted">
    /// Called the moment a message goes to the radio, so the record says what
    /// the radio was asked to do and not only what the gate decided
    /// (HM-DEC-079).
    /// </param>
    /// <param name="sendFinished">
    /// Called when it completed, failed or was aborted.
    /// </param>
    /// <param name="swrMeasured">
    /// Called the first time a send produces a real SWR reading, so the fact can
    /// be persisted (HM-DEC-081).
    /// </param>
    /// <param name="keyedSeconds">How long the radio keyed, or null.</param>
    /// <param name="skimmersListening">
    /// How many skimmers were reporting on this band, or null when it could not
    /// be obtained (HM-DEC-082).
    /// </param>
    /// <param name="bandName">Which band, for the sentence.</param>
    /// <param name="chainReported">
    /// Called with everything measured about the send, so it can be kept.
    /// </param>
    public CwTransmitViewModel(
        Func<TransmitContext> context,
        Action<SendOption>? wentOut = null,
        Action<CwReadiness, TransmitContext, string>? readinessChanged = null,
        Action<bool, CwReadiness?>? sendEnabledChanged = null,
        Action<string, TransmitContext>? sendStarted = null,
        Action<string, TransmitContext, TransmitOutcome?>? sendFinished = null,
        Action? swrMeasured = null,
        Func<double?>? keyedSeconds = null,
        Func<int?>? skimmersListening = null,
        Func<string>? bandName = null,
        Action<TransmitEvidence>? chainReported = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _wentOut = wentOut;
        _readinessChanged = readinessChanged;
        _sendEnabledChanged = sendEnabledChanged;
        _sendStarted = sendStarted;
        _sendFinished = sendFinished;
        _swrMeasured = swrMeasured;
        _keyedSeconds = keyedSeconds;
        _skimmersListening = skimmersListening;
        _bandName = bandName;
        _chainReported = chainReported;
        Options = new ObservableCollection<SendButtonViewModel>();
        Rebuild();
    }

    /// <summary>What Hamlet is offering to send, right now.</summary>
    public ObservableCollection<SendButtonViewModel> Options { get; }

    /// <summary>Where the contact has got to.</summary>
    [ObservableProperty]
    private ContactStage _stage = ContactStage.Calling;

    /// <summary>The operator's callsign.</summary>
    [ObservableProperty]
    private string _yourCall = "";

    /// <summary>Who they are working, or "".</summary>
    [ObservableProperty]
    private string _theirCall = "";

    /// <summary>The report to send.</summary>
    [ObservableProperty]
    private string _report = "579";

    /// <summary>Where the operator is, as they would send it.</summary>
    [ObservableProperty]
    private string _qth = "";

    /// <summary>
    /// Ask for a confirming press on everything, not only edited text.
    /// </summary>
    /// <remarks>
    /// <para>OFF BY DEFAULT, WHICH REVERSES HM-DEC-059 AND IS THE POINT
    /// (HM-DEC-079). "Let me read it first" was on by default and made every
    /// send take two presses with nothing on screen saying so. The operator
    /// pressed, saw nothing, and concluded the control was broken.</para>
    /// <para>It survives as an option rather than being deleted, because
    /// somebody who wants to confirm everything should be able to say so. What
    /// it may not do is describe a behavior the app no longer has.</para>
    /// </remarks>
    [ObservableProperty]
    private bool _alwaysConfirm;

    /// <summary>What is staged and waiting for a second press, or "".</summary>
    [ObservableProperty]
    private string _staged = "";

    /// <summary>True while something is going out.</summary>
    /// <summary>
    /// True while a message is going out, latched on the send itself.
    /// </summary>
    /// <remarks>
    /// DELIBERATELY NOT PART OF <see cref="CanPress"/> (HM-DEC-079). Sending is
    /// an active state and must not wear the disabled look, so the command stays
    /// executable and the double-send is prevented inside the handler instead.
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusBackground))]
    [NotifyPropertyChangedFor(nameof(StatusEdge))]
    private bool _isSending;

    /// <summary>What just happened, or what stands in the way.</summary>
    [ObservableProperty]
    private string _status = "";

    /// <summary>True when the status is a refusal rather than a report.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusBackground))]
    [NotifyPropertyChangedFor(nameof(StatusEdge))]
    private bool _isRefusal;

    /// <summary>
    /// The status block's fill, which changes rather than the block appearing
    /// (HM-DEC-080).
    /// </summary>
    /// <remarks>
    /// THE PANEL USED TO JUMP SEVERAL TIMES A SECOND. A message came and went
    /// as the transmit line toggled, and every appearance reflowed everything
    /// below it, which is distracting to the point of unusable at exactly the
    /// moment the operator is watching hardest. So the block is always there and
    /// only its content and its color change.
    /// </remarks>
    public Avalonia.Media.IBrush StatusBackground => IsSending
        ? GreenTint
        : IsRefusal ? AmberTint : Transparent;

    /// <summary>The status block's edge, matching its fill.</summary>
    public Avalonia.Media.IBrush StatusEdge => IsSending
        ? GreenEdge
        : IsRefusal ? AmberEdge : Transparent;

    private static readonly Avalonia.Media.IBrush AmberTint =
        new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FDF1DC"));

    private static readonly Avalonia.Media.IBrush AmberEdge =
        new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E8C88A"));

    private static readonly Avalonia.Media.IBrush GreenTint =
        new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E4F3E8"));

    private static readonly Avalonia.Media.IBrush GreenEdge =
        new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#9CC9A9"));

    private static readonly Avalonia.Media.IBrush Transparent =
        Avalonia.Media.Brushes.Transparent;

    /// <summary>The manual page or paragraph behind a refusal, or "".</summary>
    [ObservableProperty]
    private string _citation = "";

    /// <summary>
    /// What the decoder measured the other station sending at, or null.
    /// </summary>
    /// <remarks>
    /// OFFERED, NEVER ASSERTED (HM-DEC-059, HM-OPEN-006). Hamlet measured this
    /// and may say so. It has never asked what speed this operator can copy, so
    /// it may not claim any speed suits them.
    /// </remarks>
    [ObservableProperty]
    private int? _heardWpm;

    /// <summary>Whether this path can widen the gaps between characters.</summary>
    [ObservableProperty]
    private bool _supportsCharacterSpacing;

    /// <summary>
    /// True once a send here has produced a real SWR reading (HM-DEC-081).
    /// </summary>
    /// <remarks>
    /// What retires the note about the back of the radio. Evidence rather than a
    /// counter: by the time this is true Hamlet has measured something and the
    /// operator has read the number.
    /// </remarks>
    [ObservableProperty]
    private bool _hasMeasuredSwr;

    /// <summary>
    /// What the meter said during the last send, or "" (HM-DEC-081).
    /// </summary>
    public string SwrNote => _swrNote;

    /// <summary>True when the last reading wants doing something about.</summary>
    public bool SwrIsHigh => _swrHigh;

    private string _swrNote = "";
    private bool _swrHigh;

    /// <summary>The highest SWR seen during the send in flight, or null.</summary>
    private int? _swrDuringSend;

    /// <summary>
    /// The power meter's representative reading during the send, or null.
    /// </summary>
    /// <remarks>
    /// THE HIGHEST, NOT THE LAST AND NOT THE FIRST (HM-DEC-082). Both meters
    /// settle at key-down, so the first sample is a startup artifact and the last
    /// one lands as the transmitter drops. The peak across the send is the only
    /// one of the three that describes what the radio was actually doing while it
    /// was doing it. For SWR the peak is also the worst case, which is the number
    /// worth telling somebody about; for power it is the true output rather than
    /// a ramp.
    /// </remarks>
    private int? _powerDuringSend;

    /// <summary>Whether the radio was seen to key at all during the send.</summary>
    private bool _keyedDuringSend;

    /// <summary>
    /// The account of the last send, link by link (HM-DEC-082).
    /// </summary>
    public string ChainNote => _chainNote;

    private string _chainNote = "";

    /// <summary>
    /// True when a send would actually reach the air right now (HM-DEC-074).
    /// </summary>
    /// <remarks>
    /// THE BUTTONS FOLLOW THIS, which is a change from warning beside a live
    /// control. Break-in being off is not a permission Hamlet is withholding, it
    /// is a fact about the radio: the frame goes, the acknowledgement comes back,
    /// and no signal leaves the antenna. Somebody making their first call would
    /// read that as nobody wanting to talk to him. So the control that cannot
    /// work says why instead of inviting a press that produces silence.
    /// </remarks>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PressCommand))]
    private bool _canSend;

    /// <summary>
    /// Whether a send button may be pressed at all (HM-DEC-078).
    /// </summary>
    /// <remarks>
    /// THE COMMAND CARRIES THE GATE, not only the visual tree. A parent whose
    /// enabled state is bound is a picture of the rule, and a picture can be
    /// wrong: the buttons were rebuilt out from under it four times a second and
    /// nobody could tell whether the control was disabled or merely gone. A
    /// command that refuses cannot be invoked however the tree renders, and
    /// <c>NotifyCanExecuteChangedFor</c> is what tells the button to ask again.
    /// </remarks>
    private bool CanPress(SendButtonViewModel? button)
        => CanSend && button is not null;

    /// <summary>
    /// What Hamlet says beside the buttons about the radio itself.
    /// </summary>
    /// <remarks>
    /// What is on the antenna socket, and the power setting where it is worth a
    /// sentence. Consequences and never instructions (HM-DEC-074).
    /// </remarks>
    public ObservableCollection<string> Notes { get; } = new();

    /// <summary>
    /// True when Hamlet has no license class to check this frequency against.
    /// </summary>
    /// <remarks>
    /// Drives the label and nothing else (HM-DEC-065). No send button reads it,
    /// no button is disabled by it, and the guard never sees it: it decides for
    /// itself, from the class it is passed.
    /// </remarks>
    [ObservableProperty]
    private bool _licenseUnresolved;

    /// <summary>
    /// The panel's collapsed summary (§0.5).
    /// </summary>
    /// <remarks>
    /// IT USED TO READ "2 to send", WHICH IS TECHNICALLY HONEST AND COMPLETELY
    /// OPAQUE (HM-DEC-079). The operator read it as a count of presses rather
    /// than a count of messages, which is a reasonable reading and the wrong
    /// one, and it cost an evening. It now says what the messages are for.
    /// </remarks>
    public string Summary
    {
        get
        {
            if (IsSending)
            {
                return "sending now";
            }

            if (Options.Count == 0)
            {
                return "nothing to send yet";
            }

            if (!CanSend)
            {
                return $"{StageName} · not ready to send";
            }

            return Options.Count == 1
                ? $"{StageName} · one thing to say"
                : $"{StageName} · {Options.Count} things you could say";
        }
    }

    /// <summary>What to call the current stage on screen.</summary>
    public string StageName => Stage switch
    {
        ContactStage.Calling => "Calling",
        ContactStage.Answering => "Answering",
        ContactStage.Exchanging => "The exchange",
        ContactStage.Confirming => "Confirming",
        _ => "Signing off",
    };

    /// <summary>
    /// What Hamlet can say about the other station's speed, or "".
    /// </summary>
    /// <remarks>
    /// A measurement and an offer, and nothing about whether it suits anybody.
    /// "24 words a minute" is what the decoder read; "24 is fine for you" would
    /// be a claim against a number nobody has ever taken (HM-OPEN-006).
    /// </remarks>
    public string SpeedOffer => HeardWpm is { } wpm
        ? $"They are sending at about {wpm} words a minute. You can set the radio's "
          + "keyer to match, or to whatever you would rather send at."
        : "";

    /// <summary>
    /// What Hamlet cannot do about character spacing, said plainly.
    /// </summary>
    /// <remarks>
    /// AN EXPLICIT KNOWN-UNKNOWN AND NOT A HIDDEN ABSENCE (HM-DEC-059, §0.0).
    /// There is no Farnsworth control here that silently does nothing. There is
    /// a sentence saying the spacing belongs to the radio, which is true, and it
    /// will be replaced by a control the day the USB keying path lands.
    /// </remarks>
    public string SpacingNote => SupportsCharacterSpacing
        ? ""
        : "The speed and the spacing are the radio's own, set on its keyer. Hamlet "
          + "cannot widen the gaps between characters yet, which is the trick that "
          + "makes a letter arrive as one shape instead of a row of beeps to count. "
          + "That waits on a second way of keying the radio.";

    /// <summary>Point the panel at a radio, or at nothing.</summary>
    /// <param name="transmitter">The transmitter, or null when disconnected.</param>
    public void Attach(CwTransmitter? transmitter)
    {
        _transmitter = transmitter;
        SupportsCharacterSpacing = transmitter?.SupportsCharacterSpacing ?? false;
        ClearStaged();
        Refresh();
    }

    /// <summary>Recompute what is on offer, and whether it could go out.</summary>
    public void Refresh()
    {
        Rebuild();

        // Read before the early returns, so the label is right whether or not
        // anything is connected. Not knowing somebody's class is true of the
        // callsign and not of the radio.
        LicenseUnresolved = _context().LicenseClass == LicenseClass.Unknown;

        var context = _context();

        Notes.Clear();
        foreach (var note in TransmitNotes.For(context.State))
        {
            Notes.Add(note);
        }

        if (_transmitter is null)
        {
            if (CanSend)
            {
                CanSend = false;
                _sendEnabledChanged?.Invoke(false, null);
            }

            SetStatus("There is no radio connected, so there is nothing to send with.",
                refusal: true, citation: "");
            return;
        }

        // SENDING IS A STATE, NOT A PER-ELEMENT SAMPLE (HM-DEC-079). Under full
        // break-in the transmit line toggles on every element, so readiness
        // refuses "already transmitting" dozens of times across one eighteen
        // second call. Recomputing through that flipped the controls enabled and
        // disabled on every dah and lost clicks into the disabled frames. The
        // latch is the send operation itself: returning to ready wants the
        // message to finish, not a gap between elements.
        if (IsSending)
        {
            // SAMPLED WHILE THE TRANSMITTER IS ACTUALLY KEYING (HM-DEC-081),
            // which is the only time the meter is measuring anything. The worst
            // of the send is kept rather than the last, because one bad moment
            // is the thing worth telling somebody about.
            if (context.State[RigField.Swr] is { IsKnown: true, Number: { } level })
            {
                _swrDuringSend = _swrDuringSend is { } worst
                    ? Math.Max(worst, (int)level)
                    : (int)level;
            }

            // LINK 3: THE PROOF THAT RF LEFT THE RADIO (HM-DEC-082). A radio can
            // key, acknowledge and produce nothing, and until this was sampled
            // Hamlet's account stopped one link short of the only one that
            // decides whether a station is broken or a band is quiet.
            if (context.State[RigField.PowerOut] is { IsKnown: true, Number: { } watts })
            {
                _powerDuringSend = _powerDuringSend is { } peak
                    ? Math.Max(peak, (int)watts)
                    : (int)watts;
            }

            if (context.State.IsTransmitting)
            {
                _keyedDuringSend = true;
            }

            ApplyState();
            return;
        }

        var check = _transmitter.Check(context);

        var was = CanSend;

        CanSend = check.Sent;

        // WHAT THE OPERATOR ACTUALLY SAW, WHICH IS THE THING THE RECORD COULD
        // NOT SEE (HM-DEC-078, §0.0.1). The log said Ready while the screen said
        // no, and nothing anywhere could show that disagreement. So the button's
        // own state is reported beside the engine's verdict, and the two can be
        // compared in one file.
        if (was != CanSend)
        {
            _sendEnabledChanged?.Invoke(CanSend, check.Readiness);
        }

        // A REFUSAL WITH NOBODY PRESSING ANYTHING STILL GOES IN THE RECORD
        // (HM-DEC-077). A disabled button fires no handler, so the evening this
        // was written produced no event at all for the thing that went wrong.
        if (check.Readiness is { } readiness && _lastLogged != readiness.State)
        {
            _lastLogged = readiness.State;
            _readinessChanged?.Invoke(readiness, context, "recomputed");
        }

        if (!check.Sent)
        {
            SetStatus(check.Detail, refusal: true, citation: check.Citation);
            ApplyState();
            return;
        }

        SetStatus("", refusal: false, citation: "");
        ApplyState();
    }

    /// <summary>
    /// Put every control into the state it is actually in (HM-DEC-079).
    /// </summary>
    /// <remarks>
    /// ONE PLACE, so the three appearances cannot drift apart and so nothing
    /// can acquire the refused look by accident. Grey follows
    /// <see cref="SendButtonViewModel.LooksRefused"/> and that follows this.
    /// </remarks>
    private void ApplyState()
    {
        foreach (var option in Options)
        {
            option.State = IsSending
                ? option.State == SendState.Sending
                    ? SendState.Sending
                    : SendState.Ready
                : !CanSend
                    ? SendState.Refused
                    : option.IsArmed
                        ? SendState.Armed
                        : SendState.Ready;
        }
    }

    /// <summary>
    /// Press a send button: stage it, or send it.
    /// </summary>
    /// <param name="button">Which one.</param>
    /// <returns>A task that completes when the send has finished.</returns>
    [RelayCommand(CanExecute = nameof(CanPress))]
    private async Task PressAsync(SendButtonViewModel? button)
    {
        if (button is null || _transmitter is null || IsSending || !CanSend)
        {
            return;
        }

        // THE GUARD IS FOR TEXT THE OPERATOR WROTE, NOT TEXT HAMLET WROTE
        // (HM-DEC-079). Hamlet's own words are already on screen in full and
        // have already been read, so a confirming press adds nothing and costs
        // everything: the operator pressed, saw nothing happen, and concluded
        // the button was broken. He built this application and still read it
        // that way.
        if (NeedsConfirming(button) && !button.IsArmed)
        {
            ClearStaged();
            button.IsArmed = true;
            button.State = SendState.Armed;
            Staged = button.Message;
            SetStatus(
                "You have changed this one, so Hamlet is holding it. Read it "
                + "over and press again, and it goes out as it stands.",
                refusal: false, citation: "");
            return;
        }

        var message = button.Message.Trim();

        IsSending = true;
        button.State = SendState.Sending;
        OnPropertyChanged(nameof(Summary));

        var context = _context();

        _sendStarted?.Invoke(message, context);

        var outcome = default(TransmitOutcome);

        try
        {
            outcome = await _transmitter.SendAsync(message, context);

            SetStatus(
                outcome.Sent
                    ? "That went out."
                    : outcome.Detail,
                refusal: !outcome.Sent,
                citation: outcome.Citation);

            // ONLY ON A CONFIRMED SEND. Watching for reports of something that
            // never left would be Hamlet inventing the wait (§0.0).
            if (outcome.Sent)
            {
                _wentOut?.Invoke(button.Option);
            }
        }
        finally
        {
            IsSending = false;
            ClearStaged();

            // REPORTED AFTER THE SEND, WHICH IS WHEN IT MEANS ANYTHING
            // (HM-DEC-081). Nothing is said when nothing was measured, rather
            // than a resting figure being shown as a current one.
            _swrNote = SwrReport.Describe(_swrDuringSend);
            _swrHigh = SwrReport.IsHigh(_swrDuringSend);

            // THE SENTENCE THIS APPLICATION EXISTS FOR (HM-DEC-082). Speaking
            // into the void and speaking on the air with nobody listening are
            // different facts, and until now they both came out as silence.
            var evidence = new TransmitEvidence(
                Acknowledged: outcome?.Result?.Worked == true,
                KeyedSeconds: _keyedSeconds?.Invoke(),
                PowerReading: _powerDuringSend,
                SwrReading: _swrDuringSend,
                Reports: 0,
                SkimmersListening: _skimmersListening?.Invoke(),
                BandName: _bandName?.Invoke() ?? "");

            _chainNote = TransmitChain.Describe(evidence with
            {
                KeyedSeconds = evidence.KeyedSeconds
                    ?? (_keyedDuringSend ? null : 0),
            });

            _chainReported?.Invoke(evidence);

            OnPropertyChanged(nameof(ChainNote));

            if (_swrDuringSend is not null && !HasMeasuredSwr)
            {
                HasMeasuredSwr = true;
                _swrMeasured?.Invoke();
            }

            _swrDuringSend = null;
            _powerDuringSend = null;
            _keyedDuringSend = false;

            OnPropertyChanged(nameof(SwrNote));
            OnPropertyChanged(nameof(SwrIsHigh));

            _sendFinished?.Invoke(message, context, outcome);
            Refresh();
            OnPropertyChanged(nameof(Summary));
        }
    }

    /// <summary>
    /// Whether this message needs a confirming press.
    /// </summary>
    /// <param name="button">The one being pressed.</param>
    /// <returns>True when the operator has written something Hamlet did not.</returns>
    /// <remarks>
    /// EDITED TEXT ONLY (HM-DEC-079), unless somebody has explicitly asked for
    /// the old behavior. That is the whole ruling: the press guards what nobody
    /// has checked, and Hamlet's own words have been checked by being on screen.
    /// </remarks>
    private bool NeedsConfirming(SendButtonViewModel button)
        => AlwaysConfirm || button.IsEdited;

    /// <summary>
    /// Stop whatever is going out, now.
    /// </summary>
    /// <remarks>
    /// Deliberately not async (§0.2). It runs on the thread that pressed it and
    /// awaits nothing, because a stop that waits its turn is not a stop.
    /// </remarks>
    [RelayCommand]
    private void Abort()
    {
        _transmitter?.Abort();
        ClearStaged();
        SetStatus("Stopped.", refusal: false, citation: "");
    }

    /// <summary>Throw away anything waiting for a second press.</summary>
    [RelayCommand]
    private void Cancel()
    {
        ClearStaged();
        SetStatus("", refusal: false, citation: "");
    }

    partial void OnStageChanged(ContactStage value) => Refresh();

    partial void OnAlwaysConfirmChanged(bool value) => ClearStaged();

    partial void OnHeardWpmChanged(int? value) => OnPropertyChanged(nameof(SpeedOffer));

    partial void OnSupportsCharacterSpacingChanged(bool value)
        => OnPropertyChanged(nameof(SpacingNote));

    /// <summary>
    /// Rebuild the offered messages, and only when they actually changed
    /// (HM-DEC-078).
    /// </summary>
    /// <remarks>
    /// <para>THIS CLEARED AND REPOPULATED ON EVERY CALL, AND THAT WAS THE BUG
    /// THAT KILLED TWO LIVE ATTEMPTS. The rig monitor raises its state event
    /// every poll cycle whether anything changed or not, four times a second,
    /// and every one of those reaches here. So the send buttons were destroyed
    /// and constructed again four times a second, and a click cannot survive
    /// that: a press and its release have to land on the same control, and the
    /// control the press landed on was gone inside 250 milliseconds. The button
    /// looked dead because it was, repeatedly, and no handler ever ran so
    /// nothing was written and the record showed a healthy engine.</para>
    /// <para>It also wiped a staged message on the same cadence, so composing
    /// first and sending on a second press could never have worked either
    /// (HM-DEC-059).</para>
    /// <para>Now the options are compared to what is already on screen and left
    /// alone when they match, which is the same rule the spot list already
    /// follows so a surviving card keeps its identity (HM-DEC-025).</para>
    /// </remarks>
    private void Rebuild()
    {
        var offered = ContactScript.Offer(Stage, YourCall, TheirCall, Report, Qth);

        if (!HasChanged(offered))
        {
            return;
        }

        Options.Clear();
        foreach (var option in offered)
        {
            Options.Add(new SendButtonViewModel(option));
        }

        OnPropertyChanged(nameof(StageName));
        OnPropertyChanged(nameof(Summary));
    }

    /// <summary>Whether the offered messages differ from what is on screen.</summary>
    /// <param name="offered">What the script says to offer now.</param>
    /// <returns>True when the buttons have to be rebuilt.</returns>
    /// <remarks>
    /// By what would actually go out, because that is what a button is. Two
    /// options with the same label and the same message are the same button to
    /// everybody who matters, and replacing one with the other costs a click.
    /// </remarks>
    private bool HasChanged(IReadOnlyList<SendOption> offered)
    {
        if (offered.Count != Options.Count)
        {
            return true;
        }

        for (var i = 0; i < offered.Count; i++)
        {
            // AGAINST THE ORIGINAL, NEVER AGAINST THE EDITED TEXT. The message
            // is editable now (HM-DEC-079), so comparing what the script offers
            // against what is in the box would rebuild the moment somebody
            // typed, and the rebuild would throw their words away four times a
            // second. What decides a rebuild is the script changing its mind,
            // not the operator changing his.
            if (!string.Equals(
                    offered[i].Message, Options[i].Original, StringComparison.Ordinal)
                || !string.Equals(
                    offered[i].Label, Options[i].Label, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private void ClearStaged()
    {
        foreach (var option in Options)
        {
            option.IsArmed = false;
        }

        Staged = "";
    }

    /// <summary>
    /// Step back from an armed message without sending it (HM-DEC-079).
    /// </summary>
    /// <remarks>
    /// THERE WAS NO WAY BACK OUT. An armed message could only be resolved by
    /// pressing the very thing somebody was unsure about, which is the opposite
    /// of what a confirming press is for.
    /// </remarks>
    [RelayCommand]
    private void Disarm()
    {
        ClearStaged();
        SetStatus("", refusal: false, citation: "");
        ApplyState();
    }

    /// <summary>Put an edited message back to Hamlet's own words.</summary>
    [RelayCommand]
    private void Revert(SendButtonViewModel? button)
    {
        if (button is null)
        {
            return;
        }

        button.Message = button.Original;
        button.IsArmed = false;
        ApplyState();
    }

    private void SetStatus(string text, bool refusal, string citation)
    {
        Status = text;
        IsRefusal = refusal;
        Citation = citation;
    }
}
