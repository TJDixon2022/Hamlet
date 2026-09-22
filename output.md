```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. After this unit every criterion a
   session can move is met or honestly unmet; 5.1 waits on Tim.
B. The criteria, one line each, met or not, with the number (section 3, first table).
C. The report last, and section 4 carries the queue. This report raises 9 items
   for Tim; items 1 to 3 bear on B (ticks he may want to reverse), none on A.
```

```
UNIT:       390 - complete at task 5 of 6 (tasks 0 to 5, all six), none dropped - 2026-09-22 17:24
PHASE GOAL: Hamlet keeps what it has already been given, hardened and proved, with nothing new opened; the phase ends when Tim looks at it (5.1).
UNIT GOAL:  Close every open criterion a session can reach in one pass - 7.5, 9.3, 9.2, 9.4, 9.5, 7.2, 4.3, 0.1, 10.3 - make the favorites row worth looking at, and leave only Tim's 5.1.
ADVANCED:   yes - step 7 criterion 5 is built, and every other session-movable criterion is ticked with its number.
NUMBER:     unticked criteria a session can move 8 -> 0 (all unticked 9 -> 1, the one left 5.1); plus 7.5, ticked early by unit 378, now earned
DRIFT:      carried
```

## 1. What Claude did

**Complete: all six tasks (0 to 5) done, none dropped, every task committed and pushed to `main`.** Machine QUIVERFULL, project Hamlet. The gate held: `SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` exist, `CoreHMI.sln` and `MURC.sln` don't, root `C:\Source\HamLet`. Nothing in this report is evidence about the radio. No port was opened, nothing was keyed, and no send path, composer, Arm or PttOn site was touched.

| Task | Commit | What |
|---|---|---|
| 0 | `406d1efe` | 1.13.76 -> 1.13.77, `UNIT 390 - STEP 7` in `PHASE_OUTCOME.md`, `PHASE_STATUS.md` to step 7 and unit 390. Entry round below. |
| 1 | `901b085a` | **7.5.** The filled mode chip is now the chosen mode. The chips are raised buttons with a hand cursor, a hover state and a tooltip giving the cited frequency. The label reads *tune to:*. |
| 2 | `c0157488` | **The favorites row is chips** (refines 10.6). The drop-down is off the window. |
| 3 | `7bee209a` | **9.3** under ruling A: the RST fields are editable, with *heard* and *yours* told apart in the dialog and in the record. |
| 4 | `dbb6647c` | **9.2, 9.4, 9.5, 7.2, 4.3 and 0.1** re-measured by name (38 of 38) and ticked. No file under `src`. |
| 5 | `72ebeb73` | **10.3** stated, asserted by a new test and ticked. No file under `src`. |

**Task 1: why 7.5 was ticked and still wrong on Tim's screen.** Unit 378 asserted `DigitalModeChip.IsChosen` and the send line, and both were right. But the strip drew its fill from `IsLit`, which means the dial is inside that mode's map block. Olivia's cited 20 m dial, 14.0715, sits inside PSK31's block. The new test was watched red first, 4 of 6: under FT8, FT4 and Olivia, with the dial at 14.0715, the filled chip was PSK31. The send line already read `ChosenDigitalMode` (unit 378), so it needed nothing.

**Task 2: two measurements the trace forced** (`Unit390TraceTests`).
- The scroller's horizontal bar reserved 16 px, so it is now hidden.
- `Unit376TheTopBandTests.TheNineSizesHoldAndThePanelsAreNeverShorterThanTheyWere` went red at 900 × 620 (68 against 71). The cause was **task 1's own chip padding** (10,3 to 12,4 with a 2 px bottom edge), not the favorites. It is now 12,2, one pixel shorter than before this unit, and the guard is green.

**Task 3.** `LogReport` holds one box per report. The entry leaves a field exactly as the ledger made it until the box differs from what was heard. After that, his value goes into the field the report was already in: RST for PSK31 and Olivia, decibels for FT8 and FT4. `psk31_contact_logged` gains `rstSentSource` and `rstReceivedSource` (`heard` / `yours` / null).

**Tests rewritten under R12:**
- `TheFavoritesAreUnderTheGreenZoneTests` lost its two drop-down names (`TheDropDownUnderTheGreenZoneOpensTheNamedSpotsAndOneClickTunes`, `WithNothingSavedTheListSaysSoRatherThanOpeningEmpty`) because that control is gone. Its two star names are unedited, and its band name now measures the chip row.
- `TheLogDialogAndTheWorkedMarkTests.NothingTypedOverwritesAnythingObserved`: remark updated, body unedited, still green.

**Carry-forward line 7 went from 62 to 65 terms:** `TheChipSaysTheChosenModeTests`, `TheFavoritesAreChipsTests`, `TheRstIsYoursToCorrectTests`, each with its paragraph in `docs/carry-forward-tests.txt`.

**Decisions I made myself (author's, overrulable, none recorded in `DECISIONS.md`):**
1. A chosen chip whose dial is not in its block is **still filled** (7.5 requires the fill), and is set apart by italic text and by its tooltip ending *chosen, the dial is not there*. That keeps a remembered press from reading as a measurement of the radio (§0.0).
2. **Olivia's "here" is its cited dial.** Olivia has no map block, so before this its chip read *chosen, the dial is not there* while sitting on its own dial. That was false; I measured it and fixed it in task 1.
3. **Tooltip wording:** *14.070 · PSK31*, and for Olivia the calling spot with the dial beside it: *14.073 · Olivia (dial 14.0715)*. A band with no row says so.
4. **The chips follow the band pills' look:** white, slate edge, rounded, the spot and mode in the family's ink, the name capped at 140 px with an ellipsis (the full name is in the tooltip). One row that scrolls sideways; there is never a second row. Saved modes `USB-D` map to digital and `USB-?` to open; everything else goes through `ModeGuide.FamilyFor`.
5. **The marks read *heard by Hamlet* and *yours - typed by you*.** The prefilled *Report you sent* is also marked *heard by Hamlet*, because the dialog already used that phrase for every observed field.
6. **I added an overload to `TheTopRowTests.Realized` that takes settings**, so saved favorites load the way they do at startup.

**Status cadence:** `PROJECT_STATUS.md` was written from the clock at every task boundary and immediately before every long test run. I didn't time the gaps inside task 2, so I can't claim every interval there stayed under ten minutes.

## 2. What the owner should expect

On the Digital tab, the strip reads *tune to:* followed by a row of raised buttons. Only the mode you chose is filled: under Olivia, Olivia is lit and PSK31 is not, even on 14.0715. Hovering a button shows where it takes the radio (*14.070 · PSK31*), and a chosen mode whose dial is elsewhere is in italics. Under the green zone, your saved spots are now chips. Each shows its frequency and mode in the mode's color, then the name you gave it. One click tunes there, the star on the rig face fills when you're on a saved spot, and a ✕ appears on hover to forget one. With nothing saved, the row reads *no spots saved yet - press ☆ to keep this one*. In the Log dialog, the two reports are boxes. What Hamlet heard is filled in and marked *heard by Hamlet*; anything you type is marked *yours*, and the record keeps which it was. **Every appearance claim here is computed in the headless test window, not seen on a screen.**

**What will look wrong but isn't:**
- At 1400 wide only about one favorite chip fits beside the sun map's caption; the rest scroll sideways with the scrollbar hidden. I haven't verified that the mouse wheel scrolls that row.
- The test font runs about 10 px per character, so the real screen fits more than the tests show.
- `FavoritesDropDownControl.cs` is still in the tree but nothing places it on the window.

**Tests:** the exit round was **app 278 of 278 and engine 150 of 150, both green on the first attempt**. Version 1.13.77, branch `main`, and every push went through.

## 3. What you should see

**The answer: every criterion a session can move is ticked. Unticked criteria went from 9 to 1, and the one left is 5.1, yours.**

| Criterion | Before tonight | Now | The number |
|---|---|---|---|
| 7.5 | ticked, not true on screen | **met** | fill = chosen under FT8, FT4, PSK31, Olivia; 6 of 6; *29 s of Olivia*, *13.3 s of PSK31* |
| 9.3 | open (read-only RST) | **met** under ruling A | 4 of 4; 599 heard, then 579 typed → `<RST_RCVD:3>579`, `rstReceivedSource: yours` |
| 9.2 | open | **met as built** | `TheCarrierHoldsTheButtonsTests` 8 of 8; the plan's words that differ are named (section 4 item 2) |
| 9.4 | open | **met** on the fixture | 4 of 4; the garbled 17:48:33 over belongs to the demodulator |
| 9.5 | open | **met** | 3 of 3, plus the shared RST dialog |
| 7.2 | open | **met** | `TheCannedListIsOfferedTests` 14 of 14: announced, withinCap, `macro: canned`, no text, all seven |
| 4.3 | open | **met** in the sheet's own voice | 183 lines; 0 tier-1 phrases in prose; 2 inside declared Hamlet quotes (lines 110, 151); 9 hardware nouns |
| 0.1 | open | **met as a negative** | 119 commits in `681d45c8..ec4b466e`, 0 touching `*ettings*` |
| 10.3 | open | **met as built**, one case stated | 393 × 214 at 1920 (FT8, PSK31, Olivia) and 1400 (FT8, Olivia); 327 × 178 at 1400 on PSK31 off 14.070; band 214 |
| 10.6 | met | refined | chips 4 of 4; band 214 at 1920 and 1400 with three saved |
| 5.1 | open | open | yours |

**Entry and exit rounds** (both command lines unedited, one build each):
- **Entry:** app attempt 1 was 261 of 266, with 5 lost to the headless dispatcher loop at 1 ms. The re-run was 265 of 266, with 1 lost the same way on a different name. Every name went green in at least one attempt and none failed an assertion. Engine was 150 of 150 in 4 m 55 s.
- **Exit:** **app 278 of 278 in 2 m 37 s, engine 150 of 150 in 4 m 53 s, both first time.**
- **The 278:** the entry's 266, plus 6 + 4 + 4 new names, minus 2 removed. No regression.

**Guard runs along the way:**
- Task 1: 75 of 75.
- Task 2: 59 of 59 across the favorites, top-row, band, nine-size, working-panel, window-minimum, give-up-height, Stop, binding, chip and voice guards.
- Task 3: 55 of 55 across the log dialog, ledger, dial, privacy, card, Olivia-card, binding, voice and word-count guards. The Log dialog now carries 766 characters against its 850 ceiling (it was 716).

## 4. What's blocking us

**Nothing blocks. Items 1 to 3 are ticks you may want to reverse; the rest are findings.**

**1. 0.1 is ticked on this instruction's word, which overrules instruction 386 section 6 ruling 1 ("never ticked by a unit of this phase").** *A ruling request if you disagree.* The later instruction wins under `PHASE_PLAN.md` §6, and both were the author's and overrulable. The tick says what is true: no such commit exists (119 commits, 0 on a settings path). **Option A:** keep the tick as *met as a negative*. **Option B:** untick it and reword 0.1 so a completed negative meets it. **Industry standard:** B, because a criterion should be met by its own words. A was taken because the instruction said so.

**2. 9.2 and 9.4 are ticked "as built", and their words don't all match what was built.** *Findings; rewording is yours.*
- **9.2** says *Report, Confirm, the canned lines and the typed line are held - greyed with he is still sending - until his carrier drops*. As built, it would read: *the typed line is greyed with he is still sending, Report and Confirm are not offered and the canned lines give way to one note, until his hand-back or his carrier drops*.
- In 9.2 and 9.4, *replayed from the ... record* would read *replayed from a fixture built to that record's shape*. Your `2026-09-21.jsonl` is not on this machine.

**3. 10.3 is ticked with one case under the band's full height:** 327 × 178 at 1400 on PSK31 with the dial off 14.070, where the card also has to say *PSK31 lives at 14.070*. The instruction stated this case and asked for the tick. Untick it if the case matters to you.

**4. Tonight's ruling A and the 2026-09-07 read-only ruling are in no decision record.** *A mismatch against section 5, which says "HM-DEC- number: find it".* **The 2026-09-07 ruling has no HM-DEC number.** It lives only in `LogContactViewModel`'s remarks and the dialog's markup comment. Ruling A is now recorded in `PHASE_PLAN.md` 9.3, the code remarks and the commit. Neither is in `DECISIONS.md`, and I did not assign an id (§4.8).

**5. 10.6's ticked words (*one control reading Favorites*) now describe the drop-down this unit replaced.** Rewording is yours.

**6. `src\Hamlet.App\Controls\FavoritesDropDownControl.cs` should be deleted.** Nothing places it. It stays only because `Unit388TraceTests` names the type and `rm` is refused here. It is marked as off the window in its own remarks.

**7. Section 5 mismatches:**
- **7.5 was already `[x]`** although the instruction says it was never built. The tick was early (task 1's finding).
- **At 1100 × 780 the map is 246 × 134 in a band of 402**, not the 327 × 178 stated for "under 1400 wide". It is asserted as measured in `TheSunMapStandsWhereItWasLeftTests.AtTheSizeItOpensAtItKeepsTheMockupsSize`.
- **Unit 389's 9.2 finding holds:** 1 of 4 controls is greyed, and the hold ends at the hand-back.
- **The RST prefill and the words list:** `docs/RADIO_SHEET.md` quotes none of the strings changed tonight. I checked by running its test, which stayed green.

**8. The favorites row scrolls sideways with the bar hidden, and I haven't verified wheel scrolling.** *A finding.* At 1400 roughly one chip is in view beside the map's caption. If the hidden row doesn't respond to the wheel, a small arrow or a count is a one-unit follow-up.

**9. The `RULES_AT` split, the thirteenth unit running.** `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)` because `tools/status.sh` writes it as a literal, while `CLAUDE.md` holds `CPS-DEC-0165`. `tools\` is not mine to edit.

**`validate-output.bat`:** not run; it asked for approval in earlier units. Hand-checked against its six rules: `UNIT:` above section 1; four sections in order with exact names; no fifth; section 4 present; section 3 not empty; the ordering block above `UNIT:` with A, B, C and a count.

### Asks still outstanding

Carried per HM-DEC-139. **Unit 389's item 2, 9.3's conflict with the 2026-09-07 ruling, is answered by your ruling A of 2026-09-22 and dropped.** The instruction's section 3 also answers 7.5 and the favorites; neither was on the queue.

**Unit 389's own, verbatim (items 1 and 3 to 7):**

**1. 9.2 falls short in two places, and both are on the send path, so they were left.** *A finding with numbers. Changing either needs a licence, because each changes whether a send control is enabled.*
- **Only 1 of 4 controls is greyed.** Report and Confirm are withheld by R1 mid-over, and the canned lines become a note.
- **The hold ends at his hand-back, not when his carrier drops.** Unit 385 measured that holding to the drop would refuse every answer for 9 s after a PSK31 `K`, and 22.94 to 38.23 s after an Olivia one.

Unit 385's claim was *"3 of the four controls held with one sentence and the fourth already withheld by R1"*. Measured, all four are held, three carry the sentence, and one is greyed.

**3. Neither replay is the record, and 9.4's is not what you got.** *A finding. Its remedy needs your file, or a licence to open the engine.* Your `2026-09-21.jsonl` is not on this machine. Unit 385's item 2 measured that `Psk31MessageSplitter` completes nothing from a garbled over, and that site is under `src\Hamlet.RadioEngine\Psk31\`.

**4. 10.3 at 1400 on PSK31 with the dial off 14.070 falls back to stage A.** *A finding. Whether it blocks 10.3 is a judging session's reading.* The card also draws *PSK31 lives at 14.070; you are at 14.074*, and at 401 px wide that no longer fits in 178 px. Any fix would cost one of three things you ruled on: the map's full height, the rig's width, or the strayed line. On 14.070 the same window stands at the left edge. The fallback is what keeps that line on the screen.

**5. The card's green word and the row's *sending* can disagree for a few seconds.** *A finding.* The card follows the hold, which ends at his hand-back. The row follows his carrier. For the 9 s (PSK31) to 38 s (Olivia) tail after a `K`, his row still says *sending* while his card no longer does.

**6. *Your turn?* now stands beside the unchanged *His turn, a guess*.** *Author's. It is one line either way if you want them to match.* R14 held me to the one word 9.4 names.

**7. `validate-output.bat` - see the last line of this section.**

**The older queue, carried by reference as units 385 to 389 did:** the whole of *"Unit 388's section 4, carried per HM-DEC-139, verbatim"* and *"Asks still outstanding - carried per HM-DEC-139, verbatim (unit 388's)"* in `output.md` at commit `cd18cc6c`, lines 270 to 506. That covers unit 388's seven; the sixty (unit 387's seven, unit 386's six, unit 385's eight, unit 383's ten); and the twenty-nine carried by reference from units 369 to 382. **None of them is answered tonight** except as follows. Unit 383's item 1 (4.3 partial under the strictest reading) and unit 386's item 6 (0.1 stays unticked) are overtaken by tonight's ticks, and items 1 and 3 above put both back to you.
