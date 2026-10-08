## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `src\Hamlet.App\ViewModels\MainWindowViewModel.cs`, `docs\cw-scoreboard.md` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 16:21:07 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. No recording added; nothing keys or transmits; the one new radio write is the AGC in CW. Version 1.13.248 to **1.13.249**. Ruling **HM-DEC-268**, the number the order gave. Nothing was recorded under §12.1.

**Task 1, AGC SLOW in CW, the hand winning** (`19d95c18`).
- **The premise was half right.** Hamlet already wrote the AGC on entering CW, and wrote **FAST**. The CW row of `data\bands\mode-receiver-conditions.json` had asked for `agc` = 1 since 2026-08-29 (`16f4a4c4`), which is why every sheet read FAST. No ruling chose it; the row's own reasoning did.
- **The command:** `16 12`, read before write and read back after, through the existing receiver setup: `00` off, `01` FAST, `02` MID, `03` SLOW (Full Manual `A7292-4EX-6`, p. 19-3, already in `CivWrites.Agc` and `CivReads.Agc`).
- **The row** now asks for SLOW (`03`), with its operator-facing reason rewritten.
  - It is written once on entering the CW tab or connecting in CW, as the preamp is.
  - The story line opens `AGC set to SLOW for CW.`
- **The setting:** `AGC in CW`, the last row of the scan's settings popover (`⋯` beside Scan). It offers SLOW (default), MID, FAST and leave the radio alone.
  - It is kept as `AgcInCw` in `settings.json`, and the popover's heading names it.
  - The engine applies it (`ReceiverConditions.ForTab(..., ReceiverChoices)` and `CwAgc.Apply`). Leave alone takes the row out, so nothing is written.
- **The hand wins:** the existing memory of what a tune-in left keeps a hand change, and nothing re-sets it until the CW tab is next entered.
- **Leaving CW** puts back what the radio had before Hamlet changed it (`CwAgc.RestoreAsync`), unless his hand has moved it since. That restore is the only write the restore makes.
- **The Digital and Voice tabs write no AGC.** The FT8 row's AGC was already unconfirmed and spoken only.
- **The record:**
  - Both capture sheets (Record and the automatic ones) gain `agc        SLOW  (set by Hamlet; Hamlet's choice for CW is SLOW)`.
  - Each `cw_listen` row gains `agc` and `agcSetBy`.
  - Whose it is reads `set by Hamlet`, `already so when Hamlet looked`, `set by hand`, `not Hamlet's, the CW tab is not the mode`, `the radio's own, Hamlet is set to leave it alone`, or `unknown, the radio has not said`.
- **Tests:** eight new fake-rig tests in `tests\Hamlet.App.Tests\ViewModels\HamletSetsAgcForCwTests.cs`, all passing.
- **Existing tests:**
  - **Updated to SLOW:** two engine tests pinned the CW row at FAST: `EveryModeAnswersForEverySettingTests.MorseStatesWhatMorseNeeds` and `TheBlockStatesWhatTheModeNeedsTests.TheMorseBlocksStateWhatMorseNeeds`.
  - **Kept as written:** `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten` checks that the view model's receive path names no control. My first version named the AGC there. Rather than weaken the test, the choice now travels to the engine as one record, and the test passes unchanged.

**Task 2, the comparison** (`60a40c4c`): `docs\agc-comparison.md`, one page.
- The setting to flip.
- What to capture: a W1AW session on each setting, or two ragchews.
- What to send: the capture folders and the day's telemetry.
- What the web session will measure: letters right against W1AW, junk before weak calls, the printed sender's shape score, and the noise humps between letters.

**Records:**
- `PHASE_OUTCOME.md`, both copies: `## UNIT 564 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 564.
- `Directory.Build.props`: 1.13.249.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-268.
- `docs\cw-scoreboard.md`: a 564 row in each table, unchanged.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors. **Guards:** 4 of 4. **Decision-log and voice tests:** 7 of 7. **App carry-forward:** 278 of 278 in 2 m 11 s.

## 2. What the owner should expect

**Rebuild.** You won't need to touch the AGC on the radio any more.

**What happens now:**
- **When you come to the CW tab, Hamlet sets the radio's AGC to SLOW,** once, and the status line says `AGC set to SLOW for CW.` It also does this when it connects while you're on CW.
- **Why SLOW:** it lets the gain settle on the station and hold through the gaps between dits, so the hiss stays flat instead of swelling in every gap.
- **It was Hamlet that set FAST before.** The CW settings asked for FAST, which is why every sheet said FAST.
- **If you turn the AGC on the radio yourself, that wins.** Hamlet leaves it where you put it until you next come to the CW tab.
- **When you leave CW for Digital or Voice,** Hamlet puts the AGC back to what the radio had before, unless you've changed it yourself.
- **Digital and Voice never touch the AGC.**

**Where the setting is:** on the CW tab, the small `⋯` button beside **Scan**; its last row is **AGC in CW**. Your choices are SLOW (the default), MID, FAST, or leave the radio alone. It's remembered across restarts, and it takes effect the next time you come to the CW tab: switch to Digital and back to make it take effect at once.

**Every capture sheet and every ten-second telemetry row now says what the AGC was and who set it.**

**Tonight, to compare** (all in `docs\agc-comparison.md`):
1. Listen to a W1AW session on SLOW (Hamlet records it by itself), then set FAST, go to Digital and back to CW, and listen to the next session.
2. If no W1AW session suits, record two ragchews instead, one on each setting.
3. Don't touch the radio's AGC during a run.
4. Send the web session the capture folders and the day's telemetry file.

**Reading is unchanged:** **297** on your twelve plus KM3STU's three (**234** without), and **2111** on W1AW. Pushed to `main`.

## 3. What you should see

**The fake-rig tests** (`HamletSetsAgcForCwTests`, all passing):

| test | what it shows |
|---|---|
| `EnteringCwSetsSlowOnce` | radio on FAST; entering CW writes `03` once and moving the dial writes nothing more; the story line opens `AGC set to SLOW for CW.` |
| `AHandChangeIsNotOverridden` | after SLOW, the radio set to MID by hand; another tune-in on CW writes nothing; the record says `MID`, `set by hand` |
| `LeavingCwPutsBackWhatTheRadioHad` | writes `03` then `01`; the radio ends on FAST; says `AGC put back to FAST, as it was before CW.` |
| `LeavingCwAfterAHandChangeLeavesHisSetting` | writes `03` only; the radio stays on his MID |
| `TheDigitalAndVoiceTabsWriteNoAgc` | no AGC write on either tab; the radio stays on FAST |
| `LeaveAloneWritesNothing` | no AGC write; the record says `FAST`, `the radio's own, Hamlet is set to leave it alone` |
| `TheSheetAndTheRowCarryTheAgcAndWhoSetIt` | sheet `agc        SLOW  (set by Hamlet; Hamlet's choice for CW is SLOW)`; row `agc` SLOW, `agcSetBy` set by Hamlet |
| `TheChoiceIsKept` | FAST chosen, stored as `fast`, read back as FAST by a new view model |

**Both scoreboards' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| 564 task 1 | 318 of 353 | 20 | 1 | **297** (234 without) | 0 | 83 of 110, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| 564 task 1 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

Every stretch reads as at HEAD. Noise prints nothing, the carrier at seed 5195 as before, and the first recording whole.

## 4. What's blocking us

- **SLOW against FAST has not been measured on the air.** The default is the owner's order, and `docs\agc-comparison.md` is how to measure it.
- **Changing `AGC in CW` while on the CW tab** takes effect only on the next entry to CW. Switching to Digital and back does it at once.
- **`TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`** reads the whole process's CPU and is red when the engine line runs whole. Not touched here.
- **Two reply tests stay red at HEAD:** `TAW` and `IAN`.

### Asks still outstanding

- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
