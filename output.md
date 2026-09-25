READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial on 3.6;
   4 not started; 5 the owner's; 6 partial on 6.5; 7 partial on 7.1, 7.2, 7.4, 7.6, 7.8.
B. Step 7, criterion 7.4: whether the unit estimate's local trigger cut was kept, 3.2's four
   tests as numbers, and the opening - stream 30 to 46.2 s, 003901 and 003919 cold - before
   and after as text with named counts.
   Answer: not kept. Test 1 passed, 165 to 163 over 565. Test 2 failed on 5 of 13 floors. Test
   3 failed: 013347 lost VA3VRR, and the adjudicated floor also lost 031838's ", AND". Test 4
   failed on 16 of 51 rows. The opening's text moved a long way, from 22 named to 12 named
   `EANQNID  EAN■IK`, read at 24 WPM from the estimator. It went back out with the change.
C. The rest, weighed against A and B. Section 4 raises 3 items. None stands in the way of 7.4
   as a criterion. Item 3 says the build's gain in the opening came with a different tracker
   path, so it cannot be credited to the cut alone.

UNIT:       438 - complete at task 3 of 3, none dropped - 2026-09-25 12:49
PHASE GOAL: The CW decoder reads a real CQ call as the text that was sent, measured by edit distance against keys, and at the end the owner says so at the radio.
UNIT GOAL:  Find out whether the estimator halves the sender's dot because one trigger level over 12 s chops marks where the level moves, and fix that by cutting each half second at its own level, if doing so costs no other recording.
ADVANCED:   no - the local cut was built and judged, failed 3.2's tests 2, 3 and 4, and was taken back out; 7.4 is not ticked
NUMBER:     stream 30 to 46.2 s: 22 named UIEH EE E E T I NIEEE E E ET N ■IK -> 12 named EANQNID EAN■IK under the change, 22 at exit; unit 34.5 to 44.5 s entry 17.5 to 35 ms -> replay 17.5 to 40 ms, built 45 to 67.5 ms; keyed 165 over 565 -> 163 under the change, 165 at exit
DRIFT:      2 consecutive units without advance (was 1)

## 1. What Claude did

**Complete, task 3 of 3, none dropped.** The variant, task 2's drop candidate, was not built.
Its condition, a failure on exactly one row or one test, never arose, because three tests
failed. Run on machine QUIVERFULL, branch main. Hamlet confirmed: SHACK_FACTS.md and
CwProbabilisticDecoder.cs are present, CoreHMI.sln and MURC.sln are absent. Entry HEAD was
66a99377, plus the launcher's root and `.run-unit` files; nothing else came in. Every push
succeeded.

**Task 0, the record** (`755373d2`, entry round `d2ee0fd1`).
- **No earlier chain was live**, so nothing was waited for. At 11:26 there was no dotnet or
  testhost process and no unit435-, unit436- or unit437- script.
  - I found two claude.exe processes. 43708 is this session. 26364 is an interactive session
    started 2026-09-24 13:51. It started no test and committed nothing. I noted it and left it
    alone.
- 7.4 was unticked, so this unit builds.
- The record:
  - Version 1.13.123 to 1.13.124.
  - PHASE_STATUS.md names unit 438, *the unit's trigger is cut where the marks are*, and
    CURRENT_STEP 7. The launcher had left 3 there.
  - PHASE_OUTCOME.md has UNIT 438 - STEP 7, with the entry round's numbers.
  - The launcher's modified root and `.run-unit` files were committed as they stood.
- **Section 5 matched the tree, with no mismatch.**
  - `Measure` is at `CwUnitEstimator.cs` 77 to 106 and calls `Elements` once.
  - `Elements` is at 391 to 406, with `Runs(db, Otsu(db), ...)`, one cut for the whole
    envelope.
  - `MeasureGaps` (174) and `MeasureCharacterGap` (272) also call `Elements`.
  - `HysteresisDb` is 6.0 and `ShortestRunHops` is 2.
  - `CwProbabilisticStream.cs:428` is the only call to `Measure` in src.
- **Entry round**, one type per invocation. Every number in section 5 held.

  | Check | Result | Time |
  |---|---|---|
  | Build, warnings as errors | 0 errors | 15 s |
  | Engine carry-forward | 178 of 178 | 381 s |
  | App carry-forward | 274 of 278, all four the dispatcher-loop loss; the three types alone 11 of 11 | 166 s |
  | Captures | 51 of 51 | 120 s |
  | Adjudicated | 13 of 13 | 29 s |
  | Keyed floors | 13 of 13 | 61 s |
  | Keyed totals | 165 over 565; 17:37 19 over 25 | 19 s and 40 s |
  | WhatTheStrayLettersRestOnTests | 3 of 3; 17 added, 8 single-element | 291 s |
  | WhatTheOpeningHeardTests | 22 of 22 | 461 s |
  | TheUnitIsMeasuredNotSearchedTests | 4 of 5, the plateau red, recorded only | 1 s |

  - The four app losses were `TheRstIsYoursToCorrectTests.OnTheWindowTheTwoReportsAreBoxesWithTheirMarks`,
    `TheWindowHoldsBelowItsMinimumTests.TheWorkingPanelsScrollInsideThemselvesRatherThanCollapsing`,
    and the two `TheFavoritesAreChipsTests` cases. Each was `InvalidProgramException: You've caused
    dispatcher loop`.
  - The plateau figures at entry: 1 dB 165 marks, 3 dB 130, 5 dB 118, 6 dB 117, 8 dB 116,
    failing on "one decibel gave 165 against 117".
  - `ItRecoversASpeedItWasNeverTold` was green at entry: 25 WPM measured 25.3 and 25.3, 18 WPM
    18.5 and 17.8, 12 WPM 12.0 and 12.0, at noise 0.02 and 0.08.

**Task 1, the trace** (`a610332a`). I added one printer, `WhatTheOpeningHeardTests.WhereTheTriggerCuts`.
It asserts nothing and writes nothing, and nothing in src changed.
- **How it replays.** It drives the decoder a hop at a time. On every read it takes the stream's
  own 12 s window and cuts it twice:
  - once over the whole window, as `Measure` does at entry;
  - once per 0.5 s block, at Otsu's level over the 3.0 s centred on the block, as section 6
    fixes it.

  Each unit then goes through the stream's own speed choice, gap structure, `Decode`, `Spaced`
  and settling. Each read starts from the state the stream held just before it, read by
  reflection and never written.
- **The replay checks itself three ways**, on all three runs:
  - the entry column's unit against `CwUnitEstimator.Measure`: 0 reads differ;
  - its settled text against what the stream settled: 0 reads differ;
  - each span's level against the private `Otsu`: 0 differ.
- **One limit of the replay:** each read is answered alone, and the tracker still sees the entry's
  reads. The build showed this matters a great deal (task 2).
- **The stop did not hold, on its letter.**
  - From 34.5 to 44.5 s, the local unit was under 40 ms on 20 of 21 reads. The 21st, at 44.5 s,
    was exactly 40.0 ms.
  - From 30 to 46.2 s, 3 of 33 reads settled differently: 30.0, 32.0 and 42.5 s.

  So task 2 built. Read as a trend, the replay pointed the other way. From 32.5 to 44.0 s the
  local cut's unit was **lower** than the entry's on 16 of 24 reads, the same on 7 and higher on
  1. It ran 17.5 to 27.5 ms, against 17.5 to 35 ms at entry.
- **The pieces.** The reads from 34.5 to 44.5 s held 464 marks under 40 ms at entry, 102
  distinct ones. All were 10 to 35 ms: 22 at 10 ms, 79 at 15, 129 at 20, 92 at 25, 64 at 30 and
  78 at 35. The local cut:
  - left 390 standing alone;
  - joined 48 to a neighbor: 30 to the one after, 15 to the one before and 3 on both sides;
  - removed 26.

  Most of the pieces that stood sit in blocks whose 3 s span fell back to the whole window's cut.
- **Cost:** one local-cut `Measure` over a 2400-hop window took a median 0.25 ms on this machine
  (0.241 to 0.31 over 20 passes). The entry's took 0.045 ms (0.043 to 0.052). The stream reads
  every 500 ms.

**Task 2, built and judged.** Built at `63ce84bf` and taken out at `1f741112`.
- The change went into `CwUnitEstimator.cs` only, as section 6 states it:
  - `LocalCuts` takes a cut per 0.5 s block, falling back to the whole cut where a span's
    class means stand under 12 dB apart.
  - `Runs` takes a cut per hop, with the hysteresis state carried across blocks.
  - `OtsuSplit` returns Otsu's level and both class means. `Otsu` is now `OtsuSplit(db).Cut`,
    unchanged in value.
  - `Elements` fills every hop with the one whole cut, so it is unchanged for `MeasureGaps` and
    `MeasureCharacterGap`.
  - The remark cites unit 436's short heaps, unit 437's halving on a clean window, and why one
    cut chops a mark when the level moves.
  - The class remark now says there are two constants not measured from the audio,
    `HysteresisDb` and `CutSpanSeconds` 3.0. I also changed `HysteresisDb`'s own remark, which
    called it "the one constant", so the file does not contradict itself.
- **`TheUnitIsMeasuredNotSearchedTests` under the build: 4 of 5, as at entry.**
  - `ItRecoversASpeedItWasNeverTold` stayed green. Only one figure moved: 25 WPM at noise 0.08,
    from 25.3 to 24.0.
  - The plateau red was unchanged at 165, 130, 118, 117 and 116 marks.

The four tests:

| Test | Result | Numbers |
|---|---|---|
| 1. Total keyed edits do not rise | **pass** | 165 to 163 over 565. 17:37 19 to 18 over 25, `CQ CQ CQ DEWTEETEEERED ETTTB 7E E I`. Added 17 to 14, single-element 8 to 6 |
| 2. No named floor breaks | **fail**, 5 of 13 | 013347 57 to 54, 134712 11 to 10, 003758 43 to 38, 031838 40 to 36, 032129 65 to 63. 004507 49, 031948 31, 032012 43, 012403 19 and 173723 46 held. 031905 rose 36 to 38, 032050 44 to 45, 032113 47 to 49. R73 not examined, because tests 3 and 4 fail either way |
| 3. Adjudicated readings unchanged | **fail**; the adjudicated floor 11 of 13 | See below |
| 4. No capture row's above-bar count falls | **fail**; the captures test fails 16 of 51 | See below |

Test 3, the three adjudicated readings, quoted before and after:
- `013347` VA3VRR: `…T E HA E WVRR VA3VRRT` became `…T E HA EWVRR VA3■R`. **Lost.**
- `003758` MP/4 QNIK: `…EAND E A ET EEEETMP/4 QNIKK` became `…EANDE AA4MP/4 QNIKK`. This moved
  onto its own adjudicated text, which test 3 allows.
- `012403` DE KD0UN KD0UN K: `DEQ 6Q Q DE KD0UN KD0UN K`, identical.
- Outside the three, the adjudicated floor also failed on `031838`. It read
  `A 3, AT3 M, E2TT TTTTT , AND ■T TTT TEAH A MEAN TOF 2 TT` and now reads
  `A 3, 3, 2T 2 ■ AN AM ■ WIT TTTT A MEA TT TTTTTO TTTTT`, with no `, AND`.

Test 4, every row that fell (entry to change):
- Characters, 16 rows: 013347 57 to 54, 134712 11 to 10, 003758 43 to 38, 031838 40 to 36,
  032129 65 to 63, 001831 43 to 39, 001952 46 to 42, 002016 34 to 33, 012823 23 to 18, 012922 43
  to 42, 013303 44 to 41, 013520 55 to 53, 013637 60 to 59, 021629 27 to 26, 021825 19 to 15,
  003919 25 to 24.
- Elements alone: 003126 131 to 130, 021410 88 to 84.
- 9 rows gained and 24 were unchanged. All 51 rows, before and after, are in
  `.run-unit/unit438-rows-table.txt`.

Three tests failed, so the variant was not built. The change came back out in the next commit,
and src is identical to entry. P48 gained this unit's measurement. 7.4 is not ticked.

**Task 3, the exit round.**

| Check | Result | Time |
|---|---|---|
| Build | 0 errors | 14 s |
| Engine carry-forward | 178 of 178 | 375 s |
| App carry-forward | 278 of 278 | 152 s |
| Captures | 51 of 51 | 120 s |
| Adjudicated | 13 of 13 | 29 s |
| Keyed floors | 13 of 13 | 61 s |
| Keyed totals | 165 over 565; 17:37 19 over 25 | 19 s and 41 s |
| Strays | 3 of 3; 17 added, 8 single | 290 s |
| WhatTheOpeningHeardTests | 25 of 25 | 495 s |
| TheUnitIsMeasuredNotSearchedTests | 4 of 5, the same plateau red as entry, every figure identical | 2 s |

- **Every output is identical to entry apart from timings.** That covers captures, adjudicated,
  floors, keyed, baseline, strays and plateau, and the opening's text.
- **App carry-forward** was 278 of 278 this time. The dispatcher-loop loss did not recur, and app
  code is untouched.
- **WhatTheOpeningHeardTests** went from 22 tests to 25, because of the new printer's three
  cases.
- **The type touched is `CwUnitEstimator`.** No test type carries its name. Its direct test is
  `TheUnitIsMeasuredNotSearchedTests`, run above.
- **Nothing green at entry is red at exit** (HM-DEC-165).
- **Diffs:**
  - `git diff` of the eleven transmit files against 7e209cb4 printed nothing.
  - `git diff` of CwToneTracker.cs, CwToneSurvey.cs, CwDecoder.cs and CwProbabilisticStream.cs
    against entry 755373d2 printed nothing.
  - All of src and data against 755373d2 printed nothing.

## 2. What the owner should expect

**Nothing changes at the radio. The first minute on a new frequency still comes apart.** But for
the first time, a change made the opening of the 7.052 session read at the sender's speed.
- **What I changed:** the decoder decided key-down against key-up at a local level every half
  second, instead of one level for the whole 12 seconds.
- **The opening under that change:** from 31.5 to 44.5 s, every read measured a 45 to 67.5 ms
  dot, 50 ms on most reads. That is about 24 WPM, against the sender's roughly 22 WPM and 55 ms dot.
- **Its text:** `EANQNID  EAN■IK`, the same text the decoder gives for that audio cold. Before
  the change it read `UIEH EE E E T I NIEEE E  E ET N ■IK`.

It came back out:
- It cost 16 other recordings characters;
- it broke 5 named floors;
- it lost the adjudicated `VA3VRR` on 2026-08-17 013347.

It is also not clear the new level did the work alone. Under the change the tracker never made
its wrong move to 525 Hz at 30.5 s. When I replayed the new level at the old pitch, it did not
lengthen the dot. So what is still in the way is the tracker's move, which R76 holds behind 4.1,
together with how short the marks read at the wrong pitch.

Two things may look wrong but are not:
- `WhatTheOpeningHeardTests` now has 25 tests, not 22. The three new ones are one printer and
  decide nothing.
- That type took 495 s at exit, closer to its 600 s timeout than before.

## 3. What you should see

**The opening, text and named count:**

| Stretch | Before, and at exit | Under the built change |
|---|---|---|
| stream 30 to 46.2 s | `UIEH EE E E T I  NIEEE E  E ET N ■IK`, 22 named | `EANQNID  EAN■IK`, 12 named |
| 003901 cold | `EII E T NHHK`, 9 named | `EII E T NXNIK`, 10 named |
| 003919 cold | `EITEETNXNIK  EANQNID  EANQNIK`, 25 named | `E ANETNXNIK  EANQNID  EANQNIK`, 24 named |

**No visible change.** The change was taken back out, and at exit the decoder is identical to
entry.

**Task 1's per-read rows, the stream, offline replay.** The unit is in ms, then WPM and where the
speed came from. Cuts are in dB. Every row is in `.run-unit/unit438-trace.txt`.

| s | mix Hz | whole cut | block cuts | fell back | entry marks, unit, speed | local marks, unit, speed | entry settles | local settles |
|---|---|---|---|---|---|---|---|---|
| 30.0 | 600 | -33.4 | -34.4 to -31.1 | 13 of 24 | 20, 85.0, 14.1 est | 21, 52.5, 22.9 est | `  ` | ` ` |
| 31.0 | 525 | -34.6 | -34.6 to -31.1 | 11 | 25, 67.5, 17.8 est | 24, 87.5, 13.7 est | | |
| 31.5 | 525 | -35.3 | -41.3 to -30.9 | 8 | 43, 40.0, 30.0 est | 44, 45.0, 26.7 est | | |
| 32.0 | 525 | -36.3 | -41.7 to -31.1 | 8 | 65, 30.0, 40.0 est | 58, 27.5, 43.6 grid | ` UIEH` | `■` |
| 32.5 to 34.0 | 525 | -37.2 to -39.0 | -42.8 to -31.1 | 8 | 69 to 76, 22.5 to 27.5, 43.6 to 53.3 grid | 69 to 82, 17.5 to 25, 48 to 68.6 grid | `EE`, `E`, `E T`, `I` | the same |
| 34.5 to 35.5 | 525 | -39.5 to -40.6 | -44.8 to -31.2 | 8 | 70 to 79, 25 to 27.5, 43.6 to 48 grid | 81 to 86, 17.5, 68.6 grid | `NI`, `EEE` | the same |
| 36.0 to 41.0 | 525, then 625 at 36.5 | -41.0 to -41.8 | -44.8 to -40.6 | 13 to 15 | 67 to 79, 17.5 to 25, 48 to 68.6 grid | 78 to 90, 17.5 to 20, 60 to 68.6 grid | nothing | nothing |
| 41.5 | 625 | -40.8 | -44.8 to -33.5 | 12 | 76, 27.5, 43.6 grid | 80, 22.5, 53.3 grid | `E  E ET` | the same |
| 42.0 | 625 | -40.5 | -44.8 to -33.3 | 12 | 75, 30.0, 40.0 est | 82, 25.0, 48.0 grid | `N` | `N` |
| 42.5 | 625 | -40.2 | -44.8 to -33.3 | 12 | 78, 32.5, 36.9 est | 85, 22.5, 53.3 grid | | `TT` |
| 43.0 to 44.0 | 625 | -38.5 to -39.8 | -44.0 to -33.3 | 12 | 75 to 81, 30 to 35, 34.3 to 40 est | 80 to 82, 22.5 to 27.5, 43.6 to 53.3 grid | | |
| 44.5 | 625 | -38.0 | -41.7 to -33.3 | 12 | 78, 32.5, 36.9 est | 72, 40.0, 30.0 est | | |
| 45.0 to 46.0 | 575 | -36.7 to -37.2 | -37.2 to -33.3 | 18 | 60 to 63, 37.5 to 45, 26.7 to 32 est | 58 to 59, 42.5 to 45, 26.7 to 28.2 est | | |

- **The locked stretch** (stream 90 to 106.2 s, `004108` 13.8 to 30 s): identical both ways on all
  33 reads. The unit was 55 ms at 21.8 WPM, and 0 blocks fell back.
- **Cold:** `003901` settled differently on 5 of 55 reads, at 24.5, 25.0, 25.5, 27.0 and 30.0 s. `003919`
  settled differently on 5 of 55, at 6.0, 6.5, 14.5, 22.5 and 30.0 s.

**Under the built change, the stream's own reads beside entry.** The build carries its own reads
forward, and the tracker sees them.

| s | entry mix, unit, speed | built mix, unit, speed | built settles |
|---|---|---|---|
| 31.0 | 525, 67.5, 17.8 est | 600, 52.5, 22.9 est | |
| 31.5 to 34.0 | 525, 22.5 to 40, 30 to 40 est then 38 grid | 600, 50.0, 24.0 est | `E` 32.0, `AN` 32.5, `Q` 33.5, `NI` 34.0 |
| 34.5 to 36.0 | 525, 17.5 to 27.5, 30 to 38 grid | 625, 50.0, 24.0 est | `D` 34.5 |
| 36.5 to 41.5 | 625, 17.5 to 27.5, 36 to 38 grid | 625, 47.5 to 67.5, 17.8 to 25.3 est | `E` 41.0, `A` 41.5 |
| 42.0 to 44.5 | 625, 30 to 35, 34.3 to 40 est | 625, 45 to 52.5, 22.9 to 26.7 est | `N` 42.0, `■` 43.0, `I` 43.5, `K` 44.0 |
| 45.0 to 46.0 | 575, 37.5 to 45, 26.7 to 32 est | 575, 35 to 40, 30 to 34.3 est | |

**The question, answered.** With the marks cut where they stand, the build's opening from 34.5 s
does read at about the sender's 22 WPM. Its unit was 45 to 67.5 ms, 50 ms on most reads, and 24
WPM, always from the estimator. It reads `EANQNID  EAN■IK`, which is what the same audio reads
cold. No key scores the opening, so whether that is the sender's text is not established here.
The offline replay at the entry's pitch did not lengthen the unit. So the build's reading came
with the tracker holding 600 Hz and never moving to 525, and cannot be credited to the cut alone.

**The pieces**, reads 34.5 to 44.5 s. There were 464 marks under 40 ms at entry, 102 distinct.
The local cut left 390 alone, joined 48 and removed 26. These are the ones on the first read,
34.5 s:

| at s | length ms | gap after ms | its block's cut dB | local |
|---|---|---|---|---|
| 25.040 | 25 | 35 | -34.4 | gone |
| 25.685 to 29.800 | 10 to 30, 10 of them | 10 to 95 | -39.5 to -40.6, fallback | each stands alone |
| 30.120, 30.155 | 20, 20 | 15, 15 | -41.5 | joined both sides, into one 235 ms mark |
| 31.115 | 30 | 35 | -42.9 | joined to the one after, 130 ms |
| 32.050 | 15 | 335 | -44.8 | stands alone |
| 32.725 | 20 | 30 | -43.8 | joined to the one after, 105 ms |
| 32.905, 33.075 | 25, 35 | 145, 500 | -43.8, -43.6 | stand alone |
| 33.905 | 35 | 40 | -43.6 | joined both sides, 595 ms |

Every piece on every read is in `.run-unit/unit438-trace.txt`, on the `piece |` lines.

## 4. What's blocking us

1. **`TheUnitIsMeasuredNotSearchedTests.TheFiveToEightDecibelPlateauHolds` is still red at HEAD,
   exactly as at entry,** after the build, and at exit: 1 dB 165 marks against 117. It is outside
   the carry-forward.
   - It was red at this unit's entry, so it does not count against HM-DEC-165.
   - **Proposed ruling:** park it as a standing red for whichever unit opens the hysteresis.
   - **Reasoning:** §9 parks `HysteresisDb`, and §12.5 says repair nothing.
   - **Rejected:** repairing it here.
   - Not blocking 7.4.
2. **The launcher-overlap ruling is still the owner's and still carried:** the launcher should end
   a redirected session before it launches the next.
   - There was no collision. Nothing was live at 11:26, and task 0's check held.
   - Interactive claude.exe 26364, started 2026-09-24, ran beside this session throughout. It
     started no test and committed nothing. I did not touch it.
   - Not blocking 7.4.
3. **The build's opening came with a different tracker path, so it cannot be credited to the cut
   alone.**
   - Under the build the tracker held 600 Hz to 34.0 s and moved to 625 at 34.5 s. It never made
     the move to 525 at 30.53 s that R75 and R76 hold.
   - The offline replay held the entry's pitch, and there the local cut did not lengthen the
     unit.
   - So the opening reading at 24 WPM needed the cut and the tracker's changed path together, and
     this unit cannot separate them without touching the tracker, which R76 forbids.
   - **Proposed ruling:** record that 7.4's opening reads as it does cold once the tracker does not
     move to 525. Hold the next 7.4 attempt for 4.7 unless a route is found that does not act
     through the tracker.
   - **Reasoning:** every route downstream of the pitch has now been built against 7.4, and each
     cost other recordings (HM-DEC-091): four follow rules (R75), the speed carry, the
     dit-against-dah rule, the window re-mix, and now the local cut. This one moved the opening
     furthest, and apparently by way of the tracker.
   - **Rejected:** keeping the cut for the opening, because it costs 16 capture rows, 5 floors and
     `VA3VRR`.
   - This is the author's or the owner's to rule, not this unit's. It is not blocking 7.4 as a
     criterion.

Two corrections to the pushed commit messages. P48 and this report carry the correct figures.
- Task 1's message says the local cut "stands higher than the whole window's". It stands higher
  only in the blocks before the splice. At the sender it stands lower, as section 3 shows.
- The same message gives the entry unit from 34.5 to 44.5 s as 25 to 45 ms and the local one as
  20 to 40. Those were short-mark medians read from the wrong column. The units were 17.5 to 35 ms
  at entry and 17.5 to 40 ms under the local cut.
