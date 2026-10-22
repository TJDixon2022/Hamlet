# Work instruction 586 - One finder, the opening: a lane hears a station from its first mark

**Hand run. One finder (`CwRules.BankFinds`, off), one cause, up to three rounds, one commit per
round plus the record. The owner's ruling, 2026-10-21: one more unit on one finder, on the
opening alone, with a hard stop.** The tag `before-one-finder` stands.

**Where one finder stands** (unit 585, round three, against two finders as shipped): the
hand-sent recordings **253 against 297**; W1AW 2098 with 8 invented against 2111 with none; and
everything else better - the short-dah rows 832 and 886 against 569 and 624, the 5 dB CQs 80 of
84 against 71, the fading half 1,166 against 439, the working fixtures 17 against 4, the random
carrier at no seed against two, the strays 12.8 against 26.8.

**The one cause, in the unit's own words:** *a bank lane calls nothing until its own contrast,
taken over three seconds of its own history, clears 12 dB. A station that starts after quiet
therefore loses its first letters, where the grid read them at once.* The `F` of `FER`, the `N
T` of `N TEMP`, `RR` as `EENR`, the second station's first words on `221530` (31 to 15). On
`221530` the station began near 9 s and its own lane called no mark until 10.2 s. **Second**
come noise lanes held at that same floor while nothing louder keys (`TENTESIS`, 8 invented, 3
in silences). Loudest first moved none of it, since at an opening there is nothing louder yet.

**Why the grid reads an opening.** Its bins run all the time, so the noise floor under a pitch
is known before the station keys, and a mark is a mark the moment it rises over it. A lane runs
all the time too, so its key-up (the 30th percentile of three seconds) is known just as well.
What a lane lacks at an opening is a key-down: the 90th percentile of three seconds of mostly
noise does not move until a third of a second of key-down has gone into it, and the contrast
does not clear 12 dB until well after. **The lane's floor is right; its ceiling is late.**

**Standing rules** (HM-DEC-276, HM-DEC-284) apply, with up to three rounds. Engine line 1500 s,
survey 1800 s or more as this unit finds (one finder slowed the survey to 21 minutes last unit;
if round one's measure shows the cost is the failing marks fed to the gates, say so and take
the cheaper of the two forms that read the same), `--no-build` after one build per round,
build servers shut down first. One test run at a time.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.**

**Numbering.** This is unit 586, ruling HM-DEC-290. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit586-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The trace, first

**The openings**, with `BankFinds` on as it stands, on the first recording (`FER`), `144020`
(`N TEMP`), `221828` (`RR`, `FER`), `221530`'s second station, and W1AW `piece-02`'s `QST`
after its pause: for the station's own lane, every 50 ms from one second before the station's
first keyed mark to its first printed letter - the lane's level, its key-up line, its key-down
line, its contrast, and whether the mark then keyed (from the board's keying map) was called.
Beside it the grid's bin at that pitch: its floor and when it called the first mark. One table
per opening, condensed to one row per keyed mark, with the full rows in `.run-unit\reports\`.

**The paragraph:** how long after the first keyed mark the lane's key-down line reaches the
station's level, how long until its contrast clears 12 dB, and which of the two is the delay
that loses the letters. Name the one change.

## 3. The one rule

The likely shape, for the trace to confirm or correct - **`CwRules.LaneHearsTheOpening`**:

- A lane's **key-up** stays as it is: the 30th percentile of its three-second history, known
  before any station keys.
- A lane's **key-down**, while it holds nothing, is **the greater of** the 90th percentile of
  its history **and the top of the last mark-length of hops** (the median of the top quarter of
  the hops in the last `OpeningHops`, a dah's worth at the lane's dit), so that the first dah of
  a station lifts the ceiling at once and the contrast is read against the station's own level
  from its first mark.
- **The contrast floor (12 dB) is judged on that contrast**, so a station 12 dB over the lane's
  known floor stands on its first marks rather than a second later.
- **Noise lanes:** a ceiling lifted by one noise burst falls back as soon as the burst ends,
  because the last mark-length of hops no longer holds it, so a lane stands on noise no more
  than it does now; and the grid's tests on lanes, loudest first and two-lanes stay as they are.
  If the trace shows the lifted ceiling lets noise lanes stand more, the second round takes
  that.

Build it, measure once.

## 4. Rounds two and three

If round one fails the bar, trace the single largest loss against two finders as shipped (board
or group, to the stretch or fist, to the second), fix the one thing the trace names, commit,
measure again. Twice at most. Each round's figures go in the report's table.

## 5. The bar, and the ship - the hard stop

**`BankFinds` ships on by default only if, at the end of the last round:**

- **both boards hold:** the twelve-plus-three **297** or better, W1AW **2111** or better, wrong
  and invented not rising, spaces not worse;
- **the fist bank's score 2,500 or better**, invented not above 50; **the fading half 1,000 or
  better**; the flat half not below 2,129;
- **the weak-end guard holds:** the four 5 dB CQs at 71 of 84 or better; nothing in the 20 s of
  noise after them; the short-dah rows and the nine hands hold their guards;
- **the hard limits:** noise prints nothing, the random carrier at no more than two seeds, the
  steady carrier at none, the first recording whole, nothing in silences, no old capture above
  six strays per 10 s; the summed strays reported;
- the easy fixtures not worse than 77 of 90; the working fixtures reported; `two-station`
  reported; the CPU of the survey with one finder reported against two.

**For the owner's ruling, not a fail:** if everything holds but one board is under by no more
than 1% (the twelve-plus-three **294** or better, W1AW **2090** or better), do not ship; put the
full figures under *For the owner's ruling* with the stretch that fell and why.

**The hard stop:** if the twelve-plus-three is under 294 at the end of the last round, say so in
one line at the top of section 2, leave `BankFinds` off, keep the tag and every one-finder rule
in the tree behind it, and end the one-finder line. No further one-finder unit will be issued
without a new ruling; the next units go to what the owner sees on the air with two finders.

If it ships: both scoreboard guards at the figures that ship, on the board as the author's; the
fading half joins `TheFistBankGuardsTests` as a floor at the figure that shipped less 5%;
`CW_SPEC.md` §2 is rewritten for one finder; the seam's rules are listed for removal in the
next unit; the `Weak stations` row reads as the layout allows.

---

## 6. The lines, the survey, the record

Run the app line, the engine line and the survey line once at the end; rewrite
`docs\cw-survey-2026-10-09.md` with a new `Rerun` line; the fist bank's matrices and every
group before and after in the report.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 586 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 586.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-290**, headline *One finder, the opening*, quoting the
  owner's ruling and the hard stop, naming the rule, the rounds, whether it shipped, waits on a
  ruling, or ended the line, and the figures before and after.
- `docs\cw-scoreboard.md`: a 586 row in each table at the figures that shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-290.

---

## 7. Report

Section 2, for the owner, in plain words, leading with **the whole picture, one finder against
two, row by row** (W1AW, the hand-sent recordings, a short-dah fist, a weak CQ, a fading station,
the far side, the carrier) and whether it ships, waits on a ruling, or stops. Then the rule in a
sentence and what the openings read now. Section 1: per round. **Section 3: the openings'
tables; the rounds' table (every condition, each round, two finders); the three matrices; the
easy and working fixtures; both scoreboards' rows.** Section 4: anything left, a line each.
