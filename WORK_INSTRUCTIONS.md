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

**This unit measures one hypothesis and ships nothing.** If it ends with a table
and no code in `src/`, it has succeeded. The last three units that shipped
something were each preceded by a measurement-only session; the two that guessed
came back empty.

Last session killed the transition clock and, in killing it, found something better
than the candidate it was testing:

> `cw-2026-08-17-134712`'s callsign window fits a clock at 48 ms with the edges
> agreeing at **0.677** — the strongest figure of any real window measured. The
> same recording taken whole scores **0.177**, against **0.116** for a recording
> holding nothing at all.

**The statistic was never the problem. The window was.**

Four candidates have now been measured and rejected for HM-OPEN-054: the survey's
verdict, the ratio band, the transition clock, and the peak's prominence. **All
four were different tests applied to the same inherited window** — a rolling span
of the decoder's own state that contains the station and twenty-six seconds of band
noise at once, and therefore measures the noise.

The keying meter separates these same recordings cleanly. It differs from all four
candidates in one respect that has been invisible because nobody was looking at it:
**it chooses its own window.** Six seconds, swept independently, recomputed each
second, owing nothing to the decoder's state.

**The hypothesis under test: does the transition clock separate a real character
from an invented one when it is allowed to choose where it looks?**

**Tim's ruling: measure this before any ruling is made about whether the decoder
may choose its window.** Nothing is being permitted here — only measured.

---

## What this unit is not

**It is not permission for the decoder to speak about a moment other than now.**
§0.0 exists to prevent exactly that, and last session correctly rejected gating on
the best window rather than the current one: a decoder that may speak because it
heard something well several seconds ago is asserting something it does not know.

**This unit does not build a gate, does not change emission, and does not decide
where any window should be.** It answers whether the separation exists at all. If
it does not, the idea is dead and that is the deliverable.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.
  **2,096 tests, four failing. Anything above four is new.**
- `ACarrierClockDoesNotSeparateTests` holds last session's measurement and the
  clock fit itself. **Reuse it. Do not rewrite the fit.**
- The amplitude rule is in the tree. **`Refine` is not.**

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-144 — `cw-2026-08-17-134712` holds a station, callsign `N4L`, elements
ending 21.45–23.01 s, dit 56.3 ms.** The only adjudicated ground truth in nine real
recordings, and everything here rests on it.

**HM-DEC-090 — marking is not a substitute for silence.**

**HM-DEC-091 — one source, and it says which.**

**§0.0 — the display asserts only what is known now.** *Named because this unit
sits next to it. Measuring where a window would have to be is fine. Concluding the
decoder may speak from an old one is not, and is not this unit's to conclude.*

**HM-OPEN-054 and HM-DEC-143 remain open.** Nothing here settles them.

**The keying meter is not to be read by the decoder.** *Its value is that it can
contradict the decoder. This unit may study its principle; nothing may reference
`CwKeyingMeter` or `CwKeyingThresholds` from the decoder.*

**HM-OPEN-053 — `ShortestVote` stays at 5. `MaximumRatio` stays at 3.8.**

**HM-DEC-093 — no radio.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Where do the transitions cluster?

Before choosing any window, say where there is anything to look at.

For each of the nine real captures, report **where in the recording the transitions
are dense enough to be a sender** — from the transitions alone, not from the
decoder, the meter, or any figure in this instruction.

- Say how you decided that, in one paragraph, before the numbers.
- **On `134712` the answer is known**: the callsign runs 21.45–23.01 s. Say whether
  your method finds it, and say so plainly if it does not.
- **On `014854` and `014935` the answer is that there is nothing to find.** Say
  what your method returns there. *If it confidently proposes a window in a
  recording holding no keying, that is the finding and it is worth more than a
  clean result elsewhere.*

---

## Task 2 — The clock, on windows chosen that way

Re-run last session's clock fit on the windows task 1 identified rather than on
whole recordings or on the decoder's rolling state.

Report, for every real capture and every easy-tier fixture:

- the window chosen, the interval found, and the agreement,
- **and, on `134712`, the interval against HM-DEC-144's 56.3 ms.**

**Then the comparison this unit exists for, in one table:** the agreement on
windows from recordings holding a station, beside the agreement on windows from
`014854`, `014935` and synthesized noise.

**Say whether they separate, and by how much. A number, not an impression.**

---

## Task 3 — The case that killed the last candidate

Last session's fatal figures: at the moment of emission a real character came out
at **0.389** and an invented one at **0.470**, and with `Refine` applied an empty
band invented characters at **0.456 to 0.533** while `tightfist-easy` emitted a
real one at **0.497**.

**Re-measure exactly those, with windows chosen by task 1's method.** Same
recordings, same characters, same question.

- **If a real character still comes out below an invented one, the idea is dead.**
  Say so, and this unit is finished having eliminated a fifth candidate for the
  price of one session.
- If they separate, report the margin and the overlap, **and say what would have
  to be true for it to hold on a station ten decibels weaker** — using numbers, as
  the amplitude unit did.

---

## Task 4 — Say what it means, and build nothing

One paragraph. If the separation is real, say what a gate built on it would need
and **what it would have to assert about the present moment to satisfy §0.0**, since
that is the objection that killed gating on the best window.

**Do not build it. Do not change `src/`.** *A measurement that arrives with a
change attached cannot be read on its own.*

---

## Parked — do not touch, do not raise

- **Character structure.** *The boundary. Element versus character versus word
  gaps, letter boundaries, anything needing to know a character occurred.*
- **The keying meter, as a thing the decoder reads.**
- **`Refine`**, `MaximumRatio`, the three-way length fit, the speed-tracker
  rewrite.
- **Why the 19th's stations are missing from the audio.** Five theories dead.
- **The 69 and 233.**
- **Adjudicating any recording.** Tim's ear.
- **HM-OPEN-052**, the five synthesized tests, rulings 096–133, the scorer,
  `CaptureAudioAsync` end to end, `TheRosterIsOneFilePerEvening`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

Unit-specific:

- **Do not change `src/`.** *The whole point.*
- **Do not choose a window using a constant.** *Seventh instance of the error class
  five rulings have gone on closing. Fitted from the transitions or not at all.*
- **Do not use the known callsign window to choose a window.** *`134712` is the
  test of the method, not an input to it. Using HM-DEC-144's boundaries to find
  HM-DEC-144's boundaries proves nothing.*
- **Do not make the answer prettier than it is.** *A fifth candidate eliminated in
  one session is a good outcome and should be reported as one.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139.

**Section 1 opens with task 3**, because it is the question, and tasks 1 and 2
follow as how it was reached.

**Section 2 says in one sentence whether choosing the window rescues the clock** —
and says so even when the answer is no.

**Stop and report.**
