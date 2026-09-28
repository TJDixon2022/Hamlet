# Brief: move preamp tests to R98 (work instruction 486) - edit tests only

Repository C:\Source\HamLet. You edit ONLY the test files named in your task. Do not run dotnet,
do not build, do not use git, do not touch anything under src/ or data/.

## What changed, and why these tests now fail

`data/bands/mode-receiver-conditions.json`, the CW row's preamp condition, was changed by the
owner's ruling R98 (HM-DEC-191, 2026-09-28): *"I still hate the preamp crap."*

- BEFORE (HM-DEC-177): CW preamp `condition: "band"` with `bands` (1.8-29.999 MHz -> 1, 50-54 MHz -> 2),
  `whenOverloading: 0`, wanted 1, `wantedText` citing IC-7300_ENG_FM_12b page 4-3.
- NOW (R98): CW preamp is a plain constant: `"wanted": 0`, `"wantedText": "off"`, `"confirmed": true`,
  no `condition`, no `bands`, no `whenOverloading`. Its `says` is "the owner has ruled it off for
  Morse"; its `because` cites R98 and HM-DEC-191 and keeps the manual page (IC-7300_ENG_FM_12b,
  page 4-3) as history.
- `CW DX` and `QRP` are `sameAs: "CW"`, so they get the same. FT8/FT4 rows do NOT state the preamp
  at all (unchanged).
- Consequence: no row in the file carries a preamp overload rule now, so
  `ReceiverSetup.FollowOverloadAsync` follows nothing on any real row (its code is unchanged and
  still works on a condition that has `WhenOverloading` and `Bands`).
- Also changed in `src/Hamlet.RadioEngine/Rig/ReceiverSetup.cs` ApplyAsync: the operator's-hand
  check (`memory.MovedByHandSince`) now runs BEFORE the "found already right" branch. This alone
  broke no test.

`CW_REQUIREMENTS.md`, `CLAUDE.md` and `DECISIONS.md` (HM-DEC-177 and the new HM-DEC-191) are background.

## What to do

For each failing test in your files (the failures are listed in
`.run-unit/unit486-failures.txt`), rewrite it so it states R98's behaviour:

- CW, CW DX and QRP tune-ins want preamp OFF (0) on every band, including 50 MHz.
- A radio found at 0 is left alone (nothing written). A radio at 1 or 2 is written to 0.
- The operator's own change is still respected exactly as before (HM-DEC-056): where a test set
  the preamp by hand and checked it survived, keep that meaning with the new values (for example he
  sets it ON by hand after Hamlet set it off, and a later tune-in of the same mode leaves it on).
- Band change still re-arms (HM-DEC-056 unchanged).
- Tests about the overload follow: the follow no longer has a real row to follow. Where the test's
  PURPOSE is the follow mechanism, keep it by building a preamp `ReceiverCondition` in the test
  itself with `Bands` and `WhenOverloading`, the way the old CW row had them (read
  `src/Hamlet.RadioEngine/Explore/ReceiverConditions.cs` for the record's shape). Add one test
  per file where it makes sense that asserts the real CW row has no overload rule, so nothing
  follows in CW.
- Tests whose purpose was "the value comes from the manual" become "the value comes from the
  owner's ruling": assert the row says off, is confirmed, and its text cites R98 and keeps the
  manual page.

Rules:
- Keep each test's intent and every assertion unrelated to the preamp exactly as it is.
- Do not delete a test to make it pass. Rename a test (method name and remarks) where its old name
  states the old rule (for example `OnEveryHfBandItIsPreamp1` -> `OnEveryHfBandItIsOff`).
- Update the `<remarks>` / comments you touch to say R98 (work instruction 486) and why.
- Keep the file compiling: same usings style, no unused variables (warnings are errors;
  xUnit analyzers are on, e.g. use Assert.Contains/DoesNotContain rather than Assert.NotEmpty on
  a Where).
- American spelling; at most one em dash per paragraph in any string a user reads (none in tests
  is best).

When done, report per file: which tests you changed and how, and anything you could not settle.
