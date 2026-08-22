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

**Two things. The first is a regression that has taken the project's only
measuring instrument off the screen, and it comes first.**

### One: the advisory region is empty and the capture press is gone

Below the transcript there is now **an empty grey box and then a large blank gap**,
and then the dimmed-character legend. Everything that used to sit between them has
vanished from the screen: **the "I hear a station" capture press**, the keying
meter, the tone advisory, the nothing-coming-through note and the speed row.

**The button is not greyed. It is absent**, and so is the whole strip, which points
at the fixed-height advisory region shipped by the layout unit rendering with
nothing placed into it rather than at anything in the decoder.

**The decoder is not the fault.** The terminal in the same screenshot is reading
and emitting.

**This is the highest-priority item in the unit.** The capture press is how Tim
marks a case; the roster is the only instrument this project has for scoring
whether he can read the band; and **without the press an evening at the rig
produces no evidence at all.** It was on the keep list of every removal order for
exactly this reason.

### Two: the decoder's window after a retune

**The decoder's window holds twelve seconds of envelope mixed down at whatever
pitch the tracker held at each moment. When the tracker moves to another station
part-way through, the window holds two pitches at once and the decode is made over
the mixture.**

Measured last session on the sensitivity sweep: exactly one retune at every level,
600 Hz to 650 on a fixture sending at 640, happening legitimately between
characters. What it costs, from eleven decibels down: **0.06 of the message wrong
at eleven, 0.19 at three, 0.64 at minus four.**

**This is not the mid-character interlock and cannot be fixed by it.** That
interlock now holds the tracker inside every character it reads, which is what it
is for. The move that costs the characters is between characters and is correct.

### Ruled by Tim

**The window is cleared when the tracker moves to a different station.** The
decode is always made over one pitch.

*Rejected: re-mixing the held envelope to the new pitch.* Better if it works, and
worth doing later, but it is new arithmetic in the one path that is currently
reading and clearing is provably correct today.

*Rejected: leaving it and living with a known-bad window after each move.* The
retune happens exactly when somebody answers a call. **Twelve seconds of nothing
is recoverable; twelve seconds of confident wrong characters is what HM-DEC-009
exists to prevent, at the worst possible moment.**

**The cost is accepted and must be stated on screen, not hidden.** Clearing loses
up to twelve seconds of reach every time Hamlet follows somebody.

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **Rulings below are cited by number only. Read each and apply what it says.**
  **If a ruling does not support what this order needs, report it and stop.**
- **The failing set is 28.** Record the exact set before and after and name every
  difference.
- **Beware the synthetic sweep.** Last session found that a change looking perfect
  on the sensitivity fixture had silenced all six real recordings, because the
  fixture never exercised what broke. **Every measurement in this unit is reported
  on the sweep AND on all six recordings, together, every time.**

---

## Rulings in force

- **HM-DEC-120.** Nothing is emitted on audio holding no signal.
- **HM-DEC-009.** No confident wrong answer. The whole reason for this ruling.
- **HM-DEC-096**, whose phase 3 is the mid-character interlock. **Untouched here.**
- **HM-DEC-091.**
- **HM-DEC-150**, the version scheme. Task 5.
- **HM-DEC-093** and `SHACK_FACTS.md` — no radio on the development machine.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md`
**§13**, which names that file's fields — `STATE`, `PHASE`, `BALL`, `NEXT_PASTE`,
`UPDATED`, `NOTE`. `UPDATED` from the clock; `NOTE` says what is moving inside the
task. Also every ten minutes while a task runs.

---

## Task 1 — Find out why the strip is empty

**Report before changing anything.**

1. What renders the advisory region, and what is supposed to place messages into
   it.
2. **Why is it empty?** Is nothing being placed, is something placed and not
   drawn, or was the placement lost when the panels were consolidated?
3. **Where did the capture press go?** Name the element, whether it still exists
   in the view, and what decides that it renders.
4. **Is anything else missing that this order has not noticed?** Compare what the
   region is meant to hold against what it holds.

**If the cause is not in the layout work, say so and say where it is.**

---

## Task 2 — Put the capture press back, first

**The press returns to the screen before anything else in this unit is built, and
is committed on its own** so it can be reached without the rest.

- It marks a case and keeps the audio exactly as it did. **Nothing about its
  behaviour changes.**
- **Every other message that belongs in that region returns with it** — the keying
  meter, the tone advisory, the nothing-coming-through note. The layout unit was
  about where they sit, not whether they are said.
- **The layout rule still stands: the transcript does not move.** Fixing the
  emptiness must not bring back the jump.
- **Add a test that the capture press renders**, so this cannot happen silently
  again.

---

## Task 3 — What counts as a move

**Report before changing anything.**

The tracker retunes for more than one reason. **Name every one**, and for each say
whether the audio already in the window was mixed at a different pitch afterwards.

- A refinement of a few hertz around the same station is not a station change and
  **must not clear the window** — the envelope is still substantially coherent and
  clearing on every small correction would empty the window constantly.
- A move to a different station is.
- **Say where the line falls and what evidence you have for it.** If the tracker
  does not currently distinguish them, say so — that is the first thing to build.

**If clearing cannot be triggered on the right subset of moves, stop and report.**
Task 2 is already committed by then, so the press is safe either way.
Clearing on all of them is a different ruling from the one Tim made.

---

## Task 4 — Clear the window on a station change

The held envelope is dropped and refilled from the new pitch.

- **The decoder must not invent while the window refills.** A short window is less
  evidence, and less evidence must mean silence rather than guesses (HM-DEC-120).
  **Report what the likelihood ratio does while the window is short.**
- **Nothing already settled is retracted.** Characters read before the move stand;
  this is about what is decoded after it.

---

## Task 5 — Say so on screen

**The operator must know why the terminal went quiet.** Twelve seconds of silence
with no explanation reads as a dead band, and this will happen at the exact moment
somebody answers his call.

The terminal says, in the project voice, that Hamlet has moved to another station
and is listening afresh. It clears when text resumes.

**Wording is yours. The layout rule stands: the transcript does not move.**

---

## Task 6 — Measure it, on both

**The sweep and all six recordings, every level, together.**

| what | expected |
|---|---|
| the sweep, 18 dB down to −6 | **invention at or below what it is now**, and ideally nothing above 12 dB |
| `004507`, `003016`, `003126`, `003758` | **at or better than last session's strings**, quoted verbatim against them |
| `014854`, `014935` | silent, offline and streamed |

**`003758` and `003016` are the two that have not come back** to their
pre-removal strings. Say plainly whether they have.

**If any recording reads worse than last session's string, stop and report.**

---

## Task 7 — Bump the version

Read the current version from `Directory.Build.props`, bump the patch, report what
it moved from and to. **HM-DEC-150.** One work unit, one patch.

---

## Parked — do not touch, do not raise

- **Re-mixing the held envelope to the new pitch.** Rejected for now, worth doing
  later. **Do not build it as an optimisation.**
- **`FollowSpeed` has no supplier**, and `MostRealRecordingsSitInTheWidestWindow`
  asserts a window nothing sets. Its own unit.
- **The reacquiring guard** and `NoSpeedIsNamedWithoutCharactersToNameItFrom`.
- **The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.**
- **`HM-OPEN-051`** recorded open while HM-DEC-143 closes it.
- **The twenty-two failures that predate the removal.**
- The sidecar's `text` and the leading edge; the missing captures; mode-follow's
  guard; `RfGain`; the likelihood gate at 15.0; the keying meter's thresholds.
- **HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.**

---

## Asks still outstanding

Carried inbound per HM-DEC-139, verbatim until ruled. **Verify against
`OPEN_ISSUES.md` and report anything here that is closed, or open and missing.**

- Whether the sidecar's `text` should include the leading edge.
- The captures from the evenings of the 20th and 21st are not in the tree.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0.
- The keying meter's provisional thresholds.
- `FollowSpeed` has no supplier.
- The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.

**The window after a retune leaves this queue** with this unit.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not clear on a small refinement.** *The window would empty constantly and
  the cure would cost more than the disease.*
- **Do not let a short window guess.** *Less evidence means silence.*
- **Do not touch the mid-character interlock.** *It was measured and is right.*
- **Do not report a result on the sweep alone.** *A change that looked perfect on
  that fixture silenced all six recordings last session.*
- **Do not build the re-mix.** *Parked.*
- **Do not remove any advisory to tidy the region.** *They went missing once
  already. Task 2 is restoration.*
- **Do not change what the capture press does.** *Only whether it is on screen.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **§12.2 names the four
headings** — **What Claude did**, **What Tim should expect**, **What we should do
next**, **What's blocking us** — the last carrying **Asks still outstanding** per
HM-DEC-139. No other headings.

**Section 1 opens with why the advisory strip was empty and whether the capture
press is back**, because an evening without it produces no evidence. **Then**
where the line falls between a refinement and a station change.

**Section 2 states in one sentence whether he can mark a case again, and in one
more what he sees when Hamlet follows somebody mid-contact.**

**Stop and report.**
