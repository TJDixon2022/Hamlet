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

**Tim can hear stations Hamlet cannot, and he finds out the next morning.**

On the 19th he heard two stations, pressed twice, and both rows read `nothing
read`. Measurement of the recordings — three independent ways, one of them now
inside this repository — says the audio contains **no keyed signal at any pitch**.
The decoder was correct. Something between the antenna and the WAV is losing the
station, and four theories for what have each been killed by measurement: a sloppy
fist, the wrong capture device, no audio arriving at all, and the signal sitting on
the filter skirt. The last died on the 20th: `cw-2026-08-20-014854.wav` has its
dominant narrow content at 608 Hz against a 600 Hz pitch, dead centre.

**Nobody knows what the fault is.** This unit does not guess at a fifth theory.
It builds the instrument that lets Tim find it himself, at the rig, in minutes —
by telling him *while he is sitting there* whether Hamlet can hear keying in what
he is listening to. He turns the AF gain, changes the filter, retunes, switches the
preamp, and watches the indicator answer.

**This is the same move as the case roster and it is the reason that worked.** The
roster made the denominator visible and found this fault in one evening. This makes
the audio visible.

---

## The measurement, and it is already validated on his own recordings

Taken with `KeyingEnvelope`, built last session for exactly this and sharing no
code with the decoder (§12.5): quadrature mixdown at a candidate tone, 10 ms
boxcar, 1 ms sampling, threshold midway in amplitude between the 10th and 90th
percentile of the envelope. Key-down run lengths, then the **median**.

| source | median key-down | runs / 30 s | envelope swing |
|---|---|---|---|
| four captures that decoded (both bands, two nights) | 44–57 ms | 172–234 | 21.8–28.7 dB |
| the two that read nothing | 7 ms | 1,393–1,563 | 13.7–15.6 dB |
| pure noise, last session's own control | 2 ms | 3,025 | 13.4 dB |

**No overlap anywhere, and the gap is a factor of six.**

It also survives being cut into live-sized windows, which is what makes an
indicator possible. Sweeping 400–1200 Hz in 25 Hz steps, taking the best tone in
each window:

| window | four that decoded, median of windows | two that did not |
|---|---|---|
| 10 s | 44.0, 48.5, 49.0, 57.0 ms | 7.0, 7.5 ms |
| 6 s | 44.5, 48.0, 48.5, 56.8 ms | 7.0, 7.0 ms |
| 4 s | 45.0, 48.5, 49.0, 57.0 ms | 7.0, 8.0 ms |

At six seconds every window of every keyed recording lands between 44 and 58 ms
and every window of the unkeyed ones lands at 7. At four seconds two windows of
keyed audio dropped to 5 and 8 ms — silence between overs, which is honest and is
why the indicator must hold rather than flicker.

**These numbers were measured outside this repository, on six recordings, four of
which are now in `tests\fixtures\cw\captured\unadjudicated\`. Reproduce them
inside before relying on them.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`. Last session
  reported 2,026 tests with those three failing. **Anything above three is new.**
- Believed present: `KeyingEnvelope` and `KeyingIsInTheAudioTests` from last
  session; `AudioTap.Snapshot()`; `CwCounterTrail`; the case roster and its
  `I hear a station` button.

---

## Rulings in force

**§9.5.1 — one branch, `main`, and every session commits *and pushes* to `main`.**
Four previous orders in this series misquoted this section as a prohibition on
pushing. **That was an error in the orders, not a ruling.** Commit and push.

**HM-OPEN-053 — `CwGate.ShortestVote` stays at 5.** Fifth unit running. **Do not
touch `CwGate`, `CwSettledPass`, `CwToneSurvey` or `CwDecoder`.** *The indicator
must be independent of the decoder or it cannot tell Tim anything the decoder was
not already telling him.*

**HM-DEC-091 — one source, and it says which.** The indicator reports what it
measured. It does not report a verdict it cannot support.

**HM-DEC-093 — no radio on this machine.** Everything verifiable without one.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Reproduce the table before building on it

Run `KeyingEnvelope` over the four unadjudicated captures and the two from the
20th, in 6-second windows, sweeping 400–1200 Hz in 25 Hz steps, and **report the
median-of-windows for each**.

**If the separation above does not reproduce, stop and report.** Everything below
depends on it and a fifth dead theory is cheaper found here than at the rig.

---

## Task 2 — A live keying meter

A component that runs the same measurement continuously on the audio already
flowing through `AudioTap`, independent of the decoder.

- **Six-second rolling window, recomputed about once a second.** Both numbers come
  from task 1's table: six seconds is the shortest window where every keyed
  recording stayed above 44 ms, and one second is fast enough to feel like a
  response when Tim turns a knob.
- Report, every update: the **best candidate tone**, the **median key-down in
  milliseconds**, the **envelope swing in dB**, and the **run count**.
- **It must run whether or not the decoder has latched a tone.** That is the whole
  point — the case it exists for is the one where the decoder has nothing.
- **Do not reuse the decoder's tone.** Sweep independently. The decoder chose
  800 Hz on a recording whose content was at 608.
- Cost matters: this runs every second beside a live decoder. Say in the report
  what one update costs.

---

## Task 3 — Put it on the screen, and make it hold

On the CW terminal, near the **I hear a station** button, in three states:

- **keying** — median key-down in the CW range
- **no keying** — median far below it
- **listening** — not enough evidence yet, or between overs

**It must hold rather than flicker.** At four seconds, two windows of genuinely
keyed audio read 5 and 8 ms because the operator had stopped sending. A meter that
drops to *no keying* between overs is worse than none, because Tim will stop
trusting it in the first ten minutes and then it cannot help him.

Hold the last confident state through quiet, and only fall back to **no keying**
after it has been unambiguous for several consecutive windows. **Say in the report
exactly what rule you chose and what it does across a gap between overs.**

Show the measured numbers beside the word — tone, median, swing. *Tim is going to
use this to chase a fault by turning knobs; the number moving is worth more than
the word changing.*

**Thresholds are provisional and must be named as such in the report.** They come
from six recordings on two nights. Put them somewhere a later session can change
in one place.

---

## Task 4 — The roster carries the verdict

The sidecar and the roster gain what the meter said at the moment of the press —
the state and the three numbers.

*This is what makes tonight worth something even if the fault does not fall out.
Tomorrow every row says whether Hamlet could hear keying, which no row has ever
said, and the ones where he heard a station and the meter said no are the evidence
that finds this.*

Columns and order otherwise unchanged. `read` stays last and stays empty.

---

## Task 5 — Prove it without a radio. **DROP CANDIDATE.**

Drive the meter from the captures through `BufferedAudioSource` and assert it
reaches **keying** on the four that decoded and **no keying** on the two from the
20th, and that a gap between overs does not knock it out of **keying**.

**Drop it whole if the session runs long and say so.** Tasks 1 to 4 are what he
needs at the rig tonight.

---

## Parked — do not touch, do not raise

- **The speed-tracker rewrite.** Still parked, still unsupported by evidence.
- **Why the station is missing from the audio.** Four theories are dead. **Do not
  offer a fifth in the report.** This unit is the instrument for finding out.
- **HM-OPEN-052, HM-OPEN-053, HM-OPEN-054**, the five synthesized tests, the three
  expected failures, rulings 096–133, the scorer, `CaptureAudioAsync` end to end,
  and the non-hermetic `TheRosterIsOneFilePerEvening`.
- **Adjudicating the unadjudicated captures.** Tim's ear, not a session's.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

Unit-specific:

- **Do not change the decoder, the gate, the tone survey or the settled pass.**
  *An indicator that shares the decoder's opinion cannot contradict it, and
  contradicting it is the job.*
- **Do not make the meter drive anything.** It does not retune, does not switch the
  decoder on or off, does not gate the capture. *It reports.*
- **Do not write to the radio.**
- **Do not tune the thresholds to make a recording pass.** *Six recordings is a
  small sample and the gap is a factor of six; if something does not separate, that
  is a finding.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139.

**Section 1 opens with task 1's table**, reproduced in the repository, beside the
one above.

**Section 2 says what the meter will show him when he tunes across a band with no
station on it, and what it will show on a station he can hear.** He is going to
be sitting in front of it tonight with no other instrument.

**Stop and report.**
