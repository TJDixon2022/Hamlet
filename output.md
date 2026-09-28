## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt and `WORK_INSTRUCTIONS.md` both carry `PROJECT: Hamlet`, and all five of
section 0's checks hold. Hamlet confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked; the checkbox count is 42 before and after.
- No recording, fixture or telemetry was read.
- No scratch log or source copy under `.run-unit\` was committed.

**The changes, file by file** (all in `de1de4a4`):
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs` - the blocks it called.** The detector now
  keeps every block it called at the watched pitch. They are kept for 60 seconds, on the audio
  clock of every sample it was handed, which is the clock the decoder stamps its characters on.
  `BlocksBetween(from, to)` counts the blocks whose middle falls between two moments.
- **`src/Hamlet.RadioEngine/Cw/CwDecoder.cs` - a letter needs blocks (R99).** A new
  `DetectorBlocks` adds a third condition to the gate. A character is let out only if the blocks
  under its span are exactly as many as its elements, one per dit or dah. None, fewer or more,
  and it is not emitted at all: not as a letter and not as a placeholder.
- **The tolerance is 10 ms, `BlockToleranceSeconds`.** The span is widened by two hops at each
  end, because the detector reads each hop through a window two hops long and can place a block's
  edge up to one window from where the decoder places the same element. It is derived from the
  hop, not from any recording.
- **`CwDecoder.cs` - printed stays printed (R100).**
  - On the falling edge, the leading edge the screen was showing is settled into the transcript
    instead of cleared. Anything not yet shown is dropped.
  - Nothing is offered while keying is false, not even an empty edge.
  - Nothing promoted is settled a second time.
- **`src/Hamlet.App/Controls/CwScopeControl.cs` and `CwHearingViewModel.cs`.** The scope no
  longer blanks when the detector lets go: blocks and letters scroll off the left with time. The
  hover says so.
- **`src/Hamlet.App/Controls/BandGovernsTheMapPanel.cs` - one layout (R101).** The card is on the
  left, the map beside it at the mockup's one size (246 by 134), and the rig face on the right.
  That holds at every width, dial, mode and block. The row is the rig face's height, and nothing
  the card says moves a column: its strayed-frequency and licence lines wrap and scroll inside it.
  Unit 389's width rule is superseded.
- **Why the map is 246 by 134 and not as tall as the rig face.** At 327 by 178, the size Hamlet
  opens at (1100 wide) left the card about 181 px, below unit 388's 400 px floor for the green
  block's words.
- **`MainWindowViewModel.cs`** sets `DetectorBlocks` to the detector's `BlocksBetween`.
- **Records.** R99, R100 and R101 are in both copies of `PHASE_PLAN.md`, and `DECISIONS.md` has
  HM-DEC-192. Both status and outcome copies name 487, and the version went from 1.13.173 to
  1.13.174.

**Unit 389's tests that had to change.** Only their placement and size assertions changed:
- `TheSunMapStandsWhereItWasLeftTests.ItIs393By214AtTheLeftEdgeAnd327By178BesideTheCard` is now
  `ItIs246By134BesideTheCardAtEveryWidth`. Every case is beside the card at 246 by 134, and its
  band-height check is unchanged.
- In `TheTopRowTests.TheWorldClockIsAtTheCardsRightEndWithOneMarker`, the clock is beside the card
  in every case, at the map's one height.
- In `Unit376TheTopBandTests.TheSunMapIsTheSizeItWasAndStillCarriesHisGrid`, the case table is
  beside the card throughout, and the map is 134 tall at every width.

**Watched failing first.**
- **`NoDetectionNoLettersTests.ALetterReadFromNoiseDoesNotReachTheScreen`**, with a burst of uneven
  blips inside the open window. With the blocks unasked, which is unit 486's gate, `E`s from the
  burst reached the screen. With the blocks asked, nothing did.
- **`PrintedStaysPrintedTests`**, run on HEAD in a worktree: the screen read
  `<AR>Q CQ EIE N,CALL K` while keying and `<AR>Q CQ EIE N,CALL` after the silence, with the `K`
  gone. Now the screen after the silence still holds everything shown while keying.
- **`TheLayoutIsOneLayoutTests`**, run on HEAD in a worktree:
  - At 1920 wide the map stood at the band's left edge (393 by 214).
  - At 1100 it stood beside the card (246 by 134).
  - The three dials did not move anything at either width headless; unit 484's fix holds there.
  - Now the arrangement and the map are the same across all six cases.

**Verification.**
- The build: 0 warnings, 0 errors.
- The app carry-forward line: 276 of 278. The two failures are the dispatcher-loop loss,
  `TheCarrierHoldsTheButtonsTests` and `TheWindowHoldsBelowItsMinimumTests`, and both pass alone,
  8 of 8 and 3 of 3.
- The layout, scope and transcript app types pass. Two clock-dependent names failed once and
  passed alone: `NothingLeftTheWindow…` and `TheBestBetPill…`.
- The engine detector and gate types: 16 of 18. The two reds are
  `AMarkIsTheEnvelopeOverAThresholdTests`' ten and fifteen decibel cases, with the same counts as
  before (40 and 208).

## 2. What the owner should expect

1. Rebuild.
2. On a band with no CW: nothing on the scope and nothing in the terminal.
3. On a station: blocks, letters over them, and no letter that does not sit over its blocks. A
   letter the detector did not hear as flat-topped marks, one per dit and dah, is not printed.
4. Text once printed never vanishes, whatever the detector does. The only thing that clears it is
   your own Clear.
5. The window looks the same at 14.069, at 14.070 and at 14.076, in CW and in data, narrow and
   wide: band row on top, card on the left, the sun map beside it, the radio on the right.

**The cost, measured.** On the test's clean call, `CQ CQ DE N0CALL K` at 9 WPM, the settled text
went from 14 characters to 11. The three dropped were an `<AR>`, an `E` and a comma, none of which
the call sent. None of the letters the call did send were lost to this rule. The decoder's own
misreads on that synthetic signal are unchanged.

## 3. What you should see

| | settled text | characters |
|---|---|---|
| Clean call, gate alone (unit 486) | `<AR>Q CQ EIE N,CALL K` | 14 |
| Clean call, a letter needs blocks | `Q CQ IE NCALL K` | 11 |
| Call with a burst, gate alone | `CQ CQ E IE N9 RALL A` | 14 |
| Call with a burst, a letter needs blocks | `CQ CQ  IE N RALL A` | 12 |

- **The scope** keeps what it drew after the station stops, and it scrolls off to the left.
- **The top of the window** has one arrangement everywhere, and the sun map is always the
  mockup's size.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The leading edge is still revised while a station is keying.** The decoder decides late and
  rewrites the tip, so in the test the screen shrank 200 times mid-call (for example `N■` became
  `N`). R100 is met at the falling edge. Stopping this would mean showing only settled text, which
  costs seconds of lag, and that is the owner's call.
- **The detector can call a burst blip a block** when it sits at the station's own level right
  after a station's mark. The rule then lets it through by design, because the detector heard a
  flat-topped bar.
- **An isolated single mark is never called a block**, because the detector pairs bars. A lone
  `E` or `T` with no neighboring bar in its window is not printed.
- **The sun map is 246 by 134 everywhere, smaller than the 393 by 214 it had at wide windows.**
- **Pre-existing app reds, not this unit's:** `HowMuchTheApplicationSaysTests` (the CW tab at 585
  against 550) and `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
- **`AMarkIsTheEnvelopeOverAThresholdTests`** has two cases red since before unit 485.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, new, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is
  ever revised, at the cost of seconds of lag. It is waiting on the owner; no change sits in the
  tree.

Unit 484's strayed-line question and unit 485's screen 1 question are answered by R101: there is
one layout, and the strayed line no longer moves anything.
