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

# Work instruction 021 — the gate cuts noise in half and calls it Morse

**ISSUED: 2026-08-26. A fresh order, not an amendment.**

**Four tasks; task 4 is the drop.** The operator's standing goal governs every
part: **he hears CW, Hamlet must decode it.** This unit closes items one, three
and four of his list if it lands, because unit 1.11.17 proved they are one
fault.

## Why this unit exists

**The unit's number: 926 against nought.**

`cw-2026-08-17-013347` reads the adjudicated `VA3VRR`. Its gate stays **shut on
926 of 1,425 bin readings** — most of the band produces no marks at all — and
where it opens at 600 Hz it yields twelve marks, dit 92 ms, dah 275 ms,
separation 5.92, admitted twelve passes of fifty-seven, first time.

**On every other capture measured — four holding stations the operator can hear,
and two holding nothing whatsoever — not one bin in the band produces zero
marks.** The median is about nineteen a pass, in bins carrying only noise.

The cause, from unit 1.11.17's instrument: **the gate's threshold is derived
from each bin's own two levels.** A bin of pure noise has its noise split in
half and yields a stream of structureless marks. Every admission test
downstream then reasons about marks cut out of nothing — which is why
separation reports the same continuum, about 1.7, for a station the operator
can hear *and* for a recording of silence, and why **no bound separates them**:
the two silence controls' best bins reach 3.58 and 4.92 while three of the four
stations reach only 3.82, 3.03 and 5.87.

**This is why four consecutive units measured dead.** The admission valve, the
integrator sweep, the confirmation window, fist-quality selection — all
reasoning about a mark stream that is noise on most captures.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway.

**This unit changes the instrument this project measures everything else with.
HM-DEC-119 is explicit about that**, and it is the reason task 3 exists: every
anchor, every floor and both silence controls are re-measured after the change,
and the change is judged on them rather than on the four captures it was built
for.

**Expected state after unit 1.11.17: 29 failing of 1841 in the engine — 28 plus
a sixth intermittent in the rig path that passes alone — and 503 of 503 in the
app.** Six timing intermittents now exist. **Do not chase any of them**; when
counting, diff which tests moved rather than trusting the total.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor
Tim's rulings of 2026-08-25/26.** HM-DEC-095's index row is transcribed in unit
1.11.6's report. **Its principle — a note is chosen by how it is keyed, never by
how loud it is — is not questioned here.** The gate is not a loudness ranking;
it is the step that decides what counts as a mark at all, and it currently says
"mark" to noise.

**`CLAUDE_CODE.md` is at version 1.4.** Read its own section count for the
report shape.

## Rulings in force

**Tim's ruling, 2026-08-26, by adopting this unit (flagged for veto in the
delivery), in the words unit 1.11.17 asked for it:**

> **The gate's threshold is derived from each bin's own two levels, and in a bin
> holding only noise that splits the noise in half and manufactures marks. The
> threshold must be derived from something that knows the difference — the
> band's own noise floor, which `_bandNoise` already computes for the lift, or a
> requirement that a bin's two levels be far enough apart to be two things.**

**Both candidate derivations are built and measured; whichever meets task 2's
acceptance ships. If neither does, nothing ships and the measurement is the
answer** — that outcome names the next ruling and is worth as much as a fix.

**HM-DEC-120 is absolute and is the acceptance test, not a caveat.** Both
silence controls emit nothing, and — new, and stricter — **their bins must
mostly produce no marks at all**, which is what the healthy capture does.

**Rejected already, do not revisit:** moving `MinimumSeparation` (swept six
bounds across eight captures — no value separates); moving the ratio band; the
admission valve (rejected twice, grounds now corrected); the integrator width
(swept 20–120 Hz, settled 45); the confirmation window (swept 2–8 surveys);
Q75/P97 as a readable-station test; the four dead squelch axes; locking to
`CwPitch`.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — the signature, measured before anything changes

Unit 1.11.17's instrument already reports marks per bin per pass. Extend it into
the unit's acceptance metric and record the baseline for **every capture in the
corpus, not only the eight already measured**:

- bins producing **zero** marks, against bins producing eight or more;
- the median marks per pass;
- the same figures for both silence controls and for all twelve anchored
  captures.

**`cw-2026-08-17-013347` is the reference signature: 926 of 1,425 shut.** State
plainly how many captures resemble it today and how many resemble noise.

Build and run; record the green baseline by diffing which tests fail, not by
the total.

### Task 2 — a threshold that knows noise from keying

Build **both** derivations named in the ruling:

**A — from the band's noise floor.** `_bandNoise` already computes it for the
lift. A bin's mark threshold sits a stated distance above the band floor rather
than halfway between the bin's own two levels.

**B — two levels must be two things.** A bin whose two levels are not separated
by a stated minimum produces **no marks at all**, rather than marks cut from a
continuum.

Measure both, and both together, against this acceptance:

- **the silence controls go quiet at the source** — most bins producing zero
  marks, approaching the reference signature; emission still nothing;
- **the four stations produce marks at their own pitches** — `012823` at 500,
  `014113` at 600, `014308` at 625, `125941` at 400 — and their separation is
  then measured and reported against the bar of four;
- **all twelve anchors green, character for character**;
- **every floor held**; chunk invariance intact.

**Ship whichever meets it. If both fail, ship nothing and report both sweeps** —
including, for each candidate, what the stated distance would have to be to
admit the stations and what it costs in noise bins.

### Task 3 — re-measure the corpus, because the instrument moved

With whatever task 2 shipped: re-run every capture and report, against unit
1.11.17's figures, the marks-per-bin signature, admission counts at each
station's pitch, and the decode — characters, unsure, pitch, whether the pitch
was measured — for all four target captures and all twelve anchors.

**A capture whose gate is now healthy and still does not read is a finding, not
a failure**: it means the fault has moved downstream, and it names where the
next unit goes. Say so explicitly for each of the four.

### Task 4 — the survey's history hop *(the drop candidate)*

Unit 1.11.15 found the survey's ten-millisecond history hop cannot resolve a
31 ms dit. **Measure only**: what the hop is, what dit lengths it can and cannot
resolve, and which captures in the corpus send faster than it can follow.
No change. Dropped whole if time runs out, and the report says so.

## Parked — do not touch, do not raise

Admission tests other than the gate's threshold; the ratio band; separation's
bound; confirmation; displacement; the hold; fist-quality selection; the meter;
the squelch's successor; the integrator width; the whole-file second pass;
`001520`'s quadrillions; the reference and port integrator difference; the
short-character bias; the Avalonia offset; `CHANGELOG.md`; **all six
intermittents**; HM-OPEN-057; HM-OPEN-059; **the panel, entirely.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not trade the silence property.** It is this unit's acceptance test in a
  stricter form than ever before.
- **Do not touch any admission test other than the gate's threshold.**
- **Do not fit the threshold to the four target captures.** The anchors and the
  silence controls are the judge; the four are the motivation.
- **Do not chase an intermittent.**
- **Floors only rise; anchors stay green; chunk invariance holds; no panel
  change.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with two numbers: how many bins produce zero marks on the two
silence controls, before and after; and how many of the four stations the
operator can hear now produce marks at their own pitches.** Section 2 says
plainly whether a station he can hear now reaches the decoder.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Fourteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26, including the one this unit acts under.**
5. **The tone tracker** — the gate is this unit; confirmation, displacement and
   selection stay measured inert until admission works.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures; the
   operator's noise session crossed it live on 2026-08-26.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's item five,
   the last of his list not yet attempted.
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on noise in every bin** — **this unit acts on it.**
13. **Six intermittents.** A full-run count can no longer be read without
    diffing which tests moved. Worth its own small unit.
14. **The survey's ten-millisecond history hop cannot resolve a 31 ms dit** —
    task 4 measures it.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference and
port integrator difference**; **an unmeasured pitch costs `N4L`**; **the
six-hertz window disagreement**; **the short-character bias**; **the Avalonia
geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.17**; **the whole-file
second pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**; **the speed ceiling may be short for a 36–43 WPM station**.

**If you finish every task, stop and report. Do not start the next unit.**
