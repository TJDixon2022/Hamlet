# Work instruction 571 - A shadow is told by its timing: the sequence rule, and the handover ships

**Hand run. Two tasks, commit per task, then the measure and the ship.** This unit adds one rule
to the lane handover already in the tree (`CwEnvelopeDetector.LaneHandover`, off) and, if the bar
is met, turns it on by default with its setting.

**The owner's ruling, 2026-10-13: option A.** The carrier at seed 5209 prints through a lane 75 Hz
from a station the grid already holds, every one of its marks starting within 10 to 30 ms of one
of that station's own (`TheWeakEndTests.TheCarrierAtTheLane`). **It is a shadow: a property of
timing, not of distance.** A per-mark shadow rule, measured last unit, cost the board six points
and a 5 dB CQ, because one coincidence held a real mark back. **The rule is judged over the lane's
whole sequence**, the way the pattern gate judges everything else.

**The same thing live:** a sender 50 Hz under W1AW in every live session, never reproduced on
replay, measured once as energy locked to W1AW's keying. And the open ask from unit 520, how a mark
finds its own tone beside a louder one. This rule is the first answer to both.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 571, ruling HM-DEC-275. If taken, use the next free and say so.
(HM-DEC-269 stays held for unit 565.)

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
- One `dotnet test` invocation per line, filtered, with a `timeout`. The survey line gets 1800 s.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit571-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves, except the one settings row in task 2.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.
- Status is written on the tree's own triggers; the phase count is as `PHASE_STATUS.md` holds it;
  the asks list is carried forward from the last report.

---

## 2. Task 1 - the shadow rule, over the sequence

**The rule.** Before a lane's station is offered to the sender gate (after the wait, after the
carrier check), its sequence is judged against **every stronger station Hamlet holds anywhere in
the passband** - the grid's standing stations and candidates, other lanes' held stations - more
than the same-station distance (50 Hz) away:

- for each mark in the lane's sequence (the last ten, the pattern gate's own memory, or the
  whole sequence if shorter), whether it **starts within `ShadowStartMs`** of a mark of that
  stronger station; start at 30 ms, the figure the carrier trace measured, and report 20 and 40
  beside it;
- the **share** of the lane's marks that do: if it is **`ShadowShare` or more** - start at two
  thirds, report a half and three quarters beside it - the lane is **a shadow of that station**,
  and hands nothing on while the share stays there. It is judged again as the sequence moves, so
  a lane that was a shadow and stops being one may stand.
- "Stronger" is by the two stations' own contrast, the lane's against the other's, in dB; a lane
  is never a shadow of a weaker station.
- The rule is one method with a name (`CwShadow.IsShadowOf` or the tree's own naming), judged on
  the sequences the gate already holds, so the grid's own sequences can be judged by it later
  without a second copy.

**Prove:**

- the carrier at twenty seeds prints at **no more than one** (5195 as at HEAD; 5209 silent), the
  handover on;
- the four 5 dB CQs still stand on their lanes and still read at half or better (they have no
  stronger station to be a shadow of);
- `two-station` still reads: its two stations are more than 50 Hz apart and key at different
  times, so neither is the other's shadow - say the shares measured;
- on W1AW piece-04, name every lane the rule holds back as a shadow, its pitch and share, so the
  live 550 Hz sender can be compared with it when the next session's sheets come.

## 3. Task 2 - the measure, and the ship

With the shadow rule in and the handover on, measure everything the bank unit measured, and
**ship the handover on by default only if**:

- **all four 5 dB CQs** read at half their letters or better;
- **the hard limits hold:** noise prints nothing (`noisy-18wpm`, the noise seeds, the silences),
  **the carrier at no more than one of twenty seeds**, the first recording whole, nothing in
  silences;
- **both boards hold:** **297** stands or rises, **2111** stands or rises, wrong and invented do
  not rise on either;
- **the stray-letter figure on the 65 old captures does not rise** above the handover-off figure
  (21.6);
- `two-station` is reported, not required.

**If it ships:**

- `CwEnvelopeDetector.LaneHandover` defaults on;
- the scan's settings popover gains its last row, **`Weak stations`** (on, default / off), kept
  as `WeakStations` in `settings.json`, taking effect at once; the sheet and `cw_listen` record
  it beside the AGC line;
- `TheWeakEndTests` gains the guard: the four 5 dB CQs at half their letters or better at the
  figures that ship; the carrier at twenty seeds joins the scoreboard guard's hard limits as it
  already is;
- both scoreboard guards pass at the figures that ship, which ship on the board as the author's;
- the story line says once, when a lane's station prints: `Weak station at <pitch> Hz`.

**If it does not ship:** say which condition failed, by how much, and which file; leave the
handover off; the shadow rule stays in the tree.

**Then rerun the survey**, rewrite `docs\cw-survey-2026-10-09.md` with a new `rerun` line, and put
per group before and after in the report.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 571 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 571.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-275**, headline *A shadow is told by its timing*, quoting
  the owner's ruling, naming the rule, `ShadowStartMs`, `ShadowShare`, whether the handover ships
  on, the figures before and after per group and per board, and the lanes held back on W1AW.
- `docs\cw-scoreboard.md`: a 571 row in each table, at the figures that shipped.
- `CW_SPEC.md` §2: the handover paragraph updated with the shadow rule and the setting.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 5. Report

Section 2, for the owner, in plain words, leading with **whether weak stations now read in the app
and what it cost** - the four CQs, the two boards, the carrier, the survey's groups before and
after - then the shadow rule in two sentences, where the `Weak stations` setting is, and what to
look for in the next W1AW session's sheet (the lanes held back as shadows, against the 550 Hz
sender). Section 1: per task, what was done, the constants and the figures beside them.
**Section 3: the carrier at twenty seeds; the four 5 dB CQs; `two-station`'s shares; the W1AW
shadows table; the survey's summary before and after; then both scoreboards' rows.** Section 4:
anything left, a line each.
