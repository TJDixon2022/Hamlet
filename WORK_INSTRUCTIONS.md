# Work instruction 472 - where the invented letters sit against what was sent: step 3's first attempt at HM-REQ-011 since unit 445, with every commit green

**Loop unit.** Step 3 (`CW_REQUIREMENTS.md` HM-REQ-011, judged under R78 with HM-REQ-010, 012
and MET-WBE as guards).

**Why step 3, and why this route.**
- **The launcher names step 3. Three lines are open: 3.4, 3.5 and 3.6.**
  - **3.4 cannot be met by any unit as the tree stands.** The 7.052 traffic-net recording
    holding `EETTTEETTTTTTTTETTETETKTETEE` is not in the tree. Unit 443 searched for it and found
    only the plan's quotation. The arbiter searched again today and found no tracked file dated
    2026-09-25 or later. It is logged for the owner and not worked (DECIDED (3)).
  - **3.5 is a closing rule.** Its count is **0 of 3**: unit 445 kept a change, and no unit has
    worked step 3 since. This unit takes it to 1 of 3 if it keeps nothing, and leaves it at 0 if
    it keeps a change. It cannot flip 3.5 either way.
  - **3.6 is the line this unit can flip.** Units 443 and 445 exited red on 17:37's floor, which
    has since been re-banked (unit 460). 2.5, worded the same, was ticked by unit 460 on every
    commit from its working task on exiting green, under 442 DECIDED (2). **This unit is held to
    the same test: every commit it makes exits green on the five.**
- **The unit still does step 3's work.** HM-REQ-011 asks for MET-INVENTED at zero. It stands at
  **33 over 473** on the real set (inferred keys) and **14 over 252** on the synthetic set (exact
  keys). **11 of the synthetic 14 sit in one condition**, character gap 5 at 5 dB, where the key
  is exact by construction. No unit has traced where the invented letters sit *against the sent
  text*: inside a sent word gap, inside a sent character gap, splitting one sent character, or
  merging two. That is what "the junk between words" asks, and it is the trace this unit makes
  before it builds one change.
- **The route is new.** The loop test finds no entry for it. It is none of the refused routes
  listed in section 7.
- **R86's words bar steps 2 to 8 until 9.4 has a kept change.** 9.4 has no route left. This is
  carried to the owner as a mismatch, as units 468 to 471 did (DECIDED (1)). It is not a claim
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
- Scripts go in `.run-unit\unit472-<name>.sh` and are run with `sh`. Unit 471's
  `.run-unit\unit471-*.sh` helpers may be copied and renamed.
- Run `tools/status.sh` from the repository root only.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 3 criterion 6`, as the decision block does. Report whether it flipped. If
  any commit of this unit exited red on the five, it did not flip, and the report says which
  commit and which check.
- WHY cites the plan.

**R85. A CW question is answered from the documents, never raised to the owner.** Where a letter
"sits" against the sent text, what counts as a split or a merge, and which gap a letter fell in
are all such questions. DECIDED (4) answers the ones foreseen. If the tree forces a reading this
instruction did not foresee, record it in one line of section 4 and carry on. **Do not hand the
question back.**

---

## 2. Why this unit exists

**The count today, by the plan's checkboxes:**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | done (5 of 5; closed partial at unit 471, HM-REQ-010 not met) |
| 3 | 3 of 6 (3.4 recording absent; 3.5 count 0 of 3; 3.6 open) |
| 4 | 5 of 7 |
| 5 | 1 of 6 |
| 6 | 3 of 6 |
| 7 | 4 of 5 (7.4 open) |
| 8 | 0 of 6 |
| 9 | 7 of 8 (9.4 open, and every mechanism 9.3 named is refused) |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  Every sure letter the decoder invents - added, or printed
            in place of a different sent letter - is placed against the
            sent text: in a word gap, in a character gap, splitting one
            sent character, merging two, or standing in for one. One
            change is built against the largest group no earlier unit
            attacked and judged under R78 as step 3's change against
            HM-REQ-011. Every commit of the unit exits green on the
            five, which is what 3.6 asks.
ADVANCES:   step 3 criterion 6
```

**Read these first. They win over this instruction:**
- `CW_REQUIREMENTS.md`: HM-REQ-010, 011, 012 and 014, and their verification rows.
- `CW_SPEC.md` section 11, the metrics (how MET-INVENTED aligns and counts), section 4 (what a
  condition is), and section 10's TX-ITU timing: 1, 3, 1, 3, 7 units.
- `docs/phase-requirements/metrics.md`: the sections for units 443, 445 and 471, and
  `## Step 2 - closed partial at unit 471`.
- `.run-unit\unit471-trace.txt`, and unit 443's and 445's trace files, if the tree has them.
- The code:
  - `CwMetrics`, and how it aligns emitted against sent and decides added against substituted;
  - `CwProbabilisticDecoder`, where it splits the envelope into characters and where it assigns
    a character's class;
  - the synthetic character-gap-5 cases' builder, and how their key is made.

**The requirements, verbatim:**

> **HM-REQ-011:** read it from `CW_REQUIREMENTS.md` and quote it in the trace file's first line.
> Its threshold is MET-INVENTED at zero.
>
> **HM-REQ-010:** "On every must-tier condition at or above the sensitivity floor, the decoder
> shall keep MET-CER-SURE below 1 %."
>
> **HM-REQ-012:** "On every must-tier condition at the sensitivity floor, the decoder shall emit at
> least 90 % of sent characters as sure (MET-COVERAGE ≥ 0.90)." Rationale: "Without a floor,
> dimming everything satisfies HM-REQ-010."

**This unit succeeds when one of these is true:**
1. **Kept.** The change is committed in its own commit, under R78 as 3.2 reads it:
   - MET-INVENTED falls;
   - MET-CER-SURE does not rise;
   - MET-COVERAGE does not fall;
   - the adjudicated readings hold, or move onto their own adjudicated text;
   - V-11 holds on all 35 recordings;
   - the three floor tests, both carry-forward lines and `TheArbitrationEarnsItsPlaceTests` are
     green.

   Every metric is given per condition, real and synthetic, with the key's kind beside each
   figure. Step 3's count stays at **0**.
2. **Refused, or nothing to act on.** The change is measured and refused on a named metric, or
   task 1 finds no group outside section 7's excluded routes. The change is kept as a patch under
   `.run-unit\`, and nothing under `src` is committed. Step 3's count goes to **1 of 3**.

**Either way, 3.6 is judged on this unit's commits** (DECIDED (2)). Neither outcome is a
failure. Do not bend the keep rule to get the first.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `80df7cd5` or a runner commit on top of it. The version is 1.13.158.
- **`PHASE_PLAN.md`**, in both copies:
  - shows every line of step 2 ticked;
  - shows 3.1, 3.2 and 3.3 ticked, and 3.4, 3.5 and 3.6 open;
  - shows 9.4 as step 9's only open line;
  - carries R86 in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **The 7.052 traffic net.** Search the tree once for `GRAY KC`, `LIVER VIA` and any recording
  dated 2026-09-25. **Report what you find in one line.** If it is still absent, 3.4 is not worked.
  If it is present with a `cases-*.txt` row, print the stretch between `GRAY KC` and `LIVER VIA`
  at entry and at exit, with every character's class. Do not reconstruct it, and do not fetch.
- **Unit 471's patch** `.run-unit\unit471-reread.patch` and unit 470's `.run-unit\unit470-gate.patch`
  exist. **Do not apply either.**
- **The port** is untouched: `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints
  nothing.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files (including `watched.cpu`), `PHASE_OUTCOME.md`,
    `PHASE_STATUS.md` and `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-10-output-2.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
  `SESSION.lock` is the launcher's. Do not stage it.
- **Logged and not this unit's:**
  - the reload's `RULES_AT` disagreement (HM-DEC-165 against CPS-DEC-0183);
  - `outcome-read`'s step titles for steps 2, 3 and 8, which differ from `PHASE_PLAN.md`'s;
  - unit 471's `PARKED.md` line carrying `unit 10` and `not recorded` in its unit and launch
    fields.

**Entry figures, 471's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278, or 277 of 278 with the lost type passing alone under DECIDED (7).
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:**
  - MET-INVENTED 33 over 473 (give the added and substituted split);
  - MET-CER-SURE 33 of 436;
  - coverage 403 over 473;
  - MET-WBE 37 over 113.
- **Synthetic set, 12 cases, exact key:**
  - MET-INVENTED 14 over 252 (give the split; character gap 5 at 5 dB is expected to hold 11);
  - MET-CER-SURE 14 of 173;
  - coverage 159 over 252;
  - MET-WBE 44 over 84.
- **`TheArbitrationEarnsItsPlaceTests`:** both parts green.
- **Whole texts:** byte-identical to `.run-unit\unit471-texts-entry.txt`.

If any entry figure differs, report it and use the measured one.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31. It is on neither carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under
  DECIDED (7).
- The parity run rewriting the decode-time rows of `parity.md`. Keep a copy under `.run-unit\` and
  restore the committed file, as units 462 to 471 did.
- **The change refused under R78, or the trace finding nothing outside the excluded routes.**
  Both are outcomes this instruction foresees, and neither is a regression.

---

## 4. Rulings in force - transcribed, do not re-argue

**The step and its keep rule:**
- **Step 3, 3.2:** *"Each change is built in its own commit and kept under R78: MET-INVENTED
  falls, MET-CER-SURE does not rise, MET-COVERAGE does not fall, the adjudicated readings hold,
  and V-11 holds."*
- **Step 3, 3.3:** *"MET-INVENTED is reported before and after every kept change, per condition,
  with the key kind beside each number, and the running figure is in `metrics.md`."*
- **Step 3, 3.5:** *"After three consecutive units with no kept change the trace goes to
  `PARKED.md` and the step closes partial."*
- **Step 3, 3.6:** *"The three floor tests and both carry-forward lines are green at the exit of
  every commit of the step."*
- **R78:** *"A change is kept when it moves a requirement's metric the right way and breaks no
  other requirement. MET-INVENTED at zero, MET-CER-SURE below 1%, MET-COVERAGE at or above 90%,
  MET-WBE at or below 5%, per condition, at the tier the requirement names. The capture floors stay
  as V-11's overfitting guard ... and stop being the keep rule. A capture row's character count
  falling is a finding to report, not a rejection, when no requirement's metric got worse."*
- **R81:** step 3 is MET-INVENTED.
- **442 DECIDED (2):** the floors-green line is judged green at every commit of the unit from its
  working task on. This unit commits no test that was watched red and no refused change under
  `src`. **Unit 460 ticked 2.5 on this reading.**
- **449 DECIDED (2):** *"the unit counts as one with no kept change if its change is refused
  under R78 or if the trace finds no group outside the excluded routes."*
- **The owner's ruling of 2026-09-23:** a step's state is its checkboxes. Step 2 is closed. Do
  not author into it, and do not tick or untick any of its lines.

**The ordering, and the mismatch this unit carries:**
- **R86** (`PHASE_PLAN.md` section 6): *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."* This
  unit is authored against it under DECIDED (1). That is reported to the owner as a mismatch. It
  is not a claim that R86 is overruled.
- **`ARBITER.md` section 4:** *"If you judge it a loop, the step ends. Say so, name the approaches
  that were tried, and move to another step or declare it unachievable."* 9.4 is so judged.

**Standing rulings this unit leans on:**
- **R12.** A session rewrites its own tests.
- **R61.** A key is inferred unless it was transcribed.
- **R66.** An adjudicated reading may move onto its own adjudicated text.
- **R72.** No word, dictionary or callsign prior, in any form. **The change uses only the audio
  and what the decoder already measures from it. The key is read by the trace and the scorer
  only, never by the decoder.**
- **R77.** The test names HM-REQ-011.
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
- **The stream's own rule: nothing already settled is retracted.**
- HM-DEC-095, HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task;
- when the 7.052 traffic-net search is done, saying found or absent;
- when the trace's table is in, naming the largest group and its size on each set;
- when the rule is registered, with the commit hash;
- when the test comes back red, naming the case;
- at the verdict, naming the deciding metric, or saying there was nothing to act on;
- at each commit, with its hash and the five results;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 472 - STEP 3` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 472 with `CURRENT_STEP: 3`, in both copies.
3. Bump the patch by one from what the tree carries. It should be 1.13.158, going to 1.13.159.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `TheArbitrationEarnsItsPlaceTests`, both parts.

   Save every recording's text, with every character's class, to
   `.run-unit\unit472-texts-entry.txt`. Diff it against `.run-unit\unit471-texts-entry.txt`.
6. **Task 0's commit must exit green on the five.** It is the first commit 3.6 is judged on. If
   the entry round is red on any of the five, say which, do not tick 3.6, and carry on with
   task 1.

### Task 1 - the trace: where each invented letter sits against what was sent

**Nothing under `src` changes in this task.**

1. **Read the scorer first.** In `.run-unit\unit472-trace.txt`, quote HM-REQ-011 and write how
   `CwMetrics` aligns emitted against sent and decides added against substituted. The trace
   uses **the same alignment**. It does not invent a second one.
2. **Write `WhereTheInventedLettersSitFact`**, naming HM-REQ-011. It asserts nothing. It drives
   through the same harness and the same `Measure` and `CwMetrics` calls as
   `TheRequirementsAreMeasuredTests`:
   - the 23 real keyed recordings;
   - the 12 synthetic cases.

   For **every sure letter MET-INVENTED counts**, added or substituted, print:
   - the recording, the condition, the key's kind, and the span;
   - what the key says was sent there, and what was emitted, with its class;
   - **where it sits against the sent text, under DECIDED (4)**: in a sent word gap; in a sent
     character gap; one of two or more letters emitted over one sent character (a split); one
     letter emitted over two or more sent characters (a merge); or in place of one sent
     character (a stand-in);
   - its marks and gaps in units at the unit in force, and the gap before and after it;
   - the envelope's level on its marks against the level in the gaps either side of it;
   - the speed and pitch in force, with their proof states;
   - whether it was emitted while acquiring, in unit 470's sense.
3. **Table them**, per condition and with the key's kind: the five positions down the side,
   real and synthetic across. Beside each position, name **whether an earlier unit's route
   already acted on it**: 443's double read, 445's inner-gap edge, 470's acquisition gate, 471's
   re-read, 442's rival margin, and fldigi's (A) to (E) and (W).
4. **For the largest group no earlier route acted on**, print the same fields for **every sure
   right letter** in the same position class. A split's right neighbours, for example, are sure
   right letters followed by a character gap of the same length. **This is what the change would
   put at risk.** Print it before building anything.
5. **Register the rule in the trace file before any after-figure exists.** Write, in one
   paragraph, what the change does, on which measured quantity, at what edge, and why the edge
   comes from `CW_SPEC.md`'s timing or from a line no right letter crosses on both sets. Name the
   file and method it goes into. Commit the fact and the trace. **This commit must exit green on
   the five.**
6. **If no group lies outside the excluded routes, or no edge exists that the right letters in
   step 4 do not cross**, say so in the trace, skip task 2, and go to task 3 as a unit with no kept
   change (449 DECIDED (2)).

### Task 2 - one change, judged under R78

**Build it in the working tree. Commit it only if kept.**

1. **The test first.** Write `TheInventedLettersAreNotPrintedSureTests`, naming HM-REQ-011. Take
   a generated case whose key is exact by construction, where the trace shows a sure invented
   letter of the chosen group. Assert that no sure letter is invented on that span. **Watch it red
   at HEAD** and save the output to `.run-unit\unit472-red.txt`. If no generated case shows the
   group, say so, and judge the change on R78 alone.
2. **Build the change under the registered rule**, in our decoder only, one form. Then run the
   test green.
3. **Judge it under R78**, per condition, real and synthetic, with the key's kind beside each
   figure. Print the table before and after for:
   - MET-INVENTED, with the added and substituted split;
   - MET-CER-SURE;
   - sure-and-right coverage;
   - MET-WBE;
   - dim precision (HM-REQ-014);
   - the adjudicated readings;
   - V-11 on each of the 35 recordings;
   - the three floor tests;
   - both carry-forward lines;
   - `TheArbitrationEarnsItsPlaceTests`.

   **Beside the table, print:**
   - **every text the change moved**, before and after, with each character's class;
   - **the 7.052 traffic net's stretch**, before and after, only if the tree now holds it;
   - **decode time**, before and after, on the real set.
4. **The verdict:**
   - **Kept:** every line of 3.2 holds. Commit the change and its test together as one commit,
     which must exit green on the five. Write the before and after into `metrics.md` as a new
     `## Unit 472` section, in the form unit 445 used, with the running MET-INVENTED figure
     (3.3). Step 3's count stays 0.
   - **Refused:** name the metric and the condition that refused it. Save the change and the
     test as `.run-unit\unit472-change.patch`, and restore `src` and `tests` to HEAD. Write the
     refusal and its table into `metrics.md` under `## Unit 472`, in the form unit 471 used.
     Commit only the documents and the `.run-unit` files. Step 3's count goes to 1 of 3.

   **Do not try a second form.** If the trace suggests one, it goes in one line of section 4.

**Drop candidate:** the decode-time line, then the level columns of task 1's table, then the
synthetic half of task 1's step 4. These are never shed: the five-position table on both sets,
the right letters at risk for the chosen group, the rule registered before the build, the test
watched red where a case exists, the full R78 table, and the five green at every commit.

### Task 3 - the step's state, written down

1. Write into `metrics.md`, under `## Unit 472`, one line: step 3's count of consecutive units
   with no kept change, **0 or 1 of 3**, and the units it counts.
2. **Do not tick 3.5.** It cannot flip in this unit. **Do not append to `PARKED.md`.**
3. If task 2 did not run, commit this line with task 1's trace in the documents-only form.
   Otherwise fold it into task 2's commit.

### Task 4 - the exit round, and 3.6

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `TheArbitrationEarnsItsPlaceTests`, both parts;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff 80df7cd5 -- src`. If the change was kept, this is only the change. If not, it prints
  nothing;
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- **a table of every commit this unit made, with its results on the five at its exit**: build,
  both carry-forward lines, and the three floor tests. A commit that changes nothing under `src`
  or `tests` takes the figures of the last commit that did, and says so.

**Ticks:**
- **3.6**, in both copies of `PHASE_PLAN.md`, **only if every commit in that table exits green on
  all five**, with the dispatcher loss handled under DECIDED (7). Change `- [ ] 3.6` to
  `- [x] 3.6` and nothing else. Commit it with the report.
- **3.4 only if** the tree held the traffic net, a change was kept, and its stretch is printed
  before and after.
- **Nothing else.** Do not tick 3.5, 4.4, 6.4, 9.4, or any line of steps 4 to 9.

---

## 7. Parked - do not touch, do not raise

- **9.4 and every mechanism 9.3 named, (A) to (E) and (W), in any form.** The loop is ended.
- **Every route already refused or used against these letters:**
  - 443's double read: a letter starting inside one already said is not announced;
  - 445's inner-gap edge at 6.5 units: already kept, and not moved;
  - 444's dropouts left out of the gap clustering;
  - 442's rival margin;
  - 441's marks' speed, and 446's speed bounds;
  - 448's from-cold move;
  - **470's acquisition gate, in any form**, and **471's re-read** of the acquiring stretch.
    Their patches are not applied.
- **Step 2.** It is closed. Its lines are not ticked, unticked or reworked.
- **`CwToneTracker`, `CwToneSurvey`, the competing-station logic, and how either proof state is
  computed.**
- **Every switch in `CwSwitchTable`, the vote table, both confidences, calibration and
  HM-REQ-127's margin.**
- **TX-STRAIGHT and TX-SLOPPY; HM-REQ-008, 051, 054, 063, 064 and 065.**
- **5.1, 6.1, 6.4 and 7.4.**
- **The two `PHASE_STATUS.md` copies differing beyond the unit's fields.**

## 8. Do not

- **Do not dim every letter of a class to catch a few wrong ones.** Coverage falling refuses the
  change, and 470 measured what that costs.
- **Do not fit an edge to the trace that a sure right letter crosses** (V-14). An edge is from
  `CW_SPEC.md`'s timing or is one no right letter crosses on both sets.
- **Do not loosen a separation limit, confirmation rule or plausibility bound** (V-14).
- **Do not retract, rewrite or re-class any character after it has settled.**
- **Do not change the keep rule, the scorer's alignment, or any key.**
- **Do not build a change that only the scored path sees.** It must act the same on the path the
  CW tab reads.
- **Do not commit a watched-red test or a refused change under `src` or `tests`** (442 DECIDED
  (2)).
- **Do not read any key or text into the decoder** (R72).
- **Do not reconstruct or fetch the 7.052 traffic-net recording.**
- **Do not touch, call or reuse `KeyerCwSender.cs`** or any transmit file (§0.2).
- Do not add a new test to either carry-forward line.
- Do not re-point or retire an existing test (R80).
- Do not edit `CW_SPEC.md`, `CW_REQUIREMENTS.md`, `PARKED.md`, or any ruling in `PHASE_PLAN.md` or
  `CLAUDE.md`.
- **Do not install any package. That is `MOVE: stop`.**
- Do not stage, commit or modify `.run-unit\fldigi\`, and do not fetch.
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose a
  timestamp.**

## 9. Committing and pushing

**One commit per step:**
- task 0;
- task 1's fact and trace, with the rule registered;
- task 2: either the kept change with its test and `metrics.md`, or, if refused, the documents
  and the `.run-unit` files only, with task 3's line;
- task 4: the exit printouts, `output.md`, and the 3.6 tick if earned.

**Messages** take the form `unit472 task N: <what> (3.6)`. Task 2's message names the verdict and
the deciding metric, with MET-INVENTED before and after, real and synthetic.

**Exit state:** every commit exits with these green:
- the build;
- both carry-forward lines;
- the three floor tests.

The one exception is the dispatcher loss under DECIDED (7). **This is not housekeeping here. It
is the criterion this unit names.**

**Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes first.**
`validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 done (closed partial), 3 <3|4|5> of 6, 4 5 of 7,
   5 1 of 6, 6 3 of 6, 7 4 of 5, 8 0 of 6, 9 7 of 8; 9.4 ended as a
   loop, and this unit worked step 3 against R86's words, reported as
   a mismatch.
B. Step 3 (HM-REQ-011): the invented letters placed against the sent
   text - largest unattacked group <position, n real, n synthetic>;
   the change <kept | refused on <metric, condition> | nothing to act
   on>; real MET-INVENTED <before> -> <after>, synthetic <before> ->
   <after>; 3.6 <ticked, every commit green on the five | not ticked,
   <commit> red on <check>>; 3.5's count <0|1> of 3; 3.4 <recording
   absent | printed>.
C. The findings weighed against A and B: how many items section 4
   raises, whether any is in the way of 3.4, 3.5 or 3.6, and the R86
   mismatch stated in one line.
```

```
UNIT:       472 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes - step 3 criterion 6, ticked | no - step 3 criterion 6, <commit> red on <check>>
NUMBER:     MET-INVENTED real <x/y> -> <x/y>, synthetic <x/y> -> <x/y>; coverage real <x> -> <x>; change <kept|refused|nothing to act on>
DRIFT:      step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the commit table, with the five results at each commit**, because that is
what 3.6 is judged on. Then give:
1. the R78 table, per condition, before and after, with the key's kind, or, if there was nothing
   to act on, task 1's position table instead;
2. task 1's position table on both sets, with the earlier routes beside each position;
3. the right letters at risk for the chosen group;
4. the rule as registered, with its commit hash;
5. every recording whose text or classes changed, before and after;
6. the 7.052 traffic-net search result, and the stretch before and after if it was in the tree.

**Section 2, one paragraph, in the owner's words.**
- **If kept:** say which kind of junk letter no longer reaches the screen looking certain, and
  quote a stretch from before and after.
- **If not kept:** say that nothing on the screen changes, where the invented letters were found
  to sit, and why this unit's one change did not hold.

**Section 4 must say, as a plain reading and not a ruling request:**
- **The R86 mismatch**, in one line, as units 468 to 471 put it.
- **3.4**: whether the traffic-net recording is in the tree. If it is not, say that 3.4 cannot be
  met by any unit until the recording and its `cases-*.txt` row are added.
- **DECIDED (4)**, the reading of where a letter sits, as the arbiter's and overrulable.
- **3.5's count after this unit.**

---

```
ARBITER-DECISION
STEP: 3
APPROACH: place every sure invented letter, added or substituted, against the sent text through CwMetrics' own alignment - in a word gap, in a character gap, a split of one sent character, a merge of two, or a stand-in - on the real and exact-key synthetic sets, print the sure right letters in the same position class, build one change against the largest group no earlier route acted on at an edge from CW_SPEC timing or one no right letter crosses, judged under R78 against HM-REQ-011, every commit green on the five for 3.6
MOVE: work around
WHY: PHASE_PLAN.md step 3's open lines are 3.4, whose 7.052 recording is absent from the tree, 3.5, a closing rule at 0 of 3, and 3.6, which a unit meets as unit 460 met 2.5 by every commit exiting green; so the unit names 3.6 and spends its work on HM-REQ-011's MET-INVENTED, 33 over 473 real and 14 over 252 synthetic, with a trace no unit has made - where each invented letter sits against what was sent - and the loop test finds no entry for it.
STATE: partial
DECIDED: author's, overrulable - (1) step 3 is worked as the launcher named it although R86 (PHASE_PLAN.md section 6) reads that steps 2 to 8 are not authorable until 9.4 has a kept change; this is carried to the owner as a mismatch, as units 468 to 471 did, and not a claim that R86 is overruled, 9.4 staying judged a loop under ARBITER.md section 4 with (A) to (E) and (W) refused; (2) 3.6 is judged on this unit's commits from task 0 on, every one green on the five, as 442 DECIDED (2) judged 2.5 and unit 460 ticked it, and 443's and 445's red exits on 17:37's since re-banked floor are not held against it; (3) 3.4 is not worked because the 7.052 traffic-net recording is not in the tree (unit 443's search, the arbiter's today), it is searched once and printed only if present, and never reconstructed or fetched; (4) a letter's position is read through CwMetrics' own alignment: added in a sent word gap or in a sent character gap by where its span falls against the key's timing, a split where two or more emitted letters cover one sent character, a merge where one emitted letter covers two or more, and a stand-in where one emitted letter replaces one sent character; (5) the keep rule is 3.2 as written, per condition, real and synthetic, with TheArbitrationEarnsItsPlaceTests green as a further gate, one form only, and the edge from CW_SPEC timing or one no sure right letter crosses on both sets (V-14); (6) the unit counts as one with no kept change if its change is refused under R78 or the trace finds no group outside the excluded routes (449 DECIDED (2)), taking 3.5's count to 1 of 3, and 3.5 is not ticked and PARKED.md is not written; (7) the app line's headless dispatcher-loop loss gets one rerun, and any type lost again is run alone and must pass, named in the report; (8) 471's section 4 is logged, not chased: step 2 is closed by its checkboxes and not reopened, its PARKED.md line's fields are logged, and its V-14 note on a longer window is not tried
LICENCE: PHASE_PLAN.md step 3 lines 3.2, 3.3, 3.4, 3.5 and 3.6 and its dependency line, section 5, section 6 (R86 as reported, V-14, the documents win, the second decoder is faithful), section R (R77, R78, R80, R81, R85); the owner's ruling of 2026-09-23 that a step's state is its checkboxes; ARBITER.md sections 3, 4 and 6; arbiter rulings 442 DECIDED (2), 449 DECIDED (2); units 443, 445, 460, 470 and 471; CW_REQUIREMENTS.md HM-REQ-010, 011, 012, 014; CW_SPEC.md sections 4, 10 and 11; R12, R61, R66, R72; V-04, V-08, V-11, V-13, V-14; HM-DEC-095, HM-DEC-155, HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the junk letters Hamlet prints with confidence are each placed against what was actually sent - in the silence between words, between letters, or by cutting or joining real letters - and one change removes the largest kind no earlier attempt touched, or is refused with the reason written down; either way every commit leaves the floor tests green, which closes step 3's green-at-every-commit line
ADVANCES: step 3 criterion 6
END-ARBITER-DECISION
```
