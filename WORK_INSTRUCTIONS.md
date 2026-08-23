# WORK_INSTRUCTIONS.md

**Phase 10 — unit 12. Build 1.10.11 → 1.10.12.**

> **How moving the unit moves the phase:** the phase is 80% correct translation on
> a single clear CW signal; the gap-boundary mechanism built last unit repairs the
> worst clear signal in the corpus but is switched off because a single twelve-
> second window can show structure the recording does not, and this unit makes it
> safe to switch on by requiring that structure to survive more evidence than one
> window holds.

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwUnitEstimator.cs
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

**The mechanism is built, measured and off, and turning it on costs two of the
three adjudicated readings.**

Last unit built `CwUnitEstimator.MeasureGaps`: 3-means on `log(gap)`, boundaries at
the geometric means of adjacent centroids, held inside `[1.3u, 2.6u]` and
`[3.5u, 6.5u]` as a clip. **And a trough test, which is the part that makes it
mean anything** — a boundary is accepted only if fewer gaps stand near it than near
either cluster it divides, counted in equal windows on the logarithm.
**Parameter-free.**

Measured on whole recordings it **refuses eight captures in nine and accepts
generated Morse.** The one it accepts is `cw-2026-08-18-004507`, the file the
coupling is breaking.

**Wired in, it repairs that file:**

| | |
|---|---|
| off | `EE AC H STA TI O N HAN D L I NG ET H IS M E S S A G E PE` |
| on | `EE ACH STATION HANDLING ET HIS MESSAGE PE` |

**And it costs `VA3VRR` and breaks `AA4MP/4 QNIK`.** The previous session named the
reason: **the decoder reads twelve seconds at a time, and a window can show a
trough the whole recording does not.**

**That is an evidence problem, not a threshold problem.** The estimator already
runs on every read.

### Ruled by Tim

**The trough must survive across several consecutive reads before the measured gap
lengths are used.**

*Rejected: shipping it on a single window's trough.* Measured — it costs two of the
three adjudicated readings, which are the only ground truth in this repository.

*Rejected: leaving it off.* It repairs the worst clear signal in the corpus and the
phase goal is a clear signal.

---

## Task 1 — Require the structure to persist

**The estimator already runs on every read. Require the trough to hold across
several of them before the measured lengths are handed to the decoder.**

- **How many reads is yours to choose and to justify.** A read is half a second, so
  a handful of reads is a few seconds of evidence, and the recording is thirty.
  **Say what you chose and what evidence you have for it.**
- **It is a mechanism, not a threshold.** What must persist is the *structure* —
  that a trough exists between the same two clusters — **not that the boundary lands
  at the same millisecond.** A sender's gaps wander; the empty region between them
  is what does not.
- **When the structure is not established, the decoder gets one, three and seven
  units as it does today.** That is the current behaviour and it must remain the
  fallback.
- **When it is established and then stops holding, the estimator returns to the
  fallback.** Say how quickly, and why that is the right speed.

**Report, per capture:** how many reads found a trough, how many consecutive, and
whether the structure was ever established.

---

## Task 2 — Measure it against the three adjudicated readings

**These are the only ground truth in this repository and they are the acceptance
test.** All three are currently right, which was not true a day ago.

| what | where | must read |
|---|---|---|
| `N4L` (HM-DEC-144) | `cw-2026-08-17-134712` | `N4L` |
| `VA3VRR` (HM-DEC-145) | `cw-2026-08-17-013347` | `VA3VRR` |
| `AA4MP/4 QNIK` (HM-DEC-126) | `cw-2026-08-18-003758` | `AA4MP/4 QNIK` |

**All three must still be right.** If any is not, **stop and report at this task.**

And `cw-2026-08-18-004507`, quoted verbatim, against:

    EE AC H STA TI O N HAN D L I NG ET H IS M E S S A G E PE

**It should read at least as well, and the target is `ACH STATION HANDLING ET HIS
MESSAGE PE`.**

**If the persistence rule keeps the three callsigns but no longer repairs `004507`,
that is a real outcome and it is reported as one** — not tuned around. Say how many
consecutive reads would have been needed to repair it and what that would have cost
the other three.

---

## Task 3 — The clip ranges are constants and one of them decided an answer

`[1.3u, 2.6u]` and `[3.5u, 6.5u]` came from **one station on one machine keyer.**

On `004507`, the one capture that used them, **the word boundary was clipped hard,
landing at exactly 6.50u — the clip decided it, not the measurement.**

- **Report, per capture, whether either boundary was clipped**, and if so which and
  by how much.
- **A boundary that is always clipped is not a measurement.** If the word clip is
  carrying every case, say so — the previous session's own note is that word gaps
  are too rare in thirty seconds for that cluster to be trustworthy and the clip
  was meant to carry that one.
- **Do not widen a clip to stop it binding.** *That is tuning to the corpus.* If a
  clip is wrong, **say what it should be and what measurement would establish it**,
  and leave it.

---

## Task 4 — Fine tone tracking

**Independent of everything above and untouched for two units.**

The carrier on the W1AW captures measures **499.9 Hz, stable to ±0.1 Hz over four
minutes.** A 25 Hz search grid cannot express that. On the 01:41 capture the signal
sits at **608 Hz**, the grid offered 600 and 625, **and the sweep took 625 — 17 Hz
off, measuring the signal 4 dB weaker than 600 would have.**

**A full-length FFT peak costs one transform and resolves to a fraction of a
hertz.** Track it continuously; do not quantise to 25 Hz afterwards.

**This also repairs the keying sweep**, which is the independent witness — **its own
wording does not change.**

**Report the tone per capture**, and what the keying sweep picks on the 01:41
capture against a true 608 Hz.

**If task 1 does not land, do this anyway.** It depends on nothing above it.

---

## Acceptance

**The three adjudicated readings, all right.** That is the gate; nothing else
matters if one of them breaks.

Then, every real capture:

| | target |
|---|---|
| per cent of letters that are E | single figures |
| per cent of letters that are T | single figures |
| per cent single-character words | **zero** |

**Report those three on every capture, before and after.** **A character count is
not the measure and must not be reported in its place** — more single-element
letters is more characters and worse reading.

**And HM-DEC-120**: both recordings holding no keying silent, and the sensitivity
sweep. **Nothing invented from 18 dB down to 5.** That is further ahead than it has
ever been. **Do not give any of it back.**

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **The analysis this work descends from describes a decoder Hamlet no longer is,
  in places.** Two sessions have now found a change with no site as written and
  built the mechanism where it can exist — the Schmitt trigger inside the estimator
  rather than on a decode-path threshold, and the gap lengths handed to the Viterbi
  rather than applied to a boundary. **Both were right. Expect the same and say
  where you built it.**
- **Record the failing-test set exactly before and after and name every
  difference.** It is 28. `TheFollowedSentenceReachesTheScreenTests` passes when
  its class runs alone — the flake filed as `HM-OPEN-055`.
- **The seven W1AW captures, `2026-08-22.jsonl` and both analysis documents are not
  in the tree.** Every figure quoted from them goes unchecked. **Say so once and
  measure on what is here.**
- **The overfitting guard.** Everything descends from one station, one speed, one
  pitch, one machine keyer. **State for each change whether it was a mechanism
  found or a parameter tuned.**

---

## Rulings in force

- **HM-DEC-120.** Nothing emitted on audio holding no signal.
- **HM-DEC-095.** Do not loosen its separation limit, confirmation rule or
  plausibility bounds.
- **HM-DEC-048** and **HM-DEC-108**, on confidence.
- **HM-DEC-091.** The captures are permanent read-only fixtures. **Nothing edits a
  WAV or a sidecar.**
- **HM-DEC-126**, **HM-DEC-144**, **HM-DEC-145** — the three adjudicated readings.
  **The acceptance test.**
- **HM-DEC-150**, the version scheme, which governs over `CLAUDE_CODE.md` §4.11.
  They agree.
- **HM-DEC-103**, the same fragmenting signature in the reference decoder with a
  hardcoded window suspected underneath. **Named so it is not rediscovered.**
- **HM-DEC-009**, **§0.0**, **§0.0.1**.
- **HM-DEC-093** and `SHACK_FACTS.md` — no radio on the development machine.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md`
**§13**, which names that file's fields — `STATE`, `PHASE`, `BALL`, `NEXT_PASTE`,
`UPDATED`, `NOTE`. **`PHASE` is 10.** `UPDATED` from the clock; `NOTE` says what is
moving inside the task. Also every ten minutes while a task runs.

---

## The build number

**Read `Directory.Build.props`.** It should say **1.10.11**. **Bump the patch to
1.10.12 and report the move.** If it does not say 1.10.11, **say what it says**
rather than assuming.

---

## Parked — do not touch, do not raise

- **Why the mark and gap classifiers disagree about the unit.** A forced-unit sweep
  showed no single unit produces both E-dominance and fragmentation, yet Hamlet
  shows both. **Structural, its own unit, with HM-DEC-103 alongside.**
- **The narrow decoder-side filter**, ENBW 50–100 Hz around the tracked tone. **The
  crowded passband is a different problem** — four or five stations in 500 Hz, the
  target leading by 1–11 dB and losing outright in three of fifteen windows.
  Separate work, separate tests.
- **HM-OPEN-056**, `tonePeak`/`snrDb` as a held peak of an instantaneous ratio,
  reporting 54.7 dB on a capture holding no station.
- The window clear; pitch distance as a sender-change test; the survey ranking by
  loudness; the advice line; the sidecar contradiction; `FollowSpeed`;
  `HM-OPEN-051`; `HM-OPEN-055`.
- **HM-OPEN-012, HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098,
  HM-OPEN-033, HM-OPEN-007.**

---

## Asks still outstanding

Carried inbound per HM-DEC-139, verbatim until ruled. **Verify against
`OPEN_ISSUES.md` and report anything here that is closed, or open and missing.**

- **No capture has an answer key, so the phase's own number cannot be stated.**
  Three adjudicated fragments exist; ARLP034 was never published.
- Why the mark and gap classifiers disagree about the unit — structural.
- The narrow decoder-side filter for a crowded passband.
- HM-OPEN-056, the held-peak SNR.
- The seven W1AW captures and `2026-08-22.jsonl` are not in the tree.
- Whether a sender change can be decided by pitch distance at all — measured dead.
- Whether the window clear comes back on.
- The advice line asserting a cause the app can disprove.
- The sidecar asserting two incompatible things about one span.
- Whether the sidecar's `text` should include the leading edge.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0; the keying meter's provisional thresholds.
- `FollowSpeed` has no supplier.
- The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.

**Whether a window may take its gap lengths from its own gaps leaves this queue** —
ruled: only when the structure survives several reads.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

- **Do not break an adjudicated reading to repair a file.** *Three fragments are
  the only ground truth here.*
- **Do not widen a clip to stop it binding.**
- **Do not require the boundary to land at the same millisecond.** *A sender's gaps
  wander; the empty region does not.*
- **Do not report a character count in place of the signature table.**
- **Do not trade HM-DEC-120, and do not loosen HM-DEC-095.**
- **Do not chase the structural fault or the narrow filter here.** *Both parked.*
- **Do not claim a clear signal reads perfectly.** *The independently measured
  pipeline still gives `NACKETY` for `PACKET`.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **`CLAUDE_CODE.md` §8
names FIVE sections** — **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us**, **Where the phase stands**. **`CLAUDE.md`
§12.2 names four.** *Report the collision.* Under §0 the project's file wins on the
four it names; **section 5 is additive and is written.**

**Section 1 opens with the three adjudicated readings, by name, right or wrong.**

**Section 2 quotes `004507` before and after, and every other real capture**, and
says in one sentence whether the operator will see more correct characters on a
clear signal.

**Section 5, "Where the phase stands", is measurement only** — the phase number
now, what it was before this unit, and the build number this unit produced. **No
proposal and no what-next.**

**Stop and report.**
