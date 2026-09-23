# Unit 398 - the excluded files, classified and retired under R49

Step 3 of *CW decodes again*. Every result here is an indication (FACT-004); this machine has no
radio (FACT-006). *Green* means the assertion held and nothing more.

## 1. Entry

Decision 4's diff, run in `.run-unit/unit398-verify.sh`:

```
$ git diff --stat 3af36501 HEAD -- src tests docs/carry-forward-tests.txt
(nothing printed)
```

Empty, so decision 4 applies: the carry-forward lines at entry are unit 397's exit runs -
**app 278 of 278 in 167 s, engine 176 of 176 in 374 s** (unit 397 report section 3).

Floors at entry, HEAD `7e2abb7e` plus the version bump, one `dotnet build Hamlet.sln -warnaserror`
(exit 0, 14 s), then `--no-build`, one type per invocation:

| Type | Result | Wall |
|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | 37 of 37 green | 94 s |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 of 13 green | 29 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | 0 of 2, red under R53 | 3 s |

Every capture row is identical to unit 397's exit table (`unit398-cmp.sh` against
`unit397-floors-exit-1.txt`: 37 rows each side, `diff` rc 0). The two synthetics' texts:
`clean-12wpm` `■ ■ ■ ■ ■  ■ ■ ■ ■■`, `clean-18wpm` `■ ■ ■  ■■■`, as unit 397's exit.

The eleven transmit files printed nothing against `7e209cb4`; `git diff --stat 5688a8a5 HEAD -- src`
printed nothing.
