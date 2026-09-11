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

    /// <summary>The mode strip, for a mode Hamlet cannot yet read.</summary>
    /// <param name="mode">The mode the operator pressed, e.g. "PSK31".</param>
    /// <returns>One sentence naming the mode and what it cannot do yet.</returns>
    /// <remarks>
    /// <para>**A MODE WITH NO DECODER SAYS SO** (work instruction 312, step 0 of the
    /// PSK31 phase). Pressing PSK31 tunes the radio to the cited watering hole and
    /// then reads nothing, and until this sentence existed the panel underneath said
    /// *slots here run 15 seconds, so give it a slot or two before deciding the band
    /// is empty* - a sentence about FT8, on a mode that has no slots, telling the
    /// operator to wait for something that is never coming (0.0, HM-DEC-092).</para>
    /// <para>**IT NAMES THE MODE, WHICH <see cref="ModeStripFor"/> DELIBERATELY DOES
    /// NOT.** That line names the slot length because the length is what was
    /// measured and the name would have been a claim. Here the name is the whole
    /// point: what is being said is that *this* mode is the one nothing can read
    /// yet, while the two beside it can.</para>
    /// <para>**AND IT SAYS WHERE THE RADIO IS**, because the tune did happen and the
    /// operator can hear the band even though Hamlet cannot read it. An empty panel
    /// that did not say that would read as a tune that failed.</para>
    /// </remarks>
    public static string NotYetReadable(string mode)
        => "the radio is on the " + mode + " calling frequency and Hamlet cannot "
            + "read " + mode + " yet, so nothing will appear below. You can still "
            + "hear it, and it sounds like a warble you could almost hum.";

    /// <summary>The mode strip, for a mode Hamlet reads across the whole passband.</summary>
    /// <param name="mode">The mode the operator pressed.</param>
    /// <returns>One sentence saying how widely it is listening and what a line is.</returns>
    /// <remarks>
    /// <para>**IT REPLACES UNIT 314'S ONE-SPOT SENTENCE** (work instruction 315 task 3),
    /// which said, correctly then, that Hamlet listened a thousand hertz above the dial
    /// and nowhere else. It now looks at everything the receiver passes, so a quiet list
    /// does mean nothing Hamlet can be sure is PSK31 is there.</para>
    /// <para>**IT SAYS THE LINES CANNOT BE ANSWERED** (0.0, 0.2), because a line of text
    /// with a callsign in it looks like something to click, and nothing on it is.</para>
    /// </remarks>
    public static string ListeningAcrossThePassband(string mode)
        => "listening for " + mode + " across the whole passband. Every signal Hamlet is "
            + "sure is " + mode + " gets a line of its own below, with where it sits, how "
            + "strong it is and its text as it arrives. Nothing on those lines can be "
            + "answered yet.";

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
