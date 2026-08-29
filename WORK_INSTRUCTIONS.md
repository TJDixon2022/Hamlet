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

# Work instruction 048 — the lattice rebuilt

**ISSUED: 2026-08-29. A fresh order, not an amendment. Follows unit 047.**

**Eight tasks; task 8 is the drop. This is the structural change units 044, 045
and 046 were each written toward, and it is the largest this decoder has had.**

## Why this unit exists

**Five quantities have now been measured against correctness and every one is
negative.**

| quantity | against | correlation |
|---|---|---|
| fit ratio | yield | −0.179 |
| fit ratio | precision | −0.203 |
| `MarginLlr` | per-character correctness | **−0.351** |
| `MarginShareForRecord` | per-character correctness | **−0.345** |
| `SpanMarginForRecord` | per-character correctness | **−0.190** |

**Wrong characters carry more than twice the margin share of right ones** — 0.0074
against 0.0029 — and the character floor is built on that quantity.

**The common cause is understood.** All five are differences of path scores, and
the path scores are dominated by an unbounded `−e²/2σ²` term (`LogLikelihoods`,
`:973`). Two competing paths disagree about some hops, so their difference scales
with how loud those hops were. **A loud station produces a large margin whether or
not the reading is right.** Unit 046's report states it exactly: a margin between
two numbers that both scale with loudness still scales with loudness.

**Trying a sixth quantity of the same family would repeat a known dead end** — six
admission statistics were measured dead across units 1.11.17 to 1.11.21.

**A posterior is different in kind, not in degree.** It is a ratio over the sum of
all paths, so the level terms cancel in the normalisation and it cannot grow with
loudness by construction. **And it cannot be computed on this lattice.**

### Why it cannot, and what that means

Unit 046 answered the load-bearing question and the answer was worse than
expected. The lattice is semi-Markov: `best[i]` is the score of the best path
whose last segment *ends* at hop `i`, with `kindAt[i]` and `wasDown[i]` recording
that segment's kind and parity, and a transition scored
`best[j] + evidence − ½(log(span/want)/tolerance)²` (`:1462–1472`).

**The alternation rule is enforced as `if (wasDown[j] == kind.IsKeyDown) continue`
(`:1458`) — against the winning path's parity at `j`, not against a state.**

- **A forward or backward sum has to range over all paths reaching `j`, and those
  do not share a parity.** There is nothing well-defined to sum. The lattice is
  indexed by `(hop)`; forward–backward needs `(hop, kind)`.
- **And the same gap is a live defect in the Viterbi itself.** If the best path
  into `j` ends key-down while a slightly worse one ends key-up, the worse one
  could legally be extended by a key-down segment and **the search cannot see it.**
  **Paths are being discarded for a reason that is not part of the model.**

**So this is not a proper hidden Markov lattice, and the restructure is the
prerequisite rather than an optional extra.**

### Two premises of earlier orders were wrong, and are corrected here

- **The decoder does keep a runner-up.** A `second[]` array exists (`:1475–1484`)
  and `MarginLlr` is already `best[at] − second[at]` (`:1586`). Unit 045 grepped
  for `secondBest`, found nothing, and drew the wrong conclusion; unit 046's order
  inherited it. **Do not repeat that: check the code, not this order.**
- **Explicit duration densities already exist.** Each segment carries a log-normal
  penalty `½(log(span/want)/LengthToleranceShare)²` around its kind's expected
  length, bounded by `ShortestShare` and `LongestShare`. **What is missing is
  densities fitted from the corpus rather than parameterised off the dit.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Trust the tree over this order everywhere they differ.

From unit 047's report:

- App **519 passing, 0 failing.** Corpus **yield 0.763, precision 0.761** over 384
  adjudicated characters, unchanged.
- **`TheSilencePropertyIsLockedTests` is green and unmodified.** It covers
  `cw-2026-08-20-014854`, `-014935`, `cw-2026-08-22-014113`, `-014308`,
  `cw-2026-08-26-125941` and an all-zero buffer, and it locks **letters**, not
  characters — a block is honest output.
- `CwAccuracy` aligns per character. `tools/Hamlet.PitchRank` has `confidence`.
- **Unit 047's engine regression never landed.** Its amendment line was left
  unreplaced, which that report states is the honest record of a run the
  HM-OPEN-061 host crash ended. **Unit 046 left the engine at 28 failing of 1990.**
  **Establish the real number in task 1; do not carry 1990.**
- **047 did not touch the decoder** and the corpus score confirms it.

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings, 2026-08-29:**

> **The lattice is indexed by `(hop, kind)`, and that is accepted as a change to
> what the decoder reads.** It is measured with `CwAccuracy` before and after like
> any other change.
>
> **Rejected: attempting forward–backward on the lattice as it stands.** There is
> no correct way to do it, and an approximation would produce a number that looks
> like a posterior and is not one — §0.0 in the place it does most damage.
> **Rejected: trying a sixth quantity of the existing family.** Five measured, five
> negative, one understood cause.

> **The phase goal is precision before yield.** Hamlet never puts a wrong character
> on the screen, and shows as much of the traffic as it can under that constraint.
> Yield of ninety-nine percent is a research problem; **precision of ninety-nine
> percent is what §0.0 actually demands.** The corpus today is 0.763 and 0.761 —
> nearly equal, which means Hamlet is refusing almost nothing. **61 substitutions
> against 30 deletions.**

> **Do not break the silence behaviour.** *I'm no longer seeing random characters
> when there's just noise. Don't break it.* **Not tradeable at any price.**

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
- **§0.4** — reproduce, then change, then measure.
- **HM-DEC-007** — tested against WAV fixtures.
- **§5.4** — pure over samples and elapsed time; no clock below the pump.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The measurement rule that governs every task below

**Every change is measured with `CwAccuracy` over the whole scored corpus, before
and after, and both numbers are reported per task.**

- **Precision must not fall. Ever.** A change that lowers precision is reverted and
  reported, whatever it does to yield.
- **A change that raises precision and lowers yield is kept**, and both numbers are
  stated. **That is the trade this unit exists to make.**
- **`TheSilencePropertyIsLockedTests` is run after every task and may not be
  modified.** If a task turns it red, that task is reverted.
- **Substitutions are reported separately at every step.** 61 today. **That count
  is the unit's real target.**

## The tasks

### Task 1 — baseline and the lattice, exactly

**Run both suites and the corpus score. Record all three.** The engine number is
not known — 047's run never landed — so **establish it and say whether the failing
set is byte-identical to unit 046's.**

Then read the lattice and **report what is actually there**, not what this order
says:

- **The state, the transitions, and every place `wasDown[j]` or `kindAt[j]` is
  read.** The restructure has to preserve every one of them.
- **The five kinds**, their expected lengths, and where `ShortestShare`,
  `LongestShare` and `LengthToleranceShare` are set.
- **How `second[]` is maintained** and everything downstream of `MarginLlr`.
- **Where the speed grid enters and what it discards** (`:794–805`).
- **Every numerical guard already present** — clamping, scaling, log-domain
  handling. Unit 045 recorded fit values of 5,521,967, 17.2 million and
  quadrillions on degenerate bins, and the carried asks record intermittent
  overflows. **Underflow and overflow are the known failure of what task 3 builds.**

### Task 2 — the lattice indexed by `(hop, kind)`

**Viterbi only. No posterior in this task.**

- Index the DP by `(hop, kind)` — five kinds — so the alternation rule is checked
  **against a state rather than against the winning path's parity.**
- **Every path legal under the model becomes reachable**, including the ones the
  current search discards because a different, better path into the same hop ended
  on the wrong parity.
- `second[]` becomes the runner-up **within the new indexing**, and `MarginLlr`
  follows it.
- **The duration densities and the evidence term are unchanged in this task.** Only
  the indexing moves.

**Acceptance:** **the output will change**, and bit-identical is explicitly not the
bar here. **Precision must not fall**, and the full table is reported before and
after with the substitution count. **If precision falls, revert and report** — that
would mean the discarded paths were doing useful work by accident, which is itself
a finding worth the unit.

### Task 3 — the posterior

**Forward–backward over the new lattice.**

- **Log domain with scaling throughout.** The overflow history above is the
  expected failure mode; **tests must cover both underflow and overflow**, on the
  degenerate bins that produced the quadrillions.
- Output **a posterior in [0,1] per emitted character** — the probability that the
  character is what the best path says, marginalised over all paths.
- **Emit nothing differently in this task.** The posterior is computed and carried;
  no gate consumes it yet.

**Acceptance, and this is the unit's gate:** correlate the posterior against
per-character correctness over the scored corpus, **reported beside the five
negatives above.**

- **If it correlates positively, continue.**
- **If it does not, stop and report.** Tasks 4 onward rest on it, and a sixth
  negative would be a finding about the evidence model rather than about the
  search — **say so plainly and stop rather than trying a seventh quantity.**

### Task 4 — the gates move onto the posterior

**Only if task 3's correlation is positive.**

Every consumer named in unit 046's task 1 takes the posterior:

| consumer | where | today |
|---|---|---|
| emission gate | `Decode`, `:822` | `ratio < Gate` (1.40) empties the window |
| character floor | `Marked`, `:1513` | `SpanMargin < 1.0` gives `#` |
| the character's confidence | `Character`, `:734` | `LikelihoodRatio` stored on every `CwCharacter` |
| the capture sheet | `MainWindowViewModel`, `:4924` | `better than silence per hop` |

- **Thresholds are derived by sweeping the posterior against the corpus and
  reporting the curve**, then choosing the point that reaches the precision target.
  **State the number and show the sweep.** Do not try values until it reads better.
- **A character below the threshold is a block**, which `CwAccuracy` scores as a
  deletion, so the trade is visible.
- **The fit figure stays computed and stays on the sheet**, labelled for what it
  is, because the corpus's history is expressed in its units.

**Acceptance: precision rises, and the substitution count falls.** Yield may fall
and that is the accepted trade. **Report both.**

### Task 5 — the speed goes inside the lattice

**Stop estimating WPM separately and feeding it in.** The withdrawn clock, the
40 WPM pin, and the garbage that follows are artefacts of a separate estimator
that can fail on its own.

- **Dit length becomes a slowly-varying state dimension**, so speed and text are
  solved together and the best path carries its own speed.
- **A transition cost penalises fast changes in dit length**, because a sender's
  speed drifts and does not jump. State the cost and its derivation.
- **The grid at `:794–805` becomes the initialisation of that dimension**, not a
  committed answer.
- **There is then no clock to withdraw.** Report what becomes of the sheet's
  `decoderWpm` line and **put the proposed wording in the report — the sheet's
  wording is Tim's.**

**Acceptance:** measured before and after. **Precision must not fall.** **Report
what happens on the captures where the clock previously withdrew.**

### Task 6 — the densities fitted from the corpus

The densities exist; **their parameters are assumed rather than measured.**

- **Fit each kind's duration density from the corpus** — dit, dah, intra-character
  gap, inter-character gap, word gap — rather than parameterising off the dit by
  the textbook ratios.
- **Report what the corpus says the real ratios are.** A hand-sent fist is not
  1:3:1:3:7, and that is a finding about the operators on the band as much as about
  the decoder.
- **Fit on captures excluded from the scored corpus**, or state plainly that they
  overlap and what that does to the score. **Fitting on the answer key would make
  the number meaningless.**

**Acceptance:** measured before and after. **Precision must not fall.**

### Task 7 — a language prior

Morse traffic is highly predictable — prosigns, callsign structure, `CQ DE`, `73`,
`RST`, plain English. This is where the last several percent live, and it is why
CW Skimmer sanity-checks against a callsign database.

- **A character n-gram prior added to the path cost.** Order and smoothing stated
  and justified.
- **Trained on text that is not the scored corpus. Say plainly what it was trained
  on.**
- **The prior is a tiebreaker, not an author.** It may reorder paths the acoustic
  evidence finds nearly equal; **it may not create a character the signal does not
  support.** State how that boundary is enforced and test it.
- **The silence lock is the proof**: a capture holding no station must still emit
  nothing.

**Acceptance:** measured before and after. **Precision must not fall.** **Report
substitutions separately** — a prior that raises yield by inventing plausible words
is the worst possible outcome, and the substitution count is where it shows.

### Task 8 — one decoder per bin *(the drop candidate)*

**The state of the art does not track a pitch. CW Skimmer runs a decoder on every
bin across the passband — hundreds in parallel — and AG1LE's multichannel fldigi
does the same: detect peaks, spin up an instance per channel, merge anything
closer than 20 Hz so one strong station does not spawn several.**

That dissolves the tracker's failures rather than patching them: no single
committed pitch means no wandering and no mid-stream migration.

- **Run a decoder per candidate bin across the admitted range** and report those
  whose posterior clears task 4's threshold.
- **Average the peak estimate before an instance is placed on it** — the documented
  cause of tuning to the wrong frequency.
- **Merge candidates closer than the resolution.** Carried ask 13 names 125 Hz.
- **The screen shows what it showed before** unless Tim rules otherwise. This task
  is about where the decoders sit, not a new surface.

**This is only possible because task 4 gives a scale-free score comparable across
channels.** With the old quantities it could not be done at all.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**The whole digital stream** — FT8, FT4, the digital tab, the slot cutter, the sync
search, the digital waterfall, the digital capture press. **Another conversation
owns it.**

**The settings contract** — `OwnedSettings`, the coverage table, the mode write's
place in it, and what the digital rows state. Unit 047 raised both and they are
Tim's.

Also: admission itself; the attenuator and preamp rules; the scanner and the
calling cycle; `CHANGELOG.md`; the missing `DECISIONS.md` records; the phrasebook
and the recent-places row; the Twin PBT; the answer key's licensing; the dial-move
threshold and the transcript break's wording; whether `CwPitch` follows an admitted
station.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not break the silence property**, and **do not modify its lock.**
- **Do not let precision fall on any task.** Revert and report.
- **Do not continue past task 3 if the posterior does not correlate positively.**
- **Do not approximate forward–backward on an under-specified lattice.** A number
  that looks like a posterior and is not one is worse than none.
- **Do not train the language prior on the scored corpus.**
- **Do not let the language prior create a character the signal does not support.**
- **Do not pick a threshold by trying values until the corpus reads better.** Sweep,
  report the curve, say why the chosen point is on it.
- **Do not adopt a value off a non-monotonic sweep.** Unit 045 got this right and it
  is the standard here.
- **Do not change admission.**
- **Do not report a score without saying whether it is yield or precision.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused push
is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**Write `output.md` before you stop, for any reason at all. Do not hold it behind a
regression run** — unit 047 drafted its report and waited, and the engine result
never landed. **Write the file, then amend it.**

**The section that reports measurements leads with a table of one row per task —
yield, precision and substitutions after each — so the whole unit's arc is one
picture. Then task 3's correlation between the posterior and correctness, beside
the five negatives.**

**The section that says what the owner should expect leads with this: Hamlet now
knows how sure it is of each character and blocks the ones it is not sure of — and
it still shows nothing at all on a frequency where nothing is happening.**

**If you finish every task, stop and report. Do not start the next unit.**
