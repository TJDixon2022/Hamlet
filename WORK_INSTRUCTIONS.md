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

**The capture record describes the session, not the capture, and a work order was
written from it as though it described the capture.**

Tim operated on the 19th. Two presses, two stations he heard, two recordings, and
both rows read `nothing read`. A diagnosis was then produced from those rows —
that the speed tracker cannot lock on a sloppy human fist — and it is wrong. The
measurement below was taken from the recordings themselves, outside this
repository, and must be reproduced inside it before anything here is trusted.

### What the audio actually contains

Envelope by quadrature Goertzel at the detected tone, 100 Hz smoothing, sampled at
1 ms, threshold midway between the 10th and 90th percentile of the envelope:

| capture | key-down runs in 30 s | median key-down | envelope swing |
|---|---|---|---|
| `cw-2026-08-18-003016.wav` (decoded acceptably) | 231 | **48 ms** | 21.8 dB |
| `cw-2026-08-20-014854.wav` (read nothing) | 1,559 | **6 ms** | 14.1 dB |
| `cw-2026-08-20-014935.wav` (read nothing) | 1,329 | **5 ms** | 13.7 dB |

Fifteen hundred key-downs at a six-millisecond median is a threshold being crossed
by noise. A sweep from 300 to 1600 Hz in 25 Hz steps found no pitch in either
recording with keying structure. **The decoder read nothing because there was
nothing in the audio to read.** It behaved correctly.

### Why the record made it look like a decoder fault

The 18th's sidecar: `elements 752 seen, 233 resolved`, `characters 69 emitted`.
The 20th's sidecar: `elements 359837 seen, 233 resolved`, `characters 69 emitted`.

**Two different nights, two different bands, the same 69 and the same 233.** And
`audioSeen` on the 20th is 1,254,549,120 samples — the application had been running
seven hours. `ElementsSeen` is cumulative since the decoder started;
`CharactersEmitted` and `ElementsResolved` appear to be stuck at values from an
earlier run entirely.

`sinceLast` already computes a difference and is the only field on the sheet that
was about the capture — and it read `0 characters` on the second press, which was
the truth nobody read.

**This is HM-DEC-091 exactly**: a field presenting something in the shape of a
measurement that is not a measurement of what it sits beside. A roster row that
cannot be trusted is worse than a missing one, because the percentage gets computed
from it anyway.

### A third thing, found in the same diff

`cw-2026-08-18-003016.txt` reads `frequency 14028000 Hz` and `band 40 m`.
**14.028 MHz is 20 metres.** The roster now carries that band column into the
evening's evidence.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Line numbers and field names came from a copy
of the source read in a chat session. Check every claim.

- **Report mismatches; do not repair the instruction silently.**
- **Expected red, do not rediscover:** `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`. Last session
  reported 2,008 tests with those three failing. **Anything above three is new.**

Believed, to be checked: `CaptureNotes` writes `elements` and `characters` from
`report.ElementsSeen`, `ElementsResolved`, `CharactersEmitted`, `CharactersUnsure`
around `MainWindowViewModel.cs:3369`; `sinceLast` is computed at 3385 against
`_lastCaptureCharacters` and `_lastCaptureElements`.

---

## Rulings in force

**HM-DEC-091 — one source, and it says which.** The whole of this unit. A field
that cannot say what interval it covers must say so rather than print a number.

**HM-OPEN-053 — `CwGate.ShortestVote` stays at 5.** Fourth unit running. **Do not
touch `CwGate`, `CwSettledPass`, `CwToneSurvey` or `CwDecoder`.** *This unit fixes
the instrument that measures the decoder. Moving both at once is how the last four
days' evidence became unreadable.*

**HM-DEC-093 — no radio on this machine.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Find out what the counters actually count

**Report before changing anything.**

For each of `ElementsSeen`, `ElementsResolved`, `CharactersEmitted` and
`CharactersUnsure`: where is it incremented, what resets it, and over what interval
does it accumulate? **Explain how the 18th and the 20th both came to read 69 and
233.** If a counter is stuck rather than cumulative, that is a different defect
from the one this instruction assumes and it must be named.

If the counters turn out to be correct and the sidecar is reading them wrongly, say
so — the fix is then in `CaptureNotes` and not in the decoder.

---

## Task 2 — The record describes the capture

Every count in the sidecar and in the roster's `chars` column must cover **the
audio in the recording beside it**, not the session.

- Where a per-capture figure can be derived, derive it.
- **Where it cannot, the field says what interval it covers, in words, in the
  file.** `since the decoder started` is an honest field. A bare number is not.
- `sinceLast` stays. It was the only honest field on the sheet and it is now the
  model for the others.
- **Do not delete a field to avoid the problem.** A missing count and a wrong count
  are both worse than a labelled one.

---

## Task 3 — The band label

`14.028 MHz` must not read `40 m`. Find the lookup, report what it does, fix it,
and cover it with a test across the amateur HF bands including the boundaries.

**If the wrong label came from somewhere other than a band lookup, say so and stop
before changing anything** — a band that is right in one place and wrong in another
is a different fault.

---

## Task 4 — Record whether the radio was broadcasting

`SHACK_FACTS.md` states CI-V Transceive is **off**, measured — 5,499 inbound frames
in sixty-one seconds, zero broadcast. **Both of last night's sidecars read
`CivTransceive on`.**

**Do not change any setting, do not advise Tim to check it, and do not write to the
radio.** That fact is his and the contradiction is his to rule.

What this unit does is make the next capture carry the evidence: alongside the
setting as read, record **whether any unsolicited frame arrived during the
capture** — the measured behaviour, not the setting's name. `radioIsBroadcasting`
already exists as a concept in this project; find it and use it if it is there.

*This matters because a setting reported as `on` and a link observed to be silent
are different facts, and only the second one is evidence.*

---

## Task 5 — Prove it. **THIS IS THE DROP CANDIDATE.**

Reproduce the envelope histogram inside the repository: a test that takes
`tests\fixtures\cw\captured\cw-2026-08-18-004507.wav`, computes key-down run
lengths as described above, and asserts the distribution is bimodal with a unit
near 48 ms.

**Drop it whole if the session is running long, and say so.** Tasks 1 to 4 make
tomorrow's evidence trustworthy; this one makes the analysis repeatable.

---

## Parked — do not touch, do not raise

- **The speed-tracker rewrite** — deriving the unit from key-down durations, the
  2-and-5 gap tolerance, the `ETO 91B` fixture. The diagnosis was drawn from a
  recording that decoded acceptably and applied to two that contain no keying.
  **It may still be a real improvement. It is not this unit and it is not yet
  supported by the evidence offered for it.**
- **`cw-2026-08-18-003016.wav` as a fixture.** It is not in the repository and a
  session cannot fetch it.
- **HM-OPEN-052, HM-OPEN-053, HM-OPEN-054**, the five synthesized tests, the three
  expected failures, rulings 096–133, the scorer, and the non-hermetic
  `TheRosterIsOneFilePerEvening`.
- **`CaptureAudioAsync` end to end.** Still no seam, still declined.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch,
`main`; **do not push**; no interactive or destructive git; do not invent a ruling
id; do not touch coverage thresholds.

Unit-specific:

- **Do not change the decoder.** *It was right. It read nothing because there was
  nothing there.*
- **Do not change the roster's columns or their order, and do not touch `read`.**
  *Only what goes into `chars` changes.*
- **Do not write to the radio or change a rig setting.** *Task 4 observes.*
- **Do not "improve" the tone detector because it reported 800 Hz against a 600 Hz
  pitch.** *That number came from noise filling the filter; changing the detector
  on the strength of it repeats the error this unit exists to correct.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings: **What Claude did**, **What Tim should expect**, **What you should
see**, **What's blocking us** — the last carrying **Asks still outstanding** per
HM-DEC-139. **The queue grows by one**: whether `SHACK_FACTS.md` still holds that
CI-V Transceive is off, given both of last night's captures reported it on.

**Section 1 opens with the answer to task 1** — how two nights came to report the
same 69 and 233 — because everything else in this unit depends on it.

**Stop and report.**
