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

    /// <summary>A mark sits inside the shape of a keyed tone, on its own.</summary>
    public const string MarkShape = "a mark's own shape";

    /// <summary>Every rule kept, in the order the work instruction names them.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        LoneLetter, ColdStartWordLine, SpeedRetry, Settle, OwnWindow, Release, FirstPickWait, Backlog, SilentNotCandidate, Edges,
        MarkShape,
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

        return new Restore(before);
    }

    private sealed class Restore(HashSet<string>? before) : IDisposable
    {
        public void Dispose() => _off = before;
    }
}
