READ IN THIS ORDER.

A. The field table at 7.030 QRP, the CW DX frequency (14.010), and 14.050 CW, before and after; the count of CW-family blocks stating conditions, before and after: 9 of 20 -> 20 of 20. Section 3 opens with both.
B. Step 7's criterion 7.7 clause by clause - the three blocks set alike, watched failing first at 7.030, conditions unchanged, unconfirmed not written - then 7.5 re-read against the new table, and 7.6 the exit round. All at the top of section 1.
C. The rest. Section 4 raises 2 items, none blocking, and carries P14, P15 and P16 as still parked. P17 is the owner's and is not in the way of any criterion of step 7.

```
UNIT:       420 - complete at task 3 of 3, none dropped - 2026-09-24 12:57
PHASE GOAL: Hamlet decodes a real CQ call into the text that was sent, with the receiver in front of the decoder set the way the mode needs it
UNIT GOAL:  Tuning into Morse in a CW DX or QRP block sets the receiver exactly as tuning into a plain CW block does
ADVANCED:   yes - 7.7 met and ticked, watched failing first at 7.030 and green after; 7.5's last hole, P13, closed with it, so 7.5 is ticked too
NUMBER:     CW-family blocks that state receiver conditions: 9 -> 20 of 20
DRIFT:      0
```

## 1. What Claude did

**Complete, task 3 of 3, none dropped.** This ran on QUIVERFULL. The gate confirmed Hamlet by its four filesystem checks. Branch `main`, entry HEAD `b378abeb`. **Nothing in this report is evidence about the radio.** No IC-7300 is attached to this machine (FACT-006). Every result comes from `ScriptedRadio`.

**Criterion 7.7, clause by clause:**

- **The three blocks set alike.** `EveryMorseBlockSetsWhatCwSetsTests.EnteringMorseHereSetsWhatTheCwBlockSets` drives the setup at 7.030 (`QRP`), 14.010 (`CW DX`) and 14.050 (`CW`). Each run hands the setup exactly what the app hands it, the block's own conditions. Beside it, the `CW` block of the same band runs at the same dial. For every confirmed CW condition, the outcome, the value after and the bytes written must match. Green 3 of 3.
- **Watched failing first at 7.030.** Committed red at `54927894`, with 1 of 5 passing. The failures:
  - `At7030ThePreampEndsOff`: *Assert.Equal() Failure: Values differ, Expected: 0, Actual: 1*.
  - `EnteringMorseHereSetsWhatTheCwBlockSets(hz: 7030000)`: *QRP at 7.030 MHz states no auto notch, so entering Morse there leaves it wherever it was*.
  - The same at 14.010 for `CW DX`.
  - `EveryMorseBlockStatesTheCwRow`: *Assert.Empty() Failure*, 9 of 20.
  - Only 14.050 `CW` passed.

  All 6 of the type's tests are green at `bba76811`.
- **The field table in each of the three blocks, and the count before and after.** Both are in section 3. The count is 9 of 20 before (`CW` 9 of 9, `CW DX` 0 of 4, `QRP` 0 of 7) and 20 of 20 after.
- **The conditions are unchanged.** The change is two `modes` entries, `CW DX` `sameAs` `CW` and `QRP` `sameAs` `CW`, in the shape `FT4` `sameAs` `FT8` already uses. `ReceiverConditions.Parse` already resolves that shape. The file's diff is 10 added lines at line 174 and no removed line, so the CW row prints nothing. Nothing under `src` changed.
- **An unconfirmed condition is still not written.** The CW row marks none of its nine unconfirmed, so in the three Morse blocks there is nothing for the rule to act on. FT8's AGC (line 163, `confirmed: false`) proves the rule. `AnUnconfirmedConditionIsStillNotWritten` drives it beside the three: FT8's AGC is filed `SpokenOnly`, no AGC byte goes out, and the radio stays at MID.

**7.7 ticked.**

**7.5 re-read against the new table, and ticked.** Unit 419 met every clause where a block stated the CW row, and left 7.5 open only on P13. PHASE_PLAN.md's R70 entry says R70 closes that hole. The re-printed table (section 3) shows every clause holding in all three Morse blocks and in FT8 and FT4:

- Fields written on a CW tune-in when the radio was already right: 0 at 14.050 and 0 at 7.030.
- The preamp's band rule is carried by what is written. At 7.030 it is now written 0.
- Every field has one owner, and no voice asks him to change a field after the setup has set it ("no" on every row).
- His hand stands, and now also across the blocks of one band.
- Unconfirmed is spoken only.

**The one row that does not end where it was asked is the attenuator in the overloading case, filed `NotConfirmed`.** That is P14, parked since unit 419 and unchanged here.

**7.6, the exit round.** Hamlet.sln builds with warnings as errors, non-incremental, 0 warnings.

- Engine carry-forward: 178 of 178.
- App carry-forward: 277 of 278. The one loss was `TheWindowHoldsBelowItsMinimumTests` to the dispatcher loop, *You've caused dispatcher loop*. Alone it was 3 of 3.
- Floors: captures 51 of 51, adjudicated 13 of 13, clean 2 of 2.
- Every Rig type: 44 types, 325 of 325, one type per invocation. That is 316 at entry plus the two new types' 9. `TheBannerSaysWhatTheRadioReadBackTests` is green among them.
- Touched types: `TheBlockStatesWhatTheModeNeedsTests` 5, `EveryModeAnswersForEverySettingTests` 6, `WhichPathsPutNarrationOnTheBarTests` 6, `HowMuchTheApplicationSaysTests` 5, `TheStatusBarStopsLecturingTests` 4. All green.
- One red: `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`, *the follow path sweep has lost its bound*. That is the same message at the same line as at unit 419's exit (P16), and no source file changed.
- `src/Hamlet.RadioEngine/Cw` prints nothing against entry. The transmit files print nothing against `7e209cb4`. All of `src` prints nothing against entry.
- **Nothing is red that was green at entry.** 7.6 held.

**Entry round.** Same build. Engine 178 of 178. App 277 of 278; `TheCarrierHoldsTheButtonsTests` was lost to the dispatcher loop and ran 8 of 8 alone. Floors 51, 13, 2. Rig types 42, 316 of 316, including `WhatEnteringAModeSetsTests` 1 of 1 and `ThePreampFollowsItsOwnTextTests` 5 of 5.

**Checking section 4 of the instruction against the tree:**

1. **Confirmed, with a line-range mismatch.** `ForBlock` looks up the block's short name, and `sameAs` is resolved in `ReceiverConditions.Parse`'s second pass, at lines 228 to 238 rather than 221 to 233. Line 221 starts the first pass.
2. **Confirmed counts, with one mismatch.** `CW` 9, `CW DX` 4, `QRP` 7, all `family: cw`, 20 blocks. **`CW DX` is not the bottom 25 kHz on 80 m.** It is 3.500 to 3.510, the bottom 10 kHz. On 40, 20 and 15 m it is the bottom 25 kHz. P13's text carried the same slip.
3. **Confirmed.** 40 m `QRP` is 7.030000 to 7.039999 after `Separate`. `Separate` trims 40 m `CW` to 7.025000 to 7.029999.
4. **Confirmed.** The CW row has no condition marked `confirmed: false`. FT8's `agc` at line 163 is the only one in the file.

**The rest of section 5:**

- **Blocks stating conditions:** 9 of 20 CW-family blocks at entry, 20 of 20 at exit. Blocks on the whole map stating nothing went from 84 to 73.
- **Tests that pinned the old silence, named before they were changed (R12, HM-DEC-165):**
  - `TheTuneInSetsOnlyWhatIsInTheWayTests.AMorseBlockProducesNoWriteAtAll` drove the first Morse block on 20 m, the `CW DX` fast lane, and asserted no write. After the data change it went red with *Assert.Empty() Failure: Collection was not empty* on its results. It was **rewritten to R70 and renamed** `ABlockThatStatesNothingProducesNoWriteAtAll`, and now drives the first 20 m block that states nothing. It was not deleted. What it guarded, that a silent block writes nothing, is kept.
  - `TheBlockStatesWhatTheModeNeedsTests.TheMorseBlocksStateWhatMorseNeeds` skipped `CW DX` and `QRP` with a comment saying they were silent. That skip is now an assertion, citing R70, so a silent Morse block fails it.
  - `ABlockWithNothingToSayProducesNoClaim` listed the 11 silent Morse blocks and asserts more than 50 silent blocks. It needed no change (73).
- **What "the same mode" means to the operator's-hand rule:** none of the three candidates. `ReceiverSetupMemory` is keyed by **field alone**. `MainWindowViewModel` carries one memory across every tune-in and re-arms it only in `OnSelectedBandChanged` (`MainWindowViewModel.cs:10183`). `_conditionsSetForBlockHz`, keyed on the block's low edge, decides only that a block gets one tune-in. So stepping from 7.025 `CW` to 7.030 `QRP` is not a new mode to the rule, and the author's ruling needed no code change.
- **Whose mode name `sameAs` gives:** neither. A `sameAs` mode is handed the target's condition list itself, and conditions carry no mode name. `ReceiverSetupVoice.Say(results)` takes only the results. So the sentence after a tune-in is word for word the same in all three blocks: 884 characters each for `CW`, `CW DX` and `QRP` (`WhichPathsPutNarrationOnTheBarTests`).
- **Also found:**
  - Task 1's radio "as unit 419 left it: preamp 1" is a mismatch. Unit 419's `ModeEntryBench.AsLeft` leaves the preamp **off** (its table reads *radio said off*). `AsLeftWithThePreampOn` was added, and the new cases use it.
  - The conditions file's own `unknowns` entry with topic `CW` still says CW's *rows state nothing* (section 4, item 2). It was not touched.
  - Mode-follow already treated all three short names as CW (`ModeFollowPlan.cs:230`). The conditions file was the only place they differed.

**Task 2 item 5, the operator's hand across blocks. Measured, not watched failing.** `TheOperatorsHandCrossesTheMorseBlocksTests` was 3 of 3 on its first run:

- 7.025 `CW`: the tune-in writes the preamp off. He sets it to 1 by hand, because on 40 m off is already the rule. At 7.030 `QRP` the preamp is filed `LeftToTheOperator`, nothing is written, and it stays at 1.
- 14.050 `CW`, then off by hand, then 14.060 `QRP`: `LeftToTheOperator`, and it stays off. This is the instruction's case.
- With the memory re-armed, as a band change does, the next tune-in sets the row's value again.

**It could not be red.** Before R70 the `QRP` block wrote nothing at all, so his hand stood by default. After R70, the field-keyed memory keeps it standing.

**Decisions this session made for itself** (the instruction's DECIDED line marks these as the author's, overrulable):

- The `CW DX` frequency is 14.010 MHz, inside the 20 m fast lane (14.000 to 14.025).
- The cross-block hand rule is left as the tree has it: keyed by field, held until the band changes. On 40 m the test's hand is preamp **on**, because the rule there already says off and a hand of "off" would prove nothing.
- Each new `sameAs` entry carries a one-sentence `because`, like FT4's. The field is not read by code.
- The renamed test and the skip turned into an assertion, both described above.
- 7.5 and 7.7 were ticked in PHASE_PLAN.md, as unit 419 ticked 6.8.
- Per-type timeouts: 300 s for single types, 600 s for captures, 480 s for the carry-forward lines, 180 s per Rig type.

**Recorded:**

- HM-DEC-175 in `DECISIONS.md` and at the top of `CLAUDE.md` §1, as the instruction gave it, as Tim's ruling.
- In `PARKED.md`, P13 is marked answered by R70, and P17 was added verbatim.
- The version moved from 1.13.106 to 1.13.107.
- Nothing was recorded under §12.1.

## 2. What the owner should expect

Tune to 7.030 with the preamp on, and Hamlet now does exactly what it does in the CW block beside it:

- the preamp goes off, as the row's text says for 40 m and below
- the noise blanker goes off
- AGC goes to fast
- anything already right is left alone and not re-sent

Tune to 14.010, in the 20 m fast lane, with the preamp on, and it stays on, because above 40 m the row wants preamp 1. The rest is set the same way as at 14.050. If you then set the preamp by hand and move along the band, from CW main street into the QRP watering hole or back, your setting stands. It holds until you change band, as it already did within one block. Nothing about the decoder changed, and nothing that keys, transmits or sets power changed. The only data change is two lines in the conditions file saying those blocks are CW. **P17 is yours:** whether a stray single-element letter the key says was never sent may leave a named floor. Until you rule, 3.6 cannot move.

**Looks wrong but is not:**

- A tune-in into the bottom of 80, 40, 20 or 15 m, or into any QRP watering hole, now writes to the radio where it wrote nothing yesterday. That is R70.
- The attenuator write in an overload may not take (P14). It was already that way before this unit.
- `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten` is red. It was red before this unit (P16).
- The app carry-forward line loses one type to the dispatcher loop on most runs, and that type passes alone.
- `TheTuneInSetsOnlyWhatIsInTheWayTests.AMorseBlockProducesNoWriteAtAll` no longer exists under that name. It is `ABlockThatStatesNothingProducesNoWriteAtAll`.

Everything was pushed to `main`, and the push succeeded on every commit: `b79865c7`, `54927894`, `bba76811`, and the exit commit carrying this report. `WORK_INSTRUCTIONS.md`, `RUN_LEDGER.md` and the runner's own `.run-unit` files were modified in the tree at entry by the loop, not by this session. They were left for the loop's own commit, as unit 419 left them.

## 3. What you should see

**The field table at the three frequencies, before and after.** Printed by `WhatEnteringAModeSetsTests.PrintTheTable` against `ScriptedRadio`, driven as the app drives it. The radio starts at preamp 1, AGC MID, noise blanker on, RF gain 100%, everything else off or open. The full outputs are `.run-unit/unit420-t1-before-WhatEnteringAModeSetsTests.txt` and `.run-unit/unit420-exit-WhatEnteringAModeSetsTests.txt`.

**7.030, 40 m: QRP watering hole.** Before, it stated 0 conditions. After, it states 9.

| field | asks (after) | radio said | before: written, filed, owner | after: written, filed, now, owner |
|---|---|---|---|---|
| auto notch | off | off | none, no tune-in, nobody | none, AlreadyRight, off, setup |
| manual notch | off | off | none, no tune-in, nobody | none, AlreadyRight, off, setup |
| noise blanker | off | on | **none, no tune-in, stays on, nobody** | **0, Changed, off, setup** |
| noise reduction | off | off | none, no tune-in, nobody | none, AlreadyRight, off, setup |
| AGC | fast | MID | **none, no tune-in, stays MID, nobody** | **1, Changed, FAST, setup** |
| RF gain | 100% [255] | 100% | none, no tune-in, nobody | none, AlreadyRight, 100%, setup |
| squelch | open | 0% | none, no tune-in, nobody | none, AlreadyRight, 0%, setup |
| attenuator | off unless overloading | off | none, no tune-in, nobody | none, AlreadyRight, off, setup |
| preamp | off at 40 m and below | preamp 1 | **none, no tune-in, stays preamp 1, nobody** | **0, Changed, off, setup** |

**14.010, 20 m: CW fast lane (`CW DX`).** Before, it stated 0 conditions and nothing was written. After, it states 9. The noise blanker is written 0 (Changed, off) and AGC 1 (Changed, FAST). Every other field is AlreadyRight, the preamp staying at preamp 1, and the setup owns all nine. That is identical to 14.050.

**14.050, 20 m: CW main street.** 9 conditions before and after, and unchanged: noise blanker 0 Changed, AGC 1 Changed, the rest AlreadyRight, preamp 1 kept, setup owns all nine.

**Count of CW-family blocks stating receiver conditions:**

| | before | after |
|---|---|---|
| `CW` | 9 of 9 | 9 of 9 |
| `CW DX` | 0 of 4 | 4 of 4 |
| `QRP` | 0 of 7 | 7 of 7 |
| **all** | **9 of 20** | **20 of 20** |

**Unit 419's table again, after the change.** CW at 14.050, CW at 14.050 overloading, and FT8 and FT4 are unchanged from unit 419's exit. CW at 7.030 no longer prints *THE APP WRITES NOTHING HERE*. The app's block there now states 9 conditions, and every field ends as the CW row asks. Fields written when the radio was already right: 0 at 14.050, 0 at 7.030. His hand stands in both of unit 419's cases.

**On the screen, at the radio:** tuning to 7.030, or anywhere in a band's fast lane or QRP watering hole, now turns the noise blanker off, sets AGC fast and sets the preamp by the band rule, where before it left the radio as it was. The sentence after the tune-in is the same one you get in a CW block.

## 4. What's blocking us

Nothing blocks the phase or step 7's remaining criteria. Two items are raised here, P14, P15 and P16 stay parked as unit 419 left them, and the loop goes on (R65).

1. **P17: 3.6 cannot remove a stray letter without lowering a floor.** It is recorded verbatim in `PARKED.md`, with the arbiter's proposed ruling. It is the owner's. Step 3 cannot move until it is ruled, and it is not in the way of any criterion of step 7.
2. **The conditions file's `unknowns` entry for `CW` is stale.** It says *Nothing else about CW has been measured here, so its rows state nothing*, while the same file states nine CW conditions, now for 20 blocks.
   - Ruling proposed: reword or remove that entry to match the file, keeping what is still true in it (the CW pitch has its own entry).
   - Reasoning: the file's `_about` says unknowns are what it deliberately does not cover, and this one describes a state the file left on 2026-08-29.
   - Rejected here: touching it in this unit. `TheBlockStatesWhatTheModeNeedsTests.ABlockWithNothingToSayProducesNoClaim` asserts an unknown whose topic contains `CW` exists, and §12.6 says it is not repaired on the way past.

Still parked, unchanged: P14 (the attenuator's 20 dB written as the plain byte `0x14`), P15 (the advice's percent-against-raw thresholds), P16 (the source-sweep bound). P13 is answered by R70 and dropped.

### Asks still outstanding

Carried verbatim from unit 418's report, first raised 2026-09-24. Both wait on the owner's ruling. The change they concern is not in the tree. Both sheet lines are composed in `MainWindowViewModel.cs` (`captured` near 11729, `broadcast` near 12981 at unit 418's count).

1. **P12, the two clock lines** (`captured`, `broadcast`). Since unit 417 they read the clock about a second after the press.
   - Ruling proposed: take one timestamp at the press and hand it to both.
   - Reasoning: it is the same fault that made the `reading` line false, and neither line is about a signal, so 6.2 does not need it.
   - Rejected: fixing it here, because the instruction's drop rule parks sentences outside 6.2's scope.
2. **Whether `clipping`, and possibly `inputFloor`, should be measured over the whole recording** rather than captioned as the level meter's figures at the press.
   - Ruling proposed: measure clipping over the file, as HM-DEC-094 already did for `inputPeak`. A clip anywhere in 30 seconds is what a reader of the sheet wants, and today's caption is true but covers only 0.2 s.
   - Rejected here: this unit's rule was that the sentence moves and the measurement does not, and a new figure on the sheet is a promise the owner should make.
