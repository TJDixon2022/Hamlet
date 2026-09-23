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
