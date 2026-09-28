# Work instruction 479 - the tolerance follows the signal

**One unit. Seed it and drop STOP, or run it by hand.** One constant becomes a formula, one
constant moves, and three ticks the web session overwrote are restored. Three tasks.

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
`.run-unit\unit479-<name>.sh`, run with `sh`.

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
UNIT GOAL:  The bars find the stations the owner hears, at the loudness
            they actually arrive at.
ADVANCES:   step 12 criterion 4
```

**What unit 477 chose, in its own words** (its report is
`.run-unit\reports\unit-1-output-27.md`; its remark is on the constant):
`FlatToleranceDb = 1.5`, chosen so that seeded loud noise read keying in **0 of 1500 reads**,
and **"what it costs, named: a tone keyed 15 dB over the noise ... wobbles past one and a
half across a long dah often enough that 51 of its 208 key-down hops are not marked - the
bar splits. Unit 476's test of that case is red on this and is left red."** Its own formula:
a tone S dB over its bin's noise wobbles by **20·log10(1 + 10^(-S/20))** - 1.4 dB at 15 dB
over, 2.4 dB at 10 dB over.

**What the owner's rows say, 2026-09-28, 15:38 to 15:39 UTC.** Four stations he heard, four
*idiot* verdicts, zero bars on every one. Their envelopes sat 10 to 15 dB lower than the
morning's sweet spot, and the meter's swing read 15 to 20 dB. **Every one is inside the range
the constant's own remark says it breaks on.** A fixed 1.5 dB admits only signals wobbling
less than 1.5 - which by 477's own formula is signals more than about 14 dB over their gaps.
That is the 20 dB swing gate again, in new clothes.

**The meter went blind on the same rows** - swing 15.1 to 19.7 against `ConfidentSwingDb` of
17, medians 3 to 9 ms - because 477's task 3 was dropped and the meter kept its own gate.
The lowest swing on a station the owner heard and pressed *idiot* on is **15.1**.

**The owner's ruling, 2026-09-28, R93:** the tolerance follows the signal. A loud bar may
wobble little and a weak bar wobbles more; one number for both is a gate against the weak
ones. *"Just do it."*

---

## 3. Verify against the tree

- `CwEnvelopeDetector.FlatToleranceDb` is 1.5, and where a run's membership is tested against
  it; `ContrastDb`, `BarDb` and `GapDb` are computed per bin per hop, so the bar-over-gap
  contrast is available when the tolerance is applied.
- `CwKeyingThresholds.ConfidentSwingDb` is 17 (unit 475), with its remark.
- Unit 476's `AToneFifteenDecibelsOverTheNoiseStillKeys` exists and is red at HEAD, and reads
  no recording.
- `PHASE_PLAN.md`, both copies: 12.1, 12.2 and 12.3 unticked, though 477's report ticked them.

## 4. Rulings in force

`PHASE_PLAN.md` R77 to R93 and §6. **R88** the corpus is banned; steps 11 and 12 only.
**R91** bars decide keying. **R93** the tolerance follows the signal. **§0.0** every number's
remark names its reason. **§0.2** nothing that keys or transmits. **HM-DEC-155, HM-DEC-165,
FACT-006.**

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-28, headline **The flatness tolerance follows the signal's contrast**,
ref HM-DEC-187:**

```
---
id: HM-DEC-187
date: 2026-09-28
refs: PHASE_PLAN.md R93 and criterion 12.4, CwEnvelopeDetector.cs FlatToleranceDb, CwKeyingMeter.cs ConfidentSwingDb, unit 477's report, the owner's verdict rows of 2026-09-28 15:38, work instruction 479
---

**A run's flatness tolerance is the wobble a tone at the bar's measured contrast actually
has, not one number for every signal; and the meter's swing bar is the lowest swing on a
station the owner heard.** Tim, 2026-09-28.

**What was wrong.** Unit 477 set the tolerance at 1.5 dB so that seeded noise never read as
keying, and named the cost: a tone 15 dB over the noise splits its bars. The owner then
pressed "You're an idiot" on four stations 10 to 15 dB weaker than the morning's, and the
bars found none of them. The meter, keeping its own 17 dB swing gate because 477's task 3 was
dropped, found none of them either at swings of 15 to 20.

**What is ruled.** The tolerance is 477's own formula applied to the measured contrast -
20·log10(1 + 10^(-S/20)) for a bar S dB over its gap - with 1.5 dB as its floor for loud
signals and no ceiling, so a weak bar is allowed the wobble a weak bar has. The meter's
`ConfidentSwingDb` moves from 17 to 15, the lowest swing on a station the owner heard.
Neither number rests on a recording; both rest on the physics 477 wrote down and the owner's
rows. The three ticks 477 earned and the web session's plan delivery erased are restored.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 479's record
of it.
```

---

## 5. The tasks

### Task 0 - the record, the ticks, the entry round

`PHASE_OUTCOME.md` gets `## UNIT 479 - STEP 12` from the block at the foot. `PHASE_STATUS.md`
names 479 and `CURRENT_STEP: 12`. Patch-bump. `DECISIONS.md` HM-DEC-187 and the `CLAUDE.md`
row. **Tick 12.1, 12.2 and 12.3 in both copies of `PHASE_PLAN.md` from units 477 and 478's
reports** - the web session's 478 plan delivery was built from a pre-477 copy and erased them;
name each report's test counts beside the tick. Entry round: the app carry-forward line.

### Task 1 - the tolerance follows the contrast (12.1, 12.4)

In `CwEnvelopeDetector`, a run's flatness tolerance becomes a function of the bar's measured
contrast over its gap: **`tolerance(S) = max(FlatToleranceDb, 20·log10(1 + 10^(-S/20)))`**,
S in dB. `FlatToleranceDb` stays at 1.5 as the floor and its remark says so. **Where S is not
yet known** - the first bar of a station, before a gap has been measured - use the bin's
level over the loudest gap seen in the last second, or if none, over the bin's own minimum
in the last second; say which in the report.

**Watch it fail first:** unit 476's `AToneFifteenDecibelsOverTheNoiseStillKeys`, red at HEAD.
Green when the 15 dB tone's long dahs are unbroken bars. **Then the noise test 477 wrote** -
seeded loud noise, 1500 reads - stays at its count or the report says by how much it rose and
why that is the physics and not a leak: noise at a bin does not hold a level, whatever the
tolerance, because it has no contrast to earn one. **Then a 10 dB tone**, a new synthetic:
bars unbroken at 2.4 dB of allowed wobble.

Rewrite the constant's remark: the floor, the formula, the reason, the owner's four rows, and
that no recording chose it.

### Task 2 - the meter's swing bar (12.4)

`ConfidentSwingDb` from 17 to **15**. The remark: 475 set 17 from seven rows; 479 sets 15 from
four more, the lowest swing on a station the owner heard being 15.1; and that the meter's
swing test is the gate 477's task 3 was to replace and did not. **Watch it fail first**: a
synthetic meter profile with score 0.20, median 45 ms, swing 15.5 - red at 17, green at 15.

### Task 3 - the exit round

`Hamlet.sln` builds with warnings as errors. The app carry-forward line. Every type touched
that reads no recording. `src\Hamlet.RadioEngine\Cw` diff against entry: `CwEnvelopeDetector`
and `CwKeyingMeter` only, and the report says what each change is. Transmit files print
nothing against `7e209cb4`. **No recording was read.**

---

## 6. Do not

- Do not pick the tolerance by looking at the owner's rows. The formula is 477's physics; the
  rows are the reason to apply it.
- Do not raise the floor above 1.5 or add a ceiling.
- Do not touch the tracker, the survey, the scope's drawing, the buttons or the verdict row.
- Do not read, run or measure against any recording.
- Do not touch what keys or transmits.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 7. Report

`output.md` at the root, four headings exactly.

```
READ IN THIS ORDER.

A. The tolerance as a formula, and the three synthetic results: the 15 dB
   tone, the 10 dB tone, the noise count.
B. Step 12: 12.1 to 12.3 re-ticked, 12.4 moved by two changes.
C. The rest. Section 4 raises <n> items, none blocking. No recording was read.
```

```
UNIT:       479 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     tolerance: 1.5 fixed -> max(1.5, wobble at measured contrast); ConfidentSwingDb 17 -> 15; 15 dB tone's dah hops unmarked: 51 -> <n>; recordings read: 0
```

**Section 2 tells the owner:** rebuild, tune the weaker stations you pressed *idiot* on this
afternoon, and look for bars. And that if noise now shows bars where there's nothing, that is
the one thing to press *idiot* on.

---

```
ARBITER-DECISION
STEP: 12
APPROACH: make the run detector's flatness tolerance the wobble a tone at the bar's measured contrast has, floored at 1.5 dB, so weak stations make unbroken bars; move the meter's swing bar from 17 to 15, the lowest swing on a station the owner heard; restore the three ticks 477 earned
MOVE: continue
WHY: PHASE_PLAN.md step 12 criterion 12.4 asks that the detector drive the decoder judged by the owner's ear and his verdict rows, and four rows show the bars and the meter both blind on stations 10 to 15 dB over the noise, inside the range unit 477's own remark says its fixed tolerance breaks on
STATE: partial
DECIDED: how contrast is estimated before a station's first gap is measured, and the per-type timeouts, are the author's, overrulable
LICENCE: PHASE_PLAN.md R88, R91, R93, section 6, step 12; HM-DEC-187; CLAUDE.md 0.0 and 0.2; HM-DEC-155; FACT-006
ACCOMPLISHED: the stations the owner hears at ordinary loudness make bars, and the meter finds them, without noise sneaking in
ADVANCES: step 12 criterion 4
END-ARBITER-DECISION
```
