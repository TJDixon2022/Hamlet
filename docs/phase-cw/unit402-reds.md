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

## 2. The trace

`tests/Hamlet.RadioEngine.Tests/Cw/TheEightRedsTests.cs`, a printer asserting nothing and on no
line, run alone: `FullyQualifiedName~TheEightRedsTests`, 7 of 7 in 9 s, output
`.run-unit/unit402-eight.txt`. `_samplesAtDiscontinuity` and `_lastSample` are private and are read
by reflection in the printer only.

**Group A, speed.** One row per quarter-second poll. #24, `CQ DE W1AW K`, 18 wpm, 620 Hz, 8.6 s:

| at | text | last wpm | named | reacquiring | follows | since discontinuity | tone |
|---|---|---|---|---|---|---|---|
| 0.00 to 1.25 s | 0 | 0 | null | True | 0 | 0.25 to 1.50 s | 600 |
| 1.50 s | 0 | 0 | null | True | **1** | 0.22 s | 625 |
| 2.75 s | 5 | 18.46 | null | True | 1 | 1.47 s | 625 |
| 5.00 s | 9 | 18.46 | null | True | 1 | 3.72 s | 640 |
| 8.50 s | 13 | 18.46 | null | True | 1 | 7.07 s | 620 |
| flushed | 14 | 18.46 | null | True | 1 | 7.07 s | 620 |

35 polls: null for empty text 11, **reacquiring 24**, outside the bounds 0. Reading ` CQ DE W1AW K `.

#41, `two-station.wav`, 35.8 s, 144 polls: null for empty text 19, **reacquiring 125**, outside the
bounds 0. Follows: 1 at 1.50 s (600 to 625 Hz, text 0), 2 at 9.50 s (625 to 600, text 8), 3 at
16.00 s (600 to 625, text 11), 4 at 25.00 s (625 to 725, text 0 at that poll). Each restarts the
12 s hold, so no poll between them clears it; after follow 4 the file ends 10.76 s later, still
reacquiring. The fitted speed across the first station reads 10.0 to 11.2, and after the handover
14.1, 19.2, 20.9 and 21.8 as the window refills.

**Group B, easy tier, the three events:**

| fixture | event | characters | unreadable | strangers | ends with the message | text |
|---|---|---|---|---|---|---|
| coverage-easy | `CharacterDecoded` | 112 | 5 | 37 | no | `VVEVSVVAW■11S■22E2H33E3S44E4H55NB6E6MZ77NO88T8MO99T9MO900■■SKRYN...` |
| coverage-easy | `CharacterSettled` | 36 | 4 | 7 | no | `VVV1234567890■A■YTTMT■■DETEETEN0CALL` |
| exchange-easy | `CharacterDecoded` | 85 | 3 | 21 | no | `VVEVSVVNKCECMZQQENCCTCMQQQ■■TNENMO■00TK...` |
| exchange-easy | `CharacterSettled` | 29 | 0 | 7 | no | `VVVCQCQIM,CALLNT0CETETEEETEEK` |
| tightfist-easy | `CharacterDecoded` | 38 | 1 | 3 | no | `VEVHVVTETEIESTSTTTDDEEETETEISTSTTTKKK■` |
| tightfist-easy | `CharacterSettled` | 15 | 1 | 0 | no | `VVVTESTDETESTK■` |

The last `LeadingEdge` list is empty on all three, because the flush settles it.

**Group C, own transmission, `qsk-preamble.wav`:** own transmit 9.1 s; the guard marked 13 spans,
the first from 1.02 s, the last ending 11.66 s, 9.2 s blocked in all. `CharacterDecoded` 116 in all,
56 before 13 s (the red's own count is 70 of 134 through `BufferedAudioSource`'s chunks; this
printer feeds fiftieths of a second). `CharacterSettled` 54 in all, 30 before 13 s, at 0.19, 0.97,
1.37, 1.86, 2.09, then every half second or so to 10.98, and 11.21, 11.57, 12.03, 12.80.
`LeadingEdge` 49 distinct before 13 s. **Counted characters reaching neither `LeadingEdge` nor
`CharacterSettled`: 0.**

**Group D, share, per seed** (settled transcript, as the harness reads it):

| case | seed | share | wpm fitted | settled | where the misses fall |
|---|---|---|---|---|---|
| #6, 25 wpm bare 18 dB | 7919 | 0.68 | 25 | `■ A CQ DE N0CALL N0CE ■ ■ T` | first character, and the second call's tail |
| #6 | 104729 | 0.89 | 25 | `■ A CQ DE N0CALL N0CALL K` | first character |
| #6 | 15485863 | 0.68 | 25 | `■ A CQ DE N0CALL N0CE ■ E ■EHE` | first character, and the second call's tail |
| #15, 12 wpm run-up 18 dB | 7919 | 0.95 | 12 | `4V CQ CQ DE N0CALL N0CALL ■` | run-up only |
| #15 | 104729 | 0.58 | **23** | `■ AV CQ CQ DE N0CAL TTTT ■ E I I I I E E H S S E S 5 TTMEE E E` | from the first call's end, as dits |
| #15 | 15485863 | 0.11 | **23** | `4V C TTTT ■ E S E I H E E 5 I E S E I I I IEEHSSES5S5 ...` | from the first character, as dits |

### Findings, one per red

- **#24.** Cause: `CwDecoder.cs` 682 to 686 counts every tracker follow as a discontinuity. The only
  follow on #24 is at 1.50 s, 600 to 625 Hz, a 25 Hz move inside the 60 Hz passband
  (`CwProbabilisticDecoder.BandwidthHz`) made before any text. The 12 s hold at 421 to 425 then
  outlasts the 8.6 s signal. Change: a follow is a discontinuity only when the move is at least
  `CwProbabilisticDecoder.BandwidthHz`, the line `ShouldClearWindow` at 198 to 201 already draws,
  whose remark gives the reason: a move inside the passband cannot have put a different sender in
  the window. Whether text was being read is **not** used: the two-station handover arrives at
  text 0.
- **#41.** Same cause at the same line: three 25 Hz follows at 1.5, 9.5 and 16.0 s each restart the
  hold, then the real 100 Hz handover at 25 s holds to the end of the file. Same change, one commit
  with #24 if both go green. The handover is 100 Hz, over the line, so it still resets.
- **#42.** Every counted character reaches `LeadingEdge` (0 reach neither), so decision 5's event
  condition does not hold and the event stays. Cause: `CwDecoder.cs` 590 hands every hop to
  `_probabilistic.Process` whether or not the tracker's guard has blocked it; only
  `DecodingSuspended`, the radio's own report, skips the stream at 501 to 514. Change, receive side,
  `CwTransmitGuard.cs` read and not written: a hop the guard has blocked goes to
  `_probabilistic.Skip` as the suspended arm does. **A candidate for movement, not for green**:
  settled characters at 0.19 and 0.97 s sit before the first guard span and four sit after its last
  span ends at 11.66 s, none of which the guard marks.
- **#43 to #45.** The red's event at `CwReceiverFixtureTests.cs` 203 is `CharacterDecoded`, which
  `CwDecoder.cs` 139 to 142 raises again for every character of the leading edge at every revision,
  so each guess counts: 112, 85 and 38 characters against 36, 29 and 15 settled. On the settled
  transcript none of the three is green: coverage-easy 4 unreadable and 7 strangers in `QRZ?` and
  `DE/`; exchange-easy 7 strangers in `DE N0` and the second call; tightfist-easy one trailing `■`
  after `K`, settled by the flush at `CwProbabilisticStream.cs` 347 (`settleEverything: true`).
  Change 1, decision 4: line 203 reads `CharacterSettled`, predicted to turn none green and so to be
  put back. Change 2, the decoder, a candidate: the flush does not settle a trailing character that
  is unreadable, aimed at #45's one placeholder. Coverage and exchange: **no cause found at a
  line**; the misses are scattered through the middle of the message on a 15 dB signal, and the
  trace rules out acquisition (the run-up reads `VVV` on all three) and the flush.
- **#6.** Misses at the first character on every seed (`Q` read as `A` after an unreadable, on a
  bare start) and at the second call's tail on two seeds. The first-character miss is acquisition on
  a bare start; the tail is a scattered miss. **No cause found at a line.**
- **#15.** Two of three seeds fit **23 wpm to a 12 wpm sender**, about twice the speed, and read the
  message as dits (`TTTT`, `EEE`, `IIII`); the third fits 12 and reads 0.95. Cause, a candidate: the
  doubled speed hypothesis wins in `CwProbabilisticDecoder`'s speed grid (`SlowestWpm` 8 at 438,
  `FastestWpm` 40 at 459), where every 12 wpm element can be re-read as two at 24. **No single line
  found.**

## 3. The attacks

Every change was built once, then run one type per invocation, `--no-build`. Before group A's
build, the four compiled types that the task 0 grep found reading `WordsPerMinute` or
`SpeedIsReacquiring` were run on the unchanged binary as their baseline: `CapturedSignalTests` 13 of
13, `CwSpeedSilenceTests` 4 of 4, `WhyTheGateDidNotFireTests` 2 of 2, `CwTwoStationTests` 5 of 5.

| change | file and line | red's own number | gate | kept |
|---|---|---|---|---|
| A1, a follow under the full passband, 60 Hz, is not a discontinuity | `CwDecoder.cs` 691 | #24 none to 18, green; #41 green | every type as at entry, every capture row identical; but `CwSpeedSilenceTests.OneStationsSpeedIsStillNamed` printed `named: 12, 21` against `named: 12` at entry. The printer found a 50 Hz follow on `exchange-easy` at 15.25 s naming 21 for a 12 wpm sender | no, replaced by A2 before any commit, because it names a speed no character supports (§0.0, HM-DEC-090) |
| A2, the same at half the passband, 30 Hz | `CwDecoder.cs` 691 | #24 18 wpm, green; #41 82 of 144 polls at 10 and 11, green | captures 37 of 37 every row identical; adjudicated 13 of 13, identical but the time; synthetics 2 of 2; `CwFixtureTests` 22 of 23 as at entry; displacement 6 of 6; `CwEmissionGateTests` 8 of 8; `CwAdjudicationTests` 11 of 11; `CwAcquisitionWindowTests` 10 of 12 and `CwReceiverFixtureTests` 23 of 27 as at entry, every share and count identical; `CapturedSignalTests` 13, `CwSpeedSilenceTests` 4 (`exchange-easy` names 12 only; two-station 10, 11), `WhyTheGateDidNotFireTests` 2, `CwTwoStationTests` 5 (final speed none, reading 22) | **yes, `7e65aac4`** |
| B1, line 203 reads `CharacterSettled` | `CwReceiverFixtureTests.cs` 203 | #43 5+37 to 4+7; #44 3+21 to 0+7; #45 1+3 to 1+0; none green | own type only (decision 4) | no, put back |
| B2, the flush does not settle a trailing unreadable character inside the delay | `CwProbabilisticStream.cs` 507 | #45 1+3 to 1+3; #43, #44 unchanged | own type only, measured for movement; it could not turn #45 green on `CharacterDecoded`, which keeps 3 strangers | no, put back, no movement |
| C, a hop the guard blocks goes to `Skip` | `CwDecoder.cs` 590 | #42 70 to 0, green, 61 in all | **captures 35 of 37**: `cw-2026-08-17-013347` 59 to 48 characters, 108 to 68 elements; `013622` 55 to 13 and 84 to 20, both red; **adjudicated 12 of 13**, `VA3VRR` red; the rest as at entry | no, put back and the tree rebuilt at `7e65aac4` |
| D, #6 and #15 | - | - | - | not attacked: decision 7 attacks them only on a cause the trace names at a line, and it named none |

## 4. Exit

| line or type | entry | exit |
|---|---|---|
| app carry-forward line | 278 of 278 in 156 s, unit 401's exit | 277 of 278 in 161 s and 275 of 278 in 171 s. Every loss was a different name, lost to the headless dispatcher loop at 1 ms and green in the other run, so each counts neither way: 278 of 278 |
| engine carry-forward line | 178 of 178 in 374 s, unit 401's exit | 178 of 178 in 371 s |
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37 | 37 of 37, every row identical to entry (diff rc 0), 92 s |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 | 13 of 13, identical but the time, 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 2 of 2 | 2 of 2 |
| `CwAcquisitionWindowTests` | 10 of 12, #6 0.75, #15 0.54 | 10 of 12, every share identical |
| `CwEmissionGateTests` | 7 of 8 | **8 of 8**, #24 names 18 |
| `CwAdjudicationTests` | 10 of 11 | **11 of 11**, #41 names 10 and 11 |
| `CwReceiverFixtureTests` | 23 of 27 | 23 of 27, every count identical: #42 70, #43 5+37, #44 3+21, #45 1+3 |
| `CwFixtureTests` | 22 of 23 | 22 of 23, the same `fading-18wpm` case |
| `CwDisplacementFloorTests` | 6 of 6 | 6 of 6 |

The eleven transmit files show nothing against `7e209cb4`. `git diff --stat 162f0259 HEAD -- src`
lists only `src/Hamlet.RadioEngine/Cw/CwDecoder.cs`, 21 insertions and 2 deletions, and nothing
under `src/Hamlet.App`. Against `162f0259`, `tests` and `docs` list `PARKED.md`, `reds-3.6.md`,
this file, the failing set's line 52, and the printer `TheEightRedsTests.cs`.
`git status --short tests` printed nothing. **No regression. 3.5 is re-confirmed on its own
sentence.** 3.6 is not ticked: 2 of 8 have a verdict.

Every put-back went through `.run-unit/unit402-putback.sh` (`git checkout -- <path>`), followed by a
build. `git diff --stat HEAD -- src tests` printed nothing after group C was put back.
