namespace Hamlet.RadioEngine.Explore;

/// <summary>Which way of talking a worked example is written in.</summary>
public enum ContactStyle
{
    /// <summary>Morse, with its abbreviations.</summary>
    Cw,

    /// <summary>Voice, where the same shape is spoken in full.</summary>
    Ssb,
}

/// <summary>Who is speaking in one line of a worked contact.</summary>
public enum ContactSpeaker
{
    /// <summary>The operator reading this.</summary>
    You,

    /// <summary>The station answering.</summary>
    Them,
}

/// <summary>One line of a worked contact, and what it means.</summary>
/// <param name="Speaker">Who sends it.</param>
/// <param name="Sent">Exactly what goes out, as it would be sent.</param>
/// <param name="Meaning">The same thing in plain English.</param>
/// <param name="Note">Why it is like that, or "" when nothing needs saying.</param>
public sealed record ContactStep(
    ContactSpeaker Speaker, string Sent, string Meaning, string Note)
{
    /// <summary>True when this step carries an explanation.</summary>
    public bool HasNote => Note.Length > 0;

    /// <summary>"You" or "Them", for the label down the left.</summary>
    public string SpeakerLabel => Speaker == ContactSpeaker.You ? "You" : "Them";
}

/// <summary>
/// What a contact actually sounds like, both sides, annotated.
/// </summary>
/// <remarks>
/// <para>THE REAL TERROR IS NOT THE RADIO (HM-DEC-043). It is not knowing
/// what to say. A contact has a shape, close to a ritual, and everybody knows
/// it except the person who has never made one. Nothing in the licence exam
/// teaches it and no manual writes it down, because to everybody already
/// doing it the shape is too obvious to mention.</para>
/// <para>So it is written down here, both sides, with the mechanical bits
/// explained as they arrive: what DE means, what K and BK and SK mean, why a
/// callsign goes out twice, and that ninety seconds is a complete and normal
/// contact rather than somebody brushing you off.</para>
/// <para>TONE MATTERS MORE HERE THAN ANYWHERE ELSE IN THE APP. Nobody should
/// finish reading this feeling like there is a test. Every note is written to
/// remove a reason not to try, and where the honest answer is "it does not
/// matter much", that is what it says.</para>
/// <para>The Morse and the voice versions are the same shape with different
/// words, which is itself the most useful thing to know: learn it once and it
/// works on any band in any mode.</para>
/// <para>Editorial content marked [extrapolated], the same status as the
/// neighborhood map and the field guide (§4). It is the common convention
/// rather than a regulation, and nothing here is required by anybody.</para>
/// </remarks>
public static class ContactShape
{
    /// <summary>The example callsign standing in for the operator.</summary>
    public const string DefaultYourCall = "KC3QIS";

    /// <summary>The example callsign standing in for the other station.</summary>
    public const string TheirCall = "W1ABC";

    /// <summary>
    /// A worked contact, start to finish, in the chosen style.
    /// </summary>
    /// <param name="style">Morse or voice.</param>
    /// <param name="yourCall">The operator's callsign, so the example is
    /// theirs rather than somebody else's.</param>
    /// <returns>The steps in order.</returns>
    public static IReadOnlyList<ContactStep> Steps(ContactStyle style, string? yourCall = null)
    {
        var you = Normalize(yourCall);

        return style == ContactStyle.Cw ? Morse(you) : Voice(you);
    }

    /// <summary>
    /// The one-paragraph reassurance that belongs at the top of it.
    /// </summary>
    public const string Preamble =
        "Every contact follows roughly this shape, and once you have seen it once "
        + "you have seen all of them. It is a ritual rather than a conversation, "
        + "which is the good news: there is nothing to think up on the spot. "
        + "Ninety seconds is a complete and perfectly normal contact, so if "
        + "somebody signs off quickly they are not brushing you off. That is just "
        + "what a contact is.";

    /// <summary>
    /// The closing reassurance, which is the part that actually gets somebody
    /// on the air.
    /// </summary>
    public const string Closing =
        "Nobody is grading this. Operators get callsigns wrong, ask for repeats, "
        + "and forget where they are. If you lose your place, send your callsign "
        + "and wait, and the other station will pick the thread back up. The worst "
        + "realistic outcome is that nobody answers, and that happens to everybody "
        + "several times a week.";

    private static IReadOnlyList<ContactStep> Morse(string you) => new[]
    {
        new ContactStep(
            ContactSpeaker.You,
            $"CQ CQ CQ DE {you} {you} K",
            "Calling anyone, this is me, over.",
            "DE is French for 'from', and it has meant 'this is' on the wire since "
            + "the landline telegraph. K means 'go ahead, your turn'. The callsign "
            + "goes twice because the first one is often half-missed while somebody "
            + "is still tuning you in."),

        new ContactStep(
            ContactSpeaker.Them,
            $"{you} DE {TheirCall} {TheirCall} K",
            "I hear you, this is me, over.",
            "They send your call first and theirs second, which is the pattern the "
            + "whole hobby uses: who it is for, then who it is from."),

        new ContactStep(
            ContactSpeaker.You,
            $"{TheirCall} DE {you}  GE TU  UR RST 579 579  QTH TRAFFORD PA  BK",
            "Good evening, thanks. Your signal is 579 here. I'm in Trafford, "
            + "Pennsylvania. Back to you.",
            "GE is good evening, TU is thank you, UR is your. BK means 'break', a "
            + "quicker handover than K between two stations already talking. The "
            + "report goes twice for the same reason the callsign did."),

        new ContactStep(
            ContactSpeaker.Them,
            $"{you} DE {TheirCall}  TU  UR RST 599 599  QTH BOSTON MA  BK",
            "Thanks. You're 599 here. I'm in Boston, Massachusetts. Back to you.",
            "The same thing the other way round. That is the entire exchange, and "
            + "everything after this point is optional politeness."),

        new ContactStep(
            ContactSpeaker.You,
            $"TU 73 DE {you} SK",
            "Thanks, best regards, this is the end of the contact.",
            "73 is best regards, and never 73s, since the number is already plural. "
            + "SK is the end of a contact rather than the end of a transmission, so "
            + "it is the sign-off you use once and not between overs."),
    };

    private static IReadOnlyList<ContactStep> Voice(string you) => new[]
    {
        new ContactStep(
            ContactSpeaker.You,
            $"CQ CQ CQ, this is {Spell(you)}, {Spell(you)}, calling CQ and standing by.",
            "Calling anyone, this is me, over.",
            "The same call, spoken. Phonetics are used because half the letters in "
            + "the alphabet sound like each other over a radio, and nobody will "
            + "think less of you for spelling your own call out slowly."),

        new ContactStep(
            ContactSpeaker.Them,
            $"{Spell(you)}, this is {Spell(TheirCall)}, {Spell(TheirCall)}.",
            "I hear you, this is me.",
            "Yours first, theirs second, exactly as in Morse. If you only catch part "
            + "of their call, say what you got and ask again. Everybody does."),

        new ContactStep(
            ContactSpeaker.You,
            $"{Spell(TheirCall)}, this is {Spell(you)}. Good evening and thanks for "
            + "the call. You're five nine here. My location is Trafford, "
            + "Pennsylvania. Back to you.",
            "Your signal is fine, here is where I am, your turn.",
            "On voice the report is two numbers rather than three, since the tone "
            + "number only means anything for Morse. Five nine is what most people "
            + "say most of the time."),

        new ContactStep(
            ContactSpeaker.Them,
            $"{Spell(you)}, this is {Spell(TheirCall)}. Thank you, you're also five "
            + "nine. I'm in Boston, Massachusetts. Back to you.",
            "Same again, the other way round.",
            "And that is a complete contact. Anything more is a conversation you "
            + "are both choosing to have."),

        new ContactStep(
            ContactSpeaker.You,
            $"Thanks very much, and seventy three to you. This is {Spell(you)}, clear.",
            "Thanks, best regards, I'm finished.",
            "Said aloud, people say 'seventy three' rather than the digits, and "
            + "never 'seventy threes', since the number is already plural. 'Clear' "
            + "means you have finished and the frequency is free for somebody "
            + "else."),
    };

    /// <summary>
    /// A callsign in the phonetic alphabet, e.g. "Kilo Charlie Three Quebec
    /// India Sierra".
    /// </summary>
    /// <param name="callsign">The callsign.</param>
    /// <returns>The spoken form.</returns>
    public static string Spell(string callsign)
        => string.Join(" ", callsign.Trim().ToUpperInvariant().Select(Phonetic));

    private static string Phonetic(char c) => c switch
    {
        'A' => "Alfa", 'B' => "Bravo", 'C' => "Charlie", 'D' => "Delta",
        'E' => "Echo", 'F' => "Foxtrot", 'G' => "Golf", 'H' => "Hotel",
        'I' => "India", 'J' => "Juliett", 'K' => "Kilo", 'L' => "Lima",
        'M' => "Mike", 'N' => "November", 'O' => "Oscar", 'P' => "Papa",
        'Q' => "Quebec", 'R' => "Romeo", 'S' => "Sierra", 'T' => "Tango",
        'U' => "Uniform", 'V' => "Victor", 'W' => "Whiskey", 'X' => "X-ray",
        'Y' => "Yankee", 'Z' => "Zulu",
        '0' => "Zero", '1' => "One", '2' => "Two", '3' => "Three",
        '4' => "Four", '5' => "Five", '6' => "Six", '7' => "Seven",
        '8' => "Eight", '9' => "Nine",
        '/' => "stroke",
        _ => c.ToString(),
    };

    private static string Normalize(string? callsign)
    {
        var call = (callsign ?? "").Trim().ToUpperInvariant();
        return call.Length == 0 ? DefaultYourCall : call;
    }
}
