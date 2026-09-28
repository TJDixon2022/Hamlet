READ IN THIS ORDER.

A. What the CW tab shows now. Under the transcript is one picture: the last four seconds of the level of the one pitch the detector is reading, drawn as a single trace. Along the bottom is a filled bar under every mark the bar detector calls, and nothing under a gap. Top left are two short lines of words: "tone 742 Hz" while the detector says keying (or "no keying"), and beside it "mixing 742 Hz", the pitch the decoder is on (or "not mixing"). To the right of the picture are the two buttons, "I agree with you" and "You're an idiot", writing the same row as before. The light, the pitch strip, the floor line, the threshold line and the shaded passband are gone.
B. Step 12: 12.2 is rewritten as the middle picture and is green on synthetic audio. It is not ticked, because the instruction does not say to tick it and the owner's eye is the test. Step 11: 11.1 and 11.2 are retired, with the clause appended in both PHASE_PLAN.md copies; neither is unticked.
C. The rest: the red and green runs, two places where the instruction does not match the tree, and three deletions the session could not make. Section 4 raises 5 items, none blocking. No recording was read.

UNIT:       478 - complete at task 3 of 3, none dropped - 2026-09-28 10:50
PHASE GOAL: make Hamlet meet the CW requirements in CW_REQUIREMENTS.md and CW_SPEC.md. While R88 bans the corpus, only steps 11 and 12 (the owner's ear and the oscilloscope) can be worked.
UNIT GOAL:  show the owner what the detector sees, as the picture he said convinced him: a level trace with bars where the keying is, and nothing else
ADVANCES:   step 12 criterion 2
ADVANCED:   yes - 12.2's picture is now the one R92 asks for, proved headless on a driven detector; whether it matches his ear is his call at the radio
NUMBER:     lines on the scope: trace and bars, 0 thresholds; controls removed: light, strip; recordings read: 0
DRIFT:      0 consecutive units without advance (the instruction carried no count)

## 1. What Claude did

**Complete at task 3 of 3, none dropped.** Claude Code on QUIVERFULL, project Hamlet confirmed by the five gate checks, branch `main`, entry `1b738a76`. Tasks are pushed as `600aa01d` (task 0), `1bd93b2b` (task 1), `8b4d5406` (task 2), and this report's commit for task 3. `SESSION.lock` and an empty `STOP` were already at the root when the session started. Both are the launcher's, and the session did not take, release or remove either.

**Task 0: the record and the entry round.**
- PHASE_OUTCOME.md (both copies) has `## UNIT 478 - STEP 12` from the ARBITER-DECISION block.
- PHASE_STATUS.md (both copies) names 478 with `CURRENT_STEP: 12`.
- The version goes from 1.13.163 to 1.13.164.
- The root copies of PHASE_OUTCOME.md and PHASE_STATUS.md held uncommitted launcher edits at entry. They are carried in the task 0 commit as found (section 4, item 5).
- Entry build: 0 errors with warnings as errors.
- App line: 277/278, then 277/278 on the one re-run. Both failures were "You've caused dispatcher loop" (`TheWindowHoldsBelowItsMinimumTests`, then `TheCarrierHoldsTheButtonsTests`), so neither run is counted (HM-DEC-155).

**Task 1: the scope becomes the middle picture.** `CwScopeControl` now draws four things and nothing else. `CwScopeControl.Lines(frame)` is the list the renderer draws from, and it holds exactly these four:
- **The trace, "level":** the detector's watched bin, hop by hop, as one line.
- **The bars, "marks":** a filled block under every marked hop, contiguous, so a dit is short and a dah long. Each bar carries its length where it fits, and nothing is drawn under a gap.
- **Tone:** "tone 742 Hz" or "no keying". It shows the pitch the detector measured at its last mark, and holds it across gaps while `Reading.Keying` stays true. The detector only names a pitch while a mark is up, so without this hold the line would flicker at every gap. The frame is handed the previous frame to do this; nothing is guessed.
- **Mixing:** "mixing 742 Hz" from `DecodeReport.ToneHz` while decoding, or "not mixing".
- **Removed:** the floor line, the threshold line and their labels, the passband rail with its shading and tone tick. `CwScopeFrame` loses `ToneLine`-as-field, `PassbandLabel` and `ThresholdLabel`, and gains `ToneHz` and `MixingHz`.
- **The detector is not changed.** It still computes floor, threshold and passband, and the verdict row still carries them.
- **Hover:**
  - over the plot: "the level of the bin the detector is reading, over the last four seconds";
  - over a bar: "a mark - the level held flat for at least a dit";
  - anywhere else: the rewritten whole-scope tip.

  The control sets the tip as the pointer moves.
- **Red first:** `TheScopeIsTheMiddlePictureTests` was red 4/4 on stubs:
  - the drawn kinds were Envelope, Floor, Passband and Threshold;
  - the tone line read "no tone";
  - the mixing line and both hover texts were empty.
- **Then green 4/4:**
  - only Trace, Bars, Tone and Mixing are drawn, and no label says gap level, midway, floor, threshold, margin, filter or whole band;
  - the pitch reads "tone 7xx Hz" in a mark and still in the gap after it (1.86 s, keying, no mark up), and "no keying" before the keying and 2 s after it;
  - "mixing 750 Hz" and "not mixing";
  - both hover texts are asserted at a point over a bar and a point over the plot.
- `TheScopeShowsTheMarksTests` is updated to the new words, and its floor/threshold/passband label test is removed.

**The trace's fallback (my decision, and not what the instruction said).** The instruction says that when no bin is found, the trace should show the loudest bin's level. `CwEnvelopeDetector.History()` only ever returns its watched bin: the centre of the passband before any keying, and the last keying bin afterwards. Per-hop levels of other bins are not exposed, and the instruction forbids changing the detector. So the trace never goes blank, but it shows the watched bin, not the loudest one (section 4, item 1).

**Task 2: the temporary controls come out.**
- The light (`CwHearingLight`, `CwHearingDark`) and the strip (`CwPitchStripRow`) are gone from `MainWindow.axaml`.
- The scope and the two buttons now share one Grid, `CwScopeRow`. The buttons are stacked to the right of the scope, with the same words, commands, `IsDecoding` gate and hover texts except for the change below. Under the scope, one line reads "tone … · mixing …".
- `CwHearingViewModel` drops `IsLit`, `LitWords`, `DarkWords`, `LightTip`, `Strip`, `StripTip`, the `CwPitchStrip` record and its builders. `Observe` now only stores the state the row reads.
- **The row's key set is unchanged: 26 fields.**
  - `light` now records the scope's keying verdict as "the bars say keying" or "the bars say no keying".
  - `sinceVerdictMs` counts from the last change of that verdict (section 4, item 3).
- **The button tips said "the light".** They now say "the scope" and "what the bars said", because the light they named is gone (section 4, item 3).
- **Three files could not be deleted.** `git rm` was refused in this session, and I did not work around the refusal. `CwPitchStripControl.cs`, `TheLightSaysWhatHamletThinksItHearsTests.cs` and `TheStripShowsWhereTheDetectorLooksTests.cs` are each overwritten with a three-line comment saying they are retired and should be deleted (section 4, item 2).
- Test results:
  - `TheScopeShowsTheMarksTests` 5/5. Its placement test now asserts both buttons are right of the scope and within its height, and that no light or strip is in the CW tab's tree.
  - `TheOwnersVerdictIsARowTests` 7/7, with a new test, `TheLightFieldIsTheBarsVerdictInWords`.
- PHASE_PLAN.md, both copies: 11.1 and 11.2 now end with "- retired by unit 478 under R92; the scope shows what they showed". Neither is unticked.

**Task 3: the exit round.**
- **Build:** a full non-incremental build of Hamlet.sln with warnings as errors gives 0 warnings and 0 errors.
- **App line:** 277/278, then 277/278 on the one re-run. Both failures were "You've caused dispatcher loop" (`ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed`, then `TheCarrierHoldsTheButtonsTests.HisCardIsDrawnInTheSendingGreenWithTheWord`), so neither run is counted. That matches entry.
- **Touched types, one invocation each:**

  | Type | Result |
  |---|---|
  | `TheScopeIsTheMiddlePictureTests` (new) | 4/4 |
  | `TheScopeShowsTheMarksTests` | 5/5 |
  | `TheVerdictCarriesTheScopeTests` | 3/3 |
  | `TheOwnersVerdictIsARowTests` | 7/7 |
  | `BindingHealthTests` | 1/1 |
  | `EveryControlSaysWhatItDoesTests` | 3/3 |
  | `WhatEveryControlSaysOnHoverTests` | 1/1 |

- **`src/Hamlet.RadioEngine/Cw` against `1b738a76` prints nothing.** This unit changed the app, not the engine.
- **Transmit files:** print nothing against `7e209cb4`.
- **Fixtures:** nothing under `tests/fixtures` was touched. No recording was read.

**Where the instruction does not match the tree** (reported, not repaired):
1. §3 says the closed control list in `EveryControlSaysWhatItDoesTests` holds the light and the strip. It never did: it lists pressable controls and hint marks, and neither the light nor the strip was one. The list is unchanged, and it is green.
2. The instruction asks for a loudest-bin fallback that the detector does not expose (task 1 above).

## 2. What the owner should expect

Rebuild, tune a station, and look at one thing: **do the bars under the trace match the dits and dahs you hear?** Press the buttons as before. They now sit to the right of the scope.

- Top left, "tone 742 Hz" means the bars say somebody is keying there. Beside it, "mixing 742 Hz" is where the decoder is. If the two numbers differ, the detector and the decoder are listening at different pitches.
- Hover over the trace or a bar to see what it is.

**What will look wrong but is not:**
- The first dit or dah of an over gets its bar a moment late. It is drawn when the second element pairs with it, in the place where it happened (unit 477's rule).
- Before any station is found, the trace shows the middle of the passband, and after a station stops it stays on that station's pitch. It is never blank, and it does not jump to the loudest pitch.
- A steady carrier and plain noise draw a trace and no bars.
- The tone line keeps its pitch between elements and says "no keying" about a second after the sending stops.
- A weak station, about 10 to 16 dB over the noise, may show broken bars on long dahs. That is unit 477's named cost and unchanged.

## 3. What you should see

**The answer this unit was commissioned for:** the scope now draws exactly the trace, the bars and two lines of words, with 0 threshold lines. `CwScopeControl.Lines` returns Trace "level", Bars "marks", Tone and Mixing, and the test asserts that set closed. On a driven detector (C and Q at 20 wpm, 742 Hz, seeded noise, no recording), the eight keyed elements are eight bars with dahs 2.4 to 3.6 times the dits. The pitch reads "tone 7xx Hz" in a mark and in the gap after it, and "no keying" before and after.

On the CW tab, the light and the pitch strip are gone, and the two buttons sit beside the scope. The verdict row keeps its 26 keys.

**Nothing changes in what is decoded.** The detector, the meter, the tracker and the decoder are untouched.

## 4. What's blocking us

Nothing here blocks the unit. Five items want a ruling or an action, most consequential first.

1. **The trace's fallback is the detector's watched bin, not the loudest bin.**
   - **Ruling wanted:** accept the watched bin (the passband centre before any keying, the last station's pitch after), or have a unit that may touch `CwEnvelopeDetector` expose the loudest bin's per-hop level for the trace.
   - **Reasoning:** the instruction lists the fallback as the author's to decide, but names the loudest bin. The detector keeps per-hop history only for the bin it watches, and §6 forbids changing it. The watched bin is never blank and is what the detector actually reads, which is what the trace's hover says it is.
   - **Rejected:** computing a loudest-bin level in the app from `Bins()`. That summary carries no per-hop levels, and anything built from it would be a line the detector does not compute (§0.0).

2. **Three retired files are emptied, not deleted.**
   - **Action wanted:** delete `src/Hamlet.App/Controls/CwPitchStripControl.cs`, `tests/Hamlet.App.Tests/ViewModels/TheLightSaysWhatHamletThinksItHearsTests.cs` and `tests/Hamlet.App.Tests/ViewModels/TheStripShowsWhereTheDetectorLooksTests.cs`, then commit.
   - **Reasoning:** `git rm` was refused in this session, like `rm`. Each file now holds only a comment saying so, and the build and tests are green with them in place.
   - **Rejected:** deleting them through a script to get past the refusal.

3. **The button tips and the row's `light` field changed wording.**
   - **Ruling wanted:** accept these, or restore the old wording:
     - the tips say "Tells Hamlet the scope is right/wrong…" and "what the bars said";
     - `light` records "the bars say keying" or "the bars say no keying";
     - `sinceVerdictMs` counts from that verdict's last change.
   - **Reasoning:** the instruction keeps the buttons "exactly as they are", but their hover texts named a light that no longer exists, which would be a false sentence on screen (§0.0). It directs that `light` record the detector's keying verdict in words. I worded it unlike 474's "I think I hear CW" so a reader of the rows can tell which picture the owner was judging. `sinceVerdictMs` was "since the light changed", and the bars' verdict is what replaced the light.
   - **Rejected:**
     - leaving the tips naming the light;
     - reusing 474's words for the new verdict, which would make old and new rows indistinguishable.

4. **12.2 is not ticked.**
   - **Ruling wanted:** tick 12.2 once the owner has looked at the scope at the radio, or say what else it waits on.
   - **Reasoning:** at entry, 12.1 to 12.3 were unticked in both PHASE_PLAN.md copies, even though unit 477 ticked them; the `1b738a76` plan commit changed that. 12.2's text there still says "trace, floor, threshold, marks as bars along the bottom, the tone line as text, the passband edges shaded", which is the picture R92 replaced. The instruction does not say to tick 12.2 or to rewrite its text.
   - **Rejected:** rewriting or ticking the criterion myself. The plan is the owner's and the web session's.

5. **The launcher's uncommitted edits to the root phase files were committed with task 0.**
   - **Ruling wanted:** confirm that the root PHASE_STATUS.md and PHASE_OUTCOME.md should read "STEP: 12 | not started", or correct them. They also gained a `HEARTBEAT:` line, and the launcher had set `CURRENT_STEP: 3`, which task 0 set back to 12.
   - **Reasoning:** these were in the working tree at entry, not written by this session. The commit script stages those files whole, as unit 477's did. Units 476 to 478 all worked step 12, so "not started" disagrees with the record. The copies under `docs/phase-requirements/` did not carry the change.
   - **Rejected:** reverting the launcher's edits. They are the launcher's, and a revert would overwrite its state without knowing why it wrote it.
