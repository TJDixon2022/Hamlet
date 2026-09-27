# Work instruction 464 - a number on every letter: both decoders give each character a confidence p, and whether letters printed at p are right p of the time is measured per condition on the keyed corpus

**Loop unit.** Step 9, criterion 9.5 (`CW_REQUIREMENTS.md` section M, HM-REQ-124; section B's
HM-REQ-014 and `CW_SPEC.md`'s MET-CAL beside it).

**Why step 9, and why 9.5, and not the launcher's step 2.**
- **R86, `PHASE_PLAN.md` section 6:** *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."* The
  owner committed this at `6de324b1`, and no later ruling has lifted it. Step 2's only open line,
  2.4, counts step-2 units with no kept change. R86 bars step-2 units, so 2.4 cannot move. This
  unit is not a step-2 unit, and 2.4's count stays at 1 of 3.
- **9.4 has no route left that the plan's words allow.** 9.4 asks for "one technique 9.3
  named". 9.3 named five mechanisms, and all five are now recorded as refused under R78:
  - (D) pair speed tracking, by unit 459;
  - (B) the spike rule, (C) the two-dot class edge and (W)/(E) the gap edges, by unit 462;
  - (A) detection, in both forms, by unit 463.

  A sixth attempt at 9.4 would restate one of them. The loop test forbids that, and V-14 forbids
  re-edging a refused form.
- **9.5 is step 9's, so R86 permits it. It is the next line section M orders.** HM-REQ-124 comes
  before any vote. The second decoder "votes only where it is calibrated", and 9.6 and 9.7 cannot
  be built until each decoder carries a confidence that means something. No unit has attempted
  9.5. The loop test finds no entry for this approach.
- **This is not unit 442's approach.** Unit 442 turned ours' rival margin into a *class* change,
  printing close calls dim, and was refused at 2.2. **This unit changes no class and no letter.**
  It attaches a number beside each character and measures it. Every text either decoder prints
  stays byte-identical.

Four working tasks, plus the exit round. Drop from the back.

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
- Python cannot run here. The fit and the measure are written in C#, in the test project.
- A multi-line commit uses `-m` more than once.
- Scripts go in `.run-unit\unit464-<name>.sh` and are run with `sh`.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 9 criterion 5`. The launcher reads the digits after "criterion", so this
  means plan line 9.5.
- WHY cites the plan.

**R85. A CW question is answered from the documents and fldigi's source, never raised to the
owner.** That covers:
- what "right" means for a dim character;
- how a bin is drawn;
- how few characters a bin can hold and still count;
- what in fldigi's receiver a confidence can honestly be read from.

Record your reading in one line of section 4, and carry on.

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
| 9 | 3 of 8 (9.4 open with every mechanism 9.3 named refused) |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  every character either decoder emits carries a confidence p
            between 0 and 1, and over the keyed corpus, per condition, the
            share of characters at p that are right is measured against p,
            so the conditions where each decoder is calibrated within 5
            points are named - with no letter and no class changed.
ADVANCES:   step 9 criterion 5
```

**Read these first. They win over this instruction:**
- `CW_REQUIREMENTS.md` section M, HM-REQ-124 above all, and HM-REQ-125 to 128 for what the
  confidence will be used for;
- section B, HM-REQ-013 and HM-REQ-014, and the verification table's rows for 014 and 124;
- `CW_SPEC.md`'s MET-CAL: *"Reliability table with three bins (sure / dim / placeholder): observed
  accuracy per bin against its stated rate."* Also its definitions of the other metrics;
- `docs/phase-requirements/parity.md` section 1, for how the port's unclassed output was mapped,
  and section 3, for the conditions;
- `CwCharacter.MarginLlr` and `CwProbabilisticDecoder.RivalMargin`, for what ours already measures
  per character;
- `FldigiCwEmission`, `FldigiCwKeyEvent` and `FldigiCwDecisionRow` in
  `src\Hamlet.RadioEngine\Cw\Second\FldigiCwDecoder.cs`, for what the port already exposes. It
  exposes `agc_peak`, `noise_floor`, `sig_avg`, `two_dots`, each element's length and the
  detector value, all without being changed;
- `.run-unit\fldigi\src\cw_rtty\cw.cxx` at `61b97f41`, for what each of those quantities is.

**The requirement this unit serves, HM-REQ-124, verbatim:**

> "Each decoder shall attach a calibrated confidence to every character it emits: over the keyed
> corpus, per condition, characters emitted at confidence p shall be right within 5 points of p. A
> decoder whose confidence is not calibrated on a condition does not vote on that condition; its
> output is advisory there."

**9.5 is met when all of these are true:**
1. Both decoders attach a p to every character.
2. Calibration is measured per condition on the keyed corpus.
3. The conditions where each decoder is calibrated are named.
4. The report says what the port's p is derived from.

**Being calibrated on every condition is not required to tick 9.5.** The criterion asks that the
calibrated conditions be *named*. A condition where a decoder is not calibrated is a finding, and
under HM-REQ-124 that decoder is advisory there.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `99c90b69` or a runner commit on top of it.
- **`PHASE_PLAN.md`**, in both copies:
  - shows 9.1, 9.2 and 9.3 ticked, and 9.4 to 9.8 open;
  - carries R86's line in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **The port** is at `src\Hamlet.RadioEngine\Cw\Second\`, and `git diff 19109b51 --
  src/Hamlet.RadioEngine/Cw/Second/` prints nothing.
- **`CwCharacter`** carries `Confidence` (the `CwConfidence` class) and `MarginLlr`, and no
  numeric probability.
- **Unit 463's saves** are in `.run-unit\`:
  - `unit463-A1.patch`, `unit463-A2.patch` and their `.txt` files;
  - `unit463-text-before.txt`, `unit463-port-before.txt` and `unit463-v11-before.txt`.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-2-output-14.md` and `.run-unit\watched.rc`. Unit 463
    reported `watched.rc` missing, and it is now present.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
- **Logged and not this unit's:**
  - the reload's `RULES_AT` disagreement (HM-DEC-165 against CPS-DEC-0183);
  - `outcome-read`'s step titles for steps 2, 3 and 8, which differ from `PHASE_PLAN.md`'s.

**Entry figures, 463's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278.
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:** MET-CER-SURE 33 of 436, MET-INVENTED 33 over
  473, coverage 403 over 473, MET-WBE 37 over 113.
- **Synthetic set, 12 cases, exact key:** MET-CER-SURE 14 of 173, MET-INVENTED 14 over 252,
  coverage 159 over 252, MET-WBE 44 over 84.
- **The port, per parity.md:**
  - real: 62 wrong of 239 sure, coverage 177;
  - synthetic: 34 of 168, coverage 134.
- **Decode time**, ours over 690 s of real audio: about 52 s.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31. It is on neither carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under DECIDED (8).
- The parity run rewriting the decode-time rows of `parity.md`. Keep a copy under `.run-unit\` and
  restore the committed file, as units 462 and 463 did.

---

## 4. Rulings in force - transcribed, do not re-argue

**The ordering:**
- **R86** (`PHASE_PLAN.md` section 6): *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."*
- **R84** (`PHASE_PLAN.md` section R): *"The order is fixed by the requirements: the second
  decoder is scored beside ours on every recording before it votes (123); it votes only where its
  confidence is calibrated against the keys (124); ..."*
- **HM-REQ-122 and 129, and section 6:** *"The second decoder is faithful, not improved. A
  technique goes into ours, judged under R78; the second decoder stays as ported."*
  - The port's confidence is therefore computed **outside** `Cw\Second\`, from what the port
    already exposes.
  - Not one line under `Cw\Second\` changes, not even to expose a field. If a quantity the
    confidence needs is not public, the confidence goes without it, and the report says so.

**R78, the keep rule, applies to any change that moves a letter or a class. This unit makes none.**
The confidence is a number beside each character.
- **Every text either decoder prints stays byte-identical** to task 0's save, class for class.
- The four metrics stay exactly at their entry figures.
- A confidence that moved a letter or a class would be a defect in this unit, not a result.

**Standing rulings this unit leans on:**
- **R77.** A new CW test names the requirement it serves, here HM-REQ-124.
- **R80.** No traceability, test inventory or decision-log work.
- **R72.** No word, dictionary or callsign prior (HM-REQ-004, HM-DEC-175). No feature of the
  confidence may know letters' frequencies or which letters make words.
- **R61.** A key is inferred unless it was transcribed. The real set's calibration is stated
  against inferred keys, every time.
- **V-11 and V-13.** One scorer, one set of keys, and nothing fitted to the corpus is measured on
  the characters it was fitted on (DECIDED (3)).
- **V-14.** The features and the map's form are fixed at task 1, before any calibration number is
  seen, and never changed after.
- **R85.** As section 1.

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode.
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched, not called and not reused.**
- `CLAUDE.md` §12.5: a fixture built from the same misunderstanding as the code proves nothing.
  This is why calibration is measured held-out.
- HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task, naming what it will measure or build;
- when task 1 fixes the features and the map;
- at each decoder's calibration verdict, per condition;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 464 - STEP 9` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 464 with `CURRENT_STEP: 9`, in both copies.
3. Bump the patch from 1.13.150 to 1.13.151.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `BothDecodersAreScoredAlikeTests`;
   - the port's own tests.
6. Save both decoders' texts, each character with its class, the way unit 463 did:
   - ours to `.run-unit\unit464-text-before.txt`;
   - the port's to `.run-unit\unit464-port-before.txt`.

   Every byte-identical check in this unit is made against these two.

### Task 1 - the trace: what each decoder already knows about each character, and the map fixed

This task reads and prints. **Nothing under `src` changes.** Write a fact that asserts nothing:
`WhatEachDecoderKnowsAboutEachCharacterFact`, serving HM-REQ-124. It prints to
`.run-unit\unit464-features.txt`.

1. **One row per emitted character**, for both decoders, on all 23 real recordings and all 12
   synthetic cases. Each row carries:
   - the recording, its condition as `parity.md` section 3 states it, and the key's kind;
   - what was emitted, and the class it carries: sure or dim for ours, sure for the port per
     `parity.md` section 1;
   - the scorer's verdict against the key: **right**, **wrong** (substituted) or **added**.
     Placeholders are listed and marked unscored, as MET-CAL's placeholder bin is.
   - **Ours:** `MarginLlr`, and `MarginShareForRecord` beside it.
   - **The port**, read only from `Emissions`, `KeyEvents` and, with `TraceDecisions` on,
     `Decisions`, for the key events that make up that character:
     - the **level margin**, `sig_avg` over `noise_floor` in dB at the character's last up event;
     - the **timing margin**, the smallest distance of any of its elements' lengths from the
       `two_dots` split, in units of `two_dots / 2`, which is fldigi's own dit;
     - the element count.

     Name the `cw.cxx` line each quantity comes from at `61b97f41`. If one of them cannot be
     read without changing the port, say so and leave it out. Do not change the port.
2. **The separation.** For each feature, per decoder, print right against wrong-or-added:
   - by quintile of the feature: count right, count wrong, count added;
   - the right share per quintile.

   Also count the characters whose feature is NaN, per decoder, and say why each kind is NaN.
3. **Fix the confidence, once, before any calibration number is seen** (DECIDED (2), V-14):
   - **The map for both decoders is a logistic** p = 1 / (1 + e^-(a + b.x)) over the features
     named below, fitted by maximum likelihood to right = 1 and wrong-or-added = 0.
   - **Ours takes one feature, `MarginLlr`.** A NaN margin takes the rule task 1 states from the
     separation print, fixed here: either a constant p fitted on the NaN characters alone, or
     another feature named here.
   - **The port takes two features, the level margin and the timing margin**, or whichever of
     the two is readable. If neither is readable, the port's p is one constant fitted by
     recording held-out, and the report says so plainly.

     Write down now what the port's p is derived from. 9.5 asks the report to say it.
   - **Dim characters of ours** are scored like sure ones. p is a probability of being right, not
     a class.
   - **The pool the fit is taken over:** the real and synthetic sets together, one map per
     decoder, not one per condition. Calibration is judged per condition. A map per condition
     would fit each condition to itself.

**Commit the fact and its printout.**

### Task 2 - the confidence attached (9.5's first half; drop candidate: the second test case)

1. **Ours.** Add a numeric confidence to each character our decoder emits: `CwCharacter.Probability`,
   between 0 and 1, set by the stream where `MarginLlr` is set. Its remarks name HM-REQ-124 and
   the map.
   - The constants are those task 3's fit on the whole pool gives. **Build the property first,
     with the constants task 3 writes back.** Task 2 and task 3 may be one working session, with
     task 2's commit carrying the final constants.
   - `CwConfidence`, `MarginLlr`, the lattice, the class rule and every threshold are untouched.
2. **The port.** Add `FldigiConfidence` under `src\Hamlet.RadioEngine\Cw\` (**never under
   `Second\`**).
   - It takes the port's `Emissions` and `KeyEvents` and returns one p per emitted character,
     by the map task 1 fixed.
   - It is wired to nothing the operator sees (HM-REQ-121). That is 9.6's.
   - A header comment says what it is derived from, names the `cw.cxx` lines, and says that the
     port itself carries no confidence.
3. **A test naming HM-REQ-124**, `EveryCharacterCarriesAConfidenceTests`, watched failing first at
   the parent commit. Say how it was watched.
   - **First case:** on a synthetic case of exact key, every character of ours and every
     character of the port has a p in [0, 1], and none is NaN.
   - **Second case, the drop candidate:** the calibration measure itself, on a hand-built list
     whose answer is known by construction. For example, 100 characters at p 0.9 with 90 right is
     calibrated; with 80 right it is not. Shed this case first. The first case is never shed.
4. **Byte-identical, before committing:**
   - both decoders' texts and classes against task 0's saves;
   - the four real and four synthetic metrics at their entry figures;
   - `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` printing nothing.

   Any difference is a defect. Find it before committing.
5. Run the three floor tests and both carry-forward lines. Commit the property, the adapter, the
   test and the constants in one commit.

### Task 3 - the calibration measured per condition (9.5's second half)

Write the measure in the test project, as a new type beside `CwMetrics`, not as a change to it:
`CwCalibration`. The type serves HM-REQ-124.

1. **Held-out, by recording** (DECIDED (3)).
   - For each of the 35 recordings and cases, fit the map on the other 34 and give the held-out
     one's characters their p from that fit.
   - The calibration verdict is taken on these held-out p's only.
   - Also print the in-sample figure, from the whole-pool fit that task 2 ships, beside each
     held-out one. The gap between the two is the report's measure of how much the fit learned
     the corpus.
2. **The reliability table, per decoder and per condition** (`parity.md` section 3's rows: the
   real set whole, and by sender; the synthetic set whole, and by character gap and level).
   - Use ten bins of p, each 0.1 wide.
   - For each bin, print the characters in it, the mean p, the share right, and the difference
     in points.
3. **The verdict** (DECIDED (4)). A decoder is **calibrated on a condition** when all three hold:
   - every bin holding at least 10 characters has its share right within 5 points of its mean
     p;
   - the condition's overall share right is within 5 points of its mean p;
   - at least 30 scored characters stand on the condition.

   A condition with fewer than 30 is **not measurable**, named as such, and is neither calibrated
   nor uncalibrated.
4. **Beside it, MET-CAL as `CW_SPEC.md` defines it,** for ours only: the three-bin table of sure,
   dim and placeholder, with the observed accuracy per bin, and HM-REQ-014's 70% for dim.
   **This is the drop candidate of the task.** Shed it before any part of 1 to 3.
5. **Write `docs/phase-requirements/calibration.md`:**
   - the map and its constants for each decoder, and what the port's p is derived from;
   - the held-out reliability tables per condition;
   - the in-sample figure beside each held-out one;
   - a list, per decoder, of the conditions where it is calibrated, not calibrated and not
     measurable;
   - "What this does not prove", in the manner of `parity.md` section 5. It must cover:
     - the real set's keys are inferred;
     - no condition here is a `CH-*` condition (7.4);
     - a map fitted on 35 recordings is not proven on the 36th.

   Add one line to `metrics.md` pointing at it.
6. **Tick 9.5** in both copies of `PHASE_PLAN.md`, in a follow-on commit, when:
   - both decoders carry a p on every character;
   - the held-out calibration is measured per condition;
   - the conditions are named;
   - the port's derivation is stated.

   A decoder being calibrated nowhere does not stop the tick. It is reported as the finding it
   is. Do not tick anything else.

### Task 4 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `BothDecodersAreScoredAlikeTests`;
- the port's own tests;
- `EveryCharacterCarriesAConfidenceTests`;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- both decoders' texts and classes against task 0's saves, byte-identical;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with its results on the five at its exit: build, both
  carry-forward lines, and the three floor tests.

**Ticks:** 9.5 only, as task 3 says. Do not tick 9.4, 9.6 to 9.8, or any line of steps 2 to 8.

---

## 7. Parked - do not touch, do not raise

- **9.4 and every mechanism 9.3 named, (A) to (E) and (W), in any form.** Each is refused. Their
  saved patches are evidence only.
- **Any change to a letter or a class of either decoder.** A confidence that says ours' sure
  letters are often wrong is a finding for 9.6 and 9.7, and is not acted on here.
- **The vote itself, 9.6's margin (HM-REQ-127's TBD 0.05), the sheet's per-character decoder
  record, and wiring anything to the CW tab.** These are 9.6's.
- **Unit 463's section 4 items 3 and 4:** our likelihood's Rayleigh noise scale, and the parity
  harness printing the port only over the stretch ours scored. The second of these is noted
  here because task 1's rows come from the same harness. Count the port's characters from its own
  `Emissions`, and say whether any fall outside the stretch ours scored.
- **The four questions `PARKED.md` records, and their `RESOLVED:` answers.**
- **Step 2's decoder work, and 2.4's count.** The count stays 1 of 3.
- **Steps 3 to 8,** under R86.
- **461's section 4, 459's section 4 items 3 to 5, 458's items 2 to 5, 6.5's tick, HM-REQ-084's
  `ABOVE`, and unit 455's two 6.1 findings.**
- **The two `PHASE_STATUS.md` copies differing beyond the unit's fields.**

## 8. Do not

- **Do not change any line under `src\Hamlet.RadioEngine\Cw\Second\`** (HM-REQ-122, 129). This
  includes making a field public.
- **Do not change a letter, a class, a threshold or the lattice** in our decoder. The confidence is
  added beside them.
- **Do not change `CwMetrics`, the scorer, `MorseAlphabet` or any test's key.** The measure is a
  new type.
- **Do not measure calibration on the characters the map was fitted on** and report it as the
  verdict. The verdict is held-out by recording.
- **Do not change the features, the map's form, the bins, or the 5-point, 10-character and
  30-character lines after seeing a calibration number** (V-14, DECIDED (2) and (4)).
- **Do not fit a map per condition.**
- Do not use any feature that knows words, callsigns or letter frequencies (R72).
- **Do not touch, call or reuse `KeyerCwSender.cs`** or any transmit file (§0.2).
- Do not add a new test to either carry-forward line.
- Do not re-point or retire an existing test (R80).
- Do not edit `PARKED.md`, `CW_SPEC.md`, `CW_REQUIREMENTS.md`, or any ruling in `PHASE_PLAN.md` or
  `CLAUDE.md`.
- **Do not install any package. That is `MOVE: stop`.** The logistic fit is a few lines of
  Newton's method, in C#.
- Do not stage, commit or modify `.run-unit\fldigi\`, and do not fetch.
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose a
  timestamp.**

## 9. Committing and pushing

**One commit per task:**
- task 0;
- task 1's fact and printout;
- task 2's property, adapter, test and constants;
- task 3's measure, `calibration.md` and the `metrics.md` line;
- the 9.5 tick;
- task 4.

**Messages** take the form `unit464 task N: <what> (9.5)`. Task 3's message names, per decoder,
the conditions calibrated, not calibrated and not measurable.

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
   7 2 of 5, 8 0 of 6, 9 <n> of 8; steps 2 to 8 still barred by R86,
   and 9.4 open with every mechanism 9.3 named refused.
B. Step 9, criterion 9.5: HM-REQ-124 - ours' p from <feature>, the
   port's p from <features, cw.cxx lines>; held-out by recording,
   ten bins, 5 points; ours calibrated on <conditions>, not on
   <conditions>, not measurable on <conditions>; the port calibrated
   on <...>, not on <...>, not measurable on <...>; in-sample against
   held-out gap <n> points; every text and class byte-identical
   <yes | no>; the port byte-identical <yes | no>; 9.5 <ticked | open>.
C. The findings weighed against A and B: how many items section 4
   raises, whether any is in the way of 9.6 - which decoder may vote
   where, under HM-REQ-124 - and whether R86 with 9.4's refusals is
   in the way of steps 2 to 8.
```

```
UNIT:       464 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-124 <met on n of m conditions for ours, k of m for the port>; held-out overall share right against mean p - ours <r> against <p> (real, inferred), <r> against <p> (synthetic, exact); the port <...>; texts byte-identical <yes|no>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the calibration table.** One row per condition. For each decoder the row
gives:
- the scored characters;
- the mean p and the share right, held-out and in-sample;
- the worst populated bin's difference in points;
- the verdict: calibrated, not calibrated or not measurable.

The key's kind goes beside each row.

Then give:
1. task 1's separation print for each feature, and the map's constants;
2. the full held-out reliability tables per condition;
3. MET-CAL's three-bin table for ours, if not dropped;
4. how `EveryCharacterCarriesAConfidenceTests` was watched failing first;
5. the commit table, with the five results at each commit.

**Section 2, one paragraph, in the owner's words.** Nothing on the screen changes. Say what the new
number would mean to him if he could see it, and say plainly where each decoder's "I'm 90% sure"
turned out to be right 90% of the time and where it was not.

**Section 4 must say, as a plain reading and not a ruling request:**
- **9.4's words ("one technique 9.3 named") and 9.3's five refused mechanisms leave 9.4 no
  authorable route, and R86 holds steps 2 to 8 behind 9.4.** Step 9's 9.5 to 9.7 carry the loop
  meanwhile. Only the owner can change how R86 or 9.4 reads. **This is logged for him, not a
  stop:** it touches neither transmit nor what the product promises the operator.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: attach a numeric confidence p to every character of both decoders - ours from its rival margin, the port from its own AGC level and element timing read outside Cw/Second - fit one logistic per decoder fixed before any number is seen, and measure calibration held-out by recording per condition in ten reliability bins against HM-REQ-124's 5 points, no letter or class changed
MOVE: work around
WHY: PHASE_PLAN.md section 6's R86 bars steps 2 to 8 until 9.4 has a kept change, so the launcher's step 2 line 2.4 cannot move, and 9.4's "one technique 9.3 named" has no route left now that all five mechanisms 9.3 named are recorded refused; step 9's own next line, 9.5 (HM-REQ-124), is permitted by R86, is what section M orders before any vote, has never been attempted, and the loop test finds no entry for this approach.
STATE: partial
DECIDED: author's, overrulable - (1) step 9 criterion 9.5 is worked instead of the launcher's step 2 on R86 (PHASE_PLAN.md section 6, the owner's commit 6de324b1), as units 462 and 463's arbiters did, and 9.4 is not re-attempted because every mechanism 9.3 named is refused and a sixth attempt would restate one; this is not a step-2 unit and 2.4's count stays at 1 of 3; (2) the confidence is a logistic over features fixed at task 1 before any calibration number - ours' MarginLlr, the port's level margin (sig_avg over noise_floor) and timing margin (distance from two_dots) read from its public Emissions and KeyEvents, never by changing Cw/Second - one map per decoder pooled over the real and synthetic sets, not per condition; (3) the calibration verdict is taken held-out by recording (fit on 34, measure the 35th) and the in-sample figure is printed beside it, because a map measured on what it was fitted to proves nothing (CLAUDE.md 12.5, V-13); (4) calibrated on a condition means every bin of width 0.1 holding at least 10 characters, and the condition overall, within 5 points of mean p, with at least 30 scored characters, else not measurable; right is the scorer's verdict, wrong and added are wrong, placeholders are unscored as MET-CAL's placeholder bin, and dim characters are scored like sure ones; (5) no letter, class or threshold of either decoder changes, texts byte-identical, so R78's keep rule is not engaged; (6) 9.5 is ticked when both decoders carry p on every character and the calibrated conditions are named, whether or not any condition is calibrated, as the criterion's own words ask; (7) R86 with 9.4's exhausted route is reported in section 4 as a reading for the owner and not a stop, since it touches neither transmit nor what the product promises; (8) the app line's headless dispatcher-loop loss: one rerun, and any type lost again is run alone and must pass, named in the report.
LICENCE: PHASE_PLAN.md step 9 lines 9.3, 9.4 and 9.5, section R (R84, R85), section 6 (R86, the second decoder is faithful, V-14), R61, R72, R77, R80; CW_REQUIREMENTS.md HM-REQ-013, 014, 121, 122, 124, 129, V-11, V-13, V-14; CW_SPEC.md MET-CAL; docs/phase-requirements/parity.md sections 1 and 3; fldigi cw.cxx at 61b97f41; unit 463's report section 4; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: every letter either decoder reads now carries a number saying how likely it is to be right, and Hamlet knows, condition by condition, where each decoder's 90% really means 90% - which is what decides, in the next step, which of the two gets the vote when they disagree
ADVANCES: step 9 criterion 5
END-ARBITER-DECISION
```
