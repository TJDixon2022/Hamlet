# Work instruction 474 - a light that says "I think I hear CW," and two buttons that say whether it was right

**Seed under `--seed`.** The owner tuned twenty strong stations tonight, high pitch to low,
and got zero characters. The decoder has no idea when to start. This unit builds nothing that
translates. It builds a light, a strip, two buttons, and one telemetry row, so the owner's
ear can teach the detector where the entry is. **Four tasks, drop from the back.**

**The owner's ruling, 2026-09-27, R88 - in his words:** *"We're trying to teach you how to
find the entry, how to know when to start evaluating. Right now, you have no clue."* And:
*"No testing against WAV files. You're banned until further notice."*

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

**R88 - the corpus is banned.** No task of this unit reads a recording under `tests\fixtures\cw`,
runs the capture floors, the adjudicated floor, the keyed set, the metrics, or any test whose
input is recorded audio. The entry and exit rounds run **the app carry-forward line only** and
the types this unit touches. **The three floor tests are not run.** If a carry-forward line
includes a CW floor, run the line's other members by name and say so. **A unit that measures
anything against a WAV has broken the owner's order.**

**HM-DEC-155.** No suite. Named types only, one per invocation, each with its own `timeout`.
Never background and poll. The app line loses names to the headless dispatcher loop; re-run
a loss once, count it neither way.

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Scripts go in
`.run-unit\unit474-<name>.sh`, run with `sh`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. `UNIT:` line without brackets.
`ADVANCES: step 11 criterion 1`. WHY cites the plan. **Nothing in section 4 halts this unit.**

---

## 2. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The owner can see whether Hamlet thinks it hears CW and where it
            is looking, and can tell it whether it was right.
ADVANCES:   step 11 criterion 1
```

**What the owner did tonight.** With the current build, twenty strong stations, pitches from
very high to low. Not one character. His ear heard code on every one.

**What the code does between "a tone" and "a signal," read from the source, every number of
it fitted to recordings and none to the air:**

1. `CwToneTracker` looks only between **300 and 900 Hz** (`MinimumToneHz`, `MaximumToneHz`) -
   the IC-7300's sidetone setting range from the manual, page 4-14, **not where a received
   station lands.** A station beating at 1000 Hz is invisible by design.
2. `CwToneSurvey` admits a 25 Hz bin only when its level swings past a midpoint with **3 dB**
   of hysteresis over a 3-second window. Under AGC FAST, which every one of the owner's
   sidecars shows, a strong station's swing is flattened.
3. `CwKeyingMeter` then requires four things over 6 seconds: score at least 0.10, median element
   25 to 250 ms, and **swing at least 20 dB** (`ConfidentSwingDb`). Twenty came from seven
   recordings on 2026-08-20. On the air the 17:37 capture failed it at 16 dB with 98 key-downs
   in the same sentence.
4. With nothing admitted, the decoder mixes at the last pitch, the bank centre, or the
   operator's 600 Hz - and reads silence there while the station keys somewhere else.

**The owner is not asking for these to be fixed.** He is asking for two things he can see, and
a way to tell the detector when it is wrong, so the next unit fixes what his ear says is
wrong rather than what a recording says.

---

## 3. Verify against the tree

- Where the CW tab's terminal lives (`CwTerminalControl`, `CwTranscript`, the view model's
  CW section) and how a control gets a hover text and a color that is never the sole carrier
  (§0.6).
- `CwDecoder.Tracker`, `CwToneSurvey.Candidates()`, `CwKeyingMeter`'s verdict and its four
  figures, and the bin list the survey sweeps - **the seams the light and the strip read
  from.** Name them. Add none to the decoder.
- How a telemetry row is written today (the `.jsonl` under `%AppData%\Hamlet\telemetry\`,
  and the writer the sidecar and the census use), so the verdict row goes through the same
  writer with a new `category` and `event`.
- `data\bands\mode-receiver-conditions.json`: what the CW row sets AGC to. **Read only; change
  nothing** - it is a fact for section 4.

## 4. Rulings in force

`PHASE_PLAN.md` R77 to R88 and §6. **R88** the corpus is banned; steps 2 to 10 are closed;
step 11 is the only authorable step. **§0.0** the light says *I think*, never *there is*.
**§0.6** color is never the sole carrier. **§0.2** nothing that keys or transmits. **R72** no
word prior. **HM-DEC-155**, **HM-DEC-165**, **FACT-006** there is no radio on this machine,
so nothing here is evidence about the air - **that is the owner's job, and it is why the
buttons exist.**

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-27, headline **The owner's ear is the yardstick; the corpus is banned**,
ref HM-DEC-184:**

```
---
id: HM-DEC-184
date: 2026-09-27
refs: PHASE_PLAN.md R88 and step 11, CwToneTracker.cs MinimumToneHz MaximumToneHz, CwKeyingThresholds.ConfidentSwingDb, work instruction 474
---

**The corpus is banned until the owner lifts the ban, and the owner's ear is the yardstick
for whether Hamlet hears CW.** Tim, 2026-09-27.

**What happened.** With the current build the owner tuned twenty strong CW stations at
pitches from very high to very low and received no characters. Every threshold between a
tone and a signal - the 300 to 900 Hz range, the survey's 3 dB hysteresis, the meter's 20 dB
swing - was fitted to recordings and never checked against the air. Twenty-three units after
unit 449 kept one change, all measured on recordings with inferred keys.

**What is ruled.** No unit reads, runs, tunes against, or keeps a change on any recording
under the fixtures until the ban is lifted. Hamlet shows a light saying whether it thinks it
hears CW, a strip showing the whole pitch range it sweeps and where it is looking, and two
buttons - *I agree with you* and *You're an idiot* - each writing a telemetry row carrying
the owner's verdict beside the detector's state at that moment. The next unit reads those
rows. The owner's words: *"We're trying to teach you how to find the entry, how to know when
to start evaluating. Right now, you have no clue."*

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 474's record
of it.
```

---

## 5. The tasks

### Task 0 - the record and the entry round

`PHASE_OUTCOME.md` gets `## UNIT 474 - STEP 11` from the block at the foot. `PHASE_STATUS.md`
names 474 and `CURRENT_STEP: 11`. Patch-bump. `DECISIONS.md` HM-DEC-184 and the `CLAUDE.md`
row. **Entry round: the app carry-forward line, and nothing that reads a recording.**

### Task 1 - the light (11.1)

On the CW tab, beside the terminal, one indicator: **"I think I hear CW"** - lit, or dark with
the words **"I don't think I hear CW."** Words as well as color (§0.6). It is driven by what
the detector already computes: lit when the keying meter's verdict is keying, or when the
survey has admitted a candidate. **Do not add a new detector.** The light shows the existing
one's opinion, so the owner can judge that opinion.

Hover text says what it rests on, in plain words: *"lit when the keying meter calls it keying
or the survey admits a pitch; this is Hamlet's guess, not a fact."*

**Watch it fail first:** a headless test that drives the view model with a meter verdict of
keying and asserts the words change. No audio.

### Task 2 - the pitch strip (11.2)

Under the light, a horizontal strip spanning the **whole range the detector could sweep -
draw it 200 to 1200 Hz** so the owner sees the range and its edges even where the detector
does not look yet. On it:

- the band the tracker actually searches today, 300 to 900, shaded and labelled with its
  numbers;
- every bin the survey has admitted, marked;
- the pitch the tracker is mixing at now, marked distinctly, with the number;
- the keying meter's own best pitch, marked distinctly, with the number and its four figures
  in the hover: score, median ms, swing dB, verdict.

Text labels for everything marked, never color alone. Hover text on the strip says what each
mark is. **It changes nothing about where the detector looks.** It shows where it looks.

**Watch it fail first**, headless, with a driven tracker pitch.

### Task 3 - the two buttons and the row (11.3)

Two buttons beside the light: **"I agree with you"** and **"You're an idiot."** Each press
writes one telemetry row through the existing writer, category `cw`, event
`owner_verdict`, carrying:

- `verdict`: `agree` or `idiot`;
- `light`: what the light said at the press;
- `trackerHz`, `trackerHasPitch`, `trackerHasKeying`;
- `meterVerdict`, `meterHz`, `meterScore`, `meterMedianMs`, `meterSwingDb`;
- `survey`: every admitted bin's Hz and level;
- `frequency`, `mode`, `agc`, `preamp`, from the rig state;
- `inputPeakDb`, `inputFloorDb`;
- `sinceVerdictMs`: milliseconds since the light last changed.

**Nothing else.** No audio is captured by the press, no sidecar is written. The row is the
owner's ear beside the detector's state, and that is all the next unit needs.

Hover text on each button says what it records. **Watch each fail first**, headless, by
asserting the row's fields.

### Task 4 - the exit round

`Hamlet.sln` builds with warnings as errors. The app carry-forward line. Every type touched.
**No recording read anywhere in this unit**, and the report says so in one line. The transmit
files print nothing against `7e209cb4`. `src\Hamlet.RadioEngine\Cw` prints nothing against
entry except any seam that had to be exposed for reading - name it.

---

## 6. Do not

- **Do not read, run, or measure against any recording.** Not for a floor, not for a check.
- **Do not change the detector.** Not a threshold, not the range. This unit shows; the next
  unit, reading the owner's rows, changes.
- **Do not translate.** The light and the strip carry no letters.
- **Do not add a detector.** The light reports the existing one.
- Do not use color as the sole carrier.
- Do not touch what keys or transmits.
- Do not halt for a question.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 7. Report

`output.md` at the root, four headings exactly.

```
READ IN THIS ORDER.

A. What the owner will see on the CW tab, in one paragraph.
B. Step 11: 11.1 the light, 11.2 the strip, 11.3 the buttons and the row.
C. The rest, in as few lines as it takes. No recording was read.
```

```
UNIT:       474 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     recordings read: 0; controls added: <n>; telemetry event: owner_verdict
```

**Section 2 tells the owner** what to do at the radio: tune a station, look at the light,
press a button, repeat; and that every press is a row the next unit reads.

**Section 4 states one fact, not a question:** what the CW receive condition sets AGC to, and
that the keying meter's swing threshold was fitted to recordings taken under it.

---

```
ARBITER-DECISION
STEP: 11
APPROACH: put a light on the CW tab that says whether Hamlet thinks it hears CW, a strip showing the whole pitch range and where the detector is looking, and two buttons whose press writes the owner's verdict beside the detector's state to telemetry - reading no recording and changing no detector
MOVE: continue
WHY: PHASE_PLAN.md step 11 criterion 11.1 asks for one indicator on the CW tab that says whether Hamlet thinks it hears CW, in words as well as color, driven by the existing detector and translating nothing
STATE: not started
DECIDED: the exact wording of the light and the hover texts, the strip's drawn range beyond what the detector sweeps, and which seams are exposed for reading are the author's, overrulable
LICENCE: PHASE_PLAN.md R85, R88, section 6, step 11; HM-DEC-184; CLAUDE.md 0.0, 0.2 and 0.6; HM-DEC-155; FACT-006
ACCOMPLISHED: the owner can see what Hamlet thinks it hears and where it is looking, and can tell it when it is wrong, so the next unit is aimed by his ear and not by a recording
ADVANCES: step 11 criterion 1
END-ARBITER-DECISION
```
