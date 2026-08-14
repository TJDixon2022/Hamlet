using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Cw;

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
    /// The one sentence about the dummy load, shown once before a first send.
    /// </summary>
    /// <remarks>
    /// HM-DEC-008 in the app's own voice. Said once, where somebody will read it
    /// before their first send, and written as the ordinary precaution it is
    /// rather than as a warning about their competence (§0.7).
    /// </remarks>
    public const string DummyLoadNote =
        "While this is new, send into a dummy load rather than an antenna. Keying "
        + "code that has never run before is worth trying somewhere nobody else "
        + "can hear it, and it takes one cable to find out that everything works.";

    private readonly Func<TransmitContext> _context;
    private CwTransmitter? _transmitter;

    /// <summary>Creates the panel over a supplier of the current state.</summary>
    /// <param name="context">
    /// How to read everything the guard and the precondition need, at the moment
    /// somebody presses.
    /// </param>
    public CwTransmitViewModel(Func<TransmitContext> context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
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

        if (_transmitter is null)
        {
            SetStatus("There is no radio connected, so there is nothing to send with.",
                refusal: true, citation: "");
            return;
        }

        var check = _transmitter.Check(_context());

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
    [RelayCommand]
    private async Task PressAsync(SendButtonViewModel? button)
    {
        if (button is null || _transmitter is null || IsSending)
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

    private void Rebuild()
    {
        var offered = ContactScript.Offer(Stage, YourCall, TheirCall, Report, Qth);

        Options.Clear();
        foreach (var option in offered)
        {
            Options.Add(new SendButtonViewModel(option));
        }

        OnPropertyChanged(nameof(StageName));
        OnPropertyChanged(nameof(Summary));
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
