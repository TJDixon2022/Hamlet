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

# Work instruction 009 — find the pitch, and hold it, without being asked

**This unit has a deadline.** Tim is at the radio this evening. Unit 1.11.5
reached **84.2 %** on `cw-2026-08-24-012403` — but that capture is a file on
disk, decoded with the pitch effectively settled. **Tonight the pitch is chosen
in real time by the tracker, and the tracker is the largest measured source of
wrong characters in this project: 22 invented against 0 when the pitch is held
still.** This unit closes that gap.

## Why this unit exists

**The unit's number: two right out of fourteen.**

An independent shack-side chain measured Hamlet's reported tone against fourteen
captures and found it exactly right **twice**, one of those a synthetic file. The
W1AW carrier held 499.9 Hz ±0.1 for four minutes and was reported 495, 300, 500,
475, 475, 475, 475. On `012403` the two halves of Hamlet **bracket a station at
439.81 Hz from opposite sides — `toneHz` says 450, the sweep bin says 425** — and
nothing lands on it.

A third independent chain has since decoded all fourteen captures from the WAVs.
It confirms the tones, and it confirms that two captures nothing has ever read —
`cw-2026-08-22-014113` and `cw-2026-08-22-014308`, both at **606 Hz with 19 dB of
keying** — remain unread by every chain. **A pitch error of the size Hamlet
routinely makes is sufficient to explain a strong signal reading nothing**, and
that is what will happen tonight to a station Tim can hear.

**What is measured and not in dispute:**

- Held still, the decoder reads. Moving costs 22 characters where holding costs 0.
- `012403` scores 13.94 at 439.81 Hz, 12.44 at 450, and 10.36 at the radio's own
  `CwPitch` of 600. **The radio's CW pitch and the station's pitch are unrelated**
  — confirmed in-tree by unit 1.11.4. Nothing may lock to `CwPitch`.
- Where the survey admits nothing, `CwToneTracker` was reported by an earlier
  review to fall back to the middle of the fine bank at three sites —
  `CwToneTracker.cs:356`, `:842`, `:1002`. **That is a number nobody keyed at,
  and the decoder mixes down at it.**

**The lock exists and is not enough.** Unit 1.11.3 added it and it must be
pressed, and it takes whatever pitch the tracker held at the moment of pressing.
A lock fed by a broken estimate locks onto the error.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway. Unit 1.11.5 corrected its own
predecessor's report on `VA3VRR`; do the same wherever this instruction is wrong.

**The three fallback line numbers above come from a 1.10.13 archive and the file
has changed since.** Find them; do not trust the numbers.

**Known red: 34 failing of 1605 in the engine, 481 of 481 in the app.** Two are a
known accepted cost — `EveryRecordingGivesBackTheShareItShould` on `clean-12wpm`
and `clean-18wpm`, which contain exact digital silence that HM-OPEN-018 records
as physically impossible. One,
`ABroadcastDoesNotAnswerTheCommandInFlight`, is flaky in both directions. **Do
not fix any of these.**

**`ARecordingWithNoStationInItSaysNothing(014854)` is green and must stay green.**

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141 or 150.**
**HM-DEC-095 (the confirmation rule) and HM-DEC-127 (the displacement floor) both
govern this unit's subsystem and both are inside that unreadable range.**
Transcribe `CLAUDE.md`'s index rows for both into the report before task 3, and
**treat anything they forbid as forbidden.** This is the largest risk in this
unit.

**`CLAUDE_CODE.md` changed from five report sections to four without moving its
version line.** Read the file's own section count.

## Rulings in force

**HM-DEC-120.** `CLAUDE.md`'s index row reads *"The refusal floor is 14 in the
decoder's own margin units, superseding the 17 of HM-DEC-117's interim."* **The
property is that nothing is emitted on audio holding no signal.** `Gate` now
stands at **1.40**, re-expressed by unit 1.11.5 in the corrected units, in a gap
running 0.840 to 1.684. **That gap is narrow and this unit must not consume it.**
Both empty captures emit nothing, checked and stated at every task that touches
the signal path.

**Tim's ruling on the character margin, standing:** the margin is nought.
Not changed here.

**Rejected already, do not revisit:**

- **Locking to the radio's `CwPitch`.** Measured wrong: 10.36 against 13.94.
- **Widening the guard.** Above 1.684 refuses `012403`.
- **Reverting the corrected scale.**
- **Quantising pitch to a bin.** The 25 Hz grid is what this unit removes.

**PROPOSAL, not ruled — §4.4.** The "Hold this pitch" button from unit 1.11.3
remains unruled. **Task 4 may change what pitch the lock receives; it must not
change the button, its label, or anything else on the panel.**

## Status cadence

Named here as well as in the prompt, per §4.5. After each task, before starting
the next, update `PROJECT_STATUS.md` per `CLAUDE.md` — `STATE`, `TASK: n of m`,
`BALL`, `UPDATED` read from the clock, and `NOTE` saying what is moving inside
the task. The same every ten minutes while a task runs. **This unit is against a
clock; if a task overruns, say so in the note.**

## The tasks

### Task 1 — measure the pitch error, and what it costs

Before changing anything, over all fourteen captures: **what pitch does Hamlet
report, what pitch is actually there, and what does the difference cost in
characters?**

Take the true pitch by full-length interpolated transform peak, measured in this
task and reported. For each capture report Hamlet's reported tone, the measured
tone, the error in hertz, and the decode at each.

**Then answer directly: do `cw-2026-08-22-014113` and `cw-2026-08-22-014308` read
when mixed down at their measured 606 Hz?** Three independent chains have failed
on those two files. **If they read, pitch was the whole fault and this unit
recovers two captures. If they do not, say so — there is a second mechanism and
Tim needs to know it exists before this evening**, and tasks 2 to 4 still stand
on the rest of the corpus.

Build and run the suite; record counts as the green baseline.

### Task 2 — find the pitch to better than a bin

Replace the coarse pitch estimate with a full-length transform peak interpolated
between bins, so the reported pitch resolves to a fraction of a hertz. **Remove
the 25 Hz grid wherever task 1 found it.**

**Both reporters are corrected together** — the decoder's `toneHz` and the sweep's
bin. They currently disagree by 25 Hz on the same file and neither is right. After
this task the sidecar carries **one** measured pitch and records where it came
from.

Report the tone table again: measured against reported, all fourteen captures,
against the two-in-fourteen this unit opened with.

### Task 3 — never report a pitch nobody keyed at

Where the survey admits no candidate, the tracker must not return a fine-bank
centre, and **the decoder must not mix down at one**. Reporting an unmeasured
number as a measurement is what §0.0 and HM-DEC-009 forbid.

Where no pitch has been measured, the sidecar says so and the decoder reads
nothing rather than reading noise at an invented frequency.

**This task is scoped to the case where nothing was admitted at all. It does not
change what happens when a candidate was admitted** — those are HM-DEC-095's and
HM-DEC-127's rules and they are not touched.

Re-run the corpus and both empty captures.

### Task 4 — hold a pitch once it is found

A pitch that has been measured and confirmed is held while the station keys, and
released when it stops — **without the operator pressing anything.**

The existing lock keeps working exactly as it does. What changes is that it no
longer has to be the only thing standing between the decoder and a wandering
tracker, and that whatever pitch it receives is a measured one from task 2.

**Report what "while the station keys" was implemented as, and what released it**,
because that choice is the whole task and a later unit will need to see it.

Re-run the corpus, the sensitivity sweep, and both empty captures. **Report the
per-capture character counts against unit 1.11.5's**, so a regression anywhere is
visible rather than averaged away.

### Task 5 — does it still read 84.2 % *(the drop candidate)*

Through the production path, guard in place, decode `cw-2026-08-24-012403`
against `CQ CQ CQ DE KD0UN KD0UN K` and report the percentage, beside unit
1.11.5's 84.2 %. Same for `cw-2026-08-18-004507` against its 91.3 %.

**A number lower than 84.2 % is a finding, not a failure to hide.** Report it.

**This is the drop candidate. Dropped whole, and the report says it was dropped.**

## Parked — do not touch, do not raise

- **The tracker's confirmation rule and displacement floor** (HM-DEC-095,
  HM-DEC-127), unreadable in this tree. Task 3 is scoped away from them.
- **The panel, and the "Hold this pitch" button.** Unruled, Tim's.
- **`Gate` at 1.40 and the character margin at nought.**
- **The `Skip()` splice wall, `ClearOnAStationChange`, `Restart()`.**
- **Adjudicating the W1AW seven** — worth doing, not this unit.
- **`cw-2026-08-23-001520` scoring in the quadrillions.** Tim's own transmission.
- **The reference decoder's boxcar against the port's Hann integrator.**
- **`ElementsSeen` and `ElementsResolved` being one field.**
- **`CwUnitEstimator.Runs`** — if anything here moves the measured unit, report it
  and leave the estimator alone.
- **The keying sweep's 5-of-13 verdicts**, **HM-OPEN-057**, **HM-OPEN-058**,
  **HM-OPEN-059**.

A parked item that turns out to block a task is raised once, and says it was
parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not change the tracker's rules for moving between admitted candidates.**
  Task 3 covers only the case where nothing was admitted.
- **Do not lock to `CwPitch`.** Measured wrong.
- **Do not consume the guard's gap.** It runs 0.840 to 1.684 and `Gate` is 1.40;
  if any task moves those window ratios, report the new edges rather than
  adjusting the guard.
- **Do not touch the panel.**
- **Do not trade the silence property**, and do not let
  `ARecordingWithNoStationInItSaysNothing(014854)` go red.

## Committing, pushing, reporting

Commit and push each task before starting the next. The report names the branch
and states whether each push succeeded; a refused push is reported as refused,
with the reason.

Report per `CLAUDE_CODE.md` §8 — **read the file's own section count rather than
trusting its version line** — to `output.md` at the repository root, overwritten
and printed.

**Section 3 leads with two numbers: how many of the fourteen captures Hamlet now
reports the correct pitch for, against two before; and whether `014113` and
`014308` read.** Section 2 says plainly what Tim will see differently at the
radio this evening, because he is going to the radio on the strength of it.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eleven inbound, none
ruled, the oldest open since 2026-08-14. Ten consecutive units have now worked
beside rulings they cannot read.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The tone tracker is a large source of soup** — *this unit's subject.*
6. **Whether the integrator ships at 45 Hz or 30 Hz.**
7. **The guard's gap is two to one**, 0.840 against 1.684, calibrated on two empty
   captures. More recordings of a genuinely empty band would settle it.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named and the operator is not told
   they are not named.**
10. **The keying witness is correct in 5 of 13 captures.**
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Still open from earlier units: **the lock helping sometimes and hurting sometimes
with nothing telling the operator which**; **the button added against
instruction**; **two clean fixtures dropped from 9 of 9 because they contain
exact digital silence**; **`001520` scoring in the quadrillions**; **the port and
its reference differing by an integrator**; **`CLAUDE_CODE.md` changing its
report contract without moving its version line.**

New, from the independent decode of all fourteen WAVs: **the seven W1AW captures
are ARRL Propagation Forecast Bulletin ARLP034, machine-keyed at 17–19 WPM on a
499.9 Hz carrier, and their text is confirmed by overlap across consecutive
captures — `031948` ends on `MEAN OF 117` and `032012` opens on the same words,
and `032129` carries `FORECAST BULLETIN ARLP034` in the audio itself. Seven
adjudicable fixtures are available and none is adjudicated.**

**If you finish every task, stop and report. Do not start the next unit.**
