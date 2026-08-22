# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticStream.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

These four files are fixed. Do not substitute a different file for any of
them and do not report a check against a file this list does not name.

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**`cw-2026-08-22-014113.wav` carries a strong, steady, high-duty station and
Hamlet read nothing from it. Every excuse outside the decoder is gone.**

`ANALYSIS-cw-2026-08-22-014113.md` measured the file independently of Hamlet's
decoder, with a separate Goertzel chain, and states its method so it can be
disagreed with. **Read that document before starting. It is the specification for
this unit.** Its findings:

- A keyed signal at **608 Hz from 15.7 s to 27.4 s**, continuously present,
  standing **30 to 53 dB** above the band median and never once out of first place.
  **18.9 dB of narrowband swing, roughly 45 % duty across the active span.**
- **The tone does not drift** — fifty consecutive windows put it between 596 and
  612 Hz.
- Marks cluster at **70–80 ms and ~200 ms**, a ratio near three. Corrected for
  threshold edge bias the unit is **≈ 62 ms, about 19 WPM**, with dah, character
  gap and word gap all within a few milliseconds of 3, 3 and 7 units.
- **Peak −12.9 dBFS, no clipping, no DC offset, the 500 Hz filter behaving.**
  The antenna, the radio, the filter, the USB device, the gain chain and the
  tuning are all exonerated **by the app's own capture**.

**This is not HM-OPEN-012.** That is about a low-duty station whose tracked peak
decays between bursts. This is 45 % duty across twelve unbroken seconds.

### The lead, and it is a mechanism rather than a symptom

The sidecar reports **20 elements seen, 20 resolved, 13 characters emitted**. That
is **1.54 elements per character.** English Morse averages near three.

**The character-gap hypothesis is winning about twice as often as it should**,
which shatters letters into single elements and produces the E, T and I soup seen
all week.

**And the speed points the same way.** Hamlet's `reading` chose **24 WPM** against
a measured **19**. A unit a quarter short promotes ordinary inter-element gaps into
character gaps, systematically, everywhere. **The two observations are one fault
seen twice.**

### What must not be done to this file

The analysis swept threshold and unit across the plausible range and **got no
readable transcript either.** It also built a scorer ranking candidate decodes by
the fraction of valid Morse characters, reached 30/30 valid, and returned
`ETTT TOGATMETTEMTTEEEATEEEMN`. **A validity score is gameable into E/T soup and
any confidence built on one will be confidently wrong.**

There are also continuous marks of **325–430 ms** in the active span — five to
seven units at the measured speed, which no single operator sends. At least three
other signals share the passband (712.7, 437.9, 767.1 Hz). **This capture may hold
two or more stations at once.**

**So no transcript is asserted for this file, by anybody, in this unit or later.**

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- Every figure above came from outside this repository. **Reproduce what you rely
  on.**
- **The failing set is 28.** Record it exactly before and after and name every
  difference.
- **Report on the sweep AND all six existing recordings together, every time.** A
  change that looked perfect on the sensitivity fixture once silenced all six.
- **A binding that resolves and an element that is not on screen look the same to a
  test that reads the log.** Anything on screen is proved by a test that builds the
  real window.

---

## Rulings in force

- **HM-DEC-120.** Nothing emitted on audio holding no signal.
- **HM-DEC-009** and **§0.0.** Tasks 1 and 4 are both this.
- **§0.0.1**, which the sidecar currently breaches — see task 4.
- **HM-DEC-091.**
- **HM-DEC-096** phase 3, the mid-character interlock. **Untouched.**
- **HM-DEC-150**, the version scheme. Task 6.
- **HM-DEC-093** and `SHACK_FACTS.md` — no radio on the development machine.
- **§12.5** — no answer key is written for a recording nobody has adjudicated.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md`
**§13**, which names that file's fields — `STATE`, `PHASE`, `BALL`, `NEXT_PASTE`,
`UPDATED`, `NOTE`. `UPDATED` from the clock; `NOTE` says what is moving inside the
task. Also every ten minutes while a task runs.

---

## Task 1 — The advice line is asserting a cause the app can disprove

The panel says:

> the signal is being lost somewhere between the antenna and Hamlet, and the gain,
> the filter and the tuning are the things to try

**Hamlet's own capture contains a 19 dB signal.** The sentence sends the operator
to check things the app has the evidence to rule out.

- **Say what the app already knows at the moment that line is shown** — the input
  peak, the floor, the keying meter's swing, the tone's standing above the band.
- The line may only claim the signal is being lost upstream **when what Hamlet
  captured actually shows that.** Otherwise it says something true about not being
  able to read what is plainly there.
- **Wording is yours.** The requirement is that it never again points at the
  antenna when the WAV holds a strong station.

**Fix this first.** It is on screen now and it is wrong now.

---

## Task 2 — Why 1.54 elements per character

**Report before changing anything.**

1. **Reproduce the count** on `cw-2026-08-22-014113.wav`, and on the four existing
   station recordings. **Report elements per character for each.** Say which are
   near three and which are not.
2. **In the segmental Viterbi, what decides between an inter-element gap and a
   character gap?** Name the scoring, the durations expected, and the penalty.
3. **What speed does the decoder choose on this file, and what does it choose if
   the correct unit near 62 ms is imposed?** Report the elements per character at
   both.
4. **Say whether the gap promotion is a consequence of the speed being a quarter
   short, or a fault in the gap model independent of it.** These need different
   fixes and this is the question the unit turns on.

**If the answer is that the fault is elsewhere, say so and stop.**

---

## Task 3 — Fix what task 2 found

Gated on task 2. Build what it found, not what this order guessed.

- **If the speed is the cause:** why did 24 win over 19? The grid runs 8 to 32 in
  steps of 2, so 19 is not on it — **report whether the step is the problem** and
  what a finer grid costs in time per second of audio.
- **If the gap model is the cause:** a hand-sent fist compresses character gaps,
  and the analysis measured this sender's gaps clustering at 50, 120–180 and
  410/495 ms against a 62 ms unit. **The model must fit that, not the textbook.**
- **Nothing raises a confidence score.** A doubtful fit lowers it.

**Report elements per character on all five station recordings after the change.**
Near three is the target. **If it is not near three, say so plainly rather than
reporting a character count instead.**

---

## Task 4 — The sidecar contradicts itself

`13 emitted, 0 unsure` beside `text nothing read`, both covering the same span.

Two candidates, and **the analysis names both**: the transcript was cleared while
the counter was not — `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
is a known red that would produce exactly this — or thirteen unreadable blocks are
rendering as "nothing read".

**Find out which, say so, and make the sheet stop asserting two incompatible
things about one span.** A wrong decode with a self-contradicting sidecar is not a
regression test, and the roster is the only instrument this project has.

---

## Task 5 — The keying sweep picked the weaker bin

The sweep reported `no keying at 625 Hz`. Re-measured over the same window:
600 Hz gives 21.2 dB, **608 gives 21.3**, and **625 gives 17.2** — four decibels
low, seventeen hertz off a signal at 608. **600 Hz was on the grid and 625 won
anyway.**

- **Report what the sweep ranks bins by.** The analysis says it does not know, and
  calls this a question rather than a defect claim.
- The reported `5 ms key down, 142 key-downs` is 0.71 s of key-down in six seconds
  — **12 % duty in 5 ms fragments, against a measured 43 % in 70–200 ms elements.**
  That number describes noise crossing a threshold. **The verdict was honest about
  what it measured; the problem is what it measured.**
- **The keying meter's wording may not change.** It is the independent witness.

---

## Task 6 — The capture becomes a fixture, and asserts no transcript

`cw-2026-08-22-014113.wav` and its sidecar into `tests\fixtures\cw\captured\`.

**Assert only what was measured**, per §12.5:

- the tone is found at **608 ± 4 Hz**
- it stands at least **18 dB** out of the band across **15.7–27.4 s**
- the element structure fits a **unit near 62 ms**

**No transcript. No answer key. Not now and not later.** Nobody knows what it says,
the analysis could not read it either, and it may hold more than one station.

**Do not build a validity scorer.** One was built during the analysis, reached
30/30 valid Morse characters, and returned `ETTT TOGATMETTEMTTEEEATEEEMN`.
**Recorded here so nobody builds it twice.**

---

## Task 7 — Bump the version

Read the current version from `Directory.Build.props`, bump the patch, report what
it moved from and to. **HM-DEC-150.** One work unit, one patch.

---

## Parked — do not touch, do not raise

- **The station-change trigger**, ruled and unbuilt: a move of at least the
  decoder's bandwidth while something was being read. Its own unit.
- **Asking the decoder whether a new sender is speaking.**
- **The two-station fixture reaching the answering station through the acquiring
  branch.**
- **`FollowSpeed` has no supplier**; the reacquiring guard; the mark-and-gap
  witness behind HM-DEC-144 and HM-DEC-145; `HM-OPEN-051` recorded open while
  HM-DEC-143 closes it.
- **The twenty-two failures predating the removal.**
- The sidecar's `text` and the leading edge; the missing captures from the 20th and
  21st; mode-follow's guard; `RfGain`; the likelihood gate at 15.0; the keying
  meter's thresholds.
- **HM-OPEN-012, HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098,
  HM-OPEN-033, HM-OPEN-007.**

---

## Asks still outstanding

Carried inbound per HM-DEC-139, verbatim until ruled. **Verify against
`OPEN_ISSUES.md` and report anything here that is closed, or open and missing.**

- What a station change is — ruled, unbuilt.
- Whether the sidecar's `text` should include the leading edge.
- The captures from the evenings of the 20th and 21st are not in the tree.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0.
- The keying meter's provisional thresholds.
- `FollowSpeed` has no supplier.
- The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not assert a transcript for `014113`.** *Nobody knows what it says.*
- **Do not build a validity scorer.** *Gameable into E/T soup. Already proved.*
- **Do not tune the gap model until task 2 says which fault it is.** *Speed and gap
  promotion need different fixes and look the same from the output.*
- **Do not touch the mid-character interlock or the keying meter's wording.**
- **Do not prove a screen element with a property test.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **§12.2 names the four
headings** — **What Claude did**, **What Tim should expect**, **What we should do
next**, **What's blocking us** — the last carrying **Asks still outstanding** per
HM-DEC-139. No other headings.

**Section 1 opens with elements per character on all five station recordings,
before and after.**

**Section 2 states in one sentence whether a strong steady station now produces
characters, and what the advice line says when it does not.**

**Stop and report.**
