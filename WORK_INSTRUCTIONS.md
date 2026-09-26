# Work instruction 460 - the three named floors settled on the owner's answer, and step 2's commits kept green

**Loop unit.** Step 2, criterion 2.5 (HM-REQ-010's step). 2.1, 2.2 and 2.3 are ticked in the plan.

**What stands in 2.5's way.** 2.5 needs the three floor tests and both carry-forward lines green
at the exit of every commit of the step. Three named floors are red:
- 17:37 at 38 of 46;
- 032113 at 43 of 45;
- 032129 at 42 of 64.

The capture and adjudicated floor tests are green. Two arbiter rulings keep the named floors red:
- **443 DECIDED (3):** no floor is re-banked while 17:37's boundaries are worse.
- **448 DECIDED (6):** 443 DECIDED (3) covers 032113 and 032129 as well.

**What changed.** Commit `9a329cdb` of 2026-09-26 17:37 is outside any unit. It appends seven
`RESOLVED:` lines to `PARKED.md`, and one of them answers 17:37's question:

> `leave it red, or re-bank it at 38 | re-bank at 38. HM-REQ-081 is measured per condition and the condition's MET-WBE fell 58 to 52.`

Both rulings rested on that question, and it now has an answer. Unit 459 launched at 21:33Z, four
minutes before that commit, so its instruction could not have acted on the answer.

**This unit:**
1. Re-banks the named floors on that answer.
2. Keeps every commit after that green.
3. Spends the rest of its time on HM-REQ-010's number. A refused change never lands in `src`, so
   2.5 does not cost the step its change.

Three working tasks, plus the exit round. Drop from the back.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST EXIST:      CW_SPEC.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all six are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all six hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules, short

**HM-DEC-155.** No suite:
- Run named types only, one per invocation, each with its own `timeout`.
- Captures get 600 s.
- Never run in the background and poll.
- If one type needs more than 600 s, split it by recording into several named types or filters.
  Never raise the timeout past 600 s.

**Shell limits:**
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, and so is `rm`.
- Python cannot run here.
- A multi-line commit uses `-m` more than once.
- Scripts go in `.run-unit\unit460-<name>.sh` and are run with `sh`.

**The report:**
- The four headings, exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 2 criterion 5`. The launcher reads the digits after "criterion", so this
  means plan line 2.5.
- WHY cites the plan.

**A CW question is answered from the documents and fldigi's source, never raised to the owner**
(R85). Section 4 records the reading in one line, and the work goes on.

---

## 2. Why this unit exists

**The count today.**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | 3 of 5 |
| 3 | 3 of 6 |
| 4 | 5 of 7 |
| 5 | 1 of 6 |
| 6 | 3 of 6 |
| 7 | 0 of 5 |
| 8 | 0 of 6 |
| 9 | 3 of 8 |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The three red named floors are settled on the owner's answer of
            9a329cdb and R78's own test, so every commit of step 2 from task 1
            on exits with the three floor tests and both carry-forward lines
            green, and in the time left one change aimed at HM-REQ-010's
            sure-but-wrong letters is judged under R78, landing in src only if
            kept.
ADVANCES:   step 2 criterion 5
```

**Read `CW_REQUIREMENTS.md` section B and `CW_SPEC.md` sections 5 and 11 first. They win over
this instruction.**
- **HM-REQ-010 (must):** MET-CER-SURE below 1%. The real set is at 33 of 436 (7.6%, inferred key)
  and the synthetic set at 14 of 173 (exact key).
- **HM-REQ-012 (must):** coverage at or above 0.90. A change that meets 010 by printing less is not
  kept.
- **HM-REQ-081:** MET-WBE at or below 5%, measured per condition. This is the metric the owner's
  answer on 17:37 is written in.

**Why 2.5, and why now:**
- **Every open step has a floor line:** 2.5, 3.6, 4.7, 5.6, 6.6, 7.5, 8.6 and 9.8. Each is held by
  the same three rows. Every step's exit waits on them.
- **2.4 cannot flip this pass.** It counts consecutive step-2 units with no kept change, and the
  count is 0 after 449's kept change. This unit, if it keeps nothing, makes it 1.
- **The loop test does not find this approach.**
  - 444 (unit 6) aimed at 2.5 by repairing 17:37's gaps in the decoder, and was refused.
  - 442 re-banked the other rows and left 17:37 red on purpose.
  - Neither had an owner-side answer to the 17:37 question. This unit has one.
- **This is not bookkeeping under R80.** The floors are named in a step-2 criterion, and 2.5 cannot
  be judged without them.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `1f1c915d` or a runner commit on top of it.
- **`PARKED.md`** carries the `RESOLVED:` line quoted above, added by `9a329cdb`. Its header says
  "nothing reads this file back as a decision". That is a mismatch with how this unit uses the
  line. Report it and do not edit the header.
- **`TheNumberCannotBeGamedTests.NamedFloors`** banks 17:37 at 46, 032113 at 45 and 032129 at 64.
  The last two were re-banked by unit 442.
- **`TheSpeedFollowsTheSendersMarkPairsTests`** is in the tree and red, at 28 of 31 sure wrong.
  459 left it there on purpose. It is on neither carry-forward line.
- **Unit 459's refused change** is commit `6a0b65a1`, and its revert is `796f9af4`.
- **`.run-unit\fldigi\`** is untracked. Do not stage it, and do not fetch.
- **The runner's uncommitted writes** are the modified `.run-unit` state files, `PHASE_OUTCOME.md`,
  `PHASE_STATUS.md` and `RUN_LEDGER.md`, plus the untracked `.run-unit\reports\unit-6-output-4.md`
  and `.run-unit\watched.rc`. Commit them as they are.
- **The reload's `RULES_AT` disagreement** (HM-DEC-165 against CPS-DEC-0183) is logged, and it is
  not this unit's.

**Entry figures, 459's exit, to compare against:**
- **Real, inferred:** MET-CER-SURE 33 of 436, MET-INVENTED 33 over 473, coverage 403 over 473,
  MET-WBE 37 (29 inserted, 8 deleted) over 113.
- **Synthetic, exact:** MET-CER-SURE 14 of 173, MET-INVENTED 14 over 252, coverage 159 over 252,
  MET-WBE 44 (13 inserted, 31 deleted) over 84.
- **Adjudicated readings:** 13 of 13.
- **Named floors:** 10 of 13.

**Expected failures, not regressions:**
- the three named floors, at task 0 only;
- `TheSpeedFollowsTheSendersMarkPairsTests`;
- the app carry-forward line losing a test to Avalonia's headless "dispatcher loop" (DECIDED (7)).

---

## 4. Rulings in force - transcribed, do not re-argue

**The keep rule (R78).** A change is kept when all of these hold:
- MET-CER-SURE falls on the real set;
- MET-INVENTED does not rise;
- coverage does not fall;
- MET-WBE does not rise (443 DECIDED (3), which reads MET-WBE into R78);
- the synthetic set is not worse on any of the four;
- the adjudicated readings are unchanged, or move onto their own adjudicated text (R66);
- **V-11** holds: no recording is made worse on a requirement metric to improve another.

A capture row's character count falling is a finding to report, not a rejection.

**The floors:**
- **443 DECIDED (3) and 448 DECIDED (6),** as `9a329cdb` resolved their question (DECIDED (1)
  and (2)). A named floor is re-banked when the requirement metrics of its recording's condition
  are not worse than they were when the floor was last banked. For 17:37 the owner's answer names
  the number: 38.
- **442 DECIDED (2).** 2.5 is read as green at the exit of every commit from the unit's task 1 on.

**The decoders:**
- **R72.** No word, dictionary or callsign prior (HM-REQ-004, HM-DEC-175).
- **HM-REQ-122 and 129.** Nothing under `Cw\Second\` changes. A technique of the port goes into
  ours as a change to ours.
- **458 DECIDED (2), the port's mapping, and 456 DECIDED (2) to (5)** stand.
- **R75 and R76.** If a change touches `CwToneTracker`, the instrument's table goes beside it.

**Fixtures and tests:**
- **V-04 and V-14.** No fixture is altered, re-seeded or dropped. No gate, separation limit,
  confirmation rule or plausibility bound is loosened to pass a fixture.
- **V-13.** A real recording's key is inferred unless it was transcribed. Every number carries its
  key's kind.
- **R77.** A new CW test names the requirement it proves.

**Scope and questions:**
- **R80.** No traceability, test inventory or decision-log work.
- **R85.** A CW question is answered from the documents and the port's source.

**Standing rules:** `CLAUDE.md` §0.0 (never present a guess as a decode), §0.2 (nothing that keys
or transmits) and §12.5. HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task, naming what it will measure or build;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 460 - STEP 2` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 460 with `CURRENT_STEP: 2`, in both copies.
3. Bump the patch from 1.13.146 to 1.13.147.
4. Commit the runner's writes as they are. Do not stage `.run-unit\fldigi\`. Check `git status`
   before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests (named, captures, adjudicated);
   - `TheRequirementsAreMeasuredTests`, real and synthetic.
6. Save every keyed recording's text, with each character's class, to
   `.run-unit\unit460-text-before.txt`.

This commit exits with the three named floors red, as recorded.

### Task 1 - the three named floors, settled (2.5's own work)

For each of 17:37, 032113 and 032129, print the following to `.run-unit\unit460-floors.txt`:
- **The last banking:** the commit where the floor was last banked (17:37 at 46, and 442's
  `0439a8e7` for the other two), and the commit where its count fell.
- **The condition:** the recording's condition as `CW_SPEC.md` names it, or `not stated`, with its
  key's kind.
- **Condition metrics:** that condition's MET-CER-SURE, MET-INVENTED, coverage and MET-WBE at the
  last banking and at HEAD. Take them from committed printouts where they exist. Re-measure only
  where none does.
- **Recording metrics:** the recording's own four metrics, then and now.
- **Its text:** key, text at the last banking, text at HEAD.

**The rule:**
- **17:37** is re-banked at 38 on the owner's answer in `9a329cdb`. If HEAD does not read 38,
  that is a mismatch. Report it and do not re-bank.
- **032113 and 032129** are re-banked at their HEAD counts only where their condition's four
  metrics are all no worse than at the last banking.
- **Where a condition metric is worse,** the row stays red and 2.5 stays open. Say so.
- **Where only the recording's own metrics are worse,** re-bank on the condition test as the
  owner's answer reads. Print the recording's own worsening beside it in section 3, so the owner
  reads it.

**Edit the rows.** Each re-banked row keeps a comment in the form of 442's: `re-banked by unit 460
from <n>`, and `(9a329cdb)` for 17:37 or `(R78, 448 DECIDED (6))` for the other two. Rewrite
17:37's `NOT RE-BANKED` comment to say what happened and when. Change nothing else in the test.

**Run and commit:**
1. Run the three floor tests and both carry-forward lines.
2. Commit. From this commit on, every commit must exit green on all five. Excepted: a named row
   left red by the rule above, and the dispatcher loss under DECIDED (7).

### Task 2 - the trace: where 459's pair speed helped and where it broke

This task reads and prints. **Nothing is committed under `src`.**

1. Apply `6a0b65a1`'s `src` diff to the working tree only.
2. Decode the recordings it moved:
   - **helped:** 003758, 031838 and 032113;
   - **broke:** 031948's `110,`, 004507's `EACH`, 031905's `PREDICTED` and 032050's word boundaries.
3. Restore `src` from HEAD with git. `git diff -- src` prints nothing when this task ends.

For every window in those stretches where the pair speed moved our unit, print to
`.run-unit\unit460-pair-trace.txt`:
- **Pairs:** the dot-dash pairs it rested on, with how many there were and each pair's dit and
  dah in samples;
- **Unit:** our path's unit before and after, and the unit the marks under that window imply;
- **Speed:** whether the move went toward the key's speed or away from it (the key is inferred);
- **fldigi:** what fldigi does at that point in `cw.cxx` at `61b97f41`, with line numbers. That
  covers its seed, its pair acceptance and its averaging.

**Name the difference between the helped windows and the broken ones as a mechanism,** such as a
pair it accepts that fldigi would not, or a count below which fldigi does not move. **Do not name a
constant tuned to the result.**
- If a difference exists, write the second form's rule in full, every constant with the reason it
  has that value, at the foot of the trace. **Commit the trace before any metric is run on the
  second form.** That commit is the rule's registration.
- If no difference separates them, say so. Task 3 is not run, and this unit counts as step 2's
  first of three with no kept change.

### Task 3 - the second form, judged before it lands (drop candidate)

**Only if task 2 registered a rule.** Build that rule, exactly as registered, in the working tree.
Nothing under `Cw\Second\` changes.

**Judge it by section 4's keep rule before any commit:**
- `TheRequirementsAreMeasuredTests`, real and synthetic, all four metrics;
- the adjudicated readings;
- V-11 per recording, over every keyed recording;
- the three floor tests at their task 1 values;
- decode time before and after;
- `TheSpeedFollowsTheSendersMarkPairsTests`, reported, and not a keep condition.

**If kept:**
1. Commit the change on its own. It must exit green on the five.
2. Add MET-CER-SURE before and after, per condition with the key's kind, to
   `docs/phase-requirements/metrics.md`.
3. Re-run `BothDecodersAreScoredAlikeTests` so `parity.md` carries our new row beside the port's
   unchanged one.
4. This is then fldigi's technique taken into ours, which is **9.4**. Tick 9.4 in both copies of
   `PHASE_PLAN.md`, and say so.

**If refused:**
1. Save the diff as `.run-unit\unit460-refused.patch`.
2. Restore `src` from HEAD.
3. Commit only the patch and the numbers that refused it, recording by recording. **No refused
   change is committed under `src`.** That is what keeps 2.5's commits green.

**Print, before and after,** every moved recording's stretch: key, ours before, ours after.

**This is the drop candidate.** If time is short, shed it first. Say so, and say that the unit then
counts as step 2's first of three with no kept change. Tasks 0 to 2 and the exit round are not shed.

### Task 4 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `TheSecondDecoderIsAFaithfulPortTests`;
- `TheSpeedFollowsTheSendersMarkPairsTests`.

**Also print:**
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- `git diff 1f1c915d -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- our texts against task 0's save, with every changed recording before and after;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- **a table of every commit this unit made, with the five results at its exit.**

**Ticks:**
- **Tick 2.5 in both copies of `PHASE_PLAN.md`** only if all of these hold:
  - that table shows every commit from task 1 on green on all five;
  - no named row was left red;
  - the exit round agrees.
- Otherwise leave 2.5 open, and name the commit and the type that held it.
- Do not tick 3.6, 4.7, 5.6, 6.6, 7.5, 8.6 or 9.8. Report which of them the floors no longer hold.

---

## 7. Parked - do not touch, do not raise

- **The four questions `PARKED.md` records, and their `RESOLVED:` answers,** other than the 17:37
  line this unit applies. Read them, do not re-work them.
- **6.5's tick, and HM-REQ-084's `ABOVE`** on 013637.
- **Unit 455's two 6.1 findings.**
- **fldigi's squelch default.**
- **457's section 4 items 1 and 2, and 458's items 2 to 5.**
- **459's section 4 items 1, 4 and 5.** The red test stays, the departure-list mismatches are
  logged, and the seed note is logged.

## 8. Do not

- Do not change any line under `src\Hamlet.RadioEngine\Cw\Second\`.
- Do not change `CwMetrics`, the scorer, `MorseAlphabet` or any key.
- **Do not commit a refused change under `src`.**
- Do not commit a watched-red test in this unit.
- Do not re-bank any floor other than the three named rows. Do not touch the captures or the
  adjudicated tables.
- Do not choose task 3's rule after seeing its metrics. Do not narrow 459's rule by an edge or an
  average picked from R78's numbers.
- Do not raise the speed search bounds. That is 5.1.
- Do not take anything that knows words, callsigns or letter frequencies (R72).
- Do not alter, re-seed, trim, pad or drop any recording or synthetic case (V-04, V-14).
- Do not edit `PARKED.md`, or any ruling in `PHASE_PLAN.md` or `CLAUDE.md`.
- Do not install any package. That is `MOVE: stop`.
- Do not stage, commit or modify `.run-unit\fldigi\`, and do not fetch.
- Do not re-point or retire an existing test (R80).
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose a
  timestamp.**

## 9. Committing and pushing

**One commit per task:**
- task 0;
- task 1;
- task 2's trace, which registers the rule;
- task 3's change or its refusal record;
- task 4.

**Messages** take the form `unit460 task N: <what> (2.5)`.

**Exit state:** from task 1 on, the build, both carry-forward lines and the three floor tests are
green at the exit of every commit. The unit's whole purpose is that this holds.

**Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes first.**
`validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 <n> of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 0 of 5, 8 0 of 6, 9 <n> of 8.
B. Step 2, criterion 2.5: named floors 17:37 <46 to n>, 032113 <45 to n>,
   032129 <64 to n>, each on <9a329cdb | the condition test | left red>;
   commits from task 1 on green on all five: <n> of <m>; 2.5 <ticked | open>.
   HM-REQ-010: task 2 <registered a rule | found no mechanism>; task 3
   <kept | refused | not run | dropped>; real MET-CER-SURE <b> to <a> of sure,
   MET-INVENTED <b> to <a>, coverage <b> to <a>, MET-WBE <b> to <a>;
   synthetic the same; adjudicated <n> of 13; V-11 <n> of <m> worse;
   2.4's count <0 | 1> of 3; 9.4 <ticked | open>.
C. The findings weighed against A and B: how many items section 4 raises,
   and whether any is in the way of 2.4, 2.5 or HM-REQ-010.
```

```
UNIT:       460 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     named floors <n> of 13; commits green from task 1 <n> of <m>; HM-REQ-010 real MET-CER-SURE <b>/<sure> to <a>/<sure> (inferred), synthetic <b> to <a> (exact); coverage real <b> to <a>; MET-INVENTED real <b> to <a>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 9 <n>
```

**Section 3 leads with the three floors.** For each, give its key, its text at the last banking,
its text at HEAD, the condition's four metrics then and now, and the recording's own four then and
now. Then give:
1. the commit table, with the five results at each commit;
2. task 2's windows, helped against broken, and the rule registered or the word none;
3. if task 3 ran, the per-condition table of the four metrics before and after with the key's kind
   and decode time, V-11 per recording, and every moved stretch's text.

**Section 2, one paragraph.** Say what the operator will see:
- if nothing under `src` changed, say that the screen reads exactly as before and why;
- if a change was kept, say what changes, in letters.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: re-bank the three red named floors under the owner's resolution of 17:37 and R78's per-condition test, then a step-2 change judged in the working tree and committed only if kept
MOVE: work around
WHY: PHASE_PLAN.md step 2 line 2.5 asks that the three floor tests and both carry-forward lines be green at the exit of every commit of the step, and only three named floor rows hold it. Both rulings keeping them red, 443 DECIDED (3) and 448 DECIDED (6), rested on whether 17:37 is re-banked while its boundaries are worse, which the owner-side commit 9a329cdb has since answered: re-bank at 38. 2.4 cannot flip this pass, its count being 0 after 449's kept change, and the loop test finds no entry for this approach.
STATE: partial
DECIDED: author's, overrulable - (1) the RESOLVED line of 9a329cdb, committed outside any unit, is read as the owner's answer to the question 443 DECIDED (3) rested on, so 17:37 is re-banked at 38 on it; this applies a later answer and overrules no arbiter ruling, and PARKED.md's header saying nothing reads it back is reported as a mismatch; (2) self-ruling 1, citing PHASE_PLAN.md R78's line that a capture row's count falling is a finding when no requirement metric got worse - 448 DECIDED (6) is applied as written, 443 DECIDED (3) covering 032113 and 032129, with 443 DECIDED (3) as the owner resolved it: re-banked where their condition's four metrics are no worse than at the last banking, the recording's own metrics printed beside; (3) 2.5 is judged by 442 DECIDED (2), green at every commit from task 1 on, so the unit commits no watched-red test and no refused change under src, a refused change being kept as a patch under .run-unit; (4) task 3's rule is registered in a committed trace before any metric runs on it, and a constant chosen from R78's numbers is refused; (5) a change kept in task 3 is fldigi's technique taken into ours, so 9.4 is ticked with it; (6) no floor line of steps 3 to 9 is ticked here, and the report names which the floors no longer hold; (7) the app line's headless dispatcher-loop loss: one rerun, and any type lost again is run alone and must pass, named in the report; (8) 459's section 4 items 1, 4 and 5 are logged, not chased, and its item 2 is used only to shape task 2's trace.
LICENCE: PHASE_PLAN.md step 2 lines 2.4 and 2.5, R78, R80, R81, R85, sections 5 and 6; owner-side commit 9a329cdb in PARKED.md; arbiter rulings 442 DECIDED (2), 443 DECIDED (3), 448 DECIDED (6), 456 DECIDED (2) to (5), 458 DECIDED (2); CW_REQUIREMENTS.md HM-REQ-004, 010, 012, 081, 122, 129; CW_SPEC.md sections 5 and 11; R66, R72, R75, R76, R77; V-04, V-11, V-13, V-14; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the three reds every step's exit has waited on since unit 441 are settled on the owner's own answer, so step 2 can show clean commits, and one more measured attempt is made at the letters the decoder prints confidently wrong without any refused attempt landing in the product
ADVANCES: step 2 criterion 5
END-ARBITER-DECISION
```
