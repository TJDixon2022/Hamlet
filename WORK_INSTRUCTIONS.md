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

# Work instruction 045 — the score, and the first structural change

**ISSUED: 2026-08-29. A fresh order, not an amendment. Follows unit 044.**

**Eight tasks; task 8 is the drop. This is a long unit by instruction.**

## Why this unit exists

**The goal is reading one mode at ninety-nine percent. This project has never
measured a percentage.**

Every unit so far has counted characters emitted, characters unsure, and blocks.
None of those is accuracy. **Nothing in the tree compares what Hamlet read against
what was actually sent**, because until last night there was nothing to compare
against.

That changed. The captures of 03:08–03:10 on 2026-08-29 hold a **W1AW propagation
bulletin** — `AUGUST 27, 2026, BY F. K. JANDA, OK1HH` — and the ARRL publishes
those bulletins verbatim. **For the first time this corpus has an external answer
key.**

**Tasks 1 to 4 build the score. Tasks 5 to 7 make the first structural change the
evidence supports, with the score to prove it. Task 8 writes down what a real
redesign would be, without building it.**

## What the analysis found, and it is not a symptom list

Three captures, fifty seconds apart, same station, same audio. Measured outside
Hamlet: the carrier sits at **399.9 Hz at +53 dB** in all three; **850 Hz is 4.4 dB
below the band floor** — nothing is there.

| capture | tracked pitch | `decoderWpm` | fit vs silence | result |
|---|---|---|---|---|
| `-030850` | **850 Hz — nothing there** | withdrawn | 36.0 | clean text |
| `-030940` | 400 Hz — correct | withdrawn | **8224.4** | **141 characters, 1 unsure, all `E I S`** |
| `-031024` | 400 Hz — correct | **24** | 11.1 | clean text |

**The garbage came from the capture where the pitch was right.** The signal never
changed. What changed was Hamlet's internal state: the tracker migrated from a
phantom at 850 Hz to the real station at 400 Hz, and **it kept emitting all the
way through the migration.**

### The structural cause

**Hamlet's decoder is a pipeline of hard decisions.** Survey admits or refuses;
the tracker commits to a pitch; the speed search commits to a WPM or withdraws;
the classifier commits to dit or dah; a character is emitted. **Each stage
commits, and no stage can be revised by what a later stage learns.** A wrong
commitment upstream produces confident nonsense downstream, and nothing in the
chain carries a notion of *I am not currently synchronised*.

**The reference implementations do the opposite, and their authors say so
plainly.** Alex Shovkoplyas VE3NEA, who wrote CW Skimmer — eight years of work and hundreds of algorithms before the first version behaved as he wanted — advises approaching every problem in the Bayesian framework: express prior knowledge as probabilities and update with observed data, and rather than deciding at each input sample whether the signal is present, compute the probability that it is present.

**Every fault in the table above is a hard decision made too early.**

### The prior art describes Hamlet's exact failures

Mauri AG1LE rebuilt the fldigi CW decoder along Bayesian lines and documented the
same three problems:

- **The wandering pitch.** Without averaging, the detected peak centre frequency can be wrong, so the decoder is tuned to the wrong frequency — and with a narrow filter this produces garbage output. **That is Hamlet at 850 Hz.**
- **The pinned speed estimator.** The speed estimator sometimes sticks at its lower or upper extreme, and an incorrect speed estimate produces a great deal of garbage text. **That is `-020809` pinning at 40 WPM and `-030850`'s sweep pinning at 400 Hz, the bottom of its own range.**
- **The unmatched filter.** Each decoder instance used one manually-set filter bandwidth rather than a speed-dependent one — faster stations need a wider bandwidth and slower ones a narrower one, so accuracy suffers when stations run at different speeds. **Hamlet's integrator width is a settled constant at 45 Hz** (carried ask 5).

His own architecture is worth naming because it is the shape this decoder is
missing: a noise estimator, Kalman filters estimating the likelihood of the key state given the observed signal, and Bayesian inference updating posterior conditional probabilities for each new path. **Paths, plural, carried forward and re-scored — not one committed guess.**

**Sober about the target:** that decoder still ran around a 3% base error rate. Ninety-nine percent on real air is at or beyond the edge of what published amateur decoders achieve. **This unit does not promise it. It builds the instrument that can tell whether we are approaching it.**

### The fit figure, and why it is inverted

The garbage scored **8224.4 better than silence per hop**; the correct read scored
**11.1**.

**The likely cause is in the name: it compares the chosen reading against
silence.** On a band with a strong signal, *any* reading beats silence, and a
reading chopped into more elements accumulates more evidence against silence than
a correct one with fewer, longer elements. **A sum over elements grows with the
number of elements.**

The quantity that would not invert is **the odds of the best reading against the
second-best reading** — how much better this interpretation is than its nearest
rival, not than nothing at all. **Task 6 measures whether that is the cause. It
does not assume it.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Trust the tree over this order everywhere they differ.

**This author has not seen unit 044's report.** Unit 044 was running when this was
written. **Task 1 must establish what 044 landed** — in particular whether the
no-clock refusal shipped, because it changes what the corpus emits and therefore
every number this unit measures. **State the baseline after 044, not before.**

**Record the failing counts for both suites before task 2.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings:**

> **The goal of this phase is reading one mode at ninety-nine percent.**

> **The only measurement is against real data from the real radio.** Synthetic
> audio may exist as a unit test; it never appears in the score.

> **Ship the refusal** (2026-08-27): no letters from a pitch the survey admitted
> no keying at. **The phantoms are the priority.**
>
> **Rejected with it:** the clock-withdrawn refusal *as unit 1.11.33 built it*,
> measured at 26, 38 and 25 characters off three good captures; and raising the
> gate.

> **When the frequency changes, clear and reset.**

> **Do not mess around at the edges.** This unit exists because the operator asked
> for the analysis to go to the goal rather than the symptoms.

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode.
- **§0.0.1** — the app's record must distinguish a fault in the signal, the radio,
  or Hamlet.
- **§0.4** — a fix that cannot be shown to fix anything is a guess. **Reproduce,
  then change, then measure.**
- **HM-DEC-007** — decoders tested against WAV fixtures.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The tasks

### Task 1 — where the tree actually is

**Run both suites whole and record the numbers.** State what unit 044 landed and
whether its refusal is in force.

Then answer from the code, and **say what you find rather than confirming this
list**:

- **Every hard decision in the decode chain, in order**, with file and line: where
  a pitch is committed, where a speed is committed, where an element is classified
  dit or dah, where a character is emitted. **For each, state what is discarded at
  that point and whether anything downstream can revise it.**
- **What the fit figure computes**, expression by expression. **Name the term that
  grows with the number of elements.**
- **The integrator width** — is 45 Hz a constant, and does anything scale it with
  the estimated speed?
- **Whether any averaging is applied to the pitch estimate before it is
  committed.**

### Task 2 — the answer key enters the tree

**The ARRL propagation bulletin of 2026-08-27 by F. K. Janda, OK1HH is published
verbatim.**

- **Obtain it and vendor it under `data/truth/` with its source and retrieval
  date**, the same way the band plan carries its citation. If the network is
  unavailable, **say so and stop this task** — do not reconstruct the text from
  memory or from Hamlet's own output, which would make the decoder its own answer
  key.
- Record which of the three captures covers which span of the bulletin, and
  **which portions of each capture have truth and which do not.** A capture is
  only scored over the span where truth exists.
- Mark the known sender-side substitution: **`DASH` is the sender's `M` read as a
  word**, not a decoder error.

### Task 3 — the scoring harness

**Build the thing that turns a decode into a percentage.**

- Align Hamlet's output against the truth text with an edit-distance alignment
  (Levenshtein or Needleman–Wunsch) and report **character accuracy, substitutions,
  insertions and deletions, separately.**
- **A block counts as a deletion, not a substitution.** Refusing to guess is not
  the same error as guessing wrong, and the score must be able to show that
  trade — it is the exact trade Tim ruled on in unit 036.
- Report **two numbers per capture**: accuracy over all truth characters, and
  accuracy over the characters Hamlet actually emitted. **The first is yield, the
  second is precision.** Ninety-nine percent means nothing until it is stated which
  one it refers to.
- Pure, deterministic, no clock, no network. Runs from a WAV and a truth file.

### Task 4 — the first score this project has ever had

Run the harness over every capture with truth and **publish the baseline table**:
per capture, accuracy, precision, substitutions, insertions, deletions.

**Report it plainly, however bad it is.** A number that flatters is worse than no
number. **This table is the unit's most important output and it leads the report.**

### Task 5 — the pitch is averaged before it is committed

The tracker committed to 850 Hz where nothing exists, then migrated mid-stream.
The prior art names the cause: **no averaging of the peak estimate before the
decoder is tuned to it.**

- **Average the pitch estimate over a stated window before committing it**, and
  state the window and why.
- **A pitch that has not been stable for that window is not committed at all**, and
  the decoder reports that it is acquiring rather than emitting.
- **While the committed pitch is changing, nothing is emitted.** A migration is not
  a decode.

**Acceptance, measured with task 3's harness:** `-030850` no longer commits to 850
Hz. `-031024` does not regress. **Report the score before and after for every
capture with truth.** If the score falls, **say so and stop rather than shipping.**

### Task 6 — what the fit figure is measuring

**Measure and report. Change nothing in this task.**

- Confirm or refute the analysis above: **does the figure compare the chosen
  reading against silence, and does its value grow with the number of elements
  rather than with correctness?**
- **Across every capture with truth: plot the fit figure against the character
  accuracy from task 3.** If the correlation is negative or absent, **the metric
  is not measuring quality and every decision that consumes it is unsafe.**
- **State what the corrected quantity would be** — this author's reading is the
  odds of the best interpretation against the second-best rather than against
  silence — **and say whether the decoder currently computes a second-best at
  all.** If it does not, that is the finding: a pipeline that keeps one path has
  nothing to compare against.

**Do not change the metric in this unit.** A metric change alters every gate that
consumes it, and it must be done with the score in hand.

### Task 7 — the filter follows the speed

Faster stations need a wider bandwidth and slower ones a narrower one; a single fixed bandwidth costs accuracy across stations of different speeds. Hamlet's integrator is a settled constant at 45 Hz.

- **Scale the integrator width with the committed speed estimate**, and state the
  relationship and its derivation. A dit at 24 WPM is 50 ms; the keying sidebands
  scale inversely with element length.
- **While the speed is not committed, the width does not change.** No estimate, no
  adaptation.
- **Measured with task 3's harness across the corpus.** The carried ask about a
  sharp-peak caveat at 45 Hz stands — **if the sweep shows 45 Hz was right, report
  that and revert.**

### Task 8 — what a real redesign would be, written not built *(the drop candidate)*

**A design note in the repository, no code.**

The analysis says Hamlet's chain commits early and cannot revise. The reference
architecture — noise estimation, Kalman-filtered likelihood of the key state, and Bayesian updating of posterior probabilities across multiple paths — keeps many interpretations alive and re-scores them as evidence arrives.

Write down, with the corpus numbers to support it:

- **What a lattice over (element, speed, pitch) hypotheses would look like** for
  this decoder, and which of the current stages it would replace.
- **What it would cost** — latency, since path decoding is not causal per
  character; complexity; and what would have to be rewritten.
- **What the current architecture's ceiling is**, argued from task 4's baseline and
  task 6's finding, and **whether ninety-nine percent is reachable by repair or
  only by replacement.**
- **The staged path**, if replacement is right: what lands first, what proves it,
  and what can be abandoned safely at each stage.

**This is for Tim to rule on. Recommend; do not decide.** Put the options in
HM-DEC-010's table form.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**The whole digital stream** — FT8, the slot cutter, the sync search, the digital
waterfall, the digital capture press.

Also: the joint decoder; the constrained margin; the meter's rebuild; the
whole-file second pass; the scanner and the calling cycle; `CHANGELOG.md`; the
missing `DECISIONS.md` records; the phrasebook and the recent-places row; the
Twin PBT; the attenuator and `CwPitch` receive conditions; **admission itself** —
unit 043 owns it and this unit must not change it.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not write the truth text from memory or from Hamlet's own output.** If it
  cannot be obtained from a published source, task 2 stops and says so.
- **Do not change the fit figure.** Task 6 measures it.
- **Do not change admission.** Unit 043 owns it.
- **Do not build the lattice.** Task 8 writes it down.
- **Do not tune a constant until a capture reads better.** Every change in tasks 5
  and 7 is measured with task 3's harness across the whole corpus, before and
  after, and **a change that lowers the score is reported and reverted.**
- **Do not report a score without saying whether it is yield or precision.**
- **Do not let `-031024` regress.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**The section that reports measurements leads with task 4's baseline table — the
first accuracy percentage this project has ever produced — and then task 6's
finding about whether the fit figure measures quality at all.**

**The section that says what the owner should expect leads with this: Hamlet now
scores itself against what was actually sent, and the number is stated plainly
whatever it is.**

**If you finish every task, stop and report. Do not start the next unit.**
