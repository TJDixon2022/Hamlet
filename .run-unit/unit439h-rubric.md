# Traceability rubric - work instruction 439 task 1 (read-only classification)

You classify CW test methods of the Hamlet repository (C:\Source\HamLet) against the
requirements in `CW_REQUIREMENTS.md` at the repository root. Read that file first, in full,
including section V (the verification table: condition, metric, threshold per requirement),
and skim `CW_SPEC.md` sections 5, 9, 10 and 11 for the vocabulary (CH-*, TX-*, INT-*, MET-*).

**You change nothing in the repository.** You write exactly one output file, named in your
task, and nothing else. Do not run `dotnet`, do not build, do not run tests.

## The rule

For each test method, read its code and its doc comment, and decide which requirement ids it
**proves**.

- A test **proves** a requirement when what it asserts is what the requirement states: the
  same behaviour, under a condition that is the requirement's (or a narrower instance of it
  that the verification row accepts), checked against the requirement's own threshold or a
  stricter one. Name every requirement it proves.
- A test that asserts a **mechanism** the requirement does not name (an internal class, a
  constant, a window length, a gate value, a histogram) proves `none`, however useful.
- A test that **prints and asserts nothing** proves `none`.
- A test that is **on the subject of a requirement but measures something other than what
  the requirement states** (a different threshold, a character-count floor instead of the
  requirement's metric, a subset of the required range, a ratchet where the row says
  pass/fail, a different condition) proves `none` - but record the requirement in the
  RELATED column with a short reason. This column is how the report counts "requirements
  with a test that measures something other than what the requirement states".
- **Where it is genuinely unclear, write `unclear`** and one line saying why. Never assign a
  requirement to fill the column. `unclear` is a real answer; a guessed id is a defect.
- Transmit, keying, send, band-plan, licence, rig-control and UI-layout tests are outside the
  specification's boundary (CW_SPEC.md section 2: the spec covers the CW **receive decoder**
  only). They prove `none`; say "outside the receive decoder's boundary" and what they assert.
- Tests in files excluded from compilation (listed below) are still classified from their
  source, and the note starts with `NOT COMPILED`.

Files excluded from compilation by `<Compile Remove>` (they never run):
tests/Hamlet.RadioEngine.Tests/Cw/AMoveStartsTheDecoderFreshTests.cs,
AStationIsABinThatSwingsTests.cs, EveryElementCarriesItsOwnPitchTests.cs,
IsTheHertzABiasOrAFloorTests.cs, NoSenderIsSplitInTwoTests.cs,
TheFirstSecondsAreReadAgainTests.cs, ThePeakAgainstASecondSignalTests.cs,
ThePeakFindsThePitchTheTrackerMissedTests.cs, TheQuietestBinNoLongerWinsTests.cs,
TheReferenceDecoderIsPortedFaithfullyTests.cs, WhereHamletAndTheReferenceDivergeTests.cs
(all under tests/Hamlet.RadioEngine.Tests/Cw/), and
tests/Hamlet.App.Tests/Views/ThePitchControlsAreOffThePanelTests.cs.

Useful anchors (but read the code - do not trust a name):
- HM-REQ-013 / 050 / 080 are pass/fail "decoded whole at 15 dB" bars (V-08). A 15 dB clean
  synthetic that must decode exactly may prove 013 for the profile it uses; a ratchet or a
  share floor does not.
- HM-REQ-011 is MET-INVENTED = 0 (sure insertions + sure substitutions). A test asserting "no
  character emitted" from noise may prove 005; a test asserting nothing invented at a
  handover proves 064.
- HM-REQ-094 is "own transmission is not evidence"; HM-REQ-090 is acquiring any pitch 300-900.
- The captures floor (`TheCapturesThatDecodeKeepDecodingTests`) and the named floors assert
  character counts, which no requirement states: RELATED to V-11's overfitting guard at most,
  not a requirement. V-rules are not requirements; do not put a V-id in PROVES.

## Output format

One line per method, in the same order as your input list, pipe-separated, no header, no
other text:

    file|class|method|PROVES|RELATED|NOTE

- PROVES: space-separated ids like `HM-REQ-013 HM-REQ-050`, or `none`, or `unclear`.
- RELATED: ids with a reason in brackets, e.g. `HM-REQ-090 (only 400-875 Hz, not 300 and 900)`,
  or `-` when none.
- NOTE: for `none` or `unclear`, one line saying what the test DOES assert (or why unclear).
  For a proving test, one short line saying what it asserts. No pipe characters inside a field.

Every input line must produce exactly one output line with the same file, class and method.
Count your output lines against your input before finishing and say the two counts in your
final message, plus the counts of PROVES-some-id, none, and unclear.
