# Subtracting a decoded message and reading the slot again — the trace, the arithmetic, and what it bought

**Written by unit 253, the first unit of this phase to attempt step 4.** Everything
in §1 to §6 was read on this tree on 2026-09-05 at `HEAD 3bd4c51`, with `Ft8Sharp`
`0.10.7` and `Ft8Sharp.Deep` `0.6.0` going in. §7 onward is measured and each table
names the test that produced it.

Nothing in §1 to §6 is a measurement. Where it names a cost, the cost is either
arithmetic that is shown or a figure already recorded in this repository with its
unit beside it. **§4 is entirely prediction and is labelled as such**; §7 says
whether the prediction held.

---

## 1. Where a second pass can happen at all, and what it costs

### 1.1 Only one entry point has the samples

`Ft8DeepSlotDecoder` has two public decode entry points and one private body:

| Member | Line | Has samples? |
|---|---|---|
| `Decode(ReadOnlySpan<float> samples)` | `src/Ft8Sharp.Deep/Ft8DeepSlotDecoder.cs:307` | **yes** — it analyses with `new Ft8Monitor(Geometry).Analyse(samples)` and passes the same span on |
| `Decode(Ft8Waterfall waterfall)` | `:325` | **no** — it calls the private body with `ReadOnlySpan<float>.Empty` |
| `Decode(Ft8Waterfall, ReadOnlySpan<float>)` (private) | `:337` | whichever it was given |

**A subtraction needs the samples for three separate reasons and a waterfall supplies
none of them.** A waterfall is magnitudes on a fixed grid, quantised to half a
decibel, with no phase; you cannot fit a carrier phase to it, you cannot subtract a
time-domain waveform from it, and there is nothing to re-analyse afterwards. This is
the same wall unit 248 hit for fine sync, and `FineSync`'s own remarks at `:257-263`
say so in terms.

### 1.2 What the waterfall-only entry point does when asked to subtract

**It refuses loudly at construction, not silently at decode time, and that is a
deliberate departure from what fine sync does.**

Fine sync's answer at `:449-454` is to count the candidates it could not touch in
`Ft8DeepFineSyncCounts.RefusedForWantOfSamples` and carry on. That is defensible for
fine sync, which is a per-candidate rescue: the slot still decodes, it just decodes
without the rescue. **It is not defensible for subtraction**, because subtraction is
not a per-candidate rescue — it is a whole extra pass over a whole slot, and a caller
who asked for three passes and silently got one has been told a decode ran that did
not run. Unit 249 found fine sync refusing 42 of 42 candidates in exactly this shape
and nobody noticed until a unit went looking.

So the rule this unit implements:

> **`Decode(Ft8Waterfall)` with subtraction configured throws.** The message names
> the entry point that does have the samples. There is no counted skip and no
> zero-pass result, because a result that says *one pass* when four were asked for is
> a result a reader will average.

The count is still kept — `Ft8DeepSubtractionCounts.RefusedForWantOfSymbols` — but it
is for the *other* refusal, the one in §3.3, which is a per-message fact and is
correctly a count rather than a throw.

### 1.3 The price of one extra pass, from the record

A pass is a whole `Decode(ReadOnlySpan<float>)`: `Ft8Monitor.Analyse` over 180 000
samples, then `Ft8SyncSearch.Find` over the waterfall, then the per-candidate loop.
The recorded slot costs in this tree, all against FT8's 15 000 ms budget:

| Configuration | ms a slot | Recorded by |
|---|---|---|
| `Ft8Sharp` port, OSD off, fine sync off | about **64** | unit 246 |
| Deep, OSD on, in isolation | about **72** | unit 246 |
| Deep, shipping configuration (OSD order 2 full basis, fine sync on) | **330.4** worst observed | unit 252, a 45× margin |

So the *decode* half of a pass is 64 ms in the isolation task 4b runs in. **The fit is
the other half and it is new**, and §2.5 prices it from arithmetic rather than
guessing: about 35-60 ms a message. A two-pass slot in the isolation is therefore
about `64 + (m x fit) + 64` ms for `m` messages subtracted, which for the two-signal
ladder's `m = 1` is about **190 ms**, a 79× margin. Task 4a measures it; this
paragraph is the prediction it is checked against.

**The multiplier that matters is not the milliseconds.** It is §3: passes multiply
submissions to the CRC-14.

---

## 2. The place, the amplitude and the phase

### 2.1 What is known about a decoded message's position, and how badly

`Ft8SlotMessage` (`src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs:241`) carries the candidate,
and `FrequencyHz(geometry)` and `TimeSeconds(geometry)` are the *candidate's*
position — **the coarse search cell and nothing finer**, even where fine sync moved
the decode somewhere else. That is unit 251's finding and it is why that unit's
estimator refines.

The cell, from `Ft8WaterfallGeometry`:

- `DefaultTimeOversampling = 2` (`:64`) over a `SymbolPeriodSeconds = 0.160f` (`:49`)
  block, so the time grid is **0.080 s**.
- `DefaultFrequencyOversampling = 2` (`:67`) over a `ToneSpacingHz = 1/0.160 = 6.25`
  Hz bin (`:234`), so the frequency grid is **3.125 Hz**.
- `CandidateTimeBiasSeconds` (`src/Ft8Sharp.Deep/Ft8DeepSlotDecoder.cs:182`) is
  exactly minus one symbol period, `-0.160 s`, measured by unit 248 on hard-decision
  distance and not derived.

So `TimeSeconds(geometry) + CandidateTimeBiasSeconds` is the start of the signal to
within **±0.040 s** and `FrequencyHz(geometry)` is the lowest tone to within
**±1.5625 Hz**.

**Neither is anywhere near good enough to subtract with**, and the arithmetic says by
how much:

- **±0.040 s of time error is ±480 samples at 12 kHz**, a quarter of a symbol. The
  GFSK envelope has moved a quarter of a symbol; the correlation with the true
  waveform is down about 2.50 dB (`Ft8DeepSignalToNoise`'s own remarks at
  `src/Ft8Sharp.Deep/Ft8DeepSignalToNoise.cs:205-213` derive `(1 - 0.25)^2`), which
  caps cancellation at a few decibels.
- **±1.5625 Hz of frequency error over a 12.64 s transmission is ±19.7 cycles of
  accumulated phase.** A coherent fit against a reference off by that much removes
  nothing at all; the correlation integrates to approximately zero.

`Ft8DeepSignalToNoise.Estimate` (`:165` for samples, `:225` for a baseband) closes
most of that. Its coordinate search (`:247-250`) is time over ±0.040 s in 0.010 s
steps, then frequency over ±1.6 Hz in 0.40 Hz steps, then time again over ±0.005 s in
0.00125 s steps, and it returns `TimeAdjustmentSeconds` and `FrequencyAdjustmentHz`
saying how far it moved. **What it leaves behind is half a search step on each axis:
±0.000625 s — 7.5 samples — and ±0.20 Hz.**

**±0.20 Hz is still 2.53 cycles over the frame.** So the estimator gets the fit into
the basin and **the fit must refine again on its own axis**. That is the gap the
instruction says is task 1's to price, and it is priced here: **the estimator is
necessary and is not sufficient, and a subtractor that trusts it and stops removes
essentially nothing.**

**And it reports no amplitude and no phase at all.** `Ft8DeepSnrEstimate` (`:471`) is
`(Decibels, Symbols, TimeAdjustmentSeconds, FrequencyAdjustmentHz)`. The decibels are
a *ratio* in a 2500 Hz reference bandwidth — a number about the signal against the
noise, not a scale factor against the reference waveform — and there is no phase
field. Both have to be fitted.

### 2.2 The reference, and it is the port's own

`Ft8Waveform.Synthesize(symbols, sampleRate = 12000, baseFrequency = 1000f)` at
`src/Ft8Sharp/Encode/Ft8Waveform.cs:142` returns `79 x SamplesPerSymbol` floats —
**151 680 samples, 12.64 s**, no padding. The pulse spans three symbol periods
(`PulseSymbolSpan = 3`, `:86`), phase accumulates across every boundary and is never
reset (`:197-201`), and a raised-cosine ramp covers the first and last eighth of a
symbol (`:205-211`).

**It renders at unit amplitude with the carrier starting at zero phase, and there is
no parameter for either** — `:194-195`, `var phase = 0.0f`. **This file is the port
and does not move**; it is called, exactly as the ruling in the work instruction
licenses.

The waveform is Franke, Somerville and Taylor, *The FT4 and FT8 Communication
Protocols*, QEX, July/August 2020 — the paper the port's own `NOTICE` cites for the
synthesis, and the paper that describes the multi-pass subtract-and-decode-again
strategy this unit implements. **No route to any of it goes through WSJT-X's source
or `ft4_ft8_public/`.**

### 2.3 Why a real scale factor alone is not enough — the arithmetic

Write the reference as `x[n] = sin(phi[n])`, which is literally what `Synthesize`
returns: `phi[n]` is the accumulated phase, carrier plus GFSK modulation. The copy
actually in the slot arrived through a path with an arbitrary carrier phase, so it is

```
r[n] = A * sin(phi[n] + theta) + (everything else in the slot)
```

for an unknown amplitude `A` and an unknown phase `theta` uniform on `[0, 2pi)`.
Fitting a single real gain `a` minimises `sum (r[n] - a*x[n])^2`, whose solution is

```
a = sum(r[n] x[n]) / sum(x[n]^2)
```

Expand the numerator with `sin(phi + theta) = cos(theta) sin(phi) + sin(theta) cos(phi)`.
Over the 79-symbol frame `sum(sin^2 phi) = E` and `sum(sin phi cos phi) ~ 0` — the
signal is a continuous-phase FM waveform sweeping across many carrier cycles, so its
in-phase and quadrature versions are orthogonal to within a part in `10^4`. Therefore

```
a  ~  A cos(theta)
```

and the residual keeps the whole quadrature part:

```
r[n] - a x[n]  ~  A sin(theta) cos(phi[n])
```

whose energy is `A^2 sin^2(theta) E`. **The fraction of the transmission's energy a
real-gain-only fit removes is `cos^2(theta)`**, which averaged over a uniform
`theta` is **one half — 3.01 dB, and no more**. At `theta = 90 degrees` it removes
**nothing**, and the reported gain reads zero while the whole transmission stays in
the residual. That last case is exactly the shape of bug task 3's watched failure is
constructed to catch.

### 2.4 What the fit actually minimises, and where it is done

**At the full sample rate, in the time domain, against two real basis vectors.**

Not in `Ft8DeepBaseband`'s complex baseband, and the reason is arithmetic rather than
taste: that baseband **decimates** (`Ft8DeepBaseband.RateHz`,
`src/Ft8Sharp.Deep/Ft8DeepBaseband.cs:95`) and low-passes through a 401-tap filter.
A residual computed there cannot be handed back to `Ft8Monitor.Analyse`, which wants
180 000 samples at 12 kHz, without an interpolation and a re-modulation that would
each leave their own artefact in the very buffer whose artefacts this step is
measuring. **The subtraction has to happen in the same samples the next pass
analyses.**

The two basis vectors are the reference and its quadrature companion:

```
x[n]  = Ft8Waveform.Synthesize(symbols, 12000, f0)[n]     the port's own waveform
y[n]  = Hilbert(x)[n]                                     its 90-degree companion
```

`y` is built by an odd-length FIR Hilbert transformer — `h[k] = 2/(pi k)` for odd `k`,
zero for even `k`, Blackman-windowed, with `x` delayed by the filter's group delay so
the pair stays aligned. **A Type III linear-phase FIR has exactly 90 degrees of phase
by its own antisymmetry**, so the only error is amplitude ripple, and over the 50 Hz
a transmission occupies — far from both DC and Nyquist — that is under one part in a
hundred, which bounds the achievable cancellation at about **-40 dB**. This is a
textbook filter and cites nothing but arithmetic.

The fit minimises the ordinary sum of squares over the samples the transmission
occupies:

```
J(a, b) = sum over n of ( r[n0 + n] - a x[n] - b y[n] )^2
```

which is two normal equations,

```
[ sum x^2    sum x y ] [ a ]   [ sum r x ]
[ sum x y    sum y^2 ] [ b ] = [ sum r y ]
```

solved as a 2x2 rather than assumed diagonal — `sum x y` is small but is computed and
used, because assuming it zero is exactly the kind of "close enough" that shows up
later as a floor on the cancellation nobody can localise. Then

```
gain  = sqrt(a^2 + b^2)          the amplitude the transmission arrived at
phase = atan2(b, a)              the carrier phase it arrived at
```

and the residual is `r[n0 + n] - a x[n] - b y[n]`, written into a **copy** of the
slot. The energy removed is reported as

```
10 log10( sum r^2 / sum (residual)^2 )   over the transmission's support
```

in decibels. **It is reported and it is never a threshold** — a bound picked tonight
would be a target written after the fact, which this phase's rulings forbid.

### 2.5 The last mile: the fit refines the place on its own axis

§2.1 established that `Ft8DeepSignalToNoise.Estimate` leaves ±0.20 Hz, which is 2.53
cycles over the frame, and ±7.5 samples. Both are fatal to a coherent fit and both are
cheap to remove **without re-synthesising anything**, because a frequency shift of the
reference is a rotation of its analytic form:

```
z[n] = x[n] + j y[n]                            the analytic reference, built once
z_d[n] = z[n] e^(j 2 pi d n / fs)               shifted by d hertz
```

so the correlation at every trial shift `d` is one Fourier evaluation of a single
sequence:

```
w[n] = r[n0 + n] * conj(z[n])
C(d) = sum over n of w[n] e^(-j 2 pi d n / fs)
```

and `(a, b) = Re C(d), Im C(d)` up to the normalisation the 2x2 above applies. `w` is
formed once per time offset (151 680 complex multiplies) and then **block-summed in
groups of 480 samples** to 316 points before the sum over `d` — a 25 Hz decimated
rate, which represents `|d| <= 12.5 Hz` without aliasing and costs `sinc(0.5/25) =
0.9993` of amplitude at the ±0.5 Hz actually searched. So sweeping fifty frequency
offsets costs about 16 000 multiply-adds rather than 7.6 million.

The search is therefore: **frequency over ±0.5 Hz in 0.02 Hz steps at the estimator's
time, then integer sample offsets over ±12 samples at the best frequency, taking the
`(offset, d)` that maximises the removed energy `(a^2 + b^2) E`.** An integer sample
of residual time error is 83 microseconds — 0.043 per cent of a symbol, an envelope
phase error of about 0.7 degrees, **-38 dB** — and the sub-sample remainder is a pure
carrier phase shift, which is precisely what `b` absorbs.

Cost, from the arithmetic: one Hilbert filter (about 7.7 million multiply-adds for the
odd taps), about 25 formations of `w` at 151 680 complex multiplies each, and the
sweeps. **Call it 35-60 ms a message**, which is the figure §1.3 predicted a pass
with.

---

## 3. THE SAFETY QUESTION: what can a second pass return that a first pass could not?

### 3.1 The submission arithmetic

Every codeword put to the port's CRC-14 is an independent chance of a false accept at
about **one in 16 384** — `2^-14 = 6.1035e-5` — which is unit 246 §5 item 2's figure
and this project's standing arithmetic. `Ft8CodewordDecoder.Decode`
(`src/Ft8Sharp/Ldpc/Ft8CodewordDecoder.cs:82-99`) runs **gate 1, parity, first**, and
only a codeword that satisfies parity ever reaches **gate 2, the checksum**. So the
number of CRC-14 submissions in a pass is the pass's `ParitySatisfiedCount`, and the
*upper bound* on it is the search's `Ft8SyncSearch.DefaultCandidateLimit = 140`
(`src/Ft8Sharp/Dsp/Ft8SyncSearch.cs:88`).

**In the isolation task 4b runs in — ordered statistics off, fine sync off — it is
exactly one submission per candidate per pass, and subtraction does not change
that.** The subtractor does not enter the per-candidate loop at all; it hands a
different buffer to a pass that is otherwise byte-for-byte the pass already measured.

The worst case, per slot, taking the bound rather than the observed count:

| Passes | CRC-14 submissions a slot (bound) | Expected messages nobody sent, a slot | Over 306 trials |
|---|---|---|---|
| 1 | 140 | **0.00854** | 2.61 |
| 2 | 280 | **0.01709** | 5.23 |
| 3 | 420 | **0.02563** | 7.84 |
| 4 | 560 | **0.03418** | 10.46 |

**Those are bounds and not predictions.** The observed `ParitySatisfiedCount` at the
rungs this project measures at is a small fraction of 140 — belief propagation refuses
most candidates, which is the whole reason ordered statistics has a seat — and every
row this phase has recorded reads **zero wrong**. But the bound is what a stopping
rule has to be argued against, and it is why the pass count is a measured setting and
not a taste.

**The rule that keeps the table above valid: no change in this unit submits more than
one codeword per candidate per pass.** A pass is a whole ordinary decode of a
different buffer.

### 3.2 The hazard that is this step's own

**A subtraction that leaves a shaped remnant where a transmission was.** Every earlier
stage of this phase read a buffer; **this is the first stage in the project's history
that writes one and then asks a decoder to believe it.** Three things follow and only
the third is new:

1. A remnant that is a scaled copy of the message that was subtracted decodes to that
   same message. That is not a wrong decode — it is a duplicate, and §3.4 is the rule.
2. A remnant too mangled to satisfy parity is refused by gate 1 and costs nothing but
   time.
3. **A remnant that satisfies parity and carries a checksum that happens to match is a
   message nobody sent, produced by arithmetic this unit introduced.** That is the
   one thing this stage can invent that no earlier stage could, and it is why every
   row of task 2 and task 4 asserts zero wrong, with the message sent printed beside
   the message returned.

Note the direction of the risk: a *good* fit leaves noise, and noise is what every
earlier measurement already put to these gates. **A bad fit is the dangerous one**,
because a half-cancelled GFSK waveform is structured, correlates with the Costas
arrays, and produces candidates. This is the reason task 3's watched failure is on the
*no-noise* case: on clean audio, anything left in the residual is the fit's own fault
and nothing else's.

### 3.3 The stopping rule, written before it is implemented

> **A pass is the last pass when any one of these is true:**
>
> 1. **The pass count has reached `MaxPasses`.** A hard bound, so the worst-case slot
>    cost is `MaxPasses` times a decode plus the fits, which is a number that can be
>    measured rather than a policy that has to be believed. Same shape as the
>    candidate-list stopping rule at `Ft8DeepSlotDecoder.cs:571-574`.
> 2. **The pass returned no message that had not already been returned.** There is
>    nothing new to subtract, so the next residual would be this residual and the next
>    pass would return this pass's answer.
> 3. **No message in the pass could be subtracted.** Every message either was refused
>    for want of symbols (§3.3's counter) or was already subtracted in an earlier
>    pass. The buffer cannot change, so neither can the answer.
>
> **And a message is not subtracted at all unless
> `Ft8DeepMessageSymbols.TryEncode` gives up its 79 symbols.** That refusal is
> counted in `Ft8DeepSubtractionCounts.RefusedForWantOfSymbols` and never hidden: a
> silent skip is how a stage comes to report a pass it did not make. Unit 251 measured
> **0 refusals in 510** on the ladder's population, so this counter is expected to
> read zero there and to be non-zero on real air.

### 3.4 The duplicate rule, written before it is implemented

`Ft8SlotDecoder`'s `seen` list is **local to one `Decode` call** — `Ft8SlotDecoder.cs`
around `:230-260`, and the sibling's copy at
`src/Ft8Sharp.Deep/Ft8DeepSlotDecoder.cs:348`. **Nothing in the tree stops a second
pass returning a message the first pass already returned**, and a strong transmission
that was imperfectly subtracted will decode again from its own remnant.

> **The multi-pass decoder keeps its own `seen` list across the passes of one call and
> de-duplicates by message text, ordinally.** A message whose text a previous pass
> already returned is counted in
> `Ft8DeepSubtractionCounts.DuplicatesAcrossPasses` and is **not** added to the
> result. Within a pass, the port's own 77-bit-key de-duplication stands untouched.

Text rather than the 77-bit key, and the reason is stated rather than left implicit:
the key is not handed back by `Ft8CodewordDecoder` and the port recovers it by
re-running belief propagation over the ratios that produced the decode
(`Ft8DeepSlotDecoder.cs:546-551`). Reaching it from outside a pass would mean
reproducing the per-candidate loop a third time in this repository. **The text is a
deterministic function of the 77 bits**, so text equality is implied by key equality;
the only way the two rules could differ is two distinct payloads printing the same
string, and in that case the operator sees one line either way and merging them is the
correct display anyway (`CLAUDE.md` §0.0).

---

## 4. The masking prediction, tabulated before anything is measured

**Everything in this section is prediction.** §7 says whether it held.

### 4.1 The geometry

A transmission is 8 tones spaced `1/0.160 = 6.25` Hz, so its lowest and highest tone
centres are `7 x 6.25 = 43.75` Hz apart and it occupies about **50 Hz**. The analysis
bin is **3.125 Hz** (§2.1), so 16 bins span one transmission and `Ft8SoftSymbols`
reads each tone out of its own bin.

Two transmissions whose lowest tones are `S` hertz apart share tone bins whenever
`S` is a multiple of 6.25 below 50 Hz, and share *analysis* bins — and therefore
inflate one another's soft ratios — whenever `|S| < 50` Hz at all:

| Separation S | Tones of 8 that land in the same bin | What the quiet station's extractor sees |
|---|---|---|
| 0 Hz | 8 | the loud station in every one of its eight bins |
| 6.25 Hz | 7 | the loud station in seven of eight |
| 12.5 Hz | 6 | six of eight |
| 25 Hz | 4 | four of eight |
| 43.75 Hz | 1 | one of eight |
| 50 Hz | **0** | only GFSK skirts and bin leakage |

### 4.2 The prediction

`Ft8SoftSymbols.Normalise` turns per-bin powers into ratios by comparison *within* the
candidate. A neighbour that raises some of the eight bins raises the apparent noise
against which the correct tone is judged, so the quiet station's soft ratios shrink
toward zero exactly where the overlap is — and a ratio near zero is an erasure, not an
error. The LDPC code tolerates erasures far better than errors, so:

1. **At S = 50 Hz there should be no measurable cost at any level difference.** No
   tone bin is shared. If there *is* a cost at 50 Hz, something other than tone
   overlap is doing it and the survey has found something.
2. **At S = 0 Hz the two stations are co-channel** and the cost should be the largest
   at every level difference — including at 0 dB, where the two are equally loud and
   both should suffer.
3. **The cost should rise monotonically with level difference at each separation
   below 50 Hz.** A neighbour 20 dB up puts 100 times the power into the shared bins
   as one at 0 dB.
4. **The knee should be around +6 to +13 dB for S <= 12.5 Hz.** `PHASE_PLAN.md` step 4
   names a station at -5 dB sitting on one at -18 — a 13 dB difference — as hiding it
   completely, and that is the figure the grid is built around.
5. **The ceiling should be flat across the whole grid**, because the ceiling column is
   the quiet station alone in the identical noise draw and the loud station's
   parameters do not enter it. **A ceiling that moves across the grid is a defect in
   the fixture**, not a result, and it is the first thing to check in §7.

### 4.3 The cells task 2 walks, and why those

**Separations {0, 6.25, 12.5, 25, 50} Hz x level differences {0, +6, +13, +20} dB —
twenty cells.**

- The separations are chosen to span the whole overlap range: total, seven-eighths,
  three-quarters, half, and **none**. 50 Hz is in the grid specifically as the
  negative control — the cell where the prediction says nothing should happen.
- The level differences are 0 dB (equal), +6 dB (a factor of two in amplitude), **+13
  dB (the plan's own example)** and +20 dB (a factor of ten in amplitude).
- The quiet station is delivered at **-18 dB**, which is above the port's measured 50
  per cent crossing of -19.54 dB (unit 246 §4). **A rung is only useful for this
  survey if the ceiling is high**: at -21 dB the unmasked column is 33 of 306 and
  there is almost nothing for a neighbour to take away, so a flat table would say
  nothing about masking. -18 dB leaves room in both directions.
- The loud station starts at the **same sample offset** as the quiet one. A fully
  time-aligned neighbour is the worst case for masking and the cheapest to reason
  about; a staggered neighbour is a second axis this unit does not have the night for
  and it is logged in `OPEN_ISSUES.md`.
- One whole block — **51 trials, the whole scoreable population** — per cell, per
  column.

---

## 5. This unit's `before`, and its ceiling

### 5.1 The call

`Ft8LadderHarness.RunMasked` — a **new** entry point beside `Run` and `RunRepeats`,
never changing `Run`. Unit 247's comment at `Ft8LadderHarness.cs:312-319` is the
precedent and says why in terms: *a change to it would invalidate all of them.*

For each trial it makes exactly these calls, in this order:

```
(clean, _)   = SearchFixture.OneSignal(12000, quiet, 1000.0, 5760)
signalPower  = SearchFixture.TransmissionPower(12000, quiet, 1000.0)
sigma        = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, 12000)
ceiling      = SearchFixture.AddNoise(clean, noise, sigma, out noisePower)
masked       = (float[])ceiling.Clone()
               SearchFixture.Place(masked, 12000, loud, 1000.0 + S, 5760, amplitude)
```

- **Population**: `Ft8Step6Ladder.Population()`, the 51 scoreable messages every
  306-trial figure in this phase was taken over. The quiet message of trial `i` is
  `population[i mod 51]`, exactly as `Run` has it. **The loud message is
  `population[(i + 25) mod 51]`** — a fixed stride co-prime with 51, so the pairing is
  deterministic, a fresh process draws the same slot, and the two stations never carry
  the same text.
- **Blocks and seeds**: `blockSeed = seed + block + round(rung * 10)`, `seed =
  DefaultSeed = 221001`, which is `Run`'s line at `Ft8LadderHarness.cs:268` verbatim.
- **Frequency**: `DefaultFrequencyHz = 1000.0` for the quiet station,
  `1000.0 + S` for the loud one.
- **Offset**: `DefaultOffsetSamples` = `1920 x 3 = 5760` samples for both.

### 5.2 The ceiling is bit-identical to what `Run` draws, and that is why it is the ceiling

**The ceiling column is not a separate synthesis. It is the array `AddNoise` returned,
before the loud station was placed into a copy of it.** So:

- The ceiling column's audio is `SearchFixture.AddNoise(SearchFixture.OneSignal(...))`
  with the same population order, the same seed arithmetic and the same sigma as
  `Ft8LadderHarness.Run` — **the same three calls with the same arguments**, so it is
  bit-identical audio to the single-signal ladder every recorded row of this phase was
  taken on, and directly comparable with 33 of 306.
- The masked column's audio is that array plus one more transmission summed into a
  copy. **The noise draw is not merely from the same distribution; it is the same
  array.** So the masked and ceiling columns differ in exactly one thing.

**The gap between the single pass on the masked audio and the ceiling is the whole of
what subtraction could ever recover.** A report that quotes a gain without it is
quoting a number with no scale, and this unit does not.

### 5.3 `SearchFixture.Place` gains an amplitude, and the existing figures are protected

`SearchFixture.Place` (`tests/Ft8Sharp.Tests/Dsp/SearchFixture.cs:55`) sums into the
slot and has **no amplitude parameter**. It gains one, **optional, defaulting to
1.0**, so every existing call site — `OneSignal` at `:84`, `ManySignals` at `:110`,
and the passband and slot-decoder tests through them — compiles and behaves
unchanged. `slot[offset + i] += (float)(amplitude * signal[i])` with `amplitude ==
1.0` is `(float)(1.0 * signal[i])`, which is `signal[i]` exactly in IEEE 754.

**That is asserted rather than argued**: task 2's test places one transmission through
the defaulted parameter and compares it sample-for-sample against
`Ft8Waveform.Synthesize` summed by hand. Several recorded figures in this repository
were taken through that call and a silent change to it would move them all.

---

## 6. What would have to change for subtraction to ship — listed, not done

**Subtraction ships OFF by default this unit and `Ft8Reception.cs` is not touched.**
That is the arbiter's second ruling and it is not re-argued here. Turning it on
changes what a capture must record about itself under step 0's must-pass — *which
decoder read the slot and which stages were on* — and that is a surface change across
five places. **This list is what step 6 reads:**

| # | Surface | What must change |
|---|---|---|
| 1 | `src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:460` | The construction `new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default, fineSync: Ft8DeepFineSyncSettings.Default)` gains a subtraction settings argument, and the pass budget has to be reconciled with the 15 000 ms slot budget for the *shipping* configuration, not the isolation |
| 2 | `Ft8DecoderIdentity`, a few lines below `:460` | It carries the decoder's name and **two** stage flags. Subtraction is a third stage. A capture written by a subtracting decoder and read back as a two-flag identity says a pass ran that the reader cannot see |
| 3 | The five-count census | `Ft8SlotResult`'s five counts are per `Decode` call. Under multi-pass they are per *pass*, and a census that silently sums or silently reports the last pass is a census that has stopped meaning what gate-set entry 6 asserts it means |
| 4 | The telemetry line | One line a slot today. It must say how many passes ran and how many messages were subtracted, or the operator's own record cannot tell a two-pass slot from a one-pass slot — `CLAUDE.md` §0.0.1 |
| 5 | The capture sidecar | Same as 2 and 4, on disk. A sidecar written before this change cannot say whether subtraction was on, which is breakage `B13`'s shape exactly |

**Nobody touches any of them tonight.**

---

## 7. The masking survey — measured

*Written by task 2. See §7 of the unit's `output.md` for the same table.*

## 8. The masked ladder — measured

*Written by task 4.*

## 9. The pass-count sweep and the stopping rule as read off the data

*Written by task 4a and task 5.*

## 10. The verdict

*Written by task 5.*
