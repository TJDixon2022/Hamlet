## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt and `WORK_INSTRUCTIONS.md` both carry `PROJECT: Hamlet`, and all five of
section 0's checks hold. Hamlet confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box in `PHASE_PLAN.md` was ticked; its checkbox count is 42 before and after.
- No recording, fixture, floor or telemetry was read.

**The changes, file by file** (all in `ada6b884`):
- **`src/Hamlet.RadioEngine/Cw/CwDecoder.cs` - no detection, no letters.** A new `KeyingGate`
  takes the detector's keying verdict.
  - The decoder keeps the stretches of its own audio clock during which the gate was open. Each
    stretch starts `CwEnvelopeDetector.KeyingSeconds` early, because that is the window in which
    the detector saw the bars that opened it.
  - A character reaches the transcript, the leading edge and the scope only if its audio lies in
    one of those stretches. It is judged by when the audio was heard, not by when it settles,
    because the settled pass runs seconds behind: the last letters of an over settle after the
    detector lets go.
  - The lattice, the unit estimator and the emission gate are not changed.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`** sets the gate to the detector's
  `Reading.Keying` when listening starts. That is the wiring criterion 12.4 named and nobody had
  built.
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs` - the station is held through its gaps.**
  - A new `HoldSeconds` of one second has a remark saying it is a word gap at the slow end of
    ordinary sending, not fitted to any recording.
  - While any bin is keying, the detector's own choice of pitch stands and the hold follows it.
  - In a gap, keying and the watched pitch are held. They are let go only when no mark has
    arrived at that pitch for `HoldSeconds`, counted from the last mark itself.
  - The hold clears on a new passband.
- **`CwScopeControl.cs`, `CwTrainingGraph.cs` and `CwHearingViewModel.cs` - blocks only.**
  - The level trace is removed from the frame, the graph and the drawing, and from the lines and
    item kinds.
  - A letter is drawn only with a block beneath it; the hover and the render read the same list.
  - With no keying the panel is empty apart from the tone and mixing words.
  - The hover text is rewritten for blocks, letters and the empty panel.
- **Tests.**
  - `NoDetectionNoLettersTests` is new. It writes its own audio: a nine words a minute call
    between four seconds of noise, with nothing read from disk.
  - Five existing scope tests asserted the trace or a letter over no block. They were rewritten
    to R97.
- **Records.** R97 is in both copies of `PHASE_PLAN.md`, `DECISIONS.md` has HM-DEC-190, both
  status and outcome copies name 485, and the version went from 1.13.171 to 1.13.172.

**Watched failing first.** Before the change, the same audio at 9 WPM behaved like this:
- The detector dropped keying for 49 hops mid-call.
- The ungated decoder printed an `E` at 29.0 s, after the call had ended at 27.3 s.

After the change:
- Keying held from 4.56 s to 28.33 s with no drop.
- The call read `<AR>Q CQ DE N,CALL KK`, with the decoder's own misreads at that speed unchanged,
  and nothing was emitted in either silence.

At 10 WPM with light noise both faults were absent. That speed was measured first and could not
fail, so the test moved to 9 WPM.

**The top of the window** was already built by unit 484: placement by width alone, and the
licence line following the block. Nothing here changes it.

**Verification.**
- The build of `Hamlet.sln` with warnings as errors: 0 warnings, 0 errors.
- The app carry-forward line: 278 of 278.
- The seven app scope and verdict types: 29 of 29.
- The four engine detector and gate types: 13 of 15. The two reds are
  `AMarkIsTheEnvelopeOverAThresholdTests`' ten and fifteen decibel cases. They fail identically on
  HEAD's detector (40 and 208), so they were not caused by this unit.

## 2. What the owner should expect

1. Rebuild.
2. On the CW tab with nobody keying: an empty panel and an empty terminal, with only the tone
   and mixing words at the top.
3. When somebody keys: blocks appear, short for a dit and long for a dah, with the letters over
   them, and the terminal fills only then. Between words the picture stays put rather than
   flickering, because the detector now holds the station through its gaps.
4. When the station stops, the blocks and letters leave about a second later.
5. Turning the dial into the data block should leave the top of the window where it is.

One thing to watch for. A station too weak or too broken for the detector to find will now print
nothing at all, even where the decoder alone would have read something. In one noisy synthetic
case the decoder read the call and the detector never found it. That is the ruling working as
written.

## 3. What you should see

- **Quiet band:** no line, no blocks, no letters, no terminal text.
- **A station keying:** flat-topped blocks at the mark lengths, a letter over each group, and the
  terminal filling at the same time.
- **Slow sending, around 9 to 10 WPM:** no flicker between words. At 5 to 8 WPM a word gap can
  run past one second, and the panel may blink once between words.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The layout the owner calls screen 1 is not what wide windows get.** Neighborhood on the left
  and map on the right is what narrow windows get. At wide windows, unit 389's rule, which this
  order keeps, puts the map at the band's left edge. Both are decided by width alone now, so
  neither moves, but which one he wants at wide windows is his ruling.
- **The strayed-frequency line can still move the map for a moment** while the mode catches up.
  The choice is put to the owner in the last conversation, options A to C; A is recommended.
- **A one-second hold does not bridge a word gap below about 8 WPM.** If slow senders blink, the
  figure is the owner's to change.
- **The decoder is not reset when keying starts.** Characters from before the silence are simply
  not let out, so the decoder's own reading is untouched, as section 3 asks.
- **`AMarkIsTheEnvelopeOverAThresholdTests` has two cases red at HEAD, 40 and 208 misjudged
  hops.** They are not on any carry-forward line.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 484's strayed-line question** (options A to C), 2026-09-28, waiting on the owner; no
  change sits in the tree.
