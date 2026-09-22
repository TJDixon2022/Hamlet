READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Steps 1, 2, 3, 6 and 8 done; step 0
   partial with 0.1 cut down and closed to units; step 4 partial with 4.3 logged to
   the owner; step 7 partial on an unjudged 7.2; step 9 landed and ungraded; step 5
   Tim's own and it ends the run. Step 10 partial after unit 387 - 10.1, 10.2, 10.4,
   10.5 met - and rev7 reworded 10.3 and added 10.6 this afternoon.
B. Step 10 - tonight's two. 10.6 met: one drop-down reading Favorites, GreenZoneFavorites,
   in the neighborhood card directly under the green block, 2 favorites in the list
   in the test, one click measured tuning from 14,094,000 Hz to 14,074,000 Hz at 1920
   and 1400; the caret gone; the band 214 px at 1920 and 1400. 10.3 partial: the map
   327 x 178 where it was 246 x 134, in a band of 214 px; stage A held, stage B
   reverted, because at 1400 the pills had 343 of the 675 px they need beside the map,
   wrapped to two rows, and the band went to 252.
C. The report last. Section 4 raises 7 items on top of the carried sixty, and
   item 1 is in the way of a criterion in B - it is 10.3's last 36 px, and it is a
   decision about Tim's pills. What the pills row did when the map stood beside it:
   at 1920 it stayed one row and the map took the band's whole 214; at 1400 it wrapped
   to a second row and pushed the band to 252, so it was put back.

```
UNIT:       388 - complete at task 4 of 5 - 2026-09-22 14:06
PHASE GOAL: Hamlet keeps what it has already earned - the screen, record and test
            work banked in the PSK31 and Olivia threads - proved by tests that ran
            without the radio or the owner, and at the end by Tim at his window.
UNIT GOAL:  Do the two things Tim marked on his own screen this afternoon: let the
            top band's height set the sun map's height rather than the other way
            round, taking the map out from under the card's chrome and its width from
            the neighborhood side; and put favorites back as the one drop-down under
            the green zone he had before 2026-08-27, with the rig face's caret gone.
ADVANCED:   step 10, criteria 10.6 met and ticked; 10.3 moved from 134 to 178 px of
            its band and left unticked, partial at stage A
NUMBER:     the sun map's share of its band: 62.6% -> 83.2%; favorites reached from
            the neighborhood card: 0 -> 2 presses to tune
DRIFT:      none
```

## 1. What Claude did

**Complete, at task 4 of 5.** I entered all five tasks (0 to 4). I tried the named drop candidate, stage B, measured it, and reverted it in the same task as ruling 1 item 3 directs. Windows 11, `C:\Source\HamLet`, branch `main`, project Hamlet. The gate's four conditions all hold. I committed and pushed each task before starting the next: the prompt said to push each task, while the instruction said push once at the end, and I followed the prompt.

**Task 0 - the record and the entry round.** Version 1.13.74 to 1.13.75 with its line in the version log. `PHASE_STATUS.md` `CURRENT_STEP` 0 to 10 and `WORK_INSTRUCTION` 387 to 388, both stale as section 5 said. `PHASE_PLAN.md` ticked nothing at this task. Line 7 carries **62** filter terms and line 9 **25**, both beginning `timeout 480 dotnet test`, read out of the file and evaluated, never retyped. **`TheTopRowTests` is not on line 7 (0 occurrences)**, which confirms section 5's mismatch against 10.5's ticked text. Entry round:
- **App attempt 1: 263 of 265.** One name was lost to the dispatcher loop at 1 ms, and `TheOliviaRowsTests.WithRowsPresentNothingIsComposedUntilAPressAndEachPressCarriesItsRowsVariant` went red at 7 s.
- **App attempt 2: 259 of 265.** Five names were lost at 1 ms, and the same name went red at 8 s on `IOException ... being used by another process`. That is a file lock on its own temp `refuse` jsonl, not an assertion.
- **Engine: 150 of 150** on the first attempt.
- **That type alone: 7 of 7.** So it is a lock under the full invocation's parallel load. Nothing under `src` or `tests` had moved since unit 387's exit, so it is **inherited**. It is named and recorded, not chased.
- **`TheMenuIsUnderTheMouseTests`: 0 of 8**, all on unit 387's scene precondition.

**Task 1 - the trace, and no file under `src` moved.** `Unit388TraceTests` asserts nothing. It measured the band's parts at three sizes, the width budget at 1400 and the pills' appetite, and it recorded the empty row under the green zone. I read the old favorites control with `git show a51bc2a6^:...`. The arithmetic said, before anything was built, that stage B in today's shape needs the pills to fit in **401 px against their 675** at 1400. Section 3 has every number.

**Task 2 - 10.6.** A new `FavoritesDropDownControl`, a `DropDownButton` reading *Favorites*, sits under `GreenZoneBlock` bound to `FavoriteMenu` and `ManageFavoritesCommand`. It builds its list at the press, the way the rig face's caret did, and reuses its note word for word. I took the caret, `_listRect`, the two properties, `OpenTheSavedList` and `SavedListUnderThePointer` off `RigDisplayControl`. That file now differs from `9968c08c^`, the commit before the caret, **by six comment lines and nothing else**, so the star's drawing, its bail and `_starRect` are exactly as they were. I rewrote the `MainWindow.axaml` comment at the rig face. There is no new event writer.

**Task 3 - 10.3.**
- **Stage A held and was committed on its own.** The map now stands in its own column in `TopRow`, between the card and the rig face, inside a new `BandGovernsTheMapPanel`. That panel asks in a fixed order: the rig face says how tall the row is, the map is made that tall, and the card takes the width that is left.
- **Stage B was tried, measured and reverted.** The map ran up beside the pills too. It held at 1920 and failed at 1400 on two rows of pills and a 252 px band. I reverted it by hand because `git checkout` is refused, and `git diff` over `src` against the stage A commit then read **empty**.

**Task 4 - the exit round, the plan, the status and this report.**
- **App attempt 1** ran 214 names: **213 passed, one red on an assertion that was this unit's own**, and then **the test host crashed**. The red was `TheFavoritesAreUnderTheGreenZoneTests.WithNothingSavedTheListSaysSoRatherThanOpeningEmpty`: its list was null, so the press never reached the button. I repaired it under R12.
- **App attempt 2: 264 of 265. App attempt 3: 263 of 265.** Every loss was the dispatcher loop at 1 ms. There was no assertion failure in either.
- **Engine: 150 of 150** on the first attempt.
- **Plan:** 10.6 ticked with tonight's numbers. 10.3 is unticked, with a unit 388 note in its text. Nothing of steps 0, 4, 7 or 9 was touched. Step 10's state word stays `partial`.

### The decisions this unit made for itself, reproduced in full

**1. The card floor is 400 px, and it is my number.** A panel that only checked the card's height was fooled at 1100 x 780. The card still fitted the row's height by giving `GreenZoneLeft` (the band, the frequency and the verdict) **0 px of width** and the caption 0. That is hiding information. So the map grows above its 246 x 134 floor only while the card keeps at least 400 px. At 1400 stage A leaves the card 467, and at 1100 x 780 the map stays at its floor with the card laid out exactly as before. The constant is `BandGovernsTheMapPanel.CardFloor` and it is overrulable.

**2. The caption sits at the right end of the card's Favorites row, beside the map.** Ruling 1 item 4 leaves where it goes to me. Under the map it cost 11 px of the band; in that row it costs nothing, because the row is already paid for. Its words are unchanged. At 1100 x 780 it wraps to three lines, 80 x 36, inside the card's scroller.

**3. The map's height comes from the rig face, never from the card.** When the card governs, as at 1100 x 780, a map that followed the row would run away: it would get taller, then wider, the card narrower, the card taller again. The rig face's height does not depend on its width, so it is the stable source.

**4. Two tests outside the ruled one were rewritten under R12, because rev7 replaced what they asserted.**
- `TheTopRowTests.TheWorldClockIsAtTheCardsRightEndWithOneMarker` went red on *the world clock is not in the card*. It now asserts the clock stands between the card and the rig face, clear of the green block, as tall as the card, with one marker at his grid.
- `TheGreenZoneTests.NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth` went red on 178 against 134. It now asserts at least 134 and as tall as the card.

Neither is on the carry-forward line.

**5. The renamed type keeps its old file path.** Its name was false, so I renamed it `TheFavoritesAreUnderTheGreenZoneTests`, changing line 7's term in the same commit (62 terms before and after). `git mv` was refused and so is `rm`, so the file is still `TheFavoritesAreBackOnTheRigDisplayTests.cs`. The filter matches the type, not the file.

**6. The exit app line ran three times, and I stopped there.** Attempt 1 found my own red and a host crash. Attempt 2 came after the repair, and attempt 3 is the one allowed re-run of a lost attempt. Name for name, every one of the 265 passed in attempt 2 or attempt 3, and each lost name passed in the other one.

## 2. What the owner should expect

**Your favorites are back where you had them: one drop-down reading *Favorites*, in the neighborhood card, right under the green block.** Press it and your saved places drop down, one line each, with `Manage favorites…` at the foot. One click on a line tunes there, and the control goes back to reading *Favorites* rather than holding on to a place the dial has since left. It is the same list the Radio menu shows, so a favorite can't be one thing in one place and another in the other. If you haven't saved anything yet it still opens, and says so. The star on the radio face still saves where the dial is, and pressing it again un-saves. The little caret unit 387 put beside the star is gone.

**Your sun map is out from under the card's header and caption and now stands as tall as the card and the radio beside it: 327 x 178 where it was 246 x 134**, same picture and same proportions, with your dot on your grid. It sits in its own spot between the neighborhood card and the radio. The width came out of the neighborhood side, which is now 467 px wide at 1400 (it was 808). It took none from the radio. The top band did not grow by a pixel: 214 at 1920 and at 1400.

**It stops 36 px short of the full 214, and here is exactly why.** Those 36 px are the band-pill row above the card. To run the map up beside the pills, the pills must fit in the room left of the map. At 1920 they do, and the map reached the full 214 when I tried it. At 1400 they had 343 px of the 675 they need, wrapped to a second row, and pushed the whole band to 252. So I put it back. Getting the last 36 px at 1400 means putting the pills somewhere other than across the top of the card, which is your call and not mine.

**Two things will look different and are not wrong.** The caption *where the sun is · you* now sits at the right end of the Favorites row, next to the map, instead of under it. At the size Hamlet opens at, 1100 x 780, the map stays at its old 246 x 134: growing it there would squeeze the green block's words to nothing. The card at that size is a little taller inside its own scroller, and the working panels are exactly the height they were.

**Every claim above is computed, not seen** (FACT-004). **No port was opened, nothing was enumerated and nothing was keyed.** No send path, modulator, `Arm` site or `PttOn` site was touched, and no file under `src/Hamlet.RadioEngine/` changed.

## 3. What you should see

### The band, before and after

| Size | | Pills row (margins) | Card header + padding | Map | Caption | With pills | Without pills |
|---|---|---|---|---|---|---|---|
| 1920 x 1040 | before | 30 (10 above, 6 below) | 27 + 5 | **246 x 134**, inside the card | 246 x 9 under the map, 2 px gap | **214** | 178 |
| 1920 x 1040 | after | 30 (10, 6) | 27 + 5, beside the map | **327 x 178**, y 119 to 297 | 220 x 9 in the Favorites row | **214** | 178 |
| 1400 x 1040 | before | 30 (10, 6) | 27 + 5 | **246 x 134**, inside the card | 246 x 9 under the map | **214** | 178 |
| 1400 x 1040 | after | 30 (10, 6) | 27 + 5, beside the map | **327 x 178**, y 119 to 297 | 220 x 9 in the Favorites row | **214** | 178 |
| 1100 x 780 | before | 30 (10, 6) | 27 + 5 | 246 x 134 | 246 x 9 | 389 | 353 |
| 1100 x 780 | after | 30 (10, 6) | 27 + 5 | 246 x 134 (held at its floor) | 80 x 36, wrapped | 429 | 393 |

**The band's own outer margins, by name.** The band runs from the top of the pills to the bottom of the card and rig row. The pills row's 10 px top margin sits outside it and its 6 px bottom margin sits inside it. The map's 178 is the card-and-rig row top to bottom, **0 px short**, so the 36 px it lacks of 214 are the pills row (30) and the pills' 6 px bottom margin. At stage A the map's share is 178 of 214, **83.2%**, where it was 62.6%. Its aspect is 1.8371 against the picture's 1.8358.

**At 1100 x 780, beside unit 387's item 3.**
- **Unit 387 found:** the card wanting 353 against the rig's 178, and about 176 px of height beside the map unused.
- **Tonight:** the map stays at its 246 x 134 floor, because growing it would take the card under 400 px. The card is 248 wide with its left stack at 218, as before. It is 393 tall: 353, plus the Favorites row (18), plus the caption wrapping beside it (22). All of that is inside `TopRow`'s 300 px cap, so the panel row there is 92 as before.

Not ruled on.

### The width budget at 1400

| | Before | After stage A |
|---|---|---|
| card | 808 | **467** |
| its left stack (strip and green block) | 518 | 437 |
| `GreenZoneLeft` | 340 of 346 wanted, 54 tall | 259 of 275 wanted, 64 tall |
| `GreenZoneRight` | 140 | 140 |
| green block | 68 tall | 78 tall |
| the empty row under the green zone | 42 px | 32 px, the Favorites row in it |
| map | 246 x 134, with 14 px column spacing inside the card | 327 x 178, its own column, 14 px gap |
| rig column | 546 | 546 |

A 214-tall map at its own proportions needs **393 px**, 147 more than before. Task 1's arithmetic said stage A leaves the green block about 415 px of content and fits under 178 with room for a short Favorites row. Measured, the card's content is 132 of its 146.

**The pills.** There are seven, 675 px with their 8 px spacing, in a horizontal `StackPanel` that cannot wrap. The row is 1888 px wide at 1920 and 1368 at 1400. Beside the map (stage B) the pills had **935 px at 1920: one row, held** and **343 at 1400: two rows, band 252**. With the map between the card and the rig, the full 214 at 1400 needs about 1628 px of row for the pills, a window of about 1660.

### The favorites control, then and now

Before `a51bc2a6` (2026-08-27), read with `git show a51bc2a6^:src/Hamlet.App/Views/MainWindow.axaml`. It sat in `TopRow`'s second row, to the right of the old green privilege block, after the recent box:

```
<TextBlock Text="favorites" FontSize="11"
           VerticalAlignment="Center" Margin="6,0,0,0"
           IsVisible="{Binding HasFavorites}"
           Foreground="{StaticResource HmTextMutedBrush}" />

<ComboBox ItemsSource="{Binding Favorites}"
          SelectedItem="{Binding SelectedFavorite}"
          PlaceholderText="places you chose"
          MinWidth="180" FontSize="12"
          IsVisible="{Binding HasFavorites}">
```

Selecting cleared the selection and called `TuneToFavorite`, which writes `FavoriteTuned` and tunes, so the box read its name again after a pick. It was absent when nothing was saved.

Shipped tonight, in the card under `GreenZoneBlock`:

```
<ctl:FavoritesDropDownControl x:Name="GreenZoneFavorites"
                              Grid.Column="0"
                              HorizontalAlignment="Left"
                              FontSize="11" Padding="8,1" MinHeight="0"
                              Favorites="{Binding FavoriteMenu}"
                              ManageFavoritesCommand="{Binding ManageFavoritesCommand}" />
```

What is the same: a drop-down beside the green block, one pick tunes, and it reads its name again afterwards. What is different, on purpose:
- its list is `FavoriteMenu`, the Radio menu's own;
- it is present and opens to a note when empty, rather than vanishing (§0.5.1, the 2026-09-06 rule).

**Measured.**
- **Where it sits:** at `[31,241 130x14]` at 1920 and `[31,254 130x14]` at 1400 before stage A, and `[31,264 130x14]` at 1400 after it. The headless font draws 11 px text 9 px tall, and its word gets its full 90 x 9, so it is not clipped.
- **What it lists:** `14.074, FT8 city`, `14.076, FT8 city`, `Manage favorites…`.
- **One click:** 14,094,000 Hz back to 14,074,000 at both widths.
- **Empty:** `Nothing saved here yet - press the star to save where you are.`, hittable False, then `Manage favorites…`.
- **The star:** at `[62,2 67x26]` at all nine sizes. A press every 6 px across the status strip outside the star saved nothing.

### The nine sizes against unit 381

| | 1920x1040 | 900x620 | 1100x780 | 1280x720 | 1366x728 | 1536x824 | 1400x1040 | 1920x1017 | 2560x1400 |
|---|---|---|---|---|---|---|---|---|---|
| unit 381 | 483 | 71 | 92 | 163 | 171 | 267 | 483 | 460 | 860 |
| tonight | **483** | **71** | **92** | **163** | **171** | **267** | **483** | **460** | **860** |

The panel row is identical at all nine, and the band is 214 at every size except 1100 x 780.

### The entry and exit rounds, name for name

| | Entry (task 0) | Exit (task 4) |
|---|---|---|
| app attempt 1 | 263 of 265, 2 m 28 s: 1 lost at 1 ms; `TheOliviaRowsTests...` red at 7 s | **213 of 214 run, then the test host crashed**: 1 red, this unit's own |
| app attempt 2 | 259 of 265, 2 m 40 s: 5 lost at 1 ms; the same file lock at 8 s | **264 of 265**, 2 m 30 s: 1 lost at 1 ms, no assertion failure |
| app attempt 3 | - | **263 of 265**, 2 m 32 s: 2 lost at 1 ms, no assertion failure |
| engine | **150 of 150**, 4 m 57 s, first attempt | **150 of 150**, 4 m 56 s, first attempt |
| `TheMenuIsUnderTheMouseTests` | 0 of 8 | 0 of 8 |
| `ViewTestsActThroughControlsTests` offenders | - | 1, `TheStopIsAlwaysOnScreenTests.cs:102` |

**The same 265 names at exit as at entry.** After the repair, every one of them passed in attempt 2 or attempt 3:
- attempt 2 lost `TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow`, which passed in attempt 3;
- attempt 3 lost `ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel` and `TheWindowHoldsBelowItsMinimumTests.TheWorkingPanelsScrollInsideThemselvesRatherThanCollapsing`, which both passed in attempt 2;
- `TheOliviaRowsTests` was green in both.

**No regression.**

**The carry-forward line: 62 terms before and 62 after**, with one term renamed, `TheFavoritesAreBackOnTheRigDisplayTests` to `TheFavoritesAreUnderTheGreenZoneTests`. Inside it:

| Name | What happened |
|---|---|
| `PressingTheStarSavesTheDialAndTheModeWithANameAndPressingItAgainRemovesIt` | unedited |
| `TheStarAndTheCaretAreDrawnAndHittableAtAllNineSizesAndDoNotOverlap` | now `TheStarIsDrawnAndHittableAtAllNineSizesAndNoCaretIsAnywhereOnTheRigFace` |
| `TheListOpensFromTheRigDisplayAndOneClickTunesToTheSavedFrequency` | now `TheDropDownUnderTheGreenZoneOpensTheNamedSpotsAndOneClickTunes` |
| `WithNothingSavedTheListSaysSoRatherThanOpeningEmpty` | name kept, body rewritten |
| `TheWayBackInCostTheTopBandNothing` | name kept, body rewritten |

## 4. What's blocking us

**One item is in the way of a criterion in B: item 1, 10.3's last 36 px.** The other six are findings and choices.

**1. The map reaching the band's full 214 at 1400 needs the pills row to stand somewhere else. That is a layout decision about your pills.** *A finding with its numbers, not a ruling I can make.*

With the map between the card and the rig face, the pills get the room left of the map: 343 px at 1400, against the 675 they need. As a non-wrapping `StackPanel` they would be clipped (a lost pill). As a `WrapPanel` they wrap to two rows and the band goes to 252.

The ways to 214 at 1400 are:
- move the map to the band's left edge, which gives the pills 961 px;
- move the pills out of the row above the card;
- narrow the pills.

**Each one changes where your pills or your map sit on the screen**, against your 2026-08-26 and 2026-09-12 arrangements. What I rejected and why: moving the map left without asking, because it relocates the pills you reach for first and moves the map away from the mockup's place.

**2. `CardFloor = 400` and the caption's place are my choices, and both are overrulable.** *Author's, marked in the code.* Section 1 decisions 1 and 2 have the reasoning. Lowering the floor lets the map grow at more widths, at the cost of more wrapping in the green block.

**3. The exit's first app attempt crashed the test host, and the cause was not determined.** *A finding, not called environmental.*

Attempt 1 ran 214 names with one red, which was this unit's own and is repaired. Then the host crashed. That red's press had never reached the button, because the test didn't move the headless mouse first. I made two changes:
- the press now moves first, as `TheStopIsAlwaysOnScreenTests.Press` does;
- every window now shuts any list a press opened before it closes.

The crash did not recur in two further attempts. Whether it came from a popup outliving its window or from something else is **not determined**.

**4. `TheOliviaRowsTests.WithRowsPresentNothingIsComposedUntilAPressAndEachPressCarriesItsRowsVariant` went red on a file lock in both entry attempts.** *Inherited, recorded, not chased.* The error was `IOException` on its own temp `refuse\2026-09-22.jsonl`, *being used by another process*, at 7 to 8 s. The type alone was 7 of 7, and it was green in both completed exit attempts. Nothing under `src` or `tests` had moved since unit 387's exit when it first failed.

**5. Section 5's mismatch, confirmed and not repaired: 10.5's ticked text says `TheTopRowTests` is on the carry-forward line, and it is not.** Line 7 has **0** occurrences. The line carries `Unit376TheTopBandTests.TheTopBandIsOneShortRowAndThePanelsAreTallerByTheDifference`, `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` and (renamed tonight) `TheFavoritesAreUnderTheGreenZoneTests`.

**6. Two tests beyond the ruled one were rewritten under R12.** *Decisions of mine, stated in section 1 decision 4.* They are `TheTopRowTests.TheWorldClockIsAtTheCardsRightEndWithOneMarker` and `TheGreenZoneTests.NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth`. Both asserted the map inside the card at 134, which is what rev7 replaced. Neither is on the carry-forward line.

**7. The renamed type's file keeps its old name.** *A consequence of the permission mode.* `git mv` and `rm` are refused. A later session with those permissions can rename `TheFavoritesAreBackOnTheRigDisplayTests.cs` to match its type in one command.

### On the carried items tonight's work touched

- **Unit 387's item 2 - 214 against 220.** Answered by Tim, not by me: rev7's 10.3 reads *the band itself does not grow, 214 px stands.* Carried closed.
- **Unit 387's item 3 - 1100 x 780.** Re-measured and in section 3 beside the old numbers. Not ruled on.
- **Unit 387's item 4 - `TheMenuIsUnderTheMouseTests`.** 0 of 8 at entry and 0 of 8 at exit, on the same precondition. Tonight's layout did not touch `DigitalDecodedRows` or `DigitalMineRows`, and I did not repair it.
- **Unit 387's items 1, 5, 6 and 7.** Not mine and not re-recorded. The `RULES_AT` split is **the eleventh unit running**: `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)`, `CLAUDE.md` holds `CPS-DEC-0165`, and `tools\` is not mine to edit.
- **Unit 383's inherited reds.** Not hunted. `ViewTestsActThroughControlsTests` scanned 52 view test files at exit and **the offender count is still one**, `TheStopIsAlwaysOnScreenTests.cs:102`. Tonight's view tests act through the control with a headless pointer and write nothing a control owns.
- **The reload's second disagreement.** The `PHASE_OUTCOME` header says step 6 is done while the last entry for it says partial. That is R42: Tim closed step 6 by ruling after the judging session's `partial`. The header is right and the entry is history.
- **`docs/RADIO_SHEET.md`.** None of the strings I changed or added is quoted there. The one note reused (*Nothing saved here yet…*) is unit 387's, word for word.

**`validate-output.bat`: hand-checked, not run. This is the ninth unit running.** The exact command was `./tools/arbiter/validate-output.bat output.md`, and the answer was `This command requires approval`. That is the permission mode, which a non-interactive session cannot answer. The table below is a hand-check against the rules the script's own header lists. It is not a run of the validator.

| Rule, from the script's header | Hand-check |
|---|---|
| 1 - a `UNIT:` line above section 1, parseable | **ok** - line 22, inside the fenced block; section 1 at line 38 |
| 2 - the four top-level sections, in order, exact names | **ok** - lines 38, 81, 93, 209 |
| 3 - no fifth top-level section | **ok** - exactly four `## ` lines; everything deeper is `### ` |
| 4 - section 4 present even when empty | **ok** - present, not empty, straight apostrophe |
| 5 - section 3 non-empty | **ok** |
| 6 - the ordering block above `UNIT:`, A, B, C, C naming a count | **ok** - `READ IN THIS ORDER.` line 1, `A.` 3, `B.` 8, `C.` 15 with *raises 7 items* |

### Asks still outstanding - carried per HM-DEC-139, verbatim

**The queue stands at sixty and this unit answers none of them**, beyond item 2 of unit 387's, which Tim answered in rev7. It is the fifty-three unit 387 carried plus unit 387's own seven, read out of unit 387's `output.md` at `752a9b62`.

**Unit 387's own seven, verbatim:**

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

**Unit 386's own six, verbatim, as unit 387 carried them:**

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

**Unit 383's ten, carried by units 385, 386 and 387, verbatim:**

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

**And the queue of twenty-nine that units 385, 386 and 387 carried by reference, unanswered here:**

Unit 382's item 5; unit 381's item 1; unit 380's items 1 and 4; unit 379's items 1, 3 and 7; unit
378's items 1, 3 and 4; unit 377's item 4; unit 376's items 3, 4 and 5; unit 375's items 3 and 4;
unit 374's item 3; unit 373's item 2; unit 372's items 4 and 7; unit 371's five; unit 369's four.
