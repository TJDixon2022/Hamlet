# Work instruction 450 - the pitch says whether it was proved

**Authored by the arbiter against step 4's open criterion 4.6.** Steps 0 and 1 are done. By the
plan's checkboxes, step 2 is at 3 of 5, step 3 at 3 of 6 and step 4 at 4 of 7. Steps 5 to 8 are at 0.

**Why not step 2.** Unit 449 kept a change: real MET-CER-SURE went from 40 of 433 to 33 of 436. That
is what step 2 is for, and it set 2.4's count of units with no kept change back to **0 of 3**. So no
unit can flip 2.4 this pass. 2.5 is still held red by 443's DECIDED (3), which only the owner lifts.
Step 3's open lines (3.4, 3.5 and 3.6) have the same shapes: a before-and-after print that needs a
kept change, a closing count, and the same red floors. Step 5's 5.1 is recorded no.

**Why 4.6.** HM-REQ-093 is a must-tier requirement, and no unit has attempted it. Today the decoder
reports pitch as one boolean, `PitchWasMeasured`, which is set to `_tracker.HasMeasuredPitch`, and
that is simply "a number is held". So a survey candidate, a from-cold point and a hold that has
outlived its keying are all reported the same way as a pitch proved on keying. The requirement's own
rationale reads: *"Not a survey candidate, not a stale hold."* The record sheet then prints any held
number as "measured from the keying the survey admitted". **That is the operator reading more
certainty about the pitch than the decoder has.** So this unit changes what the operator reads (R83).

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

## 1. Why this unit exists

The count today: **step 4 at 4 of 7** (4.1, 4.2, 4.3, 4.5), step 2 at 3 of 5, step 3 at 3 of 6,
step 5 at 0 of 6. These are the figures at HEAD `706e3874`, from unit 449's exit:

| metric | condition, key | count | value |
|---|---|---|---|
| MET-CER-SURE | real, inferred | 33 of 436 sure (28 substituted, 5 added) | 0.0757 |
| MET-CER-SURE | synthetic, exact | 14 of 173 | 0.0809 |
| MET-INVENTED | real, inferred | 33 over 473 | 0.0698 |
| MET-INVENTED | synthetic, exact | 14 over 252 | |
| sure-and-right coverage (R82) | real, inferred | 403 over 473 | 0.8520 |
| sure-and-right coverage (R82) | synthetic, exact | 159 over 252 | 0.6310 |
| MET-WBE | real, inferred | 46 over 113 | 0.4071 |
| MET-PITCH-ERR | files more than 25 Hz off; windows | 9 of 69; 248 of 1688 | |

The floors at the same HEAD: captures 51 of 51, adjudicated 13 of 13, and named 10 of 13. The three
red named floors are 17:37 (38 against 46), `032113` (43 against 45) and `032129` (42 against 64).
**Task 0 re-measures all of these, and its numbers win over this table.**

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The pitch the decoder reports carries a proof state of proved, hypothesis or
            none (HM-REQ-093). A survey candidate, a from-cold point and a hold that has
            outlived its keying stop being reported as measured, and the record sheet says
            which of the three the operator is looking at. Not one decoded character changes.
ADVANCES:   step 4 criterion 6 - the plan's line 4.6
```

`step 4 criterion 6` is the launcher's form, and it means the plan's line `- [ ] 4.6`.

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`.** In `CW_SPEC.md`, read §2 (the decoded
record: "the running pitch with its proof state") and section J. Where the documents differ from this
instruction, the documents win. **Quote each id below from the document in section 1 of your report,
and report any difference as a mismatch.**

- **HM-REQ-093:** pitch reported with a proof state of proved, hypothesis or none. The verification
  table's row for it reads: I and T, any condition, proof-state field, present, synthetic. **This is
  the requirement the unit meets.**
- **HM-REQ-091:** the tracked pitch is chosen by keying quality, never by level alone. "Proved" rests
  on that keying, never on level.
- **HM-REQ-092:** MET-PITCH-ERR. Its N is TBD, so it is measured and reported, never judged.
- **HM-REQ-034:** the same three-valued state for the speed. **It is step 5's (5.5) and is not built
  here.** See §7.
- **V-12:** nothing is diagnosed against audio that has not itself been proved.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- HEAD is `706e3874`, unit 449's exit commit. In both copies of `PHASE_PLAN.md`, **4.1, 4.2, 4.3 and
  4.5 are ticked, and 4.4, 4.6 and 4.7 are not.**
- `CwDecodeReport.cs` carries `bool PitchWasMeasured` and `PitchWasAsserted`. `CwDecoder.cs:255` sets
  `PitchWasMeasured: _tracker.HasMeasuredPitch`, and `CwToneTracker.cs:461` defines that property as
  `!double.IsNaN(_reportedHz)`. `CwPitchChoice.cs` exists. `MainWindowViewModel.ToneForTheRecord`
  prints the sheet's pitch line. Confirm each of these.
- `docs/phase-requirements/traceability.md` lists HM-REQ-093 with no proving test. It names two tests
  that measure something else: `AHeldPitchDoesNotOutliveItsEvidenceTests.MovingTheDialReleasesThePitchMeasuredBeforeIt`
  and `TheSheetSaysWhatEachElementWasSentAtTests.AnUnmeasuredPitchSaysSoRatherThanPrintingNumbers`.
  Confirm this. **Do not edit the traceability table** (R80). Only the new test's own name and
  comment carry the id.
- 447's pitch instrument is `Cw/Instruments/CwPitchInstrument.cs`, and it lives under `tests`.
- **Expected failures at entry. They are not yours to fix:**
  - The named floors 17:37, `032113` and `032129`, as §1 gives them.
  - `TheFiveToEightDecibelPlateauHolds`, the correctness phase's recorded red. It is in neither
    carry-forward line.
  - The app line losing up to 5 to the dispatcher loop, each type green alone. 448's entry saw the
    host hang once at 480 s. **If it hangs again, report it as the second occurrence** and rerun once.
- **Known, and not yours.** Report each of these once and edit none of them:
  - `PHASE_OUTCOME.md`'s header still carries the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165, while `CLAUDE.md` §1 holds CPS-DEC-0183.
  - `PARKED.md`'s header says a session never writes it.

## 3. Rulings in force - do not re-argue

- **`PHASE_PLAN.md` R77 to R81 and §6, and R82 and R83** as work instruction 441 recorded them.
  **R83:** a unit of this phase changes what the operator reads, or it is not authored. Here, what
  changes is the sheet's pitch line.
- **R75 and R76.** The tracker is open and the instrument is proved. **The instrument never enters
  the decode path** (447's DECIDED (3)). It is used by the trace and by the judgment, never by `src`.
- **Arbiter rulings carried:**
  - **443's DECIDED (3), as 448's DECIDED (6) read it:** no floor is re-banked, so 4.7 stays red.
  - **442's DECIDED (2):** a floors line is read as green at the exit of every commit from the
    unit's working task on.
  - **448's DECIDED (3):** a MET-PITCH-ERR move inside the 0.5 Hz bin is not a rise.
- **Acquisition is parked with the owner.** `PARKED.md` holds 4.5's promise question on what
  "acquiring" means for HM-REQ-102. **So "proved" must not be defined as "acquired", and no
  character's confidence class may read the proof state.** The state describes the pitch, and
  nothing downstream in the decode path consumes it.
- **This unit's own readings. They are author's, and overrulable:**
  - **(a) The state claims no more than today.** Wherever `PitchWasMeasured` is false at HEAD, the
    state is `none` or, for an asserted pitch, `hypothesis`, never `proved`. `proved` is a subset of
    what HEAD calls measured. **The product therefore never states more about a signal than it does
    today, only less.** That is why this unit does not touch the owner's promise stop.
  - **(b) What "proved" rests on is the unit's to define from the tree**, in words taken from what
    the tracker already computes. The likely source is HM-DEC-095's confirmed keyed verdict on the
    bin being demodulated, still evidenced now rather than held from before. The unit states its
    definition in one sentence, cites the lines it rests on, and names each tracker path that
    yields `hypothesis`. Those include a survey candidate, the from-cold point, an asserted pitch,
    and a hold past its keying.
  - **(c) Decoding is unchanged, byte for byte.** This is a reporting change, and R78's keep rule is
    met by showing that nothing it measures moved: every recording's text is identical, and the four
    metrics, the three floor tests and MET-PITCH-ERR are identical. **If any character anywhere
    changes, the change is wrong. Find why. Do not judge it under R78.**
  - **(d) The tick rule for 4.6.** It needs all of these:
    - the test naming HM-REQ-093 is green;
    - it was watched failing first;
    - the three values reach `CwDecodeReport`;
    - the sheet prints the state;
    - (c) holds.
    
    The real-set table in task 1 is evidence. It is not a condition of the tick.
  - **(e) 4.4's count.** This unit is not a 4.4 attempt, because it changes no pitch choice. **DRIFT
    for step 4 stays at 1.**
- **V-04 and V-14:** no fixture is admitted by lowering a gate. No separation limit, confirmation
  rule or plausibility bound is loosened. **V-06:** no digital silence in a synthetic case. **V-13:**
  an inferred key is not proof by itself.
- **`CLAUDE.md` §12.5:** a fixture built from the same misunderstanding as the code proves nothing.
  So the synthetic cases get their truth from construction: which pitch was keyed, when it stopped,
  and where a louder unkeyed carrier sits. Their truth never comes from what the tracker reports.
- **R72:** no word, dictionary or callsign prior. **`CLAUDE.md` §0.0:** never present a guess as a
  decode. **§0.2:** nothing that keys or transmits.
- **HM-DEC-155:** no suite. Named types only, one per invocation, each with its own `timeout`.
  Captures get 600 s. **Never background and poll.**
- **HM-DEC-165, FACT-004.** **If a package is needed: `MOVE: stop` (§6).**

## 4. Status cadence

At the start of each task, run `sh tools/status.sh EXECUTING "<n> of 3" code none "<one line>"`. At
the end, run `COMPLETED`. `SESSION.lock` belongs to the runner, so do not take or release it. Write
nothing to `RUN_LEDGER.md`, and touch nothing under `tools\arbiter\`.

What breaks in this shell:
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, `rm` is refused, and Python cannot run here.
- A multi-line commit uses `-m` more than once.

Scripts go in `.run-unit\unit450-<name>.sh` and run with `sh`. **Never compose a timestamp. Read the
clock.**

---

## 5. The tasks

### Task 0 - the entry

- Add `## UNIT 450 - STEP 4` to `PHASE_OUTCOME.md`, from the decision block at the foot.
- Name 450 in `PHASE_STATUS.md` with `CURRENT_STEP: 4`, and patch-bump the version.
- Commit the root's uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` with the
  entry. **These are the runner's writes. Commit them as they are, without editing them.**
- Run the entry round and print each result as a number:
  - the build;
  - both carry-forward lines, **each with its wall time**;
  - the three floor tests, with the captures type's wall time;
  - the four metrics per condition, with the key's kind beside each number;
  - the count of files more than 25 Hz off, from 447's printer.
- **Save every recording's decoded text at HEAD** to `.run-unit/unit450-text-before.txt`. Clause
  (c) is judged against this file.

### Task 1 - the trace: every way the tracker comes to hold a pitch

Read `CwToneTracker`, `CwToneSurvey`, `CwPitchChoice` and the places `_reportedHz` is set and
cleared. For **every path that sets or keeps a reported pitch**, print:
- the file and line;
- what evidence the path rests on: keying verdict, level, operator assertion, a hold, or from cold;
- whether that evidence is current at the hop, or remembered;
- the state it should yield under §3 (b), with one line of reason.

Then **print the proposed state over the real set**, as a fact that asserts nothing. Compute it
from a read-only reading of the tracker, with nothing in `src` changed yet. For each of the 23 keyed
recordings, give:
- the hops in each state;
- **how many of each state's windows the instrument puts more than 25 Hz off the tracked pitch.**

Print the same for the synthetic set. For each of the 9 files over 25 Hz, name the state the
decoder was in while it was off. **If `proved` covers windows more than 25 Hz off, that is a
finding. Print the paths that produced them.**

Commit the printer beside `WhereTheSureWrongLettersComeFromTests` or as a sibling, and commit its
output as `.run-unit/unit450-trace-pitch-state.txt`.

**Drop candidate:** the per-window instrument cross over every one of the 69 captures. Keep it on the
23 keyed recordings and the 9 files over 25 Hz, and say what was dropped. **Tasks 2 and 3 are not
dropped.**

### Task 2 - the test, then the state, then the sheet

**Write the test first.** Name it for HM-REQ-093 in its type or method name and in its comment,
along with the verification table's row (I and T, any condition, field present, synthetic). Build it
on synthetic sends with exact construction, at 15 dB, in a shaped noise band (V-06), with at least
these cases:
- **none:** a noise band with no tone;
- **proved:** a keyed tone at a known pitch, read well after its keying is confirmed. The instrument
  puts the reported pitch within one of its bins of the constructed pitch;
- **hypothesis, survey candidate:** the first moments on a keyed tone, before the confirmation
  §3 (b) names. This case applies only if the tree has such a window. If it has none, say so and
  replace it with the next case;
- **hypothesis, not proved, level alone:** a louder unkeyed carrier beside a keyed station, cold.
  Whatever the tracker holds while it rests on level is not `proved`;
- **not proved, stale hold:** the keying stops, and the tone is read past the point §3 (b) says the
  evidence lapses.

**Watch it fail first.** The first time the test runs, derive the state from today's
`PitchWasMeasured` and `PitchWasAsserted`, mapping measured to `proved`. It must fail on the
stale-hold case, the level case, or both, and the output must print which. If it passes, the cases
do not separate what the requirement separates. Rebuild them.

**Then build the state:**
- Add the three-valued proof state to `CwDecodeReport`, set by the decoder from what the tracker
  already knows. The type, its name, and whether it replaces or sits beside `PitchWasMeasured` are
  yours. If the boolean stays, it must never disagree with the state.
- **Nothing in the decode path reads the new state.**
- Make the sheet's pitch line (`ToneForTheRecord`) print the state in plain words. A `hypothesis`
  pitch must not be described as measured from keying. The wording is yours, within §3 (a).
- Run the app types that cover the sheet, each alone.

**Then judge it under §3 (c).** Decode every recording again and compare the result with
`.run-unit/unit450-text-before.txt`. Print the count of recordings compared and the count that
differ, which must be 0. Print the four metrics, the three floor tests and the MET-PITCH-ERR count
beside the entry figures.

**If (c) holds and the test is green:**
- Commit the state, the sheet change and the test on their own.
- **Tick 4.6 in both copies of `PHASE_PLAN.md`.**
- Write one line to `docs/phase-requirements/metrics.md`: HM-REQ-093 met, the task 1 table's
  totals per state, and the key's kind.

**If (c) fails, or the test cannot be made green without changing decoding:**
- Leave `src` as it was, and commit the diff with its test as `.run-unit/unit450-notkept.diff`.
- Name what failed. **Do not tick 4.6.**

### Task 3 - the exit round

- Run the build, both carry-forward lines, the three floor tests, the four metrics, MET-PITCH-ERR
  and every type touched. Print each result as a number, with its wall time.
- **Report what changed in `src`, file by file.** Say that none of it keys or transmits, and that
  nothing in the decode path reads the new state.
- **Ticks:** confirm whether 4.6 is ticked, per task 2. **Do not tick 4.4, 4.7, 2.4, 2.5** or
  anything in steps 3 or 5.
- **Make the exit commit and push it.**
- Report DRIFT: step 2 0, step 3 0, step 4 1 (§3 (e)), step 5 1.

---

## 6. Parked - do not touch, do not raise

- Everything in `PARKED.md`:
  - step 2's question on which files the transmit check covers. **This unit neither defines nor
    runs a transmit-file list.** It reports its `src` diff file by file;
  - 448's question on what "acquiring" means, which is the owner's.
- 17:37's floor and 444's section 4 question, which stay with the owner.
- 449's section 4 items:
  - item 1, HM-DEC-127's entry superseded in the decision log. That is step 8 (R80);
  - item 2, `_readingDb` never cleared. A later 4.4 unit may take it. **This unit only prints it
    in task 1 if it produces a stale hold, and changes nothing about it;**
  - item 3, HM-REQ-015's "dim rate". It is logged, and so are its two mismatch lines.
- 448's, 446's, 445's and 443's section 4 items, and 444's half-unit dropout rule.
- 4.4's tracker change, the speed proof state (5.5), the speed search (5.1 to 5.6), and the
  traffic-net print (3.4).
- The correctness phase's 5.1, which is Tim's, and all of `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These are
  step 8 (R80).

## 7. What not to do

- Do not change which pitch the tracker chooses, or when it moves. That is 4.4's, not this unit's.
- Do not let any character's class, any emission gate or any speed path read the proof state.
- Do not define `proved` as "acquired", and do not gate anything on acquisition.
- Do not report `proved` anywhere HEAD reports the pitch as unmeasured (§3 (a)).
- Do not build the speed's proof state (HM-REQ-034). It is 5.5's.
- Do not wire the instrument into `src`.
- Do not re-bank any floor. Do not tick 4.4 or 4.7.
- Do not edit `traceability.md`, the decision log or any line of `PARKED.md`.
- Do not correct any ruling. Report the disagreement.
- Do not add a package. If one is needed, stop and report it.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

Make one commit per task, and **one commit for the state, the sheet change and the test on their
own**. Each message names the criterion it serves: `unit450 task N: <what> (4.6)`. Push to
`origin/main` after each commit. End every commit message with:

```
Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
```

## 9. Reporting

Write `output.md` at the root. **`validate-output.bat` refuses it unless every one of these holds:**
- The ordering block comes first, within the first 60 lines. It has a `READ IN THIS ORDER.` line,
  then lines that begin `A.`, `B.` and `C.`.
- **Line C contains the words `Section 4 raises N items`.**
- The `UNIT:` line follows, without brackets.
- Then come exactly these four headings, and no fifth: `## 1. What Claude did`,
  `## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.
- Section 3 is not empty.

**Run `./tools/arbiter/validate-output.bat output.md` before you finish. Do not finish on INVALID.**

```
READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 4 at <n> of 7, step 2 at 3 of 5, step 3 at 3 of 6,
   step 5 at 0 of 6 by the plan's checkboxes, steps 0 and 1 done, 6, 7 and 8 not started.
B. Step 4, criterion 4.6 (HM-REQ-093): proof state <built | not built>; test <name> <green,
   watched failing on <case>>; recordings whose text changed <n> of <n>; real-set hops proved
   <n>, hypothesis <n>, none <n>; proved windows more than 25 Hz off the instrument <n>;
   4.6 <ticked | not ticked>; 4.4 and 4.7 open.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       450 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     text changed <n> of <n>; proved <n>, hypothesis <n>, none <n> hops real; proved over 25 Hz off <n>; MET-CER-SURE real <n> -> <n>
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1
```

**Section 3 opens with task 1's path table**, one row for every way the tracker comes to hold a
pitch, with its evidence and the state it yields. Then:
- **the sheet's pitch line as text, before and after**, on three recordings: one `proved`, one
  where HEAD said measured and the unit now says `hypothesis`, and one `none`. The owner reads the
  difference rather than the number;
- the real-set table per state, with the instrument's more-than-25 Hz count beside each;
- the (c) table: recordings compared and recordings changed, and the four metrics, the floors and
  MET-PITCH-ERR, before and after.

**Section 2, in one paragraph:** What does the operator now read about the pitch that they did not
read before, and in how many of the real recordings does it now say `hypothesis` where it said
measured? Give the evidence (V-13), and say what the synthetic cases do not prove (§12.5).

---

```
ARBITER-DECISION
STEP: 4
APPROACH: replace the boolean PitchWasMeasured with a three-valued pitch proof state proved hypothesis none in the decode report, test naming HM-REQ-093 watched failing first, the record sheet states it, decoding byte-identical
MOVE: work around
WHY: PHASE_PLAN.md step 4 line 4.6 asks that HM-REQ-093, pitch reported with a proof state of proved, hypothesis or none, be met with a test naming it, and today the decoder reports any held number as measured, a survey candidate and a stale hold included; step 2's 2.4 count was reset to 0 by 449's kept change and 2.5 stays held by 443's DECIDED (3), and no attempt has been recorded against 4.6.
STATE: partial
DECIDED: author's, overrulable - (1) step 4 is worked instead of step 2 because 449's kept change reset 2.4's count to 0 and 2.5 is held by 443's DECIDED (3), so no step 2 line can flip this pass; (2) proved is a subset of what HEAD calls measured, so the product never states more about a signal than today, which keeps the unit off the owner's promise stop; (3) proved is defined by the unit from what the tracker already computes, never as acquired, and nothing in the decode path reads it, since acquisition is parked with the owner; (4) the unit is judged by decoding byte-identical on every recording with the four metrics, floors and MET-PITCH-ERR unchanged, and 4.6 is ticked on a green HM-REQ-093 test watched failing first, the state in CwDecodeReport and on the sheet, and that identity; (5) this unit is not a 4.4 attempt and leaves step 4's DRIFT at 1; (6) 449's section 4 is logged and not chased: item 1 is step 8's, item 2 is printed only if it yields a stale hold, item 3 and its mismatches are logged.
LICENCE: PHASE_PLAN.md step 4 line 4.6 and section 5's independence line; R75 and R76 as carried; R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2), 443 DECIDED (3), 447 DECIDED (3), 448 DECIDED (3) and (6); V-04; V-06; V-12; V-13; V-14; R72; HM-REQ-091, 092, 093; HM-DEC-095; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the operator can tell a pitch the decoder proved on the station's keying from one it is only supposing or has held past its evidence, and no decoded letter changes to do it
ADVANCES: step 4 criterion 6
END-ARBITER-DECISION
```
