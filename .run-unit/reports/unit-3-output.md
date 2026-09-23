```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Steps 0, 1 and 2 are ticked on every
   criterion; the outcome file holds 0 and 2 at not started and 1 at done,
   a layer mismatch reported here; step 3 is this unit's, at 3.1; steps 4
   and 5 are not started. After this unit step 3 is in progress with 3.1
   met, and 3.3 met with it.
B. The criteria, one line each, met or not:
   3.1 met - all 51 names run by type at HEAD and classified with numbers in
       docs/phase-cw/unit394-reds.md: 30 green, 21 red-open, none repaired,
       retired or unmeasured.
   3.2 not met, vacuous - no retirement was made, so nothing to quote and no
       docs/cw-retired-tests.txt; left open.
   3.3 met - no audio-reading test retired; every one of the 21 reds reads
       audio and is listed red-open with its number and what it asserts.
   3.4 not this unit's - the known-reds block is unchanged, decision 7.
   3.5 not ticked, the step's exit - at this unit's exit the floors are 37 of
       37 and 13 of 13, both lines green but for dispatcher-loop losses with
       no red on an assertion, and the synthetics red at both ends under R53.
C. The report last. Section 4 raises 29 items - unit 390's nine, unit 391's
   five, unit 392's four and unit 393's five carried, and this unit's own
   six - and none is in the way of a criterion in B. Task 3, the clean
   synthetics measured four ways, was dropped on its own clock rule and is
   the next unit's first measurement.
```

```
UNIT:       394 - complete at task 4 of 5, tasks 0, 1, 2 and 4, task 3 dropped on its clock rule - 2026-09-22 23:00
PHASE GOAL: get the CW decoder reading again from the last code that read, keep it from breaking silently, clear the inherited CW reds one at a time under R49, judge the August rework on numbers, and end with Tim hearing it read at the radio
UNIT GOAL:  run every one of the 51 inherited CW reds against the restored decoder, one type per invocation, and put a classification and a number beside every name, so step 3 repairs from a list and not a guess
ADVANCED:   yes - step 3 criterion 3.1 is ticked on measured runs, and 3.3 with it
NUMBER:     of the 51 inherited CW names, classified at HEAD: 0 -> 51; green 30, red-repaired 0, red-retired 0, red-open 21, unmeasured 0; engine line 375 s -> 371 s of 480
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 5. I ran tasks 0, 1, 2 and 4. Task 3 was dropped under its own clock
rule: task 2 ended at 22:44, 41 minutes into the hour that began at 22:03, leaving 19 minutes
against the rule's 25. Task 3 was the named drop candidate.** QUIVERFULL, `C:\Source\HamLet`,
Hamlet confirmed by the gate's four checks, branch `main`.

**Task 0, the record and the entry round (`02c64ae2`).** I appended `## UNIT 394 - STEP 3` to
`PHASE_OUTCOME.md` with the decision block's fields. `PHASE_STATUS.md` read `CURRENT_STEP: 0` and
`WORK_INSTRUCTION: 393 - the list is green, and CW is on it`; I set them to 3 and
`394 - the pile is counted, name by name`. The version went from 1.13.80 to 1.13.81. The entry
round ran both lines of `docs\carry-forward-tests.txt` as its comment says, one build each, with a
status line before each: app 278 of 278 in 170 s; engine 176 of 176 in 375 s, of which 26 are CW
(176 less 150). The floors, one type per invocation: captures 37 of 37 in 97 s, with `001520`
and `013637` of the failing set green in it; adjudicated 13 of 13, 30 s of test time; clean
synthetics 0 of 2 in 5 s, the `■` placeholders R53 expects. The eleven transmit files printed
nothing against `7e209cb4`. **Slip:** my edit adding the `ENTRY` line to `PHASE_OUTCOME.md` failed
on an ambiguous match, so task 0's commit went without it. It rode in task 1's commit, and the
line says so.

**Task 1, the pile run by type (`c7fc1ecf`).** I ran thirteen invocations, one per compiled type,
each `--no-build` after task 0's engine-line build (decision 5), with `timeout 600`, the detailed
logger, and a status line naming the type and its ordinal. No run died before an assertion and
nothing was re-run. **`OneDecoderNotTwoTests` did not finish inside 600 s.** The whole-type run
printed 81 green and no red before `timeout` killed it at 601 s, and every case of
`ListeningAndFeedingReadTheSame` was green, the set's three among them. Under decision 5 I split it
by method and ran `TheBufferSizeChangesNothing` alone. That run printed 47 green and no red before
it too was killed. Six of its 53 cases are unmeasured at the cap, and none is in the set.
**Deviation, reported:** the first call, at `timeout 600` plus the status line, passed the
harness's 600 s foreground cap by about a second, and the harness moved it to the background. I
did not poll it; the notice of its end was the only read. I ran the split at `timeout 580` so it
stayed in the foreground. `ABlipDoesNotShiftEverythingAfterItTests` was not run, because it is
excluded from compilation, and I read it instead (decision 4).

**Task 2, the classification (`73b0bec0`).** `docs\phase-cw\unit394-reds.md` sections 1 to 4:
**30 green, 0 red-repaired, 0 red-retired, 21 red-open, 0 unmeasured.** No red has a wiring
cause. All 20 that ran fail on a decode result the console printed: characters, share or speed.
The one excluded file reads audio and asserts characters, so R49's second sentence forbids retiring
it. **No repair and no retirement was made, and `docs\cw-retired-tests.txt` was not created.**
3.1 and 3.3 are ticked in `PHASE_PLAN.md`; 3.2 is left open as vacuous.
`git diff --stat 02c64ae2 HEAD -- src` prints nothing.

**Task 4, the exit round (`af4ce62b`).** App line 277 of 278 in 172 s, with 1 lost to the
headless dispatcher loop. I re-ran it once: 275 of 278 in 169 s, with 3 lost the same way. Each
lost name was green in the other run, and nothing went red on an assertion. Engine line 176 of
176 in 371 s. Floors 37 of 37, 13 of 13, and synthetics 0 of 2 reading the same placeholders as
at entry. The transmit files printed nothing against `7e209cb4`, and `src` printed nothing since
`02c64ae2`. **There is no regression.** Section 5 of the doc.

**Author's decisions applied, all overrulable:** 1, step 3's entry taken as satisfied on step 2's
four ticks, with the outcome file's *not started* reported, not repaired; 2, the set is 51
distinct names run by type, and the known-reds block adds none; 3, the four classifications as
written, of which only green and red-open occurred; 4, the excluded file classified from its
source, red-open; 5, `--no-build` and `timeout 600` per type with a split by method on a timeout,
applied to `OneDecoderNotTwoTests` at 580 s as above; 6, not applied, because task 3 was dropped;
7, nothing on or off either line, and the known-reds block untouched; 8, the task 0 and task 4
floor runs cover every commit; 9, the timeouts as listed. **I made no self-ruling.** Every
repair: none. Every retirement: none. Every regression: none.

## 2. What the owner should expect

Nothing changes on the CW tab. This unit ran tests and changed nothing under `src`. The pile of
CW tests called *inherited* since August now has a result beside every one of its 51 names.
**30 of them are green on the decoder you heard read on 2026-08-25.** Green means each
assertion held, not that CW works. **21 are red on a decode result and wait their turn.** 14 of
those assert the characters read, 5 the share of a message, and 2 a speed. The largest group is
generated audio of `CQ DE W1AW K` that comes back as `■` placeholders or the wrong letters: the
two clean synthetics, the six `CwDisplacementFloorTests`, and the speed test on an 18 wpm signal.
Four receiver-tier recordings come back with the right tone and the wrong letters. **Nothing was
retired.** The one test that cannot compile decodes audio and compares text, and R49 keeps those.
Task 3 was dropped, so **where the two clean synthetics fail is still not measured.** The next
unit measures it first. What will look wrong but is not: the app line lost a few names to the
headless dispatcher loop at exit. That is the known lost run, and each lost name was green in the
other run.

## 3. What you should see

**The 51 names**, in the order of `docs\unit239-failing-set.txt`, prefix
`Hamlet.RadioEngine.Tests.Cw.` dropped, copied from `docs\phase-cw\unit394-reds.md` section 2:

| # | Name | Type | Class | Number | Reason |
|---|---|---|---|---|---|
| 1 | `ASubMinimumBlipInAGapChangesNothingAfterIt` | `ABlipDoesNotShiftEverythingAfterItTests` | red-open | not run: file excluded by `<Compile Remove>` | asserts **characters**: generates `CQ DE W1AW K` at 18 wpm and asserts the text read with a 10 ms blip equals the text read without; not retirable under R49's second sentence; see the note below the table |
| 2 | `ARecordingWithKeyingInItIsReadTests.WhereTheTrackerStartsDoesNotDecideThis(startHz: 500)` | `ARecordingWithKeyingInItIsReadTests` | green | passed | - |
| 3 | `...WhereTheTrackerStartsDoesNotDecideThis(startHz: 550)` | `ARecordingWithKeyingInItIsReadTests` | green | passed | - |
| 4 | `...WhereTheTrackerStartsDoesNotDecideThis(startHz: 600)` | `ARecordingWithKeyingInItIsReadTests` | green | passed | - |
| 5 | `CapturedSignalTests.TheSignalReadsAsStrongAsItIs(name: "cw-2026-08-17-134712")` | `CapturedSignalTests` | green | passed | - |
| 6 | `CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp(wordsPerMinute: 25, floor: 0.79)` | `CwAcquisitionWindowTests` | red-open | *25 words a minute tuned onto mid-transmission came back 0.75 of the message against a bar of 0.79* | asserts **share** of the message |
| 7 | `...AFastFistIsReadWithoutARunUp(wordsPerMinute: 28, floor: 0.79)` | `CwAcquisitionWindowTests` | green | bare 0.95 | - |
| 8 | `...AFastFistIsReadWithoutARunUp(wordsPerMinute: 30, floor: 0.79)` | `CwAcquisitionWindowTests` | green | bare 0.88 | - |
| 9 | `...AFastFistIsReadWithoutARunUp(wordsPerMinute: 35, floor: 0.78)` | `CwAcquisitionWindowTests` | green | bare 0.89 | - |
| 10 | `...TheSameFistWithARunUpDoesNot(wordsPerMinute: 25)` | `CwAcquisitionWindowTests` | green | run-up 0.95 | - |
| 11 | `...TheSameFistWithARunUpDoesNot(wordsPerMinute: 28)` | `CwAcquisitionWindowTests` | green | run-up 0.84 | - |
| 12 | `...TheSameFistWithARunUpDoesNot(wordsPerMinute: 30)` | `CwAcquisitionWindowTests` | green | run-up 0.88 | - |
| 13 | `...TheSlowEndReadsTheMessage(wordsPerMinute: 10, snrDb: 18)` | `CwAcquisitionWindowTests` | green | run-up 1.00 | - |
| 14 | `...TheSlowEndReadsTheMessage(wordsPerMinute: 10, snrDb: 3)` | `CwAcquisitionWindowTests` | green | run-up 0.98 | - |
| 15 | `...TheSlowEndReadsTheMessage(wordsPerMinute: 12, snrDb: 18)` | `CwAcquisitionWindowTests` | red-open | *12 words a minute at 18 dB came back 0.63 of the message*, bar 0.66 | asserts **share** of the message; the same speed at 6 dB and 3 dB is green at 0.96 and 1.00 |
| 16 | `...TheSlowEndReadsTheMessage(wordsPerMinute: 12, snrDb: 3)` | `CwAcquisitionWindowTests` | green | run-up 1.00 | - |
| 17 | `...TheSlowEndReadsTheMessage(wordsPerMinute: 12, snrDb: 6)` | `CwAcquisitionWindowTests` | green | run-up 0.96 | - |
| 18 | `CwDisplacementFloorTests.AStationElsewhereIsStillFound(toneHz: 400)` | `CwDisplacementFloorTests` | red-open | read `■ ■■ ■`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 19 | `...AStationElsewhereIsStillFound(toneHz: 500)` | `CwDisplacementFloorTests` | red-open | read `■ ■■ ■`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 20 | `...AStationElsewhereIsStillFound(toneHz: 750)` | `CwDisplacementFloorTests` | red-open | read `■ ■■ ■`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 21 | `...AStationElsewhereIsStillFound(toneHz: 875)` | `CwDisplacementFloorTests` | red-open | read `■ ■■ ■`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 22 | `CwDisplacementFloorTests.NothingIsRefusedBeforeAnythingIsBeingRead` | `CwDisplacementFloorTests` | red-open | read ending `VIVVV E KCTCGQQ N DEDE E WWAJ11AARW W N K`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 23 | `CwDisplacementFloorTests.TheTrackerDoesNotLeaveAStationForItsOwnImage` | `CwDisplacementFloorTests` | red-open | read `■ ■■ ■`, expected to end `CQ DE W1AW K` | asserts **characters** |
| 24 | `CwEmissionGateTests.NoSpeedIsNamedWithoutCharactersToNameItFrom` | `CwEmissionGateTests` | red-open | *real signal reported none wpm*; `Assert.NotNull` on the speed, then 14 to 24 | asserts **speed** on an 18 wpm generated signal; the noise half, no speed from noise, held |
| 25 | `CwFixtureTests.EveryRecordingGivesBackTheShareItShould(name: "clean-12wpm")` | `CwFixtureTests` | red-open | *clean-12wpm gave back 0 of 9, short of the 100% it has to manage* | asserts **share**; R53's synthetic |
| 26 | `...EveryRecordingGivesBackTheShareItShould(name: "clean-18wpm")` | `CwFixtureTests` | red-open | *clean-18wpm gave back 0 of 9, short of the 100% it has to manage* | asserts **share**; R53's synthetic |
| 27 | `...EveryRecordingGivesBackTheShareItShould(name: "fading-18wpm")` | `CwFixtureTests` | green | passed | - |
| 28 | `...EveryRecordingGivesBackTheShareItShould(name: "interference-18wpm")` | `CwFixtureTests` | green | passed | - |
| 29 | `...EveryRecordingGivesBackTheShareItShould(name: "noisy-18wpm")` | `CwFixtureTests` | green | passed | - |
| 30 | `...EveryRecordingGivesBackTheShareItShould(name: "prosigns-18wpm")` | `CwFixtureTests` | red-open | *prosigns-18wpm gave back 0 of 16, short of the 100% it has to manage* | asserts **share** |
| 31 | `CwFixtureTests.TheCleanRecordingsDecodeExactly(name: "clean-12wpm")` | `CwFixtureTests` | red-open | read `■ ■ ■ ■ ■  ■ ■ ■ ■■` against `CQ DE W1AW K` | asserts **characters**; R53, the floor synthetic, step 3's repair, not retirable |
| 32 | `CwFixtureTests.TheCleanRecordingsDecodeExactly(name: "clean-18wpm")` | `CwFixtureTests` | red-open | read `■ ■ ■  ■■■` against `CQ DE W1AW K` | asserts **characters**; R53, as above |
| 33 | `CwFixtureTests.TheProsignRecordingDecodesItsProsigns` | `CwFixtureTests` | red-open | `<BT>` not found in `■ ■ ■■ ■■■ ■■ ■ ■■■ ■■■ ■■ ■ ■■■ ■■■■■■ ■`··· | asserts **characters** |
| 34 | `CwLowDutyTests.AStationKeyedForAMomentReadsAsAStrongStation` | `CwLowDutyTests` | green | passed | - |
| 35 | `CwLowDutyTests.TheHeldFigureLetsGoWhenTheStationStops` | `CwLowDutyTests` | green | passed | - |
| 36 | `CwLowDutyTests.TheToneIsFoundWhereItActuallyIs` | `CwLowDutyTests` | green | passed | - |
| 37 | `CwRefiningRetuneTests.AHandoverToAnotherStationStillResets` | `CwRefiningRetuneTests` | green | passed | - |
| 38 | `CwRefiningRetuneTests.AMoveBeforeAnythingHasBeenReadIsAFollow` | `CwRefiningRetuneTests` | green | passed | - |
| 39 | `CwRefiningRetuneTests.TheSurveySettlingBetweenTwoBinsIsNotAStationChange` | `CwRefiningRetuneTests` | green | passed | - |
| 40 | `CwSurveyThresholdPinTests.TheToneInTheInterferenceCaptureIsStillFound` | `CwSurveyThresholdPinTests` | green | passed | - |
| 41 | `Fixtures.CwAdjudicationTests.ASpeedChangeInRealisticAudio` | `Fixtures.CwAdjudicationTests` | red-open | *no speed was ever named*; `Assert.NotEmpty` on the speeds | asserts **speed** across the two-station recording; also the known-reds block's line 158 |
| 42 | `Fixtures.CwReceiverFixtureTests.NothingIsEmittedDuringTheOperatorsOwnTransmission` | `Fixtures.CwReceiverFixtureTests` | red-open | *70 characters during the preamble, 134 in all* against 0; *own transmit measured: 9.1 s*, its over-3 s assertion held | asserts **characters** emitted inside the operator's own transmission, on `qsk-preamble.wav` |
| 43 | `Fixtures.CwReceiverFixtureTests.TheEasyTierIsReadWhole(name: "coverage-easy")` | `Fixtures.CwReceiverFixtureTests` | red-open | 5 characters unreadable, 37 not in the message, reads `VVEVSVVAW■11S■22E2H33E3S44E4H55NB6E6MZ77NO88T8MO99T9MO900■■SKRYNTTTTMEQUTOOA?TDDEENXITE/TTEENMO■00TKCCAAEARLLALL` against `1234567890QRZ?DE/N0CALL` | asserts **characters** |
| 44 | `...TheEasyTierIsReadWhole(name: "exchange-easy")` | `Fixtures.CwReceiverFixtureTests` | red-open | 3 unreadable, 21 not in the message, reads `VVEVSVVNKCECMZQQENCCTCMQQQ■■TNENMO■00TKCCAAEARLLALLTNENMOTTT00TKCCAAEARTEELAETEILNKKK` against `CQCQDEN0CALLN0CALLK` | asserts **characters** |
| 45 | `...TheEasyTierIsReadWhole(name: "tightfist-easy")` | `Fixtures.CwReceiverFixtureTests` | red-open | 1 unreadable, 3 not in the message (H, I, I), reads `VEVHVVTETEIESTSTTTDDEEETETEISTSTTTKKK■` against `TESTDETESTK` | asserts **characters** |
| 46 | `OneDecoderNotTwoTests.ListeningAndFeedingReadTheSame(name: "unadjudicated/cw-2026-08-23-001952")` | `OneDecoderNotTwoTests` | green | passed | - |
| 47 | `...ListeningAndFeedingReadTheSame(name: "unadjudicated/cw-2026-08-25-012922")` | `OneDecoderNotTwoTests` | green | passed | - |
| 48 | `...ListeningAndFeedingReadTheSame(name: "unadjudicated/cw-2026-08-28-005051")` | `OneDecoderNotTwoTests` | green | passed | - |
| 49 | `TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid(name: "unadjudicated/cw-2026-08-23-001520", ...)` | floor type | green | passed, task 0 | - |
| 50 | `TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid(name: "unadjudicated/cw-2026-08-25-013637", ...)` | floor type | green | passed, task 0 | - |
| 51 | `ThePitchCanBeHeldTests.UnlockingLetsTheTrackerSteerAgain` | `ThePitchCanBeHeldTests` | green | passed | - |

**By what the 21 red-open assert:** characters 14, share 5, speed 2. **#1 cannot run.** Its file
is excluded from compilation, and all three of its facts decode generated audio and compare text.

**The runs by type**, `--no-build`, `timeout 600` each:

| Type | File added | Cases in the type | Cases in the set | Green | Red | Lost | Wall time |
|---|---|---|---|---|---|---|---|
| `ABlipDoesNotShiftEverythingAfterItTests` | 2026-08-29 | 3 | 1 | - | - | - | not run, excluded from compilation |
| `ARecordingWithKeyingInItIsReadTests` | 2026-08-20 | 5 | 3 | 5 | 0 | 0 | 14 s |
| `CapturedSignalTests` | 2026-08-16 | 13 | 1 | 13 | 0 | 0 | 42 s |
| `CwAcquisitionWindowTests` | 2026-08-18 | 12 | 12 | 10 | 2 | 0 | 18 s |
| `CwDisplacementFloorTests` | 2026-08-18 | 6 | 6 | 0 | 6 | 0 | 13 s |
| `CwEmissionGateTests` | 2026-08-16 | 8 | 1 | 7 | 1 | 0 | 8 s |
| `CwFixtureTests` | 2026-08-14 | 23 | 9 | 14 | 9 | 0 | 14 s |
| `CwLowDutyTests` | 2026-08-16 | 4 | 3 | 4 | 0 | 0 | 21 s |
| `CwRefiningRetuneTests` | 2026-08-18 | 3 | 3 | 3 | 0 | 0 | 6 s |
| `CwSurveyThresholdPinTests` | 2026-08-17 | 3 | 1 | 3 | 0 | 0 | 7 s |
| `Fixtures.CwAdjudicationTests` | 2026-08-17 | 11 | 1 | 10 | 1 | 0 | 13 s |
| `Fixtures.CwReceiverFixtureTests` | 2026-08-17 | 27 | 4 | 23 | 4 | 0 | 14 s |
| `OneDecoderNotTwoTests` | 2026-08-25 | 106 | 3 | 100 | 0 | 0 | 601 s, timed out; split 580 s, timed out |
| `TheCapturesThatDecodeKeepDecodingTests` | before 08-25 | 37 | 2 | 37 | 0 | 0 | 97 s, task 0 |
| `ThePitchCanBeHeldTests` | 2026-08-24 | 5 | 1 | 5 | 0 | 0 | 3 s |

`OneDecoderNotTwoTests`: 106 cases, 100 green, 0 red, 6 unmeasured at the cap, none in the set.
Outside the set, the thirteen types carry three more reds, all in `CwFixtureTests`:
`NothingTheDecoderWasSureOfIsWrong` on `fading-18wpm`, `noisy-18wpm` and `interference-18wpm`,
each asserting that no confident character is wrong. For example, *invented 'N', pattern [-.],
score 17.94*. Section 3 of the doc.

**Task 3's eight-row table:** not run, task 3 dropped.

**Carry-forward lines and floors, entry and exit:**

| Run | Entry, task 0 | Exit, task 4 |
|---|---|---|
| App line | 278 of 278 in 170 s | 277 of 278 in 172 s, 1 lost to the dispatcher loop; re-run once, 275 of 278 in 169 s, 3 lost the same way; each lost name green in the other run, no red on an assertion |
| Engine line, `timeout 480` | 176 of 176 in 375 s, 26 of them CW | 176 of 176 in 371 s |
| `TheCapturesThatDecodeKeepDecodingTests`, `timeout 900` | 37 of 37 in 97 s | 37 of 37, 92 s test time |
| `TheAdjudicatedReadingsKeepReadingTests`, `timeout 600` | 13 of 13, 30 s test time | 13 of 13, 30 s test time |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly`, `timeout 300` | 0 of 2 in 5 s, `■ ■ ■ ■ ■  ■ ■ ■ ■■` and `■ ■ ■  ■■■` | 0 of 2 in 7 s, the same two readings; R53 |
| Eleven transmit files against `7e209cb4` | nothing | nothing |

## 4. What's blocking us

**Nothing blocks a criterion. This unit's six items are findings and mismatches, and none needs
a ruling to proceed. Unit 393's five, unit 392's items 2 to 5, unit 391's items 2 to 6 and unit
390's nine are carried per HM-DEC-139, verbatim, below. None of them is this unit's to answer.
Unit 393's item 1, the Olivia demodulator type in `CpuMeasuredAlone`, is a self-ruling you may
overrule, carried as such.**

**1. Task 3 was dropped on its clock rule, so the two clean synthetics are unmeasured four ways.**
*A finding.* Task 2 ended at 22:44 with 19 minutes of the hour left. The four-way printer,
`Cw\CwCleanSyntheticsDiagnosisTests.cs`, was not written. The next unit makes this measurement
before any repair of #25, 26, 31 and 32, as the instruction says. Tonight's runs add one
indication. `CwDisplacementFloorTests` generates its audio in memory through `CwSignal.Generate`
and reads the same kind of placeholders, `■ ■■ ■`. So the committed `.wav` path alone is unlikely
to be the whole of it. That is a guess from one pattern, not a measurement.

**2. `ABlipDoesNotShiftEverythingAfterItTests` names `CwReferenceDecoder` only in doc prose.**
*A mismatch with section 5, decision 4 and unit 392's table.* The name occurs once, at line 30,
inside a `<para>` of the class remarks. The code never uses it, and prose in backticks does not
bind. So the name unit 392 quoted is not what keeps the file out of the build, or not alone.
The file's code names `CwSignal.Generate`, `CwSignalRequest`, `CwSignal.DefaultToneHz`,
`BufferedAudioSource.PumpAll`, `CwDecoder.Listen`, `Flush` and `Reading.Text`. Every one but
`Reading.Text` is used by a compiled engine test. `Reading.Text` appears elsewhere only in
`AMoveStartsTheDecoderFreshTests.cs`, which is itself excluded. I opened nothing under `src` and did
not build the file, so which name fails is not measured. The classification does not depend on
it: the file is red-open either way. **Option A:** the next step 3 unit re-includes the file in
one build to read the real error, and rewires it under R12 if the error is a renamed member.
**Option B:** leave it for step 4's verdict. **My recommendation:** A. It is one build, and it
may turn an uncompilable test into a runnable one.

**3. `OneDecoderNotTwoTests` does not fit in one 600 s call, and the harness caps at 600 s.**
*A finding against HM-DEC-155.* The whole type ran over 600 s. Its slower method alone ran over
580 s, with 47 of 53 green, 0 red and 6 unmeasured. A `timeout 600` inside a call that also
writes a status line is over the harness's cap by about a second. The first call was moved to
the background by the harness, not by me, and was not polled. A type this slow needs splitting
below the method, by case, if its last six cases are ever wanted. They are not in the set.

**4. Section 5 mismatches:**
- **The excluded files outside the set number twenty-one, not twenty.** Unit 392's table has 22
  rows, 21 engine and 1 app, and removing `ABlipDoesNotShiftEverythingAfterItTests` leaves 21.
  All 21 are listed in the doc's section 4.
- **`PHASE_STATUS.md` read `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 393`,** as stated, and is now
  3 and 394. Its `STEP: 0` and `STEP: 2` lines still read `not started`. They belong to the layer,
  and I did not edit them.
- **`PHASE_OUTCOME.md` holds step 0 and step 2 at `not started`** with every criterion `[x]`, and
  carries the paired `## UNIT 392` / `## UNIT 1` and `## UNIT 393` / `## UNIT 2` entries. As
  stated, reported, not edited. I committed it and `PHASE_STATUS.md` whole, as unit 393 did, so the
  layer's uncommitted lines rode in `02c64ae2`.
- **`docs\phase-cw\PHASE_PLAN.md` is a second copy that differs from the root `PHASE_PLAN.md`.**
  Units tick the root copy, and I did too. The instruction does not mention the second copy.
- **`SESSION.lock` is untracked at the root,** and the instruction does not list it. I left it as
  found.
- **`CLAUDE.md` §1's top row reads HM-DEC-167** at line 360. `PROJECT_STATUS.md` says HM-DEC-165
  because `tools/status.sh` writes it as a literal.
- **Held as stated:** HEAD `9fe8bab4`; 1.13.80 at line 1254; `PROJECT_STATUS.md` at unit 393,
  `COMPLETED`, `TASK 4 of 5`; the five root files and the three `tools\arbiter\` entries; the
  failing set's 51 lines in the fifteen rows as counted; `docs\carry-forward-tests.txt` at 918
  lines, line 7 with 65 terms, line 9 with 27 and `timeout 480`, the known-reds block at 153 with
  its CW lines at 158 and 159, `WHAT UNIT 393 ADDED` at 900; `ASpeedChangeInRealisticAudio` at
  line 41 of the set; 21 and 1 `<Compile Remove>`, 19 under `Cw\`; 6 fixture `.wav`s, the
  receiver tier, 49 unadjudicated captures; no `docs\cw-retired-tests.txt`; the Cw diff of 4
  files, 165 and 1; the transmit files silent; three preflight worktrees.

**5. The failing set is not all of its types' reds.** *A finding for step 3's scope.*
`CwFixtureTests.NothingTheDecoderWasSureOfIsWrong` is red on `fading-18wpm`, `noisy-18wpm` and
`interference-18wpm`, and none of the three is in `docs\unit239-failing-set.txt`. Criterion 3.1's
set does not reach them, so step 3 can close with them red unless the plan says otherwise.

**6. The headless dispatcher loop lost 4 names across the two exit app runs,** and none at entry.
*A finding, recorded and not chased (§6).* No name was lost twice.

**`validate-output.bat`:** not run. It asked for approval in earlier units. I checked this file
against its rules by hand: the ordering block and `UNIT:` above section 1; a `UNIT:` line with no
parentheses and none of `& | < > ^`; four sections in order with the canonical names; section 4
present.

**`git worktree list`:** the root and the three preflight trees, nothing else.
**`git diff --stat 02c64ae2 HEAD -- src` at the end:** prints nothing. **Push:** all four task
commits pushed without refusal. This report goes in a fifth commit.

### Asks still outstanding

**Carried per HM-DEC-139, verbatim: unit 393's five, unit 392's items 2 to 5, unit 391's items 2
to 6, and unit 390's nine. None is this unit's to answer.**

**Unit 393's five, verbatim:**

**1. Putting the CW guard on the engine line turned an Olivia test red, and I moved that test's
type into `CpuMeasuredAlone` to repair it.** *Self-ruling 1, author's, overrulable.*
`TheOliviaDemodulatorTests.TheMinusTenDecibelFixtureDecodes` asserts the demodulator uses under
20 s of CPU, measured as process CPU. Beside the CW cases it read 26.563 s with every character
correct. It was not re-run. With the type in the collection the list's own Olivia types already
use, it is green at 372 s and 373 s.

| | Ruling | For | Against |
|---|---|---|---|
| **A** | Keep it: the type runs alone | Same fix and same reason as its three Olivia neighbors; no ceiling moved | The line went from 300 s to 372 s between the two task 2 runs, most of it this |
| **B** | Put the CW guard types in a collection of their own instead | Olivia stays parallel | Changes wiring on the guard, and still costs time |
| **C** | Measure the demodulator's own thread CPU rather than the process's | The ceiling then means what it says | A test-shape change beyond wiring, and a larger edit |

**Industry standard:** C, since a per-thread measure is the honest one. **My recommendation:** A,
which is already in and matches the file's precedent. The instruction's section 5 check on
`CpuMeasuredAlone` covered `TheOliviaBlindSearchTests`, `TheOliviaDriftTests` and
`TheOliviaBelowTheNoiseTests` but not this type. So decision 2's claim that the CPU-ceilinged
Olivia names run after the parallel collections was true for three types and not for the fourth.

**2. Section 5 mismatches:**
- **`docs\carry-forward-tests.txt` ended at line 897,** not 898. The last line is unit 390's
  `TheRstIsYoursToCorrectTests` paragraph, as stated.
- **`TheOliviaMoveUpTests` calls `Lines()` at more sites than listed.** There are also calls at
  837, 1019, 1114 and 1253, and a direct `File.ReadAllLines` at 1230. `Lines()` is defined at
  1367.
- **`125941` is not an 08-25 case.** It is `cw-2026-08-26-125941`, so the guard's display-name
  match does not select it. Task 3's note that it "stays green" does not apply. All thirteen
  08-25 cases have floors above nought, and all thirteen went red.
- **`PHASE_STATUS.md` read `CURRENT_STEP: 0`,** as the instruction said, and is now 1. Its
  `STEP: 1` and `STEP: 2` lines still read `partial` and `not started`. Those are the layer's, and
  I did not edit them.
- **I committed `PHASE_OUTCOME.md` and `PHASE_STATUS.md` whole.** Task 0 asks the unit to write
  both, and git commits a file whole. So the layer's uncommitted `## UNIT 1 - STEP 1` entry and
  `HEARTBEAT` line went into `05b729eb` with this unit's changes. I edited neither.
  `RUN_LEDGER.md`, `WORK_INSTRUCTIONS.md` and the three `tools\arbiter\` entries are left as found
  and uncommitted. `PROJECT_STATUS.md` is written by `tools/status.sh` and stays uncommitted.
- **`PHASE_OUTCOME.md` holds step 0 at `not started`** with all four of its criteria `[x]`, and
  carries both `## UNIT 392 - STEP 1` and `## UNIT 1 - STEP 1`. As stated, reported, not edited.
- **`CLAUDE.md` §1's top row reads HM-DEC-167.** `PROJECT_STATUS.md` says HM-DEC-165, which
  `tools/status.sh` writes as a literal.
- **Held as stated:** HEAD `e7c036fc`; 1.13.79; `PROJECT_STATUS.md` at unit 392, `COMPLETED`,
  `TASK 4 of 6`; the five root files and the three `tools\arbiter\` entries; `TheOliviaRowsTests`
  lines 279, 306 and 674-675; `JsonlTelemetry` lines 30, 62, 146-159 and 188, and no `FileShare`;
  `TheOliviaExportSaysOliviaTests` at 582; line 7 with 65 terms, line 9 with 25 and `timeout
  480`; the guard table; the known-reds block; the thirteen 08-25 passes in
  `unit392-floors-1.txt`; `CpuMeasuredAlone` at 193-194 with `DisableParallelization = true`;
  the Cw diff of 4 files, 165 and 1; the transmit files silent; 21 and 1 `<Compile Remove>`;
  51 lines in the failing set; no `cw-retired-tests.txt`; three preflight worktrees.

**3. Two sibling helpers read the telemetry file the same way and are on neither line.**
`TheOliviaMoveUpTests.Lines()` and `TheOliviaExportSaysOliviaTests` at 582 both use
`File.ReadAllLines` on a file `JsonlTelemetry`'s writer may hold. They can fail the same way on a
busy machine. I did not edit them. The same one-method fix applies if either goes on a line.

**4. Six of the guard's 26 cases can never go red.** *A finding.* Five adjudicated readings carry
a `Retired` reason from your rulings of 2026-08-30 and are printed, not asserted, and
`TheShortfallIsPrintedRatherThanPapered` asserts no decode. They cost about a second. The 20
cases that do assert all refused the broken decoder.

**5. The headless dispatcher loop lost 4 names across the two task 1 app runs,** and none in
the entry or exit runs. *A finding, recorded and not chased (§6).* None was lost twice.

**Unit 392's items 2 to 5, verbatim:**

**2. Section 5 mismatches:**
- **HEAD at entry was `a1fd388c`,** one arbiter commit past the `4baf986c` the instruction names.
  `src` and `tests` were identical to `3d6a2c12` either way.
- **`CLAUDE.md` §1's top row reads HM-DEC-167.** That is neither `PROJECT_STATUS.md`'s
  HM-DEC-165, which `tools/status.sh` writes as a literal, nor the reload's `CPS-DEC-0167`. I did
  not edit `CLAUDE.md`.
- **`CwProbabilisticDecoder.Envelope` exists at `7e209cb4`.** The instruction lists it among the
  members to check. It was never a seam; the absent `Envelope` is a record in `CwReferenceDecoder`.
- **Keeping `CwElementPitch` could not make `ElementPitchLine` build.** Its input,
  `CwProbabilisticResult.Elements`, comes from HEAD's decoder (section 1, decision 4).
- **`tools/Hamlet.PitchRank` is in `Hamlet.sln` and uses seven HEAD-only names.** The
  instruction does not mention it.
- **The clean synthetics do not read the empty string at the restored decoder.** They read `■`
  placeholders.
- **Held as stated:** version 1.13.78; `PROJECT_STATUS.md` read unit 391; the 45 commits
  `1a84188e` down to `2068f868`; 20 files, 6684 insertions and 98 deletions; the ten `A` rows as
  named; the eleven transmit files identical; `tests/Hamlet.RadioEngine.Tests/Cw`, 34 files; the
  four 1.4 tests exist; `docs\unit239-failing-set.txt` has 51 lines;
  `docs\cw-retired-tests.txt` absent and not created; three preflight worktrees.

**3. Three of my own decisions that you may want to reverse.** *Author's, overrulable.* Each
is in section 1:
- `pitch-rank` taken out of the solution build.
- The queue counters return nought.
- The element-pitch line now says *not measured*.

The first two are one-line reversals. The third is step 4's to bring back with the rework's
elements if they earn their place.

**4. Task 5 dropped.** It was the named drop candidate, dropped on the clock rule. The failing
set at the restored decoder is unmeasured, and step 3 inherits it. Four previously
uncompilable tests now compile against thin seams: `NoCwDecodeInDigitalModeTests`,
`AHeldPitchDoesNotOutliveItsEvidenceTests`, `WhereAcquisitionPointsTests` and
`TheSpanRatioReachesTheSidecarTests`. None has been run.

**5. The headless dispatcher loop cost 8 lost runs across the four app-line invocations.** *A
finding.* The lost tests were never the same twice, and each passed in the other run. It is the
§6 lost run, recorded and not chased.

**Unit 391's items 2 to 6, verbatim:**

**2. Section 5 mismatches:**
- **The failing set has 2 cases of `EachStillProducesWhatItDid`, not six** (`001520`, `013637`).
  The two clean synthetics are there as stated.
- **The floor table has 37 rows.** The class's own remarks say "thirty-six here".
- **The Cw source changes are not "2026-08-28 to 08-31 and once on 09-03".** `git log` since
  08-24 shows 51 commits on every day from 08-24 to 08-31, and **four** on 09-03 (`43efc525`,
  `865e66d8`, `9c2a7f99`, `1a84188e`).
- **The green commit isn't between 08-25 and 08-28.** Nothing since 08-24 is green on all three.
- **`PHASE_PLAN.md` §6's fallback reads "since 2026-08-25";** task 3's reads "back to 2026-08-24".
- **HM-DEC-166 has no row in `CLAUDE.md` §1.** The top row before this unit was HM-DEC-165. I
  added HM-DEC-167's row only.
- **Held as stated:** `PROJECT_STATUS.md` read unit 390; `Directory.Build.props` read 1.13.77;
  every one of the 37 captures is on disk; the three tests exist by those names with 37, 13 and 2
  cases. The known-reds block carries two CW entries: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`
  and "the 51 CW cases in docs/unit239-failing-set.txt".

**3. The captures type takes 1995 s at HEAD, against 97 s at `7e209cb4`.** *An indication, one
run each; it bears on step 2.* Criterion 2.3 measures the guard against the engine line's
`timeout 480`. At today's speed the whole type can't go on that line. §6 already rules that
a test over 300 s never does.

**4. HM-DEC-155, bent and said so.** The harness caps a foreground call at 600 s, so the two
long captures runs and the probe ran in the background. I waited with one bounded loop per run.
The synthetics-only walk overlapped the second HEAD captures run in a second tree, which only
costs time: results were identical case for case against the first run. If the rule should bind
here as written, a type over 600 s needs a different runner.

**5. Three worktrees under `C:/Users/TimDi/preflight-trees/` were there before this session.**
One is at `07f0397a`, the commit named for 0.2. I didn't make them and didn't touch them. By the
instruction's own reasoning they're "a second tree the next unit can edit by mistake".

**6. `PHASE_PLAN.md` 0.1 to 0.4 are not ticked.** The instruction didn't ask me to; the judge
ticks them.

**Unit 390's queue as unit 391 carried it, verbatim:**

**Carried per HM-DEC-139, verbatim, from unit 390. None is CW, none is this phase's, and none is
this unit's to answer.**

**Nothing blocks. Items 1 to 3 are ticks you may want to reverse; the rest are findings.**

**1. 0.1 is ticked on this instruction's word, which overrules instruction 386 section 6 ruling 1 ("never ticked by a unit of this phase").** *A ruling request if you disagree.* The later instruction wins under `PHASE_PLAN.md` §6, and both were the author's and overrulable. The tick says what is true: no such commit exists (119 commits, 0 on a settings path). **Option A:** keep the tick as *met as a negative*. **Option B:** untick it and reword 0.1 so a completed negative meets it. **Industry standard:** B, because a criterion should be met by its own words. A was taken because the instruction said so.

**2. 9.2 and 9.4 are ticked "as built", and their words don't all match what was built.** *Findings; rewording is yours.*
- **9.2** says *Report, Confirm, the canned lines and the typed line are held - greyed with he is still sending - until his carrier drops*. As built, it would read: *the typed line is greyed with he is still sending, Report and Confirm are not offered and the canned lines give way to one note, until his hand-back or his carrier drops*.
- In 9.2 and 9.4, *replayed from the ... record* would read *replayed from a fixture built to that record's shape*. Your `2026-09-21.jsonl` is not on this machine.

**3. 10.3 is ticked with one case under the band's full height:** 327 × 178 at 1400 on PSK31 with the dial off 14.070, where the card also has to say *PSK31 lives at 14.070*. The instruction stated this case and asked for the tick. Untick it if the case matters to you.

**4. Tonight's ruling A and the 2026-09-07 read-only ruling are in no decision record.** *A mismatch against section 5, which says "HM-DEC- number: find it".* **The 2026-09-07 ruling has no HM-DEC number.** It lives only in `LogContactViewModel`'s remarks and the dialog's markup comment. Ruling A is now recorded in `PHASE_PLAN.md` 9.3, the code remarks and the commit. Neither is in `DECISIONS.md`, and I did not assign an id (§4.8).

**5. 10.6's ticked words (*one control reading Favorites*) now describe the drop-down this unit replaced.** Rewording is yours.

**6. `src\Hamlet.App\Controls\FavoritesDropDownControl.cs` should be deleted.** Nothing places it. It stays only because `Unit388TraceTests` names the type and `rm` is refused here. It is marked as off the window in its own remarks.

**7. Section 5 mismatches:**
- **7.5 was already `[x]`** although the instruction says it was never built. The tick was early (task 1's finding).
- **At 1100 × 780 the map is 246 × 134 in a band of 402**, not the 327 × 178 stated for "under 1400 wide". It is asserted as measured in `TheSunMapStandsWhereItWasLeftTests.AtTheSizeItOpensAtItKeepsTheMockupsSize`.
- **Unit 389's 9.2 finding holds:** 1 of 4 controls is greyed, and the hold ends at the hand-back.
- **The RST prefill and the words list:** `docs/RADIO_SHEET.md` quotes none of the strings changed tonight. I checked by running its test, which stayed green.

**8. The favorites row scrolls sideways with the bar hidden, and I haven't verified wheel scrolling.** *A finding.* At 1400 roughly one chip is in view beside the map's caption. If the hidden row doesn't respond to the wheel, a small arrow or a count is a one-unit follow-up.

**9. The `RULES_AT` split, the thirteenth unit running.** `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)` because `tools/status.sh` writes it as a literal, while `CLAUDE.md` holds `CPS-DEC-0165`. `tools\` is not mine to edit.

**`validate-output.bat`:** not run; it asked for approval in earlier units. Hand-checked against its six rules: `UNIT:` above section 1; four sections in order with exact names; no fifth; section 4 present; section 3 not empty; the ordering block above `UNIT:` with A, B, C and a count.

#### Asks still outstanding

Carried per HM-DEC-139. **Unit 389's item 2, 9.3's conflict with the 2026-09-07 ruling, is answered by your ruling A of 2026-09-22 and dropped.** The instruction's section 3 also answers 7.5 and the favorites; neither was on the queue.

**Unit 389's own, verbatim (items 1 and 3 to 7):**

**1. 9.2 falls short in two places, and both are on the send path, so they were left.** *A finding with numbers. Changing either needs a licence, because each changes whether a send control is enabled.*
- **Only 1 of 4 controls is greyed.** Report and Confirm are withheld by R1 mid-over, and the canned lines become a note.
- **The hold ends at his hand-back, not when his carrier drops.** Unit 385 measured that holding to the drop would refuse every answer for 9 s after a PSK31 `K`, and 22.94 to 38.23 s after an Olivia one.

Unit 385's claim was *"3 of the four controls held with one sentence and the fourth already withheld by R1"*. Measured, all four are held, three carry the sentence, and one is greyed.

**3. Neither replay is the record, and 9.4's is not what you got.** *A finding. Its remedy needs your file, or a licence to open the engine.* Your `2026-09-21.jsonl` is not on this machine. Unit 385's item 2 measured that `Psk31MessageSplitter` completes nothing from a garbled over, and that site is under `src\Hamlet.RadioEngine\Psk31\`.

**4. 10.3 at 1400 on PSK31 with the dial off 14.070 falls back to stage A.** *A finding. Whether it blocks 10.3 is a judging session's reading.* The card also draws *PSK31 lives at 14.070; you are at 14.074*, and at 401 px wide that no longer fits in 178 px. Any fix would cost one of three things you ruled on: the map's full height, the rig's width, or the strayed line. On 14.070 the same window stands at the left edge. The fallback is what keeps that line on the screen.

**5. The card's green word and the row's *sending* can disagree for a few seconds.** *A finding.* The card follows the hold, which ends at his hand-back. The row follows his carrier. For the 9 s (PSK31) to 38 s (Olivia) tail after a `K`, his row still says *sending* while his card no longer does.

**6. *Your turn?* now stands beside the unchanged *His turn, a guess*.** *Author's. It is one line either way if you want them to match.* R14 held me to the one word 9.4 names.

**7. `validate-output.bat` - see the last line of this section.**

**The older queue, carried by reference as units 385 to 389 did:** the whole of *"Unit 388's section 4, carried per HM-DEC-139, verbatim"* and *"Asks still outstanding - carried per HM-DEC-139, verbatim (unit 388's)"* in `output.md` at commit `cd18cc6c`, lines 270 to 506. That covers unit 388's seven; the sixty (unit 387's seven, unit 386's six, unit 385's eight, unit 383's ten); and the twenty-nine carried by reference from units 369 to 382. **None of them is answered tonight** except as follows. Unit 383's item 1 (4.3 partial under the strictest reading) and unit 386's item 6 (0.1 stays unticked) are overtaken by tonight's ticks, and items 1 and 3 above put both back to you.
