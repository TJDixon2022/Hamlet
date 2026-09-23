# Work instruction 409 - the decoder and the sweep agree about the pitch

**Authored by the arbiter.** This unit is 3.8, the last loop criterion of the phase. First it
tables, per case, the pitch the decoder mixes at beside the pitch the independent sweep reports.
Then it changes `CwToneTracker` only where that table shows the two more than one 25 Hz bin
apart, under R56. Last, it measures #15, #43 and #44 on the tree that results. **Five tasks,
drop from the back.**

**Status.** Run `sh tools/status.sh` on the real clock after every commit and every task, and
immediately before every `dotnet test`. **Write files as UTF-8.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Run only this unit's names and `docs\carry-forward-tests.txt`, as the
file's top comment says. **Never background and poll.** One type per invocation, each with its
own `timeout`. A run lost before any assertion is re-run once and counted neither way. A red on
an assertion is red.

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`. Write
*tasks 0 to 4, none dropped* with commas.

**`TheIntegratorBandwidthTable.Write` runs 362 s and `OneDecoderNotTwoTests` will not fit a
600 s call.** Neither is in this unit's set. **`TheGateHasItsOwnWindowNowTests` did not fit 590 s
in unit 406.** Run only its two short methods, by name.

## 2. The tool facts

- Apostrophes in quoted heredocs break.
- Doubled backslashes collapse.
- `;` is refused, and so is `rm`.
- Python cannot run here.
- A multi-line commit needs `-m` more than once.
- A bare `git worktree`, `git checkout` or `git show` is refused at the prompt.

Multi-step commands go into `.run-unit\unit409-<name>.sh` and run with `sh`.

## 3. Asks still outstanding

None. Unit 408's section 4 raised nothing that blocks a criterion. Its three findings are 408
items 1 to 3 in `docs\phase-cw\PARKED.md`, which the arbiter reads as parked under R54.

---

## 4. Why this unit exists

```
PHASE GOAL: CW decodes again.
UNIT GOAL:  Measure, per case, the pitch the decoder mixes at against the
            pitch the independent sweep reports; move the tone tracker only
            where they are more than one 25 Hz bin apart; measure #15, #43
            and #44 on the result.
ADVANCES:   step 3 criterion 8
DRIFT:      0 consecutive units without advance
```

**The count today.** Step 3 has 3.1 to 3.7 and 3.9 ticked, and 3.8 is not started. Steps 1, 2
and 4 are met on every criterion, and so is step 0. Step 5 is Tim's verdict. **3.8 is the one
criterion left that the loop can move.** Once it has a verdict, the phase waits on Tim alone.

**Why the tracker, and why measured this way.** Unit 405 traced every miss on a single sender to
`CwToneTracker.Switch`. The switch moves the mix 25 to 85 Hz off the sender and reports the wrong
pitch as measured: 700 for 640, 575 for 615. Units 406 and 407 changed the switch to turn 3.6's
reds green, and they judged those changes on the reds. H1 cost five captures. H2 was kept. The
still-keying property did not separate the moves. **Nobody has yet asked the question 3.8 asks:
across every real recording, does the decoder listen where an instrument that shares nothing with
it hears the keying?** The table is the new judge. A change is kept only if the table improves.

---

## 5. Verify this instruction against the tree

Check each of these, report any mismatch, and repair nothing:

- **Which sweep is independent.** `src\Hamlet.RadioEngine\Cw\KeyingEnvelope.cs` has
  `Best(MonoAudio)`, which picks the highest-scoring candidate, and `Measure(MonoAudio, double)`,
  which is public. Its range is `CwToneTracker.MinimumToneHz` to `MaximumToneHz`, **300 to 900**
  at HEAD, in 25 Hz steps. **The "400 to 1200" that 3.8 quotes is the app's label**, at
  `src\Hamlet.App\ViewModels\MainWindowViewModel.cs` line 11916, and the label no longer matches
  the engine. That is a mismatch: report it and do not repair it.
- **What the decoder mixes at.** Find the property that tells you, and say which property you
  used. `CwToneTracker.LockedToneHz` at line 279 is the candidate.
- **The three anchors' recordings**, `tests\fixtures\cw\captured\*.wav`, and which of the 37
  capture rows they are.
- `tests\fixtures\cw\captured\unadjudicated\cw-2026-09-23-173723.wav` and its `.key.md` are
  present. The `.txt` sidecar is absent, as 408 item 1 says.
- The recipe pitch of #15, #43 and #44, from their fixtures or their generators.
- **`PHASE_OUTCOME.md` and `PHASE_STATUS.md` show step 0 as `not started` and step 2 as `not
  started`, though every criterion of both is ticked in the plan.** `PHASE_STATUS.md` also reads
  `CURRENT_STEP 0`. Report the mismatch and do not edit the step lines. Set `CURRENT_STEP` to 3,
  as every unit has.

**Expected failures:** none. At entry the captures are 37 of 37, adjudicated 13 of 13, the clean
synthetics 2 of 2, `CwReceiverFixtureTests` 25 of 27 and `CwAcquisitionWindowTests` 11 of 12.
Any other number is a mismatch, and the report leads with it.

## 6. Rulings in force

`PHASE_PLAN.md` R47 to R58 and §6. Read them before task 1. **Do not re-argue them.** These are
the ones this unit leans on, in full:

**R56** - *the tone tracker may be attacked in this phase.* Unit 405 traced one cause under four
of 3.6's reds: `CwToneTracker.Switch` moves the mixdown 25 to 85 Hz off a single sender and
reports the wrong pitch as measured (700 for 640, 575 for 615). The same disagreement appears on
the air in `cw-2026-09-23-173723`, where the decoder says 600 Hz and the independent sweep says
575 in the same capture. HM-DEC-095 and HM-DEC-127 still govern that code and are not overruled.
What R56 grants is leave to change the switch inside this phase, with the floors, the adjudicated
anchors and the 17:37 key as the guard.

**R57** - a floor is the count of named characters. A row that falls solely because placeholders
were suppressed is not a floor lowered. A row whose named count falls is a regression, and the
change goes back out.

**R58** - the gate first, then the tracker. 3.6 closed on unit 408's pass, and **no unit attacks
a 3.6 red after it.** #15, #43 and #44 are parked. This unit measures them and does not aim a
change at them.

**§6, the tracker line** - a change to `CwToneTracker` is licensed by R56 and judged by 3.8's
per-case table, the floors' named counts, the three anchors unchanged, and the 17:37 key's scored
region. **A tracker change that improves the table and costs a single anchor character goes back
out.**

**§6, the 17:37 line** - the 17:37 key is inferred, not transcribed. Every number measured against
it is stated as *against an inferred key*, and the unscored first third is never keyed.

**R49** - a test that reads audio and asserts characters, elements, a tone or a speed is never
retired.

**§3** - the eleven CW transmit files are read and never written. A change to one of them is
`MOVE: stop`.

**HM-DEC-091** - a change that reads one recording and costs another is not a fix. Costs means
named characters (R57).

**HM-DEC-165** - nothing may be red that was green before.

**CLAUDE.md §0.0** - never present a guess as a decode. A pitch agreeing is not a reading being
right.

**§0.2** - nothing that keys is touched.

**FACT-004**, **FACT-006**.

## 7. Status cadence

As the header says. `NOTE` says what is moving inside the task.

---

## 8. The tasks

### Task 0 - the record

- Write the `## UNIT 409 - STEP 3` entry in `PHASE_OUTCOME.md`, in the existing shape, from the
  decision block at the foot of this file.
- `PHASE_STATUS.md` names unit 409, *the decoder and the sweep agree about the pitch*, with
  `CURRENT_STEP 3`.
- Patch-bump `Directory.Build.props`.

**Entry round.** Run both carry-forward lines, the three floor tests, `CwReceiverFixtureTests`,
`CwAcquisitionWindowTests` and `TheSeventeenThirtySevenCaptureTests`. Then run every tracker
reader unit 406 listed by grep, and record every number. If `git diff 0534ca93 HEAD` over src,
tests and the list prints nothing, the two lines may be taken as unit 408's exit, and the report
says so.

**Drop candidate:** none.

### Task 1 - the table at entry (3.8, first half)

Write a printer, `TheTwoPitchesTableTests`, that asserts nothing. No file under `src` changes in
this task. It reads every case the same way the floors read a capture, hop by hop. The cases are
the 37 captures, the three anchors' recordings (tabled once each, with their row marked) and 17:37.
It prints one row per case with these columns:

- **decoder**: the pitch the decoder mixes at. Take the most-held value, time-weighted over the
  hops from the first named character settled to the last. With no named character, take it over
  the whole recording. Also print its lowest and highest value, and the share of those hops held
  more than one bin from the sweep.
- **sweep, 300 to 900**: `KeyingEnvelope.Best` over the whole recording, as the tree has it.
- **sweep, 400 to 1200**: the same selection rule over `Measure` from 400 to 1200 in 25 Hz steps.
- **second-best**: the score of the best candidate more than two bins from the winner, as a
  fraction of the winner's score.
- **keying**: the sweep's verdict under `CwKeyingThresholds`.
- **single-sender**, yes or no, by the rule below.
- **the difference in hertz, and agree or apart.**

**Single-sender** means all three of these hold. The rule is fixed now and is not changed after
the table is read:

- the recording is not a two-station fixture of `CwTwoStationTests`;
- the sweep's verdict is keying;
- the second-best fraction is under 0.5.

**Agree** means the two pitches differ by 25 Hz or less.

**The judgment is on the 300 to 900 column.** That is the one sweep the tree has, and the app
labels it wrong. **Every case where the two sweep columns disagree is named**, together with what
the 400 to 1200 column would have made of it.

Write the table to `docs\phase-cw\unit409-pitch.md` section 2, with the count of single-sender
cases apart at entry. **If none are apart, task 2 makes no change** and says so, and the unit goes
to task 3.

**Drop candidate:** none. Nothing else in the unit can be judged without this table.

### Task 2 - the tracker change (3.8)

For each single-sender case the table shows apart, trace the hop where the mix leaves the sweep's
pitch to the line in `CwToneTracker` that moves it. Unit 406's `TheTrackerSwitchTraceTests` is
the template. **One change per shared cause, up to three**, each built on the last kept one. Each
change is kept only if **all** of the following hold, and is otherwise reverted in the next commit
with its numbers:

- **the table improves**: fewer single-sender cases apart, and no case goes from agree to apart;
- **no capture's named count or named elements fall**;
- **the three anchors are identical on every named character**, and adjudicated is 13 of 13;
- **17:37's scored region reads no worse against an inferred key** than at entry;
- the clean synthetics stay 2 of 2, and every tracker reader and gate type holds its entry count.

**Not to be rebuilt:**

- **H1**, a held switch only if the survey re-admits the held pitch. Unit 406 measured it costing
  five captures.
- **The still-keying property.** Unit 407 measured that it does not separate the moves.

A change on the same property as either is the same change.

**The decoder may not read `KeyingEnvelope`, `CwKeyingMeter` or anything they compute.** A tracker
that asks the sweep makes the table agree by construction, and it ends the sweep's independence,
which is the sweep's whole job. A change of that kind is out, whatever it measures.

**Drop candidate:** the second and third changes. Stop after the first kept or reverted change if
the unit has run four hours.

### Task 3 - the table at exit, and the three parked reds (3.8, second half)

Run `TheTwoPitchesTableTests` again on the tree task 2 left, and write the table beside the entry
table in section 3. Then run #15, #43 and #44 by name. For each, print:

- its test value and whether it is green;
- the decoder's pitch, the sweep's pitch and the recipe's pitch.

**These are the post-R56 measurements.** They are not attacks. No change is aimed at them, and a
change that turns one green is reported as a side effect.

**3.8 is ticked only if** every single-sender case agrees at exit **and** the three are each
measured. Otherwise the criterion stays open, and the report gives the count still apart and names
each case.

**Drop candidate:** none.

### Task 4 - the exit round

Run both carry-forward lines, the three floor tests, `CwReceiverFixtureTests`,
`CwAcquisitionWindowTests`, `TheSeventeenThirtySevenCaptureTests` and every tracker reader from
task 0. Then:

- `git diff` over the eleven transmit files against `7e209cb4` prints nothing;
- `git diff 0534ca93 HEAD` over `src\Hamlet.App` prints nothing.

---

## 9. Parked - do not touch, do not raise

- **#15, #43 and #44 as targets.** They are parked under R58. They are measured in task 3 and
  nothing more.
- **The app's "400 to 1200" label.** Report it and do not edit it. It is an app file and blocks
  nothing.
- **408 items 1 to 3.** These are the missing 17:37 sidecar, the name `CwEmissionGate`, and the
  two spaces.
- **Word spacing and the callsign split at 17:37.** The table may explain them. This unit does not
  chase them.
- **The correctness phase**, its keys and its yardstick. That is Tim's next phase.
- **Step 5.** It is Tim's.

## 10. What not to do

- **Do not lower a named count anywhere** (R57). **Do not change a floor** to make a change fit.
- **Do not change the sweep, its range, or `KeyingThresholds`**, and do not make the decoder read
  the sweep. The instrument does the judging, so it is not tuned.
- **Do not change the single-sender rule or the one-bin tolerance after the entry table is read.**
- **Do not attack a 3.6 red.** **Do not rebuild H1 or the still-keying property.**
- **Do not touch a transmit file**, or anything that keys.
- **Do not call the 17:37 key a transcript**, and do not say a pitch agreeing means a reading is
  right.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches and repair nothing. American spelling. UTF-8.**

## 11. Committing and pushing

Commit per task, and commit each kept tracker change on its own. Each revert is its own commit.
Push at the end and say whether it succeeded.

---

## 12. Reporting

Write `output.md` at the root, with the canonical headings. **The ordering block comes first.
`validate-output.bat` refuses a report without it.**

```
READ IN THIS ORDER.

A. The phase: CW decodes again. Steps 1, 2 and 4 met on every criterion, and
   step 0 too; step 3 open on 3.8 alone; step 5 Tim's verdict. Say whether
   the phase now waits on Tim alone.
B. Step 3, criterion 3.8: single-sender cases apart at entry and at exit,
   of how many; each tracker change kept or out; #15, #43 and #44 each with
   its post-R56 number; 3.8 ticked or not, and why.
C. The per-case table and the rest. Section 4 raises <n> items, and whether
   any is in the way of 3.8.
```

```
UNIT:       409 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     single-sender cases more than one bin apart: <entry> -> <exit> of <n>
DRIFT:      <0 if a criterion moved>
```

**Section 3 leads with the per-case table**, entry and exit side by side, with the two sweep
columns and the single-sender flag. It then gives each tracker change's row: the change, the
number of cases apart before and after, the named counts, the anchors, 17:37 *against an inferred
key*, and kept or out.

**Section 2 tells Tim in one paragraph** whether the decoder now listens where the keying is, in
plain words, **without claiming the text is correct.**

---

```
ARBITER-DECISION
STEP: 3
APPROACH: table the decoder mix pitch against the independent KeyingEnvelope sweep per case over the 37 captures, the anchors and 17:37, then change CwToneTracker only where the table shows more than one 25 Hz bin apart, judged on the table, named counts, anchors and the 17:37 key; measure #15 #43 #44 after
MOVE: continue
WHY: 3.8 is the only unmet criterion the loop can move. Every criterion of step 0 is met although the prompt named it, so authoring there would advance nothing. The loop test found nothing like this approach, and it is not a loop: units 406 and 407 changed the tracker to flip 3.6's reds, while this unit makes a pitch table against an instrument independent of the decoder the judge, and bars H1, the still-keying property and 3.6 attacks.
STATE: partial
DECIDED: author's, overrulable - the instrument is KeyingEnvelope, the tree's one sweep, judged over its HEAD range 300 to 900 with 400 to 1200 printed beside it and every disagreement named, the app's 400 to 1200 label reported as a stale mismatch; the decoder's pitch is the most-held mix value over the named-character span; single-sender is not a two-station fixture, keying under CwKeyingThresholds, and second-best under 0.5 of the winner more than two bins away, fixed before the table is read; agree is 25 Hz or less; up to three tracker changes, the decoder never reading the sweep; #15 #43 #44 measured, never targeted; the stale step 0 and step 2 states reported, not edited; the per-type timeouts
LICENCE: PHASE_PLAN.md 3.8, R56, R57, R58, section 3, section 6 on the tracker, the 17:37 key and floors; HM-DEC-091, HM-DEC-095, HM-DEC-127, HM-DEC-155, HM-DEC-165, HM-DEC-168; CLAUDE.md 0.0 and 0.2; FACT-004
ACCOMPLISHED: the decoder listens at the pitch where an instrument that shares nothing with it hears the keying, on every saved single-station recording, with no letter lost - or the report says on which recordings it still does not
ADVANCES: step 3 criterion 8
END-ARBITER-DECISION
```
