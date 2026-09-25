# Work instruction 438 - the unit's trigger is cut where the marks are

**Authored on redirect 1 of this run: units 2 and 3 ran against 7.4 and neither moved it.**
Seven routes are recorded against 7.4 as `no`:

- four rules about when the mixdown may follow the tracker, which R75 closes;
- the stream carrying its held speed (unit 435);
- the estimator checking its dit cluster against its dah cluster (unit 436);
- the stream re-mixing its held window at the new pitch (unit 437).

R76 keeps `CwToneTracker` shut until 4.1 is met.

**What unit 437 settled.** With the window wholly at the sender's 625 Hz from 34.5 s, the
estimator still read a unit of 27.5 to 37.5 ms against the sender's 55 ms, and the opening
still did not read the sender. So the halving does not come from the pitch or from the
window's history. Unit 436 had already found where it shows: the short mark heap sits at 19
to 34 ms and the short gap heap at 10 to 15 ms. **Those are marks cut into pieces.**

**This unit changes where the trigger cuts, and nothing else.** `CwUnitEstimator.Elements`
takes one Otsu level over the whole 12 s window (`CwUnitEstimator.cs` 398 to 405). On the
opening, that window holds a splice at 30.0 s, the tail of `003901`, and a sender whose level
moves. So one cut over 12 s can stand above the sender's own mark level in part of the window.
There, every shallow dip inside a mark falls below the off level and becomes two short marks
and a short gap.

**The change: in `Measure` only, the cut is taken from the 3 s around each stretch rather than
from all 12 s.** The hysteresis depth, the shortest run, the clustering, `MeasureGaps` and
`MeasureCharacterGap` stay exactly as at entry. Four tasks. Drop from the back.

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
`docs\carry-forward-tests.txt`, as its top comment says.

- **Never background a test and poll it.**
- Run one type per invocation, each with its own `timeout`.
- The captures type has 51 rows and takes about 120 s.
- `WhatTheOpeningHeardTests` took 141 to 170 s at unit 436. With unit 437's two printers it may
  take longer.
- Give each 600 s and report what it took.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

- **The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>` or `^`.
- **`ADVANCES` reads exactly `step 7 criterion 4`.**
- **Write `output.md` at the root before the session ends.**
- **Nothing in section 4 halts this phase.** Park it and go on.

## 2. The tool facts

- Apostrophes in quoted heredocs break them.
- Doubled backslashes collapse.
- `;` and `rm` are refused.
- Python cannot run here.
- A multi-line commit message needs more than one `-m`.
- A bare `git worktree`, `git checkout` or `git show` is refused at the prompt.
- Put multi-step commands in `.run-unit\unit438-<name>.sh` and run them with `sh`.

## 3. Asks still outstanding

These are carried per HM-DEC-139. **None is this unit's to answer.**

- **P44 and P46** are the owner's and stay in `PARKED.md`.
- **The launcher should end a redirected session before it launches the next one.** Units 435,
  436 and 437 carried this ruling. It is the owner's, and it is carried here, not made. Task 0
  keeps this unit from colliding without it.
- **`TheUnitIsMeasuredNotSearchedTests.TheFiveToEightDecibelPlateauHolds` is red at HEAD.** It
  was red at unit 437's entry and exit.
  - It is a standing red outside the carry-forward.
  - **This unit touches the code that test measures.** Record it at entry, after the build, and
    at exit, and print its figures each time. Do not repair it.
  - It counts against HM-DEC-165 only if it was green at this unit's entry.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  The estimator hears the sender's marks whole in the opening, so the unit it measures is the sender's dit and not half of it.
ADVANCES:   step 7 criterion 4
DRIFT:      0
```

**The count today.**

- Step 7: 7.3, 7.5 and 7.7 are ticked. 7.1, 7.2, 7.4, 7.6 and 7.8 are open.
- P48 records 7.4 as closed partial on step 7's line, but **7.4 is not ticked**.
- It ticks only on a kept change that moves the opening's text. That was unit 436's arbiter
  decision and it stands.

**The plan line.** 7.4: *a change against what 7.3 names is judged under 3.2's four tests, and
the named characters read in the opening 60 seconds of `cw-2026-09-24-003901` and `-003919` are
reported before and after.*

**What 7.3 named.** Unit 429 (`003f2c98`) found that in the opening:

- the decoder mixes off the sender's pitch;
- the unit estimate is halved;
- the grid chooses the speed.

The pitch is R76's. Unit 437 showed that correcting the window's pitch does not repair the
halving. **The halved unit is what this unit attacks, at its input and not at its clustering.**

**Where it happens.**

- `CwProbabilisticStream.cs` 428 calls `CwUnitEstimator.Measure`. It is the only caller in
  `src`.
- `Measure` (77 to 106) calls `Elements` (391 to 406).
- `Elements` takes one `Otsu` level over every hop it is given (417 to 488).
- `Runs` (491 onward) splits the hops into marks and gaps at that level plus and minus
  `HysteresisDb`, 6 dB.
- `ShortClusterMedian` then takes the short mark heap and the short gap heap. Unit 436 found
  those at 19 to 34 ms and 10 to 15 ms on the opening, against 55 ms.

**Why this is not unit 436's route.** Unit 436 kept the pieces and changed which heap was
believed. This unit changes nothing about the heaps. It changes the level the pieces were cut
at. If the cut was the cause, the pieces are never made.

---

## 5. Verify this instruction against the tree

Check each item, report any mismatch, and repair nothing:

- **The estimator.**
  - `Measure` is at `CwUnitEstimator.cs` 77 to 106 and calls `Elements` once.
  - `Elements` is at 391 to 406 and calls `Runs(db, Otsu(db), ...)` with one cut for the whole
    envelope.
  - `MeasureGaps` (174) and `MeasureCharacterGap` (272) also call `Elements`.
  - `HysteresisDb` is 6.0 and `ShortestRunHops` is 2.
- **The caller.** `CwProbabilisticStream.cs` 428 is the only call to `CwUnitEstimator.Measure`
  in `src`.
- **The numbers to hold** (unit 437's exit, `2a948f9b`, identical to unit 436's):
  - keyed recordings: 165 edits over 565 characters, against inferred keys;
  - 17:37: 19 edits over its 25-character scored region;
  - 17 added letters, 8 of them single-element;
  - captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13;
  - engine 178 of 178;
  - `TheUnitIsMeasuredNotSearchedTests` 4 of 5, the plateau red;
  - the stream from 30 to 46.2 s: 22 named, `UIEH EE E E T I NIEEE E E ET N ■IK`;
  - `003901` cold: 9 named, `EII E T NHHK`;
  - `003919` cold: 25 named, `EITEETNXNIK EANQNID EANQNIK`.
- **Anything kept since then.** If HEAD is not `66a99377` plus the launcher's root and
  `.run-unit` files, name what came in, and take the entry round's numbers as the ones to hold.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R76, §3 and §6. The ones this unit leans on:

- **R76.** No unit changes `CwToneTracker` until 4.1 is met. **This unit does not open it.**
- **R75.** The move at 30.54 s is the tracker's choice, and no follow rule undoes it. **This
  unit changes no follow rule.**
- **3.2's four tests, with R66's third.** A change is kept only if all four hold:
  1. Total edits over all keyed recordings **do not rise**. Unit 430's arbiter set this for
     7.4, because no key scores the opening.
  2. No named floor from 2.2 breaks.
  3. The three adjudicated readings are unchanged, or move onto exactly their own adjudicated
     text. Print them before and after.
  4. No capture row's count at or above the span bar falls.
- **R71.** The floors count characters at or above raw span 13.0.
- **R73.** A key-aligned added character inside a scored stretch may leave a floor. Name each
  one with its recording. This applies nowhere else.
- **7.4 ticks only on a kept change that moves the opening's text** (unit 436's arbiter).
- **HM-DEC-091.** A change that reads one recording and costs another is not a fix.
- **HM-DEC-120.** Less evidence means silence. The refill guard stays as it is.
- **HM-DEC-165.** Nothing is red at exit that was green at entry. That includes
  `TheUnitIsMeasuredNotSearchedTests.ItRecoversASpeedItWasNeverTold`, which this change runs
  through.
- **§0.2** Nothing that keys or transmits is touched.
- **§12.5** and **§12.6** Repair nothing on the way past.
- **HM-DEC-155**, **FACT-004**, **FACT-006**.

**The author's decisions for this unit, overrulable** (also in DECIDED). They are fixed now,
before the trace.

- **The change.** In `CwUnitEstimator.Measure` only, the marks and gaps come from a cut that is
  local in time.
  - The envelope in dB is split into blocks of 0.5 s, 100 hops at 5 ms. That is the stream's
    read cadence.
  - Each block's cut is `Otsu` over the 3.0 s of hops centred on that block. At the window's
    ends, the 3.0 s span is slid inward so it stays whole.
  - Where that span's two Otsu classes have means less than twice `HysteresisDb` apart, the
    span holds no keying. That block takes the whole window's cut, exactly as at entry.
  - `Runs` then applies each hop's own block cut, plus and minus `HysteresisDb`. The hysteresis
    state carries across block boundaries.
  - Everything after `Runs` is exactly as at entry.
  - 3.0 s is the survey's own span and `RefillSeconds`. At the sender's 22 WPM, it holds about
    ten characters. It is not tuned after the trace.
- **What stays exactly as at entry:**
  - `HysteresisDb`, `ShortestRunHops`, `ShortClusterMedian` and `TwoMeansOnLogs`;
  - `Elements` as other callers see it, `MeasureGaps` and `MeasureCharacterGap`;
  - the stream's speed choice, grid, refill guard, read cadence and window;
  - `CwDecoder`, the mixdown, `CwToneTracker` and `CwToneSurvey`.
- **The one stop.** Task 1 replays the change offline first. **Build nothing** if either of
  these holds:
  - on the stream reads from 34.5 to 44.5 s, the local cut's unit stays below 40 ms on every
    read;
  - over 30 to 46.2 s, the local cut settles the same characters as the entry on every read.

  Either way, the trigger is not what halves the unit.
- **One narrower variant**, only if the change fails on exactly one row or exactly one test:
  - the same local cut, over 6.0 s spans instead of 3.0 s;
  - nothing else changes;
  - nothing is tuned after the trace.
- **A change that passes the four tests but leaves the opening's text exactly as at entry is
  not kept.** It changed nothing 7.4 asks about.
- **If nothing is kept,** P48 gains this unit's measurement and 7.4 stays unticked.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - no second session, then the record

**First, before any build or edit:**

1. Look for a live chain from an earlier unit. Check `ps` for `unit435-`, `unit436-` and
   `unit437-` scripts, and for `dotnet test` or `testhost` processes.
2. **If one is live, wait for it to end. Do not kill it.**
   - Check `ps` every few minutes, with no test of your own running, for up to 120 minutes.
   - If it is still live after 120 minutes, stop at task 0 with nothing changed. Say so in
     section 4.
   - A claude.exe process that starts no test and commits nothing is not a chain. Note it and
     go on, as unit 437 did.
3. Once nothing is live, read `git log` and 7.4 in `PHASE_PLAN.md`.
   - If 7.4 is already ticked, build nothing. Run the exit round and report.

**Then the record:**

- Give `PHASE_OUTCOME.md` its `## UNIT 438 - STEP 7` entry from the decision block at the foot
  of this file.
- Make `PHASE_STATUS.md` name unit 438, *the unit's trigger is cut where the marks are*, and
  `CURRENT_STEP: 7`.
- Patch-bump `Directory.Build.props` from whatever HEAD holds.
- Commit the launcher's modified root files as they stood.

**Then the entry round**, one type per invocation. Record every number from section 5 as the
numbers to hold:

- the build, with warnings as errors;
- both carry-forward lines;
- the captures, adjudicated and keyed-floor tests;
- the keyed totals;
- `WhatTheStrayLettersRestOnTests`;
- `WhatTheOpeningHeardTests`;
- `TheUnitIsMeasuredNotSearchedTests`, every fact with its figures.

**Drop candidate:** none.

### Task 1 - the trace (7.4)

Add a fact to `WhatTheOpeningHeardTests` that asserts nothing and writes nothing.

**The replay.** Replay the change offline over the spliced stream from 27.0 to 46.2 s, over the
locked `004108` stretch from 13.8 to 30 s, and over `003901` and `003919` cold. Nothing in `src`
changes in this task.

- At each read, take the stream's own 12 s envelope.
- Measure it twice: once with `Measure` as at entry, and once with section 6's local cut.
- Decode it with each unit through the stream's own speed choice.

**Print one row per read**, with these columns:

- the stream time and the mix pitch;
- the whole window's cut in dB;
- the lowest and highest block cut in dB;
- the count of blocks that fell back to the whole window's cut;
- for the entry and for the local cut:
  - the mark count;
  - the short mark median;
  - the short gap median;
  - the unit;
  - WPM and speed source;
  - the characters that read would settle.

**Then name the pieces.** For the reads from 34.5 to 44.5 s, print every mark shorter than
40 ms at entry. For each one, give:

- its time;
- its length;
- the gap after it;
- whether the local cut joins it to its neighbor.

**Then the cost.** Measure the cost of one local-cut `Measure` over a full 2400-hop window, in
milliseconds, on this machine. State it beside the entry's cost and the 0.5 s read cadence.

**Then apply the one stop** exactly as section 6 states it. If it holds, build nothing. Print
the per-read rows, say so, and go to task 3. Otherwise go on to task 2.

**Drop candidate:** the `004108` and cold rows. Do not drop the stream rows or the pieces.

### Task 2 - build it and judge it (7.4)

Build the change in `CwUnitEstimator.cs` only, in its own commit, exactly as section 6 states
it. Give it a remark in the file's own style saying why:

- unit 436's short heaps at 19 to 34 ms and 10 to 15 ms;
- unit 437's halving on a window wholly at the sender's pitch;
- why one cut over 12 s chops a mark when the level moves.

Update the class remark that calls the hysteresis depth the one constant not measured from the
audio: 3.0 s is now a second one. Say so plainly.

**Then run 3.2's four tests and print each one as a number:**

1. total edits over all keyed recordings, before and after, with 17:37's edits over its 25
   characters and the added-letter count;
2. all 13 named floors;
3. the three adjudicated readings, quoted before and after;
4. all 51 capture rows' above-bar counts and elements, before and after.

Then run `TheUnitIsMeasuredNotSearchedTests` and print every fact beside its entry. If
`ItRecoversASpeedItWasNeverTold` goes red, the change goes back out under HM-DEC-165.

**Then the criterion's own figure, text beside count.** Print each of these whole, before and
after, and state its named count:

- the stream from 30 to 46.2 s;
- `003901` cold, its opening 60 s;
- `003919` cold, its opening 60 s.

Print the opening's unit and speed per read under the change, beside task 1's entry column.
Then answer this question either way: **with the marks cut where they stand, does the opening
from 34.5 s read at the sender's 22 WPM, and does it read the sender's text?**

**Keep or take out.** A change is kept only if all four tests pass, nothing green at entry is
red, and the opening's text moves.

- **A change that fails any test goes back out in the next commit.** The report says which
  test, which row and which number.
- **If it fails on exactly one row or one test,** build section 6's variant once, in its own
  commit, and judge it the same way.
- **A change that passes but leaves the opening exactly as at entry goes back out.** The report
  says so.

**When a change is kept:**

- tick 7.4 in `PHASE_PLAN.md`;
- note on step 7's line in `PHASE_STATUS.md` that P48's closure is superseded;
- report the cost measured in task 1.

**When nothing is kept,** append this unit's measurement to P48.

**Drop candidate:** the variant.

### Task 3 - the exit round

**Run all of these:**

- the build, with warnings as errors;
- both carry-forward lines;
- the three floor tests;
- the keyed totals;
- `WhatTheStrayLettersRestOnTests`;
- `WhatTheOpeningHeardTests`;
- `TheUnitIsMeasuredNotSearchedTests`, recorded beside its entry;
- every type touched.

**Then check the diffs:**

- `git diff` over the transmit files against `7e209cb4` prints nothing.
- `git diff` over `CwToneTracker.cs`, `CwToneSurvey.cs`, `CwDecoder.cs` and
  `CwProbabilisticStream.cs` against this unit's entry prints nothing.
- If nothing was kept, `git diff` over all of `src` and `data` against entry prints nothing.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **`CwToneTracker`, the tone survey, and when or where the mixdown moves** (`CwDecoder.cs` 617
  to 621). These belong to R76 and to 4.1 to 4.7.
- **The stream's speed choice at `CwProbabilisticStream.cs` 428 to 435, the grid, and the window
  re-mix.** These are units 435 and 437's approaches, both recorded.
- **The short-heap clustering: `ShortClusterMedian`, `TwoMeansOnLogs`, and any dit-against-dah
  rule.** That is unit 436's approach, recorded.
- **`HysteresisDb`'s value and `ShortestRunHops`.** This unit moves the level, not the depth
  around it.
- **`MeasureGaps`, `MeasureCharacterGap`, and `Elements` as they see it.**
- **The speed range 8 to 40.** It belongs to 7.1 and 7.2.
- **The refill guard, `RefillSeconds` and the window clear.**
- **3.6 the stray letters, 6.5 the dead button, 7.8 the preamp.**
- **P27, P37, P38, P40, P41, and P43 to P48.** This unit only appends to P48.
- **The plateau red.** Record it and do not repair it.

## 10. What not to do

- **Do not stop, kill or signal another session's processes.** Wait, as task 0 says.
- **Do not change when or where the mix moves, or what the stream's window holds.**
- **Do not touch the tracker, the survey, the stream, the speed choice, or anything that keys
  or transmits.**
- **Do not change the hysteresis depth, the shortest run or the clustering.** Move the cut only,
  and in `Measure` only.
- **Do not tune 3.0 s after the trace**, and build no more than the one variant.
- **Do not keep a change that does not move the opening's text.**
- **Do not lower an above-bar count on any row, or a named floor.**
- **Do not halt for a question.** Park it.
- **Do not run an unfiltered `dotnet test`, background a test and poll it, or compose a
  timestamp.**
- **Report mismatches and repair nothing.** Use American spelling, UTF-8, and the four headings
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
B. Step 7, criterion 7.4: whether the unit estimate's local trigger cut was kept, 3.2's four
   tests as numbers, and the opening - stream 30 to 46.2 s, 003901 and 003919 cold - before
   and after as text with named counts.
C. The rest, weighed against A and B. Section 4 raises <n> items; say whether any stands in
   the way of 7.4.
```

```
UNIT:       438 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     stream 30 to 46.2 s: 22 named <text> -> <n> named <text>; unit 34.5 to 44.5 s <entry ms range> -> <ms range>; keyed 165 over 565 -> <n>
DRIFT:      <0 if a criterion moved>
```

**Section 3 opens with the opening's text before and after.** Then it gives task 1's per-read
rows, with the unit at entry and under the local cut, then the pieces.

**Section 2 tells the owner in one paragraph** whether the decoder now measures the sender's
speed in the first minute. Say whether that makes the opening read. If it does not, say what is
left in the way: the tracker's wrong move, which R76 holds behind 4.1, or something this unit
measured.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: cut the unit estimator's trigger locally in time - in CwUnitEstimator.Measure only, the Otsu level is taken per 0.5 s block over the 3 s around it instead of once over the whole 12 s window, falling back to the window's cut where a span holds no keying, so marks are not chopped into half-unit pieces where the level moves; hysteresis depth, shortest run, clustering, MeasureGaps, stream, mixdown and tracker untouched; replayed offline on the opening first, then built once and judged under 3.2's four tests
MOVE: work around
WHY: PHASE_PLAN.md 7.4 needs a change against what 7.3 named, and the redirect requires 7.4. Unit 437 measured that the estimator still halves the unit on a window wholly at the sender's pitch, and unit 436 measured that halving as marks cut into 19 to 34 ms pieces with 10 to 15 ms gaps. Every recorded route changed the pitch, the speed carry, the clustering or the window's history. None changed the level the pieces are cut at, which is one Otsu cut over 12 s across a splice and a moving sender level.
STATE: partial
DECIDED: author's, overrulable - the change, its 3.0 s span and 0.5 s block, taken from the survey's span, RefillSeconds and the read cadence; the fallback to the whole window's cut when a span's classes are less than twice HysteresisDb apart; its scope (Measure only, CwUnitEstimator.cs only, Elements unchanged for MeasureGaps and MeasureCharacterGap). All are fixed before the trace. The one stop is an offline replay in which the local cut's unit stays below 40 ms on every read from 34.5 to 44.5 s, or settles the same characters as the entry on every read from 30 to 46.2 s. There is one narrower variant, 6.0 s spans, built only on a single-row or single-test failure. ItRecoversASpeedItWasNeverTold going red takes the change out under HM-DEC-165. The plateau red is recorded at entry, after the build and at exit, and not repaired. The unit waits up to 120 minutes for any live earlier session and never kills it. Taken from unit 437's section 4 item 3 as the route and not as the target: look at the trigger rather than hold 7.4 until 4.7, because the redirect requires 7.4 and this route is untried. Carried, not ruled: the launcher-overlap ruling, which is the owner's. Standing, not re-ruled: test 1 reads does not rise (unit 430's arbiter), and 7.4 ticks only on a kept change that moves the opening's text (unit 436's arbiter). No self-ruling authorizes work outside the tasks.
LICENCE: PHASE_PLAN.md criteria 7.3 and 7.4, R64, R65, R66, R71, R73, R75, R76 and section 6; PARKED.md P48 with unit 436's short-heap measurement and unit 437's halving on a clean window; unit 429's trace (003f2c98); ARBITER.md sections 4 and 6; HM-DEC-091; HM-DEC-120; HM-DEC-139; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: in the first minute on a frequency, Hamlet measures the sender's own speed instead of half of it, and reads the sender. Otherwise the project knows the halving is in the audio itself and not in where the decoder draws the line between key-down and key-up.
ADVANCES: step 7 criterion 4
END-ARBITER-DECISION
```
