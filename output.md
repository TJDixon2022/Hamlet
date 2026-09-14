```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Step 0 done (state reader on
   unit 341). Step 1 partial on the evidence: 7 of its seven must-pass
   carry a named green test on the drawn page at 1400 AND 1920 (criterion
   7 is a telemetry event with no width), and 6 of 7 with nothing
   qualifying them, was 3 unqualified after the state reader on unit 345
   (unit 345 claimed 7). Criterion 3 keeps one qualification: on the
   Modes next card a PSK31 caller is drawn as his callsign with no
   distance, because the CQ list holds no grid for a PSK31 row. Steps 2
   and 3 not started.
B. Step 1 and its exit criteria - the two gaps the state reader named
   first, then the rest re-run:
   entry: TheAchievementsPageClicksInTests 8 of 8, run first;
      TheCategoryPagesAreTradingCardsTests 10 of 10
   3. next card names its want and its callers with distance, or no one
      calling - Modes rows at 1400 and 1920, the same at both: with
      Calling(), CW [18.080 on 17 m · the CQ list carries no Morse], FT4
      [3.575 on 80 m · the CQ list cannot tell FT4 from FT8], PSK31
      [3.580 on 80 m · no one is calling in it now]; with the PSK31
      caller, CW and FT4 the same and PSK31 [3.580 on 80 m · EA3XYZ], no
      distance; EMPTY rows 4 -> 0 (the CW row, each list, each width);
      TheNextCardKnowsWhoIsCalling pass, every other next card re-run
   4. all eight kinds per R22 - Modes: where each unworked mode lives -
      CW at 18.080 on 17 m, where a 17 m band button lands (CwBand.JumpHz
      from HfBands.Landing, HM-DEC-110, read through the public
      HfBands.Bands, no engine change), FT4 3.575 and PSK31 3.580 on 80 m
      (DigitalCallingFrequencies; 17 m has neither); who is there - CW the
      no-Morse words, FT4 that the list cannot tell it from FT8, PSK31 its
      nearest caller or no one in it now;
      TheOtherFiveKindsEachDrawTheirOwnCards pass, view model and drawn
   5. no string clips or wraps a word - pages measured: 8 kinds, 7
      continent pages, Modes on five contacts with a PSK31 caller;
      NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty
      pass; tallest page Grids 2022 px at 1400 and at 1920; States on its
      own log in StatesCountWhatTheLogsStateFieldSays, pass
   6. no card is a white rectangle - the same test, pass; 0 white of 64
      cards at 1400 and 0 white of 64 at 1920
   1, 2, 7 and the nice-to-pass - re-run in the one filter, 25 of 25:
      EveryKindsBandCarriesCountScoreLevelAndABar pass;
      EveryEarnedCardIsTheContactThatEarnedIt pass;
      OpeningACategoryWritesTheKindAndTheCardCountAndNothingElse pass;
      ACardsMapOpensInItsPopupOnAClickAndAClickOutsideClosesIt pass at
      1400 and 1920
   ruling 11's span on every kind - DROPPED (task 3, the drop candidate):
      VK2DEF 633.18 of 892 at 1920 on Grids and Bands, 258.82 short; every
      other map on every kind spans its card at both widths
C. The report last. Section 4 raises 5 items on top of the carried queue.
   Item 1, the PSK31 caller's missing distance, is the one qualification
   on criterion 3 in B. The CW place needed no engine accessor. One row's
   words may claim more than the CQ list can know: PSK31's "no one is
   calling in it now" (item 2).
```

```
UNIT:       346 - complete at task 2 of 3 - 2026-09-13 21:58
PHASE GOAL: Hamlet's main window and every achievements category page drawn as Tim's approved
            pictures at his two widths, proved by computation, then passed by Tim at his window.
UNIT GOAL:  Close the two gaps the state reader named on unit 345 - no empty row on the Modes
            next card, each row saying where its mode lives and who is there as far as the CQ
            list can know, and no clip and no white card measured over all seven continent
            pages and the Modes page that draws a next card.
ADVANCED:   yes - the empty CW row is filled and drawn at both widths and criteria 5 and 6 are
            measured on all 16 pages; criterion 3 keeps one named qualification, a PSK31
            caller drawn without distance because the list holds no grid for him
NUMBER:     step 1 must-pass held on the drawn page at 1400 and 1920 with nothing qualifying
            them: 3 of 7 -> 6 of 7
DRIFT:      0
```

**Every appearance claim here is computed on the headless test host, not seen.** Nothing
here is evidence about the radio.

## 1. What Claude did

**Complete at task 2 of 3; task 3 was dropped.** Task 3 was the named drop candidate. It was
dropped because it could not be built inside its own limits, not for time (below). This machine,
project Hamlet at `C:\Source\HamLet`, branch `main`. The gate held: `SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` exist, `CoreHMI.sln` and `MURC.sln` do not,
and `.run-unit\allowed.txt` carries `Bash(dotnet:*)`. **`dotnet test` was never refused.**

### Task 0 - the trace, before anything changed

- **Opening commit `7e830e4`**: this instruction, `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line
  only, and 1.13.32 -> 1.13.33 with its comment block.
- **Carry-forward, before**: app **111 of 111**, engine **86 of 86**.
- **Entry check**: `TheAchievementsPageClicksInTests` **8 of 8**, then
  `TheCategoryPagesAreTradingCardsTests` **10 of 10** (unit 345's nine and its trace). Green, so
  the unit went on.
- **`Unit346TraceTheModesNextCardAndEveryPage`** (`09da5d7`) asserts nothing and presses nothing
  that transmits or tunes.
  - It adds `CallingWithPsk31()`: `Calling()`'s four, plus the text-only row `CQ CQ CQ de EA3XYZ
    EA3XYZ K` read by `Psk31ExchangeParser.Read`, the way `ThePsk31ReadsTheConversationTests`
    builds one.
  - At 1400 and 1920 it printed:
    - the Modes next card on the five-contact log, row by row, on both lists;
    - the CW place candidates per band;
    - every page ruling 27 names, with its runs, cards, white cards and height;
    - every earned card's map against its card's inner width.

**Task 0's answers, from the numbers:**
- **Which Modes rows were `EMPTY`:** the CW row, at 1400 and 1920, with `Calling()` and with the
  PSK31 caller - all 4 realizations.
  - FT4 drew `3.575 on 80 m` and PSK31 `3.580 on 80 m`, with nothing about who is there.
  - With the PSK31 caller, PSK31 drew `3.580 on 80 m · 1 calling CQ`.
  - The card's no-caller line was empty in all four.
- **Where the CW place can be read:** `HfBands.Bands` is public. Each `CwBand.JumpHz` is what
  `Build` fills from the private `HfBands.Landing`, and the band buttons read that same member
  (`MainWindowViewModel.cs` lines 7295 and 10906). **So no accessor was needed and no engine file
  changed.** Per band, where it lands and its CW segment:

  | Band | Lands | CW segment |
  |---|---|---|
  | 80 m | 3.530 | 3.500 to 3.600 |
  | 40 m | 7.028 | 7.000 to 7.125 |
  | 30 m | 10.103 | 10.100 to 10.150 |
  | 20 m | 14.030 | 14.000 to 14.150 |
  | **17 m, the fixture's best bet** | **18.080** | 18.068 to 18.110 |
  | 15 m | 21.030 | 21.000 to 21.200 |
  | 10 m | 28.030 | 28.000 to 28.300 |
- **Can the list say FT4:** no. `CqSnapshot.From` marks a call `PSK31` where the row is text-only
  and `""` otherwise. Every `Heard(...)` row carried `""` and EA3XYZ carried `PSK31`. No call on
  either list said FT4.
- **Does any page clip or draw a white card today:** no. There were 0 clips and 0 white cards on
  all of these, at both widths:
  - the 8 kinds and 7 continent pages;
  - Modes on five contacts with the PSK31 caller.
- **Which maps fall short:** only VK2DEF at 1920, on Grids (QF56) and Bands (10 m): 633.18 drawn
  in 892, **258.82 short**.
  - Every other earned card's map, on every kind and continent page, spans its card's inner width
    exactly: 632 at 1400, 892 at 1920.
  - VK2DEF spans 632 at 1400.
- **Also found:** the PSK31 caller's call has grid `""`. The CQ list holds no grid for a PSK31
  row, so no distance can be drawn for him (section 4, *Raised by unit 346*, item 1).

### Task 1 - the Modes next card (`7b5fa8f`)

- **Tests first, watched red on the tree as it was:**
  - **`TheNextCardKnowsWhoIsCalling`**: the drawn loop now runs Modes on the five-contact log with
    `Calling()` and with `CallingWithPsk31()`.
    - `ModesRowMiss` asserts every drawn row has a place, and a line made of where the mode lives,
      ` · `, then who is there.
    - It works the place out on its own, from `HfBands.Bands` and `DigitalCallingFrequencies`.
    - Failure line: *1400.00 modes with 4 on the CQ list: the CW row draws an EMPTY line (rows
      drawn: CW [] / FT4 [3.575 on 80 m] / PSK31 [3.580 on 80 m])*.
  - **`TheOtherFiveKindsEachDrawTheirOwnCards`**, Modes half: the CW row equals `18.080 on 17 m ·
    the CQ list carries no Morse`, on the view model and drawn at 1400 and 1920. FT4 and PSK31
    keep `3.575 on 80 m` and `3.580 on 80 m`. Failure line: *Assert.Equal() Failure: Strings
    differ. Expected: "18.080 on 17 m · the CQ list carries no M"···, Actual: ""*.
  - **Second check, built in (ruling 19):** the same drawn rows held against a 40 m best bet. On
    every green run it prints *the CW row draws [18.080 on 17 m · the CQ list carries no Morse],
    not [7.028 on 40 m · the CQ list carries no Morse]*.
  - Every existing assertion was kept.
- **Built in `AchievementCategory.ModesFor` and `LivesAt`.** `CallingIn` is replaced by
  `WhoIsThere`:
  - `LivesAt("CW", bet)` reads `HfBands.Bands` for the best-bet band, or the lowest band, and
    writes it in `LivesAt`'s form through a shared `OnBand`. No frequency is typed.
  - `WhoIsThere` gives, per mode:
    - CW: `NoMorseOnTheList`;
    - FT4: `ListCannotTellFt4`;
    - FT8: `ListCannotTellFt8`;
    - PSK31: the nearest caller by miles, as `Joined(callsign, distance)` with ` and n more` past
      one; or `NoOneCallingInIt`; or `ListNotRead` where no list was handed in;
    - any other mode (Voice): `NoVoiceOnTheList`.
  - `MilesTo` now reads a new `MilesBetween`, and its output is unchanged.
  - No earning, score, points, other kind's next card or word from rulings 11 to 16 changed.
- **One filter, 25 of 25**: `TheCategoryPagesAreTradingCardsTests` 11 of 11,
  `TheAchievementsPageClicksInTests` 8 of 8, `BindingHealthTests` 1 of 1, `VoiceTests` 5 of 5.
  Carry-forward: app **111 of 111**, engine **86 of 86**. No engine file changed.
- **Fit:** after the build, the trace measured Modes on five contacts at 26 runs and 0 clips at
  both widths. The longest row, FT4's, fits, and nothing was shortened.

### Task 2 - criteria 5 and 6 on every page (`03a54f7`)

- **`NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty`** now realizes two logs at
  each width:
  - twelve contacts with `Calling()`, over the 8 kinds and all 7 continent pages, replacing EU and
    OC;
  - five contacts with `CallingWithPsk31()`, over Modes, asserting its next card is drawn.
- Its `Fits` asserts, the 0.5 px tolerance and `white == 0` are unchanged. Each page's line prints
  as before, and now the tallest page prints too.
- **Watched red**, built in: the first card on the first page gets its map, bar and list hidden on
  the test window, then shown again. It prints *1400.00 twelve contacts hall_of_fame [Your first
  contact] with its map, bar and list hidden on the test window (2 hidden), watched red: counted
  white*.
- **Nothing clipped and no card was white**, so nothing was shortened or widened.
- **One filter 25 of 25** (11, 8, 1, 5). Carry-forward: app **111 of 111**, engine **86 of 86**.

### Task 3 - dropped

**Dropped as the drop candidate, and not for time.** `Ft8GlobePlot.CardFrameFor` grows the frame
to the card's shape inside the file, and its remark says why VK2DEF stops: *where the shape cannot
be reached inside the file - a path too tall for a card that wide - the frame stops at the file's
edge and the picture is drawn narrower, rather than past the photograph (§0.0)*.

VK2DEF's path from FN00 needs the file's full width and more height than 892 by 231 allows. Filling
the card would take one of these, and each is ruled out:
- cropping a station out, against ruling 12;
- stretching, which `Ft8GlobeControl`'s measure remark rules out (HM-DEC-092);
- a taller map, against *change no height*;
- drawing past the photograph, against the method's §0.0 remark.

Ruling 28 makes the span no bar to step 1. Unit 345 gave VK2DEF's number, and task 0 re-measured
it: 633.18 of 892 at 1920.

### Checked against the tree (§2)

**Held:**
- **`ModesFor`**, line 662, builds one `NextCaller` per unworked mode with
  `Joined(LivesAt(...), CallingIn(...))`.
  - `LivesAt` (695) read only `DigitalCallingFrequencies`.
  - `CallingIn` (712) counted rows whose `Mode` matched.
  - The remark is at 655 to 659.
  - `Joined` of two empty strings is empty, and the card passed `""` as its no-caller line.
  - `first_cw_qso` at 571 uses `NoMorseOnTheList`.
- **`HfBands.Landing`** (190) is `private`. It is the first `CW main street` block inside the
  segment, or the segment's bottom.
- **The public read §1 left unchecked exists:** `HfBands.Bands[i].JumpHz`, which the band buttons
  use.
- **The list's modes:** `Heard(...)` rows carry no mode, and no call can say FT4.
- **The tests as §1 described them:**
  - The clip test looped the 8 kinds plus `continent-EU` and `continent-OC`, on `TwelveContacts()`
    only.
  - The twelve contacts draw no Modes next card.
  - `Calling()` is four FT8-shaped rows, with no PSK31 caller.
  - `StatesCountWhatTheLogsStateFieldSays` measures States on its own log, at 1400 and 1920, for
    fit and white cards.
- **Unit 345's commits** `de709fd`, `c5bb6a9`, `ff09a11`, `04b8abf` and `218d6e0` are on
  `origin/main`; `main` was level with it at the start. `output.md` was unchanged since `218d6e0`.
- **The version** was 1.13.32.
- **The launcher's files:**
  - `PHASE_OUTCOME.md`'s header reads step 0 `in progress`.
  - Its entries are units 337, 338, 339, 340 and 342, then `UNIT 337 - CARRIED REPAIR`, then two
    `UNIT 1 - STEP 1` (units 343 and 345). There is no unit 341 or 344 entry.
  - `PHASE_STATUS.md` reads `CURRENT_STEP: 0`.
- **`RULES_AT`:** `DECISIONS.md` tops at HM-DEC-163. `tools/status.sh` writes HM-DEC-161, so every
  write was followed by a hand edit to `HM-DEC-163 (2026-09-12)`.
- **`CPS-DEC`:** `CLAUDE.md` holds no such id (0 matches). Parked with the id schemes.
- **Housekeeping:** `tools\arbiter.bak-20260913\` stayed untracked. Files were staged by name, and
  the validator was given the report.

**Mismatches, reported and not repaired:**
- **Task 1 asks the PSK31 row with the caller to draw ` · n mi`.** The tree cannot, because a PSK31
  row has no grid on the CQ list. The assertion was written to what the list holds (section 4,
  *Raised by unit 346*, item 1).
- **§2's tool facts name `sort` with options as asking for approval.** It was not tried here. See
  the table below.

**Reds expected:** none was in any filter, so none was run. No red turned green and no new red
appeared in what ran. The one wanted red, task 1's, is given above.

**Tool facts, as they held for this session.**

| Result | Commands |
|---|---|
| Ran | `sh tools/status.sh` (every write); `&&` joining `git add`, `commit` and `push`; commits with several `-m`; `grep`, `tail`, `cut`, `uniq` and `tr` in pipes; `git diff --quiet ... && echo`; `date` |
| Asked for approval, not run | a `cd` before `git` in a compound; `grep` with `\s` in its pattern, twice; `awk` in a pipe |
| Blocked | none met |
| Not retested | output redirects; `sed -i`; `git -C`; `git show` with several hashes; `sort` with options; apostrophes in `.bat` arguments |

### Decisions made for this unit

All are the unit's own and overrulable:
1. **The words:**
   - `no one is calling in it now`, `NoOneCalling`'s sense fitted to a mode row;
   - `the CQ list cannot tell FT4 from FT8`, and its FT8 twin;
   - `the CQ list carries no voice`;
   - ` and n more` after the nearest PSK31 caller, where the instruction said *the count*.
   None needed shortening.
2. **No engine accessor.** The public `HfBands.Bands` already carries the landing, so ruling 25's
   allowance was not used.
3. **The Voice and FT8 rows get words** though no fixture draws them unworked, so no row is empty on
   any log. Neither is measured on the window (item 3).
4. **PSK31 with no list handed in says `the CQ list was not read`**, as `NextCard` does, never *no
   one*.
5. **Task 3 was dropped**, for the reason above.
6. **The PSK31 caller row is built with `Psk31ExchangeParser.Read`**, not through the splitter the
   main window feeds. It gives the same kind of `Reading`.
7. **Watched reds are built into the tests as second checks**, unit 345's pattern.

### Commits

`7e830e4`, `09da5d7`, `7b5fa8f` and `03a54f7`, each pushed to `main` without refusal. This report
and the status file follow in their own commit.

## 2. What the owner should expect

**The Modes page's next card now says something on every row.** On a log that has not worked CW,
FT4 or PSK31, with 17 m the best bet:
- *CW*: *18.080 on 17 m · the CQ list carries no Morse*;
- *FT4*: *3.575 on 80 m · the CQ list cannot tell FT4 from FT8*;
- *PSK31*: *3.580 on 80 m · no one is calling in it now*, or the nearest PSK31 caller's callsign.

The CW place is where a 17 m band button lands. Nothing on the card tunes. Nothing else on any page
changed. The version is 1.13.33.

**What will look wrong but is not:**
- **The CW row names 17 m and the digital rows name 80 m.** CW has a landing on every band. FT4 and
  PSK31 have no calling row on 17 m, so they fall to the lowest band that has one - the rule the
  digital rows already used.
- **A PSK31 caller shows no distance.** His CQ puts no grid on the CQ list.
- **`Unit346TraceTheModesNextCardAndEveryPage` passes and asserts nothing.** It is the record behind
  task 0's answers.
- **Six watched-red lines print on every green run.** Each is asserted to fail against a wrong
  expectation. A missing line is the failure.

## 3. What you should see

**Not quite.** Every step 1 must-pass has a named green test on the drawn page at 1400 and 1920,
the Modes next card draws no empty row, and all seven continent pages are measured. **One
qualification is left, on criterion 3:** on the Modes next card, at both widths, a PSK31 caller is
drawn as `EA3XYZ` with no ` · n mi`. The CQ list holds no grid for a PSK31 row. It is one row's
distance, and nothing clips.

**The Modes next card on the five-contact log**, top to bottom, the same at 1400 and 1920:
- *One more mode*, *5 pts*, *next*; then *A mode you have not worked*; then *where each one lives*.
- With the four FT8 callers:
  - *CW*: *18.080 on 17 m · the CQ list carries no Morse*;
  - *FT4*: *3.575 on 80 m · the CQ list cannot tell FT4 from FT8*;
  - *PSK31*: *3.580 on 80 m · no one is calling in it now*.
- With EA3XYZ calling CQ in PSK31: CW and FT4 the same, and *PSK31*: *3.580 on 80 m · EA3XYZ*.
- Before this unit, the CW row was blank in all four.

**The seven continent pages**, the same at 1400 and 1920. No run clipped and no card was white:

| Page | Runs fit | Cards | Height |
|---|---|---|---|
| Africa | 17 | 2 | 589 px |
| Antarctica | 11 | 1 | 589 px |
| Asia | 17 | 2 | 589 px |
| Europe | 30 | 4 | 674 px |
| North America | 24 | 3 | 674 px |
| Oceania | 12 | 1 | 589 px |
| South America | 17 | 2 | 589 px |

**The Grids VK2DEF card's map:** task 3 did not run. Task 0 measured it at 632 by 231 at 1400,
filling the card, and 633.18 by 231 in 892 at 1920.

**The step 1 table.** Every appearance claim here is computed on the headless host, not seen.

| Test | Criterion | Drawn or view model | Widths | Pages | Result | Numbers |
|---|---|---|---|---|---|---|
| `EveryKindsBandCarriesCountScoreLevelAndABar` | 1 | drawn (1040 kept) | 1400, 1920 | 8 kinds, 7 continent pages | pass | re-run; this unit did not change it |
| `EveryEarnedCardIsTheContactThatEarnedIt` | 2 | drawn | 1400, 1920 | Countries, Grids, States | pass | re-run; this unit did not change it |
| `TheNextCardKnowsWhoIsCalling` | 3 | drawn (view model kept) | 1400, 1920 | every kind, Continents, 7 continent pages; Modes on five contacts with `Calling()` and with a PSK31 caller | pass, one qualification | Modes EMPTY rows 4 -> 0; PSK31 caller without distance; watched reds |
| `TheOtherFiveKindsEachDrawTheirOwnCards` | 4 | drawn and view model | 1400, 1920 | Continents, Total Miles, Bands, Modes, Hall of Fame | pass | CW `18.080 on 17 m · the CQ list carries no Morse`; FT4 and PSK31 on 80 m |
| `ContinentsOpensToSevenAndEachToItsCountries` | 4 | drawn, by press | 1400, 1920 | 7 continents | pass | re-run; this unit did not change it |
| `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty` | 5, 6 | drawn | 1400, 1920 | 8 kinds, 7 continent pages, Modes on five contacts | pass | 0 white of 64 cards a width; tallest Grids 2022 px; watched red |
| `StatesCountWhatTheLogsStateFieldSays` | 5, 6 | drawn | 1400, 1920 | States on the state log | pass | 3 cards, 0 white |
| `OpeningACategoryWritesTheKindAndTheCardCountAndNothingElse` | 7 | view model, telemetry | none | Countries | pass | re-run |
| `ACardsMapOpensInItsPopupOnAClickAndAClickOutsideClosesIt` | nice-to-pass | drawn, real clicks | 1400, 1920 | Countries | pass | 2 of 2 |
| ruling 11's span (task 3) | ruling 11 | drawn | 1920 | Grids, Bands | DROPPED | VK2DEF 633.18 of 892 |

## 4. What's blocking us

**No ruling is needed to go on.** Unit 345's section 4 comes first, verbatim per HM-DEC-139: from
its line under `## 4. What's blocking us` to its end, as committed in `218d6e0`, kept in place with
the file editor. Two of its items are marked, as work instruction 346 §9 asks: item 2 (VK2DEF,
*DROPPED*) and item 3 (the Modes CW row, *TAKEN UP*). This unit's five items follow at the very end,
under *Raised by unit 346*.

### Asks still outstanding - carried from unit 345's section 4, per HM-DEC-139, verbatim


**No ruling wanted.** Five findings raised by this unit, then the carried queue.

**1. The engine carry-forward is 86 of 86, and unit 344 reported 87 of 87.**

*No ruling wanted; a mismatch, reported and not repaired.*
- Both engine runs, before and after this unit's changes, read 86.
- `EveryCaptureOffTheAirIsReadBack` is a `[Fact]` inside `ThePsk31DemodulatorTests`, which is on
  the list.
- Which test the 87 counted was not found. The engine and PSK31 are parked (ruling 23).

**2. VK2DEF's map is 633.18 x 231 in an 892 px card at 1920, on Grids (QF56) and Bands (10 m).**

*No ruling wanted; a finding.*
- At 1400 it is 632 x 231.
- Ruling 11's span is asserted only on Countries, where VK2DEF has no card: its callsign resolves
  to no entity, so the Grids card draws *VK2DEF* alone.
- Criterion 2's facts and map are drawn and asserted; only the span stops short.

*TAKEN UP by work instruction 346 ruling 28 and task 3 - DROPPED by unit 346 - task 3 was the drop
candidate.* Unit 346's trace re-measured it at 633.18 x 231 in 892 at 1920 on Grids and Bands, 258.82
short, and 632 x 231 at 1400. `Ft8GlobePlot.CardFrameFor` stops the frame at the file's edge by its own
§0.0 remark, so no fix fits task 3's limits (unit 346 section 1 and item 4 below).

**3. Modes' next card draws CW with no place to find it.**

*No ruling wanted; a finding for step 2, whose exit names the Modes test (ruling 22).*
- On the five-contact log the rows are *CW* with an empty call line, *FT4 3.575 on 80 m* and
  *PSK31 3.580 on 80 m*.
- R22 asks for *where the unearned mode lives and who is there*.
- The view model holds the empty line, so the page draws what it holds, and ruling 20 has nothing
  to fix.

*TAKEN UP by work instruction 346 rulings 24 to 26 and task 1.* The CW row now draws *18.080 on
17 m · the CQ list carries no Morse* at 1400 and 1920, the place a 17 m band button lands. FT4 draws
*3.575 on 80 m · the CQ list cannot tell FT4 from FT8*. PSK31 draws *3.580 on 80 m · no one is calling
in it now*, or *3.580 on 80 m · EA3XYZ* with a PSK31 caller on the list.

**4. Version 1.13.30 -> 1.13.31 has no comment block.**

*No ruling wanted; a finding beside unit 343's item 4.* It is unit 344's bump. Not repaired.

**5. The status script's `RULES_AT` and this session's blocked file routes.**

*No ruling wanted; a finding.*
- `tools/status.sh` still writes `HM-DEC-161 (2026-09-11)`, so each write here was followed by a
  hand edit to `HM-DEC-163 (2026-09-12)`.
- Redirects and `sed -i` were blocked, so this report was assembled with the file editor around
  unit 344's committed section 4.

### Asks still outstanding - carried from unit 344's section 4, per HM-DEC-139, verbatim

The words below are unit 344's, from its line under `## 4. What's blocking us` to its end, as
committed in `6da5149`. Only that top-level heading is dropped, so this report keeps four
sections. They were kept in place with the file editor, not retyped. Four items are marked, as
work instruction 345 §9 asks:
- unit 343's items 1 and 2;
- unit 340's item 5;
- unit 339's item 5.

No step criterion moves. Five items.

**1. The garble is still unexplained, and the one thing that does reproduce its shape is
argued against by the record.**

*No ruling wanted; the honest end of task 2.* Neither named hypothesis survives: the skirt
is still perfect 36 dB down, the fade is still perfect at a −60 dB floor. **Low signal-to-noise
does make the shape** — 0.1502 at −10 dB — but the record of 2026-09-13 carries squelch
qualities running to **0.999**, which is not what a −10 dB signal looks like to that
measure. So there are now three explanations and no evidence that settles any of them.
**What settles it is one capture off 7.070 in `assets\fixtures\captured\`**, which is what
task 1 was built to make and what task 3 is waiting to read.

**2. The read-back test passes on an empty folder rather than skipping, and that is a
mismatch with the instruction that could not be repaired.**

*No ruling wanted; a mismatch, reported.* Task 3 asks for *skipped, not failed, when the
folder is empty*. **xUnit 2.9.2 has no runtime `Assert.Skip`** — that is version 3 — an empty
`[Theory]` data set is reported as a failure (`System.InvalidOperationException : No data
found`), and the package that would add one is forbidden by §10. Both routes were tried.
What ships is a `[Fact]` that returns early and prints `NO CAPTURE READ` with the folder
path and the instructions, so a green tick is never read as evidence about real air. The
alternative — leaving it red until a capture exists — would put a permanent red on the
carry-forward list, which is what that list exists not to hold.

**3. A capture press that lands before any audio has arrived still writes the device rate
absent.**

*No ruling wanted; a finding, and a defect the test caught.* `Psk31Resampler` is made on the
first tick, so the press had nothing to ask and wrote `deviceSampleRate: null`. It now falls
back to the tap, which knows the rate after a single lump, and the finished event carries the
rate measured from the audio. **The remaining hole is a press with no audio at all**, where
the field is absent — which is correct by §0.0 and is named here so nobody reads it as a bug
later.

**4. The skirt arm is Hamlet's own modulator, so it cannot give an absolute error rate.**

*No ruling wanted; a limit on what task 2 proved.* No fixture exists with a PSK31 carrier at
330 Hz, so both skirt arms were made by `Psk31Modulator` and decoded by `Psk31Demodulator` —
one round trip, which §12.5 is explicit cannot judge itself. What it can show is **330 Hz
against 1000 Hz through the same code**, which is the question asked, and the unfiltered
reference run is printed beside it as the anchor. An independent 330 Hz fixture from
`reference-modem.py` would close that gap and was not made here: §2 says Python cannot run
in this session, and adding a fixture is beyond what R14 allows this unit.

**5. Two of §2's tool facts do not hold in this session.**

*No ruling wanted; a mismatch, reported and not repaired, and the third sighting.* §2 says
*Python cannot run here* and *`rm` is refused*. **Python ran** — it made several of this
unit's edits — and **`rm -f` removed the probe capture without complaint.** Unit 337 reported
the Python half; the `rm` half is new. The apostrophe and backslash facts in the same list
were not retested and are not disputed. `tools/status.sh` was again not refused, against unit
341 item 7.

### Asks still outstanding - carried from unit 343's section 4, per HM-DEC-139, verbatim

The words below are unit 343's, from its line under `## 4. What's blocking us` to its end, as
committed in `33fb6a9`, with only that top-level heading dropped so this report keeps four
sections.

**Its item 1 is answered in substance by this run and is left in place rather than deleted**,
because what it asked for was a ruling on which permission scope an unattended Hamlet unit
runs under, and that has not been ruled — it has been fixed. `.run-unit\allowed.txt` now
carries `Bash(dotnet:*)`, `Bash(timeout:*)` and `Bash(sh:*)`, this unit's gate checked it
before anything else, and every build, test and validation below ran. **Unit 343's step 1
criteria are unblocked**, and the drop belongs to the report that records the ruling.


### Raised by this unit

**1. This run cannot build, test or validate Hamlet: `230e6c0` replaced its permission scope with
ClaudeProjectStatus's.**

**ANSWERED by the tree - .run-unit\allowed.txt carries Bash(dotnet:*), noted by unit 344 and re-proved
by unit 345 task 0.** Task 0's first `dotnet` command was `timeout 480 dotnet test
tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter "FullyQualifiedName~ThePressingOfCqTests|…"`,
the carry-forward app invocation. It gave 111 of 111 and was not refused; nor was any `dotnet` command
after it.

*Ruling wanted, or a launcher fix, and it blocks every step 1 criterion: which permission scope an
unattended Hamlet unit runs under.*
- **The scope now.** `tools/arbiter/run-unit-tools.txt` allows `node tools/tests/run.js`, and
  `.run-unit/allowed.txt` for this run matches it. That script does not exist here.
- **What the layer removed:** `dotnet test`, `dotnet build` and `dotnet restore`; the four
  `validate-output.bat` spellings; the three un-staging commands; the shell reads.
- **The layer's other changes:**
  - `validate-output.bat` now defaults to `C:\Source\ClaudeProjectStatus\output.md` and checks six
    rules.
  - `.run-unit/prompt.txt` for this run no longer carries the `PHASE_STATUS.md` instruction, or the
    four-heading and validate-yourself text.

*Reasoning.* The recommendation is to restore Hamlet's lines to `run-unit-tools.txt` from `230e6c0^`:
`Bash(dotnet test:*)`, `Bash(dotnet build:*)`, `Bash(dotnet restore:*)`, and the lines after them.
Then re-issue work instruction 343 unchanged. Its task 0 opening is already committed at `681d45c`,
so a re-run should treat step 1 of task 0 as done and start at the carry-forward run. The layer
looks like a copy between projects with no per-project `TEST_CMD`. That is inferred from the diff,
not confirmed.

*What was rejected and why.*
- **Writing `tools/tests/run.js` to shell out to `dotnet`:** it routes around the scope the owner's
  launcher set.
- **Writing the assertions unrun:** ruling 19 and task 0's *measure before anything is built*.
- **Editing `run-unit-tools.txt` or `allowed.txt` here:** they are the launcher's, and a unit widening
  its own guard is the thing that file's header warns against.

**2. Criterion 2's "earned card is the contact" is held on the drawn page only for the map.**

**TAKEN UP by work instruction 345 ruling 21 and task 2.** The eight facts are now asserted drawn
at 1400 and 1920 in `EveryEarnedCardIsTheContactThatEarnedIt` (`EarnedCardMiss`, `04b8abf`), each
from the log entry that earned the card:
- Countries on the twelve- and five-contact logs;
- Grids on both;
- States on the state log.

*No ruling wanted; a finding that bears on the 4 of 7.* At 1400 and 1920,
`EveryEarnedCardIsTheContactThatEarnedIt` asserts only the map's width, height and crop on Countries
cards.
- The entity, callsign and grid, distance, band, mode, date and points are asserted on the view model.
- At 1040 the window asserts only the map count and the no-map word.
- Grids' and States' earned contacts are not asserted drawn at any width.

Work instruction 343 task 2 already names Grids. A re-run should give Countries' and States' contact
words the same drawn assertion before counting criterion 2 as held on the page.

**3. The tool facts, as they held for this session.**

*No ruling wanted; a finding.* Each was tried once unless it says otherwise.

| Result | Commands |
|---|---|
| Refused | `dotnet test` (with and without `timeout`); `sh tools/status.sh`, so every `UPDATED` is a `date` reading; two commands joined by `&&`; a shell `for` loop variable; a variable expansion (`$f`); `sort` in a pipe; `python -c` with a script |
| Ran | `python --version` (3.13.12), earlier in the same session; `date`; `grep`, `ls`, `cat` and `head` in a pipe without `sort`; a quoted heredoc fed to `git cat-file --batch-check` |
| Not tried | apostrophes in heredocs, doubled backslashes, `;`, `rm`, `git stash`, `sed -E`, redirects into `output.md`, the validator |

**Unit 337's report disagrees on two points:** `tools/status.sh` and `&&` were both refused here.
Python ran for `--version` only.

**4. Version 1.13.28 -> 1.13.29 has no comment block.**

*No ruling wanted; a finding.* It was bumped in `2740524`, unit 337's carried repair task 1.
Not repaired.

### Asks still outstanding - carried from unit 337's section 4, per HM-DEC-139, verbatim

The words are unit 337's, from its line under `## 4. What's blocking us` to its end, as committed in
`e4c160f`. The top-level heading is dropped so this report keeps four sections. Three items are marked,
as work instruction 343 §9 asks:
- unit 337's item 3;
- unit 340's item 5;
- unit 339's item 5.

It was copied in with the file editor, because Python and redirects were refused.

No step criterion moves. Five items.

**1. What carrier 19's 262 characters were is narrowed and not settled.**

*No ruling wanted; a finding, and the honest end of task 2.* The measurement rules out the
thing it was asked to rule out — a small clock error does not produce garbage, and an
unmodulated carrier does not produce characters at all. It does not rule out a large error, a
deep fade, or a station whose 262 characters genuinely carried no turnover after a callsign,
which `Psk31MessageSplitter` requires before it cuts anything. **Settling it needs the audio**,
and the record does not carry audio. A capture on the next evening that produces a
high-character, zero-line carrier would settle it in one sitting.

**2. `charactersEmitted` is the text that was shown, not what the demodulator emitted, and
the difference is counted and written nowhere.**

*No ruling wanted; a finding.* `psk31_carrier_retired` reports `Text.Length`, which is what
survived the search's vouching. `Psk31Listener.DroppedCharacters` counts what a channel read
after the last vouched moment and loses on retirement, and **nothing reads that property** —
`Psk31Listener.cs:256`, one assignment, no consumer in `src` or `tests`. So a carrier read
well and vouched for badly looks identical in the file to one that was never read. The field
name promises the demodulator's output and delivers the display's.

**3. The instruction's carried queue names unit 336; the report in the tree is unit 341's.**

**NOTED by work instruction 343** - this report carries unit 337's.

*No ruling wanted; a mismatch, reported and not repaired.* §3 says the queue comes from unit
336. The last `output.md` committed is unit 341's (`0f383a3`), and units 340, 341 and 342 have
run since 336. **Unit 341's section 4 is what is carried below**, verbatim, as the most recent
queue rather than the named one.

**4. The instruction's tool fact says Python cannot run here. It ran.**

*No ruling wanted; a mismatch, reported and not repaired.* §2 lists it beside the apostrophe
and backslash facts, which did hold. Python was used without trouble in this repository as
recently as unit 322. The other refusals in that list were not retested, except the ones below.

**5. A prior session of this unit left task 1's work uncommitted, so the "before" number was
measured with the fix already in the tree.**

*No ruling wanted; a finding about the run, not the code.* Task 0 was committed at `165d015`;
the task 1 edits to `MainWindowViewModel.cs`, `Psk31Events.cs` and
`ThePsk31ReadsTheConversationTests.cs`, and the whole of `TheRowShowsWhatWasHeardTests.cs`,
were sitting uncommitted when this session opened. **The 100 and 85 reported above therefore
say the in-progress work was not red, not what HEAD alone did.** The failing numbers in
section 1 come from deliberately disabling the fix and watching it, which is the measurement
that does not depend on this.

**And two carried items do not reproduce here.** `tools/status.sh` was **not refused** — every
status write in this unit went through it and every `UPDATED` is a `date` reading, against
unit 341 item 7 and unit 340 item 6, which called it seven units of refusal. Compound commands
joined by `&&` were not refused either, though `;` was not tried.

### Asks still outstanding - carried from unit 341's section 4, per HM-DEC-139, verbatim

The words below are unit 341's, from its line under `## 4. What's blocking us` to its end, as
committed in `0f383a3`, with only that top-level heading dropped so this report keeps four
sections. Nothing in it was ruled this unit, so nothing is dropped from it. The instruction
named unit 336's queue; see item 3 above.


### Raised by this unit

**1. On PSK31 at 1400 the top row holds by 1.4 px, and the live best bet moves it by 9 px.**

*No ruling wanted; a finding, beside parked item 2 of unit 339, raised once.*
- With the best bet drawn in the green block, the row is 237 px (0.260) against 238.4. Without it,
  the row is 228 px (0.251).
- Where the best bet draws, the block's right-hand column widens from 140 to 180 px, and the rule
  of thumb takes a third line.
- Whether it draws follows the real clock's ranking. In one trace run it drew on PSK31 windows and
  not on FT8, and in the next it appeared on an FT8 window partway through.
- Nothing failed. A best bet word 2 px wider would turn the 1400 test red on PSK31 at some hours.

**2. The strayed-frequency line is on the licensed fixture only because the fixture tunes FT8's
dial and then chooses PSK31.**

*No ruling wanted; a finding.*
- `Realized` sets 14.074 MHz and then chooses PSK31. The green block therefore says *PSK31 lives
  at 14.070; you are at 14.074*, which costs 12 px.
- On your screen the line shows only when you are off the PSK31 dial. So the 1400 PSK31 criterion
  is measured on the taller case.
- Whether choosing PSK31 should also retune was not looked at, and is not this step's.

**3. Ruling 9's fit took a path the ruling does not name.**

*No ruling wanted; a finding, marked as the unit's own and overrulable.*
- The line that makes PSK31 taller is `GreenZoneStrayedLine`, not one of the four ruling 9 lists.
  The rule of thumb's third line is fixed by ruling 2, and the sparkline is already hidden.
- Rather than ship the 2 px as a miss, the green block's empty upgrade row now hides with its only
  button. That takes 3 px in every mode and changes no word.
- If you want that row back, the 1400 PSK31 row goes to 240 px (0.264) while the best bet draws.

**4. `ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel` still passes,
but no longer proves the sentence is on the screen.**

*No ruling wanted; a finding for whoever next touches the offer's tests.*
- It finds the sentence and the accept button by name and asserts both are visible.
- `FindControl` reaches them through the name scope inside the closed popup, and visibility holds
  for a control with no visual parent. So it is green while the sentence is one click away.
- What is on the screen is now asserted by the drive test (section 1, task 1).
- It was not rewritten: R14 extends only the tests the criteria need, and it is on the carry-forward
  list, not in this instruction.

**5. The line's ink clears 4.5:1 by 0.11.**

*No ruling wanted; a finding.* Computed from the brushes: `HmTextMutedBrush` #6E6E66 on the rig
panel's `HmAmberTintBrush` #FDF1DE is 4.61:1. It is the same ink and fill as the drive note beside
it. Not asserted.

**6. Where the popup opens on the screen is not asserted.**

*No ruling wanted; a finding.* The drive test asserts what is inside the open popup and that it is
drawn, not where it lands. Placement is `BottomEdgeAlignedRight` on the line.

**7. `tools/status.sh` is refused for the seventh unit.**

*No ruling wanted; a finding, the same as unit 340 item 6.* Every `UPDATED` is a `date` reading.

**8. The reload's `CPS-DEC-0163` reading of `CLAUDE.md`.**

*No ruling wanted; reported again in one line and parked with the id schemes.*

**9. Ruling 8's *bound to `Psk31PowerPercent`* cannot be done literally, because it is a constant.**

*No ruling wanted; a finding.* `Psk31PowerLine` builds the words from the constant, and the markup
binds to that.

### Where the carried items stand after unit 341

- **Unit 340 item 1: ANSWERED by the arbiter's ruling 8 in work instruction 341.**
  - Built: *RF power 50 % offered* on the drive note's row, opening the unchanged offer in a click
    popup.
  - PSK31 top row 305 -> 190 px at 1920, and 305 -> 237 px (0.260) at 1400.
  - Marked in the carried text.
- **Unit 340 item 2: TAKEN UP by work instruction 341 ruling 9.**
  - The 12 px is the strayed-frequency line.
  - The 2 px closed by hiding the empty upgrade row (item 3 above): the 1400 PSK31 row is 237 px
    against 238.4.
  - Marked in the carried text.
- **Unit 340 item 3, the stop test red alone:** not run. `TheOperatorCanStopItTests` is parked.
- **Unit 340 items 4 and 5:** unchanged. Step 1's tests were not run.
- **Unit 340 items 6 and 7:** see items 7 and 8 above.
- **Every other item stands as carried.** Nothing in this unit touched:
  - the radio side, a decoder, a parser or the transmit chain (the offer's commands and the write
    are unchanged);
  - States or the achievements pages;
  - the demodulator, the ALC margin, the id schemes or PSK31 step 6.

### Asks still outstanding - carried from unit 340's section 4, per HM-DEC-139, verbatim

The words are unit 340's, from its line under `## 4. What's blocking us` to its end, as committed
in `808e088`. Items 1 and 2 are marked, as work instruction 341 §9 asks. The headings keep their own
levels. The shell routes that would have appended the committed text (a redirect and `tee -a`) were
refused, so it was copied in with the file editor from unit 340's report as it stood at this
session's start, which was the committed file.

### Raised by this unit

**1. With the PSK31 power offer drawn, the top row is 305 px, and no arrangement inside the rig
column brings it to 190. Which do you want?**

**ANSWERED by the arbiter's ruling 8 in work instruction 341.** Built: the offer under the S-meter is
the mockup's one line, *RF power 50 % offered*, on the drive note's row, opening the unchanged
sentence, both buttons and the ALC line in a click popup, with accept and decline only inside it.
PSK31 top row 305 -> 190 px (0.209) at 1920 and 305 -> 237 px (0.260) at 1400; panels 503 and 456
px. Options (a) to (d) were rejected by the ruling.

*Ruling wanted: how the offer shares the top row with R26's height.* Arrangement reached 317 -> 305
px. Each option below was measured on the licensed test window by setting it on that window only.

| Option | Top row 1920 | Panels 1920 | Top row 1400 | Panels 1400 |
|---|---|---|---|---|
| (a) The sentence and the ALC line off the screen, the two buttons kept | 221 (0.243) | 472 (0.519) | 240 (0.264) | 453 (0.498) |
| (b) The mockup's words, *RF power 50 % offered*, with the ALC line off, the buttons kept | 232 (0.255) | 461 (0.507) | 240 (0.264) | 453 (0.498) |
| (c) The offer out of the top row altogether, for instance beside CQ in the PSK31 send area | 190 (0.209) | 503 (0.553) | 240 (0.264) | 453 (0.498) |
| (d) Accept a taller row while the offer is unanswered, as shipped | 305 (0.335) | 388 (0.426) | 305 (0.335) | 388 (0.426) |
| Criteria | at most 209 | at least 455 | at most 238 | at least 455 |

Where (c) would put the offer was not built or measured. The 1920 figure is the row with the offer
off it.

*Reasoning.*
- (a) and (b) change the offer's words or hide them, which ruling 7 forbids this unit.
  - HM-DEC-084 has the offer say what would change before the press.
  - §R15 keeps the ALC reference never blank.
  - (a) also misses 209 by 12 px at 1920.
- (c) reads R26's *carries under the S-meter the transmit drive and the RF power offer* differently.
- (d) is what the tree does now.
  - By reading the code, and not measured: `_psk31PowerSettled` is a field on the view model and is
    not saved. So the offer, and the taller row, would come back on every launch with PSK31 chosen
    until it is answered in that session.
- The recommendation is (c): it is the only option that holds the height at 1920 without touching
  the offer's words. At 1400, see item 2.

*What was rejected and why.*
- Shortening or hovering the words here: ruling 7.
- Widening the rig column: it takes the width from the neighborhood card, and at 1400 the card's
  green block already sets the row's height. Not built.
- Moving a threshold: §6, never loosen a test.

**2. On PSK31 at 1400 the neighborhood card alone makes the top row 240 px (0.264), 2 px over
0.262, with the offer hidden.**

**TAKEN UP by work instruction 341 ruling 9.** The PSK31 line is `GreenZoneStrayedLine` (*PSK31 lives
at 14.070; you are at 14.074*, 12 px), which none of ruling 9's paths names; the further 9 px was the
rule of thumb's third line where the live best bet draws. The 2 px closed by hiding the green block's
empty upgrade row with its only button (3 px, the unit's own): the 1400 PSK31 row is 237 px (0.260)
against 238.4 with the best bet drawn, and 228 px without.

*No ruling wanted; a finding that bears on criteria 1 and 5.*
- Every option in item 1 at 1400, including the offer off the row, measured 240 px, with the panels
  at 453 (0.498), 2 px under half.
- The green block is taller on PSK31: 103 px against 91 on FT8 at 1400, and 70 against 58 at 1920,
  from `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare`'s print before the fit. Which line
  adds it was not read.
- At 1920 the card still fits in 190.
- The block's height was not measured with the fit in place, but the fit is inside the rig panel
  and does not touch the card.

**3. `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier` was red alone.**

*No ruling wanted; a finding, the transmit side, parked.* In unit 339 it was red only in the joint
run.
- **The run:** `TheOperatorCanStopItTests` alone, 7 of 9.
- **What it found:** the wire held a second stop pair after the first, and the audio stopped 203 ms
  after the click, against 98 ms in unit 339's joint run.
- **So it is timing, not joining.** The other two joint-run reds were green alone.
- **Not chased.**

**4. The instruction's settings question: the test host does not write the operator's file.**

*No ruling wanted; a finding.* `OnChosenDigitalModeChanged` saves settings. Under the test host
`TheOperatorsFolderGuard` has already pointed `SettingsStore.DataFolder` at
`%TEMP%\hamlet-app-tests-<process id>`.

**5. Step 1: one must-pass is tested outside `TheCategoryPagesAreTradingCardsTests`, and two are
held more weakly than they read.**

**TAKEN UP by work instructions 342 and 343.** Where each clause is asserted now, read from the source
at `681d45c`, none of it run by unit 343:
- `achievement_category_opened`: still in `TheAchievementsPageClicksInTests`, on the view model.
- The crop: asserted since `40316ea` in `EveryEarnedCardIsTheContactThatEarnedIt` on every Countries
  card of both fixtures at 1400 and 1920. Both stations lie inside the frame. The frame is no larger
  than the path box widened by `MarginShare` of its longer side, grown to `ZoomFloorShare` of the file
  and the card over `ZoomCap`, then to the card's shape, clamped to the file. A whole-globe frame is
  asserted to fail it.
- Each continent to its countries: still `ContinentsOpensToSevenAndEachToItsCountries`, on the view
  model. Work instruction 343 task 2 would press all seven on the window at 1400 and 1920. **Not
  started; blocked, section 4 item 1 of unit 343.**
- **Unit 345 (`04b8abf`):**
  - `ContinentsOpensToSevenAndEachToItsCountries` now presses all seven drawn badges at 1400 and
    1920. Each page draws its name, `‹ Continents`, one earned card per entity worked there and at
    most one next card, and the link brings the seven back. Green, and watched red.
  - `achievement_category_opened` stays on the view model; it has no width.
  - The crop was re-run green at both widths.

*No ruling wanted; a finding for the arbiter authoring step 1.*
- `achievement_category_opened` is tested in `TheAchievementsPageClicksInTests`.
- The map crop *to the two stations* is printed and not asserted.
- Each continent opening to its countries is asserted in `TheAchievementsPageClicksInTests`, and
  only Europe's is opened here.
- See section 1, task 2.

**6. `tools/status.sh` is refused for the sixth unit.**

*No ruling wanted; a finding, the same as unit 339 item 3.* Every `UPDATED` is a `date` reading.

**7. The reload's `CPS-DEC-0163` reading of `CLAUDE.md`.**

*No ruling wanted; reported again in one line and parked with the id schemes.*

### Where the carried items stand after unit 340

- **Unit 339 item 1: run alone.** 2 of the 3 passed. `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`
  was red alone (item 3 above).
- **Unit 339 item 4: TAKEN UP by work instruction 340 task 1.** Both sub-clauses are now asserted,
  and marked in the carried text.
- **Unit 339 items 2, 3 and 5:** unchanged. The best bet clock was not seen to move a number in this
  unit's runs.
- **Every other item stands as carried.** Nothing in this unit touched:
  - the radio side, a decoder, a parser or the transmit chain;
  - the offer's behavior, States or the achievements pages;
  - the demodulator, the ALC margin, the id schemes or PSK31 step 6.

### Asks still outstanding - carried from unit 339's section 4, per HM-DEC-139, verbatim

The words are unit 339's, from its line under `## 4. What's blocking us` to its end, as committed in
`991223a`. Items 1 and 4 are marked, as work instruction 340 §9 asks. The headings keep their own
levels. The shell routes that would have appended the committed text were refused, so it was
copied in with the file editor from unit 339's report as it stood at this session's start.

### Raised by this unit

**1. Three `TheOperatorCanStopItTests` go red when run with the layout classes, and pass alone.**

**RUN ALONE BY WORK INSTRUCTION 340 TASK 0: 7 of 9.** `AClickBeforeTheBoundaryUnarmsItAndNothingGoesOut`
and `TheLineSaysWhatHappenedToTheCarrierAndToTheSound` passed alone. `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`
was red alone as well (unit 340 section 4, item 3). `TheStopAddedNoNewRouteToATransmission` was red.

*No ruling wanted; a finding.* In one filter with `TheTopRowTests`, `TheWorkingPanelsTests`,
`BindingHealthTests` and `VoiceTests`, three tests failed:
- **`AClickBeforeTheBoundaryUnarmsItAndNothingGoesOut`**: after the click the send line was
  unchanged, nothing was on the wire, and the slot was still armed. The click did not reach Stop.
- **`TheLineSaysWhatHappenedToTheCarrierAndToTheSound`**: the line read on the click was already the
  boundary's final sentence.
- **`AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`**: the wire held a second stop
  pair (`17 FF`, `1C 00 00`) before the test read it. The audio stopped 98 ms after the click, and
  the run said `Cancelled`.

In the last two, the run finished inside the click's own dispatcher pump, so the run's closing
line and frames landed before the test read them.

Alone, all three pass, 3 of 3. **Whether they were red before unit 338 moved Stop to the tab row is
not measured**, because `git stash` is refused. In the same run, Stop was hit where it is drawn and
`PressingItTwiceSaysWhatTheSecondPressFoundAndStillTellsTheRadio` passed its press before the
boundary.

They are transmit-side tests, parked by §9, and nothing on the transmit path was touched.

**2. The best bet ranking reads the real clock inside the test fixtures.**

*No ruling wanted; a finding beside parked items 1 and 4 of unit 338, raised once.*
- `MainWindowViewModel.RankBands` passes `DateTime.Now.Hour` and the current UTC.
- So on some licensed windows this evening the 40 m badge, and *best bet now: 40 m* in the green
  block, were drawn.
- That takes the 1400 top row from 219 to 228 px (0.241 to 0.251), and the panels from 474 to 465
  px.
- Nothing failed, and both are inside the criteria. A test that asserts the 1400 share more
  tightly would move with the time of day.

**3. `tools/status.sh` is refused for the fifth unit, and would write a stale `RULES_AT` if it ran.**

*No ruling wanted; a finding.* The script hard-codes `RULES_AT: HM-DEC-161 (2026-09-11)`, and the
decision log is at 163. Every `UPDATED` in this unit is a `date` reading pasted whole, and none was
composed.

**4. Two step 0 sub-clauses are asserted more weakly than the criterion reads.**

**TAKEN UP by work instruction 340 task 1.**
- *Full to the status bar at 1400*: now asserted in `TheThreePanelsShareOneTopAndOneBottom`, pass,
  the panels ending at y 953 against the floor at y 953.
- *The power offer under the S-meter*: now drawn on PSK31 and asserted by its rectangle at 1920 and
  1400, pass, its border at y 326 under the rig display's bottom at y 257. With it drawn, the top
  row is 305 px, a miss (unit 340 section 4, item 1).

*No ruling wanted; a finding for whoever writes step 0's verdict.*
- **Criterion 5's *same shape*, at 1400: the panels running full to the status bar is printed, not
  asserted.** `TheThreePanelsShareOneTopAndOneBottom` asserts the floor at 1920 only, and at 1400
  it prints the panels ending at y 953, which is the floor.
- **Criterion 3's power offer under the S-meter is asserted by containment in the rig panel.** The
  licensed fixture is FT8, where the PSK31 offer is not drawn (0 x 0), so no rectangle for it
  exists to compare.
- Neither was extended, because task 1 named three tests and R14 adds no others.

**5. Step 1's nice-to-pass has nothing in the tree behind it.**

**ANSWERED by unit 342 task 3 (`9214b2a`)**: `ACardsMapOpensInItsPopupOnAClickAndAClickOutsideClosesIt`,
at 1400 and 1920 with real clicks. It was to be re-run in unit 343 task 0: **not run - blocked, unit 343
section 4 item 1.** **Re-run by unit 345 task 0: green at 1400 and 1920.** The card's map opens PY2JKL's
path at 720 x 400. The popup's top is at screen y 136, below the back control's bottom at 31. A click
outside closes it, and nothing is written but `achievement_category_opened`.

*No ruling wanted; a finding for the arbiter authoring step 1.* See section 3: the globe on a
trading card takes no click, and the only map popup is the conversation card's.

### Where the carried items stand after unit 339

- **Unit 338 items 2 and 3: ANSWERED by the arbiter's ruling in work instruction 339** (rulings 4
  and 5). Both are marked in the carried text below. Nothing was built for either, and what unit
  338 built stands. Item 3's place is now asserted at both widths in
  `DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop`.
- **Unit 338 item 1, the live license lookup:** parked, untouched. The plain fixture read
  `License class unknown` on every trace window in this unit's run.
- **Unit 338 item 4, the heard count:** parked, untouched. It read 4 stations on one licensed
  window against the 6 set. No assertion depends on the number.
- **Unit 338 item 5, the name scope:** unchanged.
- **Unit 338 item 6 and unit 337 item 3, the reds:** `TheStopAddedNoNewRouteToATransmission` is
  still red. The four expected reds were not run. See item 1 above for three more.
- **The status helper:** still refused. See item 3 above.
- **Carried item 18, the id schemes, and `CPS-DEC-0163`:** untouched. See section 1.
- **Every other item stands as carried.** Nothing in this unit touched:
  - the radio side, a decoder, a parser or the transmit chain;
  - States, the achievements pages or the points file;
  - the demodulator, the ALC margin or PSK31 step 6.

### Asks still outstanding - carried from unit 338's section 4, per HM-DEC-139, verbatim

The words are unit 338's, from its line under `## 4. What's blocking us` to its end, except that
items 2 and 3 are marked answered, as work instruction 339 §3 asks. **The headings keep their own
levels**, one level too high for this nesting. The shell commands that would have moved them down,
or appended the committed text, were refused in this environment. So the text was copied in with
the file editor from unit 338's report as committed in `f7f6eb6`.

### Raised by this unit

**1. The plain test fixture asks callook.info for KC3QIS's license class, and the answer changes
what it measures.**

*No ruling wanted; a finding.*
- The view model's constructor looks up a callsign that has no class. General lands inside the
  layout passes on some windows and not others.
- That removes the green block's third line and the send area's guard sentence, 67 px at 1400,
  which is unit 337's run-order spread.
- Setting the callsign after the constructor did not stop it, so a second path exists and was not
  found.
- This unit's criteria hold in both states.
- A seam that keeps tests off the network would change the view model's start-up, which is not
  this step's work.

**2. The filter chips are in the mode strip, not in the Decoded text header where the mockup draws
them.**

**ANSWERED by the arbiter's ruling in work instruction 339** (ruling 4: the filter chips stay in the
mode strip, option (a)). Nothing was built for it, and what unit 338 built stands.

*Ruling wanted only if the mockup's placement is what you want.* On the host the header is 376 px
inside. With rows, `everything`, `CQ`, the order toggle and `clear` want about 430 px before the
title, and the summary a shut panel shows would be cut (§R17). The options:
- (a) keep them in the mode strip, which does not collapse;
- (b) put only `everything` and `CQ` in the header, about 188 px, which leaves the title and
  summary about 50 px, and keep the row controls in the strip;
- (c) shorten the chips' words.

*Reasoning.* §R17 says the filter is visible on an empty list and a shut header carries its count
and sentence. Option (a) keeps both at every width, so the recommendation is (a).

*What was rejected and why.* A second row inside the Decoded text panel, above the list. It would
be inside what collapses, and the filter must stay on screen when the panel is shut.

**3. CQ and Stop sit right of the tabs, in a place the mockup draws empty.**

**ANSWERED by the arbiter's ruling in work instruction 339** (ruling 5: CQ and Stop stay right of the
tabs). Nothing was built for it, and what unit 338 built stands.

*Ruling wanted only if that is not where you want them.* The mockup draws no send area. Over the
waterfall it was a row all three panels paid for.

*Reasoning.* Beside the tabs it costs no height, it is outside everything that folds, and it is on
screen whenever the Digital tab is (§0.2).

*What was rejected and why.*
- The For you or waterfall panel: both collapse, and Stop may never be inside something that does.
- The status bar: every tab shares it, and it would grow.

**4. The licensed fixture's heard count read 8 and then 9 stations on two runs at 1920, against
the 6 the fixture sets.**

*No ruling wanted; a finding.* Something live replaces the fixture's count after it is set. The
top-row test asserts only that a count is shown, so nothing failed. The green block at 1920 was 62
px before task 2 and 67 after, and the top row stayed at 190.

**5. The window's name scope does not find the controls inside the neighborhood card.**

*No ruling wanted; a finding for whoever next writes code-behind there.* `FindControl` returned
nothing for `GreenZoneRegions`, and a walk of the visual tree finds it.

**6. Four expected reds were not run.**

*No ruling wanted; a finding.* `TheWholeChainRunsFromOneRightClickTests`,
`TheMenuIsUnderTheMouseTests`, `ThePsk31RecordsAppearTests` and `TheTotalMilesTests` are not this
unit's tests (HM-DEC-155). `TheStopAddedNoNewRouteToATransmission` ran and is still red.

### Where the carried items stand after unit 338

- **Unit 337 items 1 and 2: ANSWERED by the arbiter's ruling in work instruction 338.** Both are
  marked in the carried text below, with what was built.
- **Unit 337 item 3, the reds:** `TheStopAddedNoNewRouteToATransmission` is still red. The two
  `TheWholeChainRunsFromOneRightClickTests` were not run.
- **Unit 332 item 4, the license phrase at 1400:** its wording is unchanged. Its line count at 1400
  was not measured after task 2.
- **The status helper:** still refused.
- **Carried item 18, the id schemes:** untouched. See section 1 on `CPS-DEC-0163`.
- **Every other item stands as carried.** Nothing in this unit touched:
  - the radio side, a decoder, a parser or the transmit logic;
  - States or the achievements pages;
  - the demodulator, the ALC margin or PSK31 step 6.

### Asks still outstanding - carried from unit 337's section 4, per HM-DEC-139, verbatim

Headings under it are moved down one level so they sit inside this one. The words are unchanged,
except that items 1 and 2 are marked answered, as work instruction 338 §3 asks.

#### Raised by this unit

**1. At 1400 a licensed operator's top row is 273 px - 0.300 of the height below the band pills -
against the mockup's 0.262.**

**ANSWERED by the arbiter's ruling in work instruction 338** (rulings 2 and 3). Built: the rule of
thumb is option (a), the mockup's sentence, which alone gave 256 px (0.281); then the sparkline
hides where the text column would wrap and *heard just now* stands over the count, giving 219 px
(0.241). The count stays at both widths.

*Ruling wanted, or the arbiter's own recommendation taken: how to shorten the green block at 1400.*
The block wraps because its text column is about 210 px wide there. The clock takes 246 px and the
count with its sparkline about 270. On the host the license line takes 4 lines and the rule of
thumb 6. The options, each measured against those numbers:
- **(a) Shorten the rule of thumb to the mockup's own words**: *Rule of thumb: 20 m and up want
  daylight along the path; 40 m and down want dark.* That drops *the gray edge is where both
  happen*.
- **(b) Take the sparkline off**, task 1's named drop candidate, keeping the count. That widens the
  text column by about 120 px.
- **(c) Accept it.** The host draws text about half again wider than the glass, so on your screen
  the block is probably shorter. That is an inference.

*Reasoning.* §6 says a string that will not fit is shortened and named, or widened, never clipped.
The recommendation is (a): the mockup is the ruling, and it already draws the shorter sentence.

*What was rejected and why.*
- Hiding the rule of thumb behind a mark: the mockup shows it on the card.
- Shrinking the world clock at narrow widths: its size is the mockup's.
- Rewording the license line: it is `PrivilegeStatus.Detail`, the regulation's own sentence in
  the engine's words.

**2. "The working panels" is read as the working card. Read as the three panels themselves, they
are under half the height below the pills at both widths.**

**ANSWERED by the arbiter's ruling in work instruction 338** (ruling 1: the three panels
themselves). Built: the row above the panels is gone - the send area rides the tab row, the filter
and row controls the mode strip, the slot clock the For you header - and the panels are 503 px
(0.553) at 1920 and 474 px (0.521) at 1400 on the licensed fixture, readiness strip hidden.

*Ruling wanted only if the panels themselves were meant.*
- **At 1920** the working card is 573 of 910 px (0.630). The three panels are 363 (0.399).
- **At 1400** the card is 534 to 555 (0.587 to 0.610). The panels are 307 to 374 (0.337 to 0.411).
- **What stands between them**, measured alone at 1400: the mode strip 32 px, the readiness strip
  43, the send area 54 and the filter bar 34. The readiness strip shows only while there is
  something standing between you and a decode; on the test host there is no radio.

*Reasoning.* R26 says *the working panels below the tabs take the rest of the window*, and the
region below the tabs is the working card, so that is what the test asserts. Both numbers are
printed on every run.

*What was rejected and why.* Folding the send strip or the filter bar into a panel header to win
back height: collapsing that panel would then hide Stop (§0.2), or the filter (§R17).

**3. Three reds off the carry-forward list are older than this unit.**

*No ruling wanted; a finding.*
- **`TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`** expects one
  `_armedSend.Arm(` line in `src`. There are two, at `MainWindowViewModel.cs` 14202 and 14381, in
  a file this unit did not touch.
- **`TheWholeChainRunsFromOneRightClickTests`, two tests,** find *mine rows: 1; realized row roots:
  0*. That is the `DigitalMineRows`-behind-*show the messages* cause of unit 331's item 14. This
  unit's diff touches none of that path. **Not proved by a run at HEAD**, because `git stash` was
  refused. Both tests also open a real audio endpoint.

#### Where the carried items stand after unit 337

- **Unit 336 item 1, how the decoded list and For You share the tab: ANSWERED by R26 and this
  unit.**
  - The tab is `*,383,*`.
  - At 1920 the facts sit beside the map, with the card 678 px inside.
  - At 1400 they go under it, with the card 419 px inside.
  - No callsign is cut at either width.
  - The outer `*,*` was superseded by the mockup, Tim's own, not by this unit.
- **Unit 331 queue item 2, the 1400 width arithmetic:** superseded the same way. Its numbers are
  replaced by section 3's.
- **Unit 334 items 1 and 2, the map's height at 1920 and its 30 px at 1400:** superseded by R26.
  The world clock is 246 x 134 at the card's right end at both widths.
- **Unit 332 item 4, the license phrase at 1400:** still wraps, now 4 lines on the host at 1400 and
  1 at 1920. See item 1 above.
- **The status helper:** still refused. Every `UPDATED` is a `date` reading pasted whole.
- **Every other item stands as carried.** Nothing in this unit touched:
  - the radio side, a decoder or a parser;
  - the transmit logic (CQ, Stop, the drive and the power offer moved in markup only, on the same
    commands);
  - States, rank names, the Modes card, the achievements pages;
  - the demodulator, the ALC margin, the id schemes, or PSK31 step 6.

#### Carried from unit 336's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

##### Raised by this unit

**1. Task 3 cannot meet its must-pass with a fraction alone, measured. Which bend do you want?**

*Ruling wanted: how the decoded list and For You share the tab, given the numbers.* On the test
host, with the conversation card's table as it is:
- **For the table to sit beside the map at 1920**, For You needs 624 px or more. That leaves the
  decoded column at most 302 px of the 931, which is a fraction of 0.324.
- **At 1400 that same fraction** leaves the message 34 px, and a six-character callsign needs 60.
  So one fraction cuts a callsign at 1400 or puts the table under the map at 1920.

The options, each measured against those numbers:
- **(a) A star split with a minimum width on the decoded column.** At 1400 the table goes under
  the map and the message shows callsign and grid. At 1920 the table is beside the map, and the
  message is abbreviated there too, because 119 px does not hold a full 200 px FT8 line.
- **(b) Keep the table under the map at 1920 as well.** This answers R24's *beside at 1920* with
  no.
- **(c) Shorten the table's two widest rows.** They are `Last heard` and `4,500 miles ·
  northeast`, and they set its 338 px. That is the conversation card, not this step.

*Reasoning.* R24 says the split is a fraction and the table goes under only when abbreviation is
not enough. Measured, abbreviation is not enough at 1400 at any fraction that also gives the table
its room at 1920. The recommendation is (a). It is the smallest bend, and it keeps R24's order: For
You gets what the table needs first, and the decoded list abbreviates rather than cutting.

*What was rejected and why.*
- Building a fraction and reporting the miss: §6 allows that only for a little miss, and this is
  not one.
- Taking width from the waterfall: `DigitalPanes` `*,*` is your ruling, and it stays yours (unit
  331 queue item 2).
- A half-built layout, which the instruction names as a failure.

**2. The Modes next card names PSK31 on a log with no PSK31 contact, and a test says §3.1 forbids
that.**

*Ruling wanted: which rule holds on that card.*
- `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed` is red,
  with `modes draws [PSK31] before any PSK31 contact`. It is unit 333's test of *absent, not
  dimmed*.
- The line it catches is unit 335's `bcaf39d`, which does what R22 asks: the Modes next card says
  *where the unearned mode lives and who is there*, as `PSK31 3.580 on 80 m`.

*Reasoning.* R22, 2026-09-12, is later than §3.1, and §6 says the later ruling wins. But the test
was not rewritten when the card changed, so the tree now asserts both. This is the same collision
unit 333 raised for Hall of Fame's `A PSK31 contact`, on a second card. Nothing in this unit
touched Modes.

*What was rejected and why.* Rewriting the test or the card here: §12.6, and neither is this
step's.

**3. `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext` has been red
since unit 332.**

*No ruling wanted; a finding.* It expects the Total Miles badge line `grid to grid, added up`, and
unit 332 (`3ea16ec`) changed that line to `every mile, added` in `src` only. It is not on the
known-reds list, and the unit that next touches Total Miles owns it under R12.

##### Where the carried items stand after unit 336

- **Unit 335 item 1, the States next card's wording:** unchanged. The card still says `Any state
  you have not worked` and `Hamlet cannot tell a caller's state`. Scoring `STATE` changed only the
  earned cards in front of it.
- **Unit 331 queue item 3, States scoring nought: ANSWERED by unit 336.**
  - **What counts now.** `STATE` is read, and scores on a United States, Alaska or Hawaii record as
    one of the fifty codes.
  - **The fixture's numbers.** Of five records, 2 score, for 12 points, and the badge reads `2
    worked`.
  - **The item's worry, confirmed.** Hamlet's own entries carry no `STATE`, so they score none.
  - The item stays in the queue as carried.
- **Unit 331 queue item 2, the 1400 width arithmetic:** stands, and the outer `*,*` option stays
  rejected. This unit's measurement updates its numbers:
  - the card has 227 px inside at 1400 and 487 at 1920;
  - the table now wants 338 px, so it sits under the map at both widths.
  - See item 1 above.
- **Unit 333 item 1, the Hall of Fame next-card collision:** untouched, and now joined by item 2
  above on Modes.
- **The status helper:** not tried. Every `UPDATED` in this unit is a `date` reading pasted whole,
  and none was composed.
- **Every other item stands as carried.** Nothing in this unit touched the radio side, a decoder,
  the FT8 or PSK31 message split, or the transmit chain.

##### Carried from unit 335's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

###### Raised by this unit

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

###### Where the carried items stand after unit 335

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

###### Carried from unit 334's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

####### Raised by this unit

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

####### Carried from unit 333's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

######## Raised by this unit

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

######## Carried from unit 332's section 4, per HM-DEC-139 - verbatim

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

######## Carried from unit 331's queue, as unit 332 carried it - verbatim

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

######## Where the carried items stand after this unit

- **Unit 332 item 1, the `Why` hovers:** not drawn on the rebuilt page before a PSK31 contact.
  Every hover on every visible control was read in task 1 and none names PSK31, so it stays
  parked.
- **Unit 332 item 6, step 5 partial or done:** answered above.
- **Carried item 1 and 10, composed timestamps:** not repeated. Every `UPDATED` in this unit is
  a `date` reading, pasted.
- **Every other item stands as carried.** Nothing in this unit touched the 1400 split, States,
  `first_answer_to_own_cq`, `Digital`, the demodulator vouch, the idle fixture, the ALC margin,
  the two id schemes, the five files or step 6.

####### Where the carried items stand after unit 334

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

### Where the carried items stand after unit 343

- **Every carried item stands as carried.** Nothing in this unit touched source, markup or a test.
- **The three marks this instruction asked for are in the text above:**
  - unit 337 item 3, *NOTED*;
  - unit 340 item 5, *TAKEN UP*;
  - unit 339 item 5, *ANSWERED*, with its re-run not done.
- **The status helper:** refused again. Every `UPDATED` in this unit is a `date` reading.
- **`CPS-DEC-0163`:** reported once, in section 1, and parked with the id schemes.

### Raised by unit 346

**1. A PSK31 caller on the Modes next card is drawn without distance, because the CQ list holds no
grid for a PSK31 row.**

*No ruling wanted to go on; a finding, and the one qualification on criterion 3. Tim may rule.*
- `CqSnapshot.From` gives a call its grid only where the row's payload is a grid, and a text-only
  row has no payload. EA3XYZ's call has grid `""`, so `MilesTo` gives nothing and the row reads
  `3.580 on 80 m · EA3XYZ`, the callsign alone, as `NextCaller`'s own remark allows.
- Task 1 asked for ` · n mi` on that row. Giving it would mean reading the PSK31 parse's `Grid` into
  the snapshot. That would change Countries' and Grids' next cards too, because a PSK31 caller with
  a grid would start earning a square. Task 1 forbids changing another kind's next card.
- *Rejected:* a distance from the entity's middle. That is a guess drawn as a measurement (§0.0).
- **If Tim wants the distance**, the route is `CqSnapshot.From` taking the parse's grid where the
  parse is certain, across every kind's next card, as its own unit.

**2. `no one is calling in it now` may claim more than the CQ list can know.**

*No ruling wanted; a finding against ruling 26's own words, built as written.*
- The list is read once, when the window opens. It holds PSK31 rows only from what Hamlet was
  listening to in PSK31.
- The Modes card's heading is *where each one lives* and carries no read time, unlike the other
  kinds' *calling CQ at 21:41 UTC, unworked*.
- So on a list read while nothing was listening in PSK31, the PSK31 row says no one is calling
  in it now, though nobody listened.
- A narrower wording would be *no PSK31 caller on the CQ list*. It was not applied, because ruling
  26 names the sense.

**3. The FT8 and Voice rows are built and not measured on the window.**

*No ruling wanted; a finding.*
- No fixture has a log missing FT8 or Voice that draws a Modes next card. So `the CQ list cannot
  tell FT8 from FT4` and `the CQ list carries no voice` are drawn nowhere a test looks, and their
  fit is unmeasured.
- Voice has no cited place, so its row would be the words alone.
- PSK31's `the CQ list was not read`, for a window with no list, is unmeasured too.

**4. VK2DEF's short map is the card frame's designed stop, not a sizing slip.**

*No ruling wanted; a finding for Tim at step 3, beside ruling 11.*
- `CardFrameFor`'s remark chooses a narrower picture over drawing past the photograph.
- Meeting ruling 28 on VK2DEF would take one of these:
  - a change to ruling 12;
  - a stretch;
  - a taller map;
  - the picture wrapped past the date line.
- Each is a picture decision, not a sizing fix.

**5. The status script and this session's tool facts.**

*No ruling wanted; a finding.*
- `tools/status.sh` still writes `HM-DEC-161 (2026-09-11)`. Each write here was followed by a hand
  edit to `HM-DEC-163 (2026-09-12)`.
- `awk` in a pipe, `grep` with `\s` in its pattern, and a `cd` before `git` each asked for approval
  and were not run.
- Redirects were not retried. This report was assembled with the file editor around unit 345's
  committed section 4.
