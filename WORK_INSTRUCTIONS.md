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

**One error class, four instances, two still standing.**

Three places in this decoder assumed a textbook fist and were fixed one at a time:
HM-DEC-115 stopped deriving gaps from multiples of the dit and clusters the
sender's own; HM-DEC-119 cut the mark boundary between the two measured clusters
rather than at two dits; and last session's `MeasureCoherence` fix measures each
mark against the fitted long-mark center rather than a hardcoded three.

Last session named the remaining two and left them alone, correctly, because the
order said change one thing. They are:

**`Refine`** averages a mark-derived dit with a gap-derived one. On the fist in
`cw-2026-08-17-134712` — dit 55 ms, element gap 35 ms — it returns **45 ms**. The
fitted dah of 235 then reads **5.2 dits**, falls outside the two-to-five band the
coherence fix trusts, and the textbook three is used instead. **The fix from last
session is real and this defeats it.**

**`CwToneSurvey.MaximumRatio`** is 3.8. This station sends 4.3, so `Verdict.Keyed`
is null on all 6,000 hops while the control returns a keyed verdict on 2,294.

**Tim's ruling, this session:** the ratio band may be worked without opening
HM-OPEN-054. HM-OPEN-054 asks how the survey tells keying from a **carrier**. The
ratio band asks how wide a **fist** counts as Morse. Different questions sharing a
file. **HM-DEC-143 and the keying-versus-carrier distinguisher remain parked and
unbuilt.**

---

## `Refine`'s premise has already been measured false in this repository

Its comment says a mark measured at a threshold reads long by the same amount the
following gap reads short, so the mean of the two is the truth.

**HM-DEC-119 measured that through Hamlet's own detector: the gate reads 100 to
110 ms for a true 100 at every speed. The mark is not long, so there is nothing to
cancel.** HM-DEC-115 measured the other half — a real fist's element gap is
genuinely shorter than its dit, 40 against 57 on `cw-2026-08-18-004507` — because
that is how people send.

Averaging the two therefore shortens the dit by about a fifth on any Farnsworth
sender, which is most operators on the air.
`TheDitComesOutShortWhenTheGapIsShorterThanIt` already records the size of that
bias and asserts the measurement rather than that the behaviour is right.

---

## Measure the two changes separately, then ship them together

**This unit makes two changes, which is a departure and the reason is stated.**
The error class is now identified as a class and the fixture can attribute either
change on its own, so the cost of one-at-a-time is a session and the benefit is
already available from the test.

**That benefit is only real if you take it.** Every table in the report gives four
columns: today, `Refine` alone, ratio alone, both. **A change whose effect is only
ever measured alongside the other has not been measured.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`, and
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`, which last
  session added deliberately. **2,069 tests, four failing. Anything above four is
  new.**

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-048 — nothing raises a confidence score.** Both changes make the decoder
willing to consider a fist it currently discards. **Neither may make it more
willing to guess.** Low confidence renders dimmed; unresolved renders as a
placeholder and never as a letter.

**HM-DEC-091 — one source, and it says which.**

**HM-OPEN-053 — `ShortestVote` stays at 5.** Last session established it is not
implicated here: every mark in the clean stretch is 55 ms or longer.

**HM-OPEN-054 and HM-DEC-143 — still parked.** The ratio band is permitted. **The
keying-versus-carrier distinguisher is not.** If the work reaches for it, stop.

**HM-DEC-093 — no radio. Nothing here needs one.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — `Refine`

Fix the averaging so a sender whose element gap is shorter than the dit does not
have the dit dragged down by it.

- **Report what you changed it to and why, in one paragraph**, before the numbers.
- Whatever replaces it must be **fitted from the signal**, in the manner of
  HM-DEC-115 and HM-DEC-119, not a different constant.
- `TheDitComesOutShortWhenTheGapIsShorterThanIt` records the bias. **Update what it
  asserts to the new measurement and say what the number was and is.** Do not
  delete it.
- Report the dit and the fitted dah in dits, before and after, for all nine
  captures.

---

## Task 2 — `MaximumRatio`

The survey rejects a fist at 4.3 dits.

- **Prefer fitting to widening.** Every other instance of this error class was
  fixed by measuring the sender rather than by moving a constant to a roomier
  value, and a constant at 4.5 will meet a fist at 4.7. **If the survey has no
  fitted cluster available to it, say so plainly and widen — but say that is what
  you did and why fitting was not possible.**
- **An upper bound must remain.** Past about five dits a long mark is a carrier, a
  fade, or somebody leaning on the key, and that is the ground this unit may not
  enter. Say what bound you kept and what it protects.
- Report `Verdict.Keyed` hop counts before and after on `cw-2026-08-17-134712` and
  on the control `cw-2026-08-18-004507`.

---

## Task 3 — The nine captures, four ways

The table this unit is judged on. Characters emitted for every capture in
`tests\fixtures\cw\captured` and `captured\unadjudicated`, in four columns: today,
`Refine` only, ratio only, both.

Last session's floors, which nothing may fall below: `004507` 25, `003016` 38,
`003126` 34, `003758` 14.

**Say which of the two changes did what.** If one of them moves nothing anywhere,
that is a finding and it is reported, not buried in the combined column.

---

## Task 4 — The fixture

Re-run `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.

- **If it goes green, say what the decoder read, and say in the same breath that
  the recording has no adjudicated answer key and the text is not yet evidence of
  anything.** Tim's ear settles that, not this session and not a count.
- **If it stays red, say where it dies now.** Coherence, ratio, both, or somewhere
  new. A third cause named precisely is worth more than a fix guessed at.
- **Do not adjust either change to make this test pass.** *It is the only
  reproducible decode failure this project has and its value is entirely in being
  honest.*

---

## Task 5 — The two from the 19th. **DROP CANDIDATE.**

Run both. **Expect nothing.** The meter reads them as containing no keying at all,
so text appearing there is evidence of invention rather than repair, and must be
reported as such.

---

## Parked — do not touch, do not raise

- **HM-DEC-143 and the keying-versus-carrier distinguisher.** The ratio band is
  permitted; this is not.
- **The speed-tracker rewrite** deriving the unit from key-down durations.
- **The keying meter and `CwKeyingThresholds`.** *The meter is the independent
  witness. It agreed with the gate to within a millisecond on this fist, and that
  is worth more than any change to it.*
- **Why the 19th's stations are missing from the audio.** Five theories dead.
- **The 69 and 233.** Last session confirmed nothing in this path has a bound near
  either. Its own unit.
- **Adjudicating any capture.** Tim's ear.
- **HM-OPEN-052**, the five synthesized tests, rulings 096–133, the scorer,
  `CaptureAudioAsync` end to end, `TheRosterIsOneFilePerEvening`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

Unit-specific:

- **Do not replace one constant with another constant.** *That is the error class
  this unit exists to close, and it would leave a fifth instance for a later
  session to find.*
- **Do not touch the gate, the settled pass or the keying meter.**
- **Do not tune anything to the one recording this came from.** *The nine-capture
  table is the guard, and a change that helps only `134712` should be reported as
  that.*
- **Do not let the decoder emit a letter it is not sure of.** HM-DEC-048.

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139. The ratio ask leaves the queue this session; it was
ruled.

**Section 1 opens with task 3's four-column table**, because it is the only thing
that says whether either change was worth making.

**Section 2 says plainly whether the decoder now reads
`cw-2026-08-17-134712`**, and if it does, that Tim's ear is what makes the text
mean anything.

**Stop and report.**
