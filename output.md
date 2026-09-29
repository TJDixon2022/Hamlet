## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked; `PHASE_PLAN.md` has 69 before and after.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed.

**Section 3, the rollback, is done, and section 4 is done too** (all in `f270dfda`).

**The three switches**, in `src/Hamlet.RadioEngine/Cw/CwDecoder.cs`. Each defaults off and carries
a remark naming R102, unit 488's measurement, and that it exists to turn the gate back on:
- **`DetectorSteersPitch`.** Off, the mixing rung is the operator's lock and then the tracker, as
  before unit 486. `MixingHz` stays and still reports where the decoder really mixes.
- **`DetectorGatesKeying`.** Off, unit 486's keying gate and unit 487's promotion on its falling
  edge decide nothing.
- **`DetectorGatesBlocks`.** Off, unit 487's block rule decides nothing.

**The other files:**
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`.** The tab still wires the detector's
  pitch, keying and blocks to the decoder, and leaves the switches off, with a remark saying so.
- **`src/Hamlet.App/ViewModels/CwHearingViewModel.cs`**, section 4:
  - The scope's lines read *tone N Hz heard* and *decoding at N Hz*.
  - The hover no longer says the terminal waits on the detector. It says the two can disagree, and
    that the decoder finds its pitch for itself.
  - "not mixing" is unchanged.
- **Tests.**
  - New: `TheDecoderGetsItsEarsBackTests`.
  - The gate tests now drive their switches on, and nothing else in them changed:
    `NoDetectionNoLettersTests`, `ThePitchTheDetectorFoundReachesTheDecoderTests`,
    `PrintedStaysPrintedTests` and `TheScopeDrawsLiveTests`.
  - Three test lines pin the new panel words: two in `TheScopeIsTheMiddlePictureTests`, one in
    `TheScopeShowsTheMarksTests`.
- **Records.**
  - R102 is in both `PHASE_PLAN.md` copies, and `DECISIONS.md` has HM-DEC-194.
  - Both outcome and status copies name 489.
  - Version 1.13.175 → 1.13.176.
- **Nothing was deleted.** Every constant, remark and test is still there.

**Watched failing first.** Before the switches, the wired decoder read 5 characters where the
unbound one read 19 (section 3 below). After, both read 19.

**Verification.**
- The build: 0 warnings, 0 errors.
- **The app carry-forward line: 276 of 278.** The two failures are dispatcher-loop losses at 1 ms
  each, `ThePsk31OfferTests` and `TheChipSaysTheChosenModeTests`, and both pass alone (2 of 2 and
  6 of 6).
- **The engine gate and detector types: 20 of 23.** The reds are unit 488's named
  `ThatPitchIsTheStationsOwn` and `AMarkIsTheEnvelopeOverAThresholdTests`' two cases, at the same
  counts as before (40 and 208).
- **The app scope, transcript, layout and voice types: 86 of 87.** The red is
  `VoiceTests.NoOperatorFacingStringUsesABritishSpelling`: "centre" at two lines of
  `MainWindowViewModel.cs`, from `734f72fd2` on 2026-09-26. It is not from this unit and was left
  alone.

## 2. What the owner should expect

1. Rebuild.
2. W1AW and other strong stations should read about as they did a week ago, junk between the words
   included.
3. The scope still shows blocks, letters only over blocks, and nothing when the detector hears
   nothing.
   - **The scope and the terminal will now disagree, and that is the point.** The terminal is free
     again; the scope shows what the detector can still only partly do.
4. The panel reads *tone N Hz heard · decoding at N Hz*. These are two different numbers on
   purpose: the detector's pitch and the decoder's own.
5. Nothing printed vanishes, and the layout and the preamp are unchanged.

## 3. What you should see

The test's call, unit 488's synthetic station at 625 Hz and 23 WPM, sending
`CQ CQ DE N0CALL N0CALL K`:

| decoder | settled text | characters |
|---|---|---|
| wired as the tab wired it, before this unit | `RE    N  D   K` | 5 |
| the same decoder, unbound | `CQQ   QDEN0CALL N0CALL K` | 19 |
| wired as the tab wires it now | `CQQ   QDEN0CALL N0CALL K` | 19 |

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The detector's pitch is still a shoulder two hops in three.** The 625 Hz bin calls no bars of
  its own on those hops. Fixing that is what turns the switches back on;
  `ThatPitchIsTheStationsOwn` is the red test waiting for it.
- **The terminal's text is the decoder's own again, junk included.** That is last week's
  behavior, by R102.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling ("centre", two lines, since 2026-09-26).
  - `HowMuchTheApplicationSaysTests` (the CW tab at 585 against 550).
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. It is waiting on the owner; no change sits in the tree.
