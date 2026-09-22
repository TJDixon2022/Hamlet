READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Steps 1, 2, 3, 6 and 8 done; step 0
   partial with 0.1 closed to units by instruction 386; step 4 partial with 4.3
   logged to the owner; step 7 partial on an unjudged 7.2; step 5 Tim's own and it
   ends the run. Step 9's work landed at e5e4bee0 and had never been graded; step 10
   partial on 10.3 alone, the map 178 of the band's 214.
B. Step 9 - 9.1 met (ticked tonight); 9.2 not met: the four controls are held but
   only 1 of 4 is greyed with "he is still sending", and the hold ends at his
   hand-back, not when his carrier drops; 9.3 not met: the RST fields are not
   editable; 9.4 met differently: its rule is met and the card now reads "Your
   turn?", but the 17:48:33 replay is a constructed fixture, not the record; 9.5
   met differently: the four are alike on both modes, but two of them fall short.
   card_dismissed 0 -> 1 writer (the dismiss press, renamed in place from
   card_cleared). The 17:45:40 replay is held, with 0 keyings, but it is a
   constructed fixture that presses the typed line, not the Report. The 17:48:33
   replay reads "Your turn?". Step 10 - 10.3 partial: the map is 393 x 214 in a
   band of 214 at 1920 on all three modes and at 1400 on FT8 and Olivia; at 1400
   on PSK31 with the dial off 14.070 it stays 327 x 178. The pills are 1 row at
   1920 and 1400. The fallback switches at 1399 px (1451 on PSK31 off its
   frequency).
C. The report last. Section 4 raises 7 items on top of the carried queue, and
   items 1 to 4 are in the way of a criterion in B: 9.2, 9.3, the two replays,
   and 10.3's one case. Yes: two step 9 shortfalls sit on the send path, 9.2's
   greying and its release at the hand-back, and both were left exactly as they
   stand.

```
UNIT:       389 - complete at task 4 of 5 - 2026-09-22 14:55
PHASE GOAL: Hamlet keeps what the PSK31 and Olivia threads already earned - the
            screen, the record and the tests - hardened without the radio or the
            owner, proved by tests that ran, and judged at the end by Tim at his
            window.
UNIT GOAL:  Measure unit 385's five step 9 fixes against HEAD in the criteria's
            own words, so a judging session can grade them for the first time.
            Close the small shortfalls that are off the send path, report the ones
            on it, and stand the sun map at the band's left edge so it fills the
            band's 214 px.
ADVANCED:   step 9, criteria 9.1 (met and ticked), with 9.2 and 9.4 moved and
            9.2 to 9.5 measured for grading; step 10, criterion 10.3 moved, not met
NUMBER:     step 9 clauses met at HEAD: 23 of 23 claimed by unit 385 -> 13 of 23
            measured at entry -> 17 of 23 after tonight; the sun map's share of its
            band: 83.2% -> 100% at 1920 and 1400 (83.2% at 1400 on PSK31 off
            14.070)
DRIFT:      none
```

## 1. What Claude did

**Complete at task 4 of 5 (tasks 0 to 4). Nothing was dropped, including the named drop candidate, task 3.** Claude Code on QUIVERFULL, `PROJECT: Hamlet` confirmed against the tree (`SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present, no `CoreHMI.sln` or `MURC.sln`, root `C:\Source\HamLet`), branch `main`. **No port was opened, nothing was enumerated and nothing was keyed** (FACT-004).

| Commit | Task |
|---|---|
| `chore(unit389): task 0` | 1.13.75 -> 1.13.76, step 9 and unit 389, the three launcher files unaltered, entry round |
| `test(unit389): task 1` | `Unit389TraceTests`, the clause table, the token, the replays, the left-edge budget |
| `feat(unit389): task 2` | `card_dismissed`, the card's green word, *Your turn?* |
| `feat(unit389): task 3` | the sun map at the band's left edge |
| `docs(unit389): task 4` | the exit round, the plan, the status lines, this report |

**Task 0.** Version bumped with its log line. `PHASE_STATUS.md` set to step 9 and unit 389. The three launcher files' diffs were read. `PHASE_OUTCOME.md` appends 17 lines, `PHASE_STATUS.md` rewrites only its `CURRENT_STEP` and `HEARTBEAT` header lines, and `RUN_LEDGER.md` appends one row. **All three were committed unaltered.** `## UNIT 389 - STEP 9` appended. Entry round: **app 265 of 265, engine 150 of 150, both first attempt.**

**Task 1, the trace, no file under `src`.** Step 9's four line-7 types, read off `ad3b70ab`'s diff to the line, ran together in one build: **19 of 19**. The clause table is in section 3. `card_cleared` had **one** writer, the dismiss press. Both replays are constructed fixtures. The left-edge budget found something the instruction did not know: **at 1400 the card decides, not the pills.** The pills would have 960.9 of the 675 px they need. The card slot would be 400.9 against `CardFloor` 400, so the card's break-even is a window of about 1399 px.

**Task 2, off the send path only.** Closed:
- **9.1's token.** Renamed in place, because the press was the only writer. The privacy walk and every reader changed in the same commit.
- **9.2's card colour.** The card's existing `SendingWord` was bound nowhere. It is now `CardSendingWord`, in the row's own green, asserted drawn.
- **9.4's word.** A guessed your-turn now reads *Your turn?*.

Left: the send-path shortfalls, 9.3's editable RST (Tim's ruling), and both replays. Details are in section 4.

**Task 3, the named drop candidate, built and held.** `BandGovernsTheMapPanel` is handed the pills row. It lays out map, card, rig from the left wherever two things hold: a map as tall as the whole band leaves the pills their one row to its right, and the card keeps its floor within its row. Otherwise it keeps unit 388's stage A. **This is decided by measured widths each layout, not by a window size.** The pills never wrapped and the band never grew, so **nothing was reverted.**

**Task 4.** Exit round: **app 266 of 266, engine 150 of 150, both first attempt.** `PHASE_PLAN.md`: 9.1 ticked with tonight's numbers. 9.2 to 9.5 and 10.3 are left unticked, with their reasons beside them. The header state words were not touched.

### The decisions this unit made for itself, reproduced in full

1. **9.1 is the only criterion ticked.** Ruling 1 item 5 ticks a criterion only when every clause is met. 9.4's replay and 9.5's *the four* both depend on a shortfall a unit cannot close, so I did not tick them. That leaves a judging session to read them.
2. **The card's word follows the hold, not the carrier.** I bound the card's existing `SendingWord` rather than adding a second property. It means *his carrier is up and he has handed nothing back*, which is the hold's own reading. Building a second reading would have been a second mechanism.
3. **Only *Your turn, a guess* became *Your turn?*.** *His turn, a guess* is unchanged, because 9.4 names only the one word (R14).
4. **The view test for the card's green reads the card, not the row.** The headless window does not realize the decoded rows. The row's word is unit 385's, asserted on the view model, and drawn with the same resource.
5. **The sun map's rewritten test asserts where the map must stand, case by case, from a table.** It does not read the panel's own answer. PSK31 at 1400 on the fixture's dial is asserted to fall back, and PSK31 at 1400 on 14.070 is asserted to stand at the left edge.
6. **Pushed after every task**, because the session's own prompt asked for it. Instruction 389 section 11 says to push once at the end.

## 2. What the owner should expect

**Of the four things that went wrong with KC3FL, here is what now holds on your screen.**

- **The X holds.** It takes any card or receipt away, and the record now calls the press `card_dismissed`, the plan's own word.
- **The hold holds, but not the way 9.2 describes it.** While he is mid-over, nothing you press goes out: 0 keyings, and the refusal says *he is still sending*. His card now says *he is still sending* in the same green his row uses for *sending*. But only the typed line's Send is **greyed** with that sentence. The Report or Confirm button is simply not offered mid-over, because Hamlet is not yet sure it is your turn. The canned lines become one note. And the hold lets go the moment he hands back with `K` or `BTU`, not when his carrier finally drops. That is unit 385's deliberate correction: his row lingers for 9 s on PSK31, and 23 to 38 s on Olivia. Those two differences are on the send path, so they were **measured and left alone**.
- **Log holds.** It is on his card from the moment the card exists, with his report filled in and nothing invented. **But you still cannot edit the RST fields.** Your own ruling of 2026-09-07 made everything Hamlet heard read-only in the Log dialog, and 9.3 asks for the opposite. That choice is yours.
- **The turn holds.** Every hand-back moves the card, and a guessed one now reads *Your turn?*. **But the garbled over you actually got at 17:48:33 still would not move it.** It never becomes a parsed line. That fault is in the engine, and no unit has been licensed to open it.

Neither replay is your record, which is not on this machine. Both are fixtures built to R44's timings.

**The sun map now fills the whole band**, 393 x 214, standing at the left edge with the pills in one row to its right. That holds at 1920 and at 1400. **What will look wrong but is not:** at 1400 on PSK31 with your dial off 14.070, the card also says *PSK31 lives at 14.070; you are at 14.074*. It then has no room beside a full-height map, so the map drops back between the card and the rig at 327 x 178. Below about 1400 px wide it always does that.

Every claim here is computed, not seen (FACT-004). **No port was opened, nothing was enumerated, and nothing was keyed.**

## 3. What you should see

### Step 9, clause by clause, before and after

"Met differently" and "met by R1" are reported in those words, not rounded up (ruling 1 item 1). File paths are under `tests/Hamlet.App.Tests/ViewModels/` unless named.

| Clause | Before (entry) | After | Test and assertion line |
|---|---|---|---|
| 9.1 X on every card and every receipt | met | met | one unconditional `hm-cardx` in the one template, `Views/Unit297CardBindingTests.cs:117-118`; pressed on a card and a receipt, `TheCardOffersLogAndAnXTests.cs:158`, `:207` |
| 9.1 dismissing removes it | met | met | `TheCardOffersLogAndAnXTests.cs:158`; Olivia `TheFourAreOnOliviaCardsTooTests.cs:143` |
| 9.1 writes `card_dismissed` | **not met** (`card_cleared`) | **met** | `TheCardOffersLogAndAnXTests.cs:172`; Olivia `TheFourAreOnOliviaCardsTooTests.cs:151` |
| 9.2 his row carries a color | met | met | `DecodedRowSendingWord`, `HmGreenBrush`, in the markup; no view assertion (the headless window does not realize the rows) |
| 9.2 his row carries the word *sending* | met | met | `TheCarrierHoldsTheButtonsTests.cs:320` |
| 9.2 his card carries a color | **not met** | **met** | `TheCarrierHoldsTheButtonsTests.cs:416`, the drawn `CardSendingWord` in that exact brush |
| 9.2 his card carries the word | met differently: only in grey notes | **met** | `:415`; Olivia `TheFourAreOnOliviaCardsTooTests.cs:84` |
| 9.2 the four are held | met differently: Report and Confirm *already withheld by R1* | unchanged | `:329`, `:333`, `:340`; 0 keyings `:104` |
| 9.2 greyed with *he is still sending* | **not met: 1 of 4** | **not met: 1 of 4** (send path, left) | `:333` typed line only |
| 9.2 until his carrier drops | met differently: ends at his hand-back | unchanged (send path, left) | `:252` |
| 9.2 a send during it refused with the sentence | met | met | `:108` |
| 9.2 `send_refused reason his_carrier_live` | met | met | `:115` |
| 9.2 replayed from the 17:45:40-17:45:44 record, the Report held | met differently: a constructed fixture, the typed line | unchanged | `:291`, `:297` |
| 9.3 Log from the moment the card exists | met | met | `TheCardOffersLogAndAnXTests.cs:80`; Olivia `TheFourAreOnOliviaCardsTooTests.cs:138` |
| 9.3 RST fields editable | **not met** | **not met** (Tim's 2026-09-07 ruling) | no assertion; `LogContactWindow.axaml:63-75` draws them as text |
| 9.3 defaulted to what was exchanged | met | met | `TheCardOffersLogAndAnXTests.cs:128`; both reports `ThePsk31LogsWithRstTests.cs:99-100` |
| 9.3 no certain 73: what is known, nothing invented | met | met | `TheCardOffersLogAndAnXTests.cs:133-134` |
| 9.4 every parsed hand-back moves the turn to *your turn* | met | met | `TheTurnMovesOnEveryHandBackTests.cs:81`, `:91` |
| 9.4 marked as a guess when uncertain | met | met | `:92` |
| 9.4 not only the first answer | met | met | `:91` (the second hand-back) |
| 9.4 replayed from 17:48:33, reads *your turn?* | **not met**: *Your turn, a guess* | met differently: reads *Your turn?*, from a fixture that is not the record | `:163` |
| 9.5 the four alike on PSK31 and Olivia | met differently: partly *by identity* | met: every one driven on Olivia | `TheFourAreOnOliviaCardsTooTests.cs:84-94`, `:138-151`, `:181`; `TheTurnMovesOnEveryHandBackTests.cs:195` |
| 9.5 asserted | met | met | the same |

**Count: 23 clauses. Unit 385 claimed 23; 13 were met at entry and 17 are met now.**

### 9.2's four controls, mid-over

| Control | Held | Greyed | Words |
|---|---|---|---|
| Report | yes: *already withheld by R1*, `Offered None`, not drawn | no | *he is still sending* as `OfferNote`, where the button stands |
| Confirm | yes: *already withheld by R1*, the same one button | no | the same `OfferNote` |
| The canned lines | yes: the seven become one note (the menu's own ruling of 2026-09-06) | no | *Hamlet is offering nothing to send: he is still sending. The lines come back the moment his carrier drops.* |
| The typed line | yes | **yes**: `CanSendTyped False`, bound to `IsEnabled` | *he is still sending* as `TypedHoldNote` |

### `card_cleared`'s writers

| Where | Before | After |
|---|---|---|
| `MainWindowViewModel.cs` `ClearCard`, the dismiss press | `card_cleared`, the only writer | `card_dismissed` |
| anything else under `src` | none | none; `card_cleared` occurs **0** times |

### The two replays, quoted

- **9.2's replay is `TheReportHeSentOnTopOfTheCarrierWouldNowBeHeld`.** A test callsign's carrier at 1726 Hz, mid-over (*"... R R NAME BOB QTH ERIE AND THE RIG HERE IS"*). The typed line *"R R TNX FER RPRT"* is pressed. Result: 0 keyings, one `psk31_send_refused` with `his_carrier_live`. **It is not the record**, the moment exists only in a comment, and it presses the typed line, not the Report.
- **9.4's replay is `AtTheSecondHandBackTheCardReadsYourTurnQuestion`** (renamed tonight). Its second over is *"KC3QIS de W1ABC R R NAME BOB 5#9 QTH ERIE BTU KC3QIS de W1ABC K"*, and the card now reads **`Your turn?`**. **It is not the record either.** This over parses, while the one Tim got at 17:48:33 was garbled and completed no message (unit 385's item 2).

### The band, at 1920, 1400 and 1100 x 780

Numbers are from `Unit389TraceTests`, before and after task 3. FT8 and Olivia measure identically.

| Size, mode | Pills | Rows | Map | Card | Caption | WithPills | WithoutPills |
|---|---|---|---|---|---|---|---|
| 1920 before | 16,83 1888 wide | 1 | 1017,119 327 x 178 | 987 x 178 | in the card's Favorites row | 214 | 178 |
| **1920 after, all three modes** | **423,83 1481 wide** | **1** | **16,83 393 x 214** | **423,119 921 x 178** | 1109,237 220 x 9 | **214** | 178 |
| 1400 before | 16,83 1368 wide | 1 | 497,119 327 x 178 | 467 x 178 | 248,275 | 214 | 178 |
| **1400 after, FT8 and Olivia** | **423,83 961 wide** | **1** | **16,83 393 x 214** | **423,119 401 x 178** | 589,275 220 x 9 | **214** | 178 |
| 1400 after, PSK31 at 14.074 | 16,83 1368 wide | 1 | 497,119 327 x 178 (stage A) | 467 x 178 | 248,278 | 214 | 178 |
| 1400 after, PSK31 at 14.070 | left edge | 1 | 16,83 393 x 214 | | | 214 | |
| 1100 x 780, all, before and after | 16,83 1068 wide | 1 | 278,119 246 x 134 (stage A) | 248 x 393 / 434 / 404 | 173,471 80 x 36 | 429 / 470 / 440 | 393 / 434 / 404 |

- **Map top minus band top is 0 px, and band bottom minus map bottom is 0 px, wherever the map stands at the left edge.** The margins are the pills row's own top (its 10 px margin is outside the band) and the band's bottom (the card, the rig and `RigDriveAndPower`).
- **Aspect:** 1.8364 against the picture's 1.8358.
- **The rig face** is 546 wide at x 1358 (1920) and x 838 (1400), before and after.
- **The switch** was found by halving from 1100 to 1920 at 1040 tall: **1399 px on FT8 and Olivia, 1451 on PSK31** on the fixture's dial.

### The nine sizes

| Size | Unit 381 / 388 | Tonight |
|---|---|---|
| 1920 x 1040 | 483 | 483 |
| 900 x 620 | 71 | 71 |
| 1100 x 780 | 92 | 92 |
| 1280 x 720 | 163 | 163 |
| 1366 x 728 | 171 | 171 |
| 1536 x 824 | 267 | 267 |
| 1400 x 1040 | 483 | 483 |
| 1920 x 1017 | 460 | 460 |
| 2560 x 1400 | 860 | 860 |

### The entry and exit rounds, name for name

| Invocation | Entry | Exit |
|---|---|---|
| app (line 7) | **265 of 265**, 2 m 27 s, first attempt | **266 of 266**, 2 m 41 s, first attempt |
| engine (line 9) | **150 of 150**, 4 m 57 s, first attempt | **150 of 150**, 4 m 54 s, first attempt |

- **All 265 entry names were green at exit.** The one added name is `TheCarrierHoldsTheButtonsTests.HisCardIsDrawnInTheSendingGreenWithTheWord`. One name was renamed in its type, `TheTurnMovesOnEveryHandBackTests.AtTheSecondHandBackTheCardReadsYourTurnQuestion`. **No regression.**
- **No dispatcher loop, no file lock and no test-host crash in any invocation tonight.**
- Carry-forward line 7 has **62** filter terms and line 9 has **25**, before and after, unedited.
- `TheMenuIsUnderTheMouseTests` is **0 of 8**, on the same `RightClickRow` scene precondition as unit 388's exit.
- `ViewTestsActThroughControlsTests` is **1 of 2**. It scanned 52 files and found one offender, `TheStopIsAlwaysOnScreenTests.cs:102`, as before.
- Rewritten under R12:
  - `Unit376TheTopBandTests.TheSunMapIsTheSizeItWasAndStillCarriesHisGrid`
  - `TheTopRowTests.TheWorldClockIsAtTheCardsRightEndWithOneMarker`
  - `TheGreenZoneTests.NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth`
  - `TheTurnMovesOnEveryHandBackTests`' three *Your turn?* assertions
  - `TheAnswerYouHeardTests`, one line
  - `ThePsk31ConversationCardTests`, one line

## 4. What's blocking us

**Items 1 to 4 are in the way of a criterion in B. Items 5 to 7 are findings.**

**1. 9.2 falls short in two places, and both are on the send path, so they were left.** *A finding with numbers. Changing either needs a licence, because each changes whether a send control is enabled.*
- **Only 1 of 4 controls is greyed.** Report and Confirm are withheld by R1 mid-over, and the canned lines become a note.
- **The hold ends at his hand-back, not when his carrier drops.** Unit 385 measured that holding to the drop would refuse every answer for 9 s after a PSK31 `K`, and 22.94 to 38.23 s after an Olivia one.

Unit 385's claim was *"3 of the four controls held with one sentence and the fourth already withheld by R1"*. Measured, all four are held, three carry the sentence, and one is greyed.

**2. 9.3's *RST fields editable* reverses your ruling of 2026-09-07.** *A ruling request.* That ruling says observed fields are not editable in the Log dialog: *nothing typed overwrites anything observed*. 9.3 is later, and the later ruling normally wins. But this changes what goes into your permanent log, so I did not build it.
- **Option A:** editable RST fields that record they were typed rather than heard.
- **Option B:** keep them read-only and reword 9.3.
- **Industry standard:** A. Loggers let you correct an RST, and keeping the "heard" or "typed" mark keeps your ruling's intent.

**3. Neither replay is the record, and 9.4's is not what you got.** *A finding. Its remedy needs your file, or a licence to open the engine.* Your `2026-09-21.jsonl` is not on this machine. Unit 385's item 2 measured that `Psk31MessageSplitter` completes nothing from a garbled over, and that site is under `src\Hamlet.RadioEngine\Psk31\`.

**4. 10.3 at 1400 on PSK31 with the dial off 14.070 falls back to stage A.** *A finding. Whether it blocks 10.3 is a judging session's reading.* The card also draws *PSK31 lives at 14.070; you are at 14.074*, and at 401 px wide that no longer fits in 178 px. Any fix would cost one of three things you ruled on: the map's full height, the rig's width, or the strayed line. On 14.070 the same window stands at the left edge. The fallback is what keeps that line on the screen.

**5. The card's green word and the row's *sending* can disagree for a few seconds.** *A finding.* The card follows the hold, which ends at his hand-back. The row follows his carrier. For the 9 s (PSK31) to 38 s (Olivia) tail after a `K`, his row still says *sending* while his card no longer does.

**6. *Your turn?* now stands beside the unchanged *His turn, a guess*.** *Author's. It is one line either way if you want them to match.* R14 held me to the one word 9.4 names.

**7. `validate-output.bat` - see the last line of this section.**

### On the four carried asks that touch tonight

- **Unit 388's item 1, the map at 1400 needs the pills row elsewhere.** Answered tonight by instruction 389 ruling 2, as author and overrulable. The map moved to the left edge instead of moving the pills. At 1400 the pills have 961 of 675 px and stay one row, the map is 393 x 214, and the band is 214. PSK31 off 14.070 is the one exception (item 4).
- **Unit 388's item 2, `CardFloor = 400` and the caption's place.** The floor was left at 400, and it is now what sets the switch at 1399. The caption did not move: it stays in the card's Favorites row, at 1109,237 at 1920 and 589,275 at 1400.
- **Unit 388's item 5, `TheTopRowTests` on line 7.** Counted again: **0** occurrences. Not repaired.
- **Unit 387's item 1, *make a card anyway* on a nameless row.** Still Tim's. Tonight's step 9 measurement read cards that name someone, so it does not touch it.

### The reload's disagreements

- **The `RULES_AT` split, the twelfth unit running.** `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)` and `CLAUDE.md` holds `CPS-DEC-0165`. `tools\` is not mine to edit.
- **The `PHASE_OUTCOME` header says step 6 is done, and its last entry says partial.** That is R42: the header is right and the entry is history.
- **The three launcher files had only appends and header or ledger lines.** All three were committed unaltered with task 0. No judging session's line was edited, and the `.run-unit/` launcher files were left alone.

### Section 5's lines against the tree

Every line of section 5 matched the tree:
- version 1.13.75
- `PHASE_STATUS.md` stale at step 0 and unit 388
- `HEAD` at `3d980458`
- `docs/carry-forward-tests.txt` 866 lines, line 7 at 62 terms and line 9 at 25
- 265 and 150
- `card_dismissed` 0 times in `src`
- `TopRow`, `BandRow` and the pills in the markup as described
- unit 388's 961 px and 1114 px break-even

**One measurement went beyond it and wins:** at 1400 the card, not the pills, decides the left-edge map, at 400.9 px against a floor of 400.

**`validate-output.bat`:** hand-checked, not run. The exact command, `./tools/arbiter/validate-output.bat output.md`, returned `This command requires approval`, so the table below checks the six rules in the script's own header by hand.

| Rule, from the script's header | Hand-check |
|---|---|
| 1 - a `UNIT:` line above section 1, parseable | **ok** - inside the fenced block, above `## 1.` |
| 2 - the four top-level sections, in order, exact names | **ok** |
| 3 - no fifth top-level section | **ok** - exactly four `## ` lines; everything deeper is `### ` |
| 4 - section 4 present even when empty | **ok** - present, with a straight apostrophe |
| 5 - section 3 non-empty | **ok** |
| 6 - the ordering block above `UNIT:`, A, B, C, and C naming a count | **ok** - `READ IN THIS ORDER.` on line 1, then `A.`, `B.`, `C.`, with *raises 7 items* |

### Unit 388's section 4, carried per HM-DEC-139, verbatim

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

#### On the carried items tonight's work touched (unit 388's)

- **Unit 387's item 2 - 214 against 220.** Answered by Tim, not by me: rev7's 10.3 reads *the band itself does not grow, 214 px stands.* Carried closed.
- **Unit 387's item 3 - 1100 x 780.** Re-measured and in section 3 beside the old numbers. Not ruled on.
- **Unit 387's item 4 - `TheMenuIsUnderTheMouseTests`.** 0 of 8 at entry and 0 of 8 at exit, on the same precondition. Tonight's layout did not touch `DigitalDecodedRows` or `DigitalMineRows`, and I did not repair it.
- **Unit 387's items 1, 5, 6 and 7.** Not mine and not re-recorded. The `RULES_AT` split is **the eleventh unit running**: `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)`, `CLAUDE.md` holds `CPS-DEC-0165`, and `tools\` is not mine to edit.
- **Unit 383's inherited reds.** Not hunted. `ViewTestsActThroughControlsTests` scanned 52 view test files at exit and **the offender count is still one**, `TheStopIsAlwaysOnScreenTests.cs:102`. Tonight's view tests act through the control with a headless pointer and write nothing a control owns.
- **The reload's second disagreement.** The `PHASE_OUTCOME` header says step 6 is done while the last entry for it says partial. That is R42: Tim closed step 6 by ruling after the judging session's `partial`. The header is right and the entry is history.
- **`docs/RADIO_SHEET.md`.** None of the strings I changed or added is quoted there. The one note reused (*Nothing saved here yet…*) is unit 387's, word for word.

#### Asks still outstanding - carried per HM-DEC-139, verbatim (unit 388's)

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
