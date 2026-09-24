READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0, 1, 2 done;
   3 partial on 3.4 and 3.6, with 3.6 parked as P19 on the floors;
   4 and 5 not started; 6 partial on 6.3 and 6.5, 6.4 now ticked; 7 partial on
   7.1 to 7.4 and 7.6.
B. Step 6, criterion 6.4 clause by clause. Every control on the CW tab and the
   band row is named, 6.4's own list first. The test names each one; it was
   watched failing (message quoted below) and is now green. Every tip is quoted
   beside what its command does. Send's element is unchanged but for the tip.
   Then 6.6 at exit: held, with two reds, both there at entry (P16 and the new
   P20).
C. The rest. Section 4 raises 8 items. None is in the way of 6.4. P19 is the
   one the owner has to rule on before step 3 can move again.

```
UNIT:       422 - complete at task 4 of 4, none dropped - 2026-09-24 16:04
PHASE GOAL: Hamlet decodes an on-air CQ call into the text actually sent, and the screen around it never says what is not so
UNIT GOAL:  every button Tim can reach on the CW tab and the band row tells him on hover what it does, Send saying plainly it keys the radio, with a test that names each control and fails when one is silent
ADVANCED:   yes - 6.4 met and ticked: 49 controls named by EveryControlSaysWhatItDoesTests, red at d833bbaa, green at 21808260
NUMBER:     controls with a tip, CW tab 7 of 15 -> 15 of 15; band row 18 of 34 -> 33 of 34 not connected (19 of 34 -> 34 of 34 connected)
DRIFT:      0
```

## 1. What Claude did

**Complete, at task 4 of 4, none dropped.** The drop candidate, proving the connect button in its second state, was not dropped: both states are proven. Machine QUIVERFULL, project Hamlet (gate confirmed), branch `main`, entry `bdd0070b`. Commits, all pushed with rc 0:
- `956986b7` task 0
- `ba39a306` task 1, the inventory, alone
- `d833bbaa` task 2, the test, alone and red
- `21808260` task 3, the tips, with the test green
- the exit commit, which carries this report

**Task 0, the record.**
- P19 is in `docs/phase-correctness/PARKED.md`, in the arbiter's words.
- `PHASE_OUTCOME.md` has UNIT 422 - STEP 6 from the decision block.
- `PHASE_STATUS.md` names unit 422 with CURRENT_STEP 6.
- The version went 1.13.108 to 1.13.109.

The entry round, one type per invocation:
- Build: clean with warnings as errors.
- Engine carry-forward: 178 of 178.
- App carry-forward: 277 of 278. The one lost to the dispatcher loop, `ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed`, passed alone (the type, 2 of 2).
- Floors: captures 51 of 51, adjudicated 13 of 13, clean 2 of 2, keyed floors 13 of 13.
- `BindingHealthTests`: 1 of 1.

**Verifying section 4 of the instruction against the tree.** Each claim, checked. I repaired nothing while checking.
1. True. `TransmitButton` is at line 4092, bound to `Transmit.PressCommand`. Clear is bound to `ComposeClearCommand`. CQ, RST and 73 are at 4112 to 4117. None of them carried `ToolTip.Tip`.
2. True. The comment above Send reads *THIS KEYS THE RADIO (HM-DEC-059)*, and the one above the macros reads *The macros fill the line; they do not send.*
3. True. `Classes="hm-connect"` is at 2988, its content is bound to `ConnectButtonText`, and it had no tip.
4. True, with one clarification. `BandRow` is at 3150 and `BandPills` at 3516. **The band buttons are built by the window itself**, in `BandPills`' own item template (`Button Classes="hm-band"`, line 3575), not by a control under `src/Hamlet.App/Controls`. They already carried a tip bound to `BandButtonViewModel.ActivityTooltip`.
5. True, but the button it names is in neither place. The `?` button at 2707 reads *What is this mode, and why bother?*, and it sits in the `widget.guide` template, not on the CW tab or the band row. **The circled question marks in those two places are `HintMarkControl`s** of kind Tip, which draw a circled `?`. There are 2 on the CW tab and 3 on the band row, plus 2 `⊣` marks on the CW tab and a `⊣` and a `#` on the band row. Each takes its hover text from its own sentence, and all of them had one.
6. True. The `☆` is named in text at 1050. **The star itself has no element**: `RigDisplayControl` (line 3212) draws it inside the LCD and runs `ToggleFavoriteCommand` when the star's rectangle is pressed. The face's only tip was *Roll the scroll wheel over any digit to tune that digit.*
7. True. *Have a look* is at 1915 to 1920, with its tip from `MainWindowViewModel.ReceiveHelpUnreachable`.
8. True. `AvaloniaFact` is used, and `BindingHealthTests` builds the main window.

**What "the CW tab" is.** It is `CwWorkspace`, the grid shown when `IsCwMode` is true: the send panel, and the CW terminal from `widget.terminal`. It holds 15 pressable controls:
- Send (`TransmitButton`), Clear, CQ, RST, 73 and the send line (a text box)
- the terminal panel's header, which is a toggle
- the terminal's Clear, *I hear a station*, *Have a look* and *No thanks*
- 2 circled `?` marks and 2 `⊣` marks

**What "the band row" is.** It is everything above the divider that is the same in every mode. That is three pieces:
- the strip at the right end of the header: *Stop the scan*, *STOP TRANSMITTING*, the port list and Connect
- `BandRow`: the neighborhood card, the sun map and the rig face
- `BandPills`: the seven band buttons

Together those hold 34 pressable controls:
- the neighborhood card: its header toggle, 3 marks, the neighborhood strip, the best-bet button, the upgrade prompt, each saved spot's chip and ✕ (2 saved spots in the fixture), and the 4 license and grid answers
- the rig face: the digits, the save star, the link-check `#` mark, the drive box and its `?`, the PSK31 power line, and its accept and decline buttons

**Which of them are only on screen in some states**, read off the `IsVisible` and `IsEnabled` bindings:
- **Needs the CW tab selected:** the whole CW tab (`IsCwMode`).
- **Needs a receive offer standing:** *Have a look* and *No thanks* (`HasReceiveOffer`).
- **Needs the decoder running:** *I hear a station* is always drawn but enabled only then (`IsDecoding`).
- **Needs a scan running:** *Stop the scan* (`Scan.IsScanning`).
- **Needs a call going out:** *STOP TRANSMITTING* (`AutoCall.IsCalling`).
- **Needs no radio connected:** the port list is enabled only then (`!IsConnected`).
- **Changes with the connection:** Connect reads Connect or Disconnect.
- **Needs a license class below the top one:** the upgrade prompt, when the prompt has words.
- **Needs a disagreement to settle:** *Use the FCC value* and *Keep mine* need a license mismatch; *Use the looked-up grid* and *Keep mine* need a grid mismatch.
- **Needs at least one saved spot:** the chips and their ✕.
- **Needs a best bet:** the best-bet button (`GreenZone.HasBestBet`).
- **Needs a connected radio:** the link-check mark, which has no sentence and is not drawn until the radio answers (`HasLinkCheck`).
- **Needs a PSK31 power offer standing:** the power line. Accept and decline are only inside its popup.
- **Needs the panel open:** everything inside a collapsible panel.

Only the upgrade prompt and the license answers depend on the license. The mode (CW or not) matters only for the CW tab itself.

**The transmit files.** The `7e209cb4` check covers eleven files under `src/Hamlet.RadioEngine/Cw`: `CwTransmitter`, `KeyerCwSender`, `TransmitChain`, `AutoCall`, `AutoCallAnswers`, `CwTransmitGuard`, `TransmissionWatch`, `TransmitReadiness`, `TransmitPrivileges`, `TransmitNotes` and `ICwSender`. **`MainWindow.axaml` is not among them.**

**Task 1, the inventory.** `WhatEveryControlSaysOnHoverTests` asserts nothing. It builds the window headless on the CW tab, finds every pressable control by walking the built window, and prints one line per control: place, name, command, tip as the window resolves it, what the command does, and whether an existing tip is true. It does this twice, not connected and then connected to the training radio through `ToggleConnectCommand`, the way `TheBarSaysWhatHamletCouldNotDoTests` connects. The star's tip is read with the headless pointer on the star's own target.

At entry:
- CW tab: 7 of 15 with a tip.
- Band row: 18 of 34 not connected, 19 of 34 connected.
- **One tip was false: the save star's.** With the pointer on the star, the only hover text is the face's *Roll the scroll wheel over any digit to tune that digit.*, and the star does not tune.
- The seven band buttons' tips were true and never said that pressing tunes.

**Task 2, the test.** `EveryControlSaysWhatItDoesTests` names all 49 controls one by one. The list is closed both ways: a named control that is missing fails, and so does a control present and not named. It has three cases:
- not connected
- connected, which also requires the connect tip to change with the words
- the star, which requires its hover to be its own and not the digits'

A mark that holds no sentence is exempt, because `HintMarkControl` measures to nothing then and cannot be hovered. This matters for exactly one: the link-check mark while not connected. The test was committed red. The failure is quoted in section 3.

**Task 3, the tips.** Every tip says only what the command body does:
- Send, the terminal's *No thanks*, the stops, the port list, the upgrade prompt, the four license and grid answers, the drive box and the three PSK31 power controls: an inline `ToolTip.Tip` beside the control, the form the file already uses.
- The connect button: `ConnectButtonTip` on the view model, which follows `ConnectButtonText`.
- The band buttons: `BandButtonViewModel.PressTip`, one sentence put in front of the existing text, which is kept word for word.
- The neighborhood strip and the `?` mark beside it: one constant, `NeighborhoodMapControl.HowToUseIt`. The mark's words are unchanged.
- The save star: `StarTip` on `RigDisplayControl`.
- The panel headers: a tip in `CollapsiblePanel.axaml`.

The test went green, 3 of 3. **Send's element before and after** (from `.run-unit/unit422-diff.sh`):

```
at bdd0070b                                          now
<Button Grid.Column="1" x:Name="TransmitButton"      <Button Grid.Column="1" x:Name="TransmitButton"
        Classes="hm-send"                                    Classes="hm-send"
        Content="Send" FontSize="12"                         Content="Send" FontSize="12"
        Padding="16,5" Margin="0,0,6,0"                      Padding="16,5" Margin="0,0,6,0"
        Command="{Binding Transmit.PressCommand}"            Command="{Binding Transmit.PressCommand}"
        CommandParameter="{Binding Transmit.OwnWords}" />    CommandParameter="{Binding Transmit.OwnWords}"
                                                             ToolTip.Tip="Keys the radio and sends the line below on the air, in Morse. If Hamlet asks you to read it over first, a second press sends it." />
```

Across the whole source diff, every removed line is a closing `/>` that moved down one line to make room for the tip, or one of the lines of code replaced in the two controls below. No command, binding, visibility, enablement or layout changed.

**Task 4, the exit round.**
- Build: `Hamlet.sln` with warnings as errors, non-incremental, 0 warnings.
- Engine carry-forward: 178 of 178.
- App carry-forward: 274 of 278. The four cases of `TheRecordNamesTheSubModePressedTests` were lost to the dispatcher loop and passed alone (the type, 12 of 12).
- Floors: captures 51 of 51, adjudicated 13 of 13, clean 2 of 2, keyed floors 13 of 13.
- Green: `BindingHealthTests`, `TheWindowHoldsBelowItsMinimumTests`, `Unit376TheTopBandTests`, and both of this unit's types.
- I also ran 20 types that read tooltips or build the touched controls, one per invocation. All green except two reds:
  - **`ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`**: red with the same message as at units 419 to 421 (P16), as the instruction expected.
  - **`Unit302CeilingHoldsStillTests.TheLiveReadoutsAreStillOnScreen`**: red because it looks for an element named `TurnRingCountText` on the Digital tab, and nothing under `src` has carried that name, at entry `bdd0070b` or now. I did not run this type at entry, so the entry state is inferred from the tree, not measured. It is not this unit's, and it is parked as P20.
- The transmit files print nothing against `7e209cb4`, and `src/Hamlet.RadioEngine` prints nothing against entry.
- 6.4 is ticked in `PHASE_PLAN.md`.

**Decisions I made myself, each overrulable:**
1. **Where the band row ends.** I took it as everything above the divider that stays the same in every mode: the header strip that holds Connect, `BandRow` and `BandPills`. I left out:
   - the menu bar
   - the CW, Digital and Voice tab buttons (the tab strip is neither the CW tab's contents nor the row)
   - the sun map, which takes no press
   - the front-end and filter chips, which have hover text and take no press
   - the transcript itself, which is selectable text
2. **The band buttons' tips were true, and I still added to them.** The instruction says to leave a true tip alone, and also that a band button says it tunes. Their text never said a press tunes, so I put that one sentence in front and kept everything after it word for word. For example: *Tunes to 7.028 MHz, where Morse gathers on 40 m, and the neighborhood strip redraws for the band. With a radio connected, the radio goes there too.*
3. **The neighborhood strip got hover text at rest.** Before, it showed a tip only with a dot under the pointer. It now reads the same sentence as its `?` mark, from one constant. `NeighborhoodMapControl` puts that sentence back when the pointer leaves a dot, so a dot's line no longer stays behind as the strip's tip. Pressing, dragging and the dots' own lines are unchanged.
4. **The star borrows the face's hover while the pointer is on it.** `RigDisplayControl` swaps in `StarTip` on the star's rectangle and restores the face's own tip when the pointer moves off or leaves.
5. **The panel-header tip is in the shared template**, so every collapsible panel in the application now says *Folds this panel away, or opens it again.*, not only the two counted here.
6. **The PSK31 accept button, which sets RF power, got a tip.** I read section 10's "a tip is an attribute, not a path" as covering it. Its element changed only by the added attribute.
7. **A circled mark holding no sentence is not failed**, because by its own design it is not drawn then.

**Stray file.** Midway through task 4 I ran `tools/status.sh` from `.run-unit`, and it wrote a stray `.run-unit/PROJECT_STATUS.md` with an empty WORK_INSTRUCTION field. `rm` is refused here, so it is left untracked and uncommitted. The root `PROJECT_STATUS.md` was rewritten from the root straight after, and it is the real one.

## 2. What the owner should expect

Every button on the CW tab and the band row now tells you what it does when you rest the pointer on it. The CW tab's Send button says it keys the radio: *Keys the radio and sends the line below on the air, in Morse.* Clear, CQ, RST and 73 each say they fill or empty the line and send nothing. One tip was already there and false: over the save star in the frequency display, the only hover text was *Roll the scroll wheel over any digit to tune that digit.* The star now says *Saves the frequency you are on as a favorite, and it appears in the row under the green zone. Press it again on a saved frequency to forget it.* The band cards keep their paragraph about the band, which now starts with where pressing takes you. The connect button's hover changes with its words. Two things may look new without being wrong. Resting on the neighborhood strip away from a dot now shows how to use it. Every panel header now says it folds the panel.

## 3. What you should see

**The inventory, before (entry `bdd0070b`) and after, not connected.** "Kept" means the tip was there at entry and is unchanged. Where "before" is *none*, the tip is new.

| Place | Control | Tip before | Tip after | What its command does |
|---|---|---|---|---|
| CW tab | Send | none | *Keys the radio and sends the line below on the air, in Morse. If Hamlet asks you to read it over first, a second press sends it.* | **keys the radio**: sends the line through `PressCommand`, the one transmit path; a line he changed is held for a second press |
| CW tab | Clear (send line) | none | *Empties the line below. Sends nothing.* | empties the send line |
| CW tab | CQ | none | *Fills the line with a CQ call in your callsign. Sends nothing: Send puts it on the air.* | fills the line with `CQ CQ DE <call> <call> K` |
| CW tab | RST | none | *Fills the line with a signal report, 599. Sends nothing: Send puts it on the air.* | fills the line with `RST 599 599` |
| CW tab | 73 | none | *Fills the line with a sign-off, 73. Sends nothing: Send puts it on the air.* | fills the line with `73 TU E E` |
| CW tab | `?` mark by the macros | kept | *tip — CQ tells the band you are looking for a conversation…* | none, hover only |
| CW tab | `?` mark in the terminal | kept | *tip — what the radio is hearing, as it arrives* | none, hover only |
| CW tab | the send line | none | *What Send puts on the air. Type here, or let CQ, RST and 73 fill it. Nothing goes out until you press Send.* | the text Send sends |
| CW tab | CW terminal header | none | *Folds this panel away, or opens it again.* | toggles `IsExpanded` |
| CW tab | Clear (terminal) | kept | *Wipes what is on screen. The decoder keeps listening, and keeps the speed and the noise floor it has worked out.* | clears the transcript only |
| CW tab | `⊣` mark by *I hear a station* | kept | *what Hamlet can see — Press this whenever you can hear a station…* | none, hover only |
| CW tab | I hear a station | kept | *Says you heard CW here, whether or not Hamlet read any of it…* | keeps 30 s of audio and adds a row to tonight's list |
| CW tab | Have a look | kept, left to 6.5 | *The panel this opens is not on any screen at the moment, so this cannot do anything…* | cannot run; `CanExecute` is false (HM-OPEN-087) |
| CW tab | No thanks | none | *Hides this offer for the rest of the session. Nothing on the radio changes.* | dismisses the offer for the session |
| CW tab | `⊣` mark under the transcript | kept | *what Hamlet can see — a dimmed character is one Hamlet is not sure of…* | none, hover only |
| band row | Connect / Disconnect | none | *Connects to the radio on the port chosen beside this, so Hamlet can read it and tune it.* / *Lets go of the radio. Hamlet stops reading it and stops tuning it.* | connects on the chosen port, or disconnects |
| band row | save star | the face's *Roll the scroll wheel…*, **false of the star** | *Saves the frequency you are on as a favorite, and it appears in the row under the green zone. Press it again on a saved frequency to forget it.* | saves the dial's frequency, or forgets it if already saved |
| band row | 80, 40, 30, 20, 17, 15, 10 m | kept, silent on the press | *Tunes to 3.530 / 7.028 / 10.103 / 14.030 / 18.080 / 21.030 / 28.030 MHz, where Morse gathers on <band>, and the neighborhood strip redraws for the band. With a radio connected, the radio goes there too.* then the entry text | `SelectBand`: selects the band and puts the dial on its CW spot; a connected radio is sent there |
| band row | `?` mark, how to use the strip | kept | *tip — hover a dot to see who it is · click a dot to tune there · click the background for a neighborhood's story · drag to tune* | none, hover only |
| band row | `?` MapLegendMark | kept | *tip — the map's colors: Morse, Digital, Voice…* | none, hover only |
| band row | `?` DigitalTransmitDriveTip | kept | *tip — This is a starting point, not a specification…* | none, hover only |
| band row | Stop the scan | none | *Stops the scan now, so Hamlet stops moving the dial.* | stops the scan |
| band row | STOP TRANSMITTING | none | *Stops the transmitter now. Escape does the same from anywhere in the window.* | stops the transmitter; Escape runs the same stop, in `MainWindow.axaml.cs` |
| band row | port list | none | *The port Hamlet talks to the radio on. Choose it before you connect; it cannot be changed while connected.* | chooses `SelectedPort` |
| band row | Neighborhood map header | none | *Folds this panel away, or opens it again.* | toggles `IsExpanded` |
| band row | `⊣` GreenZoneRuleOfThumbMark | kept | *what Hamlet can see — 20 m and up want daylight along the path; 40 m and down want dark.* | none, hover only |
| band row | neighborhood strip | none (only a dot's line, over a dot) | the `?` mark's own sentence, from one constant | a dot tunes there; the background opens a story; a drag tunes |
| band row | best-bet button | kept | *The band with the most going on right now… Pressing it tunes there.* | selects the top-ranked band |
| band row | upgrade prompt | none | *Shows what the next license class would let you do on this band. Press again to hide it.* | toggles the upgrade ladder |
| band row | saved-spot chips, and their ✕ | kept | *<name>. Click to tune.* / *Forget this spot* | tune to it / forget it |
| band row | Use the FCC value / Keep mine | none | *Sets your license class in Hamlet to the one the lookup found. Nothing on the radio changes.* / *Keeps the license class you set, and Hamlet stops asking.* | sets or keeps the class in settings |
| band row | Use the looked-up grid / Keep mine | none | *Sets your grid square in Hamlet to the one the lookup found. Nothing on the radio changes.* / *Keeps the grid square you typed, and Hamlet stops asking.* | sets or keeps the grid in settings |
| band row | the frequency digits | kept | *Roll the scroll wheel over any digit to tune that digit.* | the wheel tunes a digit |
| band row | `#` LinkCheckMark | none; not drawn while not connected | *measurement — Hamlet is keeping up with your radio…*, once connected | none, hover only |
| band row | drive box | none | *How hard Hamlet drives the sound card when it transmits, as a percent of full scale. Kept for next time.* | writes `TransmitDrivePeak` to settings |
| band row | PSK31 power line | none | *Opens the RF power offer. Opening it changes nothing on the radio.* | opens the popup; writes nothing |
| band row | PSK31 accept | none | *Sets the radio's RF power to the level offered above.* | writes RF power to the radio |
| band row | PSK31 decline | none | *Closes the offer. Nothing is written to the radio.* | writes nothing |

Counts, from the fact's own print:
- CW tab: 7 of 15 → 15 of 15.
- Band row, not connected: 18 of 34 → 33 of 34. The remaining one is the link-check mark, which has no sentence and is not drawn.
- Band row, connected: 19 of 34 → 34 of 34.

**The red message, at `d833bbaa`**, from `.run-unit/unit422-test64-red.txt`. The connected case listed the same 23.

```
not connected: 23 control(s) say nothing on hover or are not where the list says:
CW tab | TransmitButton | no tip | runs Transmit.PressCommand
CW tab | "Clear" (ComposeClearCommand) | no tip | runs ComposeClearCommand
CW tab | "CQ" | no tip | runs ComposeCqCommand
CW tab | "RST" | no tip | runs ComposeRstCommand
CW tab | "73" | no tip | runs ComposeSeventyThreeCommand
CW tab | send line | no tip | runs Transmit.OwnWords.Message (the text)
CW tab | header of CW terminal | no tip | runs IsExpanded (folds the panel)
CW tab | "No thanks" | no tip | runs DismissReceiveOfferCommand
band row | "Stop the scan" | no tip | runs Scan.StopCommand
band row | "STOP TRANSMITTING" | no tip | runs AutoCall.StopCommand
band row | port list | no tip | runs SelectedPort (the choice)
band row | connect button | no tip | runs ToggleConnectCommand
band row | header of Neighborhood map | no tip | runs IsExpanded (folds the panel)
band row | neighborhood strip | no tip | runs TuneToDotCommand, ShowNeighborhoodCommand
band row | Button with no words | no tip | runs ToggleUpgradeLadderCommand
band row | "Use the FCC value" | no tip | runs AcceptLookedUpClassCommand
band row | "Keep mine" (KeepMyLicenseClassCommand) | no tip | runs KeepMyLicenseClassCommand
band row | "Use the looked-up grid" | no tip | runs AcceptLookedUpGridCommand
band row | "Keep mine" (KeepMyGridCommand) | no tip | runs KeepMyGridCommand
band row | DigitalTransmitDriveBox | no tip | runs TransmitDrivePercent (the value)
band row | DigitalPsk31PowerLine | no tip | runs OpenPsk31PowerOfferCommand
band row | DigitalPsk31PowerAccept | no tip | runs AcceptPsk31PowerCommand
band row | DigitalPsk31PowerDecline | no tip | runs DeclinePsk31PowerCommand

with the pointer on the save star the face says "Roll the scroll wheel over any digit to tune that digit.", which is the digits' sentence and says nothing about saving
```

**The green run, at `21808260` and again in the exit round:** `EveryControlSaysWhatItDoesTests`, 3 of 3.

```
not connected: "Connects to the radio on the port chosen beside this, so Hamlet can read it and tune it."
connected    : "Lets go of the radio. Hamlet stops reading it and stops tuning it."
star  : "Saves the frequency you are on as a favorite, and it appears in the row under the green zone. Press it again on a saved frequency to forget it."
digits: "Roll the scroll wheel over any digit to tune that digit."
```

## 4. What's blocking us

Nothing here blocks 6.4, and nothing halts the phase (R65). The carried items are copied word for word from the work instruction (HM-DEC-139).

1. **P19**, logged in task 0 below: 3.6's added strays stand above the bar and every floor they sit under is at its count, so none can leave without the owner's ruling. *(Carried. "Task 0 below" refers to the work instruction; P19 is now in `PARKED.md`. The ruling is proposed there: a named character that the inferred key aligns as added, inside a scored stretch, may leave a floor, with every one printed per recording before and after. The owner's, because R63, 2.2 and R71 are his. Step 3 cannot move until it is ruled on.)*
2. **P12** stays parked: the `captured` and `broadcast` clock lines.
3. **The `clipping` and `inputFloor` question** stays parked.
4. **P14, P15, P16 and P18** stay parked. *(P16 was red again at exit, with the message unchanged.)*
5. **Unit 421's R71 count:** R71's prose says 40 unkeyed captures, and the tree has 29 unkeyed rows of 51. Recorded, not a ruling.
6. **Unit 421's `SpanMarginForRecord` remark** stays until a unit touching `CwCharacter` rewrites it with the trace beside it (12.6).
7. **New, P20, parked: a ceiling test looks for a countdown that is not in the tree.** `Unit302CeilingHoldsStillTests.TheLiveReadoutsAreStillOnScreen` looks for `TurnRingCountText` on the Digital tab, and no element under `src` carries that name, at entry or now. Proposed, the owner's: step 6 decides whether the slot countdown comes back to the Digital tab or the test retires with the readout. Reasoning: a test that names a readout Tim asked for should not be edited to pass, and the readout should not be rebuilt as a drive-by (§12.6). Rejected: repairing either side in this unit, which was hover text only. Not blocking.
8. **New, author's and overrulable: what "the band row" covers.** This unit counted the header strip that holds Connect, `BandRow` and `BandPills`: 34 controls. It left out the menu bar and the CW, Digital and Voice tab buttons. Reasoning: those are the parts above the divider that stay the same in every mode, and 6.4's own list (the band buttons, connect, the star) spans exactly them. Rejected: counting the tab strip too, because it belongs to neither place, and a tab button's hover would be a sentence nobody asked for. If the owner reads the band row more widely or more narrowly, the lists in `EveryControlSaysWhatItDoesTests` are where that goes. Not blocking.
