# Work instruction 435 - the speed holds when the unit estimate halves

**Authored on redirect 6: units 432 and 433 ran against 7.4 and neither moved it.** All four
rules tried at 7.4 asked one question: *when may the mixdown follow the tracker's pitch move?*
R75 has closed that question - on the opening, the decoder's own envelope ranks the wrong pitch
highest, so no follow rule can undo a choice the evidence supports - and R76 keeps
`CwToneTracker` shut until 4.1 is met. **This unit leaves the mixdown and the tracker alone
and attacks the second figure 7.3 named: the speed.** Four tasks, drop from the back.

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

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run as
its top comment says. **Never background and poll.** One type per invocation, each with its
own `timeout`. The captures type is 51 rows and runs about 120 s; `WhatTheOpeningHeardTests`
ran 170 s at unit 433. Give each 600 s and report what it took.

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

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit435-<name>.sh` and run with `sh`.

## 3. Asks still outstanding

Carried per HM-DEC-139. **None is this unit's to answer.** P44 (a join judged by the letters
it leaves) and P46 (the replay gate's third clause) are the owner's and stay in `PARKED.md`.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  The opening of a session stops coming apart into E and T when the speed estimate halves for a second.
ADVANCES:   step 7 criterion 4
DRIFT:      0
```

**The count today.** Step 7: 7.3, 7.5, 7.7 ticked; 7.1, 7.2, 7.4, 7.6, 7.8 open. 7.4 was
recorded closed partial by unit 433 (P48) and is **not ticked**; it ticks on a kept change.

**The plan line.** 7.4: *a change against what 7.3 names is judged under 3.2's four tests, and
the named characters read in the opening 60 seconds of `cw-2026-09-24-003901` and `-003919` are
reported before and after.*

**What 7.3 named - two figures, in order** (unit 429, `003f2c98`, `WhatTheDecoderHeldAtEachCharacter`):

1. **The mix pitch**, `CwDecoder.cs` 617 to 621: 525 Hz in the opening against 625 locked.
   Four rules about when it follows have been measured and none kept (P48). R75 and R76 close
   that route until 4.1 is met.
2. **The speed**, `CwProbabilisticStream.cs` 428 to 435. Within a second of the mix moving,
   the estimator's unit fell from about 50 ms to 27.5 ms - 43.6 WPM, above the 40 ceiling - so
   the estimator was set aside and the grid (`CwProbabilisticDecoder.cs` 756 to 771) chose
   **38 WPM for a sender at 22 to 24**. 15 of the opening's 23 characters were read at a grid
   speed; the locked stretch took all of its speeds from the estimator at 21.8 WPM, 55 ms. *"At
   that speed the sender's marks came apart into E, I and T."*

**Nobody has built against the second figure.** A sender does not halve his dit between one
read and the next; the decoder's speed did, on a single window, and nothing held it. Unit
429 warned that the speed repaired alone still decodes through a filter 100 Hz off the sender;
**that is a hypothesis, and this unit measures it.**

**Nothing here touches the tracker, the tone survey, the mixdown line or the range 8 to 40.**

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- `CwProbabilisticStream.cs` 428 to 435 takes the estimator's speed when it is ready and inside
  `SlowestWpm` to `FastestWpm`, else null, and null sends the read to the grid. Name the line
  where null becomes the grid search.
- Whether the stream already carries any speed from one read to the next - a field, a held
  gap reading's unit, anything. **If a speed is already carried, say where**; the rule below is
  then built on it, not beside it.
- The read cadence: how much new audio lies between consecutive `Read` calls, so "3.0 s" and
  "two consecutive reads" below can be stated in reads.
- `WhatTheOpeningHeardTests` holds `WhatTheDecoderHeldAtEachCharacter` and the spliced stream
  from 0 to 46.2 s; unit 433 counted 15 facts in the type.
- Whether `FastestWpm` is still 40 at HEAD. If a 7.1 unit has changed it, say so; this rule
  reads the constant and does not care what it is.
- The numbers to hold (P48): 165 edits over 565 on all keyed recordings against inferred keys;
  17:37 19 over 25; 17 added, 8 single-element; captures 51 of 51 with `032113` at 47; adjudicated
  13 of 13; keyed floors 13 of 13; the stream 30 to 46.2 s 22 named,
  `UIEH EE E E T I NIEEE E E ET N ■IK`; `003901` cold 9 named `EII E T NHHK`; `003919` cold 25
  named `EITEETNXNIK EANQNID EANQNIK`.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R76, §3 and §6. The ones this unit leans on:

- **R76.** No unit changes `CwToneTracker` until 4.1 is met. **This unit does not open it.**
- **R75.** The fault at the mixdown is the tracker's choice, and no follow rule undoes it -
  which is why this unit is not at the mixdown.
- **3.2's four tests, with R66's third.** Kept only if (1) total edits over all keyed recordings
  **do not rise** - unit 430's arbiter decision for 7.4, since no key scores the opening; (2) no
  named floor from 2.2 breaks; (3) the three adjudicated readings are unchanged or move onto
  exactly their own adjudicated text, printed before and after; (4) no capture row's count at or
  above the span bar falls.
- **R71** the floors count at or above raw span 13.0. **R73** a key-aligned added character
  inside a scored stretch may leave a floor, each named with its recording; nowhere else.
- **HM-DEC-091** a change that reads one recording and costs another is not a fix.
- **§0.2** nothing that keys or transmits is touched. **§12.5**, **§12.6** repair nothing on the
  way past. **HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**The author's decisions for this unit, overrulable** (also in DECIDED):

- **The rule, fixed now, before the trace.** At `CwProbabilisticStream.cs` 431 to 435 the
  stream carries the last speed it took from the estimator and the stream time it took it.
  On a read where the estimator's speed is not ready, is outside `SlowestWpm` to `FastestWpm`,
  **or departs from the carried speed by more than a factor of 1.5 either way**, the read uses
  the carried speed if it was taken within the last **3.0 s** of audio. A departure that the
  estimator measures on **two consecutive reads** is taken - a new sender at a new speed is
  followed within two reads. With no carried speed inside 3.0 s, the grid decides as today.
  1.5 sits between the halving the opening shows (2.0) and ordinary drift; 3.0 s is the survey's
  own window. **Neither number is tuned after the trace.**
- **One narrower variant**, only if the rule fails on exactly one row or one test: the carry on
  an unusable estimate alone, without the factor-of-1.5 test. Nothing else.
- **7.4 ticks only on a kept change.** If nothing is kept, P48 gains this unit's measurement and
  7.4 stays unticked.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 435 - STEP 7` entry from the decision block at the foot of
this file. `PHASE_STATUS.md` names unit 435, *the speed holds when the unit estimate halves*,
and `CURRENT_STEP: 7`. Patch-bump `Directory.Build.props`. The launcher's modified root files
are committed as they stood. **Entry round**, one type per invocation: the build with warnings
as errors, both carry-forward lines, captures, adjudicated, keyed floors, the keyed totals,
`WhatTheStrayLettersRestOnTests`, `WhatTheOpeningHeardTests`. Record the numbers of section 5 as
the numbers to beat.

**Drop candidate:** none.

### Task 1 - the trace (7.4)

A fact that asserts nothing and writes nothing, in `WhatTheOpeningHeardTests`: **per read**,
through the spliced stream 27.0 to 46.2 s and through `003919` cold, print the stream time,
the estimator's own speed and whether it was ready, the speed the entry decoder used and where
it came from (estimator or grid), and **the speed the rule of section 6 would have used**, with
the clause that decided it. Beside the opening, print the same columns for the locked `004108`
13.8 to 30 s.

Then, over **all 51 capture rows and every keyed recording**, count the reads where the rule's
speed differs from the entry's, per recording, and name the recordings where it differs at all.

**The one stop.** If the rule changes **no read** in the stream from 30 to 46.2 s, it cannot
move 7.4: build nothing, say so, and go to task 3. Anything else goes to task 2 - the count of
other recordings touched is a forecast for the four tests, **not a gate**.

**Drop candidate:** the `004108` comparison columns.

### Task 2 - build it and judge it (7.4)

Build the rule in its own commit, exactly as section 6 states it. Then run 3.2's four tests and
print every one as a number:

1. total edits over all keyed recordings, before and after, with 17:37's over its 25;
2. all 13 named floors;
3. the three adjudicated readings, quoted before and after;
4. all 51 capture rows' above-bar counts, before and after.

**And the criterion's own figure, as P42 has it - text beside count:** the stream 30 to 46.2 s,
`003901` cold and `003919` cold, each whole, before and after, beside `EANQNID`. Print also the
opening's speed per read under the change beside task 1's entry column, so the report can say
whether the speed held and **whether holding it was enough through a mix 100 Hz off the
sender** - unit 429's hypothesis, answered either way.

**Kept only if all four tests pass.** A change that fails any goes back out in the next commit
and the report says which test, which row, which number. If it fails on exactly one row or one
test, build the variant of section 6 once, in its own commit, and judge it the same way. If
the change passes and the opening's text does not move at all, **it is not kept** - it changed
nothing 7.4 asks about; take it out and say so.

On a kept change, tick 7.4 in `PHASE_PLAN.md` and note on step 7's line in `PHASE_STATUS.md`
that the P48 closure is superseded. On none, append this unit's measurement to P48.

**Drop candidate:** the variant.

### Task 3 - the exit round

Both carry-forward lines, the three floor tests, the keyed totals, `WhatTheStrayLettersRestOnTests`,
`WhatTheOpeningHeardTests`, and every type touched. `git diff` over the transmit files against
`7e209cb4` prints nothing. `git diff` over `CwToneTracker.cs`, `CwToneSurvey.cs` and
`CwDecoder.cs` against entry prints nothing.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **`CwToneTracker`, the tone survey and the mixdown at `CwDecoder.cs` 617 to 621.** R76, and 4.1
  to 4.7's.
- **The speed range 8 to 40.** 7.1 and 7.2's. If the trace shows the ceiling matters to the
  rule, that is a finding for section 3, not a change.
- **3.6 the stray letters, 6.5 the dead button, 7.8 the preamp.**
- **004535's held-gap litter (P37)** - a separate cause, not this unit's.
- **P27, P38, P40, P41, P43 to P48.**

## 10. What not to do

- **Do not touch the tracker, the survey, the mixdown line, or what keys or transmits.**
- **Do not tune 1.5 or 3.0 after the trace**, and do not build more than the one variant.
- **Do not keep a change the opening's text does not move.**
- **Do not lower an above-bar count on any row, or a named floor.**
- **Do not halt for a question.** Park it.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**
- **Write `output.md` before the session ends.**

## 11. Committing and pushing

Commit per task; the build of task 2 and any take-out each in its own commit. Push at the end
and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them. The ordering block
first:

```
READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial on 3.6;
   4 not started; 5 the owner's; 6 partial on 6.5; 7 partial on 7.1, 7.2, 7.4, 7.6, 7.8.
B. Step 7, criterion 7.4: whether the speed rule was kept, 3.2's four tests as numbers,
   and the opening - stream 30 to 46.2 s, 003901 and 003919 cold - before and after as text.
C. The rest, weighed against A and B. Section 4 raises <n> items; say whether any stands in
   the way of 7.4.
```

```
UNIT:       435 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     stream 30 to 46.2 s: 22 named <text> -> <n> named <text>; keyed 165 over 565 -> <n>; opening reads at a grid speed 15 of 23 -> <n>
DRIFT:      <0 if a criterion moved>
```

**Section 3 leads with the opening's text before and after**, then the per-read speed beside it.

**Section 2 tells the owner in one paragraph** whether the first minute on a new frequency
still comes apart into E and T, and if it does, whether the speed or the pitch is now the
reason.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: carry the held speed when the unit estimate halves - at CwProbabilisticStream.cs 431 to 435 a single read whose estimator speed is unusable or departs by more than 1.5 times from the carried speed uses the speed carried within 3.0 s instead of the grid, a departure on two consecutive reads is taken; traced per read on the opening, then built once and judged under 3.2's four tests, mixdown and tracker untouched
MOVE: work around
WHY: PHASE_PLAN.md criterion 7.4 asks for a change against what 7.3 names, and 7.3 named two figures - the mix pitch at CwDecoder.cs 617 to 621 and then the speed at CwProbabilisticStream.cs 428 to 435, where the estimator halved and the grid took 38 WPM for a 22 WPM sender; every recorded 7.4 attempt was a mixdown follow rule, which R75 closes and R76 keeps from the tracker, so the speed is the named figure no unit has built against.
STATE: partial
DECIDED: author's, overrulable - the rule and its two numbers, 1.5 and 3.0 s, fixed before the trace; the one stop is a rule that changes no read in the stream 30 to 46.2 s; one narrower variant, the carry on an unusable estimate alone, only on a single-row or single-test failure; test 1 reads does not rise, as unit 430's arbiter decided; a change the opening's text does not move is not kept; 7.4 ticks only on a kept change, superseding P48's closure, and otherwise P48 gains the measurement. No self-ruling authorizes work outside the tasks.
LICENCE: PHASE_PLAN.md criteria 7.3 and 7.4, R64, R65, R66, R71, R73, R75, R76 and section 6; PARKED.md P42 and P48; unit 429's trace (003f2c98); unit 430's arbiter decision on test 1; HM-DEC-091; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the first minute on a new frequency no longer reads the sender at nearly twice his speed because one window's estimate halved, or the project knows that the pitch alone is what still breaks it
ADVANCES: step 7 criterion 4
END-ARBITER-DECISION
```
