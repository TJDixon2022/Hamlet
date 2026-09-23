# Unit 401 - the pile is closed out on R49's letter

Work instruction 401, step 3 of *CW decodes again*. Every number here is an indication (FACT-004);
this machine has no radio (FACT-006). *Green* means the assertion held and nothing more (§0.0).

## 1. Entry

HEAD at entry `bf7bae57`, as named. `Directory.Build.props` 1.13.87 to 1.13.88.

**Decision 6's diff**, run in `.run-unit/unit401-entry.sh`:

```
$ git diff --stat 5e70860d HEAD -- src tests docs/carry-forward-tests.txt docs/unit239-failing-set.txt
(nothing)
```

It printed nothing, so unit 400's exit runs are this unit's entry numbers for the two lines: APP 277
of 278 in 161 s and 277 of 278 in 166 s, each loss a different name to the headless dispatcher loop
at 1 ms and green in the other run; ENGINE 176 of 176 in 374 s.

`git diff --stat 5688a8a5 HEAD -- src` printed nothing. The eleven transmit files printed nothing
against `7e209cb4`.

**The floors at entry**, one build (16 s, warnings as errors, 0 errors), then `--no-build`, one type
per invocation:

| type | result | wall |
|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37, every row identical to unit 400's exit (`unit401-cmp.sh`, diff rc 0) | 94 s |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13, every printed line identical to unit 400's exit but the total time | 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 2 of 2, `clean-12wpm` and `clean-18wpm` | 1 s |

**The types this unit touches or measures, at entry:**

| type | result | wall |
|---|---|---|
| `CwFixtureTests` whole | 22 of 23, `NothingTheDecoderWasSureOfIsWrong(fading-18wpm)` red | 4 s |
| `CwDisplacementFloorTests` | 0 of 6, every case red on `Assert.EndsWith` `CQ DE W1AW K` | 14 s |
| `CwEmissionGateTests` | 7 of 8, `NoSpeedIsNamedWithoutCharactersToNameItFrom` red on `Assert.NotNull` | 9 s |

The six displacement cases' texts and `Retunes` are the printer's way 1 rows in section 2; xUnit's
`EndsWith` failure did not print the actual string to the console.

**`grep -n "Cw\|CW" docs/carry-forward-tests.txt` at entry:**

```
27:    CW     read  TheAdjudicatedReadingsKeepReadingTests, and the 08-25 cases of TheCapturesThatDecodeKeepDecodingTests by display name   engine   unit 393
158:    CwAdjudicationTests.ASpeedChangeInRealisticAudio
159:    the 51 CW cases in docs/unit239-failing-set.txt
882:mode in the family's ink (USB-D digital, CW Morse, USB voice) and the name beside; one click
900:WHAT UNIT 393 ADDED. CW's read guard, on the engine invocation, line 9 going from 25 terms to 27:
902:TheCapturesThatDecodeKeepDecodingTests selected by display name. It is PERMANENT because CW went
909:and the two clean synthetics, CwFixtureTests.TheCleanRecordingsDecodeExactly, are red (R53) and
912:recordings is pulled onto the line. Watched red against a CwDecoder that emits nothing, and green
916:ceiling - 26.6 s of process CPU against 20, every character correct - because the CW cases ran
```

Lines 158 and 159 are the known-reds block's two CW lines. Line 27 is the guard table's CW row, 882
a UI description, 900 to 916 the unit 393 paragraph. (Line 9's terms do not contain `Cw` or `CW`
at entry: its CW terms are `TheAdjudicatedReadingsKeepReadingTests` and
`TheCapturesThatDecodeKeepDecodingTests`.) The file is 918 lines, not 919 as the instruction says.

**`grep -rl unit239-failing-set tools .claude`**: no output, rc 1. Nothing under `tools` or
`.claude` reads the set by name.

**Tests still reading `CharacterDecoded`** (`grep -n CharacterDecoded tests/Hamlet.RadioEngine.Tests/Cw/*.cs`),
against the engine csproj's eleven `<Compile Remove>` items at lines 41 to 51:

| file | line | compiled |
|---|---|---|
| `CapturedSignalTests.cs` | 90 | yes |
| `CwDecodeHarness.cs` | 68 | yes - a remark, not a subscription |
| `CwDisplacementFloorTests.cs` | 45 | yes - this unit's |
| `NothingIsReadFromAudioWithNoKeyingTests.cs` | 71 | yes |
| `TheGateHasItsOwnWindowNowTests.cs` | 62 | yes |
| `TheReworkNumbersPrinterTests.cs` | 18 (remark), 56 | yes |
| `WhatBandwidthTheDecoderListensThroughTests.cs` | 216 | yes |
| `WhyTheGateDidNotFireTests.cs` | 99 | yes |

None of the eleven excluded files reads `CharacterDecoded`. Only `CwDisplacementFloorTests.cs` is
touched by this unit; the others are left.

## 2. The trace: the six displacement cases four ways, and #24

`TheDisplacementFloorFourWaysTests.PrintTheRows`, one build of 8 s, run alone, 33 s, asserting
nothing; output `.run-unit/unit401-fourways.txt`. Start 600 Hz, 18 wpm, `Message` as the
displacement type's. Way 1 is `CharacterDecoded` at the test's noise (the type as it stands at
entry); way 2 `CharacterSettled` at the test's noise; ways 3 to 6 `CharacterSettled` at 0.002,
0.005, 0.01, 0.02. The sixth case prints its own band and 0.02 only. `image` and `elsewhere-400`
generate the same audio (400 Hz from 600 at noise 0) and print the same rows, as they should.

| case | way | band | Retunes | ends `CQ DE W1AW K` | text |
|---|---|---|---|---|---|
| image (#18) | 1 | 0 | 1 | no | `■ ■■ ■` |
| image | 2 | 0 | 1 | no | `■ ■ ■ ■ ■ ■ ■A ■ ■ ■■` |
| image | 3 | 0.002 | 1 | yes | `T TEEV VVV VVV CQ DE W1AW K` |
| image | 4 | 0.005 | 1 | yes | `T TEEV VVV VVV CQ DE W1AW K` |
| image | 5 | 0.01 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| image | 6 | 0.02 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| elsewhere-400 | 1 | 0 | 1 | no | `■ ■■ ■` |
| elsewhere-400 | 2 | 0 | 1 | no | `■ ■ ■ ■ ■ ■ ■A ■ ■ ■■` |
| elsewhere-400 | 3 | 0.002 | 1 | yes | `T TEEV VVV VVV CQ DE W1AW K` |
| elsewhere-400 | 4 | 0.005 | 1 | yes | `T TEEV VVV VVV CQ DE W1AW K` |
| elsewhere-400 | 5 | 0.01 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| elsewhere-400 | 6 | 0.02 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| elsewhere-500 | 1 | 0 | 1 | no | `■ ■■ ■` |
| elsewhere-500 | 2 | 0 | 1 | no | `■ ■ ■ ■ ■ ■ ■A ■ ■ ■■` |
| elsewhere-500 | 3 | 0.002 | 1 | yes | `■ ■ T TEET ETET ETET ETET EETT EETT E■ CQ DE W1AW K` |
| elsewhere-500 | 4 | 0.005 | 1 | yes | `■ ■ ■T TEET ETET ETET ETET EETT EET■■ CQ DE W1AW K` |
| elsewhere-500 | 5 | 0.01 | 1 | yes | `■ ■ ■ EEET VVV VVV CQ DE W1AW K` |
| elsewhere-500 | 6 | 0.02 | 1 | yes | `T TEEV VVV VVV CQ DE W1AW K` |
| elsewhere-750 | 1 | 0 | 1 | no | `■ ■■ ■` |
| elsewhere-750 | 2 | 0 | 1 | no | `■ ■ ■ ■ ■ ■ ■A ■ ■ ■■` |
| elsewhere-750 | 3 | 0.002 | 1 | **no** | `■ ■ T TTTT TTTT TTTT TTTT TTTT TTT■■ ■■ OT W1AW K` |
| elsewhere-750 | 4 | 0.005 | 2 | yes | `■ E ■ E I ■ ■ 5EE E ■E EE ■ VVV CQ DE W1AW K` |
| elsewhere-750 | 5 | 0.01 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| elsewhere-750 | 6 | 0.02 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| elsewhere-875 | 1 | 0 | 1 | no | `■ ■■ ■` |
| elsewhere-875 | 2 | 0 | 1 | no | `■ ■ ■ ■ ■ ■ ■A ■ ■ ■■` |
| elsewhere-875 | 3 | 0.002 | 1 | yes | `T TEEV VVV VVV CQ DE W1AW K` |
| elsewhere-875 | 4 | 0.005 | 1 | yes | `T TEEV VVV VVV CQ DE W1AW K` |
| elsewhere-875 | 5 | 0.01 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| elsewhere-875 | 6 | 0.02 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| refused-before (#23) | 1 | 0.06 | 1 | no | `5V H VEVVVSVV I VVHVIVVV E KCTCGQQ N DEDE E WWAJ11AARW W N K` |
| refused-before | 2 | 0.06 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |
| refused-before | 6 | 0.02 | 1 | yes | `TV VVV VVV CQ DE W1AW K` |

**Finding (a), the settled event alone:** it changes the sixth case's ending and no other. The
sixth case (`NothingIsRefusedBeforeAnythingIsBeingRead`, noise 0.06) goes from the leading edge's
`...WWAJ11AARW W N K` to `TV VVV VVV CQ DE W1AW K`. The five silent cases still show blocks on the
settled transcript at noise 0.

**Finding (b), the smallest band:** **0.005.** At 0.002 the 750 Hz case ends `OT W1AW K`; at 0.005
all five silent cases end with `CQ DE W1AW K` and the image case shows `Retunes` 1. The 750 Hz case
at 0.005 shows `Retunes` 2. `AStationElsewhereIsStillFound` asserts only the ending, so that is
recorded here and does not fail the case. At 0.01 and 0.02 every case also shows `Retunes` 1.

**#24**, the request `CwEmissionGateTests` builds (`CQ DE W1AW K`, 18 wpm, 620 Hz, amplitude 0.5,
noise 0.02, seed 5), after `Flush`: `Report.CharactersEmitted` 9, `Reading.WordsPerMinute` 18.46,
`Reading.Text` `' CQ DE W1AW K '`, `WordsPerMinute` **null**, `SpeedIsReacquiring` **True**; the
settled transcript is `CQ DE W1AW K`. The reading has text and a speed inside 14 to 24; the guarded
property returns null because `SpeedIsReacquiring` is true at the end of the signal
(`CwDecoder.cs` line 445). The cause is under `src`. It is recorded here and not changed (R50,
decision 5), and #24 stays red-open.

## 3. The displacement repair

Task 2 started at minute 7 of the unit's hour (first status line 04:57:08), inside decision 11's
clock. Both changes were reached and both kept.

**Change (a)**, line 45 `CharacterDecoded` to `CharacterSettled` with a one-line remark citing unit
400 decision 3 and R12. One build of 6 s. The type 0 of 6 to **1 of 6**: `NothingIsRefusedBeforeAnythingIsBeingRead`
red to green, no case green to red. Kept, commit `8ed230ad`.

**Change (b)**, the `noise` argument at the two silent call sites (line 69, the image case, and
line 93, the four-case theory, five cases between them) from 0 to **0.005**, the smallest band the
printer found for all five, and a class-remark paragraph. The sixth call site keeps 0.06. One build
of 7 s. The type **6 of 6** in 4 s. Its printed lines:

```
400 Hz from 600, noise 0.005: 1 moves, 'T TEEV VVV VVV CQ DE W1AW K'
400 Hz from 600, noise 0.06: 1 moves, 'TV VVV VVV CQ DE W1AW K'
500 Hz from 600, noise 0.005: 1 moves, '■ ■ ■T TEET ETET ETET ETET EETT EET■■ CQ DE W1AW K'
750 Hz from 600, noise 0.005: 2 moves, '■ E ■ E I ■ ■ 5EE E ■E EE ■ VVV CQ DE W1AW K'
875 Hz from 600, noise 0.005: 1 moves, 'T TEEV VVV VVV CQ DE W1AW K'
```

Against the 0.0089 image the remark describes (thirty-five decibels under 0.5), 0.005 is 5 dB under
it. The image is still in the audio above the band, but no longer over exact silence. The remark
says so plainly: the type now proves the tracker holds a station whose image stands 5 dB over the
noise, not over nothing. `Assert.Equal(1, run.Moves)` and every `EndsWith` stand as written. The 500
and 750 Hz cases carry blocks and wrong letters before the call. The 750 Hz case moves twice, and
its assertion is only the ending.

**Neighbors after change (b), before its commit**, `--no-build`, against task 0:

| type | task 0 | after (b) |
|---|---|---|
| captures | 37 of 37 | 37 of 37 in 92 s, every row identical |
| adjudicated | 13 of 13 | 13 of 13 in 29 s, every line identical but the total time |
| `CwFixtureTests` whole (includes the two clean synthetics) | 22 of 23 | 22 of 23, identical by case |
| `CwEmissionGateTests` | 7 of 8 | 7 of 8, identical by case |

| case | task 0 | after (a) | after (b) |
|---|---|---|---|
| `TheTrackerDoesNotLeaveAStationForItsOwnImage` (#18) | red | red | green |
| `AStationElsewhereIsStillFound(400)` | red | red | green |
| `AStationElsewhereIsStillFound(500)` | red | red | green |
| `AStationElsewhereIsStillFound(750)` | red | red | green |
| `AStationElsewhereIsStillFound(875)` | red | red | green |
| `NothingIsRefusedBeforeAnythingIsBeingRead` (#23) | red | green | green |

Nothing was put back.
