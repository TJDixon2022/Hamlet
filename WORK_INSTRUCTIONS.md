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

# Work instruction 055 — a station is a bin that swings

**ISSUED: 2026-08-31. A fresh order, not an amendment. Follows unit 054.**

**Seven tasks; task 7 is the drop.**

## The operator's instruction, verbatim

**"Go. Just make it work better for a change."**

**The acceptance for this whole unit is one file: `cw-2026-08-31-003229` must show
`CQ` and a callsign attempt instead of a wall of blocks.** A station called CQ —
the single thing this application exists to show its operator — and every
character of it was refused. That file is the unit's reason and its test.

## What tonight's captures established

Seven captures from 2026-08-31, 00:24–00:34 UTC, analysed outside Hamlet on code
sharing nothing with it. Four distinct situations, each now a named fixture class:

### 1. `003229` — a CQ, refused entirely

An independent decoder reads **`CQ SITKTZDZ … K`** at 583.5 Hz, with dah CV
**0.05** — the cleanest dahs in any capture the operator has ever sent. Hamlet's
sidecar for the same audio:

```
toneHz     586.1 Hz  (NOT MEASURED: the survey has admitted no keying...)
unkeyed    YES  (43 characters reached the screen ... no keying admitted here)
competing  none found, and the survey found nothing else either
text       ■■■■■■■ ■ ■ ■ ■ ■ ■ ■■■■■ ...
```

**The survey cannot see a station that a 12-line swing measurement finds
instantly.** With the squelch wired (unit 051), an admission failure now turns the
whole screen to blocks — **which is how the operator lost reads he used to have.**
Unit 053 measured that mechanism exactly: at the squelch commit, `013347` went
from 55 named characters to 9.

### 2. `002443` — the pitch picked from noise

Hamlet chose **510.2 Hz** — the loudest *average* bin — and emitted 48 `E`s from
it. The keyed carriers in that file, found by swing, are at **562.5 (25.7 dB),
703.1 (24.1 dB) and 773.4 Hz (34.4 dB)**. **On a 33%-duty signal the loudest
average bin is noise**, because the average is dominated by the two-thirds of the
time nobody is keying.

### 3. `002829` — two stations on one frequency

The "carrier" at 0.73 Hz resolution is an 11 Hz-wide plateau. Tracked per second,
the peak alternates between **601–603 and 612–617 Hz**. **Measuring each element's
own frequency over its own duration gives a cleanly bimodal histogram**, and in
time order the elements form runs — `BBBB…AAAAAAAAAA…BBBB`.

**Two senders about 13 Hz apart, taking turns.** No envelope detector can separate
them — 13 Hz needs a response slower than 77 ms, which smears a 55 ms dit — **but
every element is long enough to measure its own pitch afterward**: a 190 ms dah
resolves to 5 Hz.

**The operator's caution, and it is a ruling: two streams may be a conversation,
or two people on the same frequency who cannot hear each other. Hamlet must not
assume which.** It separates and labels; it does not narrate a relationship.

### 4. `003408` / `003419` — shredded

124 elements and **one** longer than 90 ms. Neither Hamlet nor the independent
bench reads these. **They are honest blocks and this unit does not promise them.**

### And a plain bug

`003229`'s sidecar: **`inThis -250 characters emitted, -96 unsure, -466 elements
seen, -466 resolved.`** Negative counters on the sheet the operator diagnoses
everything with.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Trust the tree over this order everywhere they differ.

**Units 053 and 054 have reported; this author has seen 053's report and not
054's.** Task 1 states what 054 landed — the peak-referenced threshold and the
hold-over, adopted or reverted — **and what the corpus baseline now is.** From
unit 052 the floor was precision 0.888; **use the tree's current number, not this
order's.**

From unit 053: the squelch commit is `95a5e06`; the clean-read lock over `KD0UN`,
`AA4MP/4` and `VA3VRR` may exist — **if it does not, build it in task 1.** The
spectral peak abandons its station at −3 dB against the old tracker's +6, and
hysteresis on the peak is costed awaiting ruling — **that ruling has not been
given; do not build it in this unit.**

**The seven captures of 2026-08-31 arrive in this zip's `fixtures/` folder if
present; otherwise the operator has them.** If they are not obtainable, **say so
once and verify against what exists** — but note plainly that the unit's
acceptance file is `003229` and without it the acceptance cannot be claimed.

**Record both suites and the corpus score before task 2.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings:**

> **Make it work better. The regression is unacceptable** — pitches that used to
> read must read again.

> **Two streams may be a conversation or may be two people who cannot hear each
> other. Do not assume.** Separate and label only.

> **Do not break the silence behaviour.** An empty band stays silent.

> **The phase goal is 85% correct CW on a capture where the pitch is right,
> precision before yield.**

> **The only measurement is against real data from the real radio.**

> **FT8, FT4 and every other digital mode are outside this conversation's scope.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode.
- **HM-DEC-120** — nothing emitted on audio holding no signal. **Tightened only —
  and note carefully: this unit makes admission see more, not require less.** A
  noise bin does not swing 20 dB; the silence fixtures must stay silent.
- **§0.4** — reproduce, then change, then measure.
- **HM-DEC-007 / HM-DEC-091** — WAV fixtures, read-only.
- **§12.5** — a floor is not lowered to fit a change.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The rule that governs every change

- **Precision must not fall below the tree's current floor.**
- **A capture that reads at 1.000 keeps reading at 1.000.** The clean-read lock
  governs; build it first if absent.
- **`TheSilencePropertyIsLockedTests` runs after every task and may not be
  modified.**
- **Every task reports precision, yield, substitutions — and separately, what
  `003229` now shows.**

## The tasks

### Task 1 — where the tree is, the fixtures, and the counters

- **State what unit 054 landed** and the current corpus baseline.
- **Build the clean-read lock if 053 did not.**
- **Bring the seven captures in as fixtures**, each labelled with its class:
  refused-CQ (`003229`, with `003212`), noise-pick (`002443`, with `002424`),
  two-senders (`002829`), shredded (`003408`, `003419`). **Read-only.**
- **Fix the negative counters.** Find why `inThis` can report −250 characters,
  with file and line, and fix it. **This is the diagnostic sheet; it must not lie
  about arithmetic.**

### Task 2 — detection by swing

**A station is a bin whose level swings between its keyed and quiet states. It is
not the loudest average bin.**

Replace the survey's statistic and the peak's selection with, per candidate bin
over the survey window:

```
peak   = high percentile of this bin's level over time   (its keyed moments)
quiet  = low percentile of this bin's level over time    (its gaps)
swing  = peak - quiet
```

- **A candidate exists where swing exceeds a threshold; candidates are ranked by
  swing, tie-broken by peak.** On `002443` this ranks 773, 562 and 703 above 510.
  On `003229` it finds 583 where the survey found nothing.
- **Sweep the swing threshold and report the curve** over every fixture including
  the silence set. **The silence fixtures bound it from below** — the threshold
  must sit above anything a station-free capture produces. **Report that margin
  explicitly.**
- **The percentiles are taken per bin over time, within the survey's existing
  window.** Nothing about window length changes in this task.
- **Admission consumes the swing-based candidate** — duty and swing measured at a
  bin that actually keys — rather than statistics about the loudest average bin.
- **The noise reference stays inside the passband.** The 2026-08-31 sidecars show
  `+34 dB` figures referenced against 1000–1400 Hz, which is the filter's
  stopband; a strength measured against the filter skirt is the receiver's shape,
  not the signal's. **If the survey's reference is outside the passband, fix it
  here and say so.**

**Acceptance:**
- **`003229` is admitted at ~583 Hz, the squelch stops firing on it, and the
  screen shows `CQ` and a callsign attempt** — blocks where it is ragged, letters
  where it is clean.
- **`002443` no longer emits from 510 Hz.**
- **Every silence fixture still emits nothing.**
- **The corpus: precision at or above the floor, and report which captures'
  admission changed.**

### Task 3 — every element carries its own pitch

For each element the decoder resolves, **measure the element's frequency over the
element's own samples** — peak with parabolic interpolation is sufficient; a
190 ms dah resolves to ~5 Hz, a 55 ms dit to ~18 Hz with interpolation.

- **Store it on the element** alongside its duration. It reaches the capture
  sheet's element records.
- **No behaviour changes in this task.** The corpus score must be identical —
  this task adds a measurement, and its acceptance is that nothing moves.

### Task 4 — near-tied senders separate into streams

**Cluster the elements of an admitted station by their measured pitch.**

- **One cluster: nothing changes.** The overwhelmingly common case must cost
  nothing.
- **Two clusters separated by more than the measurement's own resolution but
  within the detector's bandwidth** — the `002829` case, ~13 Hz — **the elements
  divide into two streams by pitch, and each stream is decoded separately**, each
  with its own speed and its own gap structure.
- **The screen labels them as two senders, and asserts nothing about their
  relationship** — not "a contact", not "answering", per Tim's ruling. Two pitches,
  two streams, labelled by their measured pitch.
- **The cluster decision is conservative**: when in doubt, one sender. A split
  that fragments a single wobbly sender into two would be worse than the collision
  it fixes. **State the separation criterion and its margin over the per-element
  measurement error.**

**Acceptance:**
- **On `002829`, two streams are reported near 602 and 615 Hz**, the element
  timeline shows runs, and each stream's dit CV is reported beside the combined
  0.47.
- **On `003229` and every single-sender fixture, exactly one stream**, and the
  decode is unchanged from task 2's.
- **Corpus: precision at or above the floor.**

### Task 5 — the whole set, measured

Run everything over every fixture and the corpus, and report **one table**:
per capture — admitted or not, at what pitch, streams found, characters emitted,
blocks, and for scored captures precision and yield.

**Lead the table with `003229`, `002443`, `002829` and the silence set** — the
four classes this unit exists for.

### Task 6 — what the sheet says about streams

The capture sheet gains the per-element pitch and, where task 4 split streams,
**per-stream lines** — pitch, element count, characters, speed — in the sheet's
own voice.

- **Wording is proposed in the report for Tim; do not invent idioms.** The sheet
  already has a voice and §12.1 puts its words with him.
- **The negative-counter fix from task 1 is verified here** on a fresh capture
  cycle in tests.

### Task 7 — the shredded pair, characterised *(the drop candidate)*

**Measure only. Change nothing. Promise nothing.**

On `003408` and `003419`: what is the signal's structure — element-length
distribution, swing, per-element pitch spread — and **what would a decoder need
that Hamlet does not have** to read it, if anything can. If the honest answer is
"nothing reads this", **say so**; a named limit is worth more than a vague hope.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**FT8, FT4 and every other digital mode.**

**Hysteresis on the spectral peak** — costed in unit 053, awaiting Tim's ruling,
**not built here.**

**The peak-referenced threshold and the hold-over** — unit 054's; whatever it
landed stands as it landed.

**The confidence quantities** (seven measured, none discriminates — no eighth),
**the joint decoder, the lattice, the evidence term, the settings contract, the
scanner, `CHANGELOG.md`, the missing `DECISIONS.md` records, the phrasebook, the
Twin PBT, the answer key's licensing, the dial-move threshold, the transcript
break's wording, the version bump.**

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not break the silence property**, and **do not modify its lock.** Detection
  sees more; it must not require less.
- **Do not break a capture that reads at 1.000.**
- **Do not let precision fall below the tree's floor.**
- **Do not assert a relationship between two streams.** Separate and label only.
- **Do not split one sender into two.** Conservative clustering, margin stated.
- **Do not select a pitch by average level anywhere.** If a second site does it,
  fix it or report it.
- **Do not measure signal strength against the filter's stopband.**
- **Do not adopt off a non-monotonic sweep.**
- **Do not build peak hysteresis.** Awaiting ruling.
- **Do not promise the shredded pair.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**Write `output.md` before you stop, for any reason at all. Do not hold it behind
a regression run.**

**The section that says what the owner should expect leads with one line: what
`cw-2026-08-31-003229` now shows on screen.** Then the table from task 5.

**The section that reports measurements leads with the swing-threshold sweep and
its margin over the silence fixtures.**

**If you finish every task, stop and report. Do not start the next unit.**
