# Unit 257 — combining at the closing table's own rungs, and at both placements

**Work instruction 257.** `Ft8Sharp.Deep` 0.8.0, `Ft8Sharp` 0.10.7, root 1.12.58 → 1.12.59.
Written 2026-09-05, from `HEAD d80fb6f` on `main`.

**What this unit was commissioned to ask.** Step 6's exit criterion 1 asks for each stage on
and off at **-19, -20 and -21 dB, on grid and at the cell centre, 306 trials a cell, with
wrong counts**. Combining is the one stage that has never been answered at those
coordinates. Unit 256 answered it at -21, -22 and -23 dB in one mixed placement, and the
judging session returned `partial` on exactly that gap.

---

## 1. The trace, and the price of the night before it is spent

**No test ran in this task.** Everything below is read out of the tree at file and line, or
out of a committed artefact.

### 1.1 The arithmetic the whole design turns on — `Ft8LadderHarness.cs:573-574`

`Ft8LadderHarness.RunRepeats` is at **`:472`** and its signature carries the four parameters
that matter, at `:477` – `:480`:

```csharp
double frequencyHz = DefaultFrequencyHz,
int? offsetSamples = null,
double frequencyJitterHz = 0.0,
int offsetJitterSamples = 0,
```

`offsetSamples` is resolved once at **`:490`**, `var offset = offsetSamples ?? DefaultOffsetSamples;`.

Inside the per-trial loop, once per repeat `r`, at **`:571-574`**, verbatim:

```csharp
// THE HARDER VARIANT: a later slot need not sit on the same sample or the same
// bin. A combiner that only works when it does is not a decoder.
var slotFrequency = frequencyHz + (r * frequencyJitterHz);
var slotOffset = offset + (r * offsetJitterSamples);
```

and those two locals are what the synthesiser is handed at **`:576-577`**
(`SearchFixture.OneSignal(Rate, entry, slotFrequency, slotOffset)`).

**In my own words: reading 2 holds, exactly and without qualification.** `frequencyHz` and
`offsetSamples` are the origin — the placement of hearing `r = 0` — and every later hearing
steps from that origin by `r` times the jitter. Setting `frequencyJitterHz: 0.0` and
`offsetJitterSamples: 0` collapses both expressions to `slotFrequency = frequencyHz` and
`slotOffset = offset` **for every `r`**, so all four hearings of every trial sit at exactly
the stated placement. There is no other term anywhere in `:472` – `:600` that moves a slot's
frequency or offset — the only other per-repeat variation is the noise draw, one
`GaussianNoise` per repeat per block at `:553-557`, which is what makes four hearings worth
having.

> **Therefore *on grid* and *at the cell centre* mean on this panel exactly what they mean in
> §3.1 and §3.2 of the closing document, and the placement label is true rather than
> approximate.**

**Reading 3's fallback is NOT taken.** The panel is labelled *placement*, not *first-hearing
placement*, and it is entitled to be.

One consequence that must travel with the panel, and it is a real cost of the choice: with
the jitter gone, **the four hearings differ only in their noise**, which is the easy case —
a station whose oscillator and clock did not move at all across two minutes. §1.5 below says
what that does and does not model.

### 1.2 Unit 256's call, transcribed argument for argument

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit256CombiningPanelTests.cs:112` – `:120`, read and
transcribed rather than assumed:

```csharp
var run = Ft8LadderHarness.RunRepeats(
    rung,
    Trials,
    repeats: 4,
    frequencyJitterHz: 2.0,
    offsetJitterSamples: 480,
    combining: new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3),
    combinedOsd: Ft8DeepOsdSettings.Default,
    combinedFineSync: Ft8DeepFineSyncSettings.Default);
```

`Trials` is `306` at `:54`. Argument for argument against tonight's call:

| argument | unit 256 | unit 257 on grid | unit 257 cell centre | same? |
|---|---|---|---|---|
| `rungDecibels` | -22.0, -23.0 | **-19.0, -20.0, -21.0** | **-19.0, -20.0, -21.0** | rungs differ — that is the criterion |
| `trials` | 306 | 306 | 306 | **identical** |
| `repeats` | 4 | 4 | 4 | **identical** |
| `seed` | not passed → `DefaultSeed` 221 001 (`:61`) | not passed | not passed | **identical** |
| `frequencyHz` | not passed → `DefaultFrequencyHz` 1000.0 (`:64`) | `DefaultFrequencyHz` | **`DefaultFrequencyHz + 1.56`** | differs at cell centre only |
| `offsetSamples` | not passed → `DefaultOffsetSamples` (`:69`) | `DefaultOffsetSamples` | **`DefaultOffsetSamples + 480`** | differs at cell centre only |
| `frequencyJitterHz` | **2.0** | **0.0** | **0.0** | **differs — this is the unit** |
| `offsetJitterSamples` | **480** | **0** | **0** | **differs — this is the unit** |
| `combining` | `new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3)` | identical | identical | **identical** |
| `log` | not passed → null | not passed | not passed | **identical** |
| `combinedOsd` | `Ft8DeepOsdSettings.Default` | identical | identical | **identical** |
| `combinedFineSync` | `Ft8DeepFineSyncSettings.Default` | identical | identical | **identical** |

**Every argument the instruction said would be identical is identical, and I say so having
read `:112` – `:120` rather than having assumed it.** The instruction's transcription of that
call is correct in every argument.

**Three rows, not four**, from `:514` – `:523`: `single slot`, `single + OSD`, and the third
labelled from `rule.AccumulationDepth == 1 ? $"combined x{repeats}" : $"summed x{rule.AccumulationDepth + 1}"`.
With `accumulationDepth: 3` that is **`summed x4`**. That distinction is `B17`. Tonight's
panels have three rows and say so.

### 1.3 The zero-jitter consistency check — **and the instruction points at the wrong section and the wrong number**

The instruction says to find *unit 247 §2, 49 of 51 at -21 dB with no jitter*. **That figure
is not a decode rate and §2 is not where the zero-jitter walk is.**

`docs/unit247-combining.md` **§2 — *The pairing, measured before it was designed*** (`:111`)
is a table of **candidate geometry**, not of decodes (`:113` – `:118`):

| | median | max | within one bin, 3.125 Hz / 0.16 s |
|---|---|---|---|
| frequency gap | 0.00 Hz | 1703.13 Hz | **49 of 51** |
| time gap | 0.000 s | 2.240 s | **49 of 51** |

*49 of 51* there is **how many trials had the two slots' closest candidates inside one
waterfall bin and inside 0.16 s** — a statement about whether the pairing rule can find its
partner, over 51 trials, not about whether anything decoded.

**The zero-jitter decode figure is in §4** (`:205`, *-21 dB, same placement — both slots on
the same bin and the same sample*):

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
single slot      -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     64.4
single + OSD     -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.6
combined x2      -21.0    -21.000     306      217      89      0    70.9    65.6    75.7    129.0
```

and its jittered counterpart is at `:233`, where `combined x2` reads **68 of 306, 22.2 %** —
which *is* the 68 of 306 the instruction quotes, so the instruction has the jittered number
right and the zero-jitter number wrong.

> **THE CONSISTENCY CHECK TONIGHT'S ON-GRID PANEL IS READ AGAINST:
> `combined x2` = 217 of 306, 70.9 % (Wilson 65.6 – 75.7), zero wrong, at -21.0 dB, on grid,
> zero jitter, `docs/unit247-combining.md` §4.**
> Trial count **306**. Repeat count **two**. `OnlyCombined` 200 of 306. Combining stage added
> 211 messages, 211 of them the message that was sent, 0 not sent.

**It is a sanity bound and not a reproduction, and the difference is not small.** Unit 247's
row is **two** hearings, `combined x2`, with **no** accumulation depth and **no** stacked
stages. Tonight's is **four** hearings, `summed x4`, at `accumulationDepth: 3`, with ordered
statistics and fine sync stacked on the combined column's inner decoder, on `Ft8Sharp.Deep`
0.8.0 rather than whatever version unit 247 ran. **So the only thing it licenses is a
direction: tonight's on-grid `summed x4` at -21 dB should be at or above 217 of 306, because
it has strictly more to work with.** If it comes back materially below 217, something is
wrong and this document says so. It is not a figure tonight's row is compared to as an equal
and it is never put in a table beside one.

The two combining-**off** rows are a tighter check, because they do not depend on the
combiner at all: `single slot` and `single + OSD` see only slot 0, which at zero jitter is
the same audio unit 247 gave them. **They should read 13 of 306 and 33 of 306 at -21 dB on
grid** — which is also §3.1's `Ft8Sharp` and `OSD only` at the same rung, and also unit
256's cited §5.3 row. Three independent records agree on those two numbers, so tonight's
on-grid -21 dB call has a hard consistency check on two of its three rows.

### 1.4 The price, computed before it is spent

**Per-slot costs are flat across all six rung-placements**, read out of
`docs/unit255-closing-measurement.md` §3.1 (`:378`) and §3.2 (`:426`):

| column | -19 grid | -20 grid | -21 grid | -19 centre | -20 centre | -21 centre |
|---|---:|---:|---:|---:|---:|---:|
| `Ft8Sharp` (the port) ms/trial | 64.0 | 64.6 | 64.9 | 65.6 | 65.0 | 64.5 |
| `OSD only` ms/trial | 73.1 | 73.3 | 73.4 | 75.5 | 74.1 | 72.9 |

**The model, calibrated against the one call that has actually been run.**
`docs/unit255-closing-measurement.md` §5.3 (`:821`) ran tonight's call, jittered, at -21 dB,
306 trials, and its three row clocks are at `:840` – `:842`: `single slot` 19.5 s
(63.8 ms/tr), `single + OSD` 22.2 s (72.5 ms/tr), `summed x4` 90.4 s (295.5 ms/tr). Row time
132.1 s; **wall clock 145.3 s**; the 13.2 s difference is the four-slot synthesis, which is
per trial and not per column.

The combined column at 295.5 ms/trial is **4 × 73.9 ms a slot** — the `OSD only` per-slot
cost, and *not* the `SHIPPING` per-slot cost. So:

```
predicted row time = 306 x (65 + 75 + 4 x 74) ms  =  306 x 436 ms  =  133.4 s
predicted wall     = 133.4 s + 13.2 s synthesis   =  ~147 s per rung-placement
```

against §5.3's measured 145.3 s at the one rung-placement that has been run. **Every one of
tonight's six required calls is predicted at ~147 s**, because every input to that sum is
flat across the six rung-placements in the table above.

| # | call | rung | placement | predicted wall |
|---|---|---|---|---:|
| 1 | `ThePlacementPanelOnGridAtMinus21` | -21.0 | on grid | **147 s** |
| 2 | `ThePlacementPanelOnGridAtMinus20` | -20.0 | on grid | **147 s** |
| 3 | `ThePlacementPanelOnGridAtMinus19` | -19.0 | on grid | **148 s** |
| 4 | `ThePlacementPanelAtCellCentreAtMinus21` | -21.0 | cell centre | **147 s** |
| 5 | `ThePlacementPanelAtCellCentreAtMinus20` | -20.0 | cell centre | **148 s** |
| 6 | `ThePlacementPanelAtCellCentreAtMinus19` | -19.0 | cell centre | **150 s** |
| 7 *(optional, task 4)* | extension rung on grid | -22.0 | on grid | **45 s** |
| 8 *(optional, task 4)* | extension rung at centre | -22.0 | cell centre | **45 s** |
| 9 *(optional, task 4)* | extension rung on grid | -23.0 | on grid | **45 s** |
| 10 *(optional, task 4)* | extension rung at centre | -23.0 | cell centre | **45 s** |

The four optional prices are unit 256's own measured wall clocks —
`docs/unit256-runs/combining-panel-minus22.txt:63` reads **45.0 s** and
`combining-panel-minus23.txt:64` reads **44.6 s**. They are far cheaper than the required
calls because at -22 dB and below almost nothing clears the sync threshold: the `single slot`
row costs 18.5 ms/trial at -22 dB against 63.8 at -21.

Add per-invocation `dotnet test` build and discovery overhead, measured on units 252 – 256 at
roughly 20 – 40 s, and **no call is expected to exceed about 190 s of observed wall clock.**

> **THE SPLIT RULE, STATED BEFORE ANYTHING IS SPENT.** Tonight's ceiling is **480 s**. **A
> call predicted above 300 s is split by rung into more test methods and is never
> backgrounded.** Nothing is predicted above 300 s, so the six required calls stay one method
> each — which is also what keeps a red at one rung from stopping the other five.

**One pricing disagreement, stated before the call is spent, as the instruction requires.**
There are two defensible per-slot costs for the combined column's inner decoder, and they
differ by 2.8×:

- **74 ms a slot** — what §5.3 actually measured for this exact call, and what unit 254 §4c's
  *+18.2 ms a trial over two slots* independently gives. This is the one used above.
- **204 ms a slot** — §3.1's `SHIPPING` column at -21 dB, which is the *same* stage
  combination the combined column's inner decoder is built with
  (`osd: Ft8DeepOsdSettings.Default, fineSync: Ft8DeepFineSyncSettings.Default`, plus
  `rememberHearings: true`).

Priced the second way, a call comes to `306 × (65 + 75 + 4 × 204) ms ≈ 286 s` — still under
the 300 s split line, so **the split rule's answer is the same either way and no call is
restructured on account of it.** But the discrepancy is real and it is reported as an
observation rather than chased: the combined column appears not to be paying for fine sync
at anything like the rate §3's `fine sync only` column pays. `Ft8DeepSlotDecoder.cs:747`
gates fine sync on `result.Status != Ft8CodewordStatus.Decoded`, and `:267` names a counter
`Ft8DeepFineSyncCounts.RefusedForWantOfSamples`, so there is a plausible mechanism; **`RunRepeats`
does not surface `LastFineSync`, so this unit cannot settle it without changing a shared
harness, and reading 6 forbids touching `src/`.** It is carried to section 4 of the report as
an observation and it blocks nothing.

### 1.5 The placement arithmetic, and what it does and does not model

The waterfall's coarse bin is **3.125 Hz** (`Ft8WaterfallGeometry`, and it is the figure
`docs/unit247-combining.md` §2 quotes as *one bin*). The cell-centre offset is **+1.56 Hz**,
taken from `tests/Ft8Sharp.Tests/Dsp/Ft8Unit255ClosingLadderTests.cs:62`, and
**+480 samples**, from `:64` of the same file — unit 248's two constants, and no others, so
tonight's rows are comparable with §3.2's.

```
1.56 / 3.125 = 0.4992 of a bin — half a bin, to within a thousandth
```

**So the cell centre is the worst place in the analysis grid to land**: exactly as far as it
is possible to be from the nearest bin centre in frequency, and 480 samples into the symbol
period in time. On grid is `DefaultFrequencyHz = 1000.0` (`Ft8LadderHarness.cs:64`), a bin
centre, and `DefaultOffsetSamples`, three whole symbol periods in (`:69`).

**With zero jitter, all four hearings of a cell-centre trial sit at +1.56 Hz and +480
samples.** What that models:

- **It models a station whose four transmissions land in the same place off the grid.** That
  is what a stable oscillator and a disciplined clock do over the two minutes four FT8 slots
  take, and it is the ordinary case rather than a contrived one — the offset from the bin
  centre is a property of where the operator set his transmit frequency, not of drift, and it
  does not move between overs.
- **It does not model drift.** A station whose oscillator walks between hearings is §5.4's
  jittered panel, at 2.00 Hz and 480 samples per hearing, and that panel is unit 256's, it
  stands unchanged, and it is not re-run tonight.
- **It is the easier of the two for the combiner**, because every hearing lands on the same
  bin and the same sample, so the pairing rule's partner is always geometrically adjacent.
  Unit 247 measured that difference directly at two hearings: **217 of 306 aligned against 68
  of 306 jittered** at -21 dB. `HM-OPEN-075` is that cost and it is not this unit's to close.

**The two panels are therefore a pair of bounds on the same question, not a correction of one
another**, and the closing document keeps both with the difference on their faces: §5.4 is
the conservative reading and §5.5 is the aligned-repeats reading.

### 1.6 What the panel asserts, and what it does not

Per row, and nothing else: **`Wrong == 0`**, and **`CombinedDecodesVerified == CombinedDecodes`**.

**No bound is asserted on any rate**, at any rung, on either placement — the rate is the
measurement, and reading 5 rules that saturation is a result. **No bound is asserted on any
crossing.**

**`DeepestHearings == 4` is printed but NOT asserted**, and that is a deliberate departure
from unit 256's panel, which asserts it at `Ft8Unit256CombiningPanelTests.cs:214`. The
instruction names two assertions for tonight and not three. Unit 256's own summary at `:266`
– `:271` records why the third is unsafe as a general assertion: at -23 dB `DeepestHearings`
was **3**, not because of `B17` but because so little decoded that no slot ever held four
hearings' worth of candidates to accumulate. **On a walk whose rung is the variable, an
assertion that only means something at rungs where the combiner has candidates would
manufacture a red for a measurement**, which `docs/gate-set.md:57` forbids. It is printed on
every panel so the reader can see it, and where it is not 4 the panel says so in words.

---

## 2. The panel — combining on and off, three rungs, both placements, zero jitter

**LADDER:** `Ft8LadderHarness.RunRepeats`, **four slots a trial** carrying the same message.
**JITTER: zero on both axes**, so every hearing of every trial sits at the stated placement.
**306 trials a cell. `Ft8Sharp.Deep` 0.8.0.** Every row in a cell sees identical audio, so the
comparison within a cell is paired.

> **NO ROW HERE IS COMPARABLE WITH A ROW IN §3 OF THE CLOSING DOCUMENT**, which gives each
> trial **one** slot where this gives each trial **four**, **nor with a row in §5.4**, which is
> the jittered panel. §5.0 rules the first and reading 4 rules the second.

### 2.1 On the grid — 1000.0 Hz, three whole symbol periods in

| row | what it is | **-19 dB** | **-20 dB** | **-21 dB** | wrong |
|---|---|---:|---:|---:|---:|
| `single slot` | **combining OFF** — the port on the first slot alone | 248 of 306, 81.0 % (76.3 – 85.0) | 73 of 306, 23.9 % (19.4 – 28.9) | 13 of 306, 4.2 % (2.5 – 7.1) | **0** |
| `single + OSD` | **combining OFF** — the sibling with ordered statistics, same first slot | 276 of 306, 90.2 % (86.3 – 93.0) | 125 of 306, 40.8 % (35.5 – 46.4) | 33 of 306, 10.8 % (7.8 – 14.8) | **0** |
| **`summed x4`** | **combining ON** — four hearings accumulated three deep, shipping stages stacked | **306 of 306, 100.0 %** (98.8 – 100.0) | **306 of 306, 100.0 %** (98.8 – 100.0) | **306 of 306, 100.0 %** (98.8 – 100.0) | **0** |
| | `OnlyCombined` — trials no single slot decoded alone and the combination did | 1 of 306 | 39 of 306 | **200 of 306** | |
| | `AnySlotAlone` | 305 of 306 | 267 of 306 | 106 of 306 | |
| | `LostByCombining` | **0** | **0** | **0** | |
| | combinations submitted / accepted | 2 856 / 2 415 | 2 517 / 2 027 | 2 110 / 1 510 | |
| | combined decodes / **verified** | 98 / **98** | 491 / **491** | 713 / **713** | |
| | `DeepestHearings` | 4 | 4 | 4 | |
| | worst single slot | 121.6 ms, **123×** | 93.5 ms, **160×** | 92.3 ms, **162×** | |
| | wall clock | 145.2 s | 144.5 s | 144.0 s | |

Artefacts: `docs/unit257-runs/placement-panel-on-grid-minus19.txt`, `-minus20.txt`,
`-minus21.txt`.

### 2.2 At the cell centre — +1.56 Hz, +480 samples

| row | what it is | **-19 dB** | **-20 dB** | **-21 dB** | wrong |
|---|---|---:|---:|---:|---:|
| `single slot` | **combining OFF** — the port on the first slot alone | 6 of 306, 2.0 % (0.9 – 4.2) | 0 of 306, 0.0 % (0.0 – 1.2) | 0 of 306, 0.0 % (0.0 – 1.2) | **0** |
| `single + OSD` | **combining OFF** — the sibling with ordered statistics, same first slot | 33 of 306, 10.8 % (7.8 – 14.8) | 1 of 306, 0.3 % (0.1 – 1.8) | 0 of 306, 0.0 % (0.0 – 1.2) | **0** |
| **`summed x4`** | **combining ON** — four hearings accumulated three deep, shipping stages stacked | **306 of 306, 100.0 %** (98.8 – 100.0) | **270 of 306, 88.2 %** (84.1 – 91.4) | **75 of 306, 24.5 %** (20.0 – 29.6) | **0** |
| | `OnlyCombined` — trials no single slot decoded alone and the combination did | 196 of 306 | **262 of 306** | **75 of 306** | |
| | `AnySlotAlone` | 110 of 306 | 8 of 306 | **0 of 306** | |
| | `LostByCombining` | **0** | **0** | **0** | |
| | combinations submitted / accepted | 6 223 / 4 173 | 4 467 / 1 515 | 2 480 / 140 | |
| | combined decodes / **verified** | 689 / **689** | 483 / **483** | 87 / **87** | |
| | `DeepestHearings` | 4 | 4 | 4 | |
| | worst single slot | 96.9 ms, **155×** | 125.4 ms, **120×** | 114.4 ms, **131×** | |
| | wall clock | 150.4 s | 149.4 s | 145.9 s | |

Artefacts: `docs/unit257-runs/placement-panel-cell-centre-minus19.txt`, `-minus20.txt`,
`-minus21.txt`.

### 2.3 The consistency checks, and every one of them held

**The two combining-OFF rows see only slot 0**, which at zero jitter is exactly the audio §3
walked. They are therefore a hard check on whether the placement is the placement §3 means,
and they reproduce it **to the decode in all six cells**:

| rung-placement | tonight `single slot` | §3's `Ft8Sharp` | tonight `single + OSD` | §3's `OSD only` |
|---|---:|---:|---:|---:|
| -19 on grid | **248** | 248 | **276** | 276 |
| -20 on grid | **73** | 73 | **125** | 125 |
| -21 on grid | **13** | 13 | **33** | 33 |
| -19 cell centre | **6** | 6 | **33** | 33 |
| -20 cell centre | **0** | 0 | **1** | 1 |
| -21 cell centre | **0** | 0 | **0** | 0 |

**Twelve figures, twelve exact matches**, against §3.1 (`:378`) and §3.2 (`:426`). At -21 dB on
grid the same two figures also match §5.3's cited row and `docs/unit247-combining.md` §4 — four
independent records agreeing on 13 and 33 of 306.

**The combined row's directional bound also held.** Unit 247 §4's aligned `combined x2` reads
**217 of 306** at -21 dB on grid at two hearings, unaccumulated and unstacked; tonight's
`summed x4` at the same rung and placement reads **306 of 306**, which is at or above it as the
bound required. It is a sanity bound and not a reproduction — four hearings, `accumulationDepth:
3`, stacked — and the two are never set side by side as equals.

### 2.4 What the panel says, before any crossing is computed

1. **On the grid, combining is saturated at all three of the closing table's rungs.** 306 of
   306 at -19, -20 and -21 dB. That is reading 5's saturation and it is **a result, not a
   fault**: no bound is asserted on any rate and nothing was chased down the ladder inside
   tasks 2 and 3. What it means is that *at the ratios the closing table is quoted at, a
   station heard four times on the grid is decoded every time.*
2. **Off the grid, the two combining-off rows collapse and the combined row does not.** At -20
   dB the port reads **0 of 306** and combining reads **270 of 306**; at -21 dB the port and
   ordered statistics both read **0 of 306** and combining reads **75 of 306**. **At -21 dB off
   the grid, `AnySlotAlone` is 0 — nothing decodes at all without combining, and every one of
   the 75 decodes is a trial no single slot could reach.**
3. **`LostByCombining` is 0 in all six cells.** Combining is a strict superset of the single
   slot on this audio at every rung and both placements. It never took a decode away.
4. **Zero wrong in all eighteen rows, and 2 561 combined decodes with 2 561 verified.** Against
   naive false-accept expectations from 0.129 to 0.380 across the six cells. **No wrong decode
   appeared at any rung tonight**, at either placement.
5. **The worst single slot anywhere tonight is 125.4 ms** — a **120× margin** against FT8's
   15 000 ms, at -20 dB at the cell centre.
6. **`DeepestHearings` is 4 in all six cells**, so the `summed x4` label is earned everywhere on
   this panel and `B17` does not bite. It is printed and not asserted, for the reason in §1.6.

---

## 3. The crossings, and the drop decision made before the calls were spent

### 3.1 THE DROP DECISION — taken at the start of task 4, 2026-09-05 20:43

**Which columns the table's own three rungs already bracket**, read off §2 before anything was
decided:

| column | placement | -19 | -20 | -21 | straddles 50 %? |
|---|---|---:|---:|---:|---|
| `single slot` | on grid | 81.0 | 23.9 | 4.2 | **yes, by -19 / -20** |
| `single + OSD` | on grid | 90.2 | 40.8 | 10.8 | **yes, by -19 / -20** |
| `summed x4` | on grid | 100.0 | 100.0 | 100.0 | **no — still above 50 at the deepest rung** |
| `single slot` | cell centre | 2.0 | 0.0 | 0.0 | **no — already below 50 at the shallowest rung** |
| `single + OSD` | cell centre | 10.8 | 0.3 | 0.0 | **no — already below 50 at the shallowest rung** |
| `summed x4` | cell centre | 100.0 | 88.2 | 24.5 | **yes, by -20 / -21** |

**Three of six are bracketed inside the table's own rungs and need nothing.** Of the three that
are not:

- **The two cell-centre combining-OFF rows are unbracketed *upward*** — they are already below
  50 per cent at -19 dB, the shallowest rung the closing table quotes. **The named drop
  candidate is the *downward* extension and it cannot reach them.** Walking up the ladder is
  not licensed by this instruction and is not done. They are reported as **`not bracketed —
  above -19 dB`**, against that stated ceiling, and nothing is extrapolated.
- **`summed x4` on the grid is unbracketed *downward*** and is the only row the named drop
  candidate could serve.

> **DECISION: SPEND THE DOWNWARD EXTENSION, ON THE GRID ONLY. Taken now, at the start of task
> 4, on the clock and on task 1's prices — not at the end.**

**The reason.** `summed x4` on the grid at zero jitter is the headline configuration of the
night — it is the one the phase's closing sentence about combining will be written from — and
it is the only one of tonight's six columns that would otherwise have no crossing at all. Unit
256 spent its extension rungs and brought **-21.48 dB** into this project for the jittered
panel; the aligned counterpart is the number that pairs with it, and *not bracketed — below -21
dB* would leave the pair half-written. **On the clock**: the six required calls are done, the
night began at 20:18 and it is 20:43, and §1.4 prices an extension rung at **45 s** from unit
256's own measured wall clocks at -22 and -23 dB. **The search is capped at -23 dB** and a
column unbracketed there is reported against that stated ceiling.

**What is spent and in what order.** One call at **-22.0 dB on grid**. If `summed x4` is still
above 50 per cent there, **one further call at -23.0 dB on grid, and nothing below it.** The
cell centre gets no extension rung, because its `summed x4` is already bracketed by -20 / -21
and its other two rows cannot be reached downward.

**The risk, accepted with the decision.** An extension rung is likelier than any other call
tonight to return a wrong decode — `HM-OPEN-082` was found at -23 dB on the jittered panel at
four times the exposure of a single-slot row. **Rule 2 of task 2 governs and is not relaxed for
these calls:** a wrong return is printed with sent beside returned, the assertion is **left
red**, it is carried to the report as a second observation against `HM-OPEN-082`, and it is not
weakened, not chased and not allowed to stop anything else.

**No new arithmetic is written.** The crossings are computed by `Ft8Unit256CrossingBand`, which
is already in the tree, already in the gate set at entry 13 and already watched failing once as
`B18`. **No second crossing helper is written and no watched failure is manufactured**, because
a caller of existing arithmetic is not new arithmetic and a ladder walk is a measurement —
`docs/gate-set.md:57` and rule 5.
