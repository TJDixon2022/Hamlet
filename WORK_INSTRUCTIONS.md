# Work instruction 424 - the preamp is what the manual says, and nothing argues with it

**Seed under `--seed`.** Tim has raised this three times: he sets the preamp, Hamlet sets it
back, and something tells him it should be off. The condition in the tree disagrees with the
radio's own manual, and more than one voice speaks about the field. **Both are fixed here.**
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

**Write `output.md` at the root before the session ends, whatever else happened.** Unit 423's
follow-on session wrote none and the launcher halted at stop 11 reading the previous unit's
file.

**Nothing in section 4 halts this phase** (R65). Park it and go on.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit424-<name>.sh` and run with `sh`. Unit 423's scripts can be copied.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **P19 is answered by R73 and leaves the carried
list; it is step 3's, not this unit's.** Nothing else is this unit's to answer.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  The preamp is set from what the radio's manual states, and no
            component tells the operator to change what Hamlet set.
ADVANCES:   step 7 criterion 8
DRIFT:      0
```

**Tim, 2026-09-24:** *"I want to use the radio like I use my car radio. I tune to the place I
want and it just happens... You know the mode we're in. You know the range. Why do I have to
control this? I don't know the radio."* And: *"If it's set where it should be, then Hamlet
shouldn't be whining about it."*

**Fault one: the condition disagrees with the manual.** The tree's CW preamp condition reads
`"wanted": 1` with `wantedText` *preamp 1 above 40 m, off at 40 m and below*. The radio's
manual, `IC-7300_ENG_FM_12b`, *Receiving and Transmitting*, page 4-3, says:

- **P.AMP1** - *"Wide dynamic range preamplifier. It is most effective for the HF low bands."*
- **P.AMP2** - *"High-gain preamplifier. It is most effective for the 50 MHz bands."*
- *"NOTE: When you use the preamp while receiving strong signals, the signal may be distorted.
  In such a case, turn OFF the preamp."*
- *"Each band memorizes the Preamplifier setting."*

Icom's published receive sensitivity is quoted **with Preamp 1 on for 1.8 to 29.999 MHz** and
**Preamp 2 on for 50 MHz**. So the condition turns the preamp off across the low bands where
Icom specifies it on, says nothing about 6 m, and keys the off case to the band rather than to
overload. **It is wrong on all three counts.**

**Fault two: more than one voice speaks about the field.** Unit 419 scoped six voices and
Tim still hears the complaint on the current build, so at least one was missed - look at
`ReceiveObstructions` (which HM-DEC-148 says states obstructions and does not write them),
`RigObservations` beyond the attenuator pair 419 scoped, and anything else that mentions the
preamp. **A voice that tells him to change a field Hamlet set is the app contradicting itself,
whichever of the two is right** (§0.0).

---

## 5. Verify this instruction against the tree

Check every quotation above against `IC-7300_ENG_FM_12b` in project knowledge if it is
reachable, and report any mismatch; **if the manual is not in the tree, say so and take the
quotations in section 4 as given** - they are the web thread's reading of it and the citation
goes in the condition's text either way.

Then also:

- The CW preamp condition's current `wanted`, `wantedText` and `confirmed`, and how unit 419's
  band rule is carried - the band rule now lives in the written value, so find where.
- Every component that names the preamp: which ones write it, which ones speak about it, and
  which ones unit 419 scoped. **Name them all in the report.**
- Whether `RigState` or the capture sheet exposes an overload indicator the setup can read -
  the sheet prints `Overflow  not overloading`, so something holds it.
- Whether the conditions file can state a value that depends on frequency now, after 419 and
  420, or whether the setup derives it.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R74, §3 and §6.

**R74** Hamlet sets the radio; the operator is not rig control. **No voice asks him to change
a setting Hamlet established.** A criterion is met by making the setting right and the
sentences agree with it, **never by silencing a voice that is telling the truth** - if a voice
is right, the setting changes.
**R67 / HM-DEC-174** a condition is written once when the radio is not already at it, one
component decides each field, and the operator's hand is not overwritten by a later tune-in.
**HM-DEC-056** his hand wins. **HM-DEC-148** as `ReceiveObstructions` records it.
**§0.0** the app may not state two contradictory things about one setting.
**§0.2** nothing that keys, transmits or sets power is touched. **§12.4** a setting changed on
a guess is the prime directive broken with a byte - **so every value here is cited to the
manual**.
**HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-24, headline **The preamp is set from the manual, and nothing contradicts
what Hamlet set**, ref HM-DEC-176:**

```
---
id: HM-DEC-176
date: 2026-09-24
refs: PHASE_PLAN.md R74 and criterion 7.8, IC-7300_ENG_FM_12b page 4-3, data/bands/mode-receiver-conditions.json, HM-DEC-174, HM-DEC-056, HM-DEC-148, CLAUDE.md 0.0 and 12.4, work instruction 424
---

**The preamp is set from what the radio's manual states, and no component asks the operator
to change what Hamlet set.** Tim, 2026-09-24: *"You know the mode we're in. You know the
range. Why do I have to control this? I don't know the radio."*

**What the manual states.** Page 4-3: P.AMP1 is the wide dynamic range preamplifier, most
effective for the HF low bands; P.AMP2 is the high-gain preamplifier, most effective for the
50 MHz bands; when the preamp is used while receiving strong signals the signal may be
distorted, and in that case the preamp is turned off; each band memorizes its own setting.
Icom's published receive sensitivity is quoted with Preamp 1 on from 1.8 to 29.999 MHz and
with Preamp 2 on at 50 MHz.

**What was wrong.** The CW condition read *preamp 1 above 40 m, off at 40 m and below*, which
turns the preamp off across the low bands where Icom specifies it on, says nothing about 6 m,
and keys the off case to the band rather than to overload. And more than one component spoke
about the field, so the app set the preamp and then told the operator it should be otherwise.

**What is ruled.** Preamp 1 for 1.8 to 29.999 MHz, preamp 2 at 50 MHz, and the preamp off
when the receiver reports overloading. The condition cites the manual page it comes from. No
component asks the operator to change a field a condition states, after Hamlet has set it; if
a voice is right that a setting is wrong, the setting changes rather than the operator.

**Whose words are whose.** The ruling is Tim's and the values are Icom's; the wording is work
instruction 424's record of them. Rejected: leaving the band rule as it stood; asking the
owner which he wants.
```

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 424 - STEP 7` entry from the decision block at the foot
of this file. `PHASE_STATUS.md` names unit 424 and `CURRENT_STEP: 7`. Patch-bump
`Directory.Build.props`. `DECISIONS.md` HM-DEC-176 and the `CLAUDE.md` row. **Entry round:**
both carry-forward lines, the three floor tests, and every Rig type, recorded.

**Drop candidate:** none.

### Task 1 - what is said about the preamp today

A fact that asserts nothing, driving the real setup against `ScriptedRadio`, tuning into CW at
**7.030, 14.050, 21.050, 28.050 and 50.100 MHz** and printing per frequency:

- what the condition asks for and what is written;
- **every sentence any component would say to the operator about the preamp there** - the
  setup's own voice, the advice, the obstructions, the observations, and anything else task
  5's survey found;
- the same again with the receiver reporting overload, if the setup can see that.

**Print the sentences verbatim.** This is the evidence for both halves of the fix, and it goes
in section 3.

**Drop candidate:** none.

### Task 2 - the condition, from the manual (7.8)

Watch a test fail first at **7.030**, where today's rule turns the preamp off and the manual
says on. Then:

1. The CW conditions carry **preamp 1 for 1.8 to 29.999 MHz and preamp 2 at 50 MHz**, applied
   to every CW-family block as unit 420 left them.
2. The condition's own text **cites the manual page** - `IC-7300` page 4-3 - so the next
   reader does not have to guess where the value came from.
3. **The off case is keyed to the receiver overloading, not to a band.** If the setup can read
   an overload indicator, use it and say which; **if it cannot, do not invent one** - state
   plainly in the report that the off case is unimplemented and park it. §12.4: no guess.

**How the frequency-dependent value is carried is the author's**, and the reason goes in
DECIDED. It must be the same mechanism unit 419 built for the band rule if that mechanism
fits.

**Drop candidate:** item 3, with the reason stated.

### Task 3 - one voice (7.8, second half)

Every component task 1 found speaking about the preamp is scoped so that **none asks the
operator to change it after the setup has set it**. Watch a test fail first at **14.050**,
where Tim hears the complaint on the current build.

**Do not silence a voice that is right.** If a component objects to the value the condition
now sets, that objection is evidence the condition is still wrong: report it, and say which
you believe and why. A voice that speaks about a field **no condition states** keeps its
voice - that is not a contradiction.

**Drop candidate:** none.

### Task 4 - the exit round (7.6 holds)

`Hamlet.sln` builds with warnings as errors. Both carry-forward lines, the three floor tests
with captures at 51, every Rig type, the sheet tests from 411, 417 and 418, and every type
touched. **`src\Hamlet.RadioEngine\Cw` prints nothing against entry** - no decoder change -
and the transmit files print nothing against `7e209cb4`. **Print task 1's table again, after
the changes, beside the before version.**

---

## 9. Parked - do not touch, do not raise

- **3.6 and the stray letters**, now unblocked by R73. Step 3's, not this unit's.
- **Step 4 the pitch judge; 7.1 to 7.4 the speed ceiling and acquisition; 6.5 the dead button.**
- **Any other receive condition.** Only the preamp is ruled here; if another field looks wrong
  against the manual, **write it in `PARKED.md` with the page and move on**.
- **The attenuator's 0x14 against BCD 0x20**, parked by unit 419.

## 10. What not to do

- **Do not choose a preamp value from anything but the manual**, and cite the page for each.
- **Do not silence a voice that is telling the truth.** Fix the setting instead.
- **Do not invent an overload reading.** If the setup cannot see one, say so.
- **Do not touch what keys, transmits or sets power.** Not one byte.
- **Do not change the decoder.**
- **Do not ask the operator to decide a radio setting**, in any sentence this unit writes.
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends, whatever happened.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task, each change with its red quoted in the message. Push at the end and say
whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. What the preamp is set to on each band now, and every sentence the app
   says about it, before and after.
B. Step 7's criterion 7.8, clause by clause, and 7.6 at exit.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       424 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     sentences contradicting what Hamlet set: <n> -> <n>
DRIFT:      0
```

**Section 2 tells the owner in one paragraph** what the preamp will be on 40 m, on 20 m and on
6 m when he tunes there, what happens when the receiver overloads, and that nothing will ask
him to change it.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: set the CW preamp condition from the radio's manual - preamp 1 from 1.8 to 29.999 MHz, preamp 2 at 50 MHz, off on overload rather than by band, with the page cited - and scope every component so none asks the operator to change a field Hamlet has set
MOVE: continue
WHY: PHASE_PLAN.md step 7 criterion 7.8 asks that the preamp be set from what the radio's manual states with the page cited, that the off case be keyed to the receiver overloading rather than to a band, and that no component ask the operator to change a field a condition states after Hamlet has set it
STATE: partial
DECIDED: how the frequency-dependent value is carried, which overload reading is used if any, how each voice is scoped, and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R67, R74, section 6; HM-DEC-176; HM-DEC-174; HM-DEC-056; HM-DEC-148; CLAUDE.md 0.0, 0.2 and 12.4; HM-DEC-155
ACCOMPLISHED: Tim tunes to a band and the preamp is what the radio's own manual says it should be, and nothing on screen argues with it
ADVANCES: step 7 criterion 8
END-ARBITER-DECISION
```
