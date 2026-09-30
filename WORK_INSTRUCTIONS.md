# Work instruction 505 - the category pages are lists, and the map opens on a click

**Hand run. One unit, outside the CW phase.** The achievements category pages draw a map on every
card. This unit makes each category a list of what was earned, and the map something he opens by
clicking a row. **The opening page, the popup and every record behind the screen are untouched.**

Verify by building `Hamlet.sln` with warnings as errors and running the app carry-forward line
before the first change and after the last. **The owner's look at the window is the test.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`.
- **This unit is not a step of the CW phase.** Touch no copy of `PHASE_PLAN.md`, `PHASE_OUTCOME.md`
  or `PHASE_STATUS.md`. Tick nothing. `PROJECT_STATUS.md` follows the cadence in the prompt and
  names 505.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  Never run `Hamlet.App.Tests` unfiltered. The app line loses names to the dispatcher loop; re-run
  once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit505-<name>.sh`
  and are not committed.
- **A file that has to go cannot be deleted here.** Empty it to one comment saying which unit
  retired it and why, and list it once in section 2 for the owner to delete by hand.
- Nothing that keys or transmits. Nothing written to the radio. Nothing written to the log. **No
  package. No image asset.** Touch nothing under `src\Hamlet.RadioEngine\`.
- The tree is dirty by 368 files at `57b9759`. **Commit only what this unit changed**, by path.
- American spelling. `output.md` at the root, four headings exactly: `## 1. What Claude did`,
  `## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The owner's ruling

**His words, 2026-09-30:**

> *"I like where they are, but we're just so map centric. How about we change it so that we list
> continents, countries, whatever, and you can click on it and it'll pop up the map. So we still
> store the map that shows, hey, I connected with Chile. And if I click on that, I see me to Chile,
> so I know where it is in the world. But we're just overdoing it on maps."*

**What it supersedes.** R22 and rulings 11 and 16 of work instructions 335 and 342, as the comments
in `AchievementsWindow.axaml` cite them: the trading card with the path map across it at 231 px.
**The later ruling wins.** Ruling 16's popup stands exactly as built; it is now the only place a
map is drawn in this window.

**What it does not touch.** The opening page of eight badges. The conversation card's map. The
green zone's map. `ACHIEVEMENTS_PHILOSOPHY.md` - §2 no wall of blanks, §3.1 earned and then the
one nearest unearned and nothing beyond it, §4 worked and never confirmed - all in force.

---

## 3. Verify this instruction against the tree

Read from a harvest of `57b9759`, 2026-09-30. **Check each; report every mismatch in section 1; do
not repair this instruction; do not stop over a mismatch** unless a task is impossible.

- `src\Hamlet.App\Views\AchievementsWindow.axaml`, 716 lines, window 1040 by 720. One
  `ContentControl` named `AchievementsCategory` whose template holds: the back link, the band
  `AchievementsCategoryBand`, a scroller over `AchievementsSubBadges` (Continents, a two-column
  `UniformGrid`, each item a button on `OpenCategoryCommand` wrapping the card template), and a
  scroller over `AchievementsCategoryCards` (two-column `UniformGrid`, the trading-card template).
- Inside the card template: a `Button` with class `card-map` on `OpenTheMapCommand` holding an
  `Ft8GlobeControl` at `Height="231"` with `FillsBox="True"`; a `card-list` border of the same
  height for a card with no map, showing `NoMapWord` and `ContactLines`; the tier bar; the next
  card's wants line, quill line and callers panel; the distance at 28 px with band, mode and date.
- The popup `AchievementsMapPopup`: light dismiss, the X on `CloseTheMapCommand`, an
  `Ft8GlobeControl` named `AchievementsOpenedGlobe` at most 720 by 400, titled by
  `OpenedMapCallsign` alone.
- `src\Hamlet.App\ViewModels\AchievementsViewModel.cs`: `OpenedMap`, `MapIsOpen`,
  `OpenedMapCallsign`, `OpenTheMap(AchievementCategoryCard?)` which opens only where
  `card.Globe` has `Opens` true, `CloseTheMap`.
- `src\Hamlet.App\ViewModels\AchievementCategory.cs`, 1415 lines: the record
  `AchievementCategoryCard` with `Title`, `Figure`, `PointsLine`, `Earned`, `Callsign`, `Grid`,
  `CallGridLine`, `Globe`, `HasMap`, `HasNoMap`, `NoMapWord`, `ContactLines`, `DistanceLine`,
  `BandModeLine`, `DateLine`, `CountLine`, `TierLine`, `TierFraction`, `WantsLine`, `QuillLine`,
  `Callers`; and the class `AchievementCategory` with `Cards` in the order earned first, then the
  one unearned, and `SubBadges`.
- `NextCaller` in `CqSnapshot.cs` carries a place, a call line and `OpensContinent`. **It carries
  no grid and no path.**
- Tests: `TheCategoryPagesAreTradingCardsTests` (seven tests and seven unit traces by this instruction's count;
  it pins the card and the 231 px map), `TheAchievementsPageClicksInTests`
  (`InsideACategoryTheEarnedCardsComeFirstThenOneUnearned`,
  `ContinentsOpensToSevenAndEachToItsCountries`, `NoStringInAnySlotIsClippedAtTheWindowsSize`),
  `TheAchievementsPageTests`, `TheAchievementsScreenTests`, `TheMapOpensTests`,
  `TheGlobeOnTheCardFaceTests`, `BindingHealthTests`.
- `docs\carry-forward-tests.txt` names none of `TheCategoryPagesAreTradingCardsTests`.
- The newest decision is `HM-DEC-208`.

---

## 4. The tasks

### Task 0 - before anything changes

Run the app carry-forward line. Then run, filtered, one line each:
`TheCategoryPagesAreTradingCardsTests`, `TheAchievementsPageClicksInTests`,
`TheAchievementsPageTests`, `TheMapOpensTests`. **Report each green or red by name. A red here is
inherited and is not chased.**

Stand the window up headless on the fixture log and **count the `Ft8GlobeControl` instances
realized on the Countries page with the popup closed.** That number is the unit's reason.

### Task 1 - an earned item is a row

In `AchievementsWindow.axaml`, the card template becomes a row template and the items panel one
column, full width.

**An earned row carries, on one line where it fits:**

- the category's color at its left edge, as the card has now;
- the title, bold - `Chile`;
- the callsign and grid line;
- **the distance, the largest thing on the row after the title.** It was 28 px on the card; it
  does not shrink to a column of small grey figures;
- band and mode, and the date;
- the count line where the card had one;
- the points at the right.

**No map is drawn on a row.** The `card-map` button, its `Ft8GlobeControl` and the 231 px
`card-list` border leave the template.

**The row is the thing he presses.** Where `HasMap` is true the whole row is a `Button` on
`OpenTheMapCommand` with the row's card as its parameter: hand cursor, keyboard reachable, and
**the row says in a word that it opens a map** - a muted `map` at its right end is the author's
default, overrulable - so that it reads as pressable without relying on a hover (§0.5.1, §0.6).
Never on a hover.

**Where there is no map the row is not a button** and says `NoMapWord` where the word `map` would
have been. `ContactLines` move to the row's tooltip; say in the report if that loses anything.

**Total Miles' tiers and the distance firsts keep their bar on the row**, with `TierLine` in words
above it as now.

**Row height is the author's**, stated with its reason. At 1040 by 720 report how many earned rows
show without scrolling. The scroller stays; the page itself still never scrolls.

**Test watched failing first**, a new `TheCategoryPagesAreListsTests`:

- with the popup closed, **no `Ft8GlobeControl` is realized anywhere in a category**, for each of
  the eight kinds;
- every earned row shows its title, call line, distance, band and mode, date and points;
- a row with a map is a button and a row without one is not, and says why in a word;
- no string on any row clips at 1040, 1400 and 1920 wide.

**Drop candidate:** none.

### Task 2 - the click opens the path, and the popup says where

The popup is ruling 16's and is not rebuilt. Two things:

- **Pressing a row opens that row's path** in `AchievementsMapPopup`; the X and a click outside
  close it; pressing another row after shows the other path.
- **The popup's heading names the place as well as the station** - `Chile - CE3XYZ` in whatever
  form the application already joins two facts - because his question is *where is that*, and the
  callsign alone does not answer it. Keep which row was opened on the view model beside
  `OpenedMap`; `OpenedMapCallsign` may stay for what already binds it.

**Test watched failing first:** in `TheCategoryPagesAreListsTests`, a row press opens the popup
with that row's plot and a heading holding both the title and the callsign; a no-map row opens
nothing. `TheMapOpensTests` stays green and unedited.

**Drop candidate:** the heading. The row press is not droppable.

### Task 3 - the next card goes first

`ACHIEVEMENTS_PHILOSOPHY.md` §3.1 stands: the earned, and the one nearest unearned, and nothing
beyond it. **In a list of a hundred countries the one at the bottom is never seen**, so the
unearned one is drawn first, above the earned rows, as a panel and not a row: `next` in words, the
grey edge, the wants line, the quill line, and the callers panel exactly as the card draws them.

**The callers do not open a map.** `NextCaller` has no path and this unit does not give it one.

Change the order in `AchievementCategory.Cards` or in the view, whichever is smaller; say which.

**Test watched failing first:** `InsideACategoryTheEarnedCardsComeFirstThenOneUnearned` is
rewritten, and renamed, to assert the unearned one first and the earned after; the callers panel
assertions of `TheNextCardKnowsWhoIsCalling` move to the new test class unchanged in substance.

**Drop candidate:** the whole task. If dropped the unearned one stays last and the report says so.

### Task 4 - Continents

Seven rows of the same template. **A continent's row still opens its countries** - that is what
`ContinentsOpensToSevenAndEachToItsCountries` holds and it stays green. The path of the contact
that opened the continent is a **second, small button on the row, beside the first and not inside
it**, saying `map`, on `OpenTheMapCommand`. A button inside a button is not built.

An unearned continent has no map button.

**Test watched failing first:** an earned continent's row carries two pressable things, one opens
its countries and the other opens the popup with that continent's first path.

**Drop candidate:** the map button. The rows are not droppable.

### Task 5 - the old tests, the comments, and two widths

- **`TheCategoryPagesAreTradingCardsTests`:** every test in it is either carried into
  `TheCategoryPagesAreListsTests` in substance or retired. **Name each one in the report with
  which happened and why.** `EveryKindsBandCarriesCountScoreLevelAndABar` and
  `StatesCountWhatTheLogsStateFieldSays` do not depend on the card and are carried whole. The
  seven `Unit3xxTrace...` tests measured a layout that no longer exists and are retired. The file
  is emptied to a comment per section 1.
- **The comments in `AchievementsWindow.axaml`** that cite R22, the 231 px map and rulings 11 and
  16 are rewritten to cite this unit and HM-DEC-209, and say in a line what they replaced.
- **`assets\category-page-countries.png` is the superseded picture.** Do not edit it; say so in
  section 2.
- Stand the window up at 1040, 1400 and 1920 wide and **describe, computed**, the Countries page,
  Continents and Total Miles at each: rows visible, what clips if anything.
- Repeat task 0's count of `Ft8GlobeControl` instances with the popup closed. **It is zero.**

**Drop candidate:** the three-width description.

---

## 5. Record

- `DECISIONS.md`, newest first, **HM-DEC-209**, headline *A category is a list, and the map opens
  on a click*, quoting him from section 2, naming what it supersedes, and saying whose words are
  whose: the ruling is Tim's; the row's layout, the word `map`, the next card first, and the
  continent's second button are the author's under work instruction 505, and overrulable.
- The index row for it in `CLAUDE.md` §1, in the shape of the rows above it.
- Patch-bump `Directory.Build.props`.
- **No phase file.** See section 1.

---

## 6. What not to do

- **Do not draw a map on a row, in a tooltip, or on a hover.** One map in this window, in the
  popup, on a click.
- **Do not touch the opening page**, `HmBadgeTemplate`, or what a badge press does.
- **Do not rebuild the popup** or change `Ft8GlobeControl`, `Ft8GlobePlot`, the frame or the zoom.
- **Do not give the callers a path.**
- **Do not change what is earned, counted or scored.** This unit changes how a category is drawn.
- **Do not clip or wrap a word on a row.**
- **Do not nest a button in a button.**
- **Report mismatches; repair nothing else on the way past** (`CLAUDE.md` §12.6).

---

## 7. Committing and pushing

Commit per task on `main`, by path, each with its red quoted. Push at the end and say whether it
succeeded.

---

## 8. Report

`output.md` at the root, above the four headings:

```
READ IN THIS ORDER.

A. Maps drawn on a category page with the popup closed, before and after.
B. Whether a row press opens its path, and what the popup's heading says.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       505 - <complete|stopped> at task N of 5, <dropped or none dropped> - <date time>
UNIT GOAL:  <in your own words>
NUMBER:     maps on the Countries page at rest: <n> -> 0; earned rows visible at 1040 x 720: <n>
```

Section 2, for the owner, in plain words:

- rebuild;
- open Achievements and click Countries: a list, no maps;
- click a row: the map of that contact opens, you to there, and closes on the X or a click away;
- the one to earn next is at the top, with who is calling;
- Continents: a row opens its countries, the small `map` beside it opens the path;
- the files emptied for him to delete by hand.

Section 1: what changed, file by file; every mismatch against section 3; the old tests and what
became of each; that the build and the app line are green. **Section 3: the three widths,
computed.** Section 4: anything left, a line each.
