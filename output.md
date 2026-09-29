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

**The changes, file by file** (all in `fd8b917c`):
- **`src/Hamlet.RadioEngine/Bands/W1awMorseFrequencies.cs`:** two additions.
  - `AmateurBandFor(hz)` names the amateur band a frequency is in, from the privileges data's
    Extra allocations, so 160 m, 30 m and 12 m are named as well as the bands Hamlet maps. 75 m is
    joined to 80 m, as `HfBands` joins them.
  - `RowForBandOf(hz)` gives W1AW's row for that band.
  - The table file is untouched, with all nine rows.
- **`src/Hamlet.App/ViewModels/W1awButton.cs`:** `ForDial` makes the one button for the dial.
  Unit 494's `For`, which made the seven, is gone. The hover text is unit 494's.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`:**
  - `W1awHere` replaces the list. It is recomputed from the dial and the license class on every
    privileges update, and raised only when what it says changes.
  - `TuneToW1awCommand` is kept, now with `CanExecute`: a button with nowhere to go cannot be
    pressed.
  - The press path is unit 494's: `TuneTo`, `SetModeAsync` for CW, and the mode-follow hold.
- **`src/Hamlet.App/Views/MainWindow.axaml`:** the "Listen to W1AW" heading and the seven-button row
  are replaced by one button, `W1awButton`, in the same place: the end of the CW tab's send column.
- **Tests.**
  - `W1awButtonsTests` is rewritten for the four cases.
  - The hover registry `EveryControlSaysWhatItDoesTests` lists `W1awButton` and no longer the
    seven. It passes, 3 of 3.
- **Records.**
  - HM-DEC-199 gains a dated note: the owner's one-button ruling, and that the button can't be
    pressed only where W1AW does not send. The ruling's text is unedited.
  - Both outcome copies have `## UNIT 495 - STEP 11`, and both status copies name 495.
  - Version 1.13.181 → 1.13.182.

**What the button says, where.**
- **A band W1AW sends Morse on:** "W1AW on 40 m". A press tunes there and sets CW.
- **An amateur band W1AW does not send Morse on:** "W1AW not on 30 m", and it cannot be pressed.
- **Off the spectrum Hamlet knows, as on 6 m:** "W1AW: not a band Hamlet knows", and it cannot be
  pressed. W1AW does send on 6 m, so "not on 6 m" would be a false label.
- **The license never disables it.** The hover says when sending Morse there is not covered.

**Watched failing first.** At HEAD the window holds seven W1AW buttons, and neither the dial's
button nor `ForDial` exists. So case 4's "exactly one" and every label case are red by absence.

**Verification.**
- The build: 0 warnings, 0 errors.
- **The app carry-forward line: 274 of 278.** The four failures ran in 1 ms each - the Olivia CQ
  label, the PSK31 offer and two PSK31 card names - and all pass alone (12 of 12, 2 of 2, 8 of 8).
- **The app layout, voice, bindings and W1AW types: 88 of 89.** The red is the British spelling
  from 2026-09-26, not this unit.
- **A first combined run that included the text-count test hit its 590 s timeout.** It had already
  reported the CW tab's count (section 3). The rerun without that test finished in 46 s.

## 2. What the owner should expect

1. Rebuild.
2. Go to the CW tab. At the bottom of the Send column there is one button that says which band it
   will take you to: **W1AW on 40 m** while you are on 40 m.
3. Press it, and the radio tunes to W1AW there in CW. Hamlet then leaves the mode alone until you
   change band.
4. Change band, and the button changes with it.
5. On a band W1AW does not send Morse on, such as 30 m, the button says so and cannot be pressed.

## 3. What you should see

| case | what it checked | result |
|---|---|---|
| 1. on 40 m | reads "W1AW on 40 m"; a press asks for 7.0475 MHz and one CW write, no data variant, no setting, nothing keyed | pass |
| 2. on 20 m | tuned from 40 m to 20 m on the same model, it reads "W1AW on 20 m", and a press asks for 14.0475 MHz | pass |
| 3. where W1AW is not | on 30 m it reads "W1AW not on 30 m", and at 50.2 MHz "W1AW: not a band Hamlet knows"; neither can be pressed. A Technician's 20 m button can be pressed and says sending is not covered | pass |
| 4. one button, CW tab only, moving nothing | exactly one W1AW button in the window, on the CW tab only. The send column is 174 px with it and 140 without, inside the receive column's 440, and neither panel moves | pass |

**The CW tab's text count:** 597, against a ceiling of 550. It was 585 before unit 494 and 663
after it, so this unit gives back 66 of unit 494's 78. `HowMuchTheApplicationSaysTests` has been
red since before unit 484 and is not this unit's to fix.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **160 m** is named and offered, but it is not one of Hamlet's mapped bands. A press tunes there
  and sets CW, while the band row and the map stay on the band you came from.
- **6 m needs Hamlet's spectrum to know it.** Then the button offers W1AW on 6 m by itself.
- **The CW tab's text count is 597 against 550**; the ceiling is not this unit's to move.
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

Unit 494's ask - greyed or pressable - is settled by the instruction: the button can't be pressed
only where W1AW does not send, and the license never disables it.
