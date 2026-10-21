# Work instruction 583 - A held lane's marks are its station's: no level test on them

**Hand run. One fault, one yardstick with a truth, one rule, one commit, plus the record.**

**The fault, as the tree traced it** (`TheFadeTraceTests`, unit 582). On a hand under 12 dB of
QSB at 0.7 Hz, **the bank's lane reads the station almost whole** - 172 of 173 marks at 20 dB,
143 of 173 at 10 dB. **What breaks it is the gates:** the sender gate and the pattern gate hold a
mark to a sender only if its level is within 3 dB of the mean of his last three marks, and the
fade moves a station up to 7 dB between one mark and the next. So one station is dealt out
among new senders and **one letter prints per fade cycle**. At 20 dB a second blocker: the grid's
flat-top test (1.5 dB) rejects a dah that tilts on a slope, the grid hands on 99 of 173, and the
grid's standing sequence keeps the lane from being handed on, so 20 dB fading reads worse than
10 dB. The rule that let the tolerance grow with the sender's pace read the fading half 439 to
960 but admitted noise everywhere else (invented 46 to 202, W1AW 2111 to 2038) and ships off.

**The rule this unit builds is the one the architecture implies.** A lane is the station's own
matched filter at the station's own pitch; a mark a *held* lane calls is that station's mark by
construction. The level test exists to sort the grid's unowned marks into senders. **It has no
business re-judging a held lane's marks.**

**Standing rules** (HM-DEC-276) apply; and from HM-DEC-284, *every rule ships against a truth*.
Engine line 1500 s, survey 1800 s, `--no-build` after one build, build servers shut down first.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 583, ruling HM-DEC-287. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit583-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The one rule

**`CwRules.HeldLaneMarksAreItsStations`** (or the tree's naming): a mark handed on from a lane
that holds a station goes to that station's sender **without the level test** in either gate -
no 3 dB-of-the-last-three, no flat-top test - and its kind is judged by length against the
station's own clusters as now. Everything else about the mark is as now: it still counts toward
the hold's clock, the shape score, the carrier and shadow checks, the end of the hold; the
station is still let go by the gate's own release under shape 0.1, and the hold still ends in a
pause. **The grid's marks are untouched:** the level test and the flat-top test apply to them
exactly as now, so every steady sender read through the grid keeps every guard it has. Where a
held lane's station is also fed grid marks at its pitch (the handover already keeps the grid's
pieces out while a lane holds - confirm), nothing changes there.

**Measured with `CwRules.GridStepsAside` chosen on** (built, off, harmless on every board:
297, 2111, CQs 71, easy +2), because at 20 dB fading the grid stands and blocks the lane, and
the rule can only act once the lane holds the station. **If the pair ships, both ship.**

**The trace, first, on the same two fists** as the last unit (`fist-r2.5-20wpm-10db-fade`,
`-20db-fade`): per fade cycle, the marks the held lane handed on, how many the gates kept for
the station as shipped and with the rule, how many senders the station was dealt among, and
the letters printed. One table per fist, cycle rows, as before.

Build it, measure once.

## 3. The measure, and the ship

**Ships on by default (with `GridStepsAside`) only if:**

- **the bank's fading half rises from 439 of 2,880 to 1,200 or better**, and the flat half does
  not fall below 2,173 less 2% (2,129);
- **the bank's score rises from 1,891, invented not above 50**;
- **the working receiver fixtures rise from 4 of 81**, each read reported;
- **both boards hold:** 297 and 2111, wrong and invented not rising, spaces not worse;
- **the weak-end guard holds:** the four 5 dB CQs at 71 of 84 or better; **nothing in the 20 s of
  noise after them** (the last rule printed 32 letters there - this one must not); the
  short-dah rows and the nine hands hold their guards;
- **the hard limits:** noise prints nothing, the random carrier at no more than two seeds, the
  steady carrier at none, the first recording whole, nothing in silences, no old capture above
  six strays per 10 s; the summed strays reported;
- the easy fixtures not worse than 77 of 90.

If it fails, say which condition, by how much, which fist; leave it as a rule only a test can
choose; keep the trace. If everything holds but the fading half lands between 900 and 1,200,
put the figures under *For the owner's ruling*.

**If it fails on the fading half with the lane reading the station whole and the gates keeping
its marks, say so in one paragraph:** that is the evidence that the grid must retire as a finder,
and the next unit is that rebuild.

If it ships: the fading half joins `TheFistBankGuardsTests` as a floor at the figure that
shipped less 5%; the working fixtures at theirs; both scoreboard guards at the figures that
ship, on the board as the author's; the capture sheet's `bank` line says `held, marks its own`
for a held lane.

---

## 4. The lines, the survey, the record

Run the app line, the engine line and the survey line once; rewrite `docs\cw-survey-2026-10-09.md`
with a new `Rerun` line; the fist bank's matrices before and after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 583 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 583.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-287**, headline *A held lane's marks are its station's*,
  naming the rule, what the trace showed, whether it shipped with the grid stepping aside, and
  the figures before and after.
- `docs\cw-scoreboard.md`: a 583 row in each table at the figures that shipped.
- `CW_SPEC.md` §2: one sentence if it shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-287.

---

## 5. Report

Section 2, for the owner, in plain words, leading with **whether fading stations now read and
what it cost** - the signal-by-fade matrix before and after, the fading half's total, the
working fixtures, the boards, the noise after a weak CQ - then the rule in a sentence. Section 1:
the trace in brief, what was built. **Section 3: the two fists' cycle tables; the three matrices
before and after; the working fixtures; both scoreboards' rows.** Section 4: anything left, a
line each, and the one-paragraph verdict on the grid if it applies.
