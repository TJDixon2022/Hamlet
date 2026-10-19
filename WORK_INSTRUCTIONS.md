# Work instruction 575 - The split between a sender's letter gaps and word gaps; and a held lane keeps reading

**Hand run. Two tasks, commit per task, each with its own measure and its own ship.** Both faults
were named by the tree's own measurements; **this unit authorizes building the one rule each
measurement names.** No candidate list is given. The trace decides, the ship bar guards.

**Standing rules** (HM-DEC-276): one rule per task, no side variants; do not stop to ask - take
the choice that keeps both boards and name it in section 4; a run stopped for memory is run once
more alone, then reported as not measured.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 575, ruling HM-DEC-279. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit575-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.
- **The engine line's test host has crashed after its last tests three units running.** Before
  the lines, run the engine line once by itself with every other `dotnet` process ended
  (`dotnet build-server shutdown` first). If it still crashes, name the last test that ran and
  the exit code, report the line as not measured whole, and go on.

---

## 2. Task 1 - the split

**The fault, as the tree measured it** (`TheSpacesByCauseTests.TheGapsAgainstTheLines`, unit
565): on hand senders the gate sorts a sender's word gaps into his letter cluster
(`CwPatternGate.LetterAndWordGaps`, `Split`), so the letter cluster's top *is* the missed word
gap (`221745` 405, `221805` 400) and no line between the clusters can find it. Seventeen gaps
are in that table: fourteen word gaps missed, three letter gaps spaced. On the SKCC frequency the
owner reads `QTHWVWVNAMEIS JOEL` for `QTH WV WV NAME IS JOEL`.

**The yardstick:** the twelve-plus-three's spaces, **86 of 110 with 3 added** at HEAD; the
seventeen gaps by name; W1AW's 438 of 440 with 11 added.

1. **Trace the split on the seventeen.** For each gap, at the moment it is judged: the gaps the
   split had in hand (sorted, in ms), where the split put its boundary and by what rule (the
   jump it looked for, the minimum it demanded, how many gaps it needed on each side), and which
   side the gap landed on. One row each. Then the same for twenty word gaps W1AW gets right, so
   the report shows what a working split looks like beside a failing one.
2. **Name the rule.** One paragraph: what in the split sorts a hand's word gaps into his letter
   cluster - a jump demanded that a hand never produces, a boundary set from too few gaps, a
   cluster that never re-forms once set, or something else the table shows - and **the one
   change** that would put the fourteen on the word side without moving the three.
3. **Build it**, as one named rule in the gate, and measure once.
4. **Ships only if:** the twelve-plus-three's spaces rise above 86 of 110 with added not above
   3; letters hold at 318 of 353 with wrong and invented not rising; W1AW holds at 2111 with its
   spaces not worse than 438 of 440 and 11 added; the four 5 dB CQs at half or better; the hard
   limits (noise prints nothing, the carrier at one seed, the first recording whole, nothing in
   silences, no old capture above six strays per 10 s). The synthetic SKCC and touching hands
   are reported, not required. If it fails, say which condition, by how much, which recording;
   leave it as a rule only a test can choose; keep the trace.

## 3. Task 2 - a held lane keeps reading

**The fault, as the tree measured it** (`TheFoundWeakStationTests`, unit 574): on
`cq-18wpm-5db` every missed mark was present and crossed the level lines, and was lost because
the lane's running contrast dipped under `CwLaneBank.MinContrastDb` (12 dB) while the lane held
a printing station, so the lane stopped reading mid-station. The 12 dB floor exists so noise
never stands a lane; it was never meant to drop a station already standing.

**The yardstick:** the four 5 dB CQs, **13, 14, 16, 19 of 21** at HEAD; the ceiling is 21 of 21.

1. **Build the one rule the trace named:** a lane holding a station that has been handed on is
   not dropped by its own contrast floor; the floor governs standing, not holding. Say what does
   end such a lane's hold (the gate's own release at shape under 0.1, the grid taking the station
   back, a shadow judgement), so nothing holds forever on silence. Name the constant if one is
   needed; measure once.
2. **Ships only if:** the four CQs' total rises from 62 of 84 and none falls under half; noise
   prints nothing (`noisy-18wpm`, the noise seeds, the silences - this is the one this rule could
   break, so say the figures); the carrier at one seed; both boards hold; no old capture above
   six strays per 10 s; the summed strays figure reported. If it fails, say which condition and
   leave it as a rule only a test can choose.

---

## 4. The lines, the survey, the record

Run the app line, the engine line (per §1), and the survey line once; rewrite
`docs\cw-survey-2026-10-09.md` with a new `Rerun` line; per group before and after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 575 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 575.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-279**, headline *The split, and a held lane keeps
  reading*, naming each rule as built, what the trace showed, whether each shipped, and the
  figures before and after. Record the process change: *a unit names the fault and the
  yardstick; the measurement names the rule; the unit authorizes building the one rule the
  measurement names.*
- `docs\cw-scoreboard.md`: a 575 row in each table at the figures that shipped.
- `CW_SPEC.md` §2: one sentence per rule that shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-279.

---

## 5. Report

Section 2, for the owner, in plain words, leading with **whether word spaces are found on hand
senders now and whether weak stations print whole, and what each cost** - spaces before and
after, the SKCC line as it would read now, the four CQs before and after, the boards - then each
rule in a sentence. Section 1: per task, the trace in brief and the rule as built. **Section 3:
the seventeen-gap table with the new boundary as a column; the W1AW working-split rows; the CQs
before and after; the survey's groups; both scoreboards' rows.** Section 4: anything left, a
line each, and the engine line's crash if it crashed.
