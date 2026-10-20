# Work instruction 580 - The fist bank: a yardstick with a truth for every hand on the band

**Hand run. One task, one commit, plus the record. No engine change; both boards exactly as at
HEAD.** This unit builds the yardstick the next units ship against.

**Why.** The owner, 2026-10-20: *"we've had no improvement for many units … step back and
reanalyze."* The reanalysis: the last six units were aimed at specimens with no truth (a live
far-side station, a live ghost, an old capture), at the seam between two finders, and at
yardsticks of perfect keying in flat noise. The first time a fist with a dah of 2.0 dits was
generated and measured, it read **0 of 60**, and had read 0 for the whole project. **From here
on, every rule ships against a truth.** This unit makes the truths.

**Standing rules** (HM-DEC-276) apply. Engine line 1500 s, survey 1800 s or more as this unit
finds, `--no-build` after one build, build servers shut down first.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted; everything here is generated.**

**Numbering.** This is unit 580, ruling HM-DEC-284. If taken, use the next free and say so.
Unit 579 was issued and is withdrawn unrun; say so in the record if it was not run.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      docs\cw-scoreboard.md
  MUST EXIST:      docs\cw-survey-2026-10-09.md
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
- One `dotnet test` invocation per line, filtered, with a `timeout`.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit580-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The fist bank

**Generated, not stored:** the fists are made by the generator at test time from their recipe
and a fixed seed, as the synthetic CQs and the straight-key hand are (`CwFixtureGenerator`,
`Synthesize`), so nothing binary is added to the tree. The recipe is the truth.

**The text**, 60 letters, the same for every fist, a ragchew exchange with a call, a report, a
name and a QTH: `CQ CQ DE N0CALL N0CALL K GM KEN NICE TO MEET U RPRT UR 559 QTH IS CHICOPEE MA K`
(the straight-key hand's text, so its figures carry over).

**The grid of fists**, one seed each (the seed is the recipe's hash, so it never changes):

| axis | values |
|---|---|
| dah over dit | **1.6, 2.0, 2.5, 3.0** |
| speed | **10, 15, 20, 25 WPM** |
| signal, in the 520 Hz passband as the generator measures it | **5, 10, 20 dB** |
| fade | **none; 12 dB of QSB at 0.7 Hz** (the working fixtures' fade) |

Ninety-six fists. **Every fist is a hand:** dits and dahs each spread ±15%, inside gaps one dit
±20%, letter gaps 2 to 3.5 dits, word gaps 4 to 6; pitch 615 Hz with 3 Hz of drift. The 3.0
ratio at 20 dB with no fade is the machine-like corner and should read whole today; the 1.6
ratio at 5 dB with fade is the far corner and need not.

**Each fist is named** `fist-r{ratio}-{wpm}wpm-{db}db-{fade|flat}` and has a recipe line the
survey prints.

## 3. The survey reads them

1. The survey test (`TheCwSurveyTests`) gains the fist bank as a group of its own, scored by the
   boards' own yardstick (right, wrong, invented, spaces, printed in silence), each fist's read
   kept in the doc for files under 60 s as the survey already does.
2. **The summary**: a matrix per axis pair, letters right of 60 summed - ratio × speed (flat,
   20 dB), ratio × signal (flat, 20 WPM), signal × fade (ratio 2.5, 20 WPM) - and the whole
   bank's total of 5,760. Plus the worst ten fists by name.
3. **The survey's run time** is measured and reported; if the bank takes it past 1800 s, split
   the fist bank into its own survey line (`TheCwSurveyTests.TheFistBank`) with its own doc
   section and its own timeout, and say so.
4. The doc: `docs\cw-survey-2026-10-09.md` gains the fist bank section with a `Rerun` line, or a
   doc of its own `docs\cw-fist-bank.md` if split, named in the scoreboard doc under the tables.

**The figures as shipped are the baseline.** Nothing is asserted beyond "every fist was read
and scored"; the fist bank is a yardstick, not yet a guard. The next units name which fists must
rise and which must not fall.

---

## 4. The lines, the record

Run the app line, the engine line and the survey line(s) once.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 580 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 580.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-284**, headline *The fist bank: a yardstick with a truth
  for every hand*, quoting the owner's step-back, recording the rule *every rule ships against a
  truth; specimens without one are checked after, not shipped on*, naming the grid of fists, the
  baseline total, and the worst corner.
- `docs\cw-scoreboard.md`: a 580 row in each table, unchanged; a line under the tables naming
  the fist bank and where it lives.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-284.

---

## 5. Report

Section 2, for the owner, in plain words, leading with **the matrices** - what Hamlet reads today
across ratio, speed, signal and fade - and the three facts they show (where reading stops as the
dah shortens; where it stops as the signal falls; what the fade costs). Section 1: what was built.
**Section 3: the three matrices, the bank total, the worst ten, the run time, both scoreboards'
rows.** Section 4: anything left, a line each.
