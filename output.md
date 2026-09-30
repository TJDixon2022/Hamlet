READ IN THIS ORDER.

A. Maps drawn on a category page with the popup closed, before and after.
B. Whether a row press opens its path, and what the popup's heading says.
C. The rest. Section 4 raises 6 items, none blocking.

```
UNIT:       505 - complete at task 5 of 5, none dropped - 2026-09-30 11:40 -04:00
UNIT GOAL:  Draw each achievements category as a list of what he earned, with no map on it, and open the one map on a click.
NUMBER:     maps on the Countries page at rest: 8 -> 0; earned rows visible at 1040 x 720: 8 of 8 on the fixture, below the next panel
```

**A.** On the twelve-contact fixture with the popup closed, the Countries page drew **8** maps and
now draws **0**. Every other page is now **0** too; before, Grids drew 10, Continents 5, Hall of
Fame 5, Modes 5, Bands 4 and Europe 3.

**B.** Yes. A real click on the Norway row opens Norway's path, and the heading reads
**`Norway · LA1ZZZ`**. After closing it, a click on the United Kingdom row reads
**`United Kingdom · G0MNO`**. The X closes it, and so does a click outside. A row with no grid
opens nothing.

## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- **The lock:** `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same
  way. The script keeps it in `C:\Source\ClaudeProjectStatus`.
- **Records not touched:** nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\`
  was touched. No copy of `PHASE_PLAN.md`, `PHASE_OUTCOME.md` or `PHASE_STATUS.md` was touched.
- **The status:** `tools\status.sh` takes the unit from `PHASE_STATUS.md`, so a wrapper,
  `.run-unit\unit505-status.sh`, set the status to name 505 after each write.
- **What was left alone:** nothing under `src\Hamlet.RadioEngine\` was touched. No package, no image
  asset, nothing written to the radio or the log, nothing that keys.
- **Commits:** only this unit's files, by path, one commit per task, each pushed.

**Commits, in order:**
- `6894e593` task 0
- `435ada28` task 1
- `3adecc7e` task 2
- `a7e22be3` task 3
- `f9c18033` task 4
- `5644b54d` task 5

Every push succeeded.

**Version.** 1.13.191 to 1.13.192.

**Task 0, before anything changed.**
- **App carry-forward:** 277 of 278. The loss, `ThePsk31ConversationCardTests`, took 1 ms and is
  green alone.
- **`TheCategoryPagesAreTradingCardsTests`:** green, 15 of 15.
- **`TheAchievementsPageTests`:** green, 10 of 10.
- **`TheMapOpensTests`:** green, 9 of 9.
- **`TheAchievementsPageClicksInTests`:** 7 of 8. `NoStringInAnySlotIsClippedAtTheWindowsSize` was
  red and inherited, with `modes: [Olivia] needs 60.0 px and its slot is 0.0`.
- **The count:** 8 maps on Countries at rest.

**The changes, file by file:**
- **`src/Hamlet.App/Views/AchievementsWindow.axaml`:**
  - **One template for an item of a category, `HmCategoryItemTemplate`.**
    - **An earned item is a row.** It has the category's color at the left edge and the title bold in
      a 340 px column. The call line and any count sit under the title. Then come the distance at
      22 pt bold, band and mode over the date, the points, and at the right end `map` or the no-map
      word.
    - **The unearned one is a panel:** the grey edge, the title and points, `next`, the tier bar,
      the wants line, the quill line, and the callers panel as the card drew it.
  - **The card list is one column and the row is the button.** Where the contact has a map the row
    sits inside a `category-row` button on `OpenTheMapCommand`. Its resting look says it can be
    pressed: a hand cursor, the word `map`, and the edge goes amber under the pointer. Otherwise the
    row is a plain control with its contacts in the tooltip.
  - **The list binds `DrawnCards`**, so the unearned one comes first.
  - **Continents:** seven rows, each an `hm-badge` button that opens its countries. A separate
    `category-map` button beside the row opens the path.
  - **The popup is not rebuilt.** Its heading binds `OpenedMapHeading`.
  - **The comments** that cited R22, the 231 px map and rulings 11 and 16 now cite this unit and
    HM-DEC-209 and say what they replaced.
- **`src/Hamlet.App/ViewModels/AchievementCategory.cs`:**
  - `AchievementCategoryCard.MapWord` (`map`), `OpensAMap` and `ContactTip`.
  - `AchievementCategory.DrawnCards`: the unearned one first, then the earned in the order earned.
- **`src/Hamlet.App/ViewModels/AchievementsViewModel.cs`:**
  - `OpenedCard`, kept beside `OpenedMap`, and `OpenedMapHeading`.
  - `OpenedMapCallsign` stays.
- **`tests/.../TheCategoryPagesAreListsTests.cs`, new.** It holds 16 tests, 17 cases.
- **`tests/.../TheAchievementsPageClicksInTests.cs`:**
  - `InsideACategoryTheEarnedCardsComeFirstThenOneUnearned` is rewritten and renamed
    `InsideACategoryTheOneToEarnNextComesFirstThenTheEarned`.
  - Its `DrawnCards` helper looks for `category-card`.
  - Its `StateContacts` call points at the new class.
- **`tests/.../TheCategoryPagesAreTradingCardsTests.cs`:** emptied to one comment.
- **`DECISIONS.md`:** HM-DEC-209.
- **`CLAUDE.md` §1:** its index row, at the top.
- **`Directory.Build.props`:** the version bump.

**Where the order changed (task 3): in the view.** `DrawnCards` reorders for the list, and `Cards`
is untouched. That is the smaller change, because about a dozen builders fill `Cards` and every count
reads it.

**Row height, the author's.** It follows the content: two lines of text and their padding. That is
41 px on the test host (51 px where a count line adds a third), plus 8 between rows. The reason is to
be readable at arm's length and small enough that a page of countries shows seven or eight at once.

**Earned rows at 1040 x 720:**
- The list's viewport is 589 px, and the Countries next panel takes 201 of it.
- All 8 of the fixture's 8 countries show whole without scrolling.
- The space below the panel holds about 8 rows of 41 px; a ninth would scroll.

**Tooltips:** moving `ContactLines` into the tooltip loses nothing. The one no-map row on the
fixtures, Canada, lists its contact there, and the test checks every line is in it.

**Watched red first, one per task:**
- **Task 1:** `twelve contacts: 0 earned rows drawn for 8 earned`, `1040.00 hall_of_fame: no row is
  drawn`, and the map count above.
- **Task 2:** `[Norway] opened LA1ZZZ, heading LA1ZZZ`.
- **Task 3:** `hall_of_fame: the one to earn next is at y 360.00, not above the first earned row at
  115.00`.
- **Task 4:** `Africa earned, map buttons 0`.

**Every mismatch against section 3.**
- **None in the three files.** `AchievementsWindow.axaml` was 716 lines at 1040 by 720 with the
  template as described. `AchievementCategory.cs` was 1415 lines with the record and class as
  described. `NextCaller` carries no grid and no path. The newest decision was HM-DEC-208.
- **`TheCategoryPagesAreTradingCardsTests`** held seven tests and seven traces as counted; the popup
  test is a two-case theory.
- **The carry-forward list** names none of them.
- **Not in section 3:** `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` is red. It looks for
  an `ItemsControl` named `AchievementsModeRows` that is not in the window at HEAD either, so it is
  inherited and was not chased.

**What became of each old test in `TheCategoryPagesAreTradingCardsTests`:**
- **`EveryKindsBandCarriesCountScoreLevelAndABar`:** carried whole. It does not depend on the card.
- **`StatesCountWhatTheLogsStateFieldSays`:** carried whole. Its "no card is white" check now uses
  the row measure below.
- **`EveryEarnedCardIsTheContactThatEarnedIt`:** carried in substance.
  - The view-model half and the ruling 21 facts at 1400 and 1920 are unchanged. The "has a map"
    check now means the row is the button that opens one.
  - The window check at 1040 now counts rows that open a map, where it counted maps drawn.
  - **The ruling 11 and 12 half is retired:** the map across the card at 231 px, and its crop to the
    two stations. No map is drawn on a row, and the popup's frame is `TheMapOpensTests`'.
- **`ACardsMapOpensInItsPopupOnAClickAndAClickOutsideClosesIt`:** carried as
  `ARowsMapOpensInItsPopupOnAClickAndAClickOutsideClosesIt`. The click lands on the row. Hover opens
  nothing, it never covers the back control, and nothing is written; all unchanged.
- **`TheNextCardKnowsWhoIsCalling`:** moved whole in task 3. Only the class it finds the card by
  changed, `trading-card` to `category-card`.
- **`TheOtherFiveKindsEachDrawTheirOwnCards`:** carried. "No map with width" now means the row is
  not the button that opens one.
- **`NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty`:** carried.
  - "White" was "no map, bar or list". It is now "nothing but the title": a row needs a bar, a list,
    or a fact beside its title. Its built-in watched red hides those and restores them.
- **The seven traces, `Unit342`, `Unit345`, `Unit346`, `Unit347`, `Unit348`, `Unit349` and
  `Unit354`:** retired. They measured a card layout that no longer exists.

**Build and tests.**
- **Build:** `Hamlet.sln` with warnings as errors, 0 warnings, 0 errors.
- **The achievements set and binding health:** 48 of 48. That covers `TheCategoryPagesAreListsTests`,
  `TheAchievementsPageClicksInTests`, `TheAchievementsPageTests`, `TheMapOpensTests`,
  `TheGlobeOnTheCardFaceTests` and `BindingHealthTests`.
- **`NoStringInAnySlotIsClippedAtTheWindowsSize`, red at HEAD, is now green.** The callers panel now
  runs full width, so the Olivia caller row has room.
- **App carry-forward after the last change:** 277 of 278. The loss, `ThePsk31OfferTests`, took 1 ms
  and is green alone.

## 2. What the owner should expect

- **Rebuild.**
- **Open Achievements and click Countries.** You get a list, one country to a row, and no maps. Each
  row shows the country, who you worked and their grid, how far it was in big type, the band and
  mode, the date and the points.
- **Click a row.** The map of that contact opens, you to there, headed with the place and the
  station, like `Norway · LA1ZZZ`. It closes on the X or a click anywhere else. A row that says
  `no grid, so no map` isn't a button; hover over it to see the contact.
- **The one to earn next is at the top**, above the rows, with who on the CQ list would earn it for
  you.
- **Continents:** clicking a row opens that continent's countries, and the small `map` button beside
  it opens the path of the contact that first reached it.
- **Delete these by hand; they are emptied to one comment:**
  - `tests\Hamlet.App.Tests\Views\TheCategoryPagesAreTradingCardsTests.cs`.
- **`assets\category-page-countries.png`** is the picture this replaces. It's left as it was.

## 3. What you should see

The three pages at three widths, computed on the headless host with the twelve-contact fixture,
four callers and a 17 m best bet. The window is 720 tall and the list's viewport is 589 px at every
width.

| Width | Countries | Continents | Total Miles |
|---|---|---|---|
| 1040 | the next panel (201 px), then 8 of 8 earned rows whole at 41 px; nothing clips | 7 rows, 4 of the 5 earned whole at 51 px; the two unearned carry their callers; nothing clips | the next tier panel alone (117 px) with its bar; nothing clips |
| 1400 | the same: 8 of 8 whole; nothing clips | the same: 4 of 5 whole; nothing clips | the same; nothing clips |
| 1920 | the same: 8 of 8 whole; nothing clips | the same: 4 of 5 whole; nothing clips | the same; nothing clips |

**Why the widths don't change the counts:** rows are one to a line at every width, so the count
depends on the height. The extra width goes into the space between the title and the distance.

**Maps realized with the popup closed:** 0 on every kind and on Europe, on both fixtures.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The `CLAUDE.md` §1 index stops at HM-DEC-188**, with 189 to 208 never indexed.
  `DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes` was red at HEAD with gaps
  166 and 182. Adding 209's row, as the order asks, now also exposes 189 to 208. Filling those twenty
  rows is its own work (§12.6).
- **`VoiceTests.NoOperatorFacingStringUsesABritishSpelling`** is red at HEAD: two uses of `centre` at
  `MainWindowViewModel.cs:13226` and `13234`. It is inherited and not this unit's.
- **`TheAchievementsScreenTests.TheWindowDrawsEverySixRows`** is red at HEAD. It looks for
  `AchievementsModeRows`, which the window no longer has, so the test is stale.
- **The window's opening comment still says the window is 1000 wide**; it is 1040. Left alone.
- **`TheCategoryPagesAreTradingCardsTests.cs`** is emptied and waits for the owner to delete it.
- **Continents keep their alphabetical order**, so an unearned continent can sit between earned
  ones. Task 3 moved the next one first only on the card lists, as ordered.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
