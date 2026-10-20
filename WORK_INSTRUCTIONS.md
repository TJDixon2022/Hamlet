# Work instruction 577 - The hand whose dah is not twice his dit: Ken, and the far side of a QSO

**Hand run. One fault, one measure, one rule, one commit, plus the record.** The fault was named
by the tree's own trace (`TheFoundWeakStationTests`, unit 574): the far side of a live QSO, Ken
at about 430 Hz on `cw-2026-10-19-141708`, is found whole by the bank (45 of 48 marks) and never
read, because **his marks run 50 to 280 ms with no jump of two anywhere in them** - a straight
key whose dah is barely twice his dit - and the gate demands two kinds with a jump of two before
anything stands. The grid stands him at 437 Hz, never qualifies him, and blocks the lane from
handing him on. He printed as `EIE E EIET EI I I E SE I` live.

**This is the far side of most ragchews.** The near side reads like a log entry now; the far side
is dits. Fixing it is the largest change to what the owner sees on the air that is left.

**Standing rules** (HM-DEC-276): one rule; do not stop to ask - take the choice that keeps both
boards and name it in section 4; a run stopped for memory is run once more alone, then reported
as not measured. The engine line at 1500 s, the survey at 1800 s, `--no-build` after one build,
build servers shut down first.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.** The synthetic hand below is generated, not recorded.

**Numbering.** This is unit 577, ruling HM-DEC-281. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit577-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The yardstick, with a truth

**A synthetic hand** (`CwFixtureGenerator`, as the SKCC and touching hands are made): 12 WPM, a
straight key whose **dah is 1.7 times his dit** with dits and dahs each spread ±15%, inside gaps
one dit ±20%, letter gaps 2 to 3.5 dits, word gaps 4 to 6, at 20 dB, sending
`CQ CQ DE N0CALL N0CALL K GM KEN NICE TO MEET U RPRT UR 559 QTH IS CHICOPEE MA K`. Three seeds.
Beside it the same hand at a dah of **2.0** and **2.5** times, so the report shows where the
gate stops reading as the ratio falls. **This is the yardstick: it has a truth.** Ken is the
specimen.

**Measured first, as shipped:** letters right of sent for each ratio at each seed, and whether a
sender ever stands (the gate's two kinds, the jump it found, shape). One table.

## 3. The trace, then the one rule

**Ken, both overs** - about 80 to 100 s (the burst the stray capture fired on, at 14:13:46 UTC,
which is 98 s into this file) and 272 to 300 s - through the chain as shipped:

1. **The grid's sequence at 437 to 441 Hz:** its marks in order with lengths, the two clusters
   the gate tries to form (centres, the jump between them), its shape over time, and why it
   never qualifies: which test fails, with the figure and the line it had to pass.
2. **The bank's lane at 425 Hz:** its marks, its shape (0.68 to 0.72 last time), and the reason
   it is not handed on, second by second: the grid standing within 50 Hz, and nothing else.
3. **What a reader would make of his marks if they were kinded at the natural break** - sort
   his marks by hand, find the largest jump in log length (1.36 last time), kind them there,
   and run the lookup table over the result: the text, and how much of it is words. This is
   the ceiling for his fist.

**The paragraph:** where he is lost - the gate's demand for a jump of two, the grid blocking
the lane, or both in series - and **the one change** that reads the 1.7 hand at the yardstick.
If the table shows two faults in series (the gate will not stand a 1.7 hand, *and* the grid
blocks the lane even when a lane could), the one change is the gate's, and the grid stepping
aside is built only if the gate's change alone does not read the synthetic hand through the
grid. Say which.

**Build it**, as one named rule, and measure once. The likely shape, for the trace to confirm
or correct: **the gate accepts two kinds when the two clusters are stable** (each with enough
marks, their centres apart by more than the spread within either) **even where the jump is
under two, down to a named floor** (`MinDahDitRatio`, the table says where - the 1.7 hand must
pass, and the carrier and noise must not), and the letter and word lines follow from the
clusters as they do now.

## 4. The measure, and the ship

**Ships on by default only if:**

- **the 1.7 hand reads 80% or better** of its letters at all three seeds, and the 2.0 and 2.5
  hands do not read worse than as shipped;
- **Ken's overs** print at least three words of three or more letters that are not there as
  shipped, and his lone one- and two-element words per 10 s fall (reported, since he has no
  truth);
- **both boards hold:** 297 and 2111, wrong and invented not rising, W1AW's spaces not worse;
- **the weak-end guard holds:** the four 5 dB CQs at 71 of 84 or better; nothing in the 20 s of
  noise after them;
- **the hard limits:** noise prints nothing, the carrier at one seed, the first recording
  whole, nothing in silences, no old capture above six strays per 10 s; the summed strays
  reported;
- `two-station` reported.

If it fails, say which condition, by how much, which file; leave it as a rule only a test can
choose; keep the yardstick and the trace.

If it ships: the 1.7 hand joins the guards (`TheStraightKeyReadsTests`), both scoreboard guards
at the figures that ship, the figures on the board as the author's; the capture sheet's
`clusters` line gains the dah/dit ratio the gate measured.

---

## 5. The lines, the survey, the record

Run the app line, the engine line and the survey line once; rewrite `docs\cw-survey-2026-10-09.md`
with a new `Rerun` line; per group before and after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 577 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 577.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-281**, headline *The hand whose dah is not twice his
  dit*, naming the rule, the floor, what the trace showed on Ken, whether it shipped, and the
  figures before and after.
- `docs\cw-scoreboard.md`: a 577 row in each table at the figures that shipped.
- `CW_SPEC.md` §2: one sentence if it shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-281.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **whether a straight-key hand with a short
dah now reads, and what it cost** - the 1.7 hand before and after, Ken's overs before and after
as text, the boards, the CQs - then the rule in a sentence. Section 1: what was built, the floor.
**Section 3: the ratio table as shipped and after; Ken's trace; Ken's overs as printed before
and after; the survey's groups; both scoreboards' rows.** Section 4: anything left, a line each.
