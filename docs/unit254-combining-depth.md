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

## 2. The reproduction — is the instrument where unit 247 left it

*Task 2.*

---

## 3. The accumulator, and the deterministic watched failure

*Task 3.*

---

## 4. The ladder — what a third and fourth hearing bought

*Task 4.*

---

## 5. The verdict

*Task 5.*
