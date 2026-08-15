namespace Hamlet.RadioEngine.Explore;

/// <summary>One automated receiver that heard the operator (HM-DEC-075).</summary>
/// <param name="ReceiverCall">The skimmer's callsign.</param>
/// <param name="FrequencyHz">Where it heard him.</param>
/// <param name="SignalDb">Signal-to-noise it measured, or null.</param>
/// <param name="Wpm">The speed it read his sending at, or null.</param>
/// <param name="HeardAtUtc">When.</param>
public sealed record HeardReport(
    string ReceiverCall, long FrequencyHz, int? SignalDb, int? Wpm, DateTime HeardAtUtc)
{
    /// <summary>The frequency as the app writes it.</summary>
    public string FrequencyLabel
        => (FrequencyHz / 1_000_000.0).ToString(
            "0.000", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// What the measurement means, with the number beside it.
    /// </summary>
    /// <remarks>
    /// The plain words first and the figure after, because "19 dB" means
    /// nothing to somebody who has never seen one and "well above the noise"
    /// means everything (HM-DEC-042).
    /// </remarks>
    public string Signal => SignalDb is { } db
        ? $"{SignalReport.Describe(db)}"
        : "";

    /// <summary>
    /// What the receiver read his sending speed as, or "".
    /// </summary>
    /// <remarks>
    /// AN INDEPENDENT CHECK ON HIS KEYING, which is worth more than it looks.
    /// A machine that read the speed cleanly read the characters cleanly, and
    /// somebody who has never had any feedback on their sending has just been
    /// told by a stranger's computer that it was readable.
    /// </remarks>
    public string Speed => Wpm is { } wpm ? $"read at {wpm} words a minute" : "";
}

/// <summary>Where the operator is in the moment after calling.</summary>
public enum HeardState
{
    /// <summary>Nothing has been called yet, so there is nothing to watch for.</summary>
    Idle,

    /// <summary>A call went out and Hamlet is watching for reports.</summary>
    Waiting,

    /// <summary>Receivers heard him.</summary>
    Heard,

    /// <summary>The window passed and nothing came back.</summary>
    Nothing,
}

/// <summary>What Hamlet says about whether anybody heard him.</summary>
/// <param name="State">Where this is.</param>
/// <param name="Headline">The one line, and the panel's collapsed summary.</param>
/// <param name="Detail">The paragraph under it.</param>
/// <param name="Reports">Who heard him, newest first.</param>
public sealed record HeardSummary(
    HeardState State, string Headline, string Detail, IReadOnlyList<HeardReport> Reports);

/// <summary>
/// Did anybody hear me (HM-DEC-075, closing FG-008).
/// </summary>
/// <remarks>
/// <para>THE FIRST HONEST ANSWER THIS OPERATOR HAS EVER HAD TO "DID THAT WORK".
/// He has been licensed six years and made one contact. He will call CQ, and
/// perhaps nobody will answer. The Reverse Beacon Network is a mesh of automated
/// receivers listening across the bands and publishing every callsign they hear,
/// and Hamlet is already reading that feed. So even when no human answers, real
/// machines can say his signal arrived somewhere.</para>
/// <para>NEVER MANUFACTURE THE FEELING, ONLY REPORT THE FACT. Hamlet says he was
/// heard because receivers really heard him. It does not inflate, does not round
/// up, and does not soften a silence into something warmer than the truth. The
/// moment this becomes encouragement rather than evidence it is worth nothing,
/// and it takes the trust that makes the rest of the application useful with
/// it (§0.0).</para>
/// <para>THE WAITING STATE CARRIES REAL WEIGHT AND IS NOT A SPINNER. Thirty to
/// ninety seconds of silence after a first call is exactly where a beginner
/// decides it is not working and goes and does something else. So the wait is
/// held honestly: it says what it is watching for, and it says what is normal,
/// because "reports usually take a minute or two and a human takes longer
/// because they have to finish listening first" is a fact that keeps somebody in
/// the chair.</para>
/// <para>Pure: spots and a moment in, a summary out. No clock, no network (§5).</para>
/// </remarks>
public static class HeardWatch
{
    /// <summary>
    /// How long after a call Hamlet keeps watching before saying nothing came.
    /// </summary>
    /// <remarks>
    /// Ten minutes. Skimmers usually report inside a couple of minutes, and the
    /// extra room costs nothing and covers a mesh having a slow moment. Calling
    /// it after ninety seconds would turn an ordinary delay into a verdict.
    /// </remarks>
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How long a report usually takes, which is what the wait says out loud.
    /// </summary>
    public static readonly TimeSpan TypicalReport = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Every report of this operator, newest first.
    /// </summary>
    /// <param name="reports">Everything the store has.</param>
    /// <param name="yourCall">The operator's callsign.</param>
    /// <param name="sinceUtc">When the call went out.</param>
    /// <returns>His reports, newest first, possibly empty.</returns>
    public static IReadOnlyList<HeardReport> Mine(
        IEnumerable<HeardReport> reports, string? yourCall, DateTime sinceUtc)
    {
        ArgumentNullException.ThrowIfNull(reports);

        var mine = (yourCall ?? "").Trim().ToUpperInvariant();

        if (mine.Length == 0)
        {
            return Array.Empty<HeardReport>();
        }

        return reports
            .Where(r => r.HeardAtUtc >= sinceUtc)
            .OrderByDescending(r => r.HeardAtUtc)
            .ToList();
    }

    /// <summary>
    /// Whether an RBN line is a report of this operator.
    /// </summary>
    /// <param name="spot">The parsed line.</param>
    /// <param name="yourCall">The operator's callsign.</param>
    /// <returns>True when a receiver heard him.</returns>
    /// <remarks>
    /// An exact match on the callsign, case folded and nothing else. A near
    /// match is somebody else, and telling this operator he was heard when the
    /// machine heard a different station would be the cruelest possible bug
    /// (§0.0).
    /// </remarks>
    public static bool IsMine(RbnSpot? spot, string? yourCall)
    {
        var mine = (yourCall ?? "").Trim().ToUpperInvariant();

        return spot is not null
               && mine.Length > 0
               && string.Equals(
                   spot.DxCall.Trim().ToUpperInvariant(), mine, StringComparison.Ordinal);
    }

    /// <summary>Turn a matching line into a report.</summary>
    /// <param name="spot">The line, already matched.</param>
    /// <returns>The report.</returns>
    public static HeardReport From(RbnSpot spot)
    {
        ArgumentNullException.ThrowIfNull(spot);

        return new HeardReport(
            spot.Spotter, spot.FrequencyHz, spot.SignalDb, spot.Wpm, spot.HeardAtUtc);
    }

    /// <summary>
    /// What to say right now.
    /// </summary>
    /// <param name="calledAtUtc">When the last call went out, or null.</param>
    /// <param name="reports">His reports since then, newest first.</param>
    /// <param name="nowUtc">The moment.</param>
    /// <returns>The summary, never null.</returns>
    public static HeardSummary Describe(
        DateTime? calledAtUtc, IReadOnlyList<HeardReport> reports, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(reports);

        if (calledAtUtc is not { } called)
        {
            return new HeardSummary(
                HeardState.Idle,
                "nothing called yet",
                "When you call CQ, Hamlet watches the skimmer network for your own "
                + "callsign and tells you who heard you. Those are automated "
                + "receivers listening across the bands, and they report whether or "
                + "not a person answers.",
                Array.Empty<HeardReport>());
        }

        if (reports.Count > 0)
        {
            return Heard(reports);
        }

        return nowUtc - called < Window
            ? Waiting(nowUtc - called)
            : NothingCame();
    }

    private static HeardSummary Heard(IReadOnlyList<HeardReport> reports)
    {
        var receivers = reports
            .Select(r => r.ReceiverCall)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var headline = receivers.Count == 1
            ? $"{receivers[0]} heard you"
            : $"{receivers.Count} receivers heard you";

        // THE STRONGEST REPORT IS NAMED AND NOT AVERAGED. An average would
        // describe a signal nobody actually received.
        var best = reports
            .Where(r => r.SignalDb is not null)
            .OrderByDescending(r => r.SignalDb)
            .FirstOrDefault();

        var detail = receivers.Count == 1
            ? $"Your signal reached {receivers[0]}, which is a machine that was "
              + "listening and wrote down what it heard."
            : $"Your signal reached {receivers.Count} of them, and every one is a "
              + "machine that was listening and wrote down what it heard.";

        if (best is not null)
        {
            detail += $" The best of them put you {best.Signal.ToLowerInvariant()}";
            detail += best.Wpm is { } wpm
                ? $", and read your sending at {wpm} words a minute, which means the "
                  + "characters arrived cleanly enough for a computer to time them."
                : ".";
        }

        return new HeardSummary(HeardState.Heard, headline, detail, reports);
    }

    private static HeardSummary Waiting(TimeSpan elapsed)
    {
        var detail =
            "Hamlet is watching the skimmer network for your callsign. Reports "
            + "usually take a minute or two to appear, and a person takes longer "
            + "than that because they have to finish listening before they can "
            + "answer you. A quiet half minute after a call is completely "
            + "ordinary and it is not a verdict on anything.";

        return new HeardSummary(
            HeardState.Waiting,
            elapsed < TypicalReport
                ? "listening for anybody who heard you"
                : "still listening",
            detail,
            Array.Empty<HeardReport>());
    }

    private static HeardSummary NothingCame()
        => new(
            HeardState.Nothing,
            "no reports came back",
            "No skimmer reported hearing you, and that is worth reading carefully "
            + "because it is not the same as nobody hearing you. Skimmer coverage "
            + "is uneven and there are large stretches with no machine listening at "
            + "all, so a band can be wide open to people and empty of receivers. "
            + "What this says is that no automated receiver picked you up. It does "
            + "not say your signal went nowhere.",
            Array.Empty<HeardReport>());
}
