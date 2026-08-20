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

**Every synthesized fixture in this repository sends textbook 1:3:7 spacing, and
neither station proved on the air does.**

Last session found `Refine`'s one cause: it treats the sender's element gap as a
second measurement of the dit, and it is one only when the spacing is textbook.
Every fixture sends exactly one dit of gap, which is what makes the average work on
them. Both adjudicated stations are Farnsworth — **35.6 ms of gap on a 56.3 ms dit
for `N4L`, 73.3 on 100.4 for `VA3VRR`** — which is what HM-DEC-115 measured off the
air as how people actually send.

**Tim's ruling, this session, and it has two halves.**

**`Refine` is dropped.** Last session's own table settles it: without `Refine` the
dit reads **55.0 against a true 56.3 on `N4L`** and **100.0 against 100.4 on
`VA3VRR`**, both inside one per cent. **It buys nothing on either recording whose
truth is known.** Four sessions were spent unblocking a change that was not needed.
It is not to be revived without new evidence.

**The fixture suite gains Farnsworth senders.** The gap is not in the decoder, it is
in what the suite knows about. A suite that only ever sends one style cannot catch
a decoder that only handles one style, and that is the shape of every surprise this
week.

*Rejected: re-cutting the existing fixtures to Farnsworth, which would retire
answer keys the project has leaned on all week. Also rejected: keeping `Refine`
and threshold-ing gap-over-dit at 0.90 against 0.63 and 0.73, which last session
correctly named as the seventh instance of the error class six rulings have gone on
closing.*

**One correction to the previous order, made by the session and recorded here.**
It said HM-DEC-119 measured the mark as "not long, so there is nothing to cancel."
HM-DEC-119's figures are 100–110 ms for a true 100 — long by nought to ten per
cent. **The premise is half true, not false, and that half is exactly what `Refine`
cancels.** The order overstated it.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.
  **2,108 tests, four failing. Anything above four is new.**
- The separation test shipped two sessions ago. **`Refine` is not in the tree and
  is not to be added.**
- The bulletin currently reads 36 characters against a key of 47.

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-144 — `cw-2026-08-17-134712` holds `N4L`, dit 56.3 ms, dah 238.3 ms,
element gap 35.6 ms, ratio 4.24.**

**HM-DEC-145 — `cw-2026-08-17-013347` holds `VA3VRR`, dit 100.4 ms, dah 274.3 ms,
element gap 73.3 ms, ratio 2.73.**

*Those two are the only measurements in this project that are known rather than
estimated, and they are the model for what a Farnsworth fixture should sound like.*

**HM-DEC-115 — a real fist's element gap is genuinely shorter than its dit.** The
finding this unit is bringing into the suite.

**HM-DEC-114 — the easy tier passes or fails.** *New fixtures do not join the easy
tier without Tim's ruling. Say what tier you propose and why.*

**HM-DEC-091 — one source, and it says which.** *A synthesized fixture is weaker
evidence than a real capture and its answer key must say it was generated.*

**HM-DEC-048 — nothing raises a confidence score.**

**HM-OPEN-054 stays open. No transition-shape test, no gate in front of emission.**

**The keying meter is not read by the decoder.**

**HM-OPEN-053 — `ShortestVote` stays at 5. `MaximumRatio` stays at 3.8.**

**HM-DEC-093 — no radio.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — What the generator can already do. **CHANGE NOTHING.**

Read the generator and report **whether it can send an element gap other than one
dit**, and a character gap other than three, and a word gap other than seven.

- If it can, say how, and task 2 is a matter of new fixtures rather than new code.
- **If it cannot, say so and say what would have to change.** *The generator is
  `GENERATOR_BRIEF.md`'s subject and changing it is a larger thing than adding
  fixtures.*
- Report what the existing fixtures declare about their spacing, and whether any of
  them already departs from 1:3:7.

---

## Task 2 — Two Farnsworth fixtures, cut to the two known fists

Generate two, each with an answer key, each modelled on a measurement rather than
on a guess:

| fixture | dit | element gap | dah | after |
|---|---|---|---|---|
| `farnsworth-heavy` | 56 ms | 36 ms | 238 ms | `N4L`, HM-DEC-144 |
| `farnsworth-light` | 100 ms | 73 ms | 274 ms | `VA3VRR`, HM-DEC-145 |

- **The text is yours; the timing is not.** Use ordinary amateur exchange content,
  long enough to exercise word spacing.
- **The answer key says the fixture was generated**, and from which decision entry
  its timing comes (HM-DEC-091).
- Character and word gaps: **use the adjudicated recordings' own figures where they
  exist** — `N4L`'s character gap is 165 ms, `VA3VRR`'s 150 — and say what you did
  for the word gap, which neither adjudication measured.
- **Propose a tier and say why. Do not add them to the easy tier.** HM-DEC-114.

---

## Task 3 — What the decoder does with them

Run both. Report the read text against the key, character by character where they
differ, and the fitted dit against the fixture's true dit.

**This is the point of the unit.** If the decoder reads them whole, the suite has
gained coverage and nothing else is wrong. **If it does not, that is the first
reproducible Farnsworth failure with an answer key in this project**, and it is
worth more than the fixtures themselves.

- Report what fails and where. **Do not fix it.**
- Run the whole suite. **Nothing existing may break.**

---

## Task 4 — The two adjudicated recordings, unchanged

Confirm `134712` and `013347` still read as they did, and report their fitted dits
against 56.3 and 100.4.

*This unit touches no decoder code, so both should be identical. If either moved,
something is wrong and it is the finding.*

---

## Parked — do not touch, do not raise

- **`Refine`.** Dropped by ruling. *Not to be revived, re-measured or proposed.*
- **A transition-shape test, or any gate in front of emission.**
- **Character structure**, and the keying meter as something the decoder reads.
- **`MaximumRatio`**, `MinimumSeparation`, the three-way length fit, the
  speed-tracker rewrite.
- **The bulletin's standing red.** HM-DEC-114 left it deliberately.
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

- **Do not change the decoder.** *This unit changes what the suite knows about, not
  what the decoder does. A fixture and a fix in one session cannot be read apart.*
- **Do not re-cut the existing fixtures.** *Their answer keys are load-bearing.*
- **Do not add anything to the easy tier.** HM-DEC-114.
- **Do not invent timings.** *Both fixtures come from adjudicated measurements; a
  third fist made up to fill a gap would be exactly the weak evidence this unit
  exists to reduce.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139. **The fixture-spacing ask leaves the queue; it was
ruled.**

**Section 1 opens with task 3**: what the decoder read against the keys.

**Section 2 says plainly whether the decoder handles a Farnsworth sender it has
never seen before.**

**Stop and report.**
