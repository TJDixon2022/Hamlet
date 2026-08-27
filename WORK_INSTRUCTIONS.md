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

# Work instruction 024 — the operator's ear, and the unit a station keeps

**ISSUED: 2026-08-26. A fresh order, not an amendment.**

**Four tasks; task 4 is the drop. This is the last build before the operator
goes to the night band, and task 1 is written to ship something he can use
whatever the rest of the unit measures.**

The standing goal governs every part: **he hears CW, Hamlet must decode it,
eighty percent of the time.**

## Why this unit exists

**The unit's number: 0.173 against 0.409, on the same gate and the same
recording.**

Unit 1.11.20 measured the gate's own elements over the 1.56 seconds that spell
`N4L` — HM-DEC-144's hand-read run lengths — at a quantisation residual of
**0.173**, below the random null of 0.231 and level with Morse at 30% jitter.
The same gate pooled over the same thirty-second recording scores **0.409**.

**The structure was always there. Twenty-eight seconds of nobody sending
averaged it away** — and unit 1.11.19's conclusion that the stream carries no
Morse structure is withdrawn on that evidence.

**This project has already made the corresponding ruling once.** HM-DEC-090
found the reported SNR and the located pitch were averages over the silence in a
recording and replaced both with held peaks. **The admission tests were never
given the same treatment.**

**But scoring per pass is not by itself the answer, and that is measured too.**
Every capture then produces passes below the random null — including the two
that hold nothing. `cw-2026-08-24-012403`, which reads `DE KD0UN KD0UN K`, has
**three** passes under 0.20; `cw-2026-08-20-014854`, which holds nothing, has
**four**. Nineteen runs a pass is not enough evidence for a fit to carry a
decision alone.

**The untested second condition is agreement.** A station keying at one speed
produces good passes that fit **the same unit** — `N4L`'s elements fit 30.5 ms.
Noise producing a lucky pass fits whatever unit happens to suit that pass, and
the next lucky pass fits a different one. **Agreement across passes is
dimensionless and needs no new statistic**, only the fitted units the last two
units already compute.

**And because five axis families have now failed, task 1 does not depend on the
sixth.** The one detector in this system that has never been wrong is the
operator's ear. Task 1 lets it decide.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway. Each of the last three units
disproved its own order's premise and was right to.

**Expected state: 28 failing of 1841 in the engine as the stable set; 503 of
503 in the app.** **Seven timing intermittents now exist and three different
ones fired in the last four runs. Do not chase any of them.** Diff which tests
moved; never trust a total.

**Adopt unit 1.11.18's `Shut` / `StuckOpen` / `Truncated` measurement as the
ruler**, never summed. No task polishes it.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor
Tim's rulings of 2026-08-25/26.** **`CLAUDE_CODE.md` is at version 1.4.**

## Rulings in force

**Tim's ruling, 2026-08-26, by adopting this unit (flagged for veto in the
delivery) — the operator may assert a station:**

> **When the operator presses "I hear a station", Hamlet takes that as evidence
> that a station is present and decodes at the strongest keyed bin in the band
> at that moment, holding it until he clears it or the radio's frequency
> changes.** HM-DEC-095 forbids Hamlet *choosing* a note by loudness because
> loudness is not evidence of keying. **It does not forbid the operator
> supplying the evidence of keying himself and Hamlet supplying the frequency.**
> The sidecar records that the pitch was operator-asserted rather than measured,
> so no capture ever implies Hamlet found what a human found. **The button
> already exists and already banks the last half minute; this adds decoding at
> the asserted pitch to what it does.**

**Tim's ruling, same date, same mechanism — admission may hold and agree:**

> **An admission statistic averaged over a whole recording is a statistic about
> the silence, not about the station.** Admission may hold the best fit a bin
> produces over a decaying window, as HM-DEC-090 already does for the SNR and
> the pitch, **provided a second condition guards the held peak** — because a
> held maximum over noise rises to whatever the luckiest pass produced.
> **Agreement between the units those passes fit is the condition to measure
> first.**

**HM-DEC-120 is the acceptance test in its stricter form:** both silence
controls emit nothing **and** their bins are `Shut` rather than `StuckOpen` —
**and task 1's assertion path is exempt from nothing**: an operator pressing the
button on an empty band gets whatever the audio contains, which is his own
choice, but **the automatic path must not change**.

**Rejected already, do not revisit:** the envelope as the survey's input
(measured on ten captures at matched hop — improves one, the cleanest); per-pass
scoring without a second condition (measured — noise wins); a sixth statistic on
the pooled stream; `MinimumSeparation`; the ratio band; the admission valve; the
threshold above the band floor; the two-levels-apart spread; the speed-scaled
de-glitch; the integrator width; the confirmation window; the four dead squelch
axes; locking to `CwPitch`.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs. **This unit is against a
clock — the operator goes to the radio when it finishes.**

## The tasks

### Task 1 — the operator asserts a station *(ships regardless of everything below)*

Implement the first ruling. **Do this task first and commit it before starting
task 2**, so that if the session runs long the operator still gains something
tonight.

- Pressing **"I hear a station"** sets the decode pitch to the strongest keyed
  bin at that moment and holds it, bypassing admission entirely.
- **It is released** by the operator clearing it, or by the radio's frequency
  changing — the release-on-QSY rule already in the tree.
- **The sidecar says the pitch was asserted, not measured.** `HasMeasuredPitch`
  stays false; a new state says who chose it. No capture may ever imply Hamlet
  found what a human found.
- **The panel changes only where the ruling says**: the button's existing
  behaviour is kept and extended. **Nothing else on the panel moves.**

**Acceptance:** on `cw-2026-08-26-125941` — the operator's live miss, a station
at 403.5 Hz that Hamlet reads nothing from — asserting at the strongest bin
produces a measured decode; report the characters, the pitch chosen, and the
speed. Same for `cw-2026-08-22-014113` and `-014308` at 607 and 606, and
`cw-2026-08-25-012823` at 500. **The automatic path is untouched: all twelve
anchors green, both silence controls silent, every floor held.**

### Task 2 — does the unit a bin fits stay put?

**Measure before building.** For every capture in unit 1.11.20's table, and for
both silence controls, take the passes scoring under 0.20 and report:

- **the fitted unit of each such pass**, in milliseconds;
- **their spread** — the coefficient of variation across those passes,
  dimensionless;
- the same for the station's own bin and for a sample of its neighbours.

**Then answer in one sentence: do a station's good passes agree on a unit where
noise's good passes do not, and by how much?**

- **If they agree**, task 3 is built.
- **If they do not, stop, build nothing, and report it.** Six axis families
  would then have failed and the ask returns to Tim as a design question, with
  task 1 having shipped regardless. **That is an honest result and is reported
  as one.**

### Task 3 — held peak plus agreement *(only if task 2 separates)*

Admission holds the best fit a bin produces over a decaying window, guarded by
the agreement condition task 2 measured. Existing tests keep operating; this is
added, not substituted.

**Acceptance:** all four stations admitted at their own pitches; **both silence
controls admit nothing, emit nothing, bins `Shut`**; all twelve anchors green
character for character; every floor held; chunk invariance intact. **If no
setting meets all of it, ship nothing and report the sweep**, naming which line
each setting breaks.

### Task 4 — the decoder's own dit spread *(the drop candidate)*

Unit 1.11.20 measured the decoder's segmentation on `134712` implying dits from
25.0 to 61.7 ms — a spread of 2.47× across characters that decode, over an
envelope swinging 26.8 dB. **Measure only**: the same spread on the other
anchored captures, and whether it correlates with what reads correctly. It bears
on whether the decoder's clock is following a station or being dragged.
**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

The `Shut`/`StuckOpen`/`Truncated` metric; the seven intermittents; the hop's
precision; confirmation; displacement; the hold's other behaviour; fist-quality
selection; the meter; the squelch's successor; the integrator width; the
whole-file second pass; `001520`'s quadrillions; the reference and port
integrator difference; the short-character bias; the Avalonia offset;
`CHANGELOG.md`; HM-OPEN-057; HM-OPEN-059; **the panel beyond task 1's ruling.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not let task 1 slip behind task 2.** It ships first and commits first.
- **Do not let an asserted pitch be reported as a measured one**, anywhere.
- **Do not change the automatic admission path in task 1.**
- **Do not build task 3 if task 2 does not separate.**
- **Do not trade the silence property** on the automatic path, in either form.
- **Do not chase an intermittent.**
- **Floors only rise; anchors stay green; chunk invariance holds.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 2 leads with what the operator can do tonight that he could not do
this morning**, because he goes to the radio on the strength of it. **Section 3
leads with task 1's decodes on the four captures he can hear, and then task 2's
sentence.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Seventeen inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26, including the two this unit acts under.**
5. **The tone tracker** — pooling versus a held peak, task 2 and 3.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's item five,
   the last of his list not yet attempted.
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings** (1.11.18).
13. **Nineteen runs a pass is not enough evidence for a fit** (1.11.20) — task 2
    tests the second condition.
14. **Pooling versus a held peak** (1.11.20) — acted on here.
15. **A seventh intermittent, and three different ones in four runs** — a full
    total is unreadable; worth its own small unit.
16. **The decoder's own dit spread is 2.47× on a capture that reads** — task 4.
17. **Whether an asserted pitch should also feed the tracker's history** — not
    in this unit; the assertion bypasses admission and does not teach it.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference and
port integrator difference**; **an unmeasured pitch costs `N4L`**; **the
six-hertz window disagreement**; **the short-character bias**; **the Avalonia
geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.20**; **the whole-file
second pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
