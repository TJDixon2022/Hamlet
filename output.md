## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 14:20:32 and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. Version 1.13.235 to 1.13.236. Ruling HM-DEC-255, the number the order gave. Nothing was recorded under §12.1. No recording was read except by the scoreboard. All three tasks were done; none was dropped.

**Task 1, the map follows the tab** (`7f960e49`):
- **How a block gets its colour:**
  - Every block of the band carries a family: from `data\bands\us-neighborhoods.json`, or `Open` and `Phone` filled in from the band's structure, or `OutsideTheBand` past the edges.
  - The map paints that family's colour from `ModePalette`.
  - At 7.0475 the data's W1AW block is 500 Hz of Morse between the RTTY row and FT4 sprint. On a 300 kHz map that is a sliver, so the stretch read purple.
- **Shared** is the licence's word: a block where the operator's licence lets him send both Morse and data over all of it (its ends and its middle). With his class unknown, it is where any class may.
- **`ModeLens`** (engine, `Explore`) is the one rule:
  - On the CW tab every shared block is painted and named CW; on the Digital tab, Data; on the Voice tab, as the band data says.
  - A block already of the tab's family keeps its own name, so on the Digital tab FT8 stays `FT8` and the W1AW block becomes `Data`.
- **What reads it:**
  - The view model publishes `MapNeighborhoods` through the lens, and the map binds to it.
  - The green zone's header takes its word from the same block, so it says Morse where the map paints CW and Digital where it paints Data.
  - Everything that decides anything (mode follow, the receiver conditions, the story card) still reads the band data's own blocks.
- **When it repaints:** on a tab change and on a licence change, through the code that already redraws the licence card. It writes nothing to the radio, and nothing on screen moves: only colours and labels change.
- **40 m's shared blocks** are in section 3.

**Task 2, your two yeses** (`a31e8fc5`):
- Recorded as a dated addendum at the end of HM-DEC-253's record; its text is unchanged, as the project corrects rulings.
  - **Stray letters:** six or more letters of one or two elements, each printed alone, within 10 seconds.
  - **On W1AW's frequency:** within half the radio's filter width, or 250 Hz where unread.
- Both are off the outstanding asks below. No code changed.

**Task 3, the scan's ear off the sound card's thread** (`72ceba28`):
- **Before:** the ear ran its own CW chain inside the capture callback while a scan was on.
- **Now:**
  - The callback records the chunk for the catch's WAV and hands it to the same bounded queue the terminal uses (`AudioHandoff`).
  - The chain reads the chunk on the ear's own thread.
  - Everything that reads the ear back (its sense, its tone finder, the key-up share, the end of a catch) first waits for it to catch up, on the scan's thread.
- **Counted the same way:** dropped chunks and holes, carried on each catch's JSON as `earDroppedChunks` and `earHoles`, counted from the catch's start.
- **The test worlds** push audio much faster than real time, and their 10 ms chunks would overflow a three-second queue of 30 slots in 0.3 s. Four test worlds now let the ear catch up after each chunk, as real-time audio would: the scan world, the span world, the carrier test and the ear-against-app test.

**Records:**
- `docs\cw-scoreboard.md`: a row per task.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 551 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 551, that line only.
- `Directory.Build.props`: 1.13.236.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-255, and the HM-DEC-253 addendum.

**Build** `-warnaserror`: no warnings. **App carry-forward:** 278 of 278.

## 2. What the owner should expect

Rebuild and run as usual.

- **On the CW tab the map shows CW** wherever your licence lets you send both Morse and data. The whole stretch from 7.025 to 7.125 on 40 m is now amber, RTTY row, FT4, FT8 and the rest included, each labelled `CW`. At W1AW's 7.0475 the header still says `Morse`, and now the map agrees.
- **On the Digital tab it shows Data** over the same stretch. CW main street, the QRP watering hole and the W1AW sliver turn purple and say `Data`, and the header there says `Digital`.
- **On the Voice tab** the map is as it always was.
- **What never changes colour:**
  - the phone end from 7.125 up, where data isn't allowed;
  - the CW fast lane at 7.000 to 7.025, which a General can only listen to (on an Extra licence it would change with the tab too);
  - the grey `not ham` either side of the band.
- **Switching tabs only repaints.** The radio is told nothing more than the one mode change each tab already makes.
- **Your two yeses are on the record:** the stray-letter trigger and the W1AW tolerance are now your rulings, not mine, and they're gone from the open questions.
- **The scan's ear no longer works on the sound card's own thread.**
  - Like the terminal's decoder since unit 548, it reads behind a queue, so a busy moment in the scan can't make the sound card lose audio.
  - Each catch's WAV is still recorded in full, and each catch's JSON now says if the ear ever fell behind (`earDroppedChunks`, `earHoles`, both 0 when it kept up).
- **The scoreboard:** 227, exactly as before, after every task.
- **One app test is red, and was red before this unit:** `TheGreenZoneTests.NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth`, a layout measurement of the world-clock map's place. It fails identically at HEAD.
- Pushed to `main`.

## 3. What you should see

**40 m's blocks on each tab, for a General licensee** (`TheMapFollowsTheTabTests.FortyMetresOnEachTab`):

| block | kHz | the data says | shared | CW tab | Digital tab | Voice tab |
|---|---|---|---|---|---|---|
| not ham | 6975–7000 | outside | no | not ham | not ham | not ham |
| CW fast lane (CW DX) | 7000–7025 | Morse | **no** (listen-only) | CW DX | CW DX | CW DX |
| CW main street | 7025–7030 | Morse | yes | CW | **Data** | CW |
| QRP watering hole | 7030–7040 | Morse | yes | QRP | **Data** | QRP |
| RTTY row | 7040–7047.25 | data | yes | **CW** | RTTY | RTTY |
| W1AW | 7047.25–7047.75 | Morse | yes | CW | **Data** | CW |
| FT4 sprint | 7047.75–7050 | data | yes | **CW** | FT4 | FT4 |
| CW main street | 7050–7070 | Morse | yes | CW | **Data** | CW |
| PSK31 ribbons | 7070–7074 | data | yes | **CW** | PSK31 | PSK31 |
| FT8 city | 7074–7077 | data | yes | **CW** | FT8 | FT8 |
| open ground | 7077–7078 | open | yes | **CW** | **Data** | open |
| JS8 keyboard corner | 7078–7081 | data | yes | **CW** | JS8 | JS8 |
| RTTY and data | 7081–7100 | data | yes | **CW** | Data | Data |
| automatic stations | 7100–7105 | data | yes | **CW** | auto | auto |
| RTTY and data | 7105–7125 | data | yes | **CW** | Data | Data |
| Phone downtown (SSB DX) | 7125–7171 | voice | no | SSB DX | SSB DX | SSB DX |
| Picture street (SSTV) | 7171–7174 | voice | no | SSTV | SSTV | SSTV |
| Ragchew boulevard (SSB) | 7174–7290 | voice | no | SSB | SSB | SSB |
| AM corner | 7290–7293 | voice | no | AM | AM | AM |
| Phone side (SSB) | 7293–7300 | voice | no | SSB | SSB | SSB |
| not ham | 7300–7325 | outside | no | not ham | not ham | not ham |

Bold is a block whose colour changed. For an Extra licensee the CW fast lane is shared too, and turns Data on the Digital tab.

**The tests:**
- `TheMapFollowsTheTabTests` (engine, 3), passing:
  - at 7.045, 7.0475 and 7.049 the blocks are CW on the CW tab and Data on the Digital tab, and the W1AW block is named `Data` there;
  - the SSB end and `not ham` at 6.990 and 7.310 are the same block on all three tabs;
  - the Voice tab is the data's own map;
  - a General's DX window is not shared.
- `TheMapFollowsTheTabTests.AtW1awTheMapAndTheHeaderFollowTheTabAndWriteNothing` (app), passing:

  ```
  CW tab: the map paints 7.0475 Cw `CW`, the header says `Morse`
  Digital tab: the map paints 7.0475 Digital `Data`, the header says `Digital`
  Voice tab: the map paints 7.0475 Cw `CW`, the header says `Morse`
  CW tab: the map paints 7.0475 Cw `CW`, the header says `Morse`
  radio writes: Cw data False filter -, Usb data True filter 1, Lsb data False filter -, Cw data False filter -; setting writes: 0
  ```

  The four writes are the tab's own, one per tab change (HM-DEC-207); the repaint adds none.
- `TheCatchScanTests.ASlowEarLosesNothing`, passing:

  ```
  as it is: Positive, 27.50 s, WAV 220000 samples, longest delivery 0.210 ms: `RQ CQ DE W1AW W1AW W1AW K`
  slow ear: Positive, 27.50 s, WAV 220000 samples, longest delivery 0.096 ms: `RQ CQ DE W1AW W1AW W1AW K`
  ```

  No chunk dropped, no hole. `APositiveThatStopsIsLeftAfterItsSilence` reads that same `RQ CQ DE W1AW W1AW W1AW K` at HEAD, so the queue changed nothing in what the ear hears.
- **The scan's tests:** 123 of 123, the new one included.
- **Binding health and the tab-is-the-mode tests** pass. `NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth` is red here and at HEAD.
- **App carry-forward:** 278 of 278.

**The scoreboard rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| 551 task 1 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |
| 551 task 2 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |
| 551 task 3 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |

Each task's whole table was identical to HEAD's, line for line.

## 4. What's blocking us

- **`TheGreenZoneTests.NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth`** is red at HEAD and here: the world-clock map's height reads 119 against the band's. It was not touched.
- **Shared is judged at a block's ends and middle.** In this data every privilege boundary falls on a block's edge. A future block straddling one inside would be judged by those three points.
- **The scratch worktree of HEAD** used to check the inherited red was removed at the end.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 550, 2026-10-07:** the handover margin, 0.16, and the challenger's 15 s are the author's figures from measurement, and no recording yet shows the margin acting. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Cw/CwSenderGate.cs`.
