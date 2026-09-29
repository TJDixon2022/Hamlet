## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked (69 before and after), and no ruling was added.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed.

**The changes, file by file** (all in `686f8140`):
- **`data/bands/w1aw-morse.json`**, new: **where the frequency table lives.**
  - All nine of the ARRL's rows, 160 m through 2 m, with the ARRL schedule cited as the source and
    the speeds W1AW sends at.
  - No times, because W1AW's are US Central and a time computed against UTC can be wrong on
    screen.
  - It is beside `us-neighborhoods.json` and `olivia-calling.json`, and embedded in the engine the
    same way.
- **`src/Hamlet.RadioEngine/Bands/W1awMorseFrequencies.cs`**, new: reads the file strictly, in the
  pattern of `OliviaCallingTable`, with no frequency literal in the code.
- **`src/Hamlet.App/ViewModels/W1awButton.cs`**, new: one button per row Hamlet can honestly take
  the dial to. The label is "W1AW 40 m"; the hover gives the frequency, that W1AW is the ARRL's
  headquarters station, what it sends and at what speeds, and that the schedule is the ARRL's and
  the times are not shown here. It adds whether the license covers sending Morse there, from
  `PrivilegeStatusLine`, the same call behind the card's line.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`:**
  - `W1awButtons`, rebuilt only when the license class changes, never per frequency. Buttons
    rebuilt under the pointer go dead (HM-DEC-078).
  - `TuneToW1awCommand`.
- **`src/Hamlet.App/Views/MainWindow.axaml`:** a row, "Listen to W1AW", at the end of the CW tab's
  send column. That column is fixed at 320 wide and nothing sits below the row in it.
- **Tests.**
  - `W1awButtonsTests`, new: the four cases.
  - The hover registry `EveryControlSaysWhatItDoesTests` names the seven new buttons. It fails on
    any control on the CW tab it does not list.
- **Records.**
  - `DECISIONS.md` has HM-DEC-199.
  - Both outcome copies have `## UNIT 494 - STEP 11`, and both status copies name 494.
  - Version 1.13.180 → 1.13.181.

**What a press does.**
- **It tunes through `TuneTo`**, the path every tune button takes, the frequency chips included.
- **It sets CW itself**, with the same `SetModeAsync` write the Olivia tab makes.
- **It holds mode-follow off until the next band change**, exactly as the operator's own hand on
  the mode knob does (HM-DEC-056).

The chips' path alone would not have given CW:
- 7.0475 MHz sits in the map's FT4 block and 3.5815 in its PSK31 block. Mode-follow would set the
  data variant there, or, inside the CW segment, leave the radio in whatever mode it was in.
- 160 m is not a band Hamlet maps, so mode-follow says nothing there at all.

**Two departures from the instruction, stated and overrulable:**
1. **6 m is not offered.** The IC-7300 tunes 6 m, but the spectrum Hamlet knows does not carry it.
   At 50.350 MHz the card would say "not an amateur band", which is false (§0.0). 2 m is left out
   as instructed: the radio cannot tune it.
2. **A band outside the license is pressable, not greyed.** Listening is never restricted
   (HM-DEC-029), and grey is kept for what cannot be used (HM-DEC-087). These buttons only tune the
   receiver, so the hover says when the license does not cover *sending* there. The owner holds
   General, which covers Morse at all seven frequencies.

**Watched failing first.** At HEAD, the row, the button type and the command do not exist, so
every case is red by absence. The probe that shaped the design measured what the app knew at each
frequency beforehand:
- which bands have mode-follow targets, and the two digital blocks;
- 6 m reading as not amateur;
- 160 m being amateur but unmapped.

**Verification.**
- The build: 0 warnings, 0 errors.
- **The app carry-forward line: 276 of 278.** The two failures ran in 1 ms each,
  `TheCarrierHoldsTheButtonsTests` and `TheChipSaysTheChosenModeTests`, and pass alone (8 of 8 and
  6 of 6).
- **The app layout, voice, bindings, registry and W1AW types:** the W1AW cases, the registry and
  `BindingHealthTests` pass. The two top-row trace names that failed in the long run pass alone
  (15 of 15 and 2 of 2). The British spelling red is from 2026-09-26.

## 2. What the owner should expect

1. Rebuild.
2. Go to the CW tab. At the bottom of the Send column is **Listen to W1AW** and a button for each
   band.
3. Press **W1AW 40 m** or **W1AW 20 m**, and the radio tunes there and goes into CW. Hamlet then
   leaves the mode alone until you change band.
4. Which band when:
   - **40 m** after dark.
   - **20 m** in the day.
   - **80 m** late at night.
   - 160 m, 17 m, 15 m and 10 m are there too.
5. The hover says what W1AW sends and how fast. It does not say when: the times are on the ARRL's
   schedule, in US Central time, and W1AW is only on the air at those times.
6. **There is no 6 m button.** Hamlet does not yet know 6 m as amateur spectrum and would have
   called it "not an amateur band".

## 3. What you should see

| case | what it checked | result |
|---|---|---|
| 1. one button per tunable band, no 2 m | W1AW 160 m, 80 m, 40 m, 20 m, 17 m, 15 m, 10 m, and no 2 m (nor 6 m) | pass |
| 2. pressing W1AW 20 m | frequency 14.0475 MHz; one mode write, CW without the data variant; no setting written; nothing keyed | pass |
| 3. a band outside the privileges | a Technician's 20 m button is present and pressable, and says sending Morse is not covered | pass (pressable by HM-DEC-029, see section 1) |
| 4. the CW tab only, moving nothing | row shown on the CW tab, hidden on Digital. With it the send column is 250 px tall, without it 140, and both fit inside the receive column's 440, so neither panel moves | pass |

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **6 m needs Hamlet's spectrum to know it.** Then its W1AW button, 50.350 MHz, appears on its own:
  the row offers every row the spectrum calls amateur.
- **160 m is not one of Hamlet's mapped bands.** A press tunes there and sets CW, but the band row
  and the map stay on the band you came from.
- **`HowMuchTheApplicationSaysTests`, red since before unit 484,** now reads 663 for the CW tab,
  against 585 at HEAD and a ceiling of 550. The row's heading and seven labels are the difference.
- **Two departures from the instruction** (section 1) are overrulable: 6 m left out, and every
  button pressable.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling.
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
  - `ThatPitchIsTheStationsOwn`.
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
- **Unit 494, new, 2026-09-29:** whether a band outside the license should be greyed, as the
  instruction asked, or pressable with its hover saying so, as built under HM-DEC-029 and
  HM-DEC-087. It waits on the owner, and the change sits in `W1awButton.cs` and
  `TuneToW1awCommand` in `686f8140`.
