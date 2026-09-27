# Work instruction 466 - the arbitration earns its place: arbitrated, ours alone and the port alone scored on every metric of sections B and I per condition, and the arbitration switched off wherever it loses

**Loop unit.** Step 9, criterion 9.7 (`CW_REQUIREMENTS.md` section M: HM-REQ-128, with the metrics
of section B, HM-REQ-010 to 015, and section I, HM-REQ-080 to 084).

**Why step 9, why 9.7, and not the launcher's step 2.**
- **R86, `PHASE_PLAN.md` section 6:** *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."* No
  later ruling lifts it. Step 2's only open line, 2.4, counts step-2 units, and R86 bars step-2
  units. This is not a step-2 unit, and 2.4's count stays at 1 of 3.
- **9.4 has no route the plan's words allow.** All five mechanisms 9.3 named are recorded as
  refused: (D) by unit 459; (B), (C) and (W) by unit 462; (A) in both forms by unit 463. A sixth
  attempt would restate one of them.
- **9.6 is ticked** (unit 465, `cf38e1c4`). The arbiter exists, and both decoders read the same
  hops live. **9.7 is the next line section M orders.** HM-REQ-128 is the requirement that says
  whether the combination earns its place, and nothing it needs is missing. No unit has
  attempted it, and the loop test finds no entry for this approach.
- **What 9.7 will likely find.** Under today's vote table the port votes nowhere, so the
  arbitrated text equals ours on all 35 recordings. The arbitration can therefore lose only
  where the port alone beats ours on some metric of a condition. Where it does, HM-REQ-128
  switches the arbitration off on that condition and uses the better decoder alone. This unit
  measures that, per condition, fixes the switch table from the measurement, and builds it.

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
- Python cannot run here. Everything is C#.
- A multi-line commit uses `-m` more than once.
- Scripts go in `.run-unit\unit466-<name>.sh` and are run with `sh`. Unit 465's
  `.run-unit\unit465-*.sh` helpers may be copied and renamed.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 9 criterion 7`. The launcher reads the digits after "criterion", so this
  means plan line 9.7.
- WHY cites the plan.

**R85. A CW question is answered from the documents and fldigi's source, never raised to the
owner.** That covers:
- which metrics "every metric of section B and section I" means;
- what "loses" means;
- which decoder is "the better decoder alone" when the metrics split;
- which path, harness or live, decides a condition's switch.

The DECIDED block at the foot answers each of these. Where the tree forces a reading this
instruction did not foresee, record it in one line of section 4 and carry on.

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
UNIT GOAL:  the arbitrated transcript, ours alone and the port alone are
            scored side by side on every metric of sections B and I, per
            condition, on the harness and on the live path; wherever the
            arbitration loses to either decoder alone, it is switched off
            on that condition and the better decoder alone is used there;
            the switch is a table fixed from the measurement, built, and
            proved by a test naming HM-REQ-128, watched failing first.
ADVANCES:   step 9 criterion 7
```

**Read these first. They win over this instruction:**
- `CW_REQUIREMENTS.md` section M, HM-REQ-124 to 128, and the verification table's row for 128
  (*"T, A | every condition | every metric of B and I | arbitrated >= better single decoder |
  ... | switched off per condition on a loss"*);
- section B, HM-REQ-010 to 015, and section I, HM-REQ-080 to 084, all of it;
- `CW_SPEC.md`'s definitions of MET-CER-SURE, MET-INVENTED, MET-COVERAGE and MET-WBE;
- `docs/phase-requirements/parity.md` sections 1 and 3 (the port's class mapping and the
  condition rows), and `calibration.md` section 2 (the vote table's source);
- `CwArbiter`, `CwVoteTable`, `CwArbiter.TieMargin` and `CwSecondReader`, which unit 465 built;
- unit 465's `WhereTheTwoReadingsMeetFact` and its printout, `.run-unit\unit465-meet.txt`.

**The requirement this unit serves, verbatim:**

> **HM-REQ-128:** "The arbitrated output shall be no worse than the better single decoder on every
> metric of §B and §I, on every condition. If arbitration loses to either decoder alone on any
> condition, it is switched off for that condition and the better decoder alone is used there."

**9.7 is met when all of these are true:**
1. For every condition row, the arbitrated output, ours alone and the port alone are each scored
   on every metric DECIDED (2) lists, and tabled side by side, with the key's kind.
2. Each row states whether the arbitration loses to either decoder alone, by DECIDED (3), and
   which decoder is the better alone, by DECIDED (4).
3. Wherever the arbitration loses, the switch table turns it off on that row and the better
   decoder alone is emitted there. The emitted output on that row is then re-scored, and it is
   no worse than the better decoder alone on every metric.
4. A test naming HM-REQ-128 is watched failing first, and is then green.
5. The report tables all three per condition, and the emitted output after the switch beside
   them.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `9cc2c74b` or a runner commit on top of it.
- **`PHASE_PLAN.md`**, in both copies:
  - shows 9.1, 9.2, 9.3, 9.5 and 9.6 ticked, and 9.4, 9.7 and 9.8 open;
  - carries R86's line in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **The port** is at `src\Hamlet.RadioEngine\Cw\Second\`, and `git diff 19109b51 --
  src/Hamlet.RadioEngine/Cw/Second/` prints nothing.
- **Unit 465's work is present:**
  - `CwArbiter`, `CwVoteTable` and `CwArbiter.TieMargin` at 0.05;
  - `CwSecondReader`;
  - `TheHigherCalibratedReadingWinsTests`, `BothDecodersReadTheSameSamplesTests` and
    `TheOperatorSeesOneTranscriptTests`;
  - the `arbiter` line on the capture sheet.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the untracked `.run-unit\reports\unit-4-output-7.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
- **Logged and not this unit's:**
  - the reload's `RULES_AT` disagreement (HM-DEC-165 against CPS-DEC-0183);
  - `outcome-read`'s step titles for steps 2, 3 and 8, which differ from `PHASE_PLAN.md`'s.

**Entry figures, 465's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278.
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:** MET-CER-SURE 33 of 436, MET-INVENTED 33 over
  473, coverage 403 over 473, MET-WBE 37 over 113.
- **Synthetic set, 12 cases, exact key:** MET-CER-SURE 14 of 173, MET-INVENTED 14 over 252,
  coverage 159 over 252, MET-WBE 44 over 84.
- **The port, per parity.md:** real 62 wrong of 239 sure, coverage 177; synthetic 34 of 168,
  coverage 134.
- **The meeting counts, harness:** real agree 290, disagree 113, tie 16, one-sided ours 438,
  one-sided port 48. Synthetic: 120, 16, 2, 37, 32.
- **The meeting counts, live:** real 228, 94, 13, 522, 40. Synthetic: 112, 16, 2, 45, 26.
- **Decode time**, ours plus the port plus the arbiter over 690 s of real audio: 65.09 s. Ours
  alone was 52.22 s.

If any entry figure differs, report it and use the measured one.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31. It is on neither carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under DECIDED (9).
- The parity run rewriting the decode-time rows of `parity.md`. Keep a copy under `.run-unit\` and
  restore the committed file, as units 462 to 465 did.

---

## 4. Rulings in force - transcribed, do not re-argue

**The ordering:**
- **R86** (`PHASE_PLAN.md` section 6): *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."*
- **R84** (`PHASE_PLAN.md` section R): *"... and the arbitration is switched off on any condition
  where it loses to the better decoder alone (128). Rejected: the port as a bench instrument
  only; replacing ours with the port."*
- **Section 6:** *"The second decoder votes only where it is calibrated (HM-REQ-124), and the
  arbitration is switched off on any condition where it loses to a single decoder (HM-REQ-128)."*
- **HM-REQ-122 and 129, and section 6:** *"The second decoder is faithful, not improved."* Not
  one line under `Cw\Second\` changes.
- **Section 6:** *"Where this plan and the two documents differ, the documents win, and the
  difference is a finding in the report."*
- **Section 6:** *"A requirement with a TBD threshold ... is measured and the measurement
  reported; the threshold is the owner's and its absence never halts a unit."* HM-REQ-127's
  margin stays at 0.05, as unit 465 held it.

**R78 and V-11.** The switch is ordered by HM-REQ-128, not proposed as a decoder change. But it
can move letters on a row, so on every row where it changes the emitted text:
- report R78's four metrics before and after;
- check V-11 on the requirements' metrics (section 6: *"no change may redden an earlier capture
  to green a newer one, and that is judged on the requirements' metrics"*).

On a row where the switch leaves the emitted text equal to today's, R78 is not engaged. The text
must then be byte-identical to task 0's save of the arbitrated transcript.

**Standing rulings this unit leans on:**
- **R77.** A new CW test names the requirement it serves.
- **R80.** No traceability, test inventory or decision-log work.
- **R72.** No word, dictionary or callsign prior (HM-REQ-004, HM-DEC-175). A metric that prefers
  one decoder because its text makes words is refused.
- **R61.** A key is inferred unless it was transcribed.
- **V-11 and V-13.** One scorer, one set of keys, the same calls for all three outputs.
- **V-14.** The metric list, the loss rule, the better-decoder rule and the path rule are fixed
  at task 1, before any three-way number is seen, and never changed after.
- **R85.** As section 1.

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode.
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched, not called and not reused.** The switch is receive
  only.
- `CLAUDE.md` §12.5: a fixture built from the same misunderstanding as the code proves nothing.
  The injected case of task 3 is built by hand from HM-REQ-128's words, not from the switch's
  output.
- HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task, naming what it will measure or build;
- when task 1 fixes the metric list and the three rules;
- when the three-way table comes back, naming the rows where the arbitration loses;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 466 - STEP 9` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 466 with `CURRENT_STEP: 9`, in both copies.
3. Bump the patch from 1.13.152 to 1.13.153.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `BothDecodersAreScoredAlikeTests`;
   - `EveryCharacterCarriesAConfidenceTests`;
   - the port's own tests;
   - unit 465's three tests.
6. Save three texts, each character with its class and p:
   - ours alone to `.run-unit\unit466-ours-before.txt`;
   - the port alone to `.run-unit\unit466-port-before.txt`;
   - the arbitrated transcript, harness and live, to `.run-unit\unit466-arb-before.txt` and
     `.run-unit\unit466-arb-live-before.txt`.

   Every byte-identical check in this unit is made against these.
7. Time the arbitrated live path over the real set, as the entry decode time.

### Task 1 - the rules fixed, before any three-way number

This task reads and writes rules. **Nothing under `src` changes, and no three-way figure is
computed.**

1. **The metric list.** Go through section B and section I, requirement by requirement, and write
   down for each:
   - the metric it is judged by (DECIDED (2) is the starting reading);
   - which way is better;
   - whether it is defined for a decoder that emits no dim character (the port) or has no live
     rendering apart from its settled one.

   A requirement that is a property rather than a per-condition figure (HM-REQ-015, 082) is
   stated once, for all three outputs, with how it was checked.
2. **The loss rule, the better-decoder rule and the path rule**, as DECIDED (3), (4) and (5) give
   them, or as the tree forces them, with the reason.
3. **Where the switch goes.** Name, by file and member, where `CwArbiter` or `CwVoteTable` would
   consult a per-condition switch, both in the harness and on the live path through
   `CwSecondReader` and `CwDecoder`, without a line under `Second\` changing. Name how "port
   alone" would be emitted: its characters at parity.md section 1's mapping, through the same
   `CharacterSettled` seam.
4. **Write all of it** into the header of a new fact, `TheArbitrationEarnsItsPlaceFact`, which
   serves HM-REQ-128 and asserts nothing. Put it in the commit message too.

**Commit the fact with its header only**, before any three-way figure exists (V-14).

### Task 2 - the three-way table (HM-REQ-128, measured)

The fact now prints to `.run-unit\unit466-threeway.txt` and to a new
`docs/phase-requirements/arbitration.md`.

1. **Per condition, using parity.md section 3's rows**, give every metric of task 1 for:
   - the arbitrated output;
   - ours alone;
   - the port alone.

   Each figure goes through the same `CwMetrics` calls, with the key's kind beside it.
2. **Two paths:**
   - the harness, where each recording runs under its own condition row;
   - the live path, `CwDecoder` with `CwSecondReader` on, grouped by the same rows.

   Put the two tables side by side. The live path is what the operator's screen carries.
3. **Per row, state:**
   - whether the arbitration loses, and to which decoder, on which metric (DECIDED (3));
   - the better decoder alone (DECIDED (4));
   - the switch that follows: `arbitrate`, `ours alone` or `port alone`.
4. **HM-REQ-084:** print what each of the three emits on the six named spans.
5. **HM-REQ-013, 080 and 014's per-row verdicts**, met or not, for each of the three, where the
   row is one the requirement names.
6. **Byte-identical:**
   - ours alone, the port alone and both arbitrated saves, against task 0;
   - `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` printing nothing.

**Drop candidate for this task:** the per-recording breakdown under each row, and the live-path
table for the synthetic rows. Shed those if time runs short. The per-condition harness table, the
live table for the real rows, and item 3's switch per row are never shed.

**Commit the fact's printout and `arbitration.md`.**

### Task 3 - the switch built, and HM-REQ-128 watched failing first

1. **The switch table**, beside `CwVoteTable` and outside `Second\`. It holds, per condition row,
   `arbitrate`, `ours alone` or `port alone`, transcribed from task 2's `arbitration.md` with its
   commit named. Its remarks name HM-REQ-128 and DECIDED (3) to (5). The live product reads the
   row it already runs under (unit 465: real HF, all).
2. **`CwArbiter` consults it.** On `ours alone`, it emits ours as ours prints alone. On `port
   alone`, it emits the port's characters at parity.md's mapping. On either, both readings still
   go on the sheet, marked with the switch in force (HM-REQ-121). On `arbitrate`, nothing
   changes.
3. **`TheArbitrationEarnsItsPlaceTests`, naming HM-REQ-128**, split into named types if one would
   pass 600 s. It has two parts:
   - **(a) The rule, on injected figures.** One condition where the arbitration beats both. One
     where it loses to ours. One where it loses to the port. One where the decoders split on
     metrics, so DECIDED (4)'s order decides. Each case asserts the switch chosen and the output
     emitted. Watch it red against a stub that always answers `arbitrate`.
   - **(b) The corpus.** Per condition row, the emitted output is no worse than the better decoder
     alone on every metric of task 1. Run it before the switch table is wired: it is red on every
     row task 2 found losing. If task 2 found no losing row, say so; then (b) cannot be watched red
     on the corpus, and (a)'s port-loss case is its red.

   Say how each part was watched.
4. **Re-score the emitted output after the switch**, on both paths, and table it beside task 2's
   three columns.
   - On a row whose text changed, give R78's four metrics before and after, and check V-11 on the
     floor tests.
   - On a row whose text did not change, check it byte-identical to task 0's arbitrated save.
5. **Decode time** before and after, over the real set.
6. Run the three floor tests and both carry-forward lines. Commit the switch, the arbiter's use of
   it and the test in one commit.
7. **Tick 9.7** in both copies of `PHASE_PLAN.md`, in a follow-on commit, only when all five of
   section 2's conditions hold. If any part is not met, leave 9.7 open and say which part is
   missing.

### Task 4 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `BothDecodersAreScoredAlikeTests`;
- `EveryCharacterCarriesAConfidenceTests`;
- the port's own tests;
- unit 465's three tests;
- `TheArbitrationEarnsItsPlaceTests`;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- the emitted transcript on each path against task 0's arbitrated saves: identical, or every
  difference named with its row and its switch;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with its results on the five at its exit: build, both
  carry-forward lines, and the three floor tests.

**Ticks:**
- **9.7**, as task 3 says.
- **9.8**, in its own follow-on commit, only if 9.7 is ticked and the exit commit has the three
  floor tests and both carry-forward lines green. The one exception is DECIDED (9)'s dispatcher
  loss, and only as that item handles it.
- Do not tick 9.4, or any line of steps 2 to 8.

---

## 7. Parked - do not touch, do not raise

- **9.4 and every mechanism 9.3 named, (A) to (E) and (W), in any form.** Each is refused.
- **Unit 465's section 4:**
  - item 3, whether close agreements are ties;
  - item 4, the leading edge;
  - item 5's retune rule, holding the port until our pitch is measured. The live path is measured
    as it stands, because changing the port's input mid-measurement would move the thing being
    judged (V-14). That rule is a later unit's.
  - items 6 to 9: the sample rate, the row bound, decode time, and live pairing.
- **Refitting either confidence, re-measuring calibration, or changing the vote table or the
  margin.** The switch sits beside the vote table and does not edit it.
- **Unit 464's section 4 item 5; unit 463's section 4 items 3 and 4.**
- **The four questions `PARKED.md` records, and their `RESOLVED:` answers.**
- **Step 2's decoder work, and 2.4's count.** The count stays 1 of 3.
- **Steps 3 to 8,** under R86.
- **461's section 4, 459's section 4 items 3 to 5, 458's items 2 to 5, 6.5's tick, HM-REQ-084's
  `ABOVE`, and unit 455's two 6.1 findings.**
- **The two `PHASE_STATUS.md` copies differing beyond the unit's fields.**

## 8. Do not

- **Do not change any line under `src\Hamlet.RadioEngine\Cw\Second\`** (HM-REQ-122, 129).
- **Do not change a letter, a class, a threshold, the lattice or the confidence map of either
  decoder.** The switch chooses between outputs; it never edits one.
- **Do not change the metric list, the loss rule, the better-decoder rule or the path rule after a
  three-way figure is seen** (V-14). Do not choose a row's switch by looking at its text.
- **Do not show a decoder's name, or which decoder or switch is in force, anywhere on the CW tab**
  (HM-REQ-121).
- **Do not change `CwMetrics`, the scorer, `MorseAlphabet`, `CwCalibration` or any test's key.**
- Do not use any rule that knows words, callsigns or letter frequencies (R72).
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

**One commit per task:**
- task 0;
- task 1's fact header, with the rules;
- task 2's printout and `arbitration.md`;
- task 3's switch table, the arbiter's use of it, and the test;
- the 9.7 tick, if earned;
- the 9.8 tick, if earned;
- task 4.

**Messages** take the form `unit466 task N: <what> (9.7)`. Task 2's message names the rows where
the arbitration loses, or says none does.

**Exit state:** every commit exits with these green:
- the build;
- both carry-forward lines;
- the three floor tests.

The one exception is the dispatcher loss under DECIDED (9).

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
B. Step 9, criterion 9.7 (HM-REQ-128): metrics compared <list>; the
   arbitration loses on <rows, or none>, to <decoder> on <metric>;
   the better decoder alone there is <ours|port> by <dominance|order>;
   switch table <row: switch, ...>; live row real HF, all -> <switch>;
   emitted output after the switch no worse than the better decoder
   alone on every row <yes|no>; HM-REQ-128's test watched failing
   first <how>; text changed on <rows, or none>; decode time <before>
   to <after>; the port byte-identical <yes|no>; 9.7 <ticked|open>;
   9.8 <ticked|open>.
C. The findings weighed against A and B: how many items section 4
   raises, whether any is in the way of 9.7 or 9.8, and whether R86
   with 9.4's refusals is in the way of steps 2 to 8.
```

```
UNIT:       466 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-128 <met|not>; arbitration loses on <k> of <n> rows; switch <arbitrate a, ours b, port c>; real HF all MET-CER-SURE arb/ours/port <x/y/z>; decode time <s> to <s> over 690 s
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the three-way table.** One row per condition, with the key's kind. For each
metric it gives arbitrated, ours alone and the port alone, then the loss verdict, the better
decoder, the switch, and the emitted output after the switch. The harness table comes first, and
the live table beside or under it.

Then give:
1. the metric list and the three rules, as task 1 fixed them;
2. where the switch sits, by file and member;
3. how each part of `TheArbitrationEarnsItsPlaceTests` was watched failing first;
4. HM-REQ-084's six spans as each of the three reads them;
5. on every row whose text changed, R78's four metrics before and after, and V-11;
6. decode time before and after;
7. the commit table, with the five results at each commit.

**Section 2, one paragraph, in the owner's words.** Say whether anything changes on the screen, and
on which kind of signal. Say plainly whether the second reader's vote earns its place anywhere
today, and where Hamlet now simply uses the better of the two readers instead.

**Section 4 must say, as a plain reading and not a ruling request:**
- **HM-REQ-127's margin is 0.05, the requirement's recommended value, held provisionally.**
- **9.4's words and 9.3's five refused mechanisms leave 9.4 no authorable route, and R86 holds
  steps 2 to 8 behind 9.4.** This is logged for the owner, not a stop. It touches neither transmit
  nor what the product promises the operator.
- Where the switch picks the port alone on any row, one line on what that row's operator reads
  at the port's all-sure mapping. DECIDED (6) is the reading that licenses it.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: score the arbitrated transcript against ours alone and the port alone on every metric of sections B and I per condition on the harness and the live path, fix a per-condition switch table that turns arbitration off where it loses and emits the better decoder alone, HM-REQ-128 test watched failing first
MOVE: work around
WHY: PHASE_PLAN.md section 6's R86 bars steps 2 to 8 until 9.4 has a kept change, so the launcher's step 2 line 2.4 cannot move, and 9.4 has no route now that all five mechanisms 9.3 named are recorded refused; 9.6 is ticked, so step 9's next line in section M's order is 9.7 (HM-REQ-128), which R86 permits, no unit has attempted, and the loop test does not find.
STATE: partial
DECIDED: author's, overrulable - (1) step 9 criterion 9.7 is worked instead of the launcher's step 2 on R86 (PHASE_PLAN.md section 6), as units 462 to 465's arbiters did; 9.4 is not re-attempted; this is not a step-2 unit and 2.4's count stays at 1 of 3; (2) under R85, "every metric of section B and section I" is read as MET-CER-SURE (010), MET-INVENTED (011), MET-COVERAGE sure-and-right over sent (012), sent characters not emitted sure and correct on the 15 dB rows (013), dim right over dim emitted (014), MET-WBE (080, 081), live-against-settled boundary differences on the live path (083) and the six named spans read exactly (084), with 015 and 082 stated once as properties; a metric undefined for one output (the port emits no dim; the port has one rendering) is not compared for it, and the report says so; (3) the arbitration loses on a row when it is strictly worse than either decoder alone on any compared metric of that row, a rate compared as a rate, fixed at task 1 before any three-way figure (V-14); (4) the better decoder alone on a row is the one no worse on every compared metric if one exists, else the first metric where they differ in the order 011, 010, 013, 012, 014, 081, 083, 084 decides, honesty first as CLAUDE.md 0.0 and HM-REQ-011's "prime directive as a number" rank them, and ours on a full tie; (5) a row's switch is set from the path that emits under it - the live product runs only under real HF, all (unit 465), so that row is set from the live path and printed from the harness beside it, and every other row from the harness; (6) HM-REQ-128 is the later and more specific requirement, so where it names the port alone as the better decoder on a row, the port alone is emitted there at parity.md section 1's mapping even though HM-REQ-124 withholds its vote, the documents winning over this plan and the difference reported; (7) a row whose switch changes the emitted text reports R78's four metrics before and after and V-11 on the floor tests judged on the requirements' metrics, and a row whose text does not change must be byte-identical to task 0's arbitrated save; if a switch row would redden a floor test with a requirement metric on that floor's recording worse, that row is held at arbitrate, 9.7 stays open and the row is named; (8) 9.8 is ticked in its own commit only if 9.7 is ticked and the exit commit is green on the five, as 7.5 was ticked with its step still partial; (9) the app line's headless dispatcher-loop loss: one rerun, and any type lost again is run alone and must pass, named in the report.
LICENCE: PHASE_PLAN.md step 9 lines 9.4, 9.6, 9.7 and 9.8, section R (R84, R85), section 6 (R86, the second decoder is faithful, the documents win, the TBD-threshold rule, V-11 judged on the requirements' metrics, V-14, R78), R61, R72, R77, R80; CW_REQUIREMENTS.md HM-REQ-010 to 015, 080 to 084, 121, 122, 124, 127, 128, 129 and the verification table's row 128, V-11, V-13, V-14; CW_SPEC.md MET-CER-SURE, MET-INVENTED, MET-COVERAGE, MET-WBE; docs/phase-requirements/parity.md sections 1 and 3 and calibration.md section 2; unit 465's report sections 3 and 4; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: Hamlet now knows, signal type by signal type, whether listening with two decoders beats listening with the better one alone, and where it does not, it quietly uses the better one instead, so the transcript the operator reads is never worse for having a second opinion
ADVANCES: step 9 criterion 7
END-ARBITER-DECISION
```
