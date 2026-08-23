# WORK_INSTRUCTIONS.md

**Phase 10 — unit 13. Build 1.10.12 → 1.10.13.**

> **How moving the unit moves the phase:** the phase is 80% correct translation on
> a single clear CW signal, and three units of work on the unit estimator and the
> gap boundaries have moved the signature not at all — E and one-letter words sit
> where they sat; this unit chases the structural fault that a forced-unit sweep
> already proved cannot be a unit problem, and adds the first capture in this
> repository whose text is known, so the phase can finally be scored rather than
> estimated.

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
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

**Three units have moved the reading almost not at all.**

At 1.10.12 the signature reads: E at 39, 50, 76, 20, 10, 16 and 43 per cent
across the captures, against a target of single figures. One-letter words at 60,
33, 84, 48, 8, 53 and 65 per cent, against a target of zero. **Two captures
improved and five did not move.**

**The reason was written down before any of that work ran and was parked three
times.** A forced-unit sweep across 8–44 WPM measured the two failure modes
separately:

| forced WPM | % letters = E | % letters = T | % single-character words |
|---|---|---|---|
| 10 | 23 | 0 | **0** |
| 14 | 15 | 4 | **0** |
| 18 | 11 | 9 | **0** |
| 22 | 11 | 9 | **0** |
| 26 | 15 | 9 | 67 |
| 30 | 13 | **67** | 33 |
| 32 | 8 | **76** | 29 |
| 44 | 10 | **80** | 23 |

**No single unit produces both.** Where letters fragment into single-character
words the letters are **T**, not E — a boundary high enough to shred the gaps is
also high enough to call every mark a dah. Where E dominates, nothing fragments.

**Hamlet shows both at once.** Therefore its mark classifier and its gap
classifier are not working from the same unit, **or the gap classifier is not
unit-derived at all.**

**That is a structural fault and no amount of work on the unit estimator can
reach it.** Units 11 and 12 were both unit-estimator work. The measurement says
they could not have fixed this, and they did not.

---

## Task 1 — Find the disagreement

**Report before changing anything. This is a diagnosis and it is the point of the
unit.**

1. **Log the unit actually applied to mark scoring and the unit actually applied
   to gap scoring, on every decode window, on every real capture.** Not the unit
   the estimator produced — **the number each classifier used.**
2. **Are they the same number?** If not, **where do they diverge**, and is the
   difference constant, drifting, or per-window?
3. **Is the gap classifier unit-derived at all?** If some part of it comes from a
   constant, a window length, a hop count, or anything not scaled by the unit,
   **name it, name the file and line, and say what it is scaled by instead.**
4. **Force a single shared unit through both classifiers and re-run every
   capture.** **If the dual failure disappears, the fault is isolated** — report
   that and go to task 2 rather than building anything.

**HM-DEC-103 records the same signature in the reference decoder** — correct clock
fit, then every element returned as its own character, **with a hardcoded window
suspected underneath.** `tools\reference-decoder\reference_decoder.py` is in the
tree. **Check both together.** If the same constant is in both, Hamlet inherited
it at the port and that is the answer.

**If the two classifiers do use the same unit, say so and stop.** The inference
above would then be wrong, and knowing that is worth more than a change built on
it.

---

## Task 2 — Fix what task 1 found

**Gated on task 1. Build what it found, not what this order guessed.**

- **One unit, both classifiers.** If a constant is standing in for a unit-derived
  quantity, derive it.
- **Report the signature on every capture, before and after.** E, T, and
  single-character words. **The target is the 10–22 WPM rows of the table above:
  single figures for E and T, zero single-character words.**
- **If it fixes one symptom and not the other, say which and say what that
  implies.** That is still a finding.

---

## Task 3 — The known-text fixture

**This repository has never had an answer key. `cw-2026-08-23-001520` is one.**

- 8000 Hz sample rate, tone **600.000 Hz**, exactly **12.00 WPM**
- 54 % of samples exactly zero, 94 distinct sample values, **135 dB above its
  numerical floor**
- Known text: **`CQ CQ DE KC3QIS KC3QIS K`**

**It is the only capture where `0 unsure` is the correct answer, so any other
output is unambiguously a regression.** Bring it in as a fixture with the text
recorded as known, and assert against it.

**And it exposes a real defect.** It arrived at **8000 Hz where every rig capture
is 48000.** Any hardcoded sample count changes the effective analysis window by
six times between sources, silently.

- **Grep for sample-count constants across the decode chain, the estimator, the
  tracker and the keying meter.**
- **Express window, hop and smoothing in milliseconds and derive samples from the
  stream's rate.**
- **Report every constant you found and what it was scaled by.** A window that was
  right at 48 kHz and six times wrong at 8 kHz may be wrong in other ways nobody
  has looked for.

**If that file is not in the tree, say so and do the grep anyway** — the
sample-rate defect is real whether or not the fixture is available.

---

## Task 4 — A narrow decoder-side filter

**This is the limiting factor on a real band, and it is not the decoder.**

Hamlet accepts the radio's 500 Hz and treats all of it as one signal. On
2026-08-23 00:19–00:20 UTC on 7.039 MHz — a contest pileup — **two stations at
449 Hz and 520 Hz, 71 Hz apart, traded dominance every three seconds, with the
margin between them falling to 0.6 dB.** The independent analysis chain fails
there too: dit:dah of 2.5 and 2.2 against 2.9–3.1 for a clean single station.

**Give the decoder its own filter, ENBW 50–100 Hz, locked to the tracked tone.**
100 Hz around 520 excludes 449 entirely.

- **Scope it with its own tests, and do not judge it by the W1AW files or by the
  single-station captures.** Those hold one dominant signal and will not exercise
  it. **Use the 01:41, 01:43 and 00:19 captures.**
- **Report what it does to the single-station captures anyway** — it must not cost
  them anything.
- **Report what it does to the sensitivity sweep.** A narrower filter admits less
  noise and should help; **if it does not, say so.**

**Note recorded and not acted on:** a contest band holds five or more simultaneous
stations and a single-tone tracker cannot serve it. **N parallel narrow decoders
is a design decision worth taking before more code goes into the single-channel
path.** That is Tim's and it is not this unit.

---

## Acceptance

**The three adjudicated readings, all right.** That is the gate; if one breaks,
stop and report.

| what | where | must read |
|---|---|---|
| `N4L` (HM-DEC-144) | `cw-2026-08-17-134712` | `N4L` |
| `VA3VRR` (HM-DEC-145) | `cw-2026-08-17-013347` | `VA3VRR` |
| `AA4MP/4 QNIK` (HM-DEC-126) | `cw-2026-08-18-003758` | `AA4MP/4 QNIK` |

**And `CQ CQ DE KC3QIS KC3QIS K` on the known-text fixture, with `0 unsure`**, if
that file is in the tree.

Then the signature on every capture, before and after:

| | target |
|---|---|
| per cent of letters that are E | single figures |
| per cent of letters that are T | single figures |
| per cent single-character words | **zero** |

**A character count is not the measure and must not be reported in its place.**

**And HM-DEC-120**: both recordings holding no keying silent, and the sensitivity
sweep. **Nothing invented from 18 dB down to 3.** That is where 1.10.12 left it.
**Do not give any of it back.**

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **Phases 1, 2 and 3 of the brief this order descends from are already shipped**
  — the Schmitt trigger at ±6 dB with the 5–8 dB plateau pinned, the
  median-of-mark-and-gap unit, and the 3-means gap boundaries with a trough test
  and a twelve-read persistence rule. **Do not rebuild them.** If any is not in
  the tree as described, **say so.**
- **The analysis this work descends from describes a decoder Hamlet no longer is,
  in places.** Three sessions have now found a change with no site as written and
  built the mechanism where it can exist. **Expect it and say where you built it.**
- **Record the failing-test set exactly before and after and name every
  difference.** It is 28, one of which is the app flake
  `TheFollowedSentenceReachesTheScreenTests`, which passes when its class runs
  alone (`HM-OPEN-055`).
- **The seven W1AW captures, `2026-08-22.jsonl` and both analysis documents are
  not in the tree.** Every figure quoted from them goes unchecked. **Say so once
  and measure on what is here.**
- **The overfitting guard.** **State for each change whether it was a mechanism
  found or a parameter turned.**

---

## Rulings in force

- **HM-DEC-120.** Nothing emitted on audio holding no signal.
- **HM-DEC-095.** **Do not loosen its separation limit, confirmation rule or
  plausibility bounds** to make anything here pass.
- **HM-DEC-103.** The same fragmenting signature in the reference decoder, with a
  hardcoded window suspected. **Task 1 checks it.**
- **HM-DEC-048** and **HM-DEC-108**, on confidence.
- **HM-DEC-091.** Captures are permanent read-only fixtures. **Nothing edits a WAV
  or a sidecar.**
- **HM-DEC-126**, **HM-DEC-144**, **HM-DEC-145** — the adjudicated readings, the
  acceptance gate.
- **HM-DEC-150**, the version scheme, governing over `CLAUDE_CODE.md` §4.11.
- **HM-DEC-009**, **§0.0**, **§0.0.1**.
- **HM-DEC-093** and `SHACK_FACTS.md` — no radio on the development machine.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md`
**§13**, which names that file's fields — `STATE`, `PHASE`, `BALL`, `NEXT_PASTE`,
`UPDATED`, `NOTE`. **`PHASE` is 10.** `UPDATED` from the clock; `NOTE` says what
is moving inside the task. Also every ten minutes while a task runs.

**`PROJECT_STATUS.md` arrives with this order.** It has gone missing from the tree
before — a `git add -A` once recorded it as deleted. **If it is absent, this copy
is the one to use.**

---

## The build number

**Read `Directory.Build.props`.** It should say **1.10.12**. **Bump the patch to
1.10.13 and report the move.** If it does not say 1.10.12, **say what it says**
rather than assuming.

---

## Parked — do not touch, do not raise

- **The broken instruments.** The keying sweep contradicting the decoder beside it
  on six consecutive captures; `tonePeak`/`snrDb` at 62–78 dB where honest
  measurement is ~26; a fit score of 1.5×10¹⁰ on a noiseless file; the input meter
  reading "almost nothing arriving" on the loudest file in the corpus. **All real,
  all §0.0.1, all their own unit.**
- **Fine tone tracking.** Measured last unit: the tracker is within 1.5 Hz on every
  capture holding a clear station, against a 60 Hz decoder bandwidth. **The premise
  did not hold. Do not rebuild it.**
- **The word-gap clip carrying every real case.** Needs a capture whose word
  boundaries are known.
- **N parallel narrow decoders.** Tim's, noted in task 4.
- **HM-OPEN-056**, the held-peak SNR. The window clear; pitch distance as a
  sender-change test; the survey ranking by loudness; the advice line; the sidecar
  contradiction; `FollowSpeed`; `HM-OPEN-051`; `HM-OPEN-055`.
- **HM-OPEN-012, HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098,
  HM-OPEN-033, HM-OPEN-007.**

---

## Asks still outstanding

Carried inbound per HM-DEC-139, verbatim until ruled. **Verify against
`OPEN_ISSUES.md` and report anything here that is closed, or open and missing.**

- The word-gap clip is carrying every real case.
- **No capture has an answer key** — **task 3 is the answer to this** if the file
  is in the tree.
- The narrow decoder-side filter for a crowded passband — **task 4.**
- Why the mark and gap classifiers disagree about the unit — **task 1.**
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

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch
and it is `main`, **and every session commits and pushes to it**; no interactive
or destructive git; do not invent a ruling id; do not touch coverage thresholds.

- **Do not rebuild the hysteresis, the unit estimator or the gap boundaries.**
  *All three shipped in units 11 and 12.*
- **Do not tune the unit to make the signature move.** *A forced-unit sweep already
  proved no unit produces both symptoms. If the answer looks like a better unit,
  task 1 has gone wrong.*
- **Do not break an adjudicated reading to fix anything.** *Four fragments are the
  only ground truth here.*
- **Do not judge the narrow filter by the single-station captures.**
- **Do not trade HM-DEC-120, and do not loosen HM-DEC-095.**
- **Do not build N parallel decoders.** *Named, parked, Tim's.*
- **Do not report a character count in place of the signature table.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **`CLAUDE_CODE.md` §8
names FIVE sections** — **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us**, **Where the phase stands** — with
**RECORDED / NEEDS A RULING / STATE** per §12.2. **`CLAUDE.md` §12.2 names four.**
*Report the collision.* Section 5 is additive and is written.

**Section 1 opens with task 1's answer**: the unit each classifier actually used,
on every capture, and whether they are the same number.

**Section 2 quotes every real capture before and after, and the adjudicated
readings by name**, and says in one sentence whether the operator will see more
correct characters on a clear signal.

**Section 5 is measurement only** — the phase number now, what it was before this
unit, and the build number this unit produced. **No proposal, no what-next.**

**Stop and report.**
