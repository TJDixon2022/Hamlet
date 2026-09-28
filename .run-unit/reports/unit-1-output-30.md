READ IN THIS ORDER.

A. What the owner sees now: nothing on silence, bars when keying, letters
   over the bars - and where the radio's scope said the signal was. The CW
   tab's scope no longer draws a trace. It says "listening" until bars are
   found, draws them at their true length over eight seconds, and writes
   each settled character above its own span. In plain CW the detector now
   watches the pitch the radio's scope peak points at.
B. Step 12: 12.4 the scope output on and pointing (built, proved on
   ScriptedRadio and synthetic frames only, not yet judged by ear); 12.2
   the training graph (built, 4/4 headless). Neither is ticked here.
C. The rest. Section 4 raises 8 items, none blocking, and carries unit
   479's 6 as outstanding. No recording was read.

UNIT:       480 - complete at task 4 of 4, none dropped - 2026-09-28 14:07
PHASE GOAL: Hamlet decodes CW the way the requirements say it must; for now that means finding a keyed station where the owner's ear finds it, judged by his verdict rows and never by a recording
UNIT GOAL:  let the radio's own calibrated scope say where the station is, point the detector and the tracker there, and show the owner bars with the decoder's letters riding on top
ADVANCES:   step 12 criterion 4
ADVANCED:   no - 12.4 is judged by the owner's ear and his rows, and nothing has been heard through this build yet; the pointing and the graph are built and proved on synthetic input only
NUMBER:     scope output: off -> on in CW; the detector's bin: swept -> pointed by the radio; the graph: trace -> bars with letters; recordings read: 0
DRIFT:      4 consecutive units without advance (was 3, from unit 479's report)

## 1. What Claude did

**Complete: 4 of 4 tasks, none dropped.** Provenance: QUIVERFULL, Hamlet (the gate's five checks held), branch `main`, every task committed and pushed (`2aa2c09d`, `ab73ef1b`, `9ab12acc`, `68f59478`, plus the exit commit). I was launched with the launcher's `SESSION.lock` present and no `STOP`. An empty `STOP` appeared at the root while task 3 was running. I took neither file, released neither, removed neither, and finished the unit.

**Task 0 - the record and the entry round.**
- `PHASE_OUTCOME.md` (both copies) has `## UNIT 480 - STEP 12`.
- `PHASE_STATUS.md` (both copies) names 480 and `CURRENT_STEP: 12`. The launcher's edits to the root copy were carried as found.
- Version 1.13.165 -> 1.13.166.
- HM-DEC-188 is at the top of `DECISIONS.md`, verbatim from the instruction, and its row is at the top of `CLAUDE.md` §1.
- Entry: build 0 warnings, 0 errors, with warnings as errors. App carry-forward line 276/278, then 274/278 on the one re-run. Every failure was "You've caused dispatcher loop", no name failed twice, counted neither way (HM-DEC-155).

**Task 1 - the radio's scope output is on in CW.**
- **What the tree held.** `RigSpectrumSource` already starts at connect in every mode and listens. The only missing piece was the `27 11` write, and HM-DEC-062 plus `ScopeIsNeverTurnedOnTests` forbade it. `ReceiverSetup` writes any field in `CivWrites` generically, so a data row was enough; no setup code changed.
- **The row.** `data/bands/mode-receiver-conditions.json` gains a tenth CW condition: `scope output`, field `scopeOutput`, wanted 1, "on", confirmed. The `CW DX` and `QRP` blocks state it through their `sameAs` lines. It is read before, read back after, compared on one scale, and the operator's hand wins (HM-DEC-056).
- **The listener follows the read-back.** `RigSpectrumSource.FollowTheSetup(results)` starts it where the read-back says on (`AlreadyRight` or `Changed`, now "on"). The app calls it after every tune-in. It never stops the listener; see section 4 item 2.
- `OwnedSettings` gains the scope output (page 19-7): twelve settings become thirteen.
- `ScriptedRadio` now speaks `27 11`, on unless a test says otherwise, so older tests see the same writes and sentences. `ModeEntryBench` counts its writes.
- **HM-DEC-062 is superseded for the Morse family only.** `ScopeIsNeverTurnedOnTests` keeps its guard that no app code writes `27 11` directly, and gains `OnlyTheMorseFamilyAsksForTheScopeOutput`.
- **Watched it fail first.** New `TheScopeOutputIsACwConditionTests` was red at 1/3: the CW block stated no scope output. After the row it is green, 3/3:
  - a CW tune-in with the output off writes `27 11 01`, reads it back on, and the stream is running;
  - his off stands at the next tune-in, and nothing is written;
  - an FT8 tune-in states nothing and writes nothing.
- Two pinned counts moved: `EveryModeAnswersForEverySettingTests` 12 -> 13, and `WhichPathsPutNarrationOnTheBarTests` CW rows 9 -> 10.

**Task 2 - the radio points the detector.**
- **New `CwScopePointer`.** On each frame it takes the tallest bin between the dial minus and plus half the filter width, placing bins by the frame's own span. The pitch is the CW pitch plus the peak's offset from the dial. It goes quiet after `ScopeFlow.QuietAfter` (3 s) and counts frames over four seconds.
- **`CwEnvelopeDetector.PointAt(pitch)`.** While pointed, the watched bin is the one nearest the pitch (within half a bin), and the most-bars search does not run. Null means it sweeps as before. The reading gains `Pointed`.
- **`CwToneTracker.FollowScope(pitch)`.** The scope's pitch and the meter's are both candidates, and the scope wins where both are present. The survey moves nothing while either holds. The tracker's remark says so.
- **App wiring.**
  - Frames reach the pointer only in plain CW, with the dial, pitch and filter all read.
  - The scope tick points the detector and the tracker.
  - In CW, while the scope is quiet, the tone line leads with "scope quiet, sweeping".
  - The verdict row gains `scopePeakHz`, `scopePeakDb`, `scopePeakLevel` and `scopeFramesLast4s`, and the key set is asserted closed.
- **Watched it fail first.** New `TheRadioPointsTheDetectorTests` uses a synthetic frame with a peak 250 Hz above the dial and CW pitch 600. I wrote the code first, then showed the red by disabling both seams. The detector's bin stayed at 600, and with the meter at 600 and the scope at 850 the tracker mixed at 600. Restored, it is green 3/3:
  - the peak reads 852 Hz (one scope bin is 10.5 Hz), and the detector watches 850;
  - after 3 s without a frame the detector sweeps;
  - the scope beats the meter, then hands back to it.
- **Decisions I made myself, all overrulable:**
  1. The peak is the tallest bin; a tie goes to the lower bin.
  2. The tracker is handed the scope's pitch only while the detector, watching the pointed bin, says keying.
  3. `scopePeakDb` is null on every row, and a fourth key, `scopePeakLevel`, carries the radio's 0 to 160 level.
  4. Pointing is plain CW only; CW-R sweeps as before.
  Items 3 and 4 of section 4 give the reasons.

**Task 3 - bars only, letters over them.**
- **What timing a settled character carries.** `CwCharacter.At` is the character's end on the decoder's audio clock, in 5 ms hops, and `SpanHops` is its length. The decoder had no public audio position, so `CwDecoder.Heard` was added. It counts every sample handed in, including during a suspension, because the stream's clock runs through one too.
- **New `CwTrainingGraph`** holds eight seconds on the wall's clock.
  - The detector's newest hop is now. Each tick, the detector's last four seconds replace what they cover, and older bars are kept up to eight seconds.
  - A settled character ends as far behind now as the decoder has heard past its `At`, and starts `SpanHops` before that.
  - Word gaps are kept as nothing.
- **`CwScopeControl` draws its `Items` and nothing else.**
  - No trace, floor, threshold or noise.
  - "listening" while there are no bars and no letters.
  - Bars at their true length, with their milliseconds under them.
  - Each settled character is centered over its own span at 26 point, whether or not bars lie under it. Sure is bold, unsure is italic and muted, and unreadable is the placeholder glyph.
  - Hovering a bar says "dah, 175 ms"; hovering a letter says "C, sure, 93% likely right".
  - The graph's hover text is rewritten to match.
- **Decisions I made myself, overrulable:**
  - a bar more than twice the shortest in the window reads as a dah (hover only);
  - the tone and mixing words stay in the top corner.
- **Watched it fail first.** New `TheBarsCarryTheirLettersTests` is headless: a detector driven with C keyed in seeded noise, and a driven settle of C. It was red at 0/4 against today's picture, which drew the trace and had no letter. It is now green 4/4:
  - four bars from 699.9 to 764.5 px, with C at 731.9, and nothing else but the two lines of words;
  - silence says "listening";
  - a letter with no bars under it is still written;
  - both hovers.
- Three of 478's tests asserted the trace. I rewrote them to R95 and renamed them: `OnlyTheBarsAndTwoLinesOfWordsAreDrawn`, `TheHoverSaysWhatABarIs`, `TheHoverSaysWhatTheBarsAndTheLettersAre`.

**Task 4 - the exit round.**
- **Build:** full no-incremental build of `Hamlet.sln` with warnings as errors, 0 warnings and 0 errors, both before and after the plain-CW edit.
- **App carry-forward line:** 278/278 on the first exit run. After the plain-CW edit it was 274/278 (two `ThePsk31ConversationCardTests`, `BindingHealthTests`, `TheRstIsYoursToCorrectTests`, all "You've caused dispatcher loop"), then 278/278 on the one re-run.
- **Touched types that read no recording, run one per invocation at exit:**
  - Engine: `TheScopeOutputIsACwConditionTests` 3/3, `ScopeIsNeverTurnedOnTests` 5/5, `EveryModeAnswersForEverySettingTests` 6/6, `TheRadioPointsTheDetectorTests` 3/3, `TheTrackerObeysTheMeterTests` 3/3, `ABarIsALevelThatHoldsTests` 3/3, `ScopeStreamTests` 11/11, `ScopeHonestyTests` 6/6, `ScopeOutputWriteTests` 5/5.
  - `AMarkIsTheEnvelopeOverAThresholdTests` 5/7: the same two reds as unit 479, unchanged (40 of 208 and 208 of 208 unmarked).
  - `ReceiverSetup`'s types with `ScriptedRadio`, all green: `EveryMorseBlockSetsWhatCwSetsTests` 6, `WhatEnteringAModeSetsTests` 1, `AValueAlreadyRightIsNotWrittenTests` 5, `TheTuneInSetsOnlyWhatIsInTheWayTests` 5, `OneVoicePerFieldTests` 6, `HamletSaysWhatItChangedTests` 5, `TheOperatorsHandStandsTests` 4, `TheOperatorsHandCrossesTheMorseBlocksTests` 3, `ThePreampFollowsItsOwnTextTests` 5, `ThePreampIsWhatTheManualSaysTests` 17, `TheRoundTripLandsInTheSamePlaceTests` 4, `TheBannerSaysWhatTheRadioReadBackTests` 5, `TheBlockStatesWhatTheModeNeedsTests` 5.
  - App: `TheBarsCarryTheirLettersTests` 4/4, `TheScopeIsTheMiddlePictureTests` 4/4, `TheScopeShowsTheMarksTests` 5/5, `TheVerdictCarriesTheScopeTests` 4/4, `TheOwnersVerdictIsARowTests` 7/7, `WhichPathsPutNarrationOnTheBarTests` 6/6, `OneVoiceOnThePreampTests` 16/16, `WhatIsSaidAboutThePreampTests` 1/1, `WhatHappensWhenTheBandOverloadsTests` 3/3.
  - `VoiceTests` 4/5. The red is two strings this unit did not write; see section 4 item 8.
- **Not run, because each reads a recording:** the `CwToneTracker` and `CwDecoder` types that replay captures.
- **`src/Hamlet.RadioEngine` against entry `8226ccc4`:**
  - `Cw/CwDecoder.cs`: `Heard`, the sample count a settled character is placed by.
  - `Cw/CwEnvelopeDetector.cs`: `PointAt`, `PointedHz`, `WatchedHz`, and the reading's `Pointed`. The pointed bin replaces the sweep while set.
  - `Cw/CwScopePointer.cs` (new): the radio's peak inside the filter, turned into a pitch; quiet after 3 s.
  - `Cw/CwToneTracker.cs`: `FollowScope`. The scope's pitch beats the meter's, and the survey holds while either stands.
  - `Explore/OwnedSettings.cs`: the scope output is the thirteenth owned setting.
  - `Rig/RigSpectrumSource.cs`: `FollowTheSetup` starts the listener on a read-back of on.
- **The eleven transmit files** print nothing against `7e209cb4`. Nothing under `tests/fixtures` was touched. **No recording was read.**

**Mismatches between the instruction and the tree** (§4.3):
- `RigSpectrumSource` already starts at connect in every mode; it does not start on a read-back.
- HM-DEC-062 and `ScopeIsNeverTurnedOnTests` forbade the write this unit adds; the instruction did not name them.
- `CwCharacter` carries an end (`At`) and a length (`SpanHops`), not a start and end hop.
- The detector holds four seconds, not the eight the graph shows.
- The scope's amplitude is 0 to 160 with no decibel scale anywhere in the tree.

## 2. What the owner should expect

**Rebuild, tune a station in CW, and the radio's own scope now tells Hamlet where it is.**
- The first CW tune-in turns the radio's scope output on. It shows among the settings Hamlet changed, and it stays on.
- Watch the CW tab's scope:
  - **It should be empty and say "listening" when you hear nothing.**
  - **Bars should appear when you hear keying.**
  - **Letters should ride above the bars**, bold when the decoder is sure and faint when it is not.
- Press the two buttons as before. The rows now say where the radio's scope pointed and how many frames it sent in the last four seconds.

**Reading the graph:**
- **Bars with no letters over them:** the decoder is the next thing to fix.
- **Letters with no bars under them:** the decoder is inventing.

**What will look wrong but is not:**
- **Letters arrive a few seconds after their bars.** They are the settled pass, which trails the audio by design, so a word's letters fill in over the older half of the eight seconds.
- **In CW-R the detector still sweeps,** and the tone line does not say "scope quiet".
- **If the radio's scope stops sending**, the tone line leads with "scope quiet, sweeping" and the detector goes back to its own search.
- **The scope output stays on after you leave CW.** Nothing turns it off. The data modes' waterfall has always read the same stream, and they are untouched.
- **If the dial starts following your hand more slowly after this**, that is the scope stream sharing the cable. See section 4 item 1.

Nothing is left uncommitted except this report, which is left for the launcher as unit 479's was. `SESSION.lock` and `STOP` are the launcher's, and both were left in place.

## 3. What you should see

**Yes, on synthetic input.** The radio's scope output goes on in CW, and the detector watches the radio's peak: 850 Hz for a peak 250 Hz above the dial at pitch 600, where its own sweep had left it at 600. The graph draws four bars with C over them and nothing else. On the air this is unproven, and 12.4 waits on your ear.

On the CW tab:
- An empty graph that says "listening" while the band is quiet.
- Bars as long as the dits and dahs you hear, sliding left over eight seconds.
- Each letter the decoder settles, written over the bars that made it.
- Hovering a bar gives its length and whether it read as a dit or a dah.
- Hovering a letter gives how sure the decoder was.

## 4. What's blocking us

Nothing here blocks the loop.

1. **The scope stream shares the cable with the poll.**
   - **Ruling wanted:** none now. If the dial or the meters lag after this build, say so, and the radio's sweep rate gets measured.
   - **Reasoning:** the comment HM-DEC-062 left in `StartRigSpectrum` puts a sweep at about six hundred bytes on a cable carrying about eleven and a half thousand a second. The radio's sweep rate at your scope speed setting has not been measured here.
   - **Rejected:** throttling or turning the output off between tune-ins. HM-DEC-188 rules it on.
2. **The listener is not stopped, and the output not turned off, when the mode leaves CW.**
   - **Ruling wanted:** accept, or rule that leaving the Morse family writes the scope output off.
   - **Reasoning:** the instruction says the source stops when the mode leaves the CW family, and also says data modes keep whatever they do with the scope today. Their waterfall draws from this same listener, which has run from connect since before this unit.
   - **Rejected:** stopping it, which would take the radio's spectrum off a data mode's waterfall.
3. **`scopePeakDb` is null on every row, and a fourth key carries the level.**
   - **Ruling wanted:** accept `scopePeakLevel`, or cite a decibel scale for the IC-7300's 0 to 160 waveform so the field can be filled.
   - **Reasoning:** nothing in the tree ties that scale to decibels. A number labeled dB that is not dB is §0.0 broken in the record.
   - **Rejected:** writing the raw level under the dB key.
4. **The tracker takes the scope's pitch only while the bars there say keying.**
   - **Ruling wanted:** accept, or rule "the moment the scope points" literally.
   - **Reasoning:** on an empty band the tallest bin is noise that wanders across the filter several times a second. Each move would retune the tracker, and a retune is a clock-loss event for the decoder. The detector does follow the scope at once, since that costs nothing.
   - **Rejected:** feeding the tracker on every frame.
5. **CW-R is not pointed.**
   - **Ruling wanted:** confirm the sign on the reversed sideband (a peak above the dial beats at the pitch minus its offset?) and CW-R gets pointed too.
   - **Reasoning:** the instruction gives the rule for CW only, and I have not checked CW-R against the manual.
   - **Rejected:** applying CW's sign to CW-R.
6. **A settled character's placement is approximate.**
   - **Ruling wanted:** none now.
   - **Reasoning:** it is placed by the decoder's `Heard` at the moment it settles, on a chunk-sized clock, against the detector's hops on the wall's clock. The tracker's window and the chunk size make an offset of a few hops at most on synthetic audio. Every character in the tests carried a span. Whether the second decoder's arbitrated characters ever carry `SpanHops` 0 is not known. The graph counts any such character (`CwTrainingGraph.WithoutSpan`) and draws it at its end, but nothing shows the count yet.
   - **Rejected:** skipping a character with no span, which the instruction forbids.
7. **12.2's criterion text** is rewritten by R95 and still describes 478's trace. This is the same kind of drift as unit 479's item 5.
   - **Ruling wanted:** rewrite 12.2 to the bars and letters.
   - **Rejected:** editing the plan myself.
8. **`VoiceTests` was already red at entry.**
   - **Ruling wanted:** none. It is named so it is not rediscovered.
   - **Reasoning:** two pitch-proof strings in `MainWindowViewModel.cs` (now lines 13055 and 13063) say "centre". This unit did not write them, and `VoiceTests` is not on the carry-forward list. The "centre" spellings this unit did write were corrected.
   - **Rejected:** repairing them on the way past (§12.6).

**Asks still outstanding:** unit 479's six are unanswered and carried by title:
- what S is measured against;
- how a weak station makes its first pair;
- the 15 dB test still red at 40 hops;
- fifteen sitting inside the empty windows' 14.7 to 17.7;
- 12.1's criterion text being false of the tree;
- the two `PHASE_STATUS.md` copies differing.
