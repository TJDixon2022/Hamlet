# Work instruction 437 - the window is re-mixed when the mixdown moves

**Authored on redirect 1 of this run: units 1 and 2 ran against 7.4 and neither moved it.**
Six routes are recorded against 7.4 as `no`:

- four rules about when the mixdown may follow the tracker, which R75 closes;
- the stream carrying its held speed (unit 435);
- the estimator checking its dit against its dah (unit 436).

R76 keeps `CwToneTracker` shut until 4.1 is met.

**This unit changes neither when the mix moves nor where it moves to. It changes what the
stream's window holds after the mix has moved.** Unit 436 measured the remaining fault from
36.5 to 41 s of the opening:

- the mix already stood at the sender's 625 Hz;
- a held speed of 17.6 to 21.8 WPM was already in force;
- each read's 12 s window still reached back into audio mixed at 525 Hz, 100 Hz off the
  sender;
- those reads gave `NI■ EA N ■`.

**The stream mixes each sample once, at the pitch in force when it arrived, and never again.**
This unit keeps the window's raw audio. When the mix moves, it re-mixes the whole held window at
the new pitch. Four tasks. Drop from the back.

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
- `WhatTheOpeningHeardTests` took 141 to 170 s.
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
- Put multi-step commands in `.run-unit\unit437-<name>.sh` and run them with `sh`.

## 3. Asks still outstanding

These are carried per HM-DEC-139. **None is this unit's to answer.**

- **P44 and P46** are the owner's and stay in `PARKED.md`.
- **The launcher should end a redirected session before it launches the next one.** Units 435
  and 436 both asked for this ruling. It is the owner's, and it is carried here, not made.
  Task 0 keeps this unit from colliding without it.
- **`TheUnitIsMeasuredNotSearchedTests.TheFiveToEightDecibelPlateauHolds` is red at HEAD.**
  - src and data have not changed since unit 436's entry, and the test file has not changed
    since `76b295cc`.
  - It is a standing red outside the carry-forward. This unit records it at entry and at exit
    and does not repair it.
  - It counts against HM-DEC-165 only if it was green at this unit's entry.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  After the mix moves onto the sender, the next read hears the whole window at the sender's pitch, not twelve seconds mixed somewhere else.
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

**What 7.3 named, and what unit 436 added.**

- 7.3 (unit 429, `003f2c98`): the opening reads a run of E and T at a mixing pitch off the
  sender, with the unit estimate halved and the grid choosing the speed.
- Units 430 to 433 showed the tracker's move to 525 Hz at 30.54 s. That move is R76's to
  answer.
- Unit 436 separated the part after that move:
  - from 36.5 s the mix stands at 625 Hz;
  - the window still holds up to 12 s of envelope computed at 525 Hz;
  - so every read to about 48 s decodes a window that is part the wrong pitch.
- **That residue is downstream of the tracker's choice and is not a rule about following it.**

**Where it happens.** `CwProbabilisticStream.Process` (`CwProbabilisticStream.cs` 239 to 269)
mixes each sample at `ToneHz` into `_mixedI` and `_mixedQ`. Those buffers hold only the
integrator's length.

- `PushEnvelope` (350 to 407) turns them into one envelope hop.
- It pushes the hop onto `_envelope`, which is 12 s of hops, `WindowSeconds` at 31.
- `CwDecoder.cs` 617 to 621 sets `ToneHz`.
- **Once a hop is in `_envelope` it is never recomputed.** The raw audio it came from is gone.

**Why this is not the window clear that was ruled off** (remarks at 158 to 171 and 397 to
400):

- A clear throws the window away and reads nothing until it refills.
- This keeps every hop and the refill guard exactly as they are.
- It only makes the hops agree with the pitch the stream now mixes at.

---

## 5. Verify this instruction against the tree

Check each item, report any mismatch, and repair nothing:

- **The stream.**
  - `Process` is at `CwProbabilisticStream.cs` 239 to 269 and `PushEnvelope` at 350 to 407.
  - `_mixedI` and `_mixedQ` are `_windowSamples` long, the integrator's length. They are not
    12 s long.
  - `_envelope` is `_windowHops` long, 12 s at `HopMilliseconds` 5.0.
  - No raw sample is kept.
- **The mixdown.** `CwDecoder.cs` 617 to 621 is the only place in `src` that sets the stream's
  `ToneHz`.
- **The numbers to hold** (unit 436's exit, `3980e2a8`):
  - keyed recordings: 165 edits over 565 characters, against inferred keys;
  - 17:37: 19 edits over its 25-character scored region;
  - 17 added letters, 8 of them single-element;
  - captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13;
  - engine 178 of 178;
  - the stream from 30 to 46.2 s: 22 named, `UIEH EE E E T I NIEEE E E ET N ■IK`;
  - `003901` cold: 9 named, `EII E T NHHK`;
  - `003919` cold: 25 named, `EITEETNXNIK EANQNID EANQNIK`.
- **Anything kept since then.** If HEAD is not `3980e2a8` plus the launcher's root files, name
  what came in, and take the entry round's numbers as the ones to hold.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R76, §3 and §6. The ones this unit leans on:

- **R76.** No unit changes `CwToneTracker` until 4.1 is met. **This unit does not open it.**
- **R75.** The move at 30.54 s is the tracker's choice, and no follow rule undoes it.
  - **This unit changes no follow rule.** When the mix moves, and to where, stays exactly as at
    entry.
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
- **§0.2** Nothing that keys or transmits is touched.
- **§12.5** and **§12.6** Repair nothing on the way past.
- **HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**The author's decisions for this unit, overrulable** (also in DECIDED). They are fixed now,
before the trace.

- **The change.** `CwProbabilisticStream` keeps the raw samples behind its window: 12 s plus
  the integrator's length.
  - When `ToneHz` stands 25 Hz or more from the pitch the held window was last mixed at, the
    stream re-mixes the held raw audio at the new pitch before the next hop is pushed.
  - It recomputes `_mixedI`, `_mixedQ` and every hop in `_envelope` with the same taper and the
    same integrator.
  - The envelope is then exactly what it would have been had the mix stood at the new pitch
    through the whole window.
  - Moves of less than 25 Hz behave as at entry. 25 Hz is the plan's own tolerance, from 4.1
    and R76.
- **What stays exactly as at entry:**
  - `_hopsSeen`, `_envelopeCount`, the refill guard and the read cadence;
  - the settled count and every character already settled;
  - the speed choice, `CwUnitEstimator`, the gap structure and the grid;
  - `CwDecoder`, `CwToneTracker` and `CwToneSurvey`.
- **The one stop.** Task 1 replays the change offline first. If, on the stream from 30 to
  46.2 s, the re-mixed window settles the same characters as the entry on every read, build
  nothing: the change cannot move 7.4.
- **One narrower variant**, only if the change fails on exactly one row or exactly one test:
  - the same re-mix, but it fires only on a move that has stood for two consecutive reads;
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

1. Look for a live chain from an earlier unit. Check `ps` for `unit435-` and `unit436-`
   scripts, and for `dotnet test` or `testhost` processes.
2. **If one is live, wait for it to end. Do not kill it.**
   - Check `ps` every few minutes, with no test of your own running, for up to 120 minutes.
   - If it is still live after 120 minutes, stop at task 0 with nothing changed. Say so in
     section 4.
3. Once nothing is live, read `git log` and 7.4 in `PHASE_PLAN.md`.
   - If 7.4 is already ticked, build nothing. Run the exit round and report.

**Then the record:**

- Give `PHASE_OUTCOME.md` its `## UNIT 437 - STEP 7` entry from the decision block at the foot
  of this file.
- Make `PHASE_STATUS.md` name unit 437, *the window is re-mixed when the mixdown moves*, and
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
- `TheUnitIsMeasuredNotSearchedTests`, recorded only.

**Drop candidate:** none.

### Task 1 - the trace (7.4)

Add a fact to `WhatTheOpeningHeardTests` that asserts nothing and writes nothing.

**The replay.** Replay the change offline over the spliced stream from 27.0 to 46.2 s and over
`003901` and `003919` cold. Nothing in `src` changes in this task.

- Build a second envelope window at each read.
- Where the mix moved 25 Hz or more since the held window was last mixed, re-mix that window's
  own raw audio at the new pitch.
- Read it with the same `CwUnitEstimator.Measure` and the same decode.

**Print one row per read**, with these columns:

- the stream time;
- the mix pitch;
- whether a re-mix fired, and from what pitch;
- how much of the window was mixed off the current pitch at entry, in seconds;
- the entry's estimator unit, WPM and speed source;
- the same three under the re-mix;
- the characters each read would settle, at entry and under the re-mix.

**Then count the moves.** Over all 51 capture rows and every keyed recording, count per
recording how many mix moves of 25 Hz or more occur. Name every recording where one occurs.
The count is a forecast for the four tests. **It is not a gate.**

**Then the cost.** Measure the cost of one re-mix of a full window, in milliseconds, on this
machine, and state it beside the stream's hop budget of 5 ms.

**Then apply the one stop.** If the re-mixed window settles the same characters as the entry on
every read from 30 to 46.2 s, build nothing. Print the per-read rows, say so, and go to task 3.
Otherwise go on to task 2.

**Drop candidate:** the move count over the other recordings. Do not drop the per-read rows.

### Task 2 - build it and judge it (7.4)

Build the change in `CwProbabilisticStream.cs` only, in its own commit, exactly as section 6
states it. Give it a remark in the file's own style saying why:

- unit 436's window reaching back into 525 Hz;
- why this is not the window clear that was ruled off.

**Then run 3.2's four tests and print each one as a number:**

1. total edits over all keyed recordings, before and after, with 17:37's edits over its 25
   characters and the added-letter count;
2. all 13 named floors;
3. the three adjudicated readings, quoted before and after;
4. all 51 capture rows' above-bar counts and elements, before and after.

**Then the criterion's own figure, text beside count.** Print each of these whole, before and
after, and state its named count:

- the stream from 30 to 46.2 s;
- `003901` cold, its opening 60 s;
- `003919` cold, its opening 60 s.

Print the opening's speed per read under the change beside task 1's entry column. Then answer
unit 436's question either way: **with the window at the sender's pitch, does the opening from
36.5 s read the sender's text?**

**Keep or take out.** A change is kept only if all four tests pass and the opening's text moves.

- **A change that fails any test goes back out in the next commit.** The report says which
  test, which row and which number.
- **If it fails on exactly one row or one test,** build section 6's variant once, in its own
  commit, and judge it the same way.
- **A change that passes but leaves the opening exactly as at entry goes back out.** The report
  says so.

**When a change is kept:**

- tick 7.4 in `PHASE_PLAN.md`;
- note on step 7's line in `PHASE_STATUS.md` that P48's closure is superseded;
- report the cost of a re-mix measured in task 1.

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
  `CwUnitEstimator.cs` against this unit's entry prints nothing.
- If nothing was kept, `git diff` over all of `src` and `data` against entry prints nothing.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **`CwToneTracker`, the tone survey, and when or where the mixdown moves** (`CwDecoder.cs` 617
  to 621). These belong to R76 and to 4.1 to 4.7.
- **The stream's speed choice at `CwProbabilisticStream.cs` 428 to 435, the grid and
  `CwUnitEstimator`.** These are units 435 and 436's approaches, both recorded.
- **The speed range 8 to 40.** It belongs to 7.1 and 7.2.
- **The refill guard, `RefillSeconds`, the window clear, the hysteresis depth,
  `ShortestRunHops` and `MeasureGaps`.**
- **3.6 the stray letters, 6.5 the dead button, 7.8 the preamp.**
- **P27, P37, P38, P40, P41, and P43 to P48.** This unit only appends to P48.
- **The plateau red.** Record it and do not repair it.

## 10. What not to do

- **Do not stop, kill or signal another session's processes.** Wait, as task 0 says.
- **Do not change when or where the mix moves.**
- **Do not touch the tracker, the survey, the estimator, the speed choice, or anything that keys
  or transmits.**
- **Do not empty the window, shorten it, or change the refill guard.** Re-mix it only.
- **Do not tune 25 Hz after the trace**, and build no more than the one variant.
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
B. Step 7, criterion 7.4: whether re-mixing the held window at the new pitch was kept,
   3.2's four tests as numbers, and the opening - stream 30 to 46.2 s, 003901 and 003919
   cold - before and after as text with named counts.
C. The rest, weighed against A and B. Section 4 raises <n> items; say whether any stands in
   the way of 7.4.
```

```
UNIT:       437 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     stream 30 to 46.2 s: 22 named <text> -> <n> named <text>; keyed 165 over 565 -> <n>; re-mixes fired on the opening <n>, cost <ms> each
DRIFT:      <0 if a criterion moved>
```

**Section 3 opens with the opening's text before and after**, then gives the per-read rows of
task 1 beside the built change's speeds.

**Section 2 tells the owner in one paragraph** whether the minute after the decoder finds the
sender's pitch now reads the sender. If it does not, say what is left in the way: the tracker's
wrong move, which R76 holds behind 4.1, or something this unit measured.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: re-mix the stream's held window at the new pitch when the mixdown moves - CwProbabilisticStream keeps 12 s of raw audio and, on a move of 25 Hz or more, recomputes the mixed arms and every envelope hop in the window at the new pitch; when and where the mix moves, the refill guard, speed choice, estimator and tracker untouched; replayed offline on the opening first, then built once and judged under 3.2's four tests
MOVE: work around
WHY: PHASE_PLAN.md 7.4 needs a change against what 7.3 named. Every recorded route acted on when the mix moves or on how speed is measured; unit 436 measured that from 36.5 s the mix and speed are already right, yet each read's 12 s window still decodes audio mixed 100 Hz off the sender, because the stream mixes each sample once and never again. This re-mixes that window, and it touches neither the tracker's choice (R76) nor any follow rule (R75).
STATE: partial
DECIDED: author's, overrulable - the change, its 25 Hz threshold taken from the plan's own tolerance, and its scope (CwProbabilisticStream.cs only, window re-mixed but never emptied or shortened) fixed before the trace; the one stop is an offline replay in which the re-mixed window settles the same characters as the entry on every read from 30 to 46.2 s; one narrower variant, re-mixing only on a move that has stood for two reads, only on a single-row or single-test failure; the plateau red TheFiveToEightDecibelPlateauHolds is recorded at entry and exit and not repaired; the unit waits up to 120 minutes for any live earlier session and never kills it. The report's proposal to hold 7.4 until 4.7 was not taken, because the redirect requires 7.4 and this route is untried. Carried, not ruled: the launcher-overlap ruling, which is the owner's. Standing, not re-ruled: test 1 reads does not rise (unit 430's arbiter), and 7.4 ticks only on a kept change that moves the opening's text (unit 436's arbiter). No self-ruling authorizes work outside the tasks.
LICENCE: PHASE_PLAN.md criteria 7.3 and 7.4, R64, R65, R66, R71, R73, R75, R76 and section 6; PARKED.md P48 with unit 436's measurement of the window reaching back into 525 Hz; unit 429's trace (003f2c98); ARBITER.md sections 4 and 6; HM-DEC-091; HM-DEC-120; HM-DEC-139; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: once Hamlet has found the sender's pitch, the next reads hear the whole window at that pitch and read the sender, or the project knows that stale audio in the window is not what breaks the opening
ADVANCES: step 7 criterion 4
END-ARBITER-DECISION
```
