UNIT:       046 — stopped at task 2 of 8 — 2026-08-29 11:16
PHASE GOAL: Readable CW on the operator's screen — reading one mode at ninety-nine percent, measured against what was actually sent.
UNIT GOAL:  Give the decoder a real per-character confidence and rebuild the chain around it.
ADVANCED:   no — task 2's own gate fired and stopped the unit; the finding is why, and it is a harder finding than the one expected.
NUMBER:     yield 0.763, precision 0.761 — unchanged, because nothing that reaches the screen was altered.
DRIFT:      1 consecutive unit without advance  (was 0)

## 1. What Claude did

**Stopped at task 2 of 8, on task 2's own stopping condition.** Task 1 landed in
full. **Tasks 3 through 8 were not started**, and that is not a sizing decision:
task 3 is written *"Only if task 2's correlation is positive"*, and tasks 4 to 8
all measure against the metric task 3 would have installed. **The gate fired at
task 2 and the chain behind it is conditional on it.**

Development computer, prompt claimed `PROJECT: Hamlet`, branch `main`, version
`1.12.6` unchanged. **Nothing here is evidence about the radio**: no radio was
connected. **The eight captures of 2026-08-29 are still not in the tree** — a
fourth consecutive unit — so every measurement is over the corpus that exists.

### Task 1 — the baseline, the lock, and the lattice

**Baseline, all three, before anything moved:** app **519 passing, 0 failing**;
engine **28 failing of 1984** excluding `TheGateHasItsOwnWindowNowTests`, which
crashes the host (HM-OPEN-061); corpus **yield 0.763, precision 0.761** over 384
adjudicated characters. Every figure the order carries from unit 045 is confirmed.

**And after: engine 28 failing of 1990, the failing set byte-identical.** The six
added are this unit's silence lock. Nothing regressed.

**The silence lock is in and green.** `TheSilencePropertyIsLockedTests` asserts
zero *letters* on the five captures that currently emit none —
`cw-2026-08-20-014854`, `-014935`, `cw-2026-08-22-014113`, `-014308`,
`cw-2026-08-26-125941` — chosen by measuring the whole corpus rather than by
assumption, plus an all-zero buffer. It locks letters and not characters: a block
is Hamlet saying it heard something and will not name it, which is the honest
output rather than a violation.

**The load-bearing question, answered: no, a backward pass is not possible over
this lattice without restructuring it.**

The lattice is semi-Markov. `best[i]` is the score of the best path whose last
segment *ends* at hop `i`; `kindAt[i]` records that segment's kind and
`wasDown[i]` its parity. A transition from `j` to `i` is scored
`best[j] + evidence − ½(log(span/want)/tolerance)²`
(`CwProbabilisticDecoder.cs:1462–1472`).

**The state is under-specified, and that is the blocker.** The alternation rule
is enforced as `if (wasDown[j] == kind.IsKeyDown) continue` (`:1458`) — against
**the winning path's parity at `j`**, not against a state. A forward or backward
sum has to range over *all* paths reaching `j`, and those do not share a parity,
so there is nothing well-defined to sum. Forward–backward needs the lattice
indexed by `(hop, kind)`, or at minimum `(hop, parity)`; it is indexed by `(hop)`.

**And the same under-specification is a latent defect in the Viterbi itself.** If
the best path into `j` ends key-down while a slightly worse one ends key-up, the
worse one could legally be extended by a key-down segment and **the DP cannot see
it**. Paths are being discarded for a reason that is not part of the model. This
is reported rather than fixed: it changes decode output, and task 2 forbids
restructuring.

**Every consumer of the fit figure**, as task 1 asks:

| consumer | where | what it does |
|---|---|---|
| the emission gate | `CwProbabilisticDecoder.Decode`, `:822` | `ratio < Gate` (1.40) empties the whole window's text |
| the character floor | `Marked`, `:1513` | `SpanMargin < CharacterMargin` (1.0) turns a character into `#` |
| the character's own confidence | `CwProbabilisticStream.Character`, `:734` | `result.LikelihoodRatio` is stored on every `CwCharacter` |
| the capture sheet | `MainWindowViewModel`, `:4924` | prints it as `better than silence per hop` against the gate |

**How speed enters:** a grid from `SlowestWpm` 8 to `FastestWpm` 40 stepped by
`WpmStep`, each hypothesis fully decoded, **only `bestScore` retained** (`:794–805`).
Every losing hypothesis is discarded inside the loop.

**How durations are modelled — and the order's task 5 premise is wrong here.**
They are **not** thresholded. Each segment already carries an explicit duration
density: a log-normal penalty `½(log(span/want)/LengthToleranceShare)²` around the
kind's expected length, with the span constrained to `[ShortestShare·want,
LongestShare·want]`. **That is a hidden semi-Markov model with explicit duration
densities already**, which is what task 5 proposes to build. What it lacks is
densities *fitted from the corpus* rather than parameterised off the dit.

### Task 2 — stopped, and the reason is worse than the one expected

**The order's premise that the decoder keeps no runner-up is wrong, and the error
was mine.** Unit 045 reported *"the decoder computes no second-best at all"* and
that the string `secondBest` does not occur in the file. The string does not; **a
`second[]` array does** (`:1475–1484`), holding the runner-up score at every hop,
and `MarginLlr` is already `best[at] − second[at]` (`:1586`). It flows to every
character and the character floor already consumes a quantity derived from it. I
grepped for the wrong identifier and drew a conclusion from its absence. **The
order inherited that error from my report.**

**So the measurement task 2 exists for could be taken on quantities that already
exist** — and per character rather than per recording, which is new.
`CwAccuracy` gained an alignment reporting what became of each read character, so
a number attached to a character can be tested against whether that character was
right.

**Over 301 scored characters across the twelve captures with truth:**

| candidate | correlation with correctness | median on right characters | median on wrong |
|---|---|---|---|
| `MarginLlr` | **−0.351** | 2.0403 | 1.9309 |
| `MarginShareForRecord` | **−0.345** | 0.0029 | **0.0074** |
| `SpanMarginForRecord` | **−0.190** | 10.4666 | 9.1717 |

**All three are negative.** And the middle row is the plainest statement of it:
**wrong characters carry more than twice the margin share of right ones.** The
quantity the character floor is built on runs backwards.

Beside unit 045's figures for the fit ratio — **−0.179** against yield and
**−0.203** against precision — that is now **five** quantities measured against
correctness and **five** negative results. **Nothing this decoder computes about
its own confidence tracks whether it is right.**

Task 2's instruction is explicit: *if the posterior does not correlate positively,
stop and report — the rest of the unit rests on it.* **It does not, and the unit
stops.**

**Acceptance was met on the part that could run:** the corpus score is unchanged
at 0.763 / 0.761, because nothing reaching the screen was altered.

### Tasks 3 to 8 — not started, and why

- **Task 3** is written *"Only if task 2's correlation is positive"*. It is not.
- **Tasks 4, 5, 6, 7** each require measurement against the metric task 3 would
  install, and each must show precision not falling — against a precision figure
  that would come from that metric.
- **Task 8** is the drop and depends on task 4.
- **Task 5's premise is separately wrong**, per task 1: explicit duration densities
  are already there.

No decision was recorded under §12.1.

## 2. What the owner should expect

**Nothing on the screen changed. Hamlet does not yet know how sure it is of each
character — and the reason is now measured rather than suspected.** It still shows
nothing at all on a frequency where nothing is happening, and that is now locked by
a test that later work is forbidden to modify.

What is now true of the tree:

- `TheSilencePropertyIsLockedTests` guards five captures and an all-zero buffer.
- `CwAccuracy` can align per character, so any future confidence can be tested
  against correctness in one command.
- `tools/Hamlet.PitchRank` gained `confidence`.
- **No decoder behaviour changed.** The corpus score is identical.

**What will look wrong but is not:**

- **The unit stopped at task 2 of 8.** That is task 2's own instruction, not a
  sizing decision, and tasks 3 to 8 are conditional on it.
- **Unit 045's claim about the second-best was wrong and this report corrects it.**
  A runner-up exists and has all along.
- **The engine baseline is still 28 failing**, byte-identical to unit 045's set,
  now of 1990 with the silence lock added. No product code changed.
- **The full engine suite has no clean run** — the host crash of HM-OPEN-061,
  which unit 043 recorded as wider than the class it names.

## 3. What you should see

**Every quantity this decoder computes about its own confidence runs backwards
against correctness.** Five now, all negative:

| quantity | against | correlation |
|---|---|---|
| fit ratio | yield | −0.179 |
| fit ratio | precision | −0.203 |
| `MarginLlr` | per-character correctness | **−0.351** |
| `MarginShareForRecord` | per-character correctness | **−0.345** |
| `SpanMarginForRecord` | per-character correctness | **−0.190** |

**Wrong characters carry more than twice the margin share of right ones** — 0.0074
against 0.0029. The character floor at 1.0 is built on that quantity.

**And the posterior that would replace them cannot be computed over this lattice
as it stands.** The alternation rule is checked against the winning path's parity
rather than against a state, so there is no well-defined set of paths to sum over.
That is not a difficulty; it is a statement that the lattice is not a proper
hidden Markov lattice. **The same gap means the current Viterbi can discard a
legal path** because a *different*, better path into the same hop happened to end
on the wrong parity.

**So the structural change this unit was written to make is the right one, and it
is larger than the order allows for.** Task 2 says *do not restructure the lattice
in this task*; the restructure is the prerequisite, not an optional extra.

The unit did buy two things. **The silence property is locked** before any of it
starts, which was Tim's condition. And **the instrument now works at character
level**, so the next confidence proposed can be tested in one command against 301
characters rather than argued about.

## 4. What's blocking us

Two rulings, and the first is the unit.

> **The lattice is indexed by `(hop, kind)` before any posterior is attempted, and
> that is accepted as a change to what the decoder reads.**
>
> Forward–backward needs a well-defined set of paths at each node. This lattice
> checks alternation against `wasDown[j]`, the winning path's parity, so the paths
> reaching a node do not share a state and there is nothing to sum. Indexing by
> `(hop, kind)` — five kinds — fixes it, and **it also fixes a defect in the
> existing Viterbi**, which today discards legal paths whose predecessor was not
> the best one at that hop.
>
> **It will change what the decoder reads**, because paths currently unreachable
> become reachable. So it cannot be done under task 2's "bit-identical" acceptance
> and needs measuring with `CwAccuracy` before and after like any other change.
> **Rejected: attempting forward–backward on the lattice as it stands.** There is
> no correct way to do it; an approximation would produce a number that looks like
> a posterior and is not one, which is §0.0 in the place it does most damage.
> **Rejected: doing it silently inside this unit.** The order forbids restructuring
> here and the change alters output, so it is yours.

> **Whether the confidence work continues at all, given five negative results.**
>
> Five quantities have now been measured against correctness and every one is
> negative. That is a consistent finding rather than five accidents, and the
> common cause is visible: all five are differences of path scores, and the path
> scores are dominated by the unbounded `−e²/2σ²` term that makes the fit ratio a
> loudness measure. **A margin between two numbers that both scale with loudness
> still scales with loudness.**
>
> **Rejected: trying a sixth quantity of the same kind.** Six admission statistics
> were measured dead across units 1.11.17 to 1.11.21 and this is the same shape.
> **What would be different** is a quantity normalised so that it cannot grow with
> level — which is the posterior, and which needs the restructure above. **So the
> two rulings are one decision**: either the lattice is rebuilt and a real
> posterior becomes possible, or the confidence work stops and the precision target
> is pursued another way.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The eight 2026-08-29 captures are not in the tree**, blocking a fourth
   consecutive unit.
2. **The lattice restructure** — raised above, and it gates the rest of unit 046.
3. **The fit figure does not track correctness**, raised in unit 045 and now joined
   by three more quantities.
4. **The answer key's licensing** — §2.1 and HM-DEC-049 against vendoring an ARRL
   bulletin; raised in unit 045, parked by this order, named once here because it
   bounds how much truth the score can ever have.
5. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
6. **A dial move's threshold is provisional at 500 Hz.**
7. **The transcript break's wording.**
8. **The attenuator's condition on a live overflow reading**, and whether `CwPitch`
   should follow an admitted station.
9. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
10. **The `reading` line's span wording needs approval.**
11. **Two stations closer than 125 Hz are not named.**
12. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
13. **Nothing checks that deleting a surface is not deleting a capability.**
14. **The engine test host crashes**, wider than the class HM-OPEN-061 names.
    Owned by Claude, not waiting on a ruling.
15. **A second intermittent**, `ARigWhoseReadLoopIsStuckStillDisconnects`, seen in
    unit 044 and not reproduced since.

### The restatement of the goal, for Tim to enter

The order asks for this to be written into the report for him to rule on, and not
to be minted as a decision.

> **The phase goal is precision before yield: Hamlet never puts a wrong character
> on the screen, and shows as much of the traffic as it can under that
> constraint.**
>
> Yield of ninety-nine percent is a research problem and the best published
> amateur decoders do not reach it on real air; the one this project's orders cite
> carried a three percent base error rate. **Precision of ninety-nine percent is
> what §0.0 actually demands** — a screen showing three quarters of the traffic
> with the rest blocked is more useful to a novice than one showing all of it with
> a quarter wrong, because the second cannot be trusted at all.
>
> **The corpus today is yield 0.763 and precision 0.761**, and the gap between
> those two numbers is the whole problem: they are nearly equal, which means
> Hamlet is not refusing anything. 61 substitutions against 30 deletions.
