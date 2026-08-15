using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.ViewModels;

/// <summary>One send button, with what it would actually send.</summary>
public sealed partial class SendButtonViewModel : ObservableObject
{
    /// <summary>Wraps one option from the script.</summary>
    /// <param name="option">What it would send.</param>
    public SendButtonViewModel(SendOption option)
    {
        Option = option;
        Label = option.Label;
        Message = option.Message;
        Meaning = option.Meaning;
        Note = option.Note;
        Pieces = option.Pieces;
    }

    /// <summary>The option behind it.</summary>
    public SendOption Option { get; }

    /// <summary>What the button says.</summary>
    public string Label { get; }

    /// <summary>Exactly what would go out.</summary>
    public string Message { get; }

    /// <summary>The same thing in plain English.</summary>
    public string Meaning { get; }

    /// <summary>Why it is like that.</summary>
    public string Note { get; }

    /// <summary>
    /// How many keyer messages it takes, so a long one is never a surprise.
    /// </summary>
    public int Pieces { get; }

    /// <summary>True when this is the one waiting for a second press.</summary>
    [ObservableProperty]
    private bool _isStaged;
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
    public CwTransmitViewModel(
        Func<TransmitContext> context,
        Action<SendOption>? wentOut = null,
        Action<CwReadiness, TransmitContext, string>? readinessChanged = null,
        Action<bool, CwReadiness?>? sendEnabledChanged = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _wentOut = wentOut;
        _readinessChanged = readinessChanged;
        _sendEnabledChanged = sendEnabledChanged;
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
    /// Compose first and send on a second press.
    /// </summary>
    /// <remarks>
    /// On by default (HM-DEC-059). Somebody about to make their first contact
    /// wants to read the words before they go out, and the whole point of this
    /// panel is that they press the button at all.
    /// </remarks>
    [ObservableProperty]
    private bool _readFirst = true;

    /// <summary>What is staged and waiting for a second press, or "".</summary>
    [ObservableProperty]
    private string _staged = "";

    /// <summary>True while something is going out.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PressCommand))]
    private bool _isSending;

    /// <summary>What just happened, or what stands in the way.</summary>
    [ObservableProperty]
    private string _status = "";

    /// <summary>True when the status is a refusal rather than a report.</summary>
    [ObservableProperty]
    private bool _isRefusal;

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
        => CanSend && !IsSending && button is not null;

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

    /// <summary>The panel's collapsed summary (§0.5).</summary>
    public string Summary => IsSending
        ? "sending"
        : Options.Count == 0 ? "nothing to send yet" : $"{StageName} · {Options.Count} to send";

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
            return;
        }

        SetStatus("", refusal: false, citation: "");
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

        // FIRST PRESS COMPOSES, SECOND PRESS SENDS. Nothing goes out on the
        // press that stages it, which is the whole of the toggle.
        if (ReadFirst && !button.IsStaged)
        {
            ClearStaged();
            button.IsStaged = true;
            Staged = button.Message;
            SetStatus(
                "Ready to send. Press it again and it goes out.",
                refusal: false, citation: "");
            return;
        }

        IsSending = true;
        OnPropertyChanged(nameof(Summary));

        try
        {
            var outcome = await _transmitter.SendAsync(button.Message, _context());

            SetStatus(
                outcome.Sent ? $"Sent: {button.Message}" : outcome.Detail,
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
            OnPropertyChanged(nameof(Summary));
        }
    }

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

    partial void OnReadFirstChanged(bool value) => ClearStaged();

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
            if (!string.Equals(
                    offered[i].Message, Options[i].Message, StringComparison.Ordinal)
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
            option.IsStaged = false;
        }

        Staged = "";
    }

    private void SetStatus(string text, bool refusal, string citation)
    {
        Status = text;
        IsRefusal = refusal;
        Citation = citation;
    }
}
