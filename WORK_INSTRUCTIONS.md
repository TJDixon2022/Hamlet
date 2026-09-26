# Work instruction 451 - the speed says whether it was proved

**Authored by the arbiter against step 5's open criterion 5.5.** Steps 0 and 1 are done. By the
plan's checkboxes, step 2 is at 3 of 5, step 3 at 3 of 6, step 4 at 5 of 7 and step 5 at 0 of 6.
Steps 6, 7 and 8 are at 0.

**Why not step 2, which the launcher named.** Step 2 has two open lines, and neither can flip this
pass:
- **2.4** closes the step after three consecutive units with no kept change. Unit 449 kept a change
  (real MET-CER-SURE 40 of 433 to 33 of 436), which set the count to **0 of 3**. No step 2 unit has
  run since.
- **2.5** needs the three named floors green. It is held red by 443's DECIDED (3), as 448's
  DECIDED (6) read it: no floor is re-banked. Only the owner lifts that. Unit 444 showed that no
  honest gap rule gives 17:37 its boundaries back, and that route is recorded no.

Step 3's open lines have the same shape: 3.4 needs a recording that is not in the tree, 3.5 is a
closing count, and 3.6 is the same red floors. Step 4's 4.4 is a closing rule, and 4.7 is the same
red floors.

**Why 5.5.** HM-REQ-034 is must-tier and no unit has attempted it: *"The decoder shall report the
speed estimate with a proof state of proved, hypothesis, or none."* Its rationale reads: *"A speed
the decoder has not earned is not a number."*

Today the speed has no state:
- `CwDecoder.WordsPerMinute` is a nullable number behind a four-part guard.
- `SpeedIsReacquiring` is a separate boolean.
- The sheet's `SpeedForTheRecord` builds its wording from both of them and from
  `Reading.WordsPerMinute`.

Nothing tells the operator whether the number in front of them was earned now or is being held.
HM-REQ-035 (a clear keeps speed, pitch and noise floor) and HM-REQ-036 (a pitch refinement keeps
timing) sit on the same line of the plan, and neither has a test that measures what it states.

**This unit changes what the operator reads (R83):** the speed line on the sheet, and every surface
that shows the speed, states which of the three it is. Unit 450 did the same for the pitch in one
unit (4.6). This is the speed's half.

Why not the other routes:
- **7.1 and 5.3/5.4.** A generator or a measurement changes nothing the operator reads, so R83
  refuses it as a unit on its own.
- **5.1.** It is recorded no.
- **5.2.** Its recording is not in the tree.

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

The count today is **step 5 at 0 of 6**, step 4 at 5 of 7 (4.1, 4.2, 4.3, 4.5, 4.6), step 2 at 3 of
5 and step 3 at 3 of 6. These are the figures at HEAD `f96cd04e`, from unit 450's exit, which
changed no decoded character:

| metric | condition, key | count | value |
|---|---|---|---|
| MET-CER-SURE | real, inferred | 33 of 436 sure | 0.0757 |
| MET-CER-SURE | synthetic, exact | 14 of 173 | 0.0809 |
| MET-INVENTED | real, inferred | 33 over 473 | 0.0698 |
| MET-INVENTED | synthetic, exact | 14 over 252 | |
| sure-and-right coverage (R82) | real, inferred | 403 over 473 | 0.8520 |
| sure-and-right coverage (R82) | synthetic, exact | 159 over 252 | 0.6310 |
| MET-WBE | real, inferred | 46 over 113 | 0.4071 |
| MET-PITCH-ERR | files more than 25 Hz off | 9 of 69 | |

The floors at the same HEAD are captures 51 of 51, adjudicated 13 of 13 and named 10 of 13. The red
named floors are 17:37 (38 against 46), `032113` (43 against 45) and `032129` (42 against 64).
**Task 0 re-measures all of these, and its numbers win over this table.**

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  The speed the decoder reports carries a proof state of proved, hypothesis or
            none (HM-REQ-034), and the sheet and every speed display say which one the
            operator is looking at. HM-REQ-035 (a clear keeps speed, pitch and noise floor)
            and HM-REQ-036 (a pitch refinement keeps timing) each get a test naming them,
            and the report says whether each is met. Not one decoded character changes.
ADVANCES:   step 5 criterion 5 - the plan's line 5.5
```

`step 5 criterion 5` is the launcher's form, and it means the plan's line `- [ ] 5.5`.

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`.** In `CW_SPEC.md`, read §2 (the decoded
record) and whatever it says about the speed and its proof state. In `CW_REQUIREMENTS.md`, read
section D and the verification table's rows for 034, 035 and 036. Where the documents differ from
this instruction, the documents win. **Quote each id below from the document in section 1 of your
report, and report any difference as a mismatch.**

- **HM-REQ-034:** speed reported with a proof state of proved, hypothesis or none. **This is the
  requirement the unit meets.**
- **HM-REQ-035:** "When the transcript is cleared, the decoder shall retain its speed, pitch and
  noise-floor state." **Measured by a test naming it. Not repaired here** (§3 (d)).
- **HM-REQ-036:** "When the pitch is refined for the same station, the decoder shall retain its
  timing state." **Measured by a test naming it. Not repaired here** (§3 (d)).
- **HM-REQ-031:** speed within 10% of true after acquisition. It is 5.3's. This unit prints the
  speed error of `proved` hops on the synthetic set as evidence only, and judges nothing by it.
- **HM-REQ-093:** the pitch's proof state, met by unit 450. The speed's state follows its pattern,
  and the pitch's state is not changed.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- **HEAD and the plan.** HEAD is `f96cd04e`, unit 450's exit commit. In both copies of
  `PHASE_PLAN.md`:
  - 4.1, 4.2, 4.3, 4.5 and 4.6 are ticked, and 4.4 and 4.7 are not.
  - No line of step 5 is ticked.
- **The speed today.** Confirm each of these:
  - `CwDecodeReport` carries `CwPitchProof PitchProof` and has no speed state.
  - `CwDecoder.WordsPerMinute` (about line 439) is `int?`.
  - `SpeedIsReacquiring` is a boolean (about line 421).
  - `CwDecoder.Retuned()` is only `Unlock()` (line 385).
  - `MainWindowViewModel.SpeedForTheRecord` (about line 12818) prints the sheet's speed line from
    those, and from `Reading.WordsPerMinute` as "the decoder's own best hypothesis".
- **The traceability table.** `docs/phase-requirements/traceability.md` rows 94 to 96 list HM-REQ-034,
  035 and 036 with no proving test. The tests they list measure something else:
  - `CwSpeedSilenceTests.TheReacquiringStateIsReadable`;
  - `CwAdjudicationTests.ClearingTheScreenLeavesTheDecoderAloneOnRealisticAudio`;
  - `CwRefiningRetuneTests.TheSurveySettlingBetweenTwoBinsIsNotAStationChange`;
  - and others.

  Confirm this. **Do not edit the traceability table** (R80). Only the new tests' own names and
  comments carry the ids.
- **A known conflict with HM-REQ-036.** `AHeldPitchDoesNotOutliveItsEvidenceTests.TheReleaseStartsTheReadingFresh`
  asserts that the speed goes to 0 after `Retuned()`. That type is 3 of 4 red at HEAD, and unit 450
  logged it. Report how its assertion stands against HM-REQ-036, and edit nothing in it.
- **Expected failures at entry. They are not yours to fix:**
  - The named floors 17:37, `032113` and `032129`, as §1 gives them.
  - `TheFiveToEightDecibelPlateauHolds`.
  - `AHeldPitchDoesNotOutliveItsEvidenceTests`, 3 of 4. It is in neither carry-forward line.
  - The app line losing up to 5 to the dispatcher loop, each type green alone. **If the host hangs,
    rerun once and report it.**
- **Known, and not yours.** Report each once and edit none of them:
  - `PHASE_OUTCOME.md`'s header still carries the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165, while `CLAUDE.md` §1 holds CPS-DEC-0183.
  - `PARKED.md`'s header says a session never writes it.
  - `TheQuietestBinNoLongerWinsTests` and `ThePitchControlsAreOffThePanelTests` are excluded by
    `Compile Remove`.

## 3. Rulings in force - do not re-argue

- **`PHASE_PLAN.md` R77 to R81 and §6, and R82 and R83** as work instruction 441 recorded them.
  **R83:** a unit of this phase changes what the operator reads, or it is not authored. Here, what
  changes is the speed line and every speed display.
- **R75 and R76**, carried. The pitch instrument never enters `src` (447's DECIDED (3)).
- **Arbiter rulings carried:**
  - **443's DECIDED (3), as 448's DECIDED (6) read it:** no floor is re-banked. So 5.6, like 2.5, 3.6
    and 4.7, stays red.
  - **442's DECIDED (2):** a floors line is read as green at the exit of every commit from the
    unit's working task on.
  - **446's DECIDED (5):** 5.1 may be met by two kept changes, one per end. `135641` is the owner's
    recording to supply, and its absence never halts a unit.
- **Parked with the owner, and not re-raised here** (`PARKED.md`):
  - 448's question on what "acquiring" means;
  - 450's questions on when a heard pitch may be called proved.

  **So `proved` is never defined as "acquired", and nothing gates on acquisition.** The unit builds
  the strict reading 450 built for the pitch, and does not argue the parked question again.
- **This unit's own readings. They are author's, and overrulable:**
  - **(a) The state claims no more than today.** `proved` is a subset of what HEAD shows as a
    number: wherever `WordsPerMinute` is null at HEAD, the state is never `proved`. No surface shows
    a speed number that it withholds at HEAD. **The product therefore never states more about a
    signal than it does today, only less.** That keeps the unit off the owner's promise stop.
  - **(b) What `proved` rests on is the unit's to define from the tree**, in words taken from what
    the decoder already computes. The likely source is the guard on `WordsPerMinute` (a located tone,
    a resolved character, not re-acquiring, a settled pass that proved a dit), **held only while its
    evidence is current, not after the keying has stopped.**
    - The unit states its definition in one sentence and cites the lines it rests on.
    - It names each path that yields `hypothesis`. Those include the rolling reading with the guard
      failing, a speed held past its keying, and a re-acquiring clock.
    - It names each path that yields `none`.
  - **(c) Decoding is unchanged, byte for byte.** This is a reporting change, and R78's keep rule is
    met by showing that nothing it measures moved:
    - every recording's text is identical;
    - the four metrics, the three floor tests and MET-PITCH-ERR are identical.

    **If any character anywhere changes, the change is wrong. Find why. Do not judge it under R78.**
  - **(d) HM-REQ-035 and 036 are measured, not repaired, this unit.** Each test asserts what its
    requirement states, on synthetic sends of exact construction.
    - **If one is red at HEAD:** it is committed red, named in the report with the lines that make
      it fail, and put on no carry-forward line. Its repair is a later unit's.
    - **A test that passes only because it asserts less than the requirement does not count.**
  - **(e) The tick rule for 5.5.** It needs all of these:
    - the HM-REQ-034 test is green, and was watched failing first;
    - the three values reach `CwDecodeReport`, and the sheet and every speed display print them;
    - tests naming HM-REQ-035 and HM-REQ-036 exist and ran;
    - the report states met or not met for each of 034, 035 and 036, with the evidence;
    - (c) holds.

    A red 035 or 036 does not stop the tick, because 5.5 asks that the report state whether each is
    met.
  - **(f) Step 5's count.** This unit is not a 5.1 attempt. **DRIFT for step 5 stays at 1.**
- **V-04 and V-14:** no fixture is admitted by lowering a gate. No separation limit, confirmation
  rule or plausibility bound is loosened. **V-06:** no digital silence in a synthetic case. **V-13:**
  an inferred key is not proof by itself.
- **`CLAUDE.md` §12.5:** a fixture built from the same misunderstanding as the code proves nothing.
  The synthetic cases get their truth from construction: the speed sent, when the keying stopped,
  where the speed changed, and when the clear or the refinement happened. **Their truth never comes
  from what the decoder reports.**
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

Scripts go in `.run-unit\unit451-<name>.sh` and run with `sh`. **Never compose a timestamp. Read the
clock.**

---

## 5. The tasks

### Task 0 - the entry

- Add `## UNIT 451 - STEP 5` to `PHASE_OUTCOME.md`, from the decision block at the foot.
- Name 451 in `PHASE_STATUS.md` with `CURRENT_STEP: 5`, and patch-bump the version.
- Commit the entry together with the root's uncommitted `PARKED.md`, `PHASE_OUTCOME.md`,
  `PHASE_STATUS.md` and `RUN_LEDGER.md`. **These are the runner's writes. Commit them as they are,
  without editing them.**
- Run the entry round and print each result as a number:
  - the build;
  - both carry-forward lines, **each with its wall time**;
  - the three floor tests, with the captures type's wall time;
  - the four metrics per condition, with the key's kind beside each number;
  - the count of files more than 25 Hz off, from 447's printer.
- **Save every recording's decoded text at HEAD** to `.run-unit/unit451-text-before.txt`. Clause
  (c) is judged against this file.

### Task 1 - the trace: every way the decoder comes to report a speed

Read `CwDecoder`, the settled pass, the clock and re-acquisition code, and every place
`WordsPerMinute`, `SpeedIsReacquiring` and `Reading.WordsPerMinute` are set, cleared or read.
**Also find every place in `src` that shows a speed to the operator**, not only the sheet.

For **every path that sets, holds or withholds a reported speed**, print:
- the file and line;
- what evidence it rests on: a proved dit, the rolling reading, a hold, a re-acquisition, or
  nothing;
- whether that evidence is current at the hop, or remembered;
- the state it should yield under §3 (b), with one line of reason.

Then, for the same code, **trace what a transcript clear and a pitch refinement do to the timing,
the speed, the pitch and the noise floor**, citing lines. Say what "a refinement for the same
station" is in the tree: the tracker's follow against its retune, and `Retuned()`. **This is the
evidence 035's and 036's tests are built on.**

**Then print the proposed state over the sets**, as a fact that asserts nothing. Compute it from a
read-only reading, with nothing in `src` changed yet.
- **Real set.** For each of the 23 keyed recordings, give the hops in each state.
- **Synthetic set.** Give the same, plus, **for the `proved` hops, the reported speed against the
  constructed speed.** Show the count more than 10% off. That is HM-REQ-031's measure, printed as
  evidence only.
- **If `proved` covers a hop more than 10% off the constructed speed, that is a finding.** Print
  the path that produced it.

Commit the printer beside unit 450's pitch-state printer, or as its sibling, and commit its output
as `.run-unit/unit451-trace-speed-state.txt`.

**Drop candidate:** the per-recording rows of the real-set table. Keep the totals per state and the
synthetic set's speed error, and say what was dropped. **Tasks 2 and 3 are not dropped.**

### Task 2 - the tests, then the state, then the displays

**Write the HM-REQ-034 test first.** Name HM-REQ-034 in its type or method name and in its comment,
along with the verification table's row. Build it on synthetic sends with exact construction, at
15 dB, in a shaped noise band (V-06). Give it at least these cases:
- **none:** a noise band with no tone;
- **proved:** a keyed tone at a known speed, read well after the guard's evidence is in. The
  reported speed is within 10% of the constructed speed;
- **hypothesis, not yet proved:** the first moments on a keyed tone, where the rolling reading
  exists and the guard withholds the number. **This case applies only if the tree has such a
  window.** If it has none, say so and use the next case instead;
- **hypothesis or none, stale hold:** the keying stops, and the band is read past the point §3 (b)
  says the evidence lapses;
- **hypothesis, re-acquiring:** a speed change of at least 25%, read inside the re-acquisition.

**Watch it fail first.** The first time the test runs, derive the state from today's fields:
- `WordsPerMinute` present maps to `proved`;
- otherwise, `none`.

It must fail on the stale-hold case, the hypothesis case, or both, and the output must print which.
**If it passes, the cases do not separate what the requirement separates. Rebuild them.**

**Write the HM-REQ-035 and HM-REQ-036 tests**, each named for its id, from task 1's evidence:
- **035:** decode a keyed send, clear the transcript at a constructed instant, and keep decoding
  the same station. Assert that the speed, the pitch and the noise floor after the clear equal those
  just before it, within the decoder's own resolution. State that resolution in the test's
  comment.
- **036:** decode a keyed send, then refine the pitch for the same station by the tree's own
  refinement path, at a step task 1 names. Assert that the timing state (the unit, the speed, and
  whatever task 1 names) is kept across it.
- **Run each alone at HEAD and record green or red.** Do not change `src` to make either green
  (§3 (d)).

**Then build the state:**
- Add the three-valued speed proof state to `CwDecodeReport`, set by the decoder from what it
  already knows.
  - The type, its name, and whether `SpeedIsReacquiring` stays beside it are yours.
  - **Any boolean that stays must be read from the state**, as `PitchWasMeasured` now is, so the two
    cannot disagree.
  - **Nothing in the decode path reads the new state.**
- Make `SpeedForTheRecord` print the state in plain words, and every other speed display task 1
  found state it.
  - A `hypothesis` speed is never presented as measured.
  - No display shows a number that HEAD withholds (§3 (a)).
  - The wording is yours.
- Run the app types that cover the sheet and those displays, each alone.

**Then judge it under §3 (c).**
- Decode every recording again and compare the result with `.run-unit/unit451-text-before.txt`.
  Print the count of recordings compared and the count that differ, which must be 0.
- Print the four metrics, the three floor tests and the MET-PITCH-ERR count beside the entry
  figures.

**If (c) holds and the HM-REQ-034 test is green:**
- Commit the state, the display changes and the three tests on their own.
- **Tick 5.5 in both copies of `PHASE_PLAN.md`** under §3 (e).
- Write one line to `docs/phase-requirements/metrics.md` giving:
  - HM-REQ-034 met;
  - 035 and 036 met or not met;
  - task 1's totals per state;
  - the synthetic `proved` count more than 10% off, with the key's kind.

**If (c) fails, or the 034 test cannot be made green without changing decoding:**
- Leave `src` as it was.
- Commit the diff with its tests as `.run-unit/unit451-notkept.diff`.
- Name what failed. **Do not tick 5.5.**

### Task 3 - the exit round

- Run the build, both carry-forward lines, the three floor tests, the four metrics, MET-PITCH-ERR
  and every type touched. Print each result as a number, with its wall time.
- **Report what changed in `src`, file by file.** Say that none of it keys or transmits, and that
  nothing in the decode path reads the new state.
- **Ticks:** confirm whether 5.5 is ticked, per task 2. **Do not tick 5.1 to 5.4, 5.6,** or anything
  in steps 2, 3 or 4.
- **Make the exit commit and push it.**
- Report DRIFT: step 2 0, step 3 0, step 4 1, step 5 1 (§3 (f)).

---

## 6. Parked - do not touch, do not raise

- **Everything in `PARKED.md`:**
  - step 2's question on which files the transmit check covers. **This unit neither defines nor
    runs a transmit-file list.** It reports its `src` diff file by file;
  - 448's question on what "acquiring" means;
  - 450's questions on when a pitch may be called proved.
- 17:37's floor and 444's section 4 question, which stay with the owner.
- **450's section 4.** Items 1 and 2 are parked. Item 3 (`AHeldPitchDoesNotOutliveItsEvidenceTests`
  red, and the two `Compile Remove` types) is logged. **This unit only reports how
  `TheReleaseStartsTheReadingFresh` stands against HM-REQ-036, and edits nothing in it.**
- Earlier units' section 4 items, and 444's half-unit dropout rule.
- **Work that belongs to other lines:**
  - the repair of HM-REQ-035 or 036 if either is red;
  - the speed search (5.1), `135641` (5.2), MET-WPM-ERR as a judged metric (5.3), MET-TACQ and
    MET-LAT (5.4);
  - 4.4's tracker change;
  - the traffic-net print (3.4).
- The correctness phase's 5.1, which is Tim's, and all of `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These are
  step 8 (R80).

## 7. What not to do

- Do not change how the speed is found, held, cleared or re-acquired. Do not change the pitch's proof
  state.
- Do not let any character's class, any emission gate or any pitch path read the speed state.
- Do not define `proved` as "acquired", and do not gate anything on acquisition.
- Do not report `proved`, or show a number, anywhere HEAD withholds the speed (§3 (a)).
- Do not repair HM-REQ-035 or 036 this unit. Do not weaken either test until it passes.
- Do not wire the pitch instrument into `src`.
- Do not re-bank any floor. Do not tick 5.6, 2.5, 3.6 or 4.7.
- Do not edit `traceability.md`, the decision log, any line of `PARKED.md`, or
  `AHeldPitchDoesNotOutliveItsEvidenceTests`.
- Do not correct any ruling. Report the disagreement.
- Do not add a package. If one is needed, stop and report it.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

Make one commit per task, and **one commit for the state, the display changes and the three tests
on their own**. Each message names the criterion it serves: `unit451 task N: <what> (5.5)`. Push to
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

A. Hamlet meets the CW requirements: step 5 at <n> of 6, step 4 at 5 of 7, step 2 at 3 of 5,
   step 3 at 3 of 6 by the plan's checkboxes, steps 0 and 1 done, 6, 7 and 8 not started.
B. Step 5, criterion 5.5 (HM-REQ-034, 035, 036): speed proof state <built | not built>; 034 test
   <name> <green, watched failing on <case>>; 035 <met | not met>; 036 <met | not met>;
   recordings whose text changed <n> of <n>; real-set hops proved <n>, hypothesis <n>, none <n>;
   synthetic proved hops more than 10% off <n>; 5.5 <ticked | not ticked>; 5.1 to 5.4 and 5.6 open.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       451 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     text changed <n> of <n>; proved <n>, hypothesis <n>, none <n> hops real; synthetic proved over 10% off <n>; 035 <met|not met>; 036 <met|not met>
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1
```

**Section 3 opens with task 1's path table**: one row for every way the decoder comes to report,
hold or withhold a speed, with its evidence and the state it yields. Then give:
- **what a clear and a refinement do**, with lines cited, beside 035's and 036's test results;
- **the sheet's speed line as text, before and after**, on three recordings: one `proved`, one
  where HEAD showed a number and the unit now says `hypothesis` (if any exists), and one `none`.
  The owner reads the difference rather than the number;
- every other speed display, before and after;
- the per-state table, with the synthetic speed error beside `proved`;
- the (c) table: recordings compared and recordings changed, and the four metrics, the floors and
  MET-PITCH-ERR, before and after.

**Section 2, in one paragraph:** What does the operator now read about the speed that they did not
read before? In how many real recordings does it now say `hypothesis` where HEAD showed a number?
Does a clear or a refinement cost the decoder its speed today? Give the evidence (V-13), and say
what the synthetic cases do not prove (§12.5).

---

```
ARBITER-DECISION
STEP: 5
APPROACH: give the reported speed a three-valued proof state proved hypothesis none in the decode report and on the sheet and every speed display, test naming HM-REQ-034 watched failing first, and tests naming HM-REQ-035 clear retains speed pitch noise floor and HM-REQ-036 pitch refinement retains timing, measured not repaired, decoding byte-identical
MOVE: work around
WHY: PHASE_PLAN.md step 5 line 5.5 asks that HM-REQ-034, the speed reported with a proof state, and HM-REQ-035 and 036 each have a test naming them with the report stating whether each is met, and today the speed is a nullable number and a boolean with no state; no step 2 line can flip this pass, since 449's kept change set 2.4's count to 0 of 3 and 2.5 is held by 443's DECIDED (3), and no attempt has been recorded against 5.5.
STATE: not started
DECIDED: author's, overrulable - (1) step 5 is worked instead of the launcher's step 2, because 2.4's count is 0 of 3 after 449's kept change and 2.5 is held by 443's DECIDED (3) as 448's DECIDED (6) read it; 5.5 is chosen over 7.1, 5.3 and 5.4 because it changes what the operator reads (R83) and they do not, and over 5.1 and 5.2 because 5.1 is recorded no and 5.2's recording is absent; (2) proved is a subset of what HEAD shows as a speed number and no display shows a number HEAD withholds, so the product never states more about a signal than today, which keeps the unit off the promise stop, and the parked pitch question is neither re-raised nor answered; (3) proved is defined by the unit from the guard the decoder already computes, lapsing with its evidence as 450's pitch state does, never as acquired, and nothing in the decode path reads it; (4) HM-REQ-035 and 036 are measured by tests naming them and not repaired this unit, and a red one is committed red on no carry-forward line; (5) 5.5 is ticked on a green HM-REQ-034 test watched failing first, the state in CwDecodeReport and on every speed display, 035 and 036 tests run with met or not met stated, and decoding byte-identical; (6) 450's section 4 is logged and not chased: items 1 and 2 are parked, and item 3 is reported only where TheReleaseStartsTheReadingFresh bears on HM-REQ-036; this unit is not a 5.1 attempt and leaves step 5's DRIFT at 1.
LICENCE: PHASE_PLAN.md step 5 line 5.5 and section 5's independence line; R78, R80, R81, R83 and section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2), 443 DECIDED (3), 446 DECIDED (5), 447 DECIDED (3), 448 DECIDED (6), and 450 DECIDED (2) and (3) as the pattern followed; V-04; V-06; V-13; V-14; R72; HM-REQ-031, 034, 035, 036, 093; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the operator can tell a speed the decoder earned from the keying now from one it is only supposing or holding, and the owner learns whether clearing the screen or refining the pitch costs the decoder its timing, with no decoded letter changed
ADVANCES: step 5 criterion 5
END-ARBITER-DECISION
```
