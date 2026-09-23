```
READ IN THIS ORDER.

A. PHASE GOAL - CW decodes again. Steps 1 and 2 done; step 3 partial on 3.6,
   six reds open; step 4 partial on 4.7 alone; step 5 is Tim's.
B. THIS STEP - step 4, the August rework judged on numbers. 4.1 to 4.6 met;
   4.7, the fourteen as chains, met by this unit: 2 chains judged, 14 of the
   fourteen with a verdict, both chains out, ticked in PHASE_PLAN.md.
C. THIS REPORT - the chain table leads section 3; section 4 raises 4 items
   and none stands in the way of 4.7.
```

```
UNIT:       404 - complete at task 4 of 4, tasks 0 to 4, none dropped - 2026-09-23 12:22
PHASE GOAL: get the engine's CW decoder back to what it produced on 2026-08-25, held there by three floor tests, then let Tim judge it at the radio
UNIT GOAL:  put the fourteen August rework pieces that could not go in alone back in with the pieces they stand on, one commit per chain, and keep or remove each chain on the floors and the named numbers
ADVANCED:   yes - step 4 criterion 7, every one of the fourteen has a verdict and each chain has its row
NUMBER:     chains judged 2, kept 0, out 2, of the fourteen 14 with a verdict
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 4. All tasks, 0 to 4, were done and none was dropped.** Machine
QUIVERFULL, project Hamlet, branch main. The gate held: SHACK_FACTS.md and
CwProbabilisticDecoder.cs are present, there is no CoreHMI.sln or MURC.sln, and the root is
C:\Source\HamLet. Section 5 matched the tree with no mismatch: HEAD was bc2484d5, 7e65aac4 was the
newest src commit, all fourteen hashes are in unit395-rework.md, and the printer exists. The
floors at entry were 37, 13 and 2. Every commit was pushed and every push succeeded: d8f07a93,
95094c02, 3c742c4a, 675d846f, f9f22961, bfd97867, 264b97ce, d062d5e9, ad2b80da, then the task 4
commit.

**Task 0, record and entry round.**
- The version went from 1.13.90 to 1.13.91. PHASE_STATUS.md now reads 404 and CURRENT_STEP 4.
- Engine line: 178 of 178 in 372 s.
- App line: 277 of 278. One name was lost to the dispatcher loop before any assertion. The re-run
  gave 278 of 278.
- Captures: 37 of 37 in 92 s. Every row is identical to unit 395's section 3.1.
- Adjudicated: 13 of 13. Clean synthetics: 2 of 2.
- Printer: WEEKEND 5, THINKING 5, FLEX 1, ABOVE 2, BREEZE 2.

**Task 1, the trace.** No code was written. The pieces are the 45 Cw-only diffs. They were checked
on a scratch repo that holds HEAD's Cw folder. For each of the fourteen, the search:
1. tried the piece alone;
2. tried it after its named dependencies;
3. tried it after the whole prefix;
4. then removed links newest first, keeping a removal only if nothing refused more.

That produced an irreducible chain for each of the fourteen (docs\phase-cw\unit404-chains.md
section 2). The grouping rules gave **two chains, not unit 395's three**:
- S has 4 pieces.
- D has 24 pieces, because every decoder-side chain runs through 3e84ac74 and f27174b5.

**A decision the session made: two build links.** 0f2089f3 calls CwPitchRanking, which b48d1158
creates. efcd5242 calls CwSpectralPeak, which ade52536 creates. The apply check cannot see a
type. Without these two pieces, D could not compile by construction, and its verdict would have
said nothing about the work. So both were added to D. Section 4 item 2 covers this.

**Task 2, the chains, smallest first.**
- **S** applied with no conflict and needed no adaptation. It built clean. Nothing moved, so it is
  out.
- **D** applied under --3way with no conflict. Three pieces had hunks not taken:
  - the CwPitchChoice.cs hunks of 4c6e4321 and 0f2089f3, already in the tree (decision 3);
  - the meter hunk of 9c2a7f99, which the build did not need (parked as 397 item 2).

  **Adaptations, all in src\Hamlet.RadioEngine\Cw, all under decision 4.** In each case the
  chain's member was kept and unit 392's seam copy removed. There were five build errors:
  - CwCharacter.cs: WidestRecordedLlr and MarginLlr, CS0102;
  - CwDecoder.cs: Retuned, CS0111;
  - CwDecodeReport.cs: the PitchWasAsserted and PitchChoice properties, CS8907.

  After that it built clean with warnings as errors. It was then measured and is out. The numbers
  are in section 3.

**Task 3.** No chain was kept, so this task measured HEAD again. All six reds read exactly as unit
402 left them. This was not a 3.6 attempt.

**Task 4, exit round.**
- Engine line: 178 of 178.
- App line: 277 of 278. The one name lost was a different one, again to the dispatcher loop before
  any assertion. The re-run gave 278 of 278.
- Floors: 37 of 37 with every row identical to entry, 13 of 13, 2 of 2.
- The transmit files show no diff against 7e209cb4.
- git diff bc2484d5 HEAD over src shows no diff, and over src\Hamlet.App it also shows none.
- 4.7 is ticked. **No regression:** nothing green at entry is red at exit.

## 2. What the owner should expect

Tim, putting the August work back in whole did not earn any of it a place. The survey pieces
changed nothing the floors or the printer measure. The decoder group moved one number, ABOVE on
013637 from 2 to 1, but it cost a lot:
- 021410 fell from 47 characters to 40;
- three captures and both clean synthetics went below their floors;
- decoding ran so much slower that the captures test got through 5 of its 37 cases in the time
  the whole set took before.

Both groups are back out. The tree's src is byte-identical to this morning's, and the decoder on
the air is the one you had at the start of the unit. **What will look wrong but is not:**
- The git log shows a 24-piece decoder commit landing and being reverted.
- The working tree still carries loop files this unit did not write and did not commit:
  .run-unit state files, SESSION.lock and a report copy.

## 3. What you should see

**The chain table.** Distances are on the settled text.

| Chain | Pieces | Number before | Number after | Kept or out |
|---|---|---|---|---|
| S, the survey, carrying 4786c7e7 and f2e1db7a | 7fb89d5e, 44cf3fc8, 4786c7e7, f2e1db7a | 021410 47 ch, WEEKEND 5, THINKING 5, FLEX 1; 013637 63 ch, ABOVE 2, BREEZE 2; captures 37 of 37, adjudicated 13 of 13, synthetics 2 of 2 | every one of the 37 captures identical in characters, elements, unsure and tone; both texts and the five distances identical; 13 of 13; 2 of 2 | **out**, 3c742c4a reverted in 675d846f |
| D, the decoder, carrying 386fdb5d, 4c6e4321, 0f2089f3, 62262b94, fc1ee77f, 68a18d66, a91d8fe7, efcd5242, aeea24f2, a37cfcff, ee2cba8d, 9c2a7f99 | 2068f868, 6fc36a1e, 3e84ac74, 9de394da, f27174b5, 386fdb5d, 4c6e4321, b48d1158, 0f2089f3, 62262b94, fc1ee77f, 71b4f044, 68a18d66, a91d8fe7, ade52536, efcd5242, 95a5e063, b7147b1f, 4935a4f8, e6b1ece7, aeea24f2, a37cfcff, ee2cba8d, 9c2a7f99 | the same | see the notes below the table | **out**, bfd97867 reverted in 264b97ce |

**Chain D's numbers after**, against the entry numbers above:
- **Captures: killed at the 300 s timeout with 5 of 37 cases done.** At entry all 37 took 92 s.
  - 013520: 60 to 62 characters.
  - 001520: elements 45 to 39, **red**.
  - 013637: 63 to 62 characters, **red**.
  - 031948: 34 to 31 characters. It is anchored, so the count is printed and not asserted.
  - 012922: 50 to 44 characters, **red**.
  - 32 cases were not reached.
- **Adjudicated: killed at 180 s.** 4 of 13 green, none red, 9 not reached.
- **Synthetics: 0 of 2.** The expected text is `CQ DE W1AW K`; they gave `■■ ■■ W1AW K` and
  `■Q DE W1AW K`.
- **Printer.**
  - 021410: **47 to 40 characters**; WEEKEND 5, THINKING 5, FLEX 1.
  - 013637: 62 characters, 13 unsure; **ABOVE 2 to 1** (nearest `AB OVE`); BREEZE 2.

The per-chain detail and the build errors quoted in full are in docs\phase-cw\unit395-rework.md
section 2, under *Chains under 4.7, judged by unit 404*. The trace is in
docs\phase-cw\unit404-chains.md.

**Floors raised: none.** No chain was kept.

**Task 3: the six reds at HEAD, beside unit 402.**

| Red | Test | Unit 402 before | Unit 404, HEAD |
|---|---|---|---|
| #6 | CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp, 25 wpm | 0.75 of the message, bar 0.79 | 0.75 |
| #15 | CwAcquisitionWindowTests.TheSlowEndReadsTheMessage, 12 wpm 18 dB | 0.54 | 0.54 |
| #42 | CwReceiverFixtureTests.NothingIsEmittedDuringTheOperatorsOwnTransmission | 70 | 70 |
| #43 | TheEasyTierIsReadWhole coverage-easy, unreadable + not in message | 5 + 37 | 5 + 37 |
| #44 | TheEasyTierIsReadWhole exchange-easy | 3 + 21 | 3 + 21 |
| #45 | TheEasyTierIsReadWhole tightfist-easy | 1 + 3 | 1 + 3 |

Totals by type:
- CwAcquisitionWindowTests: 10 of 12.
- CwReceiverFixtureTests: 23 of 27.
- CwAdjudicationTests: 11 of 11.
- CwFixtureTests: 22 of 23. The red case is fading-18wpm's confident-mistakes case, the one
  already parked as 400 item 4.

## 4. What's blocking us

Nothing blocks 4.7 or any other criterion. Four items are recorded for the arbiter.

1. **The merge rule judged twelve of the fourteen as one 24-piece verdict.**
   - **Ruling asked: none needed for 4.7. It is recorded so the reader knows what the verdict
     covers.**
   - **Reasoning:** the instruction's rule merges chains that share any piece. 3e84ac74 and
     f27174b5 sit in every decoder-side chain, so the lattice, the estimator and the hooks became
     one chain.
   - That chain carries two links that were already out on their own for measured harm:
     - e6b1ece7, which alone turned 14 captures red in unit 397. Only a37cfcff and ee2cba8d stand
       on it.
     - 95a5e063, which alone raised unsure on 25 captures in unit 396. Only aeea24f2 and
       9c2a7f99 stand on it.
   - So D's result cannot say whether the lattice chain, fc1ee77f and 68a18d66 on 2, 3, 5, 12 and
     13, would have held the floors on its own. Inside D it moved ABOVE by one.
   - **Rejected:** splitting D myself. The rule is the author's, and the instruction marks it as
     overrulable by the author, not by the session.
2. **Two build links added by the session.**
   - **Ruling asked: none. Recorded as a decision made for itself.** b48d1158 and ade52536 went
     into D even though they are not in any minimal apply-check chain.
   - **Reasoning:** 0f2089f3 and efcd5242 call the types those two pieces create. Without them
     the chain fails to compile by construction, and its verdict would say nothing about the
     work.
   - **Rejected:** the literal chain. It turns a guaranteed compile error into an out that tells
     nobody anything.
3. **Chain D made the decode several times slower,** and the fixed per-type timeouts cut its
   measurement short. The captures test ran 5 cases in 300 s, and the printer took 61 s against
   5 s.
   - **Ruling asked: none.** The verdict does not depend on the cases not reached, because three
     reds on assertion already put the chain out.
   - The slowdown was measured and not traced. A future unit that reapplies any part of D should
     expect the engine line's 480 s to be at risk.
4. **The dispatcher loop lost one app name at entry and one at exit,** a different name each
   time, both before any assertion. Each re-run was 278 of 278. **Ruling asked: none.**
