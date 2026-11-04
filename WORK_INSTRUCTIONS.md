# Work instruction 596 - Two judges: words and the letter count together

**Hand run. One setting - the gap learning's validity judged by words and the `E`, `T`, `■`
share together, from the last unit's round-two form - up to three rounds, one commit per round
plus the record.** The tag `before-gap-learning` stands.

**Where it stands** (1.13.282, `CwRules.GapLearning` in the tree, off): the last unit's
**round two** (`93c58007`) was the best form the gap learning has had - the twelve-plus-three
**308** with 15 wrong, W1AW **2103**, the fist bank about 4,250, the easy fixtures **90 of 90**,
the first recording whole, the wild bank 3015, `bug` 321. Its **round three** (`c10b64ed`),
ranking non-valid starts by words found, let a 40 ms noise start win on a lucky `MT` or `IN`:
`bug` 280, the recordings 286 with 10 invented, the random carrier back to 20 seeds of 20.
**What the traces said:** the word share over twelve letters is binary - `TMN 40M TONITE` with
one letter wrong scores 0.00 like `TTTTT TETE` - so words alone cannot part a slightly-wrong
real hand from garbage; the `E`, `T`, `■` share parted them at the cold start (the carrier's 6
of 20 came from it) but refuses honest `TEST DE TEST`. **Each fails where the other holds. This
unit uses both.**

**Standing rules** (HM-DEC-276, HM-DEC-284) apply, with up to three rounds. Engine line 1500 s,
survey 2400 s, `--no-build` after one build per round, build servers shut down first, one test
run at a time; a run stopped for memory is run once more alone without asking.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted. The draft truth `cw-2026-10-19-141708.truth.md` is not edited.**

**Numbering.** This is unit 596, ruling HM-DEC-300. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit596-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- The change is the validity judge inside the gap learning and the vocabulary's list. The path,
  the fit, the marks, the mark finders, the lane bank, the column finder, the seam's rules, the
  keyer and the wild bank are not touched. Words decide nothing in the path.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The change

1. **Start from round two.** Revert the last unit's round three (`c10b64ed`) - one revert, said
   in the report - so a non-valid start set is chosen as round two chose it.

2. **Two judges.** A lane's text (its last `ValidityLetters` 12 letters, as built) is
   **invalid only when both say so**: the `E`, `T`, `■` share is above `ValidityShare` (the
   figure as built, 0.35) **and** the recognised-word share is under `ValidWords` 0.5. Either
   one alone leaves the text valid. Under `ValidityLeast` 4 letters, no verdict, as built. The
   same two-judge verdict at all three places: the cold start (among valid starts the
   likeliest; if none is valid, the likeliest of all - not the most recognised), the warm
   re-read (two passes running, as built), and the terminal (round two's rule: a refused lane
   takes the terminal only where no unrefused lane has marks to give).

3. **The words the traces named.** Add to the vocabulary: `TONITE TONIGHT 2NITE WKD WRKD
   SKED CONGRATS GUD GD NITE LID CUL TKS VY FINE` and a **band shape**: one to three digits
   followed by `M` (`40M 20M 160M`), and the frequency shape `7.040`-style (digits, a point,
   digits). `TheCwVocabularyTests` gains a case for each.

**The trace, first:** on the random-keyed carrier (one seed where round two printed and one
where the shipped reader printed), on `bug` at 20 WPM, 20 dB, 6 to 20 s, on `tightfist-easy`,
and on `221548`'s second station: the two shares, each judge's verdict, and the verdict
together, per window - so the report shows the carrier and the off-pitch lane refused, and the
honest text and the late starter kept.

## 3. Rounds

Round one: the change and the traces, measured once (the survey, the boards, the hard limits,
the guards, CPU on W1AW piece-04, **the random carrier measured both off and on**). Rounds two
and three: trace the largest loss against the shipped reader to the recording or hand, to the
second, to the path; fix the one thing the trace names inside the judge; measure again. Twice
at most. **No ranking by words among non-valid starts in any round.**

## 4. The bar, and the ship

**`GapLearning` ships on by default only if, at the end of the last round:**

- **the old boards hold:** the twelve-plus-three **308** or better, wrong not above 15, invented
  not above 4, spaces not worse than 92 of 110; **W1AW 2101** or better, wrong not above 24;
  the fist bank **3,898** or better and its guard; the fading half and the strong flat hands as
  guarded; **the easy fixtures 90 of 90**;
- **the wild bank:** score **3,000 or better**, spaces added 275 or fewer, invented not above 5,
  **`bug` 320 or better**, `machine` 580 or better, no recipe worse than shipped;
- **the hard limits:** noise prints nothing, the steady carrier at none, no old capture above
  six strays per 10 s; the summed strays reported; the random-keyed carrier reported both ways;
  the 5 dB CQs 84 of 84; nothing in silences beyond the one guarded; **the first recording
  whole**;
- the working and edge fixtures reported; `two-station` 27 of 27; CPU under real time with
  margin;
- **Ken and Bryan reported** per 20 s, not a condition.

**For the owner's ruling, not a fail:** everything holds but one of: wrong on the
twelve-plus-three 16 or 17; W1AW between 2080 and 2100; the wild bank between 2,950 and
3,000; `bug` between 300 and 319. Put the full figures under *For the owner's ruling*; leave
the rule off; do not ship.

If it fails otherwise: say which, by how much, where, by path; leave the rule off; keep the tag
and the traces; say in one paragraph what is still lost and whether it is the judge or a
second thing.

If it ships: the first recording's guard is restated to the whole read; both scoreboard guards
at the figures that ship; the wild bank joins the guards (`TheWildBankTests.TheBankHolds`:
total, `bug`, spaces added) at the figures that ship less 5%; the fist bank's guard rows move
up to the figures that ship less 5%; `TheCwVocabularyTests` and `TheGapLearningTests` go on the
carry-forward list; `CW_SPEC.md` §2 says how the reader learns a hand's gaps and how validity
is judged, a paragraph each; the `Weak stations` row is unchanged; the owner rebuilds and puts
it on the air.

---

## 5. The lines, the survey, the record

Run the app line, the engine line and the survey line once at the end; rewrite
`docs\cw-survey-2026-10-09.md` with a new `Rerun` line; every group before and after.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 596 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 596.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-300**, headline *Two judges*, naming the revert, the
  two-judge verdict, what the traces found, whether it shipped or waits on a ruling, and the
  figures before and after: the old boards, the easy fixtures, the wild bank, the two real
  hands, the first recording, the random carrier.
- `docs\cw-scoreboard.md`: a 596 row in every table at the figures that shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-300.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **the whole picture against the shipped
reader, row by row**, whether it ships, and the *For the owner's ruling* heading if it applies;
then what the traces found, a sentence each. Section 1: per round. **Section 3: the traces;
the wild bank matrix; the two real hands' stretches; the rounds' table; the survey's groups
before and after; CPU.** Section 4: anything left, a line each.
