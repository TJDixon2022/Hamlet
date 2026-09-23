# Work instruction 406 - the tracker switch, and #42 given the radio's word

**Authored by the arbiter.** 3.6 is the only open criterion the loop can move. It has six reds
left: #6, #15, #42, #43, #44 and #45 of `docs\unit239-failing-set.txt`. Units 402 and 405
attacked where a wrong reading shows up: the unit estimator, the gap clip, the flush, and the
re-mix. Unit 405's trace found that on every single-sender red, each miss comes after
`CwToneTracker.Switch` moves the mix 25 to 85 Hz off the one station. `CwDecoder.cs` 658 to 665
already says the fault is upstream. This unit goes to that source. For #42 it does something
new: the test gets the radio's own report that the operator is sending, which is what
HM-DEC-147 made the authority. **Six tasks. Drop from the back under the clock rule in task 2.**

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
before any assertion is lost. Re-run it once and count it neither way. A red on an assertion
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
- Put any multi-step command into `.run-unit\unit406-<name>.sh` and run it with `sh`. Every
  put-back goes through a script, as unit 405's `.run-unit\unit405-putback.sh` did, followed
  by a build.

## 3. Asks still outstanding

Every ask carried from before this phase is parked in `docs\phase-cw\PARKED.md` under R54, and
the arbiter reads that file as *parked to a later phase*. Unit 405's section 4 raised five
items. This is how the arbiter took each one. None is re-argued here:

1. **The #42 floors.** Refused. Section 6 says *a floor would have to be lowered to go green.
   Never.* Re-expressing a floor so it counts fewer characters lowers it. Instead, #42 gets
   the route in decision 2. That route moves no floor.
2. **Greens that cost trailing-placeholder rows, and #6's rows that move both ways.** Unit
   402's keep rule stands, and no self-ruling may overrule it. Any row that goes lower is a
   cost.
3. **Leave to change the tracker's switch.** Given, under decision 1. R55 opens every file
   under `src\Hamlet.RadioEngine\Cw` outside the eleven transmit files, and `CwToneTracker.cs`
   is not one of them.
4. **The count rule, head or rows.** The head of `reds-3.6.md` counts, as instruction 402's
   decision 3 ruled. That is an arbiter ruling. Only Tim may overrule it. Logged for him in
   section 12 C.
5. **Keeping G1 without a green.** Refused under the keep rule. G1 may go back in only as part
   of an attack that turns a red green (decision 4).

---

## 4. Why this unit exists

**The count today.** Steps 1, 2 and 4 are done. Step 5 is Tim's. **Step 3 has five of six
criteria ticked. 3.6 is open at 2 of eight with a verdict:** #24 and #41 went green by unit
402's `7e65aac4`. Six reds are open. Every one is at 0 of 3 under the head's rule after unit
405, which left `src` identical to its entry:

| red | test | number at HEAD | what 405 found |
|---|---|---|---|
| #6 | `CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp`, 25 wpm | share 0.75, bar 0.79 | first 1.14 s mixed at the unmeasured 600 Hz for a 640 Hz sender; the tail follows a switch to 550 and 725 Hz |
| #15 | `CwAcquisitionWindowTests.TheSlowEndReadsTheMessage`, 12 wpm 18 dB | share 0.54 | noise-mark dit cluster; upstream, the mix moved to 700 Hz for 640 |
| #42 | `CwReceiverFixtureTests.NothingIsEmittedDuringTheOperatorsOwnTransmission` | 70 | only the generator's residue separates the fixture's spans; *the fixture reports no transmission to the decoder* |
| #43 | `TheEasyTierIsReadWhole` coverage-easy | 5 + 37 | mix at 575 Hz for 615 from 22.04 to 28.54 s |
| #44 | `TheEasyTierIsReadWhole` exchange-easy | 3 + 21 | mix at 575 Hz for 615 from 11.54 to 19.04 s; G1 cost nothing, but it turned nothing green |
| #45 | `TheEasyTierIsReadWhole` tightfist-easy | 1 + 3 | the flush settles one trailing unreadable character |

**What is new here.** First, one shared upstream cause is attacked at its own line,
`CwToneTracker.cs` 1092 and `Switch` at 1154. So far only the downstream lines where the wrong
pitch becomes a wrong reading have been attacked. Second, #42 stops asking the audio to prove
that the operator is sending. `CwDecoder.RadioIsTransmitting`, at `CwDecoder.cs` 624, is the
product's own input for that fact under HM-DEC-147: *the state comes from the radio and never
from the audio*. The test never calls it. No real capture carries a radio report, so a green
reached that way cannot move a capture floor.

**This unit flips 3.6 only if all six have a verdict.** #45's only remaining fault is a
placeholder that the capture floors themselves count. No red is anywhere near three
no-movement attacks. So the honest expected result is several reds green and 3.6 still open.
The report says so plainly.

```
PHASE GOAL: CW decodes again.
UNIT GOAL:  Trace and change the tone tracker's switch that moves the mix
            off a single sender, judged on #6 #15 #43 #44 #45; give #42's
            test the radio's report of the operator's own sending under R12
            and HM-DEC-147; keep only a red to green that loses no green
            and lowers no floor; write every row to docs/phase-cw/reds-3.6.md.
ADVANCES:   step 3 criterion 6
DRIFT:      0
```

---

## 5. Verify this instruction against the tree

Check each item below. **Report a mismatch in section 4 of the report. Do not repair it.**

- HEAD is `527b1659`, or a descendant whose commits touch no file under `src` or `tests`.
- `git diff --stat 7e65aac4 HEAD -- src` prints nothing.
- `src\Hamlet.RadioEngine\Cw\CwDecoder.cs` is 795 lines:
  - line 346 is `ResumeAfter`, 500 ms;
  - line 501 tests `DecodingSuspended || DigitalMode`;
  - line 624 is `public void RadioIsTransmitting(bool? transmitting, DateTime nowUtc)`.
- `src\Hamlet.RadioEngine\Cw\CwToneTracker.cs` is 1367 lines:
  - line 959 calls `Switch(_heldSwitchHz)`;
  - line 1092 calls `Switch(keyed.ToneHz)`;
  - line 1154 is `private void Switch(double toneHz)`.
- `tests\Hamlet.RadioEngine.Tests\Cw\Fixtures\CwReceiverFixtureTests.cs`:
  - line 260 is #42's method;
  - line 282 is `source.PumpAll();`;
  - line 291 asserts `OwnTransmitSeconds > 3`;
  - line 294 is `Assert.Equal(0, during);`.
- `docs\phase-cw\reds-3.6.md` has 402's and 405's rows. The last line of
  `docs\unit239-failing-set.txt` names #6, #15, #42, #43, #44 and #45 as red-open.
- HM-DEC-147 is in `DECISIONS.md` at line 671. HM-DEC-127 is cited in `CwToneTracker.cs` at
  lines 307 and 1038. Report where its text lives. It is not in `DECISIONS.md`.

**Expected, and not a mismatch:**
- The dispatcher loop may lose an app-line name before any assertion. Re-run once. It counts
  neither way.
- `TheEightRedsTests`, `TheSixRedsTraceTests` and `TheCaptureOfTheTwentyThirdTests` exist,
  assert nothing, and are on neither line.
- `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and files under `.run-unit` are
  modified and uncommitted. That is the launcher's work. Commit none of `.run-unit`.

## 6. Rulings in force - do not re-argue

From PHASE_PLAN.md R47 to R55, criterion 3.6 and section 6, and from the arbiter's decisions
of units 402 and 405, transcribed where they bind:

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
  never retired. All six are that kind of test.
- **R55, with unit 402's decision on it.** Files under `src\Hamlet.RadioEngine\Cw` outside the
  eleven transmit files may change. **Nothing under `src\Hamlet.App` is written** (R50).
- **HM-DEC-147.** *Hamlet suspends decoding while the radio is transmitting, takes that state
  from the radio and never from the audio, and never lets sent text enter the received
  stream.* `CwTransmitGuard` answers a different question: whether the receiver is muted.
- **HM-DEC-095.** *A note is chosen by how it is keyed and never by how loud it is; the
  operator's own transmission is not evidence about anybody else.* A tracker change that picks
  by level is not made.
- **HM-DEC-009.** The window stops holding one sender's audio while it reads another's. A
  tracker change must still follow a real handover. `CwTwoStationTests` and #41 are its
  witnesses.
- **HM-DEC-090.** No speed from noise, and no speed across a handover.
- **Unit 402's definitions, standing, at the head of `reds-3.6.md`:**
  - An **attack** is a change the trace chose, made to the decoder or to the red test's own
    event under R12, built, and measured on the red's own test.
  - **Movement** is the red's own printed number moving toward its assertion.
  - **Three consecutive units** means three consecutive 3.6 attempts. A red that is left
    unattacked breaks its sequence. A red that moves starts its count again.
  - A repair only inside `CwTransmitGuard` is not made. That file is a transmit file anyway.
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

1. **The tracker's switch is this unit's target for #6, #15, #43 and #44.**
   - The change goes in `CwToneTracker.cs` at the switch: its callers at 959 and 1092, or
     `Switch` itself.
   - It is aimed at the cause task 1 prints.
   - It is judged once, on all five of #6, #15, #43, #44 and #45, and on the gate.
   - If task 1 finds that the misses do not follow a switch, the change is not made. The row
     says *not attacked* and why.
2. **#42 gets the radio's word, under R12 and HM-DEC-147.**
   - The test's own event tells the decoder, through `RadioIsTransmitting`, that the radio
     reports the transmitter keyed across the fixture's own-send span. It then reports it
     unkeyed at the span's end.
   - The span comes from the fixture's recipe or sidecar. It is never measured from the
     audio.
   - Both assertions stay exactly as written, and so does the 13 s window.
   - This is the test's own event and not a bar. A green reached this way counts as a
     repaired red, the way 3.1's tick already counts the settled event.
   - If task 1 finds that the change needs a line under `src`, it goes in `CwDecoder.cs`
     only. One example is a clock that follows the audio instead of `DateTime`. Such a change
     must leave every capture and adjudicated row identical, because no capture calls
     `RadioIsTransmitting`.
3. **Order:** the tracker change, then #42, then the G1 combination. A kept change is the
   base for the ones after it.
4. **G1 goes back in only on top of a kept or measured tracker change,** as one attack on
   #44, and only if the tracker change alone leaves #44 red. It is judged by the keep rule.
   G1 on its own is not re-made.
5. **#45** is measured on the tracker change as one of the five. It gets no change of its
   own. B2 is measured, and it costs capture rows. If the tracker change does not move it, its
   row says *not attacked* with that reason.
6. **Rows** are marked *attack 3* for the reds attacked, with the sequence count under the
   head's rule.
7. **The entry lines** are unit 405's exit runs if
   `git diff --stat ef34d601 HEAD -- src tests docs/carry-forward-tests.txt` prints nothing.
   The floors and the red-holding types are run in full either way.

## 7. Status cadence

As the header says. In `NOTE`, name the red in hand, the change, and the red's number before
and after.

---

## 8. The tasks

### Task 0 - the record and the entry round

1. Append `## UNIT 406 - STEP 3` to `PHASE_OUTCOME.md` in the shape of the existing entries,
   copying the decision block at the foot of this file.
2. Set `PHASE_STATUS.md` to unit 406 and CURRENT_STEP 3.
3. Patch-bump `Directory.Build.props`.
4. Run the entry round, one type per invocation, and record every figure:
   - the two carry-forward lines, under decision 7;
   - `TheCapturesThatDecodeKeepDecodingTests`, 300 s;
   - `TheAdjudicatedReadingsKeepReadingTests`, 180 s;
   - `CwFixtureTests.TheCleanRecordingsDecodeExactly`, 120 s;
   - `CwAcquisitionWindowTests`, `CwReceiverFixtureTests`, `CwFixtureTests`,
     `CwAdjudicationTests`, `CwEmissionGateTests` and `CwDisplacementFloorTests`, 300 s
     each;
   - the speed readers `CapturedSignalTests`, `CwSpeedSilenceTests`,
     `WhyTheGateDidNotFireTests` and `CwTwoStationTests`, 300 s each;
   - **the tracker readers:** `ThePitchCanBeHeldTests`, plus every other compiled test type
     that names `CwToneTracker`, found by grep over `tests\Hamlet.RadioEngine.Tests`. Each
     one gets 300 s. List them in the report. A type that does not fit in 300 s is run alone
     at 600 s, and the report says so.

   The eleven transmit files must print nothing against `7e209cb4`.

**Drop candidate:** none.

### Task 1 - the trace at the switch, and #42's plumbing

**Change no file under `src`.** Write a printer, `TheTrackerSwitchTraceTests`. It asserts
nothing and goes on no line. Run it alone, 600 s at most, and write its output to
`.run-unit\unit406-trace.txt`. Private state is read by reflection in the printer only.

- **The switch.** On every seed of #6 and #15, and on coverage-easy, exchange-easy and
  tightfist-easy, print every call into `Switch`. For each call, give:
  - the time and the caller line;
  - the pitch it moved from, and the pitch it moved to;
  - the sender's true pitch, from the fixture's request;
  - the keyed and noise readings the verdict held;
  - which condition let it through: the hold, the reach, or the confirm.

  Then print the same for the two-station fixture's real handover, for #41's fixture, and for
  `021410`, `013637` and one capture where the tracker moves correctly. Name the property that
  separates a wrong move off a single sender from a right one, or say that none was found.
- **#6's first 1.14 s.** Say whether the 600 Hz starting mix is a switch not yet made, or the
  tracker's initial pitch. Name the line.
- **#42's plumbing.** Print what `RadioIsTransmitting` does under a chunked pump of
  `qsk-preamble.wav` with the radio reported keyed from the recipe's span start to its end:
  - whether `DecodingSuspended` holds across the whole span when the clock passed is audio
    time;
  - whether line 501's skip leaves anything in the stream that settles later with an `At`
    under 13 s;
  - the count `during` that results.

  Do this in the printer, not in the test. Also say where the recipe or sidecar states the
  span.

Write the findings to `docs\phase-cw\unit406-reds.md` section 2. Commit the printer and the
file.

**Drop candidate:** none. No attack is made without a traced cause.

### Task 2 - the tracker change

Make one change at the line task 1 named, under decision 1. Build once. Then run, with task 0's
timeouts:

- #6's and #15's type;
- `CwReceiverFixtureTests`;
- the three floor tests;
- every red-holding type;
- the four speed readers;
- the tracker readers.

Keep or put back under the keep rule, with every number written as it is measured. A kept
change is its own commit, and its message names the reds, the file and line, and the numbers
before and after. A put-back leaves `git diff --stat HEAD -- src tests` empty before the next
change.

If the first shape moves a red and fails the gate, one narrower shape may be tried. That shape
must be aimed at the same traced property, not fitted to the rows it lost. The row records
both shapes.

**Drop candidate: the second shape.** **Clock rule:** after three and a half hours, finish the
change in hand, record it, and go to task 3.

### Task 3 - #42 given the radio's word

Under decision 2, change #42's event at `CwReceiverFixtureTests.cs` 260 to 283 so that it
reports the radio's transmit state from the recipe's span. If task 1 requires it, make the one
`CwDecoder.cs` change decision 2 allows. Build. Run `CwReceiverFixtureTests`, the three floor
tests, and `CwEmissionGateTests`. Keep under the keep rule, in its own commit. Then, if #44 is
still red, run decision 4's G1 combination in the same way.

**Drop candidate: the G1 combination.** Drop it after four and a half hours.

### Task 4 - the record

1. Add one row per red to `docs\phase-cw\reds-3.6.md` under decision 6.
2. Add the attack table to `unit406-reds.md` section 3.
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
- **`7e65aac4`: reverting it or keeping it.** Tim's.
- **Re-expressing any floor, and the head-versus-rows count rule.** Tim's, by section 3 above.
- **Tim's 12:55 capture and `TheCaptureOfTheTwentyThirdTests`.**
- **Reds outside the 51-name set:** `fading-18wpm`, `CwSensitivityTests`, and 398 item 3.
- **Everything in `PARKED.md` and in PHASE_PLAN.md section 7.**

## 10. What not to do

- **Do not re-make any change units 402 and 405 put back as it was made:** B1, B2, A1, S1,
  G1 alone, M1, M2, and the unconditioned skip at `CwDecoder.cs` 590.
- **Do not key #42's change to anything in the audio:** not the residue, not the level, not
  the rate. The span comes from the recipe (HM-DEC-147).
- **Do not write `CwTransmitGuard.cs`** or any other transmit file, or anything under
  `src\Hamlet.App`.
- **Do not lower a floor. Do not edit a bar or an assertion.** R12 covers #42's event and
  nothing else in that file.
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
   eight with a verdict - <which of #6 #15 #42 #43 #44 #45 went green on
   the tracker switch or on the radio's word, and which stay open>.
C. THIS REPORT - the red table leads section 3; section 4 raises <n>
   items and <none | which> stands in the way of 3.6. #45's fault is a
   placeholder the floors count and no red has three no-movement attacks,
   so whether 3.6 can close under the head's count rule is Tim's to weigh.
```

Then the six-line header:

```
UNIT:       406 - <complete|stopped> at task N of 5, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes - step 3 criterion 6 | no - and why>
NUMBER:     of 3.6's eight <n> with a verdict; this unit <n> green, <n> moved, <n> not attacked
DRIFT:      0
```

**Section 3 leads with one table:** red, cause as traced, change, number before, number after,
moved, kept, sequence count. After it come the gate numbers for each kept change, and then the
exit round.

**Section 2 tells Tim in one paragraph** which reds moved and what that cost. It uses no word
that claims a decode.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: change the tone tracker's switch at CwToneTracker.cs 1092 and 1154 that moves the mix off a single sender, judged on #6 #15 #43 #44 #45; give #42's test the radio's transmit state through CwDecoder.RadioIsTransmitting from the recipe's span under R12 and HM-DEC-147
MOVE: work around
WHY: 3.6 is the only criterion the loop can move. Units 402 and 405 attacked the downstream lines, and 405 traced every single-sender miss to the tracker's switch, so this unit goes to that line. #42 stops asking the audio to prove the operator is sending, and uses the radio input HM-DEC-147 made the authority, which no capture floor can feel. The loop test found neither approach. A flip needs all six, #45's placeholder is counted by the floors, and no red is near three no-movement attacks, so a non-flip is likely. It would be the second in a row, which is stop 10, and puts the count rule and the floors to Tim.
STATE: partial
DECIDED: author's, overrulable - leave to change CwToneTracker's switch under R55, judged once on five reds; #42's event reports the radio keyed across the recipe's span through RadioIsTransmitting under R12 and HM-DEC-147, assertions and window unchanged, with one CwDecoder change allowed only if the trace needs an audio clock, and a green so reached counted as repaired like the settled event; G1 only on top of the tracker change and only if #44 stays red; #45 measured on the tracker change, with no change of its own; rows marked attack 3; unit 405's section 4 answered as refused floors, keep rule standing, tracker leave given, head count standing as instruction 402's ruling, G1 refused alone; the per-type timeouts and the tracker-reader grep
LICENCE: PHASE_PLAN.md 3.6, R49, R50, R55, section 3, and section 6 on floors, transmit and three tries; unit 402's definitions and keep rule in reds-3.6.md; HM-DEC-147, HM-DEC-095, HM-DEC-009, HM-DEC-090, HM-DEC-091, HM-DEC-155, HM-DEC-165; CLAUDE.md 0.0 and 0.2
ACCOMPLISHED: the decoder's habit of sliding off the one station it is listening to gets fixed at the place it happens, if that can be done without costing any saved recording; and the test that forbids reading the operator's own sending is told the radio is transmitting, the way the real radio tells Hamlet
ADVANCES: step 3 criterion 6
END-ARBITER-DECISION
```
