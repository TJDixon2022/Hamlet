# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticStream.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

These four files are fixed. Do not substitute a different file for any of
them and do not report a check against a file this list does not name.

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**The operator hears CW clearly and Hamlet shows him a page of E, T and I.**
This unit exists to change what is on that screen and nothing else.

### The measurement this is built on

`ANALYSIS-cw-2026-08-22-014113.md` measured a real capture independently of
Hamlet's decoder, with its own Goertzel chain, and states its method so it can be
disagreed with. **Read it. It is the specification.** It should arrive with this
order; **if it and `cw-2026-08-22-014113.wav` are absent, say so and work from the
recordings that are present** — but say which figures went unchecked.

Its finding, and the target of this unit:

> **`20 elements seen, 20 resolved` against `13 characters emitted`. That is
> 1.54 elements per character. English Morse averages near 3. Twenty elements
> becoming thirteen characters means the character gap is being called about
> twice as often as it should be — which on its own would turn readable Morse
> into unreadable single-element letters.**

**That is the fault.** `E` is one element. `T` is one. `I` is two. A screen full of
them is what a decoder produces when it breaks between elements that belong to the
same letter. **Every character the operator has failed to read this week is
consistent with it.**

### And the speed points the same way

The analysis measured that sender at **≈ 62 ms, about 19 WPM**, with dah, character
gap and word gap all within a few milliseconds of 3, 3 and 7 units. **Hamlet chose
24 WPM.** The hypothesis grid runs 8 to 32 **in steps of two**, so **19 is not on
it.** A unit a quarter short promotes ordinary inter-element gaps into character
gaps, systematically, everywhere.

**Two observations, one fault seen twice.** The unit is a quarter short and the
gaps break too often — and the second may be entirely a consequence of the first.

---

## What this unit is for

**One number decides whether it worked: elements per character.**

Not a character count, not a sweep percentage, not a test tally. **Near three or it
has not worked.** Report it first, report it last, report it for every station
recording in the tree.

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- Every figure above came from outside this repository. **Reproduce what you rely
  on.**
- **The failing set is 28.** Record it exactly before and after and name every
  difference.
- **Report on the sweep AND every real recording together, every time.** A change
  that looked perfect on the sensitivity fixture once silenced all six recordings.

---

## Rulings in force

- **HM-DEC-120.** Nothing emitted on audio holding no signal. **This is the one
  property that does not bend, and it has not bent all week.**
- **HM-DEC-048** and **HM-DEC-108**, on confidence. Nothing raises a score; a
  doubtful fit lowers it.
- **HM-DEC-009** and **§0.0.**
- **HM-DEC-091.**
- **HM-DEC-096** phase 3, the mid-character interlock. **Untouched.**
- **HM-DEC-150**, the version scheme. Task 6.
- **HM-DEC-093** and `SHACK_FACTS.md` — no radio on the development machine.
- **§12.5** — no answer key for a recording nobody has adjudicated.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md`
**§13**, which names that file's fields — `STATE`, `PHASE`, `BALL`, `NEXT_PASTE`,
`UPDATED`, `NOTE`. `UPDATED` from the clock; `NOTE` says what is moving inside the
task. Also every ten minutes while a task runs.

---

## Task 1 — Measure elements per character, everywhere

**Report before changing anything. No code changes in this task.**

For **every** station recording in `tests\fixtures\cw\captured\` and
`\unadjudicated\`, and for the sensitivity fixture at 18, 15 and 12 decibels:

**elements resolved, characters emitted, and elements per character.**

Then say which are near three and which are not. **This table is the baseline the
whole unit is measured against and it must exist before anything is built.**

---

## Task 2 — Where a character gap is decided

**Report before changing anything.**

In the segmental Viterbi:

1. **What decides between an inter-element gap and a character gap?** Name the
   scoring, the durations each expects, the penalty on straying, and the file and
   line.
2. **What does a hand-sent fist actually give?** The analysis measured this
   sender's gaps clustering at **50 ms, 120–180 ms, and 410/495 ms** against a
   62 ms unit — so roughly 0.8, 2–3 and 6.6–8 units. **Compare that with what the
   model expects.** Say where the model and the operator disagree.
3. **Run the decoder at the measured 19 WPM** — impose the unit rather than letting
   the grid choose — and report elements per character. **This is the question the
   unit turns on**: if imposing the right speed brings it to three, the gap model
   is fine and the grid is the fault. If it does not, the gap model is the fault.

**Say which it is. If it is both, say that.**

---

## Task 3 — Put the right speeds on the grid

Gated on task 2 only if task 2 finds the grid innocent — **otherwise do this
anyway, because 19 WPM not being on the grid is a defect on its own.**

The grid runs 8 to 32 in steps of two. **Ordinary operators send at 13, 15, 17,
19, 21.** Half of the common speeds are unreachable.

- **Make the step fine enough to reach them.** One word a minute, or finer near the
  slow end where a step is a larger fraction of the unit.
- **Report the cost per second of audio.** The last measurement had the whole
  twelve-hypothesis search at 7.4 to 8.4 per cent of real time, so there is room —
  **but measure it, do not assume it.** If a finer grid will not keep up, say what
  you chose and what it costs.
- **Report elements per character on every recording afterwards**, against task 1's
  table.

---

## Task 4 — Fix the gap model, if task 2 says it is the fault

Gated on task 2.

- **A hand-sent fist compresses character gaps and stretches nothing.** The
  evening of the 19th measured the key-up distribution as smeared with no usable
  3-unit or 7-unit structure. **The model must fit what operators send, not the
  textbook.**
- **A gap that could be either is a doubtful call and lowers confidence**
  (HM-DEC-048, HM-DEC-108). **It does not get resolved by preferring the shorter
  reading.**
- **Do not clamp, do not tune a constant to make one recording read better.** If
  the change cannot be stated as a property of how people send Morse, it is the
  wrong change.

---

## Task 5 — Prove it on everything, together

**The table from task 1, recomputed.** Elements per character, every recording,
before and after, side by side.

Then:

- **All station recordings, quoted verbatim, before and after.** The operator reads
  these strings; they are the point.
- **Both recordings holding no keying: silent, offline and streamed.**
- **The sweep, every level, right and invented.** **If anything is invented above
  twelve decibels where nothing was, stop and report.** HM-DEC-120 is not traded
  for this.
- **The failing set, exactly, every survivor named.**

**If elements per character has not moved toward three, say so plainly in the
first line of the report rather than reporting a character count instead.**

---

## Task 6 — Bump the version

Read the current version from `Directory.Build.props`, bump the patch, report what
it moved from and to. **HM-DEC-150.** One work unit, one patch.

---

## Parked — do not touch, do not raise

- **The window clear.** Off by ruling, machinery kept, does nothing.
- **How Hamlet decides a different person is sending.** Tim's, and the pitch-
  distance approach was measured dead.
- **The survey ranking admitted bins by loudness**, which sits against HM-DEC-095.
  Real, named, not this unit.
- **The advice line asserting a cause the app can disprove.**
- **The sidecar asserting `13 emitted` beside `text nothing read`.**
- **`014113` becoming a fixture.** **No transcript is ever asserted for it**, and
  **do not build a validity scorer** — one was built during the analysis, reached
  thirty valid Morse characters out of thirty, and returned
  `ETTT TOGATMETTEMTTEEEATEEEMN`.
- **`FollowSpeed` has no supplier**; the reacquiring guard; `HM-OPEN-051`; the
  mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- **The twenty-eight failing tests**, except any this unit moves.
- **HM-OPEN-012, HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098,
  HM-OPEN-033, HM-OPEN-007.**

---

## Asks still outstanding

Carried inbound per HM-DEC-139, verbatim until ruled. **Verify against
`OPEN_ISSUES.md` and report anything here that is closed, or open and missing.**

- Whether a sender change can be decided by pitch distance at all — measured dead.
- Whether the window clear comes back on.
- The advice line asserting a cause the app can disprove.
- The sidecar asserting two incompatible things about one span.
- Whether the sidecar's `text` should include the leading edge.
- `cw-2026-08-22-014113.wav` and its analysis are not in the tree.
- The captures from the evenings of the 20th and 21st are not in the tree.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0.
- The keying meter's provisional thresholds.
- `FollowSpeed` has no supplier.
- The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not report a character count in place of elements per character.** *A
  decoder emitting more single-element letters emits more characters and reads
  worse.*
- **Do not trade HM-DEC-120 for any of this.**
- **Do not tune a constant to make one recording read better.** *State the change
  as a property of how people send Morse or do not make it.*
- **Do not touch the mid-character interlock, the tracker, the survey or the
  keying meter.** *None of them is the fault here.*
- **Do not assert a transcript for any capture, and do not build a validity
  scorer.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **§12.2 names the four
headings** — **What Claude did**, **What Tim should expect**, **What we should do
next**, **What's blocking us** — the last carrying **Asks still outstanding** per
HM-DEC-139. No other headings.

**Section 1 opens with elements per character on every station recording, before
and after, as a table.**

**Section 2 quotes what each recording reads now against what it read before, and
says in one sentence whether the operator will see more CW.**

**Stop and report.**
