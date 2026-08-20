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

**`cw-2026-08-17-134712` is not a carrier. It is a station, and its callsign is
`N4L`.**

Last session printed the gate's own element sequence from 21.45 s to 23.01 s of
that recording. Decoded by hand:

```
225  30  55 │ 180 │ 55 40 55 40 60 40 55 30 245 │ 150 │ 60 25 245 40 55 40 55
dah     dit │     │ dit dit dit dit dah         │     │ dit dah dit dit
     N      │     │             4               │     │        L
```

Marks cluster at 55 and 235–245. Gaps cluster at 25–40 between elements and
150–180 between characters. `-.` `....-` `.-..` — **N, 4, L**, a United States
amateur callsign prefix, sent by hand at about 22 words a minute.

**A carrier cannot produce that.** HM-DEC-095 ruled this recording's strong signal
to be a carrier and it has been wrong since the 17th. Three sessions have been
chasing a real defect while a ruling said they were chasing a ghost, and last
session's ratio work was blocked by a test written on that ruling.

**Tim's ruling, this session: HM-DEC-095 is overturned on the evidence above.**

---

## The cause this unit is actually aiming at

Last session named a third cause and it is now the leading one:

**The fist is about six seconds of a thirty-second recording. The speed estimator
looks at the last twenty marks. So twenty-four seconds of noise own the estimate,
and with `Refine` changed the dit reads 25 ms against a true 55.**

That arithmetic explains everything downstream. The fitted dah is 235 ms. Against
a true 55 ms dit that is **4.3**, which is a heavy but ordinary fist. Against the
noise-poisoned 25 ms dit it is **9.4**, which no ratio band would ever accept and
no coherence check would ever call Morse.

**So the ratio band may not need touching at all.** Last session widened it to 5.0
and moved no character count anywhere, which is consistent with the ratio never
having been the binding constraint.

This is the fifth instance of one error class, and it is a different flavour from
the other four. Those compared measured elements against textbook constants.
**This one measures the right thing against the wrong sample.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.
  **2,073 tests, four failing. Anything above four is new.**
- Last session committed `01491d3` and withdrew both `Refine` and the ratio
  change. **Neither is in the tree.** Confirm that before task 3.

---

## Rulings in force

**HM-DEC-095 is overturned.** Its finding that this recording's strong signal is a
carrier is contradicted by a hand decode of the decoder's own elements. **Record
the overturn in `DECISIONS.md` with the element sequence and the callsign**, so no
later session rediscovers it from the audio.

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-048 — nothing raises a confidence score.** *Everything in this unit makes
the decoder better at measuring. None of it may make it more willing to guess.*

**HM-OPEN-054 and HM-DEC-143 remain parked.** Overturning HM-DEC-095 removes a
false premise; it does not settle how the survey tells keying from a carrier. **Do
not build the distinguisher.**

**HM-OPEN-053 — `ShortestVote` stays at 5.**

**HM-DEC-093 — no radio. Nothing here needs one.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Pin the callsign, so this cannot be relitigated

A test that reads `cw-2026-08-17-134712.wav`, takes the gate's elements across the
clean stretch, and asserts they spell **N4L**.

- Assert the element sequence and the letters, **not** that the decoder as a whole
  emits them. The decoder does not, yet, and that is the point.
- If the stretch boundaries differ from 21.45–23.01 s in your run, **report the
  boundaries you used and why**.
- **If it does not spell N4L, stop and report before anything else.** Everything in
  this unit rests on it and the hand decode was done outside the repository.

Then retire `ACarrierNeverConvincesTheTrackerItIsAStation`. It asserts a
falsehood. **Do not delete it silently** — replace it with a test on a recording
that genuinely is a carrier if one exists, and if none exists say so.

---

## Task 2 — The speed estimator's sample

Make the dit estimate come from marks that are plausibly elements rather than from
whatever the last twenty were.

- **Report what you changed and why in one paragraph before the numbers.**
- It must be **fitted from the signal**, not a new constant, and not a longer
  window chosen because it happens to help this file.
- Report, for all nine captures: the dit before and after, the fitted dah in dits
  before and after.
- **On `134712` the target is a dit near 55 ms and a ratio near 4.3.** If you reach
  that, say so. If you reach it by a route that moves the other eight captures'
  dits, **report the movement even where the character counts improve**.

---

## Task 3 — Then, and only then, reconsider `Refine`

`Refine`'s premise was measured false by HM-DEC-119 and last session's fix to it
was withdrawn because it manufactured five characters from `cw-2026-08-20-014854`,
a recording the keying meter reads as holding no keying at any pitch.

- **Measure `Refine` again with task 2 in place.** The invention may have been the
  poisoned dit rather than `Refine` itself.
- **The withdrawal condition stands and is not negotiable**: if the change produces
  characters from `014854` or `014935`, it is inventing them and it does not ship.
  HM-DEC-090 already ruled that marking is not a substitute for silence.
- If it ships, four columns as before: today, task 2 only, task 2 plus `Refine`,
  and the floors held.

---

## Task 4 — The nine captures

Characters emitted for every capture in `captured` and `captured\unadjudicated`,
before and after everything in this unit.

Floors nothing may fall below: `004507` 25, `003016` 38, `003126` 34, `003758` 14.

**`014854` and `014935` must produce no more than they do today.** They contain no
keying; new text there is invention and is a failure of this unit, not a success.

---

## Task 5 — The fixture

Re-run `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.

- **If it goes green, print what the decoder read and check it against `N4L`.**
  The callsign is now a known-correct fragment — the first adjudicated ground truth
  this recording has ever had. Say whether the decoder found it.
- **If it stays red, say precisely where it dies now.**
- **Do not tune anything to make it pass.**

---

## Parked — do not touch, do not raise

- **HM-DEC-143 and the keying-versus-carrier distinguisher.**
- **`CwToneSurvey.MaximumRatio`.** Last session widened it to 5.0 and it moved no
  character count anywhere. **Leave it at 3.8 and let task 2 tell us whether the
  ratio was ever the binding constraint.**
- **The keying meter and `CwKeyingThresholds`.** *It agreed with the gate to within
  a millisecond on this fist and is the reason we know it is a station.*
- **Why the 19th's stations are missing from the audio.** Five theories dead.
- **The 69 and 233.** Its own unit.
- **Adjudicating the rest of `134712`.** `N4L` is established. The remainder of the
  transcript is Tim's ear.
- **HM-OPEN-052**, the five synthesized tests, rulings 096–133, the scorer,
  `CaptureAudioAsync` end to end, `TheRosterIsOneFilePerEvening`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

Unit-specific:

- **Do not replace one constant with another constant.** *Fifth instance of this
  class; a sixth would be avoidable.*
- **Do not ship anything that produces text from `014854` or `014935`.** *The
  meter says there is no keying in them. A decoder that reads them is guessing, and
  a guessing decoder is worse than a silent one.*
- **Do not touch the gate or the keying meter.**
- **Do not tune to `134712`.** *The nine-capture table is the guard.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139.

**Section 1 opens with task 1**: whether the elements spell `N4L`.

**Section 2 says plainly whether the decoder now reads that recording, and whether
what it read contains the callsign.**

**Stop and report.**
