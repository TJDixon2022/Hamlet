```
UNIT: 509 - complete at task 3 of 3, none dropped - 2026-09-30
UNIT GOAL: a letter on the scroll stays on the scroll (the order headed 508)
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`; the order's gate held: `SHACK_FACTS.md`, `CwProbabilisticDecoder.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, the root is `C:\Source\HamLet`, and `PROJECT_CARD.md` says Hamlet. Nothing in this report is evidence about the radio.

**Numbering.** The order is headed work instruction 508, but 508 was the icon unit that finished minutes earlier, and HM-DEC-211, which it named, was already used by unit 506. Following the owner's ruling for the order headed 502 (run as 504), this ran as **unit 509** with **HM-DEC-213**. The order gives no task count, so I split it into three tasks: the red tests, the change, and the records.

SESSION.lock was taken through `tools\arbiter\lock.bat take` and released at the end. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, no box was ticked, and no ruling was added to either plan. **Nothing in the engine changed**: `git status src/Hamlet.RadioEngine` is empty.

**What the frame drew before, and what it draws now.**
- **The mismatch with the order's diagnosis.** The scroll's letters were never rebuilt from the reader's state. They were already kept in a list in `CwTrainingGraph`, appended on `CharacterSettled` (the same event `Transcript.Settle` prints from) and trimmed only by time.
- **The blink came from the frame.** `CwScopeControl.DrawnLetters` drew only the kept letters that had a block beneath them *on that frame*. The detector rebuilds its last four seconds of blocks every tick, so a printed letter whose blocks moved or came late went out, drew late, or never drew.
- **Now** `DrawnLetters` returns every kept letter. The list is still owned by the graph that the scope feed holds, not by the control. The order asked for "a list the control owns"; the graph's list already does exactly that job, and a second copy inside the control would be two copies to drift.

**File by file.**
- `src/Hamlet.App/Controls/CwScopeControl.cs`: `DrawnLetters` no longer filters on blocks, and its remarks and those of `Items` now say so.
- `src/Hamlet.App/ViewModels/MainWindowViewModel.cs`: `ClearTerminal` also clears the scroll's graph. **This is a second mismatch**: the order says Clear clears the scroll "as now", but it did not. Only the transcript was cleared.
- `tests/Hamlet.App.Tests/ViewModels/TheScrollKeepsItsLettersTests.cs`, new: five cases on the live path. The audio is synthetic, written in the test, and every 50 ms frame is sampled.
- `tests/Hamlet.App.Tests/ViewModels/TheBarsCarryTheirLettersTests.cs`: `ALetterWithNoBlocksBeneathItIsNotDrawn` (unit 485, R97) is replaced by `APrintedLetterWithNoBlockBeneathItIsStillDrawn`, since the order requires every printed letter to be drawn.

**Watched failing first, and after.**
- Before the change, only the 5 WPM Farnsworth call was red: E printed and never drawn; 0 and C drawn 8 and 4 frames after they printed; 0, C, N and N gone before the left edge.
- The 20 WPM call, `TEST DE W1AW K` and loud noise already passed. The order expected cases 1 to 3 to be red, and on clean synthetic audio they were not.
- After the change, all five pass.

**Checks.**
- Build `Hamlet.sln` with warnings as errors: RC=0.
- The five scroll cases: 5 of 5.
- The scope tests beside them (`OneDecoderOneTruthTests`, `TheBarsCarryTheirLettersTests`, `TheLetterSitsOverItsBarsTests`, `TheScopeDrawsLiveTests`, `TheScopeIsTheMiddlePictureTests`): 19 of 19.
- App carry-forward line: 278 of 278.
- Version 1.13.195 to 1.13.196.

**Records.**
- `PHASE_OUTCOME.md` (both copies) gains `## UNIT 509 - STEP 12`.
- `PHASE_STATUS.md` (both copies) names 509.
- The `CLAUDE.md` §1 index row is added.
- HM-DEC-213 is in `DECISIONS.md`, in full:

> **A letter on the scroll stays on the scroll.** Tim, 2026-09-30, on W1AW fast code practice: *"I don't like how the scrolling letters seem to blink in and out depending on your confidence."*
>
> **What was wrong.** It was not confidence; it was the redraw. The terminal has kept what it printed since unit 487 (R100). The scroll kept its letters too, in the training graph, appended on the same `CharacterSettled` event the terminal prints from, but every frame rebuilt which of them to draw: only the letters with a block beneath them on that frame. The detector rebuilds its last four seconds of blocks every tick, so a letter whose blocks moved or came late went out, drew late, or never drew.
>
> **What is built.** The scroll draws every letter the terminal printed, from that one event, on the frame it prints, over the span it was read from, and it stays until time carries it off the left. Nothing removes it but the edge and the owner's Clear, which now clears the scroll with the terminal. A letter the terminal did not print is never drawn. Word gaps draw nothing, and the blocks are as unit 507 left them.
>
> **What it supersedes.** The scroll's rule from work instruction 485 under R97, that a letter with no block beneath it is not drawn. R102 had already taken the detector's keying and blocks off what is emitted, and R106 makes the scroll's letters the terminal's. So a letter the terminal printed now draws even where the detector drew no block under it; at 5 WPM Farnsworth one E does. The order directed this; it is Tim's to overrule.

## 2. What the owner should expect

- Rebuild.
- Every letter the terminal prints appears over its blocks on the scroll at the same moment, and stays there until it slides off the left.
- No letter blinks, and no group that became a letter is left bare.
- Clear now empties the scroll along with the terminal.
- Nothing about detection or decoding changed.
- **What may look odd but isn't a fault:**
  - A slow sender's letter can appear a moment before the blocks under it, because the detector finishes a block after the reader prints. On the 5 WPM test the blocks arrived within about a second.
  - Very rarely a letter sits over empty space: the detector drew no block for it, but the terminal printed it.
- `DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes` is still red on the index gaps 166, 182 and 189 to 210, as before this unit.

## 3. What you should see

| Case | Before | After |
|---|---|---|
| 1. `CQ CQ DE N0CALL N0CALL K`, 20 WPM: every printed letter drawn, in order, over its blocks | kept faith (19 letters) | pass |
| 2. The same, frame by frame: a letter drawn stays until the edge | kept faith | pass |
| 3. `TEST DE W1AW K`: T and E draw when they print, over their blocks, and stay | kept faith (11 letters) | pass |
| 4. The call at 5 WPM Farnsworth: late letters draw late, in place, and stay | E never drawn; 0, C drawn 8 and 4 frames late; 0, C, N, N gone before the edge | pass |
| 5. Loud noise: no letter on the scroll | no letter | pass |

In case 4, three letters draw with no block under them at the moment they print:
- 0 gains its block within about a second: a block under it on 134 of the 153 frames it is shown.
- C does the same: 146 of 153.
- E, a 60 ms span, has no block under it on any of its 79 frames.

The call's first C printed 8.6 s after it ended, already past the left edge, so it cannot appear on the scroll.

## 4. What's blocking us

Nothing blocks. Three items:

1. **A printed letter with no block under it now draws.** The order requires it, and it supersedes unit 485's R97 rule for the scroll. If you would rather the scroll keep a letter off until a block stands under it, that is a ruling, and it would bring back the late draws the order removes.
2. **At 5 WPM Farnsworth the reader can print a letter more than 8 s after it ended**, past the left edge of the 8-second scroll, so that letter is never on it. This comes from the reader's hold, not from this unit.
3. **Numbering.** Two orders both headed 508 now exist in the history: the icon, and this one run as 509. The next order's number should be 510 or later, and its ruling id HM-DEC-214 or later.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
