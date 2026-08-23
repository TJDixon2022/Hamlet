# OUTPUT.md

## 1. What Claude did

**Task 1's answer: the two classifiers use the same number. The inference this
unit was built on does not hold, and the dual signature has another cause, which
is measured below.**

Logged window by window on every capture — the unit the marks are scored against,
and the unit the gaps are scored against:

| capture | windows with one unit | with two | worst ratio |
|---|---|---|---|
| `013347` | **55** | **0** | 1.00 |
| `013622` | **54** | **0** | 1.00 |
| `134712` | **59** | **0** | 1.00 |
| `004507` | 23 | 34 | 1.42 |
| `003016` | **58** | **0** | 1.00 |
| `003126` | **57** | **0** | 1.00 |
| `003758` | **59** | **0** | 1.00 |
| `014854`, holding no station | 53 | 5 | 2.01 |
| `014935`, holding no station | **59** | **0** | 1.00 |

**Every capture carrying an adjudicated reading scores its marks and its gaps
against one number in every window.** The only capture that diverges while
producing text is `004507`, and it diverges **deliberately**: its gap structure
holds for twelve consecutive reads, so the sender's own lengths are used, which is
what unit 12 shipped. `014854` holds no station and emits nothing whatever its
windows do.

**So the order says stop, and this stops.** Task 2 is not built.

**And the gap classifier is unit-derived except where it is measured.** In
`CwProbabilisticDecoder.DecodeAt` every kind's expected length is `kind.Units *
unit`, marks and gaps alike; the one exception is the measured gap lengths handed
in from `CwProbabilisticStream`, and they are a measurement of that sender rather
than a constant. **Nothing in the gap path is scaled by a window length, a hop
count or anything else.**

### What the signature is actually measuring

Splitting every settled character by whether the tracker's keying verdict was true
when it settled:

| capture | while keying | while not |
|---|---|---|
| `013347` | 6 letters, E 0% | **51 letters, E 43%** |
| `013622` | **0 letters** | **44 letters, E 50%** |
| `134712` | **0 letters** | **25 letters, E 76%** |
| `003758` | 19 letters, E 11% | **28 letters, E 64%** |
| `004507` | 18, E 28% | 33, E 15% |
| `003016` | 35, E 6% | 23, E 17% |
| `003126` | 34, E 18% | 15, E 13% |

**The three captures with the worst signature emitted every letter outside the
keying verdict**, and on those the E share outside it is 43, 50, 76 and 64 per cent
against 0, 0, 11 and 28 inside it.

**That changes what three units of work have been aiming at.** The fragmenting
signature has been read as a decoder shredding a signal it heard; on this corpus it
is mostly a decoder emitting letters on stretches its own independent witness says
hold no keying. **Filed as `HM-OPEN-057`**, with the caveat recorded there: the
keying verdict is slow by construction — two agreeing surveys, about three seconds,
expiring three seconds after keying stops (HM-DEC-095) — so a short transmission can
fall entirely outside it, which is the likely case on `134712`.

**Mechanism found, nothing tuned.** Nothing in `src` changed this unit except the
version.

### HM-DEC-103, checked as the order asked

`tools/reference-decoder/reference_decoder.py` carries the same segmental Viterbi
Hamlet does, with the same ratio penalty, and **it has no hardcoded window**: its
envelope step and smoothing come from the file's own rate. The fragmenting
signature HM-DEC-103 records is therefore **not an inherited constant**, and the
port is not carrying one either.

### Task 3 — the sample-rate defect, grepped and answered

**The known-text fixture `cw-2026-08-23-001520` is not in the tree**, so nothing is
asserted against `CQ CQ DE KC3QIS KC3QIS K`. The grep was done anyway.

**There is no hardcoded sample count anywhere in the decode chain, the estimator,
the tracker or the keying meter.** Every window, hop and smoother is expressed in
milliseconds or hertz and converted with the stream's own rate:

| what | where | scaled by |
|---|---|---|
| tracker hop | `CwToneTracker` line 315 | `SampleRate / 200`, five milliseconds |
| tracker windows | line 316, 371, 398 | multiples of that hop |
| decoder envelope smoother | `CwProbabilisticDecoder` line ~368 | `sampleRate / BandwidthHz`, 60 Hz |
| decoder envelope step | line 369 | `sampleRate * HopMilliseconds / 1000` |
| stream hop, window, delay, refill | `CwProbabilisticStream` lines 131–140 | seconds ÷ `HopMilliseconds` |
| keying meter step | `KeyingEnvelope` line 251 | `rate * StepMs / 1000` |
| keying meter smoother | line 259 | `rate / SmoothingHz`, 100 Hz |

**And the corpus already exercises both rates.** `CwSignal.DefaultSampleRate` is
**8000**, so every synthetic fixture — the sensitivity sweep included — is 8 kHz,
while every capture is 48 kHz. **Both have been decoding side by side the whole
time**, which is stronger evidence than the grep.

**The one floor worth naming**: `Math.Max(4, SampleRate / 200)` binds below an
800 Hz sample rate, which nothing produces.

### Task 4 — the narrow filter, not built

**The captures it must be judged by are not in the tree.** The order names 01:41,
01:43 and 00:19 on 2026-08-23 and says explicitly not to judge the filter by the
single-station captures, which are all this repository has. **Building it and
measuring it on files that cannot exercise it would produce a number that means
nothing**, so it is not built and the reason is the missing evidence rather than the
merit.

### The failing set

**28 before, 29 after, and none of the difference is this unit's.** The app flake
`TheFollowedSentenceReachesTheScreenTests` passed this time; two `RigReadTests`
failed in the full run and **pass when their class runs alone** — the rig flake
filed as `HM-OPEN-055`. **Nothing in `src` changed**, so nothing could have moved.

### Mismatches

- **The seven W1AW captures, `2026-08-22.jsonl`, both analysis documents, the
  known-text fixture and the three crowded captures are not in the tree.** Every
  figure quoted from them went unchecked. Everything above is measured here.
- **`Directory.Build.props` said 1.10.12**, as the order expected.
- **`PROJECT_STATUS.md` was present**, so the copy shipped with the order was not
  needed.
- **`CLAUDE_CODE.md` §8 names five report sections and `CLAUDE.md` §12.2 names
  four.** Under §0 the project's file wins on the four it names; section 5 is
  additive and is written. Reported, not repaired.
- **Units 11 and 12 are in the tree as described** — the ±6 dB Schmitt trigger with
  the 5–8 dB plateau pinned by test, the median-of-mark-and-gap unit, and the
  3-means gap boundaries with the trough test and the twelve-read persistence rule.
  Nothing was rebuilt.

## 2. What Tim should expect

**No change on screen at all: the operator will see exactly what he saw at
1.10.12, because this unit measured and did not build.**

| capture | before | after |
|---|---|---|
| `013347` | `… E HA E WVRR VA3VRR E E` | unchanged |
| `013622` | `E I5 SHE II 5EIEIE EEUE TE ISE …` | unchanged |
| `134712` | `… E K E EN4LQ EK …` | unchanged |
| `004507` | `EE AC H STA TI O N HANDLING ET HIS MESSAGE PE` | unchanged |
| `003016` | `E IADA KPA15TT ITWASJUNK <BT> STIL<AS>HVEMY ETO 91B …` | unchanged |
| `003126` | `E S 5 IWATTCH AT L E<AS>T 2 MOVIESADAY WID X■ WHY NOT …` | unchanged |
| `003758` | `… 55H AA4MP/4 QNIK E E E EE EAN EANQNI■K …` | unchanged |
| `014854`, `014935` | silent | silent |

**The three adjudicated readings are all right**: `N4L`, `VA3VRR` and
`AA4MP/4 QNIK`. The signature is unchanged on every capture, and HM-DEC-120 is
where 1.10.12 left it — nothing invented from eighteen decibels down to three,
both silent recordings silent.

Build clean, no warnings.

## 3. What we should do next

- **`HM-OPEN-057` is the finding**: nearly every single-element letter is emitted
  outside the keying verdict. Whether the decoder's own gate and the survey's
  verdict should agree is the question, and it decides what the display asserts.
- **The signature may be the wrong instrument for this corpus.** It cannot tell a
  shredded signal from letters read out of noise, and on these captures it is
  mostly measuring the second.
- **The narrow filter needs its three captures.** So does the known-text fixture,
  which is the only thing that would let the phase be scored.
- **`AC H` and `STA TI O N` on `004507`** are still broken while `HANDLING` and
  `MESSAGE` are whole, which is a segmentation fault inside a stretch that is being
  read.

## 4. What's blocking us

**The evidence, again.** Four of the files this order names are not in the tree,
and two of its four tasks depend on them.

### RECORDED

Nothing was recorded to `DECISIONS.md`. One open issue was filed: **`HM-OPEN-057`**,
with the table above.

### NEEDS A RULING

Nothing needs a ruling to proceed.

> **Whether Hamlet may emit a character while its own keying verdict says nobody is
> sending.**
>
> On the three worst captures every letter was emitted outside that verdict, and
> the E shares outside it are 43, 50, 76 and 64 per cent against 0, 0, 11 and 28
> inside it. **Two instruments disagree about whether anybody is there**, and the
> decoder is the one putting letters on screen.
>
> **It is not acted on here** because gating emission on the verdict decides what
> the display asserts (§12.1) and cuts against HM-DEC-120 from the other side: the
> verdict takes about three seconds to form, so gating on it would silence a real
> station's opening. **The honest first step is to say which instrument is right on
> a capture where the answer is known**, and no such capture is in the tree.

### STATE

Gate verified against the tree: `Hamlet.sln` and `CwProbabilisticDecoder.cs`
present, no `CoreHMI.sln`, no `src\CoreHMI`, `PROJECT_CARD.md` says Hamlet. **This
session ran on the development computer with no radio connected, so nothing in this
report is evidence about the radio** (`SHACK_FACTS.md`, HM-DEC-093).

**Task 1 done and it stopped the unit at its own instruction.** Task 2 not built,
gated on task 1. **Task 3's grep done and answered; its fixture is absent.** Task 4
not built, for the missing captures. None was half-built.

## 5. Where the phase stands

**Phase 10. The goal is 80% correct translation on a single clear CW signal.**

**The phase number cannot be stated, and that is unchanged from before this unit.**
No capture in this repository has an answer key. The known-text fixture that would
have provided one — `cw-2026-08-23-001520`, `CQ CQ DE KC3QIS KC3QIS K` — is not in
the tree. The three adjudicated fragments are read correctly at this build, as they
were at the last.

**Build: 1.10.12 → 1.10.13.**

### Asks still outstanding

Carried per HM-DEC-139, verbatim until ruled.

- **`HM-OPEN-057`, letters emitted outside the keying verdict**, first made today.
- **Why the mark and gap classifiers disagree about the unit** — **answered today:
  they do not.** It leaves the queue.
- The word-gap clip is carrying every real case.
- No capture has an answer key — the fixture that would end this is not in the
  tree.
- The narrow decoder-side filter for a crowded passband — its captures are not in
  the tree.
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
