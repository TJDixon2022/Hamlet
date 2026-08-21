# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwGate.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

These four files are fixed. Do not substitute a different file for any of
them and do not report a check against a file this list does not name.

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**Tim is at the radio in about an hour. This unit is for tonight.**

Every failure of the last three days runs back to one number: **the fitted dit is
wrong, and everything downstream collapses from it.** The dah's ratio leaves the
band, coherence pins at nought, the opening of the message is lost. Two suspects
were eliminated today — the de-glitch makes it worse, and the analysis window is
narrower where the error is worse — and **the heavy fist now has no named mechanism
left.** That could take days.

**But Tim knows the speed by ear.** He can hear that a man is sending around twenty
words a minute. **It is the one number the machine keeps getting wrong and the one
number the operator can supply for free.**

**Tim's ruling: give him a speed control and seed the estimator from it.** Break the
circle from outside rather than from within.

*Rejected: waiting for the short-mark mechanism to be found. It is the right
long-term work and it has no candidate; an evening at the rig is worth more than a
sixth theory. Also rejected: correcting the estimator by a constant for the gate's
short reading, which was rejected this morning for the same reason it would be
rejected now — it leaves the gate reporting a length the audio does not contain.*

**This does not replace the estimator and does not make it right.** It hands it a
starting point. The estimator still fits, still tracks, still owns the answer.

---

## Task 2 is separate and understood

`Refine` averages every gap under **twice the mark-derived dit**. On
`farnsworth-light` that window is 200 ms, and this sender's **150 ms character
gaps fall inside it** alongside its 73 ms element gaps, costing seven and a half
milliseconds. **The gap classes are already fitted a few lines away.**

That is one line, the mechanism is understood, and it puts a second fixture inside
five per cent. **It is in this unit because it is cheap and independent, not
because it is related.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`,
  `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`.
  **2,117 tests, five failing. Anything above five is new.**
- `Refine` is the method at `CwTiming.cs:1151`, called at 649, **and it is in the
  tree.** What was withdrawn four times is its removal. This unit does not remove
  it.

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.** *He needs this on the ham
machine tonight.*

**HM-DEC-144 — `N4L`, dit 56.3 ms, ratio 4.24.**
**HM-DEC-145 — `VA3VRR`, dit 100.4 ms, ratio 2.73.**
**HM-DEC-146 — the gate reads short marks short; HM-DEC-119 holds at 100 ms and
fails at 56.**

**HM-DEC-114 — the easy tier passes or fails.**

**HM-DEC-048 — nothing raises a confidence score.** *A seeded speed may not make
the decoder emit a character it has not resolved. It changes where the estimate
starts, not what counts as resolved.*

**§0.0 — the display asserts only what is known.** *If the operator's figure is
being used, the terminal says so.*

**HM-OPEN-054 stays open. No transition-shape test, no gate in front of emission.**
**The keying meter is not read by the decoder.**
**`ShortestVote` stays at 5, `MaximumRatio` at 3.8, `MinimumSeparation` and the
five-dit bound stay put.**

**HM-DEC-093 — no radio on the dev machine.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Prove it works before building any of it. **CHANGE NOTHING.**

Hand the estimator the true dit as its starting point and run the four:

| audio | true dit | today | seeded |
|---|---|---|---|
| `farnsworth-heavy` | 56 ms | 3 of 12 | ? |
| `farnsworth-light` | 100 ms | 9 of 12 | ? |
| `cw-2026-08-17-134712` (`N4L`) | 56.3 ms | 0 | ? |
| `cw-2026-08-17-013347` (`VA3VRR`) | 100.4 ms | 8 | ? |

**If `farnsworth-heavy` does not improve, stop and report.** That would mean the
dit is not the cause, three days of reasoning is wrong, and **that is worth more
than any fix.** Say it plainly if so.

Also report what the estimator does with the seed afterwards: does it hold near it,
drift back to the short value, or wander.

---

## Task 2 — The light fist's window. One line.

Confine `Refine`'s averaging to gaps that are element gaps, using the gap classes
already fitted nearby rather than a multiple of the dit.

- **Fitted, not a constant.**
- Required: `farnsworth-light`'s fitted dit **within 5% of 100 ms**, its count
  **≥ 9 of 12**, and **every other fixture and the easy tier whole**.

---

## Task 3 — The control, only if task 1 showed a gain

On the CW terminal, near the transcript: a speed the operator can set in words per
minute, **off by default**.

- **Off by default and clearly off.** *Nothing about tonight may change what he
  sees until he asks for it.*
- When set, it seeds the estimator's starting dit. **The estimator still fits and
  still owns the answer** — this moves where it starts, not what it may conclude.
- **The terminal says when the operator's figure is in use** (§0.0). He must never
  be looking at a transcript without knowing which of the two produced it.
- Coarse is fine. Whole words per minute, a sensible range, and easy to change with
  one hand while the other is on the dial. *He will be adjusting this while a
  station is sending.*
- **The sidecar and the roster record the seed if one was set**, so tomorrow's rows
  say whether he was helping.

---

## Task 4 — Everything else holds

| | required |
|---|---|
| `cw-2026-08-20-014854` | **0** |
| `cw-2026-08-20-014935` | **0** |
| `004507` | ≥ 25 |
| `003016` | ≥ 38 |
| `003126` | ≥ 35 |
| `003758` | ≥ 14 |
| `013347` | ≥ 8, **and `VA3VRR` still readable** |
| the easy tier and every other fixture | **whole** |

**With no seed set, every number above must be identical to today.** *The control
is additive. If anything moves with it off, that is a defect and it does not ship.*

---

## Task 5 — What to do at the rig. **DROP CANDIDATE, but valuable.**

Three sentences in section 2 telling Tim how to use it tonight: what to set, what
he should see when the figure is right, and what he should see when it is wrong.

*He will be operating alone in a short window with no other instrument but the
keying meter.*

---

## Parked — do not touch, do not raise

- **What shortens a short mark.** *The de-glitch and the analysis window are both
  eliminated. Do not re-test them and do not offer a third theory in this unit.*
- **`Refine`'s removal**, a transition-shape test, any gate in front of emission.
- **Character structure**, the keying meter as something the decoder reads.
- **The bulletin's standing red.**
- **Why the 19th's stations are missing from the audio.**
- **The 69 and 233.**
- **Adjudicating by ear.** Tim's.
- **HM-OPEN-052**, rulings 096–133, the scorer, `CaptureAudioAsync` end to end.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

- **Do not change anything the operator sees when the control is off.**
- **Do not let the seed force an emission.** HM-DEC-048.
- **Do not re-cut a fixture.**
- **Do not spend the session on task 5.** *Tasks 1 and 3 are what he needs.*
- **Do not report a third theory of the short mark.** *Two were eliminated today.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139.

**Section 1 opens with task 1's table**, because if seeding the dit does not help,
nothing else in this unit matters.

**Section 2 opens with how to use it at the rig tonight**, and says plainly whether
a callsign at the front of a call survives with the speed set.

**Stop and report. He is going to the radio.**
