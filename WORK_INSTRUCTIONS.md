# Work instruction 452 - where the words break

**The arbiter wrote this against step 6's open criterion 6.3.** Steps 0 and 1 are done. By the
plan's checkboxes, step 2 is at 3 of 5, step 3 at 3 of 6, step 4 at 5 of 7 and step 5 at 1 of 6.
Steps 6, 7 and 8 are at 0.

**Why not step 2, which the launcher named.** Neither of its two open lines can flip in one unit:
- **2.4** closes the step after three units in a row keep no change. Unit 449 kept a change, which
  set the count to **0 of 3**. No step 2 unit has run since. One unit takes the count to 1 at most.
- **2.5** needs all three named floors green. 443's DECIDED (3), as 448's DECIDED (6) read it, holds
  it red: no floor is re-banked. Only the owner can lift that ruling. Unit 444 measured that 17:37's
  sender has overlapping letter spaces (135 to 320 ms) and word spaces (245 to 485 ms), so no honest
  duration rule gives 17:37 its boundaries back. That route is recorded no.

**Why 6.3.** Of the four metrics the requirements are written in, word-boundary error is the
farthest from its requirement:

| metric | real set, inferred key, at HEAD | the requirement |
|---|---|---|
| MET-WBE | 46 over 113 words, 0.41 | HM-REQ-081 at or below 0.05; HM-REQ-080 at 0 |
| MET-CER-SURE | 0.076 | HM-REQ-010 below 0.01 |
| MET-INVENTED | 0.070 | HM-REQ-011 at 0 |
| sure-and-right coverage | 0.85 | at or above 0.90 |

- **No unit has worked step 6.**
- **HM-REQ-080 names a recording in the tree.** Its rationale reads: *"`DEW B 6 RE D` is not whole."*
  That is 17:37's CQ, and its verification row cites the WB6RED key.
- **This unit changes what the operator reads (R83).** Where a word breaks in the middle, or two
  words run together, the operator reads the words wrong.

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

These are the counts at HEAD `6d40cc90`, from unit 451's exit. That unit changed no decoded
character:

| metric | condition, key | count | value |
|---|---|---|---|
| MET-WBE | real, inferred | 46 over 113 words | 0.4071 |
| MET-WBE | synthetic, exact | 48 over 84 words | 0.5714 |
| MET-WBE | 17:37 alone, inferred | 7 over 6 words (last measured by unit 444) | |
| MET-CER-SURE | real, inferred | 33 of 436 sure | 0.0757 |
| MET-CER-SURE | synthetic, exact | 14 of 173 | 0.0809 |
| MET-INVENTED | real, inferred | 33 over 473 | 0.0698 |
| MET-INVENTED | synthetic, exact | 14 over 252 | |
| sure-and-right coverage (R82) | real, inferred | 403 over 473 | 0.8520 |
| sure-and-right coverage (R82) | synthetic, exact | 159 over 252 | 0.6310 |
| MET-PITCH-ERR | files more than 25 Hz off | 9 of 69 | |

The floors at the same HEAD:
- captures 51 of 51;
- adjudicated 13 of 13;
- named 10 of 13. The three red rows are 17:37 (38 against 46), `032113` (43 against 45) and
  `032129` (42 against 64).

**Task 0 re-measures all of these, and where its numbers differ from this table, its numbers win.**

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  Word-boundary error is reported per condition, with the key's kind, and
            HM-REQ-080 and HM-REQ-081 are measured against it on every condition the tree
            holds. Every wrong boundary is traced to its cause, and one change against the
            largest cause is built and kept under R78 only if MET-WBE falls and no recording's
            boundaries or letters get worse.
ADVANCES:   step 6 criterion 3 - the plan's line 6.3
```

`step 6 criterion 3` is the launcher's form. It means the plan's line `- [ ] 6.3`.

**Read `CW_REQUIREMENTS.md` first, then `CW_SPEC.md`.** Read these parts:
- in `CW_REQUIREMENTS.md`, section I and the verification table's rows 080, 081 and 082;
- in `CW_SPEC.md`, MET-WBE's definition, the sensitivity floor, and which TX-* sender profiles are
  must-tier.

Where the documents differ from this instruction, the documents win. **In section 1 of your report,
quote each id below from the document, and report any difference as a mismatch.**

- **HM-REQ-080:** "On every must-tier sender profile at 15 dB reference on CH-AWGN, the decoder
  shall place every word boundary where the sender placed it (MET-WBE = 0)." **Measured.**
- **HM-REQ-081:** "On every must-tier condition at the sensitivity floor, the decoder shall keep
  MET-WBE at or below 5 % of words." **Measured.**
- **HM-REQ-082:** word-boundary errors scored separately from character errors. Step 1 built this
  (1.3). It is the scorer this unit uses, and it is not changed.
- **HM-REQ-010 and HM-REQ-011:** guards on the change. A boundary change must not make a letter
  worse.
- **HM-REQ-083 and 084** belong to 6.4 and 6.5, not this unit.

## 2. Verify this instruction against the tree

Check each of these. **Where one is wrong, report it as a mismatch in section 1 and carry on. Do not
repair it.**

- **HEAD and the plan.** HEAD is `6d40cc90`. In both copies of `PHASE_PLAN.md`:
  - 5.5 is ticked;
  - no line of step 6 is ticked;
  - 2.1, 2.2 and 2.3 are ticked, and 2.4 and 2.5 are not.
- **The scorer.** MET-WBE is computed in `CwMetrics` and scored separately from MET-CER.
  - Name the method that computes it.
  - Say whether it splits inserted boundaries from deleted ones.
  - Say whether it can report per recording and per condition.
- **The conditions the tree holds.** List the synthetic cases and what each was built as: sender
  profile, speed, character gap and SNR. The running figures in `metrics.md` name these: TX-ITU at
  15, 5 and 0 dB, and a character-gap-5 sender at 15, 5 and 0 dB. List the real recordings'
  conditions as 443's entry grouped them: sender not stated, TX-FARNS, TX-ITU and TX-TIGHT, all
  with inferred keys.
  - **State which of these are must-tier sender profiles at 15 dB (HM-REQ-080), and which sit at
    the sensitivity floor (HM-REQ-081).** Name every condition either requirement asks for that the
    tree does not hold. **No CH-* channel profile exists until step 7 builds one.** A real
    recording is never counted toward a CH-* condition (7.4).
- **17:37's key** is `CQ CQ CQ DE WB6RED WB6RED`, inferred. The second `WB6RED` runs past the audio
  (444).
- **Expected failures at entry. They are not yours to fix:**
  - the three named floors in §1;
  - `TheFiveToEightDecibelPlateauHolds`;
  - `AHeldPitchDoesNotOutliveItsEvidenceTests`, 3 of 4. It is in neither carry-forward line;
  - the app line losing up to 5 to the dispatcher loop, with each type green alone. **If the host
    hangs, rerun once and report it.**
- **Known, and not yours.** Report each once, and edit none of them:
  - `PHASE_OUTCOME.md`'s header still carries the old titles for steps 2, 3 and 8.
  - `CW_SPEC.md` §11 still defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165, while `CLAUDE.md` §1 holds CPS-DEC-0183.
  - `PARKED.md`'s header says a session never writes it.
  - `TheQuietestBinNoLongerWinsTests` and `ThePitchControlsAreOffThePanelTests` are excluded by
    `Compile Remove`.

## 3. Rulings in force - do not re-argue

- **`PHASE_PLAN.md` R77 to R81 and §6, and R82 and R83** as work instruction 441 recorded them.
  **R83:** a unit of this phase changes what the operator reads, or it is not authored. Here, what
  changes is where the words break. **R78 is the keep rule.** It judges on the requirements'
  metrics, never on character counts.
- **R72:** no word, dictionary or callsign prior, in any form. **A boundary may never be placed or
  removed because the letters on either side do or do not make a word or a callsign.** A boundary
  comes from the audio: gap durations, the unit in force, the marks, and energy.
- **R75 and R76**, carried. The pitch instrument never enters `src`.
- **Arbiter rulings carried:**
  - **443's DECIDED (3), as 448's DECIDED (6) read it:** no floor is re-banked. So 6.6, like 2.5,
    3.6, 4.7 and 5.6, stays red.
  - **442's DECIDED (2):** a floors line is read as green at the exit of every commit from the
    unit's working task on.
  - **448's DECIDED (3):** a MET-PITCH-ERR move inside the 0.5 Hz bin is not a rise.
- **Parked with the owner, and not raised again here** (`PARKED.md`):
  - 444's question on 17:37: whether it is met at 5 or fewer boundaries wrong, or re-banked at 38;
  - 448's question on what "acquiring" means;
  - 450's questions on when a heard pitch may be called proved.
- **This unit's own readings. They are the author's, and the owner may overrule them:**
  - **(a) The tick rule for 6.3.** 6.3 is a measurement. It is ticked whether or not task 2's change
    is kept, when all of these hold:
    - MET-WBE is reported per condition, with inserted and deleted boundaries split, and the key's
      kind beside each number. This is done at HEAD, and again after the change if one is kept;
    - HM-REQ-080 is stated met or not met on each must-tier sender profile at 15 dB that the tree
      holds, and on 17:37 against its inferred key;
    - HM-REQ-081 is stated met or not met on each condition at the sensitivity floor that the tree
      holds;
    - every condition either requirement names that the tree does not hold is listed as `not
      measurable here`, with the step that would build it;
    - the running figure is in `docs/phase-requirements/metrics.md`.
  - **(b) The keep rule for task 2's change, under R78. Every one of these must hold:**
    - MET-WBE on the real set falls;
    - MET-WBE on the synthetic set does not rise;
    - MET-CER-SURE and MET-INVENTED rise on neither set;
    - sure-and-right coverage falls on neither set;
    - the adjudicated readings hold, or move onto their own adjudicated text (R66);
    - **V-11, per recording:** no recording, real or synthetic, gains a boundary wrong or a
      sure-wrong letter. **In particular, 17:37 may not go above 7.**

    A capture row or named row whose character count falls is reported, not refused, when every
    item above holds.
  - **(c) If 17:37 falls to 5 or fewer boundaries wrong**, report it with the text. **Do not re-bank
    its floor, and do not tick 2.5 or 6.6.** That floor's fate belongs to the next arbiter, against
    443's DECIDED (3).
  - **(d) 444's half-unit dropout rule** (`.run-unit/unit444-wbe-notkept.diff`) **is not rebuilt as
    it stood.** It failed V-11 on 17:37, `031905` and the synthetic character-gap-5 case at 5 dB.
    The change comes from task 1's trace, whatever group it names.
    - If the trace names the group that rule answered, the change must say how it differs.
    - Before it is kept, it must show those three recordings not getting worse.
  - **(e) Step counts.** This unit is not a 2.4, 3.5 or 4.4 attempt. **DRIFT stays as it is: step 2
    at 0, step 3 at 0, step 4 at 1, step 5 at 1.** Step 6 is at 0 if the change is kept, and 1 if it
    is not.
- **V-04 and V-14:** no fixture is admitted by lowering a gate. No separation limit, confirmation
  rule or plausibility bound is loosened. **V-06:** no digital silence in a synthetic case. **V-13:**
  an inferred key is not proof by itself.
- **`CLAUDE.md` §12.5:** a fixture built from the same misunderstanding as the code proves nothing.
  A synthetic case's boundaries come from its construction, never from what the decoder reports.
- **`CLAUDE.md` §0.0:** never present a guess as a decode. **§0.2:** nothing that keys or transmits.
- **HM-DEC-155:** no suite. Run named types only, one per invocation, each with its own `timeout`.
  Captures get 600 s. **Never background and poll.**
- **HM-DEC-165, FACT-004.** **If a package is needed: `MOVE: stop` (§6).**

## 4. Status cadence

At the start of each task, run `sh tools/status.sh EXECUTING "<n> of 3" code none "<one line>"`. At
the end, run it with `COMPLETED`. `SESSION.lock` belongs to the runner, so do not take or release
it. Write nothing to `RUN_LEDGER.md`, and touch nothing under `tools\arbiter\`.

What breaks in this shell:
- Apostrophes in quoted heredocs break, and doubled backslashes collapse.
- `;` is refused, `rm` is refused, and Python cannot run here.
- A multi-line commit uses `-m` more than once.

Scripts go in `.run-unit\unit452-<name>.sh` and run with `sh`. **Never compose a timestamp. Read the
clock.**

---

## 5. The tasks

### Task 0 - the entry

- Add `## UNIT 452 - STEP 6` to `PHASE_OUTCOME.md`, from the decision block at the foot.
- Name 452 in `PHASE_STATUS.md` with `CURRENT_STEP: 6`, and patch-bump the version.
- Commit the entry together with the root's uncommitted `PARKED.md`, `PHASE_OUTCOME.md`,
  `PHASE_STATUS.md` and `RUN_LEDGER.md`. **These are the runner's writes. Commit them as they are,
  without editing them.**
- Run the entry round, and print each result as a number:
  - the build;
  - both carry-forward lines, **each with its wall time**;
  - the three floor tests, with the captures type's wall time;
  - the four metrics per condition, with the key's kind beside each number;
  - the count of files more than 25 Hz off, from 447's printer.
- **Save every recording's decoded text at HEAD** to `.run-unit/unit452-text-before.txt`.

### Task 1 - the measurement, then the trace of every wrong boundary

**First, the measurement, which is 6.3's substance.** Print MET-WBE at HEAD:
- per condition, with inserted and deleted boundaries split and the key's kind beside each number;
- per recording, for every recording with at least one boundary wrong.

Beside the numbers, state for each of HM-REQ-080 and 081, on every condition from §2, one of:
- `met`;
- `not met`, with the number;
- `not measurable here`, with the step that would build the condition.

**Then the trace.** For **every wrong boundary** on the real and synthetic sets, inserted or
deleted, print:
- the recording, the time, and the words either side as the key and as the decoder give them;
- the gap's length in ms and in units of the unit in force;
- the letter and word thresholds in force, and where the read that set them came from;
- the marks either side, and any key-up shorter than half a unit inside them;
- the speed in force and its proof state (unit 451's);
- whether it is inserted or deleted.

For each sender with at least five boundaries, also **print the distribution of its letter spaces
and word spaces against the key**, in units, and say whether they overlap. Unit 444 found that they
overlap on 17:37. A group whose spaces overlap cannot be fixed by duration, so say so rather than
build against it.

**Group the wrong boundaries by what they have in common**, and give each group's size, split real
from synthetic. Commit the printer as a fact that asserts nothing, and commit its output as
`.run-unit/unit452-wbe-trace.txt`.

**Write the measurement into `docs/phase-requirements/metrics.md`**, and commit it on its own:
`unit452 task 1: MET-WBE per condition, HM-REQ-080 and 081 measured (6.3)`.

**Tick 6.3 in both copies of `PHASE_PLAN.md` in that commit, under §3 (a).** The after-figures,
if task 2 keeps a change, are added by task 2.

**Drop candidate:** the per-boundary rows for the synthetic 0 dB cases. Keep their totals, and say
what was dropped. **The measurement and the grouping are never dropped.**

### Task 2 - one change against the largest group that can be moved

- **Choose the group** from the trace: the largest group whose cause is in the audio and not in
  overlapping spacing. Say why you chose it, and name the groups you did not attack.
- **Set the rule from the trace before any numbers are run**, and state it in one sentence.
  - It follows R72: no word or callsign reasoning.
  - It follows §3 (d).
  - Try one rule. **If it is refused, do not try a second value of it.**
- **Build it in its own commit.** Judge it under §3 (b) with this table: every metric per
  condition, before and after, with the key's kind, and the per-recording V-11 column for
  boundaries wrong and for sure-wrong letters. Also print:
  - **17:37's text, before and after**;
  - every recording whose text changed, with its before and after text.
- **If it is kept:**
  - Commit it: `unit452 task 2: <the rule> - MET-WBE <n> to <n> real (6.3)`.
  - Add the after-figures to `metrics.md` beside task 1's.
  - Restate HM-REQ-080 and 081 against them.
- **If it is refused:**
  - Leave `src` as it was.
  - Commit the diff as `.run-unit/unit452-notkept.diff`.
  - Name the line of §3 (b) that refused it.
  - Record the refused figures in `metrics.md`.

### Task 3 - the exit round

- Run the build, both carry-forward lines, the three floor tests, the four metrics, MET-PITCH-ERR
  and every type touched. Print each result as a number, with its wall time.
- **Report what changed in `src`, file by file.** Say that none of it keys or transmits.
- **Ticks:** confirm 6.3, per §3 (a). **Do not tick any other line.**
- **Make the exit commit and push it.**
- Report DRIFT per §3 (e).

---

## 6. Parked - do not touch, do not raise

- **Everything in `PARKED.md`:**
  - step 2's question on which files the transmit check covers. **This unit neither defines nor
    runs a transmit-file list.** It reports its `src` diff file by file;
  - 448's question on what "acquiring" means;
  - 450's questions on when a pitch may be called proved.
- 17:37's floor and 444's section 4 question, which stay with the owner.
- **451's section 4, logged and not chased:**
  - Item 1, the span for which a speed stays proved: 451's reading stands.
  - Item 2, a speed change at one pitch being proved at speeds nobody sent: this is HM-REQ-032's
    line, and belongs to when 5.3 is worked.
- Earlier units' section 4 items.
- **Work that belongs to other lines:**
  - live against settled boundaries (6.4, HM-REQ-083);
  - the named spans (6.5, HM-REQ-084);
  - the character table and the prosigns (6.1, 6.2);
  - the channel profiles (7.1);
  - the speed search (5.1);
  - the traffic-net print (3.4).
- The correctness phase's 5.1, which is Tim's, and all of `PHASE_PLAN.md` §7 (Carried).
- The decision log, the traceability table, and the 43 tests that measure something else. These are
  step 8 (R80).

## 7. What not to do

- Do not change MET-WBE's definition or its scorer (HM-REQ-082). If the scorer looks wrong, report
  it as a mismatch.
- Do not place or remove a boundary on what the letters spell (R72).
- Do not rebuild 444's dropout rule as it stood (§3 (d)). Do not try a second value of a refused
  rule.
- Do not change the pitch path, the tracker, the speed search, or any proof state.
- Do not re-bank any floor. Do not tick 2.5, 3.6, 4.7, 5.6 or 6.6.
- Do not edit `traceability.md`, the decision log, or any line of `PARKED.md`.
- Do not correct any ruling. Report the disagreement.
- Do not add a package. If one is needed, stop and report it.
- Do not halt on a question. Write one line in section 4 and go on.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 8. Committing and pushing

Make one commit per task, and **one commit for the change on its own** whether or not it is kept.
Each message names the criterion it serves: `unit452 task N: <what> (6.3)`. Push to `origin/main`
after each commit. End every commit message with:

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

A. Hamlet meets the CW requirements: step 6 at <n> of 6, step 5 at 1 of 6, step 4 at 5 of 7,
   step 2 at 3 of 5, step 3 at 3 of 6 by the plan's checkboxes, steps 0 and 1 done, 7 and 8 not
   started.
B. Step 6, criterion 6.3 (HM-REQ-080, 081): MET-WBE real <n> over <n> (<ins> inserted, <del>
   deleted), synthetic <n> over <n>, per condition in section 3; HM-REQ-080 <met | not met on
   <conditions>>; HM-REQ-081 <met | not met on <conditions>>; conditions not measurable here <n>;
   the change <kept | refused by <line>>, MET-WBE real <n> to <n>, 17:37 <n> to <n>; 6.3
   <ticked | not ticked>; 6.1, 6.2, 6.4 to 6.6 open.
C. What this report adds, and whether it stands in the way of a criterion in B.
   Section 4 raises <N> items; <which, if any, is in the way of a criterion in B>.
```

```
UNIT:       452 - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <which criteria flipped in PHASE_PLAN.md>
NUMBER:     MET-WBE real <n> -> <n> over 113; synthetic <n> -> <n> over 84; 17:37 <n> -> <n>; 080 <met|not met>; 081 <met|not met>
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1; step 6 <0|1>
```

**Section 3 opens with the per-condition table:** each condition, its key's kind, words, boundaries
inserted, boundaries deleted, MET-WBE, and the verdict on 080 and 081 (met, not met, or not
measurable here). Then give:
- the groups of wrong boundaries, with sizes and one example of each, printed as text with the key
  beside it;
- **17:37 as the operator reads it**, at HEAD and after the change, with its key;
- every recording whose text changed under the change, before and after, as text. The owner reads
  the difference rather than the number;
- the §3 (b) table, including the V-11 column.

**Section 2, in one paragraph:** How often does the operator read a word broken in two, or two words
run together, today? Where is it worst? Did the change make that better, and on which recordings?
Give the evidence (V-13), and say what the synthetic cases do not prove (§12.5).

---

```
ARBITER-DECISION
STEP: 6
APPROACH: report MET-WBE per condition and measure HM-REQ-080 and 081 against it, trace every wrong word boundary on the real and synthetic sets with gap length in units, thresholds, marks and spacing overlap, group by cause, build one boundary change against the largest movable group, kept under R78 with MET-WBE falling and V-11 holding per recording
MOVE: work around
WHY: PHASE_PLAN.md step 6 line 6.3 asks that MET-WBE be reported per condition and HM-REQ-080 and 081 be measured against it, and MET-WBE at 46 over 113 real is the farthest of the four metrics from its requirement, with no unit yet on step 6; step 2 cannot flip a line this pass, since 2.4's count is 0 of 3 after 449's kept change and 2.5 is held by 443's DECIDED (3).
STATE: not started
DECIDED: author's, overrulable - (1) step 6 is worked instead of the launcher's step 2, because 2.4 cannot reach 3 of 3 in one unit and 2.5 is held by 443's DECIDED (3) as 448's DECIDED (6) read it; 6.3 is chosen over 5.3, 5.4 and 7.1 because MET-WBE is the requirement metric farthest from its threshold and its change alters what the operator reads (R83); (2) 6.3 is a measurement and is ticked at task 1 on MET-WBE per condition with the key's kind, 080 and 081 stated met, not met or not measurable here per condition, and the running figure in metrics.md, whether or not task 2's change is kept; (3) task 2's keep rule is R78 with MET-WBE falling on the real set and not rising on the synthetic, letters no worse, and V-11 per recording on boundaries wrong and sure-wrong letters, 17:37 not above 7; (4) if 17:37 reaches 5 or fewer it is reported and not re-banked, and 2.5 and 6.6 are not ticked; (5) 444's dropout rule is not rebuilt as it stood, and a refused rule gets no second value; (6) 451's section 4 is logged and not chased: item 1's reading stands, item 2 belongs to 5.3 and HM-REQ-032; this unit leaves DRIFT for steps 2 to 5 as it is.
LICENCE: PHASE_PLAN.md step 6 line 6.3 and section 5's independence line; R72, R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2), 443 DECIDED (3), 448 DECIDED (3) and (6); V-04; V-06; V-11; V-13; V-14; HM-REQ-010, 011, 080, 081, 082; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: the owner can see, per condition, how often the operator reads a word broken in two or two words run together, against the requirement's zero and five percent, and the commonest cause the audio can fix is fixed if the fix makes no recording worse
ADVANCES: step 6 criterion 3
END-ARBITER-DECISION
```
