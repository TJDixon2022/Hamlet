# Work instruction 576 - A held lane keeps reading, and the hold ends in a pause

**Hand run. One rule, one commit, one measure, plus the record.** It finishes the rule unit 575
built and left off: `CwRules.HeldLaneReads`, the lane that holds a handed-on station goes on
reading while its contrast dips under `CwLaneBank.MinContrastDb`.

**What that rule did, measured:** the four 5 dB CQs read **71 of 84** (14, 17, 20, 20 of 21)
from 62; noise printed nothing; the carrier stayed at one seed; the twelve-plus-three held at
297. **It failed on one thing:** on W1AW piece 2 the held lane went on reading through the pause
after `W1AW <AS>` and printed `M T TAT TM E` - eight letters invented, 2111 to 2101. The hold had
no end but the sequence ceasing to stand, and noise keeps a sequence standing for a while.

**This unit gives the hold an end.** Nothing else moves.

**Standing rules** (HM-DEC-276): one rule; do not stop to ask - take the choice that keeps both
boards and name it in section 4; a run stopped for memory is run once more alone, then reported
as not measured. **The engine line needs 1500 s**, not 480; set the line scripts so.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 576, ruling HM-DEC-280. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      docs\cw-scoreboard.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwLaneBank.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`: app 600 s, engine 1500 s,
  survey 1800 s. `dotnet build-server shutdown` and end leftover `dotnet` processes before the
  lines; `--no-build` on the lines after one build.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit576-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The rule

**First, the pause on piece 2, traced** (one table): from the last real mark before `W1AW <AS>`
to the first real mark after, every mark the held lane called - its start, length, level against
the lane's lines, and the lane's contrast at that moment - beside the sender's own figures at that
moment: his dit, his letter gap, his word gap (the gate's clusters), and the longest gap he had
left in the piece before that. This shows how long the real pause was, how soon the first noise
mark came, and how the invented letters were made.

**Then the end of the hold**, as one named rule on the held lane: **the hold ends when the lane
has called no mark that the gate kinded for longer than `HeldLanePauseGaps` of the sender's own
word gap** - start at three word gaps, the smallest whole number the table supports, and say what
the table supported. When the hold ends, the lane is an ordinary lane again: the floor governs,
its sequence has to stand on its own, and it is handed on again only through the handover's rules
(the wait, the grid first, the carrier and shadow checks). A mark the gate did not kind - a
splinter, a piece under half a dit - does not restart the clock.

If the table shows a pause that the word-gap count cannot cover without also covering the noise
(a sender whose word gaps are long and whose pause was short), say so, take the figure that keeps
W1AW, and name it in section 4.

**Measure once**, with `HeldLaneReads` on and the end of the hold in, and **ship on by default
only if:**

- the four 5 dB CQs read **71 of 84 or better** and none under half;
- **W1AW 2111 with 0 invented**, 438 of 440 spaces, 11 added, not worse;
- the twelve-plus-three **297** with wrong and invented not rising;
- noise prints nothing (`noisy-18wpm`, the noise seeds, the silences); the carrier at one seed;
  the first recording whole; nothing in silences; no old capture above six strays per 10 s; the
  summed strays reported;
- the silence after a weak station: a synthetic 5 dB CQ followed by 20 s of the same noise
  prints nothing in the 20 s (`TheWeakEndTests`, a new case, reported with its figure).

If it ships: `HeldLaneReads` defaults on, with the end of the hold; the weak-end guard moves to
the new figures; both scoreboard guards at the figures that ship, which ship on the board as the
author's. If it fails, say which condition, by how much, which recording; leave both off.

---

## 3. The lines, the survey, the record

Run the app line, the engine line and the survey line once; rewrite `docs\cw-survey-2026-10-09.md`
with a new `Rerun` line; per group before and after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 576 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 576.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-280**, headline *A held lane keeps reading, and the hold
  ends in a pause*, naming the rule, `HeldLanePauseGaps`, what the trace showed on piece 2,
  whether it shipped, and the figures before and after.
- `docs\cw-scoreboard.md`: a 576 row in each table at the figures that shipped.
- `CW_SPEC.md` §2: one sentence if it shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-280.

---

## 4. Report

Section 2, for the owner, in plain words, leading with **whether weak stations now print whole
in the app and what it cost** - the four CQs before and after, W1AW's invented count, the boards
- then the rule in a sentence and what the pause on piece 2 looked like. Section 1: what was
built. **Section 3: the piece-2 pause table; the four CQs; the silence-after case; the survey's
groups; both scoreboards' rows.** Section 4: anything left, a line each.
