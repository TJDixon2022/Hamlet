namespace Hamlet.App.Telemetry;

/// <summary>What the <c>on_screen</c> line is about.</summary>
/// <remarks>
/// **A ROW OR A CARD, AND THE FILE SPELLS IT THE SAME WAY EVERY TIME.** Criterion 3.1 is about
/// rows and 3.2 about cards, and one writer carries both (work instruction 380 section 6 ruling
/// 1) - so the distinction is a type rather than a string typed at a call site, for the reason
/// <c>PanelKeys</c> gives: a typo in a token is silent and is only found by somebody reading the
/// file months later.
/// </remarks>
public enum OnScreenKind
{
    /// <summary>A decoded row, on the whole table or on either list.</summary>
    Row,

    /// <summary>A contact card in the **For you** panel.</summary>
    Card,
}

/// <summary>The five things that can become of a row or a card on the screen.</summary>
/// <remarks>
/// <para>**THESE FIVE ARE R36's OWN LIST**, from Tim's ruling of 2026-09-11: *the record says a
/// line was parsed but not whether the row was visible, filtered, scrolled away or in a folded
/// panel*, with <see cref="Removed"/> the fifth because the trim already counts what it drops and
/// nothing says the row was ever there.</para>
/// <para>**AN ENUM AND NOT A STRING**, so the set is the schema and a sixth state is a deliberate
/// act - the same rule <c>TelemetryCategory</c>'s own comment states about itself.</para>
/// </remarks>
public enum OnScreenState
{
    /// <summary>It is on a list the panel binds, so the operator can see it.</summary>
    Drawn,

    /// <summary>A gate held it off the list, and <c>by</c> names the gate.</summary>
    Filtered,

    /// <summary>It is on the list and outside the scroller's viewport.</summary>
    ScrolledOut,

    /// <summary>It is on the list and the panel holding it is folded shut.</summary>
    Folded,

    /// <summary>It is off the table altogether - the trim, or a card dismissed.</summary>
    Removed,
}

/// <summary>The stable tokens naming what put an item in the state it is in.</summary>
/// <remarks>
/// **NEVER A CALLSIGN, NEVER A GRID, NEVER A WORD OF DECODED TEXT** (HM-DEC-018 §2.1, and work
/// instruction 380 section 10). A gate is a mechanism and has a name of its own; a panel is a
/// <c>PanelKeys</c> value. Nothing here is about a person.
/// </remarks>
public static class OnScreenBy
{
    /// <summary>Nothing held it back.</summary>
    public const string Nothing = "";

    /// <summary>The CQ toggle, which asks an FT8-shaped row's to-field.</summary>
    public const string CqFilter = "cq_filter";

    /// <summary>
    /// It is addressed to the operator, so it is on the right-hand side and not the left.
    /// </summary>
    /// <remarks>
    /// **THIS IS NOT A ROW BEING HIDDEN.** A message addressed to him is on screen in its own
    /// column, which is why <c>DigitalHiddenCount</c> does not count it either - the record and
    /// the summary agree about that or one of them is lying (§0.0).
    /// </remarks>
    public const string AddressedToOperator = "addressed_to_operator";

    /// <summary>The row cap aged the oldest arrival off the table.</summary>
    public const string Trim = "trim";

    /// <summary>The operator dismissed the card.</summary>
    public const string Dismissed = "dismissed";
}

/// <summary>What a scroller read at the moment it settled.</summary>
/// <param name="First">The first index still inside the viewport, or -1 where none is.</param>
/// <param name="Last">The last index still inside the viewport, or -1.</param>
/// <param name="Extent">How tall the content is.</param>
/// <param name="Viewport">How much of it the panel is showing.</param>
/// <param name="Offset">How far down it has been scrolled.</param>
/// <remarks>
/// **SCROLL IS RECORDED PER PANEL AND NOT PER ROW** (work instruction 380 section 6 ruling 1
/// item 3). A row's scrolled-away state is then arithmetic on this range, which is what a
/// diagnosis actually needs and is the only shape that survives a drag: a line per row on a
/// fling would be hundreds of lines about one gesture.
/// </remarks>
public sealed record OnScreenViewport(
    int First, int Last, double Extent, double Viewport, double Offset);
