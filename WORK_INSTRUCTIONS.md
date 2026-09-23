# Work instruction 407 - the station still keying, and #45's tail on the air's terms

**Authored by the arbiter.** 3.6 is the only criterion the loop can move. Four of its eight reds
are still open: #15, #43, #44 and #45 of `docs\unit239-failing-set.txt`. Unit 406 found that
each of #15, #43 and #44 goes wrong when the tracker makes a held move off the one station. Its
H1 shape stopped those moves, but it also stopped five real captures from moving onto their
station. Unit 406 asked for a property that tells the two kinds of move apart. This unit looks
for one: **whether the pitch being read is still keying when the hold goes.** It uses no level.
#45 has never had a change of its own. This unit gives it one, in its own test's event. **Six
tasks, 0 to 5. Drop from the back under the clock rule in task 3.**

**Status.** Run `sh tools/status.sh` on the real clock after every commit, after every task,
and immediately before every `dotnet test`. **Write every file as UTF-8.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## 1. The rules that killed sessions

**HM-DEC-155.** Never run a whole suite. Run only this unit's names and
`docs\carry-forward-tests.txt`, and run the list as its top comment says. **Never background
a run and poll it.** Run one type per invocation, each with its own `timeout`. A run that dies
before any assertion is lost: re-run it once and count it neither way. A red on an assertion
is red and is never re-run.

**The report's `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>` or `^`
(PHASE_PLAN.md section 6). Write *tasks 0 to 5, none dropped* with commas.

## 2. The tool facts

- Apostrophes inside quoted heredocs break them.
- Doubled backslashes collapse.
- `;` is refused. `rm` is refused.
- Python cannot run here.
- For a multi-line commit message, use `-m` more than once.
- A bare `git worktree`, `git checkout` or `git show` is refused at the prompt.
- Put any multi-step command into `.run-unit\unit407-<name>.sh` and run it with `sh`. Every
  put-back goes through a script, as unit 406's did, and is followed by a build.

## 3. Asks still outstanding

Every ask carried from before this phase is parked in `docs\phase-cw\PARKED.md` under R54. The
arbiter reads that file as *parked to a later phase*. Unit 406's section 4 raised five items.
This is how the arbiter took each one. None is re-argued here:

1. **Does #42's green stand with `ObserveOwnTransmission` in the suspended arm?** This is not
   re-ruled here. Instruction 406's decision 2 reads, in its body: *"If task 1 finds that the
   change needs a line under `src`, it goes in `CwDecoder.cs` only. One example is a clock
   that follows the audio instead of `DateTime`. Such a change must leave every capture and
   adjudicated row identical."* The phrase *only if the trace needs an audio clock* comes from
   that instruction's decision-block summary. The body names the clock as one example. Unit
   406's line is in `CwDecoder.cs`, its trace needed it, and it left every row identical. So
   the record stays as unit 406 wrote it, `099b3a2a`, green. **This is a reading of
   instruction 406's own text. It is not a new ruling.** It is logged for Tim in section 12 C
   as the one place where a summary and its body disagree. He may put #42 back to red-open,
   and nothing in this unit depends on his answer.
2. **#45 has no route under the last instruction.** It gets an attack of its own here
   (decision 4, task 2). B2 stays refused.
3. **#15, #43 and #44: the reading level, and the separating property.** The property is to be
   looked for, in task 1. The 25 dB number of HM-DEC-127 (`FilterRejectionDb`,
   `CwToneTracker.cs` 212) is not changed. The reference level at line 1091 may be changed only
   under decision 3.
4. **The count rule.** Nothing new. Logged for Tim again in section 12 C.
5. **Two tracker readers that do not fully measure.**
   `TheGateHasItsOwnWindowNowTests.EveryWidthLeavesTheEmptyRecordingsSilent` stays out of the
   gate and is listed as *unmeasured*. HM-DEC-155 forbids backgrounding a run, and the tool's
   600 s ceiling cannot hold it. The type's two short methods are run.
   `WhatBandwidthTheDecoderListensThroughTests` is red on two cases at entry, outside the set,
   and is not this unit's to repair. Any change in its numbers is reported.

---

## 4. Why this unit exists

**The count today.** Steps 1, 2 and 4 are done. Step 5 is Tim's. **Step 3 has five of six
criteria ticked. 3.6 is open at 4 of eight with a verdict.** #24 and #41 went green by
`7e65aac4`, #6 by `775907b6`, and #42 by `099b3a2a`. Four reds are open, each at 0 of 3 under
the head's rule:

| red | test | number at HEAD | what 406 found |
|---|---|---|---|
| #15 | `CwAcquisitionWindowTests.TheSlowEndReadsTheMessage(12, 18)` | share 0.54, bar 0.66 | seed 15485863: a stale hold to 700 Hz for a 640 Hz sender. Seed 104729: the survey re-admits a 700 Hz image about 23 dB down while the sender sends the dahs of `0`, and the sender's own bins show no two-length structure |
| #43 | `TheEasyTierIsReadWhole` coverage-easy | 5 + 37 | four stale holds off 615 Hz, each on a survey that admits nothing |
| #44 | `TheEasyTierIsReadWhole` exchange-easy | 3 + 21 | a stale hold to 575 Hz at 11.5 s; held gaps of 15/1127/323 ms at `CwUnitEstimator.cs` 221 to 231 |
| #45 | `TheEasyTierIsReadWhole` tightfist-easy | 1 + 3 | no `Switch` call. On the settled transcript the only fault is one trailing placeholder, which the flush settles when the file ends |

**What is new here.**
- H1 and H2 both asked **the survey** where the keying is. This unit asks **the bank the
  tracker is reading** whether its own pitch is still keying, meaning marks with key-up between
  them, over the stretch the hold waited through.
  - A station that is still keying where the tracker listens has not left, whatever the
    survey admits.
  - A handover, or a tracker sitting on noise, has no keying at its own pitch.
  - That property can tell a stale hold off the station from a hold onto it, which unit 406
    could not. Task 1 measures whether it does, before anything is built.
  - It counts on/off keying and never compares levels (HM-DEC-095).
- #45 is the first red whose fault is where the file ends, not in the decoder. On the air the
  receiver never meets the end of a file. The test's event feeds the decoder the silence that
  would follow on the air before it flushes, and reads the settled transcript the tab keeps.
  Task 1 measures first whether the decoder itself then settles the placeholder away.

**This unit flips 3.6 only if all four go green.** No red is anywhere near three no-movement
attacks. The honest expected result is some reds green and 3.6 still open. If so, this will be
the third 3.6 unit in a row that did not flip the criterion. The report says so plainly.

```
PHASE GOAL: CW decodes again.
UNIT GOAL:  Trace whether the pitch being read is still keying when a held
            tracker move goes, and on that property alone stop the moves off
            the one station on #15 #43 #44 while keeping the moves onto it;
            give #45's event the air's trailing silence and the settled
            transcript under R12; keep only a red to green that loses no
            green and lowers no floor; write every row to reds-3.6.md.
ADVANCES:   step 3 criterion 6
DRIFT:      0
```

---

## 5. Verify this instruction against the tree

Check each item below. **Report a mismatch in section 4 of the report. Do not repair it.**

- HEAD is `9c8198b1`, or a descendant whose commits touch no file under `src` or `tests`.
- `git diff --stat 24d14e9c HEAD -- src tests docs/carry-forward-tests.txt` prints nothing.
- `src\Hamlet.RadioEngine\Cw\CwToneTracker.cs` is 1382 lines:
  - line 212 is `public const double FilterRejectionDb = 25;`;
  - lines 957 to 966 are H2's hold, with `backHere` and `Switch(_heldSwitchHz)`;
  - lines 1077 to 1080 are HM-DEC-127's floor against `_readingDb`;
  - line 1091 is `_readingDb = keyed.KeyedDb;`.
- `CwDecoder.cs` is 827 lines. `CwProbabilisticStream.cs` is 567 lines. `CwUnitEstimator.cs`
  is 549 lines.
- `tests\Hamlet.RadioEngine.Tests\Cw\Fixtures\CwReceiverFixtureTests.cs`:
  - line 175 is `TheEasyTierIsReadWhole`;
  - lines 207 and 208 are `source.PumpAll();` and `decoder.Flush();`;
  - line 260 is #42's method.
- `tests\Hamlet.RadioEngine.Tests\Cw\CwAcquisitionWindowTests.cs` line 134 is
  `TheSlowEndReadsTheMessage`, and its bar is `share >= 0.66`.
- The last line of `docs\unit239-failing-set.txt` names #15, #43, #44 and #45 as red-open.
- **Reload disagreement, to report and not repair:** `PROJECT_STATUS.md` RULES_AT says
  HM-DEC-165 of 2026-09-19. `CLAUDE.md` section 1 holds CPS-DEC-0167.

**Expected, and not a mismatch:**
- The dispatcher loop may lose an app-line name before any assertion. Re-run once. It counts
  neither way.
- `TheEightRedsTests`, `TheSixRedsTraceTests`, `TheTrackerSwitchTraceTests` and
  `TheCaptureOfTheTwentyThirdTests` exist, assert nothing, and are on neither line.
- `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and files under `.run-unit` are
  modified and uncommitted. That is the launcher's work. Commit none of `.run-unit`.

## 6. Rulings in force - do not re-argue

These come from PHASE_PLAN.md R47 to R55, criterion 3.6 and section 6, and from the arbiter's
decisions of units 402, 405 and 406. They are transcribed where they bind:

- **3.6, verbatim.** *Each of the eight reds open at unit 401's closing line - #6, #15, #24,
  #41, #42, #43, #44 and #45 of `docs\unit239-failing-set.txt` - has a verdict: green by
  repair of the decoder with no floor lowered and the three floor tests green at that
  commit's exit, or, after three consecutive units have each attacked it and measured no
  movement, parked in `docs\phase-cw\PARKED.md` as owed with its number and the three
  measurements; none retired, and the closing line of the set updated with the final count.*
- **Section 6.** *A red under 3.6 that three consecutive units have attacked without movement
  is parked, not chased.* Nothing is parked in this unit. *A floor would have to be lowered to
  go green. Never.*
- **R49.** A test that reads audio and asserts characters, elements, a tone or a speed is
  never retired. All four are that kind of test.
- **R55, with unit 402's decision on it.** Files under `src\Hamlet.RadioEngine\Cw` outside the
  eleven transmit files may change. **Nothing under `src\Hamlet.App` is written** (R50).
- **HM-DEC-095.** *A note is chosen by how it is keyed and never by how loud it is; the
  operator's own transmission is not evidence about anybody else.* This unit's property counts
  keying. It does not compare levels.
- **HM-DEC-127.** The 25 dB floor at `CwToneTracker.cs` 212 and 1077 stays at 25.
- **HM-DEC-009.** The window stops holding one sender's audio while it reads another's. A
  tracker change must still follow a real handover. `CwTwoStationTests` and #41 are its
  witnesses.
- **HM-DEC-090.** No speed from noise, and no speed across a handover.
- **HM-DEC-147.** The transmit state comes from the radio, never from the audio.
- **Unit 402's definitions, standing, at the head of `reds-3.6.md`:**
  - An **attack** is a change the trace chose, made to the decoder or to the red test's own
    event under R12, built, and measured on the red's own test.
  - **Movement** is the red's own printed number moving toward its assertion.
  - **Three consecutive units** means three consecutive 3.6 attempts. A red that is left
    unattacked breaks its sequence. A red that moves starts its count again.
- **The keep rule, unit 402's.** A change is kept only if all four of these hold:
  - a red turns green;
  - no green is lost;
  - no capture row's characters or elements go lower;
  - no bar or assertion moves.
- **Floors.** A floor is never lowered. A floor test's assertion is never edited.
- **Section 3, transmit.** These eleven files are read and never written:
  - `CwTransmitter.cs`, `KeyerCwSender.cs`, `TransmitChain.cs`, `AutoCall.cs`
  - `AutoCallAnswers.cs`, `CwTransmitGuard.cs`, `TransmissionWatch.cs`
  - `TransmitReadiness.cs`, `TransmitPrivileges.cs`, `TransmitNotes.cs`, `ICwSender.cs`

  Anything that would change what keys or transmits is `MOVE: stop`. A package is never added.
- **CLAUDE.md 0.0.** Never present a guess as a decode. No sentence says that a change makes CW
  "read". **CLAUDE.md 0.2.** Transmit safety is absolute.
- **HM-DEC-091.** A change that improves one recording and quietly costs another is not a fix.
- **HM-DEC-155.** No suites. **HM-DEC-165.** Nothing is red that was green before.
  **FACT-004.** Every number is an indication. **FACT-006.** There is no radio here.

### This unit's decisions - author's, overrulable

1. **The property is "the pitch being read is still keying."** It is measured at the bank's
   center pitch over the span the hold waited through, as on/off keying: marks with key-up
   between them, of any length. A run of dahs counts. It is never a level comparison.
   - Task 1 prints it on every held move and every cold move of the fixtures and captures
     listed there.
   - It **separates** only if both of these hold:
     - every move off a single sender on #15, #43 and #44 goes while the bank is still keying;
     - every move onto the station on `003016`, `031838`, `031905`, `032113` and `032129`,
       the rows H1 cost, and the two-station handover and #41's handover, goes while the bank
       is not keying.
   - If a known-right move goes while the bank is still keying, the property does not
     separate. Then no tracker change is made. The rows of #15, #43 and #44 say *not
     attacked* and name the move that broke the separation.
2. **The tracker change** is made at the hold, `CwToneTracker.cs` 957 to 966, on top of H2.
   It adds that property and nothing fitted to the rows. It is also made at the cold move at
   1092 only if task 1 finds a red's miss there. It is judged once on #15, #43 and #44, with
   #6 held green, and on the gate. One narrower shape may be tried at the same property, as in
   unit 406.
3. **#15 seed 104729's reference level.** Line 1091 may keep the reading level of the station
   being read instead of the confirmed candidate's own level. This is allowed only if task 1
   prints `_readingDb` standing more than 2 dB below the sender's own keyed level when the
   image is confirmed. It is judged as part of task 3's attack and never alone.
   `FilterRejectionDb` is not touched.
4. **#45 gets the air's trailing silence, in its own test under R12.** The easy tier's event
   pumps digital-zero audio after `PumpAll` and before `Flush`. The length is the stream's
   settle delay plus one second, taken from the code and printed. The event reads
   `CharacterSettled`, which is the transcript the tab keeps. The strangers, placeholders and
   expected text are computed exactly as they are now, and every assertion stays as written.
   - The change is made only if task 1's printer shows the decoder itself settling the
     placeholder away under that padding.
   - It applies to the whole theory. #43 and #44 are measured on it too.
   - A green reached this way counts as repaired, as 3.1's tick counts the settled event and
     #42's event.
   - B1, reading the settled event without the padding, is not made alone.
5. **G1 goes back in only on top of a kept or measured tracker change,** as one attack on
   #44, and only if the tracker change leaves #44 red. It is judged by the keep rule.
6. **Order:** task 2's #45 event first, because it touches no capture. Then task 3's tracker
   change, judged with the event in place. Then G1. A kept change is the base for the ones
   after it.
7. **Rows** are marked *attack 4* for each red attacked, with the sequence count under the
   head's rule.
8. **The entry lines** are unit 406's exit runs if
   `git diff --stat 24d14e9c HEAD -- src tests docs/carry-forward-tests.txt` prints nothing.
   The floors and the red-holding types are run in full either way.

## 7. Status cadence

As the header says. In `NOTE`, name the red in hand, the change, and the red's number before
and after.

---

## 8. The tasks

### Task 0 - the record and the entry round

1. Append `## UNIT 407 - STEP 3` to `PHASE_OUTCOME.md` in the shape of the existing entries,
   copying the decision block at the foot of this file.
2. Set `PHASE_STATUS.md` to unit 407 and CURRENT_STEP 3.
3. Patch-bump `Directory.Build.props`.
4. Run the entry round, one type per invocation, and record every figure:
   - the two carry-forward lines, under decision 8;
   - `TheCapturesThatDecodeKeepDecodingTests`, 300 s;
   - `TheAdjudicatedReadingsKeepReadingTests`, 180 s;
   - `CwFixtureTests.TheCleanRecordingsDecodeExactly`, 120 s;
   - `CwAcquisitionWindowTests`, `CwReceiverFixtureTests`, `CwFixtureTests`,
     `CwAdjudicationTests`, `CwEmissionGateTests`, `CwDisplacementFloorTests` and
     `HamletDoesNotDecodeYourOwnSendingTests`, 300 s each;
   - the speed readers `CapturedSignalTests`, `CwSpeedSilenceTests`,
     `WhyTheGateDidNotFireTests` and `CwTwoStationTests`, 300 s each;
   - the tracker readers unit 406 listed, 300 s each. `TheGateHasItsOwnWindowNowTests` runs
     its two short methods only (section 3 item 5).

   The eleven transmit files must print nothing against `7e209cb4`.

**Drop candidate:** none.

### Task 1 - the trace: is the station still keying, and what makes #45's tail

**Change no file under `src`.** Write a printer, `TheStationStillKeyingTraceTests`. It asserts
nothing and goes on no line. Run it alone, 600 s at most, and write its output to
`.run-unit\unit407-trace.txt`. Private state is read by reflection in the printer only.

- **The property, on every move.** Cover every seed of #6 and #15, coverage-easy,
  exchange-easy, the two-station fixture, #41's fixture, the five capture rows H1 cost
  (`003016`, `031838`, `031905`, `032113`, `032129`), and every other capture on which unit
  406 printed a hold. For each call into `Switch`, print:
  - the time and the caller line, and the pitch it moved from and to;
  - the sender's true pitch, where the fixture knows it;
  - how many marks and key-ups the bank's center pitch carried over the span the hold waited;
  - whether that span counts as still keying under decision 1.

  Then state in one sentence whether the property separates, under decision 1's test. Name
  every move that breaks the separation.
- **#15 seed 104729.** Print `_readingDb` and the sender's own keyed level at each confirm
  from 18 s to 21 s. Say whether decision 3's condition holds.
- **#45's tail.** Print:
  - the last 3 s of the stream, meaning the marks after `K` and what the trailing
    placeholder is made from;
  - the stream's settle delay, from the code;
  - the settled transcript and its strangers and placeholders with the file as it is, then
    with decision 4's padding.

  Do the same for coverage-easy and exchange-easy.

Write the findings to `docs\phase-cw\unit407-reds.md` section 2. Commit the printer and the
file.

**Drop candidate:** none. No attack is made without a traced cause.

### Task 2 - #45 given the air's trailing silence

If task 1 meets decision 4's condition, change the easy tier's event at
`CwReceiverFixtureTests.cs` 175 to 215 as decision 4 says. Build once. Then run
`CwReceiverFixtureTests`, the three floor tests, and `CwFixtureTests`. Keep or put back under
the keep rule, in its own commit, with #43's and #44's numbers recorded. If the condition does
not hold, make no change. #45's row then says *not attacked* and gives the reason.

**Drop candidate:** none. It is short and touches no capture.

### Task 3 - the tracker change

Under decisions 1 to 3, and on top of task 2's result, make one change at the line task 1
named. Build once. Then run, with task 0's timeouts:

- #15's type;
- `CwReceiverFixtureTests`;
- the three floor tests;
- every red-holding type;
- the four speed readers;
- the tracker readers.

Keep or put back under the keep rule, with every number written as it is measured. A kept
change is its own commit. Its message names the reds, the file and line, and the numbers before
and after. Before the next change, a put-back leaves `git diff --stat HEAD -- src tests`
printing nothing.

If the first shape moves a red and fails the gate, one narrower shape may be tried, aimed at
the same property. Then, if #44 is still red, run decision 5's G1 combination the same way.

**Drop candidates: the narrower shape, then the G1 combination.** **Clock rule:** after three
and a half hours, finish the change in hand, record it, and go to task 4. Drop G1 after four
hours.

### Task 4 - the record

1. Add one row per red to `docs\phase-cw\reds-3.6.md` under decision 7.
2. Add the attack table to `unit407-reds.md` section 3.
3. **If a red went green:**
   - update the closing line of `docs\unit239-failing-set.txt` with the counts and the
     numbers that remain;
   - add a clause to 3.1's tick sentence in `PHASE_PLAN.md`, in the bold form.
4. **Tick 3.6 only if every one of the eight has a verdict at task 5's exit.** Either way,
   state *n of eight with a verdict*.

**Drop candidate:** none.

### Task 5 - the exit round

1. Run task 0's full round again, one type per invocation.
   - **HM-DEC-165.** If a kept change turns anything red that was green at entry, on either
     line or in any type, it goes back out in its own commit. Its row names what it turned
     red, and the round is run again.
   - The eleven transmit files print nothing against `7e209cb4`.
   - `git diff` over `src\Hamlet.App` between entry and exit prints nothing.
2. Re-confirm 3.5 on its own sentence, or un-tick it and name the commit.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **Steps 0, 1, 2 and 4.** Their criteria are all met. No rework piece or chain is re-applied.
- **#6, #24, #41 and #42.** They are green. They are gate cases here and nothing else. #42's
  ruling question is Tim's (section 3 item 1).
- **`7e65aac4`: reverting it or keeping it.** Tim's.
- **Re-expressing any floor, and the head-versus-rows count rule.** Tim's.
- **Tim's 12:55 capture and `TheCaptureOfTheTwentyThirdTests`.**
- **Reds outside the 51-name set:** `fading-18wpm`, `CwSensitivityTests`,
  `WhatBandwidthTheDecoderListensThroughTests`' two, and 398 item 3.
- **Everything in `PARKED.md` and in PHASE_PLAN.md section 7.**

## 10. What not to do

- **Do not re-make any change units 402, 405 and 406 put back, as it was made:** B1 alone, B2,
  A1, S1, G1 alone, M1, M2, H1, and the unconditioned skip at `CwDecoder.cs` 590.
- **Do not fit the property to seconds, pitches or named rows.** It is one rule applied to
  every move, or it is not made.
- **Do not compare levels to choose a pitch** (HM-DEC-095). Do not change `FilterRejectionDb`.
- **Do not write `CwTransmitGuard.cs`** or any other transmit file, or anything under
  `src\Hamlet.App`.
- **Do not lower a floor. Do not edit a bar or an assertion.** R12 covers the easy tier's event
  and nothing else in that file.
- **Do not park or retire any red.**
- **Do not describe any transcript as correct** (section 0.0).
- **No unfiltered `dotnet test`. Never background a run and poll it.**
- **Report mismatches. Repair nothing. American spelling. UTF-8.**

## 11. Committing and pushing

- Commit per task, and per kept change.
- Push at the end, and say whether the push succeeded.

---

## 12. Reporting

Write `output.md` at the root with the canonical headings. **The ordering block comes
first**, and `validate-output.bat` refuses a report without it:

```
READ IN THIS ORDER.

A. PHASE GOAL - CW decodes again. Steps 1, 2 and 4 done, their criteria
   all met; step 3 partial on 3.6 alone; step 5 is Tim's.
B. THIS STEP - step 3, the inherited reds. 3.1 to 3.5 met; 3.6, <n> of
   eight with a verdict - <which of #15 #43 #44 #45 went green on the
   still-keying property or on the air's trailing silence, and which stay
   open>.
C. THIS REPORT - the red table leads section 3; section 4 raises <n>
   items and <none | which> stands in the way of 3.6. Whether the
   still-keying property separated the moves: <yes | no, broken by ...>.
   Carried for Tim: #42's line read from instruction 406's decision 2
   body, and the head's count rule, under which 3.6 closes only by greens.
```

Then the six-line header:

```
UNIT:       407 - <complete|stopped> at task N of 5, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes - step 3 criterion 6 | no - and why>
NUMBER:     of 3.6's eight <n> with a verdict; this unit <n> green, <n> moved, <n> not attacked
DRIFT:      0
```

**Section 3 leads with one table:** red, cause as traced, change, number before, number after,
moved, kept, sequence count. After it come the separation table from task 1, one row per move
with *still keying* yes or no, then the gate numbers for each kept change, and then the exit
round.

**Section 2 tells Tim in one paragraph** which reds moved and what that cost. It uses no word
that claims a decode.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: hold a tracker move off the pitch being read while that bank is still on/off keying, traced first on #15 #43 #44 against the five captures H1 cost and the handovers; #45's easy-tier event pumps the air's trailing silence before Flush and reads the settled transcript under R12
MOVE: work around
WHY: 3.6 is the only criterion the loop can move, and its four open reds all have a traced cause. Unit 406 showed the held moves off one station are the fault on #15, #43 and #44, but the survey could not tell them from moves onto a station. This unit asks the bank being read instead, measured before anything is built. #45's only fault is where the file ends, which the air never has. The loop test found nothing like it. A flip needs all four green, so a third non-flip in a row is likely and is reported as such.
STATE: partial
DECIDED: author's, overrulable - the property is on/off keying at the bank's center over the hold's span, never a level, and separates only if every red move goes while keying and every known-right move while not, else no tracker change and the rows say not attacked; the change sits on H2 at CwToneTracker 957 to 966, and at 1092 only on a traced red miss; line 1091's reference level only if the trace shows it more than 2 dB below the sender, FilterRejectionDb untouched; #45's event pads digital zero of the settle delay plus one second before Flush and reads CharacterSettled, whole theory, only if the trace shows the decoder settling the placeholder away; G1 only on top of the tracker change if #44 stays red; order #45 event, tracker, G1; rows attack 4; #42's record stands on instruction 406 decision 2's body text, not re-ruled, logged for Tim; the gate-window long method stays unmeasured under HM-DEC-155; entry lines from unit 406's exit if the diff is empty; the per-type timeouts
LICENCE: PHASE_PLAN.md 3.6, R49, R50, R55, section 3, and section 6 on floors, transmit and three tries; unit 402's definitions and keep rule in reds-3.6.md; instruction 406 decision 2; HM-DEC-095, HM-DEC-127, HM-DEC-009, HM-DEC-090, HM-DEC-091, HM-DEC-147, HM-DEC-155, HM-DEC-165; R12; CLAUDE.md 0.0 and 0.2
ACCOMPLISHED: the decoder stops walking away from a station that is still sending, if that can be told apart from a real change of station on every saved recording without costing any of them; and the last easy-tier test is judged the way the CW tab sees a transmission end on the air, not the way a file ends
ADVANCES: step 3 criterion 6
END-ARBITER-DECISION
```
