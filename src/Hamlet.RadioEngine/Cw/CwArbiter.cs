namespace Hamlet.RadioEngine.Cw;

/// <summary>Which of the two readers a character came from. For the sheet, never the CW tab (HM-REQ-121).</summary>
public enum CwReader
{
    /// <summary>Hamlet's own decoder.</summary>
    Ours,

    /// <summary>The fldigi port, the second decoder.</summary>
    Second,
}

/// <summary>How the two readings of one span stood.</summary>
public enum CwArbitrationCase
{
    /// <summary>Both read the same character.</summary>
    Agree,

    /// <summary>Different characters, their p's more than <see cref="CwArbiter.TieMargin"/> apart.</summary>
    Disagree,

    /// <summary>Different characters, their p's within <see cref="CwArbiter.TieMargin"/>.</summary>
    Tie,

    /// <summary>Only ours read a character on the span.</summary>
    OneSidedOurs,

    /// <summary>Only the second decoder read a character on the span.</summary>
    OneSidedSecond,
}

/// <summary>What the arbiter did with one span, for the capture sheet (HM-REQ-121, 126, 127).</summary>
/// <param name="Emitted">Whose character was emitted, or null where nothing was.</param>
/// <param name="Case">How the two readings stood.</param>
/// <param name="OursText">Ours' character, or null where ours read none.</param>
/// <param name="OursP">Ours' p, or NaN.</param>
/// <param name="SecondText">The second decoder's character, or null where it read none.</param>
/// <param name="SecondP">Its p, or NaN.</param>
/// <param name="Vote">Who voted on the condition in force.</param>
public sealed record CwArbitration(
    CwReader? Emitted, CwArbitrationCase Case, string? OursText, double OursP, string? SecondText, double SecondP, CwVote Vote)
{
    /// <summary>The reader whose character was not emitted.</summary>
    public CwReader? Other => Emitted switch
    {
        CwReader.Ours => CwReader.Second,
        CwReader.Second => CwReader.Ours,
        _ => null,
    };

    /// <summary>The other reader's character, or null where it read none.</summary>
    public string? OtherText => Other == CwReader.Ours ? OursText : SecondText;

    /// <summary>The other reader's p, or NaN.</summary>
    public double OtherP => Other == CwReader.Ours ? OursP : SecondP;
}

/// <summary>The arbiter's output over a stream: what is emitted, and a record of every span.</summary>
/// <param name="Characters">The one transcript, word boundaries as ours read them.</param>
/// <param name="Records">Every span, emitted or not, in time order.</param>
public sealed record CwArbitrated(IReadOnlyList<CwCharacter> Characters, IReadOnlyList<CwArbitration> Records);

/// <summary>
/// Turns two readings of the same audio into one transcript (HM-REQ-124 to 127;
/// work instruction 465, task 2; PHASE_PLAN.md 9.6).
/// </summary>
/// <remarks>
/// <para>**A PURE TYPE, AFTER BOTH DECODERS** (465 DECIDED (2) to (6)). It changes
/// no letter, class, threshold or lattice of either; it chooses between what they
/// already read. It knows no word, callsign or letter frequency (R72).</para>
/// <para>**THE SAME SPAN** (DECIDED (2)): two characters overlapping by at least
/// <see cref="SameSpanShare"/> of the shorter span on the audio clock, paired one
/// to one, the largest overlap first.</para>
/// <para>**THE RULES**, where both vote (HM-REQ-124): an agreement is emitted with
/// the more confident decoder's class (HM-REQ-125); a disagreement goes to the
/// higher p at its own class (HM-REQ-126); a disagreement whose p's are within
/// <see cref="TieMargin"/> goes to the higher p, dim and never sure (HM-REQ-127).
/// Where one votes, its reading is emitted at its own class and the other's is on
/// the record only: an advisory reading never displaces a character or raises a
/// class (DECIDED (4)). Where neither votes, ours is emitted as it prints today. A
/// span only ours read is ours as today; a span only the second read is emitted
/// only where it votes. Every span is recorded (HM-REQ-121, 126, 127).</para>
/// </remarks>
public static class CwArbiter
{
    /// <summary>
    /// HM-REQ-127's margin: two p's within 0.05 of each other are a tie.
    /// </summary>
    /// <remarks>
    /// **THE REQUIREMENT'S RECOMMENDED VALUE, HELD UNTIL THE OWNER RULES** (465
    /// DECIDED (5); PHASE_PLAN.md section 6). HM-REQ-127 says the margin is TBD and
    /// needs ruling; the threshold is the owner's, and its absence never halts a
    /// unit.
    /// </remarks>
    public const double TieMargin = 0.05;

    /// <summary>The share of the shorter span two characters must overlap to be on the same span (465 DECIDED (2)).</summary>
    public const double SameSpanShare = 0.5;

    /// <summary>Where one of our characters starts and ends on the audio clock, in seconds.</summary>
    /// <param name="c">The character.</param>
    /// <returns><see cref="CwCharacter.At"/> is the end of its last hop; it starts <see cref="CwCharacter.SpanHops"/> hops before. NaN start where the span was not measured.</returns>
    public static (double Start, double End) SpanOf(CwCharacter c)
    {
        ArgumentNullException.ThrowIfNull(c);

        var end = c.At.TotalSeconds;

        return (c.SpanHops > 0 ? end - (c.SpanHops * CwProbabilisticDecoder.HopMilliseconds / 1000.0) : double.NaN, end);
    }

    /// <summary>The overlap of two spans that are on the same span, or null.</summary>
    public static double? SameSpan((double Start, double End) a, (double Start, double End) b)
    {
        if (!(double.IsFinite(a.Start) && double.IsFinite(a.End) && double.IsFinite(b.Start) && double.IsFinite(b.End)))
        {
            return null;
        }

        var overlap = Math.Min(a.End, b.End) - Math.Max(a.Start, b.Start);
        var shorter = Math.Min(a.End - a.Start, b.End - b.Start);

        return overlap > 0 && overlap >= SameSpanShare * shorter ? overlap : null;
    }

    /// <summary>Arbitrates a whole stream.</summary>
    /// <param name="ours">Our settled characters, word boundaries included, in order.</param>
    /// <param name="second">The second decoder's readings.</param>
    /// <param name="vote">Who votes on the condition in force.</param>
    /// <returns>The one transcript and the record of every span.</returns>
    public static CwArbitrated Arbitrate(IReadOnlyList<CwCharacter> ours, IReadOnlyList<CwSecondReading> second, CwVote vote)
    {
        ArgumentNullException.ThrowIfNull(ours);
        ArgumentNullException.ThrowIfNull(second);

        var partner = Pair(ours, second);
        var taken = new bool[second.Count];

        foreach (var q in partner.Values)
        {
            taken[q] = true;
        }

        // The spans only the second decoder read, in time order, each placed before the first of ours that starts after it.
        var alone = Enumerable.Range(0, second.Count).Where(q => !taken[q])
            .OrderBy(q => second[q].HasSpan ? second[q].Start : double.MaxValue).ToList();
        var next = 0;
        var characters = new List<CwCharacter>();
        var records = new List<(double At, CwArbitration Record)>();

        void AloneBefore(double at)
        {
            while (next < alone.Count && (!(at < double.MaxValue) || second[alone[next]].Start < at))
            {
                var s = second[alone[next++]];
                var (emitted, record) = DecideAlone(s, vote);

                records.Add((s.Start, record));

                if (emitted is not null)
                {
                    characters.Add(emitted);
                }
            }
        }

        for (var o = 0; o < ours.Count; o++)
        {
            var c = ours[o];
            var at = c.IsWordGap ? c.At.TotalSeconds : SpanOf(c).Start is var s && double.IsFinite(s) ? s : c.At.TotalSeconds;

            AloneBefore(at);

            if (c.IsWordGap)
            {
                characters.Add(c);
                continue;
            }

            var decided = Decide(c, partner.TryGetValue(o, out var q) ? second[q] : null, vote);

            characters.Add(decided);
            records.Add((at, decided.Arbitration!));
        }

        AloneBefore(double.MaxValue);

        return new CwArbitrated(characters, records.Select(r => r.Record).ToList());
    }

    /// <summary>
    /// The span rule over two whole streams (465 DECIDED (2)): every pair on the
    /// same span, taken one to one, the largest overlap first; ties to the earlier
    /// of ours, then the earlier of the second's.
    /// </summary>
    /// <param name="ours">Our characters; word boundaries are not paired.</param>
    /// <param name="second">The second decoder's readings.</param>
    /// <returns>For each of our characters paired, the index of its partner.</returns>
    public static IReadOnlyDictionary<int, int> Pair(IReadOnlyList<CwCharacter> ours, IReadOnlyList<CwSecondReading> second)
    {
        ArgumentNullException.ThrowIfNull(ours);
        ArgumentNullException.ThrowIfNull(second);

        var candidates = new List<(int O, int Q, double Overlap)>();

        for (var o = 0; o < ours.Count; o++)
        {
            if (ours[o].IsWordGap)
            {
                continue;
            }

            var a = SpanOf(ours[o]);

            for (var q = 0; q < second.Count; q++)
            {
                if (SameSpan(a, (second[q].Start, second[q].End)) is { } overlap)
                {
                    candidates.Add((o, q, overlap));
                }
            }
        }

        var partner = new Dictionary<int, int>();
        var taken = new HashSet<int>();

        foreach (var (o, q, _) in candidates.OrderByDescending(c => c.Overlap).ThenBy(c => c.O).ThenBy(c => c.Q))
        {
            if (!partner.ContainsKey(o) && taken.Add(q))
            {
                partner[o] = q;
            }
        }

        return partner;
    }

    /// <summary>One of our characters and the second decoder's reading of its span, or none: what is emitted, with its record.</summary>
    /// <param name="ours">Our character; never a word boundary.</param>
    /// <param name="second">The second decoder's character on the same span, or null.</param>
    /// <param name="vote">Who votes on the condition in force.</param>
    /// <returns>The character emitted, carrying its <see cref="CwCharacter.Arbitration"/>.</returns>
    public static CwCharacter Decide(CwCharacter ours, CwSecondReading? second, CwVote vote)
    {
        ArgumentNullException.ThrowIfNull(ours);

        if (second is null)
        {
            // One-sided: ours as it prints today, whether or not it votes (465 DECIDED (4)).
            return ours with
            {
                Arbitration = new CwArbitration(CwReader.Ours, CwArbitrationCase.OneSidedOurs, ours.Text, ours.Probability, null, double.NaN, vote),
            };
        }

        var agree = ours.Text == second.Text;
        var kase = agree ? CwArbitrationCase.Agree
            : Math.Abs(ours.Probability - second.P) <= TieMargin ? CwArbitrationCase.Tie
            : CwArbitrationCase.Disagree;

        CwArbitration Record(CwReader emitted) => new(emitted, kase, ours.Text, ours.Probability, second.Text, second.P, vote);

        // Only one votes, or neither: the voter's reading at its own class; with neither, ours as today (HM-REQ-124).
        if (!vote.Ours || !vote.Second)
        {
            if (vote.Second && !vote.Ours)
            {
                return Second(ours, second, second.Confidence, Record(CwReader.Second));
            }

            return ours with { Arbitration = Record(CwReader.Ours) };
        }

        // Both vote. The higher p wins; an exact draw stays with ours.
        var secondWins = second.P > ours.Probability;

        if (agree)
        {
            // HM-REQ-125: the same character, with the more confident decoder's class.
            return secondWins
                ? ours with { Confidence = second.Confidence, Arbitration = Record(CwReader.Second) }
                : ours with { Arbitration = Record(CwReader.Ours) };
        }

        if (kase == CwArbitrationCase.Tie)
        {
            // HM-REQ-127: the winner, dim and never sure; a placeholder stays a placeholder.
            return secondWins
                ? Second(ours, second, Dim(second.Confidence), Record(CwReader.Second))
                : ours with { Confidence = Dim(ours.Confidence), Arbitration = Record(CwReader.Ours) };
        }

        // HM-REQ-126: the higher calibrated p, at its own class.
        return secondWins
            ? Second(ours, second, second.Confidence, Record(CwReader.Second))
            : ours with { Arbitration = Record(CwReader.Ours) };
    }

    /// <summary>A span only the second decoder read: emitted only where it votes, recorded either way.</summary>
    /// <param name="second">Its reading.</param>
    /// <param name="vote">Who votes on the condition in force.</param>
    /// <returns>The character emitted or null, and the record.</returns>
    public static (CwCharacter? Emitted, CwArbitration Record) DecideAlone(CwSecondReading second, CwVote vote)
    {
        ArgumentNullException.ThrowIfNull(second);

        if (!vote.Second)
        {
            return (null, new CwArbitration(null, CwArbitrationCase.OneSidedSecond, null, double.NaN, second.Text, second.P, vote));
        }

        var record = new CwArbitration(CwReader.Second, CwArbitrationCase.OneSidedSecond, null, double.NaN, second.Text, second.P, vote);
        var hops = second.HasSpan ? (int)Math.Round((second.End - second.Start) * 1000.0 / CwProbabilisticDecoder.HopMilliseconds) : 0;
        var at = TimeSpan.FromSeconds(double.IsFinite(second.End) ? second.End : 0);

        return (new CwCharacter(second.Text, second.Confidence, second.P, second.Pattern, double.NaN, second.WordsPerMinute, at)
        {
            SpanHops = hops,
            Probability = second.P,
            Arbitration = record,
        }, record);
    }

    // The second decoder's character in our character's place: its text, pattern, class and p; our clock.
    private static CwCharacter Second(CwCharacter ours, CwSecondReading second, CwConfidence confidence, CwArbitration record)
        => ours with
        {
            Text = second.Text,
            Pattern = second.Pattern,
            Confidence = confidence,
            Probability = second.P,
            Arbitration = record,
        };

    private static CwConfidence Dim(CwConfidence c) => c == CwConfidence.Unreadable ? CwConfidence.Unreadable : CwConfidence.Low;
}
