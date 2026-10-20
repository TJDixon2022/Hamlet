# Work instruction 581 - The short dah, measured against the fist bank, and shipped

**Hand run. One rule already in the tree, one yardstick with a truth, one commit, plus the
record.**

**The hole, as the fist bank measures it** (unit 580): across 96 generated hands Hamlet reads
27%. **A hand whose dah is 2.0 dits reads 0 of 60 at every speed and every signal; a 1.6 hand
reads 7 of 1,440.** Together the two rows read 135 of 2,880. A 2.5 or 3.0 hand reads 60 of 60
flat at 20 dB at every speed. The gate demands a jump of two between a sender's two kinds, and
a short-dah fist never shows one.

**The rule, already built and off** (`CwRules.ShortDah`, unit 577): where no two neighbouring
lengths differ by two, two means in log length part the marks into two kinds, accepted when
judged over ten marks or more, each kind recurring three times, each cluster within 0.12 in log
length, the centres four spreads apart, and the dah at least 1.5 times the dit
(`MinDahDitRatio`). It read the 1.7 and 2.0 straight-key hands **60 of 60** from 0. **Its costs
when measured:** the twelve-plus-three 297 to 293, all on `cw-2026-10-08-121357` (`5 HN 55N` for
`RR UR 55N 55N`); `cq-25wpm-5db` 20 to 18; the random carrier printing at a second seed, 5207
(`■HAE ■NII`). None of the three was traced.

**Standing rules** (HM-DEC-276) apply; and from HM-DEC-284, *every rule ships against a truth*.
Engine line 1500 s, survey 1800 s, `--no-build` after one build, build servers shut down first.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 581, ruling HM-DEC-285. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      docs\cw-scoreboard.md
  MUST EXIST:      docs\cw-survey-2026-10-09.md
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
  once for a multi-line commit. Scripts go in `.run-unit\unit581-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. Measure the rule on the bank

With `ShortDah` chosen on, as it stands: the survey's fist bank - the three matrices and the
bank's total - beside the baseline (1,538 of 5,760, score 1,031). Both boards, the four 5 dB
CQs, the carrier at twenty seeds, the easy and working fixtures, the strays. One run.

## 3. Trace the three costs

One row each, at the moment the loss happens, with the rule's own figures (the two means'
centres, the spreads apart, each cluster's width, the dah over dit it found, the marks it judged):

1. **`cw-2026-10-08-121357`**, `RR UR 55N 55N` read `5 HN 55N`: which marks the rule re-kinded,
   and what the gate had kinded them as without it. This hand's true dah over dit, from the
   board's reference keying.
2. **`cq-25wpm-5db`**, 20 to 18 of 21: the same.
3. **The carrier at seed 5207**: the run of marks that passed the five conditions - their
   lengths, the two centres, the spreads, the widths - against the figures the bank's hands show
   (`WhereTheClustersStand`): which of the five conditions sits closest to the carrier's figure,
   and the margin a hand leaves above it.

**The paragraph:** whether one tightening (a condition's figure, or the two-means judged only
where the sender has shown a letter gap, or only where the sequence has already stood on its
kinds for N marks) takes all three costs away without touching the 1.6 and 2.0 rows of the bank,
or whether the costs are the rule's price. **Build the one tightening the table names, if it
names one**, and measure once more on the bank and the boards.

## 4. The ship

**Ships on by default if:**

- **the fist bank's 1.6 and 2.0 rows rise from 135 of 2,880 to 1,500 or better**, and the 2.5
  and 3.0 rows do not fall (1,403 of 2,880);
- **the bank's total score rises** (from 1,031), with invented not rising above 94;
- **both boards hold:** 297 and 2111, wrong and invented not rising, spaces not worse;
- **the weak-end guard holds:** the four 5 dB CQs at 71 of 84 or better; nothing in the 20 s of
  noise after them;
- **the hard limits:** noise prints nothing, **the carrier at one seed**, the first recording
  whole, nothing in silences, no old capture above six strays per 10 s; the summed strays
  reported;
- the easy fixtures not worse than 78 of 90.

**If everything holds but the carrier prints at two seeds, or the twelve-plus-three reads 293 to
296:** do not ship; put the full figures in section 2 under a heading *For the owner's ruling* -
the bank's rows before and after, the exact cost, and the tightening tried - so the owner can
rule on the cost against the gain. Leave the rule off.

If it ships: the 1.6 and 2.0 rows join the guards as a floor (`TheFistBankGuardsTests`: each row
not under the figure that shipped less 5%), the straight-key hand's test asserts 48 of 60 at each
seed, both scoreboard guards at the figures that ship, on the board as the author's; the capture
sheet's `clusters` line gains the dah over dit the gate measured.

---

## 5. The lines, the survey, the record

Run the app line, the engine line and the survey line once at the end; rewrite
`docs\cw-survey-2026-10-09.md` with a new `Rerun` line; the fist bank's matrices before and
after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 581 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 581.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-285**, headline *The short dah, measured against the
  fist bank*, naming the rule, the tightening if any, whether it shipped or waits on a ruling,
  and the figures before and after.
- `docs\cw-scoreboard.md`: a 581 row in each table at the figures that shipped.
- `CW_SPEC.md` §2: one sentence if it shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-285.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **whether short-dah fists now read, and
what it cost** - the 1.6 and 2.0 rows before and after, the bank total, the boards, the carrier
- then the rule in a sentence, and the *For the owner's ruling* heading if it applies. Section 1:
what was measured, the tightening. **Section 3: the three matrices before and after; the three
costs' rows; the carrier at twenty seeds; both scoreboards' rows.** Section 4: anything left, a
line each.
