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

**For the first time this project has a decode failure it can reproduce on
demand, on a file in the tree, with no radio attached.**

`tests\fixtures\cw\captured\cw-2026-08-17-134712.wav`. 7.0119 MHz, 40 m, S4,
35.2 dB SNR, 500 Hz filter, `AGC FAST`, preamp off.

Last session's keying meter — which shares no code with the decoder — scores one
of its windows **0.37 at 500 Hz with a 54 ms element, the highest score of any
window it measured on any recording**, higher than the four captures that decoded.
The audio provably contains a keyed station at an ordinary speed.

The decoder produced **nothing at all** from it. Its sidecar:

```
toneHz     550
snrDb      35.2
elements   107 seen, 0 resolved
characters 0 emitted, 0 unsure
sinceLast  0 characters, 107 elements
```

**Zero of 107 elements resolved.** Compare `cw-2026-08-18-003016`, which read a
real QSO: 752 seen, 233 resolved. This one saw elements and resolved none of them.

Every other open question in this project waits on the air. This one does not. **It
is the whole of tonight's work and it may be the same fault that lost two stations
on the 19th.**

---

## Two facts, and neither is a diagnosis

Stated so they are not rediscovered, and **named as leads to be checked, not as
conclusions** — this project has lost evenings to confident diagnoses that named a
component.

**One.** Last session established that `ElementsSeen` has no tone gate, while
`ElementsResolved`, `CharactersEmitted` and `CharactersUnsure` move only through
`Emit`, which returns early unless a tone is latched. A sidecar reading
`107 seen, 0 resolved, 0 emitted` therefore has a shape consistent with the gate
producing elements all the way through while nothing was ever latched. **Check
whether that is what happened rather than assuming it.**

**Two.** The decoder reported `toneHz 550`. The meter, sweeping independently,
chose **500 Hz in every window**. Fifty hertz apart, and the meter's choice is the
one with a 54 ms element in it. Whether fifty hertz matters at a 500 Hz filter is
not known and must be measured, not argued.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`. Last session
  reported 2,053 tests with those three failing. **Anything above three is new.**

---

## Rulings in force

**§9.5.1 — one branch, `main`, and every session commits *and pushes* to `main`.**

**HM-OPEN-053 — `CwGate.ShortestVote` stays at 5.** Sixth unit running. It is the
one change with a measured improvement behind it and it is still unruled.
**If this unit's trace finds that `ShortestVote` is implicated, say so and stop
before changing it.**

**HM-DEC-091 — one source, and it says which.**

**HM-DEC-093 — no radio on this machine.** Nothing here needs one.

**HM-DEC-048 — nothing raises a confidence score.** If any fix here would make the
decoder more willing to emit, low confidence renders dimmed and unresolved renders
as a placeholder, never as a guessed letter.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Find out where the 107 become zero. **CHANGE NOTHING.**

Run the decoder over `cw-2026-08-17-134712.wav` and instrument the path enough to
answer, from the code and the run rather than from reasoning:

1. **Did the tone survey ever latch?** If not, what did it decide and on what
   evidence? If it did, when, at what pitch, and for how long?
2. **What are the 107 elements?** How many marks, how many gaps, and what are their
   durations? Put the distribution in the report. The meter finds a 54 ms element;
   say whether the gate's marks agree.
3. **Where does the first element die** — is `Emit` never reached, reached and
   returning early, or reached and resolving nothing?
4. **What happens if the decoder is pointed at 500 Hz instead of 550?** Measure it.
   Do not change a default to find out.

**Report all four before touching anything.** If the cause turns out to be the
tone survey's verdict, that is **HM-OPEN-054 and HM-DEC-143, which are ruled and
unbuilt and parked** — say so, stop, and do not build the distinguisher. Tim rules
that one.

---

## Task 2 — A test that holds this failure still

Before any fix, a test that reads this fixture and asserts what the decoder does
with it **today**, failing or passing as the current behaviour dictates but
recording the number.

*This is the point of the whole unit: a decode failure that can be run a thousand
times. Whatever is built after this, the test says whether it moved.*

Name it so it reads as the question it asks. Follow §12.5 — it must not share the
decoder's own judgement of what a tone is.

---

## Task 3 — Fix it, only if task 1 named the cause unambiguously and it is not parked ground

**If task 1 did not produce a single, specific, measured cause, stop and report.**
A fix built on a plausible story is what cost this project two evenings, and the
last four theories about the 19th were all plausible and all wrong.

If it did:

- Change the one thing. Not the surrounding code, not the thresholds nearby, not
  anything named in the parked list.
- Re-run task 2's test and report the number before and after.
- **Re-run the four captures that decoded and report their character counts before
  and after.** A fix that reads this recording and breaks those is not a fix.
- Report what it does to the three standing red tests.

---

## Task 4 — What it means for the 19th. **DROP CANDIDATE.**

If a fix landed, run it over `cw-2026-08-20-014854.wav` and `-014935.wav` and say
whether they now produce anything.

**Expect nothing, and say nothing if nothing comes.** The meter reads them as
having no keying at all, so a fix producing text from them would be evidence the
fix is inventing characters, not evidence the fault is cured. *That is the reading
to give it.*

---

## Parked — do not touch, do not raise

- **HM-OPEN-054 and HM-DEC-143**, how the settled pass tells keying from a carrier.
  **If task 1 lands here, stop.**
- **HM-OPEN-053**, `ShortestVote` 5 to 7. Same.
- **The speed-tracker rewrite**, deriving the unit from key-down durations.
- **Why the 19th's stations are missing from the audio.** Five theories dead. **Do
  not offer a sixth.**
- **The keying meter's thresholds.** Provisional, and being scored against an
  evening's roster.
- **HM-OPEN-052**, the five synthesized tests, the three expected failures,
  rulings 096–133, the scorer, `CaptureAudioAsync` end to end, and the
  non-hermetic `TheRosterIsOneFilePerEvening`.

**A parked item that turns out to block a task is raised once, and says it was
parked.**

---

## Also worth recording, and not this unit's work

**The 69 and 233.** Last session established both captures are the first of their
session, so nothing was carried. What remains is that `cw-2026-08-18-003016`
reached 69 emitted and 233 resolved from **752 elements in thirty seconds of a real
QSO**, and `cw-2026-08-20-014854` reached the identical pair from **359,837
elements across seven hours of noise**. The other four captures show 168/524,
41/116, 177/590 — all different.

**Two inputs three orders of magnitude apart arriving at the same pair is the
signature of a bound, not of chance.** If anything in task 1 touches a buffer,
queue or ring with a capacity near 69 characters or 233 elements, **say so**.
Otherwise leave it; it is a question for its own unit.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

Unit-specific:

- **Do not change the keying meter.** *It is the independent witness. The moment it
  shares an assumption with the decoder it stops being able to contradict it.*
- **Do not change a default to test a hypothesis.** *Measure with an override and
  report the number.*
- **Do not make the decoder more willing to emit as a way of passing this
  fixture.** *HM-DEC-048. A decoder that guesses is worse than one that is silent,
  and this recording has no adjudicated answer key to catch a guess.*
- **Do not adjudicate the fixture.** *Tim's ear. The meter's score is a
  measurement, not a verdict about what the station said.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139.

**Section 1 opens with task 1's four answers**, because whether anything else in
this unit happened depends on them.

**Section 2 says plainly whether the decoder now reads this recording, and if it
does, what it read** — with the caution that the fixture has no adjudicated answer
key and text alone is not proof it is right.

**Stop and report.**
