# Work instruction 471 - the letters read while acquiring are read again at the pitch and speed once proved, before they settle: step 2's third attempt at HM-REQ-010, and if it keeps nothing, 2.4 closes the step

**Loop unit.** Step 2 (`CW_REQUIREMENTS.md` HM-REQ-010, judged under R78 with HM-REQ-011, 012
and MET-WBE as guards; HM-REQ-102 measured beside it).

**Why step 2, and why this route.**
- **The launcher names step 2, and its only open line is 2.4.** 2.4 closes the step partial after
  three consecutive units with no kept change. The count is **2 of 3**: unit 449 kept a change,
  and units 460 and 470 kept none. **This unit settles 2.4 either way.** If it keeps a change,
  MET-CER-SURE falls and the count goes to 0. If it keeps none, the trace goes to `PARKED.md`
  and 2.4 is ticked.
- **The largest group left is the letters read while acquiring, and dimming them has been
  measured and refused.** Unit 470 found 29 of the 33 real sure-but-wrong letters inside
  acquiring. All 13 of unit 449's acquisition group were there, every one read with pitch none
  and speed hypothesis, before the first keyed verdict. Dimming them cost 293 right letters, and
  coverage fell from 403 to 110 of 473. **So this unit does not dim. It reads again.** When both
  proof states first become proved, every letter from the acquiring stretch that the stream has
  not yet settled is decoded again at the proved pitch and speed. Letters read at a pitch the
  tracker never had can come out right, and no right letter has to be given up to catch a wrong
  one.
- **The route is new.** The loop test finds no entry for it. It is none of step 2's excluded
  routes: rival margin, marks' speed, gap duration, mark-shape edge, from-cold move, speed
  bounds, the acquisition gate in any form, and the fldigi mechanisms (A) to (E) and (W). It
  changes neither the tracker nor either proof state. It reads them.
- **R86's words bar steps 2 to 8 until 9.4 has a kept change.** 9.4 has no route left. This is
  carried to the owner as a mismatch, as units 468 to 470 did (DECIDED (1)). It is not a claim
  that R86 is overruled.

Four tasks after task 0. Drop from the back.

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

**HM-DEC-155. No suite:**
- Run named types only, one per invocation, each with its own `timeout`.
- Captures get 600 s.
- Never run in the background and poll.
- If one type needs more than 600 s, split it by profile or by case into several named types or
  filters. Never raise the timeout past 600 s.

**Shell limits:**
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, and so is `rm`.
- Python cannot run here. Everything is C#.
- A multi-line commit uses `-m` more than once.
- Scripts go in `.run-unit\unit471-<name>.sh` and are run with `sh`. Unit 470's
  `.run-unit\unit470-*.sh` helpers may be copied and renamed.
- Run `tools/status.sh` from the repository root only.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 2 criterion 4`, as the decision block does. Report whether it flipped.
  If a change was kept, it did not flip, and the report says so plainly.
- WHY cites the plan.

**R85. A CW question is answered from the documents, never raised to the owner.** Which pitch
and speed count as "the proved values", which letters are still unsettled, and what class a
re-read letter takes are all such questions. DECIDED (3) and (4) answer the ones foreseen. If
the tree forces a reading this instruction did not foresee, record it in one line of section 4
and carry on. **Do not hand the question back.**

---

## 2. Why this unit exists

**The count today, by the plan's checkboxes:**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | 4 of 5 (2.4 open, its count 2 of 3) |
| 3 | 3 of 6 |
| 4 | 5 of 7 |
| 5 | 1 of 6 |
| 6 | 3 of 6 |
| 7 | 4 of 5 (7.4 open) |
| 8 | 0 of 6 |
| 9 | 7 of 8 (9.4 open, and every mechanism 9.3 named is refused) |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The letters our decoder read while it was still acquiring,
            and has not yet settled, are read again at the pitch and
            speed that were then proved, before they settle. The change
            is judged under R78 as step 2's change against HM-REQ-010.
            If it is kept, step 2's count goes to 0. If it is refused, or
            there is nothing for it to act on, this is the third unit
            with no kept change: the trace goes to PARKED.md and 2.4 is
            ticked.
ADVANCES:   step 2 criterion 4
```

**Read these first. They win over this instruction:**
- `CW_REQUIREMENTS.md`:
  - HM-REQ-010, 011, 012 and 014;
  - HM-REQ-034 and 093, the proof states;
  - HM-REQ-083, one set of word boundaries between the live and settled renderings;
  - HM-REQ-100, 101 and 102, and their verification rows.
- `CW_SPEC.md` section 11, the metrics, and section 4, what a condition is.
- `PARKED.md`:
  - its header;
  - the four `RESOLVED:` lines of 2026-09-26.
- `docs/phase-requirements/metrics.md`: the sections for units 449 and 470.
- `.run-unit\unit470-trace.txt` and `WhatTheSureLettersWerePrintedUnderFact`. This is 470's
  table of the sure letters inside and outside acquiring.
- The code:
  - `CwProbabilisticStream`: its settled mark, its decision window, and the comment *"NOTHING
    ALREADY SETTLED IS RETRACTED"*;
  - `CwProbabilisticDecoder`, and where it assigns a character's class;
  - `CwPitchProof` and `CwSpeedProof`, and where each is computed;
  - which path the harness behind `TheRequirementsAreMeasuredTests` scores, and which path
    feeds the CW tab.

**The requirements, verbatim:**

> **HM-REQ-010:** "On every must-tier condition at or above the sensitivity floor, the decoder
> shall keep MET-CER-SURE below 1 %."
>
> **HM-REQ-012:** "On every must-tier condition at the sensitivity floor, the decoder shall emit at
> least 90 % of sent characters as sure (MET-COVERAGE ≥ 0.90)." Rationale: "Without a floor,
> dimming everything satisfies HM-REQ-010."
>
> **HM-REQ-102:** "While acquiring, the decoder shall emit no sure character."

**This unit succeeds when one of these is true:**
1. **Kept.** The re-read is committed in its own commit, under R78 as 2.2 reads it:
   - MET-CER-SURE falls;
   - MET-INVENTED does not rise;
   - MET-COVERAGE does not fall;
   - the adjudicated readings hold, or move onto their own adjudicated text;
   - V-11 holds on all 35 recordings;
   - the three floor tests, both carry-forward lines and `TheArbitrationEarnsItsPlaceTests` are
     green.

   Every metric is given per condition, real and synthetic, with the key's kind beside each
   figure. Step 2's count goes to **0**, and 2.4 stays open.
2. **Refused, or nothing to act on.** Refused means the change is measured and refused on a
   named metric. Nothing to act on means task 1 shows that no letter from the acquiring stretch
   is still unsettled when both proof states become proved. Either way, the change is kept as a
   patch under `.run-unit\`, and nothing under `src` is committed. Step 2's count reaches **3 of
   3**. The trace goes to `PARKED.md` under DECIDED (6), and 2.4 is ticked in both copies of the
   plan.

**Either outcome is the step's answer.** The second is 2.4 doing what it was written for, and
not a failure of this unit. Do not bend the rule to avoid it.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `a2e07f6d` or a runner commit on top of it. The version is 1.13.157.
- **`PHASE_PLAN.md`**, in both copies:
  - shows 2.1, 2.2, 2.3 and 2.5 ticked and 2.4 open;
  - shows 9.4 as step 9's only open line;
  - carries R86 in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **`PARKED.md`'s header** says the file is *"Written by run-phase.bat, never by a session"*.
  Quote it. It is why DECIDED (6) reports a mismatch.
- **Unit 470's patch** `.run-unit\unit470-gate.patch` exists. **Do not apply it.**
- **The port** is untouched: `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints
  nothing.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-9-output-2.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
  `SESSION.lock` is the launcher's. Do not stage it.
- **Logged and not this unit's:**
  - the reload's `RULES_AT` disagreement (HM-DEC-165 against CPS-DEC-0183);
  - `outcome-read`'s step titles for steps 2, 3 and 8, which differ from `PHASE_PLAN.md`'s.

**Entry figures, 470's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278, or 277 of 278 with the lost type passing alone under DECIDED (8).
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:**
  - MET-CER-SURE 33 of 436;
  - MET-INVENTED 33 over 473;
  - coverage 403 over 473;
  - MET-WBE 37 over 113.
- **Synthetic set, 12 cases, exact key:**
  - MET-CER-SURE 14 of 173;
  - MET-INVENTED 14 over 252;
  - coverage 159 over 252;
  - MET-WBE 44 over 84.
- **`TheArbitrationEarnsItsPlaceTests`:** both parts green.
- **Whole texts:** byte-identical to `.run-unit\unit470-texts-entry.txt`.

If any entry figure differs, report it and use the measured one.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31. It is on neither carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under
  DECIDED (8).
- The parity run rewriting the decode-time rows of `parity.md`. Keep a copy under `.run-unit\` and
  restore the committed file, as units 462 to 470 did.
- **The re-read refused under R78, or finding nothing to act on.** Both are outcomes this
  instruction foresees, and neither is a regression.

---

## 4. Rulings in force - transcribed, do not re-argue

**The step and its keep rule:**
- **Step 2, 2.2:** *"Each change is built in its own commit and kept under R78: MET-CER-SURE
  falls, MET-INVENTED does not rise, MET-COVERAGE does not fall, the three adjudicated readings are
  unchanged or move onto their own adjudicated text, and V-11 holds - no capture is reddened to
  green a newer one."*
- **Step 2, 2.4:** *"After three consecutive units with no kept change the trace goes to
  `PARKED.md` and the step closes partial."*
- **Step 2, 2.5:** *"The three floor tests and both carry-forward lines are green at the exit of
  every commit of the step."*
- **R78:** *"A change is kept when it moves a requirement's metric the right way and breaks no
  other requirement. MET-INVENTED at zero, MET-CER-SURE below 1%, MET-COVERAGE at or above 90%,
  MET-WBE at or below 5%, per condition, at the tier the requirement names. The capture floors stay
  as V-11's overfitting guard ... and stop being the keep rule. A capture row's character count
  falling is a finding to report, not a rejection, when no requirement's metric got worse."*
- **R81:** step 2 is MET-CER-SURE.
- **442 DECIDED (2):** 2.5 is judged green at every commit. This unit commits no test that was
  watched red and no refused change under `src`, as units 460 and 470 did.
- **449 DECIDED (2):** *"the unit counts as one with no kept change if its change is refused
  under R78 or if the trace finds no group outside the excluded routes."*
- **449 DECIDED (3):** *"2.4's trace going to PARKED.md is done by appending one line in the
  file's own form with drift as the stop field, naming the committed trace files, and the file
  header's never-by-a-session line is reported as a mismatch."*
- **The owner's ruling of 2026-09-23:** a step's state is its checkboxes. Once 2.4 is ticked,
  every line of step 2 is ticked and the step is closed.

**The ordering, and the mismatch this unit carries:**
- **R86** (`PHASE_PLAN.md` section 6): *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."* This
  unit is authored against it under DECIDED (1). That is reported to the owner as a mismatch. It
  is not a claim that R86 is overruled.
- **`ARBITER.md` section 4:** *"If you judge it a loop, the step ends. Say so, name the approaches
  that were tried, and move to another step or declare it unachievable."* 9.4 is so judged.

**The answers the proof states rest on** (`PARKED.md`, `RESOLVED:`, 2026-09-26, author's under
R85, overrulable):
- **Acquiring:** *"acquiring runs from the first keyed element at a tracked pitch until both the
  pitch (HM-REQ-093) and the speed (HM-REQ-034) carry proof state proved; while either is
  hypothesis or none, no character is emitted sure."*
- **A heard pitch proved:** *"pitch is proved when the independent survey admits keying at that
  pitch and at least one whole character has resolved at it."*
- **A speed proved:** *"a speed is proved only while characters resolve at it with the marks' unit
  within 10 percent of the path's (HM-REQ-031); when the tone stops or the pitch changes it drops
  to hypothesis."*

**Standing rulings this unit leans on:**
- **R12.** A session rewrites its own tests.
- **R61.** A key is inferred unless it was transcribed.
- **R66.** An adjudicated reading may move onto its own adjudicated text.
- **R72.** No word, dictionary or callsign prior, in any form. **The re-read uses no key and no
  text. It uses only the audio, the proved pitch and the proved speed.**
- **R75 and R76.** The tone tracker is open to change, but not in this unit.
- **R77.** The test names HM-REQ-010 and HM-REQ-102.
- **R80.** No traceability, test inventory or decision-log work.
- **V-04, V-08, V-11, V-13, V-14.**
- **HM-REQ-122 and 129:** the second decoder is faithful, not improved. Not one line under
  `Cw\Second\` changes.
- **Section 6:** *"Where this plan and the two documents differ, the documents win, and the
  difference is a finding in the report."*

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode.
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched.**
- `CLAUDE.md` §12.5: a fixture built from the same misunderstanding as the code proves nothing.
  The test's red case is a generated fixture whose key is exact by construction.
- **The stream's own rule: nothing already settled is retracted.** The screen promises the
  operator that settled text does not change under them. **This unit keeps that promise
  exactly** (DECIDED (3)). A form of the change that needs to retract a settled character is not
  built. It is a line in section 4, and the unit counts as nothing to act on.
- HM-DEC-095, HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task;
- when task 1's reading of the code is written, naming the path the harness scores and the
  window's length;
- when the trace's table is in, naming wrong-to-right and right-to-wrong on the real set;
- when the rule is registered, with the commit hash;
- when the test comes back red, naming the case;
- at the verdict, naming the deciding metric, or saying there was nothing to act on;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 471 - STEP 2` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 471 with `CURRENT_STEP: 2`, in both copies.
3. Bump the patch by one from what the tree carries. It should be 1.13.157, going to 1.13.158.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `TheArbitrationEarnsItsPlaceTests`, both parts.

   Save every recording's text, with every character's class, to
   `.run-unit\unit471-texts-entry.txt`. Diff it against `.run-unit\unit470-texts-entry.txt`.

### Task 1 - the trace: what the acquiring letters read at the proved values

**Nothing under `src` changes in this task.**

1. **Read the code first.** Before any decode, write these into `.run-unit\unit471-trace.txt`:
   - **The path.** Which path the harness behind `TheRequirementsAreMeasuredTests` scores: the
     stream, the whole-file decode, or the arbiter over either. Which path feeds the CW tab. If
     they differ, say how. **The change must behave the same on the path that is scored and on
     the path the operator reads.** A change that only the scored path sees is not built
     (DECIDED (3)).
   - **The window.** Where the stream's settled mark is. How long its decision window is, in
     seconds and in dits at the speed in force. What may still change inside the window, and
     what may not.
   - **The proof states.** When `CwPitchProof` and `CwSpeedProof` are known relative to the
     settled mark. At the moment both first become proved, which characters from the acquiring
     stretch are still unsettled.
   - **Re-reading.** Whether the decoder can decode a span again at a given pitch and speed
     without retracting anything, and where that would sit.
2. **Write `WhatTheAcquiringLettersReadAtTheProvedValuesFact`**, naming HM-REQ-010 and HM-REQ-102.
   It asserts nothing. It drives through the same harness and the same `Measure` and `CwMetrics`
   calls as `TheRequirementsAreMeasuredTests`:
   - the 23 real keyed recordings;
   - the 12 synthetic cases.

   Take each character emitted while acquiring, in 470's sense: either proof state was not
   proved. For each one, print:
   - the recording and the span;
   - what the key says was sent;
   - what HEAD emitted, and its class;
   - the pitch and speed it was read at;
   - the pitch and speed first proved after it;
   - whether it was still unsettled at that moment;
   - what the same span reads when decoded at the proved pitch and speed, and the class it
     gets.

   Table these, per condition and with the key's kind, for the letters still unsettled at
   proof:
   - wrong to right;
   - right to wrong;
   - right to right;
   - wrong to wrong;
   - sure to dim;
   - dim to sure;
   - letters added;
   - letters lost.

   **Separately, count the letters that settled before proof.** The change cannot reach those.
   The first table is what the change can gain. The second is what is out of its reach.
   **Print both before building anything.**
3. **Place unit 449's 13 and unit 470's 29** in the two tables, by recording and span.
4. **Register the rule in the trace file before any after-figure exists.** Write DECIDED (3) as
   the tree will implement it, and name the file and method it goes into. Commit the fact and the
   trace. The fact asserts nothing, so the five stay green.
5. **If the first table is empty** on both sets, nothing is left for the change to act on. Say
   so in the trace, skip task 2, and go to task 3 as a unit with no kept change (449 DECIDED (2)).

### Task 2 - the re-read, judged under R78

**Build it in the working tree. Commit it only if kept.**

1. **The test first.** Write `TheAcquiringStretchIsReadAtTheProvedValuesTests`, naming
   HM-REQ-010 and HM-REQ-102. Pick a generated case whose key is exact by construction, where the
   trace shows HEAD settling a wrong letter that was unsettled when proof arrived and that reads
   right at the proved values. Assert that the settled text on that span is what was sent, and
   that no settled character changed after it settled. **Watch it red at HEAD** and save the
   output to `.run-unit\unit471-red.txt`. If no generated case shows it, a cold TX-ITU start or
   469's INT-CARRIER(+100 Hz, 0 dB) is where to look. If none shows it, say so, and judge the
   change on R78 alone.
2. **Build the re-read under DECIDED (3)**, in our decoder only. Then run the test green.
3. **Judge it under R78**, per condition, real and synthetic, with the key's kind beside each
   figure. Print the table before and after for:
   - MET-CER-SURE;
   - MET-INVENTED;
   - sure-and-right coverage;
   - MET-WBE;
   - dim precision (HM-REQ-014);
   - the adjudicated readings;
   - V-11 on each of the 35 recordings;
   - the three floor tests;
   - both carry-forward lines;
   - `TheArbitrationEarnsItsPlaceTests`.

   **Beside the table, print these three:**
   - **HM-REQ-102 on the 7.052 opening**: the text with every character's class, before and
     after, and the count of sure letters while acquiring.
   - **Live against settled word boundaries on every span the change touched** (HM-REQ-083).
     Report it. It is not a keep gate, and 6.4 is not ticked.
   - **Decode time**, before and after, on the real set.
4. **The verdict:**
   - **Kept:** every line of 2.2 holds. Commit the change and its test together as one commit.
     Write the before and after into `metrics.md` as a new `## Unit 471` section, in the form
     unit 449 used, with the texts that changed. Step 2's count goes to 0. Skip task 3.
   - **Refused:** name the metric and the condition that refused it. Save the change and the
     test as `.run-unit\unit471-reread.patch`, and restore `src` and `tests` to HEAD. Write the
     refusal and its table into `metrics.md` under `## Unit 471`, in the form unit 470 used.
     Commit only the documents and the `.run-unit` files.

   **Do not try a second form.** A longer window, a hold on settling, a cap or a grace period
   would be a new constant fitted to the trace (V-14). If the trace suggests one, it goes in one
   line of section 4.

**Drop candidate:** the decode-time line, then the live-against-settled line, then the synthetic
columns of task 1's second table. These are never shed: task 1's reading of the code, the
first table on both sets, the rule registered before the build, the test watched red where a
case exists, the full R78 table, and the 7.052 opening's text before and after.

### Task 3 - only if no change was kept: the trace goes to `PARKED.md` and 2.4 closes

**Run this task only if task 2 refused the change, or task 1 found nothing to act on.**

1. **Write the step's closing section into `metrics.md`**, as `## Step 2 - closed partial at unit
   471`. Keep it to one table and one short list:
   - MET-CER-SURE and coverage, real and synthetic, with the key's kind: unit 439's figure (54
     sure-but-wrong), 449's kept change, and today's figure;
   - HM-REQ-010's threshold, below 1 %, beside today's figure, per condition;
   - what is left, grouped as 470's and this unit's traces group it: inside acquiring and
     settled before proof; inside acquiring and re-read; outside acquiring;
   - every route tried at step 2, one line each, with the unit and its verdict.
2. **Append one line to `PARKED.md`**, under 449 DECIDED (3), in the file's own form:
   `PARKED: 2.4 | unit <n> launched <launch time from PHASE_OUTCOME.md> | <date time as the tree
   gives it> | drift | <the judge's words>`. In the judge's words, say in one sentence that step 2
   closed partial after three units with no kept change, that MET-CER-SURE stands at <real> and
   <synthetic> against HM-REQ-010's one in a hundred, and which committed files hold the trace:
   `metrics.md`'s step-2 sections, `.run-unit\unit470-trace.txt`, `.run-unit\unit471-trace.txt`,
   and the two facts. **Take the launch time from the tree. Do not compose a timestamp.** If the
   tree gives none, write `not recorded` in that field.
3. **Tick 2.4** in both copies of `PHASE_PLAN.md`: change `- [ ] 2.4` to `- [x] 2.4`. Change
   nothing else in either file.
4. Commit the three together as one commit.

**Do not set any step's state by hand.** The launcher reads the checkboxes.

### Task 4 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `TheArbitrationEarnsItsPlaceTests`, both parts;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff a2e07f6d -- src`. If the change was kept, this is only the change. If not, it prints
  nothing;
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with its results on the five at its exit: build, both
  carry-forward lines, and the three floor tests.

**Ticks:** 2.4 only, and only through task 3. **Do not tick 4.4, 6.4, 9.4** or any line of steps
3 to 9.

---

## 7. Parked - do not touch, do not raise

- **9.4 and every mechanism 9.3 named, (A) to (E) and (W), in any form.** The loop is ended.
- **Every excluded step-2 route:**
  - rival margin;
  - marks' speed;
  - gap duration;
  - mark-shape edge;
  - from-cold move;
  - speed bounds;
  - **the acquisition gate, in any form**: speed only, the 448 span only, a grace period.
    Unit 470 refused it on MET-COVERAGE. Its patch is not applied.
- **`CwToneTracker`, `CwToneSurvey`, the competing-station logic, and how either proof state is
  computed.** The change reads the proof states. It does not change them. Unit 470's section 4
  item 4, the proof states against their `RESOLVED:` lines, is logged and not repaired.
- **Every switch in `CwSwitchTable`, the vote table, both confidences, calibration and
  HM-REQ-127's margin.** Unit 470's item 5, the gate being ours only under the arbiter, is
  logged.
- **TX-STRAIGHT and TX-SLOPPY; HM-REQ-008, 051, 054, 063, 064 and 065.**
- **3.4, 5.1, 6.1, 6.4 and 7.4.**
- **The two `PHASE_STATUS.md` copies differing beyond the unit's fields.**

## 8. Do not

- **Do not retract, rewrite or re-class any character after it has settled.** The re-read acts
  only on characters that are still unsettled.
- **Do not lengthen the decision window, hold the settled mark, or add a constant, a cap or a
  grace period** (V-14). The window is the tree's, as it stands.
- **Do not change what "acquiring" or "proved" means**, or how either proof state is computed.
- **Do not dim a letter to catch a wrong one.** That was 470's route, and it is refused.
- **Do not change the keep rule.** Coverage falling refuses the change.
- **Do not build a change that only the scored path sees** (DECIDED (3)).
- **Do not commit a watched-red test or a refused change under `src` or `tests`** (442 DECIDED
  (2)).
- **Do not read any key or text into the decoder** (R72).
- **Do not touch, call or reuse `KeyerCwSender.cs`** or any transmit file (§0.2).
- Do not add a new test to either carry-forward line.
- Do not re-point or retire an existing test (R80).
- Do not edit `CW_SPEC.md`, `CW_REQUIREMENTS.md`, or any ruling in `PHASE_PLAN.md` or
  `CLAUDE.md`. In `PARKED.md`, write only task 3's one appended line, and edit no line already
  there.
- **Do not install any package. That is `MOVE: stop`.**
- Do not stage, commit or modify `.run-unit\fldigi\`, and do not fetch.
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose a
  timestamp.**

## 9. Committing and pushing

**One commit per step:**
- task 0;
- task 1's fact and trace, with the rule registered;
- task 2: either the kept change with its test and `metrics.md`, or, if refused, the documents
  and the `.run-unit` files only;
- task 3, if it runs: `metrics.md`'s closing section, the `PARKED.md` line and the 2.4 tick;
- task 4.

**Messages** take the form `unit471 task N: <what> (2.4)`. Task 2's message names the verdict and
the deciding metric, with MET-CER-SURE and coverage before and after, real and synthetic.
Task 3's message says that step 2 closed partial.

**Exit state:** every commit exits with these green:
- the build;
- both carry-forward lines;
- the three floor tests.

The one exception is the dispatcher loss under DECIDED (8).

**Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes first.**
`validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 <4|5> of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6,
   6 3 of 6, 7 4 of 5, 8 0 of 6, 9 7 of 8; 9.4 ended as a loop, and
   this unit worked step 2 against R86's words, reported as a mismatch.
B. Step 2 (HM-REQ-010, with HM-REQ-102 measured): the acquiring
   stretch re-read at the proved pitch and speed before it settles -
   <kept | refused on <metric, condition> | nothing to act on>;
   letters unsettled at proof <n>, wrong to right <n>, right to wrong
   <n>; real MET-CER-SURE <before> -> <after>, coverage <before> ->
   <after>; synthetic the same; 2.4 <open, count 0 | ticked, the trace
   in PARKED.md, step 2 closed partial with MET-CER-SURE at <real> and
   <synthetic> against one in a hundred>.
C. The findings weighed against A and B: how many items section 4
   raises, whether any is in the way of 2.4, and the R86 mismatch
   stated in one line.
```

```
UNIT:       471 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes - step 2 criterion 4, ticked | no - step 2 criterion 4, a change was kept and the count is 0>
NUMBER:     MET-CER-SURE real <x/y> -> <x/y>, synthetic <x/y> -> <x/y>; coverage real <x> -> <x>; re-read <kept|refused|nothing to act on>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the R78 table**, per condition, before and after, with the key's kind. If
there was nothing to act on, it leads with task 1's two tables instead. Then give:
1. task 1's reading of the code: the path, the window and when proof arrives;
2. task 1's two tables, and where 449's 13 and 470's 29 fall;
3. the 7.052 opening's text before and after, with each character's class;
4. every recording whose text or classes changed, before and after;
5. if task 3 ran: the closing table, and the `PARKED.md` line as written;
6. the commit table, with the five results at each commit.

**Section 2, one paragraph, in the owner's words.**
- **If kept:** say that when Hamlet first locks onto a station, the letters it had only guessed
  at are now read again once it knows the station's pitch and speed, so fewer wrong letters
  reach the screen looking certain. Quote a stretch from before and after.
- **If not kept:** say that nothing on the screen changes, and that the work on confidently
  wrong letters is closed for this phase. Say how many are left against the requirement's one in
  a hundred, and that the reason they remain is written down.

**Section 4 must say, as a plain reading and not a ruling request:**
- **The R86 mismatch**, in one line, as units 468 to 470 put it.
- **DECIDED (3) and (4)**, the reading of "re-read" and of "the proved values", as the arbiter's
  and overrulable.
- **If task 3 ran:** the `PARKED.md` header mismatch under 449 DECIDED (3). Also that ticking 2.4
  leaves every line of step 2 ticked, so the launcher reads the step as closed, while HM-REQ-010
  is not met. That is 2.4's own words, "closes partial", set against the checkbox rule. It is a
  finding, not a request.
- **2.4's state after this unit.**

---

```
ARBITER-DECISION
STEP: 2
APPROACH: re-decode the letters emitted while acquiring and still unsettled in the stream's existing decision window at the pitch and speed first proved, before they settle, nothing settled retracted and no constant added, traced first for wrong-to-right and right-to-wrong, test watched red on a generated exact-key case, judged under R78 against HM-REQ-010, committed if kept, else the step-2 trace appended to PARKED.md and 2.4 ticked
MOVE: work around
WHY: PHASE_PLAN.md step 2 has only 2.4 open, and its count is 2 of 3 (460 and 470 kept nothing), so this unit settles it: either one more real attempt at HM-REQ-010 keeps a change, or the step closes partial as 2.4 is written. Unit 470 measured that 29 of the 33 real sure-but-wrong letters sit inside acquiring and that dimming them fails HM-REQ-012, so this route corrects those letters rather than dimming them, and the loop test finds no entry for it.
STATE: partial
DECIDED: author's, overrulable - (1) step 2 is worked as the launcher named it although R86 (PHASE_PLAN.md section 6) reads that steps 2 to 8 are not authorable until 9.4 has a kept change; this is carried to the owner as a mismatch, as units 468 to 470 did, and not a claim that R86 is overruled, 9.4 staying judged a loop under ARBITER.md section 4 with (A) to (E) and (W) refused; (2) the acquisition gate in every form is excluded as refused by 470 on MET-COVERAGE, and its narrower forms are not tried (V-14); (3) the re-read: at the moment both CwPitchProof and CwSpeedProof are first proved, each character emitted while acquiring that the stream has not yet settled is decoded again from the same audio at the proved pitch and speed and settles as that reading, with the class the decoder assigns it; characters already settled are never retracted, the decision window and settled mark are not lengthened or held, no constant is added, neither proof state nor the tracker changes, and the change must act the same on the path the harness scores and the path the CW tab reads, else it is not built; (4) the proved values are the pitch and speed the tree reports with proof state proved at that moment, read as they stand; (5) the keep rule is 2.2 as written, per condition, real and synthetic, with TheArbitrationEarnsItsPlaceTests green as a further gate, one form only; (6) the unit counts as one with no kept change if the change is refused under R78 or if task 1 finds no unsettled acquiring letter to act on (449 DECIDED (2)); then step 2's count is 3 of 3, a closing section goes into metrics.md, one line is appended to PARKED.md under 449 DECIDED (3) with drift as the stop field naming the committed trace files and the header's never-by-a-session line reported as a mismatch, and 2.4 is ticked in both copies of PHASE_PLAN.md; if kept, the count is 0 and nothing is ticked; (7) ADVANCES names step 2 criterion 4 because this unit either flips it or keeps the change it waits on; (8) the app line's headless dispatcher-loop loss gets one rerun, and any type lost again is run alone and must pass, named in the report; (9) 470's section 4 is logged, not chased: item 3 is applied only as (2), items 4 and 5 are parked, and item 6 is the count above.
LICENCE: PHASE_PLAN.md step 2 lines 2.2, 2.4 and 2.5, section 5, section 6 (R86 as reported, V-14, the documents win, the second decoder is faithful), section R (R77, R78, R80, R81, R85); the owner's ruling of 2026-09-23 that a step's state is its checkboxes; PARKED.md RESOLVED lines of 2026-09-26 (acquiring, pitch proved, speed proved, the transmit files); ARBITER.md sections 3, 4 and 6; arbiter rulings 442 DECIDED (2), 449 DECIDED (2) and (3), 460 DECIDED (3), 470 DECIDED (4) and (5); CW_REQUIREMENTS.md HM-REQ-010, 011, 012, 014, 034, 083, 093, 102; CW_SPEC.md sections 4 and 11; units 448, 449, 450, 451 and 470; R12, R61, R66, R72, R75, R76; V-04, V-08, V-11, V-13, V-14; HM-DEC-095, HM-DEC-155, HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: either the letters Hamlet guessed at while it was still finding a station are read again once it knows the station's pitch and speed, so fewer wrong letters reach the screen looking certain and none is taken back after it settled, or step 2 is closed partial as its plan says, with what is left of the confidently wrong letters counted against the one-in-a-hundred requirement and the reason written down
ADVANCES: step 2 criterion 4
END-ARBITER-DECISION
```
