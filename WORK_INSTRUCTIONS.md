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
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**Hamlet listens to CW through a filter about 100 Hz wide. The signal occupies
about 40. Every hertz beyond what the signal needs admits noise for nothing.**

This is a receiver design error, not a decoder logic error, and it sits upstream of
everything four sessions have spent this week chasing. Narrowing the detection
bandwidth throws away noise power and keeps all the signal. Measured, on real
off-air recordings:

| recording | swing at 100 Hz | swing at 40 Hz | runs under 25 ms, 100 Hz | at 40 Hz |
|---|---|---|---|---|
| `cw-2026-08-21-015834` | 21.5 dB | **24.9 dB** | 27 | **1** |
| `cw-2026-08-21-020033` | 20.4 dB | **23.7 dB** | 1029 | **517** |
| `cw-2026-08-21-015432` | 19.2 dB | **22.5 dB** | 841 | **424** |
| `cw-2026-08-18-003016` | - | - | 1328 at 200 Hz | **272 at 40 Hz** |

**Three to three and a half decibels on every recording, consistently, for free.**

**And the sub-25 ms runs largely vanish.** Those runs are the thing two work orders
and two sessions have tried to exclude after the fact, with rulings sought about
element floors and vote windows. **They are not elements and they are not a decoder
defect. They are noise the filter is admitting, and a narrower filter deletes them
at the source.**

### The decode this produced

`cw-2026-08-18-003016.wav`, 40 Hz detection bandwidth, threshold at the 55th
percentile of the log envelope, marks shorter than 0.4 of the fitted dit dropped:

```
E= HADA KPA15TT ITWAS #K = STILL HVE MY ETO 91B TT JUST VFB TUBELIN
```

`ETO 91B` is an Alpha 91B amplifier, `VFB` is very fine business, `TUBE LIN` is a
tube linear. **That is a legible exchange out of a recording Hamlet reads as
fragments.**

### The other two things that decode needed

Both were measured on the same recording and both are smaller than the bandwidth.

**The threshold percentile matters enormously.** On identical audio: the 45th
percentile gives gibberish, the 50th gives partial words, the 55th gives the line
above. Hamlet's threshold sits midway between the 10th and 90th percentile, which
is the 50th. **One notch from working.**

**Mark rejection must scale with the fitted dit, not a fixed millisecond figure.**
0.4 dits was used above. A previous session measured the same thing from a
different direction: `cw-2026-08-17-013347` fits at ratio 4.08 with a fixed 20 ms
floor and 3.06 with a floor at half the fitted unit, against a hand-read 2.73
(HM-DEC-145).

### What this unit does and does not claim

**It claims the bandwidth is too wide and narrowing it improves the measured
signal-to-noise on every recording tried.** That is four for four and the mechanism
is elementary.

**It does not claim this makes Hamlet read the band.** The decode above came from a
script outside this repository with three settings tuned together. **Reproduce it
inside the tree before believing it.**

**An error recorded so it is not repeated.** Earlier tonight the poor copy was
attributed to AGC pumping, on a measurement showing the whole passband ducking 8 dB
in sympathy with the keying. The ducking is real. It is also present at -9.7 dB in
`cw-2026-08-18-003016`, which decodes well, **so it cannot be what separates good
copy from bad and the theory was withdrawn.** Do not build on it.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Reproduce every figure before relying on it.

- **Report mismatches; do not repair the instruction silently.**
- **The expected-red list in the last two orders was wrong.** The tree has been at
  five failing for some time: the three long-standing ones plus
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt` and
  `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`. **Report the count you find
  and treat anything above it as new.**
- `KeyingEnvelope` exists and computes the envelope. **Use it.**

---

## Rulings in force

**HM-DEC-120 - the decoder invents nothing on audio holding no signal.** This is
the property every previous attempt at short-mark exclusion broke, and it is the
one that must survive. **A change that raises character counts and also makes the
sensitivity sweep invent text is a failed change, however good the counts look.**

**HM-DEC-090 - a capture that cannot prove it is fresh is not written.**

**HM-DEC-091 - one source, and it says which.**

**HM-DEC-048 - nothing raises a confidence score.**

**HM-DEC-093 - no radio on the development machine.**

**HM-OPEN-053 - `ShortestVote` stays at 5.** A previous session established it is
not the mechanism behind the short runs. **Do not touch it.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` 13 -
the six fields 13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 - Find the detection bandwidth and report it

**Report before changing anything.**

Find every place the audio is reduced to an envelope for CW detection - the
decoder's own path and `KeyingEnvelope` both - and for each report:

1. The effective bandwidth in hertz, and how it is arrived at.
2. Whether it is fixed or varies with the fitted speed.
3. What noise bandwidth the tone tracker sees.

**If the decoder is already at 40 Hz or narrower, say so and stop.** The premise of
this unit would then be wrong and everything after it is aimed at nothing.

---

## Task 2 - Narrow it, and measure what that alone does

**Change the bandwidth and nothing else.** No threshold change, no exclusion rule,
no clock change. One variable.

- Report swing, short-run count and fitted ratio at the current bandwidth and at
  the narrower one, for every real recording in
  `tests\fixtures\cw\captured\` and `\unadjudicated\`.
- **Report the effect on the two recordings holding no keying.** If narrowing makes
  the decoder emit anything on those, that is HM-DEC-120 and the change fails.
- **The bandwidth should follow the fitted speed if it can.** CW occupies roughly
  four times the element rate; at 15 WPM that is about 40 Hz and at 30 WPM about 80.
  A fixed 40 Hz would penalise a fast sender. **If tying it to the clock is not
  straightforward, use a fixed value, say so, and say what it costs.**

---

## Task 3 - The threshold

**Gated on task 2 landing.** The midpoint between the 10th and 90th percentile is
the 50th. Sweep it and report what each value does to the character count and to
the invention rate on empty audio.

**Do not pick a value that raises copy at the cost of HM-DEC-120.** Report the
sweep and, if the best value is a judgement between two costs, **say so and stop** -
that is Tim's.

---

## Task 4 - Reproduce the decode. **THIS IS THE DROP CANDIDATE.**

A test that takes `cw-2026-08-18-003016.wav`, runs the decoder at the new
bandwidth, and records what it reads. `ETO 91B`, `VFB` and `STILL HVE MY` are real
anchors from the exchange.

**This recording has no adjudicated answer key** and must not be given one by a
session. **Assert only that the output contains those anchors**, or report what it
does contain and assert nothing.

**Drop it whole if the session is running long, and say so.**

---

## Parked - do not touch, do not raise

- **The AGC ducking.** Withdrawn above. Do not build on it.
- **`Refine` averaging the unit with key-up gaps.** Real, measured, and Tim's
  ruling - removing it turns thirteen tests red.
- **The element floor as a share of the unit, inside the decoder.** Tim's ruling.
  **Note for the report: if task 2 removes the short runs at source, that ask may
  no longer be worth answering. Say so if you find it.**
- **`RfGain` reading 100% with the knob at noon**, and stations reading 375 to 825
  Hz against a 600 Hz pitch. Real, unexplained, not this unit.
- **The lock being lost at 25 to 27 seconds of every 30 second capture.** Noticed
  twice, never chased. Not this unit.
- **HM-OPEN-052, HM-OPEN-054**, the five synthesized tests, rulings 096-133, the
  scorer, `CaptureAudioAsync` end to end.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Change one thing at a time and measure between.** *The decode above came from
  three settings tuned together and that is exactly why it is not evidence about
  any one of them.*
- **Do not break HM-DEC-120 to raise a character count.** *Every previous attempt
  at this failed that way.*
- **Do not adjudicate any capture or write an answer key.** *Tim has not listened
  to them.*
- **Do not touch `ShortestVote`.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings: **What Claude did**, **What Tim should expect**, **What we should
do next**, **What's blocking us** - the last carrying **Asks still outstanding**
per HM-DEC-139.

**Section 1 opens with the bandwidth the decoder actually uses.**

**Section 2 states in one sentence whether narrowing it, on its own, improved what
Hamlet reads from a real recording - and by how much.**

**Stop and report.**
