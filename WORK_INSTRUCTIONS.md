# Work instruction 378 - the canned list on a right-click, and a hover that says what the row knows instead of what it says

**Step 7 of the hardening phase, and the whole step: R39's ruled C.** Five tasks. **Task 1
builds nothing**: what a right-click does today on each kind of PSK31 row, which facts the
row already holds, whether the text a hover repeats is reachable by a click, and how many
seconds each of the seven framed lines takes at PSK31 and at every Olivia variant against
the cap are all measured before one line moves - because 7.1, 7.3 and 7.5 are each a claim
about something already on the screen, and none of them can be reported as met by a unit
that never read the before.

**Status.** `tools/status.sh`, real clock, after every commit and every task.

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

*All four were checked against the tree at authoring time and all four hold: both files are
present, neither solution exists, and the root is `C:\Source\HamLet`.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says - **two invocations, one build each**, status written immediately before
each. Never background and poll.

**This unit's own runs are filtered and foregrounded too.** Task 1's measurement is one
`[Fact]` trace in the app test project, run by name. Tasks 2 and 3 run their own named types
and the app types they turn red. That is not a suite and it is not a poll.

**This unit is almost all app work, and the app invocation is the flaky one.** Unit 377 lost
two of its six app invocations to Avalonia's headless `InvalidProgramException: You have
caused dispatcher loop`, both on `Unit376TheTopBandTests`, both clean on one re-run; the
engine invocation has not failed since unit 375. **Expect it, re-run once, record both
attempts, do not chase it** - it is unit 375's item 3.

**A loop goes in a script file** (`;` is refused in a compound command).

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; `-m` more than once for a multi-line commit. **Write this repository's files as
UTF-8; a PowerShell `>` redirect writes UTF-16 and the launcher cannot read it.**

- **`git worktree add` is refused.** No task here asks for one.
- **Shell output redirection (`>`) is refused to every path**, including the scratchpad.
  Write files with the editor. **A test run's output is read from the console**, or piped to
  `grep` in the same command.
- **A compound command with `;` or a second operation is refused.** **Python runs here.**
- **An apostrophe inside a quoted argument breaks the arbiter's own tooling.** Watch it in
  commit messages.
- **A new JSON data file needs a home in a project file to reach the running application.**
  `data\psk31\canned.json` is new in this unit and nothing in `src\Hamlet.App\` copies or
  embeds a `data\` file today except `data\bylines.json`, which is an `EmbeddedResource`
  (`Hamlet.App.csproj` line 40). **Section 6 ruling 1 says which way this one goes, and it
  is not that way.** Read the csproj before you write the reader.

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**. Unit 377 raised five, and **three of them
are closed before this unit starts**:

- **Unit 377's item 1 - the four new Olivia variants have no row in `data\olivia\timing.json`
  and take the 30 s fallback - is ANSWERED by R43 and is NOT this unit's work.** *Step 2
  stays partial where unit 377 left it - 2.4 open, the four new variants capped at the
  30-second fallback and refused above it, which is the safe direction. That is not a stop
  and not the next unit's work.* **Do not add a timing row. Do not raise it again.** What
  this unit does with it is measure it and report it: task 1 measures each canned line
  against the cap at every variant, so Tim can see exactly which of the seven lines the
  fallback refuses and where (section 8, task 1, item 5).
- **Unit 377's item 2 - one commit in that unit's history at which Hamlet would transmit an
  unproved variant, and which of two instructions governs - is ANSWERED in section 6 ruling
  1's last clause.** It comes off the queue.
- **Unit 377's item 3 - five named readers of the retired RSID path where there were six -
  is closed.** It was a finding about that instruction's own count, the unit moved all six,
  and nothing is outstanding.
- **Unit 377's item 4 stays on the queue** - `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessed
  YourTurn` is red, inherited, measured against the old embedded file and red there too, not
  on the carry-forward list. **A finding. This unit does not repair it and does not chase
  it.**
- **Unit 377's item 5 is the `RULES_AT` id-scheme split**, already carried. Report it again,
  do not repair it.

**The carried queue is nineteen:** unit 377's item 4; unit 376's items 3, 4 and 5; unit 375's
items 3 and 4; unit 374's item 3; unit 373's item 2; unit 372's items 4 and 7; unit 371's
five; unit 369's four. **This unit answers none of them.**

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase, screen, record and test
            work banked in the PSK31 and Olivia threads that needs neither the radio
            nor the owner, judged by tests that ran and, at the end, by Tim at his
            window.
UNIT GOAL:  A right-click on a PSK31 or Olivia row offers seven lines Tim can read,
            kept in a file he can edit without a session; one click sends the chosen
            line through the one unslotted send path, announced, capped and recorded
            with no text in the record; and the hover on that row stops repeating
            the words already on the screen and says what the row knows instead -
            with the mode chip and the send line naming the mode he chose rather
            than the family underneath it.
ADVANCES:   step 7, criteria 7.1, 7.2, 7.3, 7.4 and 7.5 - the whole step, which has
            zero units spent and whose entry R42 opened and R43 pointed at.
DRIFT:      none.
```

**The count today.** Step 0 `partial`, 2 units. **Step 1 `done`**, 6 units. Step 2 `partial`,
4 units - 2.1, 2.2 and 2.3 met, **2.4 the only thing holding it open, and R43 defers it**.
Steps 3, 4 and 5 `not started`, 0 units, and step 3 cannot start until step 8 does. **Step 6
`done`**, 2 units, closed by R42. **Step 7 `not started`, 0 units. Step 8 `not started`, 0
units.**

**Step 7's entry, checked:** *step 6 done (R43: step 2 may be partial).* Step 6 is done and
recorded so in both status files by unit 377's task 0. **The entry is open.** Step 7 depends
on step 6 only (`PHASE_PLAN.md` section 4, revision record of 2026-09-21 evening).

**Why step 7 and not 2.4, and not step 8.** **R43 is a week old at the hour you read this
and it names the order in words:** *the arbiter goes to step 7, then step 8, before anything
else.* 2.4 is deferred by the same ruling. Step 8's own entry is *step 7 done*, so it is not
available until this unit closes. **There is one step available and this is it.**

**What this unit is worth, in the owner's terms.** Tonight a right-click on a station calling
CQ offers Tim exactly one thing, and only where the parser is certain it was a CQ; on every
other PSK31 row it offers nothing at all, and the hover over the words repeats the words.
**After this unit, a right-click on any row that names a station offers him the seven things
an operator actually says, in his own words, from a file he can open in an editor and add to
- and the hover tells him who the station is, where he is, how loud, when he started, whether
he has spoken to Tim, and what a click will do, instead of reading him back the line he is
already looking at.**

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time. **Report every mismatch in section 4 and
section 1; repair nothing but this unit's.**

**The right-click today, and it is one item on one kind of row.**
`src\Hamlet.App\Views\MainWindow.axaml.cs`:

- Line **309**, `OnDecodedRowContextRequested` - the handler on `DecodedRowGrid`
  (`MainWindow.axaml` line **4947**, `ContextRequested="OnDecodedRowContextRequested"`).
  **It opens the conversation card first** (`vm.OpenPsk31CardCommand.Execute(row)`, line 327,
  R29) **and then builds the menu.** That order is Tim's and does not change.
- Line **364**, `SendFlyoutFor` - **the PSK31 branch is lines 370-382**: where
  `vm.Psk31AnswerLabelFor(row)` is not null it builds **a one-item `MenuFlyout`** carrying
  `vm.AnswerPsk31Command` with the row as parameter, and returns. Otherwise it falls to
  `vm.SendMenuFor(row)`, **which answers null for every PSK31 row** - the FT8 options are
  77-bit message shapes.
- **So a PSK31 row that is not a certain CQ has no menu at all today**, and the right-click
  on it opens a card and nothing else.

**Which rows name a station, and there is already one method for it.**
`src\Hamlet.App\ViewModels\MainWindowViewModel.cs`:

- Line **17022**, `Psk31StationOn(row)` - `IsTextOnly` **and** (`IsPsk31Chosen` or
  `IsOliviaChosen`) **and** the row's `Sender` is not blank **and** the sender is not the
  operator himself. **This is the predicate the canned menu uses** (section 6 ruling 1).
- Line **17109**, `Psk31CqOn(row)` - the narrower one: a certain `Psk31LineKind.Cq`. **It
  keeps its job** and is what the answer row is offered on.
- Line **17118**, `Psk31AnswerLabelFor(row)` - *Answer W1AW*, or null.

**The one send door, and everything 7.2 asks for is already inside it.** Line **15990**,
`SendUnslotted(string wanted)`, reached only from `SendMessage`:

- Line **15990-16010**: `kind` from `_psk31Macro`, `typed` from `_psk31Typed`, `moving` from
  `_oliviaMove`, and **the macro token is chosen at line 16006**:
  `typed ? "typed" : moving is not null ? "qsy" : Psk31MacroToken.For(kind)`. **That line is
  the shape the `canned` token takes** - a flag set beside the call and read here.
- Line **16108**: the cap handed to the composer is
  `typed ? LongestTypedSeconds : OperatorSend.LongestUnslottedSeconds` - **60 s for a typed
  line, 30 s for everything else** (`Ft8TransmitSequence.cs` line 133;
  `MainWindowViewModel.cs` line 16909).
- Line **16129**: `Psk31Events.SendComposed(...)`. **`macro` is a `string kind`**
  (`Psk31Events.cs` line **761**), **so `canned` needs no new enum member and no engine
  change**; `announced` and `rsidCode` are written there from the composed result, and
  `withinCap` is computed there.
- **The RSID burst is not optional and is not this unit's** (R27). Every unslotted send
  already begins with it.

**The typed line, which is the door the canned line walks through.** Line **16928**,
`SendTypedPsk31(card)`:

- `Psk31Macros.Sendable(card.TypedText)` -> `Psk31Macros.Typed(station, mine, clean)` ->
  `AppEvents.OperatorAction(_telemetry, "psk31_typed_pressed", ...)` -> `_psk31Macro = None`,
  `_psk31Typed = true`, `_psk31SendAtHz = Psk31OffsetOf(station)`,
  `_oliviaSendVariant = OliviaVariantOf(station)` -> `SendMessage(framed)` -> the refusal
  check on `_armedText` -> `RefreshPsk31Card(station)`.
- **`Psk31Macros.Typed` is R39's frame, already written**
  (`src\Hamlet.RadioEngine\Psk31\Psk31Macros.cs` line **136**):
  `{other} de {me}  {said}  BTU {other} de {me} K` - *the callsigns in front and the hand-back
  behind*, which is exactly *each framed with the callsigns and the hand-back*. **Do not write
  a second frame.**

**The three macros that are three of Tim's seven lines.** `Psk31Macros.Answer` (line **69**)
is `{other} de {me} {me} K`; `Report` (line **85**) carries RST, name, QTH and grid from
Settings; `Confirm` (line **112**) is `R R  TNX for the QSO  73 73 ... SK`. **`Report` and
`Confirm` cannot be composed where Settings has no name, no QTH or no grid**, and
`Psk31Offer` is what says whether they are offered.

**The hover today, and it repeats the row.** `MainWindow.axaml`:

- Line **4947**: `ToolTip.Tip="{Binding WorkedTip}"` on `DecodedRowGrid` -
  `DigitalDecodeRow.cs` line **666**, which is the *worked before* sentence or null. **This is
  the row-wide hover and it is where the facts go.**
- Lines **5201-5208**: an inline `ToolTip.Tip` on the `DecodedRowWholeMessage` button carrying
  `TextBlock x:Name="DecodedRowWholeMessageTip"` bound to **`{Binding WholeMessage}`** - **the
  hover that repeats the text R39 says must stop repeating it.**
- **The same `WholeMessage` is what a click opens** (`OpenTheWholeMessageCommand`,
  `WholeMessageIsOpen`, `DigitalDecodeRow.cs` lines 793-861). **Task 1 proves that before task
  3 changes anything** - if the words are reachable by a click, taking them off the hover hides
  detail; if they are not, taking them off hides information, and CLAUDE.md 0.5 forbids it.

**What the row already knows, against R39's list.** `DigitalDecodeRow`: `Sender` (735),
`Addressee` (735-748), `Hz` and `Snr` on the record (151), `Variant` (437), `Ended` (411),
`Reading` - a `Psk31Exchange` with `Kind`, `Speaker` and `IsCertain` - `ReadingWord` (169),
`RepeatCount` (238), `WorkedBefore` (343), `Contact` (151), `SlotStartUtc` (151).
**Country, grid, distance and the start and stop times are NOT on the row** at authoring
time; where they live - the conversation card, the ledger, `_psk31Readings` - is task 1's to
find and report. **A fact Hamlet does not have is absent from the hover** (7.3), and absent
means the line is not drawn at all, not drawn empty and not drawn as *unknown*.

**The mode chip and the send line, which are 7.5.**

- `DigitalModeChips` (line **1231**) is `DigitalModeChip.For(neighborhood, ChosenDigitalMode)`,
  and `ChosenDigitalMode` is the canonical strip label unit 374 made honest.
  **`DigitalModeChip.IsChosen` is already compared case-insensitively against the five labels**
  (`DigitalModeChip.cs` lines 65-107). **So 7.5's first half may already hold. Measure it
  before you change anything, on FT8, PSK31 and Olivia, and if it holds, the work is the
  assertion and not a repair** - and say so plainly.
- **The send line's second half does NOT hold.** Line **17959**:
  `return "Sent \"" + wanted + "\" - " + Psk31Events.Say(...) + " s of PSK31.";` - **the mode
  is a literal.** Under Olivia, Tim's screen on 2026-09-21 read *29 s of PSK31* for an Olivia
  CQ. **That is the sentence 7.5 names.** Read the whole of `WentLine` (lines ~17925-17960)
  and `StopLine` before you change it: several sentences in that region name the mode or the
  macro, and the criterion is about the mode that was chosen, never the family.

**What the record already does with the sub-mode** (unit 374, criterion 2.1):
`AppEvents.OperatorAction(_telemetry, "psk31_typed_pressed", OperatingMode, PressedSubMode)`
- `PressedSubMode` is `ChosenDigitalMode` or `StartupSnapshot.Unknown`. **A canned press
  writes an operator action the same way and with the same sub-mode.**

**What failures are expected, and what they mean.**

- **`ThePsk31ReadsTheConversationTests` WILL GO RED AT TASK 2, AND IT IS ON THE CARRY-FORWARD
  LIST.** Lines **455-473** assert, for every PSK31 row on the panel, that
  `MainWindow.SendFlyoutFor(model, row)` is **null** where `Psk31CqOn(row)` is null and a
  **single** `MenuItem` bound to `AnswerPsk31Command` where it is not. **Both are the
  one-item menu this criterion replaces. It is this unit's under R12, in its own commit, and
  the rewrite asserts more:** the seven rows in the file's order, each item's command and
  parameter, that a row naming a station has a menu where it had none, that no FT8 option
  ever appears on a PSK31 row, and that nothing is greyed or disabled. **Nothing in it is
  loosened and `Assert.Null(model.SendMenuFor(row))` stays exactly as it is.**
- **`TheWholeChainRunsFromOneRightClickTests`** mentions `SendFlyoutFor` at line 891 in a
  comment and drives the chain from the handler. **Read it before task 2 and say in the
  report whether it went red, why, and what the rewrite asserts.**
- **A hover change can turn a view test red.** `ViewTestsActThroughControlsTests`,
  `TheWholeMessageTests` and `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` are
  the three to read first; **a new binding with a typo is a `BindingHealthTests` red and it
  is a real fault, not a rewrite.**
- **What is NOT expected and is NOT a rewrite:** `ThePsk31CqGoesOutTests`,
  `TheSendReachesTheAirTests`, `TheOliviaSendTests.AnOliviaCqReachesTheAir`,
  `TheTypedLineGoesOutTests`, `ThePsk31SendIsAnnouncedTests`, `ThePsk31TransmitTelemetryTests`
  or anything in the engine going red. **Those guard the one send door. A red there means the
  canned route changed the send path**, which is a regression under HM-DEC-165 and is reported
  as one and repaired, never rewritten.

**The counts to expect.** Unit 377's exit run: **app 212 of 212, engine 150 of 150**. The app
invocation takes about 2 m 24 s; the engine about 4 m 56 s against a 480 s timeout.

**The version.** `Directory.Build.props` line **1017**: `<Version>1.13.64</Version>`.

**The record's own state, and the first is yours to repair at task 0.**

- **Yours.** `PHASE_STATUS.md` reads `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 377 - ...`, both
  stale - the rev4 commit left them behind. **Set them to `7` and to this instruction.**
- **Not yours.** `PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-165 (2026-09-19)` while
  `CLAUDE.md` section 1 holds `CPS-DEC-0165` - the id-scheme split, carried in
  `PHASE_PLAN.md` section 7 and reported unrepaired by units 376 and 377. **Report it again,
  do not repair it.**
- At authoring time HEAD was **657b119d** on `main`. **Two uncommitted at the root, neither
  under `src\`:** `.run-unit\reload.txt` and `PHASE_STATUS.md` carrying its `HEARTBEAT:` line.
- **`PHASE_OUTCOME.md`'s header says step 6 `done` and its entries agree; the reload's
  disagreement on step 6 was resolved by unit 377's task 0 and is not outstanding.**

---

## 6. Rulings in force

Transcribed in full. **Do not re-argue any of these.**

### The owner's ruling that licenses this unit, in full

**R39 - Tim, 2026-09-21: the UI comes first.** *Steps 6, 7 and 8 are worked before steps 3
and 4; step 3 depends on step 8. ... Ruled C on the canned list: seven lines - Answer him ·
Send my report · Confirm and 73 · Say again? · Please repeat your report · QRZ? · 73 and out
- each framed with the callsigns and the hand-back, one click sends, read from
`data/psk31/canned.json` so Tim adds his own without a session. The hover on a PSK31 row says
what the row knows - station, country, grid and distance if sent, offset and strength, when
he started and stopped, whether he spoke to Tim, the parser's kind and certainty, what a
click and a right-click do - never the text again.*

**R43 - Tim, 2026-09-21: the UI first; the timing question is deferred.** *Step 2 stays
partial where unit 377 left it - 2.4 open, the four new variants capped at the 30-second
fallback and refused above it, which is the safe direction. That is not a stop and not the
next unit's work. The arbiter goes to step 7, then step 8, before anything else.*

**And the stop is one inch away, as it always is.** `PHASE_PLAN.md` section 6: *anything that
would change what goes on the air, or what keys - `MOVE: stop`.* **R39 licenses new text
going out through the send path that already exists, framed by the frame that already exists,
announced by the burst that already exists and held to the cap that already exists. It
licenses nothing else.** If you find that reaching the criterion needs a second composer, a
second arming site, a second `PttOn`, a path that skips the cap, or a send that does not begin
with its RSID - **stop, record it, hand it back.** A canned line is text. It is not a new way
to transmit.

### The first is mine: the file, the menu, the send, the token, and the order

1. **The file is `data\psk31\canned.json`, it is the shipped default, and Hamlet never writes
   it.** The path is R39's own and criterion 7.1 checks it. It is added to
   `src\Hamlet.App\Hamlet.App.csproj` as **`Content` with `CopyToOutputDirectory`**, not as an
   `EmbeddedResource`, **because a file the operator is meant to edit must exist as a file
   beside the application** and `bylines.json`'s embedding is the pattern for a file nobody
   edits, not for this one.
2. **The operator's copy wins where he has made one.** The reader looks for
   `%AppData%\Hamlet\canned.json` first - the folder `AppSettings.DataFolder` already names as
   *the one folder Hamlet writes to*, beside `settings.json` and `scan-segments.json` - and
   falls back to the shipped file. **Hamlet does not create it, does not copy it there and
   does not repair it.** That is *without a session* satisfied with no new write and no new
   risk: he copies one file and edits it. **The shipped file's own header says so in words.**
3. **The format is documented at the top of the file itself** (7.1), in a leading `_note` key
   exactly as `assets\data\rsid-codes.json` carries one - JSON has no comments and a README
   nobody opens is not documentation. The note says what a row is, where to put his own copy,
   and that a line goes out framed with both callsigns and the hand-back.
4. **A malformed file is reported and never guessed** (7.1, section 0.0). No partial read, no
   skipped row, no silent default to the shipped seven: **the menu shows one note item saying
   the file could not be read and what is wrong with it**, in the manner `SendFlyoutFor`
   already adds a `Note` for an absent message, **and no canned item at all.** A note carries
   no command and cannot be clicked. **No new telemetry event type** (R13); if the tree
   already has an event for a data file that would not load, use it, and if it does not, the
   sentence on the menu is the whole of it and the report says so.
5. **A row in the file is a label and a line - or a label and the name of a macro Hamlet
   already has.** Three of Tim's seven lines *are* Hamlet's three existing macros: *Answer
   him* is `Psk31Macros.Answer`, *Send my report* is `Report`, *Confirm and 73* is `Confirm`.
   **Those three rows name their macro and are sent by the command that sends it today** -
   `AnswerPsk31Command` and the card's own offers - **unchanged, with their own record tokens
   and R1's certainty gate intact.** The other four carry text. **This is why: writing a
   second answer beside the one Hamlet already sends is two spellings of one act, and unit
   377's whole subject was two files that could disagree.** A macro row Hamlet cannot compose
   right now - no grid, no name, no certainty - **appears as a note saying why, never as an
   item that fails when clicked** (section 0.0: absent, not greyed).
6. **A text row is sent through the door the typed line uses**, at
   `Psk31Macros.Typed(station, mine, line)`, through `SendMessage` and `SendUnslotted`, with
   the offset from `Psk31OffsetOf(station)` and the variant from `OliviaVariantOf(station)`.
   **There is no second composer, no second `Arm`, no second `PttOn`.**
7. **The token is `canned` and it is a flag beside the call**, set like `_psk31Typed` and read
   at line 16006, so `psk31_send_composed` carries `macro: canned`, `announced`, `rsidCode`
   and `withinCap` and **no text and no callsign** (HM-DEC-018 section 2.1). **A canned line
   takes the macro cap of 30 s, not the typed line's 60** - it is the safe direction, and a
   canned line that will not fit is refused by the cap that already refuses, in the sentence
   that already says so. **The press writes `psk31_canned_pressed` through
   `AppEvents.OperatorAction` with `PressedSubMode`**, the way the other three presses do -
   that is an operator action in the existing writer, not a new event type.
8. **Which rows offer the menu: every row `Psk31StationOn` names a station on**, CQ or not,
   live or ended, faded or not. **Not `Psk31CqOn`** - that one keeps its job as the answer
   row's condition. **Nothing is greyed, hidden or disabled** (the 2026-09-06 ruling the
   handler already carries), and the existing `Log this contact...` item and its separator
   stay exactly where they are.
9. **The order of commits, which also answers unit 377's item 2.** *Where landing a capability
   and landing the gate on it would otherwise leave one commit in which Hamlet can do a thing
   it may not do, the two go in ONE commit. Never the capability first.* In this unit that
   means **the reader, the file and the menu land together with the cap, the frame and the
   announcement already on the path they walk** - there is no commit here at which a canned
   line can reach the air unframed, unannounced or uncapped, and the report says so.

*Author's, overrulable.* A path, a file's shape, a mechanism, a token, a refusal's wording and
the order of commits are the arbiter's by `PHASE_PLAN.md` section 6 and never a stop.

**And the honest word about 7.2.** The criterion reads *every canned send ... writes
`psk31_send_composed` with `macro: canned`*. Under item 5 above, the three macro rows write
`answer`, `report` and `confirm`, because that is what they are. **Every one of the seven
still begins with its RSID, carries `announced`, counts against the cap and carries no text.**
**Report 7.2 by id with exactly that distinction spelled out, and do not round it up to
met without saying it.** If you judge the criterion's words to require `canned` on all seven,
say so, say what you did, and let the record carry the disagreement - **do not relabel an
answer as canned to make a checkbox tick.**

### The second is mine: the hover is facts, and the two namings of 7.5

1. **The hover is built in the view model, not in the markup**, as one ordered list of fact
   lines on `DigitalDecodeRow`, so the hover a test reads and the hover Tim sees are one
   string. R39's order is the order: **station, country, grid and distance if sent, offset and
   strength, when he started and when he stopped, whether he spoke to Tim, the parser's kind
   and its certainty, and last what a click and a right-click do.**
2. **A fact Hamlet does not have is ABSENT** (7.3, section 0.0). The line is not drawn. Not
   *unknown*, not a dash, not an empty label. **A fact Hamlet has and this unit does not reach
   is a shortfall, not an absence**, and it is named in the report by name.
3. **The text goes off the hover only where a click still opens it.** Task 1 proves that
   `DecodedRowWholeMessage`'s click opens the same `WholeMessage` the tip draws. **If it does,
   the tip becomes the facts and the words stay one click away** - hiding detail, which
   section 0.5 allows. **If it does not, the words STAY, 7.3 is reported partial with the
   reason, and nothing is hidden** - hiding information, which section 0.5 forbids. **The
   measurement decides this, not the criterion.**
4. **The hover never carries the text, in any form** - not the message, not the whole message,
   not the payload, not a truncation of it. That is the whole of what R39 asked for.
5. **7.5 is two namings and one of them may already be true.** The chip: measure
   `DigitalModeChips` under Olivia and under PSK31 before touching anything; **if the Olivia
   chip is already the only chosen one, the work is one assertion and the report says the
   criterion was already met in the tree** - a criterion met by measurement is met (R14: do
   not build what is there). The send line: `WentLine` at line 17959 says `s of PSK31` under
   Olivia and **that is a sentence stating something untrue about a send** (section 0.0), so it
   names `ChosenDigitalMode` - the same canonical label unit 374 put in the record - and every
   sibling sentence in that region is read and reported, repaired only where it names the mode
   wrongly. **Nothing about what is composed, armed or keyed changes in either.**

*Author's, overrulable.* What a hover says, what a sentence says and which of two true facts a
label names are the arbiter's by `PHASE_PLAN.md` section 6.

**That is two rulings, which is R31's limit.** The nineteen carried asks get none.

### The phase's standing rulings

**R27 - every PSK31 and Olivia send begins with its RSID burst**, and answering a station at
his variant is the point of the mode. **The burst is not optional and this unit does not make
it optional**; a canned send inherits it from the one door.

**R11 - nothing at the radio.** No sentence this unit writes - not a menu item, not a note,
not a hover line - asks the operator to set a level, read a meter or know what ALC is.

**R12 - a session fixes its own tests and never asks the owner to approve it.** A test this
unit turns red is this unit's to rewrite **in its own commit**, asserting more than it
replaced and never less. **`ThePsk31ReadsTheConversationTests` is the one to expect** and it
is on the carry-forward list, so the rewrite is read twice before it is written.

**R13 - telemetry is a must-pass on every remaining step.** **This unit adds no stage and no
event type**: one macro token on an existing composed event, one operator action through the
existing writer. If you find yourself writing a new event type, you have left the criterion.

**R14 - eyes on the prize.** *"We don't focus too much on pointless testing."* The tests these
five criteria need and no others. **A criterion the tree already meets is asserted, not
rebuilt.**

**R19 - American spelling** in every operator-facing string, every canned line, every hover
line and every instruction.

**R31 - this phase runs unattended.** Criteria by id. A done step is closed. The owner's step
ends the run. **Two rulings per unit at most.**

**R43 - the timing question is deferred.** The four new Olivia variants keep the 30-second
fallback. **Measure it, report it, do not repair it.**

**HM-DEC-018 section 2.1** nothing personal in an event - the record carries the macro token,
the character count and the seconds, **never the line and never a callsign**. **Section 0.0** a
sentence on the screen is a claim, and a refusal says the true reason. **Section 0.5** hiding
detail is allowed, hiding information is not. **Section 0.6** a mark is words and not only
weight. **HM-DEC-155**, **HM-DEC-139** (the carried queue verbatim), **HM-DEC-165** (nothing
green before a unit is red after it), **FACT-004** (nothing in this unit is evidence about the
radio).

## 7. Status cadence

`tools/status.sh`, real clock, **after every task and every commit**, and **immediately
before each carry-forward invocation** - task 0's two and task 4's two. **Task 1's
measurement is one task**: status before it and after it, not between runs. Never compose a
timestamp.

---

## 8. The tasks

### Task 0 - the record and the entry run

Append `UNIT 378 - STEP 7` to `PHASE_OUTCOME.md` with `ADVANCED: step 7`. Patch-bump
**1.13.64 -> 1.13.65** in `Directory.Build.props` with its line in the version log. **Set
`CURRENT_STEP: 7` and `WORK_INSTRUCTION: 378 - ...` in `PHASE_STATUS.md`**, both stale at
`0` and `377` (section 5).

**Run the carry-forward list, both invocations, before anything changes**, status written
immediately before each, and put the two counts in the outcome entry's `ENTRY:` line. Unit
377 left it at **app 212 of 212, engine 150 of 150**. A red here is not yours; name it and go
on. **If either invocation dies of `InvalidProgramException: You have caused dispatcher loop`
before any assertion, that is unit 375's item 3 - re-run it once and record both attempts.**

**Drop candidate:** none.

### Task 1 - the trace: what the right-click does, what the row knows, and what fits

**This task builds nothing, changes no source file and repairs nothing.** One `[Fact]` trace
named for this unit in `tests\Hamlet.App.Tests\ViewModels\`, run by name, driving a real view
model over a PSK31 fixture and an Olivia fixture. Print:

1. **The menu today, row kind by row kind.** For every PSK31 row on the panel and every Olivia
   row: what `Psk31StationOn`, `Psk31CqOn` and `Psk31AnswerLabelFor` answer, and how many
   items `MainWindow.SendFlyoutFor` returns - **null, one, or more** - with each item's header
   and command. **Count the rows that name a station and have no menu at all. That number is
   7.1's before.**
2. **The hover today.** For the same rows: `WorkedTip`, `WholeMessage`, `HasWholeMessage`, and
   **whether `OpenTheWholeMessageCommand` opens the same string the tip draws** - drive the
   command and read `WholeMessageIsOpen` and the text. **This is the measurement section 6
   ruling 2 item 3 turns on. Print it plainly; the answer decides what task 3 may do.**
3. **What facts exist and where.** For each of R39's ten facts - station, country, grid,
   distance, offset, strength, started, stopped, spoke to Tim, parser kind and certainty -
   **name the property or the object that holds it today, or print `NOT IN THE TREE`.** Look
   on the row, on the `Psk31Exchange`, on the conversation card, on the contact ledger and in
   `_psk31Readings`. **A fact Hamlet genuinely lacks is 7.3's *absent*; a fact that exists
   somewhere is 7.3's work.**
4. **7.5's before, measured and not assumed.** `DigitalModeChips` under FT8, PSK31 and Olivia
   - which chip is `IsChosen`, which `IsLit`, which `IsPlain` - **and the send line after a CQ
   under Olivia, verbatim**, together with every sentence in `WentLine` and `StopLine` that
   names a mode or a macro. **Print the *29 s of PSK31* line if it reproduces; say so if it
   does not.**
5. **What each of the seven lines costs, against the cap.** Frame each of Tim's seven -
   the three macros as their own composers build them, the four text lines as
   `Psk31Macros.Typed` frames them - for one real callsign pair, and print **characters,
   seconds at PSK31, seconds at each of the seven Olivia variants, the cap that applies, and
   whether it fits.** Use the arithmetic already in `Psk31Modulator.SecondsFor` and the Olivia
   format table; nothing is composed to the sound card and nothing is keyed. **This is R43's
   deferred question made visible without touching it: the report says which of the seven
   lines the 30-second fallback refuses at which variant, and that is a fact Tim asked for in
   substance when he deferred the rows.**
6. **The tests that stand on the one-item menu.** Name every assertion in
   `ThePsk31ReadsTheConversationTests` and `TheWholeChainRunsFromOneRightClickTests` that
   reads the flyout, by line, and say what each will do when the menu becomes seven. **Read
   them; change nothing.**

**What this task must answer in the report, by id.** For **7.1**: how many rows have a menu
today and how many name a station. For **7.3**: which facts exist, where, and whether the
text is reachable by a click. For **7.5**: whether the chip is already right and what the
send line actually says.

**Drop candidate: none, and if the unit runs long everything else goes before this does.** A
measured before handed to the next unit beats an unmeasured change.

### Task 2 - the file, the menu and the send, in one commit

**`data\psk31\canned.json`**, seven rows in R39's order with his labels - *Answer him*, *Send
my report*, *Confirm and 73*, *Say again?*, *Please repeat your report*, *QRZ?*, *73 and out*
- the first three naming their macro and the last four carrying text, with the `_note` header
section 6 ruling 1 item 3 describes. **The four texts are yours to write**: short, American,
inside what the varicode carries, and each one a thing an operator actually says. **Run them
through `Psk31Macros.Sendable` before you ship them** - a canned line that loses characters to
the varicode would be Hamlet shipping a line it cannot send.

**The csproj entry, the reader, the menu and the send, all in the one commit** (ruling 1 item
9): `Content` with `CopyToOutputDirectory`, the `%AppData%` override read first, the malformed
case reported as a note with no canned item, the menu on every row `Psk31StationOn` names, the
text rows through `Psk31Macros.Typed` and `SendMessage`, the macro rows through the commands
that send them today, `macro: canned` on the composed event for a text row, and
`psk31_canned_pressed` through `AppEvents.OperatorAction` with `PressedSubMode`.

**Then the R12 rewrites, each in its own commit**, `ThePsk31ReadsTheConversationTests` first.
**Assert more than it replaced and never less.**

**Say in the report, with the trace's numbers, that after this task the send path is the same
path**: one `Compose`, one `Arm`, one `PttOn`, the same cap, the same burst - and that the
only thing that is new is what the text says.

**Drop candidate: the four text lines drop to however many are ready, never the mechanism.**
If time runs short, ship the file with fewer text rows, say which of Tim's seven are in it and
which are not, and mark 7.1 partial. **The reader, the menu and the send are never the drop:
a file with seven lines and nothing that reads it is nothing.**

### Task 3 - the hover, and the two namings of 7.5

**The hover** as section 6 ruling 2 rules it: built in the view model, R39's order, every
absent fact absent, the text nowhere in it, and the last line saying what a click and a
right-click do. **The `WholeMessage` tip goes only if task 1 proved the click opens the same
words**; if it did not, leave it, report 7.3 partial and say why.

**7.4 in the same task, by construction and asserted.** `Psk31StationOn` and the offset and
variant helpers already carry both modes; **the test drives an Olivia row through the same
menu and the same hover and asserts it reads the same**, rather than a second implementation
being written for Olivia. **If anything has to be written twice for Olivia, that is a finding
and it goes in section 4.**

**7.5:** the chip asserted on FT8, PSK31 and Olivia; the send line naming `ChosenDigitalMode`
where it named `PSK31`, with **a 29-second Olivia CQ reading *29 s of Olivia*** asserted from
the sentence itself and not from a format string.

**`docs\carry-forward-tests.txt`:** add the canned-send name and the hover name **by type and
method with their paragraphs, and change the human-readable list underneath to match** - a
mismatch there has been a finding in four of the last five units. **Add nothing that is
knowingly red.**

**Drop candidate: the hover facts that live outside the row - country, grid and distance.**
If reaching them means walking the ledger or the card for every row on a busy panel, **ship
the hover with the facts the row and its reading already hold**, mark 7.3 partial, and name
in the report exactly which facts were left, where they live and what it would cost. **The
absent-not-unknown rule is never the drop, the text coming off is never the drop, and 7.4 is
never the drop.**

### Task 4 - the exit run, the record and the report

- **Run the carry-forward list, both invocations, after the last change.** Compare name by
  name against task 0. **A red after that was green before is a regression** and sections 1
  and 4 both name it as one (HM-DEC-165). **The send guards -
  `TheSendReachesTheAirTests`, `ThePsk31CqGoesOutTests`, `TheOliviaSendTests.AnOliviaCq
  ReachesTheAir`, `TheTypedLineGoesOutTests` - are the ones to read first.**
- **`PHASE_STATUS.md` and `PHASE_OUTCOME.md`:** step 7 criterion by criterion by id (R31).
  **7.1 is met only if the seven come from the file, one click sends, the format is documented
  in the file and a malformed file is reported rather than guessed. 7.2 is met with the
  distinction section 6 spells out, stated and not rounded. 7.3 is met only if every fact
  Hamlet has is on the hover and the text is not.** Any one short and it is `partial` with
  what was reached. **Do not round up.** **Step 7 is `done` only if all five are met**, and if
  it is, say so plainly, because step 8's entry is exactly that.
- Write `output.md` per section 12.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **Criterion 2.4 and the five green rounds, and `data\olivia\timing.json`.** **R43 defers
  both.** Run the carry-forward list twice, at task 0 and task 4, and no more. **Do not write
  a timing row for any variant.** Measuring what the cap refuses (task 1 item 5) is reporting,
  not repairing.
- **Step 8 - the achievements.** It is the next unit's and its entry is this unit's close.
  **Do not build toward it.** The quill on a PSK31 row is 8.3 and not 7.3; the hover says what
  a click does and nothing about what a contact would earn.
- **Steps 3, 4 and 5.** The visibility events, the radio sheet, Tim's verdict. **In
  particular, do NOT add a visibility event for a row or a card** - that is 3.1 and 3.2 and it
  belongs to a step that has not started.
- **Step 6 and the top row.** Closed by R42. **No `.axaml` change above the working panels.**
- **The one send door.** `SendUnslotted`, `SendMessage`, `Arm`, `PttOn`, `StopNow`, the abort
  path, the teardown pair (R38 (b), closed), the drive level and every sample of composed
  audio. **A canned line walks through that door and does not touch it.**
- **The decoder, the demodulator, the modulator, the detector and the exchange parser.** Not
  this unit's, not even to make a row name a station it does not name today.
- **`Psk31Macros.Answer`, `Report` and `Confirm`, and `Psk31Offer`.** The three macro rows call
  them; nothing changes inside them and R1's certainty gate is not loosened.
- **`PHASE_PLAN.md`'s criterion checkboxes** (unit 372's item 7). Do not tick them; do not
  raise it again.
- **`tools\run-carry-forward.sh`** (unit 374's item 3). **Run the two command lines from
  `docs\carry-forward-tests.txt` itself.**
- **The headless dispatcher loop** (unit 375's item 3),
  `TheStopIsAlwaysOnScreenTests.WithNothingKeyedItSaysStopAndIsStillPressable` (item 4) and
  **`TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`** (unit 377's item 4, inherited
  red, not on the list). **Record if they show; do not chase.**
- **`ApplyBestBet`'s stale `BestBetLabel`** (unit 373's item 2), **`LearnedAlcReference.Ago()`**
  (unit 369's item 3), **the archived Olivia phase** at `docs\phase-olivia-run\` - a phase is
  never reopened - and **the `RULES_AT` id-scheme split.** Carry them; do not repair them.
- **Any package.** A package is `MOVE: stop`.

## 10. What not to do

- **No unfiltered `dotnet test`** (HM-DEC-155). Every invocation filtered, foregrounded, one
  build. **Never background and poll. Never compose a timestamp.**
- **Do not write a second send path.** No second composer, no second `Arm`, no second `PttOn`,
  no path that reaches the air without the frame, the burst and the cap. **If the criterion
  seems to need one, it does not - stop and report.**
- **Do not put the line, a callsign or a grid in the record** (HM-DEC-018 section 2.1). The
  token, the characters and the seconds, and nothing else.
- **Do not put the text on the hover** in any form (R39, 7.3), and **do not take the text off
  the screen** - the row draws it and a click opens it.
- **Do not guess at a malformed file.** No partial read, no skipped row, no silent fallback to
  the shipped seven when his own file is broken. **Say what is wrong and offer nothing.**
- **Do not grey, disable, hide or sort away a menu item** (the 2026-09-06 ruling). What Hamlet
  cannot do right now is absent with a note saying why.
- **Do not loosen a test to make a criterion pass** (`PHASE_PLAN.md` section 6). Rewrite it
  under R12 to assert the new rule, asserting more and never less, or leave it red and report.
- **Do not delete any file.** Empty it, comment it, list it.
- **Do not add an event type or a stage** (R13).
- **Do not relabel a macro send as `canned`** to make 7.2 read met. Report the distinction.
- **Do not open a port, enumerate a device, or key anything.** Every sample in this unit lives
  in an array (FACT-004), and the report says so.
- **Report mismatches; repair nothing but this unit's. Write American. Write files as UTF-8.**

## 11. Committing and pushing

**One commit per task**, with two exceptions, both named in section 6 ruling 1 item 9 and
section 8: **task 2's file, csproj entry, reader, menu and send are ONE commit** - never the
capability before the frame and the cap it rides - **and each R12 rewrite is its own commit
after it.** A carry-forward line edit goes in the same commit as the test it names. Push once,
at the end, after task 4's runs are green or their reds are named.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured - a line that is the same every unit is furniture.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial, step 1 done, step 2
   partial on 2.4 alone and deferred by R43, step 6 done by R42, steps 3, 4 and 5
   not started with step 3 waiting on step 8, step 8 not started and waiting on
   this one. This is the first unit ever spent on step 7.
B. Step 7 - the canned list and the hover. 7.1 <met|partial|not>: <n> of seven
   lines from <path>, a menu on <n> rows that had none, malformed <reported|not>.
   7.2 <met|partial|not>: <n> text lines write macro canned, <n> macro lines write
   their own token, all <n> announced and capped, no text in the record. 7.3
   <met|partial|not>: <n> of R39's ten facts on the hover, <n> absent because
   Hamlet lacks them, text <off|still on> and reachable by <a click|nothing>. 7.4
   <met|partial|not>. 7.5 <met|partial|not>: the chip <was already right|was
   repaired>, the send line reads <verbatim>.
C. The report last. Section 4 raises <N> items on top of the carried nineteen, and
   <none of them is | item <k> is> in the way of a criterion in B. Unit 377's items
   1, 2 and 3 came off the queue - R43 deferred the timing rows, section 6 ruling 1
   answered the commit order, and the reader count was a finding with nothing left
   in it.
```

```
UNIT:       378 - <complete|stopped> at task N of 5 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 7, criteria <the ids you actually moved>
NUMBER:     PSK31 and Olivia rows that offer something on a right-click:
            <before> of <n> -> <after> of <n>, and lines on the menu <1> -> <7>
DRIFT:      none
```

**Section 3 must lead with task 1's two tables** - **the menu table** (every row kind, what
`Psk31StationOn` and `Psk31CqOn` answered, how many items the flyout had, before and after)
and **the seven-lines table** (each line, its characters, its seconds at PSK31 and at each of
the seven Olivia variants, the cap that applied and whether it fit) - **because those two are
what make 7.1 and 7.2 measurements rather than claims.** Then the facts table: each of R39's
ten, where it lives, and whether it reached the hover. Then the hover before and after,
verbatim, with the proof that the text is still one click away. Then 7.5's chip readings and
the send line before and after, verbatim. Then the carry-forward counts before and after.

**Section 2 tells Tim in one paragraph what is different**, in his terms: not that a flyout
was rebuilt, but that **a right-click on any station he is hearing now offers him the seven
things an operator says, in his own words, from a file he can open in an editor - and that
the hover tells him who he is looking at instead of reading him back the line he is already
reading.** Every claim computed, not seen (FACT-004), and say in one line that no port was
opened and nothing was keyed.

**Section 4:** your own items first, most-blocking first, each saying plainly whether it wants
a ruling or is a finding - a note is not a ruling request.

**If any of the seven lines is refused by the 30-second cap at any variant, that is a finding
and not a ruling request** - R43 already ruled it deferred and the safe direction. Name the
line, the variant, the seconds and the cap.

**If you found any way a canned line could reach the air unframed, unannounced or uncapped,
that is your first item and it wants a ruling** - it is `PHASE_PLAN.md` section 6's first
stop.

Then the carried queue verbatim per HM-DEC-139: **unit 377's item 4; unit 376's items 3, 4
and 5; unit 375's items 3 and 4; unit 374's item 3; unit 373's item 2; unit 372's items 4 and
7; unit 371's five; unit 369's four - nineteen**, with one line saying that unit 377's items
1, 2 and 3 came off it.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: right-click any PSK31 or Olivia row that names a station into seven lines read from data/psk31/canned.json, sent through the one unslotted door with the typed-line frame and the macro token canned, and replace the row hover text with the facts the row knows
MOVE: continue
WHY: R43 of 2026-09-21 names the order in words - the arbiter goes to step 7, then step 8, before anything else - and step 7 is the only step whose entry is open: step 6 closed it by R42, 2.4 is deferred by the same ruling, step 8 waits on this step and step 3 waits on step 8. Step 7 has zero units spent, the loop test finds nothing resembling this approach in any entry, and every part of it rides machinery the tree already has - one frame, one door, one burst, one cap.
STATE: not started
DECIDED: author's, overrulable, two, both transcribed in work instruction 378 section 6. (1) The file, the menu, the send, the token and the order of commits: data/psk31/canned.json is the shipped default, Content with CopyToOutputDirectory rather than an embedded resource because it is a file the operator is meant to edit, with %AppData%/Hamlet/canned.json read first where he has made one and Hamlet never writing either; the format is documented in a leading _note key as rsid-codes.json documents itself; a malformed file is one note on the menu and no canned item, never a partial read and never a silent fallback; three of Tim's seven lines are Hamlet's existing Answer, Report and Confirm macros and are sent by the commands that send them today rather than written a second time, with a macro Hamlet cannot compose appearing as a note saying why; the four text rows go through Psk31Macros.Typed and the one unslotted door with the 30-second macro cap rather than the typed line's 60, writing macro canned with no text; the menu appears on every row Psk31StationOn names a station on rather than only a certain CQ; and where landing a capability and landing its gate would leave a commit in which Hamlet can do a thing it may not do, the two go in one commit and never the capability first - which also answers unit 377's item 2. (2) The hover and the two namings of 7.5: the hover is built in the view model as one ordered list in R39's order, a fact Hamlet lacks is absent rather than unknown, the text comes off the whole-message tip only where task 1 proves a click still opens the same words and otherwise stays with 7.3 reported partial, and 7.5's chip is measured before it is touched because it may already be right while the send line's literal PSK31 under Olivia is a sentence stating something untrue and names ChosenDigitalMode instead.
LICENCE: PHASE_PLAN.md R39 - the owner's ruling of 2026-09-21, ruled C on the canned list and the hover, which is this unit's whole subject; R43 which orders step 7 next and defers 2.4 and the timing rows; R42 closing step 6 and opening step 7's entry; R27, R31 and section 6 - a path, a file's shape, a mechanism, a token, a hint, a label and the order of commits are the arbiter's and never a stop; PSK31 plan R11, R12, R13, R14, R19; CLAUDE.md 0.0, 0.2, 0.5, 0.6; HM-DEC-018 section 2.1, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
ACCOMPLISHED: Tim right-clicks a station he is hearing and gets the seven things an operator actually says - answer him, send my report, confirm and 73, say again, repeat your report, QRZ, 73 and out - and one click sends the one he picked, with both callsigns and the hand-back around it, announced and held to the same limit as everything else Hamlet sends. The seven live in a file he can open in an editor and add his own to, without waiting for a session. And the hover over a row stops reading him back the line he is already looking at and tells him instead who the station is, where he is, how loud, when he started, whether he has spoken to Tim and what a click will do.
ADVANCES: step 7, criteria 7.1, 7.2, 7.3, 7.4 and 7.5 - the whole step, which has zero units spent. It also clears step 8's entry, which reads step 7 done, and through step 8 it clears step 3's; it does not advance 2.4, which R43 defers.
END-ARBITER-DECISION
```
