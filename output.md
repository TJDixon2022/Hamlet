# Work instruction 316 - read the conversation

**READ IN THIS ORDER.**

A. **The phase goal - Hamlet works PSK31 the way it works FT8.** Steps 0, 1 and 2 done,
   3 met by this unit's measurements, 4-6 not started.

B. **Step 3 and its exit criteria** - all seven must-pass met, and the nice-to-pass met.
   Corpus lines matching speaker, addressee, kind and turnover: 32 of 32. Lines asserted
   certain that the corpus marks uncertain: 0. Unknown rate: 0.00 on seven transcripts
   against 0.00 expected; on 05-garbled 0.40 uncertain and 0.20 with no speaker, against
   expected 0.20 and the corpus's own 0.40 of lines at certain:false - not lower under
   either reading. Own callsign recognised from the corpus operator field: 8 lines, none
   in 04-not-for-me. CQ filter exact: 12 of 12 CQ row-steps in, 0 of 28 others, over eight
   live channels. Unchanged-code diff: empty. Guessed-vs-certain exposed: IsGuess and the
   words guess and unknown. Name and QTH unparsed: the parse has no member for either.
   03-no-report ends on 73: both lines End, no report.

C. **The report last, and section 4 raises 6 items** on top of a carried queue of
   twenty-four. None is in the way of a criterion in B. Item 1 - one FT8 hover test that
   is red when run first - is not a step 3 criterion, but it is the one red this unit met
   that is on no known-red list.

```
UNIT:       316 - complete at task 4 of 5, task 5 dropped - 2026-09-11 12:52
PHASE GOAL: A third digital mode that works like FT8 - same two cards, same one click,
            same log and achievements - on a PSK31 modem Hamlet writes itself.
UNIT GOAL:  Turn each PSK31 row's free text into who is speaking, to whom, what kind of
            line, whose turn, and how sure - and let the FT8 row readers use it unchanged.
ADVANCED:   yes - step 3, all seven must-pass and the nice-to-pass met.
NUMBER:     corpus lines parsed as the corpus states 0 -> 32 of 32; PSK31 rows the CQ
            filter can select 0 -> every row whose latest message is a CQ (12 of 12
            row-steps, 0 of 28 others); version 1.13.3 -> 1.13.4
DRIFT:      0 consecutive units without advance  (was 0)
```

**Every appearance claim in this report is computed, not seen.** Nothing in this repository
can look at a picture. **The corpus is written, not recorded**, and nothing here is evidence
about the radio (FACT-004, FACT-006).

## 1. What Claude did

**Complete at task 4 of 5. Task 5, the named drop candidate, was dropped whole.** Development
machine, Windows 11, project Hamlet, branch `main`. The gate held: `SHACK_FACTS.md` and
`CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and `MURC.sln` absent, root
`C:/Source/HamLet`. HEAD was `1a82000` at the start and before the first commit. Every task
was committed and pushed: `2a9d9ed` task 1, `4895907` task 2, `eb67036` task 3, `132995f`
task 4, then the report commit that carries this file. Every push succeeded.

**Why task 5 went.** At task 4's end the unit had run 39 minutes, with the report and its
validation still to do, and task 5 would have taken it past §2's 60-minute window. It moves
no must-pass. The next unit inherits it unchanged. One partial proof from audio already
exists: `ThePsk31HearsEveryoneTests` ran the four-signal file through the new wiring and
printed `put on his side: True`, `on his side, not addressed him: False`. That is not task 5's
table and is not claimed as it.

**The line the launcher's prompt assigned.** `PHASE_STATUS.md` `WORK_INSTRUCTION:` set to
`316 - read the conversation`, and nothing else in that file.

**The validator did not run, so this report has no exit 0 from it.**
`tools\arbiter\validate-output.bat output.md` was refused by the shell with "requires approval",
as unit 315 measured, and per the instruction it was not retried in another form. Ask 17 stands.
**What was measured instead** is this file against the seven rules the script prints, with
`grep`. This is a reading of the same rules, not the script's verdict:

| Rule | Measured |
|---|---|
| 1 `UNIT:` line | line 25, inside the 60-line window |
| 2, 3 four sections, exact names, in order, no fifth | `## 1.` at 41, `## 2.` at 288, `## 3.` at 309, `## 4.` at 329, and no other `## ` |
| 4 section 4 present | yes |
| 5 section 3 non-empty | 14 non-blank lines |
| 6 ordering block | `READ IN THIS ORDER` at 3, `A.` at 5, `B.` at 8, `C.` at 19 with `raises 6 items` |
| 7 placeholder tokens above section 1 | 0 |

The line numbers are from before this paragraph was added. The paragraph sits inside section 1,
so every number below line 41 moves down, and nothing above it does.

### The corpus, line by line

`assets/fixtures/psk31-transcripts/corpus.json`, SHA-256
`964b9729bec883bff9d54fd63afa9bddb21874e9e86ec34439644aec658e26ba` - recorded, not checked;
there is no manifest.

| Transcript | Matched | Wrongly certain | Uncertain rate | No-speaker rate | Expected | Corpus certain:false |
|---|---|---|---|---|---|---|
| 01-textbook | 5 of 5 | 0 | 0.00 | 0.00 | 0.00 | 0.00 |
| 02-chatty | 5 of 5 | 0 | 0.00 | 0.00 | 0.00 | 0.00 |
| 03-no-report | 4 of 4 | 0 | 0.00 | 0.00 | 0.00 | 0.00 |
| 04-not-for-me | 4 of 4 | 0 | 0.00 | 0.00 | 0.00 | 0.00 |
| 05-garbled | 5 of 5 | 0 | **0.40** | 0.20 | 0.20 | 0.40 |
| 06-cq-dx | 2 of 2 | 0 | 0.00 | 0.00 | 0.00 | 0.00 |
| 07-odd-ending | 4 of 4 | 0 | 0.00 | 0.00 | 0.00 | 0.00 |
| 08-lowercase-and-slashes | 3 of 3 | 0 | 0.00 | 0.00 | 0.00 | 0.00 |
| **All** | **32 of 32** | **0** | | | | |

**The corpus's two readings.** On 05-garbled the parser's uncertain rate (0.40) meets the
fraction of lines at `certain: false`. Its no-speaker rate (0.20) meets `unknown_rate_expected`.
The assertion is held to the higher of the two. No clean line was marked uncertain: the
parser's rate is not higher than the corpus's anywhere.

**The split rule, one sentence** (`Psk31MessageSplitter.SplitRule`): a message ends at the
whitespace after K, KN, BTU, OVER, SK or CL when that word follows `de`, one or more clean
callsigns and at most one other word - so K inside OK, and SK, BTU or OVER in the middle of a
sentence, never split, 73 on its own never splits, and turnover words that arrive straight
after a message are its tail and start nothing.

**The certainty rule, one sentence** (`Psk31ExchangeParser` remarks): a parse is certain only
where no token is damaged, the speaker and the addressee are both named, the message ends on a
turnover or closing word, it opens on its own sign (`CQ`, or `CALL de CALL`) and every `de` in
it names the same pair, and no report or grid in it contradicts another.

**The unchanged-code diff, printed:**

```
$ git diff 2a9d9ed..HEAD -- src/Hamlet.RadioEngine/Explore/DxccPrefixes.cs \
    src/Hamlet.RadioEngine/Contacts/NudgeSet.cs src/Hamlet.App/ViewModels/NudgeWords.cs \
    src/Hamlet.App/Controls/AchievementMarkControl.cs
(empty)
$ git diff 2a9d9ed..HEAD -- src/Hamlet.App/ViewModels/DigitalDecodeRow.cs | grep -c "HasWorkedBefore =>\|RowOpacity =>"
0
```

`TheCodeTheseRowsReuseIsUnchanged` holds the same four files by SHA-256, with carriage returns
removed, and finds each of `public bool HasWorkedBefore => _workedBefore.Length > 0;` and
`public double RowOpacity => HasWorkedBefore ? 0.55 : 1.0;` exactly once. In
`MainWindowViewModel.cs`, `MarkIfItOpensSomething`, `Apply` and `WorkedBeforeNote` are called
from one new place and their bodies are untouched.

### Task 1 - entry, and the trace

**Entry, run by exact name:** `ThePsk31HearsEveryoneTests.TheTwoSignalFixtureYieldsTwoRowsEachWithItsOwnText`
passed - two rows, one per carrier, 1000 Hz and 1500 Hz each at CER 0.0000. **Carry-forward
before any change:** app 115 of 115, engine 71 of 71. Version 1.13.3 -> 1.13.4.

**Every reader of a row's meaning** (lines at `2a9d9ed`):

- **The CQ filter** - `WantsRow` (`MainWindowViewModel.cs:1668-1669`) calls
  `DecodedFilterRule.Wants(ShowsCqOnly, row.Addressee)`, which calls
  `Ft8MessageSplit.IsCallToAnyone` (`DecodedFilter.cs:69-70`). Runs unchanged once
  `Addressee` is supplied.
- **The operator's side** - `IsForHim` (`:1650-1652`), gated off by `IsTextOnly` and asking
  `Ft8MessageSplit.IsAddressedTo(row.Message, ...)`. It could not run unchanged, because the
  unchanged body reads text as FT8. It now asks the reading.
- **The fade** - `HasWorkedBefore` and `RowOpacity` (`DigitalDecodeRow.cs:334`, `:359`) read
  `_workedBefore`. That field is set by `WorkedBeforeNote(row.Sender)` in `PlaceRow` (`:10844`),
  `RefreshWorkedBefore` (`:11753`) and `UseWorkedBeforeForTests` (`:12166`). Runs unchanged once
  `Sender` is supplied.
- **The country** - `SenderHelp` calls `DxccPrefixes.EntityOf(Sender)` (`DigitalDecodeRow.cs:546`).
  Unchanged. `EntityOf` is never asked of `Addressee`.
- **The quill** - `MarkIfItOpensSomething` (`:11646`) asks `NudgeSet.WouldOpen(row.Sender)`,
  and `Apply` sets `NudgeWords.For`. Called from `PlaceRow` (`:10796`) and `RefreshWorkedBefore`
  (`:11743`).
- **His side's own readers** - `ConversationStation` (`:2645-2667`), `RebuildWaiting` (`:2863`),
  `CardStations` (`:3017`), `ReportFor` (`:3333`), `NewestRowFrom` (`:3451`) and `LatestSlotFor`
  (`:3581`) read only `Sender` or `StationOf`.

**The path existed and was unwired, with one gap.** PSK31 rows were added straight to
`DigitalDecodes` by `ShowPsk31Channels` (`:2059-2080`) and never passed `PlaceRow`, so neither
the quill nor the worked mark was ever asked on arrival. The fix was smaller than the build:
supply `Sender` and `Addressee`, and ask the two existing methods from the channel path.

**How a general call is spelled.** `IsCallToAnyone` takes `CQ` or anything starting `CQ `
(`Ft8MessageSplit.cs:86-92`). `EntityOf`'s guard refuses `CQ`, `CQDX` and `QRZ`
(`DxccPrefixes.cs:129`). `AddresseeHelp` tests `CQ` and `CQ ` (`DigitalDecodeRow.cs:517-518`).
So `ANY` is handed over as `CQ` and `DX` as `CQ DX`.

**The operator's callsign** is `_settings.Operator.Callsign`. The engine is handed it as a string
- the parser's `Read` argument, the splitter's constructor - and never learns settings exist.

**What one PSK31 row holds.** Each listener channel appends every confident character to a
`StringBuilder` (`Psk31Listener.cs:223`, `:239`). `Psk31Channel(Id, OffsetHz, StrengthDb, Text)`
is the snapshot (`:10`). `ShowPsk31Channels` replaces the row in place, by identity, whenever
strength, offset or text changes, and removes it when the carrier retires. The text never shrinks
and carries both sides of a QSO on one frequency.

**Clicks.** The only click on a row is right-click: `ContextRequested` at `MainWindow.axaml:4115`
and `:4947`, then `OnDecodedRowContextRequested` (`MainWindow.axaml.cs:148`), then `SendFlyoutFor`
(`:194`), then `SendMenuFor` (`MainWindowViewModel.cs:11548`). At `2a9d9ed` that returned null for
a PSK31 row only because `Sender` was empty. Once a sender is supplied, a station the FT8 ledger
already knows would get an FT8 menu. The one door, `SendMessage`, would still refuse PSK31 through
`CanTransmitIn` (`:1945`, `:12454`), but the click would have reached a send path.

**Where `Ft8MessageSplit` could still reach PSK31 text:**
- `IsForHim` - guarded.
- `Fields` - guarded.
- `CanLogRow` (`:12220`) - unguarded, reachable only through a non-null menu.
- `ContactTextFor` then `Ft8ContactLedger.RecordHeard(row.Message)` (`:10987`) - only via
  `PlaceRow`, which PSK31 rows never enter.
- `PayloadHelp` - `Explain(Fields)`, with null fields.

`ContactStandsLine` (`:11501`) and `Addressed` (`:12984`) are asked only of text he sent.

### Task 2 - the parser

`Psk31ExchangeParser.Read(message, operatorCallsign)` returns a `Psk31Exchange(Speaker, Addressee,
Kind, HandsOver, IsCertain, Rst, Grid, IsForOperator)`, where null means unknown. The kinds are
the corpus's nine. The corpus's six rules are stated once in the type's remarks, each with its
`rules[n]`.

- **A damaged token** is one with a symbol welded into it (`5#9`, `K3A~C`). It is never read as
  its nearest neighbour. It makes the field it would fill unknown, and the message uncertain.
- **A clean closing sign** still names its speaker: 05-garbled line 3 reads K3ABC > KC3QIS,
  Report, uncertain, RST unknown.
- **The callsign shape** is the tree's own pattern, held privately in `CallsignResolver`,
  `AutoCallAnswers` and `ScanStop`, and a test binds all four copies.
- **Own station** is `Ft8MessageSplit.IsSameStation` handed two callsigns the parser has already
  read. No text reaches it.

**`ThePsk31ExchangeParserTests`**, engine. Watched failing on a stub, 4 of 8 red (assertions 1,
3, 5, 7). Now 8 of 8.

### Task 3 - messages out of a growing row

`Psk31MessageSplitter.Add(char)` yields a `Psk31Message(Text, Exchange)`. **`ThePsk31MessageSplitTests`**,
engine. Watched failing on a stub, 3 of 3 red. Now 3 of 3.

- **Seven transcripts:** every message came back in corpus order and parsed as the corpus states
  (28 of 28).
- **05-garbled, exactly what came back:**
  - message 1, line 1 - K3ABC > ANY, Cq, certain;
  - message 2, line 2 - KC3QIS > K3ABC, Answer, certain;
  - message 3, line 3 - K3ABC > KC3QIS, Report, **uncertain**;
  - message 4, **lines 4 and 5 merged** - KC3QIS > K3ABC, Report, **uncertain**, because the
    message does not open on its sign.
- **No turnover:** no message, and the text waits in `Pending`. `K` inside `OK`, `SK` mid-sentence
  and `BTU` with no sign split nothing.

**One finding.** No message the splitter yields can lack a speaker: it splits only after `de` and
a clean callsign, and that callsign is the speaker.

### Task 4 - rows become stations

**`IsTextOnly` is kept, and its meaning narrowed** to "no FT8 fields may be read out of this row".
**`DigitalDecodeRow.Reading`** carries the latest complete message's parse, fed through a splitter
per channel with only the new characters.

- `Sender` and `Addressee` come from the reading. There is no addressee where there is no speaker.
- **`IsGuess`** is the member; **`ReadingWord`** is the word a reader sees: `guess` where the parse
  is uncertain, `unknown` where it names no speaker, nothing where it is certain. The row template
  shows the read sender in the FT8 sender's green, with its hover, then the word; the caption
  carries the word on his side. **The wording is this session's, not a ruling.**
- `IsForHim` asks the reading.
- `SendMenuFor`, `CanLogRow` and `CardStations` refuse text-only rows. `CardStations` needed it
  because an FT8 card, with an FT8 Send button, would otherwise follow a PSK31 row onto his side.

**`ThePsk31ReadsTheConversationTests`**, app. Rows built from corpus lines through the splitter,
with the callsign set in settings. Watched failing before wiring, 5 of 8 red. Now 8 of 8:

- **CQ filter:** 12 of 12 CQ row-steps in (including JA1XYZ's `CQ DX`); answers 7, reports 14 and
  ends 7 out.
- **His side:** exactly the rows addressed to him; 04-not-for-me never.
- **05-garbled line 3:** `IsGuess` true, word `guess`, caption `164919 · guess`.
- **No speaker:** no sender, no addressee, hover `Who sent it.`, not in the CQ filter.
- **TI2ABC:** FT8 and PSK31 identical - entity Costa Rica, hover, quill `Visible` with
  `Costa Rica · new country`, lift, and opacity 1.0.
- **Fade:** 0.55 on both, and on a row already listed once the log gains the entry.
- **Unchanged code:** diff empty.
- **Right-click:** 40 PSK31 row-clicks, with W1AW already in the FT8 ledger - no menu, no Log
  item, no card, no answer, nothing sending.

**One existing assertion replaced, not loosened.** `ThePsk31HearsEveryoneTests.NoRowCanBeAnsweredAndNothingIsParsedOutOfOne`
asserted that no row was ever on his side. That was step 2's placeholder, and `IsTextOnly`'s own
remark said "until the step that parses PSK31 says otherwise". The four-signal file's 1100 Hz row
carries `KC3QIS de W1AW W1AW K`, so it went red. It now asserts step 3's rule: a row is on his
side only while its latest message is addressed to him. No FT8 fields and no answering still stand.

**Carry-forward after:**
- Engine 82 of 82.
- App 123 of 123 on the final run.
- `TheReadinessHoverTests.NoRowCarriesABearingInDegrees` failed three times - in the first
  post-change list run and twice by exact name alone - with `Sequence contains more than one
  element` at `AFullExchange().DigitalCards.Single()`. It passed in its class run (4 of 4) and in
  the final list run. It is an FT8-only exchange, and every change in this unit is gated on
  `IsTextOnly` or the PSK31 channel path.
- **It was not measured on the pre-change tree:** the shell refused `git worktree add` with
  "requires approval", and per the instruction that was not retried in another form.
  Section 4 item 1.

### Verified against the tree, and what did not match

- **Matched:** the gate; every path and member in section 5 - `DigitalDecodeRow.cs:482-483`,
  `MainWindowViewModel.cs:1650-1652`, `:1668-1669` and `:2067`, `DxccPrefixes.cs:104` and its
  three-word guard; `NudgeSet`, `NudgeWords`, `AchievementQuill`; the corpus's eight
  transcripts, 32 lines, operator and nine kinds; the carry-forward list's 22 app and 8 engine
  names; version 1.13.3.
- **The corpus disagrees with itself on 05-garbled** - confirmed, not resolved. Two lines are at
  `certain: false`, which is 0.4; `unknown_rate_expected` is 0.2, which is the one line with speaker
  `UNKNOWN`.
- **`RULES_AT`.** `.run-unit/reload.txt` lines 9 and 34 say `CLAUDE.md` section 1 holds
  `CPS-DEC-0161`. `CLAUDE.md` has no `CPS-DEC-0161`; `HM-DEC-161` is at line 360. It is the
  reload's reading, not the tree. `tools\` was not touched.
- **The instruction's transcription of the nudge rulings is stale.** Section 6 says "sticky per
  station with a cap of two". The tree records Tim's 2026-09-11 ruling withdrawing both -
  `MarkIfItOpensSomething`'s remarks and `TheCqListNudgeTests`: "There are no rankings". The code
  was left as the tree has it.
- **Task 4 assertion 4's case cannot come out of the splitter**, as task 3 found. It is proved on
  rows with no finished message, and on one row given the parser's speakerless reading by hand.
- **`ack` is never produced.** §R3's vocabulary has no roger word. Section 4 item 4.
- **Status fields.** `CLAUDE.md` §13.1 names a `PHASE: n of m` field; the prompt and
  `CLAUDE_CODE.md` §2 name `TASK: n of m`. `CLAUDE_CODE.md` wins on shape, and `TASK` was written.

**Left uncommitted, and whose:** `.run-unit/*`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md` and
`SESSION.lock` are the launcher's. `docs/phase-ft4-run/PHASE_OUTCOME.md` is ask 19's. Nothing of
this session's is uncommitted.

## 2. What the owner should expect

- **A PSK31 row now names a station** once a message on it has finished - that is, once a station
  signs `de CALL K`. Before that the row is text with no sender, exactly as in unit 315.
- **A row's station is whoever spoke last on that frequency.** A row carrying both sides of a QSO
  changes sender and addressee at every over. It is not flickering.
- **A CQ leaves the CQ filter as soon as somebody answers on the same frequency**, because the
  row's latest message is no longer a CQ.
- **A CQ whose own callsign arrived garbled never appears in the CQ filter.** Its message never
  finishes, so it names nobody. That is the strict side (§R1, §0.0).
- **`guess` or `unknown` beside a sender is the parser saying it is not sure.** Mostly after noise,
  or when text from before a station's opening call ran into its message.
- **Right-click on a PSK31 row does nothing, and a PSK31 station on his side has no card.** That is
  deliberate: every menu and card today sends FT8. Step 4 builds PSK31's.
- **The last message on a channel counts once the character after its final `K` arrives** - the
  space or line break a macro normally sends. A station that stops dead on `K` is read when the
  next text on that frequency arrives.
- **Two conversations will not show `ack`.** A roger is text under §R3 and reads as chat.
- **One FT8 hover test can be red when run first** (`TheReadinessHoverTests.NoRowCarriesABearingInDegrees`).
  It was green in the final list run. Section 4 item 1.

## 3. What you should see

**When you press PSK31 now, with signals on the band:**

- **The CQ filter shows the stations calling CQ**, including `CQ DX`, and nothing else.
- **A station calling you is on your side** of the panel, not in the left list.
- **Each row picks out its sender in green**, before the text. Hovering it names the country where
  Hamlet's entity table is certain: "Who sent it. TI2ABC is a callsign from Costa Rica."
- **A station you have worked is faded** to the same 0.55 as on FT8.
- **A quill** marks a CQ from a country you have not worked. A continent you have never worked is
  marked as a door and not named.
- **A guessed station says `guess`** in a word beside the sender, and in the time line under the
  message on your side.

**What you must not expect yet:** no answering a row, no sending, no right-click menu, no turn
indicator, and no RST in the log. Those are steps 4 and 5.

**All of this is computed, not seen** - drawn from the view model by tests, with no picture looked
at. **The text it was proved on is the typed corpus, not recorded audio.**

## 4. What's blocking us

### This unit's items, most-blocking first

1. **Hand wanted: say whether `TheReadinessHoverTests.NoRowCarriesABearingInDegrees` predates unit
   316.** It failed three times with two cards where it expects one: alone by exact name twice, and
   once in a list run. It passed in its class run and in the final list run. Run it alone at
   `eb67036`, the commit before task 4:
   `timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter "FullyQualifiedName~TheReadinessHoverTests.NoRowCarriesABearingInDegrees"`.
   The session's `git worktree add` to do it was refused by the shell. *Reasoning:* the test builds
   an FT8-only exchange, and every change here is gated on text-only rows. *Rejected:* adding it to
   the known reds unmeasured, because a known red is one proved red before the unit that found it.

2. **Ruling wanted: accept the replaced assertion in `ThePsk31HearsEveryoneTests`.** "No row is ever
   on his side" became "a row is on his side only while its latest message is addressed to him".
   *Reasoning:* the old line was step 2's placeholder, and step 3's exit criterion requires the
   opposite for a row addressed to him. *Rejected:* keeping it, which would fail step 3's own
   criterion; deleting it, which would leave nothing guarding a wrong row on his side.

3. **Ruling wanted: the words `guess` and `unknown`** on a PSK31 row. They are the session's wording,
   not a ruling. *Reasoning:* §R1 needs the state visibly a guess, not by colour alone. *Rejected:*
   a word on certain rows too (`sure`), which puts noise on every clean row.

4. **Ruling wanted: should §R3 recognise a roger (`R`, `RR`, `QSL`) before step 4?** Without one the
   corpus's `ack` kind can never be produced, and an ack reads as chat. *Reasoning:* §R3 lists its
   vocabulary and says anything else is text, and the instruction held the parser to it. Step 4's
   turn indicator may want acks. *Rejected:* adding it here, which would widen a ruling the unit
   was told not to.

5. **Ruling wanted: accept the split rule's three edges for step 4's turn indicator.**
   - A bare `73` does not end a message; `SK` or `CL` does.
   - A sign whose callsign is damaged does not split, so that message merges with the next and
     stays uncertain.
   - A final `K` counts when the next character arrives.

   *Reasoning:* each keeps a split from cutting an over in two, or from inventing a message the
   text does not support. *Rejected:* splitting on `73`, which cuts 03-no-report line 4 in two;
   splitting on a damaged sign, which would name a speaker the text did not give.

6. **Ruling wanted (low): the callsign shape is now held in four places** - three private copies
   plus `Psk31ExchangeParser.CallsignPattern`, bound by a test. Consolidate into one engine type
   in a later unit, or leave them. *Rejected:* consolidating here, which changes CW and scanner
   code this unit had no business in.

### Asks still outstanding

Carried per HM-DEC-139, verbatim where unresolved.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's. PSK31's continuous carrier makes it sharper.
2. **Nothing in this repository can look at a picture.** `Avalonia.Headless.Skia` is
   Tim's to add (§0.4).
3. **Three inherited reds, never chased** - two in `TheAchievementsScreenTests`, one in
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests` (engine).
4. **`Ft8ContactCard.Closing`** is uncalled and left standing.
5. **`Ft8GlobePlot`'s unused framing constants.** Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** Raise; do not resolve.
7. **The FT4 phase's run files.** Plan and status recovered into `docs\phase-ft4-run\` by
   unit 315; the outcome file is an uncommitted placeholder waiting on item 19.
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
    expected 168242 bytes, md5 `b60010069f7feea96e9d4be4e7e8c0f1`, then commit.
20. **Hand wanted: `git rm src\Hamlet.RadioEngine\Psk31\Psk31Listening.cs`** - comment-only,
    builds.
21. **Ruling wanted: the carrier search's floor should be local to the signals before
    step 6** - at 48 kHz the median bin is below the radio's passband.
22. **Ruling wanted: is HM-DEC-161 the right id** for the phase-setting ruling?
23. **Ruling wanted: accept about a second of delay on PSK31 text, or reopen the step-1
    squelch.**
24. **Ruling wanted: the `snr` column's hover names FT8 and FT4 precision and not PSK31's.**
