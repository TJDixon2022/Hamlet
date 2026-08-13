using CommunityToolkit.Mvvm.ComponentModel;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>One line of the worked contact, ready to draw.</summary>
public sealed class ContactStepViewModel
{
    /// <summary>Wraps a step.</summary>
    /// <param name="step">The engine's step.</param>
    public ContactStepViewModel(ContactStep step)
    {
        Step = step;
    }

    /// <summary>The step behind this row.</summary>
    public ContactStep Step { get; }

    /// <summary>"You" or "Them".</summary>
    public string SpeakerLabel => Step.SpeakerLabel;

    /// <summary>Exactly what goes out.</summary>
    public string Sent => Step.Sent;

    /// <summary>The same thing in plain English.</summary>
    public string Meaning => Step.Meaning;

    /// <summary>Why it is like that, or "".</summary>
    public string Note => Step.Note;

    /// <summary>True when there is a note to draw.</summary>
    public bool HasNote => Step.HasNote;

    /// <summary>True for the operator's own lines, which are tinted.</summary>
    public bool IsYou => Step.Speaker == ContactSpeaker.You;
}

/// <summary>
/// The "what a contact sounds like" panel (HM-DEC-043).
/// </summary>
/// <remarks>
/// <para>The real terror is not the radio, it is not knowing what to say. This
/// panel exists to remove that, and it is the one place in the app where tone
/// matters more than information density.</para>
/// <para>Morse and voice are the same shape with different words, so they are
/// a toggle on one panel rather than two panels. Seeing that they are the same
/// shape is most of the lesson.</para>
/// <para>The operator's own callsign is used throughout, because reading your
/// own call in the example is the difference between a manual and a
/// rehearsal.</para>
/// </remarks>
public partial class ContactShapeViewModel : ObservableObject
{
    private readonly string _callsign;

    [ObservableProperty]
    private bool _isVoice;

    /// <summary>Designer constructor.</summary>
    public ContactShapeViewModel() : this(ContactShape.DefaultYourCall)
    {
    }

    /// <summary>Creates the panel's model.</summary>
    /// <param name="callsign">The operator's callsign, or "" for the default.</param>
    public ContactShapeViewModel(string? callsign)
    {
        _callsign = string.IsNullOrWhiteSpace(callsign)
            ? ContactShape.DefaultYourCall
            : callsign.Trim().ToUpperInvariant();

        Steps = Build();
    }

    /// <summary>The worked contact, in the style currently chosen.</summary>
    public IReadOnlyList<ContactStepViewModel> Steps { get; private set; }

    /// <summary>The reassurance above the example.</summary>
    public string Preamble => ContactShape.Preamble;

    /// <summary>The reassurance below it, which is the part that matters.</summary>
    public string Closing => ContactShape.Closing;

    /// <summary>What a signal report means, shown with the example that uses one.</summary>
    public string RstExplained => SignalReport.RstExplained;

    /// <summary>Collapsed-header summary (HM-DEC-021).</summary>
    public string Summary => IsVoice
        ? "on voice, both sides"
        : "in Morse, both sides";

    /// <summary>Label for the style toggle.</summary>
    public string StyleLabel => IsVoice ? "Voice" : "Morse";

    partial void OnIsVoiceChanged(bool value)
    {
        Steps = Build();
        OnPropertyChanged(nameof(Steps));
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(StyleLabel));
    }

    private IReadOnlyList<ContactStepViewModel> Build()
        => ContactShape
            .Steps(IsVoice ? ContactStyle.Ssb : ContactStyle.Cw, _callsign)
            .Select(s => new ContactStepViewModel(s))
            .ToList();
}
