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
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**Tim operated the evening of the 20th and Hamlet read fragments of stations he
could hear clearly.** Six captures, several senders, signals to S9+10. `UR RST`,
`QSB`, `DX` and `AUR` came through as real words. Most of what sat between them
did not.

The speed tracker behaved three different ways across the evening, and that
variety is the evidence this unit is built on.

### What the recordings measure

Envelope by quadrature mixdown at the tone the keying detector reported, 10 ms
boxcar, sampled at 1 ms, threshold midway in amplitude between the 10th and 90th
percentile of the envelope. Runs shorter than 20 ms excluded from the fit and
counted separately. Measured outside this repository, from files now committed
under `tests\fixtures\cw\captured\unadjudicated\`:

| capture | elements >=20 ms | runs <20 ms | dit | dah | ratio | derived | `decoderWpm` |
|---|---|---|---|---|---|---|---|
| `005902` | 71 | 28 | 83.5 | 237 | 2.84 | 14.8 WPM | not tracking |
| `010133` | 72 | 50 | 84.7 | 251 | 2.96 | 14.3 WPM | not tracking |
| `010244` | 83 | 22 | 83.4 | 261 | 3.13 | 14.1 WPM | not tracking |
| `010336` | 83 | **19** | 91.5 | 269 | 2.94 | 13.3 WPM | **12** |
| `015834` | 106 | 34 | 71.1 | 255 | **3.58** | 15.4 WPM | not tracking |
| `020033` | 84 | **145** | 40.3 | 159 | **3.94** | 25.7 WPM | panel showed **29** |

Milliseconds throughout. The first four are one QSO, one fist.

**Three behaviours, and a session must explain all three:**

1. **Four captures of a clean 14 WPM fist, and it never locked.** Every one is
   bimodal with a ratio near three. There is nothing hard about them.
2. **One where it locked, at 12 against a measured 13.3.** That capture has the
   fewest spurious short runs of any: 19.
3. **One where it locked at 29 and the independent measurement also gives 25.7 -
   so the tracker was not wrong about what it was fitting.** But that capture's
   ratio is 3.94 where Morse is 3.0, and it carries 145 sub-20 ms runs against 19
   to 50 everywhere else. The text was almost entirely `T`, `D`, `N` and `M` - the
   shortest characters there are.

**The correlation across all six is the short runs, not the speed and not the
signal strength.** 19 short runs and it locked correctly; 145 and it fitted a
distorted ratio; 22 to 50 and it did not lock at all. **That is a correlation over
six points and it is not a mechanism.** Confirm or kill it. Do not assume it.

**An error this instruction made and corrected, recorded so it is not repeated:**
the 29 WPM reading was first called "roughly double the true speed" on the strength
of the T/D/N/M text, before anything was measured. It is not double. Both methods
agree at 26 to 29. **The mechanism was named from the shape of the output rather
than from the audio, which is the trap this project has already written down.**

### Ruled by Tim

**The tracker wins once it is confident, and the operator's setting is the seed it
starts from.** He set 20 against a 14 WPM station and Hamlet emitted 2 characters
in thirty seconds. He dropped the seed to 13 by hand and the next captures emitted
11, 12 and 14. **A seed wrong by six words a minute cost five sixths of the copy.**

*Rejected: the operator's setting always winning.* It leaves that failure possible.

*Rejected: proposing a speed and waiting for him to accept it.* A prompt while
operating, which this project deliberately designed out of the capture press.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Reproduce every figure above inside the
repository before relying on it.

- **Report mismatches; do not repair the instruction silently.**
- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`. **Anything above
  three is new.**
- `KeyingEnvelope` already exists and computes exactly this envelope. **Use it. Do
  not write a second one.**

---

## Rulings in force

**HM-DEC-048 - speed is re-derived from a rolling window, and nothing raises a
confidence score.** Wider tolerance lowers confidence. Unresolved renders as a
placeholder, never as a guessed letter.

**HM-DEC-091 - one source, and it says which.**

**HM-OPEN-053 - `ShortestVote` stays at 5 unless task 1 shows it is the
mechanism.** The sub-20 ms runs are the lead in this unit and the vote window is
one candidate for where they come from. **If the trace finds that, say so and stop
before changing it.** It is Tim's ruling, outstanding since the 19th, and this
would be the first real evidence for it.

**HM-DEC-093 - no radio on the development machine.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` 13 -
the six fields 13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 - Explain all three behaviours

**Report before changing anything.**

HM-DEC-048 says speed is re-derived from a rolling window. **Find what that window
measures.** Then run all six captures through the decoder, unchanged, and report:

1. What the tracker computed on each, and the moment it locked or gave up.
2. Whether the derivation uses key-up intervals, key-down durations, or both.
3. Whether it assumes a 1:3:7 relationship holds in the received signal.
4. What `010336` had that the four failures did not.
5. Where the sub-20 ms runs come from - the gate, the de-glitch, the envelope - and
   whether they reach the tracker.
6. **Why `020033` fitted a 3.94 ratio.** A dah is three dits. A fit returning 3.94
   is fitting something that is not dits and dahs, and that is the clearest single
   symptom in the evidence.

**If the trace contradicts the table, say so and stop.** The table was measured
outside this repository and the tree governs.

---

## Task 2 - Derive the unit from key-down durations

**Gated on task 1.** If task 1 finds the tracker already derives from key-down and
fails another way, **build what task 1 found and say why.**

Otherwise: two-means clustering over key-down run lengths gives dot and dash; the
unit is `(dot + dash/3) / 2`. **Never derive the unit from key-up.** The evening's
key-up distribution has no usable 3-unit or 7-unit structure; the longest gap in
one capture is 636 ms where the standard wants about 1,200.

- **Exclude runs too short to be an element before fitting.** Twenty milliseconds
  is where this evidence separates; derive the exclusion from the fitted unit
  rather than hard-coding a figure if you can.
- **Report the excluded count and the fitted ratio, every time.** 19 against 145,
  and 2.94 against 3.94, are the two numbers that distinguish a good fit from a bad
  one in this evidence.
- **A fit whose ratio is far from three is not to be trusted, and must lower
  confidence rather than be corrected into shape.** HM-DEC-048.
- Use gaps only to split, once the unit is known, and be generous: a character
  break near 2 units, a word break near 5, not 3 and 7.

---

## Task 3 - The tracker's figure supersedes the seed, gated on keying

`copySpeed` becomes the seed. Once the tracker is confident, its figure governs and
the panel says which is in use - it already has language for this and the wording
should follow it.

**Confidence is gated on the keying detector's swing figure, not the tracker's own
opinion.** Tonight: 19 to 24 dB on every capture containing a station, 13 to 14 dB
on every capture containing none. **The swing was the only figure that held steady
all evening.**

**The panel and the sidecar must not disagree about the speed in the same moment.**
On `020033` the header showed 29 while `decoderWpm` in the file read `not tracking`.
Two readouts of one fact, differing. HM-DEC-091.

---

## Task 4 - The tone estimates disagree. **THIS IS THE DROP CANDIDATE.**

Every capture carries two tone figures and they differ: `toneHz 375` beside
`keying at 400 Hz`, `toneHz 625` beside `500 Hz` on screen, `toneHz 750` beside
`825`. Report which is which, and make the sidecar say what each one is measuring.

**Drop it whole if the session is running long, and say so.**

---

## Parked - do not touch, do not raise

- **`RfGain` reads 100% with the knob at noon.** Observed by Tim. Not this unit.
- **Stations reading 375 to 825 Hz against a `CwPitch` of 600.** Real, unexplained,
  and possibly a fourth wrong rig readout. Not this unit.
- **The second pass changing its mind about 57% of characters on `015834`.** Real,
  large, and HM-OPEN-053's territory. Not this unit.
- **`SHACK_FACTS.md` on CI-V Transceive.** Now measured: 1,284 of 28,113 frames
  were the radio announcing something. Tim's ruling.
- **HM-OPEN-052, HM-OPEN-054**, the five synthesized tests, the three expected
  failures, rulings 096-133, the scorer, `CaptureAudioAsync` end to end, and the
  non-hermetic `TheRosterIsOneFilePerEvening`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not change `ShortestVote`.** *Unruled. If task 1 finds it is the mechanism,
  report and stop.*
- **Do not name a mechanism you have not measured.** *This instruction did it once
  tonight and the correction is recorded above.*
- **Do not adjudicate the unadjudicated captures or move them into `captured\`.**
  *No answer key, and Tim has not listened to them.*
- **Do not add a fixture.** *Six real recordings are already in the tree.*
- **Do not raise a confidence score anywhere.** *HM-DEC-048.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings: **What Claude did**, **What Tim should expect**, **What we should
do next**, **What's blocking us** - the last carrying **Asks still outstanding**
per HM-DEC-139.

**Section 1 opens with task 1's answer to all three behaviours** - never locking,
locking correctly, and fitting a 3.94 ratio.

**Section 2 states in one sentence whether a station sending at 14 words a minute
now reads at a speed the operator did not have to set.**

**Stop and report.**
