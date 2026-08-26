STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 022 — Morse is quantised and noise is not

**ISSUED: 2026-08-26. A fresh order, not an amendment.**

**Four tasks; task 4 is the drop. Task 1 decides whether tasks 2 and 3 are
built at all.**

The operator's standing goal governs every part of this unit: **he hears CW,
Hamlet must decode it, eighty percent of the time.** This unit exists because
that goal currently fails at one step — a station he can hear is never admitted
— and unit 1.11.18 narrowed the reason to a single unanswered question.

## Why this unit exists

**The unit's number: fifteen, seventeen, ten and nineteen.**

Those are the marks today's gate already produces at the own pitches of the four
stations the operator can hear and Hamlet cannot read. **The gate is not missing
them.** Unit 1.11.18 measured that it produces about nineteen marks a pass in
*every* bin of *every* recording, including two holding nothing at all.

**So the whole failure reduces to one question: can a station's marks be told
from noise's marks?** Four answers have been measured and all four failed,
because all four are amplitude measures:

| axis | unit | result |
|---|---|---|
| cluster separation | 1.11.17 | station 1.75, silence 1.72 |
| dah/dit ratio | 1.11.17 | dominant refuser on one capture only |
| bin level spread | 1.11.18 | `N4L` reads at 10.4, silence sits at 12.0 |
| lift over band floor | 1.11.18 | `N4L` reads at 3.0, silence sits at 35.3 |

**`N4L` is an adjudicated callsign that reads, and an empty band outscores it on
both of the last two.** Amplitude cannot separate Morse from noise here.

**The untried family is structure.** Morse is quantised: marks fall at one unit
and three, gaps at one, three and seven. Noise crossing a threshold produces
runs at whatever length it likes — unit 1.11.18 measured them at 20 to 30 ms,
surviving a de-glitch whose floor is 20 ms. A statistic asking *"is this run
stream consistent with a single Morse unit?"* is **dimensionless by
construction**, which is the property unit 1.11.14 concluded any usable axis must
have after `marginLlr` failed for want of it.

**This tree already contains a working implementation of the idea.**
`tools/reference-decoder/` and `cwdecoder.py` refuse to emit without a clock
whose marks cluster as Morse — `fit_clock`, and the `well_separated` escape it
grew after a real 4.24-ratio station was refused. That history is the precedent,
not a new invention.

**Honest risk, stated up front: this is the fifth axis family.** If it fails,
the conclusion is that per-bin admission cannot work and the design must be
reconsidered a level above. **Task 1 finds that out before anything is built.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway. Unit 1.11.18 disproved its own
order's central premise and was right to; do the same.

**Adopt unit 1.11.18's restated measurement.** `Shut` — no marks and the gate
held open under 5% of the history, or no gate ran — reported beside `StuckOpen`
and `Truncated`, never summed. It is in the tree and every table here uses it.
**No task in this unit polishes that metric; it is only the ruler.**

**Expected state: 28 failing of 1841 in the engine, byte-identical to the stable
set; 503 of 503 in the app.** Six timing intermittents exist and two fired in
the last three runs. **Do not chase any of them.** Diff which tests moved rather
than trusting a total.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor
Tim's rulings of 2026-08-25/26.**

**`CLAUDE_CODE.md` is at version 1.4.** Read its own section count.

## Rulings in force

**Tim's ruling, 2026-08-26, by adopting this unit (flagged for veto in the
delivery): admission may be decided by whether a bin's run stream fits a single
Morse unit, rather than by how loud or how separated it is.** This is
HM-DEC-095's own principle — *a note is chosen by how it is keyed and never by
how loud it is* — carried further than the current tests carry it. **The
existing tests are not removed; the fit is measured first and added only if
task 1 shows it separates.**

**HM-DEC-120 is the acceptance test, in unit 1.11.18's stricter form:** both
silence controls emit nothing **and** their bins are `Shut` rather than
`StuckOpen`.

**Rejected already, do not revisit:** `MinimumSeparation`'s bound; the ratio
band; the admission valve; the threshold above the band floor (candidate A —
costs anchors at every setting and jams the gate open); the two-levels-apart
spread (candidate B — loses `cw-2026-08-24-012403` at every setting); the
integrator width; the confirmation window; the four dead squelch axes; locking
to `CwPitch`.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — does structure separate? Measure before building.

**This task decides the unit.** For each capture below, at the pitch named,
take the run stream today's gate already produces and report:

- the **run-length histogram** — marks and gaps separately, in milliseconds;
- the best-fitting single Morse unit and the **residual** of that fit —
  how far the runs sit from the nearest integer multiple of it, normalised by
  the unit so the figure is dimensionless;
- the same for **every bin in the band**, so the station's own bin can be
  compared with its neighbours on the same recording.

Measure on: the four stations the operator can hear — `cw-2026-08-25-012823` at
500, `cw-2026-08-22-014113` at 600, `cw-2026-08-22-014308` at 625,
`cw-2026-08-26-125941` at 400; the two silence controls `cw-2026-08-20-014854`
and `-014935`; and at least four adjudicated anchors including
`cw-2026-08-17-134712` (`N4L`, which every amplitude axis got wrong) and
`cw-2026-08-24-012403` (`DE KD0UN KD0UN K`, which candidate B kept losing).

**Then answer in one sentence: does the fit residual separate recordings holding
a station from recordings holding nothing, and by how much?**

- **If it separates**, tasks 2 and 3 are built.
- **If it does not, stop. Report the overlap, build nothing, and say plainly
  that five axis families have now failed and per-bin admission needs a ruling
  at a level above this unit.** That outcome is this unit's honest result and is
  reported as such, not as a failure.

Build and run; record the baseline by diffing which tests fail.

### Task 2 — the fit as an admission test *(only if task 1 separates)*

Add the fit residual to `CwToneSurvey`'s admission, **additively**: a bin the
current tests already admit is still admitted; a bin they refuse may be admitted
if its run stream fits a single unit well enough. Sweep the bound and report it.

**Acceptance, all of it:**

- **all four stations admitted at their own pitches**, and the pitch reported as
  measured;
- **both silence controls: nothing admitted, nothing emitted, bins `Shut` not
  `StuckOpen`**;
- **all twelve adjudicated anchors green, character for character** — including
  `012403`, which is where candidate B died;
- every floor held; chunk invariance intact.

**If no bound meets all of it, ship nothing and report the sweep**, naming
exactly which acceptance line each bound breaks.

### Task 3 — what they then read *(only if task 2 ships)*

Decode the four end to end and report characters, unsure, pitch, whether the
pitch was measured, and speed, against their floors of 41, 0, 0 and 0.

**A capture now admitted that still reads nothing is a finding, not a failure**
— it means the fault has moved downstream for the first time in this phase, and
it names where the next unit goes. Say so for each.

### Task 4 — the de-glitch against the speed being tracked *(the drop candidate)*

Unit 1.11.18 measured the de-glitch floor at 20 ms — a dit at 60 WPM — so noise
runs of 20 to 30 ms all survive. `cwdecoder.py` scales its de-glitch to the
clock it has fitted, at about 0.4 of a dit.

**Measure only**: what a speed-scaled de-glitch would remove on the two silence
controls and on the four stations, at 0.3 and 0.4 of the tracked dit. **No
change.** Dropped whole if time runs out, and the report says so.

## Parked — do not touch, do not raise

The restated metric itself; the six intermittents; the hop's precision problem;
confirmation; displacement; the hold; fist-quality selection; the meter; the
squelch's successor; the integrator width; the whole-file second pass;
`001520`'s quadrillions; the reference and port integrator difference; the
short-character bias; the Avalonia offset; `CHANGELOG.md`; HM-OPEN-057;
HM-OPEN-059; **the panel, entirely.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not build tasks 2 or 3 if task 1 does not separate.** Report and stop.
- **Do not remove or loosen an existing admission test.** The fit is additive.
- **Do not trade the silence property**, in either its emission form or unit
  1.11.18's stricter `Shut` form.
- **Do not fit the bound to the four target captures.** The anchors and the
  silence controls are the judge; the four are the motivation.
- **Do not chase an intermittent.**
- **Floors only rise; anchors stay green; chunk invariance holds; no panel
  change.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with task 1's sentence: whether the fit residual separates a
station from silence, with the numbers for `N4L` and both silence controls side
by side.** Section 2 says plainly whether a station the operator can hear now
reaches the decoder.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Fifteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26, including the one this unit acts under.**
5. **The tone tracker** — admission is this unit; confirmation, displacement and
   selection stay measured inert until it works.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures; the
   operator's noise session crossed it live on 2026-08-26.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's item five,
   the last of his list not yet attempted.
10. **The keying meter** — its measurement found a station its verdict denied,
    and it reads 44 words a minute off silence.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The acceptance metric restatement** — adopted here as the ruler.
13. **Neither spread nor lift separates a station from noise.**
14. **The gate opens on everything, including two empty recordings** — this
    unit tests the one remaining family.
15. **The de-glitch removes only 10 ms runs at a 10 ms hop** — task 4 measures
    the speed-scaled alternative.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference and
port integrator difference**; **an unmeasured pitch costs `N4L`**; **the
six-hertz window disagreement**; **the short-character bias**; **the Avalonia
geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.18**; **the whole-file
second pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**; **six timing intermittents**.

**If you finish every task, stop and report. Do not start the next unit.**
