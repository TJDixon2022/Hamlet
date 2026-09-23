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

## 2. The harness, measured before it was committed (task 1)

**What the tree says.** `src/Hamlet.RadioEngine/Cw/CwDecoder.cs`, read and not written: line 114
subscribes the probabilistic stream's `CharacterSettled`, counts it into the HM-DEC-091 counters
(*the counters count what reached the screen*) and raises `CharacterSettled` at line 132; lines 135
to 143 raise `LeadingEdge` and then `CharacterDecoded` once for every character of every leading
edge list, at line 141. Line 266: `CharacterDecoded` is *the same leading edge, one character at a
time*; line 269: `CharacterSettled` is *a character that is final and will not be revised*.

`src/Hamlet.App/ViewModels/MainWindowViewModel.cs` at HEAD, read and not written:

```
11117        _decoder.LeadingEdge += Transcript.OfferEdge;
11118        _decoder.CharacterSettled += Transcript.Settle;
11123        _decoder.CharacterDecoded += _ =>     (sets _lastDecodeUtc and _lastCharacterUtc, nothing else)
```

The comment above them, lines 11106 to 11112: *the settled pass ... is what the transcript keeps*.
At `7e209cb4`, by `git show 7e209cb4:src/Hamlet.App/ViewModels/MainWindowViewModel.cs | grep -n
"CharacterSettled\|CharacterDecoded"` in `.run-unit/unit400-task1.sh`:

```
3251:        _decoder.CharacterSettled += Transcript.Settle;
3256:        _decoder.CharacterDecoded += _ =>
3308:            _decoder.CharacterSettled -= Transcript.Settle;
```

**The hunk**, `tests/Hamlet.RadioEngine.Tests/Cw/CwDecodeHarness.cs`, the only change under `tests`
for the harness - line 71 and the remark decision 3 names:

```diff
+    /// <remarks>
+    /// **THE CHARACTERS ARE THE SETTLED ONES**, because that is the transcript
+    /// the operator reads: `MainWindowViewModel` builds the CW tab's transcript
+    /// from `CharacterSettled`, and did at `7e209cb4`, the evening the decoder
+    /// read on the air. `CharacterDecoded` is the leading edge raised again at
+    /// every revision, which no operator sees as text; collecting it appended
+    /// every version of a letter and counted each guess as a decode. Unit 400,
+    /// R12, HM-DEC-091.
+    /// </remarks>
     public static CwDecodeResult Decode(
         MonoAudio audio,
 ...
-        decoder.CharacterDecoded += characters.Add;
+        decoder.CharacterSettled += characters.Add;
```

The record, the derived lists, the speed and the in-memory overload are unchanged. The build with
it: 0 errors under warnings as errors, 7 s.

**The two synthetics under the corrected harness, off disk, no band** - still 0 of 2, as expected:
the correction alone is not the repair.

```
clean-12wpm  Actual: "■■ ■ ■"                       (entry "■ ■ ■ ■ ■  ■ ■ ■ ■■")
clean-18wpm  Actual: "■ ■ ■ ■ ■ ■■■■■ A  ■ ■ ■■"    (entry "■ ■ ■  ■■■")
```

**Every asserting type, case by case, task 0 against the corrected harness:**

| Type | Task 0 | Corrected harness | Moved |
|---|---|---|---|
| `CwFixtureTests` | 14 green, 9 red | 16 green, 7 red | `NothingTheDecoderWasSureOfIsWrong` on `noisy-18wpm` and on `interference-18wpm` red to green; no case green to red |
| `CwAcquisitionWindowTests` | 10 green, #6 and #15 red | 10 green, #6 and #15 red | nothing |
| `CwSensitivityTests` | 1 green, 1 red | 1 green, 1 red | nothing by case; the sweep's numbers moved, below |
| `EveryCharacterCarriesItsOwnEvidenceTests` | 3 green | 3 green | nothing |
| `WhereAcquisitionPointsTests` | 2 green | 2 green | nothing |
| `CwRefusalFloorTableTests` | 1 green | 1 green | nothing |
| `TheCleanReadsStayCleanTests` | 6 green, 1 red-open on `003758` | the same | nothing |
| `TheSurveyAlreadyUsesAShortWindowTests` | 2 green | 2 green | nothing |

**No case green at task 0 is red under the corrected harness**, so decision 4's first branch holds:
no pin, no `DecodeLeadingEdge`, nothing put back. The two confident-mistakes cases that went green
are not names in the 51-name set; they are `394 item 5`'s cases, reported here as numbers and
licensing nothing. `fading-18wpm`'s confident-mistakes case stays red.

**The sensitivity sweep's numbers**, `CwSensitivityTests.TheDecoderReadsAsFarDownAsItDidBefore`,
red at both ends on `Assert.NotNull` of its threshold. The share right is the same at every level
from 4 dB up; the count emitted fell from 19 or 20 to 9 at every level from 4 dB up, which is the
revisions no longer counted, and the share wrong moved at 0, 1, 2, 3, 8 and 9 dB:

| dB | entry right / wrong / emitted | corrected right / wrong / emitted |
|---|---|---|
| 0 | 0.67 / 0.14 / 26 | 0.67 / 0.22 / 12 |
| 1 | 0.67 / 0.14 / 27 | 0.67 / 0.25 / 13 |
| 2 | 0.53 / 0.31 / 28 | 0.56 / 0.25 / 14 |
| 3 | 0.61 / 0.19 / 26 | 0.53 / 0.31 / 13 |
| 4 | 0.72 / 0.14 / 19 | 0.72 / 0.14 / 9 |
| 5 to 7, 10 to 14, 16 to 18 | 0.78 / 0.11 / 20 | 0.78 / 0.11 / 9 |
| 8, 9 | 0.72 / 0.14 / 19 | 0.72 / 0.17 / 9 |
| 15 | 0.75 / 0.11 / 19 | 0.75 / 0.11 / 9 |

**Unit 399's printer, once for the record** (`TheCleanSyntheticsFourWaysTests`, 3 of 3 in 8 s,
asserting nothing). `TEXT` is now the harness and `SETTLED` the printer's own pass; they agree on
every row:

| Way | Fixture | Band | TEXT | SETTLED | wpm | letters | high | exact |
|---|---|---|---|---|---|---|---|---|
| 1 | clean-12wpm | disk | `■■ ■ ■` | the same | 8 | 4 | 0 | no |
| 1 | clean-18wpm | disk | `■ ■ ■ ■ ■ ■■■■■ A  ■ ■ ■■` | the same | 18 | 15 | 1 | no |
| 2 | clean-12wpm | 0.02 | `CQ DE W1AW K` | the same | 12 | 9 | 9 | yes |
| 2 | clean-18wpm | 0.02 | `CQ DE W1AW K` | the same | 18 | 9 | 9 | yes |
| 3 | clean-12wpm | 0.01 | `CQ DE W1AW K` | the same | 12 | 9 | 9 | yes |
| 3 | clean-18wpm | 0.01 | `CQ DE W1AW K` | the same | 18 | 9 | 9 | yes |
| 3 | clean-12wpm | 0.04 | `CQ DE W1AW K` | the same | 12 | 9 | 9 | yes |
| 3 | clean-18wpm | 0.04 | `CQ DE W1AW K` | the same | 18 | 9 | 9 | yes |

## 3. The two clean fixtures get their band (task 2)

**The band: 0.02**, the first tried under decision 7, because it gave `CQ DE W1AW K` exactly off
disk on both regenerated files through the corrected harness, the speed within one and every
letter high; so 0.04 and 0.01 were not tried as regenerations. It is the band unit 399 measured
exact in memory and the band `fading-18wpm` already carries.

**The request lines**, `tests/Hamlet.RadioEngine.Tests/Cw/CwFixtures.cs`:

```diff
-            new CwSignalRequest(Call, WordsPerMinute: 12),
+            new CwSignalRequest(Call, WordsPerMinute: 12, NoiseAmplitude: 0.02),
 ...
-            new CwSignalRequest(Call, WordsPerMinute: 18),
+            new CwSignalRequest(Call, WordsPerMinute: 18, NoiseAmplitude: 0.02),
```

`Clean: true` stays; its doc comment gains *A band under the tone is the air, not an impairment
(HM-DEC-127).* The comment above the two fixtures gains three lines naming unit 399's measurement,
this unit, HM-OPEN-018 and HM-DEC-127. `Sent`, speed, `ReadableShare` and every assertion are
unchanged.

**The regeneration.** A temporary `Cw/Unit400WriteTheCleanFixtures.cs`, one `[Fact]` writing only
the two files through `WavAudio.Write(CwFixtures.PathOf(fixture), CwSignal.Generate(fixture.Request))`,
run once by `--filter "FullyQualifiedName~Unit400WriteTheCleanFixtures"`, 1 of 1 in 1 s;
`git status --short tests/fixtures/cw` then listed exactly `clean-12wpm.wav` and `clean-18wpm.wav`
(200044 and 137644 bytes, the same sizes as before); the writer was deleted with `git rm` in the
script and the project rebuilt. `CwFixtures.WriteAll` was not called.

**One lost attempt, said plainly.** The first writer file lacked `using Xunit;`; its build failed,
the script went on without stopping, the writer never ran, and a `CwFixtureTests` run judged the
**old** files against the **new** requests (14 green, 9 red, the drift guard red on both clean
names, the texts as at task 1). Nothing was committed from it and it judges no band. The writer
was rewritten with the `using`, the script made to stop on a failed build or writer, and the whole
of step 2 run again; the numbers below are that run's.

**The drift guard** `EveryFixtureIsStillTheAudioItWasGeneratedFrom`: 6 of 6.

**The 23 cases of `CwFixtureTests`**, task 0, task 1 (corrected harness) and after the regeneration:

| Case | Task 0 | Task 1 | Band 0.02 |
|---|---|---|---|
| `EveryFixtureIsOnDisk` | green | green | green |
| `EveryFixtureIsStillTheAudioItWasGeneratedFrom` clean-12wpm | green | green | green |
| ... clean-18wpm | green | green | green |
| ... fading-18wpm | green | green | green |
| ... interference-18wpm | green | green | green |
| ... noisy-18wpm | green | green | green |
| ... prosigns-18wpm | green | green | green |
| `EveryRecordingGivesBackTheShareItShould` clean-12wpm (#25) | red | red | **green** |
| ... clean-18wpm (#26) | red | red | **green** |
| ... fading-18wpm | green | green | green |
| ... interference-18wpm | green | green | green |
| ... noisy-18wpm | green | green | green |
| ... prosigns-18wpm (#30) | red | red | red, *gave back 1 of 16* |
| `NothingTheDecoderWasSureOfIsWrong` clean-12wpm | green | green | green |
| ... clean-18wpm | green | green | green |
| ... fading-18wpm | red | red | red |
| ... interference-18wpm | red | green | green |
| ... noisy-18wpm | red | green | green |
| ... prosigns-18wpm | green | green | green |
| `TheCleanRecordingsDecodeExactly` clean-12wpm (#31) | red | red | **green** |
| ... clean-18wpm (#32) | red | red | **green** |
| `TheProsignRecordingDecodesItsProsigns` (#33) | red | red | red |
| `TheWholeSetStaysSmallEnoughToCommit` | green | green | green |
| **Total** | 14 green, 9 red | 16 green, 7 red | **20 green, 3 red** |

**Decision 8, on the regenerated files:** the four cases of each clean name green in one run;
captures 37 of 37 in 93 s with every row identical to entry (`unit400-cmp.sh`, diff empty);
adjudicated 13 of 13 in 30 s, its printed lines identical to entry bar the total-time line;
`TheCleanReadsStayCleanTests` 6 of 7, the red `003758` as at task 0 and task 1;
`TheSurveyAlreadyUsesAShortWindowTests` 2 of 2; both identical by case to task 1. **Decision 8
holds; the fixtures are kept.**

**The caller types were not re-run here.** None of the five, nor `CwSensitivity.cs`, names
`clean-12wpm`, `clean-18wpm` or `CwFixtures.` (grep, empty). Under `tests` the two names and
`CwFixtures.All` are read by `CwFixtures.cs`, `CwFixtureTests.cs`, `TheCleanSyntheticsFourWaysTests.cs`
(the printer, asserting nothing), `Fixtures/CwFixtureGenerator.cs` and
`EveryElementCarriesItsOwnPitchTests.cs`, the last excluded from compilation by the csproj's line 43.

**The set.** #25, #26, #31 and #32 green: 31 green and 20 red-open become **35 green and 16
red-open**.

**The repair commit is `7d1ffde6`.** Its first `git add` named the deleted writer, which had never
been tracked, so git refused the whole add and nothing was committed; the commit was made again
without that path. `PHASE_OUTCOME.md`'s task 2 line was appended once.

## 4. The prosigns fixture (task 3)

Started at 04:21, minute 16 of the unit, inside decision 13's clock.

**In memory first**, through a sibling printer `Cw/TheProsignsFixtureAtABandTests.cs` (one fact,
asserting nothing, kept and committed as task 3's printer; `TheCleanSyntheticsFourWaysTests` was
not edited), 1 of 1 in 5 s:

| Band | Text | `<BT>` | `<SK>` | Confident mistakes | Share | High | wpm |
|---|---|---|---|---|---|---|---|
| disk | `■ ■■■ A ■■ ■ ■■■ ■■ ■ ■■■ ■■■■ ■■■ ■■■■■` | no | no | 0 | 1 of 16 | 1 of 29 | 8 |
| 0.02 | `W1AW DE K2ABC <BT> R TU <SK>` | yes | yes | 0 | 16 of 16 | 16 of 16 | 18 |

All three held at 0.02, so the file was regenerated the same way: the request line gained
`NoiseAmplitude: 0.02` and a one-line comment naming unit 400, HM-OPEN-018 and HM-DEC-127; a
temporary `Cw/Unit400WriteTheProsignsFixture.cs` wrote `prosigns-18wpm.wav` only, 1 of 1;
`git status --short tests/fixtures/cw` listed exactly that file. The writer had never been tracked,
so it was deleted with `rm -f` inside the script rather than `git rm`, which would have refused
it, as task 2's commit found. Then the project was rebuilt.

**Decision 8, with `TheProsignRecordingDecodesItsProsigns` in place of the exact-text case.**
`CwFixtureTests` whole: **22 green, 1 red** in 5 s - `TheProsignRecordingDecodesItsProsigns` (#33),
`EveryRecordingGivesBackTheShareItShould` on `prosigns-18wpm` (#30), `NothingTheDecoderWasSureOfIsWrong`
on `prosigns-18wpm` and the drift guard on `prosigns-18wpm` all green; the one red is
`NothingTheDecoderWasSureOfIsWrong` on `fading-18wpm`, red at task 0, task 1 and task 2. Captures
37 of 37 in 92 s, every row identical to entry; adjudicated 13 of 13 in 30 s, identical to entry
bar the total-time line; `TheCleanReadsStayCleanTests` 6 of 7 and `TheSurveyAlreadyUsesAShortWindowTests`
2 of 2, identical by case to task 2. **Decision 8 holds; the fixture is kept.**

**The set.** #30 and #33 green: **37 green and 14 red-open**.

## 5. The exit round (task 4)

**The carry-forward lines**, as `docs/carry-forward-tests.txt` prints them, one build each, a status
line before each:

| Line | Entry (unit 399's exit, decision 6) | Exit |
|---|---|---|
| app | 278 of 278 in 166 s | 277 of 278 in 161 s, then 277 of 278 in 166 s on the one re-run |
| engine | 176 of 176 in 374 s | **176 of 176 in 374 s** of 480 |

The app line's two losses are the headless dispatcher loop, `System.InvalidProgramException : You've
caused dispatcher loop` in `Avalonia.Threading.Dispatcher.ResetForUnitTests`, 1 ms each, before any
assertion: the first run lost `TheRecordNamesTheSubModePressedTests.TheCqPressWritesTheLabelTheOperatorPressed(label: "Olivia")`,
the re-run lost `TheTestsStayOffTheNetworkTests.ThePlainFixtureTakesGeneralFromTheFixedAnswer`, and
neither name failed in the other run. Under section 1 of the instruction a name lost in one run and
green in the other is neither: **no red on an assertion on either line, no regression**. This is the
shape step 1's 1.5 was ticked on.

**The three floor tests at exit**, `--no-build` after the engine line's build:

| Floor test | Entry | Exit |
|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37 in 94 s | 37 of 37 in 92 s, every row identical to entry |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 in 29 s | 13 of 13 in 29 s, identical to entry bar the total-time line |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 0 of 2 in 4 s | **2 of 2 in 1 s** |

**The other types at exit**, each against its last measurement: `CwFixtureTests` 22 green and 1 red
(`NothingTheDecoderWasSureOfIsWrong` on `fading-18wpm`), identical by case to task 3;
`TheCleanReadsStayCleanTests` 6 of 7 and `TheSurveyAlreadyUsesAShortWindowTests` 2 of 2, identical
by case to task 0; `CwAcquisitionWindowTests` 10 of 12, `CwSensitivityTests` 1 of 2,
`EveryCharacterCarriesItsOwnEvidenceTests` 3 of 3, `WhereAcquisitionPointsTests` 2 of 2,
`CwRefusalFloorTableTests` 1 of 1, each identical by case to task 0 and task 1.

**Tree checks at exit.** The eleven transmit files: nothing against `7e209cb4`.
`git diff --stat 5688a8a5 HEAD -- src`: nothing. `git diff --stat 0aa08d32 HEAD -- tests`:

```
 .../Hamlet.RadioEngine.Tests/Cw/CwDecodeHarness.cs |  11 ++++-
 tests/Hamlet.RadioEngine.Tests/Cw/CwFixtures.cs    |  16 ++++--
 .../Cw/TheProsignsFixtureAtABandTests.cs           |  55 +++++++++++++++++++++
 tests/fixtures/cw/clean-12wpm.wav                  | Bin 200044 -> 200044 bytes
 tests/fixtures/cw/clean-18wpm.wav                  | Bin 137644 -> 137644 bytes
 tests/fixtures/cw/prosigns-18wpm.wav               | Bin 237910 -> 237910 bytes
 6 files changed, 76 insertions(+), 6 deletions(-)
```

The harness, the requests, task 3's printer and the three regenerated files, nothing else, no pinned
caller. `git status --short tests`: nothing. No `ANALYSIS-cw-*.md` page modified or untracked at the
root. `docs/carry-forward-tests.txt`, `docs/unit239-failing-set.txt` and `docs/cw-retired-tests.txt`:
nothing against `0aa08d32`. `git worktree list`: the root and the three preflight trees.
