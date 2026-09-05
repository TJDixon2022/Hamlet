# Taking the sum deeper than a pair — what a `combined x4` column actually computed, the budget, and what a third and fourth hearing bought

Unit 254, 2026-09-05, from `HEAD d10f598`. Root `1.12.55`, `Ft8Sharp` `0.10.7`,
`Ft8Sharp.Deep` `0.7.0` at the start of the night.

**This document is the one step 6 reads.** `output.md` is overwritten every unit and
cannot carry it. Unit 247's figures are cited rather than restated — see
`docs/unit247-combining.md`.

---

## 0. The one-line answer

Sections 2, 4 and 5 are filled in by tasks 2, 4 and 5 of this unit. Section 1 is the
trace, written and committed before a line of the accumulator existed, and section 1
§3's prediction is on the record so that section 4 can say whether it held.

---

## 1. The trace, written before the build

### 1.1 What a `combined x4` column computes today, as a fact about the tree

**Two hearings reach one call of `Ft8DeepSoftCombiner.Combine`. Always. At every repeat
count and every history depth.**

The evidence is one call site and it is the only one in `src/`:

| where | what |
|---|---|
| `src/Ft8Sharp.Deep/Ft8DeepRepeatDecoder.cs:228-229` | `Ft8DeepSoftCombiner.Combine(hearing.Ratios, partner.Ratios, settings.Weighting, combined)` |
| `src/Ft8Sharp.Deep/Ft8DeepSoftCombiner.cs:224-229` | that overload's body: `Combine(new[] { first, second }, weighting, combined)` — a list of **exactly two** |
| `src/Ft8Sharp.Deep/Ft8DeepSoftCombiner.cs:109` | the N-hearing entry point, which already sums any number and is **never called from `src/` with more than two** |

A grep of the whole tree for `Ft8DeepSoftCombiner.Combine` returns one production call
site (`Ft8DeepRepeatDecoder.cs:228`) and fourteen in
`tests/Ft8Sharp.Deep.Tests/`, of which the only ones that pass more than two hearings
are the refusal cases at `Ft8DeepSoftCombinerTests.cs:328-345` — a short span, an empty
list, a null, and a one-hearing copy. **Nothing in the tree sums three.** The premise of
work instruction 254 is confirmed against the tree and task 3 is a build, not a
measurement.

The shape of the loop, at `Ft8DeepRepeatDecoder.cs:191-267` (the method runs `:152-276`;
`LastCombine` is assigned at `:269`):

```
for each remembered slot, most recent first          :191
    for each candidate heard in THIS slot            :195
        collect up to MaximumPartners partners       :204-221   (one remembered slot only)
        for each partner                             :226
            Combine(this slot's ratios, that partner's ratios)   :228   <- TWO
            submit the result to the port's gates     :231
```

`combined` at `:186` is one re-used buffer, overwritten by every submission. Nothing
carries a partial sum from one remembered slot to the next, and nothing carries one from
one partner to the next. **A run asked for four repeats therefore computes a chain of
pairs and calls it `combined x4`**, because `RunRepeats` names the row
`$"combined x{repeats}"` at `Ft8LadderHarness.cs:479` from the repeat count and never
from the depth of any sum.

### 1.2 What the submissions are for `repeats = 2, 3, 4` at `HistoryDepth = 1, 2, 3`

`Ft8LadderHarness.RunRepeats` calls `repeat.Reset()` once per trial
(`Ft8LadderHarness.cs:554`) and then decodes the trial's R slots in order, so slot `r`
(zero-based) has `D(r) = min(r, HistoryDepth)` remembered slots behind it. The bound
`Ft8DeepCombineSettings.SubmissionsPerSlot` states (`:184`) is
`candidates × MaximumPartners × HistoryDepth`; per slot it is really
`candidates × MaximumPartners × D(r)`, and per trial it is

```
submissions(R, H) <= C * P * S(R, H),   S(R, H) = sum over r = 0..R-1 of min(r, H)
```

with `C` the candidates a slot returns and `P = MaximumPartners`.

`S(R, H)`, which is the whole of the cost arithmetic:

| repeats R | H = 1 | H = 2 | H = 3 |
|---|---|---|---|
| 2 | 1 | 1 | 1 |
| 3 | 2 | 3 | 3 |
| 4 | 3 | 5 | 6 |

**Hearings in the deepest combination is 2 in every one of those nine cells.** That is
the finding.

Worst-case submissions a trial at the defaults (`P = 1`) and the ladder's observed
`C ≈ 13` candidates a slot at -21 dB:

| repeats | H = 1 | H = 2 | H = 3 |
|---|---|---|---|
| 2 | 13 | 13 | 13 |
| 3 | 26 | 39 | 39 |
| 4 | 39 | 65 | 78 |

At the port's own candidate limit of 140 (`Ft8SyncSearch.DefaultCandidateLimit`,
`src/Ft8Sharp/Dsp/Ft8SyncSearch.cs:88`) every cell above is multiplied by 10.77.

**And the default `HistoryDepth` is 1** (`Ft8DeepCombineSettings.cs:103`), so a caller
who asks for `repeats = 4` and names no settings gets column H = 1: each slot paired with
the immediately preceding one and nothing else. Three pairs a trial, deepest sum two
hearings.

### 1.3 The safety arithmetic, written before the accumulator exists

`Ft8DeepCombineSettings.ExpectedFalseAccepts(submissions) = submissions / 16384.0`
(`:198`), quoted by its own remarks as an **upper** bound because a submission only
reaches the CRC-14 if the port's parity gate converged on it first.

Four accumulation rules were considered. The submission counts are per candidate at a
slot with `D` remembered slots behind it:

| rule | submissions per candidate per slot | deepest sum | verdict |
|---|---|---|---|
| **A — pairwise chain (today)** | `P × D` | 2 | the baseline |
| **B — every contiguous run** (2-way with slot 1, 2-way with slot 2, 3-way with 1+2, …) | `P × D(D+1)/2` | `D+1` | **REFUSED.** At `D = 3` it is 6 against today's 3 — it doubles the budget, and the instruction's own rule forbids more than one codeword per candidate per remembered slot |
| **C — prefix chain, rank-aligned** (chosen) | `≤ P × D` | `D+1` | **CHOSEN** |
| **D — deepest only** | `P` | `D+1` | cheapest, but at `D ≥ 2` it stops submitting anything a pair would have found, so the accumulated column would not contain the pairwise column even in principle |

**Rule C, stated exactly.** For each candidate in the current slot, and for each partner
rank `k = 1 … MaximumPartners`, walk the remembered slots most-recent-first. At
remembered slot `j` (1-based, `j = 1` the most recent) the chain submitted is

```
[ this slot's ratios, rank-k partner from slot 1, rank-k partner from slot 2, ..., rank-k partner from slot j ]
```

and **exactly one combination is submitted per (candidate, rank, j)**. The `j`-th
submission carries `j + 1` hearings where today's `j`-th submission carried 2.

**The budget does not move, and that is the point.** Rule C spends
`P × D` submissions per candidate per slot at most — identical to rule A — and in
practice **strictly fewer**, because the depth-`j` chain is only submitted when a partner
was found in *every* one of slots 1…`j`, whereas rule A submits whenever a partner was
found in slot `j` alone. So:

```
submissions(rule C) <= submissions(rule A), under identical settings, always
```

**The trade, said out loud.** Rule C gives up the pair *(this slot, slot j)* to buy the
chain *(this slot, slot 1, …, slot j)*. At `H = 1` the two rules are identical — the
depth-1 chain is the pair — so **`repeats = 2` and every figure ever recorded at
`HistoryDepth = 1` is bit-for-bit unchanged**, which is the identity task 3 asserts. At
`H ≥ 2` the accumulated column is not a superset of the pairwise one and a trial the
pair carried and the triple does not can be lost. That is a measurement and 4a reports
it as the discordance between the two rows rather than assuming it away.

**What is NOT given up:** every message the single-slot path returned is still in the
result unchanged (`Ft8DeepRepeatDecoder.cs:181`, `:275`), so *combining only ever adds*
survives rule C untouched, and `RunRepeats`'s `LostByCombining` stays zero by
construction.

Multiplied out, with `P = 1` and `C = 13`:

| configuration | trials | worst-case submissions | expected messages nobody sent (upper bound) |
|---|---|---|---|
| `repeats = 2`, H = 1 | 306 | 3 978 | 0.243 |
| `repeats = 3`, H = 2 | 306 | 11 934 | 0.728 |
| `repeats = 4`, H = 3 | 306 | 23 868 | 1.457 |
| `repeats = 3`, H = 2 | 51 | 1 989 | 0.121 |
| `repeats = 4`, H = 1 | 51 | 1 989 | 0.121 |
| `repeats = 4`, H = 3 | 51 | 3 978 | 0.243 |
| `repeats = 4`, H = 3, at the port's limit `C = 140` | 51 | 42 840 | 2.61 |
| `repeats = 4`, H = 3, at the port's limit `C = 140` | 306 | 257 040 | 15.69 |

**The realised figure is about a seventh of the worst case on this ladder.** Unit 247
spent **516** submissions on its 306-trial two-repeat jittered walk against a worst case
of 3 978 — 13.0 per cent — because most candidates find no partner inside 6.25 Hz and
0.32 s. Scaling that: `repeats = 4` at H = 3 over 306 trials is expected to spend about
**3 100** submissions for about **0.19** naive false accepts, and over 51 trials about
**520** for about **0.03**.

**The row at 306 trials and `C = 140` is the one that says why the budget is not left to
a policy.** 15.69 expected wrong decodes is what an unbounded pairing looks like, and it
is why `MaximumPartners` stays at 1 tonight. **Zero wrong is asserted on every row of
every measurement in this unit**, not hoped for.

### 1.4 The processing-gain arithmetic, and the prediction, written before the run

**Why `10 log10 R`.** Hearing `r` of one codeword gives, at bit position `i`, a
log-likelihood ratio `L_r[i] = mu * b[i] + n_r[i]`, where `b[i]` is the transmitted bit
as ±1, `mu` is the mean magnitude the demodulator produces for a bit of that quality, and
`n_r[i]` is zero-mean with variance `sigma^2`. The observations are conditionally
independent given the bit — different slots, different noise draw — so the log-odds given
all of them is the sum, because the log of a product of likelihoods is a sum of logs.
Summing R of them:

```
mean of the sum      = R * mu * b[i]           (adds coherently, as R)
variance of the sum  = R * sigma^2             (adds incoherently, as R)
(mean)^2 / variance  = R^2 mu^2 / (R sigma^2)  = R * mu^2 / sigma^2
```

so the per-bit signal-to-noise ratio the belief propagation sees improves by a factor of
`R`, which is `10 log10 R` decibels:

| hearings R | processing gain | increment over R-1 |
|---|---|---|
| 1 | 0.00 dB | — |
| 2 | 3.01 dB | 3.01 dB |
| 3 | 4.77 dB | 1.76 dB |
| 4 | 6.02 dB | 1.25 dB |

**The third hearing is worth 1.76 dB and the fourth 1.25 dB — each one worth less than
the one before it, and the pair 2→4 worth 3.01 dB, exactly as much again as 1→2.**
`PHASE_PLAN.md`'s *two repeats is 3 dB and four is 6* is this arithmetic and it is a
waypoint, not a gate.

**What the variance says.** Every hearing arrives at
`Ft8SoftSymbols.NormalisedVariance = 24.0` (`src/Ft8Sharp/Dsp/Ft8SoftSymbols.cs:84`),
because `Ft8DeepSoftCombiner.Combine` normalises each input on a copy before adding it
(`Ft8DeepSoftCombiner.cs:167`). So the variance the method returns — the sum's variance
*before* re-normalisation (`:203-205`) — has a known scale:

| what the R hearings are | returned variance, approximately |
|---|---|
| R independent hearings of the same codeword | between `R × 24` and `R² × 24` |
| R hearings of pure independent noise | `R × 24` |
| R copies of one hearing (perfect agreement) | `R² × 24` |
| R hearings of *different* codewords | near `R × 24`, and the mean is destroyed on the bits they disagree about |

**`returned / 24` is therefore a number between R and R², and where it sits in that range
is how much the hearings agreed.** It is reported and never turned into a threshold.

**What defeats it.** Three things, and the third is the one that decides tonight:

1. **A partner that is a different station.** The sum's mean becomes
   `mu*b1[i] + mu*b2[i]`, which cancels on the roughly half of positions where two
   unrelated codewords disagree. Unit 247's `Ft8DeepCombineGateTests` submitted 56
   deliberately wrong pairings: **51 never satisfied parity, 5 decoded and every one of
   those five returned one of its own two transmissions, and 0 returned a message nobody
   sent.** The port's two gates are what makes this safe, and they stay in the path at
   every depth.
2. **Placement jitter.** `HM-OPEN-075`. Unit 247 measured 217 of 306 with both slots on
   the same bin and the same sample against 68 of 306 with the later slot 2.00 Hz and
   480 samples away — **more than half the gain**. This is a synchronisation cost paid
   *before* the sum: a candidate returned at the wrong offset carries a smaller `mu`, so
   the thing being added is already worse. Unit 248's fine sync did not exist when that
   was measured, and 4c is the run that says whether it recovers any of it.
3. **A deeper sum cannot help a candidate that was never offered.** This is the one the
   arithmetic above does not see. The depth-`j` chain requires the sync search to have
   returned a candidate within 6.25 Hz and 0.32 s in **every** one of `j+1` slots. Unit
   247 §2 measured the two-slot version of that at **49 of 51** at -21 dB with no jitter;
   with the jitter on, and needing it three times rather than once, the availability term
   compounds while the processing gain only grows as `10 log10 R`. The ladder's own
   -24 dB result — every column zero, and unit 247 §1 measured that the search does not
   return a place near the signal there — is the same fact at a different rung.

**THE PREDICTION, ON THE RECORD BEFORE THE MEASUREMENT.**

- At -21 dB jittered, 51 trials, accumulated `repeats = 4` at H = 3 will **decode at
  least as many as** pairwise `repeats = 4` at H = 3, and the difference will be
  **between 0 and 10 of 51** — non-zero but small, and much smaller than the
  1.76 + 1.25 dB of processing gain would suggest, because term 3 above compounds while
  the gain does not.
- The deepest combination submitted will carry **4** hearings, and the count of
  submissions will be **lower** than the pairwise column's at the same settings, by rule
  C's prefix-closure.
- `repeats = 3` accumulated over `repeats = 3` pairwise will show a **smaller** absolute
  difference than the `repeats = 4` pair, because it buys 1.76 dB against 3.01 dB.
- **There is a real chance the answer is zero**, and if it is, the trace's stated cause is
  candidate availability rather than the arithmetic — which is falsifiable tonight by
  reading `CombinationsSubmitted` on the accumulated rows: if it is far below the
  pairwise column's, the chains were not being offered.

Unit 253's trace was wrong by 18 dB about its own arithmetic and found it out by running.
That is why this section exists and why it is committed before task 3.

### 1.5 What has moved under unit 247's figures

Unit 247's scoreboard was taken at `Ft8Sharp.Deep` **0.3.0**; the sibling is at **0.7.0**.
Four units landed in between.

| unit | what it added | which of the three columns it can touch | why |
|---|---|---|---|
| **248** | `Ft8DeepBaseband`, `Ft8DeepFineSync`, and the placement arithmetic | **none by default** | `Ft8DeepSlotDecoder`'s constructors (`:76`, `:116`) take `fineSync` and `baseband` as `null` by default, and `RunRepeats` passes neither |
| **251** | the SNR estimator and three new types | **none** | it is a reporting path; nothing it added is consulted by a decode decision |
| **252** | the ordered statistics window, and a grid measured around it | **none** | `Ft8DeepOsdSettings.Default` **did not move**, and `RunRepeats` builds column two as `new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default)` |
| **253** | subtraction and the residual pass | **none by default** | `subtraction` defaults to `null` in both constructors, and `RunRepeats` passes none |

**So the prediction for task 2 is that all three columns reproduce exactly**: 13, 33 and
68 of 306 at -21 dB jittered, with 55 of 306 only-combined and 0 lost. Unit 252
reproduced unit 246 to the decode and that is what said the instrument had not moved; if
tonight's reproduction does not, ruling 2 applies — **write down what it reads beside
what it read, name which unit could account for it, and carry on.** It is a finding, not
a defect to chase.

**What task 2 must reproduce:** the -21 dB jittered configuration exactly — 306 trials,
`repeats = 2`, 2.00 Hz and 480 samples of jitter, `Ft8DeepCombineSettings.Default`,
`Ft8LadderHarness.DefaultSeed`. **What task 2 may not do:** change `Ft8LadderHarness.Run`,
change `RunRepeats`'s existing behaviour, move `Ft8DeepCombineSettings.Default`, move
`Ft8DeepOsdSettings.Default`, tune a tolerance, or assert anything about the rate. It
asserts **zero wrong on every row** and nothing else.

### 1.6 The stacked configuration, and the smallest change that reaches it

**What `RunRepeats` hands its combined column today**, at `Ft8LadderHarness.cs:471-473`:

```csharp
var port   = new Ft8SlotDecoder();
var osd    = new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default);
var repeat = new Ft8DeepRepeatDecoder(combining: rule);
```

The third line builds its own inner decoder at `Ft8DeepRepeatDecoder.cs:83` —
`new Ft8DeepSlotDecoder(rememberHearings: true)` — so the combined column runs with
**ordered statistics off, fine sync off, baseband off and subtraction off**, and there is
**no parameter that lets a caller say otherwise.** That was deliberate in unit 247 (the
remarks at `:435-440` say so) and it is why the difference between columns one and three
is combining alone.

But `src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:460` builds
`new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default, fineSync: Ft8DeepFineSyncSettings.Default)`
per slot, so **the shipping configuration is not the one the combined column has ever been
measured in.** Unit 247 §5 item 1 says it in terms: *the two stack in principle and were
not run stacked. Running both on at once is the obvious next measurement and it is one
call.*

**The smallest change that reaches it, and it alters nothing any existing call computes:**
two optional parameters on `RunRepeats`, defaulting to `null`.

```csharp
Ft8DeepOsdSettings? combinedOsd = null,
Ft8DeepFineSyncSettings? combinedFineSync = null,
```

and the third decoder becomes

```csharp
var repeat = new Ft8DeepRepeatDecoder(
    inner: new Ft8DeepSlotDecoder(
        osd: combinedOsd, fineSync: combinedFineSync, rememberHearings: true),
    combining: rule);
```

With both null that is `new Ft8DeepSlotDecoder(rememberHearings: true)`, which is exactly
what `Ft8DeepRepeatDecoder.cs:83` builds today — **the same object, constructed at the
call site instead of inside the constructor.** Every existing caller of `RunRepeats` is
unchanged and every recorded figure stays comparable.

**The accumulation depth needs no new parameter at all.** It rides on
`Ft8DeepCombineSettings`, which `RunRepeats` already takes (`:460`), so task 3 extends
that type rather than the harness signature.

**`Ft8LadderHarness.Run` is not touched.** `RunRepeats` is unit 247's own entry point,
added rather than changed for exactly this reason, and the same precedent applies to it:
add a defaulted parameter, change no computation.

### 1.7 What would have to change for combining to ship — listed, not done

Combining ships **off** by default this unit. This is the list step 6 reads.

| surface | what it is today | what a cross-slot combiner needs |
|---|---|---|
| `src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:460` | `decoder ??= new Ft8DeepSlotDecoder(...)`, constructed **per slot** inside the read | an `Ft8DeepRepeatDecoder` **held across slots** for the lifetime of the reception, with `Reset()` on band change, frequency change, or a gap in the slot sequence — a slot heard after a five-minute silence must not be paired with the slot before it |
| `Ft8DecoderIdentity`, a few lines below `:460` | two stage flags — fine sync on, ordered statistics on | a third flag for combining, and with it the depth and the partner count, or the capture cannot say what read the slot |
| the five-count census | the port's five counts per slot | the four of `Ft8DeepCombineCounts` beside them, plus the hearings-per-combination count task 3 adds — a census that showed a rate move with no combining activity behind it is not evidence |
| the telemetry line | one line a slot | must distinguish *this slot decoded it* from *this slot plus the previous two decoded it*, or an operator cannot tell a fresh decode from a recovered one |
| the capture sidecar | per-slot messages with the decoder identity | a combined message belongs to more than one slot; the sidecar's per-message rows need which slots the sum drew on, and step 0's must-pass is that a capture says which decoder read the slot and which stages were on |
| **the memory** | none — the per-slot decoder holds nothing between slots | `Ft8DeepRepeatDecoder`'s own remarks (`:26-33`) price the history at at most 140 hearings × 174 floats — **about 97 kB a slot, under a megabyte at the maximum depth of eight**, one slot at the default depth of one |
| the time | about 65 ms a slot | plus one `Ft8SoftSymbols.Normalise` and one `Ft8CodewordDecoder.Decode` per submission; unit 247 measured the whole 306-trial jittered walk at 516 submissions across 612 slots and the worst slot at 101.8 ms, 147× inside FT8's 15 seconds |

**None of this is done tonight.** It is a larger surface change than subtraction was, and
the decision belongs to the closing measurement with the figures in front of it.

---

## 2. The reproduction — the instrument is exactly where unit 247 left it

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit254ReproductionTests.cs`. Two test methods, each run alone
by its exact full method name, foregrounded, 480 s timeout, 1 m 28 s and 1 m 27 s on the
wall clock.

### 2.1 -21 dB, jittered — the later slot 2.00 Hz and 480 samples away, 306 trials

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
single slot      -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     19.7     64.4
single + OSD     -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     22.3     72.8
combined x2      -21.0    -21.000     306       68     238      0    22.2    17.9    27.2     39.7    129.7

  2 slots a trial. Delivered per slot: -21.001 dB, -20.999 dB

  trials NO single slot decoded alone and the combination DID: 55 of 306
  trials SOME single slot decoded alone, no combining needed:  13 of 306
  trials a single slot decoded and the combination did NOT:     0 of 306

  candidate pairs the rule looked at        50677
  combinations submitted to the port          516
  of those, the PORT took past both gates      88
  naive expected messages nobody sent       0.031

  messages the combining stage added           62
  of those, the message that was sent          62
  of those, a message that was NOT sent         0

  WORST SINGLE SLOT: 102.6 ms, 12 candidates, 3 combinations - 146x against FT8's 15 s
```

**The discordant counts, on identical audio** (`Ft8LadderHarness.Discordance`):

| comparison | only the first | only the second |
|---|---|---|
| single slot vs combined x2 | **0** | **55** |
| single + OSD vs combined x2 | 11 | 46 |

### 2.2 -21 dB, same placement — both slots on the same bin and the same sample, 306 trials

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
single slot      -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     19.6     64.0
single + OSD     -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     22.1     72.3
combined x2      -21.0    -21.000     306      217      89      0    70.9    65.6    75.7     39.3    128.4

  trials NO single slot decoded alone and the combination DID: 200 of 306
  trials SOME single slot decoded alone, no combining needed:  17 of 306
  trials a single slot decoded and the combination did NOT:     0 of 306

  candidate pairs the rule looked at        48344
  combinations submitted to the port          357
  of those, the PORT took past both gates     216
  naive expected messages nobody sent       0.022

  messages the combining stage added          211
  of those, the message that was sent         211
  of those, a message that was NOT sent         0

  WORST SINGLE SLOT: 104.9 ms, 21 candidates, 0 combinations - 143x against FT8's 15 s
```

| comparison | only the first | only the second |
|---|---|---|
| single slot vs combined x2 | **0** | **204** |
| single + OSD vs combined x2 | 2 | 186 |

### 2.3 Against what unit 247 recorded at `Ft8Sharp.Deep` 0.3.0

| row | unit 247 | tonight | moved by |
|---|---|---|---|
| single slot, jittered | 13 of 306 | **13 of 306** | 0 |
| single + OSD, jittered | 33 of 306 | **33 of 306** | 0 |
| combined x2, jittered | 68 of 306 | **68 of 306** | 0 |
| only combined, jittered | 55 of 306 | **55 of 306** | 0 |
| lost by combining, jittered | 0 | **0** | 0 |
| pairs offered / submitted / accepted, jittered | 50 677 / 516 / 88 | **50 677 / 516 / 88** | 0 / 0 / 0 |
| combined x2, same placement | 217 of 306 | **217 of 306** | 0 |
| only combined, same placement | 200 of 306 | **200 of 306** | 0 |
| pairs offered / submitted / accepted, same | 48 344 / 357 / 216 | **48 344 / 357 / 216** | 0 / 0 / 0 |

**THE INSTRUMENT DID NOT MOVE.** Every column, every only-combined count and every line of
the submission budget reads exactly what unit 247 read four sibling versions ago at
`Ft8Sharp.Deep` 0.3.0. §1.5's prediction — that units 248, 251, 252 and 253 are all off by
default in this configuration and none of them can touch these three columns — held to the
decode and to the individual submission.

The only numbers that differ are wall-clock ones (102.6 ms against 101.8 ms for the worst
observed slot, and 21 candidates on that slot rather than 11, because the worst *observed*
slot is whichever one the operating system interrupted), and those are properties of the
machine rather than of the decoder.

**Zero wrong on all six rows.** 62 of 62 and 211 of 211 combined decodes were the message
the ladder knows it transmitted.

### 2.4 The one thing in this table that is not a reproduction

**11 trials that single + OSD decoded and the combined column did not**, jittered — and 2 of
them at the same placement. That is not a defect and not a loss: the combined column runs
with **ordered statistics off** (§1.6), so it is a different decoder from column two and
not a superset of it. It is, however, exactly the size of the prize 4c goes after — those
11 trials are decodes the shipping configuration would have had *and* combining would have
had, if anybody had ever run them stacked.

---

## 3. The accumulator, and the deterministic watched failure

`Ft8Sharp.Deep` **0.7.0 → 0.8.0**. Root **1.12.55 → 1.12.56**. `Ft8Sharp` stays at
**0.10.7** and not one line under `src/Ft8Sharp/` moved.

### 3.1 What was built, and the one refinement to §1.3's rule

**No new type.** `Ft8DeepCombineSettings` gained `AccumulationDepth` (a property and a
constructor parameter, defaulting to 1); `Ft8DeepCombineCounts` gained `DeepestHearings`;
`Ft8DeepRepeatDecoder.Combine` was rewritten around one call site. The sibling's
whole-type-list tripwire,
`Ft8DeepSlotDecoderTests.TheSiblingHoldsExactlyTheseTypesAndTheListIsAssertedWhole`, is
untouched at 25 types and was not run — it is not this instruction's test.

**§1.3 chose rule C, the prefix chain. It was implemented as a sliding window, and that is
a refinement worth naming rather than burying.** The window is `AccumulationDepth` slots
wide and it *ends* at each remembered slot `j`:

```
for each candidate in this slot
  for each partner rank k = 1 .. MaximumPartners
    for each remembered slot j = 1 .. HistoryDepth          (most recent first)
      if slot j has no rank-k partner: no submission, exactly as before
      otherwise submit ONE combination:
        [ this slot ] + [ rank-k partners of slots max(1, j-A+1) .. j ]
```

Two consequences, and both are better than the prefix chain §1.3 described:

1. **The submission count is now exactly equal to the pairwise rule's, not merely bounded
   by it** — one combination per (candidate, rank, remembered slot), submitted under
   precisely the condition the pairwise rule submitted under. §1.3 predicted "≤"; the
   window delivers "=". Asserted, offered and submitted, in
   `TheDeeperSumSpendsExactlyTheBudgetThePairwiseRuleSpent`.
2. **The pairwise reach into deep history is not given up.** §1.3's prefix chain would
   have stopped at the first remembered slot with no partner; the window submits for slot
   `j` whenever slot `j` itself has a partner, and simply shrinks the sum over the missing
   ones. Nothing a `HistoryDepth ≥ 2` caller could reach before is unreachable now.

At `AccumulationDepth = 1` the window is one slot wide and every combination is the pair
`(this slot, slot j)` — **which is what this library computed at every depth before
tonight**, and is the identity in §3.4.

The refusal is loud, in `Ft8DeepCombineSettings`' own voice:

```
One sum may draw on 1 to 2 remembered slots, which is the history depth this rule was
built with, and the deepest combination therefore carries 3 hearings at most. A depth of 3
asks for hearings the history does not hold; raise historyDepth if that is what was meant.
Zero would mean combining is on and no partner may enter a sum, which is a state a caller
cannot have meant. (Parameter 'accumulationDepth')
```

**What this is:** the sum of independent hearings of one transmission. The frame is cited
at the point of use — 174 bits in codeword order carrying a 77-bit payload and a CRC-14,
from Franke K9AN, Somerville G4WJS and Taylor K1JT, *The FT4 and FT8 Communication
Protocols*, QEX, July/August 2020 — because position `i` means the same codeword bit in
every hearing only because the protocol says so. The addition itself is textbook and comes
from nobody's source.

**What this is not:** a gate, a threshold or an acceptance rule. Every combination, at
every depth, goes to `Ft8CodewordDecoder.Decode` and faces the port's parity gate and its
CRC-14 exactly as a pair did.

### 3.2 THE WATCHED FAILURE, QUOTED VERBATIM

`Ft8Unit254AccumulationTests.TheDeepestCombinationOfFourHearingsCarriesFourAndNotTwo`, run
alone by exact full method name, foregrounded, 480 s timeout, before the accumulator
existed:

```
  Failed Ft8Sharp.Deep.Tests.Ft8Unit254AccumulationTests.TheDeepestCombinationOfFourHearingsCarriesFourAndNotTwo [319 ms]
  Error Message:
   Assert.Equal() Failure: Values differ
Expected: 4
Actual:   2
  Stack Trace:
     at Ft8Sharp.Deep.Tests.Ft8Unit254AccumulationTests.TheDeepestCombinationOfFourHearingsCarriesFourAndNotTwo() in C:\Source\HamLet\tests\Ft8Sharp.Deep.Tests\Ft8Unit254AccumulationTests.cs:line 100
  Standard Output Messages:
 slot 0: 0 slots remembered behind it, 0 combinations submitted, deepest carried 0 hearings
 slot 1: 1 slots remembered behind it, 6 combinations submitted, deepest carried 2 hearings
 slot 2: 2 slots remembered behind it, 13 combinations submitted, deepest carried 2 hearings
 slot 3: 3 slots remembered behind it, 18 combinations submitted, deepest carried 2 hearings
```

**Four hearings of one transmission, three slots of history, an accumulation depth of
three, and the deepest sum the fourth slot put to the port's gates carried two.** That is
the breakage in one integer: *a decoder that reports a four-slot combination and computes
a chain of pairs.*

The same test after the accumulator, with **the submission counts unchanged**:

```
 slot 0: 0 slots remembered behind it, 0 combinations submitted, deepest carried 0 hearings
 slot 1: 1 slots remembered behind it, 6 combinations submitted, deepest carried 2 hearings
 slot 2: 2 slots remembered behind it, 13 combinations submitted, deepest carried 3 hearings
 slot 3: 3 slots remembered behind it, 18 combinations submitted, deepest carried 4 hearings
```

0, 6, 13, 18 submissions before and 0, 6, 13, 18 after. **Depth was bought and nothing was
spent for it.**

### 3.3 The budget, asserted rather than argued

`TheDeeperSumSpendsExactlyTheBudgetThePairwiseRuleSpent`, five slots of identical audio
through a pairwise decoder and an accumulating one side by side, at one partner and at two:

```
 partners 1, slot 0: pairwise offered    0 submitted  0 deepest 0; accumulated offered    0 submitted  0 deepest 0
 partners 1, slot 1: pairwise offered  483 submitted  6 deepest 2; accumulated offered  483 submitted  6 deepest 2
 partners 1, slot 2: pairwise offered  704 submitted 14 deepest 2; accumulated offered  704 submitted 14 deepest 3
 partners 1, slot 3: pairwise offered 1020 submitted 18 deepest 2; accumulated offered 1020 submitted 18 deepest 4
 partners 1, slot 4: pairwise offered 1080 submitted 18 deepest 2; accumulated offered 1080 submitted 18 deepest 4
 partners 2, slot 0: pairwise offered    0 submitted  0 deepest 0; accumulated offered    0 submitted  0 deepest 0
 partners 2, slot 1: pairwise offered  483 submitted 12 deepest 2; accumulated offered  483 submitted 12 deepest 2
 partners 2, slot 2: pairwise offered  704 submitted 26 deepest 2; accumulated offered  704 submitted 26 deepest 3
 partners 2, slot 3: pairwise offered 1020 submitted 36 deepest 2; accumulated offered 1020 submitted 36 deepest 4
 partners 2, slot 4: pairwise offered 1080 submitted 36 deepest 2; accumulated offered 1080 submitted 36 deepest 4
```

**Pairs offered equal and combinations submitted equal on every row.** The false-accept
budget §1.3 multiplied out is the budget the deeper sum spends, to the individual
submission.

### 3.4 The two identities that protect every recorded figure

| test | what it asserts | result |
|---|---|---|
| `WithCombiningOffTheRepeatDecoderIsStillThePortExactly` | with combining off, `Ft8DeepRepeatDecoder` returns the port's messages, in order, with the port's five counts, and no combine counts at all — at three noise amplitudes | **green** — 1 message and 8 / 20 / 15 candidates, identical through both |
| `AtAccumulationDepthOneEverySubmissionIsStillAPair` | with combining on at depth 1 — the default — every submission carries exactly 2 hearings, at history depths 1, 2 and 3 | **green** — 4 of 5 slots submitted, every submission a pair, at all three histories |
| `AnAccumulationDepthTheHistoryCannotSupplyIsRefused` | a depth above the history, and a depth of zero, are refused loudly rather than clamped, and `Default.AccumulationDepth` is 1 | **green** |

**Combining stays off by default and accumulation stays at depth 1**, for the same reason
ordered statistics and subtraction do.

### 3.5 Step 5's second exit, taken deeper

`AMessageNoSlotAndNoPairCouldReadIsReadOutOfTheSumOfFour`. Four hearings of one known
transmission at eight noise amplitudes; every one of the six possible pairs tried through
its own fresh pairwise decoder before the four-way sum is asked.

```
noise  any slot alone  any pair  SUM OF FOUR  submitted  deepest
  6.0         decoded    missed       missed         13        4
  8.0         decoded    missed       missed          6        4
 10.0         decoded   decoded      DECODED          4        4
 12.0          missed   decoded      DECODED          4        4
 14.0          missed    missed      DECODED          5        4
 16.0          missed    missed       missed          2        2
 18.0          missed    missed       missed          2        2
 20.0          missed    missed       missed          2        2
```

**At noise amplitude 14.0 no single slot read the message, none of the six pairs read it,
and the sum of four did.** 1 of 8 levels, and zero wrong returns at any level.

**And the last three rows are the trace's §1.4 term 3, visible.** At 16.0 and above the
deepest combination falls back to 2 hearings and only 2 submissions are made — not because
the sum got worse but because **the sync search stopped offering a candidate near the
signal in enough slots to build a chain.** A deeper sum cannot help a candidate that was
never offered, and that is what the ladder in §4 has to contend with.

---

## 4. The ladder — what a third and fourth hearing bought

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit254DepthLadderTests.cs`. Three test methods, each run alone
by its exact full method name, foregrounded, 480 s timeout: 1 m 22 s, 3 m 41 s, 3 m 1 s.

**The change to `RunRepeats` is two optional parameters defaulting to null**
(`combinedOsd`, `combinedFineSync`), and the combined column's inner decoder is now
constructed at the call site as `new Ft8DeepSlotDecoder(osd: combinedOsd, fineSync:
combinedFineSync, rememberHearings: true)` — with both null, the identical object
`Ft8DeepRepeatDecoder` builds for itself. `RepeatsRun` gained `DeepestHearings`.
**`Ft8LadderHarness.Run` was not touched.** The third row is now labelled from what it
computes: `combined x{repeats}` at accumulation depth 1, `summed x{depth+1}` above it.

### 4a — the depth sweep, and it is the clean isolation

One block of 51 trials, jittered, at -21 dB. **The same repeat count with accumulation on
and off, and nothing else different.** History depth is `repeats - 1` in every
configuration, so both columns reach back over exactly the same slots.

| configuration | decoded | wrong | only-combined | submitted | accepted | **hearings in deepest** | ms a trial | worst slot |
|---|---|---|---|---|---|---|---|---|
| x3 pairwise (history 2, accumulation 1) | 36 of 51 | **0** | 33 | 203 | 59 | **2** | 198.5 | 80.1 ms, 187× |
| **x3 accumulated** (history 2, accumulation 2) | **39 of 51** | **0** | 36 | **203** | 66 | **3** | 193.5 | 78.9 ms, 190× |
| x4 pairwise (history 3, accumulation 1) | 37 of 51 | **0** | 34 | 359 | 76 | **2** | 259.7 | 80.0 ms, 187× |
| **x4 accumulated** (history 3, accumulation 3) | **41 of 51** | **0** | 38 | **359** | 114 | **4** | 260.0 | 77.9 ms, 193× |

**What accumulation bought, paired on identical audio:**

| comparison | net | only pairwise | only accumulated |
|---|---|---|---|
| x3 pairwise 36 → x3 accumulated 39 | **+3 of 51** | 1 | 4 |
| x4 pairwise 37 → x4 accumulated 41 | **+4 of 51** | 0 | 4 |

**And the submission counts are identical — 203 against 203, 359 against 359.** The
accumulated columns bought 3 and 4 decodes for **nothing**: not one extra codeword was put
to the port's CRC-14. What moved instead is `accepted`, 59 → 66 and 76 → 114: the *same*
submissions, carrying more hearings, passing the port's gates more often. That is what
processing gain looks like from the outside.

**Did the prediction hold?** §1.4 predicted, before the run, that accumulated x4 would beat
pairwise x4 **by between 0 and 10 of 51** — non-zero but much smaller than 1.76 + 1.25 dB
of processing gain suggests, because a chain needs the search to offer a candidate in every
slot. **It read +4 of 51.** The prediction held, at the low end of its own range, and the
sign of the x3 result (+3, smaller than +4) held too: the third hearing is worth 1.76 dB
and the fourth 1.25 dB, and the smaller increment bought the smaller number of decodes.

**One row is worth naming: x3 lost one trial.** *Only pairwise 1* — a trial the pairwise
chain read and the three-way sum did not. That is `AccumulationDepth`'s stated cost
(§3.1): the submission for remembered slot 2 is a three-way sum rather than the pair
`(this slot, slot 2)`, and if the extra hearing is poor the sum can be worse than the pair.
It happened once in 51 at three hearings and **not at all** at four.

### 4b — the scoreboard, 306 trials, jittered, at -21 dB

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
single slot      -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     19.6     63.9
single + OSD     -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     22.1     72.3
combined x2      -21.0    -21.000     306       68     238      0    22.2    17.9    27.2     39.4    128.6
summed x4        -21.0    -21.000     306      252      54      0    82.4    77.7    86.2     79.5    259.6
```

**Zero wrong on all four rows.**

| | combined x2 | summed x4 |
|---|---|---|
| only-combined (no single slot decoded alone and the combination did) | 55 of 306 | **236 of 306** |
| any slot alone (the two-chances baseline) | 13 of 306 | 16 of 306 |
| lost by combining | 0 | 0 |
| candidate pairs offered | 50 677 | 299 908 |
| combinations submitted | 516 | 2 232 |
| the port took past both gates | 88 | 736 |
| naive expected messages nobody sent | 0.031 | 0.136 |
| **hearings in the deepest combination** | **2** | **4** |
| messages the combining stage added | 62 | 470 |
| of those, the message that was sent | **62** | **470** |
| of those, a message that was NOT sent | **0** | **0** |
| worst single slot | 75.5 ms, 199× | 85.4 ms, 176× |

**The discordant counts against `combined x2`, on identical audio:**

| comparison | only combined x2 | only the other |
|---|---|---|
| vs single slot | **55** | **0** |
| vs single + OSD | 46 | 11 |
| vs **summed x4** | **0** | **184** |

**A free check that the two walks are one experiment:** the port column and the ordered
statistics column appear in both, read 13 and 33 in both, and are asserted equal.

**AND THE CAVEAT THAT MUST TRAVEL WITH 252 OF 306.** `RunRepeats` scores the combined
column on the union over the trial's slots, so **a four-repeat column gets four single-slot
attempts as well as deeper sums.** 68 → 252 is *not* the gain from accumulation; it
conflates more hearings with more chances. **4a is the isolation and it says accumulation
is worth +4 of 51 at four hearings.** The honest reading of 4b is: *a station heard four
times, with the combiner accumulating, is read 252 times in 306 against 13 for one hearing
through the port* — which is the number an operator would experience, and 4a is the number
that says how much of it is the sum.

### 4c — the stack, and it is what Hamlet actually ships

**NOT PART OF THE ISOLATION**: two whole stages change at once, deliberately. 306 trials,
jittered, at -21 dB, two repeats, combining at the default depth in both rows.

```
decoder                    trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
single slot                   306       13     293      0     4.2     2.5     7.1     64.2
single + OSD                  306       33     273      0    10.8     7.8    14.8     72.5
combined x2, alone            306       68     238      0    22.2    17.9    27.2    128.9
combined x2, STACKED          306       79     227      0    25.8    21.2    31.0    146.8
```

*"Stacked" is combining with `Ft8DeepOsdSettings.Default` and `Ft8DeepFineSyncSettings.Default`
on the combined column's inner decoder — the configuration `Ft8Reception.cs:460` builds.*

| | combining alone | combining stacked |
|---|---|---|
| decoded | 68 of 306 | **79 of 306** |
| only-combined | 55 of 306 | 46 of 306 |
| any slot alone | 13 of 306 | **33 of 306** |
| lost by combining | 0 | 0 |
| combinations submitted | 516 | **516** |
| the port took past both gates | 88 | 88 |
| combined decodes / verified | 62 / 62 | 62 / 62 |
| worst single slot | 74.7 ms, 201× | **99.6 ms, 151×** |

**The discordant counts, on identical audio:**

| comparison | only the first | only the second |
|---|---|---|
| combining alone 68 vs stacked 79 | **0** | **11** |
| single + OSD 33 vs stacked 79 | **0** | **46** |

**The stack wins outright: it takes 11 trials combining alone did not and loses none.** And
those 11 are exactly the 11 §2.4 flagged — the trials single + OSD had that the combined
column did not, because the combined column ran with ordered statistics off. Unit 247 §5
item 1's *the two stack in principle and were not run stacked* is now measured: **they
stack, and the stacked column is a strict superset of both on this audio.**

**But it is the ordered statistics doing the work, not the fine sync.** The submission
budget is identical — 516 offered, 516 submitted, 88 accepted, 62 combined decodes in both
rows. **The combining stage did precisely the same thing in both runs.** All 11 extra
decodes came from the inner decoder reading a slot on its own that it could not read
before: `any slot alone` rose from 13 to 33, exactly the 20 trials ordered statistics adds
to a single slot. Unit 247 §5 item 2 hoped fine sync would recover the jitter cost
`HM-OPEN-075` prices; **on this evidence it did not recover any of it — the pairing and the
combining behaved identically with fine sync on, and `HM-OPEN-075` stands open.**

The cost is 146.8 ms a trial against 128.9, and a worst observed slot of 99.6 ms — 151×
inside FT8's 15 seconds.

---

## 5. The verdict

*Task 5.*
