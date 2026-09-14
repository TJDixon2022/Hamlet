# What to look at, and your verdict - step 3 of *The screen, done right*

Unit 349. Every number here comes from a test that ran green in this unit at `85437c2`, with the
trace added at `40c297d`, or from a named report's commit and item. **Nothing here was seen by anyone.**

## 1. How to look

**Every appearance claim here is computed on a test host, not seen. Your window is the first eyes on
it.**

- **Your window size is not in the record.** Everything was measured at two widths, **1400 and 1920
  px**. The main window was 1040 px tall. **Please put your window size in your verdict.**
- **The achievements window opens at its own size, 1040 by 720** (`AchievementsWindow.axaml:9`).
  Nothing sizes it from the main window. Its pages were measured by setting it 1400 and 1920 wide,
  720 tall, so widen it to compare.
- **The test host draws text about half again wider than your screen does** (unit 332, carried in
  `67fe28c` section 2). Where the host says a line fits, it should fit for you. Where it wraps at
  1400, it may not wrap for you. That second half is an inference.
- **The words and counts are the tests' own logs, not yours.** The logs are twelve contacts; a
  state log; a licensed General operator on 14.074 MHz; four stations calling CQ at 21:41 UTC; and
  17 m as the best bet. Your log and the air will draw different words and numbers in the same
  places.
- **Hold these beside the screen:** `assets\main-screen-mockup.png` for the main window,
  `assets\achievements-opening-mockup.png` for the opening page, and
  `assets\category-page-countries.png` for every category page.

## 2. Page by page

### 2.1 The main window, on FT8

**Get there:** start Hamlet, open the Digital tab, and choose FT8.

**What R26 promises:** *The top row is one band, about 190 px tall at 1920, and the working panels
below the tabs take the rest of the window.* The panels never get less than half the height below
the band pills. The neighborhood card carries the band strip, the green block and the world clock,
*about 246 px wide, with the operator's dot only*. *The rig display is the same height as the
neighborhood card*, with drive and the power offer under the S-meter. Below the tabs, *waterfall,
decoded text, For You, all the same height, full to the status bar*. *At 1400: the same shape; where
the card's facts cannot sit beside the map they go under it; no callsign is ever clipped.*

**What the tree draws, licensed, FT8:**

| | 1400 | 1920 |
|---|---|---|
| Top row (neighborhood card and rig panel) | 216 px, 0.237 of the 910 px below the pills | 190 px, 0.209 |
| Neighborhood card / rig panel | 808 × 216 / 546 × 216 | 1328 × 190 / 546 × 190 |
| Green block | 518 × 88 | 1038 × 55 with no best bet; 1038 × 64 with one drawn (`2077432a`) |
| World clock | 246 × 134, one dot, 15 px from the card's right edge | the same |
| The three panels, readiness strip hidden / showing | 477 px (0.524) / 424 px (0.466) | 503 px (0.553) / 450 px (0.495) |
| Waterfall · decoded text · For You, wide | 474 · 378 · 475 | 734 · 378 · 735 |
| Where the panels end | y 953, the working card's floor; then 25 px to the status bar's top at y 978: the card's 12 px padding and 1 px border, then the status bar's 12 px margin, and nothing stands in it (`437cedd8`) | y 953, and the same 25 px |
| The conversation card's facts (no license class) | under the map: 419 px inside, and beside would need 568 | beside the map: 678 px inside, the facts at x 232 |

- **The green block's text, the same at both widths** (`2077432a`): *20 m* at 20 px is the largest. Then *14.074
  MHz* and *Digital · FT8 · yours to use* at 13; *6 stations* at 15; the license line, the rule of
  thumb and *heard just now* at 11; *last minute* at 10.
- **At 1400** (`2077432a`): the sparkline is hidden, and *heard just now* stands over *6 stations*. The license
  line takes 3 lines and the rule of thumb 2. *Digital · FT8 · yours to use* sits on its own line
  under the band.
- **The decoded list** is 378 px wide at both widths. Its longest line, *VP2MAA/P KC3QIS R-09*,
  measures 200 px in a 200 px cell.
- **CQ and Stop** are right of the tabs. Stop is 74 × 22 and inside nothing that folds. *everything*
  and *CQ* stay on screen with the list empty and with Decoded text folded.
- **With no license class** (plain), at 1400: the top row is 217 px (0.238), and the panels are 476
  hidden and 423 showing.

**Look for with your eyes:** whether the 11 px license and rule-of-thumb lines read comfortably, and
whether the green block and the clock balance each other. Check that the waterfall is not too narrow
at 1400. Check the colors against the mockup, and whether it is *not white bread boring*.

*Source: `TheTopRowTests` 14 of 14 and `TheWorkingPanelsTests` 8 of 8 at `2077432a`, three runs,
printed in `testresults\u353-t1-run1.trx` to `-run3.trx` (work instruction 353); first measured
at `85437c2`, `testresults\u349\u349-step0.trx`. The top row, block, panels, rule and sparkline:
`AtFourteenHundredTheLicensedTopRowIsTheMockupsShare`. The card, rig panel and panel sizes:
`AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest`. The clock:
`TheWorldClockIsAtTheCardsRightEndWithOneMarker`. Text sizes and wraps:
`TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest`. The floor:
`TheThreePanelsShareOneTopAndOneBottom`. The list: `TheDecodedListIsAsWideAsItsLongestLineNeeds`
and `NoCallsignIsClipped`. Facts under at 1400: `AtFourteenHundredTheSameShapeHolds`. Facts beside at
1920: `AtNineteenTwentyTheCardsFactsSitBesideTheMap`, which printed its numbers on PSK31. Stop and the
filter: `StopNeverCollapsesAndTheFilterStaysOnAnEmptyOrCollapsedList`.*

### 2.2 The main window, on PSK31, with the power offer not yet answered

**Get there:** on the Digital tab, choose PSK31 before answering the power offer.

**What R26 promises:** the rig display *carries under the frequency and S-meter the transmit drive
and the RF power offer, so no empty column stands under it*, and the top row keeps its height.

**What the tree draws, licensed, PSK31:**

| | 1400 | 1920 |
|---|---|---|
| Top row | 228 px (0.251) against 238.4 with the best bet drawn on the band you are on, on another band (40 m) and absent; pinned three ways on the test window (`1faf33a6`) | 190 px (0.209) all three ways |
| Rig panel / neighborhood card | 546 × 228 / 808 × 228 | 546 × 190 / 1328 × 190 |
| Green block | 518 × 100 all three ways | 1038 × 76 with the best bet drawn, on your band or 40 m; 1038 × 67 absent |
| The three panels, strip hidden / showing | 465 px (0.511) / 412 px (0.453) all three ways | 503 px (0.553) / 450 px (0.495) all three ways |
| The offer line | *RF power 50 % offered*, 218 × 13, on the drive note's row, 52 px under the rig display | the same |

- **Click the line** and the popup holds the offer unchanged: *PSK31 sends a steady carrier, so it
  runs warmer than voice. Hamlet can set your radio's transmit power to 50% for you. Nothing else on
  the radio changes, and nothing is set unless you press this.* It also holds *Set my power to 50%*,
  *I will set it myself*, and the ALC reference line.
- **The line's ink** is 4.61:1 on the rig panel's fill.
- **The green block adds** *PSK31 lives at 14.070; you are at 14.074*, because the test tunes FT8's
  dial and then chooses PSK31 (unit 341 item 2).
- **When the best bet draws** at 1400, *best bet now:* stands over the band, *20 m ✓* or *40 m*, and
  the top row stays 228 px (0.251) against a limit of 238.4, the same as with no best bet. It was 247
  px on your band and 237 on another before work instruction 351 (`1faf33a6`, U11).

**Look for with your eyes:** whether you notice the line as something to press, and whether one
click more to reach *Set my power to 50%* is acceptable. Check where the popup opens, and whether the
muted ink is readable on the amber panel.

*Source: `Unit341TraceTheOneLineOfferAndThePsk31GreenBlock` (line, ink, block),
`DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop` (the line's place and the popup's
words) and `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare` (top row, panels), all in
`TheTopRowTests`, 14 of 14 three times at `2077432a` (work instruction 353). The best-bet numbers are `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays`
and `TheTopRowAndThePanelShareHoldWithTheBestBetOnAnotherBand` at `1faf33a6` (work instruction 351).*

### 2.3 The achievements opening page

**Get there:** from the Radio menu choose *Achievements…*, or press the achievements mark, which opens
the same window (`MainWindow.axaml:2733` and `:6261`).

**What is promised:** the opening mockup, and Ruling C, Tim, 2026-09-12: *every kind shown, the
nearest unearned card in each, nothing beyond it.*

**What the tree draws, the same at 1400 and 1920** (twelve contacts). Each badge shows its name, what
it counts, *next* and the next card, its count, then its points and level, then the gap:

| Badge | What it counts | Next | Count | Points · level | Gap |
|---|---|---|---|---|---|
| Hall of Fame | once-only firsts | Over 10,000 miles | 5 worked | 125 pts · Bronze | 1 to Silver |
| Continents | each of the 7 | One more continent | 5 of 7 | 170 pts · Silver | 2 to Gold |
| Countries | one per entity | One more country | 8 worked | 40 pts · unranked | 2 to Bronze |
| States | the 50 states | Your first state | 0 worked, from STATE | 0 pts · unranked | 10 to Bronze |
| Grids | 4-character grids | One more grid | 10 worked | 20 pts · Bronze | 15 to Silver |
| Total Miles | every mile, added | 50,000 miles | 42,041 mi so far | 0 pts · unranked | 7,959 to Bronze |
| Bands | first on each band | One more band | 5 of 7 | 25 pts · Bronze | 1 to Silver |
| Modes | five modes to work | none | 5 of 5 | 85 pts · Gold | none |

Nothing on the page clipped or wrapped at either width.

**Look for with your eyes:** the badge colors, and whether Hall of Fame's name reads on its gold (see
section 4). Check whether eight badges on one page look inviting rather than like a form.

*Source: `Unit349TraceWhatTimLooksAt` at `40c297d`, and `TheAchievementsPageTests` 10 of 10 at
`85437c2`, including `TheEightBadgesAreOnTheWindowAndNothingIsClipped`.*

### 2.4 Every category page, before the kinds one by one

**Get there:** click a badge on the opening page. The plain link at the top brings you back.

**What R22 promises on every kind:** *the category's color band and emblem across the top with its
count, score, level and a bar to the next level; each earned card is the contact that earned it -
the entity large, the callsign and grid, a map of the path cropped to the two stations as the
conversation card draws it, the distance in large type, band, mode and date, the points; the next
card says what it wants and the one thing Hamlet knows that helps - who is calling CQ right now from
a place that would earn it, with distance, from the CQ list.* And: *Nothing white, nothing empty.*

**What the tree draws on every kind:**
- **Every earned card with a grid draws its map across the card.** The map is 632 × 231 in a card
  672 wide at 1400, and 892 × 231 in a card 932 wide at 1920.
- **Clicking a map opens the path in a popup**, and a click outside closes it.
- **Where a next card lists callers, its heading carries the time the list was read**, *calling CQ
  at 21:41 UTC, unworked*, not *right now*.
- **The clip measure read 590 drawn runs at each width** across the opening page, the eight kinds,
  Continents, the seven continent pages and States on its own log. **Nothing clipped or wrapped.**

**Look for on every page with your eyes:** the band's color against its ink, and whether the maps
look like the conversation card's. Check whether a page of cards feels like trading cards, and
whether a next card with no callers looks thin.

*Source: `Unit349TraceWhatTimLooksAt` at `40c297d`. The steps 1 and 2 filter ran 35 of 35 at
`85437c2`: `TheCategoryPagesAreTradingCardsTests` 13 of 13, `TheAchievementsPageClicksInTests` 8 of
8, `ThePsk31RecordsAppearTests` 4 of 4 and `TheAchievementsPageTests` 10 of 10. The popup:
`ACardsMapOpensInItsPopupOnAClickAndAClickOutsideClosesIt`, green at 1400 and 1920.*

### 2.5 Hall of Fame

**R22:** *the contact that earned each first, and the nearest first as next.*

**Draws, the same at 1400 and 1920:**
- **Band:** *once-only firsts · 5 worked · 125 pts · Bronze · 1 to Silver*, and over the bar *5 of 6
  to Silver*.
- **Earned, each with a map:**
  - *Your first contact*, 10 pts: W3YNI · United States, 210 mi, 20 m · FT8, Aug 14, 2026.
  - *A DX contact*, 25 pts: VA3VRR · Canada, 210 mi, 40 m · CW, Aug 16, 2026.
  - *A PSK31 contact*, 15 pts: DL1ABC · Germany, 4,100 mi, 20 m · PSK31, Aug 18, 2026.
  - *A Morse contact*, 50 pts: VA3VRR · Canada, 210 mi, 40 m · CW, Aug 16, 2026.
  - *Over 5,000 miles*, 25 pts: JA1XYZ · Japan, 6,700 mi, 15 m · FT8, Aug 19, 2026.
- **Next:** *Over 10,000 miles*, 100 pts, *furthest 9,673 of 10,000 mi*, *A contact over 10,000
  miles away*.

**Look for:** whether one contact earning two cards (VA3VRR, *A DX contact* and *A Morse contact*)
reads as right or as a repeat.

### 2.6 Continents, and one continent page standing for all seven

**R22:** *seven badges, each the first contact that opened it, the count of countries worked there
since, and the unearned ones naming the continent and who is calling from it now.*

**Draws on Continents, the same at 1400 and 1920:**
- **Band:** *each of the 7 · 5 of 7 · 170 pts · Silver · 2 to Gold*; *5 of 7 to Gold*.
- **The seven badges:**
  - *Africa*, 75 pts: ZS6GHI · South Africa, 1 country worked there, 8,300 mi, 20 m · FT8, Aug 21,
    2026.
  - *Antarctica*, 500 pts, *next*: *A first contact here*, *On the CQ list they carry the ringed
    quill.*, *no one is calling from there now*.
  - *Asia*, 50 pts: JA1XYZ · Japan, 1 country worked there, 6,700 mi.
  - *Europe*, 15 pts: LA8ENA · Norway, 3 countries worked there, 3,900 mi.
  - *North America*, 5 pts: W3YNI · United States, 2 countries worked there, 210 mi.
  - *Oceania*, 50 pts, *next*: *A first contact here*, the ringed-quill sentence, *New Zealand*,
    *ZL1ABC · 8,600 mi*.
  - *South America*, 25 pts: PY2JKL · Brazil, 1 country worked there, 4,900 mi.

**Get to Europe:** click *Europe* on Continents. *‹ Continents* brings you back.

**Draws on Europe, the same at both widths:**
- **Band:** *Europe · a DXCC continent · 3 worked · 15 pts*, and *no levels per continent* with no
  bar.
- **Cards:** 4, starting with *Germany*: DL1ABC · JN48, 4,100 mi, 20 m · PSK31, Aug 18, 2026, with a
  map.
- **Next:** *One more country*, 5 pts, *Any unworked country in Europe*, *On the CQ list they carry
  the green quill.*, and *Austria*, *OE8DDX · 4,400 mi*.

**The other six pages** draw 2, 1, 2, 3, 1 and 2 cards (Africa, Antarctica, Asia, North America,
Oceania, South America). The next cards name *Grenada J38DX · 2,200 mi* on North America and *New
Zealand ZL1ABC · 8,600 mi* on Oceania. Africa, Antarctica, Asia and South America say *no one is
calling from there now*.

**Look for:** whether Antarctica's 500 pts and *A first contact here* read as a target or a taunt.
Check the ringed and green quill sentences against what your decoded list shows.

### 2.7 Countries

**R22:** *the contact that earned it.*

**Draws, the same at 1400 and 1920:**
- **Band:** *one per entity · 8 worked · 40 pts · unranked · 2 to Bronze*; *8 of 10 to Bronze*.
- **Earned, 8 cards at 5 pts each, each with a map:** Brazil, Canada, Germany, Japan, Norway, South
  Africa, *the United States* and United Kingdom. For example: *Brazil*, PY2JKL · GG66, 4,900 mi,
  20 m · FT4, Aug 22, 2026.
- **Next:** *One more country*, 5 pts, *Any country you have not worked*, *On the CQ list they carry
  a quill.*, then *calling CQ at 21:41 UTC, unworked*. The callers are *Austria OE8DDX · 4,400 mi*,
  *Grenada J38DX · 2,200 mi* and *New Zealand ZL1ABC · 8,600 mi*.

**Look for:** the card titled *the United States*, with a lower-case *the*. Check whether New
Zealand, on a continent you have not opened, belongs on this card (section 4).

### 2.8 States

**R22:** *the contact that earned it.* **R27:** *worked, never confirmed, and the count says what it
counts.*

**Draws on the twelve contacts, the same at both widths:**
- **Band:** *0 worked, from STATE · 0 pts · unranked · 10 to Bronze*; *0 of 10 to Bronze*.
- **Next:** *Your first state*, 2 pts, *3 US contacts carry no STATE*, *Any state you have not
  worked*, *Hamlet cannot tell a caller's state*.

**Draws on the state log, the same at both widths:**
- **Band:** *2 worked, from STATE · 12 pts · unranked · 8 to Bronze*; *2 of 10 to Bronze*.
- **Earned:** *AK*, 10 pts, KL7XYZ · BP51, 3,200 mi, 20 m · FT8, Aug 5, 2026; and *PA*, 2 pts,
  K3PA · FN10, 110 mi, 20 m · FT8, Aug 3, 2026. Both have maps.
- **Next:** *One more state*, 2 pts, *1 US contact carries no STATE*, *Any state you have not
  worked*, *Hamlet cannot tell a caller's state*.

**Draws on a state log with 1,234 US records carrying no `STATE`, measured at both widths:**
*1,234 US contacts carry no STATE*, whole, with its thousands separator. It needs 320 px in a
632 px slot at 1400 and an 892 px slot at 1920. Nothing on the page clips or wraps.

**Look for:** on your own log, the no-STATE number will be large, because Hamlet's own entries carry
no `STATE` (unit 348 section 2). Check whether that line reads as information or as blame, and
whether the band misses *the 50 states* (section 4).

### 2.9 Grids

**R22:** *the contact that earned it.*

**Draws, the same at 1400 and 1920:**
- **Band:** *10 worked · 20 pts · Bronze · 15 to Silver*; *10 of 25 to Silver*. There is no
  *4-character grids* on the band.
- **Earned, 10 squares at 1 pt each, each with a map:** FN03, FN20, FN31, GG66, IO91, JN48, JO59,
  KG44, PM95 and QF56. For example: *FN03*, VA3VRR · Canada, 210 mi, 40 m · CW, Aug 16, 2026. *QF56*
  shows *VK2DEF* with no country, 9,700 mi, 10 m · FT8.
- **Next:** *One more grid*, 1 pt, *Any grid you have not worked*, *calling CQ at 21:41 UTC,
  unworked*. The callers are *JN76 OE8DDX · 4,400 mi*, *FK92 J38DX · 2,200 mi* and *FN42 K1ABC · 440
  mi*, then *and 1 more on the CQ list*. There is no quill sentence.

**Look for:** VK2DEF's map picture at 1920, which stops short of the card's width (section 4).

### 2.10 Total Miles

**R22:** *a tier is a bar filling toward the next line and the contact that crossed it.*

**Draws, the same at 1400 and 1920:**
- **Band:** *42,041 mi so far · 0 pts · unranked · 7,959 to Bronze*; *42,041 of 50,000 mi to Bronze*.
  There is no *every mile, added* on the band.
- **Cards:** no earned tier on this log. The next card is *50,000 miles*, 5 pts, *42,041 of 50,000
  mi*, *Every contact adds its miles*.

**Look for:** whether a page with one card and a bar is interesting enough.

### 2.11 Bands

**R22:** *the first contact on that band and which band is the best bet now for the next.*

**Draws, the same at 1400 and 1920:**
- **Band:** *first on each band · 5 of 7 · 25 pts · Bronze · 1 to Silver*; *5 of 6 to Silver*.
- **Earned, 5 pts each:**
  - *20 m*, W3YNI, 210 mi;
  - *40 m*, K2ABC, 320 mi;
  - *15 m*, JA1XYZ, 6,700 mi;
  - *10 m*, VK2DEF, 9,700 mi.
  - Each is FT8 with a map.
  - *80 m*, VE3PQR · Canada, draws *no grid, so no map*, *VE3PQR · 80 m · Aug 25, 2026*, *80 m ·
    FT8*, *Aug 25, 2026*.
- **Next:** *One more band*, 5 pts, *A band you have not worked*, *bands you have not worked*, *17 m*
  with *best bet now*, and *30 m*.

**Look for:** the 80 m card with no grid, which draws its band and date twice.

### 2.12 Modes

**R22:** *the first contact in each mode and where the unearned mode lives and who is there.*

**Draws on the twelve contacts, the same at 1400 and 1920:**
- **Band:** *five modes to work · 5 of 5 · 85 pts · Gold*, then *Gold, the top level*, with no bar.
- **Earned, each with a map:** *FT8* 5 pts, *CW* 15, *PSK31* 5, *FT4* 5 and *Voice* 5. There is no
  next card, because all five are worked.

**The next card, where a mode is unworked**, measured on other logs by units 346 and 347 (`0b506ed`,
`066ad04`, section 3 of each). It reads *One more mode*, *A mode you have not worked*, *where each one
lives*, then one row per mode:
- *CW 18.080 on 17 m · the CQ list carries no Morse*;
- *FT4 3.575 on 80 m · the CQ list cannot tell FT4 from FT8*;
- *PSK31 3.580 on 80 m · EA3ABC · 4,100 mi*, or *· EA3XYZ* with no grid, or *· no one calling at
  21:41 UTC*, or *· the CQ list was not read*;
- *Voice the CQ list carries no voice*.

The tests that assert those rows passed at this tree (`TheNextCardKnowsWhoIsCalling`,
`NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty`).

**Look for:** whether *18.080 on 17 m* beside *3.575 on 80 m* reads sensibly, and whether the row
words are too long to scan.

*Source for 2.5 to 2.12: `Unit349TraceWhatTimLooksAt` at `40c297d`, except where a report is named.*

## 3. Decided for you

**These were decided for you and marked overrulable at step 3.** A verdict of *passed* accepts them.
**Each stands unless you name it.** To overrule one, say its number and the words given, or your own.

**The count:**
- **21** of the arbiter's rulings 1 to 40 have a screen effect;
- **19** have none;
- **10** further choices, made by the units themselves, changed a word or hid something.

All 40 rulings were found in git, in work instructions 338 to 348 (`6ab04aa`, `38b4427`, `cd517bc`,
`8796858`, `ba5179d`, `681d45c`, `de709fd`, `7e830e4`, `4be21c0`, `02884b1`). Work instructions 337
(`0e37597`) and 344 (`83888ce`, PSK31 capture) carry no numbered ruling.

### 3.1 Rulings with a screen effect

| # | What was decided | Where you see it | Rejected | To overrule, say |
|---|---|---|---|---|
| 1 | *The working panels* means the waterfall, decoded text and For You themselves, at least half the height below the pills. The row above them went. | Main window, below the tabs | Counting the whole area below the tabs, strips included (unit 337's reading) | "half means the whole area below the tabs; bring the strips back over the panels" |
| 2 | The rule of thumb is the mockup's words: *20 m and up want daylight along the path; 40 m and down want dark.* *Rule of thumb:* and *the gray edge is where both happen* came off. | Green block, last line | Rewording the license line; hiding the rule behind a mark; shrinking the clock | "put the gray-edge clause back" |
| 3 | The sparkline may hide where the green block's words would wrap; the count stays. | Green block, right side; hidden at 1400 on the host | Taking the sparkline off at every width | "keep the sparkline at every width" |
| 4 | The filter chips stay in the mode strip. | Digital tab, the strip above the panels | Two chips in the Decoded text header, as the mockup draws; shorter chip words | "put everything and CQ in the Decoded text header" |
| 5 | CQ and Stop sit right of the tabs. | Tab row | Inside For You or the waterfall (both fold); the status bar | "move CQ and Stop to" the place you want |
| 6 | The power offer is on PSK31 only, though the mockup draws it on FT8. | Under the S-meter, PSK31 | Offering power on FT8 too | "offer power on every mode" |
| 8 | Under the S-meter the offer is one line, *RF power 50 % offered*. The full offer opens in a popup on a click, and accept and decline are only inside it. | Rig panel, PSK31 | The sentence or ALC line off the screen; the offer beside CQ; a taller top row while unanswered; a hover | "show the whole offer under the S-meter again, taller row and all" |
| 9 | PSK31's extra green-block height at 1400 is fitted without rewording the license line. It was fitted by hiding the empty upgrade row (choice U2). | Green block, PSK31 at 1400 | Rewording the license line | see U2 |
| 11 | Every earned card's map spans the card, 231 px tall; a no-map card keeps the same height. | Every category page | The old 170 px map, left-aligned; a second map; an image asset | "make the card maps shorter" or "taller" |
| 13 | The band line carries the gap to the next level, e.g. *· 2 to Bronze*. | Every category band | Unit 335's band with no gap clause | "take the gap off the band line" |
| 14 | The quill sentence appears only where the decoded list draws that mark. The callers heading says the list's read time, *calling CQ at 21:41 UTC*, never *right now*. | Next cards | *right now*, which would claim a reading nobody took | "say right now on the next card" |
| 15 | The back control is a plain link, *‹ Continents*, with its words and command kept. | Top of every category page | The chip style | "make the back control a button again" |
| 16 | A card's map opens in a popup on a click, never a hover, and closes on a click outside. | Earned cards | A hover | "open the map on hover" |
| 24 | No row on the Modes next card is left empty. | Modes next card | Leaving the empty CW row for step 2 | "leave a row blank where Hamlet has nothing to say" |
| 25 | Where a mode lives comes only from a table Hamlet cites. CW takes the place a band button lands, *18.080 on 17 m* on the best-bet band. | Modes next card | The field guide's 40 m numbers; a typed *7.030* | "name the CW place from" the source you want |
| 26 | Each mode row says only what the CQ list can know: the nearest caller with distance, *no one is calling*, *carries no Morse*, *cannot tell FT4 from FT8*. | Modes next card | One no-caller line for the whole card; a press that would tune there | "one line for the whole card" |
| 29 | A PSK31 caller's grid is used only where his reading is certain, and it reaches every kind's next card. | Modes, Europe and Grids next cards | Distance from the caller's country; a grid from an uncertain reading; a Modes-only rule | "show distance from an uncertain PSK31 reading too" |
| 30 | A PSK31 caller who sent no grid, or was read uncertain, is his callsign alone, with no dash. | Modes next card | A dash in place of the distance (`PHASE_PLAN.md` §6: *show what is there; no dash*) | "mark a caller who sent no grid" |
| 31 | The PSK31 row's no-caller words carry the list's read time: *no one calling at 21:41 UTC*. | Modes next card | The read time in the card's heading, which would also cover CW and FT4 | "say now" |
| 35 | States says *2 worked, from STATE*, and the next card counts US contacts with no `STATE`. *Any state you have not worked* and *Hamlet cannot tell a caller's state* stay. | States badge, band and next card | A state guessed from a callsign; writing `STATE` into Hamlet's own log | "call the count" your words, or "take the no-STATE line off" |
| 36 | With no PSK31 contact, PSK31 appears only as a next card: the Modes caller row and Hall of Fame's *A PSK31 contact*. | Modes and Hall of Fame next cards | Taking PSK31 off both next cards | "no PSK31 anywhere until my first PSK31 contact" |

### 3.2 Rulings with no screen effect

About tests, runs or the record. None changes what you see:
- **Work instruction 340, ruling 7:** fit the offer by arrangement only. Withdrawn by work instruction
  341 in favor of ruling 8.
- **Work instruction 341, ruling 10:** the offer's popup is not a new telemetry stage.
- **Work instruction 342, ruling 12:** *cropped to the two stations* is asserted as a number.
- **Work instruction 343, rulings 17 to 20:** criteria count only on the drawn page; unit 342's build
  stands; how a red is watched; how a fact the page does not draw is fixed.
- **Work instruction 345, rulings 21 to 23:** a card's eight facts are asserted drawn; the carried
  PSK31 and States asks are not a stop; PSK31 decoding findings are logged, not chased.
- **Work instruction 346, rulings 27 and 28:** the clip and white-card measure covers every page;
  VK2DEF's span. That one was dropped, so its map is in section 4.
- **Work instruction 347, ruling 32:** every Modes row is measured for fit.
- **Work instruction 348, rulings 33, 34 and 37 to 40:**
  - step 2's entry and the step 0 reading;
  - no *confirm* anywhere, including a reworded sentence no window draws;
  - `rank_names: []` in the shipped points file, with ranks still reading *Rank n*;
  - the menu-test reds named;
  - the small reds;
  - the emptied files listed.

### 3.3 The units' own choices

| # | Unit | What was decided | Where you see it | Rejected | To overrule, say |
|---|---|---|---|---|---|
| U1 | 338 | The width at which ruling 3's sparkline hides is unit 338's own rule. On the host it is hidden at 1400 and shown at 1920. | Green block | Not recorded | "keep the sparkline at 1400" |
| U2 | 341 | The green block's empty upgrade row now hides with its only button, 3 px in every mode. | Green block | Shipping PSK31 at 1400 2 px over | "put the upgrade row back". The 1400 PSK31 top row then goes to 231 px (0.254) and the panels to 462, with the best bet on your band, on another band or absent (`eca6d60d`) |
| U3 | 342 | The band line drops the kind's meaning past 60 characters. | Grids and Total Miles bands; States on a state log | Not recorded | "keep the meaning on the band" |
| U4 | 342 | Which quill the next card names: *a quill* on Countries, *the green quill* on reached continents, *the ringed quill* on unreached ones, none on Grids and States. | Next cards | The picture's *green quill* sentence on every kind (ruling 14) | "the same quill sentence on every next card" or "no quill sentence" |
| U5 | 346 | The Modes row words *the CQ list cannot tell FT4 from FT8* and its FT8 twin, *the CQ list carries no voice*, and *and n more*. | Modes next card | Not recorded | your words for the row |
| U6 | 346 | PSK31 with no list handed in says *the CQ list was not read*, never *no one*. | Modes next card | *no one is calling* | your words |
| U7 | 347 | *no one calling at 21:41 UTC*, shortened from *no one calling CQ in it at 21:41 UTC* because the longer form squeezed *PSK31* at 1400. | Modes next card | *on the CQ list* | "use the longer words" |
| U8 | 348 | *2 worked, from STATE*, shortened from *2 states worked, read from the log's STATE field*. | States badge and band | *2 from STATE*, which drops *worked* | your words for the count |
| U9 | 348 | The States band loses *the 50 states* on a state log. That follows U3's 60-character rule; the badge keeps it. | States band | A shorter count | "keep the 50 states on the band" |
| U10 | 348 | The no-STATE line: *n US contacts carry no STATE* in the next card's count slot, blank at 0. It counts only records where a `STATE` would score; `DC` is not counted. | States next card | Not recorded | "count DC too" or "take the line off" |
| U11 | 351 | Where the sparkline hides, the best bet row is held to the width of *heard just now* and the count, so *best bet now:* stands over the band and the right column stays 140 px whatever the best bet says. The check had widened it to 200 px and wrapped the verdict. 1400 PSK31 with the best bet on your band: 228 px (was 247), panels 465 (was 446); `1faf33a6`. | Green block, 1400 | The best bet row in the left column (238.0 px, panels 455); in the band line (237, 456); under the regions (240, 453); the check's line height held (247, 446) | "keep the best bet on one line at 1400". The 1400 PSK31 top row then goes back to 247 px (0.271) with the best bet on your band |

*Sources: rulings from `git show <hash>:WORK_INSTRUCTIONS.md` at the hashes above. U1 is `f7f6eb6`
decision 4; U2 is `0f383a3` decision 6 and item 3; U3 and U4 are commit `18f5b17` (unit 342 wrote no
report); U5 and U6 are `0b506ed` decisions 1 and 4; U7 is `066ad04` decision 2; U8 to U10 are
`26e5879` decisions 1 and 2 and item 4.*

## 4. What will look wrong, but is known

One line each, with the report that found it.

**The main window:**
1. **The best bet follows the real clock**, so what the green block says changes with the hour; the
   1400 top row now moves by 0 px with it, 228 px on PSK31 and 216 on FT8 with the best bet on your
   band, on another band or absent. *Unit 339 item 2; `1faf33a6`.*
2. **The *RF power 50 % offered* line's ink is 4.61:1**, clearing 4.5:1 by 0.11. It was printed
   again at this tree. *Unit 341 item 5.*
3. **Where the power popup opens on the screen is not asserted**, only what is in it. *Unit 341 item
   6.*
4. **Setting your power is one click further away** than before ruling 8. *Unit 341 section 2.*
5. **The test's green block says *PSK31 lives at 14.070; you are at 14.074*** because the fixture
   tunes FT8's dial. On your radio it shows only when you are off the PSK31 dial. *Unit 341 item 2.*
6. **The drive and the power offer are on the CW and Voice tabs too**, in the top strip, which is the
   same on every tab. *Unit 337 section 2, decision 6.*
7. **The waterfall at 1920 is narrower than before the phase**, 926 to 735 px, and taller. It is
   734-735 px at this tree. *Unit 337 section 2.*
8. **The world clock is half the size unit 334 made it**, 492 × 269 to 246 × 134, the mockup's size.
   *Unit 337 section 2.*
9. **At 1400 *Digital · FT8 · yours to use* sits on its own line** under the band and frequency.
   *Unit 337 section 2.*
10. **At 1400 on the host, the license line takes 3 lines and the rule of thumb 2.** The license line
    is the regulation's own sentence and is not reworded. *Unit 332 item 4, carried; this tree.*
11. **With the license class unknown at 1400, the tab row is 2 px taller**, because the guard sentence
    takes two lines. *Unit 338 section 2.*

**The achievements pages:**

12. **The achievements window opens at 1040 × 720**, not your main window's size. Nothing sizes it from
    the main window. *Unit 335, on unit 332 item 3, carried; `AchievementsWindow.axaml:9`.*
13. **VK2DEF's map picture stops at 633.18 of 892 px at 1920** on Grids and Bands, because the card
    frame stops at the map's edge rather than drawing past it. At this tree the map's box is 892 ×
    231; the picture inside it was not re-measured. *Unit 346 item 4.*
14. **The no-STATE line was measured only at one digit** by unit 348. **Now measured at four:**
    *1,234 US contacts carry no STATE* is drawn whole at 1400 and 1920, needing 320 px of 632 and 892.
    Five digits and more are still unmeasured. *Unit 348 item 3; this unit's task 3,
    `StatesCountWhatTheLogsStateFieldSays`.*
15. **The States band loses *the 50 states* on a state log**, and Grids and Total Miles lose their
    meaning line too. The badges keep them. *Unit 348 item 4.*
16. **The States next card has three lines of words and no callers.** A CQ carries no state. *Unit 348
    section 2.*
17. **Hall of Fame's name on the opening page is white on gold, about 3.6:1**, under 4.5:1. It was not
    re-measured here. *Unit 335 item 3, carried.*
18. **Next cards name callers from continents you have never worked**, beside the ringed quill, though
    the decoded list keeps those areas behind a door. *Unit 335 item 2, carried, and still an ask.*
19. **Antarctica and Oceania have badges** on Continents before you open them. *Unit 332 item 2,
    carried.*
20. **The CW row names 17 m while the digital rows name 80 m.** CW has a landing on every band; FT4 and
    PSK31 fall to the lowest band with a calling row. *Unit 346 section 2.*
21. **A PSK31 caller with a grid in his text can still show no distance**, where his reading is
    uncertain. *Unit 347 section 2.*
22. **The time on a next card is when the window opened**, not now. The list is read once. *Unit 347
    section 2.*
23. **Grids' *and n more* goes up by one** when a PSK31 caller sent an unworked square. *Unit 347
    section 2 and item 3.*
24. **A PSK31 row keeps only its latest message**, so a grid is lost if the same station sends another
    message after his CQ. *Unit 347 item 2.*
25. **Hall of Fame's PSK31 first with a caller's distance is built but drawn by no test.** *Unit 347
    item 3.*
26. **The family word is *Digital*** where an earlier example said *Data*. *Unit 331 queue item 5,
    carried, and still an ask.*
27. **The host draws text wider than your screen**, so wraps at 1400 are probably fewer for you. That
    is an inference. *Unit 337 section 2.*

## 5. Known reds

Thirteen tests are red, as unit 348 named them at `26e5879`. None is on a page above:
- **`TheMenuIsUnderTheMouseTests`, 8 of 8 red.**
  - The cause: they want the *mine* rows realized, and those rows appear only behind *show the N
    messages*.
  - Not fixed: the scene drives the FT8 send path.
- **`TheWholeChainRunsFromOneRightClickTests`, 2, never run.** The same cause, plus a real audio
  endpoint.
- **`TheOperatorCanStopItTests`, 2, never run.** They are on the transmit side.
- **`TheAchievementsScreenTests.TheWindowDrawsEverySixRows`, 1 red.** It looks for the old six mode
  rows, which your click-in ruling retired.
- **`TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays`, red on some runs after `1faf33a6`,
  is gone.** A spot reload waiting on POTA's reply overwrote the pinned best bet at 1920; with the test
  window's network sources switched off (`386690a2`) it held every pin in three runs. *Work
  instruction 352.*

## 6. Yours, and not this phase's

From `PHASE_PLAN.md` §7 and work instruction 349 §6:
- Every open ask in unit 336's queue, carried in `output.md` section 4.
- Real flags on earned country cards: undecided.
- The PSK31 phase's step 6, at the radio.
- A recording of real PSK31 audio.
- The two decision-id schemes, `HM-DEC` and `CPS-DEC`.
- The map bitmap's license.
- The live license lookup and the live heard count.
- Fifteen emptied files to delete by hand: the list is in unit 348's `output.md` section 2, at
  `26e5879`.

## 7. Your verdict

Copy one of these into your reply:

```
STEP 3: passed
MY WINDOW: <width> x <height>   (achievements window: <width> x <height>)
```

```
STEP 3: not passed
MY WINDOW: <width> x <height>
PAGE: <2.1 to 2.12>   WIDTH: <yours>   WHAT IS WRONG: <in your words>
(one PAGE line per thing)
OVERRULE: <ruling number or U number, and your words>   (optional)
```

**Every decision in section 3 stands unless you name it.** A *not passed* that names a page is new
work for that page's step. The main window is step 0's, the category pages are step 1's, and the next
arbiter places anything else. It does not re-open the phase.
