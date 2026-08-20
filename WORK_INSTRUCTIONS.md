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

**The dit reads short, and everything else this week follows from it.**

Last session named the circle: on `farnsworth-heavy` the dit reads 44–47 ms against
a true 56, so the fitted dah comes out at 5.4 dits, past the bound
`MeasureCoherence` will fit to. It falls back to the textbook three, every dah
scores 2.4 dits of error, **coherence is pinned at nought for the whole recording**,
and the estimate never settles because coherence never rises. `N4L` is worse: a dit
of 31.3 puts its dah at 7.6.

Three threads run back to the same short dit:

- **The heavy fist loses nine characters of a fourteen-character call**, so a
  callsign at the front does not survive. That is the phase goal.
- **The tone settles at 575 Hz on a fixture generated at 615**, because
  `MaximumRatio` is 3.8, the fist sends 4.25, nothing is ever confirmed and the
  tracker follows loudness.
- **`cw-2026-08-17-134712` reads nothing at all.**

**Tim's ruling: break the circle at the dit.** Fix why the estimate is short and
the dah returns to 4.25, inside the band where it was always meant to sit.

*Rejected: relaxing the five-dit bound so the fitted dah survives. It was measured
as the point past which a long mark is a carrier, a fade or a key held down, and
moving it to admit a fist is the error class six rulings have gone on closing — and
it sits next to parked ground.*

**The measurement comes first and is task 1 of this unit rather than its own
session.** `farnsworth-heavy` is generated, noise-free, and its true dit is known to
the millisecond, so whatever drags 56 to 44 is fully observable.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`,
  and `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`, new last session and a
  real defect. **2,117 tests, five failing. Anything above five is new.**
- `farnsworth-heavy` is admissible and scores 100% against the reference.
  `NotYetAdmissible` is empty. **`Refine` is not in the tree and is not to be
  revived.**

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-144 — `N4L`, dit 56.3 ms, dah 238.3, element gap 35.6, ratio 4.24.**

**HM-DEC-145 — `VA3VRR`, dit 100.4 ms, dah 274.3, element gap 73.3, ratio 2.73.**

**HM-DEC-119 — the gate reads 100–110 ms for a true 100 at every speed.** *A mark
is long by nought to ten per cent, not short. Whatever drags the estimate down is
not the gate mismeasuring a mark.*

**HM-DEC-115 — a real fist's element gap is genuinely shorter than its dit.**

**HM-DEC-114 — the easy tier passes or fails.**

**HM-DEC-048 — nothing raises a confidence score.**

**HM-OPEN-054 stays open. No transition-shape test, no gate in front of emission.**

**The keying meter is not read by the decoder.**

**HM-OPEN-053 — `ShortestVote` stays at 5. `MaximumRatio` stays at 3.8.
`MinimumSeparation` and the five-dit bound are not to be moved.**

**HM-DEC-093 — no radio.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Why is the dit short? **CHANGE NOTHING.**

On `farnsworth-heavy`, which is generated at a 56 ms dit, a 36 ms element gap and a
238 ms dah, with no noise in it:

1. **What lengths does the gate actually report?** Every mark and every gap, with
   the true value beside it. HM-DEC-119 says a mark reads long by nought to ten per
   cent — **confirm that on this audio or report that it does not hold.**
2. **What does the estimator receive, and what does it do with it?** Follow one
   number from the gate's marks to the fitted dit and say which step loses the 12
   milliseconds.
3. **Is it one step or several?** Report the dit after each stage of the fit.
4. **Repeat on `farnsworth-light`**, generated at 100 ms and fitting 95. *Five per
   cent against twenty on the same code is a clue about the mechanism, not a
   separate problem.*

**Then say what the mechanism is, in one sentence: a mechanism and a line, not a
suspect.**

**If the light and heavy fists lose it at different steps, stop and report.**

---

## Task 2 — Fix it, only if task 1 named one mechanism

- **Fitted, not a constant.** *Seventh instance of the error class six rulings have
  gone on closing.*
- **Inside the estimator.** No gate, and **do not touch the five-dit bound,
  `MinimumSeparation` or `MaximumRatio`** — if the dit is right, the dah returns to
  the band on its own and none of them needs moving. **If the fix requires moving
  one of them, stop and report instead.** *That would mean the ruling was wrong,
  which is worth knowing.*
- It may make the decoder measure better. **It may not make it emit a character it
  has not resolved** (HM-DEC-048).

| | required |
|---|---|
| **`farnsworth-heavy` fitted dit** | **within 5% of 56 ms** |
| **`farnsworth-light` fitted dit** | **within 5% of 100 ms** |
| **`farnsworth-heavy`** | **> 3 of 12** |
| **`farnsworth-light`** | **> 9 of 12** |
| `cw-2026-08-20-014854` | **0** |
| `cw-2026-08-20-014935` | **0** |
| `004507` | ≥ 25 |
| `003016` | ≥ 38 |
| `003126` | ≥ 35 |
| `003758` | ≥ 14 |
| `013347` | ≥ 8, **and `VA3VRR` still readable** |
| the easy tier and every other fixture | **whole** |

**Report the fitted dit on both adjudicated recordings against 56.3 and 100.4**,
and say whether `134712` now emits anything.

---

## Task 3 — The two threads that should follow

Report, do not work on:

- **`TheToneIsFoundInRealisticAudio(farnsworth-heavy)`** — does the tone still
  settle at 575 against a generated 615? *If the dit is right, the fitted dah
  returns to 4.25, which is still past `MaximumRatio`'s 3.8, so this may not move.
  Say either way.*
- **The light fist's warm-up** — at which mark does `LooksLikeMorse` first go true
  now? It was mark 16 on `farnsworth-light` and mark 12 on `013347`.

---

## Task 4 — What it means on the air

One paragraph, and it is what Tim reads first.

**On `CQ CQ DE <callsign> K`, does the callsign survive on each fist?** Last
session: yes on the light one, no on the heavy one, which lost nine characters.
**Say the number now, on both.**

*This is the phase goal. Every other figure in this project is a character count on
a recording.*

---

## Parked — do not touch, do not raise

- **`Refine`.** Dropped by ruling. Not to be revived, re-measured or proposed.
- **The five-dit bound, `MinimumSeparation`, `MaximumRatio`.** *If the fix needs
  one, stop.*
- **A transition-shape test, or any gate in front of emission.**
- **Character structure**, and the keying meter as something the decoder reads.
- **The bulletin's standing red.** *Report its count if it moves; do not work on
  it.*
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

- **Do not re-cut or soften any fixture.** *Appending to the catalogue shifted the
  seed counter once already and silently re-cut `qsk-preamble`.*
- **Do not work the light fist's warm-up in this unit.** *Two mechanisms, one at a
  time. The heavy fist is the one that loses callsigns.*
- **Do not emit an unresolved character to recover the opening.**
- **Do not tune to one fixture.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139. **The heavy-fist-circle ask leaves the queue; it was
ruled.**

**Section 1 opens with task 1's mechanism.**

**Section 2 opens with task 4** — whether a callsign at the front of a call now
survives on each fist.

**Stop and report.**
