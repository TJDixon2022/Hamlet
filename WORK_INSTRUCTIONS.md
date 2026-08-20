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

**HM-OPEN-054 is ruled. This is the unit that builds it.**

The decoder has no reason to stay silent on a band with no station on it, and that
is now the only thing between it and reading a real off-air fist. `Refine` has been
measured three times, each time correct in principle, each time stopped by the same
recording: it reads `U EE ■ ■` out of `cw-2026-08-20-014854`, thirty seconds in
which the keying meter finds no keying at any pitch. Removing the gap average
lengthens the dit until gate chatter passes the coherence check.

**Tim's ruling, this session: the settled pass tells keying from a carrier by
whether the transitions land where a clock fitted to them predicts.**

Two candidates were rejected and the reasons matter:

**Gaps rather than marks** — a carrier has no gaps and keying does. Rejected
because **the problem in front of us is not a carrier.** `014854` is band noise,
which has plenty of gaps; they simply land nowhere in particular. A gap test
separates carriers and would not separate the recording that has actually blocked
`Refine`.

**A tight ratio near three** — rejected outright on the evidence. **`N4L` sends
4.3.** A ratio test would discard the one station this project has proved is real,
and it is the error class five rulings have gone on closing.

**Keying is periodic. Noise is not. That is the whole idea.**

---

## The boundary, and it is close

**HM-DEC-143 and HM-OPEN-054's own gate failed once already**: removing the
survey's verdict let a carrier recording produce 33 characters. That is why this
was parked rather than built.

**The clock is fitted to transitions only. Never to characters.**

Fitting a clock to the on/off edges is permitted. Asking which character a mark
belongs to, or grouping marks into characters to fit anything, is the structure
question and is **still parked**. The line is close enough that it must be watched:

- **Permitted:** the times at which the signal turned on and off, and whether they
  are consistent with some unit interval.
- **Not permitted:** element gaps versus character gaps versus word gaps, letter
  boundaries, or anything that needs to know a character occurred.

**If the work needs to cross that line to succeed, stop and report.** A unit that
comes back saying the clock cannot be fitted without character structure is a real
answer and is worth the session.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.
  **2,089 tests, four failing. Anything above four is new.**
- The amplitude rule shipped last session and is in the tree. **`Refine` is not.**

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-144 — `cw-2026-08-17-134712` holds a station, callsign `N4L`, dit
56.3 ms, dah 238.3 ms, ratio 4.24.** The only adjudicated ground truth in nine real
recordings, and **everything in this unit rests on it**.

**HM-DEC-090 — marking is not a substitute for silence.** Seventeen hundred
characters once came out of half a minute of band noise, every one marked, and
marking was not enough. **A clock test that admits `014854` and marks the result
low has failed.**

**HM-DEC-114 — the easy tier passes or fails.**

**HM-DEC-048 — nothing raises a confidence score.**

**HM-DEC-091 — one source, and it says which.**

**HM-OPEN-053 — `ShortestVote` stays at 5. `MaximumRatio` stays at 3.8.**

**HM-DEC-093 — no radio.**

**The keying meter is not to be read by the decoder.** *Tim ruled option B for
this reason: the meter's whole value is that it can contradict the decoder, and a
decoder that obeys it can never be contradicted by it again. It is the witness that
proved `134712` was a station and that the 19th's captures were empty.* Do not
reference `CwKeyingMeter` or `CwKeyingThresholds` from anything in `src\Hamlet.RadioEngine\Cw`
outside the meter itself.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Measure before building. **CHANGE NOTHING.**

Fit a clock to the transitions of each of the nine real captures and the easy tier,
and report how well the edges land on it.

- Say what you fitted and how, in one paragraph, **before the numbers**.
- The unit interval is **fitted from the transitions**, not taken from the speed
  estimator and not a constant.
- Report a single figure per recording for how well the edges agree with it, plus
  the fitted interval.
- **Report it separately for `134712`'s callsign window, 21.45–23.01 s**, where the
  answer is known.

**The question this answers: do the four that decode, plus `N4L`'s window,
separate from `014854`, `014935` and synthesized noise?**

**If they do not separate, stop and report.** That is a real answer, it costs one
session, and it says the third candidate is dead too.

---

## Task 2 — The gate, only if task 1 separated

A test in the settled pass: emission requires the transitions to agree with a
fitted clock.

- **Fitted, relative, no constant.** *Seventh instance of the error class five
  rulings have gone on closing if it is not.*
- It must be a **no-op on every clean fixture**. The easy tier is the guard.
- Say what it does with a window holding a station that pauses between overs.
  *A gate that fires during a natural pause silences a real contact.*

---

## Task 3 — `Refine`, for the fourth time

Only with task 2 in place.

| | required |
|---|---|
| `cw-2026-08-20-014854` | **no more than 1** |
| `cw-2026-08-20-014935` | **0** |
| `004507` | ≥ 26 |
| `003016` | ≥ 38 |
| `003126` | ≥ 36 |
| `003758` | ≥ 14 |
| `013347` | ≥ 8 |
| the easy tier | **whole** |

**The withdrawal condition is unchanged and non-negotiable.** `U EE ■ ■` out of
`014854` and it does not ship, no matter what else improved.

Report `134712`'s dit against HM-DEC-144's **56.3 ms**.

---

## Task 4 — The fixture

Re-run `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.

- **If green, print what it read and say whether `N4L` is in it, in the right
  place.** Last session `LooksLikeMorse` went true from 22.55 to 22.86 s with
  coherence 0.49 and no character boundary fell inside it. Say what happens there
  now.
- **If red, say precisely where it dies.**
- **Do not tune anything to make it pass.**

---

## Parked — do not touch, do not raise

- **Character structure.** *The boundary. Element versus character versus word
  gaps, letter boundaries, anything needing to know a character occurred.*
- **The keying meter.** Not read by the decoder, by ruling.
- **`MaximumRatio`**, the three-way length fit, the speed-tracker rewrite.
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

- **Do not fit anything to characters.** *The whole boundary.*
- **Do not read the keying meter from the decoder.**
- **Do not ship `Refine` if it produces text from `014854` or `014935`.**
- **Do not build a test that would reject a 4.24 ratio.** *`N4L` is the ground
  truth; a gate that discards it is wrong however well it does elsewhere.*
- **Do not tune to `134712`.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139.

**Section 1 opens with task 1's separation figures**, because whether anything else
in this unit happened depends on them.

**Section 2 says plainly whether anything shipped and whether the decoder now reads
`cw-2026-08-17-134712`.**

**Stop and report.**
