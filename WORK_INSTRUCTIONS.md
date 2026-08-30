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

# Work instruction 052 — the window where somebody is keying

**ISSUED: 2026-08-30. A fresh order, not an amendment. Follows unit 051.**

**Six tasks; task 6 is the drop.**

## Why this unit exists

**Unit 051 found the same cause from two directions and neither was the one the
order predicted.**

**From the detection side.** A station present for the last fifteen seconds of a
thirty-second capture had its duty and swing computed over the whole recording.
The statistics described neither half, admission correctly answered no to the
wrong question, and a station the operator could hear plainly was refused.

**From the pitch side.** Task 7 measured `CwSpectralPeak` against synthetic
carriers of known frequency:

| condition | error |
|---|---|
| 500.09 Hz at 18, 22 and 25 WPM | **−0.021, −0.022, −0.021 Hz** |
| 500.09 Hz, no noise at all | −0.022 Hz |
| five carriers × four speeds, busy message | never worse than ±0.03 Hz |
| **700 Hz, very low duty** | **−1.25 Hz** |
| **800 Hz, very low duty** | **+1.26 Hz** |

**The 1.1 Hz that retired `N4L` is not a keying floor.** At the exact carrier the
peak is accurate to two hundredths of a hertz. **Every outlier is a low-duty
message**, and ±1.25 Hz is the magnitude seen on the real capture — which is short
and sparse.

**So the pitch error and the false rejection are the same fault: a statistic
computed over a window that is mostly silence.** Both fix the same way — **measure
over the stretch where somebody is actually keying.**

### What was measured and refused, and must not be retried

**Unit 051's task 3 replaced the Otsu threshold with a percentile-based one,
exactly as specified, and it failed on three independent counts:**

1. **The fraction sweep is not monotonic** — 0.601, 0.728, 0.751, 0.703, 0.770,
   0.787, 0.742, 0.738 across fractions 0.20 to 0.60.
2. **Every candidate is far below the floor** — best 0.787 precision against 0.888
   with Otsu.
3. **It fails its own acceptance test.** It lands **0.6 to 4.9 dB higher on every
   one of twelve captures**, median about 3.0 dB, where the criterion was "within a
   decibel or two."

**On `cw-2026-08-17-013347` the twentieth percentile falls at −110 dB, because that
recording is mostly digital silence and a percentile of silence is not a noise
floor.** That capture is the in-tree instance of this unit's whole subject.

**Otsu is correct wherever signal and noise have comparable mass. Do not replace
it. The window is what is wrong, not the formula.** `CwUnitEstimator.Threshold`
is kept in the tree with its numbers so nobody spends an evening rediscovering
this.

### The state of the corpus

**Unit 051 wired the squelch** — nothing is emitted from a pitch the survey has
not admitted — and it cost what it was measured to cost:

| | before | after |
|---|---|---|
| **precision** | 0.858 | **0.888** |
| yield | 0.914 | **0.745** |
| substitutions | 30 | **16** |

**Four W1AW anchors are red as a consequence** — `031905`, `032050`, `032113`,
`032129`. **Tim has ruled on them; task 2.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches. Trust
the tree over this order everywhere they differ.

**The captures unit 051 was written about do not exist and will not.**
`cw-2026-08-30-001650` and `-001547` were never in the repository and are not
coming. **No acceptance criterion in this order names them.** The eight captures of
2026-08-29 are likewise absent.

**Every measurement in this unit is against the corpus that is in the tree.** If a
task cannot be verified against it, **say so and do not build the change** — unit
051 declined to build an unverifiable admission change and that was correct.

From unit 051's report: `TheSilencePropertyIsLockedTests` 6 passing, green,
unmodified. `TheAdjudicatedReadingsKeepReadingTests` 9 passing, 4 failing — the
four are task 2's. `CwEmissionGateTests.NoSpeedIsNamedWithoutCharactersToNameItFrom`
was **red before unit 051**, verified by reverting.

**Record both suites and the corpus score before task 3.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings:**

> **The four W1AW anchors are re-expressed with their reason, in the same form
> `N4L` was.** They were set on text that included stretches the survey never
> admitted. **Hamlet is not reading those bulletins worse — it is declining to
> assert a part it was never entitled to assert.**
>
> Rejected: reverting the squelch — it restores invented characters on an empty
> band. Rejected: lowering the floors to fit the change, which is the move §12.5
> exists to stop. Rejected: a narrower squelch — every narrowing is a second test
> for a state `unkeyed` already computes.

> **`N4L` is retired as a reading anchor and the measured pitch is kept.** It
> returns when the peak can find that station honestly.

> **The phase goal is 85% correct CW, precision before yield.**

> **Do not break the silence behaviour.**

> **The only measurement is against real data from the real radio.**

> **FT8, FT4 and every other digital mode are outside this conversation's scope.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode.
- **HM-DEC-120** — nothing emitted on audio holding no signal, and no letters from
  a pitch nobody judged to be a station. **Tightened only.**
- **§0.4** — reproduce, then change, then measure.
- **HM-DEC-007** — tested against WAV fixtures. **HM-DEC-091** — captures are
  read-only.
- **§12.5** — a floor is not lowered to fit a change.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The measurement rule that governs every task

**Every change is measured with `CwAccuracy` over the whole scored corpus, before
and after.** Every task reports **precision, yield, substitutions**.

- **Precision must not fall below 0.888.** A change that lowers it is reverted and
  reported.
- **`TheSilencePropertyIsLockedTests` runs after every task and may not be
  modified.**
- **A floor is not lowered.** Where a floor and a change conflict, the change is
  reported and the floor stands.

## The tasks

### Task 1 — how much of each capture holds a station

**Measure before building. This is what tells us whether the window change can
help at all on the corpus we have.**

For every capture in the tree, report:

- **The fraction of the recording in which a station is present**, measured from
  the envelope at the admitted pitch by a method that does not share code with
  admission.
- **The longest contiguous stretch of presence**, in seconds.
- **The duty within that stretch, against the duty over the whole recording.** The
  gap between those two numbers is the size of this unit's subject, per capture.
- **The twentieth percentile of the envelope**, which on `013347` sits at −110 dB.

**Rank the corpus by that gap.** A capture where presence is 95% of the file cannot
demonstrate anything here; a capture near `013347` can. **Say plainly how many
captures in the tree can test this change.** If the answer is one or two, **say so
and say what that does to the confidence in any result.**

### Task 2 — the four W1AW anchors, re-expressed

Per Tim's ruling.

- **Re-express each with its reason in the test itself**, as `N4L` was: what the
  capture reads now, that the earlier floor included stretches the survey never
  admitted, and **what would bring the full reading back.**
- **Do not delete them. Do not lower them. Do not change the decoder to satisfy
  them.**
- **`032129` still reads `…ON FORECAST BUAELETIN ARLP034` where it is admitted** —
  record what each of the four still reads, so the re-expression carries the
  evidence.
- **Write the amendment for Tim to enter. Do not mint a decision id.**

### Task 3 — the admission window

**Compute the admission statistics over the strongest contiguous stretch, not over
the whole recording.**

- **The window is chosen by signal strength alone.** Not by where characters were
  emitted, not by where a pitch was admitted — **either would make the test
  circular.**
- **State the minimum window length and why.** A station unmistakable for a few
  seconds is a station; a window short enough to fit inside one dah is noise.
- **Sweep the window length and report the curve.** Adopt only on a monotonic
  region.
- **The threshold within the window stays Otsu.** Unit 051 measured the alternative
  and it is refused; **this task changes what Otsu is applied to, not what it is.**

**Acceptance:** measured over the corpus, **precision must not fall below 0.888**.
Report per capture, and **lead with the captures task 1 ranked as able to
demonstrate the change.** If none can, **report that the change is unverifiable on
this corpus and do not adopt it** — building an unverifiable admission change is
what unit 051 correctly declined to do.

### Task 4 — the pitch measured over the same window

Task 7 of unit 051 showed the peak is accurate to two hundredths of a hertz on a
busy message and errs by ±1.25 Hz on a low-duty one.

- **Measure `CwSpectralPeak` over the task 3 window rather than over a fixed
  rolling span.**
- **Re-run task 7's synthetic sweep** — the same carriers, speeds and duties — and
  report the error table before and after. **The low-duty cases are the ones that
  must improve; the busy cases must not degrade.**
- **Then measure the corpus.** Precision must not fall below 0.888.

### Task 5 — does `N4L` come back?

`cw-2026-08-17-134712` sits at 500.09 Hz and the peak read 501.2.

- **After tasks 3 and 4, what does the peak read on that capture?**
- **If it reads within a tenth of a hertz, `N4L` returns and the anchor is restored
  to a reading anchor** with a note saying what brought it back.
- **If it does not, report the residual error and why**, and the anchor stays as
  unit 051 re-expressed it.
- **Do not change the decoder to make it return.**

### Task 6 — the confidence quantities, re-measured *(the drop candidate)*

**Measure only. Change nothing. Do not add an eighth quantity.**

Seven quantities were measured against correctness and none discriminated. **All
seven were measured against a decoder that was more than a hundred hertz from the
station on four captures of twelve**, and unit 050 noted that is not the same
experiment.

- **Re-measure all seven against the corpus as it now stands** — measured pitch,
  squelch wired, and whatever tasks 3 and 4 land.
- **Report each correlation beside its value from the earlier unit.**
- **Draw no conclusion beyond the numbers.** If one now discriminates, that is the
  next unit's subject, not this one's.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**FT8, FT4 and every other digital mode**, the digital tab, the digital capture
press, the waterfall.

**The threshold formula.** Measured and refused in unit 051 on three counts.
`CwUnitEstimator.Threshold` stays in the tree with its numbers and nothing calls
it.

**The joint decoder**, the lattice's structure, the evidence term's magnitude, the
emission gate and the character floor beyond what the squelch already does.

Also: the settings contract; the scanner and the calling cycle; `CHANGELOG.md`;
the missing `DECISIONS.md` records; the phrasebook and the recent-places row; the
Twin PBT; the answer key's licensing; the dial-move threshold; the transcript
break's wording; the version bump, which is unruled and **must not be guessed a
third time**.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not break the silence property**, and **do not modify its lock.**
- **Do not let precision fall below 0.888.** Revert and report.
- **Do not lower a floor.**
- **Do not replace the Otsu threshold.** Measured and refused.
- **Do not choose the window from the decoder's own output.** Circular.
- **Do not adopt off a non-monotonic sweep.**
- **Do not build a change this corpus cannot verify.** Report it instead.
- **Do not write an acceptance criterion against a capture that is not in the
  tree.**
- **Do not add an eighth confidence quantity.**
- **Do not change the decoder to make `N4L` return.**
- **Do not guess the version bump.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused push
is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**Write `output.md` before you stop, for any reason at all. Do not hold it behind a
regression run.**

**The section that reports measurements leads with task 1's table — how many
captures in this corpus can demonstrate a window change at all — and then task 4's
synthetic error table before and after.**

**The section that says what the owner should expect leads with the corpus
precision and whether any capture reads that did not before.**

**If you finish every task, stop and report. Do not start the next unit.**
