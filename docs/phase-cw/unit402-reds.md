# Unit 402 - the eight reds, first attempt

Work instruction 402, step 3 criterion 6 of *CW decodes again*. Every number here is an
indication (FACT-004); this machine has no radio (FACT-006). *Green* means the assertion held and
nothing more (§0.0).

## 1. Entry

HEAD at entry `306774fc`, as named. `Directory.Build.props` 1.13.88 to 1.13.89.
`PHASE_STATUS.md` read `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 401 - the pile is closed out on
R49's letter`; set to `CURRENT_STEP: 3` and `402 - the eight reds, first attempt`.

**Decision 9's diff**, run in `.run-unit/unit402-copy.sh`:

```
$ git diff --stat 162f0259 HEAD -- src tests docs/carry-forward-tests.txt docs/unit239-failing-set.txt
(nothing)
```

It printed nothing, so unit 401's exit runs are this unit's entry numbers for the two lines: APP 278
of 278 in 156 s, ENGINE 178 of 178 in 374 s.

`git diff --stat 5688a8a5 HEAD -- src` printed nothing. The eleven transmit files printed nothing
against `7e209cb4`.

**The floors at entry**, one build (14 s, warnings as errors, 0 errors), then `--no-build`, one type
per invocation:

| type | result | wall |
|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37, every row identical to unit 401's exit (`unit402-cmp.sh`, diff rc 0) | 93 s |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 | 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 2 of 2 | 2 s |

**Capture rows at entry, decision 2's baseline** (name, characters, elements, unsure, tone Hz):

| capture | characters | elements | unsure | tone |
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

**The six types at entry**, `--no-build`, one type per invocation:

| type | result | wall |
|---|---|---|
| `CwAcquisitionWindowTests` | 10 of 12; #6 and #15 red | 18 s |
| `CwEmissionGateTests` | 7 of 8; #24 red | 9 s |
| `CwAdjudicationTests` | 10 of 11; #41 red | 13 s |
| `CwReceiverFixtureTests` | 23 of 27; #42 to #45 red | 14 s |
| `CwFixtureTests` | 22 of 23; `NothingTheDecoderWasSureOfIsWrong(fading-18wpm)` red, expected | 4 s |
| `CwDisplacementFloorTests` | 6 of 6 | 3 s |

Every printed case number:

- `CwAcquisitionWindowTests`: bare 25 wpm **0.75 (bar 0.79, #6 red)**, 28 wpm 0.89, 30 wpm 0.88,
  35 wpm 0.86 (bar 0.78); run-up 25 wpm 0.95, 28 wpm 0.82, 30 wpm 0.81 (bar 0.80); slow end
  **12 wpm 18 dB 0.54 (bar 0.66, #15 red)**, 12 wpm 6 dB 0.93, 10 wpm 18 dB 1.00, 12 wpm 3 dB 0.96,
  10 wpm 3 dB 0.98. #15 is 0.54 here, not unit 394's 0.63: unit 394 measured before unit 400 moved
  the harness to the settled transcript.
- `CwEmissionGateTests`: noise 0 characters, 0 elements, no tone, wpm none; **#24: real signal
  reported none wpm** (`Assert.NotNull`). The other six green.
- `CwAdjudicationTests`: below the floor -3, -6, -10 dB 0 characters each; fast fist 21
  characters; off-pitch 500, 750, 875, 400 Hz found 500, 750, 875, 405 Hz with 26, 26, 30, 27
  characters; fade 27 characters; clearing 47 before, 116 in all; **#41: no speed was ever named**
  (`Assert.NotEmpty`).
- `CwReceiverFixtureTests`: edge tier 0 emitted each; tone found on all 19 fixtures;
  **#43 coverage-easy 112 characters, 5 unreadable, 37 not in the message; #44 exchange-easy 85, 3,
  21; #45 tightfist-easy 38, 1, 3; #42 qsk-preamble 70 characters during the preamble, 134 in all,
  own transmit measured 9.1 s.**
- `CwFixtureTests`: every case green but `fading-18wpm`'s confident-mistakes case.

**The greps.**

`grep -n "WordsPerMinute\|SpeedIsReacquiring"` over `Cw/*.cs` and `Cw/Fixtures/*.cs`, reads of the
decoder's guarded properties only (request fields and `Reading.WordsPerMinute` left out):

| file:line | reads | compiled |
|---|---|---|
| `Cw/CapturedSignalTests.cs:188` | `Decoder.WordsPerMinute` | yes |
| `Cw/CwEmissionGateTests.cs:81,96,103` | `WordsPerMinute` | yes |
| `Cw/CwSpeedSilenceTests.cs:51,148,149` | `WordsPerMinute`, `SpeedIsReacquiring` | yes |
| `Cw/WhyTheGateDidNotFireTests.cs:188` | `SpeedIsReacquiring` | yes |
| `Cw/TheDisplacementFloorFourWaysTests.cs:114,115` | both, printer | yes |
| `Cw/Fixtures/CwAdjudicationTests.cs:140` | `WordsPerMinute` | yes |
| `Cw/Fixtures/CwTwoStationTests.cs:221` | `Decoder.WordsPerMinute` | yes |
| `Cw/AMoveStartsTheDecoderFreshTests.cs:49,62` | `Reading.WordsPerMinute` | **excluded** (`<Compile Remove>` line 41) |

The csproj excludes eleven files at lines 41 to 51; none of the others above is among them.

`grep -rn "NoSingleClockIsFittedAcrossBothStations" tests`: the source hit is
`Cw/Fixtures/CwTwoStationTests.cs:217`, compiled, beside the remark at
`CwAdjudicationTests.cs:170` (other hits are built binaries).

`grep -n "OwnTransmitSeconds\|_samplesAtDiscontinuity\|_hasFollowed" src/Hamlet.RadioEngine/Cw/*.cs`:

```
CwDecodeReport.cs:28   /// <param name="OwnTransmitSeconds">
CwDecodeReport.cs:58   double OwnTransmitSeconds = 0,
CwDecoder.cs:34        private long _samplesAtDiscontinuity;
CwDecoder.cs:59        private bool _hasFollowed;
CwDecoder.cs:422       => _hasFollowed
CwDecoder.cs:423           ? _lastSample - _samplesAtDiscontinuity
CwDecoder.cs:685       _samplesAtDiscontinuity = reading.SampleIndex;
CwDecoder.cs:686       _hasFollowed = true;
```

`Hamlet.RadioEngine.csproj:144` carries `InternalsVisibleTo Hamlet.RadioEngine.Tests`.
