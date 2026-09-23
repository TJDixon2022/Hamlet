```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Step 0 closed on unit 391's answers at
   task 1 of this unit; step 1 is this unit's; steps 2 to 5 are not started.
   After this unit step 1 is partial: 1.5 is not met.
B. Step 1's criteria, one line each, met or not:
   1.1 met - the folder is 7e209cb4's plus six listed hunks and the kept
       CwPitchChoice.cs; the eleven transmit files print nothing against 7e209cb4.
   1.2 met - Hamlet.sln 0 warnings 0 errors, both Hamlet test projects built,
       22 test files excluded and listed, tools/Hamlet.PitchRank out of the build.
   1.3 met - captures 37 of 37 and adjudicated 13 of 13 green; synthetics 2 of 2
       red, reported with what they read.
   1.4 met - the four app CW tests, 13 of 13 green.
   1.5 NOT MET - engine line 150 of 150; app line red on one test green at entry,
       TheOliviaRowsTests, twice, by an IOException on its own telemetry file.
   1.6 met - three changes in MainWindowViewModel.cs, each with its seam.
C. The report last. Section 4 raises 19 items - unit 390's nine and unit 391's
   five carried, and this unit's own five - and item 1, the Olivia rows red, is
   in the way of 1.5 in B.
```

```
UNIT:       392 - complete at task 4 of 6, tasks 0 to 4, task 5 dropped - @@NOW@@
PHASE GOAL: get the CW decoder reading again by going back to the last code that read, then keep it from breaking silently, clear the inherited reds and judge the August rework on numbers, with Tim at the radio as the final judge
UNIT GOAL:  put the engine's Cw folder back to 7e209cb4, change only what today's app and tests need to build, list every change, and show the captures and adjudicated floor tests green at HEAD
ADVANCED:   yes - captures and adjudicated floor cases went from 18 red to 0 red at HEAD on the restored decoder; step 1 is partial on 1.5
NUMBER:     captures and adjudicated cases red at HEAD: 18 of 50 -> 0 of 50; synthetics 2 of 2 red -> 2 of 2 red
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 6: tasks 0 to 4 done, task 5 dropped.** Task 5 was the named drop
candidate. Task 4 ended at 20:48 and the unit started at 20:06, so less than forty minutes of
the hour were left. Step 1 is **partial**: 1.5 is not met. Windows 11, `C:\Source\HamLet`,
Hamlet confirmed by the gate, branch `main`, pushed after every task.

**Task 0.** Version 1.13.78 to 1.13.79. `PHASE_STATUS.md` named unit 391 at step 0, so I set
it to *392 - the decoder reads again* at step 1. Entry round, both carry-forward lines as the
file prints them, one build each, with a status line before each:
- **App:** 276 of 278 in 181 s. Two tests were lost to the headless dispatcher loop before any
  assertion ran. I re-ran the line once (§6): 277 of 278 in 161 s, one lost the same way. Each
  lost test passed in the other run, so nothing was red on an assertion.
- **Engine:** 150 of 150 in 301 s.

`git diff --stat 3d6a2c12 HEAD -- src tests` printed nothing, so unit 391's floor run is this
unit's entry measurement (decision 2).

**Task 1, the trace.** `docs/phase-cw/unit392-seams.md` sections 1 to 6:
- Unit 391's 145 app seam rows re-checked at `7e209cb4`: 136 whole. Of the 9 rows with an
  absent type, 6 are the same word on another type (`Outcome`, `Envelope`). The 3 real ones
  are `CwElementPitch`, `CwStreamSplit` and `CwPitchChoice`, all in `MainWindowViewModel`.
- 13 engine test files name a HEAD-only type.
- None of the 33 files at `7e209cb4` names a HEAD-only type.
- `Tap` is fed at device rate at both commits, with no resampler in either.
- `DigitalMode` is 865e66d8's and absent at `7e209cb4`.
- 0.1 to 0.4 ticked per decision 1.

**Task 2, the restore. One commit, `9fbb4728`.** Transmit diff empty. Checked out
`7e209cb4 -- src/Hamlet.RadioEngine/Cw` and deleted six HEAD-only files, then built five
times:
- **Build 1:** 2 errors, crefs in a kept file.
- **Build 2:** 134 errors.
- **Build 3:** 8 errors.
- **Build 4:** clean. A non-incremental rebuild was also clean: 9 projects, 0 warnings, 0 errors.

The result:
- **Six engine-side hunks** in `CwDecoder`, `CwDecodeReport` and `CwCharacter`.
- **Three app changes** in `MainWindowViewModel`.
- **22 test files excluded** from compilation, not retired.
- **`tools/Hamlet.PitchRank`** out of the solution build.
- **Kept:** `CwPitchChoice.cs` only. **Deleted:** nine HEAD-only files.

**Task 3.** Each floor type ran in its own foreground invocation:
- **Captures:** 37 of 37 green in 97 s. Unit 391 had 19 of 37 in 1995 s.
- **Adjudicated:** 13 of 13 in 33 s.
- **Clean synthetics:** 2 of 2 red, reading placeholders.
- **App CW tests:** 13 of 13 green.

**Task 4.** Carry-forward at exit:
- **Engine:** 150 of 150 in 297 s.
- **App:** 274 of 278 in 159 s. Three were lost to the dispatcher loop. **One was red:
  `TheOliviaRowsTests.WithRowsPresentNothingIsComposedUntilAPressAndEachPressCarriesItsRowsVariant`,**
  an `IOException` because another process held its own `refuse\2026-09-23.jsonl`.
- **App re-run once:** 275 of 278 in 170 s. Two were lost to the dispatcher loop, and the same
  Olivia test failed with the same `IOException`.

**Regression, by name, under HM-DEC-165:** that Olivia test passed at entry and was red at exit.
The evidence that the restore did not cause it:
- It is not an assertion red.
- The test builds no CW decoder. It feeds `TapForTests` with no audio input.
- Run alone, it passed at `617215f0`, the tree before the restore, in a temporary worktree
  since removed. It also passed alone on the restored tree. Each run took 8 s.

The one change that touches its run is the app test assembly losing
`ThePitchControlsAreOffThePanelTests`, which changes what runs beside it. That is a guess and
is marked as one. **1.5 is not ticked.** No other test was red that had been green at entry.

**Author's decisions applied, all overrulable:**
1. **Decision 1:** 0.1 to 0.4 ticked, each with one bold sentence naming the unit and the file.
2. **Decision 2:** unit 391's floor run is the entry measurement, because the diff was empty.
3. **Decision 3:** 22 files excluded by `<Compile Remove>`, with a comment naming this unit and
   step 3 in each test project. Listed with their missing names in section 3.
4. **Decision 4, and the keep list changed during the build.** Task 1 wrote a keep list of four:
   `CwElementPitch`, `CwStreamSplit`, `CwJointCutter` and `CwPitchChoice`. `ElementPitchLine`
   feeds `CwElementPitch` from `read.Elements`, which only HEAD's decoder produces. Keeping the
   file could not make the line build unless a HEAD-only type went back through `7e209cb4`'s
   decode function, which section 9 of the instruction parks. So I changed the line (app
   change 2), and three of the four had no user left outside Cw. **Kept: `CwPitchChoice.cs`.**
5. **Decision 5:** timeouts 900, 600, 300 and 480 s, as given. Every run finished inside the
   harness's 600 s, foreground. Nothing was backgrounded this unit.
6. **Mine, not in the instruction: `tools/Hamlet.PitchRank`'s six `Build.0` lines taken out of
   `Hamlet.sln`.** It was added 2026-08-28 to measure the rework, and it names seven HEAD-only
   types and overloads. It stays in the solution and in the tree, unedited. I treated it the way
   decision 3 treats a test.
7. **Mine: `DecodeQueueDroppedChunks` and `DecodeQueueDroppedSamples` return nought**, not NaN
   and not an app change. This decoder has no queue, HEAD returned nought whenever its queue
   was not running, and the app already writes nought when there is no decoder.
8. **Mine: `UseJointCutter` and the ranked sentence became app changes rather than shims.** A
   switch that silently did nothing, or `CwPitchRank` recreated only to return null, would each
   have been the worse lie.

**One error of mine, corrected in the record.** Task 0's PHASE_OUTCOME line said adjudicated
was 13 of 13 red at entry. Unit 391 measured 13 of 13 **green**. It was corrected at task 3,
with the correction stated on the line.

## 2. What the owner should expect

The CW decoder in the build is now the one from the evening of 2026-08-25 (`7e209cb4`), and the
CW tab is as it was in September; nothing on the screen moved. Three things on the capture sheet
changed:
- The element-pitch line now says *not measured (the decoder in this build does not say where
  each element began and ended ...)*.
- The *ranked* pitch sentence can no longer appear.
- The margin figures on the span-ratio line print *unmeasured*.

The August rework is out of the build: the joint cutter, the ranking, the spectral peak, the
posterior, the decode queue and the operator's pitch assertion. `UseJointDecoder` in the
settings file is read by nothing. `pitch-rank` is no longer built with the solution. What
looks wrong but is not: the two clean synthetics still fail, now showing `■` marks rather than
nothing. R53 makes them step 3's. The floor tables say the captures produce at least what they
produced on 08-25. That is a count, not a reading, and whether it reads on the air is yours to
say at step 5.

## 3. What you should see

**The floor table: 50 of 50 captures and adjudicated cases green at HEAD, against 32 of 50 in
unit 391's run; the two synthetics red at both.** Nothing a user sees on the screen changes;
the capture sheet changes as section 2 says.

**Captures, every case** (measured, floor, difference; the last column is unit 391's characters
and elements at HEAD and its result there). Wall 97 s, against unit 391's 1995 s.

| Capture | Result | Chars | Floor | Diff | Elements | Floor | Diff | Unsure (then) | Tone Hz | HEAD, unit 391 |
|---|---|---|---|---|---|---|---|---|---|---|
@@CAPTURES@@

**Adjudicated,** 13 of 13 green in 33 s, where unit 391 had 13 of 13 in 364 s. All seven
required anchors were found: `VA3VRR` 6 of 6, `MP/4 QNIK` 9 of 12, `DE KD0UN KD0UN K` 16 of 16,
`N HANDLING THIS MESSAG` 22 of 57, `, AND` 5 of 35, `110, AND 110 WITH A MEAN OF 117` 31 of 36,
and `R OTHER WEBSITES MENTI` 22 of 51. The five retired anchors all appear in what was read.
The shortfall fact reads 153 of 384 adjudicated characters, 40 %. Per case:
`docs/phase-cw/unit392-floors.md`.

**Clean synthetics,** red on the assertion in 7 s. `clean-12wpm` reads `■ ■ ■ ■ ■  ■ ■ ■ ■■`
and `clean-18wpm` reads `■ ■ ■  ■■■`, both expected to read `CQ DE W1AW K`. HEAD read `""`.

**The 1.1 adaptation table, every hunk of `git diff 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw`:**
4 files, 165 insertions, 1 deletion. `git diff --stat 7e209cb4 HEAD -- <the eleven transmit
files>` printed nothing at task 2 and at task 4. This is the proof of 1.1 before any seam was
touched:

```
git diff --stat 7e209cb4 -- src/Hamlet.RadioEngine/Cw
 src/Hamlet.RadioEngine/Cw/CwElementPitch.cs | 266 ++++++++++++++++++
 src/Hamlet.RadioEngine/Cw/CwJointCutter.cs  | 344 +++++++++++++++++++++++
 src/Hamlet.RadioEngine/Cw/CwPitchChoice.cs  |  72 +++++
 src/Hamlet.RadioEngine/Cw/CwStreamSplit.cs  | 410 ++++++++++++++++++++++++++++
 4 files changed, 1092 insertions(+)
```

Three of those four were deleted later in task 2 (section 1, decision 4).

| # | File | Hunk | What | Why |
|---|---|---|---|---|
| 1 | `CwPitchChoice.cs` | `@@ -0,0 +1,72` | the file, HEAD's copy | kept: `CwDecodeReport.PitchChoice` returns it and the app reads it |
| 2 | `CwCharacter.cs` | `@@ -144,0 +145,36` | `WidestRecordedLlr` 1,000,000; `MarginLlr { get; init; } = NaN`; `MarginShareForRecord`, HEAD's arithmetic | the app's sheet reads all three and a sidecar test sets `MarginLlr`; NaN because this decoder never compares two paths, and the sheet prints *unmeasured* |
| 3 | `CwDecodeReport.cs` | `@@ -62,0 +63,23` | `PitchWasAsserted => false`; `PitchChoice => PitchWasMeasured ? Keying : NotChosen` | nothing takes an assertion; at `7e209cb4` an unmeasured pitch is the middle of the bank |
| 4 | `CwDecoder.cs` | `@@ -350,0 +351,12` | `DigitalMode { get; set; }` | set by `MainWindowViewModel.cs:14155`; 865e66d8's gate |
| 5 | `CwDecoder.cs` | `@@ -353,0 +366,21` | queue counters return nought; `Retuned() => Unlock()` | there is no queue; the lock is the part of HEAD's `Retuned` this decoder has |
| 6 | `CwDecoder.cs` | `@@ -468 +501` | `DecodingSuspended` becomes `DecodingSuspended \|\| DigitalMode` | the gate: tap fed, chunk counted, stream skipped, nothing decoded |

**The exclusion table: 22 files, excluded not retired.** Only one has a name in
`docs/unit239-failing-set.txt`: `ABlipDoesNotShiftEverythingAfterItTests`. None is a
carry-forward or 1.4 test.

| Project | File | Missing name it quotes | In unit239 |
|---|---|---|---|
| engine | `Audio/TheReadPathDoesNotAllocateTests.cs` | `CwKeyingMeter.WindowSizings` | no |
| engine | `Audio/TheTapIsNotBehindTheDecoderTests.cs` | `CwDecoder.ProcessDelayForTests` | no |
| engine | `Cw/ABlipDoesNotShiftEverythingAfterItTests.cs` | `CwReferenceDecoder` | yes |
| engine | `Cw/AMoveStartsTheDecoderFreshTests.cs` | `CwDecoder.PitchWasAsserted`, `CwDecoder.Ranked` | no |
| engine | `Cw/AStationIsABinThatSwingsTests.cs` | `CwSwingSurvey` | no |
| engine | `Cw/EveryElementCarriesItsOwnPitchTests.cs` | `CwProbabilisticResult.Elements`, `CwElementPitch` | no |
| engine | `Cw/FittingKeyUpAgainstAssumingItTests.cs` | `CwProbabilisticDecoder.FittedLogLikelihoods` | no |
| engine | `Cw/IsTheHertzABiasOrAFloorTests.cs` | `CwSpectralPeak` | no |
| engine | `Cw/NoSenderIsSplitInTwoTests.cs` | `CwStreamSplit`, `CwProbabilisticResult.Elements` | no |
| engine | `Cw/NothingActsOnTheAdmissionVerdictTests.cs` | `CwDecodeReport` parameter `PitchChoice` | no |
| engine | `Cw/TheCleanReadsStayCleanTests.cs` | `CwAccuracy` | no |
| engine | `Cw/TheFirstSecondsAreReadAgainTests.cs` | `CwProbabilisticStream.ReReads` | no |
| engine | `Cw/ThePeakAgainstASecondSignalTests.cs` | `CwSpectralPeak` | no |
| engine | `Cw/ThePeakFindsThePitchTheTrackerMissedTests.cs` | `CwSpectralPeak` | no |
| engine | `Cw/ThePosteriorSurvivesItsOwnArithmeticTests.cs` | `CwProbabilisticDecoder.Posterior`, `LogSum` | no |
| engine | `Cw/TheProbabilisticDecoderTests.cs` | `CwAccuracy` | no |
| engine | `Cw/TheQuietestBinNoLongerWinsTests.cs` | `CwPitchRanking`, `CwDecoder.RankThePitch` | no |
| engine | `Cw/TheReferenceDecoderIsPortedFaithfullyTests.cs` | `CwReferenceDecoder` | no |
| engine | `Cw/TheScoreSaysWhatItIsMeasuringTests.cs` | `CwAccuracy` | no |
| engine | `Cw/WhatDecodeScoringCostsTests.cs` | `CwToneTracker.CoarseSpacingHz` | no |
| engine | `Cw/WhereHamletAndTheReferenceDivergeTests.cs` | `CwProbabilisticResult.Elements`, 5-argument `Decode` | no |
| app | `Views/ThePitchControlsAreOffThePanelTests.cs` | `CwDecoder.AssertAt`, `CwDecoder.PitchWasAsserted` | no |

**The 1.6 app changes:** `git diff 10512248 HEAD -- src/Hamlet.App` is one file,
`MainWindowViewModel.cs`, 21 insertions and 43 deletions in six diff hunks.

| # | Where | What changed | Seam |
|---|---|---|---|
| 1 | line 11080, `@@ -11080,5` | `UseJointCutter = _settings.UseJointDecoder` dropped from the decoder's construction | no joint cutter; a shim would be a switch that does nothing |
| 2 | `ElementPitchLine`, `@@ -12405,8`, `-12414`, `-12416,3`, `-12421,11` | 7e209cb4's `Decode(envelope, toneHz)`; *nothing was read, so no element was measured, which is too few ...* or *not measured (the decoder in this build does not say where each element began and ended ...)* | `CwProbabilisticResult.Elements` does not exist at `7e209cb4`; an empty list would print a count of elements nobody measured |
| 3 | `ToneForTheRecord`, `@@ -12487,15` | the *ranked* branch removed | `CwPitchRank` is in a deleted file and this decoder never ranks |

**Carry-forward, entry beside exit:**

| Line | Entry | Exit |
|---|---|---|
| App | 276 of 278 in 181 s, 2 lost; re-run 277 of 278 in 161 s, 1 lost; no assertion red | 274 of 278 in 159 s, 3 lost and **`TheOliviaRowsTests...EachPressCarriesItsRowsVariant` red, IOException**; re-run 275 of 278 in 170 s, 2 lost and the same red |
| Engine | 150 of 150 in 301 s | 150 of 150 in 297 s |

`git worktree list` at exit: the root and the three preflight trees under
`C:/Users/TimDi/preflight-trees/`, untouched. The diagnostic tree `C:/Source/HamLet-wt392` was
removed.

## 4. What's blocking us

**Item 1 is in the way of 1.5. Nothing else blocks.**

**1. `TheOliviaRowsTests.WithRowsPresentNothingIsComposedUntilAPressAndEachPressCarriesItsRowsVariant`
passed at entry and was red twice at exit on the carry-forward app line.** *A regression by
HM-DEC-165's letter, and a ruling request.* The failure is not on an assertion. It is
`IOException: The process cannot access the file ...\refuse\2026-09-23.jsonl because it is
being used by another process`, at the test's own `Lines(folder)` (line 675,
`File.ReadAllLines`), reading a file that `JsonlTelemetry`'s background writer thread appends
to. The test builds no CW decoder. Run alone, it passed on the tree before the restore and on
the restored tree, 8 s each.

| | Ruling | For | Against |
|---|---|---|---|
| **A** | Adapt the test's wiring under R12 so `Lines` opens with `FileShare.ReadWrite` (or disposes the telemetry before reading), then re-run the app line | Reading a log another thread is appending to is the test's race, and the assertions stay untouched | It changes a carry-forward test in a unit about CW |
| **B** | Accept 1.5 as met, with this finding as evidence the restore did not cause it | No code moves | HM-DEC-165 is about exactly this kind of *probably not us* |
| **C** | Hold step 1 partial and make A the next unit's first task | Keeps the rule literal and the fix small | One more unit before step 2 |

**Industry standard:** A. A test that reads a file while a writer thread may hold it is flaky
whatever else changed. **My recommendation:** C, so the fix lands in its own commit with the
before and after runs beside it.

**2. Section 5 mismatches:**
- **HEAD at entry was `a1fd388c`,** one arbiter commit past the `4baf986c` the instruction names.
  `src` and `tests` were identical to `3d6a2c12` either way.
- **`CLAUDE.md` §1's top row reads HM-DEC-167.** That is neither `PROJECT_STATUS.md`'s
  HM-DEC-165, which `tools/status.sh` writes as a literal, nor the reload's `CPS-DEC-0167`. I did
  not edit `CLAUDE.md`.
- **`CwProbabilisticDecoder.Envelope` exists at `7e209cb4`.** The instruction lists it among the
  members to check. It was never a seam; the absent `Envelope` is a record in `CwReferenceDecoder`.
- **Keeping `CwElementPitch` could not make `ElementPitchLine` build.** Its input,
  `CwProbabilisticResult.Elements`, comes from HEAD's decoder (section 1, decision 4).
- **`tools/Hamlet.PitchRank` is in `Hamlet.sln` and uses seven HEAD-only names.** The
  instruction does not mention it.
- **The clean synthetics do not read the empty string at the restored decoder.** They read `■`
  placeholders.
- **Held as stated:** version 1.13.78; `PROJECT_STATUS.md` read unit 391; the 45 commits
  `1a84188e` down to `2068f868`; 20 files, 6684 insertions and 98 deletions; the ten `A` rows as
  named; the eleven transmit files identical; `tests/Hamlet.RadioEngine.Tests/Cw`, 34 files; the
  four 1.4 tests exist; `docs\unit239-failing-set.txt` has 51 lines;
  `docs\cw-retired-tests.txt` absent and not created; three preflight worktrees.

**3. Three of my own decisions that you may want to reverse.** *Author's, overrulable.* Each
is in section 1:
- `pitch-rank` taken out of the solution build.
- The queue counters return nought.
- The element-pitch line now says *not measured*.

The first two are one-line reversals. The third is step 4's to bring back with the rework's
elements if they earn their place.

**4. Task 5 dropped.** It was the named drop candidate, dropped on the clock rule. The failing
set at the restored decoder is unmeasured, and step 3 inherits it. Four previously
uncompilable tests now compile against thin seams: `NoCwDecodeInDigitalModeTests`,
`AHeldPitchDoesNotOutliveItsEvidenceTests`, `WhereAcquisitionPointsTests` and
`TheSpanRatioReachesTheSidecarTests`. None has been run.

**5. The headless dispatcher loop cost 8 lost runs across the four app-line invocations.** *A
finding.* The lost tests were never the same twice, and each passed in the other run. It is the
§6 lost run, recorded and not chased.

**`validate-output.bat`:** not run, since it asked for approval in earlier units. I checked this
file against its rules by hand: the ordering block and `UNIT:` above section 1, a `UNIT:` line
with no parentheses and none of `& | < > ^`, four sections in order with the canonical names,
and section 4 present.

### Asks still outstanding

**Carried per HM-DEC-139: unit 391's items 2 to 6 and unit 390's nine, verbatim. None is this
unit's to answer. Unit 391's item 1 is answered by R53.**

**Unit 391's items 2 to 6, verbatim:**

