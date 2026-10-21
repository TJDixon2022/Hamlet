# Work instruction 584 - One finder: the bank finds every station, and the grid retires

**Hand run. A rebuild of the finding path, behind a switch, measured in up to three rounds, one
commit per round plus the record.** Tag **`before-one-finder`** before the first change.

**Why, in the tree's own figures.** On a fading hand the bank's lane reads the station almost
whole - 172 of 173 marks at 20 dB - and where the lane holds it, every mark it hands on prints
(20 of 20, `KEN NICE TO`). It fails because the lane is seldom allowed to hold: the grid stands
the station first, in pieces, and blocks the lane (10 dB), or takes the station back within six
seconds and the lane's kept marks are dropped (20 dB). Over seven units the seam between the two
finders has gathered seven rules - the wait, the grid first, the carrier check, the shadow rule,
the step-aside, the hold, the hold's end - and the last four units were each a patch to the one
before. **The seam is the fault. This unit removes it.**

**The design.** The lane bank (25 Hz lanes, three dit sizes, the sender window's own filter) is
the finder for every station, strong and weak. A station is found, stood, judged Sure, held,
read and released on its lane. The grid's marks no longer reach the pattern gate or the sender
gate; the grid stays for the scope display and for nothing else. There is no handover, so the
wait (`LaneLeadSeconds`), the grid-first rule, the take-back, the step-aside (`GridStepsAside`),
the lane-fits-the-station pick (`HoldWaitsAndFits`) and the hold's fallback clock go. What
stays, now as the only path: the carrier check and the shadow rule (lane against lane), the
floor governing standing and not holding (`HeldLaneReads`), the hold ending in a pause on the
printed sender's own word gaps (`HeldLanePause`), and a lane's marks being its station's with no
level test (`HeldLaneMarksAreItsStations`). The short dah stays as it ships.

**Standing rules** (HM-DEC-276) apply, with one change for a rebuild: **up to three rounds** of
measure, trace the largest loss, fix, measure again, each round one commit. From HM-DEC-284,
every round ships against the same truths. Engine line 1500 s, survey 1800 s, `--no-build`
after one build per round, build servers shut down first.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 584, ruling HM-DEC-288. If taken, use the next free and say so.

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
- One `dotnet test` invocation per line, filtered, with a `timeout`.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit584-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves. The scope keeps the grid.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. Round one - the bank as the finder, behind `CwRules.BankFinds`

1. **The gates take only lane marks.** With `BankFinds` on, the grid's marks are not offered
   to the pattern gate or the sender gate. Every sequence, candidate, standing station, Sure
   judgement and held station is on a lane. The scan's catch ear, the capture triggers and the
   sheet's `senders` line read from the lanes.
2. **One station per pitch.** Where more than one lane within 50 Hz stands at shape 0.4 or
   better, the station is the one whose lane's dit is nearest the dit its two kinds give (the
   tree already does this pick; keep it), and the other lanes at that pitch feed nothing while
   it holds. Where the kinds are not yet two, the best-shaped lane.
3. **Standing and holding.** A lane stands by the rules it has (shape, two kinds, the carrier
   and shadow checks, the 12 dB floor). A standing lane whose station is Sure is held and read
   at once - there is no wait and nothing to wait for. The floor governs standing only. A held
   lane's marks are its station's, with no level test. The hold ends in a pause on the printed
   sender's own word gaps, or when the lane's sequence stops standing, or by the carrier or
   shadow checks; it has no fallback clock.
4. **The station's window.** Today a handed-on station opens its own window sized to the dit the
   gate measures. With the bank as the finder the held lane is that window: once the kinds are
   two, the lane of the nearest dit at that pitch is the one read (step 2), and no second
   window opens.
5. **The seam's rules** - `LaneLeadSeconds`, the grid-first rule, the take-back, `GridStepsAside`,
   `HoldWaitsAndFits`, `HeldLanePrintsWithinSeconds` - do nothing with `BankFinds` on. Say in
   one line each what becomes of them in the code (left for the off path this unit; removed
   next unit if the rebuild ships).
6. **CPU:** measure the chain on W1AW piece-04 with `BankFinds` on against off; the grid's marks
   are no longer gated, so it may fall. Say the figure.

**Measure once** - the survey (the bank, the boards, every fixture group, the strays), the hard
limits with the carrier at twenty seeds and the steady carrier, the weak-end, short-dah and
straight-key guards.

## 3. Rounds two and three - trace the largest loss, fix, measure

If round one fails the bar, **trace the single largest loss** against HEAD (the board or group
that fell most, to its stretch or fist, to the second, to what the lane did there against what
the grid had done), fix the one thing the trace names, commit, measure again. Twice at most.
Each round's figures go in the report's table.

## 4. The bar, and the ship

**`BankFinds` ships on by default only if, at the end of the last round:**

- **both boards hold:** the twelve-plus-three **297** or better, W1AW **2111** or better, wrong
  and invented not rising, spaces not worse;
- **the fist bank's total score rises from 1,891**, invented not above 50; **the fading half
  rises from 439 to 1,200 or better**; the flat half not below 2,129;
- **the weak-end guard holds:** the four 5 dB CQs at 71 of 84 or better; nothing in the 20 s of
  noise after them; the short-dah rows and the nine hands hold their guards;
- **the hard limits:** noise prints nothing, the random carrier at no more than two seeds, the
  steady carrier at none, the first recording whole, nothing in silences, no old capture above
  six strays per 10 s; the summed strays reported;
- the easy fixtures not worse than 77 of 90; the working fixtures reported; `two-station`
  reported.

**For the owner's ruling, not a fail:** if everything holds but one board is under by **no more
than 1%** (the twelve-plus-three 294 or better, W1AW 2090 or better) while the fist bank's score
rises by 20% or more, do not ship; put the full figures under *For the owner's ruling* with the
stretch that fell and why.

If it fails otherwise: say which conditions, by how much, which recording or fist; leave
`BankFinds` off; keep the trace and the rounds' figures; the tag stands.

If it ships: both scoreboard guards at the figures that ship, on the board as the author's; the
fading half joins `TheFistBankGuardsTests` as a floor at the figure that shipped less 5%; the
capture sheet's `senders` line says each station's lane and dit; `CW_SPEC.md` §2 is rewritten
for one finder; the seam's rules are listed for removal in the next unit.

---

## 5. The lines, the survey, the record

Run the app line, the engine line and the survey line once at the end; rewrite
`docs\cw-survey-2026-10-09.md` with a new `Rerun` line; the fist bank's matrices and every
group before and after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 584 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 584.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-288**, headline *One finder: the bank finds every
  station*, naming the design, the rounds and what each fixed, whether it shipped or waits on a
  ruling, the figures before and after, and the CPU figure.
- `docs\cw-scoreboard.md`: a 584 row in each table at the figures that shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-288.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **whether Hamlet reads more with one
finder than with two, and what it cost** - the boards, the bank's total and fading half, the
CQs, the carrier - then the design in three sentences, and the *For the owner's ruling* heading
if it applies. Section 1: per round, what was built, what the trace named, what was fixed.
**Section 3: the rounds' table (every condition, each round, HEAD); the three matrices before
and after; the working and easy fixtures; the CPU figure; both scoreboards' rows.** Section 4:
anything left, a line each, and the seam's rules listed for removal if it shipped.
