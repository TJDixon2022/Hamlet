# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwGate.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

These four files are fixed. Do not substitute a different file for any of
them and do not report a check against a file this list does not name.

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**This unit measures one thing and ships nothing.** That is the whole scope. If it
ends with a table and no code in `src/`, it has succeeded.

Last session named the question that blocks everything else: **a mark of about two
dits is either a dah from a fist that runs its elements together, or two elements
the gate joined, and no measurement of its length will say which.** On
`cw-2026-08-17-134712` the three mark populations sit at 14, 51 and 238 ms and
dropping the lowest is correct. On `tightfist-easy` they sit at a dit, a dah and a
merged pair, and dropping the lowest drops the dits. To a fit that knows only
length, the two cases are identical.

**Tim's ruling: measure amplitude before building anything on it.**

The reasoning is that chatter and merged elements differ physically even where they
do not differ in length. A sliver the gate chopped out of band noise is a threshold
crossing — it should sit near the detection floor. A merged element is a real
keyed mark at the sender's full signal — it should sit on the plateau. **That is a
different quantity from length, it is not a constant, and it is not
HM-OPEN-054's ground.**

**It may well be wrong.** On a weak station the plateau may not stand far enough
above the floor to separate anything. That is why this unit measures and does not
build: one task spent eliminating a candidate honestly is how the last three
findings in this project arrived.

---

## What makes this measurable now

`cw-2026-08-17-134712` has adjudicated ground truth, recorded last session as
**HM-DEC-144**: the station is `N4L`, its elements run 21.45 s to 23.01 s, dit
56.3 ms, dah 238.3 ms, ratio 4.24.

So for every mark inside that window it is already known which are the station's
and which are not. **The answer exists before the measurement, which is what makes
the measurement worth anything.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.
  **2,076 tests, four failing. Anything above four is new.**
- Last session shipped nothing to `src/`. The three-way fit and the `Refine` change
  were built, measured and withdrawn. **Confirm they are not in the tree.**

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-144 — `cw-2026-08-17-134712` holds a station and its callsign is `N4L`.**
The ground truth this unit measures against.

**HM-OPEN-054 and HM-DEC-143 remain parked.** Amplitude is a property of a single
mark. **Whether a mark took part in a character is structure and is parked ground.
If the work reaches for it, stop.**

**HM-DEC-091 — one source, and it says which.** A mark whose amplitude cannot be
recovered is reported as such and is not given a plausible number.

**HM-OPEN-053 — `ShortestVote` stays at 5. `MaximumRatio` stays at 3.8.**

**HM-DEC-093 — no radio. Nothing here needs one.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — The amplitude of every mark, on the recording where the answer is known

For `cw-2026-08-17-134712`, across the twenty most recent marks at the moment the
callsign is being sent, report for each mark:

- its start time and its length in milliseconds,
- **its envelope amplitude** — state which statistic you chose, peak or median or
  otherwise, and why,
- the envelope floor at that moment, so the amplitude can be read as a height above
  it rather than as a bare number,
- and **whether it is one of `N4L`'s elements**, which HM-DEC-144 settles.

**Then say whether the two groups separate, and by how much.** A number, not an
impression. If they overlap, say by how much and on which marks.

**Report this before anything else and do not proceed past task 2 if it does not
separate.**

---

## Task 2 — The same measurement where it should fail

Repeat on `tightfist-easy`, where the short population is *merged elements* rather
than chatter.

**The prediction under test: there, the short marks should sit at full signal, not
near the floor**, because they are real keyed marks. If amplitude is a real
discriminator, this fixture must look different from `134712` — and if it looks the
same, **amplitude does not work and this unit ends here with that finding.**

Also report it on `cw-2026-08-18-004507`, which decodes, as a control.

---

## Task 3 — Say what it means, and build nothing

One paragraph, and it is the deliverable:

- **If the groups separate on `134712` and the fixtures look different**, say what
  a discriminator built on it would look like and what would have to be true for it
  to hold on a weak station. **Do not build it.**
- **If they do not separate**, say so plainly. That is a good outcome for one
  task's work and it removes a candidate.
- Either way, say whether the separation is large enough to survive a station ten
  decibels weaker, using the numbers rather than a judgement.

**Do not change `src/`. Do not touch `MedianOfShortCluster`, the three-way fit,
`Refine`, the gate, the survey or the meter.** *A measurement that arrives with a
change attached cannot be read on its own, and reading this on its own is the
entire point.*

---

## Parked — do not touch, do not raise

- **HM-OPEN-054 and HM-DEC-143**, and anything about whether a mark took part in a
  character. **The nearest boundary in this unit. If a measurement needs to know
  what character a mark belongs to, stop.**
- **`MedianOfShortCluster`.** Named last session as the line the callsign dies on.
  Its unit comes after this one.
- **The three-way fit and `Refine`.** Both measured, both withdrawn, both waiting
  on this answer.
- **The keying meter and `CwKeyingThresholds`.**
- **Why the 19th's stations are missing from the audio.**
- **The 69 and 233.**
- **HM-OPEN-052**, the five synthesized tests, rulings 096–133, the scorer,
  `CaptureAudioAsync` end to end, `TheRosterIsOneFilePerEvening`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

Unit-specific:

- **Do not introduce an amplitude threshold.** *Sixth instance of the error class
  four rulings have gone on closing. This unit reports heights; it does not decide
  a cut.*
- **Do not make the answer prettier than it is.** *A candidate eliminated in one
  task is worth more than a candidate that looks promising and costs three.*
- **Do not adjudicate any recording.** *`N4L` is established. Everything else is
  Tim's ear.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139.

**Section 1 opens with task 1's table.**

**Section 2 says, in one sentence, whether amplitude separates chatter from
elements** — and it says so even when the answer is no.

**Stop and report.**
