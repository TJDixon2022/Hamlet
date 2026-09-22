READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Steps 1, 2, 3, 6 and 8 done, step 2
   closed last night by a judging session; step 0 partial with 0.1 cut down and
   closed to units; step 4 partial with 4.3 logged to the owner; step 7 partial on
   an unjudged 7.2; step 9's work landed at e5e4bee0 and ungraded; step 5 Tim's own
   and it ends the run. Step 10 is the only step a unit can move and this is the
   first unit ever spent on it.
B. Step 10 - what Tim saw on 2026-09-22, R46. 10.1 MET: the unit and line that took
   favorites off the screen are work instruction 029's a51bc2a6 of 2026-08-27
   14:08, src/Hamlet.App/Views/MainWindow.axaml, where the favorites ComboBox, the
   favorite's name and its note left in the same Grid as Tim's ruled removal of the
   recent-places row and were written down nowhere; the saved frequencies DO
   survive the loader, 2 of 2 and 2 of 2 again after a save; and favorites are back
   by a caret beside the star on the rig face opening the view model's own
   FavoriteMenu, drawn and hittable at all nine of unit 354's sizes, one click
   measured tuning a dial 20 kHz away back to 14,074,000 Hz.
   10.2 MET, with one qualification named in section 4: 9 of 9 fixture rows open a
   menu where 3 of 9 did.
   10.3 NOT MET: the sun map is 246 x 134 where it was 246 x 134, in a band of
   214 px against 6.1's ceiling of 220 - because the map already governs that band
   and letting it fill measured the band at 460.
   10.4 MET, and it was the named drop candidate and was not dropped.
   10.5 MET: app 265 of 265, engine 150 of 150.
C. The report last. Section 4 raises 7 items on top of the carried fifty-three,
   and none of them is in the way of a criterion in B - item 1 qualifies 10.2 and
   item 2 is 10.3's own arithmetic, and both are reported in B already. In one
   line: what took favorites away was NOT this phase's own step 6 but an earlier
   ruling of Tim's - 2026-08-27, the recent-places row - which a unit obeyed by
   deleting the favorites list in the same commit without being asked to and
   without writing it down.

```
UNIT:       387 - complete at task 6 of 7 - 2026-09-22 11:36
PHASE GOAL: Hamlet keeps what it has already earned - the screen, record and test
            work banked in the PSK31 and Olivia threads - proved by tests that ran
            without the radio or the owner, and at the end by Tim at his window.
UNIT GOAL:  Give Tim back the four things off his own screen on 2026-09-22:
            favorites, with the change that took them named before anything was
            rebuilt; a right-click menu on every decoded row and not only the ones
            that named a station; the sun map filling its band; and no invented
            characters from a carrier the blind search found but cannot read.
ADVANCED:   step 10, criteria 10.1, 10.2, 10.4 and 10.5 - 10.3 measured, reported
            partial and left unticked
NUMBER:     decoded rows that open a right-click menu: 3 of 9 -> 9 of 9;
            and the sun map's share of its band: 62.6% -> 62.6%
DRIFT:      none
```

## 1. What Claude did

**Complete, at task 6 of 7 - every one of the seven tasks was entered, and the named drop
candidate was taken rather than dropped.** Windows 11, `C:\Source\HamLet`, branch `main`, project
claimed Hamlet and the gate's four conditions all hold: `SHACK_FACTS.md` present,
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present, neither `CoreHMI.sln` nor
`MURC.sln` at the root.

**Task 0 - the record and the entry round.** Version 1.13.73 to 1.13.74 with its line in the
version log; `PHASE_STATUS.md` `CURRENT_STEP` 0 to 10 and `WORK_INSTRUCTION` 386 to 387, both
stale as section 5 said. Its `STEP: 2` line already read `done` and was left alone; its `STEP: 9`
line gained one sentence carrying section 6 ruling 1 and nothing else. `PHASE_PLAN.md` ticked
nothing. The instrument was counted rather than assumed: line 7 carries **59** filter terms and
line 9 **25**, exactly section 5's numbers, and both are read out of the file by a one-line script
and evaluated rather than retyped, so nothing this unit did could edit either command line. Entry
round **app 245 of 245 and engine 150 of 150, both green on the first attempt** - section 5's
expected numbers exactly, so unlike unit 386 this unit's measurement agreed with its instruction.

**Task 1 - the trace, and no file under `src` moved.** All four items measured before anything was
built. It is not on the carry-forward list, for the reason `Unit378Trace` is not.

**Task 2 - 10.1.** The archaeology first, because the record comes before the rebuild, then the
caret on the rig face.

**Task 3 - 10.3.** The attempt was made, measured, and reverted in the same task; the assertion was
rewritten under R12 and the criterion is reported partial with both numbers.

**Task 4 - 10.2.** Every decoded row opens a menu; three existing names rewritten under R12 because
the owner replaced what they asserted.

**Task 5 - 10.4, the named drop candidate, NOT dropped**, because task 1 item 4 found the per-block
confidence already crossing the seam.

**Task 6 - the exit round, the plan, the status and this report.** The exit app invocation's first
attempt found a red on an assertion that was **this unit's own** and on a carry-forward name; it
was repaired under R12 and committed rather than worked around, and the round was re-run.

### The decisions this unit made for itself, reproduced in full

**1. The band's ceiling for 10.3 is 214 and not 220, and that is a disagreement with the
instruction.** Section 6 ruling 2(b) and section 8 task 3 both name 6.1's **220 px** as the
constraint. The tree's own carry-forward guard is **tighter**:
`Unit376TheTopBandTests.TheTopBandIsOneShortRowAndThePanelsAreTallerByTheDifference` asserts the
band at or under `BandReachedWithThePills`, which is **214**, and section 8 task 3 also says that
name must be green when this unit finishes, while section 10 says do not loosen a carry-forward
test. **I took the tighter of the two**, so the band never moved off 214. Reversible in one line
if the owner would rather have the 6 px.

**2. *Make a card anyway* is a note on a row that names nobody, not a working card.** Section 6
ruling 2(a) item 3 says `OpenPsk31CardCommand` already fires on the right-click, that an item
naming what that press did is not a second mechanism, and that I do not build one. **Task 1
measured that the press does not in fact make a card on such a row** - `OpenPsk31Card` returns at
its first statement when `Psk31StationOn` is null, and the card count goes 0 to 0. Given the
instruction forbids building a second mechanism, the line is **present on every row** and, where
Hamlet read no callsign, is a note saying why. Section 4 item 1.

**3. Three names outside this unit were rewritten under R12.** `ARowThatNamesNoStationPutsNothing-`
`UnderTheMouse`, `EveryRowThatNamesAStationOffersTheSevenOnBothModes` and
`NoFt8MessageIsOnAPsk31RowAndWhatItOffersIsTheCannedSeven` all asserted that a row naming nobody
puts nothing under the mouse, which is exactly what R46(b) supersedes. **All three now assert
more**, not less - the menu is exactly two lines and neither carries `SendMessageCommand`.

**4. Three new carry-forward names were added** - `TheFavoritesAreBackOnTheRigDisplayTests`,
`TheMenuIsOnEveryDecodedRowTests` and `TheBlindCarrierShowsNoInventedTextTests` - each in the same
commit as the code it guards, and `TheSettingsSurviveAnUpgradeTests` grew a name rather than a
second type being written.

**5. The app line was run three times at the exit round rather than twice.** Section 1's rule is
one re-run for a lost attempt; two attempts were lost with zero assertion failures, and 10.5 asks
for the list green. **All three attempts are recorded** in section 3 and in `PHASE_OUTCOME.md`,
and the third stands.

## 2. What the owner should expect

**Your stars are back, and they are on the radio face where you had them.** Press the star to save
where the dial is - it takes the frequency, the mode and the block it is in, and names it for you -
and press it again to take it off. Beside it there is now a small caret: press that and your whole
list drops down, with one line for every place you saved and `Manage favorites…` at the foot, and
one click on any line puts the dial there. The Radio menu keeps its own copy of the list; two ways
in is not a defect. If you have saved nothing yet the list says so in a sentence rather than
opening empty. **Your saved frequencies were never lost** - they have been sitting in the settings
file the whole time, and they load, save and load again with the dial and the mode intact,
including ones saved before Hamlet had a notes field.

**And the record now says what took them.** On 2026-08-27 you ruled the `recent · places you have
been` row out of the strip above the tabs. The unit that did it removed the favorites list, the
favorite's name and its note **in the same commit**, was never asked to, and wrote it down
nowhere - not in `ABANDONED_WIDGETS.md`, which records the half you ruled, and not in its own
commit message. Nothing went red, so nobody knew for twenty-six days. The star itself never left:
it has been drawn and clickable at every window size the whole time, which is measured in section
3. **It was not this phase's own step 6.**

**A right-click now gives you something on every line you can see.** It used to give you nothing at
all on a line Hamlet could not read a callsign out of - which are the lines you most want a capture
from. Every decoded row now offers `Capture` and `Make a card anyway`, and `Capture` is the same
button the panel has, not a second one. On the lines that do name a station you get everything you
got before, unchanged, plus those two. **One thing looks like a step backwards and is not**: the
lines that need your own callsign - `Answer him`, `Confirm and 73` - are now drawn grey with the
words *needs your callsign in Settings* instead of vanishing with a note. That is you asking for
them to be disabled and say why, and it is the only place anything is greyed. Nothing you could
send before has become unsendable; that is asserted directly.

**The row on 14.072 has stopped lying to you.** A carrier the blind search finds now shows nothing
at all until Hamlet is reading more of its blocks than it is throwing away - so
`4/500 sending Hk7DYYYzfzYXTDYY...` reads `heard, not readable yet` instead. The row is still
there, in the same place, at the same frequency; only what it says has changed. A station that
announced itself with an RSID is untouched, because it told Hamlet which mode it is.

**The sun map is the one thing you asked for that you do not get, and the reason is arithmetic.**
It is already the tallest thing in the band it sits in: the band is 214 px, the map's column wants
145 of it, and the rest is the band-pill row, the card's own header and padding, and the caption.
Every pixel the map takes, the top band takes - and the top band is capped so the working panels
below it keep their height. Letting the map fill was tried and measured: the map went to 698 x 381
and **the band to 460 px**, more than twice its cap. It was put back the same hour. What did change
is that the map's height is no longer a number copied off a mockup, and a test now fails if the map
ever leaves a pixel of its own row unused - so the day that band has room, the map takes it.

**Every claim above is computed, not seen** (FACT-004). **No port was opened, no device was
enumerated and nothing was keyed.** No send path, modulator, `Arm` site or `PttOn` site was touched
at any task, and no file under `src/Hamlet.RadioEngine/` changed at all.

## 3. What you should see

### 10.1's archaeology - the criterion, and it is a table and not a sentence

| Commit | Short | Unit | Date | File and line | The line, quoted |
|---|---|---|---|---|---|
| `a51bc2a6b...` | `a51bc2a6` | work instruction **029** | **2026-08-27 14:08** | `src/Hamlet.App/Views/MainWindow.axaml`, the `<Grid Grid.Column="1">` block above the tabs | `<ComboBox ItemsSource="{Binding Favorites}"` / `SelectedItem="{Binding SelectedFavorite}"` / `PlaceholderText="places you chose"` / `MinWidth="180" FontSize="12"` / `IsVisible="{Binding HasFavorites}">` |
| same | `a51bc2a6` | 029 | 2026-08-27 | same block | `<TextBlock Text="favorites" FontSize="11" ... IsVisible="{Binding HasFavorites}" />` |
| same | `a51bc2a6` | 029 | 2026-08-27 | same block | `<TextBlock Text="{Binding FavoriteHere}" FontSize="12" TextTrimming="CharacterEllipsis" ... />` |
| same | `a51bc2a6` | 029 | 2026-08-27 | same block | `<TextBox Text="{Binding FavoriteNote, Mode=TwoWay}" IsVisible="{Binding IsFavorite}" Watermark="why this one" ... />` |

The commit's subject: **`fix(app): the header says each thing once, and nothing sits above the
tabs`**, 149 lines removed from `MainWindow.axaml`. What ran, and what it returned:

```
git log --oneline -S "which favorite you landed on at the left" -- src/Hamlet.App/Views/MainWindow.axaml
  -> c949c89c feat(app): put the star in the black, and the name on the strip

git log --oneline -S "THE RECENT-PLACES ROW IS NOT HERE ANY MORE" -- src/Hamlet.App/Views/MainWindow.axaml
  -> a51bc2a6 fix(app): the header says each thing once, and nothing sits above the tabs

git show --stat --format="%h %ad %an" --date=iso a51bc2a6
  -> a51bc2a6  2026-08-27 14:08:11 -0400
     src/Hamlet.App/Views/MainWindow.axaml  | 149 +++-------------

git log --oneline --date=format:"%Y-%m-%d %H:%M" a51bc2a6~12..a51bc2a6
  -> ee34bf39  2026-08-27 13:55  docs(docs): take in work instruction 029
     ... a51bc2a6 is the ninth commit after the intake and before the next
```

**The honest answer is two things happened and only one of them was ruled**, which is what 10.1
asks for rather than a single culprit. Tim's ruling of 2026-08-27 took the
`recent · places you have been · forget this place` row out of the strip above the tabs, and
`ABANDONED_WIDGETS.md` records that removal under *The recent-places row*, with what it did, why it
existed and where it might go. **The favorites half left in the same `<Grid>` in the same commit and
is recorded nowhere**: not in `ABANDONED_WIDGETS.md`, not in the commit message - which names only
the frequency block and the recent row - and not in the comment left behind at
`MainWindow.axaml:3277`, which says the recent row is gone and that `FavoritesViewModel` is still in
the tree, and does not mention that the way to the favorites list went with it.

**And it was not this phase's own step 6 - candidate (2) is false, measured.**

| Where | `_starRect`, measured | Bail fired? |
|---|---|---|
| 1920 x 1040 | `[62,2 67x26]` | no |
| 1400 x 1040 | `[62,2 67x26]` | no |
| 1100 x 780 | `[62,2 67x26]` | no |

The rig face is **520 x 106** at all three. Unit 376's `1c187df6` moved `RigDisplayControl.PadBottom`
from 10 to 6, which is **vertical**; the bail at `DrawStar` tests **horizontal** room
(`clockX - 14 - x`) against a face whose `MeasureOverride` floors the width at 520, so it has never
fired and could not.

**Do the saved frequencies survive?** Yes. Over a 1.13.30-shaped file **this unit's test wrote from
a literal in the test** - the operator's own settings file was never opened, copied or quoted
(HM-DEC-018 §2.1) - today's loader returned **2 of 2** favorites with frequency, mode, band, name
and note; saved by today's writer and loaded again, **2 of 2**, frequency and mode intact.

### The star and the caret, after

| Size | Face | Star | Caret | Overlap |
|---|---|---|---|---|
| 1920 x 1040 | 520 x 106 | `[62,2 67x26]` | `[130,2 18x26]` | none |
| 900 x 620 | 520 x 106 | `[62,2 67x26]` | `[130,2 18x26]` | none |
| 1100 x 780 | 520 x 106 | `[62,2 67x26]` | `[130,2 18x26]` | none |
| 1280 x 720 | 520 x 106 | `[62,2 67x26]` | `[130,2 18x26]` | none |
| 1366 x 728 | 520 x 106 | `[62,2 67x26]` | `[130,2 18x26]` | none |
| 1536 x 824 | 520 x 106 | `[62,2 67x26]` | `[130,2 18x26]` | none |
| 1400 x 1040 | 520 x 106 | `[62,2 67x26]` | `[130,2 18x26]` | none |
| 1920 x 1017 | 520 x 106 | `[62,2 67x26]` | `[130,2 18x26]` | none |
| 2560 x 1400 | 520 x 106 | `[62,2 67x26]` | `[130,2 18x26]` | none |

**All nine of unit 354's sizes**, with every press made by a headless pointer at the coordinates the
control itself drew. One press on the star saved `14,074,000 Hz`, mode and band with the name
`14.074, FT8 city`; a second press removed it. The caret's list read `14.074, FT8 city`,
`14.076, FT8 city`, `Manage favorites…`, and **one click on the first line moved a dial parked 20 kHz
away back to 14,074,000 Hz**. With nothing saved the list reads *Nothing saved here yet - press the
star to save where you are.*, hittable `False`, with `Manage favorites…` under it. **The top band is
214 px with the pills and 178 without, at 1920 and at 1400 - unmoved.**

### The menu counts, before and after

| | Before | After |
|---|---|---|
| rows offering a menu | **3 of 9** | **9 of 9** |
| rows offering nothing at all | 6 of 9 | **0 of 9** |
| items on a row that names a station | 7 | 9 (the seven, then `Capture`, then `Make a card anyway`) |
| items on a row that names nobody | 0 | 2 |
| lines drawn grey with a full profile | 0 | **0** |
| live send lines across the fixture | 15 | **15** |

The six that offered nothing were a carrier heard with nothing read off it, a fragment with no
callsign, a second fragment, an ended exchange, and the two blind-found Olivia rows. `SendFlyoutFor`
returned null where `Psk31CannedMenuFor` was null **and** `SendMenuFor` was null, and
`Psk31CannedMenuFor` answered null wherever `Psk31StationOn(row)` did. With no callsign in Settings
the menu now reads: `GREY Answer him - needs your callsign in Settings`, `note Send my report - not
offered: your name, location or grid square is not set…`, `GREY Confirm and 73 - needs your callsign
in Settings`, then `Say again?`, `Please repeat your report`, `QRZ?`, `73 and out` all live.

### The map and the band at both widths, against 220

| Width | Mode | Band with pills | Band without | Map | Caption | Map's share of the band | Against 220 |
|---|---|---|---|---|---|---|---|
| 1920 | FT8 / PSK31 / Olivia | **214** | 178 | **246 x 134** | 9 | **62.6%** | inside by 6 |
| 1400 | FT8 / PSK31 / Olivia | **214** | 178 | **246 x 134** | 9 | **62.6%** | inside by 6 |

**Why it cannot be more, measured rather than argued.** `GreenZoneLeft` wants **25 px** at 1920 and
**54 px** at 1400; the map's own column wants **145** - 134 and 2 px of gap and the 9 px caption - in
a card row of **146**; the card wants **178** and the rig column wants **178**, *tied with margin 0*.
So the map governs, and every pixel it takes the card takes and the band takes. The band is ratcheted
at **214** by a carry-forward name. **The attempt was made:** with the map taking the height available
to it, the map measured **698 x 381**, the card **424** and **the band 460 px** - 240 over 6.1's
ceiling and 246 over the ratchet. It was reverted in the same task. The 80 px the map does not have
are the pills row and its margins (36), the card's own header and padding (32), and the caption and
its gap (11).

The panel row at unit 354's nine sizes after all of tonight's work: **483, 71, 92, 163, 171, 267,
483, 460, 860** - identical to unit 381's, to the pixel.

### 10.4 - the 13:37 UTC row, replayed

| Blocks read | Blocks refused | Found by | The row reads |
|---|---|---|---|
| 1 | 0 | blind | `heard, not readable yet` |
| **1** | **11** | **blind** | **`heard, not readable yet`** - Tim's own row |
| 2 | 3 | blind | `heard, not readable yet` |
| 2 | 2 | blind | what the channel read |
| 2 | 0 | blind | what the channel read |
| 12 | 11 | blind | what the channel read |
| 1 | 11 | **RSID** | what the channel read - not gated at all |

The 4/500 row at 1500 Hz carries **not one** of the characters nobody sent, reads exactly
`heard, not readable yet`, and keeps its variant and its place. Every case asserts the row is
**either** the sentence **or** what the channel read and never something between. The gate is both of
the engine's own numbers: at least `OliviaBlindSearch.ConfirmBlocks` blocks read - whose own remark
says *one block barely through is what a wrong row can do* - **and more blocks read than refused**,
which is the half nothing consulted: `BlocksRejected` has crossed the seam since unit 364 and the row
path asked only `BlocksDecoded > 0`, so **one accepted block in twelve put its characters on the
screen**. It is one-way: once confident, a fade never takes the words back. **No engine file changed
at all**, and PSK31 needed nothing - its squelch already does this and `Psk31Listener` says so.

### The entry and the exit rounds, name for name

| | Entry (task 0) | Exit (task 6) | Difference |
|---|---|---|---|
| app | **245 of 245**, 2 m 33 s, green first attempt | **265 of 265**, 2 m 42 s, green on attempt 3 | +20, this unit's own new names |
| engine | **150 of 150**, 4 m 52 s, green first attempt | **150 of 150**, 4 m 55 s, green first attempt | none |

The twenty: `TheFavoritesAreBackOnTheRigDisplayTests` **5**, `TheMenuIsOnEveryDecodedRowTests` **4**,
`TheBlindCarrierShowsNoInventedTextTests` **10**, and `TheSettingsSurviveAnUpgradeTests` **+1**.
**No regression and nothing red that was green before** (HM-DEC-165).

**All three exit app attempts, recorded.** Attempt 1: **263 of 265** - one **red on an assertion**,
`ThePsk31ReadsTheConversationTests.NoFt8MessageIsOnAPsk31RowAndWhatItOffersIsTheCannedSeven`, a
carry-forward name and **this unit's own**, repaired under R12 and committed; plus one name lost to
the dispatcher loop. Attempt 2, after the repair: **264 of 265**, one name lost, **no assertion
failure**. Attempt 3: **265 of 265 clean**. Five occurrences of
`InvalidProgramException: You've caused dispatcher loop` across the night's five app invocations,
**every one at about 1 ms before any assertion**, on `ThePsk31OfferTests.TheOfferIsOneButtonAndIt-`
`IsTheOneTheEngineNamed`, `TheRecordNamesTheSubModePressedTests.TheCqPressWritesTheLabelThe-`
`OperatorPressed` at Olivia twice and FT4 once, and
`TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow`. Recorded, not
chased. **The engine invocation was green on the first attempt both times and has never once met the
fault.**

## 4. What's blocking us

**Nothing blocks a criterion in B.** Seven items. Items 1 and 2 qualify 10.2 and 10.3 and are already
stated in B.

**1. *Make a card anyway* is a note on a row that names nobody, because a working card there is the
second mechanism the instruction forbids.** *A finding that wants a ruling if the owner disagrees.*
Section 6 ruling 2(a) item 3 states that `OpenPsk31CardCommand` already fires on the right-click and
that an item naming what that press did *is not a second mechanism, and you do not build one*.
**Measured at task 1: the press does not make a card on such a row.** `OpenPsk31Card`'s first
statement is a return where `Psk31StationOn(row)` is null (`MainWindowViewModel.cs:17931`), and the
card count goes **0 to 0**. A card is keyed by callsign throughout - `_psk31Cards`, `RefreshPsk31Card`,
`DigitalCards.FirstOrDefault(c => IsSameStation(c.Callsign, …))` - so making one for a nameless row is
a new mechanism, not a wiring job. The line is therefore **present on every row** and, where Hamlet
read no callsign, reads *Make a card anyway - Hamlet read no callsign on this row, so a card would
have nobody on it. Capture the audio and the card follows when a call comes through.* **10.2's words
are met** - Capture and the card line are always present - **and R46(b)'s intent is met in part.** If
Tim wants a card keyed by the row's place rather than a callsign, that is a unit's worth of work and
it is his call.

**2. The band's ceiling for 10.3 is 214 in the tree and 220 in the instruction, and I took 214.** *A
mismatch, reported as section 5 directs.* Section 6 ruling 2(b) and section 8 task 3 both name 6.1's
**220 px**; `Unit376TheTopBandTests.BandReachedWithThePills` is **214** and the carry-forward name
`TheTopBandIsOneShortRowAndThePanelsAreTallerByTheDifference` asserts it, while section 8 task 3 says
that name must be green at the end and section 10 says never loosen a carry-forward test. **Taking the
6 px would have turned that name red**, so I did not. It would have bought the map 134 to 140 px -
62.6% to 65.4% of the band - and it is reversible in one line if the owner would rather have it.

**3. At 1100 x 780 - the size Hamlet opens at - there IS height beside the map, and spending it costs
width.** *A finding for the owner, logged and not worked.* At that size the card wants **353 px**
against the rig column's 178, so the green-zone text governs and roughly **176 px** beside the map go
unused. The map could take them - but its width follows the picture's own proportions (HM-DEC-092), so
a 321 px map is a **590 px** map on a 1036 px card: it would take the width the text needs, the text
would wrap taller, and the map would grow again. **That is a layout trade about a screen and it is
Tim's**, not an arbiter's.

**4. Eight names in `TheMenuIsUnderTheMouseTests` fail on a scene precondition, and I could not prove
whether they were red before tonight.** *A finding, reported honestly rather than called
environmental.* The failure is `expected both decoded lists in the window, found DigitalDecodedRows` -
`RightClickRow` needs both `DigitalDecodedRows` and `DigitalMineRows` realized and finds one - and it
fires **before any menu assertion**, including in the one name this unit rewrote. **The type is not on
the carry-forward list, so it cannot fail a round.** What I can state: nothing this unit changed
touches either list, their visibility, the panel layout or the parse that classifies a row as the
operator's; the panel row at unit 354's nine sizes is identical to unit 381's to the pixel; and
`BindingHealthTests`, `TheTopRowTests`, `TheWorkingPanelsTests`, `TheStopIsAlwaysOnScreenTests`,
`TheDecodedColumnsLineUpTests` and `ClippingTests` are all green. **What I could not do is run the
pre-unit tree to prove inheritance**: `git stash push`, `git worktree add` and
`git checkout <ref> -- <paths>` were each refused by the permission mode. So it is recorded as
*not proved inherited, strongly evidenced as inherited*, and not claimed either way.

**5. The 10.4 gate writes no telemetry line of its own.** *A finding, and R14 is why.* R13 asks for
telemetry on every stage, and a gate that withholds text from the screen is arguably one. I added **no
new writer**, so `CallsignPrivacyTests` is unmoved and green at 4 of 4 and its walk did not have to
grow. The two numbers the gate reads - `blocksDecoded` and `blocksRejected` - are already written per
channel by the listener's own events, so the decision is reconstructible from the record; **the
decision itself is not in the file.** A later unit that wants it can add a field to an existing writer
rather than a second writer.

**6. Unit 386's items 5 and 6 are the owner's and were not re-recorded**, as section 3 directs. The
launcher fault twice over, and 0.1's cut-down. **Neither is mine. Neither was touched.**

**7. The `RULES_AT` id split, for the tenth unit running.** *Reported, not repaired.*
`PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)` because `tools/status.sh` writes that field as a
literal; `CLAUDE.md` §1 holds `CPS-DEC-0165`. **`tools\` is not this unit's to edit**, and section 9
parks it.

### On the carried items that tonight's work touched

- **Unit 386's item 3 - a stale expected test count beat an instruction and the measurement won.**
  It did not recur. Section 5's expected **245 and 150** were exactly what the entry round returned.
- **Unit 383's inherited reds.** `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn` and
  `TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend` were not gone hunting for and neither file was
  opened. `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns` **was run**, because
  task 3's trap sits next to it: it scanned **51** view test files, two of them this unit's new ones,
  and named only the one pre-existing offender, `TheStopIsAlwaysOnScreenTests.cs:102`. **This unit made
  it no worse and did not make it green**; both of this unit's view tests act through the control with
  a headless pointer and read the private rectangles without writing anything.
- **`docs/RADIO_SHEET.md` (section 9, parked).** Checked: none of the strings this unit changed or
  added is quoted by the sheet. `TheRadioSheetQuotesTheScreenTests` is on the carry-forward line and
  was green at the exit round.

**`validate-output.bat` - hand-checked, not run, an eighth unit running.** The exact command, in the
shape section 2 prescribes:

```
./tools/arbiter/validate-output.bat output.md
```

The exact refusal: `This command requires approval`. **It is the permission mode and not the syntax**,
and a non-interactive session cannot answer the prompt. **What follows is a hand-check against the
script's own header rules as units 385 and 386 transcribed them, and not a run of the validator.**

| Rule, from the script's own header | Hand-check |
|---|---|
| 1 - a `UNIT:` line above section 1, parseable | **ok** - inside the fenced block above section 1 and within the leading 60 lines |
| 2 - the four top-level sections, in order, exact names | **ok** - `## 1. What Claude did`, `## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us` |
| 3 - no fifth top-level section | **ok** - exactly four `## ` lines; every deeper heading is `### `, which `^## ` does not match |
| 4 - section 4 present even when empty | **ok** - present, not empty, straight apostrophe in the heading |
| 5 - section 3 non-empty | **ok** - the archaeology table, the star's nine sizes, the menu counts, the map and band numbers, 10.4's seven cases and both rounds |
| 6 - the ordering block above the `UNIT:` line, A, B, C, and C naming a count | **ok** - `READ IN THIS ORDER.` on line 1, `A.` line 3, `B.` line 9, `C.` line 25, the `UNIT:` line at 34 and section 1 at 50; and C says *raises 7 items*, which `raises \d+ item` matches |

### Asks still outstanding - carried per HM-DEC-139, verbatim

**The queue stands at fifty-three and this unit answers none of them.** It is the forty-seven unit 386
carried plus unit 386's own six, read out of unit 386's committed `output.md` at `283c6fa6` rather
than reconstructed from memory.

**Unit 386's own six, verbatim:**

**1. The two counts differ by two rounds, and that difference is the night's finding.** *A
finding, and ruling 1 asks for it by name.* **Two invocations of the soak's twelve were lost** -
round 1 app attempt 1, three occurrences at 1 ms on
`TheTestsStayOffTheNetworkTests.ThePlainFixtureTakesGeneralFromTheFixedAnswer`,
`TheTestsStayOffTheNetworkTests.TheLicensedFixtureTakesTheFixedAnswerToo` and
`ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel`; and round 5 app
attempt 1, one occurrence at 1 ms on
`ThePsk31ConversationCardTests.NoSlotClockUnderPsk31AndFt8AndFt4StillShowIt`. Both landed in
`HeadlessUnitTestSession.EnsureApplication` before any assertion, both were recovered on the
single allowed re-run, and neither is a red. **Counting round 0 and the exit round, four of ten
app attempts were lost and zero were red on an assertion.** What that means in one line: **the
carry-forward list did not fail once tonight; the Avalonia headless harness failed four times**,
and the gap between 3 and 5 is entirely the harness.

**2. No name went red on an assertion, so task 4 was not entered.** *A finding, and the reason
this unit reports complete at task 5 of 6 rather than at 6 of 6.* Task 4 is conditional by its
own terms. `TheStopIsAlwaysOnScreenTests.WithNothingKeyedItSaysStopAndIsStillPressable`, the one
name section 5 flags as not quarantined and not settled, **was green in all ten app invocations
tonight**. **Nothing was quarantined, nothing came off the command line, and nothing was added to
the list.**

**3. Section 5's expected green app count was stale and my measurement won.** *A finding, and a
mismatch reported as section 5 directs.* Section 5 says a green app invocation is **226 of 226**
and calls it unit 385's exit run. The measured total on all ten app invocations tonight is
**245**. **226 is unit 385's entry number carried into the instruction as its exit**, from before
its four new types joined the app line. The number a green round must match is 245 of 245.

**4. The identical-cost fingerprint appears twice more in the Olivia file, and both pairs were
left exactly as they stand.** *A finding, asking for nothing, and explicitly not a widening of
2.6.* Line 102 in `## UNIT 1 - STEP 1` and line 117 in `## UNIT 2 - STEP 2` both read
`COST: 11.789573999999998`, and line 183 in `## UNIT 2 - STEP 3` and line 210 in
`## UNIT 3 - STEP 3` both read `COST: 17.930809499999995`. **Whether those are the same launcher
fault or a judging session legitimately grading twice off one run was not determined and is not
claimed either way.**

**5. The launcher fault has now happened twice, in two phases, and both false rows are
unedited.** *A finding, logged to the owner.* The Olivia phase's `## UNIT 2 - STEP 1` of
2026-09-14, corrected by unit 386; and this phase's own `## UNIT 7 - STEP 2`, which unit 385
corrected on 2026-09-21. **Neither row was edited and neither was re-recorded.** Twice is a
pattern rather than an accident, and the remedy - a launcher that does not write `FATE: executed`
for a session that never ran, and a grading pass that does not score a stale `output.md` - is
outside any unit's reach.

**6. 0.1 was cut down by instruction 386's section 6 ruling 1 and no search was re-run.** *A
finding, and the rewording is the owner's.* Across those 119 commits **zero** touch any settings
path and **zero** touch any path in the repository whose name contains `settings`, so the
criterion presupposes a commit that does not exist. **0.1 stays unticked.** The remedy is to
reword 0.1 so a completed negative satisfies it, which is a change to the plan and therefore
Tim's.

**Unit 385's own eight, verbatim, as unit 386 carried them:**

**1. The measurement disagreed with holding on the carrier alone, and the measurement won.** *A
finding.* `HeIsSending` was never true while a carrier was up, but a row outlives the man: **9 s**
on PSK31 and **22.94 s to 38.23 s** on Olivia. A hold on `row.Ended` alone refused the operator's
answer for that long after the other man said `K`, and it turned 25 names red. The hold is now
*carrier up **and** nothing handed back*.

**2. The second hand-back Tim watched is not a parsed line at all, and its site is in the
engine.** *A finding that wants a licence, not a ruling.* `Psk31MessageSplitter` completes a
message on a turnover word that follows **clean** callsigns, so a garbled over completes nothing.
Measured: **0 messages from the garbled over against 1 from the same words with clean calls.**
**Nothing was repaired.**

**3. 9.1's token is `card_cleared`, not `card_dismissed`.** *A finding.* `ClearCardCommand` has
written `card_cleared` since unit 305 and R13 forbids a second writer where one exists. **The
token was left as it is.**

**4. Section 5 is out of date about what a guessed turn offers.** *A mismatch, reported.* Since
unit 371 a guessed hand-back to the operator offers the **Report** and never the **Confirm**.

**5. The owner's telemetry for 2026-09-21 is not on this machine.** *A finding about evidence.*
Every fixture was **constructed to R44's stated timings** with a test callsign.

**6. The `## UNIT 7 - STEP 2` row was appended to, never edited.** *As instructed.*

**7. One commit carries two pieces.** *A finding about unit 385's shape.* 9.1's `NoteCardOnScreen`
line went in with the hold's narrowing at `d86d4ccd`.

**8. The `RULES_AT` id split.** *Reported, not repaired.* `tools\` is not a unit's.

**Unit 383's ten, carried by units 385 and 386, verbatim:**

**1. 4.3 is met on the measurement, and partial under the strictest reading.** 183 lines scanned;
**0** tier-1 phrases in the sheet's own voice; **2** inside quotes, both declared and proved
character for character; **9** tier-2 mentions. **The remedy would be a change to a sentence
Hamlet says**, and that is the owner's. **Nothing is asked for here.**

**2. No quoted tier-1 hit failed its proof**, so the item that would have been first does not
exist. Both declared sentences are produced by the shipped view model, character for character.

**3. `_psk31Canned` has exactly one consumer and it is the token expression** - four mentions,
one read, at `MainWindowViewModel.cs:16727`. A finding, not a ruling request.

**4. A third name has joined the environmental family.**
`TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend` went red on both attempts of unit 383's exit
named-type run with two different failures, and is **green on its own in 273 ms**. **It waits on
wall time under parallel load.** Recorded, not chased, and not quarantined.

**5. The two inherited reds, reported and repaired neither**:
`ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns` - *TheStopIsAlwaysOn-
ScreenTests.cs:102 writes OperatingMode, which the CW / Digital / Voice tab strip owns - press it
instead* - and `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`.

**6. One measurement refined the instruction and the measurement won.** `:19154` is the SLOTTED
path and not reachable in PSK31 or Olivia; `:18913` is the unslotted cancelled-run line and **is
not on the sheet**; `:19587` is the stop press's own and is what the sheet quotes. **A finding.**

**7. The sheet moved further over its target and the number is reported.** 183 lines and 11,585
bytes against 120 and 10,240, from unit 382's 176 and 10,918. Nothing was dropped to pay for it.

**8. Two decisions about shape that the owner should see.** Two carry-forward names added that
the instruction did not ask for, and two commits merged that section 11 would have split.
**Findings, and both are reversible in one line.**

**9. The `RULES_AT` id-scheme split is reported and not repaired.**

**10. `validate-output.bat` refused again** and the six rules were hand-checked instead.

**And the queue of twenty-nine that units 385 and 386 carried by reference, unanswered here:**

Unit 382's item 5; unit 381's item 1; unit 380's items 1 and 4; unit 379's items 1, 3 and 7; unit
378's items 1, 3 and 4; unit 377's item 4; unit 376's items 3, 4 and 5; unit 375's items 3 and 4;
unit 374's item 3; unit 373's item 2; unit 372's items 4 and 7; unit 371's five; unit 369's four.
