## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked; R111 was appended to both `PHASE_PLAN.md` copies in the owner's words.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed, and nothing keys or transmits.

**The changes, file by file** (all in `585b7df5`):
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`:**
  - **`FollowTheMapAsync` is gone; `FollowTheTabAsync` replaces it.** It writes the mode the tab
    means, once, and then the tab's receive settings.
  - **`TabTarget`:**
    - The CW tab means CW.
    - The Digital tab means USB-D.
    - The Voice tab means the map's voice block (SSB or AM) or, off one, the sideband convention:
      LSB below 10 MHz, USB above.
  - **The receive settings follow the tab.** Each tab takes its row of
    `mode-receiver-conditions.json`, once per tab:
    - CW: the `CW` row.
    - Digital: the `FT8` row, the one data row the file states (`FT4` says the same), with the
      block's "scope span" sentence spoken beside it. That sentence writes nothing.
    - Voice: nothing, because the file states nothing for it.
  - **A tab change re-arms everything:** the mode, the operator's hand on the mode, and the operator's
    hand on the receive settings.
  - **A band change re-arms nothing.** Before this unit it re-armed mode-follow and re-applied the
    block's settings.
  - **A new connection writes the last selected tab's mode and settings once**, after the connected
    line, so the bar shows what the tab set and anything it could not read.
  - **The W1AW button only tunes.** Unit 495's CW write and unit 499's hold are removed; the data file
    is left as it is.
  - **The dwell timer writes nothing.** It still observes the dial, but nothing reads the result now.
  - **The operator's own knob stands the app down until he changes tab** (it said "change band"
    before), and nothing writes it back.
- **`src/Hamlet.RadioEngine/Explore/ReceiverConditions.cs`:** a new `ForTab(mode, block)` gives a
  tab's row plus the block's spoken span line. `ForBlock` now calls it, with the same answer as
  before.

**Every mode write the app can now make, and what causes it:**

| where | writes | caused by |
|---|---|---|
| `FollowTheTabAsync`, CW tab | CW, data off, no filter byte (the radio's own CW filter) | selecting the CW tab, or a radio connecting with it selected; once until the tab changes |
| `FollowTheTabAsync`, Digital tab | USB, data on, filter slot 1 (the widest, "FIL1") | selecting the Digital tab, or a radio connecting with it selected; once until the tab changes |
| `FollowTheTabAsync`, Voice tab | LSB, USB or AM, no filter byte | the Voice tab, whenever the dial settles somewhere its voice mode differs; still stands down on the operator's knob |
| `TuneToOliviaAsync` | USB, data on | the operator pressing an Olivia chip on the Digital tab (unchanged) |

- Nothing else in `src/Hamlet.App` calls `SetModeAsync`, and `TheTabIsTheModeTests.EveryModeWriteIsNamed`
  holds it to these two call sites.
- If "mode follows the map" is switched off in Settings, the CW and Digital tabs write nothing
  either.

**What `FollowTheMapAsync` did and what is dead.**
- It is removed.
- The settle timer, which the dial, the band and the tab all trigger, now calls
  `FollowTheTabAsync`. On the CW and Digital tabs that finds the tab already written and writes
  nothing.
- **Dead in the app, left in the engine** where their own tests still exercise them:
  - `ModeFollowPlan.WaitsForDwell`.
  - `ModeFollowPlan.WorkingCw`.
  - The dwell look's use of its maturity.
  - The map's data targets (USB-D for a digital block).

**Tests:**
- **New: `TheTabIsTheModeTests`**, the five cases below.
- **Re-pinned to R111:**
  - `TheW1awPressStaysInCwTests`: the press writes no mode, the tab's CW is the only write, and a
    band change does not re-arm the mode over his knob.
  - `W1awButtonsTests`: the press writes no mode and holds nothing off.
  - `TheBarSaysWhatHamletCouldNotDoTests`: its fixture is on the Digital tab at 14.074, where the
    FT8 row is applied by the tab.
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`: it now sweeps `FollowTheTabAsync`
    and `ReceiverConditions.ForTab`. It was red before this unit and is green now.
- **Records:**
  - `## UNIT 503 - STEP 11` is in both outcome copies, and both status copies name 503.
  - Version 1.13.189 → 1.13.190.
  - R111 is in both plans.
  - HM-DEC-207, "The tab is the mode; the map writes nothing", is in `DECISIONS.md`.

**Verification.**
- **The build:** 0 warnings, 0 errors.
- **The tab, W1AW, mode-follow and bar tests: 31 of 31.**
- **The engine's explore, receiver-condition and mode-follow tests: 639 of 639.**
- **The app carry-forward line: 275 of 278.** The three `TheChipSaysTheChosenModeTests` cases failed
  in 1 ms and passed alone (6 of 6).

## 2. What the owner should expect

1. Rebuild.
2. **On the CW tab the radio is in CW and stays in CW** - wherever the dial goes, on any band,
   through a restart, across any block on the map.
3. **On the Digital tab it is in USB-D** with the wide filter, and stays there.
4. **The map no longer touches the radio.** It still says what lives at a frequency.
5. **If you turn the radio's mode knob yourself, the app follows you.** It says "You set the radio
   to USB, so Hamlet will leave the mode alone until you next change tab," and never writes it
   back. Changing tab puts the tab's settings back.
6. **The W1AW button just tunes.** The CW tab has already set CW.
7. **So from now on, if decoding is wrong, it is the decoder.**

## 3. What you should see

Every case was run headless first against HEAD's code, and all five failed there. Before this unit
the map's writes to a data block waited for a dwell timer that does not run headless, so on the CW
tab HEAD wrote nothing at all in the test. On the air that dwell fired, and that was the 12:30
USB-D write.

| case | before (HEAD) | after |
|---|---|---|
| 1. CW tab at 7.0472 (the map's `RTTY` block, whose target was USB-D), then 7.074, 7.0472, 14.074, 14.030 | no CW write; the map's target at each data block was USB-D | one write, `CW, data off`, and nothing more |
| 2. Restart on the CW tab with the radio left in USB-D | no mode write | first write `CW, data off` |
| 3. CW, then Digital, then CW | no writes | `CW` · `USB, data on, filter 1` · `CW`; CW settings: auto notch, manual notch, noise blanker, noise reduction, AGC, RF gain, squelch, attenuator, preamp, scope output; Digital settings: noise blanker, noise reduction, auto notch, AGC, and the scope span spoken |
| 4. CW tab, then the radio reports USB | no write, and the line said "until you next change band" | no write, stands down, "You set the radio to USB, so Hamlet will leave the mode alone until you next change tab." |
| 5. Every mode write in the app | three: the map's, the W1AW press's, the Olivia press's | two: the tab's, and the Olivia press's |

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The Olivia press still writes USB-D itself.** It is redundant on the Digital tab and harmless,
  and the Olivia carry-forward tests pin it. Removing it is a choice for the owner.
- **The "mode follows the map" setting now also governs the tabs' writes.** Its name no longer says
  what it does, and renaming a stored setting needs a migration (§6.1).
- **The Digital tab applies the FT8 row whatever sub-mode is chosen.** The file states no row for
  PSK31, Olivia or JS8.
- **The Voice tab applies no receive settings**, because the file states none for voice.
- **The dwell timer and `ModeFollowPlan`'s data targets, `WaitsForDwell` and `WorkingCw` are now dead
  in the app.** They are left in place rather than removed on the way past (§12.6).
- **Two worktree folders from checking HEAD, `C:\Source\HamLet-wt503` and `C:\Source\HamLet-wt503b`,
  could not be deleted.** Windows refused with "permission denied" while files in them were held
  open. They are unregistered from git and safe to delete by hand.
- **The detector crash reported by units 501 and 502 is still in the tree.** It is a one-line guard
  for the owner to approve.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling.
  - `HowMuchTheApplicationSaysTests`' two ceiling cases, red on HEAD too, checked.
  - `TheLicenceCardAnswersForTheTabAndNotForMorseAlways`.
  - `NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth`.
  - `TheVerdictCarriesTheScopeTests`' four cases.
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
