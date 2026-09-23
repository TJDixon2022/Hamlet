# Unit 399 - the clean synthetics get a band

Work instruction 399, step 3 of *CW decodes again*. The two clean synthetics, `clean-12wpm` and
`clean-18wpm`, measured four ways before anything is touched, then repaired as HM-DEC-127 repaired
their sibling. Every number here is an indication (FACT-004); this machine has no radio (FACT-006).
*Green* means the assertion held and nothing more (`CLAUDE.md` §0.0).

## 1. Entry

HEAD `7345a4f9`. Version 1.13.85 to 1.13.86.

Decision 5's diff, run in a script:

```
git diff --stat 7a297abf HEAD -- src tests docs/carry-forward-tests.txt
```

It printed nothing, so unit 398's exit runs are this unit's entry numbers for the carry-forward
lines: **app 278 of 278 in 160 s, engine 176 of 176 in 374 s** (unit 398 section 3).

The floors at entry, one type per invocation after one `dotnet build Hamlet.sln -warnaserror` of
16 s, then `--no-build`:

| Floor test | Result | Wall |
|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37, every row identical to unit 398's exit | 94 s |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 | 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 0 of 2, as R53 expects | 4 s |

The two synthetics' texts at entry, against `CQ DE W1AW K`:

- `clean-12wpm`: `■ ■ ■ ■ ■  ■ ■ ■ ■■`
- `clean-18wpm`: `■ ■ ■  ■■■`

The fixture-reading types at entry, the regression baseline for decision 6:

- `CwFixtureTests` whole: 14 green, 9 red of 23 in 15 s, as `unit394-reds.md` section 1 has them.
  The cases are listed by name in section 4.
- `TheCleanReadsStayCleanTests`: 6 green, 1 red-open, in 20 s. The red-open one is
  `EachCleanCaptureStillContainsItsTruth` on `cw-2026-08-18-003758` (since unit 398).
- `TheSurveyAlreadyUsesAShortWindowTests`: 2 of 2 in 1 s, `TheDefaultWindowIsThreeSeconds` and
  `ThirtySecondsOfHopsLeavesThreeSecondsOfHistory`.

The eleven transmit files printed nothing against `7e209cb4`. `src` printed nothing against
`5688a8a5`.

## 2. The four ways

Printer: `tests\Hamlet.RadioEngine.Tests\Cw\TheCleanSyntheticsFourWaysTests.cs`, three facts,
asserting nothing, run alone by `--filter "FullyQualifiedName~TheCleanSyntheticsFourWaysTests"`,
3 of 3 in 10 s. Output `.run-unit\unit399-ways.txt`.

`TEXT` is `CwDecodeHarness.Decode`'s text: `CwDecoder.CharacterDecoded`, which is what
`TheCleanRecordingsDecodeExactly` compares with `Sent`. `SETTLED` is `CwDecoder.CharacterSettled`
from a second pass over the same audio. `EXACT` is the floor test's three assertions together.
**`CwConfidence` has three values, `High`, `Low` and `Unreadable`; there is no medium**, so the
tables carry high, low and unreadable (a mismatch with decision 2's list, reported).

### Way 1 - off disk, as the floor test reads them

| Fixture | Band | Text | Settled | wpm | Letters | High | Low | Unreadable | SnrDb | Tone | Exact |
|---|---|---|---|---|---|---|---|---|---|---|---|
| clean-12wpm | none | `■ ■ ■ ■ ■  ■ ■ ■ ■■` | `■■ ■ ■` | 8 | 10 | 0 | 0 | 10 | 85.9 | 600 | no |
| clean-18wpm | none | `■ ■ ■  ■■■` | `■ ■ ■ ■ ■ ■■■■■ A  ■ ■ ■■` | 18 | 6 | 0 | 0 | 6 | 85.9 | 600 | no |

### Way 2 - in memory with a band of 0.02

| Fixture | Band | Text | Settled | wpm | Letters | High | Low | Unreadable | SnrDb | Tone | Exact |
|---|---|---|---|---|---|---|---|---|---|---|---|
| clean-12wpm | 0.02 | `QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK` | `CQ DE W1AW K` | 12 | 29 | 29 | 0 | 0 | 49.6 | 600 | no |
| clean-18wpm | 0.02 | `Q N DEDE E WWAJ11AARW W N K` | `CQ DE W1AW K` | 18 | 20 | 20 | 0 | 0 | 49.7 | 600 | no |

### Way 3 - in memory with bands of 0.01 and 0.04

| Fixture | Band | Text | Settled | wpm | Letters | High | Low | Unreadable | SnrDb | Tone | Exact |
|---|---|---|---|---|---|---|---|---|---|---|---|
| clean-12wpm | 0.01 | `QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK` | `CQ DE W1AW K` | 12 | 29 | 29 | 0 | 0 | 55.5 | 600 | no |
| clean-18wpm | 0.01 | `Q N DEDE E WWAJ11AARW W N K` | `CQ DE W1AW K` | 18 | 20 | 20 | 0 | 0 | 55.7 | 600 | no |
| clean-12wpm | 0.04 | `QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK` | `CQ DE W1AW K` | 12 | 29 | 29 | 0 | 0 | 43.7 | 600 | no |
| clean-18wpm | 0.04 | `Q N DEDE E WWAJ11AARW W N K` | `CQ DE W1AW K` | 18 | 20 | 20 | 0 | 0 | 43.6 | 600 | no |

**What ways 2 and 3 say.** At every band the decoder's settled text is `CQ DE W1AW K` exactly, the
speed is the sent speed, and every letter is high. The harness text is not: `CharacterDecoded`
fires for each leading-edge revision as well as the final character, and the harness appends every
one, so `C` arrives as `QQQ`, `D` as `DDEDE`, and so on. With 29 and 20 characters emitted against
10 in the message, the text can never equal `Sent`. The rows are identical across 0.01, 0.02 and
0.04 in every column but `SnrDb`, so the size of the band is not what stands between the fixture
and the assertion. **The harness's text is**, and it is the same on the band and under way 4 below.

### Way 4 - off disk, with the digital-silence refusal bypassed, uncommitted

The hunk, applied to `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` by
`.run-unit\unit399-way4.sh` and put back by the same script with
`git checkout -- src/Hamlet.RadioEngine/Cw` before anything else happened:

```diff
@@ -1056,8 +1056,19 @@ public static class CwProbabilisticDecoder
         // already records as encoding a physical impossibility.
         if (quarter <= 0)
         {
-            sigma = double.NaN;
-            amplitude = double.NaN;
+            // UNIT 399 WAY 4, UNCOMMITTED: a scale where the quarter point is nought and the 97th
+            // percentile is not, sigma at one hundredth of that percentile, amplitude unchanged.
+            var top = PercentileOf(scratch, take, 97);
+            if (top <= 0)
+            {
+                sigma = double.NaN;
+                amplitude = double.NaN;
+
+                return;
+            }
+
+            sigma = top / 100;
+            amplitude = Math.Max(top, sigma * 1.05);
 
             return;
         }
```

Build 6 s. Then, each `--no-build`, output `.run-unit\unit399-way4-{1,2,3}.txt`:

| Run | Under the hunk | At entry |
|---|---|---|
| `TheCleanRecordingsDecodeExactly` | 0 of 2 in 5 s | 0 of 2 |
| clean-12wpm text | `QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK` | `■ ■ ■ ■ ■  ■ ■ ■ ■■` |
| clean-18wpm text | `Q N DEDE E WWAJ11AARW W N K` | `■ ■ ■  ■■■` |
| Captures type | 37 of 37 in 92 s, **one row moved** | 37 of 37 |
| Adjudicated type | 13 of 13 in 29 s, every printed line identical | 13 of 13 |

The one capture row that moved, of 37:

| Capture | Characters | Elements | Unsure | Tone |
|---|---|---|---|---|
| `unadjudicated/cw-2026-08-23-001520` at entry | 5 | 45 | 4 | 600 |
| `unadjudicated/cw-2026-08-23-001520` under the hunk | 41 | 78 | 0 | 600 |

The other 36 rows are identical. `001520` is the capture the `Estimate` comment names: returning a
floor where there is no noise *put `cw-2026-08-23-001520` at ten to the sixteenth and let three
seconds of an all-zero buffer emit characters*. Under this hunk it emits 36 more characters than its
floor, none unsure, and the captures type's floor of at least 5 does not catch that. **Way 4 cost 0
captures and 0 anchors by the floor tests' pass or fail, and moved 1 capture, `001520`, from 5
characters to 41 with none unsure.** It did not turn either synthetic green: under the hunk the
files give the same text the band gives, and the assertion fails on the harness's revisions the
same way.

After the put-back the script printed `git diff --stat 5688a8a5 HEAD -- src` and
`git status --short -- src`, both empty, and the restored tree was built again in 6 s.

## 3. The band - none chosen, nothing regenerated

Decision 3 wants the smallest of 0.01, 0.02 and 0.04 at which way 2 or way 3 gives `CQ DE W1AW K`
exactly, the speed within one and every letter high, on both fixtures, judged the way the floor
test judges. **No band of the three did.** At 0.01, 0.02 and 0.04 alike the harness text was
`QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK` on `clean-12wpm` and `Q N DEDE E WWAJ11AARW W N K` on
`clean-18wpm`. Speed and confidence held: 12 and 18 wpm, 29 of 29 and 20 of 20 high. The settled
text was exact on both at all three. So under decision 3, **nothing is regenerated**,
`CwFixtures.cs` and the two `.wav` files are unchanged, the writer fact of decision 10 was never
written, and the unit goes to task 4.

The request lines stand as they were at entry:

```
new CwSignalRequest(Call, WordsPerMinute: 12),
new CwSignalRequest(Call, WordsPerMinute: 18),
```

The drift test was not re-run for a regeneration because there was none. At entry it was green
6 of 6.

**Why no band can meet this assertion today, for the arbiter.** `CwDecodeHarness.Decode` builds its
text from `CwDecoder.CharacterDecoded`, which `CwDecoder.cs` line 266 documents as *the same leading
edge, one character at a time*. Every revision of the leading edge is appended, so any audio the
decoder reads letter by letter comes back with its revisions in the text. `CharacterSettled`, line
269, *a character that is final and will not be revised*, gave `CQ DE W1AW K` on every banded run.
Putting a band under the tone removes the digital-silence refusal, which is what way 1 shows as
`■`. It does not change which event the harness reads. Way 4 shows the same from the decoder's side:
with the refusal bypassed, the files off disk give the same revision-laden text the band gives. Which
of the two moves is not this unit's to choose: the harness, whose text is the assertion's input, or
the fixtures plus the harness. Decision 3 says the decoder route is the next arbiter's, and the same
applies to this. It is section 4 of the report.

## 4. `CwFixtureTests`, the 23 cases, and the two other fixture-reading types

Nothing was regenerated, so there is no *after* for task 2. The task 0 baseline, by name, against
the task 4 exit run in section 6:

| Case | Task 0 |
|---|---|
| `EveryFixtureIsOnDisk` | green |
| `EveryFixtureIsStillTheAudioItWasGeneratedFrom` clean-12wpm | green |
| `EveryFixtureIsStillTheAudioItWasGeneratedFrom` clean-18wpm | green |
| `EveryFixtureIsStillTheAudioItWasGeneratedFrom` prosigns-18wpm | green |
| `EveryFixtureIsStillTheAudioItWasGeneratedFrom` noisy-18wpm | green |
| `EveryFixtureIsStillTheAudioItWasGeneratedFrom` fading-18wpm | green |
| `EveryFixtureIsStillTheAudioItWasGeneratedFrom` interference-18wpm | green |
| `TheCleanRecordingsDecodeExactly` clean-12wpm | red, #31 |
| `TheCleanRecordingsDecodeExactly` clean-18wpm | red, #32 |
| `TheProsignRecordingDecodesItsProsigns` | red, #33 |
| `NothingTheDecoderWasSureOfIsWrong` clean-12wpm | green |
| `NothingTheDecoderWasSureOfIsWrong` clean-18wpm | green |
| `NothingTheDecoderWasSureOfIsWrong` prosigns-18wpm | green |
| `NothingTheDecoderWasSureOfIsWrong` noisy-18wpm | red, outside the set, 394 item 5 |
| `NothingTheDecoderWasSureOfIsWrong` fading-18wpm | red, outside the set, 394 item 5 |
| `NothingTheDecoderWasSureOfIsWrong` interference-18wpm | red, outside the set, 394 item 5 |
| `EveryRecordingGivesBackTheShareItShould` clean-12wpm | red, #25 |
| `EveryRecordingGivesBackTheShareItShould` clean-18wpm | red, #26 |
| `EveryRecordingGivesBackTheShareItShould` prosigns-18wpm | red, #30 |
| `EveryRecordingGivesBackTheShareItShould` noisy-18wpm | green |
| `EveryRecordingGivesBackTheShareItShould` fading-18wpm | green |
| `EveryRecordingGivesBackTheShareItShould` interference-18wpm | green |
| `TheWholeSetStaysSmallEnoughToCommit` | green |

14 green, 9 red. The red numbers are as `unit394-reds.md` sections 1 and 2 have them, and that file
is not edited. `TheCleanReadsStayCleanTests` was 6 green and 1 red-open on `003758`, and
`TheSurveyAlreadyUsesAShortWindowTests` was 2 of 2 (section 1).

## 5. The prosigns fixture - not reached

Task 3 runs only if task 2 committed a regeneration, and task 2 did not. `prosigns-18wpm` was not
measured with a band, nothing was regenerated, and #30 and #33 stay red-open with their numbers in
`unit394-reds.md`. The clock did not decide it. Task 2 finished at 03:25, minute 11 of the unit.

## 6. The exit round

| Run | Entry | Exit |
|---|---|---|
| App carry-forward line | 278 of 278 in 160 s, unit 398's exit, decision 5 | first run 275 of 278 in 162 s, 3 lost to the dispatcher loop at 1 ms each; re-run **278 of 278 in 166 s** |
| Engine carry-forward line | 176 of 176 in 374 s, unit 398's exit, decision 5 | **176 of 176 in 374 s** of 480 |
| Captures type | 37 of 37 in 94 s | **37 of 37 in 92 s**, every row identical to entry |
| Adjudicated type | 13 of 13 in 29 s | **13 of 13 in 29 s**, every printed line identical |
| `TheCleanRecordingsDecodeExactly` | 0 of 2 in 4 s | **0 of 2 in 3 s**, the same two texts |
| `CwFixtureTests` whole | 14 green, 9 red of 23 | 14 green, 9 red, every case by name identical |
| `TheCleanReadsStayCleanTests` | 6 green, 1 red-open on 003758 | identical |
| `TheSurveyAlreadyUsesAShortWindowTests` | 2 of 2 | identical |

The lost names in the first app run were `ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed`,
`TheTestsStayOffTheNetworkTests.ThePlainFixtureTakesGeneralFromTheFixedAnswer` and
`TheTestsStayOffTheNetworkTests.TheLicensedFixtureTakesTheFixedAnswerToo`. Each threw
`InvalidProgramException: You've caused dispatcher loop` from `Dispatcher.ResetForUnitTests`,
not an assertion, and each was green on the re-run, so none counts either way.

**No regression.** Nothing that was green at task 0 or at unit 398's exit is red.

Tree checks at exit: the eleven transmit files printed nothing against `7e209cb4`;
`git diff --stat 5688a8a5 HEAD -- src` printed nothing; `git diff --stat 7345a4f9 HEAD -- tests`
lists only `Cw/TheCleanSyntheticsFourWaysTests.cs`, 107 lines, because nothing was regenerated;
`git status --short tests` printed nothing; `docs/carry-forward-tests.txt`,
`docs/unit239-failing-set.txt` and `docs/cw-retired-tests.txt` are unchanged since `7345a4f9`;
`git worktree list` shows the root and the three preflight trees.
