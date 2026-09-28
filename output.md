## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt and `WORK_INSTRUCTIONS.md` both carry `PROJECT: Hamlet`, and all five of
section 0's checks hold. Hamlet confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box in `PHASE_PLAN.md` was ticked; its checkbox count is 42 before and after.
- No recording and no telemetry was read, and no test was written (R96).

**The changes, file by file** (all in `c269ba8e`):
- **`src/Hamlet.App/Controls/CwScopeControl.cs` - silence is empty.** `Items`, the one list
  everything drawn and every hover comes from, adds the level trace and the bars only while
  `frame.Reading.Keying` is true. With nobody keying the plot is empty. The tone and mixing words
  and the letters are drawn as before, and the detector is not touched.
- **`src/Hamlet.App/Controls/BandGovernsTheMapPanel.cs` - the top row holds still.** See below.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs` - the licence sentence follows the
  block.** A new `LicenceModeHere` gives the card the mode of the band-plan block the frequency
  is in: Morse, data or voice. It uses the tab only on open or unclaimed ground.
  `UpdatePrivileges` is the one call site of `PrivilegeStatusLine.Build`, and it runs on every
  frequency change and every tab change. So *"Your General license covers Morse here"* is not
  written on a data-block frequency, whether the dial or the app put him there. This is display
  only: the card and the green zone read it, and no send path does.
- **Records.**
  - `DECISIONS.md` has HM-DEC-189.
  - R96 is appended in the owner's words to both copies of `PHASE_PLAN.md`.
  - Both copies of `PHASE_STATUS.md` name 484.
  - Both copies of `PHASE_OUTCOME.md` have `## UNIT 484 - STEP 11`.
  - The version went from 1.13.170 to 1.13.171.

**What could move the layout before, and why it cannot now.** `BandGovernsTheMapPanel` asked two
fit questions. Both measured the neighborhood card's current height:
- `AtTheLeftEdge` put the map at the band's left edge only if the card, at that width, was no
  taller than the row.
- `CardFits` chose the map's height by halving until the card's words fitted.

So the card's word count decided where the map stood. The radio sends the new frequency before
its mode, and in that moment the card carried more lines than it fitted. The check failed, and
the map jumped from the left edge to beside the card: that is the swap. A frequency chosen from
the app arrives with its mode in one pass, so the card was measured once and nothing moved.

Now:
- **Both questions ask only widths**: the card's 400 px floor and the pills' row.
- **The strayed-frequency line is checked by name.** That is unit 389's rule, kept as it was.
- **The row's height is the rig face's plus the outside-privileges lines' (R62).** Every other
  word of the card wraps and scrolls inside the card's existing `ScrollViewer`.
- **What the arrangement depends on** is the width, the rig face, the pills and the strayed line.
  It no longer depends on the mode, or on how much the card says.

**Verification, as R96 asks.**
- The build of `Hamlet.sln` with warnings as errors: 0 warnings, 0 errors.
- The app carry-forward line: 278 of 278 on the first run, with no dispatcher-loop loss this
  time.

**Pushed** to `origin/main`: `e63032be..99e51314`, and this report's own commit after it. The
first closing commit carried unit 483's stale `output.md`, because the write failed on a changed
file. This commit replaces it.

## 2. What the owner should expect

1. Rebuild.
2. On the CW tab with nobody keying, the scope should be empty: no trace and no noise line, just
   the tone and mixing words at the top. When someone keys, the trace and the bars appear, with
   the letters over them.
3. Sit on a CW frequency and turn the radio's dial into the data block. The top of the window
   should not move: the map, the neighborhood card and the radio face stay where they are.
4. On that data-block frequency, the card should talk about data, not say that your licence
   covers Morse there.

If the neighborhood card has more to say than fits beside the radio face, it now scrolls inside
its own space instead of pushing the window around.

## 3. What you should see

- **The scope in a quiet band:** empty. The letters the decoder settled in the last eight
  seconds can still show for a moment after the keying stops, because the order kept the
  letters.
- **The top row:** the same arrangement before and after the dial crosses into the data block,
  at any window width. The one move left is unit 389's rule: the map leaves the band's left edge
  while the strayed-frequency line is showing.
- **The card at 14.074 on the CW tab:** it answers for data, the block's mode. On open ground it
  answers for the tab you are on.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The strayed-frequency line can still move the map for a moment.** It is kept as instructed,
  and it depends on the dial and the sub-mode. If the owner sees a brief jump while the mode
  catches up, that line is the cause, and the next change is to hold the arrangement across it.
- **Card text now scrolls where it used to grow the row.** At the narrowest window, lines below
  the rig face's height need a scroll to read.
- **A letter can stay over empty space for up to eight seconds after the keying stops.** The
  order kept the letters; hiding them too is one line, on the owner's word.
- **Unit 251's rule that the card follows the tab is reversed by this order.** It is reversed
  only where a block names one family, and HM-DEC-189 does not record that reversal.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.

Units 441 to 483 ran through the loop, and their reports were overwritten before this unit. Any
asks they left open are in their outcome entries and are not reconstructed here.
