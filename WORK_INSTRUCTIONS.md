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

# Work instruction 054 — the threshold reference, and the hold-over

**ISSUED: 2026-08-30. A fresh order, not an amendment. Follows unit 053.**

**Six tasks; task 6 is the drop. Two changes ship in this unit.**

## The goal this unit serves

**85% correct CW on a capture where the pitch is right, precision before yield.**

**This unit ships two changes to the detector.** Both come from analysis of the
operator's own audio, not from a report, and both attack the same measured fault:
**the detector breaks single elements apart.**

## What was found in the audio

Nine captures from the evening of 2026-08-30, 7.058 MHz, analysed outside Hamlet
on code sharing nothing with it. **The pitch is not the problem** — an independent
peak finder reads 598.1–599.1 Hz on all nine, and the fine spectrum shows one
dominant carrier at **599.12 Hz standing 46.7 dB over the floor.**

### Finding 1 — the threshold is referenced to the wrong thing

The envelope printed as digits, one character per 5 ms, scaled 0–9 across the
20th to 98th percentile:

```
15s 04202004211123124024300000378988888871005888788998888889998840023012334341...
19s 84332333410000000367502033300377223323320232432323323420000054333233331000...
20s 61000000000573333224657578633303435540464532010226653402000410434677764323...
```

**Second 15 is the strong station at levels 8 and 9. Seconds 19 and 20 are
something else, sitting at 2–4 and 5–7, with its own keying.** The fine spectrum
carries energy at **660.6 and 688.5 Hz** — sixty and ninety hertz off the carrier,
which a 12 ms detector window passes straight through.

**A threshold placed halfway between the 20th and 98th percentile of the whole
envelope lands in the middle of the second signal.** Its fades and peaks then cross
the line and are counted as elements of the first.

**Measured, referencing the threshold to the loudest signal instead — the 98th
percentile minus a fixed number of decibels:**

| threshold | dit | dit CV | dah | **dah CV** | ratio |
|---|---|---|---|---|---|
| span, 50% of p20→p98 | 57 | 0.559 | 162 | **0.267** | 2.83 |
| **peak − 5 dB** | 64 | 0.516 | 174 | **0.104** | 2.73 |
| peak − 6 dB | 66 | 0.529 | 188 | **0.121** | 2.83 |
| peak − 8 dB | 58 | 0.559 | 162 | 0.267 | 2.80 |

**Dah scatter falls from 0.267 to 0.104.** For calibration, every capture in the
corpus that reads has a dah CV between **0.028 and 0.134**. **The peak-referenced
threshold moves this station from worse-than-anything-readable into the readable
range on that measure.** Confirmed on `005913` as well: 0.20 → 0.12 at peak − 5 dB.

### Finding 2 — dits are being broken and dahs are not

**Dit scatter never falls below about 0.44**, at any detector bandwidth from 12 to
40 ms and any threshold reference tried. **A dit whose length varies by half its
own duration is not a dit; it is a dit that has been cut in two.**

**The cause is measured.** Within key-down stretches longer than 120 ms the
envelope ripples **49–61% peak to peak**, with dominant amplitude modulation at
**7, 37 and 53 Hz**. That is fading fast enough to punch a hole through a single
element.

**Dahs survive it because they are long enough to bridge a dropout. Dits are
not.** That is exactly the asymmetry in the numbers: dah CV 0.10, dit CV 0.44.

**The operator copies this station by ear.** A human hearing a tone through a 20 ms
amplitude dip hears one continuous tone; an envelope detector sees two elements.

### What was tried and did not work — do not retry it

**A phase-coherence detector** — `|Σz| / Σ|z|` over the same window, which is
amplitude-invariant and should in principle ignore fading. **Measured across five
window lengths from 8 to 45 ms it gives no useful improvement over amplitude**:
dit CV 0.500–0.635 against amplitude's 0.504–0.559. **It is not the answer and it
is not to be built.**

**Narrowing the detector bandwidth alone.** Windows of 20, 25, 32 and 40 ms were
swept against four threshold references. **The best dit CV over the whole sweep is
0.442.** Bandwidth is not the answer either.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches. Trust
the tree over this order everywhere they differ.

**Unit 053 was running when this was written and its report has not been seen.**
**Task 1 must state what 053 landed** — in particular whether it found a commit at
which a clean read broke, and whether the clean-capture lock exists. **If that lock
exists, it governs this unit.**

From unit 052: precision **0.888**, yield **0.745**, substitutions **16**.
`TheSilencePropertyIsLockedTests` green and unmodified. `CwUnitEstimator.Threshold`
and `CwSpectralPeak.FindOverLoudestStretch` exist and neither is called.

**The nine captures of 2026-08-30 may or may not be in the tree.** Every acceptance
criterion below that names one is conditional: **if they are absent, say so once,
and verify against the corpus that exists.** No task is blocked by their absence —
both changes are measurable on the current corpus.

**Record both suites and the corpus score before task 2.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings:**

> **The phase goal is 85% correct CW on a capture where the pitch is right.**

> **Ship these two changes.** They come from the audio, and they are to be built
> and measured, not measured first and built later. **The operator has stated
> plainly that this project has measured its way backwards.**

> **The 500.09 Hz figure is unsourced; `N4L` and the four W1AW anchors stay
> retired with their reasons.**

> **Do not break the silence behaviour.**

> **The only measurement is against real data from the real radio.**

> **FT8, FT4 and every other digital mode are outside this conversation's scope.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode.
- **HM-DEC-120** — nothing emitted on audio holding no signal. **Tightened only.**
- **§0.4** — a fix that cannot be shown to fix anything is a guess.
- **HM-DEC-007** — tested against WAV fixtures. **HM-DEC-091** — captures are
  read-only.
- **§12.5** — a floor is not lowered to fit a change.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The rule that governs both changes

- **Precision must not fall below 0.888.**
- **A capture that reads at 1.000 must keep reading at 1.000.** If unit 053 built
  that lock, it governs. If it did not, **build it as the first thing in task 1**
  — naming `KD0UN`, `AA4MP/4` and `VA3VRR` — because a change that lifts the
  average while breaking an easy read is what this project has been shipping.
- **`TheSilencePropertyIsLockedTests` runs after every task and may not be
  modified.**

## The tasks

### Task 1 — where the tree is, and the clean-read lock

**State what unit 053 landed**, and run both suites and the corpus score.

**If the clean-capture lock does not exist, build it now** — a test naming every
capture that currently reads at 1.000, with its text, checked separately from the
average and not modifiable by any later task in this unit.

Then find, with file and line:

- **Where the key-down threshold is computed**, and what it is referenced to.
- **Whether anything already bridges a short dropout inside a key-down** — a
  hold-over, a hysteresis, a minimum-off filter. **If something exists, report its
  parameters**; task 3 changes it rather than adding a second one.
- **The detector window length** and the bandwidth it implies.

### Task 2 — the threshold is referenced to the loudest signal

**Replace the span reference with a peak reference.** The threshold becomes the
envelope's high percentile minus a fixed number of decibels, rather than a fraction
of the distance between a low and a high percentile.

- **Sweep the setback and report the curve** — 3, 4, 5, 6, 8, 10, 12 dB — with
  precision, yield and substitutions at each, over the whole corpus.
- **Report the dah CV per capture at each point**, because that is the measure that
  moved from 0.267 to 0.104 on the operator's audio and it is the one to watch.
- **Adopt only on a monotonic region.** Unit 045 refused to adopt off a
  non-monotonic sweep and that is the standard.
- **Keep the existing Schmitt hysteresis** around whatever threshold results.

**Acceptance:** precision at or above 0.888, no clean read broken, silence lock
green. **If the sweep shows no setback that holds precision, report that and
revert** — the finding stands as evidence for the next unit either way.

### Task 3 — a hold-over across a dropout

**A key-down that dips below the threshold for less than a stated time does not
end. The element continues.**

- **Size it from the fading, not from a guess.** Dominant amplitude modulation was
  measured at **7, 37 and 53 Hz**; a 53 Hz ripple has a half-period near 9 ms and a
  37 Hz ripple near 13 ms. **State the hold-over chosen and the arithmetic that
  produced it.**
- **Sweep it and report the curve** — 0, 8, 12, 16, 24, 32 ms — with precision,
  yield, substitutions and **dit CV per capture** at each.
- **The hold-over must be shorter than the shortest legitimate key-up gap.** At
  30 WPM an inter-element gap is 40 ms. **State the bound and assert it.**
- **It applies only within a key-down that has already been admitted.** It must not
  extend a noise crossing into an element, and **it must not bridge across a real
  gap.**

**Acceptance:** **dit CV falls.** It is 0.44–0.56 on the operator's audio against
0.028–0.134 on every capture that reads. **Report it per capture before and
after.** Precision at or above 0.888, no clean read broken, silence lock green.

**This is the change with the best chance of moving the goal**, because dits being
cut in two is the specific fault the audio shows and nothing in the decoder
currently addresses it.

### Task 4 — the two together

**Measure the corpus with both changes, and report the table one row per
configuration**: neither, threshold only, hold-over only, both.

- **Precision, yield, substitutions, and dah CV and dit CV per capture.**
- **If the two interact badly** — either alone helps and together they do not —
  **say so and ship the better single change.**

### Task 5 — the second signal, and what it costs

**Measure only. Change nothing.**

The passband holds more than one station. The fine spectrum on `010032` shows the
carrier at 599.12 Hz at +46.7 dB and further energy at **660.6 and 688.5 Hz** at
about +31 dB — **sixty and ninety hertz away, inside a 12 ms detector's
bandwidth.**

- **For every capture in the corpus, report how many carriers stand more than
  15 dB over the floor within ±120 Hz of the admitted pitch**, and their level
  differences.
- **State how many captures have a second signal at all.** If the answer is one or
  two, **say plainly that the corpus cannot represent the operator's band**, which
  is the standing reason the corpus and his experience disagree.
- **Do not build a second-signal rejector.** That is a later unit and it needs this
  measurement first.

### Task 6 — what the corpus cannot see *(the drop candidate)*

**A short written note in the repository, no code.**

Every capture in the corpus has one dominant station keyed throughout. The
operator's band has fading, overlap and partial presence. **Write down what
classes of failure the corpus structurally cannot test**, drawing on task 5's
count and unit 052's presence table, **and what a capture set that could test them
would have to contain.**

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**FT8, FT4 and every other digital mode**, the digital tab, the digital capture
press, the waterfall.

**The phase-coherence detector** and **narrowing the detector bandwidth alone** —
both measured on the operator's audio and neither works. Do not build either.

**The threshold formula of unit 051** and **the admission window of unit 052** —
measured and refused. `CwUnitEstimator.Threshold` and
`CwSpectralPeak.FindOverLoudestStretch` stay uncalled with their numbers.

**The `134712` carrier and `N4L`.** Ruled: unsourced, retired, closed.

**The confidence quantities.** Seven measured, none discriminates. **Do not add an
eighth.**

Also: the joint decoder; the evidence term's magnitude; the settings contract; the
scanner and the calling cycle; `CHANGELOG.md`; the missing `DECISIONS.md` records;
the phrasebook and the recent-places row; the Twin PBT; the answer key's licensing;
the dial-move threshold; the transcript break's wording; **the version bump, which
is unruled and must not be guessed a third time.**

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not break the silence property**, and **do not modify its lock.**
- **Do not break a capture that reads at 1.000.**
- **Do not let precision fall below 0.888.**
- **Do not lower a floor.**
- **Do not build a phase-coherence detector.** Measured, does not work.
- **Do not let the hold-over bridge a real inter-element gap.** State the bound and
  assert it.
- **Do not add a second hold-over** if one already exists — change the one that is
  there.
- **Do not adopt off a non-monotonic sweep.**
- **Do not build a second-signal rejector.** Task 5 measures; a later unit builds.
- **Do not add an eighth confidence quantity.**
- **Do not guess the version bump.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused push
is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**Write `output.md` before you stop, for any reason at all. Do not hold it behind a
regression run.**

**The section that reports measurements leads with task 4's table — neither,
threshold only, hold-over only, both — with dit CV and dah CV per capture beside
precision and yield.**

**The section that says what the owner should expect leads with whether any capture
reads that did not before, named.**

**If you finish every task, stop and report. Do not start the next unit.**
