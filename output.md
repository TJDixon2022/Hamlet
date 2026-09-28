READ IN THIS ORDER.

A. The phase goal and the steps: steps 0, 1 done; 2 closed partial; 3 to 10
   closed under R88; 11 at 3 of 6 (11.1 and 11.2 retired); 12 at 0 of 5,
   12.1 to 12.3 unticked by the owner, 12.2 waiting on his eye.
B. Step 11, criterion 11.4: 0 owner_verdict rows read (0 agree, 0 idiot);
   the gate named is none, moved: not moved, on the rows at: none; agree rows
   flipped: 0; test not written. The telemetry folder is outside what this
   session is allowed to read.
C. The rest. Section 4 raises 2 items; the first is in the way of 11.4 in B.
   No recording was read; the telemetry file was neither read nor written.

UNIT:       483 - stopped at task 1 of 3, none dropped (the stop in task 1 held, not the drop candidate) - 2026-09-28 16:06
PHASE GOAL: Hamlet meets the CW requirements. What is left is the owner's ear teaching the detector (11.4), and the oscilloscope he has to judge (step 12).
UNIT GOAL:  Read the owner's verdict rows from his telemetry file and replay them against the detector's gates as they stand now. Name the one gate his "You're an idiot" presses contradict without flipping an "I agree" press, and move it by one number that one row's own figure gives.
ADVANCED:   no - the rows could not be read: the file tools and the shell are confined to C:\Source\HamLet, and the telemetry folder is under C:\Users\TimDi\AppData\Roaming
NUMBER:     idiot rows the gate contradicted at HEAD: unmeasured -> unmeasured (0 rows read); agree rows flipped: 0; recordings read: 0
DRIFT:      1 consecutive unit without advance (was 0)

## 1. What Claude did

**Stopped at task 1 of 3.** Task 1 has a stop for exactly this case: "If the folder is not readable from the session, say so in section 4 and stop at task 1." It held. Tasks 2 (the move, its test and its remark) and 3 (the exit round and the tick) were not run. That is the instruction's own stop, not a sizing decision. Neither task has anything to work on without rows.

Provenance: host QUIVERFULL, project Hamlet confirmed by the §0 gate (three files present, CoreHMI.sln and MURC.sln absent, root C:\Source\HamLet), branch main. SESSION.lock was present at the start and is left as found.

- **Task 0** (`07a43e9e`, pushed):
  - `PHASE_OUTCOME.md` (both copies) gained `## UNIT 483 - STEP 11` from the ARBITER-DECISION block.
  - `PHASE_STATUS.md` (both copies) now names 483 with CURRENT_STEP 11. The launcher had written 3 again. The HEARTBEAT line is carried as found.
  - The version moved from 1.13.169 to 1.13.170.
  - Entry round:
    - The build had 0 warnings and 0 errors.
    - The app carry-forward line went 276/278 on the first run. Both failures were "You've caused dispatcher loop", in TheChipSaysTheChosenModeTests for FT8 and Olivia. The re-run went 278/278, and neither run is counted (HM-DEC-155).
    - TheOwnersVerdictIsARowTests 7/7, TheVerdictCarriesTheScopeTests 4/4 and TheOwnersPressLandsInTheFileTests 1/1, each in its own invocation.
- **Task 1** (`dfb551ce`, pushed): wrote `.run-unit/unit483-replay.txt`.
  - **The rows.** The folder is `C:\Users\TimDi\AppData\Roaming\Hamlet\telemetry`, from `src/Hamlet.App/Settings/AppSettings.cs:757-762` and the user folder named in `docs/unit259-send-path-trace.md:108`.
    - The file reader refused `...\telemetry\2026-09-28.jsonl`: *"is outside C:\Source\HamLet; --restricted confines the file tools to the working directory."*
    - The shell refused a listing of `/c/Users` and a read of `APPDATA`.
    - I tried nothing to get around the restriction.
    - **Rows read: 0.** `.run-unit/unit483-rows.jsonl` is not written, and no `*.jsonl` copy exists in the tree.
  - **The gate list at HEAD** is written anyway, with file, line, value and the row key each gate needs. It is the part of task 1 that does not depend on the rows.
  - **Finding:** most of the gates that decide the light cannot be replayed from the rows 11.4 needs. The light is `CwEnvelopeDetector`'s `keying`, true when any bin makes two bars with a gap between them inside a second.
    - Its gates are:
      - the flat tolerance, `FlatToleranceDb` 1.5 through `ToleranceDb` (`CwEnvelopeDetector.cs:133`, `:144`, `:904`);
      - the shortest bar and gap, `ShortestBarMs` 25 (`:156`, used at `:698` and `:735`);
      - the pairing window, `KeyingSeconds` 1 (`:167`, `:736`);
      - the pair-level and gap-drop checks (`:737`, `:743`);
      - the waves rule, which has no constant (`:761`);
      - the passband (`:273-313`, with WholeBand 100/3000 as the fallback).
    - On a row where the bars said *no keying*, `scopeContrastDb`, `scopeFloorDb` and `scopeThresholdDb` are all null, and the row carries no bar length and no passband width. So an idiot row with the bars dark names a bar-side gate only by proxy.
    - Only the meter's gates replay fully from a row: `KeyingScore`, `SlowestChatterMs`, `LongestElementMs` and `ConfidentSwingDb`. The meter does not drive the light.
    - The survey's gates sit neither between the tone and the bars nor on the row. The row carries only the bins the survey admitted.
  - **Mismatches with the instruction's §2**, reported, not repaired:
    - Its line numbers all match HEAD.
    - Its list missed the passband and WholeBand edges, the two other uses of FlatToleranceDb, the waves rule, the meter's element-length bounds, and four survey constants.
    - Its survey constants are not between the tone and the bars, except ShortestDitMs through ShortestBarMs.
  - **The stop held.** No row was read, so no idiot row disagrees and there is no candidate. Nothing was moved and no move was invented. 11.4 stays unticked.
- **Also found, as §2 foresaw:**
  - PROJECT_STATUS.md RULES_AT says HM-DEC-188 and CLAUDE.md §1 says CPS-DEC-0188. It is left as found.
  - The launcher's root and `.run-unit` files were modified and uncommitted. The launcher had appended an `## UNIT 3 - STEP 11` entry to PHASE_OUTCOME.md, and task 0's commit carried it in with 483's entry, as unit 482's commit did.
  - The launcher again wrote CURRENT_STEP 3. Step 3 is not authored while R88 stands (plan §6 routing).

Decisions I made myself, author's and overrulable:
- I did not try to reach the folder any other way, such as a script that reads outside the working directory. The restriction is the session's permission boundary, and the instruction names the stop for exactly this.
- I did not replay the row figures quoted in the remarks of `CwKeyingMeter.cs:113-134`. They are summaries, not the file, and they come from builds before units 477, 479 and 480 moved the detector.

## 2. What the owner should expect

- **Nothing changed in the app.** None of your presses moved a number this time. Nothing under `src/` changed, and the scope, the bars and the meter behave exactly as in 1.13.169. The only change is the version, now 1.13.170.
- **Why:** this session is only allowed to read inside `C:\Source\HamLet`. Your telemetry lives in `C:\Users\TimDi\AppData\Roaming\Hamlet\telemetry`, so the unit could not see a single one of your presses.
- **What will look wrong but is not:**
  - 11.4 is still unticked after a unit aimed at it. The unit stopped where its instruction said to stop.
  - `.run-unit/unit483-rows.jsonl` does not exist. Nothing was read, so there was nothing to copy.
- **What to press next time, so a gate can be named:**
  - Press "You're an idiot" on a station you can hear while the words say "the bars say no keying".
  - Press "I agree with you" once while the bars say keying on a station, and once while they say no keying on an empty stretch.
  - Use a build from 1.13.167 on, which has unit 480's change.
- **A caveat worth knowing now.** A press made while the bars are dark does not record the bar-side figures: the contrast, the gap level and the bar length. So those presses can name one of the meter's numbers outright, but a bar gate only indirectly.

## 3. What you should see

**The replay table**: no rows, so no row lines. This is the gate list with each gate's replayability. The column for a named gate is empty because no gate was named. The full table is in `.run-unit/unit483-replay.txt`.

| gate | file:line | value at HEAD | row key | replayable from a "no keying" row |
|---|---|---|---|---|
| passband edges / WholeBandLowHz, WholeBandHighHz | CwEnvelopeDetector.cs:273-313, :179, :182 | rig pitch ± width/2; 100 / 3000 Hz | none | no |
| FlatToleranceDb via ToleranceDb (run holds) | CwEnvelopeDetector.cs:133, :144, :904-906 | 1.5 dB floor | scopeContrastDb (null when dark) | no |
| ShortestBarMs (bar and gap length) | CwEnvelopeDetector.cs:156, :233, :698, :735 | 25 ms | none (meterMedianMs is a proxy) | proxy only |
| KeyingSeconds (pair within a second) | CwEnvelopeDetector.cs:167, :234, :736 | 1 s | scopeRunMs when scopeMark false (watched bin) | partly |
| pair at one level / gap dropped | CwEnvelopeDetector.cs:737, :743 | FlatToleranceDb 1.5 | none | no |
| bars clear the gaps' waves | CwEnvelopeDetector.cs:761 | no constant | scopeFloorDb (null when dark) | no, and nothing to move |
| KeyingScore (meter) | CwKeyingMeter.cs:106, :337 | 0.10 | meterScore | yes (meter, not the light) |
| SlowestChatterMs / LongestElementMs (meter) | CwKeyingMeter.cs:89, :96, :338-339 | 25 / 250 ms | meterMedianMs | yes (meter, not the light) |
| ConfidentSwingDb (meter) | CwKeyingMeter.cs:170, :340 | 15 dB | meterSwingDb | yes (meter, not the light) |
| survey: MinimumSeparation, Min/MaximumRatio, Shortest/LongestDitMs, MinimumMarks, InterferenceLiftDb, HysteresisDb, NoiseSeparationHz | CwToneSurvey.cs:153-199 | 4.0, 2.5/3.8, 25/200 ms, 8, 10 dB, 3.0 dB, 125 Hz | survey carries admitted bins only | no, and not on the bars' path |

Candidate table: **none**. There are 0 idiot rows, 0 agree rows and no value taken from a row.

**First failure from the watched-red run:** none. No test was written, because task 2 did not run.

**The constant's new remark:** none. No constant was moved, and `git diff c2480ac1 -- src` prints nothing.

Evidence in the tree:
- `.run-unit/unit483-replay.txt`
- `.run-unit/unit483-build-entry.txt`
- `.run-unit/unit483-cf-app-entry.txt` (276/278, the dispatcher loop) and `-entry2.txt` (278/278)
- `.run-unit/unit483-t-entry-row.txt`, `-scope.txt`, `-482.txt`

## 4. What's blocking us

1. **The loop cannot read the owner's telemetry, so 11.4 cannot be met by any loop unit as things stand.** Blocks 11.4.
   - Ruling needed: how the rows reach a session. The ruling is the owner's, because it widens what a session may read on his machine.
   - Recommendation: before the next 11.4 unit, the owner copies the `owner_verdict` lines of his telemetry files into `.run-unit/unit483-rows.jsonl`, or into whatever name the next instruction gives. The unit then reads the copy, and "read from the telemetry file" is met by his copy.
   - Rejected: adding the telemetry folder to the session's allowed directories. It works, but it gives every future session standing read access to his AppData, which is wider than one criterion needs.
   - Rejected: having a session script its way around the restriction. That defeats the permission boundary the owner set, and no unit should do it.
   - Also rejected: replaying the figures quoted in `CwKeyingMeter.cs` remarks. Those are summaries from builds before the detector changed, not the file.
2. **The rows cannot name a bar-side gate outright, even once they can be read.** Not blocking a criterion today, but it decides whether 11.4 can move a bar gate or only a meter gate.
   - On a row where the bars said no keying, the row has no bin contrast, gap level, bar length or passband width. The bar gates' figures are therefore absent exactly on the idiot rows that would name them.
   - Author's ruling (R85): under 11.4 a unit names a meter gate from such rows directly, and a bar gate only where a proxy key is stated in the replay and the owner's remark accepts it.
   - Rejected: adding bar-side keys to the row within an 11.4 unit. That changes what the row carries, which is 11.3's work. It would be its own unit, and step 11's lines do not ask for it.
