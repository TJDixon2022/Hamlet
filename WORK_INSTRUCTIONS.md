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

# Work instruction — stop refusing the signals we can already read

**This unit has a deadline.** Tim is at the radio this evening and the target he
set is **eighty percent of a strong CW signal read correctly, first time**.
Everything below is ordered toward that. The tone-tracker instruction dated
2026-08-24 is withdrawn and must not be run; its subject is real and it is not
what is blocking tonight.

## Why this unit exists

**The unit's number: 11.31 against a gate of 15.**

`cw-2026-08-24-012403` — KD0UN calling CQ, 20 dB above everything more than
40 Hz away — was measured this morning against the WAV itself, using Hamlet's
own likelihood model and segmental Viterbi, mixed down at the tone measured by
interpolated transform peak at **439.81 Hz**:

| audio | speed | ratio | text |
|---|---|---|---|
| 20–30 s, the strong stretch | 20 WPM | **11.31** | `DE KD0UN KD0UN K` |
| 18–30 s | 20 WPM | **11.00** | `ENQ DE KD0UN KD0UN K` |

**The first is a perfect read.** It scores 11.31 and the gate is 15, so nothing
is emitted. The capture's own sidecar says the same in Hamlet's words:
`20 WPM won out of 8 to 32, 11.2 better than silence per hop against a gate of
15`. **The decoder found the right speed, produced the right letters, and the
gate threw them away.**

**The pitch error is nearly free**, which is why the tracker is not tonight's
subject. Over 8–28 s of the same file: 439.9 Hz gives 9.15, 450 Hz gives 8.21,
425 Hz gives 7.83. Ten hertz off costs about one point of ratio on a decode that
was already being refused at eleven.

**And the gate is anti-correlated with correctness**, same method, correct pitch:

| capture | ratio | gate 15 | text |
|---|---|---|---|
| `012403` strong stretch | 11.31 | **refused** | `DE KD0UN KD0UN K` — perfect |
| W1AW `031905` | 17.50 | passed | `I I CTED 00## #E NT` — soup |
| training radio `001520` | 11,584,537,864 | passed | garbage |

The one capture that reads correctly is the one that is refused. This is the
fourth sighting of the same fault: unit 001 found the gap inverted on the
streaming windower (an adjudicated station at 1.7 against an empty band at 6.5),
unit 002 found the Hann integrator narrowed the headroom from 6.6 to 8.0 and
broke three margin assertions, and now this.

**The phase's number:** eighty percent of a strong signal, tonight. The decoder
already delivers a hundred percent on the strong stretch of `012403` and about
eighty-five across the last twelve seconds. **This unit is not about making the
decoder smarter. It is about not silencing it.**

**Build: read `Directory.Build.props` and increment the patch by one.** It read
1.11.2 after unit 002.

## Verify this instruction against the tree

**Nothing here describes the tree.** Every measurement above was taken outside
Hamlet, on the WAVs, with an independent implementation of Hamlet's documented
model. **Reproduce them in-tree at task 4 and report any disagreement** — if
Hamlet's own numbers differ from these, Hamlet's numbers are the truth about
Hamlet and this instruction's premise needs re-examining.

**Known red:** unit 002 reported **1553 passing, 31 failing of 1584** in the
engine, 481 of 481 in the app. Twenty-seven inherited; four are unit 002's Hann
swap — `ARecordingWithNoStationInItSaysNothing(014854)`, `TheGateSitsInAWideGap`,
`TheFiveToEightDecibelPlateauHolds`, `OnlyTheOneTheCouplingBreaksHasTheTrough`.
**Three of those four are gate-margin assertions and this unit changes the gate**,
so they are expected to move. Say what they do; do not tune anything to restore
them.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141 or 150.** This
unit works directly on HM-DEC-120's mechanism and cannot read its text.

## Rulings in force

**HM-DEC-120 — the property, not the number.** `CLAUDE.md`'s index row reads
*"The refusal floor is 14 in the decoder's own margin units, superseding the 17
of HM-DEC-117's interim."* **The property that ruling protects is that nothing
is emitted on audio holding no signal.** The number is the mechanism by which it
was achieved, and the mechanism is now measured to refuse correct reads while
admitting soup.

**Tim's ruling, given 2026-08-24, on being shown the measurements above:**
replace the emit decision with a per-character test and keep the window ratio as
an outer silence guard. **The property is not traded.** Both captures holding no
station must still emit nothing, and that is checked and stated explicitly at
tasks 2, 3, 4 and 5. **A change that reads better and breaks the silence
property is a failed change and must be reverted, not tuned.**

**Do not re-argue. Rejected already:** lowering the window gate on its own
(releases the W1AW soup that already scores 17.5); tuning a threshold to make a
red test green; fitting a constant to a fixture invented in the same session
(unit 002 refused this and was right).

**The keying witness is not a referee this unit reports against** — measured
correct in 5 of 13 captures shack-side. Report absolute counts.

**The sweep's `invented` column counts `CwMatchKind.Wrong`.** Unruled. Report
both columns.

**Shape conflict:** `CLAUDE_CODE.md` §8's five sections win over
`SESSION_PROTOCOL.md` §12.2's three, per §0. Fourth consecutive unit — say so.

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` — `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE`
saying what is moving inside the task. The same every ten minutes while a task
runs. **This unit is against a clock; if a task overruns, say so in the note.**

## The tasks

### Task 1 — why `0 elements`, before anything is built

`012403`'s sidecar says `0 elements seen, 0 resolved` while the same file's
`reading` line says 20 WPM won at 11.2. **The gate explains zero characters. It
does not explain zero elements.** Those counts come from somewhere else and
nobody knows where.

Trace it and report: where element counts are produced, what path they are on,
and why a decode scoring 11.2 reported none.

- **If elements are counted after the gate**, the mystery dissolves, tasks 2 and
  3 fix it, and say so in one line.
- **If they are counted on a separate path that found nothing**, there is a
  second blocker behind the gate. **Report that prominently and keep going** —
  tasks 2 and 3 still stand on their own measurements, but Tim needs to know
  tonight's target may not be reachable by them alone, and he needs to know it
  in the first hour rather than the last.

Build and run the suite; record counts as the green baseline.

### Task 2 — gate each character on its own evidence

Replace the emit decision. Each character must clear a margin on **its own span
likelihood against all-key-up over that same span** — two subtractions from the
cumulative sums `DecodeAt` already builds, and unit 001 already carries
`spanLlr` to the sidecar, so the instrument exists.

- A character clearing its margin prints as a letter.
- A character inside a window that passed the outer guard but failing its own
  margin prints **`■`**, which this tree already renders — unit 002's own output
  shows `NNM■0E0K`, so this is existing display behaviour, not a new assertion.
- **The window ratio survives only as the outer silence guard**, at its current
  value, doing the job HM-DEC-120 gave it.

**The margin is provisional in this task and is derived in task 5**, marked so
in the code. Choose the starting value from the span-LLR distributions the
existing sidecars already hold, not from a guess, and say where it came from.

**Both empty captures emit nothing** — `014854` and `014935`, checked and stated.

### Task 3 — lock the pitch, and show that it is locked

A locked mode in which the mixdown uses a frozen pitch and the tracker does not
steer the decoder. The tracker may keep measuring and reporting; while locked it
does not move the mixdown.

**Set the lock from the strongest measured tone at the moment it engages, by
interpolated transform peak — not from a 25 Hz bin, and not from the radio's
`CwPitch`.** `012403`'s sidecar reads `CwPitch 600 Hz` while the station sat at
439.81, so a lock to the radio's pitch would have listened to empty spectrum.
That is measured, not supposed.

**The lock's state is visible**, following HM-DEC-148's precedent for the
advisory area: whether it is engaged and what pitch it is holding. **A lock the
operator cannot see is a lock he cannot trust, and a wandering decode and a held
one look identical today.** This is a display change made under existing
precedent rather than a new class of assertion; **flag it in the report for
Tim's review** and change nothing else on the panel.

Re-run the corpus and both empty captures.

### Task 4 — measure it, on the real captures

Using unit 002's four-way harness, over every capture in the corpus and both
empty ones, report per capture: correct, wrong, invented, emitted, `■` count,
and the window ratio — read through the production path, and read with the pitch
locked to the measured tone.

**Reproduce the three numbers this instruction was built on** and say whether
they hold in-tree:

- `012403`, 20–30 s, 20 WPM, pitch 439.81 → ratio ~11.3, text `DE KD0UN KD0UN K`
- W1AW `031905` at 499.9 → ratio ~17.5, soup
- `001520` → a ratio in the billions

**Section 3 of the report leads with this table.**

### Task 5 — derive the margin from the measurement *(the drop candidate)*

Task 2's margin is provisional. Set it from task 4's own distributions: the span
LLRs of characters that are correct against those that are invented, with the
margin in the measured gap and the gap reported. **If the distributions overlap
so far that no margin separates them, say that** — it is the most important
finding this unit could produce and it must not be hidden by choosing a number
anyway.

**This is the drop candidate. Dropped whole, and the report says it was
dropped.** Task 2's provisional margin ships if this does not run.

## Parked — do not touch, do not raise

- **`LogLikelihoods` and the `P25 × 0.6` scale.** Real, and the reason
  `001520` scores eleven billion, but at the right pitch the shipped model reads
  `012403` perfectly. It is broken for scoring, not for decoding. **Next unit.**
- **The speed grid and forcing a speed by hand.** Next unit.
- **The tone tracker's movement rules** (HM-DEC-095, HM-DEC-127). Task 3 lets Tim
  bypass the tracker; it does not change what the tracker does when unlocked.
- **The keying sweep and its 5-of-13 verdicts.**
- **`ClearOnAStationChange`, `Restart()`, the `Skip()` splice wall.**
- **`CwUnitEstimator.Runs`**, **`tonePeak`** (fourth sighting), **the
  `characters emitted` / `text nothing read` contradiction**, **HM-OPEN-057**,
  **HM-OPEN-058**, **HM-OPEN-059**.

## What not to do

- **Do not lower the outer window gate.** Task 2 works because the decision moves
  to the character, not because the window bar drops. W1AW's soup already clears
  15.
- **Do not trade the silence property.** Checked and stated at tasks 2, 3, 4, 5.
- **Do not fix the four tests unit 002's Hann swap broke.** Three are gate-margin
  assertions this unit is expected to move; report what they do.
- **Do not add anything to the panel except the lock state in task 3.**
- **Do not change `LogLikelihoods`, the speed grid, or the tracker's movement
  rules.** The unit's claim is that it changed the emit decision and the pitch
  source and nothing else.

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch and say
whether each push succeeded.

Report per `CLAUDE_CODE.md` §8, five sections, `output.md` at the repository
root, overwritten and printed. **Section 2 must say plainly what Tim will see
differently at the radio this evening**, because he is going to the radio on the
strength of it. Section 5 carries the phase number — eighty percent of a strong
signal — and the build confirmed from `Directory.Build.props`.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eleven inbound, none
ruled, the oldest open since 2026-08-14.** Four consecutive units have worked
beside rulings they cannot read.

1. **The sweep's `invented` column counts substitutions, not invented
   characters** — twelve of twenty characters at 18 dB were never sent against a
   column reading nought.
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor** — no field is left for it to match.
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The tone tracker is a large source of soup** — 22 invented against 0 at a
   fixed pitch, on a generated fixture. Task 3 lets Tim bypass it; the rules
   themselves wait on 4 above.
6. **Whether the integrator ships at 45 Hz or 30 Hz.**
7. **The gate's calibration** — now measured anti-correlated with correctness.
   *(This unit acts on it under Tim's ruling above; the outer guard's value is
   still unexamined.)*
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named and the operator is not told
   they are not named.** Display, therefore Tim's.
10. **The keying witness is correct in 5 of 13 captures** and is what is on screen
    when the decoder is silent.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

**If you finish every task, stop and report. Do not start the next unit.**
