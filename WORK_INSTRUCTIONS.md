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

**The detection filter widens on the strength of a speed its own width helped get
wrong.** Chatter shortens the fitted dit; a short dit reads as a fast fist; a fast
fist selects the 20 ms window; that window is 75 Hz instead of 30; more noise
crosses the gate; more chatter. Eight of nine recordings sit at 75 Hz on senders
working near fourteen words a minute.

Last session held the detection window at 50 ms in a harness, changing nothing
else:

| recording | today | window held at 50 ms |
|---|---|---|
| `cw-2026-08-18-004507` | 25 chars, fragments | **32 — `NET  EAC5 STATION HANDLING HIS ESSAGEP`** |
| `cw-2026-08-18-003016` | 38 chars | **43 — `STILL HVE MY E TO 91B ETT USTFB TUBELI`** |
| `cw-2026-08-20-014854` (no keying) | 0 | **0** |
| `cw-2026-08-20-014935` (no keying) | 0 | **0** |

**Both recordings holding no keying stay silent at 50 ms.** At 40 ms one of them
emits four characters. Fifty reads more and invents nothing; forty is one notch
from breaking §0.0.

**It cannot be had by changing a constant, because the coarse survey shares the
analysis window.** A longer taper is a narrower search, so narrowing detection also
narrows acquisition and every station-finding test goes red.

**Ruled by Tim: the survey and the detection filter may stop sharing a window.**
They ask different questions. The survey searches frequency; the gate measures
time. There is no reason they must share a taper.

*Rejected: fixing the speed fit first.* Real, measured, and still worth doing — but
the loop is self-reinforcing, so a corrected fit can be dragged back round by a
noisy patch of band. Remove the feedback path first.

*Rejected: doing both in one unit.*

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Every figure above came from last session's
harness. Reproduce before relying on it.

- **Report mismatches; do not repair the instruction silently.**
- **Expected red: five.** `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`,
  `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`. **Anything above five is
  new.**
- `WhatBandwidthTheDecoderListensThroughTests` holds last session's measurements.
  **Read it first. It is the specification for this unit.**

---

## Rulings in force

**HM-DEC-120 — the decoder invents nothing on audio holding no signal.** Three
attempts at narrowing have failed here. **A change that raises character counts and
also makes the sensitivity sweep invent text is a failed change.** This is the gate
on the whole unit.

**HM-DEC-048 — nothing raises a confidence score.**

**HM-DEC-091 — one source, and it says which.**

**HM-DEC-093 — no radio on the development machine.**

**HM-OPEN-053 — `ShortestVote` stays at 5.** Established as not the mechanism.
**Do not touch it.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` 13 —
the six fields 13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Report the seam

Where do the survey and the gate share a window today? Name the type, the field and
the call sites.

**If they are already separable without a design change, say so and go straight to
task 2.** If separating them touches something this instruction has not anticipated,
**report and stop.**

---

## Task 2 — Separate them

The survey keeps the window it has. The gate gets its own.

- **Station-finding must not regress.** The survey's behaviour is unchanged by
  definition; prove it with the existing tests rather than asserting it.
- The gate's window may then be chosen on its own merits.
- **One variable at a time, measured between.**

---

## Task 3 — Choose the gate's window on evidence

Sweep it. For every real recording in `tests\fixtures\cw\captured\` and
`\unadjudicated\`, report characters read. For both recordings holding no keying,
report characters invented.

- **Any width that emits anything on empty audio is disqualified, whatever it reads
  elsewhere.**
- Report the margin: which widths are silent, which are not, and by how much.
- **If the best width is a judgement between two costs, say so and stop.** That is
  Tim's.

---

## Task 4 — Does the loop still close? **THIS IS THE DROP CANDIDATE.**

With the windows separated, report the fitted speed and the selected width per
recording, as last session's table did. **Eight of nine were at 75 Hz on
fourteen-words-a-minute senders. Say what it is now.**

**Drop it whole if the session is running long, and say so.**

---

## Parked — do not touch, do not raise

- **`Refine` averaging the unit with key-up gaps.** Tim's ruling. Thirteen red
  tests.
- **The element floor as a share of the unit.** Tim's ruling.
- **The AGC ducking.** Withdrawn — present in a recording that decodes well.
- **`RfGain` reading 100% with the knob at noon**; stations reading 375 to 825 Hz
  against a 600 Hz pitch. Real, not this unit.
- **The lock lost at 25 to 27 seconds of every 30 second capture.** Noticed twice,
  never chased.
- **HM-OPEN-052, HM-OPEN-054**, the five synthesized tests, rulings 096-133, the
  scorer, `CaptureAudioAsync` end to end.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not break HM-DEC-120 to raise a character count.** *Three attempts have
  already failed that way.*
- **Do not change the survey's behaviour.** *Station-finding is the one thing
  working.*
- **Do not adjudicate any capture or write an answer key.** *Tim has not listened
  to them.*
- **Do not touch `ShortestVote`, `Refine`, or the element floor.** *All three are
  Tim's, all three are outstanding.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings: **What Claude did**, **What Tim should expect**, **What we should
do next**, **What's blocking us** — the last carrying **Asks still outstanding**
per HM-DEC-139.

**Section 2 opens with what the bulletin recording reads now, verbatim.**

**Stop and report.**
