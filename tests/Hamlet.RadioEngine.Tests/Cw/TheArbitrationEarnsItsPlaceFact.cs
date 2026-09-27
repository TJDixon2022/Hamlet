namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The arbitrated transcript, ours alone and the port alone, scored side by side
/// on every metric of `CW_REQUIREMENTS.md` sections B and I, per condition, on
/// the harness and on the live path (work instruction 466, task 1; HM-REQ-128;
/// PHASE_PLAN.md 9.7).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT ANY OF THE THREE.** Serves
/// HM-REQ-128. Nothing under `src` changes for it.</para>
/// <para>**FIXED HERE AT TASK 1, BEFORE ANY THREE-WAY FIGURE WAS COMPUTED, AND
/// NEVER CHANGED AFTER** (V-14; 466 DECIDED (2) to (5)). The three outputs are
/// scored by one scorer, against one set of keys, by the same calls
/// (V-11, V-13): <see cref="TheRequirementsAreMeasuredTests.Measure"/> and the
/// <see cref="CwMetrics"/> calls, each figure with its key's kind.</para>
///
/// <para>**1. THE METRIC LIST** (466 DECIDED (2), requirement by requirement; a
/// rate is compared as a rate, count over count, and a rate whose denominator is
/// zero for an output is not defined for it):</para>
/// <list type="bullet">
/// <item>**HM-REQ-010, MET-CER-SURE**: sure characters wrong or added over sure
/// characters emitted (`CwMetrics.SureErrors`). Lower is better. Defined for all
/// three; not defined for an output that emits nothing sure on the row.</item>
/// <item>**HM-REQ-011, MET-INVENTED**: sure added plus sure wrong over characters
/// sent (`CwMetrics.Invented`). Lower is better. Defined for all three.</item>
/// <item>**HM-REQ-012, MET-COVERAGE**: sure and right over characters sent (R82,
/// `CwMetrics.Coverage`). Higher is better. Defined for all three.</item>
/// <item>**HM-REQ-013**: sent characters not emitted sure and correct, over
/// characters sent (sent less sure and right). Lower is better. Compared only on
/// the rows the requirement names, the 15 dB rows; no row in the tree is shown
/// to be CH-AWGN, so the synthetic 15 dB rows stand for it and say so. Its
/// per-row verdict is met where the count is zero and MET-CER-SURE is zero, as
/// its rationale reads it. Defined for all three.</item>
/// <item>**HM-REQ-014, dim accuracy**: characters emitted dim (class not sure)
/// and right, over characters emitted dim, from the same alignment's steps.
/// Higher is better; the verdict is met at 0.70 or more. **Not defined for the
/// port**: parity.md section 1 maps every non-space character it prints to sure
/// and its no-match output to a placeholder, so it emits no dim; and not defined
/// for any output that emits no dim on the row. Compared between the
/// arbitrated output and ours only.</item>
/// <item>**HM-REQ-080 and 081, MET-WBE**: word boundaries inserted plus deleted
/// over words sent (`CwMetrics.WordBoundaries`), one metric; 080's verdict (zero)
/// on the 15 dB rows, 081's (at most 0.05) on every row. Lower is better.
/// Defined for all three.</item>
/// <item>**HM-REQ-083, live against settled boundaries**: on the live path only,
/// over every pair of consecutive characters the leading edge ever showed that
/// also settle as consecutive characters (each matched by its end on the audio
/// clock, <c>CwCharacter.At</c>), the share of distinct pairs where some live
/// rendering's word boundary between them differs from the settled one. Lower is
/// better. **Not defined for the port**: it prints once and has one rendering.
/// **Not defined on the harness**, which has no live rendering. Compared between
/// the arbitrated output and ours only, on the live path.</item>
/// <item>**HM-REQ-084, the named spans**: of the six spans DEV_ANALYSIS_2026-08-27
/// section 4 names, those whose audio is in the tree
/// (<see cref="WhatTheNamedWordsReadTests.Spans"/>: `ABOVE`, `BREEZE`, `FLEX`;
/// `WEEKEND`, `THINKING` and `USED TO USE A FIRM` are not measurable here, as
/// <see cref="WhatTheNamedWordsReadTests.NotHere"/> records), each read exactly
/// by that printer's rule: the characters whose spans overlap the span spell the
/// word, with a boundary on each side and none inside, every letter sure. Spans
/// read exactly over spans measurable. Higher is better. Defined for all three,
/// each character placed by its own span. Its recordings name no sender in
/// `CW_SPEC.md` section 10, so it is compared on the rows real HF, all and real
/// HF, sender not stated.</item>
/// <item>**HM-REQ-015 and 082 are properties, stated once for all three.** 015:
/// each character carries its class when it is emitted - ours on both seams
/// (`EveryCharacterCarriesAConfidenceTests`), the port's from the harvester's
/// reading at the moment it prints, and the arbitrated character from
/// <c>CwArbiter.Decide</c> at the moment it is decided; checked by those tests and
/// by inspection. 082: word-boundary errors are counted by
/// <c>CwMetrics.WordBoundaries</c> apart from character errors, for all three,
/// through the one scorer.</item>
/// </list>
///
/// <para>**2. THE THREE RULES.**</para>
/// <list type="bullet">
/// <item>**The loss rule** (466 DECIDED (3)): on a row, the arbitration loses
/// where it is strictly worse than ours alone, or than the port alone, on any
/// metric compared on that row and defined for both of the pair.</item>
/// <item>**The better-decoder rule** (466 DECIDED (4)): the better decoder alone
/// on a row is the one no worse than the other on every metric compared for both;
/// where neither is, the first metric where they differ in the order 011, 010,
/// 013, 012, 014, 081, 083, 084 decides, honesty first as CLAUDE.md 0.0 and
/// HM-REQ-011's "prime directive as a number" rank them; ours on a full tie.</item>
/// <item>**The path rule** (466 DECIDED (5)): a row's switch is set from the
/// path that emits under it. The live product runs only under real HF, all
/// (<c>CwVoteTable.LiveCondition</c>, unit 465), so that row is set from the live
/// path and printed from the harness beside it; every other row from the
/// harness, where each recording runs under its own row.</item>
/// <item>**The switch that follows**: no loss, `arbitrate`; a loss, the better
/// decoder alone - `ours alone` or `port alone`. Where the rule names the port,
/// the port alone is emitted at parity.md section 1's mapping though HM-REQ-124
/// withholds its vote (466 DECIDED (6)).</item>
/// </list>
///
/// <para>**3. WHERE THE SWITCH GOES**, with no line under `Cw/Second/`
/// changing:</para>
/// <list type="bullet">
/// <item>A table beside <c>CwVoteTable</c>, <c>CwSwitchTable</c>
/// (`src/Hamlet.RadioEngine/Cw/CwSwitchTable.cs`): per condition row `arbitrate`,
/// `ours alone` or `port alone`, with <c>For(condition)</c>, <c>Live</c> at
/// <c>CwVoteTable.LiveCondition</c>, and the rule above as <c>Choose</c>.</item>
/// <item>**The harness**: <c>CwArbiter.Arbitrate</c>, <c>Decide</c> and
/// <c>DecideAlone</c> take the switch in force. `arbitrate` is today's code
/// unchanged; `ours alone` emits ours' characters as ours prints them alone;
/// `port alone` emits the port's readings in order, each a
/// <c>CwCharacter</c> at parity.md section 1's mapping as <c>DecideAlone</c>
/// builds it, with a word boundary where the port printed a space. Every span is
/// still paired and recorded, both readings on it, the switch in force on
/// <c>CwArbitration</c> (HM-REQ-121).</item>
/// <item>**The port's spaces**: <c>CwSecondHarvester.Take</c> reads them today
/// and drops them; it marks the next reading <c>WordGapBefore</c>. Nothing the
/// arbiter reads under `arbitrate` changes.</item>
/// <item>**The live path**: <c>CwDecoder.Switch</c>, <c>CwSwitchTable.Live</c> by
/// default, consulted in <c>CwDecoder.Arbitrated</c>, <c>ArbitratedEdge</c> and
/// <c>Flush</c>, where <c>CwSecondReader</c>'s readings meet ours; under `port
/// alone` ours' settled characters and word boundaries are not emitted and the
/// port's characters and spaces go out through the same <c>Settle</c> and
/// <c>CharacterSettled</c> seam the CW tab reads.</item>
/// <item>**The sheet**: <c>MainWindowViewModel.ArbitrationLine</c> writes the
/// switch beside a character only where it is not `arbitrate`. The CW tab never
/// shows a decoder or a switch (HM-REQ-121).</item>
/// </list>
/// </remarks>
public sealed class TheArbitrationEarnsItsPlaceFact
{
}
