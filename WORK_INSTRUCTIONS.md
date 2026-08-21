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

**Tim watched a QSO on 7.059 MHz on the evening of the 20th and Hamlet read
fragments of it.** Five captures of the same two operators, minutes apart, strong
enough that he called them the best of the night and the S-meter reached S9+10.
`UR RST`, `QSB` and `DX` came through as real words. Most of what sat between them
did not.

`decoderWpm` read **`not tracking` on four of the five**, and `12` on the last. The
station was sending at about 14 WPM throughout.

This is the first evidence in this project worth building on: one sender, one fist,
five recordings, four failures and one success. **The question is not why the
tracker never locks. It is why it locked on the fifth and not on the first four.**

### What the recordings measure

Envelope by quadrature mixdown at 825 Hz, 10 ms boxcar, sampled at 1 ms, threshold
midway in amplitude between the 10th and 90th percentile. Runs shorter than 20 ms
excluded from the fit and counted separately:

| capture | elements >=20 ms | runs <20 ms | dit | dah | ratio | derived | `decoderWpm` |
|---|---|---|---|---|---|---|---|
| `005902` | 71 | 28 | 83.5 ms | 237 ms | 2.84 | 14.8 WPM | not tracking |
| `010133` | 72 | **50** | 84.7 ms | 251 ms | 2.96 | 14.3 WPM | not tracking |
| `010244` | 83 | 22 | 83.4 ms | 261 ms | 3.13 | 14.1 WPM | not tracking |
| `010336` | 83 | **19** | 91.5 ms | 269 ms | 2.94 | 13.3 WPM | **12** |

**Every one is cleanly bimodal with a ratio near three.** This is not a sloppy fist
and it is not a weak signal. The unit is derivable from key-down durations alone,
on every capture, by two-means clustering, and the four answers agree to within 1.5
WPM.

**The capture the tracker locked on has the fewest spurious short runs** - 19,
against 28, 50 and 22. The one with 50 is also where the keying detector reported
`9 ms key down` in the sidecar and `3 ms` on screen, for a station the other
captures measure at about 90 ms.

That is the lead: **a stream of sub-20 ms runs that are not elements, polluting
whatever the tracker fits.** It is a hypothesis drawn from four points. Confirm or
kill it; do not assume it.

### Ruled by Tim

**The tracker wins once it is confident, and the operator's setting is the seed it
starts from.** He set 20, the station sent 14, and Hamlet emitted 2 characters in
thirty seconds. He dropped the seed to 13 by hand and the next captures emitted 11,
12 and 14. **A seed wrong by six words a minute cost five sixths of the copy.** He
should not have to find that by turning a knob.

*Rejected: the operator's setting always winning.* It leaves tonight's failure
possible.

*Rejected: proposing a speed and waiting for him to accept it.* A prompt while
operating, which this project deliberately designed out of the capture press.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** The measurements above were taken outside this
repository, from recordings now committed under
`tests\fixtures\cw\captured\unadjudicated\`. **Reproduce them inside the repository
before relying on them.**

- **Report mismatches; do not repair the instruction silently.**
- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`. **Anything above
  three is new.**
- `KeyingEnvelope` already exists from an earlier session and computes exactly the
  envelope described above. **Use it. Do not write a second one.**

---

## Rulings in force

**HM-DEC-048 - speed is re-derived from a rolling window, and nothing raises a
confidence score.** Wider tolerance means a less certain result and that belongs in
the confidence, downward. Unresolved renders as a placeholder, never as a guessed
letter.

**HM-DEC-091 - one source, and it says which.** Task 4 is that ruling applied to a
number that has been wrong for this project's whole life.

**HM-OPEN-053 - `ShortestVote` stays at 5 unless task 1 shows it is the
mechanism.** If the trace finds the vote window is what admits the sub-20 ms runs,
**say so and stop before changing it.** That is Tim's ruling and it has been
outstanding since the 19th.

**HM-DEC-093 - no radio on the development machine.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` 13 -
the six fields 13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 - Why the fifth and not the first four

**Report before changing anything.**

HM-DEC-048 says speed is re-derived from a rolling window. **Find what that window
measures.** Then run all five of the evening's captures through the decoder,
unchanged, and report for each:

1. What the tracker computed, and the moment it either locked or gave up.
2. Whether the derivation uses key-up intervals, key-down durations, or both.
3. Whether it assumes a 1:3:7 relationship holds in the received signal.
4. What it saw on `010336`, where it locked, that it did not see on the other four.
5. Where the sub-20 ms runs come from - the gate, the de-glitch, or the envelope -
   and whether they reach the tracker at all.

**If the trace contradicts the table above, say so and stop.** That table was
measured outside this repository and the tree governs.

---

## Task 2 - Derive the unit from key-down durations

**Gated on task 1.** If task 1 finds the tracker already derives from key-down and
fails for some other reason, **build what task 1 found instead, and say why.**

Otherwise: two-means clustering over key-down run lengths gives dot and dash
directly; the unit is `(dot + dash/3) / 2`. **Never derive the unit from key-up.**
The evening's key-up distribution has no usable 3-unit or 7-unit structure - the
longest gap in one capture is 636 ms where the standard wants roughly 1,200.

- **Exclude runs too short to be an element before fitting.** Twenty milliseconds is
  where this evidence separates cleanly; derive the exclusion from the fitted unit
  rather than hard-coding a millisecond figure if you can.
- **Report the excluded count.** It is diagnostic: 19 against 50 is the difference
  between the capture that locked and the one that went haywire.
- Use gaps only to split, once the unit is known, and be generous - a character
  break near 2 units and a word break near 5, not 3 and 7.
- **Nothing raises a confidence score** (HM-DEC-048). Wider spacing tolerance lowers
  it.

---

## Task 3 - The tracker's figure supersedes the seed, gated on keying

`copySpeed` becomes the seed. Once the tracker is confident its figure governs, and
the panel says which is in use - it already has language for this and the wording
should follow it.

**Confidence is gated on the keying detector's swing figure, not on the tracker's
own opinion.** Tonight: 20-24 dB on all four real captures, 13-14 dB on every
capture containing no station. **The swing was the only figure that held steady all
evening.** The detector's key-down timing did not, and must not be the gate.

**When the detector is holding through a quiet stretch it must not report timing
figures at all.** `010133` printed `9 ms key down` while coasting through a gap on a
station sending 90 ms elements, and the same detector said `3 ms` on screen. The
verdict held; the numbers beside it were about the gap. **A held verdict prints the
verdict and no measurements.**

---

## Task 4 - `snrDb` is wrong and it has cost this project two days

`snrDb` reported **46.5** on a recording containing no station and **41.4** on a
strong one. It rated silence above an S9+10 signal, and a work order was written
from it opening with the claim that this is not a weak-signal problem.

The terminal already computes the honest figure and displays it - *input peaking at
-13 dB, noise around -22 dB* - from the same audio. Measured at the tone, the real
separation is about 17 dB on an empty capture and 22-24 dB on the real ones.

Find what `snrDb` computes, report it, and make it agree with the panel or say in
the field what it actually measures. **Do not delete it.** A missing number and a
wrong number are both worse than a labelled one.

---

## Task 5 - The roster's `text` column. **THIS IS THE DROP CANDIDATE.**

Tonight's rows carry `text` covering the whole session beside `chars` covering
thirty seconds. The sidecar labels it - `textCovers everything read since the
decoder started listening` - and the roster cell does not, so the row reads as
though Hamlet read that text from that capture.

Give the cell the clause `chars` already has. **Drop it whole if the session is
running long, and say so.**

---

## Parked - do not touch, do not raise

- **`RfGain` reads 100% with the knob at noon.** Real, observed by Tim, not this
  unit.
- **`toneHz` and the keying detector disagree by 75 Hz on the same file.** Real, not
  this unit.
- **`SHACK_FACTS.md` on CI-V Transceive.** The `broadcast` line now measures it -
  110 of 110,492 frames were the radio announcing something. Tim's ruling.
- **HM-OPEN-052, HM-OPEN-054**, the five synthesized tests, the three expected
  failures, rulings 096-133, the scorer, `CaptureAudioAsync` end to end, and the
  non-hermetic `TheRosterIsOneFilePerEvening`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

*Four earlier work orders in this project cited 9.5.1 as forbidding a push. They
misquoted it. The file governs.*

Unit-specific:

- **Do not change `ShortestVote`.** *Unruled since the 19th. If task 1 finds it is
  the mechanism, report and stop.*
- **Do not adjudicate the unadjudicated captures or move them into `captured\`.**
  *They have no answer key and Tim has not listened to them.*
- **Do not add a fixture.** *Five real recordings of one QSO are now in the tree.*
- **Do not raise a confidence score anywhere.** *HM-DEC-048.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no other
headings: **What Claude did**, **What Tim should expect**, **What we should do
next**, **What's blocking us** - the last carrying **Asks still outstanding** per
HM-DEC-139.

*Earlier orders in this project called section three "What you should see". 13 names
it "What we should do next" and 13 governs.*

**Section 1 opens with the answer to task 1** - why `010336` locked and the other
four did not.

**Section 2 states in one sentence whether a strong station sending at 14 words a
minute now reads at a speed the operator did not have to set.**

**Stop and report.**
