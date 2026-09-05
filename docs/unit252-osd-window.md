# A window on the ordered-statistics search — the trace, the prices, and what it bought

**Written by unit 252, the first unit of this phase to attempt step 3.** Everything
below was measured or read on this tree on 2026-09-05, at `Ft8Sharp` 0.10.7 and
`Ft8Sharp.Deep` 0.5.0 going in.

Nothing here is a plan. Where it names a cost, the cost is either arithmetic that is
shown, or a measurement whose test is named. **Predictions are labelled as
predictions** and §3 is entirely predictions; §6 says whether they held.

---

## 1. Are the pivots in reliability order?

**Yes. `_pivots[90]` is the least reliable position *of the basis*, and it is also
merely the last one found — on this construction those are the same position, and
that is arithmetic rather than a hope.**

The reading, in three steps:

1. `SortByReliability`, `src/Ft8Sharp.Deep/Ft8DeepOrderedStatistics.cs:349-365`,
   fills `_order` with the 174 positions sorted by `_magnitude` **descending**. The
   loop's comparison is `_magnitude[_order[j]] < key`, strictly less, so equal
   magnitudes never shift and ties keep ascending position. The ordering is total and
   deterministic.
2. `Eliminate`, `:386-447`, sweeps `for (var c = 0; c < CodewordBits && rank <
   BasisBits; c++)` and takes `var column = _order[c]` — **it visits columns in
   exactly that descending-reliability order.**
3. It writes `_pivots[rank] = column; rank++;` at `:434-435`, and `rank` only ever
   increments, inside a loop whose index `c` only ever increments. So `_pivots` is
   filled in visitation order, and visitation order *is* reliability order.

Therefore `|ratio|` is **non-increasing along `_pivots`**: for any `p < q`,
`_magnitude[_pivots[p]] >= _magnitude[_pivots[q]]`. `_pivots[90]` is the least
reliable member of the 91 that were chosen.

**What it is not.** `_pivots[90]` is *not* the least reliable position of the
codeword. A dependent column is stepped over at `:407-410` (`if (pivot < 0)
continue;`), so the basis can reach further down the reliability list than position
90 — the remarks at `:95-101` say so, and unit 246 §1's caveat is the same point.
Every position that was stepped over is **more** reliable than `_pivots[90]` and is
**not in the basis at all**. A window over the basis is therefore a window over the
91 positions the search can actually flip, which is the only set the enumeration
ranges over, and that is the right set to define it on.

**So the window is `_pivots[91-W .. 90]`** — the least reliable `W` of the basis —
and in the code it is one number: the value `Search` starts its enumeration at.
`Search(1, BasisBits - W, order)` instead of `Search(1, 0, order)`. No other
definition is needed and no reordering of anything is needed.

**Why this is the end the errors are at.** `Ft8DeepOrderedStatistics`'s own remarks
at `:24-30` record unit 246's measurement: the median trial has about six of its
errors inside the 91 most reliable positions and the rest below. Reliability is a
monotone proxy for the probability a hard decision is wrong, which is what the
`|ratio|` ordering *means*; the flips worth trying are therefore concentrated at the
unreliable end of the basis. **That is a prior, not a measurement, and §6 is where
it is tested** — if a windowed order 3 decodes as much as a full-basis order 3, the
prior held.

**The window is Fossorier and Lin's own segmentation of the most reliable basis**
(M. P. C. Fossorier and S. Lin, *Soft-decision decoding of linear block codes based
on ordered statistics*, IEEE Transactions on Information Theory 41(5), September
1995, pages 1379-1396), and it comes from that published description and nowhere
else.

---

## 2. The cost arithmetic, tabulated before anything is built

An order-λ search over a window of `W` basis positions costs

```
1 + sum over i = 1..λ of C(W, i)
```

re-encodings per candidate — one for the order-0 re-encoding at
`Ft8DeepOrderedStatistics.cs:208` (`_reencodings = 1;`) and one for every pattern
`Search` toggles at `:297` (`_reencodings++;`). The window changes `W` and nothing
else in that expression.

**`W = 91` is the full basis and is what ships today.**

| order | **W = 91 (full)** | W = 60 | W = 45 | W = 40 | W = 30 | W = 20 |
|---|---|---|---|---|---|---|
| 2 | **4 187** | 1 831 | 1 036 | 821 | 466 | 211 |
| 3 | **125 672** | 36 051 | 15 226 | 10 701 | 4 526 | 1 351 |
| 4 | **2 798 342** | 523 686 | 164 221 | 102 091 | 31 931 | 6 196 |

For reference the full-basis column at every order is 1, 92, **4 187**, **125 672**,
2 798 342 for orders 0 to 4. The first four are pinned by
`Ft8DeepOrderedStatisticsTests.TheCostOfAnOrderIsTheNumberOfSubsetsOfTheBasis`.

**Cheaper than today's shipping order 2 at 4 187:** every order-2 cell with a window
(1 831, 1 036, 821, 466, 211), **order 3 at W = 30 (4 526 is not — it is 8 per cent
dearer) and order 3 at W = 20 (1 351)**. Stated exactly: order 3 at W ≤ 29 is
cheaper than order 2 at the full basis; order 3 at W = 30 costs 4 526 against 4 187.

**Cheaper than full-basis order 3 at 125 672:** every windowed cell in the table
except order 4 at W ≥ 45 (164 221 and 523 686). **Order 4 at W = 40 (102 091) is
cheaper than full-basis order 3**, and order 4 at W = 30 (31 931) and W = 20 (6 196)
are far cheaper.

**The lever, stated plainly.** Order 3 at W = 40 costs 10 701 against full-basis
order 3's 125 672 — **11.7 times cheaper** — and against today's shipping 4 187 it is
**2.6 times dearer**. That is the trade this unit exists to price.

---

## 3. The predicted price of each cell — A PREDICTION, NOT A MEASUREMENT

**The model.** Two anchors from unit 246 §3, one whole 51-trial block at -21 dB, 664
candidates offered over 51 trials:

- the port at **64.1 ms a trial** (unit 246 §4's -21 dB row) — the floor that is not
  OSD at all;
- order 2, full basis, **74.3 ms a trial** at 4 187 re-encodings a candidate;
- order 3, full basis, **311.4 ms a trial** at 125 672 a candidate.

Take the marginal cost of the stage to be linear in re-encodings a candidate, since
the candidate count offered does not depend on the order (unit 246 §3 reports 664
offered on every OSD row):

```
ms a trial  =  64.1  +  c * R          R = re-encodings a candidate
```

The two anchors give **c = 2.44e-3** (from order 2: 10.2 / 4 187) and **c = 1.968e-3**
(from order 3: 247.3 / 125 672). They agree within 24 per cent, which for a
prediction over a 300-fold range of `R` is the accuracy claimed and no more. **The
table below uses c = 1.968e-3**, the anchor with the larger lever; the order-2 cell
it reproduces at 72.3 against a measured 74.3, so the model reads about 3 per cent
low there.

| cell | R a candidate | predicted ms/trial | predicted 306-trial column, s |
|---|---|---|---|
| port, no OSD | 0 | 64.1 (anchor) | 19.6 |
| order 2, W = 91 (**ships today**) | 4 187 | 72.3 (measured 74.3) | 22.1 |
| order 2, W = 40 | 821 | 65.7 | 20.1 |
| order 3, W = 20 | 1 351 | 66.8 | 20.4 |
| order 3, W = 30 | 4 526 | 73.0 | 22.3 |
| order 3, W = 40 | 10 701 | 85.2 | 26.1 |
| order 3, W = 60 | 36 051 | 135.0 | 41.3 |
| order 3, W = 91 (**full**) | 125 672 | 311.4 (anchor) | 95.3 |
| order 4, W = 20 | 6 196 | 76.3 | 23.3 |
| order 4, W = 30 | 31 931 | 126.9 | 38.8 |
| order 4, W = 40 | 102 091 | 265.0 | 81.1 |

**What task 4 can afford inside one 480 s foreground call per rung.** A rung runs
every column over the same trial, so the call costs the *sum* of its columns plus the
harness's own per-trial synthesis, which is outside the decoders' stopwatches and is
not in the anchors above. Task 4's four isolation columns — port, OSD off, order 2
full, and one windowed cell — predict **19.6 + 19.6 + 22.1 + 26.1 = 87.4 s** of
decoding for a 306-trial rung at order 3 W = 40, and the synthesis is bounded by
unit 246's own whole-run figure. **Every cell in the table above fits**, including
the full-basis order 3 column at 95.3 s: four columns plus it is about 183 s of
decoding against a 480 s ceiling.

**And that contradicts the premise this instruction was written on**, which is
reported in §7 rather than repaired: unit 246 §5 item 4 says *306 trials at order 3
is about 25 minutes of wall clock*, but unit 246 §3's own measured 311.4 ms a trial
gives **1.6 minutes for one 306-trial rung and about 4.8 minutes for all three**.
The two are out by roughly fifteen-fold. **The window is still worth building and
pricing** — step 3's third exit asks for order *and* search weight with the cost each
buys, and 11.7× cheaper at the same order is the answer to that question whether or
not the expensive cell was affordable — but **the named drop candidate looks
affordable and task 4 should try to keep it.** §6 says whether it did.

**Which cells task 4 cannot afford:** none of them, on this prediction, at four or
five columns a rung. Task 3 measures whether the prediction held before task 4 spends
anything at 306.

---

## 4. THE SAFETY QUESTION: does a window change how many codewords reach the CRC-14?

**No. Not by one. The number is exactly one submission per candidate the stage is
offered, before and after, and the window cannot move it.**

The submissions a candidate makes today, with line references:

- `Ft8DeepSlotDecoder.Decode`, `src/Ft8Sharp.Deep/Ft8DeepSlotDecoder.cs:394`, calls
  `Ft8CodewordDecoder.Decode(ratios, cache, MaxIterations)` **once per candidate** —
  that is the port's own submission and it happens whether or not OSD exists.
- OSD runs only where that returned `ParityNeverSatisfied` (`:400-403`). Inside that
  branch, `_statistics.Decode(...)` at `:410` returns **exactly one codeword**:
  `Ft8DeepOrderedStatistics.Decode` copies `_best` out once, at `:218-222`, and
  returns one `Ft8DeepOsdResult`. `produced++` at `:412` counts one per offered
  candidate, which is what `Ft8DeepOsdCounts.Produced` documents.
- That one codeword is saturated at `:418` and submitted at `:419` —
  `Ft8CodewordDecoder.Decode(_osdRatios, cache, MaxIterations)`, **once**. There is no
  loop around it, no retry and no second order.

**So a candidate makes at most two submissions today: the port's, and OSD's one.**

**What this unit changes.** The window changes the range of `r` in
`Ft8DeepOrderedStatistics.Search` — `for (var r = start; r < BasisBits; r++)` at
`:294` — and therefore which patterns are ranked and which codeword `_best` ends as.
It changes **no line in `Ft8DeepSlotDecoder`**, adds no branch around `:419`, and
cannot change `produced`, which is incremented unconditionally once per offered
candidate. **A window can only make the search consider fewer patterns; it cannot
make it submit more answers, because the number of answers was never the number of
patterns.**

**The false-accept arithmetic, written out.** Unit 246 §5 item 2: every codeword put
to the CRC-14 is an independent false accept at about **one in 16 384**
(6.10e-5).

| | submissions a slot, ceiling | expected wrong messages a slot |
|---|---|---|
| the port alone | 140 (`Ft8SyncSearch.DefaultCandidateLimit`, `src/Ft8Sharp/Dsp/Ft8SyncSearch.cs:88`) | 0.0085 |
| **before** — port + OSD order 2 full basis | 140 + 140 = 280 | **0.0171** |
| **after** — port + OSD at any (order, window) | 140 + 140 = 280 | **0.0171** |
| the failure mode this unit may not commit — one submission per re-encoding at order 2 | 140 × 4 187 = 586 180 | **35.8** |

**The before and after are the same number and the third row is why the rule
exists.** The last row is unit 246's own arithmetic and is what §0.0 governs; nothing
in this unit goes near it.

Across a whole scoreboard the expectation is small but not zero: unit 246 recorded
11 451 candidates offered to the stage over three rungs at 306 trials, so **about
0.70 expected false accepts for the OSD column of a whole scoreboard** on top of the
port's own. Zero observed is the ordinary outcome and one observed would not be
astonishing — **and the ruling still stands: an approach that returns one wrong
decode is rejected and another taken.** That is a policy about what this project will
ship, not a claim that the arithmetic forbids it, and the two are not the same
sentence.

**The answer to the question as asked: it does not move.** The stage produces one
codeword per candidate by construction, and the window changes which one, not how
many.

---

## 5. Does the recorded `before` reproduce?

**Yes, and it is the same audio and the same noise draw, bit for bit.**

`Ft8Unit246ScoreboardTests.TheWholeLadderThroughThreeColumns`,
`tests/Ft8Sharp.Tests/Dsp/Ft8Unit246ScoreboardTests.cs:116`, makes exactly one call
per rung:

```csharp
var results = Ft8LadderHarness.Run(rung, Trials, decoders: decoders);
```

with `Trials = 306` (`:37`) and `rung` walking `{ -19.0, -20.0, -21.0 }` (`:114`).
Everything else is the harness's default:

| parameter | value used | where |
|---|---|---|
| `seed` | `221001` | `Ft8LadderHarness.DefaultSeed`, `Ft8LadderHarness.cs:61` |
| `frequencyHz` | `1000.0` | `DefaultFrequencyHz`, `:64` |
| `offsetSamples` | `SamplesPerSymbol(12000) * 3` = 5 760 | `DefaultOffsetSamples`, `:69` |
| `log` | null | not passed |
| decoder construction | `new Ft8SlotDecoder()`, `new Ft8DeepSlotDecoder()`, `new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default)` | `:86-88` |

**Why adding columns cannot perturb the before column.** `Run`'s loop, `:263-301`,
synthesises the trial's audio *before* it touches the decoder list — `OneSignal`,
`TransmissionPower`, `NoiseAmplitudeFor`, `AddNoise` at `:278-282` — and only then
walks `for (var d = 0; d < used.Count; d++)` handing every decoder the **same
`mixed` array**. The noise generator is one `GaussianNoise(blockSeed)` per block at
`:269`, drawn once per trial in the population's fixed order, and `blockSeed = seed +
block + rungOffset` at `:268` depends on the rung and the block index **and on
nothing about the decoder list**. So a run with seven columns draws the same audio as
a run with three, and each decoder is a fresh instance carrying no state between
trials except its own counters.

**The one thing that is not bit-identical is the clock.** `ms/trial` and worst
observed slot are wall-clock measurements on this machine tonight and will not
reproduce unit 246's to the decimal. Decoded, missed and wrong will.

**The check that makes this assertable rather than assumed:** tonight's `before`
column is `new Ft8DeepSlotDecoder(osd: new Ft8DeepOsdSettings(2))` at the full basis
— and task 2 asserts, in a test, that at the full basis the re-encoding count and the
recovered codeword are bit-for-bit what they are today. If the default path had moved
by one re-encoding, every figure in `docs/unit246-osd.md`, every row of
`HM-OPEN-067`, and the `before` column here would be measuring a different decoder.

---

## 6. The grid at 51 trials, and the cell taken to 306

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit252GridTests.TheOrderAndWindowGridAtMinus21DbOverOneBlock`.
One whole 51-trial block at -21 dB, delivered **-21.004 dB on every row**, every row
seeing the same seed and the same noise draw. **664 candidates offered to the stage
on every OSD row**, which is unit 246 §3's own figure to the candidate.

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
Ft8Sharp         -21.0    -21.004      51        3      48      0     5.9     2.0    15.9      3.3     64.5
Deep OSD off     -21.0    -21.004      51        3      48      0     5.9     2.0    15.9      3.3     64.6
o2 full          -21.0    -21.004      51        4      47      0     7.8     3.1    18.5      3.8     73.7
o2 W40           -21.0    -21.004      51        3      48      0     5.9     2.0    15.9      3.4     67.2
o3 W20           -21.0    -21.004      51        3      48      0     5.9     2.0    15.9      3.5     68.5
o3 W30           -21.0    -21.004      51        4      47      0     7.8     3.1    18.5      3.8     74.9
o3 W40           -21.0    -21.004      51        4      47      0     7.8     3.1    18.5      4.4     86.1
o3 W60           -21.0    -21.004      51        5      46      0     9.8     4.3    21.0      6.8    134.3
o3 full          -21.0    -21.004      51        5      46      0     9.8     4.3    21.0     15.6    305.9
o4 W20           -21.0    -21.004      51        4      47      0     7.8     3.1    18.5      4.0     77.7
o4 W30           -21.0    -21.004      51        5      46      0     9.8     4.3    21.0      6.6    129.8
```

| row | order | window | per candidate | decoded | missed | **wrong** | ms/trial | worst slot ms | offered | accepted | re-encodings |
|---|---|---|---|---|---|---|---|---|---|---|---|
| `Ft8Sharp` | - | - | - | 3 | 48 | **0** | 64.5 | 72.0 | 0 | 0 | 0 |
| Deep OSD off | - | - | - | 3 | 48 | **0** | 64.6 | 71.4 | 0 | 0 | 0 |
| o2 full **(ships)** | 2 | full | 4 187 | 4 | 47 | **0** | 73.7 | 87.9 | 664 | 1 | 2 780 168 |
| o2 W40 | 2 | 40 | 821 | 3 | 48 | **0** | 67.2 | 77.3 | 664 | 0 | 545 144 |
| o3 W20 | 3 | 20 | 1 351 | 3 | 48 | **0** | 68.5 | 79.8 | 664 | 0 | 897 064 |
| o3 W30 | 3 | 30 | 4 526 | 4 | 47 | **0** | 74.9 | 91.1 | 664 | 1 | 3 005 264 |
| o3 W40 | 3 | 40 | 10 701 | 4 | 47 | **0** | 86.1 | 110.8 | 664 | 1 | 7 105 464 |
| **o3 W60** | 3 | 60 | 36 051 | **5** | 46 | **0** | **134.3** | 198.8 | 664 | 2 | 23 937 864 |
| o3 full | 3 | full | 125 672 | **5** | 46 | **0** | 305.9 | 512.9 | 664 | 2 | 83 446 208 |
| o4 W20 | 4 | 20 | 6 196 | 4 | 47 | **0** | 77.7 | 95.2 | 664 | 1 | 4 114 144 |
| o4 W30 | 4 | 30 | 31 931 | **5** | 46 | **0** | 129.8 | 194.0 | 664 | 2 | 21 202 184 |

**Zero wrong on all eleven rows. The OSD-off row equals the port decode for decode,
miss for miss and wrong for wrong**, asserted per row rather than printed.

**The before reproduced.** Unit 246 §3 measured this same block at o2 full as **4 of
51 at 74.3 ms a trial with 664 offered and 1 accepted**, and at o3 full as **5 of 51
at 311.4 ms with 2 accepted**. Tonight: 4 of 51 at 73.7 with 664 offered and 1
accepted, and 5 of 51 at 305.9 with 2 accepted. **Decode for decode identical; the
clock within one per cent.**

### Did the prediction of §3 hold?

| cell | §3 predicted ms/trial | measured ms/trial | error |
|---|---|---|---|
| o2 W40 | 65.7 | 67.2 | +2 % |
| o3 W20 | 66.8 | 68.5 | +3 % |
| o3 W30 | 73.0 | 74.9 | +3 % |
| o3 W40 | 85.2 | 86.1 | +1 % |
| o3 W60 | 135.0 | 134.3 | -1 % |
| o4 W20 | 76.3 | 77.7 | +2 % |
| o4 W30 | 126.9 | 129.8 | +2 % |

**The linear model held to within 3 per cent on every cell**, over a 40-fold range of
re-encoding counts. The whole grid ran in **59.1 s of wall clock against 58.5 s of
summed decoder time**, so the harness's own synthesis costs about **12 ms a trial**
and a 306-trial rung's synthesis is about 3.6 s.

### The decision, and what it was taken on

**Order 3 over a window of 60 goes to 306 trials.**

- It is the **cheapest cell that reached the best decode count on this block**: 5 of
  51, the same 5 as full-basis order 3, at **134.3 ms a trial against 305.9** — 44 per
  cent of the price — and its worst observed slot is 198.8 ms against 512.9.
- It found **the same two codewords the full basis found** and the port's own gates
  accepted both. That is the direct evidence that the window did not throw away
  anything the whole basis reached, and it is the claim §1's prior was making.
- **51 trials cannot separate 5 from 4 from 3**, and this document does not pretend
  otherwise — the three Wilson intervals here are 4.3-21.0, 3.1-18.5 and 2.0-15.9 and
  they overlap almost completely. **So the choice among the cells that reached 5 was
  made on price**, which is exactly what unit 246 did when it chose order 2. `o4 W30`
  also reached 5, at 129.8 ms — within 3 per cent of `o3 W60` and indistinguishable
  on this block — and `o3 W60` was preferred because it is one axis away from what
  ships rather than two, and because order 3 is the question unit 246 left open.
- **`o2 W40` and `o3 W20` bought nothing over the port and are reported as buying
  nothing.** A window can be too narrow, and 20 of 91 is.

**AND THE NAMED DROP CANDIDATE IS NOT DROPPED.** Full-basis order 3 measured **305.9
ms a trial**, so a 306-trial column costs **94 s**, not the *about 25 minutes* unit
246 §5 item 4 predicted. It goes to 306 trials as a fifth column and unit 246's open
question is settled rather than carried forward.

---

## 7. The scoreboard at 306 trials

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit252ScoreboardTests`, **one test method per rung**,
each run alone by its exact full method name in the foreground with a 480 s timeout.
306 trials a rung, five columns, the same audio handed to all five. **Fine sync off on
every column of the isolation.**

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
Ft8Sharp         -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     20.0     65.5
Deep OSD off     -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     19.5     63.7
o2 full          -19.0    -19.001     306      276      30      0    90.2    86.3    93.0     22.0     71.7
o3 W60           -19.0    -19.001     306      277      29      0    90.5    86.7    93.3     39.7    129.7
o3 full          -19.0    -19.001     306      281      25      0    91.8    88.2    94.4     88.5    289.1

Ft8Sharp         -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     19.6     63.9
Deep OSD off     -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     19.8     64.7
o2 full          -20.0    -20.000     306      125     181      0    40.8    35.5    46.4     22.4     73.2
o3 W60           -20.0    -20.000     306      144     162      0    47.1    41.5    52.7     40.9    133.6
o3 full          -20.0    -20.000     306      155     151      0    50.7    45.1    56.2     92.1    301.1

Ft8Sharp         -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     19.5     63.9
Deep OSD off     -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     19.6     64.2
o2 full          -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     22.1     72.3
o3 W60           -21.0    -21.001     306       41     265      0    13.4    10.0    17.7     40.5    132.4
o3 full          -21.0    -21.001     306       43     263      0    14.1    10.6    18.4     91.2    298.1
```

**Zero wrong decodes on all fifteen rows. The OSD-off column equals the port column
on every rung**, decode for decode and miss for miss, asserted per rung.

**The recorded before reproduced exactly.** Every figure `docs/unit246-osd.md` §4
records came back to the decode: 248/248/276 at -19, 73/73/125 at -20, 13/13/33 at
-21. **The instrument did not move underneath the measurement**, which is what makes
the two new columns attributable.

Wall clock: 193.2 s, 198.3 s and 196.6 s a rung, against a 480 s ceiling.

### What the stage itself did, and the time budget

| rung | column | worst slot ms | its candidates | margin vs 15 s | offered | accepted | re-encodings |
|---|---|---|---|---|---|---|---|
| -19 | `Ft8Sharp` | 74.2 | 15 | 202× | 0 | 0 | 0 |
| -19 | Deep OSD off | 73.0 | 11 | 205× | 0 | 0 | 0 |
| -19 | o2 full | 86.0 | 23 | 174× | 3 729 | 28 | 15 613 323 |
| -19 | o3 W60 | 194.4 | 23 | 77× | 3 729 | 29 | 134 434 179 |
| -19 | o3 full | 495.1 | 23 | 30× | 3 729 | 33 | 468 630 888 |
| -20 | `Ft8Sharp` | 78.0 | 20 | 192× | 0 | 0 | 0 |
| -20 | Deep OSD off | 99.4 | 14 | 151× | 0 | 0 | 0 |
| -20 | o2 full | 97.1 | 20 | 155× | 3 863 | 52 | 16 174 381 |
| -20 | o3 W60 | 192.1 | 22 | 78× | 3 863 | 71 | 139 265 013 |
| -20 | o3 full | 485.9 | 22 | 31× | 3 863 | 82 | 485 470 936 |
| -21 | `Ft8Sharp` | 97.0 | 10 | 155× | 0 | 0 | 0 |
| -21 | Deep OSD off | 100.8 | 15 | 149× | 0 | 0 | 0 |
| -21 | o2 full | 106.1 | 11 | 141× | 3 859 | 20 | 16 157 633 |
| -21 | o3 W60 | 201.7 | 24 | 74× | 3 859 | 28 | 139 120 809 |
| -21 | o3 full | 516.7 | 24 | 29× | 3 859 | 30 | 484 968 248 |

**The same number of candidates is offered to every OSD column on a rung** — 3 729,
3 863 and 3 859 — because the stage runs exactly where belief propagation gave up and
the order and the window do not change that.

**AND HERE IS THE THING WORTH NOTICING.** On every rung, the increase in trials
decoded is *exactly* the increase in codewords the port's own gates accepted:

| rung | accepted: o2 full → o3 W60 → o3 full | decoded: o2 full → o3 W60 → o3 full |
|---|---|---|
| -19 | 28 → 29 → 33 (+1, +4) | 276 → 277 → 281 (**+1, +4**) |
| -20 | 52 → 71 → 82 (+19, +11) | 125 → 144 → 155 (**+19, +11**) |
| -21 | 20 → 28 → 30 (+8, +2) | 33 → 41 → 43 (**+8, +2**) |

Six deltas, six exact matches. **On this population the wider search kept everything
the narrower one found and added to it** — it is additive, not a trade. That is not
a proof of set inclusion, because equal deltas could in principle hide a loss and a
gain, but six coincidences is a strong indication and §8 says what it does and does
not license.

### Nice to pass, and NOT part of the isolation: the shipping configuration

`TheShippingConfigurationAtMinus21Db`, 306 trials, **fine sync on**, which is what
`Ft8Reception.cs:460` builds. Two stages are stacked here and no figure from it is
reported as step 3's.

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
ship today       -21.0    -21.001     306       35     271      0    11.4     8.3    15.5     61.3    200.3
ship o3 W60      -21.0    -21.001     306       43     263      0    14.1    10.6    18.4     79.6    260.3
```

| column | worst slot ms | margin vs 15 s |
|---|---|---|
| ship today (fine sync + o2 full) | 330.4 | **45×** |
| ship o3 W60 (fine sync + o3 W60) | 476.0 | **32×** |

**Zero wrong on both rows.** 144.1 s of wall clock.

---

## 8. The verdict

### 8.1 The 50 per cent crossing, before and after

**Quoted as an interpolation over three rungs and not as a measured crossing**, by the
same linear arithmetic `docs/unit246-osd.md` §4 used and that `HM-OPEN-067`'s *near
-19.5* was read off.

| column | -19 dB | -20 dB | -21 dB | 50 per cent crossing |
|---|---|---|---|---|
| `Ft8Sharp` | 81.0 | 23.9 | 4.2 | **-19.54 dB** (unit 246 read -19.54) |
| Deep OSD off | 81.0 | 23.9 | 4.2 | **-19.54 dB** |
| **o2 full — the `before`** | 90.2 | 40.8 | 10.8 | **-19.81 dB** (unit 246 read -19.81) |
| **o3 W60 — the `after`** | 90.5 | 47.1 | 13.4 | **-19.93 dB** |
| o3 full | 91.8 | 50.7 | 14.1 | **-20.02 dB** |

**Both control figures reproduce unit 246's to the hundredth of a decibel.**

`o3 full` is the one row whose crossing is not bracketed by the -19 and -20 rungs —
its -20 dB rate is 50.7 per cent, already past 50 — so its figure is interpolated
between **-20 and -21** instead. The -19/-20 line extrapolates to the same -20.02.

**What the window bought, in decibels: 0.12 dB over the order 2 that ships**
(-19.81 to -19.93). Full-basis order 3 bought 0.21 dB (-19.81 to -20.02) for 2.25
times the price of the window. Against the port, ordered statistics now stands at
**0.39 dB windowed and 0.48 dB at the full basis**, of the 1.5 dB `HM-OPEN-067`
records.

### 8.2 The -21 dB rate, before and after, with trial counts and Wilson intervals

| | decoded | trials | rate | 95 per cent Wilson | **wrong** |
|---|---|---|---|---|---|
| `Ft8Sharp` | 13 | 306 | 4.2 % | 2.5 - 7.1 | **0** |
| **before** — o2 full | **33** | **306** | **10.8 %** | **7.8 - 14.8** | **0** |
| **after** — o3 W60 | **41** | **306** | **13.4 %** | **10.0 - 17.7** | **0** |
| o3 full | 43 | 306 | 14.1 % | 10.6 - 18.4 | **0** |

### 8.3 The time budget

The configuration being recommended is **the one that ships, unchanged** — see 8.4 —
so its worst observed slot is the shipping row's: **330.4 ms against a 15 000 ms
budget, a 45× margin.** In the isolation, with fine sync off, `o2 full`'s worst
observed slot was 106.1 ms, a **141× margin**.

Had the window shipped, it would have been **476.0 ms, a 32× margin** in the shipping
configuration and 201.7 ms, a 74× margin, in isolation. **Neither is anywhere near
the budget**, and time is not what decides 8.4.

### 8.4 `Ft8DeepOsdSettings.Default` — THE DECISION, AND IT IS NOT TO MOVE IT

**The default stays at order 2 over the full basis.**

**Why, stated as the rule it was decided on.** At every one of the three rungs the 95
per cent Wilson intervals of the `before` and the `after` overlap:

| rung | before | after | overlap |
|---|---|---|---|
| -19 | 86.3 - 93.0 | 86.7 - 93.3 | almost complete |
| -20 | 35.5 - 46.4 | 41.5 - 52.7 | 41.5 - 46.4 |
| -21 | 7.8 - 14.8 | 10.0 - 17.7 | 10.0 - 14.8 |

**306 trials did not separate these two cells by the only interval this project
computes, and a measurement that does not separate two options is a result.** Moving
the default anyway would be moving it on the point estimate, which is tuning until a
number passes, and step 3's third exit forbids that by name.

**AND THE HONEST QUALIFICATION, because it is the reason a later unit should re-open
this and not let it lie.** `Ft8Step6Ladder.Wilson` is an *independent-sample*
interval, and this design is **paired**: `Ft8LadderHarness.Run` synthesises one audio
array per trial and hands the same array to every column, which the harness's own
remarks call *worth far more than two independent runs*. An independent-sample
interval is the wrong statistic for a paired design and it overstates the uncertainty
of a difference. The right one - McNemar on the trials where exactly one of the two
columns decoded - **needs the discordant pairs, and nothing in this tree records
them**: `Ft8LadderHarness.Result` carries totals only. §7's six exactly-matching
deltas are a strong hint that the discordant count is one-sided, which would make the
difference significant at -20 and -21, **but a hint is not a test and this document
does not report one.**

**So the finding is not "the window bought nothing".** It is: *the window bought a
measured 0.12 dB and 8 more decodes of 306 at -21 dB with zero wrong, and the
statistic this project computes cannot say that difference is not chance.* **Settling
it costs one instrumented run and no new algorithm** - record per-trial decoded flags
per column and apply McNemar - and that is logged in `OPEN_ISSUES.md` for step 6.

**What the operator gets tonight: nothing new.** `Ft8Reception` still builds
`Ft8DeepOsdSettings.Default`, which is still order 2 over the whole basis, and the
shipping configuration still reads **35 of 306 at -21 dB**. Nothing on his screen
changed and nothing costs him a millisecond more per slot. **What is new is that the
window exists, is measured, and any caller can name one** - `new
Ft8DeepOsdSettings(3, 60)` - and knows what it costs and what it bought.

### 8.5 The step closes on the figure it reached

Ordered statistics, taken as far as this unit takes it:

- The port reaches **4.2 per cent at -21 dB** and crosses 50 per cent at **-19.54 dB**.
- What ships reaches **10.8 per cent (33 of 306, 7.8 - 14.8)** and crosses at
  **-19.81 dB**.
- The furthest any cell measured tonight reaches is **14.1 per cent (43 of 306,
  10.6 - 18.4)** at full-basis order 3, crossing at **-20.02 dB**, for 298.1 ms a
  trial against the port's 63.9.
- **Order 3 is no longer unresolved.** Unit 246 left it at one decode of 51 and about
  25 minutes of wall clock; it is now measured at 306 trials on all three rungs, it
  buys 0.21 dB over order 2, and the window buys 0.12 dB of that for 44 per cent of
  the price.
- **Zero wrong decodes on every row of every measurement in this unit** - eleven grid
  rows, fifteen scoreboard rows and two shipping rows.

That is how far ordered statistics goes on this decoder, and the step closes on it.

---

## 9. Where this instruction and the tree disagreed

*Filled by task 6.*
