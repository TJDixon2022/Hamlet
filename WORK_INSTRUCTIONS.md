# Work instruction 579 - The hold's clock waits for the station to print, and the lane that fits the station is the one handed on

**Hand run. One handover, two corrections to it, one commit, plus the record.** Both corrections
were named by the tree's own measure (`TheGridStepsAsideTests`, unit 578):

- **The hold ends before the station has printed.** The end of the hold counts three of the
  sender's word gaps; where no sender is printed within 50 Hz it falls back to seven of the
  lane's own dit. A lane handed on from the bank has no printed sender yet, and the lane picked
  is often the 40 ms dit lane on a 12 WPM station, so the clock runs out in 0.84 s - the
  station's first letter gap - before a letter prints. `coverage-working`: held at 11.72 s, let
  go at 12.57 s. The fallback is wrong for exactly the case the rule exists for.
- **The lane picked is the best-shaped, not the one that fits.** On a 12 WPM station the 40 ms
  lane often scores highest and is handed on; its marks and gaps are then judged at the wrong
  dit.

The grid stepping aside (`CwRules.GridStepsAside`, built last unit, off, harmless on every
board) is measured **on** with these two, since it is the rule they were found under.

**The yardstick:** the five working receiver fixtures (5 dB, 12 dB of QSB at 0.7 Hz), **4 of
81** as shipped, with a truth. **The specimen:** Ken, both overs, on `cw-2026-10-19-141708`.
The fade on the fixtures is a separate limit; the trace is to show how much of the shortfall is
the hold and the lane, and how much is the fade.

**Standing rules** (HM-DEC-276): one rule; do not stop to ask - take the choice that keeps both
boards and name it in section 4; a run stopped for memory is run once more alone, then reported
as not measured. Engine line 1500 s, survey 1800 s, `--no-build` after one build, build servers
shut down first.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 579, ruling HM-DEC-283. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      docs\cw-scoreboard.md
  MUST EXIST:      tests\fixtures\cw\captured\cw-2026-10-19-141708.wav
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
  once for a multi-line commit. Scripts go in `.run-unit\unit579-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The two corrections

1. **The hold's clock waits for the station.** Until the handed-on station has printed its first
   letter, the end of the hold does not run: only the floor's own rules on standing, the grid
   taking the station back, and the carrier and shadow checks can end the hold. Once it has
   printed, the clock is the printed sender's word gap from the gate's clusters, three of them,
   as before. **The fallback to seven of the lane's own dit is removed.** If a lane handed on
   never prints within a bound, name the bound (`HeldLanePrintsWithinSeconds`, from the table -
   the longest a 5 dB CQ or a working fixture took to print its first letter, with margin) and
   let the hold go there, so a lane on noise does not hold forever; the trace shows the figure.
2. **The lane that fits is handed on.** Where more than one lane at a pitch stands at shape 0.4
   or better, the handover picks **the lane whose dit is nearest the dit the sequence's two
   kinds give**, not the best-shaped; where the kinds are not yet two, the best-shaped as now,
   and the pick is revisited when they become two. Say in one line how the lane's dit and the
   sequence's dit are compared (ratio in log).

**The trace, first, per file** (the five working fixtures, Ken's overs): for each lane handed
on, its dit against the station's, when it was held, when it printed its first letter, when it
was let go and by what - as shipped and with the two corrections. One row per hold.

Build both as one named change to the handover, measure once with `GridStepsAside` chosen on.

## 3. The measure, and the ship

**Ships on by default - the two corrections and `GridStepsAside` together - only if:**

- **the working fixtures rise from 4 of 81**, with at least two of the five at half their
  letters or better;
- **Ken's reply** prints at least three words of three or more letters at his pitch (reported);
- **both boards hold:** 297 and 2111, wrong and invented not rising, spaces not worse;
- **the weak-end guard holds:** the four 5 dB CQs at 71 of 84 or better; nothing in the 20 s of
  noise after them;
- **the hard limits:** noise prints nothing, the carrier at one seed, the first recording whole,
  nothing in silences, no old capture above six strays per 10 s; the summed strays reported;
- the easy fixtures not worse than 78 of 90; `two-station` reported.

**If the working fixtures rise but fewer than two reach half**, and the trace shows the rest is
the fade (the lane's 3 s key-up and key-down not following 12 dB of QSB at 0.7 Hz), ship on
anyway with that said plainly in section 2 and 4, since the hold and the lane are then right and
the fade is a separate unit. If a board or a hard limit fails, do not ship; say which.

If it ships: the working fixtures join the weak-end guard at the figures that ship; both
scoreboard guards at the figures that ship, on the board as the author's; the capture sheet's
`bank` line says `stepped aside at <pitch> Hz` and the lane's dit when a lane is handed on.

---

## 4. The lines, the survey, the record

Run the app line, the engine line and the survey line once; rewrite `docs\cw-survey-2026-10-09.md`
with a new `Rerun` line; per group before and after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 579 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 579.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-283**, headline *The hold's clock waits for the
  station, and the lane that fits is handed on*, naming the two corrections, the bound, whether
  they shipped with the grid stepping aside, and the figures before and after.
- `docs\cw-scoreboard.md`: a 579 row in each table at the figures that shipped.
- `CW_SPEC.md` §2: the handover paragraph brought up to date if it shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-283.

---

## 5. Report

Section 2, for the owner, in plain words, leading with **whether weak and far-side stations now
print, and what it cost** - the working fixtures before and after as text, Ken's reply before and
after, the boards, the CQs - then the two corrections in a sentence each, and how much of what is
left on the working fixtures is the fade. Section 1: what was built, the bound. **Section 3: the
holds table per file as shipped and after; the working fixtures' reads; Ken; the survey's
groups; both scoreboards' rows.** Section 4: anything left, a line each.
