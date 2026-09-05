# The candidate re-synced below the grid it was found on — the placement sweep, the oracle ceiling, and what the search moved

**Written by unit 248, the first unit of this phase to attempt step 4.** Everything
below was measured on this tree on 2026-09-05, at `Ft8Sharp` 0.10.7 and
`Ft8Sharp.Deep` 0.4.0. It is written up so the next unit on step 4 does not have to
re-measure any of it.

Nothing here is a plan. Where it names a cost, the cost is measured and the
measurement is named.

---

## 0. The one-line answer

**The coarse grid costs almost the whole decode rate, the fine search recovers almost
all of the distance that costs, and on the placement this phase's crossing is quoted at
there is very little for it to recover because that placement sits exactly on the
grid.**

At -20 dB over 306 trials the port reads **73 of 306 (23.9 per cent)** at the ladder's
default placement and **0 of 306** at the centre of the same coarse cell — one eightieth
of a second later and one and a half hertz higher. The ordered statistics column reads
**125 of 306** and **1 of 306** at those two places. Averaged uniformly over the cell,
which is what real air delivers, the port reads **8.1 per cent against the 23.5 the
phase has been quoting**.

§1 is that sweep. §2 is the extractor and its ceiling. §3 is the search. §4 is the
scoreboard.

---

## 1. Task 1 — what the coarse grid costs, measured with nothing new

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit248PlacementTraceTests.cs`. **Not a line of new
production code runs in this section.** Every row comes out of
`Ft8LadderHarness.Run` unmodified, which already takes a `frequencyHz` and an
`offsetSamples`.

### 1.1 The grid, from the tree's own constants

At 12 kHz, from `Ft8WaterfallGeometry`:

```
SymbolPeriodSeconds     0.160 s
BlockSize               1920 samples
SubblockSize            960 samples (0.080 s)
TransformLength         3840
TransformBinSpacingHz   3.1250 Hz
ToneSpacingHz           6.2500 Hz
TimeOversampling        2
FrequencyOversampling   2
```

A candidate is therefore placed to within **±480 samples (±0.04 s, a quarter of a
symbol)** in time and **±1.5625 Hz (a quarter of a tone)** in frequency, and the port
reads all 58 data symbols at that quantised position.

### 1.2 And the ladder's default sits exactly on it

```
DefaultFrequencyHz      1000.0000 Hz = 320.000000 transform bins
nearest waterfall bin   bin 128 sub 0, centred at 1000.0000 Hz, error -0.000022 Hz
DefaultOffsetSamples    5760 samples = 6.000000 sub-blocks = 3.000000 blocks
```

**Both axes, exactly.** The residual 22 microhertz is the float `SymbolPeriodSeconds`
and not a placement. **Every figure this phase has recorded was measured at the one
placement where the coarse grid has nothing to lose.**

### 1.3 The placement sweep — 16 placements across one cell at -20 dB

51 trials each, one whole block of the population, port and ordered statistics side by
side on an identical noise draw. `dHz` is added to 1000.0000 Hz; `dSamp` is added to
5760 samples. **Zero wrong on all 32 rows.**

```
 dHz  dSamp   PORT dec  miss  WRONG    rate    DEEP-OSD dec  miss  WRONG    rate
------------------------------------------------------------------------------
0.00      0          12    39      0    23.5               21    30      0    41.2
0.00    240           7    44      0    13.7               13    38      0    25.5
0.00    480           0    51      0     0.0                4    47      0     7.8
0.00    720           8    43      0    15.7               19    32      0    37.3
0.78      0          10    41      0    19.6               19    32      0    37.3
0.78    240           6    45      0    11.8               10    41      0    19.6
0.78    480           2    49      0     3.9                3    48      0     5.9
0.78    720           3    48      0     5.9               12    39      0    23.5
1.56      0           2    49      0     3.9                7    44      0    13.7
1.56    240           1    50      0     2.0                2    49      0     3.9
1.56    480           0    51      0     0.0                0    51      0     0.0
1.56    720           1    50      0     2.0                3    48      0     5.9
2.34      0           5    46      0     9.8               14    37      0    27.5
2.34    240           5    46      0     9.8               10    41      0    19.6
2.34    480           1    50      0     2.0                2    49      0     3.9
2.34    720           3    48      0     5.9               12    39      0    23.5
```

**The best cell position is the on-grid corner and the worst is the cell centre**,
`+1.56 Hz, +480 samples`, where both columns read zero. The time axis costs more than
the frequency axis: `+480 samples` at any frequency is at or near zero, while
`+1.56 Hz` at `+0 samples` still returns 2 and 7.

### 1.4 The two corners at 306 trials — the size of the prize

**Zero wrong on all eight rows.**

```
BEST PLACEMENT (on grid): +0.00 Hz, +0 samples
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
Ft8Sharp         -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     63.7
Deep OSD on      -20.0    -20.000     306      125     181      0    40.8    35.5    46.4     72.6
Ft8Sharp         -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     63.6
Deep OSD on      -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.3

WORST PLACEMENT (cell centre): +1.56 Hz, +480 samples
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
Ft8Sharp         -20.0    -20.000     306        0     306      0     0.0     0.0     1.2     64.7
Deep OSD on      -20.0    -20.000     306        1     305      0     0.3     0.1     1.8     74.3
Ft8Sharp         -21.0    -21.001     306        0     306      0     0.0     0.0     1.2     63.7
Deep OSD on      -21.0    -21.001     306        0     306      0     0.0     0.0     1.2     72.9
```

**The -21 dB on-grid row is exactly unit 246's 13 of 306 and 33 of 306**, which is the
step 2 regression check passing before this unit wrote a line.

### 1.5 The placement-averaged rate, stated once

Uniform over the cell, 816 trials (16 placements × 51), at -20 dB:

```
                decoded of 816    rate    Wilson 95%
Ft8Sharp             66           8.1     6.4 – 10.2
Deep OSD on         151          18.5    16.0 – 21.3

on-grid alone, 51 trials
Ft8Sharp             12          23.5
Deep OSD on          21          41.2
```

**Real air is uniform over the cell and the ladder's default is one corner of it.** The
placement-averaged rate is about a third of the on-grid rate for the port and about
45 per cent of it for ordered statistics. **This is recorded as a finding about what
this phase's own baseline means. No target in `PHASE_PLAN.md` is changed by it and no
baseline is restated — that is the arbiter's to do with the number.**

### 1.6 What it meant before anything was built

**The spread across the cell is not a couple of points of rate; it is the whole rate.**
That said, it is a statement about *placements the phase does not measure at*. Step 4's
second exit is judged at the ladder's default, which is the on-grid corner, and §1.3's
top-left cell is where the crossing lives. **A large prize sitting where the crossing is
not measured is the shape of this unit's result and it was visible before the search was
written.**

---

## 2. Task 2 — the baseband extractor, and its ceiling

`src/Ft8Sharp.Deep/Ft8DeepBasebandSettings.cs`, `Ft8DeepBaseband.cs`,
`Ft8DeepBasebandExtractor.cs`. Tests in
`tests/Ft8Sharp.Deep.Tests/Ft8DeepBasebandTests.cs` and
`tests/Ft8Sharp.Tests/Dsp/Ft8Unit248ExtractorTraceTests.cs`.

### 2.1 The three numbers and the arithmetic behind each

Mixed at the **centre of the eight tones** — the base tone plus three and a half tone
spacings — so the occupied band is symmetric about zero at **±21.875 Hz** and the
low-pass gives every tone the same gain. A response that sloped across the span would
bias `Ft8SoftSymbols.ExtractSymbol`, which compares tone magnitudes against one another.

| | Value | Why |
|---|---|---|
| what must be passed | ±25 Hz flat | eight tones at ±21.875 Hz, plus the ±1.5625 Hz the search moves the mixing frequency by, plus the GFSK skirts |
| decimation | **24**, to **500 Hz** | 500 × 0.160 = **80 baseband samples in a symbol**, exactly, so a symbol window is a window and not a resampling. 48 would leave 40, which is a 4 ms time quantisation — the same order as the step the search wants |
| what must be rejected | from **475 Hz** | after decimating to 500 Hz, energy folds into ±25 Hz wherever \|f − n × 500\| < 25 |
| filter | **401-tap Blackman-windowed sinc, 150 Hz cutoff** | a Blackman transition is about 5.5 × fs / N, which at 12 kHz and 401 taps is 165 Hz — flat to about 68 Hz and in the stopband by about 232, inside the 475 the decimation needs. **A shorter filter cannot do both**: at 121 taps the transition is 545 Hz and no cutoff is both above 25 Hz and below 475 |

Measured response, from `Ft8DeepBasebandTests`:

```
    0.0 Hz     -0.00 dB
   25.0 Hz      0.00 dB
   50.0 Hz      0.00 dB
   68.0 Hz     -0.00 dB
  150.0 Hz     -6.02 dB
  232.0 Hz    -72.89 dB
  475.0 Hz   -100.38 dB
  500.0 Hz   -108.11 dB
```

**The window is rectangular and exactly one symbol long.** The tone spacing is the
reciprocal of the symbol period, so over exactly one symbol the eight tone exponentials
are orthogonal and the correlation is the matched filter for the alphabet. Any taper
widens each tone until it overlaps its neighbours. §2.4 has the tapered figure,
measured once as instructed rather than swept.

**Ruling 3 is kept literally.** The eight magnitudes are computed here, ordered by
symbol value through `Ft8Tables.Ft8GrayMap`, and handed to the port's public
`Ft8SoftSymbols.ExtractSymbol`. The Gray map, the bit partition and the ratio
arithmetic are the port's and are not re-implemented.

### 2.2 The one thing that had to be measured rather than read

`Ft8WaterfallGeometry.TimeSeconds` says in its own remarks that it returns *the block's
nominal position and not the centre of the window that produced it*, and that **the
exact alignment could not be settled by reading and is not asserted there.** The
extractor needs it exactly: a constant error here is a constant time error in every
position this library reports, and it would look precisely like a fine search that does
not work.

So it was swept on the distance instrument. One whole block at -14 dB, the candidate
closest to the transmitted codeword taken in each trial, the extractor run at that
candidate's nominal time plus a bias:

```
 bias s   bias symbols   median distance   best   trials at 17 or less
-------------------------------------------------------------------
 -0.320           -2.0                81     53                      0
 -0.240           -1.5                48     33                      0
 -0.160           -1.0                 0      0                     51
 -0.080           -0.5                47     19                      0
  0.000            0.0                80     53                      0
  0.080            0.5                82     56                      0
  0.160            1.0                84     51                      0
```

**Exactly minus one symbol period.** Median 0 of 174 with all 51 trials inside the
code's recovery threshold, and 47 or worse at every neighbouring half-symbol step. It is
`Ft8DeepSlotDecoder.CandidateTimeBiasSeconds`, a named constant with the measurement in
its remarks.

### 2.3 The control — and it is not flattering

**Nothing after this means anything if the extractor is worse at the same position, so
it comes first.** One whole block at -21 dB through `Ft8LadderHarness.Run` unmodified,
identical noise draw between the columns:

```
THE CONTROL at -21.0 dB, 51 trials, ON GRID
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
Ft8Sharp         -21.0    -21.004      51        3      48      0     5.9     2.0    15.9     65.2
baseband@grid    -21.0    -21.004      51        1      50      0     2.0     0.3    10.3    169.9

THE CONTROL at -21.0 dB, 51 trials, CELL CENTRE
Ft8Sharp         -21.0    -21.004      51        0      51      0     0.0     0.0     7.0     64.9
baseband@grid    -21.0    -21.004      51        0      51      0     0.0     0.0     7.0    172.6
```

**At the same coarse position the new extractor is worse than the port's**: 1 of 51
against 3 of 51 on grid, which is well inside the interval at 51 trials, and the
distance rows in §2.4 say the same thing more sharply — median **56 against 48** at
-21 dB and **46 against 36** at -20.

**What is responsible, and it is the window rather than a defect.** A rectangular
one-symbol correlation is the matched filter *when the time is right*. At a coarse grid
position the time is wrong by up to a quarter of a symbol **by construction**, and the
port's `HannSquaredSine`-tapered 3840-sample frame — two symbols wide — is more
forgiving of that. **The port's advantage is entirely an advantage at the wrong place**,
which is exactly what the fine search removes, and §2.4's oracle row is the evidence.

### 2.4 The oracle ceiling

At the worst placement task 1 found, one whole block at each rung. Hard-decision
distance to the codeword the ladder knows it transmitted. **Chance is 87 of 174; the
code's iterative recovery reaches zero at about 17.**

```
THE ORACLE CEILING at -21.0 dB, 51 trials, cell centre (+1.56 Hz, +480 samples)
  trials with no candidate at all: 0
  row                                    trials  median  best   worst   <=17
  port Extract, closest candidate            51      48    32      80      0
  baseband, same grid position               51      56    39      86      0
  baseband, ORACLE position                  51      32    20      48      0

THE ORACLE CEILING at -20.0 dB, 51 trials, cell centre (+1.56 Hz, +480 samples)
  trials with no candidate at all: 0
  row                                    trials  median  best   worst   <=17
  port Extract, closest candidate            51      36    22      79      0
  baseband, same grid position               51      46    27      81      0
  baseband, ORACLE position                  51      22    12      43      7
```

**Reading at the exact transmitted position takes 16 bits off the port's own closest
candidate at -21 dB and 14 at -20**, and at -20 dB it puts **7 of 51** trials inside the
recovery threshold where neither grid row has any. **That is the ceiling on everything a
fine search can win, and it is a real one.**

**It is also not enough at -21 dB.** Even at the oracle position the median there is 32,
nearly twice the threshold, and no trial reaches 17. **At the cell centre at -21 dB
perfect synchronisation is still not sufficient**, which bounds what this step could
have done at that placement whatever the search did.

**The oracle number is an oracle number and is added to no total.**
`SearchFixture.Truth` appears in this measurement and in no scored column.

### 2.5 The window shape, measured once

**Picked one, measured the other once, and here are both numbers.** At the oracle position,
-21 dB, 51 trials, hard-decision distance:

```
row                                    trials  median  best   worst   <=17
rectangular, one symbol (this library)     51      32    20      48      0
Hann-squared-sine taper                    51      44    32      55      0
```

**The rectangular window is 12 bits of 174 better at the right position**, which is what the
orthogonality argument in §2.1 predicts and is why it is the one this library uses. **It was not
swept.**

---

## 3. Task 3 — the fine search

`src/Ft8Sharp.Deep/Ft8DeepFineSyncSettings.cs`, `Ft8DeepFineSync.cs`. Tests in
`tests/Ft8Sharp.Deep.Tests/Ft8DeepFineSyncTests.cs` and the trace in
`tests/Ft8Sharp.Tests/Dsp/Ft8Unit248FineSyncTraceTests.cs`.

### 3.1 The extent, and it is read off the geometry

**±0.04 s and ±1.5625 Hz**, which is the whole of what the coarse grid leaves
undetermined — half a sub-block and half a transform bin.
`Ft8DeepFineSyncSettings.CoversTheCell(geometry)` asserts it from the geometry rather
than from a constant. Default step **0.005 s and 0.5208 Hz**, which is **119 positions
a candidate**.

### 3.2 What the step buys — and the answer is nothing measurable

At -20 dB at the cell centre, 51 trials, the closest candidate each time. **The row to
beat is the candidate's own unmoved position at median 46; the oracle reaches 22.**

```
 time step   freq step   positions   median   <=17   ms/candidate   edge t   edge f
----------------------------------------------------------------------------------
    0.0200      0.5208          35       24      8           0.56    96.1%    72.5%
    0.0100      0.5208          63       24      7           1.01    88.2%    72.5%
    0.0050      0.5208         119       24      6           1.88    70.6%    72.5%
    0.0025      0.5208         231       24      8           3.67    52.9%    74.5%
    0.0050      1.5625          51       25      5           0.81    70.6%    98.0%
    0.0050      0.7812          85       25      6           1.34    68.6%    84.3%
    0.0050      0.2604         221       24      6           4.20    68.6%    60.8%
```

**Every step from 2.5 ms to 20 ms gives median 24, and every frequency step from 0.26 Hz
to 1.5625 Hz gives 24 or 25.** The step is finer than the measurement can distinguish at
51 trials, which is exactly the question the instruction asked. **The default is not
tuned to a target; it is the coarsest step whose row is indistinguishable from the
finest.**

**And the search reaches the oracle.** Median 24 against the oracle's 22 and the unmoved
46 — the search recovers **22 of the 24 bits** the ceiling says are there.

The high edge rates in this table are **an artefact of the placement, not a defect**: at
the cell centre the truth sits at half a sub-block and half a bin from the nearest grid
point, which *is* the extent, so the winner is legitimately on the boundary. §3.3 is the
honest edge rate over all candidates.

### 3.3 The edge-hit rate over real candidates

Every candidate of every slot searched — not only the ones the port refused, which is
the loop's rule and is measured in §4.

```
rung   placement     slots  candidates   edge t   edge f   ms/cand   worst slot
-21.0  ON GRID          51         667     9.6%    13.5%      9.19    216.5 ms over 24 candidates
-21.0  CELL CENTRE      51         686    15.2%    20.0%      9.22    217.8 ms over 24 candidates
-20.0  CELL CENTRE      51         725    17.7%    24.8%      9.22    198.6 ms over 22 candidates
```

**On the grid the edge rate is under 14 per cent in both axes**, which says the extent
is the right size for the placement the crossing is quoted at. At the cell centre it
rises to 15–25 per cent, for the reason in §3.2. **The grid was not widened to make
these numbers smaller.**

The offset distributions at -21 dB on grid: mean absolute time shift **0.0176 s**, mean
absolute frequency shift **0.6466 Hz**, with **9.1 per cent** of candidates left unmoved
in time and **24.6 per cent** unmoved in frequency. **Most candidates at these rungs are
noise** — the search returns about 13 a slot — and a search over noise moves things
about, which is what a roughly flat histogram over the extent looks like.

### 3.4 Cost

**9.2 ms a candidate**, mixing and filtering and searching together, against the port at
about 64 ms a slot and ordered statistics at 72. **Worst slot observed 218 ms over 24
candidates with every one of them searched**, which is a 69-fold margin against FT8's
15 seconds and comfortably inside the tenfold margin of 1.5 s. §4.4 has the figure under
the loop's own rule, where only refused candidates are searched.

The mixing and the 401-tap filter are the expensive part and they depend only on where
the eight tones sit, so `Ft8DeepSlotDecoder` builds **one baseband per mixing frequency
per slot** rather than one per candidate.

---

## 4. Task 5 — the scoreboard

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit248ScoreboardTests.cs`. Three rungs, three columns, both
placements, **306 trials on every row**, through `Ft8LadderHarness.Run` unmodified.

**Column two has ordered statistics OFF and combining OFF**, so the difference between column one
and column two is one named change. **Fine sync and OSD are never stacked here** and no combined
figure is reported as step 4's.

### 4.1 On the grid — where exit 2 is judged

**Zero wrong decodes on all nine rows.**

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
Ft8Sharp         -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     64.2
Deep fine sync   -19.0    -19.001     306      268      38      0    87.6    83.4    90.8    193.4
Deep OSD on      -19.0    -19.001     306      276      30      0    90.2    86.3    93.0     73.2

Ft8Sharp         -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     64.1
Deep fine sync   -20.0    -20.000     306       95     211      0    31.0    26.1    36.4    194.7
Deep OSD on      -20.0    -20.000     306      125     181      0    40.8    35.5    46.4     73.2

Ft8Sharp         -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     64.0
Deep fine sync   -21.0    -21.001     306       18     288      0     5.9     3.8     9.1    192.3
Deep OSD on      -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.3
```

**The 50 per cent crossing, interpolated linearly between the -19 and -20 dB rungs** — the
arithmetic `HM-OPEN-067`'s "near -19.5" was read off and the one unit 246 used:

```
column                     -19 dB   -20 dB   crossing
Ft8Sharp                   81.0     23.9     -19.54 dB    (unit 246 read -19.54)
Deep fine sync             87.6     31.0     -19.66 dB    <- step 4 exit 2
Deep OSD on                90.2     40.8     -19.81 dB    (unit 246 read -19.81)
```

**Step 4's second exit is met and the figure is 0.12 dB.** Small, and it is the honest size of the
prize at a placement that sits exactly on the grid — §1.2 said so before anything was built.
**Both control figures reproduce unit 246's to the hundredth of a decibel**, which is what says the
instrument did not move underneath the measurement.

### 4.2 At the centre of one coarse cell — and this is the result

`+1.56 Hz, +480 samples`. **Zero wrong decodes on all nine rows.**

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
Ft8Sharp         -19.0    -19.001     306        6     300      0     2.0     0.9     4.2     66.3
Deep fine sync   -19.0    -19.001     306      277      29      0    90.5    86.7    93.3    208.0
Deep OSD on      -19.0    -19.001     306       33     273      0    10.8     7.8    14.8     75.8

Ft8Sharp         -20.0    -20.000     306        0     306      0     0.0     0.0     1.2     65.1
Deep fine sync   -20.0    -20.000     306       73     233      0    23.9    19.4    28.9    205.8
Deep OSD on      -20.0    -20.000     306        1     305      0     0.3     0.1     1.8     75.4

Ft8Sharp         -21.0    -21.001     306        0     306      0     0.0     0.0     1.2     64.3
Deep fine sync   -21.0    -21.001     306        3     303      0     1.0     0.3     2.8    194.3
Deep OSD on      -21.0    -21.001     306        0     306      0     0.0     0.0     1.2     72.8

column                     -19 dB   -20 dB   crossing
Ft8Sharp                    2.0      0.0     not bracketed — it never reaches 50 per cent
Deep fine sync             90.5     23.9     -19.61 dB
Deep OSD on                10.8      0.3     not bracketed
```

**Read the -19 dB row: 6 of 306 against 277 of 306.** The port collapses from 81.0 per cent to
2.0 when the sender moves an eightieth of a second and one and a half hertz — inside one cell of
the analysis grid, a distance no operator could control or would notice. **With fine sync on it
goes from 87.6 to 90.5.**

**At -20 dB the fine sync column reads 23.9 per cent at the cell centre, which is the port's
on-grid figure to one decimal place.** And its crossing off the grid, **-19.61 dB**, is *better*
than the port's on the grid, -19.54.

**The re-sync does not mainly move the crossing. It makes the crossing the same wherever the
sender lands**, and the placement-averaged rate in §1.5 is the number that changes most.

### 4.3 What the stage actually did

One whole block at each rung and placement, counts read off the decoder after every slot.
**Offered equals re-synced on every row** — one submission per refused candidate and never more.

```
 rung  placement    slots  cand  offered  resync  accepted  mean|dt| s  mean|df| Hz  edge t  edge f
-19.0  ON GRID         51   685      645     645         7      0.0181       0.7259    9.1%   20.0%
-20.0  ON GRID         51   665      653     653         4      0.0175       0.6835    8.6%   15.6%
-21.0  ON GRID         51   667      664     664         0      0.0177       0.6463    9.6%   13.6%
-19.0  CELL CENTRE     51   765      764     764       128      0.0223       0.8767   19.2%   31.2%
-20.0  CELL CENTRE     51   725      725     725        25      0.0214       0.8096   17.7%   24.8%
-21.0  CELL CENTRE     51   686      686     686         0      0.0207       0.7509   15.2%   20.0%
```

**The accepted column is the evidence that a rate that moved has re-sync activity behind it**, and
it is where the two placements part: **128 codewords rescued at the cell centre at -19 dB against
7 on the grid.**

### 4.4 The submission arithmetic, in full

Every codeword put to the CRC-14 is an independent chance of a false accept at about **one in
16 384**; `Ft8DeepCombineSettings.ExpectedFalseAccepts` is that arithmetic already written down and
is used rather than restated.

```
submissions across this whole measurement   4137
expected false accepts at one in 16384      0.253
worst a single slot could submit            140 (the port's candidate limit)
which is                                    0.0085 expected false accepts a slot
observed wrong decodes                      0, on every row of every table above
```

**0.253 expected against 0 observed** across the counts walk, and **0 wrong on all 18 scoreboard
rows** at 306 trials each. The bound holds because **exactly one codeword is submitted per refused
candidate**: the search visits 119 positions and submits the one it picks. Submitting all of them
would be 16 660 a slot and about one message nobody sent every slot.

### 4.5 Cost

```
the worst single slot observed   315.5 ms at -21.0 dB, cell centre
over                             24 candidates, 24 of them re-synced
margin against FT8's 15 seconds  48-fold
against the tenfold margin of 1.5 s: inside it
```

**Nothing was cut.** Per trial the fine sync column costs **193 ms on the grid and 208 at the cell
centre**, against the port's 64 and ordered statistics' 73.

---

## 5. `HM-OPEN-074` re-measured over 306 trials

The entry asked the unit taking step 4 to re-measure its "about four per cent" before quoting it.
Closest candidate to the transmitted codeword, -21 dB, six whole blocks:

```
placement      trials   none   >60 of 174   >=87 (chance)   median   worst
ON GRID           306      0      7 ( 2.3%)        0 ( 0.0%)       31      81
CELL CENTRE       306      0     57 (18.6%)        0 ( 0.0%)       47      84
```

**On the grid it is 2.3 per cent, not four**, and the median of 31 is exactly unit 246's and unit
222's figure. **Off the grid it is 18.6 per cent** — eight times as many trials where the search
never returns a place near the signal. **The population `HM-OPEN-074` identified is largely a
placement effect**, and it is a candidate-search effect rather than a candidate-refinement one:
refining a candidate that does not exist cannot help, and `Ft8SyncSearch` is the port's and is
untouchable this phase.

---

## 6. Task 7 — fine sync underneath combining, and it recovers nothing

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit248RepeatsTests.cs`. `HM-OPEN-075` says placement jitter took
unit 247's combined column from 200 of 306 to 55 of 306 and **names step 4 as the work that would
recover it**. Tested once, at -21 dB, 306 trials, two slots a trial, with the same 2.00 Hz and
480-sample jitter.

**The control is `Ft8LadderHarness.RunRepeats` unmodified** and it reproduces unit 247 exactly:
13 of 306 for a single slot, 33 with ordered statistics, **68 of 306 combined with 55 only-combined
and zero wrong**. 516 combinations submitted, 62 combined decodes, all 62 the message that was
sent.

```
column                    DECODED   rate    lo 95   hi 95   WRONG   only-combined   worst slot ms
combined x2                   68    22.2    17.9    27.2       0              55           104.6
combined x2 + fine sync       68    22.2    17.9    27.2       0              55           120.1
```

**Identical. Not close — identical, on every column.**

**And the reason is exact rather than mysterious.** `Ft8DeepSlotDecoder` captures a hearing from the
**coarse** ratios, before the port's gate, because that is where unit 247 put it and because
`RemembersHearings` was specified as changing no decision. Fine sync produces a *second* set of
ratios at the re-synced position and submits them, but it does not rewrite the hearing. **So the
combiner is still adding two coarse hearings and the re-sync never reaches it.**

**What would actually test `HM-OPEN-075`'s claim** is capturing the hearing at the re-synced
position rather than at the coarse one, so two hearings of one station arrive on the same footing
before they are added. **That is a change to step 6's code**, which unit 248 was told not to touch,
and it is the measurement the next unit on either step should take. **It is not evidence that the
claim is wrong; it is evidence that this unit did not test it**, and saying so is worth more than
the number.

---

## 7. What this leaves for the next unit on step 4

- **The crossing on the grid moved 0.12 dB and there is not much more there.** §2.4's oracle row
  says a perfect synchroniser at the cell centre reaches median 32 at -21 dB against the code's
  threshold of 17, so **even perfect alignment is not sufficient at -21 dB**. The remaining 1.2 dB
  is not all in the grid.
- **The placement result is the one worth carrying.** Every figure this phase quotes was taken at
  the one placement where the grid has nothing to lose. §1.5's placement-averaged rate — 8.1 per
  cent for the port against 23.5 on the grid at -20 dB — is what a real station sees, and the fine
  sync column is flat across the cell where the port is not.
- **The extractor is worse than the port's at the same coarse position** (§2.3), by about eight
  bits of median distance. A unit that wanted the last of the gain could try the port's tapered
  window *at the fine-synced position* — §2.5 says rectangular wins at the right position, but
  that was measured at the oracle rather than at what the search finds.
- **`HM-OPEN-074`'s population is 18.6 per cent off the grid** and belongs to the candidate search
  rather than to refinement. Nothing in this phase may touch `Ft8SyncSearch`.
- **Nothing here was tuned to a target.** The step sweep in §3.2 shows every step giving the same
  answer, so the default was chosen for cost rather than for a number, and no setting was tried
  until one passed.
