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

# Work instruction 023 — the survey reads the wrong thing

**ISSUED: 2026-08-26. A fresh order, not an amendment.**

**Four tasks; task 4 is the drop. Task 1 decides whether tasks 2 and 3 are
built at all.**

The operator's standing goal governs every part: **he hears CW, Hamlet must
decode it, eighty percent of the time.** Items one, three and four of his list
all reduce to a station he can hear never being admitted, and unit 1.11.19
narrowed that to a single cause worth testing.

## Why this unit exists

**The unit's number: worse than random, on every recording.**

Unit 1.11.19 fitted the survey's own run stream to a single Morse unit and
measured the statistic against its null:

| stream | residual |
|---|---|
| generated Morse, no jitter | **0.000** |
| generated Morse, 30% jitter | 0.182 |
| **uniform random lengths** | **0.230 – 0.257** |
| **every real capture in the corpus** | **0.258 – 0.409** |

The statistic is sound — it scores nought on real Morse and 0.23 on random
numbers. **It reads the survey's stream as worse than random on all ten
captures tested, including four that decode adjudicated callsigns.** The two
silence controls, at 0.318 and 0.327, fit Morse *better* than
`cw-2026-08-17-134712`, which reads `N4L`.

**So five axis families have failed for one reason, and it is not the choice of
statistic.** The survey's gate output is not a description of keying on any
recording in this corpus. No measure computed from it can separate anything.

**Meanwhile Hamlet reads `VA3VRR`, `N4L`, `AA4MP/4 QNIK` and the ARRL bulletin
every day — from a different measurement.** The probabilistic path mixes down
at a pitch, integrates through a Hann window, and recovers characters from that
envelope. The survey computes its own gate from raw bin levels with a
hysteresis band, and that is what produces about nineteen structureless runs a
pass in every bin of every recording, empty band included.

**This unit asks the question that follows: is the probabilistic path's envelope
quantised where the survey's stream is not?** If it is, the survey has been
reading the wrong thing all along and the fix is to feed it the right one.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway. The last two units each disproved
their own order's premise and were right to.

**Corrections carried from unit 1.11.19, because the previous order got them
wrong:** `fit_clock` and `well_separated` are in `cwdecoder.py` at the
repository root, not in `tools/reference-decoder/`. `well_separated` is a
*scatter* test, the same family as `MinimumSeparation` and already rejected —
not the quantisation test the previous order claimed as precedent.
`cw-2026-08-24-012403` sits under `unadjudicated/`.

**Expected state: 28 failing of 1841 in the engine, byte-identical to the
stable set; 503 of 503 in the app.** Six timing intermittents exist; none fired
last run. **Do not chase any of them.** Diff which tests moved rather than
trusting a total.

**Adopt unit 1.11.18's `Shut` / `StuckOpen` / `Truncated` measurement as the
ruler**, never summed. No task here polishes it.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor
Tim's rulings of 2026-08-25/26.** **`CLAUDE_CODE.md` is at version 1.4.**

## Rulings in force

**Tim's ruling, 2026-08-26, by adopting this unit (flagged for veto in the
delivery), in the words unit 1.11.19 asked for it:**

> **Per-bin admission cannot be made to work from the survey's gate output,
> because that output carries no Morse structure on any recording in this
> corpus — including the four that decode adjudicated callsigns. The next unit
> is not another statistic. It is whether the survey should be reading the
> probabilistic path's envelope — the measurement that already recovers
> `VA3VRR` and `N4L` — instead of a gate of its own.**

**This changes the instrument the whole project measures with, so HM-DEC-119
governs**: task 3 re-measures every anchor, every floor and both silence
controls, and the change is judged on those rather than on the four captures
that motivated it.

**HM-DEC-095's principle is untouched** — a note is chosen by how it is keyed,
never by how loud it is. This unit changes *what is measured* to decide how a
bin is keyed, not the principle.

**HM-DEC-120 is the acceptance test in its stricter form:** both silence
controls emit nothing **and** their bins are `Shut` rather than `StuckOpen`.

**Rejected already, do not revisit:** a sixth statistic on the survey's existing
gate output — the null control shows the stream is the problem, not the measure;
`MinimumSeparation`'s bound; the ratio band; the admission valve; the threshold
above the band floor; the two-levels-apart spread; the speed-scaled de-glitch
(measured — it would loosen, not tighten); the integrator width; the
confirmation window; the four dead squelch axes; locking to `CwPitch`.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — the same capture, the same pitch, two streams

**This task decides the unit.** For each capture below, at the pitch named,
produce **two** run streams and score both with unit 1.11.19's quantisation
statistic, which stays exactly as it is so the numbers are comparable:

- **A** — the survey's gate output, as it is today;
- **B** — runs taken from **the probabilistic path's envelope**, mixed down at
  that same pitch and integrated through the same Hann window the decoder uses.

Measure on all ten of unit 1.11.19's captures, at the same pitches, so the two
tables sit side by side: the four stations (`012823` at 500, `014113` at 600,
`014308` at 625, `125941` at 400), both silence controls, and the anchored
`013347`, `134712`, `012403` and `004507`.

**Report the null values again beside them** — generated Morse at 0.000 and
0.182, random at 0.23 — so B is judged against the same scale.

**Then answer in one sentence: does stream B score like Morse where stream A
scores worse than random, and does it separate the stations from the two
silence controls?**

- **If B is quantised and separates**, tasks 2 and 3 are built.
- **If B scores like A**, stop. Build nothing, and report that the envelope the
  decoder reads is *also* unquantised at these pitches — which would mean the
  characters Hamlet recovers come from somewhere the run-level view cannot see,
  and the next question is a design one about the decoder itself, not the
  survey. **That outcome is this unit's result and is reported as such.**

Build and run; record the baseline by diffing which tests fail.

### Task 2 — feed the survey the envelope *(only if task 1 separates)*

Give the survey the same envelope for every bin it scans, replacing its own gate
as the source of runs. Keep every existing admission test operating on the new
stream; **change what they read, not what they are.**

**Cost is part of the task, not an afterthought.** The band is scanned in 25 Hz
steps and the envelope is per-pitch, so this is a filterbank where there was a
threshold. Report: the work per survey pass before and after, whether the survey
still completes inside its own cadence, and what was done if it does not. **A
correct survey that cannot run in time is not a solution** — if it will not fit,
report the measured cost and stop rather than thinning the band silently.

### Task 3 — the corpus, because the instrument moved *(only if task 2 ships)*

Re-run everything and report against unit 1.11.19's figures:

- **all four stations admitted at their own pitches**, pitch reported as
  measured;
- **both silence controls: nothing admitted, nothing emitted, bins `Shut` not
  `StuckOpen`**;
- **all twelve adjudicated anchors green, character for character**;
- every floor held; chunk invariance intact;
- the decode of all four target captures end to end, against their floors of
  41, 0, 0 and 0.

**A capture now admitted that still reads nothing is a finding, not a failure**
— it means the fault has moved downstream for the first time in this phase.
Say so for each.

### Task 4 — what the decoder sees that the survey does not *(the drop candidate)*

Whatever task 1 concluded, one measurement is worth keeping: on `134712`, which
reads `N4L`, report the envelope's own shape across the characters that decode
correctly — the run lengths, the fitted unit, and how they differ from the
survey's runs over the same seconds. **Measure only.** Dropped whole if time
runs out, and the report says so.

## Parked — do not touch, do not raise

The `Shut`/`StuckOpen`/`Truncated` metric itself; the six intermittents; the
hop's precision problem; confirmation; displacement; the hold; fist-quality
selection; the meter; the squelch's successor; the integrator width; the
whole-file second pass; `001520`'s quadrillions; the reference and port
integrator difference; the short-character bias; the Avalonia offset;
`CHANGELOG.md`; HM-OPEN-057; HM-OPEN-059; **the panel, entirely.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not build tasks 2 or 3 if task 1 does not separate.** Report and stop.
- **Do not change any admission test's logic.** This unit changes their input.
- **Do not thin the band or lengthen the survey's cadence to make the cost
  fit** without reporting it as the trade it is.
- **Do not trade the silence property**, in either form.
- **Do not fit anything to the four target captures.** The anchors and the
  silence controls are the judge.
- **Do not chase an intermittent.**
- **Floors only rise; anchors stay green; chunk invariance holds; no panel
  change.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with task 1's two tables side by side — stream A and stream B,
same captures, same pitches, with the null values beside them.** Section 2 says
plainly whether a station the operator can hear now reaches the decoder.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Sixteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26, including the one this unit acts under.**
5. **The tone tracker** — five axis families measured; this unit tests whether
   the input itself is wrong.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures; the
   operator's noise session crossed it live on 2026-08-26.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's item five,
   the last of his list not yet attempted.
10. **The keying meter** — its measurement found a station its verdict denied,
    and it reads 44 words a minute off silence.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings** (1.11.18).
13. **The survey's gate output carries no Morse structure on any capture**
    (1.11.19) — **this unit acts on it.**
14. **The hop's ±32% cannot explain a worse-than-random residual** (1.11.19).
15. **The survey's cost if it must integrate per bin** — task 2 measures it.
16. **Whether the probabilistic envelope is itself quantised** — task 1 answers
    it, and a negative answer is a design question about the decoder.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference and
port integrator difference**; **an unmeasured pitch costs `N4L`**; **the
six-hertz window disagreement**; **the short-character bias**; **the Avalonia
geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.19**; **the whole-file
second pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**; **six timing intermittents**.

**If you finish every task, stop and report. Do not start the next unit.**
