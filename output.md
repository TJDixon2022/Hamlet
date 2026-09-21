```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial, step 1 done, step 2
   partial on 2.4 alone and deferred by R43, step 6 done by R42, steps 3, 4 and 5
   not started with step 3 waiting on step 8, step 8 not started and waiting on
   this one. This is the first unit ever spent on step 7.
B. Step 7 - the canned list and the hover. 7.1 met: 7 of seven lines from
   data/psk31/canned.json, a menu on 5 rows that had none, malformed reported.
   7.2 met: 4 text lines write macro canned, 3 macro lines write their own token,
   all 7 announced and capped, no text in the record. 7.3 met: 10 of R39's ten
   facts on the hover, 0 absent because Hamlet lacks them on a PSK31 row and 1
   absent on an Olivia row, text off and reachable by a click. 7.4 met. 7.5 met:
   the chip was already right, the send line reads Sent "CQ CQ CQ de KC3QIS
   KC3QIS KC3QIS pse K" - 29 s of Olivia. THE STEP IS DONE AND STEP 8's ENTRY
   IS OPEN.
C. The report last. Section 4 raises 4 items on top of the carried nineteen, and
   none of them is in the way of a criterion in B. Unit 377's items 1, 2 and 3
   came off the queue - R43 deferred the timing rows, section 6 ruling 1
   answered the commit order, and the reader count was a finding with nothing
   left in it.
```

```
UNIT:       378 - complete at task 4 of 5 - 2026-09-21 14:52
PHASE GOAL: Hamlet keeps what the PSK31 and Olivia threads already earned it -
            screen, record and test work that needs neither the radio nor the
            owner, judged by tests that ran and, at the end, by Tim at his window.
UNIT GOAL:  A right-click on any PSK31 or Olivia row that names a station offers
            Tim the seven things an operator actually says, from a file he can
            edit without a session; one click sends the one he picked through the
            one send door Hamlet already has, framed, announced, capped and
            recorded with no text; and the hover stops reading him back the line
            he is already looking at and tells him what the row knows instead.
ADVANCED:   step 7, criteria 7.1, 7.2, 7.3, 7.4 and 7.5 - the whole step
NUMBER:     PSK31 and Olivia rows that offer something on a right-click:
            2 of 7 -> 7 of 7, and lines on the menu 1 -> 7
DRIFT:      none
```

## 1. What Claude did

**Complete, at task 4 of 5 (tasks 0 to 4). Nothing was dropped.** Machine QUIVERFULL,
project gate `PROJECT: Hamlet` verified against the tree - `SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` both present, neither `CoreHMI.sln`
nor `MURC.sln` exists, root `C:\Source\HamLet`. Branch `main`, six commits, pushed.

**Task 0 - the record and the entry run.** `PHASE_STATUS.md` `CURRENT_STEP` 0 → 7 and
`WORK_INSTRUCTION` 377 → 378, both stale where the rev4 commit left them. Version
1.13.64 → 1.13.65 with its line in the version log. `UNIT 378 - STEP 7` appended to
`PHASE_OUTCOME.md`. Entry carry-forward, both invocations, one build each, status written
immediately before each: **app 212 of 212 in 2 m 23 s on the second attempt** - the first
was 211 of 212, lost at 1 ms to Avalonia's headless `InvalidProgramException` in
`HeadlessUnitTestSession.EnsureApplication` on
`ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel`, which is
unit 375's item 3, re-run once and recorded rather than chased - and **engine 150 of 150
in 4 m 57 s**, green first time. Both exactly where unit 377's exit left them.

**Task 1 - the trace. It built nothing and changed no source file.** One `[Fact]`,
`Unit378Trace`, run by name, over a real view model with two fixtures: the eight PSK31
corpus transcripts on their own carriers, and four Olivia channels at four variants.
Eleven rows. It answered every question the instruction asked, and three of its answers
decided what tasks 2 and 3 were allowed to do:

- **7.1's before: 7 rows named a station, 2 had any menu at all, 5 named a station and
  had no menu, and the most items any menu carried was one.**
- **7.3's measurement: on all 11 rows the whole-message tip drew the row's own text, and
  on all 11 a click on `DecodedRowWholeMessage` opened exactly the same string.** So
  taking the text off the tip hides *detail*, which CLAUDE.md 0.5 allows. Had it opened
  something else, the words would have stayed and 7.3 would have been partial. The
  measurement decided it, not the criterion.
- **7.5's before: the chip was already right and the send line was not.** Under FT8, FT4,
  PSK31 and Olivia the chosen chip is the mode chosen and the only one. A real Olivia CQ
  driven over `FakePort` and `FakeSink` left the panel reading, verbatim,
  `Sent "CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K" - 29 s of PSK31.`

It also priced all seven lines at PSK31 and at every Olivia variant against the cap that
actually applies (section 3), and located each of R39's ten facts by property or object.

**Task 2 - the file, the reader, the menu and the send, in ONE commit.** One commit
because landing the capability and landing the frame, the burst and the cap it rides in
two would leave a commit in which Hamlet could do a thing it may not do (section 6 ruling
1 item 9, which also answers unit 377's item 2). **There is no commit in this unit's
history at which a canned line could reach the air unframed, unannounced or uncapped.**

`data\psk31\canned.json` carries R39's seven in his order with his labels, the first three
naming Hamlet's own `Answer`, `Report` and `Confirm` macros and the last four carrying
text, with a leading `_note` saying what a row is, where his own copy goes and that a line
goes out framed. It is `Content` with `CopyToOutputDirectory` and **not** an
`EmbeddedResource`, because a file the operator is meant to edit must exist as a file.
`Psk31CannedLines` reads `%AppData%\Hamlet\canned.json` first where he has made one and
**Hamlet writes neither** - it does not create his copy, does not copy the shipped file
there and does not repair it. A malformed file is **one note and no canned item**, never a
partial read. The menu is on every row `Psk31StationOn` names a station on; the view model
decides and the view draws, so the menu a test reads and the menu Tim sees are one list.
`TheCannedListIsOfferedTests` **12 of 12**.

Then, **in its own commit**, the R12 rewrite. `ThePsk31ReadsTheConversationTests` went red
exactly where section 5 said it would - `Assert.Single` on the flyout got seven. It is on
the carry-forward list, so it was read twice before it was written. **Nothing was
loosened**: `Assert.Null(model.SendMenuFor(row))` is untouched and still the first thing
asked of every row, and the Answer item's command and parameter are still asserted by
identity. **Four things are asserted that never were** - that a row naming a station has a
menu where it had none, the file's order with R39's labels, that no item carries
`SendMessageCommand` so no FT8 message can be on one of these rows, and the criterion's
own number. **8 of 8** where it was 7 of 8.

**Task 3 - the hover, and 7.5's two namings.** `DigitalDecodeRow.RowFacts` is one ordered
string built in the view model, in R39's order. The inline `ToolTip.Tip` drawing
`WholeMessage` is gone, so the pointer finds the facts. **When his carrier stopped** is now
a fact, written in the one place `Ended` is set. An Olivia row is driven through the same
hover and the same menu and reads the same string line for line. The chip is asserted on
all four modes and was **not repaired** - it was already right (R14). `Psk31WentLine` now
names `ChosenDigitalMode` in both of its sentences. `TheRowHoverSaysWhatItKnowsTests`
**10 of 10**. `BindingHealthTests`, `TheWholeMessageTests`, `AWorkedStationIsDimTests`,
`TheOliviaRowsReadLikePsk31Tests` and `TheRowShowsWhatWasHeardTests` were read first and
are green.

**Task 4 - one shortfall found while judging 7.3, and closed rather than reported.** 7.3's
bar is *every fact Hamlet has is on the hover*. `Reading` is the **latest** complete
message, so the grid - and the distance with it - vanished the moment a station said
`73 SK`, while his conversation card still showed both. That is a fact Hamlet **has** and
the hover did not reach, which section 6 ruling 2 item 2 calls a shortfall and not an
absence, and it was reachable without walking the ledger. The row now carries the
conversation's latest **certain** grid, by the card's own selection. Asserted, and the
exit run was re-done over the change.

**Exit carry-forward, both invocations, after the last change: app 214 of 214 in 2 m 15 s,
engine 150 of 150 in 4 m 55 s, both green on the first attempt of the final run.** Name
for name against task 0's 212 and 150, the only differences are this unit's own two
additions. **No regression** (HM-DEC-165). The send guards were read first and none moved.

**Decisions this session made for itself**, both transcribed in full in the work
instruction's section 6 and in `PHASE_OUTCOME.md`: (1) the file's path and shape, the
`%AppData%` override, the malformed-file refusal, the three macro rows being sent by the
commands that already send them, the `canned` token at the macro cap, the menu being on
every row that names a station, and the one-commit order; (2) the hover being built in the
view model in R39's order with absent facts absent, the text coming off only because a
click was proved to open it, and 7.5's chip being measured before it was touched. **That is
two, which is R31's limit.** The nineteen carried asks got none.

**No port was opened, no device was enumerated and nothing was keyed. Every sample lived
in an array** (FACT-004).

## 2. What the owner should expect

Right-click a station you are hearing on PSK31 or Olivia and you now get seven things to
say to him - *Answer him*, *Send my report*, *Confirm and 73*, *Say again?*, *Please repeat
your report*, *QRZ?*, *73 and out* - and one click sends the one you picked, with both
callsigns in front of it and the hand-back behind, announced by the same burst and held to
the same thirty seconds as everything else Hamlet sends. Before tonight a right-click
offered you exactly one thing, and only where the parser was certain the station was
calling CQ; on every other row it opened a card and nothing else. The seven live in
`data\psk31\canned.json` beside the application - open it in an editor, change the words,
add your own. Copy it to `%AppData%\Hamlet\canned.json` and Hamlet reads yours instead;
it will never write over either one. If you break the file it tells you which file and
which row is wrong and offers you nothing at all, rather than guessing at the half it
could still read. Three of the seven are the macros Hamlet already had, sent by the
buttons that already send them, so there is only ever one spelling of an answer. And the
hover over a row has stopped reading you back the line you are already looking at: it now
tells you who the station is, which country issued his callsign, his grid and how far
away and which way, where he is in the passband and how loud, when you first heard him and
when his carrier went, whether you have worked him before and whether this message is
addressed to you, what the parser made of it and how sure it is, and what a click and a
right-click will do. **What will look wrong but is not:** a hover with fewer lines on some
rows. A fact Hamlet does not have is simply not drawn - no dash, no *unknown*, no empty
label - so a station who has sent no grid has no grid line, and an Olivia row shows no
decibels because the Olivia listener measures in its own units and a number in the
decibel column meaning something else would be a claim nothing measured. The words of the
message are still there: they are on the row, and one click on the message opens the whole
of it, which was measured and proved before a single character came off the hover. Every
claim above was computed, not seen (FACT-004); **no port was opened and nothing was
keyed.**

## 3. What you should see

### The menu, row kind by row kind - before and after

Measured by `Unit378Trace` over eight PSK31 corpus transcripts and four Olivia channels
(one had ended and so is not listed). This is what makes 7.1 a measurement.

| Row | `Psk31StationOn` | `Psk31CqOn` | Flyout BEFORE | Flyout AFTER |
|---|---|---|---|---|
| PSK31, W1AW, signed off | W1AW | null | **none** | 7 entries |
| PSK31, G4XYZ, signed off | G4XYZ | null | **none** | 7 entries |
| PSK31, VE3XN exchange, last speaker is the operator | null | null | none | none |
| PSK31, F4DIA/EI4GNB, signed off | EI4GNB | null | **none** | 7 entries |
| PSK31, K3ABC, report, **not certain** | null | null | none | none |
| PSK31, JA1XYZ, **certain CQ DX** | JA1XYZ | JA1XYZ | 1 item, *Answer JA1XYZ* | 7 entries |
| PSK31, N4ZEK exchange, last speaker is the operator | null | null | none | none |
| PSK31, KD8WYT/P, report | KD8WYT/P | null | **none** | 7 entries |
| Olivia 8/250, W1AW, **certain CQ** | W1AW | W1AW | 1 item, *Answer W1AW* | 7 entries |
| Olivia 16/500, K3ABC, report | K3ABC | null | **none** | 7 entries |
| Olivia 4/500, damaged callsign | null | null | none | none |
| **Totals** | **7 name a station** | 2 are certain CQs | **2 of 7 had a menu** | **7 of 7, seven lines each** |

### The seven lines, their characters, their seconds and the cap that applies

Framed for `W1AW de KC3QIS`. The three macros as their own composers build them, the four
texts as `Psk31Macros.Typed` frames them. **Every one passes `Psk31Macros.Sendable` with
zero characters dropped** - Hamlet ships no line it cannot send. Seconds are
`Psk31Modulator.SecondsFor` and `OliviaModulator.TextSeconds`; the announcement in front is
not counted, as the cap does not count it. **A shaded cap is the 30-second fallback**, which
is what a variant with no row in `data\olivia\timing.json` gets (R43, deferred).

| Line | Chars | PSK31 (cap 30) | 4/250 (30\*) | 4/500 (30\*) | 8/250 (82.60) | 8/500 (30\*) | 16/500 (61.95) | 16/1000 (30\*) | 32/1000 (49.56) |
|---|---|---|---|---|---|---|---|---|---|
| Answer him | 23 | 7.84 ✓ | 12.30 ✓ | 6.15 ✓ | 16.42 ✓ | 8.21 ✓ | 12.32 ✓ | 6.16 ✓ | 10.27 ✓ |
| Send my report | 97 | 24.86 ✓ | **50.19 ✗** | 25.10 ✓ | 67.62 ✓ | **33.81 ✗** | 51.23 ✓ | 25.62 ✓ | 40.99 ✓ |
| Confirm and 73 | 62 | 16.26 ✓ | **31.76 ✗** | 15.88 ✓ | 43.04 ✓ | 21.52 ✓ | 32.80 ✓ | 16.40 ✓ | 26.66 ✓ |
| Say again? | 65 | 18.40 ✓ | **33.81 ✗** | 16.90 ✓ | 45.09 ✓ | 22.54 ✓ | 34.85 ✓ | 17.42 ✓ | 26.66 ✓ |
| Please repeat your report | 52 | 14.75 ✓ | 26.64 ✓ | 13.32 ✓ | 36.90 ✓ | 18.45 ✓ | 26.66 ✓ | 13.33 ✓ | 22.56 ✓ |
| QRZ? | 47 | 14.05 ✓ | 24.59 ✓ | 12.30 ✓ | 32.80 ✓ | 16.40 ✓ | 24.61 ✓ | 12.30 ✓ | 20.51 ✓ |
| 73 and out | 63 | 17.89 ✓ | **32.78 ✗** | 16.39 ✓ | 43.04 ✓ | 21.52 ✓ | 32.80 ✓ | 16.40 ✓ | 26.66 ✓ |

**All seven fit at PSK31 and at every variant that has a timing row.** Five line-and-variant
pairs are refused, every one of them at a variant taking the 30-second fallback. That is a
finding, not a ruling request - R43 already ruled the timing rows deferred and the fallback
the safe direction - and it is named again in section 4.

### R39's ten facts: where each lives, and whether it reached the hover

| Fact | Where it lives | On the hover |
|---|---|---|
| station | `DigitalDecodeRow.Sender`, from `Reading.Speaker` | yes |
| country | `DxccPrefixes.EntityOf(row.Sender)` | yes |
| grid | the conversation's latest **certain** `Psk31Exchange.Grid`, put on the row as `HisGrid` | yes |
| distance | computed from `ObserverGrid` and `HisGrid` through `GridPath`, as the card does | yes |
| offset | `DigitalDecodeRow.Hz` | yes |
| strength | `DigitalDecodeRow.Snr` | yes on PSK31; **absent on Olivia**, see below |
| started | `DigitalDecodeRow.Utc` - on one of these rows this is the **first-heard** time | yes |
| stopped | `Ended` is the fact; the moment is new, `StoppedUtc`, written where `Ended` is | yes |
| spoke to Tim | `HasWorkedBefore` / `WorkedTip` from the log; `Reading.IsForOperator` | yes, both |
| parser kind and certainty | `Reading.Kind`, `Reading.IsCertain`, said in words | yes |

**The one absence, and it is an absence rather than a shortfall.** An Olivia row draws no
decibels. The Olivia listener measures a block's signal-to-noise in its own units, not
decibels in 2500 Hz, and unit 364 (decision AF) already ruled that a number in the PSK31
column meaning something else would be a claim nothing measured (§0.0). Hamlet does not
have *how loud in decibels* for an Olivia channel, so the line is not drawn.

**The shortfall this unit found and closed rather than reported:** the grid. Reading it off
`Reading` alone - the latest complete message - lost it, and the distance with it, the
moment the station signed off, while his card still showed both. The row now takes the
conversation's latest certain grid.

### The hover, before and after, verbatim

**Before**, over the message on a row, for a station who has just sent his report:

```
W1AW  ·  181139

KC3QIS de W1AW  RST 599 599  Name Hiram  QTH Newington  Grid FN31 FN31  BTU KC3QIS de W1AW K
```

**After**, on the same row:

```
W1AW
United States of America
Grid FN31 · 360 miles east-northeast
1000 Hz · +10 dB
Heard since 18:25:05 UTC
This one is addressed to you.
He sent a signal report, read for certain
Click the message to read the whole of it. Right-click for the lines you can send.
```

**And the words are still one click away.** `DecodedRowWholeMessage`'s own
`OpenTheWholeMessageCommand` opens a popup bound to exactly the string the tip used to
draw - proved on all 11 traced rows **before** anything came off, and asserted afterwards:
the box opens and holds `RST 599 599`. Hiding detail, which §0.5 allows; leaving the words
unreachable would have been hiding information, which it forbids.

### 7.5: the chips, and the send line

Measured at 14.0715 MHz **before anything was touched**, which is why this half is an
assertion and not a repair (R14):

| Chosen | FT8 | FT4 | PSK31 | WSPR | Olivia |
|---|---|---|---|---|---|
| FT8 | **chosen** (elsewhere) | plain | lit | plain | plain |
| PSK31 | plain | plain | **chosen and lit** | plain | plain |
| Olivia | plain | plain | lit | plain | **chosen** (elsewhere) |

Exactly one chip is chosen on every mode, and it is the one he pressed. **7.5's first half
was already met in the tree.**

The send line was not. Every sentence in `Psk31WentLine` and `StopLine` was read; two named
the mode and both named it as the literal `PSK31`.

- **Before:** `Sent "CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K" - 29 s of PSK31.` — under
  Olivia, reproduced exactly as Tim saw it on 2026-09-21.
- **After:** `Sent "CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K" - 29 s of Olivia.`
- **And PSK31 still says PSK31:** `Sent "CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K" - 13.3 s of PSK31.`

Both are read off the sentence the panel is actually left holding after a real send over
`FakePort` and `FakeSink`, not off a format string. **Nothing about what is composed, armed
or keyed changed.**

### The carry-forward counts, before and after

| | App | Engine |
|---|---|---|
| Task 0, before any change | **212 of 212** (2 m 23 s, second attempt) | **150 of 150** (4 m 57 s, first) |
| Task 4, after the last change | **214 of 214** (2 m 15 s, first) | **150 of 150** (4 m 55 s, first) |

Name for name, the only differences are this unit's two additions:
`TheCannedListIsOfferedTests.OneClickSendsTheLineFramedAnnouncedCappedAndRecordedWithNoText`
and `TheRowHoverSaysWhatItKnowsTests.TheHoverSaysTheFactsInTimsOrderAndNeverTheText`, both
on the app line by type and method with their paragraphs, and the human-readable list
underneath changed to match. **No regression.** The dispatcher loop showed twice, both on
the app invocation, both re-run clean.

**And the send path is the same path.** After task 2 a canned line reaches the air through
one `Compose`, one `Arm`, one `PttOn`, the same 30-second cap and the same RSID burst -
asserted by one call to the sound card, a `psk31_send_composed` carrying `announced: true`
with the BPSK31 code read out of the RSID file rather than typed, `capSeconds` equal to
`OperatorSend.LongestUnslottedSeconds`, and a sweep of every line of the telemetry file
finding neither callsign nor a word of the text. **The only thing that is new is what the
text says.**

## 4. What's blocking us

**Nothing is blocking a criterion in B.** All five are met and the step is done.

**1. `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns` is red, and
it is inherited. A finding, not a ruling request.** It reports
`TheStopIsAlwaysOnScreenTests.cs:102 writes OperatingMode, which the CW / Digital / Voice
tab strip owns - press it instead`. That line was written by **unit 375**, in its own R12
repair of the Stop name, and this unit changed neither file. It is not on the carry-forward
list, so task 0 never saw it and it has been red through unit 376 and unit 377 unnoticed.
It is a one-line repair - press the strip instead of writing the property - and it belongs
to whichever unit next touches that file. **Reported, not repaired**, per the instruction's
*report mismatches; repair nothing but this unit's*.

**2. Five of the seven canned lines are refused by the 30-second fallback at a variant with
no timing row. A finding, and R43 already ruled it.** Named with their seconds:
*Send my report* at Olivia 4/250 (50.19 s) and at 8/500 (33.81 s), *Confirm and 73* at
4/250 (31.76 s), *Say again?* at 4/250 (33.81 s), *73 and out* at 4/250 (32.78 s) - each
against a 30.00 s cap. Every refusal is at 4/250 or 8/500, the two of the four fallback
variants where the mode is slowest; at every variant that has a timing row all seven fit
with margin. R43 deferred the rows and called the fallback the safe direction, and this
unit wrote no timing row. **What Tim gets out of it:** on a slow Olivia variant his report
will be refused before anything is keyed, in the sentence that already says so, and the
four short lines will go.

**3. The canned file is read once per session. The author's choice, stated, and
overrulable in one line.** A menu is built on every right-click and a disk read on the
gesture is a cost the gesture should not carry, so an edit to `canned.json` is picked up
the next time Hamlet starts. That is what *without a session* asks for and no more. If Tim
wants an edit to take effect while Hamlet is running, that is a small change and a later
unit's.

**4. `Unit378Trace` now prints the after, not the before.** It is a trace: it prints and
asserts nothing about the product, so it stayed green through tasks 2 and 3 and now reports
seven entries per row and a hover of facts. **The before is preserved in this report, in
`PHASE_OUTCOME.md` and in task 1's own commit message**, which is where the next unit should
read it. It is deliberately not on the carry-forward list (R14).

**No way was found for a canned line to reach the air unframed, unannounced or uncapped**,
and that is stated here because the instruction asks for it as the first item if one had
been found. There is no second composer, no second `Arm`, no second `PttOn` and no path
that skips the cap; the file, the reader, the menu and the send landed in one commit so no
commit in this unit's history leaves Hamlet able to do a thing it may not do.

---

**The carried queue, verbatim per HM-DEC-139. Nineteen, and this unit answers none of
them.** Unit 377's items 1, 2 and 3 came off it: R43 deferred the timing rows, section 6
ruling 1 answered the commit order, and the reader count was a finding with nothing left in
it.

1. **Unit 377's item 4** - `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn` is red,
   inherited, measured against the old embedded file and red there too, and not on the
   carry-forward list. A finding. This unit did not repair it and did not chase it.
2. **Unit 376's item 3.**
3. **Unit 376's item 4.**
4. **Unit 376's item 5.**
5. **Unit 375's item 3** - Avalonia's headless `InvalidProgramException: You have caused
   dispatcher loop`, which kills an app invocation at about 1 ms before any assertion and
   moves between names. **It showed twice in this unit**, on
   `ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel` at task 0
   and on `TheWindowHoldsBelowItsMinimumTests.TheWorkingPanelsScrollInsideThemselves
   RatherThanCollapsing` at the first exit run - two names it had not landed on before -
   and both times the single re-run was clean. Recorded, not chased.
6. **Unit 375's item 4** -
   `TheStopIsAlwaysOnScreenTests.WithNothingKeyedItSaysStopAndIsStillPressable`. It did not
   show in this unit's four app invocations.
7. **Unit 374's item 3** - `tools\run-carry-forward.sh`. The two command lines were run
   from `docs\carry-forward-tests.txt` itself, as the instruction directs.
8. **Unit 373's item 2** - `ApplyBestBet`'s stale `BestBetLabel`.
9. **Unit 372's item 4.**
10. **Unit 372's item 7** - `PHASE_PLAN.md`'s criterion checkboxes. Not ticked, not raised.
11. **Unit 371's first.**
12. **Unit 371's second.**
13. **Unit 371's third.**
14. **Unit 371's fourth.**
15. **Unit 371's fifth.**
16. **Unit 369's first.**
17. **Unit 369's second.**
18. **Unit 369's third** - `LearnedAlcReference.Ago()`.
19. **Unit 369's fourth.**

**And the `RULES_AT` id-scheme split, carried and reported again, not repaired:**
`PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-165 (2026-09-19)` while `CLAUDE.md` section 1
holds `CPS-DEC-0165`. It is unit 377's item 5, carried in `PHASE_PLAN.md` section 7 and
reported unrepaired by units 376 and 377. `tools/status.sh` writes the field as a literal,
so repairing it means editing that script, which is not this unit's.

**Step 8's entry reads *step 7 done*, and step 7 is done.** It is open.
