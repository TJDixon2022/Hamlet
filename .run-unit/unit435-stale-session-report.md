READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial on 3.6;
   4 not started; 5 the owner's; 6 partial on 6.5; 7 partial on 7.1, 7.2, 7.4, 7.6, 7.8.
B. Step 7, criterion 7.4: this report answers none of B. No speed rule was traced, built or judged.
C. Section 4 raises 1 item: this session was the one the launcher redirected at 08:11 and never
   shut down, and it ran unit 435's entry round alongside two later sessions in the same tree.
   It does not stand in the way of 7.4. It does stand in the way of trusting a run that overlaps it.

UNIT:       435 - stopped at task 0 of 3, tasks 1 to 3 not started - 2026-09-25 08:31
PHASE GOAL: The CW decoder reads a real CQ call as the text that was sent, measured by edit distance against keys, and at the end the owner agrees at the radio.
UNIT GOAL:  When the speed estimate halves for one read as a session opens, the decoder keeps the sender's speed, so the opening stops coming apart into E and T.
ADVANCED:   no - stopped at task 0 because the launcher had moved on to unit 436 and a later session holds SESSION.lock
NUMBER:     unchanged - stream 30 to 46.2 s 22 named UIEH EE E E T I NIEEE E E ET N ■IK; keyed 165 over 565; opening reads at a grid speed 15 of 23; nothing built
DRIFT:      0, carried as the instruction gave it; this session makes no claim on it

## 1. What Claude did

**Stopped at task 0 of 3. Nothing committed, nothing pushed.** Machine QUIVERFULL, project
Hamlet, confirmed by SHACK_FACTS.md and CwProbabilisticDecoder.cs present, CoreHMI.sln and MURC.sln
absent. Branch main, entry HEAD 8555034d. This session is PID 32764, started 08:07:39.

**Why it stopped.** At 08:30 the tree showed this session was stale. RUN_LEDGER.md records a
redirect at 08:11 and then a run from 08:11 to 08:19, marked complete. The output.md written at
08:18:57 is that run's (PID 43768). It stopped at task 0 because it found this session running,
and its report names this session's shell chain. The launcher then recorded 435 as not advanced
and wrote work instruction 436, *the estimator checks its dit against its dah* (WORK_INSTRUCTIONS.md,
08:23:22). SESSION.lock now names PID 48584, started 08:23:36. Carrying on would have put two
sessions building, committing and pushing in one tree.

**What it did before it saw that.** It ran the task 0 entry round, one type per invocation. Every
number below matches unit 433's exit apart from timing lines:

- Hamlet.sln builds with warnings as errors: RC 0, 18 s.
- Engine carry-forward: 178 of 178 in 384 s.
- App carry-forward: 277 of 278 in 172 s. The one failure was the known dispatcher-loop loss
  on Unit376TheTopBandTests. That type re-run alone passed 5 of 5.
- Captures: 51 of 51 in 124 s, with 032113 at 47 named, all 47 at or above the bar.
- Adjudicated: 13 of 13. Keyed floors (TheNumberCannotBeGamedTests): 13 of 13.
- TheBenchmarkIsKeyedTests: 1 of 1, 35 over 156 at the bench and 41 over 156 live.
- TheBaselineIsScoredTests: 2 of 2, 22 over 46 and 108 over 363 outside.
- WhatTheStrayLettersRestOnTests: 3 of 3 in 292 s. 165 edits over 565 on 23 keyed recordings;
  892 named, 353 right, 56 wrong, 17 added, 8 of them single-element.
- WhatTheOpeningHeardTests: 17 of 17 in 335 s. 003901 cold 9 named `EII E T NHHK`; 003919
  cold 25 named `EITEETNXNIK EANQNID EANQNIK`; the stream 30 to 46.2 s 22 named
  `UIEH EE E E T I NIEEE E E ET N ■IK`.

**Those numbers are not clean evidence, and here is why.** The opening type ran **17** tests.
Two of them, `WhatTheSpeedRuleWouldHaveUsed` and `WhereTheSpeedRuleWouldDiffer`, are not in the
source tree now. The compiled DLL (08:17:13) contains them, and the .cs file was rewritten at 08:18:18.
So the build at 08:17 compiled somebody else's in-flight edit to the test project. The engine DLL
was rebuilt at the same moment, so this session cannot say what source its numbers ran against.

Section 5 was checked against the tree at 08:10, before the overlap. It matches:
- CwProbabilisticStream.cs 431 to 435 takes the estimator's speed when it is ready and within
  SlowestWpm to FastestWpm, else null.
- Null becomes the grid search at CwProbabilisticDecoder.cs 756 to 757 (`atWordsPerMinute ??
  SlowestWpm` / `?? FastestWpm`); the loop runs to 771.
- **No speed is carried from one read to the next.** The only thing carried is `_heldGaps`, the
  sender's gap lengths in milliseconds, used once the structure has held for 12 reads. It is not
  a speed and is not handed to the speed argument.
- A read comes every 0.5 s of new audio (`ReadEverySeconds`). So 3.0 s is 6 reads, and two
  consecutive reads are 1.0 s.
- FastestWpm is 40 at CwProbabilisticDecoder.cs 480.

Files this session changed or wrote, all uncommitted:
- PHASE_STATUS.md: CURRENT_STEP 7 and WORK_INSTRUCTION 435. The launcher has since rewritten it.
- Directory.Build.props: 1.13.120 to 1.13.121. It still reads 1.13.121. This session cannot tell
  whether that is its own bump or the live session's, so it left it alone.
- PROJECT_STATUS.md, last at 08:29:55: it names 435 and this session's entry round, **which is
  wrong for the panel while 436 runs**. This session stopped writing to it so it would not overwrite
  the live session's status again.
- .run-unit/unit435-*.sh (copied from unit 433's) and .run-unit/unit435-*-entry.txt.
- This file. output.md is left as the 08:18 run wrote it, because unit 436 owns it now.

No decisions recorded under 12.1.

## 2. What the owner should expect

Nothing in src changed because of this session, and nothing reached the remote. The first minute
on a new frequency still comes apart into E and T exactly as before. The panel may show unit 435 in
EXECUTING until unit 436's session next writes its status, and that line is stale. Check
Directory.Build.props before unit 436 commits it: 1.13.121 may be this session's leftover bump.

## 3. What you should see

No visible change. Nothing was built, so the opening still reads `UIEH EE E E T I NIEEE E E ET N ■IK`
on the stream from 30 to 46.2 s.

## 4. What's blocking us

**1. A redirected session is not stopped before the next one starts.**
Ruling asked: when the launcher redirects, it ends the running session's process before it seeds
the next one, and a session checks SESSION.lock's PID against its own before each commit.
Reasoning: this session started at 08:07, was redirected at 08:11, and kept running until 08:31.
It overlapped two later sessions. Its build compiled one of their test edits, and its status writes
overwrote theirs. The 08:11 session's report records the same overlap from the other side.
Rejected: relying on each session to check the lock only at start. This one did not check it, and
the lock was replaced under it at 08:23 without its knowing.

**Asks still outstanding**

P44 and P46 are the owner's and are still in docs/phase-correctness/PARKED.md, as the instruction
carried them. None was this unit's to answer.
