# WORK_INSTRUCTIONS.md

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

**A pipeline measured on Hamlet's own captures reads the bulletin. Hamlet, on the
identical audio, does not.**

| | on `cw-2026-08-22-032113` |
|---|---|
| the measured pipeline | `NACKETY ANDIINTERNET VERSIONSE O 20J6 PROPAGATION FOI` |
| Hamlet | `EG EMTEM TMOEM TPETT O5ZG RTEG T W ORR TT` |

**Same WAV. Same information. One reads a bulletin, the other does not.** This
unit implements the three changes that carry nearly all of that distance, plus one
cheap fourth. They are about sixty lines between them.

**Every number below was measured on the seven W1AW captures and the two crowded
40 m captures. Reproduce what you rely on.** If those files are not in the tree,
**say so and measure on whatever real captures are**, naming what went unchecked.

---

## The four changes, in the order they pay

### 1. Hysteresis, not a threshold — and about 6 dB deep

**The single largest lever. Nothing else here is worth doing first**, because §2
and §3 both compute statistics on the mark stream this produces.

A single threshold makes the envelope cross back and forth on every rise, every
fall, and through every shallow fade inside a mark. **A Schmitt trigger — on at
`threshold + h`, off at `threshold - h` — fixes it with no time constant at all**,
and because both edges are delayed by roughly the same amount, mark lengths
survive. The usual repair, a minimum-run despeckle, is a millisecond constant that
must be retuned for every speed. **This is not.**

Measured, on a source whose true mark count is ~125 per 30 s:

| hysteresis | dah/dit | marks per 30 s | letters kept vs the 6 dB run |
|---|---|---|---|
| ±1 dB | 2.92 | **213** | 24 % |
| ±2 dB | 2.84 | 184 | 33 % |
| ±3 dB | 2.72 | 159 | 47 % |
| ±4 dB | 2.73 | 141 | 75 % |
| ±5 dB | 2.64 | 130 | 90 % |
| **±6 dB** | **2.61** | **125** | **100 %** |
| ±8 dB | 2.64 | 120 | 90 % |
| ±10 dB | 2.73 | 115 | 82 % |
| ±12 dB | — | — | signal lost |

**At ±1 dB, seventy per cent of the "elements" are edge chatter** and every
downstream statistic is computed on them. **The optimum is broad and flat from 5
to 8 dB and collapses below 4 and above 10.**

- **Ship 6 dB.** **Test that the 5–8 dB plateau holds** — that is the evidence the
  value is a mechanism and not a tuned constant.
- **This is the one constant in the whole pipeline not derived from the audio.**
  Say so where it lives.
- **It is also a candidate explanation for the 95-then-202 element counts** on
  `032050` and `032113` against a flat independent 118–127. A counter fed by a bare
  threshold swings like that; a hysteretic one does not. **Report whether it
  stops.**

### 2. The unit comes from a mark AND a gap, never from either alone

**This replaces the speed search entirely and explains why the likelihood is
flat.**

**Any amplitude threshold biases marks and gaps in opposite directions by the same
amount**: a mark reads *b* too long and the gap beside it *b* too short. Measured
on `031838`: dit marks at 82 ms, element gaps at 54 ms. **From marks alone, 14.6
WPM. From gaps alone, 22 WPM. Both wrong, neither knows it.** The average is
68 ms — **18 WPM, and the bias has cancelled exactly.**

    u = (median dit mark + median element gap) / 2

**Across the seven files this returns 62.5–65.7 ms — a 5 % spread, mean 18.7 WPM
against a true 18.0.** One line. No grid, no scoring function, no hypothesis
search.

**The information a 33-wide likelihood sweep could not resolve to better than 0.05
is sitting in the ratio of two medians.**

- **Both clusters come from k-means on `log` durations**, because dit:dah is a
  ratio, not a difference.
- **This needs §1 first.** A chattery mark stream has no dit cluster to take a
  median of.
- **Report what happens to the existing speed grid.** If it becomes dead code, say
  so; if something else still reads it, name what.

### 3. Gap boundaries from the observed gaps, not from multiples of u

Hamlet places the character-gap boundary at a multiple of its speed estimate.
**That couples two independent failures: get the speed wrong and the letter
spacing dies with it.**

**The gap distribution does not need the speed.** Measured on `031838`:

    element gaps    52, 55, 55, 55 ... 60, 65     (72 of them, spread 13 ms)
       [ empty from 65 to 125 ]
    character gaps  125, 135, 140 ... 185, 190, 192
       [ empty from 235 to 405 ]
    word gaps       405, 410, 412 ... 437, 442

**Two empty regions, 60 ms and 170 ms wide.** Put the boundaries at the geometric
means of the k-means centroids and they land in dead space **where no observation
can be misclassified.**

**Compare a boundary at 2u**: with u derived correctly it lands at 133 ms, which is
**above** the shortest real character gaps at 125, **and those letters silently
merge.**

Measured effect: this alone turned `MENTIMETER` into `CENTIMETER`, and produced
`PROPAGATION FORECAST` where the u-derived boundary gave `P<.-.--->PAGATION`.

- **3-means on `log(gap)`.**
- **Keep u as a sanity clip only** — `[1.3u, 2.6u]` and `[3.5u, 6.5u]` — **not as
  the estimate.** Word gaps are too rare in thirty seconds for the cluster to be
  trustworthy and the clip carries that one.

### 4. Find the tone finely, and re-find it

The carrier measures **499.9 Hz in all seven files, stable to ±0.1 Hz over four
minutes.** A 25 Hz search grid cannot express that.

On the 01:41 capture the signal sits at **608 Hz**, the grid offered 600 and 625,
**and the sweep took 625 — 17 Hz off, measuring the signal 4 dB weaker than 600
would have.**

**A full-length FFT peak costs one transform and resolves to a fraction of a
hertz.** Track the peak continuously and do not quantise the answer to 25 Hz
afterwards.

**This also repairs the keying sweep**, which is the independent witness — **but
the witness's own wording does not change.**

---

## The reference pipeline, for comparison

Every parameter derived from the audio except the hysteresis depth:

    tone       full-length FFT peak, 300-1300 Hz            -> 499.9 Hz
    envelope   30 ms Hann Goertzel at that tone,
               2.5 ms hop, 10 ms power-domain smoothing
    threshold  Otsu two-class split of the envelope histogram -> about -37 dB
    keying     Schmitt trigger at threshold +/- 6 dB          -> ~125 marks / 30 s
    unit       2-means on log(mark), 2-means on log(gap),
               u = (dit + element_gap) / 2                    -> 63-67 ms
    marks      dit/dah split at sqrt(dit * dah)
    gaps       3-means on log(gap); boundaries at the geometric
               means of adjacent centroids, clipped into
               [1.3u, 2.6u] and [3.5u, 6.5u]

**Result: tone 499.9 Hz every time, speed 18.0–19.1 WPM against a true 18.0.**

---

## Acceptance — the signature, not a character count

**A character count is not the measure and must not be reported in place of
this.** Hamlet's signature is **every mark read as a dit and every gap read as a
character break** — `EE EE E E E E EE`.

A forced-unit sweep proved **no single unit produces both symptoms**:

| forced WPM | % letters that are E | % letters that are T | % single-character words |
|---|---|---|---|
| 10 | 23 | 0 | **0** |
| 14 | 15 | 4 | **0** |
| 18 | 11 | 9 | **0** |
| 22 | 11 | 9 | **0** |
| 26 | 15 | 9 | 67 |
| 30 | 13 | **67** | 33 |
| 32 | 8 | **76** | 29 |
| 44 | 10 | **80** | 23 |

**Where letters fragment, the letters are T, not E.** Where E dominates, nothing
fragments.

**Report those three percentages on every real capture, before and after.** The
target is the 10–22 WPM rows: single-character words at zero, and E and T both
in single figures.

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **Record the failing-test set exactly before and after, and name every
  difference.** Two synthetic 30 WPM tests are already red from the ratio penalty
  and are **not this unit's to fix.**
- **Report on the sweep AND every real capture together, every time.**
- **The overfitting guard.** Everything above was measured on **one station, one
  speed, one pitch, one machine keyer.** The hysteresis and the log-domain
  clustering are mechanisms and should generalise; **the specific plateau width
  must be re-measured against the synthetic corpus and the older captures before
  any of these numbers become constants.** **State for each change whether it was a
  mechanism found or a parameter tuned.**

---

## Rulings in force

- **HM-DEC-120.** Nothing emitted on audio holding no signal. **The ratio penalty
  improved it to 1.00 right and 0.00 invented at eleven and nine decibels. Do not
  give that back.**
- **HM-DEC-095.** **Do not loosen its separation limit, confirmation rule or
  plausibility bounds** to make anything here pass.
- **HM-DEC-048** and **HM-DEC-108**, on confidence.
- **HM-DEC-091.** The captures are permanent read-only fixtures. **Nothing edits a
  WAV or a sidecar.**
- **HM-DEC-103**, which records this same fragmenting signature in the *reference*
  decoder with a hardcoded window suspected underneath. **Named here so it is not
  rediscovered. Chasing it is the next unit, not this one.**
- **HM-DEC-009**, **§0.0**, **§0.0.1**.
- **HM-DEC-150**, the version scheme.
- **HM-DEC-093** and `SHACK_FACTS.md` — no radio on the development machine.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md`
**§13**, which names that file's fields — `STATE`, `PHASE`, `BALL`, `NEXT_PASTE`,
`UPDATED`, `NOTE`. `UPDATED` from the clock; `NOTE` says what is moving inside the
task. Also every ten minutes while a task runs.

---

## Task order

**One change at a time, measured between. §1 first, always.**

1. **Hysteresis at 6 dB.** Report mark counts per capture, before and after, and
   whether the 95/202 instability stops. **Report the 5–8 dB plateau.**
2. **`u = (dit + element_gap) / 2`.** Report the unit and the speed per capture
   against 62.5–65.7 ms and 18.0–19.1 WPM. Report what became of the speed grid.
3. **Gap boundaries from clustered gaps**, u as a clip only. Report the boundaries
   found per capture, in milliseconds and in units, and whether they landed in
   dead space.
4. **Fine tone tracking.** Report the tone per capture against 499.9 ± 0.1, and
   what the keying sweep now picks on the 01:41 capture against 608 Hz.
5. **The signature table**, every real capture, before and after.
6. **Bump the version.** Read `Directory.Build.props`, bump the patch, report the
   move.

**If a change makes something worse, stop and report at that task** rather than
continuing to the next.

---

## Parked — do not touch, do not raise

- **§5 of the analysis: why the mark classifier and the gap classifier disagree
  about the unit.** **A structural fault, not a tuning error, and fixing the speed
  will not touch it.** Its own unit, with HM-DEC-103 checked alongside.
- **§6: the narrow decoder-side filter**, ENBW 50–100 Hz around the tracked tone.
  **The crowded-passband case is a different problem** — at 01:43 four or five
  stations shared the 500 Hz passband and the 606 Hz signal led by only 1–11 dB,
  losing outright in three of fifteen two-second windows. **No segmenter recovers a
  gap somebody else is transmitting in.** Separate work, separate tests, **and it
  must not be judged by this unit's measurements.**
- The two 30 WPM synthetic tests. The W1AW captures as fixtures and the ARLP034
  harness — **ARLP034 is not published; the archive stops at ARLP033, 14 August.**
- The window clear; pitch distance as a sender-change test; the survey ranking by
  loudness; the advice line; the sidecar contradiction; `FollowSpeed`;
  `HM-OPEN-051`; **HM-OPEN-056**, the held-peak SNR.
- **HM-OPEN-012, HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098,
  HM-OPEN-033, HM-OPEN-007.**

---

## Asks still outstanding

Carried inbound per HM-DEC-139, verbatim until ruled. **Verify against
`OPEN_ISSUES.md` and report anything here that is closed, or open and missing.**

- Why the mark and gap classifiers disagree about the unit — §5, structural.
- The narrow decoder-side filter for a crowded passband — §6.
- HM-OPEN-056, `tonePeak`/`snrDb` as a held peak of an instantaneous ratio.
- The two 30 WPM synthetic tests, red since the ratio penalty.
- The seven W1AW captures and `2026-08-22.jsonl` are not in the tree; **ARLP034 is
  not published.**
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

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

- **Do not report a character count in place of the signature table.** *More
  single-element letters is more characters and worse reading.*
- **Do not derive a gap boundary from a multiple of u.** *That is the coupling §3
  removes.*
- **Do not cluster on raw durations.** *dit:dah is a ratio. Cluster on logs.*
- **Do not add a minimum-run despeckle.** *A millisecond constant retuned per
  speed is what the hysteresis replaces.*
- **Do not chase §5 or §6 here.** *Both are named, both are parked, both are their
  own units.*
- **Do not trade HM-DEC-120, and do not loosen HM-DEC-095.**
- **Do not claim the bulletin reads perfectly.** *The measured pipeline still gives
  `NACKETY` for `PACKET` and `TELEWRINTER` for `TELEPRINTER`. Some residual errors
  are segmentation and some are the air.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **§12.2 names the four
headings** — **What Claude did**, **What Tim should expect**, **What we should do
next**, **What's blocking us** — the last carrying **Asks still outstanding** per
HM-DEC-139. No other headings.

**Section 1 opens with the signature table** — per cent E, per cent T, per cent
single-character words — **on every real capture, before and after.**

**Section 2 quotes every real capture before and after**, and says in one sentence
whether the operator will see readable words.

**Stop and report.**
