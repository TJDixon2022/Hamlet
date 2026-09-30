## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked, and no ruling was added to `PHASE_PLAN.md`.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed.

**The instruction's diagnosis was not what the data held.**
- 7.0475 MHz was not in a PSK31 block drawn 22 kHz low. It was in the 40 m **FT4** block
  (7.047–7.050), cited to WSJT-X's default frequency table.
- WSJT-X's own 40 m FT4 dial frequency is 7.0475, the same frequency W1AW sends Morse on. Two
  published conventions claimed one frequency, and nothing checked the map against
  `w1aw-morse.json`.
- The card's "PSK31" was the Digital tab's pick, shown because the block was digital. It was not a
  PSK31 block.
- The ARRL band plan page does not say "PSK31 at 7.070" or "everything below 7.070 is CW". Its
  40 m lines are `7.040 RTTY/Data DX`, `7.080-7.125 RTTY/Data`, `7.171 SSTV` and
  `7.290 AM calling frequency`.

**The changes, file by file** (all in `1dfb721e`):
- **`data/bands/us-neighborhoods.json`:**
  - Two new sources: `arrl-bandplan` (arrl.org/band-plan, read 2026-09-29) and `w1aw-schedule`.
  - Each band carries an `arrlBandPlan` entry quoting the plan's lines word for word. Ranges are
    drawn as blocks. Single frequencies are listed under `conventions` and not drawn.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`:** the W1AW press holds mode-follow off
  *before* it moves the dial. A band change onto the band holding the W1AW frequency keeps the
  hold. The operator's own band press, or a tune anywhere else, ends it.
- **`src/Hamlet.RadioEngine/Explore/ModeGuide.cs`:** the field guide's 40 m FT4 frequency follows
  the row to 7.04775.
- **`tests/Hamlet.RadioEngine.Tests/Explore/TheBandPlanSaysWhereTheModesAreTests.cs`, new:**
  - The nine-row W1AW check.
  - The band-by-band table against the ARRL ranges.
  - A check that the ARRL source is cited.
- **`tests/Hamlet.App.Tests/ViewModels/TheW1awPressStaysInCwTests.cs`, new:** four cases covering
  the press, the report-back, the hold, and the card.
- **Two existing tests re-pinned:** `ArrivingAnywhereOnTheMapFollowsItTests` and
  `TheGuardSilencesEveryDigitalBlockTests` count the map's rows. Digital rows go 28 → 29 and Morse
  rows 20 → 24, with the reason written beside each assert.
- **Records:**
  - `## UNIT 499 - STEP 11` is in both outcome copies, and both status copies name 499.
  - Version 1.13.185 → 1.13.186.
  - HM-DEC-203, "The band plan is checked against the app's own frequencies", is in
    `DECISIONS.md`.

**The band-by-band table** (Hamlet draws 80, 40, 30, 20, 17, 15 and 10 m; 160, 12 and 6 m are not
drawn by an earlier scope decision):

| band | was | became | why |
|---|---|---|---|
| 80 m | PSK31 3.580–3.583, over W1AW's 3.5815 | PSK31 3.580–3.58125, **W1AW (CW) 3.58125–3.58175**, PSK31 3.58175–3.583 | W1AW's Morse frequency |
| 80 m | open ground 3.583–3.585 | RTTY and data | inside ARRL `3.570-3.600 RTTY/Data` |
| 40 m | RTTY 7.040–7.047 | RTTY 7.040–7.04725 | gives way to W1AW's row |
| 40 m | **FT4 7.047–7.050, over W1AW's 7.0475** | **W1AW (CW) 7.04725–7.04775**, FT4 7.04775–7.050 | W1AW's Morse frequency |
| 40 m | open ground 7.081–7.100 and 7.105–7.125 | RTTY and data | inside ARRL `7.080-7.125 RTTY/Data` |
| 30 m | unchanged | unchanged | already inside the plan's RTTY and packet ranges |
| 20 m | open ground 14.077–14.078 | RTTY and data | inside ARRL `14.070-14.095 RTTY` |
| 17 m | open ground 18.102–18.104 | RTTY and data | inside ARRL `18.100-18.105 RTTY` |
| 15 m | open ground 21.070–21.074, 21.076–21.078, 21.081–21.090 | RTTY and data | inside ARRL `21.070-21.110 RTTY/Data` |
| 10 m | open ground 28.070–28.074 and 28.077–28.078 | RTTY and data | inside ARRL `28.070-28.150 RTTY` |
| 10 m | automatic stations 28.120–**28.180** | 28.120–28.150 | the ARRL gives 28.150–28.190 to CW |
| 10 m | automatic 28.150–28.180, open 28.183–28.190 | CW | ARRL `28.150-28.190 CW` |

- W1AW on 20, 17, 15 and 10 m was already inside Morse blocks.
- The new "RTTY and data" rows carry no mode for mode-follow to write. Hamlet leaves the radio
  alone there.

**What overrode the button's CW, honestly.**
- **The only code that writes USB-D with FIL1** is `FollowTheMapAsync`, at
  `MainWindowViewModel.cs:14521` before this change. It writes because the map called 7.0475 FT4.
- **The only line that can undo the press's hold** is the band-change re-arm in
  `OnSelectedBandChanged`, at `MainWindowViewModel.cs:10487` before this change. It is reached from
  `ApplyRigFrequency` (`:22377`) whenever a frequency report names another band.
- **Before this change the press started mode-follow's settle timer and only then suspended it.**
- **A headless reproduction did not reproduce the override.** The press was made from 40 m, the
  radio reported 7.0475 and CW back, and mode-follow looked again. The result was one CW write and
  the hold intact.
- So I cannot name the exact report that crossed the band on your radio, and no telemetry was read
  to find it.
- Two things now protect against it:
  - The hold is set before the dial moves, and survives a band change onto W1AW's band.
  - The map makes 7.0475 Morse, so even a re-armed mode-follow writes CW there.

**Watched failing first.**
- **The nine-row check** was red on 80 m 3.5815 (PSK31) and 40 m 7.0475 (FT4). It is green after.
  160 m, 6 m and 2 m are on bands the map does not draw.
- **The ARRL table** was red on seven ranges: 80, 40, 20, 17, 15 and 10 m RTTY, plus 10 m's
  28.150–28.190 CW. It is green after.
- **The card test** read `Digital` and now reads `Morse`.
- **The press tests** were never red. See above.

**Verification.**
- **The build:** 0 warnings, 0 errors.
- **Engine map, scan and band tests: 696 of 696.**
- **The new app tests: 4 of 4.**
- **The app carry-forward line: 277 of 278.** `TheStopIsAlwaysOnScreenTests` failed in 1 ms and
  passed alone (5 of 5).
- **The app mode, map and W1AW set:** four reds, none new.
  - `VoiceTests`' British spelling and `NothingButTheModeIsEverWritten` are the known ones.
  - `TheLicenceCardAnswersForTheTabAndNotForMorseAlways` and
    `NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth` are red on HEAD too, checked in a clean
    worktree.
- **A wider app sweep over the map, card, mode and chip tests was stopped** by Claude Code because
  the machine ran low on memory. It produced no results and was not rerun.

## 2. What the owner should expect

1. Rebuild.
2. Press `W1AW on 40 m`.
   - The card says **Morse**, and that your license covers Morse there.
   - The radio goes to CW with the CW filter and **stays in CW** until you change band or mode
     yourself.
3. **Then look at the terminal on W1AW.** This is the first honest look at units 496 to 498 on a
   real signal, because until now the detector was being handed 3 kHz of band.
4. **FT4 on 40 m now tunes to 7.04775 rather than 7.047.** WSJT-X's own figure is 7.0475, which is
   W1AW's. FT4 should still decode there.
5. On the map, W1AW shows as a thin Morse sliver on 80 m and 40 m. The gaps inside the ARRL's data
   ranges now read "RTTY and data" instead of open ground.

## 3. What you should see

| case | before | after |
|---|---|---|
| nine W1AW rows against the map | 3.5815 PSK31 and 7.0475 FT4 wrong; seven right or not drawn | all nine right or not drawn |
| the card at 7.0475 | `40 m 7.048 MHz Digital` | `40 m 7.048 MHz Morse · yours to use`, "Your General license covers Morse here" |
| a W1AW press, then the radio reports back | not reproduced | one mode write, CW, no data variant, hold kept |
| every band against the ARRL ranges | seven ranges off | all thirteen ranges match; W1AW on 80 m and FT4 on 10 m are named exceptions |

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The exact report that undid the hold on your radio is not identified.** If CW still does not
  stick, the telemetry of that press would name it.
- **FT4's 40 m dial is 250 Hz above WSJT-X's**, because WSJT-X's figure is W1AW's. Which one should
  win at 7.0475 is your call. The owner's ruling "Morse at 7.0475" is what was applied.
- **W1AW's rows are 250 Hz either side.** That width is the author's and can be overruled.
- **On 10 m, WSJT-X's FT4 at 28.180 sits inside the ARRL's 28.150–28.190 CW range.** It is kept,
  and the test names it as an exception.
- **The ARRL's 28.200–28.300 beacon range is still drawn as open ground**, apart from the existing
  NCDXF block. Beacons were not given a family.
- **Single ARRL frequencies the file already drew as three-kilohertz blocks were kept** (7.040 RTTY,
  the SSTV and AM spots), as HM-DEC-054's editorial rule has them. New single frequencies such as
  14.230 SSTV are listed, not drawn.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling.
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
  - `TheLicenceCardAnswersForTheTabAndNotForMorseAlways`.
  - `NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth`.
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
