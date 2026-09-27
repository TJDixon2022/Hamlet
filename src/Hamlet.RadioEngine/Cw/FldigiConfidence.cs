using Hamlet.RadioEngine.Cw.Second;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// A probability of being right for every character the fldigi port prints,
/// computed outside the port from what it already exposes (HM-REQ-124; work
/// instruction 464; PHASE_PLAN.md 9.5).
/// </summary>
/// <remarks>
/// <para>**THE PORT ITSELF CARRIES NO CONFIDENCE.** fldigi has none, and the
/// second decoder stays as ported (HM-REQ-122, 129): nothing under
/// <c>Cw/Second/</c> is changed for this, not even to expose a field. Every
/// quantity is read from the port's public <see cref="FldigiCwDecoder.KeyEvents"/>,
/// <see cref="FldigiCwDecoder.Emissions"/> and, with
/// <see cref="FldigiCwDecoder.TraceDecisions"/> on, <see cref="FldigiCwDecoder.Decisions"/>.</para>
/// <para>**DERIVED FROM TWO THINGS fldigi's RECEIVER MEASURES**, `cw.cxx` at
/// `61b97f41`:</para>
/// <list type="bullet">
/// <item>the level margin, 20 log10(sig_avg / noise_floor) in dB at the
/// character's last up event - <c>sig_avg</c> at cw.cxx:610 and
/// <c>noise_floor</c> at 612-616, the same ratio fldigi's own squelch metric reads
/// at 635-636;</item>
/// <item>the timing margin, the least distance of any of the character's element
/// lengths from the dot/dash split, in fldigi's own dit - the element's length at
/// cw.cxx:811-812, the split <c>element_usec &lt;= two_dots</c> at 847, and the dit
/// <c>two_dots / 2</c> at 502.</item>
/// </list>
/// <para>**ONE LOGISTIC, FIXED BEFORE IT WAS MEASURED** (464 task 1, V-14):
/// p = 1 / (1 + e^-(a + b.level + c.timing)), fitted by maximum likelihood over
/// the whole keyed corpus, real and synthetic together, to right against wrong or
/// added under `parity.md`'s mapping. A character where either margin cannot be
/// read takes one constant. Calibration is measured held-out, per condition, in
/// `docs/phase-requirements/calibration.md`.</para>
/// <para>**THE DECISION ROWS ARE NEEDED FOR ORDER.** The port's filter hands out
/// 1024 samples at a time, so an emission and the next character's first key
/// events can carry the same <see cref="FldigiCwKeyEvent.InputSample"/>; the
/// decision row each was raised in is the only thing that orders them.</para>
/// <para>Wired to nothing the operator sees (HM-REQ-121). That is 9.6's.</para>
/// </remarks>
public static class FldigiConfidence
{
    /// <summary>The logistic's intercept, a: the whole pool's fit, 407 characters (work instruction 464, task 2).</summary>
    public const double Intercept = 0.37776906958829465;

    /// <summary>The logistic's slope on the level margin in dB, b.</summary>
    public const double LevelSlope = 0.18544731603622516;

    /// <summary>The logistic's slope on the timing margin in fldigi's dits, c.</summary>
    public const double TimingSlope = -0.40029810858361803;

    /// <summary>The probability given a character where a margin cannot be read: the pool's share right, 311 of 407, since every scored character's margins were readable.</summary>
    public const double NotFinite = 0.7641277641277642;

    /// <summary>What the port's receiver measured about one printed character.</summary>
    /// <param name="LevelDb">20 log10(sig_avg / noise_floor) at its last up event, or NaN.</param>
    /// <param name="TimingDits">min |element - two_dots| / (two_dots / 2) over its elements, or NaN.</param>
    /// <param name="Elements">The up events found for it; nought where none were.</param>
    public sealed record Margins(double LevelDb, double TimingDits, int Elements);

    /// <summary>One probability per emission of a port run with its decisions traced.</summary>
    /// <param name="port">The port, run with <see cref="FldigiCwDecoder.TraceDecisions"/> on.</param>
    /// <returns>Aligned with <see cref="FldigiCwDecoder.Emissions"/>; a word space, not a character, carries NaN.</returns>
    public static IReadOnlyList<double> For(FldigiCwDecoder port)
    {
        ArgumentNullException.ThrowIfNull(port);

        return For(port.Emissions, port.KeyEvents, port.Decisions);
    }

    /// <summary>One probability per emission.</summary>
    /// <param name="emissions">The port's emissions.</param>
    /// <param name="keyEvents">The port's key events.</param>
    /// <param name="decisions">The port's decision rows, traced over the whole run.</param>
    /// <returns>Aligned with the emissions; a word space carries NaN.</returns>
    public static IReadOnlyList<double> For(
        IReadOnlyList<FldigiCwEmission> emissions,
        IReadOnlyList<FldigiCwKeyEvent> keyEvents,
        IReadOnlyList<FldigiCwDecisionRow> decisions)
    {
        var margins = Read(emissions, keyEvents, decisions);

        return margins.Select(m => m is null ? double.NaN : Of(m.LevelDb, m.TimingDits)).ToList();
    }

    /// <summary>The probability that a character with these margins is right.</summary>
    /// <param name="levelDb">The level margin in dB.</param>
    /// <param name="timingDits">The timing margin in fldigi's dits.</param>
    /// <returns>A probability from 0 to 1.</returns>
    public static double Of(double levelDb, double timingDits)
        => double.IsFinite(levelDb) && double.IsFinite(timingDits)
            ? 1 / (1 + Math.Exp(-(Intercept + LevelSlope * levelDb + TimingSlope * timingDits)))
            : NotFinite;

    /// <summary>The margins of every emission, read from the port's public record.</summary>
    /// <param name="emissions">The port's emissions.</param>
    /// <param name="keyEvents">The port's key events.</param>
    /// <param name="decisions">The port's decision rows, traced over the whole run.</param>
    /// <returns>Aligned with the emissions; null for a word space.</returns>
    public static IReadOnlyList<Margins?> Read(
        IReadOnlyList<FldigiCwEmission> emissions,
        IReadOnlyList<FldigiCwKeyEvent> keyEvents,
        IReadOnlyList<FldigiCwDecisionRow> decisions)
    {
        ArgumentNullException.ThrowIfNull(emissions);
        ArgumentNullException.ThrowIfNull(keyEvents);
        ArgumentNullException.ThrowIfNull(decisions);

        if (decisions.Count == 0 && emissions.Count > 0)
        {
            throw new ArgumentException("the port must be run with TraceDecisions on: only its decision rows order an emission against the key events beside it", nameof(decisions));
        }

        var result = new List<Margins?>(emissions.Count);
        var ups = new List<FldigiCwKeyEvent>();
        var k = 0;

        foreach (var row in decisions)
        {
            foreach (var token in row.Events.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (token is not ("down" or "up" or "spike" or "overflow"))
                {
                    continue;
                }

                var ev = keyEvents[k++];

                if (ev.Kind == "up")
                {
                    ups.Add(ev);
                }
            }

            if (row.Printed.Length > 0)
            {
                var e = emissions[result.Count];

                result.Add(e.Text == MorseAlphabet.WordGap ? null : MarginsOf(ups, e));
                ups = new List<FldigiCwKeyEvent>();
            }
        }

        if (k != keyEvents.Count || result.Count != emissions.Count)
        {
            throw new ArgumentException($"the decision rows account for {k} of {keyEvents.Count} key events and {result.Count} of {emissions.Count} emissions", nameof(decisions));
        }

        return result;
    }

    // The last Representation.Length up events before the emission, the last carrying its representation.
    private static Margins MarginsOf(IReadOnlyList<FldigiCwKeyEvent> upsBefore, FldigiCwEmission e)
    {
        var want = e.Representation.Length;
        var ups = upsBefore.Skip(Math.Max(0, upsBefore.Count - want)).ToList();

        if (want == 0 || ups.Count != want || ups[^1].Representation != e.Representation)
        {
            return new Margins(double.NaN, double.NaN, 0);
        }

        var last = ups[^1];
        var level = last.NoiseFloor > 0 && last.SigAvg > 0
            ? 20 * Math.Log10(last.SigAvg / last.NoiseFloor)
            : double.NaN;
        var timing = ups.Min(u => u.TwoDots > 0 ? Math.Abs(u.Element - u.TwoDots) / (u.TwoDots / 2.0) : double.NaN);

        return new Margins(level, timing, ups.Count);
    }
}
