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

**`Refine` is unblocked for the first time in four sessions, and what stops it now
is a better problem.**

Last session found the cause of the invention and closed it: **a two-means fit cuts
any continuum in half and the halves land near three to one by construction.**
`cw-2026-08-18-003016`'s short marks span 50–70 ms, a factor of 1.4;
`cw-2026-08-20-014854`'s span 10–80, a factor of eight. Both fit two clusters at a
ratio near three. The coherence check never asked whether its two fitted lengths
were really two things. It does now, using `CwToneSurvey.MinimumSeparation` shared
rather than copied.

**Tim's ruling, this session: the separation test stands.** Two characters lost on
`cw-2026-08-18-004507` and `cw-2026-08-18-003126` are an acceptable price for one
no longer invented out of `cw-2026-08-20-014854`. **One of those two costs is
measured and the other is a guess**, and §0.0 weighs a confident wrong answer above
a missing one. Both recordings still clear their committed floors of 25 and 34.

*Rejected: tuning the separation figure to recover the two, which would be the
error class six rulings have now gone on closing. Also rejected: withholding the
change until those recordings are adjudicated, which would leave an invented
character on screen meanwhile.*

**With the separation test in place, `Refine` no longer invents anything from
either empty recording.** It breaks four synthesized fixtures instead:
`clean-12wpm`, `clean-18wpm`, `CwFarnsworthTests.TheBulletinsWordsComeOutAsWords`
and `prosigns-edge`.

**Those have answer keys.** That is the difference between this session and the
last four: the thing standing in the way can now be read against a known-correct
transcript rather than argued about.

---

## What `Refine` is and why it is wanted

It averages a mark-derived dit with a gap-derived one. Its premise — that a mark
reads long by the same amount the following gap reads short, so the mean of the two
is the truth — **was measured false in this repository by HM-DEC-119**: the gate
reads 100 to 110 ms for a true 100 at every speed, so the mark is not long and
there is nothing to cancel. HM-DEC-115 measured the other half, that a real fist's
element gap is genuinely shorter than its dit.

Averaging therefore shortens the dit by about a fifth on any Farnsworth sender,
**which both adjudicated stations are**: `N4L` at 4.24 dits to the dah on a 56.3 ms
dit, `VA3VRR` at 2.73 on 100.4.

`TheDitComesOutShortWhenTheGapIsShorterThanIt` records the bias and asserts the
measurement rather than that the behaviour is right.

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
- The separation test shipped last session. **`Refine` is not in the tree.**

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-144 — `cw-2026-08-17-134712` holds `N4L`, dit 56.3 ms, ratio 4.24.**

**HM-DEC-145 — `cw-2026-08-17-013347` holds `VA3VRR`, dit 100.4 ms, ratio 2.73.**
*Two adjudicated fists, and they are different. A rule fitted to one now has
somewhere to be wrong — use both.*

**HM-DEC-114 — the easy tier passes or fails.**

**HM-DEC-090 — marking is not a substitute for silence.**

**HM-DEC-048 — nothing raises a confidence score.**

**HM-OPEN-054 stays open. No sixth transition-shape test. No gate standing in front
of emission.**

**The keying meter is not read by the decoder.**

**HM-OPEN-053 — `ShortestVote` stays at 5. `MaximumRatio` stays at 3.8.**

**HM-DEC-093 — no radio.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — What the four fixtures read, and what they should. **CHANGE NOTHING.**

With `Refine` applied, for each of `clean-12wpm`, `clean-18wpm`, `prosigns-edge`
and the bulletin's word spacing:

- **the answer key, and what the decoder reads instead** — the actual strings, side
  by side,
- the dit before and after `Refine`, and the sender's true dit,
- and **whether the failure is the same in all four or different in each.**

**This is the whole question and it must be answered before anything changes.**
Four fixtures breaking from one cause is a defect. Four breaking from four causes
means `Refine` is wrong and the previous sessions were right to withdraw it.

**If the four have one cause, name it and the line.** If they do not, **stop and
report** — do not repair four things at once.

---

## Task 2 — Fix it, only if task 1 named one cause

- **Fitted, not a constant.** *Seventh instance of the error class six rulings have
  gone on closing.*
- **Inside the estimator.** *No gate.*
- It may make the decoder measure better. **It may not make it more willing to
  emit** (HM-DEC-048).
- **`Refine`'s own premise is false and may be replaced rather than defended.** The
  goal is a dit that matches the sender, not the preservation of an average whose
  justification HM-DEC-119 already disproved.

| | required |
|---|---|
| `cw-2026-08-20-014854` | **0** |
| `cw-2026-08-20-014935` | **0** |
| `004507` | ≥ 25 |
| `003016` | ≥ 38 |
| `003126` | ≥ 35 |
| `003758` | ≥ 14 |
| `013347` | ≥ 8, **and `VA3VRR` still readable in it** |
| the easy tier | **whole** |
| `clean-12wpm`, `clean-18wpm`, `prosigns-edge`, the bulletin | **whole** |

**Report the dit for both adjudicated recordings against their known figures**:
`134712` against 56.3 ms, `013347` against 100.4 ms. *Those two numbers are the
only ones in this project that are known rather than estimated, and a change that
moves either away from its truth is wrong however the counts read.*

---

## Task 3 — The fixture

Re-run `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.

Last session `134712` emitted nothing with the separation test, and **one
character** with `Refine` on top.

- **If green, print what it read and say whether `N4L` is in it, in the right
  place.**
- **If red, say precisely where it dies.**
- **Do not tune anything to make it pass.**

---

## Task 4 — The bulletin. **DROP CANDIDATE.**

`CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey` has been standing red since
HM-DEC-114 left it deliberately. Report what it reads now and how far from its key
it is.

**Do not fix it and do not change what it asserts.** *It is a long-standing red
with a ruling behind it; this unit only reports the number so its movement is
visible.*

**Drop it whole if the session runs long.**

---

## Parked — do not touch, do not raise

- **A sixth transition-shape test, or any gate in front of emission.**
- **Character structure**, and the keying meter as something the decoder reads.
- **`MaximumRatio`**, the three-way length fit, the speed-tracker rewrite.
- **The separation figure**, `CwToneSurvey.MinimumSeparation`. *Ruled this session.
  Do not move it to recover a character.*
- **Why the 19th's stations are missing from the audio.** Five theories dead.
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

- **Do not weaken the separation test to make `Refine` fit.** *It is what stopped
  the invention and it was ruled this session.*
- **Do not ship anything producing text from `014854` or `014935`.**
- **Do not tune to any one fixture.** *Two adjudicated recordings and the fixture
  suite are the guards, and they now disagree with each other enough to be useful.*
- **Do not touch the gate, the survey or the meter.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139. **The two-lost-characters ask leaves the queue; it
was ruled.**

**Section 1 opens with task 1's four fixtures, their keys beside what was read.**

**Section 2 says plainly whether `Refine` shipped, and whether the decoder now
reads `cw-2026-08-17-134712`.**

**Stop and report.**
