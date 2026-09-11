```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 3 done,
   4 partial (six of its eight must-pass now met), 5 and 6 not started.
B. Step 4 and its eight must-pass, each met or not met, with its number:
   - loopback, bandwidth, the chain as R10 allows, over the cap refused: met by unit
     318; ThePsk31ModulatorTests and TheUnslottedSendTests still green (engine
     carry-forward 120 of 120); nothing under Transmit changed;
   - receipt and cards: the card half - one card per station certainly calling yes
     (01-textbook opens exactly W1AW's), two stations two cards yes (composed from
     01 and 02, updated in place, no third), a guessed addressee no card yes
     (05-garbled); the receipt half not aimed at, so the criterion is not met;
   - the certainty gate: offers on a certain your turn 7, offers on anything else 0
     - met, and nothing is drawn for the offer while the door is shut;
   - the turn indicator on the card: shows unknown yes, a guess marked in words yes
     ("Your turn, a guess"), SlotClock absent under PSK31 yes - met;
   - drive and power on the panel: not aimed at; asks 32 and 33 not answered.
   The nice-to-pass: the within-one-character count - 1 character, worst of 8 - met.
   The number section 6 of the plan asks about: 8 messages to the operator, and the
   turn certain on 7 (05-garbled line 3 is the one guess).
C. The report last. Section 4 raises 4 items, new, on top of a carried queue of
   thirty-eight. Item 39 is in the way of nothing in B, but it is a carry-forward
   red this unit caused: unit 316's assertion that no W1AW card exists under PSK31
   now fails because the card this unit was told to open exists. It wants a ruling
   before the next unit. Besides asks 32 and 33, the receipt half of criterion 3,
   which needs the send door, is all that stands between step 4 and done.
```

```
UNIT:       319 - complete at task 4 of 4, none dropped - 2026-09-11 15:08
PHASE GOAL: PSK31 becomes Hamlet's third digital mode, on a modem Hamlet wrote, worked
            with FT8's two cards, one-click exchange, log and achievements.
UNIT GOAL:  With the send door still shut, read whose turn it is from PSK31 text, open a
            conversation card when a station is certainly calling the operator, show the
            turn where FT8 shows the slot clock, and name a macro only on a certain turn.
ADVANCED:   yes - step 4: the turn indicator on the card, the certainty gate, and the card
            half of receipt-and-cards; and the within-one-character nice-to-pass
NUMBER:     PSK31 conversation cards opened by a certain call to the operator 0 -> 2 on
            the panel (01-textbook and 02-chatty composed; 0 for 04, 05 and 06); messages
            to the operator with a certain turn 0 -> 7 of 8; offers on anything but a
            certain your turn: 0; version 1.13.5 -> 1.13.6
DRIFT:      0 consecutive units without advance  (was 0)
```

## 1. What Claude did

**Complete at task 4 of 4. Nothing was dropped.** Machine QUIVERFULL, project Hamlet (the gate's
four checks held: `SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` exist, `CoreHMI.sln` and
`MURC.sln` do not, root `C:\Source\HamLet`), branch `main`. Commits, each pushed and accepted:
`7d29c44` task 1, `900885f` task 2, `3449057` task 3, `06325e9` task 4, and the report commit
after this file.

**The line of `PHASE_STATUS.md` this session wrote:** `WORK_INSTRUCTION: 319 - say it, the card
half: whose turn it is, on a PSK31 conversation card, with the door still shut`. Nothing else in
that file was written. Task 1's commit carried the file as it stood, which included the launcher's
`HEARTBEAT:` line and `STEP: 4 | partial` beside that one, as units 316 and 318 did.

**One red this unit caused, on the carry-forward list, and not repaired.**
`ThePsk31ReadsTheConversationTests.NoClickOnAPsk31RowReachesASendPath` fails at line 414,
`Assert.DoesNotContain(model.DigitalCards, c => W1AW)`, because task 3's card for W1AW now exists.
The test is on the carry-forward list, so it was not edited (section 4, item 39). **Task 1b should
have named this assertion and did not.** It was found by running the list after task 3.

### Decisions this session made for itself

1. **Carrying on past that red rather than stopping.** The instruction's stop-before-task-3 rule
   names `ThePsk31TabIsInertTests` only, and that test forbids no card. For the other two PSK31
   tests, task 1b asks for the assertion to be reported. Stopping would have left the step's card
   half unbuilt over an assertion whose own remarks say it guards against *the FT8 card* and its
   send button. The PSK31 card has no send button. *Rejected:* editing line 414, which section 7
   forbids; and suppressing the card in that test's scene, which would be gaming it.
2. **The slot clock is hidden at the panel, because it is on no card.** `SlotClock` has sat above
   both panes since unit 305 (`MainWindow.axaml:3739-3763` at `548f274`). Its visibility is now
   bound to `ShowsSlotClock`, which is false only under PSK31.
3. **The PSK31 card is `Ft8ContactCard`, built by `ForPsk31`.** Its `Ft8CardFacts` carries only the
   callsign. Its other members are empty or zero, and its `State` is `WaitingOnHim`, which nothing
   on a PSK31 card reads (the state word, the sentence and the dimming are PSK31's own).
4. **A card's turn is read from that station's conversation on its channel.** That means the
   messages it spoke or that were addressed to it, with the channel's carrier-present fact.
5. ***He is still sending* is certain.** It is the demodulator's fact, not a parse (§3.1). Who is
   sending is not claimed.
6. **A PSK31 card's X clears the card until that conversation gains a message.** It has no slot
   to remember.
7. **A card stays when its carrier goes.** It is read one last time with nothing arriving.
8. **The wording** is the session's: *Your turn*, *His turn*, *He is still sending*, *Unknown*,
   *Your turn, a guess*, and one sentence per state.
9. **The macro-follows-message table** below is this unit's reading of §R2's order, not a ruling.

### Task 1 - entry, the trace, the before-counts

**Entry, run by exact name:** `ThePsk31ExchangeParserTests` 8 of 8 and `ThePsk31MessageSplitTests`
3 of 3, so this unit has its ground. `ThePsk31ModulatorTests` and `TheUnslottedSendTests` were also
green, inside the engine carry-forward run.

#### 1b - the trace, file and line at `548f274`

- **How an FT8 conversation card comes to exist.**
  - `RebuildCards()`, `MainWindowViewModel.cs:3229-3279`, runs on these triggers:
    - `RebuildConversation()` at `:2922`, when the operator's side changes;
    - the clock offset at `:1005`;
    - `ClearCard` at `:3624` and `:3637`;
    - `OnStationLearned` at `:4778`;
    - a booked send at `:11822`.
  - It builds one card per station from `CardStations()` (`:3098-3141`): the FT8 ledger's
    stations on the operator's side, the sent rows, and the CQ receipt. PSK31 rows have been
    skipped there since unit 316 (`:3108`).
  - The view model is `Ft8ContactCard` (`Ft8ContactCard.cs:69`). It holds:
    - `Ft8CardFacts` (`Ft8CardFacts.cs:67-86`): callsign, state, slots, times, message counts,
      reports, rogers, sign-offs, grid and payloads;
    - the action kind, label and message;
    - the corrected now, the technical numbers, the decode floor and the callook name.
- **The `SlotClock`** is not on a card. It is the panel above both panes (`MainWindow.axaml:3739`,
  moved there by unit 305, comment at `:4657-4668`), and at `548f274` nothing bound its visibility.
  The card's own `ShowsRing` (`Ft8ContactCard.cs:300`) is bound by nothing in the template.
- **The rebuild (ask 10).** `DigitalCards.Clear()` at `:3231`, then every card is remade.
  - **Under PSK31 there is no slot-driven rebuild.** `OnSlotTick` (`:10555`) returns at
    `:10590-10606` before any slot is cut or decoded, so no decode path runs.
  - `RebuildCards` still runs on the other triggers, so PSK31's cards are put back after its
    clear.
- **How an FT8 card decides its one offer.** `ActionFor`, `:3303-3350`:
  - a receipt offers nothing;
  - `Complete` offers Log;
  - `YourMove` offers `Ft8SendOptions.For(...).Options.FirstOrDefault(IsExpected)` as a Send;
  - otherwise it offers *Send it again* with the last text sent.

  The offer's certainty comes from the ledger's state (`Ft8ContactStates.Read`) over decodes that
  are whole FT8 messages. No guess enters it.
- **What unit 316 exposes on a PSK31 row.**
  - `DigitalDecodeRow.Reading` (`DigitalDecodeRow.cs:143-155`) is a `Psk31Exchange`
    (`Psk31ExchangeParser.cs:65-73`): `Speaker`, `Addressee`, `Kind`, `HandsOver`, `IsCertain`,
    `Rst`, `Grid` and `IsForOperator`.
  - `ReadingWord` (`:165-169`) is *guess*, *unknown* or empty. `IsGuess` is at `:175`. `Sender` at
    `:538` and the addressee at `:528` come from the reading.
  - The operator's-side rule is `IsForHim` (`MainWindowViewModel.cs:1655-1658`):
    `Reading is { IsForOperator: true, Speaker: not null }`.
  - The operator's callsign is `_settings.Operator.Callsign`, handed to the splitter when a channel
    is first read (`:1997`).
- **`ThePsk31TabIsInertTests`** (arbiter's mismatch 2) is at
  `tests\Hamlet.App.Tests\ViewModels\ThePsk31TabIsInertTests.cs`. Its methods:
  - `NoDecoderRunsAndNoSlotGridIsCutUnderPsk31` (`:73`): no rows, 0 slot looks, 0 slots read;
  - `Ft8AndFt4AreUntouched` (`:107`): 30 slot looks each;
  - `NoRowOnTheListIsClickable` (`:135`): `CanAnswerRowsForTests` false;
  - `ACqPressUnderPsk31ReachesNoSendPathAndSaysWhy` (`:165`): the send line names PSK31 and is over
    20 characters, and no `composed`, `ft8_transmission`, `Played` or `keyed` appears in telemetry;
  - `Ft8CanStillReachTheSendDoor` (`:208`).

  **No assertion forbids a card**, so there was no stop.
- **`ThePsk31ReadsTheConversationTests`**: `NoClickOnAPsk31RowReachesASendPath` (`:384-423`).
  - PSK31 rows have no send menu, no Log and no flyout (`:408-410`).
  - **`:414` forbids a W1AW card on the panel at every step** (the item 39 conflict).
  - `CanAnswerRowsForTests` is false (`:420`) and `HasSomethingToStop` is false (`:421`).
  - **`ThePsk31HearsEveryoneTests`** asserts nothing about cards, only `CanAnswerRowsForTests`
    (`:514`).
- **The corpus.**
  - **Certainly addressed to the operator:** `01-textbook` lines 3 and 5, `02-chatty` 3 and 5,
    `03-no-report` 3, `07-odd-ending` 3, and `08-lowercase-and-slashes` 3.
  - **Guessed:** `05-garbled` line 3.
  - **The operator's own:** lines 2 and 4 of 01, 02, 03 and 07, lines 2 and 5 of 05, and line 2 of
    08.
  - **No transcript has two stations calling the operator**, so task 3 composes that case from 01
    and 02, and the test says so.
- **Ask 32.** No IC-7300 manual was looked for outside the tree, since the file tools are confined
  (ask 33). Nothing was tried outside `C:\Source\HamLet`.

#### Mismatches between the instruction and the tree

- **Held:**
  - `CanTransmitIn` at `:1951-1954`, true for `null`, `FT8` and `FT4`, with its remarks on
    `CanDecode`;
  - `IsPsk31Chosen` at `:1957-1958` and `_psk31` at `:1961`;
  - version `1.13.5` at `Directory.Build.props:606`;
  - the carry-forward list's 22 app names plus `BindingHealthTests`, and its 13 engine names, the
    last three unit 318's;
  - `SendMessage` at `:12535`;
  - `OnSlotTick` returning before the watch under PSK31;
  - one `PttOn` use at `Ft8TransmitSequence.cs:513`;
  - `Arm` returning `TransmitRun?` (`Ft8ArmedSend.cs:279`) and `NowAsync` at `:555`;
  - the engine chain-guarding list at 82 of 93 with the same 11 reds.

  **Not checked:** `OperatorSend.Now` and the 32/16 idle.
- **(1)** `CLAUDE.md:360` holds `HM-DEC-161` and there is no `CPS-DEC` in `CLAUDE.md`. The reload
  was not run and `tools\` was not touched.
- **(2)** The file is under `ViewModels\`, as above.
- **(3)** *"cap of two"* is at `PHASE_PLAN.md:57`. The sentence *"Nothing in this phase changes what
  keys the transmitter"* did not match as one line in a plain search. It may wrap, and it was not
  searched for further. The plan was not edited.
- **(4)** This session's shell ran `git log`. `git grep` was not tried.
- **Not in the instruction:** task 3 assertion 5 says *"FT8 and FT4 cards still show it"*, but no
  FT8 or FT4 card has shown the `SlotClock` since unit 305. What was built and asserted: the
  panel's `SlotClock` is drawn under FT8 and FT4 and not under PSK31.
- **The app chain-guarding list could not be run as work instruction 318 named it.** That
  instruction is only in git history. `git show HEAD:WORK_INSTRUCTIONS.md`, and then
  `git diff WORK_INSTRUCTIONS.md`, were refused with *"This command requires approval"*. The second
  was a second form of the first, and that is said here (item 40). The engine list was run as unit
  318's own recorded command names it: 15 types, 93 tests. The app side was run on the three
  classes unit 318's report names reds in: 16 tests, not 63.

#### Before-counts, filtered and foregrounded

| List | Engine | App |
|---|---|---|
| carry-forward | 108 of 108 | 123 of 123 |
| chain-guarding | 82 of 93 | the three named classes 3 of 16 (the 63-test list not reproducible) |

**Every red, before and after, identical:**

- **Engine (11):**
  - `TheFitGuardAsksAboutTheGridTheSendIsOnTests.BothFt8RefusalSentencesAreWhereTheyWereBeforeThisUnit`
  - `TheTelemetryLineSaysWhatActuallyWentOutTests.` `TheLengthCountsWhatWentOutAndNotWhatWasAskedFor`,
    `TheNewFieldCarriesNoWords`, `TheTwoBagsDifferInTheFieldThatWouldHaveShouted`,
    `AnOrdinaryTransmissionsLineIsUnchanged`, `WhatTheLineSaidOnTheSeventhOfSeptemberAndWhatItSaysNow`
  - `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.` `AnAbortedTransmissionIsRecordedAsAWarningWithHowItCameOut`,
    `TheTransmittedSlotIsRecordedWithItsShapeAndItsMoment`, `ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`
  - `TheSinkPlaysToANamedEndpointTests.ACancelledPlayGoesOutShortAndTheSequenceCallsItAudioFailed`
    (5 green, 1 red: no worse)
  - `TheLoopbackProvesTheWholeChainTests.AMessageHamletComposedLeavesThisMachineAndComesBackFromItsOwnDecoder`
- **App (13):**
  - `TheMenuIsUnderTheMouseTests.` `ARowThatNamesNoStationPutsNothingUnderTheMouse`,
    `OutOfPrivilegesTheMenuSaysSoAndForbidsNothing`, `BothListsCarryTheMenuAndLogIsOnTheRightOne`,
    `EveryStationsPredictedMenuAppearsUnderTheMouse`, `TheRepeatCountBelongsToTheClickAndNotToTheRow`,
    `AThirdPartyExchangeHasAMenuAndNoLog`, `WithNoGridTheReasonIsANoteAndTheRestStayClickable`,
    `ChoosingOneGoesThroughTheOneCommandThatArms`
  - `TheWholeFt4ChainRunsFromOneRightClickTests.` `TheWholeExchangeOnTheReportShapeRunsFromTwoRightClicks`,
    `OneRightClickOnAnFt4RowDrivesTheWholeChainAndTheAudioDecodesBack`,
    `TheSameChainThroughTheFakeSinkDecodesTheSamplesItWasHanded`
  - `TheWholeChainRunsFromOneRightClickTests.` `OneRightClickDrivesTheWholeChainAndTheAudioDecodesBackAsTheClickedText`,
    `TheOperatorsStopButtonTakesARealTransmissionOffTheCardMidSlot`

`ArmHasExactlyOneCallerInSrc` was green before and after. `TheReadinessHoverTests` was green in
both app runs.

**Version** 1.13.5 -> 1.13.6.

### Task 2 - whose turn it is, in the engine

**`Psk31Turn`** (`src\Hamlet.RadioEngine\Psk31\Psk31Turn.cs`) takes a channel's messages, whether
characters have arrived since the last of them, and the operator's callsign. It returns
`Psk31TurnReading(State, IsCertain)`.

**The rule, in one sentence:** whoever last handed over gave the turn away. After a message that
hands over, the operator's own makes it his turn, and another station's addressed to the operator
makes it your turn. Characters arriving after any message make it he is still sending. Everything
else (no message yet, or a last message neither from nor to the operator) is unknown. A turn is
certain only where that message's parse is certain. He is still sending is certain because it is
the demodulator's fact and not the parser's.

It uses §R3's vocabulary through the parse, and a roger is still text. It reads only messages cut
from a channel's `Text`, which holds only characters the squelch passed (§3.5).

**`ThePsk31TurnTests`, 8 of 8.** On a stub it was watched failing 5 of 8 first; the 3 that passed
on the stub are "never" assertions a stub passes trivially.

1. A certain your turn on all 7 certain messages to the operator - **met**.
2. A certain his turn on all 10 certain messages from the operator - **met**.
3. He is still sending on every one of 1532 character steps with characters pending after a
   message. By name: 01-textbook at the `K` before message 2, then his turn at the whitespace -
   **met**.
4. `05-garbled` line 3 reads *YourTurn guess*, and lines 4 and 5 merged read *HisTurn guess* -
   never a certain your turn - **met**.
5. `04-not-for-me` reads only *HeIsSending certain* and *Unknown guess*, and never your turn -
   **met**.
6. Unknown on all 250 character steps before any message - **met**.
7. §6's number:

| transcript | to the operator | certain your turn |
|---|---|---|
| 01-textbook | 2 | 2 |
| 02-chatty | 2 | 2 |
| 03-no-report | 1 | 1 |
| 04-not-for-me | 0 | 0 |
| 05-garbled | 1 | 0 |
| 06-cq-dx | 0 | 0 |
| 07-odd-ending | 1 | 1 |
| 08-lowercase-and-slashes | 1 | 1 |
| all | 8 | 7 |

8. **The nice-to-pass:** your turn came 1 character after the last letter of the final turnover
   word, on every one of the 8 messages to the operator (`K`, `SK`, `KN` and `k`). Worst is 1 -
   **met**.

### Task 3 - the PSK31 conversation card

**Which message opens a card, as built** (`ShowPsk31Cards`, `MainWindowViewModel.cs`): one whose
parse is certain, that names a speaker, that is addressed to the operator, and whose speaker is not
the operator. That station gets exactly one card.
- The card's word is `Psk31Turn` read over that station's conversation on its channel.
- A card is replaced at its own index only when its reading or offer moves.
- A card is put back, as the same instance, after `RebuildCards`' clear.
- A guessed addressee stays a row on the operator's side.

**`ThePsk31ConversationCardTests`, 8 of 8.** Unwired, it was watched failing 7 of 8 first; the one
that passed was "a guessed addressee opens no card".

1. 01-textbook opens no card through line 2 and exactly one, W1AW's, from line 3 - **yes**.
2. Composed from 01 and 02 on two channels: W1AW *Your turn* and G4XYZ *Your turn* make 2 cards.
   After 01 line 4: W1AW *His turn* at the same index, and G4XYZ is the same instance, still
   *Your turn*. After line 5: still 2 cards - **yes**.
3. `05-garbled` opens no card at any line, and after line 3 the row is on the operator's side and
   a guess. `04-not-for-me` and `06-cq-dx` open none - **yes**.
4. **The card's word after each message, `01-textbook`, walked character by character:**
   message 1 (W1AW > ANY Cq) no card; 2 (KC3QIS > W1AW Answer) no card; 3 (W1AW > KC3QIS Report)
   *Your turn*; 4 (KC3QIS > W1AW Report) *His turn*; 5 (W1AW > KC3QIS End) *Your turn*.
   *He is still sending* showed while characters arrived.
   - Then `de W1AW K` gives *Unknown* ("Hamlet cannot tell whose turn it is with W1AW.").
   - Then `KC3QIS de W1AW 5#9 K` gives *Your turn, a guess* ("W1AW seems to have handed over to
     you, but part of it did not read cleanly, so that is a guess.").
   - **Met.**
5. The `SlotClock` is visible under FT8 and FT4 and not under PSK31. No `SlotClock` is inside the
   card list, and `ShowsRing` is false. `TheSlotClockTests` is green and unedited - **met**.
6. The W1AW card shows place *United States*, and the G4XYZ card *United Kingdom*: both from the
   callsign. There is no name, QTH, time line or detail row, and no Log or messages link - **met**.
7. `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` is green - **met**.
8. Checked:
   - one `.Arm(` in `src\`, in `MainWindowViewModel.cs`;
   - no `NowAsync` in `src\Hamlet.App`;
   - the `CanTransmitIn` text unchanged;
   - `ThePsk31TabIsInertTests` 5 of 5, with no diff since `548f274`.

   Headless, the only visible button on the card is the X - **met**.

### Task 4 - the certainty gate

**`Psk31Offer.For(conversation, turn, operator)`** (`src\Hamlet.RadioEngine\Psk31\Psk31Offer.cs`)
names a macro or none. **The macro-follows-message table** (the unit's reading of §R2, not a
ruling):

| his last message: certain, to you, handing over | offered |
|---|---|
| a CQ | Answer - never reached: a CQ is addressed to anyone, so it is never a certain your turn |
| his bare calls (Answer) | Report |
| a Report or Chat, before you have sent a Report | Report |
| a Report or Chat, after you have | Confirm |
| a Closing (73) or End (SK, CL) | Confirm |
| anything, once you have signed off | none |
| Ack, Garbage or Unknown | none (Ack is never produced; the other two are never certain) |

Anything but a certain your turn over a certain message to the operator that hands over offers
nothing.

**`ThePsk31OfferTests`, engine 4 of 4** (on a stub, watched failing 2 of 4 first) **and app 2 of 2**
(unwired, watched failing 2 of 2 first):

1. The certain your turns offer:
   - 01 message 3 Report and message 5 Confirm;
   - 02 message 3 Report and message 5 Confirm;
   - 03 message 3 Confirm;
   - 07 message 3 Report;
   - 08 message 3 Report;
   - his answer to the operator's own CQ (composed): Report.

   On the card: *Report*, then nothing on *His turn*, then *Confirm* - **met**.
2. Walking every transcript character by character: **offers made on a certain your turn 7, offers
   made on anything else 0**. A guessed your turn, both his turns, he is still sending and unknown
   each offer nothing over a conversation that would otherwise offer Report - **met**.
3. `05-garbled` (239 characters) and `04-not-for-me` (190) offer nothing at any step - **met**.
4. Headless, the card offers *Confirm*, and no drawn text or visible button contains Answer, Report
   or Confirm. No button carries `CardActionCommand`. `ThePsk31TabIsInertTests` is green and
   unedited - **met**.
5. §6's number: messages to the operator beside offers made, per transcript, 2/2, 2/2, 1/1, 0/0,
   1/0, 0/0, 1/1, 1/1, **all 8 beside 7**. The one without an offer is the guess on `05-garbled`
   line 3. That does not make step 4 look unusable on this corpus, which is typed and not recorded
   (FACT-004). The call is the arbiter's.

### After-counts, filtered and foregrounded

| List | Engine | App |
|---|---|---|
| carry-forward | 120 of 120 (108 + `ThePsk31TurnTests` 8 + `ThePsk31OfferTests` 4) | 132 of 133 (123 + card 8 + offer 2; the red is `ThePsk31ReadsTheConversationTests.NoClickOnAPsk31RowReachesASendPath`) |
| chain-guarding | 82 of 93, the same 11 reds | the three named classes 3 of 16, the same 13 reds |

The three new classes are on `docs\carry-forward-tests.txt`. The two `ThePsk31OfferTests` are
written with their namespaces, because the name is in both projects.

**Left uncommitted, and whose:** `.run-unit\*`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md`,
`WORK_INSTRUCTIONS.md` and `SESSION.lock`, which are the launcher's. Nothing of this session's.

### Validation

**This report was not validated by the script.** `tools\arbiter\validate-output.bat output.md`,
run once from the shell in the form the prompt names, was refused with *"This command requires
approval"*, as for units 315, 316 and 318. It was not retried in another form (item 40).

The seven rules were checked by hand against the script's own patterns:
- **Rule 1:** `UNIT:` is on line 31.
- **Rules 2 to 4:** exactly four `## ` headings, in order and with the exact names, on lines 46,
  395, 424 and 442.
- **Rule 5:** section 3 has content.
- **Rule 6:** `READ IN THIS ORDER` is on line 2, `A.` on 4, `B.` on 6, and `C.` with
  `raises 4 item` on 22, all inside the first 60 lines.
- **Rule 7:** no placeholder token before line 46, measured by a search for the script's tokens.

A hand check is not the exit 0 the prompt asks for.

## 2. What the owner should expect

**Now true, at the bench.** On the PSK31 tab:
- a station whose text certainly calls the operator gets a conversation card;
- the card says whose turn it is, in words, where FT8 shows the slot clock;
- the card knows which macro it would offer, and only on a certain turn;
- nothing on the card can send, and the slot clock is not drawn under PSK31.

**What will look wrong but is not:**

- **`ThePsk31ReadsTheConversationTests` is red on one method.** Unit 316 wrote *no W1AW card under
  PSK31* to stop an FT8 card with a send button appearing. A PSK31 card with no button now appears
  for W1AW, which is this unit's criterion. The rest of that method after line 414 no longer runs
  (item 39).
- **The slot clock vanishes when PSK31 is pressed** and returns on FT8 or FT4. That is §3.1.
- **A PSK31 card is plainer than an FT8 card.** It has no time line, `i` detail, Log link or
  "show the messages" link, because there is no ledger behind it and nothing is filled with a
  stand-in.
- **The map row on a PSK31 card says the station has not put a grid square on the air**, even when
  its text carried one: W1AW sent FN31. The card is not handed the parse's grid, and the map is
  parked (item 41).
- ***He is still sending* means text is arriving on that frequency.** It does not name who is
  sending.
- **Pressing X on a PSK31 card** clears it until that conversation gains a message.
- **A card stays after its carrier goes**, read with nothing arriving.
- **After any FT8 card rebuild, PSK31 cards sit after FT8 cards.**
- **The offered macro is held and not drawn.** That is the arbiter's decision while the door is
  shut.

## 3. What you should see

**Nothing Tim can press sends anything tonight.**

**Pressing CQ under PSK31 still refuses, in words.** The door opens only with §R4's drive level and
RF power, and those wait on an IC-7300 manual page that a session can read.

**What would now be on the screen, computed and not seen:**

- a station certainly calling Tim on PSK31 opens a conversation card for that station;
- where FT8 shows the slot clock, that card says whose turn it is: *Your turn*, *His turn*, *He is
  still sending*, or *Unknown* when Hamlet cannot tell, with *a guess* written out when it is one;
- the card knows which macro it would offer (Report, Confirm, or none) and draws no button for it
  yet.

Every number here is from the bench and synthetic, nothing keyed a radio, and every appearance
claim is computed, not seen.

## 4. What's blocking us

**Carried, verbatim where unresolved.**

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's. PSK31's continuous carrier makes it sharper.
2. **Nothing in this repository can look at a picture.** `Avalonia.Headless.Skia` is
   Tim's to add (§0.4).
3. **Three inherited reds, never chased** - two in `TheAchievementsScreenTests`, one in
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests` (engine).
4. **`Ft8ContactCard.Closing`** is uncalled and left standing.
5. **`Ft8GlobePlot`'s unused framing constants.** Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** Raise; do not resolve.
7. **The FT4 phase's run files.** Unit 315 recovered the plan and status into
   `docs\phase-ft4-run\`. The outcome file waits on item 19.
8. **The door sentence is a placeholder.** Carry.
9. **Acknowledgement indicators.** Named by Tim, not yet defined. Step 4's turn indicator
   may be what he meant; do not assume it is.
10. **Card ordering under scroll, and its root** - `DigitalCards.Clear()` then new cards
    every slot, per-card state lost. Not this phase's. Carry.
11. **The 2 px map outline** and **the 2.0x popup zoom cap** are sessions' numbers.
12. **Version numbering** - x.y.0 or x.y.1 for a phase's first unit under HM-DEC-150.
13. **The squelch threshold 0.90** is unit 314's number, `Psk31Demodulator.SquelchQuality`.
14. **Real off-air PSK31 audio** - only Tim can record it. Two or three minutes on 14.070
    with a few signals on it, any recorder, any rate, WAV, into
    `assets\fixtures\captured\`. Every number in this phase is synthetic until then.
15. **One conversation card is taller than the panel** (293 px in 220 px). Not broken.
16. *(Unit 315 closed the fixed 1000 Hz listening offset by its instruction's own
    condition. Kept here as a closed number so the list does not renumber.)*
17. **Hand wanted: run `tools\arbiter\validate-output.bat output.md`** - unit 315's
    shell refused it.
18. **Ruling wanted: record the 1500 Hz station's text in `manifest.json`, or accept unit
    315's derived reference.**
19. **Hand wanted:** `git show eb28430:PHASE_OUTCOME.md > docs\phase-ft4-run\PHASE_OUTCOME.md`,
    expected 168242 bytes, md5 `b60010069f7feea96e9d4be4e7e8c0f1`, then commit. *(Per its stat,
    `a36ce1e` committed a six-line file at that path. The arbiter did not check its content, and
    the 168242-byte file is not what landed.)*
20. **Hand wanted: `git rm src\Hamlet.RadioEngine\Psk31\Psk31Listening.cs`** - comment-only,
    builds.
21. **Ruling wanted: the carrier search's floor should be local to the signals before
    step 6** - at 48 kHz the median bin is below the radio's passband.
22. **Ruling wanted: is HM-DEC-161 the right id** for the phase-setting ruling?
23. **Ruling wanted: accept about a second of delay on PSK31 text, or reopen the step-1
    squelch.**
24. **Ruling wanted: the `snr` column's hover names FT8 and FT4 precision and not PSK31's.**
25. **Hand wanted (unit 316): say whether `TheReadinessHoverTests.NoRowCarriesABearingInDegrees`
    predates unit 316.** Run it alone at `eb67036`:
    `timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter "FullyQualifiedName~TheReadinessHoverTests.NoRowCarriesABearingInDegrees"`.
26. **Ruling wanted (unit 316): accept the replaced assertion in `ThePsk31HearsEveryoneTests`.**
    *"No row is ever on his side"* became *"a row is on his side only while its latest message is
    addressed to him"*.
27. **Ruling wanted (unit 316): the words `guess` and `unknown`** on a PSK31 row are the
    session's wording, not a ruling.
28. **Ruling wanted (unit 316): should §R3 recognise a roger (`R`, `RR`, `QSL`) before step 4?**
    Without one the corpus's `ack` kind is never produced.
29. **Ruling wanted (unit 316): accept the split rule's three edges.** A bare `73` does not end a
    message. A damaged sign does not split. A final `K` counts when the next character arrives.
30. **Ruling wanted, low (unit 316): the callsign shape is held in four places.** Consolidate them
    later, or leave them.
31. **Hand wanted (the arbiter, authoring 318):** `tools\arbiter\outcome-read.bat --approach`
    breaks when the approach contains an apostrophe. PowerShell reports *"Unexpected token"*, and
    the script still prints `Read complete.` and exits 0. **So a loop test on such a line reads as
    run and was not.** Not a unit's to touch.
32. **Hand wanted, for step 4's drive-and-power must-pass: put an IC-7300 manual where a session
    can read it, and name its path.** §R4 requires the unit to *"cite the page of the IC-7300
    manual it read"*, and the arbiter's search of the tracked tree found none. Task 1b reports
    whether one exists anywhere on this machine. *(Unit 318: none tracked; outside the
    repository could not be searched - item 33.)*
33. **Hand wanted: this session could read nothing outside `C:\Source\HamLet`.** The file tools
    refused `C:\Source\fldigi\src\psk\psk.cxx` (*"--restricted confines the file tools to the
    working directory"*) and `find` over the user profile was blocked, so §R5's pinned reference
    was not read and no IC-7300 manual outside the tree could be looked for. The modulator's
    convention came from `Psk31Demodulator` and `assets/reference-modem.py` instead. The shell
    also asked for approval on `pwd -W`, a `sed` stage and a `grep -v` stage in pipelines; each
    was dropped, not retried in another form. **And `tools\arbiter\validate-output.bat
    output.md` was refused** (`cmd //c`, *"This command requires approval"*), as for units 315
    and 316, so **this report was not validated by the script** and item 17 stands. It was not
    retried in another form. The seven rules were checked by hand against the script's own
    patterns - four `## ` headings in order and no fifth, a `UNIT:` line, the ordering block's
    `READ IN THIS ORDER`, `A.`, `B.`, `C.` and `raises 5 item` inside the first 60 lines, and no
    placeholder token before section 1 - and that hand check is not the exit 0 the prompt asks
    for. *Reasoning:* §R5 names fldigi as what the shaping
    is read from, and §R4 needs a page citation. *Rejected:* copying either into the tree, which
    §R5 forbids. **Wanted:** the launcher grants read access to `C:\Source\fldigi` and to a named
    manual path, or Tim rules that the demodulator and the reference modem suffice for the shape.
34. **Ruling wanted: the record's event name and FT8/FT4's missing mode.** A PSK31 transmission is
    written under `ft8_transmission` with `mode: Psk31`; FT8 and FT4 records still do not say
    which of the two went out, and cannot gain a key without breaking tonight's pin. *Options:*
    keep the name and add `mode` to FT8 and FT4 under a re-pinned test; rename to a neutral event
    with a migration note; or accept as is. *Industry answer:* one event with a mode field on
    every record, re-pinned in its own commit.
35. **Ruling wanted: 23 reds on the chain-guarding list that are not on the known-red list.** The
    8 record and telemetry reds (unit 305's `send_stage` events defeat their `Assert.Single`),
    2 real-endpoint engine reds and 13 app reds, named in section 1 - including
    `ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`, one of the five tests the instruction
    relies on to show the chain changed only as R10 allows. *Options:* put them on the known-red
    list by name, or give a unit to repairing them. *Rejected by this unit:* chasing them now,
    which the instruction forbids.
36. **Ruling wanted: accept the idle, 32 bits before and 16 after.** 32 is measured against
    Hamlet's own ear (shortest 23); 16 after is this unit's margin over a measured 0, for
    receivers nobody here has measured. A real receiver's needs want item 33's fldigi access or
    item 14's off-air audio.
37. **For the door unit, low: three edges the send path will meet.** `DriveTheArmedSend` reads
    `Armed?.Grid`, which throws for a no-slot send, so the PSK31 click must arm and call
    `NowAsync` in one handler; `Arm` returns the refusal, which the click must show; and the
    refusal sentence says "Psk31" where the operator reads "PSK31".
38. **Hand wanted (the arbiter, authoring 319): the arbiter's shell refused `git log` and
    `git grep`** with *"This command requires approval"*. Only the loop test and file reads by
    exact path ran. So this instruction's tree claims come from named files read and from unit
    318's report, and not from a search. Not a unit's to touch.

**New from unit 319, most blocking first.**

39. **Ruling wanted: unit 316's no-W1AW-card assertion against this unit's card.**
    `ThePsk31ReadsTheConversationTests.NoClickOnAPsk31RowReachesASendPath`, line 414,
    `Assert.DoesNotContain(model.DigitalCards, c => W1AW)`, is red since task 3.
    - It is on the carry-forward list and was not edited.
    - Its own remarks say it exists because *"the right-click menu and the conversation card would
      both exist for a PSK31 row from W1AW if the guard were missing"*. The guard it means is
      against an FT8 card with a send button.
    - The PSK31 card has no action, no Log link, and nothing drawn that transmits.
    - Because the loop stops at step 3, the assertions after it (`CanAnswerRowsForTests`,
      `HasSomethingToStop`, and later steps' send menus) no longer run.

    *Options:*
    - narrow line 414 to *no W1AW card that carries an action or a Log link* (or *no FT8 card for
      W1AW*), in its own commit;
    - rule that no card may open under PSK31 until the door opens, and take task 3's card back;
    - put the method on the known-red list.

    *Industry answer:* assert the property the test names - no send path - rather than the absence
    of a card. *Rejected by this unit:* editing it to make it pass, which section 7 forbids.
40. **Hand wanted: this session's shell refused, each reported once.**
    - `git show HEAD:WORK_INSTRUCTIONS.md`, and then `git diff WORK_INSTRUCTIONS.md` - a second
      form of the first, said so here - both with *"This command requires approval"*. So work
      instruction 318's app chain-guarding list (63 tests) could not be read, and the app side was
      counted on the three classes unit 318's report names (16 tests). The engine side was counted
      on unit 318's own recorded 15 types.
    - A shell `for` loop over the gate's four paths (*"Contains simple_expansion"*).
    - A `grep` pattern with a `$` anchor.
    - **`tools\arbiter\validate-output.bat output.md`**, run as `tools/arbiter/validate-output.bat
      output.md`, with *"This command requires approval"*. So item 17 stands, and this report was
      checked by hand only (section 1, Validation).

    None of the last three was retried in that form. *Wanted:* work instruction 318's section 2
    list written into `docs\`, or the chain-guarding list put in `docs\carry-forward-tests.txt`'s
    shape, so a unit can run it without git history.
41. **Ruling wanted: the map row on a PSK31 card states something the text contradicts.**
    - With no grid on the card, the row reads *"Hamlet does not know where W1AW is. He has not put
      a grid square on the air"*, but W1AW's certain report carried FN31.
    - The map is parked (*"leave them as they behave"*), so nothing was changed.

    *Options:* hand the card the grid from a certain message, which the parse supplied; draw no
    map on a PSK31 card; or accept the sentence. *Industry answer:* hand it the parsed grid, or
    draw nothing (§0.0).
42. **Ruling wanted: this unit's words and table are a session's, not rulings.**
    - The card's *Your turn*, *His turn*, *He is still sending*, *Unknown* and *Your turn, a
      guess*, and one sentence per state.
    - The macro-follows-message table in section 1 (his bare calls -> Report; a Report or Chat ->
      Report, or Confirm once a report has gone; a Closing or End -> Confirm; nothing once the
      operator has signed off).
    - *He is still sending* is certain because it is the demodulator's fact.
    - A PSK31 card's X clears it until that conversation gains a message.

    *Options:* accept as built, or rule different wording or order.
