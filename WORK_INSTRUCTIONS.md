# Work instruction 429 - what the decoder was doing while it read E ET E E

**Seed under `--seed`.** The first two minutes of the 7.052 session on 2026-09-24 read
`E ET E E   E  E E  E` before the decoder locked and read a whole QSO. This unit measures why,
and builds nothing. Four tasks, drop from the back.

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
own `timeout`. The captures type is 51 rows and ran 119 s in unit 428; give it 600 s. The new
fact decodes a handful of 30 s recordings; give it 600 s, and if it runs past 300 s it goes on
no carry-forward line (section 6 of the plan).

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**Write `output.md` at the root before the session ends, whatever else happened.**

**Nothing in section 4 halts this phase** (R65). Park it and go on.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit429-<name>.sh` and run with `sh`. Unit 428's scripts can be copied.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **None is this unit's.** P27 stays the owner's;
P29 to P34 stay parked. Unit 428's section 4 items 1 and 2 (banking the 2026-09-25 traffic
net, and whether 3.6 is reachable on any span figure) are parked by task 0 as P35 and P36 with
428's own proposed rulings. Item 3 was for the record and is not parked. This unit answers
nothing but its own criterion.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  Say, from the decoder's own figures, what differs between the
            opening that read E ET E E and the stretch after it locked.
ADVANCES:   step 7 criterion 3
DRIFT:      0
```

**The count today.** Steps 0, 1 and 2 done. Step 3 partial, 3.4 and 3.6 open. Step 4 not
started. Step 5 is the owner's verdict. Step 6 partial, 6.5 open. Step 7 partial, 7.1 to 7.4,
7.6 and 7.8 open.

**Why not 3.6 again.** Five routes at the stray single elements have been measured: a fixed span
bar (421); per-hop span; features other than span, meaning Gate score, gaps, standing alone,
pitch and energy (425); span against neighbors; and span per mark against neighbors (428). None
separated the 8 added single elements from the 62 right ones, and unit 428 ended with the
criterion unmoved. A sixth span-shaped trace would be the loop that §4 of `ARBITER.md` names.
Two step 3 units in a row have kept nothing. If the next one also keeps nothing, it is the
third, and 3.4 then closes the step partial with the traces in `PARKED.md`. That is for a later
unit, not this one.

**Why 7.3.** The opening's litter is the same thing 3.6 chases: single dits and dahs read as
`E` and `T` with confidence. But here there is a clean comparison the keyed corpus never gave:
**the same sender, the same pitch, the same receiver, minutes apart**, reading junk and then
reading `KA2GJV` and `AA3SB`. Whatever the decoder held differently in those two stretches is
the cause, and 7.3 asks only that it be named. It depends on nothing, touches no floor, and
the recordings are in the tree.

**What the owner read**, from the sidecar of `cw-2026-09-24-003901`, the first 30 s after the
transcript was cleared at 00:38:54 UTC:

```
decoderWpm 24
text       E ET E E   E  E E  E  E E  E    E A TE E T N QNIK     EE
```

`-003919` says `decoderWpm withdrawn (the clock is being re-acquired; the decoder's own best
hypothesis was 34 WPM)`. By `-004108` it holds 22 WPM and reads `DE KA2 G J V HR NR 2 0`.

---

## 5. Verify this instruction against the tree

Check each of these. Report any mismatch and repair nothing:

- `cw-2026-09-24-003901`, `-003919`, `-004027`, `-004108` and the later captures of the run are
  in `tests\fixtures\cw\captured\unadjudicated`, each 30 s. **The recordings overlap**: `-003901`
  was kept at 00:39:01 and `-003919` at 00:39:19. Say how much, from the sidecars.
- **Whether the bench reproduces the opening at all.** The live decoder ran continuously from
  00:38:54, but the bench decodes each recording from a cold start. Decode `-003901` and
  `-003919` as the captures type does and quote the bench's text beside the sidecar's. If the
  bench reads the opening cleanly, or reads the locked stretch as junk, that is the first
  finding and it is stated before anything else.
- Where the decoder holds each of 7.3's four figures: the speed (the WPM the search won, and
  the unit it implies), the pitch it mixes at (`CwToneTracker`), the unit it estimates
  (`CwUnitEstimator`), and the score a character is admitted on (`CwProbabilisticDecoder.Judged`,
  `CharacterMargin` 1.0 per hop, and `StrayElementSpan` 13.0 raw for single elements).
- The keyed totals at HEAD: all keyed 165 over 565 against inferred keys, and added letters 17.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R74, §3 and §6.

**R68** the acquisition failure is step 7's: the first two minutes of the 7.052 session read
`E ET E E` before the decoder locks.
**R72** no word, dictionary or callsign prior, in any form.
**R63** the fourteen captures of 2026-09-24 are the benchmark; their floors are never lowered.
**R71** the floors count at or above raw span 13.0. **R73** only a key-aligned added character
inside a scored stretch may leave a floor.
**3.2's four tests** are the keep rule for any later change against what this unit names.
**§0.0** no decode is called what was sent. **§0.2** nothing that keys or transmits is touched.
**§12.5** a fixture built from the same misunderstanding as the code proves nothing.
**HM-DEC-091**, **HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

No new decision is recorded. The unit reports its own choices; it does not rule on them.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 429 - STEP 7` entry from the decision block at the foot of
this file. `PHASE_STATUS.md` names unit 429 and `CURRENT_STEP: 7`. Patch-bump
`Directory.Build.props`. Park unit 428's items 1 and 2 in `docs\phase-correctness\PARKED.md` as
P35 and P36, verbatim from its section 4 with its proposed rulings. **Entry round:** both
carry-forward lines, the three floor tests with captures at 51, the keyed totals and the added
letters, recorded as the numbers to hold.

**Drop candidate:** none.

### Task 1 - the opening, on the bench and live (7.3)

Before any figure is traced, establish what the bench reads. For `-003901`, `-003919` and one
recording from after the lock (`-004108` or later, your choice, and say why), print the bench's
text beside the sidecar's `text` line for the same 30 s.

**If the bench decodes each recording from a cold start, the locked recording also starts
cold.** So also feed the run as one continuous stream: the recordings in capture order, with
each overlap removed using the sidecars' own timestamps. Print the text of that stream per
30 s. The splicing is yours; say how the overlaps were found and cut. If the continuous stream
does not reproduce the opening's litter followed by a lock, say so plainly. Task 2 then traces
whichever stretch does show it, and the report names the gap between live and bench as a
finding.

**Drop candidate:** the continuous stream, if the cold decodes already show the litter on the
opening and clean text on the locked recording. Say which was dropped.

### Task 2 - the trace (7.3)

Write one new fact in `tests\Hamlet.RadioEngine.Tests\Cw` that **asserts nothing**, and name it
for what it shows, for example `WhatTheOpeningHeardTests`. Over the opening stretch that reads
`E ET E E`, and over a stretch of the same length after the lock, print **for every character
emitted**:

- time in the recording, and the character;
- the speed the decoder held at that moment, in WPM and as the unit in milliseconds;
- the pitch it mixed at, in Hz, and the tone the survey measured on the same recording;
- the unit its estimator held, if that is a different figure from the speed's;
- the score that admitted the character: its margin against `CharacterMargin`, its raw span,
  and its element count.

Then print a side-by-side summary: each figure's median and spread in the opening against the
locked stretch. **Name the line or property in `src\Hamlet.RadioEngine\Cw` that differs**, with
the file and line number, and say how far apart the two stretches are on it. If two differ,
name both and say which moves first in time. If none differs, say that plainly. The opening's
litter may then be what the decoder makes of a stretch with no sender in it, and the report
says how the trace tells those two cases apart.

**Build nothing.** No file under `src` changes in this unit.

**Drop candidate:** none.

### Task 3 - the second session (7.3)

Run task 2's fact unchanged on a second opening stretch: `-003919`'s own opening if task 2
used `-003901`, otherwise the next recording of the run. Say whether the same property differs
the same way. One stretch is a reading; two that agree are evidence.

**Drop candidate:** whole task, with what was measured stated.

### Task 4 - the exit round

Run both carry-forward lines, the three floor tests with captures at 51, the keyed totals and
the added letters (all as at entry), and the new fact's type. `src` and `data` show no change
against entry, and the transmit files show no change against `7e209cb4`.

---

## 9. Parked - do not touch, do not raise

- **3.6 and the stray single elements.** Five routes measured; not this unit's.
- **7.1 and 7.2, the speed ceiling.** If the trace shows the top of the search mattering in the
  opening, report it as a finding and build nothing.
- **7.4, the change.** That is the next unit's, against what this one names.
- **7.8, the attenuator sentences.** P27 and HM-DEC-179 stay as they stand.
- **Step 4 the pitch judge; 6.5 the dead button.**
- **P27, P29 to P36.**
- **Any key, scored region, floor or bar.** Fixed.

## 10. What not to do

- **Do not change any file under `src` or `data`.** 7.3 is a trace; 7.4 builds.
- **Do not add a word, dictionary or callsign prior** (R72).
- **Do not claim 7.4 or 7.6.** Tick 7.3 only if the report names a line or property from the
  printed figures, or states plainly that none differs, with the figures shown.
- **Do not touch what keys or transmits.**
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit after each task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial with 3.4
   and 3.6 open; 4 not started; 5 the owner's; 6 partial with 6.5 open;
   7 partial with 7.1 to 7.4, 7.6 and 7.8 open.
B. Step 7, criterion 7.3: whether the bench reproduces the opening, and the
   line or property that differs between the E ET E E stretch and the
   locked one - met or not, and why.
C. The rest, weighed against A and B. Section 4 raises <n> items; say
   whether any stands in the way of 7.3 or 7.4.
```

```
UNIT:       429 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     the named difference - <property, opening figure against locked figure>; keyed 165 -> <n> over 565, unchanged
DRIFT:      <0 if a criterion moved>
```

**Section 3 leads with the side-by-side summary**: each of the four figures, opening against
locked, and the one line it names. **Then the bench's text beside the sidecar's** for the
opening, so the owner can see the trace is of the thing he read.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: trace the decoder's held speed, mixing pitch, unit estimate and admission scores through the opening stretch of 003901 and 003919 beside the locked stretch, name the line that differs, build nothing
MOVE: work around
WHY: Criterion 3.6 has five span and feature routes measured with none separating added from right single elements, and unit 428 did not advance, so a sixth would be a loop and a second non-advance in a row; 7.3 is an untried, dependency-free trace on recordings in the tree, and its litter is the same stray E and T 3.6 chases, seen against a locked stretch from the same sender.
STATE: partial
DECIDED: author's, overrulable - routing from 3.6 to 7.3 rather than 6.5 (HM-OPEN-087) or 7.8 (P27, the owner's); 3.4 is left for the next step 3 unit, since two consecutive step 3 units have kept nothing and the criterion asks three; unit 428's items 1 and 2 parked as P35 and P36; which locked recording is compared, how the overlaps are spliced, and the per-type timeouts are the unit's, reported
LICENCE: PHASE_PLAN.md R68, R64, R65, R72, section 6 and criterion 7.3; ARBITER.md section 4; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the project knows, from the decoder's own figures, why the first minutes on 7.052 read E ET E E before the QSO came through clean, and which line a repair has to change
ADVANCES: step 7 criterion 3
END-ARBITER-DECISION
```
