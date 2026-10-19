# Work instruction 565 - The word line sits between the sender's two gap clusters

**Hand run. One small unit, two tasks, commit per task. Task 1 is the unit; task 2 drops from the
back.** Written 2026-10-08, re-presented 2026-10-19 unchanged in substance; the figures below are
as the board stands today (1.13.258). **Standing rules** (HM-DEC-276): one setting per unit - the
"report beside" figures in step 2 are dropped, build the rule as stated and measure it once; do
not stop to ask - take the choice that keeps both boards and name it in section 4; a run stopped
for memory is run once more alone, then reported as not measured. Nothing here touches the radio, the marks, the windows or the lookup table: **it moves one
line, the one that tells a letter gap from a word gap**, and it is measured before it is moved.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. No recording is added.**

**Numbering.** This is unit 565, ruling HM-DEC-269. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      docs\cw-scoreboard.md
  MUST EXIST:      docs\carry-forward-tests.txt
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
  once for a multi-line commit. Scripts go in `.run-unit\unit565-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Why

**The letters are there now; the spaces between words are not.** The owner, 2026-10-08, parked on
the SKCC frequency with the radio's AGC on SLOW, read a whole straight-key QSO from the screen for
the first time - both calls, the report, the state, the name, the weather, the sign-off - and the
thing between readable and clean was this:

```
QTHWVWVNAMEIS JOEL          QTH WV WV NAME IS JOEL
GAESTUURRST5 995NN          GA ES TU UR RST 599 NN
DEKE8WIC GUD CPY            DE KE8WIC GUD CPY
```

The letters are right. The spaces are missing. **The scoreboard says the same**: on the twelve plus
KM3STU's three, **86 of 110 word spaces, 3 added** (the lane handover gave back three on 2026-10-14), and the spaces-by-cause trace in the tree
(`TheSpacesByCauseTests`) already names where the twenty went:

| cause | count |
|---|---|
| a word gap **under the line**, the sender's word cluster already shown | 5 |
| a word gap **under the line**, no word cluster yet | 4 |
| a letter gap **over the line**, a space added | 3 |
| a word gap as short as the sender's letter gaps - invisible in the timing | 6 |
| a letter either side not printed | 5 |

**Nine of twenty missing, and all three added, are the line**: a gap of 252 ms under a line at 254;
283 under 319; 372 and 424 under 508; 285 under 300; 305, 380 and 405 under 463; 356 under 399. The
sender's letter gaps cluster well below each of those, and his word gaps cluster above. **The line
is sitting at the bottom of the word cluster, not between the two clusters.** Hand senders - the
SKCC straight key most of all - leave word gaps nowhere near seven dits; the line has to come from
the sender's own two clusters, not from the dit.

The six invisible and the five unprinted stay as they are: nothing in the timing can find the first,
and the second is the letter fault, not the space fault.

---

## 3. Task 1 - the line between the clusters

1. **Measure first, on the gate's own gap kinds, and put the table in output.md section 3.** For
   each of the nine under-the-line gaps and the three over-the-line gaps named above, at the moment
   the gap is judged: the sender's letter-gap cluster (its centre and its top, the 90th
   percentile of the letter gaps held), his word-gap cluster if one stands (its centre and its
   bottom, the 10th percentile), **where the line was**, and the gap itself. One row each. Name how
   the line is set today, in one sentence, with the file and the rule.
2. **Move the line to the sender's own two clusters.** The rule, to be confirmed or corrected by the
   table:
   - **Both clusters standing:** the line is the **geometric mean of the letter cluster's top and
     the word cluster's bottom** - the same place in the log of time that the gap kinds already
     use for their jump. A gap is a word gap above it, a letter gap below.
   - **Only the letter cluster standing** (the sender's first words, or a sender who never leaves a
     clear word gap): the line is the letter cluster's top **times the gap kinds' own jump (√3)**.
     This is the case for `221745` and `DAYS`: 305, 380 and 405 under 463, 356 under 399, with no
     word cluster yet.
   - **The clusters are the sender's, held over the same last-ten-marks memory the kinds use**, so
     a change of hand moves the line with it and nothing is fixed to the dit or to the speed.
   - **Added spaces:** a letter gap over the new line (`6CHA` 541 over 277 is a real pause inside a
     word; `SCOU` 453 over 314) - count them. If the new line adds more than it finds, say so and
     do not ship; if it finds nine and adds three, that is the trade and it ships.
3. **Measure after.** The scoreboard's spaces column on both tables, letters right, wrong and
   invented. **Ships if the twelve-plus-three total rises and the W1AW table does not fall**:
   letters right (318 of 353, 2131 of 2151) must not drop by one, wrong and invented must not rise.
   W1AW's spaces (438 of 440, 11 added) may not get worse. The four 5 dB CQs stay at half or better. If the line cannot be placed without
   costing a letter, **trace it in section 3 and ship nothing**; the owner rules.
4. **The hard limits stand:** noise prints nothing, the carrier at no more than one of twenty
   seeds, the first recording whole, nothing in silences.
5. **Tests:** the synthetic hand whose letter and word gaps touch
   (`AHandWhoseLetterAndWordGapsTouchKeepsItsWords`) stays at or above 12, 14 and 13 of 14; add
   one that sets a straight-key hand at 15 WPM with letter gaps of 2 to 4 dits and word gaps of
   4 to 6 - the SKCC hand - and counts its word spaces at three seeds, before and after, in the
   report. Both scoreboard guards pass at the new figures and the figures ship on the board as
   the author's. Rerun the survey once and put the groups before and after in the report.

## 4. Task 2 - the clusters on the sheet

Every capture sheet (Record and the automatic ones) gains one line per printed sender: **letter
cluster centre and top, word cluster centre and bottom, the line, in ms**, so tonight's SKCC
captures on SLOW can be read tomorrow without a replay. One line, the same shape as the pitch and
speed lines already there. **Drop candidate.**

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 565 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 565.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-269**, headline *The word line sits between the sender's
  two gap clusters*, quoting the owner's SKCC read, naming the rule and the figures before and
  after.
- `docs\cw-scoreboard.md`: a 565 row in each table, at the new figures.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **whether more word spaces are found and at
what cost in letters**, the two totals before and after, and what the SKCC lines above would read
now. Section 1: per task, what was done, the rule in one sentence, where the line was and where it
is. **Section 3: the twelve-gap table from step 1 with the new line added as a column; the
synthetic hands' counts; then both scoreboards' rows.** Section 4: anything left, a line each,
including the six invisible gaps and the five unprinted letters by name.
