namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// Whether the dial has come to rest inside a block long enough to act on.
/// </summary>
/// <remarks>
/// <para>**MOVEMENT DISQUALIFIES, NOT POSITION** (work instruction 050, task 4).
/// The condition is the same neighborhood and an unchanged frequency across
/// consecutive observations spanning one second — **not one second spent inside
/// the block**. A slow tune sits inside a three-kilohertz block for longer than a
/// second while still moving, and the operator crosses data territory every time
/// he scans from Morse up to voice. A rule written on time-in-block would fire on
/// both.</para>
/// <para>**THIS EXTENDS HM-DEC-056'S SETTLE RULE RATHER THAN REPLACING IT.** That
/// ruling says a flip waits for the dial to settle, so crossing three
/// neighborhoods in one drag produces one change and not three. It does not say
/// how long settled is, or what unsettles it. This says both.</para>
/// <para>**IT IS PURE AND TAKES ITS OWN CLOCK AS AN ARGUMENT** (§5.4). Nothing
/// below the pump reads the wall clock, so the same sequence of observations
/// produces the same answer every run and a test can advance time by hand.</para>
/// </remarks>
public readonly record struct ModeDwell
{
    /// <summary>How long the dial must be still before a dwell matures.</summary>
    /// <remarks>
    /// **ONE SECOND, WHICH IS THE FIGURE THE ORDER RULES.** Long enough that a
    /// hand still moving does not reach it, short enough that arriving somewhere
    /// deliberately does not feel like waiting.
    /// </remarks>
    public static readonly TimeSpan Matures = TimeSpan.FromSeconds(1);

    private ModeDwell(string block, long frequencyHz, DateTime sinceUtc, bool spent)
    {
        Block = block;
        FrequencyHz = frequencyHz;
        SinceUtc = sinceUtc;
        Spent = spent;
    }

    /// <summary>The block the dial has been sitting in, or empty.</summary>
    public string Block { get; } = "";

    /// <summary>Where the dial has been sitting.</summary>
    public long FrequencyHz { get; }

    /// <summary>When it last arrived there.</summary>
    public DateTime SinceUtc { get; }

    /// <summary>
    /// Whether this dwell has already been acted on, so it fires once.
    /// </summary>
    /// <remarks>
    /// **A MATURED DWELL IS AN EVENT AND NOT A STATE.** Without this, every
    /// observation after the first second would report a mature dwell and the
    /// write would go out four times a second at a dial nobody is touching.
    /// </remarks>
    public bool Spent { get; }

    /// <summary>Nothing observed yet.</summary>
    public static ModeDwell Nowhere { get; } = new("", 0, DateTime.MinValue, false);

    /// <summary>Note where the dial is, and say whether that matured a dwell.</summary>
    /// <param name="block">
    /// The neighborhood's short name, or empty where the dial is off the map.
    /// </param>
    /// <param name="frequencyHz">Where the dial is.</param>
    /// <param name="atUtc">When this observation was taken.</param>
    /// <param name="scanning">
    /// Whether the scanner is running. **Suppressed entirely while it is**: a
    /// scan moves the dial on its own and every block it crosses would otherwise
    /// look like an arrival.
    /// </param>
    /// <returns>The dwell to carry forward, and whether it matured on this look.</returns>
    /// <remarks>
    /// **LEAVING AND RE-ENTERING RE-ARMS FROM ZERO, AND LEAVING EARLY IS
    /// SILENT.** A write that did not happen is not narrated; HM-DEC-056 already
    /// narrates the ones that do, and a running commentary on the ones that
    /// nearly did is noise on the one line the operator reads.
    /// </remarks>
    public (ModeDwell Next, bool Matured) Observe(
        string block, long frequencyHz, DateTime atUtc, bool scanning)
    {
        if (scanning || string.IsNullOrEmpty(block))
        {
            return (Nowhere, false);
        }

        // A different block, or a dial that moved at all, starts the clock again.
        if (block != Block || frequencyHz != FrequencyHz)
        {
            return (new ModeDwell(block, frequencyHz, atUtc, false), false);
        }

        if (Spent || atUtc - SinceUtc < Matures)
        {
            return (this, false);
        }

        return (new ModeDwell(block, frequencyHz, SinceUtc, true), true);
    }
}
