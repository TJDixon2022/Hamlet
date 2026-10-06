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

    /// <summary>
    /// A mark crowds the last one only where it overlaps it, or is a piece under half a dit (work instruction 535); a switch
    /// since work instruction 539.
    /// </summary>
    public const string CrowdsNarrowed = "a mark crowds the last only where it overlaps it or is a piece";

    /// <summary>A mark's energy is narrow, in its own bin (work instruction 498; removed in 534, back in 541, HM-DEC-245).</summary>
    public const string Narrowness = "narrowness";

    /// <summary>Three or more lone letters in a row are dropped together (removed in 534, back in 541, HM-DEC-245).</summary>
    public const string ThreeLone = "three lone letters dropped";

    /// <summary>A gap is judged against its neighbours (work instruction 526; removed in 534, back in 541, HM-DEC-245).</summary>
    public const string NeighbourGaps = "the neighbor judgement of gaps";

    /// <summary>A sender keeps the dot and dash line it last showed while a few marks hide the jump (work instruction 544, HM-DEC-248).</summary>
    public const string KeptSplit = "a sender keeps its line";

    /// <summary>
    /// Three gaps in a row past the word line mean the sender's spacing changed, and its gaps are taken from the first of them
    /// (work instruction 546, task 2).
    /// </summary>
    public const string SpacingNow = "three words in a row are a new spacing";

    /// <summary>Every rule kept, in the order the work instruction names them.</summary>
    /// <remarks>
    /// **THREE RULES EARNED THEIR PLACE; TEN LEFT THE TREE** (work instruction 541, HM-DEC-245): of the thirteen work
    /// instruction 534 removed, narrowness, three lone letters dropped and the neighbour judgement of gaps came back on the
    /// score of right less wrong less invented. The other ten were measured, raised nothing, and are held at the tags
    /// <c>before-scoreboard</c> and <c>before-false-characters</c>.
    /// </remarks>
    public static readonly IReadOnlyList<string> All =
    [
        LoneLetter, ColdStartWordLine, SpeedRetry, Settle, OwnWindow, Release, FirstPickWait, Backlog, SilentNotCandidate, Edges,
        MarkShape, KindsHeld, GapKinds, OverlapSplit, GapCrossing, CrowdsNarrowed, Narrowness, ThreeLone, NeighbourGaps, KeptSplit,
        SpacingNow,
    ];

    [ThreadStatic]
    private static HashSet<string>? _off;

    /// <summary>Whether a rule is on: always, unless a test turned it off on this thread.</summary>
    /// <param name="rule">The rule.</param>
    /// <returns>Whether it applies.</returns>
    public static bool On(string rule) => _off is null || !_off.Contains(rule);

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

    /// <summary>This thread's switches, to carry to another thread (work instruction 539: the scoreboard reads in parallel).</summary>
    internal static HashSet<string>? Current => _off;

    /// <summary>Takes another thread's switches on this one until the returned handle is disposed.</summary>
    /// <param name="state">The switches, from <see cref="Current"/>.</param>
    /// <returns>The handle that puts this thread's own back.</returns>
    internal static IDisposable Use(HashSet<string>? state)
    {
        var before = _off;

        _off = state;

        return new Restore(() => _off = before);
    }

    private sealed class Restore(Action undo) : IDisposable
    {
        public void Dispose() => undo();
    }
}
