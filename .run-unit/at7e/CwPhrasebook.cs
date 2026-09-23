namespace Hamlet.RadioEngine.Cw;

/// <summary>What a phrase is for.</summary>
public enum PhraseKind
{
    /// <summary>Starting and ending a transmission.</summary>
    Handover,

    /// <summary>Ordinary courtesy.</summary>
    Courtesy,

    /// <summary>Saying what you heard, or did not.</summary>
    Copy,

    /// <summary>Saying you are new, or asking for help.</summary>
    NewOperator,
}

/// <summary>One phrase, what it means, and when to send it.</summary>
/// <param name="Sent">Exactly what goes out.</param>
/// <param name="Meaning">The same thing in plain English.</param>
/// <param name="When">When somebody would send it.</param>
/// <param name="Kind">What it is for.</param>
public sealed record CwPhrase(string Sent, string Meaning, string When, PhraseKind Kind);

/// <summary>
/// The phrases people actually send, with what each one means (HM-DEC-059).
/// </summary>
/// <remarks>
/// <para>THE VOCABULARY IS THE GATE THIS HOBBY IS KEPT BEHIND (HM-DEC-041).
/// Morse abbreviations are not a code anybody is hiding: they are what a
/// hundred years of people paying by the word settled on, and every one of them
/// is obvious once somebody tells you. Nobody tells you.</para>
/// <para>THERE IS A COLUMN FOR ADMITTING YOU ARE NEW, and it is the reason this
/// exists at all. "QRS PSE, I am new" is a real and welcome thing to send. A
/// beginner who knows that sentence exists is far more likely to call in the
/// first place, and a beginner who does not know it assumes the band is a room
/// full of experts who will be annoyed with them.</para>
/// <para>Editorial content, marked [extrapolated] like the worked example and
/// the field guide (§4). These are conventions rather than regulations and
/// nothing here is required by anybody.</para>
/// </remarks>
public static class CwPhrasebook
{
    /// <summary>The summary a collapsed panel carries (§0.5).</summary>
    /// <returns>e.g. "24 phrases · 6 for saying you are new".</returns>
    public static string Summary()
    {
        var newcomer = All.Count(p => p.Kind == PhraseKind.NewOperator);
        return $"{All.Count} phrases · {newcomer} for saying you are new";
    }

    /// <summary>What to call a column.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>Its heading.</returns>
    public static string Heading(PhraseKind kind) => kind switch
    {
        PhraseKind.Handover => "Starting and finishing",
        PhraseKind.Courtesy => "Being polite",
        PhraseKind.Copy => "What you heard",
        _ => "Saying you are new",
    };

    /// <summary>
    /// The one sentence at the head of the newcomer column.
    /// </summary>
    /// <remarks>
    /// Doing the emotional work the definitions cannot (HM-DEC-041). The
    /// vocabulary is only half of it; the other half is knowing that using it
    /// this way is normal.
    /// </remarks>
    public const string NewOperatorNote =
        "None of this is an apology. Telling somebody you are new is ordinary "
        + "and it works: most operators will slow right down, and a good many of "
        + "them will tell you afterward that they remember doing the same thing.";

    /// <summary>Everything in the book.</summary>
    public static IReadOnlyList<CwPhrase> All { get; } = new[]
    {
        new CwPhrase("CQ", "Calling anybody at all",
            "At the start, when you want somebody to answer you.",
            PhraseKind.Handover),
        new CwPhrase("DE", "This is, or from",
            "Between who you are calling and who you are.",
            PhraseKind.Handover),
        new CwPhrase("K", "Go ahead, your turn",
            "At the end of a transmission, inviting anybody to reply.",
            PhraseKind.Handover),
        new CwPhrase("KN", "Go ahead, but only you",
            "When you are already talking to somebody and would rather not be "
            + "joined.",
            PhraseKind.Handover),
        new CwPhrase("BK", "Break, back to you",
            "A quicker handover than K, between two stations already talking.",
            PhraseKind.Handover),
        new CwPhrase("AR", "End of this message",
            "Closing a transmission that is not the end of the contact.",
            PhraseKind.Handover),
        new CwPhrase("SK", "End of contact",
            "Once, at the very end. Not between overs.",
            PhraseKind.Handover),

        new CwPhrase("TU", "Thank you",
            "Any time somebody has done something, which is most of the time.",
            PhraseKind.Courtesy),
        new CwPhrase("73", "Best regards",
            "At the sign-off. Never 73s, since the number is already plural.",
            PhraseKind.Courtesy),
        new CwPhrase("GM GA GE", "Good morning, afternoon, evening",
            "Near the start, and nobody minds if you get the time zone wrong.",
            PhraseKind.Courtesy),
        new CwPhrase("FB", "Fine business, that is great",
            "When somebody tells you something good.",
            PhraseKind.Courtesy),
        new CwPhrase("ES", "And",
            "Joining two things, exactly like the word.",
            PhraseKind.Courtesy),
        new CwPhrase("PSE", "Please",
            "Wherever you would say it out loud.",
            PhraseKind.Courtesy),
        new CwPhrase("HPE CUAGN", "Hope to see you again",
            "At the sign-off, when you enjoyed it.",
            PhraseKind.Courtesy),

        new CwPhrase("RST", "Readability, strength, tone",
            "Introducing the signal report, e.g. RST 579.",
            PhraseKind.Copy),
        new CwPhrase("UR", "Your",
            "As in UR RST 579, which is your signal report.",
            PhraseKind.Copy),
        new CwPhrase("QTH", "My location",
            "Saying where you are.",
            PhraseKind.Copy),
        new CwPhrase("R", "Received and understood",
            "Confirming you got what they sent.",
            PhraseKind.Copy),
        new CwPhrase("QSL", "I acknowledge that",
            "The same thing again, and both are used.",
            PhraseKind.Copy),
        new CwPhrase("QRM", "Interference from other stations",
            "Explaining why you missed something.",
            PhraseKind.Copy),
        new CwPhrase("QRN", "Interference from static",
            "The same, when it is the weather rather than people.",
            PhraseKind.Copy),

        new CwPhrase("QRS", "Please send more slowly",
            "Any time. This is the single most useful thing in this list.",
            PhraseKind.NewOperator),
        new CwPhrase("QRS PSE", "Please send more slowly",
            "The same with a please on it, which is how most people send it.",
            PhraseKind.NewOperator),
        new CwPhrase("NEW OP", "I am a new operator",
            "Early in the contact, and it changes how the whole thing goes.",
            PhraseKind.NewOperator),
        new CwPhrase("AGN", "Again, say that once more",
            "When you missed something. Everybody sends this every day.",
            PhraseKind.NewOperator),
        new CwPhrase("PSE AGN QRS", "Please repeat that, more slowly",
            "The whole request in three letters and a bit, which is why Morse "
            + "operators talk like this.",
            PhraseKind.NewOperator),
        new CwPhrase("MY 1ST QSO", "This is my first contact",
            "Once, if you like. The other operator will be delighted.",
            PhraseKind.NewOperator),
    };

    /// <summary>The phrases of one kind, in order.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>Its phrases.</returns>
    public static IReadOnlyList<CwPhrase> OfKind(PhraseKind kind)
        => All.Where(p => p.Kind == kind).ToList();
}
