# Work instruction 475 - the swing bar the owner's ear says is wrong

**One unit, by hand or by seed - not a loop.** Seven verdict rows from the owner at the radio
on 2026-09-28 say which gate refuses real stations. This unit moves that one number, and
nothing else, so the owner can press again and say whether it helped. Three tasks.

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
carry-forward line, a metric or the keyed set. Entry and exit rounds: **the app carry-forward
line and the types this unit touches.** If any touched type reads a recording, do not run it,
and say so.

**HM-DEC-155.** No suite. Named types only, one per invocation, own `timeout`. Never
background and poll. The app line loses names to the dispatcher loop; re-run once, count
neither way.

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Scripts go in
`.run-unit\unit475-<name>.sh`, run with `sh`.

**By hand:** take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end,
write nothing to `RUN_LEDGER.md`, touch nothing under `tools\arbiter\`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. `UNIT:` line without brackets.
`ADVANCES: step 11 criterion 4`. **Nothing in section 4 halts this unit.**

---

## 2. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The light agrees with the owner's ear more often, by moving the
            one gate his rows say is wrong.
ADVANCES:   step 11 criterion 4
```

**The owner's seven verdict rows, 2026-09-28, 01:14 to 01:17 UTC, 7.020 and 7.054 MHz, AGC
FAST, preamp 1, every one.** Category `cw`, event `owner_verdict`, in
`%AppData%\Hamlet\telemetry\2026-09-28.jsonl`. **Read them from that file, not from here.**

| time | verdict | light | meter verdict | score | median ms | swing dB | survey |
|---|---|---|---|---|---|---|---|
| 01:14:56 | idiot | dark | no keying | 0.02 | 2 | 18.7 | nothing |
| 01:16:01 | agree | lit | keying | 0.14 | 8 | 18.4 | nothing |
| **01:16:22** | **idiot** | **dark** | **no keying** | **0.27** | **48** | **18.6** | **nothing** |
| 01:16:28 | agree | lit | keying | 0.19 | 9 | 20.4 | nothing |
| 01:16:56 | agree | lit | keying | 0.37 | 50 | 21.1 | nothing |
| 01:17:19 | agree | lit | keying | 0.08 | 4 | 21.8 | nothing |
| 01:17:37 | agree | lit | keying | 0.17 | 6 | 17.5 | nothing |

**What the rows say.**

- **01:16:22 is the finding.** The owner heard code. Score 0.27 is nearly three times the
  0.10 bar. Median 48 ms is a clean dit near 25 WPM, dead centre of the 25 to 250 ms window.
  The only test that failed was the swing: **18.6 dB against `ConfidentSwingDb` of 20.** A
  real station, keying plausibly, refused for 1.4 dB.
- **The swing never left the high teens or low twenties on any row.** 17.5 to 21.8. Every row
  is under AGC FAST, which the CW receive condition sets, and which flattens level swing by
  design. The 20 dB bar was fitted to seven recordings that swung 21 to 91 - strong bulletins
  - and to two empty ones at 13 and 17 (`CwKeyingMeter.cs` remarks at 113 to 125). **It does
  not survive ordinary strong stations on 40 m under the app's own AGC setting.**
- **The survey admitted nothing on all seven.** Its 3 dB hysteresis is the same kind of gate
  one stage earlier. So the tracker had no candidate, six of seven rows read *mixing ...
  assumed*, and the decoder was reading 600 Hz regardless of where the station was.
- **The agreed rows with medians of 4, 6, 8 and 9 ms are not dits.** The meter said keying on
  noise, held through its quiet-window rule, while the owner heard a station somewhere in
  the passband. The light was right by accident. That is a second finding for section 4, not
  for this unit.

**The owner's ruling, R89, 2026-09-28:** *"write it."* The swing bar moves to where his rows
put it. One number.

---

## 3. Verify against the tree

- `CwKeyingThresholds.ConfidentSwingDb` is 20, at `CwKeyingMeter.cs` around line 131, and
  its remarks say where 20 came from.
- `CwToneSurvey.HysteresisDb` is 3.0, around line 179.
- The verdict rows exist in the telemetry file named above, seven of them, and their figures
  match the table. **If they differ, the file wins; say so.**
- Which tests pin either number, and whether any of them reads a recording.

## 4. Rulings in force

`PHASE_PLAN.md` R77 to R89 and §6. **R88** the corpus is banned; steps 2 to 10 closed;
step 11 only. **R89** the swing bar is set from the owner's rows. **§0.0** the remark on the
constant says what it now rests on. **§0.2** nothing that keys or transmits. **HM-DEC-155,
HM-DEC-165, FACT-006.**

---

## 5. The tasks

### Task 0 - the record and the entry round

`PHASE_OUTCOME.md` gets `## UNIT 475 - STEP 11` from the block at the foot. `PHASE_STATUS.md`
names 475 and `CURRENT_STEP: 11`. Patch-bump. **Tick 11.1, 11.2 and 11.3 in both copies of
`PHASE_PLAN.md` from unit 474's report** - its report was refused by the validator over its
ordering block, not its work, and the light, the strip and the buttons are in the tree and on
the owner's screen; name 474's test counts beside each tick. Entry round: the app
carry-forward line.

### Task 1 - the swing bar (11.4)

Change `ConfidentSwingDb` from 20 to **17**. Seventeen clears every row the owner agreed
with (17.5 and up), catches the refused station at 18.6, and stays above the two empty
recordings the old remark cites at 13 and 14 - and only just above the 17.7 one, which is the
honest edge and is named in the remark.

**Rewrite the constant's remark** to say what it now rests on: seven owner verdicts at the
radio on 2026-09-28 under AGC FAST, the refused station at 18.6 with score 0.27 and median
48 ms, and that the previous 20 came from recordings and did not survive the air. Keep the
old figures in the remark as history. **Do not delete them.**

**Watch a test fail first**: a fact over `CwKeyingMeter` fed a synthetic profile with score
0.27, median 48 ms, swing 18.6 dB asserting the verdict is keying. Red at 20, green at 17.
**No recording.** If an existing test pins 20 and reads no recording, update its expectation
and say so; if it reads a recording, do not run it, and list it in section 4.

### Task 2 - the survey's hysteresis (11.4, second half)

**Do not change it in this unit.** Print, from the source, exactly what `HysteresisDb` gates
and what a 3 dB swing requirement means under AGC FAST beside a meter that now accepts 17.
One paragraph in the report, so the owner's next set of rows can say whether the survey is
the next gate. If the owner presses "You're an idiot" with the meter saying keying and the
survey still admitting nothing, the next unit moves this one.

### Task 3 - the exit round

`Hamlet.sln` builds with warnings as errors. The app carry-forward line. Every type touched
that reads no recording. `src\Hamlet.RadioEngine\Cw` diff against entry shows **one constant
and its remark, and nothing else.** The transmit files print nothing against `7e209cb4`.
**No recording was read**, and the report says so.

---

## 6. Do not

- Do not change any number but `ConfidentSwingDb`. The survey's 3 dB, the score's 0.10, the
  25 and 250 ms, the 300 and 900 Hz - all named, all left.
- Do not read, run or measure against any recording.
- Do not touch the light, the strip, or the buttons.
- Do not touch what keys or transmits.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 7. Report

`output.md` at the root, four headings exactly.

```
READ IN THIS ORDER.

A. The one number that moved, from what to what, and the row that moved it.
B. Step 11: 11.4 - which gate the owner's rows named, and what was done.
C. The rest. Section 4 raises <n> items, none blocking. No recording was read.
```

```
UNIT:       475 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     ConfidentSwingDb 20 -> 17; owner rows refused at the old bar and accepted at the new: <n> of 7; recordings read: 0
```

**Section 2 tells the owner:** rebuild, tune the same stations, press again; what changed is
one gate, and the light should now agree with him on the 01:16:22 kind of station. And that
if the light now lights on nothing, the score gate is the next thing his rows will show.

**Section 4 states two facts:** the survey's hysteresis is the same kind of gate and admitted
nothing on all seven rows; and four agreed rows had medians of 4 to 9 ms, which are not dits,
so the meter's score gate lets noise through.

---

```
ARBITER-DECISION
STEP: 11
APPROACH: move CwKeyingThresholds.ConfidentSwingDb from 20 to 17 on the owner's seven verdict rows, rewrite its remark to say so, watch a synthetic-profile test fail first, and change nothing else
MOVE: continue
WHY: PHASE_PLAN.md step 11 criterion 11.4 asks that the owner's verdict rows be read and that the detector's gate which disagreed with his ear be named from them; his row at 01:16:22 names the swing bar - score 0.27, median 48 ms, swing 18.6 against 20
STATE: partial
DECIDED: seventeen, from the rows, is the author's and overrulable; the survey's hysteresis is described and not moved
LICENCE: PHASE_PLAN.md R85, R88, R89, section 6, step 11; HM-DEC-184; CLAUDE.md 0.0 and 0.2; HM-DEC-155; FACT-006
ACCOMPLISHED: the light stops refusing a plausibly keyed strong station for 1.4 dB of swing the radio's own AGC took away
ADVANCES: step 11 criterion 4
END-ARBITER-DECISION
```
