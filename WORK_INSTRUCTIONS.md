# Work instruction 582 - The fade: a lane follows the signal it is reading

**Hand run. One fault, one yardstick with a truth, one rule, one commit, plus the record.**

**The hole, as the fist bank measures it** (1.13.267): across every ratio and speed, **a hand
under 12 dB of QSB at 0.7 Hz reads about a fifth of what it reads flat** - 439 against 2,173 of
2,880. The 2.5 hand at 20 WPM reads 53, 59 and 60 flat at 5, 10 and 20 dB and 0, 14 and 13
fading. Signal level is not the problem; a clean fist holds to 5 dB flat. **The fade is.** It is
the largest hole left, and the working receiver fixtures (5 dB with the same fade, 4 of 81) are
the same hole.

**What the tree already knows about why.** A lane's key-up and key-down are the 30th and 90th
percentiles of its own levels over the last three seconds, refreshed every 250 ms
(`CwLaneBank.HistoryHops`). A fade at 0.7 Hz swings the level 12 dB every 0.7 s, so a three-second
history straddles two peaks and two troughs: the key-down line sits at the peaks, the key-up at
the troughs, and in a trough the station's marks fall under 0.4 of that contrast and are not
called; at a peak the noise rises over 0.6 and is. The weak-end trace said it: *the bank's
three-second key-up and key-down do not follow a fade*. The same holds for the grid's level
rule where the station is read there.

**Standing rules** (HM-DEC-276) apply; and from HM-DEC-284, *every rule ships against a truth*.
Engine line 1500 s, survey 1800 s, `--no-build` after one build, build servers shut down first.

**The RSID real-time test** (`TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`): the owner's
ruling, 2026-10-21 - it measures its own thread's time, not the process's. Make that change in
this unit's first commit and record it under HM-DEC-286 with the rest.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 582, ruling HM-DEC-286. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit582-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The trace, first

**On `fist-r2.5-20wpm-10db-fade`** (reads 14 of 60; flat it reads 59) and
**`fist-r2.5-20wpm-20db-fade`** (13 of 60; flat 60), through the lane that holds the station:
every 250 ms, the lane's key-up and key-down lines, its contrast, the fade's true level at that
moment from the recipe, and for every mark the recipe keyed in that window whether the lane
called it, called it short, or missed it, and for every mark the lane called that the recipe did
not key, its length. One table per fist, condensed to one row per fade cycle (1.4 s) with the
counts, and the full rows in `.run-unit\reports\`.

**The paragraph:** how far the lines lag the fade (in ms and in dB), what share of marks are
missed in troughs and what share of noise marks are called at peaks, and whether the lines
would follow the fade if the history were as long as a word gap rather than three seconds, or
if the lines were set on the last few marks' own tops and the gaps' own floors instead of
percentiles of everything. Name the one change.

## 3. The one rule

The likely shape, for the trace to confirm or correct: **a lane that holds a station sets its
lines on the station's own marks, not on three seconds of everything** - key-down is the median
top of the last `FollowMarks` marks it called (say eight, the table says), key-up the median
floor of the gaps between them, each refreshed as every mark ends, so the lines move with the
signal within a few marks, and a fade of 0.7 Hz at 20 WPM (about twenty marks a cycle) is
followed. The contrast floor (12 dB) still governs standing; a station already held keeps its
lines on its own marks until the hold ends. Where no station is held, the lane's lines are as
now. If the grid's level rule for a printed station (its own window) uses the same
three-second percentiles, give it the same change; if it already follows the marks, say so.

Build it as one named rule, measure once.

## 4. The measure, and the ship

**Ships on by default only if:**

- **the bank's fading half rises from 439 of 2,880 to 1,200 or better**, and the flat half does
  not fall below 2,173 less 2%;
- **the bank's total score rises**, invented not above 46 plus 10%;
- **the working receiver fixtures rise from 4 of 81**, reported with each read;
- **both boards hold:** 297 and 2111, wrong and invented not rising, spaces not worse;
- **the weak-end guard holds:** the four 5 dB CQs at 71 of 84 or better; nothing in the 20 s of
  noise after them; the short-dah rows and the nine hands hold their guards;
- **the hard limits:** noise prints nothing, the random carrier at no more than two seeds, the
  steady carrier at none, the first recording whole, nothing in silences, no old capture above
  six strays per 10 s; the summed strays reported;
- the easy fixtures not worse than 77 of 90.

If it fails, say which condition, by how much, which fist; leave it as a rule only a test can
choose; keep the trace. If everything holds but the fading half lands between 900 and 1,200,
put the figures under *For the owner's ruling* as unit 581 did.

If it ships: the fading half joins `TheFistBankGuardsTests` as a floor at the figure that
shipped less 5%; both scoreboard guards at the figures that ship, on the board as the author's.

---

## 5. The lines, the survey, the record

Run the app line, the engine line and the survey line once; rewrite `docs\cw-survey-2026-10-09.md`
with a new `Rerun` line; the fist bank's matrices before and after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 582 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 582.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-286**, headline *The fade: a lane follows the signal it
  is reading*, naming the rule and `FollowMarks`, what the trace showed, whether it shipped, the
  figures before and after, and the RSID test's ruling.
- `docs\cw-scoreboard.md`: a 582 row in each table at the figures that shipped.
- `CW_SPEC.md` §2: one sentence if it shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-286.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **whether fading stations now read and
what it cost** - the signal-by-fade matrix before and after, the fading half's total, the
working fixtures, the boards - then the rule in a sentence. Section 1: the trace in brief, what
was built. **Section 3: the two fists' cycle tables; the three matrices before and after; the
working fixtures; both scoreboards' rows.** Section 4: anything left, a line each.
