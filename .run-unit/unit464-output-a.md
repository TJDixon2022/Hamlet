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

