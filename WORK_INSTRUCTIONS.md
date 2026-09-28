# Work instruction 478 - the scope is the middle picture

**One unit, after 477. Seed it and drop STOP, or run it by hand.** It changes what is drawn,
not what is detected. The oscilloscope becomes the picture that convinced the owner: the level
trace, and bars along the bottom where the run detector calls a bar. The temporary light and
strip from unit 474 come out. Three tasks.

**The owner's ruling, 2026-09-28, R92:** *"We should have replaced the temp controls with
something that looks like #2"* - the middle panel of the bars-not-waves picture: a keyed
station's trace with flat tops and flat bottoms, and the dits and dahs marked underneath.

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
the types this unit touches.

**HM-DEC-155.** No suite. Named types only, one per invocation, own `timeout`. Never
background and poll. The app line loses names to the dispatcher loop; re-run once, count
neither way.

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Scripts go in
`.run-unit\unit478-<name>.sh`, run with `sh`.

**By hand:** take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end,
write nothing to `RUN_LEDGER.md`, touch nothing under `tools\arbiter\`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. `UNIT:` line without brackets.
`ADVANCES: step 12 criterion 2`. **Line C names how many items section 4 raises.** Nothing in
section 4 halts this unit.

---

## 2. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The CW tab shows the owner what the detector sees, as the
            picture he asked for: a trace, and bars where the keying is.
ADVANCES:   step 12 criterion 2
```

**What the owner looks at now, and why it is wrong.** Unit 474 put a light, a pitch strip and
two buttons on the tab; unit 476 put a scope under them with a floor line and a threshold
line. Unit 477 changed what decides keying - bars, not a floor and a margin - but the scope
still draws floor and threshold, which is the old idea with new labels. The owner was
convinced by a picture with none of that: **a level trace, and bars along the bottom.**

**Unit 477 must be in the tree before this runs.** Its run detector is what the bars come
from. If `CwEnvelopeDetector` still carries a floor tracker and a margin constant, stop at
task 1 and say so - this unit draws 477's output; it does not build a detector.

---

## 3. Verify against the tree

- `CwEnvelopeDetector` after 477: per hop, the found bin's level, mark-or-gap by the bar test,
  run length, the pitch and its contrast, and marks in the last four seconds. **Name what it
  exposes; the scope draws only that.**
- `CwScopeControl`, `CwPitchStripControl`, `CwHearingViewModel`: what each draws and where the
  CW tab hosts them. The light and the strip come out; the buttons and the verdict row stay.
- `EveryControlSaysWhatItDoesTests` and the CW tab's closed control list: the light and the
  strip are removed from it, the scope and the buttons stay, and the test is updated to match.
- The verdict row's key set: **unchanged.** The scope's fields already ride on it from 476.

## 4. Rulings in force

`PHASE_PLAN.md` R77 to R92 and §6. **R88** the corpus is banned; steps 11 and 12 only.
**R91** bars decide keying. **R92** the scope is the middle picture. **§0.0** what is drawn
is what was measured; no line the detector does not compute. **§0.6** color never the sole
carrier - bars carry a word on hover and the tone line is text. **§0.2** nothing that keys or
transmits. **HM-DEC-155, HM-DEC-165, FACT-006.**

No decision record: R92 is a display ruling under R90 and R91, already recorded.

---

## 5. The tasks

### Task 0 - the record and the entry round

`PHASE_OUTCOME.md` gets `## UNIT 478 - STEP 12` from the block at the foot. `PHASE_STATUS.md`
names 478. Patch-bump. Entry round: the app carry-forward line.

### Task 1 - the scope becomes the middle picture (12.2 rewritten)

`CwScopeControl` draws, over the last **four seconds**:

- **the trace**: the found bin's level, hop by hop, as one line - and when no bin is found, the
  loudest bin's level, so the trace never goes blank;
- **the bars**: a filled block along the bottom under every hop the run detector calls a
  mark, contiguous, so a dit is a short block and a dah a long one; **nothing under a gap**;
- **the pitch, as text in a corner**: *"tone 742 Hz"* while keying, *"no keying"* otherwise;
  beside it *"mixing 742 Hz"* from the tracker, so the owner sees the two agree or not;
- **the passband edges**: not drawn. The trace is one bin; the edges belong to a spectrum
  view, not to this.

**Removed from the scope:** the floor line, the threshold line, the margin label, the shaded
passband. **The detector's outputs are unchanged**; the scope stops drawing three of them.

Hover on the trace: *"the level of the bin the detector is reading, over the last four
seconds"*. Hover on a bar: *"a mark - the level held flat for at least a dit"*. Words for
everything (§0.6).

Fed at the decode hop rate as 476 was. **Watch it fail first**, headless, with a driven
detector: red while a floor or threshold line is drawn, green when only the trace, the bars
and the two text lines are.

### Task 2 - the temporary controls come out (11.1, 11.2 retired)

Remove the light and the pitch strip from the CW tab. **Keep the two verdict buttons and the
verdict row exactly as they are** - they are the owner's ear talking to the record and they
stay until he says otherwise. Move the buttons beside the scope.

Update the closed control list and `EveryControlSaysWhatItDoesTests`. Update
`CwHearingViewModel` to drop what the light and the strip read, and nothing else - **the
verdict row's fields are unchanged**, including `light`, which now records the detector's
keying verdict in words so the row's key set stays closed and the next unit can still read
it.

In `PHASE_PLAN.md`, both copies, 11.1 and 11.2 get one appended clause: *"retired by unit 478
under R92; the scope shows what they showed"*. **Do not untick them.**

### Task 3 - the exit round

`Hamlet.sln` builds with warnings as errors. The app carry-forward line. Every type touched.
`src\Hamlet.RadioEngine\Cw` diff against entry prints nothing - **this unit changes the app,
not the engine.** Transmit files print nothing against `7e209cb4`. **No recording was read.**

---

## 6. Do not

- Do not change `CwEnvelopeDetector`, `CwKeyingMeter` or `CwToneTracker`. 477 owns them.
- Do not draw a floor, a threshold, a margin or a passband on the scope.
- Do not remove the buttons or change the verdict row's key set.
- Do not read, run or measure against any recording.
- Do not touch what keys or transmits.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 7. Report

`output.md` at the root, four headings exactly.

```
READ IN THIS ORDER.

A. What the CW tab shows now, in one paragraph: the trace, the bars, two
   words of pitch, two buttons.
B. Step 12: 12.2 rewritten; step 11: 11.1 and 11.2 retired.
C. The rest. Section 4 raises <n> items, none blocking. No recording was read.
```

```
UNIT:       478 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     lines on the scope: trace and bars, 0 thresholds; controls removed: light, strip; recordings read: 0
```

**Section 2 tells the owner:** rebuild, tune a station, and look at one thing - do the bars
under the trace match the dits and dahs you hear. Press the buttons as before.

---

```
ARBITER-DECISION
STEP: 12
APPROACH: redraw the scope as the level trace with bars along the bottom where the run detector calls a mark, drop the floor and threshold lines, retire the light and the pitch strip, and keep the verdict buttons and row unchanged
MOVE: continue
WHY: PHASE_PLAN.md step 12 criterion 12.2 asks that the CW tab draw the last four seconds as an oscilloscope with marks as bars along the bottom, fed fast enough that the owner sees dits, and R92 rules that it look like the picture that convinced him - a trace and bars, nothing else
STATE: partial
DECIDED: the trace's fallback bin when nothing is found, the bar's exact drawing, and the placement of the buttons beside the scope are the author's, overrulable
LICENCE: PHASE_PLAN.md R88, R90, R91, R92, section 6, step 12; CLAUDE.md 0.0, 0.2 and 0.6; HM-DEC-155; FACT-006
ACCOMPLISHED: the owner watches the detector see what he hears, in the one picture he said he understood
ADVANCES: step 12 criterion 2
END-ARBITER-DECISION
```
