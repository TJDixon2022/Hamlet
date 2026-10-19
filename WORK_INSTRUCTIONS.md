# Work instruction 573 - The shadow rule reaches the grid, down to one lane's spacing

**Hand run. Two tasks, commit per task, then the measure.** This unit extends the shadow rule
already in the tree (`CwShadow.IsShadowOf`, judged on the bank's lanes against stations more than
50 Hz away) to **the grid's own sequences**, and lowers the distance it reaches to **one lane's
spacing, 25 Hz**. It changes no constant of the rule itself (30 ms, two thirds) and nothing in the
reader.

**Why.** A sender 50 Hz under W1AW has printed in every live session for two weeks, never
reproduced on replay, measured once as energy locked to W1AW's keying. The shadow rule, when it
ran on W1AW piece-04, held **every lane from 350 to 525 Hz and 650 to 850 Hz** as W1AW's shadow at
shares of 0.9 to 1.0 - and could not look at 550 Hz, because 550 is exactly 50 Hz from 600 and
the rule only judges stations further than that. The ghost is a shadow the rule is not allowed to
see, and it prints from the grid, which the rule does not judge at all.

**Standing rules** (HM-DEC-276): one setting per unit, no side variants; do not stop to ask -
take the choice that keeps both boards and name it in section 4; a run stopped for memory is run
once more alone, then reported as not measured.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.** A synthetic fixture is generated for the test; it is not a recording.

**Numbering.** This is unit 573, ruling HM-DEC-277. If taken, use the next free and say so.
(HM-DEC-269 stays held for unit 565.)

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      docs\cw-scoreboard.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwShadow.cs
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
- One `dotnet test` invocation per line, filtered, with a `timeout`. The survey line gets 1800 s.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit573-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. Task 1 - the rule reaches the grid, and reaches 25 Hz

1. **Distance.** The shadow rule judges a sequence against every stronger station **more than
   `ShadowReachHz` away, 25 Hz** - one lane's spacing, the smallest distance at which the chain
   tells two pitches apart. The constant has a name; the "same station" distance the handover
   uses (50 Hz) is not changed. A station 25 to 50 Hz from a stronger one is therefore judged for
   shadow but is still "the same station" for the handover's purposes; say in one line how the
   two rules sit together in the code, and that nothing is judged twice.
2. **The grid.** Before the sender gate picks or keeps a grid sequence as a station, the sequence
   is judged by the same rule, by reference, against every stronger station Hamlet holds (grid
   and bank) more than 25 Hz away. **A grid sequence that is a shadow is never picked, and a
   held grid station that becomes a shadow is let go** - the gate's own release path, last letter
   settled or dropped by its own rule. Judged over the sequence's last ten marks, as the lanes
   are, and again as the sequence moves.
3. **Order.** Shadow is judged before the pick, for grid and bank alike, so dropping a shadow
   never sets off a fresh pick at the same station; a station picked within `LaneLeadSeconds`
   of a shadow being dropped hands on no backlog. These are the handover's rules already in the
   tree; they apply to the grid's path the same way.
4. **The record.** The capture sheet's `bank` line gains the shadows held in the last minute
   (pitch, share, the station it shadows); `cw_listen` gains `shadows`. **This is the line that
   names the live 550 Hz sender in the next W1AW session's sheet.**

## 3. Task 2 - the fixture and the trace

1. **A synthetic fixture for the test** (not a recording; generated as the synthetic CQs are):
   a sender at 600 Hz, 18 WPM, 20 dB, and **a second tone at 550 Hz keyed in lockstep with it,
   15 dB under it** - the shadow as measured live. Before task 1, say what the chain prints at
   550 Hz; after, it prints nothing there, and the 600 Hz station reads whole. A second case with
   the shadow at 575 Hz, and a third at 650. All three in `TheShadowReachesTheGridTests`.
2. **The side trace, `cw-2026-08-31-003229`:** the one old capture whose strays rose when the
   handover shipped (`S EA I E THS NE A`). Through the chain as shipped: which lane or grid
   sequence printed it, its pitch, its contrast, what stronger station stood at the time and
   where, and whether the rule as extended now holds it as a shadow. One paragraph; no rule is
   changed for it.

## 4. The measure, and the ship

With both tasks in, measure, and **ship only if**:

- **both boards hold:** **297** stands or rises, **2111** stands or rises, wrong and invented do
  not rise on either;
- **the hard limits hold:** noise prints nothing, the carrier at no more than one of twenty
  seeds, the first recording whole, nothing in silences, no old capture above six strays per
  10 s;
- **the weak-end guard holds:** the four 5 dB CQs at half their letters or better;
- **the three fixtures** read as task 2 states;
- **on W1AW piece-04, no grid sequence within 25 to 50 Hz of W1AW is ever picked** - list every
  one the rule holds back, with pitch and share, the way the lanes were listed.

If it fails, say which condition, by how much, which file; leave the grid path behind a switch
off by default (`CwShadow.JudgesTheGrid`), keep the fixture and the trace, and report.

Then rerun the survey, rewrite `docs\cw-survey-2026-10-09.md` with a new `Rerun` line, and put
per group before and after in the report.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 573 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 573.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-277**, headline *The shadow rule reaches the grid, down
  to one lane's spacing*, naming `ShadowReachHz`, the grid path, the fixture's figures before and
  after, the W1AW list, and the boards.
- `docs\cw-scoreboard.md`: a 573 row in each table.
- `CW_SPEC.md` §2: the shadow paragraph updated: both finders are judged, to 25 Hz.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-277.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **whether the ghost beside W1AW is now
caught and what it cost** - the fixture at 550 Hz before and after, the boards, the weak-end
guard - then what the sheet will say tonight (`shadows` with pitch and share) and that the 550 Hz
sender should appear there by name. Section 1: per task. **Section 3: the three fixtures before
and after; the W1AW piece-04 list of grid sequences held back; the 003229 paragraph; the survey's
groups before and after; both scoreboards' rows.** Section 4: anything left, a line each.
