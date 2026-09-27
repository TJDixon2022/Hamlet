# Work instruction 469 - the INT-* interference profiles are generated with the wanted station's key exact by construction, and HM-REQ-060, 061, 062 and 066 are measured against them

**Loop unit.** Step 7, criterion 7.3 (`CW_REQUIREMENTS.md` section G: HM-REQ-060, 061, 062 and
066, judged through HM-REQ-010 and HM-REQ-011).

**Why step 7 and 7.3, and not the launcher's step 2.**
- **2.4, the launcher's line, cannot flip in this unit.** Its count is 1 of 3 (unit 460 kept no
  change; unit 467's arbiter's reading, carried by 468, not re-argued). A step-2 unit with no kept
  change takes it to 2 of 3. A step-2 unit with a kept change resets it to 0. No outcome of one
  unit ticks 2.4. 2.1, 2.2, 2.3 and 2.5 are already met, so step 2 has nothing else to flip.
- **9.4 is a loop and stays ended (`ARBITER.md` section 4).** All five mechanisms 9.3 named, (A)
  to (E) and (W), were refused under R78 by units 459, 462 and 463. No route is left.
- **R86 and this move disagree. This is reported, not ruled away.** R86 reads: *"Steps 2 to 8 are
  not authorable until 9.4 has a kept change."* 9.4 has no route. `ARBITER.md` section 4, the
  launcher's "choose another criterion or another step", `PHASE_PLAN.md` section 5 ("the loop
  need never stall for want of work") and step 7's "Independent of steps 2 to 6" all point the
  other way. This unit does **not** claim R86 is overruled. DECIDED (2) carries the mismatch to
  the owner, as unit 468 did.
- **Why 7.3 among the rest.**
  - 3.4's recording is not in the tree (unit 443's finding).
  - 6.1 needs ITU-R M.1677-1 vendored, and nothing is fetched.
  - 5.1's widening is recorded refused (unit 446).
  - **7.3 is the last generator step.** Section T reads *"060–062, 066 | none: needs INT-* in
    generator"*. Section G is must-tier throughout: a second station, a tuning carrier and a
    pileup are what the operator actually hears on 40 m. 7.4 waits on 7.3. Unit 461 built the
    channel layer (7.1) and unit 468 built the sender layer (7.2), so the layers this one stands
    on exist.

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
- Scripts go in `.run-unit\unit469-<name>.sh` and are run with `sh`. Unit 468's
  `.run-unit\unit468-*.sh` helpers may be copied and renamed.
- Run `tools/status.sh` from the repository root only. Unit 468 ran it once from the fixtures
  folder and it wrote a stray file there.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 7 criterion 3`. The launcher reads the digits after "criterion", so this
  means plan line 7.3.
- WHY cites the plan.

**R85. A CW question is answered from the documents, never raised to the owner.** A profile's
offset, level, speed or text, which station is "wanted", and the veto margin are all such
questions. DECIDED (4) to (6) answer the ones foreseen. If the tree forces a reading this
instruction did not foresee, record it in one line of section 4 and carry on. **Do not hand the
question back.**

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
| 7 | 3 of 5 (7.1, 7.2 and 7.5 met; 7.3 and 7.4 open) |
| 8 | 0 of 6 |
| 9 | 7 of 8 (9.4 open, and every mechanism 9.3 named is refused) |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The test fixtures generate every INT-* interference profile
            CW_SPEC.md section 9.3 names - INT-ADJ, INT-COCHAN,
            INT-CARRIER and INT-PILEUP - over the wanted station's
            TX-ITU keying on CH-AWGN, the wanted key exact by
            construction, each proved on its own rendered audio to be
            what its definition states; and HM-REQ-060, 061, 062 and
            066 are measured - met or not, per requirement - with the
            decoder unchanged.
ADVANCES:   step 7 criterion 3
```

**Read these first. They win over this instruction:**
- `CW_SPEC.md` section 9.3, the impairment and interference vocabulary; section 4, what a
  condition is; section 8, the reference bandwidth; section 11, the metrics;
- `CW_REQUIREMENTS.md`:
  - HM-REQ-008, 010, 011, and 060 to 066;
  - the verification table's rows 008, 010, 011, 060, 061, 062, 065 and 066;
  - section T's row *"060–062, 066"*;
- `docs/phase-requirements/channels.md` and `senders.md`, with `CwChannel.cs`, `CwSender.cs`,
  `TheChannelProfilesAreWhatTheySayTests` and `TheSenderProfilesAreWhatTheySayTests`, which units
  461 and 468 built;
- the two-signal fixtures already in `tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/`:
  `CwTwoInOnePassband.cs`, `CwTwoStationTests.cs` and `TheTwoStationTable.cs`;
- unit 448's HM-REQ-091 and 102 cold-start test beside a louder unkeyed carrier, and unit 447's
  pitch instrument.

**The requirements, verbatim:**

> **HM-REQ-060:** "With INT-ADJ(100 Hz, 0 dB, any speed) present, the decoder shall hold the
> wanted station and meet HM-REQ-010 and HM-REQ-011 on it."
> Verification row 060: *"T | INT-ADJ(100, 0, 25) | 010, 011 on wanted | held | as 010 |
> synthetic"*.
>
> **HM-REQ-061:** "With INT-ADJ(50 Hz, −6 dB, any speed) present, the decoder shall hold the
> wanted station and meet HM-REQ-010 and HM-REQ-011 on it."
> Verification row 061: *"T | INT-ADJ(50, −6, 25) | 010, 011 on wanted | held | as 010 |
> synthetic"*.
>
> **HM-REQ-062:** "With INT-CARRIER at any offset and any level present, the decoder shall not
> select the carrier over a keyed station."
> Verification row 062: *"T | INT-CARRIER at 0, +10, +20 dB, offsets 50–500 Hz | station
> selected | keyed station | — | synthetic | HM-DEC-095"*.
>
> **HM-REQ-066:** "When a second keyed signal is within the veto margin of the tracked one, the
> decoder shall report it as a competing station." Rationale: "`competing: none found` beside a
> station 2.4 dB away is a false report."
> Verification row 066: *"T | second signal within veto margin | competing reported | yes | — |
> synthetic | DEV_ANALYSIS 2.4 dB case"*.
>
> **HM-REQ-010:** MET-CER-SURE below 1 %. **HM-REQ-011:** MET-INVENTED at 0. Dim and placeholder
> are excluded from both.

**7.3 is met when all of these are true:**
1. The generator produces each of INT-ADJ, INT-COCHAN, INT-CARRIER and INT-PILEUP, with its
   parameters, as a second layer mixed over the wanted station before the channel noise. The
   wanted station's key is exact by construction. An interferer's own text is carried in the
   sidecar and is never scored as the wanted key.
2. Each profile is proved on its own rendered audio, and not from the recipe, to be what section
   9.3 says: offset, level relative to the wanted, keyed against steady, and the station count.
   The proof is watched failing first.
3. The recipe for every case is written down so that another unit can rebuild it.
4. HM-REQ-060, 061, 062 and 066 are each measured on their verification rows' conditions, and the
   report states met or not met per requirement.

**Not met is a finding, not a failure of this unit.** 7.3 asks for the measurement.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `e058da0c` or a runner commit on top of it. The version is 1.13.155.
- **`PHASE_PLAN.md`**, in both copies:
  - shows 7.1, 7.2 and 7.5 ticked, and 7.3 and 7.4 open;
  - shows 9.4 as the only open line of step 9;
  - carries R86's line in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **Units 461 and 468's layers are present:** `CwChannel` with `CH-AWGN`, `CwSender` with
  `TX-ITU`, `channels.md` and `senders.md`.
- **The port** is untouched: `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints
  nothing.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-7-output-4.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
  `SESSION.lock` is the launcher's. Do not stage it.
- **Logged and not this unit's:**
  - the reload's `RULES_AT` disagreement (HM-DEC-165 against CPS-DEC-0183);
  - `outcome-read`'s step titles for steps 2, 3 and 8, which differ from `PHASE_PLAN.md`'s.

**Entry figures, 468's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278, or 277 of 278 with the lost type passing alone under DECIDED (8).
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:** MET-CER-SURE 33 of 436, coverage 403 over 473,
  MET-WBE 37 over 113.
- **Synthetic set, 12 cases, exact key:** MET-CER-SURE 14 of 173, coverage 159 over 252, MET-WBE
  44 over 84.
- **`TheChannelProfilesAreWhatTheySayTests`:** 71 of 71.
- **`TheSenderProfilesAreWhatTheySayTests`:** 16 of 16.
- **`TheArbitrationEarnsItsPlaceTests`:** both parts green.

If any entry figure differs, report it and use the measured one.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31. It is on neither carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under
  DECIDED (8).
- The parity run rewriting the decode-time rows of `parity.md`. Keep a copy under `.run-unit\` and
  restore the committed file, as units 462 to 468 did.
- **Any of HM-REQ-060, 061, 062 or 066 not met.** Report it. It is the measurement, not a
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
- **R77:** every new test names the requirement it proves. The profile proof names section 9.3's
  ids and HM-REQ-060, 061, 062 and 066. The measurement fact names each requirement it measures.
- **R78:** it is not engaged, because nothing under `src` changes.
- **Section 6:** *"Where this plan and the two documents differ, the documents win, and the
  difference is a finding in the report."*
- **Section 6:** *"No fixture is admitted by lowering a gate (V-04), and no separation limit,
  confirmation rule or plausibility bound is loosened to pass a fixture (V-14)."*
- **Section 6:** *"A requirement with a TBD threshold (HM-REQ-032, 065, 092) is measured and the
  measurement reported; the threshold is the owner's and its absence never halts a unit."*
- **R85:** a CW question is answered from the documents, recorded as the arbiter's reading and
  overrulable, and never parked for the owner.

**Standing rulings this unit leans on:**
- **R12.** A session rewrites its own tests.
- **R61.** A key is inferred unless it was transcribed. Every key here is exact by construction.
- **R72.** No word, dictionary or callsign prior, in any form. The texts are keys for scoring
  only, never a decoder input.
- **R75 and R76.** The tone tracker is open to change, but not in this unit.
- **R80.** No traceability, test inventory or decision-log work.
- **V-04, V-06, V-08, V-13, V-14.**
- **HM-REQ-122 and 129:** the second decoder is faithful, not improved. Not one line under
  `Cw\Second\` changes.
- **The four `RESOLVED:` answers in `PARKED.md`**, including the eleven transmit files.

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode.
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched, not called and not reused.** The interferers are audio
  synthesis in the test fixtures. They are not a keyer, and nothing reaches the radio.
- `CLAUDE.md` §12.5: a fixture built from the same misunderstanding as the code proves nothing.
  The profile proof measures the rendered audio, never the recipe's own numbers read back. It
  shares no code with `CwToneTracker`, `CwToneSurvey` or any decoder front end. Unit 447's
  instrument may be used, because it was built to share none.
- HM-DEC-095, HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task;
- when the profile list is fixed, naming each profile's parameters;
- when the profile proof comes back red, naming what failed;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 469 - STEP 7` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 469 with `CURRENT_STEP: 7`, in both copies.
3. Bump the patch by one from what the tree carries. It should be 1.13.155, going to 1.13.156.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `TheChannelProfilesAreWhatTheySayTests`;
   - `TheSenderProfilesAreWhatTheySayTests`;
   - `TheArbitrationEarnsItsPlaceTests`, both parts.
6. **The trace, before anything is built.** Read `CwChannel`, `CwSender`, `CwTwoInOnePassband`,
   `TheTwoStationTable` and unit 448's carrier test. Print to `.run-unit\unit469-trace.txt`:
   - what the tree can already mix: a second keyed tone, a steady tone, their offset, their level,
     and their start time;
   - where the level is set, and whether it is relative to the wanted tone's key-down power;
   - how a mix passes through `CwChannel`'s CH-AWGN, and whether the SNR in the 2500 Hz reference
     is set on the wanted station alone;
   - **what the decoder reports as the tracked pitch, and where "competing" is decided and
     reported** (`CwProbabilisticDecoder`, the decode report, and `MainWindowViewModel`'s
     `none found` sentence near lines 12054 and 12601). HM-REQ-066 is measured on whatever the
     tree reports. If the engine reports nothing and only the app composes a sentence, say so;
   - the detector bandwidth in force, which INT-COCHAN is defined against;
   - what is missing.

   Nothing under `src` or `tests` changes in this step.

### Task 1 - the INT-* profiles, generated and proved

**Nothing under `src` changes.** Everything is in
`tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/`.

1. **Fix the profile list under DECIDED (4)** before writing code. Print it in
   `docs/phase-requirements/interference.md`, one row per section 9.3 `INT-*` id, with these
   columns:
   - the id, and section 9.3's definition quoted;
   - each parameter with its value, and where the value comes from: a verification row, a section
     9.3 definition, `TX-ITU nominal`, or this instruction's DECIDED;
   - the requirement it serves.
2. **Build `CwInterference`** (or the tree's own name for it) beside `CwChannel` and `CwSender`.
   Given a wanted case and an INT-* profile with its parameters and a seed, it:
   - mixes the interferer or interferers over the wanted station's keyed tone, at the stated
     offset and at the stated level relative to the wanted station's key-down power;
   - passes the mix through `CwChannel`'s CH-AWGN, with the SNR set on the wanted station in the
     2500 Hz reference, over the shaped noise band and never digital silence (V-06);
   - writes a sidecar with the wanted key, which is the wanted text itself (exact, R61, V-13),
     plus each interferer's pitch, level, speed, text, start and seed.

   Reuse or extend the two-signal code the trace found rather than writing a third mixer, where it
   can carry the parameters.
3. **Write `TheInterferenceProfilesAreWhatTheySayTests`**, naming section 9.3's ids and HM-REQ-060,
   061, 062 and 066. For each produced profile, it measures from the rendered mix before noise,
   never from the recipe:
   - each tone's frequency, and its offset from the wanted, within 2 Hz;
   - each tone's key-down power relative to the wanted, within 0.5 dB;
   - keyed or steady: the carrier's envelope holds within 1 dB for the whole case, and an
     adjacent station's envelope keys on and off;
   - for INT-COCHAN, the offset lies inside the detector bandwidth the trace named;
   - for INT-PILEUP, three or more keyed stations inside the passband.

   It asserts each against the profile's parameters.
4. **Watch it fail first.** Run the proof against a mixer that ignores its profile: it adds the
   interferer at the wanted's own pitch and level, keyed like the wanted. It must be red on every
   profile. Save the printout to `.run-unit\unit469-int-red.txt`. Then run it against
   `CwInterference`, green, and save that to `.run-unit\unit469-int-green.txt`.
5. **Write the recipe** into `interference.md`: the construction, the seeds, the texts, and the
   start times. Write the section *"What these fixtures do not prove"* (§12.5, V-04): an
   interferer here is a clean TX-ITU tone or a steady tone at one level, with no fading, drift or
   chirp. A result on them is a result on section 9.3's words, not on a 40 m evening.
6. Run `TheChannelProfilesAreWhatTheySayTests`, `TheSenderProfilesAreWhatTheySayTests`, the three
   floor tests and both carry-forward lines. **Commit** `CwInterference`, the proof and
   `interference.md` in one commit.

### Task 2 - HM-REQ-060, 061, 062 and 066 measured

1. **Generate the cases** under DECIDED (5) and (6), with every seed fixed before any decode:
   - **HM-REQ-060:** INT-ADJ(±100 Hz, 0 dB, 25 WPM) over the wanted at each of
     `SyntheticCq.Speeds`. That is 6 cases.
   - **HM-REQ-061:** INT-ADJ(±50 Hz, −6 dB, 25 WPM), the same way. That is 6 cases.
   - **HM-REQ-062:** INT-CARRIER at offsets +50, +100, +200 and +500 Hz, at 0, +10 and +20 dB,
     over the wanted at 18 WPM. That is 12 cases.
   - **HM-REQ-066:** a second keyed station, TX-ITU at 25 WPM, at +100 Hz, at 0, −2.4 and −6 dB,
     over the wanted at 18 WPM. That is 3 cases.
   - **Control:** the wanted alone at each of `SyntheticCq.Speeds`, the same seeds and SNR, so
     every figure has its no-interference twin beside it.
2. **Write `TheInterferenceIsMeasuredFact`**, naming HM-REQ-060, 061, 062, 066, 010 and 011. It
   asserts nothing. It drives each case through the same harness and the same `Measure` and
   `CwMetrics` calls `TheRequirementsAreMeasuredTests` uses. For each case, it prints on ours
   alone:
   - the wanted key, what ours read, and the control's reading beside it;
   - MET-CER-SURE, MET-INVENTED, sure-and-right coverage and MET-WBE, against the wanted key;
   - the tracked pitch at the end of each wanted word, beside the wanted pitch and the
     interferer's. It is **held** only if every one lies within 25 Hz of the wanted pitch;
   - for 062, **station selected**: the keyed station if every tracked pitch after the first
     sure character lies within 25 Hz of the wanted pitch, else the carrier;
   - for 066, what the tree reports as competing, quoted as it appears;
   - **met or not met** per case. A requirement is met only if all its cases are.

   Then print the live emitted transcript under `CwSwitchTable` as it stands, and the port alone,
   beside ours. **This is the drop candidate.**
3. **Write the table** into `interference.md` as a measurement section. Every figure carries its
   key's kind, `synthetic, exact` (V-13).
4. **Write `metrics.md`'s running line** for HM-REQ-060, 061, 062 and 066, with the date and this
   unit's number.
5. Run the three floor tests and both carry-forward lines. **Commit** the fact and the two
   documents. Then **tick 7.3** in both copies of `PHASE_PLAN.md`, in a follow-on commit, when
   section 2's four conditions hold. If any fails, leave 7.3 open and say which.

**Drop candidate:** the emitted-output and port-alone columns in task 2's step 2, then the
negative-offset half of the 060 and 061 cases. These are never shed: all four profiles produced
and proved red then green, the recipe, and a verdict on ours for each of 060, 061, 062 and 066 on
at least its verification row's own condition.

### Task 3 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `TheChannelProfilesAreWhatTheySayTests`;
- `TheSenderProfilesAreWhatTheySayTests`;
- `TheInterferenceProfilesAreWhatTheySayTests`;
- `TheArbitrationEarnsItsPlaceTests`, both parts;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff e058da0c -- src`, which prints nothing;
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with its results on the five at its exit: build, both
  carry-forward lines, and the three floor tests.

**Ticks:** 7.3 only, as task 2 says. **Do not tick 7.4.** It asks for "a real capture ...
reported by the sender profile the spec names or as `not stated`", which this unit does not do.
Section 4 states what 7.4 still lacks once 7.3 is in. Do not tick 9.4, or any line of steps 2 to
6 or 8.

---

## 7. Parked - do not touch, do not raise

- **9.4 and every mechanism 9.3 named, (A) to (E) and (W), in any form.** The loop is ended.
- **Every switch in `CwSwitchTable`, the vote table, both confidences, calibration and
  HM-REQ-127's margin.** The new cases are not added as switch rows. Where the emitted output
  loses on one, that is a line in section 4, and nothing is switched.
- **Any change to `CwToneTracker`, the survey or the competing-station logic.** Where 062 or 066
  is not met, that is a finding for steps 4 and 2. Nothing is repaired here.
- **HM-REQ-008 (INT-COCHAN), 063 (INT-PILEUP), 064 and 065.** Those profiles are produced and
  proved, but not measured. HM-REQ-051's sweep and HM-REQ-054 are also parked.
- **TX-STRAIGHT and TX-SLOPPY**, which unit 468 refused, and the 7.2 judge's remark about them.
- **Unit 468's section 4 items 2 to 9.**
- **The four questions `PARKED.md` records, and their `RESOLVED:` answers.**
- **Step 2's decoder work, and 2.4's count.** The count stays 1 of 3.
- **The two `PHASE_STATUS.md` copies differing beyond the unit's fields.**

## 8. Do not

- **Do not change any line under `src`.** This unit is fixtures, a proof, a fact and two
  documents.
- **Do not invent a parameter section 9.3 and the verification rows do not give**, beyond the
  readings DECIDED (4) to (6) make (V-04, §12.5).
- **Do not tune an offset, a level, a seed, a text, a start time or a speed to make a case pass**
  (V-14). The seeds are fixed before any decode is run and are printed.
- **Do not read either key into the decoder** (R72), and never score an interferer's text as the
  wanted key.
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
- task 1's interference layer, proof and `interference.md`;
- task 2's fact, `interference.md` and `metrics.md`;
- the 7.3 tick, if earned;
- task 3.

**Messages** take the form `unit469 task N: <what> (7.3)`. Task 1's message names the four
profiles and their parameters. Task 2's names the verdict on each of HM-REQ-060, 061, 062 and 066.

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
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 <n> of 5, 8 0 of 6, 9 7 of 8; 9.4 ended as a loop, the launcher's
   2.4 cannot flip in one unit at 1 of 3, and this unit worked step 7
   against R86's words, reported as a mismatch for the owner.
B. Step 7, criterion 7.3 (HM-REQ-060, 061, 062, 066): profiles produced
   INT-ADJ, INT-COCHAN, INT-CARRIER, INT-PILEUP <all|list>; proof
   watched red against a nominal mixer, then green <yes|no>; ours:
   060 <met|not>, 061 <met|not>, 062 <met|not>, 066 <met|not>, with
   the worst case's MET-CER-SURE, MET-INVENTED and held pitch; src
   unchanged <yes|no>; 7.3 <ticked|open>; 7.4 still lacks <what>.
C. The findings weighed against A and B: how many items section 4
   raises, whether any is in the way of 7.3 or 7.4, and the R86
   mismatch stated in one line.
```

```
UNIT:       469 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-060 <met|not>, 061 <met|not>, 062 <met|not>, 066 <met|not> (synthetic, exact, CH-AWGN 15 dB on the wanted); worst MET-CER-SURE <x/y>, MET-INVENTED <x/y>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the measurement table:** per requirement and per case, the interferer,
the wanted key, what ours read, the control's reading, the held or selected verdict, and
MET-CER-SURE and MET-INVENTED, with met or not. Then give:
1. the profile list from `interference.md`;
2. the proof's red and green printouts;
3. what the tree reports as competing, on each 066 case, quoted;
4. the commit table, with the five results at each commit.

**Section 2, one paragraph, in the owner's words.** Nothing on the screen changes. Say that
Hamlet can now make test signals with a second station beside the one being read, a station
tuning up on a steady carrier, and a pileup, and that each was checked to be what the
specification describes. Then say, per case, whether Hamlet stayed on the right station and read
it cleanly. Where it did not, quote what it read against what was sent.

**Section 4 must say, as a plain reading and not a ruling request:**
- **The R86 mismatch.** This unit worked step 7 while R86's words bar steps 2 to 8 until 9.4 has a
  kept change. 9.4 has no route left, and the launcher's 2.4 cannot flip in one unit. If the owner
  holds R86, this unit's work is fixtures and a measurement, and nothing the operator reads has
  changed.
- **Every parameter DECIDED (4) to (6) took as the arbiter's reading, overrulable**, the veto
  margin among them.
- **Where a requirement is not met:** the case, what was read, and whether the pitch was lost or
  the interferer's letters were printed as sure. This is recorded as a finding for the decoder
  steps and not chased here.
- **What 7.4 still lacks** once 7.3 is in.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: build INT-ADJ INT-COCHAN INT-CARRIER INT-PILEUP interference layers mixed over the wanted TX-ITU station in the test fixtures before CH-AWGN noise, prove offset level keyed-or-steady and station count on the rendered mix watched red, measure HM-REQ-060 061 062 066 on ours with held pitch and competing report, decoder unchanged
MOVE: work around
WHY: PHASE_PLAN.md step 7 line 7.3 asks that the generator produce the INT-* profiles and that HM-REQ-060, 061, 062 and 066 be measured against them, and section T records those four as "none: needs INT-* in generator"; the launcher's step 2 has only 2.4 open, a count at 1 of 3 that no single unit can tick, and 9.4 is an ended loop with every mechanism 9.3 named refused. The loop test finds no entry for this approach.
STATE: partial
DECIDED: author's, overrulable - (1) 9.4 stays judged a loop under ARBITER.md section 4, with (A) to (E) and (W) refused by units 459, 462 and 463, and it is not re-attempted; (2) step 7 is worked although R86 (PHASE_PLAN.md section 6) reads that steps 2 to 8 are not authorable until 9.4 has a kept change. This is a mismatch reported for the owner, not a claim that R86 is overruled, since R86 waits on a line with no route while ARBITER.md section 4, the launcher and PHASE_PLAN.md section 5 direct the move; (3) 7.3 is chosen over the launcher's 2.4 because 2.4's count is 1 of 3 (unit 467's arbiter's reading, not re-argued), so one unit takes it at most to 2 of 3 and a kept change resets it, and 2.1, 2.2, 2.3 and 2.5 are met; over 3.4, 6.1 and 5.1 because 3.4's recording is absent, 6.1 needs M.1677-1 vendored and nothing is fetched, and 5.1's widening is recorded refused; (4) under R85, the INT-* profiles take section 9.3 and the verification rows as written: INT-ADJ(df, dB, WPM) is a TX-ITU station at its own text and speed; INT-CARRIER(df, dB) is a steady unkeyed tone present from the first sample; INT-COCHAN is a second TX-ITU station at 0 dB, offset inside the detector bandwidth the trace names; INT-PILEUP is three TX-ITU stations at equal level, at distinct speeds and pitches inside the passband. Every level is relative to the wanted station's key-down power, and the SNR in the 2500 Hz reference is set on the wanted station alone, on CH-AWGN at 15 dB; (5) the wanted station is TX-ITU at SyntheticCq.Text and is keyed alone for its first word, and an adjacent or second station starts at the wanted's second word and runs to the end with its own text, so "wanted" is the station present first. "Held" is every tracked pitch at a wanted word's end within 25 Hz of the wanted pitch, together with HM-REQ-010 and 011 on the wanted key. For 062, the carrier is present from cold and "keyed station selected" is every tracked pitch after the first sure character within 25 Hz of the wanted; (6) HM-REQ-066's veto margin is read as HM-REQ-065's recommended 6 dB, which that row names as HM-REQ-008's margin, and 066 is measured at 0, -2.4 (the DEV_ANALYSIS case) and -6 dB on whatever the tree reports as competing; (7) nothing under src changes, no switch or vote changes, and no new case is added to CwSwitchTable; (8) the app line's headless dispatcher-loop loss gets one rerun, and any type lost again is run alone and must pass, named in the report.
LICENCE: ARBITER.md sections 3, 4 and 6; the launcher's instruction on criteria whose every route is recorded no; PHASE_PLAN.md step 7 line 7.3 and its independence clause, section 5, section 6 (R86 as reported, V-04, V-14, TBD thresholds measured, the documents win, the second decoder is faithful), section R (R77, R78, R80, R85); CW_SPEC.md sections 4, 8, 9.3 and 11; CW_REQUIREMENTS.md HM-REQ-008, 010, 011, 060 to 066, the verification rows 010, 011, 060, 061, 062, 065 and 066, and section T row 060-062, 066; units 461 and 468's CwChannel, CwSender, channels.md and senders.md; R12, R61, R72, R75, R76; HM-DEC-095, HM-DEC-155, HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: Hamlet can make test signals with a second station beside the one being read, a station tuning up on a steady carrier, and a pileup, each proved to be what the specification describes, and the owner learns, case by case, whether Hamlet stays on the right station and reads it cleanly when the band is busy
ADVANCES: step 7 criterion 3
END-ARBITER-DECISION
```
