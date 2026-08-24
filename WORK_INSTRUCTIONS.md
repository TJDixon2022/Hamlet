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

# Work instruction 008 — the guard in the units it is measured in

**This unit has a deadline.** Tim is at the radio this evening and the target is
**eighty percent of a strong CW signal read correctly, first time.** Task 4
states whether that target is met, as a number.

## Why this unit exists

**The unit's number: 1.10 against a guard of 15.**

Unit 1.11.4 corrected the noise scale and the decoder got better at reading.
With the guard bypassed, the corpus reads the best this repository has recorded —
**`WESTERNS` where the old scale read `WESNRNS`, `FLUX` where it read `FLAX`** —
and `cw-2026-08-24-012403` produces **`DE KD0UN KD0UN K`**, the exact text this
work was commissioned to recover, at a window ratio of **1.10**.

`Gate = 15` was calibrated when the scale was 2.2× too small. In the corrected
units the captures that read score between **1.10 and 10.77**, so the guard now
refuses nearly all of them: four recordings that read yesterday read nothing
today. **The model improved and the threshold did not follow it.**

Unit 1.11.4 was forbidden to touch the guard and was right to stop. **Setting a
constant from numbers a session measured itself is fitting; setting it from
numbers already published in a prior unit's report is re-expression.** This unit
does the second, and says which published line each number came from.

**Two adjudicated callsigns were lost by the same change and nobody knows why.**
`VA3VRR` on `cw-2026-08-17-013347` and `N4L` on `cw-2026-08-17-134712` both read
under the old scale and neither reads now. `013347` still scores 1.7 × 10⁷,
which is not a guard problem. **If the guard explains them, task 1 closes in
minutes. If the estimator explains them, that is the more important finding and
task 3 exists for it.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway. Do not repair this instruction
silently. Unit 1.11.4 found three of its instruction's claims out of date and
said so; do the same.

**Known red: 49 failing of 1600 in the engine, 481 of 481 in the app**, against
a 32 baseline. Nineteen moved when the scale changed, three went green.
**Eleven are ordinary decode assertions failing because the guard silences those
recordings** — expect them to move back as task 2 lands.

**`ARecordingWithNoStationInItSaysNothing(014854)` went green for the first time
since unit 002 and must stay green.**

**`ItReadsWhatTheReferenceReads` is failing because
`tools/reference-decoder/reference_decoder.py` still carries `P25 × 0.6` and the
Gaussian key-up.** Task 5 addresses it. **Do not touch it before then, and never
in the same commit as a decoder change**, which would make the check agree with
itself by construction.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141 or 150.** This
unit sets the constant HM-DEC-120's floor is expressed in and cannot read its
text.

**`CLAUDE_CODE.md` changed from five report sections to four on 2026-08-24
without its version line moving** — both copies read 1.3. **Read the file's own
section count and follow that.**

## Rulings in force

**HM-DEC-120.** `CLAUDE.md`'s index row reads *"The refusal floor is 14 in the
decoder's own margin units, superseding the 17 of HM-DEC-117's interim."*
**The property is that nothing is emitted on audio holding no signal.** Both
empty captures must emit nothing, checked and stated explicitly at every task
that touches the signal path. **A change that reads better and breaks the silence
property is a failed change and is reverted, not tuned.**

**Tim's ruling on the character margin, standing from unit 1.11.3:** the margin
is nought, the point where silence explains the span exactly as well as the
letter does. It survived the rescaling untouched. **Not changed here.**

**Rejected already, do not revisit:**

- **Reverting the corrected scale.** It reads better character for character
  wherever it reads; the fault is the threshold.
- **Replacing the guard with a character margin.** Unit 1.11.4 measured it: the
  best noise character scores 4.50 and `KD0UN`'s weakest scores 1.75.
- **Setting the guard from numbers this session invents.**
- **Improving the `1e-9` σ floor.** The floor is the symptom.

**PROPOSAL, not ruled — §4.4.** The "Hold this pitch" button from unit 1.11.3 is
still unruled. **Leave it exactly as it is. Do not extend, remove, or add to the
panel.**

## Status cadence

Named here as well as in the prompt, per §4.5. After each task, before starting
the next, update `PROJECT_STATUS.md` per `CLAUDE.md` — `STATE`, `TASK: n of m`,
`BALL`, `UPDATED` read from the clock, and `NOTE` saying what is moving inside
the task. The same every ten minutes while a task runs. **This unit is against a
clock; if a task overruns, say so in the note.**

## The tasks

### Task 1 — why `VA3VRR` and `N4L` vanished

Both read under the old scale. Neither reads now.

Answer one question first, because it decides the size of this unit: **are they
refused by the guard, or are they never decoded at all?**

- **If the guard refuses them**, report the window ratios they score and say so
  in one line. Task 2 fixes them and task 3 is not needed.
- **If they are not decoded even with the guard bypassed**, report what the
  estimator does on those recordings — the proportion of exact zeros, the longest
  run, what σ and the amplitude evaluate to in the worst window, whether the
  `1e-9` clamp is reached — and **whether it is the same fault as
  `cw-2026-08-23-001520`**, which scores 1.4 × 10¹⁶.

Build and run the suite; record counts as the green baseline.

### Task 2 — the guard, re-expressed

**Set `Gate` from unit 1.11.4's published ungated table**, naming the line each
figure came from. That table gives, in the corrected units:

| capture | window ratio | holds |
|---|---|---|
| `003758` | 10.77 | `AA4MP/4 QNIK` |
| `004507` | 6.96 | the ARRL bulletin |
| `003126` | 5.96 | readable English |
| `031905` | 4.93 | a propagation bulletin |
| `003016` | 4.55 | readable English |
| `012403` | **1.10** | `DE KD0UN KD0UN K` |
| `014854` | **0.65** | **nothing** |
| `013622` | **0.20** | 55 characters, no station adjudicated |

**Re-measure every line before using it** — task 1 or task 3 may have moved them —
and set the guard in the gap the current figures show, **stating the gap and both
of its edges.**

**If no gap admits `012403` and refuses `014854`, say so plainly and leave `Gate`
where it is.** A guard that cannot separate is the finding of this unit, and a
fitted number would hide it.

**The doc-comment carries the derivation, the source of each number, and the
date**, so the next session can tell a measured constant from an inherited one.

Re-run the corpus, the sensitivity sweep, and both empty captures. **The silence
property is asserted, not inferred.**

### Task 3 — the estimator on digital silence *(only if task 1 implicates it)*

**Skip this task and say it was skipped if task 1 found the guard responsible for
the two lost callsigns.**

Otherwise: replace the percentile-based noise scale with one that survives audio
containing exact zeros and long silences. **The specification, not the method:**

- On a window that is entirely digital silence it returns **no estimate**, and
  the decoder reads nothing from that window rather than reading noise against a
  clamped σ. Silence is an absence of measurement, and HM-DEC-009 says an unread
  value says so.
- On a window holding keying it lands within a few per cent of the true noise σ
  on the generated fixtures, where the truth is known.
- No capture in the corpus reaches an arbitrary floor.

**Report what was chosen and what was rejected, with the numbers.** If this task
runs, task 2's guard is re-derived after it and the report says so.

### Task 4 — does `012403` clear eighty percent, end to end

**Through the production path, with the guard in place** — not bypassed, not
whole-file, not forced — decode `cw-2026-08-24-012403` and report:

- the text emitted;
- **the percentage of the sent text read correctly**, against
  `CQ CQ CQ DE KD0UN KD0UN K`, stated as a number;
- the same for the strong stretch alone, 20–30 s, where the station stands 20 dB
  above everything more than 40 Hz away;
- the window ratios across the run, and how many windows cleared the guard.

**This is the number the day was spent on. It leads section 3 whether it is
eighty percent or nine.**

Report the same percentage for `004507`, the cleanest recording in the tree, so
one figure is not the whole basis.

### Task 5 — the reference implementation *(the drop candidate)*

`tools/reference-decoder/reference_decoder.py` still carries the old model, so
`ItReadsWhatTheReferenceReads` compares a corrected port against an uncorrected
reference.

Port the corrected key-up density and, if task 3 ran, its estimator — **as a
separate commit from any decoder change.** Report what the test does. **If the
two still disagree, report the disagreement rather than closing it**; a reference
edited until it agrees is worth nothing.

**This is the drop candidate. Dropped whole, and the report says it was dropped.**

## Parked — do not touch, do not raise

- **The character margin at nought.** Ruled, and it survived the rescaling.
- **The panel, the "Hold this pitch" button, and whether the panel should show
  the tracker disagreeing with an engaged lock.** Unruled, Tim's.
- **`ElementsSeen` and `ElementsResolved` being one field.**
- **The tone tracker's movement rules** (HM-DEC-095, HM-DEC-127), unreadable here.
- **The integrator at 45 Hz against 30 Hz.**
- **The keying sweep's 5-of-13 verdicts.**
- **`ClearOnAStationChange`, `Restart()`, the `Skip()` splice wall.**
- **`CwUnitEstimator.Runs`** — if anything here moves the measured unit, report
  it and leave the estimator alone.
- **The rolling span length**, measured at 2.5 s with 1.5 s losing `KD0UN`. If
  task 3 changes that sensitivity, report it; do not re-tune the span.
- **HM-OPEN-057, HM-OPEN-058, HM-OPEN-059.**

A parked item that turns out to block a task is raised once, and says it was
parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not revert the corrected scale.**
- **Do not set the guard from numbers this session invented**, and do not set it
  at all if no gap separates `012403` from `014854`.
- **Do not run task 3 if task 1 did not implicate the estimator.**
- **Do not edit the reference decoder in the same commit as a decoder change.**
- **Do not trade the silence property**, and do not let
  `ARecordingWithNoStationInItSaysNothing(014854)` go red again.
- **Do not touch the panel.**

## Committing, pushing, reporting

Commit and push each task before starting the next. The report names the branch
and states whether each push succeeded; a refused push is reported as refused,
with the reason.

Report per `CLAUDE_CODE.md` §8 — **read the file's own section count rather than
trusting its version line** — to `output.md` at the repository root, overwritten
and printed. **Section 3 leads with task 4's percentage on `012403` through the
production path.** Section 2 says plainly what Tim will see differently at the
radio this evening, including whether `VA3VRR` and `N4L` are read again, because
he is going to the radio on the strength of it.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eleven inbound, none
ruled, the oldest open since 2026-08-14. Eight consecutive units have now worked
beside rulings they cannot read.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The tone tracker is a large source of soup** — 22 invented against 0 at a
   fixed pitch.
6. **Whether the integrator ships at 45 Hz or 30 Hz.**
7. **The gate's calibration** — this unit re-expresses it if a gap exists.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named and the operator is not told
   they are not named.**
10. **The keying witness is correct in 5 of 13 captures.**
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Unit 1.11.3's, still open: **the lock helping sometimes and hurting sometimes
with nothing telling the operator which**; **the button added against
instruction**; **`ElementsSeen` and `ElementsResolved` being one field**.

Unit 1.11.4's five: **the guard blocking everything** *(task 2)*; **two
adjudicated callsigns lost** *(task 1)*; **percentile estimation failing on
audio with exact zeros** *(task 3, conditionally)*; **the port and its reference
diverged** *(task 5)*; **`CLAUDE_CODE.md` changing its report contract without
moving its version line** — outside this tree, belongs to whoever maintains it.

**If you finish every task, stop and report. Do not start the next unit.**
