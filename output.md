READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 0 of 5, 8 0 of 6, 9 3 of 8.
B. Step 2, criterion 2.5: named floors 17:37 46 to 38, 032113 45 to 43,
   032129 64 to 42 - 17:37 on 9a329cdb, 032113 and 032129 on the condition
   test (R78, 448 DECIDED (6)); commits from task 1 on green on all five:
   3 of 3; 2.5 ticked.
   HM-REQ-010: task 2 found no mechanism; task 3 not run; real MET-CER-SURE
   33 to 33 of 436 sure, MET-INVENTED 33 to 33, coverage 403 to 403,
   MET-WBE 37 to 37; synthetic the same at 14 of 173, 14, 159 and 44;
   adjudicated 13 of 13; V-11 0 of 23 worse (no src line changed);
   2.4's count 1 of 3; 9.4 open.
C. The findings weighed against A and B: section 4 raises 6 items. None is
   in the way of 2.5, which is ticked. Item 3 bears on 2.4: its count is
   now 1 of 3. Item 5 bears on HM-REQ-010. It is a finding, not a blocker:
   the moves 459's pair speed got wrong rest on marks of five units or more
   that our trigger makes by joining elements, and no pair rule
   distinguishes those. None of the 6 asks the owner anything about keying,
   transmit, or what the product tells the operator.

UNIT:       460 - complete at task 4 of 4, none dropped (task 3 not run: task 2 registered no rule) - 2026-09-26 20:36
PHASE GOAL: Hamlet's CW decoder meets every requirement in CW_REQUIREMENTS.md, each proven by a test that names it, with fldigi's receiver run beside ours as a second decoder under section M.
UNIT GOAL:  Settle the three red named floors on the owner's answer in 9a329cdb and the per-condition test, so every step-2 commit from task 1 on exits with the floors and both carry-forward lines green; then trace where 459's pair speed helped and broke, and judge a second form only if a mechanism separates them.
ADVANCED:   yes - PHASE_PLAN.md 2.5 ticked in both copies: the three rows are re-banked on the recorded rule, and every commit from task 1 on (8d607c4f, dddf3b21 and the exit commit) exits green on the build, both carry-forward lines and the three floor tests
NUMBER:     named floors 10 of 13 to 13 of 13; commits green from task 1 3 of 3; HM-REQ-010 real MET-CER-SURE 33/436 to 33/436 (inferred), synthetic 14/173 to 14/173 (exact); coverage real 403 to 403; MET-INVENTED real 33 to 33
DRIFT:      step 2 0 (was 0); step 3 0; step 4 1; step 5 1; step 6 0; step 9 1

## 1. What Claude did

**Complete at task 4 of 4, with none dropped.** Task 3 was conditional on task 2 registering a rule.
Task 2 registered none, so task 3 was not run. That is the instruction's own branch, not a sizing
decision.
- **Where.** Development machine, Claude Code, `PROJECT: Hamlet` claimed and confirmed by the gate:
  the four files present, no `CoreHMI.sln` or `MURC.sln`, root `C:\Source\HamLet`.
- **Branch.** `main`, pushed after every commit.
- **Lock.** `SESSION.lock` was already present (PID 52096, 19:09:22). It is the launcher's and was
  neither taken nor released.

**Commits:** `21c67798` (task 0), `8d607c4f` (task 1), `dddf3b21` (task 2), and the exit commit that
carries this report (task 4).

**Where the tree and the instruction disagree** (the tree is the fact):
- **`PARKED.md`'s header** says "nothing reads this file back as a decision", and this unit applied its
  `RESOLVED:` line on 17:37 as the owner's answer. Reported, header not edited (section 4, item 1).
- **442's re-banking commit** is `3cf9f9ea` (unit 442 task 1). `0439a8e7` is 441's closing, the decoder
  442 measured on; 442's task 0 changed no `src`.
- **The owner's answer** says "the condition's MET-WBE fell 58 to 52". Those are the real set's total
  MET-WBE, at G1 (`unit441-metrics-g1.txt`) and at 442's entry. The condition's own figure over the same
  two printouts went 51 to 45. The direction is the same (section 4, item 2).
- **`.run-unit/watched.rc`** is not in the tree.
- **`.run-unit/watched.cpu`** was committed as the runner left it at task 0. The runner has rewritten it
  since, and the exit commit carries it as it stands.

**Task 0.**
- **The record.** The 460 block went into both copies of `PHASE_OUTCOME.md`. `PHASE_STATUS.md` names
  460 with `CURRENT_STEP: 2` in both copies. Version 1.13.146 to 1.13.147. The runner's writes were
  committed as they were, with `.run-unit/fldigi/` not staged.
- **The entry round**, every figure as 459 left it:
  - build 0 errors; engine line 178 of 178;
  - app line 275 of 278, all three losses "You've caused dispatcher loop"
    (`ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed` and two
    `TheStopIsAlwaysOnScreenTests` cases), and the one rerun under DECIDED (7) 278 of 278;
  - captures 51 of 51; adjudicated 13 of 13;
  - named 10 of 13, the three red as recorded (17:37 38 of 46, 032113 43 of 45, 032129 42 of 64);
  - the four metrics as in section 3.
- **The texts.** Our 126 text lines were saved to `.run-unit/unit460-text-before.txt`. They are
  byte-identical to 459's exit. The class is in the text: plain is sure, `[x]` is not sure, `■` is a
  placeholder.

**Task 1, the floors** (`.run-unit/unit460-floors.txt`).
- **Where the figures come from.** Every one is read from a committed printout, so nothing was
  re-measured. 17:37 was last banked at 46 by unit 421 (`df26a5d7`). No Cw source changed between 421's
  exit and 441's entry, so `unit441-metrics-r82.txt` is that decoder measured under today's scoring.
  032113 and 032129 were last banked by 442 in `3cf9f9ea`, with `unit442-metrics-entry.txt` as their
  printout.
- **When they fell.** 17:37 fell 46 to 38 at `42d5dbb9` (441, G1 kept). 032113 and 032129 fell at
  `b4884813` (447, CwToneTracker).
- **The condition, all three.** Real HF, sender not stated in `CW_SPEC.md`, inferred keys. It is better
  at HEAD than at either banking on all four metrics.
- **The recordings' own four.** Only 17:37's word boundaries are worse, 5 to 7 wrong over 6 words, and
  that is what the owner's answer covers.
- **The re-banking.** 17:37 was re-banked at 38 on `9a329cdb`, and HEAD reads 38, so there is no
  mismatch. 032113 was re-banked at 43 and 032129 at 42 on the condition test.
- **The rows.** Each carries 442's comment form, and 17:37's `NOT RE-BANKED` comment now says what
  happened and when. Nothing else in the test changed.
- **The five at the commit:** build 0 errors; named 13 of 13; captures 51 of 51; adjudicated 13 of 13;
  engine line 178 of 178; app line 276 of 278. Both losses were the dispatcher loop
  (`ThePsk31ConversationCardTests`, two cases), and the one rerun went 278 of 278.

**Task 2, the trace** (`.run-unit/unit460-pair-trace.txt`).
- **The printer.** `WhereThePairSpeedMovedTheUnitTests` asserts only that it printed all seven
  recordings. It drives our decoder hop by hop and keeps every read of the stream: 399 reads over the
  seven named recordings. On each it applies `6a0b65a1`'s `PairUnit`, copied line for line, keeping
  every pair and spike. It aligns every settled letter to its key.
- **Two runs, joined.** It ran at HEAD, then with `6a0b65a1`'s `src` diff applied to the working tree
  only. After that, `git checkout -- src` restored the tree, and `git diff -- src` printed nothing. The
  two printouts were joined read by read.
- **Sameness 399 of 399.** With the diff in, the stream's speed on every read is what the copied rule
  predicts.
- **The key's unit.** It is the median, over the letters settled sure and right inside the window, of
  each letter's span per unit of its pattern. Inferred keys.
- **The moved reads.** 81 reads moved: 35 toward the key's unit, 16 away, and 30 with no sure-right
  letter in the window.
- **The texts with the diff.** They reproduce 459's list. It helped `AA4MP`, `30 [2] 2` and
  `AND INTERNET VERSIONS`, and broke `11■`, `I ACH`, `PLDICTED` and `PACKE   [H] E SEEIE`.
- **fldigi's lines**, `cw.cxx` at `61b97f41`:
  - **seed:** 272-273, 317-324, 481-487;
  - **average:** `filters.cxx:273-291` with `TRACKING_FILTER_SIZE` 16 (`cw.h:70`);
  - **spikes:** 515, 818-822;
  - **pair acceptance:** 524-531, 832-844;
  - **classing:** 846-855.
- **The verdict: no mechanism of the pair rule separates the helped windows from the broken ones.**
  - Every pair in the broken windows passes fldigi's own test.
  - `6a0b65a1`'s one departure from fldigi is that each window's first pair fills the 16-slot average
    where fldigi's seed does. That fill holds 9 to 14 slots in 031948 and 031905. It holds 7 to 13 in
    helped reads of 031838 and 032113 too, and 0 or 1 where 004507's E and 032050's reads 49 and 52
    broke.
  - The broken moves rest on marks of 300 to 470 ms, five to seven and a half units at the key's
    61 ms, which our trigger makes by joining elements. That is upstream of the rule, and a limit on it
    would be a constant picked from these seven recordings.
- **So:** no second form is registered, task 3 is not run, and **this unit is step 2's first of three
  with no kept change (2.4's count 0 to 1).**
- **The five at the commit:** build 0 errors; named 13 of 13; captures 51 of 51; adjudicated 13 of 13;
  engine line 178 of 178; app line 278 of 278 with no dispatcher loss.

**Task 4, the exit round.** Every figure beside its entry figure is in section 3.
- **2.5 is ticked in both copies of `PHASE_PLAN.md`.** The commit table shows every commit from task 1
  on green on all five, no named row is left red, and the exit round agrees.
- **No other floor line is ticked.** The floors no longer hold any of 3.6, 4.7, 5.6, 6.6, 7.5, 8.6 or
  9.8, because the three floor tests are green. Each still wants its own step's reading at that step's
  exit, and 3.6 is worded, like 2.5, over every commit of its step.

**Decisions the session made for itself**, author's and overrulable:
- **(a) The key's speed.** "Toward the key's speed" is read through the key's own unit: the median span
  per unit of the letters the key confirms were read sure and right inside the window. No speed is
  recorded for a real recording, and a letter read element for element carries the sender's timing
  whatever speed the path had. That is R85's reading, from `CW_SPEC.md` 11's alignment and our
  decoder's own spans.
- **(b) The helped and broken reads.** They are joined with the letters that settled at the same read
  in each run.
- **(c) Task 3 not run.** A mechanism that amplifies two of the four broken stretches, and just as much
  a helped one, was judged not to separate helped from broken. The instruction's "if no difference
  separates them" branch was taken.
- **(d) What was not staged.** The task 2 printer is committed under `tests` and passes, and it is on
  no carry-forward line. The ignored `.tmp` join files were not forced into the commit.

## 2. What the owner should expect

Nothing under `src` changed over the unit (`git diff 1f1c915d -- src` prints nothing), so the screen
reads exactly as before, letter for letter. All 126 recording texts are byte-identical to task 0's save.
The pair speed was only ever in the working tree, and only for the trace. What changed is the test
suite's view of the decoder: the three named floors that every step's exit had waited on since unit 441
are green, at the counts the decoder reads today. So a future change that drops 17:37 below 38, 032113
below 43, or 032129 below 42 characters now fails loudly. What will look wrong but is not:
`TheSpeedFollowsTheSendersMarkPairsTests` is still red, at 28 of 31 sure characters wrong, as unit 459
left it on purpose. It is on neither carry-forward line.

## 3. What you should see

**The three floors: all three re-banked, none left red.** Keys inferred. The texts are the scored
regions the floor test prints. The metrics are sure wrong or added over sure emitted, sure wrong plus
sure added over sent, sure and right over sent, and boundaries wrong over words.

**17:37 (`unadjudicated/cw-2026-09-23-173723`): 46 to 38, on `9a329cdb`.**

| | last banking (46, `df26a5d7`) | HEAD (38) |
|---|---|---|
| key | `CQ CQ CQ DE WB6RED WB6RED` | same |
| text | `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I` | `CQ CQ CQ DEWB6 RE D W B 7E E I` |
| condition MET-CER-SURE | 66 of 364 | 33 of 374 |
| condition MET-INVENTED | 66 over 410 | 33 over 410 |
| condition coverage | 298 over 410 | 341 over 410 |
| condition MET-WBE | 50 over 97 | 32 over 97 |
| own MET-CER-SURE | 14 of 28 | 3 of 20 |
| own MET-INVENTED | 14 over 20 | 3 over 20 |
| own coverage | 14 over 20 | 17 over 20 |
| own MET-WBE | 5 over 6 | **7 over 6 - worse, the owner's answered case** |

**032113 (`unadjudicated/cw-2026-08-22-032113`): 45 to 43, on the condition test.**

| | last banking (45, `3cf9f9ea`) | HEAD (43) |
|---|---|---|
| key | `ACKET, AND INTERNET VERSIONS` | same |
| text | `A KET■ A N O INT ERNE T ■ E RSIONS` | `A KET■ A N O INTERNET ■ERSIONS` |
| condition MET-CER-SURE | 47 of 359 | 33 of 374 |
| condition MET-INVENTED | 47 over 410 | 33 over 410 |
| condition coverage | 312 over 410 | 341 over 410 |
| condition MET-WBE | 45 over 97 | 32 over 97 |
| own MET-CER-SURE | 1 of 22 | 1 of 22 |
| own MET-INVENTED | 1 over 25 | 1 over 25 |
| own coverage | 21 over 25 | 21 over 25 |
| own MET-WBE | 7 over 4 | 3 over 4 |

**032129 (`unadjudicated/cw-2026-08-22-032129`): 64 to 42, on the condition test.** The condition's
four are the same as 032113's. The count fell because the E and I litter went, while the rest of the
bulletin's name is now read.

| | last banking (64, `3cf9f9ea`) | HEAD (42) |
|---|---|---|
| key | `2026 PROPAGATION FORECAST BULLETIN ARLP034` | same |
| text | `TJ26 PGOPAGATION E EE EIIEE I E EE ` | `TJ26 PGOPAGATION FORECAST BUAELETIN ARLP034` |
| own MET-CER-SURE | 12 of 27 | 5 of 39 |
| own MET-INVENTED | 12 over 38 | 5 over 38 |
| own coverage | 15 over 38 | 34 over 38 |
| own MET-WBE | 5 over 5 | 0 over 5 |

**1. Every commit of the unit, with the five at its exit.**

| commit | task | build | engine line | app line | named | captures | adjudicated |
|---|---|---|---|---|---|---|---|
| `21c67798` | 0 | 0 errors | 178/178 | 275/278, dispatcher loop x3; rerun 278/278 | 10/13, red as recorded | 51/51 | 13/13 |
| `8d607c4f` | 1 | 0 errors | 178/178 | 276/278, dispatcher loop x2; rerun 278/278 | 13/13 | 51/51 | 13/13 |
| `dddf3b21` | 2 | 0 errors | 178/178 | 278/278 | 13/13 | 51/51 | 13/13 |
| exit commit | 4 | 0 errors | 178/178 | 278/278 | 13/13 | 51/51 | 13/13 |

From task 1 on, 3 of 3 commits are green on all five. The exit commit adds only this report, the tick
and the exit printouts to the tree the exit round ran on.

**The exit round, beside entry.**

| | entry | exit |
|---|---|---|
| build | 0 errors | 0 errors |
| engine line | 178 of 178 | 178 of 178 |
| app line | 275, rerun 278 of 278 | 278 of 278 |
| named / captures / adjudicated | 10 / 51 / 13 | 13 / 51 / 13 |
| real, inferred: MET-CER-SURE | 33 of 436 | 33 of 436 |
| real: MET-INVENTED | 33 over 473 | 33 over 473 |
| real: coverage | 403 over 473 | 403 over 473 |
| real: MET-WBE | 37 (29 ins, 8 del) over 113 | 37 (29, 8) over 113 |
| synthetic, exact: the four | 14 of 173; 14 over 252; 159 over 252; 44 (13, 31) over 84 | the same |
| `TheSecondDecoderIsAFaithfulPortTests` | - | 8 of 8 |
| `TheSpeedFollowsTheSendersMarkPairsTests` | red, 28 of 31 | red, 28 of 31 at 37/34/32 WPM |

- **The per-recording and per-condition metric rows** did not move (`.run-unit/unit460-tx-exit.txt`).
- **`git diff 7e209cb4`** over the eleven transmit files prints nothing, and all eleven are present.
- **`git diff 1f1c915d -- src/Hamlet.RadioEngine/Cw/Second/`** prints nothing.
- **Our 126 texts** are identical to task 0's save.
- **`git status`** shows `.run-unit/fldigi/` still untracked.

**2. Task 2's windows, helped against broken. Rule registered: none.** The key's unit comes from
sure-right letters in the window, with inferred keys.

| recording | reads | unit before -> after | key's unit | pairs; first pair and slots it holds | letters |
|---|---|---|---|---|---|
| 003758 helped | 18-19 | 40.0 -> 22.0 WPM | 49.8 ms (24.1 WPM), toward | 21; 55/155 ms, 0 | E E T to `A 4` of AA4MP |
| 031838 helped | 11-15 | 22.8-28.2 -> 16.5-17.0 | none in window | 6-8; 65/220, 9-11 | T wrong gone, `2` right (not sure) |
| 032113 helped | 8-19 | 24-30 -> 16.1-19.5 | 55-60 ms, toward on 11 of 12 | 13-21; 30/85, 0-4 | O to `D`, placeholder to `V` |
| 032050 helped | 20-21, 44 | 30-34 -> 17.8-18.1 | 60.6-62.7 ms, toward | 21-22; 0 slots | A to `U`, placeholder to `C` |
| 031948 broke | 18-20 | 17.8-18.5 -> 8.2-8.6 | 61 ms, away | 5-6; 220/455, 11-12 | `,` of 110, to a placeholder |
| 031905 broke | 3-7 | 17.8-18.5 -> 12.7-14.1 | 57-61 ms, away | 3-8; 80/305, 9-14 | PREDICTED to PLDICTED |
| 004507 broke | 21-22 | 32-37 -> 20.0-20.4 | 51.8 ms, toward | 20-22; 65-70/170, 0 | E of EACH to I |
| 032050 broke | 49-56 | 21-37 -> 11.0-15.9 | 61-63 ms, away | 10-17; 0-7, with 150/320, 160/435, 180/445 | I right to E wrong; H stays wrong, not sure |

In the samples column of the trace, each pair's dit and dah are also given in samples at 48000 Hz.

**3. Task 3 did not run**, so there is no per-condition table, decode time, V-11 or moved stretch for a
second form.

## 4. What's blocking us

Nothing blocks the next unit.

1. **`PARKED.md`'s header says nothing reads it back as a decision; this unit read one of its
   `RESOLVED:` lines as the owner's answer.**
   - *Proposed ruling:* a `RESOLVED:` line committed by the owner outside any unit is read as the
     owner's answer to the question it quotes, and the header is amended to say so.
   - *Reasoning:* 443 DECIDED (3) and 448 DECIDED (6) rested on that question, and the instruction
     applied the answer. The header still says the opposite, and the next author will meet the
     contradiction.
   - *Rejected:* editing the header in this unit. The instruction forbids it, and it is the owner's
     file.
2. **The owner's 17:37 answer quotes "the condition's MET-WBE fell 58 to 52"; those are the real
   set's totals.**
   - *Proposed ruling:* the answer stands as written. The condition's own figure, 51 to 45 over the
     same printouts, moved the same way, and at HEAD it is 32.
   - *Reasoning:* the answer's premise holds on either reading, so nothing re-banked on it changes.
   - *Rejected:* holding 17:37 red on the wording.
3. **2.4's count is now 1 of 3.** This unit kept no `src` change. Logged, not a question.
4. **The floor lines of steps 3 to 9 are no longer held by the floors.**
   - *Proposed ruling:* each is ticked at its own step's next exit that reads the five green. 3.6 is
     judged, like 2.5, over every commit of step 3 from the unit that ticks it.
   - *Reasoning:* the plan words them per step.
   - *Rejected:* ticking them here. The instruction forbids it, and no step-3 to step-9 commit was
     made.
5. **The finding for HM-REQ-010's next author**, logged under R85, not asked. 459's pair speed breaks
   where our trigger joins elements into marks of five units or more: 031948, 031905 and 032050's
   later reads. fldigi's tracker takes the same pairs but gives each one a sixteenth of a vote,
   because its average is continuous and seeded, not because it refuses them. 004507's `EACH` broke
   at a speed near the sender's own (TX-FARNS), which is not a speed-tracking failure.
6. **The runner rewrote `.run-unit/watched.cpu` during the unit.** It was committed at task 0 as found,
   and the exit commit carries it as it stands. Logged.
