# Work instruction 408 - the floors stop counting junk, and the gate stops printing it

**Seed under `--seed`.** The first unit of step 3's new work. It re-measures the 37 capture
floors as named characters under R57, takes one closing pass at 3.6's four remaining reds
with the new rule in force, and then raises the emission gate so the decoder stops
printing characters it does not believe. **Five tasks, drop from the back.**

**Status.** `sh tools/status.sh`, real clock, after every commit and every task, and
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

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run as
its top comment says. **Never background and poll.** One type per invocation, each with its
own `timeout`. A run lost before any assertion is re-run once and counted neither way; a red
on an assertion is red.

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`. Write
*tasks 0 to 4, none dropped* with commas.

**`TheIntegratorBandwidthTable.Write` runs 362 s and `OneDecoderNotTwoTests` will not fit a
600 s call.** Neither is in this unit's set.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit408-<name>.sh` and run with `sh`.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. Units 405, 406 and 407 each asked rulings
about the floors and the reds; **R57 and R58 answer them and they leave the carried list.**
Anything else that blocks no criterion here is parked under R54.

---

## 4. Why this unit exists

```
PHASE GOAL: CW decodes again.
UNIT GOAL:  Re-measure the floors as named characters, close 3.6 in one pass
            with the new rule, and raise the emission gate so placeholders
            stop reaching the screen.
ADVANCES:   step 3 criterion 9
DRIFT:      2 consecutive units without advance
```

**What Tim said, 2026-09-23:** *"I hate all the false positive garbage."*

**What the record says.** Seven units, 401 to 407, tried thirty-four changes and kept none.
Two of them worked: unit 405's **B2** turned #45 green and **M2** turned #6 green, and both
were thrown out for lowering capture rows that were nothing but placeholders. Unit 405's
**G1** cost nothing on any floor or type and was thrown out only because no red went green
with it. **R57 changes the rule those three died to. R58 says the gate comes first.**

**What the air says**, `cw-2026-09-23-173723.txt`: 85 characters emitted, 26 unsure, 169 of
169 elements resolved; the real text stands at spans of 205 to 850 and the placeholders at 2
to 4. `CQ CQ CQ DEW B 6 RE D W B` - every letter right, the spaces wrong. **The decoder
already knows which characters it believes.** It prints the others anyway.

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- `src\Hamlet.RadioEngine\Cw\CwEmissionGate.cs` exists, and what its bar is expressed in.
- `tests\fixtures\cw\captured\unadjudicated\cw-2026-09-23-173723.wav` and its `.txt` are on
  disk, and whether `cw-2026-09-23-173723.key.md` is beside them. **If the WAV is absent,
  task 3 drops to the fixtures and the anchors alone and the report says so.**
- The floor table in `TheCapturesThatDecodeKeepDecodingTests` holds 37 rows, and what its
  character floor is counted from.
- `docs\phase-cw\reds-3.6.md` holds #15, #43, #44, #45 as open.
- Unit 405's `docs\phase-cw\unit405-reds.md` describes B2, M2 and G1 well enough to rebuild
  them. **405's diffs are not in the tree**; rebuild from the descriptions, as 405 rebuilt
  402's B2, and say so.

## 6. Rulings in force

`PHASE_PLAN.md` R47 to R58 and §6, read before task 1 and not re-argued.

**R57** a floor is the count of **named** characters; a row that falls solely because
placeholders were suppressed is not a floor lowered; a row whose named count falls is a
regression and the change goes back out.
**R58** the gate first, then the tracker; 3.6 closes on this unit's single pass.
**R56** the tracker is open - **but not in this unit.** 3.8 is the next unit's.
**CLAUDE.md §0.0** the 17:37 key is inferred, never "what was sent"; every number against it
is stated as *against an inferred key*. **§0.2** nothing that keys is touched.
**HM-DEC-091** a change that reads one recording and costs another is not a fix - now read
with R57: costs means named characters.
**HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this ruling in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md`
§1's table dated 2026-09-23 with headline **A CW floor counts named characters, not
placeholders; the emission gate comes before the tone tracker**, ref HM-DEC-168:**

```
---
id: HM-DEC-168
date: 2026-09-23
refs: PHASE_PLAN.md R56 R57 R58, docs/phase-cw/unit405-reds.md, work instruction 408, HM-DEC-091, HM-DEC-095, HM-DEC-127
---

**A CW floor counts named characters, not placeholders, and the emission gate is repaired
before the tone tracker.** Tim, 2026-09-23.

**Why.** The capture floors count every character the decoder emits, and a placeholder is a
character, so a change that stops the decoder printing what it does not believe reads as a
regression. Unit 405 turned two inherited reds green and threw both changes away on that
reading; units 406 and 407 then spent themselves on the same four reds under the same rule.
Tim, at the radio: *"I hate all the false positive garbage."*

**What is ruled.** A floor is the count of named characters. A row that falls solely because
placeholders were suppressed is not a floor lowered; a row whose named count falls is a
regression. All 37 rows are re-measured once with both numbers printed, and the three
adjudicated readings are the independent check that nothing real was lost. The emission gate
is repaired first; leave to change `CwToneTracker` is granted by R56 for the unit after,
without overruling HM-DEC-095 or HM-DEC-127.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 408 task 0's
record of it. Nothing was rejected in the recording.
```

## 7. Status cadence

As the header says. `NOTE` says what is moving inside the task.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 408 - STEP 3` entry in the existing shape from the
decision block at the foot of this file. `PHASE_STATUS.md` names unit 408. Patch-bump
`Directory.Build.props`. `DECISIONS.md` HM-DEC-168 and the `CLAUDE.md` row. Commit the
17:37 capture, its sidecar and its key file if git sees them untracked. **Entry round:** both
carry-forward lines and the three floor tests, every number recorded.

**Drop candidate:** none.

### Task 1 - the floors re-measured (3.9)

In one commit: change the floor table and the harness so a capture row carries **named
characters** and **placeholders** as separate numbers, the named count being the floor that
only rises and the placeholder count recorded but asserted on nothing. Re-measure all 37 rows
at HEAD. **The report prints a 37-row table: old total, new named count, placeholder count.**

Then run the three adjudicated anchors and, if the WAV is present, the 17:37 capture, and
state plainly whether any named character anywhere was lost. **If a row's named count is
lower than its old total minus its placeholder count, stop and report it** - that is a real
loss and it means the re-measurement is wrong, not the floor.

**Drop candidate:** none. Nothing else in the unit can be judged without this.

### Task 2 - 3.6's closing pass (3.6)

Rebuild **B2**, **M2** and **G1** from `unit405-reds.md`, one per commit, and judge each under
R57: kept if a red goes green and no row's **named** count falls and the three anchors are
unchanged. Then #15, #43, #44 and #45 each get their verdict: green, or parked in
`docs\phase-cw\PARKED.md` with its number and the units that attacked it. **Write the closing
line into `reds-3.6.md` and do not attack any 3.6 red beyond this pass** (R58).

**Drop candidate:** M2 and #6. B2 and G1 first; they are the two with the clearest evidence.

### Task 3 - the gate (3.7)

Raise `CwEmissionGate`'s bar so a character the decoder scores in the single digits is not
emitted at all. Watch a test fail first: a named fact over the 17:37 capture asserting that
no emitted character's span is below the bar, and that the scored region of the inferred key
reads no worse than at entry. Then measure, per case, over the 17:37 capture, the three
anchors and all 37 fixtures: named characters before and after, placeholders before and
after.

**The bar is the author's, overrulable, and it is stated as a number in the report.** Pick it
from the evidence rather than from taste: the 17:37 spans separate at roughly 4 against 200.
**No named character may be lost anywhere, and the three anchors must read character for
character as they did.** A bar that loses one anchor character is too high; lower it and say
so.

**Drop candidate:** the 37-fixture sweep, if the clock is short - the 17:37 capture and the
three anchors are the minimum, and the report says the sweep was dropped.

### Task 4 - the exit round

Both carry-forward lines, the three floor tests, and every type task 2 touched. `git diff`
over the transmit files named in `PHASE_PLAN.md` §3 against `7e209cb4` prints nothing.

---

## 9. Parked - do not touch, do not raise

- **`CwToneTracker` and 3.8.** R56 grants it; the next unit does it.
- **#6 and #42.** #6 rides along only if M2 is built; #42 is parked already.
- **The screen findings** - the RF gain sentence, the window reflow, hover text. A later
  phase's, in `OPEN_ISSUES.md`.
- **`tonePeak`, `elementHz`, and `keying` reporting "no keying at 575 Hz" beside 98
  key-downs.** Park as 408 items in `PARKED.md` if seen; chase none.
- **The correctness phase**, its keys and its yardstick. Tim's next phase, interviewed
  separately.

## 10. What not to do

- **Do not lower a named-character count anywhere.** R57 frees placeholders, nothing else.
- **Do not fit the gate to the fixtures.** The bar comes from the spans, and if it needs
  per-case tuning to hold the floors, it is the wrong bar and the report says so.
- **Do not touch `CwToneTracker`** in this unit, and do not attack a 3.6 red after task 2.
- **Do not call the 17:37 key a transcript** or write that any transcript is what was sent.
- **Do not adjudicate the unscored first third** of the 17:37 recording.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, canonical headings.

```
READ IN THIS ORDER.

A. The gate's bar as a number, and what the 17:37 capture reads with it on,
   beside what it read with it off.
B. Step 3's criteria: 3.9 the re-measurement, 3.6 closed, 3.7 the gate,
   3.8 the tracker not started.
C. The 37-row table and the rest. Section 4 raises <n> items.
```

```
UNIT:       408 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     placeholders on the 17:37 capture: 26 -> <n>, named 59 -> <n>
DRIFT:      <0 if a criterion moved>
```

**Section 2 tells Tim in one paragraph** what the CW terminal will look like now compared
with what he saw at 17:37, in plain words, without claiming the text is correct.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: re-measure the 37 capture floors as named characters and placeholders under R57, take 3.6's single closing pass with B2, M2 and G1 rebuilt under the new rule, then raise the emission gate so characters the decoder scores in the single digits are not printed
MOVE: continue
WHY: PHASE_PLAN.md step 3 criterion 9 asks for all 37 capture rows re-measured once under R57 with the old count, the named count and the placeholder count printed per row
STATE: partial
DECIDED: the gate's bar, the separate-column form of the floor table and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R49, R55, R57, R58, section 6; HM-DEC-168; HM-DEC-091; HM-DEC-155; HM-DEC-139; CLAUDE.md 0.0
ACCOMPLISHED: the floors measure what Tim reads instead of what the decoder emits, 3.6 is closed either way, and the placeholders stop reaching the CW terminal
ADVANCES: step 3 criterion 9
END-ARBITER-DECISION
```
