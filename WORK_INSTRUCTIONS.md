# Work instruction 468 - the TX-* sender profiles are generated with their keys exact by construction, and HM-REQ-050 is measured on each must-tier fist at 15 dB on CH-AWGN

**Loop unit.** Step 7, criterion 7.2 (`CW_REQUIREMENTS.md` section E: HM-REQ-050, through
HM-REQ-013).

**Why step 7 and 7.2, and not the launcher's step 2 or step 9's 9.4.**
- **9.4 is a loop, and it ends here (`ARBITER.md` section 4).** 9.3 named five fldigi mechanisms.
  All five have been screened, in six forms, and every one was refused under R78:
  - (D) pair speed tracking, by unit 459;
  - (C) the two-dot class edge, (B) the spike rule and (W) the word-space edge, by unit 462;
  - (A) detection, as A1 the half-dit integrator and A2 the AGC, by unit 463.

  Another form of any of them would resemble one that failed. Step 9 stands at 7 of 8 and is not
  re-attempted.
- **R86 and this move disagree, and this is reported, not ruled away.** R86 reads: *"Steps 2 to 8
  are not authorable until 9.4 has a kept change."* It waits on a line that no route the plan
  allows can reach. Four instructions point the other way:
  - `ARBITER.md` section 4 says a looping step ends and the arbiter "move[s] to another step";
  - the launcher says "If every route you can see at a criterion is recorded as no ... choose
    another criterion or another step";
  - `PHASE_PLAN.md` section 5 says "the loop need never stall for want of work";
  - step 7 says it is "Independent of steps 2 to 6".

  This unit does **not** claim R86 is overruled. Only the owner does that. DECIDED (2) records
  the mismatch for the owner. If the owner holds R86, all this unit leaves is test fixtures and a
  measurement. Nothing the operator reads changes, and the loop goes back on his word.
- **Why 7.2 among steps 2 to 7.**
  - **2.4, the launcher's line,** cannot flip in one unit. Its count is 1 of 3, which is unit
    467's arbiter's reading and is not re-argued here.
  - **3.4's recording** is not in the tree (unit 443's finding).
  - **6.1** needs ITU-R M.1677-1 vendored. It is not in the tree, and nothing is fetched.
  - **5.1's widening** is recorded as refused.
  - **7.2 builds the four fists HM-REQ-050 names.** Sections D, E, H and I are judged per sender,
    and no requirement that names a sender can be measured at its own condition without them.
    Unit 439's finding 1 says so, and so does section T: *"050–053 | none: needs TX-* profiles in
    generator"*.

  No unit has tried this, and the loop test finds no entry for the approach.

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
- If one type needs more than 600 s, split it by profile or by case into several named types or
  filters. Never raise the timeout past 600 s.

**Shell limits:**
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, and so is `rm`.
- Python cannot run here. Everything is C#.
- A multi-line commit uses `-m` more than once.
- Scripts go in `.run-unit\unit468-<name>.sh` and are run with `sh`. Unit 467's
  `.run-unit\unit467-*.sh` helpers may be copied and renamed.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 7 criterion 2`. The launcher reads the digits after "criterion", so this
  means plan line 7.2.
- WHY cites the plan.

**R85. A CW question is answered from the documents, never raised to the owner.** A profile's
parameter, a seed or a speed is such a question. DECIDED (4) and (5) answer the ones foreseen. If
the tree forces a reading this instruction did not foresee, record it in one line of section 4 and
carry on. **Do not hand the question back.**

---

## 2. Why this unit exists

**The count today, by the plan's checkboxes:**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | 4 of 5 (2.4's count 1 of 3) |
| 3 | 3 of 6 |
| 4 | 5 of 7 |
| 5 | 1 of 6 |
| 6 | 3 of 6 |
| 7 | 2 of 5 (7.1 and 7.5 met; 7.2, 7.3 and 7.4 open) |
| 8 | 0 of 6 |
| 9 | 7 of 8 (9.4 open, and every mechanism 9.3 named is refused) |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The test fixtures generate every TX-* sender profile
            CW_SPEC.md section 10 gives numbers for, each keyed with an
            exact key by construction and each proved on its own keying
            to have the timing its definition states; and HM-REQ-050 is
            measured - met or not, per profile - on TX-ITU, TX-KEYER-W,
            TX-FARNS and TX-TIGHT at 15 dB in the 2500 Hz reference on
            CH-AWGN, with the decoder unchanged.
ADVANCES:   step 7 criterion 2
```

**Read these first. They win over this instruction:**
- `CW_SPEC.md` section 10, the `TX-*` table: every profile's definition and tier;
- `CW_SPEC.md` section 8, the reference bandwidth;
- `CW_SPEC.md` section 11, the metrics;
- `CW_REQUIREMENTS.md`:
  - HM-REQ-013 and HM-REQ-050 to 054;
  - the verification table's rows 013 and 050;
  - section T's row *"050–053"*;
- `docs/phase-requirements/channels.md`, and `CwChannel.cs` with
  `TheChannelProfilesAreWhatTheySayTests`, which unit 461 built for 7.1;
- `SyntheticCq.cs`, `CwFixtureGenerator.cs` and `CwFixtureRecipe`, which already key TX-ITU and a
  character gap of 5.

**The requirements, verbatim:**

> **HM-REQ-050:** "On each of TX-ITU, TX-KEYER-W, TX-FARNS and TX-TIGHT, the decoder shall meet
> HM-REQ-013."
>
> **HM-REQ-013:** "On every must-tier sender profile at 15 dB reference on CH-AWGN, the decoder
> shall emit every sent character as sure and correct." Rationale: "Coverage 100 %, MET-CER-SURE
> 0, MET-WBE 0."
>
> Verification row 013: *"T | each must TX at 15 dB, CH-AWGN | MET-COVERAGE, MET-CER-SURE,
> MET-WBE | 1.00, 0, 0 | none permitted | synthetic + any real ≥ 15 dB | pass/fail, not a ratchet
> (V-08)"*.

**7.2 is met when all of these are true:**
1. The generator produces every TX-* profile that section 10 gives numbers for. A profile whose
   defining feature section 10 gives no number for is refused by name, with the reason, as 7.1
   refused CH-MDV. The four must-tier profiles are never refused.
2. Each produced profile is proved on its own keying, from the rendered envelope and not from the
   recipe, to have the ratios and gaps its definition states. The proof is watched failing first.
3. The recipe for every case is written down so that another unit can rebuild it.
4. HM-REQ-050 is measured on each of the four must-tier profiles at 15 dB in the 2500 Hz reference
   on CH-AWGN, per HM-REQ-013's three numbers, and the report states met or not met per profile.

**Not met is a finding, not a failure of this unit.** 7.2 asks for the measurement.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `0f30a782` or a runner commit on top of it.
- **`PHASE_PLAN.md`**, in both copies:
  - shows 7.1 and 7.5 ticked, and 7.2, 7.3 and 7.4 open;
  - shows 9.4 as the only open line of step 9;
  - carries R86's line in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **Unit 461's channel layer is present:** `CwChannel`, with `CH-AWGN` among its profiles, and
  `TheChannelProfilesAreWhatTheySayTests`.
- **The port** is untouched: `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints
  nothing.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-6-output-5.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
  `SESSION.lock` is the launcher's. Do not stage it.
- **Logged and not this unit's:**
  - the reload's `RULES_AT` disagreement (HM-DEC-165 against CPS-DEC-0183);
  - `outcome-read`'s step titles for steps 2, 3 and 8, which differ from `PHASE_PLAN.md`'s.

**Entry figures, 467's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278, under DECIDED (7).
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:** MET-CER-SURE 33 of 436, MET-INVENTED 33 over
  473, coverage 403 over 473, MET-WBE 37 over 113.
- **Synthetic set, 12 cases, exact key:** MET-CER-SURE 14 of 173, MET-INVENTED 14 over 252,
  coverage 159 over 252, MET-WBE 44 over 84.
- **`TheChannelProfilesAreWhatTheySayTests`:** 71 of 71.
- **`TheArbitrationEarnsItsPlaceTests`:** both parts green.

If any entry figure differs, report it and use the measured one.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31. It is on neither carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under
  DECIDED (7).
- The parity run rewriting the decode-time rows of `parity.md`. Keep a copy under `.run-unit\` and
  restore the committed file, as units 462 to 467 did.
- **HM-REQ-050 not met on one or more profiles.** Report it. It is the measurement, not a
  regression.

---

## 4. Rulings in force - transcribed, do not re-argue

**The ordering, and the mismatch this unit carries:**
- **R86** (`PHASE_PLAN.md` section 6): *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."* This
  unit is authored against it under DECIDED (2), which is reported to the owner as a mismatch and
  is not a claim that R86 is overruled.
- **`ARBITER.md` section 4:** *"If you judge it a loop, the step ends. Say so, name the approaches
  that were tried, and move to another step or declare it unachievable."*
- **`PHASE_PLAN.md` section 5:** *"Steps 2, 3, 4, 5, 6 and 7 each depend only on those two, so
  there are six independent places to route and the loop need never stall for want of work."*
- **Step 7:** *"Independent of steps 2 to 6."*

**The requirements this unit measures:**
- **R77:** every new test names the requirement it proves. The profile proof names the section 10
  profiles and HM-REQ-050. The measurement fact names HM-REQ-050 and HM-REQ-013.
- **R78:** it is not engaged, because nothing under `src` changes.
- **Section 6:** *"Where this plan and the two documents differ, the documents win, and the
  difference is a finding in the report."*
- **Section 6:** *"No fixture is admitted by lowering a gate (V-04), and no separation limit,
  confirmation rule or plausibility bound is loosened to pass a fixture (V-14)."*
- **R85:** a CW question is answered from the documents, recorded as the arbiter's reading and
  overrulable, and never parked for the owner.

**Standing rulings this unit leans on:**
- **R12.** A session rewrites its own tests.
- **R61.** A key is inferred unless it was transcribed. Every key here is exact by construction.
- **R72.** No word, dictionary or callsign prior, in any form. The text is a key for scoring only,
  never a decoder input.
- **R80.** No traceability, test inventory or decision-log work.
- **V-04, V-06, V-08, V-13, V-14.**
- **HM-REQ-122 and 129:** the second decoder is faithful, not improved. Not one line under
  `Cw\Second\` changes.

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode.
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched, not called and not reused.** The sender profiles are
  audio synthesis in the test fixtures. They are not a keyer, and nothing reaches the radio.
- `CLAUDE.md` §12.5: a fixture built from the same misunderstanding as the code proves nothing.
  The profile proof measures the rendered envelope, never the recipe's own numbers read back, and
  it shares no code with the decoder's timing estimators.
- HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task;
- when the profile list is fixed, naming each profile as produced or refused;
- when the profile proof comes back red, naming what failed;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 468 - STEP 7` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 468 with `CURRENT_STEP: 7`, in both copies.
3. Bump the patch by one from what the tree carries. It should be 1.13.154, going to 1.13.155.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `TheChannelProfilesAreWhatTheySayTests`;
   - `TheArbitrationEarnsItsPlaceTests`, both parts.
6. **The trace, before anything is built.** Read `CwFixtureRecipe`, `SyntheticCq.Recipe` and
   `CwChannel.Generate`. Print to `.run-unit\unit468-trace.txt`:
   - which timing parameters the tree can already vary: element, character and word gap, dah
     ratio, character speed against overall speed, and per-element jitter;
   - which of section 10's profiles each parameter serves;
   - what is missing;
   - how `CwChannel` takes a keyed envelope and sets the SNR in the 2500 Hz reference.

   Nothing under `src` or `tests` changes in this step.

### Task 1 - the TX-* profiles, generated and proved

**Nothing under `src` changes.** Everything is in
`tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/`.

1. **Fix the profile list under DECIDED (4)** before writing code. Print it in
   `docs/phase-requirements/senders.md`, one row per section 10 profile, with these columns:
   - the id, the tier, and section 10's definition quoted;
   - each parameter with the number or range it takes, and where the number comes from (section
     10, a named capture, or `TX-ITU nominal`);
   - the `[verify]` marks section 10 carries, carried into the row;
   - produced or refused, with the reason for a refusal.
2. **Build `CwSender`** (or the tree's own name for it) beside `CwChannel`. For a profile, a text,
   a speed and a seed, it produces a keyed envelope and a key sidecar, and the key is the text
   itself: exact by construction (R61, V-13). The profile varies only timing, never the characters
   sent. The case is then passed through `CwChannel`'s CH-AWGN at the named SNR in the 2500 Hz
   reference. That uses the shaped noise band and never digital silence (V-06). The sidecar
   carries the profile id, speed, seed, SNR and every drawn parameter.
3. **Write `TheSenderProfilesAreWhatTheySayTests`**, naming the section 10 ids and HM-REQ-050. For
   each produced profile, it measures from the rendered envelope before noise, never from the
   recipe:
   - the dah-to-dit ratio;
   - element, character and word gaps in units of the measured dit;
   - for TX-FARNS, character speed against overall speed.

   It asserts each against the definition's number or range. The measuring code is written for
   this test. It shares nothing with `CwUnitEstimator`, the lattice or any decoder timing code
   (§12.5).
4. **Watch it fail first.** Run the proof against a sender that ignores its profile and keys
   exactly 1:3:1:3:7. It must be red on every produced profile except TX-ITU. Save the printout to
   `.run-unit\unit468-senders-red.txt`. Then run it against `CwSender`, green, and save that to
   `.run-unit\unit468-senders-green.txt`.
5. **Write the recipe** into `senders.md`: the construction, the seeds, and how each drawn
   parameter is drawn. Write the section *"What these fixtures do not prove"* (§12.5, V-04): they
   are the spec's parameter ranges, not any operator's fist, and a floor on them is a floor on
   section 10's numbers.
6. Run `TheChannelProfilesAreWhatTheySayTests`, the three floor tests and both carry-forward lines.
   **Commit** `CwSender`, the proof and `senders.md` in one commit.

### Task 2 - HM-REQ-050 measured

1. **Generate the cases** under DECIDED (5): each of the four must-tier profiles, at
   `SyntheticCq.Text`, at each of `SyntheticCq.Speeds`, one fixed seed each, at 15 dB in the 2500
   Hz reference on CH-AWGN. That is 12 cases. The should-tier profiles produced in task 1 get the
   same three cases each, reported under HM-REQ-052, not HM-REQ-050.
2. **Write `TheMustFistsAtFifteenDecibelsFact`**, naming HM-REQ-050 and HM-REQ-013. It asserts
   nothing. It drives each case through the same harness and the same `Measure` and `CwMetrics`
   calls `TheRequirementsAreMeasuredTests` uses, and prints per case and per profile:
   - ours alone:
     - MET-COVERAGE as sure-and-right over sent;
     - MET-CER-SURE;
     - MET-WBE;
     - MET-INVENTED;
     - the decoded text beside the key;
   - HM-REQ-013's three numbers against 1.00, 0 and 0, and **met or not met** per case and per
     profile. A profile is met only if all three of its cases are;
   - the emitted output under `CwSwitchTable` as it stands, and the port alone, beside ours. This
     is the drop candidate.
3. **Write the table** into `senders.md` as a measurement section. Every figure carries its key's
   kind, `synthetic, exact` (V-13).
4. **Write `metrics.md`'s running line** for HM-REQ-050 with the date and this unit's number.
5. Run the three floor tests and both carry-forward lines. **Commit** the fact and the two
   documents. Then **tick 7.2** in both copies of `PHASE_PLAN.md`, in a follow-on commit, when
   section 2's four conditions hold. If any fails, leave 7.2 open and say which.

**Drop candidate:** the emitted-output and port-alone columns in task 2's step 2, and the
should-tier cases. The four must-tier profiles, their proof watched red, the recipe, and
HM-REQ-050's verdict per profile on ours are never shed.

### Task 3 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `TheChannelProfilesAreWhatTheySayTests`;
- `TheSenderProfilesAreWhatTheySayTests`;
- `TheArbitrationEarnsItsPlaceTests`, both parts;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff 0f30a782 -- src`, which prints nothing;
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with its results on the five at its exit: build, both
  carry-forward lines, and the three floor tests.

**Ticks:** 7.2 only, as task 2 says. Do not tick 7.4, even if it looks close. It waits on the
INT-* profiles of 7.3. Do not tick 7.3, 9.4, or any line of steps 2 to 6 or 8.

---

## 7. Parked - do not touch, do not raise

- **9.4 and every mechanism 9.3 named, (A) to (E) and (W), in any form.** The loop is ended.
- **Every switch in `CwSwitchTable`, the vote table, both confidences, calibration and
  HM-REQ-127's margin.** The new sender cases are not added as switch rows. Where the emitted
  output loses on one, that is a line in section 4, and nothing is switched.
- **The INT-* profiles (7.3) and HM-REQ-051's sensitivity sweep.** They come after this.
- **HM-REQ-054's word-boundary behaviour on TX-FARNS.** Measure it only as MET-WBE falls out of
  task 2. Nothing is built for it.
- **Unit 467's section 4 items 1 to 3, 5 and 6; unit 466's section 4.**
- **The four questions `PARKED.md` records, and their `RESOLVED:` answers.**
- **Step 2's decoder work, and 2.4's count.** The count stays 1 of 3.
- **The two `PHASE_STATUS.md` copies differing beyond the unit's fields.**

## 8. Do not

- **Do not change any line under `src`.** This unit is fixtures, a proof, a fact and two
  documents.
- **Do not invent a range section 10 does not give** (V-04, §12.5). A missing number is a refusal
  by name, or `TX-ITU nominal` for a secondary parameter, as DECIDED (4) says.
- **Do not tune a profile, a seed, a text or a speed to make a case pass** (V-14). The seeds are
  fixed before any decode is run and are printed.
- **Do not read the key into the decoder** (R72).
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
- task 1's sender, proof and `senders.md`;
- task 2's fact, `senders.md` and `metrics.md`;
- the 7.2 tick, if earned;
- task 3.

**Messages** take the form `unit468 task N: <what> (7.2)`. Task 1's message names the profiles
produced and refused. Task 2's names HM-REQ-050's verdict per must-tier profile.

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
   7 <n> of 5, 8 0 of 6, 9 7 of 8; 9.4 ended as a loop with every
   mechanism 9.3 named refused, and this unit worked step 7 against
   R86's words, reported as a mismatch for the owner.
B. Step 7, criterion 7.2 (HM-REQ-050): profiles produced <list>,
   refused <list with reason>; proof watched red on <profiles> against
   a nominal sender, then green <yes|no>; HM-REQ-050 at 15 dB on
   CH-AWGN, ours: TX-ITU <met|not>, TX-KEYER-W <met|not>, TX-FARNS
   <met|not>, TX-TIGHT <met|not>, with the worst case's coverage,
   MET-CER-SURE and MET-WBE; src unchanged <yes|no>; 7.2
   <ticked|open>.
C. The findings weighed against A and B: how many items section 4
   raises, whether any is in the way of 7.2, 7.3 or 7.4, and the R86
   mismatch stated in one line.
```

```
UNIT:       468 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-050 <met|not> on <k> of 4 must-tier profiles at 15 dB CH-AWGN (synthetic, exact); worst coverage <x/y>, MET-CER-SURE <x/y>, MET-WBE <x/y>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with HM-REQ-050's table:** per must-tier profile and per case, the key, what
ours read, and the three HM-REQ-013 numbers with met or not. Then give:
1. the profile list from `senders.md`, produced and refused;
2. the proof's red and green printouts;
3. the should-tier cases, if run;
4. the commit table, with the five results at each commit.

**Section 2, one paragraph, in the owner's words.** Nothing on the screen changes. Say that
Hamlet can now make test signals in the four kinds of "fist" the specification says it must read,
and that each was checked to be keyed the way the specification describes. Say, per fist, whether
a loud clean signal reads whole. Where it does not, quote what it read against what was sent.

**Section 4 must say, as a plain reading and not a ruling request:**
- **The R86 mismatch.** This unit worked step 7 while R86's words bar steps 2 to 8 until 9.4 has a
  kept change. 9.4 has no route left, because every mechanism 9.3 named was refused. `ARBITER.md`
  section 4 and the launcher direct a move to another step. If the owner holds R86, this unit's
  work is fixtures and a measurement, and nothing the operator reads has changed.
- **Every profile parameter taken as the arbiter's reading of `CW_SPEC.md` section 10,
  overrulable,** with the `[verify]` marks carried.
- **Where HM-REQ-050 is not met,** the profile, the case, and what was read. This is recorded as a
  finding for the decoder steps and not chased here.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: build TX-* sender profile keying in the test fixtures, exact key by construction, prove each profile's ratios and gaps on its own keying, over CH-AWGN at 15 dB reference, measure HM-REQ-050 via HM-REQ-013 on each must-tier profile, decoder unchanged
MOVE: work around
WHY: Step 9's only open line, 9.4, is a loop under ARBITER.md section 4: all five mechanisms 9.3 named are recorded as refused in six forms (units 459, 462, 463), so step 9 stands at 7 of 8 and the work moves on. The launcher's 2.4 cannot flip in one unit, 3.4's recording is absent and 6.1's source is unvendored, so PHASE_PLAN.md 7.2 (HM-REQ-050), which step 7 says is independent and section T says no requirement for a named fist can be measured without, is taken. The loop test finds no entry for it.
STATE: partial
DECIDED: author's, overrulable - (1) 9.4 is judged a loop under ARBITER.md section 4, with every mechanism 9.3 named, (A) to (E) and (W), refused, and it is not re-attempted; (2) step 7 is worked although R86 (PHASE_PLAN.md section 6) reads that steps 2 to 8 are not authorable until 9.4 has a kept change. This is a mismatch reported for the owner, not a claim that R86 is overruled: R86 waits on a line with no route, while ARBITER.md section 4, the launcher's instruction to choose another step when every route is recorded no, and PHASE_PLAN.md section 5 direct the move. If the owner holds R86, this unit leaves only fixtures and a measurement; (3) 7.2 is chosen over the launcher's 2.4 because 2.4's count is 1 of 3 (unit 467's arbiter's reading, not re-argued), 3.4's recording is not in the tree, 6.1 needs M.1677-1 vendored and nothing is fetched, and 5.1's widening is recorded refused; (4) under R85, each TX-* profile takes CW_SPEC.md section 10's numbers as written, with [verify] marks carried: TX-ITU exactly 1:3:1:3:7; TX-KEYER-W ratio 2.5 to 3.5 with gaps +-30 percent; TX-FARNS character speed above overall with character gap 3 to 7 units at character speed and a longer word gap; TX-TIGHT from the 013347 capture as HM-DEC-101 records it, or as measured on that capture if HM-DEC-101 gives no figure; TX-BUG ratio 3.5 to 5. A secondary parameter section 10 gives no number for is held at TX-ITU nominal and marked so. A profile whose defining feature has no number (expected TX-STRAIGHT and TX-SLOPPY) is refused by name, as 7.1 refused CH-MDV, and no range is invented. The four must-tier profiles are never refused; (5) HM-REQ-050 is measured on SyntheticCq.Text at SyntheticCq.Speeds, one seed each fixed before any decode, at 15 dB in the 2500 Hz reference on CH-AWGN through unit 461's CwChannel. It is judged on ours alone through the same Measure and CwMetrics calls against HM-REQ-013's 1.00, 0 and 0, pass/fail and not a ratchet (V-08); a profile is met only if all its cases are, not met is a finding, and 7.2 ticks on the measurement; (6) nothing under src changes, no switch or vote changes, and the new cases are not added to CwSwitchTable; (7) the app line's headless dispatcher-loop loss gets one rerun, and any type lost again is run alone and must pass, named in the report.
LICENCE: ARBITER.md sections 3, 4 and 6; the launcher's instruction on criteria whose every route is recorded no; PHASE_PLAN.md step 7 line 7.2 and its independence clause, section 5, section 6 (R86 as reported, V-04, V-14, the documents win, the second decoder is faithful), section R (R77, R78, R80, R85); CW_SPEC.md sections 4, 8, 10 and 11; CW_REQUIREMENTS.md HM-REQ-013, 050 and 052, the verification rows 013 and 050, and section T row 050-053; unit 461's CwChannel and channels.md; R12, R61, R72; HM-DEC-101, HM-DEC-155, HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: Hamlet can make test signals in each kind of fist the specification says it must read - a machine keyer, a weighted keyer, a Farnsworth traffic sender and a tight hand - each proved to be keyed as the specification describes, and the owner learns, fist by fist, whether a loud clean signal reads whole
ADVANCES: step 7 criterion 2
END-ARBITER-DECISION
```
