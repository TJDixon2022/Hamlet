# Work instruction 578 - The grid steps aside for a lane that reads a station whole

**Hand run. One fault, one yardstick, one rule, one commit, plus the record.**

**The fault, as the tree traced it** (`KensOverTraced`, unit 577): Ken, the far side of the QSO
on `cw-2026-10-19-141708` at about 430 Hz, is read whole by the bank's 425 Hz lane at a shape of
0.70 for his entire over - and never printed, because **the grid chops him into 20 to 30 ms
pieces, stands a sequence on the pieces at 437 Hz, never qualifies it (shape falls to 0.00), and
that standing sequence blocks the lane from being handed on** under the handover's rule that the
grid goes first within 50 Hz. The trace says it in one line: *the only reason the lane is not
handed on is the grid standing within 50 Hz.* His fist is not the problem: by two means his dah
is 2.27 times his dit.

**The same thing on the receiver fixtures:** on at least two of the five *working* fixtures (5 dB
with 12 dB of QSB) the grid stands and the staged handover leaves the station to it, and the
group reads **5 of 81**. Those five have a truth. **They are the yardstick; Ken is the specimen.**

**Standing rules** (HM-DEC-276): one rule; do not stop to ask - take the choice that keeps both
boards and name it in section 4; a run stopped for memory is run once more alone, then reported
as not measured. Engine line 1500 s, survey 1800 s, `--no-build` after one build, build servers
shut down first.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 578, ruling HM-DEC-282. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit578-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The trace, first

**The five working fixtures and Ken's reply (272 to 300 s), one row per second** where a lane
within 50 Hz of the true pitch stands at shape 0.4 or better: the lane's pitch, dit and shape;
what the grid holds within 50 Hz (standing sequence, candidate, window: its pitch, its marks'
median length against the lane's dit, its shape, whether it has ever qualified and how long it
has stood without qualifying); and whether the lane is handed on. Then, per file, the seconds in
which a lane read the station whole and the grid blocked it, as a share of the station's keying.

**The paragraph:** what a grid sequence that blocks a whole-reading lane looks like against one
that is reading the station itself (W1AW, KB2UZK: marks the length of dits and dahs, qualified
within seconds) - the figure that parts them, from the table.

## 3. The one rule

**The grid steps aside** (`CwRules.GridStepsAside` or the tree's naming): a bank lane standing on
a station at shape 0.4 or better is handed on **even where the grid stands within 50 Hz**, when
the grid's sequence there **has not qualified for `GridStandsOnPiecesSeconds`** (start at the
handover's own 3 s; the table says) **and its marks are pieces** - median length under a third
of the lane's dit, or whatever figure the table shows parts pieces from marks. Once the lane is
handed on, the grid's pieces at that pitch are not offered while the lane holds, as the handover
already does. **The grid still takes the station back** the moment its own sequence qualifies
there. Nothing else in the handover moves: the wait, the carrier check, the shadow rule, the end
of the hold.

Build it, measure once.

## 4. The measure, and the ship

**Ships on by default only if:**

- **the five working fixtures rise from 5 of 81**, with at least two of the five at half their
  letters or better (`coverage-working` 23, `exchange-working` 19, `fast-working` 19,
  `prosigns-working` 9, `tightfist-working` 11);
- **Ken's reply** prints at least three words of three or more letters at his pitch that are not
  there as shipped (reported; he has no truth), and his first over (80 to 100 s) is reported;
- **both boards hold:** 297 and 2111, wrong and invented not rising, spaces not worse;
- **the weak-end guard holds:** the four 5 dB CQs at 71 of 84 or better; nothing in the 20 s of
  noise after them;
- **the hard limits:** noise prints nothing, the carrier at one seed, the first recording whole,
  nothing in silences, no old capture above six strays per 10 s; the summed strays reported;
- the easy fixtures not worse than 78 of 90; `two-station` reported.

If it fails, say which condition, by how much, which file; leave it as a rule only a test can
choose; keep the trace.

If it ships: the working fixtures join the weak-end guard at the figures that ship; both
scoreboard guards at the figures that ship, on the board as the author's; the capture sheet's
`bank` line says `stepped aside at <pitch> Hz` when it happens, and `cw_listen` carries it.

---

## 5. The lines, the survey, the record

Run the app line, the engine line and the survey line once; rewrite `docs\cw-survey-2026-10-09.md`
with a new `Rerun` line; per group before and after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 578 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 578.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-282**, headline *The grid steps aside for a lane that
  reads a station whole*, naming the rule, its figures, what the trace showed, whether it
  shipped, and the figures before and after.
- `docs\cw-scoreboard.md`: a 578 row in each table at the figures that shipped.
- `CW_SPEC.md` §2: one sentence if it shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-282.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **whether the far side of a QSO reads now
and what it cost** - Ken's reply as text before and after, the working fixtures before and after,
the boards, the CQs - then the rule in a sentence. Section 1: what was built, the figures.
**Section 3: the trace table in brief (per file, the share of seconds blocked); the working
fixtures' reads before and after; Ken before and after; the survey's groups; both scoreboards'
rows.** Section 4: anything left, a line each.
