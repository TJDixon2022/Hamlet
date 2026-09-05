using System;
using Ft8Sharp.Dsp;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>The pairing rule, and the budget that stops it putting a message nobody sent in front of the
/// operator.</b> Which candidate in an earlier slot is combined with which in a later one, how far
/// back the search looks, and — the part that matters — <b>how many combinations may be submitted to
/// the port's CRC-14.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS WHERE SOFT COMBINING FAILS QUIETLY, SO THE ARITHMETIC IS WRITTEN OUT HERE RATHER THAN
/// LEFT TO A REPORT.</b> Every codeword put to the port's checksum is an independent chance of a false
/// accept at about <b>one in 16 384</b>. A slot pair with 140 candidates on each side offers
/// <c>140 × 140 = 19 600</c> pairings; submitting all of them is about <b>1.2 expected wrong decodes
/// per trial</b>, which would put wrong messages in front of Tim inside one rung of the ladder — each
/// one carrying a valid checksum and looking exactly like a decode. <c>CLAUDE.md</c> §0.0 says a
/// decode nobody sent is worse than a decode missed, so the number of submissions is bounded here, by
/// construction, and counted at run time by <see cref="Ft8DeepCombineCounts"/>.
/// </para>
/// <para>
/// <b>The bound is <c>MaximumPartners</c> per candidate per remembered slot, and nothing else.</b> A
/// slot's own candidate list is the port's, capped by <c>Ft8SlotDecoder.CandidateLimit</c>, so the
/// worst case for one slot is <c>candidates × MaximumPartners × HistoryDepth</c> submissions and it is
/// a number that can be multiplied out before the run rather than a policy that has to be believed.
/// At the defaults, with the ladder's observed 13 candidates a slot at -21 dB, that is <b>13
/// submissions a slot pair</b> and about <b>4 000 across a 306-trial rung</b>, for a naive expectation
/// of <b>0.24 wrong decodes</b> — and the naive figure is an upper bound, because a submission only
/// reaches the CRC-14 at all if the port's parity gate converged on it first.
/// </para>
/// <para>
/// <b>The tolerances came out of unit 247 task 1, measured before this rule was designed.</b> Over one
/// whole 51-trial block at -21 dB, the closest candidate in each of two independent hearings of the
/// same transmission sat a median <b>0.00 Hz</b> and <b>0.000 s</b> apart, and within <b>3.125 Hz</b>
/// and <b>0.16 s</b> on <b>49 of 51</b> trials. The two that missed are the two trials unit 246
/// recorded as having no candidate within 60 of the transmitted codeword at all, which is a
/// synchronisation finding and not a pairing one.
/// </para>
/// <para>
/// <b>So the defaults are one tone and two symbol periods, which is wider than the measurement and
/// narrower than an accident.</b> A transmitter repeating a message does not move by a tone between
/// slots — that is the physical claim the frequency tolerance rests on — and two symbol periods covers
/// a station whose clock is off by more than the sub-block grid can express. Widening either buys
/// pairings and costs submissions in exact proportion, which is why the two numbers live beside the
/// budget rather than somewhere else.
/// </para>
/// </remarks>
public sealed class Ft8DeepCombineSettings
{
    /// <summary>
    /// The most slots back a repeat may be looked for. A bound rather than a tuning: memory and time
    /// both grow linearly in it, and a slot pair's cost is the same whichever slot it reaches back to.
    /// </summary>
    public const int MaximumHistoryDepth = 8;

    /// <summary>
    /// The most partners one candidate may be paired with in one remembered slot. <b>The budget.</b>
    /// </summary>
    public const int MaximumPartnersAllowed = 8;

    /// <summary>
    /// <b>The settings this library uses when nobody names any.</b> One slot of history, one tone of
    /// frequency tolerance, two symbol periods of time tolerance, one partner per candidate, and equal
    /// weight.
    /// </summary>
    /// <remarks>
    /// <b>Every number here is a measurement or a bound, and none of it was tuned to a target.</b>
    /// The tolerances are unit 247 task 1's; the partner count is the smallest number that can produce
    /// a combination at all and is what keeps the naive false-accept expectation at 0.24 over a whole
    /// 306-trial rung; the history depth is one because FT8's repeat is the next slot and a deeper
    /// history costs submissions in proportion to what it remembers.
    /// </remarks>
    public static Ft8DeepCombineSettings Default { get; } = new();

    /// <summary>Builds a pairing rule.</summary>
    /// <param name="historyDepth">
    /// How many previous slots to keep and look back through, 1 to <see cref="MaximumHistoryDepth"/>.
    /// </param>
    /// <param name="frequencyToleranceHz">
    /// How far apart in frequency two candidates may sit and still be called the same transmission.
    /// Defaults to <b>one FT8 tone, 6.25 Hz</b>, which is <c>1 / 0.160 s</c> — the geometry's own
    /// <see cref="Ft8WaterfallGeometry.ToneSpacingHz"/>, written as a literal here because a default
    /// parameter has to be a compile-time constant.
    /// </param>
    /// <param name="timeToleranceSeconds">
    /// How far apart in time-within-the-slot they may sit. Defaults to <b>two symbol periods,
    /// 0.32 s</b>.
    /// </param>
    /// <param name="maximumPartners">
    /// <b>The budget: how many combinations one candidate may put to the port's gates per remembered
    /// slot.</b> 1 to <see cref="MaximumPartnersAllowed"/>.
    /// </param>
    /// <param name="weighting">How much each hearing counts for.</param>
    /// <param name="accumulationDepth">
    /// <b>How many remembered slots may go into ONE sum</b>, 1 to <paramref name="historyDepth"/>.
    /// <b>One is the pairwise behaviour every figure recorded before unit 254 was measured at</b> and
    /// is the default. See <see cref="AccumulationDepth"/> for what a larger number does and, more to
    /// the point, what it does not do to the budget.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Any of the five numbers is outside its range. <b>Refused loudly rather than clamped
    /// silently</b>, because a caller who asked for eight partners and got one would be reading a
    /// measurement of something it did not ask for — and, the other way round, a caller who asked for
    /// a hundred and got them would be spending a false-accept budget it never counted.
    /// </exception>
    public Ft8DeepCombineSettings(
        int historyDepth = 1,
        double frequencyToleranceHz = 6.25,
        double timeToleranceSeconds = 0.32,
        int maximumPartners = 1,
        Ft8DeepCombineWeighting weighting = Ft8DeepCombineWeighting.Equal,
        int accumulationDepth = 1)
    {
        if (historyDepth < 1 || historyDepth > MaximumHistoryDepth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(historyDepth),
                historyDepth,
                $"A repeat is looked for in the previous 1 to {MaximumHistoryDepth} slots. Zero would "
                + "mean combining is on and there is nothing to combine with, which is a state a "
                + "caller cannot have meant.");
        }

        if (!(frequencyToleranceHz >= 0.0) || double.IsInfinity(frequencyToleranceHz))
        {
            throw new ArgumentOutOfRangeException(
                nameof(frequencyToleranceHz),
                frequencyToleranceHz,
                "The frequency tolerance is how far a repeating station's oscillator may have moved "
                + "between slots and must be a finite number of hertz at or above zero. An infinite "
                + "tolerance pairs every candidate with every candidate and spends the whole "
                + "false-accept budget in one slot.");
        }

        if (!(timeToleranceSeconds >= 0.0) || double.IsInfinity(timeToleranceSeconds))
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeToleranceSeconds),
                timeToleranceSeconds,
                "The time tolerance is how far a repeating station's clock may have moved between "
                + "slots and must be a finite number of seconds at or above zero.");
        }

        if (maximumPartners < 1 || maximumPartners > MaximumPartnersAllowed)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumPartners),
                maximumPartners,
                $"One candidate may be paired with 1 to {MaximumPartnersAllowed} partners per "
                + "remembered slot. THIS IS THE FALSE-ACCEPT BUDGET: every combination submitted is "
                + "an independent chance of the port's CRC-14 accepting a message nobody sent, at "
                + "about one in 16384, and an unbounded pairing puts about 1.2 of them in front of "
                + "the operator every slot pair.");
        }

        // REFUSED RATHER THAN CLAMPED, and this one is the refusal unit 254 exists to make. A caller
        // who asks for a four-hearing sum out of a one-slot history and is quietly handed a pair
        // would be reading a "combined x4" column that computed a chain of pairs, which is exactly
        // the breakage this unit found in the tree. The history is what supplies the hearings, so the
        // accumulation depth can never exceed it.
        if (accumulationDepth < 1 || accumulationDepth > historyDepth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(accumulationDepth),
                accumulationDepth,
                $"One sum may draw on 1 to {historyDepth} remembered slots, which is the history "
                + "depth this rule was built with, and the deepest combination therefore carries "
                + $"{historyDepth + 1} hearings at most. A depth of {accumulationDepth} asks for "
                + "hearings the history does not hold; raise historyDepth if that is what was meant. "
                + "Zero would mean combining is on and no partner may enter a sum, which is a state a "
                + "caller cannot have meant.");
        }

        HistoryDepth = historyDepth;
        FrequencyToleranceHz = frequencyToleranceHz;
        TimeToleranceSeconds = timeToleranceSeconds;
        MaximumPartners = maximumPartners;
        Weighting = weighting;
        AccumulationDepth = accumulationDepth;
    }

    /// <summary>How many previous slots are kept and looked back through.</summary>
    public int HistoryDepth { get; }

    /// <summary>How far apart in frequency two candidates may sit and still be paired.</summary>
    public double FrequencyToleranceHz { get; }

    /// <summary>How far apart in time-within-the-slot they may sit and still be paired.</summary>
    public double TimeToleranceSeconds { get; }

    /// <summary>
    /// <b>How many combinations one candidate may put to the port's gates per remembered slot.</b>
    /// The whole of the submission budget lives on this property.
    /// </summary>
    public int MaximumPartners { get; }

    /// <summary>How much each hearing counts for when they are added.</summary>
    public Ft8DeepCombineWeighting Weighting { get; }

    /// <summary>
    /// <b>How many remembered slots may go into ONE sum. The deepest combination carries
    /// <c>AccumulationDepth + 1</c> hearings.</b> One — the default — is a pair, which is what this
    /// library computed at every depth before unit 254.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>THIS DOES NOT MOVE THE FALSE-ACCEPT BUDGET, AND THAT IS THE WHOLE DESIGN.</b> The rule is
    /// still <em>one combination submitted per candidate, per partner rank, per remembered slot</em>
    /// — the bound <see cref="SubmissionsPerSlot"/> multiplies out, unchanged, at every accumulation
    /// depth. What changes is <em>what is in</em> the combination submitted for remembered slot
    /// <c>j</c>: at depth 1 it is this slot and slot <c>j</c>'s partner; at depth <c>A</c> it is this
    /// slot and the partners of the last <c>A</c> remembered slots ending at <c>j</c>. <b>More
    /// hearings per submission, never more submissions.</b>
    /// </para>
    /// <para>
    /// <b>Why depth is bought this way rather than by submitting every sum.</b> A rule that put the
    /// 2-way and the 3-way and the 4-way to the port's gates would spend three chances of a false
    /// accept where the pairwise rule spent one — <c>CLAUDE.md</c> §0.0 says a decode nobody sent is
    /// worse than a decode missed, so the deeper sum has to be bought out of the same budget or not
    /// at all. The sliding window buys it out of the same budget exactly.
    /// </para>
    /// <para>
    /// <b>What it costs instead.</b> At depth <c>A</c> the submission for remembered slot <c>j</c> is
    /// no longer the pair <c>(this slot, slot j)</c>; it is a sum that includes slot <c>j</c> along
    /// with up to <c>A - 1</c> slots more recent than it. If those extra hearings are of a different
    /// station the sum is worse than the pair would have been, and the port refuses it — which is a
    /// missed decode rather than a wrong one, and is measured rather than argued. See
    /// <c>docs/unit254-combining-depth.md</c> §4.
    /// </para>
    /// <para>
    /// <b>The arithmetic the depth is worth.</b> Summing <c>R</c> conditionally independent hearings
    /// of one codeword adds their log-likelihood ratios, whose mean grows as <c>R</c> and whose
    /// variance grows as <c>R</c>, so the per-bit signal-to-noise ratio grows as <c>R</c> —
    /// <c>10 log10 R</c> decibels, which is 3.01 dB at two hearings, 4.77 at three and 6.02 at four.
    /// <b>Textbook soft-decision theory and no source but arithmetic.</b> What is cited is the frame:
    /// 174 bits in codeword order carrying a 77-bit payload and a CRC-14, from S. Franke K9AN,
    /// B. Somerville G4WJS and J. Taylor K1JT, <em>The FT4 and FT8 Communication Protocols</em>, QEX,
    /// July/August 2020 — position <c>i</c> means the same codeword bit in every hearing because the
    /// protocol says so, and a repeated CQ is the same 174 bits again.
    /// </para>
    /// <para>
    /// <b>And nothing here decides that a message is real, at any depth.</b> A four-hearing sum goes
    /// to <c>Ft8CodewordDecoder.Decode</c> and faces the port's parity gate and CRC-14 gate exactly
    /// as a pair does. This is not a gate, not a threshold and not an acceptance rule.
    /// </para>
    /// </remarks>
    public int AccumulationDepth { get; }

    /// <summary>
    /// <b>The worst case, multiplied out, for a caller who wants the number before the run.</b>
    /// </summary>
    /// <param name="candidatesPerSlot">
    /// The most candidates one slot's search returns — <c>Ft8SlotDecoder.CandidateLimit</c> for the
    /// worst case, or the observed mean for the expected one.
    /// </param>
    /// <returns>The most combinations one slot can submit to the port's parity and CRC-14 gates.</returns>
    public int SubmissionsPerSlot(int candidatesPerSlot) =>
        candidatesPerSlot <= 0 ? 0 : candidatesPerSlot * MaximumPartners * HistoryDepth;

    /// <summary>
    /// <b>The expected number of messages nobody sent that a run of this size would accept, if every
    /// submission reached the checksum.</b>
    /// </summary>
    /// <param name="submissions">Combinations put to the port's gates across the whole run.</param>
    /// <remarks>
    /// <b>An upper bound, and it is quoted as one.</b> A combination only reaches the CRC-14 if the
    /// port's parity gate converged on it first, and most do not — unit 246 spent 11 451 submissions
    /// across a 918-trial ladder for zero wrong where this arithmetic predicts 0.70. <b>An upper bound
    /// is what a budget is set from.</b>
    /// </remarks>
    public static double ExpectedFalseAccepts(long submissions) => submissions / 16384.0;
}
