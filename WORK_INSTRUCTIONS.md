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

# Work instruction 035 — key-up is not the noise floor

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Six tasks; task 6 is the drop.**

## Why this unit exists

**The unit's number: 25.7 decibels, and nothing on the screen.**

`ANALYSIS-cw-key-up-is-not-noise-2026-08-27.md` ships in this zip and is
committed by task 1. It was measured outside Hamlet, from the WAV files. Its
three findings:

**The stations are not weak.** Narrowband SNR at their own pitch is **16.6 dB**
on `cw-2026-08-22-014113` and **25.7 dB** on `cw-2026-08-22-014308`. **Unit
1.11.31's conclusion that they are below the decoder's sensitivity is wrong.**

**They are Morse.** Envelope autocorrelation peaks at 110 and 118 ms against
114 ms for `cw-2026-08-24-012403`, which decodes a callsign, with stable phase
in all three.

**And one number separates them from the capture that reads:**

| capture | key-down | key-up | separation | key-up above the band floor |
|---|---|---|---|---|
| `012403` reads | −22.6 | −36.6 | **14.1 dB** | **31.8 dB** |
| `014113` unread | −25.4 | −37.3 | **11.9 dB** | **18.5 dB** |
| `014308` unread | −28.1 | −39.2 | **11.1 dB** | **31.3 dB** |

**On all three — including the one that reads — the key-up state sits 18 to 32
decibels above the band noise floor.**

**`CwProbabilisticDecoder.LogLikelihoods` scores key-up as noise**, with the
scale taken from the envelope's own lower quartile. **It is being asked to
explain a key-up state that is not noise.** Where the states separate by 14 dB
it carries anyway; at 11–12 dB it does not, and the window ratio lands at 0.84
and 0.44 against a floor of 1.40.

**Neither published implementation assumes what Hamlet assumes.** RSCW sets its
threshold so the average distance to the samples above equals the average
distance to the samples below — both states fitted from the data.
`cwdecoder.py`, **in this repository's own root**, fits two means to the dB
envelope per window and thresholds between them, and it reads these captures.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway. **Four
consecutive units disproved part of their own order's premise and each was right
to.** Task 2 exists so this one can be disproved before anything is built on it.

**The figures above came from outside Hamlet.** Where Hamlet's own instruments
disagree, **Hamlet's numbers are the truth about Hamlet** and this order's
premise is what gets re-examined.

**Expected state: 28 failing of 1854 in the engine — measured, byte-identical
by name — plus two tests added by unit 1.11.31, so 1856 is the expectation and
is labelled one.** The app is 509 of 509. Seven timing intermittents exist; do
not chase them, diff which tests moved.

**A run at `-v n` prints `Total tests: / Passed: / Failed:` and never prints
`Passed!`** — unit 1.11.31 found that its own grep for the latter had been
killing runs it then reported as killed by the environment.

**`CwPitchRanking` is deleted by Tim's ruling below.**

**`CLAUDE_CODE.md` is at version 1.6.** **`DECISIONS.md` has no record for
HM-DEC-096–133, 136, 141, 150** — HM-DEC-090, 095, 120, 125 and 127 are all
inside it.

## Rulings in force

**Tim's ruling, 2026-08-27 — `CwPitchRanking` is deleted.** No caller, no
prospect of one, and a measured record of choosing wrongly on all four captures.
**Its lesson goes to `DECISIONS.md` as an ask, not into a class nobody calls.**

**Tim's direction, same date:** *"This is math. This is transforms. This is
filters. You're built for this."* **This unit is the observation model, not
another threshold.**

**HM-DEC-120's property is not traded.** Nothing is emitted on audio holding no
signal. Both recordings holding nothing emit nought today; **any change here
that breaks that is this unit's doing and is reverted rather than explained.**

**HM-DEC-090's lesson applies and is the shape of this fix:** the reported SNR
and the located pitch were averages over the silence in a recording and both
became held peaks. **The key-up hypothesis was never given the same treatment** —
it is still assumed rather than measured.

**Rejected already, do not revisit:** moving the emission floor's value; pooling
the ratio over keyed hops only (measured in 1.11.31 — it admits an empty
recording at 1.62 while still refusing a real station at 0.73); any further
acquisition work; a channel hold for a leak the tree does not reproduce.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — bank the analysis and the two captures, delete the ranking

Commit `ANALYSIS-cw-key-up-is-not-noise-2026-08-27.md` to the repository root
and the two captures with their sidecars to
`tests/fixtures/cw/captured/unadjudicated/` if they are not already there.

**Delete `CwPitchRanking` and its tests**, per Tim's ruling. **Record its lesson
in section 4 as an ask for `DECISIONS.md`:** an offline sweep over a bank is not
what the application runs, and a figure from one was carried into two orders as
though it described the app.

Build and run the suite to completion; **report the total and the failing set by
name.**

### Task 2 — reproduce the three findings in the tree

**Before building anything.** For `014113` at 606 Hz, `014308` at 606 Hz and
`012403` at 439.8 Hz as the control, measure and report:

- the narrowband SNR at the station's pitch;
- the envelope autocorrelation's first peak;
- **the two states fitted to the dB envelope, and where key-up sits relative to
  the band noise floor measured at the same bandwidth well away from the
  station.**

**Then answer in one sentence: does the key-up state sit well above the band
noise floor on captures that read as well as on captures that do not?**

- **If it does**, the premise holds and tasks 3 to 5 proceed.
- **If it does not**, stop. **Report what Hamlet measures instead** — that is
  the finding, and building on a premise the tree contradicts is what cost units
  1.11.30 and 1.11.31 seven tasks between them.

### Task 3 — fit key-up instead of assuming it

In `CwProbabilisticDecoder.LogLikelihoods`, **the key-up hypothesis is fitted
from the observed inter-mark level rather than assumed to be the noise floor.**

The shape is the published one: **two states fitted to the envelope**, key-down
and key-up, each with its own location, rather than a key-down at the observed
amplitude and a key-up pinned to noise. `cwdecoder.py` in the repository root is
the working reference and is in the same language family as the port under
`tools/reference-decoder/`.

**The scale is taken from the data too.** Where the fitted key-up level and the
noise floor disagree, **the fitted level wins** — that is the whole change.

**Constraints, each with its reason:**

- **The estimate is local in time**, on the rolling span already used for the
  noise scale. A key-up level averaged over a whole recording is HM-DEC-090's
  own fault arriving again.
- **On audio holding no station the two fitted states collapse together**, and
  that must produce a *lower* likelihood ratio than a keyed signal, not a higher
  one. **State how the model behaves when the two states are indistinguishable**
  — that is the case that protects the silence property.
- **The floor of 1.40 is not moved. If the quantity it measures changes scale,
  re-derive it and report the derivation** — a threshold whose scale moved
  underneath it reads as a working gate while gating nothing, which this project
  has done once already.

### Task 4 — measure it, and be willing to lose

Re-run the corpus and report:

- **`014113`, `014308` and `125941`: the window ratio at the station's pitch,
  before and after, and what they emit.** Their floors are nought.
- **Both recordings holding nothing: nought characters.** Absolute, per capture,
  stated. **This is the first acceptance line, not the last.**
- **All twelve adjudicated anchors, character for character.**
- Every floor held; chunk invariance intact; the sensitivity sweep.

**If the change reads the unread captures and costs an anchor, it has failed and
is reverted.** If it reads them and costs nothing, **that is the first movement
on those captures in the project's history and the report says so plainly.**

### Task 5 — what the operator will see

**Through the production path, not offline**, report what each of the following
now shows: `014113`, `014308`, `125941`, `012823`, and both recordings holding
nothing.

**Section 2 is written from this task.** If a station he can hear now reaches
the screen, say which and what it spells. If not, say that too — he is going to
the radio on the strength of it.

### Task 6 — the reference decoders, side by side *(the drop candidate)*

`cwdecoder.py` reads these captures and Hamlet does not.

**Run it on `014113` and `014308` and report what it produces**, beside what
Hamlet produces after task 3. **Measure only.** If the gap is still wide, that
names what the next unit inherits; if it has closed, that is the strongest
evidence this unit could offer.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

Acquisition in every form — the survey, the tracker, the strongest bin, the
acquisition floor; the channel hold; the joint cutter and its word gaps; the
constrained margin; the meter's rebuild; the integrator width; the whole-file
second pass; the short-character bias; `001520`'s quadrillions and `013347`'s
17.2 million; the reference and port integrator difference. Also: **the entire
screen**; `CHANGELOG.md`; the seven intermittents; HM-OPEN-057; HM-OPEN-059.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not build task 3 if task 2 contradicts the premise.** Report and stop.
- **Do not move the floor's value.** Re-derive only if its scale changes, and
  report the derivation.
- **Do not trade the silence property.** Both empty recordings emit nought today
  and must at every task.
- **Do not touch acquisition.** Three units went there and the signal was never
  getting through the observation model.
- **Do not fit anything to the two captures.** The anchors and the empty
  recordings are the judge; the two are the motivation.

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason. **The report is the only exit.**

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with task 2's sentence, and then the window ratios on
`014113`, `014308` and `125941` before and after against the floor of 1.40.**
**Section 2 says plainly whether a station he can hear now reaches the screen.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-five inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** —
   HM-DEC-090, 095, 120, 125 and 127 are all inside it.
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
6. **A boxcar's nulls made two of five swept offsets pathological best cases.**
7. **Two stations closer than 125 Hz are not named** — the operator's item five.
8. **The keying meter** — an adjudicated anchor holding a ruled-on callsign reads
   `no keying, 6 ms key down`, identical to a recording holding nothing.
   **Confirmed on two captures and it is what blocks the empty corpus.**
9. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
10. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22).
11. **The constrained margin is bounded and still does not separate** (1.11.22).
12. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
13. **HM-DEC-086's supersession needs a record** (1.11.25).
14. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
15. **The recent-places row has no home** (1.11.26).
16. **The owned-property list has no enforcement of staying current** (1.11.27).
17. **A test resolved an ambiguous control by accident** (1.11.27).
18. **Nothing checks that deleting a surface is not deleting a capability**
    (1.11.28).
19. **The scanner and the calling cycle are attached to the rig with no
    control** (1.11.28), and one of them transmits.
20. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones** (1.11.28).
21. **Whether every constructed view model should be reachable from a binding**
    (1.11.28).
22. **No capture reproduces the junk the operator is watching** (1.11.30) — the
    sheet's `unkeyed` line will carry it when it next happens.
23. **The empty corpus is two recordings and cannot be built from recorded
    evidence** (1.11.31) — one candidate, `cw-2026-08-25-012748`.
24. **An offline sweep over a bank is not what the application runs** — task 1
    asks for it in the decision log.
25. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **an
unmeasured pitch costs `N4L`**; **the six-hertz window disagreement**;
**`CHANGELOG.md` at 1.9.0 against 1.11.31**; **the three morning captures of
2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
