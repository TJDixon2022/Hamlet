# Unit 394 - the 51 inherited CW reds, run at HEAD by type

Work instruction 394, step 3 criterion 3.1. Every name in `docs\unit239-failing-set.txt` run
against the restored decoder at HEAD, one invocation per type, and classified under R49. Every
result here is an indication (FACT-004); *green* means the assertion held, not that CW works
(`CLAUDE.md` 0.0).

## 1. The runs

Thirteen invocations against `tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj`,
each `--filter "FullyQualifiedName~<Type>"` (the two `Fixtures` types and `CwFixtureTests` by
full namespace), `--no-build` after task 0's engine-line build (decision 5), `timeout 600`,
`--logger "console;verbosity=detailed"`, raw output under `.run-unit\unit394-set-<Type>.txt`,
not committed. The floor type's invocation is task 0's captures run. Wall time is the script's
clock around `dotnet test`. No run died before an assertion; nothing was re-run.

| Type | File added | Cases in the type | Cases in the set | Green | Red | Lost | Wall time |
|---|---|---|---|---|---|---|---|
| `ABlipDoesNotShiftEverythingAfterItTests` | 2026-08-29 | 3 | 1 | - | - | - | not run, excluded from compilation |
| `ARecordingWithKeyingInItIsReadTests` | 2026-08-20 | 5 | 3 | 5 | 0 | 0 | 14 s |
| `CapturedSignalTests` | 2026-08-16 | 13 | 1 | 13 | 0 | 0 | 42 s |
| `CwAcquisitionWindowTests` | 2026-08-18 | 12 | 12 | 10 | 2 | 0 | 18 s |
| `CwDisplacementFloorTests` | 2026-08-18 | 6 | 6 | 0 | 6 | 0 | 13 s |
| `CwEmissionGateTests` | 2026-08-16 | 8 | 1 | 7 | 1 | 0 | 8 s |
| `CwFixtureTests` | 2026-08-14 | 23 | 9 | 14 | 9 | 0 | 14 s |
| `CwLowDutyTests` | 2026-08-16 | 4 | 3 | 4 | 0 | 0 | 21 s |
| `CwRefiningRetuneTests` | 2026-08-18 | 3 | 3 | 3 | 0 | 0 | 6 s |
| `CwSurveyThresholdPinTests` | 2026-08-17 | 3 | 1 | 3 | 0 | 0 | 7 s |
| `Fixtures.CwAdjudicationTests` | 2026-08-17 | 11 | 1 | 10 | 1 | 0 | 13 s |
| `Fixtures.CwReceiverFixtureTests` | 2026-08-17 | 27 | 4 | 23 | 4 | 0 | 14 s |
| `OneDecoderNotTwoTests` | 2026-08-25 | 106 | 3 | 100 | 0 | 0 | 601 s, timed out; split 580 s, timed out |
| `TheCapturesThatDecodeKeepDecodingTests` | before 08-25 | 37 | 2 | 37 | 0 | 0 | 97 s, task 0 |
| `ThePitchCanBeHeldTests` | 2026-08-24 | 5 | 1 | 5 | 0 | 0 | 3 s |

**`OneDecoderNotTwoTests` did not finish inside 600 s** (decision 5). The whole-type run printed
81 green and no red before `timeout` killed it at 601 s, exit 124: all 53 cases of
`ListeningAndFeedingReadTheSame`, the set's three among them, and 28 of 53 of
`TheBufferSizeChangesNothing`. That method was then run alone, split by method, at
`timeout 580` rather than 600, so the call stayed inside the harness's 600 s foreground cap. The
first call, at `timeout 600` plus the status line, passed the cap by about a second and the
harness moved it to the background; it was not polled, and the notice of its end was the only
read. The split run printed 47 green and no red before it too was killed, at 580 s. Across the
two runs 47 of the 53 `TheBufferSizeChangesNothing` cases are green and **six are unmeasured at
the cap**: `cw-2026-08-17-134712`, `unadjudicated/cw-2026-08-23-001831`,
`unadjudicated/cw-2026-08-25-011552`, `unadjudicated/cw-2026-08-25-021410`,
`unadjudicated/cw-2026-08-31-003212`, `unadjudicated/cw-2026-08-31-003408`. **None of the six is
in the set**, so 3.1 does not depend on them. The *Test host process crashed* line in both
outputs is the kill, not HM-OPEN-063.
