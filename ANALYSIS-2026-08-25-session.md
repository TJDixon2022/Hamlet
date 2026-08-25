# The 2026-08-25 session, measured end to end — and what to fix next

Thirteen captures, 0115–0219 UTC, all 40 m, all −13.3 dBFS input, analysed
independently (separate Goertzel, 30 ms Hann, 2.5 ms hop, Otsu threshold,
Schmitt trigger at ±6 dB, unit from `(dit + element gap)/2`). Every number
below is from the WAVs; nothing is from memory of the code.

## The whole evening in one table

| capture | tone meas. | `toneHz` | Δtone | WPM meas. | WPM rep. | duty | outcome |
|---|---|---|---|---|---|---|---|
| 011552 | 500.3 | 495.0 | 5.3 | 23.1 | 25 | 40% | callsign read, some soup |
| 012748 | 401.2 | 395.0 | 6.2 | 21.8 | (22) | **46%** | **2 chars — see Bug A** |
| 012823 | 499.8 | **450.0** | **49.8** | 22.5 | (32) | 43% | pure soup |
| 012922 | 492.3 | 490.0 | 2.3 | 21.5 | 22 | 39% | turning good |
| 013010 | 501.1 | 495.0 | 6.1 | 26.8 | 27 | 40% | full QSO |
| 013150 | 501.4 | 500.0 | 1.4 | 27.6 | (27) | 42% | good |
| 013303 | 501.4 | 500.0 | 1.4 | 28.0 | 27 | 42% | excellent — beat the independent chain |
| 013402 | 536.6 | 525.0 | 11.6 | 30.9 | 32 | 46% | excellent, 0 unsure |
| 013520 | 536.8 | 540.0 | 3.2 | 30.8 | 32 | 43% | excellent, 1 unsure |
| 013637 | 536.2 | 525.0 | 11.2 | 30.6 | 30 | 47% | excellent, 0 unsure; spacing errors |
| 021410 | 540.7 | 540.0 | **0.7** | 18.2 | 18 | 38% | good; in-character cut errors |
| 021629 | 504.7 | 510.0 | 5.3 | 20.8 | (21) | **24%** | real `559 559 IN MI MI` buried in noise |
| 021825 | 394.0 | 395.0 | **1.0** | 17.9 | (10, withdrawn) | **18%** | one 8-s call (`KC1UEK`) buried in noise |

Parenthesised speeds are `not proved` / `withdrawn` hypotheses — correct honesty
in every case.

## What is proven fixed — protect it

- **Tone.** Median error 5.3 Hz; three captures inside 1.4 Hz; best 0.7 Hz.
  A week ago this was a 25 Hz grid reporting 300 Hz on a 499.9 Hz carrier.
- **Speed.** Within 2 WPM whenever the tone is within ~12 Hz, from 18 to 31 WPM,
  and the grid now runs to 40.
- **Honesty plumbing.** `not proved`, `withdrawn`, `spanLlr`, `competing` all
  shipped and all say true things about the decoder's own state.
- **Output at 38–47% duty:** rag chews read end to end, twice at 0 unsure, and
  on 013303 Hamlet out-read the independent chain.

These thirteen files with their sidecars are the regression floor. Bank them
before changing anything.

## The one law the evening obeys

Sort the table by duty and the outcome sorts itself:

```
38–47% duty  ->  readable, 0–8 unsure       (10 captures)
24% duty     ->  real content buried in 48 noise characters
18% duty     ->  8 seconds of station, 22 seconds of invented text
```

Same input level, same bands, tone locked to a few hertz in every case. **The
decoder's remaining failure on tonight's evidence is not decoding signals —
it is that nothing stops it decoding silence.** On a rag chew the silence is
short and the damage invisible; on a calling or lightly-used frequency the
silence is most of the file and the output is mostly invented.

Everything below follows from that.

## Fix 1 — the confidence gate, now with three decisive fixtures

`spanLlr` already contains the answer. Measured tonight:

- **021825**: real characters score `T:6234, T:4798, T:765, K:712`; the noise
  scores `E:0.9, E:1.8, I:4.1`; and three characters were emitted at
  **−93.7, −152.4 and −594.8**. A character at minus five hundred and
  ninety-five is the decoder emitting a letter its own evidence says is
  strongly absent.
- **021629**: gate at per-element LLR ≥ 50 removes 48 characters — every one an
  E, I, S or ■ — and keeps every character of the real
  `T GE OT … R F R T 559 559 IN MI MI`.
- **013010** (the control from the good hour): the same gate suppresses only
  29 E's, 7 blocks and one T from 146 characters, and no real word is damaged.

**Normalise by element count before thresholding.** Median LLR by character
length, measured on 013010: 1 element → 40, 2 → 225, 3 → 254, 4 → 446,
5 → 812. An E is one dit and can never accumulate what a 7 does; a flat gate
turns `NICE` into `NIC`. Per-element threshold ~25–50 preserves `NICE`,
`MEET`, `CALL ES` and still removes the soup.

Suppressed characters become nothing (or `■` where an element genuinely
sounded); §0.0 ranks a marked unknown above a wrong letter, and the number
that makes the distinction is already printed in every sidecar.

Fixtures: **021825** (nearly all noise), **021629** (buried but recoverable),
**013010** and **013520** (controls that must not lose real words).

## Fix 2 — protect the clock from silence

Two captures show the clock being dragged by noise, not by signal:

- **012823**: hypothesis 32 WPM on a 22.5 WPM station — the one soup capture of
  the QSO hour.
- **021825**: hypothesis 10 WPM on a 17.9 WPM station — fitted to the gaps
  between an 8-second transmission.

The speed estimator currently feeds on everything. **Let it update only from
spans that pass Fix 1's gate** (or during high-swing stretches). A clock that
learns only from evidence cannot be pulled to 32 by hiss, and 012823's
tone-plus-clock double failure stops being reachable. The
withhold-while-reacquiring behaviour (HM-OPEN-022) is untouched — this changes
what the clock eats, not what it reports.

## Fix 3 — the boundary cuts on clean signals

With tone and clock right, the dominant *residual* on good signals is cuts in
the wrong places, and tonight brackets it from both ends:

- **021410, 18.2 WPM, machine-grade fist** (dit spread 4 ms; gap clusters
  53 / 221 / 913 ms — 0.81u / 3.36u / 13.9u with nothing between): still
  produced `ATEEKEND` (W cut into A-T-E), `TTHINKING`, `FLENX`, `XMT R`. The
  classes are perfectly separable and the cutter still cut inside characters.
  **This is a decision-rule fault, not a signal fault.**
- **013637, 30.6 WPM**: gap clusters collapse to 24 / 28 / 171 ms — the element
  and character gaps are four milliseconds apart and **no longer separable by
  clustering at all**. Result: `AB OVE`, `BREE Z E`, `TEN TE C` — letters right,
  spacing wrong.

So the two speeds need the same fix from opposite directions: at 18 WPM the
information is there and the rule ignores it; at 30 WPM no per-gap rule can
work because single gaps genuinely overlap. **The cut decision has to stop
being local.** The candidate that covers both: score cut/no-cut jointly over a
short window against character validity and the fitted clock (a small dynamic
program over the element stream), rather than thresholding each gap in
isolation. That is a design change, not a parameter, and worth an options
table before building. What is *not* worth building: more cluster tuning —
013637 proves the clusters merge at speed.

Fixtures: **021410** (separable, still miscut) and **013637** (not separable).

## Fix 4 — pick the pitch by fist quality, not energy alone

012823's 50 Hz miss is the only tone failure of the night, and 021629 shows
the robust selection rule. Sweeping candidate pitches on 021629 and scoring
each for *keying quality* (dit:dah ratio 2.4–3.6, duty 18–55%):

```
485–540 Hz : one station — ratio 2.7–3.0, duty 24–31%   CLEAN
545–620 Hz : ratio 4+, duty 62–76%                       not a station
```

A clean-fist score separates a real station from a mush of neighbours even
when their energies are comparable. The survey already admits keying per bin;
adding the ratio-and-duty test to the bin choice would have prevented 012823
and costs one pass over data already computed.

## Fix 5 — the instruments that are still lying

- **The keying sweep is now wrong on 14 of 20 captures**, including
  `no keying at 550 Hz` on 021410 — 37 characters emitted, tone 540.7,
  38% duty, 79/212 ms elements — and `no keying at 600 Hz` on 021825 while the
  real station sat at 394. Its `4–7 ms key down` medians remain arithmetically
  impossible for Morse. Fix it against these fixtures or remove it from the
  screen; it is the advice the operator sees precisely when the decoder is
  silent, and it points at the radio.
- **`competing: none found` in all thirteen sidecars**, including 021629 where
  the 545–620 Hz region carries energy within 2.4 dB of the tracked station.
  Whatever it measures, it does not find what the spectrum plainly shows.
- **Duty belongs in the sidecar.** It predicted every outcome tonight, it is
  one number, and the envelope it comes from already exists.

## Order

1. Bank all thirteen captures with floors (the zip is packaged for this).
2. Fix 1 — the gate. Largest win, data already computed, fixtures decisive.
3. Fix 2 — the clock's diet. Small change, removes the double-failure mode.
4. Fix 4 — pitch by fist quality. One pass over existing survey data.
5. Fix 3 — the joint cutter. Design decision first; options table if it
   threatens anything ruled.
6. Fix 5 — sweep, competing, duty.

## The caution that applies to all of it

Ten of tonight's thirteen captures are two operators on one frequency in one
hour. The gate thresholds, the fist-quality bounds and the duty law were
measured on them plus three quiet-frequency files. **Mechanisms should
generalise; the constants should be re-measured against the synthetic corpus,
the W1AW seven and the KD0UN capture before they harden into code.** And the
floors from the good hour exist precisely so that fixing the quiet-frequency
case cannot quietly cost the rag-chew case: 013520 (59 chars, 1 unsure) and
013303 (beat the independent chain) must never read worse than they read
tonight.
