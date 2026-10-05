namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **EVERY DECISION RULE ON THE SHAPE SIDE CAN BE SWITCHED OFF ALONE** (work instruction 534, HM-DEC-238), so the owner's
/// recordings can measure what each is worth.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-03:** *"Right now we suck."* The rules were fitted to synthetic signals one unit at a time, and
/// real fists break them. The scoreboard (<c>TheRecordingsScoreboardTests</c>) reads the owner's recordings with each rule
/// off in turn. A rule whose removal raised or held the total came out of the tree; these eleven lowered it, or broke a
/// hard limit, and stay. Thirteen came out in work instruction 534, and the tag <c>before-scoreboard</c> holds them.</para>
/// <para>**EVERY RULE IS ON UNLESS A TEST TURNS IT OFF**, on its own thread, for the length of one reading. Nothing in the
/// app turns one off, and a rule removed from the tree is removed from this list with it.</para>
/// </remarks>
internal static class CwRules
{
    /// <summary>The lone-letter rule: a one-mark letter is printed only where a longer letter of the same sender confirms it.</summary>
    public const string LoneLetter = "lone letter";

    /// <summary>Until the word cluster is trusted, the word line is never under √21 of the sender's element gaps.</summary>
    public const string ColdStartWordLine = "cold-start word line at √21";

    /// <summary>Where a sender's marks show two speeds, its newer marks are taken as its speed now.</summary>
    public const string SpeedRetry = "retry over newer marks when speed changes";

    /// <summary>A key-down's first hops may settle before its top is held to one level.</summary>
    public const string Settle = "settle at key-down";

    /// <summary>A printed sender is measured through its own window.</summary>
    public const string OwnWindow = "the sender's own window";

    /// <summary>A printed sender is let go under a shape of 0.1.</summary>
    public const string Release = "release under 0.1";

    /// <summary>The first pick waits one word gap.</summary>
    public const string FirstPickWait = "first pick waits one word gap";

    /// <summary>A sender given the terminal prints the letters it sent since it stood.</summary>
    public const string Backlog = "handover backlog";

    /// <summary>A sender silent past its release is not a candidate.</summary>
    public const string SilentNotCandidate = "a silent sender is not a candidate";

    /// <summary>A mark rises and falls like a key.</summary>
    public const string Edges = "edges";

    /// <summary>A sender prints only where its two kinds hold over its last ten marks, each kind recurring.</summary>
    public const string KindsHeld = "two kinds held over the last ten marks";

    /// <summary>A sender's gaps show a clean jump of √3 between gaps inside a letter and gaps between letters.</summary>
    public const string GapKinds = "gap kinds";

    /// <summary>A mark sits inside the shape of a keyed tone, on its own.</summary>
    public const string MarkShape = "a mark's own shape";

    /// <summary>Where a sender's letter and word gaps show no clean jump, they are split in two where they overlap (work instruction 538).</summary>
    public const string OverlapSplit = "split overlapping letter and word gaps";

    /// <summary>The word line is where the letter and word clusters cross, each weighed by its own spread (work instruction 538).</summary>
    public const string GapCrossing = "word line at the clusters' crossing";

    /// <summary>Every rule kept, in the order the work instruction names them.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        LoneLetter, ColdStartWordLine, SpeedRetry, Settle, OwnWindow, Release, FirstPickWait, Backlog, SilentNotCandidate, Edges,
        MarkShape, KindsHeld, GapKinds, OverlapSplit, GapCrossing,
    ];

    /// <summary>A mark crowds the last one only where it overlaps it, or is a piece under half a dit (work instruction 535).</summary>
    public const string CrowdsNarrowed = "a mark crowds the last only where it overlaps it or is a piece";

    /// <summary>The rules added since the scoreboard began, each switchable (work instruction 539).</summary>
    public static readonly IReadOnlyList<string> Added = [CrowdsNarrowed, KindsHeld, GapKinds, OverlapSplit, GapCrossing];

    /// <summary>Removed in work instruction 534: a sender prints only at a shape of 0.2, and a sequence stands only there.</summary>
    public const string StandingLine = "the 0.2 standing line";

    /// <summary>Removed in work instruction 534: three or more lone letters in a row are dropped together.</summary>
    public const string ThreeLone = "three lone letters dropped";

    /// <summary>Removed in work instruction 534: a standing sender's quieter mark inside its letter is its own.</summary>
    public const string QuieterMarks = "quieter marks";

    /// <summary>Removed in work instruction 534: a gap over three of the sender's word gaps is a pause, out of its word cluster.</summary>
    public const string Pause = "the pause";

    /// <summary>Removed in work instruction 534: a hand's overlapping lengths are its two kinds.</summary>
    public const string HandTwoKinds = "a hand's two kinds";

    /// <summary>Removed in work instruction 534: a bar must end where its tone's peak drops.</summary>
    public const string KeyUp = "key-up";

    /// <summary>Removed in work instruction 534: a word gap is five dits or more where only the letter cluster shows.</summary>
    public const string FiveDitFloor = "the five-dit floor";

    /// <summary>Removed in work instruction 534: the shape scores how tightly the gaps inside letters cluster.</summary>
    public const string InsideLetterTightness = "the shape's inside-letter tightness";

    /// <summary>Removed in work instruction 534: the shape scores how tightly the gaps between letters cluster.</summary>
    public const string LetterGapTightness = "the shape's letter-gap tightness";

    /// <summary>Removed in work instruction 534: a rectangle fitted as a whole fills what the per-hop tests left.</summary>
    public const string RectangleFit = "the rectangle fit";

    /// <summary>Removed in work instruction 534: a mark's energy is narrow, in its own bin.</summary>
    public const string Narrowness = "narrowness";

    /// <summary>Removed in work instruction 534: a gap is judged against its neighbours.</summary>
    public const string NeighbourGaps = "the neighbour judgement of gaps";

    /// <summary>Removed in work instruction 534: a letter's marks are split against their neighbours.</summary>
    public const string NeighbourMarks = "the neighbour split of marks";

    /// <summary>
    /// **THE THIRTEEN REMOVED IN WORK INSTRUCTION 534, BACK BEHIND A SWITCH** (work instruction 539, HM-DEC-243): restored from
    /// the tag <c>before-scoreboard</c> to be measured on the new score, and off unless a test turns one on or it is listed in
    /// <see cref="Restored"/>.
    /// </summary>
    public static readonly IReadOnlyList<string> Removed =
    [
        StandingLine, ThreeLone, QuieterMarks, Pause, HandTwoKinds, KeyUp, FiveDitFloor, InsideLetterTightness,
        LetterGapTightness, RectangleFit, Narrowness, NeighbourGaps, NeighbourMarks,
    ];

    /// <summary>The removed rules that came back on the new score (work instruction 539): none, as for <see cref="TakenOut"/>.</summary>
    public static readonly IReadOnlyList<string> Restored = [];

    /// <summary>
    /// **KEPT OR ADDED RULES TAKEN OUT ON THE NEW SCORE** (work instruction 539, HM-DEC-243): off unless a test turns one on.
    /// None: no rule set measured met the order's bar (docs/cw-scoreboard.md), so the reading is HEAD's.
    /// </summary>
    public static readonly IReadOnlyList<string> TakenOut = [];

    [ThreadStatic]
    private static HashSet<string>? _off;

    [ThreadStatic]
    private static HashSet<string>? _with;

    /// <summary>
    /// Whether a rule is on: a kept or added rule always, unless a test turned it off on this thread; a removed rule never,
    /// unless it came back or a test turned it on.
    /// </summary>
    /// <param name="rule">The rule.</param>
    /// <returns>Whether it applies.</returns>
    public static bool On(string rule)
    {
        if (_off is not null && _off.Contains(rule))
        {
            return false;
        }

        if (_with is not null && _with.Contains(rule))
        {
            return true;
        }

        return !TakenOut.Contains(rule) && (!Removed.Contains(rule) || Restored.Contains(rule));
    }

    /// <summary>Turns rules off on this thread until the returned handle is disposed.</summary>
    /// <param name="rules">The rules.</param>
    /// <returns>The handle that turns them back on.</returns>
    public static IDisposable Off(params string[] rules)
    {
        var before = _off;

        _off = new HashSet<string>(before ?? [], StringComparer.Ordinal);
        _off.UnionWith(rules);

        return new Restore(() => _off = before);
    }

    /// <summary>Turns removed or taken-out rules on on this thread until the returned handle is disposed (work instruction 539).</summary>
    /// <param name="rules">The rules.</param>
    /// <returns>The handle that turns them off again.</returns>
    public static IDisposable With(params string[] rules)
    {
        var before = _with;

        _with = new HashSet<string>(before ?? [], StringComparer.Ordinal);
        _with.UnionWith(rules);

        return new Restore(() => _with = before);
    }

    /// <summary>This thread's switches, to carry to another thread (work instruction 539: the scoreboard reads in parallel).</summary>
    internal static (HashSet<string>? Off, HashSet<string>? With) Current => (_off, _with);

    /// <summary>Takes another thread's switches on this one until the returned handle is disposed.</summary>
    /// <param name="state">The switches, from <see cref="Current"/>.</param>
    /// <returns>The handle that puts this thread's own back.</returns>
    internal static IDisposable Use((HashSet<string>? Off, HashSet<string>? With) state)
    {
        var (off, with) = (_off, _with);

        (_off, _with) = state;

        return new Restore(() => (_off, _with) = (off, with));
    }

    private sealed class Restore(Action undo) : IDisposable
    {
        public void Dispose() => undo();
    }
}
