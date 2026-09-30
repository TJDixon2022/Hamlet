READ IN THIS ORDER.

A. Whether the opening page shows the ring, the trail, eight tiles and the strip, and which kinds
   draw locked on the fixture.
B. Whether a logged contact shows the unlock panel, and where the log's write point is.
C. The rest. Section 4 raises 7 items, none blocking.

```
UNIT:       506 - complete at task 6 of 6, none dropped - 2026-09-30 14:20 -04:00
UNIT GOAL:  Make the achievements window show overall progress, what is locked and what opens it, and say so in the main window the moment a logged contact earns something.
NUMBER:     tiles drawn: 8 of 8, locked: 1 (States, on the twelve-contact fixture; all 8 on an empty log); unlock panel on a new country: shown
```

**A.** Yes, all four are drawn at 1280, 1400 and 1920, and nothing clips at any of them.
- **Ring:** 0.660 on the twelve contacts, which is 415 points between Rank 4's 250 and Rank 5's 500.
- **Trail:** Ranks 1 to 3 checked, `Rank 4 · you are here`, and `Rank 5 · opens at 500 points` locked.
- **Tiles:** eight. On the fixture **States** is locked, with `To open it: a US state.`
- **Strip:** `ZL1ABC New Zealand opens Oceania`, ringed, then `OE8DDX Austria` and `J38DX Grenada`, each `a new country and grid`.

**B.** Yes. The write point is **`MainWindowViewModel.WriteLoggedContact`**, the only caller of `ContactLogStore.Append`. Every mode's Save passes through it, so no mode is left unwired.
- **The case the tests log:** Italy after Norway shows `Unlocked · A DX contact · I2ZZZ in Italy, 4,228 miles, 20 m FT8. · +31 points · Also: Italy, JN45.`
- **Named first:** `A DX contact`, the largest single thing earned by points. It names the first DX contact, not the country. The country is in the `Also:` line.

## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch `main`.
The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet confirmed.
Nothing in this report is evidence about the radio.

**Run by hand.**
- **The lock:** `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
- **Records not touched:** nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched. No phase file was touched and nothing was ticked.
- **The status:** `PROJECT_STATUS.md` names 506 through `.run-unit\unit506-status.sh`, on unit 505's pattern.
- **What stayed out:** no package, no font file and no image asset.
- **Commits:** only this unit's files, by path, one commit per task, each pushed. Every push succeeded.

**Commits:**
- `f6101189` task 0
- `8a08a730` task 1
- `995ba56b` task 2
- `ae656081` task 3
- `1639f21d` task 4
- `e33b1965` task 5
- `50d297d1` task 6

**Version.** 1.13.193 to 1.13.194.

**Task 0, before anything changed.**
- **App carry-forward:** 278 of 278.
- **`TheCategoryPagesAreListsTests`:** green, 16 of 16.
- **`TheAchievementsPageClicksInTests`:** green, 8 of 8.
- **`TheAchievementsPageTests`:** green, 10 of 10.
- **`TheMapOpensTests`:** green, 9 of 9.
- **`TheAchievementsScreenTests`:** 9 of 10. `TheWindowDrawsEverySixRows` is red and inherited: it looks for `AchievementsModeRows`, which the window has not had since before this unit.

**Mismatches against section 3.**
- **Newest decision:** the order says HM-DEC-209. It was **HM-DEC-210**, taken by unit 507, so this ruling is **HM-DEC-211**.
- **Version:** 1.13.193, not 1.13.192.
- **The window:** it was 1040 by 720 as stated.
- **The page:** `AchievementsPage` was the `StackPanel` described, with the badges, total line, problem line and legend.
- **`AchievementBadge`:** carries every member listed.
- **`AchievementBadgePage`:** as listed, and `Scores` exposes `Total`, `RankName`, `ToNextRank` and `NextRankName`.
- **Kind colors:** as listed.
- **The points file:** `ranks` starts 25, 100, 250, 500. There are **five** ranks in the shipped file, not eight, so Rank 5 at 500 is the top threshold and Rank 6 is the top rank. `rank_names` is empty, and `levels` gives each kind three or four counts.
- **Unit 505's names:** all present.
- **A miss of my own, not a mismatch:** `ThePsk31RecordsAppearTests` had been red since unit 505. It still found cards by `trading-card`, and I didn't catch it then. It is repointed and green.

**The changes, file by file:**
- **`src/Hamlet.RadioEngine/Contacts/AchievementScores.cs`:** one additive, read-only member, `RankStartsAt`: where the rank he holds began. The ring needs it, and the class already worked it out without exposing it.
- **`src/Hamlet.App/Controls/StandingRingControl.cs`, new:** the ring, drawn in code.
- **`src/Hamlet.App/Controls/AchievementSealControl.cs`, new:** a double-ruled ring turned eight degrees, holding the code in the application's monospace face. It shrinks the code to the ring rather than spilling.
- **`src/Hamlet.App/ViewModels/AchievementStanding.cs`, new:** the ring fraction (the file's arithmetic), rank name or number, gap, three facts, and just-unlocked.
- **`src/Hamlet.App/ViewModels/AchievementBadges.cs`:**
  - The tile members: `IsLocked`, `TileBand`, `LevelChip`, `CountText`, `CountWords`, `BarFraction`, `BarGapLine`, `NextLabel`, `ToOpenLine`.
  - The rank trail: `AchievementRankTrail` and `AchievementRankStep`.
  - `Page.Trail`.
- **`src/Hamlet.App/ViewModels/AchievementWithinReach.cs`, new:** the strip.
- **`src/Hamlet.App/ViewModels/AchievementCategory.cs`:**
  - On the card: `EarnedUtc`, `Place`, `Miles`, `SealCode`, `SealColor`, `HasSeal`.
  - The seal codes, stamped by kind in the constructor (`SealFor`).
  - `DrawnCards`, now the earned rows newest first.
  - `NextStamp` and `NextStamps`, the reach members, and the header's `LevelChip`.
  - `Cards` is untouched.
- **`src/Hamlet.App/ViewModels/AchievementsViewModel.cs`:** `Earned`, `EarnedIn`, `Standing` and `WithinReach`.
- **`src/Hamlet.App/ViewModels/UnlockMoment.cs`, new:** what a logged contact unlocked, compared before and after on the achievements window's own pages. It returns nothing where the points file can't be read.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`:**
  - `WriteLoggedContact` reads the log before the write and shows the moment after a successful one.
  - `Unlocked`, `UnlockIsOpen`, `CloseUnlockCommand` and `SeeWhereUnlockedCommand`.
  - `OpenAchievementsAt`, which opens the window with the moment's path already in the popup.
  - `ApplyRigState` closes the panel when `TransmitStatus` reads 1.
- **`src/Hamlet.App/Views/AchievementsWindow.axaml`:**
  - The opening page is a two-column grid: the standing panel, then the title, trail, eight tiles on `HmTileTemplate` (which replaces `HmBadgeTemplate`), and the strip.
  - Category rows carry their seal, with band, mode and date on a third line.
  - The next stamp and `Your reach` stand in a 330 px column at the right.
  - The header band carries the level in words.
  - The window opens at 1280 by 860.
  - The comments are rewritten to cite this unit and HM-DEC-211.
- **`src/Hamlet.App/Views/MainWindow.axaml`:** the `UnlockPanel` popup.
- **Records:** HM-DEC-211 in `DECISIONS.md`, its index row in `CLAUDE.md` §1 (189 to 208 not filled), and `Directory.Build.props`.

**What the author chose, with reasons:**
- **Seal codes:**
  - A country, and a country on a continent's page: the callsign's own prefix, the letters before its first digit or a leading digit and the letters after it (`LA`, `G`, `4X`). The callsign is what the log holds.
  - A state: its two letters. A grid: its four characters.
  - A band: its number of metres. A mode: its name, or its first three letters past five characters.
  - A continent: its two-letter code.
  - A Hall of Fame first: `1ST`, `DX`, `PSK`, `OLV` or `CW`, or its distance in thousands, `5K` or `10K`.
  - A Total Miles tier: its line in thousands, `50K`.
- **Locked-tile words:** `Nothing here yet.` and a `To open it:` phrase of 16 characters or fewer, such as `a US state.` or `any contact.`. That is what a tile holds on the test host's measure.
- **`Next:` and `Opens a set:`:** on their own line above what's next, for the same reason.

**The picture versus what's drawn, and why:**
- **Faces:** no serif display face or web font ships. The application's own face is used at the picture's sizes and weights, with Consolas for callsigns.
- **The three facts** are three rows, not three across. At the panel's 292 px, a large figure and its word don't both fit a third of it.
- **Ring detail:** the ring shows no second sentence ("one new continent gets you most of the way"), because nothing computes it.
- **Tiles:** the name is 13 pt, and the level chip sits on its own row under the header. The Total Miles gap and count shrink to the tile if they ever outgrow it.
- **Rows:** band, mode and date sit under the call line, so a row fits beside the stamp column at 1280.
- **Next stamp:** its sentences may wrap between words, as the picture draws them there. No word is broken.
- **The strip's heading** says when the list was read, not on what band: the snapshot carries a time and no band.
- **The unlock panel** is 560 px wide, not 640, and sits at the window's left rather than the center. That keeps it clear of the right-hand column the conversation cards stand in.
- **`Best day`** in the picture's reach panel isn't drawn; the order asks for farthest and newest.

**The legend went.** The tiles now say in words everything it explained. `Opens a set:` in the ring is the door, `Next:` is a counter, `Locked` says what can't be opened yet, and a hand cursor shows every tile is pressable. The running-total line went too, because the ring carries it.

**The old tests, and what became of each:**
- **Carried in substance:**
  - `TheEightBadgesAreOnTheWindowAndNothingIsClipped`: counts visibly drawn emblems, and finds the total in the ring's points line.
  - `NoStringInAnySlotIsClippedAtTheWindowsSize`: the badge slots became the tile slots. The corner slot is gone.
  - `ThePageHasNoScrollerAndNothingBelowTheLegend` became **`...NothingBelowTheWindow`**.
  - `TheAchievementsWindowAtItsOwnSize`: the tiles' bottom edge replaces the legend's.
  - `StatesCountWhatTheLogsStateFieldSays`: the tile says `2` and `states, from STATE`, so ruling 35's words survive.
- **Carried with changed substance, and renamed:**
  - `TheOneToEarnNextIsDrawnAboveTheEarnedRows` became **`TheNextStampStandsBesideTheList`**, since the picture puts it beside.
  - `InsideACategoryTheOneToEarnNextComesFirstThenTheEarned` became **`InsideACategoryTheEarnedAreNewestFirstAndTheNextStandsApart`**.
- **The page-wide measures** in the lists and clicks-in tests now let a sentence wrap only on an unearned card, and only where every word fits. Two built-in watched reds hold their line under its longest word, not under half the line.
- **My own unit 505 tests** stood the window up at its old 1040 and now use its 1280.
- **`TheNextCardKnowsWhoIsCalling`** reads the callers from the stamp, not the list.
- **Retired:** none.

**Watched red first:**
- **Task 1:** no `AchievementsStandingRing`, no `AchievementsStanding`, no `AchievementsJustUnlocked`.
- **Task 2:** expected 8 tiles; no `AchievementsRankTrail`; the old badges drew `5 of 7`.
- **Task 3:** no `AchievementsWithinReach`, twice.
- **Task 4:** `hall_of_fame [Over 5,000 miles] has no seal`; no `AchievementsNextCard`; no `AchievementsReach`.
- **Task 5:** the window case, with no `UnlockPanel`. **The view-model cases were not watched red.** I wrote `UnlockMoment` before them, and they passed on their first run.

**Build and tests.**
- **Build:** `Hamlet.sln` with warnings as errors, 0 warnings, 0 errors.
- **The achievements set with voice, record and binding health:** 87 of 90. The three reds are all inherited:
  - `TheWindowDrawsEverySixRows`.
  - `VoiceTests.NoOperatorFacingStringUsesABritishSpelling`: the two `centre`s in `MainWindowViewModel.cs`.
  - `DecisionLogOrderTests`: the index gaps 166, 182 and 189 to 210, since the order says not to fill 189 to 208.
- **App carry-forward after the last change:** 278 of 278.

## 2. What the owner should expect

- **Rebuild.**
- **Tools, Achievements.** Your rank sits in a green ring at the left, with your points and how many more to the next rank.
  - Under it: how many contacts and countries, your farthest in miles, and what you last unlocked. Click that to see the map, where it has one.
  - Across the top: the trail of ranks, with the next one locked and the points it opens at.
  - Eight tiles, each with its level, its count, a bar to the next level, and what's next.
  - Along the bottom: who on the CQ list, when you opened it, would earn you something new.
- **A kind you have nothing in is locked** and says what opens it. Click it and it still opens.
- **Click a tile, then a row: the map, as before.** Each row now has its stamp at the left, and the newest are at the top. The next one to earn stands in its own column at the right, with who's calling and your farthest and newest under it.
- **Log a contact that earns something new** and a panel comes up at the left of the main window. It says what you unlocked, where the station is, the points it added, and your rank bar with the new part in lighter green.
  - **See where Chile is** opens the achievements window with that map showing.
  - **Keep going**, or a click anywhere else, closes it.
  - It never takes the keyboard, and it goes away the moment you transmit.
- **Where it differs from the picture:**
  - the application's own typeface instead of the picture's serif;
  - the three facts as rows rather than side by side;
  - the level on its own line in each tile;
  - band, mode and date under the callsign in each row;
  - the strip says when the CQ list was read but not the band;
  - the unlock panel sits at the left, clear of the conversation cards.
  - The reasons are in section 1.
- **Nothing is left for you to delete by hand.**

## 3. What you should see

These are computed on the headless host with the twelve-contact fixture, four callers and the shipped points file.

| Width | Opening page | Countries | Continents | Total Miles |
|---|---|---|---|---|
| 1280 x 860 | ring at 0.660; trail Rank 1 to 3 checked, Rank 4 here, Rank 5 locked at 500; 8 tiles of 212 x 196, States locked; strip of 3; nothing clips | 8 rows, newest first, all shown without scrolling, 64 px each, with the next stamp and Your reach beside; nothing clips | 7 rows, the 5 earned whole, the 2 unreached as panels with their callers; nothing clips | no earned tier, the 50,000-mile tier as the next stamp with its bar; nothing clips |
| 1400 x 860 | the same, tiles 242 wide; nothing clips | the same; nothing clips | the same; nothing clips | the same; nothing clips |
| 1920 x 860 | the same, tiles 372 wide; nothing clips | the same; nothing clips | the same; nothing clips | the same; nothing clips |

**What the fixture gives:**
- **Empty log:** all eight tiles are locked, and the standing panel says `Your first contact earns` / `10 points.`
- **Unreadable points file:** no ring and no score. The panel says why, and the page still draws.
- **Maps at rest:** none, anywhere in the window, as unit 505 left it.

## 4. What's blocking us

Nothing blocks. What's left, a line each:
- **Decision id:** the order named HM-DEC-210, which unit 507 had already taken, so this ruling is HM-DEC-211.
- **The trail's top:** the shipped points file has five ranks, not eight. At Rank 6 the trail draws the last three passed ranks and `Rank 6 · you are here` with nothing locked, and the ring is full.
- **Unlock panel placement:** it is fixed at the window's left, clear of the conversation cards' column. It does not track any other panel's position.
- **The strip** says when the CQ list was read and not on what band, because the snapshot carries no band.
- **`VoiceTests`' two `centre`s** in `MainWindowViewModel.cs` are still red and inherited.
- **`TheWindowDrawsEverySixRows`** is still red and inherited. It looks for a control the window no longer has.
- **The decision index** still stops short at 189 to 208. The order says not to fill them.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
