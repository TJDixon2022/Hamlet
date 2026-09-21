using System.Collections.Generic;

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

/// <summary>Where every item one <c>on_screen</c> line counts actually was.</summary>
/// <param name="At">
/// Each item's own place, an audio offset in **whole Hz**, in the order they entered the window.
/// </param>
/// <param name="Dropped">
/// How many of the line's <c>count</c> have **no** element in <paramref name="At"/> - because
/// Hamlet had no place for them, or because the cap bit.
/// </param>
/// <param name="SlotLast">The last slot the group reached, where it spans more than one.</param>
/// <remarks>
/// <para>**THIS IS THE WHOLE OF UNIT 381** (work instruction 381 section 6 ruling 1). Unit 380's
/// line says *three rows were held back by the CQ filter* and carries the place of one of them;
/// this says *the rows at 1,084, 1,410 and 2,205 Hz were held back by the CQ filter*. 3.1 asks
/// for **the row's** offset or slot and 3.3 asks **which** rows were hidden; a count answers *how
/// many*, which is the right half of both and the whole of neither.</para>
/// <para>**IT IS ONE PARAMETER AND NOT THREE**, for the reason <see cref="OnScreenViewport"/> is
/// one: the three facts are true together or not at all, and a writer with fourteen positional
/// parameters is one a call site gets wrong in silence.</para>
/// <para>**A PLACE IS A NUMBER IN WHOLE Hz AND NEVER AN IDENTITY** (HM-DEC-018 §2.1, ruling 1 item
/// 3). A row is identified by which carrier it is, and a tenth of a hertz costs two bytes a row to
/// say nothing - measured by unit 381 task 1 at **5.07 bytes a place in whole Hz against 6.93 at
/// one decimal and 14.07 as a slot-and-offset string**. The fields that repeat - the slot, the
/// dial, the sub-mode - stay on the line and are written once; only the field that differs is
/// written per row, and that is what makes a place cost five bytes instead of the 142-byte
/// envelope a line of its own would cost.</para>
/// <para>**AND THE CARD MAP IS KEYED BY CALLSIGN** (<c>"c|" + who</c>). That key never goes near
/// the file: a card's element is the offset of the station it stands for where Hamlet measured
/// one, and **nothing at all** where it did not - counted in <paramref name="Dropped"/> rather
/// than named. A record Hamlet cannot answer is absent, never invented (§0.0).</para>
/// <para>**TRUNCATION IS NEVER SILENT.** <c>count</c> still counts every item, so
/// <c>count == At.Count + Dropped</c> always holds: a reader who sees no <c>atDropped</c> knows
/// the file is naming every row it counted, and a reader who sees one knows exactly how many it
/// is not claiming to name.</para>
/// </remarks>
public sealed record OnScreenPlaces(
    IReadOnlyList<long> At, int Dropped, string SlotLast);

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
