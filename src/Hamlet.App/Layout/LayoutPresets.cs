namespace Hamlet.App.Layout;

/// <summary>
/// The arrangements Hamlet ships, named by what you are doing (HM-DEC-086).
/// </summary>
/// <remarks>
/// <para>**NAMED BY ACTIVITY, NOT BY MODE.** "Making contacts" is a thing
/// somebody wants to do; "CW layout" is a thing somebody has to already
/// understand in order to pick. This application exists for a person who has held
/// a license for six years and made one contact, and every name here has to make
/// sense to them before they know what any of it does.</para>
/// <para>**NOBODY EVER STARTS ON AN EMPTY CANVAS** (HM-DEC-086). A first run
/// lands on Getting started, furnished. An empty canvas with a tray beside it is
/// a puzzle handed to somebody who came here to talk on the radio.</para>
/// <para>**AND A PRESET IS A STARTING POINT, NEVER A DOCUMENT.** Pressing one
/// loads a fresh copy, every time. Dragging afterward changes what is on screen
/// and never changes the preset, so the way back is always one press away and
/// nobody can spoil it by rearranging.</para>
/// </remarks>
public static class LayoutPresets
{
    private const double Gap = 12;

    /// <summary>Where a first run lands.</summary>
    public const string FirstRun = "Getting started";

    /// <summary>
    /// Getting started: what to do, and what everything is.
    /// </summary>
    /// <remarks>
    /// The lead card first, because it is the one that answers "what now". The
    /// field guide and the worked contact beside it, because the two questions a
    /// newcomer actually has are what that noise is and what you are supposed to
    /// say.
    /// </remarks>
    private static CanvasLayout GettingStarted { get; } = new(
        FirstRun,
        "Where to point the radio, what you are hearing, and what people say.",
        new[]
        {
            new Placement(Widgets.Lead, Gap, Gap, 520, 210),
            new Placement(Widgets.Spots, Gap, 234, 520, 470),
            new Placement(Widgets.Guide, 544, Gap, 440, 340),
            new Placement(Widgets.Contact, 544, 364, 440, 340),
        },
        Preset: true);

    /// <summary>
    /// Listening around: the band as a picture.
    /// </summary>
    /// <remarks>
    /// The three surfaces that share one frequency axis, stacked so a signal is
    /// in the same place on all of them (HM-DEC-047), with the terminal under
    /// them because the point of tuning around is finding something to read.
    /// </remarks>
    private static CanvasLayout ListeningAround { get; } = new(
        "Listening around",
        "The whole band as a picture, and whatever you land on turned into text.",
        new[]
        {
            new Placement(Widgets.Map, Gap, Gap, 700, 190),
            new Placement(Widgets.Tape, Gap, 214, 700, 180),
            new Placement(Widgets.Waterfall, Gap, 406, 700, 300),
            new Placement(Widgets.Terminal, 724, Gap, 400, 400),
            new Placement(Widgets.ReceiveHelp, 724, 424, 400, 282),

            // THE SCANNER BELONGS IN THIS ONE AND NOWHERE ELSE (HM-DEC-107).
            // It is the same argument the waterfall makes one step further on:
            // the picture says which parts of the band are busy, and the scanner
            // is what turns that into the radio actually pointing at them.
            // Appended rather than fitted in, so nothing already on this preset
            // moves under somebody who has learned where it sits.
            new Placement(Widgets.Scan, 724, 722, 400, 380),
        },
        Preset: true);

    /// <summary>
    /// Making contacts: what I am doing on the left, who is out there on the
    /// right.
    /// </summary>
    /// <remarks>
    /// <para>Tim's own arrangement, and the reasoning is his. **The send controls
    /// sit directly beneath the terminal so that reading a call and answering it
    /// is one motion** rather than a hunt across the screen, and "Did anybody hear
    /// me" goes under that because it answers the question the send raises.</para>
    /// <para>**The band map is deliberately absent.** It belongs to looking
    /// around, and this is the arrangement for when you have already found
    /// somebody.</para>
    /// </remarks>
    private static CanvasLayout MakingContacts { get; } = new(
        "Making contacts",
        "Read on the left, answer underneath, and see who else is calling on the "
        + "right.",
        new[]
        {
            new Placement(Widgets.Terminal, Gap, Gap, 620, 300),
            new Placement(Widgets.Send, Gap, 324, 620, 380),
            new Placement(Widgets.Heard, Gap, 716, 620, 230),
            new Placement(Widgets.Spots, 644, Gap, 420, 700),

            // **CALLING ON A CYCLE BELONGS WHERE ANSWERING DOES**, and it was in
            // no preset at all until now (HM-DEC-098 built it and nothing put it
            // on a canvas). A widget reachable only from the tray is reachable
            // only by somebody who already knows it exists, which is HM-DEC-072's
            // own shape: ruled, built, and never invoked.
            //
            // Appended rather than fitted in, for the reason the scanner was:
            // nothing already on this preset moves under somebody who has learned
            // where it sits.
            new Placement(Widgets.AutoCall, 644, 716, 420, 230),
        },
        Preset: true);

    /// <summary>Everything Hamlet offers, in the order the bar shows them.</summary>
    public static IReadOnlyList<CanvasLayout> All { get; } = new[]
    {
        GettingStarted,
        ListeningAround,
        MakingContacts,
    };

    /// <summary>
    /// One of them, by name, as a fresh copy.
    /// </summary>
    /// <param name="name">Which one.</param>
    /// <returns>An independent arrangement, or null when there is no such preset.</returns>
    public static CanvasLayout? Fresh(string? name)
        => All.FirstOrDefault(l => l.Name == name)?.Fresh();

    /// <summary>What a first run gets, furnished.</summary>
    /// <returns>A fresh copy of Getting started.</returns>
    public static CanvasLayout Start() => GettingStarted.Fresh();
}
