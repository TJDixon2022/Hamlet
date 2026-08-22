# WORK_INSTRUCTIONS.md

**Phase 10 — unit 11. Build 1.10.10 → 1.10.11.**

> **How moving the unit moves the phase:** the phase is 80% correct translation on
> a single clear CW signal; this unit removes the one mechanism that turns a
> correct element stream into wrong characters when the estimated unit is off,
> which is what `cw-2026-08-18-004507` is doing right now at 24 words a minute
> against a true 18.

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

**Ruled by Tim: the measured unit ships, and this is the change that repairs what
it broke.**

The previous unit replaced the speed search with a direct measurement and it
bought a great deal: **nothing invented on the sweep from eighteen decibels down
to five**, where it invented from ten down; `VA3VRR` in full on the capture
HM-DEC-145 adjudicated as exactly that; `AA4MP/4 QNIK` back, which HM-DEC-126
records as independently confirmed; `IT WAS JUNK`, `STILL HVE MY ETO 91B`,
`AT L EAST 2 MOVI ES A DAY WID X WHY NOT`. **Failing set 32 → 28.**

**And `cw-2026-08-18-004507` came apart**, from

    E AT ARRL DOT NET <BT> EACH STATION HANDLING ET HIS

to

    E E E U T EA R R L D O T N E T <BT> E E A C H S TA TI O N HAN D L I NG

**The mechanism is named, not a mystery.** That file's measured unit came out
**50 ms — 24 words a minute — on a source sending 18.** The boundary between an
element gap and a character gap is derived from the unit, so **a wrong unit shreds
the spacing**, and every letter breaks into single elements.

**This unit removes the coupling.** The gap boundary stops being a multiple of the
estimated unit and comes from the gaps themselves.

---

## The change

**The gap distribution does not need the speed.** Measured on the W1AW captures:

    element gaps    52, 55, 55, 55 ... 60, 65     (72 of them, spread 13 ms)
       [ empty from 65 to 125 ]
    character gaps  125, 135, 140 ... 185, 190, 192
       [ empty from 235 to 405 ]
    word gaps       405, 410, 412 ... 437, 442

**Two empty regions, 60 ms and 170 ms wide.** Boundaries placed at the geometric
means of the k-means centroids land in dead space **where no observation can be
misclassified.**

**Compare a boundary at 2u.** With u derived correctly it lands at 133 ms, which is
**above the shortest real character gaps at 125** — and those letters silently
merge. With u wrong, as on `004507` at 50 ms, it lands at 100 ms, **inside the
element-gap cluster**, and every gap becomes a letter break. **That is exactly the
observed failure.**

Measured effect of taking the boundary from the gaps: `MENTIMETER` became
`CENTIMETER`, and `P<.-.--->PAGATION` became `PROPAGATION FORECAST`.

**Build it:**

- **3-means on `log(gap)`**, because a word gap is seven times a dit rather than
  six units longer.
- **Boundaries at the geometric means of adjacent centroids.**
- **Keep u as a sanity clip only** — `[1.3u, 2.6u]` and `[3.5u, 6.5u]` — **not as
  the estimate.** Word gaps are too rare in thirty seconds for that cluster to be
  trustworthy and the clip carries it.
- **When there are too few gaps to cluster, say so and fall back to the clip.**
  A window of noise has no gap structure and must not be given one.

**Report, per capture:** the boundaries found, in milliseconds and in units, and
**whether each landed in an empty region of that capture's own gap distribution.**
That last is the evidence the mechanism is working rather than the number looking
plausible.

---

## Then, if the first change lands cleanly

**Fine tone tracking.** Independent of the above and cheap.

The carrier on the W1AW captures measures **499.9 Hz, stable to ±0.1 Hz over four
minutes.** A 25 Hz search grid cannot express that. On the 01:41 capture the signal
sits at **608 Hz**, the grid offered 600 and 625, **and the sweep took 625 — 17 Hz
off, measuring the signal 4 dB weaker than 600 would have.**

**A full-length FFT peak costs one transform and resolves to a fraction of a
hertz.** Track it continuously; do not quantise to 25 Hz afterwards.

**This also repairs the keying sweep**, which is the independent witness — **its
own wording does not change.**

**Report the tone per capture**, and what the keying sweep picks on the 01:41
capture against a true 608 Hz.

---

## Acceptance

**`cw-2026-08-18-004507` reads at least as well as it did before the measured
unit** — that is the regression this unit exists to repair — **and nothing else
gets worse.**

Then the signature, every real capture, before and after:

| | target |
|---|---|
| per cent of letters that are E | single figures |
| per cent of letters that are T | single figures |
| per cent single-character words | **zero** |

**Report those three on every capture. A character count is not the measure and
must not be reported in its place** — more single-element letters is more
characters and worse reading.

**And the three adjudicated readings**, by name:

- `N4L` on `134712` — **currently reading `R4L` and that is a regression from the
  previous unit. Report it.**
- `VA3VRR` on `013347` — currently full and correct.
- `AA4MP/4 QNIK` on `003758` — currently correct.

**All three should be right. Say plainly if they are not.**

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **The previous session found change 1 of the analysis had no site as written**,
  because the decoder forms no threshold at all — every hop is scored against two
  Gaussians and nothing commits. **It built the Schmitt trigger inside the new
  estimator instead, which was right.** Expect the same here: **the analysis
  describes a decoder Hamlet no longer is, in places. Build the mechanism where it
  can exist and say where that was.**
- **Record the failing-test set exactly before and after and name every
  difference.** It is 28. Three are known and named: `ItKeepsUpWithLiveAudio`,
  which fails on letters and not timing at 1.3 % of real time;
  `HoldingTheWindowLongInTimeReadsMore` on `004507`; and
  `TheSlowEndReadsTheMessage(12 wpm, 18 dB)`.
- **The seven W1AW captures, `2026-08-22.jsonl` and both analysis documents are
  not in the tree.** Every figure quoted from them goes unchecked. **Say so once
  and measure on what is here.**
- **The overfitting guard.** The gap figures came from one station, one speed, one
  pitch, one machine keyer. **The clustering is a mechanism and should generalise;
  the clip bounds are constants and must be re-measured against the synthetic
  corpus and the older captures. State for each change whether it was a mechanism
  found or a parameter tuned.**

---

## Rulings in force

- **HM-DEC-120.** Nothing emitted on audio holding no signal. **It is further ahead
  than it has ever been — nothing invented from eighteen decibels down to five. Do
  not give any of that back.**
- **HM-DEC-095.** **Do not loosen its separation limit, confirmation rule or
  plausibility bounds.**
- **HM-DEC-048** and **HM-DEC-108**, on confidence.
- **HM-DEC-091.** The captures are permanent read-only fixtures. **Nothing edits a
  WAV or a sidecar.**
- **HM-DEC-063** and **HM-DEC-150**, the version scheme. **HM-DEC-150 rules that
  the minor is the phase and the patch is the work unit, in
  `Directory.Build.props` alone.** `CLAUDE_CODE.md` §4.11 says the same and defers
  to a project ruling; **they agree, and HM-DEC-150 is the governing text. Report
  that as a collision resolved rather than a conflict.**
- **HM-DEC-103**, the same fragmenting signature in the reference decoder with a
  hardcoded window suspected underneath. **Named so it is not rediscovered. Not
  this unit.**
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

**Read `Directory.Build.props`. It should say 1.10.10 — measured from the About
box on 2026-08-22 21:27 UTC. Bump the patch to 1.10.11 and report the move.** If it
does not say 1.10.10, **say what it says** rather than assuming.

`PHASE_GOAL.md` arrives with this order and states the phase goal: **80% correct
translation on a single clear CW signal.**

---

## Parked — do not touch, do not raise

- **§5 of the analysis: why the mark and gap classifiers disagree about the unit.**
  Structural. Its own unit, with HM-DEC-103 alongside.
- **§6: the narrow decoder-side filter**, ENBW 50–100 Hz around the tracked tone.
  **The crowded passband is a different problem** — at 01:43 four or five stations
  shared the 500 Hz passband, the 606 Hz signal led by only 1–11 dB and lost
  outright in three of fifteen windows. **No segmenter recovers a gap somebody else
  is transmitting in.** Separate work, separate tests.
- **HM-OPEN-056**, `tonePeak`/`snrDb` as a held peak of an instantaneous ratio,
  which reports 54.7 dB on a capture holding no station.
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

**Whether the measured unit ships while `004507` reads worse leaves this queue** —
ruled: it ships, and this unit repairs it.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

- **Do not derive a gap boundary from a multiple of the unit.** *That coupling is
  the whole of what this unit removes.*
- **Do not cluster on raw durations.** *A word gap is seven times a dit, not six
  units longer. Cluster on logs.*
- **Do not report a character count in place of the signature table.**
- **Do not revert the measured unit.** *Ruled. It ships.*
- **Do not trade HM-DEC-120, and do not loosen HM-DEC-095.**
- **Do not chase the structural fault or the narrow filter here.** *Both parked,
  both their own units.*
- **Do not claim a clear signal reads perfectly.** *The measured pipeline still
  gives `NACKETY` for `PACKET`.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **`CLAUDE_CODE.md` §8
now names FIVE sections** — **What Claude did**, **What Tim should expect**, **What
we should do next**, **What's blocking us**, and **Where the phase stands**.
**§12.2 of `CLAUDE.md` names four.** *Report that collision.* Under §0 the
project's own file wins on the four it names; **section 5 is additive and is
written.**

**Section 5, "Where the phase stands", is measurement only** — the phase number
now, what it was before this unit, and the build number this unit produced. **No
proposal and no what-next.**

**Section 1 opens with `cw-2026-08-18-004507` quoted before and after**, because
repairing it is what this unit is for.

**Section 2 quotes every real capture before and after, and the three adjudicated
readings by name**, and says in one sentence whether the operator will see more
correct characters on a clear signal.

**Stop and report.**
