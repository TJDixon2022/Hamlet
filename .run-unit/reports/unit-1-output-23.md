READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 2 of 5, 8 0 of 6, 9 3 of 8; steps 2 to 8 still barred by R86.
B. Step 9, criterion 9.4: HM-REQ-129 - techniques screened C, B, W in
   that order. C refused on 031838, MET-WBE. B refused on 031905,
   MET-CER-SURE, with every real metric worse. W refused on 032113,
   coverage, and on 031838 and 004550, MET-WBE. None kept: real
   (inferred) MET-CER-SURE 33 of 436, MET-INVENTED 33, coverage 403 and
   MET-WBE 37 unchanged; synthetic (exact) 14, 14, 159, 44 unchanged;
   adjudicated 13 of 13; V-11 0 of 35 worse at exit (C 1, B 3, W 3 of
   35 while screened); the port byte-identical yes; 9.4 open.
C. The findings weighed against A and B: section 4 raises 5 items. Item
   1 is in the way of 9.4. W moved every total on both sets the right way
   and was refused only by V-11's per-recording test as DECIDED (4)
   reads it. Whether that reading is R78's is the owner's question.
   Item 2 bears on how the next 9.4 attempt is chosen. None is in the
   way of 9.5, which R86 does not yet reach. None asks anything about
   keying, transmit, or what the product tells the operator.

UNIT:       462 - complete at task 4 of 4, none dropped - 2026-09-27 00:20
PHASE GOAL: Hamlet's CW decoder meets every requirement in CW_REQUIREMENTS.md at the condition each names, proved by a test that names it. For now the plan routes everything through step 9: fldigi's receiver beside ours, until one of its techniques earns a place in ours.
UNIT GOAL:  Find where ours decides spikes, dit against dah, and letter and word gaps. Measure what fldigi's three untried edges would move on our own reads. Screen them one at a time at fldigi's own ratios, and keep the first one R78 says makes ours read better with no recording reading worse, the port untouched.
ADVANCED:   no - 9.4 is not ticked. All three techniques were screened and refused under R78 and reverted. W came closest: it moved all eight totals the right way, and V-11 found 032113, 031838 and 004550 worse.
NUMBER:     HM-REQ-129 not met; real MET-CER-SURE 33/436 to 33/436 (inferred); synthetic 14/173 to 14/173 (exact); MET-INVENTED real 33 to 33; coverage real 403 to 403; MET-WBE real 37 to 37; the port byte-identical yes
DRIFT:      step 2 1; step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 2 (was 1)

## 1. What Claude did

**Complete at task 4 of 4, none dropped.** Task 3, the drop candidate, ran: it screened B and W.

- **Where.** Development machine, Claude Code. `PROJECT: Hamlet` confirmed by the gate: the four
  files are present, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`.
  Branch `main`, pushed after every commit.
- **The lock.** `SESSION.lock` was already there (PID 40900, 23:02:00). It is the launcher's. The
  session neither took it nor released it, and did not commit it.

**The tree against the instruction.** Mismatches are reported, not repaired.
- **Matched.**
  - HEAD was `6de324b1`.
  - Both copies of `PHASE_PLAN.md` are identical: 9.1 to 9.3 ticked, 9.4 to 9.8 open, and R86 in
    section 6.
  - `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` printed nothing.
  - 459's three tools are in the tree. `WhatSetTheSpeedAtEachDeparture` is a method of
    `WhereOursLosesWhatThePortKeepsTests`, not a type.
  - `CwUnitEstimator.PairUnit` is not in `src`.
- **The runner's files.** There was no `.run-unit/watched.rc`. `.run-unit/watched.cpu` was
  deleted at the start; the runner then wrote it again during the session, so it is committed as it
  stands. `WORK_INSTRUCTIONS.md` was modified by the runner and is committed as it was.
- **Both copies of `PHASE_STATUS.md` differ beyond the fields task 0 sets.** The docs copy has no
  `HEARTBEAT` line and no `STEP: 9` line, and shows steps 2 to 7 as `not started`. The root copy
  shows them `partial`. Both now name 462 at `CURRENT_STEP: 9`; the rest was not repaired.
- **Logged as the instruction asks, and not re-checked or repaired.** The `RULES_AT` disagreement
  (HM-DEC-165 against CPS-DEC-0183), and `outcome-read`'s step titles for steps 2, 3 and 8.
- **The carry-forward lines** were read from `docs/carry-forward-tests.txt` (178 and 278), as 461
  did, because `tools/run-carry-forward.sh` still carries an older list. There was no dispatcher-loop
  loss at entry or at exit, so DECIDED (8) was not needed.

**Task 0 - the record and the entry figures** (`258c95e6`).
- **The record.** `## UNIT 462 - STEP 9` is in both copies of `PHASE_OUTCOME.md`. `PHASE_STATUS.md`
  names 462 at `CURRENT_STEP: 9` in both copies. The version went from 1.13.148 to 1.13.149. The
  runner's writes were committed as they were; `.run-unit/fldigi/` was not staged.
- **The entry figures, every one as at 461's exit.**
  - build 0 errors;
  - engine line 178 of 178; app line 278 of 278;
  - named 13 of 13; captures 51 of 51; adjudicated 13 of 13;
  - both sets' four metrics as in section 3;
  - `BothDecodersAreScoredAlikeTests` 5 of 5; the port 8 of 8;
  - ours 51.65 s over 690 s of real audio.
- **The saves.**
  - Ours: 126 lines in `.run-unit/unit462-text-before.txt`.
  - The port: 74 lines in `.run-unit/unit462-port-before.txt`, byte-identical to 459's save.
  - V-11: the 35-row table in `.run-unit/unit462-v11-before.txt`.
- **A mismatch in the saves.** The tree's text printer
  (`WhereTheSureAddedLettersComeFromTests.EveryRecordingAsTheOperatorReadsIt`) prints text without
  classes. Each character's class, and the scorer's verdict on it, come from task 1's fact, which
  drives the same harness. They are in `.run-unit/unit462-text-before-classes.txt`.
- The parity run rewrites the decode-time rows of `docs/phase-requirements/parity.md` each time. A
  copy was kept under `.run-unit` each time, and the committed file was restored.

**Task 1 - the trace** (`d616e680`).
- **The fact.** `WhatFldigisEdgesWouldMoveFact` (HM-REQ-129) asserts nothing and changed nothing
  under `src`. Its printout is `.run-unit/unit462-edges.txt`.
- **How it drives.** It drives all 35 through 459's `Drive`, exactly as the metrics do. It finds
  every settled letter on 459's copy of `DecodeAt`: 908 of 908 reads spelled alike, and 1032 of
  1032 letters were found. Every letter and every boundary carries the scorer's own verdict from
  `TheRequirementsAreMeasuredTests`' alignments.
- **What it found.** Ours makes no edge decision at all except the shortest span. Dit against dah,
  and every gap class, is decided by likelihood. The ranking and the three forms are in section 3.

**Task 2 - the first screen, C** (`fa5225b9`, evidence only).
- **Refused.** It was built into both lattices through one `Spans` helper and judged in the working
  tree. The real set's MET-WBE went from 37 to 38, and V-11 found 031838 worse on MET-WBE.
- **Reverted.** `git diff -- src` printed nothing afterwards. The patch and full numbers are in
  `.run-unit/unit462-C-refused.patch` and `.txt`.

**Task 3 - B, then W** (`7fc1b56a`, evidence only).
- **B was refused.** All four real metrics got worse, and V-11 found three recordings worse.
- **W was refused.** All eight totals moved the right way, but V-11 found three recordings worse.
- **Both reverted.** `git diff -- src` printed nothing after each. The evidence is in
  `.run-unit/unit462-B-refused.*` and `.run-unit/unit462-W-refused.*`.
- **Nothing was narrowed, re-edged or combined.**

**Task 4 - the exit round.** Every figure is as at entry (section 3).
- 459's `TheSpeedFollowsTheSendersMarkPairsTests` is red at 28 of 31, as 459 left it.
- 9.4 is not ticked, and no other line is.

**Decisions I made for myself:**
1. **Task 2's refusal evidence was committed on its own** (`fa5225b9`) before task 3 began, as the
   prompt's commit-per-task rule asks. The instruction lists one evidence commit for task 3; there
   are two, and neither touches `src`.
2. **Each technique went into the rival-margin lattice (`MarginAndRival`) as well as `DecodeAt`.**
   The rival lattice's own remarks say it scores every segment "exactly as DecodeAt scores it,
   between the same shortest and longest spans". Its margin is for the record only; nothing reads
   it back.
3. **Rounding at the edges.** A span in whole hops is set against the edge in dits, rounded as
   fldigi's inequalities fall:
   - (B): a mark is kept at `span >= unit / 2`, as `element_usec < threshold` is dropped;
   - (C): a dit is `span <= 2 x unit`, as `element_usec <= two_dots`;
   - (W): a gap under 2 dits is an element gap, 2 to 4 dits a character gap, and over 4 a word gap.

   Each has a 1e-9 guard against the unit's floating-point error.
4. **W's gap kinds are bounded by the dit, even with held gaps.** Where the sender's own gaps are
   held, W still bounds each gap kind by the dit edges, and keeps the held wanted lengths for the
   penalty only. So the three kinds cover every span without a hole. The element gap's own floor
   and the word gap's own ceiling stay as they were.
5. **Refused screens were timed and their texts saved before reverting** (136 s each), so that the
   refusal records show what each technique actually changed on the page.
6. **The fact's `refused-D` match was corrected at task 4,** after writing this report showed its
   031905 line was wrong. It had reported no event on `PREDICTED` because the scored stretch opens
   on `DICTED`. The fact was rerun: every other line of its printout is unchanged, and the ranking
   and the forms were not touched.
7. **The screens' floors were not run.** Every screen was refused on R78's rule 1 or on V-11 before
   the floors would have been reached. The instruction runs the floors only for a kept change.

## 2. What the owner should expect

The screen reads exactly as before: all three of fldigi's edges were tried and taken back out, so
every recording shows the same letters it did yesterday. The one that came closest was fldigi's
word-space rule, the one that ends a letter after two dits of silence and starts a new word after
four. With it in, the synthetic CQ at 18 WPM and 5 dB read `DE N0NNALL` where Hamlet prints junk,
and the QSO on 004234 read `THANK YOU` instead of `HANTT TK■ET`. Across the real recordings it
printed two fewer wrong letters and two fewer wrong spaces, and on the synthetic set nine fewer
wrong letters. What stood in its way were three recordings it made worse:
- `VERSIONS` on the 032113 bulletin became `■E■IONS`;
- the `2 TT` on 031838 lost its space;
- `KA2GJV` and `ROBIN` on 004550 were split as `KA2 G JV` and `RO B IN`.

The rules say one worse recording refuses the change. What will look wrong but is not: 459's
`TheSpeedFollowsTheSendersMarkPairsTests` still fails when run alone. It is on neither carry-forward
line, and it was left red on purpose.

## 3. What you should see

**The screen table.** Each technique screened in the working tree, against task 0's figures. The
four real (inferred key) and four synthetic (exact key) numbers are MET-CER-SURE,
MET-INVENTED, coverage and MET-WBE.

| technique | real before | real with it | synthetic before | synthetic with it | adjudicated | V-11 worse | decode, ours over 690 s | verdict |
|---|---|---|---|---|---|---|---|---|
| C, two-dot class edge, `cw.cxx:846-855` | 33/436, 33, 403, 37 (29 ins, 8 del) | 32/438, 32, 406, **38 (29 ins, 9 del)** | 14/173, 14, 159, 44 (13, 31) | 14/173, 14, 159, 44 (13, 31) | 13 of 13 | 1: 031838 (MET-WBE, deleted 1 to 2) | 51.22 s (entry 51.65) | **refused**, MET-WBE real the wrong way, and V-11 |
| B, half-dit spike rule, `cw.cxx:515, 818-822` | as above | **36/437, 36, 401, 39 (30, 9)** | as above | 12/172, 12, 160, **46 (15, 31)** | 13 of 13 | 3: 031905 (sure wrong 2 to 4, coverage 30 to 28, MET-WBE 1 to 3), 032050 (sure added 1 to 2), cq-18wpm-5db-char5 (added 4 to 5, inserted 6 to 8) | 52.03 s | **refused**, every real metric the wrong way, and V-11 |
| W, word-space edge, `cw.cxx:883-914` | as above | 31/436, 31, 405, 35 (27, 8) | as above | 5/170, 5, 165, 43 (12, 31) | 13 of 13 | 3: 032113 (coverage, sure right 21 to 19), 031838 (MET-WBE, deleted 1 to 2), 004550 (MET-WBE, inserted 1 to 2) | 49.50 s | **refused**, on V-11 alone |

The port was byte-identical at every screen, and `BothDecodersAreScoredAlikeTests` was 5 of 5 at
each. On V-11, W also made 004234 (sure wrong 2 to 0), 17:37 (inserted 6 to 3) and
cq-18wpm-5db-char5 (wrong 7 to 1, added 4 to 1) better.

**1. Task 1: ours' edges beside fldigi's, in dits, and the ranking.**

| decision | where in ours | ours | fldigi |
|---|---|---|---|
| a mark is formed | `CwProbabilisticDecoder.cs` 894-1085 (per-hop key-down and key-up log-likelihoods), 1257-1304 `DecodeAt` | a key-down segment the segmental Viterbi chooses; nothing is thresholded | 610-656, the detector against its squelch |
| a spike is discarded | 542 and 1265 (and 1509): no key-down segment under `max(1, (int)(0.45 x want))` hops; 1609-1626 `Judged` and 1644 `IsStrayElement` drop characters, never marks | **0.45**, truncated to a 5 ms hop | **0.5** (515, 818-822) |
| dit or dah | 526-533, 1291-1293: the length penalty `ln(span/want)/0.35`, squared; spans 0.45 to 2.2 for a dit, 1.35 to 6.6 for a dah | by likelihood, crossing at **1.73** | **2** (846-855) |
| element, character or word gap | 530-532 (1, 3, 7 units), or the held gaps (`CwProbabilisticStream.cs` 497-504); then the relabel, 713-734, which only takes spaces out | by likelihood, **1.73** and **4.58** (or the held gaps' geometric means), then the relabel | **2** and **4** (883-914) |
| the dit in force | `CwProbabilisticStream.cs` 442-449 (the measured speed, else the grid), 516-531 (the marks' overrule) | the unit of the read's own speed, 1200 / WPM ms | the tracked `two_dots` (502-515) |

The ranking. Fixable events sit in wrong or added letters, or at wrong boundaries; at-risk events
sit in right ones. Letters outside any scored stretch, and dim letters and placeholders, count as
neither.

| rank | technique | events | fixable | at risk | neither | net |
|---|---|---|---|---|---|---|
| 1 | C | 20 | 5 | 1 | 14 | +4 |
| 2 | B | 110 | 6 | 23 | 81 | -17 |
| 3 | W | 246 | 17 | 98 | 131 | -81 |

- **Also listed for B, not counted:** 48 marks of half a dit or more that ours takes as key-up.
  (B)'s form does not restore them.
- **None had zero fixable events,** so all three were screened.
- **The three reads that refused (D):**
  - 031948 `110,`: one W event, an unscored boundary, neither.
  - 004507 `EACH`: two W events, both at a right boundary (at risk).
  - 031905 `PREDICTED`: one B event, the `I` at 5.375 s, 0.44 dits (at risk).
  - B did break 031905, but on `10.7` (`10■T7`), where task 1 had flagged the `.`'s 0.44-dit dit
    at risk. The fact's first printout, committed at task 1, reported 0 events on `PREDICTED`: its
    word match missed a stretch that opens mid-word (`DICTED`). That was corrected and rerun at
    task 4. Nothing else in the printout changed.
- **The forms, fixed before any screen,** are the `form |` lines of the printout. The dit in each is
  the unit of the speed the lattice is run at.

**2. Every recording a kept change moved: none, since nothing was kept.** For the record, these are
the lines that refused W and C. Text is the scored stretch where there is one.

```
032113 (inferred)  key        ACKET, AND INTERNET VERSIONS
                   ours       A KET■ A N O INTERNET ■ERSIONS
                   with W     A KET■ A N O INTERNET ■E■IONS
                   with C     I KET■ A N D INTERNET VERSIONS
                   port       T TNDINE OJ
031838 (inferred)  key        2, 2, AND 2 WITH A MEAN OF 2.9. PRE
                   ours       2TT 2, AND ■ WIAHA MEAN OF 2 TT
                   with W     2T 2, AND ■ WIAHA MEUN OF 2TT
                   with C     2TT 2, AND ■ WIAHA MEAN OF 2TT
                   port       2, 2, AN■TWV2F 2P
004550 (inferred)  key        DE KA2GJV
                   ours       ... A3HB DE KA2 GJV HW ROBIN?
                   with W     ... A3HB DE KA2 G JV HW RO B IN ?
                   port       D■2 GJV
004234 (inferred)  key        OT COM <BT> THANK YOU
                   ours       C O M <BT> T HANTT TK■ET
                   with W     C O M <BT> T HANK YOU
                   port       T T AN■U O
```

Every line each technique moved is in the texts diff inside its `.run-unit/unit462-<C|B|W>-refused.txt`.

**3. The kept change's test** was not written, because no change was kept.

**4. Every commit, with the five at its exit.**

| commit | task | build | engine line | app line | named | captures | adjudicated |
|---|---|---|---|---|---|---|---|
| 258c95e6 | 0 | 0 errors | 178 of 178 | 278 of 278 | 13 of 13 | 51 of 51 | 13 of 13 |
| d616e680 | 1 | 0 errors | not run at the commit | not run at the commit | not run at the commit | not run at the commit | not run at the commit |
| fa5225b9 | 2 | 0 errors | not run at the commit | not run at the commit | not run at the commit | not run at the commit | not run at the commit |
| 7fc1b56a | 3 | 0 errors | 178 of 178 | 278 of 278 | 13 of 13 | 51 of 51 | 13 of 13 |
| task 4 (this) | 4 | 0 errors | 178 of 178 (7fc1b56a's run) | 278 of 278 (7fc1b56a's run) | 13 of 13 (7fc1b56a's run) | 51 of 51 (7fc1b56a's run) | 13 of 13 (7fc1b56a's run) |

- **Where the exit round ran.** On 7fc1b56a's tree, which is why that row carries the figures.
- **d616e680 and fa5225b9.** The exit round was not run at either. `git diff d616e680 7fc1b56a --
  src tests` prints nothing, so both carry the same code the exit round passed. d616e680 differs
  from 258c95e6 only by the new fact, which is on neither line.
- **The task 4 commit.** It changes only the fact's `refused-D` word match, which is on neither
  line, and was built at 0 errors (`t4fact`).
- **The builds.** Each "0 errors" is the build on that tree: `t1b`, `revert-C`, `exit` and
  `t4fact`.
- **The dispatcher loop.** There was none at entry or exit.

**The exit round against entry.**
- **Both sets' four metrics:** unchanged, and no row moved. Real (inferred): MET-CER-SURE 33 of 436,
  MET-INVENTED 33 / 473, coverage 403 / 473, MET-WBE 37 (29 ins, 8 del) / 113. Synthetic (exact):
  14 of 173, 14 / 252, 159 / 252, 44 (13, 31) / 84.
- **The tests:** `BothDecodersAreScoredAlikeTests` 5 of 5; the port 8 of 8;
  `TheSpeedFollowsTheSendersMarkPairsTests` 1 of 1 red, 28 of 31 sure wrong or added (not required
  green).
- **Decode time:** ours 51.88 s.
- **The diffs, all empty:** `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`;
  `git diff 7e209cb4` over the eleven transmit files, all present; `git diff 6de324b1 -- src`.
- **The texts:** the port's byte-identical to task 0's save (74 lines), ours identical (126 lines).
- **`git status`** shows `.run-unit/fldigi/` still untracked.
- **Printout:** `.run-unit/unit462-tx-exit.txt`.

**No visible change in the application.** This unit only measured and refused.

## 4. What's blocking us

Most-blocking first.

1. **W is held out by V-11 as DECIDED (4) reads it, and whether R78 means that is the owner's.**
   - *What happened.* W moved all eight totals the right way and held the 13 adjudicated readings.
     V-11 as this unit applied it refused W: one recording worse on any one metric refuses the
     change.
   - *Why the reading matters.* R78 itself says the capture floors "stay as V-11's overfitting
     guard", and that no change may redden an earlier capture to green a newer one. Read that
     narrowly, the floors are the guard, not a per-recording metric test. W's floors were not run,
     because it was refused before them.
   - *Proposed ruling:* keep DECIDED (4) as written. Per-recording V-11 on the four metrics is how
     459 was judged, and moving the bar after W's numbers were seen is exactly what V-14 and
     DECIDED (3) forbid.
   - *What the next unit may still do.* It may run W's saved patch unchanged against the three
     floor tests, as evidence for the owner, not as a keep.
   - *Rejected:* keeping W here on a narrower reading of V-11, which would decide the rule by the
     change it lets through.
   - *Needs the owner* only if R78's guard is meant to be the floors alone.
2. **The trace's net count misjudged W.** C ranked first at +4, and W last at -81, yet W was the
   one that moved all eight totals right. The trace's at-risk flags were borne out where it named
   them: B broke 031905's flagged `.`.
   - *Why.* The trace counts first-order events: a letter's own marks and gaps set against an
     edge. A changed edge re-chooses every span on the lattice, so what it fixes elsewhere is not
     counted.
   - *Proposed ruling for the next 9.4 author:* rank candidates by a screen, not by a count of
     edge events. Every screen here took about 10 minutes.
   - *Rejected:* re-ranking here after the numbers were seen.
3. **The refusals were narrow.** W was refused on three recordings, each by one or two characters
   or boundaries (`VERSIONS`, `2 TT`, `KA2GJV`/`ROBIN`). C was refused on one boundary on 031838,
   and 031838's boundary refused W as well. Logged, not chased: no technique was narrowed (V-14,
   DECIDED (3)).
4. **R85 readings, one line each.**
   - The dit in force is the unit of the speed the read was decoded at, 1200 / WPM, whether that
     speed was measured, won on the grid or taken from the marks.
   - A spike is dropped at the lattice's shortest key-down span. fldigi's reset of the character in
     progress was not taken.
   - "Ends a character" inside a lattice means the path's gap segment is the letter-gap or word-gap
     kind. The relabel, which only takes spaces out, was left as it is.
5. **The two `PHASE_STATUS.md` copies differ beyond the fields a unit sets** (section 1). Logged, not
   repaired.
