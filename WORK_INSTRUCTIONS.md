STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 049 — the model stops being overconfident

**ISSUED: 2026-08-29. A fresh order, not an amendment. Follows unit 048.**

**Eight tasks; task 8 is the drop.**

## The number this unit is judged by

**Precision is 0.766. The phase goal is 0.85. The gap is 8.4 points.**

**Every task below reports precision, yield, substitutions, and the distance
remaining to 0.85.** A task that does not move that distance says so plainly and
is reported as not having advanced.

**This unit is not a response to a report. It is the work that closes that gap**,
and the reason it can is that unit 048 measured exactly why the decoder cannot
currently tell a good reading from a bad one.

## Why this unit exists

Unit 048 rebuilt the lattice by `(hop, kind)` and precision rose 0.761 to 0.766
with substitutions falling 61 to 58. It then built a real forward–backward
posterior and measured it against per-character correctness:

| quantity | correlation |
|---|---|
| `MarginLlr` | −0.341 |
| `MarginShareForRecord` | −0.286 |
| `SpanMarginForRecord` | −0.246 |
| **`Posterior`** | **+0.050** |

**The first of six that is not negative, and it is inside the noise** — the
standard error on 301 characters is about 0.058. **Median posterior 0.8433 on
right characters against 0.8382 on wrong.** The threshold sweep peaks at 0.822
precision and then goes backwards.

**The session's diagnosis is correct and this unit acts on it: the model is
overconfident, not the search.** The lattice now finds every legal path and the
posterior says nearly all the probability sits on one of them, which cannot be
true of audio a human reads at 76%.

### The cause, named

**The evidence term sums a per-sample log-likelihood over every sample in a span
and treats those samples as independent. They are not.**

The envelope is band-limited by the integrator — a settled 45 Hz. A dit at 24 WPM
is 50 ms. At 8 kHz that span holds 400 samples but only about **2 × 45 × 0.05 ≈
4.5 independent degrees of freedom.** **The accumulated log-likelihood is on the
order of a hundred times too peaked**, and exponentiating it drives all the
probability onto one path whatever the audio says.

**This is the frame conditional independence assumption, and it is a named problem
with a named remedy.** Speech recognition describes the identical symptom —
estimated posteriors tending toward 1.0 whether the utterance is correctly
transcribed or not — and the standard remedy is a scaling exponent on the acoustic
log-likelihoods, which the frame re-weighting literature describes as effectively
applying a power operation to the likelihoods.

### Why this is safe to try

**A temperature applied to the whole path score cannot change the Viterbi
argmax.** Scaling every path by the same positive constant leaves the largest
one largest. **So the decode stays bit-identical, precision cannot fall, and only
the posterior changes.** Task 3 finds out whether a properly tempered posterior
discriminates without risking a character.

**Scaling the evidence term alone is a different question** — it shifts the balance
against the duration penalty and does change the decode. **Task 5 does that
separately**, measured on its own, because tangling the two would make neither
answerable.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches. Trust
the tree over this order everywhere they differ.

From unit 048's report:

- App **519 passing, 0 failing.** Corpus **yield 0.768, precision 0.766,
  substitutions 58, deletions 31** over 384 adjudicated characters.
- Engine **28 failing of 1963**, byte-identical to unit 046's set, **plus two
  turned red by 048** — `TheSpeedIsFoundAndNotTold` on `003016` and `003126`.
- **`TheSilencePropertyIsLockedTests` is green and unmodified.** It locks letters,
  not characters.
- The posterior lives in `CwProbabilisticDecoder.Posterior.cs`, log domain, with
  thirteen numerical tests. **It is computed once for the winning speed**, because
  the backward pass is O(hops × kinds² × span) and the grid runs thirty-three.
- The five kinds are at `:525`; `ShortestShare` 0.45, `LongestShare` 2.2,
  `LengthToleranceShare` 0.35. Transition scoring at `:1462–1472`. Speed grid at
  `:794–805`. `LogLikelihoods` at `:973` — **no bound on the accumulated score.**
- **The threshold sweep is over 301 characters inside an aligned truth span, which
  is not the same set as the 384 the corpus score uses.** Do not compare the two.
- **The eight captures of 2026-08-29 are still not in the tree**, a sixth
  consecutive unit.

**Record both suites and the corpus score before task 2.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings, 2026-08-29:**

> **The phase goal is 85% correct CW, precision before yield.** Never a wrong
> character on screen, and as much of the traffic as that allows. **Precision is
> 0.766 and the target is 0.850.**

> **The two speed pins are re-measured from the reference as it stands and
> relabelled for what they are — the reference's answer, not ground truth.**
>
> Rejected: updating them to what Hamlet now says, which is fitting the test to the
> change. Rejected: reverting the lattice on their account — precision rose across
> the scored corpus and neither capture carries adjudicated truth.
>
> **The provenance claim is what broke. Fix the claim.**

> **Do not break the silence behaviour.** Not tradeable at any price.

> **Ship the refusal** (2026-08-27): no letters from a pitch the survey admitted no
> keying at.
>
> **Rejected with it:** the clock-withdrawn refusal as unit 1.11.33 built it, and
> raising the gate.

> **The only measurement is against real data from the real radio.**

> **FT8, FT4 and every other digital mode are outside this conversation's scope.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode.
- **HM-DEC-120** — nothing emitted on audio holding no signal, and no letters from
  a pitch nobody judged to be a station. **Tightened only.**
- **§0.4** — reproduce, then change, then measure.
- **HM-DEC-007** — tested against WAV fixtures.
- **§5.4** — pure over samples and elapsed time.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The measurement rule that governs every task

**Every change is measured with `CwAccuracy` over the whole scored corpus, before
and after.** Every task reports **precision, yield, substitutions, and points
remaining to 0.85.**

- **Precision must not fall. Ever.** A change that lowers it is reverted and
  reported.
- **A change that raises precision and lowers yield is kept**, and both are stated.
- **`TheSilencePropertyIsLockedTests` runs after every task and may not be
  modified.** A task that turns it red is reverted.

## The tasks

### Task 1 — baseline, and the evidence term measured

**Run both suites and the corpus score. Record all three**, including whether the
two speed pins are still the only additions to 046's failing set.

Then read the evidence term and **report what is actually there**:

- **`LogLikelihoods` at `:973`** — what is summed, over how many samples per span,
  and at what sample rate the decoder actually runs.
- **The integrator's effective bandwidth**, from the code rather than from this
  order's 45 Hz.
- **Compute the implied number of independent degrees of freedom per span**, for a
  dit and a dah at 20, 24 and 30 WPM. **Report the ratio of samples to degrees of
  freedom.** That ratio is the size of the overconfidence and **it is the first
  number this unit needs.**
- **Whether the duration penalty is on the same scale as the evidence term** — it
  is a squared log-ratio, not a sum over samples, so it does not grow with span
  length the way the evidence does. **Report the two magnitudes side by side on a
  real capture.**

### Task 2 — the speed pins' provenance

**Small, and it clears the board before the measurement work.**

- **Re-measure all four `TheSpeedIsFoundAndNotTold` captures from the reference as
  it stands in the tree**, and record what it says now.
- **Relabel the pins for what they are** — the reference implementation's answer,
  not ground truth — with the re-measured values and the date.
- **Where the reference refuses, the pin records a refusal**, not a number.
- **Do not change the decoder to satisfy a pin.**

### Task 3 — a temperature on the path score

**The decode does not change in this task. That is the point.**

- Apply a scaling exponent α to the **whole** path score before the
  forward–backward normalisation — equivalently, divide every log score by a
  constant. **The Viterbi argmax is unaffected by construction; assert it.**
- **Derive a starting α from task 1's ratio** of samples to degrees of freedom, and
  say so, rather than picking one.
- **Sweep α** across at least two decades around that value. For each α report:
  the **spread of the posterior** (its distribution across characters, not just a
  mean), the **medians on right and wrong characters separately**, and the
  **correlation with per-character correctness**.

**Acceptance:**
- **The decode is bit-identical.** The corpus score must not move at all. If it
  does, the temperature is not being applied where this task says.
- **Report the α at which the posterior's medians on right and wrong separate
  furthest**, and the correlation there, **beside 048's +0.050 and the standard
  error of 0.058.**
- **If no α makes the posterior discriminative, stop and report.** That would say
  the evidence term is not merely too sharp but uninformative, which is a finding
  about the likelihood model and is worth more than a seventh quantity.

### Task 4 — the gate opens

**Only if task 3 found an α where the posterior discriminates beyond the noise.**

- **Sweep the threshold at that α and report the curve** — kept, blocked, yield,
  precision, substitutions, at each point.
- **Choose the threshold that reaches the highest precision**, and state the
  number and show the sweep. **Do not pick off a plateau inside the noise** — unit
  048 refused to and that is the standard.
- **Every consumer moves onto it:** the emission gate at `:822`, the character
  floor at `:1513`, the character's confidence at `:734`, the sheet at `:4924`.
- **A character below the threshold is a block**, which `CwAccuracy` scores as a
  deletion so the trade is visible.
- **The fit figure stays computed and stays on the sheet**, labelled for what it
  is.

**Acceptance: precision rises and substitutions fall.** Report the distance
remaining to 0.85. **This is the task the phase goal rests on.**

### Task 5 — the evidence and the duration prior, weighed against each other

**This one does change the decode, and it is measured on its own.**

Task 1 reports the two magnitudes side by side. If the evidence term dominates the
duration penalty by the ratio the overconfidence implies, **the duration prior is
currently doing almost nothing** — the decoder is fitting the envelope and
essentially ignoring how implausible the resulting element lengths are.

- **Scale the evidence term alone** by a factor, leaving the duration penalty
  unscaled, and **sweep it.**
- **Report the curve**: precision, yield and substitutions at each point.
- **Adopt only a value on a monotonic region of the curve.** Unit 045 refused to
  adopt 35 Hz off a non-monotonic sweep and that is the standard.
- **If the curve is flat or non-monotonic, report it and change nothing.**

**Acceptance:** measured before and after. **Precision must not fall.**

### Task 6 — the durations fitted from the corpus

The densities exist and are parameterised off the dit by the textbook ratios, with
one shared `LengthToleranceShare` of 0.35 for all five kinds.

- **Fit each kind's duration density from the corpus** — dit, dah, and the three
  gaps — **including a separate spread per kind.** A word gap's spread is not a
  dit's.
- **Report what the corpus says the real ratios and spreads are.** A hand-sent fist
  is not 1:3:1:3:7, and that is a finding about the operators on the band.
- **Fit on captures excluded from the scored corpus, or state the overlap and what
  it does to the score.** Fitting on the answer key would make the number
  meaningless.
- **If task 5 showed the duration prior is being drowned, this task's effect is
  contingent on task 5's result** — say so in the report either way.

**Acceptance:** measured before and after. **Precision must not fall.**

### Task 7 — the speed inside the lattice

- **Dit length becomes a slowly-varying state dimension**, so speed and text are
  solved together and the best path carries its own speed.
- **A transition cost penalises fast changes in dit length.** State the cost and
  its derivation.
- **The grid at `:794–805` becomes the initialisation of that dimension**, not a
  committed answer.
- **There is then no clock to withdraw.** Report what becomes of the sheet's
  `decoderWpm` line and **put the proposed wording in the report — the sheet's
  wording is Tim's.**
- **The posterior becomes computable across speeds** rather than once for the
  winner, which is what 048 had to settle for.

**Acceptance:** measured before and after. **Precision must not fall.** Report what
happens on the captures where the clock previously withdrew.

### Task 8 — a language prior *(the drop candidate)*

Morse traffic is highly predictable — prosigns, callsign structure, `CQ DE`, `73`,
`RST`, plain English.

- **A character n-gram prior added to the path cost.** Order and smoothing stated.
- **Trained on text that is not the scored corpus. Say what it was trained on.**
- **The prior is a tiebreaker, not an author.** It may reorder paths the acoustic
  evidence finds nearly equal; **it may not create a character the signal does not
  support.** State how the boundary is enforced and test it.
- **The silence lock is the proof.**

**Acceptance:** measured before and after. **Precision must not fall.** **Report
substitutions separately** — a prior that raises yield by inventing plausible words
is the worst outcome and the substitution count is where it shows.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**FT8, FT4 and every other digital mode**, the digital tab, the digital capture
press, the slot cutter, the sync search, the digital waterfall. **Outside this
conversation's scope entirely.**

**The settings contract** — `OwnedSettings`, the coverage table, the mode write's
place in it. Unit 047 raised it and it is Tim's.

Also: admission itself; the tracker; multichannel decoding; the attenuator and
preamp rules; the scanner and the calling cycle; `CHANGELOG.md`; the missing
`DECISIONS.md` records; the phrasebook and the recent-places row; the Twin PBT;
the answer key's licensing; the dial-move threshold; the transcript break's
wording; whether `CwPitch` follows an admitted station.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not break the silence property**, and **do not modify its lock.**
- **Do not let precision fall on any task.** Revert and report.
- **Do not let task 3 change the decode.** Bit-identical or the temperature is in
  the wrong place.
- **Do not continue past task 3 if no α makes the posterior discriminative.**
- **Do not adopt a value off a plateau inside the noise or a non-monotonic sweep.**
- **Do not update a test pin to match the decoder.**
- **Do not train the language prior on the scored corpus.**
- **Do not let the language prior create a character the signal does not support.**
- **Do not report a score without saying whether it is yield or precision.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused push
is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**Write `output.md` before you stop, for any reason at all. Do not hold it behind a
regression run** — unit 047 did and its engine result never landed. **Write the
file, then amend it.**

**The section that reports measurements leads with a table of one row per task —
precision, yield, substitutions, and points remaining to 0.85 — so the whole unit's
arc against the phase goal is one picture.** Then task 1's ratio of samples to
degrees of freedom, and task 3's α sweep.

**The section that says what the owner should expect leads with the precision
number and its distance from 0.85.**

**If you finish every task, stop and report. Do not start the next unit.**
