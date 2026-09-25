# Work instruction 436 - the estimator checks its dit against its dah

**Authored on redirect 1 of this run: units 6 and 1 ran against 7.4 and neither moved it.**
Four rules tried at 7.4 were about when the mixdown may follow the tracker. R75 closes that
question and R76 keeps `CwToneTracker` shut. The fifth approach, carrying the held speed
through a read whose estimate halves, is recorded against 7.4 as `no`. That is its outcome row
(`RUN_LEDGER.md` 08:11 to 08:19). But the session that wrote that report stopped at task 0 and
built nothing. **This unit leaves the mixdown, the tracker and the carry alone. It goes one step
further back, into the estimator, to find why the estimate halved at all.** Four tasks. Drop
from the back.

**Status.** `sh tools/status.sh`, real clock, after every commit and every task, and
immediately before every `dotnet test`. **Write files as UTF-8.**

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

**HM-DEC-155.** Do not run the whole suite. Run only this unit's named types and
`docs\carry-forward-tests.txt`, as its top comment says. **Never background a test and poll
it.** Run one type per invocation, each with its own `timeout`. The captures type has 51 rows
and takes about 120 s. `WhatTheOpeningHeardTests` took 170 s at unit 433. Give each 600 s and
report what it took.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.
**`ADVANCES` reads exactly `step 7 criterion 4`.**
**Write `output.md` at the root before the session ends.**
**Nothing in section 4 halts this phase.** Park it and go on.

## 2. The tool facts

- Apostrophes in quoted heredocs break them.
- Doubled backslashes collapse.
- `;` and `rm` are refused.
- Python cannot run here.
- A multi-line commit message needs more than one `-m`.
- A bare `git worktree`, `git checkout` or `git show` is refused at the prompt.
- Put multi-step commands in `.run-unit\unit436-<name>.sh` and run them with `sh`.

## 3. Asks still outstanding

Carried per HM-DEC-139. **None is this unit's to answer.** P44 and P46 are the owner's and stay
in `PARKED.md`. **Unit 435's second session asked for a ruling: the launcher should end a
redirected session before it launches the next one.** That ruling is the owner's. It is
carried here and is not this unit's to make. Task 0 below keeps this unit from colliding
without it.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  The speed estimate stops halving as a session opens, because the estimator no longer takes broken marks for dits.
ADVANCES:   step 7 criterion 4
DRIFT:      0
```

**The count today.** Step 7 has 7.3, 7.5 and 7.7 ticked. 7.1, 7.2, 7.4, 7.6 and 7.8 are open.
Unit 433 recorded 7.4 as closed partial (P48), but it is **not ticked**. It ticks on a kept
change.

**The plan line.** 7.4: *a change against what 7.3 names is judged under 3.2's four tests, and
the named characters read in the opening 60 seconds of `cw-2026-09-24-003901` and `-003919` are
reported before and after.*

**What 7.3 named** (unit 429, `003f2c98`). In the opening, within a second of the mix moving,
the estimator's unit fell from about 50 ms to 27.5 ms. That is 43.6 WPM, above the 40 ceiling,
so the stream set the estimator aside and the grid chose **38 WPM for a sender at 22 to 24**.
15 of the opening's 23 characters were read at a grid speed. The locked stretch took every
speed from the estimator: 21.8 WPM, a 55 ms unit.

**Where the halving happens.** `CwUnitEstimator.Measure` (`CwUnitEstimator.cs` 77 to 106)
calls the unit the mean of two figures:

- the median of the short mark cluster;
- the median of the short gap cluster.

`ShortClusterMedian` splits the marks in two with `TwoMeansOnLogs` and never looks at the long
cluster again. A sender's dah is about three dits. **When the envelope breaks marks into
pieces, the short cluster fills with pieces, and nothing checks it against the dahs beside
it.** The remarks at 529 to 532 already say that real audio puts very short crossings in the
short heap. The median was the repair for a few of them. It is not a repair for a heap made of
them.

**No unit has asked the estimator to check its own dit against its own dah.** That check is
this unit's approach. It changes nothing in the stream, the grid, the mixdown or the tracker.
`CwProbabilisticStream.cs` 428 is `Measure`'s only caller in `src`.

---

## 5. Verify this instruction against the tree

Check each item, report any mismatch, and repair nothing:

- `Measure` is at `CwUnitEstimator.cs` 77 to 106, and the unit is `(shortMark + shortGap) / 2`.
  `ShortClusterMedian` is at 534 and `TwoMeansOnLogs` at 557. The long mark cluster is computed
  and then thrown away.
- `CwProbabilisticStream.cs` 428 is the only caller of `Measure` in `src`.
- **What HEAD is when you start.** Unit 435's first session may have committed by then. If it
  kept a speed carry at `CwProbabilisticStream.cs` 431 to 435, say so, and give its commit.
  This unit builds on HEAD as it finds it.
- `FastestWpm` at HEAD: 40 at `d548a565`.
- The numbers to hold (P48), unless unit 435 kept a change, in which case take its exit
  numbers and name them:
  - 165 edits over 565 on all keyed recordings, against inferred keys.
  - 17:37: 19 edits over its 25-character scored region.
  - 17 added letters, 8 of them single-element.
  - Captures 51 of 51, with `032113` at 47.
  - Adjudicated 13 of 13. Keyed floors 13 of 13.
  - The stream from 30 to 46.2 s: 22 named, `UIEH EE E E T I NIEEE E E ET N ■IK`.
  - `003901` cold: 9 named, `EII E T NHHK`.
  - `003919` cold: 25 named, `EITEETNXNIK EANQNID EANQNIK`.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R76, §3 and §6. The ones this unit leans on:

- **R76.** No unit changes `CwToneTracker` until 4.1 is met. **This unit does not open it.**
- **R75.** The fault at the mixdown is the tracker's choice, and no follow rule undoes it.
- **3.2's four tests, with R66's third.** A change is kept only if all four hold:
  1. Total edits over all keyed recordings **do not rise**. This is unit 430's arbiter
     decision for 7.4, since no key scores the opening.
  2. No named floor from 2.2 breaks.
  3. The three adjudicated readings are unchanged, or move onto exactly their own adjudicated
     text, printed before and after.
  4. No capture row's count at or above the span bar falls.
- **R71.** The floors count characters at or above raw span 13.0.
- **R73.** A key-aligned added character inside a scored stretch may leave a floor, each named
  with its recording. Nowhere else.
- **HM-DEC-091.** A change that reads one recording and costs another is not a fix.
- **§0.2** Nothing that keys or transmits is touched.
- **§12.5** and **§12.6** Repair nothing on the way past.
- **HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**The author's decisions for this unit, overrulable** (also in DECIDED):

- **The rule, fixed now, before the trace.** In `Measure`, take both mark cluster centroids
  from `TwoMeansOnLogs`. If the long centroid stands **more than 4.5 times** the short one, the
  short heap is not this sender's dits. The reading then takes its unit as **the median of the
  long mark cluster's members divided by 3**, and reports that as `DitMarkMilliseconds`.
  Otherwise the reading is exactly what it is today.
  - 4.5 sits halfway between a textbook dah (3 dits) and a dah measured against half a dit (6).
  - The divisor 3 is the Morse standard.
  - **Neither number is tuned after the trace.**
- **One narrower variant**, and only if the rule fails on exactly one row or one test: the same
  test, but the reading is returned not ready rather than rebuilt from the dah. Nothing else.
- **7.4 ticks only on a kept change that moves the opening's text.** If nothing is kept, P48
  gains this unit's measurement and 7.4 stays unticked.
- **If unit 435's first session has already ticked 7.4 by the time task 0 ends, build nothing.**
  Record that, run the exit round, and report. The criterion is met, and a second change
  against it is not this unit's.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - no second session, then the record

**First, before any build or edit:** look for a live chain from unit 435. Check `ps` for
`unit435-` scripts and for `dotnet test` or `testhost` processes. At 08:21 today one was running
its captures entry round (PIDs 627 to 637).

- **If one is live, wait for it to end. Do not kill it.** Check `ps` every few minutes, with
  no test running meanwhile, for up to 120 minutes.
- **If it is still live after 120 minutes,** stop at task 0 with nothing changed and say so in
  section 4.
- **Once it has ended,** read `git log` and `PHASE_PLAN.md` 7.4, and apply the last decision of
  section 6.

Then do the record:

- Give `PHASE_OUTCOME.md` its `## UNIT 436 - STEP 7` entry from the decision block at the foot
  of this file.
- Make `PHASE_STATUS.md` name unit 436, *the estimator checks its dit against its dah*, and
  `CURRENT_STEP: 7`.
- Patch-bump `Directory.Build.props` from whatever HEAD holds.
- Commit the launcher's modified root files as they stood.

Then run the **entry round**, one type per invocation, and record the numbers of section 5 as
the numbers to beat:

- the build, with warnings as errors;
- both carry-forward lines;
- the captures, adjudicated and keyed-floor tests;
- the keyed totals;
- `WhatTheStrayLettersRestOnTests`;
- `WhatTheOpeningHeardTests`.

**Drop candidate:** none.

### Task 1 - the trace (7.4)

Add a fact to `WhatTheOpeningHeardTests` that asserts nothing and writes nothing. It prints one
row **per read**, through the spliced stream from 27.0 to 46.2 s and through `003919` cold, with
these columns:

- the stream time;
- the mark count;
- the short and long mark centroids and their ratio;
- the short-cluster median;
- the long-cluster median;
- the short gap median;
- the entry unit and WPM, and whether the stream took it or the grid decided;
- **the unit and WPM the rule of section 6 would give**, and whether the ratio test fired.

Print the same columns for the locked `004108` from 13.8 to 30 s, beside the opening.

Then, over **all 51 capture rows and every keyed recording**, count per recording the reads
where the ratio test fires, and name every recording where it fires at all.

**The one stop.** The rule cannot move 7.4 if the ratio test fires on **no read** in the stream
from 30 to 46.2 s, or if every read where it fires still falls outside 8 to 40 WPM. If either
holds, build nothing, print what the halved readings' clusters actually held, say so, and go to
task 3. Anything else goes on to task 2. The count of other recordings touched is a forecast
for the four tests. **It is not a gate.**

**Drop candidate:** the `004108` comparison columns.

### Task 2 - build it and judge it (7.4)

Build the rule in its own commit, exactly as section 6 states it, with a remark in the file's
own style saying why. Then run 3.2's four tests and print each one as a number:

1. total edits over all keyed recordings, before and after, with 17:37's edits over its 25;
2. all 13 named floors;
3. the three adjudicated readings, quoted before and after;
4. all 51 capture rows' above-bar counts, before and after.

**Then the criterion's own figure, as P42 has it, text beside count.** Print each of these
whole, before and after, beside `EANQNID`:

- the stream from 30 to 46.2 s;
- `003901` cold;
- `003919` cold.

Print the opening's speed per read under the change beside task 1's entry column. Section 3 can
then say whether the speed held, and **whether a held speed was enough through a mix 100 Hz off
the sender**. That was unit 429's hypothesis. Answer it either way.

**A change is kept only if all four tests pass.**

- **A change that fails any test goes back out in the next commit.** The report says which
  test, which row and which number.
- **If it fails on exactly one row or one test,** build the variant of section 6 once, in its
  own commit, and judge it the same way.
- **If the change passes and the opening's text does not move at all, it is not kept.** It
  changed nothing 7.4 asks about. Take it out and say so.

On a kept change, tick 7.4 in `PHASE_PLAN.md`, and note on step 7's line in `PHASE_STATUS.md`
that the P48 closure is superseded. If nothing is kept, append this unit's measurement to P48.

**Drop candidate:** the variant.

### Task 3 - the exit round

Run all of these:

- both carry-forward lines;
- the three floor tests;
- the keyed totals;
- `WhatTheStrayLettersRestOnTests`;
- `WhatTheOpeningHeardTests`;
- every type touched.

`git diff` over the transmit files against `7e209cb4` prints nothing. `git diff` over
`CwToneTracker.cs`, `CwToneSurvey.cs`, `CwDecoder.cs` and `CwProbabilisticStream.cs` against
this unit's entry prints nothing.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **`CwToneTracker`, the tone survey, and the mixdown at `CwDecoder.cs` 617 to 621.** These
  belong to R76 and to 4.1 to 4.7.
- **The stream's speed choice at `CwProbabilisticStream.cs` 428 to 435, and the grid.** Unit
  435's approach, recorded.
- **The speed range 8 to 40.** It belongs to 7.1 and 7.2.
- **The hysteresis depth, `ShortestRunHops`, and `MeasureGaps`.** They are not this rule.
- **3.6 the stray letters, 6.5 the dead button, 7.8 the preamp.**
- **P27, P37, P38, P40, P41, P43 to P48.**

## 10. What not to do

- **Do not stop, kill or signal another session's processes.** Wait, as task 0 says.
- **Do not touch the tracker, the survey, the mixdown line, the stream's speed choice, or
  anything that keys or transmits.**
- **Do not tune 4.5 or 3 after the trace**, and build no more than the one variant.
- **Do not keep a change that does not move the opening's text.**
- **Do not lower an above-bar count on any row, or a named floor.**
- **Do not halt for a question.** Park it.
- **Do not run an unfiltered `dotnet test`, background a test and poll it, or compose a
  timestamp.**
- **Report mismatches and repair nothing.** American spelling, UTF-8, and the four headings
  exactly.
- **Write `output.md` before the session ends.**

## 11. Committing and pushing

Commit once per task. Give the task 2 build and any take-out a commit each. Push at the end and
say whether the push succeeded.

---

## 12. Reporting

Write `output.md` at the root, with the four headings exactly as section 1 gives them. Put the
ordering block first:

```
READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial on 3.6;
   4 not started; 5 the owner's; 6 partial on 6.5; 7 partial on 7.1, 7.2, 7.4, 7.6, 7.8.
B. Step 7, criterion 7.4: whether the estimator's dit-against-dah rule was kept, 3.2's four
   tests as numbers, and the opening - stream 30 to 46.2 s, 003901 and 003919 cold - before
   and after as text; and whether unit 435's first session had kept anything first.
C. The rest, weighed against A and B. Section 4 raises <n> items; say whether any stands in
   the way of 7.4.
```

```
UNIT:       436 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     stream 30 to 46.2 s: 22 named <text> -> <n> named <text>; keyed 165 over 565 -> <n>; opening reads at a grid speed 15 of 23 -> <n>
DRIFT:      <0 if a criterion moved>
```

**Section 3 opens with the opening's text before and after**, then gives the per-read ratio and
speed beside it.

**Section 2 tells the owner in one paragraph** whether the first minute on a new frequency still
comes apart into E and T. If it does, say whether the estimator or the pitch is now the reason.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: unit estimator checks its dit cluster against its dah cluster - in CwUnitEstimator.Measure, when the long mark centroid stands more than 4.5 times the short one, the short heap is broken marks and the unit is taken as the long cluster's median over 3; traced per read on the opening, then built once and judged under 3.2's four tests, stream speed choice, grid, mixdown and tracker untouched
MOVE: work around
WHY: PHASE_PLAN.md 7.4 asks for a change against what 7.3 names. 7.3 named the speed falling to a 27.5 ms unit for a 55 ms sender, and that figure comes from CwUnitEstimator.Measure, which takes the short mark heap as the dit without checking it against the dahs. Every recorded 7.4 route acted downstream of that measurement: four mixdown follow rules, which R75 closes, and the stream's speed carry. This one corrects the measurement where it is made.
STATE: partial
DECIDED: author's, overrulable - the rule and its two numbers, 4.5 and 3, fixed before the trace; the one stop is a ratio test that fires on no read in the stream 30 to 46.2 s or yields no usable speed there; one narrower variant, returning not ready instead of rebuilding from the dah, only on a single-row or single-test failure; test 1 reads does not rise, as unit 430's arbiter decided; a change the opening's text does not move is not kept; 7.4 ticks only on a kept change, superseding P48's closure, otherwise P48 gains the measurement; the unit waits up to 120 minutes for unit 435's still-running first session to end and never kills it, builds on the HEAD it finds, and builds nothing if that session already ticked 7.4; unit 435's second session's launcher question is carried to the owner, not ruled. No self-ruling authorizes work outside the tasks.
LICENCE: PHASE_PLAN.md criteria 7.3 and 7.4, R64, R65, R66, R71, R73, R75, R76 and section 6; PARKED.md P42 and P48; unit 429's trace (003f2c98); unit 430's arbiter decision on test 1; ARBITER.md sections 4 and 6; HM-DEC-091; HM-DEC-139; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the first minute on a new frequency is read at the sender's real speed, because the decoder no longer takes pieces of broken marks for his dits, or the project knows the halving is not in the estimator's clustering
ADVANCES: step 7 criterion 4
END-ARBITER-DECISION
```
