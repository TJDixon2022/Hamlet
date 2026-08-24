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

# Work instruction 006 — the noise scale, and the guard that can go once it is fixed

## Why this unit exists

**The unit's number: 7.98 against 4.64.** On this repository's own audio, the
empty capture `cw-2026-08-20-014854` scores 7.98 on the outer window guard while
`cw-2026-08-17-134712`, which holds the adjudicated `N4L`, scores 4.64. **The
empty band outscores the real station, so no value of the guard both admits the
station and refuses the noise.** Unit 1.11.3 measured that and correctly refused
to tune it.

The guard is what stands between a strong signal and the screen. A window scoring
below it is thrown away before any character is judged, so the per-character
decision that unit 1.11.3 built never runs on the signals that most need it.

**Why the guard cannot simply be deleted today.** Unit 1.11.3 found a clean
character-level separation on whole-file reads — `cw-2026-08-18-004507`'s weakest
real character scores 49.8, and the best character either empty capture produces
is 42.5 — and found it collapses on the streaming path, where the same capture's
weakest real character scores 3.1. **The two paths disagree because the noise
scale is estimated once over a whole recording and re-estimated every window in
streaming, so one character is scored against two different noise floors.**

That scale is `Percentile(sorted, 25) * 0.6` in
`CwProbabilisticDecoder.LogLikelihoods`. For a Rayleigh envelope the identity is
`σ = P25 / 0.759`, so the scale is **0.455 σ rather than σ, 2.2× too small**, and
every quadratic term is inflated about 4.8×. It is the same fault behind
`cw-2026-08-23-001520` scoring in the billions — measured outside Hamlet at
**11,584,537,864** on a capture that is 54.1 % exact zeros.

**Fix the scale, and the character margin becomes derivable on the path
production actually runs. Then the guard can go rather than be tuned.**

**Three captures this unit needs are in this zip**, at
`tests/fixtures/cw/captured/unadjudicated/`. Unit 1.11.3 could not verify its
own premise because they were absent; that was the delivering session's fault,
not the tree's.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway. Do not repair
this instruction silently.

**A contradiction in unit 1.11.3's own report, to resolve before task 2.** Its
section 3 corpus table shows `134712` at window 35.8 emitting 28 characters with
`N4L` visible; its premise table shows `134712` at 4.64, refused. Both cannot
describe the same measurement. **Say which is which and which is right** — it
changes how much of the corpus the guard is actually suppressing.

**Known red:** 31 failing of 1596 in the engine, 481 of 481 in the app, the
failing set byte-identical to what unit 002 left. Four of those are unit 002's
Hann swap — `ARecordingWithNoStationInItSaysNothing(014854)`,
`TheGateSitsInAWideGap`, `TheFiveToEightDecibelPlateauHolds`,
`OnlyTheOneTheCouplingBreaksHasTheTrough`. **Three are gate-margin assertions and
this unit changes both the units they are expressed in and possibly the guard's
existence, so they will move.** Report what they do; do not tune to restore them.

**`ElementsSeen` and `ElementsResolved` are the same field** — `CwDecoder` passes
`_elementsResolved` into both slots. Named by unit 1.11.3, still not fixed, and
it will make this unit's element counts read identically. Do not fix it here;
just do not trust the pair.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141 or 150.** This
unit works directly on HM-DEC-120's mechanism and cannot read its text. It also
touches HM-DEC-119's lesson about scales estimated over different spans, cited by
unit 1.11.3 and likewise unreadable.

## Rulings in force

**HM-DEC-120.** `CLAUDE.md`'s index row reads *"The refusal floor is 14 in the
decoder's own margin units, superseding the 17 of HM-DEC-117's interim."*
**The property is that nothing is emitted on audio holding no signal.** Both
captures holding no station must emit nothing, checked and stated explicitly at
every task that touches the signal path. **A change that reads better and breaks
the silence property is a failed change and is reverted, not tuned.**

**Rejected already, do not revisit:** tuning the outer guard to a different
number — the corpus proves no number works, 7.98 against 4.64; fitting a
threshold to a fixture invented in the same session, which unit 002 was asked to
do and correctly refused; carrying a threshold across a change that alters the
units it is expressed in.

**Tim's ruling on the character margin, from unit 1.11.3 and standing:** the
margin is nought, because nought is the point where silence explains the span
exactly as well as the letter does, and it is the one value that is not a tuned
threshold. **If this unit's corrected scale produces a clean measured gap, a
margin inside that gap is a proposal for Tim, not a change to make.**

**PROPOSAL, not ruled — §4.4.** Unit 1.11.3 added a "Hold this pitch" button to
the panel and flagged it. Tim has not ruled it. **Leave it exactly as it is.
Do not extend it, do not remove it, do not add to it.**

**Shape conflict:** `CLAUDE_CODE.md` moved to version 1.3 on 2026-08-24 and the
report is now **four** sections, not five. `SESSION_PROTOCOL.md` §12.2 still says
three headings. §0 gives `CLAUDE_CODE.md` the win. **Read the version line at the
top of the file and follow what is there rather than what this instruction says.**
Every unit since 001 has named this — name it again.

## Status cadence

Named here as well as in the prompt, per §4.5. After each task, before starting
the next, update `PROJECT_STATUS.md` per `CLAUDE.md` — `STATE`, `TASK: n of m`,
`BALL`, `UPDATED` read from the clock, and `NOTE` saying what is moving inside
the task. The same every ten minutes while a task runs.

## The tasks

### Task 1 — the three captures, and the premise at last

The zip places `cw-2026-08-24-012403`, `cw-2026-08-22-031905` and
`cw-2026-08-23-001520` with their sidecars in
`tests/fixtures/cw/captured/unadjudicated/`. Commit them.

**Then reproduce, in-tree, the three figures unit 1.11.3 could not:**

| capture | pitch | expected | source |
|---|---|---|---|
| `012403`, 20–30 s, 20 WPM | 439.81 Hz | ratio ≈ 11.3, text `DE KD0UN KD0UN K` | outside Hamlet |
| `031905` | 499.9 Hz | ratio ≈ 17.5, soup | outside Hamlet |
| `001520` | 600.0 Hz | ratio in the billions | outside Hamlet |

**These came from an independent implementation of Hamlet's documented model, not
from Hamlet. Where Hamlet disagrees, Hamlet is the truth about Hamlet** — report
the disagreement and carry on; the unit does not depend on them matching.

`012403`'s own sidecar records `20 WPM won out of 8 to 32, 11.2 better than
silence per hop against a gate of 15` and `CwPitch 600 Hz` against a station
measured at 439.81 — **note whether the tree agrees that the radio's CW pitch and
the station's pitch are unrelated**, because a later unit may be tempted to lock
to `CwPitch`.

Then trace, with file and line: where the noise scale and the amplitude are
formed, over what span on each path, and every caller that depends on their
present scaling. **If any threshold elsewhere is expressed in these units, name
it** — task 2 moves the ground under all of them.

Build and run the suite; record counts as the green baseline.

### Task 2 — a scale that means the same thing on every capture and every path

In `CwProbabilisticDecoder.LogLikelihoods`:

- **σ from `P25 / 0.759`**, which is `1 / √(2·ln(4/3))`. Put the derivation in
  the doc-comment so nobody re-tunes it as if it were a fudge factor.
- **Key-up as a Rayleigh density**, `ln e − 2 ln σ − e²/2σ²`. The missing `ln e`
  term is what keeps the noise hypothesis competitive in the upper tail, and its
  absence is why noise scores as evidence.
- **σ and the amplitude taken over a rolling two-to-three-second span on both
  paths.** This is the part that makes whole-file and streaming agree, which is
  the whole point: unit 1.11.3's margin of 46 held on one path and cost `VA3VRR`
  on the other. The span length is provisional and marked so; **report what 1.5 s
  and 4 s do to the same table** so it arrives with its own sensitivity measured.

**Both empty captures emit nothing** — `014854` and `014935` — checked and stated.

### Task 3 — measure whether the guard can go

With the corrected scale, over every capture including the three added in task 1:

- the span log-likelihood of every emitted character, on **both** paths;
- the separation between characters in adjudicated callsigns and everything else;
- what either empty capture's characters would score **with the outer guard
  removed entirely**, which is the only way to see them at all — unit 1.11.3 found
  the guard refuses every empty window before any character is judged, so the
  corpus has never produced a single noise-minted character to measure against.

**Then answer one question: with the guard removed, does a character margin exist
that silences both empty captures and keeps all three adjudicated callsigns?**

- **If yes**, report the gap and the value inside it. **Do not remove the guard
  and do not set the margin** — that is a proposal for Tim, in section 4, in the
  decision log's format.
- **If no**, report the overlap and which callsign a silencing margin would cut.
  That is equally the answer and it is section 3's headline either way.

### Task 4 — the corpus table *(the drop candidate)*

Re-run the four-way harness across every capture and both empty ones: correct,
wrong, invented, emitted, `■` count, window ratio, per-character margins, read
through the production path and with the pitch locked. Same shape as unit
1.11.3's so the two can be laid side by side.

**This is the drop candidate. Dropped whole, and the report says it was dropped.**
Tasks 2 and 3 each measure the corpus already; this exists to put one comparable
table in one place, and a partial version is worse than none.

## Parked — do not touch, do not raise

Built from unit 1.11.3's sections 3 and 4:

- **The outer guard's removal and the margin's value.** Measured here, ruled by
  Tim, changed in a later unit.
- **The "Hold this pitch" button and anything else on the panel.** Unruled.
- **Whether the panel should show the tracker disagreeing with an engaged lock.**
  Display, therefore Tim's.
- **`ElementsSeen` / `ElementsResolved` being one field.**
- **The tone tracker's movement rules** (HM-DEC-095, HM-DEC-127), unreadable here.
- **The integrator at 45 Hz against 30 Hz.**
- **The keying sweep's 5-of-13 verdicts.**
- **`ClearOnAStationChange`, `Restart()`, the `Skip()` splice wall.**
- **`CwUnitEstimator.Runs`** — if task 2 moves the measured unit, report it and
  leave the estimator alone.
- **`tonePeak` inflation** — but if task 2's local scale makes the honest figure
  free, say so.
- **HM-OPEN-057, HM-OPEN-058, HM-OPEN-059.**

A parked item that turns out to block a task is raised once, and says it was
parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not remove the outer guard in this unit, and do not tune it.** Task 3
  measures whether it can go; removing it on the strength of that measurement in
  the same session is fitting the change to the fixture that justified it.
- **Do not carry any threshold across task 2.** Every one of them is expressed in
  units this task changes; a threshold whose scale moved underneath it reads as a
  working gate while gating nothing.
- **Do not trade the silence property.** It is the one thing never traded, and
  both empty captures are the test.
- **Do not touch the panel.** What the display asserts is Tim's without exception,
  and there is an unruled button on it already.

## Committing, pushing, reporting

Commit and push each task before starting the next. The report names the branch
and states whether each push succeeded; a refused push is reported as refused,
with the reason.

Report per `CLAUDE_CODE.md` §8 — **check the version line; as of 1.3 it is four
sections** — to `output.md` at the repository root, overwritten and printed.
**Section 3 leads with task 3's answer: whether a character margin exists that
holds silence on both empty captures and keeps all three adjudicated callsigns,
with the numbers.** Section 2 says plainly what Tim will see differently at the
radio.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eleven inbound, none
ruled, the oldest open since 2026-08-14. Six consecutive units have now worked
beside rulings they cannot read.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters** — twelve of twenty characters at 18 dB were never sent against a
   column reading nought.
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor** — no field is left for it to match.
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The tone tracker is a large source of soup** — 22 invented against 0 at a
   fixed pitch.
6. **Whether the integrator ships at 45 Hz or 30 Hz.**
7. **The gate's calibration** — measured anti-correlated with correctness; this
   unit measures whether it can be removed rather than re-tuned.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named and the operator is not told
   they are not named.**
10. **The keying witness is correct in 5 of 13 captures** and is what is on screen
    when the decoder is silent.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Unit 1.11.3's five, still unruled: **the missing captures** *(fixed by this
unit's task 1)*; **the outer guard needing replacement rather than re-tuning**;
**the lock helping sometimes and hurting sometimes with nothing telling the
operator which**; **the button added against instruction**; **`ElementsSeen` and
`ElementsResolved` being one field.**

**If you finish every task, stop and report. Do not start the next unit.**
