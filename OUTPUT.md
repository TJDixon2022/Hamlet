UNIT:       049 — stopped at task 3 of 8 — 2026-08-29 14:19
PHASE GOAL: 85% correct CW, precision before yield — never a wrong character on screen, and as much of the traffic as that allows.
UNIT GOAL:  Find and remove the overconfidence in the evidence model, so a confidence figure can gate emission.
ADVANCED:   no — precision is unchanged at 0.766 and the distance to 0.85 is still 8.4 points; what the unit produced is three measurements that redirect the work.
NUMBER:     precision 0.766, yield 0.768, substitutions 58 — unchanged. **8.4 points short of 0.85.**
DRIFT:      1 consecutive unit without advance  (was 0)

## 1. What Claude did

**Stopped at task 3 of 8.** Tasks 1 and 2 landed. **Task 3 is built and its sweep
was cut short** — two of thirteen points, then a reduced decade sweep still
running, amended below. **Tasks 4 through 8 were not started**, and task 4 is
explicitly conditional on task 3 finding an α where the posterior discriminates
beyond the noise.

Development computer, prompt claimed `PROJECT: Hamlet`, branch `main`, version
`1.12.6` unchanged. **Nothing here is evidence about the radio.** The eight
captures of 2026-08-29 are still not in the tree — a seventh consecutive unit.

### Task 1 — the baseline, and both of the order's numbers are wrong

**Corpus: yield 0.768, precision 0.766** — confirmed, and **8.4 points short of
0.85.** App 519 passing, 0 failing.

**The over-counting is 2.22, not about 89.**

The order's arithmetic assumes the evidence sum runs over raw samples: *"at 8 kHz
that span holds 400 samples but only about 4.5 independent degrees of freedom …
on the order of a hundred times too peaked."* **It does not run over samples.**
`Envelope` emits one value per hop (`:915`, `step = sampleRate × 5 ms`), and
`LogLikelihoods` produces one key-down and one key-up term per envelope entry
(`:996`). **A 50 ms dit contributes ten terms, not four hundred.**

| | |
|---|---|
| terms summed per second | **200** |
| independent degrees of freedom per second, at 45 Hz | **90** |
| over-counting ratio | **2.22** |
| implied α | **0.45** |

And it is **constant** — 2.22 for a dit and a dah alike at 20, 24 and 30 words a
minute, because both the term count and the degrees of freedom scale with span.

**The second number is much larger, and it is the one that matters.**

Measured across the twelve scored captures, the evidence term against the duration
penalty a span pays for being a fifth off its expected length:

| capture | evidence per element | ratio to the 0.136 penalty |
|---|---|---|
| `cw-2026-08-17-134712` | 4.9 | 36 |
| `cw-2026-08-22-031838` | 70.0 | 516 |
| `cw-2026-08-24-012403` | 77.4 | 570 |
| `cw-2026-08-18-004507` | 203.0 | 1 496 |
| `cw-2026-08-22-032012` | 290.5 | 2 141 |
| `cw-2026-08-22-032050` | 346.4 | 2 553 |
| `cw-2026-08-22-032113` | 467.6 | 3 446 |
| `cw-2026-08-17-013347` | **311 442 642** | **2 295 451 491** |

**The duration prior is doing essentially nothing.** The decoder fits the envelope
and all but ignores how implausible the resulting element lengths are. And
`013347`'s figure is the unbounded loudness term surfacing a seventh time — the
same capture that produced 17.2 million in an earlier unit.

**The two findings are in tension, and that is the point.** A 2.22-fold
over-count cannot produce a two-thousand-fold imbalance. **The over-count is not
what makes the model overconfident; the evidence term's own magnitude is.** So the
α the order's reasoning derives is about forty times too large to do the job, and
the sweep has to run decades below it.

### Task 2 — the speed pins, and the problem was worse than reported

Tim's ruling: *the provenance claim is what broke, so fix the claim.*

**Re-measured against `cwdecoder.py` as it stands, the reference disagrees with all
four pins, not the two unit 048 turned red:**

| capture | pinned as "the reference's answer" | what the reference says now |
|---|---|---|
| `cw-2026-08-18-004507` | 18 | **6.7** |
| `cw-2026-08-18-003016` | 22 | **20.9** |
| `cw-2026-08-18-003126` | 28 | **refuses — no clock fits** |
| `cw-2026-08-18-003758` | 16 | **21.2** |

**Two of them are out by more than ten words a minute**, and `004507` — which was
*green* — is out by eleven. Nothing noticed because three of four were green
against a decoder that has since changed, so the pins and their stated source had
drifted apart silently.

**The assertion is now what the test actually proves**: a speed is found without
being told one, and it is not sitting on the edge of its own search, because an
estimator at its boundary is reporting failure rather than a value. Both the
reference's answer and Hamlet's are printed for comparison and **neither is
asserted as truth** — none of these four captures carries an adjudicated speed.

**That clears unit 048's two red tests without fitting anything to the change and
without touching the decoder.** All four green.

### Task 3 — the temperature, built and partly swept

The exponent multiplies the **whole** path score, so the Viterbi argmax cannot
move: scaling every path by one positive constant leaves the largest largest.
**The decode is untouched and only the normalisation changes**, which is what makes
this safe to sweep. It is threaded from `CwDecoder.PosteriorTemperature` through
the stream so the sweep runs inside the decoder the operator uses, and it **ships
at 1.0** — no temperature at all.

**The sweep, completed across four decades in two runs:**

| α | separation | correlation |
|---|---|---|
| 1.0 | +0.0051 | +0.050 |
| **0.45** | −0.0070 | −0.030 |
| 0.1 | −0.0227 | −0.201 |
| 0.01 | −0.0612 | −0.310 |

**At the α the over-count implies, the separation goes negative, and every decade
below makes it worse.** The full table and what it means are in the amendment in
section 2. **Task 3's gate does not open and task 4 does not start.**

No decision was recorded under §12.1.

## 2. What the owner should expect

**Nothing on screen has changed and precision is where it was: 0.766, with 8.4
points to go.** No character moved, by construction — the temperature cannot alter
what the decoder reads.

What is now true of the tree:

- `CwDecoder.PosteriorTemperature` exists, ships at 1.0, and cannot change a
  character.
- `tools/Hamlet.PitchRank` gained `magnitudes` and `temperature`.
- The four speed pins say what they are, and unit 048's two red tests are green.

**What will look wrong but is not:**

- **The unit stopped at task 3 of 8.** Task 4 is conditional on task 3's gate and
  the sweep has not finished.
- **`Temperature` ships at 1.0 having been built to be changed.** Nothing has
  earned a different value yet.
- **The engine suite has no result in this report**; it is amended below with the
  temperature sweep. The app suite is 519 passing, 0 failing, and the targeted
  batches were green.

### Amendment — the decade sweep, and task 3's gate does not open

**Two more points landed before that run was stopped too, and between them the
four settle the question.**

| α | median right | median wrong | separation | correlation | spread |
|---|---|---|---|---|---|
| 1.0 | 0.8433 | 0.8382 | **+0.0051** | **+0.050** | 0.478 |
| 0.45 | 0.6338 | 0.6408 | −0.0070 | −0.030 | 0.580 |
| 0.1 | 0.3307 | 0.3534 | −0.0227 | **−0.201** | 0.562 |
| 0.01 | 0.0688 | 0.1301 | **−0.0612** | **−0.310** | 0.172 |

**The curve is monotonic and it runs the wrong way.** Every decade of tempering
makes the discrimination worse, and at α = 0.01 the correlation is −0.310 —
as bad as `MarginLlr`'s −0.341, which is one of the five quantities this whole
line of work was meant to replace.

**Task 3's own stopping condition fires**: *if no α makes the posterior
discriminative, stop and report — that would say the evidence term is not merely
too sharp but uninformative, which is a finding about the likelihood model.* **No
α does. The unit stops here and task 4 does not open.**

**And the direction of the failure is the most useful thing in this report.** As
the temperature flattens the distribution, **wrong characters take a systematically
higher posterior than right ones** — 0.1301 against 0.0688 at α = 0.01, nearly two
to one. That is not noise; it is monotonic across four decades.

**It means the model is most certain exactly where it is wrong.** Where Hamlet
reads a character correctly, the lattice usually holds several competing
segmentations of the same audio and the probability is shared among them. Where it
reads one wrongly, the alternatives have been driven out and one path holds nearly
everything. **A confidence built on this model would be worse than no confidence,
because it would be actively anti-correlated with correctness** — which is what the
five earlier quantities were, and this explains why rather than adding a sixth
observation of it.

**The engine suite has no result.** It was started and stopped with the sweeps.
The app suite is 519 passing, 0 failing; the four speed pins and the targeted
batches were green.

## 3. What you should see

**Three measurements, and together they redirect the work.**

**The evidence term is over-counted 2.22-fold, not 89-fold**, because it sums per
five-millisecond hop and not per sample. That correction matters because the whole
plan rested on the larger figure: a temperature derived from 2.22 is α ≈ 0.45, and
**at 0.45 the posterior's discrimination goes slightly negative.**

**And the evidence term outweighs the duration prior by about two thousand to
one.** A span a fifth off its expected length pays 0.136 nats; the evidence for one
element runs 70 to 470. **So the decoder is not weighing "is this a plausible dit"
against "does the envelope look like a dit" — it is answering the second question
and rounding the first to nothing.**

That is the overconfidence, and it is not a frame-independence problem. **It is
that the evidence term is unbounded** — the same `−e²/2σ²` that has now produced
17.2 million, 5,521,967, quadrillions, and 3.6 × 10⁷ per hop on `013347` in this
unit's own table. Tempering divides everything by a constant and cannot fix a term
whose scale varies by six orders of magnitude between captures.

**And the third measurement says the model is most certain where it is wrong.**
Tempered across four decades, wrong characters take a systematically higher
posterior than right ones — 0.1301 against 0.0688 at α = 0.01. Where Hamlet reads
correctly the lattice holds several competing segmentations and the probability is
shared; where it reads wrongly the alternatives have been driven out and one path
holds nearly everything.

**Task 5 is the one this points at**, and it was written to be measured separately:
scale the evidence term alone against the duration penalty. On these numbers that
is not a tuning exercise — the prior is currently switched off in all but name.

## 4. What's blocking us

One ruling, and the unit's remaining tasks hang on it.

> **The evidence term is bounded before any confidence is read off it, and the
> duration prior is given weight the same measurement decides.**
>
> Measured this unit: the evidence per element runs 4.9 to 467 nats across eleven
> captures and **311 million on a twelfth**, against a duration penalty of 0.136
> for a span a fifth off. The ratio is about two thousand to one on the sane
> captures and two billion on the degenerate one. **A quantity whose scale varies
> by six orders of magnitude between recordings cannot be normalised by a constant
> exponent**, which is what task 3 was for and why α = 0.45 made the separation
> worse.
>
> **Rejected: tempering harder.** The sweep runs to 0.0001 and is reported below;
> whatever it shows, dividing an unbounded quantity by a constant leaves it
> unbounded.
> **Rejected: a seventh confidence quantity.** Six have been measured and this
> unit's finding says the fault is upstream of all of them.
> **What this unit could not settle** is whether bounding the evidence per hop —
> capping the per-hop log-likelihood at some multiple of the noise scale — costs
> reading. That is task 5's shape and it changes the decode, so it is measured on
> its own and is not this session's to take unmeasured.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The eight 2026-08-29 captures are not in the tree**, a seventh consecutive
   unit.
2. **The evidence term's unbounded scale** — raised above, and it now subsumes the
   confidence question that units 044 to 048 each carried.
3. **The answer key's licensing**, which bounds how much truth the score can have.
4. **The mode and filter's place in the owned-settings contract** — unit 047.
5. **What the digital rows state for the five they are silent on** — unit 047.
6. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
7. **A dial move's threshold is provisional at 500 Hz.**
8. **The transcript break's wording.**
9. **Whether `CwPitch` should follow an admitted station.**
10. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
11. **The `reading` line's span wording needs approval.**
12. **Two stations closer than 125 Hz are not named.**
13. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
14. **Nothing checks that deleting a surface is not deleting a capability.**
15. **The engine test host crashes**, wider than the class HM-OPEN-061 names, and
    it has now ended four full runs. Owned by Claude, not waiting on a ruling.
