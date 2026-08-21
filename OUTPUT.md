# OUTPUT.md

## 1. What Claude did

### Task 1: the decoder listens through 30, 38 or 75 hertz, and the 100 is a different instrument

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet`, all four gate checks hold, and **no radio is
attached** (HM-DEC-093).

**The decoder's detection filter is a Hann-tapered Goertzel over its analysis
window**, and there are three of them:

| window | bandwidth | when it is used | noise measured |
|---|---|---|---|
| 50 ms | **30 Hz** | a fist proved at 14 WPM or slower | −63.42 dB |
| 40 ms | **38 Hz** | acquiring, and 14 to 22 WPM | −62.78 dB |
| 20 ms | **75 Hz** | a fist read at 22 WPM or faster | −60.94 dB |

Measured rather than computed: identical Gaussian noise through each window, and
noise power through a filter is proportional to its noise bandwidth. **The 20 ms
window admits 2.48 dB more noise than the 50 ms one.**

**`KeyingEnvelope` is the 100 Hz instrument.** It is a ten millisecond boxcar over
the quadrature arms, fixed, and it is the keying meter — the independent witness,
not the decode path. **The order's table was measured with it.** A figure taken
with one of these instruments is not a figure about the other, and that has now
put a bandwidth claim about the decoder into a work order.

**And the bandwidth already follows the fitted speed**, which task 2 asks for as
though it did not. The order's own sizing rule — four times the element rate, so
about 40 Hz at 15 WPM and 80 at 30 — is what the tree implements: 30 Hz at 14 WPM
and below, 75 Hz at 22 and above.

### But it is not narrow in practice, and the reason is a loop

| recording | fitted | mostly | share |
|---|---|---|---|
| `013347` | 14 wpm | 40 ms / 38 Hz | 72% |
| `013622` | 56 wpm | **20 ms / 75 Hz** | 54% |
| `134712` | 38 wpm | **20 ms / 75 Hz** | 84% |
| `004507` | 22 wpm | **20 ms / 75 Hz** | 92% |
| `003016` | 25 wpm | **20 ms / 75 Hz** | 96% |
| `003126` | 24 wpm | **20 ms / 75 Hz** | 95% |
| `003758` | 35 wpm | **20 ms / 75 Hz** | 94% |
| `014854` | 28 wpm | **20 ms / 75 Hz** | 93% |
| `014935` | 29 wpm | **20 ms / 75 Hz** | 93% |

**Eight of nine sit at 75 Hz, on senders working near fourteen words a minute.**
The filter widens on the strength of a speed the filter's own width helped get
wrong: chatter shortens the fitted dit, a short dit reads as a fast fist, a fast
fist shortens the window, a shorter window trebles the bandwidth, and more noise
crosses the gate.

### Task 2: narrowing it, three single-variable ways, and all three fail HM-DEC-120

| what was changed | result |
|---|---|
| the window follows the **proved** speed, and the unproved default goes 40 ms → 50 ms | 21 red, including every station-finding test |
| the window follows the **proved** speed, tracker untouched | 21 red, including `NothingIsEmittedAnywhereBelowTheFloor` and three cases of `ARecordingWithNoKeyingStaysSilentAtEverySpeed` |
| the fast window 20 ms → 30 ms, one constant, 75 Hz → 50 Hz | 16 red, again including `NothingIsEmittedAnywhereBelowTheFloor` |

**Every genuine narrowing made the decoder emit characters on audio holding no
signal**, which is the one ruling the order says must survive. The first also
breaks station-finding outright, and the reason matters: **the coarse survey that
finds a station in the first place shares the analysis window**, so a window
widened in time is also a search narrowed in frequency.

`src` is byte-identical to what it was at the start of the session.

### And what the window is worth when it is held, which is the finding

Holding the detection window at 50 ms for every measurement, changing nothing else
about the decoder:

| recording | window followed | window held at 50 ms |
|---|---|---|
| `cw-2026-08-18-004507` | 25 — `O T ■T ■■ T ■T ■ O   ■ N D L I ISE SSRG E ■` | **32 — `NET  EAC5 STATION HANDLING HIS ESSAGEP`** |
| `cw-2026-08-18-003016` | 38 — `1■T ITWAS JUNK  <BT> TIL ■VE MY E TOE9■ B  T UST FB TLIN` | **43 — `5TT IT AS JUNK <BT>  STILLHVE MY E TO 91B ETT USTFB TUBELI■`** |
| `cw-2026-08-20-014854` (no keying) | 0 | **0** |
| `cw-2026-08-20-014935` (no keying) | 0 | **0** |

**`STILL`, `HVE MY`, `91B` and `TUBELI` are three of the four anchors the order
names**, and the bulletin line is legible where it was fragments. **And both
recordings holding no keying stay silent**, which is the half that decides whether
any of it is worth having.

**At 40 ms rather than 50 it is not silent**: `cw-2026-08-20-014854` emits four
characters. Fifty is the width that reads more and invents nothing, and forty is
one notch away from breaking §0.0.

Seven tests record all of this, in
`WhatBandwidthTheDecoderListensThroughTests`. Nothing in `src` reads them and
nothing was changed on their strength. **Task 4 is answered inside them**: the
`003016` reading is printed and no answer key was written for it (§12.5).

**Task 3 did not run**, because it is gated on task 2 landing and task 2 did not
land. One thing worth saying about it anyway: **Hamlet's gate does not use a
percentile threshold at all.** It tracks a noise floor and a peak and decides
halfway between them, capped six decibels below the peak. The 10th-to-90th
percentile midpoint the order describes is `KeyingEnvelope.Measure`'s threshold —
the meter again, not the decoder.

**No decision was recorded under §12.1.**

### Three of the four recordings in the table are not in the tree

`cw-2026-08-21-015834`, `-020033` and `-015432` do not exist in the repository or
on this machine. `cw-2026-08-18-003016` does, and everything above that concerns
it is reproduced. This is the third order in a row naming captures that are not
committed.

## 2. What Tim should expect

### Did narrowing it, on its own, improve what Hamlet reads?

**No — every narrowing that can be expressed as a change to the tree made the
decoder invent characters on audio holding nothing** — but holding the detection
window at 50 ms in a harness took `cw-2026-08-18-004507` from 25 characters of
fragments to 32 of legible bulletin and `cw-2026-08-18-003016` from 38 to 43
carrying `STILL HVE MY E TO 91B` and `TUBELI`, with both empty recordings still
silent, **which is worth a design change rather than a constant.**

### What is different in the app

**Nothing.** No source file changed. This session measured and wrote down what it
measured.

### What will look wrong and is not

**2,152 tests, five failing**, and they are the five the order names — which is
the first order to name them correctly:

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`
- `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`

Build clean, no warnings. Seven tests added, all green.

Pushed to `main`.

## 3. What we should do next

- **Separate the survey window from the detection window.** That is the change the
  measurement points at, and it is the only way to have a 30 Hz detector without a
  30 Hz search. The survey runs every hundred hops on a decimated grid and asks a
  different question from the gate; there is no reason they must share a taper.
- **The element-floor ask still stands.** The order asked whether task 2 would make
  it unnecessary. Task 2 did not land, so it does not, and the short runs are still
  arriving at the estimator.
- **The fitted speed is still the thing everything hangs off.** Eight of nine
  recordings read 22 to 56 words a minute on senders working near fourteen, and
  that number picks the filter, the vote window and every boundary. Two asks about
  it are already outstanding.

## 4. What's blocking us

**Nothing blocks the next unit**, but the change the evidence points at is a design
change rather than a constant, and it is not a session's to make.

**Two asks, both new this session.**

> **May the coarse survey and the detection filter stop sharing an analysis
> window?**
>
> Holding the detection window at 50 ms reads `NET EAC5 STATION HANDLING HIS
> ESSAGEP` off `cw-2026-08-18-004507`, against 25 characters of fragments today,
> and `STILL HVE MY E TO 91B ETT USTFB TUBELI` off `cw-2026-08-18-003016` against
> 38. **Both recordings holding no keying stay silent at that width**, so it does
> not cost HM-DEC-120.
>
> It cannot be had by changing a constant. Every version tried breaks something:
> making the window follow the proved speed leaves it at 38 Hz while unproved and
> `cw-2026-08-20-014854` then emits four characters; narrowing the unproved
> default as well turns every station-finding test red, because the survey uses
> that same window and a longer taper is a narrower search.
>
> **Rejected as a session's own**: it is a change to how the receiver is built, it
> touches what the display asserts, and the one property it must not cost is the
> one three previous attempts at this have cost.

> **Three of the four recordings in the table are not in the tree.**
>
> `cw-2026-08-21-015834`, `-020033` and `-015432` are named in the instruction's
> own measurements and do not exist in the repository or on this machine. This is
> the third order in a row built partly on captures that were not committed.
> `cw-2026-08-18-003016` is here and every figure about it was reproduced.

### Asks still outstanding

- **Whether the coarse survey and the detection filter may stop sharing an
  analysis window.** First made 2026-08-20, this session. Waiting on Tim. Nothing
  is in the tree; the measurement is in
  `WhatBandwidthTheDecoderListensThroughTests`.
- **Three recordings named in the instruction are not in the tree.** First made
  2026-08-20, this session. Waiting on the files. Supersedes the same ask about
  six other recordings, which were also never committed.
- **Whether a clock fit may exclude runs below a share of its own unit, and in
  which instrument.** First made 2026-08-20. Waiting on Tim. **Task 2 did not
  land, so this is not made moot by it.**
- **Whether a mark too short to be an element may be set aside before the
  decoder's clock is fitted, at the cost of HM-DEC-120's zero-invention
  property.** First made 2026-08-20. Waiting on Tim.
- **Whether the unit may still be averaged with key-up gaps.** First made
  2026-08-20. Waiting on Tim. Removing `Refine` puts `013347` at 100.0 ms against
  a hand-read 100.4 and turns thirteen tests red.
- **The speed control needs an entry in `DECISIONS.md` and an id.** First made
  2026-08-20. Waiting on Tim. The code is on `main`.
- **The keying meter's provisional thresholds**, including
  `CwKeyingThresholds.ConfidentSwingDb` at 20 dB. First made 2026-08-20. Waiting
  on one evening's roster scored against the `meter` column.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam between two sends measured into the
  dummy load.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load, including the link pulled
  mid-cycle. The cycle is built and is dummy-load only.
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it. Waiting on its own
  work order.
- **HM-OPEN-007.** Open and unruled since 2026-08-14, named in HM-DEC-140 as the
  reason the queue's own premise is worth re-testing. Waiting on Tim.

**Nothing leaves the queue this session.**
