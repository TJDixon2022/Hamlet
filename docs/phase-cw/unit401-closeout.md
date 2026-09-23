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
