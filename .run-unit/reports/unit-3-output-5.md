READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0, 1, 2 done; 3 partial,
   3.4 only open and not flippable this unit; 4 not started; 5 the
   owner's; 6 partial.
B. Step 6: 6.2 met, and 6.6 held. The lead sentence and each of the three clauses, one line each, with the red quoted, are at the top of section 1.
C. The rest. Section 4 raises 2 items; none in the way of 6.2.

```
UNIT:       418 - complete at task 3 of 3, none dropped - 2026-09-24 10:53
PHASE GOAL: Hamlet decodes a real CQ call into the text that was sent, measured against keys, and the screen says nothing about the signal that is not so
UNIT GOAL:  Every line on a CW capture sheet is checked against the code that writes it, and each one that is false about the signal is made true, watched failing first on a saved capture, the keying caption's 400 to 1200 Hz first
ADVANCED:   yes - 6.2 ticked, with every false sentence red on a saved capture before its fix and its three named clauses still green
NUMBER:     sidecar sentences false 12 -> 2 of 30 checked; about a signal 10 -> 0
DRIFT:      0
```

## 1. What Claude did

**Complete, task 3 of 3, none dropped.** QUIVERFULL, Hamlet confirmed by the gate, `main`,
entry HEAD 3211874b, exit 8c7d59ca plus this report's commit.

**6.2, one line each:**
- **Lead sentence - met.** 30 lines checked against the tree. The 10 that were false about a signal are now true, each red first on a saved capture. The 2 still false, `captured` and `broadcast`, are about the clock and the radio link, not a signal, and are parked as P12.
- **`tonePeak` clause - holds.** On 17:37 it reads `26.8 (a figure about this recording ...)` and on 014113 `not measured`. TheTonePeakIsAboutThisRecordingTests is 3 of 3 after the fixes.
- **`elementHz` clause - holds.** On 17:37 it reads `each element's own pitch not measured (the elements counted above were measured by their timing ...)`. TheSidecarDoesNotContradictItselfTests is 3 of 3.
- **`keying` clause - holds.** It reads `no keying at 575 Hz: 109 rises above the threshold ...`, rises and not key-downs, as unit 411 left it.
- **The red for the caption P10 named** (765d946b, 17:37): `Assert.Contains() Failure: Sub-string not found ... Not found: "sweep of 300 to 900 Hz in 25 Hz steps"`. It turned green at 70e3bc16.

**Section 5 against the tree - no mismatch.**
- `KeyingRecordLine` held the 400 to 1200 caption as a literal (`MainWindowViewModel.cs` 12435) and was the only place the line was composed.
- `KeyingEnvelope.LowestToneHz` and `HighestToneHz` are `CwToneTracker.MinimumToneHz` and `MaximumToneHz`, 300 and 900, and `ToneStepHz` is 25 (`KeyingEnvelope.cs` 130-138).
- `CwKeyingMeter` sweeps only through `KeyingEnvelope.Best` (`CwKeyingMeter.cs` 209).
- Both sidecar tests were green at HEAD.
- Nothing parses the `keying` line; the only reader is TheSidecarIsReReadTests' `StartsWith`.
- Keyed totals at HEAD: 167 over 565, the ten 35 over 156, 17:37 19 over 25.

**Task 0.**
- Version 1.13.105. PHASE_STATUS names 418 and step 6. PHASE_OUTCOME has its entry. P10's answer is appended in PARKED.md.
- Entry round:
  - Engine: 178 of 178.
  - App: 275 of 278. Two were lost to Avalonia's dispatcher loop, and `TheFavoritesAreUnderTheGreenZoneTests` went 3 of 3 alone. `TheCardOffersLogAndAnXTests.LogIsNeverOnAReceipt` failed once with two CQ cards where it expects one; its type went 5 of 5 alone.
  - Floors: captures 51 of 51, adjudicated 13 of 13, clean 2 of 2.
  - Sidecar tests: 3 of 3, 2 of 2 and 3 of 3.

**Task 1, the trace.**
- `EverySentenceOnTheSheetTests` builds a view model, puts a decoder that heard only one file behind it, and calls the sheet writer `CaptureNotes` by reflection, exactly as the press does. It does this for 17:37, 014113 and 013347, then prints each line beside the constants and measurements behind it.
- No radio is attached, so the rig lines take their unread branches. For those lines, and for branches a replay cannot reach, the verdict comes from the code and from saved sheets: `cw-2026-08-28-004844` was written 39 s after a transcript clear, and `-005051` holds `unkeyed YES`.
- The table is in section 3.

**Task 2.** Every false sentence was committed red on its own, then fixed. The fix is always the sentence; no measurement changed.
- **The keying caption.**
  - TheKeyingCaptionNamesTheSweepItRanTests, 2 facts, red at 765d946b and green at 70e3bc16.
  - The range, step and window now come from `KeyingEnvelope` and `CwKeyingThresholds` (P10's answer).
  - "The last six seconds" is now "the 6 seconds the meter last read before the press".
  - "Sharing nothing with the decoder" is now "taking the same audio as the decoder and the tracker's own range, and none of the decoder's code or its choice of pitch".
- **The other nine.** TheRestOfTheSheetIsTrueTests has 10 facts, red 10 of 10 at 212b01d2 and green at 0e9a2e15. The reds:
  - inputFloor: `String: "inputFloor -32.1 dBFS"; Not found: "the level meter's running floor at the mo"`
  - clipping: `String: "clipping   False"; Not found: "the level meter last measured when it was"`
  - toneHz: `String: "toneHz     600.0 Hz  (measured from the k"; Not found: "the centre of the survey bin it was admit"`
  - unkeyed: `Sub-string found: "characters reached the screen from a pitc"` in `"unkeyed    YES  (252 characters reached th"`
  - elements, characters and sinceLast after the clear: `Not found: "since the decoder started listening"`
  - competing: `Sub-string found: "keyed "` in `"...ver the band floor, keyed 29% of the time"`
  - reading: `Not found: "against a gate of 1.40"`, and `Sub-string found: "at the moment of the press"`
- **The regenerated sheets.** They are written whole to `.run-unit/unit418-sidecar-cw-2026-09-23-173723.txt`, `-08-22-014113.txt` and `-08-17-013347.txt`, with old beside new in `.run-unit/unit418-oldnew.txt`. No capture in the tree was edited.
- **6.2 ticked** in PHASE_PLAN.md.

**Task 3, the exit round (6.6 holds).**
- Build: Hamlet.sln, non-incremental, warnings as errors, 0 warnings.
- Engine: 178 of 178.
- App: 275 of 278. Three were lost to the dispatcher loop, `BindingHealthTests` and two in `ThePowerIsOfferedTests`, and they went 1 of 1 and 3 of 3 alone.
- Floors: captures 51 of 51, adjudicated 13 of 13, clean 2 of 2.
- Sidecar and roster tests: TheSidecarDoesNotContradictItselfTests 3/3, TheSidecarIsReReadTests 2/2, TheTonePeakIsAboutThisRecordingTests 3/3, CaseRosterSurvivesAnEveningTests 6/6.
- New tests: TheKeyingCaptionNamesTheSweepItRanTests 2/2, TheRestOfTheSheetIsTrueTests 10/10, EverySentenceOnTheSheetTests 1/1.
- Touched types: ASheetSaysWhichInstrumentSpokeTests 3/3, CwCaseCountsSayWhatTheyCountTests 7/7, TheSwingIsTheFigureThatHoldsTests 4/4, AReplayedCaptureArrivesWholeTests 1/1.
- Keyed totals: all keyed 167 over 565, the ten 35 over 156, 17:37 19 over 25, identical to entry.
- `git diff 3211874b -- src/Hamlet.RadioEngine/Cw` prints nothing, and the eleven transmit files print nothing against 7e209cb4.
- The whole src diff is two files: `MainWindowViewModel.cs` and `AudioTap.cs`.

**Decisions I made myself, in full:**
1. **The audit went past the keying caption, because the instruction's lead sentence covers every line.** Ten sentences about a signal were false, not one, and all ten were fixed in this unit rather than held back. Nothing was dropped.
2. **Captions rather than new measurements.**
   - `clipping` and `inputFloor` now say they are the level meter's figures at the press. I did not recompute them over the recording.
   - The `reading` line now says it was read when the sheet was written. I did not snapshot it at the press.
   - Each of these is the smallest change that makes the sentence true under §6. The owner may prefer the other way; see section 4.
3. **`competing` no longer says "the loudest thing in the band".** Where a station is admitted, the field is the loudest thing that is *not* keying (`CwToneTracker.cs` 1103, 1122, 1128). With nothing admitted it can fall back to the loudest thing overall (994, 1048, 1085). The report cannot tell those apart, so the line now says "the survey names a tone at ...".
   - This superlative was corrected from the code alone. No saved sheet among those checked shows it false; the red for this fact is the "keyed 29%" wording on 014113.
4. **Two reds are about wording rather than a visible wrong number.**
   - No saved capture clips (the highest peak is -1.6 dBFS on 013347), so the clipping red on 013347 shows the line not naming its window. That window peaks at -9.9 dBFS while the recording peaks at -1.6.
   - The `reading` line's timing cannot be reproduced in a replay, so its red is on the words "at the moment of the press". The 871 ms wait behind it was measured on 17:37.
5. **Two tests borrow state from a saved sheet.**
   - The 004844 facts set the transcript clear 39 s back, the gap that sheet records.
   - The 005051 fact gives the report that sheet's own count of 252 characters. The replay itself leaves the pitch unmeasured, as the evening did.
6. **`AudioTap.LevelSeconds` changed from private to public, with its value unchanged,** so the clipping caption reads 0.2 from the code rather than from a second literal. That is the same lesson as P10. `AudioTap` is in `Audio`, not `Cw`.
7. **The `duty` line was judged true.** It says "the key was down", which is `KeyingProfile.Duty` as the engine defines it. The trace prints how much of that is element-length: 45.9 of 48.0 points on 17:37 and 23.1 of 26.6 on 013347.
8. **TheSidecarIsReReadTests was not edited.** Its asserts are `StartsWith` and no wording forced a change.
9. **Unit 417's evidence files are modified in the working tree and not committed.** TheSidecarIsReReadTests writes to fixed names, so the exit round overwrote `.run-unit/unit417-sidecar-*.txt` with today's lines. `git restore` needs approval I did not have. None of my commits stage those files, so unit 417's committed evidence is unchanged; the working copies can be discarded.

**Push:** every commit pushed, rc 0.

## 2. What the owner should expect

Every line on a CW capture's sheet has now been checked against the code that writes it: 30 lines, 12 of them false. The ten that were false about the signal now say what the code does:
- The keying line names the 300 to 900 Hz sweep the meter really runs. It used to say 400 to 1200, while an evening sheet printed a keying pitch of 375 under it.
- `toneHz` says it is a survey bin centre, not an interpolation.
- The `reading` line prints the gate as 1.40 rather than 1, and says it was read when the sheet was written, not at the press.
- The element and character counts no longer claim to start at a transcript clear, which never reset them.
- `unkeyed` stops crediting an evening's characters to the current pitch.
- `competing` stops calling a tone keyed after saying nothing judged it a station.
- `inputFloor` and `clipping` say they are the level meter's figures at the moment of the press, not the recording's.

**What will look wrong but is not:** the sheet's lines are longer. `clipping False` now carries "over the 0.2 seconds the level meter last measured", which is honest and narrow; a clip earlier in the recording would not show there. Section 4 asks whether it should be measured over the whole file instead. Nothing the radio does, and no decode, changed.

## 3. What you should see

The 17:37 keying line, old then new:

```
keying     no keying at 575 Hz: 109 rises above the threshold, median 71 ms, 16 dB swing; not called keying on a 16 dB swing where it needs 20  (an independent sweep of 400 to 1200 Hz in 25 Hz steps over the last six seconds, sharing nothing with the decoder)
keying     no keying at 575 Hz: 109 rises above the threshold, median 71 ms, 16 dB swing; not called keying on a 16 dB swing where it needs 20  (an independent sweep of 300 to 900 Hz in 25 Hz steps over the 6 seconds the meter last read before the press, taking the same audio as the decoder and the tracker's own range, and none of the decoder's code or its choice of pitch)
```

Task 1's sentence table. Line numbers are `MainWindowViewModel.cs` at entry unless another file is named. The full wording of every row, before and after, is in `.run-unit/unit418-table.md`.

| # | line | what the tree says | before | after |
|---|---|---|---|---|
| 1 | captured | `UtcNow` at composition, after the tonePeak wait (11677, 871 ms on 17:37); the file stamp is taken earlier at 11669 | false by up to 1 s, not about a signal | parked P12 |
| 2 | audioSeen | `tap.SamplesSeen` at the press (11643) | true | true |
| 3 | fingerprint | SHA-256 of the samples (13005) | true | true |
| 4 | seconds | duration written (11737) | true | true |
| 5 | sampleRate | 11738 | true | true |
| 6 | frequency | radio value with its age, or Hamlet's own, labeled (12018-12043) | true, about the radio | true |
| 7 | band | derived from the same frequency (12059-12074) | true, about the radio | true |
| 8 | broadcast | window `UtcNow - Duration` at composition (12981) | false by up to 1 s, about the link | parked P12 |
| 9 | inputPeak | largest sample in the file (AudioTap.cs 765) | true | true |
| 10 | meterPeak | level meter's last 0.2 s in the report taken at the press (11577) | true | true |
| 11 | inputFloor | the meter's running floor, carried across everything heard (AudioTap.cs 86-90, 287-292), printed bare; 013347 prints -32.1 where its quietest fifth is -75.4 | **false** | true |
| 12 | clipping | the meter's last 0.2 s only (AudioTap.cs 84, 253-302), printed bare | **false** | true |
| 13 | toneHz | the admitted pitch is `_binHz[bin]` (CwToneSurvey.cs 792), reported unchanged (CwToneTracker.cs 1120, 1127, 1184); 600.000 and 625.000, on the 5 Hz grid; caption said interpolated | **false** | true |
| 14 | tonePeak | unit 417 (12394-12415) | true or not measured | same |
| 15 | heldPeak | 12425-12430 | true | true |
| 16 | inThis | counters over exactly these samples (12876-12891) | true or not derived | same |
| 17 | unkeyed | cumulative `CharactersEmitted` credited to the current pitch (12605-12630); 005051 says 252, inThis 29 | **false** | true |
| 18 | elements | transcript's cover after a clear, but `ClearTerminal` (10726) resets only the transcript; 004844 | **false** after a clear | true |
| 19 | characters | same as 18 | **false** after a clear | true |
| 20 | decoderWpm | decoder's speed or its guard's reason (12641-12672) | true | true |
| 21 | text | transcript (11867) | true | true |
| 22 | textCovers | the transcript's own start (12901-12922) | true | true |
| 23 | spanLlr | 12293-12327 | true or nothing read | same |
| 24 | competing | `PresentFraction` is time above the band (CwToneSurvey.cs 41-43), printed as keyed; "loudest" is the loudest unkeyed where a station is admitted | **false** | true |
| 25 | reading | gate 1.40 (CwProbabilisticDecoder.cs 235) printed `{4:0}` as 1; read at composition, 871 ms after the press, while the reading moves every 0.5 s (CwProbabilisticStream.cs 34) | **false** | true |
| 26 | keying | literal 400 to 1200; KeyingEnvelope.cs 130-138 sweeps 300 to 900 in 25; CwKeyingMeter.cs 197-201 reads the decoder's own tap; the window ends up to about 2 s before the press | **false** | true |
| 27 | duty | share of the file above the envelope's threshold at the measured pitch (12523-12535); 45.9 of 48.0 points element-length on 17:37 | true or not measured | same |
| 28 | elementHz | unit 411 (12444-12508) | not measured, said so | same |
| 29 | sinceLast | first-capture branch uses row 18's cover; 004844 | **false** after a clear | true |
| 30 | rig block | each field with provenance (11981-11988) | true, about the radio | true |

Before: 12 of 30 false, 10 about a signal. After: 2 of 30, rows 1 and 8, neither about a signal, parked P12.

No visible change on screen. The change is in the text file written beside each kept recording.

## 4. What's blocking us

Nothing blocks the phase. Two asks are parked and the loop goes on.

1. **P12, the two clock lines** (`captured`, `broadcast`). Since unit 417 they read the clock about a second after the press.
   - Ruling proposed: take one timestamp at the press and hand it to both.
   - Reasoning: it is the same fault that made the `reading` line false, and neither line is about a signal, so 6.2 does not need it.
   - Rejected: fixing it here, because the instruction's drop rule parks sentences outside 6.2's scope.
2. **Whether `clipping`, and possibly `inputFloor`, should be measured over the whole recording** rather than captioned as the level meter's figures at the press.
   - Ruling proposed: measure clipping over the file, as HM-DEC-094 already did for `inputPeak`. A clip anywhere in 30 seconds is what a reader of the sheet wants, and today's caption is true but covers only 0.2 s.
   - Rejected here: this unit's rule was that the sentence moves and the measurement does not, and a new figure on the sheet is a promise the owner should make.
