# Work instruction 480 - the radio points, the bars show, the letters ride on top

**One unit. Seed it and drop STOP, or run it by hand.** Three things the owner asked for on
2026-09-28, in one build: the radio's own spectrum scope is turned on and read so it points
the detector at the signal; the noise trace goes away and only bars are drawn; and every
character the decoder settles is written above the bars that made it. Four tasks.

**The owner's words, R94:** *"You have a waterfall. Why aren't we using that? You can read
any settings from the radio."* And R95: *"I don't care when it's noise. We don't need to show
that. When we start to detect bars, I want to graph those. As we start to find letters, mark
them in that graph and put the letter over top. This is a passive training tool for learning
how to read CW."*

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

**R88 - the corpus is banned.** No task reads a recording, runs a floor, the engine line, a
metric or the keyed set. Entry and exit: the app carry-forward line and the types this unit
touches; a touched type that reads a recording is not run and is named.

**HM-DEC-155.** No suite. Named types only, one per invocation, own `timeout`. Never
background and poll. The app line loses names to the dispatcher loop; re-run once, count
neither way.

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Scripts go in
`.run-unit\unit480-<name>.sh`, run with `sh`.

**By hand:** take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end,
write nothing to `RUN_LEDGER.md`, touch nothing under `tools\arbiter\`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. `UNIT:` line without brackets.
`ADVANCES: step 12 criterion 4`. **Line C names how many items section 4 raises.** Nothing in
section 4 halts this unit.

---

## 2. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The radio says where the signal is; the detector watches there;
            the owner sees bars with letters over them.
ADVANCES:   step 12 criterion 4
```

**What the owner's rows say tonight, 17:13 to 17:15 UTC, two stations.** Seven *idiot*
verdicts. The meter's best pitch sat at 350 to 425 Hz on both stations with scores of 0.01
to 0.04 and medians of 3 to 6 ms - it was reading the loudest noise bin and calling it its
best. The bars found nothing. The owner, who wears hearing aids, heard both stations plainly
and never touches the radio except to tune. **A signal a hearing-aided ear picks out at once
is not weak. The software was not looking where it was.**

**What the sidecars have said all week, and nobody read:** `ScopeOn on, ScopeOutput off`. The
IC-7300's spectrum scope is running on its own screen and Hamlet has never asked it to send
the data. **The tree already parses it**: `RigSpectrumSource` assembles CI-V 0x27 waveform
frames into `SpectrumFrame`s and raises `FrameReady`; `ScopeFlow` and `ScopeReadiness` know
whether the output is on. Its own remark says it makes no write - something else has to turn
the output on, and in CW mode nothing does.

**The radio's scope is the frequency instrument.** Calibrated by Icom, centered on the dial,
level per bin across the span. In CW mode a peak at the dial is the station at `CwPitch`; a
peak 300 Hz above the dial is a beat note at `CwPitch` + 300. No sweep, no tolerance, no
guessed sample rate. The detector's job shrinks to watching the one bin the radio points at
for bars - which is what it is good at.

**And the graph.** The trace draws noise; the owner does not want to see noise. He wants the
bars, sized as they are, and **the letter the decoder settled written above the bars that made
it.** A word slides past with its letters riding on top. It is a training tool, and it is the
honest debugging view: a bar with no letter is a miss, a letter with no bar is an invention,
a wrong letter sits over the shape that should have been something else.

---

## 3. Verify against the tree

- `RigSpectrumSource`: how it starts, what a `SpectrumFrame` carries (bins, span, centre,
  level scale), and `FrameReady`. `ScopeFlow.Check` and `ScopeReadiness`: what states they
  report and what "output off" looks like to them.
- `Ic7300Rig`: the write that sets scope output on - the CI-V sub-command under 0x27 - and
  whether any code path issues it today. **Which modes, if any, turn it on now.**
- `data\bands\mode-receiver-conditions.json`: the CW row, and whether a `scopeOutput` field
  exists as a condition anywhere. `ReceiverSetup`: how a new condition would be written and
  read back (unit 419's one-scale comparison).
- `CwEnvelopeDetector`: how its watched bin is chosen today (the bin with the most bars in the
  last second), and the seam where an external pitch could name the bin instead.
- The decoder's settled-character stream: `CwTranscript`, `CharacterSettled`, and **what
  timing each settled character carries** - start and end hop, or span - so it can be placed
  over the bars.
- `CwScopeControl` and `CwScopeFrame` after 478: the trace and the bars.

## 4. Rulings in force

`PHASE_PLAN.md` R77 to R95 and §6. **R88** the corpus is banned; steps 11 and 12 only.
**R74 / R67** Hamlet sets the radio for the mode; the operator is not rig control - **turning
the scope output on in CW mode is a receive condition and is Hamlet's to set.** **R91** bars
decide keying. **R94** the radio's scope points the detector. **R95** bars only, letters over
them. **§0.0** a letter is drawn only where the decoder settled it; the graph invents nothing.
**§0.2** nothing that keys or transmits - scope output is receive-side. **§0.6** color never
the sole carrier. **HM-DEC-155, HM-DEC-165, FACT-006.**

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-28, headline **The radio's scope points the detector; bars with letters
over them**, ref HM-DEC-188:**

```
---
id: HM-DEC-188
date: 2026-09-28
refs: PHASE_PLAN.md R94 R95 and criterion 12.4, RigSpectrumSource.cs, ScopeFlow.cs, Ic7300Rig.cs 0x27, CwEnvelopeDetector.cs, CwScopeControl.cs, the owner's verdict rows of 2026-09-28 17:13, work instruction 480
---

**In CW mode Hamlet turns on the IC-7300's scope output and reads its waveform over CI-V
0x27; the detector watches the bin the radio's scope points at; the CW tab draws bars only,
never noise, with each settled character written above the bars that made it.** Tim,
2026-09-28.

**What was wrong.** Every capture sheet since the restore phase has read "ScopeOn on,
ScopeOutput off". The tree parses the radio's scope stream and nothing in CW mode turns it
on. Meanwhile the detector chose its own bin by sweeping, and on two stations the owner heard
plainly tonight its meter sat at 350 to 425 Hz with scores near zero, reading the loudest
noise. The owner: "You have a waterfall. Why aren't we using that?"

**What is ruled.** Scope output on is a CW receive condition, set the way the preamp is set,
read back the same way. The detector's watched bin is the peak the radio's scope reports,
offset from the dial by the CW pitch; the sweep stays as the fallback when the scope is
unavailable. The scope on the CW tab draws nothing while no bars are found, draws the bars
as they are when found, and writes each settled character above its bars - a training tool
and the honest view of every miss, invention and wrong letter.

**Whose words are whose.** The rulings are Tim's; the wording is work instruction 480's
record of them.
```

---

## 5. The tasks

### Task 0 - the record and the entry round

`PHASE_OUTCOME.md` gets `## UNIT 480 - STEP 12` from the block at the foot. `PHASE_STATUS.md`
names 480 and `CURRENT_STEP: 12`. Patch-bump. `DECISIONS.md` HM-DEC-188 and the `CLAUDE.md`
row. Entry round: the app carry-forward line.

### Task 1 - the radio's scope output is on in CW (12.4)

Add a receive condition to the CW row - and the CW-family blocks unit 420 made state the CW
conditions - `scopeOutput` wanted **on**, `confirmed: true`, with `wantedText` saying why: the
detector reads the radio's own spectrum. Written by `ReceiverSetup` like the preamp, read
back on one scale, and the operator's hand wins as HM-DEC-056 says. **`RigSpectrumSource`
starts when the read-back says on**, and stops when the mode leaves the CW family.

**Watch it fail first**: `ScriptedRadio` with scope output off, a CW tune-in, and the
assertion that the write went and `RigSpectrumSource.IsRunning` is true after read-back.

**Data modes are untouched**; whatever they do with the scope today, they keep doing.

### Task 2 - the radio points the detector (12.4)

On each `SpectrumFrame`, find the peak within the radio's filter passband around the dial -
`CwPitch` plus and minus half `FilterBandwidth`, mapped onto the frame's bins by its span and
centre. **The station's beat note is the peak's offset from the dial plus `CwPitch`.** Hand
that pitch to `CwEnvelopeDetector` as its watched bin.

- **While the scope is delivering frames**, the detector watches the pointed bin and does not
  sweep. Its bars, its contrast, its pitch all come from that bin.
- **When no frame has arrived for 3 seconds** (`ScopeFlow.QuietAfter`), the detector falls
  back to its own sweep as today, and the tone line on the tab says *"scope quiet, sweeping"*.
- **The tracker follows the same pitch** the moment the scope points, the way 477 made it
  follow the meter - the meter's pitch and the scope's peak are both candidates, and when both
  are present the scope wins, because it is the calibrated one. Say so in the tracker's remark.

**Watch it fail first**: a synthetic `SpectrumFrame` with a peak 250 Hz above centre and
`CwPitch` 600; red while the detector's watched bin is elsewhere, green at 850. **No
recording.**

Add to the verdict row: `scopePeakHz` and `scopePeakDb` from the radio's frame, and
`scopeFramesLast4s`. Key set asserted closed.

### Task 3 - bars only, letters over them (12.2 rewritten, R95)

`CwScopeControl` becomes the training graph:

- **Nothing is drawn while no bars are found.** No trace, no line. The area is empty and says
  *"listening"* in small words.
- **When bars are found, draw them**, sized as they are, scrolling left over the last **eight
  seconds** - long enough for a word. Gaps are empty space of their true length. **No trace
  above them**; the bars are the picture.
- **Each character the decoder settles is written above the bars that made it**, centred over
  their span, in a size the owner can read from his chair. Sure characters in the terminal's
  sure style, dim in its dim style, a placeholder as the placeholder glyph. **A character is
  written only where the decoder settled it and only over its own span** (§0.0). A word gap
  gets a wider empty space, and nothing else.
- **If a settled character's span does not line up with any bars** - the decoder read
  something the bars did not see - draw it anyway, above empty space, so the invention is
  visible.
- Hover on a bar: its length in ms and whether it read as a dit or a dah. Hover on a letter:
  its class and confidence.

**The timing.** The decoder's settled characters carry a span; the bars carry hop indices.
Map both onto the same eight-second axis at the hop rate. **If a settled character carries no
usable span, say so in section 4 and draw it at the settle time** - that is a finding for the
decoder, not a reason to skip the graph.

**Watch it fail first**, headless: a driven detector with bars for dah-dit-dah-dit and a
driven settle of `C` over that span; red while the trace is drawn or the letter is missing,
green when the four bars have `C` above them and nothing else on the canvas.

### Task 4 - the exit round

`Hamlet.sln` builds with warnings as errors. The app carry-forward line. Every type touched
that reads no recording; `ReceiverSetup`'s types with `ScriptedRadio`. `src\Hamlet.RadioEngine`
diff against entry names every file with one line each. Transmit files print nothing against
`7e209cb4`. **No recording was read.**

---

## 6. Do not

- Do not sweep for pitch while the radio's scope is delivering frames. The radio points.
- Do not draw a trace, a floor, a threshold or noise. Bars, and letters over them.
- Do not draw a letter the decoder did not settle, or anywhere but over its own span.
- Do not change what any other receive condition asks for.
- Do not touch data modes' use of the scope.
- Do not read, run or measure against any recording.
- Do not touch what keys or transmits.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 7. Report

`output.md` at the root, four headings exactly.

```
READ IN THIS ORDER.

A. What the owner sees now: nothing on silence, bars when keying, letters
   over the bars - and where the radio's scope said the signal was.
B. Step 12: 12.4 the scope output on and pointing; 12.2 the training graph.
C. The rest. Section 4 raises <n> items, none blocking. No recording was read.
```

```
UNIT:       480 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     scope output: off -> on in CW; the detector's bin: swept -> pointed by the radio; the graph: trace -> bars with letters; recordings read: 0
```

**Section 2 tells the owner:** rebuild, tune a station, and the radio's own scope now tells
Hamlet where it is. Watch for bars appearing when you hear keying and nothing when you don't,
and letters riding above the bars. Press the buttons. And that if the graph shows bars with
no letters, the decoder is the next thing; if letters with no bars, it is inventing.

---

```
ARBITER-DECISION
STEP: 12
APPROACH: turn on the IC-7300's scope output as a CW receive condition and read its 0x27 waveform through RigSpectrumSource, point the envelope detector and the tracker at the peak the radio reports within the filter, and redraw the CW tab's scope as bars only with each settled character written above the bars that made it
MOVE: continue
WHY: PHASE_PLAN.md step 12 criterion 12.4 asks that the detector drive the decoder judged by the owner's ear and his verdict rows, and seven rows tonight show the meter reading the loudest noise bin at scores near zero on two stations the owner heard plainly, while every capture sheet this week has said the radio's scope output is off and the tree already parses it
STATE: partial
DECIDED: the peak-finding within the frame, the fallback rule when the scope is quiet, the graph's eight-second window and its letter placement are the author's, overrulable
LICENCE: PHASE_PLAN.md R67, R74, R88, R91, R94, R95, section 6, step 12; HM-DEC-188; HM-DEC-056; CLAUDE.md 0.0, 0.2 and 0.6; HM-DEC-155; FACT-006
ACCOMPLISHED: the calibrated instrument the radio already has points Hamlet at the signal, and the owner watches bars with letters over them - a picture of every miss and every invention, and a way to learn the code by watching it
ADVANCES: step 12 criterion 4
END-ARBITER-DECISION
```
