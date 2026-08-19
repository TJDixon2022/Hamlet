PROJECT: Hamlet
ISSUED: 2026-08-19

## Asks still outstanding (inbound, per HM-DEC-139)

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening; `BENCH_CARD.md` can be followed end to end |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench, from the send panel |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling |
| **Whether an empty middle class is HM-DEC-142's case** (HM-OPEN-048's remainder) | 2026-08-19 | **Not ruled: phase 1 below measures whether it is a ruling at all** |

---

# Work order — the letter T is coming out as A, and it is costing real copy

**Five phases. Phase 5 is the one to drop.**

Gate first (HM-DEC-099). Write `PROJECT_STATUS.md` now, at every phase boundary,
and at the finish.

The operator reads live CW off the air. **Character accuracy on real off-air
recordings is what decides whether he can follow a contact**, and one substitution
is behind two of the three standing failures.

---

## Phase 1 — Measure before asking for a ruling (minutes, not an evening)

**The `coverage-easy` question was handed back as a ruling and may not be one.**
The report's own words: the classes are named by position, and the content here is
element and character-or-longer.

80 gaps, 61 in the element class, 0 in the character class, 19 above.

**Measure what those 19 gaps actually are**, exactly as phase 3 of the last order
measured the other fixture — the durations, and their ratio to the element class.

- **At roughly three times the element gap they are character gaps**, the three-way
  seeding has put them in the wrong slot, and this is HM-DEC-142's ruled case with
  a mislabelled heap. **That is a defect. Fix it, and do not ask.** The fix is in
  the seeding or the assignment, not in what the transcript asserts.
- **At roughly seven times they are word gaps** and there genuinely are no
  character gaps, which is unusual keying and is a different case from the one
  ruled. **Stop and hand it back with the numbers.**
- **If they are mixed** — some at three, some at seven — say so plainly with the
  distribution, because that is the case where relabelling would place no spaces
  where spaces were sent, which HM-DEC-142 rejected in terms.

Report the table either way. `APassThatReadSomethingEmitsSomething` goes green only
if the first case holds.

## Phase 2 — Name the mechanism behind T becoming A (HM-OPEN-049)

**Do not repair anything in this phase.** This project has twice this week lost an
evening to a diagnosis that named a suspect without naming a line, and both times
the correction came from the operator rather than the code.

`STAAION` for `STATION`. `AHIS` for `THIS`. `■ DE W1AW K` for `CQ DE W1AW K`. Every
one is a lone dah acquiring a leading dit, or a character boundary landing inside
one.

- **Open the audio.** `cw-2026-08-18-003126` and whatever backs
  `TheBulletinDecodesToItsAnswerKey` and
  `ClearingTheTranscriptLeavesTheDecoderAlone`. Find the T that became an A and
  look at the samples around it.
- **Which is it**: a mark being split into two by a dropout, or a character
  boundary missed so a preceding dit joins the dah? Those are different faults with
  different repairs and the waveform says which.
- **Check the de-glitch first.** The reference de-glitches at 20 ms and again at
  0.4·dit. A short mark surviving the first pass and a real mark being cut by the
  second are both consistent with what is on screen. State which threshold, at
  which stage, on which sample.
- HM-DEC-112 took element edges at half amplitude for the clock fit. Whether
  element *extraction* uses the same rule, and what happens at a dah's leading
  edge if it does not, is worth the look.

**The output of this phase is a mechanism and a line number**, not a repair.

## Phase 3 — Repair it, and say what it bought

With the mechanism named. Report character accuracy before and after on every
fixture that moves, and on every one that does not.

**`TheBulletinDecodesToItsAnswerKey` is the honest yardstick**: 36 characters
against 47 on the day it was written and 36 today. Any number you report against
it is a number nobody has moved in three days.

## Phase 4 — HM-DEC-115's premise

**Not a repair. A correction to the record, and it is small.**

HM-DEC-115 states that the same bulletin audio read every character correctly
after acquisition. The test written the next day showed 36 of 47, and today shows
36 of 47. **That measurement was never reproduced.**

The ruling itself may still be right — no cuts means no transcript rather than a
guessed one stands on its own reasoning. What is wrong is a fact cited inside it.
Record that in `OPEN_ISSUES.md` against HM-DEC-115 with the numbers, so the next
session reading that ruling does not take the claim at face value. **Do not amend
the ruling's text** and do not treat this as a supersession.

## Phase 5 — `ClearingTheTranscriptLeavesTheDecoderAlone` (DROP THIS ONE IF SHORT)

`■ DE W1AW K` against `CQ DE W1AW K`. The last session judged this the same
element-level fault as the bulletin's, so phase 3 may take it. Check before doing
separate work, and if phase 3 fixed it say so rather than claiming a phase.

**Drop this whole if short and say you dropped it.**

## Named and left (§12.6)

The four unruled asks above. **No transmit work toward auto-CQ** — HM-DEC-098 is
unruled and dummy-load only.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106), section four carrying the asks queue.

**Section two opens with the accuracy number**, before and after, on the bulletin.
He is going to trust the transcript or not based on that one figure, and everything
else in the section is context for it.

**If you finish every phase, stop and report.**
