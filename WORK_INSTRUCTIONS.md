# Work instruction 459 - one technique from the fldigi port taken into our decoder, judged under R78

**Loop unit.** Step 9, criterion 9.4 (HM-REQ-129). 9.1, 9.2 and 9.3 are ticked in the plan.
`docs/phase-requirements/parity.md` tables both decoders on 23 keyed and 12 synthetic
recordings. `.run-unit\unit458-where-it-wins.txt` names, from `cw.cxx`, what the port does
differently on the 13 recordings where it scores better.

**What those 13 say about our decoder.** Six are "wins" only because the port printed less. On the
rest, the port keeps elements that ours loses: the dits of `2,` on 031838 and of `OU` on 004234,
and the dahs of R and D on 17:37. It also holds a speed that ours overshoots: ours ran at 40 WPM
on 003758 and cut `AA4` to `EET`. Each of these is a sure character printed wrong, which is what
HM-REQ-010 counts. This unit takes **one** of the port's techniques into **our** decoder, as a
change in its own commit, judged by R78's keep rule. The port stays as ported.

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

**HM-DEC-155.** No suite. Run named types only, one per invocation, each with its own `timeout`.
Captures get 600 s. Never run in the background and poll. If one type needs more than 600 s, split
it by recording into several named types or filters. Never raise the timeout past 600 s.

Apostrophes in quoted heredocs break. Doubled backslashes collapse. `;` is refused. `rm` is
refused. Python cannot run here. A multi-line commit uses `-m` more than once. Scripts go in
`.run-unit\unit459-<name>.sh` and are run with `sh`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. Write the `UNIT:` line without
brackets. Write `ADVANCES: step 9 criterion 4`. WHY cites the plan.

**A CW question is answered from the documents and from fldigi's source, never raised to the
owner** (R85). Section 4 records the reading in one line, and the work goes on.

---

## 2. Why this unit exists

**The count today.**

| step | met |
|---|---|
| 0 | done |
| 1 | done |
| 2 | 3 of 5 |
| 3 | 3 of 6 |
| 4 | 5 of 7 |
| 5 | 1 of 6 |
| 6 | 3 of 6 |
| 7 | 0 of 5 |
| 8 | 0 of 6 |
| 9 | 3 of 8 |

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  One technique the fldigi port uses where it reads better than
            ours - its mark detection, spike rule, element classing, speed
            tracking or gap rule, whichever the trace shows behind the most
            elements our decoder loses - is built into our decoder in its own
            commit and kept under R78, so fewer sure characters are printed
            wrong, with FldigiCwDecoder left byte-for-byte as ported.
ADVANCES:   step 9 criterion 4
```

**Read `CW_REQUIREMENTS.md` section M, section B and `CW_SPEC.md`'s metric definitions first.
They win over this instruction.**

- **HM-REQ-129 (must):** *"Where a technique of the second decoder is taken into the first, it is
  taken as a change to the first, judged as any other change, and the second decoder is left as
  ported."*
- **HM-REQ-010 (must):** *"...the decoder shall keep MET-CER-SURE below 1 %."* The real set stands
  at 33 of 436 (7.6 %) and the synthetic set at 14 of 173.
- **HM-REQ-012 (must):** coverage at or above 0.90. A change that meets 010 by printing less is not
  a kept change.
- **HM-REQ-122:** the second decoder is faithful. Nothing under `Cw\Second\` changes.

**Why step 9, and why 9.4.**
- 9.4 is step 9's first open line. It is also the line of section M that changes what the operator
  reads: 9.5 to 9.7 build calibration and voting, while 9.4 improves our decoder, which reads
  alone until 9.6.
- It is aimed at the same number as step 2, sure characters printed wrong, with a source of
  techniques that no step-2 unit had.
- **Step 2 cannot flip a line this pass.** 2.4 counts consecutive step-2 units with no kept
  change. 449's kept change reset it to 0, and no step-2 unit has run since. 2.5 is held red by
  443's DECIDED (3).
- **The loop test** on this approach finds no entry. No unit has taken a technique from the port
  into ours. 446's widened speed search (5.1, refused) changed the search bounds. It did not take
  up the port's pair tracking, which is a different mechanism.

---

## 3. Verify against the tree

**Report mismatches rather than repair them.** Where this instruction and the tree disagree, the
tree is the fact. Say so in section 1, and carry on as far as the tree allows.

- **HEAD** is `19109b51` or a runner commit on top of it.
- **`docs\phase-requirements\parity.md`** exists with its five parts.
- **`BothDecodersAreScoredAlikeTests`** is green, 5 of 5.
- **`.run-unit\unit458-where-it-wins.txt`** lists 13 recordings and mechanisms (A) to (G), with
  `cw.cxx` lines at `61b97f41`.
- **`TheSecondDecoderIsAFaithfulPortTests`** is green, 8 of 8.
- **`.run-unit\fldigi\`** is untracked. Do not stage it, and do not fetch.
- **The runner's uncommitted writes** are the modified `.run-unit` state files, `PHASE_OUTCOME.md`,
  `PHASE_STATUS.md` and `RUN_LEDGER.md`, plus the untracked `.run-unit\reports\unit-5-output-4.md`
  and `.run-unit\watched.rc`. They are the runner's. Commit them as they are, following the
  precedent of units 453 to 458.
- **The reload's `RULES_AT` disagreement** (HM-DEC-165 against CPS-DEC-0183) is logged. It is
  not this unit's.

**Entry figures, 458's exit, to compare against:**
- **Real, inferred:** MET-CER-SURE 33 of 436, MET-INVENTED 33 over 473, coverage 403 over 473,
  MET-WBE 37 (29 inserted, 8 deleted) over 113.
- **Synthetic, exact:** MET-CER-SURE 14 of 173, MET-INVENTED 14 over 252, coverage 159 over 252,
  MET-WBE 44 (13 inserted, 31 deleted) over 84.
- **Adjudicated readings:** 13 of 13.

**Expected failures, not regressions:**
- the three red named floors: 17:37 at 38 of 46, 032113 at 43 of 45 and 032129 at 42 of 64;
- the app carry-forward line losing a test to Avalonia's headless "dispatcher loop" (see
  DECIDED (6)).

---

## 4. Rulings in force - transcribed, do not re-argue

- **R84, and section M of `CW_REQUIREMENTS.md` v1.1.** Two decoders read the same audio.
  - Rejected: the port as a bench instrument only.
  - Rejected: replacing ours with the port.
  - **The order is fixed:** scored beside ours (123), then calibrated (124), and only then does it
    vote (125 to 128). HM-REQ-129 allows a technique to be taken into ours at any point, as a change
    to ours. **This unit is 129 and nothing else.**
- **HM-REQ-122 and 129.** Faithful, not improved. The teacher is not edited to match the student.
  No line of `Cw\Second\` changes in this unit.
- **R78, the keep rule.** A change is kept when all of these hold:
  - MET-CER-SURE falls on the real set;
  - MET-INVENTED does not rise;
  - coverage does not fall;
  - MET-WBE does not rise (443's DECIDED (3): R78 names MET-WBE among its metrics);
  - the synthetic set is not worse on any of the four;
  - the adjudicated readings are unchanged, or move onto their own adjudicated text (R66);
  - **V-11** holds: no recording is made worse on a requirement metric to improve another.

  A capture row's character count falling is a finding to report, not a rejection.
- **R72.** No word, dictionary or callsign prior (HM-REQ-004, HM-DEC-175). The technique is taken
  from the port's signal path, never from its table or anything that knows words.
- **V-04 and V-14.** No fixture is altered, re-seeded or dropped. No gate, separation limit,
  confirmation rule or plausibility bound is loosened to pass a fixture.
- **V-13.** A real recording's key is inferred unless it was transcribed. Every number carries its
  key's kind.
- **R75 and R76.** The tone tracker is open, and the pitch instrument is built. If the chosen
  technique touches `CwToneTracker`, the instrument's table goes beside it, and 4.4 is named as
  touched but not ticked.
- **R77.** A new CW test names the requirement it proves.
- **R80.** No unit is authored for bookkeeping. No traceability or test-inventory work.
- **R85.** A CW question is answered from the documents and the second decoder's source.
- **443 DECIDED (3), as 448 DECIDED (6) reads it.** No floor is re-banked while 17:37's
  boundaries are worse. 2.5, 6.6 and 9.8 are not ticked in this unit.
- **458 DECIDED (2), the mapping.** Every non-space character the port prints is sure, `*` is a
  placeholder, and spaces are word boundaries. It stands. `parity.md` is re-run with it.
- **456 DECIDED (2) to (5).**
  - The port uses fldigi's shipped `progdefaults` at `61b97f41`.
  - The caller supplies the carrier.
  - Resampling sits outside the ported files.
  - `.run-unit\fldigi\` stays untracked.
  - The port is wired to nothing the operator sees.
- **`CLAUDE.md` §0.0.** Never present a guess as a decode.
- **`CLAUDE.md` §0.2.** Nothing that keys or transmits.
- **`CLAUDE.md` §12.5.** A fixture built from the same misunderstanding as the code proves
  nothing. Keys come from where the metrics already take them.
- **HM-DEC-155, HM-DEC-165, FACT-004.**

---

## 5. Status cadence

Post one line at the start of each task, naming the task and what it is about to measure or
build. Post one line at each commit, with its hash. Post one line if a type runs past its
timeout. Nothing between those.

---

## 6. The tasks

### Task 0 - the record and the entry numbers

1. Add `## UNIT 459 - STEP 9` to `PHASE_OUTCOME.md`, from the block at the foot.
2. Set `PHASE_STATUS.md` to name 459 with `CURRENT_STEP: 9`, in both copies.
3. Bump the patch: 1.13.145 to 1.13.146.
4. Commit the runner's writes as they are. Do not stage `.run-unit\fldigi\`. Check `git status`
   before each commit.
5. Run the entry round:
   - build;
   - both carry-forward lines;
   - the three floor tests;
   - the adjudicated readings;
   - `TheRequirementsAreMeasuredTests`, real and synthetic;
   - `TheSecondDecoderIsAFaithfulPortTests`;
   - `BothDecodersAreScoredAlikeTests`.
6. Save every recording's text from our decoder, with each character's class, to
   `.run-unit\unit459-text-before.txt`. Save the port's texts to
   `.run-unit\unit459-port-before.txt`.

### Task 1 - the trace: where ours loses the element the port keeps

This task reads and prints. It changes nothing in `src`.

Take every departure in `unit458-where-it-wins.txt` where **ours printed a sure character wrong
and the port read that character right**. The known ones are:
- 17:37 R and D;
- 031838 `2` and `,`;
- 004234 O and U;
- 003758 A, A and 4.

Add any others the printout holds, and any sure-wrong character on the other keyed recordings
where the port reads it right. **Leave out** the recordings where the port wins only by printing
less (17:37's opening, 032129, 004108 and the 0 dB cases). No technique is taken to print less.

For each departure, print to `.run-unit\unit459-trace.txt`, from our decoder's own evidence:
- every mark and gap under the character in samples and in our units, with the unit, speed and
  pitch in force;
- our thresholds for mark or no-mark, dit or dah, and element, character or word gap at that
  moment;
- where the missing element went. Say which of these it was:
  - (a) never keyed as a mark;
  - (b) keyed and thrown away as a spike;
  - (c) keyed and classed as the other element;
  - (d) present, but the character was ended before it by a gap rule or a speed too high.

Beside each departure, print the port's representation, speed and `two_dots`. Name the mechanism,
(A) to (E) of 458's list, that keeps that element in the port, citing the `cw.cxx` line.

**Group the departures by (a) to (d) and name the largest group.** The technique for task 2 is
the port mechanism behind that group. State in one line why it and not the next group.

### Task 2 - the technique, in our decoder, under R78 (9.4 ticked here if kept)

Build the one mechanism from task 1 into our decoder, in the form our pipeline takes it. It is
our code, written for our decoder and citing the `cw.cxx` lines it follows. Ours keeps its
classes (sure, dim, placeholder); the technique does not make ours print more than it can
support (HM-REQ-012 is not bought with HM-REQ-010).

1. **Watch it fail first.** Write a test naming HM-REQ-010 and HM-REQ-129 on a synthetic case
   whose key is exact by construction, built from task 1's evidence and not from the change: the
   element pattern ours loses, for example a dit at the edge of our threshold inside a character.
   It must be red at HEAD. Commit it red.
2. **Build the change in its own commit.** Nothing under `Cw\Second\` changes.
3. **Judge it** by the keep rule in section 4:
   - `TheRequirementsAreMeasuredTests`, real and synthetic, all four metrics, before and after;
   - the adjudicated readings;
   - V-11 per recording on the four metrics, over every keyed recording;
   - decode time before and after;
   - `BothDecodersAreScoredAlikeTests` re-run, so `parity.md` carries our new row beside the port's
     unchanged one.
4. **Kept:** the change stands. Add MET-CER-SURE before and after, per condition with the key's
   kind, to `docs/phase-requirements/metrics.md`. Tick **9.4** in both copies of `PHASE_PLAN.md`.
5. **Not kept:** revert it in a commit of its own. Report the numbers that refused it, recording by
   recording.

**Print, before and after,** the text of every departure's stretch from task 1: key, ours before,
ours after, port. The owner reads the letters, not only the number.

### Task 3 - the second group (drop candidate)

**Only if task 2's change was not kept.** Take the next-largest group of task 1, whose mechanism
differs from task 2's, and do task 2 again with it: test red first, the change in its own commit,
judged under the same rule, and reverted in its own commit if refused.

If task 2 was kept, skip this task and say so. A second technique is not stacked on a kept one in
the same unit.

**This is the drop candidate.** If time is short, it is shed first. Tasks 0 to 2 and the exit round
are not shed.

### Task 4 - the exit round

Run each of the following, and report every figure beside its entry figure:
- both carry-forward lines;
- the three floor tests;
- the adjudicated readings;
- `TheRequirementsAreMeasuredTests`, real and synthetic;
- `TheSecondDecoderIsAFaithfulPortTests`;
- `BothDecodersAreScoredAlikeTests`;
- the test from task 2, and from task 3 if run.

Also print:
- `git diff 7e209cb4` over the eleven transmit files, which prints nothing;
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`, which prints nothing;
- the port's texts against task 0's save, byte-identical;
- our decoder's texts against task 0's save: every changed recording, before and after;
- `git status`, showing `.run-unit\fldigi\` still untracked.

If a named floor moved, say which way and by how much. Do not re-bank it.

---

## 7. Parked - do not touch, do not raise

- **2.2's transmit-file list** (keying).
- **4.5's definition of acquiring** (promise).
- **4.6's proved pitch** and **5.5's proved speed** (promise).
- **6.5's tick and HM-REQ-084's `ABOVE`** on 013637.
- **Unit 455's two 6.1 findings:** KN's pattern is also `(`, and a missing prosign is dropped at
  the end of a send.
- **fldigi's squelch default in `status.cxx`**, which is not in the tree. The squelch stays off.
- **457's section 4 item 1, the reading on which 9.1 was ticked.** The state reader judged it not
  honestly supported. The tick stands until the owner unticks it. Logged here, not re-worked.
- **457's section 4 item 2**, the six keys the control printer did not locate.
- **458's section 4 items 2 to 5.** The reading on which 9.3 was ticked, `CwScorer.Within`'s
  overlapping stretches, the app line's lost tests and `parity.md`'s wall-clock rows. All are
  logged, not chased.

## 8. Do not

- Do not change any line under `src\Hamlet.RadioEngine\Cw\Second\` (HM-REQ-122, 129).
- Do not change `CwMetrics`, the scorer, `MorseAlphabet` or any key.
- Do not take more than one technique into our decoder in a kept state.
- Do not take anything that knows words, callsigns or the character table's frequencies (R72).
- Do not raise the speed search bounds. That is 5.1, refused once as 446.
- Do not alter, re-seed, trim, pad or drop any recording or synthetic case (V-04, V-14).
- Do not build calibration, voting or arbitration (9.5 to 9.7). Do not wire the port to anything
  the operator sees.
- Do not install any package. That is `MOVE: stop` in the plan's section 6.
- Do not stage, commit or modify `.run-unit\fldigi\`. Do not fetch from the network.
- Do not re-bank any floor. Do not tick 2.5, 6.6 or 9.8 (443 DECIDED (3)).
- Do not re-point or retire an existing test (R80).
- Do not raise a CW question to the owner (R85).
- **Do not run an unfiltered `dotnet test`. Never run in the background and poll. Never compose
  a timestamp.**

## 9. Committing and pushing

- **One commit per step.** Task 0 is one commit and task 1 one. Task 2 is the red test, then the
  change, then either the metrics and tick or the revert. Task 3, if run, follows task 2's shape.
  Task 4 is one commit.
- **Message form:** `unit459 task N: <what> (9.4)`.
- **Exit state:** the build and the named types are green at the exit of every commit, except:
  - the three floors, held red as stated;
  - the watched-red test commits.
- **Push each commit** to `origin/main`.

## 10. Report

Write `output.md` at the root with the four headings exactly. **The ordering block comes
first.** `validate-output.bat` refuses a report without it.

```
READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 3 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 0 of 5, 8 0 of 6, 9 <n> of 8.
B. Step 9, criterion 9.4: HM-REQ-129 - the technique taken from cw.cxx
   <lines>, against task 1's largest group <(a)-(d)>, <n> of <m>
   departures; real MET-CER-SURE <before> to <after> of sure, MET-INVENTED
   <b> to <a>, coverage <b> to <a>, MET-WBE <b> to <a>; synthetic the same;
   adjudicated <n> of 13; V-11 <n> of <m> worse; <kept | refused>; 9.4
   <ticked | open>; the port byte-identical <yes | no>; 9.5 to 9.8 as they
   stand.
C. The findings weighed against A and B: how many items section 4 raises,
   and whether any is in the way of 9.4, 9.5 or step 2's MET-CER-SURE.
```

```
UNIT:       459 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   <yes|no> - <the criterion and why>
NUMBER:     HM-REQ-129 <met|not met>; HM-REQ-010 real MET-CER-SURE <b>/<sure> to <a>/<sure> (inferred), synthetic <b> to <a> (exact); coverage real <b> to <a>; MET-INVENTED real <b> to <a>; the port byte-identical <yes|no>
DRIFT:      step 2 <n>; step 3 <n>; step 4 <n>; step 5 <n>; step 6 <n>; step 9 <n>
```

**Section 3 leads with the before-and-after text** of every departure task 1 traced: key, ours
before, ours after, port. Then give:
1. task 1's grouping, (a) to (d), with counts and the `cw.cxx` line behind each;
2. the per-condition table of the four metrics, before and after, with key kind and decode time;
3. V-11 per recording: every recording that moved on any metric, and which way;
4. the three named floors before and after;
5. if task 3 ran, the same for its technique.

**Section 2, one paragraph:** say what the operator will see on the recordings that changed, in
letters. If nothing was kept, say that nothing changes, and why the number refused it.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: trace every sure letter ours prints wrong where the fldigi port reads it right, classify the lost element as never keyed, spiked, misclassed or cut by a gap or speed rule, then build the port mechanism behind the largest group into our decoder in its own commit, kept under R78, the port untouched
MOVE: continue
WHY: PHASE_PLAN.md step 9 line 9.4 (HM-REQ-129) is step 9's first open line and the one that changes what the operator reads, aimed at HM-REQ-010's sure-but-wrong letters with evidence 9.3 put in the tree. No approach has been recorded against it and the loop test finds none. Step 2 cannot flip a line this pass: 2.4's count is 0 after 449's kept change, and 2.5 is held by 443's DECIDED (3).
STATE: partial
DECIDED: author's, overrulable - (1) step 9 is worked instead of the launcher's step 2, on the plan's section 5 independence of steps and 9.4 being the next open line of the preferred step; (2) the technique is chosen by task 1's trace, the port mechanism behind the largest group of lost elements, and not by the report's six print-less wins, which no technique is taken to reproduce; (3) the keep rule is R78 with MET-WBE not rising, the synthetic set not worse, adjudicated 13 of 13 or moved onto their own text, and V-11 per recording on the four metrics, and 9.4 is ticked only on a kept change; (4) if the first technique is refused, a second from a different group is the drop candidate, and a second technique is never stacked on a kept one in the same unit; (5) 458's DECIDED (2) mapping and 456's DECIDED (2) to (5) stand, and parity.md is re-run with the port's row unchanged; (6) the app line's headless dispatcher-loop loss: if the one rerun also loses a test, each lost type is run alone and must pass, and the report names them; (7) 457's section 4 items 1 and 2 and 458's items 2 to 5 are logged, not chased, and a kept change here is reported as bearing on step 2's MET-CER-SURE without ticking any step 2 line.
LICENCE: PHASE_PLAN.md step 9 line 9.4 and sections 5 and 6; CW_REQUIREMENTS.md section M HM-REQ-122 and 129, section B HM-REQ-010 and 012, HM-REQ-004; CW_SPEC.md metric definitions; R66, R72, R75, R76, R77, R78, R80, R84, R85; arbiter rulings 443 DECIDED (3), 448 DECIDED (6), 456 DECIDED (2) to (5), 458 DECIDED (2); V-04, V-11, V-13, V-14; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5; GPL-3
ACCOMPLISHED: Hamlet's own decoder borrows one proven piece of fldigi's receiver, the one that keeps the dits and dahs ours drops, so letters like the R and D of WB6RED or the 2 of a propagation bulletin stop printing confidently wrong, with fldigi's copy left untouched as the reference
ADVANCES: step 9 criterion 4
END-ARBITER-DECISION
```
