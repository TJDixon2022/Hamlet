READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0 to 2 done; 3 partial on 3.6;
   4 not started; 5 the owner's; 6 partial on 6.5; 7 partial on 7.1, 7.2, 7.4, 7.6, 7.8.
B. Step 7, criterion 7.4: whether the speed rule was kept, 3.2's four tests as numbers,
   and the opening - stream 30 to 46.2 s, 003901 and 003919 cold - before and after as text.
   This report answers none of B: this session stopped at task 0 and changed nothing.
C. The rest, weighed against A and B. Section 4 raises 1 item: two sessions were running
   unit 435 in the same tree at once. It does not stand in the way of 7.4 itself, but it does
   stand in the way of this session doing the work.

UNIT:       435 - stopped at task 0 of 3, tasks 0 to 3 left to the session already running them - 2026-09-25 08:18
PHASE GOAL: The CW decoder reads a real CQ call as the text that was sent, measured by edit distance against keys, and at the end Tim at the radio agrees.
UNIT GOAL:  When the speed estimate briefly halves as a session opens, the decoder keeps the sender's real speed, so the opening stops reading as E and T.
ADVANCED:   no - this session stopped before task 1 because an earlier session was already running the same unit in the same working tree, and it changed nothing in src or the record
NUMBER:     unchanged - stream 30 to 46.2 s still 22 named UIEH EE E E T I NIEEE E E ET N ■IK; keyed still 165 over 565; opening reads at a grid speed still 15 of 23; nothing was built
DRIFT:      carried as the work instruction gives it, 0; this session makes no claim on it

## 1. What Claude did

**Stopped at task 0 of 3.** No task finished, nothing was committed and nothing was pushed.
Hamlet confirmed at `C:\Source\HamLet` on `main` at `d548a565`, machine QUIVERFULL. This session
holds `SESSION.lock` (PID 43768, started 08:11:55).

**Why it stopped.** Another Claude Code session is live in this working tree and is running this
same unit 435. It is the session the launcher redirected at 08:11 (`RUN_LEDGER.md`, redirect row
at 08:11). It was never shut down. `ps` showed its shell chain (PIDs 356, 358, 362, 368 and 369)
running `sh .run-unit/unit435-cf.sh app cf-app-entry "TASK 0 of 3" ...`, started at 08:17:01.
That was right after its engine carry-forward line finished at 08:16:59. My solution build at
08:17 failed because that session's `testhost (34168)` held the App.Tests binaries. Two sessions
building, editing `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `Directory.Build.props`, and
committing to `main` at the same time would corrupt each other's commits. Continuing would have
put two copies of every record edit on the branch. I also did not stop the other session: I
did not start it, and killing another agent's process is not this unit's call. So I stopped.

**What I did before I found it, and then undid:**
- **Section 0 gate.** `SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` exist, and
  `CoreHMI.sln` and `MURC.sln` do not.
- **Task 0 records.** I wrote the `## UNIT 435 - STEP 7` entry in `PHASE_OUTCOME.md`, set
  `CURRENT_STEP: 7`, and bumped `Directory.Build.props` from 1.13.121 to 1.13.122 with a comment.
  All three are backed out. `PHASE_OUTCOME.md` and `Directory.Build.props` match HEAD again.
  `PHASE_STATUS.md` is back as the launcher left it, with `CURRENT_STEP: 3`. The edits are kept
  as `.run-unit/unit435-second-session-record-draft.patch`.
- **Task 1 trace code.** I added two facts, `WhatTheSpeedRuleWouldHaveUsed` and
  `WhereTheSpeedRuleWouldDiffer`, plus a replay of section 6's rule (`UnderTheRule`) to
  `WhatTheOpeningHeardTests.cs`. They were never run. The file is back to HEAD, and the draft is
  kept as `.run-unit/unit435-second-session-trace-draft.patch`. The draft may help the other
  session. It carries the estimator's last taken speed for 3.0 s of audio. On each read it uses
  the carried speed when the estimate is unready, outside 8 to 40 WPM, or departs from the carried
  speed by more than a factor of 1.5. It takes a departure seen on two consecutive reads. With
  nothing carried, it falls back to the grid.
- `PROJECT_STATUS.md` was written by `tools/status.sh` and says why this session stopped.

**Entry measurements I can report. None of them were run by this session:** the other session's
engine carry-forward line, `.run-unit/unit435-cf-engine-entry.txt`, finished 178 of 178, RC 0,
384 s, against `src` unchanged from `d548a565`. Its app line was still running when I stopped.

**Section 5, checked against the tree. No mismatch, and nothing repaired:**
- `CwProbabilisticStream.cs` 428 to 435 is as stated. `CwUnitEstimator.Measure` is called on the
  window, and its speed is taken when `IsReady` and inside `SlowestWpm` to `FastestWpm`, else
  null. Null becomes the grid search at `CwProbabilisticDecoder.cs` 756 and 757
  (`from = atWordsPerMinute ?? SlowestWpm`, `to = atWordsPerMinute ?? FastestWpm`). The loop is
  759 to 771.
- **No speed is carried from one read to the next.** `_heldGaps`, line 121, carries the gap
  lengths in milliseconds once structure is held, but not a speed. `Last.WordsPerMinute` keeps
  the previous read's speed, and nothing feeds it back.
- **Read cadence.** `ReadEverySeconds` is 0.5 and `HopMilliseconds` is 5, so reads are 100 hops
  apart. 3.0 s is six reads, and two consecutive reads are 0.5 s apart.
- `FastestWpm` is still 40 at HEAD (`CwProbabilisticDecoder.cs` 480), and `SlowestWpm` is 8.
- `WhatTheOpeningHeardTests` holds `WhatTheDecoderHeldAtEachCharacter`, with the stream spliced
  from `Session` by `Splice`.
- **One mismatch outside section 5.** HEAD's `Directory.Build.props` already reads 1.13.121.
  The seed commit `d548a565` bumped it from 1.13.120, so the other session's task 0 had already
  bumped it. A further bump to 1.13.122 is that session's decision.

**Tasks not done:** 0, 1, 2 and 3, all of them. None is a drop candidate. They were left
because the unit already has a session running it in this tree, and a second one working the
same files could only damage it.

## 2. What the owner should expect

Nothing has changed. The first minute on a new frequency still comes apart into E and T, as it
did at unit 433's exit. This session cannot tell you whether the speed or the pitch is now the
reason, because it measured nothing. Another session is still running unit 435 in the same tree
and may finish it. Its own `output.md` will overwrite this one and carry the answer. If this
file is still here with this `UNIT:` line after that session has ended, that session did not
finish either.

## 3. What you should see

No visible change. This session changed no source, no test and no record, so the application
reads the opening exactly as before. The opening's text is unchanged: stream 30 to 46.2 s
`UIEH EE E E T I NIEEE E E ET N ■IK`, 22 named; `003919` cold `EITEETNXNIK EANQNID EANQNIK`, 25
named. Both figures are carried from P48, not re-measured here. No per-read speed column was
produced.

## 4. What's blocking us

**Two sessions launched on one unit in one tree.** Ruling asked of the owner: when the launcher
redirects a unit, it has to end the session it redirected before it launches the next one. It
can check `ps` or the `.run-unit` pid files for a live `unit<n>-*.sh` chain before it takes
`SESSION.lock`. Reasoning: this time the redirected session kept running unit 435's entry round
after the lock went to a new session at 08:11:55. The two collided on the App.Tests binaries
within six minutes, and they would have collided on every record file and commit after that.
Rejected: this session carrying on beside the other one, which doubles every record edit and
commit on `main`, and this session killing the other, which destroys work already paid for and
is not a session's call. Not blocking the phase: the session already running can finish unit
435 alone.
