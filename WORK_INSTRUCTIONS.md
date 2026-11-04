# Work instruction 594 - The gap fit alone: marks stay with the medians

**Hand run. One setting - the gap learning with the mark fit taken out - up to three rounds, one
commit per round plus the record.** The tag `before-gap-learning` stands.

**Where it stands** (1.13.280, `CwRules.GapLearning` in the tree, off): with the rule on, the
first recording reads whole for the first time (`FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`),
the fist bank **4634** from 3898, the fading half 2088 from 1771, the twelve-plus-three **313**
from 308, the wild bank 2944 from 2859 with spaces added 267 from 400, `machine` 588 whole,
`steady` 573, `tight` 505, Bryan 123 from 118. **And** `bug` **254** from 345, Ken 37 from 43,
W1AW 2098 from 2101, wrong up two on each board. The trace named the loss: `bug`'s dah centre
slides from 250 to 165 ms as a window fills, his dah share climbs to 0.6, and his long dits read
as dahs. That is the **two-cluster mark fit** that went in beside the gap fit; on a hand with a
dit spread of 0.37 the two mark clusters do not hold still where a median does. The gap fit
moved five recipes the right way. **This unit takes the mark fit out and measures the gap fit
alone.**

**Standing rules** (HM-DEC-276, HM-DEC-284) apply, with up to three rounds. Engine line 1500 s,
survey 2400 s, `--no-build` after one build per round, build servers shut down first, one test
run at a time; a run stopped for memory is run once more alone without asking.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted. The draft truth `cw-2026-10-19-141708.truth.md` is not edited.**

**Numbering.** This is unit 594, ruling HM-DEC-298. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit594-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- The change is inside the sequence reader's gap learning. The mark finders, the lane bank, the
  column finder, the seam's rules, the keyer and the wild bank are not touched.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The change

**With `GapLearning` on, marks are learned as they are with it off**: dit and dah as the medians
of the marks the last pass called dits and dahs, the dit/dah boundary where the shipped reader
puts it. The two-cluster mark fit goes - removed, not switched. Everything else the rule holds
stays as built: the cold ranges that fit a hand, the start chosen by validity, the ordered
three-cluster gap fit with geometric boundaries and `FitLetterStep` 2.0, word gaps laid as
nobody keying fitted up to `FitWordGapMost`, a warm lane that goes invalid read cold.

**The trace, first, on `bug`** (20 dB, own speed, QSO text, 40 to 66 s, where the dah centre
slid): per pass, the dit and dah the medians give against the dit and dah the fit gave, the dah
share, and the letters right - so the report shows the slide is gone. **And on Ken 60-80 s**,
where the rule on read 8 against 13: the start taken and its validity, the learned classes per
pass against his measured 136 / 344 / 608, and the path's letters against the draft truth, one
row per letter that differs, naming whether each is a mark or a gap fault.

## 3. Rounds

Round one: the change and the two traces, measured once (the survey, the boards, the hard
limits, the guards, CPU on W1AW piece-04). Rounds two and three: trace the largest loss on the
wild bank against the shipped reader to the hand, to the second, to the path; fix the one thing
the trace names inside the gap learning; measure again. Twice at most. **No change to how
marks are learned in any round.**

## 4. The bar, and the ship

**`GapLearning` ships on by default only if, at the end of the last round:**

- **the wild bank:** score **3,050 or better** (from 2859), spaces added **270 or fewer**,
  invented not above 5; **`bug` 345 or better** (shipped) and `farnsworth` **545 or better**;
  no recipe worse than shipped by more than 10;
- **the old boards hold:** the twelve-plus-three **308** or better, W1AW **2101** or better,
  wrong not above 13 and 24, invented not rising, spaces not worse; the fist bank **3,898** or
  better and its guard; the fading half and the strong flat hands as guarded;
- **the hard limits:** noise prints nothing, the steady carrier at none, no old capture above
  six strays per 10 s; the summed strays reported; the random-keyed carrier reported, measured
  **both off and on** this time; the 5 dB CQs 84 of 84; nothing in silences beyond the one
  guarded; **the first recording whole**;
- the easy fixtures 90 of 90; the working and edge fixtures reported; `two-station` 27 of 27;
  CPU under real time with margin;
- **Ken and Bryan reported** per 20 s against the draft truth, not a condition.

**For the owner's ruling, not a fail:** everything holds but one old board under by no more
than 1% (305, 2080, 3,859), or everything holds but wrong on one board up by one or two, or
everything holds but the wild bank between 2,950 and 3,050: put the full figures under *For the
owner's ruling*; leave the rule off; do not ship.

If it fails otherwise: say which, by how much, where, by path; leave the rule off; keep the
tag and the traces; say in one paragraph what the wild bank still loses and whether it is the
gap fit, the marks, or a third thing.

If it ships: the first recording's guard is restated to the whole read; both scoreboard guards
at the figures that ship; the wild bank joins the guards (`TheWildBankTests.TheBankHolds`:
total, `bug`, spaces added) at the figures that ship less 5%; the fist bank's guard rows move
up to the figures that ship less 5%; `CW_SPEC.md` §2 says how the reader learns a hand's gaps,
in a paragraph; `TheGapLearningTests` goes on the carry-forward list.

---

## 5. The lines, the survey, the record

Run the app line, the engine line and the survey line once at the end; rewrite
`docs\cw-survey-2026-10-09.md` with a new `Rerun` line; every group before and after.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 594 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 594.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-298**, headline *The gap fit alone*, naming what the
  two traces found, that the mark fit was removed, whether it shipped or waits on a ruling, and
  the figures before and after: the wild bank, the two real hands, the old boards, the first
  recording.
- `docs\cw-scoreboard.md`: a 594 row in every table at the figures that shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-298.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **the whole picture against the shipped
reader, row by row** - the wild bank per recipe, Bryan and Ken per 20 s with what they print,
the old boards, the first recording - whether it ships, and the *For the owner's ruling* heading
if it applies; then what the two traces found, a sentence each. Section 1: per round.
**Section 3: the `bug` trace (medians against the fit, per pass); the Ken trace; the wild bank
matrix; the two real hands' stretches; the rounds' table; the survey's groups before and after;
CPU.** Section 4: anything left, a line each.
