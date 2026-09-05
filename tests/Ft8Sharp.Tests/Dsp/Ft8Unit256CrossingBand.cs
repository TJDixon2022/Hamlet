namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE 50 PER CENT CROSSING, AND THE BAND THAT SAYS WHAT 306 TRIALS CAN SEPARATE.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>No crossing in this project had ever been computed in code.</b> Every crossing in
/// <c>docs/unit248-*.md</c>, <c>docs/unit252-*.md</c> and <c>docs/unit255-closing-measurement.md</c>
/// was interpolated by hand in prose and published as a bare point value — <c>-19.90 dB</c>,
/// <c>-19.61 dB</c> — with the intervals printed beside it belonging to the two rungs and not to
/// the crossing. This is the first time the arithmetic is executed.
/// </para>
/// <para>
/// <b>THE BAND IS NOT A CONFIDENCE INTERVAL ON THE CROSSING, and nothing that prints it may call
/// it one.</b> It is obtained by pushing each rung's 95 per cent Wilson bounds through the same
/// linear interpolation the point crossing already uses, under the assumption that document
/// already makes — <b>that the decode rate moves linearly in decibels between two rungs one
/// decibel apart</b>. Join the two <em>upper</em> bounds and you have the optimistic curve, which
/// reaches 50 per cent at the lower (better) ratio; join the two <em>lower</em> bounds and you
/// have the pessimistic curve, which reaches it at the higher ratio. <b>The pair is the band.</b>
/// </para>
/// <para>
/// <b>A SIDE THAT DOES NOT REACH 50 PER CENT INSIDE THE BRACKET IS OPEN AND IS NEVER
/// EXTRAPOLATED.</b> <c>SHIPPING</c> on the grid is exactly that case: its -20 dB rung's Wilson
/// upper bound is <c>50.700</c>, still above 50, so the optimistic curve never crosses inside
/// <c>[-20, -19]</c> and the honest statement is <em>at least as good as the pessimistic end, and
/// this ladder cannot put a floor under how much better</em>.
/// </para>
/// <para>
/// <b>WHY THE PAIRING IS THE THING WORTH GETTING RIGHT.</b> Write the crossing's position in the
/// bracket as <c>t(a, b) = (a - 50) / (a - b)</c>, with <c>a</c> the upper rung's rate and
/// <c>b</c> the lower rung's. Both partial derivatives are positive — <c>∂t/∂a = (50 - b)/(a -
/// b)²</c> and <c>∂t/∂b = (a - 50)/(a - b)²</c> — so <c>t</c> is increasing in <em>both</em>
/// arguments. That is what makes <c>t(hi, hi)</c> the extreme optimistic end and <c>t(lo, lo)</c>
/// the extreme pessimistic one, and it is why <b>pairing an upper bound at one rung against a
/// lower bound at the other does not merely narrow the band — it can invert it</b>, putting the
/// optimistic end at a worse ratio than the point crossing it is supposed to bracket. Unit 256's
/// watched failure is that inversion, on <c>SHIPPING</c> on the grid.
/// </para>
/// <para>
/// <b>Nothing here asserts a bound on any crossing.</b> Targets are waypoints; this type computes
/// and reports and never judges.
/// </para>
/// </remarks>
internal static class Ft8Unit256CrossingBand
{
    /// <summary>The rate the crossing is defined at, in per cent.</summary>
    internal const double HalfPerCent = 50.0;

    /// <summary>One rung of a bracket: where it was measured and what came back.</summary>
    /// <param name="Decibels">The ratio asked for, in decibels in the 2500 Hz reference bandwidth.</param>
    /// <param name="Decoded">How many trials returned the message that was sent.</param>
    /// <param name="Trials">How many trials were walked.</param>
    internal readonly record struct Rung(double Decibels, int Decoded, int Trials)
    {
        /// <summary>Decodes as a percentage of trials.</summary>
        internal double Rate => Trials == 0 ? double.NaN : 100.0 * Decoded / Trials;

        /// <summary>The 95 per cent Wilson score interval on <see cref="Rate"/>, in per cent.</summary>
        internal (double Lower, double Upper) Interval => Ft8Step6Ladder.Wilson(Decoded, Trials);

        /// <summary>The one line a bracket table prints this rung as.</summary>
        public override string ToString()
        {
            var (lower, upper) = Interval;
            return $"{Decibels,6:F1} dB  {Decoded,4} of {Trials,4}  {Rate,6:F2} % "
                + $"({lower,6:F2} - {upper,6:F2})";
        }
    }

    /// <summary>
    /// <b>A point crossing and the band around it.</b> Every field is in decibels except the flags.
    /// </summary>
    /// <param name="Bracketed">
    /// <see langword="false"/> when the two rungs do not straddle 50 per cent, in which case every
    /// decibel field is <see cref="double.NaN"/> and nothing may be quoted from this record.
    /// </param>
    /// <param name="Point">Where the measured rates cross 50 per cent.</param>
    /// <param name="Optimistic">
    /// Where the curve through both <em>upper</em> Wilson bounds crosses 50 per cent — the better
    /// (more negative) end. <see cref="double.NaN"/> when <paramref name="OptimisticOpen"/>.
    /// </param>
    /// <param name="Pessimistic">
    /// Where the curve through both <em>lower</em> Wilson bounds crosses 50 per cent — the worse
    /// (less negative) end. <see cref="double.NaN"/> when <paramref name="PessimisticOpen"/>.
    /// </param>
    /// <param name="OptimisticOpen">
    /// <b>The optimistic curve is still above 50 per cent at the lower rung</b>, so it does not
    /// cross inside the bracket and that side of the band is open. <b>Never extrapolated.</b>
    /// </param>
    /// <param name="PessimisticOpen">
    /// <b>The pessimistic curve is already below 50 per cent at the upper rung</b>, so it does not
    /// cross inside the bracket and that side of the band is open.
    /// </param>
    /// <param name="Upper">The rung with the higher rate — the less negative ratio.</param>
    /// <param name="Lower">The rung with the lower rate — the more negative ratio.</param>
    internal readonly record struct Band(
        bool Bracketed,
        double Point,
        double Optimistic,
        double Pessimistic,
        bool OptimisticOpen,
        bool PessimisticOpen,
        Rung Upper,
        Rung Lower)
    {
        /// <summary>
        /// <b>The band written out the way the closing document must write it</b> — with an open
        /// side said to be open, against the rung it is open beyond.
        /// </summary>
        internal string BandText
        {
            get
            {
                if (!Bracketed)
                {
                    return "not bracketed";
                }

                var low = OptimisticOpen
                    ? $"open beyond {Lower.Decibels:F1} dB"
                    : $"{Optimistic:F2} dB";
                var high = PessimisticOpen
                    ? $"open above {Upper.Decibels:F1} dB"
                    : $"{Pessimistic:F2} dB";

                return $"{low} to {high}";
            }
        }

        /// <summary>
        /// How wide the band is in decibels, or <see cref="double.NaN"/> where either side is open
        /// — <b>an open side has no width and none is invented for it.</b>
        /// </summary>
        internal double WidthDecibels =>
            !Bracketed || OptimisticOpen || PessimisticOpen
                ? double.NaN
                : Pessimistic - Optimistic;

        /// <summary>
        /// <b>Whether the point crossing lies inside the band</b>, treating an open side as
        /// unbounded. <b>This must be true for every band this project publishes</b>, and the whole
        /// of unit 256's watched failure is a pairing that makes it false.
        /// </summary>
        internal bool ContainsPoint =>
            Bracketed
            && (OptimisticOpen || Point >= Optimistic - Tolerance)
            && (PessimisticOpen || Point <= Pessimistic + Tolerance);
    }

    /// <summary>
    /// A hundredth of a hundredth of a decibel — far below anything this project quotes, and there
    /// only so that a band whose end IS the point does not fail on a floating-point last bit.
    /// </summary>
    private const double Tolerance = 1e-9;

    /// <summary>
    /// <b>THE ENTRY POINT: two rungs in, a point crossing and its band out.</b>
    /// </summary>
    /// <param name="upper">The rung with the higher rate. Its ratio is the less negative one.</param>
    /// <param name="lower">The rung with the lower rate. Its ratio is the more negative one.</param>
    /// <remarks>
    /// <b>Not straddled is a result and not an error.</b> Where the two rungs are on the same side
    /// of 50 per cent this returns a record with <see cref="Band.Bracketed"/> false and nothing
    /// else in it, because quoting a crossing from two rungs on the same side would be an
    /// extrapolation and unit 255's ruling 3 forbids it.
    /// </remarks>
    internal static Band Crossing(Rung upper, Rung lower)
    {
        var pointUpper = upper.Rate;
        var pointLower = lower.Rate;

        if (!(pointUpper > HalfPerCent) || !(pointLower < HalfPerCent))
        {
            return new Band(
                false, double.NaN, double.NaN, double.NaN, false, false, upper, lower);
        }

        var (loUpperRung, hiUpperRung) = upper.Interval;
        var (loLowerRung, hiLowerRung) = lower.Interval;

        var point = Interpolate(upper.Decibels, pointUpper, lower.Decibels, pointLower);

        // THE OPTIMISTIC CURVE joins the two UPPER Wilson bounds. It lies at or above the measured
        // curve at both rungs, so it reaches 50 per cent at or before it does - the better ratio.
        // It does not reach 50 inside the bracket at all when the lower rung's upper bound is
        // itself still above 50, and that side is then OPEN and is not extrapolated.
        var optimisticOpen = hiLowerRung >= HalfPerCent;
        var optimistic = optimisticOpen
            ? double.NaN
            : Interpolate(upper.Decibels, hiUpperRung, lower.Decibels, hiLowerRung);

        // THE PESSIMISTIC CURVE joins the two LOWER Wilson bounds, and is open on its own side when
        // the upper rung's lower bound has already fallen below 50.
        var pessimisticOpen = loUpperRung <= HalfPerCent;
        var pessimistic = pessimisticOpen
            ? double.NaN
            : Interpolate(upper.Decibels, loUpperRung, lower.Decibels, loLowerRung);

        return new Band(
            true, point, optimistic, pessimistic, optimisticOpen, pessimisticOpen, upper, lower);
    }

    /// <summary>The same, from two walked rungs of <see cref="Ft8LadderHarness"/>.</summary>
    /// <remarks>
    /// <b>The rung's decibels are taken from the caller and not from
    /// <c>Result.Requested</c></b>, so a caller that walked a rung under one label and reports it
    /// under another cannot do so silently. The counts come from the result and nothing else.
    /// </remarks>
    internal static Band Crossing(
        (double Decibels, Ft8LadderHarness.Result Result) upper,
        (double Decibels, Ft8LadderHarness.Result Result) lower) =>
        Crossing(
            new Rung(upper.Decibels, upper.Result.Decoded, upper.Result.Trials),
            new Rung(lower.Decibels, lower.Result.Decoded, lower.Result.Trials));

    /// <summary>
    /// <b>The linear rule, and the only place it is written.</b> Where the curve joining
    /// <c>(dU, pU)</c> to <c>(dL, pL)</c> passes 50 per cent.
    /// </summary>
    private static double Interpolate(double dU, double pU, double dL, double pL) =>
        dU + ((dL - dU) * (pU - HalfPerCent) / (pU - pL));

    /// <summary>The header <see cref="AsRow"/> lines up under.</summary>
    internal const string Header =
        "column               placement    upper rung                            "
        + "lower rung                            point       band                             width";

    /// <summary>One line of the crossings table, with both rungs and their intervals beside it.</summary>
    internal static string AsRow(string column, string placement, Band band)
    {
        var point = band.Bracketed ? $"{band.Point,8:F2}" : "     n/a";
        var width = double.IsNaN(band.WidthDecibels) ? "  open" : $"{band.WidthDecibels,6:F3}";

        return $"{column,-20} {placement,-12} {band.Upper,-37} {band.Lower,-37} "
            + $"{point}  {band.BandText,-32} {width}";
    }
}
