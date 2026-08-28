namespace Hamlet.App.ViewModels;

/// <summary>
/// What each panel on the Digital tab says before anything has been heard.
/// </summary>
/// <remarks>
/// <para>**PLACEHOLDER TEXT, LIKE EVERYTHING ELSE ON THAT TAB** (work
/// instruction 037, task 6). The strings live here and the markup shows the busy
/// version; the unit that makes the tab live picks these up and swaps between
/// them. Nothing reads them yet, and that is said plainly rather than left for
/// somebody to discover.</para>
/// <para>**THEY ARE WRITTEN IN THE CW TERMINAL'S VOICE**, whose own idle line is
/// `listening to Training radio. Nothing decoded yet.` — connected speech, the
/// reason attached to the fact, and a way forward rather than a bare absence
/// (§0.7, HM-DEC-034).</para>
/// <para>**AN EMPTY PANEL IS INDISTINGUISHABLE FROM A BROKEN ONE** (Tim,
/// 2026-08-28), and one message for the whole tab is lost the moment a panel is
/// collapsed, so each carries its own (HM-DEC-021).</para>
/// </remarks>
public static class DigitalIdleText
{
    /// <summary>The mode strip, before a mode has been heard.</summary>
    public const string ModeStrip =
        "nothing on this frequency yet. FT8 runs in fifteen second slots, so "
        + "give it a slot or two before deciding the band is empty.";

    /// <summary>The waterfall, before any spectrum has arrived.</summary>
    /// <remarks>
    /// **THE CONTROL DRAWS ITS OWN EMPTY STATE ALREADY**, saying no spectrum has
    /// arrived. This is the panel's line rather than the picture's, and it says
    /// the thing the picture cannot: what the waterfall is for here.
    /// </remarks>
    public const string Waterfall =
        "no spectrum yet. When it arrives you will see each slot as a band of "
        + "marks, and a signal that decoded nothing still shows up here.";

    /// <summary>The decoded text table, before anything has decoded.</summary>
    public const string Decoded =
        "nothing decoded yet. Every message that comes out of a slot lands here "
        + "exactly as it was sent, before Hamlet makes anything of it.";

    /// <summary>The plain-English panel, before anything has decoded.</summary>
    public const string Saying =
        "nobody heard yet. As stations come in this is where they are put into "
        + "ordinary words, with the raw line underneath so you can check it.";
}
