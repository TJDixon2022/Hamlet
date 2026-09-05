using System;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>How hard the ordered statistics stage tries, or nothing at all.</b> A null settings reference
/// on <see cref="Ft8DeepSlotDecoder"/> means the stage is off and the sibling does exactly what the
/// port does.
/// </summary>
/// <remarks>
/// <para>
/// <b>OFF IS THE DEFAULT AND OFF IS AN EXACT REPRODUCTION.</b> With no settings the sibling's
/// per-candidate loop runs the port's stages, in the port's order, with the port's counts and the
/// port's de-duplication key - and <c>Ft8DeepIdentityTests</c> holds it to returning the whole
/// <c>Ft8SlotResult</c> the port returns over two ladder blocks and the committed capture. That test
/// is what makes a difference between the OSD-off and OSD-on columns of the scoreboard attributable
/// to this setting and to nothing else.
/// </para>
/// <para>
/// <b><see cref="Order"/> is the number of basis positions the search is allowed to flip</b>, which
/// is Fossorier and Lin's λ (M. P. C. Fossorier and S. Lin, "Soft-decision decoding of linear block
/// codes based on ordered statistics", IEEE Transactions on Information Theory 41(5), September 1995,
/// pages 1379-1396). Order 0 re-encodes the most reliable basis as it stands; order λ additionally
/// tries every subset of the basis of size 1 to λ. The cost is
/// <c>sum over i of C(91, i)</c> re-encodings per candidate, which is 1, 92, 4187 and 125672 for
/// orders 0, 1, 2 and 3.
/// </para>
/// <para>
/// <b><see cref="Window"/> is how much of that basis the flips may fall in</b>, counted from the
/// least reliable end. It is Fossorier and Lin's own segmentation of the most reliable basis, from
/// the same 1995 paper, and it is the second knob: order says how many positions may be flipped,
/// window says which positions those may be. The cost is <c>1 + sum over i of C(window, i)</c>, so
/// order 3 over a window of 40 costs 10 701 re-encodings a candidate against the full basis's
/// 125 672 - <b>eleven times cheaper at the same order</b>.
/// </para>
/// <para>
/// <b>There is no threshold, no confidence and no acceptance rule here</b>, because
/// <c>Ft8Sharp.Deep</c> never decides that a message is real. Whatever this stage produces is handed
/// to the port's <c>Ft8CodewordDecoder</c> and accepted or refused by the port's own parity and
/// CRC-14 gates. See <c>src/Ft8Sharp.Deep/porting-notes.md</c>.
/// </para>
/// <para>
/// <b>AND THE WINDOW IS NOT AN ACCEPTANCE RULE EITHER.</b> It is not a threshold, not a confidence
/// and not a gate; it narrows which patterns the search enumerates and nothing else. It does not
/// change how many codewords are put to the port's gates - the stage produces exactly one codeword
/// per candidate offered, before and after, and <c>docs/unit252-osd-window.md</c> §4 writes the
/// arithmetic out.
/// </para>
/// </remarks>
public sealed class Ft8DeepOsdSettings
{
    /// <summary>
    /// <b>The whole most reliable basis, 91 positions</b>, which is what a window defaults to and is
    /// what shipped before unit 252. <c>Ft8DeepOrderedStatistics.BasisBits</c>.
    /// </summary>
    public const int FullBasis = Ft8DeepOrderedStatistics.BasisBits;

    /// <summary>The largest order this library will run. Beyond it the search stops being tractable.</summary>
    /// <remarks>
    /// A bound rather than a tuning: order 4 is about two and a half million re-encodings per
    /// candidate, and a slot carrying a hundred candidates would not finish inside FT8's fifteen
    /// seconds. The bound is refused loudly rather than clamped silently, because a caller who asked
    /// for order 5 and got order 3 would be reading a measurement of something it did not ask for.
    /// </remarks>
    public const int MaximumOrder = 4;

    /// <summary>
    /// <b>The order this library uses when nobody names one, and it is read off a measurement.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unit 246 task 6, one whole 51-trial block at -21 dB, every row seeing the same seed and the
    /// same noise draw, with the port at 3 of 51 and zero wrong on every row:
    /// </para>
    /// <code>
    /// order   decoded   ms/trial   worst slot ms   codewords the port accepted
    ///     0         3       66.3           102.1                             0
    ///     1         4       65.8            75.5                             1
    ///     2         4       74.3           110.1                             1
    ///     3         5      311.4           511.6                             2
    /// </code>
    /// <para>
    /// <b>The gains cannot be separated at 51 trials</b> - one decode either way is well inside the
    /// noise - so the choice is made on cost against the headroom task 1 measured. Task 1's ceiling
    /// admits <b>7 of 51 trials at order 2 against 2 of 51 at order 1</b>, and order 2 costs 8.8 ms a
    /// trial more than the port with a worst observed slot of 110 ms, which is a 136-fold margin
    /// against FT8's 15 seconds. <b>Order 2 is therefore where the headroom is at a price that is
    /// nothing.</b>
    /// </para>
    /// <para>
    /// <b>Order 3 is not ruled out and is not the default.</b> It bought one more decode of 51 and
    /// cost 246 ms a trial, and one decode of 51 is not a difference this table can resolve. Resolving
    /// it needs more trials, not a bigger claim.
    /// </para>
    /// <para>
    /// <b>Nothing here was tuned to a target.</b> No order in this table reaches step 2's 40 per cent
    /// at -21 dB, and the default was not chosen by trying settings until one passed.
    /// </para>
    /// <para>
    /// <b>Task 7 then ran this default over the whole ladder</b> - 306 trials at each of -19, -20 and
    /// -21 dB - and it took the -21 dB rate from <b>4.2 per cent (13 of 306) to 10.8 per cent (33 of
    /// 306)</b> with a 95 per cent Wilson interval of 7.8 to 14.8 and <b>zero wrong decodes on every
    /// rung</b>, at 72.5 ms a trial against the port's 64.1 and a worst observed slot of 110 ms.
    /// Whether order 1 would have done as much at 306 trials is not known and is not claimed.
    /// </para>
    /// <para>
    /// <b>UNIT 252 LOOKED AGAIN WITH A WINDOW AVAILABLE, AND LEFT THIS WHERE IT IS.</b> Three rungs,
    /// 306 trials each, five columns, fine sync off, the same audio to all five, and <b>zero wrong
    /// decodes on all fifteen rows</b>. The port and the order-2 column reproduced unit 246's
    /// figures to the decode, which is what says the instrument had not moved:
    /// </para>
    /// <code>
    /// column          -19 dB rate   -20 dB rate   -21 dB rate (of 306)   95% Wilson   ms/trial   worst slot
    /// Ft8Sharp               81.0          23.9              4.2 (13)      2.5-7.1       63.9        97.0
    /// Deep OSD off           81.0          23.9              4.2 (13)      2.5-7.1       64.2       100.8
    /// order 2, full          90.2          40.8             10.8 (33)     7.8-14.8       72.3       106.1
    /// order 3, window 60     90.5          47.1             13.4 (41)    10.0-17.7      132.4       201.7
    /// order 3, full          91.8          50.7             14.1 (43)    10.6-18.4      298.1       516.7
    /// </code>
    /// <para>
    /// The 50 per cent crossing, <b>interpolated</b> between the -19 and -20 rungs and quoted as an
    /// interpolation: <b>-19.54 dB</b> for the port, <b>-19.81 dB</b> at order 2 full basis,
    /// <b>-19.93 dB</b> at order 3 over a window of 60, and <b>-20.02 dB</b> at order 3 over the
    /// whole basis. <b>So the window bought 0.12 dB over this default and the whole basis bought
    /// 0.21 dB, for 2.25 times the window's price.</b>
    /// </para>
    /// <para>
    /// <b>AND IT IS STILL ORDER 2 OVER THE FULL BASIS, because the intervals overlap at every
    /// rung</b> - 86.3-93.0 against 86.7-93.3, 35.5-46.4 against 41.5-52.7, and 7.8-14.8 against
    /// 10.0-17.7. <b>306 trials did not separate the two cells by the only interval this project
    /// computes, and a measurement that does not separate two options is a result.</b> Moving this
    /// on the point estimate would be tuning until a number passed.
    /// </para>
    /// <para>
    /// <b>The qualification, because it is why this is worth re-opening rather than closed.</b>
    /// <c>Ft8Step6Ladder.Wilson</c> is an independent-sample interval and the ladder is a paired
    /// design - one audio array per trial, handed to every column - so that interval overstates the
    /// uncertainty of a difference. McNemar on the discordant trials is the right test and
    /// <c>Ft8LadderHarness.Result</c> does not record the pairs. Unit 252 also measured that on all
    /// three rungs the increase in trials decoded exactly equalled the increase in codewords the
    /// port's gates accepted - +1/+4, +19/+11 and +8/+2 - which is a strong hint the discordant
    /// count is one-sided, <b>and a hint is not a test</b>. The whole table is
    /// <c>docs/unit252-osd-window.md</c> §7 and §8.
    /// </para>
    /// <para>
    /// <b>Nothing here was tuned to a target either.</b> No cell in that table reaches 40 per cent
    /// at -21 dB, and this default was not moved because a number was wanted.
    /// </para>
    /// </remarks>
    public static Ft8DeepOsdSettings Default { get; } = new(2);

    /// <summary>Builds settings for one order, over the whole basis or over a window of it.</summary>
    /// <param name="order">
    /// How many basis positions may be flipped, 0 to <see cref="MaximumOrder"/>.
    /// </param>
    /// <param name="window">
    /// <b>How many of the least reliable basis positions those flips may fall in</b>, 1 to
    /// <see cref="FullBasis"/>, and at least <paramref name="order"/>.
    /// <see cref="FullBasis"/> - the default - is the whole basis and is the behaviour that shipped
    /// before unit 252, unchanged in every re-encoding.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The order is negative or above <see cref="MaximumOrder"/>, or the window is outside 1 to
    /// <see cref="FullBasis"/>, or the window is smaller than the order.
    /// </exception>
    public Ft8DeepOsdSettings(int order, int window = FullBasis)
    {
        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(order),
                order,
                "An order is how many of the most reliable positions may be flipped and cannot be "
                + "negative. Zero is allowed and means re-encode the basis as it stands.");
        }

        if (order > MaximumOrder)
        {
            throw new ArgumentOutOfRangeException(
                nameof(order),
                order,
                $"Order {order} is beyond this library's bound of {MaximumOrder}. Order 4 is already "
                + "about two and a half million re-encodings per candidate; clamping instead of "
                + "refusing would report a measurement of an order nobody asked for.");
        }

        if (window < 1 || window > FullBasis)
        {
            throw new ArgumentOutOfRangeException(
                nameof(window),
                window,
                $"A window is 1 to {FullBasis} of the basis, counted from its least reliable end. "
                + "Clamping instead of refusing would report a measurement of a search nobody asked "
                + "for.");
        }

        if (window < order)
        {
            throw new ArgumentOutOfRangeException(
                nameof(window),
                window,
                $"A window of {window} positions cannot carry a search of order {order}: there are "
                + "not that many positions to flip. This is a caller mistake rather than a bad "
                + "signal, so it is refused rather than reduced to an order nobody asked for.");
        }

        Order = order;
        Window = window;
    }

    /// <summary>How many of the most reliable basis positions may be flipped.</summary>
    public int Order { get; }

    /// <summary>
    /// <b>How many of the least reliable basis positions the flips may fall in</b>, counted from the
    /// bottom of the basis. <see cref="FullBasis"/> means the whole 91 and is what ships.
    /// </summary>
    /// <remarks>
    /// <b>The window is over the basis, not over the codeword.</b> The basis is
    /// <c>Ft8DeepOrderedStatistics.MostReliableBasis</c>, whose 91 pivots come back in
    /// <c>|ratio|</c> order because the elimination visits columns in that order and appends pivots
    /// in visitation order - so the last <see cref="Window"/> of them are the least reliable of the
    /// basis. Positions the elimination stepped over are more reliable than any of them and are not
    /// in the basis at all. <c>docs/unit252-osd-window.md</c> §1 has the reading with line numbers.
    /// </remarks>
    public int Window { get; }
}
