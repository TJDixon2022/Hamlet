READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial on 3.6;
   4 not started; 5 the owner's; 6 partial on 6.5; 7 partial on 7.1, 7.2, 7.4, 7.6, 7.8.
B. Step 7, criterion 7.4: whether the estimator's dit-against-dah rule was kept, 3.2's four
   tests as numbers, and the opening - stream 30 to 46.2 s, 003901 and 003919 cold - before
   and after as text; and whether unit 435's first session had kept anything first.
   Answer: not kept. Tests 1 and 3 passed. Tests 2 and 4 failed, on 4 floors and on 14
   capture rows. The opening's text moved under the change, 22 named to 17, and went back
   with it. Unit 435's session kept nothing.
C. The rest, weighed against A and B. Section 4 raises 3 items. None stands in the way of 7.4
   as a criterion, but item 3 asks whether 7.4 should go on being seeded before step 4.

UNIT:       436 - complete at task 3 of 3, none dropped - 2026-09-25 09:52
PHASE GOAL: The CW decoder reads a real CQ call as the text that was sent, measured by edit distance against keys, and at the end the owner says so at the radio.
UNIT GOAL:  Find out whether the opening's speed halves because the estimator takes broken pieces of marks for dits, and fix it there if a rule can do that without costing any other recording.
ADVANCED:   no - the rule was built and judged, failed 3.2's tests 2 and 4, and was taken back out; 7.4 is not ticked
NUMBER:     stream 30 to 46.2 s: 22 named UIEH EE E E T I NIEEE E E ET N ■IK -> 22 named, the same at exit; under the change 17 named UIEH EE E E T I NI■ EA N ■IK; keyed 165 over 565 -> 165 at exit, 156 under the change; opening reads at a grid speed 15 of 23 -> 15 of 23 at exit, 8 of 19 under the change
DRIFT:      1 consecutive unit without advance  (was 0)

## 1. What Claude did

**Complete, task 3 of 3, none dropped.** The variant, task 2's drop candidate, was not built. Its
condition, a failure on exactly one row or one test, never arose: two tests failed, on many rows.
Machine QUIVERFULL, Hamlet confirmed (SHACK_FACTS.md and CwProbabilisticDecoder.cs present,
CoreHMI.sln and MURC.sln absent), branch main, entry HEAD d548a565. Every push succeeded.

**Task 0, the wait and the record** (`91d87c5e`).
- At 08:23:59 unit 435's other session was live: cmd 32764, started 08:07:39, running its keyed
  entry round and then WhatTheOpeningHeardTests. SESSION.lock and `.run-unit/watched.pid` named
  this session (cmd 48584).
- I waited, signalling nothing, and checked `ps` and the other session's process every 2.5
  minutes. By 08:37:48 it had ended, after about 14 minutes of waiting.
- It committed nothing. HEAD was still d548a565, src, tests and data were identical to HEAD, and
  7.4 was unticked, so section 6's last decision did not apply.
- Its own report, `.run-unit/unit435-stale-session-report.md`, says its entry DLL held two
  in-flight tests, so I did not reuse its numbers.
- 1.13.121 to 1.13.122. PHASE_STATUS.md names unit 436, *the estimator checks its dit against its
  dah*, and CURRENT_STEP 7. PHASE_OUTCOME.md has UNIT 436 - STEP 7. The launcher's five root files
  were committed as they stood.
- Section 5 matched the tree with no mismatch:
  - Measure is at CwUnitEstimator.cs 77 to 106, `(shortMark + shortGap) / 2`.
  - ShortClusterMedian is at 534 and TwoMeansOnLogs at 557. The long centroid sets only the
    boundary.
  - CwProbabilisticStream.cs 428 is the only caller in src. There is no speed carry at 431 to 435.
  - FastestWpm is 40.
- Entry round, on a fresh build:
  - Build: 0 errors.
  - Engine carry-forward: 178 of 178 in 374 s.
  - App carry-forward: 276 of 278 in 173 s. Both failures were the dispatcher-loop loss, and
    each type passed alone, 2 of 2 and 8 of 8.
  - Captures: 51 of 51 in 121 s, with 032113 at 47.
  - Adjudicated: 13 of 13. Keyed floors: 13 of 13.
  - Keyed totals: 165 over 565. 17:37: 19 over 25.
  - Strays: 17 added, 8 of them single-element, in 292 s.
  - WhatTheOpeningHeardTests: 15 of 15 in 183 s.
  - Every number is P48's.

**Task 1, the trace** (`e9411c58`). I added two printers to WhatTheOpeningHeardTests. They assert
nothing and write nothing, and they read the estimator's private cluster functions by reflection.
- `WhereTheDitMeetsTheDah` prints one row per read for the stream from 27.0 to 46.2 s, for the
  locked stretch (stream 90 to 106.2 s, which is 004108 13.8 to 30 s) and for 003919 cold.
- `WhereTheDahRuleFires` counts, over the 52 recordings in the 51 capture rows and the 23 keyed
  recordings, the reads where the ratio test fires.
- On the stream from 30 to 46.2 s, the rule fires on 14 of 33 reads, and on 13 of them its speed
  is inside 8 to 40. So the stop did not hold.
- The rule fires on 35 of the 52 recordings. On 23 of them it moves a speed the stream had
  already taken.

**Task 2, built and judged** (`4ffe7ef6`, taken out at `22b2b5c3`). The rule went in as section 6
states it.
- In Measure, after the existing not-ready checks, take TwoMeansOnLogs of the marks. If the long
  centroid is more than `BrokenDitRatio` 4.5 times the short one, return a unit and
  DitMarkMilliseconds of the long cluster's median over 3. The long cluster is the members above
  the same geometric-mean boundary, member n/2.
- **One reading I made for myself:** I kept ElementGapMilliseconds as the short gap median. The
  unit is the dah over 3 alone, not averaged with the gap, because section 6 says the reading
  *takes its unit as* the dah over 3.
- 4.5 and 3 were not tuned.

The four tests:

| Test | Result | Numbers |
|---|---|---|
| 1. Total keyed edits do not rise | **pass** | 165 to 156 over 565. 17:37 19 over 25 to 19 over 25. Added 17 to 16, single 8 to 8 |
| 2. No named floor breaks | **fail**, 4 of 13 | 031838 40 to 36, 003758 43 to 40, 031905 36 to 35, 031948 31 to 30. The other 9 held |
| 3. Adjudicated readings unchanged | **pass** | Output identical. VA3VRR 6 of 6, MP/4 QNIK 9 of 12, DE KD0UN KD0UN K 16 of 16, the same before and after |
| 4. No capture row's above-bar count falls | **fail**, 14 of 51 | See below |

Test 4's failing rows:
- Above-bar characters fell on 7 failing rows: 001831 43 to 38, 021825 19 to 15, 031838 40 to 36,
  013622 49 to 47, 012823 23 to 22, 021629 27 to 26, 031905 36 to 35.
- Above-bar characters also fell on 2 anchored rows, whose count floor is retired and which
  therefore passed: 003758 43 to 40 and 031948 31 to 30. Test 4 counts them as falls too.
- Above-bar elements fell on 9 rows: 013347 106 to 104, 004507 117 to 116, 003016 146 to 145,
  001952 103 to 102, 031838 91 to 90, 031905 108 to 106, 032050 105 to 100, 032113 102 to 100,
  021410 88 to 87.
- Rows that rose, for the record: 001952, 032113, 013402, 021410, 012748, 002016 and 032129 in
  above-bar characters.

R73 cannot cover the floor drops, because added letters fell by one. Two tests failed on many
rows, so the variant was not built, and the change came out in the next commit. P48 gained this
unit's measurement. 7.4 is not ticked.

**Task 3, the exit round.**
- Engine carry-forward: 178 of 178 in 375 s.
- App carry-forward: 276 of 278 in 168 s. Both failures were the dispatcher-loop loss, on
  TheChipSaysTheChosenModeTests, which passed alone 6 of 6.
- Captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13, keyed 1 of 1, baseline 2 of 2.
- Strays 3 of 3 in 295 s: 165 over 565, 17 added, 8 single.
- WhatTheOpeningHeardTests: 18 of 18 in 315 s.
- Every figure is identical to entry.
- TheUnitIsMeasuredNotSearchedTests, run as the estimator's own type, gave 4 of 5. The failure
  was already standing at HEAD (section 4 item 1).
- `git diff` of the eleven transmit files against 7e209cb4 printed nothing.
- `git diff` of CwToneTracker.cs, CwToneSurvey.cs, CwDecoder.cs and CwProbabilisticStream.cs
  against entry printed nothing. So did all of src and data.

## 2. What the owner should expect

**Nothing changes at the radio. The first minute on a new frequency still comes apart into E and
T, and the reason is now the pitch, not the estimator.** The estimator does halve the speed:
broken marks fill its short heap. Reading the unit off the dahs undoes that. Under the change the
opening's reads between 35.5 and 41.5 s sat at 18 to 22 WPM, the sender's speed, not the 30 to
38 the grid chose. The text still did not come out. Those reads' 12-second windows still held
audio mixed at 525 Hz, 100 Hz off the sender. The same rule cost 14 other recordings characters
or elements, so it came back out. What looks wrong but is not: `WhatTheOpeningHeardTests` now has
18 tests, not 15. The three new ones are printers and decide nothing.

## 3. What you should see

**No visible change.** The rule was taken back out, and at exit the decoder is byte-identical to
entry.

**The opening, before and after the change** (text, then named count):

| Stretch | Before, and at exit | Under the change |
|---|---|---|
| stream 30 to 46.2 s | `UIEH EE E E T I NIEEE E E ET N ■IK`, 22 named | `UIEH EE E E T I NI■ EA N ■IK`, 17 named, 2 placeholders |
| 003901 cold | `EII E T NHHK`, 9 named | `EII E T NHHK`, 9 named |
| 003919 cold | `EITEETNXNIK EANQNID EANQNIK`, 25 named | the same, 25 named |

The characters in the stream stretch settled at a grid speed on 15 of 23 before and 8 of 19
under the change.

**Per read, stream, entry beside the change.** Ratio is the long mark centroid over the short.
The mix is the same in both.

| s | ratio | short ms | long ms | short gap ms | entry wpm, from | change wpm, from | mix Hz |
|---|---|---|---|---|---|---|---|
| 32.0 | 4.14 | 21.9 | 90.5 | 40 | 40.0 stream | 40.0 stream | 525 |
| 32.5 | 4.82 | 19.0 | 91.4 | 35 | 43.6 grid | 45.0 grid | 525 |
| 33.0 to 35.0 | 3.77 to 4.37 | 23 to 36 | 102 to 148 | 15 to 25 | 43.6 to 53.3 grid | the same | 525 |
| 35.5 | 4.90 | 34.1 | 167.2 | 15 | 43.6 grid | 21.2 stream | 525 |
| 36.0 | 5.89 | 27.5 | 161.9 | 10 | 68.6 grid | 21.2 stream | 525 |
| 36.5 to 41.0 | 5.42 to 6.23 | 27 to 31 | 162 to 187 | 10 to 15 | 48.0 to 68.6 grid | 17.6 to 21.8 stream | 625 |
| 41.5 | 4.62 | 32.4 | 149.6 | 20 | 43.6 grid | 21.8 stream | 625 |
| 42.0 to 46.0 | 2.77 to 4.06 | 32 to 39 | 105 to 157 | 20 to 45 | 26.7 to 40.0 stream | the same | 625, then 575 |

The locked stretch has a ratio of 3.12 to 3.43, never fires, and holds 21.8 WPM, a 55 ms unit, in
both. 003919 cold fires on 4 reads (3.0 to 4.5 s), which go from 80 WPM on the grid to 30.0 to
34.3 WPM, and its text does not move.

**Unit 429's hypothesis, answered: no.** A held speed was not enough.
- From 36.5 s the mix stands at the sender's 625 Hz, and the change holds 17.6 to 21.8 WPM.
- The window ratio stays at 0.53 to 0.96 through 41.0 s, at entry and under the change alike.
  Each read's 12 s window still reaches back past the move into audio mixed at 525 Hz.
- The long-cluster medians there are 165 to 205 ms, against 160 to 165 ms on the locked stretch.
  So the rule's 17.6 to 18.9 WPM is itself slow of the sender's 22 to 24.
- In this opening the pitch is what stands in the way of the text, and R76 holds that behind 4.1.

## 4. What's blocking us

1. **TheUnitIsMeasuredNotSearchedTests.TheFiveToEightDecibelPlateauHolds fails at HEAD. It is
   not on the carry-forward list.**
   - Its message: *one decibel gave 165 against 117, so the chatter a two-level trigger exists to
     remove is no longer there.*
   - src and data are identical to this unit's entry, and the test file has not changed since
     76b295cc. So it was failing before this unit, and nothing here caused it.
   - **Proposed ruling:** park it as a standing red outside the carry-forward, for whichever unit
     next opens the hysteresis. It sits in §9's parked list: the hysteresis depth and
     ShortestRunHops.
   - **Reasoning:** repairing it is §12.6 work past the unit's own scope.
   - **Rejected:** fixing it here, because the instruction says repair nothing.
   - Not blocking 7.4.
2. **The launcher started this session while unit 435's first session was still running**, the
   same collision unit 435's second session reported.
   - This unit waited 14 minutes and touched nothing of the other session's.
   - **The ruling asked for is the owner's and is carried here, not made:** the launcher should end
     a redirected session before it launches the next one.
   - **Reasoning:** two sessions shared one tree and one test DLL. Unit 435's entry numbers were
     taken against another session's in-flight test source.
   - **Rejected:** having a session stop another, which §10 forbids.
   - Not blocking 7.4, but it undermines any run that overlaps another.
3. **Whether 7.4 should keep being seeded before step 4.**
   - Every route recorded against 7.4 has now failed or been closed: four mixdown follow rules
     (R75), unit 435's speed carry (built nothing) and this estimator rule (fails tests 2 and 4).
   - This unit measured that, even with the speed right, the opening's text is broken by the pitch
     in the window, which R76 keeps behind 4.1.
   - **Proposed ruling:** hold 7.4 for 4.7, which already reopens it when a tracker change is
     kept, and seed no further 7.4 unit until then.
   - **Reasoning:** the remaining cause is measured and is outside what 7.4 may touch.
   - **Rejected:** a fifth downstream rule, because nothing left downstream of the pitch would
     change the text in that window.
   - The author's or the owner's to rule, not this unit's.
