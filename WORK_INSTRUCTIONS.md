# Work instruction 462 - one fldigi technique earns its place in ours: the spike rule, the two-dot class edge or the word-space edge, screened on every recording and kept under R78

**Loop unit.** Step 9, criterion 9.4 (`CW_REQUIREMENTS.md` section M, HM-REQ-129; kept under R78
against HM-REQ-010, 011, 012 and 080).

**Why step 9 and not the launcher's step 2.**
- **R86, `PHASE_PLAN.md` section 6:** *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."* The
  owner committed this at `6de324b1`, the newest commit in the tree. The later ruling wins.
- Step 2's only open line, 2.4, is a count of three step-2 units with no kept change. R86 bars
  step-2 units, so 2.4 cannot move now. This unit is not a step-2 unit and does not count toward 2.4.
- 9.4 is the one line of step 9 that everything else waits behind. 9.5 to 9.7 build the vote. R86
  puts 9.4 first.

**What was tried at 9.4, and why this is not that.** Unit 459 traced 18 departures and took one
technique, (D): fldigi's dot-dash pair speed tracking, `cw.cxx` 524-535 and 831-843.
- It was refused under R78 and reverted. It broke the adjudicated `110,` on 031948, and V-11 found
  004507, 031948 and 032050 worse.
- **(D) is not retried here in any form.** Unit 459's section 4 item 2 names the rejected
  narrowing: *a larger edge or a full 16-pair average, after seeing R78's numbers, is tuning a
  constant to the fixture that refused it.*

This unit takes the three techniques 9.3 named that nobody has tried. Each is fldigi's own rule at
fldigi's own ratio, applied at the dit **ours** has in force:
- **(B) the spike rule.** `cw.cxx:515` and `818-822`: a mark shorter than half a dit is not an
  element.
- **(C) the two-dot class edge.** `cw.cxx:846-855`: a mark up to two dits is a dit, and anything
  longer is a dah.
- **(W) the word-space edge.** `cw.cxx:883-914`: a gap of more than two dits and up to four ends a
  character, and a gap of more than four dits is a word space.

**How the unit works:**
1. A trace, run against our decoder's own reads, predicts what each technique would move.
2. The techniques are screened one at a time in the working tree, in the order the trace ranks
   them.
3. The first one R78 keeps goes into its own commit, and 9.4 is ticked.
4. A refused technique is reverted, with its patch and numbers saved. It is never narrowed and
   retried.

**The port under `Cw\Second\` is not touched** (HM-REQ-122, 129).

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
- Python cannot run here.
- A multi-line commit uses `-m` more than once.
- Scripts go in `.run-unit\unit462-<name>.sh` and are run with `sh`.
- To revert a refused screen, use `git checkout -- <paths>` or `git restore`. Never use `rm`.

**The report:**
- Use the four headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- Write the `UNIT:` line without brackets.
- Write `ADVANCES: step 9 criterion 4`. The launcher reads the digits after "criterion", so this
  means plan line 9.4.
- WHY cites the plan.

**R85. A CW question is answered from the documents and fldigi's source, never raised to the
owner.** That covers which dit is "in force", where in our chain a spike is dropped, and what
"ends a character" means inside a lattice. Record your reading in one line of section 4, and
carry on.

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
| 9 | 3 of 8 |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  One technique fldigi's CW modem uses and ours does not - the
            half-dit spike rule, the two-dot dit/dah edge, or the four-dit
            word-space edge - is taken into our decoder in its own commit and
            kept because the requirements' metrics say ours reads better with
            it and no recording reads worse, with the port left as ported.
ADVANCES:   step 9 criterion 4
```

**Read these first. They win over this instruction:**
- `CW_REQUIREMENTS.md` section M (HM-REQ-122, 123, 129), section B (HM-REQ-010 to 013) and
  section I (HM-REQ-080 and 081), and the V-rules, V-11 and V-14 above all;
- `CW_SPEC.md`'s definitions of MET-CER-SURE, MET-INVENTED, MET-COVERAGE and MET-WBE;
- `.run-unit\fldigi\src\cw_rtty\cw.cxx` at `61b97f41`, lines 500-517, 787-921;
- unit 459's report, `.run-unit\reports\unit-6-output-4.md`, sections 1 and 3, for its 18
  departures and its V-11 table;
- `docs/phase-requirements/parity.md`.

**The requirements this unit serves:**
- **HM-REQ-129:** a technique from the second decoder is taken into ours as a change of its own,
  judged under the keep rule, and the second decoder stays as ported.
- **HM-REQ-010 (MET-CER-SURE < 1%), 011 (MET-INVENTED zero), 012 (coverage at least 90%), 080 and
  081 (MET-WBE at most 5%):** the keep rule's metrics.

**The loop test finds no entry for this approach.** The one recorded refusal at 9.4 is unit 459's
approach and technique (D). Neither is repeated here.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

**Check each of these:**
- **HEAD** is `6de324b1` or a runner commit on top of it.
- **`PHASE_PLAN.md`**, in both copies:
  - shows 9.1, 9.2 and 9.3 ticked, and 9.4 to 9.8 open;
  - carries R86's line in section 6.

  If `docs/phase-requirements/PHASE_PLAN.md` differs from the root copy, report it.
- **The port** is at `src\Hamlet.RadioEngine\Cw\Second\`, and `git diff 19109b51 --
  src/Hamlet.RadioEngine/Cw/Second/` prints nothing.
- **Unit 459's tools** are in the tree: `WhereOursLosesWhatThePortKeepsTests`,
  `WhatSetTheSpeedAtEachDeparture` and `TheSpeedFollowsTheSendersMarkPairsTests`. If any is
  missing, say so.
- **`CwUnitEstimator.PairUnit` is not in `src`.** Unit 459 reverted it at `796f9af4`.
- **The runner's uncommitted writes:**
  - the modified `.run-unit` state files, `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and
    `RUN_LEDGER.md`;
  - the deleted root `output.md`;
  - the untracked `.run-unit\reports\unit-8-output-2.md` and `.run-unit\watched.rc`.

  Commit them as they are. **`.run-unit\fldigi\` is untracked. Do not stage it, and do not fetch.**
- **The reload's `RULES_AT` disagreement** (HM-DEC-165 against CPS-DEC-0183) is logged. It is not
  this unit's.
- **`outcome-read`'s step titles** for steps 2, 3 and 8 differ from `PHASE_PLAN.md`'s. That is a
  harness finding. Log it, and do not repair it.

**Entry figures, 461's exit, to compare against:**
- **Build:** 0 errors.
- **Engine line:** 178 of 178.
- **App line:** 278 of 278.
- **Floor tests:** named 13 of 13, captures 51 of 51, adjudicated 13 of 13.
- **Real set, 23 keyed recordings, inferred key:**
  - MET-CER-SURE 33 of 436;
  - MET-INVENTED 33 over 473;
  - coverage 403 over 473;
  - MET-WBE 37 (29 inserted, 8 deleted) over 113.
- **Synthetic set, 12 cases, exact key:**
  - MET-CER-SURE 14 of 173;
  - MET-INVENTED 14 over 252;
  - coverage 159 over 252;
  - MET-WBE 44 (13 inserted, 31 deleted) over 84.
- **Decode time**, ours over 690 s of real audio: about 52 s.

**Expected failures, not regressions:**
- `TheSpeedFollowsTheSendersMarkPairsTests`, red at 28 of 31, as unit 459 left it. It is on neither
  carry-forward line.
- The app line losing a test to Avalonia's headless "dispatcher loop". Handle it under DECIDED (8).

---

## 4. Rulings in force - transcribed, do not re-argue

**The ordering:**
- **R86** (`PHASE_PLAN.md` section 6): *"Steps 2 to 8 are not authorable until 9.4 has a kept
  change. Every unit is step 9's until a technique from the second decoder is kept in ours."*
- **HM-REQ-122 and 129, and section 6:** *"The second decoder is faithful, not improved. A
  technique goes into ours, judged under R78; the second decoder stays as ported."*

**R78, the keep rule.** *"A change is kept when it moves a requirement's metric the right way and
breaks no other requirement. MET-INVENTED at zero, MET-CER-SURE below 1%, MET-COVERAGE at or above
90%, MET-WBE at or below 5%, per condition, at the tier the requirement names. The capture floors
stay as V-11's overfitting guard - no change may redden an earlier capture to green a newer one -
and stop being the keep rule. A capture row's character count falling is a finding to report, not
a rejection, when no requirement's metric got worse."* This unit applies it as follows (DECIDED
(4)). A screened technique is **kept** only if all of these hold:
1. **Per condition.** On the real set and on the synthetic set, none of the four metrics moves the
   wrong way, and at least one moves the right way on at least one of them.
2. **The adjudicated readings.** All 13 are unchanged, or move onto their own adjudicated text
   (R66).
3. **V-11.** On every one of the 35 keyed recordings and cases, none of the four metrics is worse.
   One recording worse on any metric refuses the change. That is how unit 459 was judged, and it
   binds here.

**Standing rulings this unit leans on:**
- **R66.** An adjudicated reading may move onto its own adjudicated text.
- **V-14.** No separation limit, confirmation rule or plausibility bound is loosened to pass a
  fixture.
- **R72.** No word, dictionary or callsign prior (HM-REQ-004, HM-DEC-175).
- **R80.** No traceability, test inventory or decision-log work.
- **R77.** A new CW test names the requirement it serves.
- **R85.** As section 1.

**Standing rules:**
- `CLAUDE.md` §0.0: never present a guess as a decode.
- `CLAUDE.md` §0.2: nothing that keys or transmits. **`KeyerCwSender.cs` and the eleven transmit
  files `PARKED.md` names are not touched, not called and not reused.**
- `CLAUDE.md` §12.5.
- HM-DEC-155, HM-DEC-165 and FACT-004.

---

## 5. Status cadence

Post one line:
- at the start of each task, naming what it will measure or build;
- at each screen's verdict, kept or refused, with its four real and four synthetic numbers;
- at each commit, with its hash;
- if a type runs past its timeout.

Post nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 462 - STEP 9` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 462 with `CURRENT_STEP: 9`, in both copies.
3. Bump the patch from 1.13.148 to 1.13.149.
4. Commit the runner's writes as they are. Check `git status` before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `BothDecodersAreScoredAlikeTests`;
   - the port's own tests.
6. Save both decoders' texts, with each of ours' characters' class:
   - ours to `.run-unit\unit462-text-before.txt`;
   - the port's to `.run-unit\unit462-port-before.txt`.
7. Save the per-recording four-metric table, all 35, to `.run-unit\unit462-v11-before.txt`. Every
   V-11 comparison in this unit is made against this table.

### Task 1 - the trace: what each untried technique would move in ours

This task reads and prints. **Nothing under `src` changes.** Write a fact that asserts nothing:
`WhatFldigisEdgesWouldMoveFact`, serving HM-REQ-129. It prints to
`.run-unit\unit462-edges.txt`.

1. **Where ours makes each decision, with file and line.** Name each of these in our decoder:
   - where a mark is formed from the envelope;
   - where a short mark is discarded, if anywhere, and at what edge in dits;
   - where a mark becomes a dit or a dah;
   - where a gap becomes an element gap, a character gap or a word gap, and at what edge in dits;
   - which dit is "in force" at each of those points.

   Print ours' edge beside fldigi's in dits: 0.5 for a spike, 2 for dit against dah, 2 and 4 for
   the gaps. Where ours makes a decision by likelihood rather than by an edge, say so, and name
   the quantity it weighs.
2. **The what-if, over our own reads of all 35.** Drive every recording exactly as the harness
   drives it, using unit 459's re-spelling of `DecodeAt` where it serves. Then list each event
   below, with its recording, time, length in ms and in dits at the dit in force, and whether it
   sits in a letter the scorer marks right, wrong or added, or at a boundary it marks right,
   inserted or deleted:
   - **(B)** every mark under half the dit in force that ours keeps as an element, and every mark
     at or over half a dit that ours discards;
   - **(C)** every element ours classes against the two-dit edge;
   - **(W)** every gap ours places on the other side of fldigi's 2-dit or 4-dit edge.
3. **Rank the three.** For each technique, count the events in wrong or added letters and wrong
   boundaries, which the technique would fix, against those in right letters and right boundaries,
   which it would put at risk. Rank by fixable minus at-risk, and print the ranking.
   - **A technique with zero fixable events is not screened.** Say so.
   - **Name the three reads that refused (D)**: 031948 `110,`, 004507 `EACH` and 031905
     `PREDICTED`. State which events sit in them. These are the known places a speed-adjacent
     change breaks.
4. **Before any screen, state each technique's one form.** For each technique, write down:
   - where it goes in ours;
   - the edge, at fldigi's ratio;
   - the dit it is measured against.

   Take that form from the trace. **It is fixed from here on** (DECIDED (3)).

**Commit the fact and its printout.**

### Task 2 - the first screen (9.4's own work)

**Build the top-ranked technique in the form task 1 fixed.** It goes into our decoder under
`src\Hamlet.RadioEngine\Cw\`, never under `Cw\Second\`. Where our code follows `cw.cxx`, a comment
names the line at `61b97f41`.

**Judge it in the working tree, before any commit:**
1. Build.
2. Run the adjudicated readings.
3. Run `TheRequirementsAreMeasuredTests`, real and synthetic.
4. Build the per-recording four-metric table and diff it against task 0's.
5. Time the decode over the real set.

Apply R78 exactly as section 4 states.

**If it is kept:**
1. Run the three floor tests and both carry-forward lines.
   - A named floor row that falls on its character count, where that recording's four metrics are
     no worse, is re-banked in the same commit under R78's own sentence. Name the row, its count
     before and after, and its four metrics before and after (DECIDED (5)).
   - A floor row that falls where a metric got worse is not re-banked. That is a V-11 refusal, and
     the change is refused.
2. Commit the change in **its own commit**. It carries only the change, any re-banked floor row,
   and a test naming HM-REQ-129 and the metric it moved.
   - The test is watched failing first at the parent commit, on the recording or synthetic case
     the change moved.
   - If the change moves only real recordings, the test asserts on the real recording.
   - Say how the test was watched.
3. Tick **9.4** in both copies of `PHASE_PLAN.md`, in a follow-on commit. Go to task 4. Task 3 is
   not run once 9.4 is kept.

**If it is refused:**
1. Save the diff to `.run-unit\unit462-<B|C|W>-refused.patch` with `git diff > file`.
2. Save its full numbers and V-11 table to `.run-unit\unit462-<B|C|W>-refused.txt`.
3. Revert the working tree with `git checkout`.
4. Confirm that `git diff -- src` prints nothing. Nothing is committed under `src`.

**Do not narrow, re-edge or re-scope a refused technique** (DECIDED (3)).

### Task 3 - the next screens (drop candidate)

**Only if task 2's technique was refused.** Screen the second-ranked technique exactly as task 2
did. If that is refused too, screen the third-ranked technique the same way. Stop at the first
one kept.

**If all are refused:**
- 9.4 stays open.
- Commit the three saved patches and their numbers under `.run-unit\` as evidence. Nothing under
  `src` is committed.
- The report names, for each refused technique, the recording and metric that refused it.

**This is the drop candidate.** Shed the third screen first, then the second. Tasks 0, 1, 2 and
the exit round are not shed.

### Task 4 - the exit round

**Run the following, and report every figure beside its entry figure:**
- build;
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `BothDecodersAreScoredAlikeTests`;
- the port's own tests;
- `TheSpeedFollowsTheSendersMarkPairsTests`. Report its figure. It is not required green.

**Also print:**
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- the port's texts against task 0's save, which are byte-identical;
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names, which prints nothing;
- our texts against task 0's save:
  - if a technique was kept, every line that moved, before and after, beside the key;
  - if none was kept, identical;
- `git status`, showing `.run-unit\fldigi\` still untracked;
- a table of every commit this unit made, with its results on the five at its exit: build, both
  carry-forward lines, and the three floor tests.

**Ticks:**
- **9.4**, as task 2 says, and only then.
- Do not tick 9.5 to 9.8, and no line of steps 2 to 8.
- If 9.4 is ticked, **R86's bar is lifted by the plan's own wording.** Say so in section 4 as a
  reading, not a ruling.

---

## 7. Parked - do not touch, do not raise

- **Technique (D), fldigi's pair speed tracking,** in any form. It was refused by unit 459.
- **Technique (A), fldigi's detector** (`cw.cxx:610-656`). Unit 459's trace says ours' envelope
  holds 17:37's lost dahs at noise level. It is not this unit's.
- **The four questions `PARKED.md` records, and their `RESOLVED:` answers.**
- **Step 2's decoder work, and 2.4's count.** The count stays 1 of 3.
- **Steps 3 to 8,** under R86.
- **461's section 4:** the 1.0 s lead-in first character, the fading profiles read sure and wrong,
  and the rest. These are step 7's, logged and not chased (DECIDED (7)).
- **459's section 4 items 3 to 5,** and 458's items 2 to 5.
- **6.5's tick, HM-REQ-084's `ABOVE`, and unit 455's two 6.1 findings.**
- **fldigi's squelch default, and its seed speed.**

## 8. Do not

- **Do not change any line under `src\Hamlet.RadioEngine\Cw\Second\`** (HM-REQ-122, 129).
- **Do not change `CwMetrics`, the scorer, `MorseAlphabet` or any test's key.**
- **Do not touch, call or reuse `KeyerCwSender.cs`** or any transmit file (§0.2).
- **Do not commit a refused change under `src`,** even to revert it. Refusals live in the working
  tree and in `.run-unit\` patches.
- **Do not change an edge's ratio away from fldigi's after seeing R78's numbers** (V-14,
  DECIDED (3)).
- Do not screen more than one technique at a time, and do not combine two into one change.
- Do not widen the 8 to 40 WPM speed bounds. That is 5.1's, which unit 446 was refused on.
- Do not re-bank any floor except under DECIDED (5). Do not touch the captures or the adjudicated
  tables.
- Do not take anything that knows words, callsigns or letter frequencies (R72).
- Do not edit `PARKED.md`, `CW_SPEC.md`, `CW_REQUIREMENTS.md`, or any ruling in `PHASE_PLAN.md` or
  `CLAUDE.md`.
- Do not add a new test to either carry-forward line.
- Do not re-point or retire an existing test (R80).
- **Do not install any package. That is `MOVE: stop`.**
- Do not stage, commit or modify `.run-unit\fldigi\`, and do not fetch.
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose a
  timestamp.**

## 9. Committing and pushing

**One commit per task, and the kept change alone in its own:**
- task 0;
- task 1's fact and printout;
- the kept change, if any, with its test and any re-banked row;
- the 9.4 tick;
- task 3's refusal evidence, if any;
- task 4.

**Messages** take the form `unit462 task N: <what> (9.4)`. The kept change's message names the
technique, the `cw.cxx` lines, and the four real and four synthetic numbers before and after.

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
   7 2 of 5, 8 0 of 6, 9 <n> of 8; steps 2 to 8 <still barred | released>
   by R86.
B. Step 9, criterion 9.4: HM-REQ-129 - techniques screened <list of B, C,
   W in order>, each <kept | refused on <recording, metric> | not
   screened, zero fixable>; the kept one <technique, cw.cxx lines>: real
   (inferred) MET-CER-SURE 33 of 436 to <a> of <s>, MET-INVENTED 33 to
   <b>, coverage 403 to <c>, MET-WBE 37 to <d>; synthetic (exact) 14, 14,
   159, 44 to <...>; adjudicated <n> of 13; V-11 <0 | n> of 35 worse; the
   port byte-identical <yes | no>; 9.4 <ticked | open>.
C. The findings weighed against A and B: how many items section 4 raises,
   and whether any is in the way of 9.4 or of 9.5, which comes next.
```

```
UNIT:       462 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-129 <met | not met>; real MET-CER-SURE 33/436 to <a>/<s> (inferred); synthetic 14/173 to <x>/<y> (exact); MET-INVENTED real 33 to <b>; coverage real 403 to <c>; MET-WBE real 37 to <d>; the port byte-identical <yes|no>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 7 <n>; step 9 <n>
```

**Section 3 leads with the screen table.** For each technique screened, one row with:
- the four real and four synthetic numbers, before and with it in;
- adjudicated;
- V-11's count of recordings worse, naming each one;
- decode time;
- the verdict.

Then give:
1. task 1's table of ours' edges beside fldigi's, and the ranking with fixable and at-risk counts;
2. every recording the kept change moved: key, ours before, ours after, and the port, as unit
   459's section 3 printed them;
3. how the kept change's test was watched failing first;
4. the commit table, with the five results at each commit.

**Section 2, one paragraph.** If a technique was kept, say which lines on which recordings now read
differently, in the owner's words, as letters he would see. If none was kept, say the screen reads
exactly as before, and which technique came closest and what stood in its way.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: trace ours against fldigi's untried edges - half-dit spike rule, two-dot dit/dah class edge, four-dit word-space edge - rank by fixable against at-risk events, then screen each in the working tree at fldigi's own ratio in rank order and commit the first kept under R78 as its own change, the port untouched
MOVE: work around
WHY: PHASE_PLAN.md section 6's R86 bars steps 2 to 8 until 9.4 has a kept change, so 9.4 is the only line that can move and step 2's 2.4 cannot; unit 459's one 9.4 attempt took technique (D), pair speed tracking, and was refused under R78, so this unit works around it with the three techniques 9.3 named that nobody has tried, each at fldigi's fixed ratio and screened before any commit, and the loop test finds no entry for it.
STATE: partial
DECIDED: author's, overrulable - (1) step 9 criterion 9.4 is worked instead of the launcher's step 2, on R86 in PHASE_PLAN.md section 6, the owner's commit 6de324b1, which bars steps 2 to 8 until 9.4 has a kept change; this is not a step-2 unit and 2.4's count stays at 1 of 3; (2) the techniques are the three 9.3 and unit 459's grouping named and nobody tried - (B) cw.cxx 515 and 818-822, (C) 846-855, (W) 883-914 - and (D) is not retried in any form, while (A) is left because 459's trace says our envelope holds the lost dahs at noise; (3) each technique takes fldigi's own ratio against the dit ours has in force, in one form task 1 fixes before any screen, and a refused technique is never narrowed or re-edged after its numbers are seen (V-14, 459 section 4 item 2); (4) R78 is applied as 459 was judged - kept only if on both the real and synthetic sets none of the four metrics moves wrong and one moves right, the 13 adjudicated readings hold or move onto their own text (R66), and no one of the 35 recordings is worse on any metric (V-11); (5) a named floor row that falls on character count alone where its recording's four metrics are no worse is re-banked in the kept commit under R78's own sentence, named with its metrics before and after, and one that falls with a metric worse refuses the change; (6) screens are judged in the working tree and a refused one is reverted with its patch saved under .run-unit, so nothing refused is committed under src, and the kept one is committed alone as HM-REQ-129 asks; (7) 461's section 4 is logged, not chased - its lead-in and fading findings are step 7's, which R86 bars; (8) the app line's headless dispatcher-loop loss: one rerun, and any type lost again is run alone and must pass, named in the report.
LICENCE: PHASE_PLAN.md step 9 lines 9.3 and 9.4, section 6 (R86, R78, the second decoder is faithful, V-14), R66, R72, R77, R80, R85; CW_REQUIREMENTS.md HM-REQ-010, 011, 012, 080, 081, 122, 123, 129, V-11, V-14; fldigi cw.cxx at 61b97f41 lines 500-517 and 787-921; unit 459's report sections 1, 3 and 4; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: one thing fldigi's receiver does that ours does not - throwing away blips shorter than half a dit, calling a mark a dah only past two dits, or calling a word space only past four - is in Hamlet's decoder because it made the text read better with no recording reading worse, which is what the owner's R86 asks before any other decoder work resumes
ADVANCES: step 9 criterion 4
END-ARBITER-DECISION
```
