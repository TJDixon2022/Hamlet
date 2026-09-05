# Unit 256 — the crossings get their intervals, the unbracketed columns get brackets, and combining gets its own on-and-off panel

**Working record.** The trace, the price, the predictions, and — as the night proceeds —
what those predictions turned out to be. **The deliverable is the amendment to
`docs/unit255-closing-measurement.md`**, which this file links to and does not duplicate.

- Root version at start `1.12.57` (`Directory.Build.props:205`), moving to `1.12.58` at task 6.
- `Ft8Sharp` `0.10.7` (`src/Ft8Sharp/Directory.Build.props:396`) — **untouched**.
- `Ft8Sharp.Deep` `0.8.0` (`src/Ft8Sharp.Deep/Directory.Build.props:155`) — **untouched**.
- `HEAD` at authoring of this section: `13516b4`.

**Nothing under `src/` changes tonight.** Every figure below is a measurement or a
computation on measurements already committed.

---

## 1. The trace — what unit 255 actually left

**Read out of `docs/unit255-closing-measurement.md` and out of the seven committed
artefacts in `docs/unit255-runs/`, not out of the work instruction.**

### 1.1 The six crossings on the grid

`docs/unit255-closing-measurement.md:509–516`, the table under §4.1's sentence *On the
grid, every column is straddled by -19 and -20*. Rates confirmed against
`docs/unit255-runs/minus19-on-grid.txt` and `minus20-on-grid.txt`, both at 306 trials.

| column | -19 dB | -20 dB | published crossing | doc line |
|---|---|---|---|---|
| `Ft8Sharp` | 248 of 306, 81.0 % (76.3 – 85.0) | 73 of 306, 23.9 % (19.4 – 28.9) | **-19.54 dB** | `:511` |
| `Deep all off` | 248 of 306, 81.0 % | 73 of 306, 23.9 % | **-19.54 dB** | `:512` |
| `fine sync only` | 268 of 306, 87.6 % (83.4 – 90.8) | 95 of 306, 31.0 % (26.1 – 36.4) | **-19.66 dB** | `:513` |
| `OSD only` | 276 of 306, 90.2 % (86.3 – 93.0) | 125 of 306, 40.8 % (35.5 – 46.4) | **-19.81 dB** | `:514` |
| **`SHIPPING`** | **283 of 306, 92.5 % (89.0 – 94.9)** | **138 of 306, 45.1 % (39.6 – 50.7)** | **-19.90 dB** | `:515` |
| `subtraction only` | 248 of 306, 81.0 % | 73 of 306, 23.9 % | **-19.54 dB** | `:516` |

**Six crossings, every one a bare point value.** Not one carries an interval of its own;
the intervals printed beside them belong to the two rungs.

### 1.2 The two at the cell centre, and the rows that read `not bracketed`

`docs/unit255-closing-measurement.md:520–527`. Rates confirmed against
`docs/unit255-runs/minus19-cell-centre.txt` and `minus20-cell-centre.txt`.

| column | -19 dB | -20 dB | published crossing | doc line |
|---|---|---|---|---|
| `Ft8Sharp` | 6 of 306, 2.0 % (0.9 – 4.2) | 0 of 306, 0.0 % (0.0 – 1.2) | **not bracketed** — above -19 dB | `:522` |
| `Deep all off` | 6 of 306, 2.0 % | 0 of 306, 0.0 % | **not bracketed** — above -19 dB | `:523` |
| `fine sync only` | 277 of 306, 90.5 % (86.7 – 93.3) | 73 of 306, 23.9 % (19.4 – 28.9) | **-19.61 dB** | `:524` |
| `OSD only` | 33 of 306, 10.8 % (7.8 – 14.8) | 1 of 306, 0.3 % (0.1 – 1.8) | **not bracketed** — above -19 dB | `:525` |
| **`SHIPPING`** | **278 of 306, 90.8 % (87.1 – 93.6)** | **73 of 306, 23.9 % (19.4 – 28.9)** | **-19.61 dB** | `:526` |
| `subtraction only` | 6 of 306, 2.0 % | 0 of 306, 0.0 % | **not bracketed** — above -19 dB | `:527` |

### 1.3 THE COUNT, AND §4.1's PROSE DOES NOT MATCH ITS OWN TABLE

**Counted by hand off the table at `:520–527`: FOUR rows read `not bracketed`** —
`Ft8Sharp` (`:522`), `Deep all off` (`:523`), `OSD only` (`:525`) and `subtraction only`
(`:527`). Two rows carry a crossing: `fine sync only` and `SHIPPING`.

**The prose above the table says three**, twice:

- `:518` — *At the cell centre, **three** of the six are not straddled:*
- `:529–531` — *`not bracketed` is a result and it is the point. **Three** columns never
  reach 50 per cent at any rung this ladder measured off the grid…*

**The table is right and the prose is wrong.** Four columns are unbracketed at the cell
centre, and the arithmetic says so independently: `Ft8Sharp`, `Deep all off` and
`subtraction only` all read 6 / 0 / 0 across the three rungs and `OSD only` reads 33 / 1 / 0
(§3.2, and the artefacts). **None of the four reaches 50 per cent at any rung measured**, so
none is straddled. The most likely origin of *three* is that the three columns reading
6 / 0 / 0 are identical row for row and were counted once.

**This is a mismatch found in the tree, reported and not silently repaired.** Task 5 replaces
§4.1 whole, and the replacement says **four**, says that the earlier text said three, and
says why — which is task 5 item 1's rule that nothing is silently rewritten.

**Four is therefore tonight's workload for task 3**, and it is exactly the four columns the
work instruction names.

---

## 2. The interpolation, written out as arithmetic

### 2.1 The formula

The crossing is the ratio at which the decode rate curve passes 50 per cent, obtained by
**linear interpolation in decibels between the two rungs that straddle it**. With the upper
rung `d₁` (the better ratio's neighbour, here -19 dB) at rate `p₁`, and the lower rung `d₂`
(here -20 dB) at rate `p₂`, with `p₁ > 50 > p₂`:

```
crossing = d₁ + (d₂ - d₁) × (p₁ - 50) / (p₁ - p₂)
```

With `d₂ - d₁ = -1` dB, that is `crossing = d₁ - (p₁ - 50) / (p₁ - p₂)`.

**The assumption is stated because the band inherits it: the rate is taken to move linearly
in decibels between two rungs one decibel apart.** That is unit 255's own assumption, made
implicitly at `:502`; tonight it is made explicitly, because a band computed under it must
be described under it.

### 2.2 `SHIPPING` on grid, reproduced by hand

From `docs/unit255-runs/minus19-on-grid.txt` and `minus20-on-grid.txt`: **283 of 306** and
**138 of 306**.

```
p₁ = 100 × 283 / 306 = 92.48366 %      (published 92.5)
p₂ = 100 × 138 / 306 = 45.09804 %      (published 45.1)

crossing = -19 - (92.48366 - 50) / (92.48366 - 45.09804)
         = -19 - 42.48366 / 47.38562
         = -19 - 0.896552
         = -19.896552 dB
         = -19.90 dB to two decimals
```

**It reproduces.** The same arithmetic on the published rounded rates, 92.5 and 45.1, gives
`-19 - 42.5/47.4 = -19.89662 = -19.90 dB`, the same to two decimals. **The number this phase
is about to be quoted on checks out**, and nothing goes at the top of section 3 of the report
on this account.

### 2.3 The band, ruling 1's, written out and predicted before task 2 computes it

**The band is obtained by pushing each rung's Wilson bound through the same interpolation.**
The optimistic curve joins the two **upper** bounds and crosses 50 per cent at the lower
(better) ratio; the pessimistic curve joins the two **lower** bounds and crosses at the
higher ratio.

```
optimistic = d₁ - (hi₁ - 50) / (hi₁ - hi₂)
pessimistic = d₁ - (lo₁ - 50) / (lo₁ - lo₂)
```

**It is not a confidence interval on the crossing and this document will never call it one.**
It is a band obtained by inverting the two rungs' 95 per cent Wilson intervals through the
same linear rule, under the linearity assumption of §2.1. **Where a bound curve does not
reach 50 per cent inside the bracket, that side is open** and is written as open, never
extrapolated.

**Prediction for `SHIPPING` on grid**, computed here by hand at full Wilson precision
(`Ft8Step6Ladder.Wilson`, `z = 1.959963984540054`, returning percentages —
`tests/Ft8Sharp.Tests/Dsp/Ft8Step6Ladder.cs:255`):

```
Wilson(283, 306) = (88.974, 94.937)      published 89.0 – 94.9
Wilson(138, 306) = (39.618, 50.700)      published 39.6 – 50.7

optimistic curve: hi₁ = 94.937, hi₂ = 50.700
  hi₂ = 50.700 is STILL ABOVE 50 at the lower rung
  -> the optimistic curve does not reach 50 per cent inside [-20, -19]
  -> THIS SIDE OF THE BAND IS OPEN BEYOND -20 dB

pessimistic curve: lo₁ = 88.974, lo₂ = 39.618
  -19 - (88.974 - 50) / (88.974 - 39.618)
  = -19 - 38.974 / 49.356
  = -19 - 0.789657
  = -19.789657
  = -19.79 dB
```

> **PREDICTED BAND FOR `SHIPPING` ON GRID: open beyond -20 dB, to -19.79 dB**, containing
> the point crossing **-19.90 dB**. **The open side is open by 0.70 percentage points** —
> the -20 dB upper bound is 50.700 against the 50.000 it would have to fall below — which is
> the honest statement of what 306 trials can say: *at least as good as -19.79 dB, and this
> ladder cannot put a floor under how much better.*

**And a prediction where the band closes on both sides**, so task 2 exercises both branches
— `Ft8Sharp` on grid, 248 of 306 and 73 of 306:

```
Wilson(248, 306) = (76.280, 85.042)      published 76.3 – 85.0
Wilson( 73, 306) = (19.423, 28.937)      published 19.4 – 28.9

optimistic:  -19 - (85.042 - 50) / (85.042 - 28.937) = -19 - 35.042 / 56.105 = -19.625
pessimistic: -19 - (76.280 - 50) / (76.280 - 19.423) = -19 - 26.280 / 56.857 = -19.462
```

> **PREDICTED BAND FOR `Ft8Sharp` ON GRID: -19.62 dB to -19.46 dB**, containing the point
> crossing **-19.54 dB**. **A band 0.16 dB wide** — which is the sentence §0.0 needs: a
> 0.2 dB difference between two decoders is inside the width of this project's own bracket
> at 306 trials, and cannot be called a win.

**If task 2's computed band does not contain its point crossing, that is a defect in the
implementation and not in ruling 1** — and it is exactly the defect the watched failure is
built to show.

---

## 3. The price, computed and not copied

**`docs/unit246-osd.md` §5 item 4 is why this is a task.** Unit 252 found its wall-clock
figure wrong by about fifteen-fold and the shape of that night had been built on it.

### 3.1 The inputs — unit 255's measured per-trial costs

`docs/unit255-closing-measurement.md:582–587`, over all six rung-placements:

| column | mean ms/trial | range |
|---|---:|---|
| `Ft8Sharp` | 64.8 | 64.0 – 65.6 |
| `Deep all off` | 64.7 | 64.1 – 65.7 |
| `OSD only` | 73.7 | 72.9 – 75.5 |
| `subtraction only` | 88.4 | **64.7 – 166.4** |

> **`subtraction only` is priced at the TOP of its measured range, 166.4 ms.** Its cost
> rises with the decode rate because the stopping rule buys a second pass on every slot that
> returned something — `:592–598`, where the marginal cost is 30.0 ms at -20 dB and
> **101.6 ms at -19 dB on the grid**, three times as much. **Every bracket rung tonight is
> at or above -17 dB**, where the decode rate is higher than at any rung unit 255 walked, so
> 166.4 is the floor of the pessimistic case and not the ceiling.

**Four columns, priced pessimistically: 64.8 + 64.7 + 73.7 + 166.4 = 369.6 ms a trial.**

### 3.2 The predictions

| # | call | trials | arithmetic | **predicted wall clock** |
|---|---|---:|---|---:|
| 1 | one cell-centre bracket rung, four columns | 51 | 369.6 × 51 = 18 850 ms | **18.8 s** |
| 2 | one cell-centre bracket rung, four columns | 306 | 369.6 × 306 = 113 098 ms | **113.1 s** |
| 3 | the coarse search, **five** rungs in one method | 51 each | 18.8 × 5 | **94.2 s** |
| 3′ | the coarse search, **four** rungs (see note) | 51 each | 18.8 × 4 | **75.4 s** |
| 4 | one repeats-panel rung, `repeats: 4`, stacked | 306 | see §3.3 | **≈ 145 s** |

**A note on three against five.** The work instruction's task 1 item 3 asks for *the four
coarse rungs of ruling 3*, and **ruling 3 names five** — -17, -15, -13, -11 and -9. Task 3
item 1 also says five. **Both counts are priced above**; the method will walk up to five and
stop early once every column has been seen on both sides of 50 per cent, so **94.2 s is the
ceiling and 75.4 s or less is the likely outcome.**

### 3.3 The repeats-panel rung, priced against a measured call

Unit 255 §5.3 ran **exactly the call task 4 runs**, at -21 dB, 306 trials, and it took
**145.3 s** (`docs/unit255-closing-measurement.md:724`, and the row block at `:736–739`:
63.8, 72.5 and 295.5 ms a trial, summing to 431.8 ms a trial × 306 = 132.1 s of decode
inside a 145.3 s call).

**-22 and -23 dB are quieter, so fewer slots decode and fewer candidates are raised.** The
combined column's cost is driven by the pairing budget, which is bounded rather than
rate-driven, so **the prediction is 130 – 150 s a rung and does not rise**. A rung at -24 dB,
if licensed, prices the same.

### 3.4 The night, summed, against the ceiling

| task | calls | predicted total |
|---|---:|---:|
| 2 — the crossing arithmetic, no ladder | 1 | **< 5 s** |
| 3 — coarse search + two 306-trial rungs | 3 | 94.2 + 113.1 + 113.1 = **320 s** |
| 4 — two panel rungs (a third if licensed) | 2 – 3 | 290 – **435 s** |
| | **6 – 7** | **≈ 610 – 760 s of foreground decode** |

> **THE DEAREST SINGLE CALL IS PREDICTED AT ABOUT 145 s, against a stated ceiling of 480 s.**
> **Nothing tonight is predicted above 150 s**, which is the check the work instruction asks
> for: no call is near 300 s, so no arithmetic here disagrees with unit 255's measured
> per-trial costs, and there is nothing to report before spending the calls.
>
> **Where the prediction could be wrong, named in advance.** `subtraction only` at -13 or
> -9 dB decodes on nearly every trial and every decode buys a second pass; if its per-trial
> cost runs above 166.4 ms the coarse search grows proportionally. At 250 ms a trial for that
> one column the five-rung search would be 116 s, still far inside the ceiling. **The 306-trial
> rungs are the exposure**: at 250 ms for subtraction the pair costs 133 s each. **Still under
> 150 s, and still under a third of the ceiling.**

---

## 4. What tonight will and will not change in `docs/unit255-closing-measurement.md`

**Amended, visibly, at task 5:**

| section | what changes |
|---|---|
| **head of file** | **new dated amendment note** — *amended by unit 256, 2026-09-05* — what changed, what the earlier text said, and that §3 is untouched |
| **§4.1** (`:500–551`) | **replaced whole.** Every crossing gains its band; the prose figure *three* is corrected to **four** with the earlier wording quoted; every previously unbracketed row carries either a crossing from tonight's rungs or `not bracketed - above -9 dB` |
| **§5 heading** (`:651`) | may gain *and combining on its own ladder* if the new subsection needs it in the title; a cross-reference change only |
| **new §5.4** | *Combining on and off, on its own ladder* — the panel's rungs, its three rows, `OnlyCombined` on each, the crossing with its band, unit 255's unfair-attempt caveat, and the sentence that this ladder is not §3's |
| **§6.1 item 4** (`:840–842`) | the two crossings the operator section quotes gain their bands. **If no other claim in §6.1 moves, §6.1 says so and says it was checked** |

**Left exactly as it is:**

| section | why |
|---|---|
| **§1, §2** | the trace and the method of unit 255's own walks |
| **§3, all thirty-six cells** (`:338–491`) | **not re-run and not touched.** Parked, explicitly |
| **§4.2** (`:553–599`) | exit 3 is met at 336.8 ms and 44.5×; **tonight's walks are on different rungs and columns and disturb nothing in it** |
| **§4.3** (`:601–647`) | the reproduction against the record; nothing tonight re-measures a recorded cell |
| **§5.0, §5.1, §5.2, §5.3** | §5.0's ruling is what licenses §5.4; §5.1–§5.3 are citations and the §5.3 row is cited by task 4 rather than re-run |
| **§6.0, §6.2, §6.3, §6.4** | the synthesizer caveat, what he does not get, the deferred criteria and the closing position |

**Where a cross-reference must follow the new §5.4, it is changed and said so.**

---

## 5. The rungs, the placements and the exact call arguments

**Written out here so tasks 3 and 4 copy rather than re-derive.**

### 5.1 Task 3 — the cell-centre bracket

**Placement**, from `tests/Ft8Sharp.Tests/Dsp/Ft8Unit255ClosingLadderTests.cs:62` and `:64`,
originally unit 248's, **these two numbers and no others**:

```csharp
frequencyHz:    Ft8LadderHarness.DefaultFrequencyHz + 1.56   // 1000.0 Hz + 1.56
offsetSamples:  Ft8LadderHarness.DefaultOffsetSamples + 480  // 3 symbol periods + 480
```

**Four columns**, transcribed from `Ft8Unit255ClosingLadderTests.cs:106–113` — the same
constructor calls, so tonight's rows are the same instruments as §3.2's:

```csharp
var port        = new Ft8SlotDecoder();
var allOff      = new Ft8DeepSlotDecoder();
var osd         = new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default);
var subtraction = new Ft8DeepSlotDecoder(subtraction: Ft8DeepSubtractionSettings.Default);
```

named `Ft8Sharp`, `Deep all off`, `OSD only`, `subtraction only`, in that order — so
`results[0]` is the port and `results[1]` is the attribution column, which is what the
equality assertion compares.

**`fine sync only` and `SHIPPING` are NOT in this walk.** They already have crossings at the
cell centre, -19.61 dB both, and re-running them would cost the two dearest columns on the
board for nothing.

**The call**:

```csharp
Ft8LadderHarness.Run(
    rung, trials, decoders: decoders,
    frequencyHz:   Ft8LadderHarness.DefaultFrequencyHz + 1.56,
    offsetSamples: Ft8LadderHarness.DefaultOffsetSamples + 480)
```

**The rungs.** Coarse search at **51 trials**: -17, -15, -13, -11, -9 dB, climbing, stopping
early once every column has been seen both above and below 50 per cent. **Capped at -9 dB**;
a column not across by there is `not bracketed - above -9 dB` with its rates and the
statement that the search was capped rather than exhausted. Then the straddling pair at
**306 trials**, one method each, the rungs held as named constants set from the coarse search.

**Artefacts**: `docs/unit256-runs/cell-centre-coarse.txt`, then
`docs/unit256-runs/cell-centre-<rung>.txt` per 306-trial rung.

**Assertions, two and only two**: zero wrong on every row, every wrong return printed sent
beside returned; and `Deep all off` equal to `Ft8Sharp` in decoded, missed and wrong.
**No bound on any rate.** Everything printed before anything is asserted.

**THE DROP CANDIDATE, decided now and not when tired: it is NOT taken.** The 306-trial
confirmation of `OSD only`'s bracket is priced inside the same two calls as the other three
columns — all four columns walk together in one `Run`, so dropping `OSD only` would save
73.7 ms a trial out of 369.6, about 22 s of a 113 s call, and would cost the column its
306-trial crossing. **The saving does not justify the loss and the night is priced at half
its ceiling.** This is recorded here, at task 1, which is where the instruction says the
decision is made.

### 5.2 Task 4 — the combining panel

**One placement only**, ruling 5: the same as unit 255 §5.3, so its -21 dB row can be cited
rather than re-run. The first slot starts on grid and every later hearing is jittered
2.00 Hz and 480 samples from the one before it, **so the panel is already a mixed-placement
instrument** and a second column labelled *cell centre* would not mean what that phrase means
in §3.

**The call, transcribed from
`tests/Ft8Sharp.Tests/Dsp/Ft8Unit255RepeatsCellTests.cs:81–89`, with only the first argument
changed:**

```csharp
Ft8LadderHarness.RunRepeats(
    rung, 306, repeats: 4,
    frequencyJitterHz: 2.0, offsetJitterSamples: 480,
    combining: new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3),
    combinedOsd: Ft8DeepOsdSettings.Default,
    combinedFineSync: Ft8DeepFineSyncSettings.Default)
```

> **CHECKED AGAINST THE TREE, ARGUMENT BY ARGUMENT, AND IT MATCHES.** The work instruction
> warned that a difference here would be the most important finding of task 1, because the
> panel's -21 dB row is cited rather than re-run. **There is none.** `seed` is left at
> `Ft8LadderHarness.DefaultSeed`, `frequencyHz` at `DefaultFrequencyHz` and `offsetSamples`
> at `DefaultOffsetSamples` in both, by omission. **The -21 dB row may therefore be cited.**

**Three rows, not four** (`Ft8LadderHarness.cs:518`): `single slot`, `single + OSD`, and the
combined row, **labelled `summed x4`** here because `AccumulationDepth` is 3 and the label is
`summed x{depth+1}` when the depth is above 1 (`:514`). Unit 255 reported the three-not-four
shape as its own mismatch 6; **tonight's panel has three rows and says so.**

**The rungs.** -22 dB and -23 dB, one method each. The combined row reads **83.0 per cent at
-21**, so its crossing lies below -21 dB. **If -23 is still above 50 per cent, one rung at
-24 dB is licensed** and carries unit 247 §1's finding beside it verbatim: *at -24 dB the
sync search does not return a place near the signal, median closest candidate 69 of 174
against a chance distance of 87* — measured **with fine sync off and combining at depth one,
which is not tonight's configuration**, and saying so is the whole justification for the
rung. **Nothing below -24 dB.** If -24 does not bracket: `not bracketed - below -24 dB`, stop.

**Artefacts**: `docs/unit256-runs/combining-panel-<rung>.txt`.

**Assertions, three**: zero wrong on every row with every wrong return printed;
`CombinedDecodes == CombinedDecodesVerified`; and `DeepestHearings == 4`, because B17 is a
column that claimed four hearings and summed two. **No bound on any rate.**

### 5.3 The cited -21 dB row, for the panel's own table

From `docs/unit255-closing-measurement.md:736–739`, `Ft8Sharp.Deep` **0.8.0**, which is the
version in the tree tonight — **so it is cited, not re-run**:

| row | decoded | rate | Wilson 95 % | ms/trial |
|---|---:|---:|---|---:|
| `single slot` | **13 of 306** | 4.2 % | 2.5 – 7.1 | 63.8 |
| `single + OSD` | **33 of 306** | 10.8 % | 7.8 – 14.8 | 72.5 |
| **`summed x4`** | **254 of 306** | **83.0 %** | **78.4 – 86.8** | 295.5 |

`OnlyCombined` **206 of 306**, `AnySlotAlone` **48**, `LostByCombining` **0**,
`CombinationsSubmitted` **2 232**, `CodewordsAccepted` **736**, `CombinedDecodes` **458**,
`CombinedDecodesVerified` **458**, wrong **0**, expected false accepts **0.136**,
`DeepestHearings` **4**, worst slot **109.6 ms**, 137×, whole call **145.3 s**.

---

## 6. Predictions, and what they turned out to be

**Filled in as the night proceeds. Nothing here is written before it is measured.**

| # | prediction | outcome |
|---|---|---|
| P1 | `SHIPPING` on grid: point -19.90 dB, band **open beyond -20 dB to -19.79 dB** | **exact.** `-19.896552` dB, `open beyond -20.0 dB to -19.789651 dB` |
| P2 | `Ft8Sharp` on grid: point -19.54 dB, band **-19.62 to -19.46 dB**, 0.16 dB wide | **exact.** `-19.5429` dB, `-19.62 to -19.46`, **0.162 dB** wide |
| P3 | the coarse search costs **≤ 94.2 s** | *task 3* |
| P4 | each 306-trial cell-centre rung costs **≈ 113 s** | *task 3* |
| P5 | each panel rung costs **130 – 150 s** | *task 4* |
| P6 | every row of every walk reads **zero wrong** | *tasks 3, 4* |
| P7 | `Deep all off` equals `Ft8Sharp` on every cell-centre rung | *task 3* |

**And a correction to §2.3's own arithmetic, which changes nothing.** The hand computation
wrote `Wilson(283, 306)`'s upper bound as `94.937`; the code returns **`94.940`**, and
`Wilson(248, 306)` as `(76.280, 85.042)` against the code's **`(76.28, 85.04)`**. **The hand
slip is in the fourth significant figure and no band in §2.3 moves because of it** — the
optimistic side of `SHIPPING` was open either way, and both rounded band ends are exactly as
predicted. The digits above are the computed ones.

### 6.1 All eight published crossings, with the band the rungs support

`docs/unit256-runs/crossing-bands.txt`, task 2, computed from committed counts, **no ladder
walked**. Every one of the eight reproduces unit 255's published point crossing.

| column | placement | point | **band** | width |
|---|---|---:|---|---:|
| `Ft8Sharp` | on grid | -19.54 | -19.62 to -19.46 dB | 0.162 dB |
| `Deep all off` | on grid | -19.54 | -19.62 to -19.46 dB | 0.162 dB |
| `fine sync only` | on grid | -19.66 | -19.75 to -19.58 dB | 0.167 dB |
| `OSD only` | on grid | -19.81 | -19.92 to -19.71 dB | 0.209 dB |
| **`SHIPPING`** | **on grid** | **-19.90** | **open beyond -20.0 dB to -19.79 dB** | **open** |
| `subtraction only` | on grid | -19.54 | -19.62 to -19.46 dB | 0.162 dB |
| `fine sync only` | cell centre | -19.61 | -19.67 to -19.55 dB | 0.127 dB |
| **`SHIPPING`** | **cell centre** | **-19.61** | **-19.67 to -19.55 dB** | **0.126 dB** |

> **THE NARROWEST BAND IN THIS TABLE IS 0.126 dB WIDE AND ONE IS OPEN.** A 0.2 dB
> difference between two decoders is inside the width of this project's own bracket at 306
> trials, and no sentence this project writes may call one a win over the other on that
> margin.

### 6.2 The gate-set judgement, and it is yes

**The test earns entry 13 and a `docs/breakage-record.md` entry, `B18`**, written at task 6.
Judged against `docs/gate-set.md`'s own rule 5 — *no test is added without naming the
breakage it would have caught*:

- **It names a breakage that actually happened, tonight, and was watched happening**
  (`docs/unit256-runs/task2-watched-failure.txt`). `B11` is the precedent: a check written
  against a placeholder token that could not see the token until it was watched failing.
- **The defective pairing does not throw and does not look wrong.** It printed a complete,
  plausible, publication-shaped table in which **every width was negative**, `Ft8Sharp` read
  0.021 dB against the 0.162 dB its rungs support, and `SHIPPING` claimed a closed
  optimistic bound where the measurement has none. **Nothing but the containment check
  caught it**, and no number in the printed table announces itself as wrong to a reader.
- **It costs 6 ms.** It walks no ladder and decodes nothing. It would be **the cheapest
  entry in the set**, ahead of entry 12's 0.6 s.
- **It guards the number this project will be quoted on for years.** The line 57 exclusion —
  *the ladder is a measurement, not a test* — does not reach it, because this is arithmetic
  on committed counts and not a walk.

---

## 7. Where the deliverable is

**`docs/unit255-closing-measurement.md` is the phase's closing statement and stays ONE
document.** Tonight's numbers land there:

- **§4.1**, replaced — every crossing with its band, both placements, the count corrected to
  four, every previously unbracketed row resolved or reported against its ceiling.
- **§5.4**, new — combining on and off, on its own ladder, with its own crossing.
- **§6.1 item 4**, re-read — the operator's two crossings with their bands.

**This file is the working record and does not duplicate them.**
