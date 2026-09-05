# The baseline, reproduced from a cold start — unit 243, 2026-09-04

**Work instruction 243, task 3.** `Ft8Step6CurveTests.TheDecodeRateIsMeasuredAtEveryRungAndTheVerdictIsReadAtMinusTwentyOne`,
run in a fresh process on this machine, `dotnet test`, **4 m 23 s**, 3519 slot
decodes, **passed**.

**It reproduces. Every rung, to the decode.**

| rung | delivered | trials | decoded | rate | 95 % Wilson | **wrong** | units 221–223 |
|---|---|---|---|---|---|---|---|
| **-19 dB** | -19.001 | 306 | **248** | **81.0 %** | 76.3 – 85.0 | **0** | 248 / 306, 81.0 % ✔ |
| **-20 dB** | -20.000 | 306 | **73** | **23.9 %** | 19.4 – 28.9 | **0** | 73 / 306, 23.9 % ✔ |
| **-21 dB** | -21.001 | 306 | **13** | **4.2 %** | **2.5 – 7.1** | **0** | 13 / 306, 4.2 %, 2.5–7.1 ✔ |

**Not one decode different on any of the three.** The figure `HM-OPEN-067` carries
and every target in `PHASE_PLAN.md` is quoted against is the figure this machine
produces today. **The offset clause in the plan's table of alternatives to
stopping — *record both figures, adopt the new one, move every target by the same
offset* — is not needed, and no target moves.**

## The whole curve, as printed

```
requested  delivered  trials  returned    rate   lo 95   hi 95  WRONG    cand     par     crc     txt
    -10.0    -10.000     153       153   100.0    97.6   100.0      0   18.13    1.70    1.70    1.70
    -13.0    -13.001     153       153   100.0    97.6   100.0      0   16.77    1.10    1.09    1.09
    -16.0    -16.001     306       306   100.0    98.8   100.0      0   14.72    1.00    1.00    1.00
    -17.0    -17.001     306       306   100.0    98.8   100.0      0   13.55    1.00    1.00    1.00
    -18.0    -17.999     306       304    99.3    97.6    99.8      0   13.11    1.00    0.99    0.99
    -19.0    -19.001     306       248    81.0    76.3    85.0      0   13.00    0.81    0.81    0.81
    -20.0    -20.000     306        73    23.9    19.4    28.9      0   12.87    0.24    0.24    0.24
    -21.0    -21.001     306        13     4.2     2.5     7.1      0   12.65    0.04    0.04    0.04
    -22.0    -21.999     306         0     0.0     0.0     1.2      0   12.39    0.00    0.00    0.00
    -23.0    -22.999     306         0     0.0     0.0     1.2      0   11.73    0.00    0.00    0.00
    -24.0    -24.001     306         0     0.0     0.0     1.2      0   11.71    0.00    0.00    0.00
    -26.0    -26.000     153         0     0.0     0.0     2.4      0   11.03    0.00    0.00    0.00
    -28.0    -28.000     153         0     0.0     0.0     2.4      0   11.54    0.01    0.00    0.00
    -30.0    -30.000     153         0     0.0     0.0     2.4      0   10.97    0.00    0.00    0.00
```

## What the figures around the rate say

**The axis is where it was.** Worst requested-versus-delivered error over the
whole run **0.0503 dB**, mean absolute error **0.0006 dB**. Unit 222's second
instrument — the periodogram in `Unit222AxisTests.cs`, which never calls
`SignalToNoise` and never calls `SearchFixture`'s power helpers — is still in the
tree and still agrees with the first to 0.0098 dB mean. **Two independent reasons
to believe the label on the horizontal axis.**

**Wrong decodes: 0 out of 3519 trials, at every rung of the whole ladder.** That
is the number this phase may not trade against rate (§0.0), and it is where a
later unit must keep it. **This is the figure the phase's step 2 will be judged
against: OSD returns codewords that satisfy parity and the CRC, and a soft-decision
list decoder is exactly the machinery that can start returning messages nobody
sent.** The baseline for that is zero and it is measured, not assumed.

**The collapse is 4.0 dB wide.** 99.3 per cent at -18, 81.0 at -19, 23.9 at -20,
4.2 at -21, nothing at -22. The 50 per cent crossing, interpolated between -19 and
-20, is near **-19.5 dB** — the 1.5 dB `HM-OPEN-067` records, unchanged.

**The candidate count barely moves.** 13.00 candidates a slot at -19 dB where 81
per cent decode, 12.65 at -21 dB where 4.2 per cent do. **The search finds the
transmission at both.** What changes is the column beside it: `par`, the fraction
of candidates reaching a valid codeword, falls 0.81 → 0.24 → 0.04. That is unit
222's finding on the face of the table — the demodulator is not the stage, belief
propagation is.

## What it costs, because every later unit pays it

| | measured |
|---|---|
| the whole 14-rung curve, 3519 trials | **4 m 23 s**, end to end |
| **one slot decode alone**, timed by the harness's own stopwatch | **63.9 ms** |
| one trial end to end, synthesis and noise and decode | **≈ 74.5 ms** |
| **one 306-trial rung** | **19.6 s** of decoding, **≈ 23 s** end to end |
| the three rungs the phase is measured on, 918 trials | **1 m 8 s** |

The two figures are different measurements and both are wanted. 63.9 ms is
`Ft8LadderHarness`'s stopwatch around the decoder call and nothing else; the
remaining ~10 ms a trial is synthesising the waveform and drawing the noise, which
a unit comparing two decoders on the same samples pays once rather than twice.

**Unit 221's cost model said 64.1 ms a slot decode. This machine measures 63.9.**
The model was right to a fifth of a per cent, and every plan built on it stands. **A
unit can afford to walk the three rungs several times in a night.** Walking the whole
curve is a four-and-a-half-minute commitment and should be done when the shape
matters, not by habit.

## The same three rungs through the new harness, which is the proof it is the same instrument

`Ft8LadderHarnessTests.TheThreeRungsThePhaseIsMeasuredOnAreWalkedAndTheThreeCountsAreReported`,
a separate process, `Ft8LadderHarness.Run(rung, 306, seed: 221001)`:

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
Ft8Sharp         -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     19.6     63.9
Ft8Sharp         -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     19.5     63.9
Ft8Sharp         -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     19.6     64.0
```

**Every count and every delivered mean is identical to the curve test's, to the
decode and to three decimal places on the axis.** That is what task 4's
instruction was protecting: *a rebuilt ladder is a different measurement, and
task 3's reproduction is what decides whether it is the same one.* It is the same
one, because `Ft8LadderHarness` calls `Ft8Step6Ladder`'s population, synthesiser,
noise and calibration rather than copying them, and walks whole blocks of the
population in the same order so that each trial draws the same noise from the same
generator.

**And it is now three counts rather than two.** `DECODED + MISSED = trials`;
`WRONG` counts messages returned that were not sent and is deliberately not part
of that partition, since a slot can return the right message and a wrong one at
once. Every wrong return would print on its own line with the message sent beside
the message returned. **There are none, at any of the three rungs.**

## Provenance, so this is a measurement and not a recollection

- **Instrument:** `Ft8Step6Ladder.cs`, unchanged. Every constant in it was
  committed before the curve was ever run, and nothing in this unit touched it.
- **Decoder:** `Ft8Sharp` 0.10.7, `src/Ft8Sharp/` untouched.
- **Root version:** 1.12.45 at the time of the run, 1.12.46 after this unit.
- **Population:** 51 scoreable messages of the 56-entry corpus; the 5 excluded are
  the hashed-callsign entries, which have no text on either side to compare.
- **Seeds:** 221001–221006, six draws through the collapse, three on the anchors.
- **Signal:** 1000.00 Hz, exactly on a bin centre; offset 5760 samples, on the
  block grid.
- **Convention:** signal power over noise power in a **2500 Hz** reference
  bandwidth, one-sided density, signal power measured over the transmission's own
  samples and noise power over the whole slot.

**Nothing was adjusted to make it reproduce.** Had it disagreed, the disagreement
would be this document.
