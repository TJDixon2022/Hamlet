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

# Work instruction 046 — the decoder learns to doubt

**ISSUED: 2026-08-29. A fresh order, not an amendment. Follows unit 045.**

**Eight tasks; task 8 is the drop. This is a long unit by instruction, and it is
the largest structural change this decoder has had.**

## Why this unit exists

**Unit 045 produced the first score: yield 0.763, precision 0.761 over 384
adjudicated characters. The phase goal is 0.99.**

It also produced the finding that explains the gap. **The quantity Hamlet uses as
confidence does not measure correctness.** It correlates **−0.179** with yield and
**−0.203** with precision. It is `(bestScore − nothingAtAll) / hops`
(`CwProbabilisticDecoder.cs:812`), and because both hypotheses carry `−e²/2σ²`
(`:973`) the difference **scales as the square of signal level over the estimated
noise floor, unbounded.** It measures how loud the station is. It gates emission
at 1.40 and the character floor at 1.0.

**And 045 confirmed the architectural cause.** Every stage of the chain commits
irreversibly and keeps nothing to compare against:

| stage | where | commits | revisable |
|---|---|---|---|
| pitch admitted | `CwToneTracker:287` | one bin after two agreeing surveys | no |
| pitch to the mixer | `CwDecoder.Step:753` | one pitch per hop | no |
| speed | `CwProbabilisticDecoder.Decode:798` | **one WPM, keeping only `bestScore`** | no |
| element and character | the Viterbi in `DecodeAt` | **one path** | no |
| emission | `CwProbabilisticStream.Character:730` | **`CwConfidence.High` for every pattern the alphabet knows** | no |

**At the emit seam no confidence judgement is made at all.** The only thing that
marks a character unsure is the alphabet failing to recognise its pattern. **The
string `secondBest` does not occur in the file.**

**That is why the corpus fails by substitution.** 61 substitutions against 30
deletions across the twelve scored captures; on the two worst, 19 wrong letters
against 4 blocks and 13 against 9. **Hamlet is not refusing on those. It is
guessing and missing** — which is the failure §0.0 exists to prevent.

## What the state of the art does instead

The published work converges on one idea. **Do not decide; compute a
probability.** That is Alex Shovkoplyas VE3NEA's own advice, from eight years and
hundreds of algorithms behind CW Skimmer: express prior knowledge as
probabilities, update with observed data, and rather than deciding at each input
sample whether the signal is present, compute the probability that it is.

Mauri AG1LE's rebuild of the fldigi decoder along those lines is built from a
noise estimator, Kalman filters giving the likelihood of the key state given the
observed signal, and **Bayesian inference updating posterior probabilities across
many paths** — paths kept and re-scored, not one committed guess. That decoder
still carried around a 3% base error rate, which is the honest measure of how hard
this is.

His documented failures are Hamlet's, precisely: **without averaging the peak
estimate the decoder tunes to the wrong frequency and emits garbage** (Hamlet at
850 Hz); **the speed estimator sticks at its upper or lower extreme and an
incorrect speed produces a great deal of garbage text** (Hamlet pinning at 40 WPM
and at 400 Hz); and **a fixed filter bandwidth costs accuracy where a
speed-dependent one would not** (Hamlet's 45 Hz constant).

**This unit rebuilds the decoder along those lines, in the order the evidence
supports, with the score to prove each step.**

## The goal, restated — and this needs Tim's formal ruling

**Yield of 99% is a research problem.** The best published amateur decoders do not
reach it on real air.

**Precision of 99% — never putting a wrong character on screen — is reachable, and
it is what the prime directive actually demands.** A screen showing three quarters
of the traffic with the rest blocked is more useful to a novice than one showing
all of it with a quarter wrong, because the second cannot be trusted at all.

**This unit is built to raise precision first and then yield.** Tim directed the
plan; **write the formal restatement into the report's decision section for him to
enter.** Do not mint an id.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Trust the tree over this order everywhere they differ.

From unit 045's report, not measured by this author:

- App **519 passing, 0 failing.** Engine **28 failing of 1984**, excluding the five
  of `TheGateHasItsOwnWindowNowTests` which crash the host (HM-OPEN-061).
- `CwAccuracy` exists, with semi-global alignment, **a block counted as a deletion
  rather than a substitution**, and two numbers never reported apart.
- `tools/Hamlet.PitchRank` has `score` and `width`.
- **Unit 044 shipped no product code.** Its no-clock refusal cost 356 of 599
  adjudicated characters and it stopped at unit 036's bar.
- **The width sweep is non-monotonic** — 35 Hz reads 0.771/0.811, 40 Hz reads
  0.703/0.734, 45 Hz reads 0.763/0.761 — **and 35 Hz was correctly not adopted.**
  The wide end is settled: 55 and 70 Hz are far worse.

**Record both suites' failing counts and the corpus score before task 2.** Every
later task is measured against that baseline.

**The eight captures of 2026-08-29 have blocked tasks in three consecutive units.
State whether they are in the tree now.** If they are, they join the corpus for
every measurement below; if they are not, say so once and proceed with what
exists.

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings:**

> **Do all of it, in this order: give the decoder a real confidence, put the speed
> inside the model, model the durations properly, add a language prior, and stop
> tracking a single pitch.**

> **Do not break the silence behaviour.** In his words: *I'm no longer seeing
> random characters when there's just noise, so that seems to be solved. Don't
> break it.* **That property is not tradeable in this unit at any price**, and task
> 1 locks it before anything moves.

> **Ship the refusal** (2026-08-27): no letters from a pitch the survey admitted no
> keying at.
>
> **Rejected with it:** the clock-withdrawn refusal as unit 1.11.33 built it, and
> raising the gate.

> **The only measurement is against real data from the real radio.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode. **This unit is that
  rule becoming a computable quantity.**
- **HM-DEC-120** — nothing emitted on audio holding no signal, and no letters from
  a pitch nobody judged to be a station. **Tightened only, never loosened.**
- **§0.4** — reproduce, then change, then measure. A fix that cannot be shown to
  fix anything is a guess.
- **HM-DEC-007** — tested against WAV fixtures.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The measurement rule that governs every task below

**Every change in tasks 3 through 8 is measured with `CwAccuracy` over the whole
scored corpus, before and after, and both numbers are reported per task.**

- **Precision must not fall. Ever.** A change that lowers precision is reverted and
  reported, whatever it does to yield.
- **A change that raises precision and lowers yield is kept**, and both numbers are
  stated. That is the trade this unit exists to make.
- **The silence property is asserted after every task.** Captures holding no
  station emit no letters. **If any task breaks that, stop the unit and report.**

## The tasks

### Task 1 — the baseline, and the silence lock

**Run both suites and the corpus score. Record all three.**

Then **lock the silence property before anything moves**: a test asserting that
every capture in the corpus holding no admitted station emits **zero letters**.
Name the captures it covers. **This test may not be modified by any later task in
this unit.** If a later task turns it red, that task is reverted.

Then answer from the code, and **say what you find**:

- **The Viterbi in `DecodeAt`** — its state space, its transition structure, what a
  path score is, and **whether a backward pass is possible over the same lattice
  without restructuring it.** This is the load-bearing question for task 2.
- **Every consumer of the fit figure** — the 1.40 emission gate, the 1.0 character
  floor, the sheet's `better than silence per hop` line, and anything else. **Name
  each with file and line.** Task 3 replaces what they consume.
- **How speed enters** — the grid at `:798`, what is evaluated, and what is
  discarded.
- **How element and gap durations are modelled** — thresholds, distributions, or
  something else.

### Task 2 — a real posterior per character

**The Viterbi finds the best path. A forward–backward pass over the same lattice
gives the probability that each character is what the best path says it is.** That
number is the confidence this decoder has never had.

- Implement forward–backward over the existing lattice. **Do not restructure the
  lattice in this task.**
- **Work in log domain with scaling.** Unit 045 recorded fit values of 5,521,967,
  17.2 million and quadrillions on degenerate bins; the carried asks record
  intermittent overflows. **Underflow and overflow are the known failure of this
  algorithm and the tests must cover both.**
- Output **a posterior in [0,1] per emitted character**, and the **margin between
  the best path and the runner-up** where the lattice affords it.
- **Emit nothing differently yet.** This task adds a number and changes no
  behaviour.

**Acceptance:** the corpus score is **bit-identical** to task 1's baseline, because
nothing that reaches the screen has changed. **If it moves, something was changed
that should not have been.**

**Then measure the thing that matters:** correlate the new posterior against
per-character correctness from `CwAccuracy`, across the whole corpus, **and report
it beside the fit figure's −0.179 and −0.203.** If the posterior does not correlate
positively, **stop and report** — the rest of the unit rests on it.

### Task 3 — the posterior replaces the loudness proxy

**Only if task 2's correlation is positive.**

- **Every consumer named in task 1 takes the posterior instead of the fit figure.**
- **The thresholds are derived, not guessed**: choose them by sweeping the
  posterior against the corpus and picking the point that reaches the precision
  target, then state the number and the sweep that produced it.
- **A character below the threshold is a block, not a deletion and not a guess** —
  unit 036's ruling, and `CwAccuracy` already scores blocks as deletions so the
  trade is visible.
- **The fit figure stays computed and stays on the capture sheet**, labelled for
  what it is, because the corpus's history is expressed in its units.

**Acceptance:** **precision rises.** Report the full table before and after. **The
61 substitutions are the target; report the new count.** Yield may fall and that is
the accepted trade.

### Task 4 — the speed goes inside the model

**Stop estimating WPM separately and feeding it in.** The withdrawn clock, the
40 WPM pin, and the garbage that follows both are artefacts of a separate
estimator that can fail on its own.

- **Dit length becomes a slowly-varying state dimension of the lattice**, so speed
  and text are solved together and the best path carries its own speed.
- **A transition cost penalises fast changes in dit length**, because a sender's
  speed drifts and does not jump. State the cost and its reasoning.
- **The grid search at `:798` goes away**, or becomes the initialisation of the
  state dimension rather than a committed answer.
- **There is then no clock to withdraw.** Report what happens to the sidecar's
  `decoderWpm` line and put the proposed wording in the report — **the sheet's
  wording is Tim's.**

**Acceptance:** measured before and after. **Precision must not fall.** Report what
happens to the captures where the clock previously withdrew.

### Task 5 — durations modelled, not thresholded

Element and gap lengths are **distributions around the dit**, not sides of a
threshold. A hidden semi-Markov model with explicit duration densities is the
standard tool and it is what makes a decoder survive a wobbly fist and fading.

- **Give each state an explicit duration density** parameterised on the dit length
  from task 4 — dit, dah, intra-character gap, inter-character gap, word gap.
- **Fit the densities from the corpus, not from the textbook ratios**, and report
  what the corpus says the real ratios are. A hand-sent fist is not 1:3:1:3:7.
- **Report the fitted parameters in the report**, because they are a finding about
  the operators on the band as much as about the decoder.

**Acceptance:** measured before and after. **Precision must not fall.**

### Task 6 — a language prior

Morse traffic is enormously predictable. **Prosigns, callsign structure, `CQ DE`,
`73`, `RST`, and plain English** carry most of the information the decoder is
currently ignoring. This is where the last several percent live, and it is why
CW Skimmer sanity-checks against a callsign database.

- **A character n-gram prior added to the path cost.** Order and smoothing stated
  and justified.
- **Trained on text that is not the scored corpus.** Training on the answer key
  would make the score meaningless — **say plainly what it was trained on.**
- **The prior is a tiebreaker, not an author.** It may reorder paths the acoustic
  evidence finds nearly equal; **it may not create a character the signal does not
  support.** State how that boundary is enforced and test it: **a capture holding
  no station must still emit nothing**, and task 1's lock proves it.

**Acceptance:** measured before and after. **Precision must not fall.** **Report
substitutions separately** — a language prior that raises yield by inventing
plausible words is the worst possible outcome and the substitution count is where
it would show.

### Task 7 — stop tracking one pitch

The 850 Hz phantom and the mid-stream migration are not bugs to patch; **they are
what a single committed pitch does.**

- **Run a decoder per candidate bin** across the admitted range, and report the
  ones whose posterior clears the task 3 threshold. CW Skimmer runs hundreds in
  parallel; this needs a handful.
- **Average the peak estimate before a decoder is placed on it**, which is the
  documented cause of tuning to the wrong frequency.
- **Two candidates closer than the resolution are one station** — carried ask 10
  names 125 Hz.
- **The screen shows what it showed before** unless Tim rules otherwise; this task
  is about where the decoders sit, not about a new surface.

**Acceptance:** measured before and after. **Precision must not fall.** **Report
what happens on the captures where the tracker previously wandered.**

### Task 8 — the speed-scaled filter width *(the drop candidate)*

**Only after task 4, and only if the corpus can now resolve it.**

Unit 045 could not settle 35 against 45 Hz because the sweep was non-monotonic on
twelve captures. **With speed inside the model there is a per-path dit length to
scale from**, which is a different question from picking one constant.

- **Scale the integrator width from the path's own dit length**, with the
  relationship derived and stated.
- **Sweep the scaling factor, not the width.** Report the curve.
- **If it is still non-monotonic, report that and change nothing.** Fitting noise
  twice is worse than once.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**The whole digital stream** — FT8, the slot cutter, the sync search, the digital
waterfall, the digital capture press.

Also: admission itself, which unit 043 owns; the attenuator and `CwPitch` receive
conditions; the scanner and the calling cycle; `CHANGELOG.md`; the missing
`DECISIONS.md` records; the phrasebook and the recent-places row; the Twin PBT;
the answer key's licensing, raised in 045 and unruled; the pedestal ranking; the
dial-move threshold and the transcript break's wording.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not break the silence property**, and do not modify task 1's lock.
- **Do not let precision fall on any task.** Revert and report instead.
- **Do not train the language prior on the scored corpus.**
- **Do not let the language prior create a character the signal does not support.**
- **Do not pick a threshold or a width by trying values until the corpus reads
  better.** Sweep, report the curve, and say why the chosen point is on it.
- **Do not adopt a value off a non-monotonic sweep.** Unit 045 got this right and
  it is the standard here.
- **Do not change admission.**
- **Do not report a score without saying whether it is yield or precision.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**The section that reports measurements leads with the precision and yield after
every task, as a table with one row per task**, so the whole unit's arc is one
picture — and then with **task 2's correlation between the new posterior and
correctness, beside the fit figure's −0.179 and −0.203.**

**The section that says what the owner should expect leads with this: Hamlet now
knows how sure it is of each character, and blocks the ones it is not sure of —
and it still shows nothing at all on a frequency where nothing is happening.**

**If you finish every task, stop and report. Do not start the next unit.**
