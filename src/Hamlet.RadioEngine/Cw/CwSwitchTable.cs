namespace Hamlet.RadioEngine.Cw;

/// <summary>What is emitted on a condition (HM-REQ-128).</summary>
public enum CwArbitrationSwitch
{
    /// <summary>The arbiter decides every span, as unit 465 built it.</summary>
    Arbitrate,

    /// <summary>Ours alone is emitted, as ours prints alone; the port's reading is on the record only.</summary>
    OursAlone,

    /// <summary>The port alone is emitted, at parity.md section 1's mapping; ours' reading is on the record only.</summary>
    PortAlone,
}

/// <summary>One metric's figure for one output on one condition: a count over what it is counted of.</summary>
/// <param name="Count">The numerator.</param>
/// <param name="Of">The denominator; zero where the metric is not defined for the output.</param>
public readonly record struct CwFigure(int Count, int Of)
{
    /// <summary>True where there is something to divide by.</summary>
    public bool Defined => Of > 0;
}

/// <summary>What the rule found on one condition.</summary>
/// <param name="LostOn">Each metric where the arbitration is strictly worse than a decoder alone, as `metric to ours` or `metric to port`; empty where it does not lose.</param>
/// <param name="Better">The better decoder alone.</param>
/// <param name="BetterBy">How that was decided: dominance, the first metric in the order that differs, or a full tie.</param>
/// <param name="Switch">What is emitted there.</param>
public sealed record CwSwitchVerdict(IReadOnlyList<string> LostOn, CwReader Better, string BetterBy, CwArbitrationSwitch Switch)
{
    /// <summary>True where the arbitration loses to either decoder alone.</summary>
    public bool Loses => LostOn.Count > 0;
}

/// <summary>
/// Where the arbitration earns its place (HM-REQ-128; work instruction 466;
/// PHASE_PLAN.md 9.7).
/// </summary>
/// <remarks>
/// <para>**THE RULE WAS FIXED AT TASK 1, BEFORE ANY THREE-WAY FIGURE WAS SEEN**
/// (V-14; `TheArbitrationEarnsItsPlaceFact`'s header, `9eef6850`). It knows no
/// word, callsign or letter frequency (R72); it reads figures only.</para>
/// <list type="bullet">
/// <item>**The loss rule** (466 DECIDED (3)): the arbitration loses where it is
/// strictly worse than either decoder alone on any metric defined for both of
/// the pair, a rate compared as a rate.</item>
/// <item>**The better decoder** (466 DECIDED (4)): the one no worse on every
/// metric defined for both; else the first metric in <see cref="Order"/> where
/// they differ; ours on a full tie.</item>
/// <item>**The switch**: no loss, <see cref="CwArbitrationSwitch.Arbitrate"/>;
/// a loss, the better decoder alone (HM-REQ-128).</item>
/// </list>
/// </remarks>
public static class CwSwitchTable
{
    /// <summary>The metrics, honesty first (466 DECIDED (4)): 011, 010, 013, 012, 014, 081, 083, 084.</summary>
    public static IReadOnlyList<string> Order { get; } = new[] { "011", "010", "013", "012", "014", "081", "083", "084" };

    /// <summary>The metrics where more is better: coverage, dim accuracy and the named spans read.</summary>
    public static IReadOnlySet<string> HigherIsBetter { get; } = new HashSet<string>(StringComparer.Ordinal) { "012", "014", "084" };

    /// <summary>How one output stands against another on one metric.</summary>
    /// <param name="metric">One of <see cref="Order"/>.</param>
    /// <param name="a">The first output's figure.</param>
    /// <param name="b">The second's.</param>
    /// <returns>Negative where a is better, positive where a is worse, zero where equal; null where either is not defined.</returns>
    public static int? Compare(string metric, CwFigure a, CwFigure b)
    {
        if (!a.Defined || !b.Defined)
        {
            return null;
        }

        // a.Count / a.Of against b.Count / b.Of, exactly.
        var sign = ((long)a.Count * b.Of).CompareTo((long)b.Count * a.Of);

        return HigherIsBetter.Contains(metric) ? -sign : sign;
    }

    /// <summary>The rule on one condition.</summary>
    /// <param name="arbitrated">The arbitrated output's figures, by metric; a metric absent is not compared.</param>
    /// <param name="ours">Ours alone.</param>
    /// <param name="port">The port alone.</param>
    /// <returns>Where the arbitration loses, the better decoder alone, and the switch.</returns>
    public static CwSwitchVerdict Choose(
        IReadOnlyDictionary<string, CwFigure> arbitrated,
        IReadOnlyDictionary<string, CwFigure> ours,
        IReadOnlyDictionary<string, CwFigure> port)
    {
        ArgumentNullException.ThrowIfNull(arbitrated);
        ArgumentNullException.ThrowIfNull(ours);
        ArgumentNullException.ThrowIfNull(port);

        int? On(IReadOnlyDictionary<string, CwFigure> x, IReadOnlyDictionary<string, CwFigure> y, string m)
            => x.TryGetValue(m, out var fx) && y.TryGetValue(m, out var fy) ? Compare(m, fx, fy) : null;

        var lost = new List<string>();

        foreach (var m in Order)
        {
            if (On(arbitrated, ours, m) > 0)
            {
                lost.Add($"{m} to ours");
            }

            if (On(arbitrated, port, m) > 0)
            {
                lost.Add($"{m} to port");
            }
        }

        var between = Order.Select(m => (Metric: m, Sign: On(ours, port, m))).Where(x => x.Sign is not null).ToList();
        CwReader better;
        string by;

        if (between.All(x => x.Sign == 0))
        {
            (better, by) = (CwReader.Ours, "a full tie, ours");
        }
        else if (between.All(x => x.Sign <= 0))
        {
            (better, by) = (CwReader.Ours, "dominance");
        }
        else if (between.All(x => x.Sign >= 0))
        {
            (better, by) = (CwReader.Second, "dominance");
        }
        else
        {
            var first = between.First(x => x.Sign != 0);

            (better, by) = (first.Sign < 0 ? CwReader.Ours : CwReader.Second, $"order, at {first.Metric}");
        }

        var choice = lost.Count == 0 ? CwArbitrationSwitch.Arbitrate
            : better == CwReader.Ours ? CwArbitrationSwitch.OursAlone
            : CwArbitrationSwitch.PortAlone;

        return new CwSwitchVerdict(lost, better, by, choice);
    }
}
