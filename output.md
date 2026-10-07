## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 16:25:29 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. R88 was lifted for the owner's twelve recordings, the three scan catches, W1AW's `cw-2026-10-06-212015` and the four pieces of W1AW's session of 2026-10-07, and no other. Version 1.13.238 to **1.13.239**. Ruling **HM-DEC-258**, the number the order gave. Nothing was recorded under §12.1. Every test run was one filtered invocation, one piece set per run; the longest was the app carry-forward line, 2.5 minutes.

**Task 1, E and T inside a word are not noise** (`793d6d46`), **shipped:**
- **Seen failing first:** `EAndTInsideAWordTests`, 30 WPM through the filter with the bench's AGC, read through the app's chain. At HEAD all five lines lost their E and T runs (section 3).
- **The rule:** three or more one-mark letters in a row are dropped only where they do not lie inside a word. Inside a word means a letter of two marks or more just before the run and just after it, with no word gap anywhere between.
- **Narrower than the order's wording.** The order said a run is dropped only when each of its letters is printed alone as a word. Built that way, the board read **237**, under the bar, and a clean-marks variant read 236, so neither shipped.
- **Results:** all five lines read whole, `E T E T E` printed as five lone words still reads nothing, and the board holds at **238**.

**Task 2, the 550 Hz ghost, replayed** (`9a4fc209`, `accba896`):
- **The replay:** `TheW1awSessionReplayTests` plays the four pieces in order through the app's chain, on one audio clock, at the radio's state from their sheets.
- **No sender at 550 or 650 Hz in replay.** W1AW stands at 600 Hz with 5772 marks (5762 on the live sheet), beside three short noise senders at 675, 700 and 800 Hz, none printed (section 3). The same holds:
  - at the live capture's 50 ms chunks as well as 10 ms;
  - over the whole audio band;
  - on the 552 build and the 553 task 2 build, in a scratch worktree, as on this unit's;
  - after twenty seconds of a 20 WPM station at 550 Hz keyed in front of the session. That sender takes its own 67 marks, is let go a minute later, and takes none of W1AW's.
- **So the ghost is not made of the audio**, the chunk timing, the passband, the build since 552, or a sequence standing from before. No shadow rule was built, because nothing measured shows a shadow. What is left, and how to test each, is in section 4.
- **The replay found a fault of its own, now fixed:**
  - **The fault:** this tree's replay broke W1AW's 35 WPM text (`TND` for AND at 546.7 s, 74 places off live).
  - **The cause:** in a word gap the radio's AGC lifts the noise to within 10 to 20 dB of the key-down level, over the window's up line. Unit 553's level read joined that noise to the next dit and made a 70 ms mark of A's 35 ms dit.
  - **The fix:** an end of a level-read span now comes in where two hops or more lie under half the sender's amplitude. The trim is kept only where what is left stands within 3 dB of the key-down level; otherwise the span is offered as before.
  - **Measured and not shipped:**
    - trimming a single hop moved every mark and printed `?` and `S` inside `221502`'s silence;
    - three hops, or the window's rise, left the strong catch at 130 of 152;
    - skipping short runs over the line as well cost the strong catch two letters;
    - letting the held dit go after a pause, and switching off the half-dit floor, did not change the fast text: they are not the cause.
- **The board** holds at **238**, now 14 wrong and 0 invented where it was 13 and 1. Nothing prints in a silence.
- `TheFastTextReadsWhole` asserts A at 546.6 s and the eight fast-text words the live text broke.

**Task 3, seventeen minutes of W1AW join the board** (`e6a92f52`), **shipped:**
- **The offline read:** `TheW1awTableTests.EachPieceReadOffline` reads each piece non-causally.
  - An envelope at the sheet's pitch, cut at half the key-down level of each ten seconds, with a mark kept only where its top reaches within 3 dB of it. The AGC's noise sits over the old midpoint cut.
  - Marks and gaps are classed against the forty marks around each, so the speed changes need no table.
- **The reference:** `TheOfflineReadAgainstLive` sets the offline read against the live text word by word.
  - **Put to English:** QSA DI to QST DE, 2026 ISSUE, 13, AND, THE, ATTACH, INCREASE, HIGHER, and BANDS with its period.
  - **Kept as read:** MIKROWAVE, since both reads hear a K.
  - **Left out:** piece 1's first QST; the west coast station's call after K, a run of dahs neither read can part; the `E` and `TEA` between `AS` and `BT`; and the Y the piece boundary cuts.
  - **Confidence:** high where the offline read and live agree on every word, medium where any word differed or was corrected. That gives 43 stretches, 18 high and 25 medium, **2151 letters**.
- **The `w1aw` table** in `docs\cw-scoreboard.md` is scored as the twelve are, each piece read cold through the live path, with its own rows and total.
  - **Invented** means a letter with no offline mark under it.
  - **The baseline at HEAD** was measured in a scratch worktree at `f4bd0d96`, and **after task 1** at `793d6d46`.
- **The guard** holds the table: `TheScoreboardGuardsCwTests.TheW1awTableFallsNowhere` reads the table's last row, and `TheW1awRowIsWhatItsGuardReads` proves neither guard reads the other's rows. `docs\carry-forward-tests.txt` names it on the CW line.

**Records:**
- `docs\cw-scoreboard.md`: a row per task, and the `w1aw` section with its stretch table.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 554 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 554, that line only.
- `Directory.Build.props`: 1.13.239.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-258.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors.

**The scoreboard guard:** 4 of 4 pass, the board at 238 and the w1aw table at 2097.

**The decision-log and voice tests:** 7 of 7.

**App carry-forward:** 277 of 278. The one failure, `Unit376TheTopBandTests`, was `You've caused dispatcher loop`, and it passes alone.

The scratch worktrees were removed.

## 2. What the owner should expect

Rebuild and run as usual.

- **DETECT and DETERMINED read whole.** Hamlet used to throw away three one-letter E's and T's in a row as noise, even in the middle of a word. Now it only does that when they don't sit inside a word, so ATTEMPT, KILOMETERS, BETTER and LETTER come through too. Noise that prints a string of lone E's and T's is still dropped.
- **W1AW's fast text reads better than it did live.** Played back through this build, the 35 and 30 WPM text reads DETECT, RADIATED, SPECTRUM, DETERMINED, PRETTY, EFFECTIVE, ATTEMPT and KILOMETERS whole, where live it broke them. Only 19 small places differ from what you saw live, and from DETECT on the playback is the better read.
- **The fix behind that:** between words your radio's AGC lifts the background noise close to the signal's level. Hamlet was gluing that noise onto the next dot and reading it as a dash, so AND came out as TND. It now trims a mark back to where it really stands at the station's level.
- **The 550 Hz station is not in the recordings.** Played back any way I could, at either chunk size, over the whole band, on last week's builds, even with a station planted at 550 Hz first, Hamlet never holds a sender at 550 Hz. Whatever made it live isn't in the audio the app saved. What's left to test is in section 4.
- **W1AW's seventeen minutes are now on the board** as their own table of about two thousand letters: 2044 before this unit, **2097** after. The guard holds that table as it holds the twelve.
- **The twelve recordings** read 238 throughout. Nothing prints in a silence, noise prints nothing, the first recording reads whole, and the random carrier prints at one seed as before.
- **The strong 860 Hz scan catch** gives 137 of 152 letters right, one more than before, but 12 wrong where it had 10. The weaker catch is unchanged at 22 of 35.
- **What will look wrong but is not:** the app's test line shows one "dispatcher loop" failure, a different test each run, and that test passes on its own.
- Pushed to `main`.

## 3. What you should see

**The scoreboard rows, the twelve:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 252 of 288 | 13 | 1 | **238** | 0 | 65 of 87, 2 added |
| 554 task 1 | 252 of 288 | 13 | 1 | **238** | 0 | 65 of 87, 2 added |
| 554 task 2 | 252 of 288 | 14 | 0 | **238** | 0 | 65 of 87, 2 added |
| 554 task 3 | 252 of 288 | 14 | 0 | **238** | 0 | 65 of 87, 2 added |

Task 2 moves one stretch: `221745` reads `EUR MREE H ED TO` where HEAD read `EUROREE H ERD TO`, so one invented letter becomes one wrong one.

**The `w1aw` table:**

| unit | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2044** | 2081 of 2151 | 37 | 0 | 437 of 440, 20 added |
| 554 task 1 | **2059** | 2096 of 2151 | 37 | 0 | 437 of 440, 20 added |
| 554 task 2 | **2097** | 2116 of 2151 | 19 | 0 | 438 of 440, 11 added |
| 554 task 3 | **2097** | 2116 of 2151 | 19 | 0 | 438 of 440, 11 added |

Measured on the way and not shipped (task 2):

| variant | board | in silence | strong catch | why not |
|---|---|---|---|---|
| task 1's rule as the order worded it | 237 | 0 | | under the bar |
| trim one hop, skip short runs | 237 | 1 | 134 of 152, 15 wrong | invented 2, `221502` |
| trim one hop | 240 | 1 | 138 of 152, 11 wrong | `?` and `S` in `221502`'s silence |
| trim three hops, or the window's rise | 238 | 0 | 130 of 152, 13 wrong | the strong catch falls |
| **trim two hops (shipped)** | **238** | **0** | 137 of 152, 12 wrong | |

**Task 1's synthetic lines**, 30 WPM, through the filter with the bench's AGC:

| sent | HEAD | after |
|---|---|---|
| `DETECT THE RADIATED SIGNAL` | `D CT THE RADIATED SIGNAL` | whole |
| `PERFORMANCE IS DETERMINED BY` | `PERFORMANCE IS D RMINED BY` | whole |
| `HUNDREDS OF KILOMETERS` | `HUNDREDS OF KILOM RS` | whole |
| `INSPIRED TO ATTEMPT A TRANSVERTER` | `INSPIRED TO A MPT A TRANSVERTER` | whole |
| `A BETTER LETTER` | `A B R L R` | whole |
| `E T E T E`, five lone words | nothing | nothing |

**The senders held through the pieces, replayed** (seconds on the session's clock, piece 1 from 0):

| pitch | seen | marks | shape | printed |
|---|---|---|---|---|
| 600 Hz | 63 to 1182 s | 5772 | 0.95 | 1102 s |
| 675 Hz | 521 to 580 s | 6 | 0.54 | never |
| 700 Hz | 315 to 318 s | 5 | 0.61 | never |
| 800 Hz | 205 to 211 s | 5 | 0.63 | never |

- **The same at 50 ms chunks**, and on the 552 and 553 task 2 builds (5601 marks at 600 Hz there).
- **Over the whole band:** short noise senders from 150 to 1400 Hz, and none at 550.
- **With a 550 Hz station first:** it stood 5 to 79 s and took 67 marks, its own.
- **Live, by the sheets:** 550 Hz held 147, 347, 740 and 978 marks, and 650 Hz 21 in piece 2.

**Replay against live**, every difference, with the replay's time (this unit's tree; 74 places before task 2's fix):

| time | live | replay |
|---|---|---|
| 61.9 s | `M ■AW TU CODE PRACTICE … DE W1AW <AS> G ST GST` | `QST QST` (the minutes before 19:59 are not in the pieces) |
| 336.1 s | `K9 OM` | `K■M` |
| 473.8 s | `A E` | `TEXT` |
| 537.7 s | `N T T O T ■IE` | `<BT> NOW 35` |
| 695.3 s | `MICROWAVELENGTHS` | `MICROWAVELENGTI I` |
| 746.7 s | | `ATTEMPT` |
| 749.1 s | `MPT A` | |
| 821.0 s | `ASE` | `TSE` |
| 872.3 s | `KILOM A S.` | `KILOMETERS.` |
| 883.5 s | `NEXT` | `NETAT` |
| 968.4 s | `EFFE RTIVE` | `EFFECTIVE` |
| 1050.6 s | `PR Y` | `PRETTY` |
| 1059.8 s | `D CT` | `DETECT` |
| 1062.7 s | `NADIATED` | `RADIATED` |
| 1096.4 s | `SP CTRUM` | `SPECTRUM` |
| 1127.6 s | `D RMINED` | `DETERMINED` |
| 1139.6 s | `THAN` | `TIEAN` |
| 1156.2 s | `ULTRE` | `ULTRA` |
| 1180.2 s | `QS` | `QST DE WE` (the replay runs to the end of piece 4) |

The whole replay's text and its word times print in `TheSessionReplayed`; the w1aw stretch table, reference against what printed, is in `docs\cw-scoreboard.md`.

## 4. What's blocking us

- **The 550 Hz ghost is not found.** Three things are left, each with a way to test it:
  - **Which build ran live.** The capture sheet does not name the app's version; adding it answers this next session.
  - **The chain's state after hours of listening before 19:59.** A capture that starts earlier, or a sheet line naming the senders held when capture began, answers it.
  - **A passband that flickers.** If the radio's pitch or filter goes unread for a tick, the detector sums the whole band and rebuilds its bins. The day's telemetry on the station computer answers it; none is on this machine.
- **The strong scan catch** reads 137 of 152 with 12 wrong, against 136 and 10 at HEAD.
- **The w1aw reference is the author's,** from an offline read and English, and waits on the owner's ear. In particular: AND where both reads hear AT D, INCREASE where both hear INTNR ASE, MIKROWAVE kept, and the west coast station's call left out.
- **The dispatcher-loop failure** in the app's test line hits a different test each run and is not this unit's.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 550, 2026-10-07:** the handover margin, 0.16, and the challenger's 15 s are the author's figures from measurement, and no recording yet shows the margin acting. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Cw/CwSenderGate.cs`.
- **Unit 552, 2026-10-07:** the key-down term's form, one to Morse's 15 of 22 and straight to nought at a key never up, is the author's. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Cw/CwSequenceShape.cs`.
- **Unit 553, 2026-10-07:** these figures are the author's from measurement, and wait on the owner:
  - the hysteresis, 0.4 up and 0.6 down;
  - the half-dit floor on the dit the window opened on;
  - opening the window on the waiting sender.

  The change sits in `src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`.
- **Unit 554, 2026-10-07:** these are the author's and wait on the owner:
  - task 1's rule, inside a word rather than each letter alone as a word, since the order's wording read 237;
  - the trim's two hops and 3 dB;
  - the w1aw reference's corrections and omissions.

  The changes sit in `src/Hamlet.RadioEngine/Cw/CwSenderGate.cs`, `src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs` and `tests/Hamlet.RadioEngine.Tests/Cw/TheW1awTableTests.cs`.
