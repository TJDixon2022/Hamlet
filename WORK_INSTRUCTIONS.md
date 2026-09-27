# Work instruction 467 - HM-REQ-128 judged on conditions as CW_SPEC.md defines them: a union row is a summary, not a condition, and 9.7 is closed on the switch unit 466 built

**Loop unit.** Step 9, criterion 9.7 (`CW_REQUIREMENTS.md` section M: HM-REQ-128), then 9.8.

**Why step 9, why 9.7, and not the launcher's step 2.**
- **R86, `PHASE_PLAN.md` section 6:** *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."* No
  later ruling lifts it. Step 2's only open line, 2.4, counts consecutive step-2 units with no
  kept change. Unit 449 (outcome unit 11) kept a change and reset that count, and it now stands at
  1 of 3. One unit cannot flip 2.4, and R86 bars the unit that would try.
- **9.4 has no route the plan's words allow.** All five mechanisms 9.3 named are recorded as
  refused, by units 459, 462 and 463.
- **9.7 is one question short.** Unit 466 built the switch and measured it. HM-REQ-128 holds on
  the live row and on all 4 losing rows. Its test stays red on one row only, `synthetic, all`,
  which is the union of six synthetic rows at three levels. Unit 466 asked whether a union row is
  a "condition". Under R85 that is a CW question, answered from the documents. **`CW_SPEC.md`
  section 4 answers it:** *"Every condition is a named profile from this file (`CH-*`, `TX-*`,
  `INT-*`, `IMP-*`) and an SNR in the reference bandwidth (section 8), never prose."* A row that
  mixes 0, 5 and 15 dB is not a condition. DECIDED (2) rules it. This unit applies the ruling to
  the test and the record, re-watches the test, and ticks 9.7 if every condition row holds.
- **This is not unit 466's approach again.** Nothing is re-measured to choose a switch, and no
  switch changes. The loop test finds no entry for this approach.

Three tasks. Drop from the back.

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
- If one type needs more than 600 s, split it by recording or by case into several named types or
  filters. Never raise the timeout past 600 s.

**Shell limits:**
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, and so is `rm`.
- Python cannot run here. Everything is C#.
- A multi-line commit uses `-m` more than once.
- Scripts go in `.run-unit\unit467-<name>.sh` and are run with `sh`. Unit 466's
  `.run-unit\unit466-*.sh` helpers may be copied and renamed.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 9 criterion 7`. The launcher reads the digits after "criterion", so this
  means plan line 9.7.
- WHY cites the plan.

**R85. A CW question is answered from the documents, never raised to the owner.** Which rows are
conditions is such a question, and DECIDED (2) answers it. If the tree forces a reading this
instruction did not foresee, record it in one line of section 4 and carry on. **Do not hand the
question back.**

---

## 2. Why this unit exists

**The count today, by the plan's checkboxes:**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | 4 of 5 (2.4's count 1 of 3; barred by R86) |
| 3 | 3 of 6 (barred by R86) |
| 4 | 5 of 7 (barred by R86) |
| 5 | 1 of 6 (barred by R86) |
| 6 | 3 of 6 (barred by R86) |
| 7 | 2 of 5 (barred by R86) |
| 8 | 0 of 6 |
| 9 | 5 of 8 (9.4 open with every mechanism 9.3 named refused; 9.7 and 9.8 open) |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  HM-REQ-128 is judged on the rows CW_SPEC.md section 4 calls
            conditions - one named profile at one SNR - plus the live
            product's own row; a union row across levels or profiles is
            printed as a summary and not asserted; the test naming
            HM-REQ-128 is re-watched failing first on the losing
            condition rows, is green, and 9.7 and 9.8 are ticked.
ADVANCES:   step 9 criterion 7
```

**Read these first. They win over this instruction:**
- `CW_SPEC.md` section 4, line 88 onward: what a condition is;
- `CW_REQUIREMENTS.md` HM-REQ-128, and the verification table's rows 010 (*"one row per condition
  profile"*) and 128 (*"every condition ... switched off per condition on a loss"*);
- `docs/phase-requirements/arbitration.md` and `parity.md` section 3 (the rows);
- `CwSwitchTable`, `TheArbitrationEarnsItsPlaceTests` and `TheArbitrationEarnsItsPlaceFact`, which
  unit 466 built;
- unit 466's `output.md`, sections 3 and 4 item 1.

**The requirement, verbatim:**

> **HM-REQ-128:** "The arbitrated output shall be no worse than the better single decoder on every
> metric of §B and §I, on every condition. If arbitration loses to either decoder alone on any
> condition, it is switched off for that condition and the better decoder alone is used there."

**9.7 is met when all of these are true:**
1. `arbitration.md` states which rows are conditions under DECIDED (2), and which are summaries,
   quoting `CW_SPEC.md` section 4.
2. On every condition row, the output emitted under the switch is no worse than the better decoder
   alone on every metric unit 466's task 1 fixed (`9eef6850`). The live product's row, `real HF,
   all`, is judged on the live path as DECIDED (3) says.
3. `TheArbitrationEarnsItsPlaceTests`, naming HM-REQ-128, asserts exactly those rows. It is watched
   failing first again, on those rows, and is then green.
4. The summary rows are still printed with all three figures and the emitted output. Where the
   emitted output is worse than ours alone on a summary row, the report says so plainly.
5. The report tables all three per condition, with the emitted output beside them.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `d34a0563` or a runner commit on top of it.
- **`PHASE_PLAN.md`**, in both copies:
  - shows 9.1, 9.2, 9.3, 9.5 and 9.6 ticked, and 9.4, 9.7 and 9.8 open;
  - carries R86's line in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **The port** is untouched: `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints
  nothing.
- **Unit 466's work is present:**
  - `CwSwitchTable` with 4 switched rows: TX-FARNS, TX-ITU 0 dB and char-gap-5 0 dB set to ours
    alone, and char-gap-5 5 dB set to the port alone;
  - `CwSwitchTable.Live` set to arbitrate;
  - `TheArbitrationEarnsItsPlaceTests`, with part (b) harness red on `synthetic, all` only;
  - `TheArbitrationEarnsItsPlaceFact` and `arbitration.md`.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-5-output-5.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
  `SESSION.lock` is the launcher's. Do not stage it.
- **Logged and not this unit's:**
  - the reload's `RULES_AT` disagreement (HM-DEC-165 against CPS-DEC-0183);
  - `outcome-read`'s step titles for steps 2, 3 and 8, which differ from `PHASE_PLAN.md`'s.

**Entry figures, 466's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** as 466's exit.
- **App line:** as 466's exit, under DECIDED (6).
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:** MET-CER-SURE 33 of 436, MET-INVENTED 33 over
  473, coverage 403 over 473, MET-WBE 37 over 113.
- **Synthetic set, 12 cases, exact key:** MET-CER-SURE 14 of 173, MET-INVENTED 14 over 252,
  coverage 159 over 252, MET-WBE 44 over 84.
- **`TheArbitrationEarnsItsPlaceTests`:** (a) 5 of 5; (b) live green; (b) harness red on
  `synthetic, all` only.
- **Decode time**, the live path over 690 s of real audio: 64.27 s.

If any entry figure differs, report it and use the measured one.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31. It is on neither carry-forward line.
- `TheArbitrationEarnsItsPlaceTests` part (b) harness, red on `synthetic, all` at entry. That red
  is what this unit resolves.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under DECIDED (6).
- The parity run rewriting the decode-time rows of `parity.md`. Keep a copy under `.run-unit\` and
  restore the committed file, as units 462 to 466 did.

---

## 4. Rulings in force - transcribed, do not re-argue

**The ordering:**
- **R86** (`PHASE_PLAN.md` section 6): *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."*
- **R84** (`PHASE_PLAN.md` section R): *"... and the arbitration is switched off on any condition
  where it loses to the better decoder alone (128)."*
- **R85** (`PHASE_PLAN.md` section R): *"A question about a metric, a threshold, a floor, a
  definition or what the decoder should do is answered from `CW_REQUIREMENTS.md`, `CW_SPEC.md`,
  the radio's manual, the second decoder's source and research, recorded as the arbiter's reading
  and overrulable, and never parked for the owner."*
- **Section 6:** *"Where this plan and the two documents differ, the documents win, and the
  difference is a finding in the report."*
- **HM-REQ-122 and 129, and section 6:** *"The second decoder is faithful, not improved."* Not one
  line under `Cw\Second\` changes.

**The definition this unit applies:**
- **`CW_SPEC.md` section 4:** *"Every condition is a named profile from this file (`CH-*`, `TX-*`,
  `INT-*`, `IMP-*`) and an SNR in the reference bandwidth (section 8), never prose."*
- **`CW_REQUIREMENTS.md` verification table, row 010:** *"one row per condition profile"*.

**Unit 466's rules stand as fixed at `9eef6850`.** That covers the metric list, the loss rule, the
better-decoder rule and the path rule. **None changes, and no switch changes** (V-14). This unit
changes which rows the test asserts on, per the documents' definition. It changes no threshold, no
rule and no switch.

**Standing rulings this unit leans on:**
- **R77.** The test keeps naming HM-REQ-128.
- **R80.** No traceability, test inventory or decision-log work.
- **R12.** A session rewrites its own tests. `TheArbitrationEarnsItsPlaceTests` is unit 466's, and
  narrowing its row set to conditions is not a retirement.
- **R72.** No word, dictionary or callsign prior.
- **R61.** A key is inferred unless it was transcribed.
- **V-11, V-13, V-14.**

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode.
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched, not called and not reused.**
- `CLAUDE.md` §12.5: a fixture built from the same misunderstanding as the code proves nothing.
  The re-watch in task 1 uses an empty switch table, not the table under test.
- HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task;
- when `arbitration.md` names the condition rows and the summary rows;
- when the re-watch comes back red, naming the rows;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 467 - STEP 9` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 467 with `CURRENT_STEP: 9`, in both copies.
3. Bump the patch from 1.13.153 to 1.13.154.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `BothDecodersAreScoredAlikeTests`;
   - the port's own tests;
   - unit 465's three tests;
   - `TheArbitrationEarnsItsPlaceTests`, both parts.
6. Save the emitted transcript, harness and live, each character with its class and p, to
   `.run-unit\unit467-emitted-before.txt` and `.run-unit\unit467-emitted-live-before.txt`. Every
   byte-identical check in this unit is made against these.

### Task 1 - conditions named, the test narrowed to them, and re-watched

**Nothing under `src` changes in this task.** If the tree forces a `src` change, name it in
section 4 and leave 9.7 open.

1. **Classify every row** of `arbitration.md` section 2 and `parity.md` section 3, under DECIDED
   (2) and (3):
   - **condition**: one named sender or channel profile and one level;
   - **the live product's row**: `real HF, all`, judged on the live path;
   - **summary**: a union across levels or profiles, such as `synthetic, all`.

   Each real row is marked with its SNR stated as `not measured`. Give each row's class and the
   one-line reason. **Write this into `arbitration.md`** as a new section that quotes `CW_SPEC.md`
   section 4, and into `TheArbitrationEarnsItsPlaceFact`'s header. The fact still asserts
   nothing.
2. **Narrow part (b) of `TheArbitrationEarnsItsPlaceTests`** to assert only:
   - the condition rows, on the path unit 466's path rule gives each;
   - `real HF, all` on the live path.

   The rows it asserts come from one list in the test, named for DECIDED (2) and (3), with a
   comment quoting `CW_SPEC.md` section 4. Summary rows are not asserted. They are still computed,
   and printed by the fact. **Do not touch part (a).**
3. **Watch it fail first again.** Run the narrowed part (b) against an empty switch table, the
   same way unit 466 did. It must be red on exactly the 4 losing condition rows: TX-FARNS, TX-ITU
   0 dB, char-gap-5 0 dB and char-gap-5 5 dB. If it is red on any other row, or green on any of
   the 4, stop the task, leave 9.7 open and report it. Then restore the table and run it green.
   Save both printouts to `.run-unit\unit467-watch-red.txt` and `.run-unit\unit467-green.txt`.
4. **Print the summary rows** in `arbitration.md` with all three figures and the emitted output.
   State plainly that `synthetic, all` emits worse than ours alone on 012 (158 against 159 of 252)
   and 081 (48 against 44 of 84), and that this comes from char-gap-5 5 dB's port-alone switch.
   Use the figures the tree gives now.
5. **Byte-identical:** the emitted transcripts, harness and live, against task 0's saves; and
   `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` printing nothing.
6. Run the three floor tests and both carry-forward lines. **Commit** `arbitration.md`, the fact
   and the test in one commit.
7. **Tick 9.7** in both copies of `PHASE_PLAN.md`, in a follow-on commit, only when all five of
   section 2's conditions hold. If any part fails, leave 9.7 open and say which part.

**Drop candidate:** the live-path reprint of the synthetic condition rows in `arbitration.md`.
Unit 466's live table stands for them. The classification, the narrowed test, the re-watch and
the summary rows' plain statement are never shed.

### Task 2 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `BothDecodersAreScoredAlikeTests`;
- the port's own tests;
- unit 465's three tests;
- `TheArbitrationEarnsItsPlaceTests`, both parts;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- `git diff d34a0563 -- src`, which prints nothing;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with its results on the five at its exit: build, both
  carry-forward lines, and the three floor tests.

**Ticks:**
- **9.7**, as task 1 says.
- **9.8**, in its own follow-on commit, only if 9.7 is ticked and the exit commit has the three
  floor tests and both carry-forward lines green. The one exception is DECIDED (6)'s dispatcher
  loss, and only as that item handles it.
- Do not tick 9.4, or any line of steps 2 to 8.

---

## 7. Parked - do not touch, do not raise

- **9.4 and every mechanism 9.3 named, (A) to (E) and (W), in any form.**
- **Every switch in `CwSwitchTable`, the vote table, both confidences, calibration and
  HM-REQ-127's margin.** None is re-chosen or refitted.
- **Unit 466's section 4:**
  - item 2, the margin;
  - item 5, the switches that change no text today;
  - item 6, the live path against the harness on char-gap-5 15 dB. The path rule stands;
  - item 7, unpaired characters on the live sheet;
  - item 8, the dispatcher loop, beyond DECIDED (6).
- **Unit 465's section 4 items 3 to 9; unit 464's item 5; unit 463's items 3 and 4.**
- **The four questions `PARKED.md` records, and their `RESOLVED:` answers.**
- **Step 2's decoder work, and 2.4's count.** The count stays 1 of 3.
- **Steps 3 to 8,** under R86.
- **The two `PHASE_STATUS.md` copies differing beyond the unit's fields.**

## 8. Do not

- **Do not change any line under `src`.** This unit is a test, a fact and a record.
- **Do not change a switch, a rule of `9eef6850`, a metric, a threshold or a key** (V-14). The only
  thing that changes is which rows part (b) asserts, per `CW_SPEC.md` section 4.
- **Do not drop a row from the print.** A summary row is printed with its figures, never hidden.
- **Do not show a decoder's name, or the switch in force, on the CW tab** (HM-REQ-121).
- Do not use any rule that knows words, callsigns or letter frequencies (R72).
- **Do not touch, call or reuse `KeyerCwSender.cs`** or any transmit file (§0.2).
- Do not add a new test to either carry-forward line.
- Do not re-point or retire an existing test (R80). Part (a) is untouched.
- Do not edit `PARKED.md`, `CW_SPEC.md`, `CW_REQUIREMENTS.md`, or any ruling in `PHASE_PLAN.md` or
  `CLAUDE.md`.
- **Do not install any package. That is `MOVE: stop`.**
- Do not stage, commit or modify `.run-unit\fldigi\`, and do not fetch.
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose a
  timestamp.**

## 9. Committing and pushing

**One commit per step:**
- task 0;
- task 1's `arbitration.md`, fact and test;
- the 9.7 tick, if earned;
- the 9.8 tick, if earned;
- task 2.

**Messages** take the form `unit467 task N: <what> (9.7)`. Task 1's message names the condition
rows the re-watch found red.

**Exit state:** every commit exits with these green:
- the build;
- both carry-forward lines;
- the three floor tests.

The one exception is the dispatcher loss under DECIDED (6).

**Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes first.**
`validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 2 of 5, 8 0 of 6, 9 <n> of 8; steps 2 to 8 still barred by R86,
   and 9.4 open with every mechanism 9.3 named refused.
B. Step 9, criterion 9.7 (HM-REQ-128): condition rows <k>, summary
   rows <list>, live row real HF, all; re-watch red on <rows> against
   an empty table; green on every condition row <yes|no>; summary
   synthetic, all emitted against ours alone on 012 <x vs y> and 081
   <x vs y>; src unchanged <yes|no>; the port byte-identical <yes|no>;
   emitted text byte-identical to entry <yes|no>; 9.7 <ticked|open>;
   9.8 <ticked|open>.
C. The findings weighed against A and B: how many items section 4
   raises, whether any is in the way of 9.7 or 9.8, and whether R86
   with 9.4's refusals leaves any step 9 line authorable.
```

```
UNIT:       467 - <complete|stopped> at task N of 2, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-128 <met|not> on <k> of <k> condition rows and the live row; re-watch red on <n> rows; real HF, all MET-CER-SURE arb/ours/port <x/y/z> live
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the row classification**: every row, its class, and the reason. Then give
the three-way table as unit 466 printed it, with a class column added and the summary rows marked.
Then give:
1. the red and green printouts of part (b);
2. the byte-identical checks;
3. the commit table, with the five results at each commit.

**Section 2, one paragraph, in the owner's words.** Nothing on the screen changes. Say what "every
condition" was taken to mean and where that came from. Say that on the mixed synthetic set, taken
as a whole, the switch to fldigi's reader on one kind of signal costs one right letter and four
word spaces. Say that no condition is worse for having two readers.

**Section 4 must say, as a plain reading and not a ruling request:**
- **Which rows are conditions is the arbiter's reading of `CW_SPEC.md` section 4, overrulable.**
  If the owner rules union rows are conditions, 9.7 reopens.
- **HM-REQ-127's margin is 0.05, the requirement's recommended value, held provisionally.**
- **With 9.7 and 9.8 ticked, step 9's only open line is 9.4, which has no authorable route, and
  R86 holds steps 2 to 8 behind it.** The next arbiter has no step R86 permits. This is logged for
  the owner, not a stop, because it touches neither transmit nor what the product promises the
  operator.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: rule aggregate union rows are not conditions under CW_SPEC section 4 one named profile at one SNR, narrow HM-REQ-128 test part b to condition rows and the live row, re-watch red on an empty switch table, tick 9.7 on the switch unit 466 built with no switch or rule changed
MOVE: work around
WHY: PHASE_PLAN.md section 6's R86 bars steps 2 to 8 until 9.4 has a kept change, so the launcher's step 2 line 2.4 cannot move, and its count stands at 1 of 3 after unit 449's kept change; 9.4 has no route left. 9.7 (HM-REQ-128) fails on only the union row synthetic, all, and CW_SPEC.md section 4 defines a condition as one named profile at one SNR, so under R85 the arbiter answers unit 466's question from the documents and the unit closes 9.7 and 9.8 without changing a switch or a rule.
STATE: partial
DECIDED: author's, overrulable - (1) step 9 criterion 9.7 is worked instead of the launcher's step 2 on R86 (PHASE_PLAN.md section 6), as units 462 to 466's arbiters did; 2.4's count stays at 1 of 3; (2) under R85 and CW_SPEC.md section 4 ("Every condition is a named profile ... and an SNR in the reference bandwidth, never prose") with the verification table's "one row per condition profile", a condition row is one named sender or channel profile at one level; a row that is the union of rows at different levels or profiles (synthetic, all) is a summary, printed with all three figures and the emitted output but not asserted by HM-REQ-128's test. This answers a question unit 466's instruction left open ("for every condition row") and overrules no earlier arbiter ruling; (3) real HF, all is asserted on the live path as the row the live product runs under (unit 466 DECIDED (5)); the real per-sender rows are condition rows with SNR not measured, as unit 466 tabled them; (4) unit 466's metric list, loss, better-decoder and path rules at 9eef6850 and every switch in CwSwitchTable are unchanged, and nothing under src changes, so V-14 is not engaged - the test's row set is narrowed to the documents' definition, and no threshold or rule is loosened; (5) 9.8 is ticked in its own commit only if 9.7 is ticked and the exit commit is green on the five; (6) the app line's headless dispatcher-loop loss: one rerun, and any type lost again is run alone and must pass, named in the report.
LICENCE: PHASE_PLAN.md step 9 lines 9.7 and 9.8, section R (R84, R85), section 6 (R86, the documents win, the second decoder is faithful, V-14, R78); CW_SPEC.md section 4 on conditions; CW_REQUIREMENTS.md HM-REQ-121, 122, 128, 129 and the verification table's rows 010 and 128; R12, R61, R72, R77, R80; unit 466's rules at 9eef6850 and its DECIDED (5) and (9); HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: Hamlet's second reader is shown to earn its place, or step aside, on every kind of signal the specification names: on no such signal is the transcript worse for having two readers, and the one mixed test set where it is slightly worse is shown to the owner rather than hidden
ADVANCES: step 9 criterion 7
END-ARBITER-DECISION
```
