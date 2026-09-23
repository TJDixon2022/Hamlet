# Unit 395 - the rework goes back one piece at a time, on numbers

Step 4 of *CW decodes again*. Section 1 is criterion 4.1, the list. Section 2 is one row per
piece judged, in 4.5's shape. Section 3 holds the numbers: the numbers before piece 1 at entry,
the printer's texts and distances, and the exit round. Every number here is an indication
(FACT-004); *green* means the assertion held and nothing more (`CLAUDE.md` §0.0).

## 1. The list

Criterion 4.1. Made by `.run-unit\unit395-list.sh`: `git log --reverse --date=short
--format="%h|%ad|%s" --name-only 07f0397a..HEAD --until=2026-09-04 -- src/Hamlet.RadioEngine/Cw`,
then `git diff-tree --name-only -r` for each hash's files outside `Cw` and `git log -1 --format=%b`
for its body. **84 commits: 39 up to `7e209cb4`, 45 after it.** `7e209cb4` itself touches nothing
under `Cw`; the last Cw commit at or before it is `ca252057`. The same log over the eleven
transmit files, over `07f0397a..` and over `7e209cb4..`, **printed nothing**: no commit in either
table touches a transmit file. The claim column is the subject, with a clause from the body where
the subject alone does not say what changed. `Cw\` below means `src\Hamlet.RadioEngine\Cw\`.
Files outside `Cw` are named and are never re-applied (R50); root docs such as `OUTPUT.md`,
`PROJECT_STATUS.md`, `WORK_INSTRUCTIONS.md` and the `ANALYSIS-*.md` files are listed as *docs*.

### 1.1 In the tree by R53's restore, not a piece

The 39 Cw commits after `07f0397a`, 0.2's literal answer, up to and including `7e209cb4`.

| # | Hash | Date | Files under Cw | Claim |
|---|---|---|---|---|
| a1 | 8e3ee277 | 08-21 | CwDecoder, CwProbabilisticDecoder, CwProbabilisticStream | decode CW by likelihood instead of by threshold |
| a2 | f28657b6 | 08-21 | CwDecoder, CwProbabilisticStream | stop decoding the operator's own sending, and stop the screen moving |
| a3 | 866d2259 | 08-21 | CwDecoder, CwProbabilisticDecoder | point every reported number at the decoder that decodes |
| a4 | 4bc3bce8 | 08-21 | CwCharacter, CwDecoder, CwGapClasses, CwGate, CwSettledPass, CwSignalWatch, CwTiming | delete the old CW decoder |
| a5 | 7fb7f5d0 | 08-21 | CwDecoder | hold the tracker inside a character again |
| a6 | 55362633 | 08-21 | CwDecoder, CwProbabilisticDecoder, CwProbabilisticStream | let the decoder say where it is in a character |
| a7 | 67ac0796 | 08-21 | CwDecoder, CwProbabilisticStream, CwToneTracker | build the window clear, and say why it is not switched on |
| a8 | d0fd0b56 | 08-21 | CwDecoder | empty the window when Hamlet crosses to somebody else |
| a9 | 62a4648a | 08-21 | CwDecoder, CwProbabilisticStream | turn the window clear off, and keep the machinery |
| a10 | ceb3bd5d | 08-21 | CwToneSurvey, CwToneTracker | show what the survey admitted, not only what it chose |
| a11 | df5f7e4c | 08-21 | CwProbabilisticDecoder | find where characters break, and measure three fixes that do not work |
| a12 | 7f2e90f4 | 08-22 | CwProbabilisticDecoder | score a segment's length as a ratio, not as a difference |
| a13 | 76b295cc | 08-22 | CwProbabilisticStream, CwUnitEstimator | measure the sender's dit instead of searching for it |
| a14 | 58768058 | 08-22 | CwProbabilisticDecoder, CwProbabilisticStream, CwUnitEstimator | take a sender's gap lengths from the gaps |
| a15 | f48e6b19 | 08-22 | CwProbabilisticStream, CwUnitEstimator | use a sender's own gap lengths once the structure has held |
| a16 | 3b7f7e13 | 08-23 | CwCharacter, CwProbabilisticDecoder, CwProbabilisticStream | score every character's own span against the key never going down |
| a17 | baf51ff0 | 08-23 | CwProbabilisticStream | set the refill guard where a fresh stream can see it |
| a18 | 2eb40706 | 08-23 | CwProbabilisticDecoder | put two senders in one passband and measure what survives |
| a19 | 7907cc0e | 08-23 | CwProbabilisticDecoder, CwProbabilisticStream | window the integrator, and make the two envelope paths agree |
| a20 | fe104f02 | 08-23 | CwProbabilisticDecoder | measure what each integrator width buys and what it costs |
| a21 | 3c4b1b16 | 08-23 | CwCompetitor, CwDecodeReport, CwDecoder, CwToneTracker | say when somebody else is keying in the same passband |
| a22 | f93c388f | 08-24 | CwProbabilisticDecoder | carry each character's span length, and measure the margins |
| a23 | 07d8dc23 | 08-24 | CwCharacter, CwProbabilisticDecoder, CwProbabilisticStream | let each character answer for itself, and keep the window as a guard |
| a24 | 4e4b9bac | 08-24 | CwDecoder, CwToneTracker | let the operator hold the decoder's pitch |
| a25 | c052d100 | 08-24 | CwProbabilisticDecoder | record the premise reproduced in-tree, and what disagreed |
| a26 | b65ac6b4 | 08-24 | CwProbabilisticDecoder | take the noise scale from the Rayleigh identity, over a rolling span |
| a27 | 1a5d9285 | 08-24 | CwProbabilisticDecoder | re-express the guard in the units the scale now uses |
| a28 | ab95f4d9 | 08-24 | CwProbabilisticDecoder | move the guard to 1.40, inside the same measured gap |
| a29 | 61b423aa | 08-24 | CwProbabilisticDecoder | read nothing from digital silence rather than noise from a floor |
| a30 | b022d925 | 08-24 | CwProbabilisticDecoder | keep the quarter point as the silence test, and say what it costs |
| a31 | ff6590d6 | 08-24 | CwToneTracker | report where an admitted station sits, not which bin found it |
| a32 | 13661992 | 08-24 | CwDecodeReport, CwDecoder, CwToneTracker | say when the reported pitch is a bank centre rather than a station |
| a33 | aa5a93c7 | 08-24 | CwDecoder, CwToneTracker | hold the last measured pitch, and withdraw a refinement measured worse |
| a34 | 13119e6e | 08-24 | CwProbabilisticDecoder | raise the speed ceiling to forty, and say when a winner is at the edge |
| a35 | 07260a2a | 08-25 | CwProbabilisticDecoder | put a floor under what a character has to prove before it prints |
| a36 | 7c8a5f00 | 08-25 | KeyingEnvelope | put the recording's keying duty on the capture sheet |
| a37 | cf83821f | 08-25 | CwKeyingMeter, KeyingEnvelope | let the keying witness look where the decoder looks |
| a38 | 2f52f6db | 08-25 | CwKeyingMeter | the witness judges on element length, guarded by the swing |
| a39 | ca252057 | 08-25 | CwDecoder | one decoder, whatever size the audio arrives in |

### 1.2 The 45 pieces, oldest first

Every commit after `7e209cb4` to 2026-09-03 that touched `Cw`, in `git log --reverse` order. By
day: 2 on 08-25, 11 on 08-26, 4 on 08-27, 4 on 08-28, 10 on 08-29, 6 on 08-30, 4 on 08-31, 4 on
09-03, as the instruction counted. A piece is the row's `Cw` diff only.

| n | Hash | Date | Files under Cw | Files outside Cw, not re-applied | Claim |
|---|---|---|---|---|---|
| 1 | 2068f868 | 08-25 | CwDecoder, CwProbabilisticStream, CwToneTracker | src\Hamlet.RadioEngine\Audio\AudioTap.cs; 2 tests | read the first seconds again, at the note they were sent on - re-mix the window once the first pitch is measured, from the tap's raw audio |
| 2 | 6fc36a1e | 08-25 | CwCharacter, CwProbabilisticDecoder, CwProbabilisticStream | src\Hamlet.App MainWindowViewModel | log how close the argument was, beside how loud it was - score a character against the second-best reading |
| 3 | 3e84ac74 | 08-26 | CwDecoder, CwToneTracker | MainWindowViewModel; 1 test | let go of a pitch measured on a frequency the radio has left |
| 4 | 39a42c3f | 08-26 | CwCharacter | props; MainWindowViewModel; 1 app test | put the margin's share of the span on the sheet |
| 5 | 9de394da | 08-26 | CwDecoder, CwProbabilisticStream, CwToneTracker | none | open the integrator width and confirmation window as constructor parameters for a sweep; nothing in the app passes either |
| 6 | 3d4694e5 | 08-26 | CwToneTracker | props | record that the confirmation window stays at two, and why |
| 7 | 7fb89d5e | 08-26 | CwToneSurvey, CwToneTracker | none | record which admission test refused which bin, and by how much |
| 8 | 1bf4372d | 08-26 | CwToneSurvey | props | the separation bound must not move, and the reason is upstream |
| 9 | 44cf3fc8 | 08-26 | CwToneSurvey, CwToneTracker | none | build both gate-threshold derivations, both off by default |
| 10 | 4786c7e7 | 08-26 | CwToneSurvey | props | both gate derivations measured, neither ships |
| 11 | f2e1db7a | 08-26 | CwToneSurvey, CwToneTracker | none | collect the raw run stream, marks and gaps apart, only when a caller asks |
| 12 | f27174b5 | 08-26 | CwDecodeReport, CwDecoder | docs; MainWindowViewModel | the operator may assert a station, and Hamlet finds the pitch |
| 13 | 386fdb5d | 08-26 | CwDecoder, CwJointCutter, CwProbabilisticDecoder, CwProbabilisticStream | props; src\Hamlet.App AppSettings, MainWindowViewModel | decide the cuts and the characters together, behind a setting |
| 14 | 8ca6a633 | 08-27 | CwPitchRanking | docs; 1 test | rank candidate pitches by what the decoder reads at each; not wired |
| 15 | 4c6e4321 | 08-27 | CwDecodeReport, CwDecoder, CwPitchChoice, CwToneTracker | docs; 1 test | let the strongest bin choose the note where nothing is confirmed, and record that it did |
| 16 | 501e8e2d | 08-27 | CwPitchRanking | docs; 2 tests | bank the key-up analysis and delete CwPitchRanking |
| 17 | f9c11989 | 08-27 | CwProbabilisticDecoder | docs; 1 test | fit the key-up state, measure it, and ship nothing |
| 18 | b48d1158 | 08-28 | CwPitchRanking | docs; 1 test; tools\Hamlet.PitchRank | rank candidate pitches against one band-wide noise floor |
| 19 | 0f2089f3 | 08-28 | CwDecodeReport, CwDecoder, CwPitchChoice | docs; MainWindowViewModel; 1 test; tools\Hamlet.PitchRank | let the ranking supply the mixdown pitch, and leave it off |
| 20 | ac1d56da | 08-28 | CwReferenceDecoder | docs; 1 test; tools\Hamlet.PitchRank | port the reference decoder's chain into the engine |
| 21 | 62262b94 | 08-28 | CwDecoder | docs; data\bands; MainWindowViewModel; 3 tests | start the decoder fresh when the dial actually moves |
| 22 | 0f48c33e | 08-29 | CwAccuracy | docs; 1 test | score a decode against what was actually sent |
| 23 | b8cad1f9 | 08-29 | CwAccuracy | docs; tools\Hamlet.PitchRank | test every confidence the decoder has against correctness |
| 24 | fc1ee77f | 08-29 | CwProbabilisticDecoder | docs | index the lattice by hop and kind |
| 25 | 71b4f044 | 08-29 | CwProbabilisticDecoder.Posterior, CwProbabilisticDecoder | docs; 1 test | a posterior over the lattice, in the log domain |
| 26 | 68a18d66 | 08-29 | CwCharacter, CwProbabilisticDecoder, CwProbabilisticStream | docs; tools\Hamlet.PitchRank | carry the posterior to each character and measure it |
| 27 | a91d8fe7 | 08-29 | CwDecoder, CwProbabilisticDecoder.Posterior, CwProbabilisticDecoder, CwProbabilisticStream | docs; tools\Hamlet.PitchRank | a temperature on the path score, swept; ships at 1.0, the decode untouched |
| 28 | ade52536 | 08-29 | CwSpectralPeak | docs; cwbench.py; 1 test; tools\Hamlet.PitchRank, tools\cwbench | the bench's run-merging bug is not in Hamlet, and here is why |
| 29 | efcd5242 | 08-29 | CwDecoder | docs; 1 test; tools\Hamlet.PitchRank, tools\cwbench | find the pitch from the band rather than from a bin already chosen |
| 30 | 95a5e063 | 08-29 | CwDecoder | docs | assert nothing from a pitch nobody judged to be a station |
| 31 | b7147b1f | 08-29 | CwUnitEstimator | docs; 1 test; tools\Hamlet.PitchRank | measure the percentile threshold and refuse it, three ways |
| 32 | c8685e4d | 08-30 | CwSpectralPeak | docs; 1 test; tools\Hamlet.PitchRank | the peak window buys nothing, and N4L does not come back |
| 33 | 4935a4f8 | 08-30 | CwUnitEstimator | docs; tools\Hamlet.PitchRank | measure the peak-referenced threshold and refuse it |
| 34 | e6b1ece7 | 08-30 | CwUnitEstimator | docs; tools\Hamlet.PitchRank | a key-down that comes back inside twelve milliseconds never ended |
| 35 | dfb357ef | 08-30 | CwSpectralPeak | docs; tools\Hamlet.PitchRank | the four configurations, and a carrier count that does not work |
| 36 | efc33267 | 08-30 | CwCounterTrail | docs; MainWindowViewModel; 1 test; 18 fixture files | the sheet stops lying about arithmetic, and tonight's captures land |
| 37 | aeea24f2 | 08-30 | CwDecoder, CwSwingSurvey | docs; 1 test; tools\Hamlet.PitchRank | admit a station by how far its bin swings, not by its average |
| 38 | a37cfcff | 08-31 | CwElementPitch, CwJointCutter, CwProbabilisticDecoder, CwUnitEstimator | docs; 1 test; tools\Hamlet.PitchRank, tools\cwbench, tools\write-status.py | lower the speed ceiling to thirty, and carry every element out |
| 39 | a09b36a7 | 08-31 | CwStreamSplit | docs; 1 test; tools\Hamlet.PitchRank | measure whether two people are sending, and withhold the verdict |
| 40 | ee2cba8d | 08-31 | CwUnitEstimator | 1 test; tools\Hamlet.PitchRank | the element streams agree, so the reading is lost after them |
| 41 | 2828ab69 | 08-31 | CwStreamSplit | docs; tools\Hamlet.PitchRank | report work instruction 056 - the streams agree, the reading is lost after |
| 42 | 43efc525 | 09-03 | CwDecoder | src\Hamlet.RadioEngine\Audio AudioHandoff, WasapiAudioSource; 1 test | the tap is fed from the callback, the decoder from a queue |
| 43 | 865e66d8 | 09-03 | CwDecoder | MainWindowViewModel; 1 test | no CW decode in Digital mode |
| 44 | 9c2a7f99 | 09-03 | CwDecoder, CwKeyingMeter | docs; MainWindowViewModel; src\Hamlet.RadioEngine\Audio Ft8SlotWatch, ReusableWindow; 1 test | a reader on a timer stops allocating the audio it reads |
| 45 | 1a84188e | 09-03 | CwKeyingMeter | docs; src\Hamlet.App Telemetry\AppEvents, MainWindowViewModel; src\Hamlet.RadioEngine\Audio AudioArrival, CallbackBudget, DigitalCaptureSheet, WasapiAudioSource; 3 tests | the callback budget is set, not inherited, and overruns are counted |

Pieces 15 and 19 wrote `CwPitchChoice.cs`, already in the tree as the one HEAD-only file unit
392 kept; their hunks on it are dropped as already in the tree when they come up. Pieces 1, 42,
44 and 45 changed files under `src\Hamlet.RadioEngine\Audio`, which is outside `Cw` and not
taken (R50); a piece whose `Cw` half needs its `Audio` half fails to build and is out, decision 4.

## 2. The pieces judged

One row per piece, in the list's order, judged per decisions 6 and 7 against the kept state's
numbers. Distances are on the settled text (section 3.1a). *Before* is the kept state before the
piece; for piece 1 that is section 3.1 and 3.1a.

| n | Hash | Claim | Number before | Number after | Captures wall | Kept or out | Why |
|---|---|---|---|---|---|---|---|
| 1 | 2068f868 | read the first seconds again, at the note they were sent on | 021410 47 ch, WEEKEND 5, THINKING 5, FLEX 1; 013637 63 ch, ABOVE 2, BREEZE 2; 004507 50, 003758 63, 031948 34, 012748 4; synthetics 0 of 2 | 021410 47 ch, WEEKEND 5, THINKING 5, FLEX 1; 013637 63 ch, ABOVE 2, BREEZE 2; 004507 49, 003758 58, 031948 31, 012748 2; synthetics 0 of 2 | 94 s | **out**, piece `5cf8f9c8`, reverted in the next commit | applied clean and built; captures 37 of 37 and adjudicated 13 of 13 green; no named number moved and no capture rose, while four fell - 004507, 003758 and 031948 are anchored, so their counts are printed and not asserted, and 012748 fell from 4 to its floor of 2; the other 33 identical. Out under R51. |

| 2 | 6fc36a1e | log how close the argument was, beside how loud it was | the entry state, piece 1 out: 021410 47 ch, WEEKEND 5, THINKING 5, FLEX 1; 013637 63 ch, ABOVE 2, BREEZE 2; synthetics 0 of 2 | every one of the 37 captures identical in characters, elements, unsure and tone; the five distances identical; synthetics 0 of 2 | 95 s | **out**, pieces `99db35fc` and `0246f209`, both reverted in the next commit | applied clean without piece 1 but did not build: `CwCharacter.cs(179,25): error CS0102: The type 'CwCharacter' already contains a definition for 'WidestRecordedLlr'`, and the same for `MarginLlr` - its `CwCharacter` hunk is unit 392's seam, already in the tree, so it was dropped in `0246f209` under decision 3; then built clean, 37 of 37, 13 of 13, and nothing moved. Out under R51. |

Piece 1's commit carried only `Cw`; `AudioTap.cs` was not taken and the `Cw` half built without
it.

## 3. The numbers

### 3.1 The numbers before piece 1, at entry

Unit 395 task 0, HEAD `ee0ea0dc` plus task 0's non-`src` edits, `src` as unit 394 left it.
`TheCapturesThatDecodeKeepDecodingTests`, one invocation, `--no-build` after the engine line's
build, 37 of 37 green in 92 s wall, 1.53 min test time; output `.run-unit\unit395-floors-1.txt`.
Characters, elements and unsure as the test prints them from `decoder.Report`; tone in Hz.

| Capture | Characters | Elements | Unsure | Tone |
|---|---|---|---|---|
| cw-2026-08-17-013347 | 59 | 108 | 2 | 625 |
| cw-2026-08-17-013622 | 55 | 84 | 4 | 600 |
| cw-2026-08-17-134712 | 63 | 98 | 42 | 500 |
| cw-2026-08-18-004507 | 50 | 118 | 1 | 500 |
| unadjudicated/cw-2026-08-18-003016 | 57 | 149 | 3 | 670 |
| unadjudicated/cw-2026-08-18-003126 | 54 | 144 | 6 | 665 |
| unadjudicated/cw-2026-08-18-003758 | 63 | 121 | 19 | 500 |
| unadjudicated/cw-2026-08-20-014854 | 0 | 0 | 0 | 600 |
| unadjudicated/cw-2026-08-20-014935 | 0 | 0 | 0 | 825 |
| unadjudicated/cw-2026-08-22-014113 | 0 | 0 | 0 | 600 |
| unadjudicated/cw-2026-08-22-014308 | 0 | 0 | 0 | 575 |
| unadjudicated/cw-2026-08-22-031838 | 57 | 126 | 15 | 525 |
| unadjudicated/cw-2026-08-22-031905 | 42 | 118 | 6 | 300 |
| unadjudicated/cw-2026-08-22-031948 | 34 | 114 | 3 | 500 |
| unadjudicated/cw-2026-08-22-032012 | 44 | 120 | 1 | 500 |
| unadjudicated/cw-2026-08-22-032050 | 53 | 123 | 9 | 325 |
| unadjudicated/cw-2026-08-22-032113 | 55 | 118 | 8 | 650 |
| unadjudicated/cw-2026-08-22-032129 | 66 | 119 | 1 | 650 |
| unadjudicated/cw-2026-08-23-001520 | 5 | 45 | 4 | 600 |
| unadjudicated/cw-2026-08-23-001831 | 55 | 124 | 11 | 525 |
| unadjudicated/cw-2026-08-23-001952 | 75 | 142 | 19 | 525 |
| unadjudicated/cw-2026-08-23-002016 | 75 | 136 | 31 | 525 |
| unadjudicated/cw-2026-08-24-012403 | 22 | 65 | 1 | 440 |
| unadjudicated/cw-2026-08-25-011552 | 30 | 89 | 8 | 500 |
| unadjudicated/cw-2026-08-25-012748 | 4 | 16 | 2 | 395 |
| unadjudicated/cw-2026-08-25-012823 | 41 | 62 | 15 | 450 |
| unadjudicated/cw-2026-08-25-012922 | 50 | 112 | 5 | 475 |
| unadjudicated/cw-2026-08-25-013010 | 54 | 131 | 6 | 475 |
| unadjudicated/cw-2026-08-25-013150 | 58 | 139 | 7 | 495 |
| unadjudicated/cw-2026-08-25-013303 | 54 | 146 | 10 | 500 |
| unadjudicated/cw-2026-08-25-013402 | 61 | 161 | 5 | 525 |
| unadjudicated/cw-2026-08-25-013520 | 60 | 153 | 5 | 540 |
| unadjudicated/cw-2026-08-25-013637 | 63 | 164 | 3 | 550 |
| unadjudicated/cw-2026-08-25-021410 | 47 | 99 | 11 | 550 |
| unadjudicated/cw-2026-08-25-021629 | 47 | 96 | 20 | 500 |
| unadjudicated/cw-2026-08-25-021825 | 41 | 74 | 16 | 400 |
| unadjudicated/cw-2026-08-26-125941 | 0 | 0 | 0 | 400 |

36 rows equal their floor in the table; `012748` stands at 4 characters and 16 elements against a
floor of 2 and 4. The anchored cases, the ones `TheAdjudicatedReadingsKeepReadingTests` covers,
print their count and do not assert it. `TheAdjudicatedReadingsKeepReadingTests` 13 of 13 green in 29 s wall;
`CwFixtureTests.TheCleanRecordingsDecodeExactly` 0 of 2, `clean-12wpm` and `clean-18wpm` red as
R53 expects, in 3 s. Outputs `.run-unit\unit395-floors-2.txt` and `-3.txt`.

### 3.1a The printer at entry, the numbers before piece 1

`Cw\TheReworkNumbersPrinterTests`, run alone, 2 of 2 (it asserts nothing), 10 s wall; output
`.run-unit\unit395-printer-entry.txt`. **Decision on the text member:** `CwDecoder.Reading` is
the last window only, and `CwDecodeHarness` takes text from `CharacterDecoded`, which re-emits
the leading edge at every revision (`FFRLELETT`, `NEVVENEN`). The `CharacterSettled` text holds
each final character once and has the sidecar's shape (`FLENT 66O`, `AB OV E`, `BR EE Z E`).
**Pieces are judged on the settled distances**; the harness-text distances are printed and
recorded beside them. The printer was built twice, once to find this and once with the settled
distances added.

| Capture | Settled text | Word | Distance | Nearest substring | Harness-text distance |
|---|---|---|---|---|---|
| 021410 | `■ ■ ■ M ■ ■ ■ ■ T O MTT T  Y M TT ■ ■ O AO IHI DT ■RIGHR IS ■ FLENT 66OAM` | WEEKEND | 5 | ` FLEN` | 5 |
| | | THINKING | 5 | `T ■RIG` | 5 |
| | | FLEX | 1 | `FLE` | 2 |
| 013637 | `TE MP NEVEN T REV■R G O T AB OV E ■7 5 F ES ■CLEAR S KY LI TE BR EE Z E ALL DAY JUST AWE SO` | ABOVE | 2 | `AB OV` | 3 |
| | | BREEZE | 2 | `BR EE` | 3 |

**021410 in the floors harness does not carry `ATEEKEND` or `TTHINKING`.** Those are the app's
readings on the evening, a minute of listening fed in 960-sample chunks; the harness pumps one
hop at a time from the start of the file, and its settled text carries only the tail
(`■RIGHR IS ■ FLENT 66O`, the sidecar's `■RIGHR IS ■ FLENX 66O`). So WEEKEND and THINKING start
at 5, the nearest a 7- and 8-letter word gets to text with no trace of either, and FLEX at 1.

### 3.2 The carry-forward lines at entry

- **App**, line 7 as printed: 276 of 278 in 171 s, 2 lost to the headless dispatcher loop
  (`ThePsk31ConversationCardTests.NoSlotClockUnderPsk31AndFt8AndFt4StillShowIt`,
  `TheRstIsYoursToCorrectTests.OnTheWindowTheTwoReportsAreBoxesWithTheirMarks`); re-run once,
  276 of 278 in 169 s, 2 lost the same way (`BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`,
  `ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed`). Each lost name absent from
  the other run's failures, so green there; no red on an assertion.
- **Engine**, line 9 as printed: 176 of 176 in 375 s of 480.
