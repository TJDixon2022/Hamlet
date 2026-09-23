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
