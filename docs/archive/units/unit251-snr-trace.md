# Unit 251, task 1 — tracing the number before building it

**Written 2026-09-05 by work instruction 251, step 2 of the on-air phase, against
`HEAD 1956718`. No test was run to produce it. One `dotnet build Hamlet.sln`
was run, foregrounded, 420 s timeout, and returned `Build succeeded, 0 Warning(s),
0 Error(s)` in 11.8 s — the baseline this unit starts from.**

The unit's goal is a signal-to-noise ratio per decoded message, measured against
the commanded SNR of the ladder, and shown only if it earns it. Five questions
had to be answered from the tree before the estimator could be written, because
every one of them changes what the estimator's signature can be. They are
answered here in the order the work instruction asks them.

---

## 1. Where the known symbol sequence comes from

**The decode result hands back text and carries no bits.** That is the whole
problem, and it is true at three levels:

| Type | File | What it carries | What it does not |
|---|---|---|---|
| `Ft8DecodeResult` | `src/Ft8Sharp/Message/Ft8MessageDecoder.cs:152` | `Type`, `Status`, `Text`, `Fields` | the 77 message bits |
| `LdpcDecodeResult` | `src/Ft8Sharp/Ldpc/LdpcDecodeResult.cs` | `UnsatisfiedChecks`, `Iterations` | the codeword |
| `Ft8CodewordResult` | `src/Ft8Sharp/Ldpc/Ft8CodewordDecoder.cs:174` | `Status`, `Correction`, `Message` | both of the above |

The corrected bits exist for about four statements inside
`Ft8CodewordDecoder.Decode` — `codewordBits` at line 75 and `message` at line 96
are both `stackalloc` — and then the frame returns and they are gone.

### Route B, and why it is not taken

**Reaching the codeword inside Deep's own loop cannot be made to cover the
messages this unit is about.** `Ft8DeepSlotDecoder.Decode`
(`src/Ft8Sharp.Deep/Ft8DeepSlotDecoder.cs:337`) holds `_osdCodeword` only where
ordered statistics rescued a candidate — line 408, inside
`if (result.Status == Ft8CodewordStatus.ParityNeverSatisfied)`. **Where the port
decoded outright, OSD is never asked and there is no codeword in Deep at all.**
On the ladder at the rungs this unit measures at, that is the overwhelming
majority of messages: OSD's whole recorded contribution at −21 dB is 13 of 306 to
33 of 306.

The only way to have the bits for *every* message would be to run
`LdpcDecoder.Decode` a second time in Deep over the same ratios purely to
recover the codeword. That is a second belief-propagation pass per decoded
candidate for a report-only number, and it would still not cover a message the
port decoded through the waterfall path rather than through Deep's loop.
**Changing `Ft8CodewordDecoder` to hand the bits out is forbidden** — `Ft8Sharp`
is a faithful port and nothing in this phase changes a line of it.

### Route A, and what it costs

**Re-pack the text and re-encode.** `Ft8SymbolEncoder.Encode(message, symbols)`
(`src/Ft8Sharp/Encode/Ft8SymbolEncoder.cs:140`) turns 77 packed bits into the 79
channel symbols, and those symbols are **tone indices in 0..7**, which is exactly
what `TonePowerGrid` is indexed by. The packers are public:

- `Ft8StandardMessage.TryPack(callTo, callDe, extra, message)` — line 45.
- `Ft8FreeText.TryPackText(text, message)` — line 60.
- `Ft8NonstandardMessage.TryPack(...)` — line 102.

`Ft8DecodeResult.Fields` is an `Ft8StandardFields` carrying `CallTo`, `CallDe`
and `Extra` already split, so a standard message re-packs from the fields rather
than by re-parsing its own text. Cost: one `Ft8Payload.Create`, one
`LdpcEncoder.Encode` and one `Lay` per decoded message — **microseconds**, and
nothing compared to §2's baseband.

**This unit takes route A.**

### The failure mode, named

**A hashed callsign is the case where re-packing produces different bits than
were sent.** `Ft8CallsignField.Bracket` (`src/Ft8Sharp/Message/Ft8CallsignField.cs:529`)
renders a call that travelled as a hash as `<W1ABC>`. Two things can then happen:

1. The cache resolved the hash, the text says `<PJ4/K1ABC>`, and re-packing that
   string re-hashes the same call to the same bits. **Harmless.**
2. The cache did not resolve it, or the message is a
   `NonstandardCallsign`/`Telemetry`/contest form whose text is not a
   round-trippable input to any packer. **Re-packing then produces a different
   77 bits, a different 79 symbols, and an SNR measured against a signal that
   was never transmitted** — which is `CLAUDE.md` §0.0's fault exactly: a
   plausible number that is not a measurement.

**The guard, and it is not optional.** After re-packing, the bits are put back
through `Ft8MessageDecoder.Decode` and the text compared **ordinally** against
the text the decoder returned. Any mismatch, and any packer that does not return
`Ft8PackResult.Ok`, means **no symbol sequence and therefore no SNR**. The
measurement is null, not floored and not guessed — `Ft8SlotLevel`'s own remarks
in `src/Hamlet.RadioEngine/Audio/Ft8SlotLevel.cs` are the pattern and say why a
floored substitute is worse than a null.

**The ladder's own population is the right place to measure how often that
bites.** `EncodeCorpus.Entry` carries `CarriesHashedCallsign`
(`tests/Ft8Sharp.Tests/Encode/EncodeCorpus.cs:57`) and the corpus deliberately
contains standard-with-hash and non-standard-with-hashed-companion entries. The
task 3 test counts the refusals rather than filtering them out.

---

## 2. Does a decoded message have a baseband behind it

**No, and this is the cost of the whole unit.**
`Ft8DeepSlotDecoder.Decode` builds a baseband at line 452 inside

```
if (_fineSync is not null && result.Status != Ft8CodewordStatus.Decoded)
```

— that is, **only for candidates the port refused, and only with fine sync on.**
A message the port decoded outright has no baseband behind it, and the dictionary
at line 377 is keyed by `(candidate.BinOffset, candidate.FrequencySubOffset)` and
cleared with the slot.

**So an SNR per message costs one extra `Ft8DeepBaseband.Build` per message**, in
the worst case. In practice one build per *distinct mixing frequency* — two
messages in the same waterfall bin share one, which is the same economy Deep
already applies — but a slot of real air has stations at different frequencies,
so the honest planning figure is **one build a message**.

**What one costs.** `docs/unit248-baseband-resync.md` §3.4 measures **9.2 ms a
candidate for mixing, filtering and searching together**, against the port at
about 64 ms a slot, and says in its own words that *the mixing and the 401-tap
filter are the expensive part*. The search inside that 9.2 ms is a grid of
Costas correlations over sync symbols only; the build alone is therefore the
larger part of it, and **6 to 8 ms is the reading this unit plans against**.
That figure is quoted from unit 248 and was not re-measured tonight.

On top of the build, the estimate itself is `TonePowerGrid` calls: 79 symbols ×
8 tones × 80 baseband samples ≈ **50,560 complex correlations a call**. The
alignment refinement in §3 spends 19 of them.

**The budget.** Hamlet's own example slot decoded 14 messages (unit 249). At
one build and ~20 grid calls a message that is of the order of **150 to 250 ms a
slot** added to a 15,000 ms slot period — the same order as the extra waterfall
analysis unit 249 already accepted, and inside the tenfold margin the phase
works to. **It is not free and it is not close to the limit.**

---

## 3. What `TonePowerGrid` actually returns, and how to get back to power

`Ft8DeepBaseband.TonePowerGrid(startSeconds, frequencyOffsetHz, decibels, syncSymbolsOnly)`
(`src/Ft8Sharp.Deep/Ft8DeepBaseband.cs:285`) fills a `79 × 8` span laid out
`[symbol * 8 + tone]` with

```
decibels = 10 * log10(1e-12 + power)
```

where `power = |sum over one symbol window of x[n] * e^(-j2 pi k n / L)|^2`, an
`L`-point DFT bin with `L = 80` at the default settings. Two things in that
sentence are traps.

### The floor

`1e-12` is folded in before the logarithm — it is `Ft8Monitor`'s own conversion,
kept deliberately so the two extractors are on one scale. **The inverse is
exact:**

```
power = 10^(decibels / 10) - 1e-12
```

and it is applied before anything is averaged, then clamped at zero for the
rounding case where the subtraction lands a hair below it. **Averaging decibels
would be the fiction.** The mean of a set of logarithms is the logarithm of the
geometric mean, which is not the power in the bins; at the rungs this unit works
at the wrong bins hold noise whose per-bin power is exponentially distributed,
and the geometric mean of an exponential sits **2.51 dB** below its arithmetic
mean (Euler–Mascheroni, `10 log10 e^gamma`). A noise floor estimated 2.5 dB low
is an SNR estimate 2.5 dB high, on a gate of 2 dB.

### The NaN

`decibels.Fill(double.NaN)` at line 289, and a symbol whose window falls outside
the baseband keeps it — line 322, `if (start < 0 || start + length > Length)`.

**A NaN symbol is dropped from both sums, not replaced by anything**, and the
count of symbols actually used is carried out of the estimator beside the
number. That is not bookkeeping: an estimate taken over 12 symbols because the
transmission ran off the end of the slot is a different quantity from one taken
over 79, and the caller has to be able to refuse it. **This unit refuses below
40 symbols** — half the frame — and returns no measurement.

Both hazards are the same hazard: **a floor and a NaN are both ways an average
becomes a fiction**, and both are removed before the average rather than after.

---

## 4. The reference-bandwidth constant, derived

**A tone bin of a symbol-length correlation is not 2500 Hz wide.** It is
6.25 Hz wide, and the derivation runs from the tone spacing and the symbol period
and touches neither the sample rate nor the decimation nor the filter.

### The arithmetic, in the style of `SignalToNoise.cs`

Write `T` for the FT8 symbol period, `0.160 s`
(`Ft8WaterfallGeometry.SymbolPeriodSeconds`). The tone spacing is its reciprocal,
`1/T = 6.25 Hz` — that is not a coincidence, it is what makes the eight tones
orthogonal over one symbol, and it is why `TonePowerGrid`'s window is
rectangular and exactly one symbol long.

Let the audio be real, at rate `fs`, holding a signal of power `S` and white
noise of one-sided power spectral density `P` watts per hertz. The baseband is
`x[n] = lowpass( s[n] * e^(-j 2 pi fc t) )` at rate `R = fs / D`, with
`L = R * T` samples in a symbol — `500 Hz` and `80` at the default decimation of
24.

**The signal in the correct bin.** A real tone of power `S` mixes down to a
complex exponential of squared magnitude `S/2` — half, because only the
positive-frequency half survives the low-pass. Correlated coherently over `L`
samples:

```
E |X_correct|^2  =  (S/2) * L^2
```

**The noise in any bin.** A real process of one-sided density `P` mixes down to a
complex process of two-sided density `P/2`, so its total baseband power is
`(P/2) * R`. One DFT bin collects `L` times the per-sample variance:

```
E |X_wrong|^2  =  L * (P/2) * R
```

**The ratio, which is what the estimator forms.** Divide, and note `L / R = T`:

```
  E|X_correct|^2 / E|X_wrong|^2
=  (S/2) L^2 / ( L (P/2) R )
=  S * L / (P * R)
=  S * T / P
=  S / (P * (1/T))
=  S / (noise power in 1/T = 6.25 Hz)
```

**The halves cancel and `L`, `R` and `D` all cancel.** The bin ratio is a
signal-to-noise ratio in a **6.25 Hz** noise bandwidth — the tone spacing — and
it does not depend on how the baseband was built. That is the property worth
having: change the decimation and the number does not move.

**Carrying it to the reference.** `SignalToNoise.ReferenceBandwidthHz` is
`2500.0`, and noise power in `B` hertz is `P * B`, so

```
SNR(2500 Hz) = S / (P * 2500) = binRatio * 6.25 / 2500 = binRatio / 400
```

and in decibels:

```
ReferenceOffsetDecibels = 10 * log10( 2500 / 6.25 )
                        = 10 * log10( 400 )
                        = 26.020599913279625 dB

SNR_dB(2500 Hz) = 10 * log10(binRatio) - 26.0206
```

**The number is 26.02 dB.** It is a derivation, not a fit; nothing was measured
to produce it and nothing on the ladder was consulted.

### The sanity check that is not a calibration

The published FT8 threshold is about −21 dB in 2500 Hz. Put that through the
constant: `-21 + 26.02 = +5.02 dB` per bin, so at threshold the correct tone
holds about **3.2 times** the power of a wrong one, symbol by symbol. That is
the right order for a rate-1/2 code carrying three bits a symbol, and it is
quoted here as a check on the sign and the magnitude of the constant and for
nothing else. **No constant in this unit is fitted to the ladder.**

### The estimator's own arithmetic, and why it sums before it divides

Over the `K` symbols whose windows lie inside the slot, with `c[s]` the power in
the transmitted tone and `w[s]` the mean of the seven others:

```
binRatio = ( sum c[s] - sum w[s] ) / sum w[s]
```

**Sum first, divide once.** The per-symbol ratio `c[s]/w[s]` is a ratio of
exponential variates; its expectation does not exist in the usual sense and its
sample mean is dominated by whichever symbol happened to draw the smallest
denominator. Summing the powers first is the non-coherent energy estimator and
is what the published description of FT8's own SNR estimate describes — Franke,
Somerville and Taylor, *The FT4 and FT8 Communication Protocols*, QEX,
July/August 2020. **No route to this arithmetic goes through WSJT-X source or
`ft4_ft8_public/`.**

`sum w[s]` estimates the noise from `7K` bins and `sum c[s]` estimates signal
plus noise; subtracting removes the noise the correct bin also holds, which is
the whole reason the seven wrong bins are read at the *same instant* rather than
a noise floor being taken from elsewhere in the slot. Where the subtraction
lands at or below zero — noise-only, or an estimate at a place with nothing in
it — **there is no measurement and the estimator says so** rather than clamping
to a floor.

### The placement sensitivity, which is why the estimator refines

The correlation is a matched filter, so it is only matched at the right place.

- **Frequency.** An error `df` costs `sinc^2(df * T)`. The waterfall's frequency
  step is 3.125 Hz, so a coarse candidate can be **1.56 Hz** out, which is
  `sinc^2(0.25) = 0.81`, or **−0.91 dB**, with the missing energy landing in the
  neighbouring bins where it inflates the noise estimate as well.
- **Time.** An error `dt` within a symbol costs roughly `(1 - |dt|/T)^2` and puts
  the rest into the *previous or next symbol's* tone, which is one of the seven
  wrong bins. The waterfall's time step is 0.080 s, so a coarse candidate can be
  **0.040 s** out, which is a quarter of a symbol: `0.5625`, or **−2.50 dB**.

**Together that is up to 3.4 dB, low, against a 2 dB gate**, and it depends
entirely on where the station happened to land in the analysis cell. `G1` in
`docs/breakage-record.md` is the record of this project quoting figures taken
where the grid had nothing to lose, so it is measured rather than assumed: the
task 3 test runs both placements, **on grid and at the cell centre**
(`+1.56 Hz, +480 samples`, unit 248's own definition).

The estimator therefore refines its place before it measures, over a coordinate
search using the symbol sequence it was given — nine time steps of 0.010 s
across ±0.040 s, then nine frequency steps of 0.39 Hz across ±1.56 Hz — 19
`TonePowerGrid` calls including the final one. **That is alignment, not
calibration**, and the task 3 test quotes the unrefined figure beside the
refined one so the difference is visible rather than absorbed.

### The one bias that is added and must be named

`Ft8DeepSlotDecoder.CandidateTimeBiasSeconds` is **exactly minus one symbol
period** (line 182), measured by unit 248 rather than derived, and it is the
distance from a candidate's nominal time to the start of the signal it found.
`Ft8SlotMessage.TimeSeconds(geometry)` returns the *nominal* time, which is what
`Ft8Reader` puts in `Ft8Decode.OffsetSeconds` and what the `dt` column shows.
**A caller that hands the estimator `TimeSeconds` unbiased is measuring a window
one symbol early**, which is the same fault as a wrong constant and would look
exactly like an estimator that does not work. The bias is applied at the call
site and is stated in the estimator's own remarks.

---

## 5. The three surfaces, and what exists today

| Surface | Write site | Per-message row today | What has to be made |
|---|---|---|---|
| The panel | `DigitalDecodeRow.From(Ft8Decode)`, `src/Hamlet.App/ViewModels/DigitalDecodeRow.cs:99` | **Yes** — a five-field record, one per decode | Nothing structural. `Snr` is written `NoMeasurement` unconditionally at line 105; it becomes the formatted figure or keeps the dash. The column is `ColumnDefinitions="76,48,48,54,*"` in `MainWindow.axaml` — **48 px is already reserved** |
| Telemetry | `AppEvents.Ft8SlotsRead`, `src/Hamlet.App/Telemetry/AppEvents.cs:921` | **No** — it writes `ft8_slot` **per slot**, from `IReadOnlyList<Ft8SlotCensus>`, and never sees an `Ft8Decode` | Either a per-message `ft8_decode` event, or slot-level aggregates. **Smallest honest shape: three fields on the existing per-slot line** — how many messages carried a measurement, and the weakest and strongest of them. A per-slot event that invented a single SNR for a slot would be describing several stations with one number |
| The sidecar | `DigitalCaptureSheet.Compose`, `src/Hamlet.RadioEngine/Audio/DigitalCaptureSheet.cs:105` | **No, and it is worse than that** — the signature takes `IReadOnlyList<Ft8SlotCensus>? census` and **no decodes at all**. Searched: there is no per-message line in the file | A new `IReadOnlyList<Ft8Decode>?` parameter and a block of `utc / snr / dt / hz / message` lines, plus every call site. **This is the drop candidate the work instruction names in advance** |

`Ft8Decode` itself is a five-field record at
`src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:23` and **`new Ft8Decode(` appears
in three files**: that one, and `tests/Hamlet.App.Tests/ViewModels/`. The
measurement is added as a **nullable init-only member beside the five** rather
than as a sixth positional parameter, on `Ft8SlotCensus.Level`'s own precedent
(*added rather than substituted*), so the two test construction sites keep
compiling.

**One meaning across three surfaces.** Step 0's criterion is that a number's
meaning never changes silently, and it applies to this number from the moment it
exists: the same decibels, in the same 2500 Hz reference bandwidth, from the same
estimator, with **null meaning not measured** on all three and never zero and
never a floor.

---

## 6. The measurement — added by task 3, after the estimator existed

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit251SnrAgreementTests.TheEstimateAgreesWithTheCommandedRatioOverTwoHundredSynthesizedMessages`,
run twice, foregrounded, filtered by exact name, 480 s timeout, **2 m 25 s red
and 2 m 24 s green**. No suite was run.

**Decoder: `Ft8DeepSlotDecoder` with ordered statistics and fine sync both on**
— what `Ft8Reader.Read` builds when nobody passes one, so the figure is the one
Hamlet will actually produce.

```
placement     rung   trials  decoded  measured   MAE ref   p95 ref   bias ref   MAE raw   p95 raw
on grid        -18       51       51        51      0.33      0.80       0.05      0.34      0.80
on grid        -15       51       51        51      0.19      0.49      -0.06      0.20      0.49
on grid        -12       51       51        51      0.22      0.51      -0.10      0.23      0.51
on grid         -9       51       51        51      0.21      0.58      -0.17      0.22      0.56
on grid         -6       51       51        51      0.35      0.65      -0.35      0.35      0.66
cell centre    -18       51       51        51      0.31      0.72       0.06      3.78      4.52
cell centre    -15       51       51        51      0.21      0.59      -0.07      4.80      5.68
cell centre    -12       51       51        51      0.16      0.38      -0.04      6.14      6.94
cell centre     -9       51       51        51      0.23      0.60      -0.19      8.34      9.21
cell centre     -6       51       51        51      0.33      0.62      -0.32     10.57     11.58

BOTH           all      510      510       510      0.26      0.62      -0.12      3.50     10.52

trials 510, decoded 510, measured 510, re-pack refused 0, no measurement 0
reference offset: 26.0206 dB, derived from a 6.2500 Hz bin against 2500 Hz. Nothing was fitted.
```

**The headline: mean absolute error 0.26 dB, 95th percentile 0.62 dB, over 510
synthesized messages** across five rungs and two placements. Against
`PHASE_PLAN.md`'s 2 dB, read by the arbiter's ruling against the mean absolute
error: **the column shows a number.**

### Four things the table says that a single figure would not

**1. There is no selection effect at these rungs, because there was nothing to
select from.** 510 trials, 510 decoded, 510 measured. Every message the ladder
sent came back, so the sample is the whole population and not the lucky half of
it. That is why the rungs stop at −18 dB: at −21 dB the rate is about 11 per
cent and an agreement figure taken there would be measured on the trials whose
noise happened to be kind. **Nothing in this unit is claimed at −21 dB.**

**2. The refinement is the whole difference and it is not a small one.** On grid
the two columns agree to a hundredth of a decibel — the coarse candidate is
already at the signal, and there is nothing to refine. At the cell centre the
unrefined estimate is **3.78 dB out at −18 and 10.57 dB out at −6**, and the
column is a straight line in the wrong direction.

**3. And the reason it gets *worse* as the signal gets stronger is worth
naming.** A quarter-symbol time error and half-bin frequency error take energy
out of the correct bin and put it into the neighbouring ones — which are among
the seven "wrong" bins the noise is estimated from. At a weak rung that leakage
is buried in real noise; at a strong one the noise estimate is **the signal's own
leakage**, so the ratio saturates and the estimate stops tracking. An
unrefined estimator would have read a 20 dB station as a 10 dB one and been most
wrong about the stations the operator cares most about.

**4. Route A held for all 510, including the hashed callsigns.** `re-pack refused
0`. And the test asserts, on every one of the 510, that the symbols packed back
out of the decoded text are **byte for byte the symbols that were transmitted**.
The round-trip guard was never needed on this population; it stays because the
population is 51 messages and the band is not.

### What was watched failing, and what it said

The first run asserted the **unrefined** figure — which is what a reader who had
not done §4's placement arithmetic would write — and said:

```
the mean absolute error against the delivered ratio is 3.50 dB over 510
messages, against a bound of 2.00 dB.
```

**That red is evidence and not ceremony.** It is the measurement that justifies
the refinement existing at all, and the same run printed the refined column
beside it at 0.26 dB. The assertion was then moved to the refined figure and the
bound tightened from the plan's 2.00 dB to **1.00 dB**, which is four times the
measured error and tight enough to catch a 2.5 dB floor-inversion or
candidate-bias regression that 2.00 dB would let through.

### Nothing was calibrated

**No constant in `Ft8DeepSignalToNoise` was adjusted by anything on this table.**
`ReferenceOffsetDecibels` is `10 log10(2500 / 6.25)` and was written down in §4
before the estimator compiled. The unfitted figure and the shipped figure are
the same figure, because there is no fitted one.

---

## What this trace changes about task 2

Four things, none of which were knowable without reading:

1. **Route A, with a round-trip guard**, because route B cannot cover a message
   the port decoded outright.
2. **One `Ft8DeepBaseband.Build` a message**, ~6 to 8 ms, because a decoded
   message has no baseband behind it.
3. **Powers un-floored and NaNs dropped before any average**, and a minimum
   symbol count, because a floor and a NaN are both ways an average becomes a
   fiction.
4. **26.0206 dB**, derived from the tone spacing and the reference bandwidth,
   and an alignment refinement, because the coarse grid can put a station 3.4 dB
   below where it is.
