using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>How the ordered-statistics re-encoding count varies with the number of candidates, measured
/// on slots that carry few and slots that carry many.</b> Work instruction 250, task 6.
/// </summary>
/// <remarks>
/// <para>
/// <b>WHY THIS EXISTS, AND THE BREAKAGE IT WOULD HAVE CAUGHT.</b> This phase's standing rule is
/// that no test is added without naming one, so: <b>unit 249 measured 192,602 re-encodings on one
/// slot of clean synthetic audio and reported that nothing bounds that number.</b> Two tests in this
/// suite each already pin one half of the arithmetic - that an order costs a fixed number of subsets
/// of the 91-position basis, and that on one particular slot the stage is offered exactly the
/// candidates belief propagation refused - and <b>nothing anywhere multiplied them together across a
/// range of candidate counts.</b> A stage whose cost is superlinear in how busy the band is would
/// have looked exactly the same on unit 249's single clean slot, and the first place it showed would
/// have been a crowded evening on 14.074 with the next slot's boundary arriving mid-decode.
/// </para>
/// <para>
/// <b>NOTHING HERE CAPS ANYTHING</b>, and work instruction 250 forbids it in terms. A cap would make
/// the decoder's reach depend on how busy the band is, silently, which is the <c>CLAUDE.md</c> §0.0
/// fault in a new place. This test measures and reports; it changes no arithmetic.
/// </para>
/// <para>
/// <b>THE COUNT IS BOUNDED, and unit 249's report is corrected rather than confirmed.</b> The bound
/// is not in <c>Ft8Sharp.Deep</c> at all - it is <see cref="Ft8SyncSearch.DefaultCandidateLimit"/>,
/// upstream's <c>kMax_candidates</c>, which is 140. The stage is offered at most one candidate per
/// candidate the search returned, and it spends exactly the order's subset count on each, so the
/// worst slot this decoder can ever construct is <c>140 x 4187</c> at the shipping order of 2.
/// That is a ceiling from two constants and it does not depend on the audio.
/// </para>
/// <para>
/// <b>ADOPTED AND EXTENDED BY UNIT 252, WHICH IS THE STEP UNIT 250 SAID THIS TEST WAS WAITING
/// FOR.</b> This file was left untracked by unit 250 and had never been run; unit 252 ran it
/// unmodified first, and it passed on adoption exactly as written. Its closing lines said <em>what
/// would actually move this is the order, not the band</em>, and that is now a second dimension in
/// the sweep: the same busy slot decoded at a grid of <c>(order, window)</c> cells, with the
/// re-encoding count reported and asserted per cell as well as per candidate count. <b>The breakage
/// that half would have caught is unit 252's own:</b> a window that is stored and reported but not
/// honoured by the enumeration spends the full-basis count while every table in the report says
/// otherwise, and this is the test that reads the count off a real slot rather than off a
/// synthesised set of ratios.
/// </para>
/// </remarks>
public class Ft8DeepOsdCostTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>
    /// The re-encodings one candidate costs at order 2: one for the hard decision, plus every subset
    /// of the 91 basis positions of size 1 and of size 2. Pinned independently by
    /// <c>Ft8DeepOrderedStatisticsTests.TheCostOfAnOrderIsTheNumberOfSubsetsOfTheBasis</c>.
    /// </summary>
    private const long PerCandidateAtOrderTwo = 1L + 91L + (91L * 90L / 2L);

    /// <summary>A clean transmission of one free-text message, laid into a whole slot.</summary>
    private static float[] CleanSlot(string text)
    {
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(text, message));

        var symbols = Ft8SymbolEncoder.Encode(message);
        return Ft8Waveform.SynthesizeSlot(symbols, Rate);
    }

    /// <summary>
    /// One clean transmission plus uniform noise of a stated amplitude. <b>The noise is what varies
    /// the candidate count</b> - the sync search finds more places worth trying as the floor rises,
    /// and every one of them fails parity, which is precisely the population this stage is handed.
    /// </summary>
    private static float[] Slot(string text, double amplitude, int seed)
    {
        var clean = CleanSlot(text);
        var random = new Random(seed);
        var mixed = new float[clean.Length];

        for (var i = 0; i < clean.Length; i++)
        {
            mixed[i] = clean[i] + (float)((random.NextDouble() - 0.5) * amplitude);
        }

        return mixed;
    }

    /// <summary>
    /// <b>Re-encodings against candidates, over slots from nearly empty to nearly full.</b>
    /// </summary>
    [Fact]
    public void TheReencodingCountIsExactlyLinearInTheCandidatesTheStageIsOffered()
    {
        // From clean audio, where the port takes the signal and offers the stage almost nothing, up
        // to a floor high enough that the search fills its list with places nothing was sent.
        var amplitudes = new[] { 0.0, 1.0, 2.0, 4.0, 6.0, 8.0, 12.0 };

        output.WriteLine("ORDERED STATISTICS: WHAT IT SPENDS, AGAINST HOW BUSY THE SLOT IS.");
        output.WriteLine(string.Empty);
        output.WriteLine("  order          : 2, the shipping order");
        output.WriteLine($"  basis          : 91 positions, so one candidate costs "
            + $"{PerCandidateAtOrderTwo} re-encodings");
        output.WriteLine($"  candidate cap  : {Ft8SyncSearch.DefaultCandidateLimit}, "
            + "Ft8SyncSearch.DefaultCandidateLimit, upstream's kMax_candidates");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"{"noise",6} {"cand",5} {"parity",7} {"offered",8} {"re-encodings",13} {"ms",8}  {"ms/offered",10}");

        var worstOffered = 0;
        var worstReencodings = 0L;
        var worstMs = 0.0;

        foreach (var amplitude in amplitudes)
        {
            var samples = Slot("HAMLET 250", amplitude, seed: 250_006);

            var decoder = new Ft8DeepSlotDecoder(osd: new Ft8DeepOsdSettings(2));

            var started = Stopwatch.GetTimestamp();
            var result = decoder.Decode(samples);
            var ms = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;

            var counts = decoder.LastOsd;
            var perOffered = counts.Offered == 0 ? 0.0 : ms / counts.Offered;

            output.WriteLine(
                $"{amplitude,6:F1} {result.CandidateCount,5} {result.ParitySatisfiedCount,7} "
                + $"{counts.Offered,8} {counts.Reencodings,13:N0} {ms,8:F0}  {perOffered,10:F2}");

            // **THE SHAPE, ASSERTED ROW BY ROW.** Linear, with a constant the order fixes and the
            // audio cannot move. This is the whole question task 6 asked.
            Assert.Equal(counts.Offered * PerCandidateAtOrderTwo, counts.Reencodings);

            // **THE BOUND.** It is the port's candidate limit and nothing in the sibling.
            Assert.True(
                counts.Offered <= Ft8SyncSearch.DefaultCandidateLimit,
                $"the stage was offered {counts.Offered} candidates against a search limit of "
                    + $"{Ft8SyncSearch.DefaultCandidateLimit}, so the ceiling below is not a ceiling");

            if (counts.Offered > worstOffered)
            {
                worstOffered = counts.Offered;
                worstReencodings = counts.Reencodings;
                worstMs = ms;
            }
        }

        var ceiling = Ft8SyncSearch.DefaultCandidateLimit * PerCandidateAtOrderTwo;

        output.WriteLine(string.Empty);
        output.WriteLine("THE WORST SLOT SEEN, AND THE ONE THAT CANNOT BE EXCEEDED:");
        output.WriteLine(string.Empty);
        output.WriteLine($"  worst seen     : {worstOffered} offered, {worstReencodings:N0} "
            + $"re-encodings, {worstMs:F0} ms");
        output.WriteLine($"  arithmetic max : {Ft8SyncSearch.DefaultCandidateLimit} offered, "
            + $"{ceiling:N0} re-encodings");
        output.WriteLine($"  FT8 budget     : 15,000 ms a slot");

        if (worstOffered > 0)
        {
            var atTheCeiling = worstMs / worstOffered * Ft8SyncSearch.DefaultCandidateLimit;
            output.WriteLine($"  a full list would cost about {atTheCeiling:F0} ms on this machine, "
                + $"{atTheCeiling / 15_000.0:P2} of the budget");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("UNIT 249 REPORTED 192,602 RE-ENCODINGS AND SAID NOTHING BOUNDED IT.");
        output.WriteLine("192,602 is 46 x 4,187 exactly. The count was never unbounded - it is one");
        output.WriteLine("candidate's fixed cost times a candidate list the PORT caps at 140. What");
        output.WriteLine("varies with a crowded band is the candidate count, linearly, and 140 is");
        output.WriteLine("the end of it.");
        output.WriteLine(string.Empty);
        output.WriteLine("WHAT WOULD ACTUALLY MOVE THIS IS THE ORDER, NOT THE BAND. Order 3 costs");
        output.WriteLine("125,672 a candidate against order 2's 4,187 - thirty times - and step 3 of");
        output.WriteLine("this phase is the step that would raise it. THIS TEST IS WHERE THAT SHOWS.");

        // The sweep has to have actually produced a busy slot, or it measured one point twice.
        Assert.True(
            worstOffered > 0,
            "no candidate was refused on any row, so this sweep measured nothing about a busy slot");

        // =============================================================================
        // UNIT 252'S SECOND DIMENSION: THE SAME SLOT, AT A GRID OF (ORDER, WINDOW) CELLS.
        // =============================================================================
        var gridSlot = Slot("HAMLET 250", amplitude: 4.0, seed: 250_006);

        output.WriteLine(string.Empty);
        output.WriteLine("AND HERE IT IS. ONE SLOT, THE SAME SLOT, AT A GRID OF ORDERS AND WINDOWS.");
        output.WriteLine(string.Empty);
        output.WriteLine("The window is how many of the LEAST RELIABLE basis positions the order's");
        output.WriteLine("flips may fall in. A cell costs 1 + sum over i of C(window, i) re-encodings");
        output.WriteLine("a candidate, so the window buys an order back at a fraction of its price.");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"{"order",5} {"window",7} {"offered",8} {"per cand",10} {"re-encodings",14} {"ms",8}"
            + $"  {"vs order 2 full",15}");

        (int Order, int Window)[] cells =
        [
            (2, Ft8DeepOsdSettings.FullBasis),
            (2, 40),
            (2, 20),
            (3, Ft8DeepOsdSettings.FullBasis),
            (3, 60),
            (3, 45),
            (3, 40),
            (3, 30),
            (3, 20),
            (4, 40),
            (4, 30),
            (4, 20),
        ];

        var shipping = SubsetCount(2, Ft8DeepOsdSettings.FullBasis);

        foreach (var (order, window) in cells)
        {
            var decoder = new Ft8DeepSlotDecoder(osd: new Ft8DeepOsdSettings(order, window));

            var started = Stopwatch.GetTimestamp();
            decoder.Decode(gridSlot);
            var ms = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;

            var counts = decoder.LastOsd;
            var perCandidate = SubsetCount(order, window);

            output.WriteLine(
                $"{order,5} {window,7} {counts.Offered,8} {perCandidate,10:N0} "
                + $"{counts.Reencodings,14:N0} {ms,8:F0}  {(double)perCandidate / shipping,14:F2}x");

            // **THE WINDOW, ASSERTED CELL BY CELL AND ON A REAL SLOT.** The arithmetic is pinned
            // independently, against written-out constants rather than against this same
            // expression, by
            // Ft8DeepOrderedStatisticsTests.TheCostOfAnOrderInAWindowIsTheNumberOfSubsetsOfTheWindow.
            Assert.Equal(counts.Offered * perCandidate, counts.Reencodings);

            // **AND THE ONE-CODEWORD RULE, WHICH THE WINDOW MAY NOT MOVE.** Exactly one codeword
            // per candidate offered reaches the port's CRC-14, whatever the order and whatever the
            // window - docs/unit252-osd-window.md section 4.
            Assert.Equal(counts.Offered, counts.Produced);
        }

        output.WriteLine(string.Empty);
        output.WriteLine("So order 3 is not thirty times the shipping price after all - it is thirty");
        output.WriteLine("times over the WHOLE basis. Over the unreliable end of it, where unit 246");
        output.WriteLine("measured the errors to be, it is a small multiple. What that buys in");
        output.WriteLine("decodes is not a question this test can answer: it is the ladder's, and");
        output.WriteLine("unit 252 task 3 takes the grid there.");
    }

    /// <summary>
    /// <c>1 + sum over i = 1..order of C(window, i)</c>: what one candidate costs at a cell.
    /// </summary>
    private static long SubsetCount(int order, int window)
    {
        var total = 1L;
        var term = 1L;

        for (var i = 1; i <= order; i++)
        {
            term = term * (window - i + 1) / i;
            total += term;
        }

        return total;
    }
}
