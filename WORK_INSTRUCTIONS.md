# Work instruction 593 - The sequence reader learns a hand's gaps

**Hand run. One change to the reader - how it finds and learns the gap classes - up to three
rounds, one commit per round plus the record.** Tag `before-gap-learning` first.

**Where it stands** (1.13.279, the wild yardstick in place, the reader as shipped): the wild
bank **2859** (3133 of 3528 letters right, 274 wrong, 0 invented, **400 spaces added**);
per recipe `machine` 572, `farnsworth` 567, `steady` 524, `mobile` 522, `tight` 489, `bug` 459
of 588; the two real hands Bryan **118 of 144**, Ken at most **43 of 76**; the old boards 308,
2101, 3,898. **What the baseline says:** the reader keeps a hand's letters and breaks his words.
The two hands with an inside gap of 1.6 dits are the worst - `bug` with 114 wrong (letters
merged and split at the inside/letter line), `mobile` with 110 spaces added (the letter/word
line) and `5 7 9` for `579`; on the air Bryan prints `IAM`, `TNXFER`, `Q TH`, `C H I C OP E E`,
and Ken `GITTSNI■GH` for `PITTSBURGH`. The reader's cold inside-gap range is 0.4 to 1.7 dits,
its classes are learned as medians of its own last decode, and its multi-start varies only the
dit. **This unit changes how the reader finds and learns the gaps, and nothing else.**

**Standing rules** (HM-DEC-276, HM-DEC-284) apply, with up to three rounds. Engine line 1500 s,
**survey 2400 s** (it ran 23 min with the two new groups), `--no-build` after one build per
round, build servers shut down first, one test run at a time; a run stopped for memory is run
once more alone without asking.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted. The draft truth `cw-2026-10-19-141708.truth.md` is not edited** - the owner corrects
it by ear; Ken and Bryan stay reported, not guarded.

**Numbering.** This is unit 593, ruling HM-DEC-297. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      docs\cw-scoreboard.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwSequenceReader.cs
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
  once for a multi-line commit. Scripts go in `.run-unit\unit593-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- The change is inside the sequence reader. The mark finders, the lane bank, the column finder
  and the seam's rules are not touched. The keyer and the wild bank are not touched.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The trace, first

On `bug` and `mobile` (20 dB, own speed, QSO text) and on Ken 20-60 s and Bryan 140-180 s: the
cold read's starts as built (each dit start), and beside each its decode's likelihood, its share
of `E`, `T` and `■`, its learned gap classes after the first pass against the hand's sent or
measured gaps, and which start was taken. Then the same table for the starts §3 adds. The table
shows, per hand, whether the fault is the cold range (the hand's inside gaps fall outside
0.4-1.7 dits), the learning (the first pass's medians are wrong and later passes do not move),
or the choice (a better start existed and was not taken). Build §3 as the table directs; where
it says a part of §3 is not needed for any of the four, say so and leave that part out.

## 3. The change: the reader finds and learns the gaps

1. **The cold ranges fit a hand.** Inside gap 0.5 to 2.5 dits (was 0.4 to 1.7); letter gap 1.8
   to 6.5; word gap 5 to 30 (the `farnsworth` hand sends 21). The mark ranges do not change.

2. **The cold read starts over the gap line as well as the dit.** `ReadCold` tries, for each dit
   start it already tries, the inside/letter boundary at `ColdGapStarts` 1.5, 2.2 and 3.0 dits
   (the table says; three at most). The letter/word boundary stays where the ranges put it on
   the cold pass. CPU is reported; if the extra starts cost more than 20% on W1AW piece-04, the
   starts run only when the first pass's `E`, `T` and `■` share is above `ValidityShare` (say
   0.35), and the report says so.

3. **The winner is chosen by validity as well as likelihood.** Each start's decode gets a
   validity figure: the share of its letters that are `E`, `T` or `■`, and the share of its
   gaps that fall within ±20% of its own class boundaries (a decode that splits a hand has
   many). Among the starts whose validity is under `ValidityShare`, the likeliest is kept; if
   none is, the most valid is kept. The pattern test's figures are unchanged and still apply.

4. **The classes are learned by an ordered fit, not medians.** After each pass, all gaps in the
   window are fitted in log length by three ordered clusters (k-means in log length, three
   centres kept in order, at most `FitRounds` 10 rounds, started from the current class medians);
   the class boundaries are the **geometric** midpoints of adjacent centres (the arithmetic
   midpoint sits too high for a hand whose classes are close). Marks are fitted the same way
   with two clusters. A cluster that ends up with fewer than `LeastInCluster` 3 members keeps
   the previous boundary on that side. A fit whose centres come out closer than `GapStep`
   (the existing table figure) is refused and the pass keeps the previous classes.

5. **A warm lane that goes invalid re-reads cold.** The warm decode's validity (3) joins the
   pattern figures that already send a warm lane back to a cold read when they fall; the cold
   read uses the starts of (2).

Each figure named here goes in the reader's table beside the others, with the reason.

## 4. Rounds

Round one: the trace, then §3 as the trace directs, measured once (the survey, the boards, the
hard limits, the guards, CPU on W1AW piece-04). Rounds two and three: trace the largest loss on
the wild bank against the shipped reader to the hand, to the second, to the path; fix the one
thing the trace names; measure again. Twice at most.

## 5. The bar, and the ship

**The change ships (stays on; there is no switch for it) only if, at the end of the last round:**

- **the wild bank improves:** score **3,150 or better** (from 2,859), spaces added **250 or
  fewer** (from 400), invented not above 5; `bug` **510 or better** (from 459); `mobile`'s
  spaces added **50 or fewer** (from 110); no recipe worse than shipped by more than 10;
- **the old boards hold:** the twelve-plus-three **308** or better, W1AW **2101** or better,
  wrong and invented not rising, spaces not worse; the fist bank **3,898** or better and its
  guard; the fading half and the strong flat hands as guarded;
- **the hard limits:** noise prints nothing, the steady carrier at none, no old capture above
  six strays per 10 s; the summed strays reported; the random-keyed carrier reported; the 5 dB
  CQs 84 of 84; nothing in silences; the first recording whole;
- the easy fixtures 90 of 90; the working and edge fixtures reported; `two-station` 27 of 27;
  CPU under real time with margin, and the extra cold starts' cost said;
- **Ken and Bryan reported** per 20 s against the draft truth, not a condition.

**For the owner's ruling, not a fail:** everything holds but one old board under by no more
than 1% (305, 2080, 3,859), or everything holds but the wild bank between 3,000 and 3,150: put
the full figures under *For the owner's ruling*; leave the change in the tree behind a rule
(`CwRules.GapLearning`, off) and do not ship.

If it fails otherwise: say which, by how much, where, by path; leave the change behind
`CwRules.GapLearning`, off; keep the tag and the traces; say in one paragraph what the wild
bank still loses and whether it is the cold range, the learning, the choice, or a fourth thing.

If it ships: the wild bank joins the guards at the figures that ship less 5%
(`TheWildBankTests.TheBankHolds`: total, `bug`, `mobile`'s spaces added); both scoreboard
tables get their 593 row; `CW_SPEC.md` §2 says how the reader learns a hand's gaps, in a
paragraph.

---

## 6. The lines, the survey, the record

Run the app line, the engine line and the survey line once at the end; rewrite
`docs\cw-survey-2026-10-09.md` with a new `Rerun` line; every group before and after.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 593 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 593.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-297**, headline *The reader learns a hand's gaps*,
  naming what the trace found per hand, what was built, whether it shipped or waits on a ruling,
  and the figures before and after: the wild bank, the two real hands, the old boards.
- `docs\cw-scoreboard.md`: a 593 row in every table at the figures that shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-297.

---

## 7. Report

Section 2, for the owner, in plain words, leading with **the whole picture against the shipped
reader, row by row** - the wild bank per recipe, Bryan and Ken per 20 s with what they print
now, the old boards - whether it ships, and the *For the owner's ruling* heading if it applies;
then what the trace found, a sentence per hand. Section 1: per round. **Section 3: the trace
tables of §2 (before and after); the wild bank matrix; the two real hands' stretches; the
rounds' table; the survey's groups before and after; CPU with the starts' cost.** Section 4:
anything left, a line each.
