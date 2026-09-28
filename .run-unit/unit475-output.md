READ IN THIS ORDER.

A. Phase goal: Hamlet's CW receive meets CW_REQUIREMENTS.md, and under R88 the owner's ear at
   the radio judges it, not the corpus. The one number that moved: `ConfidentSwingDb`, from 20
   to 17. The row that moved it is 01:16:22: the owner heard code, and the meter refused it on
   swing alone at 18.6 dB (score 0.27, plain median 48 ms).
B. Step 11, criterion 11.4: read the owner's rows and name the gate that disagreed with his
   ear. The rows name the keying meter's swing bar. It is moved to 17 and its remark says why.
   A synthetic window with the refused row's figures was red at 20 and is green at 17. The
   survey's hysteresis is described and not moved. Neither copy of `PHASE_PLAN.md` has any
   step 11 criterion lines, so nothing could be ticked: not 11.1 to 11.3 from unit 474, and
   not 11.4.
C. The rest. Section 4 raises 7 items, none blocking. Two bear on A and B. First, 17 is BELOW
   the tree's 17.7 dB empty recording, not above it as the instruction says, so the light may
   now light on an empty band. Second, the verdict row records the plain median, not the
   element median the gate tests. No recording was read.

UNIT:       475 - complete at task 3 of 3, none dropped - 2026-09-27 22:17
PHASE GOAL: Hamlet's CW receive meets CW_REQUIREMENTS.md. With the corpus banned, the owner's verdicts at the radio are what the detector is now tuned against.
UNIT GOAL:  Make the "I hear CW" light agree with the owner's ear more often by moving the one detector gate his verdict rows showed refusing a real station, and nothing else.
ADVANCED:   yes - step 11 criterion 4: the gate his rows named is moved, with a watched red-then-green fact. Whether it helped on the air is his next set of presses.
ADVANCES:   step 11 criterion 4
NUMBER:     ConfidentSwingDb 20 -> 17; owner rows refused at the old bar and accepted at the new: 1 of 7; recordings read: 0
DRIFT:      0 consecutive units without advance - was 0

## 1. What Claude did

**Complete: tasks 0 to 3 all done, none dropped.** Claude Code on QUIVERFULL, project Hamlet
(gate confirmed: SHACK_FACTS.md, CwProbabilisticDecoder.cs and CW_REQUIREMENTS.md present, no
CoreHMI.sln or MURC.sln), branch `main`. Each task was committed and pushed before the next one
started: 34262754 (task 0), 57f50c02 (task 1), 25b96e70 (task 2), and the task 3 commit that
carries this report.

**Task 0, the record and the entry round.**
- `## UNIT 475 - STEP 11` was appended to both copies of `PHASE_OUTCOME.md`, from the
  instruction's block.
- Both copies of `PHASE_STATUS.md` now name 475 and `CURRENT_STEP: 11`.
- Version 1.13.160 to 1.13.161.
- **11.1, 11.2 and 11.3 were not ticked.** Neither copy of `PHASE_PLAN.md` has a step 11
  section or any `11.k` line. The seed commit 62e272e6 is titled "step 11 criteria", but its
  plan diff only changes the R88 bullet and adds the line-C bullet. Unit 474's evidence is
  named in the outcome entry instead: light 5/5, strip 7/7, buttons 6/6, each count checked in
  474's saved exit outputs. No criteria were invented. See section 4.
- Entry round: build 0 errors. App carry-forward line: 274/278 on the first run. The four
  losses took 1 ms each, and the output mentions "dispatcher loop" 8 times. On the one re-run
  it was 278/278.

**Task 1, the swing bar.**
- `CwKeyingThresholds.ConfidentSwingDb` changed from 20 to 17 (`src/Hamlet.RadioEngine/Cw/CwKeyingMeter.cs`).
- The remark now leads with what 17 rests on:
  - the seven owner verdicts of 2026-09-28, all under AGC FAST;
  - 01:16:22 refused for 1.4 dB;
  - swing 17.5 to 21.8 across all seven rows;
  - what 17 gives up, named: it sits below the tree's 17.7 empty recording and inside the
    14.7 to 17.7 range of the eleven empty windows, which already cleared the score and the
    element range.
- Everything the old remark said about the 2026-08-20 evening and the tree's recordings is
  kept, under "HISTORY, KEPT".
- **Watched it fail first.** `TheOwnersRefusedStationIsKeyingTests`, a new file with no
  recording, builds a 600 Hz tone keyed in 48 ms dits at duty 0.27. Between dits it drops to
  18.6 dB below itself.
  - First red, at 20: my own guard failed, not the verdict. The window measured 19.6 dB, and I
    had set the guard to 17.5 to 19.5.
  - I set the guard to the band that matters, 17 up to just under 20, and ran it again. Red at
    20: `Listening at 675 Hz, score 0.31, element median 54 ms, swing 19.6 dB against 20.0`.
  - Green at 17: `Keying ... against 17.0, held False`.
- Build 0 errors.
- **Tests that pin the bar:**
  - No app test pins 20. The app's no-keying line reads the constant, and no test asserts on
    its text.
  - `TheSwingIsTheFigureThatHoldsTests` and `TheTwoPitchesTableTests` use it and read
    recordings, so neither was run. See section 4.

**Task 2, the survey's hysteresis: described, not moved.** Printed in section 4, item 4. The
full paragraph is at `.run-unit/unit475-survey.md`.

**Task 3, the exit round.**
- Build 0 errors, warnings as errors.
- App carry-forward line 278/278 on the first run.
- `TheOwnersRefusedStationIsKeyingTests` 1/1. It is the one touched type that reads no
  recording. Every other test type that drives `CwKeyingMeter` reads recordings, so none was
  run: `CwKeyingMeterTests`, `TheMeterRunsOnLiveAudioTests`,
  `TheKeyingWitnessSaysNothingImpossibleTests`, `TheCwBaselineTable`.
- `git diff 7e209cb4` over the eleven transmit files prints nothing.
- `git diff 62e272e6 -- src/Hamlet.RadioEngine/Cw`: one file, 20 insertions and 1 deletion.
  The only line changed that is not a doc comment is `ConfidentSwingDb = 20` to `17`.
- No file under `tests/fixtures` changed.

**No recording was read.** No floor was run, and no engine line, metric or keyed set.

**Decisions I made myself, all overrulable:**
1. **I used 17 exactly as the instruction ordered**, even though its reason for 17 is wrong
   about the 17.7 recording (section 4, item 1). The owner ruled R89 "write it", and choosing a
   different number was not mine to do.
2. **I did not edit the comment inside `Update`**, which still says the bar "is already
   twenty". The instruction limits the Cw diff to the constant and its remark. It is now stale
   (section 4, item 2).
3. **The test's window was built at 18.6 dB and measures 19.6 dB**, at 675 Hz rather than the
   600 Hz it was built at. That still lies between 17 and 20, which is the one thing the fact
   depends on. I did not tune the construction to measure exactly 18.6.
4. **The status file was written by `.run-unit/unit475-status.sh`**, because Python cannot run
   here.

**Mismatches, reported for the record:**
- **The owner's telemetry file was not read.** This session's file tools and search are
  confined to `C:\Source\HamLet`, and both refused
  `%AppData%\Hamlet\telemetry\2026-09-28.jsonl`. I did not try to get around that. The
  figures used are the instruction's table, **not checked against the file**.
- **An owner's empty `STOP` file is at the root**, as it was for unit 474. `run-phase.bat`
  reads it between units. This single seeded unit was finished and nothing was started after
  it. The file was left in place, unstaged.
- **`SESSION.lock`** was already held by the launcher (watched PID 56780), and this session did
  not take or release it.

## 2. What the owner should expect

**Rebuild, tune the same stations on 7.020 and 7.054 with the same AGC FAST and preamp 1, and
press again.** Only one gate changed: the keying meter now accepts a swing of 17 dB where it
wanted 20. A station like the 01:16:22 one should now light it. That station's figures were
score 0.27, a steady fist and a swing in the high teens. In the owner's words, it should now
read "I think I hear CW" where it said it didn't.

**What will look wrong but is not:**
- **The light may now come on with nothing there.** The tree's own empty-band windows swing up
  to 17.7 dB, and they already cleared the score and the element-length tests. If the light
  lights on a dead stretch, press "You're an idiot". That row shows whether the score gate is
  the next thing to move, or whether 17 was too low. Either answer comes from his rows.
- **The strip will look the same.** The survey still admits nothing where it admitted nothing
  before. Its gate was described and not moved, so "survey admitted nothing" and "mixing ...
  not measured" will keep appearing.
- **Rows that were already lit stay lit.** The 01:16:01 and 01:17:37 rows swung 18.4 and 17.5
  dB and were lit only because the meter was holding an earlier yes. Windows like those now pass
  the swing test on their own, so the light will hold less often and say keying directly more
  often. The screen looks no different.
- **Nothing about decoding changed.** The constant decides only what the meter says. The
  decoder does not read it.

## 3. What you should see

**The answer: `ConfidentSwingDb` is 17. The window built to the refused row's figures is called
keying at 17, and was not at 20.**

- `.run-unit/unit475-swing-red2.txt`: `Listening at 675 Hz, score 0.31, element median 54 ms,
  swing 19.6 dB against 20.0`, then `Assert.Equal() Failure ... Expected: Keying`.
- `.run-unit/unit475-swing-green.txt` and `-exit-TheOwnersRefusedStationIsKeyingTests.txt`:
  `Keying at 675 Hz ... against 17.0, held False`, 1/1.
- Owner rows refused at 20 and accepted at 17: **1 of 7**, the 01:16:22 row. The 01:14:56 row
  is still refused on its score of 0.02. The other five were already lit.
- Build: 0 errors, warnings as errors, at entry, after task 1 and at exit.
- App carry-forward line: 274 then 278/278 at entry (one re-run, dispatcher loop), 278/278 at
  exit.
- Diffs: the transmit files print nothing. `Cw` shows one constant and its remark.
- `PHASE_OUTCOME.md` (both copies) ends with `## UNIT 475 - STEP 11`. `PHASE_STATUS.md` (both
  copies) reads `CURRENT_STEP: 11` and `WORK_INSTRUCTION: 475 ...`. `Directory.Build.props`
  reads 1.13.161.

## 4. What's blocking us

Nothing here halts the unit. Seven items, the most consequential first.

1. **17 is below the 17.7 dB empty recording, not above it.** The instruction says 17 "stays
   above the two empty recordings ... and only just above the 17.7 one". 17 is 0.7 dB below
   17.7. It is also inside the 14.7 to 17.7 range of the eleven empty six-second windows from
   `cw-2026-08-20-014854` and `-014935`. The comment in `CwKeyingMeter.Update` records that
   those windows already clear the score and the element range. Any of them at 17 or more is
   now called keying. **Ruling wanted: keep 17 or use 18.** 18 still accepts the 01:16:22 row
   at 18.6 and stays above 17.7. It gives up the 17.5 row, which was only lit by the hold.
   18 was rejected in `Update`'s own comment as "fitting a constant to a fixture". That
   objection was about recordings, and R89 now sets the bar from the air. I kept 17 because
   the instruction names it and the owner ruled "write it".
2. **Two recording-reading tests now disagree with the bar. Neither was run, under R88.**
   `TheSwingIsTheFigureThatHoldsTests` asserts that the empty recordings' maximum, 17.7, is
   below `ConfidentSwingDb`. At 17 it will be red. `TheTwoPitchesTableTests` computes a
   verdict with the constant and prints it, so its table will shift. The comment inside
   `CwKeyingMeter.Update` still says the bar "is already twenty". It was left alone because the
   instruction limits the diff to the constant and its remark. **Ruling wanted:** when the
   corpus ban lifts, re-point the first test at the new basis or retire it (HM-DEC-103: list,
   never delete), and correct that comment.
3. **The verdict row records the plain median, not the element median the gate tests.**
   `CwHearingViewModel.cs:290` writes `meterMedianMs = meter.MedianMs`, the middle of every
   threshold crossing. The meter decides on `ElementMedianMs`, which only counts runs of 20 to
   500 ms. Two consequences:
   - **01:16:22 "failed only the swing" is an inference.** The row has no element median. A
     plain median of 48 makes an element median inside 25 to 250 ms likely, but not certain.
   - **The instruction asks section 4 to say that four agreed rows had medians of 4 to 9 ms,
     "which are not dits, so the meter's score gate lets noise through".** The rows do show
     plain medians of 4, 6, 8 and 9 ms (01:17:19, 01:17:37, 01:16:01, 01:16:28). But the source
     says a plain median that low is normal on a real station: `KeyingEnvelope.cs:292-296`
     measured the adjudicated `VA3VRR` at plain 4 ms with an element median of 88, and `N4L`
     at plain 3 with 55. **Those four rows are not evidence that the score gate lets noise
     through.**

   **Ruling wanted:** add `meterElementMedianMs` to the `owner_verdict` row so the next rows
   carry the figure the gate uses. Rejected: judging the score gate from the plain median,
   because the code says that number is not a key-down length.
4. **The survey's hysteresis admitted nothing on all seven rows. It is a gate on the same kind
   of level evidence, one stage earlier than the meter's swing.** From the source
   (`CwToneSurvey.cs:195, 564-565, 609 on`): `HysteresisDb = 3.0` sets a mark mask that turns
   on above, and off below, the two-means midpoint of each bin's own dB history, ±3 dB. So it
   asks for the bin's quiet and loud cluster means to sit **more than about 6 dB apart**. It is
   not a 3 dB swing requirement. The meter's 17 dB is a 10th-to-90th percentile spread of a
   1 ms linear envelope, so the two figures are not directly comparable. Even so, a station
   swinging 17 to 22 dB on the meter would have to lose two thirds of that contrast to fail a
   6 dB span. **Reading the source, the likelier refusals are the mark-duration gates after
   the mask:** 8 marks, dits of 25 to 200 ms, a separation of 4.0 and a ratio of 2.5 to 3.8.
   None of their figures is in the verdict row. If the owner presses "You're an idiot" while
   the meter says keying and the survey admits nothing, the next unit should first record which
   survey test refused, then move that one.
5. **`PHASE_PLAN.md` has no step 11 criteria.** Neither copy has a step 11 or step 10 section.
   Step 11 exists only as a `PHASE_STATUS.md` line marked "not started", even though 474
   delivered the light, the strip and the buttons. The seed commit's message says "step 11
   criteria", but its diff adds none. **Ruling wanted:** write step 11's criteria 11.1 to 11.4
   into the plan, so that 474's work (5/5, 7/7, 6/6) and this unit's 11.4 can be ticked and the
   step's state is not read as not started.
6. **The owner's rows were not read from the file.** This session is confined to the
   repository, and it refused `%AppData%\Hamlet\telemetry\2026-09-28.jsonl`. The table in the
   instruction was used as written. **Ruling wanted:** copy the day's `owner_verdict` rows into
   the tree for the next unit, or let the session read that folder.
7. **An owner's empty `STOP` file is at the root.** It was left in place, as unit 474 left it.
   The runner will end the next launch at the door and delete it, unless it is removed by hand
   first.
