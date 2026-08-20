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

**Hamlet loses the front of every message and keeps the end, and on the air the
front is the callsign.**

Last session produced the first Farnsworth failure with a written-down answer:

| audio | key | Hamlet reads | of 12 |
|---|---|---|---|
| `farnsworth-light` | `CQ DE N0CALL K` | `DE N0CALL K` | 9 |
| `farnsworth-heavy` | `CQ DE N0CALL K` | `AL K` | 3 |

**Nothing is invented in either.** Every character produced is correct and in the
right place. What is missing is the opening — `CQ ` on the lighter fist and
`CQ DE N0C` on the heavier one. The same shape appears on both adjudicated
recordings, and the fitted dit is short in all four: 95.0 against 100 on the light
fixture, 47.0 against 56 on the heavy, 87.0 against 100.4 on `VA3VRR`, 31.3 against
56.3 on `N4L`.

**This is a warm-up problem, not an accuracy problem**, and it is the one that costs
Tim contacts. A call is short: `CQ CQ DE N4L K` is fourteen characters, and losing
the first nine loses the callsign, which is the only part that matters. It is also
the likeliest reason `cw-2026-08-17-134712` reads nothing at all — the fist is about
six seconds inside a thirty-second recording, so the clock never gets its warm-up
before the sender stops.

**The numbers to move: 9 of 12 and 3 of 12.**

---

## Rulings carried in, both Tim's, this session

**The reference decoder is to be fixed to read a 4.25-dit fist.** It scores
`farnsworth-heavy` at 0% saying `read nothing (do not cluster as Morse)`, because it
expects a dah near three dits. That fixture's timing is adjudicated to the
millisecond from HM-DEC-144, read out of the gate's own elements. **A judge that
cannot read a fist the radio has heard is not independent, it is wrong**, and
HM-DEC-101 records that one earlier entry was cleared exactly this way.

*Rejected: softening the fixture toward three dits, which would be generating a
fist nobody has measured in order to pass a check. Also rejected: admitting the
fixture without fixing the reference, because HM-DEC-101's gate exists to stop a
session deciding its own fixture is good enough.*

**The reference fix is task 1 and is bookkeeping.** *Do not let it consume the
session. The rest of this unit is the point.*

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.
  **2,117 tests, four failing. Anything above four is new.**
- `farnsworth-light` and `farnsworth-heavy` shipped last session;
  `farnsworth-heavy` is in `NotYetAdmissible`. **`Refine` is not in the tree and is
  not to be revived.**

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-144 — `N4L`, dit 56.3 ms, dah 238.3, element gap 35.6, character gap 165,
ratio 4.24.**

**HM-DEC-145 — `VA3VRR`, dit 100.4 ms, dah 274.3, element gap 73.3, character gap
150, ratio 2.73.**

**HM-DEC-114 — the easy tier passes or fails.**

**HM-DEC-101 — a fixture the reference cannot read is a bad fixture.**

**HM-DEC-090 — marking is not a substitute for silence.**

**HM-DEC-048 — nothing raises a confidence score.** *A decoder that emits the
opening by guessing at it has not solved this.*

**HM-OPEN-054 stays open. No transition-shape test, no gate in front of emission.**

**The keying meter is not read by the decoder.**

**HM-OPEN-053 — `ShortestVote` stays at 5. `MaximumRatio` stays at 3.8.
`MinimumSeparation` is not to be moved.**

**HM-DEC-093 — no radio.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — The reference. Bookkeeping, done quickly.

Fix the reference so it reads a 4.25-dit fist, re-score `farnsworth-heavy`, and
admit it if it passes.

- **Fitted, not a widened constant** where the reference allows it. *Seventh
  instance of the error class six rulings have gone on closing.*
- Report the score before and after, and confirm no other fixture's score moved.
- **If this cannot be done in a small change, stop and report** rather than
  rebuilding the reference. `farnsworth-light` alone is enough to work against.

---

## Task 2 — Why the opening is lost. **CHANGE NOTHING.**

The measurement this unit turns on.

On `farnsworth-light`, `farnsworth-heavy` and both adjudicated recordings, report
**character by character from the first mark**:

1. **What the fitted dit is at each character**, against the true dit — so the
   warm-up curve is visible rather than inferred.
2. **At which character does the estimate first come within five per cent of
   truth**, and how many characters were emitted before that point.
3. **Which check rejects each lost character** — coherence, the separation test,
   `LooksLikeMorse`, or something else. *Name the line for each one.*
4. **What is in the window at the first character**, and how it differs from what
   is there at the tenth.

**Then say what the mechanism is, in one sentence.** *Not a suspect. A mechanism
and a line.*

**If the four differ in mechanism, stop and report.** Four openings lost four ways
is a different unit.

---

## Task 3 — Fix it, only if task 2 named one mechanism

- **Fitted, not a constant.**
- **Inside the estimator.** No gate.
- It may make the decoder measure sooner. **It may not make it emit a character it
  has not resolved** (HM-DEC-048).

| | required |
|---|---|
| **`farnsworth-light`** | **> 9 of 12** |
| **`farnsworth-heavy`** | **> 3 of 12** |
| `cw-2026-08-20-014854` | **0** |
| `cw-2026-08-20-014935` | **0** |
| `004507` | ≥ 25 |
| `003016` | ≥ 38 |
| `003126` | ≥ 35 |
| `003758` | ≥ 14 |
| `013347` | ≥ 8, **and `VA3VRR` still readable** |
| the easy tier | **whole** |
| every other fixture | **whole** |

**Report the fitted dit for all four against their true figures**, and say whether
`134712` now emits anything.

**A change that lifts the fixtures and drops a real recording is not a fix.**

---

## Task 4 — What it means on the air

One paragraph, and it is what Tim reads first.

**On a fourteen-character call, how many characters does Hamlet now need before it
is reading?** Say it as a number, on both fists, and say what that means for
someone calling `CQ CQ DE <callsign> K` — whether the callsign now survives.

*Every other measure in this project is a character count on a recording. This one
is the phase goal.*

---

## Parked — do not touch, do not raise

- **`Refine`.** Dropped by ruling. Not to be revived, re-measured or proposed.
- **A transition-shape test, or any gate in front of emission.**
- **Character structure**, and the keying meter as something the decoder reads.
- **`MaximumRatio`**, `MinimumSeparation`, the three-way length fit.
- **The bulletin's standing red.** HM-DEC-114 left it deliberately. *Report its
  count if it moves; do not work on it.*
- **Why the 19th's stations are missing from the audio.**
- **The 69 and 233.**
- **Adjudicating by ear.** Tim's.
- **HM-OPEN-052**, rulings 096–133, the scorer, `CaptureAudioAsync` end to end,
  `TheRosterIsOneFilePerEvening`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

Unit-specific:

- **Do not re-cut or soften any fixture.** *Their timings are adjudicated and their
  bytes are load-bearing. Appending to the catalogue shifted the seed counter once
  already and silently re-cut `qsk-preamble`.*
- **Do not spend the session on the reference.** *Task 1 is bookkeeping.*
- **Do not emit an unresolved character to recover the opening.** HM-DEC-048.
- **Do not tune to one fixture.** *Two generated fists, two adjudicated recordings
  and the whole suite are the guards, and they disagree enough to be useful.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139. **The reference ask leaves the queue; it was ruled.**

**Section 1 opens with task 3's table**, or with task 2's mechanism if nothing
shipped.

**Section 2 opens with task 4** — whether a callsign at the front of a call now
survives.

**Stop and report.**
