```
READ IN THIS ORDER.
```

A. The phase goal - the screen says what is true and looks like someone meant it.
   Step 0 done; step 1 done by this unit, every must-pass met; steps 2 and 3 not started.
B. Step 1 and its seven must-pass - band with count/score/level/bar; earned card
   from the log entry; next card with the CQ list; all eight kinds per R22;
   no clip at 1400 and 1920; no white card; telemetry with the card count.
   1. The band with count, score, level and a bar, or words where there is no next
      level: met, on 8 of 8 kinds.
   2. The earned card from the log entry that earned it: met, on every kind that has
      earned cards.
   3. The next card with the CQ list, or no one is calling from there now: met.
   4. All eight kinds per R22, and Continents to seven and each to its countries: met.
   5. No clip or wrap at 1400 and 1920: met - every visible run fits on 10 pages at
      each width.
   6. No white card: met - 0 of 51 cards at each width.
   7. Telemetry with the card count: met.
   The nice-to-pass, the map opening in the popup, is dropped whole: it was task 5,
   the named drop candidate.
C. The report last. Section 4 raises 3 items on top of the carried queue; none stands in
   the way of a criterion in B.

```
UNIT:       335 - complete at task 4 of 5 - 2026-09-12 17:23
PHASE GOAL: Maintenance - make what Hamlet shows true and deliberate-looking, screen only, judged
            finally by Tim at his own window size.
UNIT GOAL:  Turn all eight achievements category pages from lists of titles into trading cards:
            each earned card the contact that earned it, with its path map, and each next card
            naming who on the CQ list would earn it.
ADVANCED:   yes - all seven of step 1's must-pass are met, by tests run in this session
NUMBER:     kinds drawn as trading cards 0 -> 8 of 8
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 5.** Tasks 0 to 4 are done and each is pushed.

**Task 5, the map opening in the popup, was dropped whole.** It is the drop candidate the
instruction named, and a nice-to-pass, so no must-pass was lost with it. It was dropped at
about 47 minutes into the session, inside `CLAUDE_CODE.md` §2's 45-to-60-minute window. The
next unit inherits it if Tim wants it:
- the card's map is already the conversation card's `Ft8GlobeControl`, drawn in the popup's
  path-fitted frame;
- what is missing is the press and the popup.

Claude Code on Tim's Windows 11 machine, project Hamlet. The gate passed on all four checks.
Branch `main`. Pushed: task 0 as `5306c2e`, task 1 as `8f06bea`, task 2 as `14bb53f`, task 3 as
`9cfe26d`. Task 4 is the last commit, and its push is reported at the end of this section.

**Every appearance claim in this report is computed on the Avalonia headless host, not seen.**
That host draws text at a flat ten pixels a character, wider than the glass.

### Task 0 - the trace, before a line is built

- Carry-forward list, before any edit, the two invocations its comment orders: app **100 of
  100** in 9 s, engine **85 of 85** in 4 s.
- **Entry criterion met.** `TheAchievementsPageClicksInTests` **6 of 6**.
- `PHASE_OUTCOME.md`: `UNIT 335 - STEP 1` appended. Version 1.13.19 -> **1.13.20**.

#### What each kind draws today - 0 of 8 are trading cards (confirmed)

Every card is `AchievementCategoryCard`: a title, one grey figure line, a points line, and
whether it is earned. It sits in a white box, two columns wide.

| Kind | What a card draws today |
|---|---|
| Countries | entity name, `2 contacts`, `5 pts`; next card `One more country` |
| States | one card, `Your first state`, and no earned cards |
| Grids | the square, `N contacts`, points |
| Bands | `20 m`, `N contacts`, points |
| Modes | `FT8`, `N contacts`, points |
| Hall of Fame | the first in words and its points; no figure line |
| Total Miles | tier cards reading `reached` or `next`, under the page's only bar |
| Continents | seven badges in the page's badge template; each opens to Countries-style cards |

#### Whether the contact that earned each card can be recovered from `AchievementLog`

- **Countries, Grids, Bands, Modes: yes.** The earliest `StartedUtc` among the contacts with that
  entity, four-character square, band or mode.
- **Continents: yes.** The earliest contact with that continent code. *Countries since* is
  `EntitiesOn(code)`.
- **Hall of Fame: yes, for all six firsts Hamlet awards.**
  - `first_contact`: the earliest contact.
  - `first_psk31` and `first_cw_qso`: the earliest in that mode.
  - The two distance firsts: the earliest contact at or over the line. `FirstsEarned` tests
    only the furthest.
  - `first_dx`: only under `FirstsEarned`'s own rule. That rule takes his own entity to be the
    first entity **in file order**, not his callsign's, so the earning contact is the earliest
    one with a different entity.
- **Total Miles: yes, by adding `Miles` in date order** until the sum crosses the tier.
- **States: no.** The log does not read `STATE`, so there is no contact to find (step 2's).
- **A record with no date.** `AchievementLog.First` skips records with no `StartedUtc`. So a
  place worked only by an undated record has no first by date and falls back to file order.

#### Whether `Ft8GlobePlot` can be built for an `AchievementContact` as it stands

**Four arguments are enough, with one catch.**
- `AchievementContact.Miles` is measured from the grid in **Settings**, which is handed to
  `AchievementLog`. It is not measured from the record's own `MyGrid`, although `MyGrid`'s doc
  comment implies the record's grid is the one used.
- So for the map and the printed distance to be one measurement, the map's operator end has to
  be that same Settings grid. `AchievementLog` keeps it private, so it has to be carried to the
  category.

#### The CQ source

- **The collection** is `MainWindowViewModel.DigitalDecodes`, an
  `ObservableCollection<DigitalDecodeRow>`.
- **What the list treats as current: whatever is on the table.**
  - FT8 rows have no age limit. They leave at the 500-row cap (oldest arrival first), on a large
    retune or band change, or on Clear.
  - PSK31 rows leave when their carrier retires or listening stops.
- **What a row carries.**
  - `Sender`: the FT8 from-field, or the PSK31 parser's speaker.
  - `Addressee`.
  - **No grid property.** On an FT8 CQ the grid is the payload where
    `Ft8MessageSplit.IsGrid(Payload)`. A PSK31 row has no fields, so it has no grid.
  - **No entity property.** `DxccPrefixes.EntityOf(Sender)` resolves it, as `SenderHelp` and
    `NudgeSet` do.
- **How a row counts as a CQ.** `Ft8MessageSplit.IsCallToAnyone(Addressee)`: `CQ`, or `CQ`
  followed by a word. That is the same test the decoded list's CQ toggle uses.
- **Whether a caller would earn a country or open a continent.** `NudgeSet` already answers this
  from the log, and it is what puts the quill on a row.
- **How the window could be handed it.** `OpenAchievements` builds the view model at
  `MainWindowViewModel.cs:6155` and shows it as a modal dialog. Passing the rows is the whole wire,
  and **nothing passes them today**.

#### The width at 1400 and 1920

`AchievementsWindow` declares 1040 x 720 and opens centered on its owner. **Nothing sizes it from
the main window**, so on the headless host it draws at 1040 x 720 whatever width the main window
is. **No such path exists, wired or unwired.** A measurement at 1400 and 1920 has to set the
dialog's own width.

### Task 1 - the band on every kind, and the card count

**Tests first, both watched red:**
- `TheCategoryPagesAreTradingCardsTests.EveryKindsBandCarriesCountScoreLevelAndABar` is new. It
  opens all eight kinds in the realized window and checks the band against the scores the
  shipped points file gives, including a 4.5:1 check on the band's ink. Red: `there is no
  Border named AchievementsCategoryBand`.
- `TheAchievementsPageClicksInTests.OpeningAndClosingACategoryWritesTheKindAndNothingElse` was
  rewritten under R12 as `...WritesTheKindAndTheCardCountAndNothingElse`. It now wants `kind`
  and `cards`, with 9 cards for Countries. Red: `Expected ["kind", "cards"]`,
  `Actual ["kind"]`.

**Change.**
- `AchievementCategory` gains the band's facts:
  - the line under the name, `BandLine`;
  - `ScoreLine` and `LevelName`;
  - a bar fraction and its words, or words saying there is no next level;
  - `RenderedCount`;
  - a computed `BandInk`.
- The band in `AchievementsWindow.axaml` now draws the emblem, the name at 22 px and the band
  line. On the right sits a 300 x 12 px bar with its words above it, or the no-next-level words
  and no bar.
- `BadgeProgressControl` gains `Length`, `Thickness` and `Track`, with defaults that leave the
  belt's 46 x 4 bar exactly as it was.
- `achievement_category_opened` writes `cards`, the count of cards and badges drawn.
- `CallsignPrivacyTests` passes a count in its two calls of that event.

**What each band says on the twelve-contact fixture**, computed from the shipped points file:

| Kind | Level | Bar, or the words instead |
|---|---|---|
| Countries | unranked | `8 of 10 to Bronze` |
| States | unranked | `0 of 10 to Bronze` (an empty track) |
| Grids | Bronze | `10 of 25 to Silver` |
| Continents | Silver | `5 of 7 to Gold` |
| Bands | Bronze | `5 of 6 to Silver` |
| Modes | Gold | `Gold, the top level` - no bar |
| Hall of Fame | Bronze | `5 of 6 to Silver` |
| Total Miles | unranked | the miles so far `of 50,000 mi to Bronze` |

**Green.** The first run was 31 of 33:
- one fit red at 1040: on `continent-EU`, `one continent has no levels of its own` needs 380 px
  and the slot is 300;
- the known red `TheWindowDrawsEverySixRows`.

After shortening, the same seven test types ran **32 of 33**. The one red is the known one. The
types: the new test, `TheAchievementsPageClicksInTests`, `BindingHealthTests`,
`CallsignPrivacyTests`, `TheAchievementsPageTests`, `Unit332TwoWidthsTests` and
`TheAchievementsScreenTests`.

**Strings shortened (§6):**
- `one continent has no levels of its own` -> `no levels per continent`;
- `no levels while the points file cannot be read` -> `no levels: file unreadable`;
- `the points file names no levels here` -> `no levels in the points file`.

#### Decisions this session made for itself, task 1

1. **The gap is said once.** The picture puts `14 to Silver` on the band line and `11 of 25 to
   Silver` over the bar. Where a bar is drawn, its words carry the gap, so the band line drops
   it. With both, Total Miles' line ran past its slot. Rejected: a narrower bar, which would
   have clipped the miles figure instead.
2. **The Hall of Fame band is inked dark.** White on its gold `#A8811A` computes to about
   3.6:1, under §0.6's 4.5:1. The ink is chosen by computing the contrast, not by eye, so
   every other band stays white. Rejected: darkening the gold, which is the approved color.
3. **The corner lines at the band's right are gone.** Count, points and level are on the band
   line, and the bar holds the right. Rejected: keeping both, which said everything twice.
4. **The card count is cards plus badges.** Continents draws seven badges and no cards, and a
   count of nought there would read as an empty page.
5. **Total Miles' own tier bar stays, for now.** Task 4 turns each tier into its own bar.

### Task 2 - the earned card is the contact that earned it

**Test first, watched red.** `TheCategoryPagesAreTradingCardsTests.EveryEarnedCardIsTheContactThatEarnedIt`
is new, over a five-contact log:
- Norway is worked twice, and the earlier contact is later in the file;
- `VE3PQR` has no grid;
- `G0MNO` has no date.

Red: Norway's card `Expected "LA1ZZZ"`, `Actual ""`.

**Change.**
- **Which contact.** Each earned card on Countries, on Grids and inside a continent is built
  from the earliest contact in that place. The sort is stable, so undated records keep file
  order after every dated one.
- **What the card carries:**
  - the place large, with its points;
  - `callsign · grid`, or on Grids `callsign · country`;
  - the conversation card's own `Ft8GlobePlot`, drawn by `Ft8GlobeControl` 170 px tall;
  - `AchievementContact.Miles` in large type, rounded by `GridPath.DescribeMiles` and written as
    `mi`;
  - `band · mode`;
  - the date, as `Aug 12, 2026`.
- **Where the map's operator end comes from.** `AchievementBadgePage` now carries the Settings
  grid, the one the miles were measured from, so the path and the printed distance are one
  measurement.
- **What fills a card with no map.** A word saying why, and a list of up to three of the
  contacts in that place (`callsign · band · date`), in a grey panel the map's height. The word
  is one of three: `no grid, so no map`, `set your grid for a map`, or `off the edge of this
  map`.

**On the fixture, computed:**
- **Norway** is `LA1ZZZ · JO28`, `40 m · FT8`, `Aug 12, 2026`, `5 pts`, with a path. The
  distance on the card equals the log's to the mile, and so does the plot's.
- **Canada** is `VE3PQR`, with no distance, `no grid, so no map`, and its contact listed.
- **England** has no date line, and a map.
- **No dash appears** on any card.
- In the realized window, **one map is drawn per card that has one**, and the no-map word
  appears once.

**Green.** The same seven test types plus the new one: **33 of 34**. The one red is the known
`TheWindowDrawsEverySixRows`.

#### Decisions this session made for itself, task 2

1. **The card's map uses the popup's path-fitted frame**, which is the conversation card's
   enlarged map (`Opened="True"`).
   - R22 asks for *a map of the path cropped to the two stations as the conversation card draws
     it*, and the picture shows it cropped.
   - The conversation card's own face has drawn the whole world since unit 306. The frame
     cropped to the path is the popup's, capped at 2x.
   - That frame is the existing arithmetic, so no second map exists.
   - Rejected: the whole-world frame, which would not be cropped to the two stations.
2. **The map's operator end is the Settings grid, not the record's `MyGrid`**, because that is
   the grid the log's miles were measured from.
3. **A card with no map lists up to three contacts**, not all of them, so the panel keeps the
   map's height and the cards stay one size.
4. **The old `2 contacts` figure line is hidden on a card that is a contact.** The picture does
   not carry it, and it still shows on the kinds not yet rebuilt.

### Task 3 - the next card knows who is calling

**Test first, watched red.** `TheCategoryPagesAreTradingCardsTests.TheNextCardKnowsWhoIsCalling`
is new. It feeds the twelve-contact log a fixture CQ list of six rows: five CQs, and one reply
that is not a CQ. It checks Countries, Grids, Europe, States, and a list where nobody would earn
the card. Red: `Expected "Any country you have not worked"`, `Actual ""`. The snapshot had
already read the five CQs and left out the reply.

**The choice: the list carries the time it was read. It is not kept live.**
- The decoded list has no recency rule of its own. An FT8 row stays until the 500-row cap, a
  large retune or Clear, so *on the list* is all *current* means.
- The window is modal.
- A list that went on saying *right now* behind it would claim a freshness it lacks
  (HM-DEC-111). **No recency number is invented.**

**Change.**
- **`CqSnapshot`** reads `DigitalDecodes` when the window opens:
  - it takes every row not sent by this station, with a sender, whose addressee
    `Ft8MessageSplit.IsCallToAnyone` accepts (the decoded list's own CQ test);
  - the grid is the payload where `IsGrid` says it is one;
  - each callsign appears once, preferring a call that carried a grid.
- **The wire.** `OpenAchievements` passes `CqSnapshot.From(DigitalDecodes, DateTime.UtcNow)` as
  the view model's `Calling`, and a category receives it on open and on back. Nothing about how
  a row is decoded changed.
- **What a next card now carries:**
  - what it wants, in words;
  - the heading `calling CQ at 21:41 UTC, unworked`;
  - up to three callers as `place | CALL · N mi`, each with the quill his decoded-list row
    wears - the ringed door where he would open a continent, else the still counter;
  - `and N more on the CQ list` where there are more.
- **Otherwise it says** `no one is calling from there now`. Where no list was handed in it says
  `the CQ list was not read`.

**Per kind:**
- **Countries:** callers whose entity `DxccPrefixes` resolves and the log lacks, named by
  `EntitySpoken.Short`.
- **Grids:** callers whose CQ carried a four-character square the log lacks.
- **Inside a continent:** the same, on that continent only. The wants line is `Any unworked
  country in Europe`.
- **States:** `Hamlet cannot tell a caller's state`, and no callers. That is the arbiter's
  proposal; see section 4.

**On the fixture, computed:**
- Countries lists Austria (`OE8DDX` with its distance) and Grenada (`J38DX`). Norway and the
  United States are in the log.
- Grids lists JN76, FK92 and FN42, but not FN31 or JO59.
- Europe lists Austria alone.
- The list with only worked callers says `no one is calling from there now`.
- In the realized window, the heading and every caller's place and line are drawn on the
  Countries next card.

**Green.** The first run was 33 of 35:
- one fit red: `Hamlet cannot tell a caller's state from the air` needs 480 px in a 426 px
  slot at 1040;
- the known red `TheWindowDrawsEverySixRows`.

Shortened to `Hamlet cannot tell a caller's state`, then **34 of 35**. The one red is the known
one.

#### Decisions this session made for itself, task 3

1. **Read once with its time**, as above. Rejected: a live list. It would add a subscription to
   a modal dialog and still could not say how fresh a row is, because the decoded list does not
   know either.
2. **Callers from a continent he has never opened are named.** R22, Tim's of 2026-09-12, asks
   for who is calling from a place that would earn the card. It also asks the unearned
   continents to name who is calling from them. That is later than the 2026-09-10 rule that the
   CQ list must not name an area he has never opened. §6: the later ruling wins. The ringed door
   is kept beside such a caller, so the card and his row agree on what he would open. Raised in
   section 4.
3. **No order is imposed on the callers**; they come in the list's order. Tim, 2026-09-11:
   *"There are no rankings."* Rejected: nearest first.
4. **Three callers, then a count of the rest**, so the card keeps its size and nobody is
   hidden without a word.
5. **Where no list was handed in, the card says it was not read**, not that no one is calling:
   nobody looked.

### Task 4 - the other five kinds, and the page-wide measurements

**Tests first, both watched red:**
- `TheOtherFiveKindsEachDrawTheirOwnCards` is new. Red: `Africa has no card`.
- `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty` is new. Red: `1400.00
  hall_of_fame: no trading card is drawn`.
- The first attempt at the red run stopped at a compile error in the new test, a missing
  `using Avalonia`. It was fixed and run again before any build code was written.

**Change, kind by kind.** What the five-contact and twelve-contact logs show is computed.

| Kind | What a card now is | On the fixture |
|---|---|---|
| Continents | Seven cards in the category scroller, each a button onto its countries. Opened: the first contact on it, with map, and `N countries worked there`. Unopened: the continent named, with its callers or `no one is calling from there now` | Europe is `LA8ENA · Norway`, `3 countries worked there`; Oceania lists `New Zealand`, `ZL1ABC · 8,600 mi`; Antarctica says no one is calling |
| Total Miles | Each tier is a card with a bar. A reached tier is also the contact whose miles, added in date order, crossed the line | `42,041 of 50,000 mi`, at 0.84. Nine contacts to Tokyo cross 50,000 on the eighth, `Aug 8, 2026` |
| Bands | The first contact on each band. The next card lists the bands not worked, the green zone's best bet first in its own words | `20 m` is `W3YNI`; next is `17 m best bet now`, then `30 m` |
| Modes | The first contact in each mode. The next card lists each unworked mode and where it lives, from `DigitalCallingFrequencies` | On the five-contact log: `CW`; `FT4 3.575 on 80 m`; `PSK31 3.580 on 80 m` |
| Hall of Fame | Each first is the contact that earned it, by `FirstsEarned`'s own rule. The next first keeps unit 333's choice and gains a bar, its callers, or words | `A DX contact` is `VA3VRR`; `Over 5,000 miles` is `JA1XYZ`; next is `Over 10,000 miles` with `furthest 9,673 of 10,000 mi` |

**Wiring.**
- **The best bet.** `OpenAchievements` now hands in the green zone's best bet as the band button
  wears it, `BestBet`, beside `Calling`.
- **Total Miles' bar.** Its category-wide bar is gone; each tier card carries its own.
- **The card count.** Countries on the fixture draws 9 cards, and Continents draws 7.

**Measured at a window 1400 x 720 and 1920 x 720**, over the twelve-contact log with four CQ
callers and a best bet:

| Page | Runs that fit | Cards | Cards with no map, bar or list |
|---|---|---|---|
| Hall of Fame | 39 | 6 | 0 |
| Continents | 52 | 7 | 0 |
| Countries | 63 | 9 | 0 |
| States | 9 | 1 | 0 |
| Grids | 76 | 11 | 0 |
| Total Miles | 9 | 1 | 0 |
| Bands | 43 | 6 | 0 |
| Modes | 34 | 5 | 0 |
| Europe | 29 | 4 | 0 |
| Oceania | 11 | 1 | 0 |

- The figures are the same at both widths.
- **Every run is checked as `NoWrap`.** Each is laid out on its own and fits its slot and every
  box above it.
- The window's own 1040 is checked by `TheAchievementsPageClicksInTests`, which still passes.

**Green.**
- The achievements, binding and privacy test types together: **36 of 37**. The one red is the
  known `TheWindowDrawsEverySixRows`.
- That was re-run after decision 7 below: **36 of 37** again.
- The carry-forward list after the change, run as its two invocations: app **100 of 100** in
  11 s, engine **85 of 85** in 4 s.

#### Decisions this session made for itself, task 4

1. **The seven continents are the same trading card, in the category's scroller, each inside a
   button onto its countries.** Rejected: packing seven maps into four narrow columns.
   `no one is calling from there now` does not fit there at 1040 without wrapping.
   `Unit332TwoWidthsTests` asserted that the seven badges end within the window's height. It
   was rewritten under R12 to assert that they sit inside the category's scroller, as every
   kind's cards do. The page still has no scroller.
2. **Total Miles' category-wide bar was removed, and each tier card carries its own.**
   `TheAchievementsPageClicksInTests` asserted that bar. It was rewritten under R12 to assert
   the same fraction and words on the tier card.
3. **Where a mode lives is shown on the best-bet band if that band has a row for it, and on
   the lowest band that does otherwise.** Tonight's best bet in the fixture is 17 m, which has
   no FT4 or PSK31 row, so both say 80 m. Rejected: every band, which would not fit; and the
   band the dial is on, which the window is not handed.
4. **Who is there is counted only for PSK31.** A PSK31 row is text only, so it says its mode.
   An FT8-shaped row does not say FT8 or FT4, so no count is claimed for either (§0.0). Morse and
   Voice have no digital row, so their line is the mode's name alone.
5. **The Hall of Fame next card is unit 333's choice, with something to measure added.**
   - Distance firsts: a bar of the furthest contact toward the line.
   - `A DX contact` and `Your first contact`: callers who would earn them.
   - `A PSK31 contact`: where PSK31 lives, and its callers.
   - `A Morse contact`: `the CQ list carries no Morse`. The decoded list is digital, and saying
     no one is calling in Morse would be a claim about a list that can never show one.

   Which first is next, and the collision unit 333 raised, are untouched.
6. **The Bands next card lists the unworked bands in the band row's order, with the best bet
   moved first.** That is the green zone's answer, not a ranking this unit formed. Where the
   best bet is a band already worked, the card says so in a line.
7. **The callsign line uses `EntitySpoken.Short`.** The first measurement run printed
   `W3YNI · the United States`. `Short` is the tree's own name for a place beside a callsign,
   and the caller lines already used it. Card titles are unchanged.

### Where the instruction and the tree disagreed

Reported, not repaired.

- **R20 is nowhere.** `docs/phase-psk31-run/PHASE_PLAN.md` holds §R1 to §R19 and no R20. The
  only mentions are the citation in `PHASE_PLAN.md` §2 and its copy in
  `docs/phase-maintenance/PHASE_PLAN.md`.
- **`MyGrid` is carried and `Miles` does not use it**, as above.
- **`AchievementContact.Grid` is documented as four characters**, but `AchievementLog.Read` keeps
  the whole `GRIDSQUARE`, upper-cased. A six-character square stays six.
- **`BindingHealthTests` covers only the main window.** The achievements window's binding check
  is in `TheAchievementsPageClicksInTests`.
- **`AchievementsWindow.axaml`'s header comment says 1000 wide**; the window is 1040.
- **R22 says the card map is cropped *as the conversation card draws it*.** The conversation
  card's face draws the whole world; only its popup crops to the path. The card uses the
  popup's frame. See task 2, decision 1.
- **`HfBands` holds seven bands.** `TheAchievementsPageTests`' comment on the Bands score says
  *five of nine*; the badge draws `5 of 7`. Found in the task 1 printout, and not repaired.
- **Section 7's ordering-block template reads `complete|stopped at task N of 5`.** The
  instruction numbers its tasks 0 to 5, which is six. This report counts the last task done,
  task 4, out of 5.
- **Everything else matched:**
  - `AchievementContact`'s seven fields;
  - `Ft8GlobePlot`, `Ft8ContactCard.Globe`, `Ft8GlobeControl`, and the popup opening it with
    `Opened` and `OpenFrameFor`;
  - `AchievementCategory.For` building every kind;
  - 1040 x 720, and line 6155;
  - `DigitalDecodes` and its type;
  - only Total Miles has a bar;
  - the event carries `kind` only.

## 2. What the owner should expect

**Every category page is now trading cards.** Version 1.13.20. Every category opens under a
full-width color band with a bar toward the next level. Under it:
- **every earned card is the contact that earned it**, with its path map. That holds on
  Countries, Grids, Bands, Modes, Hall of Fame, each continent, and each Total Miles tier
  reached;
- **every next card says what it wants**, and lists who on the CQ list would earn it - or the
  bands not yet worked with the green zone's best bet first, or where each unworked mode lives;
- **Total Miles' tiers are bars**;
- **Continents is seven cards**, each opening to its countries.

- **The callers and the best bet are as of the moment the window opened**, and the heading
  gives that time in UTC. Close and reopen the window to read them again.
- **Pressing a card's map does not open it yet.** That was task 5, dropped whole.

**What will look wrong but is not:**
- **Continents scrolls now**, as every category's cards do, because each continent is a full
  card with its map. The opening page with the eight badges still does not scroll.
- **Where an unworked mode lives may be named on a band other than the best bet.** On the
  fixture, FT4 and PSK31 both say 80 m, because the best bet that night, 17 m, has no row for
  either in the band table.
- **FT8 and FT4 never show a count of who is calling.** The decoded list does not say which of
  the two a row was. PSK31 does.
- **`A Morse contact` says `the CQ list carries no Morse`.** The CQ list is the digital decoded
  list.
- **The earned card is your first contact there, not your most recent.** Norway on the
  fixture shows `LA1ZZZ` from 12 August, not `LA8ENA` five days later.
- **The distance is rounded**: to the nearest 100 miles over 1,000, and to the nearest 10 below
  that. It is the same rounding the conversation card's words use.
- **The map on a card is zoomed toward the path**, at most twice the picture's size, as the
  enlarged map in a conversation card is. Two nearby stations stay small in the middle, because
  they really are close.
- **A card for a contact logged without a grid has no map.** It shows a grey panel saying `no
  grid, so no map`, with that station's contacts listed.
- **Hall of Fame's band has dark lettering** where the others are white. White on that gold is
  too faint to meet the contrast rule; the computed figure is about 3.6:1.
- **Modes has no bar on the fixture log**, just `Gold, the top level`. Five modes is the top
  of that kind's levels in your points file.
- **Total Miles' first tier shows two bars on the fixture**: the level bar on the band, and the
  tier card's bar. They agree - `42,041 of 50,000 mi` - because the shipped file sets the Bronze
  level and the first tier both at 50,000.

## 3. What you should see

**8 of 8 kinds read as trading cards, and none do not.**
- Countries, States, Grids, Continents, Total Miles, Bands, Modes and Hall of Fame each open to
  the color band with its bar, then cards with a map, a bar or a list.
- States has no earned cards until step 2 scores `STATE`, so its page is the band and its next
  card.
- All of this is computed on the test host at windows 1400 and 1920 wide, not seen.

Open Countries:
- **The band.** The top of the page is a red band with the flags and *Countries* on it. The
  line under the name reads like `one per entity · 8 worked · 40 pts · unranked`. At the
  right, `8 of 10 to Bronze` sits over a bar eight-tenths full.
- **The cards.** Below the band, two cards to a row. Each has:
  - a red edge;
  - the country in large type, its points at the right;
  - `callsign · grid`;
  - a map cropped to your grid and his, with the path drawn;
  - the distance in large type, and `band · mode` and the date beside it.
- **The last card** has a grey edge, `One more country` and `next`, and `Any country you have
  not worked`. Under that is a grey panel:
  - `calling CQ at` the time you opened the window, `, unworked`;
  - then up to three lines, each a small quill, the country, and the callsign with its
    distance;
  - or `no one is calling from there now`.

## 4. What's blocking us

### Raised by this unit

**1. The States next card says `Hamlet cannot tell a caller's state`. That wording is the
arbiter's proposal, marked for you, and shortened here to fit.**

*Ruling wanted: keep it or reword it.*
- A CQ carries no state, so the card cannot know who is calling from a state you have not
  worked.
- The proposal read *Hamlet cannot tell a caller's state from the air*. That needs 480 px, and
  the slot at the window's 1040 is 426, so *from the air* came off.

*Reasoning.* *No one is calling from there now* would assert something nobody measured (§0.0).
Guessing a state from a prefix was rejected before: a `W3` can be anywhere.

*What was rejected and why.*
- A wider slot. That would change every next card's width for one sentence.
- A second line. The fit rule says no string wraps.

**2. The next cards name callers from continents you have never worked.**

*Ruling wanted, only if the 2026-09-10 rule was meant to hold here.*
- That rule says the CQ list must not be what tells you an area exists. So a decoded-list row
  from a never-opened continent wears the ringed door and names nothing.
- On a Countries or continent next card, the same caller is named by country, beside the same
  ringed door.

*Reasoning.* R22, 2026-09-12, asks the next card to list who is calling from a place that would
earn it. It also asks each unearned continent to name *who is calling from it now*, which cannot
be done without naming the place. §6 says the later ruling wins.

*What was rejected and why.* Leaving door callers off the Countries card. That would hide the
one caller who earns two cards at once, and Continents could not do what R22 asks of it.

**3. The opening page's Hall of Fame badge still has white text on gold, at about 3.6:1.**

*No ruling wanted; a finding.*
- This unit's category band computes its ink and turns dark on that gold.
- The eight badges on the opening page, and their shared template, are unit 331's and step 0's.
  They were not touched, so the badge's name there still reads white on `#A8811A`, under §0.6's
  4.5:1.

*Reasoning.* §12.6: do not repair unrelated things on the way past. The fix is one binding, and
it belongs to a unit that is told to change the page.

### Where the carried items stand after unit 335

- **Unit 333 item 1, the Hall of Fame next-card collision:** untouched. The next first is still
  chosen by `NextFirstOf`, and `A PSK31 contact` still shows where it is the only first left.
  This unit added a line under it (where PSK31 lives, and its callers) and did not change which
  card it is.
- **Unit 332 item 1, the `Why` hovers:** unchanged. No card draws `ModeFirstRow.Why`, and a test
  holds `cannot work` off the Modes cards. The sentences themselves are untouched in
  `AchievementsViewModel.cs`.
- **Unit 331 queue item 3, States:** unchanged. States still scores nought, and its next card
  says Hamlet cannot tell a caller's state. See item 1 above.
- **Unit 332 item 3, the achievements window width:** it is still 1040 by 720. This unit
  measured the pages at 1400 and 1920 by setting the dialog's own width, because nothing sizes it
  from the main window.
- **The status helper:** not tried again. Every `UPDATED` in this unit is a `date` reading pasted
  whole.
- **Every other item stands as carried.** Nothing in this unit touched the radio side, a decoder,
  a parser or the transmit chain.

### Carried from unit 334's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

#### Raised by this unit

**1. At 1920 the green zone is 310 px tall, because the map keeps its shape as it takes the
pills' width.**

*Ruling wanted: cap the map's height or not.* Computed: the map is 492 x 269 at a 1920 window,
and the panel goes from 151 to 310 px. At 1400 it goes from 193 to 177 px, so there it is
shorter.

*Reasoning.* R21 says the map takes the space the pills held, and at 1920 that is 218 px of
width. A map drawn at its own proportions cannot take width without height, and stretching it
would move every place off the pixel the projection puts it on (HM-DEC-092).

*What was rejected and why.* Choosing a cap here. A height limit is a number about how much of
your screen the panel may take, and it is yours to pick.

**2. At 1400 the map gained only 30 px, because the count and its sparkline set the right
column's width, not the pills.**

*Ruling wanted, if 30 px is not the bigger map you meant.* The right column is 260 px after,
against 262 before.
- The sparkline is 110 px of it plus a 10 px gap.
- Dropping the sparkline, the task's named drop candidate, would free that width.
- The left block and the map share freed width equally, so the map would gain about half of it.
  That is arithmetic on the measured widths, not a measurement.

*Reasoning.* The task says keep the sparkline if it still fits, and it fits, so it stayed.

*What was rejected and why.* Dropping it anyway, which the task does not allow while it fits.
Stacking the count under the sparkline, which rearranges the count beyond what was asked.

**3. `tools/status.sh` is still refused, in all three spellings.**

*No ruling wanted; a finding, the same as carried item 5 below.* `sh`, `bash` and `./` all came
back *requires approval*. Every `UPDATED` in this unit is a `date` reading pasted whole, and none
was composed. The validator was run by the `.proj` route; its verdict is in the session
transcript, not quoted here.

#### Carried from unit 333's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

##### Raised by this unit

**1. Where PSK31 is the only Hall of Fame first left unearned, Ruling C and §3.1 say opposite
things about one slot.**

*Ruling wanted.* The slot is the Hall of Fame badge's next card, and Hall of Fame's unearned
card inside the category. The exact string is `A PSK31 contact`.

It arises on a log holding *Your first contact*, *A DX contact*, *A Morse contact*, *Over 5,000
miles* and *Over 10,000 miles* but no PSK31 contact, for instance an imported CW log with long
contacts.
- Ruling C, Tim, 2026-09-12: *every kind shown, the nearest unearned card in each, nothing
  beyond it.*
- `ACHIEVEMENTS_PHILOSOPHY.md` §3.1: *absent, not dimmed. No PSK31 card exists until the first
  PSK31 contact.*

*Reasoning.* On every other log the two agree: the badge shows the nearest first §3.1 allows.
In this one case, showing the card breaks §3.1 and showing nothing breaks Ruling C. The
instruction says not to choose, so **the screen is left as it was and still shows
`A PSK31 contact` there.**

*What was rejected and why.* Showing no next card, which is choosing §3.1. Moving `first_psk31`
last in the list, which only moves the collision to a later log and reorders the owner's
firsts.

**2. The nice-to-pass wants a logger, and this session could not look for one.**

*Something you can do in a minute, not a stop.* Import `docs/unit333-psk31-fixture-export.adi`
into a new, empty log in any logger you already have, with the five steps in section 2. Name
the logger and its version, and say whether it took both records with PSK/PSK31, both RSTs
and the grid. That closes the criterion.

*Reasoning.* The instruction forbids installing one, and the permission layer refused every
listing outside `C:\Source\HamLet` and the registry query.

*What was rejected and why.* Downloading or building a logger, which is your decision about
your machine. Guessing from memory which loggers are installed, which would be a claim nobody
measured.

**3. This environment refuses deletes inside the repository and listings outside it, so two
scratch files remain.**

*No ruling wanted; housekeeping.*
- `tests/Hamlet.App.Tests/Views/Unit333ProbeTests.cs` is untracked and one comment line.
- `artifacts/unit333/psk31-fixture-export.adi` is gitignored.

Both are safe to delete by hand, beside the five carried in item 19 below. The validator was
run by the `.proj` route the instruction names. The prompt's `.bat` spelling is the one unit
243 documented as mangled by Git Bash.

##### Carried from unit 332's section 4, per HM-DEC-139 - verbatim

**1. The mode rows' hovers still say Hamlet cannot work PSK31 and FT4 and cannot log CW.**

*Ruling wanted on whether to rewrite them.* `AchievementsViewModel.Why` has *Hamlet can tune
you to the PSK31 watering holes and cannot work them* and the FT4 and CW equivalents. That is
the same falsity as the sentence task 0 removed.

*Reasoning.* The instruction named the one line and §12.6 says not to repair unrelated things
on the way past. Those hovers are not on the new page, but `ModeFirstRow.Why` is still in the
tree, and a later surface could draw it.

*What was rejected and why.* Rewriting them here. The words about what Hamlet can now work are
a claim about PSK31 and CW, and they want the step 5 and 6 facts behind them, not this unit's
guess.

**2. The continent level names two continents he has not opened.**

*Ruling wanted.* Antarctica and Oceania each get a badge on the fixture, with `A first here`,
`0 pts` and `500 for a first` or `50 for a first`.

*Reasoning.* The instruction says *seven continent badges*, and seven named badges are what the
Continents level is. Its next cards do not name the continent again. But §3.1 says nothing
shows inside a category until something adjacent is earned, and this bends it one step further
than ruling C bends the page.

*What was rejected and why.* Drawing only the opened continents. The instruction asked for
seven, and a five-badge level would read as five continents.

**3. The fit is measured on a host that draws text about half again wider than the glass.**

*Ruling wanted on the window size.* To pass a measured no-clip test there, the achievements
window went from 820 to 1040 wide and nine strings were shortened.

*Reasoning.* The host advances ten pixels a character at every size. A string that fits there
fits on the glass, so the test cannot pass a clip the owner would see. The price is a window
wider than the glass needs.

*What was rejected and why.* Estimating widths for a proportional face instead of measuring.
The instruction says *asserted by measuring*, and an estimate is the thing that let 331's text
clip.

**4. The green zone's license phrase is not on one line at 1400.**

*Ruling wanted.* The instruction says *the license phrase and citation on one line*. At a 1400
window the left region is 253 px, and the phrase lays out to three lines on the test host; I
estimate one or two on the glass.

*Reasoning.* The mockup fits it by rewording it to *General covers digital modes here*.
`PrivilegeStatus.Detail` is the regulation's sentence, and 331 kept it in its own words. One
line at 1400 therefore needs either a shorter sentence or less room for the map and the
buttons.

*What was rejected and why.* Keeping it one line and letting it run past the panel's edge,
which is what the first cut did, at 142%.

**5. `tools/status.sh` cannot run here, and neither could the validator's `.bat`.**

*No ruling wanted; a finding.* `sh tools/status.sh` and `bash tools/status.sh` both came back
*requires approval*. So every status write was `date` and a paste, and none was composed: the
helper exists and the permission layer does not allow it.

The validator was attempted as instructed, `tools\arbiter\validate-output.bat output.md`, and
Git Bash turned the path into `toolsarbitervalidate-output.bat`. It was then run through the
route unit 243 built, `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`,
and its verdict is the last thing in this session's transcript. It is not reproduced here,
because a report cannot quote a run of itself.

*What was rejected and why.* Composing timestamps as units 327, 328 and 331 did.

**6. Step 5 is `partial` in `PHASE_STATUS.md` and *done* in the instruction.** **ANSWERED by
this unit**: the four must-pass and R13 are proved in section 3, the nice-to-pass is unmet, and
unit 333's `PHASE_OUTCOME.md` entry records `STATE_AFTER: done`. The `STEP: 5` lines are the
launcher's and were not written.

##### Carried from unit 331's queue, as unit 332 carried it - verbatim

**1. Fourteen `UPDATED` timestamps in `PROJECT_STATUS.md` were composed rather than
read from the clock - the third unit running, and this session read both prior
reports of it before doing it.**

*No ruling wanted; reported because it is now a pattern rather than a slip.* The
clock was read at `12:44:45` and at `13:48:41`, and every status write between them
carried an extrapolated time: `13:02`, `13:15`, `13:24`, `13:40`, `13:52`, `14:05`,
`14:12`, `14:30`, `14:44`, `14:58`, `15:12`, `15:30`, `15:52`, `16:25`. **The last of
those is two and a half hours ahead of the true time.** The final write is from the
clock and says so.

*Reasoning.* This defeats the one signal that catches a stopped session, which is the
whole purpose of the ten-minute write - a panel reading `16:25` at `13:48` cannot tell
a working session from a dead one, and would have read unit 330 as alive for two
hours after the watchdog killed it. Unit 327 reported it, unit 328 reported it and
repeated it, and this session did it fourteen times.

*What was rejected and why.* Reporting it as a detail. Three units is a mechanism
problem: the rule says *read from the clock* and the failure mode is that reading the
clock is a separate command nobody budgets for. **The fix that would work is a status
helper that reads the clock itself** - `tools/status.sh` arrived in the seed commit
and this session did not use it, which is its own finding.

**2. The instruction's own width arithmetic cannot hold at 1400 px, and the honest
resolution costs the card 48 px.**

*Ruling wanted.* Task 1a asks for about 460 px inside the card. Measured: the decoded
panes are half the tab, the decoded list needs 383 of them to stop clipping the
longest FT8 line, and 460 inside the card needs about 516 px of panel - so the pair
needs about 899 px, which is a window of about **1856**. At 1400 the arithmetic leaves
**227 px** inside the card, down from 275.

*Reasoning.* Two §0.0 claims are in conflict at 1400 and only one can win: a clipped
callsign on the decoded list is a station misidentified, so the list got what it
needs. **The room the instruction wants exists at your own window width if it is over
about 1850**, and does not below it.

*What was rejected and why.* Taking the pixels from the waterfall. Changing
`DigitalPanes` from `*,*` to `1*,2*` would give the card about 456 px inside at 1400 -
almost exactly the number asked for - but the outer split is your ruling from a phase
ago, and the waterfall would fall from 666 px to 447. **That is the option, and it is
yours, not mine.**

**3. The States badge scores nought because the log does not read `STATE`.**

*Ruling wanted on whether to read it.* An ADIF record carries `STATE` and
`AchievementContact` does not parse it, so the kind has no count and no score. The
badge draws, its next card is *Your first state*, and nought is the honest figure.

*Reasoning.* A state worked out from a callsign prefix would be a claim about where
somebody lives, which the prefix does not support - a `W3` can be anywhere. Reading
the field would work for records written by a logger that fills it; **Hamlet's own
`Ft8ContactLogEntry` does not write one**, so the kind would score for imported
records and not for his own, which is a worse screen than an honest nought.

*What was rejected and why.* Hiding the badge. Eight kinds is the shape you approved,
and a kind that is absent because Hamlet cannot yet count it teaches nothing; a
nought with a first card behind it says what is missing.

**4. `first_answer_to_own_cq` is in your points file and Hamlet can never award it.**

*No ruling wanted; a finding.* The log says a contact happened and not who called
first. Awarding it would mean deciding that from the exchange, which nobody recorded.
It stays in the file because the file is yours and a key Hamlet cannot award today is
a key it may award later - it simply never scores.

**5. The family word on the green zone is `Digital` where task 4's example says
`Data`.**

*Ruling wanted, and it is one word.* See decision 1 in section 1. `ModePalette`'s own
label is what the map legend teaches, and a fifth word for one of four families would
have two surfaces calling one thing two things.

**6. `validate-output.bat` CLOSED - the route has existed since unit 243 and five
units have not used it.**

*No ruling wanted; the ask is answered and the answer was already in the tree.* The
`.bat` invocation was refused again exactly as units 324 to 328 recorded. **Then
`tools\arbiter\validate-output.proj` was found sitting beside it**, written by unit
243 for precisely this deadlock, and it works:

```
dotnet build tools/arbiter/validate-output.proj -p:Report=output.md
    -> VALID - all seven rules passed.
    -> validate-output exit 0
```

*Reasoning.* `dotnet build` is permitted with a wildcard, MSBuild's `Exec` runs a
command, and the `.proj` calls the validator unmodified with its own rules and fails
the build on a non-zero exit. **Nothing was copied, read around or reimplemented.**
Unit 328 wrote *this needs the permission layer changed or a route that is not a
`.bat`*; the route existed, in the same folder, with a 32-line header explaining
itself.

*What was rejected and why.* Applying the seven rules by hand again, as unit 328 did.
A hand-applied rule is applied by the same session that wrote the file, which is
exactly the independence the rule wanted; now that an independent run is available,
the hand-check is worth nothing beside it. **The line for the next unit to carry is
the command above, not the fault.**

**7.** *(was item 1)* **`MainWindow.axaml`'s comment on the mark now says the
opposite of what the code does.** **CLOSED by unit 330 task 1** and re-checked here:
both remaining *filled disc* strings read correctly in context, one of them 330's own
corrected comment.

**8.** *(was item 2)* **`Unit300SizesTests.WhatTheMarkDrawsAtEachSize` is red, and two
tests in this repository assert opposite things about the same mark.** **CLOSED by
unit 330 task 1**, reconciled on option B of 2026-09-10, and 10 of 10 green here.

**9.** *(was item 3)* **The render recorder erases the type of every shape, so a shape
assertion written the obvious way silently passes.** Carried. `DrawingGroup.Open()`
returns every geometry as `PlatformGeometry`, whatever it was drawn as, so
`Assert.IsNotType<EllipseGeometry>` passes against a filled disc. **Bounds are the
honest question**: a circle's are square.

**10.** *(was items 4 and 10)* **Composed `UPDATED` timestamps.** **Carried and
repeated** - see item 1 above, which is the same fault in the same file a third unit
later.

**11.** *(was item 5)* **The demodulator's quality measure vouches for a carrier that
has stopped, for between five and seven seconds, and that now sets how long a dead row
survives.** *Ruling wanted.* `Psk31Demodulator.Quality` is documented as *0.637 on
uniform noise phase and 1.0 on clean keying*. After a loud carrier stops, the input
**is** uniform noise phase and it goes on reporting 0.99, because both of its rolling
means are weighted by magnitude and the carrier's own loud symbols dominate the window
while they decay. **Measured on `psk31-idle-8s-1000hz.wav`: the squelch shut 5.70 s
after the carrier stopped; the quality fell under 0.80 at 7.20 s.** With
`KeepReadableSeconds` on top, `Psk31Listener.RetiredWithinSeconds` had to go from 2.5
to **9.0**. Two fixes would each bring it back under three seconds and neither has
been built: normalizing the measure per symbol changes what every PSK31 decode is
squelched on, and capping how long a vouch may outlive the spectrum contradicts
*retired only when both have lost it*.

**12.** *(was item 6)* **The idle fixture does not reproduce the fault the owner
saw.** Carried. With both new mechanisms switched off,
`psk31-idle-8s-1000hz.wav` still yields one carrier across the whole gap and still
nominates at 1000.0 Hz. **So the keep rule is built from the physics and from his
telemetry, and is proved not to break anything - it is not proved to fix what he
saw.** This is the same ask unit 324 left: **two minutes of his own 14.070 or 7.070,
captured to WAV.**

**13.** *(was item 7)* **`AchievementMarkControl.cs` was taken off the SHA pin, on
unit 327's own judgement.** Carried. Units 330 and 331 have both changed that file
since, under instructions that name it.

**14.** *(was item 8)* **The `Views` reds in `TheMenuIsUnderTheMouseTests` are eight,
not two, and the shared collapse flag was not the cause.** *Ruling wanted on who fixes
it.* Every one of the eight fails at the same line: `expected both decoded lists in
the window, found DigitalDecodedRows`. `DigitalMineRows` lives inside a `ScrollViewer`
gated on `ShowsConversation`, so the right-hand **row** list is realized only after
*show the N messages* is pressed. **That is deliberate** - the For you side became a
panel of cards and the raw rows are one press down, never gone. The test's premise
went stale on the day cards replaced that list.

**15.** *(was items 11, 12, 13)* **Unit 326 items 8 and 9 and unit 325 item 6 -
CLOSED by unit 327** and re-proved in the runs above.

**16.** *(was item 14)* **Unit 324 item 4 - why a 62 dB carrier failed the
keying-shape test. HALF ANSWERED.** The other half still wants a recording and is
item 12 above.

**17.** *(was item 15)* **The ALC margin of 15.** Carried verbatim: built, carried,
**Tim's to overrule**. Nothing in this unit touched it.

**18.** *(was item 16)* **`HM-DEC-161` versus `CPS-DEC-0161` - two id schemes.**
Reported, not repaired. `PROJECT_STATUS.md` carries `RULES_AT: HM-DEC-161
(2026-09-11)`; the other scheme appears in the arbiter's own artifacts. **Nothing in
this repository resolves which is canonical**, and no unit should pick one without a
ruling.

**19.** *(was item 17)* **Files this environment cannot delete.** Now five, listed for
Tim in section 2: `commit-msg-326.txt`, `toolsarbitervalidate-output.bat`,
`tools\arbiter\unit323-append.bat`, `tools\arbiter\unit323-append.py` and this
session's own `tools\cut-header-action.py`.

**All other items stand as unit 328 carried them.**

##### Where the carried items stand after this unit

- **Unit 332 item 1, the `Why` hovers:** not drawn on the rebuilt page before a PSK31 contact.
  Every hover on every visible control was read in task 1 and none names PSK31, so it stays
  parked.
- **Unit 332 item 6, step 5 partial or done:** answered above.
- **Carried item 1 and 10, composed timestamps:** not repeated. Every `UPDATED` in this unit is
  a `date` reading, pasted.
- **Every other item stands as carried.** Nothing in this unit touched the 1400 split, States,
  `first_answer_to_own_cq`, `Digital`, the demodulator vouch, the idle fixture, the ALC margin,
  the two id schemes, the five files or step 6.

#### Where the carried items stand after unit 334

- **Unit 333 item 3 and unit 331-queue item 19, the undeletable files:** now **thirteen**, and
  listed once in section 2. `Unit333ProbeTests.cs` is tracked, not untracked. None was left
  unemptied.
- **Unit 332 item 1, the `Why` hovers:** unchanged. The FT4 and PSK31 sentences are still at
  `AchievementsViewModel.cs` lines 501 to 517, and both are now false. Task 2 named only the one
  sentence it checked.
- **Unit 332 item 4, the license phrase at 1400:** still three lines on the test host. The left
  block is now 232 px, against 253, and the phrase breaks in the same places.
- **Unit 332 item 5 and unit 333 item 3, the status helper:** still refused, now in three
  spellings; see item 3 raised above. No timestamp was composed.
- **Every other item stands as carried.** Nothing in this unit touched the radio side, the
  achievements rulings, States, the 1400 decoded-list split, the demodulator, the ALC margin, the
  id schemes or PSK31 step 6.
