READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 2 of 5, 8 0 of 6, 9 4 of 8; steps 2 to 8 still barred by R86,
   and 9.4 open with every mechanism 9.3 named refused.
B. Step 9, criterion 9.5: HM-REQ-124 - ours' p from its rival margin
   (MarginLlr, as sign(m) ln(1 + |m|)); the port's p from its level
   margin 20 log10(sig_avg / noise_floor) (cw.cxx:610, 612-616, 635-636)
   and its timing margin |element - two_dots| / (two_dots / 2)
   (cw.cxx:811-812, 847, 502); held-out by recording, ten bins,
   5 points. Ours is calibrated on real HF all, real sender-not-stated,
   synthetic all and synthetic TX-ITU 15 dB. It is not calibrated on
   real TX-FARNS or synthetic TX-ITU 5 dB. It is not measurable on real
   TX-ITU, real TX-TIGHT, synthetic TX-ITU 0 dB and synthetic gap-5 at
   0, 5 and 15 dB. The port is calibrated on none. It is not calibrated
   on real HF all, real TX-FARNS, real sender-not-stated, synthetic all
   and synthetic TX-ITU 15 and 5 dB. It is not measurable on the same
   six as ours. In-sample against held-out gap: 0.49 points per
   character for ours, 1.17 for the port. Every text and class
   byte-identical: yes. The port byte-identical: yes. 9.5 ticked.
C. The findings weighed against A and B: section 4 raises 5 items.
   Item 3 bears on 9.6. Under HM-REQ-124 the port is calibrated on no
   condition, so it is advisory everywhere, and a vote built today
   could never let it win a disagreement. Ours' p is calibrated where
   it is mostly because it barely moves (0.77 to 0.99). That tells 9.6
   little about which of its letters to doubt. Neither stops 9.6 from
   being built. Item 1 is the standing reading: R86 with 9.4's refusals
   holds steps 2 to 8, and only the owner can change how they read. It
   is not a stop here. Nothing touches transmit or what the operator
   is shown.

UNIT:       464 - complete at task 4 of 4, none dropped - 2026-09-27 03:59
PHASE GOAL: Hamlet's CW decoding meets every requirement in CW_REQUIREMENTS.md at the condition each names, each proved by a test that names it. Until one fldigi technique is kept in ours, R86 routes the loop through step 9's own lines.
UNIT GOAL:  Give every character either decoder prints a number, the chance it is right. Ours takes its number from how far its reading beat the nearest rival; the port's comes from fldigi's own signal-over-noise and timing, read without touching the port. Then measure, condition by condition and on recordings the fit never saw, whether "90%" means right 90% of the time. No letter or class changes.
ADVANCED:   yes - 9.5 is ticked in both copies. Both decoders carry a p on every character (EveryCharacterCarriesAConfidenceTests; 0 of 1683 characters differ from the map), calibration is measured held-out per condition, the conditions are named, and the port's derivation is stated.
NUMBER:     HM-REQ-124 met on 4 of 12 conditions for ours and 0 of 12 for the port (6 of 12 not measurable for each). Held-out share right against mean p: ours 0.924 against 0.918 (real, inferred), 0.919 against 0.933 (synthetic, exact); the port 0.741 against 0.769 (real, inferred), 0.798 against 0.756 (synthetic, exact). Texts byte-identical: yes.
DRIFT:      step 2 1; step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 0 (was 3)

## 1. What Claude did

**Complete, at task 4 of 4, none dropped.** QUIVERFULL, Hamlet confirmed by the gate (all six
checks held), branch `main`. The session found `SESSION.lock` already there (PID 37056, 01:57:15).
The lock is the launcher's; the session did not take it or release it.

**Tree against the instruction.** HEAD was `99c90b69`. Both copies of `PHASE_PLAN.md` were
identical, with 9.1 to 9.3 ticked, 9.4 to 9.8 open and R86 at line 342. The port matched
`19109b51`. `CwCharacter` carried `Confidence` and `MarginLlr` and no numeric probability. Unit
463's saves were all present. There are three mismatches:
- **No `.run-unit\watched.rc`.** The instruction says it is now present. It is not.
- **`watched.cpu`.** Git status showed it deleted at the start. The runner then rewrote it, and it
  was committed at task 0 as the runner left it. Task 0's commit message still says "deleted",
  which was out of date by the time of the commit.
- **The port's key events cannot be ordered from `Emissions` and `KeyEvents` alone.** The
  instruction says the adapter takes those two. The port's filter hands out 1024 samples at once,
  so an emission and the next character's first key events can carry the same `InputSample`. On
  the first trace this left 44 port characters with no key events found. Only the decision rows,
  with `TraceDecisions` on, give the true order. `TraceDecisions` is public and records only.
  `FldigiConfidence` therefore takes the decision rows as well, and refuses a run made without
  them. No line under `Cw/Second/` changed.

Logged and not this unit's: the `RULES_AT` disagreement, and `outcome-read`'s step titles.

**Task 0.** I recorded the unit (`## UNIT 464 - STEP 9`, `PHASE_STATUS` in both copies, 1.13.151)
and committed the runner's writes as they were. The entry round ran, and every figure matched
463's exit (table in section 3). Both decoders' texts and ours' classes were saved.

**Task 1: the trace, and the map fixed.** `WhatEachDecoderKnowsAboutEachCharacterFact` (HM-REQ-124)
prints one row per emitted character, for both decoders, on all 35 recordings, with the scorer's
verdict. Its counts equal `parity.md`'s:
- ours: 609 scored (562 right, 36 wrong, 11 added), all sure, since ours emits no dim;
- the port: 407 scored (311 right, 94 wrong, 2 added);
- 9 characters are covered by two stretches and are scored once per stretch, as the metrics do.

`MarginLlr` is never NaN. It is +Infinity on 97 short letters, 38 of them scored, most of which
have no rival reading. Both port features read on every character. The map, fixed from the
separation print before any calibration number:
- **both:** one logistic per decoder, pooled over both sets, fitted by Newton's method to right
  against wrong or added;
- **ours:** x = sign(m) ln(1 + |m|). The raw margin runs to the hundreds and to infinity; the
  compression is monotone. A non-finite margin takes a constant fitted on those characters alone.
- **the port:** linear in level dB and timing dits.

**Task 2: p attached.**
- `CwCharacter.Probability` is set by the stream at the line where `MarginLlr` is set, through
  `CwCharacterProbability.Of`.
- `FldigiConfidence` sits under `Cw\` and derives the port's p as above.
- The constants are the whole pool's fit. `TheShippedConstantsAreTheWholePoolFit` checks every
  character of both decoders against the map, and 0 of 1683 differ.
- Every text and class, the port's texts, all 35 V-11 rows and the four metrics are byte-identical
  to task 0.

**Task 3: measured.** `CwCalibration` is new, beside `CwMetrics`. `EachDecodersConfidenceIsMeasuredTests`
fits on 34 recordings and measures the 35th, all 35 times. It writes
`docs/phase-requirements/calibration.md`, and `metrics.md` now points at it. MET-CAL was not
dropped. 9.5 was ticked in both copies in a follow-on commit, and nothing else was ticked.

**Task 4: the exit round.** Every figure is as at entry (section 3).

**Decisions made for myself:**
- **The margin's compression and the +Infinity rule.** Both were forced by the separation print:
  the instruction's option for a NaN margin, applied to +Infinity too.
- **Task 1's commit is two commits.** My commit helper staged only `.run-unit` files, so the fact
  itself went in `d1abc6cb` right after `12252dcc`. I did not force-push main.
- **An overwritten save, restored.** During task 2 a wrong V-11 suffix overwrote
  `unit464-v11-before.txt` with an empty file. I restored it exactly as task 0 had made it (a copy
  of the entry rows); git shows it unchanged from task 0's commit.
- **The dispatcher loop hit the app line at every gate.** It took a different test each time.
  DECIDED (8)'s rerun was used each time; wherever the rerun lost a type too, that type passed alone.
- **Task 3's gate.** Task 3's commit adds only a test type, which was already in the tree when
  task 2's gate ran, and two documents no test reads. I rebuilt for it and did not rerun the five.
  The exit round ran them all on the final tree.
- **The hand-built case was not watched failing.** Case 2 of `EveryCharacterCarriesAConfidenceTests`
  was written against the finished measure. Only case 1 was watched failing (section 3).

## 2. What the owner should expect

Nothing on the screen changes. Every letter either decoder reads now carries a hidden number,
Hamlet's own estimate of the chance that the letter is right. If you could see it:
- **Ours, on the real recordings as a whole and on the clean 15 dB synthetic signal:** "92% sure"
  really did come out right about 92% of the time.
- **Ours on the traffic-net sender (TX-FARNS) and the 5 dB synthetic signal:** it undersold
  itself. It said about 90% and was right nearly every time.
- **Ours, the catch:** its number hardly moves, between 77% and 99%. It is honest mostly because
  it says roughly the same thing about every letter, so it does not yet single out the letters
  you should doubt.
- **The fldigi port:** its number was not honest on any condition. On the real recordings it said
  about 77% and was right 74% of the time, but its "84%" letters were right only 74%. On the
  synthetic signals it undersold its good letters and oversold its bad ones.

The requirement says a decoder whose number is not honest on a condition only advises there. So
as things stand, the port could advise but never win an argument with ours. Most of the finer
conditions (single senders, the 0 dB and wide-gap synthetic cases) have too few letters to judge
either way.

## 3. What you should see

No visible change. This unit gives each decoder a number the next step (9.6) needs before either
may vote.

**The calibration table, held-out by recording (the verdict) with in-sample beside it.** "Worst" is
the populated bin (10 or more characters) furthest from its mean p, in points, share right less
mean p. Real keys are inferred and synthetic keys exact.

| condition | key | decoder | scored | mean p held-out / in-sample | share right | worst bin held-out / in-sample | verdict (held-out) |
|---|---|---|---|---|---|---|---|
| real HF, all | inferred | ours | 436 | 0.918 / 0.919 | 0.924 | +0.8 / +1.1 | calibrated |
| real HF, all | inferred | port | 239 | 0.769 / 0.768 | 0.741 | +10.9 / +13.7 | not calibrated |
| real, TX-FARNS | inferred | ours | 43 | 0.936 / 0.941 | 1.000 | +5.8 / +5.3 | not calibrated |
| real, TX-FARNS | inferred | port | 33 | 0.731 / 0.747 | 0.909 | +11.1 / +14.7 | not calibrated |
| real, TX-ITU | inferred | ours | 13 | 0.914 / 0.916 | 1.000 | +7.9 / +7.9 | not measurable |
| real, TX-ITU | inferred | port | 6 | 0.719 / 0.720 | 0.833 | none / none | not measurable |
| real, TX-TIGHT | inferred | ours | 6 | 0.858 / 0.863 | 1.000 | none / none | not measurable |
| real, TX-TIGHT | inferred | port | 3 | 0.891 / 0.882 | 0.333 | none / none | not measurable |
| real, sender not stated | inferred | ours | 374 | 0.918 / 0.918 | 0.912 | -1.1 / -1.7 | calibrated |
| real, sender not stated | inferred | port | 197 | 0.775 / 0.772 | 0.716 | -12.8 / +12.0 | not calibrated |
| synthetic, all | exact | ours | 173 | 0.933 / 0.932 | 0.919 | +3.7 / +5.3 | calibrated |
| synthetic, all | exact | port | 168 | 0.756 / 0.758 | 0.798 | +13.0 / -21.0 | not calibrated |
| synthetic TX-ITU 0 dB | exact | ours | 0 | - | - | none | not measurable |
| synthetic TX-ITU 0 dB | exact | port | 27 | 0.664 / 0.631 | 0.222 | -33.4 / -45.0 | not measurable |
| synthetic TX-ITU 15 dB | exact | ours | 64 | 0.964 / 0.966 | 0.984 | +2.0 / +1.9 | calibrated |
| synthetic TX-ITU 15 dB | exact | port | 56 | 0.848 / 0.857 | 0.964 | +11.9 / +12.4 | not calibrated |
| synthetic TX-ITU 5 dB | exact | ours | 63 | 0.894 / 0.899 | 0.984 | +10.4 / +9.6 | not calibrated |
| synthetic TX-ITU 5 dB | exact | port | 47 | 0.691 / 0.701 | 0.894 | +21.9 / +21.1 | not calibrated |
| synthetic gap 5, 0 dB | exact | ours | 0 | - | - | none | not measurable |
| synthetic gap 5, 0 dB | exact | port | 3 | 0.653 / 0.648 | 0.333 | none / none | not measurable |
| synthetic gap 5, 15 dB | exact | ours | 21 | 0.969 / 0.968 | 0.952 | -1.6 / -1.6 | not measurable |
| synthetic gap 5, 15 dB | exact | port | 19 | 0.849 / 0.858 | 0.947 | +11.8 / +11.2 | not measurable |
| synthetic gap 5, 5 dB | exact | ours | 25 | 0.919 / 0.898 | 0.560 | -31.4 / -33.5 | not measurable |
| synthetic gap 5, 5 dB | exact | port | 16 | 0.692 / 0.695 | 0.813 | +19.0 / +18.6 | not measurable |

The synthetic set is ours' only row where the held-out and in-sample verdicts differ. The
in-sample bin 8 sits at +5.3 points, just past the line.

**How much the fit learned the corpus.** A character's held-out p differs from its in-sample p by:
- ours: 0.49 points on average, 5.49 at most;
- the port: 1.17 points on average, 9.79 at most.

**1. Task 1's separation print, and the constants.** Scored characters by quintile of the feature,
both sets pooled (right / wrong / added, right share). The full print, per set and with NaN and
infinite counts, is in `.run-unit/unit464-features.txt`.

| decoder | feature | Q1 | Q2 | Q3 | Q4 | Q5 |
|---|---|---|---|---|---|---|
| ours | MarginLlr | -0.61 to 2.76: 102/16/3, 0.843 | to 7.93: 110/7/5, 0.902 | to 23.7: 115/6/1, 0.943 | to 68.2: 119/1/2, 0.975 | to +Inf: 116/6/0, 0.951 |
| port | level dB | -8.66 to 3.81: 39/42/0, 0.481 | to 5.73: 71/9/1, 0.877 | to 7.53: 60/21/1, 0.732 | to 9.81: 62/19/0, 0.765 | to 26.7: 79/3/0, 0.963 |
| port | timing dits | 0.02 to 0.59: 55/26/0, 0.679 | to 0.82: 69/12/0, 0.852 | to 0.92: 74/8/0, 0.902 | to 0.99: 66/15/0, 0.815 | to 1.87: 47/33/2, 0.573 |

Infinite `MarginLlr`: 97 rows, 38 scored, 35 right. The patterns are `.`, `-`, `..`, `-...--` and
`.--`, and most have no rival reading. No NaN on either decoder once the port's events were
ordered by the decision rows.

Constants, the whole pool's fit:
- **ours:** a = 1.4046693133246466, b = 0.47470729666229566 (571 characters); non-finite
  0.9210526315789473 (35 of 38).
- **the port:** a = 0.37776906958829465, b (level) = 0.18544731603622516, c (timing) =
  -0.40029810858361803 (407 characters); unreadable 0.7641277641277642, the pool's share right,
  since none was unreadable.

**2. The full held-out reliability tables per condition** (from `calibration.md` section 3):


Ten bins of p; empty bins left out; `(under 10)` marks a bin that does not count toward the verdict.

**ours, real HF, all** (inferred keys; 436 scored; held-out calibrated, in-sample calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 7 | 0.7 | 1 | 0.779 | 1.000 | +22.1 (under 10) | 0 | - | - | none populated |
| 8 | 0.8 | 147 | 0.870 | 0.871 | +0.1 | 148 | 0.871 | 0.865 | -0.6 |
| 9 | 0.9 | 288 | 0.944 | 0.951 | +0.8 | 288 | 0.944 | 0.955 | +1.1 |

**ours, real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net)** (inferred keys; 43 scored; held-out not calibrated, in-sample not calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 8 | 0.8 | 2 | 0.817 | 1.000 | +18.3 (under 10) | 2 | 0.819 | 1.000 | +18.1 |
| 9 | 0.9 | 41 | 0.942 | 1.000 | +5.8 | 41 | 0.947 | 1.000 | +5.3 |

**ours, real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture)** (inferred keys; 13 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 8 | 0.8 | 3 | 0.890 | 1.000 | +11.0 (under 10) | 2 | 0.888 | 1.000 | +11.2 |
| 9 | 0.9 | 10 | 0.921 | 1.000 | +7.9 | 11 | 0.921 | 1.000 | +7.9 |

**ours, real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101)** (inferred keys; 6 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 8 | 0.8 | 6 | 0.858 | 1.000 | +14.2 (under 10) | 6 | 0.863 | 1.000 | +13.7 |

**ours, real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md** (inferred keys; 374 scored; held-out calibrated, in-sample calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 7 | 0.7 | 1 | 0.779 | 1.000 | +22.1 (under 10) | 0 | - | - | none populated |
| 8 | 0.8 | 136 | 0.871 | 0.860 | -1.1 | 138 | 0.872 | 0.855 | -1.7 |
| 9 | 0.9 | 237 | 0.945 | 0.941 | -0.4 | 236 | 0.944 | 0.945 | +0.1 |

**ours, synthetic, all** (exact keys; 173 scored; held-out calibrated, in-sample not calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 7 | 0.7 | 0 | - | - | none populated (under 10) | 2 | 0.766 | 0.000 | -76.6 |
| 8 | 0.8 | 42 | 0.868 | 0.905 | +3.7 | 42 | 0.875 | 0.929 | +5.3 |
| 9 | 0.9 | 131 | 0.953 | 0.924 | -3.0 | 129 | 0.953 | 0.930 | -2.3 |

**ours, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 0 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|

**ours, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 64 scored; held-out calibrated, in-sample calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 9 | 0.9 | 64 | 0.964 | 0.984 | +2.0 | 64 | 0.966 | 0.984 | +1.9 |

**ours, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 63 scored; held-out not calibrated, in-sample not calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 8 | 0.8 | 38 | 0.870 | 0.974 | +10.4 | 36 | 0.876 | 0.972 | +9.6 |
| 9 | 0.9 | 25 | 0.929 | 1.000 | +7.1 | 27 | 0.930 | 1.000 | +7.0 |

**ours, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 0 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|

**ours, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 21 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 9 | 0.9 | 21 | 0.969 | 0.952 | -1.6 | 21 | 0.968 | 0.952 | -1.6 |

**ours, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 25 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 7 | 0.7 | 0 | - | - | none populated (under 10) | 2 | 0.766 | 0.000 | -76.6 |
| 8 | 0.8 | 4 | 0.849 | 0.250 | -59.9 (under 10) | 6 | 0.871 | 0.667 | -20.4 |
| 9 | 0.9 | 21 | 0.933 | 0.619 | -31.4 | 17 | 0.923 | 0.588 | -33.5 |

**port, real HF, all** (inferred keys; 239 scored; held-out not calibrated, in-sample not calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 1 | 0.1 | 1 | 0.146 | 1.000 | +85.4 (under 10) | 0 | - | - | none populated |
| 2 | 0.2 | 2 | 0.255 | 1.000 | +74.5 (under 10) | 3 | 0.246 | 0.667 | +42.1 |
| 3 | 0.3 | 2 | 0.330 | 0.000 | -33.0 (under 10) | 3 | 0.357 | 0.333 | -2.4 |
| 4 | 0.4 | 5 | 0.451 | 0.600 | +14.9 (under 10) | 4 | 0.481 | 0.500 | +1.9 |
| 5 | 0.5 | 7 | 0.541 | 0.714 | +17.3 (under 10) | 7 | 0.553 | 0.571 | +1.8 |
| 6 | 0.6 | 27 | 0.669 | 0.778 | +10.9 | 26 | 0.671 | 0.808 | +13.7 |
| 7 | 0.7 | 83 | 0.764 | 0.747 | -1.7 | 85 | 0.763 | 0.729 | -3.4 |
| 8 | 0.8 | 106 | 0.843 | 0.736 | -10.7 | 102 | 0.838 | 0.755 | -8.4 |
| 9 | 0.9 | 6 | 0.920 | 0.833 | -8.7 (under 10) | 9 | 0.914 | 0.889 | -2.5 |

**port, real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net)** (inferred keys; 33 scored; held-out not calibrated, in-sample not calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 1 | 0.1 | 1 | 0.146 | 1.000 | +85.4 (under 10) | 0 | - | - | none populated |
| 2 | 0.2 | 0 | - | - | none populated (under 10) | 1 | 0.207 | 1.000 | +79.3 |
| 4 | 0.4 | 1 | 0.448 | 1.000 | +55.2 (under 10) | 0 | - | - | none populated |
| 5 | 0.5 | 1 | 0.585 | 1.000 | +41.5 (under 10) | 1 | 0.514 | 1.000 | +48.6 |
| 6 | 0.6 | 1 | 0.666 | 1.000 | +33.4 (under 10) | 1 | 0.638 | 1.000 | +36.2 |
| 7 | 0.7 | 23 | 0.758 | 0.870 | +11.1 | 23 | 0.766 | 0.913 | +14.7 |
| 8 | 0.8 | 6 | 0.806 | 1.000 | +19.4 (under 10) | 7 | 0.813 | 0.857 | +4.4 |

**port, real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture)** (inferred keys; 6 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 6 | 0.6 | 1 | 0.673 | 1.000 | +32.7 (under 10) | 1 | 0.674 | 1.000 | +32.6 |
| 7 | 0.7 | 5 | 0.728 | 0.800 | +7.2 (under 10) | 5 | 0.729 | 0.800 | +7.1 |

**port, real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101)** (inferred keys; 3 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 7 | 0.7 | 1 | 0.793 | 0.000 | -79.3 (under 10) | 1 | 0.783 | 0.000 | -78.3 |
| 8 | 0.8 | 1 | 0.883 | 1.000 | +11.7 (under 10) | 1 | 0.870 | 1.000 | +13.0 |
| 9 | 0.9 | 1 | 0.996 | 0.000 | -99.6 (under 10) | 1 | 0.992 | 0.000 | -99.2 |

**port, real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md** (inferred keys; 197 scored; held-out not calibrated, in-sample not calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 2 | 0.2 | 2 | 0.255 | 1.000 | +74.5 (under 10) | 2 | 0.265 | 0.500 | +23.5 |
| 3 | 0.3 | 2 | 0.330 | 0.000 | -33.0 (under 10) | 3 | 0.357 | 0.333 | -2.4 |
| 4 | 0.4 | 4 | 0.451 | 0.500 | +4.9 (under 10) | 4 | 0.481 | 0.500 | +1.9 |
| 5 | 0.5 | 6 | 0.534 | 0.667 | +13.3 (under 10) | 6 | 0.560 | 0.500 | -6.0 |
| 6 | 0.6 | 25 | 0.669 | 0.760 | +9.1 | 24 | 0.672 | 0.792 | +12.0 |
| 7 | 0.7 | 54 | 0.770 | 0.704 | -6.6 | 56 | 0.765 | 0.661 | -10.4 |
| 8 | 0.8 | 99 | 0.845 | 0.717 | -12.8 | 94 | 0.840 | 0.745 | -9.5 |
| 9 | 0.9 | 5 | 0.905 | 1.000 | +9.5 (under 10) | 8 | 0.904 | 1.000 | +9.6 |

**port, synthetic, all** (exact keys; 168 scored; held-out not calibrated, in-sample not calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 2 | 0.2 | 3 | 0.262 | 0.000 | -26.2 (under 10) | 3 | 0.251 | 0.000 | -25.1 |
| 3 | 0.3 | 2 | 0.385 | 0.000 | -38.5 (under 10) | 2 | 0.359 | 0.000 | -35.9 |
| 4 | 0.4 | 5 | 0.467 | 0.400 | -6.7 (under 10) | 4 | 0.465 | 0.500 | +3.5 |
| 5 | 0.5 | 5 | 0.563 | 0.400 | -16.3 (under 10) | 8 | 0.567 | 0.375 | -19.2 |
| 6 | 0.6 | 25 | 0.648 | 0.560 | -8.8 | 27 | 0.655 | 0.444 | -21.0 |
| 7 | 0.7 | 57 | 0.729 | 0.860 | +13.0 | 57 | 0.733 | 0.877 | +14.5 |
| 8 | 0.8 | 58 | 0.875 | 0.931 | +5.6 | 30 | 0.880 | 1.000 | +12.0 |
| 9 | 0.9 | 13 | 0.906 | 1.000 | +9.4 | 37 | 0.909 | 1.000 | +9.1 |

**port, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 27 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 3 | 0.3 | 1 | 0.396 | 0.000 | -39.6 (under 10) | 1 | 0.371 | 0.000 | -37.1 |
| 4 | 0.4 | 2 | 0.481 | 0.500 | +1.9 (under 10) | 1 | 0.475 | 1.000 | +52.5 |
| 5 | 0.5 | 2 | 0.566 | 0.000 | -56.6 (under 10) | 5 | 0.572 | 0.200 | -37.2 |
| 6 | 0.6 | 13 | 0.642 | 0.308 | -33.4 | 15 | 0.650 | 0.200 | -45.0 |
| 7 | 0.7 | 4 | 0.761 | 0.000 | -76.1 (under 10) | 5 | 0.715 | 0.200 | -51.5 |
| 8 | 0.8 | 5 | 0.807 | 0.200 | -60.7 (under 10) | 0 | - | - | none populated |

**port, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 56 scored; held-out not calibrated, in-sample not calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 2 | 0.2 | 2 | 0.269 | 0.000 | -26.9 (under 10) | 2 | 0.248 | 0.000 | -24.8 |
| 5 | 0.5 | 2 | 0.589 | 1.000 | +41.1 (under 10) | 2 | 0.586 | 1.000 | +41.4 |
| 7 | 0.7 | 3 | 0.767 | 1.000 | +23.3 (under 10) | 2 | 0.757 | 1.000 | +24.3 |
| 8 | 0.8 | 38 | 0.881 | 1.000 | +11.9 | 20 | 0.876 | 1.000 | +12.4 |
| 9 | 0.9 | 11 | 0.907 | 1.000 | +9.3 | 30 | 0.909 | 1.000 | +9.1 |

**port, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 47 scored; held-out not calibrated, in-sample not calibrated)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 2 | 0.2 | 1 | 0.249 | 0.000 | -24.9 (under 10) | 1 | 0.258 | 0.000 | -25.8 |
| 4 | 0.4 | 3 | 0.458 | 0.333 | -12.4 (under 10) | 3 | 0.462 | 0.333 | -12.9 |
| 6 | 0.6 | 6 | 0.658 | 1.000 | +34.2 (under 10) | 5 | 0.664 | 1.000 | +33.6 |
| 7 | 0.7 | 37 | 0.727 | 0.946 | +21.9 | 38 | 0.737 | 0.947 | +21.1 |

**port, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 3 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 6 | 0.6 | 2 | 0.628 | 0.500 | -12.8 (under 10) | 3 | 0.648 | 0.333 | -31.4 |
| 7 | 0.7 | 1 | 0.701 | 0.000 | -70.1 (under 10) | 0 | - | - | none populated |

**port, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 19 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 3 | 0.3 | 1 | 0.374 | 0.000 | -37.4 (under 10) | 1 | 0.347 | 0.000 | -34.7 |
| 7 | 0.7 | 1 | 0.718 | 1.000 | +28.2 (under 10) | 1 | 0.722 | 1.000 | +27.8 |
| 8 | 0.8 | 15 | 0.882 | 1.000 | +11.8 | 10 | 0.888 | 1.000 | +11.2 |
| 9 | 0.9 | 2 | 0.902 | 1.000 | +9.8 (under 10) | 7 | 0.909 | 1.000 | +9.1 |

**port, synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference)** (exact keys; 16 scored; held-out not measurable, in-sample not measurable)

| bin | p from | held-out characters | held-out mean p | held-out share right | held-out points | in-sample characters | in-sample mean p | in-sample share right | in-sample points |
|---|---|---|---|---|---|---|---|---|---|
| 5 | 0.5 | 1 | 0.504 | 0.000 | -50.4 (under 10) | 1 | 0.506 | 0.000 | -50.6 |
| 6 | 0.6 | 4 | 0.664 | 0.750 | +8.6 (under 10) | 4 | 0.667 | 0.750 | +8.3 |
| 7 | 0.7 | 11 | 0.719 | 0.909 | +19.0 | 11 | 0.723 | 0.909 | +18.6 |


**3. MET-CAL, ours** (`CW_SPEC.md` section 11): three bins, over every stretch scored.

| class | set | key | in stretches | right | wrong | added | observed accuracy | stated rate |
|---|---|---|---|---|---|---|---|---|
| sure | real | inferred | 436 | 403 | 28 | 5 | 0.924 | asserted as sent (HM-REQ-013) |
| sure | synthetic | exact | 173 | 159 | 8 | 6 | 0.919 | asserted as sent (HM-REQ-013) |
| dim | real | inferred | 0 | 0 | 0 | 0 | no number: none emitted | at least 0.70 (HM-REQ-014) |
| dim | synthetic | exact | 0 | 0 | 0 | 0 | no number: none emitted | at least 0.70 (HM-REQ-014) |
| placeholder | real | inferred | 15 | - | - | - | unscored | unscored |
| placeholder | synthetic | exact | 2 | - | - | - | unscored | unscored |

**4. How `EveryCharacterCarriesAConfidenceTests` was watched failing first.** The test cannot compile
at the parent commit, where neither `Probability` nor `FldigiConfidence` exists. So case 1 was
watched red in the working tree first, with:
- the property added and defaulting to NaN;
- the stream not yet setting it;
- `FldigiConfidence`'s constants NaN.

It failed with "40 characters without a probability" on `cq-18wpm-15db`
(`.run-unit/unit464-confidence-red.txt`). Then the constants were written back and the stream line
added, and it passed 2 of 2 (`unit464-confidence-green.txt`). Case 2, the hand-built measure (100
at 0.9: 90 right calibrated, 80 right not, 29 not measurable), was written against the finished
`CwCalibration` and was not watched failing.

**5. The commits, with the five at each** (build; engine line; app line; named, captures and
adjudicated floors):

| commit | what | build | engine | app | named | captures | adjudicated |
|---|---|---|---|---|---|---|---|
| `b34f4f56` | task 0: record, runner's writes, entry | 0 errors | 178/178 | 278/278 (DECIDED (8): run and rerun each lost a different test; both types pass alone) | 13/13 | 51/51 | 13/13 |
| `12252dcc` | task 1: the printout and gate files | 0 errors | 178/178 | 278/278 (DECIDED (8): the rerun lost `BindingHealthTests`, which passes alone) | 13/13 | 51/51 | 13/13 |
| `d1abc6cb` | task 1: the fact itself, left out of `12252dcc` | as `12252dcc`, same tree | | | | | |
| `d3fb86fb` | task 2: the property, map, adapter, stream line, test, `CwCalibration` | 0 errors | 178/178 | 278/278 on the rerun | 13/13 | 51/51 | 13/13 |
| `efdd5d11` | task 3: the measure, `calibration.md`, `metrics.md` | 0 errors | task 2's gate: same code, only docs added | | | | |
| `b30bef48` | 9.5 ticked in both copies | plan text only | | | | | |
| task 4's commit | exit round, `output.md` | 0 errors | 178/178 | 278/278 on the rerun | 13/13 | 51/51 | 13/13 |

**The exit round beside the entry:**

| check | entry | exit |
|---|---|---|
| build | 0 errors | 0 errors |
| engine line | 178 of 178 | 178 of 178 |
| app line | 278 of 278 (DECIDED (8)) | 278 of 278 on the rerun (`TheTestsStayOffTheNetworkTests` lost to the dispatcher on the run) |
| floors | named 13/13, captures 51/51, adjudicated 13/13 | 13/13, 51/51, 13/13 |
| real, 23, inferred | CER-SURE 33 of 436, INVENTED 33 / 473, coverage 403 / 473, WBE 37 / 113 | the same |
| synthetic, 12, exact | 14 of 173, 14 / 252, 159 / 252, 44 / 84 | the same |
| the port, per parity.md | real 62 of 239, coverage 177; synthetic 34 of 168, coverage 134 | the same |
| `BothDecodersAreScoredAlikeTests` | 5 of 5 | 5 of 5 |
| the port's own tests | 8 of 8 | 8 of 8 |
| `EveryCharacterCarriesAConfidenceTests` | - | 2 of 2 |
| `TheSpeedFollowsTheSendersMarkPairsTests` | red at 28 of 31 | red at 28 of 31, not required green |
| decode time, ours over 690 s real | 51.93 s | 51.71 s |
| V-11, 35 rows | saved | every row identical |
| our texts (126 lines), classes (35), the port's texts (74) | saved | byte-identical |

Exit prints (`.run-unit/unit464-exit-print.txt`):
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints nothing.
- `git diff 7e209cb4` over the eleven transmit files prints nothing, and all eleven are present.
- `git status` shows `.run-unit/fldigi/` still untracked.
- src this unit is four files, 234 lines added: `CwCharacter.cs` (+16), `CwCharacterProbability.cs`
  (new), `CwProbabilisticStream.cs` (+1), `FldigiConfidence.cs` (new).
- The parity runs' rewrites of `parity.md` are kept under `.run-unit/unit464-parity-*.md`, and the
  committed file was restored each time.

## 4. What's blocking us

1. **R86 and 9.4, as a plain reading.** 9.4 asks for "one technique 9.3 named", and the five
   mechanisms 9.3 named are all refused under R78. That leaves 9.4 no authorable route. R86 holds
   steps 2 to 8 behind 9.4. Step 9's 9.5 to 9.7 carry the loop meanwhile; 9.5 is now ticked, and
   9.6 and 9.7 remain. Only the owner can change how R86 or 9.4 reads. This is logged for the
   owner and is not a stop: it touches neither transmit nor what the product promises the
   operator.
2. **R85 readings, one line each, recorded and acted on:**
   - "right" for a dim character is the scorer's alignment, key character equal to it, exactly as
     for sure;
   - a bin is floor(10p), the last closed at 1;
   - a bin counts at 10 characters and a condition at 30, as DECIDED (4);
   - fldigi's receiver can honestly be read for the level its own squelch metric reads
     (sig_avg over noise_floor, cw.cxx:635-636) and for how far each element fell from the
     dot/dash split it was judged by (cw.cxx:847).
3. **For 9.6, a finding and not a question.**
   - **The port.** Under HM-REQ-124, the port is calibrated on no condition, so it votes nowhere.
     An arbiter built on today's maps would always take ours on a disagreement.
   - **Ours.** Ours' p spans only 0.77 to 0.99, nearly all in bins 8 and 9. It is calibrated
     where it is because it is close to a base rate, not because it picks out its wrong letters
     (task 1's print: MarginLlr's quintiles run 0.84 to 0.98 right).
   - **The port's timing margin** falls in right share at both ends. The form fixed at task 1 was
     linear and was not bent after the print (V-14).

   A better port confidence would need a new form, fixed afresh before measurement. That is a
   later unit's choice, not this one's.
4. **The adapter's input, against the instruction's words.** `FldigiConfidence` needs the port's
   decision rows (`TraceDecisions` on) as well as `Emissions` and `KeyEvents` (section 1). 9.6 will
   have to run the port with the trace on. The trace records and changes nothing; the port's texts
   were byte-identical with it on.
5. **Port characters outside the scored stretches.** The harness locates each decoder's stretches
   in that decoder's own text, not in ours'. 220 of the port's 651 printed non-space characters fall
   outside every stretch located in its text (and 406 of ours' 1032 outside ours'). They carry a p
   and are unscored. This is 463's section 4 item 4 again, parked, and noted only because task 1's
   rows come from the same harness.
