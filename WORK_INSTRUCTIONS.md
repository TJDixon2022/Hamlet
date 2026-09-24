# Work instruction 419 - entering a mode sets the radio, once, correctly

**Seed under `--seed`.** Tim, at the radio: Hamlet puts the preamp to stage 1 or 2 in CW,
complains it is not off, and turns it back on after he sets it off by hand. Entering a mode
is supposed to set the receiver correctly and leave it set. **This unit makes that true.**
Four tasks, drop from the back.

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
own `timeout`. The captures type is 51 rows; give it 600 s.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**Nothing in section 4 halts this phase** (R65). A question is parked in
`docs\phase-correctness\PARKED.md` and the loop goes on. **Do not carry an ask forward as
blocking** unless 7.5 itself cannot be met without it.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit419-<name>.sh` and run with `sh`. Unit 418's scripts can be copied.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **Unit 411's RF gain ask and unit 418's two
parked sheet lines are not this unit's**, except where the RF gain field falls out of 7.5's
own table, which it does. Nothing else.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Entering CW mode and entering data mode each set the receiver
            correctly, once, and leave it set.
ADVANCES:   step 7 criterion 5
DRIFT:      0
```

**Tim, 2026-09-24:** *"We're supposed to automatically set the right radio settings when we
enter CW mode and when we enter data mode, and we're not doing it. That's a violation of an
absolute order."* He also reports the app complaining the preamp is not off while having just
turned it on, and turning it back on after he set it off by hand.

**What the tree shows, checked by the web thread on 2026-09-24. Verify every line of it.**

1. **The band rule is not carried by the value written.**
   `data\bands\mode-receiver-conditions.json`, mode CW, control `preamp`:
   `"field": "preamp"`, `"wanted": 1`, `"wantedText": "preamp 1 above 40 m, off at 40 m and
   below"`, `"confirmed": true`. **`wanted` is a single number; the text it sits beside
   states a rule that depends on the band.** `ReceiverSetup.cs` around line 291 knows the
   rule in a comment - *"the preamp follows the frequency. Off at 40 m and below"* - and
   around line 167 mentions the front end overloading and the preamp at 40 m. So the rule
   lives in prose in two places and in no value.
2. **Three components decide the preamp.**
   - `ReceiverSetup` writes it on a tune-in.
   - `ReceiveAdvice.Preamp` (around line 318) reads the state and, when the preamp is off,
     returns a suggestion whose words are *"Switch the preamp on..."*, and when it is on says
     *"The preamp is already on."* **It never suggests off.**
   - `RigObservations.AttenuatorAndPreampTogether` (around line 173) objects when the
     attenuator is on and the preamp is on as well.
   One sets it, one asks for it on, one objects to it being on. **That is the complaint Tim
   hears.**
3. **Nothing is ever recorded as already correct.** Unit 411 found
   `Ic7300Rig.SetSettingAsync` comparing the CW condition's `rfGain` 255 against a read-back
   decoded as percent 100, so the write is always filed `ReadBackDisagreed` and the memory
   never records the value. HM-DEC-056's *your hand wins* cannot apply to a field that is
   never recorded, which is why a later tune-in of the same mode stamps over his hand.
4. **`ReceiveObstructions.cs` line 19 records HM-DEC-148**: that component states obstructions
   and does not write them. **Read that decision before changing any writer**, and say in the
   report what it governs.

**What this unit may and may not change (plan §6, R67).** It may make the app write **less** -
a value the radio already holds is not rewritten - and it may make the value written carry a
band rule the condition's own text already states. **It may not change what any condition asks
for.** Nothing that keys, transmits or sets power is touched.

---

## 5. Verify this instruction against the tree

Check every numbered claim in section 4 and report any mismatch; repair nothing. Then also:

- The full condition list for every mode in `mode-receiver-conditions.json`, and **which modes
  the file speaks for**. Its own `_about` says nothing is stated for a mode it cannot speak
  for, and that PSK31, RTTY and JS8 have no conditions. **So "data mode" in 7.5 means the
  modes this file actually states conditions for** - FT8, FT4 and whatever else it lists.
  Name them in the report.
- Which conditions are `confirmed: false`. The file says an unconfirmed condition is stated to
  the operator and **written to nobody's radio**. FT8's `agc` is one. That rule stands.
- Where `ReceiverSetup` is called from, and **how often**: what triggers a tune-in, and
  whether anything re-runs it while the operator sits in one mode on one frequency.
- Whether `RigState` holds a value's age, so "the radio already holds it" can be judged
  without a fresh read.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R69, §3 and §6.

**R67** entering CW mode and entering data mode each set the receiver correctly and it stays
set; 7.5 is the criterion, and it ranks above the decode work in step 7.
**HM-DEC-056** the operator's hand wins: a value he sets himself is not overwritten by a
later tune-in of the same mode.
**HM-DEC-148** as `ReceiveObstructions.cs` records it - read it and honor it.
**The file's own rule**: a condition marked `confirmed: false` is stated and not written.
**CLAUDE.md §0.0** never state as known what is not known, and never state as unknown what is
known. **§0.2** nothing that keys, transmits or sets power is touched - not one byte.
**§12.4** a setting changed on a guess is the prime directive broken with a byte.
**HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006** - there is no radio on this machine,
so every result here is an indication and the table is built against `ScriptedRadio` or the
equivalent, never against a real rig.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-24, headline **Entering a mode sets the receiver correctly and it stays
set**, ref HM-DEC-174:**

```
---
id: HM-DEC-174
date: 2026-09-24
refs: PHASE_PLAN.md R67 and criterion 7.5, data/bands/mode-receiver-conditions.json, src/Hamlet.RadioEngine/Rig/ReceiverSetup.cs, ReceiveAdvice.cs, RigObservations.cs, HM-DEC-056, HM-DEC-148, work instruction 419
---

**Entering CW mode and entering data mode each set the receiver correctly, once, and leave it
set.** Tim, 2026-09-24: *"We're supposed to automatically set the right radio settings when we
enter CW mode and when we enter data mode, and we're not doing it."*

**What was wrong.** The CW preamp condition states `wanted: 1` beside text reading *preamp 1
above 40 m, off at 40 m and below*, so a band rule is written in prose and not in the value.
Three components decide the preamp independently: the setup writes it, the advice asks the
operator to switch it on whenever it reads off, and the observations object when it is on.
And because a read-back decoded on a different scale is filed as disagreement, no value is
ever recorded as already correct, so the operator's own change is stamped over by the next
tune-in of the same mode.

**What is ruled.** Every condition a mode states is written once when the radio is not already
at it and not written when it is. A band rule stated in a condition's text is carried by what
is written. Exactly one component decides each field, and no other component asks the operator
to change a field the setup has just set. A value the operator sets himself is not overwritten
by a later tune-in of the same mode. A condition marked unconfirmed is still stated and still
not written.

**What is not changed.** What any condition asks for. Anything that keys, transmits or sets
power.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 419's record
of it. Rejected: widening the earlier RF gain criterion instead of stating the requirement.
```

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 419 - STEP 7` entry from the decision block at the foot
of this file. `PHASE_STATUS.md` names unit 419 and `CURRENT_STEP: 7`. Patch-bump
`Directory.Build.props`. `DECISIONS.md` HM-DEC-174 and the `CLAUDE.md` row. **Entry round:**
both carry-forward lines, the three floor tests, and every Rig type, recorded.

**Drop candidate:** none.

### Task 1 - the table, before anything changes (7.5's last clause)

A fact that asserts nothing, driving the real `ReceiverSetup` against `ScriptedRadio`, that
tunes into **CW at 14.050 MHz, CW at 7.030 MHz, and each data mode the file speaks for**, and
prints per field: what the condition asks for, what the radio answered, whether a write was
sent, what the result was filed as, and **which component or components mention that field**.

**The two CW frequencies matter**: 14.050 is above 40 m and 7.030 is at 40 m, so the preamp
rule's two halves both appear in the table.

Then print, for each field, whether any other component would ask the operator to change it
after the setup has set it - the advice, the obstructions, the observations.

**This table is the evidence for the whole unit and goes in section 3.**

**Drop candidate:** none.

### Task 2 - one owner per field, and the band rule in the value (7.5)

Watch a test fail first, then change. In order of what Tim reported:

1. **The preamp's band rule is carried by what is written.** How - a second value in the
   condition, a rule the condition names, or the setup deriving it from the frequency it was
   given - is the author's, and the reason goes in DECIDED. **The condition's text is the
   specification; the written value must match it at both frequencies in task 1's table.**
2. **One owner per field.** The setup owns a field a mode states a condition for. No other
   component asks the operator to change such a field after a tune-in of that mode -
   `ReceiveAdvice.Preamp`'s *"Switch the preamp on"* must not fire for a mode whose conditions
   state the preamp, and `RigObservations` must not object to a state the setup just
   established. **Do not delete these voices**; scope them, and say how.
3. **A value already correct is not written**, and the outcome says so rather than
   `NotConfirmed`. This is unit 411's 255-against-100 comparison and any other field with the
   same shape; the comparison is made on one scale. **No new byte is sent for a field already
   right.**
4. **The operator's hand wins** (HM-DEC-056): a value he changed himself is not overwritten by
   a later tune-in of the same mode. State the rule you implemented and how long it holds.

**Every one of the four is watched failing first and quoted red in the report.** If the clock
forces a cut, do them in this order and say where you stopped.

**Drop candidate:** item 4, last.

### Task 3 - the exit round (7.6)

`Hamlet.sln` builds with warnings as errors. Both carry-forward lines, the three floor tests
with captures at 51, every Rig type, `TheBannerSaysWhatTheRadioReadBackTests`, the sheet tests
from 411, 417 and 418, and every type touched. **`src\Hamlet.RadioEngine\Cw` prints nothing
against entry** - no decoder change in this unit - and the transmit files print nothing against
`7e209cb4`. **Print the table from task 1 again, after the changes, beside the before version.**

---

## 9. Parked - do not touch, do not raise

- **7.1 to 7.4**, the speed ceiling and acquisition. The next units'.
- **3.6 the stray letters**, step 4 the pitch judge, 6.3 the reflow, 6.4 hover text, 6.5 the
  dead button.
- **What any condition asks for.** Not this unit's, not any unit's without a ruling.
- **Any mode the conditions file does not speak for.** PSK31, RTTY, JS8 state nothing and so
  write nothing; that is correct behavior.
- **Unit 418's two parked sheet lines**, `captured` and `broadcast`.

## 10. What not to do

- **Do not change what a condition asks for.** Only whether, when and how it is written.
- **Do not write a condition marked `confirmed: false`.**
- **Do not delete a voice to stop it complaining.** Scope it to the fields it should speak for.
- **Do not touch anything that keys, transmits or sets power.** Not one byte.
- **Do not change the decoder.** `src\Hamlet.RadioEngine\Cw` prints nothing at exit.
- **Do not send a new read or a new write to prove a value is correct** if `RigState` already
  holds it with an age; if it does not, say so before adding a read.
- **Do not halt for a question.** Park it.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task, and each of task 2's four items in its own commit with its red quoted in the
message. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. The field table for CW at 14.050, CW at 7.030, and each data mode:
   before and after.
B. Step 7's criterion 7.5, clause by clause - written once, band rule
   carried, one owner per field, hand wins - and 7.6 the exit round.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       419 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     fields written on a CW tune-in when the radio was already right: <n> -> <n>
DRIFT:      0
```

**Section 2 tells the owner in one paragraph** what happens now when he tunes into CW at
14.050 and at 7.030, what the preamp does at each, what happens when he sets it off by hand,
and that nothing about the decoder or what keys changed.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: table every receive condition for CW at two frequencies and for each data mode the conditions file speaks for, then carry the preamp's band rule in the written value, give each field one owner, stop rewriting a value the radio already holds, and let the operator's own change stand
MOVE: continue
WHY: PHASE_PLAN.md step 7 criterion 7.5 asks that entering CW mode and entering data mode each set the receiver correctly and leave it set, with every condition written once when the radio is not already at it, the band rule carried by what is written, exactly one component deciding each field, and the operator's own change not overwritten
STATE: not started
DECIDED: how the band rule is carried, how each competing voice is scoped, which scale the comparison is made on, how long the operator's hand holds, and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R67, R65, section 6; HM-DEC-174; HM-DEC-056; HM-DEC-148; CLAUDE.md 0.0, 0.2 and 12.4; HM-DEC-155; FACT-006
ACCOMPLISHED: tuning into CW or a data mode sets the radio the way the mode needs it, once, and it stays that way - including when Tim has set something himself
ADVANCES: step 7 criterion 5
END-ARBITER-DECISION
```
