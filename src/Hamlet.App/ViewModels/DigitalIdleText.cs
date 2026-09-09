using System.Globalization;
using Hamlet.RadioEngine.Audio;

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
    /// <param name="grid">The grid the tab is running on.</param>
    /// <returns>One sentence, naming the slot length actually in use.</returns>
    /// <remarks>
    /// <para>**IT USED TO SAY *FT8 RUNS IN FIFTEEN SECOND SLOTS* AND IT WAS A CONST**
    /// (work instruction 290 task 7). On a 7.5-second grid that is a sentence telling
    /// the operator to wait twice as long as he needs to before deciding a band is
    /// empty, on the panel whose whole job is to stop him deciding that too early -
    /// §0.0 broken by a string nobody would think to check.</para>
    /// <para>**AND IT NAMES THE LENGTH RATHER THAN THE MODE**, for the same reason
    /// the capture sidecar does: the length is what the application measured and the
    /// mode name is a label, and until the tab chooses a mode at runtime - which is
    /// step 4's - a name here would be a claim the application cannot support.</para>
    /// </remarks>
    public static string ModeStripFor(SlotGrid grid)
        => "nothing on this frequency yet. Slots here run "
            + grid.SlotSeconds.ToString("0.##", CultureInfo.InvariantCulture)
            + " seconds, so give it a slot or two before deciding the band "
            + "is empty.";

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
}
