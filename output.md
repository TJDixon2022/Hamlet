READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial on 3.6;
   4 not started; 5 the owner's; 6 partial on 6.5; 7 partial on 7.1, 7.2, 7.4, 7.6, 7.8.
B. Step 7, criterion 7.4: whether re-mixing the held window at the new pitch was kept,
   3.2's four tests as numbers, and the opening - stream 30 to 46.2 s, 003901 and 003919
   cold - before and after as text with named counts.
   Answer: not kept. Test 1 passed, 165 to 163 over 565. Test 2 failed on 3 floors, each
   down by 1. The three adjudicated readings held, but the adjudicated floor lost 031838's
   ", AND". Test 4 failed: 14 of 51 rows fell, 11 of them failing the captures test. The
   opening's text moved, from 22 named to 19 named, and went back out with the change.
C. The rest, weighed against A and B. Section 4 raises 3 items. None stands in the way of 7.4
   as a criterion. Item 3 says what this unit measured is still in the way of the opening's
   text: the estimator's unit at the right pitch, and the tracker's move, which R76 holds.

UNIT:       437 - complete at task 3 of 3, none dropped - 2026-09-25 11:19
PHASE GOAL: The CW decoder reads a real CQ call as the text that was sent, measured by edit distance against keys, and at the end the owner says so at the radio.
UNIT GOAL:  Find out whether the opening reads wrong after the mix reaches the sender because the 12 s window still holds audio mixed at the old pitch, and fix that by re-mixing the window, if doing so costs no other recording.
ADVANCED:   no - the re-mix was built and judged, failed 3.2's tests 2 and 4, and was taken back out; 7.4 is not ticked
NUMBER:     stream 30 to 46.2 s: 22 named UIEH EE E E T I NIEEE E E ET N ■IK -> 19 named I ANTTETNEETET E ET N ■I K under the change, 22 at exit; keyed 165 over 565 -> 163 under the change, 165 at exit; re-mixes fired on the opening 4, cost 21.2 ms each
DRIFT:      1 consecutive unit without advance  (was 0)

## 1. What Claude did

**Complete, task 3 of 3, none dropped.** The variant, task 2's drop candidate, was not built,
because its condition, a failure on exactly one row or one test, never arose. Run on machine
QUIVERFULL, branch main. Hamlet confirmed: SHACK_FACTS.md and CwProbabilisticDecoder.cs are
present, CoreHMI.sln and MURC.sln are absent. Entry HEAD was 3980e2a8. Every push succeeded.

**Task 0, the record** (`53abef84`).
- **No earlier session was live**, so nothing was waited for. At 09:58 there was no dotnet or
  testhost process and no unit435- or unit436- script. Unit 436 had committed its exit at
  09:53:50.
  - I found two claude.exe processes. 45956 is this session. 26364 has 15 minutes of CPU. It
    started no test and no unit script and committed nothing during this unit. I left it alone.
  - cmd 53948, named in SESSION.lock, is this session's launcher.
- 7.4 was unticked, so this unit builds.
- The record:
  - Version 1.13.122 to 1.13.123.
  - PHASE_STATUS.md names unit 437, *the window is re-mixed when the mixdown moves*, and
    CURRENT_STEP 7. The launcher had left 3 there.
  - PHASE_OUTCOME.md has UNIT 437 - STEP 7.
  - The launcher's root files were committed as they stood: RUN_LEDGER.md, WORK_INSTRUCTIONS.md
    and PROJECT_STATUS.md. The launcher's modified `.run-unit` files are left uncommitted, as
    unit 436 left them.
- **Section 5 matched the tree, with no mismatch.**
  - `Process` is at `CwProbabilisticStream.cs` 239 to 270 and `PushEnvelope` at 350 to 407.
  - `_mixedI` and `_mixedQ` are `_windowSamples` long, the integrator's 1600 samples.
  - `_envelope` is 2400 hops of 5 ms.
  - No raw sample is kept.
  - `CwDecoder.cs` 617 to 621 is the only assignment to the stream's `ToneHz` in src.
- **Entry round**, one type per invocation. Every output was identical to unit 436's exit apart
  from timings.

  | Check | Result | Time |
  |---|---|---|
  | Build, warnings as errors | 0 errors | |
  | Engine carry-forward | 178 of 178 | 381 s |
  | App carry-forward | 278 of 278 | 155 s |
  | Captures | 51 of 51 | 121 s |
  | Adjudicated | 13 of 13 | 29 s |
  | Keyed floors | 13 of 13 | 61 s |
  | Keyed totals | 165 over 565; 17:37 19 over 25 | |
  | WhatTheStrayLettersRestOnTests | 3 of 3; 17 added, 8 single-element | 291 s |
  | WhatTheOpeningHeardTests | 18 of 18 | 312 s |
  | TheUnitIsMeasuredNotSearchedTests | 4 of 5, the plateau red at entry, recorded only | |

**Task 1, the trace** (`998de3fe`). I added two printers to `WhatTheOpeningHeardTests`. They
assert nothing and write nothing.
- **`WhenTheWindowIsRemixed`** drives the decoder a hop at a time and feeds two shadow streams
  that the test owns. They get the same audio at the pitch the decoder mixed each hop at.
  - The first shadow is left alone. On all three runs it read identically to the decoder.
  - The second is re-mixed through reflection whenever that pitch stands 25 Hz or more from the
    pitch its window was last mixed at. The re-mix uses the same taper and integrator, and the
    stream's own phase running backwards. The decoder's own stream is never touched.
  - It runs on the stream from 27 to 50 s, and on 003901 and 003919 cold, whole.
- **One limit of the replay:** the tracker still sees the entry's reads through the interlock.
  The build showed this matters. The tracker moved onto 625 Hz 2 s earlier, at 34.03 s rather
  than 36.03 s.
- **The stop did not hold.** On the stream the re-mix fires 4 times: 30.53 s 600 to 525 Hz,
  36.03 s 525 to 625, 44.53 s 625 to 575, and 48.53 s 575 to 600. It settles differently on 13
  of the 33 reads from 30 to 46.2 s, and the speed differs on 25 of them.
- **Cost:** one re-mix of a full 2400-hop window takes a median 21.2 ms on this machine (range 20
  to 24.6 over 20 passes). The stream's hop budget is 5 ms.
- **`WhereTheMixMovesFar`** counts the moves: 48 of the 52 capture and keyed recordings have at
  least one move of 25 Hz or more. The most are 20 on `125941` and 16 on `014935`. The full list
  is in `.run-unit/unit437-moves.txt`. This was a forecast, not a gate.

**Task 2, built and judged.** Built at `80bf1f07`, taken out at `45394cb3`.
- The change went into `CwProbabilisticStream.cs` only, as section 6 states it.
  - A raw ring holds 12 s plus the integrator's length.
  - On a move of `RemixFromHz` 25 or more, `Remix()` recomputes the arms and every envelope hop
    at the new pitch before the next hop.
  - The remark in the file names unit 436's window reaching back into 525 Hz, and why this is
    not the window clear that was ruled off.
- **Two decisions I made for myself:**
  - `Skip` now drops the raw history, since audio skipped during a transmission does not run on.
    A re-mix then recomputes only hops whose audio it holds.
  - `Restart` was left untouched. It is unreachable while `ClearOnAStationChange` is const false.
- Under the build, the replay's shadows and the decoder agree on every read. That confirms the
  build does what the replay did.

The four tests:

| Test | Result | Numbers |
|---|---|---|
| 1. Total keyed edits do not rise | **pass** | 165 to 163 over 565. 17:37 19 to 18 over 25. Added 17 to 12, single-element 8 to 6 |
| 2. No named floor breaks | **fail**, 3 of 13 | 031838 40 to 39, 004507 49 to 48, 003758 43 to 42. The other 10 held. R73 not examined, because test 4 fails either way |
| 3. Adjudicated readings unchanged | **pass on the three**; the adjudicated floor 12 of 13 | VA3VRR 6 of 6, MP/4 QNIK 9 of 12, DE KD0UN KD0UN K 16 of 16, the same before and after. 031838 no longer contains `, AND`: it read `U, TE3, 3, 2 TT, 2, AN ATT TTTTT  WIU H A…` |
| 4. No capture row's above-bar count falls | **fail**, 14 of 51 fell; 11 fail the captures test | See below |

Test 4, every row that fell (entry to change):
- Characters: 012823 23 to 12, 021629 27 to 20, 021825 19 to 12, 001831 43 to 41, 021410 36 to
  34, 012748 2 to 0, 012922 43 to 39, 013622 49 to 48, 011552 22 to 20, 004507 49 to 48, 003758
  43 to 42, 031838 40 to 39.
- Elements: 134712 31 to 30, 004427 111 to 107, 012823 37 to 25, 021825 43 to 36, 011552 74 to
  69, 012748 3 to 0.
- 004507, 003758 and 031838 are the three that pass the captures type, because their count floor
  is carried by the keyed floors, which they broke.
- 21 rows gained, some a lot: 031905 36 to 68, 032050 44 to 69, 032113 47 to 67, 032129 65 to 98.
  16 rows were unchanged.
- All 51 rows, before and after, are in `.run-unit/unit437-rows-table.txt`.

Two tests failed, and test 4 on many rows, so the variant was not built. The change came back
out in the next commit, and src is identical to entry. P48 gained this unit's measurement. 7.4
is not ticked.

**Task 3, the exit round.**

| Check | Result | Time |
|---|---|---|
| Build | 0 errors | |
| Engine carry-forward | 178 of 178 | 375 s |
| App carry-forward | 274 of 278 | 162 s |
| Captures | 51 of 51 | 120 s |
| Adjudicated | 13 of 13 | 29 s |
| Keyed floors | 13 of 13 | 62 s |
| Keyed totals | 165 over 565 | 19 s |
| Strays | 3 of 3; 17 added, 8 single | 289 s |
| WhatTheOpeningHeardTests | 22 of 22 | 460 s |
| TheUnitIsMeasuredNotSearchedTests | 4 of 5, the same plateau red as entry | |

- **App carry-forward:** all four failures were the dispatcher-loop loss (P45): two
  `TheChipSaysTheChosenModeTests` cases and two `ThePsk31ConversationCardTests`. Each type passed
  alone, 6 of 6 and 8 of 8. App code is untouched.
- **WhatTheOpeningHeardTests** went from 18 tests to 22, because of the new printers.
- **Every output is identical to entry apart from timings.** That covers captures, adjudicated,
  floors, keyed, strays and plateau, and the opening text and replay rows too.
- **No stream test type:** nothing is named for `CwProbabilisticStream`, and the filter matched
  none. The stream is exercised by the captures, floors and opening types above.
- **Diffs:**
  - `git diff` of the eleven transmit files against 7e209cb4 printed nothing.
  - `git diff` of CwToneTracker.cs, CwToneSurvey.cs, CwDecoder.cs and CwUnitEstimator.cs against
    3980e2a8 printed nothing.
  - All of src and data against 3980e2a8 printed nothing.

## 2. What the owner should expect

**Nothing changes at the radio. The first minute on a new frequency still comes apart, and this
unit showed that stale audio in the window is not what holds it.** I rebuilt the whole 12-second
window at the new pitch every time the decoder's mixdown moved far. From the moment the mix
reached the sender, the decoder heard only audio at his pitch. Even then, the reads from 36.5 to
41.5 s measured a 27.5 to 37.5 ms dot. That is 30 to 40 WPM for a sender near 22 WPM, whose dot
is about 55 ms, and the reads settled almost nothing until `E ET N`. Two things are still in the
way:
- the tracker's wrong move to 525 Hz at 30.5 s, which R76 holds behind 4.1;
- the estimator reading the sender's dot at about half its length, even at the right pitch.

Re-mixing did help many other recordings: 21 of 51 rows gained, and four of the 2026-08-22
recordings, 031905, 032050, 032113 and 032129, gained 20 to 33 characters each. But it
cost 14 recordings characters or elements, so it came back out.

Two things may look wrong but are not:
- `WhatTheOpeningHeardTests` now has 22 tests, not 18. The four new ones are printers and decide
  nothing.
- That type now takes about 460 s, closer to its 600 s timeout than before.

## 3. What you should see

**No visible change.** The change was taken back out, and at exit the decoder is byte-identical
to entry.

**The opening, text and named count:**

| Stretch | Before, and at exit | Offline replay, task 1 | Under the built change |
|---|---|---|---|
| stream 30 to 46.2 s | `UIEH EE E E T I NIEEE E E ET N ■IK`, 22 named | `E A N Q N IK E ET N ■I K`, 13 named | `I ANTTETNEETET E ET N ■I K`, 19 named |
| 003901 cold | `EII E T NHHK`, 9 named | the same, no re-mix fires | the same, 9 named |
| 003919 cold | `EITEETNXNIK EANQNID EANQNIK`, 25 named | `… EANQNIS`, 25 named | `EITEETNXNIK EANQNID EANQNIS`, 25 named |

**Per read, stream, task 1's entry beside the built change.** Speeds are in WPM with where they
came from; unit is the estimator's, in ms.

| s | entry mix Hz | off-pitch s at entry | entry unit, speed | built mix Hz | built unit, speed | built settles |
|---|---|---|---|---|---|---|
| 30.0 to 30.5 | 600 | 0 | 85.0, 14.1 est | 600 | the same | - |
| 31.0 | 525, move | 11.53 | 67.5, 17.8 est | 525, re-mixed | 20.0, 40 grid | - |
| 31.5 to 32.0 | 525 | 11.0 to 10.5 | 40 to 30, 30 to 40 est; `UIEH` at 32.0 | 525 | 17.5 to 20, 40 grid | - |
| 32.5 to 34.0 | 525 | 10.0 to 8.5 | 22.5 to 27.5, 38 grid; `EE E E T I` | 525 | 17.5 to 20, 30 to 38 grid | - |
| 34.5 | 525 | 8.0 | 25.0, 38 grid | 625, re-mixed | 30.0, 40 est | `I ANTTETNEETET` |
| 35.0 to 36.0 | 525 | 7.5 to 6.5 | 17.5 to 27.5, 30 to 38 grid; `NIEEE` | 625 | 30 to 32.5, 36.9 to 40 est | - |
| 36.5 to 38.0 | 625, move at 36.03 | 11.5 to 10.0 | 17.5 to 20, 36 to 38 grid | 625 | 27.5, 30 grid | - |
| 38.5 to 41.0 | 625 | 9.5 to 7.0 | 20 to 25, 36 to 38 grid | 625 | 30 to 37.5, 32 to 40 est | - |
| 41.5 to 42.0 | 625 | 6.5 to 6.0 | 27.5 to 30, 38 grid then 40 est; `E E ET N` | 625 | 32.5, 36.9 est | `E ET N` at 42.0 |
| 42.5 to 44.5 | 625 | 5.5 to 3.5 | 30 to 35, 34.3 to 40 est; `■IK` settled later | 625 | 37.5 to 45, 26.7 to 32 est | `■` 43.0, `I` 43.5 |
| 45.0 to 46.0 | 575, move | 11.5 to 10.5 | 37.5 to 45, 26.7 to 34.3 est | 575, re-mixed | 35 to 37.5, 32 to 34.3 est | - |

From 36.5 s, the replay and the build give the same unit and speed on every read.

**Unit 436's question, answered: no.** With the window at the sender's pitch, the opening from
36.5 s does not read the sender's text. Its window is all 625 Hz from 34.5 s under the build.
Even so, its estimator unit is 27.5 to 37.5 ms and its speed 30 to 40 WPM, against the sender's
55 ms and about 22 WPM. It settles only `E ET N ■I K` by 46.2 s.

## 4. What's blocking us

1. **`TheUnitIsMeasuredNotSearchedTests.TheFiveToEightDecibelPlateauHolds` is still red at HEAD,
   exactly as at entry.** It is outside the carry-forward.
   - It was red at this unit's entry, so it does not count against HM-DEC-165.
   - **Proposed ruling:** park it as a standing red for whichever unit opens the hysteresis.
   - **Reasoning:** §9 parks the hysteresis, and §12.5 says repair nothing.
   - **Rejected:** repairing it here.
   - Not blocking 7.4.
2. **The launcher-overlap ruling is still the owner's and still carried:** the launcher should end
   a redirected session before it launches the next.
   - There was no collision this time. Nothing was live at 09:58, and task 0's check held.
   - One claude.exe process, 26364, was running beside this session the whole time. It started
     no test and committed nothing. I did not identify it or touch it.
   - Not blocking 7.4.
3. **What is in the opening's way, as measured.**
   - Every route downstream of the pitch has now been tried against 7.4 and failed 3.2's tests:
     four follow rules (R75), the speed carry, the dit-against-dah rule, and now the window
     re-mix.
   - This unit found that the window's pitch is not the remaining cause. The estimator's unit
     is still about half the sender's at the right pitch, over a window that is wholly at that
     pitch.
   - **Proposed ruling:** the next 7.4 unit, if one is seeded, looks at why `CwUnitEstimator`
     reads about 30 ms at 625 Hz on this sender. Otherwise 7.4 waits for 4.7.
   - **Reasoning:** unit 436's rule attacked that halving and cost recordings. This unit shows
     the halving survives a clean window, so it lies in the audio or the trigger, not the mix.
   - **Rejected:** keeping the re-mix for the recordings it helps, because HM-DEC-091 forbids
     reading one recording at another's cost.
   - The author's or the owner's to rule, not this unit's. Not blocking 7.4 as a criterion.
