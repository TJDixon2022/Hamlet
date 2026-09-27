# Work instruction 470 - no letter is printed sure while the decoder is still acquiring: HM-REQ-102's gate, on the resolved meaning of acquiring, judged under R78 as step 2's next change against HM-REQ-010

**Loop unit.** Step 2 (`CW_REQUIREMENTS.md` HM-REQ-010, judged under R78 with HM-REQ-011, 012
and MET-WBE as guards; HM-REQ-102 measured beside it).

**Why step 2, and why this route.**
- **The launcher names step 2, and its only open line is 2.4.** 2.4 closes the step partial after
  three consecutive units with no kept change. The count is **1 of 3**: unit 449 kept a change,
  and unit 460 kept none (unit 467's arbiter's reading, carried by 468 and 469, not re-argued).
  Two arbiters in a row stepped around step 2 because one unit cannot tick 2.4. The step
  never closes that way. This unit is the next real attempt at HM-REQ-010. Either it keeps a change
  that lowers MET-CER-SURE, which is what step 2 is for, or it takes the count to 2 of 3.
- **The largest group of what is left has been parked, and the park has since been answered.**
  Unit 449 left 33 sure-but-wrong letters on the real set. **13 of them come before the tracker's
  first keyed verdict**, which is acquisition. 449's DECIDED (4) kept that group out of reach
  "*which is parked with the owner*". The owner-side commit `9a329cdb` then put this line in
  `PARKED.md`:
  > *RESOLVED: 2026-09-26 | define acquiring for HM-REQ-102 | acquiring runs from the first keyed
  > element at a tracked pitch until both the pitch (HM-REQ-093) and the speed (HM-REQ-034) carry
  > proof state proved; while either is hypothesis or none, no character is emitted sure.*

  Units 450 and 451 built both proof states. So the gate HM-REQ-102 requires can now be built from
  parts the tree already has. **No unit has tried it.** The loop test finds no entry for the
  approach. It is none of step 2's excluded routes: rival margin, marks' speed, gap duration,
  mark-shape edge, from-cold move, speed bounds, and the fldigi mechanisms (A) to (E) and (W).
- **R86's words bar steps 2 to 8 until 9.4 has a kept change.** 9.4 has no route left. This is
  carried to the owner as a mismatch, as units 468 and 469 did (DECIDED (1)). It is not a claim
  that R86 is overruled.

Three tasks after task 0. Drop from the back.

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
- Scripts go in `.run-unit\unit470-<name>.sh` and are run with `sh`. Unit 469's
  `.run-unit\unit469-*.sh` helpers may be copied and renamed.
- Run `tools/status.sh` from the repository root only.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: none - clears a blocker: step 2 criterion 4`, as the decision block does. 2.4
  cannot flip in this unit, and the report does not claim it did.
- WHY cites the plan.

**R85. A CW question is answered from the documents, never raised to the owner.** What counts as
acquiring, which proof state the gate reads and what class a gated letter takes are all such
questions. DECIDED (2) to (4) answer the ones foreseen. If the tree forces a reading this
instruction did not foresee, record it in one line of section 4 and carry on. **Do not hand the
question back.**

---

## 2. Why this unit exists

**The count today, by the plan's checkboxes:**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | 4 of 5 (2.4 open, its count 1 of 3) |
| 3 | 3 of 6 |
| 4 | 5 of 7 |
| 5 | 1 of 6 |
| 6 | 3 of 6 |
| 7 | 4 of 5 (7.4 open) |
| 8 | 0 of 6 |
| 9 | 7 of 8 (9.4 open, and every mechanism 9.3 named is refused) |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The decoder prints no letter as sure while it is acquiring -
            from the first keyed element at a tracked pitch until both
            the pitch and the speed carry proof state proved - as
            HM-REQ-102 and the 2026-09-26 resolution of acquiring
            require. The change is judged under R78 as a step-2 change
            against HM-REQ-010. It is committed if kept, and held as a
            patch if refused.
ADVANCES:   none - clears a blocker: step 2 criterion 4
```

**Read these first. They win over this instruction:**
- `CW_REQUIREMENTS.md`:
  - HM-REQ-010, 011, 012 and 014;
  - HM-REQ-034 and 093, the proof states;
  - HM-REQ-100, 101 and 102, and their verification rows.
- `CW_SPEC.md` section 11, the metrics, and section 4, what a condition is.
- `PARKED.md`, the four `RESOLVED:` lines of 2026-09-26: acquiring, a heard pitch proved, a
  speed proved, and the transmit files.
- `docs/phase-requirements/metrics.md`:
  - unit 449's section, which holds the 33 that remain and the acquisition group;
  - unit 448's section, which is the 7.052 opening acquiring trace (`3c4e1f75`: acquires 0 to
    26.04 s with 9 sure letters).
- The code:
  - units 450 and 451's proof states, `CwPitchProof` and `CwSpeedProof`, and where each is
    computed;
  - `CwDecodeReport`;
  - where `CwProbabilisticDecoder` assigns a character's class.

**The requirements, verbatim:**

> **HM-REQ-102:** "While acquiring, the decoder shall emit no sure character." Rationale:
> "Candidates naming 325, 550, 725 Hz for a 640 Hz signal is a failure."
>
> **HM-REQ-010:** "On every must-tier condition at or above the sensitivity floor, the decoder
> shall keep MET-CER-SURE below 1 %."
>
> **HM-REQ-012:** "On every must-tier condition at the sensitivity floor, the decoder shall emit at
> least 90 % of sent characters as sure (MET-COVERAGE ≥ 0.90)." Rationale: "Without a floor,
> dimming everything satisfies HM-REQ-010."
>
> **HM-REQ-100:** "When a transmission begins with a run-up of at least five characters, the
> decoder shall emit its first sure character within 2.0 s of the first element at 20 WPM, scaled
> by dit length at other speeds (MET-TACQ)."

**This unit succeeds when either of these is true:**
1. **Kept.** The gate is committed in its own commit under R78 as 2.2 reads it:
   - MET-CER-SURE falls;
   - MET-INVENTED does not rise;
   - MET-COVERAGE does not fall;
   - the adjudicated readings hold or move onto their own adjudicated text;
   - V-11 holds on all 35 recordings;
   - the three floor tests, both carry-forward lines and `TheArbitrationEarnsItsPlaceTests` are
     green.

   Every metric is per condition, real and synthetic, with the key's kind beside each figure.
   Step 2's count goes to **0**.
2. **Refused.** The gate is measured, refused on the named metric, and kept as a patch under
   `.run-unit\`. Nothing under `src` is committed. Step 2's count goes to **2 of 3**.

**Either way:**
- HM-REQ-102 is measured on the 7.052 opening before and after, as text.
- The time of each recording's first sure character is printed before and after.

**Refused is a finding, not a failure of this unit.** HM-REQ-012's rationale says plainly that
dimming costs coverage. The unit measures that cost and does not bend the rule to avoid it.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `1f6f20e8` or a runner commit on top of it. The version is 1.13.156.
- **`PHASE_PLAN.md`**, in both copies:
  - shows 2.1, 2.2, 2.3 and 2.5 ticked and 2.4 open;
  - shows 7.1, 7.2, 7.3 and 7.5 ticked;
  - shows 9.4 as step 9's only open line;
  - carries R86 in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **`PARKED.md` carries the `RESOLVED:` line defining acquiring.** Quote it in the report.
- **Both proof states exist in `src`** and are reported per decode: `CwPitchProof` (unit 450) and
  `CwSpeedProof` (unit 451). If either is missing, or is computed in a way that differs from its
  `RESOLVED:` line, report it. **Do not change it.** The gate reads them as they stand.
- **The port** is untouched: `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints
  nothing.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-8-output-3.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
  `SESSION.lock` is the launcher's. Do not stage it.
- **Logged and not this unit's:**
  - the reload's `RULES_AT` disagreement (HM-DEC-165 against CPS-DEC-0183);
  - `outcome-read`'s step titles for steps 2, 3 and 8, which differ from `PHASE_PLAN.md`'s.

**Entry figures, 469's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278, or 277 of 278 with the lost type passing alone under DECIDED (7).
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:** MET-CER-SURE 33 of 436, coverage 403 over 473,
  MET-WBE 37 over 113.
- **Synthetic set, 12 cases, exact key:** MET-CER-SURE 14 of 173, coverage 159 over 252, MET-WBE
  44 over 84.
- **`TheChannelProfilesAreWhatTheySayTests`:** 71 of 71.
- **`TheSenderProfilesAreWhatTheySayTests`:** 16 of 16.
- **`TheInterferenceProfilesAreWhatTheySayTests`:** 21 of 21.
- **`TheArbitrationEarnsItsPlaceTests`:** both parts green.

If any entry figure differs, report it and use the measured one.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31. It is on neither carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under
  DECIDED (7).
- The parity run rewriting the decode-time rows of `parity.md`. Keep a copy under `.run-unit\` and
  restore the committed file, as units 462 to 469 did.
- **The gate refused under R78.** That is an outcome this instruction foresees, not a regression.

---

## 4. Rulings in force - transcribed, do not re-argue

**The step and its keep rule:**
- **Step 2, 2.2:** *"Each change is built in its own commit and kept under R78: MET-CER-SURE
  falls, MET-INVENTED does not rise, MET-COVERAGE does not fall, the three adjudicated readings are
  unchanged or move onto their own adjudicated text, and V-11 holds - no capture is reddened to
  green a newer one."*
- **Step 2, 2.4:** *"After three consecutive units with no kept change the trace goes to
  `PARKED.md` and the step closes partial."*
- **R78:** *"A change is kept when it moves a requirement's metric the right way and breaks no
  other requirement. MET-INVENTED at zero, MET-CER-SURE below 1%, MET-COVERAGE at or above 90%,
  MET-WBE at or below 5%, per condition, at the tier the requirement names. The capture floors stay
  as V-11's overfitting guard ... and stop being the keep rule. A capture row's character count
  falling is a finding to report, not a rejection, when no requirement's metric got worse."*
- **R81:** step 2 is MET-CER-SURE.
- **442 DECIDED (2):** 2.5 is judged green at every commit. So this unit commits no test that was
  watched red and no refused change under `src`, as unit 460 did under its DECIDED (3).

**The ordering, and the mismatch this unit carries:**
- **R86** (`PHASE_PLAN.md` section 6): *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."* This
  unit is authored against it under DECIDED (1). That is reported to the owner as a mismatch. It
  is not a claim that R86 is overruled.
- **`ARBITER.md` section 4:** *"If you judge it a loop, the step ends. Say so, name the approaches
  that were tried, and move to another step or declare it unachievable."* 9.4 is so judged.
- **`PHASE_PLAN.md` section 5:** *"Steps 2, 3, 4, 5, 6 and 7 each depend only on those two, so
  there are six independent places to route and the loop need never stall for want of work."*

**The answers this unit builds on (`PARKED.md`, `RESOLVED:`, 2026-09-26, author's under R85,
overrulable):**
- **Acquiring:** *"acquiring runs from the first keyed element at a tracked pitch until both the
  pitch (HM-REQ-093) and the speed (HM-REQ-034) carry proof state proved; while either is
  hypothesis or none, no character is emitted sure. Consistent with HM-REQ-100, which requires
  proof reachable within a five-character run-up."*
- **A heard pitch proved:** *"pitch is proved when the independent survey admits keying at that
  pitch and at least one whole character has resolved at it; hypothesis when a tone is tracked but
  keying is not admitted or no character has resolved; none when no tone is tracked."*
- **A speed proved:** *"a speed is proved only while characters resolve at it with the marks' unit
  within 10 percent of the path's (HM-REQ-031); when the tone stops or the pitch changes it drops
  to hypothesis."*

**Standing rulings this unit leans on:**
- **R12.** A session rewrites its own tests.
- **R61.** A key is inferred unless it was transcribed.
- **R66.** An adjudicated reading may move onto its own adjudicated text.
- **R72.** No word, dictionary or callsign prior, in any form.
- **R75 and R76.** The tone tracker is open to change, but not in this unit.
- **R77.** The gate's test names HM-REQ-102 and HM-REQ-010.
- **R80.** No traceability, test inventory or decision-log work.
- **V-04, V-08, V-11, V-13, V-14.**
- **HM-REQ-122 and 129:** the second decoder is faithful, not improved. Not one line under
  `Cw\Second\` changes.
- **Section 6:** *"Where this plan and the two documents differ, the documents win, and the
  difference is a finding in the report."*

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode. **This gate is that rule applied: a letter
  read before the pitch and speed are proved is a guess, and it is shown dim.**
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched.**
- `CLAUDE.md` §12.5: a fixture built from the same misunderstanding as the code proves nothing.
  The test's red case is chosen from a generated fixture whose key is exact by construction.
- HM-DEC-095, HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task;
- when the trace's table is in, naming the wrong-sure and right-sure letters inside acquiring;
- when the rule is registered, with the commit hash;
- when the test comes back red, naming the case;
- at the keep-or-refuse verdict, naming the deciding metric;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 470 - STEP 2` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 470 with `CURRENT_STEP: 2`, in both copies.
3. Bump the patch by one from what the tree carries. It should be 1.13.156, going to 1.13.157.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `TheArbitrationEarnsItsPlaceTests`, both parts;
   - `TheChannelProfilesAreWhatTheySayTests`, `TheSenderProfilesAreWhatTheySayTests` and
     `TheInterferenceProfilesAreWhatTheySayTests`.

   Save every recording's text, with every character's class, to
   `.run-unit\unit470-texts-entry.txt`.

### Task 1 - the trace: what each sure letter was printed under

**Nothing under `src` changes in this task.**

1. **Read the code first.** Before any decode, write into `.run-unit\unit470-trace.txt`:
   - where `CwProbabilisticDecoder` assigns a character's class;
   - where `CwPitchProof` and `CwSpeedProof` are computed, and whether both are known at the
     moment a character is emitted. If a proof state is only computed after the fact, for the
     report, say so. That decides how the gate can be built;
   - whether either proof state departs from its `RESOLVED:` line. Report the departure. Do not
     repair it;
   - how the arbiter (`CwArbiter`, `CwSwitchTable`) takes our class, so the effect on the emitted
     transcript is known before any build.
2. **Write `WhatTheSureLettersWerePrintedUnderFact`**, naming HM-REQ-102 and HM-REQ-010. It
   asserts nothing. It drives through the same harness and the same `Measure` and `CwMetrics`
   calls as `TheRequirementsAreMeasuredTests`:
   - the 23 real keyed recordings;
   - the 12 synthetic cases;
   - **drop candidate, first to go:** the TX-* cases of `TheMustFistsAtFifteenDecibelsFact` and
     the INT-* cases of `TheInterferenceIsMeasuredFact`.

   For every sure character, it prints:
   - the recording and the span;
   - what the key says was sent, and what was emitted;
   - right, wrong or added;
   - the pitch proof state and the speed proof state at its emission.

   It then tables, per condition and with the key's kind, the sure letters **inside acquiring**
   (either state is not proved) and **outside it**, each split into right and wrong-or-added. The
   inside-acquiring right count is the coverage this gate will cost. The inside-acquiring
   wrong-or-added count is the MET-CER-SURE it will save. **Print both before building anything.**
3. **Place the 13 in the table.** Match unit 449's acquisition 13 by recording and span against
   the inside-acquiring column, and say how many fall inside it.
4. **Register the rule in the trace file before any after-figure exists.** Write DECIDED (3) as
   the tree will implement it. Commit the fact and the trace. The fact asserts nothing, so the five
   stay green.

### Task 2 - the gate, judged under R78

**Build it in the working tree. Commit it only if kept.**

1. **The test first.** Write `NoSureLetterWhileAcquiringTests`, naming HM-REQ-102 and HM-REQ-010.
   On a generated case whose key is exact by construction, it asserts that no character emitted
   while either proof state is not proved has the sure class. Use a case where the trace shows
   HEAD printing a sure letter inside acquiring. Unit 469's INT-CARRIER(+100 Hz, 0 dB), which
   prints `CT  EIQ` before `CQ`, is the likely one, and a cold TX-ITU start is the other. **Watch it
   red at HEAD** and save the output to `.run-unit\unit470-red.txt`.
2. **Build the gate under DECIDED (3)** in our decoder only. It sits where the class is assigned,
   or where the trace in task 1 says the proof states are first known. **No constant is added,
   and no proof-state logic is changed.** Run the test green.
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

   **Beside the table, print these two:**
   - **HM-REQ-102 on the 7.052 opening:** the opening's text with every character's class, before
     and after, and the sure letters inside acquiring before and after. Unit 448 measured 9 sure
     letters over 0 to 26.04 s.
   - **The first sure character's time, per recording, before and after.** This stands in for
     HM-REQ-100's MET-TACQ, which does not exist yet (5.4). It is reported and is not a keep gate.
4. **The verdict:**
   - **Kept:** every line of 2.2 holds. Commit the gate and its test together as one commit. Write
     the before-and-after into `metrics.md` as a new `## Unit 470` section, in the form unit 449
     used, with the texts that changed. Step 2's count goes to 0.
   - **Refused:** name the metric and the condition that refused it. Save the change and the test
     as `.run-unit\unit470-gate.patch`, and restore `src` and `tests` to HEAD. Write the refusal and
     its table into `metrics.md` under `## Unit 470`, in the form unit 446 used. Commit only the
     documents and `.run-unit` files. Step 2's count goes to 2 of 3.

   **Do not try a second form of the gate.** A narrower form, one proof state only or a grace
   period, would be a threshold fitted to the trace (V-14). If the trace suggests one, it is a
   line in section 4.

**Drop candidate:** the TX-* and INT-* columns of task 1, then the first-sure-time printout.
These are never shed: task 1's inside-and-outside table on the real and synthetic sets, the rule
registered before the build, the test watched red, the full R78 table, and the 7.052 opening's
text before and after.

### Task 3 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `TheArbitrationEarnsItsPlaceTests`, both parts;
- `TheChannelProfilesAreWhatTheySayTests`, `TheSenderProfilesAreWhatTheySayTests` and
  `TheInterferenceProfilesAreWhatTheySayTests`;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff 1f6f20e8 -- src`. If the gate was kept, this is only the gate. If refused, it prints
  nothing;
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with its results on the five at its exit: build, both
  carry-forward lines, and the three floor tests.

**Ticks: none.** 2.4 cannot flip here. **Do not tick 4.4 or any line of steps 4 or 5** on the
HM-REQ-102 measurement. 4.5 is already met. Do not tick 9.4: this gate is not a technique from
the second decoder. **Do not write to `PARKED.md`.** 2.4's parking waits for the third unit.

---

## 7. Parked - do not touch, do not raise

- **9.4 and every mechanism 9.3 named, (A) to (E) and (W), in any form.** The loop is ended.
- **Every excluded step-2 route:** rival margin, marks' speed, gap duration, mark-shape edge,
  from-cold move and speed bounds.
- **`CwToneTracker`, `CwToneSurvey`, the competing-station logic, and how either proof state is
  computed.** The gate reads the proof states. It does not change them.
- **Every switch in `CwSwitchTable`, the vote table, both confidences, calibration and
  HM-REQ-127's margin.**
- **Unit 469's section 4.** Items 3 and 4 shape task 2's red case and nothing more. Items 1, 2,
  5 and 6 are logged. The 25 WPM extra `K` and HM-REQ-066's lenient reading are not chased.
- **TX-STRAIGHT and TX-SLOPPY; HM-REQ-008, 051, 054, 063, 064 and 065.**
- **7.4, 3.4, 5.1 and 6.1.**
- **The two `PHASE_STATUS.md` copies differing beyond the unit's fields.**

## 8. Do not

- **Do not change what "acquiring" or "proved" means.** Both are the `RESOLVED:` lines, and a
  departure the tree shows is reported, not repaired.
- **Do not add a threshold, a grace period or a character count to the gate**, and do not try a
  second form after a refusal (V-14).
- **Do not lower a gated letter below dim, drop it, or raise any letter's class.** No letter
  changes, only a sure class that becomes dim.
- **Do not change the keep rule.** Coverage falling refuses the gate, however much MET-CER-SURE
  falls. HM-REQ-012's rationale is why.
- **Do not commit a watched-red test or a refused change under `src` or `tests`** (442 DECIDED
  (2)).
- **Do not read any key into the decoder** (R72).
- **Do not touch, call or reuse `KeyerCwSender.cs`** or any transmit file (§0.2).
- Do not add a new test to either carry-forward line.
- Do not re-point or retire an existing test (R80).
- Do not edit `PARKED.md`, `CW_SPEC.md`, `CW_REQUIREMENTS.md`, or any ruling in `PHASE_PLAN.md` or
  `CLAUDE.md`.
- **Do not install any package. That is `MOVE: stop`.**
- Do not stage, commit or modify `.run-unit\fldigi\`, and do not fetch.
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose a
  timestamp.**

## 9. Committing and pushing

**One commit per step:**
- task 0;
- task 1's fact and trace, with the rule registered;
- task 2, either the kept gate with its test and `metrics.md`, or, if refused, the documents and
  `.run-unit` files only;
- task 3.

**Messages** take the form `unit470 task N: <what> (2.4)`. Task 2's message names the verdict and
the deciding metric, with MET-CER-SURE and coverage before and after, real and synthetic.

**Exit state:** every commit exits with these green:
- the build;
- both carry-forward lines;
- the three floor tests.

The one exception is the dispatcher loss under DECIDED (7).

**Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes first.**
`validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 4 of 5, 8 0 of 6, 9 7 of 8; 9.4 ended as a loop, and this unit
   worked step 2 against R86's words, reported as a mismatch.
B. Step 2 (HM-REQ-010, with HM-REQ-102 measured): the gate -
   no sure letter while pitch or speed is not proved - <kept|refused
   on <metric, condition>>; real MET-CER-SURE <before> -> <after>,
   coverage <before> -> <after>; synthetic the same; sure letters
   inside acquiring <right>/<wrong> of unit 449's 13; the 7.052
   opening's sure letters while acquiring <before> -> <after>;
   2.4's count <0|2> of 3; 2.4 open.
C. The findings weighed against A and B: how many items section 4
   raises, whether any is in the way of 2.4 closing, and the R86
   mismatch stated in one line.
```

```
UNIT:       470 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   no - none - clears a blocker: step 2 criterion 4, count <0|2> of 3
NUMBER:     MET-CER-SURE real <x/y> -> <x/y>, synthetic <x/y> -> <x/y>; coverage real <x> -> <x>; gate <kept|refused>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the R78 table**, per condition, before and after, with the key's kind.
Then give:
1. task 1's inside-and-outside acquiring table, and where unit 449's 13 fall;
2. the 7.052 opening's text before and after, with each character's class;
3. every recording whose text or classes changed, before and after;
4. the first-sure-character times;
5. the commit table, with the five results at each commit.

**Section 2, one paragraph, in the owner's words.** If kept: say that while Hamlet is still
finding a station's pitch and speed, the letters it prints are now shown dim, as guesses, and no
longer as certain. Quote a stretch from before and after. If refused: say that nothing on the
screen changes, and say how many right letters the gate would have dimmed for how many wrong ones.

**Section 4 must say, as a plain reading and not a ruling request:**
- **The R86 mismatch**, in one line, as units 468 and 469 put it.
- **DECIDED (2) and (3)**, the gate's reading of the resolution, as the arbiter's and
  overrulable.
- **If refused:** that HM-REQ-102 and HM-REQ-012 pull against each other on this corpus, with the
  numbers. That is a finding about the requirements, not a request.
- **Any departure of the proof states from their `RESOLVED:` lines** that task 1 found.
- **2.4's count after this unit.**

---

```
ARBITER-DECISION
STEP: 2
APPROACH: gate the sure class on acquiring as PARKED.md's 2026-09-26 resolution defines it - no character emitted sure while the pitch proof or the speed proof is hypothesis or none - traced first for right and wrong sure letters inside acquiring, HM-REQ-102 test watched red on a generated carrier case, judged under R78 against HM-REQ-010, committed if kept else held as a patch
MOVE: work around
WHY: PHASE_PLAN.md step 2 has only 2.4 open, which closes the step after three consecutive units with no kept change and stands at 1 of 3, so step 2 advances only by real attempts at HM-REQ-010; the largest group of the 33 sure-but-wrong letters unit 449 left, 13 read while acquiring, was excluded as parked with the owner, and the owner-side RESOLVED line of 2026-09-26 has since defined acquiring, so this route is newly open and the loop test finds no entry for it.
STATE: partial
DECIDED: author's, overrulable - (1) step 2 is worked as the launcher named it although R86 (PHASE_PLAN.md section 6) reads that steps 2 to 8 are not authorable until 9.4 has a kept change; this is carried to the owner as a mismatch, as units 468 and 469 did, and not a claim that R86 is overruled, 9.4 staying judged a loop under ARBITER.md section 4 with (A) to (E) and (W) refused; (2) 449 DECIDED (4)'s exclusion of a sure-class gate on acquisition rested on its being parked with the owner, and the RESOLVED line of 9a329cdb has answered it, so this applies a later answer and overrules no arbiter ruling; (3) the gate: a character whose class would be sure is emitted dim when, at its emission, the tree's pitch proof state or speed proof state (units 450 and 451, read as they stand) is hypothesis or none; nothing else changes, no letter is dropped, lowered below dim or raised, nothing is promoted after the fact, and no constant is added; (4) the keep rule is 2.2 as written, per condition, real and synthetic, with TheArbitrationEarnsItsPlaceTests green as a further gate; coverage falling refuses the gate; HM-REQ-102 on the 7.052 opening and the first sure character's time per recording are reported and are not keep gates, MET-TACQ not existing (5.4); (5) kept resets 2.4's count to 0, refused takes it to 2 of 3 with the change held as a patch under .run-unit and nothing under src or tests committed (442 DECIDED (2), as 460 DECIDED (3)); one form only, no second form after a refusal; (6) ADVANCES is none - clears a blocker, because 2.4 cannot flip in one unit and this unit is the attempt its count waits on; (7) the app line's headless dispatcher-loop loss gets one rerun, and any type lost again is run alone and must pass, named in the report; (8) 469's section 4 is logged, not chased, its items 3 and 4 used only to choose the test's red case.
LICENCE: PHASE_PLAN.md step 2 lines 2.2, 2.4 and 2.5, section 5, section 6 (R86 as reported, V-14, the documents win, the second decoder is faithful), section R (R77, R78, R80, R81, R85); PARKED.md RESOLVED lines of 2026-09-26 in owner-side commit 9a329cdb (acquiring, pitch proved, speed proved); ARBITER.md sections 3, 4 and 6; arbiter rulings 442 DECIDED (2), 449 DECIDED (4) as answered, 460 DECIDED (3); CW_REQUIREMENTS.md HM-REQ-010, 011, 012, 014, 034, 093, 100, 102; CW_SPEC.md sections 4 and 11; units 448, 449, 450, 451 and 469; R12, R61, R66, R72, R75, R76; V-04, V-08, V-11, V-13, V-14; HM-DEC-095, HM-DEC-155, HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: either the letters Hamlet prints while it is still finding a station's pitch and speed are shown dim as guesses rather than as certain, so fewer wrong letters reach the operator looking sure, or the owner learns exactly how many right letters that rule would cost for how many wrong ones it would catch, and step 2 moves to its last attempt
ADVANCES: none - clears a blocker: step 2 criterion 4
END-ARBITER-DECISION
```
