```
A. PHASE GOAL - CW decodes again: the engine's CW decoder reads on the air as it did on
   2026-08-25, held there by three floor tests and, at the end, judged by Tim at the radio.
B. THIS STEP - step 3, the inherited reds are gone. Exit criteria 3.1 to 3.5 ticked; 3.6
   open: each of #6, #15, #24, #41, #42, #43, #44, #45 needs a verdict, green by a decoder
   repair with no floor lowered and the three floors green at that commit, or parked after
   three consecutive attacks with no movement. #24 and #41 went green at 7e65aac4.
C. THIS REPORT ADDS - Tim's 12:55 UTC capture decoded through the decoder at HEAD and at
   a902cdf8, the src state before 7e65aac4: the transcripts are byte-identical and every
   count matches, and none of the 37 capture cases moves between the two builds. Bears on
   B: on this evidence 7e65aac4, the repair behind #24 and #41, is not what changed the
   reading of this capture, so the remaining 3.6 work is not resting on a decoder that
   commit damaged. It does not advance A. The ruling on 7e65aac4 stays Tim's.
```

```
READ IN THIS ORDER.

A. The two transcripts, one above the other, with the table - section 3.
B. Whether any of the 37 capture cases moved between the two builds - section 3: none did.
C. The rest. Section 4 raises 4 items and none blocks a criterion.
```

```
UNIT:       403 - complete at task 4 of 4, none dropped - 2026-09-23 10:10
PHASE GOAL: get the CW decoder reading on the air again, as it did on 2026-08-25, held by three floor tests
UNIT GOAL:  read one recording, Tim's 12:55 UTC capture, through today's decoder and the one before 7e65aac4, and print both
ADVANCED:   no - a blocker-clear by design
NUMBER:     characters unsure on the capture: HEAD 45, a902cdf8 45
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 4, tasks 0 to 4, none dropped.** Machine QUIVERFULL, project
Hamlet, confirmed by the gate: SHACK_FACTS.md and CwProbabilisticDecoder.cs present, no
CoreHMI.sln or MURC.sln, root C:\Source\HamLet. Branch main, each task pushed, and every
push succeeded: a0077397, bab02a85, e69b49ca, c4db4090, then the task 4 commit.

**Task 0, the record.** Version 1.13.89 to 1.13.90. PHASE_STATUS.md read CURRENT_STEP 0
and 402. It now reads CURRENT_STEP 3 and 403 - the same recording, two builds.
PHASE_OUTCOME.md gains `## UNIT 403 - STEP 3` with the decision block. The capture wav
was already tracked, committed by Tim's seed at 7fc2ccfc, so there was nothing untracked
to commit. Entry round: captures 37 of 37 in 94 s, adjudicated 13 of 13 in 29 s, clean
synthetics 2 of 2 in 2 s, engine line 178 of 178 in 377 s, app line 278 of 278 in 183 s.

**Task 1, the capture at HEAD.** `TheCaptureOfTheTwentyThirdTests` sits beside the other
printers under tests\Hamlet.RadioEngine.Tests\Cw. It reads the wav through
`CwDecodeHarness` with a starting tone of 600 Hz, the same value the floor test uses. It
asserts only that samples were read and that something was emitted, and asserts no
text. Its output is in .run-unit\unit403-head.txt.

**Task 2, the capture at a902cdf8.** The worktree was C:/Source/HamLet-wt403. The printer
and the wav were copied in. The worktree does not have the wav because the file was
committed after a902cdf8.

**A decision made by the session: HEAD's CwDecodeHarness.cs was copied in beside the
printer even though the old one would have compiled.** At a902cdf8 the harness differs
from HEAD's by 11 lines. Using it would have measured the harness change as well as the
decoder change, and HM-DEC-091 asks that both sides use the same harness. Nothing under
src in the worktree was touched.

**A second decision: the a902cdf8 run was done twice.** The first worktree build finished
in 2 s and printed HEAD's numbers exactly. I would not report "identical" on a build that
fast without proof, so I removed the worktree, re-created it and checked:

- no bin folder existed before the build
- the worktree's CwDecoder.cs differs from HEAD's
- the built Hamlet.RadioEngine.dll hash is f3aa79e6 there and d9f29fa0 at HEAD
- CwEmissionGateTests there is 7 of 8, with `NoSpeedIsNamedWithoutCharactersToNameItFrom` red. That is #24's test, red before 7e65aac4, as unit 402 recorded.

The fast build was a warm compiler server, not a stale DLL. Both runs printed the same
thing. The worktree was removed both times by a trap that runs on every exit, and
`git worktree list` now shows:

```
C:/Source/HamLet                       c4db4090 [main]
C:/Users/TimDi/preflight-trees/206bd90 263949d7 (detached HEAD)
C:/Users/TimDi/preflight-trees/8e3ee27 07f0397a (detached HEAD)
C:/Users/TimDi/preflight-trees/f595938 351784ae (detached HEAD)
```

There is no HamLet-wt403. The three preflight-trees were there before this unit, and I
did not touch them.

**Task 3, the fixtures.** The a902cdf8 side ran in the same worktree build as the task 2
printer, twice, with identical rows. The HEAD side is the entry run. src did not change
after it, and the exit run's 37 rows match it byte for byte.

**Task 4, the exit round.**

- Floors: captures 37 of 37 with every row identical to entry, adjudicated 13 of 13, synthetics 2 of 2.
- Engine line: 178 of 178 in 372 s.
- App line: 276 of 278 on the first run, 277 of 278 on the re-run. All three losses were `InvalidProgramException: You've caused dispatcher loop` before any assertion, under a different name each time:
  - `TheFavoritesAreUnderTheGreenZoneTests`, two of them
  - `ThePsk31ConversationCardTests.NoSlotClockUnderPsk31AndFt8AndFt4StillShowIt`
- Under HM-DEC-155 the app line is 278, with the losses counted neither way.
- **`git diff 6c287f0f HEAD -- src` prints nothing.** This unit changed no file under src.

**Status cadence.** PROJECT_STATUS.md was written through tools/status.sh with the real
clock at the start, before every dotnet test, inside the worktree script before each
step, and after every task. No run went past ten minutes without a write. The longest
gap was the 377 s engine line.

## 2. What the owner should expect

Laid side by side, the two transcripts are the same string, character for character:
mostly unsure-character placeholders, with a short run of letters in the middle. Every
count matches, down to 61 characters, 45 unsure, 93 elements and 30 wpm. Both builds
also produce the same numbers on all 37 recordings the floors hold. On this recording,
read this way, the decoder before 7e65aac4 and the decoder at HEAD cannot be told apart.
That does not say either transcript is what was sent, and nothing here can say whether
the capture was read better last night. It does say that whatever Tim heard change is
not in this commit's effect on this file.

**What will look wrong but is not:**

- **The harness figures are not the sidecar's.** The harness reads 61 emitted, 45 unsure, 93 elements, 615 Hz. The work instruction quotes the sidecar as 40, 27, 64 and 625 Hz. The live app decodes from the sound card mid-stream with its own pitch setting; the harness decodes the file from its first sample at 600 Hz. The speed agrees at 30 wpm. I could not check the sidecar itself, because it is not in the tree (section 4).
- **The app line shows a red on each run.** Those are the dispatcher losses above, counted neither way, the same pattern unit 402 recorded.

## 3. What you should see

**The two transcripts, one above the other, cw-2026-09-23-125515, settled transcript
through CwDecodeHarness:**

```
HEAD      [■ ■■ ■ ■■ ■ ■■■ ■■■ ■EE ■ E■E ■ ■ EEE■■■■ ■ ■ ■ ■ ■ ■■ ■T ES T IVDW <BT> ■ ■ ■■ ■ ■ ■■■ ■ ■ ■■ ■ ■]
a902cdf8  [■ ■■ ■ ■■ ■ ■■■ ■■■ ■EE ■ E■E ■ ■ EEE■■■■ ■ ■ ■ ■ ■ ■■ ■T ES T IVDW <BT> ■ ■ ■■ ■ ■ ■■■ ■ ■ ■■ ■ ■]
```

`cmp` over the two printers' output lines reports them identical.

| | HEAD | a902cdf8 |
|---|---|---|
| characters emitted | 61 | 61 |
| characters unsure | 45 | 45 |
| elements seen | 93 | 93 |
| elements resolved | 93 | 93 |
| winning speed | 30 wpm | 30 wpm |
| tone admitted | 615 Hz, measured | 615 Hz, measured |

Every figure is an indication (FACT-004). Neither transcript is claimed to be what was
sent (§0.0).

**The 37 capture cases: none moved.** `TheCapturesThatDecodeKeepDecodingTests` passes 37
of 37 in both builds. The sorted per-case rows, holding characters, elements, unsure and
tone, `diff` to nothing. The files are .run-unit\unit403-rows-head.txt and
unit403-rows-before.txt, and unit403-rows-diff.txt is empty. So 7e65aac4 moved nothing
the floors can see and nothing they cannot see either.

**Proof that the a902cdf8 build held the older decoder:** the DLL hashes differ, f3aa79e6
against d9f29fa0, and CwEmissionGateTests is 7 of 8 there, with
`NoSpeedIsNamedWithoutCharactersToNameItFrom` red. At HEAD it is 8 of 8.

Files: .run-unit\unit403-head.txt, unit403-before.txt, unit403-before-run1.txt,
unit403-before-gate.txt, unit403-before-steps.txt, unit403-before-steps-run1.txt, and
the entry and exit runs, all committed.

## 4. What's blocking us

Nothing blocks a criterion. Four items are raised for the record: three mismatches with
the work instruction under its section 5, none repaired, and one observation.

1. **a902cdf8 is not the parent of 7e65aac4.** The parent is 7d109850, unit 402's
   task 1 trace. `git diff a902cdf8 7d109850 -- src` prints nothing, so a902cdf8 is the
   src state before 7e65aac4 and the comparison stands as ordered. Rejected: switching
   to 7d109850. The instruction named a902cdf8, and the src is the same at both.
2. **The sidecar cw-2026-09-23-125515.txt is not in the tree.** Only the wav is in
   tests\fixtures\cw\captured\unadjudicated. The instruction's sidecar figures could not
   be checked, and nothing about tonePeak or elementHz was seen, so there is nothing to
   park.
3. **The harness at a902cdf8 differs from HEAD's.** It compiled anyway. HEAD's was
   copied in so both sides share one harness, for the reason in section 1. Rejected:
   using the worktree's own harness, which would have mixed a harness change into the
   decoder comparison.
4. **The instruction says the capture was 22 minutes after 7e65aac4, and the repair
   does not change this capture's reading.** If the reading was better last night, the
   cause is outside 7e65aac4's effect on this file. It could be the signal, the radio or
   the app path. This unit measures only the engine through the harness, so that is left
   for Tim's ruling and not chased.
