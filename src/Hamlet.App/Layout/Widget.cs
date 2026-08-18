namespace Hamlet.App.Layout;

/// <summary>
/// Everything Hamlet knows about one widget (HM-DEC-086).
/// </summary>
/// <param name="Id">
/// The stable name it is saved under. **Never renamed**: a saved layout that
/// mentions a widget nobody recognizes loses that widget silently, which is the
/// operator's arrangement quietly forgetting a piece of itself.
/// </param>
/// <param name="Title">What it is called on its header and in the tray.</param>
/// <param name="Family">
/// Which color family it belongs to, from the app's one language (§0.6).
/// Declared on the widget rather than on the control, so a second surface cannot
/// give the same panel a different color.
/// </param>
/// <param name="Blurb">
/// One line in the tray saying what it is for, in the app's voice (§0.7). The
/// tray is a list of names to somebody who already knows what they mean, and the
/// person this application is for does not yet.
/// </param>
/// <param name="Width">How wide it starts, in canvas units.</param>
/// <param name="Height">How tall it starts.</param>
/// <param name="Summoned">
/// True when Hamlet may bring this one out on its own (HM-DEC-086).
/// </param>
public sealed record Widget(
    string Id,
    string Title,
    string Family,
    string Blurb,
    double Width,
    double Height,
    bool Summoned = false);

/// <summary>
/// The widgets there are (HM-DEC-086).
/// </summary>
/// <remarks>
/// <para>**ONE LIST, AND EVERY SURFACE READS IT.** The tray, the presets, the
/// saved layouts and the missing-widget answer all come from here, so a widget
/// cannot exist in the tray and be unknown to a preset, or be placeable and
/// unsaveable.</para>
/// <para>Adding one is adding a row here and a template keyed by its id. There
/// is deliberately no registration call to forget.</para>
/// </remarks>
public static class Widgets
{
    /// <summary>The neighborhood map.</summary>
    public const string Map = "map";

    /// <summary>The dial tape.</summary>
    public const string Tape = "tape";

    /// <summary>The waterfall.</summary>
    public const string Waterfall = "waterfall";

    /// <summary>The CW terminal.</summary>
    public const string Terminal = "terminal";

    /// <summary>The send controls.</summary>
    public const string Send = "send";

    /// <summary>The phrasebook.</summary>
    public const string Phrasebook = "phrasebook";

    /// <summary>"I can hear it and Hamlet can't".</summary>
    public const string ReceiveHelp = "receiveHelp";

    /// <summary>"Did anybody hear me".</summary>
    public const string Heard = "heard";

    /// <summary>"Where to start".</summary>
    public const string Lead = "lead";

    /// <summary>"Happening now".</summary>
    public const string Spots = "spots";

    /// <summary>Field notes.</summary>
    public const string Story = "story";

    /// <summary>The mode field guide.</summary>
    public const string Guide = "guide";

    /// <summary>What a contact sounds like.</summary>
    public const string Contact = "contact";

    /// <summary>The band scanner (HM-DEC-107).</summary>
    public const string Scan = "scan";

    /// <summary>Calling CQ on a cycle (HM-DEC-098).</summary>
    public const string AutoCall = "autocall";

    /// <summary>Every widget, in the order the tray offers them.</summary>
    /// <remarks>
    /// Ordered by what a newcomer reaches for rather than alphabetically, which
    /// is the same reasoning that set the panel order (HM-DEC-064): the ones that
    /// get somebody on the air first, then the ones that explain what they are
    /// hearing.
    /// </remarks>
    public static IReadOnlyList<Widget> All { get; } = new[]
    {
        new Widget(
            Lead, "Where to start", "Amber",
            "One sentence saying where to point the radio right now, and why that "
            + "is the answer.",
            360, 200),

        new Widget(
            Spots, "Happening now", "Green",
            "Who is on the air this minute, ranked for what you could actually "
            + "work rather than for how far away they are.",
            420, 520),

        new Widget(
            Terminal, "CW terminal", "Green",
            "Morse arriving as text, with the characters Hamlet is unsure of "
            + "marked rather than guessed at.",
            520, 320),

        new Widget(
            Send, "Send", "Amber",
            "What you could say next, written out in full, with a button that "
            + "sends it.",
            520, 400),

        new Widget(
            Heard, "Did anybody hear me", "Green",
            "Whether your last call reached anyone, from the receivers that "
            + "listen all day and report what they hear.",
            420, 260),

        new Widget(
            Phrasebook, "Phrasebook", "Green",
            "The handful of things people actually say on the air, with what "
            + "each one means.",
            380, 340, Summoned: true),

        new Widget(
            Map, "Neighborhood map", "Blue",
            "What lives where across the band, so you can see what you are "
            + "tuning into before you get there.",
            560, 200),

        new Widget(
            Tape, "Dial tape", "Amber",
            "Fine tuning by dragging, with the stations somebody has reported "
            + "marked along the top.",
            560, 180),

        new Widget(
            Scan, "Scanner", "Blue",
            "Hamlet works down the band for you, stopping where somebody is "
            + "actually calling rather than wherever there is a tone.",
            460, 400),

        new Widget(
            AutoCall, "Call CQ on a cycle", "Green",
            "Hamlet does the calling and listens between rounds, and it stops the "
            + "moment somebody answers. Into a dummy load while this is being "
            + "proved.",
            460, 520),

        new Widget(
            Waterfall, "Waterfall", "Blue",
            "The radio's own picture of the band, with signals as bright marks "
            + "moving down the screen.",
            560, 320),

        new Widget(
            ReceiveHelp, "I can hear it and Hamlet can't", "Blue",
            "One button for when the radio is clearly hearing something and the "
            + "decoder is not.",
            420, 300),

        new Widget(
            Guide, "Field guide", "Blue",
            "What each mode looks and sounds like, so an unfamiliar noise stops "
            + "being a mystery.",
            420, 400),

        new Widget(
            Story, "Field notes", "Amber",
            "The story of whatever stretch of band you are sitting in.",
            380, 260),

        new Widget(
            Contact, "What a contact sounds like", "Amber",
            "A whole contact from the first call to the sign-off, both sides, in "
            + "your own callsign.",
            420, 420),
    };

    /// <summary>Look one up, or null when nothing answers to that name.</summary>
    /// <param name="id">The saved name.</param>
    /// <returns>The widget, or null.</returns>
    /// <remarks>
    /// Null rather than a placeholder. A layout naming a widget this build does
    /// not have is a fact worth keeping quiet about rather than an empty box with
    /// a question mark in it, and the placement is preserved either way so that
    /// going back to a build that has it restores the arrangement.
    /// </remarks>
    public static Widget? Find(string? id)
        => id is null ? null : All.FirstOrDefault(w => w.Id == id);

    /// <summary>Whether anything answers to that name.</summary>
    /// <param name="id">The saved name.</param>
    /// <returns>True when it is a widget this build has.</returns>
    public static bool Knows(string? id) => Find(id) is not null;
}
