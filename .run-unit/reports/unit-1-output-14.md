READ IN THIS ORDER.

A. The phase goal is that Hamlet reads a CQ call correctly. Section 3 opens with the field table for CW at 14.050, CW at 7.030 and each data mode, before and after.
B. Step 7, criterion 7.5, clause by clause: written once, band rule carried, one owner per field, the operator's hand wins. Then 7.6, the exit round. Both are at the top of section 1.
C. The rest. Section 4 raises 4 items, none blocking. P13 bears on B: at 7.030 the app writes nothing, because that frequency is the QRP block and the QRP block states no conditions. Nothing here bears on A. The decoder was not touched.

```
UNIT:       419 - complete at task 3 of 3, none dropped - 2026-09-24 12:06
PHASE GOAL: Hamlet decodes a real CQ call into the text that was sent, and the receiver in front of the decoder is set the way the mode needs it
UNIT GOAL:  Tuning into CW or a data mode sets the receive side once, rewrites nothing the radio already holds, lets one component decide each setting, and leaves alone a setting the operator changed himself
ADVANCED:   yes - 7.5's four clauses now hold in every block that states the CW, FT8 or FT4 row, each fix watched failing first; 7.5 stays open only on P13, the CW DX and QRP blocks; 6.8 ticked
NUMBER:     fields written on a CW tune-in when the radio was already right: 1 -> 0, at 14.050 and at 7.030
DRIFT:      0
```

## 1. What Claude did

**Complete, task 3 of 3, none dropped.** This ran on QUIVERFULL. The gate confirmed Hamlet by its four filesystem checks. Branch `main`, entry HEAD `4bd85b35`. **Nothing in this report is evidence about the radio.** No IC-7300 is attached to this machine (FACT-006). Every result comes from `ScriptedRadio`.

**Criterion 7.5, clause by clause:**

- **Written once, and not written when the radio already holds the value.** Met at 71e89027. Before, the RF gain was written 255 on every CW tune-in, even with the radio already at full, and was filed `NotConfirmed`. Red, 5 of 5: `TheRfGainAtFullIsNotWritten` failed with *Collection was not empty, [255]* (radio at raw 255: wrote [255], filed NotConfirmed, was 100%, now 100%). `TheRigConfirmsALevelOnTheScaleItReads` failed with *outcome ReadBackDisagreed, read back 100%*. Green after, 5 of 5.
- **The band rule is carried by what is written.** Already true at entry. **Not watched failing, because the premise was false** (see the mismatches below). `ThePreampFollowsItsOwnTextTests` was 5 of 5 on its first run and now pins the rule (2389b1d4). No code changed for this clause.
- **One owner per field.** Met at 9340b79f. Red: `OneVoicePerFieldTests` failed 4 of 6. The advice said *Switch the preamp on* at 7.030, *Open the receive gain ... about 39 percent* on CW, and *Set the gain control to fast* on FT8. The observations said *The attenuator is at 20 dB and the preamp is on as...*. `TheOverloadSentenceLeavesTheModesFieldsTests` failed 2 of 4 with *Sub-string found ... Press P.AMP/ATT on the fron...*. Green after, 6 of 6 and 4 of 4.
- **The operator's hand wins.** Met at d2f13ab0. Red: `TheOperatorsHandStandsTests` failed 2 of 4, *Expected: LeftToTheOperator, Actual: Changed* (preamp 1 at first; off by hand; second tune-in Changed, radio now 1). Green after, 4 of 4.
- **The table.** Task 1, `WhatEnteringAModeSetsTests.PrintTheTable`. It is printed before and after in section 3.

**7.5 is advanced and not ticked.** Every clause holds where the block states the CW row. At 7.030, one of this instruction's own two frequencies, the app's block is the QRP watering hole, which states nothing. Entering CW there sets nothing (P13, section 4). **6.8 is ticked.** Item 3 is exactly 6.8: it was watched failing against a radio already at the value, and it now sends no byte when the value matches.

**7.6, the exit round.** Hamlet.sln builds with warnings as errors, non-incremental, 0 warnings. Results:

- Engine carry-forward: 178 of 178.
- App carry-forward: 277 of 278. The one loss is `ThePowerIsOfferedTests` to the dispatcher loop; alone it was 3 of 3.
- Floors: captures 51 of 51, adjudicated 13 of 13, clean 2 of 2.
- Every Rig type: 42 types, 316 of 316, one type per invocation.
- `TheBannerSaysWhatTheRadioReadBackTests`: 5 of 5.
- Sheet tests: `EverySentenceOnTheSheetTests` 1, `TheKeyingCaptionNamesTheSweepItRanTests` 2, `TheRestOfTheSheetIsTrueTests` 10, `TheSidecarDoesNotContradictItselfTests` 3, `TheSidecarIsReReadTests` 2, `TheTonePeakIsAboutThisRecordingTests` 3, `CaseRosterSurvivesAnEveningTests` 6. All green.
- Touched types: `RigDiagnosticsTests` 10, `TheAlcIsReadTests` 8, `HowMuchTheApplicationSaysTests` 5, `TheFrontEndIsOnThePanelTests` 6, `TheStatusBarStopsLecturingTests` 4, `WhichPathsPutNarrationOnTheBarTests` 6, `AutoCallSafetyTests` 17, `CwTransmitTests` 18, `BandScannerSafetyTests` 13, `EveryModeAnswersForEverySettingTests` 6, `TheBlockStatesWhatTheModeNeedsTests` 5. All green.
- One red: `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`. **It was red before this unit** (P16). It fails on a source-length bound over a region that is byte-identical at entry, and none of this unit's hunks fall in that region.
- `src/Hamlet.RadioEngine/Cw` prints nothing against entry. The eleven transmit files print nothing against `7e209cb4`.

**Entry round.** Same build. Engine 178 of 178. App 273 of 278; five were lost to the dispatcher loop, and their four types ran 10 of 10 alone. Floors 51, 13, 2. Rig types 37, 295 of 295.

**Checking section 4 of the instruction against the tree:**

1. **Mismatch.** The band rule is carried by a value, not only by prose. The CW preamp row carries `"condition": "band"`. `ReceiverSetup.ResolveAsync` (`ReceiverSetup.cs` 321 to 334 at entry) reads the frequency and writes 1 above 10 MHz and 0 at or below. The comment at 291 describes that code. `condition.Wanted` has one reader in `src`, `ReceiverSetup.cs` 163, and for this row the resolver overrides it. The data file does hold `wanted: 1` beside the rule's text, and the 10 MHz boundary lives in code, not in the file.
2. **Confirmed, with a fourth voice the instruction did not name.** `ReceiveAdvice.Preamp` (318) only ever suggests on. `RigObservations.AttenuatorAndPreampTogether` (173) objects to both being on. The fourth voice is `MainWindowViewModel.OverflowAdviceFor`, on the main panel beside the front-end chip. While the radio reports overload with the preamp on, it says *Press P.AMP/ATT on the front of the radio until the preamp reads off*. That matches Tim's "complains it is not off" most closely. The advice's *Do all* button (`ReceiveHelpViewModel`) writes what the advice suggests, preamp 1 included.
3. **Confirmed.** `CivDecode.DecodePercent` reads the RF gain as a percent. `ReceiverSetup` compared 100 against 255 and wrote, and `SetSettingAsync` compared the percent read-back against the raw value and filed `ReadBackDisagreed`. The claim that the hand therefore never sticks is **half right**. A preamp Hamlet wrote was remembered, and the operator's change to it did stand. A field the first tune-in *found* already right was not remembered, and that is how his hand was overwritten.
4. **Confirmed.** `ReceiveObstructions.cs` line 19 cites HM-DEC-148. **What HM-DEC-148 governs:** the main panel's front-end chip and the obstruction line show the receive path's own settings and say what they mean, and do not write them. The attenuator is mentioned only once the preamp is off. RF gain is not shown. Its *Rejected: Hamlet turning the preamp off itself* was later overtaken by Tim's ruling of 2026-08-29, which put the band rule into the setup (`ReceiverConditions.cs` remarks). No writer was added to any HM-DEC-148 surface. `ReceiveObstructions` speaks only on the noise blanker, the noise reduction and the filter width. It never proposes a write and never objects to a value the setup set, so it is unchanged.

**The rest of section 5:**

- **Modes the file speaks for:** CW, FT8 and FT4. FT4 is `sameAs` FT8. **"Data mode" in 7.5 is FT8 and FT4.**
- **Unconfirmed conditions:** only AGC in FT8, and so in FT4 as well (asks slow, confirm tim). It is spoken and not written, before and after.
- **Callers of `ReceiverSetup.ApplyAsync`:** one, `MainWindowViewModel.EstablishReceiveConditionsAsync`. It is reached from `FollowTheMapAsync` on the mode-settle timer after `ScheduleModeFollow`, and on a matured dwell. It runs **once per block**, keyed on the block's low edge (`_conditionsSetForBlockHz`), and is re-armed only by a band change. **Nothing re-runs it while the operator sits in one mode on one frequency.**
- **Value ages:** `RigState` holds each value's age (`RigValue.AtUtc`, `Age(now)`). The setup already read each field fresh before deciding, and still does. No new read or write was added.
- **Seen on the way past:**
  - The advice's own RF gain and USB-level thresholds compare percent reads against raw constants (P15).
  - The attenuator's 20 dB is sent as the plain byte `0x14`, where the radio's scale is BCD `0x20` (P14).

**Decisions this session made for itself** (the instruction's DECIDED line marks these as the author's, overrulable):

- **How the band rule is carried.** Kept as the tree already had it: the row names the rule and the setup derives the value from the frequency the radio reports. Nothing was changed.
- **How each voice is scoped.** `ReceiverSetup.Owns(results)` is the set of fields the last tune-in's conditions name, whether written or only spoken. Each voice uses that set:
  - `ReceiveAdvice.For(state, owned)` keeps every row. An owned field's row becomes a no-change row: *The {name} is covered by what this mode states when you tune in, so Hamlet is leaving it out of these suggestions.*
  - `RigObservations.For(state, owned)` skips the attenuator-and-preamp observation only when both are owned.
  - `OverflowAdviceFor(overloading, preampIsOn, frontEndOwned)` still says the front end is overloading and adds *The preamp and the attenuator are set by this mode when you tune in, so Hamlet is not asking you to change them here.*
  - With no tune-in behind them, all three say exactly what they said before, and tests hold that. The app feeds `MainWindowViewModel.OwnedByTheMode` to the overload sentence, `ReceiveHelpViewModel` and `RigDiagnosticsViewModel`.
- **Which scale the comparison uses.** The read's. `CivDecode.OnReadScale` converts the six percent-read levels and passes every other field through unchanged. `SetSettingAsync` applies it to receive-tier writes only, so RF power, keyer speed and break-in are compared exactly as before. The byte written is unchanged: still the row's own 255.
- **How long the hand holds.** Until the band changes. That is where `Rearmed` is already called, the same as mode-follow under HM-DEC-056. The memory now holds every field a tune-in left right, whether written or found.
- **Two earlier tests edited, because they encoded the old behaviour:**
  - `TheBannerSaysWhatTheRadioReadBackTests.AHeldReadBackIsStatedWithItsTime` got its held read-back from the scale fault. It now gets a real disagreement from `ScriptedRadio.LevelWritesLandAt`, a radio that lands the gain at 250. Data changed from 255 and 108 to 108 and 200, and the expected text from 100% to 98%. Criterion unchanged.
  - `TheTuneInSetsOnlyWhatIsInTheWayTests` asserted the memory held 2 fields (writes only). It now asserts 3, which includes the noise reduction found right, per HM-DEC-174.
- **Per-type timeouts:** 300 s for single types, 600 s for captures, 480 s for the carry-forward lines, 180 s per Rig type.

**Recorded:** HM-DEC-174 in `DECISIONS.md` and at the top of `CLAUDE.md` §1, as the instruction gave it, as Tim's ruling. Nothing was recorded under §12.1. P13 to P16 were added to `docs/phase-correctness/PARKED.md`. The version moved from 1.13.105 to 1.13.106.

## 2. What the owner should expect

**What changed at the radio.** In a CW block at 14.050, a tune-in sets:

- the preamp to 1
- the noise blanker, noise reduction and both notches off
- AGC to fast
- the attenuator off, unless the front end is overloading

It no longer rewrites an RF gain that is already at full, so a radio already set this way receives no bytes at all. In a CW block on 40 m, such as 7.025 to 7.029, the same tune-in leaves the preamp off, or turns it off if it was on. **At exactly 7.030 nothing is written, because that frequency starts the QRP watering hole and that block states no conditions (P13).** If you set the preamp off by hand, the next tune-in on the same band leaves it off. This now holds whether Hamlet wrote the preamp or found it already on. It holds until you change band, which re-arms the tune-in as mode-follow's re-arm has always done. After a CW tune-in, neither the *I can hear it* panel nor the diagnostics screen asks you to change a setting the tune-in decided. While the front end is overloading, the panel says so but no longer tells you to press P.AMP/ATT in a CW block. **Nothing about the decoder, and nothing that keys, transmits or sets power, changed.**

**Looks wrong but is not:**

- Outside a CW block, the advice still describes an RF gain at full as *about 39 percent* (P15).
- The attenuator write in an overload may not take (P14). It was already that way before this unit.
- `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten` is red. It was red before this unit started (P16).
- The app carry-forward line loses a type or two to the dispatcher loop on most runs. They pass alone.

Everything was pushed to `main`. Push succeeded on every commit: 55b10b29, 36ffb1ee, 2389b1d4, 9340b79f, 71e89027, d2f13ab0, and the exit commit carrying this report.

## 3. What you should see

**The field table, before and after.** Printed by `WhatEnteringAModeSetsTests.PrintTheTable` against `ScriptedRadio`. The radio starts as an operator might have left it: preamp off, AGC mid, noise blanker on, RF gain at full, everything else off or open. The full tables are in `.run-unit/unit419-table-before.txt` and `.run-unit/unit419-table-after.txt`. "Voices after" means any component asking the operator to change the field after the setup has set it.

CW at 14.050 (CW main street, 9 conditions):

| field | asks | radio said | before: write, filed, voices after | after: write, filed, voices after | who mentions it |
|---|---|---|---|---|---|
| auto notch | off | off | none, AlreadyRight, no | none, AlreadyRight, no | setup, advice, observations |
| manual notch | off | off | none, AlreadyRight, no | none, AlreadyRight, no | setup |
| noise blanker | off | on | 0, Changed, no | 0, Changed, no | setup, advice, observations, obstructions |
| noise reduction | off | off | none, AlreadyRight, no | none, AlreadyRight, no | setup, advice, observations, obstructions |
| AGC | fast | MID | 1, Changed, no | 1, Changed, no | setup, advice |
| RF gain | 100% [255] | 100% | **255, NotConfirmed, advice: open it, "about 39 percent"** | **none, AlreadyRight, no** | setup, advice |
| squelch | open | 0% | none, AlreadyRight, no | none, AlreadyRight, no | setup, observations |
| attenuator | off unless overloading | off | none, AlreadyRight, no | none, AlreadyRight, no | setup, observations, panel |
| preamp | 1 above 40 m, off at 40 m and below | off | 1, Changed, no | 1, Changed, no | setup, advice, observations, panel |

CW at 7.030. **The app's block is the QRP watering hole, which states 0 conditions, so the app writes nothing, before and after.** Below is the CW row driven directly. It matches 14.050 except:

| field | before | after |
|---|---|---|
| RF gain | 255 written, NotConfirmed, advice "about 39 percent" | nothing written, AlreadyRight, no voice |
| preamp | nothing written (already off), AlreadyRight, **advice: "Switch the preamp on"** | nothing written, AlreadyRight, **no voice** |

CW at 14.050, front end overloading:

- Before: the attenuator write was refused (P14), and the panel **asked for P.AMP/ATT until the preamp reads off** a moment after the setup set it to 1.
- After: the same refusal, and the panel **states the overload and names no knob**.

FT8 at 14.074 and FT4 at 14.080 (5 conditions each):

- The noise blanker is written 0 and filed Changed.
- Noise reduction and auto notch are already right.
- The scope span is spoken only.
- AGC asks slow, is unconfirmed, and is spoken only, before and after. **Before, the advice asked for AGC fast; after, no voice.**

Summary:

| | before | after |
|---|---|---|
| fields written on a CW tune-in when the radio was already right, 14.050 | 1 (RF gain) | 0 |
| the same, CW row at 7.030 | 1 (RF gain) | 0 |
| voice lines asking him to change a field the setup had set, all five cases | 6, plus the panel on overload | 0 |
| preamp written by Hamlet, then off by hand, next tune-in | left alone | left alone |
| preamp found already at 1, then off by hand, next tune-in | **written back to 1** | **left alone, 0** |

**On the screen:** tuning into a CW block no longer re-sends the RF gain. The operator's own preamp setting survives the next tune-in on the band. The help panel and the overload sentence stop asking for the opposite of what the tune-in just did.

## 4. What's blocking us

Nothing blocks the phase. Four items are parked, and the loop goes on (R65).

1. **P13: the CW DX and QRP blocks state no receiver conditions.** This leaves 7.5 open.
   - Ruling proposed: state the CW row for the `CW DX` and `QRP` blocks with two `sameAs` lines in `mode-receiver-conditions.json`. Entering Morse in those blocks is entering CW mode.
   - Reasoning: all three carry `family: cw`. 7.030, the instruction's own 40 m frequency, is the first hertz of the QRP block, and a radio left at preamp 1 there stays at preamp 1, against the row's text.
   - Rejected here: doing it in this unit. It makes the app write in 11 blocks where it writes nothing today, and section 6 licenses 7.5 only to write less and to carry a band rule.
2. **P14: the attenuator's 20 dB is sent as the plain byte `0x14`.**
   - Ruling proposed: send it as BCD `0x20`, the way the read decodes it.
   - Reasoning: in an overload the byte sent is not the value asked for. The scripted radio refuses it. What an IC-7300 does with `11 14` is not measured.
   - Rejected here: it changes a byte sent to the radio, outside what 7.5 licenses.
3. **P15: the advice's gain and USB-level thresholds compare percent reads against raw constants.**
   - Ruling proposed: put the thresholds through `CivDecode.PercentOfLevel`, and rebuild the `RigWriteTests` fixtures that carry raw numbers labelled as percents (§12.5).
   - Rejected here: another unit's diff. After item 2, the RF gain is left alone after a CW tune-in anyway.
4. **P16: `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten` was red at entry, on its 6,000-character bound.**
   - Ruling proposed: raise the bound or trim the region's comments, so its forbidden-write checks run again.
   - Rejected here: §12.6, not repaired on the way past.

### Asks still outstanding

Carried verbatim from unit 418's report, first raised 2026-09-24. Both wait on the owner's ruling. The change they concern is not in the tree. Both sheet lines are composed in `MainWindowViewModel.cs` (`captured` near 11729, `broadcast` near 12981 at unit 418's count). Unit 411's RF gain ask was answered by R65 and is dropped. This unit's 6.8 closes it.

1. **P12, the two clock lines** (`captured`, `broadcast`). Since unit 417 they read the clock about a second after the press.
   - Ruling proposed: take one timestamp at the press and hand it to both.
   - Reasoning: it is the same fault that made the `reading` line false, and neither line is about a signal, so 6.2 does not need it.
   - Rejected: fixing it here, because the instruction's drop rule parks sentences outside 6.2's scope.
2. **Whether `clipping`, and possibly `inputFloor`, should be measured over the whole recording** rather than captioned as the level meter's figures at the press.
   - Ruling proposed: measure clipping over the file, as HM-DEC-094 already did for `inputPeak`. A clip anywhere in 30 seconds is what a reader of the sheet wants, and today's caption is true but covers only 0.2 s.
   - Rejected here: this unit's rule was that the sentence moves and the measurement does not, and a new figure on the sheet is a promise the owner should make.
