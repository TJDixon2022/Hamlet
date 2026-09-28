# Work instruction 476 - the oscilloscope: a mark is the envelope over a threshold, at any pitch

**One unit. Seed it and drop STOP, or run it by hand.** It builds the front half of a signal
detector the decoder does not have, and puts it on screen so the owner's ear can judge it. It
drives nothing yet. Four tasks.

**The owner's words, 2026-09-28, R90:** *"Think you're an oscilloscope. Once we hit a certain
amplitude, regardless of frequency, that's probably a character. Everything else is noise."*
And: *"Really, it's a signal that we're finding in the noise and we're decoding."*

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules, short

**R88 - the corpus is banned.** No task reads a recording, runs a floor, the engine
carry-forward line, a metric or the keyed set. Entry and exit: the app carry-forward line and
the types this unit touches. A touched type that reads a recording is not run, and is named.

**HM-DEC-155.** No suite. Named types only, one per invocation, own `timeout`. Never
background and poll. The app line loses names to the dispatcher loop; re-run once, count
neither way.

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Scripts go in
`.run-unit\unit476-<name>.sh`, run with `sh`.

**By hand:** take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end,
write nothing to `RUN_LEDGER.md`, touch nothing under `tools\arbiter\`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. `UNIT:` line without brackets.
`ADVANCES: step 12 criterion 1`. **Line C of the ordering block names how many items section 4
raises.** Nothing in section 4 halts this unit.

---

## 2. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The owner can watch Hamlet find a keyed signal in the noise,
            at any pitch, the way an oscilloscope would show it.
ADVANCES:   step 12 criterion 1
```

**What the owner heard.** Twenty strong stations, high pitch to low, zero characters. Then
seven verdict rows: a station keying at score 0.27 with a 48 ms dit, refused because its swing
was 18.6 dB in a bin where the code wanted 20. The survey admitted nothing on any row. The
decoder mixed at 600 Hz *assumed* on six of seven.

**What the code does, read from the source, and why it cannot see a strong station.** There
is no start decision. The decoder is always decoding at a guessed pitch. Alongside it,
`CwToneSurvey` looks only at bins from 300 to 900 Hz, waits 3 seconds and 8 marks, and admits
a bin only if its *level over time* swings 6 dB across a midpoint - a measurement AGC FAST,
which the app itself sets on every CW tune-in, flattens by design. `CwKeyingMeter` then wants
20 dB of the same swing over 6 seconds. So a strong station at 750 Hz under AGC dies at the
first gate with the noise, and nothing downstream ever sees its durations. Three thresholds
that only agree on one kind of signal near 600 Hz. That is the pitch blindness the owner hears.

**What signal-in-noise actually is, and what this unit builds.** The oldest CW detector: the
envelope of the audio over time, a noise floor tracked under it, a threshold above the floor,
and **above the threshold is a mark, below is a gap**. Marks and gaps are durations. Durations
at three to one are dits and dahs. **Frequency is not needed to decide that a station is
keying** - it comes for free afterward, from where the energy sits in the spectrum while the
envelope is up. The owner's rows show swings of 17.5 to 21.8 dB on every station under AGC; a
threshold at half of that catches all of them, at any pitch the radio's filter passes.

**This unit builds the detector and puts it on screen. It changes nothing about how the decoder
is driven.** That is the next unit, after the owner's ear has judged this one.

---

## 3. Verify against the tree

- Where the CW tab gets its audio: the hop the decoder receives (`CwDecoder.Process` or
  `Listen`), its sample rate and hop size. The oscilloscope reads the same hops.
- `CwHearingViewModel` and `CwPitchStripControl` from unit 474: how a control under the
  transcript is drawn and fed once a second. **The oscilloscope is drawn beside them; nothing
  of 474's is removed.**
- The rig state's `FilterBandwidth` and `CwPitch`, so the passband edges can be shown.
- How `JsonlTelemetry` writes a `cw` row, so the owner's verdict row of 474 can carry the
  oscilloscope's state as well (task 3).
- **Read only, change nothing:** `CwToneSurvey`, `CwKeyingMeter`, `CwToneTracker`. This unit
  adds a detector; it does not touch the old one.

## 4. Rulings in force

`PHASE_PLAN.md` R77 to R90 and §6. **R88** the corpus is banned; step 12 is authorable beside
step 11 and nothing else is. **R90** the detector is the envelope over a threshold, regardless
of frequency; frequency is read after. **§0.0** every number on screen is measured, and the
threshold is shown as what it is. **§0.6** color is never the sole carrier. **§0.2** nothing
that keys or transmits. **HM-DEC-155, HM-DEC-165, FACT-006.**

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-28, headline **Signal detection is an envelope over a threshold at any
pitch; frequency is read after**, ref HM-DEC-185:**

```
---
id: HM-DEC-185
date: 2026-09-28
refs: PHASE_PLAN.md R90 and step 12, CwToneSurvey.cs HysteresisDb, CwKeyingMeter.cs ConfidentSwingDb, CwToneTracker.cs MinimumToneHz MaximumToneHz, the owner's verdict rows of 2026-09-28, work instruction 476
---

**A CW signal is detected as the audio envelope standing over a threshold above the tracked
noise floor, at any pitch the radio's filter passes; its frequency is read from the spectrum
after it is detected, and the decoder does not start until it has been.** Tim, 2026-09-28.

**What the code did.** The decoder was always decoding at a guessed pitch. The survey and the
meter tried to correct the guess afterward, each by measuring a bin's level swing over time -
6 dB and 20 dB - which the AGC FAST the app itself sets flattens, and only in bins from 300 to
900 Hz, which is the radio's sidetone setting and not where a received station lands. Twenty
strong stations gave no characters, and the owner's verdict rows showed a keyed station
refused for 1.4 dB of swing while the survey admitted nothing at any pitch.

**What is ruled.** In the owner's words: think like an oscilloscope; once the amplitude is
over a threshold, regardless of frequency, that is probably a character, and everything else
is noise. The detector is built as that, shown on the CW tab first so the owner's ear can
judge it, and wired to drive the decoder in the unit after. The 300 to 900 window, the survey's
swing gate and the meter's swing gate are not repaired; they are superseded.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 476's record
of it.
```

---

## 5. The tasks

### Task 0 - the record and the entry round

`PHASE_OUTCOME.md` gets `## UNIT 476 - STEP 12` from the block at the foot. `PHASE_STATUS.md`
names 476 and `CURRENT_STEP: 12`. Patch-bump. `DECISIONS.md` HM-DEC-185 and the `CLAUDE.md`
row. Entry round: the app carry-forward line.

### Task 1 - the detector (12.1)

A new type in `src\Hamlet.RadioEngine\Cw\`, `CwEnvelopeDetector`, fed the same hops the decoder
gets, computing per hop:

- **the envelope**: the energy across the passband the radio's filter passes - `CwPitch` plus
  and minus half `FilterBandwidth` from the rig state, or the whole audio band if those are
  unknown - as one number per hop, in dB;
- **the floor**: the envelope's level when nobody is keying, tracked - falling quickly when the
  envelope drops, rising slowly, so a station's marks do not drag it up. **How it is tracked
  is the author's; the report says how, in one sentence, and it is a rule with no fitted
  constant from any recording.**
- **the threshold**: floor plus a margin in dB. **Start at 9 dB** - half the smallest swing on
  the owner's rows - and make it a single named constant with a remark saying it came from
  those seven rows and nothing else.
- **mark or gap**: the envelope over the threshold, or not, this hop;
- **the run**: how long the current mark or gap has lasted, in milliseconds;
- **the pitch, while a mark is up**: the bin of the spectrum carrying the most energy, across
  the whole passband - not 300 to 900 - and its contrast over the bins beside it in dB.

**It changes nothing about the decoder.** It observes the same audio and reports.

**Watch it fail first**, with synthetic hops made in the test - a tone keyed on and off at a
known pitch over a known noise floor. Red on a stub; green when marks land where the tone was
on, gaps where it was off, and the pitch reads the tone. **No recording.**

### Task 2 - the oscilloscope on screen (12.2)

A control under 474's strip, `CwScopeControl`, drawing the last **four seconds**:

- the envelope as a trace;
- the floor as a line under it;
- the threshold as a line above the floor, labelled with its margin;
- **marks as bars along the bottom** while the trace is over the threshold, so the owner sees
  dits and dahs light up as he hears them;
- the current pitch and contrast as text while a mark is up: *"tone 742 Hz, 24 dB over the
  band"*; when no mark is up: *"no tone"*;
- the passband edges shaded, labelled with the radio's filter width and pitch.

Words on every mark, never color alone. Hover text says what each line is and that the
threshold is the one number that decides a mark. Fed at the decode hop rate, not once a
second - **the owner has to see dits.** If that costs the UI, fed at 20 per second and say so.

**Watch it fail first**, headless, with a driven detector.

### Task 3 - the owner's verdict carries the scope (12.3)

Extend 474's `owner_verdict` row with the detector's state at the press: `scopeEnvelopeDb`,
`scopeFloorDb`, `scopeThresholdDb`, `scopeMark` (true or false), `scopeRunMs`, `scopePitchHz`,
`scopeContrastDb`, and **`scopeMarksLast4s`** - the count of marks in the last four seconds -
so the next unit can read whether the scope saw keying where the owner heard it. The row's
key set is asserted closed, as 474's test does.

### Task 4 - the exit round

`Hamlet.sln` builds with warnings as errors. The app carry-forward line. Every type touched.
**`src\Hamlet.RadioEngine\Cw` diff against entry is one new file and nothing changed** - name
it - plus whatever the view model and control needed. Transmit files print nothing against
`7e209cb4`. **No recording was read.**

---

## 6. Do not

- Do not wire the detector to the decoder, the tracker, the survey or the meter. It observes.
- Do not fit any constant to a recording. The margin comes from the owner's rows; the floor
  tracker is a rule.
- Do not limit the pitch search to 300 to 900 Hz. The whole passband.
- Do not read, run or measure against any recording.
- Do not remove or change anything unit 474 built.
- Do not touch what keys or transmits.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 7. Report

`output.md` at the root, four headings exactly.

```
READ IN THIS ORDER.

A. What the owner will see: the trace, the floor, the threshold, the marks,
   and the tone line - in one paragraph.
B. Step 12: 12.1 the detector, 12.2 the scope, 12.3 the row.
C. The rest. Section 4 raises <n> items, none blocking. No recording was read.
```

```
UNIT:       476 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     threshold margin: 9 dB from seven owner rows; pitch search: whole passband; recordings read: 0
```

**Section 2 tells the owner:** rebuild, tune a station, watch the bars light up under the trace
as you hear dits and dahs, press the buttons as before. And what to look for: bars where you
hear keying, no bars on silence, the tone line where your ear puts the pitch.

---

```
ARBITER-DECISION
STEP: 12
APPROACH: build an envelope detector over the whole passband - envelope, tracked floor, a 9 dB threshold from the owner's rows, marks and gaps, the pitch read from the spectrum while a mark is up - draw it as an oscilloscope on the CW tab, and carry its state on the owner's verdict row, driving nothing yet
MOVE: continue
WHY: PHASE_PLAN.md step 12 criterion 12.1 asks for a detector that finds a keyed signal as the audio envelope standing over a threshold above the tracked noise floor at any pitch the filter passes, reading the pitch from the spectrum after, and changing nothing about the decoder
STATE: not started
DECIDED: the floor-tracking rule, the scope's frame rate, and the drawn passband when the rig state is unknown are the author's, overrulable
LICENCE: PHASE_PLAN.md R88, R90, section 6, step 12; HM-DEC-185; CLAUDE.md 0.0, 0.2 and 0.6; HM-DEC-155; FACT-006
ACCOMPLISHED: the owner can watch Hamlet find a keyed signal in the noise at any pitch, the way he hears it, before it is trusted to drive the decoder
ADVANCES: step 12 criterion 1
END-ARBITER-DECISION
```
