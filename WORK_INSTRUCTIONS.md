# Work instruction 585 - One finder, loudest first: a lane is not held while a louder lane keyed in step with it stands

**Hand run. One finder (`CwRules.BankFinds`, built, off), one fault, up to three rounds, one
commit per round plus the record.** The tag `before-one-finder` stands.

**Where one finder stands** (unit 584, round three, against as shipped): the fist bank 2,853 from
1,891; the fading half 1,111 from 439; the 5 dB CQs 80 of 84 from 71; the 0 dB CQs 32 of 84 from
nothing; the working fixtures 20 of 81 from 4. **And the twelve-plus-three 239 from 297**, W1AW
2111 with 3 invented, the random carrier at four seeds, `tightfist-easy` 0 of 14, three letters
in silences.

**The one fault, in the unit's own words:** *the bank stands a strong station's shadows before
the station itself, and holds them … a lane is not yet asked whether a louder lane keyed in step
with it might still stand, before it is held.* Two finders never met this: the grid stood the
station first, and its shadows never mattered. Rounds two and three fixed the shadow rule's two
wrong assumptions (backlog spared for the shadowed station; lane against lane by level, not
contrast), and each fix recovered what it touched. **What the grid did for free, and the bank
does not yet do, is hold the loudest first.** The carrier's four seeds are the same family: the
grid's narrowness and edge tests never ran on lanes.

**Standing rules** (HM-DEC-276, HM-DEC-284) apply, with up to three rounds as unit 584 had.
Engine line 1500 s, survey 1800 s, `--no-build` after one build per round, build servers shut
down first. **Memory:** the survey and the scoreboard are not run together; one test run at a
time.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 585, ruling HM-DEC-289. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit585-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. Round one - loudest first, and the grid's tests on lanes

With `BankFinds` on:

1. **Loudest first.** Before a standing lane is held, every other lane standing or about to
   stand (shape 0.3 or better, say; the table says) anywhere in the passband that is **louder by
   level** is looked at: if the candidate's marks start in step with the louder lane's
   (`CwShadow`'s own test, over the last ten marks, 30 ms, two thirds), the candidate is a shadow
   and is not held, whether or not the louder lane has been held yet. The louder lane is held
   first; the hold order at any moment is by level. A held lane that a louder lane later proves
   to be a shadow of is let go, as now.
2. **The grid's tests on lanes.** The tests the grid's marks met before they could stand and
   which lane marks never met - narrowness (a mark's energy sits in one bin and not its
   neighbours), the edge test (a mark's rise and fall are keyed, not noise), and whatever else
   `CwEnvelopeDetector` applied to a grid mark before offering it - are applied to a lane's
   sequence before it may stand, by reference where the code allows, else as the same figures
   with the same names. Say which tests those are and which one stops each of the carrier's
   seeds 5201 and 5208 (the two that were not seeds before).
3. **`tightfist-easy`**, 0 of 14 with one finder where the grid read 13: trace it (what the lane
   at its pitch did, and why nothing was held or printed) and fix it if the trace names a
   one-line cause; otherwise report it.

**Measure once:** the survey (the bank, the boards, every fixture group, the strays), the hard
limits with the carrier at twenty seeds and the steady carrier, the weak-end, short-dah and
straight-key guards - one run at a time.

## 3. Rounds two and three

If round one fails the bar, trace the single largest loss against as shipped (board or group,
to the stretch or fist, to the second), fix the one thing the trace names, commit, measure
again. Twice at most. Each round's figures go in the report's table.

## 4. The bar, and the ship

**`BankFinds` ships on by default only if, at the end of the last round:**

- **both boards hold:** the twelve-plus-three **297** or better, W1AW **2111** or better, wrong
  and invented not rising, spaces not worse;
- **the fist bank's score 2,500 or better** (from 1,891 as shipped; 2,853 with one finder before
  this unit), invented not above 50; **the fading half 1,000 or better**; the flat half not
  below 2,129;
- **the weak-end guard holds:** the four 5 dB CQs at 71 of 84 or better; nothing in the 20 s of
  noise after them; the short-dah rows and the nine hands hold their guards;
- **the hard limits:** noise prints nothing, the random carrier at no more than two seeds, the
  steady carrier at none, the first recording whole, nothing in silences, no old capture above
  six strays per 10 s; the summed strays reported;
- the easy fixtures not worse than 77 of 90; the working fixtures reported; `two-station`
  reported.

**For the owner's ruling, not a fail:** if everything holds but one board is under by no more
than 1% (the twelve-plus-three 294 or better, W1AW 2090 or better), do not ship; put the full
figures under *For the owner's ruling* with the stretch that fell and why.

**If it fails otherwise, this is the last unit on one finder for now:** say so, leave `BankFinds`
off, keep every trace and the rounds' figures, and in section 4 give in one paragraph what the
twelve-plus-three still loses with one finder and whether it is one cause or several - that is
the owner's evidence for going back to two finders.

If it ships: both scoreboard guards at the figures that ship, on the board as the author's; the
fading half joins `TheFistBankGuardsTests` as a floor at the figure that shipped less 5%;
`CW_SPEC.md` §2 is rewritten for one finder; the seam's rules are listed for removal in the
next unit; the `Weak stations` setting's row reads `Weak stations: always, one finder` and
does nothing, or is removed, as the layout allows.

---

## 5. The lines, the survey, the record

Run the app line, the engine line and the survey line once at the end; rewrite
`docs\cw-survey-2026-10-09.md` with a new `Rerun` line; the fist bank's matrices and every
group before and after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 585 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 585.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-289**, headline *One finder, loudest first*, naming
  the rules, the rounds and what each fixed, whether it shipped or waits on a ruling or ends
  the one-finder line for now, and the figures before and after.
- `docs\cw-scoreboard.md`: a 585 row in each table at the figures that shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-289.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **the whole picture** - what Hamlet
reads with one finder against two, row by row: W1AW, the hand-sent recordings, a short-dah fist,
a weak CQ, a fading station, the far side (the working fixtures) - and whether it ships. Then
the rules in a sentence each. Section 1: per round. **Section 3: the rounds' table (every
condition, each round, as shipped); the three matrices; the easy and working fixtures; the
carrier at twenty seeds with which test stopped each; both scoreboards' rows.** Section 4:
anything left, a line each, and the paragraph on the twelve-plus-three if it did not ship.
