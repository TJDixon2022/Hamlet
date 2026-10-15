# Work instruction 572 - The handover ships on

**Hand run. One task, one commit, plus the record. No new rule, no new measurement beyond the
lines.** The owner ruled on 2026-10-13: **ship the lane handover on.** It was measured in the tree
with every condition met - the four 5 dB CQs at 13, 14, 16 and 19 of 21; the carrier at one seed;
297 and 2111 held with wrong and invented unchanged - and failed only a strays bar that this unit
restates. The ship code was written and left as a patch outside the tree; this unit puts it in.

**Standing rules, from here on, in every unit:**

- **One setting per unit.** No side variants, no figures reported "beside" the chosen one.
- **Do not stop to ask.** When a measurement cannot be run, report it as *not measured* and
  continue. When the instruction leaves a choice open, take the one that keeps both boards and
  name it in section 4. Stop only if the ship condition itself is in doubt.
- **If a run is stopped for memory, run it once more alone with everything else closed; if it
  stops again, report it as not measured and do not retry.**

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 572, ruling HM-DEC-276. If taken, use the next free and say so.
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
  once for a multi-line commit. Scripts go in `.run-unit\unit572-<name>.sh`, not committed.
- **The one layout holds**, except the one settings row below.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The task - ship it

**First, check whether the ship commit already landed** (a commit after the shadow rule's
measure, `f42a2cc2`, with `LaneHandover` defaulting on). If it did, say so in one line, verify the
rest of this list is in, and go to the record. If not:

**Apply the ship code.** If `unit571-ship-unshipped.patch` is still in the session scratchpad,
apply it; if it is gone, write it again from this list. Either way the tree ends with:

1. **`CwEnvelopeDetector.LaneHandover` defaults on.**
2. **The setting:** the scan's settings popover gains its last row, **`Weak stations`** (on,
   default / off), under `AGC in CW`, kept as `WeakStations` in `settings.json`, taking effect at
   once. Nothing else on screen moves.
3. **The record:** both capture sheets and each `cw_listen` row carry `weakStations` (on/off)
   beside the AGC line, and the bank line already there.
4. **The story line** says once, when a lane's station prints: `Weak station at <pitch> Hz`.
5. **The guard:** `TheWeakEndTests.TheWeakCqsReadAtHalfOrBetter` - the four 5 dB CQs at half
   their letters or better.
6. **The strays bar, restated** in the scoreboard's hard limits and in `docs\cw-scoreboard.md`:
   *no capture above the owner's line of six lone one- or two-element words per 10 s; the summed
   figure over the 65 old captures is reported beside the board, not a condition.* The summed
   figure at ship is 21.9.
7. **Both scoreboard guards** at 297 and 2111, the figures shipping on the board as the author's.

**Lines:** the app carry-forward line, the engine line, and the survey line once (handover on,
as shipped; `docs\cw-survey-2026-10-09.md` rewritten with a new `rerun` line and the per-group
figures - expected: easy 75, ungraded 63, CQs at 15 dB 72, CQs at 5 dB 46, boards 297 and 2111).

---

## 3. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 572 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 572.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-276**, headline *The handover ships on*, quoting the
  owner's ruling, the restated strays bar, and the figures at ship: the four CQs, the carrier at
  one seed, 297, 2111, the survey's groups. **Also the three standing rules above**, so every
  later unit carries them.
- `docs\cw-scoreboard.md`: a 572 row in each table; the strays line restated.
- `CW_SPEC.md` §2: the handover is on; the setting named.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-276.

---

## 4. Report

Section 2, for the owner, in plain words: **rebuild**; weak stations now read in the app; where
the `Weak stations` switch is and that turning it off puts Hamlet back exactly as last week; what
the story line will say; what to look for tonight on W1AW (a `Weak station at … Hz` line on
anything real, and the 550 Hz sender, which this rule does not yet reach). Section 1: what was
applied, from the patch or rewritten. **Section 3: the four 5 dB CQs; the carrier at twenty
seeds; the survey's groups; both scoreboards' rows.** Section 4: anything left, a line each.
