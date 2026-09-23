# Work instruction 405 - the second attack on the six reds

**Authored by the arbiter.** This unit makes the second attack on the six reds still open
under 3.6: #6, #15, #42, #43, #44 and #45 of `docs\unit239-failing-set.txt`. It does not
re-run unit 402's method. It starts where 402's trace stopped, at the causes 402 named but
did not reach, and at the two changes 402 measured only one at a time. **Five tasks. Drop
from the back under the clock rule in task 2.**

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
(PHASE_PLAN.md section 6). Write *tasks 0 to 4, none dropped* with commas.

## 2. The tool facts

- Apostrophes inside quoted heredocs break them.
- Doubled backslashes collapse.
- `;` is refused. `rm` is refused.
- Python cannot run here.
- For a multi-line commit message, use `-m` more than once.
- A bare `git worktree`, `git checkout` or `git show` is refused at the prompt.
- Put any multi-step command into `.run-unit\unit405-<name>.sh` and run it with `sh`. Every
  put-back goes through a script such as unit 402's `.run-unit\unit402-putback.sh`, then
  a build.

## 3. Asks still outstanding

Every ask carried from before this phase is parked in `docs\phase-cw\PARKED.md` under R54,
and the arbiter reads that file as *parked to a later phase*. None of them is this unit's.
Unit 404's section 4 raised four items. None asks for a ruling, and none is in the way of
3.6. They are logged, not chased.

---

## 4. Why this unit exists

**The count today.** Steps 1, 2 and 4 are done. Step 5 is Tim's. **Step 3 has five of six
criteria ticked, and 3.6 is open.** Of 3.6's eight reds, #24 and #41 went green by unit
402's decoder repair, `7e65aac4`. Six are left, with these numbers at HEAD, measured by unit
404's task 3 and identical to unit 402's:

| red | test | number at HEAD | where 402 left it |
|---|---|---|---|
| #6 | `CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp`, 25 wpm | share 0.75, bar 0.79 | not attacked, no cause at a line |
| #15 | `CwAcquisitionWindowTests.TheSlowEndReadsTheMessage`, 12 wpm 18 dB | share 0.54 | not attacked; 2 of 3 seeds fit 23 wpm to a 12 wpm sender |
| #42 | `CwReceiverFixtureTests.NothingIsEmittedDuringTheOperatorsOwnTransmission` | 70 | skipping guard-blocked hops turned it green, but cost `013347`, `013622` and `VA3VRR`, so it was put back |
| #43 | `TheEasyTierIsReadWhole` coverage-easy | 5 + 37 | the settled event moved it to 4 + 7 |
| #44 | `TheEasyTierIsReadWhole` exchange-easy | 3 + 21 | the settled event moved it to 0 + 7 |
| #45 | `TheEasyTierIsReadWhole` tightfist-easy | 1 + 3 | the settled event moved it to 1 + 0, leaving one trailing placeholder; the flush change alone did not move it |

**What is new here.** Unit 402 measured two #45 changes separately: the settled event, and
the flush settling a trailing unreadable character. It never measured them together, and
together they are aimed at both halves of #45's remaining number. Unit 402's #42 change
failed because the guard blocks hops on real captures too. This unit looks for a measurable
property that separates the fixture's own-send spans from those. On #15, and on the strangers
in #43 and #44 (`TETEEETEE`, `TMT`, where `N0CALL` and `DE` should be), the misreads look like
the doubled-speed fit. This unit traces that fit to a line in the speed grid or the unit
estimator.

**This unit flips 3.6 only if all six go green.** Parking needs three consecutive attacks
with no movement, and #6 and #15 have never been attacked. So a partial result is the
expected outcome, and the report says so plainly. Each red that goes green is one fewer for
the units after this one.

```
PHASE GOAL: CW decodes again.
UNIT GOAL:  Make the second attack on each of 3.6's six open reds - #6, #15,
            #42, #43, #44, #45 - from the causes unit 402's trace named, keep
            only a red to green that loses no green and lowers no floor, and
            write each attack as its row in docs/phase-cw/reds-3.6.md.
ADVANCES:   step 3 criterion 6
DRIFT:      0
```

---

## 5. Verify this instruction against the tree

Check each item below. **Report a mismatch in section 4 of the report; do not repair it.**

- HEAD is `f74b6d51` or a descendant whose commits touch no file under `src` or `tests`.
- `git diff --stat 7e65aac4 HEAD -- src` prints nothing. Chain S and chain D went back out
  in `675d846f` and `264b97ce`.
- `src\Hamlet.RadioEngine\Cw\CwDecoder.cs` is 795 lines:
  - line 513 hands a chunk to `_probabilistic.Skip`;
  - line 590 hands samples to `_probabilistic.Process`;
  - line 701 carries 402's half-passband test, `CwProbabilisticDecoder.BandwidthHz / 2`.
- `CwProbabilisticDecoder.cs` line 438 is `SlowestWpm = 8`, and line 459 is `FastestWpm = 40`.
- `tests\Hamlet.RadioEngine.Tests\Cw\Fixtures\CwReceiverFixtureTests.cs` line 203 is
  `decoder.CharacterDecoded += read.Add;`.
- `tests\Hamlet.RadioEngine.Tests\Cw\TheEightRedsTests.cs` exists, asserts nothing, and is on
  neither carry-forward line.
- `docs\phase-cw\reds-3.6.md` has one row per red, all from unit 402. The last line of
  `docs\unit239-failing-set.txt` names #6, #15, #42, #43, #44 and #45 as red-open.

**Expected, and not a mismatch:**
- The dispatcher loop may lose an app-line name before any assertion. Re-run once; it counts
  neither way.
- `TheCaptureOfTheTwentyThirdTests` exists from unit 403 and is on neither line.
- In `reds-3.6.md`, the head says a red that moved without going green starts its count
  again, while the #42 to #45 rows read *attempt 1 of 3*. Report the disagreement. Do not
  rewrite either. The rows this unit adds follow section 6's decision 3.

## 6. Rulings in force - do not re-argue

From PHASE_PLAN.md R47 to R55, criterion 3.6 and section 6, and from the arbiter's decisions
of unit 402, transcribed where they bind:

- **3.6, verbatim.** *Each of the eight reds open at unit 401's closing line - #6, #15, #24,
  #41, #42, #43, #44 and #45 of `docs\unit239-failing-set.txt` - has a verdict: green by
  repair of the decoder with no floor lowered and the three floor tests green at that
  commit's exit, or, after three consecutive units have each attacked it and measured no
  movement, parked in `docs\phase-cw\PARKED.md` as owed with its number and the three
  measurements; none retired, and the closing line of the set updated with the final count.*
- **Section 6.** *A red under 3.6 that three consecutive units have attacked without
  movement is parked, not chased, and the criterion closes on that verdict; the loop is never
  held on a single test.* Nothing is parked this unit. No red has three attacks.
- **R49.** A test that reads audio and asserts characters, elements, a tone or a speed is
  never retired. All six are that kind of test.
- **R55, and unit 402's decision on it.** R55 is the later ruling. Files under
  `src\Hamlet.RadioEngine\Cw` outside the eleven transmit files may change. **Nothing under
  `src\Hamlet.App` is written** (R50).
- **Unit 402's definitions, standing.** They are at the head of `reds-3.6.md`:
  - An **attack** is a change the trace chose, built, and measured on the red's own test.
  - **Movement** is the red's own printed number moving toward its assertion.
  - **Three consecutive units** means three consecutive 3.6 attempts. A 4.7 unit in between
    does not break the sequence. A 3.6 unit that leaves a red unattacked does break that
    red's sequence.
  - A test that reads the settled transcript the tab keeps counts as repaired, as 3.1's tick
    already counts it. This is allowed for #43 to #45 at line 203. For #42, it is allowed
    only if a counted character reaches neither `LeadingEdge` nor `CharacterSettled`.
    Unit 402 measured 0 such characters on #42.
  - A repair only inside `CwTransmitGuard` is not made.
  - A speed repair keeps HM-DEC-090's two promises, no speed from noise and none across a
    handover, or it is not made.
- **The keep rule, unit 402's.** A change is kept only if all of these hold:
  - a red turns green;
  - no green is lost;
  - no capture row's characters or elements go lower;
  - no bar or assertion moves.

  Gate each change with one build and one type per call.
- **Section 6, floors.** A floor is never lowered. A floor test's assertion is never edited.
- **Section 3, transmit.** These eleven files are read and never written:
  - `CwTransmitter.cs`, `KeyerCwSender.cs`, `TransmitChain.cs`, `AutoCall.cs`
  - `AutoCallAnswers.cs`, `CwTransmitGuard.cs`, `TransmissionWatch.cs`
  - `TransmitReadiness.cs`, `TransmitPrivileges.cs`, `TransmitNotes.cs`, `ICwSender.cs`

  Anything that would change what keys or transmits is `MOVE: stop`. A package is never
  added.
- **CLAUDE.md section 0.0.** Never present a guess as a decode. A share or a count is a
  number. No sentence says a change makes CW "read".
- **CLAUDE.md section 0.2.** Transmit safety is absolute.
- **HM-DEC-091.** A change that improves one recording and quietly costs another is not a
  fix.
- **HM-DEC-155.** Never run a suite. **HM-DEC-165.** Nothing may be red that was green
  before. **FACT-004.** Every number is an indication. **FACT-006.** There is no radio here.

### This unit's decisions - author's, overrulable

1. **Two changes at one red.** Unit 402 measured two #45 changes one at a time: B1, line 203
   reading `CharacterSettled`, and B2, `CwProbabilisticStream.cs` 507, where the flush does
   not settle a trailing unreadable character still inside the delay. Here they are applied
   together and judged as one attack on #43, #44 and #45. If they turn any of the three green,
   they are kept under the keep rule. B1 alone is still judged as 402 judged it.
2. **The order of attack:**
   - the easy tier, #43 to #45;
   - then the speed fit, #15, and #43 and #44 if their strangers trace to it;
   - then #42;
   - then #6.

   A change kept for an earlier red is the base for the later ones.
3. **The row.** Each red gets a row marked *attack 2*. In its verdict column the row states
   the sequence count under the head's rule, and says whether its number moved.
4. **Not attacked.** A red whose trace names no line and no measurable property gets no
   change. Its row says *not attacked* and why. That breaks its sequence, as unit 402's
   definitions say.
5. **The entry lines.** The two carry-forward lines at entry are unit 404's exit runs if
   `git diff --stat f74b6d51 HEAD -- src tests docs/carry-forward-tests.txt` prints nothing.
   The floors and the red-holding types are run in full either way.
6. **The timeouts** are as each task lists them.

## 7. Status cadence

As the header says. Put what is moving inside the task in `NOTE`: the red in hand, the
change, and its red's number before and after.

---

## 8. The tasks

### Task 0 - the record and the entry round

1. Append `## UNIT 405 - STEP 3` to `PHASE_OUTCOME.md` in the shape of the existing entries,
   copying the decision block at the foot of this file.
2. Set `PHASE_STATUS.md` to unit 405 and CURRENT_STEP 3.
3. Patch-bump `Directory.Build.props`.
4. Run the entry round, one type per invocation, and record every figure:
   - the two carry-forward lines, under decision 5;
   - `TheCapturesThatDecodeKeepDecodingTests`, 300 s;
   - `TheAdjudicatedReadingsKeepReadingTests`, 180 s;
   - `CwFixtureTests.TheCleanRecordingsDecodeExactly`, 120 s;
   - `CwAcquisitionWindowTests`, `CwReceiverFixtureTests`, `CwFixtureTests`,
     `CwAdjudicationTests`, `CwEmissionGateTests` and `CwDisplacementFloorTests`, 300 s
     each;
   - the speed readers `CapturedSignalTests`, `CwSpeedSilenceTests`,
     `WhyTheGateDidNotFireTests` and `CwTwoStationTests`, 300 s each.

   The eleven transmit files must print nothing against `7e209cb4`.

**Drop candidate:** none.

### Task 1 - the trace, past where 402 stopped

**Change no file under `src`.** Extend `TheEightRedsTests` or write a sibling printer. It
asserts nothing and goes on no line. Run it alone, 600 s at most, and write its output to
`.run-unit\unit405-trace.txt`. Private state is read by reflection in the printer only.
Name a file and line for each finding, or say that none was found.

- **Group B, the easy tier.** For each settled character of coverage-easy, exchange-easy and
  tightfist-easy, print:
  - the time it settled;
  - the lattice's speed at that time;
  - the unit estimator's dit length;
  - where it falls in the fixture's expected text.

  The fixture's own wpm comes from its request. Then say whether the strangers sit where
  the fitted speed is about double or half the sender's. Also print #45's trailing
  placeholder with B1 and B2 applied together in an uncommitted change, then put the change
  back.
- **Group D, #15 and #6.** On each seed, print how the speed grid's likelihood falls across
  wpm before and after the point where the fit leaves 12 for 23. Print the estimator's
  short-cluster and long-cluster medians at the same points. Then name the line where the
  doubled hypothesis wins, or say that none was found.

  For #6, print the first second of each seed: every element's length, where the first
  key-down is cut, and what the lattice holds before `Q`. Then name the line where the
  `Q` becomes `■ A`, or say that none was found.
- **Group C, #42.** For every span `CwTransmitGuard` marks, print its start, its length, and
  whatever the guard and tracker expose at that time: level, SNR, tone, and the reason if
  one is exposed. Do this on `qsk-preamble.wav`, on `cw-2026-08-17-013347`, on `013622`,
  and on the `VA3VRR` capture.

  Then say whether one property separates the fixture's own-send spans from every span on
  the three real captures. Also say whether the fixture feeds the decoder any report of the
  operator's own transmission, `DecodingSuspended` or another.

Write the findings to `docs\phase-cw\unit405-reds.md` section 2, and commit the printer and
the file.

**Drop candidate:** none. No attack is made without a traced cause.

### Task 2 - the attacks, in decision 2's order

For each red, make one change per cause task 1 named:

- in `src\Hamlet.RadioEngine\Cw` outside the transmit files;
- or, for #43 to #45, at the test's own line 203 under R12.

Build once. Run the red's own type, then the gate, each with task 0's timeouts:

- the three floor tests;
- every red-holding type;
- for any change to speed, the four speed readers.

Keep or put back under the keep rule, with the numbers written as each change is measured.

- **#42's change must be conditioned on the property task 1 found.** If task 1 found none,
  #42 is not attacked (decision 4). Do not re-make unit 402's unconditioned skip.
- **A speed change** must keep `CwSpeedSilenceTests` naming only 12 on exchange-easy. It
  must also keep `CwTwoStationTests`' handover. Otherwise it is put back.
- **Each kept change is its own commit.** The message names the red, the file and line,
  and the number before and after.

**Drop candidate: #6's attack.** **Clock rule:**
- Do not start #6 if four hours have passed. Its row then says *not attacked, clock*.
- If four and a half hours have passed, finish the change in hand, record every red not
  reached as *not attacked, clock*, and go to task 3.

### Task 3 - the record

1. Add one row per red to `docs\phase-cw\reds-3.6.md`, in its columns, under decision 3.
2. Add the attack table to `unit405-reds.md` section 3: change, file and line, red's number,
   gate, kept.
3. **If a red went green:**
   - update the closing line of `docs\unit239-failing-set.txt` with the new counts and the
     remaining numbers;
   - add a clause to 3.1's tick sentence in `PHASE_PLAN.md`, in the bold form the ticked
     lines use.
4. **Tick 3.6 only if all six are green at task 4's exit.** Say that 3.6 has *n of eight
   with a verdict* either way.

**Drop candidate:** none.

### Task 4 - the exit round

1. Run task 0's full round again, one type per invocation.
   - **HM-DEC-165.** If a kept change turns anything red on either line, or in any type,
     that was green at entry, it goes back out in its own commit. Its row changes to that
     name. The round is then run again.
   - The eleven transmit files print nothing against `7e209cb4`.
   - `git diff` over `src\Hamlet.App` between entry and exit prints nothing.
2. Re-confirm 3.5 on its own sentence, or un-tick it and name the commit.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **Step 4, closed.** No chain or piece of the August rework is re-applied. Unit 404's items
  1 to 4 are logged.
- **`7e65aac4`: reverting it or keeping it.** Tim's.
- **Tim's 12:55 capture and `TheCaptureOfTheTwentyThirdTests`.** Logged, not chased.
- **Reds outside the 51-name set:** the `fading-18wpm` case, `CwSensitivityTests`, and 398
  item 3.
- **Everything in `PARKED.md` and in PHASE_PLAN.md section 7.**

## 10. What not to do

- **Do not re-make any change unit 402 put back as it was made.** B1 alone, B2 alone, and
  the unconditioned skip at line 590 are all measured.
- **Do not attack a red without a traced cause** (decision 4).
- **Do not park or retire any red.** No red has three attacks.
- **Do not lower a floor. Do not edit a bar or an assertion.** R12 covers only line 203's
  event.
- **Do not write a transmit file or anything under `src\Hamlet.App`.**
- **Do not describe any transcript as correct** (section 0.0).
- **No unfiltered `dotnet test`. Never background a run and poll it.**
- **Report mismatches; repair nothing. American spelling. UTF-8.**

## 11. Committing and pushing

- Commit per task, and per kept change in task 2.
- Every put-back leaves `git diff --stat HEAD -- src tests` empty before the next change.
- Push at the end, and say whether the push succeeded.

---

## 12. Reporting

Write `output.md` at the root with the canonical headings. **The ordering block comes
first**, and `validate-output.bat` refuses a report without it:

```
READ IN THIS ORDER.

A. PHASE GOAL - CW decodes again. Steps 1, 2 and 4 done; step 3 partial on
   3.6 alone; step 5 is Tim's.
B. THIS STEP - step 3, the inherited reds. 3.1 to 3.5 met; 3.6, <n> of eight
   with a verdict - <which of #6 #15 #42 #43 #44 #45 went green, and which
   stay open at which attack>.
C. THIS REPORT - the six-red table leads section 3; section 4 raises <n>
   items and <none | which> stands in the way of 3.6.
```

Then the six-line header:

```
UNIT:       405 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes - step 3 criterion 6 | no - and why>
NUMBER:     of 3.6's eight <n> with a verdict; this unit <n> green, <n> moved, <n> not attacked
DRIFT:      0
```

**Section 3 leads with one table:** red, cause as traced, change, number before, number
after, moved, kept, and sequence count. Then the gate numbers for each kept change, then the
exit round.

**Section 2 tells Tim in one paragraph** which of the six reds moved and what that cost.
It uses no word that claims a decode.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: second attack on the six reds: easy tier on the settled event with the trailing flush settle combined, doubled-speed fit traced to the speed grid, guard spans told apart from real captures for 42, bare-start first character for 6
MOVE: work around
WHY: 3.6 is the one open criterion outside Tim's verdict now that step 4 is done; unit 402's generic trace-and-change is recorded no, so this unit starts at the causes and combinations 402 named but never tried - B1 with B2 together, a conditioned guard skip, the doubled-speed line - and loop-test found nothing like it; 3.6 flips only if all six go green, because #6 and #15 have no attacks toward parking, so a non-flip is the likely honest result, and the three-tries rule may meet stop 10's two-unit count on the next 3.6 unit, which is the owner's to weigh.
STATE: partial
DECIDED: author's, overrulable - B1 and B2 judged together as one attack on #43 to #45; attack order easy tier, speed fit, #42, #6, kept changes the base for later ones; rows marked attack 2 with the sequence under the record head's rule and the head-versus-row disagreement reported not rewritten; a red with no traced line or property is not attacked; #42's skip only on a traced property separating own-send spans; entry lines are unit 404's exit if the f74b6d51 diff over src, tests and the list is empty; #6 the drop candidate at four hours; the per-type timeouts
LICENCE: PHASE_PLAN.md 3.6, R49, R50, R55, section 3, section 6 on floors, transmit and three tries; unit 402's standing definitions in reds-3.6.md; CLAUDE.md 0.0 and 0.2; HM-DEC-090, HM-DEC-091, HM-DEC-155, HM-DEC-165
ACCOMPLISHED: each of the six test recordings the decoder still misreads gets a second try aimed at the cause the first try found - the easy-tier sends, the slow sender read at double speed, the operator's own sending leaking into the text, the fast fist's first letter - and every change that clears one without costing another recording is kept
ADVANCES: step 3 criterion 6
END-ARBITER-DECISION
```
