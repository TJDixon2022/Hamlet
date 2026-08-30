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

# Work instruction 053 — find where the good reads broke, and lock them

**ISSUED: 2026-08-30. A fresh order, not an amendment. Follows unit 052.**

**Six tasks; task 6 is the drop. The band opens within the hour and tasks 1 to 4
are sized to be done before it does.**

## The goal this unit serves

**85% correct CW on a capture where the pitch is right.**

**Not an average over twelve recordings.** An average can rise while the easy cases
collapse, and that is what this unit exists to test. Precision went 0.858 → 0.888
across units 050 and 051 while the operator's experience of the app got worse.
**Those two facts are compatible, and if both are true the score has been
measuring the wrong thing.**

## Why this unit exists

**The operator reports that stations he used to read with a strong signal now
produce garbage.** He has no capture of one, and he should not have to produce
one — **the evidence is already in the tree.**

Three captures read at **1.000 precision**: `KD0UN`, `AA4MP/4` and `VA3VRR`. Those
are the good reads. **Nothing in this project has ever run them against each commit
in turn.** Every unit since 048 has reported one number averaged over the whole
corpus, and a per-capture table only at the end, against the previous unit — never
against the state before the run of changes began.

**Three changes shipped in that window and none had a strong-signal check:**

- **Unit 050 replaced the tone tracker with `CwSpectralPeak`.** The tracker held
  its pitch once locked. The peak re-measures independently and takes the loudest
  bin in the range. **On a band with more than one signal in the passband it can
  walk between them** — and every capture in the corpus has one dominant station,
  so the corpus cannot see that.
- **Unit 048 rebuilt the lattice by `(hop, kind)`**, making legal paths reachable
  that were previously discarded. Measured on the corpus average.
- **Unit 051 wired the squelch.** Nothing is emitted from a pitch the survey has
  not admitted. **If admission is marginal on a station that used to read, that is
  blocks where text used to be** — and unit 051 measured exactly that on four W1AW
  bulletins.

**Any of the three could have done it. This unit does not guess between them; it
bisects.**

### What an independent measurement already rules out

Nine captures from the evening of 2026-08-29 were analysed outside Hamlet on a
decoder sharing no code with it. On all nine the pitch was found correctly at
598.1–599.1 Hz, one dominant carrier at 599.12 Hz standing 46.7 dB over the floor.
**The independent decoder produced the same class of garbage Hamlet does.**

So **that** station is hard audio, not a regression: during key-down its envelope
ripples **49–61% peak to peak** with dominant modulation at 7, 37 and 53 Hz —
fading fast enough to punch holes in single elements. **That is a different problem
and it is not this unit's.**

**It also means the regression the operator reports is not visible in those nine
files, and must be found where the good reads are.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches. Trust
the tree over this order everywhere they differ.

From unit 052's report: precision **0.888**, yield **0.745**, substitutions **16**.
`TheAdjudicatedReadingsKeepReadingTests` 13 passing, 0 failing.
`TheSilencePropertyIsLockedTests` green and unmodified. `CwUnitEstimator.Threshold`
and `CwSpectralPeak.FindOverLoudestStretch` both exist and neither is called —
measured and refused, kept with their numbers.

**Record both suites and the corpus score before task 2.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings:**

> **The phase goal is 85% correct CW on a capture where the pitch is right,
> precision before yield.**

> **The 500.09 Hz figure that retired `N4L` is treated as unsourced.** It exists
> only in a comment in `CwDecoder.cs`; it is not in HM-DEC-144, which records the
> element timings and the callsign and no carrier frequency. Three independent
> windows read that station at 500.99–501.16 and agree with each other far better
> than any agrees with 500.09, and the instrument reads a known 500.09 to within
> 0.023 Hz.
>
> Rejected: spending another unit on it. Rejected: changing the decoder to make
> `N4L` return.

> **The four W1AW anchors and `N4L` stay retired with their reasons.**

> **Do not break the silence behaviour.**

> **The only measurement is against real data from the real radio.**

> **FT8, FT4 and every other digital mode are outside this conversation's scope.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode.
- **HM-DEC-120** — nothing emitted on audio holding no signal. **Tightened only.**
- **§0.4** — reproduce, then change, then measure.
- **HM-DEC-007** — tested against WAV fixtures. **HM-DEC-091** — captures are
  read-only.
- **§12.5** — a floor is not lowered to fit a change.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The tasks

### Task 1 — the bisect *(first, and it is the unit)*

**Mechanical. No interpretation. A table.**

- **Identify every commit from unit 048's first through the current head.** List
  them with their unit number and one-line subject.
- **At each commit, run the three clean captures** — `KD0UN`, `AA4MP/4`, `VA3VRR`,
  by whatever filenames the tree gives them — **and record the decoded text and the
  per-capture precision.**
- **Print one row per commit.** Nothing else. **The commit where a 1.000 capture
  stops reading is the answer**, and it is read off the table rather than argued
  for.

**If the harness cannot be run at an old commit**, say so and bisect by reverting
the individual changes at head instead — the spectral peak, the lattice indexing,
the squelch — **one at a time, measured, and reported the same way.**

**If none of the three ever falls**, that is the finding and it is decisive: the
regression is not in the decoder and this unit reports it and stops at task 5.
**Say it plainly rather than hunting for something to blame.**

### Task 2 — the clean captures become a floor that cannot be averaged away

**Whatever task 1 finds.**

- **A test asserting that every capture currently reading at 1.000 continues to.**
  Named individually, each with its text.
- **It is checked separately from the corpus average and it may not be modified**,
  in the same form as `TheSilencePropertyIsLockedTests`.
- **A change that raises the corpus average while breaking one of these fails.**
  That is the rule this unit exists to establish, and it is why the operator's
  experience diverged from the number.

**Acceptance:** the test exists, passes at head, and the report names every capture
it covers.

### Task 3 — the peak against a second signal

**Unit 050's replacement of the tracker is the suspect this corpus cannot test**,
because every capture in it has one dominant station.

**Measure only in this task. Change nothing.**

- **Build a two-signal case from captures already in the tree** — sum a clean
  capture with a second at a different pitch and a lower level, at several level
  differences. **This is a unit test, not corpus evidence, and it must say so.**
- **Report what `CwSpectralPeak` does as the second signal rises**: at what level
  difference it stops holding the stronger station, and whether it walks between
  them within one recording.
- **Report what the old tracker would have done on the same input**, if it is still
  in the tree and runnable.

**This is the measurement that says whether the operator's report is explained by
the peak.** A real 40 m evening has more than one signal in a 500 Hz passband; the
corpus never does.

### Task 4 — what the fix is, costed, not built

**From tasks 1 and 3, state what would restore the good reads**, with the cost of
each option in HM-DEC-010's table form, **and build none of them.**

The likely shapes, to be confirmed or replaced by what the tasks find:

- **Hysteresis on the peak** — hold the current pitch unless a competitor exceeds
  it by a stated margin for a stated time. Restores what the tracker had without
  restoring the tracker's error.
- **Reverting one of the three changes**, with the corpus cost stated.
- **Nothing** — if task 1 shows no regression, say so.

**Recommend; do not decide. This is Tim's ruling and the band is open.**

### Task 5 — the fading detector, measured *(evidence for the next unit)*

**Measure only. Change nothing.**

Independent analysis of the 2026-08-29 evening found that during key-down the
envelope ripples **49–61% peak to peak**, with dominant amplitude modulation at
**7, 37 and 53 Hz** — fading fast enough to break a single element into pieces.
**That is why the same recording measures 21 WPM at one threshold and 37 at
another, and why the ear reads it and an envelope detector does not.**

- **Reproduce that measurement on captures in the tree**: for every capture, the
  peak-to-peak ripple within key-down stretches longer than 120 ms, and the
  dominant modulation frequency.
- **Rank the corpus by it.** Captures that read at 1.000 should sit at the bottom.
- **State what a hold-over on the key-down state would have to be sized at** to
  bridge the observed dropouts, from the measured modulation rates — **and do not
  build it.**

**This is the next unit's evidence and it is measured now while the numbers are
cheap.**

### Task 6 — the confidence quantities, re-measured *(the drop candidate)*

**Measure only. Change nothing. Do not add an eighth quantity.**

Seven were measured against a decoder that was more than a hundred hertz from the
station on four captures of twelve. **Re-measure all seven against the corpus as it
now stands and report each beside its earlier value.** Draw no conclusion beyond
the numbers.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**FT8, FT4 and every other digital mode**, the digital tab, the digital capture
press, the waterfall.

**The threshold formula** and **the admission window** — both measured and refused,
in units 051 and 052. `CwUnitEstimator.Threshold` and
`CwSpectralPeak.FindOverLoudestStretch` stay in the tree, uncalled, with their
numbers.

**The `134712` carrier and `N4L`.** Ruled: unsourced, retired, closed.

**The joint decoder**, the evidence term's magnitude, the emission gate and the
character floor beyond what the squelch already does.

Also: the settings contract; the scanner and the calling cycle; `CHANGELOG.md`;
the missing `DECISIONS.md` records; the phrasebook and the recent-places row; the
Twin PBT; the answer key's licensing; the dial-move threshold; the transcript
break's wording; **the version bump, which is unruled and must not be guessed a
third time.**

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not break the silence property**, and **do not modify its lock.**
- **Do not build a fix in this unit.** Task 4 costs it; Tim rules; the next unit
  builds it. **The band is open and a wrong fix shipped tonight is worse than
  none.**
- **Do not let a capture that reads at 1.000 stop reading.**
- **Do not lower a floor.**
- **Do not present the two-signal case as corpus evidence.** It is synthetic and
  must say so.
- **Do not add an eighth confidence quantity.**
- **Do not reopen the threshold formula, the admission window, or `N4L`.**
- **Do not guess the version bump.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused push
is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**Write `output.md` before you stop, for any reason at all. Do not hold it behind a
regression run.**

**The section that reports measurements leads with task 1's table — one row per
commit, the three clean captures' text and precision — and nothing before it.**

**The section that says what the owner should expect leads with a plain answer to
one question: did a good read break, and if so at which commit.**

**If you finish every task, stop and report. Do not start the next unit.**
