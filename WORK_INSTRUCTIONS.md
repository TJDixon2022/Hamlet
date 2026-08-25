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

# Work instruction 013 — bank the evening at last, then read it again at the note it was sent on

**Four tasks; task 4 is the drop.**

## Why this unit exists

**The unit's number: 4 becomes 22.**

Unit 1.11.9 measured, by accident, the largest single lever in the tree: started
at each station's own note instead of the operator's 600 Hz, `032113` reads 22
characters of its adjudicated line instead of 4, `032012` 43 instead of 22,
`032050` 24 instead of 17. Hamlet cannot know the note in advance — **but it
knows it a few seconds in, once the tracker settles. Nothing re-reads the audio
it already holds at the pitch it now knows.** The decoder lives forever with
whatever its first seconds were mixed at, and its first seconds are mixed at a
guess.

The corpus stands at **153 of 384 adjudicated characters, 40 %**, against a
target of eighty. The re-read attacks exactly the deficit the measurement
found.

**And the thirteen captures of 2026-08-25 are in this zip** — absent from four
consecutive units, delivered at last: the 59-characters-1-unsure reference, the
capture where Hamlet first beat the independent chain, and the negative control
that tells a real improvement from a trade. Task 1 banks them before anything
else is touched, along with `ANALYSIS-2026-08-25-session.md`, which unit 1.11.8
implemented from quotation because the file itself was never in the tree.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway.

**Known state after unit 1.11.9: 30 failing of 1674 in the engine** (three
accepted-cost silence fixtures, two timing-flaky rig tests among them), **483 of
483 in the app. Twelve success tests guard the adjudicated readings and every
one is green. `ARecordingWithNoStationInItSaysNothing(014854)` is green and
must stay green.**

**A new invariant stands from unit 1.11.9's task 4 and must survive this unit:
every capture reads identically at 240, 480, 960, 1920 and 4800 samples a
chunk, and identically through `Listen` and `Process`, in text and tracked
pitch.** The re-read must preserve chunk-size invariance — a re-read that fires
at different moments for different chunk sizes reintroduces the two-decoders
fault that unit just closed.

**`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150, nor for
Tim's three rulings of 2026-08-25** — the W1AW adjudication among them, on which
the twelve success tests rest.

**`CLAUDE_CODE.md` says four report sections; its version line reads 1.3.**
Read the file's own section count.

## Rulings in force

**Tim's adjudication of the seven W1AW captures stands** as written in the
truth file, header ADJUDICATED. Only quoted words are adjudicated.

**HM-DEC-120.** Nothing is emitted on audio holding no signal, and the meter
does not claim Keying on it — **zero empty windows, absolute**, held by unit
1.11.9 and re-verified here at every task touching the signal path. `Gate`
stays 1.40; `CharacterMargin` stays 1; the meter's element-median-plus-swing
verdict stays as shipped.

**Rejected already, do not revisit:** the clock's span-restricted diet and the
fist-quality band (built, measured, reverted — 1.11.8); the validity term at
any weight (built, measured flat-until-harmful — 1.11.9; its untried half is
task 4, not a revisit); locking to `CwPitch`; widening the guard; tuning any
threshold to green a test; treating duty as a station test.

**PROPOSAL, not ruled:** everything on the panel. **Untouched — including any
indication that a re-read occurred.** If the re-read wants to say so on screen,
that is a display ask for section 4, not a change.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what
is moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — bank the thirteen, and the two measurements waiting on them

1. Commit the thirteen captures with sidecars from
   `tests/fixtures/cw/captured/unadjudicated/`, and
   `ANALYSIS-2026-08-25-session.md` to the repository root beside its siblings;
   the manifest and cases file beside the captures.
2. Floors for all thirteen. **`013520`** (59 characters, 1 unsure, 157
   elements) is the reference; **`013303`** the beat-the-chain case;
   **`012823`** the negative control — **the harness must say so if a change
   improves `013520` while regressing `012823`.**
3. **Re-run the two reverted fixes' target measurements, now that their
   evidence exists**: does the shipped decoder still hypothesise 32 WPM on the
   22.5 WPM `012823` and 10 on the 17.9 `021825`? Does the shipped pitch
   chooser still miss `012823` by 50 Hz? **Measure only, change nothing** —
   these numbers are the starting line for any future attempt, recorded so the
   next session does not begin from quotation.

Build and run; record the green baseline.

### Task 2 — re-read on settle

When the tracker settles on a **measured** pitch that differs from the pitch
the window's early audio was mixed at by more than a threshold the task
derives, the decoder re-mixes and re-decodes the audio it still holds at the
settled pitch **before those characters settle and emit.**

Design constraints, each with its reason:

- **Only backward, only within the buffer the stream already holds.** No new
  audio retention beyond what exists; the feature re-reads what it has, it does
  not grow memory unboundedly.
- **Only on a measured pitch** (`HasMeasuredPitch`). A re-read at a bank centre
  is decoding at a number nobody keyed at, which is the fault unit 1.11.6
  removed.
- **Chunk-size invariant.** The settle decision and the re-read boundary are
  functions of hops, not of arriving chunk shape. The five-chunk-size identity
  is asserted after, on all thirty-six captures.
- **Settled text never changes.** Characters already emitted are never
  retracted — §0.0: the display does not un-say things. The re-read exists to
  make the *first* emission right, which is why it must complete before the
  settle delay expires. If the delay is too short for a re-read on some
  captures, report which and by how much rather than silently extending the
  delay — the delay is a ruled-adjacent constant.
- **Empty captures never re-read** — no pitch settles on them; assert zero
  re-reads and unchanged silence on `014854` and `014935`.

**Acceptance, from the measurement that commissioned this:** adjudicated
characters rise from 153 of 384, with `032113`, `032012` and `032050`
individually improved (their whole-file-at-station-note readings were 22, 43
and 24 — the re-read need not reach them but must move toward them, reported
per capture); `031905` and `032129` hold their floors; every success test
green; every floor intact; the 2026-08-25 thirteen at or above their new
floors; both flaky rig tests excluded from judgement either way.

### Task 3 — the six recordings the meter still calls empty

All six now fail on the **keying score**, not the element median. Diagnose what
the score measures on each and why a recording holding a station scores below
the bar, with file and line. **Fix only if the fix is contained and costs zero
empty Keying windows — absolute, live path, window by window.** Otherwise
report the mechanism and the overlap, which is the answer.

### Task 4 — the cutter's untried half *(the drop candidate)*

Unit 1.11.9 named it: validity scored **against the fitted clock as a second
term**, not the length penalty alone — and measured against `N4L`, whose
failure is a cut inside a character. Build it behind the same discipline: swept
weight, success tests as the judge, **ship nothing if the largest safe weight
buys nothing**, and say so with the table.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

- **The panel** — including re-read indication (section 4 ask if wanted) and
  the "Hold this pitch" button.
- **The starting-pitch question as a *setting*** — ruled out by its own
  mechanism; the re-read is the actionable form.
- **`competing`**, the 125 Hz ask, the integrator width,
  `014113`/`014308`'s smear, `001520`'s quadrillions, the reference/port
  integrator difference, the six-hertz window disagreement, the
  unmeasured-pitch-costs-`N4L` ruling, the short-character bias, the Avalonia
  hit-test geometry offset, `CHANGELOG.md`, HM-OPEN-057, HM-OPEN-059.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not retract settled text.** The re-read improves first emission; it
  never edits history.
- **Do not break chunk-size invariance.** Asserted, all captures, all five
  sizes.
- **Do not let any success test or floor go red**, and do not lower a floor.
- **Do not trade the silence property in any form** — emission, meter, or
  re-read.
- **Do not touch the panel.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with the adjudicated-character count, before and after the
re-read — 153 of 384 is the before — and the per-capture movement on `032113`,
`032012`, `032050`.** Section 2 says plainly what Tim will see at the radio:
the first seconds of a station no longer wearing the wrong pitch for the rest
of the contact.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes
   `PHASE` match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor
   for Tim's three rulings of 2026-08-25, one of which twelve tests rest on.**
5. **The tone tracker** — narrowed by the hold, not closed.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best
   cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter's remaining six** — task 3 acts on it.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Still open: **the lock's mixed help**; **the "Hold this pitch" button**;
**three fixtures at accepted cost**; **`001520`'s quadrillions**; **the
reference/port integrator difference**; **`CLAUDE_CODE.md`'s version line**;
**an unmeasured pitch costs `N4L`**; **`014113`/`014308`'s second mechanism**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset**; **the joint cutter's flat-until-harmful validity
term** (task 4 tries its untried half); **a second rig test flakes**.

Closed by delivery: **the thirteen captures of 2026-08-25 are in this zip** —
task 1 closes the ask that led four units.

**If you finish every task, stop and report. Do not start the next unit.**
