# Hamlet CW decode chain — deep code review

**Reviewed against build 1.10.13, 2026-08-23.** Files cited by path and line as
shipped in the archive. Where a claim is checkable, I checked it numerically by
replicating the exact model and Viterbi from `reference_decoder.py` (which the
C# is a line-for-line port of) and running it on synthetic Rayleigh/Rician
envelopes — the correct statistics for a quadrature-mixed envelope of noise and
of a keyed carrier in noise. The replication code and its output are quoted
where used.

---

## The one-paragraph answer

The soup is not one fault; it is three faults that share a signature, and §5's
witness table separates them cleanly. **(1) The per-hop likelihood model is
wrong in form and in scale**, exactly as §4 suspected: under it, **41.8 % of
pure-noise hops score key-down higher than key-up, with a mean free surplus of
2.95 nats per hop**, so the Viterbi mints letters out of any noise it is shown —
and the letters it mints are E, T and I, because those are the cheapest legal
paths through noise. **(2) The gate is a per-window average over 12 seconds
while emission is per character**, so a window that holds any real signal pays
the gate for every noise-minted character in it, and a window whose real signal
has a low duty cycle is diluted below the gate and emits nothing — which is why
the three worst captures emitted *zero* letters while the witness said KEYING
and *everything* while it said nothing. **(3) The production streaming path has
the window-clear on station change ruled off (`ClearOnAStationChange = false`),
which also makes `Restart()` dead code, which also disables the refill guard**
— so the decoder routinely reads 12-second windows containing envelope mixed
down at two different pitches, spliced across the operator's own transmissions,
a configuration the code's own comments measure at 0.06–0.64 of the message
invented. The offline reference "works" because it has none of (2) or (3) and
only shows (1) at its mildest.

---

## Q1. The per-hop likelihood model (§4) — yes, it is wrong, and yes, it is the E/T/I engine

`CwProbabilisticDecoder.LogLikelihoods`, lines 418–444; identical in
`reference_decoder.py::loglik_streams`.

### (a) The wrong density, at the wrong scale

The envelope of narrowband noise is Rayleigh with parameter σ. For a Rayleigh,
the 25th percentile is `σ·√(2·ln(4/3)) ≈ 0.759 σ`. The code sets

```csharp
var noise = Math.Max(Percentile(sorted, 25) * 0.6, 1e-6);   // = 0.455 σ
```

so the scale handed to both Gaussians is **0.455 σ — 2.2× too small**. If the
intent was to recover σ from P25, the factor should be `1/0.759 ≈ 1.318`, not
0.6. Every log-likelihood in the decoder is inflated by `(1/0.455)² ≈ 4.8×` in
its quadratic term, which turns ordinary Rayleigh fluctuation into "decisive"
evidence.

Then the key-up hypothesis is a Gaussian *centred at zero* with that tiny
scale. A typical noise hop sits at the Rayleigh mode σ or mean 1.25 σ — i.e.
2.2–2.75 of the model's own scale units — so **the model scores typical noise
at −2.4 to −3.8 nats under its own noise hypothesis**, and it collapses
catastrophically in the Rayleigh upper tail. The missing `+ln(e)` Rayleigh term
is precisely the term that keeps the noise hypothesis competitive at higher
envelope values.

The consequence is arithmetic, not interpretive. keyDown beats keyUp whenever
`|e − A| < e`, i.e. whenever `e > A/2`. With `A = P97 ≈ 2.65 σ` on noise, that
crossover is `1.32 σ`, and `P(Rayleigh > 1.32 σ) = exp(−1.32²/2) = 41.7 %`.
Measured on 200 000 simulated Rayleigh hops through the code's exact formulas:

```
pure Rayleigh noise: 41.8% of hops score KEY-DOWN higher;
                     mean clamp(kd-ku, 0) = 2.95 nats/hop
corrected model    : 34.2% of hops favor key-down, mean clamp = 0.37 nats/hop
```

The corrected model (Rayleigh key-up with σ from `P25/0.759`, Gaussian(A, σ)
key-down as the high-SNR Rician limit) has a similar crossover *location* —
that part of the geometry is inherent — but **an 8× smaller surplus**, because
the correctly-scaled quadratics stop turning tail wobble into strong evidence.
Under the shipped model, every second noise hop is a small key-down vote and
the votes are large; the segmental Viterbi's only defence is the length prior,
and 2.95 nats/hop of free money buys through it. The cheapest legal purchases
are one-unit marks separated by gaps: **E, T, I. The observed signature is the
model's arbitrage strategy written out in letters.**

### (b) Shared σ — real but second-order

Rician key-down spread does differ from Rayleigh key-up spread (and at high
SNR the Rician σ is *approximately* the noise σ, so sharing is nearly right
there). This is a genuine imprecision but it is not the driver; fix (a) and (c)
first.

### (c) Global amplitude over 12 s — this is the *fragmentation* half of the soup

One `P97` for the whole window means a QSB fade reads as key-up. I generated
`PARIS PARIS` at 18 WPM, 12 dB, with a 6 dB fade over the second half, and ran
the shipped model:

```
hamlet: ratio 10.0  wpm 28   'P A R I S AIE EEE EIE E E E '
                              (sent: PARIS PARIS)
```

The faded half shatters into exactly the E/I/gap soup in the brief's own
outputs (`E I5 SHE II 5EIEIE EEUE TE ISE`). At `e = A/2` — a 6 dB fade — key-down
and key-up tie *identically* under this model, so mid-fade dahs dissolve into
dit-gap-dit chains. `013622` and the "movies" capture both look like this
failure. Note also the speed: the fade *pulled the winning hypothesis to
28 WPM* on an 18 WPM sender, because fragmented paths prefer short units. Some
of the "unit estimator reads 24 on a real 18" mystery is this same coupling
seen from the other instrument.

**What the right density is:** key-up `ln e − 2 ln σ − e²/2σ²` (Rayleigh);
key-down `ln(e/σ²) − (e² + A²)/2σ² + ln I₀(eA/σ²)` (Rician), with the usual
`ln I₀(z) ≈ z − ½ln(2πz)` expansion for z > 3. σ and A must be **time-local** —
a running lower-quartile/upper-tail over ~2–3 s (or Bell's tracked noise power),
not window globals. That single change addresses (a), (b) and (c) together and
is a few dozen lines.

---

## Q2. §5 — fragmentation or letters read out of noise? **Both, and the witness table tells you which is which per capture**

The gate is `ratio = (bestScore − nothingAtAll) / envelope.Count`
(`CwProbabilisticDecoder.cs:331`) — a **per-window average** — while emission is
per character. Two failure modes follow mechanically, and I reproduced both in
one experiment: `PARIS PARIS` at 18 WPM / 12 dB occupying the first ~5.5 s of a
12 s window, the rest noise:

```
hamlet: ratio 12.1  wpm 28
        during keying: 'PARISPARIS'
        after keying : 'EEEEEEEEE'   <- minted from noise
```

Two things at once. First, **ratio 12.1 is below the gate of 15**: a perfectly
decoded real message was *silenced* because averaging over the noise hops
diluted it. That is your `013622`/`134712` row — **zero letters while the
witness says KEYING** is a low-duty or briefly-keying station whose windows
never clear a per-window-average gate (compounded by finding 3 below: the
tracker may also not be on the pitch the witness is scoring). Second, when the
window *does* clear the gate (longer keying, higher SNR), **every noise-minted
character in it rides out with the real ones**, stamped
`CwConfidence.High` with the whole window's LR
(`CwProbabilisticStream.Character`, line 497–510). That is the 43–76 % E-share
outside the witness verdict. I confirmed the streaming version too: a
settle-logic simulation of `CQ CQ DE WAW` at 14 dB emitted the message plus
four spurious E's minted from the trailing noise inside still-gated windows.

**How to tell without an answer key:** you already did — the witness split *is*
the measurement, and its two failure directions are distinguishable. Letters
outside the verdict at high E-share = noise minting (finding Q1a + this gate
granularity). Zero letters inside the verdict = gate dilution and/or wrong
pitch. Two cheap confirmations: (i) log, per emitted character, the *segment*
LLR of just that character's span against all-key-up over the same span — noise-
minted characters will sit near zero while real ones sit far above, no truth
needed; (ii) run the decoder on pure recorded band noise *with the gate forced
open* and confirm the output is E/T/I at the predicted ~42 % hop statistics.

**The fix implied:** gate per character, not per window. Each character's own
span LLR (two subtractions from the same cumulative sums you already have) must
clear a margin, and `■` is the right output for characters that pass the window
gate but fail their own. That preserves HM-DEC-120 — pure-noise windows still
score 2–6 and stay shut — while removing both dilution and free-riding.

---

## Q3. §6 — why the thresholding pipeline beats the no-threshold one

Because the comparison is not "threshold vs no threshold." The W1AW pipeline
and `cwdecoder.py` differ from Hamlet in four ways that all favour them, and
the threshold is the *least* of it:

1. **Their statistics are taken locally in time.** `cwdecoder.py::gate` fits a
   two-means threshold **per 3-second window with 50 % overlap** and refuses any
   region below 6 dB contrast. Hamlet's `noise`/`amplitude` are single numbers
   over 12 s. Under QSB the local threshold rides the fade; Hamlet's global
   amplitude turns the fade into key-up (Q1c, demonstrated above).
2. **Their decision variable is in dB with hysteresis; Hamlet's is a linear-
   domain quadratic with a mis-scaled σ.** An Otsu/two-means cut between the
   classes *is* the likelihood-ratio boundary a correctly-specified two-class
   model would give you, and ±6 dB hysteresis rejects exactly the Rayleigh tail
   that Hamlet's model rewards. The shipped "no-threshold" model is in effect a
   *worse* threshold — a soft cut at A/2, in the amplitude domain, with 2.2×
   overstated confidence, applied independently every 5 ms with no hysteresis.
3. **They have a refusal chain; Hamlet has one global gate.** `cwdecoder.py`
   refuses at acquisition (tone chosen by keying *contrast*, P90−P30, not
   loudness), refuses regions without gate contrast, and refuses to emit at all
   without a clock whose marks cluster as Morse (`fit_clock`,
   `well_separated`). Hamlet's only refusal is the diluted per-window ratio.
4. **They deglitch by merging; Hamlet doesn't.** `deglitch` merges sub-20 ms
   (later sub-0.4-dit) runs into their neighbours before anything is measured.
   Hamlet's estimator *drops* short runs from the lists but still toggles state
   (`CwUnitEstimator.Runs`, lines 444–462), so a one-hop spike **splits** a mark
   or gap into two short halves that then poison the clusters — see Q5.

So the architecture (segmental Viterbi, null hypothesis, late decision) is not
what lost; Bell's architecture with correct densities and local noise tracking
should beat the threshold pipeline, especially at low SNR. What lost is the
implementation of the observation model plus the streaming wrapper (Q7/F1).

---

## Q4. `cwdecoder.py` vs `CwProbabilisticDecoder.cs` — what the working Python does that the C# does not

Beyond the four items in Q3, three specifics worth stealing:

- **TX/mute guard with holdoff and truncation-tainting.** `mute_mask` extends
  every mute by 150 ms for AGC recovery, `runs(..., border_ms=60)` marks marks
  whose evidence touches a mute boundary, `fit_clock(marks[~trunc])` excludes
  them from the clock, and tainted characters render as `▯`. Hamlet's
  counterpart is `CwProbabilisticStream.Skip` — see finding F2: it *splices* the
  envelope across the operator's transmission and decodes across the seam with
  no taint at all.
- **The tone is chosen by keying contrast** (`acquire_tone` scores
  `P90 − P30` of the bin's dB envelope). Hamlet's survey ranks admitted bins by
  `LiftDb` (loudness) after an admission test that requires dah/dit within
  2.5–3.8 — and `cwdecoder.py`'s own docstring records that this exact ratio
  band **refused a real adjudicated station sending 4.24 dits per dah**
  (HM-DEC-144) and had to grow the `well_separated` escape valve.
  `CwToneSurvey` has the band (`MinimumRatio = 2.5`, `MaximumRatio = 3.8`,
  lines 145–148) **without the escape valve**. A heavy fist is therefore never
  admitted, no candidate forms, and `CwToneTracker.ToneHz` falls back to *the
  middle of the fine bank* (line 416) — the decoder then spends the capture
  mixing down a pitch nobody is keying at. This is the second mechanism that
  produces §5's "everything outside the witness verdict," because the witness
  sweeps independently and has no ratio band.
- **A carrier clause.** An unbroken tone ≥ 8 dah emits `<carrier>`, never
  letters. Hamlet's Kinds have no long-mark escape; a carrier is forced to be
  read as chains of dahs (`LongestShare = 2.2` caps a mark at 6.6 units) and
  the alternation constraint then invents gaps to separate them.

Also note the port is *not* faithful to the reference in one place it claims to
be: `reference_decoder.py` searches speeds 10–32 **always**; the production
stream forces the measured unit as the *only* speed when `IsReady` and in range
(`CwProbabilisticStream.Read`, lines 379–383, 431–434) — so a 24-read on an
18 WPM sender is imposed rather than out-voted.

---

## Q5. Is `u = (median dit mark + median element gap)/2` sound?

The bias-cancellation idea is sound — a symmetric-in-dB trigger delays both
edges roughly equally, so mark-long/gap-short cancels in the average. The
24-on-18 error comes from the plumbing around it:

- **The splitting bug.** `Runs` toggles on *every* hysteresis crossing and
  merely omits sub-2-hop runs from the lists. A 1-hop upward spike inside a gap
  therefore splits that gap into two shorter gaps (both kept); a 1-hop dropout
  splits a mark into two short marks. Both halves land in the short clusters
  and drag them down; a shorter unit is a faster speed. `cwdecoder.py` merges
  instead (`deglitch`), which is the correct repair. Fix: merge runs shorter
  than the floor into their neighbours before measuring.
- **The floor is absolute, not unit-relative.** `ShortestRunHops = 2` is 10 ms;
  the Python's post-clock deglitch is 0.4 dit (27 ms at 18 WPM). At 5 ms hops
  a 2-hop noise crossing is common at ±6 dB in the Rayleigh tail.
- **Fragmented-envelope contamination.** In a window where QSB has already
  shattered the envelope (Q1c), the "dit mark" cluster is full of fade
  fragments. The estimator then feeds the decoder a fast unit *as the only
  speed hypothesis*, which reads the intact stretches wrong too. This coupling
  — measured envelope → forced unit → decode — is a milder rebuild of the very
  feedback loop the architecture was designed to kill. If the measured speed is
  used at all, use it as a *prior* (score the grid, add a bonus near the
  measurement), never as the sole hypothesis.

Better estimators, in the order I'd try them: (1) fix the merge bug and make
the floor 0.3–0.4 u — this alone may close the 24-vs-18 gap; (2) envelope
autocorrelation of the *thresholded-in-dB* key function — the first non-zero
peak of the autocorrelation of a Morse keying function sits at 2 u and is
robust to chatter; (3) joint estimation in the trellis is the principled answer
(Bell's multiple speed hypotheses carried in parallel with per-path
likelihoods) but is not needed until (1) and (2) are exhausted. Cepstrum and
cyclostationary methods are overkill for an on-off keyed signal at these SNRs.

## Q6. Word spacing for a 4–8-unit fist

The `[3.5u, 6.5u]` clip binding on 43/45 reads is, as §8 says, a constant in
measurement's clothing — but the reason the cluster boundary keeps landing
outside the clip is upstream: with fade fragments and noise-minted gaps in the
distribution, the 3-means centroids are not measuring the sender. Fix Q1 first
and re-measure before concluding the clip is load-bearing. Structurally, word
gaps are too rare in 12 s to cluster reliably, so: treat the word/character
decision as *soft* — since `|` and ` ` differ only in the length prior, emit a
space when the posterior odds clear a margin and otherwise emit the character
boundary alone. A lexical prior is not the only answer and shouldn't be the
first one; a two-component prior (character gap ~3 u, word gap ~5.5 u with wide
σ for hand keys) plus soft decision will recover most of it. The current hard
crossover at √(3·7) = 4.58 u sits exactly inside the 4–8 u fist's own scatter,
which is why it coin-flips.

---

## Q7. Things that are simply wrong — line-level findings, ranked

**F1 (critical). `ClearOnAStationChange = false` makes `Restart()` dead code
and disables three protections at once.** `CwDecoder.cs:144` is `const false`;
`CwDecoder.cs:485–489` is the only caller of `CwProbabilisticStream.Restart()`.
Consequences: (i) when the tracker follows a different station, the 12 s window
keeps envelope mixed at the old pitch and decodes the mixture — the exact fault
`Restart()`'s own doc-comment measures at "0.06 of the message wrong at eleven
[dB], 0.19 at three, 0.64 at minus four"; (ii) `_refillHops` is only assigned
inside `Restart()` (line 293) and is **never initialised in the constructor**,
so it is 0 forever and the `RefillSeconds` guard (line 349) never fires — the
"nothing is read from a window that has not refilled" property documented at
length in lines 70–87 does not exist in production; (iii) `WindowClears` can
never increment, so any telemetry on it reads as "no clears needed." The
comment says the clear "fired only when the tracker wrongly left a station for
noise" — but turning the clear off did not stop those wrong moves; it made
their windows decode as mixtures. Fix the tracker's wrong moves *and* turn the
clear back on; until the tracker is fixed, the clear is the cheaper of the two
evils by the code's own measurements.

**F2 (critical). `Skip()` splices the envelope across the operator's own
transmissions.** `CwProbabilisticStream.cs:249–259` advances the audio clock
but leaves the envelope buffer intact, so after every transmission the window
contains pre-TX audio butted directly against post-TX audio with no seam
marker. The decoder reads marks and gaps *across the splice* — fabricated
timing by construction — and the AGC recovery transient after TX
(`ResumeAfter` holdoff exists in `CwDecoder` but the envelope from before it
still meets the envelope after it). `cwdecoder.py` solves this properly:
truncation-taint anything touching a mute boundary and exclude it from the
clock and from confident emission. Minimum fix: record splice positions and
forbid any Viterbi segment from spanning one (a `-inf` wall at the splice hop
costs two lines in `DecodeAt`).

**F3 (high). Gate granularity — per-window average, per-character emission.**
`CwProbabilisticDecoder.cs:331–339` plus `CwProbabilisticStream.Character`
(497–510). Demonstrated in Q2: dilution silences real low-duty signals
(ratio 12.1 < 15 on a cleanly-decoded message) and free-riding emits
noise-minted characters at `CwConfidence.High`. Gate each character on its own
span LLR; keep the window gate as the outer HM-DEC-120 guard.

**F4 (high). `CwToneSurvey` admission has the ratio band (2.5–3.8) that
already refused an adjudicated real station, without `well_separated`'s escape
valve.** `CwToneSurvey.cs:145–148` vs `cwdecoder.py::fit_clock`'s own
HM-DEC-144 history. With no candidate admitted, `CwToneTracker.ToneHz`
(line 416) reports the fine bank's centre, and the stream decodes a pitch
nobody is keying. Combined with F1 (no clear when it finally moves), this is
the likeliest complete account of `013622`/`134712`: witness sees keying, the
decoder never does, and everything emitted comes from noise at the wrong pitch.
Port `well_separated` into the survey's admission.

**F5 (medium). Doc/code contradiction: `FastestWpm`.** The remark at
`CwProbabilisticDecoder.cs:121–128` argues at length for **forty** ("the old
ceiling of thirty-two put it outside the grid"); the constant on line 129 is
**32**. Either the fix was never applied or was reverted without the comment.
Machine senders at 35–40 WPM remain unfittable and will be read at 32 with
every segment penalised. (Also: `SlowestWpm = 8` vs the reference's grid
starting at 10 — the "checked against the reference" property is already not
exact.)

**F6 (medium). Forced single speed from the unit estimator.**
`CwProbabilisticStream.cs:379–383` — see Q5. The reference always searches.
A contaminated measurement becomes the only hypothesis. Use it as a prior.

**F7 (medium). `Runs` splits instead of merging short runs.**
`CwUnitEstimator.cs:444–462` — see Q5. Biases the unit fast; likely the
24-on-18.

**F8 (low-medium). Settle watermark is jitter-fragile.**
`CwProbabilisticStream.cs:461–475`: the same physical character re-read half a
second later with its end hop jittered one hop *later* in absolute terms
re-emits as a new character (the monotonic `_settledThrough` only suppresses
non-increasing positions). My streaming simulation did not show duplicates on a
clean signal, so this is second-order, but over noise stretches — where the
path is maximally unstable between reads — it can only add to the soup. Dedup
by span overlap, not by a single monotonic end position.

**F9 (low). Streaming envelope is trailing; offline is centred.**
`PushEnvelope` sums the last `window` samples; `Envelope()` (lines 384–400)
centres the boxcar to match numpy `same`. An ~8 ms systematic offset between
the path the tests validate and the path production runs. Harmless to decode
quality, not harmless to "the port reads what the reference reads."

**F10 (low). Boxcar sidelobes.** The 60 Hz boxcar's first sidelobe is −13 dB;
a strong neighbour 100 Hz away leaks in at only ~−16 dB. With F4 parking the
mixdown off-pitch, adjacent-signal leakage is enough to pay the window gate. A
Hann on the quadrature arms (or the Goertzel windows the rest of the tree
already uses) costs one multiply per sample.

**F11 (observation). The sensitivity sweep cannot see any of F1–F4.** Its
noise is Gaussian white (correct envelope family — good), but the fixture is a
single station at a known pitch, decoded offline-style: no tracker parking, no
splices, no mixed windows, full-duty windows that never test gate dilution.
That is why three units of estimator work "moved this almost not at all" — the
instruments in the repository measure the parts that mostly work.

---

## Answers to §9's standing question

Does the lack of ground truth invalidate the conclusions above? Mostly no,
because the strongest findings here are internal-consistency faults
(mis-scaled σ against a known distribution family, dead code paths, a gate
whose granularity mismatches its emission) demonstrable on synthetic audio with
*known* text, which is ground truth you can mint at will. The one conclusion I
would down-weight is anything tuned on the six-capture LR separation (24–39 vs
3–6): those numbers were produced by the offline reference on whole files, and
this review shows the streaming path's ratios live in a different regime
(dilution). Re-measure the gate's separation with the streaming windower before
trusting 15.

## What I would do, in order

1. Fix `LogLikelihoods`: Rayleigh key-up, Rician (Gaussian-limit) key-down,
   σ = P25/0.759, **rolling** 2–3 s percentiles for σ and A. (Q1)
2. Gate per character on its own span LLR; keep the window gate as the outer
   silence guard; re-tune both against the sweep and HM-DEC-120. (Q2, F3)
3. Turn `ClearOnAStationChange` back on; initialise `_refillHops` in the
   constructor; wall the Viterbi at `Skip()` splices. (F1, F2)
4. Port `well_separated` into `CwToneSurvey`'s admission. (F4)
5. Merge-deglitch in `CwUnitEstimator.Runs`; demote the measured speed from
   mandate to prior. (F7, F6)
6. Re-run the witness split of §5 after 1–4; the E-share inside/outside the
   verdict is the right acceptance metric and needs no answer key.

Each of 1–5 is independently testable against the existing fixtures plus
synthetic Rician envelopes with known text, and none of them trades HM-DEC-120:
1 and 2 *strengthen* it (pure-noise windows score lower under the correct
model — measured 0.14 vs 1.92 ratio in my replication).
