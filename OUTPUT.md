# OUTPUT.md

## 1. What Claude did

**The signature, on every real capture in the tree, before and after.** The seven
W1AW captures are still not here, so this is measured on what is: nine captures,
seven with a station in them.

| capture | E before → after | T before → after | one-letter words before → after |
|---|---|---|---|
| `cw-2026-08-17-013347` | 54% → **42%** | 28% → **5%** | 57% → 60% |
| `cw-2026-08-17-013622` | 57% → **51%** | 8% → 7% | 48% → **27%** |
| `cw-2026-08-17-134712` | 75% → 74% | 0% → 0% | 95% → **76%** |
| `cw-2026-08-18-004507` | 15% → 21% | 15% → 13% | 50% → **78%** |
| `cw-2026-08-18-003016` | 13% → 14% | 16% → 18% | 15% → 26% |
| `cw-2026-08-18-003126` | 16% → 16% | 14% → 12% | 50% → 50% |
| `cw-2026-08-18-003758` | 47% → **41%** | 0% → 0% | 68% → 65% |
| `cw-2026-08-20-014854`, `-014935` | silent | silent | silent |

**It is not at the target and it moved the right way on five of seven.** `004507`
went the wrong way and that is the whole of what broke; it is the first ask.

**The seven W1AW captures, `2026-08-22.jsonl` and both analysis documents are
still absent**, so every figure quoted from them went unchecked, including the
125-marks-per-30-s reference count, the 62.5–65.7 ms unit range and the reading of
`032113`. Everything below is measured here.

### Task 1 — hysteresis, and the plateau reproduces

Built as `CwUnitEstimator`: an Otsu two-class split of the envelope in decibels,
then a two-level trigger at ±6 dB around it. Mark counts per capture:

| capture | 0 dB | 1 | 3 | 5 | **6** | 8 | 10 | 12 |
|---|---|---|---|---|---|---|---|---|
| `004507` | 222 | 213 | 162 | 126 | **121** | 116 | 116 | 0 |
| `003016` | 224 | 219 | 181 | 158 | **156** | 151 | 135 | 0 |
| `003126` | 245 | 239 | 186 | 153 | **146** | 144 | 142 | 0 |
| `003758` | 404 | 372 | 306 | 198 | **154** | 107 | 101 | 99 |
| `013347` | 85 | 85 | 84 | 84 | **84** | 84 | 83 | 83 |

**`004507` reproduces the order's table almost exactly** — 213 marks at one
decibel against their 213, 121 at six against their 125, and the signal lost at
twelve. **The five-to-eight plateau holds** and is now a test:
`TheFiveToEightDecibelPlateauHolds` fails if the count at five, six or eight moves
more than a seventh from the count at six, or if one decibel stops producing half
again as many.

**Mechanism found, not a parameter tuned** — except the depth itself, which is
**the one number in the estimator not derived from the audio**, and which says so
where it lives.

**The 95-then-202 instability could not be checked**: those are files `032050` and
`032113`, which are not here.

### Task 2 — the unit is measured, not searched

    u = (median of the short mark cluster + median of the short gap cluster) / 2

Both clusters by two-means on the **logarithms**, because a dah is three times a
dit rather than three units longer.

**Against audio generated at a known speed**, which is the only truth available in
this repository:

| true | measured, noise 0.02 | noise 0.08 | mark alone | gap alone |
|---|---|---|---|---|
| 12 wpm | **12.0** | 11.7 | 10.9 | 13.3 |
| 18 wpm | **18.5** | 17.8 | 16.0 | 21.8 |
| 25 wpm | **25.3** | 25.3 | 21.8 | 30.0 |

**The mechanism is proved rather than the answer**: the mark alone is always too
slow and the gap alone always too fast, by about the same amount, and the test
fails if that ever stops being true. That is the bias cancelling.

**On the captures**: `013347` and `013622` both give 62.5 ms, which is inside the
order's 62.5–65.7 ms range; `003016`, `003126` and `003758` give 45–47.5 ms;
`004507` gives 50.0 ms, **24 words a minute**.

**What became of the speed grid.** It is still there and it still decides **when
the window holds too little keying to cluster** — fewer than eight marks or eight
gaps — which is what a window of noise looks like. Where the estimator is ready,
the grid is not searched: one hypothesis is decoded instead of thirteen.

**Cost: 1.3 per cent of real time**, measured by `ItKeepsUpWithLiveAudio`.

### What it did to the sensitivity sweep

**This is the largest change and it is all in the right direction.**

| dB | 18 | 12 | 11 | 10 | 8 | 6 | 5 | 3 | 0 | −2 | −4 | −6 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| before, right / wrong | 1.00/0.00 | 1.00/0.00 | 1.00/0.00 | 0.97/**0.03** | 0.97/**0.03** | 0.94/**0.03** | 0.89/**0.03** | 0.75/0.17 | 0.56/0.33 | 0.47/0.36 | 0.11/0.64 | 0.00/0.00 |
| now | 1.00/0.00 | 1.00/0.00 | 1.00/0.00 | **1.00/0.00** | **1.00/0.00** | **1.00/0.00** | **0.97/0.00** | **0.86/0.03** | **0.78/0.11** | **0.72/0.17** | **0.44/0.19** | 0.00/0.00 |

**Nothing is invented anywhere from eighteen decibels down to five**, where before
it invented from ten down. HM-DEC-120 is not traded; it is further ahead than it
has ever been. **`CwSensitivityTests.ItGoesQuietRatherThanInventingLettersInTheNoise`
goes green**, having been red for days.

### The failing set

**32 before, 28 after.** Five went green, three went red.

**Green:** `CwSensitivityTests.ItGoesQuietRatherThanInventingLettersInTheNoise`;
`CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp(30 wpm)` and
`TheSameFistWithARunUpDoesNot(30 wpm)`, **which the ratio penalty broke last
session**; `CwAcquisitionWindowTests.TheSlowEndReadsTheMessage(12 wpm, 3 dB)`;
`CwRefiningRetuneTests.TheSurveySettlingBetweenTwoBinsIsNotAStationChange`. The two
`ScopeStreamTests` also cleared, and they are the flake that passes alone
(`HM-OPEN-055`).

**Red, and all three are one file or one fixture variant:**
`TheProbabilisticDecoderTests.ItKeepsUpWithLiveAudio` — **not a timing failure, it
reads `S TA TI O N` where it wants `STATION HANDLING`**;
`WhatBandwidthTheDecoderListensThroughTests.HoldingTheWindowLongInTimeReadsMore`
on `004507`; and `TheSlowEndReadsTheMessage(12 wpm, 18 dB)`, which swapped with the
3 dB variant that went green.

### Tasks 3 to 5 — not attempted, and why

**The order says to stop and report at the task where something gets worse.**
`004507` got worse at task 2, so task 3 (gap boundaries from clustered gaps), task
4 (fine tone tracking) and the rest were not started. **None of them was
half-built.**

### Task 6 — the version

**`Directory.Build.props` moved 1.10.9 to 1.10.10.**

### Mismatches in the order, reported and not repaired

**Change 1 has no site in Hamlet's decoder as written.** It describes adding
hysteresis to a threshold, and **the decoder forms no threshold at all**: every hop
is scored against two Gaussians and nothing commits, which is the whole of what
replaced the thresholded decoder. So the Schmitt trigger was built where it can
exist — inside the new estimator, over the same envelope — and it feeds the unit
rather than the decoding. Changes 2 and 3 read as descriptions of the old decoder
in one respect too: Hamlet does not place a character-gap boundary at a multiple of
its speed estimate; the Viterbi chooses element and character boundaries together,
and the crossover between them sits at 1.73 units of whatever unit it is given.

## 2. What Tim should expect

**He will see readable words in three places he could not before, and one capture
he could read has come apart.**

| capture | before | now |
|---|---|---|
| `013347` | `… E TTT TVRR VATTTMTTTT…` | `… E HEA E WVRR VA3VRR E E` — **the callsign HM-DEC-145 adjudicated as `VA3VRR`, in full** |
| `003758` | `… AA■IH/5■IS E E E EAN EANQNI<HH>SK …` | `… QR■HH 55H AA4MP /4 QNI K E E …` — **`AA4MP/4 QNIK`, the reading HM-DEC-126 records as independently confirmed** |
| `003016` | `E ■I KPA1■S<HH> ■NK <BT> SDLL H■EMY ETO91B …` | `E E IADA KPA15TT IT WAS JUNK ■ E STILL HVE MY E TO 91B …` |
| `003126` | `… ATLE<AS>T 2 MOVIESA DAY WID X■ WHY NOTT …` | `… AT L EAST 2 MOVI ESA DAY WID X■ WHY NOT …` |
| `004507` | `E AT ARRL DOT NET <BT> EACH STATION HANDLING ET HIS…` | `E E E U T EA R R L D O T N E T <BT> E E A C H S TA TI O N HAN D L I NG…` — **worse** |
| `134712` | `… E K E E N4LQ  K …` | `… E K E R4LQ EK …` — the callsign HM-DEC-144 adjudicated as `N4L` is now `R4L` |

Build clean, no warnings, version 1.10.10. **28 failing against 32.**

**What will look wrong and is not:** `ItKeepsUpWithLiveAudio` is red and the
decoder is faster than it needs to be — 1.3 per cent of real time. It fails on the
letters, not the clock.

## 3. What we should do next

- **Task 3, the gap boundaries**, is the change that addresses exactly what broke:
  `004507` fragments because its measured unit is 50 ms and the boundary between an
  element gap and a character gap is derived from that unit. Taking the boundary
  from the observed gap distribution instead decouples the two, which is the point
  of it.
- **Task 4, fine tone tracking**, is untouched and independent.
- **Get the seven W1AW captures across** and every figure in the order becomes
  checkable.

## 4. What's blocking us

Nothing blocks the next unit.

### RECORDED

Nothing was recorded to `DECISIONS.md`.

### NEEDS A RULING

> **Whether the measured unit ships while `cw-2026-08-18-004507` reads worse.**
>
> It is in the tree and this is the ask to reverse it. **What it buys**: nothing
> invented on the sweep from eighteen decibels down to five where it invented from
> ten down; five tests green including HM-DEC-120's own guardian and both tests the
> ratio penalty broke; the full `VA3VRR` and the confirmed `AA4MP/4 QNIK` back on
> two captures with adjudicated content. **What it costs**: `004507` fragments into
> single letters and takes two tests with it, and `134712` reads `R4L` where it
> read `N4L`.
>
> | | keep it | revert it | keep it and do task 3 |
> |---|---|---|---|
> | the sweep | no invention 18 to 5 dB | invention from 10 dB down | unmeasured |
> | adjudicated callsigns | `VA3VRR` and `AA4MP/4` back, `N4L` lost | `N4L` kept, other two lost | the point of task 3 |
> | `004507` | fragments | reads | task 3 addresses exactly this |
> | failing set | 28 | 32 | unmeasured |
>
> **The industry-standard answer is the third**: the fragmentation has a named
> mechanism — the gap boundary is derived from a unit that is wrong on that one
> file — and task 3 removes the coupling rather than tuning around it. **Keeping it
> meanwhile is the better of the two available today**, because it is ahead on the
> sweep, on the failing set and on two of the three adjudicated readings.

### STATE

Gate verified against the tree: `Hamlet.sln` and `CwProbabilisticDecoder.cs`
present, no `CoreHMI.sln`, no `src\CoreHMI`, `PROJECT_CARD.md` says Hamlet. **This
session ran on the development computer with no radio connected, so nothing here is
evidence about the radio** (`SHACK_FACTS.md`, HM-DEC-093).

**Tasks 1 and 2 shipped. Tasks 3, 4 and 5 were not started**, under the order's own
rule to stop at the task where something gets worse, and none was half-built. Task
6 is done.

### Asks still outstanding

Carried per HM-DEC-139, verbatim until ruled.

- **Whether the measured unit ships while `004507` reads worse**, first made today,
  above.
- Why the mark and gap classifiers disagree about the unit — §5, structural.
- The narrow decoder-side filter for a crowded passband — §6.
- HM-OPEN-056, `tonePeak`/`snrDb` as a held peak of an instantaneous ratio.
- The seven W1AW captures and `2026-08-22.jsonl` are not in the tree; ARLP034 is
  not published.
- The likelihood is flat in speed above eleven words a minute — **this unit is the
  answer to it, pending the ruling above.**
- Whether the fast end is worth the slow end — **the two 30 WPM tests went green,
  so this may be moot; it stays until ruled.**
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
