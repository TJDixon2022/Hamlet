READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial with 0.2 to 0.5 now
   ticked and 0.1 a ruled negative, steps 1, 3, 6 and 8 done, step 2 partial on 2.4
   and 2.6 which are parked for the end-of-phase record unit, step 4 partial on 4.3
   which is cut down and logged to the owner, step 7 partial on an unjudged 7.2,
   step 5 Tim's own. This is the first unit ever spent on step 9.
B. Step 9 - the contact Tim had. 9.1 met: an X on every card and every receipt, which
   was already his ruling of 2026-09-08 and is asserted here, with the card's leaving
   now written through unit 380's NoteCardOnScreen on the press as well as in the
   reconcile; the press's token is the existing card_cleared and not a second
   spelling. 9.2 met: the hold reads the row's own liveness AND his hand-back, 3 of
   the four controls held with one sentence and the fourth already withheld by R1,
   the refusal his_carrier_live written at MainWindowViewModel's mode gate above the
   only composer, the release proved three ways - hand-back, carrier drop and empty
   band - and Tim's press at 17:45:40 would have been held. 9.3 met: Log on a card
   from the moment it exists where it waited for the exchange to finish, RST
   defaulted from the mode's own parser, 5 fields left absent rather than guessed.
   9.4 met: 2 of 2 parsed hand-backs moved the turn where 2 already did, and the
   17:48:33 replay reads "Your turn, a guess". 9.5 met: 3 of the four proved on
   Olivia by replay, the fourth by identity of the type and the method.
C. The report last. Section 4 raises 8 items on top of the carried thirty-nine, and
   none of them is in the way of a criterion in B. Task 1 item 2's measurement
   agreed with section 6 ruling 1 item 1 - the carrier is the fact and HeIsSending
   was never once true while he was on the air - but a second measurement, of how
   long a row outlives the man, disagreed with holding on the carrier alone, and the
   measurement won: item 1.

UNIT:       385 - complete at task 6 of 6 - 2026-09-22 00:36
PHASE GOAL: Hamlet keeps what it already has working, hardened where the screen, the record or the tests can be made honest without the radio and without the owner.
UNIT GOAL:  The four things Tim watched go wrong in one four-minute PSK31 contact are made right on the conversation card: an X on every card, a station who is still sending holding the send controls, Log from the moment there is somebody to log, and a turn that moves on every hand-back.
ADVANCED:   step 9, criteria 9.1, 9.2, 9.3, 9.4 and 9.5
NUMBER:     sends attempted on top of a station mid-over, before and after: 1 of 1
            went out -> 0 of 1; and conversation cards offering Log at the moment
            they appear: 0 of 1 -> 1 of 1
DRIFT:      none

## 1. What Claude did

**Surface and gate.** Claude Code on the development machine, branch `main`. The prompt claimed
`PROJECT: Hamlet`; the tree confirmed it: `PROJECT_CARD.md` says `PROJECT: Hamlet`, `Hamlet.sln`,
`SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` exist, `CoreHMI.sln`
and `MURC.sln` do not, root `C:\Source\HamLet`. **No port was opened, nothing was enumerated and
nothing was keyed; nothing here is evidence about the radio** (FACT-004).

**Commits on `main`, pushed at the end:**

| Commit | What |
| --- | --- |
| `813afd8a` | task 0 - the record, 1.13.71 -> 1.13.72, the entry runs |
| `37e8dad7` | task 1 - `Unit385Trace`, the before |
| `6d192b98` | task 2 (c) - the gate, `his_carrier_live`, and its five tests |
| `3659f03a` | task 2 (a)(b) - the row's word, the card's word, the held controls |
| `d86d4ccd` | task 2 corrected, with 9.1's on-screen line - the hold is the over, not the tail |
| `91fe9b9f` | task 3 - 9.3, Log from the moment the card exists |
| `b69319f8` | §R12 rewrites, four tests |
| `45ba1ea9` | task 4 - 9.4 asserted on PSK31 and Olivia |
| `ad3b70ab` | task 5 - 9.5 and the carry-forward names |

### Task 0 - the record and the entry run

Version **1.13.71 -> 1.13.72**; `PHASE_STATUS.md` moved from step 2 and unit 384 to **step 9 and
unit 385**; `PHASE_PLAN.md` **0.2, 0.3, 0.4 and 0.5 ticked**, citing the judging session's own
words at `PHASE_OUTCOME.md:41` (0.1 left unticked, step 0 still `partial`); the `UNIT 385 - STEP 9`
entry appended with **the correction about unit 384** - its run ended on an API 529 after task 0,
it never wrote `output.md`, and its `## UNIT 7 - STEP 2` row was graded from unit 383's leftover
report. **The false row was not edited.**

**The entry runs:**

| Invocation | Result |
| --- | --- |
| engine | **150 of 150** |
| app, attempt 1 | 225 of 226 - one dispatcher loop at 1 ms in `TheStopIsAlwaysOnScreenTests`, before any assertion |
| app, attempt 2 | **226 of 226** |
| the ten named types | **108 of 109** - the one red is the inherited `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn` |

### Task 1 - the trace, and it changed what tasks 2 and 4 did

`Unit385Trace`, one `[Fact]`, asserting nothing. **The owner's `2026-09-21.jsonl` is not on this
machine**, so every fixture is constructed to R44's stated timings with a test callsign and is
never called his record. What it measured is in section 3, and two readings decided the unit:

- **The carrier and the parser disagree, and the carrier is the fact** - `HeIsSending` was never
  once true at any moment his carrier was up, exactly as ruling 1 item 1 says.
- **Both parsed hand-backs already moved the card**, so 9.4 was already true as written; **what
  Tim actually hit is a garbled over that completes no message at all** (section 4 item 2).

### Task 2 - 9.2, the carrier holds the buttons

Built in the ruling's order: **the refusal first**, then the display, then the greying.

- **The gate**: a send addressed to a station who is still sending is refused at
  `MainWindowViewModel`'s mode gate, **above the only composer**, writing `send_refused` with
  `reason his_carrier_live` through `AppEvents.OperatorAction` and `Psk31Events.SendRefused` -
  **no new event type, no new category, no new stage** (R13). It refuses; it starts, delays and
  alters nothing.
- **The display**: the row says `sending` beside the word it already says when he stops; the card
  carries the same fact, handed in from the row.
- **The hold**: the offered button greys with the sentence beside it, the typed line keeps his
  words and greys its Send, and the canned menu - which greys nothing by its own ruling of
  2026-09-06 - becomes one note saying why. **One sentence, `he is still sending`, in all three
  places and in the record.**
- **The release**, proved three ways: his hand-back, his carrier dropping, and an empty band.

**And the measurement corrected the ruling.** Holding on `row.Ended` alone would have refused every
answer for **9 s** after a PSK31 hand-back (`Psk31Listener.RetiredWithinSeconds`) and for
**22.94 s to 38.23 s** after an Olivia one (`OliviaTiming.RetireAfterCharacters`, 56 characters) -
a gate that sticks, which ruling 1 item 4 calls worse than the fault it fixes. **So the hold is
the over, not the tail**: he is still sending while characters are arriving, or while his carrier
is up and the last thing he completed handed nothing back. Section 4 item 1.

### Task 3 - 9.3 and 9.1

- **9.3**: `ShowsLogLink` on a PSK31 or Olivia card stops being `_psk31Complete`. The receipt and
  call-to-anyone guard is untouched (Tim, 2026-09-11; §R8). **Nothing was rebuilt**: the log entry
  already takes its report from the mode's own parser and leaves every unobserved field null, and
  that is now asserted (R14).
- **9.1**: the X has been on every card and every receipt since Tim's ruling of 2026-09-08, bound
  to `ClearCardCommand`, which already writes the press. **What was missing is the other half**:
  the PSK31 branch removed the card and returned before the reconcile, so unit 380's
  `NoteCardOnScreen` never recorded it. The press now notes it with `OnScreenBy.Dismissed`.

### Task 4 - 9.4, asserted rather than rebuilt

Both hand-backs move the card, the second marked `Your turn, a guess` in the tree's own words, on
PSK31 and on Olivia. **Moving the card is not offering a send**: a guessed turn still never offers
the Confirm.

### Task 5 - 9.5, the list and the exit run

The hold and Log driven down an Olivia channel, and the card, the row and the reading proved to be
**the same type and the same method** PSK31 uses. Four types added to the carry-forward list with
the reason each is permanent.

| Invocation | Entry | Exit |
| --- | --- | --- |
| engine | 150 of 150 | **150 of 150** |
| app | 226 of 226 | **245 of 245** (+19, this unit's four types) |

**No regression** (HM-DEC-165): every name green before is green after.

**§R12 rewrites, four**, each after the change it is about: `ThePsk31ConversationCardTests`'s three
(a card with no station facts now offers Log; a card is rebuilt when its own liveness moves; the
Log button is drawn and transmits nothing) and `ThePsk31ReadsTheConversationTests`'s canned-seven
walk (the seven where he is not sending, one note naming the sentence where he is).

**Recorded under §12.1: nothing.**

## 2. What the owner should expect

**Hamlet will not let you transmit while the other man is still sending, and it says so.** While
his over is running his row says `sending`, his card says `he is still sending`, the offered
button and the typed line grey with that sentence beside them, and the seven canned lines become
one note saying the same thing - and if a press gets through anyway it is refused at the one door
that composes, with the reason in the record. **The moment he hands back with `K` or `BTU`, or his
carrier drops, everything goes live again**: the hold is a reading and never a latch, and a press
on an empty band is never refused. **Log is on the card from the moment there is somebody to log**,
rather than only when the exchange finished in the exact shape Hamlet expects, and it writes only
what actually passed - his report because he sent it, and nothing where nothing was observed.
**The card follows the conversation past its first turn**, saying `Your turn, a guess` where the
reading was not clean. **Which halves were already true**: the X has been on every card since your
ruling of 2026-09-08, the turn already moved on both *parsed* hand-backs, and the log entry already
took its report from the parser - those are asserted now rather than rebuilt. What is new is the
hold, Log arriving early, and the card's leaving being written down.

Every claim here is computed, not seen (FACT-004): no port was opened, nothing was enumerated, and
nothing was keyed.

## 3. What you should see

**The before, from `Unit385Trace`.**

*The two candidate facts, over one station's whole visit:*

| Moment | `row.Ended` | Turn word | State |
| --- | --- | --- | --- |
| carrier up, nothing read | False | (no card) | (no card) |
| mid-over, partial text | False | (no card) | (no card) |
| his first over complete | False | Your turn | YourTurn |
| pause, carrier still up | False | Your turn | YourTurn |
| his second over | False | Your turn, a guess | YourTurn |
| carrier gone | True | Your turn, a guess | YourTurn |

**`HeIsSending` is not on that table anywhere**: it was never true while his carrier was up, which
is what ruling 1 item 1 predicted.

*The four controls while his carrier was up, before:*

| Control | Before |
| --- | --- |
| the offered macro | `Report`, `ActionKind Send`, label *Tell him how he is coming through* |
| the press | **keyed once** - 0 keyings before, 1 after |
| the typed line | `CanType True`, no note |
| the canned lines | 7 offered |
| Log on his row | `CanLogRow False` |

*Log, before:* `ShowsLogLink False` on an unfinished card, while `ContactLogEntryForStation`
already returned `Call W1ABC, Mode PSK/PSK31, Band 20m, RstReceived 599`, with `RstSent`,
`GridSquare`, `ReportSent`, `ReportReceived` and `Comment` **null**.

*The two hand-backs, before:* parse 1 `Report, certain True, handsOver True`; parse 2 `Report,
certain False, handsOver True`; the card read `Your turn`, then `His turn` after Hamlet answered,
then `Your turn, a guess`. **Both moved it.** And the garbled over: **0 messages completed**
against **1** for the same words with clean callsigns.

**The after.**

| Criterion | After |
| --- | --- |
| 9.2, mid-over | typed send held, canned = 1 note, card note `he is still sending`, row `sending`, **0 keyings** |
| 9.2, released | at his hand-back with his carrier still up, and at his carrier dropping: **1 keying**, one refusal written in total |
| 9.2, empty band | a CQ goes out, **no refusal written** |
| 9.3 | `ShowsLogLink True` on an unfinished card; `False` on a receipt |
| 9.4 | `Your turn` → `His turn` → `Your turn, a guess` |
| 9.5 | the same on an Olivia channel; card type `Ft8ContactCard` and row type `DigitalDecodeRow` on both |

**The refusal, verbatim out of the written record:**

```
psk31_send_refused {"reason":"his_carrier_live","macro":null,"stage":"gate"}
operator_action    {"action":"send_refused","mode":"Digital","detail":"his_carrier_live"}
on_screen          {"kind":"card","state":"removed","by":"dismissed","count":1,"subMode":"PSK31"}
```

**And on the screen:** *Hamlet did not send that: he is still sending. Transmitting now would put
your signal on top of his, and neither of you would be readable. It goes out the moment his carrier
drops.*

## 4. What's blocking us

**Nothing blocks step 9.** Eight items, findings unless marked; none is in the way of a criterion.

### Raised by this unit

**1. The measurement disagreed with holding on the carrier alone, and the measurement won.**

*A finding, as section 6 says.* Ruling 1 item 1's choice of fact was right - `HeIsSending` was
never true while a carrier was up. But a row outlives the man: **9 s** on PSK31
(`Psk31Listener.RetiredWithinSeconds`) and **22.94 s to 38.23 s** on Olivia
(`OliviaTiming.RetireAfterCharacters`, 56 characters). A hold on `row.Ended` alone refused the
operator's answer for that long after the other man said `K` - **the gate that sticks ruling 1
item 4 forbids**, and it turned 25 names red. The hold is now *carrier up **and** nothing handed
back*, which released 21 of those 25 immediately; the other four were §R12 rewrites.

**2. The second hand-back Tim watched is not a parsed line at all, and its site is in the engine.**

*A finding that wants a licence, not a ruling.* `Psk31MessageSplitter` completes a message on a
turnover word that follows **clean** callsigns (`FollowsASign`), so a garbled over completes
nothing, its text stays pending, and the card sits at *He is still sending*. Measured: **0 messages
from the garbled over against 1 from the same words with clean calls.** 9.4 as written - *every
parsed line ... moves the turn* - is met, and this is the case underneath it. **Nothing was
repaired**: the site is `src\Hamlet.RadioEngine\Psk31\`, which section 6 forbids this unit to open.

**3. 9.1's token is `card_cleared`, not `card_dismissed`.**

*A finding.* Ruling 2 item 3 names `card_dismissed` for the press. `ClearCardCommand` has written
`card_cleared` through `AppEvents.OperatorAction` since unit 305, and R13 forbids a second writer
where one exists - a second spelling of one press is exactly what it forbids. **The token was left
as it is** and the card's *leaving* is recorded with `dismissed`, which is unit 380's own word.

**4. Section 5 is out of date about what a guessed turn offers.**

*A mismatch, reported.* It cites `Psk31Offer.cs:36` for *a guessed your turn ... offers nothing*.
Since unit 371 a guessed hand-back to the operator offers the **Report** - which asserts nothing
about the contact - and never the **Confirm**. The tests assert the tree.

**5. The owner's telemetry for 2026-09-21 is not on this machine.**

*A finding about evidence.* `%AppData%\Hamlet\telemetry\2026-09-21.jsonl` does not exist here, so
every fixture was **constructed to R44's stated timings** with a test callsign, and no replay in
this unit is described as his record.

**6. The `## UNIT 7 - STEP 2` row was appended to, never edited.**

*As instructed.* The correction is in this unit's own entry: unit 384's run ended on an API 529
after task 0, it wrote no `output.md`, and that row was graded from unit 383's leftover report.

**7. One commit carries two pieces.**

*A finding about this unit's shape.* 9.1's `NoteCardOnScreen` line lives in the same file as the
hold's narrowing and went in with it (`d86d4ccd`) rather than in a commit of its own.

**8. The `RULES_AT` id split, for the seventh unit running.**

*Reported, not repaired.* `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)`; `CLAUDE.md` section
1 holds `CPS-DEC-0165`. `tools\` is not this unit's.

### Asks still outstanding - carried from unit 383's section 4, per HM-DEC-139, verbatim

**The queue stands at thirty-nine and this unit answers none of them.** The words below are unit
383's, from its line under `## 4. What's blocking us` to its end, as committed in `6c6232cc^` -
the report that commit deleted from the working tree. **Work instruction 384's own section 3 was
deleted with it and is not recoverable, so nothing is recorded as answered by unit 384.**

On the three that this unit's work touched: **unit 383's item 1** (4.3 and the radio sheet) - no
task here opened `docs\RADIO_SHEET.md`, its test or any string it quotes; it stays the owner's.
**Unit 383's item 5** (two inherited reds) - `TheOliviaMoveUpTests.cs` was **not edited**, so
`ItIsNotOfferedOnAGuessedYourTurn` stays red and is not this unit's; `ViewTestsActThroughControlsTests`
likewise. **Unit 383's item 4** (`TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend`) - not
quarantined, not chased, not this unit's; it did not appear in any run here.


**1. 4.3 is met on the measurement, and partial under the strictest reading. A finding
with a number, not a ruling request.** 183 lines scanned with nothing excluded; **0**
tier-1 phrases in the sheet's own voice; **2** inside quotes, both declared, both proved
character for character to be sentences Hamlet itself produces; **9** tier-2 mentions
counted. Under the strictest reading of *no sentence tells the operator to touch the
radio*, a declared exception is still a sentence on the page that does - the page quotes
it and a man reading the page reads it. **The remedy would be a change to a sentence
Hamlet says.** One of the two is what Hamlet says when its own unkey did not get out, and
the only correct advice at that moment is the advice it gives; changing either is a change
to what the product tells the operator about a send and about the radio's safety, and that
is the owner's. **This is the honest limit of what a unit can reach**, and nothing is asked
for here.

**2. No quoted tier-1 hit failed its proof, so the item that would have been first does
not exist.** Both declared sentences are kind (a) - produced by the shipped view model,
character for character. Nothing on the sheet says something the screen does not.

**3. `_psk31Canned` has exactly one consumer and it is the token expression** - four
mentions, one read, at `MainWindowViewModel.cs:16727`. Ruling 2 item 4's stop was not
reached, so task 3 ran. What it did: it marks the press and re-routes nothing, and the
three macro rows still send through `AnswerPsk31Command` and the card's own
`CardActionCommand`. A finding, not a ruling request.

**4. A third name has joined the environmental family, and this unit's own load may be why
it showed.** `TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend` went red on **both**
attempts of the exit named-type run with **two different failures** - unit 380's file-lock
`IOException` on the temp `jsonl`, then a wall-clock loss where the record had not been
written when the wait gave up - and is **green on its own in 273 ms**. It is not on the
carry-forward list and the carry-forward invocation was green. **It waits on wall time
under parallel load, which is unit 381's item 1's shape**, and the invocation it lost in
grew from 104 names to 112 tonight because this unit added them. **Recorded, not chased,
and not quarantined** - a finding for whoever picks up 2.3 or 2.4.

**5. The two inherited reds, reported and repaired neither**, verbatim and unmoved from
task 0 to task 4: `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns`,
`Assert.Empty` failure - *TheStopIsAlwaysOnScreenTests.cs:102 writes OperatingMode, which
the CW / Digital / Voice tab strip owns - press it instead*; and
`TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`, `Assert.Equal` failure. Neither is
on the carry-forward list and neither is this unit's.

**6. One measurement refined the instruction and the measurement won.** Section 5 names
three sites for *stop it at the radio* and treats them alike. Measured: **`:19154` is the
SLOTTED path** - `WentLine`, FT8 and FT4 - and so is not reachable in this unit's two
modes at all; `:18913` is the unslotted cancelled-run line; `:19587` is the stop press's
own. **The sheet quotes `:19587`, which is the one a fixture produced whole.** `:18913`
says the same thing in different words - *Nothing Hamlet sent to stop the radio got out -
if it is still transmitting, stop it at the radio* - and **is not on the sheet**, because
no fixture here reached a cancelled run whose abort got nowhere. It is not a gap in any
criterion (4.2's backward direction is about refusals, and it is 12 of 12), but it is a
second operator-facing sentence about the same moment, and a later unit could quote it.
**A finding.**

**7. The sheet moved further over its target and the number is reported.** 183 lines and
11,585 bytes against 120 and 10,240, from unit 382's 176 and 10,918. **What bought it is
the stop sentence and the four lines that explain it**, which ruling 1 item 5 put there
because a page read at the radio that leaves out what Hamlet says when its own unkey did
not get out would be hiding the one sentence safety turns on. Nothing was dropped to pay
for it.

**8. Two decisions about shape that the owner should see.** Two new carry-forward names
were added that the instruction did not ask for (section 1, decision 1), and two commits
were merged that section 11 would have split (section 1, decisions 2 and 3), in both cases
to avoid leaving a commit tip with a carry-forward name red. **Findings, and both are
reversible in one line.**

**9. The `RULES_AT` id-scheme split is reported and not repaired**, as it has been by five
units now. `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)` because `tools/status.sh`
writes that field as a literal, while `CLAUDE.md` §1 holds `CPS-DEC-0165`. **`tools\` is
not this unit's to edit.**

**10. `validate-output.bat` refused again - a sixth unit running - and the six rules were
hand-checked instead.** The exact command, in the shape section 2 prescribes:

```
./tools/arbiter/validate-output.bat output.md
```

The exact refusal: `This command requires approval`. **It is the permission mode and not
the syntax**, and a non-interactive session cannot answer the prompt. **What follows is a
HAND-CHECK against the script's own source, not a run of it**, and nothing below was
produced by the validator:

| Rule, from the script's own header | Hand-check |
|---|---|
| 1 - a `UNIT:` line above section 1, parseable | **ok** - line 29, within the 60 lines it reads |
| 2 - the four top-level sections, in order, exact names | **ok** - `## 1. What Claude did` (48), `## 2. What the owner should expect` (115), `## 3. What you should see` (141), `## 4. What's blocking us` (315) |
| 3 - no fifth top-level section | **ok** - there are exactly four `## ` lines; every deeper heading is `### `, which its pattern `^## ` does not match |
| 4 - section 4 present even when empty | **ok** - present and not empty, with a straight apostrophe as its own matcher expects |
| 5 - section 3 non-empty | **ok** - lines 141 to 314 |
| 6 - the ordering block above the `UNIT:` line, A, B, C, and C naming a count | **ok** - `READ IN THIS ORDER.` line 1, `A.` line 3, `B.` line 10, `C.` line 20, and C says *raises 10 items*, which its `raises \d+ item` pattern matches |

### The carried queue, verbatim per HM-DEC-139 - twenty-nine, and this unit answers none

Unit 382's item 5; unit 381's item 1; unit 380's items 1 and 4; unit 379's items 1, 3 and
7; unit 378's items 1, 3 and 4; unit 377's item 4; unit 376's items 3, 4 and 5; unit 375's
items 3 and 4; unit 374's item 3; unit 373's item 2; unit 372's items 4 and 7; unit 371's
five; unit 369's four.

**Which of unit 382's five came off.** Items 1, 2 and 3 came off, answered in instruction
383 section 3: the `mode` refusal token at `:16400` cannot fire under PSK31 or Olivia and
the token stays exactly as it is; the four `DigitalCaptureRefusal` values belong to the
thirty-second FT8 press; only `DigitalSendCqButton` at `MainWindow.axaml:3558` sends.
**Item 4 - that a sentence Hamlet says would hit R11's word list and the scan excluded
quotes - was this unit's whole subject and is answered tonight**: the scan now covers every
line, the hit is found, and it is declared and proved rather than excluded. **Item 5 stays
on** - five of the twenty-six quotes got kind (b) rather than kind (a), which is still five
of twenty-seven, and nothing tonight was licensed to reach those states.
