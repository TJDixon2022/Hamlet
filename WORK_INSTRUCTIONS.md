# Work instruction 418 - every sentence on the capture sheet is true of that capture

**Step 6, criterion 6.2.** Unit 417 made `tonePeak` a figure about its own recording and
ticked 6.7. It left 6.2 open for one reason, and it wrote that reason down: the `keying` line
says it comes from *an independent sweep of 400 to 1200 Hz*, but the meter sweeps 300 to 900.
6.2's opening words are *every sentence the capture sidecar states about a signal is true of
that capture*, so one false caption keeps it open. This unit checks every sentence on the sheet
against the tree, fixes the caption and any other sentence the check finds false, watching each
fix fail first, and ticks 6.2. Three tasks after task 0; drop tasks from the back.

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

**HM-DEC-155.** Do not run the whole suite. Run only this unit's test names and
`docs\carry-forward-tests.txt`, the way that file's top comment says. **Never run a test in the
background and poll it.** Run one type per invocation, each with its own `timeout`. The
captures type has 51 rows; give it 600 s. Unit 417 ran four dispatcher-loop re-runs in one
invocation; do not do that. Run one type per invocation.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**Nothing in section 4 halts this phase** (R65). Park any question in
`docs\phase-correctness\PARKED.md` and keep going. **Do not carry an ask forward as blocking**
unless 6.2 itself cannot be met without it.

## 2. The tool facts

- Apostrophes in quoted heredocs break.
- Doubled backslashes collapse.
- `;` is refused, and so is `rm`.
- Python cannot run here.
- A multi-line commit needs `-m` more than once.
- A bare `git worktree`, `git checkout` or `git show` is refused at the prompt.

Put multi-step commands in `.run-unit\unit418-<name>.sh` and run them with `sh`. You can copy
unit 417's scripts.

## 3. Asks still outstanding

None carried.

- **P10 is answered by this instruction** (see section 4). It stays in `PARKED.md` with the
  answer appended beneath it: one paragraph, marked the arbiter's, overrulable.
- **P11 stays parked.** Where the noise is taken for `tonePeak` is not this unit's work, and it
  does not stop 6.2. The sheet prints no figure where the pitch was not measured, and the
  stopband was seen only there.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Every sentence the capture sidecar states about a signal is
            checked against the code that produced it, and each one found
            false - the keying caption's 400 to 1200 Hz first - is made
            true, watched failing first on a saved capture, so 6.2 ticks.
ADVANCES:   step 6 criterion 2
DRIFT:      0
```

**Where the count stands:**

| Step | State | Criteria |
|---|---|---|
| 0, 1, 2 | done | all met |
| 3 | partial | 3.1, 3.2, 3.3 and 3.5 met; 3.4 open |
| 4 | not started | none met |
| 5 | the owner's | 5.1, his verdict |
| 6 | partial | 6.1, 6.6 and 6.7 met; 6.2, 6.3, 6.4, 6.5 and 6.8 open |

**Why not step 3.** Criterion 3.4 fires only after three units in a row keep no change. Unit 416
kept two, so the count is zero, and no unit can honestly flip 3.4 now. Unit 417 already answered
this as P9, and nothing since has changed it. **The spacing stays as unit 416 left it.** R64
prefers the screen once the spacing has nothing open.

**Why 6.2 of the five open screen criteria.**
- The work is already measured: unit 417 found the false sentence and named the constants that
  make it true (P10).
- 6.2's three named clauses already hold on the regenerated sheets: `tonePeak` since unit 417,
  `elementHz` and `keying` since unit 411, each watched failing first.
- So this unit closes a criterion rather than starting one.

6.3, 6.4, 6.5 and 6.8 are for the units after this one.

**What the tree says today** (`src\Hamlet.App\ViewModels\MainWindowViewModel.cs`):

- **`KeyingRecordLine`** (near line 12435) writes this caption as a fixed string:
  *(an independent sweep of 400 to 1200 Hz in 25 Hz steps over the last six seconds, sharing
  nothing with the decoder)*.
- **The sweep's actual range** is in `KeyingEnvelope.cs` at lines 130 to 138:
  `LowestToneHz = CwToneTracker.MinimumToneHz`, `HighestToneHz = CwToneTracker.MaximumToneHz`
  and `ToneStepHz = 25`. The comment at lines 111 and 112 records that it *used to run 400 to
  1200*. The caption was never updated when the range changed. That is the failure 6.2 exists
  to catch.
- **The rest of the caption is unchecked:** *over the last six seconds* and *sharing nothing
  with the decoder*. So is every other line on the sheet. Task 1 checks them.

---

## 5. Verify this instruction against the tree

Check each item below. Report any mismatch, and repair nothing:

- `KeyingRecordLine` holds the 400 to 1200 caption as a literal. It is the only place the
  `keying` line is composed.
- `KeyingEnvelope.LowestToneHz`, `HighestToneHz` and `ToneStepHz` are as quoted, and they
  resolve to 300, 900 and 25.
- `CwKeyingMeter` sweeps through `KeyingEnvelope.Best` and nothing else.
- `TheSidecarIsReReadTests` and `TheSidecarDoesNotContradictItselfTests` exist and are green at
  HEAD.
- Nothing in `src` or `tests` parses the sidecar's `keying` line. If something does, say so
  before you change the caption.
- The keyed totals at HEAD are 167 over 565, the ten 35 over 156, and 17:37 19 over 25, all
  against inferred keys.

---

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R66, §3 and §6 apply. These are the ones this unit leans on, in full:

**6.2, as the plan states it:** *Every sentence the capture sidecar states about a signal is true
of that capture or says plainly that it is not measured: `tonePeak` is a figure about this
recording or is not printed as one, `elementHz` does not report nothing measured while the line
above it resolves elements, and the `keying` line does not say no keying at a pitch in the same
breath as counting key-downs there; each is watched failing first on a saved capture that shows
the contradiction.*

**§6: Step 6 changes what the operator reads, never what the radio does.** *A screen criterion
is met by making a sentence true, never by deleting the sentence and saying nothing, and never
by changing a radio setting to match a claim.* For this unit, that means **the caption changes
to match the meter. The meter does not change to match the caption.**

**R63** on `tonePeak` and **HM-DEC-091**: `CwDecodeReport.SnrDb` is not changed, and neither is
the `heldPeak` line. **R65**: nothing carried halts the loop. **CLAUDE.md §0.0**: never state as
known what is not known. **§0.2**: nothing that keys or transmits is touched. **HM-DEC-155**,
**HM-DEC-165** and **FACT-004** also apply.

**P10's answer, author's, overrulable.** The caption takes its range and step from
`KeyingEnvelope`'s own constants rather than from a second literal, so it cannot fall out of date
again. No decision record is needed: this makes a sentence true under 6.2 and promises the
operator nothing new.

## 7. Status cadence

Follow the cadence in the header.

---

## 8. The tasks

### Task 0 - the record

- `PHASE_OUTCOME.md` gets its `## UNIT 418 - STEP 6` entry, built from the decision block at the
  foot of this file.
- `PHASE_STATUS.md` names unit 418 and `CURRENT_STEP: 6`.
- Patch-bump `Directory.Build.props` to 1.13.105.
- Append P10's answer in `PARKED.md`.

**Entry round.** Run both carry-forward lines, the three floor tests,
`TheSidecarDoesNotContradictItselfTests`, `TheSidecarIsReReadTests` and
`TheTonePeakIsAboutThisRecordingTests`. Record the results as the numbers to beat.

**Drop candidate:** none.

### Task 1 - the trace: every sentence on the sheet

Write a fact in `tests\Hamlet.App.Tests\Cw` that asserts nothing. It regenerates the whole
sidecar through the writer's own code for `cw-2026-09-23-173723`, `cw-2026-08-22-014113` and
`cw-2026-08-17-013347`. For each line and each caption that states something about the signal
or about how a figure was produced, it prints the line beside what the code that produced it
actually does. That means the range, the step, the window length, the thread, and what is shared
with the decoder.

In the report, table each sentence:

- the sentence
- what the tree says, with file and line
- whether it is **true**, **false**, or **says it is not measured**

Include every line, not only `keying`. **Choose which sentences are false from the code, not
from what reads well.**

**Drop candidate:** none. Task 2 is built from this table.

### Task 2 - the false sentences made true (6.2)

**Write the test first and watch it fail on a saved capture.** Create
`TheKeyingCaptionNamesTheSweepItRanTests`: on 17:37, the `keying` line's caption names the range
and step `KeyingEnvelope` sweeps. **It is red today because the caption says 400 to 1200.**
Quote the red, and commit the test red on its own before the fix.

Then change `KeyingRecordLine` so the caption reads its figures from `KeyingEnvelope`'s
constants. Also correct any other part of that caption the trace found false (*six seconds*,
*sharing nothing with the decoder*).

**Every other sentence task 1 found false gets the same treatment:** its own red test on a saved
capture, committed red, then the fix. The fix may make the sentence say plainly that the figure
is not measured, but **it may not delete a line or leave it saying nothing** (§6).

You may edit `TheSidecarIsReReadTests` only where the wording forces it; say what changed.
Regenerate `.run-unit\unit418-sidecar-*.txt` for 17:37 and 014113, and print the old and new
lines side by side. **No capture sidecar in the tree is edited.**

**Tick 6.2 only if** every sentence in task 1's table is now true or says it is not measured,
and the three named clauses still hold. Report each clause, and then the lead sentence, one
line each.

**Drop candidate:** a sentence the trace finds false outside the `keying` caption and outside
6.2's scope (not about a signal). Park it as P12 with its measurement instead of fixing it.

### Task 3 - the exit round (6.6 holds)

- Hamlet.sln builds with warnings as errors.
- Run both carry-forward lines, the three floor tests with captures at 51,
  `TheSidecarDoesNotContradictItselfTests`, `TheSidecarIsReReadTests`,
  `TheTonePeakIsAboutThisRecordingTests`, `CaseRosterSurvivesAnEveningTests`, the new tests, and
  every type you touched.
- The keyed totals are unchanged at 167 over 565.
- `src\Hamlet.RadioEngine\Cw` prints nothing against entry.
- The transmit files print nothing against `7e209cb4`.

---

## 9. Parked - do not touch, do not raise

- **Step 3**, and P6, P7, P8 and P9. The spacing stays as unit 416 left it.
- **Step 4, the pitch judge**, P11, and the roster's `tonePeakDb` column.
- **6.3, 6.4, 6.5 and 6.8:** the reflow, the hover text, the dead button and the RF gain scale.
  These are for the next units.
- **The acquisition failure** in the first two minutes of the 7.052 session.
- **Any key, scored region or floor.** These are fixed.

## 10. What not to do

- **Do not change `KeyingEnvelope`, `CwKeyingMeter`, `CwDecodeReport.SnrDb` or anything in
  `src\Hamlet.RadioEngine\Cw`.** The sentence moves; the measurement does not.
- **Do not delete a sidecar line** or leave it saying nothing.
- **Do not edit a sidecar already in the tree.**
- **Do not touch anything that keys, transmits or writes to the radio.**
- **Do not halt for a question.** Park it.
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose
  a timestamp.**
- **Report mismatches and repair nothing. Use American spelling and UTF-8, and the four headings
  exactly.**

## 11. Committing and pushing

Commit after each task. Each failing test in task 2 is committed red on its own before its fix,
with the red quoted in the commit message. Push at the end, and say whether the push succeeded.

---

## 12. Reporting

Write `output.md` at the root, with the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0, 1, 2 done; 3 partial,
   3.4 only open and not flippable this unit; 4 not started; 5 the
   owner's; 6 partial.
B. Step 6: 6.2 met or not - the lead sentence and each of its three
   clauses, one line each, with the red quoted; 6.6 held.
C. The rest. Section 4 raises <n> items; <none | which> in the way of
   6.2.
```

```
UNIT:       418 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     sidecar sentences false <before> -> <after> of <total checked>
DRIFT:      <0 if a criterion moved>
```

**Section 3 leads with** the old and new `keying` lines for 17:37, one above the other. After
them comes task 1's sentence table, with its before and after verdicts.

**Section 2 tells the owner in one paragraph** that every sentence on a capture's sheet has now
been checked against the code that writes it, and what changed.

---

```
ARBITER-DECISION
STEP: 6
APPROACH: audit every sentence of the capture sidecar against the tree and make the keying caption name the sweep range KeyingEnvelope actually sweeps, watched failing first
MOVE: continue
WHY: PHASE_PLAN.md step 6 criterion 6.2 is held open only by the keying caption naming a 400 to 1200 Hz sweep the meter no longer runs (P10); its three named clauses already hold. Step 3's only open criterion, 3.4, cannot flip while unit 416's kept changes stand, so under R64 and R65 the loop stays on the screen.
STATE: partial
DECIDED: author's, overrulable - P10 answered: the caption reads its range and step from KeyingEnvelope's constants rather than a second literal; 6.2 chosen over 6.3, 6.4, 6.5 and 6.8 because it is measured and one sentence from met; the whole sheet audited rather than the one caption because 6.2's lead sentence covers every sentence; a false sentence outside 6.2's scope is parked as P12 rather than fixed; per-type timeouts are the unit's
LICENCE: PHASE_PLAN.md R62, R64, R65, section 6; step 6 criterion 6.2; PARKED.md P10; HM-DEC-091; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0 and 0.2
ACCOMPLISHED: every sentence on a capture's sheet says what the code that wrote it actually did, so nobody works from a sweep range the meter stopped using
ADVANCES: step 6 criterion 2
END-ARBITER-DECISION
```
