# Unit 400 - the harness reads the transcript

Work instruction 400, step 3 of *CW decodes again*. Every number here is an indication (FACT-004);
this machine has no radio (FACT-006). *Green* means the assertion held and nothing more (§0.0).

## 1. Entry

**Decision 6's diff**, run in `.run-unit/unit400-verify.sh` at task 0:

```
$ git diff --stat 80b1aa3e HEAD -- src tests docs/carry-forward-tests.txt
(nothing)
```

Empty, so decision 6 applies: the carry-forward lines at entry are unit 399's exit runs -
**app 278 of 278 in 166 s** (the re-run), **engine 176 of 176 in 374 s**.

**Tree checks at task 0.** `git diff --stat 5688a8a5 HEAD -- src` printed nothing. The eleven
transmit files printed nothing against `7e209cb4`. `git diff --stat 7e209cb4 HEAD --
src/Hamlet.RadioEngine/Cw` printed 4 files, `CwCharacter.cs`, `CwDecodeReport.cs`, `CwDecoder.cs`,
`CwPitchChoice.cs`.

**The three floor tests at entry**, one build (15 s, warnings as errors), then `--no-build`, one
type per invocation:

| Floor test | Result | Wall |
|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37 | 94 s |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 | 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 0 of 2 | 4 s |

Every captures row is identical to unit 399's exit (`unit400-cmp.sh`, 37 rows each side, diff
empty). The adjudicated type's printed lines are identical to unit 399's exit bar the total-time
line. The two synthetics read, against `CQ DE W1AW K`:

```
clean-12wpm  Actual: "■ ■ ■ ■ ■  ■ ■ ■ ■■"
clean-18wpm  Actual: "■ ■ ■  ■■■"
```

**The fixture-reading types at entry.** `CwFixtureTests` whole, 14 green and 9 red of 23 in 14 s,
exactly as `unit399-synthetics.md` section 4 has them (the 23 cases are tabled in section 3).
`TheCleanReadsStayCleanTests` 6 green and 1 red-open on `003758` in 20 s.
`TheSurveyAlreadyUsesAShortWindowTests` 2 of 2 in 1 s.

**The callers of the harness.** `grep -rl "CwDecodeHarness\.Decode" tests --include=*.cs` lists the
ten files section 5 of the instruction names; none is among the engine csproj's 11
`<Compile Remove>` items (lines 41 to 51). `TheIntegratorBandwidthTable.cs` sits under
`Cw\Fixtures\`, not `Cw\`, and does not call `CwDecodeHarness.Decode` by that name; it reaches the
harness through `CwTwoInOnePassband.Tracked` (line 204, the call at line 208).

| Type | Cases | What it asserts through the harness | Run | Task 0 |
|---|---|---|---|---|
| `CwFixtureTests` | 23 | exact text, speed within one, every letter high; share; no confident mistake; drift guard | yes | 14 green, 9 red |
| `CwAcquisitionWindowTests` | 12 | `Assert.True` on the share of the message read against a floor (lines 94, 115, 142) | yes | 10 green; #6 `AFastFistIsReadWithoutARunUp(25, 0.79)` and #15 `TheSlowEndReadsTheMessage(12, 18)` red |
| `CwSensitivityTests` | 2 | through `CwSensitivity.Sweep`: a threshold exists and sits where it did (lines 40, 41); it goes quiet in noise (line 65) | yes | 1 green; `TheDecoderReadsAsFarDownAsItDidBefore` red, `Assert.NotNull` on the threshold at line 40 |
| `EveryCharacterCarriesItsOwnEvidenceTests` | 3 | letters present, each with its span ratios; none on empty audio | yes | 3 green |
| `WhereAcquisitionPointsTests` | 2 | `Assert.Equal(4, Audible.Length)`; `emitted >= 0` | yes | 2 green |
| `CwRefusalFloorTableTests` | 1 | `Assert.NotEmpty(levels)` at line 140 | yes | 1 green |
| `TheCleanSyntheticsFourWaysTests` | 3 | nothing - unit 399's printer | once at task 1, for the record | - |
| `TheCwBaselineTable` | 1 | `Assert.True(File.Exists(path))` after `File.WriteAllText` at line 128 to an `ANALYSIS-cw-*.md` page at the root | **no** - writes a root page, asserts only that it exists (decision 4) | - |
| `TheTwoStationTable` | 1 | the same, write at line 97 | **no**, the same reason | - |
| `TheIntegratorBandwidthTable` | 1 | the same, write at line 94 | **no**, the same reason, and 362 s | - |
| `TheOperatorIsToldAboutASecondStationTests` | - | builds its own `CwDecoder`; takes only `CwTwoInOnePassband.Audio` and `Alone` | no - does not call the harness | - |

Case times at entry (the console's total): acquisition 17 s, sensitivity 39 s, evidence 6 s, where
21 s, refusal 7 s.

**One red at entry outside the set.** `CwSensitivityTests.TheDecoderReadsAsFarDownAsItDidBefore` is
red at task 0 with `Assert.NotNull() Failure` on the sweep's threshold: its sweep prints right
0.78 and wrong 0.11 at every level from 18 dB down, so no level crosses the bar the threshold is
read from. It is not a name in `docs/unit239-failing-set.txt` and no document under `docs` names it.
It was red before this unit changed anything; it is task 0's baseline for decision 4, and it is
parked as `400 item 1`, blocking no criterion of step 3's that this unit works.
